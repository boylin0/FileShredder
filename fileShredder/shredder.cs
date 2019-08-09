using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace fileShredder
{
    class shredder
    {
        public float ProgressRate = 0; //處理進度
        public long totalsize = 0; //總需要處理的大小 (bytes)
        public long currentPosition = 0; //當前處理到的檔案位置

        //回傳函數定義
        public delegate void shredCalback(string currentfile, bool isend);

        /**
         * 完整移除檔案函數
         * @desc 混淆檔案內容及混淆檔案名稱
         * @param filepaths 檔案路徑
         * @param writecount 覆寫次數
         * @param callback 回傳狀態
         */
        public void ShredMultiple(List<string> filepaths, int writeCount, shredCalback callback)
        {
            //將總大小及位置設為零
            totalsize = 0;
            currentPosition = 0;

            //計算檔案總大小並從列表中移除不存在的檔案
            for (int index = 0; index < filepaths.Count; index++)
            {
                if (File.Exists(filepaths[index]))
                {
                    //計算總大小
                    totalsize += new System.IO.FileInfo(filepaths[index]).Length;
                }
                else
                {
                    //移除不存在的檔案
                    filepaths.RemoveAt(index);
                }
            }

            //原始檔案大小 x 重複寫入n次 = 總共需要寫入 n bytes
            totalsize *= writeCount;
            Random rnd = new Random(); //建立隨機函數類別

            //歷遍所有檔案路徑並處理檔案
            for (int index = 0; index < filepaths.Count; index++)
            {
                long filesize = new System.IO.FileInfo(filepaths[index]).Length; //當前處理檔案的大小
                string filepath = filepaths[index]; //當前處理檔案路徑
                string filename = System.IO.Path.GetFileName(filepath); //當前處理檔案名稱

                //覆寫檔案N次
                for (int i = 0; i < writeCount; i++)
                {
                    //定義頭檔 (自行定義或採用隨機變數)
                    byte[] headBytes =
                    {
                        (byte)( (byte)rnd.Next(0, 255) + (i % 256)),
                        (byte)( (byte)rnd.Next(0, 255) + (i % 256)),
                        (byte)( (byte)rnd.Next(0, 255) + (i % 256)),
                        (byte)( (byte)rnd.Next(0, 255) + (i % 256)),
                        (byte)( (byte)rnd.Next(0, 255) + (i % 256)),
                    };

                    //開起檔案並寫入檔案
                    using (FileStream fstream = new FileStream(filepath, FileMode.Open))
                    {
                        //定位檔案到頭部
                        fstream.Seek(0, SeekOrigin.Begin);

                        //寫入頭檔
                        for (int writeHeadCount = 0; writeHeadCount < headBytes.Length; writeHeadCount++)
                        {
                            fstream.WriteByte((byte)headBytes[writeHeadCount]);
                        }

                        //混淆內容
                        for (int j = 0; j < filesize - headBytes.Length; j++)
                        {
                            fstream.WriteByte((byte)rnd.Next(0, 255));
                        }

                        //清除緩衝並寫入檔案
                        fstream.Flush();
                    }

                    //開啟檔案並讀取判斷標頭是否已寫入 (判斷失敗則無窮迴圈!!)
                    using (FileStream fstream = new FileStream(filepath, FileMode.Open))
                    {
                        for (int index_head = 0; index_head < headBytes.Length; index_head++) {
                            fstream.Seek(index_head, SeekOrigin.Begin);
                            if ( (byte)fstream.ReadByte()  != headBytes[index_head] ) index_head--;
                        }
                    }

                    
                    currentPosition += filesize; //計算當前處理到的檔案位置
                    callback(filename, false); //回傳當前狀態
                }



            }


            callback("正在完成...", false); //回傳當前狀態

            System.Threading.Thread.Sleep(2000); //等候並開始混淆檔案名稱

            //產生隨機檔名
            List<string> newNames = new List<string>();
            for (int index = 0; index < filepaths.Count; index++)
            {
                newNames.Add(Path.GetRandomFileName());
            }

            //對檔案重新命名
            RenameFileMultiple(filepaths, newNames, filepaths.Count); 


            //將檔名加上路徑
            for (int index = 0; index < filepaths.Count; index++)
            {
                newNames[index] = Path.GetDirectoryName(filepaths[index]) + "\\" + newNames[index];
            }

            //刪除檔案
            RemoveFileMultiple(newNames);

            //回傳已完成
            callback("", true); 
        }

        private void RemoveFileMultiple(List<string> filepaths)
        {
            for (int index = 0; index < filepaths.Count; index++)
            {
                File.Delete(filepaths[index]);
            }
            /*
            //開啟命令提示字元
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            //曆遍檔案路徑並移除
            for (int index = 0; index < filepaths.Count; index++)
            {
                process.StandardInput.WriteLine("del /f \"" + filepaths[index] + "\"");
            }

            //結束命令提示字元
            process.StandardInput.WriteLine("exit");
            process.WaitForExit();
            process.Close();
            */
        }

        private void RenameFileMultiple(List<string> filepaths, List<string> names, int length)
        {
            for (int index = 0; index < length; index++)
            {
                File.Move(filepaths[index], Path.GetDirectoryName(filepaths[index]) + "\\" + names[index]);
            }
            /*
            //開啟命令提示字元
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            //曆遍檔案路徑並重新命名
            for (int index = 0; index < length; index++)
            {
                process.StandardInput.WriteLine("ren \"" + filepaths[index] + "\" \"" + names[index] + "\"");
            }

            //結束命令提示字元
            process.StandardInput.WriteLine("exit");
            process.WaitForExit();
            process.Close();
            */

        }


    }
}