import { useEffect, useState } from 'react';
import type { ReactNode } from 'react';
import { contas, ideias as apiIdeias } from '../api/servicos';
import { nomeTipo } from '../api/tipos';
import type { IdeiaCriada, TipoIdeia, UtilizadorResumo } from '../api/tipos';
import { useAtualizar } from '../api/usePedido';
import { Gaveta } from '../componentes/Gaveta';
import { Camara, Fechar, SetaContinuar, Upload, Visto } from '../componentes/Icones';
import { useUtilizador } from '../sessao/Sessao';

type Autoria = 'eu' | 'anonimo' | 'outros';

const tamanhoMaximo = 25 * 1024 * 1024;

const termos = [
  'Pela subscrição da presente Declaração, exercida através do botão “Aceitar”, reconheço renunciar a todos os direitos relativos a qualquer propriedade, reconhecimento, direito, compensação monetária ou royalties atribuídos ou devidos ao dstgroup em resultado da venda, cessão, licenciamento ou qualquer outra transação envolvendo os direitos da propriedade intelectual. Reconheço igualmente que qualquer contribuição submetida através desta plataforma, se aceite pelo dstgroup, será, bem como a propriedade intelectual a ela relativa, propriedade exclusiva do dstgroup.',
  'Texto provisório — o texto final é o da declaração em vigor no site atual, a fornecer pela equipa de Inovação.',
];

function Passo({ n, titulo, estado, children }: { n: number; titulo: string; estado: 'feito' | 'atual' | 'futuro'; children?: ReactNode }) {
  return (
    <section className={`passo ${estado}`}>
      <div className="passo-cabecalho">
        <span className="passo-numero">{estado === 'feito' ? <Visto /> : n}</span>
        <h3>{titulo}</h3>
      </div>
      {estado === 'atual' && <div className="passo-corpo">{children}</div>}
    </section>
  );
}

function Opcao({ escolhida, onClick, children }: { escolhida: boolean; onClick: () => void; children: ReactNode }) {
  return (
    <button type="button" className={`opcao${escolhida ? ' escolhida' : ''}`} onClick={onClick}>
      <span className="opcao-circulo">{escolhida && <Visto />}</span>
      {children}
    </button>
  );
}

function Campo({ nome, valor, mudar, linhas = 3 }: { nome: string; valor: string; mudar: (v: string) => void; linhas?: number }) {
  return (
    <label className="campo">
      <span>{nome}</span>
      {linhas === 1 ? (
        <input value={valor} onChange={(e) => mudar(e.target.value)} />
      ) : (
        <textarea value={valor} rows={linhas} onChange={(e) => mudar(e.target.value)} />
      )}
    </label>
  );
}

/** Pesquisa de colegas para coautores (GET /api/Utilizadores?texto=). */
function EscolherCoautores({ escolhidos, mudar }: { escolhidos: UtilizadorResumo[]; mudar: (lista: UtilizadorResumo[]) => void }) {
  const eu = useUtilizador();
  const [texto, setTexto] = useState('');
  const [resultados, setResultados] = useState<UtilizadorResumo[]>([]);

  useEffect(() => {
    const t = texto.trim();
    if (t.length < 2) {
      setResultados([]);
      return;
    }
    let ativo = true;
    const espera = setTimeout(() => {
      contas
        .pesquisar(t)
        .then((lista) => ativo && setResultados(lista.filter((u) => u.id !== eu.id && !escolhidos.some((e) => e.id === u.id))))
        .catch(() => ativo && setResultados([]));
    }, 250);
    return () => {
      ativo = false;
      clearTimeout(espera);
    };
  }, [texto, escolhidos, eu.id]);

  return (
    <div className="coautores">
      <label className="campo">
        <span>outros autores</span>
        <input value={texto} onChange={(e) => setTexto(e.target.value)} placeholder="escreva o nome de um colega" />
      </label>
      {resultados.length > 0 && (
        <ul className="sugestoes">
          {resultados.map((u) => (
            <li key={u.id}>
              <button
                type="button"
                onClick={() => {
                  mudar([...escolhidos, u]);
                  setTexto('');
                }}
              >
                <strong>{u.nome}</strong> {u.empresa && <span className="cinzento-escuro">{u.empresa}</span>}
              </button>
            </li>
          ))}
        </ul>
      )}
      {escolhidos.length > 0 && (
        <div className="fichas">
          {escolhidos.map((u) => (
            <span key={u.id} className="ficha-autor-escolhido">
              {u.nome}
              <button type="button" aria-label={`Tirar ${u.nome}`} onClick={() => mudar(escolhidos.filter((e) => e.id !== u.id))}>
                <Fechar width={12} height={12} />
              </button>
            </span>
          ))}
        </div>
      )}
    </div>
  );
}

export function NovaIdeiaGaveta({ aberta, aoFechar }: { aberta: boolean; aoFechar: () => void }) {
  return (
    <Gaveta aberta={aberta} aoFechar={aoFechar}>
      {aberta && <NovaIdeia aoTerminar={aoFechar} />}
    </Gaveta>
  );
}

function NovaIdeia({ aoTerminar }: { aoTerminar: () => void }) {
  const atualizarListas = useAtualizar();
  const [passo, setPasso] = useState(1);
  const [tipo, setTipo] = useState<TipoIdeia | null>(null);
  const [titulo, setTitulo] = useState('');
  const [descricao, setDescricao] = useState('');
  const [visivel, setVisivel] = useState(true);
  const [autoria, setAutoria] = useState<Autoria>('eu');
  const [coautores, setCoautores] = useState<UtilizadorResumo[]>([]);
  const [anexos, setAnexos] = useState<File[]>([]);
  const [erroAnexo, setErroAnexo] = useState('');
  const [vantagens, setVantagens] = useState('');
  const [requisitos, setRequisitos] = useState('');
  const [comoEFeito, setComoEFeito] = useState('');
  const [modeloNegocio, setModeloNegocio] = useState('');
  const [competidores, setCompetidores] = useState('');
  const [custos, setCustos] = useState('');
  const [aceito, setAceito] = useState(false);
  const [registada, setRegistada] = useState<IdeiaCriada | null>(null);
  const [aEnviar, setAEnviar] = useState(false);
  const [erro, setErro] = useState('');
  const [avisoAnexos, setAvisoAnexos] = useState('');

  const estado = (n: number): 'feito' | 'atual' | 'futuro' => (n < passo ? 'feito' : n === passo ? 'atual' : 'futuro');

  const juntarAnexos = (ficheiros: FileList | null) => {
    if (!ficheiros) return;
    const novos = [...anexos, ...Array.from(ficheiros)];
    const total = novos.reduce((s, f) => s + f.size, 0);
    if (total > tamanhoMaximo) {
      setErroAnexo('Os anexos ultrapassam o limite de 25MB.');
      return;
    }
    setErroAnexo('');
    setAnexos(novos);
  };

  const submeter = async () => {
    setErro('');
    setAEnviar(true);
    try {
      const vazioANull = (v: string) => (v.trim() ? v.trim() : null);
      const detalhado = tipo === 'NovoProdutoServicoDetalhado';
      const criada = await apiIdeias.criar({
        tipo: tipo ?? 'Melhoria',
        titulo: titulo.trim(),
        descricao: descricao.trim(),
        privada: !visivel,
        anonima: autoria === 'anonimo',
        coautoresIds: autoria === 'outros' ? coautores.map((c) => c.id) : [],
        vantagens: vazioANull(vantagens),
        requisitos: vazioANull(requisitos),
        comoEFeitoAtualmente: vazioANull(comoEFeito),
        modeloNegocio: detalhado ? vazioANull(modeloNegocio) : null,
        competidores: detalhado ? vazioANull(competidores) : null,
        custos: detalhado ? vazioANull(custos) : null,
        termosAceites: aceito,
      });

      // Os anexos vão depois de a ideia existir (um pedido por ficheiro).
      const falhados: string[] = [];
      for (const f of anexos) {
        try {
          await apiIdeias.anexar(criada.id, f);
        } catch {
          falhados.push(f.name);
        }
      }
      if (falhados.length > 0) {
        setAvisoAnexos(`Não foi possível anexar: ${falhados.join(', ')}. Pode tentar outra vez a partir da ideia.`);
      }

      setRegistada(criada);
      atualizarListas();
    } catch (e) {
      setErro(e instanceof Error ? e.message : String(e));
    } finally {
      setAEnviar(false);
    }
  };

  if (registada !== null) {
    return (
      <div className="registada">
        <span className="passo-numero grande">
          <Visto />
        </span>
        <h3>Ideia {registada.codigo} registada</h3>
        <p>
          {autoria === 'outros'
            ? 'Os outros autores vão receber um pedido para confirmar a autoria. Depois a equipa de Inovação valida a ideia e abre a discussão pública durante 30 dias.'
            : 'A equipa de Inovação vai validar a ideia e depois abre a discussão pública durante 30 dias.'}
        </p>
        {avisoAnexos && <p className="erro">{avisoAnexos}</p>}
        <button className="botao-largo" onClick={aoTerminar}>
          fechar
        </button>
      </div>
    );
  }

  return (
    <div className="assistente">
      <Passo n={1} titulo="tipo de ideia" estado={estado(1)}>
        <div className="opcoes">
          {(Object.keys(nomeTipo) as TipoIdeia[]).map((t) => (
            <Opcao key={t} escolhida={tipo === t} onClick={() => setTipo(t)}>
              {nomeTipo[t]}
            </Opcao>
          ))}
        </div>
        <div className="botoes">
          <button className="botao-largo" disabled={tipo === null} onClick={() => setPasso(2)}>
            continuar <SetaContinuar className="seta-vermelha" />
          </button>
        </div>
      </Passo>

      <Passo n={2} titulo="informação geral" estado={estado(2)}>
        <Campo nome="título" valor={titulo} mudar={setTitulo} linhas={1} />
        <Campo nome="descrição" valor={descricao} mudar={setDescricao} />
        <label className="caixa">
          <input type="checkbox" checked={visivel} onChange={(e) => setVisivel(e.target.checked)} />
          <span className="caixa-quadrado">{visivel && <Visto />}</span>
          Visível para todos
        </label>
        <div className="rotulo">autor ou autores da ideia</div>
        <div className="opcoes">
          <Opcao escolhida={autoria === 'eu'} onClick={() => setAutoria('eu')}>
            Autor é o utilizador actual
          </Opcao>
          <Opcao escolhida={autoria === 'anonimo'} onClick={() => setAutoria('anonimo')}>
            Autor é anónimo
          </Opcao>
          <Opcao escolhida={autoria === 'outros'} onClick={() => setAutoria('outros')}>
            Outro autor ou autores
          </Opcao>
        </div>
        {autoria === 'outros' && <EscolherCoautores escolhidos={coautores} mudar={setCoautores} />}
        <label
          className="anexos"
          onDragOver={(e) => e.preventDefault()}
          onDrop={(e) => {
            e.preventDefault();
            juntarAnexos(e.dataTransfer.files);
          }}
        >
          <input type="file" multiple onChange={(e) => juntarAnexos(e.target.files)} />
          <strong>
            <Camara /> anexar ficheiros
          </strong>
          <span className="cinzento">ou arraste para aqui</span>
          <span>* Limite 25MB</span>
          {anexos.length > 0 && <span className="lista-anexos">{anexos.map((f) => f.name).join(' · ')}</span>}
          {erroAnexo && <span className="erro">{erroAnexo}</span>}
        </label>
        <div className="botoes">
          <button className="botao-largo curto" onClick={() => setPasso(1)}>
            voltar
          </button>
          <button
            className="botao-largo"
            disabled={!titulo.trim() || !descricao.trim() || (autoria === 'outros' && coautores.length === 0)}
            onClick={() => setPasso(3)}
          >
            continuar <SetaContinuar className="seta-vermelha" />
          </button>
        </div>
      </Passo>

      <Passo n={3} titulo="informação detalhada" estado={estado(3)}>
        <Campo nome="vantagens" valor={vantagens} mudar={setVantagens} />
        <Campo nome="requisitos" valor={requisitos} mudar={setRequisitos} />
        <Campo nome="como é feito actualmente" valor={comoEFeito} mudar={setComoEFeito} />
        {tipo === 'NovoProdutoServicoDetalhado' && (
          <>
            <Campo nome="modelo de negocio" valor={modeloNegocio} mudar={setModeloNegocio} />
            <Campo nome="competitores" valor={competidores} mudar={setCompetidores} />
            <Campo nome="custos" valor={custos} mudar={setCustos} />
          </>
        )}
        <div className="botoes">
          <button className="botao-largo curto" onClick={() => setPasso(2)}>
            voltar
          </button>
          <button className="botao-largo" onClick={() => setPasso(4)}>
            continuar <SetaContinuar className="seta-vermelha" />
          </button>
        </div>
      </Passo>

      <Passo n={4} titulo="termos e condições" estado={estado(4)}>
        {erro && <div className="erro">{erro}</div>}
        <div className="rotulo">ler termos e condições</div>
        {termos.map((t) => (
          <p key={t.slice(0, 20)} className="termos">
            {t}
          </p>
        ))}
        <label className="caixa">
          <input type="checkbox" checked={aceito} onChange={(e) => setAceito(e.target.checked)} />
          <span className="caixa-quadrado">{aceito && <Visto />}</span>
          li e aceito os termos e condições
        </label>
        <div className="botoes">
          <button className="botao-largo curto" onClick={() => setPasso(3)}>
            voltar
          </button>
          <button className="botao-largo" disabled={!aceito || aEnviar} onClick={() => void submeter()}>
            {aEnviar ? 'a registar…' : 'registar ideia'} <Upload className="seta-vermelha" />
          </button>
        </div>
      </Passo>
    </div>
  );
}
