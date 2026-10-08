import { useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { authApi } from '../api/services'
import { getErrorMessage } from '../api/client'
import { Alert } from '../components/Alert'
import { AuthCard } from '../components/AuthCard'
import { OtpField } from '../components/OtpField'
import { PasswordField } from '../components/PasswordField'
import { isValidEmail } from '../utils/format'

const MIN_PASSWORD = 8

type Step = 'email' | 'code' | 'password'

export function ForgotPassword() {
  const navigate = useNavigate()

  const [step, setStep] = useState<Step>('email')
  const [email, setEmail] = useState('')
  const [code, setCode] = useState('')
  const [password, setPassword] = useState('')
  const [confirm, setConfirm] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  const run = async (action: () => Promise<void>) => {
    setError('')
    setLoading(true)
    try {
      await action()
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }

  const sendCode = (event: FormEvent) => {
    event.preventDefault()
    if (!isValidEmail(email)) return setError('Please enter a valid email address.')

    void run(async () => {
      await authApi.forgotPassword(email.trim())
      setStep('code')
    })
  }

  const checkCode = (event: FormEvent) => {
    event.preventDefault()

    void run(async () => {
      await authApi.verifyResetOtp(email.trim(), code)
      setStep('password')
    })
  }

  const resetPassword = (event: FormEvent) => {
    event.preventDefault()
    if (password.length < MIN_PASSWORD) return setError(`Password must be at least ${MIN_PASSWORD} characters.`)
    if (password !== confirm) return setError('Passwords do not match.')

    void run(async () => {
      await authApi.resetPassword(email.trim(), code, password)
      navigate('/login', { replace: true, state: { notice: 'Password updated. Sign in with your new password.' } })
    })
  }

  const footer = <Link to="/login">Back to sign in</Link>

  if (step === 'code') {
    return (
      <AuthCard
        title="Enter the code"
        subtitle={`We emailed a 6-digit code to ${email.trim()}. It expires in 5 minutes.`}
        footer={footer}
      >
        <form onSubmit={checkCode} className="form">
          {error && <Alert kind="error">{error}</Alert>}
          <OtpField value={code} onChange={setCode} />
          <button type="submit" className="btn btn-primary btn-block" disabled={loading || code.length !== 6}>
            {loading ? 'Checking…' : 'Continue'}
          </button>
        </form>
      </AuthCard>
    )
  }

  if (step === 'password') {
    return (
      <AuthCard title="Choose a new password" subtitle="Pick something you have not used before." footer={footer}>
        <form onSubmit={resetPassword} className="form" noValidate>
          {error && <Alert kind="error">{error}</Alert>}

          <PasswordField
            label="New password"
            autoComplete="new-password"
            placeholder={`At least ${MIN_PASSWORD} characters`}
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
          <PasswordField
            label="Confirm new password"
            autoComplete="new-password"
            placeholder="Repeat your password"
            value={confirm}
            onChange={(e) => setConfirm(e.target.value)}
            required
          />

          <button type="submit" className="btn btn-primary btn-block" disabled={loading || !password || !confirm}>
            {loading ? 'Saving…' : 'Update password'}
          </button>
        </form>
      </AuthCard>
    )
  }

  return (
    <AuthCard title="Forgot your password?" subtitle="Enter your email and we will send you a reset code." footer={footer}>
      <form onSubmit={sendCode} className="form" noValidate>
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

        <button type="submit" className="btn btn-primary btn-block" disabled={loading || !email}>
          {loading ? 'Sending…' : 'Send reset code'}
        </button>
      </form>
    </AuthCard>
  )
}
