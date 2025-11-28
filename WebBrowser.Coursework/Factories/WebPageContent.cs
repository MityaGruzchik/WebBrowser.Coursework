using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Wpf;

namespace WebBrowser.Coursework.Factories
{
    public class WebPageContent : IBrowserContent
    {
        // Приватне поле для WebView2
        private readonly WebView2 _webView;

        public WebPageContent()
        {
            _webView = new WebView2();
        }

        public async void Load(string url)
        {
            await _webView.EnsureCoreWebView2Async();
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                _webView.Source = uri;
            }
        }

        // --- ДОДАНО ЦЕЙ МЕТОД ДЛЯ ВИПРАВЛЕННЯ ПОМИЛКИ ---
        public async void LoadHtml(string html)
        {
            await _webView.EnsureCoreWebView2Async();
            _webView.NavigateToString(html);
        }

        public UIElement GetVisualElement()
        {
            return _webView;
        }
    }
}