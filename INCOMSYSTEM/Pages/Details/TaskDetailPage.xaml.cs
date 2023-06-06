using System;
using System.Globalization;
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
    public partial class TaskDetailPage : Page
    {
        public TaskDetailPage(Tasks task)
        {
            InitializeComponent();
            Title = "Редактирование задачи";
            _isEdit = true;
            SaveBtn.Content = "Сохранить";
            
            _task = task;
            _file = task.HistoryUploaded;
            SetFileValues(task);

            SetInputBoxValues(task);

            using (var db = new INCOMSYSTEMEntities())
            {
                SpecBox.ItemsSource = db.Specializations.ToList();
                SpecBox.SelectedItem = SpecBox.ItemsSource.Cast<Specializations>()
                    .First(s => s.id == task.idSpecialization);
            }
        }

        private void SetInputBoxValues(Tasks task)
        {
            NameBox.Text = task.name;
            DescriptionBox.Text = task.description;
            PriceBox.Text = $"{task.price.ToString("#,#", CultureInfo.CurrentCulture)}";
            if (task.discount != null) DiscountBox.Text = $"{task.discount}";
            ApproxTimeBox.Text = $"{task.approxCompleteTime}";
            SupportPeriod.Text = task.supportPeriod.ToString();
        }

        private void SetFileValues(Tasks task)
        {
            if (_file == null)
            {
                FileDownload.IsEnabled = false;
                ReturnBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                FileDownload.Content = $"{_file.fileName}.{_file.fileExtension}";
                FileSizeNameBlock.Text = _file.fileSize.ConvertFileSizeToString();
                FileDownload.IsEnabled = true;
                TempFile = _file;
                ClearBtn.IsEnabled = true;
            }
        }

        public TaskDetailPage()
        {
            InitializeComponent();
            Title = "Добавление задачи";
            _isEdit = false;
            SaveBtn.Content = "Добавить";

            FileDownload.IsEnabled = false;
            ReturnBtn.Visibility = Visibility.Collapsed;
            using (var db = new INCOMSYSTEMEntities())
            {
                SpecBox.ItemsSource = db.Specializations.ToList();
            }
        }

        private readonly Tasks _task;

        private readonly bool _isEdit;

        private readonly HistoryUploaded _file;


        private HistoryUploaded _tempFile;
        private HistoryUploaded TempFile
        {
            get => _tempFile;
            set
            {
                FileDownload.IsEnabled = value != null;
                _tempFile = value;
                FileDownload.Content = value == null ? string.Empty : $"{value.fileName}.{value.fileExtension}";
                FileSizeNameBlock.Text = value == null ? string.Empty : value.fileSize.ConvertFileSizeToString();
            }
        }

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

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            switch (_isEdit)
            {
                case true when !SaveTask():
                case false when !AddTask():
                    return;
            }

            AdditionalWindow.HideError();
            var addWindow = Application.Current.Windows.OfType<AdditionalWindow>().First();
            addWindow.DialogResult = true;
            addWindow.Close();
        }

        private bool SaveTask()
        {
            if (IsNullOrWhiteSpace())
            {
                AdditionalWindow.ShowError("Поля отмеченные звёздочкой не могут быть пустыми");
                return false;
            }
            if (!GetValues()) return false;

            if (IsEqualsOrNull()) return true;
            using (var db = new INCOMSYSTEMEntities())
            {
                var task = db.Tasks.First(s => s.id == _task.id);
                task.name = _name;
                task.description = _desc;
                task.price = _price;
                if (_disc > 0) task.discount = _disc;
                else task.discount = null;
                task.approxCompleteTime = _approx;
                task.idSpecialization = _spec.id;
                task.supportPeriod = _support;

                if (task.HistoryUploaded == null && TempFile != null)
                {
                    db.HistoryUploaded.Add(TempFile);
                    task.idFile = TempFile.id;
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

                db.SaveChanges();
                MessageBox.Show("Данные успешно сохранены");
            }

            return true;
        }

        private bool AddTask()
        {
            if (IsNullOrWhiteSpace())
            {
                AdditionalWindow.ShowError("Поля не могут быть пустыми");
                return false;
            }
            if (!GetValues()) return false;

            using (var db = new INCOMSYSTEMEntities())
            {
                long? idFile = null;
                if (TempFile != null)
                {
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
                }

                var task = new Tasks
                {
                    name = _name,
                    description = _desc,
                    price = _price,
                    approxCompleteTime = _approx,
                    idSpecialization = _spec.id,
                    supportPeriod = _support,
                    idFile = idFile
                };
                if (_disc > 0) task.discount = _disc;
                else task.discount = null;

                db.Tasks.Add(task);
                db.SaveChanges();
                MessageBox.Show("Данные успешно добавлены");
            }

            return true;
        }

        private string _name;
        private Specializations _spec;
        private string _desc;
        private decimal _price;
        private int _support;
        private byte _disc;
        private int _approx;

        private bool GetValues()
        {
            if (IsNullOrWhiteSpace())
            {
                AdditionalWindow.ShowError("Поля не могут быть пустыми");
                return false;
            }
            _name = NameBox.Value;
            _spec = SpecBox.SelectedItem as Specializations;
            _desc = DescriptionBox.Value;
            if (!decimal.TryParse(PriceBox.Value, out _price))
            {
                AdditionalWindow.ShowError("Не удалось преобразовать строку в число");
                return false;
            }

            if(!int.TryParse(SupportPeriod.Value, out _support))
            {
                AdditionalWindow.ShowError("Не удалось преобразовать период поддержки в число");
                return false;
            }

            if (DiscountBox.IsWhiteSpace || byte.TryParse(DiscountBox.Value, out _disc))
                return int.TryParse(ApproxTimeBox.Value, out _approx);
            AdditionalWindow.ShowError("Не удалось преобразовать скидку в число");
            return false;

        }

        private bool IsNullOrWhiteSpace()
        {
            return NameBox.IsWhiteSpace || DescriptionBox.IsWhiteSpace || PriceBox.IsWhiteSpace ||
                   ApproxTimeBox.IsWhiteSpace || SupportPeriod.IsWhiteSpace;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!IsEqualsOrNull()
                && MessageBox.Show("Отменить внесённые изменения?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) return;
            
            var addWindow = Application.Current.Windows.OfType<AdditionalWindow>().First();
            addWindow.DialogResult = false;
            addWindow.Close();
        }

        private bool IsEqualsOrNull()
        {
            if (_isEdit) return NameBox.Value == _task.name
                        && ((Specializations)SpecBox.SelectedItem).id == _task.idSpecialization
                        && DescriptionBox.Value == _task.description
                        && PriceBox.Value == _task.price.ToString("#,#", CultureInfo.CurrentCulture)
                        && SupportPeriod.Value == _task.supportPeriod.ToString()
                        && DiscountBox.Value == _task.discount.ToString()
                        && ApproxTimeBox.Value == _task.approxCompleteTime.ToString()
                        && (_file?.fileContent == TempFile?.fileContent
                            && _file?.fileExtension == TempFile?.fileExtension
                            && _file?.fileName == TempFile?.fileName
                            && _file?.fileSize == TempFile?.fileSize);

            return NameBox.IsWhiteSpace
                   && DescriptionBox.IsWhiteSpace
                   && PriceBox.IsWhiteSpace
                   && DiscountBox.IsWhiteSpace
                   && SupportPeriod.IsWhiteSpace
                   && ApproxTimeBox.IsWhiteSpace;
        }

        private void PriceBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (PriceBox.IsWhiteSpace) return;
            
            if (!decimal.TryParse(PriceBox.Value, out _price))
            {
                AdditionalWindow.ShowError("Не удалось преобразовать строку в число");
                return;
            }
            
            AdditionalWindow.HideError();
            var selStart = PriceBox.SelectionStart;
            var selLength = PriceBox.SelectionLength;
            var length = PriceBox.Text.Length;
            PriceBox.Text = _price.ToString("#,#", CultureInfo.CurrentCulture);
            if (length == PriceBox.Text.Length - 1) selStart++;
            PriceBox.SelectionStart = selStart;
            PriceBox.SelectionLength = selLength;
        }
    }
}