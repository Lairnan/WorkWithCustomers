using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using INCOMSYSTEM.Context;

namespace INCOMSYSTEM.Pages
{
    public partial class ProfilePage : Page
    {
        public ProfilePage(UsersDetail usersDetail)
        {
            _usersDetail = usersDetail;
            InitializeComponent();

            RefreshOrders();
            
            OrdersList.ItemsSource = OrdersCollection;

            NameBlock.Text = usersDetail.idPos == 1 ? $"ФИО: {usersDetail.Customers.name}" : $"ФИО: {usersDetail.Employees}";
            PositionBlock.Text = $"Должность: {usersDetail.Positions.name}";
            PassportBlock.Text = $"Паспорт: {usersDetail.SeriePassport} {usersDetail.NumberPassport}";
            PhoneBlock.Text = $"Номер телефона: {usersDetail.phone}";
            PhoneBlock.Text = "Номер телефона: " + (usersDetail.phone != null ? usersDetail.phone.ToString() : "Отсутствует");
            
            if (usersDetail.idPos == 1) CustomerLegalFormBlock.Text = $"Правовая форма: {usersDetail.Customers.LegalForms.name}";
            else CustomerLegalFormBlock.Visibility = Visibility.Collapsed;
            
            AddressBlock.Text = $"Адрес проживания: {usersDetail.address}";
            DateStart.Text = "Дата регистрации: " + usersDetail.dateStart.ToString("dd MMMM yyyy", CultureInfo.CurrentCulture);
        }

        private readonly UsersDetail _usersDetail;
        private ObservableCollection<Orders> OrdersCollection { get; set; } = new ObservableCollection<Orders>();

        private async void RefreshOrders()
        {
            var list = await Task.Run(() =>
            {
                using (var db = new INCOMSYSTEMEntities())
                {
                    return db.Orders
                        .Include(s => s.Employees)
                        .Include(s => s.Employees.UsersDetail)
                        .Include(s => s.Customers.UsersDetail)
                        .Include(s => s.Statuses)
                        .Include(s => s.Tasks)
                        .Include(s => s.Chats)
                        .Include(s => s.Chats.Employees)
                        .Where(s => s.idCustomer == _usersDetail.idUser || s.idExecutor == _usersDetail.idUser || s.Chats.idManager == _usersDetail.idUser)
                        .ToList();
                }
            });

            OrdersCollection = new ObservableCollection<Orders>(list);
            OrdersList.ItemsSource = OrdersCollection;

            var count = 0;
            Orders order = null;
            
            var groupBy = list.GroupBy(s => s.idTask);
            foreach (var ordersEnumerable in groupBy)
            {
                var k = ordersEnumerable.Count();
                if (k <= count) continue;
                
                count = k;
                order = ordersEnumerable.First();
            }
            PopularOrderBlock.Text = "Популярный заказ: " + (order != null ? order.Tasks.name : "Отсутствует");
            MostExpensiveBlock.Text = "Самый дорогой заказ: " + (list.Count > 0 ? list.Max(s => s.price).ToString("#,#", CultureInfo.CurrentCulture) + " руб." : "Отсутствует");
        }
        
        private void RefreshOrdersBtn_Click(object sender, RoutedEventArgs e)
        {
            RefreshOrders();
        }
    }
}