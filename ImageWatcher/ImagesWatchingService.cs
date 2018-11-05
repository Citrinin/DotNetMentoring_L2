using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using ZXing;

namespace ImageWatcher
{
    public class ImagesWatchingService
    {
        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly string _inputDirectory;
        private readonly string _outputDirectory;
        private readonly string _tempDirectory;
        private readonly string _prefix;

        private readonly AutoResetEvent _newFileEvent;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private Document _document;
        private Section _section;
        private PdfDocumentRenderer _renderer;
        private bool _isDocumentEmpty;
        private int _imageSequenceNumber;
        private readonly BarcodeReader _barcodeReader;

        private readonly int _timeout;

        public ImagesWatchingService(string inputDirectory, string outputDirectory, string prefix)
        {
            _inputDirectory = inputDirectory;
            _outputDirectory = outputDirectory;
            _tempDirectory = "temp";
            _prefix = prefix;

            if (!Directory.Exists(_inputDirectory))
            {
                Directory.CreateDirectory(_inputDirectory);
            }

            if (!Directory.Exists(_outputDirectory))
            {
                Directory.CreateDirectory(_outputDirectory);
            }

            if (!Directory.Exists(_tempDirectory))
            {
                Directory.CreateDirectory(_tempDirectory);
            }
            else
            {
                foreach (var file in Directory.EnumerateFiles(_tempDirectory))
                {
                    if (TryOpenFile(file, 3))
                    {
                        File.Delete(file);
                    }
                }
            }

            _fileSystemWatcher = new FileSystemWatcher(_inputDirectory);
            _fileSystemWatcher.Created += FileSystemWatcherOnCreated;
            _fileSystemWatcher.EnableRaisingEvents = true;

            _cancellationTokenSource = new CancellationTokenSource();
            _newFileEvent = new AutoResetEvent(false);
            _timeout = 10000;

            _barcodeReader = new BarcodeReader() { AutoRotate = true };

            CreateNewDocument();
        }

        public void Start()
        {
            var token = _cancellationTokenSource.Token;
            Task.Run(()=>
            {
                do
                {
                    foreach (var inFile in Directory.EnumerateFiles(_inputDirectory))
                    {
                        var outFile = Path.Combine(_tempDirectory, Path.GetFileName(inFile));

                        if (TryOpenFile(inFile, 3))
                        {
                            if (CheckIfImageContainsStopCode(inFile))
                            {
                                File.Move(inFile, outFile);
                                Console.WriteLine("new file - barcode");
                                SaveAndCreateNewDocument();
                                continue;
                            }

                            if (!CheckIfImageContinuingSequence(inFile))
                            {
                                Console.WriteLine("new file - end of sequence");
                                SaveAndCreateNewDocument();
                            }
                            File.Move(inFile, outFile);
                            try
                            {
                                Console.WriteLine("img to pdf");
                                var img = _section.AddImage(outFile);
                                img.Height = _document.DefaultPageSetup.PageHeight;
                                img.Width = _document.DefaultPageSetup.PageWidth;
                                _isDocumentEmpty = false;
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("exception - wrong format");
                            }
                        }
                    }

                    if (!_newFileEvent.WaitOne(_timeout) && !_isDocumentEmpty)
                    {
                        SaveAndCreateNewDocument();
                        Console.WriteLine("new file - timeout");
                    }
                }
                while (!token.IsCancellationRequested);
            }, token);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _fileSystemWatcher.EnableRaisingEvents = false;
            SaveDocument();
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

        private static string GetPdfFileVersion(string fileName)
        {
            while (true)
            {
                var existingFileName = fileName + ".pdf";
                if (File.Exists(existingFileName))
                {
                    var match = Regex.Match(existingFileName, @"(?<=\()\d+(?=\).pdf)");

                    if (match.Value == "")
                    {
                        fileName = fileName + "(1)";
                        continue;
                    }

                    var version = int.Parse(match.Value);
                    fileName = Regex.Replace(fileName, @"(?<=\()\d+(?=\)$)", $"{++version}");
                    continue;
                }

                return fileName;
            }
        }

        private bool CheckIfImageContinuingSequence(string inFile)
        {
            var fileNumberMatch = Regex.Match(inFile, $@"(?<={_prefix}_)(\d+).(?=\.(jpeg|jpg|png)$)");
            var currentImageSequenceNumber = int.Parse(fileNumberMatch.Value);

            if (_imageSequenceNumber + 1 == currentImageSequenceNumber || _isDocumentEmpty)
            {
                _imageSequenceNumber = currentImageSequenceNumber;
                return true;
            }

            return false;
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
            _renderer.RenderDocument();
            _renderer.Save($"{GetPdfFileVersion($"{_outputDirectory}/result")}.pdf");

            foreach (var file in Directory.EnumerateFiles(_tempDirectory))
            {
                if (TryOpenFile(file, 3))
                {
                    File.Delete(file);
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
    }
}
