import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Permissoes } from '../api/permissoes';
import { iniciativas as api } from '../api/servicos';
import { classeEstadoIniciativa, nomeEstadoIniciativa, nomePilar, nomeTipoIniciativa } from '../api/tipos';
import type { EstadoIniciativa, PilarEstrategico, TipoIniciativa } from '../api/tipos';
import { usePedido } from '../api/usePedido';
import { EstadoPedido } from '../componentes/Estado';
import { Cabecalho } from '../componentes/Layout';
import { Mais, SetaDiagonal } from '../componentes/Icones';
import { formatarData } from '../dados/regras';
import { usePermissao } from '../sessao/Sessao';
import { IniciativaGaveta } from './IniciativaDetalhe';

/** Portefólio (Mod.246): todas as iniciativas ou só um tipo (projetos, desafios, vigilâncias, oportunidades). */
export function IniciativasLista({ titulo, tipo }: { titulo: string; tipo?: TipoIniciativa }) {
  const tem = usePermissao();
  const { id } = useParams();
  const navegar = useNavigate();
  const [aberta, setAberta] = useState<number | 'nova' | null>(id ? Number(id) : null);
  const [filtroTipo, setFiltroTipo] = useState<TipoIniciativa | ''>('');
  const [estado, setEstado] = useState<EstadoIniciativa | ''>('');
  const [pilar, setPilar] = useState<PilarEstrategico | ''>('');
  const [ano, setAno] = useState<number | ''>('');
  const anoAtual = new Date().getFullYear();
  const [texto, setTexto] = useState('');
  const [procura, setProcura] = useState('');

  useEffect(() => {
    if (id) setAberta(Number(id));
  }, [id]);

  // Só procura depois de parar de escrever.
  useEffect(() => {
    const t = setTimeout(() => setProcura(texto), 300);
    return () => clearTimeout(t);
  }, [texto]);

  const tipoEfetivo = tipo ?? (filtroTipo || undefined);
  const { dados, erro, aCarregar } = usePedido(
    () => api.listar({ tipo: tipoEfetivo, estado: estado || undefined, texto: procura, pilar: pilar || undefined, ano: ano || undefined }),
    [tipoEfetivo, estado, procura, pilar, ano],
  );
  const lista = dados ?? [];

  const fechar = () => {
    setAberta(null);
    if (id) navegar('/inovacao/iniciativas', { replace: true });
  };

  return (
    <>
      <Cabecalho
        titulo={titulo}
        corLampada="#7a9a3a"
        acao={
          tem(Permissoes.iniciativasGerir) ? (
            <button className="nova-ideia" onClick={() => setAberta('nova')}>
              <Mais /> nova iniciativa
            </button>
          ) : undefined
        }
      />
      <div className="filtros filtros-varios">
        {!tipo && (
          <label>
            tipo
            <select value={filtroTipo} onChange={(e) => setFiltroTipo(e.target.value as TipoIniciativa | '')}>
              <option value="">todos</option>
              {(Object.keys(nomeTipoIniciativa) as TipoIniciativa[]).map((t) => (
                <option key={t} value={t}>
                  {nomeTipoIniciativa[t]}
                </option>
              ))}
            </select>
          </label>
        )}
        <label>
          estado
          <select value={estado} onChange={(e) => setEstado(e.target.value as EstadoIniciativa | '')}>
            <option value="">todos</option>
            {(Object.keys(nomeEstadoIniciativa) as EstadoIniciativa[]).map((s) => (
              <option key={s} value={s}>
                {nomeEstadoIniciativa[s]}
              </option>
            ))}
          </select>
        </label>
        <label>
          pilar
          <select value={pilar} onChange={(e) => setPilar(e.target.value as PilarEstrategico | '')}>
            <option value="">todos</option>
            {(Object.keys(nomePilar) as PilarEstrategico[]).map((p) => (
              <option key={p} value={p}>
                {nomePilar[p]}
              </option>
            ))}
          </select>
        </label>
        <label title="Iniciativas com atividade nesse ano (entre a data de início e a de fim)">
          ativas em
          <select value={ano} onChange={(e) => setAno(e.target.value ? Number(e.target.value) : '')}>
            <option value="">qualquer ano</option>
            {[anoAtual + 1, anoAtual, anoAtual - 1, anoAtual - 2, anoAtual - 3].map((a) => (
              <option key={a} value={a}>
                {a}
              </option>
            ))}
          </select>
        </label>
        <label>
          procurar
          <input className="filtro-texto" value={texto} onChange={(e) => setTexto(e.target.value)} placeholder="título, código ou responsável" />
        </label>
        {dados && <span className="cinzento-escuro">{lista.length} iniciativas</span>}
      </div>
      <section className="tabela">
        <div className={`tabela-linha tabela-cabecalho iniciativa-linha${tipo ? '' : ' com-tipo'}`}>
          <span>Código</span>
          <span>Título</span>
          {!tipo && <span>Tipo</span>}
          <span>Responsável</span>
          <span>Início</span>
          <span>Estado</span>
          <span />
        </div>
        {lista.map((i) => (
          <div key={i.id} className={`tabela-linha iniciativa-linha${tipo ? '' : ' com-tipo'}`} onClick={() => setAberta(i.id)}>
            <span className="num">{i.codigo}</span>
            <span className="forte">{i.titulo}</span>
            {!tipo && <span>{nomeTipoIniciativa[i.tipo]}</span>}
            <span>{i.responsavel ?? '—'}</span>
            <span>{i.dataInicio ? formatarData(i.dataInicio) : '—'}</span>
            <span className={`etiqueta ${classeEstadoIniciativa[i.estado]}`}>{nomeEstadoIniciativa[i.estado]}</span>
            <span className="abrir" aria-label="Abrir">
              <SetaDiagonal />
            </span>
          </div>
        ))}
        <EstadoPedido aCarregar={aCarregar} erro={erro} vazio={dados === undefined} />
        {dados && lista.length === 0 && (
          <div className="vazio">
            Não há iniciativas para mostrar.{' '}
            {tem(Permissoes.iniciativasGerir) && 'Pode importá-las do Excel Mod.246 em administração › importar Mod.246.'}
          </div>
        )}
      </section>
      <IniciativaGaveta id={aberta} tipoNova={tipo} aoFechar={fechar} />
    </>
  );
}
