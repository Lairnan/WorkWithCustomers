using INCOMSYSTEM.Pages;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Media.Animation;
using System;
using System.Threading.Tasks;

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
            MainFrame.Navigate(new ViewTasks());
        }

        private void MainFrameOnNavigated(object sender, NavigationEventArgs e)
        {
            Nav(sender);
            BackBtn.IsEnabled = MainFrame.CanGoBack;
        }

        private async void Nav(object sender)
        {
            var anim = new DoubleAnimation
            {
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut },
                To = 0d,
                Duration = TimeSpan.FromSeconds(0.7)
            };

            MFrame.BeginAnimation(OpacityProperty, anim);

            await Task.Delay(700);

            var page = ((Frame)sender).Content as Page;
            this.Title = page?.Title;

            await Task.Delay(25);

            anim.To = 1.0d;
            anim.Duration = TimeSpan.FromSeconds(0.7);
            MFrame.BeginAnimation(OpacityProperty, anim);
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
