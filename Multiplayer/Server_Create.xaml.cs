using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Xml.Serialization;
using WK.Libraries.BetterFolderBrowserNS;

namespace WoTB_Voice_Mod_Creater.Multiplayer
{
    public partial class Server_Create : System.Windows.Controls.UserControl
    {
        bool IsProcessing = false;
        bool IsMessageShowing = false;
        List<string> Voice_Add_Files = new List<string>();
        public Server_Create()
        {
            InitializeComponent();
            Set_Password_T.Visibility = Visibility.Hidden;
            Process_P.Visibility = Visibility.Hidden;
            Process_T.Visibility = Visibility.Hidden;
        }
        public async void Window_Show()
        {
            Opacity = 0;
            Visibility = Visibility.Visible;
            while (Opacity < 1 && !IsProcessing)
            {
                Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
        }
        private async void Exit_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing)
            {
                return;
            }
            if (Directory.Exists(Voice_Set.Special_Path + "/Server/Voices"))
            {
                Directory.Delete(Voice_Set.Special_Path + "/Server/Voices", true);
            }
            if (Opacity >= 1)
            {
                IsProcessing = true;
                while (Opacity > 0)
                {
                    Opacity -= Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
                Visibility = Visibility.Hidden;
                IsProcessing = false;
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
        private async void Voice_Select_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing)
            {
                return;
            }
            BetterFolderBrowser f = new BetterFolderBrowser
            {
                RootFolder = Sub_Code.Get_OpenDirectory_Path(),
                Title = "音声フォルダを選択してください。",
                Multiselect = false
            };
            if (f.ShowDialog() == DialogResult.OK)
            {
                Sub_Code.Set_Directory_Path(f.SelectedFolder);
                Message_T.Text = "ファイルを確認しています...";
                Directory.CreateDirectory(Voice_Set.Special_Path + "/Server/Voices");
                Voice_Add_L.Items.Clear();
                List<string> Voice_Not_Add = new List<string>();
                bool IsVoiceNotAdd = false;
                IsProcessing = true;
                string[] GetFiles = Directory.GetFiles(f.SelectedPath, "*", SearchOption.TopDirectoryOnly);
                int Voice_All = GetFiles.Length;
                await Task.Delay(50);
                foreach (string File_Name in GetFiles)
                {
                    string File_Ex = Path.GetExtension(File_Name);
                    if (File_Ex == ".mp3" || File_Ex == ".wav" || File_Ex == ".ogg" || File_Ex == ".flac" || File_Ex == ".wma")
                    {
                        Voice_Add_L.Items.Add(Path.GetFileName(File_Name));
                        Voice_Add_Files.Add(File_Name);
                    }
                    else
                    {
                        Voice_Not_Add.Add(Path.GetFileName(File_Name));
                        IsVoiceNotAdd = true;
                    }
                }
                int Number = 1;
                if (Voice_Not_Add.Count > 0)
                {
                    while (true)
                    {
                        if (!File.Exists(Voice_Set.Special_Path + "/Configs/Log_" + Number + ".txt"))
                        {
                            break;
                        }
                        Number++;
                    }
                    StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Configs/Log_" + Number + ".txt");
                    foreach (string Name in Voice_Not_Add)
                    {
                        stw.WriteLine(Name);
                    }
                    stw.Close();
                }
                IsProcessing = false;
                Voice_Select_T.Text = "フォルダパス:" + f.SelectedPath;
                Message_T.Text = "";
                if (IsVoiceNotAdd)
                {
                    System.Windows.MessageBox.Show("対応していないファイル形式が見つかりました。Log_*.txtを参照してください。");
                }
            }
        }
        private void Voice_Delete_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing)
            {
                return;
            }
            if (Voice_Add_L.SelectedIndex == -1)
            {
                System.Windows.MessageBox.Show("取り消す音声ファイルが選択されていません。");
                return;
            }
            Voice_Add_L.Items.RemoveAt(Voice_Add_L.SelectedIndex);
            Voice_Add_L.SelectedItem = -1;
        }
        private void Voice_Add_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing)
            {
                return;
            }
            OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "音声ファイルを選択してください。",
                Filter = "音声ファイル(*.mp3;*.wav;*.ogg;*.flac;*.wma)|*.mp3;*.wav;*.ogg;*.flac;*.wma",
                Multiselect = true
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                bool IsFileExists = false;
                string Not_Add_FileName = "";
                foreach (string File_Path in ofd.FileNames)
                {
                    if (File.Exists(Voice_Set.Special_Path + "/Server/Voices/" + Path.GetFileName(File_Path)))
                    {
                        IsFileExists = true;
                        Not_Add_FileName += Path.GetFileName(File_Path) + "\n";
                    }
                    else
                    {
                        Voice_Add_L.Items.Add(Path.GetFileName(File_Path));
                        File.Copy(File_Path, Voice_Set.Special_Path + "/Server/Voices/" + Path.GetFileName(File_Path), true);
                    }
                }
                if (IsFileExists)
                {
                    System.Windows.MessageBox.Show("同名のファイルが存在します。以下のファイルは追加されませんでした。\n" + Not_Add_FileName);
                }
            }
        }
        private void Alert_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing)
            {
                return;
            }
            string Alert_01 = "注意:サーバーには容量がありますので、できればファイルサイズは合わせて1GB以下にしてください。\n";
            string Alert_02 = "基本ないとは思いますが、もしプロジェクトが消えてしまったらすぐにSRTTbacon#2395にご連絡ください。バックアップから復元を試みます。\n";
            string Alert_03 = "私が不必要なプロジェクトだと判断した場合、本人の意思に関係なく削除する場合があります。以上のことをご了承ください。\n";
            string Alert_04 = "対応しているフォーマット:.mp3 .wav .ogg .flac .wma";
            System.Windows.MessageBox.Show(Alert_01 + Alert_02 + Alert_03 + Alert_04);
        }
        private void Set_Password_C_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing)
            {
                return;
            }
            if (Set_Password_C.IsChecked.Value)
            {
                Set_Password_T.Visibility = Visibility.Visible;
            }
            else
            {
                Set_Password_T.Visibility = Visibility.Hidden;
            }
        }
        private async void Save_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing)
            {
                return;
            }
            if (Project_Name_T.Text == "")
            {
                Message_T.Text = "プロジェクト名が設定されていません。";
                return;
            }
            try
            {
                Directory.CreateDirectory(Voice_Set.Special_Path + "/Temp/" + Project_Name_T.Text);
                Directory.Delete(Voice_Set.Special_Path + "/Temp", true);
                if (Project_Name_T.Text.Contains("/"))
                {
                    Message_T.Text = "プロジェクト名に不適切な文字が含まれています。";
                    return;
                }
            }
            catch
            {
                Message_T.Text = "プロジェクト名に不適切な文字が含まれています。";
                return;
            }
            if (Project_Name_T.Text.CountOf("  ") > 0)
            {
                Message_T.Text = "プロジェクト名に空白を2つ続けて付けることはできません。";
                return;
            }
            if (Voice_Add_L.Items.Count <= 0)
            {
                Message_T.Text = "最低1つは音声ファイルが必要です。";
                return;
            }
            if (Voice_Set.FTPClient.Directory_Exist("/WoTB_Voice_Mod/Voice_Online/" + Project_Name_T.Text))
            {
                Message_T.Text = "同名のプロジェクトが存在するか、別の目的で使用されています。";
                return;
            }
            Server_Create_Config Conf = new Server_Create_Config
            {
                IsEnableR18 = R_18_C.IsChecked.Value,
                IsEnablePassword = Set_Password_C.IsChecked.Value,
                Explanation = Explanation_T.Text,
                Master_User_Name = Voice_Set.UserName
            };
            if (Set_Password_C.IsChecked.Value)
            {
                Conf.Password = Set_Password_T.Text;
            }
            IsProcessing = true;
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Server_Create_Config));
            StreamWriter streamWriter = new StreamWriter(Voice_Set.Special_Path + "/Temp_Create_Server.dat", false, new UTF8Encoding(false));
            xmlSerializer.Serialize(streamWriter, Conf);
            streamWriter.Close();
            Voice_Set.FTPClient.Directory_Create("/WoTB_Voice_Mod/Voice_Online/" + Project_Name_T.Text + "/Voices");
            Voice_Set.FTPClient.UploadFile(Voice_Set.Special_Path + "/Temp_Create_Server.dat", "/WoTB_Voice_Mod/Voice_Online/" + Project_Name_T.Text + "/Server_Config.dat");
            File.Delete(Voice_Set.Special_Path + "/Temp_Create_Server.dat");
            StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Temp_Change_Names.dat");
            stw.WriteLine("ここにファイルの変更を保存します。");
            stw.Close();
            Voice_Set.FTPClient.UploadFile(Voice_Set.Special_Path + "/Temp_Change_Names.dat", "/WoTB_Voice_Mod/Voice_Online/" + Project_Name_T.Text + "/Change_Names.dat");
            File.Delete(Voice_Set.Special_Path + "/Temp_Change_Names.dat");
            StreamWriter Chat_Create = File.CreateText(Voice_Set.Special_Path + "/Temp_Chat_Create.dat");
            Chat_Create.WriteLine(Voice_Set.UserName + "がプロジェクトを作成しました。");
            Chat_Create.Close();
            Voice_Set.FTPClient.UploadFile(Voice_Set.Special_Path + "/Temp_Chat_Create.dat", "/WoTB_Voice_Mod/Voice_Online/" + Project_Name_T.Text + "/Chat.dat");
            File.Delete(Voice_Set.Special_Path + "/Temp_Chat_Create.dat");
            Message_T.Text = "サーバー内にプロジェクトを作成しています...";
            await Task.Delay(50);
            Process_P.Value = 0;
            int Count_Max = Voice_Add_Files.Count;
            Process_P.Maximum = Count_Max;
            Process_T.Text = "0/" + Count_Max;
            Process_P.Visibility = Visibility.Visible;
            Process_T.Visibility = Visibility.Visible;
            await Task.Delay(50);
            //Upload_Wait(Count_Max);
            Message_T.Text = "音声ファイルをアップロードしています。音声の数や通信環境によって時間がかかる場合があります。";
            bool IsOK = await Upload_File_Multithread();
            if (!IsOK)
            {
                Process_P.Visibility = Visibility.Hidden;
                Process_T.Visibility = Visibility.Hidden;
                Message_Feed_Out("エラーが発生しました。");
                return;
            }
            Voice_Set.FTPClient.DownloadFile("/WoTB_Voice_Mod/Voice_Online/Server_Names.dat", Voice_Set.Special_Path + "/Server_Names.dat");
            StreamWriter stw2 = new StreamWriter(Voice_Set.Special_Path + "/Server_Names.dat", true, Encoding.UTF8);
            stw2.WriteLine(Project_Name_T.Text + "|" + R_18_C.IsChecked.Value + "|" + Set_Password_C.IsChecked.Value);
            stw2.Close();
            Voice_Set.FTPClient.UploadFile(Voice_Set.Special_Path + "/Server_Names.dat", "/WoTB_Voice_Mod/Voice_Online/Server_Names.dat");
            Voice_Add_L.Items.Clear();
            R_18_C.IsChecked = false;
            Set_Password_C.IsChecked = false;
            Project_Name_T.Text = "";
            Voice_Select_T.Text = "フォルダパス:";
            Set_Password_T.Text = "";
            Process_P.Visibility = Visibility.Hidden;
            Process_T.Visibility = Visibility.Hidden;
            Message_Feed_Out("プロジェクトを作成しました。");
            await Task.Delay(1000);
            while (Opacity > 0)
            {
                Opacity -= Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
            Visibility = Visibility.Hidden;
            IsProcessing = false;
        }
        async Task<bool> Upload_File_Multithread()
        {
            try
            {
                for (int i = 0; i < Voice_Add_Files.Count; i++)
                {
                    Voice_Set.FTPClient.UploadFile(Voice_Add_Files[i], "/WoTB_Voice_Mod/Voice_Online/" + Project_Name_T.Text + "/Voices/" + Path.GetFileName(Voice_Add_Files[i]));
                    await Task.Delay(1);
                    Process_P.Value = i;
                    Process_T.Text = i + "/" + Voice_Add_Files.Count;
                }
                return true;
            }
            catch (Exception ex)
            {
                Sub_Code.Error_Log_Write(ex.Message);
                return false;
            }
        }
    }
}
public class Server_Create_Config
{
    public bool IsEnableR18;
    public bool IsEnablePassword;
    public string Password;
    public string Explanation;
    public string Master_User_Name;
}