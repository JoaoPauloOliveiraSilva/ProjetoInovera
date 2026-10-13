import { useState } from 'react';
import { Link } from 'react-router-dom';
import { Permissoes } from '../api/permissoes';
import { estrategia as api, iniciativas as apiIniciativas, kpis as apiKpis } from '../api/servicos';
import { areasAtuacao } from '../api/tipos';
import type { IniciativaResumo, Kpi, LinhaEstrategia } from '../api/tipos';
import { usePedido } from '../api/usePedido';
import { EstadoPedido } from '../componentes/Estado';
import { Cabecalho } from '../componentes/Layout';
import { TabelaEditavel, texto } from '../componentes/TabelaEditavel';
import type { DefTabela } from '../componentes/TabelaEditavel';
import { usePermissao } from '../sessao/Sessao';

const horas = (n: number) => `${Math.round(n).toLocaleString('pt-PT')} h`;

function valorKpi(k: Kpi | undefined) {
  if (!k || k.valor === null) return '—';
  if (k.unidade === 'Percentagem') return `${k.valor.toLocaleString('pt-PT', { maximumFractionDigits: 1 })} %`;
  if (k.unidade === 'Euros') return k.valor.toLocaleString('pt-PT', { style: 'currency', currency: 'EUR', maximumFractionDigits: 0 });
  return k.valor.toLocaleString('pt-PT', { maximumFractionDigits: 1 });
}

function defEstrategia(ano: number, areas: string[], iniciativas: IniciativaResumo[], kpis: Kpi[]): DefTabela<LinhaEstrategia> {
  return {
    titulo: 'linhas da estratégia',
    singular: 'linha',
    feminino: true,
    colunas: 'minmax(130px, 0.7fr) minmax(220px, 1.4fr) minmax(150px, 0.9fr) 150px 60px 110px',
    cabecalhos: ['Área de atuação', 'Abordagem · plano de ação', `KPIs (valor ${ano})`, 'Recursos', 'Horiz.', 'Iniciativa'],
    celulas: (l) => [
      <span className="forte">{l.areaAtuacao}</span>,
      <span>
        {l.abordagem && <span className="forte">{l.abordagem}</span>}
        {l.planoAcao && <span className="caixa-id">{l.planoAcao}</span>}
        {!l.abordagem && !l.planoAcao && '—'}
      </span>,
      l.kpis.length === 0 ? (
        <span className="cinzento-escuro">—</span>
      ) : (
        <span className="kpis-linha">
          {l.kpis.map((k) => (
            <span key={k.numero} title={k.nome}>
              <span className="num">KPI {k.numero}</span> {valorKpi(kpis.find((x) => x.numero === k.numero))}
            </span>
          ))}
        </span>
      ),
      <span
        title={
          l.dedicacaoTotalHoras !== null
            ? `${l.duracaoMeses} meses × ${l.numeroRecursosHumanos} RH × ${l.horasMesPorRecurso} h/mês`
            : undefined
        }
      >
        {l.dedicacaoTotalHoras !== null ? <span className="num">{horas(l.dedicacaoTotalHoras)}</span> : '—'}
        {l.recursosExtra && <span className="caixa-id">{l.recursosExtra}</span>}
      </span>,
      l.horizonte ?? '—',
      l.iniciativaId ? (
        <Link className="ligacao" to={`/inovacao/iniciativas/${l.iniciativaId}`} title={l.iniciativaTitulo ?? undefined} onClick={(e) => e.stopPropagation()}>
          {l.iniciativaCodigo}
        </Link>
      ) : (
        '—'
      ),
    ],
    campos: [
      {
        nome: 'areaAtuacao',
        rotulo: 'área de atuação',
        tipo: 'opcoes',
        opcoes: areas.map((a): [string, string] => [a, a]),
        obrigatorio: true,
      },
      { nome: 'abordagem', rotulo: 'abordagem / aplicação específica', max: 500 },
      { nome: 'planoAcao', rotulo: 'plano de ação (ex.: desenvolver o projeto AI Poles)', tipo: 'area', max: 2000 },
      {
        nome: 'kpis',
        rotulo: 'KPIs associados (números do Mod.239)',
        ajuda: 'Separados por vírgula, ex.: 14, 36. Os KPIs vão de 1 a 41.',
        max: 100,
      },
      { nome: 'duracaoMeses', rotulo: 'duração (meses)', tipo: 'numero' },
      { nome: 'numeroRecursosHumanos', rotulo: 'n.º de RH', tipo: 'numero' },
      {
        nome: 'horasMesPorRecurso',
        rotulo: 'horas por mês de cada RH',
        tipo: 'numero',
        ajuda: 'Dedicação total = duração × n.º de RH × horas/mês.',
      },
      { nome: 'recursosExtra', rotulo: 'recursos extra (ex.: Consultoria INESC - 30k€)', max: 500 },
      { nome: 'horizonte', rotulo: 'horizonte', tipo: 'opcoes', opcoes: [['H1', 'H1'], ['H2', 'H2'], ['H3', 'H3']] },
      {
        nome: 'iniciativaId',
        rotulo: 'iniciativa ligada',
        tipo: 'opcoes',
        numerico: true,
        opcoes: iniciativas.map((i): [string, string] => [String(i.id), `${i.codigo} · ${i.titulo}`]),
      },
    ],
    valores: (l) => ({
      areaAtuacao: l?.areaAtuacao ?? areas[0],
      abordagem: texto(l?.abordagem),
      planoAcao: texto(l?.planoAcao),
      kpis: l ? l.kpis.map((k) => k.numero).join(', ') : '',
      duracaoMeses: texto(l?.duracaoMeses),
      numeroRecursosHumanos: texto(l?.numeroRecursosHumanos),
      horasMesPorRecurso: texto(l?.horasMesPorRecurso),
      recursosExtra: texto(l?.recursosExtra),
      horizonte: texto(l?.horizonte),
      iniciativaId: texto(l?.iniciativaId),
    }),
    vazio: `Ainda não há estratégia para ${ano}.`,
  };
}

/** "14, 36" ou "14 e 36" → [14, 36]. Texto que não é número fica de fora (o servidor valida 1–41). */
const lerKpis = (v: unknown) =>
  String(v ?? '')
    .split(/[^0-9]+/)
    .filter(Boolean)
    .map(Number);

/** Estratégia de inovação anual (Mod.52): áreas, planos de ação, KPIs, recursos e horizonte. */
export function EstrategiaPagina() {
  const tem = usePermissao();
  const podeEditar = tem(Permissoes.gerirRegistos);
  const [ano, setAno] = useState(new Date().getFullYear());
  const [erroCopia, setErroCopia] = useState('');
  const { dados, erro, aCarregar, recarregar } = usePedido(() => api.listar(ano), [ano]);
  const indicadores = usePedido(() => apiKpis.indicadores(ano), [ano]);
  const iniciativas = usePedido(() => (podeEditar ? apiIniciativas.listar() : Promise.resolve([])), [podeEditar]);
  const anoAtual = new Date().getFullYear();

  const linhas = [...(dados ?? [])].sort((a, b) => a.areaAtuacao.localeCompare(b.areaAtuacao, 'pt'));
  const areas = [...new Set([...areasAtuacao, ...linhas.map((l) => l.areaAtuacao)])];
  const total = linhas.reduce((t, l) => t + (l.dedicacaoTotalHoras ?? 0), 0);
  const porHorizonte = (['H1', 'H2', 'H3'] as const).map((h) => [h, linhas.filter((l) => l.horizonte === h).length] as const);

  const copiar = async () => {
    setErroCopia('');
    try {
      await api.copiar(ano - 1, ano);
      recarregar();
    } catch (falha) {
      setErroCopia(falha instanceof Error ? falha.message : String(falha));
    }
  };

  return (
    <>
      <Cabecalho titulo="estratégia" corLampada="#7a5aa0" />
      <div className="filtros filtros-varios">
        <label>
          ano
          <select value={ano} onChange={(e) => setAno(Number(e.target.value))}>
            {[anoAtual + 1, anoAtual, anoAtual - 1, anoAtual - 2].map((a) => (
              <option key={a} value={a}>
                {a}
              </option>
            ))}
          </select>
        </label>
        <span className="cinzento-escuro">Estratégia de inovação (Mod.52): o que se vai fazer em cada área, com que recursos e que KPIs mexe.</span>
      </div>
      <EstadoPedido aCarregar={aCarregar} erro={erro} vazio={dados === undefined} />
      {dados && (
        <>
          {linhas.length > 0 && (
            <div className="estrategia-resumo">
              <div className="kpi">
                <span className="kpi-nome">linhas de ação</span>
                <span className="kpi-valor">{linhas.length}</span>
                <span className="kpi-rodape">{new Set(linhas.map((l) => l.areaAtuacao)).size} áreas de atuação</span>
              </div>
              <div className="kpi">
                <span className="kpi-nome">dedicação total</span>
                <span className="kpi-valor">{horas(total)}</span>
                <span className="kpi-rodape">duração × RH × horas/mês</span>
              </div>
              <div className="kpi">
                <span className="kpi-nome">por horizonte</span>
                <span className="kpi-valor kpi-valor-texto">{porHorizonte.map(([h, n]) => `${h} ${n}`).join(' · ')}</span>
                <span className="kpi-rodape">H1 negócio atual · H3 novas áreas</span>
              </div>
            </div>
          )}
          {podeEditar && linhas.length === 0 && (
            <div className="aviso-copiar">
              <span>A estratégia de {ano} está vazia.</span>
              <button className="botao-pequeno" onClick={() => void copiar()}>
                começar a partir de {ano - 1}
              </button>
              {erroCopia && <span className="erro">{erroCopia}</span>}
            </div>
          )}
          <TabelaEditavel
            def={defEstrategia(ano, areas, iniciativas.dados ?? [], indicadores.dados?.kpis ?? [])}
            itens={linhas}
            podeEditar={podeEditar}
            guardar={(id, corpo) => api.guardar(id, { ...corpo, ano, kpis: lerKpis(corpo.kpis) })}
            apagar={(id) => api.apagar(id)}
            aoMudar={recarregar}
            sobretitulo={`ESTRATÉGIA DE INOVAÇÃO · ${ano}`}
            semTitulo
          />
        </>
      )}
    </>
  );
}
