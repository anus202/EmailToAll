import type { ReactNode } from 'react'
import { Icon } from './Icon'

interface AlertProps {
  kind?: 'error' | 'success' | 'info' | 'warning'
  children: ReactNode
}

export function Alert({ kind = 'info', children }: AlertProps) {
  const icon = kind === 'success' ? 'check' : kind === 'info' ? 'info' : 'alert'

  return (
    <div className={`alert alert-${kind}`} role={kind === 'error' ? 'alert' : 'status'}>
      <Icon name={icon} size={18} className="alert-icon" />
      <div className="alert-body">{children}</div>
    </div>
  )
}
