using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace S2_IM_Client.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName()] string name = null)
        {
            if (name != null) PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}