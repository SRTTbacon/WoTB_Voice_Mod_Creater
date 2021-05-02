using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;

namespace WoTB_Voice_Mod_Creater.Class
{
    public partial class Voice_Create : System.Windows.Controls.UserControl
    {
        int Stream;
        bool IsBusy = false;
        bool IsMessageShowing = false;
        bool IsCreating = false;
        bool IsNewMode = false;
        List<string> Main_Voice_List = new List<string>();
        List<string> Sub_Voice_List = new List<string>();
        List<List<string>> Voice_List_Full_File_Name = new List<List<string>>();
        List<List<string>> Voice_Sub_List_Full_File_Name = new List<List<string>>();
        public Voice_Create()
        {
            InitializeComponent();
            Voice_Sub_List.Visibility = Visibility.Hidden;
            Voice_Back_B.Visibility = Visibility.Hidden;
            Volume_S.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Volume_MouseUp), true);
            Volume_S.Value = 50;
            if (File.Exists(Voice_Set.Special_Path + "/Configs/Voice_Create.conf"))
            {
                try
                {
                    using (var eifs = new FileStream(Voice_Set.Special_Path + "/Configs/Voice_Create.conf", FileMode.Open, FileAccess.Read))
                    {
                        using (var eofs = new FileStream(Voice_Set.Special_Path + "/Configs/Temp_Voice_Create.tmp", FileMode.Create, FileAccess.Write))
                        {
                            FileEncode.FileEncryptor.Decrypt(eifs, eofs, "Voice_Create_Configs_Save");
                        }
                    }
                    StreamReader str = new StreamReader(Voice_Set.Special_Path + "/Configs/Temp_Voice_Create.tmp");
                    Volume_S.Value = double.Parse(str.ReadLine());
                    try
                    {
                        ColorMode_C.IsChecked = bool.Parse(str.ReadLine());
                    }
                    catch
                    {
                        ColorMode_C.IsChecked = false;
                    }
                    str.Close();
                    File.Delete(Voice_Set.Special_Path + "/Configs/Temp_Voice_Create.tmp");
                }
                catch
                {
                }
            }
            List_Text_Reset();
        }
        void List_Text_Reset()
        {
            //リストの状態を初期化
            Voice_List.Items.Clear();
            Voice_Sub_List.Items.Clear();
            Voice_File_List.Items.Clear();
            Main_Voice_List.Clear();
            Sub_Voice_List.Clear();
            Main_Voice_List.Add("味方にダメージ | 未選択");
            Main_Voice_List.Add("弾薬庫 | 未選択");
            Main_Voice_List.Add("敵への無効弾 | 未選択");
            Main_Voice_List.Add("敵への貫通弾 | 未選択");
            Main_Voice_List.Add("敵への致命弾 | 未選択");
            Main_Voice_List.Add("敵への跳弾 | 未選択");
            Main_Voice_List.Add("車長負傷 | 未選択");
            Main_Voice_List.Add("操縦手負傷 | 未選択");
            Main_Voice_List.Add("敵炎上 | 未選択");
            Main_Voice_List.Add("敵撃破 | 未選択");
            Main_Voice_List.Add("エンジン破損 | 未選択");
            Main_Voice_List.Add("エンジン大破 | 未選択");
            Main_Voice_List.Add("エンジン復旧 | 未選択");
            Main_Voice_List.Add("自車両火災 | 未選択");
            Main_Voice_List.Add("自車両消火 | 未選択");
            Main_Voice_List.Add("燃料タンク破損 | 未選択");
            Main_Voice_List.Add("主砲破損 | 未選択");
            Main_Voice_List.Add("主砲大破 | 未選択");
            Main_Voice_List.Add("主砲復旧 | 未選択");
            Main_Voice_List.Add("砲手負傷 | 未選択");
            Main_Voice_List.Add("装填手負傷 | 未選択");
            Main_Voice_List.Add("通信機破損 | 未選択");
            Main_Voice_List.Add("通信手負傷 | 未選択");
            Main_Voice_List.Add("戦闘開始 | 未選択");
            Main_Voice_List.Add("観測装置破損 | 未選択");
            Main_Voice_List.Add("観測装置大破 | 未選択");
            Main_Voice_List.Add("観測装置復旧 | 未選択");
            Main_Voice_List.Add("履帯破損 | 未選択");
            Main_Voice_List.Add("履帯大破 | 未選択");
            Main_Voice_List.Add("履帯復旧 | 未選択");
            Main_Voice_List.Add("砲塔破損 | 未選択");
            Main_Voice_List.Add("砲塔大破 | 未選択");
            Main_Voice_List.Add("砲塔復旧 | 未選択");
            Main_Voice_List.Add("自車両大破 | 未選択");
            Sub_Voice_List.Add("敵発見 | 未選択");
            Sub_Voice_List.Add("第六感 | 未選択");
            Sub_Voice_List.Add("了解 | 未選択");
            Sub_Voice_List.Add("拒否 | 未選択");
            Sub_Voice_List.Add("救援を請う | 未選択");
            Sub_Voice_List.Add("攻撃せよ！ | 未選択");
            Sub_Voice_List.Add("攻撃中 | 未選択");
            Sub_Voice_List.Add("陣地を占領せよ！ | 未選択");
            Sub_Voice_List.Add("陣地を防衛せよ！ | 未選択");
            Sub_Voice_List.Add("固守せよ！ | 未選択");
            Sub_Voice_List.Add("ロックオン | 未選択");
            Sub_Voice_List.Add("アンロック | 未選択");
            Sub_Voice_List.Add("装填完了 | 未選択");
            Sub_Voice_List.Add("マップクリック時 | 未選択");
            Sub_Voice_List.Add("戦闘終了時 | 未選択");
            Sub_Voice_List.Add("戦闘BGM | 未選択");
            for (int Number = 0; Number < Main_Voice_List.Count; Number++)
            {
                Voice_List.Items.Add(Main_Voice_List[Number]);
            }
            for (int Number = 0; Number < Sub_Voice_List.Count; Number++)
            {
                Voice_Sub_List.Items.Add(Sub_Voice_List[Number]);
            }
            ColorMode_Change();
            Voice_List_Full_File_Name = new List<List<string>>();
            Voice_Sub_List_Full_File_Name = new List<List<string>>();
            for (int Number = 0; Number < 34; Number++)
            {
                List<string> Temp = new List<string>();
                Voice_List_Full_File_Name.Add(Temp);
            }
            for (int Number = 0; Number < 16; Number++)
            {
                List<string> Temp = new List<string>();
                Voice_Sub_List_Full_File_Name.Add(Temp);
            }
        }
        //引数:新サウンドエンジンの音声Mod=true,旧サウンドエンジン=false
        public async void Window_Show(bool Mode)
        {
            //画面を表示
            IsNewMode = Mode;
            Volume_S.Value = 50;
            if (File.Exists(Voice_Set.Special_Path + "/Configs/Voice_Create.conf"))
            {
                try
                {
                    using (var eifs = new FileStream(Voice_Set.Special_Path + "/Configs/Voice_Create.conf", FileMode.Open, FileAccess.Read))
                    {
                        using (var eofs = new FileStream(Voice_Set.Special_Path + "/Configs/Temp_Voice_Create.tmp", FileMode.Create, FileAccess.Write))
                        {
                            FileEncode.FileEncryptor.Decrypt(eifs, eofs, "Voice_Create_Configs_Save");
                        }
                    }
                    StreamReader str = new StreamReader(Voice_Set.Special_Path + "/Configs/Temp_Voice_Create.tmp");
                    Volume_S.Value = double.Parse(str.ReadLine());
                    try
                    {
                        ColorMode_C.IsChecked = bool.Parse(str.ReadLine());
                        ColorMode_Change();
                        BGM_Reload_C.IsChecked = bool.Parse(str.ReadLine());
                    }
                    catch
                    {
                    }
                    str.Close();
                    File.Delete(Voice_Set.Special_Path + "/Configs/Temp_Voice_Create.tmp");
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show("設定を読み込めませんでした。。\nエラー回避のため設定は削除されます。");
                    File.Delete(Voice_Set.Special_Path + "/Configs/Voice_Create.conf");
                    Volume_S.Value = 75;
                    Sub_Code.Error_Log_Write(e.Message);
                }
            }
            Opacity = 0;
            Visibility = Visibility.Visible;
            while (Opacity < 1 && !IsBusy)
            {
                Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
        }
        private async void Exit_B_Click(object sender, RoutedEventArgs e)
        {
            //閉じる
            if (!IsBusy)
            {
                IsBusy = true;
                Bass.BASS_ChannelStop(Stream);
                Bass.BASS_StreamFree(Stream);
                while (Opacity > 0)
                {
                    Opacity -= Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
                Visibility = Visibility.Hidden;
                IsBusy = false;
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
            Message_T.Text = "";
            Message_T.Opacity = 1;
        }
        private void Voice_Back_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
            //音声リスト1へ移動
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
            //音声リスト2へ移動
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
                //音声が選択されたら実行
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
                //↑と同様
                Voice_File_Reset(Voice_Sub_List_Full_File_Name, Voice_Sub_List.SelectedIndex);
            }
        }
        void Voice_File_Reset(List<List<string>> List, int SelectIndex)
        {
            //選択されているタイプの音声を取得してリストに追加
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
            //種類の選択を解除
            Voice_List.SelectedIndex = -1;
            Voice_File_List.Items.Clear();
        }
        private void Voice_Sub_List_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //音声の選択を解除
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
            //選択している音声の種類に音声ファイルを追加
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
                //fmod designerが対応しているファイルのみ
                Title = "音声ファイルを選択してください。",
                Filter = "音声ファイル(*.mp3;*.wav;*.ogg;*.flac;*.wma;*.aac;*.mp4)|*.mp3;*.wav;*.ogg;*.flac;*.wma;*.aac;*.mp4",
                Multiselect = true
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                //音声を追加しそのタイプを選択済みにする
                if (Voice_List.Visibility == Visibility.Visible)
                {
                    List<string> Temp = Voice_List_Full_File_Name[IndexNumber];
                    foreach (string SelectFile in ofd.FileNames)
                    {
                        Temp.Add(SelectFile);
                    }
                    Voice_List_Full_File_Name[IndexNumber] = Temp;
                    Voice_File_Reset(Voice_List_Full_File_Name, IndexNumber);
                    Main_Voice_List[IndexNumber] = Main_Voice_List[IndexNumber].Replace("未選択", "選択済み");
                    Voice_List.Items[IndexNumber] = Main_Voice_List[IndexNumber];
                    if (ColorMode_C.IsChecked.Value)
                    {
                        ListBoxItem LBI = new ListBoxItem();
                        LBI.Content = Main_Voice_List[IndexNumber];
                        //LBI.Foreground = (Brush)new BrushConverter().ConvertFromString("#BFFF2C8C");
                        Voice_List.Items[IndexNumber] = LBI;
                    }
                    Voice_List.SelectedIndex = IndexNumber;
                }
                else if (Voice_Sub_List.Visibility == Visibility.Visible)
                {
                    List<string> Temp = Voice_Sub_List_Full_File_Name[Voice_Sub_List.SelectedIndex];
                    foreach (string SelectFile in ofd.FileNames)
                    {
                        Temp.Add(SelectFile);
                    }
                    Voice_Sub_List_Full_File_Name[IndexNumber] = Temp;
                    Voice_File_Reset(Voice_Sub_List_Full_File_Name, IndexNumber);
                    Sub_Voice_List[IndexNumber] = Sub_Voice_List[IndexNumber].Replace("未選択", "選択済み");
                    Voice_Sub_List.Items[IndexNumber] = Sub_Voice_List[IndexNumber];
                    if (ColorMode_C.IsChecked.Value)
                    {
                        ListBoxItem LBI = new ListBoxItem();
                        LBI.Content = Sub_Voice_List[IndexNumber];
                        //LBI.Foreground = (Brush)new BrushConverter().ConvertFromString("#BFFF2C8C");
                        Voice_Sub_List.Items[IndexNumber] = LBI;
                    }
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
            //選択している音声をリストから削除
            //音声が1つしかなかった場合選択済みから未選択に変える
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
                    int Number_Selected = Voice_List.SelectedIndex;
                    Main_Voice_List[Number_Selected] = Main_Voice_List[Number_Selected].Replace("選択済み", "未選択");
                    Voice_List.Items[Number_Selected] = Main_Voice_List[Number_Selected];
                    if (ColorMode_C.IsChecked.Value)
                    {
                        ListBoxItem LBI = new ListBoxItem();
                        LBI.Content = Main_Voice_List[Number_Selected];
                        LBI.Foreground = (Brush)new BrushConverter().ConvertFromString("#BFFF2C8C");
                        Voice_List.Items[Number_Selected] = LBI;
                    }
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
                    int Number_Selected = Voice_Sub_List.SelectedIndex;
                    Sub_Voice_List[Number_Selected] = Sub_Voice_List[Number_Selected].Replace("選択済み", "未選択");
                    Voice_Sub_List.Items[Number_Selected] = Sub_Voice_List[Number_Selected];
                    if (ColorMode_C.IsChecked.Value)
                    {
                        ListBoxItem LBI = new ListBoxItem();
                        LBI.Content = Sub_Voice_List[Number_Selected].Replace("System.Windows.Controls.ListBoxItem: ", "");
                        LBI.Foreground = (Brush)new BrushConverter().ConvertFromString("#BFFF2C8C");
                        Voice_Sub_List.Items[Number_Selected] = LBI;
                    }
                }
            }
            if (Voice_File_List.Items.Count > Number)
            {
                Voice_File_List.SelectedIndex = Number;
            }
        }
        private void Voice_File_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Bass.BASS_ChannelStop(Stream);
            Bass.BASS_StreamFree(Stream);
        }
        private void Volume_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //音量を変更
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)Volume_S.Value / 100);
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
            //選択している音声をファイルから再生
            //ファイルがなかった場合メッセージを表示
            if (Voice_List.Visibility == Visibility.Visible)
            {
                List<string> Temp = Voice_List_Full_File_Name[Voice_List.SelectedIndex];
                if (!File.Exists(Temp[Voice_File_List.SelectedIndex]))
                {
                    Message_Feed_Out("音声ファイルが存在しません。削除された可能性があります。");
                    return;
                }
                Bass.BASS_ChannelStop(Stream);
                Bass.BASS_StreamFree(Stream);
                int StreamHandle = Bass.BASS_StreamCreateFile(Temp[Voice_File_List.SelectedIndex], 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_LOOP);
                Stream = BassFx.BASS_FX_TempoCreate(StreamHandle, BASSFlag.BASS_FX_FREESOURCE);
                Bass.BASS_ChannelSetDevice(Stream, Video_Mode.Sound_Device);
                Bass.BASS_ChannelPlay(Stream, false);
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)Volume_S.Value / 100);
            }
            else if (Voice_Sub_List.Visibility == Visibility.Visible)
            {
                List<string> Temp = Voice_Sub_List_Full_File_Name[Voice_Sub_List.SelectedIndex];
                if (!File.Exists(Temp[Voice_File_List.SelectedIndex]))
                {
                    Message_Feed_Out("音声ファイルが存在しません。削除された可能性があります。");
                    return;
                }
                Bass.BASS_ChannelStop(Stream);
                Bass.BASS_StreamFree(Stream);
                int StreamHandle = Bass.BASS_StreamCreateFile(Temp[Voice_File_List.SelectedIndex], 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_LOOP);
                Stream = BassFx.BASS_FX_TempoCreate(StreamHandle, BASSFlag.BASS_FX_FREESOURCE);
                Bass.BASS_ChannelSetDevice(Stream, Video_Mode.Sound_Device);
                Bass.BASS_ChannelPlay(Stream, false);
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)Volume_S.Value / 100);
            }
        }
        private void Voice_Pause_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
            //再生している音声を停止
            Bass.BASS_ChannelStop(Stream);
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
            //現在の状態をファイルに保存する
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Temp_Voice_Save.dat");
                    if (Project_Name_T.IsEnabled)
                    {
                        stw.WriteLine(Project_Name_T.Text);
                    }
                    else
                    {
                        stw.WriteLine(Project_Name_T.Text + "|IsNotChangeProjectNameMode=true");
                    }
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
                    Sub_Code.File_Encrypt(Voice_Set.Special_Path + "/Temp_Voice_Save.dat", sfd.FileName, "SRTTbacon_Create_Voice_Save", true);
                    Message_Feed_Out("セーブしました。");
                }
                catch (Exception e1)
                {
                    Message_Feed_Out("指定したファイルにアクセスできませんでした。");
                    Sub_Code.Error_Log_Write(e1.Message);
                }
            }
        }
        private void Voice_Load_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy || IsCreating)
            {
                return;
            }
            //保存したファイルから状態を復元
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
                    Sub_Code.File_Decrypt(ofd.FileName, Voice_Set.Special_Path + "/Temp_Load_Voice.dat", "SRTTbacon_Create_Voice_Save", false);
                    //音声を配置
                    string line;
                    StreamReader file = new StreamReader(Voice_Set.Special_Path + "/Temp_Load_Voice.dat");
                    string Name_All = file.ReadLine();
                    if (Name_All.Contains("|"))
                    {
                        string Mode_Name = Name_All.Substring(Name_All.LastIndexOf('|'));
                        if (Mode_Name == "|IsNotChangeProjectNameMode=true")
                        {
                            string Name_Only = Name_All.Substring(0, Name_All.LastIndexOf('|'));
                            Project_Name_T.Text = Name_Only;
                            Project_Name_Text.Text = "プロジェクト名(変更できません)";
                            Project_Name_T.IsEnabled = false;
                        }
                        else
                        {
                            Project_Name_T.Text = Name_All;
                            Project_Name_Text.Text = "プロジェクト名";
                            Project_Name_T.IsEnabled = true;
                        }
                    }
                    else
                    {
                        Project_Name_T.Text = Name_All;
                        Project_Name_Text.Text = "プロジェクト名";
                        Project_Name_T.IsEnabled = true;
                    }
                    List_Text_Reset();
                    Bass.BASS_ChannelStop(Stream);
                    Bass.BASS_StreamFree(Stream);
                    while ((line = file.ReadLine()) != null)
                    {
                        int Number = int.Parse(line.Substring(0, line.IndexOf('|')));
                        string File_Path = line.Substring(line.IndexOf('|') + 1);
                        if (Number < 34)
                        {
                            List<string> List_Number = Voice_List_Full_File_Name[Number];
                            List_Number.Add(File_Path);
                            Main_Voice_List[Number] = Main_Voice_List[Number].Replace("未選択", "選択済み");
                            Voice_List.Items[Number] = Main_Voice_List[Number];
                            Voice_List_Full_File_Name[Number] = List_Number;
                        }
                        else
                        {
                            List<string> List_Number = Voice_Sub_List_Full_File_Name[Number - 34];
                            List_Number.Add(File_Path);
                            Sub_Voice_List[Number - 34] = Sub_Voice_List[Number - 34].Replace("未選択", "選択済み");
                            Voice_Sub_List.Items[Number - 34] = Sub_Voice_List[Number - 34];
                            Voice_Sub_List_Full_File_Name[Number - 34] = List_Number;
                        }
                    }
                    file.Close();
                    File.Delete(Voice_Set.Special_Path + "/Temp_Load_Voice.dat");
                    if (Voice_List.Visibility == Visibility.Visible && Voice_List.SelectedIndex != -1)
                    {
                        Voice_File_Reset(Voice_List_Full_File_Name, Voice_List.SelectedIndex);
                    }
                    else if (Voice_Sub_List.Visibility == Visibility.Visible && Voice_Sub_List.SelectedIndex != -1)
                    {
                        Voice_File_Reset(Voice_Sub_List_Full_File_Name, Voice_Sub_List.SelectedIndex);
                    }
                    ColorMode_Change();
                    Message_Feed_Out("ロードしました。");
                }
                catch (Exception e1)
                {
                    Message_Feed_Out("指定したセーブデータが破損しています。");
                    Sub_Code.Error_Log_Write(e1.Message);
                }
            }
        }
        private async void Voice_Create_B_Click(object sender, RoutedEventArgs e)
        {
            try
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
                bool IsOK = false;
                foreach (string Name_Now in Main_Voice_List)
                {
                    if (Name_Now.Contains("選択済み"))
                    {
                        IsOK = true;
                    }
                }
                if (!IsOK)
                {
                    foreach (string Name_Now in Sub_Voice_List)
                    {
                        if (Name_Now.Contains("選択済み"))
                        {
                            IsOK = true;
                        }
                    }
                    if (!IsOK)
                    {
                        Message_Feed_Out("音声が1つも選択されていません。");
                        return;
                    }
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
                    Directory.CreateDirectory(Voice_Set.Special_Path + "/Temp/" + Project_Name_T.Text);
                    Directory.Delete(Voice_Set.Special_Path + "/Temp", true);
                    if (Project_Name_T.Text.Contains("/") || Project_Name_T.Text.Contains("\\"))
                    {
                        throw new Exception("プロジェクト名に'/'または'\'を付けることはできません。");
                    }
                }
                catch (Exception e1)
                {
                    Message_Feed_Out("プロジェクト名に不適切な文字が含まれています。");
                    Sub_Code.Error_Log_Write(e1.Message);
                    return;
                }
                if (Sub_Code.IsTextIncludeJapanese(Project_Name_T.Text) && !IsNewMode)
                {
                    Message_Feed_Out("プロジェクト名に日本語を含めることはできません。");
                    return;
                }
                /*if (Sub_Code.IsTextIncludeJapanese(Directory.GetCurrentDirectory()))
                {
                    Message_Feed_Out("パスに日本語が含まれています。");
                    return;
                }*/
                //作成画面へ
                IsCreating = true;
                Message_T.Opacity = 1;
                IsMessageShowing = false;
                string Dir_Path = Directory.GetCurrentDirectory();
                string Dir_Name = Dir_Path + "/Projects/" + Project_Name_T.Text;
                List<List<string>> Temp = new List<List<string>>();
                for (int Number_01 = 0; Number_01 < Voice_List_Full_File_Name.Count; Number_01++)
                {
                    Temp.Add(Voice_List_Full_File_Name[Number_01]);
                }
                for (int Number_02 = 0; Number_02 < Voice_Sub_List_Full_File_Name.Count; Number_02++)
                {
                    Temp.Add(Voice_Sub_List_Full_File_Name[Number_02]);
                }
                Voice_Create_Window.Window_Show_V2(Project_Name_T.Text, Temp, IsNewMode);
                Voice_Create_Window.Opacity = 0;
                Voice_Create_Window.Visibility = Visibility.Visible;
                while (Voice_Create_Window.Opacity < 1)
                {
                    Voice_Create_Window.Opacity += Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
                while (Voice_Create_Window.Visibility == Visibility.Visible)
                {
                    await Task.Delay(100);
                }
                //作成画面で作成ボタンが押されたら開始
                if (Sub_Code.CreatingProject)
                {
                    Sub_Code.CreatingProject = false;
                    Border_All.Visibility = Visibility.Visible;
                    if (Directory.Exists(Dir_Name))
                    {
                        try
                        {
                            Directory.Delete(Dir_Name + "/Voices", true);
                            Directory.Delete(Dir_Name + "/" + Project_Name_T.Text + "_Mod", true);
                        }
                        catch
                        {
                        }
                    }
                    Message_T.Opacity = 1;
                    Message_T.Text = "ファイルをコピーしています...";
                    await Task.Delay(30);
                    string Log_01 = Sub_Code.Set_Voice_Type_Change_Name_By_Index(Dir_Name + "/Voices", Temp);
                    if (Log_01 != "")
                    {
                        Message_Feed_Out("ファイルをコピーできませんでした。詳しくは\"Error_Log.txt\"を参照してください。");
                        Directory.Delete(Dir_Name, true);
                        IsCreating = false;
                        return;
                    }
                    if (Sub_Code.VolumeSet)
                    {
                        Message_T.Text = "音量をWoTB用に調整しています...";
                        await Task.Delay(50);
                        Sub_Code.Check_MP3_Rename(Dir_Name + "/Voices");
                        await Multithread.Convert_To_MP3(Directory.GetFiles(Dir_Name + "/Voices", "*.raw", SearchOption.TopDirectoryOnly), Dir_Name + "/Voices", true);
                        Sub_Code.MP3_Volume_Set(Dir_Name + "/Voices");
                        /*Message_T.Text = "音量を均一にしています...";
                        await Task.Delay(50);
                        await Sub_Code.Change_MP3_Encode(Dir_Name + "/Voices");*/
                        await Task.Delay(500);
                    }
                    string File_Name = Project_Name_T.Text.Replace(" ", "_");
                    if (IsNewMode)
                    {
                        //BNK_Create_V1(Dir_Name);
                        await BNK_Create_V2(Dir_Name);
                        if (Sub_Code.DVPL_Encode)
                        {
                            Message_T.Text = "DVPL化しています...";
                            await Task.Delay(50);
                            DVPL.DVPL_Pack(Dir_Name + "/voiceover_crew.bnk", Dir_Name + "/voiceover_crew.bnk.dvpl", true);
                            DVPL.DVPL_Pack(Dir_Name + "/ui_battle.bnk", Dir_Name + "/ui_battle.bnk.dvpl", true);
                            DVPL.DVPL_Pack(Dir_Name + "/ui_battle_basic.bnk", Dir_Name + "/ui_battle_basic.bnk.dvpl", true);
                            DVPL.DVPL_Pack(Dir_Name + "/ui_chat_quick_commands.bnk", Dir_Name + "/ui_chat_quick_commands.bnk.dvpl", true);
                            DVPL.DVPL_Pack(Dir_Name + "/reload.bnk", Dir_Name + "/reload.bnk.dvpl", true);
                        }
                    }
                    else if (Sub_Code.AndroidMode)
                    {
                        await Android_Create.Android_Project_Create(Message_T, Project_Name_T.Text, Dir_Name + "/Voices", Voice_Set.Special_Path + "/SE");
                        Message_T.Text = "DVPL化しています...";
                        await Task.Delay(50);
                        try
                        {
                            DVPL.DVPL_Pack(Dir_Name + "/ingame_voice_ja.fsb", Dir_Name + "/ingame_voice_ja.fsb.dvpl", true);
                            DVPL.DVPL_Pack(Dir_Name + "/GUI_battle_streamed.fsb", Dir_Name + "/GUI_battle_streamed.fsb.dvpl", true);
                            DVPL.DVPL_Pack(Dir_Name + "/GUI_notifications_FX_howitzer_load.fsb", Dir_Name + "/GUI_notifications_FX_howitzer_load.fsb.dvpl", true);
                            DVPL.DVPL_Pack(Dir_Name + "/GUI_quick_commands.fsb", Dir_Name + "/GUI_quick_commands.fsb.dvpl", true);
                            DVPL.DVPL_Pack(Dir_Name + "/GUI_sirene.fsb", Dir_Name + "/GUI_sirene.fsb.dvpl", true);
                        }
                        catch (Exception e1)
                        {
                            Message_Feed_Out("DVPL化できませんでした。");
                            Sub_Code.Error_Log_Write(e1.Message);
                            IsCreating = false;
                            return;
                        }
                        if (!File.Exists(Dir_Name + "/ingame_voice_ja.fsb.dvpl"))
                        {
                            Message_Feed_Out("正常に作成できませんでした。もう一度お試しください。");
                            Sub_Code.Error_Log_Write("ingame_voice_ja.fsb.dvplが作成されませんでした。");
                            IsCreating = false;
                            return;
                        }
                    }
                    else
                    {
                        //fdpプロジェクトを作成
                        Voice_Mod_Create.Project_Create(ref Message_T, Project_Name_T.Text, Dir_Name + "/Voices", Voice_Set.Special_Path + "/SE");
                        //fdpプロジェクトをビルド
                        await Sub_Code.Project_Build(Dir_Name + "/" + File_Name + ".fdp", Message_T);
                        DateTime dt = DateTime.Now;
                        string Time = Sub_Code.Get_Time_Now(dt, ".", 1, 6);
                        //配布用のフォルダを作成
                        Directory.CreateDirectory(Dir_Name + "/" + Project_Name_T.Text + "_Mod/Mods");
                        Directory.CreateDirectory(Dir_Name + "/" + Project_Name_T.Text + "_Mod/Configs/Sfx");
                        Directory.CreateDirectory(Dir_Path + "/Backup/" + Time);
                        try
                        {
                            //ビルドされたファイルをコピー
                            File.Copy(Dir_Name + "/" + File_Name + ".fev", Dir_Name + "/" + File_Name + "_Mod/Mods/" + File_Name + ".fev", true);
                            File.Copy(Dir_Name + "/" + File_Name + ".fsb", Dir_Name + "/" + File_Name + "_Mod/Mods/" + File_Name + ".fsb", true);
                            File.Delete(Dir_Name + "/" + File_Name + ".fev");
                            File.Delete(Dir_Name + "/" + File_Name + ".fsb");
                            File.Delete(Dir_Name + "/fmod_designer.log");
                            File.Delete(Dir_Name + "/undo-log.txt");
                        }
                        catch (Exception e1)
                        {
                            Sub_Code.Error_Log_Write(e1.Message);
                        }
                        //WoTBのフォルダから各ファイルをコピー
                        Sub_Code.DVPL_File_Copy(Voice_Set.WoTB_Path + "/Data/sounds.yaml", Dir_Path + "/Backup/" + Time + "/sounds.yaml", false);
                        Sub_Code.DVPL_File_Copy(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_high.yaml", Dir_Path + "/Backup/" + Time + "/sfx_high.yaml", false);
                        Sub_Code.DVPL_File_Copy(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_low.yaml", Dir_Path + "/Backup/" + Time + "/sfx_low.yaml", false);
                        Sub_Code.Backup_Update(Time);
                        if (File.Exists(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_high.yaml.dvpl"))
                        {
                            DVPL.DVPL_UnPack(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_high.yaml.dvpl", Dir_Name + "/" + Project_Name_T.Text + "_Mod/Configs/Sfx/sfx_high.yaml", false);
                        }
                        else if (File.Exists(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_high.yaml"))
                        {
                            File.Copy(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_high.yaml", Dir_Name + "/" + Project_Name_T.Text + "_Mod/Configs/Sfx/sfx_high.yaml");
                        }
                        if (File.Exists(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_low.yaml.dvpl"))
                        {
                            DVPL.DVPL_UnPack(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_low.yaml.dvpl", Dir_Name + "/" + Project_Name_T.Text + "_Mod/Configs/Sfx/sfx_low.yaml", false);
                        }
                        else if (File.Exists(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_low.yaml"))
                        {
                            File.Copy(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_low.yaml", Dir_Name + "/" + Project_Name_T.Text + "_Mod/Configs/Sfx/sfx_low.yaml");
                        }
                        string[] Configs = { "sfx_high.yaml", "sfx_low.yaml" };
                        //使用するfevファイルを追加
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
                        File.Copy(Voice_Set.Special_Path + "/Temp_Sounds.yaml", Dir_Name + "/" + Project_Name_T.Text + "_Mod/sounds.yaml", true);
                        File.Delete(Voice_Set.Special_Path + "/Temp_Sounds.yaml");
                        if (Sub_Code.DVPL_Encode)
                        {
                            Message_T.Text = "DVPL化しています...";
                            await Task.Delay(10);
                            try
                            {
                                //DVPL化にチェックが入っている場合使用するファイルすべてdvpl化する
                                DVPL.DVPL_Pack(Dir_Name + "/" + Project_Name_T.Text + "_Mod/Mods/" + File_Name + ".fev", Dir_Name + "/" + Project_Name_T.Text + "_Mod/Mods/" + File_Name + ".fev.dvpl", true);
                                DVPL.DVPL_Pack(Dir_Name + "/" + Project_Name_T.Text + "_Mod/Mods/" + File_Name + ".fsb", Dir_Name + "/" + Project_Name_T.Text + "_Mod/Mods/" + File_Name + ".fsb.dvpl", true);
                                DVPL.DVPL_Pack(Dir_Name + "/" + Project_Name_T.Text + "_Mod/Configs/Sfx/sfx_high.yaml", Dir_Name + "/" + Project_Name_T.Text + "_Mod/Configs/Sfx/sfx_high.yaml.dvpl", true);
                                DVPL.DVPL_Pack(Dir_Name + "/" + Project_Name_T.Text + "_Mod/Configs/Sfx/sfx_low.yaml", Dir_Name + "/" + Project_Name_T.Text + "_Mod/Configs/Sfx/sfx_low.yaml.dvpl", true);
                                DVPL.DVPL_Pack(Dir_Name + "/" + Project_Name_T.Text + "_Mod/sounds.yaml", Dir_Name + "/" + Project_Name_T.Text + "_Mod/sounds.yaml.dvpl", true);
                            }
                            catch (Exception e1)
                            {
                                Message_Feed_Out("エラー:DVPL化できませんでした。");
                                Sub_Code.Error_Log_Write(e1.Message);
                                IsCreating = false;
                                return;
                            }
                        }
                    }
                    if (Directory.Exists(Dir_Name + "/.fsbcache"))
                    {
                        try
                        {
                            Directory.Delete(Dir_Name + "/.fsbcache", true);
                        }
                        catch (Exception e1)
                        {
                            Sub_Code.Error_Log_Write(e1.Message);
                        }
                    }
                    try
                    {
                        Message_T.Text = "一時フォルダを削除しています...";
                        await Task.Delay(50);
                        Directory.Delete(Dir_Name + "/Voices", true);
                    }
                    catch (Exception e1)
                    {
                        Sub_Code.Error_Log_Write(e1.Message);
                    }
                    Message_T.Text = "ダイアログを表示しています...";
                    await Task.Delay(50);
                    MessageBoxResult result = System.Windows.MessageBox.Show("完了しました。WoTBに適応しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes);
                    if (result == MessageBoxResult.Yes)
                    {
                        if (Voice_Set.WoTB_Path == "")
                        {
                            Message_Feed_Out("WoTBのインストール場所を取得できませんでした。");
                            IsCreating = false;
                            return;
                        }
                        try
                        {
                            //WoTBのフォルダに作成したファイルをコピー
                            if (IsNewMode)
                            {
                                string GetDir = Dir_Name + "/" + Project_Name_T.Text + "_Mod/Data/WwiseSound";
                                Sub_Code.DVPL_File_Delete(Voice_Set.WoTB_Path + "/Data/WwiseSound/" + Sub_Code.SetLanguage + "/voiceover_crew.bnk");
                                Sub_Code.DVPL_File_Delete(Voice_Set.WoTB_Path + "/Data/WwiseSound/reload.bnk");
                                Sub_Code.DVPL_File_Delete(Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_chat_quick_commands.bnk");
                                Sub_Code.DVPL_File_Delete(Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_battle.bnk");
                                /*Sub_Code.DVPL_File_Copy(GetDir + "/" + Sub_Code.SetLanguage + "/voiceover_crew.bnk", Voice_Set.WoTB_Path + "/Data/WwiseSound/" + Sub_Code.SetLanguage + "/voiceover_crew.bnk", true);
                                Sub_Code.DVPL_File_Copy(GetDir + "/reload.bnk", Voice_Set.WoTB_Path + "/Data/WwiseSound/reload.bnk", true);
                                Sub_Code.DVPL_File_Copy(GetDir + "/ui_chat_quick_commands.bnk", Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_chat_quick_commands.bnk", true);
                                Sub_Code.DVPL_File_Copy(GetDir + "/ui_battle.bnk", Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_battle.bnk", true);*/
                                Sub_Code.DVPL_File_Copy(Dir_Name + "/voiceover_crew.bnk", Voice_Set.WoTB_Path + "/Data/WwiseSound/" + Sub_Code.SetLanguage + "/voiceover_crew.bnk", true);
                                Sub_Code.DVPL_File_Copy(Dir_Name + "/reload.bnk", Voice_Set.WoTB_Path + "/Data/WwiseSound/reload.bnk", true);
                                Sub_Code.DVPL_File_Copy(Dir_Name + "/ui_chat_quick_commands.bnk", Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_chat_quick_commands.bnk", true);
                                Sub_Code.DVPL_File_Copy(Dir_Name + "/ui_battle.bnk", Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_battle.bnk", true);
                            }
                            else if (Sub_Code.AndroidMode)
                            {
                                File.Copy(Dir_Name + "/ingame_voice_ja.fsb.dvpl", Voice_Set.WoTB_Path + "/Data/Sfx/ingame_voice_ja.fsb.dvpl", true);
                                File.Copy(Dir_Name + "/GUI_battle_streamed.fsb.dvpl", Voice_Set.WoTB_Path + "/Data/Sfx/GUI_battle_streamed.fsb.dvpl", true);
                                File.Copy(Dir_Name + "/GUI_notifications_FX_howitzer_load.fsb.dvpl", Voice_Set.WoTB_Path + "/Data/Sfx/GUI_notifications_FX_howitzer_load.fsb.dvpl", true);
                                File.Copy(Dir_Name + "/GUI_quick_commands.fsb.dvpl", Voice_Set.WoTB_Path + "/Data/Sfx/GUI_quick_commands.fsb.dvpl", true);
                                File.Copy(Dir_Name + "/GUI_sirene.fsb.dvpl", Voice_Set.WoTB_Path + "/Data/Sfx/GUI_sirene.fsb.dvpl", true);
                            }
                            else
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
                        }
                        catch (Exception e1)
                        {
                            Message_Feed_Out("WoTBに適応できませんでした。");
                            Sub_Code.Error_Log_Write(e1.Message);
                            IsCreating = false;
                            return;
                        }
                    }
                    Sub_Code.DVPL_Encode = false;
                    Sub_Code.SetLanguage = "";
                    Message_Feed_Out("完了しました。\nファイル容量が極端に少ない場合、失敗している可能性があります。");
                }
            }
            catch (Exception e1)
            {
                Message_Feed_Out("致命的なエラーが発生し正常に作成されませんでした。");
                Sub_Code.Error_Log_Write(e1.Message);
            }
            Border_All.Visibility = Visibility.Hidden;
            IsCreating = false;
        }
        void Volume_MouseUp(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Configs_Save();
        }
        void Configs_Save()
        {
            try
            {
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Configs/Voice_Create.tmp");
                stw.WriteLine(Volume_S.Value);
                stw.WriteLine(ColorMode_C.IsChecked.Value);
                stw.Write(BGM_Reload_C.IsChecked.Value);
                stw.Close();
                using (var eifs = new FileStream(Voice_Set.Special_Path + "/Configs/Voice_Create.tmp", FileMode.Open, FileAccess.Read))
                {
                    using (var eofs = new FileStream(Voice_Set.Special_Path + "/Configs/Voice_Create.conf", FileMode.Create, FileAccess.Write))
                    {
                        FileEncode.FileEncryptor.Encrypt(eifs, eofs, "Voice_Create_Configs_Save");
                    }
                }
                File.Delete(Voice_Set.Special_Path + "/Configs/Voice_Create.tmp");
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        void Wwise_Bnk_Pck_Replace(string From_File, string From_Dir, string Language_OR_Mode, bool IsVoiceMod)
        {
            if (!File.Exists(From_File) || !Directory.Exists(From_Dir))
            {
                return;
            }
            List<string> WoTB_IDs;
            if (IsVoiceMod)
            {
                WoTB_IDs = Sub_Code.Get_Voices_ID(Language_OR_Mode);
            }
            else
            {
                WoTB_IDs = Sub_Code.Get_SE_ID(Language_OR_Mode);
            }
            string[] Files = Directory.GetFiles(From_Dir, "*.wav", SearchOption.TopDirectoryOnly);
            string Ex = Path.GetExtension(From_File);
            if (Ex == ".pck")
            {
                Wwise_Class.Wwise_File_Extract_V1 Wwise_Pck = new Wwise_Class.Wwise_File_Extract_V1(From_File);
                List<string> File_IDs = Wwise_Pck.Wwise_Get_Banks_ID();
                Wwise_Pck.Pck_Clear();
                for (int Number = 0; Number < Files.Length; Number++)
                {
                    string File_ID_Now = "";
                    for (int Number_01 = 0; Number_01 < WoTB_IDs.Count; Number_01++)
                    {
                        string Name = WoTB_IDs[Number_01].Substring(0, WoTB_IDs[Number_01].IndexOf('|'));
                        if (Name == Path.GetFileName(Files[Number]))
                        {
                            File_ID_Now = WoTB_IDs[Number_01].Substring(WoTB_IDs[Number_01].IndexOf('|') + 1);
                            break;
                        }
                    }
                    if (File_ID_Now != "")
                    {
                        for (int Number_01 = 0; Number_01 < File_IDs.Count; Number_01++)
                        {
                            if (File_IDs[Number_01] == File_ID_Now)
                            {
                                Sub_Code.File_Move(Files[Number], Path.GetDirectoryName(Files[Number]) + "/" + (Number_01 + 1) + ".wav", true);
                                break;
                            }
                        }
                    }
                }
            }
            else if (Ex == ".bnk")
            {
                Wwise_Class.Wwise_File_Extract_V2 Wwise_Bnk = new Wwise_Class.Wwise_File_Extract_V2(From_File);
                List<string> File_IDs = Wwise_Bnk.Wwise_Get_Names();
                Wwise_Bnk.Bank_Clear();
                for (int Number = 0; Number < Files.Length; Number++)
                {
                    string File_ID_Now = "";
                    for (int Number_01 = 0; Number_01 < WoTB_IDs.Count; Number_01++)
                    {
                        string Name = WoTB_IDs[Number_01].Substring(0, WoTB_IDs[Number_01].IndexOf('|'));
                        if (Name == Path.GetFileName(Files[Number]))
                        {
                            File_ID_Now = WoTB_IDs[Number_01].Substring(WoTB_IDs[Number_01].IndexOf('|') + 1);
                            break;
                        }
                    }
                    if (File_ID_Now != "")
                    {
                        for (int Number_01 = 0; Number_01 < File_IDs.Count; Number_01++)
                        {
                            if (File_IDs[Number_01] == File_ID_Now)
                            {
                                Sub_Code.File_Move(Files[Number], Path.GetDirectoryName(Files[Number]) + "/" + Number_01 + ".wav", true);
                                break;
                            }
                        }
                    }
                }
            }
        }
        async Task Encode_WEM_And_Create_Bnk_Pck(string From_Bnk_Pck_File, string From_Dir)
        {
            await Encode_WEM_And_Create_Bnk_Pck(From_Bnk_Pck_File, From_Bnk_Pck_File, From_Dir);
        }
        async Task Encode_WEM_And_Create_Bnk_Pck(string From_Bnk_Pck_File, string To_Bnk_Pck_File, string From_Dir)
        {
            if (!File.Exists(From_Bnk_Pck_File) || !Directory.Exists(From_Dir))
            {
                return;
            }
            try
            {
                Wwise_Class.Wwise_File_Extract_V2 Wwise_Bnk = null;
                bool IsPck;
                if (Path.GetExtension(From_Bnk_Pck_File) == ".pck")
                {
                    IsPck = true;
                }
                else
                {
                    IsPck = false;
                    Wwise_Bnk = new Wwise_Class.Wwise_File_Extract_V2(From_Bnk_Pck_File);
                }
                string[] WAV_Files = Directory.GetFiles(From_Dir, "*.wav", SearchOption.TopDirectoryOnly);
                int Number = 1;
                foreach (string File_Now in WAV_Files)
                {
                    Message_T.Text = Number + "個目のサウンドファイルをエンコードしています...";
                    await Task.Delay(10);
                    string Dir_Name_Voice = Path.GetDirectoryName(File_Now) + "/" + Path.GetFileNameWithoutExtension(File_Now);
                    int Index = int.Parse(Path.GetFileNameWithoutExtension(File_Now));
                    Sub_Code.File_To_WEM(File_Now, Dir_Name_Voice + ".wem", true, true);
                    if (!IsPck)
                    {
                        Wwise_Bnk.Bank_Edit_Sound(Index, Dir_Name_Voice + ".wem", false);
                    }
                    Number++;
                }
                Message_T.Text = "WoTBのModファイルを作成しています...";
                await Task.Delay(25);
                if (IsPck)
                {
                    Wwise_Class.Wwise_File_Extract_V1 Wwise_Pck = new Wwise_Class.Wwise_File_Extract_V1(From_Bnk_Pck_File);
                    if (From_Bnk_Pck_File == To_Bnk_Pck_File)
                    {
                        Wwise_Pck.Wwise_PCK_Save(From_Dir);
                    }
                    else
                    {
                        Wwise_Pck.Wwise_PCK_Save(To_Bnk_Pck_File, From_Dir, true);
                    }
                }
                else
                {
                    if (From_Bnk_Pck_File == To_Bnk_Pck_File)
                    {
                        Wwise_Bnk.Bank_Save();
                    }
                    else
                    {
                        Wwise_Bnk.Bank_Save(To_Bnk_Pck_File);
                    }
                }
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        //.bnkファイルを作成(既に存在する.bnkの中身のみを置き換えるのでファイル数は変更しない)
        async Task BNK_Create_V1(string Dir_Name)
        {
            try
            {
                if (File.Exists(Voice_Set.WoTB_Path + "/Data/WwiseSound/" + Sub_Code.SetLanguage + "/voiceover_crew.bnk.dvpl"))
                {
                    DVPL.DVPL_UnPack(Voice_Set.WoTB_Path + "/Data/WwiseSound/" + Sub_Code.SetLanguage + "/voiceover_crew.bnk.dvpl", Voice_Set.Special_Path + "/Wwise/voiceover_crew.bnk", false);
                }
                else
                {
                    File.Copy(Voice_Set.WoTB_Path + "/Data/WwiseSound/" + Sub_Code.SetLanguage + "/voiceover_crew.bnk", Voice_Set.Special_Path + "/Wwise/voiceover_crew.bnk", true);
                }
                if (File.Exists(Voice_Set.WoTB_Path + "/Data/WwiseSound/reload.bnk.dvpl"))
                {
                    DVPL.DVPL_UnPack(Voice_Set.WoTB_Path + "/Data/WwiseSound/reload.bnk.dvpl", Voice_Set.Special_Path + "/Wwise/reload.bnk", false);
                }
                else
                {
                    File.Copy(Voice_Set.WoTB_Path + "/Data/WwiseSound/reload.bnk", Voice_Set.Special_Path + "/Wwise/reload.bnk", true);
                }
                if (File.Exists(Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_chat_quick_commands.bnk.dvpl"))
                {
                    DVPL.DVPL_UnPack(Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_chat_quick_commands.bnk.dvpl", Voice_Set.Special_Path + "/Wwise/ui_chat_quick_commands.bnk", false);
                }
                else
                {
                    File.Copy(Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_chat_quick_commands.bnk", Voice_Set.Special_Path + "/Wwise/ui_chat_quick_commands.bnk", true);
                }
                if (File.Exists(Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_battle.bnk.dvpl"))
                {
                    DVPL.DVPL_UnPack(Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_battle.bnk.dvpl", Voice_Set.Special_Path + "/Wwise/ui_battle.bnk", false);
                }
                else
                {
                    File.Copy(Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_battle.bnk", Voice_Set.Special_Path + "/Wwise/ui_battle.bnk", true);
                }
                if (File.Exists(Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_battle_basic.bnk.dvpl"))
                {
                    DVPL.DVPL_UnPack(Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_battle_basic.bnk.dvpl", Voice_Set.Special_Path + "/Wwise/ui_battle_basic.bnk", false);
                }
                else
                {
                    File.Copy(Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_battle_basic.bnk", Voice_Set.Special_Path + "/Wwise/ui_battle_basic.bnk", true);
                }
            }
            catch
            {
                throw new Exception("'" + Voice_Set.Special_Path + "\\Wwise\\voiceover_crew.bnk'にアクセスできないか、WoTB内にファイルが存在しません。");
            }
            if (!Directory.Exists(Dir_Name + "/Voices"))
            {
                return;
            }
            Message_T.Text = "音声のファイル名を変換しています...";
            await Task.Delay(50);
            Android_Create.Voice_Name_To_Ingame_Voice(Dir_Name + "/Voices");
            Message_T.Text = "音声のファイル数を修正しています...";
            await Task.Delay(50);
            Android_Create.Ingame_Voice_Set_Number(Voice_Set.Special_Path + "/Wwise", Dir_Name + "/Voices");
            Message_T.Text = "音声ファイルにSEを付けています...";
            await Task.Delay(50);
            await Android_Create.Ingame_Voice_In_SE_By_Dir(Dir_Name + "/Voices", Voice_Set.Special_Path + "/SE", Voice_Set.Special_Path + "/Wwise/Voices");
            Directory.Delete(Dir_Name + "/Voices", true);
            Android_Create.Ingame_Voice_Move_Directory(Voice_Set.Special_Path + "/Wwise/Voices", false);
            Message_T.Text = "ファイルをコピーしています...";
            await Task.Delay(50);
            Sub_Code.Directory_Copy(Voice_Set.Special_Path + "/Wwise/Voices", Dir_Name + "/Voices");
            Directory.Delete(Voice_Set.Special_Path + "/Wwise/Voices", true);
            Message_T.Text = "音声ファイルをwavに変換しています...";
            await Task.Delay(50);
            string[] Voice_Files = Directory.GetFiles(Dir_Name + "/Voices", "*", SearchOption.AllDirectories);
            foreach (string Voice_Now in Voice_Files)
            {
                if (!Sub_Code.Audio_IsWAV(Voice_Now))
                {
                    string Dir_Name_Voice = Path.GetDirectoryName(Voice_Now) + "/" + Path.GetFileNameWithoutExtension(Voice_Now);
                    Sub_Code.File_Move(Voice_Now, Dir_Name_Voice + ".tmp", true);
                    Sub_Code.Audio_Encode_To_Other(Dir_Name_Voice + ".tmp", Dir_Name_Voice + ".wav", "wav", true);
                }
            }
            Message_T.Text = "ファイル名を変更しています...";
            await Task.Delay(50);
            Wwise_Bnk_Pck_Replace(Voice_Set.Special_Path + "/Wwise/voiceover_crew.bnk", Dir_Name + "/Voices", "ja", true);
            Wwise_Bnk_Pck_Replace(Voice_Set.Special_Path + "/Wwise/reload.bnk", Dir_Name + "/Voices/GUI_notifications_FX_howitzer_load", "reload", false);
            Wwise_Bnk_Pck_Replace(Voice_Set.Special_Path + "/Wwise/ui_chat_quick_commands.bnk", Dir_Name + "/Voices/GUI_quick_commands", "command", false);
            Wwise_Bnk_Pck_Replace(Voice_Set.Special_Path + "/Wwise/ui_battle.bnk", Dir_Name + "/Voices/GUI_battle_streamed", "battle_streamed", false);
            if (Directory.Exists(Dir_Name + "/" + Project_Name_T.Text + "_Mod"))
            {
                Directory.Delete(Dir_Name + "/" + Project_Name_T.Text + "_Mod", true);
            }
            string GetDir = Dir_Name + "/" + Project_Name_T.Text + "_Mod/Data/WwiseSound";
            Directory.CreateDirectory(GetDir + "/" + Sub_Code.SetLanguage);
            await Encode_WEM_And_Create_Bnk_Pck(Voice_Set.Special_Path + "/Wwise/voiceover_crew.bnk", GetDir + "/" + Sub_Code.SetLanguage + "/voiceover_crew.bnk", Dir_Name + "/Voices");
            await Encode_WEM_And_Create_Bnk_Pck(Voice_Set.Special_Path + "/Wwise/reload.bnk", GetDir + "/reload.bnk", Dir_Name + "/Voices/GUI_notifications_FX_howitzer_load");
            await Encode_WEM_And_Create_Bnk_Pck(Voice_Set.Special_Path + "/Wwise/ui_chat_quick_commands.bnk", GetDir + "/ui_chat_quick_commands.bnk", Dir_Name + "/Voices/GUI_quick_commands");
            await Encode_WEM_And_Create_Bnk_Pck(Voice_Set.Special_Path + "/Wwise/ui_battle.bnk", GetDir + "/ui_battle.bnk", Dir_Name + "/Voices/GUI_battle_streamed");
            if (Sub_Code.DVPL_Encode)
            {
                Message_T.Text = "DVPL化しています...";
                await Task.Delay(50);
                try
                {
                    DVPL.DVPL_Pack(GetDir + "/" + Sub_Code.SetLanguage + "/voiceover_crew.bnk", GetDir + "/" + Sub_Code.SetLanguage + "/voiceover_crew.bnk.dvpl", true);
                    DVPL.DVPL_Pack(GetDir + "/reload.bnk", GetDir + "/reload.bnk.dvpl", true);
                    DVPL.DVPL_Pack(GetDir + "/ui_chat_quick_commands.bnk", GetDir + "/ui_chat_quick_commands.bnk.dvpl", true);
                    DVPL.DVPL_Pack(GetDir + "/ui_battle.bnk", GetDir + "/ui_battle.bnk.dvpl", true);
                }
                catch (Exception e1)
                {
                    Message_Feed_Out("DVPL化できませんでした。");
                    Sub_Code.Error_Log_Write(e1.Message);
                    return;
                }
            }
        }
        //Wwiseのプロジェクトファイルを用いて.bnkファイルを作成(ファイル数やイベントの内容も変更できます)
        async Task BNK_Create_V2(string Dir_Name)
        {
            if (!Directory.Exists(Dir_Name + "/Voices"))
            {
                return;
            }
            Message_T.Text = "音声ファイルをwavに変換しています...";
            await Task.Delay(50);
            string[] Voice_Files = Directory.GetFiles(Dir_Name + "/Voices", "*", SearchOption.AllDirectories);
            await Multithread.Convert_To_Wav(Dir_Name + "/Voices", true, false, true);
            Message_T.Text = "プロジェクトファイルを作成しています...";
            await Task.Delay(50);
            FileInfo fi = new FileInfo(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.wwu");
            if (File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp") && fi.Length >= 1000000)
            {
                File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.wwu", true);
            }
            if (!File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp"))
            {
                File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.wwu", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp", true);
            }
            Message_T.Text = "SEの有無をチェックし、適応しています...";
            await Task.Delay(50);
            Voice_Set.Set_SE_Change_Name();
            Wwise_Class.Wwise_Project_Create Wwise = new Wwise_Class.Wwise_Project_Create(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod");
            Wwise.Sound_Add_Wwise(Dir_Name + "/Voices", false, true);
            Wwise.Save();
            if (File.Exists(Dir_Name + "/Voices/battle_bgm_01.wav"))
            {
                Message_T.Text = ".bnkファイルを作成しています...\nBGMファイルが含まれているため時間がかかります。";
            }
            else
            {
                Message_T.Text = ".bnkファイルを作成しています...";
            }
            await Task.Delay(75);
            Wwise.Project_Build("ui_battle", Dir_Name + "/ui_battle.bnk");
            await Task.Delay(500);
            Wwise.Project_Build("ui_battle_basic", Dir_Name + "/ui_battle_basic.bnk");
            await Task.Delay(500);
            Wwise.Project_Build("ui_chat_quick_commands", Dir_Name + "/ui_chat_quick_commands.bnk");
            await Task.Delay(500);
            if (BGM_Reload_C.IsChecked.Value)
            {
                Wwise.Project_Build("voiceover_crew", Dir_Name + "/voiceover_crew.bnk");
                await Task.Delay(500);
                Wwise.Sound_Music_Add_Wwise(Dir_Name + "/Voices");
                Wwise.Save();
                Wwise.Project_Build("reload", Dir_Name + "/reload.bnk");
            }
            else
            {
                Wwise.Project_Build("reload", Dir_Name + "/reload.bnk");
                await Task.Delay(500);
                Wwise.Sound_Music_Add_Wwise(Dir_Name + "/Voices");
                Wwise.Save();
                Wwise.Project_Build("voiceover_crew", Dir_Name + "/voiceover_crew.bnk");
            }
            await Task.Delay(500);
            Wwise.Clear();
            if (File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp"))
            {
                File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.wwu", true);
            }
        }
        private void Voice_Clear_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
            MessageBoxResult result = System.Windows.MessageBox.Show("追加された音声をクリアしますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                List_Text_Reset();
                Project_Name_T.Text = "";
                Bass.BASS_ChannelStop(Stream);
                Bass.BASS_StreamFree(Stream);
                Voice_Back_B.Visibility = Visibility.Hidden;
                Voice_Sub_List.Visibility = Visibility.Hidden;
                Voice_Next_B.Visibility = Visibility.Visible;
                Voice_List.Visibility = Visibility.Visible;
                Project_Name_Text.Text = "プロジェクト名";
                Project_Name_T.IsEnabled = true;
                Voice_List_T.Text = "音声リスト1";
                if (Voice_List.SelectedIndex != -1)
                {
                    Voice_File_Reset(Voice_List_Full_File_Name, Voice_List.SelectedIndex);
                }
                else
                {
                    Voice_File_List.Items.Clear();
                }
                Message_Feed_Out("内容をクリアしました。");
            }
        }
        private void ColorMode_C_Click(object sender, RoutedEventArgs e)
        {
            ColorMode_Change();
            Configs_Save();
        }
        void ColorMode_Change()
        {
            Brush br;
            if (ColorMode_C.IsChecked.Value)
            {
                br = (Brush)new BrushConverter().ConvertFromString("#BFFF2C8C");
            }
            else
            {
                br = Brushes.Aqua;
            }
            int Select_Index = -1;
            if (Voice_List.Visibility == Visibility.Visible && Voice_List.SelectedIndex != -1)
            {
                Select_Index = Voice_List.SelectedIndex;
            }
            else if (Voice_Sub_List.Visibility == Visibility.Visible && Voice_Sub_List.SelectedIndex != -1)
            {
                Select_Index = Voice_Sub_List.SelectedIndex;
            }
            for (int Number = 0; Number < Voice_List.Items.Count; Number++)
            {
                if (Main_Voice_List[Number].Contains("未選択"))
                {
                    ListBoxItem LBI = new ListBoxItem();
                    LBI.Content = Main_Voice_List[Number];
                    LBI.Foreground = br;
                    Voice_List.Items[Number] = LBI;
                }
            }
            for (int Number = 0; Number < Voice_Sub_List.Items.Count; Number++)
            {
                if (Sub_Voice_List[Number].Contains("未選択"))
                {
                    ListBoxItem LBI = new ListBoxItem();
                    LBI.Content = Sub_Voice_List[Number];
                    LBI.Foreground = br;
                    Voice_Sub_List.Items[Number] = LBI;
                }
            }
            if (Voice_List.Visibility == Visibility.Visible && Select_Index != -1)
            {
                Voice_List.SelectedIndex = Select_Index;
            }
            else if (Voice_Sub_List.Visibility == Visibility && Select_Index != -1)
            {
                Voice_Sub_List.SelectedIndex = Select_Index;
            }
        }
        private void BGM_Reload_C_Click(object sender, RoutedEventArgs e)
        {
            Configs_Save();
        }
    }
}