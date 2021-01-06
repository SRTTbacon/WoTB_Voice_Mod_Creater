using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using WMPLib;

namespace WoTB_Voice_Mod_Creater.Class
{
    public partial class Voice_Create : System.Windows.Controls.UserControl
    {
        bool IsBusy = false;
        bool IsMessageShowing = false;
        bool IsCreating = false;
        List<List<string>> Voice_List_Full_File_Name = new List<List<string>>();
        List<List<string>> Voice_Sub_List_Full_File_Name = new List<List<string>>();
        readonly string Special_Path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/WoTB_Voice_Mod_Creater";
        readonly WindowsMediaPlayer Player = new WindowsMediaPlayer();
        public Voice_Create()
        {
            InitializeComponent();
            Voice_Sub_List.Visibility = Visibility.Hidden;
            Voice_Back_B.Visibility = Visibility.Hidden;
            List_Text_Reset();
        }
        void List_Text_Reset()
        {
            Voice_List.Items.Clear();
            Voice_Sub_List.Items.Clear();
            Voice_List.Items.Add("味方にダメージ | 未選択");
            Voice_List.Items.Add("弾薬庫 | 未選択");
            Voice_List.Items.Add("敵への無効弾 | 未選択");
            Voice_List.Items.Add("敵への貫通弾 | 未選択");
            Voice_List.Items.Add("敵への致命弾 | 未選択");
            Voice_List.Items.Add("敵への跳弾 | 未選択");
            Voice_List.Items.Add("車長負傷 | 未選択");
            Voice_List.Items.Add("操縦手負傷 | 未選択");
            Voice_List.Items.Add("敵炎上 | 未選択");
            Voice_List.Items.Add("敵撃破 | 未選択");
            Voice_List.Items.Add("エンジン破損 | 未選択");
            Voice_List.Items.Add("エンジン大破 | 未選択");
            Voice_List.Items.Add("エンジン復旧 | 未選択");
            Voice_List.Items.Add("自車両火災 | 未選択");
            Voice_List.Items.Add("自車両消火 | 未選択");
            Voice_List.Items.Add("燃料タンク破損 | 未選択");
            Voice_List.Items.Add("主砲破損 | 未選択");
            Voice_List.Items.Add("主砲大破 | 未選択");
            Voice_List.Items.Add("主砲復旧 | 未選択");
            Voice_List.Items.Add("砲手負傷 | 未選択");
            Voice_List.Items.Add("装填手負傷 | 未選択");
            Voice_List.Items.Add("通信機破損 | 未選択");
            Voice_List.Items.Add("通信手負傷 | 未選択");
            Voice_List.Items.Add("戦闘開始 | 未選択");
            Voice_List.Items.Add("観測装置破損 | 未選択");
            Voice_List.Items.Add("観測装置大破 | 未選択");
            Voice_List.Items.Add("観測装置復旧 | 未選択");
            Voice_List.Items.Add("履帯破損 | 未選択");
            Voice_List.Items.Add("履帯大破 | 未選択");
            Voice_List.Items.Add("履帯復旧 | 未選択");
            Voice_List.Items.Add("砲塔破損 | 未選択");
            Voice_List.Items.Add("砲塔大破 | 未選択");
            Voice_List.Items.Add("砲塔復旧 | 未選択");
            Voice_List.Items.Add("自車両大破 | 未選択");
            Voice_Sub_List.Items.Add("敵発見 | 未選択");
            Voice_Sub_List.Items.Add("第六感 | 未選択");
            Voice_Sub_List.Items.Add("了解 | 未選択");
            Voice_Sub_List.Items.Add("拒否 | 未選択");
            Voice_Sub_List.Items.Add("救援を請う | 未選択");
            Voice_Sub_List.Items.Add("攻撃せよ！ | 未選択");
            Voice_Sub_List.Items.Add("攻撃中 | 未選択");
            Voice_Sub_List.Items.Add("陣地を占領せよ！ | 未選択");
            Voice_Sub_List.Items.Add("陣地を防衛せよ！ | 未選択");
            Voice_Sub_List.Items.Add("固守せよ！ | 未選択");
            Voice_Sub_List.Items.Add("ロックオン | 未選択");
            Voice_Sub_List.Items.Add("アンロック | 未選択");
            Voice_Sub_List.Items.Add("装填完了 | 未選択");
            Voice_Sub_List.Items.Add("マップクリック時 | 未選択");
            Voice_Sub_List.Items.Add("戦闘終了時 | 未選択");
            Voice_List_Full_File_Name = new List<List<string>>();
            Voice_Sub_List_Full_File_Name = new List<List<string>>();
            for (int Number = 0; Number < 34; Number++)
            {
                List<string> Temp = new List<string>();
                Voice_List_Full_File_Name.Add(Temp);
            }
            for (int Number = 0; Number < 15; Number++)
            {
                List<string> Temp = new List<string>();
                Voice_Sub_List_Full_File_Name.Add(Temp);
            }
        }
        public async void Window_Show()
        {
            Volume_S.Value = 50;
            Opacity = 0;
            Visibility = Visibility.Visible;
            while (Opacity < 1 && !IsBusy)
            {
                Opacity += 0.025;
                await Task.Delay(1000 / 60);
            }
        }
        private async void Exit_B_Click(object sender, RoutedEventArgs e)
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
                IsBusy = false;
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
        private void Voice_Back_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
            Voice_Back_B.Visibility = Visibility.Hidden;
            Voice_Sub_List.Visibility = Visibility.Hidden;
            Voice_Next_B.Visibility = Visibility.Visible;
            Voice_List.Visibility = Visibility.Visible;
            Voice_List_T.Text = "音声リスト1";
            if (Voice_List.SelectedIndex != -1)
            {
                Voice_File_Reset(Voice_List_Full_File_Name, Voice_List.SelectedIndex);
            }
            else
            {
                Voice_File_List.Items.Clear();
            }
        }
        private void Voice_Next_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
            Voice_Back_B.Visibility = Visibility.Visible;
            Voice_Sub_List.Visibility = Visibility.Visible;
            Voice_Next_B.Visibility = Visibility.Hidden;
            Voice_List.Visibility = Visibility.Hidden;
            Voice_List_T.Text = "音声リスト2";
            if (Voice_Sub_List.SelectedIndex != -1)
            {
                Voice_File_Reset(Voice_Sub_List_Full_File_Name, Voice_Sub_List.SelectedIndex);
            }
            else
            {
                Voice_File_List.Items.Clear();
            }
        }
        private void Voice_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
            if (Voice_List.SelectedIndex != -1)
            {
                Voice_File_Reset(Voice_List_Full_File_Name, Voice_List.SelectedIndex);
            }
        }
        private void Voice_Sub_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
            if (Voice_Sub_List.SelectedIndex != -1)
            {
                Voice_File_Reset(Voice_Sub_List_Full_File_Name, Voice_Sub_List.SelectedIndex);
            }
        }
        void Voice_File_Reset(List<List<string>> List, int SelectIndex)
        {
            Voice_File_List.Items.Clear();
            List<string> Files = List[SelectIndex];
            if (Files.Count > 0)
            {
                foreach (string Temp in Files)
                {
                    Voice_File_List.Items.Add(Path.GetFileName(Temp));
                }
            }
        }
        private void Voice_List_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Voice_List.SelectedIndex = -1;
            Voice_File_List.Items.Clear();
        }
        private void Voice_Sub_List_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Voice_Sub_List.SelectedIndex = -1;
            Voice_File_List.Items.Clear();
        }
        private void Voice_Select_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy || IsCreating)
            {
                return;
            }
            if (Voice_List.SelectedIndex == -1 && Voice_List.Visibility == Visibility.Visible)
            {
                Message_Feed_Out("音声タイプが選択されていません。");
                return;
            }
            else if (Voice_Sub_List.SelectedIndex == -1 && Voice_Sub_List.Visibility == Visibility.Visible)
            {
                Message_Feed_Out("音声タイプが選択されていません。");
                return;
            }
            int IndexNumber = -1;
            if (Voice_List.Visibility == Visibility.Visible)
            {
                IndexNumber = Voice_List.SelectedIndex;
            }
            else if (Voice_Sub_List.Visibility == Visibility.Visible)
            {
                IndexNumber = Voice_Sub_List.SelectedIndex;
            }
            OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "音声ファイルを選択してください。",
                Filter = "音声ファイル(*.mp3;*.wav;*.ogg;*.flac;*.wma)|*.mp3;*.wav;*.ogg;*.flac;*.wma",
                Multiselect = true
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (Voice_List.Visibility == Visibility.Visible)
                {
                    List<string> Temp = Voice_List_Full_File_Name[Voice_List.SelectedIndex];
                    foreach (string SelectFile in ofd.FileNames)
                    {
                        Temp.Add(SelectFile);
                    }
                    Voice_List_Full_File_Name[Voice_List.SelectedIndex] = Temp;
                    Voice_File_Reset(Voice_List_Full_File_Name, Voice_List.SelectedIndex);
                    Voice_List.Items[Voice_List.SelectedIndex] = Voice_List.Items[Voice_List.SelectedIndex].ToString().Replace("未選択", "選択済み");
                    Voice_List.SelectedIndex = IndexNumber;
                }
                else if (Voice_Sub_List.Visibility == Visibility.Visible)
                {
                    List<string> Temp = Voice_Sub_List_Full_File_Name[Voice_Sub_List.SelectedIndex];
                    foreach (string SelectFile in ofd.FileNames)
                    {
                        Temp.Add(SelectFile);
                    }
                    Voice_Sub_List_Full_File_Name[Voice_Sub_List.SelectedIndex] = Temp;
                    Voice_File_Reset(Voice_Sub_List_Full_File_Name, Voice_Sub_List.SelectedIndex);
                    Voice_Sub_List.Items[Voice_Sub_List.SelectedIndex] = Voice_Sub_List.Items[Voice_Sub_List.SelectedIndex].ToString().Replace("未選択", "選択済み");
                    Voice_Sub_List.SelectedIndex = IndexNumber;
                }
            }
        }
        private void Voice_Delete_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy || IsCreating)
            {
                return;
            }
            if (Voice_File_List.SelectedIndex == -1)
            {
                Message_Feed_Out("取消したい音声ファイルが選択されていません。");
                return;
            }
            int Number = Voice_File_List.SelectedIndex;
            Voice_File_List.SelectedIndex = -1;
            if (Voice_List.Visibility == Visibility.Visible)
            {
                List<string> Temp = Voice_List_Full_File_Name[Voice_List.SelectedIndex];
                Temp.RemoveAt(Number);
                Voice_List_Full_File_Name[Voice_List.SelectedIndex] = Temp;
                Voice_File_List.Items.RemoveAt(Number);
                if (Temp.Count == 0)
                {
                    Voice_List.Items[Voice_List.SelectedIndex] = Voice_List.Items[Voice_List.SelectedIndex].ToString().Replace("選択済み", "未選択");
                }
            }
            else if (Voice_Sub_List.Visibility == Visibility.Visible)
            {
                List<string> Temp = Voice_Sub_List_Full_File_Name[Voice_Sub_List.SelectedIndex];
                Temp.RemoveAt(Number);
                Voice_List_Full_File_Name[Voice_Sub_List.SelectedIndex] = Temp;
                Voice_File_List.Items.RemoveAt(Number);
                if (Temp.Count == 0)
                {
                    Voice_Sub_List.Items[Voice_Sub_List.SelectedIndex] = Voice_Sub_List.Items[Voice_Sub_List.SelectedIndex].ToString().Replace("選択済み", "未選択");
                }
            }
        }
        private void Volume_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Player.settings.volume = (int)Volume_S.Value;
            Volume_T.Text = "音量:" + (int)Volume_S.Value;
        }
        private void Voice_Play_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
            if (Voice_File_List.SelectedIndex == -1)
            {
                Message_Feed_Out("音声ファイルが選択されていません。");
                return;
            }
            if (Voice_List.Visibility == Visibility.Visible)
            {
                List<string> Temp = Voice_List_Full_File_Name[Voice_List.SelectedIndex];
                if (!File.Exists(Temp[Voice_File_List.SelectedIndex]))
                {
                    Message_Feed_Out("音声ファイルが存在しません。削除された可能性があります。");
                    return;
                }
                Player.URL = Temp[Voice_File_List.SelectedIndex];
                Player.controls.play();
            }
            else if (Voice_Sub_List.Visibility == Visibility.Visible)
            {
                List<string> Temp = Voice_Sub_List_Full_File_Name[Voice_Sub_List.SelectedIndex];
                if (!File.Exists(Temp[Voice_File_List.SelectedIndex]))
                {
                    Message_Feed_Out("音声ファイルが存在しません。削除された可能性があります。");
                    return;
                }
                Player.URL = Temp[Voice_File_List.SelectedIndex];
                Player.controls.play();
            }
        }
        private void Voice_Pause_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
            Player.controls.pause();
        }
        private void Voice_Save_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy || IsCreating)
            {
                return;
            }
            SaveFileDialog sfd = new SaveFileDialog()
            {
                Title = "保存先を指定してください。",
                InitialDirectory = Directory.GetCurrentDirectory(),
                Filter = "セーブファイル(*.wvs)|*.wvs",
            };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    StreamWriter stw = File.CreateText(Special_Path + "/Temp_Voice_Save.dat");
                    stw.WriteLine(Project_Name_T.Text);
                    int Number = 0;
                    foreach (List<string> Lists in Voice_List_Full_File_Name)
                    {
                        foreach (string Files in Lists)
                        {
                            stw.WriteLine(Number + "|" + Files);
                        }
                        Number++;
                    }
                    foreach (List<string> Lists in Voice_Sub_List_Full_File_Name)
                    {
                        foreach (string Files in Lists)
                        {
                            stw.WriteLine(Number + "|" + Files);
                        }
                        Number++;
                    }
                    stw.Close();
                    using (var eifs = new FileStream(Special_Path + "/Temp_Voice_Save.dat", FileMode.Open, FileAccess.Read))
                    {
                        using (var eofs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write))
                        {
                            FileEncode.FileEncryptor.Encrypt(eifs, eofs, "SRTTbacon_Create_Voice_Save");
                        }
                    }
                    File.Delete(Special_Path + "/Temp_Voice_Save.dat");
                    Message_Feed_Out("セーブしました。");
                }
                catch
                {
                    Message_Feed_Out("指定したファイルにアクセスできませんでした。");
                }
            }
        }
        private void Voice_Load_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy || IsCreating)
            {
                return;
            }
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Title = "セーブファイルを選択してください。",
                Filter = "セーブファイル(*.wvs)|*.wvs",
                Multiselect = false,
                InitialDirectory = Directory.GetCurrentDirectory()
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (var eifs = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read))
                    {
                        using (var eofs = new FileStream(Special_Path + "/Temp_Load_Voice.dat", FileMode.Create, FileAccess.Write))
                        {
                            FileEncode.FileEncryptor.Decrypt(eifs, eofs, "SRTTbacon_Create_Voice_Save");
                        }
                    }
                    string line;
                    StreamReader file = new StreamReader(Special_Path + "/Temp_Load_Voice.dat");
                    Project_Name_T.Text = file.ReadLine();
                    List_Text_Reset();
                    while ((line = file.ReadLine()) != null)
                    {
                        int Number = int.Parse(line.Substring(0, line.IndexOf('|')));
                        string File_Path = line.Substring(line.IndexOf('|') + 1);
                        if (Number < 34)
                        {
                            List<string> List_Number = Voice_List_Full_File_Name[Number];
                            List_Number.Add(File_Path);
                            Voice_List.Items[Number] = Voice_List.Items[Number].ToString().Replace("未選択", "選択済み");
                            Voice_List_Full_File_Name[Number] = List_Number;
                        }
                        else
                        {
                            List<string> List_Number = Voice_Sub_List_Full_File_Name[Number - 34];
                            List_Number.Add(File_Path);
                            Voice_Sub_List.Items[Number - 34] = Voice_Sub_List.Items[Number - 34].ToString().Replace("未選択", "選択済み");
                            Voice_Sub_List_Full_File_Name[Number - 34] = List_Number;
                        }
                    }
                    file.Close();
                    File.Delete(Special_Path + "/Temp_Load_Voice.dat");
                    if (Voice_List.Visibility == Visibility.Visible && Voice_List.SelectedIndex != -1)
                    {
                        Voice_File_Reset(Voice_List_Full_File_Name, Voice_List.SelectedIndex);
                    }
                    else if (Voice_Sub_List.Visibility == Visibility.Visible && Voice_Sub_List.SelectedIndex != -1)
                    {
                        Voice_File_Reset(Voice_Sub_List_Full_File_Name, Voice_Sub_List.SelectedIndex);
                    }
                    Message_Feed_Out("ロードしました。");
                }
                catch
                {
                    Message_Feed_Out("指定したセーブデータが破損しています。");
                }
            }
        }
        private async void Voice_Create_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy || IsCreating)
            {
                return;
            }
            if (Voice_Set.WoTB_Path == "")
            {
                Message_Feed_Out("WoTBのインストール場所を取得できませんでした。");
                return;
            }
            if (Project_Name_T.Text == "")
            {
                Message_Feed_Out("プロジェクト名が設定されていません。");
                return;
            }
            if (Project_Name_T.Text.Contains("  "))
            {
                Message_Feed_Out("プロジェクト名に空白を連続で使用することはできません。");
                return;
            }
            try
            {
                Directory.CreateDirectory(Special_Path + "/Temp/" + Project_Name_T.Text);
                Directory.Delete(Special_Path + "/Temp/" + Project_Name_T.Text);
                if (Project_Name_T.Text.Contains("/"))
                {
                    Message_Feed_Out("プロジェクト名に不適切な文字が含まれています。");
                    return;
                }
            }
            catch
            {
                Message_Feed_Out("プロジェクト名に不適切な文字が含まれています。");
                return;
            }
            IsCreating = true;
            string Dir_Name = Directory.GetCurrentDirectory() + "/Projects/" + Project_Name_T.Text;
            List<List<string>> Temp = new List<List<string>>();
            for (int Number_01 = 0; Number_01 <= Voice_List_Full_File_Name.Count - 1; Number_01++)
            {
                Temp.Add(Voice_List_Full_File_Name[Number_01]);
            }
            for (int Number_02 = 0; Number_02 <= Voice_Sub_List_Full_File_Name.Count - 1; Number_02++)
            {
                Temp.Add(Voice_Sub_List_Full_File_Name[Number_02]);
            }
            Voice_Create_Window.Window_Show_V2(Project_Name_T.Text, Temp);
            Voice_Create_Window.Opacity = 0;
            Voice_Create_Window.Visibility = Visibility.Visible;
            while (Voice_Create_Window.Opacity < 1)
            {
                Voice_Create_Window.Opacity += 0.025;
                await Task.Delay(1000 / 60);
            }
            while (Voice_Create_Window.Visibility == Visibility.Visible)
            {
                await Task.Delay(100);
            }
            if (Sub_Code.CreatingProject)
            {
                Sub_Code.CreatingProject = false;
                try
                {
                    Directory.Delete(Dir_Name + "/Voices", true);
                    Directory.Delete(Dir_Name + "/" + Project_Name_T.Text + "_Mod", true);
                }
                catch
                {

                }
                Message_T.Text = "ファイルをコピーしています...";
                await Task.Delay(10);
                Directory.CreateDirectory(Dir_Name + "/Voices");
                if (Sub_Code.Set_Voice_Type_Change_Name_By_Index(Dir_Name + "/Voices", Temp) != "")
                {
                    Message_Feed_Out("ファイルをコピーできませんでした。もう一度お試しください。");
                    Directory.Delete(Dir_Name);
                    return;
                }
                if (Sub_Code.VolumeSet)
                {
                    Message_T.Text = "音量を均一にしています...";
                    await Task.Delay(10);
                    await Sub_Code.Change_MP3_Encode(Dir_Name + "/Voices");
                }
                string File_Name = Project_Name_T.Text.Replace(" ", "_");
                Voice_Mod_Create.Project_Create(Message_T, Project_Name_T.Text, Dir_Name + "/Voices", Special_Path + "/SE");
                await Sub_Code.Project_Build(Dir_Name + "/" + Project_Name_T.Text.Replace(" ", "_") + ".fdp", Message_T);
                DateTime dt = DateTime.Now;
                string Time = Sub_Code.Get_Time_Now(dt, ".", 1, 6);
                Directory.CreateDirectory(Dir_Name + "/" + Project_Name_T.Text + "_Mod/Mods");
                Directory.CreateDirectory(Dir_Name + "/" + Project_Name_T.Text + "_Mod/Configs/Sfx");
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/Backup/" + Time);
                try
                {
                    File.Copy(Dir_Name + "/" + File_Name + ".fev", Dir_Name + "/" + File_Name + "_Mod/Mods/" + File_Name + ".fev", true);
                    File.Copy(Dir_Name + "/" + File_Name + ".fsb", Dir_Name + "/" + File_Name + "_Mod/Mods/" + File_Name + ".fsb", true);
                    File.Delete(Dir_Name + "/" + File_Name + ".fev");
                    File.Delete(Dir_Name + "/" + File_Name + ".fsb");
                    File.Delete(Dir_Name + "/fmod_designer.log");
                    File.Delete(Dir_Name + "/undo-log.txt");
                }
                catch
                {

                }
                Sub_Code.DVPL_File_Copy(Voice_Set.WoTB_Path + "/Data/sounds.yaml", Directory.GetCurrentDirectory() + "/Backup/" + Time + "/sounds.yaml", false);
                Sub_Code.DVPL_File_Copy(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_high.yaml", Directory.GetCurrentDirectory() + "/Backup/" + Time + "/sfx_high.yaml", false);
                Sub_Code.DVPL_File_Copy(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_low.yaml", Directory.GetCurrentDirectory() + "/Backup/" + Time + "/sfx_low.yaml", false);
                if (File.Exists(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_high.yaml.dvpl"))
                {
                    Sub_Code.DVPL_Unlock(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_high.yaml.dvpl", Dir_Name + "/" + Project_Name_T.Text + "_Mod/Configs/Sfx/sfx_high.yaml", true);
                }
                else if (File.Exists(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_high.yaml"))
                {
                    File.Copy(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_high.yaml", Dir_Name + "/" + Project_Name_T.Text + "_Mod/Configs/Sfx/sfx_high.yaml");
                }
                if (File.Exists(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_low.yaml.dvpl"))
                {
                    Sub_Code.DVPL_Unlock(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_low.yaml.dvpl", Dir_Name + "/" + Project_Name_T.Text + "_Mod/Configs/Sfx/sfx_low.yaml", true);
                }
                else if (File.Exists(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_low.yaml"))
                {
                    File.Copy(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_low.yaml", Dir_Name + "/" + Project_Name_T.Text + "_Mod/Configs/Sfx/sfx_low.yaml");
                }
                string[] Configs = { "sfx_high.yaml", "sfx_low.yaml" };
                foreach (string File_Now in Configs)
                {
                    StreamReader str2 = new StreamReader(Dir_Name + "/" + Project_Name_T.Text + "_Mod/Configs/Sfx/" + File_Now);
                    string[] Read = str2.ReadToEnd().Split('\n');
                    str2.Close();
                    bool IsExist_Voice = false;
                    bool IsExist_Music = false;
                    foreach (string Line in Read)
                    {
                        if (Line.Contains(File_Name + ".fev"))
                        {
                            IsExist_Voice = true;
                        }
                        if (Line.Contains("Music.fev"))
                        {
                            IsExist_Music = true;
                        }
                    }
                    StreamWriter stw4 = new StreamWriter(Dir_Name + "/" + Project_Name_T.Text + "_Mod/Configs/Sfx/" + File_Now, true);
                    if (!IsExist_Voice)
                    {
                        stw4.Write("\n -\n  \"~res:/Mods/" + File_Name + ".fev\"");
                    }
                    if (!IsExist_Music)
                    {
                        stw4.Write("\n -\n  \"~res:/Mods/Music.fev\"");
                    }
                    stw4.Close();
                }
                File.Copy(Special_Path + "/Temp_Sounds.yaml", Dir_Name + "/" + Project_Name_T.Text + "_Mod/sounds.yaml", true);
                File.Delete(Special_Path + "/Temp_Sounds.yaml");
                if (Sub_Code.DVPL_Encode)
                {
                    Message_T.Text = "DVPL化しています...";
                    await Task.Delay(10);
                    try
                    {
                        DVPL.DVPL_Encode(Dir_Name + "/" + Project_Name_T.Text + "_Mod/Mods/" + File_Name + ".fev");
                        DVPL.DVPL_Encode(Dir_Name + "/" + Project_Name_T.Text + "_Mod/Mods/" + File_Name + ".fsb");
                        DVPL.DVPL_Encode(Dir_Name + "/" + Project_Name_T.Text + "_Mod/Configs/Sfx/sfx_high.yaml");
                        DVPL.DVPL_Encode(Dir_Name + "/" + Project_Name_T.Text + "_Mod/Configs/Sfx/sfx_low.yaml");
                        DVPL.DVPL_Encode(Dir_Name + "/" + Project_Name_T.Text + "_Mod/sounds.yaml");
                        File.Delete(Dir_Name + "/" + Project_Name_T.Text + "_Mod/Mods/" + File_Name + ".fev");
                        File.Delete(Dir_Name + "/" + Project_Name_T.Text + "_Mod/Mods/" + File_Name + ".fsb");
                        File.Delete(Dir_Name + "/" + Project_Name_T.Text + "_Mod/Configs/Sfx/sfx_high.yaml");
                        File.Delete(Dir_Name + "/" + Project_Name_T.Text + "_Mod/Configs/Sfx/sfx_low.yaml");
                        File.Delete(Dir_Name + "/" + Project_Name_T.Text + "_Mod/sounds.yaml");
                    }
                    catch
                    {
                        Message_Feed_Out("エラー:DVPL化できませんでした。");
                        return;
                    }
                }
                MessageBoxResult result = System.Windows.MessageBox.Show("完了しました。WoTBに適応しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes);
                if (result == MessageBoxResult.Yes)
                {
                    if (Voice_Set.WoTB_Path == "")
                    {
                        Message_Feed_Out("WoTBのインストール場所を取得できませんでした。");
                        return;
                    }
                    try
                    {
                        Directory.CreateDirectory(Voice_Set.WoTB_Path + "/Data/Mods");
                        Sub_Code.DVPL_File_Delete(Voice_Set.WoTB_Path + "/Data/sounds.yaml");
                        Sub_Code.DVPL_File_Delete(Voice_Set.WoTB_Path + "/Data/Mods/" + File_Name + ".fev");
                        Sub_Code.DVPL_File_Delete(Voice_Set.WoTB_Path + "/Data/Mods/" + File_Name + ".fsb");
                        Sub_Code.DVPL_File_Delete(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_high.yaml");
                        Sub_Code.DVPL_File_Delete(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_low.yaml");
                        Sub_Code.DVPL_File_Copy(Dir_Name + "/" + Project_Name_T.Text + "_Mod/sounds.yaml", Voice_Set.WoTB_Path + "/Data/sounds.yaml", true);
                        Sub_Code.DVPL_File_Copy(Dir_Name + "/" + Project_Name_T.Text + "_Mod/Mods/" + File_Name + ".fev", Voice_Set.WoTB_Path + "/Data/Mods/" + File_Name + ".fev", true);
                        Sub_Code.DVPL_File_Copy(Dir_Name + "/" + Project_Name_T.Text + "_Mod/Mods/" + File_Name + ".fsb", Voice_Set.WoTB_Path + "/Data/Mods/" + File_Name + ".fsb", true);
                        Sub_Code.DVPL_File_Copy(Dir_Name + "/" + Project_Name_T.Text + "_Mod/Configs/Sfx/sfx_high.yaml", Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_high.yaml", true);
                        Sub_Code.DVPL_File_Copy(Dir_Name + "/" + Project_Name_T.Text + "_Mod/Configs/Sfx/sfx_low.yaml", Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_low.yaml", true);
                    }
                    catch
                    {
                        Message_Feed_Out("WoTBに適応できませんでした。");
                        return;
                    }
                }
                Message_Feed_Out("完了しました。");
            }
            IsCreating = false;
        }
    }
}