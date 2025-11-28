using System.Net.Http;
using System.Threading;

namespace WebBrowser.Coursework.Services
{
    public class RealPageLoader : IPageLoader
    {
        public string DownloadHtml(string url)
        {
            Thread.Sleep(3000);

            using (var client = new HttpClient())
            {
                try
                {
                    var response = client.GetAsync(url).Result;
                    return response.Content.ReadAsStringAsync().Result;
                }
                catch
                {
                    return $"<html><body><h1>Error loading {url}</h1></body></html>";
                }
            }
        }
    }
}