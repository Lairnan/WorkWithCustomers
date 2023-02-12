using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace INCOMSYSTEM.BehaviorsFiles
{
    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnRaiseChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}