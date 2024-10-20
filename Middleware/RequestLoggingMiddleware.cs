using MyApiApp.Data;
using MyApiApp.Models;
using System.Security.Claims;
using System.Text;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Request.EnableBuffering(); // Включаем возможность повторного чтения потока

        using (var scope = context.RequestServices.CreateScope())
        {
            var historyDbContext = scope.ServiceProvider.GetRequiredService<HistoryDbContext>();

            var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim, out int userId))
            {
                string bodyContent = string.Empty;

                // Читаем тело запроса, если это не GET запрос
                if (context.Request.Method != HttpMethods.Get)
                {
                    context.Request.Body.Position = 0;
                    using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
                    {
                        bodyContent = await reader.ReadToEndAsync();
                        context.Request.Body.Position = 0; // Сбрасываем позицию обратно
                    }
                }

                var historyRecord = new History
                {
                    UserId = userId,
                    HttpMethod = context.Request.Method,
                    Path = context.Request.Path,
                    QueryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : "",
                    Timestamp = DateTime.UtcNow,
                    BodyContent = bodyContent // Сохраняем тело запроса
                };

                historyDbContext.History.Add(historyRecord);
                try
                {
                    await historyDbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при сохранении данных: {ex.Message}");
                }
            }
        }

        await _next(context);
    }

}
