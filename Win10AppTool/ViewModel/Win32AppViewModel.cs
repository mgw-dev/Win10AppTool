using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Win10AppTool.Classes;

namespace Win10AppTool.ViewModel
{
    public class Win32AppViewModel
    {
        public ObservableCollection<Win32App> apps { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void InitApps()
        {
            apps ??= new ObservableCollection<Win32App>();
        }

        public void SortApps()
        {
            apps = new ObservableCollection<Win32App>(apps.OrderBy(x => x.Name));
        }
        public void LoadWin32()
        {
            InitApps();
            foreach (Win32App application in ApplicationHelper.LoadWin32Apps())
            {
                apps.Add(application);
                application.PropertyChanged += Application_PropertyChanged;
            }
            SortApps();
        }

        private void Application_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }
    }

}
