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
            MFrame.Navigated += MainFrameOnNavigated;
        }

        private void MainFrameOnNavigated(object sender, NavigationEventArgs e)
        {
            var page = ((Frame)sender).Content as Page;
            this.Title = page?.Title;
        }
    }
}