using System;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Net.Http; 
using Microsoft.Web.WebView2.Wpf; 
using WebBrowser.Coursework.Entities;
using WebBrowser.Coursework.Managers;
using WebBrowser.Coursework.Repositories;
using WebBrowser.Coursework.Views;
using WebBrowser.Coursework.Services; 
using WebBrowser.Coursework.Factories; 
using WebBrowser.Coursework.Patterns; 

namespace WebBrowser.Coursework
{
    public partial class MainWindow : Window
    {
        
        private const string ConnString = "Host=localhost;Username=postgres;Password=12345;Database=browser_db";

        private readonly HistoryManager _historyManager;
        private readonly IRepository<Bookmark> _bookmarkRepo;

        private readonly IPageLoader _pageLoader = new SmartProxy();

        private readonly ContentFactory _contentFactory = new ContentFactory();

        private IBrowserContent _currentContent;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                var historyRepo = new HistoryRepository(ConnString);
                _bookmarkRepo = new BookmarkRepository(ConnString);
                _historyManager = new HistoryManager(historyRepo);

                LoadBookmarksSidePanel();

                AddressBar.Text = "https://www.google.com";
                NavigateToUrl();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Критична помилка запуску: {ex.Message}");
            }
        }

        private async void NavigateToUrl()
        {
            var url = AddressBar.Text;
            if (string.IsNullOrWhiteSpace(url)) return;

            if (!url.StartsWith("browser://") && !url.StartsWith("http"))
            {
                url = "https://" + url;
            }

            try
            {
                _currentContent = _contentFactory.CreateContent(url);

                MainContentContainer.Content = _currentContent.GetVisualElement();

                if (_currentContent is WebPageContent webContent)
                {

                    int statusCode = 0;
                    try
                    {
                        using (var client = new HttpClient())
                        {
                            var request = new HttpRequestMessage(HttpMethod.Head, url);
                            var response = await client.SendAsync(request);
                            statusCode = (int)response.StatusCode;
                        }
                    }
                    catch
                    {
                        statusCode = 404;
                    }

                    var hRedirect = new RedirectHandler();
                    var hClientError = new ClientErrorHandler();
                    var hServerError = new ServerErrorHandler();
                    var hSuccess = new SuccessHandler();

                    hRedirect.SetNext(hClientError).SetNext(hServerError).SetNext(hSuccess);

                    var context = new RequestContext
                    {
                        Url = url,
                        StatusCode = statusCode,
                        PageContent = webContent 
                    };

                    hRedirect.Handle(context);

                    if (statusCode >= 200 && statusCode < 400)
                    {
                        webContent.Load(url); 

                        _historyManager.AddEntry("Web Page", url);
                    }
                }
                else
                {
                    _currentContent.Load(url);
                }

                AddressBar.Text = url;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка навігації: {ex.Message}");
            }
        }


        private void BtnGo_Click(object sender, RoutedEventArgs e)
        {
            NavigateToUrl();
        }

        private void AddressBar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) NavigateToUrl();
        }

        private WebView2 GetCurrentWebView()
        {
            if (_currentContent is WebPageContent webContent)
            {
                return webContent.GetVisualElement() as WebView2;
            }
            return null;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            var wv = GetCurrentWebView();
            if (wv != null && wv.CanGoBack) wv.GoBack();
        }

        private void BtnForward_Click(object sender, RoutedEventArgs e)
        {
            var wv = GetCurrentWebView();
            if (wv != null && wv.CanGoForward) wv.GoForward();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            var wv = GetCurrentWebView();
            if (wv != null) wv.Reload();
            else NavigateToUrl(); 
        }

        private void BtnOpenHistory_Click(object sender, RoutedEventArgs e)
        {
            var historyWindow = new HistoryWindow(_historyManager);
            historyWindow.Owner = this;
            historyWindow.Show();
        }


        private void BtnAddBookmark_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string title = "New Page";
                string url = AddressBar.Text;

                var wv = GetCurrentWebView();
                if (wv != null && wv.CoreWebView2 != null)
                {
                    title = wv.CoreWebView2.DocumentTitle;
                    url = wv.Source.ToString();
                }

                var bookmark = new Bookmark
                {
                    Title = string.IsNullOrEmpty(title) ? "Bookmark" : title,
                    Url = url,
                    CreatedAt = DateTime.Now
                };

                _bookmarkRepo.Add(bookmark);
                LoadBookmarksSidePanel();
                MessageBox.Show("Закладку додано!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка БД: {ex.Message}");
            }
        }

        private void LoadBookmarksSidePanel()
        {
            try
            {
                ListBookmarks.ItemsSource = _bookmarkRepo.GetAll();
            }
            catch {  }
        }

        private void ListBookmarks_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ListBookmarks.SelectedItem is Bookmark b)
            {
                AddressBar.Text = b.Url;
                NavigateToUrl();
            }
        }


        private async void BtnViewSource_Click(object sender, RoutedEventArgs e)
        {
            string url = AddressBar.Text;
            if (string.IsNullOrEmpty(url)) return;

            if (url.StartsWith("browser://"))
            {
                MessageBox.Show("Це внутрішня сторінка, вихідний код недоступний.");
                return;
            }

            BtnViewSource.Content = "Wait...";
            BtnViewSource.IsEnabled = false;

            string htmlContent = await Task.Run(() =>
            {
                return _pageLoader.DownloadHtml(url);
            });

            string preview = htmlContent.Length > 800 ? htmlContent.Substring(0, 800) + "..." : htmlContent;
            MessageBox.Show(preview, "HTML Source (Proxy Pattern)");

            BtnViewSource.Content = "📄 HTML Code";
            BtnViewSource.IsEnabled = true;
        }
    }
}