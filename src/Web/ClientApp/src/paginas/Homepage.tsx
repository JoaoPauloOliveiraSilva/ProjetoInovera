import { useState } from 'react';
import { Link } from 'react-router-dom';
import { Cabecalho } from '../componentes/Layout';
import { Balao, Coracao, Direita, Esquerda, Mais } from '../componentes/Icones';
import { ideias as apiIdeias } from '../api/servicos';
import { usePedido } from '../api/usePedido';
import { EstadoPedido } from '../componentes/Estado';
import { NovaIdeiaGaveta } from './NovaIdeia';
import { IdeiaGaveta } from './IdeiaDetalhe';

const porPagina = 3;

export function Homepage() {
  const { dados, erro, aCarregar } = usePedido(() => apiIdeias.listar({ estado: 'EmDiscussao' }), []);
  const [inicio, setInicio] = useState(0);
  const [novaAberta, setNovaAberta] = useState(false);
  const [aberta, setAberta] = useState<number | null>(null);

  const emDiscussao = dados ?? [];
  const visiveis = emDiscussao.slice(inicio, inicio + porPagina);

  return (
    <>
      <Cabecalho
        titulo="homepage"
        acao={
          <button className="nova-ideia" onClick={() => setNovaAberta(true)}>
            <Mais /> nova ideia
          </button>
        }
      />
      <section className="painel">
        <div className="painel-topo">
          <h2 className="painel-titulo">ideias</h2>
          <Link to="/inovacao/ideias/discussao" className="ligacao">
            ver todas em discussão
          </Link>
        </div>
        <div className="setas">
          <button className="seta" onClick={() => setInicio(Math.max(0, inicio - porPagina))} disabled={inicio === 0} aria-label="Anteriores">
            <Esquerda />
          </button>
          <button
            className="seta"
            onClick={() => setInicio(inicio + porPagina)}
            disabled={inicio + porPagina >= emDiscussao.length}
            aria-label="Seguintes"
          >
            <Direita />
          </button>
        </div>
        <EstadoPedido aCarregar={aCarregar} erro={erro} vazio={dados === undefined} />
        {dados && emDiscussao.length === 0 && <div className="vazio">Não há ideias em discussão.</div>}
        <div className="cartoes">
          {visiveis.map((i) => (
            <button key={i.id} className="cartao-ideia" onClick={() => setAberta(i.id)}>
              <span className="cartao-titulo">{i.titulo}</span>
              <span className="cartao-contadores">
                <span>
                  <Coracao /> {i.likes}
                </span>
                <span>
                  <Balao /> {i.comentarios}
                </span>
              </span>
            </button>
          ))}
        </div>
      </section>
      <NovaIdeiaGaveta aberta={novaAberta} aoFechar={() => setNovaAberta(false)} />
      <IdeiaGaveta id={aberta} aoFechar={() => setAberta(null)} />
    </>
  );
}
