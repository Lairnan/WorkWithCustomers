using System.Windows;
using System.Windows.Controls;
using INCOMSYSTEM.Windows;

namespace INCOMSYSTEM.Pages.MainPages
{
    /// <summary>
    /// Логика взаимодействия для ManagerPage.xaml
    /// </summary>
    public partial class ManagerPage : Page
    {
        public ManagerPage()
        {
            InitializeComponent();
        }
        private void Tasks_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.MainFrame.Navigate(new ViewTasksPage());
        }

        private void Order_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.MainFrame.Navigate(new ViewOrdersPage());
        }
    }
}
