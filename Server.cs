using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyApiApp.Data;
using MyApiApp.Services;
using System.Text;

public class Server
{
    public static async Task Run()
    {
        var builder = WebApplication.CreateBuilder();

        // Настройка подключения к базам данных
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite("Data Source=DB/users.db"));

        builder.Services.AddDbContext<ContactDbContext>(options =>
            options.UseSqlite("Data Source=DB/contacts.db"));

        builder.Services.AddDbContext<HistoryDbContext>(options =>
            options.UseSqlite("Data Source=DB/history.db"));

        // Настройка JWT токена
        var secretKey = builder.Configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("JWT secret key не собран");
        }
        var key = Encoding.ASCII.GetBytes(secretKey);

        // Регистрация сервисов
        builder.Services.AddSingleton(new TokenService(secretKey));
        builder.Services.AddSingleton<PasswordService>();

        // Добавление кеширования и сессий
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        // Настройка аутентификации и авторизации
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

        // Добавление авторизации
        builder.Services.AddAuthorization();
        builder.Services.AddControllers();

        // Создание и запуск приложения
        var app = builder.Build();

        app.UseStaticFiles();
        app.UseSession();
        app.UseAuthentication();
        app.UseAuthorization();  
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.MapControllers();

        // Создаем базы данных и таблицы при старте приложения
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

            var userDbContext = services.GetRequiredService<ApplicationDbContext>();
            await BDback.InitializeUserDatabase(userDbContext);

            var contactDbContext = services.GetRequiredService<ContactDbContext>();
            await BDback.InitializeContactDatabase(contactDbContext);

            var historyDbContext = services.GetRequiredService<HistoryDbContext>();
            await BDback.InitializeHistoryDatabase(historyDbContext);
        }

        await app.RunAsync("http://localhost:5194");
    }
}
