using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace INCOMSYSTEM.BehaviorsFiles
{
    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnRaiseChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
