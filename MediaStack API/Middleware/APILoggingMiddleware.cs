using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MediaStack_API.Middleware
{
    public class APILoggingMiddleware
    {
        #region Data members

        private readonly RequestDelegate next;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public APILoggingMiddleware(RequestDelegate next, ILogger<APILoggingMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        #endregion

        #region Methods

        public async Task Invoke(HttpContext context)
        {
            await this.LogResponse(context);
        }

        private async Task LogResponse(HttpContext context)
        {
            await this.next(context);
            this.logger.LogInformation(
                $"{context.Request.Method} -> {context.Request.Path} ({context.Response.StatusCode})");
        }

        #endregion
    }

    public static class APILoggingMiddlewareExtensions
    {
        #region Methods

        public static IApplicationBuilder UseAPILogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<APILoggingMiddleware>();
        }

        #endregion
    }
}
