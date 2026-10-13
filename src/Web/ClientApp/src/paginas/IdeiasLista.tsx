import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { ideias as apiIdeias } from '../api/servicos';
import { estadoIdeia, nomeClasse, nomeEstado } from '../api/tipos';
import type { ClasseIdeia, EstadoIdeia, Responsabilidade } from '../api/tipos';
import { usePedido } from '../api/usePedido';
import { EstadoPedido } from '../componentes/Estado';
import { Cabecalho } from '../componentes/Layout';
import { Balao, Coracao, Mais, SetaDiagonal } from '../componentes/Icones';
import { formatarData } from '../dados/regras';
import { NovaIdeiaGaveta } from './NovaIdeia';
import { IdeiaGaveta } from './IdeiaDetalhe';

type Modo = 'discussao' | 'todas' | 'minhas';

const titulos: Record<Modo, string> = { discussao: 'ideias', todas: 'ideias', minhas: 'as minhas ideias' };

const anoAtual = new Date().getFullYear();
const anos = Array.from({ length: anoAtual - 2022 }, (_, i) => anoAtual - i);

/**
 * "ideias de inovação › em discussão", "› todas as ideias" (portefólio de ideias com filtros, Mod.246)
 * e "a minha caixa › as minhas ideias". /inovacao/ideias/12 abre logo a ideia 12 (links dos emails e avisos).
 */
export function IdeiasLista({ modo }: { modo: Modo }) {
  const { id } = useParams();
  const navegar = useNavigate();
  const [aberta, setAberta] = useState<number | null>(id ? Number(id) : null);
  const [novaAberta, setNovaAberta] = useState(false);
  const [estado, setEstado] = useState<EstadoIdeia | ''>('');
  const [classe, setClasse] = useState<ClasseIdeia | ''>('');
  const [responsabilidade, setResponsabilidade] = useState<Responsabilidade | ''>('');
  const [ano, setAno] = useState<number | ''>('');
  const [texto, setTexto] = useState('');
  const [procura, setProcura] = useState('');
  const comEstado = modo !== 'discussao';
  const portefolio = modo === 'todas';

  useEffect(() => {
    if (id) setAberta(Number(id));
  }, [id]);

  useEffect(() => {
    const t = setTimeout(() => setProcura(texto), 300);
    return () => clearTimeout(t);
  }, [texto]);

  const filtro = {
    estado: modo === 'discussao' ? ('EmDiscussao' as const) : estado || undefined,
    soMinhas: modo === 'minhas',
    texto: procura,
    classe: classe || undefined,
    responsabilidade: responsabilidade || undefined,
    ano: ano || undefined,
  };
  const { dados, erro, aCarregar } = usePedido(() => apiIdeias.listar(filtro), [
    filtro.estado,
    modo,
    procura,
    classe,
    responsabilidade,
    ano,
  ]);
  const lista = dados ?? [];
  const porConfirmar = lista.filter((i) => i.porConfirmar);

  const fechar = () => {
    setAberta(null);
    if (id) navegar(modo === 'todas' ? '/inovacao/ideias' : '/inovacao', { replace: true });
  };

  return (
    <>
      <Cabecalho
        titulo={titulos[modo]}
        corLampada="#5bb8cc"
        acao={
          <button className="nova-ideia" onClick={() => setNovaAberta(true)}>
            <Mais /> nova ideia
          </button>
        }
      />
      {modo === 'minhas' && porConfirmar.length > 0 && (
        <div className="aviso aviso-topo">
          {porConfirmar.length === 1
            ? 'Há uma ideia à espera que confirme a coautoria.'
            : `Há ${porConfirmar.length} ideias à espera que confirme a coautoria.`}
          <button className="botao-pequeno" onClick={() => setAberta(porConfirmar[0].id)}>
            ver
          </button>
        </div>
      )}
      {comEstado && (
        <div className="filtros filtros-varios">
          <label>
            estado
            <select value={estado} onChange={(e) => setEstado(e.target.value as EstadoIdeia | '')}>
              <option value="">todos</option>
              {(Object.keys(nomeEstado) as EstadoIdeia[]).map((e) => (
                <option key={e} value={e}>
                  {nomeEstado[e]}
                </option>
              ))}
            </select>
          </label>
          {portefolio && (
            <>
              <label>
                classe
                <select value={classe} onChange={(e) => setClasse(e.target.value as ClasseIdeia | '')}>
                  <option value="">todas</option>
                  {(Object.keys(nomeClasse) as ClasseIdeia[]).map((c) => (
                    <option key={c} value={c}>
                      {nomeClasse[c]}
                    </option>
                  ))}
                </select>
              </label>
              <label>
                responsabilidade
                <select value={responsabilidade} onChange={(e) => setResponsabilidade(e.target.value as Responsabilidade | '')}>
                  <option value="">todas</option>
                  <option value="Dstelecom">dstelecom</option>
                  <option value="Dst">dst</option>
                </select>
              </label>
              <label>
                ano
                <select value={ano} onChange={(e) => setAno(e.target.value ? Number(e.target.value) : '')}>
                  <option value="">todos</option>
                  {anos.map((a) => (
                    <option key={a} value={a}>
                      {a}
                    </option>
                  ))}
                </select>
              </label>
              <label>
                procurar
                <input
                  className="filtro-texto"
                  value={texto}
                  onChange={(e) => setTexto(e.target.value)}
                  placeholder="título, código ou n.º da Caixa"
                />
              </label>
            </>
          )}
          {dados && <span className="cinzento-escuro">{lista.length} ideias</span>}
        </div>
      )}
      <section className="tabela">
        <div className={`tabela-linha tabela-cabecalho${comEstado ? ' com-estado' : ''}`}>
          <span>Código</span>
          <span>Título</span>
          <span>Autores</span>
          <span>Data</span>
          {comEstado && <span>Estado</span>}
          <span />
          <span />
          <span />
        </div>
        {lista.map((i) => (
          <div
            key={i.id}
            className={`tabela-linha${comEstado ? ' com-estado' : ''}${i.porConfirmar ? ' destacada' : ''}`}
            onClick={() => setAberta(i.id)}
          >
            <span className="num" title={i.idCaixaInovacao ? `Caixa de Inovação n.º ${i.idCaixaInovacao}` : undefined}>
              {i.codigo}
              {i.idCaixaInovacao && <span className="caixa-id">#{i.idCaixaInovacao}</span>}
            </span>
            <span className="forte">
              {i.titulo}
              {i.privada && <span className="privada"> · privada</span>}
              {i.classe && <span className="privada"> · {nomeClasse[i.classe]}</span>}
            </span>
            <span className="forte">{i.autores}</span>
            <span className="forte">{formatarData(i.data)}</span>
            {comEstado && <span className={`etiqueta ${estadoIdeia(i).classe}`}>{estadoIdeia(i).nome}</span>}
            <span className="contador">
              <Coracao /> {i.likes}
            </span>
            <span className="contador">
              <Balao /> {i.comentarios}
            </span>
            <span className="abrir" aria-label="Abrir">
              <SetaDiagonal />
            </span>
          </div>
        ))}
        <EstadoPedido aCarregar={aCarregar} erro={erro} vazio={dados === undefined} />
        {dados && lista.length === 0 && <div className="vazio">Não há ideias para mostrar.</div>}
      </section>
      <IdeiaGaveta id={aberta} aoFechar={fechar} />
      <NovaIdeiaGaveta aberta={novaAberta} aoFechar={() => setNovaAberta(false)} />
    </>
  );
}
