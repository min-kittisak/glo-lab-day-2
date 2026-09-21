using Serilog.Context;

namespace IdentityService.Api.Middlewares
{
    public class CorrelationIdMiddleware
    {
        private const string CorrelationIdHeader = "X-Correlation-ID";

        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context)
        {
            var correlationId =
                context.Request.Headers
                    .TryGetValue(
                        CorrelationIdHeader,
                        out var headerValue)
                && !string.IsNullOrWhiteSpace(headerValue)
                    ? headerValue.ToString()
                    : Guid.NewGuid().ToString();

            context.Response.Headers[
                CorrelationIdHeader] = correlationId;

            using (LogContext.PushProperty(
                       "CorrelationId",
                       correlationId))
            {
                await _next(context);
            }
        }
    }
}
