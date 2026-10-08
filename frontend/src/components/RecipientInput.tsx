import { useId, useRef, useState, type ClipboardEvent, type KeyboardEvent } from 'react'
import { isValidEmail, splitAddresses } from '../utils/format'
import { Icon } from './Icon'

interface RecipientInputProps {
  label: string
  values: string[]
  onChange: (values: string[]) => void
  placeholder?: string
  hint?: string
}

/**
 * Chip-style address field. Typing or pasting (comma, semicolon, space or new line separated)
 * turns text into chips; invalid entries stay visible in red so they can be fixed or removed.
 */
export function RecipientInput({ label, values, onChange, placeholder, hint }: RecipientInputProps) {
  const id = useId()
  const inputRef = useRef<HTMLInputElement>(null)
  const [draft, setDraft] = useState('')

  const commit = (text: string) => {
    const tokens = splitAddresses(text)
    if (tokens.length === 0) {
      setDraft('')
      return
    }

    const existing = new Set(values.map((v) => v.toLowerCase()))
    const added: string[] = []
    for (const token of tokens) {
      const key = token.toLowerCase()
      if (!existing.has(key)) {
        existing.add(key)
        added.push(token)
      }
    }

    if (added.length > 0) onChange([...values, ...added])
    setDraft('')
  }

  const onKeyDown = (event: KeyboardEvent<HTMLInputElement>) => {
    if (['Enter', ',', ';', ' ', 'Tab'].includes(event.key) && draft.trim()) {
      event.preventDefault()
      commit(draft)
    } else if (event.key === 'Backspace' && !draft && values.length > 0) {
      onChange(values.slice(0, -1))
    }
  }

  const onPaste = (event: ClipboardEvent<HTMLInputElement>) => {
    const text = event.clipboardData.getData('text')
    if (/[\s,;<]/.test(text.trim())) {
      event.preventDefault()
      commit(text)
    }
  }

  const remove = (value: string) => onChange(values.filter((v) => v !== value))

  return (
    <div className="field">
      <label htmlFor={id}>{label}</label>
      <div className="chip-box" onClick={() => inputRef.current?.focus()}>
        {values.map((value) => {
          const valid = isValidEmail(value)
          return (
            <span key={value} className={`chip ${valid ? '' : 'chip-invalid'}`} title={valid ? value : 'Not a valid email address'}>
              {value}
              <button
                type="button"
                onClick={(event) => {
                  event.stopPropagation()
                  remove(value)
                }}
                aria-label={`Remove ${value}`}
              >
                <Icon name="x" size={12} />
              </button>
            </span>
          )
        })}
        <input
          id={id}
          ref={inputRef}
          value={draft}
          onChange={(event) => setDraft(event.target.value)}
          onKeyDown={onKeyDown}
          onPaste={onPaste}
          onBlur={() => commit(draft)}
          placeholder={values.length === 0 ? placeholder : undefined}
          autoComplete="off"
          spellCheck={false}
        />
      </div>
      {hint && <p className="hint">{hint}</p>}
    </div>
  )
}
