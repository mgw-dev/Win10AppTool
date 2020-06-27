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
    public class Appx : INotifyPropertyChanged, IComparable
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

        public int CompareTo(object obj)
        {
            Appx other = (Appx)obj;
            return String.CompareOrdinal(this.Name, other?.name);
        }

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
                    var nsmgr = new XmlNamespaceManager(doc.NameTable);
                    nsmgr.AddNamespace("appx", "http://schemas.microsoft.com/appx/manifest/foundation/windows10");

                    string lPath = doc.SelectSingleNode("//appx:Logo", nsmgr)?.InnerText;
                    string friendlyName = doc.SelectSingleNode("//appx:DisplayName", nsmgr)?.InnerText;

                    if (friendlyName != null && !friendlyName.StartsWith("ms-resource:"))
                    {
                        name = friendlyName;
                    }

                    if (lPath != null)
                    {
                        int index = lPath.LastIndexOf('\\');
                        string searchPath = $"{installLocation}\\{lPath.Substring(0, index)}";
                        string searchPattern = lPath.Substring(index + 1).Replace(".", "*.");
                        string imgPath = Directory.GetFiles(searchPath, searchPattern)[0];
                        img.Source = (new ImageSourceConverter()).ConvertFromString(imgPath) as ImageSource;
                    }
                    else
                    {
                        img.Source = (new ImageSourceConverter()).ConvertFromString("pack://application:,,,/Win10AppTool;component/Resources/Image.png") as ImageSource;
                    }
                }
            }
            else
            {
                img.Source = (new ImageSourceConverter()).ConvertFromString("pack://application:,,,/Win10AppTool;component/Resources/Cloud.png") as ImageSource;
            }
        }
    }
}
