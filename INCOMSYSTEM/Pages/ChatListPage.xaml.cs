using System.Windows.Controls;
using System.Windows.Input;
using INCOMSYSTEM.ViewModels;
using Page = System.Windows.Controls.Page;

namespace INCOMSYSTEM.Pages
{
    public partial class ChatListPage : Page
    {
        public ChatListPage()
        {
            InitializeComponent();
        }

        private void ChatEnter_LeftBtnUp(object sender, MouseButtonEventArgs e)
        {
            var chatMess = (sender as Grid).DataContext as ChatMess;
        }
    }
}