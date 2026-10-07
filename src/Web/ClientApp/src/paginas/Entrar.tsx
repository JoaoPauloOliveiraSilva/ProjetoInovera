import { useState } from 'react';
import type { FormEvent } from 'react';
import { Link, Navigate, useSearchParams } from 'react-router-dom';
import { SetaContinuar } from '../componentes/Icones';
import { useSessao } from '../sessao/Sessao';

/** Entrar / criar conta. As contas são da própria plataforma (sem ligação aos sistemas da dst). */
export function Entrar() {
  const { utilizador, entrar, registar } = useSessao();
  const [parametros] = useSearchParams();
  const voltar = parametros.get('voltar') ?? '/inovacao';
  const destino = voltar.startsWith('/') && !voltar.startsWith('//') ? voltar : '/inovacao';

  const [modo, setModo] = useState<'entrar' | 'registar'>('entrar');
  const [nome, setNome] = useState('');
  const [empresa, setEmpresa] = useState('dstelecom');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [erro, setErro] = useState('');
  const [aEnviar, setAEnviar] = useState(false);

  if (utilizador) {
    return <Navigate to={destino} replace />;
  }

  const submeter = async (e: FormEvent) => {
    e.preventDefault();
    setErro('');
    setAEnviar(true);
    try {
      if (modo === 'entrar') {
        await entrar(email, password);
      } else {
        await registar({ nome: nome.trim(), email: email.trim(), password, empresa: empresa.trim() || null });
      }
    } catch (falha) {
      setErro(falha instanceof Error ? falha.message : 'Não foi possível continuar.');
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
        <h1>{modo === 'entrar' ? 'entrar' : 'criar conta'}</h1>

        {modo === 'registar' && (
          <>
            <label className="campo">
              <span>nome</span>
              <input value={nome} onChange={(e) => setNome(e.target.value)} autoComplete="name" required />
            </label>
            <label className="campo">
              <span>empresa</span>
              <input value={empresa} onChange={(e) => setEmpresa(e.target.value)} autoComplete="organization" />
            </label>
          </>
        )}
        <label className="campo">
          <span>email</span>
          <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} autoComplete="email" required />
        </label>
        <label className="campo">
          <span>palavra-passe</span>
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            autoComplete={modo === 'entrar' ? 'current-password' : 'new-password'}
            minLength={modo === 'registar' ? 8 : undefined}
            required
          />
          {modo === 'registar' && <small className="ajuda">Pelo menos 8 caracteres, com maiúscula, minúscula e número.</small>}
        </label>

        {erro && <div className="erro">{erro}</div>}

        <button type="submit" className="botao-largo" disabled={aEnviar}>
          {aEnviar ? 'a enviar…' : modo === 'entrar' ? 'entrar' : 'criar conta'} <SetaContinuar className="seta-vermelha" />
        </button>

        <button
          type="button"
          className="ligacao entrar-trocar"
          onClick={() => {
            setModo(modo === 'entrar' ? 'registar' : 'entrar');
            setErro('');
          }}
        >
          {modo === 'entrar' ? 'ainda não tenho conta' : 'já tenho conta'}
        </button>
      </form>
    </div>
  );
}
