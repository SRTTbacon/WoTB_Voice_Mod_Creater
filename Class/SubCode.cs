using Microsoft.Win32;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace WoTB_Voice_Mod_Creater
{
    public class Sub_Code
    {
        public const string Random_String = "0123456789abcdefghijklmnopqrstuvwxyz";
        public const double Window_Feed_Time = 0.04;
        static List<string> IsAutoListAdd = new List<string>();
        public static Random r = new Random();
        static string IsLanguage = "";
        public static string IsWwise_Blitz_Actor_Update = "1.0";
        public static string IsWwise_Blitz_Update = "1.0";
        public static string IsWwise_Hits_Update = "1.0";
        public static string IsWwise_Gun_Update = "1.0";
        public static string IsWwise_WoT_Update = "1.0";
        public static string IsWwise_Player_Update = "1.0";
        public static bool IsWindowBarShow = true;
        public static readonly string[] Default_Name = { "ally_killed_by_player", "ammo_bay_damaged", "armor_not_pierced_by_player", "armor_pierced_by_player", "armor_pierced_crit_by_player", "armor_ricochet_by_player",
        "commander_killed","driver_killed","enemy_fire_started_by_player","enemy_killed_by_player","engine_damaged","engine_destroyed","engine_functional","fire_started","fire_stopped",
        "fuel_tank_damaged","gun_damaged","gun_destroyed","gun_functional","gunner_killed","loader_killed","radio_damaged","radioman_killed","start_battle","surveying_devices_damaged",
        "surveying_devices_destroyed","surveying_devices_functional","track_damaged","track_destroyed","track_functional","turret_rotator_damaged","turret_rotator_destroyed","turret_rotator_functional","vehicle_destroyed"};
        public static BitmapFrame Check_01;
        public static BitmapFrame Check_02;
        public static BitmapFrame Check_03;
        public static BitmapFrame Check_04;
        static bool IsServerCreating = false;
        static bool IsCreatingProject = false;
        static bool IsVolumeSet = false;
        static bool IsDVPLEncode = false;
        static bool IsModChange = false;
        static bool IsAndroidMode = false;
        static bool IsDefaultVoiceMode = false;
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
        public static string SetLanguage
        {
            get { return IsLanguage; }
            set { IsLanguage = value; }
        }
        public static bool Default_Voice
        {
            get { return IsDefaultVoiceMode; }
            set { IsDefaultVoiceMode = value; }
        }
        //必要なdllがない場合そのdll名のリストを返す
        public static List<string> DLL_Exists()
        {
            string DLL_Path = Directory.GetCurrentDirectory() + "/dll";
            List<string> DLL_List = new List<string>();
            if (!File.Exists(DLL_Path + "/bass.dll"))
                DLL_List.Add("bass.dll");
            if (!File.Exists(DLL_Path + "/bass_fx.dll"))
                DLL_List.Add("bass_fx.dll");
            if (!File.Exists(DLL_Path + "/DdsFileTypePlusIO_x86.dll"))
                DLL_List.Add("DdsFileTypePlusIO_x86.dll");
            if (!File.Exists(DLL_Path + "/fmod_event.dll"))
                DLL_List.Add("fmod_event.dll");
            if (!File.Exists(DLL_Path + "/fmodex.dll"))
                DLL_List.Add("fmodex.dll");
            if (!File.Exists(DLL_Path + "/bassenc.dll"))
                DLL_List.Add("bassenc.dll");
            if (!File.Exists(DLL_Path + "/bassmix.dll"))
                DLL_List.Add("bassmix.dll");
            if (!File.Exists(DLL_Path + "/libmp3lame.32.dll"))
                DLL_List.Add("libmp3lame.32.dll");
            return DLL_List;
        }
        //.dvplを抜いたファイルパスからファイルが存在するか
        //例:sounds.yaml.dvpl -> DVPL_File_Exists(sounds.yaml) -> true,false
        public static bool DVPL_File_Exists(string File_Path)
        {
            if (File.Exists(File_Path) || File.Exists(File_Path + ".dvpl"))
                return true;
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
                            FileEncode.FileEncryptor.Encrypt(eifs, eofs, "WoTB_Directory_Path_Pass");
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
                                using (var eofs = new FileStream(Directory.GetCurrentDirectory() + "/WoTB_Path.dat", FileMode.Create, FileAccess.Write))
                                    FileEncode.FileEncryptor.Encrypt(eifs, eofs, "WoTB_Directory_Path_Pass");
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
        //WoTのディレクトリを取得
        public static bool WoT_Get_ModDirectory()
        {
            try
            {
                RegistryKey rKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\WOT.ASIA.PRODUCTION");
                string location = (string)rKey.GetValue("InstallLocation");
                rKey.Close();
                if (File.Exists(location + "/WorldOfTanks.exe"))
                {
                    string[] ModDirs = Directory.GetDirectories(location + "/res_mods");
                    List<double> Version_List = new List<double>();
                    List<string> ModDir_List = new List<string>();
                    foreach (string Dir_Now in ModDirs)
                    {
                        string Dir_Name_Only = Path.GetFileName(Dir_Now);
                        if (!Dir_Name_Only.Contains('.'))
                            continue;
                        try
                        {
                            string Temp_01 = Dir_Name_Only.Substring(Dir_Name_Only.IndexOf('.') + 1).Replace(".", "");
                            string Temp_02 = Dir_Name_Only.Substring(0, Dir_Name_Only.IndexOf('.') + 1) + Temp_01;
                            Version_List.Add(double.Parse(Temp_02));
                            ModDir_List.Add(Dir_Now);
                        }
                        catch
                        {

                        }
                    }
                    //最新バージョンのフォルダ名を取得
                    if (Version_List.Count > 0)
                    {
                        // 最大の要素を取得
                        double max = 0;
                        foreach (double e in Version_List)
                            if (max < e) max = e;
                        Voice_Set.WoT_Mod_Path = ModDir_List[Version_List.IndexOf(max)];
                        Version_List.Clear();
                        ModDir_List.Clear();
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        //ディレクトリをコピー(サブフォルダを含む)
        public static void Directory_Copy(string From_Dir, string To_Dir)
        {
            try
            {
                if (!Directory.Exists(To_Dir))
                    Directory.CreateDirectory(To_Dir);
                DirectoryInfo dir = new DirectoryInfo(From_Dir);
                if (!dir.Exists)
                    return;
                DirectoryInfo[] dirs = dir.GetDirectories();
                Directory.CreateDirectory(To_Dir);
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    string tempPath = Path.Combine(To_Dir, file.Name);
                    file.CopyTo(tempPath, true);
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
                if (File.Exists(FromFilePath + ".dvpl"))
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
            if (File.Exists(FilePath + ".dvpl"))
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
        //.dvplを抜いたファイルを移動
        public static bool DVPL_File_Move(string From_File, string To_File, bool IsOverWrite)
        {
            bool IsMoved = false;
            From_File = From_File.Replace(".dvpl", "");
            if (File.Exists(From_File))
                IsMoved = File_Move(From_File, To_File, IsOverWrite);
            if (File.Exists(From_File + ".dvpl"))
                IsMoved = File_Move(From_File + ".dvpl", To_File + ".dvpl", IsOverWrite);
            return IsMoved;
        }
        //ファイルを移動(正確にはコピーして元ファイルを削除)
        public static bool File_Move(string From_File_Path, string To_File_Path, bool IsOverWrite)
        {
            if (!File.Exists(From_File_Path))
                return false;
            if (File.Exists(To_File_Path) && !IsOverWrite)
                return false;
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
            string Dir = Path.GetDirectoryName(From_File_Path);
            string Name = Path.GetFileName(From_File_Path);
            var files = Directory.GetFiles(Dir, Name + ".*");
            if (files.Length > 0)
                File_Path = files[0];
            if (File_Path == "" || !File.Exists(From_File_Path))
                return false;
            if (File_Exists(To_File_Path) && !IsOverWrite)
                return false;
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
        //指定した文字列を含むファイルをすべてコピー
        public static void File_Copy_V2(string From_Dir, string To_Dir, string Name)
        {
            if (!Directory.Exists(From_Dir))
                return;
            string[] Files = Directory.GetFiles(From_Dir, "*", SearchOption.TopDirectoryOnly);
            foreach (string File_Now in Files)
            {
                string Name_Only = Path.GetFileName(File_Now);
                if (File_Now.Contains(Name))
                {
                    try
                    {
                        File.Copy(File_Now, To_Dir + "\\" + Name_Only, true);
                    }
                    catch { }
                }
            }
        }
        //ファイル拡張子を指定しないでファイルを削除
        public static bool File_Delete(string File_Path)
        {
            string Dir = Path.GetDirectoryName(File_Path);
            string Name = Path.GetFileName(File_Path);
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
                string Dir = Path.GetDirectoryName(File_Path);
                string Name = Path.GetFileName(File_Path);
                var files = Directory.GetFiles(Dir, Name + ".*");
                if (files.Length > 0)
                    return true;
                else
                    return false;
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
                string Dir = Path.GetDirectoryName(Path_01);
                string Name = Path.GetFileName(Path_01);
                var files = Directory.GetFiles(Dir, Name + "_01.*");
                if (files.Length > 0)
                    return true;
                else
                    return false;
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
                string Dir = Path.GetDirectoryName(File_Path);
                string Name = Path.GetFileName(File_Path);
                var files = Directory.GetFiles(Dir, Name + ".*");
                if (files.Length > 0)
                    return files[0];
                else
                    return "";
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
                if (Voice_Set.Voice_Name_Hide(Dir_List[Number]))
                    Voice_List_Type.Add(Path.GetFileName(Dir_List[Number]));
            List<string> Voice_Type_Ref = new List<string>();
            List<int> Voice_Type_Number_Ref = new List<int>();
            int Voice_Type_Number = 0;
            string Name_Now = "";
            for (int Number = 0; Number < Voice_List_Type.Count; Number++)
            {
                string Name_Only = Voice_List_Type[Number].Substring(0, Voice_List_Type[Number].LastIndexOf('_'));
                if (Name_Now != Name_Only)
                {
                    if (Name_Now != "")
                        Voice_Type_Number_Ref.Add(Voice_Type_Number);
                    Name_Now = Name_Only;
                    Voice_Type_Number = 1;
                    Voice_Type_Ref.Add(Voice_Set.Get_Voice_Type_Japanese_Name(Name_Only));
                }
                else
                    Voice_Type_Number++;
            }
            Voice_Type_Number_Ref.Add(Voice_Type_Number);
            Voice_Type = Voice_Type_Ref;
            Voice_Number = Voice_Type_Number_Ref;
        }
        //音声タイプの名前に変換
        //例:Indexが2で既にそのタイプのファイル数が3個ある場合 -> danyaku_04.mp3
        public static string Set_Voice_Type_Change_Name_By_Index(string Dir, List<List<string>> Lists)
        {
            if (!Directory.Exists(Dir))
                Directory.CreateDirectory(Dir);
            int Romaji_Number = 0;
            foreach (List<string> Index in Lists)
            {
                int File_Number = 1;
                foreach (string File_Path in Index)
                {
                    try
                    {
                        if (File_Number < 10)
                            File.Copy(File_Path, Dir + "/" + Voice_Set.Get_Voice_Type_Romaji_Name(Romaji_Number) + "_0" + File_Number + Path.GetExtension(File_Path), true);
                        else
                            File.Copy(File_Path, Dir + "/" + Voice_Set.Get_Voice_Type_Romaji_Name(Romaji_Number) + "_" + File_Number + Path.GetExtension(File_Path), true);
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
        //↑の拡張子もフォルダ名もいらないバージョン
        public static string Set_Voice_Type_Change_Name_By_Index(string From_Dir, string To_Dir, List<List<string>> Lists, List<List<bool>> Lists_Enable = null)
        {
            if (!Directory.Exists(To_Dir))
                Directory.CreateDirectory(To_Dir);
            int Romaji_Number = 0;
            foreach (List<string> Index in Lists)
            {
                if (Romaji_Number == 5 && Index.Count == 0)
                    Change_Name_01(Lists, Lists_Enable, 5, 2, From_Dir, To_Dir);
                else if (Romaji_Number == 4 && Index.Count == 0)
                    Change_Name_01(Lists, Lists_Enable, 4, 3, From_Dir, To_Dir);
                else
                {
                    int File_Number = 1;
                    int File_Number_01 = -1;
                    foreach (string File_Path in Index)
                    {
                        try
                        {
                            File_Number_01++;
                            if (Lists_Enable != null && !Lists_Enable[Romaji_Number][File_Number_01])
                                continue;
                            string File_Path_01 = File_Get_FileName_No_Extension(From_Dir + "/" + File_Path);
                            if (File_Number < 10)
                                File.Copy(File_Path_01, To_Dir + "/" + Voice_Set.Get_Voice_Type_Romaji_Name(Romaji_Number) + "_0" + File_Number + Path.GetExtension(File_Path_01), true);
                            else
                                File.Copy(File_Path_01, To_Dir + "/" + Voice_Set.Get_Voice_Type_Romaji_Name(Romaji_Number) + "_" + File_Number + Path.GetExtension(File_Path_01), true);
                            File_Number++;
                        }
                        catch (Exception e)
                        {
                            Sub_Code.Error_Log_Write(e.Message);
                            return "ファイルをコピーできませんでした。";
                        }
                    }
                }
                Romaji_Number++;
            }
            return "";
        }
        static void Change_Name_01(List<List<string>> Lists, List<List<bool>> Lists_Enable, int FromIndexNumber, int ToIndexNumber, string From_Dir, string To_Dir)
        {
            int File_Number = 1;
            int File_Number_01 = -1;
            foreach (string File_Path in Lists[ToIndexNumber])
            {
                try
                {
                    File_Number_01++;
                    if (Lists_Enable != null && !Lists_Enable[ToIndexNumber][File_Number_01])
                        continue;
                    string File_Path_01 = File_Get_FileName_No_Extension(From_Dir + "/" + File_Path);
                    if (File_Number < 10)
                        File.Copy(File_Path_01, To_Dir + "/" + Voice_Set.Get_Voice_Type_Romaji_Name(FromIndexNumber) + "_0" + File_Number + Path.GetExtension(File_Path_01), true);
                    else
                        File.Copy(File_Path_01, To_Dir + "/" + Voice_Set.Get_Voice_Type_Romaji_Name(FromIndexNumber) + "_" + File_Number + Path.GetExtension(File_Path_01), true);
                    File_Number++;
                }
                catch (Exception e)
                {
                    Sub_Code.Error_Log_Write(e.Message);
                }
            }
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
                        File_Import = "\"" + File_Now + "\"";
                    else
                        File_Import += " \"" + File_Now + "\"";
                }
                if (File_Import != "")
                {
                    StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Encode_Mp3/Volume_Set.bat");
                    stw.WriteLine("chcp 65001");
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
        //MP3形式のファイルの音量を調整
        public static void MP3_Volume_Set(string To_Dir, int Gain = 10)
        {
            string File_Import = "";
            string[] Files_03 = Directory.GetFiles(To_Dir, "*.mp3", SearchOption.TopDirectoryOnly);
            foreach (string File_Now in Files_03)
            {
                if (File_Import == "")
                    File_Import = "\"" + File_Now + "\"";
                else
                    File_Import += " \"" + File_Now + "\"";
                //Windowsのコマンドプロンプトは8191文字までしか入力できないため、この時点で8000文字を超えていたら実行
                if (File_Import.Length > 8000)
                {
                    Volume_Set_Start(File_Import, Gain);
                    File_Import = "";
                }
            }
            if (File_Import != "")
                Volume_Set_Start(File_Import, Gain);
        }
        public static void Volume_Set_Start(string File_Import, int Gain = 10)
        {
            StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Encode_Mp3/Volume_Set.bat");
            stw.WriteLine("chcp 65001");
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
        //ファイルがMP3形式か判定してファイル拡張子を.rawに変更する
        public static void Check_MP3_Rename(string Dir)
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
        }
        public static List<string> Check_MP3_Get_List(string Dir, bool IsRename)
        {
            List<string> Voice_List = new List<string>();
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
                    if (Path.GetExtension(File_Now) == ".mp3" && IsRename)
                    {
                        string To_File = Path.GetDirectoryName(File_Now) + "\\" + Path.GetFileNameWithoutExtension(File_Now) + ".raw";
                        Sub_Code.File_Move(File_Now, To_File, false);
                        Voice_List.Add(To_File);
                    }
                    else
                        Voice_List.Add(File_Now);
                }
            }
            return Voice_List;
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
                    Message_T.Text = "FSBファイルを作成しています.";
                else if (Number_01 == 1)
                    Message_T.Text = "FSBファイルを作成しています..";
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
                return "";
            if (First == End)
                return Get_Time_Index(dt, First);
            string Temp = "";
            for (int Number = First; Number <= End; Number++)
            {
                if (Number != End)
                    Temp += Get_Time_Index(dt, Number) + Between;
                else
                    Temp += Get_Time_Index(dt, Number);
            }
            return Temp;
        }
        static string Get_Time_Index(DateTime dt, int Index)
        {
            if (Index > 0 && Index < 7)
            {
                if (Index == 1)
                    return dt.Year.ToString();
                else if (Index == 2)
                    return dt.Month.ToString();
                else if (Index == 3)
                    return dt.Day.ToString();
                else if (Index == 4)
                    return dt.Hour.ToString();
                else if (Index == 5)
                    return dt.Minute.ToString();
                else if (Index == 6)
                    return dt.Second.ToString();
            }
            return "";
        }
        //sounds.yamlの中身が古かった場合サーバーに置いてある最新のものと比較して置き換える
        //本当にWoTBの仕様が変わると思っていなかったのでこの方法でしていて良かったです...
        public static void Sounds_Yaml_Update(string File_Path, string To_Path, bool IsDVPLEncode)
        {
            try
            {
                Voice_Set.FTPClient.DownloadFile("/WoTB_Voice_Mod/Mods/Backup/sounds.yaml.dvpl", Voice_Set.Special_Path + "/Temp_Download_Sounds.yaml.dvpl");
                DVPL.DVPL_UnPack(Voice_Set.Special_Path + "/Temp_Download_Sounds.yaml.dvpl", Voice_Set.Special_Path + "/Temp_Download_Sounds.yaml", true);
                string Server_File = Voice_Set.Special_Path + "/Temp_Download_Sounds.yaml";
                StreamReader str = new StreamReader(File_Path);
                bool IsSoundsIn = false;
                while (str.EndOfStream == false)
                {
                    string Line = str.ReadLine();
                    //この方法で大丈夫そうですが、念のためgui_sounds:を追加
                    if (Line.Contains("sounds:") || Line.Contains("gui_sounds:"))
                        IsSoundsIn = true;
                    if (IsSoundsIn)
                        Sounds_Yaml_IsUpdate(Server_File, Line);
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
                    DVPL.DVPL_Pack(Voice_Set.Special_Path + "/Temp_DVPL.yaml", To_Path, true);
                else
                    File.Copy(Voice_Set.Special_Path + "/Temp_DVPL.yaml", To_Path, true);
            }
            catch
            {
                //サーバーにsounds.yaml.dvplが存在しない場合
            }
        }
        static bool Sounds_Yaml_IsUpdate(string Server_File, string Line)
        {
            if (Line == "" || !Line.Contains(":"))
                return false;
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
                return;
            if (File.Exists(Out_File) && !IsOverWrite)
                return;
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
            File.Delete(Voice_Set.Special_Path + "/Encode_Mp3/Audio_Video_Convert.bat");
        }
        //エラーをログに記録(改行コードはあってもなくてもよい)
        public static void Error_Log_Write(string Text)
        {
            DateTime dt = DateTime.Now;
            string Time = Get_Time_Now(dt, ".", 1, 6);
            if (Text.EndsWith("\n"))
                File.AppendAllText(Directory.GetCurrentDirectory() + "/Error_Log.txt", Time + ":" + Text);
            else
                File.AppendAllText(Directory.GetCurrentDirectory() + "/Error_Log.txt", Time + ":" + Text + "\n");
        }
        //BitmapをBitmapImageへ変換
        public static System.Windows.Media.Imaging.BitmapImage Bitmap_To_BitmapImage(System.Drawing.Bitmap bitmap)
        {
            if (bitmap != null)
            {
                MemoryStream stream = new MemoryStream();
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Seek(0, SeekOrigin.Begin);
                System.Windows.Media.Imaging.BitmapImage img = new System.Windows.Media.Imaging.BitmapImage();
                img.BeginInit();
                img.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                img.StreamSource = stream;
                img.EndInit();
                stream.Close();
                stream.Dispose();
                return img;
            }
            else
                return null;
        }
        public static System.Windows.Media.Imaging.BitmapImage Bitmap_To_BitmapImage(System.Drawing.Image bitmap)
        {
            MemoryStream stream = new MemoryStream();
            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            stream.Seek(0, SeekOrigin.Begin);
            System.Windows.Media.Imaging.BitmapImage img = new System.Windows.Media.Imaging.BitmapImage();
            img.BeginInit();
            img.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
            img.StreamSource = stream;
            img.EndInit();
            stream.Close();
            stream.Dispose();
            return img;
        }
        //ファイル名に使用できない文字を_に変更
        public static string File_Replace_Name(string FileName)
        {
            string valid = FileName;
            char[] invalidch = Path.GetInvalidFileNameChars();
            foreach (char c in invalidch)
                valid = valid.Replace(c, '_');
            return valid;
        }
        //指定したファイルが.wav形式だった場合true
        public static bool Audio_IsWAV(string File_Path)
        {
            bool Temp = false;
            try
            {
                using (FileStream fs = new FileStream(File_Path, FileMode.Open))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        if (Encoding.ASCII.GetString(br.ReadBytes(4)) == "RIFF")
                            Temp = true;
                    }
                }
            }
            catch
            {
            }
            return Temp;
        }
        //音声ファイルを指定した拡張子へエンコード
        public static bool Audio_Encode_To_Other(string From_Audio_File, string To_Audio_File, string Encode_Mode, bool IsFromFileDelete)
        {
            try
            {
                if (!File.Exists(From_Audio_File))
                    return false;
                Encode_Mode = Encode_Mode.Replace(".", "");
                string Encode_Style = "";
                //変換先に合わせて.batファイルを作成
                if (Encode_Mode == "aac")
                    Encode_Style = "-y -vn -strict experimental -c:a aac -b:a 144k";
                else if (Encode_Mode == "flac")
                    Encode_Style = "-y -vn -ac 2 -ar 44100 -acodec flac -f flac";
                else if (Encode_Mode == "mp3")
                    Encode_Style = "-y -vn -ac 2 -ar 44100 -ab 144k -acodec libmp3lame -f mp3";
                else if (Encode_Mode == "ogg")
                    Encode_Style = "-y -vn -ac 2 -ar 44100 -ab 144k -acodec libvorbis -f ogg";
                else if (Encode_Mode == "wav")
                    Encode_Style = "-y -vn -ac 2 -ar 44100 -acodec pcm_s24le -f wav";
                else if (Encode_Mode == "webm")
                    Encode_Style = "-y -vn -f opus -acodec libopus -ab 144k";
                else if (Encode_Mode == "wma")
                    Encode_Style = "-y -vn -ac 2 -ar 44100 -ab 144k -acodec wmav2 -f asf";
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Encode_Mp3/Audio_Encode.bat");
                stw.WriteLine("chcp 65001");
                stw.Write("\"" + Voice_Set.Special_Path + "/Encode_Mp3/ffmpeg.exe\" -i \"" + From_Audio_File + "\" " + Encode_Style + " \"" + To_Audio_File + "\"");
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
                    return false;
                if (IsFromFileDelete)
                    File.Delete(From_Audio_File);
                File.Delete(Voice_Set.Special_Path + "/Encode_Mp3/Audio_Encode.bat");
                return true;
            }
            catch
            {
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
                    return false;
                using (var eifs = new FileStream(From_File, FileMode.Open, FileAccess.Read))
                {
                    using (var eofs = new FileStream(To_File, FileMode.Create, FileAccess.Write))
                        FileEncode.FileEncryptor.Encrypt(eifs, eofs, Password);
                }
                if (IsFromFileDelete)
                    File.Delete(From_File);
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
                    return false;
                using (var eifs = new FileStream(From_File, FileMode.Open, FileAccess.Read))
                {
                    using (var eofs = new FileStream(To_File, FileMode.Create, FileAccess.Write))
                        FileEncode.FileEncryptor.Decrypt(eifs, eofs, Password);
                }
                if (IsFromFileDelete)
                    File.Delete(From_File);
                return true;
            }
            catch (Exception e)
            {
                Error_Log_Write(e.Message);
                return false;
            }
        }
        //フォルダ選択画面の初期フォルダを取得
        public static string Get_OpenDirectory_Path()
        {
            string InDir = "C:\\";
            if (File.Exists(Voice_Set.Special_Path + "/Configs/OpenDirectoryPath.dat"))
            {
                try
                {
                    Sub_Code.File_Decrypt(Voice_Set.Special_Path + "/Configs/OpenDirectoryPath.dat", Voice_Set.Special_Path + "/Configs/OpenDirectoryPath.tmp", "Directory_Save_SRTTbacon", false);
                    StreamReader str = new StreamReader(Voice_Set.Special_Path + "/Configs/OpenDirectoryPath.tmp");
                    string Read = str.ReadLine();
                    str.Close();
                    if (Directory.Exists(Read))
                        InDir = Read;
                    File.Delete(Voice_Set.Special_Path + "/Configs/OpenDirectoryPath.tmp");
                }
                catch
                {
                }
            }
            return InDir;
        }
        //フォルダ選択画面の初期フォルダを更新
        public static bool Set_Directory_Path(string Dir)
        {
            if (!Directory.Exists(Dir))
                return false;
            try
            {
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Configs/OpenDirectoryPath.tmp");
                stw.Write(Dir);
                stw.Close();
                Sub_Code.File_Encrypt(Voice_Set.Special_Path + "/Configs/OpenDirectoryPath.tmp", Voice_Set.Special_Path + "/Configs/OpenDirectoryPath.dat", "Directory_Save_SRTTbacon", true);
                return true;
            }
            catch
            {
                return false;
            }
        }
        //フォルダ内のファイルを削除
        public static void Directory_Delete(string Dir)
        {
            string[] Files = Directory.GetFiles(Dir, "*", SearchOption.AllDirectories);
            foreach (string File_Now in Files)
            {
                try
                {
                    File.Delete(File_Now);
                }
                catch
                {
                }
            }
            try
            {
                Directory.Delete(Dir, true);
            }
            catch
            {
            }
        }
        public static bool WEM_To_OGG_WAV(string From_WEM_File, string To_OGG_WAV_File, bool IsFromFileDelete)
        {
            try
            {
                if (!File.Exists(From_WEM_File))
                    return false;
                Wwise_Class.WEM_To_OGG.Create_OGG(From_WEM_File, Voice_Set.Special_Path + "\\Wwise\\Temp.ogg");
                if (File.Exists(Voice_Set.Special_Path + "\\Wwise\\Temp.ogg"))
                {
                    File_Move(Voice_Set.Special_Path + "\\Wwise\\Temp.ogg", To_OGG_WAV_File + ".ogg", true);
                    if (IsFromFileDelete)
                        File.Delete(From_WEM_File);
                    return true;
                }
                else
                {
                    Process WEMToWAV = new Process();
                    WEMToWAV.StartInfo.FileName = Voice_Set.Special_Path + "/WEM_To_WAV/WEM_To_WAV.exe";
                    WEMToWAV.StartInfo.WorkingDirectory = Voice_Set.Special_Path + "/Wwise";
                    WEMToWAV.StartInfo.Arguments = "-o \"" + Voice_Set.Special_Path + "\\Wwise\\Temp.wav\" \"" + From_WEM_File + "\"";
                    WEMToWAV.StartInfo.CreateNoWindow = true;
                    WEMToWAV.StartInfo.UseShellExecute = false;
                    WEMToWAV.StartInfo.RedirectStandardError = true;
                    WEMToWAV.StartInfo.RedirectStandardOutput = true;
                    WEMToWAV.Start();
                    WEMToWAV.WaitForExit();
                    if (File.Exists(Voice_Set.Special_Path + "\\Wwise\\Temp.wav"))
                    {
                        File_Move(Voice_Set.Special_Path + "\\Wwise\\Temp.wav", To_OGG_WAV_File + ".wav", true);
                        if (IsFromFileDelete)
                            File.Delete(From_WEM_File);
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        //.wemファイルを指定した形式に変換
        public static bool WEM_To_File(string From_WEM_File, string To_Audio_File, string Encode_Mode, bool IsFromFileDelete)
        {
            try
            {
                if (!File.Exists(From_WEM_File))
                    return false;
                /*Process wwToOgg = new Process();
                wwToOgg.StartInfo.FileName = Voice_Set.Special_Path + "/Wwise/ww2ogg.exe";
                wwToOgg.StartInfo.WorkingDirectory = Voice_Set.Special_Path + "/Wwise";
                wwToOgg.StartInfo.Arguments = "--pcb packed_codebooks_aoTuV_603.bin -o \"" + Voice_Set.Special_Path + "\\Wwise\\Temp.ogg\" \"" + From_WEM_File + "\"";
                wwToOgg.StartInfo.CreateNoWindow = true;
                wwToOgg.StartInfo.UseShellExecute = false;
                wwToOgg.StartInfo.RedirectStandardError = true;
                wwToOgg.StartInfo.RedirectStandardOutput = true;
                wwToOgg.Start();
                wwToOgg.WaitForExit();
                Process revorb = new Process();
                revorb.StartInfo.FileName = Voice_Set.Special_Path + "/Wwise/revorb.exe";
                revorb.StartInfo.Arguments = "\"" + Voice_Set.Special_Path + "\\Wwise\\Temp.ogg\"" + "\"";
                revorb.StartInfo.CreateNoWindow = true;
                revorb.StartInfo.UseShellExecute = false;
                revorb.StartInfo.RedirectStandardError = true;
                revorb.Start();
                revorb.WaitForExit();*/
                Wwise_Class.WEM_To_OGG.Create_OGG(From_WEM_File, Voice_Set.Special_Path + "\\Wwise\\Temp.ogg");
                if (File.Exists(Voice_Set.Special_Path + "\\Wwise\\Temp.ogg"))
                {
                    if (Encode_Mode == "ogg")
                    {
                        Process revorb = new Process();
                        revorb.StartInfo.FileName = Voice_Set.Special_Path + "/Encode_Mp3/ffmpeg.exe";
                        revorb.StartInfo.Arguments = "-y -i \"" + Voice_Set.Special_Path + "\\Wwise\\Temp.ogg\" -acodec copy \"" + To_Audio_File + "\"";
                        revorb.StartInfo.CreateNoWindow = true;
                        revorb.StartInfo.UseShellExecute = false;
                        revorb.StartInfo.RedirectStandardError = true;
                        revorb.Start();
                        revorb.WaitForExit();
                        File.Delete(Voice_Set.Special_Path + "\\Wwise\\Temp.ogg");
                    }
                    else if (Encode_Mode == "wav")
                    {
                        Un4seen.Bass.Misc.EncoderWAV w = new Un4seen.Bass.Misc.EncoderWAV(0);
                        w.InputFile = Voice_Set.Special_Path + "\\Wwise\\Temp.ogg";
                        w.OutputFile = To_Audio_File;
                        w.WAV_BitsPerSample = 24;
                        w.Start(null, IntPtr.Zero, false);
                        w.Stop();
                        File.Delete(Voice_Set.Special_Path + "\\Wwise\\Temp.ogg");
                    }
                    else
                        Sub_Code.Audio_Encode_To_Other(Voice_Set.Special_Path + "\\Wwise\\Temp.ogg", To_Audio_File, Encode_Mode, true);
                    if (IsFromFileDelete)
                        File.Delete(From_WEM_File);
                    return true;
                }
                else
                {
                    Process WEMToWAV = new Process();
                    WEMToWAV.StartInfo.FileName = Voice_Set.Special_Path + "/WEM_To_WAV/WEM_To_WAV.exe";
                    WEMToWAV.StartInfo.WorkingDirectory = Voice_Set.Special_Path + "/Wwise";
                    WEMToWAV.StartInfo.Arguments = "-o \"" + Voice_Set.Special_Path + "\\Wwise\\Temp.wav\" \"" + From_WEM_File + "\"";
                    WEMToWAV.StartInfo.CreateNoWindow = true;
                    WEMToWAV.StartInfo.UseShellExecute = false;
                    WEMToWAV.StartInfo.RedirectStandardError = true;
                    WEMToWAV.StartInfo.RedirectStandardOutput = true;
                    WEMToWAV.Start();
                    WEMToWAV.WaitForExit();
                    if (File.Exists(Voice_Set.Special_Path + "\\Wwise\\Temp.wav"))
                    {
                        if (Encode_Mode == "wav")
                            File_Move(Voice_Set.Special_Path + "\\Wwise\\Temp.wav", To_Audio_File, true);
                        else
                            Sub_Code.Audio_Encode_To_Other(Voice_Set.Special_Path + "\\Wwise\\Temp.wav", To_Audio_File, Encode_Mode, true);
                        if (IsFromFileDelete)
                            File.Delete(From_WEM_File);
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        //サウンドファイル(.mp3や.oggなど)を.wem形式に変換
        //内容によってかなり時間がかかります。
        public static bool File_To_WEM(string From_WAV_File, string To_WEM_File, bool IsOverWrite, bool IsFromFileDelete = false)
        {
            try
            {
                if (!File.Exists(From_WAV_File))
                    return false;
                if (File.Exists(To_WEM_File) && !IsOverWrite)
                    return false;
                using (FileStream fs = new FileStream(From_WAV_File, FileMode.Open))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        if (Encoding.ASCII.GetString(br.ReadBytes(4)) != "RIFF")
                            Sub_Code.Audio_Encode_To_Other(From_WAV_File, Voice_Set.Special_Path + "/Wwise/Project/Originals/SFX/song.wav", "wav", false);
                        else
                            File.Copy(From_WAV_File, Voice_Set.Special_Path + "/Wwise/Project/Originals/SFX/song.wav", true);
                    }
                }
                Create_Wwise_Project_XML(Voice_Set.Special_Path + "\\Wwise\\Project");
                Process wwToOgg = new Process();
                wwToOgg.StartInfo.FileName = Voice_Set.Special_Path + "/Wwise/x64/Release/bin/WwiseCLI.exe";
                wwToOgg.StartInfo.WorkingDirectory = Voice_Set.Special_Path + "/Wwise/x64/Release/bin";
                wwToOgg.StartInfo.Arguments = "\"" + Voice_Set.Special_Path + "\\Wwise\\Project\\Template.wproj\" -GenerateSoundBanks";
                wwToOgg.StartInfo.CreateNoWindow = true;
                wwToOgg.StartInfo.UseShellExecute = false;
                wwToOgg.StartInfo.RedirectStandardError = true;
                wwToOgg.StartInfo.RedirectStandardOutput = true;
                wwToOgg.Start();
                wwToOgg.WaitForExit();
                File.Delete(Voice_Set.Special_Path + "/Wwise/Project/Originals/SFX/song.wav");
                string GetWEMFile = Directory.GetFiles(Voice_Set.Special_Path + "/Wwise/Project/.cache/Windows/SFX", "*.wem", SearchOption.TopDirectoryOnly)[0];
                Sub_Code.File_Move(GetWEMFile, To_WEM_File, true);
                if (File.Exists(To_WEM_File))
                {
                    if (IsFromFileDelete)
                        File.Delete(From_WAV_File);
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                return false;
            }
        }
        static void Create_Wwise_Project_XML(string To_Project_Dir)
        {
            try
            {
                StreamWriter stw = File.CreateText(To_Project_Dir + "/GeneratedSoundBanks/Windows/SoundBanksInfo.xml");
                stw.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                          "<SoundBanksInfo Platform=\"Windows\" SchemaVersion=\"10\" SoundbankVersion=\"112\">" +
                          "	<RootPaths>" +
                          "		<ProjectRoot>" + To_Project_Dir + "\\</ProjectRoot>" +
                          "		<SourceFilesRoot>" + To_Project_Dir + "\\.cache\\Windows\\</SourceFilesRoot>\n" +
                          "		<SoundBanksRoot>" + To_Project_Dir + "\\GeneratedSoundBanks\\Windows\\</SoundBanksRoot>\n" +
                          "		<ExternalSourcesInputFile></ExternalSourcesInputFile>\n" +
                          "		<ExternalSourcesOutputRoot>" + To_Project_Dir + "\\GeneratedSoundBanks\\Windows</ExternalSourcesOutputRoot>\n" +
                          "	</RootPaths>\n" +
                          "	<DialogueEvents/>\n" +
                          "	<StreamedFiles>\n" +
                          "		<File Id=\"1071015983\" Language=\"SFX\">\n" +
                          "			<ShortName>song.wav</ShortName>\n" +
                          "			<Path>SFX\\song_B7537E32.wem</Path>\n" +
                          "		</File>\n" +
                          "	</StreamedFiles>\n" +
                          "	<SoundBanks>\n" +
                          "		<SoundBank Id=\"1355168291\" Language=\"SFX\">\n" +
                          "			<ShortName>Init</ShortName>\n" +
                          "			<Path>Init.bnk</Path>\n" +
                          "		</SoundBank>\n" +
                          "		<SoundBank Id=\"2289279978\" Language=\"SFX\">\n" +
                          "			<ShortName>RS_SOUNDBANK</ShortName>\n" +
                          "			<Path>RS_SOUNDBANK.bnk</Path>\n" +
                          "			<ReferencedStreamedFiles>\n" +
                          "				<File Id=\"1071015983\"/>\n" +
                          "			</ReferencedStreamedFiles>\n" +
                          "			<IncludedMemoryFiles>\n" +
                          "				<File Id=\"1071015983\" Language=\"SFX\">\n" +
                          "					<ShortName>song.wav</ShortName>\n" +
                          "					<Path>SFX\\song_B7537E32.wem</Path>\n" +
                          "					<PrefetchSize>0</PrefetchSize>\n" +
                          "				</File>\n" +
                          "			</IncludedMemoryFiles>\n" +
                          "		</SoundBank>\n" +
                          "	</SoundBanks>\n" +
                          "</SoundBanksInfo>\n");
                stw.Close();
            }
            catch (Exception e)
            {
                Error_Log_Write(e.Message);
            }
        }
        //音声のIDをすべて取得(国によって異なるので引数にjaやenを渡します)
        public static List<string> Get_Voices_ID(string Language)
        {
            List<string> Temp = new List<string>();
            if (!File.Exists(Voice_Set.Special_Path + "/Wwise/SoundbanksInfo.json"))
            {
                return Temp;
            }
            try
            {
                StreamReader str = new StreamReader(Voice_Set.Special_Path + "/Wwise/SoundbanksInfo.json");
                string Read_Line = "";
                int Number = 0;
                while ((Read_Line = str.ReadLine()) != null)
                {
                    if (Read_Line == "        \"Language\": \"" + Language + "\",")
                    {
                        string ID = File.ReadLines(Voice_Set.Special_Path + "/Wwise/SoundbanksInfo.json").Skip(Number - 1).First().Replace("        \"Id\": \"", "");
                        string Name = File.ReadLines(Voice_Set.Special_Path + "/Wwise/SoundbanksInfo.json").Skip(Number + 1).First().Replace("        \"ShortName\": \"", "");
                        Temp.Add(Name.Replace("\",", "") + "|" + ID.Replace("\",", ""));
                    }
                    Number++;
                }
                str.Close();
            }
            catch (Exception e)
            {
                Error_Log_Write(e.Message);
            }
            return Temp;
        }
        //SEのIDをすべて取得
        public static List<string> Get_SE_ID(string SE_Name)
        {
            //SEのIDは規則性がないため手動で入力
            List<string> Temp = new List<string>();
            if (SE_Name == "reload")
            {
                Temp.Add("howitzer_load_04.wav|197288924");
                Temp.Add("howitzer_load_01.wav|329813567");
                Temp.Add("howitzer_load_03.wav|379442700");
                Temp.Add("howitzer_load_05.wav|437991333");
            }
            else if (SE_Name == "command")
            {
                Temp.Add("chat_all.wav|680945491");
                Temp.Add("chat_allies.wav|281698299");
                Temp.Add("chat_squad.wav|564473327");
                Temp.Add("quick_commands_attack.wav|521032820");
                Temp.Add("quick_commands_attack_target.wav|553358501");
                Temp.Add("quick_commands_capture_base.wav|569608305");
                Temp.Add("quick_commands_defend_base.wav|326643922");
                Temp.Add("quick_commands_help_me.wav|754223183");
                Temp.Add("quick_commands_negative.wav|34803346");
                Temp.Add("quick_commands_positive.wav|611400702");
                Temp.Add("quick_commands_reloading.wav|1040105232");
            }
            else if (SE_Name == "battle_streamed")
            {
                Temp.Add("adrenalin_off.wav|5395403");
                Temp.Add("adrenalin.wav|145322336");
                Temp.Add("auto_target_off.wav|184826681");
                Temp.Add("auto_target_on.wav|535724311");
                Temp.Add("capture_tick_04.wav|533332128");
                Temp.Add("capture_tick_02.wav|592199082");
                Temp.Add("capture_tick_01.wav|704285192");
                Temp.Add("capture_tick_03.wav|801627646");
                Temp.Add("sirene_01.wav|2052318");
                Temp.Add("capture_end.wav|358568085");
                Temp.Add("shot_no.wav|815477745");
                Temp.Add("shot_no_no.wav|434456653");
                Temp.Add("shot_yes.wav|629250465");
                Temp.Add("shot_yes_yes.wav|982137706");
                Temp.Add("enemy_sighted.wav|244729890");
                Temp.Add("fire_extinguisher_01.wav|836180377");
                Temp.Add("furious_off.wav|236092940");
                Temp.Add("furious.wav|119583868");
                Temp.Add("lamp_01.wav|1033024224");
                Temp.Add("medikit_03.wav|732908901");
                Temp.Add("perk_shooter1.wav|394994028");
                Temp.Add("perk_shooter_max.wav|435001957");
                Temp.Add("quick_commands_close.wav|55835868");
                Temp.Add("quick_commands_open.wav|927236219");
                Temp.Add("repair_08.wav|446072416");
                Temp.Add("repair_06.wav|697244477");
                Temp.Add("restorer.wav|750716839");
                Temp.Add("shell_choose_01.wav|990572193");
                Temp.Add("shell_close_01.wav|971410303");
                Temp.Add("shell_open_01.wav|625795726");
                Temp.Add("sight_convergence_01.wav|361548454");
                Temp.Add("sight_convergence_03.wav|815083652");
                Temp.Add("snipermode_off.wav|990674039");
                Temp.Add("snipermode_on.wav|248236283");
                Temp.Add("stripe.wav|272832027");
                Temp.Add("timer.wav|382538041");
                Temp.Add("zoom_in.wav|66011054");
                Temp.Add("zoom_out.wav|890232967");
            }
            return Temp;
        }
        //バックアップフォルダを更新
        public static void Backup_Update(string Time)
        {
            string[] Dirs = Directory.GetDirectories(Directory.GetCurrentDirectory() + "/Backup", "*", SearchOption.TopDirectoryOnly);
            foreach (string Dir in Dirs)
            {
                string Dir_Name_Only = Path.GetFileName(Dir);
                if (Dir_Name_Only == Time || Dir_Name_Only == "Main")
                    continue;
                try
                {
                    Directory.Delete(Dir, true);
                }
                catch (Exception e1)
                {
                    Sub_Code.Error_Log_Write(e1.Message);
                }
            }
        }
        //音声ファイルの言語を変更
        //例:/Data/WwiseSound/ja/voiceover_crew.bnk -> /Data/WwiseSound/en/voiceover_crew.bnk
        //bnk内のIDが異なるためそのままコピーすることはできません。
        public static void Voice_Change_Language(string From_BNK_File, string To_BNK_File, string Set_Language)
        {
            if (!File.Exists(Voice_Set.Special_Path + "/Wwise/SoundbanksInfo.json") || !File.Exists(From_BNK_File))
                return;
            try
            {
                Wwise_Class.Wwise_File_Extract_V2 Wwise_Bnk = new Wwise_Class.Wwise_File_Extract_V2(From_BNK_File);
                if (Directory.Exists(Voice_Set.Special_Path + "/Voice_Temp"))
                    Directory.Delete(Voice_Set.Special_Path + "/Voice_Temp", true);
                Wwise_Bnk.Wwise_Extract_To_WEM_Directory(Voice_Set.Special_Path + "/Voice_Temp", 1);
                string Get_Language_ID = Wwise_Bnk.Wwise_Get_Name(0);
                Wwise_Bnk.Bank_Clear();
                StreamReader str = new StreamReader(Voice_Set.Special_Path + "/Wwise/SoundbanksInfo.json");
                string Read_Line = "";
                int Number = 0;
                string Get_Language_Now = "";
                while ((Read_Line = str.ReadLine()) != null)
                {
                    if (Read_Line == "        \"Id\": \"" + Get_Language_ID + "\"")
                        Get_Language_Now = File.ReadLines(Voice_Set.Special_Path + "/Wwise/SoundbanksInfo.json").Skip(Number + 1).First().Replace("        \"Language\": \"", "").Replace("\"", "");
                    Number++;
                }
                str.Close();
                if (Get_Language_Now != "")
                {
                    List<string> Replace_Name_Voice = Get_Voices_ID(Get_Language_Now);
                    foreach (string Replace_Name_Now in Replace_Name_Voice)
                    {
                        string Name_Only = Replace_Name_Now.Substring(0, Replace_Name_Now.IndexOf(':'));
                        string ID_Only = Replace_Name_Now.Substring(Replace_Name_Now.IndexOf(':') + 1);
                        File_Move(Voice_Set.Special_Path + "/Voice_Temp/" + ID_Only + ".wem", Voice_Set.Special_Path + "/Voice_Temp/" + Name_Only.Replace(".wav", ".wem"), true);
                    }
                    List<string> Get_Set_Language_ID = Get_Voices_ID(Set_Language);
                    Wwise_Class.Wwise_File_Extract_V2 Wwise_Bnk_02 = new Wwise_Class.Wwise_File_Extract_V2(Voice_Set.Special_Path + "/Voice_Temp/voiceover_crew.bnk");
                    List<string> New_ID = Wwise_Bnk_02.Wwise_Get_Names();
                    for (int Number_01 = 0; Number_01 < New_ID.Count; Number_01++)
                    {
                        foreach (string ID in Get_Set_Language_ID)
                        {
                            string Name_Only = ID.Substring(0, ID.IndexOf(':')).Replace(".wav", ".wem");
                            string ID_Only = ID.Substring(ID.IndexOf(':') + 1);
                            if (ID_Only == New_ID[Number_01])
                                Wwise_Bnk_02.Bank_Edit_Sound(Number_01, Voice_Set.Special_Path + "/Voice_Temp/" + Name_Only, false);
                        }
                    }
                    Wwise_Bnk_02.Bank_Save(To_BNK_File);
                    Wwise_Bnk_02.Bank_Clear();
                }
                else
                    Error_Log_Write("指定された.bnkファイルは音声ファイルでない可能性があります。");
            }
            catch (Exception e)
            {
                Error_Log_Write(e.Message);
            }
        }
        //ファイル名を変更
        public static void File_Rename_Number(string From_File, string To_File_Name_Only)
        {
            if (!File.Exists(From_File))
                return;
            if (!To_File_Name_Only.Contains("/") && !To_File_Name_Only.Contains("\\"))
                To_File_Name_Only = Path.GetDirectoryName(From_File) + "\\" + To_File_Name_Only;
            int Number = 1;
            while (true)
            {
                if (Number < 10)
                {
                    if (!File_Exists(To_File_Name_Only + "_0" + Number))
                    {
                        File_Move(From_File, To_File_Name_Only + "_0" + Number + Path.GetExtension(From_File), true);
                        return;
                    }
                }
                else
                {
                    if (!File_Exists(To_File_Name_Only + "_" + Number))
                    {
                        File_Move(From_File, To_File_Name_Only + "_" + Number + Path.GetExtension(From_File), true);
                        return;
                    }
                }
                Number++;
            }
        }
        public static string File_Rename_Get_Name(string To_File_Name_Only)
        {
            int Number = 1;
            while (true)
            {
                if (Number < 10)
                {
                    if (!File_Exists(To_File_Name_Only + "_0" + Number))
                        return To_File_Name_Only + "_0" + Number;
                }
                else
                {
                    if (!File_Exists(To_File_Name_Only + "_" + Number))
                        return To_File_Name_Only + "_" + Number;
                }
                Number++;
            }
        }
        public static void Actor_Mixer_Update(string Version)
        {
            if (!Directory.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy") || !Directory.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Events"))
                return;
            Voice_Set.FTPClient.DownloadFile("/WoTB_Voice_Mod/Update/Wwise/Default Work Unit.wwu", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.wwu");
            Voice_Set.FTPClient.DownloadFile("/WoTB_Voice_Mod/Update/Wwise/SoundBanks/Default Work Unit.wwu", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/SoundBanks/Default Work Unit.wwu");
            Voice_Set.FTPClient.DownloadFile("/WoTB_Voice_Mod/Update/Wwise/Events.zip", Voice_Set.Special_Path + "/Wwise/Download_Wwise_Events.dat");
            StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Version.dat");
            stw.Write(Version);
            stw.Close();
            System.IO.Compression.ZipFile.ExtractToDirectory(Voice_Set.Special_Path + "/Wwise/Download_Wwise_Events.dat", Voice_Set.Special_Path + "/Wwise/Download_Wwise_Events");
            Sub_Code.Directory_Copy(Voice_Set.Special_Path + "/Wwise/Download_Wwise_Events", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Events");
            File.Delete(Voice_Set.Special_Path + "/Wwise/Download_Wwise_Events.dat");
            File.Delete(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp");
            Directory.Delete(Voice_Set.Special_Path + "/Wwise/Download_Wwise_Events", true);
        }
        //Wwiseプロジェクトをサーバーからダウンロード
        public static async Task<int> Wwise_Project_Update(TextBlock Message_T, ProgressBar Download_P, TextBlock Download_T, Border Download_Border)
        {
            try
            {
                if (File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Version.dat"))
                {
                    if (Voice_Set.FTPClient.IsConnected)
                    {
                        StreamReader str2 = new StreamReader(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Version.dat");
                        string Version = str2.ReadLine();
                        str2.Close();
                        if (File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Version.dat"))
                        {
                            StreamReader str3 = new StreamReader(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Version.dat");
                            string Actor_Version_Now = str3.ReadLine();
                            str3.Close();
                            if (IsWwise_Blitz_Actor_Update != Actor_Version_Now)
                            {
                                Actor_Mixer_Update(IsWwise_Blitz_Actor_Update);
                            }
                        }
                        else
                            Actor_Mixer_Update(IsWwise_Blitz_Actor_Update);
                        if (IsWwise_Blitz_Update != Version)
                        {
                            MessageBoxResult result = MessageBox.Show("プロジェクトデータのアップデートがあります。アップデートしますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes);
                            if (result == MessageBoxResult.Yes)
                            {
                                if (Directory.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod"))
                                    Directory.Delete(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod", true);
                                File.Delete(Voice_Set.Special_Path + "/Wwise_Project.dat");
                                string Path = Directory.GetCurrentDirectory();
                                long Full_Size = Voice_Set.FTPClient.GetFileSize("/WoTB_Voice_Mod/Update/Wwise/Wwise_Project_01.zip"); ;
                                double SizeMB = (double)(Full_Size / 1024.0 / 1024.0);
                                SizeMB = (Math.Floor(SizeMB * 10)) / 10;
                                Message_T.Text = "サーバーからプロジェクトデータをダウンロードしています...\nダウンロードサイズ:約" + SizeMB + "MB";
                                await Task.Delay(50);
                                Voice_Set.App_Busy = true;
                                Download_P.Visibility = Visibility.Visible;
                                Download_T.Visibility = Visibility.Visible;
                                Download_Border.Visibility = Visibility.Visible;
                                try
                                {
                                    Task task = Task.Run(() =>
                                    {
                                        Voice_Set.FTPClient.DownloadFile("/WoTB_Voice_Mod/Update/Wwise/Wwise_Project_01.zip", Voice_Set.Special_Path + "/Wwise_Project.dat");
                                    });
                                    while (true)
                                    {
                                        long File_Size_Now = 0;
                                        if (File.Exists(Voice_Set.Special_Path + "/Wwise_Project.dat"))
                                        {
                                            FileInfo fi = new FileInfo(Voice_Set.Special_Path + "/Wwise_Project.dat");
                                            File_Size_Now = fi.Length;
                                        }
                                        double Download_Percent = (double)File_Size_Now / Full_Size * 100;
                                        int Percent_INT = (int)Math.Round(Download_Percent, MidpointRounding.AwayFromZero);
                                        Download_P.Value = Percent_INT;
                                        Download_T.Text = "進捗:" + Percent_INT + "%";
                                        if (File_Size_Now >= Full_Size)
                                        {
                                            Download_P.Value = 0;
                                            Download_T.Text = "進捗:0%";
                                            break;
                                        }
                                        await Task.Delay(100);
                                    }
                                }
                                catch (Exception e1)
                                {
                                    Sub_Code.Error_Log_Write(e1.Message);
                                    Download_P.Visibility = Visibility.Hidden;
                                    Download_T.Visibility = Visibility.Hidden;
                                    Download_Border.Visibility = Visibility.Hidden;
                                    Voice_Set.App_Busy = false;
                                    Message_T.Text = "";
                                    return 1;
                                }
                                System.IO.Compression.ZipFile.ExtractToDirectory(Voice_Set.Special_Path + "/Wwise_Project.dat", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod");
                                File.Delete(Voice_Set.Special_Path + "/Wwise_Project.dat");
                                File.WriteAllText(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Version.dat", IsWwise_Blitz_Update);
                                Download_P.Visibility = Visibility.Hidden;
                                Download_T.Visibility = Visibility.Hidden;
                                Download_Border.Visibility = Visibility.Hidden;
                                Voice_Set.App_Busy = false;
                                Message_T.Text = "";
                                return 0;
                            }
                            else
                            {
                                Message_T.Text = "";
                                return 2;
                            }
                        }
                    }
                }
                else
                {
                    MessageBoxResult result = MessageBox.Show("サーバーからプロジェクトデータをダウンロードしますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes);
                    if (result == MessageBoxResult.Yes)
                    {
                        if (Directory.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod"))
                        {
                            Directory.Delete(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod", true);
                        }
                        File.Delete(Voice_Set.Special_Path + "/Wwise_Project.dat");
                        string Line = Voice_Set.FTPClient.GetFileLine("/WoTB_Voice_Mod/Update/Wwise/Version_01.txt");
                        long Full_Size = Voice_Set.FTPClient.GetFileSize("/WoTB_Voice_Mod/Update/Wwise/Wwise_Project_01.zip");
                        double SizeMB = (double)(Full_Size / 1024.0 / 1024.0);
                        SizeMB = (Math.Floor(SizeMB * 10)) / 10;
                        Message_T.Text = "サーバーからプロジェクトデータをダウンロードしています...\nダウンロードサイズ:約" + SizeMB + "MB";
                        await Task.Delay(50);
                        Voice_Set.App_Busy = true;
                        Download_P.Visibility = Visibility.Visible;
                        Download_T.Visibility = Visibility.Visible;
                        Download_Border.Visibility = Visibility.Visible;
                        try
                        {
                            Task task = Task.Run(() =>
                            {
                                Voice_Set.FTPClient.DownloadFile("/WoTB_Voice_Mod/Update/Wwise/Wwise_Project_01.zip", Voice_Set.Special_Path + "/Wwise_Project.dat");
                            });
                            while (true)
                            {
                                long File_Size_Now = 0;
                                if (File.Exists(Voice_Set.Special_Path + "/Wwise_Project.dat"))
                                {
                                    FileInfo fi = new FileInfo(Voice_Set.Special_Path + "/Wwise_Project.dat");
                                    File_Size_Now = fi.Length;
                                }
                                double Download_Percent = (double)File_Size_Now / Full_Size * 100;
                                int Percent_INT = (int)Math.Round(Download_Percent, MidpointRounding.AwayFromZero);
                                Download_P.Value = Percent_INT;
                                Download_T.Text = "進捗:" + Percent_INT + "%";
                                if (File_Size_Now >= Full_Size)
                                {
                                    Download_P.Value = 0;
                                    Download_T.Text = "進捗:0%";
                                    break;
                                }
                                await Task.Delay(100);
                            }
                        }
                        catch (Exception e1)
                        {
                            Sub_Code.Error_Log_Write(e1.Message);
                            Download_P.Visibility = Visibility.Hidden;
                            Download_T.Visibility = Visibility.Hidden;
                            Download_Border.Visibility = Visibility.Hidden;
                            Voice_Set.App_Busy = false;
                            return 1;
                        }
                        Message_T.Text = "ファイルを展開しています...";
                        await Task.Delay(50);
                        System.IO.Compression.ZipFile.ExtractToDirectory(Voice_Set.Special_Path + "/Wwise_Project.dat", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod");
                        File.Delete(Voice_Set.Special_Path + "/Wwise_Project.dat");
                        File.WriteAllText(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Version.dat", Line);
                        Download_P.Visibility = Visibility.Hidden;
                        Download_T.Visibility = Visibility.Hidden;
                        Download_Border.Visibility = Visibility.Hidden;
                        Voice_Set.App_Busy = false;
                        return 0;
                    }
                    else
                    {
                        return 4;
                    }
                }
            }
            catch (Exception e1)
            {
                Sub_Code.Error_Log_Write(e1.Message);
                Download_P.Visibility = Visibility.Hidden;
                Download_T.Visibility = Visibility.Hidden;
                Download_Border.Visibility = Visibility.Hidden;
                Voice_Set.App_Busy = false;
                return 5;
            }
            return 0;
        }
        public static async Task<int> Wwise_WoT_Project_Update(TextBlock Message_T, ProgressBar Download_P, TextBlock Download_T, Border Download_Border)
        {
            try
            {
                if (File.Exists(Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod/Version.dat"))
                {
                    if (Voice_Set.FTPClient.IsConnected)
                    {
                        StreamReader str2 = new StreamReader(Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod/Version.dat");
                        string Version = str2.ReadLine();
                        str2.Close();
                        if (Version != IsWwise_WoT_Update)
                        {
                            MessageBoxResult result = MessageBox.Show("プロジェクトデータのアップデートがあります。アップデートしますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes);
                            if (result == MessageBoxResult.Yes)
                            {
                                if (Directory.Exists(Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod"))
                                    Directory.Delete(Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod", true);
                                File.Delete(Voice_Set.Special_Path + "/WoT_Sound_Mod.dat");
                                string Path = Directory.GetCurrentDirectory();
                                long Full_Size = Voice_Set.FTPClient.GetFileSize("/WoTB_Voice_Mod/Update/Wwise/WoT_Sound_Mod.zip");
                                double SizeMB = (double)(Full_Size / 1024.0 / 1024.0);
                                SizeMB = (Math.Floor(SizeMB * 10)) / 10;
                                Message_T.Text = "サーバーからプロジェクトデータをダウンロードしています...\nダウンロードサイズ:約" + SizeMB + "MB";
                                await Task.Delay(50);
                                Voice_Set.App_Busy = true;
                                Download_P.Visibility = Visibility.Visible;
                                Download_T.Visibility = Visibility.Visible;
                                Download_Border.Visibility = Visibility.Visible;
                                try
                                {
                                    Task task = Task.Run(() =>
                                    {
                                        Voice_Set.FTPClient.DownloadFile("/WoTB_Voice_Mod/Update/Wwise/WoT_Sound_Mod.zip", Voice_Set.Special_Path + "/WoT_Sound_Mod.dat");
                                    });
                                    while (true)
                                    {
                                        long File_Size_Now = 0;
                                        if (File.Exists(Voice_Set.Special_Path + "/WoT_Sound_Mod.dat"))
                                        {
                                            FileInfo fi = new FileInfo(Voice_Set.Special_Path + "/WoT_Sound_Mod.dat");
                                            File_Size_Now = fi.Length;
                                        }
                                        double Download_Percent = (double)File_Size_Now / Full_Size * 100;
                                        int Percent_INT = (int)Math.Round(Download_Percent, MidpointRounding.AwayFromZero);
                                        Download_P.Value = Percent_INT;
                                        Download_T.Text = "進捗:" + Percent_INT + "%";
                                        if (File_Size_Now >= Full_Size)
                                        {
                                            Download_P.Value = 0;
                                            Download_T.Text = "進捗:0%";
                                            break;
                                        }
                                        await Task.Delay(100);
                                    }
                                }
                                catch (Exception e1)
                                {
                                    Sub_Code.Error_Log_Write(e1.Message);
                                    Download_P.Visibility = Visibility.Hidden;
                                    Download_T.Visibility = Visibility.Hidden;
                                    Download_Border.Visibility = Visibility.Hidden;
                                    Voice_Set.App_Busy = false;
                                    return 1;
                                }
                                System.IO.Compression.ZipFile.ExtractToDirectory(Voice_Set.Special_Path + "/WoT_Sound_Mod.dat", Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod");
                                File.Delete(Voice_Set.Special_Path + "/WoT_Sound_Mod.dat");
                                File.WriteAllText(Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod/Version.dat", IsWwise_WoT_Update);
                                Download_P.Visibility = Visibility.Hidden;
                                Download_T.Visibility = Visibility.Hidden;
                                Download_Border.Visibility = Visibility.Hidden;
                                Voice_Set.App_Busy = false;
                                return 0;
                            }
                            else
                                return 2;
                        }
                    }
                }
                else
                {
                    MessageBoxResult result = MessageBox.Show("サーバーからプロジェクトデータをダウンロードしますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes);
                    if (result == MessageBoxResult.Yes)
                    {
                        if (Directory.Exists(Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod"))
                            Directory.Delete(Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod", true);
                        File.Delete(Voice_Set.Special_Path + "/WoT_Sound_Mod.dat");
                        string Line = Voice_Set.FTPClient.GetFileLine("/WoTB_Voice_Mod/Update/Wwise/Version_02.txt");
                        long Full_Size = Voice_Set.FTPClient.GetFileSize("/WoTB_Voice_Mod/Update/Wwise/WoT_Sound_Mod.zip");
                        double SizeMB = (double)(Full_Size / 1024.0 / 1024.0);
                        SizeMB = (Math.Floor(SizeMB * 10)) / 10;
                        Message_T.Text = "サーバーからプロジェクトデータをダウンロードしています...\nダウンロードサイズ:約" + SizeMB + "MB";
                        await Task.Delay(50);
                        Voice_Set.App_Busy = true;
                        Download_P.Visibility = Visibility.Visible;
                        Download_T.Visibility = Visibility.Visible;
                        Download_Border.Visibility = Visibility.Visible;
                        try
                        {
                            Task task = Task.Run(() =>
                            {
                                Voice_Set.FTPClient.DownloadFile("/WoTB_Voice_Mod/Update/Wwise/WoT_Sound_Mod.zip", Voice_Set.Special_Path + "/WoT_Sound_Mod.dat");
                            });
                            while (true)
                            {
                                long File_Size_Now = 0;
                                if (File.Exists(Voice_Set.Special_Path + "/WoT_Sound_Mod.dat"))
                                {
                                    FileInfo fi = new FileInfo(Voice_Set.Special_Path + "/WoT_Sound_Mod.dat");
                                    File_Size_Now = fi.Length;
                                }
                                double Download_Percent = (double)File_Size_Now / Full_Size * 100;
                                int Percent_INT = (int)Math.Round(Download_Percent, MidpointRounding.AwayFromZero);
                                Download_P.Value = Percent_INT;
                                Download_T.Text = "進捗:" + Percent_INT + "%";
                                if (File_Size_Now >= Full_Size)
                                {
                                    Download_P.Value = 0;
                                    Download_T.Text = "進捗:0%";
                                    break;
                                }
                                await Task.Delay(100);
                            }
                        }
                        catch (Exception e1)
                        {
                            Sub_Code.Error_Log_Write(e1.Message);
                            Download_P.Visibility = Visibility.Hidden;
                            Download_T.Visibility = Visibility.Hidden;
                            Download_Border.Visibility = Visibility.Hidden;
                            Voice_Set.App_Busy = false;
                            return 1;
                        }
                        Message_T.Text = "ファイルを展開しています...";
                        await Task.Delay(50);
                        System.IO.Compression.ZipFile.ExtractToDirectory(Voice_Set.Special_Path + "/WoT_Sound_Mod.dat", Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod");
                        File.Delete(Voice_Set.Special_Path + "/WoT_Sound_Mod.dat");
                        File.WriteAllText(Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod/Version.dat", Line);
                        Download_P.Visibility = Visibility.Hidden;
                        Download_T.Visibility = Visibility.Hidden;
                        Download_Border.Visibility = Visibility.Hidden;
                        Voice_Set.App_Busy = false;
                        return 0;
                    }
                    else
                        return 4;
                }
            }
            catch (Exception e1)
            {
                Sub_Code.Error_Log_Write(e1.Message);
                Download_P.Visibility = Visibility.Hidden;
                Download_T.Visibility = Visibility.Hidden;
                Download_Border.Visibility = Visibility.Hidden;
                Voice_Set.App_Busy = false;
                return 5;
            }
            return 0;
        }
        //指定した文字の後に数字があるか(含まれていたらtrue)
        public static bool IsIncludeInt_From_String(string All_String, string Where)
        {
            if (!All_String.Contains(Where))
                return false;
            for (int Number = 0; Number < 10; Number++)
                if (All_String.Contains(Where + Number))
                    return true;
            return false;
        }
        public static bool IsIncludeInt_From_String_V2(string All_String, string LastString)
        {
            if (!All_String.Contains(LastString))
                return false;
            if (All_String.Length < All_String.LastIndexOf(LastString) + 2)
                return false;
            All_String = All_String.Substring(All_String.LastIndexOf(LastString) + 1, 1);
            for (int Number = 0; Number < 10; Number++)
                if (All_String == Number.ToString())
                    return true;
            return false;
        }
        //指定したフォルダにアクセスできるか
        public static bool CanDirectoryAccess(string Dir_Path)
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
        //Wwiserを用いて.bnkファイルを解析する
        public static void BNK_Parse_To_XML(string From_BNK_File, string To_XML_File)
        {
            StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Wwise_Parse/BNK_Parse_Start.bat");
            stw.WriteLine("chcp 65001");
            stw.Write("\"" + Voice_Set.Special_Path + "/Wwise_Parse/Python/python.exe\" \"" + Voice_Set.Special_Path + "/Wwise_Parse/wwiser.pyz\" -iv \"" + From_BNK_File + "\" -dn \"" +
                To_XML_File.Replace(".xml", "") + "\"");
            stw.Close();
            ProcessStartInfo processStartInfo1 = new ProcessStartInfo
            {
                FileName = Voice_Set.Special_Path + "/Wwise_Parse/BNK_Parse_Start.bat",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process p = Process.Start(processStartInfo1);
            p.WaitForExit();
            File.Delete(Voice_Set.Special_Path + "/Wwise_Parse/BNK_Parse_Start.bat");
        }
        /// <summary>
        /// BitmapSourceを指定したサイズにカット(指定サイズと範囲外を抽出)
        /// 引数:元のBitmapSource, 横幅, 縦幅, out 指定したサイズのBitmapSource, out サイズ外のBitmapSource
        /// </summary>
        public static void Resize_From_BitmapImage(BitmapSource From_Image, int Width, int Height, out BitmapSource Inside_Image, out BitmapSource Outside_Image)
        {
            var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(From_Image));
            using (var ms = new System.IO.MemoryStream())
            {
                encoder.Save(ms);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                using (System.Drawing.Image image = System.Drawing.Image.FromStream(ms))
                {
                    using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(Width, Height))
                    {
                        using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap))
                        {
                            System.Drawing.SolidBrush solidBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Transparent);
                            graphics.FillRectangle(solidBrush, new System.Drawing.RectangleF(0, 0, Width, Height));
                            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            graphics.DrawImage(image, 0, 0, image.Width, image.Height);
                            Inside_Image = Bitmap_To_BitmapImage(bitmap);
                        }
                    }
                    using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap((int)(From_Image.Width - Width), (int)From_Image.Height))
                    {
                        using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap))
                        {
                            System.Drawing.SolidBrush solidBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Transparent);
                            graphics.FillRectangle(solidBrush, new System.Drawing.RectangleF(0, 0, Width, Height));
                            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            graphics.DrawImage(image, -Width, 0, image.Width, image.Height);
                            Outside_Image = Bitmap_To_BitmapImage(bitmap);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// BitmapSourceを指定したサイズにカット(指定サイズのみ抽出)
        /// 引数:元のBitmapSource, 横幅, 縦幅,開始位置(左右のみ) out 指定したサイズのBitmapSource
        /// </summary>
        public static void Resize_From_BitmapImage(BitmapSource From_Image, int Width, int Height, int Left, out BitmapSource Inside_Image)
        {
            var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(From_Image));
            using (var ms = new System.IO.MemoryStream())
            {
                encoder.Save(ms);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                using (System.Drawing.Image image = System.Drawing.Image.FromStream(ms))
                {
                    using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(Width, Height))
                    {
                        using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap))
                        {
                            System.Drawing.SolidBrush solidBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Transparent);
                            graphics.FillRectangle(solidBrush, new System.Drawing.RectangleF(0, 0, Width, Height));
                            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            graphics.DrawImage(image, -Left, 0, image.Width, image.Height);
                            Inside_Image = Bitmap_To_BitmapImage(bitmap);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// BitmapSourceを指定したサイズにカットしてファイルに保存
        /// 引数:元のBitmapSource, 横幅, 縦幅, 指定したサイズの画像の保存先, 指定したサイズ外の画像の保存先
        /// </summary>
        public static void Resize_From_BitmapImage(BitmapImage From_Image, int Width, int Height, string To_Inside_File, string To_Outside_File)
        {
            var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(From_Image));
            using (var ms = new System.IO.MemoryStream())
            {
                encoder.Save(ms);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                using (System.Drawing.Image image = System.Drawing.Image.FromStream(ms))
                {
                    using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(Width, Height))
                    {
                        using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap))
                        {
                            System.Drawing.SolidBrush solidBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Transparent);
                            graphics.FillRectangle(solidBrush, new System.Drawing.RectangleF(0, 0, Width, Height));
                            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            graphics.DrawImage(image, 0, 0, image.Width, image.Height);
                            bitmap.Save(To_Inside_File, System.Drawing.Imaging.ImageFormat.Png);
                        }
                    }
                    using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap((int)(From_Image.Width - Width), (int)From_Image.Height))
                    {
                        using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap))
                        {
                            System.Drawing.SolidBrush solidBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Transparent);
                            graphics.FillRectangle(solidBrush, new System.Drawing.RectangleF(0, 0, Width, Height));
                            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            graphics.DrawImage(image, -Width, 0, image.Width, image.Height);
                            bitmap.Save(To_Outside_File, System.Drawing.Imaging.ImageFormat.Png);
                        }
                    }
                }
            }
        }
        public static bool IsSafeFileName(string File_Name)
        {
            if (string.IsNullOrEmpty(File_Name))
                return false;
            char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
            if (File_Name.IndexOfAny(invalidChars) >= 0)
                return false;
            if (System.Text.RegularExpressions.Regex.IsMatch(File_Name
                                           , @"(^|\\|/)(CON|PRN|AUX|NUL|CLOCK\$|COM[0-9]|LPT[0-9])(\.|\\|/|$)"
                                           , System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                return false;
            return true;
        }
        public static string Generate_Random_String(int Min_Length, int Max_Length)
        {
            return Generate_Random_String("C:\\ASGYAifei", Min_Length, Max_Length);
        }
        public static string Generate_Random_String(string File_Path, int Min_Length, int Max_Length)
        {
            int Length = r.Next(Min_Length, Max_Length + 1);
            StringBuilder sb = new StringBuilder(Length);
            for (int i = 0; i < Length; i++)
            {
                int pos = r.Next(Random_String.Length);
                char c = Random_String[pos];
                sb.Append(c);
            }
            if (File_Exists(File_Path + sb))
                return Generate_Random_String(File_Path, Min_Length, Max_Length);
            return sb.ToString();
        }
        ///<summary>
        ///Bass Audio Libraryを使用して波形を作成
        ///</summary>
        public static BitmapImage BassRenderWaveForm(string Audio_File)
        {
            Un4seen.Bass.Misc.WaveForm WF_Color = new Un4seen.Bass.Misc.WaveForm();
            BitmapImage Temp_Image = null;
            WF_Color = new Un4seen.Bass.Misc.WaveForm(Audio_File);
            WF_Color.CallbackFrequency = 0;
            WF_Color.ColorBackground = System.Drawing.Color.Transparent;
            WF_Color.ColorLeft = System.Drawing.Color.Aqua;
            WF_Color.ColorMiddleLeft = System.Drawing.Color.DarkBlue;
            WF_Color.ColorMiddleRight = System.Drawing.Color.DarkBlue;
            WF_Color.ColorRight = System.Drawing.Color.Aqua;
            WF_Color.ColorLeft2 = System.Drawing.Color.Transparent;
            WF_Color.ColorRight2 = System.Drawing.Color.Transparent;
            WF_Color.ColorLeftEnvelope = System.Drawing.Color.Transparent;
            WF_Color.ColorRightEnvelope = System.Drawing.Color.Transparent;
            WF_Color.RenderStart(true, Un4seen.Bass.BASSFlag.BASS_DEFAULT);
            while (!WF_Color.IsRendered)
                System.Threading.Thread.Sleep(50);
            Temp_Image = Sub_Code.Bitmap_To_BitmapImage(WF_Color.CreateBitmap(1920, 300, -1, -1, false));
            WF_Color.RenderStop();
            return Temp_Image;
        }
        public static System.Drawing.Bitmap BassRenderWaveForm_To_Bitmap(string Audio_File)
        {
            Un4seen.Bass.Misc.WaveForm WF_Color = new Un4seen.Bass.Misc.WaveForm();
            WF_Color = new Un4seen.Bass.Misc.WaveForm(Audio_File);
            WF_Color.CallbackFrequency = 0;
            WF_Color.ColorBackground = System.Drawing.Color.Transparent;
            WF_Color.ColorLeft = System.Drawing.Color.Aqua;
            WF_Color.ColorMiddleLeft = System.Drawing.Color.DarkBlue;
            WF_Color.ColorMiddleRight = System.Drawing.Color.DarkBlue;
            WF_Color.ColorRight = System.Drawing.Color.Aqua;
            WF_Color.ColorLeft2 = System.Drawing.Color.Transparent;
            WF_Color.ColorRight2 = System.Drawing.Color.Transparent;
            WF_Color.ColorLeftEnvelope = System.Drawing.Color.Transparent;
            WF_Color.ColorRightEnvelope = System.Drawing.Color.Transparent;
            WF_Color.RenderStart(true, Un4seen.Bass.BASSFlag.BASS_DEFAULT);
            while (!WF_Color.IsRendered)
                System.Threading.Thread.Sleep(50);
            System.Drawing.Bitmap Temp = WF_Color.CreateBitmap(1920, 300, -1, -1, false);
            WF_Color.RenderStop();
            return Temp;
        }
        public static double Get_WAV_Gain(string WAV_File)
        {
            int Number = r.Next(0, 10000);
            StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Other/WAV_Get_Gain_" + Number + ".bat");
            stw.WriteLine("chcp 65001");
            stw.Write("\"" + Voice_Set.Special_Path + "/Other/WaveGain.exe\" -f Gain_Log_" + Number + ".txt \"" + WAV_File + "\"");
            stw.Close();
            ProcessStartInfo processStartInfo1 = new ProcessStartInfo
            {
                FileName = Voice_Set.Special_Path + "/Other/WAV_Get_Gain_" + Number + ".bat",
                WorkingDirectory = Voice_Set.Special_Path + "\\Other",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process p = Process.Start(processStartInfo1);
            p.WaitForExit();
            File.Delete(Voice_Set.Special_Path + "/Other/WAV_Get_Gain_" + Number + ".bat");
            StreamReader str = new StreamReader(Voice_Set.Special_Path + "\\Other\\Gain_Log_" + Number + ".txt");
            string line;
            double Gain = 0;
            while ((line = str.ReadLine()) != null)
            {
                if (line.Contains("-----------------"))
                {
                    string Line_Temp = str.ReadLine().Trim();
                    string Gain_Temp = Line_Temp.Substring(0, Line_Temp.IndexOf("dB"));
                    Gain += double.Parse(Gain_Temp) + 0.01;
                    break;
                }
            }
            str.Close();
            File.Delete(Voice_Set.Special_Path + "\\Other\\Gain_Log_" + Number + ".txt");
            return Gain;
        }
        public static void Set_WAV_Gain(string WAV_File, double Gain)
        {
            if (Gain <= -20)
                Gain = -19.9;
            else if (Gain >= 12)
                Gain = 11.9;
            int Number = r.Next(0, 10000);
            StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Other/WAV_Set_Gain_" + Number + ".bat");
            stw.WriteLine("chcp 65001");
            stw.Write("\"" + Voice_Set.Special_Path + "/Other/WaveGain.exe\" -r -y -n -g " + Gain + " \"" + WAV_File + "\"");
            stw.Close();
            ProcessStartInfo processStartInfo1 = new ProcessStartInfo
            {
                FileName = Voice_Set.Special_Path + "/Other/WAV_Set_Gain_" + Number + ".bat",
                WorkingDirectory = Voice_Set.Special_Path + "\\Other",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process p = Process.Start(processStartInfo1);
            p.WaitForExit();
            File.Delete(Voice_Set.Special_Path + "/Other/WAV_Set_Gain_" + Number + ".bat");
        }
    }
    //ウィンドウにフォーカスがないとき、アイコンを光らせる
    public class Flash
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);
        public const uint FLASHW_STOP = 0;
        public const uint FLASHW_CAPTION = 1;
        public const uint FLASHW_TRAY = 2;
        public const uint FLASHW_ALL = 3;
        public const uint FLASHW_TIMER = 4;
        public const uint FLASHW_TIMERNOFG = 12;
        public static Window Handle = null;
        public static bool IsFlashing = false;
        private struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }
        //アイコンを光らせる
        public static void Flash_Start(UInt32 count = UInt32.MaxValue)
        {
            if (Win2000OrLater && Handle != null && !IsFlashing)
            {
                if (Handle.IsActive) return;
                IsFlashing = true;
                WindowInteropHelper h = new WindowInteropHelper(Handle);
                FLASHWINFO info = new FLASHWINFO
                {
                    hwnd = h.Handle,
                    dwFlags = FLASHW_ALL | FLASHW_TIMER,
                    uCount = count,
                    dwTimeout = 0
                };
                info.cbSize = Convert.ToUInt32(Marshal.SizeOf(info));
                FlashWindowEx(ref info);
                Flash_Loop();
            }
        }
        //アイコンを戻す
        public static void Flash_Stop()
        {
            if (Win2000OrLater && Handle != null && IsFlashing)
            {
                WindowInteropHelper h = new WindowInteropHelper(Handle);
                FLASHWINFO info = new FLASHWINFO
                {
                    hwnd = h.Handle,
                    dwFlags = FLASHW_STOP,
                    uCount = UInt32.MaxValue,
                    dwTimeout = 0
                };
                info.cbSize = Convert.ToUInt32(Marshal.SizeOf(info));
                FlashWindowEx(ref info);
                IsFlashing = false;
            }
        }
        //ウィンドウにフォーカスが与えられたらアイコンを戻す
        static async void Flash_Loop()
        {
            while (!Handle.IsActive && IsFlashing)
                await Task.Delay(500);
            Flash_Stop();
        }
        //Windows XP以上か判定(まぁ.NET FrameWork4.6はWindows7以上なので必ずtrueを返しますが...)
        private static bool Win2000OrLater
        {
            get { return System.Environment.OSVersion.Version.Major >= 5; }
        }
    }
}