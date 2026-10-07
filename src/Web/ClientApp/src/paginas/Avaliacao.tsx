import { useState } from 'react';
import { Cabecalho } from '../componentes/Layout';
import { useIdeias } from '../dados/IdeiasContext';
import { calcularNota, codigoIdeia, formatarData, limiarAprovacao, variaveis } from '../dados/regras';
import { classesAprovada, classesNaoAprovada, nomeClasse } from '../dados/tipos';
import type { ClasseIdeia, Ideia, Nota, Notas, Responsabilidade } from '../dados/tipos';

const semNotas: Notas = { custo: null, enquadramento: null, beneficio: null, adequacao: null, incerteza: null };

interface Rascunho {
  responsabilidade: Responsabilidade;
  notas: Notas;
  decisaoDst: boolean | null;
  classe: ClasseIdeia | null;
}

/**
 * Página de avaliação: junta todas as ideias que terminaram a discussão ("em avaliação pelo manager")
 * para serem analisadas ao mesmo tempo na reunião mensal com a CE.
 */
export function Avaliacao() {
  const { ideias, avaliar } = useIdeias();
  const porAvaliar = ideias.filter((i) => i.estado === 'em-avaliacao');
  const [rascunhos, setRascunhos] = useState<Record<number, Rascunho>>({});

  const rascunho = (i: Ideia): Rascunho =>
    rascunhos[i.num] ?? { responsabilidade: i.responsabilidade ?? 'dstelecom', notas: semNotas, decisaoDst: null, classe: null };

  const mudar = (i: Ideia, alteracao: Partial<Rascunho>) =>
    setRascunhos((r) => ({ ...r, [i.num]: { ...rascunho(i), ...alteracao } }));

  return (
    <>
      <Cabecalho titulo="avaliação" corLampada="#e3b341" />
      <p className="introducao">
        Ideias com a discussão terminada. Notas de 0 a 4 (1 = caro/mau, 4 = bom). Nota final = 15% custo + 25% enquadramento + 30%
        benefício + 20% adequação técnica + 10% incerteza; com {limiarAprovacao} ou mais fica aprovada. As ideias da dst só registam a
        decisão.
      </p>
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
          const ehDst = r.responsabilidade === 'dst';
          const nota = ehDst ? null : calcularNota(r.notas);
          const aprovada = ehDst ? r.decisaoDst : nota === null ? null : nota >= limiarAprovacao;
          const classes: ClasseIdeia[] = aprovada === null ? [] : aprovada ? classesAprovada : classesNaoAprovada;
          const completa = aprovada !== null && r.classe !== null && classes.includes(r.classe);

          return (
            <div key={i.num} className="aval-linha">
              <span className="num">{codigoIdeia(i.num)}</span>
              <span className="forte" title={`${formatarData(i.data)} · ${i.likes} gostos`}>
                {i.titulo}
              </span>
              <span>
                <select
                  className={`resp resp-${r.responsabilidade}`}
                  value={r.responsabilidade}
                  onChange={(e) => mudar(i, { responsabilidade: e.target.value as Responsabilidade, classe: null })}
                >
                  <option value="dstelecom">dstelecom</option>
                  <option value="dst">dst</option>
                </select>
              </span>
              {variaveis.map((v) => (
                <span key={v.chave}>
                  {ehDst ? (
                    <span className="na">N.A.</span>
                  ) : (
                    <select
                      value={r.notas[v.chave] ?? ''}
                      onChange={(e) => {
                        const valor: Nota = e.target.value === '' ? null : Number(e.target.value);
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
                    onChange={(e) =>
                      mudar(i, { decisaoDst: e.target.value === '' ? null : e.target.value === 'sim', classe: null })
                    }
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
                <button className="botao-pequeno" disabled={!completa} onClick={() => avaliar(i.num, r.responsabilidade, r.notas, r.decisaoDst, r.classe)}>
                  registar
                </button>
              </span>
            </div>
          );
        })}
        {porAvaliar.length === 0 && <div className="vazio">Não há ideias por avaliar.</div>}
      </section>
    </>
  );
}
