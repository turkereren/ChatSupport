using System;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatSupport.Data
{
    public class ChatDbContext : DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options)
            : base(options) { }

        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            
            if (!optionsBuilder.IsConfigured ||
                optionsBuilder.Options.Extensions
                    .OfType<Microsoft.EntityFrameworkCore.Infrastructure.RelationalOptionsExtension>()
                    .Any(ext => string.IsNullOrWhiteSpace(ext.ConnectionString)))
            {
                var fallback = "Server=127.0.0.1;Port=3306;Database=ChatDb;Uid=chatuser;Pwd=12345;";
                optionsBuilder.UseMySql(
                    fallback,
                    new MySqlServerVersion(new Version(8, 0, 26)),
                    mysqlOpts => mysqlOpts.EnableRetryOnFailure()
                );
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

    [Table("ChatSessions")]
    public class ChatSession
    {
        [Key, MaxLength(32)]
        public string ChatId { get; set; } = default!;

        [MaxLength(100)]
        public string UserName { get; set; } = default!;

        public bool ClaimedBySupport { get; set; }
    }

    [Table("ChatMessages")]
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(32)]
        public string ChatId { get; set; } = default!;

        [MaxLength(100)]
        public string Sender { get; set; } = default!;

        public string Content { get; set; } = default!;

        public DateTime Timestamp { get; set; }
    }
}
