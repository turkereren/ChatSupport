// Data/ChatDbContextFactory.cs
using System;
using ChatSupport.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace ChatSupport.Data
{
    /// <summary>
    /// EF Core CLI, migrations ve database update i�lemleri i�in
    /// tasar�m-zaman� do�ru DbContext �rne�ini bu factory�den al�r.
    /// </summary>
    public class ChatDbContextFactory : IDesignTimeDbContextFactory<ChatDbContext>
    {
        public ChatDbContext CreateDbContext(string[] args)
        {
            // Burada appsettings okumak yerine sabit yaz�yoruz
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
