using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace My.Service.Logging
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<RequestLoggingMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            string userId;
            try
            {
                userId=context.Request.Cookies["user"];
                context.Items["user"]=this;
            }
            catch{

            }
            try
            {
                 
                await _next(context);
            }
            finally
            {
                _logger.LogInformation(
                    "Request {method} {url} => {statusCode}",
                    context.Request?.Method,
                    context.Request?.Path.Value,
                    context.Response?.StatusCode);
            }
        }
    }
}