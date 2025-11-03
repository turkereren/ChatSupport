// Data/ChatDbContextFactory.cs
using System;
using ChatSupport.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ChatSupport.Data
{
    /// <summary>
    /// EF Core CLI, migrations ve database update işlemleri için
    /// tasarım-zamanı doğru DbContext örneğini bu factory'den alır.
    /// </summary>
    public class ChatDbContextFactory : IDesignTimeDbContextFactory<ChatDbContext>
    {
        public ChatDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<ChatDbContext>();
            builder.UseSqlite("Data Source=chatsupport.db");

            return new ChatDbContext(builder.Options);
        }
    }
}
