namespace WebBrowser.Coursework.Services
{
    // Спільний інтерфейс для Реального завантажувача та Проксі.
    // Клієнтський код (MainWindow) буде залежати саме від цього інтерфейсу.
    public interface IPageLoader
    {
        string DownloadHtml(string url);
    }
}