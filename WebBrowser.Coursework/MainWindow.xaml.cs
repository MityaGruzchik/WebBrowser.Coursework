using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Web.WebView2.Core;
using WebBrowser.Coursework.Entities;
using WebBrowser.Coursework.Managers;     // Підключення шару бізнес-логіки
using WebBrowser.Coursework.Repositories; // Підключення шару даних
using WebBrowser.Coursework.Views;        // Підключення шару представлення (друге вікно)
using System.Threading.Tasks;
using WebBrowser.Coursework.Services; // Підключаємо нашу папку Services
using System.Net.Http;
using System.Threading.Tasks;
using WebBrowser.Coursework.Patterns;

namespace WebBrowser.Coursework
{
    public partial class MainWindow : Window
    {
        // Рядок підключення (Змініть пароль!)
        private const string ConnString = "Host=localhost;Username=postgres;Password=12345;Database=browser_db";

        // Архітектурні компоненти
        private readonly HistoryManager _historyManager; // Бізнес-логіка історії
        private readonly IRepository<Bookmark> _bookmarkRepo; // Прямий доступ (для спрощення, але можна теж через менеджер)
                                                              // Ми працюємо через інтерфейс, але ініціалізуємо саме Proxy
        private readonly IPageLoader _pageLoader = new SmartProxy();

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                // 1. Ініціалізація Data Layer (Repositories)
                var historyRepo = new HistoryRepository(ConnString);
                _bookmarkRepo = new BookmarkRepository(ConnString);

                // 2. Ініціалізація Logic Layer (Managers)
                // Впроваджуємо залежність (Dependency Injection)
                _historyManager = new HistoryManager(historyRepo);

                // 3. Ініціалізація UI
                LoadBookmarksSidePanel();
                BrowserView.NavigationCompleted += BrowserView_NavigationCompleted;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка запуску: {ex.Message}");
            }
        }

        // --- ЛОГІКА ВІДКРИТТЯ ДРУГОЇ ФОРМИ ---

        private void BtnOpenHistory_Click(object sender, RoutedEventArgs e)
        {
            // Створюємо друге вікно, передаючи йому вже існуючий менеджер
            // Це демонструє зв'язок між компонентами UI
            var historyWindow = new HistoryWindow(_historyManager);

            historyWindow.Owner = this; // Встановлюємо батьківське вікно
            historyWindow.Show();       // Відкриваємо не модально (можна працювати паралельно)
        }

        // --- ЛОГІКА БРАУЗЕРА ТА ЗБЕРЕЖЕННЯ ДАНИХ ---

        private void BrowserView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                AddressBar.Text = BrowserView.Source.ToString();

                // Викликаємо БІЗНЕС-ЛОГІКУ, а не базу даних напряму
                // Менеджер сам вирішить, як валідувати та зберігати дані
                _historyManager.AddEntry(
                    BrowserView.CoreWebView2.DocumentTitle,
                    BrowserView.Source.ToString()
                );
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

        // Цей метод викликається при натисканні Enter або кнопки Go
        private async void NavigateToUrl()
        {
            string url = AddressBar.Text;
            if (string.IsNullOrWhiteSpace(url)) return;

            if (!url.StartsWith("http")) url = "https://" + url;

            // 1. Налаштування Ланцюжка (Chain Configuration)
            var hRedirect = new RedirectHandler();
            var hClientError = new ClientErrorHandler();
            var hServerError = new ServerErrorHandler();
            var hSuccess = new SuccessHandler();

            // Будуємо ланцюг: Redirect -> ClientError -> ServerError -> Success
            hRedirect.SetNext(hClientError).SetNext(hServerError).SetNext(hSuccess);

            // 2. Отримання статус-коду (HTTP HEAD Request)
            // Ми робимо це окремим клієнтом, щоб перевірити сторінку ДО того, як її покаже WebView2
            int statusCode = 0;
            try
            {
                using (var client = new HttpClient())
                {
                    // Використовуємо SendAsync з HttpCompletionOption.ResponseHeadersRead
                    // щоб не качати все тіло сторінки, а тільки заголовки
                    var request = new HttpRequestMessage(HttpMethod.Head, url);
                    // Деякі сайти не підтримують HEAD, тоді пробуємо GET
                    var response = await client.SendAsync(request);
                    statusCode = (int)response.StatusCode;
                }
            }
            catch (HttpRequestException)
            {
                // Якщо домену не існує або немає інтернету
                MessageBox.Show("Не вдалося з'єднатися з сервером.", "Мережева помилка");
                return;
            }

            // 3. Запуск Ланцюжка
            var context = new RequestContext
            {
                Url = url,
                StatusCode = statusCode,
                Browser = BrowserView
            };

            hRedirect.Handle(context);
        }

        // Навігація
        private void BtnBack_Click(object sender, RoutedEventArgs e) { if (BrowserView.CanGoBack) BrowserView.GoBack(); }
        private void BtnForward_Click(object sender, RoutedEventArgs e) { if (BrowserView.CanGoForward) BrowserView.GoForward(); }
        private void BtnRefresh_Click(object sender, RoutedEventArgs e) { BrowserView.Reload(); }

        // --- ЛОГІКА ЗАКЛАДОК (Спрощена бічна панель) ---

        private void BtnAddBookmark_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var bookmark = new Bookmark
                {
                    Title = BrowserView.CoreWebView2.DocumentTitle ?? "New Page",
                    Url = BrowserView.Source.ToString(),
                    CreatedAt = DateTime.Now
                };

                _bookmarkRepo.Add(bookmark);
                LoadBookmarksSidePanel(); // Оновити список
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving bookmark: {ex.Message}");
            }
        }

        private async void BtnViewSource_Click(object sender, RoutedEventArgs e)
        {
            string url = AddressBar.Text;
            if (string.IsNullOrEmpty(url)) return;

            // Блокуємо кнопку і показуємо статус
            BtnViewSource.Content = "Завантаження...";
            BtnViewSource.IsEnabled = false;

            // Запускаємо в окремому потоці, щоб Thread.Sleep у RealLoader не заморозив вікно
            string htmlContent = await Task.Run(() =>
            {
                return _pageLoader.DownloadHtml(url);
            });

            // Показуємо результат (перші 500 символів, щоб не спамити)
            string preview = htmlContent.Length > 500 ? htmlContent.Substring(0, 500) + "..." : htmlContent;

            MessageBox.Show(preview, "HTML Source Code");

            // Відновлюємо кнопку
            BtnViewSource.Content = "📄 HTML Code";
            BtnViewSource.IsEnabled = true;
        }

        private void LoadBookmarksSidePanel()
        {
            try
            {
                ListBookmarks.ItemsSource = _bookmarkRepo.GetAll();
            }
            catch { /* Тихо ігноруємо помилки підключення для UI */ }
        }

        private void ListBookmarks_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ListBookmarks.SelectedItem is Bookmark b)
            {
                BrowserView.Source = new Uri(b.Url);
            }
        }
    }
}