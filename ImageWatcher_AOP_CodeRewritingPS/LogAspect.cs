using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PostSharp.Aspects;
using PostSharp.Serialization;

namespace ImageWatcher
{
    [PSerializable]
    public class LogAspect : OnMethodBoundaryAspect
    {
        public override void OnEntry(MethodExecutionArgs args)
        {
            var methodName = $"{args.Method.DeclaringType}.{args.Method.Name}";
            var methodParameters = string.Join("\n", GetParameters(args.Arguments));

            Console.WriteLine($"Calling: {methodName}");
            Console.WriteLine($"Args: {methodParameters}");
            args.FlowBehavior = FlowBehavior.Default;
        }

        public override void OnSuccess(MethodExecutionArgs args)
        {
            Console.WriteLine($"Done: result was {JsonConvert.SerializeObject(args.ReturnValue)}");
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
