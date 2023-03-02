using INCOMSYSTEM.Context;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using INCOMSYSTEM.Windows;

namespace INCOMSYSTEM.Pages.MainPages.Views
{
    /// <summary>
    /// Логика взаимодействия для ViewDetail.xaml
    /// </summary>
    public partial class ViewDetailTaskPage : Page
    {
        public ViewDetailTaskPage(Tasks task)
        {
            InitializeComponent();

            discountStyle.IsChecked = task.discoutStyle;
            discountVisible.IsChecked = task.discountVisible;
            TitleTask.Text = task.name;
            SpecTask.Text = $"{task.Specializations.name}";
            DescriptionTask.Text = task.description;
            PriceTask.Text = $"Цена: {task.price:0.00}";
            DiscountTask.Text = $"Скидка: {task.discount}%";
            NewPriceTask.Text = $"Новая цена: {task.newPrice:0.00}";
            ApproxCompleteTime.Text = $"Примерное время выполнения {task.approxCompleteTime} дней";
            AttachmentBlock.Visibility = task.attachment == null ? Visibility.Collapsed : Visibility.Visible;
            Task = task;
        }

        private readonly Tasks Task;

        private void DownloadAttachmentBtn_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Title = "Скачивание файла",
                CreatePrompt = true,
                FileName = $"{Task.name}.{Task.fileExtension}",
                Filter = $"File | *.{Task.fileExtension}"
            };
            if (saveFileDialog.ShowDialog() != true) return;

            using(var file = new FileStream(saveFileDialog.FileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                file.Write(Task.attachment, 0, Task.attachment.Length);
                file.Dispose();
            }
        }

        private void MakeOrderBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены с заказом?", "Подтверждение", MessageBoxButton.YesNo,
                    MessageBoxImage.Information) != MessageBoxResult.Yes) return;
            
            var order = new Orders
            {
                idCustomer = MainWindow.AuthUser.idUser,
                idTask = Task.id,
                difficulty = 1f,
                dateOrder = DateTime.Now,
                idStatus = 1
            };
            order.price = Task.price * (decimal)order.difficulty;
            var chat = new Chats
            {
                idChat = order.id,
                idCustomer = MainWindow.AuthUser.idUser,
                dateCreate = DateTime.Now
            };
            using (var db = new INCOMSYSTEMEntities())
            {
                db.Orders.Add(order);
                db.Chats.Add(chat);
                db.SaveChanges();
            }

            MessageBox.Show("Заказ успешно сформирован!");
            
            MainWindow.ReviewFrame.GoBack();
        }
    }
}
