using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using System.Net;
using Newtonsoft.Json;

namespace LoggerApi
{
    public sealed class CustomExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public CustomExceptionHandlerMiddleware(
            RequestDelegate next,
            ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<CustomExceptionHandlerMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                try
                {
                    _logger.LogError(ex, "Http middleware handler catch the error");
                    //EmailLogger.SendError(ex);

                    // if you don't want to rethrow the original exception
                    // then call return:
                    // return;
                }
                catch (Exception ex2)
                {
                    _logger.LogError(
                        0, ex2,
                        "An exception was thrown attempting " +
                        "to execute the error handler.");
                }

                // Otherwise this handler will
                // re -throw the original exception
                throw;
                //await HandleExceptionAsync(context, ex);
            }
        }

        //private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        //{
        //    var code = HttpStatusCode.InternalServerError; // 500 if unexpected

        //    if (exception is MyNotFoundException) code = HttpStatusCode.NotFound;
        //    else if (exception is MyUnauthorizedException) code = HttpStatusCode.Unauthorized;
        //    else if (exception is MyException) code = HttpStatusCode.BadRequest;

        //    var result = JsonConvert.SerializeObject(new { error = exception.Message });

        //    context.Response.ContentType = "application/json";
        //    context.Response.StatusCode = (int)code;

        //    return context.Response.WriteAsync(result);
        //}
    }
}
