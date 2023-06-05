using INCOMSYSTEM.BehaviorsFiles;
using INCOMSYSTEM.Context;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace INCOMSYSTEM
{
    public class TestViewModel : ObservableObject
    {
        public TestViewModel()
        {
            using(var db = new INCOMSYSTEMEntities())
            {
                if (UsersDetail.idPos != 1) return;

                OrdersCollection = db.Orders
                    .Include(s => s.Employees)
                    .Include(s => s.Employees.UsersDetail)
                    .Include(s => s.Customers.UsersDetail)
                    .Include(s => s.Statuses)
                    .Include(s => s.Tasks)
                    .Include(s => s.Chats)
                    .Include(s => s.Chats.Employees)
                    .Where(s => s.idCustomer == UsersDetail.idUser)
                    .ToList();
            }
        }

        public static List<Orders> OrdersCollection { get; set; }
        
        public static UsersDetail UsersDetail { get; set; }
    }
}