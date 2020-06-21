using System;
using System.Collections.Generic;
using System.Management.Automation;
using Win10AppTool.Model;

namespace Win10AppTool
{
    public static class PSRunner
    {
        /// <summary>
        /// Remove AppxPackages
        /// </summary>
        /// <param name="apps">Collection of packages to remove</param>
        /// <param name="allUsers">Remove for all users</param>
        public static void RemoveAppx(IEnumerable<Appx> apps, bool allUsers)
        {
            using PowerShell psInstance = PowerShell.Create();
            foreach (Appx appx in apps)
            {
                string comm = GetRemovalCommand(appx, allUsers);
                psInstance.AddScript(comm);
            }
            psInstance.Invoke();
        }

        /// <summary>
        /// Determine what PowerShell command to use
        /// </summary>
        /// <param name="app"></param>
        /// <param name="allUsers"></param>
        /// <returns></returns>
        private static string GetRemovalCommand(Appx app, bool allUsers) =>
            (app.Remove, app.OnlineProvisioned, allUsers) switch
            {
                (false, _, _) => "",
                (true, true, _) => $"Remove-AppxProvisionedPackage {app.FullName} -Online",
                (true, false, false) => $"Remove-AppxPackage -Package {app.FullName}",
                (true, false, true) => $"Remove-AppxPackage -Package {app.FullName} -AllUsers"
            };

        public static IEnumerable<Appx> LoadAppx(bool? allUsers)
        {
            List<Appx> apps = new List<Appx>();
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
            apps.Sort();
            return apps;
        }

        public static IEnumerable<Appx> LoadAppxOnline()
        {
            List<Appx> apps = new List<Appx>();
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
            apps.Sort();
            return apps;
        }

    }
}
