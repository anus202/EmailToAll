import { useEffect, useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { authApi } from '../api/services'
import { getErrorMessage } from '../api/client'
import { Alert } from '../components/Alert'
import { AuthCard } from '../components/AuthCard'
import { OtpField } from '../components/OtpField'
import { PasswordField } from '../components/PasswordField'
import { isValidEmail } from '../utils/format'

const MIN_PASSWORD = 8
const RESEND_SECONDS = 30

export function Signup() {
  const navigate = useNavigate()

  const [step, setStep] = useState<'details' | 'verify'>('details')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirm, setConfirm] = useState('')
  const [code, setCode] = useState('')
  const [error, setError] = useState('')
  const [info, setInfo] = useState('')
  const [loading, setLoading] = useState(false)
  const [cooldown, setCooldown] = useState(0)

  useEffect(() => {
    if (cooldown <= 0) return
    const timer = window.setTimeout(() => setCooldown((s) => s - 1), 1000)
    return () => window.clearTimeout(timer)
  }, [cooldown])

  const submitDetails = async (event: FormEvent) => {
    event.preventDefault()
    setError('')

    if (!isValidEmail(email)) return setError('Please enter a valid email address.')
    if (password.length < MIN_PASSWORD) return setError(`Password must be at least ${MIN_PASSWORD} characters.`)
    if (password !== confirm) return setError('Passwords do not match.')

    setLoading(true)
    try {
      await authApi.signup(email.trim(), password, confirm)
      setStep('verify')
      setCooldown(RESEND_SECONDS)
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }

  const submitCode = async (event: FormEvent) => {
    event.preventDefault()
    setError('')
    setInfo('')
    setLoading(true)

    try {
      await authApi.verifySignupOtp(email.trim(), code)
      navigate('/login', { replace: true, state: { notice: 'Email verified. You can sign in now.' } })
    } catch (err) {
      setError(getErrorMessage(err))
      setLoading(false)
    }
  }

  const resend = async () => {
    setError('')
    setInfo('')
    try {
      await authApi.resendSignupOtp(email.trim())
      setInfo('A new code is on its way.')
      setCooldown(RESEND_SECONDS)
    } catch (err) {
      setError(getErrorMessage(err))
    }
  }

  if (step === 'verify') {
    return (
      <AuthCard
        title="Check your email"
        subtitle={`We sent a 6-digit code to ${email.trim()}. It expires in 5 minutes.`}
        footer={<Link to="/login">Back to sign in</Link>}
      >
        <form onSubmit={submitCode} className="form">
          {error && <Alert kind="error">{error}</Alert>}
          {info && <Alert kind="success">{info}</Alert>}

          <OtpField value={code} onChange={setCode} />

          <button type="submit" className="btn btn-primary btn-block" disabled={loading || code.length !== 6}>
            {loading ? 'Verifying…' : 'Verify email'}
          </button>

          <button type="button" className="btn btn-ghost btn-block" onClick={resend} disabled={cooldown > 0}>
            {cooldown > 0 ? `Resend code in ${cooldown}s` : 'Resend code'}
          </button>
        </form>
      </AuthCard>
    )
  }

  return (
    <AuthCard
      title="Create your account"
      subtitle="Send one email to hundreds of people from your own mailbox."
      footer={
        <>
          Already have an account? <Link to="/login">Sign in</Link>
        </>
      }
    >
      <form onSubmit={submitDetails} className="form" noValidate>
        {error && <Alert kind="error">{error}</Alert>}

        <div className="field">
          <label htmlFor="email">Email</label>
          <input
            id="email"
            type="email"
            autoComplete="email"
            placeholder="you@example.com"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            autoFocus
          />
        </div>

        <PasswordField
          label="Password"
          autoComplete="new-password"
          placeholder={`At least ${MIN_PASSWORD} characters`}
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
        />

        <PasswordField
          label="Confirm password"
          autoComplete="new-password"
          placeholder="Repeat your password"
          value={confirm}
          onChange={(e) => setConfirm(e.target.value)}
          required
        />

        <button type="submit" className="btn btn-primary btn-block" disabled={loading || !email || !password || !confirm}>
          {loading ? 'Creating account…' : 'Create account'}
        </button>
      </form>
    </AuthCard>
  )
}
