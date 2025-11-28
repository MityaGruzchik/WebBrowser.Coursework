using System;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Net.Http; // Для перевірки статусів (Lab 5)
using Microsoft.Web.WebView2.Wpf; // Для кастингу типів
using WebBrowser.Coursework.Entities;
using WebBrowser.Coursework.Managers;
using WebBrowser.Coursework.Repositories;
using WebBrowser.Coursework.Views;
using WebBrowser.Coursework.Services; // Proxy
using WebBrowser.Coursework.Factories; // Factory Method (Lab 6)
using WebBrowser.Coursework.Patterns;  // Chain of Responsibility (Lab 5)

namespace WebBrowser.Coursework
{
    public partial class MainWindow : Window
    {
        // Рядок підключення (перевірте пароль!)
        private const string ConnString = "Host=localhost;Username=postgres;Password=12345;Database=browser_db";

        // --- Архітектурні компоненти ---
        private readonly HistoryManager _historyManager;
        private readonly IRepository<Bookmark> _bookmarkRepo;

        // Lab 4: Proxy
        private readonly IPageLoader _pageLoader = new SmartProxy();

        // Lab 6: Factory Method
        private readonly ContentFactory _contentFactory = new ContentFactory();

        // Поточний контент вкладки (може бути Web або Internal)
        private IBrowserContent _currentContent;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                // Ініціалізація шарів даних та логіки
                var historyRepo = new HistoryRepository(ConnString);
                _bookmarkRepo = new BookmarkRepository(ConnString);
                _historyManager = new HistoryManager(historyRepo);

                // Завантаження закладок у бічну панель
                LoadBookmarksSidePanel();

                // Завантаження стартової сторінки
                AddressBar.Text = "https://www.google.com";
                NavigateToUrl();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Критична помилка запуску: {ex.Message}");
            }
        }

        // =========================================================
        // ГОЛОВНА ЛОГІКА НАВІГАЦІЇ (Factory + Chain)
        // =========================================================

        private async void NavigateToUrl()
        {
            var url = AddressBar.Text;
            if (string.IsNullOrWhiteSpace(url)) return;

            // Валідація протоколу
            if (!url.StartsWith("browser://") && !url.StartsWith("http"))
            {
                url = "https://" + url;
            }

            try
            {
                // КРОК 1 (Lab 6): Фабрика створює контент залежно від URL
                // Якщо це "browser://about" -> InternalPageContent
                // Якщо це "http://google.com" -> WebPageContent
                _currentContent = _contentFactory.CreateContent(url);

                // Відображаємо створений елемент у вікні (Dynamic UI)
                MainContentContainer.Content = _currentContent.GetVisualElement();

                // КРОК 2: Розгалуження логіки залежно від типу контенту
                if (_currentContent is WebPageContent webContent)
                {
                    // --- ЛОГІКА ДЛЯ ВЕБ-САЙТІВ (Lab 5: Chain of Responsibility) ---

                    // 2.1. Перевіряємо статус код через HttpClient
                    int statusCode = 0;
                    try
                    {
                        using (var client = new HttpClient())
                        {
                            // HEAD запит швидший, бо не качає тіло сторінки
                            var request = new HttpRequestMessage(HttpMethod.Head, url);
                            var response = await client.SendAsync(request);
                            statusCode = (int)response.StatusCode;
                        }
                    }
                    catch
                    {
                        // Якщо помилка мережі, імітуємо 404 або 500 для тесту ланцюжка
                        statusCode = 404;
                    }

                    // 2.2. Налаштування Ланцюжка
                    var hRedirect = new RedirectHandler();
                    var hClientError = new ClientErrorHandler();
                    var hServerError = new ServerErrorHandler();
                    var hSuccess = new SuccessHandler();

                    hRedirect.SetNext(hClientError).SetNext(hServerError).SetNext(hSuccess);

                    // 2.3. Створення контексту запиту
                    var context = new RequestContext
                    {
                        Url = url,
                        StatusCode = statusCode,
                        PageContent = webContent // Передаємо об'єкт для відображення помилок/контенту
                    };

                    // 2.4. Запуск ланцюжка
                    // Обробники самі вирішать: показати HTML помилки чи нічого не робити
                    hRedirect.Handle(context);

                    // 2.5. Якщо статус ОК (2xx), то SuccessHandler дозволяє завантаження
                    // (Або якщо це редірект 3xx, який WebView2 обробить сам)
                    if (statusCode >= 200 && statusCode < 400)
                    {
                        webContent.Load(url); // Реальне завантаження у WebView2

                        // Збереження в історію (Lab 2/3)
                        _historyManager.AddEntry("Web Page", url);
                    }
                }
                else
                {
                    // --- ЛОГІКА ДЛЯ СЛУЖБОВИХ СТОРІНОК (Internal) ---
                    // Тут не потрібен HttpClient і ланцюжок перевірок
                    _currentContent.Load(url);
                }

                // Оновлюємо рядок адреси (для краси)
                AddressBar.Text = url;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка навігації: {ex.Message}");
            }
        }

        // =========================================================
        // ОБРОБНИКИ ПОДІЙ ІНТЕРФЕЙСУ
        // =========================================================

        private void BtnGo_Click(object sender, RoutedEventArgs e)
        {
            NavigateToUrl();
        }

        private void AddressBar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) NavigateToUrl();
        }

        // Допоміжний метод для отримання WebView2, якщо він зараз активний
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
            else NavigateToUrl(); // Для текстових сторінок просто перезавантажуємо
        }

        private void BtnOpenHistory_Click(object sender, RoutedEventArgs e)
        {
            var historyWindow = new HistoryWindow(_historyManager);
            historyWindow.Owner = this;
            historyWindow.Show();
        }

        // =========================================================
        // ЛОГІКА ЗАКЛАДОК (Lab 2)
        // =========================================================

        private void BtnAddBookmark_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string title = "New Page";
                string url = AddressBar.Text;

                // Спробуємо дістати реальний заголовок з WebView2
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
            catch { /* Ігноруємо помилки під час завантаження UI */ }
        }

        private void ListBookmarks_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ListBookmarks.SelectedItem is Bookmark b)
            {
                AddressBar.Text = b.Url;
                NavigateToUrl();
            }
        }

        // =========================================================
        // ЛОГІКА PROXY (Lab 4)
        // =========================================================

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

            // Виклик через Proxy (кешування)
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