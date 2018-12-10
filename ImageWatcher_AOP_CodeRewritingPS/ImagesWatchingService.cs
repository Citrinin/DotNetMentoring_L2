using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ImageWatcher.Configuration;
using NLog;
using System.Configuration;
using SBCommon;

namespace ImageWatcher
{
    public class ImagesWatchingService
    {
        private readonly ManualResetEvent _stopWorkEvent;
        private readonly List<Thread> _workThreads;
        private readonly List<WatchingThreadParameter> _workThreadParameters;
        private readonly int _timeout;
        private readonly ImageWatcherConfigurationSection _configuration;
        private static Logger _log = LogManager.GetCurrentClassLogger();

        public ImagesWatchingService()
        {

            _configuration =
                (ImageWatcherConfigurationSection)ConfigurationManager.GetSection("imageWatcherConfiguration");
            _workThreadParameters = new List<WatchingThreadParameter>();
            _workThreads = new List<Thread>();

            WatchingThreadParameter.Logger = _log;
            foreach (var imagesWatcher in _configuration.ImagesWatchers)
            {
                _workThreadParameters.Add(new WatchingThreadParameter((ImagesWatcherElement)imagesWatcher));
            }
            _stopWorkEvent = new ManualResetEvent(false);
            _timeout = 10000;
        }

        public void Start()
        {
            _workThreadParameters.ForEach(threadParam =>
            {
                var thread = new Thread(WorkProcedure);
                _workThreads.Add(thread);
                thread.Start(threadParam);
            });
        }

        public void Stop()
        {
            _stopWorkEvent.Set();
            _workThreads.ForEach(thread => thread.Join());
        }

        private async void WorkProcedure(object obj)
        {
            var threadParameter = (WatchingThreadParameter)obj;
            threadParameter.FileSystemWatcher.EnableRaisingEvents = true;
            threadParameter.CreateNewDocument();
            do
            {
                foreach (var inFile in Directory.EnumerateFiles(threadParameter.InputFolder))
                {
                    if (!threadParameter.IsFileMatchPrefix(inFile))
                    {
                        continue;
                    }

                    if (Utils.TryOpenFile(inFile, 3))
                    {
                        var outFile = Path.Combine(threadParameter.TempFolder, Path.GetFileName(inFile));

                        try
                        {
                            if (string.Equals(Utils.GetImageBarcodeValue(inFile), "next document", StringComparison.OrdinalIgnoreCase))
                            {
                                File.Delete(inFile);
                                _log.Info("new file - barcode");
                                await threadParameter.SaveAndCreateNewDocument();
                                continue;
                            }

                            if (!threadParameter.CheckIfImageContinuingSequence(inFile))
                            {
                                _log.Info("new file - end of sequence");
                                await threadParameter.SaveAndCreateNewDocument();
                            }

                            _log.Info("img to pdf");
                            File.Move(inFile, outFile);
                            threadParameter.AddImageToDocument(outFile);
                        }
                        catch (Exception e)
                        {
                            _log.Info("exception - wrong format");
                            _log.Error(e.ToString());
                            if (!File.Exists(outFile))
                            {
                                File.Move(inFile, outFile);
                            }
                            threadParameter.MoveFilesToCorruptedFolder();
                            threadParameter.CreateNewDocument();
                        }
                    }
                }

                if (threadParameter.IsTimeoutOfNewDocumentEventExpired(_timeout))
                {
                    _log.Info("new file - timeout");
                    await threadParameter.SaveAndCreateNewDocument();
                }
            }
            while (!_stopWorkEvent.WaitOne(1000));
            await threadParameter.SaveDocument();
            threadParameter.FileSystemWatcher.EnableRaisingEvents = false;
            _log.Info("End of work");
        }
    }
}