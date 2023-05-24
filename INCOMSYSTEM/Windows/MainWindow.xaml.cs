using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Media.Animation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using INCOMSYSTEM.Context;
using System.Linq;
using INCOMSYSTEM.Pages;
using INCOMSYSTEM.Pages.MainPages;
using System.Windows.Input;

namespace INCOMSYSTEM.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ReviewFrame = new Frame();
            ChatFrame = new Frame();
            NavigationCommands.BrowseBack.InputGestures.Clear();
            NavigationCommands.BrowseForward.InputGestures.Clear();
            NavigationCommands.BrowseHome.InputGestures.Clear();
            NavigationCommands.BrowseStop.InputGestures.Clear();
            _sideBarMenu.Add(MenuItems.Review, null);
            _sideBarMenu.Add(MenuItems.Profile, null);
            _sideBarMenu.Add(MenuItems.Chat, null);
            IsClosed = false;
            ReviewFrame.Navigated += ReviewFrameOnNavigated;
            ChatFrame.Navigated += ChatFrameOnNavigated;
            if (AuthUser == null)
            {
                AuthBlock.Text = "Вы вошли как гость!";
                ProfileBtn.Visibility = Visibility.Collapsed;
                ChatBtn.Visibility = Visibility.Collapsed;
                ReviewFrame.Navigate(new ViewTasksPage());
                return;
            }
            SetAuthUserText();
            SetStartPage();
        }

        #region StartSettings
        private void SetAuthUserText()
        {
            using (var db = new INCOMSYSTEMEntities())
            {
                if (AuthUser.idPos == 1)
                {
                    var user = db.Customers.First(s => s.idUser == AuthUser.idUser);
                    AuthBlock.Text = $"{user.name}";
                }
                else
                {
                    var user = db.Employees.First(s => s.idUser == AuthUser.idUser);
                    AuthBlock.Text = $"{user}";
                }
            }
        }

        private void SetStartPage()
        {
            if (AuthUser == null)
            {
                ReviewFrame.Navigate(new ViewTasksPage());
                return;
            }
            
            switch (AuthUser.idPos)
            {
                case 1:
                    ReviewFrame.Navigate(new ViewTasksPage());
                    break;
                case 2:
                    ReviewFrame.Navigate(new ViewOrdersPage());
                    ChatBtn.Visibility = Visibility.Collapsed;
                    break;
                case 3:
                    ReviewFrame.Navigate(new ManagerPage());
                    break;
                default:
                    MessageBox.Show("Такого окна не существует, ты как вообще сюда попал?");
                    this.Close();
                    break;
            }
        }
        #endregion

        public static UsersDetail AuthUser { get; set; }

        #region ReviewNavigated
        private void ReviewFrameOnNavigated(object sender, NavigationEventArgs e)
        {
            if (!Equals(MFrame.Content, _sideBarMenu[MenuItems.Review])) return;
            var page = ((Frame)sender).Content as Page;
            this.Title = page?.Title;
            _sideBarMenu[MenuItems.Review] = page;
            Nav(page);
            BackBtn.IsEnabled = ReviewFrame.CanGoBack;
        }
        #endregion

        private async void Nav(Page page)
        {
            
            var anim = new DoubleAnimation
            {
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut },
                To = 0d,
                Duration = TimeSpan.FromSeconds(0.35)
            };

            MFrame.BeginAnimation(OpacityProperty, anim);

            await Task.Delay(350);

            MFrame.Content = page;
            
            await Task.Delay(25);

            anim.To = 1.0d;
            anim.Duration = TimeSpan.FromSeconds(0.35);
            MFrame.BeginAnimation(OpacityProperty, anim);
        }

        #region ChatNavigated
        private void ChatFrameOnNavigated(object sender, NavigationEventArgs e)
        {
            if (!Equals(MFrame.Content, _sideBarMenu[MenuItems.Chat])) return;
            var page = ((Frame)sender).Content as Page;
            this.Title = page?.Title;
            _sideBarMenu[MenuItems.Chat] = page;
            Nav(page);
            BackChatBtn.IsEnabled = !(page is ChatListPage);
        }
        #endregion

        public static Frame ReviewFrame { get; private set; }
        public static Frame ChatFrame { get; private set; }
        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            ReviewFrame.GoBack();
        }

        private async void QuitBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AuthUser != null)
            {
                using (var db = new INCOMSYSTEMEntities())
                {
                    var usr = db.UsersDetail.First(s => s.idUser == AuthUser.idUser);
                    usr.isOnline = false;
                    await db.SaveChangesAsync();
                }
            }

            IsClosed = true;
            new AuthWindow().Show();
            this.Close();
        }

        private readonly Dictionary<MenuItems, Page> _sideBarMenu = new Dictionary<MenuItems, Page>();
        public static bool IsClosed { get; private set; }
        private Page ChatListPage { get; set; }

        #region Buttons
        private void ReviewBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Title = _sideBarMenu[MenuItems.Review].Title;
            BackBtn.Visibility = Visibility.Visible;
            BackChatBtn.Visibility = Visibility.Collapsed;
            MFrame.Content = _sideBarMenu[MenuItems.Review];
        }

        private void ProfileBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_sideBarMenu[MenuItems.Profile] == null) _sideBarMenu[MenuItems.Profile] = new ProfilePage(AuthUser);
            
            this.Title = "Профиль";
            BackBtn.Visibility = Visibility.Collapsed;
            BackChatBtn.Visibility = Visibility.Collapsed;
            MFrame.Content = _sideBarMenu[MenuItems.Profile];
        }

        private void ChatBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_sideBarMenu[MenuItems.Chat] == null)
            {
                ChatListPage = new ChatListPage();
                _sideBarMenu[MenuItems.Chat] = ChatListPage;
            }
            
            this.Title = _sideBarMenu[MenuItems.Chat].Title;
            BackBtn.Visibility = Visibility.Collapsed;
            BackChatBtn.Visibility = Visibility.Visible;
            MFrame.Content = _sideBarMenu[MenuItems.Chat];
        }

        public void GoChat(Chats chat)
        {
            ChatBtn_Click(null, new RoutedEventArgs());

            if (!(ChatListPage is ChatListPage chatPage)) return;
            chatPage.Chat = chat;
            chatPage.ChatEnter_LeftBtnUp(null, null);
        }

        private void MainMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            ReviewBtn_Click(sender, e);
            ReviewFrame = new Frame();
            ReviewFrame.Navigated += ReviewFrameOnNavigated;
            SetStartPage();
        }

        private void BackChatBtn_Click(object sender, RoutedEventArgs e)
        {
            ChatFrame.Navigate(ChatListPage);
        }
        #endregion
    }

    internal enum MenuItems
    {
        Review = 0,
        Profile = 1,
        Chat = 2
    }
}
