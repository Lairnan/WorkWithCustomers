using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using INCOMSYSTEM.Context;
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
        public Chats Chat { get; set; }

        public void ChatEnter_LeftBtnUp(object sender, MouseButtonEventArgs e)
        {
            ChatMess chatMess;
            if (Chat != null)
            {
                if (ChatPages.ContainsKey(Chat.idChat))
                {
                    MainWindow.ChatFrame.Navigate(ChatPages[Chat.idChat]);
                }
                else
                {
                    chatMess = new ChatMess
                    {
                        Id = Chat.idChat,
                        IsConnected = true,
                        Recipient = Chat.Customers.name
                    };
                    var chatPage = new ChatPage(chatMess);
                    ChatPages.Add(Chat.idChat, chatPage);
                    MainWindow.ChatFrame.Navigate(chatPage);
                }
            }
            else
            {
                chatMess = (sender as Grid).DataContext as ChatMess;
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

            Chat = null;
        }
    }
}