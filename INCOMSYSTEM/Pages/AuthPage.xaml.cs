﻿using System;
using System.Linq;
using System.Threading.Tasks;
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

        private async void GenerateCaptcha()
        {
            const string pattern = "abcdefghijklmnopqrstuvwxyz" +
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                "1234567890" +
                "!@#$%";

            var captcha = "";
            for (var i = 0; i < _random.Next(4, 6); i++)
                captcha += pattern[_random.Next(0, pattern.Length)];

            _isAllowed = true;

            await Task.Delay(Timer * 1000);
            _isAllowed = false;
        }

        private int _attempts = 0;
        private bool _isAllowed;
        private const int Timer = 30;

        private void AuthBtnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(PassBox.Value);

            return;
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

            if(_attempts > 3) GenerateCaptcha();
        }

        private bool CheckEmpty()
        {
            if (LogBox.IsPlaceHolder || string.IsNullOrWhiteSpace(LogBox.Text)) return SetError("Логин не может быть пустым");
            if (PassBox.IsPlaceHolder || string.IsNullOrWhiteSpace(PassBox.Text)) return SetError("Пароль не может быть пустым");
            // if (CaptchaBox.Visibility == Visibility.Visible && string.IsNullOrWhiteSpace(CaptchaBox.Text)) return SetError("Капча не может быть пустой");

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