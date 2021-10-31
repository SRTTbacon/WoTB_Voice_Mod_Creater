using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Xml.Linq;

namespace WoTB_Voice_Mod_Creater.Class
{
    public partial class Mod_Uploads : System.Windows.Controls.UserControl
    {
        bool IsBusy = false;
        bool IsMessageShowing = false;
        readonly List<string> Mod_Name_Full = new List<string>();
        /*Cauldron.FMOD.EVENT_LOADINFO ELI = new Cauldron.FMOD.EVENT_LOADINFO();
        Cauldron.FMOD.EventProject EP = new Cauldron.FMOD.EventProject();
        Cauldron.FMOD.Event FE = new Cauldron.FMOD.Event();*/
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
                Opacity += Sub_Code.Window_Feed_Time;
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
                    Opacity -= Sub_Code.Window_Feed_Time;
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
            /*string a = "voiceover_crew.bnk;";
            string b = "voiceover_crew.bnk.dvpl;";
            string c = "reload.bnk;";
            string d = "reload.bnk.dvpl;";
            string f = "ui_chat_quick_commands.bnk;";
            string g = "ui_chat_quick_commands.bnk.dvpl;";
            string h = "ui_battle.bnk;";
            string i = "ui_battle.bnk.dvpl";*/
            OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "Modファイルを選択してください。",
                Filter = "Modファイル(*.bnk;*.bnk.dvpl)|*.bnk;*.bnk.dvpl",
                Multiselect = true
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                foreach (string Mod_File in ofd.FileNames)
                {
                    string Name = Path.GetFileName(Mod_File);
                    /*if (Path.GetExtension(Mod_File) == ".yaml" || Mod_File.Contains(".yaml.dvpl"))
                    {
                        if (Name != "sounds.yaml" && Name != "sounds.yaml.dvpl")
                        {
                            System.Windows.MessageBox.Show(".yamlファイルは、sounds.yamlファイル以外追加できません。\n現在のファイル名:" + Name);
                            continue;
                        }
                    }*/
                    for (int Number = 0; Number <= Mod_File_List.Items.Count - 1; Number++)
                    {
                        if (Mod_File_List.Items[Number].ToString() == Name)
                        {
                            System.Windows.MessageBox.Show("同名のファイルが存在します。\n現在のファイル名:" + Name);
                        }
                        /*else if (Name.Replace(".dvpl","") == Mod_File_List.Items[Number].ToString())
                        {
                            System.Windows.MessageBox.Show("同名の非dvplファイルが存在します。\n現在のファイル名:" + Name);
                        }
                        else if (Name + ".dvpl" == Mod_File_List.Items[Number].ToString())
                        {
                            System.Windows.MessageBox.Show("同名のdvplファイルが存在します。\n現在のファイル名:" + Name);
                        }*/
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
            /*string Message_01 = "・Mod名は、配布する際のタイトルになります。\n・ingame_voice以外の音声Modを指定するときは、sounds.yamlもリストに入れる必要があります。\n";
            string Message_02 = "・sfx_high(sfx_low).yamlは追加しなくてもソフト側で自動的にダウンロードしたユーザーに設定します。\n";
            string Message_03 = "・Modファイルに戦闘BGMが含まれる場合は\"BGMModに設定\"にチェックを入れるようにしてください。(Music.fevの構成はMusic/Music/Musicのみ)\n";*/
            string Message_01 = "・Mod名は、配布する際のタイトルになります。\n";
            string Message_02 = "・このバージョンで入れることができるファイルは.bnkまたは.pck形式のみです。\n";
            string Message_03 = "・voiceover_crew.bnkにBGMが入っていれば\"BGMModに設定\"にチェックを入れてください。\n";
            string Message_04 = "・管理者が不必要なModファイルと判断した場合は本人の意思に関係なく削除する可能性があります。";
            System.Windows.MessageBox.Show(Message_01 + Message_02 + Message_03 + Message_04);
        }
        private async void Mod_Create_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
                return;
            if (Mod_File_List.Items.Count == 0)
            {
                Message_Feed_Out("最低1つはファイルが必要です。");
                return;
            }
            if (Mod_Create_Name_T.Text == "")
            {
                Message_Feed_Out("Mod名が指定されていません。");
                return;
            }
            try
            {
                Directory.CreateDirectory(Voice_Set.Special_Path + "/" + Mod_Create_Name_T.Text);
                Directory.Delete(Voice_Set.Special_Path + "/" + Mod_Create_Name_T.Text);
                if (Mod_Create_Name_T.Text.Contains("/"))
                {
                    Message_Feed_Out("Mod名に不適切な文字が含まれています。");
                    return;
                }
            }
            catch
            {
                Message_Feed_Out("Mod名に不適切な文字が含まれています。");
                return;
            }
            if (Mod_Create_Name_T.Text.CountOf("  ") > 0)
            {
                Message_Feed_Out("Mod名に空白を2つ続けて付けることはできません。");
                return;
            }
            if (Mod_Create_Name_T.Text == "Backup")
            {
                Message_Feed_Out("そのMod名は別の目的に使用されています。");
                return;
            }
            if (Voice_Set.FTPClient.Directory_Exist("/WoTB_Voice_Mod/Mods/" + Mod_Create_Name_T.Text))
            {
                Message_Feed_Out("同名のModが既に存在します。");
                return;
            }
            bool IsError = false;
            foreach (string File_Now in Mod_Name_Full)
            {
                if (!File.Exists(File_Now))
                {
                    IsError = true;
                    System.Windows.MessageBox.Show("次のファイルが見つかりませんでした。削除したか移動している可能性があります。\n" + File_Now);
                }
            }
            if (IsError)
            {
                Message_Feed_Out("エラー:指定されたファイルが存在しません。");
                return;
            }
            if (Mod_Explanation_T.Text == "")
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Modの説明が入力されていません。実行しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
                if (result == MessageBoxResult.No)
                    return;
            }
            Message_T.Text = "";
            IsBusy = true;
            try
            {
                Message_T.Text = "Modを公開しています...";
                await Task.Delay(50);
                //サーバーにフォルダを作成
                Voice_Set.FTPClient.Directory_Create("/WoTB_Voice_Mod/Mods/" + Mod_Create_Name_T.Text);
                Voice_Set.FTPClient.Directory_Create("/WoTB_Voice_Mod/Mods/" + Mod_Create_Name_T.Text + "/Files");
                //Modの情報をXMLファイルに書き込む
                XDocument xml = new XDocument();
                XElement datas = new XElement("Mod_Upload_Config",
                new XElement("IsBGMMode", BGM_Mode_C.IsChecked.Value),
                new XElement("IsPassword", Password_C.IsChecked.Value),
                new XElement("IsEnableR18", R_18_C.IsChecked.Value),
                new XElement("UserName", Voice_Set.UserName),
                new XElement("Explanation", Mod_Explanation_T.Text),
                new XElement("Password", Password_T.Text));
                xml.Add(datas);
                xml.Save(Voice_Set.Special_Path + "/Temp_Create_Mod.dat");
                //Mod情報をアップロード
                Voice_Set.FTPClient.UploadFile(Voice_Set.Special_Path + "/Temp_Create_Mod.dat", "/WoTB_Voice_Mod/Mods/" + Mod_Create_Name_T.Text + "/Configs.dat");
                File.Delete(Voice_Set.Special_Path + "/Temp_Create_Mod.dat");
                await Task.Delay(50);
                //Mod本体をアップロード
                foreach (string Upload_File in Mod_Name_Full)
                    Voice_Set.FTPClient.UploadFile(Upload_File, "/WoTB_Voice_Mod/Mods/" + Mod_Create_Name_T.Text + "/Files/" + Path.GetFileName(Upload_File));
                Voice_Set.AppendString("/WoTB_Voice_Mod/Mods/Mod_Names_Wwise.dat", Mod_Create_Name_T.Text + "\n");
                Voice_Set.TCP_Server.Send("Message|" + Voice_Set.UserName + "->Mod名:" + Mod_Create_Name_T.Text + "を公開しました。");
                IsBusy = false;
                Message_Feed_Out("Modを公開しました。");
                Window_Close();
            }
            catch (Exception e1)
            {
                System.Windows.MessageBox.Show("エラー:" + e1.Message);
                Message_Feed_Out("エラーが発生しました。");
                Sub_Code.Error_Log_Write(e1.Message);
                IsBusy = false;
                Window_Close();
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
                await Task.Delay(1000 / 30);
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
    }
}