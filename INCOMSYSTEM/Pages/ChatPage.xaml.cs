using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using INCOMSYSTEM.Context;
using INCOMSYSTEM.ViewModels;
using INCOMSYSTEM.Windows;
using Page = System.Windows.Controls.Page;

namespace INCOMSYSTEM.Pages
{
    public partial class ChatPage : Page
    {
        public ChatPage(ChatMess chatMess)
        {
            _chatMess = chatMess;
            InitializeComponent();

            Title = chatMess.IsConnected ? $"Переписка с {chatMess.Recipient}" : chatMess.Recipient;
            CollectionViewMessages.SortDescriptions.Insert(0, new SortDescription("SendDate", ListSortDirection.Ascending));
            MessagesList.ItemsSource = CollectionViewMessages;
            Task.Run(RefreshMessages);
        }

        private ObservableCollection<DialogMess> MessagesCollection { get; } = new ObservableCollection<DialogMess>();
        private ICollectionView CollectionViewMessages => CollectionViewSource.GetDefaultView(MessagesCollection);
            
        private readonly ChatMess _chatMess;

        private async void RefreshMessages()
        {
            while (!MainWindow.IsClosed)
            {
                using (var db = await Task.Run(() => new INCOMSYSTEMEntities()))
                {
                    var messages = db.Messages
                        .Include(s => s.UsersDetail)
                        .Include(s => s.UsersDetail.Employees)
                        .Include(s => s.UsersDetail.Customers)
                        .Include(s => s.Chats)
                        .Include(s => s.Chats.Orders)
                        .Include(s => s.Chats.Orders.Tasks)
                        .Where(s => s.idChat == _chatMess.Id)
                        .ToList();

                    foreach (var message in messages)
                    {
                        var dialogMess = new DialogMess
                        {
                            Id = message.id,
                            Message = message.message,
                            File = message.attachment,
                            FileExtension = message.fileExtension,
                            SendDate = message.dateSend,
                            FileName = message.attachment != null ? $"{message.Chats.Orders.Tasks.name}.{message.fileExtension}" : string.Empty
                        };

                        switch (message.UsersDetail.idPos)
                        {
                            case 1:
                                dialogMess.Sender = message.UsersDetail.Customers.name;
                                break;
                            case 3:
                                dialogMess.Sender = $"{message.UsersDetail.Employees}";
                                break;
                        }

                        if (MessagesCollection.FirstOrDefault(s => s.Id == dialogMess.Id) == null)
                            Application.Current.Dispatcher.Invoke(() => MessagesCollection.Add(dialogMess));
                    }

                    await Task.Delay(2500);
                }
            }
        }
    }

    internal class DialogMess
    {
        public long Id { get; set; }
        public string Sender { get; set; }
        public string Message { get; set; }
        public byte[] File { get; set; }
        public string FileExtension { get; set; }
        public string FileName { get; set; }
        public DateTime SendDate { get; set; }
    }
}