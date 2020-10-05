﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Win10AppTool.Classes;

namespace Win10AppTool.ViewModel
{
    public class AppxViewModel
    {
        public ObservableCollection<AppxPackage> apps { get; set; }
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
            foreach (AppxPackage appx in ApplicationHelper.LoadAppx(allUsers, noStore))
            {
                apps.Add(appx);
            }

            SortApps();
        }

        public void LoadAppxOnline(bool noStore)
        {
            InitApps();
            foreach (AppxPackage appx in ApplicationHelper.LoadAppxOnline(noStore))
            {
                apps.Add(appx);
            }
            SortApps();
        }
    }
}
