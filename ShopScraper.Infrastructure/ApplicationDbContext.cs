namespace ShopScraper.Infrastructure;

using Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }
    

    protected override void OnModelCreating(ModelBuilder builder)
    {
        SeedRolesAndUsers(builder);
        
        base.OnModelCreating(builder);
    }

    private void SeedRolesAndUsers(ModelBuilder builder)
    {
        var roles = new[]
        {
            new IdentityRole
            {
                Id = "efd1518a-5357-4836-8e92-23ad222b9637",
                Name = "Administrator",
                NormalizedName = "ADMINISTRATOR"
            }
        };

        var hasher = new PasswordHasher<ApplicationUser>();

        var adminUser = new ApplicationUser
        {
            Id = "d5c0f85b-3d01-4f65-b3de-818c37d6b9b0",
            UserName = "admin@example.com",
            NormalizedUserName = "ADMIN@EXAMPLE.COM",
            Email = "admin@example.com",
            RealName = "Иван Темнохолмов", 
            NormalizedEmail = "ADMIN@EXAMPLE.COM",
            EmailConfirmed = true,
            LockoutEnabled = false,
            SecurityStamp = "d5007d0d-e59f-4d00-af11-d6b4274bef3e",
            PasswordHash = hasher.HashPassword(null!, "123")
        };

        var adminRole = new IdentityUserRole<string>
        {
            RoleId = "efd1518a-5357-4836-8e92-23ad222b9637",
            UserId = "d5c0f85b-3d01-4f65-b3de-818c37d6b9b0"
        };

        builder.Entity<IdentityRole>().HasData(roles);
        builder.Entity<ApplicationUser>().HasData(adminUser);
        builder.Entity<IdentityUserRole<string>>().HasData(adminRole);
    }
}