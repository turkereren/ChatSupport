using ChatSupport.Data;
using ChatSupport.Hubs;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
// Pomelo’nun MySQL altyapı uzantılarını getiren using
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 1) Connection string’i önce appsettings.json’den çek, yoksa fallback’e dön
var connString = builder.Configuration.GetConnectionString("MySqlConnection");
if (string.IsNullOrWhiteSpace(connString))
{
    Console.WriteLine("[Warning] MySqlConnection boş, sabit fallback kullanılacak.");
    connString = "Server=127.0.0.1;Port=3306;Database=ChatDb;Uid=chatuser;Pwd=12345;";
}

// 2) DbContext’i MySQL’e bağla ve retry-on-failure etkinleştir
var serverVersion = new MySqlServerVersion(new Version(8, 0, 26));
builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseMySql(
        connString,
        serverVersion,
        mySqlOptions => mySqlOptions.EnableRetryOnFailure()
    )
);

// 3) SignalR & Swagger
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ChatSupport API", Version = "v1" });
});

var app = builder.Build();

// 4) Geliştirme ortamında Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 5) Statik dosyalar + Hub endpoint
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapHub<ChatHub>("/chatHub");

// 6) Uygulama ayağa kalkarken migrations’ı uygula
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    db.Database.Migrate();
}

app.Run();
