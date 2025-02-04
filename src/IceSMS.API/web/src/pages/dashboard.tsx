import { AppLayout } from "@/components/app-layout"

export default function Dashboard() {
  return (
    <AppLayout 
      title="Data Fetching"
      breadcrumbs={[
        { title: "Dashboard", href: "#" }
      ]}
    >
      <div className="flex flex-1 flex-col gap-4 p-4 pt-0">
      <h1>Dashboard</h1>
      </div>
    </AppLayout>
  )
}
