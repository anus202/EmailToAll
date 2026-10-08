import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

/** Only signed-in users may enter; everyone else is sent to the login page. */
export function ProtectedRoute() {
  const { token } = useAuth()
  const location = useLocation()

  if (!token) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />
  }

  return <Outlet />
}

/** Login, signup and password reset are hidden once the user is signed in. */
export function PublicOnlyRoute() {
  const { token } = useAuth()

  if (token) {
    return <Navigate to="/dashboard" replace />
  }

  return <Outlet />
}
