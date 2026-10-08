/** Envelope returned by every endpoint of the API. */
export interface ApiResponse<T> {
  success: boolean
  message?: string
  data?: T
}

export interface LoginData {
  message: string
  token: string
  email: string
}

export interface EmailSetupInfo {
  configured: boolean
  email: string | null
  provider: string | null
  updatedAt: string | null
  supportedProviders: string[]
}

export type EmailStatus = 'Sent' | 'Failed'

export interface EmailHistoryItem {
  id: number
  fromEmail: string
  to: string[]
  cc: string[]
  bcc: string[]
  subject: string
  isHtml: boolean
  attachments: string[]
  recipientCount: number
  status: EmailStatus
  errorMessage: string | null
  sentAt: string
  /** Only present on the detail endpoint. */
  body: string | null
}

export interface HistoryPage {
  items: EmailHistoryItem[]
  total: number
  page: number
  pageSize: number
}

export interface DashboardData {
  emailConfigured: boolean
  configuredEmail: string | null
  totalSent: number
  totalFailed: number
  totalRecipients: number
  sentLast24Hours: number
  recent: EmailHistoryItem[]
}

export interface SendEmailResult {
  id: number
  recipientCount: number
  sentAt: string
}
