using IceSMS.API.Data;
using IceSMS.API.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
optionsBuilder.UseNpgsql("Host=localhost;Database=smsmerkezi;Username=admin_smsmerkezi;Password=wrpQuWwNR3EY");
ApplicationDbContext context = new ApplicationDbContext(optionsBuilder.Options);

// Veritabanını temizle
context.Database.ExecuteSqlRaw("DELETE FROM \"UserRoles\"");
context.Database.ExecuteSqlRaw("DELETE FROM \"RolePermissions\"");
context.Database.ExecuteSqlRaw("DELETE FROM \"Permissions\"");
context.Database.ExecuteSqlRaw("DELETE FROM \"Roles\"");
context.Database.ExecuteSqlRaw("DELETE FROM \"Users\"");
context.Database.ExecuteSqlRaw("DELETE FROM \"Tenants\"");

// Tenants oluştur
var HostTenant = new Tenant
{
    Name = "Test Host",
    Domain = "host.localhost",
    Credit = 10000,
    IsActive = true,
};
context.Tenants.Add(HostTenant);
context.SaveChanges();
Console.WriteLine("Host Tenant created");

// Permissions oluştur
var permissions = new List<Permission>
{
    new Permission { Name = "user.create", Description = "Kullanıcı oluşturma izni" },
    new Permission { Name = "user.read", Description = "Kullanıcı okuma izni" },
    new Permission { Name = "user.update", Description = "Kullanıcı güncelleme izni" },
    new Permission { Name = "user.delete", Description = "Kullanıcı silme izni" },
    new Permission { Name = "role.manage", Description = "Rol yönetimi izni" },
    new Permission { Name = "tenant.manage", Description = "Tenant yönetimi izni" }
};

context.Permissions.AddRange(permissions);
context.SaveChanges();
Console.WriteLine("Permissions created");

// Roles oluştur
var adminRole = new Role { Name = "Admin", Description = "Sistem yöneticisi", TenantId = HostTenant.Id };
var resellerRole = new Role { Name = "Reseller", Description = "Bayi", TenantId = HostTenant.Id };
var customerRole = new Role { Name = "Customer", Description = "Müşteri", TenantId = HostTenant.Id };

context.Roles.AddRange(adminRole, resellerRole, customerRole);
context.SaveChanges();
Console.WriteLine("Roles created");

// Role Permissions ata
foreach (var permission in permissions)
{
    context.RolePermissions.Add(new RolePermission { RoleId = adminRole.Id, PermissionId = permission.Id });
}

context.RolePermissions.AddRange(
    new RolePermission { RoleId = resellerRole.Id, Permission = permissions.First(p => p.Name == "user.read") },
    new RolePermission { RoleId = resellerRole.Id, Permission = permissions.First(p => p.Name == "user.create") },
    new RolePermission { RoleId = customerRole.Id, Permission = permissions.First(p => p.Name == "user.read") }
);

context.SaveChanges();
Console.WriteLine("Role permissions assigned");

// Host User oluştur
var HostUser = new User
{
    Username = "admin",
    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
    Email = "admin@test.com",
    IsActive = true,
    Tenant = HostTenant
};
context.Users.Add(HostUser);
context.SaveChanges();

// Host User profili oluştur
var hostProfile = new UserProfile
{
    UserId = HostUser.Id,
    PhoneNumber = "+905555555555",
    IsPhoneVerified = false,
    IsSmsVerificationEnabled = false,
    IsEmailVerificationEnabled = false,
    IsGoogleAuthEnabled = false
};
context.UserProfiles.Add(hostProfile);
context.SaveChanges();

// Admin rolünü ata
context.UserRoles.Add(new UserRole { UserId = HostUser.Id, RoleId = adminRole.Id });
context.SaveChanges();
Console.WriteLine("Host User Created with Admin role");

// Reseller Tenant oluştur
var ResellerTenant = new Tenant
{
    Name = "Test Reseller",
    Domain = "reseller.localhost",
    Credit = 10000,
    ParentId = HostTenant.Id,
    Description = "reseller test",
    IsActive = true,
};
context.Tenants.Add(ResellerTenant);
context.SaveChanges();
Console.WriteLine("Reseller Tenant created");

// Reseller User oluştur
var ResellerUser = new User
{
    Username = "reseller",
    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
    Email = "reseller@test.com",
    IsActive = true,
    Tenant = ResellerTenant
};
context.Users.Add(ResellerUser);
context.SaveChanges();

// Reseller User profili oluştur
var resellerProfile = new UserProfile
{
    UserId = ResellerUser.Id,
    PhoneNumber = "+905555555556",
    IsPhoneVerified = false,
    IsSmsVerificationEnabled = false,
    IsEmailVerificationEnabled = false,
    IsGoogleAuthEnabled = false
};
context.UserProfiles.Add(resellerProfile);
context.SaveChanges();

// Reseller rolünü ata
context.UserRoles.Add(new UserRole { UserId = ResellerUser.Id, RoleId = resellerRole.Id });
context.SaveChanges();
Console.WriteLine("Reseller User Created with Reseller role");

// Customer Tenant oluştur
var CustomerTenant = new Tenant
{
    Name = "Test Customer",
    Domain = "customer.localhost",
    Credit = 10000,
    ParentId = ResellerTenant.Id,
    IsActive = true
};
context.Tenants.Add(CustomerTenant);
context.SaveChanges();
Console.WriteLine("Customer Tenant created");

// Customer User oluştur
var CustomerUser = new User
{
    Username = "customer",
    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
    Email = "customer@test.com",
    IsActive = true,
    Tenant = CustomerTenant
};
context.Users.Add(CustomerUser);
context.SaveChanges();

// Customer User profili oluştur
var customerProfile = new UserProfile
{
    UserId = CustomerUser.Id,
    PhoneNumber = "+905555555557",
    IsPhoneVerified = false,
    IsSmsVerificationEnabled = false,
    IsEmailVerificationEnabled = false,
    IsGoogleAuthEnabled = false
};
context.UserProfiles.Add(customerProfile);
context.SaveChanges();

// Customer rolünü ata
context.UserRoles.Add(new UserRole { UserId = CustomerUser.Id, RoleId = customerRole.Id });
context.SaveChanges();
Console.WriteLine("Customer User Created with Customer role");

Console.WriteLine("Seeding completed successfully!");