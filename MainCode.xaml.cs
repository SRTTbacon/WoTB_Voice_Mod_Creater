using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
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
public static class ListBoxExtensions
{
    public static IList<int> SelectedIndexs(this System.Windows.Controls.ListBox list)
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
            throw new ArgumentNullException("値がnullです。このメゾットを実行する前に値を初期化してください。");
        var provider = new ButtonAutomationPeer(button) as IInvokeProvider;
        provider.Invoke();
    }
}
public class SRTTbacon_Server
{
    public const string IP_LocalHost = "localhost";
    public const string IP_GlocalHost = "srttbacon-lostwords.net";
    public const string SFTP_UserName = "SRTTbacon_Server";
    public const string SFTP_Password = "Local_Period_Lost_Words";
    //本物
    public static string IP_Local = "非公開";
    public static string IP_Global = "非公開";
    public static string Name = "非公開";
    public static string Password = "非公開";
    public static string Version = "1.4.9";
    public static int TCP_Port = -1;
    public static int SFTP_Port = -1;
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
        bool IsDragMoveMode = false;
        //チャットモード(0が全体:1がサーバー内:2が管理者チャット)
        //管理者チャットは管理者(SRTTbacon)と個人チャットする用(主にバグ報告かな？)
        int Chat_Mode = 0;
        readonly List<string> Server_Names_List = new List<string>();
        BrushConverter bc = new BrushConverter();
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", EntryPoint = "GetWindowText", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
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
            if (!File.Exists(Path + "/dll/DdsFileTypePlusIO_x86.dll"))
                DVPL.Resources_Extract("DdsFileTypePlusIO_x86.dll");
            if (!File.Exists(Path + "/dll/bassenc.dll"))
                DVPL.Resources_Extract("bassenc.dll");
            if (!File.Exists(Path + "/dll/bassmix.dll"))
                DVPL.Resources_Extract("bassmix.dll");
            if (Environment.UserName == "SRTTbacon" || Environment.UserName == "SRTTbacon_V1")
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
                //左上のアイコンを設定
                System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(Process.GetCurrentProcess().MainModule.FileName);
                BitmapSource source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                Icon_Small_Image.Source = source;
                Save_Window.Opacity = 0;
                MouseLeftButtonDown += (sender, e) => { ScreenMove(); };
                Fmod_Player.ESystem.Init(128, Cauldron.FMOD.INITFLAGS.NORMAL, IntPtr.Zero);
                FMOD_API.System FModSys = new FMOD_API.System();
                FMOD_API.Factory.System_Create(ref FModSys);
                FMOD.Fmod_System.FModSystem = FModSys;
                FMOD.Fmod_System.FModSystem.init(16, FMOD_API.INITFLAGS.NORMAL, IntPtr.Zero);
                Connect_Mode_Layout();
                //自分はサーバーに参加できないためIPを分ける
                if (SRTTbacon_Server.IsSRTTbaconOwnerMode)
                    SRTTbacon_Server.IP = SRTTbacon_Server.IP_Local;
                else
                    SRTTbacon_Server.IP = SRTTbacon_Server.IP_Global;
                //一時ファイルの保存先を変更している場合それを適応
                if (File.Exists(Path + "/TempDirPath.dat"))
                {
                    try
                    {
                        StreamReader str = Sub_Code.File_Decrypt_To_Stream(Path + "/TempDirPath.dat", "Temp_Directory_Path_Pass");
                        string Read = str.ReadLine();
                        str.Close();
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
                        StreamReader str = Sub_Code.File_Decrypt_To_Stream(Path + "/WoTB_Path.dat", "WoTB_Directory_Path_Pass");
                        string Read = str.ReadLine();
                        str.Close();
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
                    Sub_Code.DVPL_File_Copy(Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_battle_basic.bnk", Path + "/Backup/Main/ui_battle_basic.bnk", false);
                }
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
                        using (StreamReader str = Sub_Code.File_Decrypt_To_Stream(Voice_Set.Special_Path + "/Configs/Main_Configs_Save.conf", "SRTTbacon_Main_Config_Save"))
                        {
                            IsFullScreen = bool.Parse(str.ReadLine());
                            Sub_Code.IsWindowBarShow = bool.Parse(str.ReadLine());
                            str.Close();
                            if (!IsFullScreen)
                                Window_Size_Change(false);
                        }
                    }
                    catch (Exception e)
                    {
                        Sub_Code.Error_Log_Write(e.Message);
                    }
                }
                if (Sub_Code.IsWindowBarShow)
                    WindowBarMode_Image.Source = Sub_Code.Check_03;
                else
                    WindowBarMode_Image.Source = Sub_Code.Check_01;
                Flash.Handle = this;
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;
                Button_Move();
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                MessageBox.Show("エラーが発生しました。作者にError_Log.txtを送ってください。\nソフトは強制終了されます。");
                Application.Current.Shutdown();
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            System.Drawing.Size MaxSize = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size;
            MaxWidth = MaxSize.Width;
            MaxHeight = MaxSize.Height;
            Video_Mode.Width = Width;
            if (IsFullScreen)
            {
                Left = 0;
                Top = 0;
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
                    if (Directory.Exists(Path + "/Backup/Update"))
                        Directory.Delete(Path + "/Backup/Update", true);
                }
                catch (Exception e)
                {
                    Sub_Code.Error_Log_Write(e.Message);
                }
            }
        }
        //ウィンドウが最大化していない場合ドラッグで移動
        void ScreenMove()
        {
            if (!IsFullScreen && IsDragMoveMode && !Video_Mode.IsVideoClicked)
                DragMove();
            else if (!IsFullScreen && !Sub_Code.IsWindowBarShow && !Video_Mode.IsVideoClicked)
                DragMove();
        }
        //ウィンドウのフェードイン
        async void Window_Show()
        {
            Opacity = 0;
            Load_Window_Set();
            Loop();
            while (Opacity < 1 && !IsClosing)
            {
                Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
            if (Voice_Set.TCP_Server.IsConnected)
                Voice_Set.TCP_Server.Send(Voice_Set.UserName + "|Get_Music_Count");
            if (!SRTTbacon_Server.IsSRTTbaconOwnerMode)
            {
                if (Voice_Set.UserName == "")
                    Voice_Set.TCP_Server.Send("Login|ゲスト");
                else
                    Voice_Set.TCP_Server.Send("Login|" + Voice_Set.UserName);
            }
        }
        async void Load_Window_Set()
        {
            if (!Voice_Set.FTPClient.IsConnected)
                return;
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
                bool IsOK_09 = true;
                bool IsOK_10 = true;
                StreamReader str = Voice_Set.FTPClient.GetFileRead("/WoTB_Voice_Mod/Update/Wwise/Version_01.txt");
                Sub_Code.IsWwise_Blitz_Update = str.ReadLine();
                Sub_Code.IsWwise_Blitz_Actor_Update = str.ReadLine();
                Sub_Code.IsWwise_Hits_Update = str.ReadLine();
                Sub_Code.IsWwise_Gun_Update = str.ReadLine();
                Sub_Code.IsWwise_Player_Update = str.ReadLine();
                Sub_Code.IsWwise_WoT_Gun_Update= str.ReadLine();
                Sub_Code.IsWwise_UI_Button_Sound = str.ReadLine();
                Sub_Code.SE_Version = str.ReadLine();
                str.Dispose();
                bool IsDeleteSE = false;
                bool IsMove = false;
                if (File.Exists(Voice_Set.Special_Path + "\\SE\\Version.dat"))
                {
                    StreamReader str2 = new StreamReader(Voice_Set.Special_Path + "\\SE\\Version.dat");
                    if (str2.ReadLine() != Sub_Code.SE_Version)
                        IsDeleteSE = true;
                    str2.Close();
                    str2.Dispose();
                }
                else
                    IsDeleteSE = true;
                if (IsDeleteSE)
                {
                    if (Directory.Exists(Voice_Set.Special_Path + "\\SE\\Voices"))
                    {
                        Directory.Move(Voice_Set.Special_Path + "\\SE\\Voices", Voice_Set.Special_Path + "\\Voices");
                        IsMove = true;
                    }
                    if (Directory.Exists(Voice_Set.Special_Path + "\\SE"))
                        Directory.Delete(Voice_Set.Special_Path + "\\SE", true);
                }
                Download_Data_File.Download_Total_Size = 0;
                Task task = Task.Run(async () =>
                {
                    if (!File.Exists(Voice_Set.Special_Path + "/Loading/148.png"))
                    {
                        string Loading_Path = Voice_Set.Special_Path + "/Loading.dat";
                        IsOK_00 = false;
                        Download_Data_File.Download_File_Path.Add(Loading_Path);
                        Download_Data_File.Download_Total_Size += Voice_Set.FTPClient.GetFileSize("/WoTB_Voice_Mod/Update/Data/Loading.zip");
                        Task task_02 = Task.Run(() =>
                        {
                            Task task_01 = Task.Run(() =>
                            {
                                SFTP_Client ftp1 = new SFTP_Client(SRTTbacon_Server.IP, SRTTbacon_Server.Name, SRTTbacon_Server.Password, SRTTbacon_Server.SFTP_Port);
                                ftp1.DownloadFile("/WoTB_Voice_Mod/Update/Data/Loading.zip", Loading_Path);
                                ftp1.Close();
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
                        Download_Data_File.Download_Total_Size += Voice_Set.FTPClient.GetFileSize("/WoTB_Voice_Mod/Update/Data/DVPL.zip");
                        Task task_02 = Task.Run(() =>
                        {
                            Task task_01 = Task.Run(() =>
                            {
                                SFTP_Client ftp1 = new SFTP_Client(SRTTbacon_Server.IP, SRTTbacon_Server.Name, SRTTbacon_Server.Password, SRTTbacon_Server.SFTP_Port);
                                ftp1.DownloadFile("/WoTB_Voice_Mod/Update/Data/DVPL.zip", Voice_Set.Special_Path + "/DVPL.dat");
                                ftp1.Close();
                            });
                            task_01.Wait();
                            IsOK_01 = true;
                        });
                    }
                    if (!File.Exists(Voice_Set.Special_Path + "/Encode_Mp3/ffmpeg.exe"))
                    {
                        IsOK_02 = false;
                        Download_Data_File.Download_File_Path.Add(Voice_Set.Special_Path + "/Encode_Mp3.dat");
                        Download_Data_File.Download_Total_Size += Voice_Set.FTPClient.GetFileSize("/WoTB_Voice_Mod/Update/Data/Encode_Mp3.zip");
                        Task task_02 = Task.Run(() =>
                        {
                            Task task_01 = Task.Run(() =>
                            {
                                SFTP_Client ftp1 = new SFTP_Client(SRTTbacon_Server.IP, SRTTbacon_Server.Name, SRTTbacon_Server.Password, SRTTbacon_Server.SFTP_Port);
                                ftp1.DownloadFile("/WoTB_Voice_Mod/Update/Data/Encode_Mp3.zip", Voice_Set.Special_Path + "/Encode_Mp3.dat");
                                ftp1.Close();
                            });
                            task_01.Wait();
                            IsOK_02 = true;
                        });
                    }
                    if (!File.Exists(Voice_Set.Special_Path + "/Encode_Mp3/lame.exe"))
                    {
                        IsOK_08 = false;
                        Download_Data_File.Download_Total_Size += Voice_Set.FTPClient.GetFileSize("/WoTB_Voice_Mod/Update/Data/lame.exe");
                        Task task_02 = Task.Run(() =>
                        {
                            Task task_01 = Task.Run(() =>
                            {
                                SFTP_Client ftp1 = new SFTP_Client(SRTTbacon_Server.IP, SRTTbacon_Server.Name, SRTTbacon_Server.Password, SRTTbacon_Server.SFTP_Port);
                                ftp1.DownloadFile("/WoTB_Voice_Mod/Update/Data/lame.exe", Voice_Set.Special_Path + "/Encode_Mp3/lame.exe");
                                ftp1.Close();
                            });
                            task_01.Wait();
                            IsOK_08 = true;
                        });
                    }
                    if (!File.Exists(Voice_Set.Special_Path + "/Fmod_Designer/Fmod_designer.exe"))
                    {
                        IsOK_03 = false;
                        Download_Data_File.Download_File_Path.Add(Voice_Set.Special_Path + "/Fmod_Designer.dat");
                        Download_Data_File.Download_Total_Size += Voice_Set.FTPClient.GetFileSize("/WoTB_Voice_Mod/Update/Data/Fmod_Designer.zip");
                        Task task_02 = Task.Run(() =>
                        {
                            Task task_01 = Task.Run(() =>
                            {
                                SFTP_Client ftp1 = new SFTP_Client(SRTTbacon_Server.IP, SRTTbacon_Server.Name, SRTTbacon_Server.Password, SRTTbacon_Server.SFTP_Port);
                                ftp1.DownloadFile("/WoTB_Voice_Mod/Update/Data/Fmod_Designer.zip", Voice_Set.Special_Path + "/Fmod_Designer.dat");
                                ftp1.Close();
                            });
                            task_01.Wait();
                            IsOK_03 = true;
                        });
                    }
                    if (!File.Exists(Voice_Set.Special_Path + "/SE/Capture_End_01.wav") || IsDeleteSE)
                    {
                        IsOK_04 = false;
                        Download_Data_File.Download_File_Path.Add(Voice_Set.Special_Path + "/SE.dat");
                        Download_Data_File.Download_Total_Size += Voice_Set.FTPClient.GetFileSize("/WoTB_Voice_Mod/Update/Data/SE.zip");
                        Task task_02 = Task.Run(() =>
                        {
                            Task task_01 = Task.Run(() =>
                            {
                                SFTP_Client ftp1 = new SFTP_Client(SRTTbacon_Server.IP, SRTTbacon_Server.Name, SRTTbacon_Server.Password, SRTTbacon_Server.SFTP_Port);
                                ftp1.DownloadFile("/WoTB_Voice_Mod/Update/Data/SE.zip", Voice_Set.Special_Path + "/SE.dat");
                                ftp1.Close();
                            });
                            task_01.Wait();
                            IsOK_04 = true;
                        });
                    }
                    if (!File.Exists(Voice_Set.Special_Path + "/Wwise/x64/Release/bin/Wwise.exe"))
                    {
                        IsOK_06 = false;
                        Download_Data_File.Download_File_Path.Add(Voice_Set.Special_Path + "/Wwise.dat");
                        Download_Data_File.Download_Total_Size += Voice_Set.FTPClient.GetFileSize("/WoTB_Voice_Mod/Update/Data/Wwise.zip");
                        Task task_02 = Task.Run(() =>
                        {
                            Task task_01 = Task.Run(() =>
                            {
                                SFTP_Client ftp1 = new SFTP_Client(SRTTbacon_Server.IP, SRTTbacon_Server.Name, SRTTbacon_Server.Password, SRTTbacon_Server.SFTP_Port);
                                ftp1.DownloadFile("/WoTB_Voice_Mod/Update/Data/Wwise.zip", Voice_Set.Special_Path + "/Wwise.dat");
                                ftp1.Close();
                            });
                            task_01.Wait();
                            IsOK_06 = true;
                        });
                    }
                    if (!File.Exists(Voice_Set.Special_Path + "/Wwise_Parse/wwiser.pyz"))
                    {
                        IsOK_07 = false;
                        Download_Data_File.Download_File_Path.Add(Voice_Set.Special_Path + "/Wwise_Parse.dat");
                        Download_Data_File.Download_Total_Size += Voice_Set.FTPClient.GetFileSize("/WoTB_Voice_Mod/Update/Data/Wwise_Parse.zip");
                        Task task_02 = Task.Run(() =>
                        {
                            Task task_01 = Task.Run(() =>
                            {
                                SFTP_Client ftp1 = new SFTP_Client(SRTTbacon_Server.IP, SRTTbacon_Server.Name, SRTTbacon_Server.Password, SRTTbacon_Server.SFTP_Port);
                                ftp1.DownloadFile("/WoTB_Voice_Mod/Update/Data/Wwise_Parse.zip", Voice_Set.Special_Path + "/Wwise_Parse.dat");
                                ftp1.Close();
                            });
                            task_01.Wait();
                            IsOK_07 = true;
                        });
                    }
                    if (!File.Exists(Voice_Set.Special_Path + "/WEM_To_WAV/WEM_To_WAV.exe"))
                    {
                        IsOK_09 = false;
                        Download_Data_File.Download_File_Path.Add(Voice_Set.Special_Path + "/WEM_To_WAV.dat");
                        Download_Data_File.Download_Total_Size += Voice_Set.FTPClient.GetFileSize("/WoTB_Voice_Mod/Update/Data/WEM_To_WAV.zip");
                        Task task_02 = Task.Run(() =>
                        {
                            Task task_01 = Task.Run(() =>
                            {
                                SFTP_Client ftp1 = new SFTP_Client(SRTTbacon_Server.IP, SRTTbacon_Server.Name, SRTTbacon_Server.Password, SRTTbacon_Server.SFTP_Port);
                                ftp1.DownloadFile("/WoTB_Voice_Mod/Update/Data/WEM_To_WAV.zip", Voice_Set.Special_Path + "/WEM_To_WAV.dat");
                                ftp1.Close();
                            });
                            task_01.Wait();
                            IsOK_09 = true;
                        });
                    }
                    if (!File.Exists(Voice_Set.Special_Path + "/Other/Init.bnk"))
                    {
                        IsOK_10 = false;
                        Download_Data_File.Download_File_Path.Add(Voice_Set.Special_Path + "/Other.dat");
                        Download_Data_File.Download_Total_Size += Voice_Set.FTPClient.GetFileSize("/WoTB_Voice_Mod/Update/Data/Other.zip");
                        Task task_02 = Task.Run(() =>
                        {
                            Task task_01 = Task.Run(() =>
                            {
                                SFTP_Client ftp1 = new SFTP_Client(SRTTbacon_Server.IP, SRTTbacon_Server.Name, SRTTbacon_Server.Password, SRTTbacon_Server.SFTP_Port);
                                ftp1.DownloadFile("/WoTB_Voice_Mod/Update/Data/Other.zip", Voice_Set.Special_Path + "/Other.dat");
                                ftp1.Close();
                            });
                            task_01.Wait();
                            IsOK_10 = true;
                        });
                    }
                });
                await Task.Delay(100);
                while (true)
                {
                    if (IsOK_00 && IsOK_01 && IsOK_02 && IsOK_03 && IsOK_04 && IsOK_05 && IsOK_06 && IsOK_07 && IsOK_08 && IsOK_09 && IsOK_10)
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
                if (IsMove)
                    Directory.Move(Voice_Set.Special_Path + "\\Voices", Voice_Set.Special_Path + "\\SE\\Voices");
                StreamWriter stw = new StreamWriter(Voice_Set.Special_Path + "\\SE\\Version.dat");
                stw.Write(Sub_Code.SE_Version);
                stw.Close();
                stw.Dispose();
                StreamReader str3 = Voice_Set.FTPClient.GetFileRead("/WoTB_Voice_Mod/Update/Wwise/Version_02.txt");
                Sub_Code.IsWwise_WoT_Update = str3.ReadLine();
                str3.Dispose();
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
        //全てのサーバーの情報を取得
        void Connect_Start()
        {
            //Server_List_Reset();
            //コマンドをリアルタイムで通信
            Voice_Set.TCP_Server.DataReceive += (msg) =>
            {
                try
                {
                    if (Voice_Set.SRTTbacon_Server_Name != "" || Voice_Set.TCP_Server.IsConnected)
                    {
                        string[] Message_Temp = msg.Split('|');
                        if (Message_Temp[0] == Voice_Set.SRTTbacon_Server_Name)
                        {
                            if (Chat_Mode == 1 && Message_Temp[1] == "Chat")
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    Chat_T_Change("/WoTB_Voice_Mod/" + Voice_Set.SRTTbacon_Server_Name + "/Chat.dat");
                                    Chat_Scrool.ScrollToEnd();
                                });
                            }
                            else if (Message_Temp[1] == "Change_Configs")
                                TCP_Change_Config(Message_Temp);
                        }
                        else if (Message_Temp[0] == "Public")
                        {
                            if (Chat_Mode == 0 && Message_Temp[1] == "Chat")
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    Chat_T_Change("/WoTB_Voice_Mod/Chat.dat");
                                    Chat_Scrool.ScrollToEnd();
                                });
                            }
                            else if (Message_Temp[1].Contains("Music_Change"))
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    Other_Window.Vocal_Inst_Cut_Window.Change_Server_Stetus(int.Parse(Message_Temp[2].Trim()));
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
                                    Chat_Scrool.ScrollToEnd();
                                });
                            }
                            else if (Message_Temp[1].Contains("Music_OK"))
                                Other_Window.Vocal_Inst_Cut_Window.Music_Status_Change(Message_Temp[2]);
                            else if (Message_Temp[1].Contains("Get_Music_Count"))
                                Other_Window.Vocal_Inst_Cut_Window.Change_Server_Stetus(int.Parse(Message_Temp[2]));
                        }
                    }
                }
                catch (Exception e)
                {
                    Sub_Code.Error_Log_Write(e.Message);
                }
            };
            //IsConnecting();
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
                        Voice_Set.TCP_Server.Send("Login|" + Voice_Set.UserName);
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
                Voice_Set.FTPClient.UploadFile(Voice_Set.Special_Path + "/Temp_User_Chat.dat", "/WoTB_Voice_Mod/Users/" + User_Name_T.Text + "_Chat.dat");
                Voice_Set.AppendString("/WoTB_Voice_Mod/Accounts.dat", User_Name_T.Text + ":" + User_Password_T.Text + "\n");
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
                    Voice_Set.TCP_Server.Send("Register|" + Voice_Set.UserName);
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
                return;
            //現在のバージョンでは使用できません。
            if (Chat_Mode == 1)
                return;
            if (Chat_Send_T.Text.CountOf("  ") >= 1 || Chat_Send_T.Text.CountOf("　　") >= 1)
            {
                Message_Feed_Out("スパム防止のため空白を2回連続で使用できません。");
                return;
            }
            if (Chat_Send_T.Text.Length >= 125)
            {
                Message_Feed_Out("文字数が多すぎます。125文字以下にしてください。");
                return;
            }
            if (Chat_Send_T.Text.Contains("|"))
            {
                Message_Feed_Out("'|'を送信することはできません。");
                return;
            }
            if (Chat_Mode == 0)
            {
                Voice_Set.AppendString("/WoTB_Voice_Mod/Chat.dat", Voice_Set.UserName + ":" + Chat_Send_T.Text + "\n");
                Voice_Set.TCP_Server.Send("Public|Chat|" + Voice_Set.UserName + ":" + Chat_Send_T.Text);
            }
            else if (Chat_Mode == 1)
            {
                /*Voice_Set.AppendString("/WoTB_Voice_Mod/" + Voice_Set.SRTTbacon_Server_Name + "/Chat.dat", Encoding.UTF8.GetBytes(Voice_Set.UserName + ":" + Chat_Send_T.Text + "\n"));
                Voice_Set.TCP_Server.WriteLine(Voice_Set.SRTTbacon_Server_Name + "|Chat|" + Voice_Set.UserName + ":" + Chat_Send_T.Text + '\0');*/
            }
            else if (Chat_Mode == 2)
            {
                Voice_Set.AppendString("/WoTB_Voice_Mod/Users/" + Voice_Set.UserName + "_Chat.dat", Voice_Set.UserName + ":" + Chat_Send_T.Text + "\n");
                Voice_Set.TCP_Server.Send(Voice_Set.UserName + "_Private|Chat|" + Voice_Set.UserName + ":" + Chat_Send_T.Text);
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
                string Line = Voice_Set.FTPClient.GetFileLine("/WoTB_Voice_Mod/Update/Configs.dat", true);
                if (Line == SRTTbacon_Server.Version)
                    Message_Feed_Out("既に最新のバージョンです。");
                else
                {
                    MessageBoxResult result = MessageBox.Show("新しいバージョンが見つかりました(V" + Line + ")。ダウンロードして適応しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes);
                    if (result == MessageBoxResult.Yes)
                    {
                        string Download_Path = System.IO.Path.GetTempPath();
                        Voice_Set.App_Busy = true;
                        Download_P.Visibility = Visibility.Visible;
                        Download_T.Visibility = Visibility.Visible;
                        Download_Border.Visibility = Visibility.Visible;
                        Message_T.Text = "ダウンロード中です。処理が完了したらソフトを再起動します...";
                        await Task.Delay(50);
                        if (Directory.Exists(Path + "/Backup/Update"))
                            Directory.Delete(Path + "/Backup/Update", true);
                        Directory.CreateDirectory(Path + "/Backup/Update");
                        Directory.CreateDirectory(Download_Path + "/Mod_Creater_Update");
                        IsProcessing = true;
                        if (Voice_Set.UserName == "")
                            Voice_Set.TCP_Server.Send("Update|ゲスト");
                        else
                            Voice_Set.TCP_Server.Send("Update|" + Voice_Set.UserName);
                        foreach (string File_Name in Voice_Set.FTPClient.GetFiles("/WoTB_Voice_Mod/Update/" + Line, false, false))
                        {
                            try
                            {
                                long File_Size_Full = Voice_Set.FTPClient.GetFileSize("/WoTB_Voice_Mod/Update/" + Line + "/" + File_Name);
                                Task task = Task.Run(() =>
                                {
                                    Voice_Set.FTPClient.DownloadFile("/WoTB_Voice_Mod/Update/" + Line + "/" + File_Name, Download_Path + "/Mod_Creater_Update/" + File_Name);
                                });
                                while (true)
                                {
                                    long File_Size_Now = 0;
                                    if (File.Exists(Download_Path + "/Mod_Creater_Update/" + File_Name))
                                    {
                                        FileInfo fi = new FileInfo(Download_Path + "/Mod_Creater_Update/" + File_Name);
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
                        bool IsReboot = false;
                        string[] Dir_Update = Directory.GetFiles(Download_Path + "/Mod_Creater_Update", "*", SearchOption.AllDirectories);
                        foreach (string File_Now in Dir_Update)
                        {
                            if (System.IO.Path.GetFileName(File_Now) == "WoTB_Voice_Mod_Creater.exe")
                                IsReboot = true;
                            string Dir_Only = File_Now.Replace(System.IO.Path.GetFileName(File_Now), "");
                            string Temp_01 = Dir_Only.Replace("/", "\\");
                            string File_Dir = Temp_01.Replace(Download_Path.Replace("/", "\\") + "\\Mod_Creater_Update\\", "");
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
                        try
                        {
                            Directory.Delete(Download_Path + "/Mod_Creater_Update", true);
                        }
                        catch (Exception e1)
                        {
                            Sub_Code.Error_Log_Write("アップデートに関するフォルダを削除できませんでした。詳細:" + e1.Message);
                        }
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
            if (IsProcessing)
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
            Voice_Create_Window.Window_Show();
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
                    Message_Feed_Out("steamappsフォルダ以降の階層のフォルダを選択する必要があります。");
                ApplyAllFiles(ofd.SelectedFolder, ProcessFile);
                if (Voice_Set.WoTB_Path == "")
                    Message_Feed_Out("WoTBのフォルダを取得できませんでした。");
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
            bool IsForce = false;
            while (Message_T.Opacity > 0)
            {
                if (!IsMessageShowing)
                {
                    IsForce = true;
                    break;
                }
                Number++;
                if (Number >= 120)
                    Message_T.Opacity -= 0.025;
                await Task.Delay(1000 / 60);
            }
            if (!IsForce)
            {
                IsMessageShowing = false;
                Message_T.Text = "";
                Message_T.Opacity = 1;
            }
        }
        void Main_Config_Save()
        {
            try
            {
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Configs/Main_Configs_Save.tmp");
                stw.WriteLine(IsFullScreen);
                stw.Write(Sub_Code.IsWindowBarShow);
                stw.Close();
                Sub_Code.File_Encrypt(Voice_Set.Special_Path + "/Configs/Main_Configs_Save.tmp", Voice_Set.Special_Path + "/Configs/Main_Configs_Save.conf", "SRTTbacon_Main_Config_Save", true);
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        //ウィンドウサイズを変更
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
        bool IsOtherWindowShowed()
        {
            if (Save_Window.Visibility == Visibility.Visible || Voice_Mods_Window.Visibility == Visibility.Visible || Tools_Window.Visibility == Visibility ||
                Other_Window.Visibility == Visibility.Visible || Voice_Create_Window.Visibility == Visibility.Visible || Message_Window.Visibility == Visibility.Visible || Load_Data_Window.Visibility == Visibility.Visible ||
                Tools_V2_Window.Visibility == Visibility.Visible || Change_To_Wwise_Window.Visibility == Visibility.Visible || WoT_To_Blitz_Window.Visibility == Visibility.Visible ||
                Blitz_To_WoT_Window.Visibility == Visibility.Visible || Bank_Editor_Window.Visibility == Visibility.Visible || Create_Save_File_Window.Visibility == Visibility.Visible ||
                Create_Loading_BGM_Window.Visibility == Visibility.Visible || BNK_Event_Window.Visibility == Visibility.Visible || BNK_To_Project_Window.Visibility == Visibility.Visible ||
                Sound_Editor_Window.Visibility == Visibility.Visible)
                return true;
            return false;
        }
        //キーが押されたときの処理
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
            if (IsOtherWindowShowed())
                return;
            if (Chat_Send_T.IsFocused)
                return;
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
                        Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/WEM_To_WAV");
                        Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/Other");
                        Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/Temp");
                    }
                    catch (Exception e1)
                    {
                        Sub_Code.Error_Log_Write(e1.Message);
                    }
                    try
                    {
                        if (Voice_Set.FTPClient.IsConnected)
                        {
                            Voice_Set.FTPClient.Close();
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
                        try
                        {
                            Sub_Code.Set_Directory_Path(ofd.SelectedFolder);
                            string Dir = ofd.SelectedFolder;
                            if (Voice_Set.Special_Path.Replace("/", "\\") == Dir)
                            {
                                Message_Feed_Out("変更元と変更先が同じフォルダです。");
                                return;
                            }
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
                                Message_Feed_Out("フォルダ内は空である必要があります。");
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
                            Sub_Code.Directory_Copy(Voice_Set.Special_Path + "/WEM_To_WAV", Dir);
                            Sub_Code.Directory_Copy(Voice_Set.Special_Path + "/Other", Dir);
                            Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/Configs");
                            Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/DVPL");
                            Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/Encode_Mp3");
                            Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/Fmod_Android_Create");
                            Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/Fmod_Designer");
                            Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/Loading");
                            Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/SE");
                            Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/Wwise");
                            Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/Wwise_Parse");
                            Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/WEM_To_WAV");
                            Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/Other");
                            if (File.Exists(Voice_Set.Special_Path + "/Spleeter_Miniconda.dat"))
                                File.Move(Voice_Set.Special_Path + "/Spleeter_Miniconda.dat", Dir + "\\Spleeter_Miniconda.dat");
                            StreamWriter stw = File.CreateText(Dir + "/TempDirPath.dat");
                            stw.Write(Dir);
                            stw.Close();
                            Sub_Code.File_Encrypt(Dir + "/TempDirPath.dat", Path + "/TempDirPath.dat", "Temp_Directory_Path_Pass", true);
                            Voice_Set.Special_Path = Dir;
                            Message_Feed_Out("一時ファイルの保存先を変更しました。");
                        }
                        catch (Exception e1)
                        {
                            Sub_Code.Error_Log_Write(e1.Message);
                            Message_Feed_Out("エラーが発生しました。");
                        }
                    }
                }
            }
            //エラーログをクリア
            if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Shift) == System.Windows.Forms.Keys.Shift && e.Key == System.Windows.Input.Key.L)
            {
                try
                {
                    if (!File.Exists(Path + "/Error_Log.txt"))
                        Message_Feed_Out("ログファイルが存在しません。");
                    StreamReader str = new StreamReader(Path + "/Error_Log.txt");
                    string Temp = str.ReadToEnd();
                    str.Close();
                    if (Temp == "")
                        Message_Feed_Out("ログはすでにクリアされています。");
                    MessageBoxResult result = MessageBox.Show("エラーログをクリアしますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
                    if (result == MessageBoxResult.Yes)
                        File.WriteAllText(Path + "/Error_Log.txt", "");
                }
                catch (Exception e1)
                {
                    Message_Feed_Out("エラーログをクリアできませんでした。");
                    Sub_Code.Error_Log_Write(e1.Message);
                }
            }
            //一時フォルダの位置を確認
            if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Shift) == System.Windows.Forms.Keys.Shift && e.Key == System.Windows.Input.Key.P)
                MessageBox.Show("一時フォルダ場所:" + Voice_Set.Special_Path);
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
                //音声仕分け。多分もう使わない
                if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Shift) == System.Windows.Forms.Keys.Shift && e.Key == System.Windows.Input.Key.I)
                {
                    BetterFolderBrowser bfd = new BetterFolderBrowser()
                    {
                        Title = "フォルダを選択してください。",
                        RootFolder = Sub_Code.Get_OpenDirectory_Path(),
                        Multiselect = false
                    };
                    if (bfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        BetterFolderBrowser bfd2 = new BetterFolderBrowser()
                        {
                            Title = "フォルダを選択してください。",
                            RootFolder = Sub_Code.Get_OpenDirectory_Path(),
                            Multiselect = false
                        };
                        if (bfd2.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            Directory.CreateDirectory(bfd2.SelectedFolder + "\\Erisa");
                            Directory.CreateDirectory(bfd2.SelectedFolder + "\\Yuuri");
                            Directory.CreateDirectory(bfd2.SelectedFolder + "\\Kazuha");
                            Directory.CreateDirectory(bfd2.SelectedFolder + "\\Yaya");
                            Directory.CreateDirectory(bfd2.SelectedFolder + "\\Yuki");
                            Directory.CreateDirectory(bfd2.SelectedFolder + "\\Kozue");
                            Directory.CreateDirectory(bfd2.SelectedFolder + "\\Ririeto");
                            foreach (string File_Now in Directory.GetFiles(bfd.SelectedFolder, "*.ogg", SearchOption.AllDirectories))
                            {
                                string Charactor_Name = "";
                                string FileName = System.IO.Path.GetFileNameWithoutExtension(File_Now).ToLower();
                                int Count = CountChar(FileName, '_');
                                if (Count == 3)
                                {
                                    string Name_01 = FileName.Substring(FileName.IndexOf("_") + 1);
                                    Charactor_Name = Name_01.Substring(Name_01.IndexOf("_") + 1).ToLower();
                                }
                                else if (Count == 2)
                                    Charactor_Name = FileName.Substring(FileName.IndexOf("_") + 1).ToLower();
                                if (Charactor_Name.Contains("erisa"))
                                    File.Move(File_Now, bfd2.SelectedFolder + "\\Erisa\\1" + System.IO.Path.GetFileNameWithoutExtension(File_Now) + Sub_Code.r.Next(0, 10000) + ".ogg");
                                else if (Charactor_Name.Contains("yuuri"))
                                    File.Move(File_Now, bfd2.SelectedFolder + "\\Yuuri\\1" + System.IO.Path.GetFileNameWithoutExtension(File_Now) + Sub_Code.r.Next(0, 10000) + ".ogg");
                                else if (Charactor_Name.Contains("kazuha"))
                                    File.Move(File_Now, bfd2.SelectedFolder + "\\Kazuha\\1" + System.IO.Path.GetFileNameWithoutExtension(File_Now) + Sub_Code.r.Next(0, 10000) + ".ogg");
                                else if (Charactor_Name.Contains("yaya"))
                                    File.Move(File_Now, bfd2.SelectedFolder + "\\Yaya\\1" + System.IO.Path.GetFileNameWithoutExtension(File_Now) + Sub_Code.r.Next(0, 10000) + ".ogg");
                                else if (Charactor_Name.Contains("yuki"))
                                    File.Move(File_Now, bfd2.SelectedFolder + "\\Yuki\\1" + System.IO.Path.GetFileNameWithoutExtension(File_Now) + Sub_Code.r.Next(0, 10000) + ".ogg");
                                else if (Charactor_Name.Contains("kozue"))
                                    File.Move(File_Now, bfd2.SelectedFolder + "\\Kozue\\1" + System.IO.Path.GetFileNameWithoutExtension(File_Now) + Sub_Code.r.Next(0, 10000) + ".ogg");
                                else if (Charactor_Name.Contains("ririeto"))
                                    File.Move(File_Now, bfd2.SelectedFolder + "\\Ririeto\\1" + System.IO.Path.GetFileNameWithoutExtension(File_Now) + Sub_Code.r.Next(0, 10000) + ".ogg");
                            }
                            Message_Feed_Out("完了しました。");
                        }
                        bfd2.Dispose();
                    }
                    bfd.Dispose();
                }
                //パイモン抽出器
                if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Shift) == System.Windows.Forms.Keys.Shift && e.Key == System.Windows.Input.Key.G)
                {
                    BetterFolderBrowser bfd = new BetterFolderBrowser()
                    {
                        Title = "フォルダを選択してください。",
                        RootFolder = Sub_Code.Get_OpenDirectory_Path(),
                        Multiselect = true
                    };
                    if (bfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        BetterFolderBrowser bfd2 = new BetterFolderBrowser()
                        {
                            Title = "保存先フォルダを選択してください。",
                            RootFolder = Sub_Code.Get_OpenDirectory_Path(),
                            Multiselect = false
                        };
                        if (bfd2.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            Sub_Code.Set_Directory_Path(bfd2.SelectedFolder);
                            int Count = 0;
                            foreach (string Dir in bfd.SelectedFolders)
                            {
                                string[] Files = Directory.GetFiles(Dir, "*.wav", SearchOption.AllDirectories);
                                foreach (string File in Files)
                                {
                                    Count++;
                                    if (Count < 10)
                                        System.IO.File.Move(File, bfd2.SelectedFolder + "\\1Paimon_00" + Count);
                                    else if (Count < 100)
                                        System.IO.File.Move(File, bfd2.SelectedFolder + "\\1Paimon_0" + Count);
                                    else
                                        System.IO.File.Move(File, bfd2.SelectedFolder + "\\1Paimon_" + Count);
                                }
                            }
                            MessageBox.Show("完了しました。\nパイモンの音声数:" + Count);
                        }
                        bfd2.Dispose();
                    }
                    bfd.Dispose();
                }
            }
            IsClosing = false;
        }
        int CountChar(string s, char c)
        {
            return s.Length - s.Replace(c.ToString(), "").Length;
        }
        private async void Voice_Create_V2_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing || NotConnectedLoginMessage())
                return;
            if (!File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Version.dat") && !Voice_Set.FTPClient.IsConnected)
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
                if (Version_Wwise < 1.7)
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
            IsMessageShowing = false;
            Voice_Create_Window.Window_Show();
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
            if (!File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Version.dat") && !Voice_Set.FTPClient.IsConnected)
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
                await Task.Delay(100);
            IsProcessing = false;
            if (Voice_Set.SRTTbacon_Server_Name != "")
                Server_Voices_Sort_Window.Window_Show(false, "サーバーに参加しました。");
        }
        private async void WoT_To_Blitz_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing || NotConnectedLoginMessage())
                return;
            if (!File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Version.dat") && !Voice_Set.FTPClient.IsConnected)
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
            if (!File.Exists(Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod/Version.dat") && !Voice_Set.FTPClient.IsConnected)
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
            if (!File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Version.dat") && !Voice_Set.FTPClient.IsConnected)
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
            if (!Voice_Set.FTPClient.IsConnected)
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
            if (!File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Version.dat") && !Voice_Set.FTPClient.IsConnected)
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
            if (Ex == ".wvs" || Ex == ".wms" || Ex == ".wse")
                e.Effects = DragDropEffects.Copy;
            else if (Other_Window.Visibility == Visibility.Visible)
            {
                if (Ex == ".mp3" || Ex == ".wav" || Ex == ".ogg" || Ex == ".aiff" || Ex == ".flac" || Ex == ".m4a" || Ex == ".mp4")
                    e.Effects = DragDropEffects.Copy;
                else
                    e.Effects = DragDropEffects.None;
            }
            else if (Sound_Editor_Window.Visibility == Visibility.Visible)
            {
                if (Ex == ".mp3" || Ex == ".wav")
                    e.Effects = DragDropEffects.Copy;
                else
                    e.Effects = DragDropEffects.None;
            }
            else if (Voice_Create_Window.Visibility == Visibility.Visible)
            {
                if (Ex == ".mp3" || Ex == ".wav" || Ex == ".ogg" || Ex == ".wma" || Ex == ".flac" || Ex == ".mp4")
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
                    if (Other_Window.Visibility == Visibility.Visible)
                    {
                        if (Ex == ".mp3" || Ex == ".wav" || Ex == ".ogg" || Ex == ".aiff" || Ex == ".flac" || Ex == ".m4a" || Ex == ".mp4")
                            Other_Window.Add_Music_From_Drop(Drop_Files);
                        else
                            Message_Feed_Out("対応したファイルをドラッグしてください。");
                    }
                    else if (Voice_Create_Window.Visibility == Visibility.Visible)
                    {
                        if (Ex == ".wvs")
                            Voice_Create_Window.Voice_Load_From_File(Drop_Files[0]);
                        if (Ex == ".mp3" || Ex == ".wav" || Ex == ".ogg" || Ex == ".wma" || Ex == ".flac" || Ex == ".mp4")
                            Voice_Create_Window.Add_Voice(Drop_Files, true);
                        else
                            Message_Feed_Out("対応したファイルをドラッグしてください。");
                    }
                    else if (Sound_Editor_Window.Visibility == Visibility.Visible)
                    {
                        if (Ex == ".mp3" || Ex == ".wav")
                            Sound_Editor_Window.Add_Sound_File(Drop_Files);
                        else if (Ex == ".wse")
                            Sound_Editor_Window.Contents_Load(Drop_Files[0]);
                        else
                            Message_Feed_Out("対応したファイルをドラッグしてください。");
                    }
                    else if (Ex == ".wvs")
                    {
                        Voice_Create_Window.Window_Show();
                        Voice_Create_Window.Voice_Load_From_File(Drop_Files[0]);
                    }
                    else if (Ex == ".wms")
                    {
                        if (Create_Loading_BGM_Window.Visibility != Visibility.Visible)
                            Create_Loading_BGM_Window.Window_Show();
                        Create_Loading_BGM_Window.Load_From_File(Drop_Files[0]);
                    }
                    else if (Ex == ".wse")
                    {
                        Sound_Editor_Window.Window_Show();
                        Sound_Editor_Window.Contents_Load(Drop_Files[0]);
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
        //イベントハンドラーよりIsActiveの方が精度が高いと感じたため、IsActiveを定期的にSound_Editor_Windowに反映させる
        async void Loop()
        {
            while (true)
            {
                StringBuilder sb = new StringBuilder(65535);
                GetWindowText(GetForegroundWindow(), sb, 65535);
                if (sb.ToString() == "WoTB_Voice_Mod_Creater")
                    Sound_Editor_Window.IsFocusMode = true;
                else
                    Sound_Editor_Window.IsFocusMode = false;
                //if (IsLoaded_2)
                //    Message_T.Text = Wwise.Get_Position(uint.Parse(Test_T.Text)).ToString();
                await Task.Delay(250);
            }
        }
        private void Sound_Editor_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing || NotConnectedLoginMessage())
                return;
            Sound_Editor_Window.Window_Show();
        }
        private void Minimize_B_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void Maximize_B_Click(object sender, RoutedEventArgs e)
        {
            Window_Size_Change(true);
            Main_Config_Save();
        }
        private void Close_B_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void Window_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IsDragMoveMode = false;
        }
        private void Canvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IsDragMoveMode = true;
        }
        void Button_Move()
        {
            if (Sub_Code.IsWindowBarShow)
            {
                Main_Button_Canvas.Margin = new Thickness(0, 25, 0, 0);
                WindowBarCanvas.Visibility = Visibility.Visible;
            }
            else
            {
                Main_Button_Canvas.Margin = new Thickness(0, 0, 0, 0);
                WindowBarCanvas.Visibility = Visibility.Hidden;
            }
        }
        private void WindowBarMode_Image_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Sub_Code.IsWindowBarShow)
            {
                Sub_Code.IsWindowBarShow = false;
                WindowBarMode_Image.Source = Sub_Code.Check_02;
            }
            else
            {
                Sub_Code.IsWindowBarShow = true;
                WindowBarMode_Image.Source = Sub_Code.Check_04;
            }
            Button_Move();
            Main_Config_Save();
        }
        private void WindowBarMode_Image_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Sub_Code.IsWindowBarShow)
                WindowBarMode_Image.Source = Sub_Code.Check_04;
            else
                WindowBarMode_Image.Source = Sub_Code.Check_02;
        }
        private void WindowBarMode_Image_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Sub_Code.IsWindowBarShow)
                WindowBarMode_Image.Source = Sub_Code.Check_03;
            else
                WindowBarMode_Image.Source = Sub_Code.Check_01;
        }
        private void Chat_Send_T_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (IsOtherWindowShowed())
            {
                if (Chat_Send_T.Text.Length == 1)
                    Chat_Send_T.Text = "";
                else if (Chat_Send_T.Text.Length >= 2)
                    Chat_Send_T.Text = Chat_Send_T.Text.Substring(0, Chat_Send_T.Text.Length - 1);
            }
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //ウィンドウが画面外に行ったときサイズが変更されないように
            if (Top <= 0 && !IsFullScreen)
            {
                double Left_Before = Left;
                Window_Size_Change(false);
                Top = 0;
                Left = Left_Before;
            }
        }
        private async void Wwise_Player_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing || NotConnectedLoginMessage())
                return;
            string Dir = Directory.GetCurrentDirectory();
            string Before_Wwise_Player_Version = "1.0";
            if (File.Exists(Voice_Set.Special_Path + "\\Other\\Wwise_Player_Version.dat"))
            {
                StreamReader str = Sub_Code.File_Decrypt_To_Stream(Voice_Set.Special_Path + "\\Other\\Wwise_Player_Version.dat", "Wwise_Player_Version_Check");
                Before_Wwise_Player_Version = str.ReadLine();
                str.Close();
                str.Dispose();
            }
            if (Before_Wwise_Player_Version != Sub_Code.IsWwise_Player_Update)
            {
                IsMessageShowing = false;
                Message_T.Opacity = 1;
                Message_T.Text = "Wwise_Player.dllをアップデートしています...";
                await Task.Delay(75);
                try
                {
                    Voice_Set.TCP_Server.Send("Message|" + Voice_Set.UserName + "->Wwise_Player.dllをアップデートしています...");
                    File.Delete(Dir + "\\dll\\Wwise_Player.dll");
                    Voice_Set.FTPClient.DownloadFile("/WoTB_Voice_Mod/Update/Wwise/Wwise_Player.dll", Voice_Set.Special_Path + "\\Other\\Wwise_Player.dll");
                    StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "\\Other\\Wwise_Player_Version_Temp.dat");
                    stw.Write(Sub_Code.IsWwise_Player_Update);
                    stw.Close();
                    stw.Dispose();
                    Sub_Code.File_Encrypt(Voice_Set.Special_Path + "\\Other\\Wwise_Player_Version_Temp.dat", Voice_Set.Special_Path + "\\Other\\Wwise_Player_Version.dat", "Wwise_Player_Version_Check", true);
                    Message_Feed_Out("Wwise_Player.dllのアップデートが完了しました。");
                }
                catch (Exception e1)
                {
                    Sub_Code.Error_Log_Write(e1.Message);
                    Message_Feed_Out("エラーが発生しました。Wwise_Player.dllを正常にロードできません。");
                }
            }
            if (!File.Exists(Dir + "\\dll\\Wwise_Player.dll"))
            {
                if (File.Exists(Voice_Set.Special_Path + "\\Other\\Wwise_Player.dll"))
                    File.Copy(Voice_Set.Special_Path + "\\Other\\Wwise_Player.dll", Dir + "\\dll\\Wwise_Player.dll", true);
                else
                {
                    Message_Feed_Out("Wwise_Player.dllを参照できませんでした。ソフトを再起動すると改善する可能性があります。");
                    return;
                }
            }
            Wwise_Event_Player_Window.Window_Show();
        }
    }
}