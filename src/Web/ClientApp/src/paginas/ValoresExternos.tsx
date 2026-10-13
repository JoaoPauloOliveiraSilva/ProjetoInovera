import { kpis as api } from '../api/servicos';
import { nomePeriodoKpi, nomeTipoValorExterno } from '../api/tipos';
import type { ValorExterno } from '../api/tipos';
import { usePedido } from '../api/usePedido';
import { EstadoPedido } from '../componentes/Estado';
import { Cabecalho } from '../componentes/Layout';
import { TabelaEditavel, opcoes, texto } from '../componentes/TabelaEditavel';
import type { DefTabela } from '../componentes/TabelaEditavel';

const formatar = (v: ValorExterno) =>
  v.tipo === 'NumeroTrabalhadores'
    ? v.valor.toLocaleString('pt-PT')
    : v.valor.toLocaleString('pt-PT', { style: 'currency', currency: 'EUR', maximumFractionDigits: 0 });

const defValores: DefTabela<ValorExterno> = {
  titulo: 'valores externos',
  singular: 'valor',
  colunas: '80px minmax(200px, 1fr) 160px 160px minmax(160px, 1fr)',
  cabecalhos: ['Ano', 'Indicador', 'Período', 'Valor', 'Observações'],
  celulas: (v) => [
    <span className="num">{v.ano}</span>,
    <span className="forte">{nomeTipoValorExterno[v.tipo]}</span>,
    nomePeriodoKpi[v.periodo],
    formatar(v),
    v.observacoes ?? '—',
  ],
  campos: [
    { nome: 'tipo', rotulo: 'indicador', tipo: 'opcoes', opcoes: opcoes(nomeTipoValorExterno), obrigatorio: true },
    { nome: 'ano', rotulo: 'ano', tipo: 'numero', obrigatorio: true },
    { nome: 'periodo', rotulo: 'período', tipo: 'opcoes', opcoes: opcoes(nomePeriodoKpi), obrigatorio: true },
    { nome: 'valor', rotulo: 'valor', tipo: 'numero', obrigatorio: true },
    { nome: 'observacoes', rotulo: 'observações (ex.: fonte)', tipo: 'area', max: 1000 },
  ],
  valores: (v) => ({
    tipo: v?.tipo ?? 'NumeroTrabalhadores',
    ano: texto(v?.ano ?? new Date().getFullYear()),
    periodo: v?.periodo ?? 'Anual',
    valor: texto(v?.valor),
    observacoes: texto(v?.observacoes),
  }),
  vazio: 'Ainda não há valores. Comece pelo n.º de trabalhadores do ano.',
};

/** Valores que não existem na plataforma e entram à mão para os KPIs (só o Administrador, por omissão). */
export function ValoresExternos() {
  const { dados, erro, aCarregar, recarregar } = usePedido(() => api.valoresExternos(), []);
  return (
    <>
      <Cabecalho titulo="valores externos" corLampada="#3a6f9a" />
      <p className="cinzento-escuro charter-nota pagina-nota">
        Dados dos indicadores (Mod.239) que vêm de fora da plataforma: n.º de trabalhadores, receitas e proveitos de IDI.
      </p>
      <EstadoPedido aCarregar={aCarregar} erro={erro} vazio={dados === undefined} />
      {dados && (
        <TabelaEditavel
          def={defValores}
          itens={dados}
          podeEditar
          guardar={(id, corpo) => api.guardarValorExterno(id, corpo)}
          apagar={(id) => api.apagarValorExterno(id)}
          aoMudar={recarregar}
          sobretitulo="INDICADORES · VALORES EXTERNOS"
          semTitulo
        />
      )}
    </>
  );
}
