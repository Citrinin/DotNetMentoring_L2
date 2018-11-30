using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Xsl;

namespace HTMLReportGenerator
{
    class Program
    {
        static void Main(string[] args)
        {

            var inputFile = GetFileName("input file", "../../../books.xml");
            var processFile = GetFileName("processing XSLT file", "../../HtmlReport.xslt");
            var outputFile = GetFileName("output file", "../../result.html");



            var xsl = new XslCompiledTransform();

            xsl.Load(processFile);

            var xslParams = new XsltArgumentList();

            xslParams.AddParam("Date", "", DateTime.Now.ToLongDateString());

            xsl.Transform(inputFile, xslParams,new FileStream(outputFile, FileMode.Create));
        }

        public static string GetFileName(string purpose, string defaultFileName)
        {
            Console.WriteLine($"Please, enter the path to the {purpose} file (default: {defaultFileName})");
            var fileName = Console.ReadLine();
            if (!File.Exists(fileName))
            {
                Console.WriteLine($"Incorrect file path. {defaultFileName} will be used as input file");
                fileName = defaultFileName;
            }

            return fileName;
        }
    }
}
