using INCOMSYSTEM.Context;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        }
    }
}
