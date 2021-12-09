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
        string Max_Time = "00:00";
        int Stream;
        int List_Index_Mode = 0;
        int Selected_Voice_Index = -1;
        bool IsBusy = false;
        bool IsMessageShowing = false;
        bool IsCreating = false;
        bool IsExecuteWoTB = false;
        bool IsPaused = false;
        bool IsPlayingMouseDown = false;
        bool IsLocationChanging = false;
        bool IsEnded = false;
        SYNCPROC IsMusicEnd;
        List<string> Main_Voice_List = new List<string>();
        List<string> Sub_Voice_List = new List<string>();
        List<string> Three_Voice_List = new List<string>();
        List<List<string>> Voice_List_Full_File_Name = new List<List<string>>();
        List<List<string>> Voice_Sub_List_Full_File_Name = new List<List<string>>();
        List<List<string>> Voice_Three_List_Full_File_Name = new List<List<string>>();
        public Voice_Create()
        {
            InitializeComponent();
            Voice_Sub_List.Visibility = Visibility.Hidden;
            Voice_Three_List.Visibility = Visibility.Hidden;
            Voice_Back_B.Visibility = Visibility.Hidden;
            Volume_S.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Volume_MouseUp), true);
            Volume_S.Value = 50;
            Position_S.AddHandler(MouseDownEvent, new MouseButtonEventHandler(Position_MouseDown), true);
            Position_S.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Position_MouseUp), true);
            if (File.Exists(Voice_Set.Special_Path + "/Configs/Voice_Create.conf"))
            {
                try
                {
                    StreamReader str = Sub_Code.File_Decrypt_To_Stream(Voice_Set.Special_Path + "/Configs/Voice_Create.conf", "Voice_Create_Configs_Save");
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
                }
                catch
                {
                }
            }
            List_Text_Reset();
            Position_Change();
        }
        async void Position_Change()
        {
            double nextFrame = (double)Environment.TickCount;
            float period = 1000f / 30f;
            while (true)
            {
                double tickCount = (double)Environment.TickCount;
                if (tickCount < nextFrame)
                {
                    if (nextFrame - tickCount > 1)
                        await Task.Delay((int)(nextFrame - tickCount));
                    System.Windows.Forms.Application.DoEvents();
                    continue;
                }
                if (Visibility == Visibility.Visible)
                {
                    if (Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_PLAYING && !IsLocationChanging)
                    {
                        long position = Bass.BASS_ChannelGetPosition(Stream);
                        Position_S.Value = Bass.BASS_ChannelBytes2Seconds(Stream, position);
                        TimeSpan Time = TimeSpan.FromSeconds(Position_S.Value);
                        string Minutes = Time.Minutes.ToString();
                        string Seconds = Time.Seconds.ToString();
                        if (Time.Minutes < 10)
                            Minutes = "0" + Time.Minutes;
                        if (Time.Seconds < 10)
                            Seconds = "0" + Time.Seconds;
                        Position_T.Text = Minutes + ":" + Seconds + " / " + Max_Time;
                    }
                }
                if (IsEnded)
                {
                    Bass.BASS_ChannelStop(Stream);
                    Position_S.Value = 0;
                    Position_T.Text = "00:00 / " + Max_Time;
                    Selected_Voice_Index = -1;
                    IsEnded = false;
                }
                if ((double)System.Environment.TickCount >= nextFrame + (double)period)
                {
                    nextFrame += period;
                    continue;
                }
                nextFrame += period;
            }
        }
        async void EndSync(int handle, int channel, int data, IntPtr user)
        {
            if (!IsEnded)
            {
                await Task.Delay(500);
                IsEnded = true;
            }
        }
        void List_Text_Reset()
        {
            //リストの状態を初期化
            Voice_List.Items.Clear();
            Voice_Sub_List.Items.Clear();
            Voice_Three_List.Items.Clear();
            Voice_File_List.Items.Clear();
            Main_Voice_List.Clear();
            Sub_Voice_List.Clear();
            Three_Voice_List.Clear();
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
            Three_Voice_List.Add("チャット:味方-送信 | 未選択");
            Three_Voice_List.Add("チャット:味方-受信 | 未選択");
            Three_Voice_List.Add("チャット:全体-送信 | 未選択");
            Three_Voice_List.Add("チャット:全体-受信 | 未選択");
            Three_Voice_List.Add("チャット:小隊-送信 | 未選択");
            Three_Voice_List.Add("チャット:小隊-受信 | 未選択");
            for (int Number = 0; Number < Main_Voice_List.Count; Number++)
                Voice_List.Items.Add(Main_Voice_List[Number]);
            for (int Number = 0; Number < Sub_Voice_List.Count; Number++)
                Voice_Sub_List.Items.Add(Sub_Voice_List[Number]);
            for (int Number = 0; Number < Three_Voice_List.Count; Number++)
                Voice_Three_List.Items.Add(Three_Voice_List[Number]);
            ColorMode_Change();
            Voice_List_Full_File_Name.Clear();
            Voice_Sub_List_Full_File_Name.Clear();
            Voice_Three_List_Full_File_Name.Clear();
            for (int Number = 0; Number < 34; Number++)
                Voice_List_Full_File_Name.Add(new List<string>());
            for (int Number = 0; Number < 16; Number++)
                Voice_Sub_List_Full_File_Name.Add(new List<string>());
            for (int Number = 0; Number < 6; Number++)
                Voice_Three_List_Full_File_Name.Add(new List<string>());
        }
        //引数:新サウンドエンジンの音声Mod=true,旧サウンドエンジン=false
        public async void Window_Show()
        {
            //画面を表示
            Volume_S.Value = 50;
            if (File.Exists(Voice_Set.Special_Path + "/Configs/Voice_Create.conf"))
            {
                try
                {
                    StreamReader str = Sub_Code.File_Decrypt_To_Stream(Voice_Set.Special_Path + "/Configs/Voice_Create.conf", "Voice_Create_Configs_Save");
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
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show("設定を読み込めませんでした。。\nエラー回避のため設定は削除されます。");
                    File.Delete(Voice_Set.Special_Path + "/Configs/Voice_Create.conf");
                    Volume_S.Value = 75;
                    Sub_Code.Error_Log_Write(e.Message);
                }
            }
            else
            {
                ColorMode_C.IsChecked = true;
                ColorMode_Change();
            }
            if (Sub_Code.IsWindowBarShow)
            {
                Voice_Clear_B.Margin = new Thickness(Voice_Clear_B.Margin.Left, 25, 0, 0);
                ColorMode_C.Margin = new Thickness(ColorMode_C.Margin.Left, 45, 0, 0);
                ColorMode_T.Margin = new Thickness(ColorMode_T.Margin.Left, 30, 0, 0);
            }
            else
            {
                Voice_Clear_B.Margin = new Thickness(Voice_Clear_B.Margin.Left, 0, 0, 0);
                ColorMode_C.Margin = new Thickness(ColorMode_C.Margin.Left, 40, 0, 0);
                ColorMode_T.Margin = new Thickness(ColorMode_T.Margin.Left, 25, 0, 0);
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
            if (!IsBusy && !IsCreating)
            {
                IsBusy = true;
                Pause_Volume_Animation(true, 10f);
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
                    Message_T.Opacity -= 0.025;
                await Task.Delay(1000 / 60);
            }
            if (IsMessageShowing)
            {
                IsMessageShowing = false;
                Message_T.Text = "";
            }
            Message_T.Opacity = 1;
        }
        private void Voice_Back_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy || IsCreating)
                return;
            Pause_Volume_Animation(true, 5);
            //音声リスト1へ移動
            Voice_Next_B.Visibility = Visibility.Visible;
            Voice_Three_List.Visibility = Visibility.Hidden;
            if (List_Index_Mode == 2)
            {
                Voice_Sub_List.Visibility = Visibility.Visible;
                Voice_List.Visibility = Visibility.Hidden;
            }
            else if (List_Index_Mode == 1)
            {
                Voice_Sub_List.Visibility = Visibility.Hidden;
                Voice_List.Visibility = Visibility.Visible;
                Voice_Back_B.Visibility = Visibility.Hidden;
            }
            if (List_Index_Mode > 0)
                List_Index_Mode--;
            Voice_List_T.Text = "音声リスト" + (List_Index_Mode + 1);
            if (List_Index_Mode == 0 && Voice_List.SelectedIndex != -1)
                Voice_File_Reset(Voice_List_Full_File_Name, Voice_List.SelectedIndex);
            else if (List_Index_Mode == 1 && Voice_Sub_List.SelectedIndex != -1)
                Voice_File_Reset(Voice_Sub_List_Full_File_Name, Voice_Sub_List.SelectedIndex);
            else
                Voice_File_List.Items.Clear();
        }
        private void Voice_Next_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy || IsCreating)
                return;
            Pause_Volume_Animation(true, 5);
            //音声リスト2へ移動
            Voice_Back_B.Visibility = Visibility.Visible;
            Voice_List.Visibility = Visibility.Hidden;
            if (List_Index_Mode == 1)
            {
                Voice_Sub_List.Visibility = Visibility.Hidden;
                Voice_Three_List.Visibility = Visibility.Visible;
                Voice_Next_B.Visibility = Visibility.Hidden;
            }
            else if (List_Index_Mode == 0)
            {
                Voice_Sub_List.Visibility = Visibility.Visible;
                Voice_Three_List.Visibility = Visibility.Hidden;
            }
            if (List_Index_Mode < 2)
                List_Index_Mode++;
            Voice_List_T.Text = "音声リスト" + (List_Index_Mode + 1);
            if (List_Index_Mode == 1 && Voice_Sub_List.SelectedIndex != -1)
                Voice_File_Reset(Voice_Sub_List_Full_File_Name, Voice_Sub_List.SelectedIndex);
            else if (List_Index_Mode == 2 && Voice_Three_List.SelectedIndex != -1)
                Voice_File_Reset(Voice_Three_List_Full_File_Name, Voice_Three_List.SelectedIndex);
            else
                Voice_File_List.Items.Clear();
        }
        private void Voice_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsBusy || IsCreating)
                return;
            Pause_Volume_Animation(true, 5);
            if (Voice_List.SelectedIndex != -1)
            {
                //音声が選択されたら実行
                Voice_File_Reset(Voice_List_Full_File_Name, Voice_List.SelectedIndex);
            }
        }
        private void Voice_Sub_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsBusy || IsCreating)
                return;
            Pause_Volume_Animation(true, 5);
            if (Voice_Sub_List.SelectedIndex != -1)
            {
                //↑と同様
                Voice_File_Reset(Voice_Sub_List_Full_File_Name, Voice_Sub_List.SelectedIndex);
            }
        }
        private void Voice_Three_List_SeletionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsBusy || IsCreating)
                return;
            Pause_Volume_Animation(true, 5);
            if (Voice_Three_List.SelectedIndex != -1)
            {
                //↑と同様
                Voice_File_Reset(Voice_Three_List_Full_File_Name, Voice_Three_List.SelectedIndex);
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
                    Voice_File_List.Items.Add(Path.GetFileName(Temp));
            }
        }
        private void Voice_List_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //種類の選択を解除
            Voice_List.SelectedIndex = -1;
            Voice_File_List.Items.Clear();
        }
        private void Voice_Sub_List_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //音声の選択を解除
            Voice_Sub_List.SelectedIndex = -1;
            Voice_File_List.Items.Clear();
        }
        private void Voice_Three_List_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //音声の選択を解除
            Voice_Three_List.SelectedIndex = -1;
            Voice_File_List.Items.Clear();
        }
        private void Voice_Select_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy || IsCreating)
                return;
            if (Voice_List.SelectedIndex == -1 && List_Index_Mode == 0)
            {
                Message_Feed_Out("音声タイプが選択されていません。");
                return;
            }
            else if (Voice_Sub_List.SelectedIndex == -1 && List_Index_Mode == 1)
            {
                Message_Feed_Out("音声タイプが選択されていません。");
                return;
            }
            else if (Voice_Three_List.SelectedIndex == -1 && List_Index_Mode == 2)
            {
                Message_Feed_Out("音声タイプが選択されていません。");
                return;
            }
            OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "音声ファイルを選択してください。",
                Filter = "音声ファイル(*.mp3;*.wav;*.ogg;*.flac;*.wma;*.aac;*.mp4)|*.mp3;*.wav;*.ogg;*.flac;*.wma;*.aac;*.mp4",
                Multiselect = true
            };
            if (ofd.ShowDialog() == DialogResult.OK)
                Add_Voice(ofd.FileNames, false);
        }
        public void Add_Voice(string[] Voice_Files, bool IsDragDropMode)
        {
            if (IsDragDropMode)
            {
                if (IsBusy || IsCreating)
                    return;
                if (Voice_List.SelectedIndex == -1 && List_Index_Mode == 0)
                {
                    Message_Feed_Out("音声タイプが選択されていません。");
                    return;
                }
                else if (Voice_Sub_List.SelectedIndex == -1 && List_Index_Mode == 1)
                {
                    Message_Feed_Out("音声タイプが選択されていません。");
                    return;
                }
                else if (Voice_Three_List.SelectedIndex == -1 && List_Index_Mode == 2)
                {
                    Message_Feed_Out("音声タイプが選択されていません。");
                    return;
                }
            }
            //選択している音声の種類に音声ファイルを追加
            int IndexNumber = -1;
            if (List_Index_Mode == 0)
                IndexNumber = Voice_List.SelectedIndex;
            else if (List_Index_Mode == 1)
                IndexNumber = Voice_Sub_List.SelectedIndex;
            else if (List_Index_Mode == 2)
                IndexNumber = Voice_Three_List.SelectedIndex;
            //音声を追加しそのタイプを選択済みにする
            if (List_Index_Mode == 0)
            {
                List<string> Temp = Voice_List_Full_File_Name[IndexNumber];
                foreach (string SelectFile in Voice_Files)
                    Temp.Add(SelectFile);
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
            else if (List_Index_Mode == 1)
            {
                List<string> Temp = Voice_Sub_List_Full_File_Name[Voice_Sub_List.SelectedIndex];
                foreach (string SelectFile in Voice_Files)
                    Temp.Add(SelectFile);
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
            else if (List_Index_Mode == 2)
            {
                List<string> Temp = Voice_Three_List_Full_File_Name[Voice_Three_List.SelectedIndex];
                foreach (string SelectFile in Voice_Files)
                    Temp.Add(SelectFile);
                Voice_Three_List_Full_File_Name[IndexNumber] = Temp;
                Voice_File_Reset(Voice_Three_List_Full_File_Name, IndexNumber);
                Three_Voice_List[IndexNumber] = Three_Voice_List[IndexNumber].Replace("未選択", "選択済み");
                Voice_Three_List.Items[IndexNumber] = Three_Voice_List[IndexNumber];
                if (ColorMode_C.IsChecked.Value)
                {
                    ListBoxItem LBI = new ListBoxItem();
                    LBI.Content = Three_Voice_List[IndexNumber];
                    //LBI.Foreground = (Brush)new BrushConverter().ConvertFromString("#BFFF2C8C");
                    Voice_Three_List.Items[IndexNumber] = LBI;
                }
                Voice_Three_List.SelectedIndex = IndexNumber;
            }
        }
        private void Voice_Delete_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy || IsCreating)
                return;
            if (Voice_File_List.SelectedIndex == -1)
            {
                Message_Feed_Out("取消したい音声ファイルが選択されていません。");
                return;
            }
            Pause_Volume_Animation(true, 5);
            //選択している音声をリストから削除
            //音声が1つしかなかった場合選択済みから未選択に変える
            int Number = Voice_File_List.SelectedIndex;
            Voice_File_List.SelectedIndex = -1;
            if (List_Index_Mode == 0)
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
            else if (List_Index_Mode == 1)
            {
                List<string> Temp = Voice_Sub_List_Full_File_Name[Voice_Sub_List.SelectedIndex];
                Temp.RemoveAt(Number);
                Voice_Sub_List_Full_File_Name[Voice_Sub_List.SelectedIndex] = Temp;
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
            else if (List_Index_Mode == 2)
            {
                List<string> Temp = Voice_Three_List_Full_File_Name[Voice_Three_List.SelectedIndex];
                Temp.RemoveAt(Number);
                Voice_Three_List_Full_File_Name[Voice_Three_List.SelectedIndex] = Temp;
                Voice_File_List.Items.RemoveAt(Number);
                if (Temp.Count == 0)
                {
                    int Number_Selected = Voice_Three_List.SelectedIndex;
                    Three_Voice_List[Number_Selected] = Three_Voice_List[Number_Selected].Replace("選択済み", "未選択");
                    Voice_Three_List.Items[Number_Selected] = Three_Voice_List[Number_Selected];
                    if (ColorMode_C.IsChecked.Value)
                    {
                        ListBoxItem LBI = new ListBoxItem();
                        LBI.Content = Sub_Voice_List[Number_Selected].Replace("System.Windows.Controls.ListBoxItem: ", "");
                        LBI.Foreground = (Brush)new BrushConverter().ConvertFromString("#BFFF2C8C");
                        Voice_Three_List.Items[Number_Selected] = LBI;
                    }
                }
            }
            if (Voice_File_List.Items.Count > Number)
                Voice_File_List.SelectedIndex = Number;
        }
        private void Voice_File_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Pause_Volume_Animation(true, 5);
        }
        private void Volume_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //音量を変更
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)Volume_S.Value / 100);
            Volume_T.Text = "音量:" + (int)Volume_S.Value;
        }
        private void Voice_Play_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy || IsCreating)
                return;
            if (Voice_File_List.SelectedIndex == -1)
            {
                Message_Feed_Out("音声ファイルが選択されていません。");
                return;
            }
            if (Selected_Voice_Index != Voice_File_List.SelectedIndex)
            {
                //選択している音声をファイルから再生
                //ファイルがなかった場合メッセージを表示
                string Select_File = "";
                if (List_Index_Mode == 0)
                {
                    List<string> Temp = Voice_List_Full_File_Name[Voice_List.SelectedIndex];
                    Select_File = Temp[Voice_File_List.SelectedIndex];
                }
                else if (List_Index_Mode == 1)
                {
                    List<string> Temp = Voice_Sub_List_Full_File_Name[Voice_Sub_List.SelectedIndex];
                    Select_File = Temp[Voice_File_List.SelectedIndex];
                }
                else if (List_Index_Mode == 2)
                {
                    List<string> Temp = Voice_Three_List_Full_File_Name[Voice_Three_List.SelectedIndex];
                    Select_File = Temp[Voice_File_List.SelectedIndex];
                }
                if (!File.Exists(Select_File))
                {
                    Message_Feed_Out("音声ファイルが存在しません。削除された可能性があります。");
                    return;
                }
                Bass.BASS_ChannelStop(Stream);
                Bass.BASS_StreamFree(Stream);
                Position_S.Value = 0;
                int StreamHandle = Bass.BASS_StreamCreateFile(Select_File, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_LOOP);
                Stream = BassFx.BASS_FX_TempoCreate(StreamHandle, BASSFlag.BASS_FX_FREESOURCE);
                Position_S.Maximum = Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetLength(Stream, BASSMode.BASS_POS_BYTES));
                IsMusicEnd = new SYNCPROC(EndSync);
                Bass.BASS_ChannelPlay(Stream, true);
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)Volume_S.Value / 100);
                Bass.BASS_ChannelSetSync(Stream, BASSSync.BASS_SYNC_END | BASSSync.BASS_SYNC_MIXTIME, 0, IsMusicEnd, IntPtr.Zero);
                Bass.BASS_ChannelSetDevice(Stream, Video_Mode.Sound_Device);
                TimeSpan Time = TimeSpan.FromSeconds(Position_S.Maximum);
                string Minutes = Time.Minutes.ToString();
                string Seconds = Time.Seconds.ToString();
                if (Time.Minutes < 10)
                    Minutes = "0" + Time.Minutes;
                if (Time.Seconds < 10)
                    Seconds = "0" + Time.Seconds;
                IsPaused = false;
                Max_Time = Minutes + ":" + Seconds;
                Selected_Voice_Index = Voice_File_List.SelectedIndex;
            }
            else
                Play_Volume_Animation(15);
        }
        private void Voice_Pause_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
                return;
            //再生している音声を停止
            Pause_Volume_Animation(false, 5);
        }
        private void Voice_Save_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy || IsCreating)
                return;
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
                        stw.WriteLine(Project_Name_T.Text);
                    else
                        stw.WriteLine(Project_Name_T.Text + "|IsNotChangeProjectNameMode=true");
                    int Number = 0;
                    foreach (List<string> Lists in Voice_List_Full_File_Name)
                    {
                        foreach (string Files in Lists)
                            stw.WriteLine(Number + "|" + Files);
                        Number++;
                    }
                    foreach (List<string> Lists in Voice_Sub_List_Full_File_Name)
                    {
                        foreach (string Files in Lists)
                            stw.WriteLine(Number + "|" + Files);
                        Number++;
                    }
                    foreach (List<string> Lists in Voice_Three_List_Full_File_Name)
                    {
                        foreach (string Files in Lists)
                            stw.WriteLine(Number + "|" + Files);
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
                return;
            //保存したファイルから状態を復元
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Title = "セーブファイルを選択してください。",
                Filter = "セーブファイル(*.wvs)|*.wvs",
                Multiselect = false,
                InitialDirectory = Directory.GetCurrentDirectory()
            };
            if (ofd.ShowDialog() == DialogResult.OK)
                Voice_Load_From_File(ofd.FileName);
            ofd.Dispose();
        }
        public void Voice_Load_From_File(string WVS_File)
        {
            try
            {
                //音声を配置
                string line;
                StreamReader file = Sub_Code.File_Decrypt_To_Stream(WVS_File, "SRTTbacon_Create_Voice_Save");
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
                    else if (Number < 50)
                    {
                        List<string> List_Number = Voice_Sub_List_Full_File_Name[Number - 34];
                        List_Number.Add(File_Path);
                        Sub_Voice_List[Number - 34] = Sub_Voice_List[Number - 34].Replace("未選択", "選択済み");
                        Voice_Sub_List.Items[Number - 34] = Sub_Voice_List[Number - 34];
                        Voice_Sub_List_Full_File_Name[Number - 34] = List_Number;
                    }
                    else
                    {
                        List<string> List_Number = Voice_Three_List_Full_File_Name[Number - 50];
                        List_Number.Add(File_Path);
                        Three_Voice_List[Number - 50] = Three_Voice_List[Number - 50].Replace("未選択", "選択済み");
                        Voice_Three_List.Items[Number - 50] = Three_Voice_List[Number - 50];
                        Voice_Three_List_Full_File_Name[Number - 50] = List_Number;
                    }
                }
                file.Close();
                if (List_Index_Mode == 0 && Voice_List.SelectedIndex != -1)
                    Voice_File_Reset(Voice_List_Full_File_Name, Voice_List.SelectedIndex);
                else if (List_Index_Mode == 1 && Voice_Sub_List.SelectedIndex != -1)
                    Voice_File_Reset(Voice_Sub_List_Full_File_Name, Voice_Sub_List.SelectedIndex);
                else if (List_Index_Mode == 2 && Voice_Three_List.SelectedIndex != -1)
                    Voice_File_Reset(Voice_Three_List_Full_File_Name, Voice_Three_List.SelectedIndex);
                ColorMode_Change();
                Message_Feed_Out("ロードしました。");
            }
            catch (Exception e1)
            {
                Message_Feed_Out("指定したセーブデータが破損しています。");
                Sub_Code.Error_Log_Write(e1.Message);
            }
        }
        private async void Voice_Create_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy || IsCreating)
                return;
            bool IsOK = false;
            foreach (string Name_Now in Main_Voice_List)
                if (Name_Now.Contains("選択済み"))
                    IsOK = true;
            if (!IsOK)
            {
                foreach (string Name_Now in Sub_Voice_List)
                    if (Name_Now.Contains("選択済み"))
                        IsOK = true;
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
                    throw new Exception("プロジェクト名に'/'または'\\'を付けることはできません。");
            }
            catch (Exception e1)
            {
                Message_Feed_Out("プロジェクト名に不適切な文字が含まれています。");
                Sub_Code.Error_Log_Write(e1.Message);
                return;
            }
            Pause_Volume_Animation(false, 15);
            //作成画面へ
            List<List<string>> Temp = new List<List<string>>();
            for (int Number_01 = 0; Number_01 < Voice_List_Full_File_Name.Count; Number_01++)
                Temp.Add(Voice_List_Full_File_Name[Number_01]);
            for (int Number_02 = 0; Number_02 < Voice_Sub_List_Full_File_Name.Count; Number_02++)
                Temp.Add(Voice_Sub_List_Full_File_Name[Number_02]);
            for (int Number_03 = 0; Number_03 < Voice_Three_List_Full_File_Name.Count; Number_03++)
                Temp.Add(Voice_Three_List_Full_File_Name[Number_03]);
            Voice_Create_Window.Window_Show_V2(Project_Name_T.Text, Temp);
            Voice_Create_Window.Opacity = 0;
            Voice_Create_Window.Visibility = Visibility.Visible;
            while (Voice_Create_Window.Opacity < 1)
            {
                Voice_Create_Window.Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
            while (Voice_Create_Window.Visibility == Visibility.Visible)
                await Task.Delay(100);
            //作成画面で作成ボタンが押されたら開始
            if (Sub_Code.CreatingProject)
            {
                Sub_Code.CreatingProject = false;
                IsCreating = true;
                Message_T.Opacity = 1;
                IsMessageShowing = false;
                try
                {
                    string Dir_Path = Directory.GetCurrentDirectory();
                    string Dir_Name = Dir_Path + "/Projects/" + Project_Name_T.Text;
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
                        await Multithread.Convert_To_Wav(Sub_Code.Check_WAV_Get_List(Dir_Name + "/Voices", true).ToArray(), Dir_Name + "/Voices", true);
                        Sub_Code.Volume_Set(Dir_Name + "/Voices", Encode_Mode.WAV);
                        await Task.Delay(500);
                    }
                    string File_Name = Project_Name_T.Text.Replace(" ", "_");
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
                    Flash.Flash_Start();
                    if (Voice_Set.WoTB_Path != "")
                    {
                        Message_T.Text = "ダイアログを表示しています...";
                        await Task.Delay(50);
                        MessageBoxResult result = System.Windows.MessageBox.Show("完了しました。WoTBに適応しますか?", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes);
                        if (result == MessageBoxResult.Yes)
                        {
                            try
                            {
                                string GetDir = Dir_Name + "/" + Project_Name_T.Text + "_Mod/Data/WwiseSound";
                                Sub_Code.DVPL_File_Delete(Voice_Set.WoTB_Path + "/Data/WwiseSound/" + Sub_Code.SetLanguage + "/voiceover_crew.bnk");
                                Sub_Code.DVPL_File_Delete(Voice_Set.WoTB_Path + "/Data/WwiseSound/reload.bnk");
                                Sub_Code.DVPL_File_Delete(Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_chat_quick_commands.bnk");
                                Sub_Code.DVPL_File_Delete(Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_battle.bnk");
                                Sub_Code.DVPL_File_Copy(Dir_Name + "/voiceover_crew.bnk", Voice_Set.WoTB_Path + "/Data/WwiseSound/" + Sub_Code.SetLanguage + "/voiceover_crew.bnk", true);
                                Sub_Code.DVPL_File_Copy(Dir_Name + "/reload.bnk", Voice_Set.WoTB_Path + "/Data/WwiseSound/reload.bnk", true);
                                Sub_Code.DVPL_File_Copy(Dir_Name + "/ui_chat_quick_commands.bnk", Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_chat_quick_commands.bnk", true);
                                Sub_Code.DVPL_File_Copy(Dir_Name + "/ui_battle.bnk", Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_battle.bnk", true);
                            }
                            catch (Exception e1)
                            {
                                Message_Feed_Out("WoTBに適応できませんでした。");
                                Sub_Code.Error_Log_Write(e1.Message);
                                IsCreating = false;
                                return;
                            }
                        }
                    }
                    Sub_Code.DVPL_Encode = false;
                    Sub_Code.SetLanguage = "";
                    Message_Feed_Out("完了しました。\n保存先:\\Projects\\" + Project_Name_T.Text);
                    Border_All.Visibility = Visibility.Hidden;
                    IsCreating = false;
                }
                catch (Exception e1)
                {
                    Message_Feed_Out("致命的なエラーが発生し正常に作成されませんでした。");
                    Sub_Code.Error_Log_Write(e1.Message);
                    Border_All.Visibility = Visibility.Hidden;
                    IsCreating = false;
                }
                if (IsExecuteWoTB)
                {
                    Message_Feed_Out("WoTBを起動しています...");
                    System.Diagnostics.Process.Start(Voice_Set.WoTB_Path + "\\wotblitz.exe");
                }
            }
            else
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
                Sub_Code.File_Encrypt(Voice_Set.Special_Path + "/Configs/Voice_Create.tmp", Voice_Set.Special_Path + "/Configs/Voice_Create.conf", "Voice_Create_Configs_Save", true);
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        //Wwiseのプロジェクトファイルを用いて.bnkファイルを作成(ファイル数やイベントの内容も変更できます)
        async Task BNK_Create_V2(string Dir_Name)
        {
            if (!Directory.Exists(Dir_Name + "/Voices"))
                return;
            if (BGM_Reload_C.Visibility == Visibility.Hidden)
                BGM_Reload_C.IsChecked = false;
            if (Sub_Code.Default_Voice)
            {
                //項目に音声が入っていないかつ、設定画面のチェックを入れている場合、標準の音声を再生させるように
                for (int Number = 0; Number < Voice_List_Full_File_Name.Count; Number++)
                    if (Voice_List_Full_File_Name[Number].Count == 0)
                        Sub_Code.File_Copy_V2(Voice_Set.Special_Path + "\\SE\\Voices", Dir_Name + "/Voices", Sub_Code.Default_Name[Number]);
            }
            Message_T.Text = "音声ファイルをwavに変換しています...";
            await Task.Delay(50);
            await Multithread.Convert_To_Wav(Dir_Name + "/Voices", true, false, true);
            Message_T.Text = "プロジェクトファイルを作成しています...";
            await Task.Delay(50);
            if (File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp"))
                File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.wwu", true);
            else
                File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.wwu", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp", true);
            Message_T.Text = "SEの有無をチェックし、適応しています...";
            await Task.Delay(50);
            Wwise_Class.Wwise_Project_Create Wwise = new Wwise_Class.Wwise_Project_Create(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod");
            await Set_SE(Wwise);
            Wwise.Sound_Add_Wwise(Dir_Name + "/Voices", false, true);
            Wwise.Save();
            if (File.Exists(Dir_Name + "/Voices/battle_bgm_01.wav"))
                Message_T.Text = ".bnkファイルを作成しています...\nBGMファイルが含まれているため時間がかかります。";
            else
                Message_T.Text = ".bnkファイルを作成しています...";
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
                Wwise.Event_Not_Include("Reload", 224498802);
                Wwise.Project_Build("reload", Dir_Name + "/reload.bnk");
                await Task.Delay(500);
                Wwise.Sound_Music_Add_Wwise(Dir_Name + "/Voices");
                Wwise.Save();
                Wwise.Project_Build("voiceover_crew", Dir_Name + "/voiceover_crew.bnk");
                Wwise.Event_Reset();
            }
            await Task.Delay(500);
            Wwise.Clear();
            if (File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp"))
                File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.wwu", true);
        }
        async Task<bool> Set_SE(Wwise_Class.Wwise_Project_Create Wwise)
        {
            List<string> ShortIDs = new List<string>();
            ShortIDs.Add("924876614");            //占領で戦闘終了_遭遇戦
            ShortIDs.Add("366387412");            //クイックコマンド
            ShortIDs.Add("528636008");            //弾薬庫
            ShortIDs.Add("667880140");            //大破
            ShortIDs.Add("965426293");            //貫通
            ShortIDs.Add("330527106");            //モジュールダメージ
            ShortIDs.Add("973877864");            //無線機
            ShortIDs.Add("602706971");            //燃料タンク
            ShortIDs.Add("1017674104");           //非貫通
            ShortIDs.Add("138290727");            //装填完了
            ShortIDs.Add("917399664");            //第六感
            ShortIDs.Add("479275647");            //敵発見
            ShortIDs.Add("816581364");            //戦闘開始タイマー
            ShortIDs.Add("208623057");            //ロックオン
            ShortIDs.Add("1025889019");           //アンロック
            ShortIDs.Add("921545948");            //ノイズ音
            Voice_Set.Set_SE_Change_Name(Wwise.Project_Dir + "\\Originals\\Voices\\ja");
            Message_T.Text = "SEをプリセットからロードしています...";
            await Task.Delay(75);
            for (int Number_01 = 0; Number_01 < ShortIDs.Count; Number_01++)
            {
                Wwise.Delete_CAkSounds(ShortIDs[Number_01]);
                string Temp = Voice_Create_Window.SE_Change_Window.Preset_List[Voice_Create_Window.SE_Change_Window.Preset_Index][Number_01 + 1];
                foreach (string File_Now in Temp.Split('|'))
                {
                    if (File.Exists(File_Now))
                    {
                        if (Path.GetExtension(File_Now) == ".wav")
                        {
                            if (ShortIDs[Number_01] == "816581364")
                                Wwise.Add_Sound(ShortIDs[Number_01], File_Now, "ja", false, null, "", 0, false, true, true);
                            else
                                Wwise.Add_Sound(ShortIDs[Number_01], File_Now, "ja");
                        }
                        else
                        {
                            string To_File = Voice_Set.Special_Path + "\\SE\\" + Path.GetFileNameWithoutExtension(File_Now) + "_TEMP" + Sub_Code.r.Next(0, 100000) + ".wav";
                            Sub_Code.Audio_Encode_To_Other(File_Now, To_File, ".wav", false);
                            if (ShortIDs[Number_01] == "816581364")
                                Wwise.Add_Sound(ShortIDs[Number_01], To_File, "ja", false, null, "", 0, false, true, true);
                            else
                                Wwise.Add_Sound(ShortIDs[Number_01], To_File, "ja");
                            File.Delete(To_File);
                        }
                    }
                }
            }
            return true;
        }
        private void Voice_Clear_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy || IsCreating)
                return;
            MessageBoxResult result = System.Windows.MessageBox.Show("追加された音声をクリアしますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                List_Text_Reset();
                Pause_Volume_Animation(true, 5);

                Voice_Next_B.Visibility = Visibility.Visible;
                Voice_Sub_List.Visibility = Visibility.Hidden;
                Voice_Three_List.Visibility = Visibility.Hidden;
                Voice_List.Visibility = Visibility.Visible;
                Voice_Back_B.Visibility = Visibility.Hidden;
                List_Index_Mode = 0;
                Voice_List_T.Text = "音声リスト" + (List_Index_Mode + 1);
                Project_Name_T.Text = "";
                Project_Name_Text.Text = "プロジェクト名";
                Project_Name_T.IsEnabled = true;
                if (Voice_List.SelectedIndex != -1)
                    Voice_File_Reset(Voice_List_Full_File_Name, Voice_List.SelectedIndex);
                else
                    Voice_File_List.Items.Clear();
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
                br = (Brush)new BrushConverter().ConvertFromString("#BFFF2C8C");
            else
                br = Brushes.Aqua;
            int Select_Index = -1;
            if (Voice_List.Visibility == Visibility.Visible && Voice_List.SelectedIndex != -1)
                Select_Index = Voice_List.SelectedIndex;
            else if (Voice_Sub_List.Visibility == Visibility.Visible && Voice_Sub_List.SelectedIndex != -1)
                Select_Index = Voice_Sub_List.SelectedIndex;
            else if (Voice_Three_List.Visibility == Visibility.Visible && Voice_Three_List.SelectedIndex != -1)
                Select_Index = Voice_Three_List.SelectedIndex;
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
            for (int Number = 0; Number < Voice_Three_List.Items.Count; Number++)
            {
                if (Three_Voice_List[Number].Contains("未選択"))
                {
                    ListBoxItem LBI = new ListBoxItem();
                    LBI.Content = Three_Voice_List[Number];
                    LBI.Foreground = br;
                    Voice_Three_List.Items[Number] = LBI;
                }
            }
            if (List_Index_Mode == 0 && Select_Index != -1)
                Voice_List.SelectedIndex = Select_Index;
            else if (List_Index_Mode == 1 && Select_Index != -1)
                Voice_Sub_List.SelectedIndex = Select_Index;
            else if (List_Index_Mode == 2 && Select_Index != -1)
                Voice_Three_List.SelectedIndex = Select_Index;
        }
        private void BGM_Reload_C_Click(object sender, RoutedEventArgs e)
        {
            Configs_Save();
        }
        private void Execute_WoTB_C_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (IsExecuteWoTB)
                Execute_WoTB_C.Source = Sub_Code.Check_02;
            else
            {
                if (Voice_Set.WoTB_Path == "")
                {
                    Message_Feed_Out("Steam版WoTBのインストール先を取得できませんでした。\nホーム画面からフォルダを指定してください。");
                    return;
                }
                Execute_WoTB_C.Source = Sub_Code.Check_04;
            }
            IsExecuteWoTB = !IsExecuteWoTB;
        }
        private void Execute_WoTB_C_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsExecuteWoTB)
                Execute_WoTB_C.Source = Sub_Code.Check_04;
            else
                Execute_WoTB_C.Source = Sub_Code.Check_02;
        }
        private void Execute_WoTB_C_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsExecuteWoTB)
                Execute_WoTB_C.Source = Sub_Code.Check_03;
            else
                Execute_WoTB_C.Source = Sub_Code.Check_01;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Execute_WoTB_C.Source = Sub_Code.Check_01;
        }
        async void Play_Volume_Animation(float Feed_Time = 30f)
        {
            IsPaused = false;
            Bass.BASS_ChannelPlay(Stream, false);
            float Volume_Now = 1f;
            Bass.BASS_ChannelGetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, ref Volume_Now);
            float Volume_Plus = (float)(Volume_S.Value / 100) / Feed_Time;
            while (Volume_Now < (float)(Volume_S.Value / 100) && !IsPaused)
            {
                Volume_Now += Volume_Plus;
                if (Volume_Now > 1f)
                    Volume_Now = 1f;
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, Volume_Now);
                await Task.Delay(1000 / 60);
            }
        }
        async void Pause_Volume_Animation(bool IsStop, float Feed_Time = 30f)
        {
            IsPaused = true;
            float Volume_Now = 1f;
            Bass.BASS_ChannelGetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, ref Volume_Now);
            float Volume_Minus = Volume_Now / Feed_Time;
            while (Volume_Now > 0f && IsPaused)
            {
                Volume_Now -= Volume_Minus;
                if (Volume_Now < 0f)
                    Volume_Now = 0f;
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, Volume_Now);
                await Task.Delay(1000 / 60);
            }
            if (Volume_Now <= 0f)
            {
                if (IsStop)
                {
                    Bass.BASS_ChannelStop(Stream);
                    Bass.BASS_StreamFree(Stream);
                    Position_S.Value = 0;
                    Position_S.Maximum = 0;
                    Position_T.Text = "00:00 / 00:00";
                    Selected_Voice_Index = -1;
                    Max_Time = "00:00";
                }
                else if (IsPaused)
                    Bass.BASS_ChannelPause(Stream);
            }
        }
        void Position_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsBusy)
                return;
            IsLocationChanging = true;
            if (Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_PLAYING)
            {
                IsPlayingMouseDown = true;
                Pause_Volume_Animation(false, 10);
            }
        }
        void Position_MouseUp(object sender, MouseButtonEventArgs e)
        {
            IsLocationChanging = false;
            Bass.BASS_ChannelSetPosition(Stream, Position_S.Value);
            if (IsPlayingMouseDown)
            {
                IsPaused = false;
                Play_Volume_Animation(10);
                IsPlayingMouseDown = false;
            }
        }
        private void Position_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLocationChanging)
                Music_Pos_Change(Position_S.Value, false);
        }
        void Music_Pos_Change(double Pos, bool IsBassPosChange)
        {
            if (IsBusy)
                return;
            if (IsBassPosChange)
                Bass.BASS_ChannelSetPosition(Stream, Pos);
            TimeSpan Time = TimeSpan.FromSeconds(Pos);
            string Minutes = Time.Minutes.ToString();
            string Seconds = Time.Seconds.ToString();
            if (Time.Minutes < 10)
                Minutes = "0" + Time.Minutes;
            if (Time.Seconds < 10)
                Seconds = "0" + Time.Seconds;
            Position_T.Text = Minutes + ":" + Seconds + " / " + Max_Time;
        }
    }
}