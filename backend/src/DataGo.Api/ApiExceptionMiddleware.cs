using DataGo.Application;
using DataGo.Domain;
using Microsoft.AspNetCore.Mvc;

namespace DataGo.Api;

public sealed class ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            var (status, title, detail) = exception switch
            {
                DomainValidationException => (StatusCodes.Status400BadRequest, "Error de validación", exception.Message),
                DomainConflictException => (StatusCodes.Status409Conflict, "Conflicto", exception.Message),
                NotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado", exception.Message),
                ConflictException => (StatusCodes.Status409Conflict, "Conflicto", exception.Message),
                AssistantProviderException provider when provider.Code == "not_configured" =>
                    (StatusCodes.Status503ServiceUnavailable, "Asistente no configurado", provider.Message),
                AssistantProviderException provider when provider.Code == "invalid_response" =>
                    (StatusCodes.Status502BadGateway, "Respuesta de IA inválida", provider.Message),
                AssistantProviderException provider when provider.Code == "invalid_request" =>
                    (StatusCodes.Status502BadGateway, "Configuración de IA rechazada", provider.Message),
                AssistantProviderException provider when provider.Code is "invalid_credentials" or "quota_exhausted" =>
                    (StatusCodes.Status503ServiceUnavailable, "Configuración de OpenRouter no disponible", provider.Message),
                AssistantProviderException provider when provider.Code == "rate_limited" =>
                    (StatusCodes.Status503ServiceUnavailable, "Límite temporal de OpenRouter", provider.Message),
                AssistantProviderException provider =>
                    (StatusCodes.Status503ServiceUnavailable, "Proveedor de IA no disponible", provider.Message),
                _ => (StatusCodes.Status500InternalServerError, "Error interno", "Ocurrió un error inesperado")
            };
            if (status == 500) logger.LogError(exception, "Unhandled request error");
            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = status, Title = title, Detail = detail, Instance = context.Request.Path
            });
        }
    }
}
