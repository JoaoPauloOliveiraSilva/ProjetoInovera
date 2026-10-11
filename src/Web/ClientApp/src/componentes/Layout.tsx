import { useEffect, useState } from 'react';
import type { ReactNode } from 'react';
import { Link, NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { Permissoes } from '../api/permissoes';
import type { Permissao } from '../api/permissoes';
import { primeiroNome } from '../dados/regras';
import { usePermissao, useSessao, useUtilizador } from '../sessao/Sessao';
import { Avisos } from './Avisos';
import { Chevron, IconeAjuda, IconeInovacao, IconeLivros, IconeMinhaArea, IconeSair, Lampada } from './Icones';

interface ItemMenu {
  nome: string;
  para?: string;
  filhos?: { nome: string; para: string; permissao?: Permissao }[];
}

// Menu igual ao do site atual de inovação.
const menu: ItemMenu[] = [
  { nome: 'homepage', para: '/inovacao' },
  { nome: 'a minha caixa', filhos: [{ nome: 'as minhas ideias', para: '/inovacao/minha-caixa' }] },
  {
    nome: 'sistema de gestão de inovação',
    filhos: [
      { nome: 'validação de ideias', para: '/inovacao/validacao', permissao: Permissoes.ideiasValidar },
      { nome: 'avaliação de ideias', para: '/inovacao/avaliacao', permissao: Permissoes.ideiasAvaliar },
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
  {
    nome: 'repositório do conhecimento',
    filhos: [
      { nome: 'conhecimento codificado', para: '/inovacao/conhecimento' },
      { nome: 'lições aprendidas', para: '/inovacao/licoes' },
    ],
  },
  { nome: 'indicadores de desempenho', filhos: [{ nome: 'KPIs', para: '/inovacao/indicadores' }] },
  {
    nome: 'administração',
    filhos: [
      { nome: 'utilizadores', para: '/inovacao/admin/utilizadores', permissao: Permissoes.gerirUtilizadores },
      { nome: 'perfis e permissões', para: '/inovacao/admin/perfis', permissao: Permissoes.gerirPerfis },
      { nome: 'importar Mod.246', para: '/inovacao/admin/importar', permissao: Permissoes.iniciativasGerir },
      { nome: 'registo de auditoria', para: '/inovacao/admin/auditoria', permissao: Permissoes.verAuditoria },
    ],
  },
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

/** Tira as opções para as quais o utilizador não tem permissão (e os grupos que ficam vazios). */
function menuPara(tem: (p: Permissao) => boolean): ItemMenu[] {
  return menu
    .map((item) => (item.filhos ? { ...item, filhos: item.filhos.filter((f) => !f.permissao || tem(f.permissao)) } : item))
    .filter((item) => !item.filhos || item.filhos.length > 0);
}

export function Layout() {
  const utilizador = useUtilizador();
  const { sair } = useSessao();
  const tem = usePermissao();
  const navegar = useNavigate();

  return (
    <div className="app">
      <nav className="barra" aria-label="Aplicações">
        <Link to="/" className="barra-logo" title="Plataforma de Gestão da Certificação">
          dstelecom
        </Link>
        <div className="barra-avatar" aria-hidden="true">
          {utilizador.nome[0]?.toUpperCase()}
        </div>
        <Avisos />
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
          {menuPara(tem).map((item) => (
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
