using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using INCOMSYSTEM.Context;

namespace INCOMSYSTEM.Pages
{
    public partial class AuthPage : Page
    {
        private readonly Random Random = new Random();

        public AuthPage()
        {
            InitializeComponent();
            LogBox.TextChanged += (s, e) => CheckEmpty();
            PassBox.PasswordChanged += (s, e) => CheckEmpty();

            this.KeyDown += (s, e) => { if (e.Key == Key.Enter && AuthBtn.IsEnabled) AuthBtnClick(s, e); };
            AuthBtn.Click += AuthBtnClick;
            RegBtn.Click += RegBtnClick;
        }

        private void RegBtnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Test");
        }

        private void GenerateCaptcha()
        {
            const string pattern = "abcdefghijklmnopqrstuvwxyz" +
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                "1234567890" +
                "!@#$%";

            var captcha = "";
            for (var i = 0; i < Random.Next(4, 6); i++)
                captcha += pattern[Random.Next(0, pattern.Length)];

            MessageBox.Show(captcha);
        }

        private void AuthBtnClick(object sender, RoutedEventArgs e)
        {
            if(CheckEmpty())
            {
                MessageBox.Show("Поля не могут быть пустыми");
                return;
            }

            GenerateCaptcha();
        }

        private bool CheckEmpty()
        {
            if (string.IsNullOrWhiteSpace(LogBox.Text)) return SetError("Логин не может быть пустым");
            if (string.IsNullOrWhiteSpace(PassBox.Password)) return SetError("Пароль не может быть пустым");

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
    }
}