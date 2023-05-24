using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using INCOMSYSTEM.Context;
using INCOMSYSTEM.Pages.Details;
using INCOMSYSTEM.Pages.MainPages.Views;
using INCOMSYSTEM.Windows;

namespace INCOMSYSTEM.Pages.MainPages
{
    /// <summary>
    /// Логика взаимодействия для ViewTasks.xaml
    /// </summary>
    public partial class ViewTasksPage
    {
        public ViewTasksPage()
        {
            InitializeComponent();

            using (var db = new INCOMSYSTEMEntities())
            {
                AllCountTask.Text = db.Tasks.Count().ToString();

                var max = db.Tasks.Max(s => s.price);

                RightSlider.Maximum = (long)max;

                LeftSlider.Value = 0;
                RightSlider.Value = (long)max;

                TasksList.ItemsSource = db.Tasks
                    .Include(s => s.HistoryUploaded)
                    .Include(s => s.Specializations)
                    .ToList();

                var list = new List<Specializations> { new Specializations { name = "Очистить" } };
                list.AddRange(db.Specializations.ToList());
                SpecBox.ItemsSource = list;
            }
        }

        private void SelectBtn_Click(object sender, RoutedEventArgs e)
        {
            var task = (Tasks)((Button)sender).CommandParameter;
            MainWindow.ReviewFrame.Navigate(new ViewDetailTaskPage(task));
        }

        private void FilterBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ApplyFilter();
        }

        private void SettingsFilterBtn_Click(object sender, RoutedEventArgs e)
        {
            var anim = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(0.7d),
                From = SideBarFilter.ActualWidth,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut },
                To = _isShowed ? 0 : 350
            };
            _isShowed = !_isShowed;
            SideBarFilter.BeginAnimation(WidthProperty, anim);
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            FilterText.Clear();
        }

        private bool _isShowed;

        private void ApplyFilter()
        {
            if (PriceBox == null || FilterText == null || TasksList == null || SpecBox == null || LeftSlider == null || RightSlider == null) return;

            using (var db = new INCOMSYSTEMEntities())
            {
                IEnumerable<Tasks> list = db.Tasks.Include(s => s.Specializations).Include(s => s.HistoryUploaded);
                
                if(!FilterText.IsWhiteSpace) list = list.Where(s => s.name.ToLower().Trim().Contains(FilterText.Text.ToLower().Trim())).ToList();

                if (SpecBox.SelectedIndex > 0)
                    list = list.Where(s => s.idSpecialization == ((Specializations)SpecBox.SelectedItem).id).ToList();
                
                switch (PriceBox.SelectedIndex)
                {
                    case 0:
                        list = list.OrderBy(s => s.discount != null && s.discount > 0 ? s.newPrice : s.price);
                        break;
                    case 1:
                        list = list.OrderByDescending(s => s.discount != null && s.discount > 0 ? s.newPrice : s.price);
                        break;
                }

                var min = (decimal)LeftSlider.Value;
                var max = (decimal)RightSlider.Value;

                list = list.Where(s => (s.discount != null && s.discount > 0 ? s.newPrice : s.price) >= min 
                                        && (s.discount != null && s.discount > 0 ? s.newPrice : s.price) <= max);

                TasksList.ItemsSource = list.ToList();
            }
        }
        private void FilterText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ClearBtn == null || FilterText == null) return;
            ApplyFilter();
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            var task = (Tasks)((Button)sender).CommandParameter;
            
            var addWindow = new AdditionalWindow();
            addWindow.MFrame.Navigate(new TaskDetailPage(task));
            if (addWindow.ShowDialog() != true) return;
            
            using (var db = new INCOMSYSTEMEntities())
            {
                var max = db.Tasks.Max(s => s.price);

                RightSlider.Maximum = (long)max;
            }
            
            ApplyFilter();
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите удалить эту задачу?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            
            var task = (Tasks)((Button)sender).CommandParameter;
            using (var db = new INCOMSYSTEMEntities())
            {
                db.Tasks.Remove(db.Tasks.First(s => s.id == task.id));
                db.SaveChanges();
                
                var max = db.Tasks.Max(s => s.price);

                RightSlider.Maximum = (long)max;
            }
            
            ApplyFilter();
        }

        private void AddTaskMenu_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AdditionalWindow();
            addWindow.MFrame.Navigate(new TaskDetailPage());
            if (addWindow.ShowDialog() != true) return;
            
            using (var db = new INCOMSYSTEMEntities())
            {
                var max = db.Tasks.Max(s => s.price);

                RightSlider.Maximum = (long)max;
            }
            
            ApplyFilter();
        }
    }
    //**Пупсик камазик!** 😀😍😘
}
