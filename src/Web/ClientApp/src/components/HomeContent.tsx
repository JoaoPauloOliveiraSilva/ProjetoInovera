import { BulbLogo, Heart, Comment, ArrowUpRight } from './icons'
import { Button } from '@fluentui/react-components'
import type { Idea } from '../types'

interface HomeContentProps {
  ideas: Idea[]
  onNewIdea: () => void
  onOpenIdea: (idea: Idea) => void
  onToggleLike: (num: string) => void
}

export default function HomeContent({ ideas, onNewIdea, onOpenIdea, onToggleLike }: HomeContentProps) {
  const featured = ideas[0]
  return (
    <div>
      <section className="hero">
        <div className="hero-bulb-wrap">
          <BulbLogo size={130} />
        </div>
        <div className="hero-text">
          <div className="eyebrow">INOVAÇÃO</div>
          <h1>homepage</h1>
          <Button appearance="subtle" className="hero-new" onClick={onNewIdea}>
            <span className="plus">+</span> nova ideia
          </Button>
        </div>
      </section>

      <section className="ideas-section">
        <div className="ideas-head">
          <h2>ideias</h2>
          <button className="arrow-btn">←</button>
          <button className="arrow-btn">→</button>
        </div>

        {featured && <article className="idea-card">
          <button className="card-open" onClick={() => onOpenIdea(featured)}>{featured.titulo}<ArrowUpRight size={16} /></button>
          <div className="card-meta">
            <button className="table-action" onClick={() => onToggleLike(featured.num)} aria-label={`Gosto, ${featured.likes}`}><Heart size={17} /> {featured.likes}</button>
            <button className="table-action" onClick={() => onOpenIdea(featured)} aria-label={`Comentários, ${featured.comments.length}`}><Comment size={17} /> {featured.comments.length}</button>
          </div>
        </article>}

        <table className="idea-table">
          <thead>
            <tr><th>Num</th><th>Título</th><th>Autores</th><th>Data</th><th aria-label="Interações" /></tr>
          </thead>
          <tbody>
            {ideas.map(idea => (
              <tr key={idea.num}>
                <td className="num">{idea.num}</td>
                <td><button className="idea-title-button" onClick={() => onOpenIdea(idea)}>{idea.titulo}</button></td>
                <td>{idea.author}</td>
                <td>{idea.date}</td>
                <td className="idea-actions">
                  <button className="table-action" onClick={() => onToggleLike(idea.num)} aria-label={`Gosto, ${idea.likes}`}><Heart size={17} />{idea.likes}</button>
                  <button className="table-action" onClick={() => onOpenIdea(idea)} aria-label={`Comentários, ${idea.comments.length}`}><Comment size={17} />{idea.comments.length}</button>
                  <button className="table-action jump-action" onClick={() => onOpenIdea(idea)} aria-label={`Abrir ${idea.titulo}`}><ArrowUpRight size={17} /></button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>
    </div>
  )
}
