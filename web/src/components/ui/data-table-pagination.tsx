import { Table } from '@tanstack/react-table'
import { Button } from '@/components/ui/button'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'

interface DataTablePaginationProps<TData> {
  table: Table<TData>
  isLoading?: boolean
  pageSizeOptions?: number[]
  rowsPerPageText?: string
  totalRowText?: string
}

export function DataTablePagination<TData>({
  table,
  isLoading,
  pageSizeOptions = [10, 20, 30, 50],
  rowsPerPageText = 'Sayfa başına satır:',
  totalRowText = 'Toplam kayıt:'
}: DataTablePaginationProps<TData>) {
  return (
    <div className="flex items-center justify-between px-4 py-3 dark:bg-gray-900">
      <div className="flex items-center space-x-2">
        <p className="text-sm font-medium text-gray-700 dark:text-gray-300">
          {rowsPerPageText} {table.getState().pagination.pageIndex + 1} / {table.getPageCount()}
        </p>
        <Select
          value={`${table.getState().pagination.pageSize}`}
          onValueChange={(value) => table.setPageSize(Number(value))}
        >
          <SelectTrigger className="h-8 w-[70px] dark:border-gray-700 dark:bg-gray-800">
            <SelectValue placeholder={table.getState().pagination.pageSize} />
          </SelectTrigger>
          <SelectContent side="top" className="dark:bg-gray-800">
            {pageSizeOptions.map((pageSize) => (
              <SelectItem 
                key={pageSize} 
                value={`${pageSize}`}
                className="dark:hover:bg-gray-700"
              >
                {pageSize}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>
      <div className="flex items-center space-x-2">
        <Button
          variant="outline"
          size="sm"
          onClick={() => table.previousPage()}
          disabled={!table.getCanPreviousPage() || isLoading}
          className="dark:border-gray-700 dark:bg-gray-800 dark:text-gray-200"
        >
          Önceki
        </Button>
        <Button
          variant="outline"
          size="sm"
          onClick={() => table.nextPage()}
          disabled={!table.getCanNextPage() || isLoading}
          className="dark:border-gray-700 dark:bg-gray-800 dark:text-gray-200"
        >
          Sonraki
        </Button>
      </div>
    </div>
  )
} 