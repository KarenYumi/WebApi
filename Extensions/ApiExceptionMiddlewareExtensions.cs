using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using MinhaAPI.Models;

namespace MinhaAPI.Extensions
{//classe esática que define um método de extensão 
    public static class ApiExceptionMiddlewareExtensions
    {
        public static void ConfigureExceptionHandler (this IApplicationBuilder app)
        {//middleware (appRun) para fazer o tratamento de exceção global
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        await context.Response.WriteAsync(new ErrorDetails()
                        {//vai exibir essas info sobre o erro que ocorrer
                            StatusCode = context.Response.StatusCode,
                            Message = contextFeature.Error.Message,
                            Trace = contextFeature.Error.StackTrace
                        }.ToString());
                    }

                });
            });
        }
    }
}
