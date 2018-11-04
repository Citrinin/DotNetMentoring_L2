using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.Rendering;
using Topshelf;

namespace ImageWatcher
{
    public class ImagesWatchingService
    {
        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly string _inputDirectory;
        private readonly string _outputDirectory;
        private readonly string _tempDirectory;

        private readonly AutoResetEvent _newFileEvent;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private Document _document;
        private Section _section;
        private PdfDocumentRenderer _renderer;

        private readonly int _timeout;

        public ImagesWatchingService(string inputDirectory, string outputDirectory, string prefix)
        {
            _inputDirectory = inputDirectory;
            _outputDirectory = outputDirectory;
            _tempDirectory = "temp";

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

            _fileSystemWatcher = new FileSystemWatcher(_inputDirectory);
            _fileSystemWatcher.Created += FileSystemWatcherOnCreated;
            _fileSystemWatcher.EnableRaisingEvents = true;

            _cancellationTokenSource = new CancellationTokenSource();
            _newFileEvent = new AutoResetEvent(false);
            _timeout = 10000;

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
                            File.Move(inFile, outFile);

                            var img = _section.AddImage(outFile);
                            img.Height = _document.DefaultPageSetup.PageHeight;
                            img.Width = _document.DefaultPageSetup.PageWidth;
                        }
                    }

                    if (!_newFileEvent.WaitOne(_timeout))
                    {
                        SaveAndCreateNewDocument();
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

        private bool TryOpenFile(string fileName, int tryCount)
        {

            for (int i = 0; i < tryCount; i++)
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
