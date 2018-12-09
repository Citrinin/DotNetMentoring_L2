using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ImageWatcher.Configuration;
using SBCommon;

namespace ImageWatcher
{
    public interface IWatchingThreadParameter
    {
        FileSystemWatcher FileSystemWatcher { get; }
        string InputFolder { get; }
        string TempFolder { get; }
        void MoveFilesToCorruptedFolder();
        bool CheckIfImageContinuingSequence(string inFile);
        bool IsTimeoutOfNewDocumentEventExpired(int timeout);
        bool IsFileMatchPrefix(string fileName);
        void CreateNewDocument();
        Task SaveDocument();
        Task SaveAndCreateNewDocument();
        void AddImageToDocument(string imgPath);
    }

    public class WatchingThreadParameter : IWatchingThreadParameter
    {
        private readonly PdfDocumentWrapper _pdfDocumentWrapper;
        private int _imageSequenceNumber;

        private readonly string _corruptedFolder;
        private readonly string _outputFolder;
        private readonly string _prefix;
        private readonly AutoResetEvent _newFileEvent;


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

            _pdfDocumentWrapper = new PdfDocumentWrapper();
            _newFileEvent = new AutoResetEvent(false);

            FileSystemWatcher = new FileSystemWatcher(InputFolder);
            FileSystemWatcher.Created += (sender, args) => _newFileEvent.Set();
        }

        public FileSystemWatcher FileSystemWatcher { get; }

        public string InputFolder { get; }

        public string TempFolder { get; }

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

        public bool CheckIfImageContinuingSequence(string inFile)
        {
            var currentImageSequenceNumber = Utils.GetImageSequenceNumber(inFile, _prefix);
            var result = _imageSequenceNumber + 1 == currentImageSequenceNumber || _pdfDocumentWrapper.IsDocumentEmpty;

            _imageSequenceNumber = currentImageSequenceNumber;
            return result;
        }

        public bool IsTimeoutOfNewDocumentEventExpired(int timeout)
        {
            return !_newFileEvent.WaitOne(timeout) && !_pdfDocumentWrapper.IsDocumentEmpty;
        }

        public bool IsFileMatchPrefix(string fileName)
        {
            return Regex.IsMatch(fileName, $@"(?<={_prefix}_)(\d+).(?=\.(jpeg|jpg|png)$)");
        }

        public void CreateNewDocument()
        {
            _pdfDocumentWrapper.CreateNewDocument();
        }

        public async Task SaveDocument()
        {
            await _pdfDocumentWrapper.SaveDocument();
            Utils.ClearDirectory(TempFolder);
        }

        public async Task SaveAndCreateNewDocument()
        {
            await SaveDocument();
            CreateNewDocument();
        }

        public void AddImageToDocument(string imgPath)
        {
            _pdfDocumentWrapper.AddImageToDocument(imgPath);
        }
    }
}