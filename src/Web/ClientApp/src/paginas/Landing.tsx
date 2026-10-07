import type { ReactNode } from 'react';
import { Link } from 'react-router-dom';
import { IconeCarrinho, IconeCertificado, IconeFamilia, IconeLampadaLanding } from '../componentes/Icones';

interface Area {
  nome: string;
  icone: ReactNode;
  para?: string;
}

// Mockup Landing Page.pdf: só a Inovação está ativa por agora.
const areas: Area[] = [
  { nome: 'Qualidade Ambiente e Segurança', icone: <IconeCertificado /> },
  { nome: 'Inovação', icone: <IconeLampadaLanding />, para: '/inovacao' },
  { nome: 'Conciliação Profissional e Familiar', icone: <IconeFamilia /> },
  { nome: 'Compras Sustentáveis', icone: <IconeCarrinho /> },
];

export function Landing() {
  return (
    <div className="landing">
      <h1 className="landing-titulo">Plataforma de Gestão da Certificação</h1>
      <div className="landing-areas">
        {areas.map((a) =>
          a.para ? (
            <Link key={a.nome} to={a.para} className="landing-area">
              <span className="landing-cartao">{a.icone}</span>
              <span className="landing-nome">{a.nome}</span>
            </Link>
          ) : (
            <div key={a.nome} className="landing-area desativada" aria-disabled="true" title="Brevemente">
              <span className="landing-cartao">{a.icone}</span>
              <span className="landing-nome">{a.nome}</span>
            </div>
          ),
        )}
      </div>
    </div>
  );
}
