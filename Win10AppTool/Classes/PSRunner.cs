using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Win10AppTool.Classes
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
            foreach (Appx app in apps)
            {
                string c = GetRemovalCommand(app, allUsers);
                if (!string.IsNullOrEmpty(c))
                {
                    RunPsCommand(c);
                }
            }
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
                (true, false, false) => $"Remove-AppxPackage {app.FullName}",
                (true, false, true) => $"Remove-AppxPackage {app.FullName} -AllUsers"
            };

        public static IEnumerable<Appx> LoadAppx(bool allUsers)
        {
            StringBuilder argsBuilder = new StringBuilder();
            argsBuilder.Append("Get-AppxPackage");
            if (allUsers)
            {
                argsBuilder.Append(" -AllUsers");
            }

            
            argsBuilder.Append(" | Where-Object {$_.IsFramework -Match 'false' -and $_.NonRemovable -Match 'false'} | select-object -property @{N='Name';E={$_.Name}}, @{N='FullName';E={$_.PackageFullName}}, @{N='InstallLocation';E={$_.InstallLocation}}, @{N='OnlineProvisioned';E={$false}} | ConvertTo-Json");
            string output = RunPsCommand(argsBuilder.ToString());
            if (output.Length > 0)
            {
                return JsonSerializer.Deserialize<Appx[]>(output);
            }

            return new List<Appx>();
        }

        public static IEnumerable<Appx> LoadAppxOnline()
        {
            string output = RunPsCommand("Get-AppxProvisionedPackage -Online | select-object -property @{N='Name';E={'(Online) ' + $_.DisplayName}}, @{N='FullName';E={$_.PackageName}}, @{N='installLocation';E={$_.InstallLocation}}, @{N='OnlineProvisioned';E={$true}} | ConvertTo-Json");
            if (output.Length > 0)
            {
                return JsonSerializer.Deserialize<Appx[]>(output);
            }
            return new List<Appx>();
        }

        private static string RunPsCommand(string command)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = @"powershell.exe";
            startInfo.Arguments = $"& {command}";
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            Process process = new Process {StartInfo = startInfo};
            process.Start();
            return process.StandardOutput.ReadToEnd();
        }

    }
}
