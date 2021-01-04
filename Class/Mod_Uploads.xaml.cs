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
    public partial class Mod_Uploads : System.Windows.Controls.UserControl
    {
        bool IsBusy = false;
        readonly string Special_Path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/WoTB_Voice_Mod_Creater";
        readonly List<string> Mod_Name_Full = new List<string>();
        public Mod_Uploads()
        {
            InitializeComponent();
            Password_T.Visibility = Visibility.Hidden;
        }
        async public void Window_Show()
        {
            Opacity = 0;
            Visibility = Visibility.Visible;
            while (Opacity < 1 && !IsBusy)
            {
                Opacity += 0.025;
                await Task.Delay(1000 / 60);
            }
        }
        private void Exit_B_Click(object sender, RoutedEventArgs e)
        {
            Window_Close();
        }
        async void Window_Close()
        {
            if (!IsBusy)
            {
                IsBusy = true;
                while (Opacity > 0)
                {
                    Opacity -= 0.025;
                    await Task.Delay(1000 / 60);
                }
                Visibility = Visibility.Hidden;
                BGM_Mode_C.IsChecked = false;
                R_18_C.IsChecked = false;
                Password_C.IsChecked = false;
                Mod_Create_Name_T.Text = "";
                Mod_Explanation_T.Text = "";
                Password_T.Text = "";
                Message_T.Text = "";
                Mod_Name_Full.Clear();
                Mod_File_List.Items.Clear();
                IsBusy = false;
            }
        }
        private void Mod_Delete_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
            if (Mod_File_List.SelectedIndex != -1)
            {
                int Number = Mod_File_List.SelectedIndex;
                Mod_File_List.SelectedIndex = -1;
                Mod_Name_Full.RemoveAt(Number);
                Mod_File_List.Items.RemoveAt(Number);
            }
        }
        private void Mod_Add_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
            OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "Modファイルを選択してください。",
                Filter = "Modファイル(*.yaml;*.fev;*.fsb)|*.yaml;*.fev;*.fsb",
                Multiselect = true
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                foreach (string Mod_File in ofd.FileNames)
                {
                    string Name = Path.GetFileName(Mod_File);
                    if (Path.GetExtension(Mod_File) == ".yaml")
                    {
                        if (Name != "sounds.yaml")
                        {
                            System.Windows.MessageBox.Show(".yamlファイルは、sounds.yamlファイル以外追加できません。\n現在のファイル名:" + Name);
                            continue;
                        }
                    }
                    for (int Number = 0; Number <= Mod_File_List.Items.Count - 1; Number++)
                    {
                        if (Mod_File_List.Items[Number].ToString() == Name)
                        {
                            return;
                        }
                    }
                    Mod_Name_Full.Add(Mod_File);
                    Mod_File_List.Items.Add(Name);
                }
            }
        }
        private void Help_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
            string Message_01 = "・Mod名は、配布する際のタイトルになります。\n・ingame_voice以外の音声Modを指定するときは、sounds.yamlもリストに入れる必要があります。\n";
            string Message_02 = "・sfx_high(sfx_low).yamlは追加しなくてもソフト側で自動的にダウンロードしたユーザーに設定します。\n";
            string Message_03 = "・ModファイルがBGMModのみの場合は必ず\"BGMModに設定\"にチェックを入れるようにしてください。(sounds.yamlが指定されている場合はチェックしないでください。)\n";
            string Message_04 = "・管理者が不必要なModファイルと判断した場合は本人の意思に関係なく削除する可能性があります。";
            System.Windows.MessageBox.Show(Message_01 + Message_02 + Message_03 + Message_04);
        }
        private async void Mod_Create_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
            if (Mod_File_List.Items.Count == 0)
            {
                Message_T.Text = "最低1つはファイルが必要です。";
                return;
            }
            if (Mod_Create_Name_T.Text == "")
            {
                Message_T.Text = "Mod名が指定されていません。";
                return;
            }
            try
            {
                Directory.CreateDirectory(Special_Path + "/" + Mod_Create_Name_T.Text);
                Directory.Delete(Special_Path + "/" + Mod_Create_Name_T.Text);
                if (Mod_Create_Name_T.Text.Contains("/"))
                {
                    Message_T.Text = "Mod名に不適切な文字が含まれています。";
                    return;
                }
            }
            catch
            {
                Message_T.Text = "Mod名に不適切な文字が含まれています。";
                return;
            }
            if (Mod_Create_Name_T.Text.CountOf("  ") > 0)
            {
                Message_T.Text = "Mod名に空白を2つ続けて付けることはできません。";
                return;
            }
            if (Voice_Set.FTP_Server.DirectoryExists("/WoTB_Voice_Mod/Mods/" + Mod_Create_Name_T.Text))
            {
                Message_T.Text = "同名のModが既に存在します。";
                return;
            }
            if (Mod_Create_Name_T.Text == "Backup")
            {
                Message_T.Text = "そのMod名は別の目的に使用されています。";
            }
            if (BGM_Mode_C.IsChecked.Value)
            {
                try
                {
                    int Number = -1;
                    for (int Number_01 = 0; Number_01 <= Mod_Name_Full.Count - 1; Number_01++)
                    {
                        if (Path.GetFileName(Mod_Name_Full[Number_01]).Contains("Music.fev"))
                        {
                            Number = Number_01;
                            break;
                        }
                    }
                    if (Number == -1)
                    {
                        Message_T.Text = "BGMModにチェックが入っていますが、BGMファイルが見つかりません。";
                        return;
                    }
                    Message_T.Text = "BGMファイルを確認しています...";
                    await Task.Delay(100);
                    Fmod_Player.ESystem.load(Mod_Name_Full[Number], ref Fmod_Player.ELI, ref Fmod_Player.EP);
                    Fmod.RESULT result = Fmod_Player.ESystem.getEvent("Music/Music/Music", Fmod.EVENT_MODE.DEFAULT, ref Fmod_Player.FE);
                    Fmod_Player.EP.release();
                    Fmod_Player.FE.release();
                    if (result != Fmod.RESULT.OK)
                    {
                        throw new Exception("Music/Music/Musicが存在しません。");
                    }
                }
                catch
                {
                    Message_T.Text = "BGMを付ける場合はファイル構成を\"Music/Music/Music\"にしてください。";
                    return;
                }
            }
            Message_T.Text = "";
            if (Mod_Explanation_T.Text == "")
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Modの説明が入力されていません。実行しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }
            IsBusy = true;
            try
            {
                Message_T.Text = "Modを公開しています...";
                await Task.Delay(100);
                Mod_Upload_Config Configs = new Mod_Upload_Config()
                {
                    IsBGMMode = BGM_Mode_C.IsChecked.Value,
                    IsPassword = Password_C.IsChecked.Value,
                    IsEnableR18 = R_18_C.IsChecked.Value,
                    UserName = Voice_Set.UserName,
                    Explanation = Mod_Explanation_T.Text
                };
                if (Password_C.IsChecked.Value)
                {
                    Configs.Password = Password_T.Text;
                }
                StreamWriter stw = new StreamWriter(Special_Path + "/Test.dat");
                stw.Write("テスト");
                stw.Close();
                Voice_Set.FTP_Server.CreateDirectory("/WoTB_Voice_Mod/Mods/" + Mod_Create_Name_T.Text + "/Files", true);
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Mod_Upload_Config));
                StreamWriter streamWriter = new StreamWriter(Special_Path + "/Temp_Create_Mod.dat", false, new UTF8Encoding(false));
                xmlSerializer.Serialize(streamWriter, Configs);
                streamWriter.Close();
                Voice_Set.FTP_Server.UploadFile(Special_Path + "/Temp_Create_Mod.dat", "/WoTB_Voice_Mod/Mods/" + Mod_Create_Name_T.Text + "/Configs.dat");
                File.Delete(Special_Path + "/Temp_Create_Mod.dat");
                await Task.Delay(50);
                foreach (string Upload_File in Mod_Name_Full)
                {
                    Voice_Set.FTP_Server.UploadFile(Upload_File, "/WoTB_Voice_Mod/Mods/" + Mod_Create_Name_T.Text + "/Files/" + Path.GetFileName(Upload_File));
                }
                Voice_Set.AppendString("/WoTB_Voice_Mod/Mods/Mod_Names.dat", Encoding.UTF8.GetBytes(Mod_Create_Name_T.Text + "\n"));
                IsBusy = false;
                Message_T.Text = "Modを公開しました。";
                Window_Close();
            }
            catch (Exception e1)
            {
                System.Windows.MessageBox.Show("エラー:" + e1.Message);
                IsBusy = false;
            }
        }
        private void Password_C_Click(object sender, RoutedEventArgs e)
        {
            if (Password_C.IsChecked.Value)
            {
                Password_T.Visibility = Visibility.Visible;
            }
            else
            {
                Password_T.Visibility = Visibility.Hidden;
            }
        }
    }
}
public class Mod_Upload_Config
{
    public bool IsBGMMode;
    public bool IsPassword;
    public bool IsEnableR18;
    public string UserName;
    public string Password;
    public string Explanation;
}