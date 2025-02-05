import { createContext, useContext, useEffect, useState } from 'react'

type User = {
  id: string
  name: string
  email: string
  username: string
  roles: string[]
  permissions: string[]
  tenantId: string
}

type AuthContextType = {
  user: User | null
  login: (token: string) => void
  logout: () => void
}

const AuthContext = createContext<AuthContextType>({
  user: null,
  login: () => {},
  logout: () => {},
})

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null)
  const [loading, setLoading] = useState(true)

  const decodeToken = (token: string) => {
    try {
      if (!token || typeof token !== 'string') {
        throw new Error('Geçersiz token')
      }

      const parts = token.split('.')
      if (parts.length !== 3) {
        throw new Error('Geçersiz JWT formatı')
      }

      const payload = parts[1]
      const decoded = atob(payload.replace(/-/g, '+').replace(/_/g, '/'))
      const parsedData = JSON.parse(decoded)

      if (!parsedData || typeof parsedData !== 'object') {
        throw new Error('Token içeriği geçersiz')
      }

      return parsedData
    } catch (error) {
      console.error('Token decode hatası:', error)
      return null
    }
  }

  useEffect(() => {
    const token = document.cookie
      .split('; ')
      .find(row => row.startsWith('token='))
      ?.split('=')[1]

    if (token) {
      const authData = decodeToken(token)
      if (authData) {
        setUser({
          id: authData['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || '',
          name: Array.isArray(authData['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name']) 
            ? authData['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'].join(' ') 
            : authData['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || '',
          email: authData['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || '',
          username: authData['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name']?.[0] || '',
          roles: Array.isArray(authData['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']) 
            ? authData['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] 
            : [authData['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']],
          permissions: authData.permission || [],
          tenantId: authData.tenant_id || ''
        })
      }
    }
    setLoading(false)
  }, [])

  const login = (token: string) => {
    const expires = new Date(Date.now() + 3 * 60 * 60 * 1000).toUTCString()
    document.cookie = `token=${token}; expires=${expires}; path=/; sameSite=Lax`
    const authData = decodeToken(token)
    setUser(authData)
  }

  const logout = () => {
    document.cookie = 'token=; Path=/; Expires=Thu, 01 Jan 1970 00:00:01 GMT;'
    setUser(null)
  }

  if (loading) {
    return <div>Yükleniyor...</div>
  }

  return (
    <AuthContext.Provider value={{ user, login, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export const useAuth = () => useContext(AuthContext) 