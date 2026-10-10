import { useState } from 'react';
import type { FormEvent } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { contas } from '../api/servicos';
import { SetaContinuar } from '../componentes/Icones';
import { useSessao } from '../sessao/Sessao';

/**
 * Página do link enviado por email: aceitar o convite (tipo=convite) ou escolher uma password nova (tipo=recuperar).
 * Depois de definir a password a pessoa fica logo com sessão iniciada.
 */
export function DefinirPassword() {
  const [parametros] = useSearchParams();
  const tipo = parametros.get('tipo') === 'recuperar' ? 'recuperar' : 'convite';
  const utilizadorId = parametros.get('u') ?? '';
  const token = parametros.get('t') ?? '';
  const { recarregar } = useSessao();
  const navegar = useNavigate();

  const [password, setPassword] = useState('');
  const [repetir, setRepetir] = useState('');
  const [erro, setErro] = useState('');
  const [aEnviar, setAEnviar] = useState(false);

  const linkInvalido = !utilizadorId || !token;

  const submeter = async (e: FormEvent) => {
    e.preventDefault();
    setErro('');
    if (password !== repetir) {
      setErro('As duas passwords não são iguais.');
      return;
    }
    setAEnviar(true);
    try {
      if (tipo === 'convite') {
        await contas.aceitarConvite(utilizadorId, token, password);
      } else {
        await contas.redefinirPassword(utilizadorId, token, password);
      }
      await recarregar();
      navegar('/inovacao', { replace: true });
    } catch (falha) {
      setErro(falha instanceof Error ? falha.message : 'Não foi possível definir a password.');
    } finally {
      setAEnviar(false);
    }
  };

  return (
    <div className="landing entrar">
      <Link to="/" className="landing-titulo entrar-titulo">
        Plataforma de Gestão da Certificação
      </Link>
      <form className="entrar-cartao" onSubmit={(e) => void submeter(e)}>
        <div className="sobretitulo">INOVAÇÃO</div>
        <h1>{tipo === 'convite' ? 'bem-vindo' : 'password nova'}</h1>
        {linkInvalido ? (
          <>
            <div className="erro">Este link está incompleto. Abra-o outra vez a partir do email.</div>
            <Link to="/entrar" className="ligacao entrar-trocar">
              ir para entrar
            </Link>
          </>
        ) : (
          <>
            <p className="ajuda">
              {tipo === 'convite' ? 'Escolha a password para entrar na plataforma.' : 'Escolha uma password nova.'} Pelo menos 8
              caracteres, com maiúscula, minúscula e número.
            </p>
            <label className="campo">
              <span>password</span>
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                autoComplete="new-password"
                minLength={8}
                required
              />
            </label>
            <label className="campo">
              <span>repetir a password</span>
              <input
                type="password"
                value={repetir}
                onChange={(e) => setRepetir(e.target.value)}
                autoComplete="new-password"
                minLength={8}
                required
              />
            </label>
            {erro && <div className="erro">{erro}</div>}
            <button type="submit" className="botao-largo" disabled={aEnviar}>
              {aEnviar ? 'a guardar…' : 'guardar e entrar'} <SetaContinuar className="seta-vermelha" />
            </button>
          </>
        )}
      </form>
    </div>
  );
}
