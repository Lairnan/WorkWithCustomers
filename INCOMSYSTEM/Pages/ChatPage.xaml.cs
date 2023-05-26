using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
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

            _tempTitle = chatMess.IsConnected ? $"Переписка с {chatMess.Recipient}" : chatMess.Recipient;
            if (!chatMess.IsConnected)
            {
                TitleEllipse.Visibility = Visibility.Collapsed;
                StatusBlock.Visibility = Visibility.Collapsed;
            }
            CollectionViewMessages.SortDescriptions.Insert(0, new SortDescription("SendDate", ListSortDirection.Ascending));
            MessagesList.ItemsSource = CollectionViewMessages;

            InputMessageBox.KeyDown += (s, e) =>
            {
                if (e.Key != Key.Enter) return;
                
                var start = InputMessageBox.SelectionStart;
                InputMessageBox.Text = InputMessageBox.Text.Insert(start++, "\n");
                InputMessageBox.SelectionStart = start;
            };
            
            Task.Run(RefreshMessages);
            Task.Run(RefreshStatusUser);
        }
        
        private readonly string _tempTitle;
        private bool _isOnline;

        private async void RefreshStatusUser()
        {
            Application.Current.Dispatcher.Invoke(() => TitleBlock.Text = _tempTitle);
            while (!MainWindow.IsClosed)
            {
                using (var db = new INCOMSYSTEMEntities())
                {
                    var mes = db.Messages.First(s => s.id == _chatMess.Id);
                    var chat = db.Chats.First(s => s.idChat == mes.idChat);
                    var id = MainWindow.AuthUser.idUser != chat.idManager ? chat.idManager : chat.idCustomer;
                    var user = db.UsersDetail.First(s => s.idUser == id);

                    if (_chatMess.IsConnected && TitleEllipse.Visibility == Visibility.Collapsed)
                    {
                         Application.Current.Dispatcher.Invoke(() => TitleBlock.Text = $"Переписка с {_chatMess.Recipient}");
                         Application.Current.Dispatcher.Invoke(() => TitleEllipse.Visibility = Visibility.Visible);
                         Application.Current.Dispatcher.Invoke(() => StatusBlock.Visibility = Visibility.Visible);
                    }

                    if(_isOnline == user.isOnline)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(3));
                        continue;
                    }
                    _isOnline = user.isOnline;
                    
                    if (user.isOnline)
                    {
                        Application.Current.Dispatcher.Invoke(() => TitleEllipse.Fill = new SolidColorBrush(Colors.Green));
                        Application.Current.Dispatcher.Invoke(() => StatusBlock.Text = "В сети");
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() => TitleEllipse.Fill = new SolidColorBrush(Colors.Red));
                        Application.Current.Dispatcher.Invoke(() => StatusBlock.Text = "Не сети");
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(3));
            }
        }

        private void MarkAllMessagesAsRead()
        {
            using (var db = new INCOMSYSTEMEntities())
            {
                foreach (var mess in MessagesCollection.Where(s => !s.IsRead && !s.ThisUser))
                {
                    var mes = db.Messages.First(s => s.id == mess.Id);
                    mes.isReadded = true;
                    mess.IsRead = true;
                }

                db.SaveChanges();
            }
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
                        .Include(s => s.HistoryUploaded)
                        .Where(s => s.idChat == _chatMess.Id)
                        .ToList();

                    foreach (var message in messages)
                    {
                        var dialogMess = new DialogMess
                        {
                            Id = message.id,
                            Message = message.message,
                            File = message.HistoryUploaded,
                            SendDate = message.dateSend,
                            IsRead = message.isReadded
                        };

                        if (MainWindow.AuthUser.idUser == message.idUser) dialogMess.ThisUser = true;

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

                    if (IsActive) Application.Current.Dispatcher.Invoke(MarkAllMessagesAsRead);
                    await Task.Delay(2500);
                }
            }
        }

        private bool _canSend = true;

        private void SendMessageBtn_Click(object sender, RoutedEventArgs e)
        {
            if (InputMessageBox.IsWhiteSpace)
            {
                ErrorBorder.Visibility = Visibility.Visible;
                SetError("Сообщение не может быть пустым");
                return;
            }

            if (InputMessageBox.Text.Length > 255)
            {
                ErrorBorder.Visibility = Visibility.Visible;
                SetError("Сообщение не должно превышать 255 символов");
                return;
            }
            
            if (!_canSend) { ErrorBorder.Visibility = Visibility.Visible; return; }

            Task.Run(async () =>
            {
                Application.Current.Dispatcher.Invoke(() => SendMessageBtn.IsEnabled = false);
                Application.Current.Dispatcher.Invoke(() => _canSend = false);
                for (var i = 5; i >= 0; i--)
                {
                    string timerStr;
                    if (i > 1) timerStr = $"{i} секунды";
                    else if (i == 1) timerStr = $"{i} секунду";
                    else timerStr = $"{i} секунд";
                    Application.Current.Dispatcher.Invoke(() => SetError($"Перед отправкой следующего сообщения подождите {timerStr}"));
                    await Task.Delay(1000);
                }

                Application.Current.Dispatcher.Invoke(() => _canSend = true);
                Application.Current.Dispatcher.Invoke(() => SendMessageBtn.IsEnabled = true);
                HideError();
            });
            
            HideError();
            SendMessage();
        }
    
        private async void SendMessage()
        {
            using (var db = await Task.Run(() => new INCOMSYSTEMEntities()))
            {
                var message = new Messages
                {
                    idChat = _chatMess.Id,
                    idUser = MainWindow.AuthUser.idUser,
                    message = InputMessageBox.Text,
                    dateSend = DateTime.Now
                };

                db.Messages.Add(message);
                await db.SaveChangesAsync();

                var dialogMess = new DialogMess
                {
                    Id = message.id,
                    Message = message.message,
                    SendDate = message.dateSend,
                    ThisUser = true,
                };

                switch (MainWindow.AuthUser.idPos)
                {
                    case 1:
                        dialogMess.Sender = MainWindow.AuthUser.Customers.name;
                        break;
                    case 3:
                        dialogMess.Sender = $"{MainWindow.AuthUser.Employees}";
                        break;
                }
                
                Application.Current.Dispatcher.Invoke(() => MessagesCollection.Add(dialogMess));

                InputMessageBox.Clear();
            }

            if (MessagesCollection.Count >= 2) return;
            Application.Current.Dispatcher.Invoke(() =>
            {
                var style = MessagesCollection.Count < 2 ? MessagesList.Style : null;
                MessagesList.Style = null;
                MessagesList.Style = style;
            });
        }

        private void SetError(string error)
        {
            ErrorBlock.Text = error;
        }

        private void HideError()
        {
            ErrorBorder.Visibility = Visibility.Collapsed;
            ErrorBlock.Text = string.Empty;
        }

        public bool IsActive { get; set; }
    }

    internal class DialogMess
    {
        public long Id { get; set; }
        public string Sender { get; set; }
        public string Message { get; set; }
        public HistoryUploaded File { get; set; }
        public DateTime SendDate { get; set; }
        public bool ThisUser { get; set; }
        public bool IsRead { get; set; }
    }
}