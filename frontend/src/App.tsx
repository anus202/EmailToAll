import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { Layout } from './components/Layout'
import { ProtectedRoute, PublicOnlyRoute } from './components/RouteGuards'
import { AuthProvider } from './context/AuthContext'
import { Dashboard } from './pages/Dashboard'
import { EmailSetup } from './pages/EmailSetup'
import { ForgotPassword } from './pages/ForgotPassword'
import { History } from './pages/History'
import { Login } from './pages/Login'
import { SendEmail } from './pages/SendEmail'
import { Signup } from './pages/Signup'

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route element={<PublicOnlyRoute />}>
            <Route path="/login" element={<Login />} />
            <Route path="/signup" element={<Signup />} />
            <Route path="/forgot-password" element={<ForgotPassword />} />
          </Route>

          <Route element={<ProtectedRoute />}>
            <Route element={<Layout />}>
              <Route path="/dashboard" element={<Dashboard />} />
              <Route path="/send" element={<SendEmail />} />
              <Route path="/history" element={<History />} />
              <Route path="/setup" element={<EmailSetup />} />
            </Route>
          </Route>

          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  )
}
