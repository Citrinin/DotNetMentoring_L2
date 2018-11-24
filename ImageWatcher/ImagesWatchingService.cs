using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using ImageWatcher.Configuration;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using NLog;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
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
        private static Logger _log;


        private static string connString =
            "Endpoint=sb://messagequeuestask.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=eZrzUCBPE+R+VlHed0FMkCqcUV6SzfDoAhXD9e2EmPk=";
        private static string queueName = "imageswatcherqueue";

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

        private class WatchingThreadParameter
        {
            private Document _document;
            private Section _section;
            private PdfDocumentRenderer _renderer;
            private bool _isDocumentEmpty;
            private int _imageSequenceNumber;

            private readonly string _corruptedFolder;
            private readonly string _outputFolder;
            private readonly string _prefix;
            private readonly AutoResetEvent _newFileEvent;
            private readonly IQueueClient client;

            public WatchingThreadParameter(ImagesWatcherElement configurationImagesWatcher)
            {
                TempFolder = Path.Combine(Path.GetTempPath(), "ImageWatcher", Guid.NewGuid().ToString());
                InputFolder = configurationImagesWatcher.InFolder;
                _outputFolder = configurationImagesWatcher.OutFolder;
                _corruptedFolder = configurationImagesWatcher.CorruptedFolder;
                _prefix = configurationImagesWatcher.Prefix;

                Utils.CheckDirectory(TempFolder, true);
                Utils.CheckDirectory(InputFolder, false);
                Utils.CheckDirectory(_outputFolder, false);
                Utils.CheckDirectory(_corruptedFolder, false);

                _newFileEvent = new AutoResetEvent(false);

                FileSystemWatcher = new FileSystemWatcher(InputFolder);
                FileSystemWatcher.Created += (sender, args) => _newFileEvent.Set();

                client = new QueueClient(connString, queueName);
            }

            public FileSystemWatcher FileSystemWatcher { get; }

            public string InputFolder { get; }

            public string TempFolder { get; }

            public async Task SaveDocument()
            {
                if (!_isDocumentEmpty)
                {
                    _renderer.RenderDocument();
                    //_renderer.Save($"{_outputFolder}/result-{Utils.GetTimeStamp()}.pdf");

                    var messages = await CreateMessage(_renderer);
                    foreach (var message in messages)
                    {
                        await client.SendAsync(message);
                    }
                }

                Utils.ClearDirectory(TempFolder);
            }

            public void MoveFilesToCorruptedFolder()
            {
                var time = Utils.GetTimeStamp();
                Directory.CreateDirectory(Path.Combine(_corruptedFolder, time));
                foreach (var file in Directory.EnumerateFiles(TempFolder))
                {
                    var outFile = Path.Combine(_corruptedFolder, time, Path.GetFileName(file));
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

            public async Task SaveAndCreateNewDocument()
            {
                await SaveDocument();
                CreateNewDocument();
            }

            public bool CheckIfImageContinuingSequence(string inFile)
            {
                var currentImageSequenceNumber = Utils.GetImageSequenceNumber(inFile, _prefix);
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
                return !_newFileEvent.WaitOne(timeout) && !_isDocumentEmpty;
            }

            public bool IsFileMatchPrefix(string fileName)
            {
                return Regex.IsMatch(fileName, $@"(?<={_prefix}_)(\d+).(?=\.(jpeg|jpg|png)$)");
            }

            private Task<IEnumerable<Message>> CreateMessage(PdfDocumentRenderer renderer)
            {
                if (renderer == null)
                {
                    return null;
                }

                using (var ms = new MemoryStream())
                {
                    renderer.Save(ms, false);
                    using (var fs = new FileStream($"{_outputFolder}/result-{Utils.GetTimeStamp()}.pdf", FileMode.Create))
                    {
                        ms.CopyTo(fs);
                        ms.Seek(0, SeekOrigin.Begin);
                    }
                    var pdfCreator = new PdfMessagesCreator();
                    return pdfCreator.CreatePdfPartialMessageAsync(ms);
                }
            }
        }
    }
}