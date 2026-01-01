using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace PortfolioAPI.Middleware
{
    public class RequestCorrelationMiddleware
    {
        private const string HeaderName = "X-Request-Id";
        private readonly RequestDelegate _next;

        public RequestCorrelationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Use incoming request ID if present, otherwise generate one
            var requestId = context.Request.Headers.ContainsKey(HeaderName)
                ? context.Request.Headers[HeaderName].ToString()
                : Guid.NewGuid().ToString();

            // Store for downstream usage (logging, diagnostics, etc.)
            context.Items[HeaderName] = requestId;

            // Always return the request ID to the client
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[HeaderName] = requestId;
                return Task.CompletedTask;
            });

            await _next(context);
        }
    }
}
