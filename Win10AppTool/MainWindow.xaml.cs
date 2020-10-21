using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ModernWpf.Controls;
using Win10AppTool.Annotations;
using Win10AppTool.Classes;
using Win10AppTool.ViewModel;

namespace Win10AppTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string delText = "Delete Apps";
        private string appxHeaderText = "AppxPackage Apps";
        private string win32HeaderText = "Win32 Apps";

        public string DelText
        {
            get => delText;
            set
            {
                delText = value;
                OnPropertyChanged();
            }
        }
        public string AppxHeaderText
        {
            get => appxHeaderText;
            set
            {
                appxHeaderText = value;
                OnPropertyChanged();
            }
        }
        public string Win32HeaderText
        {
            get => win32HeaderText;
            set
            {
                win32HeaderText = value;
                OnPropertyChanged();
            }
        }

        private AppxViewModel appxViewModel;
        private Win32AppViewModel win32AppViewModel;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AdminCheck();
            LoadApps();
        }

        private void LoadApps(object sender = null, RoutedEventArgs e = null)
        {
            appxViewModel = new AppxViewModel();
            appxViewModel.LoadAppx(cbAllUsers.IsChecked == true, cbExclStore.IsChecked == true);
            if (cbOnline.IsChecked == true)
            {
                appxViewModel.LoadAppxOnline(cbExclStore.IsChecked == true);
            }
            MainAppxView.DataContext = appxViewModel;
            appxViewModel.SortApps();

            win32AppViewModel = new Win32AppViewModel();
            win32AppViewModel.LoadWin32();
            Win32View.DataContext = win32AppViewModel;
            win32AppViewModel.SortApps();

            tbCount.Text = $"Apps found: {appxViewModel.apps.Count + win32AppViewModel.apps.Count}";

            appxViewModel.PropertyChanged += ViewModel_PropertyChanged;
            win32AppViewModel.PropertyChanged += ViewModel_PropertyChanged;


            SetButtonText();
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SetButtonText();
        }

        private void SetButtonText()
        {
            int appxCount = appxViewModel.apps.AsEnumerable().Count(app => app.Remove);
            int w32Count = win32AppViewModel.apps.AsEnumerable().Count(app => app.Remove);
            int selectedCount = appxCount + w32Count;
            DelText = $"Delete {(selectedCount == 0 ? string.Empty : selectedCount + " ")}App{(selectedCount == 1 ? string.Empty : "s")}";

            AppxHeaderText = $"AppxPackage Apps: ({appxCount}/{appxViewModel.apps.Count} selected)";
            Win32HeaderText = $"Win32 Apps: ({w32Count}/{win32AppViewModel.apps.Count} selected)";
        }

        private async void btnDel_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog cd = new ContentDialog
            {
                Title = "Warning!",
                Content = "Removing pre-installed applications may cause errors. Are you sure you want to do this?",
                PrimaryButtonText = "Yes",
                CloseButtonText = "No"
            };
            ContentDialogResult result = await cd.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await ApplicationHelper.RemoveAppx(appxViewModel.apps.Where(x => x.Remove).ToList());
                await ApplicationHelper.RemoveWin32(win32AppViewModel.apps.Where(x => x.Remove).ToList());
                LoadApps();
            }
        }

        // Disable controls if not admin
        private void AdminCheck()
        {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                cbAllUsers.IsChecked = false;
                cbOnline.IsChecked = false;

                Control[] conts = { cbAllUsers, cbOnline, btnDel };

                foreach (Control cont in conts)
                {
                    cont.IsEnabled = false;
                    cont.ToolTip = "You must run this program with administrator privileges to use this feature.";
                }
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            foreach (Win32App app in win32AppViewModel.apps)
            {
                app.Remove = false;
            }

            foreach (AppxPackage app in appxViewModel.apps)
            {
                app.Remove = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
