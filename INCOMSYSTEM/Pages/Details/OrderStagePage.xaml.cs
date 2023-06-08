using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using INCOMSYSTEM.BehaviorsFiles;
using INCOMSYSTEM.Context;
using INCOMSYSTEM.Windows;
using Microsoft.Win32;

namespace INCOMSYSTEM.Pages.Details
{
    public partial class OrderStagePage : Page
    {
        private string _result;
        
        public OrderStagePage(Orders order)
        {
            _order = order;
            InitializeComponent();

            Title = "Добавление этапа";

            using (var db = new INCOMSYSTEMEntities())
            {
                TypeStagesBox.ItemsSource = db.TypesStage.OrderBy(s => s.id).ToList();
                var taskStages = db.TaskStages.Where(s => s.idTask == order.idTask).ToList();
                var orderStages = db.OrderStages.Where(s => s.idOrder == order.id && s.idTaskStage != null)
                    .Select(s => s.idTaskStage)
                    .ToList();
                TaskStageBox.ItemsSource = taskStages.Where(s => !orderStages.Contains(s.id)).ToList();
            }
            
            ReturnBtn.Visibility = Visibility.Collapsed;
        }

        private DateTime? _factDateStart;
        private DateTime? FactDateStart
        {
            get => _factDateStart;
            set
            {
                _factDateStart = value;
                DateStart.Text = value.HasValue ? value.Value.ToString("dd.MM.yyyy") : "Ожидание";
                DateStartBtn.IsEnabled = !value.HasValue;
                DateEndBtn.IsEnabled = value.HasValue;
            }
        }

        private DateTime? _factDateEnd;
        private DateTime? FactDateEnd
        {
            get => _factDateEnd;
            set
            {
                _factDateEnd = value;
                DateEnd.Text = value.HasValue ? value.Value.ToString("dd.MM.yyyy") : "Ожидание";
                DateEndBtn.IsEnabled = !value.HasValue && FactDateStart.HasValue;
            }
        }
        
        public OrderStagePage(OrderStages orderStage)
        {
            _orderStage = orderStage;
            InitializeComponent();

            Title = "Редактирование этапа";

            TypeStagesBlock.Visibility = Visibility.Visible;
            TypeStagesBox.Visibility = Visibility.Collapsed;
            NameStageBox.Visibility = Visibility.Visible;
            TaskStageBox.Visibility = Visibility.Collapsed;
            TaskStageBox.IsEnabled = false;
            
            TypeStagesBlock.Text = _orderStage.TypesStage.name;
            NameStageBox.Text = _orderStage.idType == 2 ? orderStage.name : orderStage.TaskStages.name;
            NameStageBox.IsEnabled = _orderStage.idType == 2;

            FactDateStart = orderStage.factDateStart;
            DateStartBtn.IsEnabled = false;
            FactDateEnd = orderStage.factDateComplete;

            // DateStart.SelectedDate = orderStage.factDateStart;
            // DateStart.IsEnabled = false;
            // DateEnd.SelectedDate = orderStage.factDateComplete;
            // DateEnd.IsEnabled = orderStage.factDateComplete == null;

            _result = orderStage.description;

            _file = orderStage.HistoryUploaded;
            TempFile = _file;

            if (orderStage.Orders.idStatus >= 4) FileButtonsPanel.Visibility = Visibility.Collapsed;
            else FileUploadGrid.Drop += FileUpload_OnDrop;

            ReturnBtn.Visibility = _file != null ? Visibility.Visible : Visibility.Collapsed;
        }
        
        private readonly OrderStages _orderStage;
        private readonly Orders _order;

        private readonly HistoryUploaded _file;


        private HistoryUploaded _tempFile;
        private HistoryUploaded TempFile
        {
            get => _tempFile;
            set
            {
                FileDownload.IsEnabled = value != null;
                _tempFile = value;
                ClearBtn.IsEnabled = value != null;
                FileDownload.Content = value == null ? string.Empty : $"{value.fileName}.{value.fileExtension}";
                FileSizeNameBlock.Text = value == null ? string.Empty : value.fileSize.ConvertFileSizeToString();
            }
        }

        private void UploadBtn_Click(object sender, RoutedEventArgs e)
        {
            var openFile = new OpenFileDialog
            {
                Title = "Загрузка файла",
                Filter = "Документы (*.doc, *.docx)|*.doc;*.docx" +
                         "|Архивы (*.zip, *.rar, *.7zip)|*.zip;*.rar;*.7zip" +
                         "|PDF файлы (*.pdf)|*.pdf" +
                         "|Изображения (*.jpg, *.jpeg, *.png, *.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
                DefaultExt = "docx"
            };
            if (openFile.ShowDialog() != true) return;

            var name = openFile.SafeFileName;
            var ext = name.Split('.').Last();
            name = name.Remove(name.IndexOf(ext, StringComparison.Ordinal) - 1, ext.Length + 1);

            var file = new HistoryUploaded
            {
                fileName = name,
                fileContent = File.ReadAllBytes(openFile.FileName),
                fileExtension = ext,
                fileSize = new FileInfo(openFile.FileName).Length,
                uploadedBy = MainWindow.AuthUser.idUser
            };
            TempFile = file;
            
            ClearBtn.IsEnabled = true;
            ReturnBtn.IsEnabled = true;
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            TempFile = null;
            ClearBtn.IsEnabled = false;
            ReturnBtn.IsEnabled = _file != null;
        }

        private void ReturnBtn_Click(object sender, RoutedEventArgs e)
        {
            TempFile = _file;
            ReturnBtn.IsEnabled = false;
            ClearBtn.IsEnabled = true;
        }

        private void FileDownloadBtn_Click(object sender, RoutedEventArgs e)
        {
            if (TempFile == null) return;
            
            var saveFileDialog = new SaveFileDialog
            {
                Title = "Скачивание файла",
                OverwritePrompt = true,
                FileName = $"{TempFile.fileName}.{TempFile.fileExtension}",
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
            if (fileExtension != "zip" && fileExtension != "rar" && fileExtension != "7zip"
                                       && fileExtension != "doc" && fileExtension != "docx"
                                       && fileExtension != "pdf" && fileExtension != "jpg"
                                       && fileExtension != "jpeg" && fileExtension != "png"
                                       && fileExtension != "bmp")
            {
                AdditionalWindow.ShowError("Не верный формат файла");
                return;
            }

            var fileName = openFile.Split('\\').Last();
            var file = new HistoryUploaded
            {
                fileName = fileName.Remove(fileName.IndexOf(fileExtension, StringComparison.Ordinal) - 1, fileExtension.Length + 1),
                fileContent = File.ReadAllBytes(openFile),
                fileExtension = fileExtension,
                fileSize = new FileInfo(openFile).Length,
                uploadedBy = MainWindow.AuthUser.idUser
            };
            TempFile = file;
            
            ClearBtn.IsEnabled = true;
            ReturnBtn.IsEnabled = true;
            AdditionalWindow.HideError();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            switch (_order != null)
            {
                case true when !AddOrderStage():
                case false when !SaveOrderStage():
                    return;
            }
            
            var wnd = Window.GetWindow(this);
            wnd.DialogResult = true;
            wnd.Close();
        }

        private bool SaveOrderStage()
        {
            if (IsNullOrWhiteSpace())
            {
                AdditionalWindow.ShowError("Поля не могут быть пустыми");
                return false;
            }

            using (var db = new INCOMSYSTEMEntities())
            {
                var orderStage = db.OrderStages.First(s => s.id == _orderStage.id);
                
                orderStage.description = _result;
                orderStage.factDateComplete = FactDateEnd;

                UpdateFileOrderStage(ref orderStage, db);
                db.SaveChanges();
                
                SetFactDateAndCompleteOrder(db, orderStage, _orderStage.Orders);
                db.SaveChanges();
                MessageBox.Show("Данные успешно сохранены");
            }

            return true;
        }

        private void UpdateFileOrderStage(ref OrderStages orderStage, INCOMSYSTEMEntities db)
        {
            if (orderStage.HistoryUploaded == null && TempFile != null)
            {
                db.HistoryUploaded.Add(TempFile);
                orderStage.idFile = TempFile.id;
            }
            else if (TempFile != null && (_file.fileContent != TempFile.fileContent
                                          || _file.fileExtension != TempFile.fileExtension
                                          || _file.fileName != TempFile.fileName
                                          || _file.fileSize != TempFile.fileSize))
            {
                var file = db.HistoryUploaded.First(s => s.id == _file.id);
                file.fileName = TempFile.fileName;
                file.fileContent = TempFile.fileContent;
                file.fileExtension = TempFile.fileExtension;
                file.fileSize = TempFile.fileSize;
                file.uploadedBy = TempFile.uploadedBy;
            }
        }

        private bool AddOrderStage()
        {
            if (!CheckFields()) return false;

            using (var db = new INCOMSYSTEMEntities())
            {
                var idFile = GetIdFile(db);

                var orderStage = AddOrderStage(idFile, db);
                db.SaveChanges();

                SetFactDateAndCompleteOrder(db, orderStage, _order);
                db.SaveChanges();
                MessageBox.Show("Этап успешно добавлен");
            }

            return true;
        }

        private void SetFactDateAndCompleteOrder(INCOMSYSTEMEntities db, OrderStages orderStage, Orders order)
        {
            var orderStages = db.OrderStages.Where(s => s.idOrder == order.id);
            var orderDb = db.Orders.First(s => s.id == order.id);

            if (orderDb.idStatus == 4 && orderStage.idType == 2)
            {
                orderDb.idStatus = (byte)(orderStage.factDateComplete.HasValue ? 4 : 3);
                orderDb.factDateComplete = orderStage.factDateComplete;
                return;
            }
            
            if (!orderDb.factDateStart.HasValue)
                orderDb.factDateStart = orderStages.First().factDateStart;

            if (!(orderStages.All(s => s.factDateComplete.HasValue)
                  || !db.TaskStages.Where(s => s.idTask == orderDb.idTask)
                      .All(s => orderStages.Select(x => x.idTaskStage).Contains(s.id)))) return;
            
            orderDb.idStatus = 4;
            orderDb.factDateComplete = orderStage.factDateComplete;
        }

        private OrderStages AddOrderStage(long? idFile, INCOMSYSTEMEntities db)
        {
            var orderStage = new OrderStages
            {
                idOrder = _order.id,
                idType = ((TypesStage)TypeStagesBox.SelectedItem).id,
                idFile = idFile,
                factDateStart = FactDateStart,
                factDateComplete = FactDateEnd,
                description = _result
            };
            switch (orderStage.idType)
            {
                case 1:
                    orderStage.idTaskStage = ((TaskStages)TaskStageBox.SelectedItem).id;
                    break;
                case 2:
                    orderStage.name = NameStageBox.Value;
                    break;
            }

            db.OrderStages.Add(orderStage);
            return orderStage;
        }

        private long? GetIdFile(INCOMSYSTEMEntities db)
        {
            long? idFile = null;
            if (TempFile == null) return idFile;
            var file = new HistoryUploaded
            {
                fileName = TempFile.fileName,
                fileContent = TempFile.fileContent,
                fileExtension = TempFile.fileExtension,
                fileSize = TempFile.fileSize,
                uploadedBy = TempFile.uploadedBy
            };
            db.HistoryUploaded.Add(file);
            idFile = file.id;

            return idFile;
        }

        private bool CheckFields()
        {
            if (TypeStagesBox.SelectedIndex == 0 && TaskStageBox.SelectedItem == null)
            {
                MessageBox.Show("Плановые этапы уже завершены", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            if (IsNullOrWhiteSpace() || TypeStagesBox.SelectedIndex == -1)
            {
                MessageBox.Show("Поля не могут быть пустыми");
                return false;
            }

            return true;
        }

        private bool IsNullOrWhiteSpace()
        {
            if(_order != null) return (TypeStagesBox.Visibility == Visibility.Collapsed && NameStageBox.IsWhiteSpace) || FactDateStart == null;
            return false;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var wnd = Window.GetWindow(this);
            wnd.DialogResult = false;
            wnd.Close();
        }

        private void AddResulStageButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AdditionalWindow();
            var resultPage = _result == null ? new ResultStagePage() : new ResultStagePage(_result);
            addWindow.MFrame.Navigate(resultPage);
            if (addWindow.ShowDialog() != true) return;
            
            _result = resultPage.ResultStage;
        }

        private void TypeStagesBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (TypeStagesBox.SelectedIndex)
            {
                case 0:
                    NameStageBox.Visibility = Visibility.Collapsed;
                    TaskStageBox.Visibility = Visibility.Visible;
                    WarningBlock.Visibility = Visibility.Collapsed;
                    break;
                case 1:
                    NameStageBox.Visibility = Visibility.Visible;
                    TaskStageBox.Visibility = Visibility.Collapsed;
                    WarningBlock.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void SetFactDateStartButton_Click(object sender, RoutedEventArgs e)
        {
            FactDateStart = DateTime.Now;
        }

        private void SetFactDateEndButton_Click(object sender, RoutedEventArgs e)
        {
            FactDateEnd = DateTime.Now;
        }

        private void ClearDateBtn_Click(object sender, RoutedEventArgs e)
        {
            var param = int.Parse((string)((Button)sender).CommandParameter);
            switch (param)
            {
                case 1:
                    FactDateStart = null;
                    break;
                case 2:
                    FactDateEnd = null;
                    break;
            }
        }
    }
}