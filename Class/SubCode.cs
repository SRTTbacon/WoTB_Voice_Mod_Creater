using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace WoTB_Voice_Mod_Creater
{
    public class Sub_Code
    {
        static bool IsServerCreating = false;
        static bool IsCreatingProject = false;
        static bool IsVolumeSet = false;
        static bool IsDVPLEncode = false;
        static bool IsModChange = false;
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
            catch
            {
                MessageBox.Show("WoTBのインストール先を取得できませんでした。SteamにWoTBがインストールされていないか、32BitOSを使用している可能性があります。");
                return false;
            }
        }
        //ディレクトリをコピー(サブフォルダを含む)
        public static void Directory_Copy(string From_Dir, string To_Dir)
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
                    catch
                    {

                    }
                }
                else if (File.Exists(FromFilePath + ".dvpl"))
                {
                    try
                    {
                        File.Copy(FromFilePath + ".dvpl", ToFilePath + ".dvpl", IsOverWrite);
                        return true;
                    }
                    catch
                    {

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
                catch
                {
                    
                }
            }
            else if (File.Exists(FilePath + ".dvpl"))
            {
                try
                {
                    File.Delete(FilePath + ".dvpl");
                    IsDelected = true;
                }
                catch
                {
                    
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
            catch
            {
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
            catch
            {
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
            catch
            {
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
                    catch
                    {
                        return "ファイルをコピーできませんでした。";
                    }
                }
                Romaji_Number++;
            }
            return "";
        }
        //音声がMP3でない場合MP3に変換する(拡張子も変更される)
        //例:Test.wav->Test.mp3
        public static async Task Change_MP3_Encode(string Dir)
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
                stw.Write("\"" + Voice_Set.Special_Path + "/Encode_Mp3/ffmpeg.exe\" -i \"" + File_Now + "\" -acodec libmp3lame -ab 128k \"" + Dir + "/" + Path.GetFileNameWithoutExtension(File_Now) + ".mp3\"");
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
                    File_Import = File_Now;
                }
                else
                {
                    File_Import += " " + File_Now;
                }
            }
            await Task.Delay(50);
            if (File_Import != "")
            {
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Encode_Mp3/Volume_Set.bat");
                stw.Write("\"" + Voice_Set.Special_Path + "/Encode_Mp3/mp3gain.exe\" -r -c -p -d 10 " + File_Import);
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
        //Android用のプロジェクトを作成(プロジェクト名は関係なくingane_voice_ja.fsbが作成される)
        public static async Task Android_Project_Create(System.Windows.Controls.TextBlock Message_T, string Project_Name, string Voice_Dir, string SE_Dir)
        {
            try
            {
                Message_T.Opacity = 1;
                Message_T.Text = "Androidプロジェクトを作成中...";
                if (!Directory.Exists(Directory.GetCurrentDirectory() + "/Projects/" + Project_Name))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/Projects/" + Project_Name);
                }
                if (!Directory.Exists(Directory.GetCurrentDirectory() + "/Backup"))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/Backup");
                }
                DateTime dt = DateTime.Now;
                string Time_Now = dt.Year + "." + dt.Month + "." + dt.Day + "." + dt.Hour + "." + dt.Minute + "." + dt.Second;
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/Backup/" + Time_Now);
                DVPL_File_Copy(Voice_Set.WoTB_Path + "/Data/Sfx/ingame_voice_ja.fsb", Directory.GetCurrentDirectory() + "/Backup/" + Time_Now + "/ingame_voice_ja.fsb", true);
                Voice_Name_To_Ingame_Voice(Voice_Dir);
                //Ingame_Voice_Set_Number(Voice_Dir);
                //Ingame_Voice_In_SE_By_Dir(Voice_Dir, SE_Dir, Voice_Set.Special_Path + "/Fmod_Android_Create/Voices");
                await Task.Delay(100);
            }
            catch
            {
                Message_T.Text = "エラー:正常に作成できませんでした。";
            }
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
                    if (Line.Contains("sounds:"))
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
        public static bool IsTextIncludeJapanese(string text)
        {
            bool isJapanese = Regex.IsMatch(text, @"[\p{IsHiragana}\p{IsKatakana}\p{IsCJKUnifiedIdeographs}]+");
            return isJapanese;
        }
        static string Temp_Dir = "";
        static string File_Rename_Number(string File_Path_Not_Ex_And_Number)
        {
            if (Temp_Dir == "")
            {
                return "";
            }
            int Number = 1;
            string Number_01;
            while (true)
            {
                if (Number < 10)
                {
                    if (!File.Exists(File_Get_FileName_No_Extension(Temp_Dir + "/" + File_Path_Not_Ex_And_Number + "_0" + Number)))
                    {
                        Number_01 = "0" + Number;
                        break;
                    }
                }
                else
                {
                    if (!File.Exists(File_Get_FileName_No_Extension(Temp_Dir + "/" + File_Path_Not_Ex_And_Number + "_" + Number)))
                    {
                        Number_01 = Number.ToString();
                        break;
                    }
                }
                Number++;
            }
            return Number_01;
        }
        //ファイル名をAndroidで使用できるファイル名に変更する
        public static void Voice_Name_To_Ingame_Voice(string Voice_Dir)
        {
            Temp_Dir = Voice_Dir;
            string[] Voices = Directory.GetFiles(Voice_Dir, "*", SearchOption.TopDirectoryOnly);
            foreach (string Voice_Now in Voices)
            {
                MessageBox.Show(Voice_Now);
                string Name_Only = Voice_Mod_Create.Get_Voice_Type(Voice_Now);
                if (Name_Only == "reload")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/howitzer_load_" + File_Rename_Number("howitzer_load") + Path.GetExtension(Voice_Now), false);
                }
                if (Name_Only == "hakken")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/enemy_sighted" + Path.GetExtension(Voice_Now), false);
                    if (File.Exists(Voice_Now))
                    {
                        File.Delete(Voice_Now);
                    }
                }
                if (Name_Only == "lamp")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/lamp_" + File_Rename_Number("lamp") + Path.GetExtension(Voice_Now), false);
                }
                if (Name_Only == "battle_end")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/capture_end" + Path.GetExtension(Voice_Now), false);
                    if (File.Exists(Voice_Now))
                    {
                        File.Delete(Voice_Now);
                    }
                }
                if (Name_Only == "lock")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/auto_target_on" + Path.GetExtension(Voice_Now), false);
                    if (File.Exists(Voice_Now))
                    {
                        File.Delete(Voice_Now);
                    }
                }
                if (Name_Only == "unlock")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/auto_target_off" + Path.GetExtension(Voice_Now), false);
                    if (File.Exists(Voice_Now))
                    {
                        File.Delete(Voice_Now);
                    }
                }
                if (Name_Only == "mikata")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/ally_killed_by_player_" + File_Rename_Number("ally_killed_by_player") + Path.GetExtension(Voice_Now), false);
                }
                if (Name_Only == "taiha")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/vehicle_destroyed_" + File_Rename_Number("vehicle_destroyed") + Path.GetExtension(Voice_Now), false);
                }
                if (Name_Only == "kantuu")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/armor_pierced_by_player_" + File_Rename_Number("armor_pierced_by_player") + Path.GetExtension(Voice_Now), false);
                }
                if (Name_Only == "tokusyu")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/armor_pierced_crit_by_player_" + File_Rename_Number("armor_pierced_crit_by_player") + Path.GetExtension(Voice_Now), false);
                }
                if (Name_Only == "hikantuu")
                {
                    File.Copy(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/armor_not_pierced_by_player_" + File_Rename_Number("armor_not_pierced_by_player") + Path.GetExtension(Voice_Now), true);
                    File.Copy(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/armor_ricochet_by_player_" + File_Rename_Number("armor_ricochet_by_player") + Path.GetExtension(Voice_Now), true);
                    File.Delete(Voice_Now);
                }
                if (Name_Only == "tekikasai")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/enemy_fire_started_by_player_" + File_Rename_Number("enemy_fire_started_by_player") + Path.GetExtension(Voice_Now), false);
                }
                if (Name_Only == "battle")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/start_battle_" + File_Rename_Number("start_battle") + Path.GetExtension(Voice_Now), false);
                }
                if (Name_Only == "ryoukai")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/quick_commands_positive" + Path.GetExtension(Voice_Now), false);
                    if (File.Exists(Voice_Now))
                    {
                        File.Delete(Voice_Now);
                    }
                }
                if (Name_Only == "kyohi")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/quick_commands_negative" + Path.GetExtension(Voice_Now), false);
                    if (File.Exists(Voice_Now))
                    {
                        File.Delete(Voice_Now);
                    }
                }
                if (Name_Only == "keep")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/quick_commands_reloading" + Path.GetExtension(Voice_Now), false);
                    if (File.Exists(Voice_Now))
                    {
                        File.Delete(Voice_Now);
                    }
                }
                if (Name_Only == "help")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/quick_commands_help_me" + Path.GetExtension(Voice_Now), false);
                    if (File.Exists(Voice_Now))
                    {
                        File.Delete(Voice_Now);
                    }
                }
                if (File.Exists(Voice_Now))
                {
                    File.Delete(Voice_Now);
                }
                if (Name_Only == "capture")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/quick_commands_capture_base" + Path.GetExtension(Voice_Now), false);
                    if (File.Exists(Voice_Now))
                    {
                        File.Delete(Voice_Now);
                    }
                }
                if (Name_Only == "defence")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/quick_commands_defend_base" + Path.GetExtension(Voice_Now), false);
                    if (File.Exists(Voice_Now))
                    {
                        File.Delete(Voice_Now);
                    }
                }
                if (Name_Only == "attack")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/quick_commands_attack" + Path.GetExtension(Voice_Now), false);
                    if (File.Exists(Voice_Now))
                    {
                        File.Delete(Voice_Now);
                    }
                }
                if (Name_Only == "attack_now")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/quick_commands_attack_target" + Path.GetExtension(Voice_Now), false);
                    if (File.Exists(Voice_Now))
                    {
                        File.Delete(Voice_Now);
                    }
                }
                if (Name_Only == "kasai")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/fire_started_" + File_Rename_Number("fire_started") + Path.GetExtension(Voice_Now), false);
                }
                if (Name_Only == "syouka")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/fire_stopped_" + File_Rename_Number("fire_stopped") + Path.GetExtension(Voice_Now), false);
                }
                if (Name_Only == "ritaitaiha")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/track_destroyed_" + File_Rename_Number("track_destroyed") + Path.GetExtension(Voice_Now), false);
                }
                if (Name_Only == "ritaihason")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/track_damaged_" + File_Rename_Number("track_damaged") + Path.GetExtension(Voice_Now), false);
                }
                if (Name_Only == "ritaihukkyuu")
                {
                    File.Copy(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/track_functional_" + File_Rename_Number("track_functional") + Path.GetExtension(Voice_Now), true);
                    File.Copy(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/track_functional_can_move_" + File_Rename_Number("track_functional_can_move") + Path.GetExtension(Voice_Now), true);
                    File.Delete(Voice_Now);
                }
                if (Name_Only == "housinhason")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/gun_damaged_" + File_Rename_Number("gun_damaged") + Path.GetExtension(Voice_Now), false);
                }
                if (Name_Only == "housintaiha")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/gun_destroyed_" + File_Rename_Number("gun_destroyed") + Path.GetExtension(Voice_Now), false);
                }
                if (Name_Only == "housinhukkyuu")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/gun_functional_" + File_Rename_Number("gun_functional") + Path.GetExtension(Voice_Now), false);
                }
                if (Name_Only == "musen")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/radio_damaged_" + File_Rename_Number("radio_damaged") + Path.GetExtension(Voice_Now), false);
                }
                if (Name_Only == "kansokuhason")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/surveying_devices_damaged_" + File_Rename_Number("surveying_devices_damaged") + Path.GetExtension(Voice_Now), false);
                }
                if (Name_Only == "kansokuhukkyuu")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/surveying_devices_functional_" + File_Rename_Number("surveying_devices_functional") + Path.GetExtension(Voice_Now), false);
                }
                if (Name_Only == "kansokutaiha")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/surveying_devices_destroyed_" + File_Rename_Number("surveying_devices_destroyed") + Path.GetExtension(Voice_Now), false);
                }
                if (Name_Only == "houtouhason")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/turret_rotator_damaged_" + File_Rename_Number("turret_rotator_damaged") + Path.GetExtension(Voice_Now), false);
                }
                if (Name_Only == "houtouhukkyuu")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/turret_rotator_functional_" + File_Rename_Number("turret_rotator_functional") + Path.GetExtension(Voice_Now), false);
                }
                if (Name_Only == "houtoutaiha")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/turret_rotator_destroyed_" + File_Rename_Number("turret_rotator_destroyed") + Path.GetExtension(Voice_Now), false);
                }
                if (Name_Only == "danyaku")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/ammo_bay_damaged_" + File_Rename_Number("ammo_bay_damaged") + Path.GetExtension(Voice_Now), false);
                }
                if (Name_Only == "enjinhukkyuu")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/engine_functional_" + File_Rename_Number("engine_functional") + Path.GetExtension(Voice_Now), false);
                }
                if (Name_Only == "enjintaiha")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/engine_destroyed_" + File_Rename_Number("engine_destroyed") + Path.GetExtension(Voice_Now), false);
                }
                if (Name_Only == "enjinhason")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/engine_damaged_" + File_Rename_Number("engine_damaged") + Path.GetExtension(Voice_Now), false);
                }
                if (Name_Only == "nenryou")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/fuel_tank_damaged_" + File_Rename_Number("fuel_tank_damaged") + Path.GetExtension(Voice_Now), false);
                }
                if (Name_Only == "musensyu")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/radioman_killed_" + File_Rename_Number("radioman_killed") + Path.GetExtension(Voice_Now), false);
                }
                if (Name_Only == "soutensyu")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/loader_killed_" + File_Rename_Number("loader_killed") + Path.GetExtension(Voice_Now), false);
                }
                if (Name_Only == "housyu")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/gunner_killed_" + File_Rename_Number("gunner_killed") + Path.GetExtension(Voice_Now), false);
                }
                if (Name_Only == "souzyuusyu")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/driver_killed_" + File_Rename_Number("driver_killed") + Path.GetExtension(Voice_Now), false);
                }
                if (Name_Only == "syatyou")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/commander_killed_" + File_Rename_Number("commander_killed") + Path.GetExtension(Voice_Now), false);
                }
                if (Name_Only == "gekiha")
                {
                    File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/enemy_killed_by_player_" + File_Rename_Number("enemy_killed_by_player") + Path.GetExtension(Voice_Now), false);
                }
            }
        }
        //音声を指定したファイル数にする(ファイルがなければ依存のファイルで補い、ファイル数を超えていれば削除)
        static void Set_Voice_Number(string Voice_Dir, string Voice_Type, int Number)
        {
            int File_Number = 0;
            for (int Number2 = 0; Number2 <= 9; Number2++)
            {
                string[] Files2 = Directory.GetFiles(Voice_Dir, Voice_Type + "_" + Number2 + "*", SearchOption.TopDirectoryOnly);
                File_Number += Files2.Length;
            }
            if (File_Number == 0)
            {
                for (int Number2 = 1; Number2 <= Number; Number2++)
                {
                    try
                    {
                        if (Number2 < 10)
                        {
                            File.Copy(Voice_Set.Special_Path + "/Fmod_Android_Create/Not_Voice.mp3", Voice_Dir + "/" + Voice_Type + "_0" + Number2 + ".mp3", true);
                        }
                        else
                        {
                            File.Copy(Voice_Set.Special_Path + "/Fmod_Android_Create/Not_Voice.mp3", Voice_Dir + "/" + Voice_Type + "_" + Number2 + ".mp3", true);
                        }
                    }
                    catch
                    {

                    }
                }
                return;
            }
            if (File_Number > Number)
            {
                for (int Number2 = Number + 1; Number2 <= File_Number; Number2++)
                {
                    try
                    {
                        if (Number2 < 10)
                        {
                            File.Delete(File_Get_FileName_No_Extension(Voice_Dir + "/" + Voice_Type + "_0" + Number2));
                        }
                        else
                        {
                            File.Delete(File_Get_FileName_No_Extension(Voice_Dir + "/" + Voice_Type + "_" + Number2));
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            else if (File_Number < Number)
            {
                Random r = new Random();
                for (int Number2 = File_Number; Number2 <= Number; Number2++)
                {
                    int Test = 0;
                    start:
                    Test++;
                    try
                    {
                        int r2 = r.Next(1, File_Number + 1);
                        string r3;
                        if (r2 < 10)
                        {
                            r3 = "0" + r2;
                        }
                        else
                        {
                            r3 = r2.ToString();
                        }
                        string FilePath = File_Get_FileName_No_Extension(Voice_Dir + "/" + Voice_Type + "_" + r3);
                        if (Number2 < 10)
                        {
                            File.Copy(FilePath, Voice_Dir + "/" + Voice_Type + "_0" + Number2 + Path.GetExtension(FilePath), true);
                        }
                        else
                        {
                            File.Copy(FilePath, Voice_Dir + "/" + Voice_Type + "_" + Number2 + Path.GetExtension(FilePath), true);
                        }
                    }
                    catch
                    {
                        if (Test >= 10)
                        {
                            continue;
                        }
                        goto start;
                    }
                }
            }
        }
        //音声の数をWoTBに合わせる(音声の数が規定以上だった場合その音声は削除され、なければ今ある音声の中からランダムでコピーされる)
        static void Ingame_Voice_Set_Number(string Voice_Dir)
        {
            Set_Voice_Number(Voice_Dir, "howitzer_load", 5);
            Set_Voice_Number(Voice_Dir, "lamp", 1);
            Set_Voice_Number(Voice_Dir, "ally_killed_by_player", 2);
            Set_Voice_Number(Voice_Dir, "vehicle_destroyed", 3);
            Set_Voice_Number(Voice_Dir, "armor_pierced_by_player", 12);
            Set_Voice_Number(Voice_Dir, "armor_pierced_crit_by_player", 9);
            Set_Voice_Number(Voice_Dir, "armor_not_pierced_by_player", 5);
            Set_Voice_Number(Voice_Dir, "armor_ricochet_by_player", 7);
            Set_Voice_Number(Voice_Dir, "enemy_fire_started_by_player", 4);
            Set_Voice_Number(Voice_Dir, "start_battle", 8);
            Set_Voice_Number(Voice_Dir, "fire_started", 2);
            Set_Voice_Number(Voice_Dir, "fire_stopped", 3);
            Set_Voice_Number(Voice_Dir, "track_destroyed", 4);
            Set_Voice_Number(Voice_Dir, "track_damaged", 4);
            Set_Voice_Number(Voice_Dir, "track_functional", 4);
            Set_Voice_Number(Voice_Dir, "track_functional_can_move", 5);
            Set_Voice_Number(Voice_Dir, "gun_damaged", 4);
            Set_Voice_Number(Voice_Dir, "gun_destroyed", 3);
            Set_Voice_Number(Voice_Dir, "gun_functional", 4);
            Set_Voice_Number(Voice_Dir, "radio_damaged", 5);
            Set_Voice_Number(Voice_Dir, "surveying_devices_damaged", 5);
            Set_Voice_Number(Voice_Dir, "surveying_devices_functional", 6);
            Set_Voice_Number(Voice_Dir, "surveying_devices_destroyed", 6);
            Set_Voice_Number(Voice_Dir, "turret_rotator_damaged", 2);
            Set_Voice_Number(Voice_Dir, "turret_rotator_functional", 2);
            Set_Voice_Number(Voice_Dir, "turret_rotator_destroyed", 2);
            Set_Voice_Number(Voice_Dir, "ammo_bay_damaged", 3);
            Set_Voice_Number(Voice_Dir, "engine_functional", 2);
            Set_Voice_Number(Voice_Dir, "engine_destroyed", 3);
            Set_Voice_Number(Voice_Dir, "engine_damaged", 5);
            Set_Voice_Number(Voice_Dir, "fuel_tank_damaged", 4);
            Set_Voice_Number(Voice_Dir, "radioman_killed", 1);
            Set_Voice_Number(Voice_Dir, "loader_killed", 2);
            Set_Voice_Number(Voice_Dir, "gunner_killed", 3);
            Set_Voice_Number(Voice_Dir, "driver_killed", 3);
            Set_Voice_Number(Voice_Dir, "commander_killed", 3);
            Set_Voice_Number(Voice_Dir, "enemy_killed_by_player", 9);
        }
        //指定した音声にSEを合わせる
        static void Ingame_Voice_In_SE_By_FileName(string Voice_Path, string SE_Path, string To_Dir)
        {
            string OutPath = To_Dir + "/" + Path.GetFileName(Voice_Path);
            StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Encode_Mp3/Voice_In_SE.bat");
            stw.WriteLine("chcp 65001");
            stw.Write(Voice_Set.Special_Path + "/Encode_Mp3/ffmpeg.exe -y -i " + Voice_Path + " -i " + SE_Path + " -filter_complex amix=inputs=2:duration=longest " + OutPath);
            stw.Close();
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = Voice_Set.Special_Path + "/Encode_Mp3/Voice_In_SE.bat",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process p = Process.Start(processStartInfo);
            p.WaitForExit();
        }
        //指定した音声フォルダの中身のファイルすべてにSEを付ける(SEが有効な場合のみ)
        static void Ingame_Voice_In_SE_By_Dir(string Voice_Dir, string SE_Dir, string To_Dir)
        {
            try
            {
                if (Directory.Exists(To_Dir))
                {
                    Directory.Delete(To_Dir, true);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("エラーが発生しました。\n" + e.Message);
            }
            Directory.CreateDirectory(To_Dir);
            Random r = new Random();
            string[] Voice_Files = Directory.GetFiles(Voice_Dir, "*", SearchOption.TopDirectoryOnly);
            foreach (string Voice_Now in Voice_Files)
            {
                string Voice_Type = Voice_Mod_Create.Get_Voice_Type(Voice_Now);
                string NameOnly = Path.GetFileNameWithoutExtension(Voice_Now);
                if (Voice_Type == "howitzer_load" && Voice_Set.SE_Enable_List[9])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, File_Get_FileName_No_Extension(SE_Dir + "/Reload_0" + r.Next(1, 7)), To_Dir);
                    continue;
                }
                if (Voice_Type == "lamp" && Voice_Set.SE_Enable_List[10])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, File_Get_FileName_No_Extension(SE_Dir + "/Sixth_0" + r.Next(1, 4)), To_Dir);
                    continue;
                }
                if (Voice_Type == "vehicle_destroyed" && Voice_Set.SE_Enable_List[3])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, File_Get_FileName_No_Extension(SE_Dir + "/Destroy_01"), To_Dir);
                    continue;
                }
                if (Voice_Type == "armor_pierced_by_player" && Voice_Set.SE_Enable_List[4])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, File_Get_FileName_No_Extension(SE_Dir + "/Enable_0" + r.Next(1, 4)), To_Dir);
                    continue;
                }
                if (Voice_Type == "armor_pierced_crit_by_player" && Voice_Set.SE_Enable_List[5])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, File_Get_FileName_No_Extension(SE_Dir + "/Enable_Special_01"), To_Dir);
                    continue;
                }
                if (Voice_Type == "armor_not_pierced_by_player" && Voice_Set.SE_Enable_List[8])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, File_Get_FileName_No_Extension(SE_Dir + "/Not_Enable_01"), To_Dir);
                    continue;
                }
                if (Voice_Type == "armor_ricochet_by_player" && Voice_Set.SE_Enable_List[8])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, File_Get_FileName_No_Extension(SE_Dir + "/Not_Enable_01"), To_Dir);
                    continue;
                }
                if (Voice_Type == "radio_damaged" && Voice_Set.SE_Enable_List[6])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, File_Get_FileName_No_Extension(SE_Dir + "/Musenki_01"), To_Dir);
                    continue;
                }
                if (Voice_Type == "ammo_bay_damaged" && Voice_Set.SE_Enable_List[2])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, File_Get_FileName_No_Extension(SE_Dir + "/Danyaku_SE_01"), To_Dir);
                    continue;
                }
                if (Voice_Type == "fuel_tank_damaged" && Voice_Set.SE_Enable_List[7])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, File_Get_FileName_No_Extension(SE_Dir + "/Nenryou_SE_01"), To_Dir);
                    continue;
                }
                if (Voice_Type == "enemy_killed_by_player" && Voice_Set.SE_Enable_List[4])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, File_Get_FileName_No_Extension(SE_Dir + "/Enable_0" + r.Next(1, 4)), To_Dir);
                    continue;
                }
                if (NameOnly == "enemy_sighted" && Voice_Set.SE_Enable_List[11])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, File_Get_FileName_No_Extension(SE_Dir + "/Spot_01"), To_Dir);
                    continue;
                }
                if (NameOnly == "capture_end" && Voice_Set.SE_Enable_List[0])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, File_Get_FileName_No_Extension(SE_Dir + "/Capture_End_0" + r.Next(1, 3)), To_Dir);
                    continue;
                }
                if (NameOnly == "auto_target_on" && Voice_Set.SE_Enable_List[13])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, File_Get_FileName_No_Extension(SE_Dir + "/Lock_01"), To_Dir);
                    continue;
                }
                if (NameOnly == "auto_target_off" && Voice_Set.SE_Enable_List[14])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, File_Get_FileName_No_Extension(SE_Dir + "/Unlock_01"), To_Dir);
                    continue;
                }
                if (NameOnly == "quick_commands_positive" && Voice_Set.SE_Enable_List[1])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, File_Get_FileName_No_Extension(SE_Dir + "/Command_01"), To_Dir);
                    continue;
                }
                if (NameOnly == "quick_commands_negative" && Voice_Set.SE_Enable_List[1])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, File_Get_FileName_No_Extension(SE_Dir + "/Command_01"), To_Dir);
                    continue;
                }
                if (NameOnly == "quick_commands_reloading" && Voice_Set.SE_Enable_List[1])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, File_Get_FileName_No_Extension(SE_Dir + "/Command_01"), To_Dir);
                    continue;
                }
                if (NameOnly == "quick_commands_help_me" && Voice_Set.SE_Enable_List[1])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, File_Get_FileName_No_Extension(SE_Dir + "/Command_01"), To_Dir);
                    continue;
                }
                if (NameOnly == "quick_commands_capture_base" && Voice_Set.SE_Enable_List[1])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, File_Get_FileName_No_Extension(SE_Dir + "/Command_01"), To_Dir);
                    continue;
                }
                if (NameOnly == "quick_commands_defend_base" && Voice_Set.SE_Enable_List[1])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, File_Get_FileName_No_Extension(SE_Dir + "/Command_01"), To_Dir);
                    continue;
                }
                if (NameOnly == "quick_commands_attack" && Voice_Set.SE_Enable_List[1])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, File_Get_FileName_No_Extension(SE_Dir + "/Command_01"), To_Dir);
                    continue;
                }
                if (NameOnly == "quick_commands_attack_target" && Voice_Set.SE_Enable_List[1])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, File_Get_FileName_No_Extension(SE_Dir + "/Command_01"), To_Dir);
                    continue;
                }
                File.Copy(Voice_Now, To_Dir + "/" + Path.GetFileName(Voice_Now), true);
            }
        }
    }
}