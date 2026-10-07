import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { Layout } from './componentes/Layout';
import { IdeiasProvider } from './dados/IdeiasContext';
import { Avaliacao } from './paginas/Avaliacao';
import { EmConstrucao } from './paginas/EmConstrucao';
import { Homepage } from './paginas/Homepage';
import { IdeiasLista } from './paginas/IdeiasLista';
import { Landing } from './paginas/Landing';

const emConstrucao: [string, string][] = [
  ['minha-caixa', 'a minha caixa'],
  ['i9talks', 'i9talks'],
  ['i9news', 'i9news'],
  ['premios', 'prémios'],
  ['desafios', 'desafios'],
  ['vigilancias', 'vigilâncias'],
  ['projetos', 'projetos'],
  ['sifide', 'sifide'],
  ['iniciativas', 'iniciativas'],
  ['oportunidades', 'oportunidades'],
  ['conhecimento', 'conhecimento'],
  ['indicadores', 'indicadores'],
];

export function App() {
  return (
    <IdeiasProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<Landing />} />
          <Route path="/inovacao" element={<Layout />}>
            <Route index element={<Homepage />} />
            <Route path="ideias" element={<IdeiasLista soEmDiscussao={false} />} />
            <Route path="ideias/discussao" element={<IdeiasLista soEmDiscussao />} />
            <Route path="avaliacao" element={<Avaliacao />} />
            {emConstrucao.map(([caminho, titulo]) => (
              <Route key={caminho} path={caminho} element={<EmConstrucao titulo={titulo} />} />
            ))}
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </IdeiasProvider>
  );
}
