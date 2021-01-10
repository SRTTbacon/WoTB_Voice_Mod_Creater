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
        bool IsMessageShowing = false;
        readonly List<string> Mod_Name_Full = new List<string>();
        Cauldron.FMOD.EVENT_LOADINFO ELI = new Cauldron.FMOD.EVENT_LOADINFO();
        Cauldron.FMOD.EventProject EP = new Cauldron.FMOD.EventProject();
        Cauldron.FMOD.Event FE = new Cauldron.FMOD.Event();
        public Mod_Uploads()
        {
            InitializeComponent();
            Password_T.Visibility = Visibility.Hidden;
        }
        async public void Window_Show()
        {
            //画面を表示
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
            //閉じる
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
            //リストに追加したファイルを削除
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
            //リストにアップロードするModファイルを追加
            OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "Modファイルを選択してください。",
                Filter = "Modファイル(*.yaml;*.fev;*.fsb;*.yaml.dvpl;*.fev.dvpl;*.fsb.dvpl)|*.yaml;*.fev;*.fsb;*.yaml.dvpl;*.fev.dvpl;*.fsb.dvpl",
                Multiselect = true
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                foreach (string Mod_File in ofd.FileNames)
                {
                    string Name = Path.GetFileName(Mod_File);
                    if (Path.GetExtension(Mod_File) == ".yaml" || Mod_File.Contains(".yaml.dvpl"))
                    {
                        if (Name != "sounds.yaml" && Name != "sounds.yaml.dvpl")
                        {
                            System.Windows.MessageBox.Show(".yamlファイルは、sounds.yamlファイル以外追加できません。\n現在のファイル名:" + Name);
                            continue;
                        }
                    }
                    for (int Number = 0; Number <= Mod_File_List.Items.Count - 1; Number++)
                    {
                        if (Mod_File_List.Items[Number].ToString() == Name)
                        {
                            System.Windows.MessageBox.Show("同名のファイルが存在します。\n現在のファイル名:" + Name);
                        }
                        else if (Name.Replace(".dvpl","") == Mod_File_List.Items[Number].ToString())
                        {
                            System.Windows.MessageBox.Show("同名の非dvplファイルが存在します。\n現在のファイル名:" + Name);
                        }
                        else if (Name + ".dvpl" == Mod_File_List.Items[Number].ToString())
                        {
                            System.Windows.MessageBox.Show("同名のdvplファイルが存在します。\n現在のファイル名:" + Name);
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
            //注意事項の画面
            string Message_01 = "・Mod名は、配布する際のタイトルになります。\n・ingame_voice以外の音声Modを指定するときは、sounds.yamlもリストに入れる必要があります。\n";
            string Message_02 = "・sfx_high(sfx_low).yamlは追加しなくてもソフト側で自動的にダウンロードしたユーザーに設定します。\n";
            string Message_03 = "・Modファイルに戦闘BGMが含まれる場合は\"BGMModに設定\"にチェックを入れるようにしてください。(Music.fevの構成はMusic/Music/Musicのみ)\n";
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
                Directory.CreateDirectory(Voice_Set.Special_Path + "/" + Mod_Create_Name_T.Text);
                Directory.Delete(Voice_Set.Special_Path + "/" + Mod_Create_Name_T.Text);
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
            if (Mod_Create_Name_T.Text == "Backup")
            {
                Message_T.Text = "そのMod名は別の目的に使用されています。";
            }
            if (Voice_Set.FTP_Server.DirectoryExists("/WoTB_Voice_Mod/Mods/" + Mod_Create_Name_T.Text))
            {
                Message_T.Text = "同名のModが既に存在します。";
                return;
            }
            //Modを配布
            if (BGM_Mode_C.IsChecked.Value)
            {
                //BGMModも一緒に配布する場合は実行
                try
                {
                    int Number = -1;
                    for (int Number_01 = 0; Number_01 <= Mod_Name_Full.Count - 1; Number_01++)
                    {
                        if (Path.GetFileName(Mod_Name_Full[Number_01]) == "Music.fev" || Path.GetFileName(Mod_Name_Full[Number_01]) == "Music.fev.dvpl")
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
                    await Task.Delay(50);
                    Fmod_Player.ESystem.Load(Mod_Name_Full[Number], ref ELI, ref EP);
                    Cauldron.FMOD.RESULT result = Fmod_Player.ESystem.GetEvent("Music/Music/Music", Cauldron.FMOD.EVENT_MODE.DEFAULT, ref FE);
                    EP.Release();
                    FE.Release();
                    if (result != Cauldron.FMOD.RESULT.OK)
                    {
                        throw new Exception("Music/Music/Musicが存在しません。");
                    }
                }
                catch
                {
                    Message_Feed_Out("BGMを付ける場合はファイル構成を\"Music/Music/Music\"にしてください。");
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
                await Task.Delay(50);
                //Modの情報をファイルに書き込む
                Mod_Upload_Config Configs = new Mod_Upload_Config()
                {
                    IsBGMMode = BGM_Mode_C.IsChecked.Value,
                    IsPassword = Password_C.IsChecked.Value,
                    IsEnableR18 = R_18_C.IsChecked.Value,
                    UserName = Voice_Set.UserName,
                    Explanation = Mod_Explanation_T.Text
                };
                //パスワードが有効だった場合
                if (Password_C.IsChecked.Value)
                {
                    Configs.Password = Password_T.Text;
                }
                //サーバーにフォルダを作成
                Voice_Set.FTP_Server.CreateDirectory("/WoTB_Voice_Mod/Mods/" + Mod_Create_Name_T.Text + "/Files", true);
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Mod_Upload_Config));
                StreamWriter streamWriter = new StreamWriter(Voice_Set.Special_Path + "/Temp_Create_Mod.dat", false, new UTF8Encoding(false));
                xmlSerializer.Serialize(streamWriter, Configs);
                streamWriter.Close();
                //Mod情報をアップロード
                Voice_Set.FTP_Server.UploadFile(Voice_Set.Special_Path + "/Temp_Create_Mod.dat", "/WoTB_Voice_Mod/Mods/" + Mod_Create_Name_T.Text + "/Configs.dat");
                File.Delete(Voice_Set.Special_Path + "/Temp_Create_Mod.dat");
                await Task.Delay(50);
                //Mod本体をアップロード
                foreach (string Upload_File in Mod_Name_Full)
                {
                    Voice_Set.FTP_Server.UploadFile(Upload_File, "/WoTB_Voice_Mod/Mods/" + Mod_Create_Name_T.Text + "/Files/" + Path.GetFileName(Upload_File));
                }
                Voice_Set.AppendString("/WoTB_Voice_Mod/Mods/Mod_Names.dat", Encoding.UTF8.GetBytes(Mod_Create_Name_T.Text + "\n"));
                IsBusy = false;
                Message_Feed_Out("Modを公開しました。");
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
            //パスワードにチェックを入れると入力するテキストボックスを表示させる
            if (Password_C.IsChecked.Value)
            {
                Password_T.Visibility = Visibility.Visible;
            }
            else
            {
                Password_T.Visibility = Visibility.Hidden;
            }
        }
        async void Message_Feed_Out(string Message)
        {
            //テキストが一定期間経ったらフェードアウト
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
    }
}
public class Mod_Upload_Config
{
    //Mod情報を保存する用
    public bool IsBGMMode;
    public bool IsPassword;
    public bool IsEnableR18;
    public string UserName;
    public string Password;
    public string Explanation;
}