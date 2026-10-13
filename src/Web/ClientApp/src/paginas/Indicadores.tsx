import { useState } from 'react';
import { Link } from 'react-router-dom';
import { Permissoes } from '../api/permissoes';
import { kpis as api } from '../api/servicos';
import { nomeEstadoIniciativa, nomeTipoIniciativa } from '../api/tipos';
import type { EstadoIniciativa, Indicadores, Kpi, TipoIniciativa } from '../api/tipos';
import { usePedido } from '../api/usePedido';
import { EstadoPedido } from '../componentes/Estado';
import { Cabecalho } from '../componentes/Layout';
import { formatarData } from '../dados/regras';
import { usePermissao } from '../sessao/Sessao';

function formatar(k: Kpi, valor: number | null) {
  if (valor === null) return '—';
  switch (k.unidade) {
    case 'Percentagem':
      return `${valor.toLocaleString('pt-PT', { maximumFractionDigits: 1 })} %`;
    case 'Euros':
      return valor.toLocaleString('pt-PT', { style: 'currency', currency: 'EUR', maximumFractionDigits: 0 });
    case 'Media':
      return valor.toLocaleString('pt-PT', { maximumFractionDigits: 1 });
    default:
      return valor.toLocaleString('pt-PT', { maximumFractionDigits: 0 });
  }
}

/** Diferença para o ano anterior, em texto (a cor não diz se é bom ou mau: depende do indicador). */
function Variacao({ k }: { k: Kpi }) {
  if (k.valor === null || k.valorAnoAnterior === null) return null;
  const d = k.valor - k.valorAnoAnterior;
  if (d === 0) return <span className="kpi-variacao">= ano anterior</span>;
  const texto =
    k.unidade === 'Percentagem'
      ? `${Math.abs(d).toLocaleString('pt-PT', { maximumFractionDigits: 1 })} p.p.`
      : formatar(k, Math.abs(d));
  return (
    <span className="kpi-variacao">
      {d > 0 ? '▲' : '▼'} {texto}
    </span>
  );
}

function CartaoKpi({ k, ano }: { k: Kpi; ano: number }) {
  return (
    <div className={`kpi${k.disponivel ? '' : ' indisponivel'}`} title={k.nota ?? undefined}>
      <span className="kpi-numero">KPI {k.numero}</span>
      <span className="kpi-nome">{k.nome}</span>
      <span className="kpi-valor">{formatar(k, k.valor)}</span>
      <span className="kpi-rodape">
        {k.disponivel ? (
          <>
            {ano - 1}: {formatar(k, k.valorAnoAnterior)} <Variacao k={k} />
          </>
        ) : (
          (k.nota ?? 'Sem dados.')
        )}
      </span>
      {k.disponivel && k.nota && <span className="kpi-nota">{k.nota}</span>}
    </div>
  );
}

/** Indicadores de desempenho (Mod.239): os 41 KPIs de um ano, com o anterior para comparar. */
export function IndicadoresPagina() {
  const tem = usePermissao();
  const [ano, setAno] = useState(new Date().getFullYear());
  const { dados, erro, aCarregar } = usePedido(() => api.indicadores(ano), [ano]);
  const anoAtual = new Date().getFullYear();

  const categorias = dados ? [...new Set(dados.kpis.map((k) => k.categoria))] : [];

  return (
    <>
      <Cabecalho
        titulo="indicadores"
        corLampada="#3a6f9a"
        acao={
          tem(Permissoes.valoresExternos) ? (
            <Link className="botao-pequeno" to="/inovacao/indicadores/valores-externos">
              valores externos
            </Link>
          ) : undefined
        }
      />
      <div className="filtros filtros-varios">
        <label>
          ano
          <select value={ano} onChange={(e) => setAno(Number(e.target.value))}>
            {[anoAtual, anoAtual - 1, anoAtual - 2, anoAtual - 3].map((a) => (
              <option key={a} value={a}>
                {a}
              </option>
            ))}
          </select>
        </label>
        <span className="cinzento-escuro">Indicadores do Mod.239, calculados a partir dos registos da plataforma.</span>
      </div>
      <EstadoPedido aCarregar={aCarregar} erro={erro} vazio={dados === undefined} />
      {dados && (
        <>
          <PainelResumo dados={dados} />
          {categorias.map((c) => (
            <section key={c} className="kpi-categoria">
              <h3 className="secao">{c.toLowerCase()}</h3>
              <div className="kpis">
                {dados.kpis
                  .filter((k) => k.categoria === c)
                  .map((k) => (
                    <CartaoKpi key={k.numero} k={k} ano={dados.ano} />
                  ))}
              </div>
            </section>
          ))}
        </>
      )}
    </>
  );
}

// ---------- Dashboard: iniciativas por estado, próximas atividades e alertas ----------

const nomeAlerta: Record<string, string> = { validade: 'validade', atraso: 'atraso', risco: 'risco', vigilancia: 'vigilância' };

/** Resumo do dia a dia (também na homepage). */
export function PainelResumo({ dados }: { dados: Indicadores }) {
  const contagens = dados.iniciativasPorEstado;
  const tipos = [...new Set(contagens.map((c) => c.tipo))] as TipoIniciativa[];
  const estados = (Object.keys(nomeEstadoIniciativa) as EstadoIniciativa[]).filter((e) => contagens.some((c) => c.estado === e));
  const valor = (t: TipoIniciativa, e: EstadoIniciativa) => contagens.find((c) => c.tipo === t && c.estado === e)?.quantidade ?? 0;

  return (
    <div className="dashboard">
      <section className="dashboard-bloco dashboard-estados">
        <div className="dashboard-topo">
          <h3>iniciativas por estado</h3>
          <Link className="ligacao" to="/inovacao/iniciativas">
            portefólio
          </Link>
        </div>
        {tipos.length === 0 ? (
          <div className="vazio">Ainda não há iniciativas.</div>
        ) : (
          <div className="tabela-scroll">
            <table className="tabela-contagens">
              <thead>
                <tr>
                  <th />
                  {estados.map((e) => (
                    <th key={e}>{nomeEstadoIniciativa[e]}</th>
                  ))}
                  <th>total</th>
                </tr>
              </thead>
              <tbody>
                {tipos.map((t) => (
                  <tr key={t}>
                    <th>{nomeTipoIniciativa[t]}</th>
                    {estados.map((e) => (
                      <td key={e} className={valor(t, e) ? undefined : 'zero'}>
                        {valor(t, e) || '·'}
                      </td>
                    ))}
                    <td className="forte">{estados.reduce((s, e) => s + valor(t, e), 0)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
      <section className="dashboard-bloco">
        <div className="dashboard-topo">
          <h3>próximas atividades</h3>
          <Link className="ligacao" to="/inovacao/plano-anual">
            plano anual
          </Link>
        </div>
        {dados.proximasAtividades.length === 0 ? (
          <div className="vazio">Nada marcado nos próximos 30 dias.</div>
        ) : (
          <ul className="dashboard-lista">
            {dados.proximasAtividades.map((a) => (
              <li key={`${a.id}-${a.data}`}>
                <span className="ponto-cor" style={{ background: a.cor ?? '#c8102e' }} />
                <span className="num">{formatarData(a.data)}</span>
                <span>{a.titulo}</span>
              </li>
            ))}
          </ul>
        )}
      </section>
      <section className="dashboard-bloco">
        <div className="dashboard-topo">
          <h3>alertas</h3>
          {dados.alertas.length > 0 && <span className="cinzento-escuro">{dados.alertas.length}</span>}
        </div>
        {dados.alertas.length === 0 ? (
          <div className="vazio">Sem alertas.</div>
        ) : (
          <ul className="dashboard-lista">
            {dados.alertas.map((a, i) => (
              <li key={i}>
                <span className={`etiqueta alerta-${a.tipo}`}>
                  <span aria-hidden="true">!</span> {nomeAlerta[a.tipo] ?? a.tipo}
                </span>
                <Link className="dashboard-alerta" to={a.link}>
                  <span className="forte">{a.titulo}</span>
                  <span className="cinzento-escuro">{a.detalhe}</span>
                </Link>
              </li>
            ))}
          </ul>
        )}
      </section>
    </div>
  );
}

/** Painel da homepage: carrega o resumo do ano atual. */
export function PainelHomepage() {
  const { dados, erro, aCarregar } = usePedido(() => api.indicadores(new Date().getFullYear()), []);
  return (
    <section className="painel painel-avaliadas">
      <div className="painel-topo">
        <h2 className="painel-titulo">painel</h2>
        <Link to="/inovacao/indicadores" className="ligacao">
          ver indicadores
        </Link>
      </div>
      <EstadoPedido aCarregar={aCarregar} erro={erro} vazio={dados === undefined} />
      {dados && <PainelResumo dados={dados} />}
    </section>
  );
}
