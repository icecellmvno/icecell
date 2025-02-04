using IceSMS.API.Data;
using IceSMS.API.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
optionsBuilder.UseNpgsql("Host=localhost;Database=smsmerkezi;Username=admin_smsmerkezi;Password=wrpQuWwNR3EY");
ApplicationDbContext context = new ApplicationDbContext(optionsBuilder.Options);

var tenants = context.Tenants.ToList();

context.Database.ExecuteSqlRaw("DELETE FROM \"Users\"");
context.Database.ExecuteSqlRaw("DELETE FROM \"Tenants\"");

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
var HostUser = new User()
{
    Username = "admin",
    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
    Email = "admin@test.com",
    IsActive = true,
    Tenant = HostTenant,
    
};
context.Users.Add(HostUser);
context.SaveChanges();
Console.WriteLine("Host User Created");
var ResellerTenant = new Tenant
{
    Name = "Test Host",
    Domain = "reseller.localhost",
    Credit = 10000,
    ParentId = HostTenant.Id,
    Description = "reseller test",
    IsActive = true,
};
context.Tenants.Add(ResellerTenant);
context.SaveChanges();
Console.WriteLine("Reseller Tenant created");
var ResellerUser = new User()
{
    Username = "reseller",
    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
    Email = "reseller@test.com",
    IsActive = true,
    Tenant = ResellerTenant,
};
context.Users.Add(ResellerUser);
context.SaveChanges();
Console.WriteLine("Reseller User Created");
var CustomerTenant = new Tenant
{
    Name = "Test Host",
    Domain = "customer.localhost",
    Credit = 10000,
    ParentId = ResellerTenant.Id
};
context.Tenants.Add(CustomerTenant);
context.SaveChanges();
Console.WriteLine("Customer Tenant created");
var CustomerUser = new User()
{
    Username = "customer",
    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
    Email = "customer@test.com",
    IsActive = true,
    Tenant = CustomerTenant
 
};
context.Users.Add(CustomerUser);
context.SaveChanges();
Console.WriteLine("Customer User Created");