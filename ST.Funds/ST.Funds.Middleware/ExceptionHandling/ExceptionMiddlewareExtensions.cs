using Microsoft.AspNetCore.Builder;
using ST.Funds.Api.Middleware.ExceptionHandling;
using System;
using System.Collections.Generic;
using System.Text;

namespace ST.Funds.Middleware.ExceptionHandling
{
    public static class ExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
