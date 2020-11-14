using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Win10AppTool.Classes;

namespace Win10AppTool.ViewModel
{
    public class AppxViewModel
    {
        public ObservableCollection<AppxPackage> apps { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void InitApps()
        {
            apps ??= new ObservableCollection<AppxPackage>();
        }

        public void SortApps()
        {
            apps = new ObservableCollection<AppxPackage>(apps.OrderBy(x => x.Name));
        }
        public void LoadAppx(bool allUsers, bool noStore)
        {
            InitApps();
            foreach (AppxPackage application in ApplicationHelper.LoadAppx(allUsers, noStore))
            {
                apps.Add(application);
                application.PropertyChanged += Appx_PropertyChanged;
            }

            SortApps();
        }

        public void LoadAppxOnline(bool noStore)
        {
            InitApps();
            foreach (AppxPackage application in ApplicationHelper.LoadAppxOnline(noStore))
            {
                apps.Add(application);
                application.PropertyChanged += Appx_PropertyChanged;
            }
            SortApps();
        }

        private void Appx_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }

    }
}
