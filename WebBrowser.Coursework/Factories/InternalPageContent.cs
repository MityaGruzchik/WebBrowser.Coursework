using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WebBrowser.Coursework.Factories
{
    public class InternalPageContent : IBrowserContent
    {
        private readonly StackPanel _panel;
        private readonly TextBlock _textBlock;

        public InternalPageContent()
        {
            _panel = new StackPanel { Background = Brushes.WhiteSmoke, Margin = new Thickness(10) };
            _textBlock = new TextBlock { FontSize = 24, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 50, 0, 0) };
            _panel.Children.Add(_textBlock);
        }

        public void Load(string url)
        {
            string command = url.Replace("browser://", "").ToLower();

            switch (command)
            {
                case "about":
                    _textBlock.Text = "Студентський Веб-Браузер v1.0\nКурсова робота";
                    _textBlock.Foreground = Brushes.DarkBlue;
                    break;
                case "help":
                    _textBlock.Text = "Довідка користувача:\n1. Введіть URL для переходу.\n2. Використовуйте browser://about для інформації.";
                    _textBlock.Foreground = Brushes.Green;
                    break;
                default:
                    _textBlock.Text = $"Невідома службова сторінка: {command}";
                    _textBlock.Foreground = Brushes.Red;
                    break;
            }
        }

        public UIElement GetVisualElement()
        {
            return _panel;
        }
    }
}