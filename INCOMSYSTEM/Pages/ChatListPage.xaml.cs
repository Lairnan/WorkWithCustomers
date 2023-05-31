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
                if (ChatPages.TryGetValue(Chat.idChat, out var page))
                {
                    ((ChatPage)page).IsActive = true;
                    MainWindow.ChatFrame.Navigate(page);
                }
                else
                {
                    chatMess = new ChatMess
                    {
                        Id = Chat.idChat,
                        IsConnected = Chat.idManager != null,
                        Recipient = Chat.Customers.name,
                        Chat = Chat,
                    };
                    var chatPage = new ChatPage(chatMess);
                    ChatPages.Add(Chat.idChat, chatPage);
                    chatPage.IsActive = true;
                    MainWindow.ChatFrame.Navigate(chatPage);
                }
            }
            else
            {
                chatMess = (sender as Grid).DataContext as ChatMess;
                if (ChatPages.TryGetValue(chatMess.Id, out var page))
                {
                    ((ChatPage)page).IsActive = true;
                    MainWindow.ChatFrame.Navigate(page);
                }
                else
                {
                    var chatPage = new ChatPage(chatMess);
                    ChatPages.Add(chatMess.Id, chatPage);
                    chatPage.IsActive = true;
                    MainWindow.ChatFrame.Navigate(chatPage);
                }
            }

            Chat = null;
        }
    }
}