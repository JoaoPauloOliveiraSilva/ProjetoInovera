import { useState } from 'react';
import { iniciativas as api } from '../api/servicos';
import type { ResultadoImportacao } from '../api/tipos';
import { useAtualizar } from '../api/usePedido';
import { Cabecalho } from '../componentes/Layout';
import { Camara, Upload } from '../componentes/Icones';

/** Importa o Excel Mod.246 (portefólio) da dstelecom: ideias da Caixa de Inovação e todas as iniciativas. */
export function ImportarMod246() {
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
      setResultado(await api.importarMod246(ficheiro));
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
        Escolha o Excel <strong>Mod.246 Portfólio de Iniciativas de Inovação</strong>. São importadas as ideias (com o n.º da Caixa de
        Inovação, as notas e a decisão) e as folhas Projetos, Melhorias Contínuas, Oportunidades, Vigilâncias, Mestrados e Ações de
        Inovação. Pode importar outra vez mais tarde: o que já existe não se repete.
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
            <h3 className="secao">resultado</h3>
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
