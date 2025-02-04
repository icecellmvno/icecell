import { useState } from 'react'
import * as XLSX from 'xlsx'
import { useMutation } from '@tanstack/react-query'
import { Textarea } from '@/components/ui/textarea'
import { Button } from '@/components/ui/button'
import { useToast } from '@/hooks/use-toast'
import { SheetContent, SheetHeader, SheetTitle, SheetFooter } from '@/components/ui/sheet'

export const BulkAddSheet = () => {
  const [inputNumbers, setInputNumbers] = useState('')
  const [file, setFile] = useState<File | null>(null)
  const { toast } = useToast()

  const mutation = useMutation({
    mutationFn: async (numbers: string[]) => {
      const token = document.cookie
        .split('; ')
        .find(row => row.startsWith('authToken='))
        ?.split('=')[1]

      const response = await fetch('/api/protected/blacklist/bulk', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ numbers })
      })
      if (!response.ok) throw new Error('Toplu ekleme hatası')
      return response.json()
    }
  })

  const handleFileUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (!file) return

    try {
      const numbers = await parseFile(file)
      setInputNumbers(numbers.join('\n'))
      setFile(file)
      toast({ title: 'Yüklendi', description: `${numbers.length} numara bulundu` })
    } catch (error) {
      toast({ title: 'Hata', description: 'Dosya işlenemedi' })
    }
  }

  const parseFile = async (file: File): Promise<string[]> => {
    const extension = file.name.split('.').pop()?.toLowerCase()
    
    if (extension === 'csv') {
      const text = await file.text()
      return text.split('\n').map(line => line.trim().replace(/[^0-9]/g, ''))
    }

    if (extension === 'xlsx') {
      const data = await file.arrayBuffer()
      const workbook = XLSX.read(data, { type: 'array' })
      const sheet = workbook.Sheets[workbook.SheetNames[0]]
      return XLSX.utils.sheet_to_json(sheet).map((row: any) => 
        String(row.phone || row.tel || row.numara).replace(/[^0-9]/g, '')
      )
    }

    throw new Error('Desteklenmeyen dosya formatı')
  }

  const handleSubmit = () => {
    const numbers = inputNumbers
      .split('\n')
      .map(n => n.trim().replace(/[^0-9]/g, ''))
      .filter(n => n.length >= 10)

    if (numbers.length === 0) {
      toast({ title: 'Hata', description: 'Geçerli numara bulunamadı' })
      return
    }

    mutation.mutate(numbers, {
      onSuccess: () => {
        toast({ title: 'Başarılı', description: `${numbers.length} numara eklendi` })
        setInputNumbers('')
        setFile(null)
      },
      onError: () => toast({ title: 'Hata', description: 'Eklenirken hata oluştu' })
    })
  }

  return (
    <SheetContent className="max-w-[600px]">
      <SheetHeader>
        <SheetTitle>Toplu Ekle</SheetTitle>
      </SheetHeader>
      
      <div className="py-4 space-y-4">
        <div className="border-2 border-dashed rounded-lg p-4 text-center">
          <input
            type="file"
            accept=".csv,.xlsx"
            onChange={handleFileUpload}
            className="hidden"
            id="bulk-upload"
          />
          <label
            htmlFor="bulk-upload"
            className="cursor-pointer text-blue-600 hover:text-blue-700"
          >
            CSV/Excel dosyası seç
          </label>
          {file && <p className="mt-2 text-sm">{file.name}</p>}
        </div>

        <div className="relative">
          <div className="absolute inset-0 flex items-center">
            <div className="w-full border-t" />
          </div>
          <div className="relative flex justify-center">
            <span className="bg-background px-2 text-sm text-muted-foreground">VEYA</span>
          </div>
        </div>

        <Textarea
          value={inputNumbers}
          onChange={(e) => setInputNumbers(e.target.value)}
          placeholder="Satır başına bir numara"
          rows={8}
          className="font-mono text-sm"
        />
      </div>

      <SheetFooter>
        <Button
          onClick={handleSubmit}
          disabled={mutation.isPending}
          className="w-full"
        >
          {mutation.isPending ? `Ekleniyor (${inputNumbers.split('\n').length})...` : 'Toplu Ekle'}
        </Button>
      </SheetFooter>
    </SheetContent>
  )
} 