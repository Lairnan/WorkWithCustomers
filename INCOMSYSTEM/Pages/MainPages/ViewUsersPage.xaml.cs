using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using INCOMSYSTEM.Context;

namespace INCOMSYSTEM.Pages.MainPages
{
    /// <summary>
    /// Логика взаимодействия для ViewTasks.xaml
    /// </summary>
    public partial class ViewUsersPage
    {
        public ViewUsersPage()
        {
            InitializeComponent();

            using (var db = new INCOMSYSTEMEntities())
            {
                AllCountUsers.Text = db.UsersDetail.Count().ToString();

                UsersList.ItemsSource = db.UsersDetail
                    .Include(s => s.Employees)
                    .Include(s => s.Customers)
                    .Include(s => s.Positions)
                    .ToList();

                var list = new List<Positions> { new Positions { name = "Очистить" } };
                list.AddRange(db.Positions.ToList());
                PositionBox.ItemsSource = list;
            }
        }

        private void FilterBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
            if (FilterText == null || UsersList == null) return;

            using (var db = new INCOMSYSTEMEntities())
            {
                IEnumerable<UsersDetail> list = db.UsersDetail
                    .Include(s => s.Employees)
                    .Include(s => s.Customers)
                    .Include(s => s.Positions)
                    .ToList();
                
                if(!FilterText.IsWhiteSpace) list = list.Where(s => s.idPos == 1 ? s.Customers.name.ToLower().Trim().StartsWith(FilterText.Text.ToLower().Trim())
                : s.Employees.ToString().ToLower().Trim().StartsWith(FilterText.Text.ToLower().Trim()));

                if (PositionBox.SelectedIndex > 0) list = list.Where(s => s.idPos == ((Positions)PositionBox.SelectedItem).id);

                UsersList.ItemsSource = list.ToList();
            }
        }
        private void FilterText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ClearBtn == null || FilterText == null) return;
            ApplyFilter();
        }
    }
}
