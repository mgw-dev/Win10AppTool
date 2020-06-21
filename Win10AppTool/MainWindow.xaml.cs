using System.Windows;
using Win10AppTool.Model;
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
            LoadApps();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void LoadApps()
        {
            appxViewModel = new AppxViewModel();
            appxViewModel.LoadAppx(cbAllUsers.IsChecked);
            if (cbOnline.IsChecked == true)
            {
                appxViewModel.LoadAppxOnline();
            }

            MainAppxView.DataContext = appxViewModel;
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

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            PSRunner.RemoveAppx(appxViewModel.apps, cbAllUsers.IsChecked == true);
            LoadApps();
        }
    }
}
