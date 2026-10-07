import { useState } from 'react';
import { Cabecalho } from '../componentes/Layout';
import { Balao, Coracao, Mais, SetaDiagonal } from '../componentes/Icones';
import { useIdeias } from '../dados/IdeiasContext';
import { formatarData } from '../dados/regras';
import { nomeEstado } from '../dados/tipos';
import type { EstadoIdeia } from '../dados/tipos';
import { NovaIdeiaGaveta } from './NovaIdeia';
import { IdeiaGaveta } from './IdeiaDetalhe';

/** "ideias de inovação › em discussão" e "› todas as ideias". */
export function IdeiasLista({ soEmDiscussao }: { soEmDiscussao: boolean }) {
  const { ideias } = useIdeias();
  const [aberta, setAberta] = useState<number | null>(null);
  const [novaAberta, setNovaAberta] = useState(false);
  const [filtro, setFiltro] = useState<EstadoIdeia | 'todas'>('todas');

  const lista = ideias
    .filter((i) => (soEmDiscussao ? i.estado === 'em-discussao' : filtro === 'todas' || i.estado === filtro))
    .sort((a, b) => b.num - a.num);

  return (
    <>
      <Cabecalho
        titulo="ideias"
        corLampada="#5bb8cc"
        acao={
          <button className="nova-ideia" onClick={() => setNovaAberta(true)}>
            <Mais /> nova ideia
          </button>
        }
      />
      {!soEmDiscussao && (
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
        <div className={`tabela-linha tabela-cabecalho${soEmDiscussao ? '' : ' com-estado'}`}>
          <span>Num</span>
          <span>Título</span>
          <span>Autores</span>
          <span>Data</span>
          {!soEmDiscussao && <span>Estado</span>}
          <span />
          <span />
          <span />
        </div>
        {lista.map((i) => (
          <div key={i.num} className={`tabela-linha${soEmDiscussao ? '' : ' com-estado'}`} onClick={() => setAberta(i.num)}>
            <span className="num">{i.num}</span>
            <span className="forte">{i.titulo}</span>
            <span className="forte">{i.anonima ? 'Autor Anónimo' : i.autores.join(', ')}</span>
            <span className="forte">{formatarData(i.data)}</span>
            {!soEmDiscussao && <span className={`etiqueta estado-${i.estado}`}>{nomeEstado[i.estado]}</span>}
            <span className="contador">
              <Coracao /> {i.likes}
            </span>
            <span className="contador">
              <Balao /> {i.comentarios.length}
            </span>
            <span className="abrir" aria-label="Abrir">
              <SetaDiagonal />
            </span>
          </div>
        ))}
        {lista.length === 0 && <div className="vazio">Não há ideias para mostrar.</div>}
      </section>
      <IdeiaGaveta num={aberta} aoFechar={() => setAberta(null)} />
      <NovaIdeiaGaveta aberta={novaAberta} aoFechar={() => setNovaAberta(false)} />
    </>
  );
}
