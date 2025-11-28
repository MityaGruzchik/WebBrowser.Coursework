using System.Windows;
using System.Windows.Controls;
using WebBrowser.Coursework.Managers;

namespace WebBrowser.Coursework.Views
{
    public partial class HistoryWindow : Window
    {
        private readonly HistoryManager _historyManager;

        // Ми передаємо Менеджер у вікно, а не створюємо його тут
        public HistoryWindow(HistoryManager manager)
        {
            InitializeComponent();
            _historyManager = manager;
            LoadData();
        }

        private void LoadData()
        {
            HistoryGrid.ItemsSource = _historyManager.GetAllHistory();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = SearchBox.Text;
            if (string.IsNullOrWhiteSpace(query))
            {
                LoadData();
            }
            else
            {
                // Виклик бізнес-логіки пошуку
                HistoryGrid.ItemsSource = _historyManager.Search(query);
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Видалити всю історію?", "Підтвердження", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _historyManager.ClearAllHistory();
                LoadData();
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}