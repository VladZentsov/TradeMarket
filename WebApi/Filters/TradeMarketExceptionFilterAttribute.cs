using Business.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading.Tasks;

namespace WebApi.Filters
{
    public class TradeMarketExceptionFilterAttribute : Attribute, IAsyncExceptionFilter
    {
        public Task OnExceptionAsync(ExceptionContext context)
        {
            var action = context.ActionDescriptor.DisplayName;
            var callStack = context.Exception.StackTrace;
            var exceptionMessage = context.Exception.Message;
            int StatusCode = 400;

            if (context.Exception is MarketException)
            {
                StatusCode = 400;
            }
            else if (context.Exception is MarketNotFoundException)
            {
                StatusCode = 404;
            }

            context.Result = new ContentResult
            {
                Content = $"Calling {action} failed, because: {exceptionMessage}. Callstack: {callStack}.",
                StatusCode = StatusCode,
            };

            context.ExceptionHandled = true;
            return Task.CompletedTask;

        }
    }
}
