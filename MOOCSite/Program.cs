using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Конфигурация
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:44345/";

// Добавление сервисов
builder.Services.AddControllersWithViews();

// Регистрация HttpClient с настройками
builder.Services.AddHttpClient("MOOCApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    // Отключаем проверку SSL только в development
    ServerCertificateCustomValidationCallback = builder.Environment.IsDevelopment()
        ? (_, _, _, _) => true
        : null
});

var app = builder.Build();

// Конфигурация конвейера HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();  // Добавляем аутентификацию
app.UseAuthorization();

// Маршрутизация
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();