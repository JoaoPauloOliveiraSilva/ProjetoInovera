import { useState, type ChangeEvent } from 'react'
import { Button } from '@fluentui/react-components'
import { X, ArrowDownRight, ArrowUpRight, Camera, Check, PaperPlane } from './icons'
import type { NewIdea } from '../types'

interface IdeaWizardProps {
  currentUser: string
  onClose: () => void
  onSubmitted: (idea: NewIdea) => void
}

interface IdeaForm {
  titulo: string
  descricao: string
  visivel: boolean
  autor: 'current' | 'anon' | 'other'
  vantagens: string
  requisitos: string
  como: string
  modelo: string
  competidores: string
  custos: string
  termos: boolean
}

type TextField = Exclude<keyof IdeaForm, 'visivel' | 'autor' | 'termos'>

const IDEA_TYPES = [
  'melhoria',
  'novo produto/serviço simples',
  'novo produto/serviço detalhado',
]

const STEPS = ['tipo de ideia', 'informação geral', 'informação detalhada', 'termos e condições']

export default function IdeaWizard({ currentUser, onClose, onSubmitted }: IdeaWizardProps) {
  const [step, setStep] = useState(1)
  const [ideaType, setIdeaType] = useState<string | null>(null)
  const [termsExpanded, setTermsExpanded] = useState(false)
  const [form, setForm] = useState<IdeaForm>({
    titulo: '',
    descricao: '',
    visivel: true,
    autor: 'current',
    vantagens: '',
    requisitos: '',
    como: '',
    modelo: '',
    competidores: '',
    custos: '',
    termos: false,
  })

  const set = (key: TextField) => (event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    setForm((current) => ({ ...current, [key]: event.target.value }))
  }

  const next = () => setStep((s) => Math.min(4, s + 1))
  const back = () => setStep((s) => Math.max(1, s - 1))

  const submit = () => {
    onSubmitted({ titulo: form.titulo || 'Sem título', descricao: form.descricao, tipo: ideaType ?? 'melhoria', autor: form.autor, visivel: form.visivel })
  }

  return (
    <div>
      <div className="drawer-scrim" onClick={onClose} />
      <div className="drawer">
        <button className="drawer-close" onClick={onClose}><X /></button>
        <div className="wizard">
          {/* stepper */}
          <div className="stepper">
            {STEPS.map((t, i) => {
              const n = i + 1
              const state = n < step ? 'done' : n === step ? 'current' : 'todo'
              return (
                <div className="step-block" key={t}>
                  <div className="step-row">
                    <div className={`step-circle ${state}`}>
                      {state === 'done' ? <Check size={15} color="#fff" /> : n}
                    </div>
                    <div className="step-title">{t}</div>
                  </div>
                  {i < STEPS.length - 1 && <div className="step-line" />}
                </div>
              )
            })}
          </div>

          {/* body */}
          <div className="wizard-body" style={{ paddingTop: (step - 1) * 100 }}>
            {step === 1 && (
              <>
                {IDEA_TYPES.map((t) => (
                  <button
                    key={t}
                    className={`radio-opt ${ideaType === t ? 'selected' : ''}`}
                    onClick={() => setIdeaType(t)}
                  >
                    <span className="radio-dot" />
                    {t}
                  </button>
                ))}
                <Button appearance="primary" className="pill full" onClick={next} disabled={!ideaType}>
                  continuar <ArrowDownRight />
                </Button>
              </>
            )}

            {step === 2 && (
              <>
                <label className="field-label" style={{ marginTop: 0 }}>título</label>
                <input className="text-input" value={form.titulo} onChange={set('titulo')} />

                <label className="field-label">descrição</label>
                <textarea className="text-area" value={form.descricao} onChange={set('descricao')} />

                <div className="check-row">
                  <button
                    className={`checkbox ${form.visivel ? 'checked' : ''}`}
                    onClick={() => setForm((f) => ({ ...f, visivel: !f.visivel }))}
                  >
                    {form.visivel && <Check size={13} color="#fff" />}
                  </button>
                  <span>Visível para todos</span>
                </div>

                <label className="field-label">autor ou autores da ideia</label>
                {([
                  ['current', `Autor é ${currentUser}`],
                  ['anon', 'Autor é anónimo'],
                  ['other', 'Outro autor ou autores'],
                ] as const).map(([v, label]) => (
                  <button
                    key={v}
                    className={`author-opt ${form.autor === v ? 'selected' : ''}`}
                    onClick={() => setForm((f) => ({ ...f, autor: v }))}
                  >
                    <span className="author-check">
                      {form.autor === v && <Check size={11} color="#fff" />}
                    </span>
                    {label}
                  </button>
                ))}

                <div className="dropzone">
                  <div className="dz-title"><Camera /> anexar ficheiros</div>
                  <div className="dz-sub">ou arraste para aqui</div>
                  <div className="dz-limit">* Limite 25MB</div>
                </div>

                <div className="pair">
                  <button className="pill voltar" onClick={back}>voltar</button>
                  <button className="pill continue" onClick={next}>continuar <ArrowDownRight /></button>
                </div>
              </>
            )}

            {step === 3 && (
              <>
                {([
                  ['vantagens', 'vantagens'],
                  ['requisitos', 'requisitos'],
                  ['como', 'como é feito actualmente'],
                  ['modelo', 'modelo de negocio'],
                  ['competidores', 'competidores'],
                  ['custos', 'custos'],
                ] as const).map(([k, label]) => (
                  <div className="det-field" key={k}>
                    <label className="field-label">{label}</label>
                    <textarea value={form[k]} onChange={set(k)} />
                  </div>
                ))}
                <div className="pair">
                  <button className="pill voltar" onClick={back}>voltar</button>
                  <button className="pill continue" onClick={next}>continuar <ArrowDownRight /></button>
                </div>
              </>
            )}

            {step === 4 && (
              <>
                <button className="terms-link" type="button" aria-expanded={termsExpanded} onClick={() => setTermsExpanded(expanded => !expanded)}>
                  ler termos e condições <ArrowUpRight />
                </button>
                {termsExpanded && <div className="terms-content">
                  <p>Pela subscrição da presente Declaração, exercida através do botão “Aceitar”, eu reconheço renunciar a todos os direitos relativos a qualquer propriedade, reconhecimento, direito, compensação monetária, ou royalties atribuídos ou devidos a Domingos da Silva Teixeira – SGPS, S.A., suas participadas, subsidiárias, afiliadas, seus sucessores e cessionários (colectivamente denominados como dstgroup) em resultado da venda, cessão, sublicenciamento, depósito, opção de venda ou qualquer outra transação envolvendo algum(ns) ou todos os direitos da propriedade intelectual. Eu reconheço igualmente que qualquer contribuição por mim efectuada, submetida e/ou armazenada através da presente plataforma informática, se aceite pelo dstgroup, através do direito exclusivo que lhe assiste, será, bem como a propriedade intelectual a ela relativa, propriedade exclusiva do dstgroup e que nenhum elemento nela compreendido foi criado a título de trabalho que me foi contratado.</p>
                  <p>Por este meio reconheço ainda, para todos os efeitos previstos pelas leis da República Portuguesa, que renuncio a todos os direitos que tenho, ou possa vir a ter, para contestar a validade e vinculatividade desta Declaração e que renuncio a quaisquer causas de acção relacionadas com a presente Declaração. Para conhecer de qualquer litígio emergente da interpretação e/ou aplicação desta Declaração, serão competentes os tribunais da comarca de Braga.</p>
                </div>}
                <div className="check-row" style={{ marginTop: 18 }}>
                  <button
                    type="button"
                    className={`checkbox ${form.termos ? 'checked' : ''}`}
                    aria-pressed={form.termos}
                    aria-label="Li e aceito os termos e condições"
                    onClick={() => setForm((f) => ({ ...f, termos: !f.termos }))}
                  >
                    {form.termos && <Check size={13} color="#fff" />}
                  </button>
                  <span>li e aceito os termos e condições</span>
                </div>
                <div className="pair">
                  <button className="pill voltar" onClick={back}>voltar</button>
                  <button
                    className="pill continue"
                    onClick={submit}
                    disabled={!form.termos}
                    style={{ opacity: form.termos ? 1 : 0.5 }}
                  >
                    registar ideia <PaperPlane />
                  </button>
                </div>
              </>
            )}
          </div>
        </div>
      </div>
    </div>
  )
}
