using System.Data.Entity;
using System.Linq;
using System.Windows.Controls;
using INCOMSYSTEM.Context;

namespace INCOMSYSTEM.Pages.MainPages
{
    public partial class ViewOrdersPage : Page
    {
        public ViewOrdersPage()
        {
            InitializeComponent();
            using (var db = new INCOMSYSTEMEntities())
            {
                AllItemsCount.Text = db.Orders.Count().ToString();
                OrdersList.ItemsSource = db.Orders
                    .Include(s => s.Customers)
                    .Include(s => s.Employees)
                    .Include(s => s.Statuses)
                    .Include(s => s.Tasks)
                    .Include(s => s.Chats)
                    .ToList();
            }
        }
    }
}