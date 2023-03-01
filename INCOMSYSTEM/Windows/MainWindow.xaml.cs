using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Media.Animation;
using System;
using System.Threading.Tasks;
using INCOMSYSTEM.Context;
using System.Linq;
using INCOMSYSTEM.Pages.MainPages;

namespace INCOMSYSTEM.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame = MFrame;
            MainFrame.Navigated += MainFrameOnNavigated;
            if (AuthUser == null)
            {
                AuthBlock.Text = "Вы вошли как гость!";
                MainFrame.Navigate(new ViewTasksPage());
            }
            AuthBlock.Text = $"{AuthUser.Positions.name}, ";
            using (var db = new INCOMSYSTEMEntities())
            {
                if (AuthUser.idPos == 1)
                {
                    var user = db.Customers.First(s => s.idUser == AuthUser.idUser);
                    AuthBlock.Text += $"{user.name}!";
                }
                else
                {
                    var user = db.Employees.First(s => s.idUser == AuthUser.idUser);
                    AuthBlock.Text += $"{user.surname} {user.name}";
                    AuthBlock.Text += user.patronymic != null ? $" {user.patronymic}" : "";
                }
            }

            switch (AuthUser.idPos)
            {
                case 1:
                    MainFrame.Navigate(new ViewTasksPage());
                    break;
                case 2:
                    MainFrame.Navigate(new ViewOrdersPage());
                    break;
                case 3:
                    MainFrame.Navigate(new ManagerPage());
                    break;
                default:
                    MessageBox.Show("Такого окна не существует, ты как вообще сюда попал?");
                    break;
            }
        }

        public static UsersDetail AuthUser { get; set; }

        private void MainFrameOnNavigated(object sender, NavigationEventArgs e)
        {
            Nav(sender);
            BackBtn.IsEnabled = MainFrame.CanGoBack;
        }

        private async void Nav(object sender)
        {
            var page = ((Frame)sender).Content as Page;
            this.Title = page?.Title;

            //var anim = new DoubleAnimation
            //{
            //    EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut },
            //    To = 0d,
            //    Duration = TimeSpan.FromSeconds(0.7)
            //};

            //MFrame.BeginAnimation(OpacityProperty, anim);

            //await Task.Delay(700);

            //await Task.Delay(25);

            //anim.To = 1.0d;
            //anim.Duration = TimeSpan.FromSeconds(0.7);
            //MFrame.BeginAnimation(OpacityProperty, anim);
        }

        public static Frame MainFrame { get; private set; }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.GoBack();
        }

        private void QuitBtn_Click(object sender, RoutedEventArgs e)
        {
            new AuthWindow().Show();
            this.Close();
        }
    }
}
