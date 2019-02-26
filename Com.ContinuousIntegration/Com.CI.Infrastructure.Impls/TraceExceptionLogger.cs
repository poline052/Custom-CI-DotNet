using log4net;
using System.ComponentModel;
using System.Reflection;
using System.Web.Http.ExceptionHandling;

namespace Com.CI.Infrastructure.Impls
{
    public class TraceExceptionLogger : ExceptionLogger
    {
        private static readonly ILog Log4Net = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [Localizable(false)]
        public override void Log(ExceptionLoggerContext context)
        {
            Log4Net.Error($"Unhandled exception thrown in {context.Request.Method} for request {context.Request.RequestUri}: { context.Exception}");
        }
    }
}
