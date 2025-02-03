import { Card } from "@/components/ui/card"

import { Ban } from "lucide-react"
import { useNavigate, useSearchParams } from "react-router-dom"

export default function TenantDisabledPage() {
  const [searchParams] = useSearchParams()
  const errorType = searchParams.get('error')
  
  const errorMessage = errorType === 'connection' 
    ? 'Sunucuya bağlanılamadı' 
    : 'Bu bayi bulunamadı veya erişime kapalı'

  return (
    <div className="flex items-center justify-center min-h-screen bg-gray-100">
      <Card className="p-8 max-w-md w-full text-center">
        <div className="flex justify-center mb-4">
          <Ban className="h-12 w-12 text-red-500" />
        </div>
        <h1 className="text-2xl font-bold text-red-600 mb-4">
          Bayi Devre Dışı
        </h1>
        <p className="text-gray-600 mb-6">
          {errorMessage}
        </p>
     
      </Card>
    </div>
  )
} 