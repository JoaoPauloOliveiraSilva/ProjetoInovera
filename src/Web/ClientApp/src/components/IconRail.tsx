import { IconBooks, IconHelp, IconUser } from './icons'
import type { PageId } from '../types'

interface IconRailProps {
  username: string
  page: PageId
  onNavigate: (page: PageId) => void
}

export default function IconRail({ username, page, onNavigate }: IconRailProps) {
  return (
    <nav className="icon-rail">
      <div className="rail-logo">dstgroup</div>
      <button className={`rail-item user-rail-item ${page === 'profile' ? 'active' : ''}`} onClick={() => onNavigate('profile')} aria-label={`Abrir área de ${username}`}>
        <span className="rail-avatar">{username.slice(0, 1).toUpperCase()}</span>
        <span className="rail-label">{username}</span>
      </button>
      <button className={`rail-item ${page === 'ocienegocio' ? 'active' : ''}`} onClick={() => onNavigate('ocienegocio')}>
        <IconBooks />
        <span className="rail-label">ocienegocio</span>
      </button>
      <button className={`rail-item ${page === 'home' || page === 'projects' ? 'active' : ''}`} onClick={() => onNavigate('home')}>
        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
          <path d="M9 18h6M10 21h4" />
          <path d="M12 3a6 6 0 0 0-3.5 10.9c.8.6 1.5 1.4 1.5 2.1h4c0-.7.7-1.5 1.5-2.1A6 6 0 0 0 12 3Z" />
        </svg>
        <span className="rail-label">inovação</span>
      </button>
      <button className={`rail-item ${page === 'profile' ? 'active' : ''}`} onClick={() => onNavigate('profile')}>
        <IconUser />
        <span className="rail-label">minha área</span>
      </button>
      <div className="rail-spacer" />
      <button className={`rail-item ${page === 'help' ? 'active' : ''}`} onClick={() => onNavigate('help')}>
        <IconHelp />
        <span className="rail-label">ajuda</span>
      </button>
    </nav>
  )
}
