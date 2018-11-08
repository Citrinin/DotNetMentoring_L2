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
        private readonly string _coruptedDirectory;
        private readonly string _prefix;

        private readonly AutoResetEvent _newFileEvent;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private Task _workTask;
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
            _coruptedDirectory = "corupted";
            _prefix = prefix;

            CheckDirectory(_inputDirectory, false);
            CheckDirectory(_outputDirectory, true);
            CheckDirectory(_tempDirectory, true);
            CheckDirectory(_coruptedDirectory, true);

            CopyImages();

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
            _workTask = Task.Run(()=>
            {
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
                                    Console.WriteLine("new file - barcode");
                                    SaveAndCreateNewDocument();
                                    continue;
                                }

                                if (!CheckIfImageContinuingSequence(inFile))
                                {
                                    Console.WriteLine("new file - end of sequence");
                                    SaveAndCreateNewDocument();
                                }

                                Console.WriteLine("img to pdf");
                                File.Move(inFile, outFile);
                                var img = _section.AddImage(outFile);
                                img.Height = _document.DefaultPageSetup.PageHeight;
                                img.Width = _document.DefaultPageSetup.PageWidth;
                                _isDocumentEmpty = false;
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("exception - wrong format");
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
                        Console.WriteLine("new file - timeout");
                        SaveAndCreateNewDocument();
                    }
                }
                while (!token.IsCancellationRequested);
                Console.WriteLine("End of work");
            }, token);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _workTask.Wait();
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

        private static string GetFileVersion(string fileName, string extension)
        {
            while (true)
            {
                var existingFileName =$"{fileName}.{extension}";
                if (File.Exists(existingFileName))
                {
                    var match = Regex.Match(existingFileName, $@"(?<=\()\d+(?=\).{extension})");

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
            _renderer.RenderDocument();
            _renderer.Save($"{GetFileVersion($"{_outputDirectory}/result", "pdf")}.pdf");
            ClearDirectory(_tempDirectory);
        }

        private void MoveFilesToCoruptedFolder()
        {
            foreach (var file in Directory.EnumerateFiles(_tempDirectory))
            {
                var outFile = Path.Combine(_coruptedDirectory, Path.GetFileName(file));
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

        private void CopyImages()
        {
            var imageFolder = @"../../images";
            foreach (var image in Directory.EnumerateFiles(imageFolder))
            {
                if (TryOpenFile(image, 3))
                {
                    var imageCopy = Path.Combine(_inputDirectory, Path.GetFileName(image));
                    File.Copy(image, imageCopy);
                }
            }
        }
    }
}
