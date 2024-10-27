using System.Net;

namespace WebAPI.Middlewares
{
    public class GlobalExceptionHandlingMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception e)
            {

                context.Response.StatusCode =
                    (int)HttpStatusCode.InternalServerError;
            }
        }
    }
}

