using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Win32;
using Image = System.Windows.Controls.Image;

namespace Win10AppTool.Classes
{
    public static class ApplicationHelper
    {
        #region Appx Stuff

        /// <summary>
        /// Remove AppxPackages
        /// </summary>
        /// <param name="apps">Collection of packages to remove</param>
        /// <param name="allUsers">Remove for all users</param>
        public static async Task RemoveAppx(IEnumerable<AppxPackage> apps)
        {
            await Task.Run(() =>
            {
                foreach (AppxPackage app in apps)
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
        /// Uses PowerShell to get list of AppxPackage Packages.
        /// </summary>
        /// <param name="allUsers"></param>
        /// <returns></returns>
        public static IEnumerable<AppxPackage> LoadAppx(bool allUsers, bool noStore)
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

            return RunPsCommand(argsBuilder.ToString()).Select(obj => new AppxPackage(obj)).ToList();
        }

        /// <summary>
        /// Uses PowerShell to get list of online AppxPackage Packages.
        /// </summary>
        /// <param name="allUsers"></param>
        /// <returns></returns>
        public static IEnumerable<AppxPackage> LoadAppxOnline(bool noStore)
        {
            StringBuilder argsBuilder = new StringBuilder();
            argsBuilder.Append("Get-AppxProvisionedPackage -Online | select-object -property @{N='Name';E={$_.DisplayName}}, @{N='FullName';E={$_.PackageName}}, @{N='installLocation';E={$_.InstallLocation}}, @{N='OnlineProvisioned';E={$true}} ");
            if (noStore)
            {
                argsBuilder.Append("| Where-Object {$_.Name -NotLike '*Microsoft.WindowsStore*' -and $_.Name -NotLike '*Microsoft.StorePurchaseApp*'}");
            }

            return RunPsCommand(argsBuilder.ToString()).Select(obj => new AppxPackage(obj)).ToList();
        }
        #endregion


        /// <summary>
        /// Determine what PowerShell command to use
        /// </summary>
        /// <param name="app"></param>
        /// <param name="allUsers"></param>
        /// <returns></returns>
        private static string GetRemovalCommand(AppxPackage app) =>
            (app.Remove, app.OnlineProvisioned) switch
            {
                (false, _) => "",
                (true, true) => $"Remove-AppxProvisionedPackage {app.FullName} -Online",
                (true, false) => $"Remove-AppxPackage {app.FullName}"
            };

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
            return output;
        }

        public static IEnumerable<Win32App> LoadWin32Apps()
        {
            List<Win32App> apps = new List<Win32App>();
            string registry_key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            using RegistryKey key = Registry.LocalMachine.OpenSubKey(registry_key);
            foreach (string subkey_name in key.GetSubKeyNames())
            {
                using RegistryKey subkey = key.OpenSubKey(subkey_name);
                Win32App w32App = new Win32App();
                object displayName = subkey.GetValue("DisplayName");
                if (displayName != null)
                {
                    w32App.Name = displayName.ToString();
                    w32App.Remove = false;
                    w32App.Img = new Image();

                    Icon icon = new Icon(SystemIcons.Application, 64, 64);
                    string displayIcon = (subkey.GetValue("DisplayIcon") ?? string.Empty).ToString();
                    string exeMatch = Regex.Match(displayIcon ?? string.Empty, @"[A-Z]:.+\.exe", RegexOptions.IgnoreCase).Value;
                    string icoMatch = Regex.Match(displayIcon ?? string.Empty, @"[A-Z]:.+\.ico", RegexOptions.IgnoreCase).Value;

                    if (!string.IsNullOrEmpty(exeMatch))
                    {
                        icon = Icon.ExtractAssociatedIcon(exeMatch);
                    }

                    if (!string.IsNullOrEmpty(icoMatch))
                    {
                        icon = new Icon(icoMatch);
                    }


                    w32App.Img.Source = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());


                    apps.Add(w32App);
                }
            }



            return apps;
        }

    }
}
