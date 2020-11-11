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
using System.Text.Json.Serialization;
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
using Namotion.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        private static string GetRemovalCommand(Win32App app) =>
            (app.Remove, string.IsNullOrEmpty(app.QuietUninstallString), string.IsNullOrEmpty(app.UninstallString)) switch
            {
                (false, _, _) => string.Empty,
                (true, false, _) => app.QuietUninstallString,
                (true, true, false) => app.UninstallString,
                (true, true, true) => "!!!"
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

            // Gross hacky workaround because "Get-AppxPackage" was returning nothing after moving to .net 5
            if (command.StartsWith("Get-AppxPackage") && output.Count == 0)
            {
                string strCmdText = $"{command} | ConvertTo-Json";
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.CreateNoWindow = true;
                startInfo.FileName = "powershell.exe";
                startInfo.RedirectStandardOutput = true;
                startInfo.Arguments = strCmdText;
                process.StartInfo = startInfo;
                process.Start();
                string jsonOut = process.StandardOutput.ReadToEnd().Replace("\r\n", "");
                foreach (JToken jt in JArray.Parse(jsonOut))
                {
                    PSObject pso = new PSObject();
                    pso.Properties.Add(new PSVariableProperty(new PSVariable("FullName", jt["FullName"])));
                    pso.Properties.Add(new PSVariableProperty(new PSVariable("Name", jt["Name"])));
                    pso.Properties.Add(new PSVariableProperty(new PSVariable("InstallLocation", jt["InstallLocation"])));
                    pso.Properties.Add(new PSVariableProperty(new PSVariable("OnlineProvisioned", (bool)jt["OnlineProvisioned"])));
                    output.Add(pso);
                }

            }

            return output;
        }

        public static IEnumerable<Win32App> LoadWin32Apps()
        {
            List<Win32App> output = new List<Win32App>();
            output.AddRange(LoadAppsFromRegistry(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", RegistryHive.CurrentUser));
            output.AddRange(LoadAppsFromRegistry(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", RegistryHive.LocalMachine));
            output.AddRange(LoadAppsFromRegistry(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall", RegistryHive.LocalMachine));

            return output;
        }

        private static IEnumerable<Win32App> LoadAppsFromRegistry(string registryKey, RegistryHive hive)
        {
            List<Win32App> apps = new List<Win32App>();
            RegistryKey key;
            switch (hive)
            {
                case RegistryHive.CurrentUser:
                    key = Registry.CurrentUser.OpenSubKey(registryKey);
                    break;
                case RegistryHive.LocalMachine:
                    key = Registry.LocalMachine.OpenSubKey(registryKey);
                    break;

                case RegistryHive.ClassesRoot:
                case RegistryHive.Users:
                case RegistryHive.PerformanceData:
                case RegistryHive.CurrentConfig:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException(nameof(hive), hive, null);
            }

            foreach (string subkeyName in key.GetSubKeyNames())
            {
                using RegistryKey subKey = key.OpenSubKey(subkeyName);
                Win32App w32App = new Win32App();
                if (visible(subKey))
                {
                    w32App.Name = subKey?.GetValue("DisplayName").ToString();
                    w32App.Remove = false;
                    w32App.Img = new Image();

                    w32App.UninstallString = subKey?.GetValue("UninstallString")?.ToString();
                    w32App.QuietUninstallString = subKey?.GetValue("QuietUninstallString")?.ToString();

                    Icon icon = new Icon(SystemIcons.WinLogo, 64, 64);
                    string displayIcon = (subKey?.GetValue("DisplayIcon") ?? string.Empty).ToString();
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
            bool visible(RegistryKey subKey)
            {
                string name = (string)subKey.GetValue("DisplayName");
                string releaseType = (string)subKey.GetValue("ReleaseType");
                object systemComponent = subKey.GetValue("SystemComponent");
                string parentName = (string)subKey.GetValue("ParentDisplayName");
                return !string.IsNullOrEmpty(name) && string.IsNullOrEmpty(releaseType) && string.IsNullOrEmpty(parentName) && (systemComponent == null);
            }
        }

        public static async Task RemoveWin32(IEnumerable<Win32App> apps)
        {
            await Task.Run(() =>
            {
                foreach (Win32App app in apps)
                {
                    string c = GetRemovalCommand(app);
                    if (!string.IsNullOrEmpty(c))
                    {
                        if (c == "!!!")
                        {

                        }
                        else
                        {
                            Debug.WriteLine(c);
                        }
                        //RunPsCommand(c);
                    }
                }
            });
        }

    }
}
