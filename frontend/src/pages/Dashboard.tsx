import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { emailApi } from '../api/services'
import { getErrorMessage } from '../api/client'
import type { DashboardData } from '../api/types'
import { Alert } from '../components/Alert'
import { RecipientSummary, StatusBadge } from '../components/EmailParts'
import { Icon, type IconName } from '../components/Icon'
import { formatRelative } from '../utils/format'

interface StatProps {
  icon: IconName
  label: string
  value: number
  tone: 'primary' | 'success' | 'info' | 'danger'
}

function Stat({ icon, label, value, tone }: StatProps) {
  return (
    <div className="card stat">
      <span className={`stat-icon tone-${tone}`}>
        <Icon name={icon} size={20} />
      </span>
      <div>
        <div className="stat-value">{value.toLocaleString()}</div>
        <div className="stat-label">{label}</div>
      </div>
    </div>
  )
}

export function Dashboard() {
  const [data, setData] = useState<DashboardData | null>(null)
  const [error, setError] = useState('')

  useEffect(() => {
    let cancelled = false

    emailApi
      .dashboard()
      .then((result) => {
        if (!cancelled) setData(result)
      })
      .catch((err) => {
        if (!cancelled) setError(getErrorMessage(err))
      })

    return () => {
      cancelled = true
    }
  }, [])

  return (
    <>
      <div className="page-header">
        <div>
          <h1>Dashboard</h1>
          <p className="muted">A quick look at what you have sent.</p>
        </div>
        <Link to="/send" className="btn btn-primary">
          <Icon name="send" /> Compose email
        </Link>
      </div>

      {error && <Alert kind="error">{error}</Alert>}

      {!data && !error && <div className="loading">Loading…</div>}

      {data && (
        <>
          {!data.emailConfigured && (
            <div className="callout">
              <Icon name="settings" size={22} />
              <div>
                <strong>Connect your mailbox to start sending</strong>
                <p>Add the email address and password you want to send from. It takes less than a minute.</p>
              </div>
              <Link to="/setup" className="btn btn-primary">
                Set up email
              </Link>
            </div>
          )}

          {data.emailConfigured && (
            <p className="sending-as">
              Sending as <strong>{data.configuredEmail}</strong>
            </p>
          )}

          <div className="stats">
            <Stat icon="send" label="Emails sent" value={data.totalSent} tone="primary" />
            <Stat icon="users" label="People reached" value={data.totalRecipients} tone="success" />
            <Stat icon="clock" label="Sent in last 24 hours" value={data.sentLast24Hours} tone="info" />
            <Stat icon="alert" label="Failed" value={data.totalFailed} tone="danger" />
          </div>

          <section className="card">
            <div className="card-header">
              <h2>Recent activity</h2>
              <Link to="/history" className="link-sm">
                View all
              </Link>
            </div>

            {data.recent.length === 0 ? (
              <div className="empty">
                <Icon name="mail" size={28} />
                <p>No emails yet. Your sent emails will show up here.</p>
              </div>
            ) : (
              <div className="table-wrap">
                <table className="table">
                  <thead>
                    <tr>
                      <th>Subject</th>
                      <th>To</th>
                      <th>People</th>
                      <th>Status</th>
                      <th>When</th>
                    </tr>
                  </thead>
                  <tbody>
                    {data.recent.map((item) => (
                      <tr key={item.id}>
                        <td className="cell-strong">{item.subject}</td>
                        <td>
                          <RecipientSummary item={item} />
                        </td>
                        <td>{item.recipientCount}</td>
                        <td>
                          <StatusBadge status={item.status} />
                        </td>
                        <td className="nowrap muted">{formatRelative(item.sentAt)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </section>
        </>
      )}
    </>
  )
}
