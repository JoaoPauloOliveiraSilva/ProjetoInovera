import { useState, type FormEvent } from 'react';
import { Button } from '@fluentui/react-components';

interface LoginScreenProps {
  onLogin: (username: string) => void;
}

const DEMO_USERS: Record<string, string> = { admin: '123', chen: '123' };

export default function LoginScreen({ onLogin }: LoginScreenProps) {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const normalizedUsername = username.trim().toLowerCase();
    if (DEMO_USERS[normalizedUsername] === password) {
      onLogin(normalizedUsername);
      return;
    }
    setError('Utilizador ou palavra-passe incorretos.');
  }

  return (
    <main className="login-screen">
      <section className="login-panel">
        <div className="login-brand"><span className="login-mark">d</span><span>dstgroup</span></div>
        <div className="login-copy">
          <div className="eyebrow">INOVAÇÃO</div>
          <h1>Boas ideias<br />começam aqui.</h1>
          <p>Entre na plataforma para partilhar ideias e acompanhar a inovação da equipa.</p>
        </div>
        <p className="login-footnote">Ambiente de demonstração · acesso local</p>
      </section>
      <section className="login-form-panel" aria-labelledby="login-title">
        <div className="login-form-content">
          <p className="eyebrow">BEM-VINDO</p>
          <h2 id="login-title">Iniciar sessão</h2>
          <p className="login-description">Use uma das contas locais de demonstração.</p>
          <form className="login-form" onSubmit={handleSubmit}>
            <label htmlFor="login-user">Utilizador</label>
            <input id="login-user" autoComplete="username" value={username} onChange={event => setUsername(event.target.value)} required />
            <label htmlFor="login-password">Palavra-passe</label>
            <input id="login-password" type="password" autoComplete="current-password" value={password} onChange={event => setPassword(event.target.value)} required />
            {error && <p className="login-error" role="alert">{error}</p>}
            <Button type="submit" appearance="primary" className="login-submit">Entrar</Button>
          </form>
          <p className="login-hint">Contas de teste: <strong>admin</strong> / <strong>123</strong> · <strong>chen</strong> / <strong>123</strong></p>
        </div>
      </section>
    </main>
  );
}
