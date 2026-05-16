using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace backend.api.Middleware
{
    public sealed class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext ctx)
        {
            try
            {
                await _next(ctx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception for {Method} {Path}",
                    ctx.Request.Method, ctx.Request.Path);

                await WriteErrorResponseAsync(ctx, ex);
            }
        }

        private static async Task WriteErrorResponseAsync(HttpContext ctx, Exception ex)
        {
            var (status, title) = ex switch
            {
                ArgumentException => (HttpStatusCode.BadRequest, "Bad Request"),
                UnauthorizedAccessException => (HttpStatusCode.Forbidden, "Forbidden"),
                KeyNotFoundException => (HttpStatusCode.NotFound, "Not Found"),
                _ => (HttpStatusCode.InternalServerError, "Internal Server Error"),
            };

            var problem = new ProblemDetails
            {
                Status = (int)status,
                Title = title,
                Detail = ex.Message,
            };

            ctx.Response.StatusCode = (int)status;
            ctx.Response.ContentType = "application/problem+json";

            await ctx.Response.WriteAsync(
                JsonSerializer.Serialize(problem, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                }));
        }
    }

}
