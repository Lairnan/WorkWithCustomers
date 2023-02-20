using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using INCOMSYSTEM.Context;

namespace INCOMSYSTEM.Pages
{
    /// <summary>
    /// Логика взаимодействия для ViewTasks.xaml
    /// </summary>
    public partial class ViewTasks : Page
    {
        public ViewTasks()
        {
            InitializeComponent();

            using (var db = new INCOMSYSTEMEntities())
            {
                TasksList.ItemsSource = db.Tasks.Include(s => s.Specializations).ToList();
            }
        }

        private void SelectBtn_Click(object sender, RoutedEventArgs e)
        {
            var task = (sender as Button).CommandParameter as Tasks;
            MessageBox.Show(task.description);
        }

        private void FilterBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Slider_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        private void SettingsFilterBtn_Click(object sender, RoutedEventArgs e)
        {
            var anim = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(0.7d),
                From = SideBarFilter.ActualWidth,
                EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut }
            };
            if (_isShowed) anim.To = 0;
            else anim.To = 350;
            _isShowed = !_isShowed;
            SideBarFilter.BeginAnimation(WidthProperty, anim);
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            FilterText.Clear();
        }

        private bool _isShowed;

        private void Look()
        {
            if (PriceBox == null || FilterText == null || TasksList == null || SpecBox == null) return;

            // using (var db = new INCOMSYSTEMEntities())
            // {
            //     var list = db.Tasks.Include(s => s.Specializations);
            // }
            
            switch (PriceBox.SelectedIndex)
            {
                case 0:
                    TasksList.ItemsSource = TasksList.ItemsSource.Cast<Tasks>().OrderBy(s => s.price).ToList();
                    break;
                case 1:
                    TasksList.ItemsSource = TasksList.ItemsSource.Cast<Tasks>().OrderByDescending(s => s.price).ToList();
                    break;
            }
        }
        private void FilterText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ClearBtn == null || FilterText == null) return;
            Look();
        }
    }
    //**Пупсик камазик!** 😀😍😘
}
