using System.Linq;
using System.Windows.Controls;
using INCOMSYSTEM.Context;
using System.Data.Entity;
using System.Globalization;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using System;
using INCOMSYSTEM.Windows;

namespace INCOMSYSTEM.Pages.MainPages.Views
{
    public partial class ViewDetailOrderPage : Page
    {
        public ViewDetailOrderPage(Orders order)
        {
            _order = order;
            InitializeComponent();

            Title = $"Подробности заказа №{order.id}";

            _fileOrder = order.attachment;
            _fileOrderExtension = order.fileExtension;
            SetInputBoxValues(order);
            SetFileValues(order);

            using(var db = new INCOMSYSTEMEntities())
            {
                ExecutorBox.ItemsSource = db.Employees.SelectMany(s => s.SpecializationsEmployee)
                    .Include(s => s.Employees.UsersDetail)
                    .Where(s => s.Employees.UsersDetail.idPos == 2 && s.idSpecialization == order.Tasks.idSpecialization).ToList();
                if (order.idExecutor != null) ExecutorBox.SelectedItem = ExecutorBox.ItemsSource.Cast<SpecializationsEmployee>().First(s => s.Employees.idUser == order.idExecutor);
            }
        }

        private void SetInputBoxValues(Orders order)
        {
            CustomerBlock.Text = $"{order.Customers.name}";
            PriceBox.Text = $"{order.price:0.00 руб.}";
            DifficultyBox.Text = order.difficulty.ToString(CultureInfo.InvariantCulture);
            if (order.Chats.Employees != null)
            {
                ManagerBlock.Text = $"{order.Chats.Employees.surname} {order.Chats.Employees.name}";
                ManagerBlock.Text += order.Chats.Employees.patronymic != null
                    ? $" {order.Chats.Employees.patronymic}"
                    : "";
            }
            if(order.factDateStart != null)
            {
                PlanDateStartBox.IsHitTestVisible = false;
                PlanDateCompleteBox.IsHitTestVisible = false;
            }
            PlanDateStartBox.SelectedDate = order.planDateStart;
            PlanDateCompleteBox.SelectedDate = order.planDateComplete;
            DateOrderBlock.Text = order.dateOrder.ToString("dd MMMM yyyy", CultureInfo.CurrentCulture);
        }

        private void SetFileValues(Orders order)
        {
            if (_fileOrder == null)
            {
                FileDownload.IsEnabled = false;
                ReturnBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                FileDownload.Content = $"Дополнение к договору.{order.fileExtension}";
                FileDownload.IsEnabled = true;
                TempFile = _fileOrder;
                TempFileExtension = _fileOrderExtension;
                ClearBtn.IsEnabled = true;
            }
        }

        private readonly Orders _order;
        private string _fileName = "Дополнение к договору";
        private string FileName
        {
            get => _fileName;
            set
            {
                FileDownload.IsEnabled = !string.IsNullOrWhiteSpace(value);
                _fileName = value;
                FileDownload.Content = value;
            }
        }

        private readonly byte[] _fileOrder;
        private readonly string _fileOrderExtension;

        private byte[] TempFile { get; set; }
        private string TempFileExtension { get; set; }

        private void UploadBtn_Click(object sender, RoutedEventArgs e)
        {
            var openFile = new OpenFileDialog
            {
                Title = "Загрузка файла",
                Filter = "Document | *.docx; *.doc | Portable Document | *.pdf",
                DefaultExt = ".docx"
            };
            if (openFile.ShowDialog() != true) return;

            TempFile = File.ReadAllBytes(openFile.FileName);
            TempFileExtension = openFile.SafeFileName.Split('.').Last();
            FileName = $"Дополнение к договору.{TempFileExtension}";
            ClearBtn.IsEnabled = true;
            ReturnBtn.IsEnabled = true;
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            TempFile = null;
            TempFileExtension = null;
            FileName = string.Empty;
            ClearBtn.IsEnabled = false;
            ReturnBtn.IsEnabled = true;
        }

        private void ReturnBtn_Click(object sender, RoutedEventArgs e)
        {
            TempFile = _fileOrder;
            TempFileExtension = _fileOrderExtension;
            FileName = $"Дополнение к договору.{TempFileExtension}";
            ReturnBtn.IsEnabled = false;
            ClearBtn.IsEnabled = true;
        }

        private void FileDownloadBtn_Click(object sender, RoutedEventArgs e)
        {
            if (TempFile == null) return;

            var saveFileDialog = new SaveFileDialog
            {
                Title = "Скачивание файла",
                FileName = $"{FileName}",
                Filter = $"File | * {TempFileExtension}",
                DefaultExt = $".{TempFileExtension}"
            };
            if (saveFileDialog.ShowDialog() != true) return;

            using (var file = new FileStream(saveFileDialog.FileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                file.Write(TempFile, 0, TempFile.Length);
            }
        }
        private void FileDownload_OnDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var file = ((string[])e.Data.GetData(DataFormats.FileDrop))?[0];
            if (file == null) return;
            var fileExtension = file.Split('.').Last();
            if (fileExtension != "docx" && fileExtension != "doc" && fileExtension != "pdf")
            {
                AdditionalWindow.ShowError("Не верный формат файла");
                return;
            }
            TempFile = File.ReadAllBytes(file);
            TempFileExtension = fileExtension;
            ClearBtn.IsEnabled = true;
            ReturnBtn.IsEnabled = true;
            FileName = $"Дополнение к договору.{TempFileExtension}";
            AdditionalWindow.HideError();
        }

        private void DifficultyBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!decimal.TryParse(DifficultyBox.Text.Replace(',', '.'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var difficulty))
            {
                AdditionalWindow.ShowError("Не удалось преобразовать коэффициент сложности в строку");
                return;
            }
            var price = _order.Tasks.newPrice * difficulty;
            PriceBox.Text = Math.Round(price).ToString(CultureInfo.InvariantCulture);
            AdditionalWindow.HideError();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(DifficultyBox.Text.Replace(',', '.'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var difficulty))
            {
                AdditionalWindow.ShowError("Не удалось преобразовать коэффициент сложности в строку");
                return;
            }
            if (!IsEquals())
            {
                byte? status = null;
                if (_order.factDateStart != null && _order.factDateComplete != null) status = 4;
                else if ((_order.factDateStart != null && _order.factDateComplete == null)
                            || (PlanDateStartBox.SelectedDate != null && PlanDateCompleteBox.SelectedDate != null)) status = 3;
                using (var db = new INCOMSYSTEMEntities())
                {
                    var order = db.Orders.First(s => s.id == _order.id);
                    order.idExecutor = ((SpecializationsEmployee)ExecutorBox.SelectedItem)?.idEmployee;
                    order.price = Math.Round(_order.Tasks.newPrice * difficulty);
                    order.difficulty = (double)difficulty;
                    order.planDateStart = PlanDateStartBox.SelectedDate;
                    order.planDateComplete = PlanDateCompleteBox.SelectedDate;
                    if (status != null) order.idStatus = status.Value;
                    order.attachment = TempFile;
                    order.fileExtension = TempFileExtension;
                    db.SaveChanges();
                }
            }

            AdditionalWindow.HideError();
            var addWindow = Application.Current.Windows.OfType<AdditionalWindow>().First();
            addWindow.DialogResult = true;
            addWindow.Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!IsEquals()
                && MessageBox.Show("Отменить внесённые изменения?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) return;

            var addWindow = Application.Current.Windows.OfType<AdditionalWindow>().First();
            addWindow.DialogResult = false;
            addWindow.Close();
        }

        private bool IsEquals()
        {
            return ((SpecializationsEmployee)ExecutorBox.SelectedItem)?.idEmployee == _order.idExecutor
                        && DifficultyBox.Text == _order.difficulty.ToString(CultureInfo.InvariantCulture)
                        && PlanDateStartBox.SelectedDate == _order.planDateStart
                        && PlanDateCompleteBox.SelectedDate == _order.planDateComplete
                        && (_fileOrder == TempFile && _fileOrderExtension == TempFileExtension);
        }

        private void DateBox_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            PlanDateStartBox.IsWrong = PlanDateStartBox.SelectedDate != null && PlanDateCompleteBox.SelectedDate != null
                                       && PlanDateStartBox.SelectedDate.Value > PlanDateCompleteBox.SelectedDate.Value;
            
            if(PlanDateStartBox.IsWrong) AdditionalWindow.ShowError("Плановая дата начала не может быть больше плановой даты завершения");
            else AdditionalWindow.HideError();
        }
    }
}