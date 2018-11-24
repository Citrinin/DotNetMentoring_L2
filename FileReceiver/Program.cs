using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using SBCommon;

namespace FileReceiver
{
    class Program
    {
        private static string connString =
            "Endpoint=sb://messagequeuestask.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=eZrzUCBPE+R+VlHed0FMkCqcUV6SzfDoAhXD9e2EmPk=";
        private static string queueName = "imageswatcherqueue";
        private static IQueueClient client;
        private static Dictionary<Guid, List<PdfPartialMessage>> storage = new Dictionary<Guid, List<PdfPartialMessage>>();
        private static PdfMessagesCreator pdfMessagesCreator = new PdfMessagesCreator();

        static void Main(string[] args)
        {
            client = new QueueClient(connString, queueName, ReceiveMode.ReceiveAndDelete);
            ReadMessagAsync();

            Console.ReadKey();
        }

        public static async Task ReadMessagAsync()
        {

            client.RegisterMessageHandler(ProcessingFunction,
                args =>
                {
                    Console.WriteLine(args.Exception.ToString());
                    return Task.FromResult("success");
                });
        }

        public static async Task ProcessingFunction(Message message, CancellationToken token)
        {
            Console.WriteLine("Received");
            GetDocumentFromMessage(message);
            return;
        }

        public static void GetDocumentFromMessage(Message message)
        {
            if (message == null)
            {
                return;
            }

            var pdfMessage = pdfMessagesCreator.GetPdfPartialMessage(message);

            if (storage.ContainsKey(pdfMessage.MessageId))
            {
                storage[pdfMessage.MessageId].Add(pdfMessage);
            }
            else
            {
                storage.Add(pdfMessage.MessageId, new List<PdfPartialMessage> { pdfMessage });
            }

            if (pdfMessage.EndOfSequence)
            {
                using (var fs = File.Create(@"D:\imageWatcher\watcher1\out\" + Guid.NewGuid() + ".pdf"))
                {
                    var messages = storage[pdfMessage.MessageId].OrderBy(m => m.SequenceNumber);
                    foreach (var pdfPartialMessage in messages)
                    {
                        fs.Write(pdfPartialMessage.PartialMessage, 0, pdfPartialMessage.PartialMessage.Length);
                    }
                }
                storage.Remove(pdfMessage.MessageId);
            }
        }


    }
}
