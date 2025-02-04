import { useAuth } from '@/context/auth-context'
import { useTenant } from '@/context/TenantContext'
import { Navigate, Outlet } from 'react-router-dom'
import { Suspense } from 'react'

export default function ProtectedRoute() {
  const { user } = useAuth()
  const { isActive } = useTenant() || {}

  if (!user) {
    return <Navigate to="/auth/login" replace />
  }

  if (!isActive) {
    return <Navigate to="/tenantdisabled" replace />
  }

  return (
    <Suspense fallback={<div>Loading...</div>}>
      <Outlet />
    </Suspense>
  )
} 