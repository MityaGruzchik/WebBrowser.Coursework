using System.Windows;

namespace WebBrowser.Coursework.Factories
{
    // Інтерфейс (Product), з яким буде працювати головне вікно
    public interface IBrowserContent
    {
        void Load(string url);
        UIElement GetVisualElement(); // Повертає XAML-компонент для відображення
    }
}