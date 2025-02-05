import { useState } from 'react'
import {
  ColumnDef,
  flexRender,
  getCoreRowModel,
  getPaginationRowModel,
  useReactTable,
  SortingState,
  getSortedRowModel,
  ColumnFiltersState,
  getFilteredRowModel,
  RowSelectionState,
  VisibilityState,
} from '@tanstack/react-table'
import { AppLayout } from "@/components/app-layout"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { DataTablePagination } from '@/components/ui/data-table-pagination'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Sheet, SheetTrigger } from '@/components/ui/sheet'
import { Button } from '@/components/ui/button'
import { useToast } from '@/hooks/use-toast'
import { PlusIcon, FilePlusIcon } from '@radix-ui/react-icons'
import { SingleAddSheet } from '@/components/phonebook/SingleAddSheet'
import { BulkAddSheet } from '@/components/phonebook/BulkAddSheet'
import { Input } from "@/components/ui/input"
import { Card, CardContent } from "@/components/ui/card"
import { Search, MoreHorizontal, Download, Trash2, DownloadCloud } from 'lucide-react'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import { Checkbox } from "@/components/ui/checkbox"

interface BlacklistEntry {
  ID: number
  phone: string
  tenant_id: number
  CreatedAt: string
  UpdatedAt: string
  DeletedAt: string | null
}

interface ApiResponse {
  data: BlacklistEntry[]
  total_page: number
  total: number
  page: number
  limit: number
}

export default function PageBlacklist() {
  const [sorting, setSorting] = useState<SortingState>([])
  const [columnFilters, setColumnFilters] = useState<ColumnFiltersState>([])
  const [pagination, setPagination] = useState({
    pageIndex: 0,
    pageSize: 10,
  })
  const [globalFilter, setGlobalFilter] = useState('')
  const [rowSelection, setRowSelection] = useState<RowSelectionState>({})
  const [columnVisibility, setColumnVisibility] = useState<VisibilityState>({})

  const queryClient = useQueryClient()
  const { toast } = useToast()

  const deleteMutation = useMutation({
    mutationFn: async (id: number) => {
      const token = document.cookie
        .split('; ')
        .find(row => row.startsWith('authToken='))
        ?.split('=')[1]

      if (!token) throw new Error('Yetkilendirme hatası')

      const response = await fetch(`/api/v1/blacklist/${id}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${token}`,
        },
      })

      if (!response.ok) throw new Error('Silme işlemi başarısız')
      return response.json()
    },
    onSuccess: () => {
      toast({
        title: "Başarılı",
        description: "Kayıt başarıyla silindi",
      })
      queryClient.invalidateQueries({ queryKey: ['blacklist'] })
    },
    onError: () => {
      toast({
        title: "Hata",
        description: "Silme işlemi sırasında bir hata oluştu",
        variant: "destructive",
      })
    },
  })

  const handleDelete = (id: number) => {
    if (window.confirm('Bu kaydı silmek istediğinize emin misiniz?')) {
      deleteMutation.mutate(id)
    }
  }

  const columns: ColumnDef<BlacklistEntry>[] = [
    {
      id: "select",
      header: ({ table }) => (
        <Checkbox
          checked={table.getIsAllPageRowsSelected()}
          onCheckedChange={(value: boolean) => table.toggleAllPageRowsSelected(!!value)}
          aria-label="Tümünü seç"
        />
      ),
      cell: ({ row }) => (
        <Checkbox
          checked={row.getIsSelected()}
          onCheckedChange={(value: boolean) => row.toggleSelected(!!value)}
          aria-label="Satır seç"
        />
      ),
      enableSorting: false,
      enableHiding: false,
    },
    {
      accessorKey: 'phone',
      header: 'Telefon Numarası',
      cell: ({ row }) => <div className="font-medium">{row.getValue('phone')}</div>,
      filterFn: 'includesString',
    },
    {
      accessorKey: 'tenant_id',
      header: 'Tenant ID',
    },
    {
      accessorKey: 'CreatedAt',
      header: 'Eklenme Tarihi',
      cell: ({ row }) => new Date(row.getValue('CreatedAt')).toLocaleDateString()
    },
    {
      accessorKey: 'UpdatedAt',
      header: 'Güncelleme Tarihi',
      cell: ({ row }) => new Date(row.getValue('UpdatedAt')).toLocaleDateString()
    },
    {
      id: 'actions',
      cell: ({ row }) => {
        const entry = row.original
        return (
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" className="h-8 w-8 p-0">
                <MoreHorizontal className="h-4 w-4" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem onClick={() => handleDelete(entry.ID)}>
                <Trash2 className="mr-2 h-4 w-4" />
                Sil
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        )
      },
    }
  ]

  const { data, isLoading } = useQuery<ApiResponse>({
    queryKey: ['blacklist', pagination],
    queryFn: async (): Promise<ApiResponse> => {
      const token = document.cookie
        .split('; ')
        .find(row => row.startsWith('authToken='))
        ?.split('=')[1]

      if (!token) throw new Error('Yetkilendirme hatası')

      const params = new URLSearchParams({
        page: (pagination.pageIndex + 1).toString(),
        limit: pagination.pageSize.toString()
      })
      
      const response = await fetch(`/api/v1/blacklist?${params}`, {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      })
      
      if (!response.ok) throw new Error('API hatası')
      return response.json()
    },
  })

  const exportToCSV = async () => {
    const token = document.cookie
      .split('; ')
      .find(row => row.startsWith('authToken='))
      ?.split('=')[1]

    if (!token) throw new Error('Yetkilendirme hatası')

    const response = await fetch('/api/v1/blacklist/export?fileType=CSV', {
      headers: {
        'Authorization': `Bearer ${token}`,
      },
    })

    if (!response.ok) throw new Error('Dışa aktarma başarısız')

    const blob = await response.blob()
    const url = window.URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `karalist_${new Date().toISOString().split('T')[0]}.csv`
    document.body.appendChild(a)
    a.click()
    window.URL.revokeObjectURL(url)
    document.body.removeChild(a)
  }

  const table = useReactTable({
    data: data?.data || [],
    columns,
    pageCount: data?.total_page || -1,
    state: {
      pagination,
      sorting,
      columnFilters,
      globalFilter,
      rowSelection,
      columnVisibility,
    },
    onPaginationChange: setPagination,
    onSortingChange: setSorting,
    onColumnFiltersChange: setColumnFilters,
    onGlobalFilterChange: setGlobalFilter,
    onRowSelectionChange: setRowSelection,
    onColumnVisibilityChange: setColumnVisibility,
    manualPagination: true,
    getCoreRowModel: getCoreRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    enableSorting: true,
    enableColumnFilters: true,
    enableRowSelection: true,
  })

  return (
    <AppLayout title="Kara Liste Yönetimi Sayfası" breadcrumbs={[{ title: 'Kara Liste Yönetimi', href: '/phonebook/blacklist' }]}>
      <Card>
        <CardContent className="p-6">
          <div className="flex items-center justify-between mb-6">
            <div className="flex items-center space-x-2 w-1/3">
              <Search className="w-4 h-4 text-gray-500" />
              <Input
                placeholder="Ara..."
                value={globalFilter}
                onChange={e => setGlobalFilter(e.target.value)}
                className="h-9"
              />
            </div>
            <div className="flex gap-2">
              <Button 
                variant="outline" 
                size="sm"
                onClick={exportToCSV}
              >
                <Download className="mr-2 h-4 w-4" />
                CSV İndir
              </Button>

              <Sheet>
                <SheetTrigger asChild>
                  <Button variant="outline" size="sm">
                    <PlusIcon className="mr-2 h-4 w-4" />
                    Tekli Ekle
                  </Button>
                </SheetTrigger>
                <SingleAddSheet />
              </Sheet>

              <Sheet>
                <SheetTrigger asChild>
                  <Button className="bg-blue-600 hover:bg-blue-700" size="sm">
                    <FilePlusIcon className="mr-2 h-4 w-4" />
                    Toplu Ekle
                  </Button>
                </SheetTrigger>
                <BulkAddSheet />
              </Sheet>
            </div>
          </div>

          {isLoading ? (
            <div className="flex items-center justify-center h-64">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-gray-900"></div>
            </div>
          ) : (
            <>
              <div className="rounded-md border">
                <Table>
                  <TableHeader>
                    {table.getHeaderGroups().map(headerGroup => (
                      <TableRow key={headerGroup.id}>
                        {headerGroup.headers.map(header => (
                          <TableHead 
                            key={header.id}
                            className="font-semibold"
                          >
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
                    {table.getRowModel().rows.length === 0 ? (
                      <TableRow>
                        <TableCell colSpan={columns.length} className="h-24 text-center">
                          Kayıt bulunamadı.
                        </TableCell>
                      </TableRow>
                    ) : (
                      table.getRowModel().rows.map(row => (
                        <TableRow key={row.id}>
                          {row.getVisibleCells().map(cell => (
                            <TableCell key={cell.id}>
                              {flexRender(
                                cell.column.columnDef.cell,
                                cell.getContext()
                              )}
                            </TableCell>
                          ))}
                        </TableRow>
                      ))
                    )}
                  </TableBody>
                </Table>
              </div>
              <div className="mt-4">
                <DataTablePagination table={table} />
              </div>
            </>
          )}
        </CardContent>
      </Card>
    </AppLayout>
  )
} 