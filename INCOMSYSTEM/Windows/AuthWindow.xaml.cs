using System.Windows.Controls;
using System.Windows.Navigation;
using INCOMSYSTEM.Pages;

namespace INCOMSYSTEM.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class AuthWindow
    {
        public AuthWindow()
        {
            InitializeComponent();
            AuthFrame = AFrame;
            AuthFrame.Navigated += AuthFrameOnNavigated;
            AuthFrame.Navigate(new AuthPage());
        }

        private void AuthFrameOnNavigated(object sender, NavigationEventArgs e)
        {
            var page = ((Frame)sender).Content as Page;
            this.Title = page?.Title;
        }

        public static Frame AuthFrame { get; private set; }
    }
}