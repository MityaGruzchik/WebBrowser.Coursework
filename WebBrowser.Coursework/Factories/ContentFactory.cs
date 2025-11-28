namespace WebBrowser.Coursework.Factories
{
    public class ContentFactory
    {
        public IBrowserContent CreateContent(string url)
        {
            if (url.StartsWith("browser://"))
            {
                return new InternalPageContent();
            }
            else
            {
                return new WebPageContent();
            }
        }
    }
}