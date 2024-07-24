using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SchoolManagement.Data.Identity;

public class IdentityManagementDbContext : IdentityDbContext<User>
{
    public IdentityManagementDbContext(DbContextOptions<IdentityManagementDbContext> options) : base(options) { }
    
    public virtual DbSet<User> User { get; set; }

    public virtual DbSet<Role> Role { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>().Property(e => e.Id).HasMaxLength(50).IsRequired();

        modelBuilder.Entity<User>().Property(e => e.Id).HasMaxLength(50).IsRequired();
    }
}