import { useState } from 'react';
import type { EvolucaoKpi, UnidadeKpi } from '../api/tipos';

// Evolução anual de um KPI: uma coluna por ano (valor oficial do Mod.239 nos anos fechados, calculado nos outros)
// e um traço com o objetivo do ano. O ano em curso fica mais claro ("até hoje"). Por baixo, a tabela com os mesmos valores.

const COR = '#2f6fa8';
const TINTA = '#1f1f1f';
const GRELHA = '#d9d9d9';
const SUAVE = '#6b6b6b';

const largura = 560;
const altura = 220;
const margem = { cima: 22, direita: 12, baixo: 34, esquerda: 52 };

/** O número de um objetivo escrito como no Mod.239 ("≥ 30", "> 1 000 000€", "≥ 80%", "2,5"). */
export function numeroObjetivo(objetivo: string | null): number | null {
  if (!objetivo) return null;
  let t = objetivo.replace(/[^\d.,-]/g, '');
  if (t === '') return null;
  if (t.includes(',')) t = t.replace(/\./g, '').replace(',', '.');
  else if ((t.match(/\./g) ?? []).length > 1 || /\.\d{3}$/.test(t)) t = t.replace(/\./g, '');
  const n = Number(t);
  return Number.isFinite(n) ? n : null;
}

/** Valor completo (tabela e tooltip). */
export function formatarValor(unidade: UnidadeKpi, valor: number | null): string {
  if (valor === null) return '—';
  switch (unidade) {
    case 'Percentagem':
      return `${valor.toLocaleString('pt-PT', { maximumFractionDigits: 1 })} %`;
    case 'Euros':
      return valor.toLocaleString('pt-PT', { style: 'currency', currency: 'EUR', maximumFractionDigits: 0 });
    case 'Media':
      return valor.toLocaleString('pt-PT', { maximumFractionDigits: 1 });
    default:
      return valor.toLocaleString('pt-PT', { maximumFractionDigits: 0 });
  }
}

/** Valor curto para o eixo (0 / 500 mil / 1 M). */
function formatarEixo(unidade: UnidadeKpi, valor: number): string {
  const curto = valor.toLocaleString('pt-PT', { notation: 'compact', maximumFractionDigits: 1 });
  return unidade === 'Percentagem' ? `${curto}%` : unidade === 'Euros' ? `${curto} €` : curto;
}

/** Máximo "redondo" do eixo e o passo entre linhas da grelha. */
function escala(maximo: number): { topo: number; passo: number } {
  if (maximo <= 0) return { topo: 1, passo: 1 };
  const bruto = maximo / 4;
  const potencia = 10 ** Math.floor(Math.log10(bruto));
  const passo = [1, 2, 2.5, 5, 10].map((m) => m * potencia).find((p) => p >= bruto) ?? bruto;
  return { topo: Math.ceil(maximo / passo) * passo, passo };
}

/** Coluna com o topo arredondado (4px) e a base reta. */
function coluna(x: number, y: number, w: number, h: number): string {
  if (h <= 0) return '';
  const r = Math.min(4, w / 2, h);
  return `M${x},${y + h} V${y + r} Q${x},${y} ${x + r},${y} H${x + w - r} Q${x + w},${y} ${x + w},${y + r} V${y + h} Z`;
}

export function GraficoEvolucao({ evolucao }: { evolucao: EvolucaoKpi }) {
  const [ativo, setAtivo] = useState<number | null>(null);
  const pontos = evolucao.pontos.map((p) => ({ ...p, alvo: numeroObjetivo(p.objetivo) }));
  const comValor = pontos.filter((p) => p.valor !== null);

  if (comValor.length === 0) {
    return <p className="cinzento-escuro">{evolucao.nota ?? 'Ainda não há valores para mostrar a evolução.'}</p>;
  }

  const maximo = Math.max(...pontos.flatMap((p) => [p.valor ?? 0, p.alvo ?? 0]));
  const { topo, passo } = escala(maximo);
  const plotL = largura - margem.esquerda - margem.direita;
  const plotA = altura - margem.cima - margem.baixo;
  const faixa = plotL / pontos.length;
  const larguraColuna = Math.min(24, faixa * 0.45);
  const y = (v: number) => margem.cima + plotA - (v / topo) * plotA;
  const linhas = Array.from({ length: Math.round(topo / passo) + 1 }, (_, i) => i * passo);
  const ultimo = pontos.length - 1 - [...pontos].reverse().findIndex((p) => p.valor !== null);
  const temObjetivo = pontos.some((p) => p.alvo !== null);
  const sel = ativo === null ? null : pontos[ativo];

  return (
    <figure className="grafico-evolucao">
      <div className="grafico-legenda" aria-hidden="true">
        <span>
          <i className="chave-coluna" style={{ background: COR }} /> valor do ano
        </span>
        {temObjetivo && (
          <span>
            <i className="chave-objetivo" style={{ background: TINTA }} /> objetivo
          </span>
        )}
        {pontos.some((p) => p.parcial) && (
          <span>
            <i className="chave-coluna" style={{ background: COR, opacity: 0.35 }} /> ano em curso (até hoje)
          </span>
        )}
      </div>
      <div className="grafico-area">
        <svg
          viewBox={`0 0 ${largura} ${altura}`}
          role="img"
          aria-label={`Evolução anual do KPI ${evolucao.numero}: ${comValor.map((p) => `${p.ano} ${formatarValor(evolucao.unidade, p.valor)}`).join(', ')}`}
          onMouseLeave={() => setAtivo(null)}
        >
          {linhas.map((v) => (
            <g key={v}>
              <line x1={margem.esquerda} x2={largura - margem.direita} y1={y(v)} y2={y(v)} stroke={GRELHA} strokeWidth={1} />
              <text x={margem.esquerda - 8} y={y(v) + 4} textAnchor="end" fontSize={11} fill={SUAVE}>
                {formatarEixo(evolucao.unidade, v)}
              </text>
            </g>
          ))}
          {pontos.map((p, i) => {
            const cx = margem.esquerda + faixa * i + faixa / 2;
            const topoColuna = p.valor === null ? null : y(p.valor);
            return (
              <g key={p.ano}>
                {topoColuna !== null && (
                  <path
                    d={coluna(cx - larguraColuna / 2, topoColuna, larguraColuna, margem.cima + plotA - topoColuna)}
                    fill={COR}
                    opacity={p.parcial ? 0.35 : ativo === null || ativo === i ? 1 : 0.55}
                  />
                )}
                {p.alvo !== null && (
                  <line
                    x1={cx - larguraColuna / 2 - 7}
                    x2={cx + larguraColuna / 2 + 7}
                    y1={y(p.alvo)}
                    y2={y(p.alvo)}
                    stroke={TINTA}
                    strokeWidth={2}
                    strokeLinecap="round"
                  />
                )}
                {i === ultimo && topoColuna !== null && (
                  <text x={cx} y={Math.min(topoColuna, p.alvo !== null ? y(p.alvo) : topoColuna) - 7} textAnchor="middle" fontSize={11}
                    fontWeight={600} fill={TINTA}>
                    {formatarValor(evolucao.unidade, p.valor)}
                  </text>
                )}
                <text x={cx} y={altura - margem.baixo + 18} textAnchor="middle" fontSize={12} fill={p.parcial ? SUAVE : TINTA}>
                  {p.ano}
                </text>
                {/* Zona de hover maior do que a coluna. */}
                <rect
                  x={margem.esquerda + faixa * i}
                  y={margem.cima}
                  width={faixa}
                  height={plotA}
                  fill="transparent"
                  onMouseEnter={() => setAtivo(i)}
                  onFocus={() => setAtivo(i)}
                  tabIndex={0}
                  aria-label={`${p.ano}: ${formatarValor(evolucao.unidade, p.valor)}`}
                />
              </g>
            );
          })}
          <line
            x1={margem.esquerda}
            x2={largura - margem.direita}
            y1={margem.cima + plotA}
            y2={margem.cima + plotA}
            stroke="#bdbdbd"
            strokeWidth={1}
          />
        </svg>
        {sel && ativo !== null && (
          <div
            className="grafico-dica"
            style={{
              left: `${((margem.esquerda + faixa * ativo + faixa / 2) / largura) * 100}%`,
              top: `${(Math.min(sel.valor === null ? margem.cima + plotA : y(sel.valor), sel.alvo === null ? Infinity : y(sel.alvo)) / altura) * 100}%`,
            }}
            role="tooltip"
          >
            <strong>{sel.ano}</strong>
            <span>{formatarValor(evolucao.unidade, sel.valor)}</span>
            <span className="cinzento-escuro">{sel.oficial ? 'valor do Mod.239' : sel.parcial ? 'até hoje' : 'calculado na plataforma'}</span>
            {sel.objetivo && <span>objetivo {sel.objetivo}</span>}
          </div>
        )}
      </div>
      <table className="grafico-tabela">
        <thead>
          <tr>
            <th>Ano</th>
            <th>Valor</th>
            <th>Origem</th>
            <th>Objetivo</th>
            <th>Atingido</th>
          </tr>
        </thead>
        <tbody>
          {pontos.map((p) => (
            <tr key={p.ano}>
              <td className="num">{p.ano}</td>
              <td>{formatarValor(evolucao.unidade, p.valor)}</td>
              <td className="cinzento-escuro">{p.oficial ? 'Mod.239' : p.parcial ? 'até hoje' : 'calculado'}</td>
              <td>{p.objetivo ?? '—'}</td>
              <td>{p.atingido === null ? '—' : p.atingido ? '✓ sim' : '✗ não'}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </figure>
  );
}
