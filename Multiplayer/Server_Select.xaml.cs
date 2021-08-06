using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace WoTB_Voice_Mod_Creater.Multiplayer
{
    public partial class Server_Select : UserControl
    {
        bool IsMessageShowing = false;
        bool IsClosing = false;
        List<string> Server_Names_List = new List<string>();
        public Server_Select()
        {
            InitializeComponent();
            Server_Create_Name_T.Visibility = Visibility.Hidden;
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
        public async void Window_Show()
        {
            Opacity = 0;
            Visibility = Visibility.Visible;
            Server_List_Reset();
            while (Opacity < 1 && !IsClosing)
            {
                Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
        }
        private async void Back_B_Click(object sender, RoutedEventArgs e)
        {
            if (!IsClosing)
            {
                IsClosing = true;
                Voice_Set.SRTTbacon_Server_Name = "";
                while (Opacity > 0)
                {
                    Opacity -= Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
                Visibility = Visibility.Hidden;
                IsClosing = false;
            }
        }
        //サーバーを取得
        string[] GetServerNames()
        {
            string[] Server_Lists = Server_File.Server_Open_File_Line("/WoTB_Voice_Mod/Voice_Online/Server_Names.dat");
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
        //サーバーリストを更新
        void Server_List_Reset()
        {
            Server_Lists.Items.Clear();
            Server_Names_List.Clear();
            string[] Temp = GetServerNames();
            for (int Number = 0; Number <= Temp.Length - 1; Number++)
            {
                try
                {
                    string Name = Temp[Number];
                    string Add_Server_Item_String = Name.Substring(0, Name.IndexOf('|'));
                    if (bool.Parse(Name.Substring(Name.IndexOf('|') + 1, Name.LastIndexOf('|') - (Name.IndexOf('|') + 1))))
                    {
                        Add_Server_Item_String += "|R18:有効";
                    }
                    else
                    {
                        Add_Server_Item_String += "|R18:無効";
                    }
                    if (bool.Parse(Name.Substring(Name.LastIndexOf('|') + 1)))
                    {
                        Add_Server_Item_String += "|パスワード:有効";
                    }
                    else
                    {
                        Add_Server_Item_String += "|パスワード:無効";
                    }
                    Server_Lists.Items.Add(Add_Server_Item_String);
                    Server_Names_List.Add(Name.Substring(0, Name.IndexOf('|')));
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
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
        //サーバーに参加
        private async void Server_Connect_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
            {
                return;
            }
            if (Server_Lists.SelectedIndex == -1)
            {
                Message_Feed_Out("サーバーが選択されていません。");
                return;
            }
            try
            {
                string Directory_Name = Server_Names_List[Server_Lists.SelectedIndex] + "/Voices";
                if (Directory.Exists(Voice_Set.Special_Path + "/Server/" + Directory_Name) && Directory.GetFiles(Voice_Set.Special_Path + "/Server/" + Directory_Name).Length == 0)
                {
                    Directory.Delete(Voice_Set.Special_Path + "/Server/" + Directory_Name, true);
                }
                XDocument xml2 = XDocument.Load(Voice_Set.FTPClient.GetFileRead("/WoTB_Voice_Mod/Voice_Online/" + Server_Names_List[Server_Lists.SelectedIndex] + "/Server_Config.dat"));
                XElement item2 = xml2.Element("Server_Create_Config");
                if (bool.Parse(item2.Element("IsEnablePassword").Value))
                {
                    if (Password_T.Text != item2.Element("Password").Value)
                    {
                        Message_Feed_Out("パスワードが違います。");
                        return;
                    }
                }
                Message_T.Text = "サーバーに参加しています...";
                await Task.Delay(50);
                Server_Voices.Voice_List.Clear();
                FtpWebRequest fwr = (FtpWebRequest)WebRequest.Create(new Uri("ftp://" + SRTTbacon_Server.IP + "/WoTB_Voice_Mod/Voice_Online/" + Directory_Name + "/"));
                fwr.UsePassive = true;
                fwr.KeepAlive = false;
                fwr.Credentials = new NetworkCredential(SRTTbacon_Server.Name, SRTTbacon_Server.Password);
                fwr.Method = WebRequestMethods.Ftp.ListDirectory;
                StreamReader sr = new StreamReader(fwr.GetResponse().GetResponseStream());
                string str = sr.ReadLine();
                while (str != null)
                {
                    if (str != "")
                    {
                        Server_Voices.Voice_List.Add(str);
                    }
                    str = sr.ReadLine();
                }
                sr.Close();
                fwr.Abort();
                Voice_Set.SRTTbacon_Server_Name = Server_Names_List[Server_Lists.SelectedIndex];
                Directory.CreateDirectory(Voice_Set.Special_Path + "/Server/" + Directory_Name);
                Voice_Set.FTPClient.DownloadFile("/WoTB_Voice_Mod/Voice_Online/" + Directory_Name + "/" + Server_Voices.Voice_List[0], Voice_Set.Special_Path + "/Server/" + Directory_Name + "/" + Server_Voices.Voice_List[0]);
                string Chat_Temp = Server_File.Server_Open_File("/WoTB_Voice_Mod/Voice_Online/" + Voice_Set.SRTTbacon_Server_Name + "/Chat.dat");
                if (Voice_Set.UserName == item2.Element("Master_User_Name").Value)
                {
                    if (!Chat_Temp.Contains(Voice_Set.UserName + "(管理者)が参加しました。"))
                    {
                        Voice_Set.AppendString("/WoTB_Voice_Mod/Voice_Online/" + Voice_Set.SRTTbacon_Server_Name + "/Chat.dat", Voice_Set.UserName + "が参加しました。\n");
                    }
                }
                else
                {
                    if (!Chat_Temp.Contains(Voice_Set.UserName + "が参加しました。"))
                    {
                        Voice_Set.AppendString("/WoTB_Voice_Mod/Voice_Online/" + Voice_Set.SRTTbacon_Server_Name + "/Chat.dat", Voice_Set.UserName + "が参加しました。\n");
                    }
                }
                Voice_Set.TCP_Server.Send(Voice_Set.SRTTbacon_Server_Name + "|Chat");
                Visibility = Visibility.Hidden;
                Opacity = 0;
            }
            catch (Exception e1)
            {
                Sub_Code.Error_Log_Write(e1.Message);
                Message_Feed_Out("エラーが発生しました。");
            }
        }
        //サーバーを作成
        private async void Server_Create_B_Click(object sender, RoutedEventArgs e)
        {
            Server_Create_Window.Window_Show();
            while (Server_Create_Window.Visibility == Visibility.Visible)
            {
                await Task.Delay(100);
            }
            Server_List_Reset();
        }
        //何もないところをクリックしたらレイアウト初期化
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
        //サーバーの詳細を表示
        private void Server_Lists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Server_Lists.SelectedIndex != -1)
            {
                Explanation_Scrool.Visibility = Visibility.Visible;
                Explanation_Text.Visibility = Visibility.Visible;
                Explanation_Border.Visibility = Visibility.Visible;
                XDocument xml2 = XDocument.Load(Voice_Set.FTPClient.GetFileRead("/WoTB_Voice_Mod/Voice_Online/" + Server_Names_List[Server_Lists.SelectedIndex] + "/Server_Config.dat"));
                XElement item2 = xml2.Element("Server_Create_Config");
                if (bool.Parse(item2.Element("IsEnablePassword").Value))
                {
                    Password_Text.Visibility = Visibility.Visible;
                    Password_T.Visibility = Visibility.Visible;
                }
                else
                {
                    Password_Text.Visibility = Visibility.Hidden;
                    Password_T.Visibility = Visibility.Hidden;
                }
                Explanation_T.Text = item2.Element("Explanation").Value;
                Server_Create_Name_T.Text = "制作者:" + item2.Element("Master_User_Name").Value;
                Server_Create_Name_T.Visibility = Visibility.Visible;
            }
        }
    }
}