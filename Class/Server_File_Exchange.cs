using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace WoTB_Voice_Mod_Creater
{
    public class Server_File
    {
        //サーバー内のファイルを直接読み込む
        public static string Server_Open_File(string From_File)
        {
            StreamReader str = Voice_Set.FTPClient.GetFileRead(From_File);
            string Temp = str.ReadToEnd();
            str.Dispose();
            if (Temp == "" || Temp == null)
                throw new Exception("指定したファイルは存在しません。");
            return Temp;
        }
        //サーバー内のファイルを直接読み込む(改行で分ける)
        public static string[] Server_Open_File_Line(string From_File)
        {
            List<string> Temp = new List<string>();
            StreamReader stream = Voice_Set.FTPClient.GetFileRead(From_File);
            while (stream.EndOfStream == false)
                Temp.Add(stream.ReadLine());
            stream.Dispose();
            return Temp.ToArray();
        }
    }
}