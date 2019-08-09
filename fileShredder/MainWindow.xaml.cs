using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;

namespace fileShredder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {


        page.version_check page_versioncheck;
        page.main page_main;

        public MainWindow()
        {
            //初始化物件
            InitializeComponent();

            page_versioncheck = new page.version_check(this);
            page_main = new page.main(this);

            this.Content = page_main;
        }

        private void Btn_menu_about_Click(object sender, RoutedEventArgs e)
        {
            if(page_versioncheck.status_icon.Visibility != Visibility.Visible) page_versioncheck.getVersion();
            this.Content = page_versioncheck;
            wflyout.IsOpen = false;
        }

        private void Btn_menu_main_Click(object sender, RoutedEventArgs e)
        {
            this.Content = page_main;
            wflyout.IsOpen = false;
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
