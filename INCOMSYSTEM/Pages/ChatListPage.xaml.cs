using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using INCOMSYSTEM.ViewModels;
using INCOMSYSTEM.Windows;
using Page = System.Windows.Controls.Page;

namespace INCOMSYSTEM.Pages
{
    public partial class ChatListPage : Page
    {
        public ChatListPage()
        {
            InitializeComponent();
        }

        private Dictionary<long, Page> ChatPages { get; } = new Dictionary<long, Page>();

        private void ChatEnter_LeftBtnUp(object sender, MouseButtonEventArgs e)
        {
            var chatMess = (sender as Grid).DataContext as ChatMess;
            if (ChatPages.ContainsKey(chatMess.Id))
            {
                MainWindow.ChatFrame.Navigate(ChatPages[chatMess.Id]);
            }
            else
            {
                var chatPage = new ChatPage(chatMess);
                ChatPages.Add(chatMess.Id, chatPage);
                MainWindow.ChatFrame.Navigate(chatPage);
            }
        }
    }
}