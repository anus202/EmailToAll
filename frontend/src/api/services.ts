import { api, unwrap } from './client'
import type {
  DashboardData,
  EmailHistoryItem,
  EmailSetupInfo,
  HistoryPage,
  LoginData,
  SendEmailResult,
} from './types'

export const authApi = {
  login: (email: string, password: string) =>
    unwrap<LoginData>(api.post('/api/Login/login', { Email: email, Password: password })),

  signup: (email: string, password: string, confirmPassword: string) =>
    unwrap<unknown>(
      api.post('/api/Signup/Signup', { Email: email, Password: password, ConfirmPass: confirmPassword }),
    ),

  verifySignupOtp: (email: string, token: string) =>
    unwrap<string>(api.post('/api/Signup/OptVerification', { Email: email, Token: token })),

  resendSignupOtp: (email: string) => unwrap<string>(api.post('/api/Signup/resendOtp', { Email: email })),

  forgotPassword: (email: string) => unwrap<string>(api.post('/api/Login/ForgetPassword', { Email: email })),

  verifyResetOtp: (email: string, token: string) =>
    unwrap<string>(api.post('/api/Login/LoginOptVerification', { Email: email, Token: token })),

  resetPassword: (email: string, token: string, password: string) =>
    unwrap<string>(api.post('/api/Login/PasswordReset', { Email: email, Token: token, Password: password })),
}

export interface HistoryQuery {
  search?: string
  page?: number
  pageSize?: number
}

export const emailApi = {
  getSetup: () => unwrap<EmailSetupInfo>(api.get('/api/EmailSetup')),

  saveSetup: (email: string, password: string) =>
    unwrap<EmailSetupInfo>(api.post('/api/EmailSetup/Save', { Email: email, Password: password })),

  /** `form` is multipart/form-data: To/Cc/Bcc (repeatable), Subject, Body, IsHtml, Attachments (repeatable). */
  send: (form: FormData, onProgress?: (percent: number) => void) =>
    unwrap<SendEmailResult>(
      api.post('/api/Email/Send', form, {
        timeout: 10 * 60_000,
        onUploadProgress: (event) => {
          if (onProgress && event.total) {
            onProgress(Math.round((event.loaded / event.total) * 100))
          }
        },
      }),
    ),

  history: (query: HistoryQuery = {}) =>
    unwrap<HistoryPage>(api.get('/api/History', { params: query })),

  historyItem: (id: number) => unwrap<EmailHistoryItem>(api.get(`/api/History/${id}`)),

  dashboard: () => unwrap<DashboardData>(api.get('/api/Dashboard')),
}
