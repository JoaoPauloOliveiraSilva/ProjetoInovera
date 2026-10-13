import { useState } from 'react';
import type { FormEvent } from 'react';
import { Permissoes } from '../api/permissoes';
import { iniciativas as api } from '../api/servicos';
import { nomePeriodicidade } from '../api/tipos';
import type { VigilanciaPainel } from '../api/tipos';
import { usePedido } from '../api/usePedido';
import { EstadoPedido } from '../componentes/Estado';
import { Mais } from '../componentes/Icones';
import { Cabecalho } from '../componentes/Layout';
import { hoje } from '../componentes/TabelaEditavel';
import { formatarData, formatarTamanho } from '../dados/regras';
import { usePermissao } from '../sessao/Sessao';
import { IniciativaGaveta } from './IniciativaDetalhe';

/**
 * Página das vigilâncias (pedido da dstelecom a 13/10): cada vigilância com a informação e os relatórios gerados pela
 * tarefa automática. Carregar um relatório regista a execução e põe-no no conhecimento codificado (Mod.237).
 */
export function VigilanciasPagina() {
  const tem = usePermissao();
  const podeGerir = tem(Permissoes.iniciativasGerir);
  const [retiradas, setRetiradas] = useState(false);
  const [aberta, setAberta] = useState<number | 'nova' | null>(null);
  const { dados, erro, aCarregar, recarregar } = usePedido(() => api.vigilancias(retiradas), [retiradas]);
  const lista = dados ?? [];

  return (
    <>
      <Cabecalho
        titulo="vigilâncias"
        corLampada="#7a9a3a"
        acao={
          podeGerir ? (
            <button className="nova-ideia" onClick={() => setAberta('nova')}>
              <Mais /> nova vigilância
            </button>
          ) : undefined
        }
      />
      <div className="filtros filtros-varios">
        <label className="caixa-simples">
          <input type="checkbox" checked={retiradas} onChange={(e) => setRetiradas(e.target.checked)} />
          <span>mostrar também as retiradas</span>
        </label>
        {dados && (
          <span className="cinzento-escuro">
            {lista.filter((v) => v.ativa).length} vigilâncias ativas · cada relatório carregado entra no conhecimento codificado (Mod.237)
          </span>
        )}
      </div>
      <EstadoPedido aCarregar={aCarregar} erro={erro} vazio={dados === undefined} />
      {dados && lista.length === 0 && <div className="vazio">Ainda não há vigilâncias.</div>}
      <div className="vigilancias">
        {lista.map((v) => (
          <CartaoVigilancia key={v.id} v={v} podeGerir={podeGerir} aoMudar={recarregar} aoAbrir={() => setAberta(v.id)} />
        ))}
      </div>
      <IniciativaGaveta
        id={aberta}
        tipoNova="Vigilancia"
        aoFechar={() => {
          setAberta(null);
          recarregar();
        }}
      />
    </>
  );
}

function CartaoVigilancia({
  v,
  podeGerir,
  aoMudar,
  aoAbrir,
}: {
  v: VigilanciaPainel;
  podeGerir: boolean;
  aoMudar: () => void;
  aoAbrir: () => void;
}) {
  const [aCarregarRelatorio, setACarregarRelatorio] = useState(false);
  const [erro, setErro] = useState('');
  const relatorios = v.execucoes.filter((e) => e.dataRealizada);
  const atrasada = v.execucoes.some((e) => e.atrasada);

  const alternar = async () => {
    setErro('');
    try {
      await api.alterarVigilanciaAtiva(v.id, !v.ativa);
      aoMudar();
    } catch (falha) {
      setErro(falha instanceof Error ? falha.message : String(falha));
    }
  };

  return (
    <article className={`vigilancia${v.ativa ? '' : ' retirada'}`}>
      <div className="vigilancia-topo">
        <div>
          <span className="num">{v.codigo}</span>
          <h3>{v.titulo}</h3>
        </div>
        <button className="ligacao-simples" onClick={aoAbrir}>
          ficha
        </button>
      </div>
      <div className="vigilancia-dados">
        <span>{nomePeriodicidade[v.periodicidade]}</span>
        {v.responsavel && <span>responsável: {v.responsavel}</span>}
        {v.ativa ? (
          v.proximaExecucao && (
            <span className={atrasada ? 'etiqueta estado-em-discussao' : undefined}>
              {atrasada ? 'atrasada desde ' : 'próximo relatório: '}
              {formatarData(v.proximaExecucao)}
            </span>
          )
        ) : (
          <span className="etiqueta estado-nao-aprovada">retirada</span>
        )}
      </div>
      {v.descritivo && <p className="texto">{v.descritivo}</p>}

      <ul className="relatorios" aria-label={`Relatórios de ${v.titulo}`}>
        {relatorios.length === 0 && <li className="cinzento-escuro">Ainda sem relatórios.</li>}
        {relatorios.map((e) => (
          <li key={e.id}>
            <span className="num">{formatarData(e.dataRealizada!)}</span>
            <span className="resultado" title={e.resultado ?? undefined}>
              {e.resultado}
            </span>
            {e.linkResultado ? (
              <a className="ligacao" href={e.linkResultado} target={e.nomeFicheiro ? undefined : '_blank'} rel="noreferrer" download={e.nomeFicheiro ?? undefined}>
                {e.nomeFicheiro ? `${e.nomeFicheiro}${e.tamanhoBytes ? ` (${formatarTamanho(e.tamanhoBytes)})` : ''}` : 'abrir ↗'}
              </a>
            ) : (
              <span />
            )}
          </li>
        ))}
      </ul>

      {podeGerir && v.ativa && !aCarregarRelatorio && (
        <div className="botoes-linha" style={{ justifyContent: 'space-between' }}>
          <button className="ligacao-simples cinzento-escuro" onClick={() => void alternar()}>
            retirar
          </button>
          <button className="botao-pequeno" onClick={() => setACarregarRelatorio(true)}>
            carregar relatório
          </button>
        </div>
      )}
      {podeGerir && !v.ativa && (
        <button className="ligacao-simples" onClick={() => void alternar()}>
          voltar a ativar
        </button>
      )}
      {aCarregarRelatorio && (
        <CarregarRelatorio
          vigilanciaId={v.id}
          aoTerminar={(mudou) => {
            setACarregarRelatorio(false);
            if (mudou) aoMudar();
          }}
        />
      )}
      {erro && <div className="erro">{erro}</div>}
    </article>
  );
}

const tamanhoMaximo = 25 * 1024 * 1024;

function CarregarRelatorio({ vigilanciaId, aoTerminar }: { vigilanciaId: number; aoTerminar: (mudou: boolean) => void }) {
  const [ficheiro, setFicheiro] = useState<File | null>(null);
  const [descricao, setDescricao] = useState('');
  const [data, setData] = useState(hoje());
  const [erro, setErro] = useState('');
  const [aEnviar, setAEnviar] = useState(false);

  const submeter = async (e: FormEvent) => {
    e.preventDefault();
    if (!ficheiro) return;
    if (ficheiro.size > tamanhoMaximo) {
      setErro('O relatório tem de ter no máximo 25 MB.');
      return;
    }

    setErro('');
    setAEnviar(true);
    try {
      await api.carregarRelatorio(vigilanciaId, ficheiro, descricao, data);
      aoTerminar(true);
    } catch (falha) {
      setErro(falha instanceof Error ? falha.message : String(falha));
      setAEnviar(false);
    }
  };

  return (
    <form className="carregar-relatorio" onSubmit={(e) => void submeter(e)}>
      <input type="file" aria-label="Relatório" required onChange={(e) => setFicheiro(e.target.files?.[0] ?? null)} />
      <input
        value={descricao}
        maxLength={4000}
        placeholder="resumo do resultado (opcional)"
        aria-label="Resumo do resultado"
        onChange={(e) => setDescricao(e.target.value)}
      />
      <input type="date" value={data} required aria-label="Data do relatório" onChange={(e) => setData(e.target.value)} />
      {erro && <div className="erro largo">{erro}</div>}
      <div className="botoes-linha">
        <button type="button" className="ligacao-simples" onClick={() => aoTerminar(false)}>
          cancelar
        </button>
        <button type="submit" className="botao-pequeno" disabled={aEnviar || !ficheiro}>
          {aEnviar ? 'a carregar…' : 'carregar'}
        </button>
      </div>
    </form>
  );
}
