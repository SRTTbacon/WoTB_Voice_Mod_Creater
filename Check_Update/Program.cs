
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Check_Update
{
    public class SRTTbacon_Server
    {
        public const string Global_IP = "srttbacon.jp";
        public const string Local_IP = "192.168.0.119";
        public static string IP = "";
        public const int Port = 50000;
    }
    public class Program
    {
        static Dictionary<string, string> Text = new Dictionary<string, string>();
        static Dictionary<string, double> Resource_Version = new Dictionary<string, double>();
        static string Local_Path = Directory.GetCurrentDirectory();
        static string Resource_Update = "";
        static TCP_Client Client = new TCP_Client();
        static int Percent = 0;
        static double TotalSize = 0;
        static double Version = 0;
        static bool isDownloaded = false;
        static byte IsCanConnectServer = 0;
        static bool IsJapanese = true;
        public static void Main(string[] args)
        {
            Random r = new Random();
            int random = r.Next(0, 5);
            Console.CursorVisible = false;
            if (IsJapanese)
            {
                if (random == 0)
                    Console.WriteLine("起動中...");
                else if (random == 1)
                    Console.WriteLine("ピーピー...キドウチュウダヨ...");
                else if (random == 2)
                    Console.WriteLine("Booting SRTT-Yuna...");
                else if (random == 3)
                    Console.WriteLine("Thanks for starting!");
                else if (random == 4)
                    Console.WriteLine("アップデートシステムを初期化中...");
            }
            else
                Console.WriteLine("Booting SRTT-Yuna...");
            Console.WriteLine();
            if (!File.Exists(Local_Path + "\\WoTB_Voice_Mod_Creater.exe") && !File.Exists(Local_Path + "\\WoTB_Mod_Creator_English.exe"))
            {
                if (IsJapanese)
                    Console.WriteLine("WoTB_Voice_Mod_Creater.exeが存在しません。キーを押すと終了します。");
                else
                    Console.WriteLine("Does not exist WoTB_Mod_Creator_English.exe. Press key to exit.");
                Console.CursorVisible = true;
                Console.ReadKey();
                return;
            }
            if (!CanDirectoryAccess(Local_Path))
            {
                if (IsJapanese)
                {
                    Console.WriteLine("フォルダにアクセスできませんでした。アクセス権限の設定を見直してください。");
                    Console.WriteLine("キーを押すと終了します。");
                }
                else
                {
                    Console.WriteLine("Could not access folder. Please review the access authority settings.");
                    Console.WriteLine("Press key to exit.");
                }
                Console.CursorVisible = true;
                Console.ReadKey();
                return;
            }
            if (IsOpenedModCreator("SRTTbacon_WoTB_Voice_Mod_Creater_V1.5") || IsOpenedModCreator("SRTTbacon_WoTB_Voice_Mod_Creater_V1.6"))
            {
                if (IsJapanese)
                {
                    Console.WriteLine("Mod Creatorが起動中です。ソフトを終了させてから起動してください。");
                    Console.WriteLine("キーを押すと終了します。");
                }
                else
                {
                    Console.WriteLine("Mod Creator is running. Terminate the software and then start it again.");
                    Console.WriteLine("Press key to exit.");
                }
                Console.CursorVisible = true;
                Console.ReadKey();
                return;
            }
            bool IsSRTTbaconMode = false;
            if (Environment.UserName == "SRTTbacaaaon")
            {
                SRTTbacon_Server.IP = SRTTbacon_Server.Local_IP;
                IsSRTTbaconMode = true;
            }
            else
                SRTTbacon_Server.IP = SRTTbacon_Server.Global_IP;
            Process Mod_Creator_Process = new Process();
            if (IsJapanese)
                Mod_Creator_Process.StartInfo.FileName = Local_Path + "\\WoTB_Voice_Mod_Creater.exe";
            else
                Mod_Creator_Process.StartInfo.FileName = Local_Path + "\\WoTB_Mod_Creator_English.exe";
            Mod_Creator_Process.StartInfo.Arguments = "/Version";
            Mod_Creator_Process.StartInfo.CreateNoWindow = true;
            Mod_Creator_Process.StartInfo.UseShellExecute = false;
            Mod_Creator_Process.Start();
            Mod_Creator_Process.WaitForExit();
            Mod_Creator_Process.Dispose();
            if (!File.Exists(Local_Path + "\\Version.dat"))
            {
                if (IsJapanese)
                    Console.WriteLine("バージョンを取得できませんでした。キーを押すと終了します。");
                else
                    Console.WriteLine("Failed to get version. Press key to exit.");
                Console.CursorVisible = true;
                Console.ReadKey();
                return;
            }
            if (File.Exists(Local_Path + "\\Resources\\Versions.dat"))
            {
                StreamReader str = Encode.Crypt_Decrypt.File_Decrypt_To_Stream(Local_Path + "\\Resources\\Versions.dat", "SRTTbacon_Resource_Versions_Crypt");
                string line;
                while ((line = str.ReadLine()) != null)
                {
                    if (line.Contains("|"))
                    {
                        double Version = double.Parse(line.Substring(0, line.IndexOf('|')));
                        string Resource_Name = line.Substring(line.IndexOf('|') + 1);
                        Resource_Version.Add(Resource_Name, Version);
                    }
                }
            }
            double Now_Version = int.Parse(File.ReadAllText(Local_Path + "\\Version.dat").Replace(".", "").Replace(",", ""));
            File.Delete(Local_Path + "\\Version.dat");
            string Now_Version_Temp = "";
            foreach (char c in Now_Version.ToString())
            {
                if (Now_Version_Temp == "")
                    Now_Version_Temp += c;
                else
                    Now_Version_Temp += "." + c;
            }
            string Temp = "";
            foreach (char c in Now_Version.ToString())
            {
                if (Temp == "")
                    Temp = c + ".";
                else
                    Temp += c;
            }
            Now_Version = double.Parse(Temp, CultureInfo.InvariantCulture);
            Temp = "";
            if (IsJapanese)
                Console.WriteLine("現在のバージョン:V" + Now_Version_Temp + " - サーバーに接続しています...");
            else
                Console.WriteLine("Current version:V" + Now_Version_Temp + " - Connecting to server...");
            Console.WriteLine();
            Client.Connect();
            if (!Client.IsConnected)
            {
                if (IsJapanese)
                    Console.WriteLine("サーバーに接続できませんでした。キーを押すと終了します。");
                else
                    Console.WriteLine("Could not connect to server. Press key to exit.");
                Console.CursorVisible = true;
                Console.ReadKey();
                return;
            }
            Client.DataReceive += Client_DataReceive;
            Client.Send("IsCanConnectServer|");
            int Env_01 = Environment.TickCount;
            while (IsCanConnectServer == 0)
            {
                if (Environment.TickCount - Env_01 >= 5000)
                {
                    if (IsJapanese)
                    {
                        Console.WriteLine("5秒間サーバーが応答しませんでした。時間を空けてから再度お試しください。");
                        Console.WriteLine("キーを押すと終了します。");
                    }
                    else
                    {
                        Console.WriteLine("Server did not respond within 5 seconds. Please try again later.");
                        Console.WriteLine("Press key to exit.");
                    }
                    Console.CursorVisible = true;
                    Console.ReadKey();
                    return;
                }
                System.Threading.Thread.Sleep(100);
            }
            if (IsCanConnectServer == 2)
            {
                if (IsJapanese)
                {
                    Console.WriteLine("現在アップデート機能を利用できません。サービスが終了したか、メンテナンス中の可能性があります。");
                    Console.WriteLine("キーを押すと終了します。");
                }
                else
                {
                    Console.WriteLine("The update function is currently unavailable. The service may have ended or it may be under maintenance.");
                    Console.WriteLine("キーを押すと終了します。");
                }
                Console.ReadKey();
                return;
            }
            if (IsJapanese)
                Console.WriteLine("サーバーに接続しました。現在の最新バージョンを取得しています...");
            else
                Console.WriteLine("Connected to server. Getting current latest version...");
            Console.WriteLine();
            if (IsJapanese)
                Client.Send("Get_Version|True");
            else
                Client.Send("Get_Version|False");
            Env_01 = Environment.TickCount;
            while (Version == 0)
            {
                if (Environment.TickCount - Env_01 >= 5000)
                {
                    if (IsJapanese)
                    {
                        Console.WriteLine("5秒間サーバーが応答しませんでした。時間を空けてから再度お試しください。");
                        Console.WriteLine("キーを押すと終了します。");
                    }
                    else
                    {
                        Console.WriteLine("Server did not respond within 5 seconds. Please try again later.");
                        Console.WriteLine("Press key to exit.");
                    }
                    Console.CursorVisible = true;
                    Console.ReadKey();
                    return;
                }
                System.Threading.Thread.Sleep(100);
            }
            if (Now_Version >= Version)
            {
                if (IsJapanese)
                    Console.WriteLine("既に最新バージョンを利用しています。");
                else
                    Console.WriteLine("Already using the latest version.");
            }
            else
            {
                string New_Version_Temp = Version.ToString().Replace(".", "").Replace(",", "");
                string New_Version_String = "";
                foreach (char c in New_Version_Temp)
                {
                    if (New_Version_String == "")
                        New_Version_String += c;
                    else
                        New_Version_String += "." + c;
                }
                if (IsJapanese)
                    Console.WriteLine("新たなバージョンを取得しました。V:" + New_Version_String);
                else
                    Console.WriteLine("Got a new version. V:" + New_Version_String);
                Console.WriteLine();
                if (IsJapanese)
                    Console.WriteLine("5秒後にダウンロードを開始します。キャンセルする場合は5秒以内にコンソールを閉じてください。");
                else
                    Console.WriteLine("Download will start after 5 seconds. If you want to cancel, please close the console within 5 seconds.");
                Console.WriteLine();
                System.Threading.Thread.Sleep(5000);
                if (IsJapanese)
                {
                    Console.Write("ダウンロードを開始します。");
                    Client.Send("Update_PC|Japanese - アップデートを開始します。V" + Now_Version_Temp + " -> V" + New_Version_String);
                }
                else
                {
                    Console.Write("Start download.");
                    Client.Send("Update_PC|English - アップデートを開始します。V" + Now_Version_Temp + " -> V" + New_Version_String);
                }
                WebClient client = new WebClient();
                Uri uri;
                if (IsJapanese)
                {
                    if (IsSRTTbaconMode)
                        uri = new Uri("http://" + SRTTbacon_Server.Local_IP + "/Mod_Creater/Versions/" + New_Version_String + "/WoTB_Voice_Mod_Creater.exe");
                    else
                        uri = new Uri("http://" + SRTTbacon_Server.Global_IP + "/Mod_Creater/Versions/" + New_Version_String + "/WoTB_Voice_Mod_Creater.exe");
                }
                else
                {
                    if (IsSRTTbaconMode)
                        uri = new Uri("http://" + SRTTbacon_Server.Local_IP + "/Mod_Creater/Versions_English/" + New_Version_String + "/WoTB_Mod_Creator_English.exe");
                    else
                        uri = new Uri("http://" + SRTTbacon_Server.Global_IP + "/Mod_Creater/Versions_English/" + New_Version_String + "/WoTB_Mod_Creator_English.exe");
                }
                client.DownloadProgressChanged += DownloadProgressCallback;
                client.DownloadFileCompleted += Client_DownloadFileCompleted;
                client.DownloadFileAsync(uri, Local_Path + "\\Update_Mod_Creator.tmp");
                string Download_Text = "";
                bool IsDeleteDownload = false;
                while (!isDownloaded)
                {
                    if (File.Exists(Local_Path + "\\Update_Mod_Creator.tmp"))
                    {
                        if (!IsDeleteDownload)
                        {
                            if (IsJapanese)
                                Delete_Text("ダウンロードを開始します。");
                            else
                                Delete_Text("Start download.");
                            IsDeleteDownload = true;
                        }
                        Delete_Text(Download_Text);
                        FileInfo Info = new FileInfo(Local_Path + "\\Update_Mod_Creator.tmp");
                        if (IsJapanese)
                            Download_Text = "ファイルをダウンロード中... " + Math.Round(Info.Length / 1024.0 / 1024.0, 2) + "/" + TotalSize + "MB" + " - " + Percent + "%";
                        else
                            Download_Text = "Downloading file... " + Math.Round(Info.Length / 1024.0 / 1024.0, 2) + "/" + TotalSize + "MB" + " - " + Percent + "%";
                        Console.Write(Download_Text);
                    }
                    System.Threading.Thread.Sleep(1000 / 60);
                }
                if (File.Exists(Local_Path + "\\Update_Mod_Creator.tmp"))
                {
                    FileInfo FI = new FileInfo(Local_Path + "\\Update_Mod_Creator.tmp");
                    if (FI.Length >= 4000000)
                    {
                        if (IsJapanese)
                            File.Copy(Local_Path + "\\Update_Mod_Creator.tmp", Local_Path + "\\WoTB_Voice_Mod_Creater.exe", true);
                        else
                            File.Copy(Local_Path + "\\Update_Mod_Creator.tmp", Local_Path + "\\WoTB_Mod_Creator_English.exe", true);
                        File.Delete(Local_Path + "\\Update_Mod_Creator.tmp");
                    }
                }
                Delete_Text(Download_Text);
                client.Dispose();
                if (IsJapanese)
                    Console.WriteLine("V" + New_Version_String + "をインストールしました。");
                else
                    Console.WriteLine("Installed V" + New_Version_String);
            }
            Console.WriteLine();
            if (IsJapanese)
                Console.WriteLine("リソースファイルの更新があるか確認しています...");
            else
                Console.WriteLine("Checking for resource file updates...");
            Console.WriteLine();
            Client.Send("Get_Resources|" + Now_Version);
            Env_01 = Environment.TickCount;
            while (Resource_Update == "")
            {
                if (Environment.TickCount - Env_01 >= 5000)
                {
                    if (IsJapanese)
                    {
                        Console.WriteLine("5秒間サーバーが応答しませんでした。時間を空けてから再度お試しください。");
                        Console.WriteLine("キーを押すと終了します。");
                    }
                    else
                    {
                        Console.WriteLine("Server did not respond within 5 seconds. Please try again later.");
                        Console.WriteLine("Press key to exit.");
                    }
                    Console.CursorVisible = true;
                    Console.ReadKey();
                    return;
                }
                System.Threading.Thread.Sleep(100);
            }
            if (Resource_Update == "None")
            {
                if (IsJapanese)
                    Console.WriteLine("リソースフォルダの更新はありませんでした。");
                else
                    Console.WriteLine("There was no resource folder update.");
                Console.WriteLine();
            }
            else
            {
                string[] Split_01 = Resource_Update.Split(',');
                bool IsHasUpdate = false;
                foreach (string S1 in Split_01)
                {
                    string[] Split_02 = S1.Split(':');
                    if (Split_02[0] == "Update")
                    {
                        if (Resource_Version.ContainsKey(Split_02[1]) && double.Parse(Split_02[3], CultureInfo.InvariantCulture) <= Resource_Version[Split_02[1]])
                            continue;
                        IsHasUpdate = true;
                        isDownloaded = false;
                        WebClient client = new WebClient();
                        Uri uri = new Uri("http://" + SRTTbacon_Server.Global_IP + "/Mod_Creater/Resources/" + Split_02[1] + ".zip");
                        if (IsSRTTbaconMode)
                            uri = new Uri("http://" + SRTTbacon_Server.Local_IP + "/Mod_Creater/Resources/" + Split_02[1] + ".zip");
                        client.DownloadProgressChanged += DownloadProgressCallback;
                        client.DownloadFileCompleted += Client_DownloadFileCompleted;
                        client.DownloadFileAsync(uri, Local_Path + "\\Resources\\Update_Resources.tmp");
                        string Download_Text = "";
                        while (!isDownloaded)
                        {
                            if (File.Exists(Local_Path + "\\Resources\\Update_Resources.tmp"))
                            {
                                FileInfo Info = new FileInfo(Local_Path + "\\Resources\\Update_Resources.tmp");
                                Delete_Text(Download_Text);
                                if (IsJapanese)
                                    Download_Text = "ファイルをダウンロード中... " + Math.Round(Info.Length / 1024.0 / 1024.0, 2) + "/" + TotalSize + "MB" + " - " + Percent + "%";
                                else
                                    Download_Text = "Downloading file... " + Math.Round(Info.Length / 1024.0 / 1024.0, 2) + "/" + TotalSize + "MB" + " - " + Percent + "%";
                                Console.Write(Download_Text);
                            }
                            System.Threading.Thread.Sleep(1000 / 60);
                        }
                        if (File.Exists(Local_Path + "\\Resources\\Update_Resources.tmp"))
                        {
                            Delete_Text(Download_Text);
                            if (IsJapanese)
                                Console.Write("ファイルを展開しています...");
                            else
                                Console.Write("Unpacking files...");
                            Extract_ZipFile(Local_Path + "\\Resources\\Update_Resources.tmp", Local_Path + "\\Resources\\" + Split_02[2]);
                            if (IsJapanese)
                            {
                                Delete_Text("ファイルを展開しています...");
                                Console.WriteLine(Split_02[2] + "を更新しました。");
                            }
                            else
                            {
                                Delete_Text("Unpacking files...");
                                Console.WriteLine("Updated " + Split_02[2] + ".");
                            }
                            File.Delete(Local_Path + "\\Resources\\Update_Resources.tmp");
                        }
                        client.Dispose();
                        if (Resource_Version.ContainsKey(Split_02[1]))
                            Resource_Version[Split_02[1]] = double.Parse(Split_02[3], CultureInfo.InvariantCulture);
                        else
                            Resource_Version.Add(Split_02[1], double.Parse(Split_02[3], CultureInfo.InvariantCulture));
                    }
                    else if (Split_02[0] == "Delete")
                    {
                        if (Resource_Version.ContainsKey(Split_02[1]) && double.Parse(Split_02[2], CultureInfo.InvariantCulture) <= Resource_Version[Split_02[1]])
                            continue;
                        IsHasUpdate = true;
                        if (Directory.Exists(Local_Path + "\\Resources\\" + Split_02[1]))
                        {
                            Directory.Delete(Local_Path + "\\Resources\\" + Split_02[1], true);
                            if (IsJapanese)
                                Console.WriteLine(Split_02[1] + "を削除しました。");
                            else
                                Console.WriteLine(Split_02[1] + " has been removed.");
                        }
                        if (Resource_Version.ContainsKey(Split_02[1]))
                            Resource_Version[Split_02[1]] = double.Parse(Split_02[2], CultureInfo.InvariantCulture);
                        else
                            Resource_Version.Add(Split_02[1], double.Parse(Split_02[2], CultureInfo.InvariantCulture));
                    }
                }
                if (!IsHasUpdate)
                {
                    if (IsJapanese)
                        Console.WriteLine("リソースフォルダの更新はありませんでした。");
                    else
                        Console.WriteLine("There was no resource folder update.");
                }
                else
                {
                    StreamWriter stw = File.CreateText(Local_Path + "\\Resources\\Versions.dat.tmp");
                    foreach (string Key in Resource_Version.Keys)
                        stw.WriteLine(Resource_Version[Key] + "|" + Key);
                    stw.Close();
                    Encode.Crypt_Decrypt.Stream_Encrypt_To_File(Local_Path + "\\Resources\\Versions.dat.tmp", Local_Path + "\\Resources\\Versions.dat", "SRTTbacon_Resource_Versions_Crypt", true);
                }
                Console.WriteLine();
            }
            File.Delete(Local_Path + "\\System.Diagnostics.DiagnosticSource.xml");
            File.Delete(Local_Path + "\\DotNetZip.xml");
            if (IsJapanese)
                Console.WriteLine("すべての処理が終了しました。キーを押すと終了します。");
            else
                Console.WriteLine("All processing has ended. Press key to exit.");
            Console.CursorVisible = true;
            Console.ReadKey();
        }
        private static bool IsOpenedModCreator(string Name)
        {
            Mutex mutex = new Mutex(false, Name);
            if (!mutex.WaitOne(0, false))
            {
                mutex.Dispose();
                return true;
            }
            mutex.Dispose();
            return false;
        }
        private static void Delete_Text(string Text)
        {
            byte[] Text_Bytes = Encoding.UTF8.GetBytes(Text);
            for (int Number = 0; Number < Text_Bytes.Length; Number++)
                Console.Write("\b \b");
        }
        private static void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            isDownloaded = true;
        }
        private static void Client_DataReceive(string data)
        {
            string[] Split = data.Split('|');
            if (Split[0] == "Response" && Split[1] == "Version")
                Version = double.Parse(Split[2], CultureInfo.InvariantCulture);
            else if (Split[0] == "Response" && Split[1] == "Update_Resources")
                Resource_Update = Split[2];
            else if (Split[0] == "Response" && Split[1] == "IsCanConnectServer")
            {
                bool IsCan = bool.Parse(Split[2]);
                if (IsCan)
                    IsCanConnectServer = 1;
                else
                    IsCanConnectServer = 2;
            }
        }
        private static void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
        {
            TotalSize = Math.Round(e.TotalBytesToReceive / 1024.0 / 1024.0, 2);
            Percent = e.ProgressPercentage;
        }
        private static bool CanDirectoryAccess(string Dir_Path)
        {
            try
            {
                WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                DirectorySecurity security = Directory.GetAccessControl(Dir_Path);
                AuthorizationRuleCollection authRules = security.GetAccessRules(true, true, typeof(SecurityIdentifier));
                foreach (FileSystemAccessRule accessRule in authRules)
                    if (principal.IsInRole(accessRule.IdentityReference as SecurityIdentifier))
                        if ((FileSystemRights.WriteData & accessRule.FileSystemRights) == FileSystemRights.WriteData)
                            if (accessRule.AccessControlType == AccessControlType.Allow)
                                return true;
                return false;
            }
            catch
            {
                return false;
            }
        }
        private static void Extract_ZipFile(string ZipFile, string DirectoryName)
        {
            Ionic.Zip.ZipFile zip = Ionic.Zip.ZipFile.Read(ZipFile);
            DirectoryInfo di = Directory.CreateDirectory(DirectoryName);
            string destinationDirectoryFullPath = di.FullName;
            int count = 0;
            foreach (Ionic.Zip.ZipEntry entry in zip.Entries)
            {
                count++;
                string Combine = System.IO.Path.Combine(destinationDirectoryFullPath, entry.FileName);
                if (destinationDirectoryFullPath.Length > 247 || Combine.Length > 259)
                    continue;
                try
                {
                    string fileDestinationPath = System.IO.Path.GetFullPath(Combine);
                    if (!fileDestinationPath.StartsWith(destinationDirectoryFullPath, StringComparison.OrdinalIgnoreCase))
                    {
                        if (IsJapanese)
                            throw new IOException("指定されたフォルダの外にファイルが抽出されています。");
                        else
                            throw new IOException("Files have been extracted outside the specified folder.");
                    }
                    if (System.IO.Path.GetFileName(fileDestinationPath).Length == 0)
                        Directory.CreateDirectory(fileDestinationPath);
                    else
                    {
                        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fileDestinationPath));
                        entry.Extract(destinationDirectoryFullPath, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
                    }
                }
                catch { }
            }
            zip.Dispose();
        }
    }
}