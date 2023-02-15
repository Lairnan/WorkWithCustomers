using System.Data.Entity;
using System.Linq;
using System.Windows;
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
                TasksList.ItemsSource = db.Tasks.Include(s => s.Specializations).ToList();
            }
        }

        private void SelectBtn_Click(object sender, RoutedEventArgs e)
        {
            var task = (sender as Button).CommandParameter as Tasks;
            MessageBox.Show(task.description);
        }
    }
}
