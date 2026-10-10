import { useState } from 'react';
import type { FormEvent } from 'react';
import { Link, Navigate, useSearchParams } from 'react-router-dom';
import { contas } from '../api/servicos';
import { SetaContinuar } from '../componentes/Icones';
import { useSessao } from '../sessao/Sessao';

/**
 * Entrar. As contas são criadas pelo Administrador no backoffice; a pessoa recebe um email com o link
 * para definir a password. Aqui também se pede o link para recuperar a password.
 */
export function Entrar() {
  const { utilizador, entrar } = useSessao();
  const [parametros] = useSearchParams();
  const voltar = parametros.get('voltar') ?? '/inovacao';
  const destino = voltar.startsWith('/') && !voltar.startsWith('//') ? voltar : '/inovacao';

  const [modo, setModo] = useState<'entrar' | 'recuperar'>('entrar');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [erro, setErro] = useState('');
  const [aviso, setAviso] = useState('');
  const [aEnviar, setAEnviar] = useState(false);

  if (utilizador) {
    return <Navigate to={destino} replace />;
  }

  const submeter = async (e: FormEvent) => {
    e.preventDefault();
    setErro('');
    setAviso('');
    setAEnviar(true);
    try {
      if (modo === 'entrar') {
        await entrar(email, password);
      } else {
        await contas.recuperarPassword(email.trim());
        setAviso(`Se existir uma conta com ${email.trim()}, vai receber um email com o link para escolher uma password nova.`);
      }
    } catch (falha) {
      setErro(falha instanceof Error ? falha.message : 'Não foi possível continuar.');
    } finally {
      setAEnviar(false);
    }
  };

  const trocar = (novo: 'entrar' | 'recuperar') => {
    setModo(novo);
    setErro('');
    setAviso('');
  };

  return (
    <div className="landing entrar">
      <Link to="/" className="landing-titulo entrar-titulo">
        Plataforma de Gestão da Certificação
      </Link>
      <form className="entrar-cartao" onSubmit={(e) => void submeter(e)}>
        <div className="sobretitulo">INOVAÇÃO</div>
        <h1>{modo === 'entrar' ? 'entrar' : 'recuperar password'}</h1>

        <label className="campo">
          <span>email</span>
          <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} autoComplete="email" required />
        </label>
        {modo === 'entrar' && (
          <label className="campo">
            <span>password</span>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              autoComplete="current-password"
              required
            />
          </label>
        )}

        {erro && <div className="erro">{erro}</div>}
        {aviso && <div className="aviso-ok">{aviso}</div>}

        <button type="submit" className="botao-largo" disabled={aEnviar}>
          {aEnviar ? 'a enviar…' : modo === 'entrar' ? 'entrar' : 'enviar link'} <SetaContinuar className="seta-vermelha" />
        </button>

        <button type="button" className="ligacao entrar-trocar" onClick={() => trocar(modo === 'entrar' ? 'recuperar' : 'entrar')}>
          {modo === 'entrar' ? 'esqueci-me da password' : 'voltar a entrar'}
        </button>
        {modo === 'entrar' && <p className="ajuda centro">Não tem conta? Peça à equipa de Inovação para o adicionar.</p>}
      </form>
    </div>
  );
}
