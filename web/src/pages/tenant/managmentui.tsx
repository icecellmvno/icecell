import { AppLayout } from "@/components/app-layout"
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { ColumnDef, createColumnHelper, getCoreRowModel, useReactTable, flexRender } from "@tanstack/react-table"
import { useState } from "react"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { DataTablePagination } from '@/components/ui/data-table-pagination'
import { Input } from "@/components/ui/input"
import { Button } from "@/components/ui/button"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import { Label } from "@/components/ui/label"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { PlusIcon } from "lucide-react"

interface Tenant {
  ID: number
  CreatedAt: string
  UpdatedAt: string
  DeletedAt: string | null
  name: string
  title: string
  favicon: string
  logo: string
  tenant_type: number
  is_active: boolean
  tenant_credit: number
  domain_name: string
  tenant_cover_image: string
  parent_id: number
}

export default function TenantManagement() {
  const [searchTerm, setSearchTerm] = useState('')
  const [pagination, setPagination] = useState({
    pageIndex: 0,
    pageSize: 10,
  })

  const { data: tenants } = useQuery({
    queryKey: ['tenants', pagination, searchTerm],
    queryFn: async () => {
      const token = document.cookie
        .split('; ')
        .find(row => row.startsWith('token='))
        ?.split('=')[1]
      
      const params = new URLSearchParams({
        page: (pagination.pageIndex + 1).toString(),
        limit: pagination.pageSize.toString(),
        search: searchTerm
      })
      
      const response = await fetch(`/api/v1/tenants?${params}`, {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      })
      if (!response.ok) throw new Error('API hatası')
      return response.json()
    }
  })

  const { data: currentUser } = useQuery({
    queryKey: ['currentUser'],
    queryFn: async () => {
      const token = document.cookie
        .split('; ')
        .find(row => row.startsWith('token='))
        ?.split('=')[1]
      
      const response = await fetch('/api/v1/auth/me', {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      })
      if (!response.ok) throw new Error('API hatası')
      return response.json()
    }
  })

  const columnHelper = createColumnHelper<Tenant>()
  const columns: ColumnDef<Tenant>[] = [
    {
      accessorKey: 'ID',
      header: 'ID',
    },
    {
      accessorKey: 'name',
      header: 'Ad',
    },
    {
      accessorKey: 'title',
      header: 'Başlık',
    },
    {
      accessorKey: 'domain_name',
      header: 'Domain',
    },
    {
      accessorKey: 'tenant_type',
      header: 'Tip',
      cell: info => {
        switch(info.getValue()) {
          case 1: return 'Host'
          case 2: return 'Bayi'
          case 3: return 'Müşteri'
          default: return 'Bilinmeyen'
        }
      }
    },
    {
      accessorKey: 'tenant_credit',
      header: 'Kredi',
    },
    {
      accessorKey: 'is_active',
      header: 'Durum',
      cell: ({ row }) => (
        <Badge variant={row.getValue("is_active") ? "default" : "destructive"}>
          {row.getValue("is_active") ? "Aktif" : "Pasif"}
        </Badge>
      ),
    },
    {
      accessorKey: 'CreatedAt',
      header: 'Oluşturulma Tarihi',
      cell: info => new Date(info.getValue<string>()).toLocaleDateString()
    },
    {
      accessorKey: 'UpdatedAt',
      header: 'Güncelleme Tarihi',
      cell: info => new Date(info.getValue<string>()).toLocaleDateString()
    },
    {
      id: 'actions',
      cell: ({ row }) => (
        <div className="space-x-2">
          <button onClick={() => handleEdit(row.original)}>Düzenle</button>
          <button onClick={() => handleDelete(row.original.ID)}>Sil</button>
        </div>
      ),
    }
  ]

  const [isModalOpen, setIsModalOpen] = useState(false)
  const [selectedTenant, setSelectedTenant] = useState<Tenant | null>(null)
  const queryClient = useQueryClient()

  const createMutation = useMutation({
    mutationFn: async (newTenant: Omit<Tenant, 'ID'>) => {
      const token = document.cookie
        .split('; ')
        .find(row => row.startsWith('token='))
        ?.split('=')[1]

      const response = await fetch('/api/v1/tenants', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(newTenant)
      })
      if (!response.ok) throw new Error('Oluşturma hatası')
      return response.json()
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['tenants'] })
  })

  const updateMutation = useMutation({
    mutationFn: async (updatedTenant: Tenant) => {
      const token = document.cookie
        .split('; ')
        .find(row => row.startsWith('token='))
        ?.split('=')[1]

      const response = await fetch(`/api/v1/tenants/${updatedTenant.ID}`, {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(updatedTenant)
      })
      if (!response.ok) throw new Error('Güncelleme hatası')
      return response.json()
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['tenants'] })
  })

  const deleteMutation = useMutation({
    mutationFn: async (id: number) => {
      const token = document.cookie
        .split('; ')
        .find(row => row.startsWith('token='))
        ?.split('=')[1]

      const response = await fetch(`/api/v1/tenants/${id}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${token}`
        }
      })
      if (!response.ok) throw new Error('Silme hatası')
      return response.json()
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['tenants'] })
  })

  const handleEdit = (tenant: Tenant) => {
    setSelectedTenant(tenant)
    setIsModalOpen(true)
  }

  const handleFormSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if(selectedTenant) {
      if(selectedTenant.ID) {
        updateMutation.mutate(selectedTenant);
      } else {
        createMutation.mutate(selectedTenant);
      }
      setIsModalOpen(false);
      setSelectedTenant(null);
    }
  }

  const getTenantTypeOptions = (currentTenantType: number) => {
    switch(currentTenantType) {
      case 1: // Host
        return [
          { value: "2", label: "Bayi" },
          { value: "3", label: "Müşteri" }
        ]
      case 2: // Reseller
        return [
          { value: "3", label: "Müşteri" }
        ]
      default:
        return []
    }
  }

  const handleNew = () => {
    setSelectedTenant({ 
      ID: 0, 
      name: '', 
      title: '', 
      favicon: '', 
      logo: '', 
      tenant_type: currentUser?.tenant_type === 1 ? 2 : 3,
      is_active: true, 
      tenant_credit: 0, 
      domain_name: '', 
      tenant_cover_image: '', 
      parent_id: 0,
      CreatedAt: '',
      UpdatedAt: '',
      DeletedAt: null
    })
    setIsModalOpen(true)
  }

  const handleDelete = (id: number) => {
    if(confirm('Silmek istediğinize emin misiniz?')) {
      deleteMutation.mutate(id)
    }
  }

  const table = useReactTable({
    data: tenants?.data || [],
    columns,
    pageCount: tenants?.total_page || -1,
    state: {
      pagination,
    },
    manualPagination: true,
    onPaginationChange: setPagination,
    getCoreRowModel: getCoreRowModel(),
  })

  return (
    <AppLayout title="Kiracı Yönetim">
      <div className="container mx-auto py-6 space-y-6">
        <div className="flex justify-between items-center">
          <h1 className="text-3xl font-bold tracking-tight dark:text-gray-100">Kiracı Yönetimi</h1>
          <Button onClick={handleNew}>
            <PlusIcon className="mr-2 h-4 w-4" /> Yeni Kiracı
          </Button>
        </div>

        <Card className="dark:border-gray-800">
          <CardHeader className="space-y-0 pb-4 dark:border-gray-800">
            <div className="flex items-center justify-between">
              <CardTitle className="dark:text-gray-100">Kiracılar</CardTitle>
              <div className="flex w-full max-w-sm items-center space-x-2">
                <Input
                  placeholder="Kiracı ara..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="h-9 dark:bg-gray-950 dark:border-gray-800 dark:text-gray-100"
                />
              </div>
            </div>
          </CardHeader>
          <CardContent>
            <div className="rounded-md border dark:border-gray-800">
              <Table>
                <TableHeader>
                  {table.getHeaderGroups().map(headerGroup => (
                    <TableRow key={headerGroup.id} className="dark:border-gray-800">
                      {headerGroup.headers.map(header => (
                        <TableHead key={header.id} className="dark:text-gray-400">
                          {flexRender(
                            header.column.columnDef.header,
                            header.getContext()
                          )}
                        </TableHead>
                      ))}
                    </TableRow>
                  ))}
                </TableHeader>
                <TableBody>
                  {table.getRowModel().rows.length ? (
                    table.getRowModel().rows.map(row => (
                      <TableRow key={row.id} className="dark:border-gray-800 dark:hover:bg-gray-900">
                        {row.getVisibleCells().map(cell => (
                          <TableCell key={cell.id} className="dark:text-gray-300">
                            {flexRender(
                              cell.column.columnDef.cell,
                              cell.getContext()
                            )}
                          </TableCell>
                        ))}
                      </TableRow>
                    ))
                  ) : (
                    <TableRow>
                      <TableCell colSpan={columns.length} className="h-24 text-center dark:text-gray-400">
                        Kayıt bulunamadı.
                      </TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </div>
            <div className="mt-4">
              <DataTablePagination table={table} />
            </div>
          </CardContent>
        </Card>

        {isModalOpen && (
          <div className="fixed inset-0 bg-background/80 backdrop-blur-sm z-50 dark:bg-gray-950/80">
            <div className="fixed left-[50%] top-[50%] z-50 grid w-full max-w-3xl translate-x-[-50%] translate-y-[-50%]">
              <Card className="dark:border-gray-800">
                <CardHeader className="dark:border-gray-800">
                  <CardTitle className="dark:text-gray-100">
                    {selectedTenant?.ID ? 'Kiracı Düzenle' : 'Yeni Kiracı Ekle'}
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <form onSubmit={handleFormSubmit} className="space-y-4">
                    <div className="grid grid-cols-2 gap-4">
                      <div className="space-y-2">
                        <Label htmlFor="name" className="dark:text-gray-100">Ad</Label>
                        <Input
                          id="name"
                          value={selectedTenant?.name || ''}
                          onChange={e => setSelectedTenant({...selectedTenant!, name: e.target.value})}
                          required
                          className="dark:bg-gray-950 dark:border-gray-800 dark:text-gray-100"
                        />
                      </div>
                      <div className="space-y-2">
                        <Label htmlFor="title" className="dark:text-gray-100">Başlık</Label>
                        <Input
                          id="title"
                          value={selectedTenant?.title || ''}
                          onChange={e => setSelectedTenant({...selectedTenant!, title: e.target.value})}
                          required
                          className="dark:bg-gray-950 dark:border-gray-800 dark:text-gray-100"
                        />
                      </div>
                      <div className="space-y-2">
                        <Label htmlFor="domain_name" className="dark:text-gray-100">Domain</Label>
                        <Input
                          id="domain_name"
                          value={selectedTenant?.domain_name || ''}
                          onChange={e => setSelectedTenant({...selectedTenant!, domain_name: e.target.value})}
                          required
                          className="dark:bg-gray-950 dark:border-gray-800 dark:text-gray-100"
                        />
                      </div>
                      <div className="space-y-2">
                        <Label htmlFor="tenant_credit" className="dark:text-gray-100">Kredi</Label>
                        <Input
                          id="tenant_credit"
                          type="number"
                          value={selectedTenant?.tenant_credit || 0}
                          onChange={e => setSelectedTenant({...selectedTenant!, tenant_credit: Number(e.target.value)})}
                          className="dark:bg-gray-950 dark:border-gray-800 dark:text-gray-100"
                        />
                      </div>
                      <div className="space-y-2">
                        <Label htmlFor="status" className="dark:text-gray-100">Durum</Label>
                        <Select
                          value={selectedTenant?.is_active ? "1" : "0"}
                          onValueChange={value => setSelectedTenant({...selectedTenant!, is_active: Boolean(Number(value))})}
                        >
                          <SelectTrigger className="dark:bg-gray-950 dark:border-gray-800 dark:text-gray-100">
                            <SelectValue placeholder="Durum seçin" />
                          </SelectTrigger>
                          <SelectContent className="dark:bg-gray-950 dark:border-gray-800">
                            <SelectItem value="1" className="dark:text-gray-100">Aktif</SelectItem>
                            <SelectItem value="0" className="dark:text-gray-100">Pasif</SelectItem>
                          </SelectContent>
                        </Select>
                      </div>
                      <div className="space-y-2 col-span-2">
                        <Label htmlFor="tenant_type" className="dark:text-gray-100">Kiracı Tipi</Label>
                        <Select
                          value={String(selectedTenant?.tenant_type)}
                          onValueChange={value => setSelectedTenant({...selectedTenant!, tenant_type: Number(value)})}
                          required
                          disabled={!currentUser?.data}
                        >
                          <SelectTrigger className="dark:bg-gray-950 dark:border-gray-800 dark:text-gray-100">
                            <SelectValue placeholder="Kiracı tipi seçin" />
                          </SelectTrigger>
                          <SelectContent className="dark:bg-gray-950 dark:border-gray-800">
                            {currentUser?.data && getTenantTypeOptions(currentUser.data.tenant_type).map(option => (
                              <SelectItem 
                                key={option.value} 
                                value={option.value} 
                                className="dark:text-gray-100"
                              >
                                {option.label}
                              </SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                      </div>
                    </div>
                    <div className="flex justify-end gap-2">
                      <Button 
                        variant="outline" 
                        onClick={() => setIsModalOpen(false)}
                        className="dark:border-gray-800 dark:hover:bg-gray-900"
                      >
                        İptal
                      </Button>
                      <Button type="submit">Kaydet</Button>
                    </div>
                  </form>
                </CardContent>
              </Card>
            </div>
          </div>
        )}
      </div>
    </AppLayout>
  )
} 