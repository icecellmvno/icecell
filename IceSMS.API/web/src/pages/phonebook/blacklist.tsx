import { useState, useEffect } from 'react'
import {
  ColumnDef,
  flexRender,
  getCoreRowModel,
  getPaginationRowModel,
  useReactTable,
  SortingState,
  getSortedRowModel,
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
import { useQuery, useMutation } from '@tanstack/react-query'
import { Sheet, SheetTrigger, SheetContent, SheetHeader, SheetTitle, SheetFooter } from '@/components/ui/sheet'
import { Button } from '@/components/ui/button'
import { Textarea } from '@/components/ui/textarea'
import { useToast } from '@/hooks/use-toast'
import { PlusIcon, FilePlusIcon } from '@radix-ui/react-icons'
import { SingleAddSheet } from '@/components/phonebook/SingleAddSheet'
import { BulkAddSheet } from '@/components/phonebook/BulkAddSheet'

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

const columns: ColumnDef<BlacklistEntry>[] = [
  {
    accessorKey: 'phone',
    header: 'Telefon Numarası',
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
  }
]

export default function PageBlacklist() {
  const [sorting, setSorting] = useState<SortingState>([])
  const [pagination, setPagination] = useState({
    pageIndex: 0,
    pageSize: 10,
  })

  const { data, isLoading } = useQuery<ApiResponse>({
    queryKey: ['blacklist', pagination],
    queryFn: async (): Promise<ApiResponse> => {
      // Cookie'den token'ı al
      const token = document.cookie
        .split('; ')
        .find(row => row.startsWith('authToken='))
        ?.split('=')[1]

      if (!token) throw new Error('Yetkilendirme hatası')

      const params = new URLSearchParams({
        page: (pagination.pageIndex + 1).toString(),
        limit: pagination.pageSize.toString()
      })
      
      const response = await fetch(`/api/protected/blacklist/get?${params}`, {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      })
      
      if (!response.ok) throw new Error('API hatası')
      return response.json()
    },
    placeholderData: (previousData: ApiResponse | undefined) => previousData,
  })

  const table = useReactTable({
    data: data?.data || [],
    columns,
    pageCount: data?.total_page || -1,
    state: {
      pagination,
      sorting,
    },
    onPaginationChange: setPagination,
    onSortingChange: setSorting,
    manualPagination: true,
    getCoreRowModel: getCoreRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    getSortedRowModel: getSortedRowModel(),
  })

  const { toast } = useToast()

  return (
    <AppLayout title="Kara Liste Yönetimi Sayfası" breadcrumbs={[{ title: 'Kara Liste Yönetimi', href: '/phonebook/blacklist' }]}>
      
      <div className="flex justify-end gap-2 mb-4">
        <Sheet>
          <SheetTrigger asChild>
            <Button variant="outline">
              <PlusIcon className="mr-2 h-4 w-4" />
              Tekli Ekle
            </Button>
          </SheetTrigger>
          <SingleAddSheet />
        </Sheet>

        <Sheet>
          <SheetTrigger asChild>
            <Button className="bg-blue-600 hover:bg-blue-700">
              <FilePlusIcon className="mr-2 h-4 w-4" />
              Toplu Ekle
            </Button>
          </SheetTrigger>
          <BulkAddSheet />
        </Sheet>
      </div>
      
      <div className="flex flex-1 flex-col gap-4 p-4 pt-0">
        <div className="rounded-md border bg-white shadow-sm dark:border-gray-700 dark:bg-gray-900">
          <Table>
            <TableHeader>
              {table.getHeaderGroups().map(headerGroup => (
                <TableRow key={headerGroup.id} className="dark:hover:bg-gray-800">
                  {headerGroup.headers.map(header => (
                    <TableHead key={header.id} className="dark:text-gray-200">
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
              {table.getRowModel().rows.map(row => (
                <TableRow 
                  key={row.id} 
                  className="dark:border-gray-700 dark:hover:bg-gray-800"
                >
                  {row.getVisibleCells().map(cell => (
                    <TableCell key={cell.id} className="dark:text-gray-300">
                      {flexRender(
                        cell.column.columnDef.cell,
                        cell.getContext()
                      )}
                    </TableCell>
                  ))}
                </TableRow>
              ))}
            </TableBody>
          </Table>
          
          <DataTablePagination table={table} />
        </div>
      </div>
    </AppLayout>
  )
} 