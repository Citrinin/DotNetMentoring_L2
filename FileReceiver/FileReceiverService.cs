using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SBCommon;
using System.IO;

namespace FileReceiver
{
    public class FileReceiverService
    {
        private readonly IQueueClient _client;
        private readonly Dictionary<Guid, List<PdfPartialMessage>> _storage = new Dictionary<Guid, List<PdfPartialMessage>>();
        private readonly PdfMessagesCreator _pdfMessagesCreator = new PdfMessagesCreator();
        private readonly ManualResetEvent _stopWorkEvent;
        private readonly string _outputFolder;
        private Task _workingTask;


        public FileReceiverService()
        {
            var connectionString = ConfigurationManager.AppSettings["SBConnectionString"];
            var queueName = ConfigurationManager.AppSettings["QueueName"];
            _client = new QueueClient(connectionString, queueName, ReceiveMode.ReceiveAndDelete);
            _stopWorkEvent = new ManualResetEvent(false);

            _outputFolder = ConfigurationManager.AppSettings["OutputFolder"];
            Utils.CheckDirectory(_outputFolder, false);
        }

        public void Start()
        {
            _workingTask = Task.Run( async () =>
            {
                _client.RegisterMessageHandler((message, token) =>
                    {
                        MessageHandler(message);
                        return Task.CompletedTask;
                    },
                    args =>
                    {
                        Console.WriteLine(args.Exception.ToString());
                        return Task.FromResult("success");
                    }
                );
                _stopWorkEvent.WaitOne();
                await _client.CloseAsync();
            });
        }

        public void Stop()
        {
            _stopWorkEvent.Set();
            _workingTask.Wait();
            Console.WriteLine("End of work");
        }

        private void MessageHandler(Message message)
        {
            if (message == null)
            {
                return;
            }

            var pdfMessage = _pdfMessagesCreator.GetPdfPartialMessage(message);

            if (_storage.ContainsKey(pdfMessage.MessageId))
            {
                _storage[pdfMessage.MessageId].Add(pdfMessage);
            }
            else
            {
                _storage.Add(pdfMessage.MessageId, new List<PdfPartialMessage> { pdfMessage });
            }

            var messages = _storage[pdfMessage.MessageId].ToList();

            if (CheckIfSequenceIsReceived(messages))
            {
                using (var fs = File.Create(Path.Combine(_outputFolder, $"result-{Utils.GetTimeStamp()}.pdf")))
                {
                    foreach (var pdfPartialMessage in messages.OrderBy(m => m.SequenceNumber))
                    {
                        fs.Write(pdfPartialMessage.PartialMessage, 0, pdfPartialMessage.PartialMessage.Length);
                    }
                }
                _storage.Remove(pdfMessage.MessageId);
            }
        }

        private bool CheckIfSequenceIsReceived(List<PdfPartialMessage> messages)
        {
            var lastMessage = messages.FirstOrDefault(m => m.EndOfSequence);
            if (lastMessage == null)
            {
                return false;
            }

            return messages.Count - 1 == lastMessage.SequenceNumber;
        }
    }
}
