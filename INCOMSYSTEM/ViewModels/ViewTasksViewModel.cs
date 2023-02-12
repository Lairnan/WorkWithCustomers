using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using INCOMSYSTEM.BehaviorsFiles;
using INCOMSYSTEM.Context;

namespace INCOMSYSTEM.ViewModels
{
    public class ViewTasksViewModel : ObservableObject
    {
        public ViewTasksViewModel()
        {
            using (var db = new INCOMSYSTEMEntities())
            {
                TasksCollection = new ObservableCollection<Tasks>(db.Tasks.Include(s => s.Specializations).ToList());
            }
        }

        public ObservableCollection<Tasks> TasksCollection { get; set; }
    }
}