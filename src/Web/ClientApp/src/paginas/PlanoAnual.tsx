import { useState } from 'react';
import { Permissoes } from '../api/permissoes';
import { planoAnual as api, urlExportacao } from '../api/servicos';
import { nomePeriodicidade } from '../api/tipos';
import type { AtividadePlano } from '../api/tipos';
import { usePedido } from '../api/usePedido';
import { EstadoPedido } from '../componentes/Estado';
import { Cabecalho } from '../componentes/Layout';
import { TabelaEditavel, data, hoje, opcoes, pessoas, texto } from '../componentes/TabelaEditavel';
import type { DefTabela } from '../componentes/TabelaEditavel';
import { usePermissao } from '../sessao/Sessao';

const meses = ['jan', 'fev', 'mar', 'abr', 'mai', 'jun', 'jul', 'ago', 'set', 'out', 'nov', 'dez'];
const corPorOmissao = '#c8102e';

const defAtividades: DefTabela<AtividadePlano> = {
  titulo: 'atividades',
  singular: 'atividade',
  feminino: true,
  colunas: '24px minmax(220px, 1fr) 130px 130px 130px minmax(160px, 0.6fr)',
  cabecalhos: ['', 'Atividade', 'Início', 'Repete-se', 'Próxima', 'Responsáveis'],
  celulas: (a) => [
    <span className="ponto-cor" style={{ background: a.cor ?? corPorOmissao }} />,
    <span className="forte">{a.titulo}</span>,
    data(a.dataInicio),
    nomePeriodicidade[a.recorrencia],
    data(a.proximaOcorrencia),
    a.responsaveis.length > 0 ? a.responsaveis.map((r) => r.nome).join(', ') : <span className="cinzento-escuro">equipa de inovação</span>,
  ],
  campos: [
    { nome: 'titulo', rotulo: 'atividade (ex.: avaliação de ideias com a CE)', obrigatorio: true, max: 200 },
    { nome: 'descricao', rotulo: 'descrição', tipo: 'area' },
    { nome: 'dataInicio', rotulo: 'data (primeira vez)', tipo: 'data', obrigatorio: true },
    { nome: 'dataFim', rotulo: 'até (se durar vários dias)', tipo: 'data' },
    { nome: 'recorrencia', rotulo: 'repete-se', tipo: 'opcoes', opcoes: opcoes(nomePeriodicidade), obrigatorio: true },
    {
      nome: 'diasAntecedenciaAviso',
      rotulo: 'avisar com quantos dias de antecedência',
      tipo: 'numero',
      obrigatorio: true,
      ajuda: 'Os responsáveis recebem um aviso na plataforma e por email. Sem responsáveis, avisa-se quem gere o plano.',
    },
    { nome: 'responsaveisIds', rotulo: 'responsáveis', tipo: 'pessoas' },
    { nome: 'cor', rotulo: 'cor no calendário', tipo: 'cor' },
  ],
  valores: (a) => ({
    titulo: texto(a?.titulo),
    descricao: texto(a?.descricao),
    dataInicio: a ? a.dataInicio : hoje(),
    dataFim: texto(a?.dataFim),
    recorrencia: a?.recorrencia ?? 'Nenhuma',
    diasAntecedenciaAviso: texto(a?.diasAntecedenciaAviso ?? 7),
    responsaveisIds: pessoas(a?.responsaveis ?? []),
    cor: a?.cor ?? corPorOmissao,
  }),
  vazio: 'Ainda não há atividades neste ano.',
};

/** Plano anual de atividades (Mod.69): calendário do ano e lista para editar. */
export function PlanoAnualPagina() {
  const tem = usePermissao();
  const [ano, setAno] = useState(new Date().getFullYear());
  const { dados, erro, aCarregar, recarregar } = usePedido(() => api.listar(ano), [ano]);

  return (
    <>
      <Cabecalho titulo="plano anual" corLampada="#7a5aa0" exportar={urlExportacao('mod69', ano)} />
      <div className="filtros filtros-varios">
        <label>
          ano
          <select value={ano} onChange={(e) => setAno(Number(e.target.value))}>
            {[ano - 1, ano, ano + 1].map((a) => (
              <option key={a} value={a}>
                {a}
              </option>
            ))}
          </select>
        </label>
        <span className="cinzento-escuro">Planeamento anual de atividades (Mod.69). A plataforma avisa os responsáveis antes de cada atividade.</span>
      </div>
      <EstadoPedido aCarregar={aCarregar} erro={erro} vazio={dados === undefined} />
      {dados && (
        <>
          <Calendario ano={ano} atividades={dados} />
          <TabelaEditavel
            def={defAtividades}
            itens={dados}
            podeEditar={tem(Permissoes.gerirPlano)}
            guardar={(id, corpo) => api.guardar(id, corpo)}
            apagar={(id) => api.apagar(id)}
            aoMudar={recarregar}
            sobretitulo="INOVAÇÃO · PLANO ANUAL"
          />
        </>
      )}
    </>
  );
}

/** Uma linha por atividade e uma coluna por mês, com os dias de cada ocorrência na cor da atividade. */
function Calendario({ ano, atividades }: { ano: number; atividades: AtividadePlano[] }) {
  const agora = new Date();
  const mesAtual = agora.getFullYear() === ano ? agora.getMonth() : -1;

  const diasNoMes = (a: AtividadePlano, mes: number) =>
    a.ocorrencias
      .map((o) => new Date(`${o}T00:00:00`))
      .filter((d) => d.getFullYear() === ano && d.getMonth() === mes)
      .map((d) => {
        if (a.duracaoDias <= 0) return String(d.getDate());
        const fim = new Date(d);
        fim.setDate(fim.getDate() + a.duracaoDias);
        return fim.getMonth() === d.getMonth() ? `${d.getDate()}–${fim.getDate()}` : `${d.getDate()}–${fim.getDate()}/${fim.getMonth() + 1}`;
      });

  if (atividades.length === 0) return null;

  return (
    <section className="charter-seccao">
      <h3 className="secao">calendário {ano}</h3>
      <div className="calendario">
        <div className="calendario-linha calendario-cabecalho">
          <span />
          {meses.map((m, i) => (
            <span key={m} className={i === mesAtual ? 'mes-atual' : undefined}>
              {m}
            </span>
          ))}
        </div>
        {atividades.map((a) => (
          <div key={a.id} className="calendario-linha">
            <span className="calendario-titulo" title={a.descricao ?? a.titulo}>
              {a.titulo}
            </span>
            {meses.map((m, i) => {
              const dias = diasNoMes(a, i);
              return (
                <span key={m} className={i === mesAtual ? 'mes-atual' : undefined}>
                  {dias.map((d) => (
                    <span key={d} className="calendario-dia" style={{ background: a.cor ?? corPorOmissao }}>
                      {d}
                    </span>
                  ))}
                </span>
              );
            })}
          </div>
        ))}
      </div>
    </section>
  );
}
