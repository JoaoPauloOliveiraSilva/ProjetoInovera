import { useState } from 'react';
import { ideias as apiIdeias } from '../api/servicos';
import { nomeTipo } from '../api/tipos';
import { useAtualizar, usePedido } from '../api/usePedido';
import { EstadoPedido } from '../componentes/Estado';
import { Cabecalho } from '../componentes/Layout';
import { formatarData } from '../dados/regras';
import { IdeiaGaveta } from './IdeiaDetalhe';

/**
 * Fila da equipa de Inovação: ideias submetidas (e com a autoria confirmada) à espera de validação.
 * Ao validar, a ideia fica pública em discussão durante 30 dias.
 */
export function Validacao() {
  const atualizar = useAtualizar();
  const { dados, erro, aCarregar } = usePedido(() => apiIdeias.listar({ estado: 'ValidacaoEquipa' }), []);
  const [aberta, setAberta] = useState<number | null>(null);
  const [aValidar, setAValidar] = useState<number | null>(null);
  const [erroAcao, setErroAcao] = useState('');
  const lista = dados ?? [];

  const validar = async (id: number) => {
    setErroAcao('');
    setAValidar(id);
    try {
      await apiIdeias.validar(id);
      atualizar();
    } catch (e) {
      setErroAcao(e instanceof Error ? e.message : String(e));
    } finally {
      setAValidar(null);
    }
  };

  return (
    <>
      <Cabecalho titulo="validação" corLampada="#5bb8cc" />
      <p className="introducao">
        Ideias à espera da validação da equipa de Inovação. Abra a ideia para a ler; ao validar fica em discussão pública durante 30
        dias.
      </p>
      {erroAcao && <div className="erro">{erroAcao}</div>}
      <section className="tabela">
        <div className="tabela-linha tabela-cabecalho validacao-linha">
          <span>Código</span>
          <span>Título</span>
          <span>Autores</span>
          <span>Data</span>
          <span>Tipo</span>
          <span />
        </div>
        {lista.map((i) => (
          <div key={i.id} className="tabela-linha validacao-linha" onClick={() => setAberta(i.id)}>
            <span className="num">{i.codigo}</span>
            <span className="forte">
              {i.titulo}
              {i.privada && <span className="privada"> · privada</span>}
            </span>
            <span className="forte">{i.autores}</span>
            <span className="forte">{formatarData(i.data)}</span>
            <span>{nomeTipo[i.tipo]}</span>
            <span>
              <button
                className="botao-pequeno"
                disabled={aValidar !== null}
                onClick={(e) => {
                  e.stopPropagation();
                  void validar(i.id);
                }}
              >
                {aValidar === i.id ? 'a validar…' : 'validar'}
              </button>
            </span>
          </div>
        ))}
        <EstadoPedido aCarregar={aCarregar} erro={erro} vazio={dados === undefined} />
        {dados && lista.length === 0 && <div className="vazio">Não há ideias à espera de validação.</div>}
      </section>
      <IdeiaGaveta id={aberta} aoFechar={() => setAberta(null)} />
    </>
  );
}
