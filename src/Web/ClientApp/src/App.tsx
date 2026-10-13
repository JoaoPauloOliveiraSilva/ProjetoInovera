import { useMemo, useState } from 'react';
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { VersaoDados } from './api/usePedido';
import { Layout } from './componentes/Layout';
import { Permissoes } from './api/permissoes';
import { AdminAuditoria } from './paginas/AdminAuditoria';
import { AdminPerfis } from './paginas/AdminPerfis';
import { AdminUtilizadores } from './paginas/AdminUtilizadores';
import { Avaliacao } from './paginas/Avaliacao';
import { CharterPagina } from './paginas/Charter';
import { DefinirPassword } from './paginas/DefinirPassword';
import { EmConstrucao } from './paginas/EmConstrucao';
import { EstrategiaPagina } from './paginas/Estrategia';
import { Entrar } from './paginas/Entrar';
import { Homepage } from './paginas/Homepage';
import { IdeiasLista } from './paginas/IdeiasLista';
import { ImportarModelos } from './paginas/ImportarModelos';
import { IndicadoresPagina } from './paginas/Indicadores';
import { IniciativasLista } from './paginas/IniciativasLista';
import { Landing } from './paginas/Landing';
import { PlanoAnualPagina } from './paginas/PlanoAnual';
import { PlanoKpiPagina } from './paginas/PlanoKpi';
import { AtivosIntangiveis, ConhecimentoTacitoPagina, Ferramentas, Parcerias } from './paginas/RegistosManuais';
import { ValoresExternos } from './paginas/ValoresExternos';
import { RepositorioConhecimento, RepositorioLicoes } from './paginas/Repositorios';
import { Validacao } from './paginas/Validacao';
import { VigilanciasPagina } from './paginas/Vigilancias';
import { Protegido, SessaoProvider } from './sessao/Sessao';

const emConstrucao: [string, string][] = [
  ['i9talks', 'i9talks'],
  ['i9news', 'i9news'],
  ['premios', 'prémios'],
  ['sifide', 'sifide'],
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
            <Route path="/definir-password" element={<DefinirPassword />} />
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
              <Route path="ideias/:id" element={<IdeiasLista modo="todas" />} />
              <Route path="iniciativas" element={<IniciativasLista titulo="iniciativas" />} />
              <Route path="iniciativas/:id" element={<IniciativasLista titulo="iniciativas" />} />
              <Route path="iniciativas/:id/charter" element={<CharterPagina />} />
              <Route path="licoes" element={<RepositorioLicoes />} />
              <Route path="conhecimento" element={<RepositorioConhecimento />} />
              <Route path="conhecimento-tacito" element={<ConhecimentoTacitoPagina />} />
              <Route path="ferramentas" element={<Ferramentas />} />
              <Route path="parcerias" element={<Parcerias />} />
              <Route path="ativos" element={<AtivosIntangiveis />} />
              <Route path="plano-anual" element={<PlanoAnualPagina />} />
              <Route path="estrategia" element={<EstrategiaPagina />} />
              <Route path="indicadores" element={<IndicadoresPagina />} />
              <Route
                path="indicadores/valores-externos"
                element={
                  <Protegido permissao={Permissoes.valoresExternos}>
                    <ValoresExternos />
                  </Protegido>
                }
              />
              <Route path="indicadores/:numero" element={<PlanoKpiPagina />} />
              <Route path="projetos" element={<IniciativasLista titulo="projetos" tipo="Projeto" />} />
              <Route path="desafios" element={<IniciativasLista titulo="desafios" tipo="Desafio" />} />
              <Route path="vigilancias" element={<VigilanciasPagina />} />
              <Route path="oportunidades" element={<IniciativasLista titulo="oportunidades" tipo="Oportunidade" />} />
              <Route
                path="admin/importar"
                element={
                  <Protegido permissao={Permissoes.iniciativasGerir}>
                    <ImportarModelos />
                  </Protegido>
                }
              />
              <Route
                path="validacao"
                element={
                  <Protegido permissao={Permissoes.ideiasValidar}>
                    <Validacao />
                  </Protegido>
                }
              />
              <Route
                path="avaliacao"
                element={
                  <Protegido permissao={Permissoes.ideiasAvaliar}>
                    <Avaliacao />
                  </Protegido>
                }
              />
              <Route
                path="admin/utilizadores"
                element={
                  <Protegido permissao={Permissoes.gerirUtilizadores}>
                    <AdminUtilizadores />
                  </Protegido>
                }
              />
              <Route
                path="admin/perfis"
                element={
                  <Protegido permissao={Permissoes.gerirPerfis}>
                    <AdminPerfis />
                  </Protegido>
                }
              />
              <Route
                path="admin/auditoria"
                element={
                  <Protegido permissao={Permissoes.verAuditoria}>
                    <AdminAuditoria />
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
