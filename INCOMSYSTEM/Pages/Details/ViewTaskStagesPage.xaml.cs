using System.Linq;
using System.Windows.Controls;
using INCOMSYSTEM.Context;

namespace INCOMSYSTEM.Pages.Details
{
    public partial class ViewTaskStagesPage : Page
    {
        public ViewTaskStagesPage(Tasks task)
        {
            InitializeComponent();
            using (var db = new INCOMSYSTEMEntities())
            {
                TaskStagesList.Items.Clear();
                TaskStagesList.ItemsSource = db.TaskStages.Where(s => s.idTask == task.id).ToList();
            }
        }
    }
}