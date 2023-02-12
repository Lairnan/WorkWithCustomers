using INCOMSYSTEM.Windows;
using System.Windows;
using System.Windows.Controls;

namespace INCOMSYSTEM.Pages
{
    /// <summary>
    /// Логика взаимодействия для ViewTasks.xaml
    /// </summary>
    public partial class ViewTasks : Page
    {
        public ViewTasks()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.MainFrame.Navigate(new ViewTasks());
        }
    }
}
