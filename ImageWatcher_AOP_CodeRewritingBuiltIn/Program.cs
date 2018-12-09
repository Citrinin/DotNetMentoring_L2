using NLog;
using NLog.Config;
using NLog.Targets;
using System.Diagnostics;
using System.IO;
using Topshelf;
using PostSharp.Patterns.Diagnostics;
using PostSharp.Patterns.Diagnostics.Backends.NLog;

namespace ImageWatcher
{
    public class Program
    {
        static void Main(string[] args)
        {
            var currentDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

            var conf = new LoggingConfiguration();
            var fileTarget = new FileTarget()
            {
                Name = "Default",
                FileName = Path.Combine(currentDir, "log.txt"),
                Layout = "${date} -Thread ${threadid}- ${message}  ${onexception:inner=${exception:format=toString}}"
            };
            conf.AddTarget(fileTarget);
            conf.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Debug, fileTarget));

            var consoleTarget = new ConsoleTarget("console");
            conf.AddTarget(consoleTarget);
            conf.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Debug, consoleTarget));

            LogManager.EnableLogging();

            var logFactory = new LogFactory(conf);

            LoggingServices.DefaultBackend = new NLogLoggingBackend(logFactory);

            HostFactory.Run(
                hostConf => 
                {
                    hostConf.Service<ImagesWatchingService>(
                        s =>
                        {
                            s.ConstructUsing(() => new ImagesWatchingService(logFactory));
                            s.WhenStarted(serv => serv.Start());
                            s.WhenStopped(serv => serv.Stop());
                        });
                    hostConf.SetServiceName("ImagesWatchingService");
                    hostConf.SetDisplayName("Images watching service");
                    hostConf.StartAutomaticallyDelayed();
                    hostConf.UseNLog(logFactory);
                });
        }
    }
}
