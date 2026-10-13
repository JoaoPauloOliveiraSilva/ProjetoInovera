import { useState } from 'react';
import { Link, useParams, useSearchParams } from 'react-router-dom';
import { kpis as api } from '../api/servicos';
import { nomeTipoAcaoKpi } from '../api/tipos';
import type { AcaoKpi, PlanoKpi } from '../api/tipos';
import { usePedido } from '../api/usePedido';
import { EstadoPedido } from '../componentes/Estado';
import { Cabecalho } from '../componentes/Layout';
import { TabelaEditavel, data, hoje, opcoes, pessoas, texto } from '../componentes/TabelaEditavel';
import type { DefTabela } from '../componentes/TabelaEditavel';
import { formatar } from './Indicadores';

const defAcoes: DefTabela<AcaoKpi> = {
  titulo: 'plano de ação e acompanhamento',
  singular: 'ação',
  feminino: true,
  colunas: '150px minmax(220px, 1.4fr) minmax(140px, 0.8fr) 100px 100px 100px',
  cabecalhos: ['Tipo', 'Ação', 'Responsável', 'Prazo', 'Concluída', 'Eficaz'],
  celulas: (a) => [
    nomeTipoAcaoKpi[a.tipo],
    <span>
      <span className="forte">{a.descricao}</span>
      {a.analiseResultados && <span className="caixa-id">{a.analiseResultados}</span>}
    </span>,
    a.responsavel ?? '—',
    data(a.prazo),
    data(a.concluidaEm),
    a.eficaz === null ? '—' : a.eficaz ? <span className="etiqueta objetivo-atingido">eficaz</span> : <span className="etiqueta objetivo-falhado">não eficaz</span>,
  ],
  campos: [
    { nome: 'tipo', rotulo: 'tipo', tipo: 'opcoes', opcoes: opcoes(nomeTipoAcaoKpi), obrigatorio: true },
    {
      nome: 'analiseResultados',
      rotulo: 'análise dos resultados',
      tipo: 'area',
      max: 4000,
      ajuda: 'No acompanhamento: o que mostram os números e porquê.',
    },
    { nome: 'descricao', rotulo: 'ação (plano de ação)', tipo: 'area', obrigatorio: true, max: 2000 },
    { nome: 'recursos', rotulo: 'recursos', max: 1000 },
    { nome: 'responsavel', rotulo: 'responsável', tipo: 'pessoas', ajuda: 'Escolha uma pessoa.' },
    { nome: 'dataCriacao', rotulo: 'data de criação', tipo: 'data' },
    { nome: 'prazo', rotulo: 'prazo', tipo: 'data' },
    { nome: 'concluidaEm', rotulo: 'concluída em', tipo: 'data' },
    {
      nome: 'eficaz',
      rotulo: 'resultado',
      tipo: 'opcoes',
      opcoes: [
        ['true', 'eficaz'],
        ['false', 'não eficaz'],
      ],
    },
    { nome: 'avaliacaoAnual', rotulo: 'avaliação no fim do ano', tipo: 'area', max: 4000 },
  ],
  valores: (a) => ({
    tipo: a?.tipo ?? 'PlanoInicial',
    analiseResultados: texto(a?.analiseResultados),
    descricao: texto(a?.descricao),
    recursos: texto(a?.recursos),
    responsavel: pessoas(a?.responsavelId ? [{ id: a.responsavelId, nome: a.responsavel ?? '', empresa: null }] : []),
    dataCriacao: a ? a.dataCriacao : hoje(),
    prazo: texto(a?.prazo),
    concluidaEm: texto(a?.concluidaEm),
    eficaz: a?.eficaz === null || a?.eficaz === undefined ? '' : String(a.eficaz),
    avaliacaoAnual: texto(a?.avaliacaoAnual),
  }),
  vazio: 'Ainda não há ações para este indicador.',
};

/** Objetivo anual e plano de ação de um KPI (bloco ISO do Mod.239). */
export function PlanoKpiPagina() {
  const { numero } = useParams();
  const [parametros, setParametros] = useSearchParams();
  const n = Number(numero);
  const ano = Number(parametros.get('ano')) || new Date().getFullYear();
  const plano = usePedido(() => api.plano(n, ano), [n, ano]);
  const indicadores = usePedido(() => api.indicadores(ano), [ano]);
  const kpi = indicadores.dados?.kpis.find((k) => k.numero === n);

  return (
    <>
      <Cabecalho titulo={`KPI ${n}`} corLampada="#3a6f9a" />
      <div className="filtros filtros-varios">
        <Link className="ligacao" to="/inovacao/indicadores">
          ← indicadores
        </Link>
        <label>
          ano
          <select value={ano} onChange={(e) => setParametros({ ano: e.target.value }, { replace: true })}>
            {[ano + 1, ano, ano - 1, ano - 2].map((a) => (
              <option key={a} value={a}>
                {a}
              </option>
            ))}
          </select>
        </label>
      </div>
      <EstadoPedido aCarregar={plano.aCarregar} erro={plano.erro} vazio={plano.dados === undefined} />
      {plano.dados && (
        <>
          <section className="plano-kpi-topo">
            <div>
              <span className="kpi-numero">{plano.dados.categoria.toUpperCase()}</span>
              <h2 className="secao" style={{ margin: '4px 0 0' }}>
                {plano.dados.nome}
              </h2>
            </div>
            <div className="kpi" style={{ minHeight: 0, padding: 0 }}>
              <span className="kpi-nome">valor em {ano}</span>
              <span className="kpi-valor">{kpi ? formatar(kpi, kpi.valor) : '—'}</span>
              {kpi?.atingido != null && (
                <span className={`etiqueta ${kpi.atingido ? 'objetivo-atingido' : 'objetivo-falhado'}`}>
                  {kpi.atingido ? '✓ objetivo atingido' : '✗ abaixo do objetivo'}
                </span>
              )}
            </div>
            <Objetivo plano={plano.dados} aoGuardar={() => {
              plano.recarregar();
              indicadores.recarregar();
            }} />
          </section>
          <TabelaEditavel
            def={defAcoes}
            itens={plano.dados.acoes}
            podeEditar={plano.dados.podeEditar}
            guardar={(id, corpo) => {
              const responsaveis = (corpo.responsavel as number[] | undefined) ?? [];
              if (responsaveis.length > 1) return Promise.reject(new Error('Escolha só uma pessoa como responsável.'));
              const { responsavel: _ignorar, eficaz, ...resto } = corpo;
              void _ignorar;
              return api.guardarAcao(n, ano, id, {
                ...resto,
                responsavelId: responsaveis[0] ?? null,
                eficaz: eficaz === null || eficaz === undefined ? null : eficaz === 'true',
              });
            }}
            apagar={(id) => api.apagarAcao(n, ano, id)}
            aoMudar={() => {
              plano.recarregar();
              indicadores.recarregar();
            }}
            sobretitulo={`KPI ${n} · ${ano}`}
          />
        </>
      )}
    </>
  );
}

function Objetivo({ plano, aoGuardar }: { plano: PlanoKpi; aoGuardar: () => void }) {
  const [valor, setValor] = useState(plano.objetivo ?? '');
  const [erro, setErro] = useState('');
  const [aEnviar, setAEnviar] = useState(false);
  const alterado = valor.trim() !== (plano.objetivo ?? '');

  const guardar = async () => {
    setErro('');
    setAEnviar(true);
    try {
      await api.guardarObjetivo(plano.numero, plano.ano, valor.trim() || null);
      aoGuardar();
    } catch (falha) {
      setErro(falha instanceof Error ? falha.message : String(falha));
    } finally {
      setAEnviar(false);
    }
  };

  return (
    <div>
      <span className="kpi-nome">objetivo anual</span>
      {plano.podeEditar ? (
        <div className="plano-objetivo">
          <input
            value={valor}
            maxLength={50}
            placeholder="ex.: ≥ 30"
            aria-label="Objetivo anual"
            onChange={(e) => setValor(e.target.value)}
          />
          {alterado && (
            <button className="botao-pequeno" disabled={aEnviar} onClick={() => void guardar()}>
              {aEnviar ? '…' : 'guardar'}
            </button>
          )}
        </div>
      ) : (
        <div className="kpi-valor" style={{ fontSize: 22 }}>
          {plano.objetivo ?? '—'}
        </div>
      )}
      <small className="cinzento-escuro">Como no Mod.239: ≥ 30, &gt; 1 000 000 €, ≥ 80 %…</small>
      {erro && <div className="erro">{erro}</div>}
    </div>
  );
}
