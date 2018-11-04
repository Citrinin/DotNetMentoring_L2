using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            HostFactory.Run(
                hostConf => hostConf.Service<ImagesWatchingService>(
                    s => {
                        s.ConstructUsing(() => new ImagesWatchingService(inputDir, outputDir, prefix));
                        s.WhenStarted(serv => serv.Start());
                        s.WhenStopped(serv => serv.Stop());
                    }
                ));
        }
    }
}
