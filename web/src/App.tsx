import { lazy, Suspense } from 'react';
import { Routes, Route, Navigate } from "react-router-dom";
import "./assets/index.css";
import { useTenant } from '@/context/TenantContext';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import ProtectedRoute from '@/components/protected-route'
import NotFound from '@/pages/Notfound';
// Dinamik import'lar
const Dashboard = lazy(() => import("@/pages/dashboard"));
const PageTags = lazy(() => import("@/pages/phonebook/tags"));
const PhonebookContacts = lazy(() => import("@/pages/phonebook/contacts"));
const PageBlacklist = lazy(() => import("@/pages/phonebook/blacklist"));
const SmsSend = lazy(() => import("@/pages/sms/sendsms"));
const CampaignReports = lazy(() => import("@/pages/reports/campaingreports"));
const SendLogs = lazy(() => import("@/pages/reports/sendlogs"));
const TenantManagement = lazy(() => import("@/pages/tenant/managmentui"));
const Login = lazy(() => import("@/pages/auth/login"));
const TenantDisabledPage = lazy(() => import("@/pages/tenant/TenantDisabledPage"));

const queryClient = new QueryClient()

function App() {
    return (
        <QueryClientProvider client={queryClient}>
            <Routes>
                <Route path="/auth/login" element={<Login />} />
                <Route path="/tenantdisabled" element={<TenantDisabledPage />} />
                <Route element={<ProtectedRoute />}>
                    <Route path="/" element={<Dashboard />} />
                    <Route path="/phonebook/blacklist" element={<PageBlacklist />} />
                    <Route path="/phonebook/tags" element={<PageTags />} />
                    <Route path="/phonebook/contacts" element={<PhonebookContacts />} />
                    <Route path="/sms/sendsms" element={<SmsSend />} />
                    <Route path="/reports/campaingreports" element={<CampaignReports />} />
                    <Route path="/reports/sendlogs" element={<SendLogs />} />
                    {/* Tenant Management */}
                    <Route path="/management/tenants" element={<TenantManagement />} />
                </Route>
                <Route path="*" element={<NotFound />} />
            </Routes>
        </QueryClientProvider>
    );
}

export default App;