using System;
using System.Windows;
using FluentFTP;
using System.IO;
using System.Threading.Tasks;
using WMPLib;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Diagnostics;

public static partial class StringExtensions
{
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
namespace WoTB_Voice_Mod_Creater
{
    public partial class MainCode : Window
    {
        readonly string Version = "1.2.3.2";
        readonly string Path = Directory.GetCurrentDirectory();
        bool IsClosing = false;
        bool IsMessageShowing = false;
        readonly bool IsSRTTbacon_V1 = false;
        //チャットモード(0が全体:1がサーバー内:2が管理者チャット)
        //管理者チャットは管理者(SRTTbacon)と個人チャットする用(主にバグ報告かな？)
        int Chat_Mode = 0;
        readonly string IP;
        readonly WindowsMediaPlayer Player = new WindowsMediaPlayer();
        readonly List<string> Server_Names_List = new List<string>();
        public MainCode()
        {
            Clean();
            InitializeComponent();
            try
            {

                Download_Progress_P.Visibility = Visibility.Hidden;
                Download_Progress_T.Visibility = Visibility.Hidden;
                Load_Image.Visibility = Visibility.Hidden;
                Server_Create_Window.Visibility = Visibility.Hidden;
                Password_Text.Visibility = Visibility.Hidden;
                Password_T.Visibility = Visibility.Hidden;
                Server_Create_Name_T.Visibility = Visibility.Hidden;
                WoTB_Select_B.Visibility = Visibility.Hidden;
                Server_Create_Window.Opacity = 0;
                Save_Window.Opacity = 0;
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
                    Close();
                }
                Connect_Mode_Layout();
                //自分はサーバーに参加できないためIPを分ける
                if (Environment.UserName == "SRTTbacon" || Environment.UserName == "SRTTbacon_V1")
                {
                    IP = "192.168.3.12";
                    ConnectType = FtpDataConnectionType.PASV;
                    IsPassiveMode = true;
                    if (Environment.UserName == "SRTTbacon_V1")
                    {
                        IsSRTTbacon_V1 = true;
                    }
                }
                else
                {
                    IP = "60.151.34.219";
                    ConnectType = FtpDataConnectionType.PASV;
                    IsPassiveMode = true;
                }
                //一時ファイルの保存先を変更している場合それを適応
                if (File.Exists(Directory.GetCurrentDirectory() + "/TempDirPath.dat"))
                {
                    try
                    {
                        using (var eifs = new FileStream(Directory.GetCurrentDirectory() + "/TempDirPath.dat", FileMode.Open, FileAccess.Read))
                        {
                            using (var eofs = new FileStream(Directory.GetCurrentDirectory() + "/Temp.dat", FileMode.Create, FileAccess.Write))
                            {
                                FileEncode.FileEncryptor.Decrypt(eifs, eofs, "Temp_Directory_Path_Pass");
                            }
                        }
                        StreamReader str = new StreamReader(Directory.GetCurrentDirectory() + "/Temp.dat");
                        string Read = str.ReadLine();
                        str.Close();
                        File.Delete(Directory.GetCurrentDirectory() + "/Temp.dat");
                        if (Read != "")
                        {
                            Voice_Set.Special_Path = Read;
                        }
                    }
                    catch
                    {

                    }
                }
                if (!Directory.Exists(Voice_Set.Special_Path + "/Server"))
                {
                    Directory.CreateDirectory(Voice_Set.Special_Path + "/Server");
                }
                //V1.2を実行した人用に削除
                if (File.Exists(Voice_Set.Special_Path + "/DVPL/Pack.py"))
                {
                    File.Delete(Voice_Set.Special_Path + "/DVPL/Pack.py");
                }
                Server_Connect();
                Voice_Volume_S.Maximum = 100;
                Voice_Volume_S.Value = 50;
                Voice_Type_C.Items.Add("モード:メイン");
                Voice_Type_C.Items.Add("モード:サブ");
                Voice_Type_C.SelectedIndex = 0;
                BrushConverter bc = new BrushConverter();
                Chat_Mode_Public_B.Background = Brushes.Transparent;
                Chat_Mode_Server_B.Background = Brushes.Transparent;
                Chat_Mode_Private_B.Background = (Brush)bc.ConvertFrom("#59999999");
                Chat_Mode_Public_B.BorderBrush = Brushes.Transparent;
                Chat_Mode_Server_B.BorderBrush = Brushes.Transparent;
                Chat_Mode_Private_B.BorderBrush = Brushes.Red;
                Chat_Mode_Change(2);
                Player_Position_Change();
                Window_Show();
                if (!File.Exists(Path + "/WoTB_Path.dat"))
                {
                    Sub_Code.WoTB_Get_Directory();
                }
                else
                {
                    try
                    {
                        using (var eifs = new FileStream(Path + "/WoTB_Path.dat", FileMode.Open, FileAccess.Read))
                        {
                            using (var eofs = new FileStream(Voice_Set.Special_Path + "/Temp_WoTB_Path.dat", FileMode.Create, FileAccess.Write))
                            {
                                FileEncode.FileEncryptor.Decrypt(eifs, eofs, "WoTB_Directory_Path_Pass");
                            }
                        }
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
                        {
                            Voice_Set.WoTB_Path = Read;
                        }
                    }
                    catch
                    {
                        if (!Sub_Code.WoTB_Get_Directory())
                        {
                            WoTB_Select_B.Visibility = Visibility.Visible;
                            Message_Feed_Out("WoTBのインストール先を取得できません。手動で指定してください。");
                        }
                    }
                }
                if (Voice_Set.WoTB_Path != "" && !Sub_Code.DVPL_File_Exists(Path + "/Backup/Main/sounds.yaml"))
                {
                    Directory.CreateDirectory(Path + "/Backup/Main");
                    Sub_Code.DVPL_File_Copy(Voice_Set.WoTB_Path + "/Data/sounds.yaml", Path + "/Backup/Main/sounds.yaml", false);
                    Sub_Code.DVPL_File_Copy(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_high.yaml", Path + "/Backup/Main/sfx_high.yaml", false);
                    Sub_Code.DVPL_File_Copy(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_low.yaml", Path + "/Backup/Main/sfx_low.yaml", false);
                }
                System.Drawing.Size MaxSize = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size;
                MaxWidth = MaxSize.Width;
                MaxHeight = MaxSize.Height;
                Player.settings.volume = 100;
                Version_T.Text = "V" + Version;
            }
            catch (Exception e)
            {
                StreamWriter stw = File.CreateText(Path + "/Log.txt");
                stw.Write(e.Message);
                stw.Close();
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
                catch (Exception)
                {

                }
                Directory.Delete(Path + "/Backup/Update", true);
            }
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
        }
        async void Load_Window_Set()
        {
            Load_Data_Window.Window_Start();
            bool IsOK_01 = true;
            bool IsOK_02 = true;
            bool IsOK_03 = true;
            bool IsOK_04 = true;
            Task task = Task.Run(() =>
            {
                if (!File.Exists(Voice_Set.Special_Path + "/DVPL/UnPack.py"))
                {
                    IsOK_01 = false;
                    Task task_01 = Task.Run(() =>
                    {
                        DVPL.DVPL_Unpack_Extract();
                    });
                    task_01.Wait();
                    IsOK_01 = true;
                }
                if (!File.Exists(Voice_Set.Special_Path + "/Encode_Mp3/ffmpeg.exe"))
                {
                    IsOK_02 = false;
                    Task task_01 = Task.Run(() =>
                    {
                        DVPL.Encode_Mp3_Extract();
                    });
                    task_01.Wait();
                    IsOK_02 = true;
                }
                if (!File.Exists(Voice_Set.Special_Path + "/Fmod_Designer/Fmod_designer.exe"))
                {
                    IsOK_03 = false;
                    Task task_01 = Task.Run(() =>
                    {
                        DVPL.Fmod_Designer_Extract();
                    });
                    task_01.Wait();
                    IsOK_03 = true;
                }
                if (!File.Exists(Voice_Set.Special_Path + "/SE/Capture_End_01.wav"))
                {
                    IsOK_04 = false;
                    Task task_01 = Task.Run(() =>
                    {
                        DVPL.SE_Extract();
                    });
                    task_01.Wait();
                    IsOK_04 = true;
                }
            });
            await Task.Delay(100);
            while (true)
            {
                if (IsOK_01 && IsOK_02 && IsOK_03 && IsOK_04)
                {
                    break;
                }
                await Task.Delay(100);
            }
            Load_Data_Window.Window_Stop();
        }
        //サーバーを取得
        string[] GetServerNames()
        {
            string[] Server_Lists = Server_Open_File_Line("/WoTB_Voice_Mod/Server_Names.dat");
            string[] Temp = { };
            for (int Number = 0; Number <= Server_Lists.Length - 1; Number++)
            {
                if (Server_Lists[Number] != "")
                {
                    Array.Resize(ref Temp, Temp.Length + 1);
                    Temp[Temp.Length - 1] = Server_Lists[Number];
                }
            }
            return Temp;
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
                            if (Message_Temp[1] == "Rename")
                            {
                                Voice_Set.App_Busy = true;
                                string Server_Voice_Temp = Message_Temp[3].Replace("\0", "");
                                File.Copy(Voice_Set.Special_Path + "/Server/" + Voice_Set.SRTTbacon_Server_Name + "/Voices/" + Message_Temp[2], Voice_Set.Special_Path + "/Server/" + Voice_Set.SRTTbacon_Server_Name + "/Voices/" + Server_Voice_Temp, true);
                                File.Delete(Voice_Set.Special_Path + "/Server/" + Voice_Set.SRTTbacon_Server_Name + "/Voices/" + Message_Temp[2]);
                                int Index = Voice_Set.Voice_Files.IndexOf(Message_Temp[2]);
                                if (Voice_Set.Voice_Files_Number > Index)
                                {
                                    Voice_Set.Voice_Files_Number -= 1;
                                }
                                Voice_Set.Voice_Files.RemoveAt(Index);
                                Dispatcher.Invoke(() =>
                                {
                                    if (Voice_S.Value != Voice_Set.Voice_Files_Number)
                                    {
                                        Voice_S.Value = Voice_Set.Voice_Files_Number;
                                    }
                                    else
                                    {
                                        Voice_T.Text = Voice_Set.Voice_Files[Voice_Set.Voice_Files_Number];
                                        Voice_All_Number_T.Text = Voice_Set.Voice_Files_Number + "|" + (Voice_Set.Voice_Files.Count - 1);
                                    }
                                    Voice_Set.App_Busy = false;
                                });
                            }
                            else if (Message_Temp[1] == "Remove")
                            {
                                Voice_Set.App_Busy = true;
                                string Server_Voice_Remove = Message_Temp[2].Replace("\0", "");
                                File.Delete(Voice_Set.Special_Path + "/Server/" + Voice_Set.SRTTbacon_Server_Name + "/Voices/" + Server_Voice_Remove);
                                int Index = Voice_Set.Voice_Files.IndexOf(Server_Voice_Remove);
                                Voice_Set.Voice_Files.RemoveAt(Index);
                                if (Voice_Set.Voice_Files_Number > Index)
                                {
                                    Voice_Set.Voice_Files_Number -= 1;
                                }
                                Dispatcher.Invoke(() =>
                                {
                                    if (Voice_S.Value != Voice_Set.Voice_Files_Number)
                                    {
                                        Voice_S.Value = Voice_Set.Voice_Files_Number;
                                    }
                                    else
                                    {
                                        Voice_T.Text = Voice_Set.Voice_Files[Voice_Set.Voice_Files_Number];
                                        Voice_All_Number_T.Text = Voice_Set.Voice_Files_Number + "|" + (Voice_Set.Voice_Files.Count - 1);
                                    }
                                    Voice_Set.App_Busy = false;
                                });
                            }
                            else if (Chat_Mode == 1 && Message_Temp[1] == "Chat")
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    Chat_T.Text = Server_Open_File("/WoTB_Voice_Mod/" + Voice_Set.SRTTbacon_Server_Name + "/Chat.dat");
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
                                    Chat_T.Text = Server_Open_File("/WoTB_Voice_Mod/Chat.dat");
                                });
                            }
                        }
                        else if (Message_Temp[0] == Voice_Set.UserName + "_Private")
                        {
                            if (Chat_Mode == 2 && Message_Temp[1] == "Chat")
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    Chat_T.Text = Server_Open_File("/WoTB_Voice_Mod/Users/" + Voice_Set.UserName + "_Chat.dat");
                                });
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("エラー:" + e.Message);
                }
            };
            IsConnecting();
        }
        //音声のインデックスを戻す
        private void Voice_Back_Click(object sender, RoutedEventArgs e)
        {
            if (Voice_Set.Voice_Files_Number > 0)
            {
                Voice_Set.Voice_Files_Number--;
                Voice_T.Text = Voice_Set.Voice_Files[Voice_Set.Voice_Files_Number];
                Voice_All_Number_T.Text = Voice_Set.Voice_Files_Number + "|" + (Voice_Set.Voice_Files.Count - 1);
                Voice_S.Value = Voice_Set.Voice_Files_Number;
            }
        }
        //音声のインデックスを進める
        private void Voice_Front_Click(object sender, RoutedEventArgs e)
        {
            if (Voice_Set.Voice_Files_Number < Voice_Set.Voice_Files.Count - 1)
            {
                Voice_Set.Voice_Files_Number++;
                Voice_T.Text = Voice_Set.Voice_Files[Voice_Set.Voice_Files_Number];
                Voice_All_Number_T.Text = Voice_Set.Voice_Files_Number + "|" + (Voice_Set.Voice_Files.Count - 1);
                Voice_S.Value = Voice_Set.Voice_Files_Number;
            }
        }
        //音声のインデックスをスライダーで変更
        private void Voice_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Voice_Set.Voice_Files_Number = (int)Voice_S.Value;
            Voice_T.Text = Voice_Set.Voice_Files[Voice_Set.Voice_Files_Number];
            Voice_All_Number_T.Text = Voice_Set.Voice_Files_Number + "|" + (Voice_Set.Voice_Files.Count - 1);
        }
        //音声を停止
        private void Voice_Stop_B_Click(object sender, RoutedEventArgs e)
        {
            if (Player.playState == WMPPlayState.wmppsPlaying)
            {
                Player.controls.stop();
            }
        }
        //音声を再生
        private void Voice_Play_B_Click(object sender, RoutedEventArgs e)
        {
            Player.URL = Voice_Set.Special_Path + "/Server/" + Voice_Set.SRTTbacon_Server_Name + "/Voices/" + Voice_Set.Voice_Files[Voice_Set.Voice_Files_Number];
            Player.controls.play();
        }
        //音声の位置を反映
        async void Player_Position_Change()
        {
            while (true)
            {
                if (Player.playState == WMPPlayState.wmppsPlaying || Player.playState == WMPPlayState.wmppsPaused)
                {
                    Voice_Location_S.Maximum = (int)(Player.controls.currentItem.duration * 100);
                    if (Voice_Location_S.IsFocused == false)
                    {
                        try
                        {
                            Voice_Location_S.Value = (int)(Player.controls.currentPosition * 100);
                        }
                        catch
                        {

                        }
                    }
                }
                await Task.Delay(1000 / 60);
            }
        }
        //音声の位置を指定
        private void Voice_Location_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Voice_Location_S.IsFocused)
            {
                Player.controls.pause();
                Player.controls.currentPosition = Voice_Location_S.Value / 100;
            }
            else
            {
                Player.controls.play();
            }
        }
        //音量を変更
        private void Voice_Volume_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Player.settings.volume = (int)Voice_Volume_S.Value;
        }
        //キャッシュを削除
        private void Cache_Delete_B_Click(object sender, RoutedEventArgs e)
        {
            string Message_01 = "キャッシュを削除すると、サーバーからダウンロードした音声がなくなりますので再度ダウンロードする必要があります。\nこの操作は取り消せません。よろしいですか？";
            MessageBoxResult result = MessageBox.Show(Message_01, "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                if (Directory.Exists(Voice_Set.Special_Path + "/Server"))
                {
                    try
                    {
                        Directory.Delete(Voice_Set.Special_Path + "/Server", true);
                        Directory.CreateDirectory(Voice_Set.Special_Path + "/Server");
                        MessageBox.Show("キャッシュを削除しました。");
                    }
                    catch (Exception m)
                    {
                        MessageBox.Show("キャッシュを削除できませんでした。ファイルが使用中でないか確認してください。\nエラーコード:" + m.Message);
                    }
                }
                else
                {
                    MessageBox.Show("既に削除されています。");
                    Directory.CreateDirectory(Voice_Set.Special_Path + "/Server");
                }
            }
        }
        //サーバーをクリックしたときにそのサーバーの情報を取得
        private void Server_Lists_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (Server_Lists.SelectedIndex != -1)
            {
                Explanation_Scrool.Visibility = Visibility.Visible;
                Explanation_Text.Visibility = Visibility.Visible;
                Explanation_Border.Visibility = Visibility.Visible;
                XDocument xml2 = XDocument.Load(Voice_Set.FTP_Server.OpenRead("/WoTB_Voice_Mod/" + Server_Names_List[Server_Lists.SelectedIndex] + "/Server_Config.dat"));
                XElement item2 = xml2.Element("Server_Create_Config");
                if (bool.Parse(item2.Element("IsEnablePassword").Value))
                {
                    Server_Connect_B.Margin = new Thickness(-600, 500, 0, 0);
                    Server_Create_B.Margin = new Thickness(-600, 650, 0, 0);
                    Password_Text.Visibility = Visibility.Visible;
                    Password_T.Visibility = Visibility.Visible;
                }
                else
                {
                    Server_Connect_B.Margin = new Thickness(-600, 375, 0, 0);
                    Server_Create_B.Margin = new Thickness(-600, 550, 0, 0);
                    Password_Text.Visibility = Visibility.Hidden;
                    Password_T.Visibility = Visibility.Hidden;
                }
                Explanation_T.Text = item2.Element("Explanation").Value;
                Server_Create_Name_T.Text = "制作者:" + item2.Element("Master_User_Name").Value;
                Server_Create_Name_T.Visibility = Visibility.Visible;
            }
        }
        //リストボックスの空欄をクリックするとサーバーを未選択にする
        private void Server_Lists_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Server_Lists.SelectedIndex = -1;
            Server_Connect_B.Margin = new Thickness(-600, 375, 0, 0);
            Server_Create_B.Margin = new Thickness(-600, 550, 0, 0);
            Password_Text.Visibility = Visibility.Hidden;
            Password_T.Visibility = Visibility.Hidden;
            Explanation_Scrool.Visibility = Visibility.Hidden;
            Explanation_Text.Visibility = Visibility.Hidden;
            Explanation_Border.Visibility = Visibility.Hidden;
            Server_Create_Name_T.Visibility = Visibility.Hidden;
        }
        //サーバーリストを更新
        private void Server_List_Update_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing)
            {
                return;
            }
            Server_List_Reset();
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
                    using (var eifs = new FileStream(Voice_Set.Special_Path + "/Temp_User_Set.dat", FileMode.Open, FileAccess.Read))
                    {
                        using (var eofs = new FileStream(Path + "/User.dat", FileMode.Create, FileAccess.Write))
                        {
                            FileEncode.FileEncryptor.Encrypt(eifs, eofs, "SRTTbacon_Server_User_Pass_Save");
                        }
                    }
                    File.Delete(Voice_Set.Special_Path + "/Temp_User_Set.dat");
                    if (Login())
                    {
                        Connectiong = true;
                        Connect_Start();
                        Connect_Mode_Layout();
                        Chat_Mode_Change(2);
                    }
                }
                else
                {
                    MessageBox.Show("ユーザー名またはパスワードが間違えています。");
                }
            }
            else
            {
                MessageBox.Show("サーバーに接続されませんでした。時間を空けて再接続ボタンを押してみてください。");
            }
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
                using (var eifs = new FileStream(Voice_Set.Special_Path + "/Temp_User_Set.dat", FileMode.Open, FileAccess.Read))
                {
                    using (var eofs = new FileStream(Path + "/User.dat", FileMode.Create, FileAccess.Write))
                    {
                        FileEncode.FileEncryptor.Encrypt(eifs, eofs, "SRTTbacon_Server_User_Pass_Save");
                    }
                }
                File.Delete(Voice_Set.Special_Path + "/Temp_User_Chat.dat");
                File.Delete(Voice_Set.Special_Path + "/Temp_User_Set.dat");
                if (Login())
                {
                    Connectiong = true;
                    Connect_Start();
                    Connect_Mode_Layout();
                }
            }
            else
            {
                MessageBox.Show("サーバーに接続されませんでした。時間を空けて再接続ボタンを押してみてください。");
            }
        }
        //管理者画面に移動
        private void Administrator_B_Click(object sender, RoutedEventArgs e)
        {
            Administrator_Window.Window_Show();
        }
        //音声のタイプを変更
        private void Voice_Type_C_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (Voice_Set.SRTTbacon_Server_Name != "")
            {
                if (Voice_Type_C.SelectedIndex == 0)
                {
                    Voice_Control_Sub_Window.Visibility = Visibility.Hidden;
                    Voice_Control_Window.Visibility = Visibility.Visible;
                }
                else
                {
                    Voice_Control_Window.Visibility = Visibility.Hidden;
                    Voice_Control_Sub_Window.Visibility = Visibility.Visible;
                }
            }
        }
        private void Chat_Send_B_Click(object sender, RoutedEventArgs e)
        {
            if (Chat_Send_T.Text == "")
            {
                return;
            }
            //現在のバージョンでは使用できません。
            if (Chat_Mode == 1)
            {
                return;
            }
            if (Chat_Send_T.Text.CountOf("  ") >= 1)
            {
                MessageBox.Show("スパム防止のため空白を2回連続で使用できません。");
                return;
            }
            if (Chat_Send_T.Text.Length >= 50)
            {
                MessageBox.Show("文字数が多すぎます。50文字以下にしてください。");
                return;
            }
            if (Chat_Mode == 0)
            {
                Voice_Set.AppendString("/WoTB_Voice_Mod/Chat.dat", Encoding.UTF8.GetBytes(Voice_Set.UserName + ":" + Chat_Send_T.Text + "\n"));
                Voice_Set.TCP_Server.WriteLine("Public|Chat|" + Voice_Set.UserName + ":" + Chat_Send_T.Text + '\0');
            }
            else if (Chat_Mode == 1)
            {
                Voice_Set.AppendString("/WoTB_Voice_Mod/" + Voice_Set.SRTTbacon_Server_Name + "/Chat.dat", Encoding.UTF8.GetBytes(Voice_Set.UserName + ":" + Chat_Send_T.Text + "\n"));
                Voice_Set.TCP_Server.WriteLine(Voice_Set.SRTTbacon_Server_Name + "|Chat|" + Voice_Set.UserName + ":" + Chat_Send_T.Text + '\0');
            }
            else if (Chat_Mode == 2)
            {
                Voice_Set.AppendString("/WoTB_Voice_Mod/Users/" + Voice_Set.UserName + "_Chat.dat", Encoding.UTF8.GetBytes(Voice_Set.UserName + ":" + Chat_Send_T.Text + "\n"));
                Voice_Set.TCP_Server.WriteLine(Voice_Set.UserName + "_Private|Chat|" + Voice_Set.UserName + ":" + Chat_Send_T.Text + '\0');
            }
            Chat_Scrool.ScrollToEnd();
            Chat_Send_T.Text = "";
        }
        private void Voice_Delete_B_Click(object sender, RoutedEventArgs e)
        {
            if (Voice_Set.App_Busy)
            {
                return;
            }
            Voice_Set.FTP_Server.DeleteFile("/WoTB_Voice_Mod/" + Voice_Set.SRTTbacon_Server_Name + "/Voices/" + Voice_Set.Voice_Files[Voice_Set.Voice_Files_Number]);
            Voice_Set.TCP_Server.WriteLine(Voice_Set.SRTTbacon_Server_Name + "|Remove|" + Voice_Set.Voice_Files[Voice_Set.Voice_Files_Number] + '\0');
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
                    BrushConverter bc = new BrushConverter();
                    Chat_Mode = 0;
                    Chat_Mode_Public_B.Background = (Brush)bc.ConvertFrom("#59999999");
                    Chat_Mode_Server_B.Background = Brushes.Transparent;
                    Chat_Mode_Private_B.Background = Brushes.Transparent;
                    Chat_Mode_Public_B.BorderBrush = Brushes.Red;
                    Chat_Mode_Server_B.BorderBrush = Brushes.Transparent;
                    Chat_Mode_Private_B.BorderBrush = Brushes.Transparent;
                    Chat_T.Text = Server_Open_File("/WoTB_Voice_Mod/Chat.dat");
                }
                else if (Mode_Number == 1)
                {
                    BrushConverter bc = new BrushConverter();
                    Chat_Mode = 1;
                    Chat_Mode_Public_B.Background = Brushes.Transparent;
                    Chat_Mode_Server_B.Background = (Brush)bc.ConvertFrom("#59999999");
                    Chat_Mode_Private_B.Background = Brushes.Transparent;
                    Chat_Mode_Public_B.BorderBrush = Brushes.Transparent;
                    Chat_Mode_Server_B.BorderBrush = Brushes.Red;
                    Chat_Mode_Private_B.BorderBrush = Brushes.Transparent;
                    //Chat_T.Text = Server_Open_File("/WoTB_Voice_Mod/" + Voice_Set.SRTTbacon_Server_Name + "/Chat.dat");
                    Chat_T.Text = "---現在のバージョンでは使用できません。---";
                }
                else if (Mode_Number == 2)
                {
                    BrushConverter bc = new BrushConverter();
                    Chat_Mode = 2;
                    Chat_Mode_Public_B.Background = Brushes.Transparent;
                    Chat_Mode_Server_B.Background = Brushes.Transparent;
                    Chat_Mode_Private_B.Background = (Brush)bc.ConvertFrom("#59999999");
                    Chat_Mode_Public_B.BorderBrush = Brushes.Transparent;
                    Chat_Mode_Server_B.BorderBrush = Brushes.Transparent;
                    Chat_Mode_Private_B.BorderBrush = Brushes.Red;
                    Chat_T.Text = Server_Open_File("/WoTB_Voice_Mod/Users/" + Voice_Set.UserName + "_Chat.dat");
                }
            }
        }
        async Task Loading_Show()
        {
            Load_Image.Visibility = Visibility.Visible;
            int File_Number = 1;
            WindowsMediaPlayer SE_Load = new WindowsMediaPlayer
            {
                URL = Voice_Set.Special_Path + "/Loading/Load_Voice.mp3"
            };
            SE_Load.settings.volume = 25;
            SE_Load.controls.play();
            Random r = new Random();
            int Max_Number = r.Next(100, 149);
            while (File_Number <= Max_Number)
            {
                Load_Image.Source = new BitmapImage(new Uri(Voice_Set.Special_Path + "/Loading/" + File_Number + ".png"));
                File_Number++;
                await Task.Delay(1000 / 30);
            }
            SE_Load.close();
            Load_Image.Visibility = Visibility.Hidden;
        }
        private void Back_B_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("メニュー画面に戻りますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                Voice_S.Visibility = Visibility.Hidden;
                Voice_Back_B.Visibility = Visibility.Hidden;
                Voice_Front_B.Visibility = Visibility.Hidden;
                Voice_Stop_B.Visibility = Visibility.Hidden;
                Voice_Play_B.Visibility = Visibility.Hidden;
                Voice_Location_S.Visibility = Visibility.Hidden;
                Voice_Volume_S.Visibility = Visibility.Hidden;
                Voice_Control_Window.Visibility = Visibility.Hidden;
                Voice_Type_C.Visibility = Visibility.Hidden;
                Voice_Delete_B.Visibility = Visibility.Hidden;
                Voice_Type_Border.Visibility = Visibility.Hidden;
                Back_B.Visibility = Visibility.Hidden;
                Save_B.Visibility = Visibility.Hidden;
                Administrator_B.Visibility = Visibility.Hidden;
                Voice_T.Text = "";
                Voice_All_Number_T.Text = "";
                Chat_T.Text = "";
                Voice_Set.SRTTbacon_Server_Name = "";
                Voice_Set.Voice_Files = new List<string>();
                Voice_Set.Voice_Files_Number = 0;
                //Voice_S.Value = 0;
                //Voice_S.Maximum = 1;
                Player.settings.volume = 100;
                Chat_Hide();
                Server_Connect_B.Margin = new Thickness(-600, 375, 0, 0);
                Server_Create_B.Margin = new Thickness(-600, 550, 0, 0);
                Password_Text.Visibility = Visibility.Hidden;
                Password_T.Visibility = Visibility.Hidden;
                Explanation_Scrool.Visibility = Visibility.Hidden;
                Explanation_Text.Visibility = Visibility.Hidden;
                Explanation_Border.Visibility = Visibility.Hidden;
                Server_Create_Name_T.Visibility = Visibility.Hidden;
                Server_List_Reset();
                Server_Lists.Visibility = Visibility.Visible;
                Server_Connect_B.Visibility = Visibility.Visible;
                Server_Create_B.Visibility = Visibility.Visible;
                Cache_Delete_B.Visibility = Visibility.Visible;
                Server_List_Update_B.Visibility = Visibility.Visible;
            }
        }
        private void Voice_Mod_Free_B_Click(object sender, RoutedEventArgs e)
        {
            Voice_Mods_Window.Window_Show();
        }
        private void Tool_B_Click(object sender, RoutedEventArgs e)
        {
            Tools_Window.Window_Show();
        }
        private async void Update_B_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StreamReader str = new StreamReader(Voice_Set.FTP_Server.OpenRead("/WoTB_Voice_Mod/Update/Configs.dat"));
                string Line = str.ReadLine();
                str.Close();
                if (Line == Version)
                {
                    Message_Feed_Out("既に最新のバージョンです。");
                }
                else
                {
                    MessageBoxResult result = MessageBox.Show("新しいバージョンが見つかりました(V" + Line + ")。ダウンロードして適応しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes);
                    if (result == MessageBoxResult.Yes)
                    {
                        Voice_Set.App_Busy = true;
                        Message_T.Text = "ダウンロード中です。処理が完了したらソフトを再起動します。";
                        await Task.Delay(50);
                        if (Directory.Exists(Path + "/Backup/Update"))
                        {
                            Directory.Delete(Path + "/Backup/Update", true);
                        }
                        Directory.CreateDirectory(Path + "/Backup/Update");
                        IsProcessing = true;
                        Voice_Set.FTP_Server.DownloadDirectory(Voice_Set.Special_Path + "/Update", "/WoTB_Voice_Mod/Update/" + Line, FtpFolderSyncMode.Update, FtpLocalExists.Overwrite);
                        bool IsReboot = false;
                        string[] Dir_Update = Directory.GetFiles(Voice_Set.Special_Path + "/Update", "*", SearchOption.AllDirectories);
                        foreach (string File_Now in Dir_Update)
                        {
                            if (System.IO.Path.GetFileName(File_Now) == "WoTB_Voice_Mod_Creater.exe")
                            {
                                IsReboot = true;
                            }
                            string Dir_Only = File_Now.Replace(System.IO.Path.GetFileName(File_Now), "");
                            string Temp_01 = Dir_Only.Replace("/", "\\");
                            string File_Dir = Temp_01.Replace(Voice_Set.Special_Path.Replace("/", "\\") + "\\Update\\", "");
                            if (File_Dir.Contains("\\"))
                            {
                                if (!Directory.Exists(Path + "/" + File_Dir))
                                {
                                    Directory.CreateDirectory(Path + "/" + File_Dir);
                                }
                            }
                            if (File.Exists(Path + "/" + File_Dir + System.IO.Path.GetFileName(File_Now)))
                            {
                                if (File_Dir.Contains("\\"))
                                {
                                    if (!Directory.Exists(Path + "/Backup/Update/" + File_Dir))
                                    {
                                        Directory.CreateDirectory(Path + "/Backup/Update/" + File_Dir);
                                    }
                                    File.Move(Path + "/" + File_Dir + System.IO.Path.GetFileName(File_Now), Path + "/Backup/Update/" + System.IO.Path.GetFileName(File_Now));
                                }
                                else
                                {
                                    File.Move(Path + "/" + System.IO.Path.GetFileName(File_Now), Path + "/Backup/Update/" + System.IO.Path.GetFileName(File_Now));
                                }
                            }
                            File.Copy(File_Now, Path + "/" + File_Dir + System.IO.Path.GetFileName(File_Now), true);
                        }
                        Directory.Delete(Voice_Set.Special_Path + "/Update", true);
                        if (IsReboot)
                        {
                            Process.Start(Path + "/WoTB_Voice_Mod_Creater.exe", "/up " + Process.GetCurrentProcess().Id);
                            Close();
                        }
                        Voice_Set.App_Busy = false;
                        IsProcessing = false;
                    }
                }
            }
            catch
            {
                Voice_Set.App_Busy = false;
                IsProcessing = false;
                Message_Feed_Out("最新バージョンを取得できませんでした。");
            }
        }
        private void Other_B_Click(object sender, RoutedEventArgs e)
        {
            Other_Window.Window_Show();
        }
        private void Message_B_Click(object sender, RoutedEventArgs e)
        {
            Message_Window.Window_Show();
        }
        private void Voice_Create_Tool_B_Click(object sender, RoutedEventArgs e)
        {
            Voice_Create_Window.Window_Show();
        }
        private void WoTB_Select_B_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Title = "WoTBのフォルダを選択してください。",
                InitialDirectory = "C:",
                FileName = "WoTBのフォルダを選択",
                Filter = "フォルダ |.",
                CheckFileExists = false
            };
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (System.IO.Path.GetDirectoryName(openFileDialog.FileName) == "C:\\")
                {
                    Message_Feed_Out("C:/を選択することはできません。");
                    return;
                }
                ApplyAllFiles(System.IO.Path.GetDirectoryName(openFileDialog.FileName), ProcessFile);
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
                using (var eifs = new FileStream(Voice_Set.Special_Path + "/Temp_WoTB_Path.dat", FileMode.Open, FileAccess.Read))
                {
                    using (var eofs = new FileStream(Path + "/WoTB_Path.dat", FileMode.Create, FileAccess.Write))
                    {
                        FileEncode.FileEncryptor.Encrypt(eifs, eofs, "WoTB_Directory_Path_Pass");
                    }
                }
                File.Delete(Voice_Set.Special_Path + "/Temp_WoTB_Path.dat");
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
                catch
                {
                    
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
        }
        private async void DockPanel_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //アンインストール
            if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Shift) == System.Windows.Forms.Keys.Shift && e.Key == System.Windows.Input.Key.Escape)
            {
                if (IsClosing || Voice_Set.App_Busy)
                {
                    return;
                }
                IsClosing = true;
                MessageBoxResult result = MessageBox.Show("ソフトをアンインストールしますか？これには一時ファイルも削除されます。", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
                if (result == MessageBoxResult.Yes)
                {
                    Voice_Set.App_Busy = true;
                    Message_T.Text = "一時ファイルを削除しています...";
                    await Task.Delay(50);
                    try
                    {
                        string[] Files = Directory.GetFiles(Voice_Set.Special_Path, "*", SearchOption.AllDirectories);
                        foreach (string File_Name in Files)
                        {
                            try
                            {
                                File.Delete(File_Name);
                            }
                            catch
                            {

                            }
                        }
                    }
                    catch
                    {

                    }
                    try
                    {
                        if (Voice_Set.FTP_Server.IsConnected)
                        {
                            Voice_Set.FTP_Server.Disconnect();
                            Voice_Set.FTP_Server.Dispose();
                        }
                    }
                    catch
                    {

                    }
                    int Minus = (int)(Voice_Volume_S.Value / 40);
                    while (Opacity > 0)
                    {
                        Player.settings.volume -= Minus;
                        Opacity -= 0.025;
                        await Task.Delay(1000 / 60);
                    }
                    StreamWriter stw = File.CreateText(System.IO.Path.GetTempPath() + "/WoTB_Voice_Mod_Creater_Remove_01.bat");
                    stw.Write("timeout 2\nrd /s /q \"" + Path + "\"\ndel %0");
                    stw.Close();
                    ProcessStartInfo processStartInfo = new ProcessStartInfo
                    {
                        FileName = System.IO.Path.GetTempPath() + "/WoTB_Voice_Mod_Creater_Remove_01.bat",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    Process.Start(processStartInfo);
                    StreamWriter stw2 = File.CreateText(System.IO.Path.GetTempPath() + "/WoTB_Voice_Mod_Creater_Remove_02.bat");
                    stw2.Write("timeout 2\nrd /s /q \"" + Voice_Set.Special_Path + "\"\ndel %0");
                    stw2.Close();
                    ProcessStartInfo processStartInfo2 = new ProcessStartInfo
                    {
                        FileName = System.IO.Path.GetTempPath() + "/WoTB_Voice_Mod_Creater_Remove_02.bat",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    Process.Start(processStartInfo2);
                    Application.Current.Shutdown();
                }
            }
            //一時ファイルの保存場所を指定
            if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Shift) == System.Windows.Forms.Keys.Shift && e.Key == System.Windows.Input.Key.D)
            {
                MessageBoxResult result = MessageBox.Show("一時ファイルの保存先を変更しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
                if (result == MessageBoxResult.Yes)
                {
                    System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog()
                    {
                        Title = "フォルダを選択してください。",
                        Filter = "フォルダを選択(Folder;)|",
                        FileName = "フォルダを指定",
                        CheckFileExists = false
                    };
                    if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        string Dir = System.IO.Path.GetDirectoryName(ofd.FileName);
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
                        Sub_Code.Directory_Copy(Voice_Set.Special_Path, Dir);
                        try
                        {
                            string[] Files = Directory.GetFiles(Voice_Set.Special_Path, "*", SearchOption.AllDirectories);
                            foreach (string File_Name in Files)
                            {
                                try
                                {
                                    File.Delete(File_Name);
                                    if (Directory.GetFiles(System.IO.Path.GetDirectoryName(File_Name), "*", SearchOption.TopDirectoryOnly).Length == 0)
                                    {
                                        Directory.Delete(System.IO.Path.GetDirectoryName(File_Name), true);
                                    }
                                }
                                catch
                                {

                                }
                            }
                            Directory.Delete(Voice_Set.Special_Path, true);
                        }
                        catch
                        {

                        }
                        StreamWriter stw = File.CreateText(Dir + "/TempDirPath.dat");
                        stw.Write(Dir);
                        stw.Close();
                        using (var eifs = new FileStream(Dir + "/TempDirPath.dat", FileMode.Open, FileAccess.Read))
                        {
                            using (var eofs = new FileStream(Path + "/TempDirPath.dat", FileMode.Create, FileAccess.Write))
                            {
                                FileEncode.FileEncryptor.Encrypt(eifs, eofs, "Temp_Directory_Path_Pass");
                            }
                        }
                        File.Delete(Dir + "/TempDirPath.dat");
                        Voice_Set.Special_Path = Dir;
                        Message_Feed_Out("一時ファイルの保存先を変更しました。");
                    }
                }
            }
        }
    }
}