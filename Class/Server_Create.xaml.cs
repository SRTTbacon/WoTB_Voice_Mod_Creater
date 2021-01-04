using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace WoTB_Voice_Mod_Creater.Class
{
    public partial class Server_Create : System.Windows.Controls.UserControl
    {
        readonly string Special_Path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/WoTB_Voice_Mod_Creater";
        bool IsProcessing = false;
        public Server_Create()
        {
            InitializeComponent();
            Set_Password_T.Visibility = Visibility.Hidden;
            Process_P.Visibility = Visibility.Hidden;
            Process_T.Visibility = Visibility.Hidden;
        }
        private async void Exit_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing)
            {
                return;
            }
            if (Directory.Exists(Special_Path + "/Server/Voices"))
            {
                Directory.Delete(Special_Path + "/Server/Voices", true);
            }
            if (Opacity >= 1)
            {
                IsProcessing = true;
                while (Opacity > 0)
                {
                    Opacity -= 0.025;
                    await Task.Delay(1000 / 60);
                }
                Visibility = Visibility.Hidden;
                IsProcessing = false;
            }
        }
        private async void Voice_Select_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing)
            {
                return;
            }
            FolderBrowserDialog f = new FolderBrowserDialog
            {
                SelectedPath = Directory.GetCurrentDirectory(),
                Description = "音声フォルダを選択してください。"
            };
            if (f.ShowDialog() == DialogResult.OK)
            {
                Message_T.Text = "ファイルを確認しています...";
                Directory.CreateDirectory(Special_Path + "/Server/Voices");
                Voice_Add_L.Items.Clear();
                List<string> Voice_List_Add = new List<string>();
                List<string> Voice_Not_Add = new List<string>();
                bool IsVoiceNotAdd = false;
                IsProcessing = true;
                int Voice_Number = 0;
                string[] GetFiles = Directory.GetFiles(f.SelectedPath, "*", SearchOption.TopDirectoryOnly);
                int Voice_All = GetFiles.Length;
                Process_T.Text = "0/" + Voice_All;
                Process_P.Maximum = Voice_All;
                Process_P.Minimum = 0;
                Process_P.Visibility = Visibility.Visible;
                Process_T.Visibility = Visibility.Visible;
                await Task.Delay(50);
                foreach (string File_Name in GetFiles)
                {
                    string File_Ex = Path.GetExtension(File_Name);
                    if (File_Ex == ".mp3" || File_Ex == ".wav" || File_Ex == ".ogg" || File_Ex == ".flac" || File_Ex == ".wma")
                    {
                        Voice_Add_L.Items.Add(Path.GetFileName(File_Name));
                        Voice_List_Add.Add(File_Name);
                        File.Copy(File_Name, Special_Path + "/Server/Voices/" + Path.GetFileName(File_Name), true);
                    }
                    else
                    {
                        Voice_Not_Add.Add(Path.GetFileName(File_Name));
                        IsVoiceNotAdd = true;
                    }
                    Voice_Number++;
                    await Task.Delay(1);
                    Process_P.Value = Voice_Number;
                    Process_T.Text = Voice_Number + "/" + Voice_All;
                }
                Process_P.Visibility = Visibility.Hidden;
                Process_T.Visibility = Visibility.Hidden;
                int Number = 1;
                while (true)
                {
                    if (!File.Exists(Directory.GetCurrentDirectory() + "Log_" + Number + ".txt"))
                    {
                        break;
                    }
                    Number++;
                }
                StreamWriter stw = File.CreateText(Directory.GetCurrentDirectory() + "Log_" + Number + ".txt");
                foreach (string Name in Voice_Not_Add)
                {
                    stw.WriteLine(Name);
                }
                stw.Close();
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
                    if (File.Exists(Special_Path + "/Server/Voices/" + Path.GetFileName(File_Path)))
                    {
                        IsFileExists = true;
                        Not_Add_FileName += Path.GetFileName(File_Path) + "\n";
                    }
                    else
                    {
                        Voice_Add_L.Items.Add(Path.GetFileName(File_Path));
                        File.Copy(File_Path, Special_Path + "/Server/Voices/" + Path.GetFileName(File_Path), true);
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
            string Alert_01 = "注意:サーバーには容量がありますので、できればファイルサイズは合わせて1GB以下にしてください。また、既にプロジェクトにある音声を再び使用することは避けてください。\n";
            string Alert_02 = "基本ないとは思いますが、もしプロジェクトが消えてしまったらすぐにSRTTbacon#2395にご連絡ください。バックアップから復元を試みます。\n";
            string Alert_03 = "私が不必要なプロジェクトだと判断した場合、本人の意思に関係なく削除する場合があります。以上のことをご了承ください。";
            System.Windows.MessageBox.Show(Alert_01 + Alert_02 + Alert_03);
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
                Directory.CreateDirectory(Special_Path + "/Temp/" + Project_Name_T.Text);
                Directory.Delete(Special_Path + "/Temp/" + Project_Name_T.Text);
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
            if (Voice_Set.FTP_Server.DirectoryExists("/WoTB_Voice_Mod/" + Project_Name_T.Text))
            {
                Message_T.Text = "同名のプロジェクトがするか、別の目的で使用されています。";
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
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Server_Create_Config));
            StreamWriter streamWriter = new StreamWriter(Special_Path + "/Temp_Create_Server.dat", false, new UTF8Encoding(false));
            xmlSerializer.Serialize(streamWriter, Conf);
            streamWriter.Close();
            Voice_Set.FTP_Server.CreateDirectory("/WoTB_Voice_Mod/" + Project_Name_T.Text, true);
            Voice_Set.FTP_Server.UploadFile(Special_Path + "/Temp_Create_Server.dat", "/WoTB_Voice_Mod/" + Project_Name_T.Text + "/Server_Config.dat");
            File.Delete(Special_Path + "/Temp_Create_Server.dat");
            StreamWriter stw = File.CreateText(Special_Path + "/Temp_Change_Names.dat");
            stw.WriteLine("ここにファイルの変更を保存します。");
            stw.Close();
            Voice_Set.FTP_Server.UploadFile(Special_Path + "/Temp_Change_Names.dat", "/WoTB_Voice_Mod/" + Project_Name_T.Text + "/Change_Names.dat");
            File.Delete(Special_Path + "/Temp_Change_Names.dat");
            StreamWriter Chat_Create = File.CreateText(Special_Path + "/Temp_Chat_Create.dat");
            Chat_Create.WriteLine(Voice_Set.UserName + "がプロジェクトを作成しました。");
            Chat_Create.Close();
            Voice_Set.FTP_Server.UploadFile(Special_Path + "/Temp_Chat_Create.dat", "/WoTB_Voice_Mod/" + Project_Name_T.Text + "/Chat.dat");
            File.Delete(Special_Path + "/Temp_Chat_Create.dat");
            Message_T.Text = "サーバー内にプロジェクトを作成しています...";
            await Task.Delay(50);
            string[] Upload_Files = Directory.GetFiles(Special_Path + "/Server/Voices", "*", SearchOption.TopDirectoryOnly);
            Process_P.Value = 0;
            Voice_Set.FTP_Server.CreateDirectory("/WoTB_Voice_Mod/" + Project_Name_T.Text + "/Voices");
            int Count_Max = Upload_Files.Length;
            int Count_Now = 0;
            Process_P.Maximum = Upload_Files.Length;
            Process_T.Text = "0/" + Upload_Files.Length;
            Process_P.Visibility = Visibility.Visible;
            Process_T.Visibility = Visibility.Visible;
            foreach (string File_Name in Upload_Files)
            {
                Voice_Set.FTP_Server.UploadFile(File_Name, "/WoTB_Voice_Mod/" + Project_Name_T.Text + "/Voices/" + Path.GetFileName(File_Name));
                Count_Now++;
                await Task.Delay(1);
                Process_P.Value = Count_Now;
                Process_T.Text = Count_Now + "/" + Count_Max;
            }
            //Voice_Set.FTP_Server.UploadDirectory(Special_Path + "/Server/Voices", "/WoTB_Voice_Mod/" + Project_Name_T.Text + "/Voices");
            Voice_Set.FTP_Server.DownloadFile(Special_Path + "/Server_Names.dat", "/WoTB_Voice_Mod/Server_Names.dat");
            StreamWriter stw2 = new StreamWriter(Special_Path + "/Server_Names.dat", true, Encoding.UTF8);
            stw2.WriteLine(Project_Name_T.Text);
            stw2.Close();
            Directory.CreateDirectory(Special_Path + "/Server/" + Project_Name_T.Text);
            Directory.Move(Special_Path + "/Server/Voices", Special_Path + "/Server/" + Project_Name_T.Text + "/Voices");
            Message_T.Text = "プロジェクトを作成しました。適応します。";
            Voice_Set.FTP_Server.UploadFile(Special_Path + "/Server_Names.dat", "/WoTB_Voice_Mod/Server_Names.dat");
            Message_T.Text = "";
            Voice_Add_L.Items.Clear();
            R_18_C.IsChecked = false;
            Set_Password_C.IsChecked = false;
            Project_Name_T.Text = "";
            Voice_Select_T.Text = "フォルダパス:";
            Set_Password_T.Text = "";
            Process_P.Visibility = Visibility.Hidden;
            Process_T.Visibility = Visibility.Hidden;
            await Task.Delay(1000);
            while (Message_T.Opacity > 0)
            {
                Message_T.Opacity -= 0.025;
                await Task.Delay(1000 / 60);
            }
            Message_T.Text = "";
            while (Opacity > 0)
            {
                Opacity -= 0.025;
                await Task.Delay(1000 / 60);
            }
            Visibility = Visibility.Hidden;
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