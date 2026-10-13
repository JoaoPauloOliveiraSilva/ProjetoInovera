import { useState } from 'react';
import { charter as api } from '../api/servicos';
import { nomeEstadoTrabalho } from '../api/tipos';
import type { MembroEquipa, ProjectCharter as Charter, Tarefa } from '../api/tipos';
import { TabelaEditavel, pessoas, texto } from '../componentes/TabelaEditavel';
import type { DefTabela } from '../componentes/TabelaEditavel';
import { formatarData } from '../dados/regras';

const meses = ['jan', 'fev', 'mar', 'abr', 'mai', 'jun', 'jul', 'ago', 'set', 'out', 'nov', 'dez'];
const pct = (n: number) => `${Math.round(n).toLocaleString('pt-PT')} %`;

/** Resumo da dedicação (a dst só quer a %, 0 = nada, 100 = dedicação máxima): média dos meses com alocação. */
function resumoAlocacao(percentagens: number[]) {
  const comAlocacao = percentagens.filter((p) => p > 0);
  if (comAlocacao.length === 0) return '—';
  const media = comAlocacao.reduce((t, p) => t + p, 0) / comAlocacao.length;
  return `${pct(media)} · ${comAlocacao.length} ${comAlocacao.length === 1 ? 'mês' : 'meses'}`;
}

// ---------- Equipa de trabalho ----------

const defEquipa: DefTabela<MembroEquipa> = {
  titulo: 'equipa de trabalho',
  singular: 'membro',
  colunas: 'minmax(200px, 1fr) minmax(160px, 1fr) 160px',
  cabecalhos: ['Nome', 'Função no projeto', 'Dedicação média'],
  celulas: (m) => [
    <span className="forte">
      {m.nome}
      {m.utilizadorId === null && <span className="cinzento-escuro"> · sem conta</span>}
    </span>,
    m.funcao || '—',
    <span className="num">{resumoAlocacao(m.alocacoes.map((a) => a.percentagem))}</span>,
  ],
  campos: [
    {
      nome: 'utilizador',
      rotulo: 'pessoa (com conta na plataforma)',
      tipo: 'pessoas',
      ajuda: 'Escolha uma só pessoa. Se não tiver conta, deixe vazio e escreva o nome em baixo.',
    },
    { nome: 'nome', rotulo: 'nome (só para quem não tem conta)', max: 200 },
    { nome: 'funcao', rotulo: 'função no projeto (ex.: gestor, técnico, consultor)', max: 150 },
  ],
  valores: (m) => ({
    utilizador: pessoas(m && m.utilizadorId !== null ? [{ id: m.utilizadorId, nome: m.nome, empresa: null }] : []),
    nome: m && m.utilizadorId === null ? m.nome : '',
    funcao: texto(m?.funcao),
  }),
  vazio: 'Ainda não há ninguém na equipa.',
};

/** Equipa de trabalho e alocação temporal por mês (Project Charter › planeamento). */
export function EquipaTrabalho({ charter, aoMudar }: { charter: Charter; aoMudar: () => void }) {
  return (
    <>
      <TabelaEditavel
        def={defEquipa}
        itens={charter.equipa}
        podeEditar={charter.podeEditar}
        guardar={(id, corpo) => {
          const escolhidos = (corpo.utilizador as number[] | undefined) ?? [];
          if (escolhidos.length > 1) return Promise.reject(new Error('Escolha só uma pessoa por linha.'));
          const utilizadorId = escolhidos[0] ?? null;
          return api.guardarMembro(charter.iniciativaId, id, {
            utilizadorId,
            nome: utilizadorId === null ? ((corpo.nome as string | null) ?? null) : null,
            funcao: (corpo.funcao as string | null) ?? null,
          });
        }}
        apagar={(id) => api.apagarMembro(charter.iniciativaId, id)}
        aoMudar={aoMudar}
        sobretitulo={`PROJECT CHARTER · ${charter.codigo}`}
      />
      {charter.equipa.length > 0 && <Alocacao charter={charter} aoMudar={aoMudar} />}
    </>
  );
}

function anoInicial(charter: Charter) {
  const atual = new Date().getFullYear();
  const inicio = charter.dataInicio ? Number(charter.dataInicio.slice(0, 4)) : atual;
  const fim = charter.dataFim ? Number(charter.dataFim.slice(0, 4)) : Math.max(atual, inicio);
  return Math.min(Math.max(atual, inicio), fim);
}

function Alocacao({ charter, aoMudar }: { charter: Charter; aoMudar: () => void }) {
  const [ano, setAno] = useState(() => anoInicial(charter));
  const anos = [ano - 1, ano, ano + 1];

  return (
    <section className="charter-seccao">
      <div className="charter-seccao-topo">
        <h3 className="secao">alocação mensal</h3>
        <label className="filtro-ano">
          ano
          <select value={ano} onChange={(e) => setAno(Number(e.target.value))}>
            {anos.map((a) => (
              <option key={a} value={a}>
                {a}
              </option>
            ))}
          </select>
        </label>
      </div>
      <p className="cinzento-escuro charter-nota">
        Dedicação de cada pessoa em % por mês: 0 = não trabalhou no projeto, 100 = dedicação máxima.
      </p>
      <div className="alocacao">
        <div className="alocacao-linha alocacao-cabecalho">
          <span>Pessoa</span>
          {meses.map((m) => (
            <span key={m}>{m}</span>
          ))}
          <span>Média {ano}</span>
          <span />
        </div>
        {charter.equipa.map((m) => (
          <LinhaAlocacao key={`${m.id}-${ano}`} charter={charter} membro={m} ano={ano} aoMudar={aoMudar} />
        ))}
        <div className="alocacao-linha alocacao-total">
          <span>Total da equipa</span>
          {meses.map((nome, i) => {
            const soma = charter.equipa.reduce(
              (t, m) => t + (m.alocacoes.find((a) => a.ano === ano && a.mes === i + 1)?.percentagem ?? 0),
              0,
            );
            return (
              <span key={nome} className="num" title={`${meses[i]} ${ano}: soma das dedicações da equipa`}>
                {soma ? `${Math.round(soma)}%` : ''}
              </span>
            );
          })}
          <span />
          <span />
        </div>
      </div>
    </section>
  );
}

function LinhaAlocacao({ charter, membro, ano, aoMudar }: { charter: Charter; membro: MembroEquipa; ano: number; aoMudar: () => void }) {
  const original = meses.map((_, i) => {
    const p = membro.alocacoes.find((a) => a.ano === ano && a.mes === i + 1)?.percentagem;
    return p ? String(p) : '';
  });
  const [valores, setValores] = useState(original);
  const [erro, setErro] = useState('');
  const [aEnviar, setAEnviar] = useState(false);
  const alterado = valores.some((v, i) => v !== original[i]);
  const numeros = valores.map((v) => (v.trim() ? Number(v.replace(',', '.')) : null));
  const invalido = numeros.some((n) => n !== null && (Number.isNaN(n) || n < 0 || n > 100));
  const media = resumoAlocacao(numeros.map((n) => (n !== null && !Number.isNaN(n) ? n : 0)));

  const guardar = async () => {
    setErro('');
    setAEnviar(true);
    try {
      await api.guardarAlocacoes(charter.iniciativaId, membro.id, ano, numeros);
      aoMudar();
    } catch (falha) {
      setErro(falha instanceof Error ? falha.message : String(falha));
    } finally {
      setAEnviar(false);
    }
  };

  return (
    <div className="alocacao-linha">
      <span className="forte" title={membro.funcao || undefined}>
        {membro.nome}
      </span>
      {valores.map((v, i) => (
        <span key={meses[i]}>
          {charter.podeEditar ? (
            <input
              aria-label={`${membro.nome}, ${meses[i]} ${ano} (%)`}
              inputMode="decimal"
              value={v}
              placeholder="0"
              onChange={(e) => setValores(valores.map((x, j) => (j === i ? e.target.value : x)))}
            />
          ) : (
            <span className="num">{v ? `${v}%` : ''}</span>
          )}
        </span>
      ))}
      <span className="num">{media}</span>
      <span>
        {charter.podeEditar && alterado && (
          <button className="botao-pequeno" disabled={aEnviar || invalido} onClick={() => void guardar()} title={invalido ? 'Use valores de 0 a 100.' : undefined}>
            {aEnviar ? '…' : 'guardar'}
          </button>
        )}
        {erro && <span className="erro-curto" title={erro}>erro</span>}
      </span>
    </div>
  );
}

// ---------- Cronograma (Gantt) ----------

const dia = 24 * 3600 * 1000;
const paraData = (s: string) => new Date(`${s}T00:00:00`);
const fmt = (d: Date) => d.toLocaleDateString('pt-PT', { day: '2-digit', month: '2-digit', year: 'numeric' });

interface Barra {
  tarefa: Tarefa;
  inicio: Date | null;
  fim: Date | null;
  inicioReal: Date | null;
  fimReal: Date | null;
}

/** Cronograma a partir das tarefas (WBS): barra do planeado e, por baixo, a do real; losangos para os milestones. */
export function Gantt({ charter }: { charter: Charter }) {
  const hojeData = new Date();
  hojeData.setHours(0, 0, 0, 0);

  const barras: Barra[] = charter.tarefas
    .map((t) => {
      const inicio = t.inicioPlaneado ? paraData(t.inicioPlaneado) : null;
      const fim = t.fimPlaneado ? paraData(t.fimPlaneado) : inicio;
      const inicioReal = t.inicioReal ? paraData(t.inicioReal) : null;
      // Em curso: a barra real vai até hoje.
      const fimReal = t.fimReal ? paraData(t.fimReal) : inicioReal ? new Date(Math.max(hojeData.getTime(), inicioReal.getTime())) : null;
      return { tarefa: t, inicio: inicio ?? fim, fim, inicioReal, fimReal };
    })
    .filter((b) => b.inicio || b.inicioReal);

  const milestones = charter.entregas.filter((e) => e.tipo === 'Milestone' && e.prazo);

  const datas = [
    ...barras.flatMap((b) => [b.inicio, b.fim, b.inicioReal, b.fimReal]),
    ...milestones.map((m) => paraData(m.prazo as string)),
  ].filter((d): d is Date => d !== null);

  if (datas.length === 0) {
    return (
      <section className="charter-seccao">
        <h3 className="secao">cronograma</h3>
        <div className="vazio gantt-vazio">O cronograma aparece quando as tarefas tiverem datas de início e fim.</div>
      </section>
    );
  }

  const minimo = new Date(Math.min(...datas.map((d) => d.getTime())));
  const maximo = new Date(Math.max(...datas.map((d) => d.getTime())));
  const inicioEixo = new Date(minimo.getFullYear(), minimo.getMonth(), 1);
  const fimEixo = new Date(maximo.getFullYear(), maximo.getMonth() + 1, 1);
  const total = fimEixo.getTime() - inicioEixo.getTime();
  const pos = (d: Date) => ((d.getTime() - inicioEixo.getTime()) / total) * 100;
  const largura = (a: Date, b: Date) => Math.max(((b.getTime() - a.getTime() + dia) / total) * 100, 0.6);

  const colunasMes: { nome: string; esquerda: number; largura: number }[] = [];
  for (let d = new Date(inicioEixo); d < fimEixo; d = new Date(d.getFullYear(), d.getMonth() + 1, 1)) {
    const seguinte = new Date(d.getFullYear(), d.getMonth() + 1, 1);
    colunasMes.push({
      nome: d.getMonth() === 0 || colunasMes.length === 0 ? `${meses[d.getMonth()]} ${String(d.getFullYear()).slice(2)}` : meses[d.getMonth()],
      esquerda: pos(d),
      largura: ((seguinte.getTime() - d.getTime()) / total) * 100,
    });
  }
  const mostrarHoje = hojeData >= inicioEixo && hojeData < fimEixo;
  const linhasMes = colunasMes.map((m) => (
    <span key={m.esquerda} className="gantt-grelha" style={{ left: `${m.esquerda}%` }} />
  ));
  const linhaHoje = mostrarHoje && <span className="gantt-hoje" style={{ left: `${pos(hojeData)}%` }} />;
  const periodo = (a: Date | null, b: Date | null) =>
    a && b ? `${fmt(a)} → ${fmt(b)}` : '—';

  return (
    <section className="charter-seccao">
      <div className="charter-seccao-topo">
        <h3 className="secao">cronograma</h3>
        <div className="gantt-legenda" aria-label="Legenda">
          <span>
            <i className="gantt-chave planeado" /> planeado
          </span>
          <span>
            <i className="gantt-chave real" /> real
          </span>
          <span>
            <i className="gantt-chave atrasada" /> real, atrasada
          </span>
          {milestones.length > 0 && (
            <span>
              <i className="gantt-chave milestone" /> milestone
            </span>
          )}
          {mostrarHoje && (
            <span>
              <i className="gantt-chave hoje" /> hoje
            </span>
          )}
        </div>
      </div>
      <div className="gantt">
        <div className="gantt-linha gantt-cabecalho">
          <span>Tarefa</span>
          <span className="gantt-pista">
            {colunasMes.map((m) => (
              <span key={m.esquerda} className="gantt-mes" style={{ left: `${m.esquerda}%`, width: `${m.largura}%` }}>
                {m.nome}
              </span>
            ))}
          </span>
        </div>
        {barras.map((b) => {
          const t = b.tarefa;
          const titulo = `${t.codigoWbs ? `${t.codigoWbs} · ` : ''}${t.nome}`;
          return (
            <div key={t.id} className="gantt-linha">
              <span className="gantt-nome" title={titulo}>
                {t.codigoWbs && <span className="num">{t.codigoWbs} </span>}
                {t.nome}
              </span>
              <span className="gantt-pista">
                {linhasMes}
                {linhaHoje}
                {b.inicio && b.fim && (
                  <span
                    className="gantt-barra planeado"
                    style={{ left: `${pos(b.inicio)}%`, width: `${largura(b.inicio, b.fim)}%` }}
                    title={`${titulo}\nplaneado: ${periodo(b.inicio, b.fim)}\nestado: ${nomeEstadoTrabalho[t.estado]}`}
                  />
                )}
                {b.inicioReal && b.fimReal && (
                  <span
                    className={`gantt-barra real${t.atrasada ? ' atrasada' : ''}`}
                    style={{ left: `${pos(b.inicioReal)}%`, width: `${largura(b.inicioReal, b.fimReal)}%` }}
                    title={`${titulo}\nreal: ${periodo(b.inicioReal, b.fimReal)}${t.fimReal ? '' : ' (em curso)'}${t.atrasada ? '\natrasada' : ''}`}
                  />
                )}
              </span>
            </div>
          );
        })}
        {milestones.length > 0 && (
          <div className="gantt-linha gantt-milestones">
            <span className="gantt-nome">Milestones</span>
            <span className="gantt-pista">
              {linhasMes}
              {linhaHoje}
              {milestones.map((m) => (
                <span
                  key={m.id}
                  className={`gantt-milestone${m.estado === 'Concluido' ? ' feito' : ''}`}
                  style={{ left: `${pos(paraData(m.prazo as string))}%` }}
                  title={`${m.codigo ? `${m.codigo} · ` : ''}${m.descricao}\nprazo: ${formatarData(m.prazo as string)}\nestado: ${nomeEstadoTrabalho[m.estado]}`}
                />
              ))}
            </span>
          </div>
        )}
      </div>
    </section>
  );
}
