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
            HostFactory.Run(
                hostConf => 
                {
                    hostConf.Service<ImagesWatchingService>(
                        s =>
                        {
                            s.ConstructUsing(() => new ImagesWatchingService());
                            s.WhenStarted(serv => serv.Start());
                            s.WhenStopped(serv => serv.Stop());
                        });
                    hostConf.SetServiceName("ImagesWatchingService");
                    hostConf.SetDisplayName("Images watching service");
                    hostConf.StartAutomaticallyDelayed();
                    hostConf.UseNLog();
                });
        }
    }
}
