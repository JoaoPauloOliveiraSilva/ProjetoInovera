import type { SVGProps } from 'react';

type P = SVGProps<SVGSVGElement>;
const base = { fill: 'none', stroke: 'currentColor', strokeWidth: 1.8, strokeLinecap: 'round', strokeLinejoin: 'round' } as const;

export const Coracao = (p: P) => (
  <svg viewBox="0 0 24 24" width="18" height="18" {...base} {...p}>
    <path d="M12 20s-7-4.4-9.2-8.6C1.3 8.4 3.2 5 6.6 5c2 0 3.3 1.1 4.1 2.3h.6C12.1 6.1 13.4 5 15.4 5c3.4 0 5.3 3.4 3.8 6.4C19 15.6 12 20 12 20z" />
  </svg>
);

export const Balao = (p: P) => (
  <svg viewBox="0 0 24 24" width="18" height="18" {...base} {...p}>
    <path d="M4 12a8 8 0 1 1 3.3 6.5L4 20l1.1-3.6A8 8 0 0 1 4 12z" />
  </svg>
);

export const SetaDiagonal = (p: P) => (
  <svg viewBox="0 0 24 24" width="16" height="16" {...base} {...p}>
    <path d="M7 17 17 7M9 7h8v8" />
  </svg>
);

export const SetaContinuar = (p: P) => (
  <svg viewBox="0 0 24 24" width="16" height="16" {...base} {...p}>
    <path d="M7 7l10 10M17 9v8H9" />
  </svg>
);

export const Fechar = (p: P) => (
  <svg viewBox="0 0 24 24" width="22" height="22" {...base} {...p}>
    <path d="M6 6l12 12M18 6 6 18" />
  </svg>
);

export const Visto = (p: P) => (
  <svg viewBox="0 0 24 24" width="16" height="16" {...base} strokeWidth={2.4} {...p}>
    <path d="m5 12 4.5 4.5L19 7" />
  </svg>
);

export const Mais = (p: P) => (
  <svg viewBox="0 0 24 24" width="18" height="18" {...base} strokeWidth={2.2} {...p}>
    <path d="M12 5v14M5 12h14" />
  </svg>
);

export const Chevron = (p: P) => (
  <svg viewBox="0 0 24 24" width="12" height="12" {...base} {...p}>
    <path d="m9 6 6 6-6 6" />
  </svg>
);

export const Esquerda = (p: P) => (
  <svg viewBox="0 0 24 24" width="18" height="18" {...base} {...p}>
    <path d="M19 12H5M11 6l-6 6 6 6" />
  </svg>
);

export const Direita = (p: P) => (
  <svg viewBox="0 0 24 24" width="18" height="18" {...base} {...p}>
    <path d="M5 12h14M13 6l6 6-6 6" />
  </svg>
);

export const Camara = (p: P) => (
  <svg viewBox="0 0 24 24" width="18" height="18" {...base} {...p}>
    <path d="M4 8h3l2-2h6l2 2h3v11H4z" />
    <circle cx="12" cy="13" r="3.2" />
  </svg>
);

export const Upload = (p: P) => (
  <svg viewBox="0 0 24 24" width="18" height="18" {...base} {...p}>
    <path d="M7 18a4 4 0 0 1-.6-8 6 6 0 0 1 11.4 1.6A3.5 3.5 0 0 1 17 18M12 12v8M9 15l3-3 3 3" />
  </svg>
);

export const Lampada = ({ cor = '#c97b63', ...p }: P & { cor?: string }) => (
  <svg viewBox="0 0 64 64" width="64" height="64" {...p}>
    <path d="M32 6c-11 0-19 8.4-19 19 0 7 3.5 11.6 7 15.2 2.2 2.3 3.4 4.6 3.4 7.3V50h17.2v-2.5c0-2.7 1.2-5 3.4-7.3 3.5-3.6 7-8.2 7-15.2C51 14.4 43 6 32 6z" fill={cor} />
    <path d="M24 22l8 16 8-16" fill="none" stroke="#1d1d1d" strokeWidth="2" strokeLinejoin="round" />
    <path d="M32 38v12" stroke="#1d1d1d" strokeWidth="2" />
    <path d="M24 54h16M27 58h10" stroke="#1d1d1d" strokeWidth="2.4" strokeLinecap="round" />
  </svg>
);

// Ícones da barra escura e da landing page
export const IconeLivros = (p: P) => (
  <svg viewBox="0 0 32 32" width="30" height="30" {...base} strokeWidth={1.6} {...p}>
    <path d="M5 6h5v20H5zM11 6h5v20h-5zM18 7l4.6-1.2 5 19.4-4.6 1.2z" />
  </svg>
);

export const IconeInovacao = (p: P) => (
  <svg viewBox="0 0 32 32" width="30" height="30" {...base} strokeWidth={1.6} {...p}>
    <path d="M16 4v3M7 8l2 2M25 8l-2 2" />
    <path d="M10 16a6 6 0 1 1 12 0c0 2-1 3.4-2 4.4V23h-8v-2.6c-1-1-2-2.4-2-4.4z" />
    <path d="M6 26h20l-2 3H8z" />
  </svg>
);

export const IconeMinhaArea = (p: P) => (
  <svg viewBox="0 0 32 32" width="30" height="30" {...base} strokeWidth={1.6} {...p}>
    <circle cx="16" cy="9" r="4" />
    <path d="M7 27v-4a5 5 0 0 1 5-5h8a5 5 0 0 1 5 5v4zM7 22h18" />
  </svg>
);

export const IconeAjuda = (p: P) => (
  <svg viewBox="0 0 32 32" width="34" height="34" {...base} strokeWidth={1.6} {...p}>
    <circle cx="16" cy="16" r="13" />
    <path d="M12.5 12.5a3.5 3.5 0 1 1 5 3.2c-1 .5-1.5 1.2-1.5 2.3v1M16 23v.5" />
  </svg>
);

export const IconeCertificado = (p: P) => (
  <svg viewBox="0 0 64 64" width="74" height="74" fill="currentColor" {...p}>
    <path d="M18 10h8a6 6 0 0 1 12 0h8v46H18zm4 4v38h20V14h-4v4H26v-4zm10-6.5A2.5 2.5 0 1 0 32 12a2.5 2.5 0 0 0 0-4.5zM26 22h12v3H26zm0 6h8v3h-8zm-1 12h7v3h-7zm14-4a5 5 0 0 1 3 9l1.5 5-4.5-2-4.5 2 1.5-5a5 5 0 0 1 3-9zm0 3a2 2 0 1 0 0 4 2 2 0 0 0 0-4z" />
  </svg>
);

export const IconeLampadaLanding = (p: P) => (
  <svg viewBox="0 0 64 64" width="74" height="74" {...base} strokeWidth={3.4} {...p}>
    <path d="M32 6v5M14 13l3.5 3.5M50 13l-3.5 3.5M8 30h5M51 30h5M14 47l3.5-3.5M50 47l-3.5-3.5" />
    <path d="M22 32a10 10 0 1 1 20 0c0 4-2 6-4 8v4H26v-4c-2-2-4-4-4-8z" />
    <path d="M26 50h12M28 55h8" />
  </svg>
);

export const IconeFamilia = (p: P) => (
  <svg viewBox="0 0 64 64" width="84" height="74" fill="currentColor" {...p}>
    <circle cx="25" cy="14" r="4" />
    <circle cx="39" cy="14" r="4" />
    <circle cx="13" cy="28" r="3" />
    <circle cx="51" cy="28" r="3" />
    <path d="M21 20h8l2 14h-3l-1 16h-4l-1-16h-3zM35 20h8l2 14h-3l-1 16h-4l-1-16h-3zM10 33h6l1 9h-2l-1 8h-3l-1-8H8zM48 33h6l2 9h-2l-1 8h-3l-1-8h-2z" />
  </svg>
);

export const IconeCarrinho = (p: P) => (
  <svg viewBox="0 0 64 64" width="74" height="74" fill="currentColor" {...p}>
    <path d="M6 8h8l3 6h40l-5 24H19l2 6h31v4H18L9 12H6zm13 10 4 16h26l3-16zm3 3h7v4h-7zm10 0h7v4h-7zm10 0h7v4h-7zM23 28h7v4h-7zm10 0h7v4h-7zm10 0h6v4h-6z" />
    <circle cx="22" cy="54" r="4" />
    <circle cx="48" cy="54" r="4" />
  </svg>
);

export const IconeSair = (p: P) => (
  <svg viewBox="0 0 24 24" width="26" height="26" {...base} {...p}>
    <path d="M14 4h4a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2h-4M10 16l4-4-4-4M14 12H4" />
  </svg>
);
