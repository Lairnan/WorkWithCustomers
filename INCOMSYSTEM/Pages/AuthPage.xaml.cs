using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using INCOMSYSTEM.Context;

namespace INCOMSYSTEM.Pages
{
    public partial class AuthPage : Page
    {
        private readonly Random _random = new Random();

        public AuthPage()
        {
            InitializeComponent();
            LogBox.TextChanged += (s, e) => CheckEmpty();
            PassBox.TextChanged += (s, e) => CheckEmpty();

            this.KeyDown += (s, e) => { if (e.Key == Key.Enter && AuthBtn.IsEnabled) AuthBtnClick(s, e); };
            AuthBtn.Click += AuthBtnClick;
            RegBtn.Click += RegBtnClick;
        }

        private void RegBtnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Test");
        }

        private int _attempts = 0;

        private void AuthBtnClick(object sender, RoutedEventArgs e)
        {
            if(CheckEmpty())
            {
                MessageBox.Show("Поля не могут быть пустыми");
                return;
            }

            // if (attemps++ > 3)
            // {
            //     TODO: Проверка на капчу
            // }

            var login = LogBox.Value;
            var password = PassBox.Value;

            using (var db = new INCOMSYSTEMEntities())
            {
                var user = db.UsersDetail.FirstOrDefault(s => s.login == login && s.password == password);
                if (user != null)
                {
                    switch (user.idPos)
                    {
                        case 1:
                            MessageBox.Show("Вы заказчик!");
                            break;
                        case 2:
                            MessageBox.Show("Вы исполнитель!");
                            break;
                        case 3:
                            MessageBox.Show("Вы менеджер!");
                            break;
                    }

                    return;
                }

                SetError("Неверный логин или пароль");
            }

            if (_attempts > 3)
            {
                // TODO: Генерация новой капчи;
            }
        }

        private bool CheckEmpty()
        {
            if (LogBox.IsPlaceHolder || string.IsNullOrWhiteSpace(LogBox.Value)) return SetError("Логин не может быть пустым");
            if (PassBox.IsPlaceHolder || string.IsNullOrWhiteSpace(PassBox.Value)) return SetError("Пароль не может быть пустым");

            AuthBtn.IsEnabled = true;
            ErrorBlock.Text = string.Empty;
            ErrorBlock.Visibility = Visibility.Collapsed;
            return false;
        }

        private bool SetError(string error)
        {
            AuthBtn.IsEnabled = false;
            ErrorBlock.Visibility = Visibility.Visible;
            ErrorBlock.Text = error;
            return true;
        }

        private void GuestBtn_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}