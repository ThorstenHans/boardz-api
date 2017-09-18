using BoardZ.API.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Net;
using Microsoft.ApplicationInsights;

namespace BoardZ.API.Filters
{
    public class ExceptionFilter : IExceptionFilter
    {
        protected ILogger Logger { get; }
        protected IHostingEnvironment HostingEnvironment { get; }
        public ExceptionFilter(ILogger<ExceptionFilter> logger, IHostingEnvironment hostingEnvironment)
        {
            Logger = logger;
            HostingEnvironment = hostingEnvironment;
        }
        public void OnException(ExceptionContext context)
        {
            TelemetryClient telemetry = new TelemetryClient();
            Logger.LogError(666, context.Exception, context.Exception.Message);
            telemetry.TrackException(context.Exception);
            string message;
            string stackTrace;
            if (HostingEnvironment.IsProduction())
            {
                message = "Unhandled error occurred.";
                stackTrace = null;
            }
            else
            {
                message = context.Exception.GetBaseException().Message;
                stackTrace = context.Exception.StackTrace;
            }
            if (context.Exception is HttpStatusCodeException)
            {
                context.HttpContext.Response.StatusCode = ((HttpStatusCodeException) context.Exception).StatusCodeAsInt;
            }
            else
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            context.Result = new JsonResult(new
            {
                context.HttpContext.Response.StatusCode,
                Message = message,
                Stack = stackTrace
            });
        }
    }
}
