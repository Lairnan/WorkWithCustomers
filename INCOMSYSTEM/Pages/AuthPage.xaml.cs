using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using INCOMSYSTEM.Context;
using INCOMSYSTEM.Windows;
using System.Data.Entity;

namespace INCOMSYSTEM.Pages
{
    public partial class AuthPage : Page
    {
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
            AuthWindow.AuthFrame.Navigate(new RegPage());
        }


        private void AuthBtnClick(object sender, RoutedEventArgs e)
        {
            if(CheckEmpty())
            {
                MessageBox.Show("Поля не могут быть пустыми");
                return;
            }

            var login = LogBox.Value;
            var password = PassBox.Value;

            using (var db = new INCOMSYSTEMEntities())
            {
                var user = db.UsersDetail.Include(s => s.Positions)
                    .Include(s => s.Employees)
                    .Include(s => s.Customers)
                    .Include(s => s.Customers.LegalForms)
                    .FirstOrDefault(s => s.login == login && s.password == password);
                if (user != null)
                {
                    MainWindow.AuthUser = user;
                    var window = new MainWindow();
                    window.Show();
                    Application.Current.Windows.OfType<AuthWindow>().First().Close();

                    return;
                }

                ShowError("Неверный логин или пароль");
            }
        }

        private bool CheckEmpty()
        {
            if (LogBox.IsWhiteSpace) return ShowError("Логин не может быть пустым");
            if (PassBox.IsWhiteSpace) return ShowError("Пароль не может быть пустым");

            AuthBtn.IsEnabled = true;
            ErrorBlock.Text = string.Empty;
            ErrorBorder.Visibility = Visibility.Collapsed;
            return false;
        }

        private bool ShowError(string error)
        {
            AuthBtn.IsEnabled = false;
            ErrorBorder.Visibility = Visibility.Visible;
            ErrorBlock.Text = error;
            return true;
        }

        private void GuestBtn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.AuthUser = null;
            new MainWindow().Show();
            Application.Current.Windows.OfType<AuthWindow>().First().Close();
        }
    }
}