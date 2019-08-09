using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
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
using Newtonsoft.Json;

namespace fileShredder.page
{
    /// <summary>
    /// Interaction logic for version_check.xaml
    /// </summary>
    public partial class version_check : Page
    {

        private MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;

        private string URL_updatefile = "https://example.com/update.zip"; //更新檔案網路位置
        private string URL_version = "https://example.com/version.json"; //版本json資訊
        private string updatefile_name = "update.zip"; //下載後的檔案名稱

        public version_check(Window form)
        {
            InitializeComponent();
            this.Width = double.NaN;
            this.Height = double.NaN;
            btn_getNewVersion.Visibility = Visibility.Hidden;
            status_icon.Visibility = Visibility.Hidden;
            pbar.Visibility = Visibility.Hidden;
        }

        private void change_status_icon(Visibility visibility, MahApps.Metro.IconPacks.PackIconFontAwesomeKind icon, Brush color)
        {
            status_icon.Foreground = color;
            status_icon.Kind = icon;
            status_icon.Visibility = visibility;
        }

        public void getVersion()
        {
            if (URL_version == "") {
                pRing.Visibility = Visibility.Hidden;
                change_status_icon(Visibility.Visible, MahApps.Metro.IconPacks.PackIconFontAwesomeKind.TimesSolid, Brushes.Red);
                btn_checkstatus.Content = "無法取得更新資訊";
                return;
            }
            Thread thread = new Thread(delegate ()
            {
                this.Dispatcher.Invoke(delegate
                {
                    pRing.Visibility = Visibility.Visible;
                    status_icon.Visibility = Visibility.Hidden;
                    btn_checkstatus.Content = "正在取得資訊";
                });
                string jsonString = "";
                Thread.Sleep(1000);
                bool isErr = false;
                using (WebClient client = new WebClient())
                {
                    try
                    {
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        jsonString = client.DownloadString(URL_version);
                        
                        //jsonString = "{\"version\": \"1.1\"}";
                    }
                    catch
                    {
                        this.Dispatcher.Invoke(delegate
                        {
                            change_status_icon(Visibility.Visible, MahApps.Metro.IconPacks.PackIconFontAwesomeKind.TimesSolid, Brushes.Red);
                            pRing.Visibility = Visibility.Hidden;
                            btn_checkstatus.Content = "取得版本資訊時發生錯誤";
                            isErr = true;
                        });
                    }
                }

                if (!isErr)
                {
                    Dictionary<string, string> jsonObject = new Dictionary<string, string>();
                    try
                    {
                        jsonObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);

                    }
                    catch {
                        isErr = true;
                    }

                    if (jsonObject.ContainsKey("version") && !isErr)
                    {
                        if (jsonObject["version"] != "1.0")
                        {
                            if (jsonObject.ContainsKey("fileurl"))
                            {
                                this.Dispatcher.Invoke(delegate
                                {
                                    URL_updatefile = jsonObject["fileurl"];
                                    btn_getNewVersion.Visibility = Visibility.Visible;
                                    btn_getNewVersion.IsEnabled = true;
                                });
                                if (jsonObject.ContainsKey("filename"))
                                {
                                    updatefile_name = jsonObject["filename"];
                                }
                            }
                            else
                            {
                                this.Dispatcher.Invoke(delegate
                                {
                                    btn_getNewVersion.Visibility = Visibility.Visible;
                                    if (URL_updatefile == "") btn_getNewVersion.IsEnabled = false;
                                });
                            }

                            this.Dispatcher.Invoke(delegate
                            {
                                change_status_icon(Visibility.Visible, MahApps.Metro.IconPacks.PackIconFontAwesomeKind.ExclamationTriangleSolid, Brushes.Yellow);
                                pRing.Visibility = Visibility.Hidden;
                                btn_checkstatus.Content = "發現新版本";
                            });
                        }
                        else
                        {
                            this.Dispatcher.Invoke(delegate
                            {
                                change_status_icon(Visibility.Visible, MahApps.Metro.IconPacks.PackIconFontAwesomeKind.CheckSolid, Brushes.LimeGreen);
                                pRing.Visibility = Visibility.Hidden;
                                btn_checkstatus.Content = "沒有更新";
                            });
                        }
                    }
                    else
                    {
                        this.Dispatcher.Invoke(delegate
                        {
                            pRing.Visibility = Visibility.Hidden;
                            change_status_icon(Visibility.Visible, MahApps.Metro.IconPacks.PackIconFontAwesomeKind.TimesSolid, Brushes.Red);
                            btn_checkstatus.Content = "無法取得版本";
                        });
                    }
                }
            });

            thread.Start();
        }

        public void DownloadFile(Uri address)
        {
            using (var client = new WebClient())
            using (var stream = client.OpenRead(address))
            using (var file = File.Create(@".\" + updatefile_name))
            {
                Int64 bytes_total = Convert.ToInt64(client.ResponseHeaders["Content-Length"]);
                var buffer = new byte[4096];
                int bytesReceived = 0;
                Int64 currentbytes = 0;
                this.Dispatcher.Invoke(delegate
                {
                    btn_checkstatus.Content = "正在下載...";
                });
                while ((bytesReceived = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    this.Dispatcher.Invoke(delegate
                    {
                        currentbytes += bytesReceived;
                        pbar.Value = (int)( (float)((currentbytes == 0 ? 1 : currentbytes) / (float)(bytes_total == 0 ? 1 : bytes_total)) * 100 );
                    });
                    file.Write(buffer, 0, bytesReceived);
                }
                this.Dispatcher.Invoke(delegate
                {
                    btn_checkstatus.Content = "下載完成";
                    btn_getNewVersion.IsEnabled = true;
                    pbar.Visibility = Visibility.Hidden;
                    btn_getNewVersion.Content = "檢視檔案";


                    btn_getNewVersion.Click -= Btn_getNewVersion_Click;
                    btn_getNewVersion.Click += delegate {
                        Process.Start("explorer.exe", " /select, " + Environment.CurrentDirectory + "\\" + updatefile_name);
                    };
                });
            }
        }

        private void Btn_getNewVersion_Click(object sender, RoutedEventArgs e)
        {
            btn_getNewVersion.IsEnabled = false;
            change_status_icon(Visibility.Visible, MahApps.Metro.IconPacks.PackIconFontAwesomeKind.DownloadSolid, Brushes.DeepSkyBlue);
            btn_checkstatus.Content = "即將下載";
            pbar.Visibility = Visibility.Visible;
            pbar.Value = 0;
            Thread thread = new Thread(delegate ()
            {
                if (URL_updatefile != "") DownloadFile(new Uri(URL_updatefile));
            });
            thread.Start();
        }
    }
}
