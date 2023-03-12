﻿using INCOMSYSTEM.Context;
using INCOMSYSTEM.Windows;
using System;
using System.Data.Entity;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using INCOMSYSTEM.BehaviorsFiles;

namespace INCOMSYSTEM.ViewModels
{
    public class ChatListPageViewModel : ObservableObject
    {
        public ChatListPageViewModel()
        {
            ChatsCollection = new ObservableCollection<ChatMess>();
            ChatsCollection.CollectionChanged += (s, e) => OnRaiseChanged();
            RefreshCollection();
        }

        public ObservableCollection<ChatMess> ChatsCollection { get; }

        public async void RefreshCollection()
        {
            while (!MainWindow.IsClosed)
            {
                using (var db = await Task.Run(() => new INCOMSYSTEMEntities()))
                {
                    var chats = db.Chats.Include(s => s.Customers)
                                            .Include(s => s.Employees)
                                            .Include(s => s.Employees.UsersDetail)
                                            .Include(s => s.Employees.UsersDetail)
                                            .Include(s => s.Orders)
                                            .Include(s => s.Orders.Tasks)
                                            .Include(s => s.Orders.Statuses)
                                            .Where(s => s.idCustomer == MainWindow.AuthUser.idUser || s.idManager == MainWindow.AuthUser.idUser)
                                            .ToList();

                    var messages = db.Messages
                        .Include(s => s.UsersDetail)
                        .ToList();

                    var chatColl = ChatsCollection.ToList().Where(s => !chats.Select(k => k.idChat).Contains(s.Id));

                    if (MainWindow.AuthUser.idPos == 3 && chatColl.Any())
                    {
                        foreach (var c in chatColl)
                        {
                            ChatsCollection.Remove(c);
                        }
                        continue;
                    }

                    foreach (var chat in chats)
                    {
                        var message = messages.LastOrDefault(s => s.idChat == chat.idChat) ?? new Messages
                        {
                            idChat = chat.idChat,
                            dateSend = chat.dateCreate,
                            message = $"Подробности заказа \"{chat.Orders.Tasks.name}\"",
                            idUser = chat.idCustomer,
                            UsersDetail = chat.Customers.UsersDetail
                        };
                        var chatMess = new ChatMess
                        {
                            Id = chat.idChat,
                            Chat = chat,
                            SendDate = message.dateSend,
                            Message = message,
                            Order = chat.Orders,
                            IsConnected = chat.Employees != null
                        };

                        if (MainWindow.AuthUser.idUser == chat.idCustomer) chatMess.Recipient = chat.Employees == null
                                ? "Ожидание подключения менеджера"
                                : $"{chat.Employees.surname} {chat.Employees.name} {chat.Employees.patronymic}".Trim();
                        else chatMess.Recipient = chat.Customers.name;

                        var chatMessColl = ChatsCollection.FirstOrDefault(s => s.Id == chatMess.Id);
                        if (chatMessColl == null) ChatsCollection.Add(chatMess);
                        else
                        {
                            chatMessColl.IsConnected = chatMess.IsConnected;
                            if (chatMessColl.Chat.Employees != chat.Employees)
                            {
                                chatMessColl.Chat.idManager = chat.idManager;
                                chatMessColl.Chat.Employees = chat.Employees;
                                chatMessColl.Recipient = chatMess.Recipient;
                            }
                            if (chatMessColl.Chat.Employees != null && chatMessColl.Message.id != chatMess.Id)
                            {
                                chatMessColl.SendDate = chatMess.SendDate;
                                chatMessColl.Message = chatMess.Message;
                            }
                        }
                    }
                }

                await Task.Delay(2500);
            }
        }
    }

    public class ChatMess : ObservableObject
    {
        private long _id;
        private string _recipient;
        private Chats _chat;
        private Orders _order;
        private Messages _message;
        private DateTime? _sendDate;
        private bool _isConnected;

        public long Id
        {
            get => _id;
            set
            {
                _id = value;
                OnRaiseChanged();
            }
        }
        public string Recipient
        {
            get => _recipient;
            set
            {
                _recipient = value;
                OnRaiseChanged();
            }
        }
        public Chats Chat
        {
            get => _chat;
            set
            {
                _chat = value;
                OnRaiseChanged();
            }
        }
        public Orders Order
        {
            get => _order;
            set
            {
                _order = value;
                OnRaiseChanged();
            }
        }
        public Messages Message
        {
            get => _message;
            set
            {
                _message = value;
                OnRaiseChanged();
            }
        }
        public DateTime? SendDate
        {
            get => _sendDate;
            set
            {
                _sendDate = value;
                OnRaiseChanged();
            }
        }
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;
                OnRaiseChanged();
            }
        }
    }
}
