using ChatSupport.Data;
using ChatSupport.Hubs;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1) Connection string'i önce appsettings.json'den çek
var connString = builder.Configuration.GetConnectionString("SqliteConnection");
if (string.IsNullOrWhiteSpace(connString))
{
    Console.WriteLine("[Warning] SqliteConnection boş, varsayılan kullanılacak.");
    connString = "Data Source=chatsupport.db";
}

// 2) DbContext'i SQLite'a bağla
builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseSqlite(connString)
);

// 3) CORS - Dış web sitelerinden erişim için
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 4) Controllers & SignalR & Swagger
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "ChatSupport API", 
        Version = "v1",
        Description = "Web sitelerine entegre edilebilir canlı destek API'si"
    });
});

var app = builder.Build();

// 5) Geliştirme ortamında Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ChatSupport API V1");
        c.RoutePrefix = "swagger";
    });
}

// 6) CORS, Static Files, Routing
app.UseCors("AllowAll");
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();

// 7) Map Controllers & Hub
app.MapControllers();
app.MapHub<ChatHub>("/chatHub");

// 8) Uygulama ayağa kalkarken migrations'ı uygula
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    try
    {
        db.Database.Migrate();
        Console.WriteLine("[INFO] Database migrations uygulandı.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] Migration hatası: {ex.Message}");
    }
}

Console.WriteLine($"[INFO] ChatSupport API başlatıldı.");
Console.WriteLine($"[INFO] Admin Panel: http://localhost:5000/index_admin.html");
Console.WriteLine($"[INFO] User Chat: http://localhost:5000/index_user_popup.html");
Console.WriteLine($"[INFO] Swagger: http://localhost:5000/swagger");

app.Run();
