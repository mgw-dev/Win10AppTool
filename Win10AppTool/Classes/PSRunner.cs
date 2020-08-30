using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Management.Automation;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;

namespace Win10AppTool.Classes
{
    public static class PSRunner
    {
        /// <summary>
        /// Remove AppxPackages
        /// </summary>
        /// <param name="apps">Collection of packages to remove</param>
        /// <param name="allUsers">Remove for all users</param>
        public static async Task RemoveAppx(IEnumerable<Appx> apps)
        {
            await Task.Run(() => {
                foreach (Appx app in apps)
                {
                    string c = GetRemovalCommand(app);
                    if (!string.IsNullOrEmpty(c))
                    {
                        RunPsCommand(c);
                    }
                }
            });
        }

        /// <summary>
        /// Determine what PowerShell command to use
        /// </summary>
        /// <param name="app"></param>
        /// <param name="allUsers"></param>
        /// <returns></returns>
        private static string GetRemovalCommand(Appx app) =>
            (app.Remove, app.OnlineProvisioned) switch
            {
                (false, _) => "",
                (true, true) => $"Remove-AppxProvisionedPackage {app.FullName} -Online",
                (true, false) => $"Remove-AppxPackage {app.FullName}"
            };

        /// <summary>
        /// Uses PowerShell to get list of Appx Packages.
        /// </summary>
        /// <param name="allUsers"></param>
        /// <returns></returns>
        public static IEnumerable<Appx> LoadAppx(bool allUsers, bool noStore)
        {
            StringBuilder argsBuilder = new StringBuilder();
            argsBuilder.Append("Get-AppxPackage");
            if (allUsers)
            {
                argsBuilder.Append(" -AllUsers");
            }

            argsBuilder.Append(" | Where-Object {$_.IsFramework -Match 'false' -and $_.NonRemovable -Match 'false'} | select-object -property @{N='Name';E={$_.Name}}, @{N='FullName';E={$_.PackageFullName}}, @{N='InstallLocation';E={$_.InstallLocation}}, @{N='OnlineProvisioned';E={$false}} ");
            if (noStore)
            {
                argsBuilder.Append("| Where-Object {$_.Name -NotLike '*Microsoft.WindowsStore*' -and $_.Name -NotLike '*Microsoft.StorePurchaseApp*'}");
            }

            return RunPsCommand(argsBuilder.ToString()).Select(obj => new Appx(obj)).ToList();
        }

        /// <summary>
        /// Uses PowerShell to get list of online Appx Packages.
        /// </summary>
        /// <param name="allUsers"></param>
        /// <returns></returns>
        public static IEnumerable<Appx> LoadAppxOnline(bool noStore)
        {
            StringBuilder argsBuilder = new StringBuilder();
            argsBuilder.Append("Get-AppxProvisionedPackage -Online | select-object -property @{N='Name';E={$_.DisplayName}}, @{N='FullName';E={$_.PackageName}}, @{N='installLocation';E={$_.InstallLocation}}, @{N='OnlineProvisioned';E={$true}} ");
            if (noStore)
            {
                argsBuilder.Append("| Where-Object {$_.Name -NotLike '*Microsoft.WindowsStore*' -and $_.Name -NotLike '*Microsoft.StorePurchaseApp*'}");
            }

            return RunPsCommand(argsBuilder.ToString()).Select(obj => new Appx(obj)).ToList();
        }

        /// <summary>
        /// Runs a PowerShell command. 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private static Collection<PSObject> RunPsCommand(string command)
        {
            PowerShell ps = PowerShell.Create();
            ps.AddScript(command);
            Collection<PSObject> output = ps.Invoke();
            ps.Streams.Progress.DataAdded += (sender, eventargs) => {
                PSDataCollection<ProgressRecord> progressRecords = (PSDataCollection<ProgressRecord>)sender;
                Debug.WriteLine("Progress is {0} percent complete", progressRecords[eventargs.Index].PercentComplete);
            };
            return output;
        }
    }
}
