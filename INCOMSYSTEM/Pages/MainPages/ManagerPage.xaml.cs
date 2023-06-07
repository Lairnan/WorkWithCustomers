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
        private void ViewTasksBtn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ReviewFrame.Navigate(new ViewTasksPage());
        }

        private void ViewOrderBtn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ReviewFrame.Navigate(new ViewOrdersPage());
        }

        private void ViewUsersBtn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ReviewFrame.Navigate(new ViewUsersPage());
        }
    }
}
