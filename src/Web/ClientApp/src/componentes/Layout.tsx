import { useEffect, useState } from 'react';
import type { ReactNode } from 'react';
import { Link, NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { primeiroNome } from '../dados/regras';
import { useSessao, useUtilizador } from '../sessao/Sessao';
import { Chevron, IconeAjuda, IconeInovacao, IconeLivros, IconeMinhaArea, IconeSair, Lampada } from './Icones';

interface ItemMenu {
  nome: string;
  para?: string;
  filhos?: { nome: string; para: string; soAdministrador?: boolean }[];
}

// Menu igual ao do site atual de inovação.
const menu: ItemMenu[] = [
  { nome: 'homepage', para: '/inovacao' },
  { nome: 'a minha caixa', filhos: [{ nome: 'as minhas ideias', para: '/inovacao/minha-caixa' }] },
  {
    nome: 'sistema de gestão de inovação',
    filhos: [
      { nome: 'validação de ideias', para: '/inovacao/validacao', soAdministrador: true },
      { nome: 'avaliação de ideias', para: '/inovacao/avaliacao', soAdministrador: true },
    ],
  },
  {
    nome: 'ideias de inovação',
    filhos: [
      { nome: 'em discussão', para: '/inovacao/ideias/discussao' },
      { nome: 'todas as ideias', para: '/inovacao/ideias' },
    ],
  },
  { nome: 'i9talks', para: '/inovacao/i9talks' },
  { nome: 'i9news', para: '/inovacao/i9news' },
  { nome: 'a inovação compensa quem inova', filhos: [{ nome: 'prémios', para: '/inovacao/premios' }] },
  { nome: 'desafios', filhos: [{ nome: 'todos os desafios', para: '/inovacao/desafios' }] },
  { nome: 'vigilâncias', filhos: [{ nome: 'todas as vigilâncias', para: '/inovacao/vigilancias' }] },
  { nome: 'projetos', para: '/inovacao/projetos' },
  { nome: 'sifide', filhos: [{ nome: 'candidaturas', para: '/inovacao/sifide' }] },
  { nome: 'iniciativas de inovação', para: '/inovacao/iniciativas' },
  { nome: 'oportunidades de inovação', para: '/inovacao/oportunidades' },
  { nome: 'repositório do conhecimento', filhos: [{ nome: 'conhecimento codificado', para: '/inovacao/conhecimento' }] },
  { nome: 'indicadores de desempenho', filhos: [{ nome: 'KPIs', para: '/inovacao/indicadores' }] },
];

function Grupo({ item }: { item: ItemMenu }) {
  const { pathname } = useLocation();
  const ativoDentro = item.filhos?.some((f) => pathname === f.para) ?? false;
  const [aberto, setAberto] = useState(ativoDentro);

  // Abre o grupo quando se navega para uma das suas páginas.
  useEffect(() => {
    if (ativoDentro) setAberto(true);
  }, [ativoDentro]);

  if (item.para) {
    return (
      <li>
        <NavLink to={item.para} end className={({ isActive }) => (isActive ? 'menu-item ativo' : 'menu-item')}>
          {item.nome}
        </NavLink>
      </li>
    );
  }

  return (
    <li>
      <button className={`menu-item menu-grupo${aberto ? ' aberto' : ''}`} onClick={() => setAberto(!aberto)}>
        <span>{item.nome}</span>
        <Chevron className="menu-chevron" />
      </button>
      {aberto && (
        <ul className="submenu">
          {item.filhos?.map((f) => (
            <li key={f.para}>
              <NavLink to={f.para} end className={({ isActive }) => (isActive ? 'menu-item ativo' : 'menu-item')}>
                {f.nome}
              </NavLink>
            </li>
          ))}
        </ul>
      )}
    </li>
  );
}

/** Tira as opções só da equipa de Inovação (e os grupos que ficam vazios). */
function menuPara(eAdministrador: boolean): ItemMenu[] {
  return menu
    .map((item) => (item.filhos ? { ...item, filhos: item.filhos.filter((f) => eAdministrador || !f.soAdministrador) } : item))
    .filter((item) => !item.filhos || item.filhos.length > 0);
}

export function Layout() {
  const utilizador = useUtilizador();
  const { sair } = useSessao();
  const navegar = useNavigate();

  return (
    <div className="app">
      <nav className="barra" aria-label="Aplicações">
        <Link to="/" className="barra-logo" title="Plataforma de Gestão da Certificação">
          dstgroup
        </Link>
        <div className="barra-avatar" aria-hidden="true">
          {utilizador.nome[0]?.toUpperCase()}
        </div>
        <span className="barra-app desativada" title="Brevemente">
          <IconeLivros />
          <span>ócionegócio</span>
        </span>
        <NavLink to="/inovacao" className="barra-app ativa">
          <IconeInovacao />
          <span>inovação</span>
        </NavLink>
        <span className="barra-app desativada" title="Brevemente">
          <IconeMinhaArea />
          <span>minha área</span>
        </span>
        <span className="barra-ajuda" title="Ajuda">
          <IconeAjuda />
          <span>ajuda</span>
        </span>
        <button
          className="barra-sair"
          title="Terminar sessão"
          onClick={() => {
            void sair().then(() => navegar('/'));
          }}
        >
          <IconeSair />
          <span>sair</span>
        </button>
      </nav>
      <aside className="menu">
        <h2 className="menu-ola">olá, {primeiroNome(utilizador.nome)}</h2>
        <ul>
          {menuPara(utilizador.eAdministrador).map((item) => (
            <Grupo key={item.nome} item={item} />
          ))}
        </ul>
      </aside>
      <main className="conteudo">
        <Outlet />
      </main>
    </div>
  );
}

/** Cabeçalho das páginas: lâmpada num círculo, "INOVAÇÃO" e título grande. */
export function Cabecalho({ titulo, corLampada, acao }: { titulo: string; corLampada?: string; acao?: ReactNode }) {
  return (
    <header className="cabecalho">
      <div className="cabecalho-circulo">
        <LampadaCabecalho cor={corLampada} />
      </div>
      <div>
        <div className="sobretitulo">INOVAÇÃO</div>
        <h1 className="titulo">{titulo}</h1>
        {acao}
      </div>
    </header>
  );
}

function LampadaCabecalho({ cor }: { cor?: string }) {
  return <Lampada cor={cor} width={72} height={72} />;
}
