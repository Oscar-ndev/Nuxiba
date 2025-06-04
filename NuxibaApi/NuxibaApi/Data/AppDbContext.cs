using Microsoft.EntityFrameworkCore;
using NuxibaApi.Models;

namespace NuxibaApi.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Login> Logins => Set<Login>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Area> Area => Set<Area>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Login>()
                .ToTable("ccloglogin") 
                .HasOne(l => l.User)
                .WithMany(u => u.Logins)
                .HasForeignKey(l => l.User_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .ToTable("ccUsers"); 

            modelBuilder.Entity<Area>()
                .ToTable("ccRIACat_Areas"); 
        }
    }
}
