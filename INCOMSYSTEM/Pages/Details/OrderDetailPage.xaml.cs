using System;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using INCOMSYSTEM.BehaviorsFiles;
using INCOMSYSTEM.Context;
using INCOMSYSTEM.Windows;
using Microsoft.Win32;
using Word = Microsoft.Office.Interop.Word;

namespace INCOMSYSTEM.Pages.Details
{
    public partial class OrderDetailPage : Page
    {
        public OrderDetailPage(Orders order)
        {
            _order = order;
            InitializeComponent();

            Title = $"Подробности заказа №{order.id}";

            _file = order.HistoryUploaded;
            
            RefreshOrderStages();
            InitTextBlock();

            SetFileValues();
        }

        private void RefreshOrderStages()
        {
            using (var db = new INCOMSYSTEMEntities())
            {
                var orderStages = db.OrderStages
                    .Include(s => s.TypesStage)
                    .Include(s => s.HistoryUploaded)
                    .Include(s => s.Orders)
                    .Include(s => s.Orders.Employees)
                    .Include(s => s.Orders.Customers)
                    .Include(s => s.Orders.Tasks)
                    .Include(s => s.TaskStages)
                    .Where(s => s.idOrder == _order.id)
                    .ToList();
                OrderStagesList.ItemsSource = orderStages;
                WarningBlock.Visibility = orderStages.Any(s => s.idType == 2) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void InitTextBlock()
        {
            DateOrderBlock.Text = $"Дата заказа: {_order.dateOrder:dd.MM.yyyy}";
            
            SetPositionBlock(_order);
            
            NameTaskBlock.Text = _order.Tasks.name;

            using (var db = new INCOMSYSTEMEntities())
            {
                var order = db.Orders.Include(s => s.Statuses).First(s => s.id == _order.id);
                PlanDateStartBlock.Text = "Плановая дата начала выполнения: " + (order.planDateStart == null
                    ? "ожидание" : order.planDateStart.Value.ToString("dd.MM.yyyy"));
                FactDateStartBlock.Text = "Фактическая дата начала выполнения: " + (order.factDateStart == null
                    ? "ожидание" : order.factDateStart.Value.ToString("dd.MM.yyyy"));
                PlanDateEndBlock.Text = "Плановая дата окончания выполнения: " + (order.planDateComplete == null
                    ? "ожидание" : order.planDateComplete.Value.ToString("dd.MM.yyyy"));
                FactDateEndBlock.Text = "Фактическая дата окончания выполнения: " + (order.factDateComplete == null
                    ? "ожидание" : order.factDateComplete.Value.ToString("dd.MM.yyyy"));

                PriceBlock.Text = $"Цена: {order.price:0} рублей";
                DifficultyBlock.Text = $"Сложность: {order.difficulty}x";
                StatusBlock.Text = $"Статус: {order.Statuses.name.ToLower()}";
            }
        }

        private void SetPositionBlock(Orders order)
        {
            switch (MainWindow.AuthUser.idPos)
            {
                case 1:
                    ExecutorBlock.Text = "Исполнитель: " + (order.idExecutor == null ? "ожидание" : order.Employees.ToString());
                    ManagerBlock.Text = "Менеджер: " + (order.Chats.idManager == null ? "ожидание" : order.Chats.Employees.ToString());
                    FileGridBlock.AllowDrop = true;
                    FilePanel.Visibility = Visibility.Visible;
                    SavePanel.Visibility = Visibility.Visible;
                    break;
                case 2:
                    ExecutorBlock.Text = $"Заказчик: {order.Customers.name}";
                    ManagerBlock.Text = "Менеджер: " + (order.Chats.idManager == null ? "ожидание" : order.Chats.Employees.ToString());
                    break;
                case 3:
                    ExecutorBlock.Text = $"Заказчик: {order.Customers.name}";
                    ManagerBlock.Text = "Исполнитель: " + (order.idExecutor == null ? "ожидание" : order.Employees.ToString());
                    break;
            }
        }

        private void SetFileValues()
        {
            if (_file == null)
            {
                FileDownload.IsEnabled = false;
                ReturnBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                FileDownload.Content = $"{_file.fileName}.{_file.fileExtension}";
                FileDownload.IsEnabled = true;
                TempFile = _file;
                ClearBtn.IsEnabled = true;
            }
        }

        private readonly Orders _order;

        private readonly HistoryUploaded _file;


        private HistoryUploaded _tempFile;
        private HistoryUploaded TempFile
        {
            get => _tempFile;
            set
            {
                _isFileEdit = value != _file;
                FileDownload.IsEnabled = value != null;
                _tempFile = value;
                FileDownload.Content = value == null ? string.Empty : $"{value.fileName}.{value.fileExtension}";
            }
        }

        private bool _isFileEdit;
        
        private void UploadBtn_Click(object sender, RoutedEventArgs e)
        {
            var openFile = new OpenFileDialog
            {
                Title = "Загрузка файла",
                Filter = "Word документ (*.docx)|*.docx|Word 97-2003 документ (*.doc)|*.doc|Portable Document|*.pdf",
                DefaultExt = ".docx"
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

            if (MessageBox.Show("Файл успешно сохранён. Открыть?", "Успешное сохранение", MessageBoxButton.YesNo,
                    MessageBoxImage.Question)
                != MessageBoxResult.Yes) return;

            Process.Start(saveFileDialog.FileName);
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

        private void DownloadFileOrderStageBtn_Click(object sender, RoutedEventArgs e)
        {
            var file = (HistoryUploaded)((Button)sender).CommandParameter;
            
            var saveFileDialog = new SaveFileDialog
            {
                Title = "Скачивание файла",
                OverwritePrompt = true,
                FileName = $"{file.fileName}.{file.fileExtension}",
                Filter = $"File | * {file.fileExtension}",
                DefaultExt = $".{file.fileExtension}"
            };
            if (saveFileDialog.ShowDialog() != true) return;

            using (var fs = new FileStream(saveFileDialog.FileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                fs.Write(file.fileContent, 0, file.fileContent.Length);
            }

            if (MessageBox.Show("Файл успешно сохранён. Открыть?", "Успешное сохранение", MessageBoxButton.YesNo,
                    MessageBoxImage.Question)
                != MessageBoxResult.Yes) return;

            Process.Start(saveFileDialog.FileName);
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_isFileEdit)
            {
                using (var db = new INCOMSYSTEMEntities())
                {
                    var orderDb = db.Orders.First(s => s.id == _order.id);
                    if (_file == null)
                    {
                        db.HistoryUploaded.Add(TempFile);
                        orderDb.idFile = TempFile.id;
                    }
                    else
                    {
                        var fileDb = db.HistoryUploaded.First(s => s.id == _file.id);
                        fileDb.fileName = TempFile.fileName;
                        fileDb.fileExtension = TempFile.fileExtension;
                        fileDb.fileSize = TempFile.fileSize;
                        fileDb.fileContent = TempFile.fileContent;
                        fileDb.dateAdded = DateTime.Now;
                        fileDb.uploadedBy = MainWindow.AuthUser.idUser;
                    }

                    db.SaveChanges();
                }
            }
            
            var wnd = Window.GetWindow(this);
            wnd.DialogResult = true;
            wnd.Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_isFileEdit && MessageBox.Show("Вы действительно хотите отменить изменения?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information) != MessageBoxResult.Yes) return;

            var wnd = Window.GetWindow(this);
            wnd.DialogResult = false;
            wnd.Close();
        }

        private void ViewTaskStagesBtn_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AdditionalWindow();
            addWindow.MFrame.Navigate(new ViewTaskStagesPage(_order.Tasks));
            addWindow.ShowDialog();
        }

        private void ViewResultStageMenu_Click(object sender, RoutedEventArgs e)
        {
            var orderStage = (OrderStages)((MenuItem)sender).CommandParameter;
            
            var addWindow = new AdditionalWindow();
            var resultStagePage = new ResultStagePage(orderStage.description);
            addWindow.MFrame.Navigate(resultStagePage);
            if(addWindow.ShowDialog() != true || orderStage.description == resultStagePage.ResultStage) return;

            using (var db = new INCOMSYSTEMEntities())
            {
                var orderStageDb = db.OrderStages.First(s => s.id == orderStage.id);
                orderStageDb.description = resultStagePage.ResultStage;
                db.SaveChanges();
            }

            RefreshOrderStages();
        }

        private void EditOrderStageMenu_Click(object sender, RoutedEventArgs e)
        {
            var orderStage = (OrderStages)((MenuItem)sender).CommandParameter;
            var addWindow = new AdditionalWindow();
            var orderStagePage = new OrderStagePage(orderStage);
            addWindow.MFrame.Navigate(orderStagePage);
            if (addWindow.ShowDialog() != true) return;

            RefreshOrderStages();
            InitTextBlock();
        }

        private void AddOrderStageMenu_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AdditionalWindow();
            var orderStagePage = new OrderStagePage(_order);
            addWindow.MFrame.Navigate(orderStagePage);
            if (addWindow.ShowDialog() != true) return;

            RefreshOrderStages();
            InitTextBlock();
        }

        private async void SaveReportMenu_Click(object sender, RoutedEventArgs e)
        {
            var orderStage = (OrderStages)((MenuItem)sender).CommandParameter;

            // Создание приложения Word
            var wApp = new Word.Application();

            // Путь к файлу
            var pathToFile = Directory.GetCurrentDirectory() + @"\Resources\ReportTemplate.docx";

            await OpenDocument(wApp, pathToFile, orderStage);
        }

        private async Task OpenDocument(Word.Application wApp, string pathToFile, OrderStages orderStage)
        {
            Word.Document doc = null;

            try
            {
                doc = await Task.Run(() => wApp.Documents.Open(pathToFile));

                await HandleSaveDialog(doc, orderStage);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при работе с документом: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (doc != null)
                {
                    doc.Saved = true;
                    doc.Close(SaveChanges: false);
                }

                wApp.Quit(SaveChanges: false);
            }
        }

        private async Task HandleSaveDialog(Word.Document doc, OrderStages orderStage)
        {
            var saveFileDialog = new SaveFileDialog
            {
                FileName = $"Отчёт №{orderStage.id}",
                Filter = "Word документ (*.docx)|*.docx|Word 97-2003 документ (*.doc)|*.doc|Portable Document|*.pdf",
                DefaultExt = "docx",
                OverwritePrompt = true
            };
            if (saveFileDialog.ShowDialog() != true) return;

            FormAgreement(ref doc, orderStage);
            await Task.Run(() => doc.SaveAs2(saveFileDialog.FileName));

            if (MessageBox.Show("Отчёт успешно сохранён. Открыть?", "Успешное сохранение", MessageBoxButton.YesNo,
                    MessageBoxImage.Question)
                != MessageBoxResult.Yes) return;

            await Task.Run(() => Process.Start(saveFileDialog.FileName));
        }

        private static void FormAgreement(ref Word.Document doc, OrderStages orderStage)
        {
            try
            {
                doc.ReplaceWordText("{idOrderStage}", orderStage.id);
                doc.ReplaceWordText("{customer}", orderStage.Orders.Customers.name);
                doc.ReplaceWordText("{executer}", orderStage.Orders.Employees.ToString());
                doc.ReplaceWordText("{idOrder}", orderStage.idOrder);
                doc.ReplaceWordText("{dateOrder}", orderStage.Orders.dateOrder.ToString("dd.MM.yyyy"));
                doc.ReplaceWordText("{nameTask}", orderStage.Orders.Tasks.name);
                doc.ReplaceWordText("{nameStage}", string.IsNullOrWhiteSpace(orderStage.name)
                    ? orderStage.TaskStages.name
                    : orderStage.name);
                doc.ReplaceWordText("{factDateStart}", orderStage.factDateStart.HasValue
                    ? orderStage.factDateStart.Value.ToString("dd.MM.yyyy")
                    : "Отсутствует");
                doc.ReplaceWordText("{factDateEnd}", orderStage.factDateComplete.HasValue
                    ? orderStage.factDateComplete.Value.ToString("dd.MM.yyyy")
                    : "Отсутствует");
                var orderStages = orderStage.description.SplitStringToObservableCollection().ToList();
                var i = 0;
                var count = orderStages.Count;
                if (count == 0) doc.ReplaceWordText("{comments}", "Результат отсутствует.");
                else
                {
                    foreach (var result in orderStages)
                    {
                        var str = result;
                        if (i++ < count - 1) str += ";\r{comments}";
                        else str += ".";
                        doc.ReplaceWordText("{comments}", str);
                    }
                }
            }
            catch
            {
                MessageBox.Show("Для сохранения записи, необходимо закрыть Word документ", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}