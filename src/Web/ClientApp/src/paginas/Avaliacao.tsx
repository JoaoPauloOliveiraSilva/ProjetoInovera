import { useState } from 'react';
import { ideias as apiIdeias } from '../api/servicos';
import { classesAprovada, classesNaoAprovada, nomeClasse } from '../api/tipos';
import type { ClasseIdeia, IdeiaResumo, Responsabilidade } from '../api/tipos';
import { useAtualizar, usePedido } from '../api/usePedido';
import { EstadoPedido } from '../componentes/Estado';
import { Cabecalho } from '../componentes/Layout';
import { calcularNota, formatarData, limiarAprovacao, variaveis } from '../dados/regras';
import type { Notas } from '../dados/regras';
import { IdeiaGaveta } from './IdeiaDetalhe';

const semNotas: Notas = { custo: null, enquadramento: null, beneficio: null, adequacaoTecnica: null, incerteza: null };

interface Rascunho {
  responsabilidade: Responsabilidade;
  notas: Notas;
  decisaoDst: boolean | null;
  classe: ClasseIdeia | null;
}

/**
 * Página de avaliação: junta todas as ideias que terminaram a discussão ("em avaliação pelo manager")
 * para serem analisadas ao mesmo tempo na reunião mensal com a Comissão Executiva (CE).
 */
export function Avaliacao() {
  const atualizar = useAtualizar();
  const { dados, erro, aCarregar } = usePedido(() => apiIdeias.listar({ estado: 'EmAvaliacao' }), []);
  const [rascunhos, setRascunhos] = useState<Record<number, Rascunho>>({});
  const [dataReuniao, setDataReuniao] = useState('');
  const [aRegistar, setARegistar] = useState<number | null>(null);
  const [erros, setErros] = useState<Record<number, string>>({});
  const [aberta, setAberta] = useState<number | null>(null);
  const porAvaliar = dados ?? [];

  const rascunho = (i: IdeiaResumo): Rascunho =>
    rascunhos[i.id] ?? { responsabilidade: i.responsabilidade, notas: semNotas, decisaoDst: null, classe: null };

  const mudar = (i: IdeiaResumo, alteracao: Partial<Rascunho>) =>
    setRascunhos((r) => ({ ...r, [i.id]: { ...rascunho(i), ...alteracao } }));

  const registar = async (i: IdeiaResumo, r: Rascunho) => {
    if (r.classe === null) return;
    setARegistar(i.id);
    setErros((e) => ({ ...e, [i.id]: '' }));
    try {
      if (r.responsabilidade === 'Dst') {
        await apiIdeias.registarDecisaoDst(i.id, r.decisaoDst === true);
      } else {
        const n = r.notas;
        await apiIdeias.registarAvaliacao(i.id, {
          custo: n.custo ?? 0,
          enquadramento: n.enquadramento ?? 0,
          beneficio: n.beneficio ?? 0,
          adequacaoTecnica: n.adequacaoTecnica ?? 0,
          incerteza: n.incerteza ?? 0,
          dataReuniaoCE: dataReuniao || null,
        });
      }
      await apiIdeias.definirClasse(i.id, r.classe);
      setRascunhos((todos) => {
        const { [i.id]: _registado, ...resto } = todos;
        return resto;
      });
      atualizar();
    } catch (e) {
      setErros((todos) => ({ ...todos, [i.id]: e instanceof Error ? e.message : String(e) }));
    } finally {
      setARegistar(null);
    }
  };

  return (
    <>
      <Cabecalho titulo="avaliação" corLampada="#e3b341" />
      <p className="introducao">
        Ideias com a discussão terminada. Notas de 0 a 4 (1 = caro/mau, 4 = bom). Nota final = 15% custo + 25% enquadramento + 30%
        benefício + 20% adequação técnica + 10% incerteza; com {limiarAprovacao} ou mais fica aprovada. As ideias da dst só registam a
        decisão.
      </p>
      <div className="filtros">
        <label>
          data da reunião com a CE
          <input type="date" value={dataReuniao} onChange={(e) => setDataReuniao(e.target.value)} />
        </label>
      </div>
      <section className="tabela avaliacao">
        <div className="aval-linha tabela-cabecalho">
          <span>Código</span>
          <span>Título</span>
          <span>Resp.</span>
          {variaveis.map((v) => (
            <span key={v.chave} title={v.nome}>
              {v.curto}
            </span>
          ))}
          <span>TOTAL</span>
          <span>Classificação</span>
          <span>Classe</span>
          <span />
        </div>
        {porAvaliar.map((i) => {
          const r = rascunho(i);
          const ehDst = r.responsabilidade === 'Dst';
          const nota = ehDst ? null : calcularNota(r.notas);
          const aprovada = ehDst ? r.decisaoDst : nota === null ? null : nota >= limiarAprovacao;
          const classes: ClasseIdeia[] = aprovada === null ? [] : aprovada ? classesAprovada : classesNaoAprovada;
          const completa = aprovada !== null && r.classe !== null && classes.includes(r.classe);

          return (
            <div key={i.id} className="aval-bloco">
              <div className="aval-linha">
                <button className="num ligacao-simples" onClick={() => setAberta(i.id)} title="Abrir a ideia">
                  {i.codigo}
                </button>
                <span className="forte" title={`${formatarData(i.data)} · ${i.likes} gostos · ${i.comentarios} comentários`}>
                  {i.titulo}
                </span>
                <span>
                  <select
                    className={`resp resp-${r.responsabilidade.toLowerCase()}`}
                    value={r.responsabilidade}
                    onChange={(e) => mudar(i, { responsabilidade: e.target.value as Responsabilidade, classe: null })}
                  >
                    <option value="Dstelecom">dstelecom</option>
                    <option value="Dst">dst</option>
                  </select>
                </span>
                {variaveis.map((v) => (
                  <span key={v.chave}>
                    {ehDst ? (
                      <span className="na">N.A.</span>
                    ) : (
                      <select
                        value={r.notas[v.chave] ?? ''}
                        aria-label={v.nome}
                        onChange={(e) => {
                          const valor = e.target.value === '' ? null : Number(e.target.value);
                          mudar(i, { notas: { ...r.notas, [v.chave]: valor }, classe: null });
                        }}
                      >
                        <option value="">–</option>
                        {[0, 1, 2, 3, 4].map((n) => (
                          <option key={n} value={n}>
                            {n}
                          </option>
                        ))}
                      </select>
                    )}
                  </span>
                ))}
                <span className="total">{ehDst ? 'N.A.' : nota === null ? '' : nota.toFixed(2)}</span>
                <span>
                  {ehDst ? (
                    <select
                      value={r.decisaoDst === null ? '' : r.decisaoDst ? 'sim' : 'nao'}
                      onChange={(e) => mudar(i, { decisaoDst: e.target.value === '' ? null : e.target.value === 'sim', classe: null })}
                    >
                      <option value="">Em avaliação</option>
                      <option value="sim">Aprovada</option>
                      <option value="nao">Não Aprovada</option>
                    </select>
                  ) : (
                    <span className={`etiqueta ${aprovada === null ? '' : aprovada ? 'estado-aprovada' : 'estado-nao-aprovada'}`}>
                      {aprovada === null ? 'Em avaliação' : aprovada ? 'Aprovada' : 'Não Aprovada'}
                    </span>
                  )}
                </span>
                <span>
                  <select
                    value={r.classe ?? ''}
                    disabled={classes.length === 0}
                    onChange={(e) => mudar(i, { classe: e.target.value === '' ? null : (e.target.value as ClasseIdeia) })}
                  >
                    <option value="">–</option>
                    {classes.map((c) => (
                      <option key={c} value={c}>
                        {nomeClasse[c]}
                      </option>
                    ))}
                  </select>
                </span>
                <span>
                  <button className="botao-pequeno" disabled={!completa || aRegistar !== null} onClick={() => void registar(i, r)}>
                    {aRegistar === i.id ? '…' : 'registar'}
                  </button>
                </span>
              </div>
              {erros[i.id] && <div className="erro aval-erro">{erros[i.id]}</div>}
            </div>
          );
        })}
        <EstadoPedido aCarregar={aCarregar} erro={erro} vazio={dados === undefined} />
        {dados && porAvaliar.length === 0 && <div className="vazio">Não há ideias por avaliar.</div>}
      </section>
      <IdeiaGaveta id={aberta} aoFechar={() => setAberta(null)} />
    </>
  );
}
