using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
            Title = "Редактирование заказа";
            _isEdit = true;
            SaveBtn.Content = "Сохранить";
            
            _task = task;
            _fileTask = task.attachment;
            _fileTaskExtension = task.fileExtension;
            SetFileValues(task);

            _oldName = task.name;
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
            if (_fileTask == null)
            {
                FileDownload.IsEnabled = false;
                ReturnBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                FileDownload.Content = $"{task.name}.{task.fileExtension}";
                FileDownload.IsEnabled = true;
                TempFile = _fileTask;
                TempFileExtension = _fileTaskExtension;
                ClearBtn.IsEnabled = true;
            }
        }

        public TaskDetailPage()
        {
            InitializeComponent();
            Title = "Добавление заказа";
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

        private string _fileName;
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

        private readonly bool _isEdit;

        private readonly string _oldName;
        
        private readonly byte[] _fileTask;
        private readonly string _fileTaskExtension;

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
            FileName = openFile.SafeFileName;
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
            TempFile = _fileTask;
            TempFileExtension = _fileTaskExtension;
            FileName = $"{_oldName}.{_fileTaskExtension}";
            ReturnBtn.IsEnabled = false;
            ClearBtn.IsEnabled = true;
        }

        private void FileDownloadBtn_Click(object sender, RoutedEventArgs e)
        {
            if (TempFile == null) return;
            
            var saveFileDialog = new SaveFileDialog
            {
                Title = "Скачивание файла",
                FileName = $"{NameBox.Value}",
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
            FileName = file.Split('\\').Last();
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
                task.attachment = TempFile;
                task.fileExtension = TempFileExtension;

                db.SaveChanges();
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
                var task = new Tasks
                {
                    name = _name,
                    description = _desc,
                    price = _price,
                    approxCompleteTime = _approx,
                    idSpecialization = _spec.id,
                    supportPeriod = _support,
                    attachment = TempFile,
                    fileExtension = TempFileExtension
                };
                if (_disc > 0) task.discount = _disc;
                else task.discount = null;

                db.Tasks.Add(task);
                db.SaveChanges();
            }

            return true;
        }

        private string _name;
        private Specializations _spec;
        private string _desc;
        private decimal _price;
        public int _support;
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

            if (!DiscountBox.IsWhiteSpace && !byte.TryParse(DiscountBox.Value, out _disc))
            {
                AdditionalWindow.ShowError("Не удалось преобразовать скидку в число");
                return false;
            }

            if (int.TryParse(ApproxTimeBox.Value, out _approx)) return true;
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
                        && (_fileTask == TempFile && _fileTaskExtension == TempFileExtension);

            return NameBox.IsWhiteSpace
                   && DescriptionBox.IsWhiteSpace
                   && PriceBox.IsWhiteSpace
                   && DiscountBox.IsWhiteSpace
                   && SupportPeriod.IsWhiteSpace
                   && ApproxTimeBox.IsWhiteSpace
                   && string.IsNullOrWhiteSpace(FileName);
        }
    }
}