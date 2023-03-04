﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Media.Animation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using INCOMSYSTEM.Context;
using System.Linq;
using INCOMSYSTEM.Pages.MainPages;

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
            _sideBarMenu.Add(MenuItems.Review, null);
            _sideBarMenu.Add(MenuItems.Profile, new ViewOrdersPage());
            _sideBarMenu.Add(MenuItems.Chat, new Page());
            ReviewFrame.Navigated += MainFrameOnNavigated;
            if (AuthUser == null)
            {
                AuthBlock.Text = "Вы вошли как гость!";
                ReviewFrame.Navigate(new ViewTasksPage());
                return;
            }
            SetAuthUserText();
            SetStartPage();
        }

        private void SetAuthUserText()
        {
            AuthBlock.Text = $"{AuthUser.Positions.name}, ";
            using (var db = new INCOMSYSTEMEntities())
            {
                if (AuthUser.idPos == 1)
                {
                    var user = db.Customers.First(s => s.idUser == AuthUser.idUser);
                    AuthBlock.Text += $"{user.name}!";
                }
                else
                {
                    var user = db.Employees.First(s => s.idUser == AuthUser.idUser);
                    AuthBlock.Text += $"{user.surname} {user.name}";
                    AuthBlock.Text += user.patronymic != null ? $" {user.patronymic}" : "";
                }
            }
        }

        private void SetStartPage()
        {
            switch (AuthUser.idPos)
            {
                case 1:
                    ReviewFrame.Navigate(new ViewTasksPage());
                    break;
                case 2:
                    ReviewFrame.Navigate(new ViewOrdersPage());
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

        public static UsersDetail AuthUser { get; set; }

        private void MainFrameOnNavigated(object sender, NavigationEventArgs e)
        {
            if (!Equals(MFrame.Content, _sideBarMenu[MenuItems.Review])) return;
            var page = ((Frame)sender).Content as Page;
            this.Title = page?.Title;
            _sideBarMenu[MenuItems.Review] = page;
            Nav(page);
            BackBtn.IsEnabled = ReviewFrame.CanGoBack;
        }

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

        public static Frame ReviewFrame { get; private set; }
        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            ReviewFrame.GoBack();
        }

        private void QuitBtn_Click(object sender, RoutedEventArgs e)
        {
            new AuthWindow().Show();
            this.Close();
        }

        private readonly Dictionary<MenuItems, Page> _sideBarMenu = new Dictionary<MenuItems, Page>();

        private void ReviewBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Title = _sideBarMenu[MenuItems.Review].Title;
            BackBtn.Visibility = Visibility.Visible;
            MFrame.Content = _sideBarMenu[MenuItems.Review];
        }
        
        private void ProfileBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Title = "Профиль";
            BackBtn.Visibility = Visibility.Collapsed;
            MFrame.Content = _sideBarMenu[MenuItems.Profile];
        }
        
        private void ChatBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Title = "Мессенджер";
            BackBtn.Visibility = Visibility.Collapsed;
            MFrame.Content = _sideBarMenu[MenuItems.Chat];
        }

        private void MainMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            ReviewBtn_Click(sender, e);
            ReviewFrame = new Frame();
            ReviewFrame.Navigated += MainFrameOnNavigated;
            SetStartPage();
        }
    }

    internal enum MenuItems
    {
        Review = 0,
        Profile = 1,
        Chat = 2
    }
}
