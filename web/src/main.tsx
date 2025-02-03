import React from 'react'
import ReactDOM from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import App from './App'
import './assets/index.css'
import { TenantProvider } from '@/context/TenantContext'
import { ThemeProvider } from './context/ThemeContext'
import { AuthProvider } from '@/context/auth-context'

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <BrowserRouter>
      <ThemeProvider>
        <AuthProvider>
          <TenantProvider>
            <App />
          </TenantProvider>
        </AuthProvider>
      </ThemeProvider>
    </BrowserRouter>

  </React.StrictMode>
)
