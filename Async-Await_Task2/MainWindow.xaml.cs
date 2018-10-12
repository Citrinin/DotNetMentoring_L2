using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Async_Await_Task2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private static HttpClient _client;
        public ObservableCollection<UriWrapper> LinksToDownload { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            LinksToDownload = new ObservableCollection<UriWrapper>();
            DataContext = this;
            _client = new HttpClient();
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            var uriWrapper = (sender as Button)?.DataContext as UriWrapper;
            uriWrapper?.TokenSource.Cancel();
        }

        private async void DownloadButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (IsAbsoluteUrl(UrlTextBox.Text))
            {
                var urlString = UrlTextBox.Text;
                UrlTextBox.Text = "";
                var uri = new UriWrapper { UriAddress = new Uri(urlString), TokenSource = new CancellationTokenSource() };
                LinksToDownload.Add(uri);
                var fileName = GetFileName(uri.UriAddress);
                var fs = File.Create(fileName);

                try
                {
                    await Task.Delay(2000, uri.TokenSource.Token);
                    var response = await _client.GetAsync(uri.UriAddress, uri.TokenSource.Token);
                    var contentStream = await response.Content.ReadAsStreamAsync();

                    await contentStream.CopyToAsync(fs, 80000, uri.TokenSource.Token);
                    contentStream.Close();
                }
                catch (TaskCanceledException)
                {
                }
                finally
                {
                    fs.Close();
                    if (uri.TokenSource.IsCancellationRequested)
                    {
                        File.Delete(fileName);
                    }
                    LinksToDownload.Remove(uri);
                }
            }
            else
            {
                MessageBox.Show("Incorrent URL");
            }
        }

        private static bool IsAbsoluteUrl(string url) => Uri.TryCreate(url, UriKind.Absolute, out var result);

        private static string GetFileName(Uri url)
        {
            var file = url.OriginalString;

            if (url.OriginalString.Contains(@"http://"))
            {
                file = url.OriginalString.Substring(7);
            }

            if (url.OriginalString.Contains(@"https://"))
            {
                file = url.OriginalString.Substring(8);
            }

            var fileParts = file.TrimEnd('/').Split('/');
            var fileName = fileParts.Last()
                .Replace('?', '+')
                .Replace(":", "")
                .Replace("/", "")
                .Replace("\\", "")
                .Replace("*", "")
                .Replace("?", "")
                .Replace("|", "")
                .Replace("<", "")
                .Replace(">", "")
                .Replace("\"", "");

            if (fileName.Length > 200)
            {
                fileName = fileName.Substring(0, 200);
            }

            return GetFileVersion(fileName) + ".html";
        }

        private static string GetFileVersion(string fileName)
        {
            while (true)
            {
                var existingFileName = fileName + ".html";
                if (File.Exists(existingFileName))
                {
                    var match = Regex.Match(existingFileName, @"(?<=\()\d+(?=\).html$)");

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
    }
}
