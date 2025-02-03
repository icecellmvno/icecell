import * as React from "react"
import { useEffect } from "react"
import {
  AudioWaveform,
  BookOpen,
  Bot,
  Building2,
  Command,
  Frame,
  GalleryVerticalEnd,
  Home,
  Map,
  Moon,
  PieChart,
  Settings2,
  SquareTerminal,
  Sun,
} from "lucide-react"
import { Link } from "react-router-dom"

import { NavMain } from "@/components/nav-main"
import { NavProjects } from "@/components/nav-projects"
import { NavUser } from "@/components/nav-user"
import { TeamSwitcher } from "@/components/team-switcher"
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarHeader,
  SidebarMenuButton,
  SidebarRail,
} from "@/components/ui/sidebar"
import { useTenant } from "@/context/TenantContext"

// This is sample data.
const NavMainData = [
  {
    title: "Panelim",
    url: "/",
    isActive: true,
    icon: Home,
    isChild: false
  },
  {
    title: "Rehber",
    url: "/phonebook",
    isActive: true,
    icon: BookOpen,
    isChild: false,
    children: [
      {
        title: "Kara Liste",
        url: "/phonebook/blacklist",
        isChild: true
      },
      {
        title: "Rehberim",
        url: "/phonebook/contacts",
        isChild: true
      }
    ]
  }
]

export function AppSidebar({ ...props }: React.ComponentProps<typeof Sidebar>) {
  const [darkMode, setDarkMode] = React.useState(false)

  useEffect(() => {
    const cookieValue = document.cookie
      .split('; ')
      .find(row => row.startsWith('darkMode='))
      ?.split('=')[1]
      
    const isDark = cookieValue === 'true'
    setDarkMode(isDark)
    if(isDark) {
      document.documentElement.classList.add('dark')
    }
  }, [])

  const toggleDarkMode = () => {
    const newDarkMode = !darkMode
    setDarkMode(newDarkMode)
    document.documentElement.classList.toggle('dark')
    document.cookie = `darkMode=${newDarkMode}; path=/; max-age=${60 * 60 * 24 * 365}`
  }

  return (
    <Sidebar collapsible="icon" {...props}>
      <SidebarHeader>
      <SidebarMenuButton
          size="lg"
          className="data-[state=open]:bg-sidebar-accent data-[state=open]:text-sidebar-accent-foreground"
          onClick={() => {
              <Link to="/dashboard"></Link>
          }}
        >
          <div className="flex aspect-square size-8 items-center justify-center rounded-lg bg-sidebar-primary text-sidebar-primary-foreground">
            <Building2 className="h-6 w-6" />
          </div>
          <div className="grid flex-1 text-left text-sm leading-tight">
            <span className="truncate font-semibold">
           Name
            </span>
            <span className="truncate text-xs">Credits</span>
          </div>
      
        </SidebarMenuButton>
      </SidebarHeader>
      <SidebarContent>
        <NavMain items={NavMainData} />
       
      </SidebarContent>
      <SidebarFooter>
        <NavUser/>
      </SidebarFooter>
      <SidebarRail />
    </Sidebar>
  )
}
