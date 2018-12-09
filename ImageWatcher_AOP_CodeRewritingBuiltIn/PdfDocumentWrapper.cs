using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using SBCommon;

namespace ImageWatcher
{
    public class PdfDocumentWrapper
    {
        private Document _document;
        private Section _section;
        private PdfDocumentRenderer _renderer;
        private bool _isDocumentEmpty;

        private readonly IQueueClient client;

        public PdfDocumentWrapper()
        {
            var connectionString = ConfigurationManager.AppSettings["SBConnectionString"];
            var queueName = ConfigurationManager.AppSettings["QueueName"];
            client = new QueueClient(connectionString, queueName);
        }

        public bool IsDocumentEmpty { get; set; }

        public void AddImageToDocument(string imgPath)
        {
            var img = _section.AddImage(imgPath);
            img.Height = _document.DefaultPageSetup.PageHeight;
            img.Width = _document.DefaultPageSetup.PageWidth;
            _isDocumentEmpty = false;
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

        public async Task SaveDocument()
        {
            if (!_isDocumentEmpty)
            {
                _renderer.RenderDocument();
                var messages = await CreateMessage(_renderer);
                foreach (var message in messages)
                {
                    await client.SendAsync(message);
                }
            }
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
                var pdfCreator = new PdfMessagesCreator();
                return pdfCreator.CreatePdfPartialMessageAsync(ms);
            }
        }
    }
}