#!/usr/bin/env bash
# Cópia de segurança diária da base de dados e dos anexos (correr no servidor, ver docs/publicacao-aws.md).
#   crontab: 30 3 * * * /opt/innovera/deploy/backup.sh >> /var/log/innovera-backup.log 2>&1
set -euo pipefail

PASTA=${PASTA_BACKUPS:-/var/backups/innovera}
DIAS=${DIAS_A_GUARDAR:-14}
AQUI=$(cd "$(dirname "$0")" && pwd)
DATA=$(date +%Y-%m-%d_%H%M)
mkdir -p "$PASTA"

# Lê LIGACAO_BD do .env e converte para os parâmetros do pg_dump.
LIGACAO=$(grep '^LIGACAO_BD=' "$AQUI/.env" | cut -d= -f2-)
valor() { echo "$LIGACAO" | tr ';' '\n' | grep -i "^$1=" | cut -d= -f2-; }

docker run --rm -e PGPASSWORD="$(valor Password)" postgres:17 \
  pg_dump -h "$(valor Host)" -p "$(valor Port)" -U "$(valor Username)" -d "$(valor Database)" -Fc \
  > "$PASTA/innovera-bd-$DATA.dump"

# Anexos (volume "dados" da aplicação).
docker run --rm -v innovera_dados:/data:ro -v "$PASTA":/backup alpine \
  tar czf "/backup/innovera-anexos-$DATA.tar.gz" -C /data ficheiros

find "$PASTA" -name 'innovera-*' -mtime +"$DIAS" -delete
echo "$(date) backup feito em $PASTA"
