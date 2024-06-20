using Microsoft.AspNetCore.Http;
using System.Web.Http.ExceptionHandling;

namespace PMS_API_DAL.Models.CustomeModel
{
    public class ExceptionHandler : IExceptionHandler
    {
        public Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ValueTask<bool> TryHandleAsync(HttpContext httpContext,Exception exception , CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
