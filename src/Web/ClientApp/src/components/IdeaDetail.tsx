import { useState, type FormEvent } from 'react'
import { Button } from '@fluentui/react-components'
import { X, Heart, Comment, ArrowDownRight, Avatar } from './icons'
import type { Idea } from '../types'

interface IdeaDetailProps {
  idea: Idea
  onClose: () => void
  onToggleLike: () => void
  onAddComment: (text: string) => void
}

const TIMELINE = [
  { label: '01. validação dos autores', active: false },
  { label: '02. validação equipa inovação', active: false },
  { label: '03. discussão publica', active: true },
]

export default function IdeaDetail({ idea, onClose, onToggleLike, onAddComment }: IdeaDetailProps) {
  const [comment, setComment] = useState('')

  function submitComment(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const text = comment.trim()
    if (!text) return
    onAddComment(text)
    setComment('')
  }

  return (
    <div>
      <div className="drawer-scrim" onClick={onClose} />
      <div className="drawer">
        <button className="drawer-close" onClick={onClose}><X /></button>
        <div className="detail">
          <div className="eyebrow">INOVAÇÃO</div>
          <h1>{idea.titulo}</h1>
          <div className="subtitle">{idea.kind}</div>

          <div className="detail-top">
            <span><Heart /> {idea.likes}</span>
            <span><Comment /> {idea.comments.length}</span>
          </div>

          <div className="info-card">
            <div className="col-left">
              <Avatar size={64} seed={1} />
              <div className="info-row" style={{ marginTop: 14 }}>
                <div className="k">autor</div>
                <div className="v">{idea.author}</div>
              </div>
              <div className="info-row">
                <div className="k">data</div>
                <div className="v">{idea.date}</div>
              </div>
              <div className="info-row">
                <div className="k">estado</div>
                <div className="v">{idea.status}</div>
              </div>
              <div className="info-row">
                <div className="k">privada</div>
                <div className="v">{idea.isPrivate ? 'sim' : 'não'}</div>
              </div>
            </div>
            <div className="timeline">
              {TIMELINE.map((t, i) => (
                <div key={t.label}>
                  <div className={`tl-node ${t.active ? 'active' : ''}`}>{t.label}</div>
                  {i < TIMELINE.length - 1 && <div className="tl-line" />}
                </div>
              ))}
            </div>
          </div>

          <h3 className="section-h">descrição</h3>
          <p className="detail-description">{idea.description || 'Sem descrição.'}</p>

          <Button appearance="secondary" className="pill full" onClick={onToggleLike}>
            <Heart /> gosto · {idea.likes}
          </Button>

          <h3 className="section-h" id="idea-comments">comentários ({idea.comments.length})</h3>
          <form onSubmit={submitComment}>
            <label className="sr-only" htmlFor="idea-comment-input">Escreva um comentário</label>
            <textarea id="idea-comment-input" className="comment-input" value={comment} onChange={event => setComment(event.target.value)} placeholder="Escreva um comentário..." />
            <Button type="submit" appearance="primary" className="pill full" disabled={!comment.trim()}>
              comentar <ArrowDownRight />
            </Button>
          </form>

          <div className="comments-list">
            {idea.comments.map((item, index) => (
              <article className="comment-box" key={`${item.author}-${index}`}>
                <Avatar size={46} seed={index + 1} />
                <div className="body">
                  <div className="name">{item.author}</div>
                  <div className="company">{item.company}</div>
                  <div className="text">{item.text}</div>
                  <div className="comment-foot"><span>gosto</span><span>{item.time}</span></div>
                </div>
                <Heart style={{ alignSelf: 'flex-start' }} />
              </article>
            ))}
            {idea.comments.length === 0 && <p className="no-comments">Ainda não há comentários. Seja o primeiro a participar.</p>}
          </div>
        </div>
      </div>
    </div>
  )
}
