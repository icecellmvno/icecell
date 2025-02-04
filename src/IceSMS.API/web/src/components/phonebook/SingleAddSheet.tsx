import { useState } from 'react'
import { useMutation } from '@tanstack/react-query'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { useToast } from '@/hooks/use-toast'
import { SheetContent, SheetHeader, SheetTitle } from '@/components/ui/sheet'

export const SingleAddSheet = () => {
  const [phoneNumber, setPhoneNumber] = useState('')
  const { toast } = useToast()

  const mutation = useMutation({
    mutationFn: async (number: string) => {
      const token = document.cookie
        .split('; ')
        .find(row => row.startsWith('authToken='))
        ?.split('=')[1]

      const response = await fetch('/api/protected/blacklist/add', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ numbers: [number] })
      })
      if (!response.ok) throw new Error('Ekleme hatası')
      return response.json()
    }
  })

  const handleSubmit = () => {
    const cleanNumber = phoneNumber.replace(/[^0-9]/g, '')
    if (cleanNumber.length < 10) {
      toast({ title: 'Hata', description: 'Geçersiz telefon numarası' })
      return
    }

    mutation.mutate(cleanNumber, {
      onSuccess: () => {
        toast({ title: 'Başarılı', description: 'Numara eklendi' })
        setPhoneNumber('')
      },
      onError: () => toast({ title: 'Hata', description: 'Eklenemedi' })
    })
  }

  return (
    <SheetContent>
      <SheetHeader>
        <SheetTitle>Tekli Ekle</SheetTitle>
      </SheetHeader>
      
      <div className="py-4 space-y-4">
        <Input
          value={phoneNumber}
          onChange={(e) => setPhoneNumber(e.target.value)}
          placeholder="Örnek: 5551234567"
        />
        <Button 
          onClick={handleSubmit}
          disabled={mutation.isPending}
          className="w-full"
        >
          {mutation.isPending ? 'Ekleniyor...' : 'Ekle'}
        </Button>
      </div>
    </SheetContent>
  )
} 