using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WoTB_Voice_Mod_Creater
{
    public class Sub_Code
    {
        static List<string> IsAutoListAdd = new List<string>();
        static bool IsServerCreating = false;
        static bool IsCreatingProject = false;
        static bool IsVolumeSet = false;
        static bool IsDVPLEncode = false;
        static bool IsModChange = false;
        static bool IsAndroidMode = false;
        public static bool ServerCreate
        {
            get { return IsServerCreating; }
            set { IsServerCreating = value; }
        }
        public static bool CreatingProject
        {
            get { return IsCreatingProject; }
            set { IsCreatingProject = value; }
        }
        public static bool VolumeSet
        {
            get { return IsVolumeSet; }
            set { IsVolumeSet = value; }
        }
        public static bool DVPL_Encode
        {
            get { return IsDVPLEncode; }
            set { IsDVPLEncode = value; }
        }
        public static bool ModChange
        {
            get { return IsModChange; }
            set { IsModChange = value; }
        }
        public static bool AndroidMode
        {
            get { return IsAndroidMode; }
            set { IsAndroidMode = value; }
        }
        public static List<string> AutoListAdd
        {
            get { return IsAutoListAdd; }
            set { IsAutoListAdd = value; }
        }
        //必要なdllがない場合そのdll名のリストを返す
        public static List<string> DLL_Exists()
        {
            string DLL_Path = Directory.GetCurrentDirectory() + "/dll";
            List<string> DLL_List = new List<string>();
            if (!File.Exists(DLL_Path + "/bass.dll"))
            {
                DLL_List.Add("bass.dll");
            }
            if (!File.Exists(DLL_Path + "/bass_fx.dll"))
            {
                DLL_List.Add("bass_fx.dll");
            }
            if (!File.Exists(DLL_Path + "/DdsFileTypePlusIO_x86.dll"))
            {
                DLL_List.Add("DdsFileTypePlusIO_x86.dll");
            }
            if (!File.Exists(DLL_Path + "/fmod_event.dll"))
            {
                DLL_List.Add("fmod_event.dll");
            }
            if (!File.Exists(DLL_Path + "/fmodex.dll"))
            {
                DLL_List.Add("fmodex.dll");
            }
            return DLL_List;
        }
        //.dvplを抜いたファイルパスからファイルが存在するか
        //例:sounds.yaml.dvpl -> DVPL_File_Exists(sounds.yaml) -> true,false
        public static bool DVPL_File_Exists(string File_Path)
        {
            if (File.Exists(File_Path) || File.Exists(File_Path + ".dvpl"))
            {
                return true;
            }
            return false;
        }
        //WoTBのディレクトリを取得
        public static bool WoTB_Get_Directory()
        {
            try
            {
                RegistryKey rKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Valve\\Steam");
                string location = (string)rKey.GetValue("InstallPath");
                rKey.Close();
                string driveRegex = @"[A-Z]:\\";
                if (File.Exists(location + "/steamapps/common/World of Tanks Blitz/wotblitz.exe"))
                {
                    StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Temp_WoTB_Path.dat");
                    stw.Write(location + "/steamapps/common/World of Tanks Blitz");
                    stw.Close();
                    using (var eifs = new FileStream(Voice_Set.Special_Path + "/Temp_WoTB_Path.dat", FileMode.Open, FileAccess.Read))
                    {
                        using (var eofs = new FileStream(Directory.GetCurrentDirectory() + "/WoTB_Path.dat", FileMode.Create, FileAccess.Write))
                        {
                            FileEncode.FileEncryptor.Encrypt(eifs, eofs, "WoTB_Directory_Path_Pass");
                        }
                    }
                    File.Delete(Voice_Set.Special_Path + "/Temp_WoTB_Path.dat");
                    Voice_Set.WoTB_Path = location + "/steamapps/common/World of Tanks Blitz";
                    return true;
                }
                string[] configLines = File.ReadAllLines(location + "/steamapps/libraryfolders.vdf");
                foreach (var item in configLines)
                {
                    Match match = Regex.Match(item, driveRegex);
                    if (item != string.Empty && match.Success)
                    {
                        string matched = match.ToString();
                        string item2 = item.Substring(item.IndexOf(matched));
                        item2 = item2.Replace("\\\\", "\\");
                        item2 = item2.Replace("\"", "\\steamapps\\common\\");
                        if (File.Exists(item2 + "World of Tanks Blitz\\wotblitz.exe"))
                        {
                            StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Temp_WoTB_Path.dat");
                            stw.Write(item2 + "World of Tanks Blitz");
                            stw.Close();
                            using (var eifs = new FileStream(Voice_Set.Special_Path + "/Temp_WoTB_Path.dat", FileMode.Open, FileAccess.Read))
                            {
                                using (var eofs = new FileStream(Directory.GetCurrentDirectory() + "/WoTB_Path.dat", FileMode.Create, FileAccess.Write))
                                {
                                    FileEncode.FileEncryptor.Encrypt(eifs, eofs, "WoTB_Directory_Path_Pass");
                                }
                            }
                            File.Delete(Voice_Set.Special_Path + "/Temp_WoTB_Path.dat");
                            Voice_Set.WoTB_Path = item2 + "World of Tanks Blitz";
                            return true;
                        }
                    }
                }
                MessageBox.Show("WoTBのインストール先を取得できませんでした。SteamにWoTBがインストールされていないか、32BitOSを使用している可能性があります。");
                return false;
            }
            catch (Exception e)
            {
                MessageBox.Show("WoTBのインストール先を取得できませんでした。SteamにWoTBがインストールされていないか、32BitOSを使用している可能性があります。");
                Sub_Code.Error_Log_Write(e.Message);
                return false;
            }
        }
        //ディレクトリをコピー(サブフォルダを含む)
        public static void Directory_Copy(string From_Dir, string To_Dir)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(From_Dir);
                if (!dir.Exists)
                {
                    throw new DirectoryNotFoundException("指定したディレクトリが存在しません。\n" + From_Dir);
                }
                DirectoryInfo[] dirs = dir.GetDirectories();
                Directory.CreateDirectory(To_Dir);
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    string tempPath = Path.Combine(To_Dir, file.Name);
                    file.CopyTo(tempPath, false);
                }
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(To_Dir, subdir.Name);
                    Directory_Copy(subdir.FullName, tempPath);
                }
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        //.dvplを抜いたファイルをコピーする
        public static bool DVPL_File_Copy(string FromFilePath, string ToFilePath, bool IsOverWrite)
        {
            FromFilePath = FromFilePath.Replace(".dvpl", "");
            ToFilePath = ToFilePath.Replace(".dvpl", "");
            if (File.Exists(FromFilePath) || File.Exists(FromFilePath + ".dvpl"))
            {
                if (File.Exists(FromFilePath))
                {
                    try
                    {
                        File.Copy(FromFilePath, ToFilePath, IsOverWrite);
                        return true;
                    }
                    catch (Exception e)
                    {
                        Sub_Code.Error_Log_Write(e.Message);
                    }
                }
                else if (File.Exists(FromFilePath + ".dvpl"))
                {
                    try
                    {
                        File.Copy(FromFilePath + ".dvpl", ToFilePath + ".dvpl", IsOverWrite);
                        return true;
                    }
                    catch (Exception e)
                    {
                        Sub_Code.Error_Log_Write(e.Message);
                    }
                }
            }
            return false;
        }
        //.dvplを抜いたファイルを削除する
        public static bool DVPL_File_Delete(string FilePath)
        {
            bool IsDelected = false;
            FilePath = FilePath.Replace(".dvpl", "");
            if (File.Exists(FilePath))
            {
                try
                {
                    File.Delete(FilePath);
                    IsDelected = true;
                }
                catch (Exception e)
                {
                    Sub_Code.Error_Log_Write(e.Message);
                }
            }
            else if (File.Exists(FilePath + ".dvpl"))
            {
                try
                {
                    File.Delete(FilePath + ".dvpl");
                    IsDelected = true;
                }
                catch (Exception e)
                {
                    Sub_Code.Error_Log_Write(e.Message);
                }
            }
            return IsDelected;
        }
        //ファイルを移動(正確にはコピーして元ファイルを削除)
        public static bool File_Move(string From_File_Path, string To_File_Path, bool IsOverWrite)
        {
            if (!File.Exists(From_File_Path))
            {
                return false;
            }
            if (File.Exists(To_File_Path) && !IsOverWrite)
            {
                return false;
            }
            try
            {
                File.Copy(From_File_Path, To_File_Path, true);
                File.Delete(From_File_Path);
                return true;
            }
            catch
            {
                return false;
            }
        }
        //↑の拡張子を指定しないバージョン
        public static bool File_Move_V2(string From_File_Path, string To_File_Path, bool IsOverWrite)
        {
            string From_Path = File_Get_FileName_No_Extension(From_File_Path);
            string To_Path = To_File_Path + Path.GetExtension(From_Path);
            return File_Move(From_Path, To_Path, IsOverWrite);
        }
        //ファイル拡張子を指定しないでファイルをコピーする
        public static bool File_Copy(string From_File_Path, string To_File_Path, bool IsOverWrite)
        {
            string File_Path = "";
            string Dir = From_File_Path.Substring(0, From_File_Path.LastIndexOf('/'));
            string Name = From_File_Path.Substring(From_File_Path.LastIndexOf('/') + 1);
            var files = Directory.GetFiles(Dir, Name + ".*");
            if (files.Length > 0)
            {
                File_Path = files[0];
            }
            if (File_Path == "" || !File.Exists(From_File_Path))
            {
                return false;
            }
            if (File_Exists(To_File_Path) && !IsOverWrite)
            {
                return false;
            }
            try
            {
                File.Copy(File_Path, To_File_Path + Path.GetExtension(File_Path), true);
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                return false;
            }
            return true;
        }
        //ファイル拡張子を指定しないでファイルを削除
        public static bool File_Delete(string File_Path)
        {
            string Dir = File_Path.Substring(0, File_Path.LastIndexOf('/'));
            string Name = File_Path.Substring(File_Path.LastIndexOf('/') + 1);
            var files = Directory.GetFiles(Dir, Name + ".*");
            if (files.Length > 0)
            {
                File.Delete(files[0]);
                return true;
            }
            return false;
        }
        public static bool File_Delete_V2(string File_Path)
        {
            try
            {
                File.Delete(File_Path);
                return true;
            }
            catch
            {
                return false;
            }
        }
        //ファイル拡張子なしでファイルが存在するか取得
        //戻り値:存在した場合true,それ以外はfalse
        public static bool File_Exists(string File_Path)
        {
            try
            {
                string Dir = File_Path.Substring(0, File_Path.LastIndexOf('/'));
                string Name = File_Path.Substring(File_Path.LastIndexOf('/') + 1);
                var files = Directory.GetFiles(Dir, Name + ".*");
                if (files.Length > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                return false;
            }
        }
        //音声の種類が存在するか
        public static bool File_Exist_Voice_Type(string Voice_Dir, string File_Path)
        {
            try
            {
                string Path_01 = Voice_Dir + "/" + File_Path;
                string Dir = Path_01.Substring(0, Path_01.LastIndexOf('/'));
                string Name = Path_01.Substring(Path_01.LastIndexOf('/') + 1);
                var files = Directory.GetFiles(Dir, Name + "_01.*");
                if (files.Length > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                return false;
            }
        }
        //ファイル拡張子なしのパスから拡張子付きのファイルパスを取得
        //戻り値:拡張子付きのファイル名
        public static string File_Get_FileName_No_Extension(string File_Path)
        {
            try
            {
                string Dir = File_Path.Substring(0, File_Path.LastIndexOf('/'));
                string Name = File_Path.Substring(File_Path.LastIndexOf('/') + 1);
                var files = Directory.GetFiles(Dir, Name + ".*");
                if (files.Length > 0)
                {
                    return files[0];
                }
                else
                {
                    return "";
                }
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                return "";
            }
        }
        //音声タイプとそのタイプのファイル数を取得
        //引数:取得するフォルダ,ref 音声タイプを入れるリスト,ref 音声数を入れるリスト   (リストは初期化される)
        public static void Get_Voice_Type_And_Index(string Dir_Path, ref List<string> Voice_Type, ref List<int> Voice_Number)
        {
            string[] Dir_List = Directory.GetFiles(Dir_Path, "*", SearchOption.TopDirectoryOnly);
            List<string> Voice_List_Type = new List<string>();
            for (int Number = 0; Number <= Dir_List.Length - 1; Number++)
            {
                if (Voice_Set.Voice_Name_Hide(Dir_List[Number]))
                {
                    Voice_List_Type.Add(Path.GetFileName(Dir_List[Number]));
                }
            }
            List<string> Voice_Type_Ref = new List<string>();
            List<int> Voice_Type_Number_Ref = new List<int>();
            int Voice_Type_Number = 0;
            string Name_Now = "";
            for (int Number = 0; Number <= Voice_List_Type.Count - 1; Number++)
            {
                string Name_Only = Voice_List_Type[Number].Substring(0, Voice_List_Type[Number].LastIndexOf('_'));
                if (Name_Now != Name_Only)
                {
                    if (Name_Now != "")
                    {
                        Voice_Type_Number_Ref.Add(Voice_Type_Number);
                    }
                    Name_Now = Name_Only;
                    Voice_Type_Number = 1;
                    Voice_Type_Ref.Add(Voice_Set.Get_Voice_Type_Japanese_Name(Name_Only));
                }
                else
                {
                    Voice_Type_Number++;
                }
            }
            Voice_Type_Number_Ref.Add(Voice_Type_Number);
            Voice_Type = Voice_Type_Ref;
            Voice_Number = Voice_Type_Number_Ref;
        }
        //音声タイプの名前に変換
        //例:Indexが2で既にそのタイプのファイル数が3個ある場合 -> danyaku_04.mp3
        public static string Set_Voice_Type_Change_Name_By_Index(string Dir, List<List<string>> Lists)
        {
            int Romaji_Number = 0;
            foreach (List<string> Index in Lists)
            {
                int File_Number = 1;
                foreach (string File_Path in Index)
                {
                    try
                    {
                        if (File_Number < 10)
                        {
                            File.Copy(File_Path, Dir + "/" + Voice_Set.Get_Voice_Type_Romaji_Name(Romaji_Number) + "_0" + File_Number + Path.GetExtension(File_Path), true);
                        }
                        else
                        {
                            File.Copy(File_Path, Dir + "/" + Voice_Set.Get_Voice_Type_Romaji_Name(Romaji_Number) + "_" + File_Number + Path.GetExtension(File_Path), true);
                        }
                        File_Number++;
                    }
                    catch (Exception e)
                    {
                        Sub_Code.Error_Log_Write(e.Message);
                        return "ファイルをコピーできませんでした。";
                    }
                }
                Romaji_Number++;
            }
            return "";
        }
        //音声がMP3でない場合MP3に変換する(拡張子も変更される)
        //例:Test.wav->Test.mp3
        public static async Task Change_MP3_Encode(string Dir, int Gain = 10)
        {
            try
            {
                string[] Files_01 = Directory.GetFiles(Dir, "*", SearchOption.TopDirectoryOnly);
                foreach (string File_Now in Files_01)
                {
                    StreamReader str = new StreamReader(File_Now);
                    string Read_01 = str.ReadLine();
                    str.Close();
                    string Read = Read_01.Substring(0, 3);
                    //最初の3文字がID3だった場合.mp3形式
                    //Xrecordで変換した場合ヘッダがなくなるためXingが含まれていたら
                    if (Read != "ID3" && !Read_01.Contains("Xing") && !Read_01.Contains("LAME"))
                    {
                        try
                        {
                            File.Move(File_Now, Dir + "/" + Path.GetFileNameWithoutExtension(File_Now) + ".raw");
                        }
                        catch
                        {

                        }
                    }
                    else if (Path.GetFileName(File_Now) != ".mp3")
                    {
                        try
                        {
                            File.Move(File_Now, Dir + "/" + Path.GetFileName(File_Now));
                        }
                        catch
                        {

                        }
                    }
                }
                await Task.Delay(10);
                string[] Files_02 = Directory.GetFiles(Dir, "*.raw", SearchOption.TopDirectoryOnly);
                foreach (string File_Now in Files_02)
                {
                    StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Encode_Mp3/Start.bat");
                    stw.WriteLine("chcp 65001");
                    stw.Write("\"" + Voice_Set.Special_Path + "/Encode_Mp3/ffmpeg.exe\" -y -i \"" + File_Now + "\" -acodec libmp3lame -ab 128k \"" + Dir + "/" + Path.GetFileNameWithoutExtension(File_Now) + ".mp3\"");
                    stw.Close();
                    ProcessStartInfo processStartInfo1 = new ProcessStartInfo
                    {
                        FileName = Voice_Set.Special_Path + "/Encode_Mp3/Start.bat",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    Process p = Process.Start(processStartInfo1);
                    p.WaitForExit();
                    File.Delete(Voice_Set.Special_Path + "/Encode_Mp3/Start.bat");
                    File.Delete(File_Now);
                }
                string File_Import = "";
                string[] Files_03 = Directory.GetFiles(Dir, "*.mp3", SearchOption.TopDirectoryOnly);
                foreach (string File_Now in Files_03)
                {
                    if (File_Import == "")
                    {
                        File_Import = "\"" + File_Now + "\"";
                    }
                    else
                    {
                        File_Import += " \"" + File_Now + "\"";
                    }
                }
                await Task.Delay(50);
                if (File_Import != "")
                {
                    StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Encode_Mp3/Volume_Set.bat");
                    stw.Write("\"" + Voice_Set.Special_Path + "/Encode_Mp3/mp3gain.exe\" -r -c -p -d " + Gain + " " + File_Import);
                    stw.Close();
                    ProcessStartInfo processStartInfo1 = new ProcessStartInfo
                    {
                        FileName = Voice_Set.Special_Path + "/Encode_Mp3/Volume_Set.bat",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    Process p = Process.Start(processStartInfo1);
                    p.WaitForExit();
                    File.Delete(Voice_Set.Special_Path + "/Encode_Mp3/Volume_Set.bat");
                }
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        //.fdpファイルから.fev + .fsbを作成する
        //例:Test.fdp -> Test.fevとTest.fsbを作成
        public static async Task Project_Build(string Project_File, System.Windows.Controls.TextBlock Message_T)
        {
            StreamWriter stw2 = File.CreateText(Voice_Set.Special_Path + "/Fmod_Designer/BGM_Create.bat");
            stw2.Write("\"" + Voice_Set.Special_Path + "/Fmod_Designer/fmod_designercl.exe\" -pc -adpcm \"" + Project_File + "\"");
            stw2.Close();
            Process p2 = new Process();
            p2.StartInfo.FileName = Voice_Set.Special_Path + "/Fmod_Designer/BGM_Create.bat";
            p2.StartInfo.CreateNoWindow = true;
            p2.StartInfo.UseShellExecute = false;
            p2.Start();
            int Number_01 = 2;
            while (true)
            {
                if (Number_01 == 0)
                {
                    Message_T.Text = "FSBファイルを作成しています.";
                }
                else if (Number_01 == 1)
                {
                    Message_T.Text = "FSBファイルを作成しています..";
                }
                else if (Number_01 == 2)
                {
                    Message_T.Text = "FSBファイルを作成しています...";
                    Number_01 = -1;
                }
                if (p2.HasExited)
                {
                    p2.Close();
                    break;
                }
                Number_01++;
                await Task.Delay(1000);
            }
            File.Delete(Voice_Set.Special_Path + "/Fmod_Designer/BGM_Create.bat");
            File.Delete(Directory.GetCurrentDirectory() + "/fmod_designer.log");
            File.Delete(Directory.GetCurrentDirectory() + "/undo-log.txt");
        }
        //現在の時間を文字列で取得
        //引数:DateTime.Now,間に入れる文字,どの部分から開始するか,どの部分で終了するか(その数字の部分は含まれる)
        //First,End->1 = Year,2 = Month,3 = Date,4 = Hour,5 = Minutes,6 = Seconds
        public static string Get_Time_Now(DateTime dt, string Between, int First, int End)
        {
            if (First > End)
            {
                return "";
            }
            if (First == End)
            {
                return Get_Time_Index(dt, First);
            }
            string Temp = "";
            for (int Number = First; Number <= End; Number++)
            {
                if (Number != End)
                {
                    Temp += Get_Time_Index(dt, Number) + Between;
                }
                else
                {
                    Temp += Get_Time_Index(dt, Number);
                }
            }
            return Temp;
        }
        static string Get_Time_Index(DateTime dt, int Index)
        {
            if (Index > 0 && Index < 7)
            {
                if (Index == 1)
                {
                    return dt.Year.ToString();
                }
                else if (Index == 2)
                {
                    return dt.Month.ToString();
                }
                else if (Index == 3)
                {
                    return dt.Day.ToString();
                }
                else if (Index == 4)
                {
                    return dt.Hour.ToString();
                }
                else if (Index == 5)
                {
                    return dt.Minute.ToString();
                }
                else if (Index == 6)
                {
                    return dt.Second.ToString();
                }
            }
            return "";
        }
        //sounds.yamlの中身が古かった場合サーバーに置いてある最新のものと比較して置き換える
        //本当にWoTBの仕様が変わると思っていなかったのでこの方法でしていて良かったです...
        public static void Sounds_Yaml_Update(string File_Path, string To_Path, bool IsDVPLEncode)
        {
            try
            {
                Voice_Set.FTP_Server.DownloadFile(Voice_Set.Special_Path + "/Temp_Download_Sounds.yaml.dvpl", "/WoTB_Voice_Mod/Mods/Backup/sounds.yaml.dvpl");
                DVPL.DVPL_UnPack(Voice_Set.Special_Path + "/Temp_Download_Sounds.yaml.dvpl", Voice_Set.Special_Path + "/Temp_Download_Sounds.yaml", true);
                string Server_File = Voice_Set.Special_Path + "/Temp_Download_Sounds.yaml";
                StreamReader str = new StreamReader(File_Path);
                bool IsSoundsIn = false;
                while (str.EndOfStream == false)
                {
                    string Line = str.ReadLine();
                    //この方法で大丈夫そうですが、念のためgui_sounds:を追加
                    if (Line.Contains("sounds:") || Line.Contains("gui_sounds:"))
                    {
                        IsSoundsIn = true;
                    }
                    if (IsSoundsIn)
                    {
                        Sounds_Yaml_IsUpdate(Server_File, Line);
                    }
                }
                str.Close();
                StreamReader str2 = new StreamReader(Server_File);
                string Read_All = str2.ReadToEnd();
                str2.Close();
                File.Delete(Server_File);
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Temp_DVPL.yaml");
                stw.Write(Read_All);
                stw.Close();
                if (IsDVPLEncode)
                {
                    DVPL.DVPL_Pack(Voice_Set.Special_Path + "/Temp_DVPL.yaml", To_Path, true);
                }
                else
                {
                    File.Copy(Voice_Set.Special_Path + "/Temp_DVPL.yaml", To_Path, true);
                }
            }
            catch
            {
                //サーバーにsounds.yaml.dvplが存在しない場合
            }
        }
        static bool Sounds_Yaml_IsUpdate(string Server_File, string Line)
        {
            if (Line == "" || !Line.Contains(":"))
            {
                return false;
            }
            string Line_Head = Line.Substring(0, Line.IndexOf(':'));
            string line_01;
            StreamReader str = new StreamReader(Server_File);
            string Read_All = str.ReadToEnd();
            str.Close();
            bool IsChanging = false;
            StreamReader file_01 = new StreamReader(Server_File);
            while ((line_01 = file_01.ReadLine()) != null)
            {
                if (line_01.Contains(":"))
                {
                    if (line_01.Substring(0, line_01.IndexOf(':')) == Line_Head)
                    {
                        if (Line != line_01)
                        {
                            IsChanging = true;
                            file_01.Close();
                            break;
                        }
                    }
                }
            }
            if (!IsChanging)
            {
                file_01.Close();
                return false;
            }
            StreamWriter stw = File.CreateText(Server_File);
            stw.Write(Read_All.Replace(line_01, Line));
            stw.Close();
            return true;
        }
        //文字列に日本語が含まれていたらtrueを返す
        public static bool IsTextIncludeJapanese(string text)
        {
            bool isJapanese = Regex.IsMatch(text, @"[\p{IsHiragana}\p{IsKatakana}\p{IsCJKUnifiedIdeographs}]+");
            return isJapanese;
        }
        /// <summary>
        /// オーディオと動画を結合させる
        /// 元動画のオーディオは消えるので注意
        /// </summary>
        /// <param name="Audio_File">オーディオのファイル場所</param>
        /// <param name="Video_File">動画のファイル場所</param>
        /// <param name="Out_File">出力場所</param>
        public static void Audio_Video_Convert(string Video_File, string Audio_File, string Out_File, bool IsOverWrite = true)
        {
            if (!File.Exists(Video_File) || !File.Exists(Audio_File))
            {
                return;
            }
            if (File.Exists(Out_File) && !IsOverWrite)
            {
                return;
            }
            StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Encode_Mp3/Audio_Video_Convert.bat");
            stw.WriteLine("chcp 65001");
            stw.Write("\"" + Voice_Set.Special_Path + "/Encode_Mp3/ffmpeg.exe\" -y -i \"" + Video_File + "\" -i \"" + Audio_File + "\" -c:v copy -c:a mp3 \"" + Out_File + "\"");
            stw.Close();
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = Voice_Set.Special_Path + "/Encode_Mp3/Audio_Video_Convert.bat",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process p = Process.Start(processStartInfo);
            p.WaitForExit();
            File.Delete(Voice_Set.Special_Path + "/Encode_Mp3/Audio_Video_Convert.dat");
        }
        //エラーをログに記録(改行コードはあってもなくてもよい)
        public static void Error_Log_Write(string Text)
        {
            DateTime dt = DateTime.Now;
            string Time = Get_Time_Now(dt, ".", 1, 6);
            if (Text.EndsWith("\n"))
            {
                File.AppendAllText(Directory.GetCurrentDirectory() + "/Error_Log.txt", Time + ":" + Text);
            }
            else
            {
                File.AppendAllText(Directory.GetCurrentDirectory() + "/Error_Log.txt", Time + ":" + Text + "\n");
            }
        }
        //BitmapをBitmapImageへ変換
        public static System.Windows.Media.Imaging.BitmapImage Bitmap_To_BitmapImage(System.Drawing.Bitmap bitmap)
        {
            MemoryStream stream = new MemoryStream();
            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            stream.Seek(0, SeekOrigin.Begin);
            System.Windows.Media.Imaging.BitmapImage img = new System.Windows.Media.Imaging.BitmapImage();
            img.BeginInit();
            img.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
            img.StreamSource = stream;
            img.EndInit();
            return img;
        }
        //ファイル名に使用できない文字を_に変更
        public static string File_Replace_Name(string FileName)
        {
            string valid = FileName;
            char[] invalidch = Path.GetInvalidFileNameChars();
            foreach (char c in invalidch)
            {
                valid = valid.Replace(c, '_');
            }
            return valid;
        }
        //音声ファイルを指定した拡張子へエンコード
        public static bool Audio_Encode_To_Other(string From_Audio_File, string To_Audio_File, string Encode_Mode, bool IsFromFileDelete)
        {
            try
            {
                if (!File.Exists(From_Audio_File))
                {
                    return false;
                }
                Encode_Mode = Encode_Mode.Replace(".", "");
                string Encode_Style = "";
                //変換先に合わせて.batファイルを作成
                if (Encode_Mode == "aac")
                {
                    Encode_Style = "-y -vn -strict experimental -c:a aac -b:a 256k";
                }
                else if (Encode_Mode == "flac")
                {
                    Encode_Style = "-y -vn -ac 2 -ar 44100 -acodec flac -f flac";
                }
                else if (Encode_Mode == "mp3")
                {
                    Encode_Style = "-y -vn -ac 2 -ar 44100 -ab 128k -acodec libmp3lame -f mp3";
                }
                else if (Encode_Mode == "ogg")
                {
                    Encode_Style = "-y -vn -ac 2 -ar 44100 -ab 128k -acodec libvorbis -f ogg";
                }
                else if (Encode_Mode == "wav")
                {
                    Encode_Style = "-y -vn -ac 2 -ar 44100 -acodec pcm_s24le -f wav";
                }
                else if (Encode_Mode == "webm")
                {
                    Encode_Style = "-y -vn -f opus -acodec libopus -ab 128k";
                }
                else if (Encode_Mode == "wma")
                {
                    Encode_Style = "-y -vn -ac 2 -ar 44100 -ab 128k -acodec wmav2 -f asf";
                }
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Encode_Mp3/Audio_Encode.bat");
                stw.WriteLine("chcp 65001");
                stw.Write(Voice_Set.Special_Path + "/Encode_Mp3/ffmpeg.exe -i \"" + From_Audio_File + "\" " + Encode_Style + " \"" + To_Audio_File + "\"");
                stw.Close();
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = Voice_Set.Special_Path + "/Encode_Mp3/Audio_Encode.bat",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process p = Process.Start(processStartInfo);
                p.WaitForExit();
                if (!File.Exists(To_Audio_File))
                {
                    return false;
                }
                if (IsFromFileDelete)
                {
                    File.Delete(From_Audio_File);
                }
                File.Delete(Voice_Set.Special_Path + "/Encode_Mp3/Audio_Encode.bat");
                return true;
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                return false;
            }
        }
        //ファイルを暗号化
        //引数:元ファイルのパス,暗号先のパス,元ファイルを削除するか
        public static bool File_Encrypt(string From_File, string To_File, string Password, bool IsFromFileDelete)
        {
            try
            {
                if (!File.Exists(From_File))
                {
                    return false;
                }
                using (var eifs = new FileStream(From_File, FileMode.Open, FileAccess.Read))
                {
                    using (var eofs = new FileStream(To_File, FileMode.Create, FileAccess.Write))
                    {
                        FileEncode.FileEncryptor.Encrypt(eifs, eofs, Password);
                    }
                }
                if (IsFromFileDelete)
                {
                    File.Delete(From_File);
                }
                return true;
            }
            catch (Exception e)
            {
                Error_Log_Write(e.Message);
                return false;
            }
        }
        //ファイルを復号化
        //引数:元ファイルのパス,復号先のパス,元ファイルを削除するか
        public static bool File_Decrypt(string From_File, string To_File, string Password, bool IsFromFileDelete)
        {
            try
            {
                if (!File.Exists(From_File))
                {
                    return false;
                }
                using (var eifs = new FileStream(From_File, FileMode.Open, FileAccess.Read))
                {
                    using (var eofs = new FileStream(To_File, FileMode.Create, FileAccess.Write))
                    {
                        FileEncode.FileEncryptor.Decrypt(eifs, eofs, Password);
                    }
                }
                if (IsFromFileDelete)
                {
                    File.Delete(From_File);
                }
                return true;
            }
            catch (Exception e)
            {
                Error_Log_Write(e.Message);
                return false;
            }
        }
    }
}