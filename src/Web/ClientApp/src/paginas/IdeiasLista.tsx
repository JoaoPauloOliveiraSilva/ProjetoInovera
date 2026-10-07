import { useState } from 'react';
import { ideias as apiIdeias } from '../api/servicos';
import { nomeEstado, classeEstado } from '../api/tipos';
import type { EstadoIdeia } from '../api/tipos';
import { usePedido } from '../api/usePedido';
import { EstadoPedido } from '../componentes/Estado';
import { Cabecalho } from '../componentes/Layout';
import { Balao, Coracao, Mais, SetaDiagonal } from '../componentes/Icones';
import { formatarData } from '../dados/regras';
import { NovaIdeiaGaveta } from './NovaIdeia';
import { IdeiaGaveta } from './IdeiaDetalhe';

type Modo = 'discussao' | 'todas' | 'minhas';

const titulos: Record<Modo, string> = { discussao: 'ideias', todas: 'ideias', minhas: 'as minhas ideias' };

/** "ideias de inovação › em discussão", "› todas as ideias" e "a minha caixa › as minhas ideias". */
export function IdeiasLista({ modo }: { modo: Modo }) {
  const [aberta, setAberta] = useState<number | null>(null);
  const [novaAberta, setNovaAberta] = useState(false);
  const [filtro, setFiltro] = useState<EstadoIdeia | 'todas'>('todas');
  const comEstado = modo !== 'discussao';

  const estado: EstadoIdeia | undefined = modo === 'discussao' ? 'EmDiscussao' : filtro === 'todas' ? undefined : filtro;
  const { dados, erro, aCarregar } = usePedido(() => apiIdeias.listar({ estado, soMinhas: modo === 'minhas' }), [estado, modo]);
  const lista = dados ?? [];

  return (
    <>
      <Cabecalho
        titulo={titulos[modo]}
        corLampada="#5bb8cc"
        acao={
          <button className="nova-ideia" onClick={() => setNovaAberta(true)}>
            <Mais /> nova ideia
          </button>
        }
      />
      {comEstado && (
        <div className="filtros">
          <label>
            estado
            <select value={filtro} onChange={(e) => setFiltro(e.target.value as EstadoIdeia | 'todas')}>
              <option value="todas">todos</option>
              {(Object.keys(nomeEstado) as EstadoIdeia[]).map((e) => (
                <option key={e} value={e}>
                  {nomeEstado[e]}
                </option>
              ))}
            </select>
          </label>
        </div>
      )}
      <section className="tabela">
        <div className={`tabela-linha tabela-cabecalho${comEstado ? ' com-estado' : ''}`}>
          <span>Código</span>
          <span>Título</span>
          <span>Autores</span>
          <span>Data</span>
          {comEstado && <span>Estado</span>}
          <span />
          <span />
          <span />
        </div>
        {lista.map((i) => (
          <div key={i.id} className={`tabela-linha${comEstado ? ' com-estado' : ''}`} onClick={() => setAberta(i.id)}>
            <span className="num">{i.codigo}</span>
            <span className="forte">
              {i.titulo}
              {i.privada && <span className="privada"> · privada</span>}
            </span>
            <span className="forte">{i.autores}</span>
            <span className="forte">{formatarData(i.data)}</span>
            {comEstado && <span className={`etiqueta ${classeEstado[i.estado]}`}>{nomeEstado[i.estado]}</span>}
            <span className="contador">
              <Coracao /> {i.likes}
            </span>
            <span className="contador">
              <Balao /> {i.comentarios}
            </span>
            <span className="abrir" aria-label="Abrir">
              <SetaDiagonal />
            </span>
          </div>
        ))}
        <EstadoPedido aCarregar={aCarregar} erro={erro} vazio={dados === undefined} />
        {dados && lista.length === 0 && <div className="vazio">Não há ideias para mostrar.</div>}
      </section>
      <IdeiaGaveta id={aberta} aoFechar={() => setAberta(null)} />
      <NovaIdeiaGaveta aberta={novaAberta} aoFechar={() => setNovaAberta(false)} />
    </>
  );
}
