import { useState } from 'react';
import { administracao } from '../api/servicos';
import { nomePerfil } from '../api/tipos';
import type { Perfil, PermissaoInfo } from '../api/tipos';
import { usePedido } from '../api/usePedido';
import { EstadoPedido } from '../componentes/Estado';
import { Cabecalho } from '../componentes/Layout';
import { Mais, Visto } from '../componentes/Icones';

/**
 * Backoffice: o que cada perfil pode fazer. Mudar aqui não exige mexer no código; o servidor lê
 * as permissões da base de dados a cada pedido.
 */
export function AdminPerfis() {
  const { dados, erro, aCarregar, recarregar } = usePedido(() => administracao.perfis(), []);
  const [novo, setNovo] = useState('');
  const [erroNovo, setErroNovo] = useState('');

  const criar = async () => {
    setErroNovo('');
    try {
      await administracao.criarPerfil(novo.trim());
      setNovo('');
      recarregar();
    } catch (e) {
      setErroNovo(e instanceof Error ? e.message : String(e));
    }
  };

  return (
    <>
      <Cabecalho titulo="perfis" corLampada="#8a8a8a" />
      <p className="introducao">
        Cada utilizador tem um ou mais perfis e pode fazer tudo o que os seus perfis permitem. O Administrador tem sempre todas
        as permissões. Registar ideias, comentar e dar gosto está aberto a todos.
      </p>
      <EstadoPedido aCarregar={aCarregar} erro={erro} vazio={dados === undefined} />
      <div className="perfis">
        {dados?.perfis.map((p) => (
          <CartaoPerfil key={p.nome} perfil={p} catalogo={dados.permissoes} aoMudar={recarregar} />
        ))}
        {dados && (
          <form
            className="perfil-cartao perfil-novo"
            onSubmit={(e) => {
              e.preventDefault();
              if (novo.trim()) void criar();
            }}
          >
            <h3>novo perfil</h3>
            <label className="campo">
              <span>nome</span>
              <input value={novo} onChange={(e) => setNovo(e.target.value)} maxLength={50} placeholder="ex.: Auditor" />
            </label>
            {erroNovo && <div className="erro">{erroNovo}</div>}
            <button type="submit" className="botao-pequeno" disabled={!novo.trim()}>
              <Mais width={14} height={14} /> criar perfil
            </button>
          </form>
        )}
      </div>
    </>
  );
}

function CartaoPerfil({ perfil, catalogo, aoMudar }: { perfil: Perfil; catalogo: PermissaoInfo[]; aoMudar: () => void }) {
  const [escolhidas, setEscolhidas] = useState<string[]>(perfil.permissoes);
  const [erro, setErro] = useState('');
  const [aGuardar, setAGuardar] = useState(false);
  const bloqueado = perfil.todasAsPermissoes;
  const alterado =
    escolhidas.length !== perfil.permissoes.length || escolhidas.some((p) => !perfil.permissoes.includes(p));
  const grupos = [...new Set(catalogo.map((c) => c.grupo))];

  const alternar = (codigo: string) =>
    setEscolhidas((atuais) => (atuais.includes(codigo) ? atuais.filter((c) => c !== codigo) : [...atuais, codigo]));

  const executar = async (f: () => Promise<unknown>) => {
    setErro('');
    setAGuardar(true);
    try {
      await f();
      aoMudar();
    } catch (e) {
      setErro(e instanceof Error ? e.message : String(e));
    } finally {
      setAGuardar(false);
    }
  };

  return (
    <section className="perfil-cartao">
      <div className="perfil-topo">
        <h3>{nomePerfil(perfil.nome)}</h3>
        <span className="cinzento-escuro">
          {perfil.utilizadores} {perfil.utilizadores === 1 ? 'utilizador' : 'utilizadores'}
        </span>
      </div>
      {bloqueado && <p className="cinzento-escuro">Tem sempre todas as permissões.</p>}
      {grupos.map((g) => (
        <div key={g} className="perfil-grupo">
          <div className="rotulo">{g}</div>
          {catalogo
            .filter((c) => c.grupo === g)
            .map((c) => {
              const marcada = bloqueado || escolhidas.includes(c.codigo);
              return (
                <label key={c.codigo} className={`caixa${bloqueado ? ' bloqueada' : ''}`}>
                  <input type="checkbox" checked={marcada} disabled={bloqueado} onChange={() => alternar(c.codigo)} />
                  <span className="caixa-quadrado">{marcada && <Visto />}</span>
                  {c.nome}
                </label>
              );
            })}
        </div>
      ))}
      {erro && <div className="erro">{erro}</div>}
      {!bloqueado && (
        <div className="botoes">
          <button
            className="botao-pequeno"
            disabled={!alterado || aGuardar}
            onClick={() => void executar(() => administracao.definirPermissoes(perfil.nome, escolhidas))}
          >
            {aGuardar ? 'a guardar…' : 'guardar'}
          </button>
          {!perfil.doSistema && (
            <button
              className="botao-pequeno claro"
              disabled={aGuardar || perfil.utilizadores > 0}
              title={perfil.utilizadores > 0 ? 'Mude primeiro o perfil dos utilizadores' : undefined}
              onClick={() => void executar(() => administracao.apagarPerfil(perfil.nome))}
            >
              apagar perfil
            </button>
          )}
        </div>
      )}
    </section>
  );
}
