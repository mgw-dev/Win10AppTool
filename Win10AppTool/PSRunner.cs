using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;
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
    }
}
