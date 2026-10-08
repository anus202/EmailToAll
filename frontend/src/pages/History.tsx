import { useEffect, useState } from 'react'
import { emailApi } from '../api/services'
import { getErrorMessage } from '../api/client'
import type { EmailHistoryItem, HistoryPage } from '../api/types'
import { Alert } from '../components/Alert'
import { RecipientSummary, StatusBadge } from '../components/EmailParts'
import { Icon } from '../components/Icon'
import { Modal } from '../components/Modal'
import { formatDateTime } from '../utils/format'

const PAGE_SIZE = 10

export function History() {
  const [search, setSearch] = useState('')
  const [query, setQuery] = useState('')
  const [page, setPage] = useState(1)
  const [data, setData] = useState<HistoryPage | null>(null)
  const [error, setError] = useState('')
  const [selected, setSelected] = useState<EmailHistoryItem | null>(null)
  const [opening, setOpening] = useState<number | null>(null)

  // Wait for the user to stop typing before querying.
  useEffect(() => {
    const timer = window.setTimeout(() => {
      setQuery(search.trim())
      setPage(1)
    }, 350)
    return () => window.clearTimeout(timer)
  }, [search])

  useEffect(() => {
    let cancelled = false

    emailApi
      .history({ search: query || undefined, page, pageSize: PAGE_SIZE })
      .then((result) => {
        if (cancelled) return
        setData(result)
        setError('')
      })
      .catch((err) => {
        if (!cancelled) setError(getErrorMessage(err))
      })

    return () => {
      cancelled = true
    }
  }, [query, page])

  const open = async (id: number) => {
    setOpening(id)
    try {
      setSelected(await emailApi.historyItem(id))
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setOpening(null)
    }
  }

  const totalPages = data ? Math.max(1, Math.ceil(data.total / data.pageSize)) : 1
  const from = data && data.total > 0 ? (data.page - 1) * data.pageSize + 1 : 0
  const to = data ? Math.min(data.page * data.pageSize, data.total) : 0

  return (
    <>
      <div className="page-header">
        <div>
          <h1>History</h1>
          <p className="muted">Every email you sent, and who it went to.</p>
        </div>
      </div>

      {error && <Alert kind="error">{error}</Alert>}

      <section className="card">
        <div className="toolbar">
          <div className="search">
            <Icon name="search" size={16} />
            <input
              type="search"
              placeholder="Search by subject or recipient"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              aria-label="Search history"
            />
          </div>
        </div>

        {!data && !error && <div className="loading">Loading…</div>}

        {data && data.items.length === 0 && (
          <div className="empty">
            <Icon name="mail" size={28} />
            <p>{query ? 'No emails match your search.' : 'You have not sent any emails yet.'}</p>
          </div>
        )}

        {data && data.items.length > 0 && (
          <>
            <div className="table-wrap">
              <table className="table table-click">
                <thead>
                  <tr>
                    <th>Date</th>
                    <th>Subject</th>
                    <th>To</th>
                    <th>People</th>
                    <th>Files</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  {data.items.map((item) => (
                    <tr
                      key={item.id}
                      onClick={() => open(item.id)}
                      onKeyDown={(e) => e.key === 'Enter' && open(item.id)}
                      tabIndex={0}
                      className={opening === item.id ? 'row-busy' : ''}
                    >
                      <td className="nowrap muted">{formatDateTime(item.sentAt)}</td>
                      <td className="cell-strong">{item.subject}</td>
                      <td>
                        <RecipientSummary item={item} />
                      </td>
                      <td>{item.recipientCount}</td>
                      <td>
                        {item.attachments.length > 0 ? (
                          <span className="with-icon">
                            <Icon name="paperclip" size={14} /> {item.attachments.length}
                          </span>
                        ) : (
                          <span className="muted">—</span>
                        )}
                      </td>
                      <td>
                        <StatusBadge status={item.status} />
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <div className="pager">
              <span className="muted">
                Showing {from}–{to} of {data.total}
              </span>
              <div className="pager-buttons">
                <button type="button" className="btn btn-ghost btn-sm" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>
                  <Icon name="left" size={14} /> Previous
                </button>
                <span className="muted">
                  Page {page} of {totalPages}
                </span>
                <button
                  type="button"
                  className="btn btn-ghost btn-sm"
                  disabled={page >= totalPages}
                  onClick={() => setPage((p) => p + 1)}
                >
                  Next <Icon name="right" size={14} />
                </button>
              </div>
            </div>
          </>
        )}
      </section>

      {selected && (
        <Modal title="Email details" onClose={() => setSelected(null)}>
          <Details item={selected} />
        </Modal>
      )}
    </>
  )
}

function AddressList({ label, values }: { label: string; values: string[] }) {
  if (values.length === 0) return null

  return (
    <div className="detail-row">
      <dt>{label}</dt>
      <dd>
        <div className="chips">
          {values.map((value) => (
            <span key={value} className="chip chip-static">
              {value}
            </span>
          ))}
        </div>
      </dd>
    </div>
  )
}

function Details({ item }: { item: EmailHistoryItem }) {
  return (
    <dl className="details">
      <div className="detail-row">
        <dt>Subject</dt>
        <dd className="cell-strong">{item.subject}</dd>
      </div>
      <div className="detail-row">
        <dt>Status</dt>
        <dd>
          <StatusBadge status={item.status} /> <span className="muted">{formatDateTime(item.sentAt)}</span>
        </dd>
      </div>
      {item.errorMessage && (
        <div className="detail-row">
          <dt>Error</dt>
          <dd className="text-danger">{item.errorMessage}</dd>
        </div>
      )}
      <div className="detail-row">
        <dt>From</dt>
        <dd>{item.fromEmail}</dd>
      </div>
      <AddressList label="To" values={item.to} />
      <AddressList label="Cc" values={item.cc} />
      <AddressList label="Bcc" values={item.bcc} />
      {item.attachments.length > 0 && (
        <div className="detail-row">
          <dt>Attachments</dt>
          <dd>
            <ul className="plain-list">
              {item.attachments.map((name) => (
                <li key={name} className="with-icon">
                  <Icon name="paperclip" size={14} /> {name}
                </li>
              ))}
            </ul>
          </dd>
        </div>
      )}
      <div className="detail-row">
        <dt>Message{item.isHtml ? ' (HTML source)' : ''}</dt>
        <dd>
          <pre className="message-body">{item.body || '(empty)'}</pre>
        </dd>
      </div>
    </dl>
  )
}
