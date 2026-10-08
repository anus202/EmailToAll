import { useState } from 'react'
import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { Icon, type IconName } from './Icon'

const NAV: { to: string; label: string; icon: IconName }[] = [
  { to: '/dashboard', label: 'Dashboard', icon: 'dashboard' },
  { to: '/send', label: 'Send Email', icon: 'send' },
  { to: '/history', label: 'History', icon: 'history' },
  { to: '/setup', label: 'Email Setup', icon: 'settings' },
]

export function Layout() {
  const { email, logout } = useAuth()
  const navigate = useNavigate()
  const [open, setOpen] = useState(false)

  const signOut = () => {
    logout()
    navigate('/login', { replace: true })
  }

  return (
    <div className="shell">
      <header className="topbar">
        <button type="button" className="icon-btn" onClick={() => setOpen((v) => !v)} aria-label="Toggle menu">
          <Icon name="menu" size={22} />
        </button>
        <span className="brand">
          <Icon name="mail" size={20} /> EmailToAll
        </span>
      </header>

      <aside className={`sidebar ${open ? 'sidebar-open' : ''}`}>
        <div className="brand brand-lg">
          <span className="brand-mark">
            <Icon name="mail" size={18} />
          </span>
          EmailToAll
        </div>

        <nav>
          {NAV.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              onClick={() => setOpen(false)}
              className={({ isActive }) => `nav-link ${isActive ? 'nav-link-active' : ''}`}
            >
              <Icon name={item.icon} />
              {item.label}
            </NavLink>
          ))}
        </nav>

        <div className="sidebar-footer">
          <div className="user-chip" title={email ?? undefined}>
            <span className="avatar">{(email ?? '?').charAt(0).toUpperCase()}</span>
            <span className="user-email">{email}</span>
          </div>
          <button type="button" className="btn btn-ghost btn-block" onClick={signOut}>
            <Icon name="logout" /> Sign out
          </button>
        </div>
      </aside>

      {open && <div className="scrim" onClick={() => setOpen(false)} />}

      <main className="content">
        <Outlet />
      </main>
    </div>
  )
}
