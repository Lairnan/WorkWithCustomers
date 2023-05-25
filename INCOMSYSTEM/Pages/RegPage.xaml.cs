using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using INCOMSYSTEM.BehaviorsFiles;
using INCOMSYSTEM.Context;
using INCOMSYSTEM.Controls;
using INCOMSYSTEM.Windows;

namespace INCOMSYSTEM.Pages
{
    /// <summary>
    /// Логика взаимодействия для RegPage.xaml
    /// </summary>
    public partial class RegPage : Page
    {
        public RegPage()
        {
            InitializeComponent();

            using (var db = new INCOMSYSTEMEntities())
            {
                CmbLegal.ItemsSource = db.LegalForms.ToList();
            }
            
            this.KeyDown += (s, e) => { if (e.Key == Key.Enter && RegBtn.IsEnabled) RegBtn_Click(s, e); };

            InpName.TextChanged += (s, e) => CheckEmpty();
            InpPhone.TextChanged += (s, e) => CheckEmpty();
            InpPassportSerie.TextChanged += (s, e) => CheckEmpty();
            InpPassportNumber.TextChanged += (s, e) => CheckEmpty();
            InpAddress.TextChanged += (s, e) => CheckEmpty();
            InpLogin.TextChanged += (s, e) => CheckEmpty();
            InpPassword.TextChanged += (s, e) => CheckEmpty();
        }


        private void RegBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CheckEmpty()) return;
            if (InpPassword.Value.Length < 6)
            {
                ShowError("Длина пароля должна достигать минимум 6 символов");
                return;
            }
            if (PasswordDifficulty.CheckDifficultyPassword(InpPassword.Value) == DifficultyPassword.Easy)
            {
                ShowError("Пароль не может быть слабым");
                return;
            }

            using (var db = new INCOMSYSTEMEntities())
            {
                var phone = InpPhone.Value;
                    
                var passport = InpPassportSerie.Value + InpPassportNumber.Value;
                
                if (db.UsersDetail.FirstOrDefault(s => s.login.ToLower() == InpLogin.Value.ToLower().Trim()) != null)
                {
                    ShowError("Такой логин уже занят");
                    return;
                }
                if (db.UsersDetail.FirstOrDefault(s => s.phone == phone) != null)
                {
                    ShowError("Номер телефона уже занят");
                    return;
                }
                if (db.UsersDetail.FirstOrDefault(s => s.passport == passport) != null)
                {
                    ShowError("Паспорт уже занят");
                    return;
                }
                
                var userDetail = new UsersDetail
                {
                    login = InpLogin.Value,
                    password = InpPassword.Value,
                    phone = phone,
                    idPos = 1,
                    passport = passport,
                    address = InpAddress.Value,
                    dateStart = DateTime.Now,
                    isOnline = true
                };
                db.UsersDetail.Add(userDetail);
                var customer = new Customers
                {
                    name = InpName.Value,
                    idLegalForm = ((LegalForms)CmbLegal.SelectedItem).id,
                };
                db.Customers.Add(customer);
                db.SaveChanges();
                MessageBox.Show("Вы успешно зарегистрировались");
                MainWindow.AuthUser = userDetail;
                new MainWindow().Show();
                Application.Current.Windows.OfType<AuthWindow>().First().Close();
            }
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            AuthWindow.AuthFrame.Navigate(new AuthPage());
        }

        private bool CheckEmpty()
        {
            if (InpName.IsWhiteSpace) return ShowError("Название/фио не может быть пустым");

            switch (InpPhone.IsWhiteSpace)
            {
                case false when !long.TryParse(InpPhone.Value, out _):
                    return ShowError("Номер телефона не может содержать символы");
                case false when InpPhone.Value.Length != 11:
                    return ShowError("Номер телефона должен содержать 11 цифр");
            }

            if (InpPassportSerie.IsWhiteSpace) return ShowError("Серия паспорта не может быть пустым");
            if (!long.TryParse(InpPassportSerie.Value, out _)) return ShowError("Серия паспорта не может содержать символы");
            if (InpPassportSerie.Value.Length != 4) return ShowError("Серия паспорта должен содержать 4 цифры");
            
            if (InpPassportNumber.IsWhiteSpace) return ShowError("Номер паспорта не может быть пустым");
            if (!long.TryParse(InpPassportNumber.Value, out _)) return ShowError("Номер паспорта не может содержать символы");
            if (InpPassportNumber.Value.Length != 6) return ShowError("Номер паспорта должен содержать 6 цифры");
            
            if (InpAddress.IsWhiteSpace) return ShowError("Адрес не может быть пустым");
            if (InpLogin.IsWhiteSpace) return ShowError("Логин не может быть пустым");
            if (InpPassword.IsWhiteSpace) return ShowError("Пароль не может быть пустым");

            RegBtn.IsEnabled = true;
            ErrorBlock.Text = string.Empty;
            ErrorBorder.Visibility = Visibility.Collapsed;
            return false;
        }

        private bool ShowError(string error)
        {
            RegBtn.IsEnabled = false;
            ErrorBorder.Visibility = Visibility.Visible;
            ErrorBlock.Text = error;
            return true;
        }

        private void GenerateNewPasswordBtn_Click(object sender, RoutedEventArgs e)
        {
            InpPassword.IsShowed = true;
            InpPassword.Text = PasswordGenerator.GetNewPassword();
        }

        private void CmbLegal_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NameBlock.Text = ((LegalForms)CmbLegal.SelectedItem).id == 1 ? "ФИО" : "Название";
        }
    }

}
