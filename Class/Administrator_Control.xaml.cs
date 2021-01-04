using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace WoTB_Voice_Mod_Creater.Class
{
    public partial class Administrator_Control : System.Windows.Controls.UserControl
    {
        readonly string Special_Path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/WoTB_Voice_Mod_Creater";
        readonly List<string> Voices_Remove = new List<string>();
        readonly List<string> Voices_Add_FullName = new List<string>();
        bool IsProcessing = true;
        public Administrator_Control()
        {
            InitializeComponent();
        }
        public void Window_Show()
        {
            try
            {
                XDocument xml2 = XDocument.Load(Voice_Set.FTP_Server.OpenRead("/WoTB_Voice_Mod/" + Voice_Set.SRTTbacon_Server_Name + "/Server_Config.dat"));
                XElement item2 = xml2.Element("Server_Create_Config");
                if (bool.Parse(item2.Element("IsEnablePassword").Value))
                {
                    Set_Password_C.IsChecked = true;
                    Set_Password_T.Text = item2.Element("Password").Value;
                }
                R_18_C.IsChecked = bool.Parse(item2.Element("IsEnableR18").Value);
                Explanation_T.Text = item2.Element("Explanation").Value;
            }
            catch
            {
                System.Windows.MessageBox.Show("プロジェクトデータが破損しています。管理者(SRTTbacon#2395)へご連絡ください。");
            }
            Project_Name_T.Text = Voice_Set.SRTTbacon_Server_Name;
            Message_T.Text = "";
            Window_Feed_In();
            string[] Voice_Files = Directory.GetFiles(Special_Path + "/Server/" + Voice_Set.SRTTbacon_Server_Name + "/Voices", "*", SearchOption.TopDirectoryOnly);
            foreach (string File in Voice_Files)
            {
                string Name_Only = Path.GetFileName(File);
                if (!Voice_Set.Voice_Name_Hide(Name_Only))
                {
                    Voice_List.Items.Add(Name_Only);
                }
            }
        }
        async void Window_Feed_In()
        {
            Visibility = Visibility.Visible;
            Opacity = 0;
            while (Opacity < 1)
            {
                Opacity += 0.025;
                await Task.Delay(1000 / 60);
            }
            IsProcessing = false;
        }
        private void File_Delete_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing)
            {
                return;
            }
            if (Voice_List.SelectedIndex == -1)
            {
                System.Windows.MessageBox.Show("削除したいファイルを選択してください。");
                return;
            }
            bool IsAddRemove = false;
            int RemoveNumber = -1;
            for (int Number = 0; Number <= Voices_Add_FullName.Count - 1; Number++)
            {
                if (Voice_List.Items[Voice_List.SelectedIndex].ToString() == Voices_Add_FullName[Number])
                {
                    IsAddRemove = true;
                    RemoveNumber = Number;
                    break;
                }
            }
            if (IsAddRemove)
            {
                Voice_List.Items.RemoveAt(Voice_List.SelectedIndex);
                Voices_Add_FullName.RemoveAt(RemoveNumber);
            }
            else
            {
                Voices_Remove.Add(Voice_List.Items[Voice_List.SelectedIndex].ToString());
                Voice_List.Items.RemoveAt(Voice_List.SelectedIndex);
            }
        }
        private void File_Add_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing)
            {
                return;
            }
            OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "ファイルを選択してください。",
                Filter = "音声ファイル(*.mp3,*.wav,*.ogg,*.flac,*.wma)|*.mp3,*.wav,*.ogg,*.flac,*.wma",
                Multiselect = true
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                bool IsExists = false;
                string Not_Add = "";
                foreach (string Files in ofd.FileNames)
                {
                    string File_Name = Path.GetFileName(Files);
                    if (File.Exists(Special_Path + "/Server/" + Voice_Set.SRTTbacon_Server_Name + "/Voices/" + File_Name))
                    {
                        IsExists = true;
                        Not_Add += File_Name + "\n";
                    }
                    else
                    {
                        File.Copy(Files, Special_Path + "/Server/" + Voice_Set.SRTTbacon_Server_Name + "/Voices/" + File_Name);
                        Voice_List.Items.Add(File_Name);
                        Voices_Add_FullName.Add(Files);
                    }
                }
                if (IsExists)
                {
                    System.Windows.MessageBox.Show("同名のファイルが既に存在するので以下のファイルは追加されませんでした。\n" + Not_Add);
                }
            }
        }
        private async void Exit_B_Click(object sender, RoutedEventArgs e)
        {
            if (Opacity >= 1)
            {
                IsProcessing = true;
                while (Opacity > 0)
                {
                    Opacity -= 0.025;
                    await Task.Delay(1000 / 60);
                }
                Voice_List.Items.Clear();
                Voices_Add_FullName.Clear();
                Voices_Remove.Clear();
                Set_Password_C.IsChecked = false;
                Visibility = Visibility.Hidden;
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
                Directory.CreateDirectory(Special_Path + "/" + Project_Name_T.Text);
                Directory.Delete(Special_Path + "/" + Project_Name_T.Text);
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
            if (Voice_List.Items.Count <= 0)
            {
                Message_T.Text = "最低1つは音声ファイルが必要です。";
                return;
            }
            if (Voice_Set.SRTTbacon_Server_Name != Project_Name_T.Text)
            {
                if (Voice_Set.FTP_Server.DirectoryExists("/WoTB_Voice_Mod/" + Project_Name_T.Text))
                {
                    Message_T.Text = "同名のプロジェクトが存在します。";
                    return;
                }
            }
            IsProcessing = true;
            StreamWriter stw1 = File.CreateText(Special_Path + "/Temp_Remove_Files.dat");
            foreach (string File_Name in Voices_Remove)
            {
                stw1.WriteLine(File_Name);
                Voice_Set.FTP_Server.DeleteFile("/WoTB_Voice_Mod/" + Voice_Set.SRTTbacon_Server_Name + "/Voices/" + File_Name);
            }
            stw1.Close();
            Voice_Set.FTP_Server.UploadFile(Special_Path + "/Temp_Remove_Files.dat", "/WoTB_Voice_Mod/" + Project_Name_T.Text + "/Remove_Files.dat");
            File.Delete(Special_Path + "/Temp_Remove_Files.dat");
            StreamWriter stw2 = File.CreateText(Special_Path + "/Temp_Add_Files.dat");
            foreach (string File_Name in Voices_Add_FullName)
            {
                string Name = Path.GetFileName(File_Name);
                stw2.WriteLine(Name);
                Voice_Set.FTP_Server.UploadFile(File_Name, "/WoTB_Voice_Mod/" + Voice_Set.SRTTbacon_Server_Name + "/Voices/" + Name);
            }
            stw2.Close();
            Voice_Set.FTP_Server.UploadFile(Special_Path + "/Temp_Add_Files.dat", "/WoTB_Voice_Mod/" + Project_Name_T.Text + "/Add_Files.dat");
            File.Delete(Special_Path + "/Temp_Add_Files.dat");
            Server_Create_Config Conf = new Server_Create_Config
            {
                IsEnableR18 = R_18_C.IsChecked.Value,
                IsEnablePassword = Set_Password_C.IsChecked.Value,
                Explanation = Explanation_T.Text
            };
            if (Set_Password_C.IsChecked.Value)
            {
                Conf.Password = Set_Password_T.Text;
            }
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Server_Create_Config));
            StreamWriter streamWriter = new StreamWriter(Special_Path + "/Temp_Create_Server.dat", false, new UTF8Encoding(false));
            xmlSerializer.Serialize(streamWriter, Conf);
            streamWriter.Close();
            Voice_Set.FTP_Server.UploadFile(Special_Path + "/Temp_Create_Server.dat", "/WoTB_Voice_Mod/" + Project_Name_T.Text + "/Server_Config.dat");
            if (Voice_Set.SRTTbacon_Server_Name != Project_Name_T.Text)
            {
                Voice_Set.FTP_Server.MoveDirectory("/WoTB_Voice_Mod/" + Voice_Set.SRTTbacon_Server_Name, "/WoTB_Voice_Mod/" + Project_Name_T.Text);
            }
            Voice_Set.TCP_Server.WriteLineAndGetReply(Voice_Set.SRTTbacon_Server_Name + "|Change_Configs|" + Project_Name_T.Text + '\0', TimeSpan.FromSeconds(3));
            Message_T.Text = "保存しました。";
            await Task.Delay(500);
            while (Opacity > 0)
            {
                Opacity -= 0.025;
                await Task.Delay(1000 / 60);
            }
            Visibility = Visibility.Hidden;
        }
    }
}