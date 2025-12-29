using Microsoft.EntityFrameworkCore;
using TechAssist.Models;

namespace TechAssist.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketReply> TicketReplies { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Users
            modelBuilder.Entity<User>(b =>
            {
                b.HasKey(u => u.Id);
                b.HasIndex(u => u.Email).IsUnique();
                b.Property(u => u.Email).IsRequired().HasMaxLength(200);
                b.Property(u => u.FullName).IsRequired().HasMaxLength(200);
                b.Property(u => u.Role).HasConversion<string>().HasMaxLength(50).IsRequired();
            });

            // Product
            modelBuilder.Entity<Product>(b =>
            {
                b.HasKey(p => p.Id);
                b.Property(p => p.Name).IsRequired().HasMaxLength(200);
            });

            // Ticket
            modelBuilder.Entity<Ticket>(b =>
            {
                b.HasKey(t => t.Id);
                b.Property(t => t.Title).IsRequired().HasMaxLength(250);
                b.Property(t => t.Priority).HasMaxLength(50).IsRequired();
                b.Property(t => t.Status).HasMaxLength(50).IsRequired();

                b.HasOne(t => t.Creator)
                 .WithMany(u => u.CreatedTickets)
                 .HasForeignKey(t => t.CreatorId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(t => t.AssignedEngineer)
                 .WithMany(u => u.AssignedTickets)
                 .HasForeignKey(t => t.AssignedEngineerId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(t => t.Product)
                 .WithMany()
                 .HasForeignKey(t => t.ProductId)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            // TicketReply
            modelBuilder.Entity<TicketReply>(b =>
            {
                b.HasKey(r => r.Id);
                b.Property(r => r.Message).IsRequired();

                b.HasOne(r => r.Ticket)
                 .WithMany(t => t.Replies)
                 .HasForeignKey(r => r.TicketId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(r => r.Author)
                 .WithMany(u => u.Replies)
                 .HasForeignKey(r => r.AuthorId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ActivityLog
            modelBuilder.Entity<ActivityLog>(b =>
            {
                b.HasKey(a => a.Id);
                b.Property(a => a.ActionType).IsRequired().HasMaxLength(100);
                b.HasOne(a => a.Actor)
                 .WithMany(u => u.ActivityLogs)
                 .HasForeignKey(a => a.ActorId)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            // Seed an admin user (example) - password hash must be real hashed value in production
            //modelBuilder.Entity<User>().HasData(new User
            //{
            //    Id = 1,
            //    FullName = "Super Admin",
            //    Email = "admin@example.com",
            //    PasswordHash = "admin@123", // replace with real hash
            //    Role = Role.Admin,
            //    CreatedAt = DateTime.UtcNow,
            //    IsActive = true
            //});
        }
    }
}
