using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NLog;
using PostSharp.Aspects;
using PostSharp.Serialization;

namespace ImageWatcher
{
    [PSerializable]
    public class LogAspect : OnMethodBoundaryAspect
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public override void OnEntry(MethodExecutionArgs args)
        {
            var methodName = $"{args.Method.DeclaringType}.{args.Method.Name}";
            var methodParameters = string.Join("\n", GetParameters(args.Arguments));

            _logger.Info($"Calling: {methodName}");
            _logger.Info($"Args: {methodParameters}");
        }

        public override void OnSuccess(MethodExecutionArgs args)
        {
            _logger.Info($"Done: result was {JsonConvert.SerializeObject(args.ReturnValue)}");
        }

        private IEnumerable<string> GetParameters(IEnumerable<object> parameters)
        {
            return parameters.Select(p =>
            {
                try
                {
                    return JsonConvert.SerializeObject(p);
                }
                catch (Exception e)
                {
                    return $"{p.ToString()} is non serializable";
                }
            });
        }
    }
}
