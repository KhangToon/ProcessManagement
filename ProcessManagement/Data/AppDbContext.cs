using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProcessManagement.Models;
using ProcessManagement.Models.Authorization;

namespace ProcessManagement.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions options) : base(options) { }

        // DbSet cho UserPermission
        public DbSet<UserPermission> UserPermissions { get; set; }
        
        // DbSet cho RolePermission (lưu quyền mặc định cho role)
        public DbSet<RolePermission> RolePermissions { get; set; }
        
        // DbSet cho Permission (quản lý permissions động)
        public DbSet<Permission> Permissions { get; set; }
        
        // DbSet cho ButtonRole (mapping button với role)
        public DbSet<ButtonRole> ButtonRoles { get; set; }
        
        // DbSet cho UserPassword (lưu password đã encrypt)
        public DbSet<UserPassword> UserPasswords { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // them default role 
            var admin = new IdentityRole("admin");
            admin.NormalizedName = "admin";

            var client = new IdentityRole("client");
            client.NormalizedName = "client";

            builder.Entity<IdentityRole>().HasData(admin, client);
            // them default role 

            // Cấu hình UserPermission
            builder.Entity<UserPermission>(entity =>
            {
                entity.HasKey(up => new { up.UserId, up.Permission });
                entity.HasIndex(up => up.UserId);
                entity.HasIndex(up => up.Permission);
                
                entity.HasOne(up => up.User)
                    .WithMany()
                    .HasForeignKey(up => up.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Cấu hình RolePermission
            builder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(rp => new { rp.RoleName, rp.Permission });
                entity.HasIndex(rp => rp.RoleName);
                entity.HasIndex(rp => rp.Permission);
            });

            // Cấu hình ButtonRole
            builder.Entity<ButtonRole>(entity =>
            {
                entity.HasKey(br => br.Id);
                entity.HasIndex(br => br.ButtonId);
                entity.HasIndex(br => br.PagePath);
                entity.HasIndex(br => new { br.ButtonId, br.RoleName });
                entity.Property(br => br.ButtonId).HasMaxLength(200);
                entity.Property(br => br.PagePath).HasMaxLength(500);
                entity.Property(br => br.ButtonText).HasMaxLength(200);
                entity.Property(br => br.RoleName).HasMaxLength(256);
            });

            // Cấu hình Permission
            builder.Entity<Permission>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.PermissionCode).IsUnique();
                entity.HasIndex(p => p.Module);
                entity.Property(p => p.PermissionCode).HasMaxLength(200);
                entity.Property(p => p.Module).HasMaxLength(100);
                entity.Property(p => p.Action).HasMaxLength(50);
                entity.Property(p => p.DisplayName).HasMaxLength(200);
            });

            // Cấu hình ButtonRole
            builder.Entity<ButtonRole>(entity =>
            {
                entity.HasKey(br => br.Id);
                entity.HasIndex(br => br.ButtonId);
                entity.HasIndex(br => br.PagePath);
                entity.HasIndex(br => new { br.ButtonId, br.RoleName });
                entity.Property(br => br.ButtonId).HasMaxLength(200);
                entity.Property(br => br.PagePath).HasMaxLength(500);
                entity.Property(br => br.ButtonText).HasMaxLength(200);
                entity.Property(br => br.RoleName).HasMaxLength(256);
            });

            // Cấu hình UserPassword
            builder.Entity<UserPassword>(entity =>
            {
                entity.HasKey(up => up.UserId);
                entity.HasIndex(up => up.UserId);
                
                entity.HasOne(up => up.User)
                    .WithOne()
                    .HasForeignKey<UserPassword>(up => up.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            base.OnModelCreating(builder);
        }
    }
}
