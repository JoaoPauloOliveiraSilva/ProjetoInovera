import { useState } from 'react';
import { importacoes as api } from '../api/servicos';
import type { ResultadoImportacao } from '../api/tipos';
import { useAtualizar } from '../api/usePedido';
import { Cabecalho } from '../componentes/Layout';
import { Camara, Upload } from '../componentes/Icones';

/** Importa os modelos Excel da dstelecom (Mod.246, 235, 236, 237, 238, 239, 241, 252, 52 e 69); o modelo é reconhecido pelo conteúdo. */
const modelos: [string, string][] = [
  ['Mod.246', 'portefólio: ideias da Caixa de Inovação e todas as iniciativas'],
  ['Mod.235', 'acordos de parceria (e os parceiros)'],
  ['Mod.236', 'ativos intangíveis, com as atividades'],
  ['Mod.237', 'conhecimento codificado, com as atividades de partilha'],
  ['Mod.238', 'conhecimento tácito'],
  ['Mod.239', 'indicadores: valores dos anos e semestres fechados, objetivos anuais e planos de ação'],
  ['Mod.241', 'ferramentas e métodos, com as análises anuais'],
  ['Mod.252', 'lições aprendidas (no Project Charter de cada projeto ou desafio)'],
  ['Mod.52', 'estratégia de inovação (uma folha por ano)'],
  ['Mod.69', 'plano anual de atividades (as cores do calendário dizem que atividade é em cada dia)'],
];

export function ImportarModelos() {
  const atualizar = useAtualizar();
  const [ficheiro, setFicheiro] = useState<File | null>(null);
  const [resultado, setResultado] = useState<ResultadoImportacao | null>(null);
  const [erro, setErro] = useState('');
  const [aEnviar, setAEnviar] = useState(false);

  const importar = async () => {
    if (!ficheiro) return;
    setErro('');
    setResultado(null);
    setAEnviar(true);
    try {
      setResultado(await api.importar(ficheiro));
      atualizar();
    } catch (e) {
      setErro(e instanceof Error ? e.message : String(e));
    } finally {
      setAEnviar(false);
    }
  };

  return (
    <>
      <Cabecalho titulo="importar" corLampada="#8a8a8a" />
      <p className="introducao">
        Escolha um dos <strong>modelos Excel da inovação</strong>: o modelo é reconhecido sozinho pelo conteúdo do ficheiro. Pode importar
        outra vez mais tarde (por exemplo depois de atualizar o Excel): o que já existe não se repete.
      </p>
      <ul className="modelos-importaveis">
        {modelos.map(([codigo, descricao]) => (
          <li key={codigo}>
            <strong>{codigo}</strong> {descricao}
          </li>
        ))}
      </ul>
      <p className="ajuda">
        Importe primeiro o Mod.246: as lições aprendidas (Mod.252) ficam nos projetos e desafios com o mesmo nome, e a estratégia (Mod.52)
        liga-se às iniciativas.
      </p>
      <section className="painel importar">
        <label className="anexos">
          <input type="file" accept=".xlsx" onChange={(e) => setFicheiro(e.target.files?.[0] ?? null)} />
          <strong>
            <Camara /> escolher o ficheiro .xlsx
          </strong>
          {ficheiro && <span className="lista-anexos">{ficheiro.name}</span>}
        </label>
        <div className="botoes">
          <button className="botao-largo" disabled={!ficheiro || aEnviar} onClick={() => void importar()}>
            {aEnviar ? 'a importar…' : 'importar'} <Upload className="seta-vermelha" />
          </button>
        </div>
        {erro && <div className="erro">{erro}</div>}
        {resultado && (
          <div className="resultado-importacao">
            <h3 className="secao">resultado · {resultado.modelo}</h3>
            <table>
              <thead>
                <tr>
                  <th>Folha</th>
                  <th>Importadas</th>
                  <th>Já existiam</th>
                  <th>Ignoradas</th>
                </tr>
              </thead>
              <tbody>
                {resultado.folhas.map((f) => (
                  <tr key={f.folha}>
                    <td>{f.folha}</td>
                    <td>{f.importadas}</td>
                    <td>{f.jaExistiam}</td>
                    <td>{f.ignoradas}</td>
                  </tr>
                ))}
              </tbody>
            </table>
            {resultado.avisos.length > 0 && (
              <ul className="avisos-importacao">
                {resultado.avisos.map((a) => (
                  <li key={a}>{a}</li>
                ))}
              </ul>
            )}
          </div>
        )}
      </section>
    </>
  );
}
