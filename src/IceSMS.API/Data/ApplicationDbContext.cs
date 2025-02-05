using Microsoft.EntityFrameworkCore;
using IceSMS.API.Models.Domain;

namespace IceSMS.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<PanelSettings> PanelSettings => Set<PanelSettings>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserProfile> UserProfiles { get; set; } = null!;
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<ContactGroup> ContactGroups => Set<ContactGroup>();
    public DbSet<Blacklist> Blacklists => Set<Blacklist>();
    public DbSet<CustomField> CustomFields => Set<CustomField>();
    public DbSet<CustomFieldValue> CustomFieldValues => Set<CustomFieldValue>();
    public DbSet<SmsTitlesModel> SmsTitles => Set<SmsTitlesModel>();
    public DbSet<Vendors> Vendors => Set<Vendors>();
    public DbSet<VendorsConnectionParameters> VendorsConnectionParameters => Set<VendorsConnectionParameters>();
    public DbSet<VendorsActionParameters> VendorsActionParameters => Set<VendorsActionParameters>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>()
            .HasOne(u => u.Tenant)
            .WithMany(t => t.Users)
            .HasForeignKey(u => u.TenantId);

        // UserRole composite key
        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<User>()
            .HasMany(u => u.Roles)
            .WithMany(r => r.Users)
            .UsingEntity<UserRole>();

        modelBuilder.Entity<User>()
            .HasOne(u => u.Profile)
            .WithOne(p => p.User)
            .HasForeignKey<UserProfile>(p => p.UserId);

        // Role
        modelBuilder.Entity<Role>()
            .HasOne(r => r.Tenant)
            .WithMany(t => t.Roles)
            .HasForeignKey(r => r.TenantId);

        // RolePermission composite key
        modelBuilder.Entity<RolePermission>()
            .HasKey(rp => new { rp.RoleId, rp.PermissionId });

        modelBuilder.Entity<Role>()
            .HasMany(r => r.Permissions)
            .WithMany(p => p.Roles)
            .UsingEntity<RolePermission>();

        // Contact
        modelBuilder.Entity<Contact>()
            .HasOne(c => c.Tenant)
            .WithMany()
            .HasForeignKey(c => c.TenantId);

        modelBuilder.Entity<Contact>()
            .HasMany(c => c.Groups)
            .WithMany(g => g.Contacts)
            .UsingEntity(j => j.ToTable("ContactGroupMembers"));

        // ContactGroup
        modelBuilder.Entity<ContactGroup>()
            .HasOne(g => g.Tenant)
            .WithMany()
            .HasForeignKey(g => g.TenantId);

        // Blacklist
        modelBuilder.Entity<Blacklist>()
            .HasOne(b => b.Tenant)
            .WithMany()
            .HasForeignKey(b => b.TenantId);

        modelBuilder.Entity<Blacklist>()
            .HasIndex(b => new { b.PhoneNumber, b.TenantId })
            .IsUnique();

        // PanelSettings
        modelBuilder.Entity<PanelSettings>()
            .HasOne(p => p.Tenant)
            .WithMany()
            .HasForeignKey(p => p.TenantId);

        // Unique indexes
        modelBuilder.Entity<User>()
            .HasIndex(u => new { u.Username, u.TenantId })
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => new { u.Email, u.TenantId })
            .IsUnique();

        modelBuilder.Entity<Role>()
            .HasIndex(r => new { r.Name, r.TenantId })
            .IsUnique();

        modelBuilder.Entity<Permission>()
            .HasIndex(p => p.Name)
            .IsUnique();

        modelBuilder.Entity<Tenant>()
            .HasIndex(t => t.Domain)
            .IsUnique();

        modelBuilder.Entity<PanelSettings>()
            .HasIndex(p => new { p.Name, p.TenantId })
            .IsUnique();

        // Tenant self-referencing relationship
        modelBuilder.Entity<Tenant>()
            .HasOne(t => t.Parent)
            .WithMany(t => t.Children)
            .HasForeignKey(t => t.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        // CustomField
        modelBuilder.Entity<CustomField>()
            .HasOne(f => f.Tenant)
            .WithMany()
            .HasForeignKey(f => f.TenantId);

        modelBuilder.Entity<CustomField>()
            .HasIndex(f => new { f.Name, f.TenantId })
            .IsUnique();

        // CustomFieldValue
        modelBuilder.Entity<CustomFieldValue>()
            .HasOne(v => v.Contact)
            .WithMany(c => c.CustomFields)
            .HasForeignKey(v => v.ContactId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CustomFieldValue>()
            .HasOne(v => v.CustomField)
            .WithMany(f => f.Values)
            .HasForeignKey(v => v.CustomFieldId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SmsTitlesModel>().HasIndex(s => s.TenantId);

        // Vendors ilişkileri
        modelBuilder.Entity<VendorsConnectionParameters>()
            .HasOne<Vendors>()
            .WithMany()
            .HasForeignKey(v => v.VendorId);

        modelBuilder.Entity<VendorsActionParameters>()
            .HasOne<Vendors>()
            .WithMany()
            .HasForeignKey(v => v.VendorId);

        modelBuilder.Entity<VendorsActionParameters>()
            .HasOne<VendorsConnectionParameters>()
            .WithMany()
            .HasForeignKey(v => v.VendorConnectionParamtersId);
    }
} 