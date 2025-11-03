using System;
using Microsoft.EntityFrameworkCore;
using ChatSupport.Models;

namespace ChatSupport.Data
{
    public class ChatDbContext : DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options)
            : base(options) { }

        // Tasarım-zamanı veya hiç Configure edilmemişse burası devreye girer
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Eğer DI'den gelen optionsBuilder boşsa veya connection string boşsa
            if (!optionsBuilder.IsConfigured)
            {
                var fallback = "Data Source=chatsupport.db";
                optionsBuilder.UseSqlite(fallback);
            }
        }

        public DbSet<ChatSession> ChatSessions { get; set; } = default!;
        public DbSet<ChatMessage> ChatMessages { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.Entity<ChatSession>(e =>
            {
                e.HasKey(x => x.ChatId);
                e.Property(x => x.ChatId).HasMaxLength(32);
                e.Property(x => x.UserName).HasMaxLength(100);
            });

            mb.Entity<ChatMessage>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.ChatId).HasMaxLength(32);
                e.Property(x => x.Sender).HasMaxLength(100);
                e.HasOne<ChatSession>()
                 .WithMany()
                 .HasForeignKey(m => m.ChatId)
                 .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
