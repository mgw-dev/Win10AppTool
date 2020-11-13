using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Win10AppTool.Annotations;

namespace Win10AppTool.Classes
{
    public class Win32App : WindowsApp
    {
        private string uninstallString;

        public Win32App()
        {
        }

        public string UninstallString
        {
            get => uninstallString;
            set
            {
                uninstallString = value;
                OnPropertyChanged(nameof(UninstallString));
            }
        }

        public override bool Uninstall()
        {
            //TODO: this
            throw new NotImplementedException();
            return false;
        }
    }
}
