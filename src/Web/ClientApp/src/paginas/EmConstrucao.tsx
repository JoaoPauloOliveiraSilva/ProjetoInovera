import { Cabecalho } from '../componentes/Layout';

export function EmConstrucao({ titulo }: { titulo: string }) {
  return (
    <>
      <Cabecalho titulo={titulo} />
      <section className="painel vazio">Esta área ainda está em construção.</section>
    </>
  );
}
