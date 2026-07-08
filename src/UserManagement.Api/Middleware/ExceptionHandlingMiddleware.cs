using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace UserManagement.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore non gestito durante l'elaborazione della richiesta {Path}", context.Request.Path);

            var problem = new ProblemDetails
            {
                Title = "Si è verificato un errore imprevisto",
                Status = (int)HttpStatusCode.InternalServerError,
                Detail = "Contattare l'amministratore di sistema se il problema persiste."
            };

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = problem.Status.Value;
            await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }
}
