﻿using FluentFTP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WK.Libraries.BetterFolderBrowserNS;
using WoTB_Voice_Mod_Creater.Class;

public static partial class StringExtensions
{
    //文字列に指定した文字が何個あるか取得
    public static int CountOf(this string self, string Name)
    {
        int count = 0;
        int index = self.IndexOf(Name, 0);
        while (index != -1)
        {
            count++;
            index = self.IndexOf(Name, index + Name.Length);
        }
        return count;
    }
}
public static class ListBoxEx
{
    public static IList<int> SelectedIndexs(System.Windows.Controls.ListBox list)
    {
        List<int> Temp = new List<int>();
        foreach (string Now_String in list.SelectedItems)
        {
            Temp.Add(list.Items.IndexOf(Now_String));
        }
        return Temp;
    }
}
public static class ButtonExtensions
{
    public static void PerformClick(this System.Windows.Controls.Button button)
    {
        if (button == null)
            throw new ArgumentNullException("button");
        var provider = new ButtonAutomationPeer(button) as IInvokeProvider;
        provider.Invoke();
    }
}
public class SRTTbacon_Server
{
    public const string IP_Local = "非公開";
    public const string IP_Global = "非公開";
    public const string Name = "非公開";
    public const string Password = "非公開";
    public const string Version = "1.4.0";
    public const int Port = -1;
    public static bool IsSRTTbaconOwnerMode = false;
    public static string IP = "";
}
namespace WoTB_Voice_Mod_Creater
{
    public partial class MainCode : Window
    {
        readonly string Path = Directory.GetCurrentDirectory();
        bool IsClosing = false;
        bool IsMessageShowing = false;
        bool IsIncludeJapanese = false;
        bool IsFullScreen = true;
        bool IsChange_To_Wwise_Checked = false;
        //チャットモード(0が全体:1がサーバー内:2が管理者チャット)
        //管理者チャットは管理者(SRTTbacon)と個人チャットする用(主にバグ報告かな？)
        int Chat_Mode = 0;
        readonly List<string> Server_Names_List = new List<string>();
        BrushConverter bc = new BrushConverter();
        public MainCode()
        {
            try
            {
                StreamWriter stw = File.CreateText(Path + "/Test.dat");
                stw.WriteLine("テストファイル");
                stw.Close();
                File.Delete(Path + "/Test.dat");
            }
            catch
            {
                MessageBox.Show("フォルダにアクセスできませんでした。ソフトを別の場所に移動する必要があります。");
                Application.Current.Shutdown();
            }
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBox.Show("インターネットに接続されていません。\nソフトは強制終了されます");
                Application.Current.Shutdown();
            }
            //V1.2.8以前からアップデートしたとき用
            if (File.Exists(Path + "/bass.dll"))
            {
                try
                {
                    if (!Directory.Exists(Path + "/dll"))
                        Directory.CreateDirectory(Path + "/dll");
                    Sub_Code.File_Move(Path + "/bass.dll", Path + "/dll/bass.dll", true);
                    Sub_Code.File_Move(Path + "/bass_fx.dll", Path + "/dll/bass_fx.dll", true);
                    Sub_Code.File_Move(Path + "/fmod_event.dll", Path + "/dll/fmod_event.dll", true);
                    Sub_Code.File_Move(Path + "/fmodex.dll", Path + "/dll/fmodex.dll", true);
                    Sub_Code.File_Delete_V2(Path + "/bass.Net.dll");
                }
                catch (Exception e)
                {
                    Sub_Code.Error_Log_Write(e.Message);
                }
            }
            if (!File.Exists(Path + "/dll/DdsFileTypePlusIO_x86.dll"))
                DVPL.DDS_DLL_Extract();
            if (!File.Exists(Path + "/dll/bassenc.dll"))
                DVPL.BASS_DLL_Extract();
            if (!File.Exists(Path + "/dll/libmp3lame.dll"))
                DVPL.LAME_DLL_Extract();
            if (!File.Exists(Path + "/dll/bassmix.dll"))
                DVPL.BASS_MIX_DLL_Extract();
            if (Environment.UserName == "SRTTbacon")
                SRTTbacon_Server.IsSRTTbaconOwnerMode = true;
            //必要なdllがなかったら強制終了
            List<string> DLL_Error_List = Sub_Code.DLL_Exists();
            if (DLL_Error_List.Count > 0)
            {
                string DLLs = "";
                foreach (string DLL_None in DLL_Error_List)
                    DLLs += DLL_None + "\n";
                MessageBox.Show("/dll内に以下のファイルが存在しません。\n" + DLLs + "ソフトは強制終了されます。");
                Application.Current.Shutdown();
            }
            Clean();
            InitializeComponent();
            try
            {
                Download_P.Visibility = Visibility.Hidden;
                Download_T.Visibility = Visibility.Hidden;
                Load_Image.Visibility = Visibility.Hidden;
                WoTB_Select_B.Visibility = Visibility.Hidden;
                Server_B.Visibility = Visibility.Hidden;
                Voice_Create_Tool_B.Visibility = Visibility.Hidden;
                //Create_Save_B.Visibility = Visibility.Hidden;
                Save_Window.Opacity = 0;
                MouseLeftButtonDown += (sender, e) => { ScreenMove(); };
                Fmod_Player.ESystem.Init(128, Cauldron.FMOD.INITFLAGS.NORMAL, IntPtr.Zero);
                FMOD_API.System FModSys = new FMOD_API.System();
                FMOD_API.Factory.System_Create(ref FModSys);
                FMOD.Fmod_System.FModSystem = FModSys;
                FMOD.Fmod_System.FModSystem.init(16, FMOD_API.INITFLAGS.NORMAL, IntPtr.Zero);
                Connect_Mode_Layout();
                //自分はサーバーに参加できないためIPを分ける
                if (Environment.UserName == "SRTTbacon" || Environment.UserName == "SRTTbacon_V1")
                {
                    SRTTbacon_Server.IP = SRTTbacon_Server.IP_Local;
                    FtpDataConnectionType Mode = FtpDataConnectionType.AutoActive;
                    if (File.Exists(Path + "/Connect_Mode.txt"))
                    {
                        try
                        {
                            StreamReader str = new StreamReader(Path + "/Connect_Mode.txt");
                            string Mode_String = str.ReadLine().Replace(" ", "");
                            if (Mode_String == "PASV")
                                Mode = FtpDataConnectionType.PASV;
                            else if (Mode_String == "AutoActive")
                                Mode = FtpDataConnectionType.AutoActive;
                            else if (Mode_String == "EPRT")
                                Mode = FtpDataConnectionType.EPRT;
                            str.Close();
                        }
                        catch (Exception e)
                        {
                            Sub_Code.Error_Log_Write(e.Message);
                            Mode = FtpDataConnectionType.AutoActive;
                        }
                    }
                    else
                    {
                        Mode = FtpDataConnectionType.AutoActive;
                        StreamWriter stw = File.CreateText(Path + "/Connect_Mode.txt");
                        stw.Write(Mode);
                        stw.Close();
                    }
                    ConnectType = Mode;
                }
                else
                {
                    SRTTbacon_Server.IP = SRTTbacon_Server.IP_Global;
                    FtpDataConnectionType Mode = FtpDataConnectionType.PASV;
                    if (File.Exists(Path + "/Connect_Mode.txt"))
                    {
                        try
                        {
                            StreamReader str = new StreamReader(Path + "/Connect_Mode.txt");
                            string Mode_String = str.ReadLine().Replace(" ", "");
                            if (Mode_String == "PASV")
                                Mode = FtpDataConnectionType.PASV;
                            else if (Mode_String == "AutoActive")
                                Mode = FtpDataConnectionType.AutoActive;
                            else if (Mode_String == "EPRT")
                                Mode = FtpDataConnectionType.EPRT;
                            str.Close();
                        }
                        catch (Exception e)
                        {
                            Sub_Code.Error_Log_Write(e.Message);
                            Mode = FtpDataConnectionType.PASV;
                        }
                    }
                    else
                    {
                        Mode = FtpDataConnectionType.PASV;
                        StreamWriter stw = File.CreateText(Path + "/Connect_Mode.txt");
                        stw.Write(Mode);
                        stw.Close();
                    }
                    ConnectType = Mode;
                }
                //一時ファイルの保存先を変更している場合それを適応
                if (File.Exists(Path + "/TempDirPath.dat"))
                {
                    try
                    {
                        Sub_Code.File_Decrypt(Path + "/TempDirPath.dat", Path + "/Temp.dat", "Temp_Directory_Path_Pass", false);
                        StreamReader str = new StreamReader(Path + "/Temp.dat");
                        string Read = str.ReadLine();
                        str.Close();
                        File.Delete(Path + "/Temp.dat");
                        if (Read != "")
                            Voice_Set.Special_Path = Read;
                    }
                    catch (Exception e)
                    {
                        Sub_Code.Error_Log_Write(e.Message);
                    }
                }
                try
                {
                    if (!Directory.Exists(Voice_Set.Special_Path + "/Server"))
                        Directory.CreateDirectory(Voice_Set.Special_Path + "/Server");
                    if (!Directory.Exists(Voice_Set.Special_Path + "/Configs"))
                        Directory.CreateDirectory(Voice_Set.Special_Path + "/Configs");
                    //V1.2を実行した人用に削除
                    if (File.Exists(Voice_Set.Special_Path + "/DVPL/Pack.py"))
                        File.Delete(Voice_Set.Special_Path + "/DVPL/Pack.py");
                    if (File.Exists(Voice_Set.Special_Path + "/Other_Music_List.dat"))
                    {
                        Sub_Code.File_Move(Voice_Set.Special_Path + "/Other_Music_List.dat", Voice_Set.Special_Path + "/Configs/Other_Music_List.dat", false);
                        File.Delete(Voice_Set.Special_Path + "/Other_Music_List.dat");
                    }
                }
                catch (Exception e)
                {
                    Sub_Code.Error_Log_Write(e.Message);
                }
                Server_Connect();
                Chat_Mode_Public_B.Background = Brushes.Transparent;
                Chat_Mode_Server_B.Background = Brushes.Transparent;
                Chat_Mode_Private_B.Background = (Brush)bc.ConvertFrom("#59999999");
                Chat_Mode_Public_B.BorderBrush = Brushes.Transparent;
                Chat_Mode_Server_B.BorderBrush = Brushes.Transparent;
                Chat_Mode_Private_B.BorderBrush = Brushes.Red;
                Chat_Mode_Change(2);
                Window_Show();
                if (!File.Exists(Path + "/WoTB_Path.dat"))
                    Sub_Code.WoTB_Get_Directory();
                else
                {
                    try
                    {
                        Sub_Code.File_Decrypt(Path + "/WoTB_Path.dat", Voice_Set.Special_Path + "/Temp_WoTB_Path.dat", "WoTB_Directory_Path_Pass", false);
                        StreamReader str = new StreamReader(Voice_Set.Special_Path + "/Temp_WoTB_Path.dat");
                        string Read = str.ReadLine();
                        str.Close();
                        File.Delete(Voice_Set.Special_Path + "/Temp_WoTB_Path.dat");
                        if (!File.Exists(Read + "/wotblitz.exe"))
                        {
                            if (!Sub_Code.WoTB_Get_Directory())
                            {
                                WoTB_Select_B.Visibility = Visibility.Visible;
                                Message_Feed_Out("WoTBのインストール先を取得できません。手動で指定してください。");
                            }
                        }
                        else
                            Voice_Set.WoTB_Path = Read;
                    }
                    catch (Exception e)
                    {
                        if (!Sub_Code.WoTB_Get_Directory())
                        {
                            WoTB_Select_B.Visibility = Visibility.Visible;
                            Message_Feed_Out("WoTBのインストール先を取得できません。手動で指定してください。");
                        }
                        Sub_Code.Error_Log_Write(e.Message);
                    }
                }
                /*if (Voice_Set.WoTB_Path != "" && !Sub_Code.DVPL_File_Exists(Path + "/Backup/Main/sounds.yaml"))
                {
                    Directory.CreateDirectory(Path + "/Backup/Main");
                    Sub_Code.DVPL_File_Copy(Voice_Set.WoTB_Path + "/Data/sounds.yaml", Path + "/Backup/Main/sounds.yaml", false);
                    Sub_Code.DVPL_File_Copy(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_high.yaml", Path + "/Backup/Main/sfx_high.yaml", false);
                    Sub_Code.DVPL_File_Copy(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_low.yaml", Path + "/Backup/Main/sfx_low.yaml", false);
                }*/
                if (Voice_Set.WoTB_Path != "" && !Sub_Code.DVPL_File_Exists(Path + "/Backup/Main/reload.bnk"))
                {
                    string[] Languages = { "arb", "cn", "cs", "de", "en", "es", "fi", "fr", "gup", "it", "ja", "ko", "pbr", "pl", "ru", "th", "tr", "vi" };
                    foreach (string Language_Now in Languages)
                    {
                        Directory.CreateDirectory(Path + "/Backup/Main/" + Language_Now);
                        Sub_Code.DVPL_File_Copy(Voice_Set.WoTB_Path + "/Data/WwiseSound/" + Language_Now + "/voiceover_crew.bnk", Path + "/Backup/Main/" + Language_Now + "/voiceover_crew.bnk", false);
                    }
                    Sub_Code.DVPL_File_Copy(Voice_Set.WoTB_Path + "/Data/WwiseSound/reload.bnk", Path + "/Backup/Main/reload.bnk", false);
                    Sub_Code.DVPL_File_Copy(Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_chat_quick_commands.bnk", Path + "/Backup/Main/ui_chat_quick_commands.bnk", false);
                    Sub_Code.DVPL_File_Copy(Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_battle.bnk", Path + "/Backup/Main/ui_battle.bnk", false);
                }
                System.Drawing.Size MaxSize = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size;
                MaxWidth = MaxSize.Width;
                MaxHeight = MaxSize.Height;
                Video_Mode.Width = Width;
                Version_T.Text = "V" + SRTTbacon_Server.Version;
                if (Sub_Code.IsTextIncludeJapanese(Voice_Set.Special_Path))
                {
                    IsIncludeJapanese = true;
                    Message_T.Text = "一時フォルダに日本語が含まれています。Shift+Dで変更してください。";
                    Sub_Code.Error_Log_Write("エラー:一時フォルダに日本語が含まれています。Shift+Dで変更してください。");
                }
                try
                {
                    File.Delete(Voice_Set.Special_Path + "/FSB_Select_Temp_01.fsb");
                    File.Delete(Voice_Set.Special_Path + "/FSB_Select_Temp_02.fsb");
                    File.Delete(Voice_Set.Special_Path + "/FSB_Select_Temp_03.fsb");
                    File.Delete(Voice_Set.Special_Path + "/Wwise/Temp_01.ogg");
                    File.Delete(Voice_Set.Special_Path + "/Wwise/Temp_02.ogg");
                    File.Delete(Voice_Set.Special_Path + "/Wwise/WoT_To_Blitz_Temp.ogg");
                }
                catch (Exception e)
                {
                    Sub_Code.Error_Log_Write(e.Message);
                }
                if (File.Exists(Voice_Set.Special_Path + "/Configs/Main_Configs_Save.conf"))
                {
                    try
                    {
                        Sub_Code.File_Decrypt(Voice_Set.Special_Path + "/Configs/Main_Configs_Save.conf", Voice_Set.Special_Path + "/Configs/Main_Configs_Save.tmp", "SRTTbacon_Main_Config_Save", false);
                        StreamReader str = new StreamReader(Voice_Set.Special_Path + "/Configs/Main_Configs_Save.tmp");
                        IsFullScreen = bool.Parse(str.ReadLine());
                        str.Close();
                        File.Delete(Voice_Set.Special_Path + "/Configs/Main_Configs_Save.tmp");
                        if (!IsFullScreen)
                            Window_Size_Change(false);
                    }
                    catch (Exception e)
                    {
                        Sub_Code.Error_Log_Write(e.Message);
                    }
                }
                Flash.Handle = this;
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                MessageBox.Show("エラーが発生しました。作者にError_Log.txtを送ってください。\nソフトは強制終了されます。");
                Application.Current.Shutdown();
            }
        }
        //アップデートしたとき用
        void Clean()
        {
            if (Environment.CommandLine.IndexOf("/up", StringComparison.CurrentCultureIgnoreCase) != -1)
            {
                try
                {
                    string[] args = Environment.GetCommandLineArgs();
                    int pid = Convert.ToInt32(args[2]);
                    Process.GetProcessById(pid).Kill();
                }
                catch
                {
                }
                try
                {
                    if (File.Exists(Path + "/Update.bat"))
                        File.Delete(Path + "/Update.bat");
                    Directory.Delete(Path + "/Backup/Update", true);
                }
                catch (Exception e)
                {
                    Sub_Code.Error_Log_Write(e.Message);
                }
            }
        }
        void ScreenMove()
        {
            if (!IsFullScreen && !Video_Mode.IsVideoClicked)
                DragMove();
        }
        //ウィンドウのフェードイン
        async void Window_Show()
        {
            Opacity = 0;
            Load_Window_Set();
            while (Opacity < 1 && !IsClosing)
            {
                Opacity += 0.025;
                await Task.Delay(1000 / 60);
            }
            /*Message_T.Text = "BNKファイルを解析しています...";
            await Task.Delay(50);
            string a = Voice_Set.Special_Path + "/Wwise_Parse";
            List<string> BNK_Files = new List<string>();
            BNK_Files.Add(a + "/engine.bnk");
            BNK_Files.Add(a + "/engine_basic.bnk");
            List<string> PCK_Files = new List<string>();
            PCK_Files.Add(a + "/engine.pck");
            PCK_Files.Add(a + "/engine_basic.pck");
            BNK_To_Wwise_Projects BNK_To_Project = new BNK_To_Wwise_Projects(a + "/Init.bnk", BNK_Files, PCK_Files, a + "/SoundbanksInfo.json");
            await BNK_To_Project.Create_Project_All(Voice_Set.Special_Path + "/Back/WoTB_Wwise_Project_Japanese", false, Message_T);
            BNK_To_Project.Clear();
            Message_Feed_Out("完了しました。");*/
        }
        async void Load_Window_Set()
        {
            if (!Voice_Set.FTP_Server.IsConnected)
            {
                return;
            }
            try
            {
                Load_Data_Window.Window_Start("必要なデータをダウンロードしています");
                bool IsOK_00 = true;
                bool IsOK_01 = true;
                bool IsOK_02 = true;
                bool IsOK_03 = true;
                bool IsOK_04 = true;
                bool IsOK_05 = true;
                bool IsOK_06 = true;
                bool IsOK_07 = true;
                bool IsOK_08 = true;
                Download_Data_File.Download_Total_Size = 0;
                Task task = Task.Run(async () =>
                {
                    if (!File.Exists(Voice_Set.Special_Path + "/Loading/148.png"))
                    {
                        string Loading_Path = Voice_Set.Special_Path + "/Loading.dat";
                        IsOK_00 = false;
                        Download_Data_File.Download_File_Path.Add(Loading_Path);
                        Download_Data_File.Download_Total_Size += Voice_Set.FTP_Server.GetFileSize("/WoTB_Voice_Mod/Update/Data/Loading.zip");
                        Task task_02 = Task.Run(() =>
                        {
                            Task task_01 = Task.Run(() =>
                            {
                            //DVPL.DVPL_Unpack_Extract();
                            FluentFTP.FtpClient ftp1 = new FtpClient(SRTTbacon_Server.IP)
                                {
                                    Credentials = new NetworkCredential(SRTTbacon_Server.Name, SRTTbacon_Server.Password),
                                    SocketKeepAlive = false,
                                    DataConnectionType = ConnectType,
                                    SslProtocols = SslProtocols.Tls,
                                    ConnectTimeout = 1000,
                                };
                                ftp1.ValidateCertificate += new FtpSslValidation(OnValidateCertificate);
                                ftp1.Connect();
                                ftp1.DownloadFile(Loading_Path, "/WoTB_Voice_Mod/Update/Data/Loading.zip");
                                ftp1.Disconnect();
                                ftp1.Dispose();
                            });
                            task_01.Wait();
                            ZipFile.ExtractToDirectory(Loading_Path, System.IO.Path.GetDirectoryName(Loading_Path) + "\\" + System.IO.Path.GetFileNameWithoutExtension(Loading_Path));
                            IsOK_00 = true;
                        });
                    }
                    await Task.Delay(10);
                    if (!File.Exists(Voice_Set.Special_Path + "/DVPL/DVPL_Convert.exe"))
                    {
                        IsOK_01 = false;
                        Download_Data_File.Download_File_Path.Add(Voice_Set.Special_Path + "/DVPL.dat");
                        Download_Data_File.Download_Total_Size += Voice_Set.FTP_Server.GetFileSize("/WoTB_Voice_Mod/Update/Data/DVPL.zip");
                        Task task_02 = Task.Run(() =>
                        {
                            Task task_01 = Task.Run(() =>
                            {
                            //DVPL.DVPL_Unpack_Extract();
                            FluentFTP.FtpClient ftp1 = new FtpClient(SRTTbacon_Server.IP)
                                {
                                    Credentials = new NetworkCredential(SRTTbacon_Server.Name, SRTTbacon_Server.Password),
                                    SocketKeepAlive = false,
                                    DataConnectionType = ConnectType,
                                    SslProtocols = SslProtocols.Tls,
                                    ConnectTimeout = 1000,
                                };
                                ftp1.ValidateCertificate += new FtpSslValidation(OnValidateCertificate);
                                ftp1.Connect();
                                ftp1.DownloadFile(Voice_Set.Special_Path + "/DVPL.dat", "/WoTB_Voice_Mod/Update/Data/DVPL.zip");
                                ftp1.Disconnect();
                                ftp1.Dispose();
                            });
                            task_01.Wait();
                            IsOK_01 = true;
                        });
                    }
                    if (!File.Exists(Voice_Set.Special_Path + "/Encode_Mp3/ffmpeg.exe"))
                    {
                        IsOK_02 = false;
                        Download_Data_File.Download_File_Path.Add(Voice_Set.Special_Path + "/Encode_Mp3.dat");
                        Download_Data_File.Download_Total_Size += Voice_Set.FTP_Server.GetFileSize("/WoTB_Voice_Mod/Update/Data/Encode_Mp3.zip");
                        Task task_02 = Task.Run(() =>
                        {
                            Task task_01 = Task.Run(() =>
                            {
                            //DVPL.Encode_Mp3_Extract();
                            FluentFTP.FtpClient ftp1 = new FtpClient(SRTTbacon_Server.IP)
                                {
                                    Credentials = new NetworkCredential(SRTTbacon_Server.Name, SRTTbacon_Server.Password),
                                    SocketKeepAlive = false,
                                    DataConnectionType = ConnectType,
                                    SslProtocols = SslProtocols.Tls,
                                    ConnectTimeout = 1000,
                                };
                                ftp1.ValidateCertificate += new FtpSslValidation(OnValidateCertificate);
                                ftp1.Connect();
                                ftp1.DownloadFile(Voice_Set.Special_Path + "/Encode_Mp3.dat", "/WoTB_Voice_Mod/Update/Data/Encode_Mp3.zip");
                                ftp1.Disconnect();
                                ftp1.Dispose();
                            });
                            task_01.Wait();
                            IsOK_02 = true;
                        });
                    }
                    else if (!File.Exists(Voice_Set.Special_Path + "/Encode_Mp3/lame.exe"))
                    {
                        IsOK_08 = false;
                        Download_Data_File.Download_Total_Size += Voice_Set.FTP_Server.GetFileSize("/WoTB_Voice_Mod/Update/Data/lame.exe");
                        Task task_02 = Task.Run(() =>
                        {
                            Task task_01 = Task.Run(() =>
                            {
                            //DVPL.Encode_Mp3_Extract();
                            FluentFTP.FtpClient ftp1 = new FtpClient(SRTTbacon_Server.IP)
                                {
                                    Credentials = new NetworkCredential(SRTTbacon_Server.Name, SRTTbacon_Server.Password),
                                    SocketKeepAlive = false,
                                    DataConnectionType = ConnectType,
                                    SslProtocols = SslProtocols.Tls,
                                    ConnectTimeout = 1000,
                                };
                                ftp1.ValidateCertificate += new FtpSslValidation(OnValidateCertificate);
                                ftp1.Connect();
                                ftp1.DownloadFile(Voice_Set.Special_Path + "/Encode_Mp3/lame.exe", "/WoTB_Voice_Mod/Update/Data/lame.exe");
                                ftp1.Disconnect();
                                ftp1.Dispose();
                            });
                            task_01.Wait();
                            IsOK_08 = true;
                        });
                    }
                    if (!File.Exists(Voice_Set.Special_Path + "/Fmod_Designer/Fmod_designer.exe"))
                    {
                        IsOK_03 = false;
                        Download_Data_File.Download_File_Path.Add(Voice_Set.Special_Path + "/Fmod_Designer.dat");
                        Download_Data_File.Download_Total_Size += Voice_Set.FTP_Server.GetFileSize("/WoTB_Voice_Mod/Update/Data/Fmod_Designer.zip");
                        Task task_02 = Task.Run(() =>
                        {
                            Task task_01 = Task.Run(() =>
                            {
                            //DVPL.Fmod_Designer_Extract();
                            FluentFTP.FtpClient ftp1 = new FtpClient(SRTTbacon_Server.IP)
                                {
                                    Credentials = new NetworkCredential(SRTTbacon_Server.Name, SRTTbacon_Server.Password),
                                    SocketKeepAlive = false,
                                    DataConnectionType = ConnectType,
                                    SslProtocols = SslProtocols.Tls,
                                    ConnectTimeout = 1000,
                                };
                                ftp1.ValidateCertificate += new FtpSslValidation(OnValidateCertificate);
                                ftp1.Connect();
                                ftp1.DownloadFile(Voice_Set.Special_Path + "/Fmod_Designer.dat", "/WoTB_Voice_Mod/Update/Data/Fmod_Designer.zip");
                                ftp1.Disconnect();
                                ftp1.Dispose();
                            });
                            task_01.Wait();
                            IsOK_03 = true;
                        });
                    }
                    if (!File.Exists(Voice_Set.Special_Path + "/SE/Capture_End_01.wav"))
                    {
                        IsOK_04 = false;
                        Download_Data_File.Download_File_Path.Add(Voice_Set.Special_Path + "/SE.dat");
                        Download_Data_File.Download_Total_Size += Voice_Set.FTP_Server.GetFileSize("/WoTB_Voice_Mod/Update/Data/SE.zip");
                        Task task_02 = Task.Run(() =>
                        {
                            Task task_01 = Task.Run(() =>
                            {
                            //DVPL.SE_Extract();
                            FluentFTP.FtpClient ftp1 = new FtpClient(SRTTbacon_Server.IP)
                                {
                                    Credentials = new NetworkCredential(SRTTbacon_Server.Name, SRTTbacon_Server.Password),
                                    SocketKeepAlive = false,
                                    DataConnectionType = ConnectType,
                                    SslProtocols = SslProtocols.Tls,
                                    ConnectTimeout = 1000,
                                };
                                ftp1.ValidateCertificate += new FtpSslValidation(OnValidateCertificate);
                                ftp1.Connect();
                                ftp1.DownloadFile(Voice_Set.Special_Path + "/SE.dat", "/WoTB_Voice_Mod/Update/Data/SE.zip");
                                ftp1.Disconnect();
                                ftp1.Dispose();
                            });
                            task_01.Wait();
                            IsOK_04 = true;
                        });
                    }
                    if (!File.Exists(Voice_Set.Special_Path + "/Fmod_Android_Create/Fmod_Android_Create.exe"))
                    {
                        IsOK_05 = false;
                        Download_Data_File.Download_File_Path.Add(Voice_Set.Special_Path + "/Fmod_Android_Create.dat");
                        Download_Data_File.Download_Total_Size += Voice_Set.FTP_Server.GetFileSize("/WoTB_Voice_Mod/Update/Data/Fmod_Android_Create.zip");
                        Task task_02 = Task.Run(() =>
                        {
                            Task task_01 = Task.Run(() =>
                            {
                            //DVPL.Fmod_Android_Create_Extract();
                            FluentFTP.FtpClient ftp1 = new FtpClient(SRTTbacon_Server.IP)
                                {
                                    Credentials = new NetworkCredential(SRTTbacon_Server.Name, SRTTbacon_Server.Password),
                                    SocketKeepAlive = false,
                                    DataConnectionType = ConnectType,
                                    SslProtocols = SslProtocols.Tls,
                                    ConnectTimeout = 1000,
                                };
                                ftp1.ValidateCertificate += new FtpSslValidation(OnValidateCertificate);
                                ftp1.Connect();
                                ftp1.DownloadFile(Voice_Set.Special_Path + "/Fmod_Android_Create.dat", "/WoTB_Voice_Mod/Update/Data/Fmod_Android_Create.zip");
                                ftp1.Disconnect();
                                ftp1.Dispose();
                            });
                            task_01.Wait();
                            IsOK_05 = true;
                        });
                    }
                    else
                    {
                        FileInfo f = new FileInfo(Voice_Set.Special_Path + "/Fmod_Android_Create/Fmod_Android_Create.exe");
                        if (f.Length != 1639936)
                        {
                            IsOK_05 = false;
                            Download_Data_File.Download_File_Path.Add(Voice_Set.Special_Path + "/Fmod_Android_Create.dat");
                            Download_Data_File.Download_Total_Size += Voice_Set.FTP_Server.GetFileSize("/WoTB_Voice_Mod/Update/Data/Fmod_Android_Create.zip");
                            Task task_02 = Task.Run(() =>
                            {
                                Task task_01 = Task.Run(() =>
                                {
                                //DVPL.Fmod_Android_Create_Extract();
                                FluentFTP.FtpClient ftp1 = new FtpClient(SRTTbacon_Server.IP)
                                    {
                                        Credentials = new NetworkCredential(SRTTbacon_Server.Name, SRTTbacon_Server.Password),
                                        SocketKeepAlive = false,
                                        DataConnectionType = ConnectType,
                                        SslProtocols = SslProtocols.Tls,
                                        ConnectTimeout = 1000,
                                    };
                                    ftp1.ValidateCertificate += new FtpSslValidation(OnValidateCertificate);
                                    ftp1.Connect();
                                    ftp1.DownloadFile(Voice_Set.Special_Path + "/Fmod_Android_Create.dat", "/WoTB_Voice_Mod/Update/Data/Fmod_Android_Create.zip");
                                    ftp1.Disconnect();
                                    ftp1.Dispose();
                                });
                                task_01.Wait();
                                IsOK_05 = true;
                            });
                        }
                    }
                    if (!File.Exists(Voice_Set.Special_Path + "/Wwise/x64/Release/bin/Wwise.exe"))
                    {
                        IsOK_06 = false;
                        Download_Data_File.Download_File_Path.Add(Voice_Set.Special_Path + "/Wwise.dat");
                        Download_Data_File.Download_Total_Size += Voice_Set.FTP_Server.GetFileSize("/WoTB_Voice_Mod/Update/Data/Wwise.zip");
                        Task task_02 = Task.Run(() =>
                        {
                            Task task_01 = Task.Run(() =>
                            {
                            //DVPL.Wwise_Extract();
                            FluentFTP.FtpClient ftp1 = new FtpClient(SRTTbacon_Server.IP)
                                {
                                    Credentials = new NetworkCredential(SRTTbacon_Server.Name, SRTTbacon_Server.Password),
                                    SocketKeepAlive = false,
                                    DataConnectionType = ConnectType,
                                    SslProtocols = SslProtocols.Tls,
                                    ConnectTimeout = 1000,
                                };
                                ftp1.ValidateCertificate += new FtpSslValidation(OnValidateCertificate);
                                ftp1.Connect();
                                ftp1.DownloadFile(Voice_Set.Special_Path + "/Wwise.dat", "/WoTB_Voice_Mod/Update/Data/Wwise.zip");
                                ftp1.Disconnect();
                                ftp1.Dispose();
                            });
                            task_01.Wait();
                            IsOK_06 = true;
                        });
                    }
                    if (!File.Exists(Voice_Set.Special_Path + "/Wwise_Parse/wwiser.pyz"))
                    {
                        IsOK_07 = false;
                        Download_Data_File.Download_File_Path.Add(Voice_Set.Special_Path + "/Wwise_Parse.dat");
                        Download_Data_File.Download_Total_Size += Voice_Set.FTP_Server.GetFileSize("/WoTB_Voice_Mod/Update/Data/Wwise_Parse.zip");
                        Task task_02 = Task.Run(() =>
                        {
                            Task task_01 = Task.Run(() =>
                            {
                                FluentFTP.FtpClient ftp1 = new FtpClient(SRTTbacon_Server.IP)
                                {
                                    Credentials = new NetworkCredential(SRTTbacon_Server.Name, SRTTbacon_Server.Password),
                                    SocketKeepAlive = false,
                                    DataConnectionType = ConnectType,
                                    SslProtocols = SslProtocols.Tls,
                                    ConnectTimeout = 1000,
                                };
                                ftp1.ValidateCertificate += new FtpSslValidation(OnValidateCertificate);
                                ftp1.Connect();
                                ftp1.DownloadFile(Voice_Set.Special_Path + "/Wwise_Parse.dat", "/WoTB_Voice_Mod/Update/Data/Wwise_Parse.zip");
                                ftp1.Disconnect();
                                ftp1.Dispose();
                            });
                            task_01.Wait();
                            IsOK_07 = true;
                        });
                    }
                });
                await Task.Delay(100);
                while (true)
                {
                    if (IsOK_00 && IsOK_01 && IsOK_02 && IsOK_03 && IsOK_04 && IsOK_05 && IsOK_06 && IsOK_07 && IsOK_08)
                        break;
                    await Task.Delay(100);
                }
                bool IsOK = false;
                Task task_03 = Task.Run(() =>
                {
                    foreach (string Extract_File_Now in Download_Data_File.Download_File_Path)
                    {
                        if (System.IO.Path.GetFileNameWithoutExtension(Extract_File_Now) == "Loading")
                        {
                            File.Delete(Extract_File_Now);
                            continue;
                        }
                        if (Directory.Exists(System.IO.Path.GetDirectoryName(Extract_File_Now) + "\\" + System.IO.Path.GetFileNameWithoutExtension(Extract_File_Now)))
                        {
                            try
                            {
                                Directory.Delete(System.IO.Path.GetDirectoryName(Extract_File_Now) + "\\" + System.IO.Path.GetFileNameWithoutExtension(Extract_File_Now), true);
                            }
                            catch (Exception e)
                            {
                                Sub_Code.Error_Log_Write(e.Message);
                            }
                        }
                        ZipFile.ExtractToDirectory(Extract_File_Now, System.IO.Path.GetDirectoryName(Extract_File_Now) + "\\" + System.IO.Path.GetFileNameWithoutExtension(Extract_File_Now));
                        File.Delete(Extract_File_Now);
                    }
                    IsOK = true;
                });
                while (!IsOK)
                    await Task.Delay(100);
                Download_Data_File.Download_File_Path.Clear();
                Download_Data_File.Download_Total_Size = 0;
                StreamReader str = new StreamReader(Voice_Set.FTP_Server.OpenRead("/WoTB_Voice_Mod/Update/Wwise/Version_01.txt"));
                Sub_Code.IsWwise_Blitz_Update = str.ReadLine();
                Sub_Code.IsWwise_Blitz_Actor_Update = str.ReadLine();
                str.Close();
                StreamReader str3 = new StreamReader(Voice_Set.FTP_Server.OpenRead("/WoTB_Voice_Mod/Update/Wwise/Version_02.txt"));
                Sub_Code.IsWwise_WoT_Update = str3.ReadLine();
                str3.Close();
                try
                {
                    ChangeLog_Window.Text_Change(Server_File.Server_Open_File_Line("/WoTB_Voice_Mod/Update/" + SRTTbacon_Server.Version + "/ChangeLog.txt"));
                }
                catch
                {
                    ChangeLog_Window.Text_Change(new string[] { "このバージョンでは変更履歴を確認できません。" });
                }
                Load_Data_Window.Window_Stop();
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                Message_Feed_Out("エラーが発生しました。");
            }
        }
        void OnValidateCertificate(FtpClient control, FtpSslValidationEventArgs e)
        {
            e.Accept = true;
        }
        //全てのサーバーの情報を取得
        void Connect_Start()
        {
            //Server_List_Reset();
            //コマンドをリアルタイムで通信
            Voice_Set.TCP_Server.DataReceived += (sender, msg) =>
            {
                try
                {
                    if (Voice_Set.SRTTbacon_Server_Name != "" || Voice_Set.TCP_Server.TcpClient.Connected)
                    {
                        string[] Message_Temp = msg.MessageString.Split('|');
                        if (Message_Temp[0] == Voice_Set.SRTTbacon_Server_Name)
                        {
                            if (Chat_Mode == 1 && Message_Temp[1] == "Chat")
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    Chat_T_Change("/WoTB_Voice_Mod/" + Voice_Set.SRTTbacon_Server_Name + "/Chat.dat");
                                    //Chat_T.Text = Server_File.Server_Open_File("/WoTB_Voice_Mod/" + Voice_Set.SRTTbacon_Server_Name + "/Chat.dat");
                                    Chat_Scrool.ScrollToEnd();
                                });
                            }
                            else if (Message_Temp[1] == "Change_Configs")
                            {
                                TCP_Change_Config(Message_Temp);
                            }
                        }
                        else if (Message_Temp[0] == "Public")
                        {
                            if (Chat_Mode == 0 && Message_Temp[1] == "Chat")
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    Chat_T_Change("/WoTB_Voice_Mod/Chat.dat");
                                    //Chat_T.Text = Server_File.Server_Open_File("/WoTB_Voice_Mod/Chat.dat");
                                    Chat_Scrool.ScrollToEnd();
                                });
                            }
                        }
                        else if (Message_Temp[0] == Voice_Set.UserName + "_Private")
                        {
                            if (Chat_Mode == 2 && Message_Temp[1] == "Chat")
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    Chat_T_Change("/WoTB_Voice_Mod/Users/" + Voice_Set.UserName + "_Chat.dat");
                                    //Chat_T.Text = Server_File.Server_Open_File("/WoTB_Voice_Mod/Users/" + Voice_Set.UserName + "_Chat.dat");
                                    Chat_Scrool.ScrollToEnd();
                                });
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Sub_Code.Error_Log_Write(e.Message);
                }
            };
            IsConnecting();
        }
        //キャッシュを削除
        private void Cache_Delete_B_Click(object sender, RoutedEventArgs e)
        {
            string Message_01 = "現在の設定を削除します。この操作は取り消せません。よろしいですか？";
            MessageBoxResult result = MessageBox.Show(Message_01, "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    Directory.Delete(Voice_Set.Special_Path + "/Server", true);
                    Directory.Delete(Voice_Set.Special_Path + "/Configs", true);
                    string[] Dirs = Directory.GetDirectories(Path + "/Backup", "*", SearchOption.TopDirectoryOnly);
                    foreach (string Dir in Dirs)
                    {
                        string Dir_Name_Only = System.IO.Path.GetFileName(Dir);
                        if (Dir_Name_Only == "Main")
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
                    Directory.CreateDirectory(Voice_Set.Special_Path + "/Server");
                    MessageBox.Show("各画面の設定を削除しました。");
                }
                catch (Exception m)
                {
                    MessageBox.Show("設定を削除できませんでした。ファイルが使用中でないか確認してください。\nエラー:" + m.Message);
                    Sub_Code.Error_Log_Write(m.Message);
                }
            }
        }
        //ログイン
        private void User_Login_B_Click(object sender, RoutedEventArgs e)
        {
            if (Server_OK)
            {
                if (Account_Exist(User_Name_T.Text, User_Password_T.Text))
                {
                    StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Temp_User_Set.dat");
                    stw.WriteLine(User_Name_T.Text + ":" + User_Password_T.Text);
                    stw.Close();
                    Sub_Code.File_Encrypt(Voice_Set.Special_Path + "/Temp_User_Set.dat", Path + "/User.dat", "SRTTbacon_Server_User_Pass_Save", true);
                    if (Login())
                    {
                        Connectiong = true;
                        Connect_Start();
                        Connect_Mode_Layout();
                        Chat_Mode_Change(2);
                        Message_Feed_Out("ログインしました。ご利用ありがとうございます！！！");
                    }
                }
                else
                    MessageBox.Show("ユーザー名またはパスワードが間違えています。");
            }
            else
                MessageBox.Show("サーバーに接続されませんでした。時間を空けて再接続ボタンを押してみてください。");
        }
        //新規登録
        private void User_Register_B_Click(object sender, RoutedEventArgs e)
        {
            if (Server_OK)
            {
                if (User_Name_T.Text.Length >= 21)
                {
                    MessageBox.Show("ユーザー名が長すぎます。20文字以下にしてください。");
                    return;
                }
                if (User_Name_T.Text.Contains("\n"))
                {
                    MessageBox.Show("ユーザー名に改行は使用できません。");
                    return;
                }
                if (User_Name_T.Text.CountOf("  ") >= 1)
                {
                    MessageBox.Show("ユーザー名に空白を2つ連続で付けることはできません。");
                    return;
                }
                if (User_Name_T.Text == "")
                {
                    MessageBox.Show("ユーザー名を空白にすることはできません。");
                    return;
                }
                if (UserExist(User_Name_T.Text))
                {
                    MessageBox.Show("そのユーザー名は既に登録されています。別のユーザー名でお試しください。");
                    return;
                }
                StreamWriter Chat = File.CreateText(Voice_Set.Special_Path + "/Temp_User_Chat.dat");
                Chat.WriteLine("---ここで管理者と1:1でチャットできます。---");
                Chat.Close();
                Voice_Set.FTP_Server.UploadFile(Voice_Set.Special_Path + "/Temp_User_Chat.dat", "/WoTB_Voice_Mod/Users/" + User_Name_T.Text + "_Chat.dat");
                Voice_Set.AppendString("/WoTB_Voice_Mod/Accounts.dat", Encoding.UTF8.GetBytes(User_Name_T.Text + ":" + User_Password_T.Text + "\n"));
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Temp_User_Set.dat");
                stw.WriteLine(User_Name_T.Text + ":" + User_Password_T.Text);
                stw.Close();
                Sub_Code.File_Encrypt(Voice_Set.Special_Path + "/Temp_User_Set.dat", Path + "/User.dat", "SRTTbacon_Server_User_Pass_Save", true);
                File.Delete(Voice_Set.Special_Path + "/Temp_User_Chat.dat");
                if (Login())
                {
                    Connectiong = true;
                    Connect_Start();
                    Connect_Mode_Layout();
                    Message_Feed_Out("アカウントを登録しました。ご利用ありがとうございます！！！");
                }
            }
            else
            {
                MessageBox.Show("サーバーに接続されませんでした。時間を空けて再接続ボタンを押してみてください。");
            }
        }
        private void Chat_Send_B_Click(object sender, RoutedEventArgs e)
        {
            if (Chat_Send_T.Text == "" || Chat_Send_T.Text == " " || Chat_Send_T.Text == "　")
            {
                return;
            }
            //現在のバージョンでは使用できません。
            if (Chat_Mode == 1)
            {
                return;
            }
            if (Chat_Send_T.Text.CountOf("  ") >= 1 || Chat_Send_T.Text.CountOf("　　") >= 1)
            {
                MessageBox.Show("スパム防止のため空白を2回連続で使用できません。");
                return;
            }
            if (Chat_Send_T.Text.Length >= 100)
            {
                MessageBox.Show("文字数が多すぎます。100文字以下にしてください。");
                return;
            }
            if (Chat_Mode == 0)
            {
                Voice_Set.AppendString("/WoTB_Voice_Mod/Chat.dat", Encoding.UTF8.GetBytes(Voice_Set.UserName + ":" + Chat_Send_T.Text + "\n"));
                Voice_Set.TCP_Server.WriteLine("Public|Chat|" + Voice_Set.UserName + ":" + Chat_Send_T.Text + '\0');
            }
            else if (Chat_Mode == 1)
            {
                /*Voice_Set.AppendString("/WoTB_Voice_Mod/" + Voice_Set.SRTTbacon_Server_Name + "/Chat.dat", Encoding.UTF8.GetBytes(Voice_Set.UserName + ":" + Chat_Send_T.Text + "\n"));
                Voice_Set.TCP_Server.WriteLine(Voice_Set.SRTTbacon_Server_Name + "|Chat|" + Voice_Set.UserName + ":" + Chat_Send_T.Text + '\0');*/
            }
            else if (Chat_Mode == 2)
            {
                Voice_Set.AppendString("/WoTB_Voice_Mod/Users/" + Voice_Set.UserName + "_Chat.dat", Encoding.UTF8.GetBytes(Voice_Set.UserName + ":" + Chat_Send_T.Text + "\n"));
                Voice_Set.TCP_Server.WriteLine(Voice_Set.UserName + "_Private|Chat|" + Voice_Set.UserName + ":" + Chat_Send_T.Text + '\0');
            }
            Chat_Send_T.Text = "";
        }
        private void Chat_Mode_Public_B_Click(object sender, RoutedEventArgs e)
        {
            Chat_Mode_Change(0);
        }
        private void Chat_Mode_Server_B_Click(object sender, RoutedEventArgs e)
        {
            Chat_Mode_Change(1);
        }
        private void Chat_Mode_Private_B_Click(object sender, RoutedEventArgs e)
        {
            Chat_Mode_Change(2);
        }
        void Chat_Mode_Change(int Mode_Number)
        {
            if (Voice_Set.UserName != "")
            {
                if (Mode_Number == 0)
                {
                    Chat_Mode = 0;
                    Chat_Mode_Public_B.Background = (Brush)bc.ConvertFrom("#59999999");
                    Chat_Mode_Server_B.Background = Brushes.Transparent;
                    Chat_Mode_Private_B.Background = Brushes.Transparent;
                    Chat_Mode_Public_B.BorderBrush = Brushes.Red;
                    Chat_Mode_Server_B.BorderBrush = Brushes.Transparent;
                    Chat_Mode_Private_B.BorderBrush = Brushes.Transparent;
                    Chat_T.Document.Blocks.Clear();
                    Chat_T_Change("/WoTB_Voice_Mod/Chat.dat");
                    //Chat_T.Text = Server_File.Server_Open_File("/WoTB_Voice_Mod/Chat.dat");
                }
                else if (Mode_Number == 1)
                {
                    Chat_Mode = 1;
                    Chat_Mode_Public_B.Background = Brushes.Transparent;
                    Chat_Mode_Server_B.Background = (Brush)bc.ConvertFrom("#59999999");
                    Chat_Mode_Private_B.Background = Brushes.Transparent;
                    Chat_Mode_Public_B.BorderBrush = Brushes.Transparent;
                    Chat_Mode_Server_B.BorderBrush = Brushes.Red;
                    Chat_Mode_Private_B.BorderBrush = Brushes.Transparent;
                    Chat_T.Document.Blocks.Clear();
                    Run myRun = new Run("---現在のバージョンでは使用できません。---");
                    Paragraph myParagraph = new Paragraph();
                    myParagraph.Background = Brushes.Transparent;
                    myParagraph.Foreground = Brushes.White;
                    myParagraph.Inlines.Add(myRun);
                    Chat_T.Document.Blocks.Add(myParagraph);
                    //Chat_T.Text = Server_Open_File("/WoTB_Voice_Mod/" + Voice_Set.SRTTbacon_Server_Name + "/Chat.dat");
                    //Chat_T.Text = "---現在のバージョンでは使用できません。---";
                }
                else if (Mode_Number == 2)
                {
                    Chat_Mode = 2;
                    Chat_Mode_Public_B.Background = Brushes.Transparent;
                    Chat_Mode_Server_B.Background = Brushes.Transparent;
                    Chat_Mode_Private_B.Background = (Brush)bc.ConvertFrom("#59999999");
                    Chat_Mode_Public_B.BorderBrush = Brushes.Transparent;
                    Chat_Mode_Server_B.BorderBrush = Brushes.Transparent;
                    Chat_Mode_Private_B.BorderBrush = Brushes.Red;
                    Chat_T_Change("/WoTB_Voice_Mod/Users/" + Voice_Set.UserName + "_Chat.dat");
                    //Chat_T.Text = Server_File.Server_Open_File("/WoTB_Voice_Mod/Users/" + Voice_Set.UserName + "_Chat.dat");
                }
                Chat_Scrool.ScrollToEnd();
            }
        }
        //チャットを更新
        void Chat_T_Change(string Chat_Server)
        {
            Chat_T.Document.Blocks.Clear();
            int Number = 0;
            foreach (string Text in Server_File.Server_Open_File_Line(Chat_Server))
            {
                Run myRun = new Run(Text);
                Paragraph myParagraph = new Paragraph();
                if (Number % 2 == 0)
                    myParagraph.Background = Brushes.Transparent;
                else
                    myParagraph.Background = (Brush)bc.ConvertFrom("#55919191");
                myParagraph.Inlines.Add(myRun);
                Chat_T.Document.Blocks.Add(myParagraph);
                Number++;
            }
        }
        async Task Loading_Show()
        {
            Load_Image.Visibility = Visibility.Visible;
            int File_Number = 1;
            Random r = new Random();
            int Max_Number = r.Next(100, 149);
            while (File_Number <= Max_Number)
            {
                Load_Image.Source = new BitmapImage(new Uri(Voice_Set.Special_Path + "/Loading/" + File_Number + ".png"));
                File_Number++;
                await Task.Delay(1000 / 30);
            }
            Load_Image.Visibility = Visibility.Hidden;
        }
        private void Voice_Mod_Free_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing || NotConnectedLoginMessage())
                return;
            Voice_Mods_Window.Window_Show();
        }
        private void Tool_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing || NotConnectedLoginMessage())
                return;
            if (IsIncludeJapanese)
            {
                if (Message_T.Text == "")
                    Message_Feed_Out("一時フォルダのパスに日本語が含まれているため表示できません。");
                return;
            }
            Tools_Window.Window_Show();
        }
        private async void Update_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing)
                return;
            try
            {
                StreamReader str = new StreamReader(Voice_Set.FTP_Server.OpenRead("/WoTB_Voice_Mod/Update/Configs.dat"));
                string Line = str.ReadLine();
                str.Close();
                if (Line == SRTTbacon_Server.Version)
                    Message_Feed_Out("既に最新のバージョンです。");
                else
                {
                    MessageBoxResult result = MessageBox.Show("新しいバージョンが見つかりました(V" + Line + ")。ダウンロードして適応しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes);
                    if (result == MessageBoxResult.Yes)
                    {
                        Voice_Set.App_Busy = true;
                        Download_P.Visibility = Visibility.Visible;
                        Download_T.Visibility = Visibility.Visible;
                        Download_Border.Visibility = Visibility.Visible;
                        Message_T.Text = "ダウンロード中です。処理が完了したらソフトを再起動します...";
                        await Task.Delay(50);
                        if (Directory.Exists(Path + "/Backup/Update"))
                            Directory.Delete(Path + "/Backup/Update", true);
                        Directory.CreateDirectory(Path + "/Backup/Update");
                        Directory.CreateDirectory(Voice_Set.Special_Path + "/Update");
                        IsProcessing = true;
                        List<string> strList = new List<string>();
                        foreach (FtpListItem item in Voice_Set.FTP_Server.GetListing("/WoTB_Voice_Mod/Update/" + Line))
                            strList.Add(item.Name);
                        foreach (string File_Name in strList)
                        {
                            try
                            {
                                long File_Size_Full = Voice_Set.FTP_Server.GetFileSize("/WoTB_Voice_Mod/Update/" + Line + "/" + File_Name);
                                Task task = Task.Run(() =>
                                {
                                    Voice_Set.FTP_Server.DownloadFile(Voice_Set.Special_Path + "/Update/" + File_Name, "/WoTB_Voice_Mod/Update/" + Line + "/" + File_Name);
                                });
                                while (true)
                                {
                                    long File_Size_Now = 0;
                                    if (File.Exists(Voice_Set.Special_Path + "/Update/" + File_Name))
                                    {
                                        FileInfo fi = new FileInfo(Voice_Set.Special_Path + "/Update/" + File_Name);
                                        File_Size_Now = fi.Length;
                                    }
                                    double Download_Percent = (double)File_Size_Now / File_Size_Full * 100;
                                    int Percent_INT = (int)Math.Round(Download_Percent, MidpointRounding.AwayFromZero);
                                    Download_P.Value = Percent_INT;
                                    Download_T.Text = "進捗:" + Percent_INT + "%";
                                    if (File_Size_Now >= File_Size_Full)
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
                            }
                        }
                        //Voice_Set.FTP_Server.DownloadDirectory(Voice_Set.Special_Path + "/Update", "/WoTB_Voice_Mod/Update/" + Line, FtpFolderSyncMode.Update, FtpLocalExists.Overwrite);
                        bool IsReboot = false;
                        string[] Dir_Update = Directory.GetFiles(Voice_Set.Special_Path + "/Update", "*", SearchOption.AllDirectories);
                        foreach (string File_Now in Dir_Update)
                        {
                            if (System.IO.Path.GetFileName(File_Now) == "WoTB_Voice_Mod_Creater.exe")
                                IsReboot = true;
                            string Dir_Only = File_Now.Replace(System.IO.Path.GetFileName(File_Now), "");
                            string Temp_01 = Dir_Only.Replace("/", "\\");
                            string File_Dir = Temp_01.Replace(Voice_Set.Special_Path.Replace("/", "\\") + "\\Update\\", "");
                            if (File_Dir.Contains("\\"))
                            {
                                if (!Directory.Exists(Path + "/" + File_Dir))
                                    Directory.CreateDirectory(Path + "/" + File_Dir);
                            }
                            if (File.Exists(Path + "/" + File_Dir + System.IO.Path.GetFileName(File_Now)))
                            {
                                if (File_Dir.Contains("\\"))
                                {
                                    if (!Directory.Exists(Path + "/Backup/Update/" + File_Dir))
                                        Directory.CreateDirectory(Path + "/Backup/Update/" + File_Dir);
                                    File.Move(Path + "/" + File_Dir + System.IO.Path.GetFileName(File_Now), Path + "/Backup/Update/" + System.IO.Path.GetFileName(File_Now));
                                }
                                else
                                    File.Move(Path + "/" + System.IO.Path.GetFileName(File_Now), Path + "/Backup/Update/" + System.IO.Path.GetFileName(File_Now));
                            }
                            File.Copy(File_Now, Path + "/" + File_Dir + System.IO.Path.GetFileName(File_Now), true);
                        }
                        Directory.Delete(Voice_Set.Special_Path + "/Update", true);
                        if (IsReboot)
                        {
                            StreamWriter stw = File.CreateText(Path + "/Update.bat");
                            stw.WriteLine("timeout 3");
                            stw.Write("\"" + Path + "/WoTB_Voice_Mod_Creater.exe\" /up " + Process.GetCurrentProcess().Id);
                            stw.Close();
                            ProcessStartInfo processStartInfo1 = new ProcessStartInfo
                            {
                                FileName = Path + "/Update.bat",
                                CreateNoWindow = true,
                                UseShellExecute = false
                            };
                            Process p = Process.Start(processStartInfo1);
                            Application.Current.Shutdown();
                        }
                        Download_P.Visibility = Visibility.Hidden;
                        Download_T.Visibility = Visibility.Hidden;
                        Download_Border.Visibility = Visibility.Hidden;
                        Voice_Set.App_Busy = false;
                        IsProcessing = false;
                        Message_Feed_Out("アップデートが完了しました。");
                    }
                }
            }
            catch (Exception e1)
            {
                Download_P.Visibility = Visibility.Hidden;
                Download_T.Visibility = Visibility.Hidden;
                Download_Border.Visibility = Visibility.Hidden;
                Voice_Set.App_Busy = false;
                IsProcessing = false;
                Message_Feed_Out("最新バージョンを取得できませんでした。");
                Sub_Code.Error_Log_Write(e1.Message);
            }
        }
        private void Other_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing || NotConnectedLoginMessage())
                return;
            Other_Window.Window_Show();
        }
        private void Message_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing || NotConnectedLoginMessage())
                return;
            Message_Window.Window_Show();
        }
        private void Voice_Create_Tool_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing || NotConnectedLoginMessage())
                return;
            Voice_Create_Window.Window_Show(false);
        }
        private void WoTB_Select_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing)
                return;
            BetterFolderBrowser ofd = new BetterFolderBrowser()
            {
                Title = "WoTBのインストール先を選択してください。",
                RootFolder = Sub_Code.Get_OpenDirectory_Path(),
                Multiselect = false
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Sub_Code.Set_Directory_Path(ofd.SelectedFolder);
                if (!ofd.SelectedFolder.Contains("\\steamapps\\") && !ofd.SelectedFolder.Contains("\\Steam\\"))
                {
                    Message_Feed_Out("steamappsフォルダ以降の階層のフォルダを選択する必要があります。");
                }
                ApplyAllFiles(ofd.SelectedFolder, ProcessFile);
                if (Voice_Set.WoTB_Path == "")
                {
                    Message_Feed_Out("WoTBのフォルダを取得できませんでした。");
                }
            }
        }
        void ProcessFile(string path)
        {
            string Dir = System.IO.Path.GetDirectoryName(path);
            if (Sub_Code.File_Exists(Dir + "/sounds"))
            {
                string WoTB_Path = Dir.Substring(0, Dir.LastIndexOf('\\'));
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Temp_WoTB_Path.dat");
                stw.Write(WoTB_Path);
                stw.Close();
                Sub_Code.File_Encrypt(Voice_Set.Special_Path + "/Temp_WoTB_Path.dat", Path + "/WoTB_Path.dat", "WoTB_Directory_Path_Pass", true);
                Voice_Set.WoTB_Path = WoTB_Path;
                Message_Feed_Out("WoTBのフォルダを取得しました。");
                WoTB_Select_B.Visibility = Visibility.Hidden;
                return;
            }
        }
        void ApplyAllFiles(string folder, Action<string> fileAction)
        {
            foreach (string file in Directory.GetFiles(folder, "sounds.*"))
            {
                if (file.Contains("World of Tanks Blitz"))
                {
                    fileAction(file);
                    return;
                }
            }
            foreach (string subDir in Directory.GetDirectories(folder))
            {
                try
                {
                    ApplyAllFiles(subDir, fileAction);
                }
                catch (Exception e)
                {
                    Sub_Code.Error_Log_Write(e.Message);
                }
            }
        }
        async void Message_Feed_Out(string Message)
        {
            if (IsMessageShowing)
            {
                IsMessageShowing = false;
                await Task.Delay(1000 / 59);
            }
            Message_T.Text = Message;
            IsMessageShowing = true;
            Message_T.Opacity = 1;
            int Number = 0;
            while (Message_T.Opacity > 0 && IsMessageShowing)
            {
                Number++;
                if (Number >= 120)
                {
                    Message_T.Opacity -= 0.025;
                }
                await Task.Delay(1000 / 60);
            }
            IsMessageShowing = false;
            Message_T.Text = "";
            Message_T.Opacity = 1;
        }
        void Main_Config_Save()
        {
            try
            {
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Configs/Main_Configs_Save.tmp");
                stw.Write(IsFullScreen);
                stw.Close();
                Sub_Code.File_Encrypt(Voice_Set.Special_Path + "/Configs/Main_Configs_Save.tmp", Voice_Set.Special_Path + "/Configs/Main_Configs_Save.conf", "SRTTbacon_Main_Config_Save", true);
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        void Window_Size_Change(bool IsChangeSize)
        {
            try
            {
                if (IsChangeSize)
                    IsFullScreen = !IsFullScreen;
                System.Drawing.Size MaxSize = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size;
                if (!IsFullScreen)
                {
                    Width = ((double)MaxSize.Width / 1.25);
                    Height = ((double)MaxSize.Height / 1.25);
                    Left = (MaxSize.Width - Width) / 2;
                    Top = (MaxSize.Height - Height) / 2;
                }
                else
                {
                    Width = MaxSize.Width;
                    Height = MaxSize.Height;
                    Left = 0;
                    Top = 0;
                }
                Video_Mode.Width = Width;
            }
            catch (Exception e1)
            {
                Sub_Code.Error_Log_Write(e1.Message);
                Message_Feed_Out("画面サイズを変更できませんでした。");
            }
        }
        private async void DockPanel_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (IsProcessing || Voice_Set.UserName == "")
                return;
            if (Other_Window.Visibility == Visibility.Visible)
                Other_Window.RootWindow_KeyDown(e);
            //ファイル名を入力中にShift+Fが働いてしまうと困るので設定
            if (Sound_Editor_Window.Setting_Window.Visibility == Visibility)
                return;
            if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Shift) == System.Windows.Forms.Keys.Shift && e.Key == System.Windows.Input.Key.F)
            {
                Window_Size_Change(true);
                Main_Config_Save();
            }
            if (Save_Window.Visibility == Visibility.Visible || Voice_Mods_Window.Visibility == Visibility.Visible || Tools_Window.Visibility == Visibility ||
                Other_Window.Visibility == Visibility.Visible || Voice_Create_Window.Visibility == Visibility.Visible || Message_Window.Visibility == Visibility.Visible || Load_Data_Window.Visibility == Visibility.Visible ||
                Tools_V2_Window.Visibility == Visibility.Visible || Change_To_Wwise_Window.Visibility == Visibility.Visible || WoT_To_Blitz_Window.Visibility == Visibility.Visible ||
                Blitz_To_WoT_Window.Visibility == Visibility.Visible || Bank_Editor_Window.Visibility == Visibility.Visible || Create_Save_File_Window.Visibility == Visibility.Visible ||
                Create_Loading_BGM_Window.Visibility == Visibility.Visible || BNK_Event_Window.Visibility == Visibility.Visible || BNK_To_Project_Window.Visibility == Visibility.Visible ||
                Sound_Editor_Window.Visibility == Visibility.Visible)
            {
                return;
            }
            //アンインストール
            if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Shift) == System.Windows.Forms.Keys.Shift && e.Key == System.Windows.Input.Key.Escape)
            {
                if (IsClosing || Voice_Set.App_Busy)
                    return;
                IsClosing = true;
                MessageBoxResult result = MessageBox.Show("ソフトをアンインストールしますか？これには一時ファイルも削除されます。\n注意:この操作は取り消しできません。", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
                if (result == MessageBoxResult.Yes)
                {
                    Voice_Set.App_Busy = true;
                    Other_Window.Pause_Volume_Animation(true, 25);
                    Message_T.Text = "一時ファイルを削除しています...";
                    await Task.Delay(50);
                    try
                    {
                        Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/Configs");
                        Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/DVPL");
                        Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/Encode_Mp3");
                        Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/Fmod_Android_Create");
                        Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/Loading");
                        Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/SE");
                        Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/Server");
                        Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/Wwise");
                        Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/Wwise_Parse");
                        Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/Temp");
                    }
                    catch (Exception e1)
                    {
                        Sub_Code.Error_Log_Write(e1.Message);
                    }
                    try
                    {
                        if (Voice_Set.FTP_Server.IsConnected)
                        {
                            Voice_Set.FTP_Server.Disconnect();
                            Voice_Set.TCP_Server.Disconnect();
                            Voice_Set.FTP_Server.Dispose();
                            Voice_Set.TCP_Server.Dispose();
                        }
                    }
                    catch (Exception e1)
                    {
                        Sub_Code.Error_Log_Write(e1.Message);
                    }
                    Other_Window.Pause_Volume_Animation(true, 25);
                    while (Opacity > 0)
                    {
                        Opacity -= Sub_Code.Window_Feed_Time;
                        await Task.Delay(1000 / 60);
                    }
                    StreamWriter stw = File.CreateText(System.IO.Path.GetTempPath() + "/WoTB_Voice_Mod_Creater_Remove.bat");
                    stw.WriteLine("timeout 2");
                    stw.WriteLine("del " + Path + "/WoTB_Voice_Mod_Creater.exe");
                    stw.WriteLine("del " + Path + "/ChangeLog.txt");
                    stw.WriteLine("del " + Path + "/Error_Log.txt");
                    stw.WriteLine("del " + Path + "/TempDirPath.dat");
                    stw.WriteLine("del " + Path + "/User.dat");
                    stw.WriteLine("del " + Path + "/WoTB_Path.dat");
                    stw.WriteLine("rd /s /q " + Path + "/dll");
                    stw.WriteLine("rd /s /q " + Path + "/Backup");
                    stw.WriteLine("rd /s /q " + Path + "/Projects");
                    stw.WriteLine("rd /s /q " + Path + "/Youtube");
                    stw.Close();
                    ProcessStartInfo processStartInfo = new ProcessStartInfo
                    {
                        FileName = System.IO.Path.GetTempPath() + "/WoTB_Voice_Mod_Creater_Remove.bat",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    Process.Start(processStartInfo);
                    Application.Current.Shutdown();
                }
            }
            //一時ファイルの保存場所を指定
            if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Shift) == System.Windows.Forms.Keys.Shift && e.Key == System.Windows.Input.Key.D)
            {
                MessageBoxResult result = MessageBox.Show("一時ファイルの保存先を変更しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
                if (result == MessageBoxResult.Yes)
                {
                    BetterFolderBrowser ofd = new BetterFolderBrowser()
                    {
                        Title = "保存先のフォルダを選択してください。",
                        RootFolder = Sub_Code.Get_OpenDirectory_Path(),
                        Multiselect = false
                    };
                    if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        Sub_Code.Set_Directory_Path(ofd.SelectedFolder);
                        string Dir = ofd.SelectedFolder;
                        if (!Sub_Code.CanDirectoryAccess(Dir))
                        {
                            Message_Feed_Out("指定したフォルダにアクセスできませんでした。");
                            return;
                        }
                        if (Sub_Code.IsTextIncludeJapanese(Dir))
                        {
                            Message_Feed_Out("エラー:パスに日本語を含むことはできません。");
                            return;
                        }
                        if (Directory.GetFiles(Dir, "*", SearchOption.AllDirectories).Length > 0)
                        {
                            MessageBox.Show("フォルダ内は空である必要があります。");
                            return;
                        }
                        Message_T.Text = "一時フォルダの場所を変更しています...";
                        await Task.Delay(50);
                        Sub_Code.Directory_Copy(Voice_Set.Special_Path + "/Configs", Dir);
                        Sub_Code.Directory_Copy(Voice_Set.Special_Path + "/DVPL", Dir);
                        Sub_Code.Directory_Copy(Voice_Set.Special_Path + "/Encode_Mp3", Dir);
                        Sub_Code.Directory_Copy(Voice_Set.Special_Path + "/Fmod_Android_Create", Dir);
                        Sub_Code.Directory_Copy(Voice_Set.Special_Path + "/Fmod_Designer", Dir);
                        Sub_Code.Directory_Copy(Voice_Set.Special_Path + "/Loading", Dir);
                        Sub_Code.Directory_Copy(Voice_Set.Special_Path + "/SE", Dir);
                        Sub_Code.Directory_Copy(Voice_Set.Special_Path + "/Wwise", Dir);
                        Sub_Code.Directory_Copy(Voice_Set.Special_Path + "/Wwise_Parse", Dir);
                        Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/Configs");
                        Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/DVPL");
                        Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/Encode_Mp3");
                        Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/Fmod_Android_Create");
                        Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/Fmod_Designer");
                        Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/Loading");
                        Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/SE");
                        Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/Wwise");
                        Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/Wwise_Parse");
                        StreamWriter stw = File.CreateText(Dir + "/TempDirPath.dat");
                        stw.Write(Dir);
                        stw.Close();
                        Sub_Code.File_Encrypt(Dir + "/TempDirPath.dat", Path + "/TempDirPath.dat", "Temp_Directory_Path_Pass", true);
                        Voice_Set.Special_Path = Dir;
                        Message_Feed_Out("一時ファイルの保存先を変更しました。");
                    }
                }
            }
            //エラーログをクリア
            if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Shift) == System.Windows.Forms.Keys.Shift && e.Key == System.Windows.Input.Key.L)
            {
                try
                {
                    if (!File.Exists(Path + "/Error_Log.txt"))
                    {
                        Message_Feed_Out("ログファイルが存在しません。");
                    }
                    StreamReader str = new StreamReader(Path + "/Error_Log.txt");
                    string Temp = str.ReadToEnd();
                    str.Close();
                    if (Temp == "")
                    {
                        Message_Feed_Out("ログはすでにクリアされています。");
                    }
                    MessageBoxResult result = MessageBox.Show("エラーログをクリアしますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
                    if (result == MessageBoxResult.Yes)
                    {
                        File.WriteAllText(Path + "/Error_Log.txt", "");
                    }
                }
                catch (Exception e1)
                {
                    Message_Feed_Out("エラーログをクリアできませんでした。");
                    Sub_Code.Error_Log_Write(e1.Message);
                }
            }
            //一時フォルダの位置を確認
            if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Shift) == System.Windows.Forms.Keys.Shift && e.Key == System.Windows.Input.Key.P)
            {
                MessageBox.Show("一時フォルダ場所:" + Voice_Set.Special_Path);
            }
            //超上級者向け
            if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Shift) == System.Windows.Forms.Keys.Shift && e.Key == System.Windows.Input.Key.E)
            {
                if (IsProcessing || NotConnectedLoginMessage())
                    return;
                BNK_Event_Window.Window_Show();
            }
            //非公開のコマンドたち
            if (SRTTbacon_Server.IsSRTTbaconOwnerMode)
            {
                //.psbファイルが入っているフォルダを指定し、中身を.wavに変換
                if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Shift) == System.Windows.Forms.Keys.Shift && e.Key == System.Windows.Input.Key.T)
                {
                    try
                    {
                        BetterFolderBrowser bfd = new BetterFolderBrowser()
                        {
                            Title = "フォルダを選択してください。",
                            RootFolder = Sub_Code.Get_OpenDirectory_Path(),
                            Multiselect = false
                        };
                        if (bfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            Sub_Code.Set_Directory_Path(bfd.SelectedFolder);
                            string[] Files = Directory.GetFiles(bfd.SelectedFolder, "*.psb", SearchOption.TopDirectoryOnly);
                            if (Files.Length == 0)
                            {
                                Message_Feed_Out("指定したフォルダ内に.psbファイルが存在しませんでした。");
                                bfd.Dispose();
                                return;
                            }
                            Message_T.Text = ".psbファイルを変換しています...";
                            await Task.Delay(50);
                            await Multithread.Convert_PSB_To_WAV(Files, true);
                            Message_Feed_Out("変換が完了しました。");
                        }
                        bfd.Dispose();
                    }
                    catch (Exception e1)
                    {
                        Sub_Code.Error_Log_Write(e1.Message);
                        Message_Feed_Out("エラーが発生しました。");
                    }
                }
                if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Shift) == System.Windows.Forms.Keys.Shift && e.Key == System.Windows.Input.Key.B)
                {
                    System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog()
                    {
                        Title = "Init.bnkを選択してください。",
                        Filter = "Init.bnk(*.bnk)|*.bnk",
                        Multiselect = false
                    };
                    if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        Wwise_Class.Master_Audio_Bus Master = new Wwise_Class.Master_Audio_Bus(ofd.FileName);
                        await Master.Get_Hash_Name_To_File(Voice_Set.Special_Path + "/Test.txt", Message_T);
                        Message_Feed_Out("解析が完了しました。");
                    }
                    ofd.Dispose();
                }
            }
            IsClosing = false;
        }
        private async void Voice_Create_V2_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing || NotConnectedLoginMessage())
                return;
            if (!File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Version.dat") && !Voice_Set.FTP_Server.IsConnected)
            {
                Message_Feed_Out("サーバーに接続できないため実行できません。");
                return;
            }
            IsProcessing = true;
            int Tmp = await Sub_Code.Wwise_Project_Update(Message_T, Download_P, Download_T, Download_Border);
            IsProcessing = false;
            if (Tmp == 1)
                Message_Feed_Out("ダウンロードに失敗しました。以前のバージョンで実行します。");
            else if (Tmp == 3)
            {
                Message_Feed_Out("ダウンロードに失敗しました。開発者へご連絡ください。");
                return;
            }
            else if (Tmp == 4)
                return;
            else if (Tmp == 5)
            {
                Message_Feed_Out("エラーが発生しました。Log.txtを参照してください。");
                return;
            }
            try
            {
                StreamReader str = new StreamReader(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Version.dat");
                double Version_Wwise = double.Parse(str.ReadLine());
                str.Close();
                if (Version_Wwise < 1.3)
                {
                    Message_Feed_Out("プロジェクトデータをアップデートしてください。");
                    return;
                }
            }
            catch
            {
                Message_Feed_Out("エラーが発生しました。");
                return;
            }
            Voice_Create_Window.Window_Show(true);
        }
        private void Tool_V2_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing || NotConnectedLoginMessage())
                return;
            Tools_V2_Window.Window_Show();
        }
        private void Advanced_Mode_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing || NotConnectedLoginMessage())
                return;
            Bank_Editor_Window.Window_Show();
        }
        private async void Change_Wwise_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing || NotConnectedLoginMessage())
                return;
            if (!File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Version.dat") && !Voice_Set.FTP_Server.IsConnected)
            {
                Message_Feed_Out("サーバーに接続できないため実行できません。");
                return;
            }
            IsProcessing = true;
            int Tmp = await Sub_Code.Wwise_Project_Update(Message_T, Download_P, Download_T, Download_Border);
            IsProcessing = false;
            if (Tmp == 1)
                Message_Feed_Out("ダウンロードに失敗しました。以前のバージョンで実行します。");
            else if (Tmp == 3)
            {
                Message_Feed_Out("ダウンロードに失敗しました。開発者へご連絡ください。");
                return;
            }
            else if (Tmp == 4)
                return;
            else if (Tmp == 5)
            {
                Message_Feed_Out("エラーが発生しました。Log.txtを参照してください。");
                return;
            }
            try
            {
                StreamReader str = new StreamReader(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Version.dat");
                double Version_Wwise = double.Parse(str.ReadLine());
                str.Close();
                if (Version_Wwise < 1.3)
                {
                    Message_Feed_Out("プロジェクトデータをアップデートしてください。");
                    return;
                }
            }
            catch
            {
                Message_Feed_Out("エラーが発生しました。");
                return;
            }
            if (!IsChange_To_Wwise_Checked)
            {
                Voice_Set.Voice_BGM_Change_List_Init();
                IsChange_To_Wwise_Checked = true;
            }
            Change_To_Wwise_Window.Window_Show();
        }
        private async void Server_B_Click(object sender, RoutedEventArgs e)
        {
            IsProcessing = true;
            Server_Select_Window.Window_Show();
            while (Server_Select_Window.Visibility == Visibility.Visible)
            {
                await Task.Delay(100);
            }
            IsProcessing = false;
            if (Voice_Set.SRTTbacon_Server_Name != "")
            {
                Server_Voices_Sort_Window.Window_Show(false, "サーバーに参加しました。");
            }
        }
        private async void WoT_To_Blitz_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing || NotConnectedLoginMessage())
                return;
            if (!File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Version.dat") && !Voice_Set.FTP_Server.IsConnected)
            {
                Message_Feed_Out("サーバーに接続できないため実行できません。");
                return;
            }
            IsProcessing = true;
            int Tmp = await Sub_Code.Wwise_Project_Update(Message_T, Download_P, Download_T, Download_Border);
            IsProcessing = false;
            if (Tmp == 1)
                Message_Feed_Out("ダウンロードに失敗しました。以前のバージョンで実行します。");
            else if (Tmp == 3)
            {
                Message_Feed_Out("ダウンロードに失敗しました。開発者へご連絡ください。");
                return;
            }
            else if (Tmp == 4)
                return;
            else if (Tmp == 5)
            {
                Message_Feed_Out("エラーが発生しました。Log.txtを参照してください。");
                return;
            }
            try
            {
                StreamReader str = new StreamReader(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Version.dat");
                double Version_Wwise = double.Parse(str.ReadLine());
                str.Close();
                if (Version_Wwise < 1.3)
                {
                    Message_Feed_Out("プロジェクトデータをアップデートしてください。");
                    return;
                }
            }
            catch
            {
                Message_Feed_Out("エラーが発生しました。");
                return;
            }
            WoT_To_Blitz_Window.Window_Show();
        }
        private async void Blitz_To_WoT_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing || NotConnectedLoginMessage())
                return;
            if (!File.Exists(Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod/Version.dat") && !Voice_Set.FTP_Server.IsConnected)
            {
                Message_Feed_Out("サーバーに接続できないため実行できません。");
                return;
            }
            IsProcessing = true;
            int Tmp = await Sub_Code.Wwise_WoT_Project_Update(Message_T, Download_P, Download_T, Download_Border);
            IsProcessing = false;
            if (Tmp == 1)
                Message_Feed_Out("ダウンロードに失敗しました。以前のバージョンで実行します。");
            else if (Tmp == 3)
            {
                Message_Feed_Out("ダウンロードに失敗しました。開発者へご連絡ください。");
                return;
            }
            else if (Tmp == 4)
                return;
            else if (Tmp == 5)
            {
                Message_Feed_Out("エラーが発生しました。Log.txtを参照してください。");
                return;
            }
            Blitz_To_WoT_Window.Window_Show();
        }
        private void Create_Save_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing || NotConnectedLoginMessage())
                return;
            Create_Save_File_Window.Window_Show();
        }
        private async void Loading_BGM_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing || NotConnectedLoginMessage())
                return;
            if (!File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Version.dat") && !Voice_Set.FTP_Server.IsConnected)
            {
                Message_Feed_Out("サーバーに接続できないため実行できません。");
                return;
            }
            IsProcessing = true;
            int Tmp = await Sub_Code.Wwise_Project_Update(Message_T, Download_P, Download_T, Download_Border);
            IsProcessing = false;
            if (Tmp == 1)
                Message_Feed_Out("ダウンロードに失敗しました。以前のバージョンで実行します。");
            else if (Tmp == 3)
            {
                Message_Feed_Out("ダウンロードに失敗しました。開発者へご連絡ください。");
                return;
            }
            else if (Tmp == 4)
                return;
            else if (Tmp == 5)
            {
                Message_Feed_Out("エラーが発生しました。Log.txtを参照してください。");
                return;
            }
            try
            {
                StreamReader str = new StreamReader(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Version.dat");
                double Version_Wwise = double.Parse(str.ReadLine());
                str.Close();
                if (Version_Wwise < 1.3)
                {
                    Message_Feed_Out("プロジェクトデータをアップデートしてください。");
                    return;
                }
            }
            catch
            {
                Message_Feed_Out("エラーが発生しました。");
                return;
            }
            Create_Loading_BGM_Window.Window_Show();
        }
        bool NotConnectedLoginMessage()
        {
            if (!Voice_Set.FTP_Server.IsConnected)
            {
                Message_Feed_Out("サーバーに接続されていないため、この機能は利用できません。");
                return true;
            }
            if (Voice_Set.UserName == "")
            {
                Message_Feed_Out("アカウント登録またはログインしてからお試しください。");
                return true;
            }
            return false;
        }
        private async void BNK_To_Project_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing || NotConnectedLoginMessage())
                return;
            if (!File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Version.dat") && !Voice_Set.FTP_Server.IsConnected)
            {
                Message_Feed_Out("サーバーに接続できないため実行できません。");
                return;
            }
            IsProcessing = true;
            int Tmp = await Sub_Code.Wwise_Project_Update(Message_T, Download_P, Download_T, Download_Border);
            IsProcessing = false;
            if (Tmp == 1)
                Message_Feed_Out("ダウンロードに失敗しました。以前のバージョンで実行します。");
            else if (Tmp == 3)
            {
                Message_Feed_Out("ダウンロードに失敗しました。開発者へご連絡ください。");
                return;
            }
            else if (Tmp == 4)
                return;
            else if (Tmp == 5)
            {
                Message_Feed_Out("エラーが発生しました。Log.txtを参照してください。");
                return;
            }
            try
            {
                StreamReader str = new StreamReader(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Version.dat");
                double Version_Wwise = double.Parse(str.ReadLine());
                str.Close();
                if (Version_Wwise < 1.3)
                {
                    Message_Feed_Out("プロジェクトデータをアップデートしてください。");
                    return;
                }
            }
            catch
            {
                Message_Feed_Out("エラーが発生しました。");
                return;
            }
            if (!IsChange_To_Wwise_Checked)
            {
                Voice_Set.Voice_BGM_Change_List_Init();
                IsChange_To_Wwise_Checked = true;
            }
            BNK_To_Project_Window.Window_Show();
        }
        private void Window_PreviewDragOver(object sender, DragEventArgs e)
        {
            string[] Drag_Files = (string[])e.Data.GetData(DataFormats.FileDrop);
            string Ex = System.IO.Path.GetExtension(Drag_Files[0]);
            if (Ex == ".wvs" || Ex == ".wms")
                e.Effects = DragDropEffects.Copy;
            else if (Other_Window.Visibility == Visibility.Visible || Sound_Editor_Window.Visibility == Visibility.Visible)
            {
                if (Ex == ".mp3" || Ex == ".wav" || Ex == ".ogg" || Ex == ".aiff" || Ex == ".flac" || Ex == ".m4a" || Ex == ".mp4")
                    e.Effects = DragDropEffects.Copy;
                else
                    e.Effects = DragDropEffects.None;
            }
            else
                e.Effects = DragDropEffects.None;
            e.Handled = true;
        }
        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (IsProcessing || NotConnectedLoginMessage())
                return;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                try
                {
                    string[] Drop_Files = e.Data.GetData(DataFormats.FileDrop) as string[];
                    string Ex = System.IO.Path.GetExtension(Drop_Files[0]);
                    if (Other_Window.Visibility == Visibility.Visible || Sound_Editor_Window.Visibility == Visibility.Visible)
                    {
                        if (Ex == ".mp3" || Ex == ".wav" || Ex == ".ogg" || Ex == ".aiff" || Ex == ".flac" || Ex == ".m4a" || Ex == ".mp4")
                        {
                            if (Sound_Editor_Window.Visibility == Visibility.Visible)
                                Sound_Editor_Window.Add_Sound_File(Drop_Files);
                            else
                                Other_Window.Add_Music_From_Drop(Drop_Files);
                        }
                        else
                            Message_Feed_Out("対応したファイルをドラッグしてください。");
                    }
                    else if (Ex == ".wvs")
                    {
                        Voice_Create_Window.Window_Show(true);
                        Voice_Create_Window.Voice_Load_From_File(Drop_Files[0]);
                    }
                    else if (Ex == ".wms")
                    {
                        Create_Loading_BGM_Window.Window_Show();
                        Create_Loading_BGM_Window.Load_From_File(Drop_Files[0]);
                    }
                    else
                        Message_Feed_Out("対応したファイルをドラッグしてください。");
                }
                catch (Exception e1)
                {
                    Message_Feed_Out("ファイルを読み込めませんでした。");
                    Sub_Code.Error_Log_Write(e1.Message);
                }
            }
        }
        private void Change_Log_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing || NotConnectedLoginMessage())
                return;
            ChangeLog_Window.Window_Show();
        }
        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (Sound_Editor_Window.Visibility == Visibility.Visible)
                Sound_Editor_Window.IsFocusMode = false;
        }
        private void Window_Activated(object sender, EventArgs e)
        {
            if (Sound_Editor_Window.Visibility == Visibility.Visible)
                Sound_Editor_Window.IsFocusMode = true;
        }

        private void Sound_Editor_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing || NotConnectedLoginMessage())
                return;
            Sound_Editor_Window.Window_Show();
        }
    }
}