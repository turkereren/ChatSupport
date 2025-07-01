using ChatSupport.Data;
using ChatSupport.Hubs;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);


var connString = builder.Configuration.GetConnectionString("MySqlConnection");
if (string.IsNullOrWhiteSpace(connString))
{
    Console.WriteLine("[Warning] MySqlConnection boş, sabit fallback kullanılacak.");
    connString = "Server=127.0.0.1;Port=3306;Database=ChatDb;Uid=chatuser;Pwd=12345;";
}

// 1) Connect DbContest to MySQL server
var serverVersion = new MySqlServerVersion(new Version(8, 0, 26));
builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseMySql(
        connString,
        serverVersion,
        mySqlOptions => mySqlOptions.EnableRetryOnFailure()
    )
);

// 2) SignalR & Swagger
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ChatSupport API", Version = "v1" });
});

var app = builder.Build();

// 3) Swagger UI in development mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 4) Static files + Hub endpoint
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapHub<ChatHub>("/chatHub");

// 6) Apply migrations when application runs
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    db.Database.Migrate();
}

app.Run();
