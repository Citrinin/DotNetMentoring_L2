using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using ZXing;


namespace ImageWatcher
{
    public static class Utils
    {
        private static object _barcodeLock = new object();

        private static readonly BarcodeReader _barcodeReader = new BarcodeReader() { AutoRotate = true };
        public static string GetTimeStamp()
        {
            return (new string(DateTime.Now.ToString().Where(char.IsDigit).ToArray()) + DateTime.Now.Millisecond);
        }

        public static void CheckDirectory(string directoryName, bool clearDirectory)
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

        public static void ClearDirectory(string directoryName)
        {
            foreach (var file in Directory.EnumerateFiles(directoryName))
            {
                if (TryOpenFile(file, 3))
                {
                    File.Delete(file);
                }
            }
        }

        public static bool TryOpenFile(string fileName, int tryCount)
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

        public static int GetImageSequenceNumber(string inFile, string prefix)
        {
            var fileNumberMatch = Regex.Match(inFile, $@"(?<={prefix}_)(\d+).(?=\.(jpeg|jpg|png)$)");
            if (int.TryParse(fileNumberMatch.Value, out var number))
            {
                return number;
            }

            throw new ArgumentException($"Wrong filename: {inFile}");
        }

        public static string GetImageBarcodeValue(string filename)
        {
            lock (_barcodeLock)
            {
                using (var bmp = (Bitmap)Image.FromFile(filename))
                {
                    return _barcodeReader.Decode(bmp)?.Text;
                }
            }
        }
    }
}
