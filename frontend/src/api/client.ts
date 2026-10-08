import axios, { AxiosError, type AxiosResponse } from 'axios'
import type { ApiResponse } from './types'

const TOKEN_KEY = 'emailtoall.token'
const EMAIL_KEY = 'emailtoall.email'

/** Thin wrapper over localStorage that never throws (private mode, blocked storage). */
export const session = {
  getToken(): string | null {
    try {
      return localStorage.getItem(TOKEN_KEY)
    } catch {
      return null
    }
  },
  getEmail(): string | null {
    try {
      return localStorage.getItem(EMAIL_KEY)
    } catch {
      return null
    }
  },
  save(token: string, email: string): void {
    try {
      localStorage.setItem(TOKEN_KEY, token)
      localStorage.setItem(EMAIL_KEY, email)
    } catch {
      /* storage unavailable: the session just won't survive a reload */
    }
  },
  clear(): void {
    try {
      localStorage.removeItem(TOKEN_KEY)
      localStorage.removeItem(EMAIL_KEY)
    } catch {
      /* ignore */
    }
  },
}

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? '',
  timeout: 60_000,
})

// Attach the JWT to every request.
api.interceptors.request.use((config) => {
  const token = session.getToken()
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// A 401 means the token is missing, invalid or expired: let the app log the user out.
let onUnauthorized: (() => void) | null = null

export function setUnauthorizedHandler(handler: (() => void) | null): void {
  onUnauthorized = handler
}

api.interceptors.response.use(
  (response) => response,
  (error: unknown) => {
    if (axios.isAxiosError(error) && error.response?.status === 401) {
      onUnauthorized?.()
    }
    return Promise.reject(error)
  },
)

/** Error thrown when the API answers `{ success: false, message }`. */
export class ApiError extends Error {
  constructor(message: string) {
    super(message)
    this.name = 'ApiError'
  }
}

/** Resolves with `data` of a successful response and throws an `ApiError` otherwise. */
export async function unwrap<T>(request: Promise<AxiosResponse<ApiResponse<T>>>): Promise<T> {
  const response = await request
  const body = response.data

  if (!body || body.success !== true) {
    throw new ApiError(body?.message || 'The request failed. Please try again.')
  }

  return body.data as T
}

/** Turns anything thrown by `unwrap`/axios into a message that is safe to show to the user. */
export function getErrorMessage(error: unknown): string {
  if (error instanceof ApiError) {
    return error.message
  }

  if (error instanceof AxiosError) {
    if (error.code === 'ECONNABORTED') {
      return 'The request took too long. Please try again.'
    }
    if (!error.response) {
      return 'Cannot reach the server. Make sure the API is running.'
    }
    if (error.response.status === 413) {
      return 'The files are too large to upload.'
    }
    if (error.response.status === 401) {
      return 'Your session has expired. Please sign in again.'
    }
    return `The server returned an error (${error.response.status}).`
  }

  if (error instanceof Error) {
    return error.message
  }

  return 'Something went wrong. Please try again.'
}
