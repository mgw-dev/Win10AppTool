using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Linq;

namespace Win10AppTool.Classes
{
    public class AppxPackage : WindowsApp
    {
        private string fullName;
        private string installLocation;
        private bool onlineProvisioned;

        public AppxPackage()
        {

        }

        public AppxPackage(PSObject psObject)
        {
            // Some error checking
            string[] props = {"FullName", "Name", "InstallLocation", "OnlineProvisioned" };
            foreach (string property in props)
            {
                if (psObject.Properties.Match(property).Count <= 0)
                {
                    throw new Exception($"PSObject is missing {property} property");
                }
            }

            FullName = psObject.Properties["FullName"].Value.ToString();
            Name = psObject.Properties["Name"].Value.ToString();
            InstallLocation = psObject.Properties["InstallLocation"].Value.ToString();
            OnlineProvisioned = Convert.ToBoolean(psObject.Properties["OnlineProvisioned"].Value.ToString());
            Remove = false;

            LoadXML();
        }

        public string FullName
        {
            get => fullName;
            set
            {
                fullName = value;
                OnPropertyChanged(nameof(FullName));
            }
        }

        public string InstallLocation
        {
            get => installLocation;
            set
            {
                installLocation = value;
                OnPropertyChanged(nameof(InstallLocation));
            }
        }


        public bool OnlineProvisioned
        {
            get => onlineProvisioned;
            set
            {
                onlineProvisioned = value;
                OnPropertyChanged(nameof(OnlineProvisioned));
            }
        }

        /// <summary>
        /// Loads information like more user-readable names and image locations from an app's AppxManifest.xml file.
        /// </summary>
        private void LoadXML()
        {
            img = new Image();
            if (!OnlineProvisioned)
            {
                string path = $"{installLocation}\\AppxManifest.xml";
                if (Directory.Exists(installLocation) && File.Exists(path))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(path);
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
                    nsmgr.AddNamespace("appx", "http://schemas.microsoft.com/appx/manifest/foundation/windows10");
                    nsmgr.AddNamespace("appx2", "http://schemas.microsoft.com/appx/2010/manifest");

                    string lPath = doc.SelectSingleNode("//appx:Logo", nsmgr)?.InnerText;
                    string friendlyName = doc.SelectSingleNode("//appx:DisplayName", nsmgr)?.InnerText;
                    lPath ??= doc.SelectSingleNode("//appx2:Logo", nsmgr)?.InnerText;
                    friendlyName ??= doc.SelectSingleNode("//appx2:DisplayName", nsmgr)?.InnerText;

                    if (friendlyName != null && !friendlyName.StartsWith("ms-resource:"))
                    {
                        name = friendlyName;
                    }
                    else
                    {
                        name = ParseName(name);
                    }
                    
                    if (lPath != null)
                    {
                        int index = lPath.LastIndexOf('\\');
                        if (index == -1)
                        {
                            img.Source = (new ImageSourceConverter()).ConvertFromString($"{installLocation}\\{lPath}") as ImageSource;
                        }
                        else
                        {
                            string searchPattern = lPath.Substring(index + 1).Replace(".", "*.");
                            img.Source = (new ImageSourceConverter()).ConvertFromString(Directory.GetFiles($"{installLocation}\\{lPath.Substring(0, index)}", searchPattern)[0]) as ImageSource;
                        }
                    }
                    else // No logo found, use placeholder.
                    {
                        img.Source = (new ImageSourceConverter()).ConvertFromString("pack://application:,,,/Win10AppTool;component/Resources/Image.png") as ImageSource;
                    }
                }
            }
            else // Online apps do include an icon to load. So a placeholder is used.
            {
                img.Source = (new ImageSourceConverter()).ConvertFromString("pack://application:,,,/Win10AppTool;component/Resources/Cloud.png") as ImageSource;
                name = ParseName(name);
            }
        }

        private string ParseName(string oldName)
        {
            string newName = oldName.Replace("Microsoft.", "").Replace("Windows.", "");
            newName = System.Text.RegularExpressions.Regex.Replace(newName, "(^[a-z]+|[A-Z]+(?![a-z])|[A-Z][a-z]+)", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
            return newName;
        }

    }
}
