using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Linq;

namespace Win10AppTool.Classes
{
    public class Appx : INotifyPropertyChanged
    {
        private string name;
        private string fullName;
        private string installLocation;
        private Image img;
        private bool onlineProvisioned;
        private bool remove;

        [JsonPropertyName("Name")]
        public string Name
        {
            get => OnlineProvisioned ? "(Online) " + name : name;
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }

        [JsonPropertyName("FullName")]
        public string FullName
        {
            get => fullName;
            set
            {
                fullName = value;
                OnPropertyChanged("FullName");
            }
        }


        [JsonPropertyName("InstallLocation")]
        public string InstallLocation
        {
            get => installLocation;
            set
            {
                installLocation = value;
                OnPropertyChanged("InstallLocation");
            }
        }

        public bool Remove
        {
            get => remove;
            set
            {
                remove = value;
                OnPropertyChanged("Remove");
            }
        }

        [JsonPropertyName("OnlineProvisioned")]
        public bool OnlineProvisioned
        {
            get => onlineProvisioned;
            set
            {
                onlineProvisioned = value;
                OnPropertyChanged("OnlineProvisioned");
            }
        }

        public Image Img
        {
            get => img;
            set
            {
                img = value;
                OnPropertyChanged("Img");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Loads information like more user-readable names and image locations from an app's AppxManifest.xml file.
        /// </summary>
        public void LoadXML()
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
