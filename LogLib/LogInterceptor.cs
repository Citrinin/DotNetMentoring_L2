using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using Castle.DynamicProxy;
using NLog;

namespace LogLib
{
    public class LogInterceptor: IInterceptor
    {
        private readonly JavaScriptSerializer _serializer;
        private readonly ILogger _logger;

        public LogInterceptor(LogFactory loggerFactory)
        {
            _serializer = new JavaScriptSerializer();
            _logger = loggerFactory.GetCurrentClassLogger();
        }

        public void Intercept(IInvocation invocation)
        {

            var methodName = $"{invocation.Method.DeclaringType}.{invocation.Method.Name}";
            var methodParameters = string.Join("\n", GetParameters(invocation.Arguments));

            _logger.Info($"Calling: {methodName}");
            _logger.Info($"Args: {methodParameters}");

            invocation.Proceed();

            _logger.Info($"Done: result was {invocation.ReturnValue}");
        }

        private IEnumerable<string> GetParameters(IEnumerable<object> parameters)
        {
            return parameters.Select(p => _serializer.Serialize(p));
        }
    }
}
