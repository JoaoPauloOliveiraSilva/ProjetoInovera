import { ChevronRight } from './icons'
import type { PageId } from '../types'

interface SidebarProps {
  username: string
  page: PageId
  onNavigate: (page: PageId) => void
}

export default function Sidebar({ username, page, onNavigate }: SidebarProps) {
  return (
    <aside className="sidebar">
      <div className="sidebar-greeting">olá, {username}</div>
      <button className={`side-link ${page === 'home' ? 'active' : ''}`} onClick={() => onNavigate('home')} aria-current={page === 'home' ? 'page' : undefined}>
        <span>homepage</span>
      </button>
      <button className={`side-link ${page === 'projects' ? 'active' : ''}`} onClick={() => onNavigate('projects')} aria-current={page === 'projects' ? 'page' : undefined}>
        <span>projeto</span><ChevronRight size={13} />
      </button>
    </aside>
  )
}
