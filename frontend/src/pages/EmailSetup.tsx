import { useEffect, useState, type FormEvent } from 'react'
import { emailApi } from '../api/services'
import { getErrorMessage } from '../api/client'
import type { EmailSetupInfo } from '../api/types'
import { Alert } from '../components/Alert'
import { Icon } from '../components/Icon'
import { PasswordField } from '../components/PasswordField'
import { formatDateTime, isValidEmail } from '../utils/format'

export function EmailSetup() {
  const [setup, setSetup] = useState<EmailSetupInfo | null>(null)
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [saved, setSaved] = useState(false)

  useEffect(() => {
    let cancelled = false

    emailApi
      .getSetup()
      .then((result) => {
        if (cancelled) return
        setSetup(result)
        if (result.email) setEmail(result.email)
      })
      .catch((err) => {
        if (!cancelled) setError(getErrorMessage(err))
      })

    return () => {
      cancelled = true
    }
  }, [])

  const onSubmit = async (event: FormEvent) => {
    event.preventDefault()
    setError('')
    setSaved(false)

    if (!isValidEmail(email)) return setError('Please enter a valid email address.')
    if (!password) return setError('Password is required.')

    setSaving(true)
    try {
      const result = await emailApi.saveSetup(email.trim(), password)
      setSetup(result)
      setEmail(result.email ?? email)
      setPassword('')
      setSaved(true)
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setSaving(false)
    }
  }

  return (
    <>
      <div className="page-header">
        <div>
          <h1>Email Setup</h1>
          <p className="muted">The mailbox your emails are sent from.</p>
        </div>
      </div>

      {!setup && !error && <div className="loading">Loading…</div>}

      {setup?.configured && (
        <div className="card connected">
          <span className="stat-icon tone-success">
            <Icon name="check" size={20} />
          </span>
          <div>
            <strong>Connected</strong>
            <p className="muted">
              {setup.email}
              {setup.provider && ` · ${setup.provider}`}
              {setup.updatedAt && ` · updated ${formatDateTime(setup.updatedAt)}`}
            </p>
          </div>
        </div>
      )}

      {setup && (
        <form className="card setup-form" onSubmit={onSubmit} noValidate>
          <h2>{setup.configured ? 'Change mailbox' : 'Connect your mailbox'}</h2>

          {saved && <Alert kind="success">Connected. You can start sending emails now.</Alert>}
          {error && <Alert kind="error">{error}</Alert>}

          <div className="field">
            <label htmlFor="setup-email">Email address</label>
            <input
              id="setup-email"
              type="email"
              autoComplete="off"
              placeholder="you@gmail.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </div>

          <PasswordField
            label="Password"
            autoComplete="new-password"
            placeholder="Your email password or app password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            hint="Gmail, Yahoo and iCloud need an app password, not your normal password."
            required
          />

          <div className="info-box">
            <Icon name="shield" size={18} />
            <p>
              We check the login with your mail provider before saving. Your password is encrypted and is never shown again.
            </p>
          </div>

          {setup.supportedProviders.length > 0 && (
            <p className="hint">Supported: {setup.supportedProviders.join(', ')}</p>
          )}

          <div className="form-actions">
            <button type="submit" className="btn btn-primary" disabled={saving || !email || !password}>
              {saving ? 'Checking…' : setup.configured ? 'Verify & update' : 'Verify & save'}
            </button>
          </div>
        </form>
      )}

      {!setup && error && <Alert kind="error">{error}</Alert>}
    </>
  )
}
