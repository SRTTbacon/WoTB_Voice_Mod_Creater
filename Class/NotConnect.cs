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
        //サーバーに接続(参加ではない)
        void Server_Connect()
        {
            try
            {
                Voice_Set.TCP_Server = new SimpleTcpClient();
                Task task = Task.Run(() =>
                {
                    try
                    {
                        Voice_Set.TCP_Server.Connect(SRTTbacon_Server.IP, SRTTbacon_Server.Port);
                        Voice_Set.TCP_Server.StringEncoder = Encoding.UTF8;
                        Voice_Set.TCP_Server.Delimiter = 0x00;
                        Message_T.Text = "";
                    }
                    catch
                    {
                        Connectiong = false;
                        Server_OK = false;
                    }
                });
                Voice_Set.FTP_Server = new FtpClient(SRTTbacon_Server.IP)
                {
                    Credentials = new NetworkCredential(SRTTbacon_Server.Name, SRTTbacon_Server.Password),
                    SocketKeepAlive = false,
                    DataConnectionType = ConnectType,
                    SslProtocols = SslProtocols.Tls,
                    ConnectTimeout = 1000,
                };
                Voice_Set.FTP_Server.ValidateCertificate += new FtpSslValidation(OnValidateCertificate);
                Voice_Set.FTP_Server.Connect();
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
                    Message_T.Text = "ログイン(アカウント登録)をすると機能が有効化されます。";
                    Connect_B.Visibility = Visibility.Hidden;
                    User_Name_Text.Visibility = Visibility.Visible;
                    User_Name_T.Visibility = Visibility.Visible;
                    User_Password_Text.Visibility = Visibility.Visible;
                    User_Password_T.Visibility = Visibility.Visible;
                    User_Login_B.Visibility = Visibility.Visible;
                    User_Register_B.Visibility = Visibility.Visible;
                    Voice_Create_Tool_B.Visibility = Visibility.Hidden;
                    Voice_Create_V2_B.Visibility = Visibility.Visible;
                    Update_B.Visibility = Visibility.Visible;
                }
            }
            catch (Exception e)
            {
                Connectiong = false;
                Server_OK = false;
                Update_B.Visibility = Visibility.Hidden;
                Connect_Mode_Layout();
                Message_T.Text = "エラー:サーバーが開いていない可能性があります。";
                Sub_Code.Error_Log_Write(e.Message.Replace(SRTTbacon_Server.IP_Global + ":" + SRTTbacon_Server.Port, "").Replace(SRTTbacon_Server.IP_Local + ":" + SRTTbacon_Server.Port, ""));
            }
        }
        //ログインできるか
        bool Login()
        {
            if (File.Exists(Path + "/User.dat"))
            {
                try
                {
                    Sub_Code.File_Decrypt(Path + "/User.dat", Voice_Set.Special_Path + "/Temp_User.dat", "SRTTbacon_Server_User_Pass_Save", false);
                    StreamReader str = new StreamReader(Voice_Set.Special_Path + "/Temp_User.dat");
                    string Login_Read = str.ReadLine();
                    str.Close();
                    File.Delete(Voice_Set.Special_Path + "/Temp_User.dat");
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
                catch (Exception e)
                {
                    Sub_Code.Error_Log_Write(e.Message);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        //終了
        private async void Exit_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing || IsClosing)
            {
                return;
            }
            MessageBoxResult result = MessageBox.Show("終了しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                IsClosing = true;
                Voice_Set.App_Busy = true;
                Other_Window.Pause_Volume_Animation(true, 25);
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
                while (Opacity > 0)
                {
                    Opacity -= 0.05;
                    await Task.Delay(1000 / 60);
                }
                try
                {
                    if (Directory.Exists(Voice_Set.Special_Path + "/Wwise/BNK_WAV"))
                    {
                        Directory.Delete(Voice_Set.Special_Path + "/Wwise/BNK_WAV", true);
                    }
                    if (Directory.Exists(Voice_Set.Special_Path + "/Wwise/BNK_WAV_WoT"))
                    {
                        Directory.Delete(Voice_Set.Special_Path + "/Wwise/BNK_WAV_WoT", true);
                    }
                }
                catch (Exception e1)
                {
                    Sub_Code.Error_Log_Write(e1.Message);
                }
                Application.Current.Shutdown();
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
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
                Voice_Create_Tool_B.Visibility = Visibility.Hidden;
                Voice_Create_V2_B.Visibility = Visibility.Visible;
                WoTB_Select_B.Visibility = Visibility.Hidden;
                Chat_Show();
            }
            else
            {
                Connect_B.Visibility = Visibility.Visible;
                Cache_Delete_B.Visibility = Visibility.Visible;
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
            Chat_Border.Margin = new Thickness(-1920, 320, 0, 0);
            Chat_Scrool.Margin = new Thickness(-1920, 325, 0, 0);
            Chat_Send_T.Margin = new Thickness(-2040, 845, 0, 0);
            Chat_Send_B.Margin = new Thickness(-1418, 845, 0, 0);
            Chat_Mode_Public_B.Margin = new Thickness(-2344, 270, 0, 0);
            Chat_Mode_Server_B.Margin = new Thickness(-1920, 270, 0, 0);
            Chat_Mode_Private_B.Margin = new Thickness(-1496, 270, 0, 0);
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