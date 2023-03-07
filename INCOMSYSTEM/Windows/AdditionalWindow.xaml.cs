using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace INCOMSYSTEM.Windows
{
    public partial class AdditionalWindow : Window
    {
        public AdditionalWindow()
        {
            InitializeComponent();
            _errorStaticBorder = ErrorBorder;
            _errorStaticBlock = ErrorBlock;
            MFrame.Navigated += MainFrameOnNavigated;
        }

        private static Border _errorStaticBorder;
        private static TextBlock _errorStaticBlock;

        private void MainFrameOnNavigated(object sender, NavigationEventArgs e)
        {
            var page = ((Frame)sender).Content as Page;
            this.Title = page?.Title;
            this.Width = page.Width + 50d;
            this.Height = page.Height + 50d;
        }

        public static void ShowError(string error)
        {
            _errorStaticBlock.Text = error;
            _errorStaticBorder.Visibility = Visibility.Visible;
        }

        public static void HideError()
        {
            _errorStaticBlock.Text = string.Empty;
            _errorStaticBorder.Visibility = Visibility.Collapsed;
        }
    }
}