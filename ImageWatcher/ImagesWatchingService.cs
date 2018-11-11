using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using ImageWatcher.Configuration;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using NLog;
using ConfigurationManager = System.Configuration.ConfigurationManager;


namespace ImageWatcher
{
    public class ImagesWatchingService
    {

        private readonly ManualResetEvent _stopWorkEvent;
        private readonly List<Thread> _workThreads;
        private readonly List<WatchingThreadParameter> _workThreadParameters;
        private readonly int _timeout;
        private readonly ImageWatcherConfigurationSection _configuration;
        private static Logger _log;

        public ImagesWatchingService(LogFactory logger)
        {
            _log = logger.GetCurrentClassLogger();
            _configuration =
                (ImageWatcherConfigurationSection)ConfigurationManager.GetSection("imageWatcherConfiguration");
            _workThreadParameters = new List<WatchingThreadParameter>();
            _workThreads = new List<Thread>();

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

        private void WorkProcedure(object obj)
        {
            var threadParameter = (WatchingThreadParameter)obj;
            threadParameter.FileSystemWatcher.EnableRaisingEvents = true;
            threadParameter.CreateNewDocument();
            do
            {
                foreach (var inFile in Directory.EnumerateFiles(threadParameter.InputFolder))
                {
                    if (!Regex.IsMatch(inFile, $@"(?<={threadParameter.Prefix}_)(\d+).(?=\.(jpeg|jpg|png)$)"))
                    {
                        continue;
                    }

                    var outFile = Path.Combine(threadParameter.TempFolder, Path.GetFileName(inFile));

                    if (Utils.TryOpenFile(inFile, 3))
                    {
                        try
                        {
                            if (string.Equals(Utils.GetImageBarcodeValue(inFile), "next document", StringComparison.OrdinalIgnoreCase))
                            {
                                File.Delete(inFile);
                                _log.Info("new file - barcode");
                                threadParameter.SaveAndCreateNewDocument();
                                continue;
                            }

                            if (!threadParameter.CheckIfImageContinuingSequence(inFile))
                            {
                                _log.Info("new file - end of sequence");
                                threadParameter.SaveAndCreateNewDocument();
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
                    threadParameter.SaveAndCreateNewDocument();
                }
            }
            while (!_stopWorkEvent.WaitOne(1000));
            threadParameter.SaveDocument();
            threadParameter.FileSystemWatcher.EnableRaisingEvents = false;
            _log.Info("End of work");
        }

        class WatchingThreadParameter
        {
            private Document _document;
            private Section _section;
            private PdfDocumentRenderer _renderer;
            private bool _isDocumentEmpty;
            private int _imageSequenceNumber;

            public WatchingThreadParameter(ImagesWatcherElement configurationImagesWatcher)
            {
                InputFolder = configurationImagesWatcher.InFolder;
                OutputFolder = configurationImagesWatcher.OutFolder;
                CorruptedFolder = configurationImagesWatcher.CorruptedFolder;
                TempFolder = Path.Combine(Path.GetTempPath(), "ImageWatcher", Guid.NewGuid().ToString());
                Prefix = configurationImagesWatcher.Prefix;

                Utils.CheckDirectory(InputFolder, false);
                Utils.CheckDirectory(OutputFolder, false);
                Utils.CheckDirectory(TempFolder, true);
                Utils.CheckDirectory(CorruptedFolder, false);

                AutoResetEvent = new AutoResetEvent(false);

                FileSystemWatcher = new FileSystemWatcher(InputFolder);
                FileSystemWatcher.Created += (sender, args) => AutoResetEvent.Set();
            }

            public FileSystemWatcher FileSystemWatcher { get; set; }

            public AutoResetEvent AutoResetEvent { get; set; }

            public string InputFolder { get; set; }

            public string OutputFolder { get; set; }

            public string TempFolder { get; set; }

            public string CorruptedFolder { get; set; }

            public string Prefix { get; set; }

            public void SaveDocument()
            {
                if (!_isDocumentEmpty)
                {
                    _renderer.RenderDocument();
                    _renderer.Save($"{OutputFolder}/result-{Utils.GetTimeStamp()}.pdf");
                }

                Utils.ClearDirectory(TempFolder);
            }

            public void MoveFilesToCorruptedFolder()
            {
                var time = Utils.GetTimeStamp();
                Directory.CreateDirectory(Path.Combine(CorruptedFolder, time));
                foreach (var file in Directory.EnumerateFiles(TempFolder))
                {
                    var outFile = Path.Combine(CorruptedFolder, time, Path.GetFileName(file));
                    if (Utils.TryOpenFile(file, 3))
                    {
                        File.Move(file, outFile);
                    }
                }
            }

            public void CreateNewDocument()
            {
                _document = new Document();
                _section = _document.AddSection();
                _isDocumentEmpty = true;

                _renderer = new PdfDocumentRenderer
                {
                    Document = _document
                };
            }

            public void SaveAndCreateNewDocument()
            {
                SaveDocument();
                CreateNewDocument();
            }

            public bool CheckIfImageContinuingSequence(string inFile)
            {
                var currentImageSequenceNumber = Utils.GetImageSequenceNumber(inFile, Prefix);
                var result = _imageSequenceNumber + 1 == currentImageSequenceNumber || _isDocumentEmpty;

                _imageSequenceNumber = currentImageSequenceNumber;
                return result;
            }

            public void AddImageToDocument(string imgPath)
            {
                var img = _section.AddImage(imgPath);
                img.Height = _document.DefaultPageSetup.PageHeight;
                img.Width = _document.DefaultPageSetup.PageWidth;
                _isDocumentEmpty = false;
            }

            public bool IsTimeoutOfNewDocumentEventExpired(int timeout)
            {
                return !AutoResetEvent.WaitOne(timeout) && !_isDocumentEmpty;
            }
        }
    }
}