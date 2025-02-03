import { createContext, useContext, useEffect, useState } from 'react'

type User = {
  name: string
  email: string
  username: string
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
      const payload = token.split('.')[1]
      const decoded = atob(payload.replace(/-/g, '+').replace(/_/g, '/'))
      return JSON.parse(decoded)
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
          name: authData.name || '',
          email: authData.email || '',
          username: authData.username || authData.email?.split('@')[0] || 'kullanici'
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