using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MOOCAPI.Data;
using MOOCAPI.Models;
using System.Linq.Dynamic.Core;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Добавляем сервисы в контейнер
builder.Services.AddControllers();

// Подключаем Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Настройка контекста базы данных
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

var app = builder.Build();

// Настройка конвейера HTTP запросов
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Маршрутизация контроллеров
app.MapControllers();

// Автомиграция базы данных (только для разработки!)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!dbContext.Users.Any(u => u.Login == "admin"))
    {
        var adminUser = new User
        {
            Login = "admin",
            Password = "admin123", // В реальном проекте используйте хеширование!
            Email = "admin@example.com",
            FirstName = "Admin",
            LastName = "Admin",
            IsActive = true
        };

        dbContext.Users.Add(adminUser);
        await dbContext.SaveChangesAsync();
    }
}
app.Run();