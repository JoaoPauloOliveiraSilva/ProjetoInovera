import { useMemo, useState } from 'react';
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { VersaoDados } from './api/usePedido';
import { Layout } from './componentes/Layout';
import { Avaliacao } from './paginas/Avaliacao';
import { EmConstrucao } from './paginas/EmConstrucao';
import { Entrar } from './paginas/Entrar';
import { Homepage } from './paginas/Homepage';
import { IdeiasLista } from './paginas/IdeiasLista';
import { Landing } from './paginas/Landing';
import { Validacao } from './paginas/Validacao';
import { Protegido, SessaoProvider } from './sessao/Sessao';

const emConstrucao: [string, string][] = [
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
  const [versao, setVersao] = useState(0);
  const dados = useMemo(() => ({ versao, atualizar: () => setVersao((v) => v + 1) }), [versao]);

  return (
    <SessaoProvider>
      <VersaoDados.Provider value={dados}>
        <BrowserRouter>
          <Routes>
            <Route path="/" element={<Landing />} />
            <Route path="/entrar" element={<Entrar />} />
            <Route
              path="/inovacao"
              element={
                <Protegido>
                  <Layout />
                </Protegido>
              }
            >
              <Route index element={<Homepage />} />
              <Route path="minha-caixa" element={<IdeiasLista modo="minhas" />} />
              <Route path="ideias" element={<IdeiasLista modo="todas" />} />
              <Route path="ideias/discussao" element={<IdeiasLista modo="discussao" />} />
              <Route
                path="validacao"
                element={
                  <Protegido soAdministrador>
                    <Validacao />
                  </Protegido>
                }
              />
              <Route
                path="avaliacao"
                element={
                  <Protegido soAdministrador>
                    <Avaliacao />
                  </Protegido>
                }
              />
              {emConstrucao.map(([caminho, titulo]) => (
                <Route key={caminho} path={caminho} element={<EmConstrucao titulo={titulo} />} />
              ))}
            </Route>
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </BrowserRouter>
      </VersaoDados.Provider>
    </SessaoProvider>
  );
}
