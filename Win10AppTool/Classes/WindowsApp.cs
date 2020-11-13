using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Controls;
using Win10AppTool.Annotations;

namespace Win10AppTool.Classes
{
    public abstract class WindowsApp : INotifyPropertyChanged
    {
        protected string name;
        protected Image img;
        protected bool remove;
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool Remove
        {
            get => remove;
            set
            {
                remove = value;
                OnPropertyChanged(nameof(Remove));
            }
        }

        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public Image Img
        {
            get => img;
            set
            {
                img = value;
                OnPropertyChanged(nameof(Img));
            }
        }

        public abstract bool Uninstall();
    }
}
