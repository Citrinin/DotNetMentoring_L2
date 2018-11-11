using NLog;
using NLog.Config;
using NLog.Targets;
using System.Diagnostics;
using System.IO;
using Topshelf;

namespace ImageWatcher
{
    public class Program
    {
        static void Main(string[] args)
        {
            var currentDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            var inputDir = Path.Combine(currentDir, "in");
            var outputDir = Path.Combine(currentDir, "out");
            var prefix = "img";

            var conf = new LoggingConfiguration();
            var fileTarget = new FileTarget()
            {
                Name = "Default",
                FileName = Path.Combine(currentDir, "log.txt"),
                Layout = "${date} -Thread ${threadid}- ${message}  ${onexception:inner=${exception:format=toString}}"
            };
            conf.AddTarget(fileTarget);
            conf.AddRuleForAllLevels(fileTarget);

            var logFactory = new LogFactory(conf);

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
