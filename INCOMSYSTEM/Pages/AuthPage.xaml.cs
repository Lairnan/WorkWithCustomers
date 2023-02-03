using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace INCOMSYSTEM.Pages
{
    public partial class AuthPage : Page
    {
        public AuthPage()
        {
            InitializeComponent();
            LogBox.TextChanged += (s, e) => CheckEmpty();
            PassBox.PasswordChanged += (s, e) => CheckEmpty();
            
            this.KeyDown += OnKeyDown;
            AuthBtn.Click += AuthBtnClick;
            RegBtn.Click += RegBtnClick;
        }

        private void RegBtnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Test");
        }

        private void AuthBtnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Test");
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter && AuthBtn.IsEnabled) AuthBtnClick(sender, e);
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