using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Win10AppTool.Classes;

namespace Win10AppTool.ViewModel
{
    public class AppxViewModel
    {
        public ObservableCollection<Appx> apps { get; set; }

        private void InitApps()
        {
            apps ??= new ObservableCollection<Appx>();
        }

        public void LoadAppx(bool allUsers, bool noStore)
        {
            InitApps();
            foreach (Appx appx in PSRunner.LoadAppx(allUsers, noStore))
            {
                appx.LoadImg();
                apps.Add(appx);
            }
        }

        public void LoadAppxOnline(bool noStore)
        {
            InitApps();
            foreach (Appx appx in PSRunner.LoadAppxOnline(noStore))
            {
                appx.LoadImg();
                apps.Add(appx);
            }
        }
    }
}
