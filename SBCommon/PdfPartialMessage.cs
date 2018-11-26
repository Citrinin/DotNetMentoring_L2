using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

namespace SBCommon
{
    [Serializable]
    public class PdfPartialMessage
    {
        public bool EndOfSequence { get; set; }
        public Guid MessageId { get; set; }
        public byte[] PartialMessage { get; set; }
        public int SequenceNumber { get; set; }
    }

    public class PdfMessagesCreator
    {
        private static int MessageSize { get; set; } = 260_000;
        public async Task<IEnumerable<Message>> CreatePdfPartialMessageAsync(Stream ms)
        {
            if (ms == null)
            {
                return null;
            }

            var result = new List<PdfPartialMessage>();
            var identifier = Guid.NewGuid();
            var position = 0;
            var sequenceNumber = 0;
            while (position < ms.Length)
            {
                var size = ms.Length - position > MessageSize ? MessageSize : ms.Length - position;
                var messageContent = new byte[size];
                var count = ms.Read(messageContent, 0, (int)size);
                position += count;
                result.Add(new PdfPartialMessage
                {
                    EndOfSequence = position == ms.Length,
                    MessageId = identifier,
                    PartialMessage = messageContent,
                    SequenceNumber = sequenceNumber
                });
                sequenceNumber++;
            }
            return result.OrderBy(pm=>pm.SequenceNumber).Select(CreateMessage);
        }

        private Message CreateMessage(PdfPartialMessage pdfPartialMessage)
        {
            if (pdfPartialMessage == null)
                return null;

            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, pdfPartialMessage);
                return new Message(ms.ToArray());
            }
        }

        public PdfPartialMessage GetPdfPartialMessage(Message message)
        {
            var bf = new BinaryFormatter();
            var ms = new MemoryStream();

            ms.Write(message.Body, 0, message.Body.Length);
            ms.Seek(0, SeekOrigin.Begin);
            var pdfPartialMessage = (PdfPartialMessage)bf.Deserialize(ms);

            return pdfPartialMessage;
        }
    }

}
