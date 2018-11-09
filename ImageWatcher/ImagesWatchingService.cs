using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using NLog;
using ZXing;


namespace ImageWatcher
{
    public class ImagesWatchingService
    {
        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly string _inputDirectory;
        private readonly string _outputDirectory;
        private readonly string _tempDirectory;
        private readonly string _corruptedDirectory;
        private readonly string _prefix;

        private readonly ManualResetEvent _stopWorkEvent;
        private readonly AutoResetEvent _newFileEvent;

        private readonly Thread _workThread;
        private Document _document;
        private Section _section;
        private PdfDocumentRenderer _renderer;
        private bool _isDocumentEmpty;
        private int _imageSequenceNumber;
        private readonly BarcodeReader _barcodeReader;

        private readonly int _timeout;

        private static Logger _log;

        public ImagesWatchingService(string inputDirectory, string outputDirectory, string prefix, LogFactory logger)
        {
            _inputDirectory = inputDirectory;
            _outputDirectory = outputDirectory;
            _tempDirectory = "temp";
            _corruptedDirectory = "corrupted";
            _prefix = prefix;
            _log = logger.GetCurrentClassLogger();

            CheckDirectory(_inputDirectory, false);
            CheckDirectory(_outputDirectory, false);
            CheckDirectory(_tempDirectory, true);
            CheckDirectory(_corruptedDirectory, false);

            _fileSystemWatcher = new FileSystemWatcher(_inputDirectory);
            _fileSystemWatcher.Created += FileSystemWatcherOnCreated;


            _workThread = new Thread(WorkProcedure);
            _stopWorkEvent = new ManualResetEvent(false);
            _newFileEvent = new AutoResetEvent(false);
            _timeout = 10000;

            _barcodeReader = new BarcodeReader() { AutoRotate = true };
        }

        public void Start()
        {
            _fileSystemWatcher.EnableRaisingEvents = true;
            _workThread.Start();
        }

        public void Stop()
        {
            _stopWorkEvent.Set();
            _fileSystemWatcher.EnableRaisingEvents = false;
            _workThread.Join();
            SaveDocument();
        }

        private void WorkProcedure(object obj)
        {
            CreateNewDocument();
            do
            {
                foreach (var inFile in Directory.EnumerateFiles(_inputDirectory))
                {
                    if (!Regex.IsMatch(inFile, $@"(?<={_prefix}_)(\d+).(?=\.(jpeg|jpg|png)$)"))
                    {
                        continue;
                    }

                    var outFile = Path.Combine(_tempDirectory, Path.GetFileName(inFile));

                    if (TryOpenFile(inFile, 3))
                    {
                        try
                        {
                            if (CheckIfImageContainsStopCode(inFile))
                            {
                                File.Delete(inFile);
                                _log.Info("new file - barcode");
                                SaveAndCreateNewDocument();
                                continue;
                            }

                            if (!CheckIfImageContinuingSequence(inFile))
                            {
                                _log.Info("new file - end of sequence");
                                SaveAndCreateNewDocument();
                            }

                            _log.Info("img to pdf");
                            File.Move(inFile, outFile);
                            var img = _section.AddImage(outFile);
                            img.Height = _document.DefaultPageSetup.PageHeight;
                            img.Width = _document.DefaultPageSetup.PageWidth;
                            _isDocumentEmpty = false;
                        }
                        catch (Exception e)
                        {
                            _log.Info("exception - wrong format");
                            _log.Error(e.ToString());
                            if (!File.Exists(outFile))
                            {
                                File.Move(inFile, outFile);
                            }
                            MoveFilesToCoruptedFolder();
                            CreateNewDocument();
                        }
                    }
                }

                if (!_newFileEvent.WaitOne(_timeout) && !_isDocumentEmpty)
                {
                    _log.Info("new file - timeout");
                    SaveAndCreateNewDocument();
                }
            }
            while (!_stopWorkEvent.WaitOne(1000));
            _log.Info("End of work");
        }

        private void FileSystemWatcherOnCreated(object sender, FileSystemEventArgs e)
        {
            _newFileEvent.Set();
        }

        private static bool TryOpenFile(string fileName, int tryCount)
        {
            for (var i = 0; i < tryCount; i++)
            {
                try
                {
                    var file = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.None);
                    file.Close();
                    return true;
                }
                catch (IOException)
                {
                    Thread.Sleep(5000);
                }
            }
            return false;
        }

        private bool CheckIfImageContinuingSequence(string inFile)
        {
            var fileNumberMatch = Regex.Match(inFile, $@"(?<={_prefix}_)(\d+).(?=\.(jpeg|jpg|png)$)");
            var currentImageSequenceNumber = int.Parse(fileNumberMatch.Value);
            var result = _imageSequenceNumber + 1 == currentImageSequenceNumber || _isDocumentEmpty;

            _imageSequenceNumber = currentImageSequenceNumber;
            return result;
        }

        private bool CheckIfImageContainsStopCode(string imgFile)
        {
            using (var bmp = (Bitmap)Image.FromFile(imgFile))
            {
                var result = _barcodeReader.Decode(bmp);
                return string.Equals(result?.Text, "next document", StringComparison.OrdinalIgnoreCase);
            }
        }

        private void SaveDocument()
        {
            if (!_isDocumentEmpty)
            {
                _renderer.RenderDocument();
                _renderer.Save($"{_outputDirectory}/result-{GetTimeStamp()}.pdf");
            }

            ClearDirectory(_tempDirectory);
        }

        private void MoveFilesToCoruptedFolder()
        {
            var time = GetTimeStamp();
            Directory.CreateDirectory(Path.Combine(_corruptedDirectory, time));
            foreach (var file in Directory.EnumerateFiles(_tempDirectory))
            {
                var outFile = Path.Combine(_corruptedDirectory, time, Path.GetFileName(file));
                if (TryOpenFile(file, 3))
                {
                    File.Move(file, outFile);
                }
            }
        }

        private void CreateNewDocument()
        {
            _document = new Document();
            _section = _document.AddSection();
            _isDocumentEmpty = true;

            _renderer = new PdfDocumentRenderer
            {
                Document = _document
            };
        }

        private void SaveAndCreateNewDocument()
        {
            SaveDocument();
            CreateNewDocument();
        }

        private void ClearDirectory(string directoryName)
        {
            foreach (var file in Directory.EnumerateFiles(directoryName))
            {
                if (TryOpenFile(file, 3))
                {
                    File.Delete(file);
                }
            }
        }

        private void CheckDirectory(string directoryName, bool clearDirectory)
        {
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            else if (clearDirectory)
            {
                ClearDirectory(directoryName);
            }
        }

        private string GetTimeStamp()
        {
            return new string(DateTime.Now.ToString().Where(char.IsDigit).ToArray());
        }
    }
}
