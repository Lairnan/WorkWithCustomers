using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using INCOMSYSTEM.BehaviorsFiles;
using INCOMSYSTEM.Context;
using INCOMSYSTEM.Windows;

namespace INCOMSYSTEM.Pages.Details
{
    public partial class ResultStagePage : Page
    {
        private readonly string _resultStage;
        public string ResultStage { get; private set; }
        
        public ResultStagePage()
        {
            _resultStage = null;
            ResultStage = null;
            InitializeComponent();

            ItemList.ItemsSource = new ObservableCollection<string>();

            InitPanelAndEvents();
        }

        private void InitPanelAndEvents()
        {
            if (MainWindow.AuthUser == null || MainWindow.AuthUser.idPos != 2)
            {
                EditPanel.Visibility = Visibility.Collapsed;
                SavePanel.Visibility = Visibility.Collapsed;
                return;
            }

            KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter) AddButton_Click(s, e);
            };
            this.PreviewMouseDown += (s, e) =>
            {
                if (!NewItemTextBox.IsMouseOver)
                    NewItemTextBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
            };
        }

        public ResultStagePage(string description)
        {
            _resultStage = description;
            ResultStage = description;
            InitializeComponent();

            if (!string.IsNullOrEmpty(ResultStage)) ItemList.ItemsSource = ResultStage.SplitStringToObservableCollection();
            else ItemList.ItemsSource = new ObservableCollection<string>();

            InitPanelAndEvents();
        }
        
        private int _selectedItemIndex = -1;
        
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button)?.CommandParameter;
            ((ObservableCollection<string>)ItemList.ItemsSource).Remove(item as string);
            ResultStage = ConcatenateListItemsToString();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (NewItemTextBox.IsWhiteSpace)
            {
                MessageBox.Show("Поле не может быть пустым");
                return;
            }

            var newItemName = NewItemTextBox.Value;
            
            if (_selectedItemIndex != -1)
            {
                ((ObservableCollection<string>)ItemList.ItemsSource)[_selectedItemIndex] = newItemName;
                _selectedItemIndex = -1;
                AddEditSaveBtn.Content = "Добавить";
            }
            else ((ObservableCollection<string>)ItemList.ItemsSource).Add(newItemName);

            ResultStage = ConcatenateListItemsToString();
            NewItemTextBox.Clear();
        }
        
        private void EditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as MenuItem)?.CommandParameter;
            if (item != null)
            {
                _selectedItemIndex = ItemList.Items.IndexOf(item);
                NewItemTextBox.Text = item as string ?? string.Empty;
            }

            AddEditSaveBtn.Content = "Сохранить";
            NewItemTextBox.Focus();
        }
        
        private string ConcatenateListItemsToString()
        {
            var items = ItemList.Items;
            var sb = new StringBuilder();
            foreach (var item in items)
            {
                sb.Append(item);
                sb.Append(";");
            }
            if (sb.ToString().EndsWith(";"))
            {
                sb.Remove(sb.Length - 1, 1);
            }
            return sb.ToString();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var wnd = Window.GetWindow(this);
            wnd.DialogResult = true;
            wnd.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (ResultStage != _resultStage
                && MessageBox.Show("Изменения не будут сохранены, продолжить?",
                    "Закрытие",
                    MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            
            var wnd = Window.GetWindow(this);
            wnd.DialogResult = false;
            wnd.Close();
        }
    }
}