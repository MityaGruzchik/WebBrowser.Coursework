using Microsoft.Web.WebView2.Wpf;

namespace WebBrowser.Coursework.Patterns
{
    public class RequestContext
    {
        public string Url { get; set; }
        public int StatusCode { get; set; }
        public WebView2 Browser { get; set; } // Посилання на компонент браузера для керування ним

        // Допоміжний метод для відображення HTML (для сторінок помилок)
        public void RenderHtml(string title, string message, string color)
        {
            string html = $@"
                <html>
                <body style='font-family: sans-serif; text-align: center; padding-top: 50px; background-color: #f8f9fa;'>
                    <h1 style='color: {color};'>{title}</h1>
                    <p>{message}</p>
                    <hr>
                    <small>Processed by Chain of Responsibility</small>
                </body>
                </html>";

            Browser.NavigateToString(html);
        }
    }
}