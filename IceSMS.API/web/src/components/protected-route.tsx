import { useAuth } from '@/context/auth-context'
import { useTenant } from '@/context/TenantContext'
import { Navigate, Outlet } from 'react-router-dom'
import { Suspense } from 'react'

export default function ProtectedRoute() {
  const { user } = useAuth()
  const { is_active } = useTenant() || {}

  if (!user) {
    return <Navigate to="/auth/login" replace />
  }

  if (is_active === false) {
    return <Navigate to="/tenantdisabled" replace />
  }

  return (
    <Suspense fallback={<div>Loading...</div>}>
      <Outlet />
    </Suspense>
  )
} 