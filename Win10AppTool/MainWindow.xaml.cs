using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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

        private void AppxView_Loaded(object sender, RoutedEventArgs e)
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

            AppxView.DataContext = appxViewModel;
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadApps();
        }

        private void btnSelAll_Click(object sender, RoutedEventArgs e)
        {
            if (appxViewModel != null)
            {
                for (int i = 0; i < appxViewModel.apps.Count; i++)
                {
                    appxViewModel.apps[i].Remove = true;
                }
            }
        }

        private void btnInvSel_Click(object sender, RoutedEventArgs e)
        {
            if (appxViewModel != null)
            {
                foreach (var appx in appxViewModel.apps)
                {
                    appx.Remove = !appx.Remove;
                }
            }
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            PSRunner.RemoveAppx(appxViewModel.apps, cbAllUsers.IsChecked == true);
            LoadApps();
        }
    }
}
