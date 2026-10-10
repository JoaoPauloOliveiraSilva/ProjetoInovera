import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { notificacoes as api } from '../api/servicos';
import type { Notificacoes } from '../api/tipos';
import { useAtualizar, usePedido } from '../api/usePedido';
import { haQuantoTempo } from '../dados/regras';
import { Gaveta } from './Gaveta';
import { IconeSino } from './Icones';

/** Sino na barra da esquerda: avisos da plataforma (coautorias, ideias publicadas, decisões…). */
export function Avisos() {
  const { dados, recarregar } = usePedido<Notificacoes>(() => api.listar(), []);
  const atualizar = useAtualizar();
  const [aberto, setAberto] = useState(false);
  const navegar = useNavigate();

  // Volta a perguntar de minuto a minuto.
  useEffect(() => {
    const intervalo = setInterval(recarregar, 60_000);
    return () => clearInterval(intervalo);
  }, [recarregar]);

  const naoLidas = dados?.naoLidas ?? 0;

  const abrir = async (id: number, link: string | null, lida: boolean) => {
    if (!lida) {
      await api.marcarLida(id).catch(() => undefined);
      recarregar();
    }
    setAberto(false);
    if (link) {
      navegar(link);
      atualizar();
    }
  };

  return (
    <>
      <button className="barra-sino" title="Avisos" onClick={() => setAberto(true)} aria-label={`Avisos (${naoLidas} por ler)`}>
        <IconeSino />
        {naoLidas > 0 && <span className="barra-contador">{naoLidas > 9 ? '9+' : naoLidas}</span>}
        <span>avisos</span>
      </button>
      <Gaveta aberta={aberto} aoFechar={() => setAberto(false)}>
        <div className="sobretitulo">INOVAÇÃO</div>
        <div className="avisos-topo">
          <h2 className="detalhe-titulo">avisos</h2>
          {naoLidas > 0 && (
            <button
              className="ligacao-simples"
              onClick={() => {
                void api.marcarTodas().then(recarregar);
              }}
            >
              marcar todos como lidos
            </button>
          )}
        </div>
        <ul className="avisos">
          {dados?.itens.map((n) => (
            <li key={n.id} className={n.lida ? '' : 'por-ler'}>
              <button onClick={() => void abrir(n.id, n.link, n.lida)}>
                <strong>{n.titulo}</strong>
                <span>{n.mensagem}</span>
                <span className="cinzento-escuro">{haQuantoTempo(n.data)}</span>
              </button>
            </li>
          ))}
          {dados && dados.itens.length === 0 && <li className="vazio">Ainda não há avisos.</li>}
        </ul>
      </Gaveta>
    </>
  );
}
