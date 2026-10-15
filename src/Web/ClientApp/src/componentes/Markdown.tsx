import type { ReactNode } from 'react';

// Markdown mínimo para o manual da ajuda (sem bibliotecas): títulos "##", parágrafos, listas (com sub-listas de 4 espaços),
// listas numeradas, tabelas com "|" e **negrito**. Cada "## N. Título" ganha o id "seccao-N" para se poder ligar a ele.

export interface SeccaoManual {
  id: string;
  titulo: string;
}

export function seccoes(texto: string): SeccaoManual[] {
  return texto
    .split('\n')
    .filter((l) => l.startsWith('## '))
    .map((l) => ({ id: idSeccao(l.slice(3)), titulo: l.slice(3).trim() }));
}

function idSeccao(titulo: string): string {
  const numero = /^(\d+)\./.exec(titulo.trim());
  return numero ? `seccao-${numero[1]}` : titulo.trim().toLowerCase().replace(/\s+/g, '-');
}

/** **negrito** dentro de uma linha. */
function emLinha(texto: string): ReactNode[] {
  return texto.split(/(\*\*[^*]+\*\*)/g).map((parte, i) =>
    parte.startsWith('**') && parte.endsWith('**') ? <strong key={i}>{parte.slice(2, -2)}</strong> : parte,
  );
}

type Item = { texto: string; filhos: string[] };

function Lista({ itens, numerada }: { itens: Item[]; numerada: boolean }) {
  const filhos = itens.map((it, i) => (
    <li key={i}>
      {emLinha(it.texto)}
      {it.filhos.length > 0 && (
        <ul>
          {it.filhos.map((f, j) => (
            <li key={j}>{emLinha(f)}</li>
          ))}
        </ul>
      )}
    </li>
  ));
  return numerada ? <ol>{filhos}</ol> : <ul>{filhos}</ul>;
}

function celulas(linha: string): string[] {
  return linha
    .trim()
    .replace(/^\|/, '')
    .replace(/\|$/, '')
    .split('|')
    .map((c) => c.trim());
}

export function Markdown({ texto }: { texto: string }) {
  const linhas = texto.replace(/\r/g, '').split('\n');
  const blocos: ReactNode[] = [];
  let i = 0;
  while (i < linhas.length) {
    const linha = linhas[i];
    if (linha.trim() === '') {
      i++;
      continue;
    }

    if (linha.startsWith('## ')) {
      const titulo = linha.slice(3).trim();
      blocos.push(
        <h2 key={i} id={idSeccao(titulo)} className="ajuda-titulo">
          {titulo}
        </h2>,
      );
      i++;
      continue;
    }

    if (linha.trim().startsWith('|')) {
      const tabela: string[] = [];
      while (i < linhas.length && linhas[i].trim().startsWith('|')) tabela.push(linhas[i++]);
      const [cabecalho, , ...corpo] = tabela;
      blocos.push(
        <table key={i} className="ajuda-tabela">
          <thead>
            <tr>
              {celulas(cabecalho).map((c, k) => (
                <th key={k}>{emLinha(c)}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {corpo.map((l, r) => (
              <tr key={r}>
                {celulas(l).map((c, k) => (
                  <td key={k}>{emLinha(c)}</td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>,
      );
      continue;
    }

    const marcador = /^(- |\d+\. )/;
    if (marcador.test(linha)) {
      const numerada = /^\d+\. /.test(linha);
      const itens: Item[] = [];
      while (i < linhas.length && (marcador.test(linhas[i]) || /^ {4}- /.test(linhas[i]))) {
        if (/^ {4}- /.test(linhas[i])) itens[itens.length - 1]?.filhos.push(linhas[i].trim().slice(2));
        else itens.push({ texto: linhas[i].replace(marcador, ''), filhos: [] });
        i++;
      }
      blocos.push(<Lista key={i} itens={itens} numerada={numerada} />);
      continue;
    }

    const paragrafo: string[] = [];
    while (i < linhas.length && linhas[i].trim() !== '' && !/^(## |\||- |\d+\. )/.test(linhas[i])) paragrafo.push(linhas[i++]);
    blocos.push(<p key={i}>{emLinha(paragrafo.join(' '))}</p>);
  }

  return <div className="ajuda-texto">{blocos}</div>;
}
