import { useTenant } from '@/context/TenantContext'

export const Header = () => {
  const tenantData = useTenant()
  
  return (
    <header>
      {tenantData?.logo && (
        <img 
          src={tenantData.logo} 
          alt="Tenant Logo" 
          className="h-12 w-auto"
        />
      )}
      <h1>{tenantData?.title || 'Varsayılan Başlık'}</h1>
    </header>
  )
} 