import { useState } from 'react'
import IconRail from './components/IconRail'
import Sidebar from './components/Sidebar'
import HomeContent from './components/HomeContent'
import IdeaWizard from './components/IdeaWizard'
import IdeaDetail from './components/IdeaDetail'
import LoginScreen from './components/LoginScreen'
import type { Idea, NewIdea, PageId } from './types'

const INITIAL_IDEAS: Idea[] = [
  { num: '3036', titulo: 'Informação de Recrutamento', author: 'Rui Silva', date: '04/10/2026', likes: 2, comments: [{ author: 'Nuno Manuel Simoes', company: 'fiber t, s.a.', text: 'Excelente ideia, sugeria até incluir um QR code de forma a facilitar o acesso ao website de recrutamento do grupo.', time: 'há 2 horas' }], description: 'Uma proposta para melhorar a divulgação das oportunidades de recrutamento do grupo.', kind: 'melhoria', status: 'em discussão', isPrivate: false },
  { num: '3035', titulo: 'Academia SAP', author: 'Joana Seguro', date: '29/09/2026', likes: 3, comments: [{ author: 'Equipa de inovação', company: 'dstgroup', text: 'Vamos analisar a proposta com as equipas envolvidas.', time: 'há 1 dia' }], description: 'Criar uma academia interna para partilha de conhecimento SAP.', kind: 'novo produto/serviço simples', status: 'em avaliação', isPrivate: false },
  { num: '3034', titulo: 'Gestão partilhada dos carregamentos de veículos elétricos', author: 'Diana Vilaça', date: '29/09/2026', likes: 3, comments: [], description: 'Melhorar a gestão e partilha dos pontos de carregamento.', kind: 'melhoria', status: 'em discussão', isPrivate: false },
  { num: '3031', titulo: 'Sugestão de melhoria de espaço - Copa', author: 'Autor Anónimo', date: '22/09/2026', likes: 7, comments: [], description: 'Melhorias para tornar a copa mais confortável para todos.', kind: 'melhoria', status: 'em discussão', isPrivate: false },
  { num: '3030', titulo: 'Oferta de bilhetes para o MUZEU no aniversário do colaborador', author: 'Ines Pimentel', date: '22/09/2026', likes: 38, comments: [{ author: 'Paula Cristina Santos', company: 'dstgroup', text: 'Uma ótima iniciativa para os colaboradores.', time: 'há 3 dias' }, { author: 'Joana Seguro', company: 'dstgroup', text: 'Concordo, seria uma boa forma de celebrar.', time: 'há 2 dias' }], description: 'Oferecer bilhetes aos colaboradores para assinalar o seu aniversário.', kind: 'novo produto/serviço simples', status: 'aprovada', isPrivate: false },
]

export default function App() {
  const [username, setUsername] = useState(() => localStorage.getItem('innovera.demo.user') ?? '')
  const [page, setPage] = useState<PageId>('home')
  const [ideas, setIdeas] = useState(INITIAL_IDEAS)
  const [drawer, setDrawer] = useState<'wizard' | Idea | null>(null)
  const [toast, setToast] = useState('')

  const openWizard = () => setDrawer('wizard')
  const openDetail = (idea: Idea) => setDrawer(idea)
  const close = () => setDrawer(null)

  const onLogin = (user: string) => {
    localStorage.setItem('innovera.demo.user', user)
    setUsername(user)
  }

  const onLogout = () => {
    localStorage.removeItem('innovera.demo.user')
    setUsername('')
    setPage('home')
  }

  const onSubmitted = (newIdea: NewIdea) => {
    const nextNumber = String(Math.max(...ideas.map(idea => Number(idea.num))) + 1)
    const idea: Idea = {
      num: nextNumber,
      titulo: newIdea.titulo || 'Sem título',
      author: newIdea.autor === 'anon' ? 'Autor Anónimo' : username,
      date: new Date().toLocaleDateString('pt-PT'),
      likes: 0,
      comments: [],
      description: newIdea.descricao,
      kind: newIdea.tipo,
      status: 'em discussão',
      isPrivate: !newIdea.visivel,
    }
    setIdeas(current => [idea, ...current])
    setDrawer(null)
    setToast(`Ideia “${idea.titulo}” registada com sucesso!`)
    setTimeout(() => setToast(''), 2600)
  }

  const toggleLike = (num: string) => setIdeas(current => current.map(idea => idea.num === num ? { ...idea, likes: idea.likes + 1 } : idea))
  const addComment = (num: string, text: string) => {
    const comment = { author: username, company: 'dstgroup', text, time: 'agora' }
    setIdeas(current => current.map(idea => idea.num === num ? { ...idea, comments: [...idea.comments, comment] } : idea))
    setDrawer(current => current && current !== 'wizard' && current.num === num ? { ...current, comments: [...current.comments, comment] } : current)
  }

  if (!username) return <LoginScreen onLogin={onLogin} />

  const activeIdea = drawer && drawer !== 'wizard' ? ideas.find(idea => idea.num === drawer.num) ?? drawer : null

  return (
    <div className="app">
      <IconRail username={username} page={page} onNavigate={setPage} />
      <Sidebar username={username} page={page} onNavigate={setPage} />
      <main className={`main ${drawer ? 'dimmed' : ''}`}>
        {page === 'home' && <HomeContent ideas={ideas} onNewIdea={openWizard} onOpenIdea={openDetail} onToggleLike={toggleLike} />}
        {page === 'projects' && <section className="simple-page"><p className="eyebrow">INOVAÇÃO</p><h1>Projeto developing</h1></section>}
        {page === 'ocienegocio' && <section className="simple-page"><h1>ocienegocio</h1></section>}
        {page === 'profile' && <section className="simple-page"><p className="eyebrow">MINHA ÁREA</p><h1>Olá, {username}</h1><p>Esta é a sua área pessoal.</p><button className="logout-button" onClick={onLogout}>Terminar sessão</button></section>}
        {page === 'help' && <section className="simple-page"><h1>Ajuda</h1></section>}
      </main>

      {drawer === 'wizard' && (
        <IdeaWizard currentUser={username} onClose={close} onSubmitted={onSubmitted} />
      )}
      {activeIdea && (
        <IdeaDetail idea={activeIdea} onClose={close} onToggleLike={() => toggleLike(activeIdea.num)} onAddComment={text => addComment(activeIdea.num, text)} />
      )}

      {toast && <div className="toast">{toast}</div>}
    </div>
  )
}
