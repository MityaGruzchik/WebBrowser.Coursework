using System.Windows;

namespace WebBrowser.Coursework.Factories
{
    public interface IBrowserContent
    {
        void Load(string url);
        UIElement GetVisualElement(); 
    }
}