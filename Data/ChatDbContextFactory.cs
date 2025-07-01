
using System;
using ChatSupport.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace ChatSupport.Data
{
    /// <summary>
    
    /// </summary>
    public class ChatDbContextFactory : IDesignTimeDbContextFactory<ChatDbContext>
    {
        public ChatDbContext CreateDbContext(string[] args)
        {
            
            var connectionString =
                "Server=localhost;Database=ChatDb;User=chatuser;Password=12345;";

            var builder = new DbContextOptionsBuilder<ChatDbContext>();
            builder.UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 0, 26)),
                mysqlOptions => mysqlOptions.EnableRetryOnFailure()
            );

            return new ChatDbContext(builder.Options);
        }
    }
}
