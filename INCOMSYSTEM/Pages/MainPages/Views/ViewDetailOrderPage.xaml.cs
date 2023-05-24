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

            _file = order.HistoryUploaded;

            SetInputBoxValues(order);
            SetFileValues(order);

            if (order.idStatus >= 4 || order.factDateStart != null || MainWindow.AuthUser.idPos == 2)
            {
                ChangeExecutorBox();
                DisableBoxes();
                return;
            }
            
            using (var db = new INCOMSYSTEMEntities())
            {
                ExecutorBox.ItemsSource = db.Employees.SelectMany(s => s.SpecializationsEmployee)
                    .Include(s => s.Employees.UsersDetail)
                    .Where(s => s.Employees.UsersDetail.idPos == 2 &&
                                s.idSpecialization == order.Tasks.idSpecialization).ToList();
                if (order.idExecutor != null)
                    ExecutorBox.SelectedItem = ExecutorBox.ItemsSource.Cast<SpecializationsEmployee>()
                        .First(s => s.Employees.idUser == order.idExecutor);
            }
        }

        private void ChangeExecutorBox()
        {
            ExecutorBox.Visibility = Visibility.Collapsed;
            ExecutorBlock.Visibility = Visibility.Visible;
        }

        private void DisableBoxes()
        {
            ExecutorBox.Visibility = Visibility.Collapsed;
            ExecutorBlock.Visibility = Visibility.Visible;
            DifficultyBox.IsReadOnly = true;
            FileDownloadGrid.AllowDrop = false;
            UploadBtn.IsEnabled = false;
            ClearBtn.IsEnabled = false;
            SaveBtn.Visibility = Visibility.Collapsed;
            CancelBtn.Content = "Закрыть";
        }

        private void SetInputBoxValues(Orders order)
        {
            CustomerBlock.Text = $"{order.Customers.name}";
            PriceBox.Text = $"{order.price.ToString("#,#", new CultureInfo("ru-RU"))} руб.";
            DifficultyBox.Text = order.difficulty.ToString(CultureInfo.InvariantCulture);
            if (order.Chats.Employees != null)
            {
                ManagerBlock.Text = $"{order.Chats.Employees.surname} {order.Chats.Employees.name}";
                ManagerBlock.Text += order.Chats.Employees.patronymic != null
                    ? $" {order.Chats.Employees.patronymic}"
                    : "";
            }

            if (order.idStatus >= 3)
            {
                ExecutorBlock.Text = $"{order.Employees.surname} {order.Employees.name}";
                ExecutorBlock.Text += order.Employees.patronymic != null
                    ? $" {order.Employees.patronymic}"
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
            StatusOrderBlock.Text = order.Statuses.name;
        }

        private void SetFileValues(Orders order)
        {
            if (_file == null)
            {
                FileDownload.IsEnabled = false;
                ReturnBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                FileDownload.Content = $"Дополнение к договору.{order.HistoryUploaded.fileExtension}";
                FileDownload.IsEnabled = true;
                TempFile = _file;
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

        private readonly HistoryUploaded _file;

        private HistoryUploaded TempFile { get; set; }

        private void UploadBtn_Click(object sender, RoutedEventArgs e)
        {
            var openFile = new OpenFileDialog
            {
                Title = "Загрузка файла",
                Filter = "Document | *.docx; *.doc | Portable Document | *.pdf",
                DefaultExt = ".docx"
            };
            if (openFile.ShowDialog() != true) return;

            var file = new HistoryUploaded
            {
                fileName = $"Дополнение к договору.{openFile.SafeFileName.Split('.').Last()}",
                fileContent = File.ReadAllBytes(openFile.FileName),
                fileExtension = openFile.SafeFileName.Split('.').Last(),
                fileSize = new FileInfo(openFile.FileName).Length
            };
            TempFile = file;
            ClearBtn.IsEnabled = true;
            ReturnBtn.IsEnabled = true;
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            TempFile = null;
            FileName = string.Empty;
            ClearBtn.IsEnabled = false;
            ReturnBtn.IsEnabled = true;
        }

        private void ReturnBtn_Click(object sender, RoutedEventArgs e)
        {
            TempFile = _file;
            FileName = $"Дополнение к договору.{_file.fileExtension}";
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
                Filter = $"File | * {TempFile.fileExtension}",
                DefaultExt = $".{TempFile.fileExtension}"
            };
            if (saveFileDialog.ShowDialog() != true) return;

            using (var file = new FileStream(saveFileDialog.FileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                file.Write(TempFile.fileContent, 0, TempFile.fileContent.Length);
            }
        }
        private void FileUpload_OnDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var openFile = ((string[])e.Data.GetData(DataFormats.FileDrop))?[0];
            if (openFile == null) return;
            var fileExtension = openFile.Split('.').Last();
            if (fileExtension != "docx" && fileExtension != "doc" && fileExtension != "pdf")
            {
                AdditionalWindow.ShowError("Не верный формат файла");
                return;
            }
            var file = new HistoryUploaded
            {
                fileName = $"Дополнение к договору.{fileExtension}",
                fileContent = File.ReadAllBytes(openFile),
                fileExtension = fileExtension,
                fileSize = new FileInfo(openFile).Length
            };
            TempFile = file;
            ClearBtn.IsEnabled = true;
            ReturnBtn.IsEnabled = true;
            FileName = $"Дополнение к договору.{fileExtension}";
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
            PriceBox.Text = Math.Round(price).ToString("#,#", new CultureInfo("ru-RU")) + " руб.";
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
                    order.HistoryUploaded = TempFile;
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
            if (_order.idStatus >= 3) return true;
            return ((SpecializationsEmployee)ExecutorBox.SelectedItem)?.idEmployee == _order.idExecutor
                        && DifficultyBox.Text == _order.difficulty.ToString(CultureInfo.InvariantCulture)
                        && PlanDateStartBox.SelectedDate == _order.planDateStart
                        && PlanDateCompleteBox.SelectedDate == _order.planDateComplete
                        && (_file.fileContent == TempFile.fileContent
                            && _file.fileExtension == TempFile.fileExtension
                            && _file.fileName == TempFile.fileName
                            && _file.fileSize == TempFile.fileSize);
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