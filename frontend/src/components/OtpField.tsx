import { useId } from 'react'

interface OtpFieldProps {
  value: string
  onChange: (value: string) => void
  label?: string
}

/** Six-digit one-time code input. */
export function OtpField({ value, onChange, label = 'Verification code' }: OtpFieldProps) {
  const id = useId()

  return (
    <div className="field">
      <label htmlFor={id}>{label}</label>
      <input
        id={id}
        className="otp-input"
        inputMode="numeric"
        autoComplete="one-time-code"
        maxLength={6}
        placeholder="••••••"
        value={value}
        onChange={(e) => onChange(e.target.value.replace(/\D/g, '').slice(0, 6))}
        autoFocus
      />
    </div>
  )
}
