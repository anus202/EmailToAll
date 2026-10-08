import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { emailApi } from '../api/services'
import { getErrorMessage } from '../api/client'
import type { EmailSetupInfo, SendEmailResult } from '../api/types'
import { Alert } from '../components/Alert'
import { FileDrop } from '../components/FileDrop'
import { Icon } from '../components/Icon'
import { RecipientInput } from '../components/RecipientInput'
import { isValidEmail, pluralize } from '../utils/format'

// Keep in sync with SendEmailService on the API.
const MAX_RECIPIENTS = 500
const MAX_FILES = 10
const MAX_ATTACHMENT_BYTES = 25 * 1024 * 1024

export function SendEmail() {
  const [setup, setSetup] = useState<EmailSetupInfo | null>(null)
  const [loadError, setLoadError] = useState('')

  const [to, setTo] = useState<string[]>([])
  const [cc, setCc] = useState<string[]>([])
  const [bcc, setBcc] = useState<string[]>([])
  const [subject, setSubject] = useState('')
  const [body, setBody] = useState('')
  const [isHtml, setIsHtml] = useState(false)
  const [files, setFiles] = useState<File[]>([])

  const [sending, setSending] = useState(false)
  const [progress, setProgress] = useState(0)
  const [error, setError] = useState('')
  const [sent, setSent] = useState<SendEmailResult | null>(null)

  useEffect(() => {
    let cancelled = false

    emailApi
      .getSetup()
      .then((result) => {
        if (!cancelled) setSetup(result)
      })
      .catch((err) => {
        if (!cancelled) setLoadError(getErrorMessage(err))
      })

    return () => {
      cancelled = true
    }
  }, [])

  // Invalid chips are shown in red and block sending, so they are not counted.
  const uniqueRecipients = useMemo(
    () =>
      new Set(
        [...to, ...cc, ...bcc].filter(isValidEmail).map((value) => value.toLowerCase()),
      ).size,
    [to, cc, bcc],
  )

  const onSubmit = async (event: FormEvent) => {
    event.preventDefault()
    setError('')
    setSent(null)

    if ([...to, ...cc, ...bcc].some((value) => !isValidEmail(value))) {
      return setError('Some addresses are not valid. Fix or remove the ones marked in red.')
    }
    if (uniqueRecipients === 0) return setError('Add at least one recipient.')
    if (uniqueRecipients > MAX_RECIPIENTS) return setError(`A single email can have at most ${MAX_RECIPIENTS} recipients.`)
    if (!subject.trim()) return setError('Subject is required.')

    const form = new FormData()
    to.forEach((value) => form.append('To', value))
    cc.forEach((value) => form.append('Cc', value))
    bcc.forEach((value) => form.append('Bcc', value))
    form.append('Subject', subject.trim())
    form.append('Body', body)
    form.append('IsHtml', String(isHtml))
    files.forEach((file) => form.append('Attachments', file, file.name))

    setSending(true)
    setProgress(0)
    try {
      const result = await emailApi.send(form, setProgress)
      setSent(result)
      setTo([])
      setCc([])
      setBcc([])
      setSubject('')
      setBody('')
      setFiles([])
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setSending(false)
    }
  }

  if (loadError) {
    return (
      <>
        <PageHeader />
        <Alert kind="error">{loadError}</Alert>
      </>
    )
  }

  if (!setup) {
    return (
      <>
        <PageHeader />
        <div className="loading">Loading…</div>
      </>
    )
  }

  if (!setup.configured) {
    return (
      <>
        <PageHeader />
        <div className="callout">
          <Icon name="settings" size={22} />
          <div>
            <strong>Set up your email first</strong>
            <p>Tell us which mailbox to send from. Your emails go out from your own address.</p>
          </div>
          <Link to="/setup" className="btn btn-primary">
            Set up email
          </Link>
        </div>
      </>
    )
  }

  return (
    <>
      <PageHeader />

      {sent && (
        <Alert kind="success">
          Email sent to {pluralize(sent.recipientCount, 'person', 'people')}.{' '}
          <Link to="/history">View in history</Link>
        </Alert>
      )}
      {error && <Alert kind="error">{error}</Alert>}

      <form className="card compose" onSubmit={onSubmit} noValidate>
        <div className="from-row">
          <span className="muted">From</span>
          <strong>{setup.email}</strong>
        </div>

        <RecipientInput
          label="To"
          values={to}
          onChange={setTo}
          placeholder="Type or paste addresses, separated by commas or spaces"
        />
        <div className="grid-2">
          <RecipientInput label="Cc" values={cc} onChange={setCc} placeholder="Visible to everyone" />
          <RecipientInput label="Bcc" values={bcc} onChange={setBcc} placeholder="Hidden from other recipients" />
        </div>
        <p className="hint recipients-count">
          {uniqueRecipients === 0 ? 'No recipients yet' : pluralize(uniqueRecipients, 'recipient')} · up to {MAX_RECIPIENTS} per email
        </p>

        <div className="field">
          <label htmlFor="subject">Subject</label>
          <input
            id="subject"
            value={subject}
            onChange={(e) => setSubject(e.target.value)}
            maxLength={500}
            placeholder="What is this email about?"
          />
        </div>

        <div className="field">
          <div className="label-row">
            <label htmlFor="body">Message</label>
            <label className="check">
              <input type="checkbox" checked={isHtml} onChange={(e) => setIsHtml(e.target.checked)} />
              Send as HTML
            </label>
          </div>
          <textarea
            id="body"
            rows={10}
            value={body}
            onChange={(e) => setBody(e.target.value)}
            placeholder={isHtml ? '<p>Hello!</p>' : 'Write your message…'}
          />
        </div>

        <FileDrop
          files={files}
          onChange={setFiles}
          maxFiles={MAX_FILES}
          maxTotalBytes={MAX_ATTACHMENT_BYTES}
          onReject={setError}
        />

        {sending && (
          <div className="progress" aria-label="Upload progress">
            <div className="progress-bar" style={{ width: `${progress}%` }} />
          </div>
        )}

        <div className="form-actions">
          <button type="submit" className="btn btn-primary" disabled={sending || uniqueRecipients === 0}>
            <Icon name="send" /> {sending ? (progress < 100 ? `Uploading ${progress}%` : 'Sending…') : 'Send email'}
          </button>
        </div>
      </form>
    </>
  )
}

function PageHeader() {
  return (
    <div className="page-header">
      <div>
        <h1>Send Email</h1>
        <p className="muted">Write once, send to everyone.</p>
      </div>
    </div>
  )
}
