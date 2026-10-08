import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

// In development every /api request is proxied to the ASP.NET Core API,
// so the browser never makes a cross-origin call and no CORS setup is needed.
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  const apiTarget = env.VITE_API_PROXY || 'http://localhost:5018'

  return {
    plugins: [react()],
    server: {
      port: 5173,
      proxy: {
        '/api': { target: apiTarget, changeOrigin: true },
      },
    },
  }
})
