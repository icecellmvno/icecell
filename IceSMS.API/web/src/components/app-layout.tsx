import { AppSidebar } from "@/components/app-sidebar"
import { AppHeader } from "@/components/app-header"
import {
  SidebarInset,
  SidebarProvider,
} from "@/components/ui/sidebar"

export function AppLayout({ 
  children,
  header = true,
  breadcrumbs,
  title 
}: { 
  children: React.ReactNode
  header?: boolean
  breadcrumbs?: Array<{ title: string; href?: string }>
  title: string
}) {
  return (
    <SidebarProvider>
      <AppSidebar />
      <SidebarInset>
        {header && <AppHeader title={title} breadcrumbs={breadcrumbs} />}
        {children}
      </SidebarInset>
    </SidebarProvider>
  )
} 