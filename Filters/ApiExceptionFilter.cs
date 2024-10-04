using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace MinhaAPI.Filters
{
    public class ApiExceptionFilter : IExceptionFilter //implementa a interface Exception filter
    {
        private readonly ILogger<ApiExceptionFilter> _logger;//lida com a itnerface de log
        public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
        {
            _logger = logger;
        }
        public void OnException(ExceptionContext context)//é chamada quando ocorrer uma exceção não tratada durante o processamento de uma request http
        {

            _logger.LogError(context.Exception, "Ocorreu um exceção não tratada: Status Code 500");

            context.Result = new ObjectResult("Ocorreu um problema ao tratar a sua solicitação: Status Code 500")//define o resultado da exceção
            {
                StatusCode = StatusCodes.Status500InternalServerError,
            };
        }
    }

}
