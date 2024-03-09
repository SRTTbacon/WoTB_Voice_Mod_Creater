using Microsoft.Win32;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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
using WoTB_Voice_Mod_Creater.Class;

namespace WoTB_Voice_Mod_Creater
{
    public enum Encode_Mode
    {
        MP3,
        WAV
    }
    public class Sub_Code
    {
        public static readonly Dictionary<int, int> LPF_Values = new Dictionary<int, int>();
        public static readonly Dictionary<int, int> HPF_Values = new Dictionary<int, int>();
        public static readonly Dictionary<int, int> Pitch_Values = new Dictionary<int, int>();
        public static List<uint> ShortIDs = new List<uint>();
        public const string Random_String = "0123456789abcdefghijklmnopqrstuvwxyz";
        public const double Window_Feed_Time = 0.04;
        static List<string> IsAutoListAdd = new List<string>();
        public static Random r = new Random();
        static string IsLanguage = "";
        public static bool IsWindowBarShow = false;
        public static bool IsForcusWindow = false;
        public static bool IsForceMusicStop = false;
        public static readonly string[] Default_Name = { "ally_killed_by_player", "ammo_bay_damaged", "armor_not_pierced_by_player", "armor_pierced_by_player", "armor_pierced_crit_by_player", "armor_ricochet_by_player",
        "commander_killed","driver_killed","enemy_fire_started_by_player","enemy_killed_by_player","engine_damaged","engine_destroyed","engine_functional","fire_started","fire_stopped",
        "fuel_tank_damaged","gun_damaged","gun_destroyed","gun_functional","gunner_killed","loader_killed","radio_damaged","radioman_killed","start_battle","surveying_devices_damaged",
        "surveying_devices_destroyed","surveying_devices_functional","track_damaged","track_destroyed","track_functional","turret_rotator_damaged","turret_rotator_destroyed","turret_rotator_functional","vehicle_destroyed"};
        public static BitmapFrame Check_01;
        public static BitmapFrame Check_02;
        public static BitmapFrame Check_03;
        public static BitmapFrame Check_04;
        static bool IsCreatingProject = false;
        static bool IsVolumeSet = false;
        static bool IsDVPLEncode = false;
        static bool IsModChange = false;
        static bool IsDefaultVoiceMode = false;
        static bool IsOnly_Wwise_Project = false;

        [DllImport("shell32.dll", SetLastError = true)]
        public static extern int SHOpenFolderAndSelectItems(IntPtr pidlFolder, uint cidl, [In, MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl, uint dwFlags);

        [DllImport("shell32.dll", SetLastError = true)]
        public static extern void SHParseDisplayName([MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr bindingContext, [Out] out IntPtr pidl, uint sfgaoIn, [Out] out uint psfgaoOut);

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
        public static bool Only_Wwise_Project
        {
            get { return IsOnly_Wwise_Project; }
            set { IsOnly_Wwise_Project = value; }
        }
        public static void Init()
        {
            LPF_Values.Add(12000, 1500);
            LPF_Values.Add(10500, 2500);
            LPF_Values.Add(8500, 2000);
            LPF_Values.Add(6500, 3200);
            LPF_Values.Add(3300, 2000);
            LPF_Values.Add(1300, 700);
            LPF_Values.Add(600, 400);
            LPF_Values.Add(200, 0);
            HPF_Values.Add(0, 45);
            HPF_Values.Add(45, 25);
            HPF_Values.Add(70, 45);
            HPF_Values.Add(115, 25);
            HPF_Values.Add(140, 60);
            HPF_Values.Add(200, 130);
            HPF_Values.Add(330, 70);
            HPF_Values.Add(400, 150);
            HPF_Values.Add(550, 350);
            HPF_Values.Add(900, 400);
            HPF_Values.Add(1300, 475);
            HPF_Values.Add(1775, 1225);
            HPF_Values.Add(3000, 1500);
            HPF_Values.Add(4500, 1500);
            HPF_Values.Add(6500, 0);
            Pitch_Values.Add(1200, 100);
            Pitch_Values.Add(1100, 90);
            Pitch_Values.Add(1000, 80);
            Pitch_Values.Add(900, 70);
            Pitch_Values.Add(800, 60);
            Pitch_Values.Add(700, 50);
            Pitch_Values.Add(575, 40);
            Pitch_Values.Add(450, 30);
            Pitch_Values.Add(300, 20);
            Pitch_Values.Add(175, 10);
            Pitch_Values.Add(0, 0);
            Pitch_Values.Add(-175, -10);
            Pitch_Values.Add(-400, -20);
            Pitch_Values.Add(-600, -30);
            Pitch_Values.Add(-900, -40);
            Pitch_Values.Add(-1200, -50);
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
            if (!File.Exists(DLL_Path + "/bassenc.dll"))
                DLL_List.Add("bassenc.dll");
            if (!File.Exists(DLL_Path + "/bassflac.dll"))
                DLL_List.Add("bassflac.dll");
            if (!File.Exists(DLL_Path + "/bassmix.dll"))
                DLL_List.Add("bassmix.dll");
            if (!File.Exists(DLL_Path + "/fmod_event64.dll"))
                DLL_List.Add("fmod_event.dll");
            if (!File.Exists(DLL_Path + "/fmodex64.dll"))
                DLL_List.Add("fmodex.dll");
            if (!File.Exists(DLL_Path + "/Wwise_Player_x64.dll"))
                DLL_List.Add("Wwise_Player_x64.dll");
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
        public static void Directory_Copy(string From_Dir, string To_Dir, bool IsIncludeDirMode = true)
        {
            if (!Directory.Exists(From_Dir))
                return;
            try
            {
                if (!Directory.Exists(To_Dir))
                    Directory.CreateDirectory(To_Dir);
                DirectoryInfo dir = new DirectoryInfo(From_Dir);
                if (!dir.Exists)
                    return;
                DirectoryInfo[] dirs = dir.GetDirectories();
                string To = IsIncludeDirMode ? To_Dir + "\\" + Path.GetFileName(From_Dir) : To_Dir;
                Directory.CreateDirectory(To);
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    string tempPath = Path.Combine(To, file.Name);
                    file.CopyTo(tempPath, true);
                }
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(To, subdir.Name);
                    Directory_Copy(subdir.FullName, tempPath, IsIncludeDirMode);
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
        public static List<string> Get_Files_By_Name(string Dir, string Name)
        {
            if (!Directory.Exists(Dir))
                return new List<string>();
            List<string> Return_List = new List<string>();
            string[] Files = Directory.GetFiles(Dir, "*", SearchOption.TopDirectoryOnly);
            foreach (string File_Now in Files)
            {
                string Name_Only = Path.GetFileName(File_Now);
                if (File_Now.Contains(Name))
                    Return_List.Add(File_Now);
            }
            return Return_List;
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
        public static string Set_Voice_Type_Change_Name_By_Index(string Dir, List<Voice_Event_Setting> Lists)
        {
            if (!Directory.Exists(Dir))
                Directory.CreateDirectory(Dir);
            int Romaji_Number = 0;
            foreach (Voice_Event_Setting Index in Lists)
            {
                int File_Number = 1;
                foreach (Voice_Sound_Setting File_Path in Index.Sounds)
                {
                    try
                    {
                        if (File_Number < 10)
                            File.Copy(File_Path.File_Path, Dir + "/" + Voice_Set.Get_Voice_Type_Romaji_Name(Romaji_Number) + "_0" + File_Number + Path.GetExtension(File_Path.File_Path), true);
                        else
                            File.Copy(File_Path.File_Path, Dir + "/" + Voice_Set.Get_Voice_Type_Romaji_Name(Romaji_Number) + "_" + File_Number + Path.GetExtension(File_Path.File_Path), true);
                        File_Number++;
                    }
                    catch (Exception e)
                    {
                        Sub_Code.Error_Log_Write(e.Message);
                        return "サウンドファイルをコピーできませんでした。";
                    }
                }
                Romaji_Number++;
            }
            return "";
        }
        public static string Set_Voice_Type_Change_Name_By_Index(string Dir, WVS_Load WVS_File, List<Voice_Event_Setting> Lists)
        {
            if (!Directory.Exists(Dir))
                Directory.CreateDirectory(Dir);
            for (int Number_01 = 0; Number_01 < Lists.Count; Number_01++)
            {
                for (int Number_02 = 0; Number_02 < Lists[Number_01].Sounds.Count; Number_02++)
                {
                    try
                    {
                        string Name_Temp = Dir + "/" + Voice_Set.Get_Voice_Type_Romaji_Name(Number_01);
                        if (Number_02 < 10)
                        {
                            if (!Lists[Number_01].Sounds[Number_02].File_Path.Contains("\\"))
                                WVS_File.Sound_To_File(Lists[Number_01].Sounds[Number_02], Name_Temp + "_0" + Number_02 + Path.GetExtension(Lists[Number_01].Sounds[Number_02].File_Path));
                            else
                                File.Copy(Lists[Number_01].Sounds[Number_02].File_Path, Name_Temp + "_0" + Number_02 + Path.GetExtension(Lists[Number_01].Sounds[Number_02].File_Path), true);
                        }
                        else
                        {
                            if (!Lists[Number_01].Sounds[Number_02].File_Path.Contains("\\"))
                                WVS_File.Sound_To_File(Lists[Number_01].Sounds[Number_02], Name_Temp + "_" + Number_02 + Path.GetExtension(Lists[Number_01].Sounds[Number_02].File_Path));
                            else
                                File.Copy(Lists[Number_01].Sounds[Number_02].File_Path, Name_Temp + "_" + Number_02 + Path.GetExtension(Lists[Number_01].Sounds[Number_02].File_Path), true);
                        }
                    }
                    catch (Exception e)
                    {
                        Sub_Code.Error_Log_Write(e.Message);
                        return "サウンドファイルをコピーできませんでした。";
                    }
                }
            }
            return "";
        }
        //2つ↑の拡張子もフォルダ名もいらないバージョン
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
                            if (File_Path_01 == "")
                                continue;
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
        //MP3またはWAV形式のファイルの音量を調整
        public static void Volume_Set(string From_Dir, Encode_Mode Mode, double Gain = 10)
        {
            Volume_Set(Directory.GetFiles(From_Dir, "*." + Mode.ToString().ToLower(), SearchOption.TopDirectoryOnly), Mode, Gain);
        }
        public static void Volume_Set(string[] Files, Encode_Mode Mode, double Gain = 10)
        {
            string File_Import = "";
            foreach (string File_Now in Files)
            {
                if (File_Import == "")
                    File_Import = "\"" + File_Now + "\"";
                else
                    File_Import += " \"" + File_Now + "\"";
                //Windowsのコマンドプロンプトは8191文字までしか入力できないため、この時点で8000文字を超えていたら実行
                if (File_Import.Length > 8000)
                {
                    Volume_Set_Start(File_Import, Mode, Gain);
                    File_Import = "";
                }
            }
            if (File_Import != "")
                Volume_Set_Start(File_Import, Mode, Gain);
        }
        public static void Volume_Set_Start(string File_Import, Encode_Mode Mode, double Gain = 10)
        {
            if (Mode == Encode_Mode.MP3)
            {
                Gain = (int)Gain;
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Encode_Mp3/Volume_Set.bat");
                stw.WriteLine("chcp 65001");
                if (File_Import.StartsWith("\""))
                    stw.Write("\"" + Voice_Set.Special_Path + "/Encode_Mp3/mp3gain.exe\" -r -c -p -d " + Gain + " " + File_Import);
                else
                    stw.Write("\"" + Voice_Set.Special_Path + "/Encode_Mp3/mp3gain.exe\" -r -c -p -d " + Gain + " \"" + File_Import + "\"");
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
            else if (Mode == Encode_Mode.WAV)
                Set_WAV_Gain(File_Import, Gain);
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
        public static List<string> Check_WAV_Get_List(string Dir, bool IsRename)
        {
            List<string> Voice_List = new List<string>();
            string[] Files_01 = Directory.GetFiles(Dir, "*.*", SearchOption.TopDirectoryOnly);
            foreach (string File_Now in Files_01)
            {
                if (!Audio_IsWAV(File_Now))
                {
                    if (Path.GetExtension(File_Now) == ".wav" && IsRename)
                    {
                        string To_File = Path.GetDirectoryName(File_Now) + "\\" + Path.GetFileNameWithoutExtension(File_Now) + ".raw";
                        Sub_Code.File_Move(File_Now, To_File, false);
                        Voice_List.Add(To_File);
                    }
                    else
                        Voice_List.Add(File_Now);
                }
                else if (Path.GetExtension(File_Now) != ".wav" && IsRename)
                    Sub_Code.File_Move(File_Now, Path.GetDirectoryName(File_Now) + "\\" + Path.GetFileNameWithoutExtension(File_Now) + ".wav", false);
            }
            return Voice_List;
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
            if (Text.Contains("Error_Log.txt' にアクセスできません。"))
                return;
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
            return img;
        }
        public static System.Drawing.Bitmap BitmapImage_To_Bitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);
                return (System.Drawing.Bitmap)bitmap.Clone();
            }
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
                    using (BinaryReader br = new BinaryReader(fs))
                        if (Encoding.ASCII.GetString(br.ReadBytes(4)) == "RIFF")
                            Temp = true;
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
        public static bool File_Decrypt_To_File(string From_File, string To_File, string Password, bool IsFromFileDelete)
        {
            try
            {
                if (!File.Exists(From_File))
                    return false;
                using (var eifs = new FileStream(From_File, FileMode.Open, FileAccess.Read))
                {
                    using (var eofs = new FileStream(To_File, FileMode.Create, FileAccess.Write))
                        FileEncode.FileEncryptor.Decrypt_To_File(eifs, eofs, Password);
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
        public static StreamReader File_Decrypt_To_Stream(string From_File, string Password, bool bIgnoreError = false)
        {
            try
            {
                StreamReader str = null;
                using (var eifs = new FileStream(From_File, FileMode.Open, FileAccess.Read))
                    str = FileEncode.FileEncryptor.Decrypt_To_Stream(eifs, Password);
                return str;
            }
            catch (Exception e)
            {
                if (!bIgnoreError)
                    Error_Log_Write(e.Message);
                return null;
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
                    StreamReader str = File_Decrypt_To_Stream(Voice_Set.Special_Path + "/Configs/OpenDirectoryPath.dat", "Directory_Save_SRTTbacon");
                    string Read = str.ReadLine();
                    str.Close();
                    if (Directory.Exists(Read))
                        InDir = Read;
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
            if (!Directory.Exists(Dir))
                return;
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
        public static string WEM_To_OGG_WAV(string From_WEM_File, string To_OGG_WAV_File, bool IsFromFileDelete)
        {
            try
            {
                if (!File.Exists(From_WEM_File))
                    return "";
                Wwise_Class.WEM_To_OGG.Create_OGG(From_WEM_File, Voice_Set.Special_Path + "\\Wwise\\Temp.ogg");
                if (File.Exists(Voice_Set.Special_Path + "\\Wwise\\Temp.ogg"))
                {
                    if (IsFromFileDelete)
                        File.Delete(From_WEM_File);
                    string To_Audio_File = To_OGG_WAV_File + ".wav";
                    Un4seen.Bass.Misc.EncoderWAV w = new Un4seen.Bass.Misc.EncoderWAV(0);
                    w.InputFile = Voice_Set.Special_Path + "\\Wwise\\Temp.ogg";
                    w.OutputFile = To_Audio_File;
                    w.WAV_BitsPerSample = 24;
                    w.Start(null, IntPtr.Zero, false);
                    w.Stop();
                    File.Delete(Voice_Set.Special_Path + "\\Wwise\\Temp.ogg");
                    return To_OGG_WAV_File + ".ogg";
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
                    WEMToWAV.Dispose();
                    if (File.Exists(Voice_Set.Special_Path + "\\Wwise\\Temp.wav"))
                    {
                        File_Move(Voice_Set.Special_Path + "\\Wwise\\Temp.wav", To_OGG_WAV_File + ".wav", true);
                        if (IsFromFileDelete)
                            File.Delete(From_WEM_File);
                        return To_OGG_WAV_File + ".wav";
                    }
                }
                return "";
            }
            catch
            {
                return "";
            }
        }
        //.wemファイルを指定した形式に変換
        public static bool WEM_To_File(string From_WEM_File, string To_Audio_File, string Encode_Mode, bool IsFromFileDelete)
        {
            try
            {
                if (!File.Exists(From_WEM_File))
                    return false;
                Process wwToOgg = new Process();
                wwToOgg.StartInfo.FileName = Voice_Set.Special_Path + "/Wwise/ww2ogg.exe";
                wwToOgg.StartInfo.WorkingDirectory = Voice_Set.Special_Path + "/Wwise";
                wwToOgg.StartInfo.Arguments = "--pcb packed_codebooks_aoTuV_603.bin -o \"" + Voice_Set.Special_Path + "\\Wwise\\Temp.ogg\" \"" + From_WEM_File + "\"";
                wwToOgg.StartInfo.CreateNoWindow = true;
                wwToOgg.StartInfo.UseShellExecute = false;
                wwToOgg.StartInfo.RedirectStandardError = true;
                wwToOgg.StartInfo.RedirectStandardOutput = true;
                wwToOgg.Start();
                wwToOgg.WaitForExit();
                //Wwise_Class.WEM_To_OGG.Create_OGG(From_WEM_File, Voice_Set.Special_Path + "\\Wwise\\Temp.ogg");
                if (File.Exists(Voice_Set.Special_Path + "\\Wwise\\Temp.ogg"))
                {
                    Process Reverb = new Process();
                    Reverb.StartInfo.FileName = Voice_Set.Special_Path + "/Wwise/revorb.exe";
                    Reverb.StartInfo.Arguments = "\"" + Voice_Set.Special_Path + "\\Wwise\\Temp.ogg\"";
                    Reverb.StartInfo.CreateNoWindow = true;
                    Reverb.StartInfo.UseShellExecute = false;
                    Reverb.Start();
                    Reverb.WaitForExit();
                    Reverb.Dispose();
                    if (Encode_Mode == "ogg")
                        Sub_Code.File_Move(Voice_Set.Special_Path + "\\Wwise\\Temp.ogg", To_Audio_File, true);
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
        public static void Wwise_Repair_Project(string Project_Dir)
        {
            string[] Files = Directory.GetFiles(Project_Dir, "*.wproj", SearchOption.TopDirectoryOnly);
            if (Files.Length == 0)
                return;
            string Project_File = Files[0];
            string SoundBanksFile = Project_Dir + "\\SoundBanks\\Default Work Unit.wwu";
            System.Collections.Generic.List<string> Project_Line = new List<string>();
            Project_Line.AddRange(File.ReadAllLines(Project_File));
            System.Collections.Generic.List<string> SoundBanks_Line = new List<string>();
            SoundBanks_Line.AddRange(File.ReadAllLines(SoundBanksFile));
            bool IsProjectChanged = false;
            bool IsSoundBanksChanged = false;
            for (int Number = 0; Number < Project_Line.Count; Number++)
            {
                if (Project_Line[Number].Contains("Path=\"Events\\Download_Wwise_Events\"") || Project_Line[Number].Contains("Path=\"Events\\Download_Wwise_Events\\LoadBGM"))
                {
                    Project_Line.RemoveAt(Number);
                    Project_Line.RemoveAt(Number);
                    Project_Line.RemoveAt(Number);
                    Number--;
                    IsProjectChanged = true;
                }
            }
            for (int Number = 0; Number < SoundBanks_Line.Count; Number++)
            {
                if (SoundBanks_Line[Number].Contains("</WwiseDocument>") && SoundBanks_Line.Count - 1 > Number + 1)
                {
                    SoundBanks_Line.RemoveRange(Number + 1, SoundBanks_Line.Count - Number - 1);
                    IsSoundBanksChanged = true;
                    break;
                }
            }
            if (IsProjectChanged)
                File.WriteAllLines(Project_File, Project_Line);
            if (IsSoundBanksChanged)
                File.WriteAllLines(SoundBanksFile, SoundBanks_Line);
            Project_Line.Clear();
            SoundBanks_Line.Clear();
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
        public static bool CanDirectoryAccess(string Dir_Path, bool IsNotExistToCreate = false)
        {
            try
            {
                if (IsNotExistToCreate && !Directory.Exists(Dir_Path))
                    Directory.CreateDirectory(Dir_Path);
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
        public static byte[] Resize_From_Bytes(MemoryStream Stream, int Size)
        {
            try
            {
                Bitmap bmp = new Bitmap(Stream);
                Bitmap res = new Bitmap(Size, Size);
                Graphics g = Graphics.FromImage(res);
                g.FillRectangle(new SolidBrush(Color.Black), 0, 0, Size, Size);
                int t = 0, l = 0;
                l = (bmp.Width - Size) / 2;
                t = (bmp.Height - Size) / 2;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(bmp, new Rectangle(0, 0, Size, Size), new Rectangle(l, t, Size, Size), GraphicsUnit.Pixel);
                MemoryStream ms = new MemoryStream();
                res.Save(ms, ImageFormat.Jpeg);
                byte[] buffer = ms.GetBuffer();
                g.Dispose();
                res.Dispose();
                bmp.Dispose();
                ms.Close();
                return buffer;
            }
            catch
            {
                return new byte[] { };
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
            if (WAV_File == "")
                return;
            if (Gain <= -20)
                Gain = -19.9;
            else if (Gain >= 12)
                Gain = 11.9;
            int Number = r.Next(0, 10000);
            StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Other/WAV_Set_Gain_" + Number + ".bat");
            stw.WriteLine("chcp 65001");
            if (WAV_File[0] == '"')
                stw.Write("\"" + Voice_Set.Special_Path + "/Other/WaveGain.exe\" -r -y -n -g " + Gain + " " + WAV_File);
            else
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
        public static void Create_WAV(string To_File, double Time)
        {
            int Stream = Un4seen.Bass.Bass.BASS_StreamCreate(44100, 2, Un4seen.Bass.BASSFlag.BASS_STREAM_DECODE, Un4seen.Bass.BASSStreamProc.STREAMPROC_DUMMY);
            Un4seen.Bass.Misc.EncoderWAV l = new Un4seen.Bass.Misc.EncoderWAV(Stream);
            l.InputFile = null;
            l.OutputFile = To_File;
            l.WAV_BitsPerSample = 24;
            l.Start(null, IntPtr.Zero, false);
            byte[] encBuffer = new byte[65536];
            while (Un4seen.Bass.Bass.BASS_ChannelIsActive(Stream) == Un4seen.Bass.BASSActive.BASS_ACTIVE_PLAYING)
            {
                int len = Un4seen.Bass.Bass.BASS_ChannelGetData(Stream, encBuffer, encBuffer.Length);
                long Pos_Byte = Un4seen.Bass.Bass.BASS_ChannelGetPosition(Stream, Un4seen.Bass.BASSMode.BASS_POS_BYTES);
                if (len <= 0)
                    break;
                else if (Time <= Un4seen.Bass.Bass.BASS_ChannelBytes2Seconds(Stream, Pos_Byte))
                    break;
            }
            l.Stop();
            Un4seen.Bass.Bass.BASS_StreamFree(Stream);
        }
        public static double Get_Decimal(double Value)
        {
            string Text = Value.ToString();
            if (!Text.Contains("."))
                return 0;
            string Decim = Text.Substring(Text.IndexOf('.') + 1);
            return double.Parse("0." + Decim);
        }
        public static double Get_Random_Double(double Minimum, double Maximum)
        {
            return r.NextDouble() * (Maximum - Minimum) + Minimum;
        }
        public static string Get_Time_String(double Position)
        {
            TimeSpan Time = TimeSpan.FromSeconds(Position);
            string Minutes = Time.Minutes.ToString();
            string Seconds = Time.Seconds.ToString();
            if (Time.Minutes < 10)
                Minutes = "0" + Time.Minutes;
            if (Time.Seconds < 10)
                Seconds = "0" + Time.Seconds;
            return Minutes + ":" + Seconds;
        }
        /*public static void Set_SE_Change_Name(string Project_SE_Dir, Wwise_Class.Wwise_Project_Create Wwise)
        {
            if (Voice_Set.SE_Enable_Disable[0])
                Sub_Code.File_Move(Project_SE_Dir + "/Capture_Finish_SE.wav", Project_SE_Dir + "/Capture_Finish_SE_tmp.wav", true);
            else
                Sub_Code.File_Move(Project_SE_Dir + "/Capture_Finish_SE_tmp.wav", Project_SE_Dir + "/Capture_Finish_SE.wav", true);
            if (Voice_Set.SE_Enable_Disable[1])
            {
                Sub_Code.File_Move(Project_SE_Dir + "/quick_commands_attack.wav", Project_SE_Dir + "/quick_commands_attack_tmp.wav", true);
                Sub_Code.File_Move(Project_SE_Dir + "/quick_commands_attack_target.wav", Project_SE_Dir + "/quick_commands_attack_target_tmp.wav", true);
                Sub_Code.File_Move(Project_SE_Dir + "/quick_commands_capture_base.wav", Project_SE_Dir + "/quick_commands_capture_base_tmp.wav", true);
                Sub_Code.File_Move(Project_SE_Dir + "/quick_commands_positive.wav", Project_SE_Dir + "/quick_commands_positive_tmp.wav", true);
                Sub_Code.File_Move(Project_SE_Dir + "/quick_commands_defend_base.wav", Project_SE_Dir + "/quick_commands_defend_base_tmp.wav", true);
                Sub_Code.File_Move(Project_SE_Dir + "/quick_commands_help_me.wav", Project_SE_Dir + "/quick_commands_help_me_tmp.wav", true);
                Sub_Code.File_Move(Project_SE_Dir + "/quick_commands_negative.wav", Project_SE_Dir + "/quick_commands_negative_tmp.wav", true);
                Sub_Code.File_Move(Project_SE_Dir + "/quick_commands_reloading.wav", Project_SE_Dir + "/quick_commands_reloading_tmp.wav", true);
            }
            else
            {
                Sub_Code.File_Move(Project_SE_Dir + "/quick_commands_attack_tmp.wav", Project_SE_Dir + "/quick_commands_attack.wav", true);
                Sub_Code.File_Move(Project_SE_Dir + "/quick_commands_attack_target_tmp.wav", Project_SE_Dir + "/quick_commands_attack_target.wav", true);
                Sub_Code.File_Move(Project_SE_Dir + "/quick_commands_capture_base_tmp.wav", Project_SE_Dir + "/quick_commands_capture_base.wav", true);
                Sub_Code.File_Move(Project_SE_Dir + "/quick_commands_positive_tmp.wav", Project_SE_Dir + "/quick_commands_positive.wav", true);
                Sub_Code.File_Move(Project_SE_Dir + "/quick_commands_defend_base_tmp.wav", Project_SE_Dir + "/quick_commands_defend_base.wav", true);
                Sub_Code.File_Move(Project_SE_Dir + "/quick_commands_help_me_tmp.wav", Project_SE_Dir + "/quick_commands_help_me.wav", true);
                Sub_Code.File_Move(Project_SE_Dir + "/quick_commands_negative_tmp.wav", Project_SE_Dir + "/quick_commands_negative.wav", true);
                Sub_Code.File_Move(Project_SE_Dir + "/quick_commands_reloading_tmp.wav", Project_SE_Dir + "/quick_commands_reloading.wav", true);
            }
            if (Voice_Set.SE_Enable_Disable[6])
                Sub_Code.File_Move(Project_SE_Dir + "/Musenki_01.wav", Project_SE_Dir + "/Musenki_01_temp.wav", true);
            else
                Sub_Code.File_Move(Project_SE_Dir + "/Musenki_01_temp.wav", Project_SE_Dir + "/Musenki_01.wav", true);
            if (Voice_Set.SE_Enable_Disable[9])
            {
                Sub_Code.File_Move(Project_SE_Dir + "/howitzer_load_01.wav", Project_SE_Dir + "/howitzer_load_01_tmp.wav", true);
                Sub_Code.File_Move(Project_SE_Dir + "/howitzer_load_03.wav", Project_SE_Dir + "/howitzer_load_03_tmp.wav", true);
                Sub_Code.File_Move(Project_SE_Dir + "/howitzer_load_04.wav", Project_SE_Dir + "/howitzer_load_04_tmp.wav", true);
                Sub_Code.File_Move(Project_SE_Dir + "/howitzer_load_05.wav", Project_SE_Dir + "/howitzer_load_05_tmp.wav", true);
            }
            else
            {
                Sub_Code.File_Move(Project_SE_Dir + "/howitzer_load_01_tmp.wav", Project_SE_Dir + "/howitzer_load_01.wav", true);
                Sub_Code.File_Move(Project_SE_Dir + "/howitzer_load_03_tmp.wav", Project_SE_Dir + "/howitzer_load_03.wav", true);
                Sub_Code.File_Move(Project_SE_Dir + "/howitzer_load_04_tmp.wav", Project_SE_Dir + "/howitzer_load_04.wav", true);
                Sub_Code.File_Move(Project_SE_Dir + "/howitzer_load_05_tmp.wav", Project_SE_Dir + "/howitzer_load_05.wav", true);
            }
            if (Voice_Set.SE_Enable_Disable[10])
                Sub_Code.File_Move(Project_SE_Dir + "/lamp_SE_01.wav", Project_SE_Dir + "/lamp_SE_01_tmp.wav", true);
            else
                Sub_Code.File_Move(Project_SE_Dir + "/lamp_SE_01_tmp.wav", Project_SE_Dir + "/lamp_SE_01.wav", true);
            if (Voice_Set.SE_Enable_Disable[11])
                Sub_Code.File_Move(Project_SE_Dir + "/enemy_sight.wav", Project_SE_Dir + "/enemy_sight_tmp.wav", true);
            else
                Sub_Code.File_Move(Project_SE_Dir + "/enemy_sight_tmp.wav", Project_SE_Dir + "/enemy_sight.wav", true);
            if (Voice_Set.SE_Enable_Disable[12])
                Sub_Code.File_Move(Project_SE_Dir + "/Timer_SE.wav", Project_SE_Dir + "/Timer_SE_tmp.wav", true);
            else
                Sub_Code.File_Move(Project_SE_Dir + "/Timer_SE_tmp.wav", Project_SE_Dir + "/Timer_SE.wav", true);
            if (Voice_Set.SE_Enable_Disable[13])
                Sub_Code.File_Move(Project_SE_Dir + "/target_on_SE_01.wav", Project_SE_Dir + "/target_on_SE_01_tmp.wav", true);
            else
                Sub_Code.File_Move(Project_SE_Dir + "/target_on_SE_01_tmp.wav", Project_SE_Dir + "/target_on_SE_01.wav", true);
            if (Voice_Set.SE_Enable_Disable[14])
                Sub_Code.File_Move(Project_SE_Dir + "/target_off_SE_01.wav", Project_SE_Dir + "/target_off_SE_01_tmp.wav", true);
            else
                Sub_Code.File_Move(Project_SE_Dir + "/target_off_SE_01_tmp.wav", Project_SE_Dir + "/target_off_SE_01.wav", true);
        }*/
        public static void Set_Event_ShortID(List<List<Voice_Event_Setting>> Event_Settings, bool IsWoTMode = false)
        {
            //イベントID, 音声コンテナID, SEコンテナID, SE_Index, 音量
            Event_Settings[0][0].Set_Param(341425709, 170029050, 366092539, 5, -6);
            Event_Settings[0][1].Set_Param(908426860, 95559763, 370075103, 3, -2);
            Event_Settings[0][2].Set_Param(280189980, 766083947, 298293840, 9, -1);
            Event_Settings[0][3].Set_Param(815358870, 569784404, 862763776, 5, -6);
            Event_Settings[0][4].Set_Param(49295125, 266422868, 876186554, 6, -1);
            Event_Settings[0][5].Set_Param(733342682, 1052258113, 568110765, 10, -1);
            Event_Settings[0][6].Set_Param(331196727, 242302464, 66753859, 18, -1);
            Event_Settings[0][7].Set_Param(619058694, 334837201, 162440597, 18, -1);
            Event_Settings[0][8].Set_Param(794420468, 381780774, 52837378, 23);
            Event_Settings[0][9].Set_Param(109598189, 489572734, 582349497, 5, -1);
            Event_Settings[0][10].Set_Param(244621664, 210078142, 750651777, 19, -2);
            Event_Settings[0][11].Set_Param(73205091, 249535989, 1042937732, 20, -2);
            Event_Settings[0][12].Set_Param(466111031, 908710042, 125367048, 21, -2);
            Event_Settings[0][13].Set_Param(471196930, 1057023960);
            Event_Settings[0][14].Set_Param(337626756, 953778289);
            Event_Settings[0][15].Set_Param(930519512, 121897540, 602706971, 8, -1);
            Event_Settings[0][16].Set_Param(1063632502, 127877647, 953241595, 19, -2);
            Event_Settings[0][17].Set_Param(175994480, 462397017, 734855314, 20, -2);
            Event_Settings[0][18].Set_Param(546476029, 651656679, 265156722, 21, -2);
            Event_Settings[0][19].Set_Param(337748775, 739086111, 738480888, 18);
            Event_Settings[0][20].Set_Param(302644322, 363753108, 97368200, 18);
            Event_Settings[0][21].Set_Param(356562073, 91697210, 948692451, 7);
            Event_Settings[0][22].Set_Param(156782042, 987172940, 87851485, 18);
            Event_Settings[0][23].Set_Param(769815093, 518589126, 267487625, 22);
            Event_Settings[0][24].Set_Param(236686366, 330491031, 904204732, 19, -2);
            Event_Settings[0][25].Set_Param(559710262, 792301846, 42606663, 20, -2);
            Event_Settings[0][26].Set_Param(47321344, 539730785, 308135346, 21, -2);
            Event_Settings[0][27].Set_Param(978556760, 38261315, 792373436, 19, -2);
            Event_Settings[0][28].Set_Param(878993268, 37535832);
            Event_Settings[0][29].Set_Param(581830963, 558576963);
            Event_Settings[0][30].Set_Param(984973529, 1014565012, 124621166, 19, -2);
            Event_Settings[0][31].Set_Param(381112709, 135817430, 634991721, 20, -2);
            Event_Settings[0][32].Set_Param(33436524, 985679417, 940515369, 21, -2);
            Event_Settings[0][33].Set_Param(116097397, 164671745, 667880140, 4);
            Event_Settings[1][0].Set_Param(308272618, 447063394, 479275647, 13);
            Event_Settings[1][1].Set_Param(767278023, 154835998, 917399664, 12);
            Event_Settings[1][2].Set_Param(230904672, 607694618, 904269149, 2);
            Event_Settings[1][3].Set_Param(390478464, 391276124, 747137713, 2);
            Event_Settings[1][4].Set_Param(17969037, 840378218, 990119123, 2);
            Event_Settings[1][5].Set_Param(900922817, 549968154, 1039956691, 2);
            Event_Settings[1][6].Set_Param(727518878, 1015337424, 1041861596, 2);
            Event_Settings[1][7].Set_Param(101252368, 271044645, 284419845, 2);
            Event_Settings[1][8].Set_Param(576711003, 496552975, 93467631, 2);
            Event_Settings[1][9].Set_Param(470859110, 430377111, 236153639, 2);
            Event_Settings[1][10].Set_Param(502585189, 839607605, 391999685, 15);
            Event_Settings[1][11].Set_Param(769354725, 233444430, 166694669, 16);
            Event_Settings[1][12].Set_Param(402727222, 299739777, 769579073, 11);
            Event_Settings[1][13].Set_Param(670169971, 120795627, 951031474, 24);
            Event_Settings[1][14].Set_Param(204685755, 820440351, 206640353, 1);
            Event_Settings[1][15].Set_Param(1065169508, 891902653);
            Event_Settings[1][16].Set_Param(198183306, 52813795, 394210856, 25);
            if (!Event_Settings[1][15].IsLoadMode)
                Event_Settings[1][15].Volume = -11;
            Event_Settings[2][0].Set_Param(420002792, 491691546);
            Event_Settings[2][1].Set_Param(420002792, 417768496);
            Event_Settings[2][2].Set_Param(420002792, 46472417);
            Event_Settings[2][3].Set_Param(420002792, 681331945);
            Event_Settings[2][4].Set_Param(420002792, 190711689);
            Event_Settings[2][5].Set_Param(420002792, 918836720);
        }
        public static void Set_Event_ShortID(List<Dictionary<string, Voice_Event_Setting>> Event_Settings)
        {
            Event_Settings[0]["味方にダメージ"].Set_Param("vo_ally_killed_by_player", 647867654);
            Event_Settings[0]["弾薬庫破損"].Set_Param("vo_ammo_bay_damaged", 956781602);
            Event_Settings[0]["敵への非貫通"].Set_Param("vo_armor_not_pierced_by_player", 891351729);
            Event_Settings[0]["敵への跳弾"].Set_Param("vo_armor_ricochet_by_player", 985747875);
            Event_Settings[0]["敵への至近弾"].Set_Param("vo_damage_by_near_explosion_by_player", 401760710);
            Event_Settings[0]["敵炎上"].Set_Param("vo_enemy_fire_started_by_player", 962073157);
            Event_Settings[0]["敵への榴弾直撃"].Set_Param("vo_enemy_hp_damaged_by_explosion_at_direct_hit_by_player", 375524654);
            Event_Settings[0]["敵への有効弾(+履帯ダメージ)"].Set_Param("vo_enemy_hp_damaged_by_projectile_and_chassis_damaged_by_player", 258641633);
            Event_Settings[0]["敵への有効弾(+モジュールダメージ)"].Set_Param("vo_enemy_hp_damaged_by_projectile_and_gun_damaged_by_player", 406687788);
            Event_Settings[0]["敵への有効弾"].Set_Param("vo_enemy_hp_damaged_by_projectile_by_player", 715908210);
            Event_Settings[0]["自車両が敵を撃破"].Set_Param("vo_enemy_killed_by_player", 130818866);
            Event_Settings[0]["味方が敵を撃破"].Set_Param("expl_enemy_NPC", 110599610);
            Event_Settings[0]["敵の誤射による敵撃破"].Set_Param("vo_enemy_killed", 206068019);
            Event_Settings[0]["敵が味方車両を撃破"].Set_Param("expl_ally_NPC", 24820636);
            Event_Settings[0]["敵への非貫通(+履帯破壊)"].Set_Param("vo_enemy_no_hp_damage_at_attempt_and_chassis_damaged_by_player", 123484080);
            Event_Settings[0]["敵への非貫通(+モジュール破壊)"].Set_Param("vo_enemy_no_hp_damage_at_attempt_and_gun_damaged_by_player", 606723544);
            Event_Settings[0]["敵への非貫通(+履帯ダメージ)"].Set_Param("vo_enemy_no_hp_damage_at_no_attempt_and_chassis_damaged_by_player", 826504023);
            Event_Settings[0]["敵への非貫通(+モジュールダメージ)"].Set_Param("vo_enemy_no_hp_damage_at_no_attempt_and_gun_damaged_by_player", 556370764);
            Event_Settings[0]["敵への非貫通"].Set_Param("vo_enemy_no_hp_damage_at_no_attempt_by_player", 891351729);
            Event_Settings[0]["搭乗員全滅"].Set_Param("vo_crew_deactivated", 156469938);
            Event_Settings[0]["車長負傷"].Set_Param("vo_commander_killed", 803799062);
            Event_Settings[0]["操縦手負傷"].Set_Param("vo_driver_killed", 509522089);
            Event_Settings[0]["砲手負傷"].Set_Param("vo_gunner_killed", 93452340);
            Event_Settings[0]["装填手負傷"].Set_Param("vo_loader_killed", 825298907);
            Event_Settings[0]["通信手負傷"].Set_Param("vo_radioman_killed", 304672798);
            Event_Settings[0]["自車両火災"].Set_Param("vo_fire_started", 630401544);
            Event_Settings[0]["自車両消火"].Set_Param("vo_fire_stopped", 228596803);
            Event_Settings[0]["燃料タンク破損"].Set_Param("vo_fuel_tank_damaged", 241888377);
            Event_Settings[0]["無線機損傷"].Set_Param("vo_radio_damaged", 298484235);
            Event_Settings[0]["戦闘開始"].Set_Param("vo_start_battle", 779140459);
            Event_Settings[0]["自車両大破"].Set_Param("vo_vehicle_destroyed", 112179165);
            Event_Settings[1]["エンジン破損"].Set_Param("vo_engine_damaged", 194063185);
            Event_Settings[1]["エンジン大破"].Set_Param("vo_engine_destroyed", 606689164);
            Event_Settings[1]["エンジン復旧"].Set_Param("vo_engine_functional", 841042183);
            Event_Settings[1]["砲身破損"].Set_Param("vo_gun_damaged", 910164046);
            Event_Settings[1]["砲身大破"].Set_Param("vo_gun_destroyed", 3474169);
            Event_Settings[1]["砲身復旧"].Set_Param("vo_gun_functional", 52992479);
            Event_Settings[1]["観測装置破損"].Set_Param("vo_surveying_devices_damaged", 260981670);
            Event_Settings[1]["観測装置大破"].Set_Param("vo_surveying_devices_destroyed", 302485444);
            Event_Settings[1]["観測装置復旧"].Set_Param("vo_surveying_devices_functional", 867900123);
            Event_Settings[1]["履帯破損"].Set_Param("vo_track_damaged", 666482897);
            Event_Settings[1]["履帯大破"].Set_Param("vo_track_destroyed", 604740571);
            Event_Settings[1]["履帯復旧"].Set_Param("vo_track_functional", 55589824);
            Event_Settings[1]["履帯復旧+移動可能"].Set_Param("vo_track_functional_can_move", 704248833);
            Event_Settings[1]["砲塔破損"].Set_Param("vo_turret_rotator_damaged", 982380106);
            Event_Settings[1]["砲塔大破"].Set_Param("vo_turret_rotator_destroyed", 68494932);
            Event_Settings[1]["砲塔復旧"].Set_Param("vo_turret_rotator_functional", 354769968);
            Event_Settings[2]["小隊へ勧誘"].Set_Param("vo_dp_assistance_been_requested", 147467011);
            Event_Settings[2]["小隊へ参加(自身)"].Set_Param("vo_dp_platoon_joined", 480443644);
            Event_Settings[2]["小隊へ参加(他人)"].Set_Param("vo_dp_player_joined_platoon", 745934433);
            Event_Settings[2]["オートエイム開始"].Set_Param("vo_target_captured", 884977578);
            Event_Settings[2]["オートエイムロスト"].Set_Param("vo_target_lost", 111545873);
            Event_Settings[2]["オートエイム解除"].Set_Param("vo_target_unlocked", 798032659);
            Event_Settings[2]["第六感"].Set_Param("lightbulb", 665693041);
            Event_Settings[2]["自走砲の警報"].Set_Param("artillery_lightbulb", 249143697);
        }
        public static void Set_Event_ShortID(List<Voice_Event_Setting> Event_Settings)
        {
            Event_Settings[0].Set_Param(341425709, 170029050, 366092539, 5, -6);
            Event_Settings[1].Set_Param(908426860, 95559763, 370075103, 3, -2);
            Event_Settings[2].Set_Param(280189980, 766083947, 298293840, 9, -1);
            Event_Settings[3].Set_Param(815358870, 569784404, 862763776, 5, -6);
            Event_Settings[4].Set_Param(49295125, 266422868, 876186554, 6, -1);
            Event_Settings[5].Set_Param(733342682, 1052258113, 568110765, 10, -1);
            Event_Settings[6].Set_Param(331196727, 242302464, 66753859, 18, -1);
            Event_Settings[7].Set_Param(619058694, 334837201, 162440597, 18, -1);
            Event_Settings[8].Set_Param(794420468, 381780774, 52837378, 23);
            Event_Settings[9].Set_Param(109598189, 489572734, 582349497, 5, -1);
            Event_Settings[10].Set_Param(244621664, 210078142, 750651777, 19, -2);
            Event_Settings[11].Set_Param(73205091, 249535989, 1042937732, 20, -2);
            Event_Settings[12].Set_Param(466111031, 908710042, 125367048, 21, -2);
            Event_Settings[13].Set_Param(471196930, 1057023960);
            Event_Settings[14].Set_Param(337626756, 953778289);
            Event_Settings[15].Set_Param(930519512, 121897540, 602706971, 8, -1);
            Event_Settings[16].Set_Param(1063632502, 127877647, 953241595, 19, -2);
            Event_Settings[17].Set_Param(175994480, 462397017, 734855314, 20, -2);
            Event_Settings[18].Set_Param(546476029, 651656679, 265156722, 21, -2);
            Event_Settings[19].Set_Param(337748775, 739086111, 738480888, 18);
            Event_Settings[20].Set_Param(302644322, 363753108, 97368200, 18);
            Event_Settings[21].Set_Param(356562073, 91697210, 948692451, 7);
            Event_Settings[22].Set_Param(156782042, 987172940, 87851485, 18);
            Event_Settings[23].Set_Param(769815093, 518589126, 267487625, 22);
            Event_Settings[24].Set_Param(236686366, 330491031, 904204732, 19, -2);
            Event_Settings[25].Set_Param(559710262, 792301846, 42606663, 20, -2);
            Event_Settings[26].Set_Param(47321344, 539730785, 308135346, 21, -2);
            Event_Settings[27].Set_Param(978556760, 38261315, 792373436, 19, -2);
            Event_Settings[28].Set_Param(878993268, 37535832);
            Event_Settings[29].Set_Param(581830963, 558576963);
            Event_Settings[30].Set_Param(984973529, 1014565012, 124621166, 19, -2);
            Event_Settings[31].Set_Param(381112709, 135817430, 634991721, 20, -2);
            Event_Settings[32].Set_Param(33436524, 985679417, 940515369, 21, -2);
            Event_Settings[33].Set_Param(116097397, 164671745, 667880140, 4);
            Event_Settings[34].Set_Param(308272618, 447063394, 479275647, 13);
            Event_Settings[35].Set_Param(767278023, 154835998, 917399664, 12);
            Event_Settings[36].Set_Param(230904672, 607694618, 904269149, 2);
            Event_Settings[37].Set_Param(390478464, 391276124, 747137713, 2);
            Event_Settings[38].Set_Param(17969037, 840378218, 990119123, 2);
            Event_Settings[39].Set_Param(900922817, 549968154, 1039956691, 2);
            Event_Settings[40].Set_Param(727518878, 1015337424, 1041861596, 2);
            Event_Settings[41].Set_Param(101252368, 271044645, 284419845, 2);
            Event_Settings[42].Set_Param(576711003, 310153012, 93467631, 2);
            Event_Settings[43].Set_Param(470859110, 379548034, 236153639, 2);
            Event_Settings[44].Set_Param(502585189, 839607605, 391999685, 15);
            Event_Settings[45].Set_Param(769354725, 233444430, 166694669, 16);
            Event_Settings[46].Set_Param(402727222, 299739777, 769579073, 11);
            Event_Settings[47].Set_Param(670169971, 120795627, 120795627, 2);
            Event_Settings[48].Set_Param(204685755, 924876614, 206640353, 1);
            Event_Settings[49].Set_Param(1065169508, 891902653);
        }
        public static uint Get_Container_By_WoT_Voice(int Type)
        {
            if (Type == 0)
                return 170029050;
            if (Type == 1)
                return 95559763;
            if (Type == 2)
                return 766083947;
            if (Type == 3)
                return 569784404;
            if (Type == 4)
                return 266422868;
            if (Type == 5)
                return 1052258113;
            if (Type == 6)
                return 242302464;
            if (Type == 7)
                return 334837201;
            if (Type == 8)
                return 381780774;
            if (Type == 9)
                return 489572734;
            if (Type == 10)
                return 210078142;
            if (Type == 11)
                return 249535989;
            if (Type == 12)
                return 908710042;
            if (Type == 13)
                return 1057023960;
            if (Type == 14)
                return 953778289;
            if (Type == 15)
                return 121897540;
            if (Type == 16)
                return 127877647;
            if (Type == 17)
                return 462397017;
            if (Type == 18)
                return 651656679;
            if (Type == 19)
                return 739086111;
            if (Type == 20)
                return 363753108;
            if (Type == 21)
                return 91697210;
            if (Type == 22)
                return 987172940;
            if (Type == 23)
                return 518589126;
            if (Type == 24)
                return 330491031;
            if (Type == 25)
                return 792301846;
            if (Type == 26)
                return 539730785;
            if (Type == 27)
                return 38261315;
            if (Type == 28)
                return 37535832;
            if (Type == 29)
                return 558576963;
            if (Type == 30)
                return 1014565012;
            if (Type == 31)
                return 135817430;
            if (Type == 32)
                return 985679417;
            if (Type == 33)
                return 164671745;
            if (Type == 34)
                return 447063394;
            if (Type == 35)
                return 154835998;
            if (Type == 36)
                return 607694618;
            if (Type == 37)
                return 391276124;
            if (Type == 38)
                return 840378218;
            if (Type == 39)
                return 549968154;
            if (Type == 40)
                return 1015337424;
            if (Type == 41)
                return 271044645;
            if (Type == 42)
                return 310153012;
            if (Type == 43)
                return 379548034;
            if (Type == 44)
                return 839607605;
            if (Type == 45)
                return 233444430;
            if (Type == 46)
                return 299739777;
            if (Type == 47)
                return 120795627;
            if (Type == 48)
                return 924876614;
            if (Type == 49)
                return 891902653;
            return 0;
        }
        public static uint Get_WoTB_New_Gun_Sound_ShortID(int Index)
        {
            if (Index == 0)
                return 634610718;
            else if (Index == 1)
                return 142135010;
            else if (Index == 2)
                return 611442385;
            else if (Index == 3)
                return 752170755;
            else if (Index == 4)
                return 220137673;
            else if (Index == 5)
                return 983327549;
            else if (Index == 6)
                return 342549628;
            else if (Index == 7)
                return 76784519;
            else if (Index == 8)
                return 670420603;
            else if (Index == 9)
                return 488206709;
            else if (Index == 10)
                return 91221195;
            else if (Index == 11)
                return 1023399622;
            else if (Index == 12)
                return 547631281;
            else if (Index == 13)
                return 61886891;
            else if (Index == 14)
                return 619459354;
            else if (Index == 15)
                return 890327147;
            else if (Index == 16)
                return 697334890;
            else if (Index == 17)
                return 950138696;
            else if (Index == 18)
                return 361462963;
            else if (Index == 19)
                return 5188110;
            else if (Index == 20)
                return 349435285;
            else if (Index == 21)
                return 288197594;
            else if (Index == 22)
                return 499157722;
            return 0;
        }

        public static bool ShowExplorerFile(string filePath)
        {
            try
            {
                Type comShellType = Type.GetTypeFromProgID("Shell.Application");
                dynamic shell = Activator.CreateInstance(comShellType);
                dynamic windows = shell.Windows();
                foreach (dynamic win in windows)
                {
                    string tmp = win.FullName;
                    if (String.Compare(Path.GetFileName(tmp), "EXPLORER.EXE", true) == 0)
                    {
                        string webUri = win.LocationURL;
                        if (webUri != "")
                        {
                            Uri u = new Uri(webUri);
                            if (u.IsFile)
                            {
                                string path = u.LocalPath + Uri.UnescapeDataString(u.Fragment);
                                if (Path.GetDirectoryName(filePath) == path)
                                {
                                    long hwndValue = win.HWND;
                                    IntPtr hwnd = new IntPtr(hwndValue);
                                    if (MainCode.IsIconic(hwnd))
                                        MainCode.ShowWindow(hwnd, 9);
                                    MainCode.SetForegroundWindow(hwnd);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch { };
            return false;
        }

        public static bool ShowExplorerFileAndSelect(string filePath)
        {
            IntPtr nativeFolder;
            uint psfgaoOut;
            SHParseDisplayName(Path.GetDirectoryName(filePath), IntPtr.Zero, out nativeFolder, 0, out psfgaoOut);

            if (nativeFolder == IntPtr.Zero)
            {
                // Log error, can't find folder
                return false;
            }

            IntPtr nativeFile;
            SHParseDisplayName(filePath, IntPtr.Zero, out nativeFile, 0, out psfgaoOut);

            IntPtr[] fileArray;
            if (nativeFile == IntPtr.Zero)
            {
                // Open the folder without the file selected if we can't find the file
                fileArray = new IntPtr[0];
            }
            else
            {
                fileArray = new IntPtr[] { nativeFile };
            }

            SHOpenFolderAndSelectItems(nativeFolder, (uint)fileArray.Length, fileArray, 0);

            Marshal.FreeCoTaskMem(nativeFolder);
            if (nativeFile != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(nativeFile);
                return true;
            }
            return false;
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