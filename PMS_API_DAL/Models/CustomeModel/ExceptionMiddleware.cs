using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PMS_API_DAL.Models.CustomeModel.Exceptions;
using System.Text.Json;

namespace PMS_API_DAL.Models.CustomeModel
{
    public class ExceptionMiddleware : IMiddleware
    {
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(ILogger<ExceptionMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong: {ex.Message}");
                await HandleException(context, ex);
            }
        }

        private static Task HandleException(HttpContext context, Exception ex)
        {
            int statusCode = StatusCodes.Status500InternalServerError;
            switch (ex)
            {
                case NotFoundException _:
                    statusCode = StatusCodes.Status404NotFound;
                    break;
                case BadRequestException _:
                    statusCode = StatusCodes.Status400BadRequest;
                    break;
                case NullReferenceException _:
                    statusCode = StatusCodes.Status404NotFound;
                    break;
                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    break;
            }

            ErrorResponse errorResponse = new ErrorResponse()
            {
                StatusCode = statusCode,
                Message = ex.Message
            };
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;
            return context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        }
    }

    public static class ExceptionMiddlewareExtension
    {
        public static void ConfigureExceptionMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionMiddleware>();
        }
    }

    public class ErrorResponse
    {
        public int? StatusCode { get; set; }
        public string? Message { get; set; }
    }
}
