import { useState } from 'react';
import type { FormEvent } from 'react';
import { administracao } from '../api/servicos';
import { nomeEstadoConta, nomePerfil } from '../api/tipos';
import type { Perfil, UtilizadorAdmin } from '../api/tipos';
import { usePedido } from '../api/usePedido';
import { EstadoPedido } from '../componentes/Estado';
import { Gaveta } from '../componentes/Gaveta';
import { Cabecalho } from '../componentes/Layout';
import { Mais, SetaContinuar, Visto } from '../componentes/Icones';
import { useUtilizador } from '../sessao/Sessao';

const classeEstado = { ConvitePendente: 'estado-validacao-equipa', Ativa: 'estado-aprovada', Desativada: 'estado-nao-aprovada' } as const;

/** Backoffice: adicionar pessoas (recebem um convite por email), mudar perfis, desativar contas. */
export function AdminUtilizadores() {
  const eu = useUtilizador();
  const lista = usePedido(() => administracao.utilizadores(), []);
  const perfis = usePedido(() => administracao.perfis(), []);
  const [filtro, setFiltro] = useState('');
  const [aEditar, setAEditar] = useState<UtilizadorAdmin | 'novo' | null>(null);
  const [mensagem, setMensagem] = useState('');
  const [erro, setErro] = useState('');
  const [ocupado, setOcupado] = useState<number | null>(null);

  const texto = filtro.trim().toLowerCase();
  const visiveis = (lista.dados ?? []).filter(
    (u) => !texto || u.nome.toLowerCase().includes(texto) || u.email.toLowerCase().includes(texto),
  );

  const acao = async (u: UtilizadorAdmin, f: () => Promise<unknown>, sucesso: string) => {
    setErro('');
    setMensagem('');
    setOcupado(u.id);
    try {
      await f();
      setMensagem(sucesso);
      lista.recarregar();
    } catch (e) {
      setErro(e instanceof Error ? e.message : String(e));
    } finally {
      setOcupado(null);
    }
  };

  return (
    <>
      <Cabecalho
        titulo="utilizadores"
        corLampada="#8a8a8a"
        acao={
          <button className="nova-ideia" onClick={() => setAEditar('novo')}>
            <Mais /> adicionar utilizador
          </button>
        }
      />
      <div className="filtros">
        <label>
          procurar
          <input className="filtro-texto" value={filtro} onChange={(e) => setFiltro(e.target.value)} placeholder="nome ou email" />
        </label>
      </div>
      {mensagem && <div className="aviso-ok barra-mensagem">{mensagem}</div>}
      {erro && <div className="erro barra-mensagem">{erro}</div>}
      <section className="tabela">
        <div className="tabela-linha tabela-cabecalho admin-linha">
          <span>Nome</span>
          <span>Email</span>
          <span>Perfis</span>
          <span>Estado</span>
          <span />
        </div>
        {visiveis.map((u) => (
          <div key={u.id} className="tabela-linha admin-linha" onClick={() => setAEditar(u)}>
            <span className="forte">
              {u.nome}
              {u.empresa && <div className="cinzento-escuro">{u.empresa}</div>}
            </span>
            <span>{u.email}</span>
            <span className="fichas">
              {u.perfis.map((p) => (
                <span key={p} className="etiqueta">
                  {nomePerfil(p)}
                </span>
              ))}
            </span>
            <span>
              <span className={`etiqueta ${classeEstado[u.estado]}`}>{nomeEstadoConta[u.estado]}</span>
            </span>
            <span className="acoes" onClick={(e) => e.stopPropagation()}>
              {u.estado === 'ConvitePendente' && (
                <button
                  className="botao-pequeno claro"
                  disabled={ocupado !== null}
                  onClick={() => void acao(u, () => administracao.reenviarConvite(u.id), `Convite reenviado para ${u.email}.`)}
                >
                  reenviar convite
                </button>
              )}
              {u.estado === 'Desativada' ? (
                <button
                  className="botao-pequeno claro"
                  disabled={ocupado !== null}
                  onClick={() => void acao(u, () => administracao.ativar(u.id), `${u.nome} voltou a ter acesso.`)}
                >
                  ativar
                </button>
              ) : (
                u.id !== eu.id && (
                  <button
                    className="botao-pequeno claro"
                    disabled={ocupado !== null}
                    onClick={() => void acao(u, () => administracao.desativar(u.id), `${u.nome} deixou de ter acesso.`)}
                  >
                    desativar
                  </button>
                )
              )}
            </span>
          </div>
        ))}
        <EstadoPedido aCarregar={lista.aCarregar} erro={lista.erro} vazio={lista.dados === undefined} />
        {lista.dados && visiveis.length === 0 && <div className="vazio">Não há utilizadores para mostrar.</div>}
      </section>

      <Gaveta aberta={aEditar !== null} aoFechar={() => setAEditar(null)}>
        {aEditar !== null && (
          <FormUtilizador
            key={aEditar === 'novo' ? 'novo' : aEditar.id}
            utilizador={aEditar === 'novo' ? null : aEditar}
            perfis={perfis.dados?.perfis ?? []}
            aoGuardar={(texto) => {
              setAEditar(null);
              setErro('');
              setMensagem(texto);
              lista.recarregar();
            }}
          />
        )}
      </Gaveta>
    </>
  );
}

function FormUtilizador({
  utilizador,
  perfis,
  aoGuardar,
}: {
  utilizador: UtilizadorAdmin | null;
  perfis: Perfil[];
  aoGuardar: (mensagem: string) => void;
}) {
  const [nome, setNome] = useState(utilizador?.nome ?? '');
  const [email, setEmail] = useState(utilizador?.email ?? '');
  const [empresa, setEmpresa] = useState(utilizador?.empresa ?? 'dstelecom, s.a.');
  const [escolhidos, setEscolhidos] = useState<string[]>(utilizador?.perfis ?? ['Trabalhador']);
  const [erro, setErro] = useState('');
  const [aEnviar, setAEnviar] = useState(false);

  const alternar = (perfil: string) =>
    setEscolhidos((atuais) => (atuais.includes(perfil) ? atuais.filter((p) => p !== perfil) : [...atuais, perfil]));

  const submeter = async (e: FormEvent) => {
    e.preventDefault();
    setErro('');
    setAEnviar(true);
    try {
      const dados = { nome: nome.trim(), empresa: empresa.trim() || null, perfis: escolhidos };
      if (utilizador) {
        await administracao.editar(utilizador.id, dados);
        aoGuardar(`${dados.nome} foi atualizado.`);
      } else {
        await administracao.adicionar({ ...dados, email: email.trim() });
        aoGuardar(`${dados.nome} foi adicionado. Enviámos um convite para ${email.trim()} para definir a password.`);
      }
    } catch (falha) {
      setErro(falha instanceof Error ? falha.message : String(falha));
    } finally {
      setAEnviar(false);
    }
  };

  return (
    <form className="form-gaveta" onSubmit={(e) => void submeter(e)}>
      <div className="sobretitulo">ADMINISTRAÇÃO</div>
      <h2 className="detalhe-titulo">{utilizador ? 'editar utilizador' : 'adicionar utilizador'}</h2>
      {!utilizador && (
        <p className="texto">A pessoa recebe um email com um link para definir a password (válido durante 3 dias).</p>
      )}
      <label className="campo">
        <span>nome</span>
        <input value={nome} onChange={(e) => setNome(e.target.value)} required maxLength={200} />
      </label>
      <label className="campo">
        <span>email</span>
        <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required disabled={utilizador !== null} />
      </label>
      <label className="campo">
        <span>empresa</span>
        <input value={empresa} onChange={(e) => setEmpresa(e.target.value)} maxLength={150} />
      </label>
      <div className="rotulo">perfis</div>
      <div className="opcoes">
        {perfis.map((p) => (
          <label key={p.nome} className="caixa">
            <input type="checkbox" checked={escolhidos.includes(p.nome)} onChange={() => alternar(p.nome)} />
            <span className="caixa-quadrado">{escolhidos.includes(p.nome) && <Visto />}</span>
            {nomePerfil(p.nome)}
          </label>
        ))}
      </div>
      {erro && <div className="erro">{erro}</div>}
      <button type="submit" className="botao-largo" disabled={aEnviar || escolhidos.length === 0}>
        {aEnviar ? 'a guardar…' : utilizador ? 'guardar' : 'adicionar e enviar convite'} <SetaContinuar className="seta-vermelha" />
      </button>
    </form>
  );
}
