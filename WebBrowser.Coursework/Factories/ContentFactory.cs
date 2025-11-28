namespace WebBrowser.Coursework.Factories
{
    public class ContentFactory
    {
        // Фабричний метод
        public IBrowserContent CreateContent(string url)
        {
            // Якщо URL починається зі спец. префіксу - це внутрішня сторінка
            if (url.StartsWith("browser://"))
            {
                return new InternalPageContent();
            }
            // В іншому випадку - це звичайна веб-сторінка
            else
            {
                return new WebPageContent();
            }
        }
    }
}