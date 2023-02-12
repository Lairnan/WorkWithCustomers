using System.Data.Entity;
using System.Linq;
using System.Windows.Controls;
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
                Lv.ItemsSource = db.Tasks.Include(s => s.Specializations).ToList();
            }
        }
    }
}
