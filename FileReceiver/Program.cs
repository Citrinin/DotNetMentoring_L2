using Topshelf;

namespace FileReceiver
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(
                hostConf =>
                {
                    hostConf.Service<FileReceiverService>(
                        s =>
                        {
                            s.ConstructUsing(() => new FileReceiverService());
                            s.WhenStarted(serv => serv.Start());
                            s.WhenStopped(serv => serv.Stop());
                        });
                    hostConf.SetServiceName("FilesReceiverService");
                    hostConf.SetDisplayName("Files receiver service");
                    hostConf.StartAutomaticallyDelayed();
                });
        }
    }
}
