using System;
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

        public string DelText
        {
            get => delText;
            set
            {
                delText = value;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private AppxViewModel appxViewModel;
        private Win32AppViewModel win32AppViewModel;

        private void MainAppxView_Loaded(object sender, RoutedEventArgs e)
        {
            
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
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SetDelButtonText();
        }

        private void SetDelButtonText()
        {
            int selectedCount = appxViewModel.apps.AsEnumerable().Count(app => app.Remove) + win32AppViewModel.apps.AsEnumerable().Count(app => app.Remove);
            DelText = $"Delete {(selectedCount == 0 ? string.Empty : selectedCount + " ")}App{(selectedCount == 1 ? string.Empty : "s")}";
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
                await ApplicationHelper.RemoveAppx(appxViewModel.apps);
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

        private void Win32View_Loaded(object sender, RoutedEventArgs e)
        {
           

        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
