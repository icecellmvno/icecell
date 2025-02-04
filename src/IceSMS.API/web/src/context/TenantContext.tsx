import { createContext, useContext, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'

interface TenantData {

  id: number
  name: string
  domain: string
  description: any
  isActive: boolean
  logo: string
  title: string
  createdAt: string
  updatedAt: string
  credit: number
  parentId: any
  parent: any
  children: any[]
  users: any[]
  roles: any[]


  // Diğer alanlar...
}

const TenantContext = createContext<TenantData | null>(null)

export const TenantProvider = ({ children }: { children: React.ReactNode }) => {
  const [tenantData, setTenantData] = useState<TenantData | null>(null)
  const navigate = useNavigate()

  useEffect(() => {
    const fetchTenantData = async () => {
      try {
        const response = await fetch(`/api/v1/PanelSettings/tenant?domain=${window.location.hostname}`)
        
        if (!response.ok) {
          if(response.status === 404) {
            navigate('/tenantdisabled')
          }
          return
        }

        const data = await response.json()
        
        if(!data.isActive) {
          navigate('/tenantdisabled?reason=disabled')
          return
        }

        // DOM'u güncelle
        document.title = data.description
        const favicon = document.querySelector('link[rel="icon"]') as HTMLLinkElement
        if(favicon) favicon.href = data.favicon
        
        setTenantData(data)
      } catch (error) {
        console.error('Tenant bilgisi alınamadı:', error)
      }
    }

    fetchTenantData()
  }, [navigate])

  return (
    <TenantContext.Provider value={tenantData}>
      {children}
    </TenantContext.Provider>
  )
}

export const useTenant = () => useContext(TenantContext) 