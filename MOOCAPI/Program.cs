using Microsoft.EntityFrameworkCore;
using MOOCAPI.Data;
using System.Linq.Dynamic.Core;

var builder = WebApplication.CreateBuilder(args);

// ��������� ������� � ���������
builder.Services.AddControllers();

// ���������� Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ��������� ��������� ���� ������
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// ��������� ��������� HTTP ��������
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// ������������� ������������
app.MapControllers();

// ������������ ���� ������ (������ ��� ����������!)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

app.Run();