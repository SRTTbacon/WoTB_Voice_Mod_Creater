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
            Stream stream = Voice_Set.FTP_Server.OpenRead(From_File);
            StreamReader str = new StreamReader(stream);
            string Temp = str.ReadToEnd();
            str.Close();
            stream.Close();
            stream.Dispose();
            return Temp;
        }
        //サーバー内のファイルを直接読み込む(改行で分ける)
        public static string[] Server_Open_File_Line(string From_File)
        {
            List<string> Temp = new List<string>();
            Stream stream = Voice_Set.FTP_Server.OpenRead(From_File);
            StreamReader str = new StreamReader(stream);
            while (str.EndOfStream == false)
            {
                Temp.Add(str.ReadLine());
            }
            str.Close();
            stream.Close();
            stream.Dispose();
            return Temp.ToArray();
        }
    }
}