using System.Net;
using System.Text.Json;
using TakeNote.Service.DTOs; // Hata modeli için DTO kullanabilir veya anonim obje dönebiliriz

namespace TakeNote.API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // İsteği bir sonraki adıma ilet (Controller'a git)
                await _next(context);
            }
            catch (Exception ex)
            {
                // Bir hata patlarsa yakala, logla ve JSON dön
                _logger.LogError(ex, "Beklenmeyen bir hata oluştu: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            // Hata tipine göre durum kodu belirle
            context.Response.StatusCode = ex switch
            {
                UnauthorizedAccessException => (int)HttpStatusCode.Forbidden, // 403
                KeyNotFoundException => (int)HttpStatusCode.NotFound,         // 404
                ArgumentException => (int)HttpStatusCode.BadRequest,          // 400
                _ => (int)HttpStatusCode.InternalServerError                  // 500
            };

            var response = new
            {
                StatusCode = context.Response.StatusCode,
                Message = ex.Message,
                // Development ortamındaysak hatanın detayını göster, değilse gizle (Güvenlik)
                Details = _env.IsDevelopment() ? ex.StackTrace?.ToString() : "Internal Server Error"
            };

            var json = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(json);
        }
    }
}