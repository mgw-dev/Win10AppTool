using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Win10AppTool.Model;

namespace Win10AppTool.ViewModel
{
    public class AppxViewModel
    {
        public ObservableCollection<Appx> apps { get; set; }

        private void InitApps()
        {
            apps ??= new ObservableCollection<Appx>();
        }

        public void LoadAppx(bool? allUsers)
        {
            InitApps();
            foreach (Appx appx in PSRunner.LoadAppx(allUsers))
            {
                apps.Add(appx);
            }
        }

        public void LoadAppxOnline()
        {
            InitApps();
            foreach (Appx appx in PSRunner.LoadAppxOnline())
            {
                apps.Add(appx);
            }
        }
    }
}
