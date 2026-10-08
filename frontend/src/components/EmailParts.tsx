import type { EmailHistoryItem, EmailStatus } from '../api/types'

export function StatusBadge({ status }: { status: EmailStatus }) {
  return <span className={`badge ${status === 'Sent' ? 'badge-success' : 'badge-danger'}`}>{status}</span>
}

/** "alice@x.com +4 more" style summary of everyone an email went to. */
export function RecipientSummary({ item }: { item: EmailHistoryItem }) {
  const all = [...item.to, ...item.cc, ...item.bcc]
  if (all.length === 0) return <span className="muted">—</span>

  const rest = all.length - 1
  return (
    <span title={all.join(', ')}>
      {all[0]}
      {rest > 0 && <span className="muted"> +{rest} more</span>}
    </span>
  )
}
