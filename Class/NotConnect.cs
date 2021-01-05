using FluentFTP;
using SimpleTCP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace WoTB_Voice_Mod_Creater
{
    public partial class MainCode : Window
    {
        bool Connectiong = false;
        bool IsProcessing = false;
        bool Server_OK = false;
        readonly FtpDataConnectionType ConnectType;
        readonly bool IsPassiveMode = false;
        //サーバーに接続(参加ではない)
        void Server_Connect()
        {
            try
            {
                Voice_Set.TCP_Server = new SimpleTcpClient().Connect(IP, 50000);
                Voice_Set.TCP_Server.StringEncoder = Encoding.UTF8;
                Voice_Set.TCP_Server.Delimiter = 0x00;
                Voice_Set.FTP_Server = new FtpClient(IP)
                {
                    Credentials = new NetworkCredential("非公開", "非公開"),
                    SocketKeepAlive = false,
                    DataConnectionType = ConnectType,
                    SslProtocols = SslProtocols.Tls,
                    ConnectTimeout = 3000,
                };
                Voice_Set.FTP_Server.ValidateCertificate += new FtpSslValidation(OnValidateCertificate);
                Voice_Set.FTP_Server.Connect();
                Voice_Set.Create_File_Server("/WoTB_Voice_Mod/Mods/Mod_Names.dat", false, true);
                Server_OK = true;
                Message_T.Text = "";
                if (Login())
                {
                    Connectiong = true;
                    Connect_Start();
                    Connect_Mode_Layout();
                }
                else
                {
                    Connect_B.Visibility = Visibility.Hidden;
                    User_Name_Text.Visibility = Visibility.Visible;
                    User_Name_T.Visibility = Visibility.Visible;
                    User_Password_Text.Visibility = Visibility.Visible;
                    User_Password_T.Visibility = Visibility.Visible;
                    User_Login_B.Visibility = Visibility.Visible;
                    User_Register_B.Visibility = Visibility.Visible;
                    Voice_Create_Tool_B.Visibility = Visibility.Visible;
                }
            }
            catch
            {
                Connectiong = false;
                Server_OK = false;
                Connect_Mode_Layout();
                Message_T.Text = "エラー:サーバーが開いていない可能性があります。";
            }
        }
        //ログインできるか
        bool Login()
        {
            if (File.Exists(Path + "/User.dat"))
            {
                try
                {
                    using (var eifs = new FileStream(Path + "/User.dat", FileMode.Open, FileAccess.Read))
                    {
                        using (var eofs = new FileStream(Special_Path + "/Temp_User.dat", FileMode.Create, FileAccess.Write))
                        {
                            FileEncode.FileEncryptor.Decrypt(eifs, eofs, "SRTTbacon_Server_User_Pass_Save");
                        }
                    }
                    StreamReader str = new StreamReader(Special_Path + "/Temp_User.dat");
                    string Login_Read = str.ReadLine();
                    str.Close();
                    File.Delete(Special_Path + "/Temp_User.dat");
                    string User_Name = Login_Read.Substring(0, Login_Read.IndexOf(':'));
                    string Password = Login_Read.Substring(Login_Read.IndexOf(':') + 1);
                    if (Account_Exist(User_Name, Password))
                    {
                        Voice_Set.UserName = User_Name;
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
            else
            {
                return false;
            }
        }
        //参加ボタン
        private async void Server_Connect_B_Click(object sender, RoutedEventArgs e)
        {
            Message_T.Visibility = Visibility.Visible;
            if (IsProcessing)
            {
                return;
            }
            if (Server_Lists.SelectedIndex == -1)
            {
                Message_T.Text = "サーバーが選択されていません。";
                return;
            }
            else
            {
                string Directory_Name = Server_Names_List[Server_Lists.SelectedIndex].ToString() + "/Voices/";
                if (Directory.Exists(Special_Path + "/Server/" + Directory_Name) && Directory.GetFiles(Special_Path + "/Server/" + Directory_Name).Length == 0)
                {
                    Directory.Delete(Special_Path + "/Server/" + Directory_Name, true);
                }
                XDocument xml2 = XDocument.Load(Voice_Set.FTP_Server.OpenRead("/WoTB_Voice_Mod/" + Server_Names_List[Server_Lists.SelectedIndex] + "/Server_Config.dat"));
                XElement item2 = xml2.Element("Server_Create_Config");
                if (bool.Parse(item2.Element("IsEnablePassword").Value))
                {
                    if (Password_T.Text != item2.Element("Password").Value)
                    {
                        Message_T.Text = "パスワードが違います。";
                        return;
                    }
                }
                Server_Connect_B.Visibility = Visibility.Hidden;
                Server_Create_B.Visibility = Visibility.Hidden;
                Cache_Delete_B.Visibility = Visibility.Hidden;
                Password_Text.Visibility = Visibility.Hidden;
                Password_T.Visibility = Visibility.Hidden;
                Voice_Mod_Free_B.Visibility = Visibility.Hidden;
                Message_B.Visibility = Visibility.Hidden;
                Update_B.Visibility = Visibility.Hidden;
                Server_List_Update_B.Visibility = Visibility.Hidden;
                Message_T.Opacity = 1;
                Message_T.Visibility = Visibility.Visible;
                if (IsSRTTbacon_V1)
                {
                    try
                    {
                        Message_T.Text = "サーバーから直接適応します...";
                        await Task.Delay(50);
                        if (Directory.Exists(Special_Path + "/Server/" + Server_Names_List[Server_Lists.SelectedIndex]))
                        {
                            Directory.Delete(Special_Path + "/Server/" + Server_Names_List[Server_Lists.SelectedIndex], true);
                        }
                        await Task.Delay(50);
                        Directory.CreateDirectory(Special_Path + "/Server/" + Server_Names_List[Server_Lists.SelectedIndex].ToString() + "/Voices");
                        string[] Files = Directory.GetFiles("E:/SRTTbacon_Server/WoTB_Voice_Mod/" + Directory_Name, "*", SearchOption.TopDirectoryOnly);
                        for (int Number = 0; Number <= Files.Length - 1; Number++)
                        {
                            string File_Name = System.IO.Path.GetFileName(Files[Number]);
                            File.Copy(Files[Number], Special_Path + "/Server/" + Directory_Name + File_Name, true);
                        }
                        Message_T.Text = "ロード中です。しばらくお待ちください。";
                    }
                    catch (Exception e1)
                    {
                        MessageBox.Show(e1.Message);
                    }
                }
                else if (Directory.Exists(Special_Path + "/Server/" + Directory_Name))
                {
                    Message_T.Text = "キャッシュから適応します...";
                    await Task.Delay(100);
                    Voice_Set.FTP_Server.DownloadFile(Special_Path + "/Temp_Download_Change_Names.dat", "/WoTB_Voice_Mod/" + Server_Names_List[Server_Lists.SelectedIndex] + "/Change_Names.dat");
                    try
                    {
                        List<string> Change_Names_Read = new List<string>();
                        StreamReader file = new StreamReader(Special_Path + "/Temp_Download_Change_Names.dat");
                        while (file.EndOfStream == false)
                        {
                            Change_Names_Read.Add(file.ReadLine());
                        }
                        file.Close();
                        File.Delete(Special_Path + "/Temp_Download_Change_Names.dat");
                        foreach (string Line in Change_Names_Read)
                        {
                            if (Line.Contains("->"))
                            {
                                string From_Name = Line.Substring(0, Line.IndexOf('>') - 1);
                                string To_Name = Line.Substring(Line.IndexOf('>') + 1);
                                File.Move(Special_Path + "/Server/" + Directory_Name + From_Name, Special_Path + "/Server/" + Directory_Name + To_Name);
                            }
                        }
                    }
                    catch
                    {

                    }
                    Message_T.Text = "ロード中です。しばらくお待ちください。";
                }
                else
                {
                    Voice_S.Value = 0;
                    Download_Progress_P.Visibility = Visibility.Visible;
                    Download_Progress_T.Visibility = Visibility.Visible;
                    Message_T.Opacity = 1;
                    Message_T.Text = "サーバーから音声をダウンロードしています...";
                    Download_Progress_T.Text = "計算しています...";
                    IsProcessing = true;
                    Directory.CreateDirectory(Special_Path + "/Server/" + Directory_Name);
                    await Task.Delay(50);
                    List<string> strList = new List<string>();
                    FtpWebRequest fwr = (FtpWebRequest)WebRequest.Create(new Uri("ftp://" + IP + "/WoTB_Voice_Mod/" + Directory_Name));
                    fwr.UsePassive = IsPassiveMode;
                    fwr.Credentials = new NetworkCredential("SRTTbacon_Server", "SRTTbacon");
                    fwr.Method = WebRequestMethods.Ftp.ListDirectory;
                    StreamReader sr = new StreamReader(fwr.GetResponse().GetResponseStream());
                    string str = sr.ReadLine();
                    while (str != null)
                    {
                        strList.Add(str);
                        str = sr.ReadLine();
                    }
                    sr.Close();
                    fwr.Abort();
                    int Max_Count = strList.Count;
                    int Now_Count = 0;
                    Download_Progress_P.Value = 0;
                    Download_Progress_P.Maximum = Max_Count;
                    foreach (string File_Name in strList)
                    {
                        try
                        {
                            Voice_Set.FTP_Server.DownloadFile(Special_Path + "/Server/" + Directory_Name + File_Name, "/WoTB_Voice_Mod/" + Directory_Name + File_Name);
                            await Task.Delay(1);
                            Download_Progress_P.Value = Now_Count;
                            Download_Progress_T.Text = Now_Count + "/" + Max_Count;
                            Now_Count++;
                        }
                        catch
                        {

                        }
                    }
                    //Server_Directory_Download("/WoTB_Voice_Mod/" + Server_Lists.Items[Server_Lists.SelectedIndex].ToString() + "/Voices", Special_Path + "/Server/Voices");
                    Message_T.Text = "音声のダウンロードが完了しました。適応します。";
                }
                if (Directory.Exists(Special_Path + "/Server/" + Voice_Set.SRTTbacon_Server_Name + "/Voices/SE"))
                {
                    string[] Files = Directory.GetFiles(Special_Path + "/Server/" + Directory_Name + "SE", "*", SearchOption.TopDirectoryOnly);
                    if (Files.Length == 0)
                    {
                        Voice_Set.FTP_Server.DownloadDirectory(Special_Path + "/Server/" + Directory_Name + "SE", "/WoTB_Voice_Mod/SE");
                    }
                }
                else
                {
                    Voice_Set.FTP_Server.DownloadDirectory(Special_Path + "/Server/" + Directory_Name + "SE", "/WoTB_Voice_Mod/SE");
                }
                Server_Connect_B.Margin = new Thickness(-600, 375, 0, 0);
                Server_Create_B.Margin = new Thickness(-600, 550, 0, 0);
                await Loading_Show();
                Server_Lists.Visibility = Visibility.Hidden;
                Server_Create_Name_T.Visibility = Visibility.Hidden;
                Download_Progress_P.Visibility = Visibility.Hidden;
                Download_Progress_T.Visibility = Visibility.Hidden;
                Explanation_Scrool.Visibility = Visibility.Hidden;
                Explanation_Text.Visibility = Visibility.Hidden;
                Explanation_Border.Visibility = Visibility.Hidden;
                await Task.Delay(50);
                string[] Temp = Directory.GetFiles(Special_Path + "/Server/" + Directory_Name);
                Voice_S.Visibility = Visibility.Visible;
                Voice_Back_B.Visibility = Visibility.Visible;
                Voice_Front_B.Visibility = Visibility.Visible;
                Voice_Stop_B.Visibility = Visibility.Visible;
                Voice_Play_B.Visibility = Visibility.Visible;
                Voice_Location_S.Visibility = Visibility.Visible;
                Voice_Volume_S.Visibility = Visibility.Visible;
                Voice_Control_Window.Visibility = Visibility.Visible;
                Voice_Type_C.Visibility = Visibility.Visible;
                Voice_Delete_B.Visibility = Visibility.Visible;
                Voice_Type_Border.Visibility = Visibility.Visible;
                Back_B.Visibility = Visibility.Visible;
                Chat_Show();
                Save_B.Visibility = Visibility.Visible;
                if (Voice_Set.UserName == item2.Element("Master_User_Name").Value)
                {
                    Administrator_B.Visibility = Visibility.Visible;
                }
                Voice_S.Minimum = 0;
                List<string> Temp_01 = new List<string>();
                for (int Number = 0; Number <= Temp.Length - 1; Number++)
                {
                    if (!Voice_Set.Voice_Name_Hide(Temp[Number]))
                    {
                        Temp_01.Add(System.IO.Path.GetFileName(Temp[Number]));
                    }
                }
                Voice_Set.Voice_Files = Temp_01;
                Voice_S.Maximum = Voice_Set.Voice_Files.Count - 1;
                Voice_T.Text = Voice_Set.Voice_Files[(int)Voice_S.Value];
                Voice_All_Number_T.Text = Voice_Set.Voice_Files_Number + "|" + (Voice_Set.Voice_Files.Count - 1);
                Voice_Set.SRTTbacon_Server_Name = Server_Names_List[Server_Lists.SelectedIndex].ToString();
                Server_Lists.SelectedIndex = -1;
                string Chat_Temp = Server_Open_File("/WoTB_Voice_Mod/" + Voice_Set.SRTTbacon_Server_Name + "/Chat.dat");
                if (Voice_Set.UserName == "SRTTbacon")
                {
                    if (Chat_Temp.Contains(Voice_Set.UserName + "(管理者)が参加しました。"))
                    {
                        Voice_Set.AppendString("/WoTB_Voice_Mod/" + Voice_Set.SRTTbacon_Server_Name + "/Chat.dat", Encoding.UTF8.GetBytes(Voice_Set.UserName + "(管理者)が参加しました。\n"));
                    }
                }
                else if (Voice_Set.UserName == item2.Element("Master_User_Name").Value)
                {
                    if (Chat_Temp.Contains(Voice_Set.UserName + "(管理者)が参加しました。"))
                    {
                        Voice_Set.AppendString("/WoTB_Voice_Mod/" + Voice_Set.SRTTbacon_Server_Name + "/Chat.dat", Encoding.UTF8.GetBytes(Voice_Set.UserName + "(管理者)が参加しました。\n"));
                    }
                }
                else
                {
                    if (Chat_Temp.Contains(Voice_Set.UserName + "が参加しました。"))
                    {
                        Voice_Set.AppendString("/WoTB_Voice_Mod/" + Voice_Set.SRTTbacon_Server_Name + "/Chat.dat", Encoding.UTF8.GetBytes(Voice_Set.UserName + "が参加しました。\n"));
                    }
                }
                Voice_Set.TCP_Server.WriteLine(Voice_Set.SRTTbacon_Server_Name + "|Chat\0");
                Chat_T.Text = Server_Open_File("/WoTB_Voice_Mod/" + Voice_Set.SRTTbacon_Server_Name + "/Chat.dat");
                Chat_Scrool.ScrollToEnd();
                Player.settings.volume = (int)Voice_Volume_S.Value;
                IsProcessing = false;
                await Task.Delay(1000);
                while (Message_T.Opacity > 0)
                {
                    Message_T.Opacity -= 0.025;
                    await Task.Delay(1000 / 60);
                }
                Message_T.Text = "";
                Message_T.Opacity = 1;
                while (true)
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
                    await Task.Delay(1000);
                }
            }
        }
        //作成ボタン
        private async void Server_Create_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing)
            {
                return;
            }
            Server_Create_Window.Visibility = Visibility.Visible;
            while (Server_Create_Window.Opacity < 1)
            {
                Server_Create_Window.Opacity += 0.025;
                await Task.Delay(1000 / 60);
            }
            while (Server_Create_Window.Visibility == Visibility.Visible)
            {
                await Task.Delay(100);
            }
            Server_List_Reset();
        }
        //終了
        private async void Exit_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing)
            {
                return;
            }
            MessageBoxResult result = MessageBox.Show("終了しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                IsClosing = true;
                Voice_Set.App_Busy = true;
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
                Application.Current.Shutdown();
            }
        }
        //再接続ボタン
        private void Connect_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing)
            {
                return;
            }
            Server_Connect();
        }
        //サーバーリストを更新
        void Server_List_Reset()
        {
            Server_Lists.Items.Clear();
            Server_Names_List.Clear();
            string[] Temp = GetServerNames();
            for (int Number = 0; Number <= Temp.Length - 1; Number++)
            {
                XDocument xml2 = XDocument.Load(Voice_Set.FTP_Server.OpenRead("/WoTB_Voice_Mod/" + Temp[Number] + "/Server_Config.dat"));
                XElement item2 = xml2.Element("Server_Create_Config");
                string Add = "";
                if (bool.Parse(item2.Element("IsEnableR18").Value))
                {
                    Add += "R18=有効";
                }
                else
                {
                    Add += "R18=無効";
                }
                if (bool.Parse(item2.Element("IsEnablePassword").Value))
                {
                    Add += "   パスワード=有効";
                }
                else
                {
                    Add += "   パスワード=無効";
                }
                Server_Lists.Items.Add(Temp[Number] + "   " + Add);
                Server_Names_List.Add(Temp[Number]);
            }
            Server_Connect_B.Margin = new Thickness(-600, 375, 0, 0);
            Server_Create_B.Margin = new Thickness(-600, 550, 0, 0);
            Password_Text.Visibility = Visibility.Hidden;
            Password_T.Visibility = Visibility.Hidden;
            Server_Create_Name_T.Visibility = Visibility.Hidden;
            Explanation_Scrool.Visibility = Visibility.Hidden;
            Explanation_Text.Visibility = Visibility.Hidden;
            Explanation_Border.Visibility = Visibility.Hidden;
        }
        //レイアウトを更新
        void Connect_Mode_Layout()
        {
            if (Connectiong)
            {
                Connect_B.Visibility = Visibility.Hidden;
                User_Name_Text.Visibility = Visibility.Hidden;
                User_Name_T.Visibility = Visibility.Hidden;
                User_Password_Text.Visibility = Visibility.Hidden;
                User_Password_T.Visibility = Visibility.Hidden;
                User_Login_B.Visibility = Visibility.Hidden;
                User_Register_B.Visibility = Visibility.Hidden;
                //Voice_Create_Tool_B.Visibility = Visibility.Hidden;
                Voice_Create_Tool_B.Visibility = Visibility.Visible;
                WoTB_Select_B.Visibility = Visibility.Hidden;
                //Server_Connect_B.Visibility = Visibility.Visible;
                //Server_Lists.Visibility = Visibility.Visible;
                //Server_Create_B.Visibility = Visibility.Visible;
                //Server_List_Update_B.Visibility = Visibility.Visible;
                Voice_Mod_Free_B.Visibility = Visibility.Visible;
                Message_B.Visibility = Visibility.Visible;
                Update_B.Visibility = Visibility.Visible;
            }
            else
            {
                Connect_B.Visibility = Visibility.Visible;
                Cache_Delete_B.Visibility = Visibility.Visible;
                Server_Connect_B.Visibility = Visibility.Hidden;
                Voice_Mod_Free_B.Visibility = Visibility.Hidden;
                Message_B.Visibility = Visibility.Hidden;
                Update_B.Visibility = Visibility.Hidden;
                Server_Lists.Visibility = Visibility.Hidden;
                Voice_S.Visibility = Visibility.Hidden;
                Voice_Back_B.Visibility = Visibility.Hidden;
                Voice_Front_B.Visibility = Visibility.Hidden;
                Voice_Stop_B.Visibility = Visibility.Hidden;
                Voice_Play_B.Visibility = Visibility.Hidden;
                Voice_Location_S.Visibility = Visibility.Hidden;
                Voice_Volume_S.Visibility = Visibility.Hidden;
                Voice_Type_C.Visibility = Visibility.Hidden;
                Voice_Delete_B.Visibility = Visibility.Hidden;
                Voice_Type_Border.Visibility = Visibility.Hidden;
                Voice_Control_Window.Visibility = Visibility.Hidden;
                Back_B.Visibility = Visibility.Hidden;
                Save_B.Visibility = Visibility.Hidden;
                Server_Create_B.Visibility = Visibility.Hidden;
                Server_List_Update_B.Visibility = Visibility.Hidden;
                Administrator_B.Visibility = Visibility.Hidden;
                Explanation_Scrool.Visibility = Visibility.Hidden;
                Explanation_Text.Visibility = Visibility.Hidden;
                Explanation_Border.Visibility = Visibility.Hidden;
                Chat_Hide();
            }
        }
        void Chat_Hide()
        {
            Chat_Border.Visibility = Visibility.Hidden;
            Chat_Scrool.Visibility = Visibility.Hidden;
            Chat_Send_B.Visibility = Visibility.Hidden;
            Chat_Send_T.Visibility = Visibility.Hidden;
            Chat_Mode_Public_B.Visibility = Visibility.Hidden;
            Chat_Mode_Server_B.Visibility = Visibility.Hidden;
            Chat_Mode_Private_B.Visibility = Visibility.Hidden;
        }
        void Chat_Show()
        {
            Chat_Border.Visibility = Visibility.Visible;
            Chat_Scrool.Visibility = Visibility.Visible;
            Chat_Send_B.Visibility = Visibility.Visible;
            Chat_Send_T.Visibility = Visibility.Visible;
            Chat_Mode_Public_B.Visibility = Visibility.Visible;
            Chat_Mode_Server_B.Visibility = Visibility.Visible;
            Chat_Mode_Private_B.Visibility = Visibility.Visible;
        }
    }
}