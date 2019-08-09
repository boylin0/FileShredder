using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace fileShredder.page
{
    /// <summary>
    /// Interaction logic for main.xaml
    /// </summary>
    public partial class main : Page
    {
        private MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;

        public main(Window form)
        {
            InitializeComponent();
            currentfileLabel.Visibility = Visibility.Hidden;
            percentageLabel.Content = "0%";
            percentageLabel.Visibility = Visibility.Hidden;
            pbar.Visibility = Visibility.Hidden;
            this.Width = double.NaN;
            this.Height = double.NaN;

            //附加命令參數處理
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 1; i < args.Length; i++)
            {
                queuefile_add(args[i]);
            }
        }

        List<string> pathlist = new List<string>(); //列表檔案實際路徑

        /** 
         * 新增檔案函數
         * @desc 對列表新增檔案並處理
         * @param filepath 檔案路徑 
         */
        void queuefile_add(string filepath)
        {
            if (System.IO.File.Exists(filepath)) //判斷檔案是否存在
            {
                if (!pathlist.Contains(filepath)) //判斷是否已存在列表
                {
                    pathlist.Add(filepath);
                    string filename = System.IO.Path.GetFileName(filepath);
                    ListViewItem litem = new ListViewItem();
                    litem.Content = filename;
                    queuelist.Items.Add(filename);
                }
            }
        }

        private void pickfile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog choofdlog = new OpenFileDialog();
            choofdlog.Filter = "All Files (*.*)|*.*";
            choofdlog.FilterIndex = 1;
            choofdlog.Multiselect = true;

            if (choofdlog.ShowDialog() == true)
            {
                string sFileName = choofdlog.FileName;
                string[] arrAllFiles = choofdlog.FileNames;
                foreach (string str in arrAllFiles)
                {
                    queuefile_add(str);
                }
            }
        }

        private void StartRemove_Click(object sender, RoutedEventArgs e)
        {
            if (pathlist.Count == 0)
            {
                //建立非同步函數以顯示訊息防止執行緒堵塞
                Func<Task> testFunc = async () =>
                {
                    await mainWindow.ShowMessageAsync("提示", "請選擇檔案，列表中沒有檔案！！");
                };
                testFunc.Invoke();
            }
            else
            {
                //建立新執行緒處理檔案
                Thread t1 = new Thread(() => thread_startremove(7));
                t1.Start();
            }
        }

        private void CleanList_Click(object sender, RoutedEventArgs e)
        {
            queuelist.Items.Clear();
            pathlist.Clear();
        }

        /**
         * 移除檔案執行緒函數
         * @desc 執行緒執行完整刪除檔案函數
         */
        private void thread_startremove(int overrideCount)
        {
            //顯示刪除檔案畫面
            this.Dispatcher.Invoke(delegate
            {
                tab_shredFile.IsEnabled = false;
                tab_shredFile.Opacity = 0;
                currentfileLabel.Content = "初始化中...";
                currentfileLabel.Visibility = Visibility.Visible;
                percentageLabel.Content = "0%";
                percentageLabel.Visibility = Visibility.Visible;
                pbar.Percentage = 0;
                pbar.Visibility = Visibility.Visible;
                tab_settings.Visibility = Visibility.Hidden;
                tab_settings.IsEnabled = false;
            });

            shredder shred = new shredder(); //建立刪除檔案Class
            shred.ShredMultiple(pathlist, overrideCount, delegate (string filename, bool isend)
            {
                //處理狀態回傳判斷
                if (!isend)
                {
                    this.Dispatcher.Invoke(delegate
                    {
                        byte percent = 0;
                        if (shred.currentPosition == 0.0f && shred.totalsize == 0.0f)
                        {
                            percent = 100;
                        }
                        else
                        {
                            percent = (byte)(((float)shred.currentPosition / shred.totalsize) * 100);
                        }

                        pbar.Percentage = (int)(percent);
                        //Debug.WriteLine("{0:0.0000####},{1:0.0000####},{2:0.0000####}", shred.currentPosition, shred.totalsize, (float)shred.currentPosition / shred.totalsize * 100 );


                        percentageLabel.Content = (percent.ToString() + "%");
                        currentfileLabel.Content = "處理檔案中: " + filename;
                    });
                }
                else
                {
                    this.Dispatcher.Invoke(delegate
                    {
                        currentfileLabel.Visibility = Visibility.Hidden;
                        currentfileLabel.Content = "";
                        percentageLabel.Content = "0%";
                        percentageLabel.Visibility = Visibility.Hidden;
                        pbar.Visibility = Visibility.Hidden;
                        tab_settings.Visibility = Visibility.Visible;
                        tab_settings.IsEnabled = true;
                        tab_shredFile.IsEnabled = true;
                        tab_shredFile.Opacity = 100;
                        Func<Task> testFunc = async () =>
                        {
                            await mainWindow.ShowMessageAsync("提示", "檔案處理完成!!");
                        };
                        testFunc.Invoke();
                        queuelist.Items.Clear();
                        pathlist.Clear();
                    });
                }
            });

        }

        private void Checkbox_systemrightmenu_Click(object sender, RoutedEventArgs e)
        {

        }

        //程式碼測試按鈕
        private void Testbutton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(queuelist.SelectedItems.Count.ToString());
        }

        //檔案拖曳處理
        private void Queuelist_Drop(object sender, DragEventArgs e)
        {
            List<string> filepaths = new List<string>();
            foreach (var s in (string[])e.Data.GetData(DataFormats.FileDrop, false))
            {
                if (Directory.Exists(s))
                {
                    //Add files from folder
                    foreach (string spath in Directory.GetFiles(s, "*.*", SearchOption.AllDirectories))
                    {
                        queuefile_add(spath);
                    }
                }
                else
                {
                    queuefile_add(s);
                }
            }
        }



    }
}
