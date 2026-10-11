# Publicar a Innovera na AWS (Lightsail)

Tudo o que é preciso já está no repositório (`deploy/`). Com o acesso à AWS, a publicação demora cerca de uma hora,
a maior parte à espera do DNS e do certificado.

```
Internet ──HTTPS──▶ Caddy (certificado Let's Encrypt) ──▶ aplicação (.NET + React) ──▶ PostgreSQL gerida do Lightsail
                       └── instância Lightsail (Docker) ──┘        └── volume /data: anexos e chaves de cifra
                                                           ──SMTP──▶ Amazon SES (emails)
```

## 0. O que pedir / ter à mão

| O quê | Onde |
|---|---|
| Acesso à conta AWS (utilizador IAM com Lightsail e SES), região **Paris (eu-west-3)** | dst |
| Subdomínio, ex.: `innovera.dstelecom.pt`, e quem mexe no DNS da dst | TI da dst |
| Email de quem vai ser o primeiro Administrador | equipa de Inovação |

## 1. Servidor (instância Lightsail)

1. Lightsail › **Create instance** › Linux/Unix › **Ubuntu 24.04 LTS** › plano **2 GB RAM / 2 vCPU / 60 GB** (12 USD/mês).
2. **Networking**: criar e associar um **IP estático**; na firewall IPv4 deixar **SSH (22)**, **HTTP (80)** e acrescentar
   **HTTPS (443)**. Restringir o SSH aos IPs da equipa, se possível.
3. Pedir à TI da dst um registo **A**: `innovera.dstelecom.pt → <IP estático>`.

## 2. Base de dados (PostgreSQL gerida)

1. Lightsail › **Databases** › Create › PostgreSQL (a versão mais recente disponível) › plano **1 GB** (15 USD/mês), mesma região.
2. Deixar o **modo público desligado** (só as instâncias Lightsail da região lhe chegam).
3. Criar a base de dados `innovera` (no 1.º passo do assistente, "master database name").
4. Anotar endpoint, utilizador (`dbmasteruser`) e password → vão para `LIGACAO_BD` no `.env`.
   O Lightsail guarda cópias automáticas de 7 dias; o `backup.sh` (passo 6) acrescenta cópias próprias.

## 3. Emails (Amazon SES)

1. SES › **Identities** › Create identity › **Domain** `innovera.dstelecom.pt` com **Easy DKIM** → a TI da dst
   acrescenta os 3 registos CNAME (e, recomendado, um registo TXT de DMARC).
2. SES › **SMTP settings** › Create SMTP credentials → `SMTP_UTILIZADOR` e `SMTP_PASSWORD`.
3. SES › **Account dashboard** › **Request production access** (sem isto só envia para endereços verificados,
   máx. 200 por dia). A resposta costuma chegar em cerca de um dia.

## 4. Instalar no servidor

Ligar por SSH (botão "Connect" no Lightsail) e:

```bash
# memória extra para compilar com conforto (2 GB de swap)
sudo fallocate -l 2G /swapfile && sudo chmod 600 /swapfile && sudo mkswap /swapfile && sudo swapon /swapfile
echo '/swapfile none swap sw 0 0' | sudo tee -a /etc/fstab

# Docker
curl -fsSL https://get.docker.com | sudo sh
sudo usermod -aG docker $USER && newgrp docker

# código
sudo mkdir -p /opt/innovera && sudo chown $USER /opt/innovera
git clone -b versao2 https://github.com/JoaoPauloOliveiraSilva/ProjetoInovera.git /opt/innovera
cd /opt/innovera
cp deploy/.env.exemplo deploy/.env && nano deploy/.env      # preencher tudo o que diz ALTERAR

docker compose -f deploy/docker-compose.producao.yml up -d --build
docker compose -f deploy/docker-compose.producao.yml logs -f app   # ver o arranque (Ctrl+C para sair)
```

No primeiro arranque a aplicação cria as tabelas, os perfis e a conta de Administrador do `.env`.
Abrir `https://innovera.dstelecom.pt`, entrar com essa conta e:

1. Mudar a password (esqueci-me da password, ou pedir a outro Administrador).
2. **administração › importar Mod.246** com o Excel atual.
3. **administração › utilizadores**: adicionar a equipa de Inovação (perfil Equipa de Inovação) — cada pessoa recebe o convite por email.

## 5. Atualizar para uma versão nova

```bash
cd /opt/innovera && git pull
docker compose -f deploy/docker-compose.producao.yml up -d --build
```

As migrações da base de dados correm sozinhas no arranque. As sessões mantêm-se (as chaves de cifra estão no volume).

## 6. Cópias de segurança

```bash
chmod +x /opt/innovera/deploy/backup.sh
sudo mkdir -p /var/backups/innovera && sudo chown $USER /var/backups/innovera
crontab -e      # acrescentar a linha:
30 3 * * * /opt/innovera/deploy/backup.sh >> /var/backups/innovera/backup.log 2>&1
```

Guarda a base de dados (`pg_dump`) e os anexos todos os dias às 03:30, durante 14 dias. Para levar uma cópia para fora
do servidor: `scp` a partir do PC, ou um snapshot da instância no Lightsail.

Repor uma cópia: `docker run --rm -i -e PGPASSWORD=… postgres:17 pg_restore -h <endpoint> -U dbmasteruser -d innovera --clean < innovera-bd-<data>.dump`.

## 7. Custos e alarmes

- AWS Budgets › criar orçamento mensal de **35 USD** com alerta por email a 80 %.
- Custo previsto: instância 12 + base de dados 15 + SES < 1 ≈ **28 USD/mês** (o bucket de 1 USD fica para quando os
  anexos passarem para S3; por agora ficam no volume do servidor, com cópia diária).

## Testar tudo no PC antes

Com o Docker Desktop: copiar `deploy/.env.exemplo` para `deploy/.env`, pôr `DOMINIO=localhost` e a `LIGACAO_BD` da
linha comentada (`Host=bd…`), e

```bash
docker compose -f deploy/docker-compose.producao.yml --profile bd-local up -d --build
```

O site fica em https://localhost (certificado de teste do Caddy — o browser avisa, é normal). Para parar e apagar:
`docker compose -f deploy/docker-compose.producao.yml --profile bd-local down -v`.
