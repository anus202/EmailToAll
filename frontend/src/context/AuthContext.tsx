import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import { session, setUnauthorizedHandler } from '../api/client'

interface AuthState {
  token: string | null
  email: string | null
  login: (token: string, email: string) => void
  logout: () => void
}

const AuthContext = createContext<AuthState | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(() => session.getToken())
  const [email, setEmail] = useState<string | null>(() => session.getEmail())

  const login = useCallback((newToken: string, newEmail: string) => {
    session.save(newToken, newEmail)
    setToken(newToken)
    setEmail(newEmail)
  }, [])

  const logout = useCallback(() => {
    session.clear()
    setToken(null)
    setEmail(null)
  }, [])

  // Any 401 from the API (expired or invalid token) signs the user out.
  useEffect(() => {
    setUnauthorizedHandler(logout)
    return () => setUnauthorizedHandler(null)
  }, [logout])

  const value = useMemo(() => ({ token, email, login, logout }), [token, email, login, logout])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthState {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used inside <AuthProvider>')
  }
  return context
}
