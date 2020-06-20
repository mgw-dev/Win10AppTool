using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
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
            using PowerShell psInstance = PowerShell.Create();

            if (allUsers == true)
            {
                psInstance.AddScript("Get-AppxPackage");
            }
            else
            {
                psInstance.AddScript("Get-AppxPackage -AllUsers");
            }
           
            IEnumerable<PSObject> results = psInstance.Invoke();
            foreach (PSObject result in results)
            {
                bool isFramework = Convert.ToBoolean(result.Properties["IsFramework"].Value);
                bool nonRemovable = Convert.ToBoolean(result.Properties["NonRemovable"].Value);
                if (!isFramework && !nonRemovable)
                {
                    Appx tmp = new Appx
                    {
                        Name = result.Properties["Name"].Value.ToString(),
                        FullName = result.Properties["PackageFullName"].Value.ToString(),
                        Remove = false,
                        OnlineProvisioned = false
                    };

                    apps.Add(tmp);
                }
            }
        }

        public void LoadAppxOnline()
        {
            InitApps();
            using PowerShell psInstance = PowerShell.Create();
            psInstance.AddScript($"Get-AppxProvisionedPackage -Online");
            IEnumerable<PSObject> results = psInstance.Invoke();
            foreach (PSObject result in results)
            {
                Appx tmp = new Appx
                {
                    Name = $"(Online) {result.Properties["DisplayName"].Value}",
                    FullName = result.Properties["PackageName"].Value.ToString(),
                    Remove = false,
                    OnlineProvisioned = true
                };
                apps.Add(tmp);
            }
        }
    }
}
