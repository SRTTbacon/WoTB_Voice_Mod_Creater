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
        readonly static string Special_Path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/WoTB_Voice_Mod_Creater";
        static bool IsServerCreating = false;
        static bool IsCreatingProject = false;
        static bool IsVolumeSet = false;
        static bool IsDVPLEncode = false;
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
        public static void WoTB_Get_Directory()
        {
            try
            {
                RegistryKey rKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Valve\\Steam");
                string location = (string)rKey.GetValue("InstallPath");
                rKey.Close();
                string driveRegex = @"[A-Z]:\\";
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
                            StreamWriter stw = File.CreateText(Special_Path + "/Temp_WoTB_Path.dat");
                            stw.Write(item2 + "World of Tanks Blitz");
                            stw.Close();
                            using (var eifs = new FileStream(Special_Path + "/Temp_WoTB_Path.dat", FileMode.Open, FileAccess.Read))
                            {
                                using (var eofs = new FileStream(Directory.GetCurrentDirectory() + "/WoTB_Path.dat", FileMode.Create, FileAccess.Write))
                                {
                                    FileEncode.FileEncryptor.Encrypt(eifs, eofs, "WoTB_Directory_Path_Pass");
                                }
                            }
                            File.Delete(Special_Path + "/Temp_WoTB_Path.dat");
                            Voice_Set.WoTB_Path = item2 + "World of Tanks Blitz";
                            return;
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("WoTBのインストール先を取得できませんでした。SteamにWoTBがインストールされていないか、32BitOSを使用している可能性があります。");
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
            FilePath = FilePath.Replace(".dvpl", "");
            if (File.Exists(FilePath))
            {
                try
                {
                    File.Delete(FilePath);
                    return true;
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
                    return true;
                }
                catch
                {
                    
                }
            }
            return false;
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
                if (Read != "ID3" || Read_01.Contains("Xing"))
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
                StreamWriter stw = File.CreateText(Special_Path + "/Encode_Mp3/Start.bat");
                stw.Write("\"" + Special_Path + "/Encode_Mp3/ffmpeg.exe\" -i \"" + File_Now + "\" -acodec libmp3lame -ab 128k \"" + Dir + "/" + Path.GetFileNameWithoutExtension(File_Now) + ".mp3\"");
                stw.Close();
                ProcessStartInfo processStartInfo1 = new ProcessStartInfo
                {
                    FileName = Special_Path + "/Encode_Mp3/Start.bat",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process p = Process.Start(processStartInfo1);
                p.WaitForExit();
                File.Delete(Special_Path + "/Encode_Mp3/Start.bat");
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
                StreamWriter stw = File.CreateText(Special_Path + "/Encode_Mp3/Volume_Set.bat");
                stw.Write("\"" + Special_Path + "/Encode_Mp3/mp3gain.exe\" -r -c -p -d 10 " + File_Import);
                stw.Close();
                ProcessStartInfo processStartInfo1 = new ProcessStartInfo
                {
                    FileName = Special_Path + "/Encode_Mp3/Volume_Set.bat",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process p = Process.Start(processStartInfo1);
                p.WaitForExit();
                File.Delete(Special_Path + "/Encode_Mp3/Volume_Set.bat");
            }
        }
        //.fdpファイルから.fev + .fsbを作成する
        //例:Test.fdp -> Test.fevとTest.fsbを作成
        public static async Task Project_Build(string Project_File, System.Windows.Controls.TextBlock Message_T)
        {
            StreamWriter stw2 = File.CreateText(Special_Path + "/Fmod_Designer/BGM_Create.bat");
            stw2.Write("\"" + Special_Path + "/Fmod_Designer/fmod_designercl.exe\" -pc -adpcm " + Project_File);
            stw2.Close();
            Process p2 = new Process();
            p2.StartInfo.FileName = Special_Path + "/Fmod_Designer/BGM_Create.bat";
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
            File.Delete(Special_Path + "/Fmod_Designer/BGM_Create.bat");
        }
        //.dvplを解除する
        //例:sounds.yaml.dvpl->sounds.yaml
        public static void DVPL_Unlock(string From_File, string To_File, bool IsOverWrite = true)
        {
            if (!IsOverWrite && File.Exists(To_File))
            {
                return;
            }
            StreamWriter DVPL_Unpack = File.CreateText(Special_Path + "/DVPL/UnPack.bat");
            DVPL_Unpack.Write("\"" + Special_Path + "/DVPL/Python/python.exe\" \"" + Special_Path + "/DVPL/UnPack.py\" \"" + From_File + "\" \"" + To_File + "\"");
            DVPL_Unpack.Close();
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = Special_Path + "/DVPL/UnPack.bat",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process p = Process.Start(processStartInfo);
            p.WaitForExit();
            File.Delete(Special_Path + "/DVPL/UnPack.bat");
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
    }
}