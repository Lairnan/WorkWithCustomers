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
            
            Task = task;
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
            PriceBox.Text = $"{task.price:0.00}";
            if (task.discount != null) DiscountBox.Text = $"{task.discount}";
            ApproxTimeBox.Text = $"{task.approxCompleteTime}";
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

        private readonly Tasks Task;

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
                // TODO: Сделать вывод ошибки о неправильном формате
                return;
            }
            TempFile = File.ReadAllBytes(file);
            TempFileExtension = fileExtension;
            ClearBtn.IsEnabled = true;
            ReturnBtn.IsEnabled = true;
            FileName = file.Split('\\').Last();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            switch (_isEdit)
            {
                case true when !SaveTask():
                case false when !AddTask():
                    return;
            }

            var addWindow = Application.Current.Windows.OfType<AdditionalWindow>().First();
            addWindow.DialogResult = true;
            addWindow.Close();
        }

        private bool SaveTask()
        {
            if (IsNullOrWhiteSpace())
            {
                MessageBox.Show("Поля не могут быть пустыми");
                // TODO: Сделать вывод ошибки снизу окна или всплывающей подсказкой
                return false;
            }
            if (!GetValues()) return false;

            if (IsEqualsOrNull()) return true;
            using (var db = new INCOMSYSTEMEntities())
            {
                var task = db.Tasks.First(s => s.id == Task.id);
                task.name = _name;
                task.description = _desc;
                task.price = _price;
                if (_disc > 0) task.discount = _disc;
                else task.discount = null;
                task.approxCompleteTime = _approx;
                task.idSpecialization = _spec.id;
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
                MessageBox.Show("Поля не могут быть пустыми");
                // TODO: Сделать вывод ошибки снизу окна или всплывающей подсказкой
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
        private byte _disc;
        private int _approx;

        private bool GetValues()
        {
            _name = NameBox.Value;
            _spec = SpecBox.SelectedItem as Specializations;
            _desc = DescriptionBox.Value;
            if (!decimal.TryParse(PriceBox.Value, out _price))
            {
                MessageBox.Show("Не удалось преобразовать цену в число");
                return false;
            }

            if (!DiscountBox.IsWhiteSpace && !byte.TryParse(DiscountBox.Value, out _disc))
            {
                MessageBox.Show("Не удалось преобразовать скидку");
                return false;
            }

            if (int.TryParse(ApproxTimeBox.Value, out _approx)) return true;
            MessageBox.Show("Не удалось преобразовать время выполнения в число");
            return false;

        }

        private bool IsNullOrWhiteSpace()
        {
            return NameBox.IsWhiteSpace || DescriptionBox.IsWhiteSpace || PriceBox.IsWhiteSpace ||
                   ApproxTimeBox.IsWhiteSpace;
            
            return string.IsNullOrWhiteSpace(NameBox.Value)
                   || string.IsNullOrWhiteSpace(DescriptionBox.Value)
                   || string.IsNullOrWhiteSpace(PriceBox.Value)
                   || string.IsNullOrWhiteSpace(ApproxTimeBox.Value);
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
            if(_isEdit) return NameBox.Value == Task.name
                       && ((Specializations)SpecBox.SelectedItem).id == Task.idSpecialization
                       && DescriptionBox.Value == Task.description
                       && PriceBox.Value == Task.price.ToString("0.00")
                       && DiscountBox.Value == Task.discount.ToString()
                       && ApproxTimeBox.Value == Task.approxCompleteTime.ToString()
                       && (_fileTask == TempFile && _fileTaskExtension == TempFileExtension);

            return NameBox.IsWhiteSpace
                   && DescriptionBox.IsWhiteSpace
                   && PriceBox.IsWhiteSpace
                   && DiscountBox.IsWhiteSpace
                   && ApproxTimeBox.IsWhiteSpace
                   && string.IsNullOrWhiteSpace(FileName);
        }
    }
}