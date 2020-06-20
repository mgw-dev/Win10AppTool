using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Win10AppTool.Model
{
    public class Appx : INotifyPropertyChanged
    {
        private string name;
        private string fullName;
        private bool remove;
        private bool onlineProvisioned;

        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }

        public string FullName
        {
            get => fullName;
            set
            {
                fullName = value;
                OnPropertyChanged("FullName");
            }
        }

        public bool Remove
        {
            get => remove;
            set
            {
                remove = value;
                OnPropertyChanged("Remove");
            }
        }

        public bool OnlineProvisioned
        {
            get => onlineProvisioned;
            set
            {
                onlineProvisioned = value;
                OnPropertyChanged("OnlineProvisioned");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
