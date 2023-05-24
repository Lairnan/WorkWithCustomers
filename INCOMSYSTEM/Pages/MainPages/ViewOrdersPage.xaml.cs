using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using INCOMSYSTEM.BehaviorsFiles;
using INCOMSYSTEM.Context;
using INCOMSYSTEM.Pages.MainPages.Views;
using INCOMSYSTEM.Windows;
using Microsoft.Win32;
using Word = Microsoft.Office.Interop.Word;

namespace INCOMSYSTEM.Pages.MainPages
{
    public partial class ViewOrdersPage : Page
    {
        public ViewOrdersPage()
        {
            InitializeComponent();
            using (var db = new INCOMSYSTEMEntities())
            {
                AllItemsCount.Text = db.Orders
                    .Count(s => MainWindow.AuthUser.idPos != 2 || s.idExecutor == MainWindow.AuthUser.idUser)
                    .ToString();
            }

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            using (var db = new INCOMSYSTEMEntities())
            {
                OrdersList.ItemsSource = db.Orders
                    .Include(s => s.Customers)
                    .Include(s => s.Employees)
                    .Include(s => s.Employees.UsersDetail)
                    .Include(s => s.Customers.UsersDetail)
                    .Include(s => s.Statuses)
                    .Include(s => s.Tasks)
                    .Include(s => s.Chats)
                    .Include(s => s.Chats.Employees)
                    .Include(s => s.Tasks.HistoryUploaded)
                    .Include(s => s.HistoryUploaded)
                    .Where(s => MainWindow.AuthUser.idPos != 2 || s.idExecutor == MainWindow.AuthUser.idUser)
                    .ToList();
            }
        }

        private void ViewOrderMenu_Click(object sender, RoutedEventArgs e)
        {
            var order = ((MenuItem)sender).CommandParameter as Orders;
            var addWindow = new AdditionalWindow();

            addWindow.MFrame.Navigate(new ViewDetailOrderPage(order));

            if (addWindow.ShowDialog() != true) return;
            ApplyFilter();
        }

        private void OpenChatOrderMenu_Click(object sender, RoutedEventArgs e)
        {
            var order = ((MenuItem)sender).CommandParameter as Orders;

            if (order.Chats.idManager == null &&
                MessageBox.Show("Перейти к чату?", "Подтверждение", MessageBoxButton.YesNo) !=
                MessageBoxResult.Yes) return;

            using (var db = new INCOMSYSTEMEntities())
            {
                var chat = db.Chats.Include(s => s.Customers).First(s => s.idChat == order.id);
                var orderDb = db.Orders.First(s => s.id == order.id);
                if(orderDb.idStatus < 2) orderDb.idStatus = 2;
                chat.idManager = MainWindow.AuthUser.idUser;
                db.SaveChanges();
                Application.Current.Windows.OfType<MainWindow>().First().GoChat(chat);
            }
            
            ApplyFilter();
        }

        private void FormAgreementOrderMenu_Click(object sender, RoutedEventArgs e)
        {
            var order = ((MenuItem)sender).CommandParameter as Orders;
            var saveFileDialog = new SaveFileDialog
            {
                FileName = $"Договор №{order.id}",
                Filter = "Documents | *.docx, *.doc",
                DefaultExt = ".docx",
                OverwritePrompt = true
            };
            if (saveFileDialog.ShowDialog() != true) return;

            if (!FormAgreement(saveFileDialog.FileName, order)) return;

            if (MessageBox.Show("Договор успешно сохранён. Открыть?", "Успешное сохранение", MessageBoxButton.YesNo,
                    MessageBoxImage.Question)
                != MessageBoxResult.Yes) return;

            Process.Start(saveFileDialog.FileName);
        }

        private static bool FormAgreement(string path, Orders order)
        {
            var wApp = new Word.Application();
            var doc = wApp.Documents.Open(
                Directory.GetCurrentDirectory() + @"\Resources\AgreementTemplate.docx");
            while (true)
            {
                try
                {
                    doc.ReplaceWordText("{dayOrder}", order.dateOrder.ToString("dd"));
                    doc.ReplaceWordText("{monthOrder}", order.dateOrder.ToString("MMMM", CultureInfo.CurrentCulture));
                    doc.ReplaceWordText("{yearOrder}", order.dateOrder.ToString("yy"));

                    doc.ReplaceWordText("{customerName}", order.Customers.name);
                    var employeePatronymic = order.Employees.patronymic != null
                        ? $" {order.Employees.patronymic}"
                        : string.Empty;
                    doc.ReplaceWordText("{executorName}",
                        $"{order.Employees.surname} {order.Employees.name}{employeePatronymic}");

                    doc.ReplaceWordText("{passportCustomer}", order.Customers.UsersDetail.passport);
                    doc.ReplaceWordText("{passportExecutor}", order.Employees.UsersDetail.passport);

                    doc.ReplaceWordText("{taskName}", order.Tasks.name);

                    doc.ReplaceWordText("{planDayStart}", order.planDateStart.Value.ToString("dd"));
                    doc.ReplaceWordText("{planMonthStart}",
                        order.planDateStart.Value.ToString("MMMM", CultureInfo.CurrentCulture));
                    doc.ReplaceWordText("{planYearStart}", order.planDateStart.Value.ToString("yy"));

                    doc.ReplaceWordText("{planDayComplete}", order.planDateComplete.Value.ToString("dd"));
                    doc.ReplaceWordText("{planMonthComplete}",
                        order.planDateComplete.Value.ToString("MMMM", CultureInfo.CurrentCulture));
                    doc.ReplaceWordText("{planYearComplete}", order.planDateComplete.Value.ToString("yy"));

                    doc.ReplaceWordText("{supportPeriod}", order.Tasks.supportPeriod);
                    doc.ReplaceWordText("{taskPrice}", (int)order.Tasks.newPrice);
                    doc.ReplaceWordText("{taskPriceInWords}", ((int)order.Tasks.newPrice).GetNumberInWords().Trim());

                    var taskDifficultyPrice = order.Tasks.newPrice * (decimal)order.difficulty;
                    doc.ReplaceWordText("{taskDifficultyPrice}", (int)taskDifficultyPrice);
                    doc.ReplaceWordText("{taskDifficultyPriceInWords}",
                        ((int)taskDifficultyPrice).GetNumberInWords().Trim());

                    doc.ReplaceWordText("{orderPrice}", (int)order.price);
                    doc.ReplaceWordText("{orderPriceInWords}", ((int)order.price).GetNumberInWords().Trim());

                    doc.ReplaceWordText("{customerPhone}", order.Customers.UsersDetail.phone ?? "Отсутствует");
                    doc.ReplaceWordText("{executorPhone}", order.Employees.UsersDetail.phone ?? "Отсутствует");

                    doc.ReplaceWordText("{customerSeriePassport}", order.Customers.UsersDetail.SeriePassport);
                    doc.ReplaceWordText("{customerNumberPassport}", order.Customers.UsersDetail.NumberPassport);
                    doc.ReplaceWordText("{executorSeriePassport}", order.Employees.UsersDetail.SeriePassport);
                    doc.ReplaceWordText("{executorNumberPassport}", order.Employees.UsersDetail.NumberPassport);

                    doc.ReplaceWordText("{customerAddress}", order.Customers.UsersDetail.address);
                    doc.ReplaceWordText("{executorAddress}", order.Employees.UsersDetail.address);
                    break;
                }
                catch
                {
                    var result = MessageBox.Show("Для сохранения записи, необходимо закрыть Word документ", "Ошибка",
                        MessageBoxButton.YesNo, MessageBoxImage.Error);
                    if (result != MessageBoxResult.No) continue;
                    doc.Close();
                    wApp.Quit();
                    return false;
                }
            }

            doc.SaveAs(path);
            doc.Close();
            wApp.Quit();
            return true;
        }

        private void SetFactDateStartMenu_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Установить текущую дату начала?", "Подтверждение", MessageBoxButton.YesNo) !=
                MessageBoxResult.Yes) return;

            var order = ((MenuItem)sender).CommandParameter as Orders;

            using (var db = new INCOMSYSTEMEntities())
            {
                var orderDb = db.Orders.First(s => s.id == order.id);
                orderDb.factDateStart = DateTime.Now;
                orderDb.idStatus = 3;
                db.SaveChanges();
            }

            ApplyFilter();
        }

        private void SetFactDateCompleteMenu_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Установить текущую дату завершения?", "Подтверждение", MessageBoxButton.YesNo) !=
                MessageBoxResult.Yes) return;
            var order = ((MenuItem)sender).CommandParameter as Orders;

            using (var db = new INCOMSYSTEMEntities())
            {
                var orderDb = db.Orders.First(s => s.id == order.id);
                orderDb.factDateComplete = DateTime.Now;
                orderDb.idStatus = 4;
                db.SaveChanges();
            }

            ApplyFilter();
        }
    }
}