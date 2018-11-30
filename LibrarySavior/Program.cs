using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace LibrarySavior
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlReaderSettings settings = new XmlReaderSettings();

            settings.Schemas.Add("http://library.by/catalog", "../../books_schema.xsd");
            settings.ValidationEventHandler += (sender, eventArgs) =>
            {
                Console.WriteLine($"On {eventArgs.Exception.LineNumber} line at {eventArgs.Exception.LinePosition} position occured error:\n\t {eventArgs.Message}\n");
            };

            settings.ValidationFlags = settings.ValidationFlags | XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.ValidationType = ValidationType.Schema;

            XmlReader reader = XmlReader.Create("../../books.xml", settings);

            while (reader.Read());
            Console.ReadKey();
        }
    }
}
