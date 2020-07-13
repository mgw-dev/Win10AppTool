using System;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using ModernWpf.Controls;
using Win10AppTool.Classes;
using Win10AppTool.ViewModel;

namespace Win10AppTool
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        public MainWindow()
        {
            InitializeComponent();
        }

        private AppxViewModel appxViewModel;

        private void MainAppxView_Loaded(object sender, RoutedEventArgs e)
        {
            AdminCheck();
            LoadApps();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }


        private void LoadApps()
        {
            appxViewModel = new AppxViewModel();
            appxViewModel.LoadAppx(cbAllUsers.IsChecked == true, cbExclStore.IsChecked == true);
            if (cbOnline.IsChecked == true)
            {
                appxViewModel.LoadAppxOnline(cbExclStore.IsChecked == true);
            }

            MainAppxView.DataContext = appxViewModel;

            tbCount.Text = $"Apps found: {appxViewModel.apps.Count}";
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadApps();
        }

        private void btnSelAll_Click(object sender, RoutedEventArgs e)
        {
            if (appxViewModel == null) return;
            foreach (Appx app in appxViewModel.apps)
            {
                app.Remove = true;
            }
        }

        private void btnInvSel_Click(object sender, RoutedEventArgs e)
        {
            if (appxViewModel == null) return;
            foreach (Appx app in appxViewModel.apps)
            {
                app.Remove = !app.Remove;
            }
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
                PSRunner.RemoveAppx(appxViewModel.apps);
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
    }
}
