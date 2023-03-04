using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using INCOMSYSTEM.BehaviorsFiles;
using INCOMSYSTEM.Context;
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
            
            this.KeyDown += (s, e) => { if (e.Key == Key.Enter && AuthBtn.IsEnabled) RegBtn_Click(s, e); };
        }
        
        private void RegBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CheckEmpty()) return;
            if (PasswordDifficulty.CheckDifficultyPassword(InpPassword.Value) == DifficultyPassword.Easy)
            {
                SetError("Пароль не может быть слабым");
                return;
            }

            using (var db = new INCOMSYSTEMEntities())
            {
                long? phone = null;
                if (!InpPhone.IsWhiteSpace)
                {
                    try { phone = long.Parse(InpPhone.Value); }
                    catch { /* Ignored */ }
                }
                long.TryParse(InpPassport.Value, out var passport);
                
                var userDetail = new UsersDetail
                {
                    login = InpLogin.Value,
                    password = InpPassword.Value,
                    phone = phone,
                    idPos = 1,
                    passport = passport,
                    address = InpAddress.Value,
                    dateStart = DateTime.Now
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

        private void AuthBtn_Click(object sender, RoutedEventArgs e)
        {
            AuthWindow.AuthFrame.Navigate(new AuthPage());
        }

        private void InpTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckEmpty();
        }

        private bool CheckEmpty()
        {
            if (InpName.IsWhiteSpace) return SetError("Название/фио не может быть пустым");
            if (InpPassport.IsWhiteSpace) return SetError("Паспорт не может быть пустым");
            if (InpAddress.IsWhiteSpace) return SetError("Адрес не может быть пустым");
            if (InpLogin.IsWhiteSpace) return SetError("Логин не может быть пустым");
            if (InpPassword.IsWhiteSpace) return SetError("Пароль не может быть пустым");

            if (!long.TryParse(InpPassport.Value, out _)) return SetError("Паспорт не может содержать символы");
            if (InpPassport.Value.Length != 10) return SetError("Паспорт должен содержать 10 символов");

            RegBtn.IsEnabled = true;
            ErrorBlock.Text = string.Empty;
            ErrorBlock.Visibility = Visibility.Collapsed;
            return false;
        }

        private bool SetError(string error)
        {
            RegBtn.IsEnabled = false;
            ErrorBlock.Visibility = Visibility.Visible;
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
