using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using WK.Libraries.BetterFolderBrowserNS;
using MessageBox = System.Windows.MessageBox;

namespace WoTB_Voice_Mod_Creater.Class
{
    public partial class Voice_Create : System.Windows.Controls.UserControl
    {
        private class Add_Voice_Param
        {
            public string File_Path { get; set; }
            public int Type_Index { get; set; }
            public int Voice_Index { get; set; }
            public Add_Voice_Param(string File_Path, int Type_Index, int Voice_Index)
            {
                this.File_Path = File_Path;
                this.Type_Index = Type_Index;
                this.Voice_Index = Voice_Index;
            }
        }
        public static string Save_Load_Dir = "";
        string Max_Time = "00:00";
        int Stream;
        int List_Index_Mode = 0;
        int Selected_Voice_Index = -1;
        int Back_Pos_Now = 0;
        bool IsBusy = false;
        bool IsMessageShowing = false;
        bool IsCreating = false;
        bool IsExecuteWoTB = false;
        bool IsPaused = false;
        bool IsPlayingMouseDown = false;
        bool IsLocationChanging = false;
        bool IsEnded = false;
        bool IsLoadedWVS = false;
        bool IsWVSIncludeSound = false;
        bool IsIncludeSound = false;
        bool IsPushedZkey = false;
        bool IsPushedYkey = false;
        bool IsPushedCkey = false;
        bool IsPushedVkey = false;
        WVS_Load WVS_File = new WVS_Load();
        SYNCPROC IsMusicEnd;
        public List<object> Back_To_The_Future = new List<object>();
        List<List<Voice_Event_Setting>> Sound_Setting = new List<List<Voice_Event_Setting>>();
        List<Voice_Sound_Setting> Copy_Voices = new List<Voice_Sound_Setting>();
        List<string> Main_Voice_List = new List<string>();
        List<string> Sub_Voice_List = new List<string>();
        List<string> Three_Voice_List = new List<string>();
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
            System.Windows.Controls.ContextMenu pMenu = new System.Windows.Controls.ContextMenu();
            System.Windows.Controls.MenuItem item1 = new System.Windows.Controls.MenuItem();
            item1.Header = "貼り付け";
            item1.Click += Voice_List_Paste_Click;
            pMenu.Items.Add(item1);
            Voice_File_List.ContextMenu = pMenu;
            Sub_Code.ShortIDs.Add(924876614);            //占領で戦闘終了_遭遇戦
            Sub_Code.ShortIDs.Add(366387412);            //クイックコマンド
            Sub_Code.ShortIDs.Add(528636008);            //弾薬庫
            Sub_Code.ShortIDs.Add(667880140);            //大破
            Sub_Code.ShortIDs.Add(965426293);            //貫通
            Sub_Code.ShortIDs.Add(330527106);            //モジュールダメージ
            Sub_Code.ShortIDs.Add(973877864);            //無線機
            Sub_Code.ShortIDs.Add(602706971);            //燃料タンク
            Sub_Code.ShortIDs.Add(1017674104);           //非貫通
            Sub_Code.ShortIDs.Add(138290727);            //装填完了
            Sub_Code.ShortIDs.Add(917399664);            //第六感
            Sub_Code.ShortIDs.Add(479275647);            //敵発見
            Sub_Code.ShortIDs.Add(816581364);            //戦闘開始タイマー
            Sub_Code.ShortIDs.Add(208623057);            //ロックオン
            Sub_Code.ShortIDs.Add(1025889019);           //アンロック
            Sub_Code.ShortIDs.Add(921545948);            //ノイズ音
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
                    if (Sub_Code.IsForcusWindow && Voice_Create_Setting_Window.Visibility == Visibility.Hidden && Voice_Create_Window.Visibility == Visibility.Hidden)
                    {
                        bool IsFocused = false;
                        if (Keyboard.FocusedElement is System.Windows.Controls.TextBox)
                        {
                            System.Windows.Controls.TextBox text = (System.Windows.Controls.TextBox)Keyboard.FocusedElement;
                            if (text.Name == "Project_Name_T")
                                IsFocused = true;
                        }
                        if (!IsBusy && !IsCreating && !IsFocused)
                        {
                            bool IsCtrlPushed = (Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) > 0;
                            if (IsCtrlPushed && (Keyboard.GetKeyStates(Key.Z) & KeyStates.Down) > 0)
                            {
                                if (!IsPushedZkey)
                                    History_Back();
                                IsPushedZkey = true;
                            }
                            else
                                IsPushedZkey = false;
                            if (IsCtrlPushed && (Keyboard.GetKeyStates(Key.Y) & KeyStates.Down) > 0)
                            {
                                if (!IsPushedYkey)
                                    History_Next();
                                IsPushedYkey = true;
                            }
                            else
                                IsPushedYkey = false;
                        }
                    }
                    if ((Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.C) & KeyStates.Down) > 0)
                    {
                        if (!IsPushedCkey)
                            Voice_List_Copy_Click(null, null);
                        IsPushedCkey = true;
                    }
                    else
                        IsPushedCkey = false;
                    if ((Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.V) & KeyStates.Down) > 0)
                    {
                        if (!IsPushedVkey)
                            Voice_List_Paste_Click(null, null);
                        IsPushedVkey = true;
                    }
                    else
                        IsPushedVkey = false;
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
            Sound_Setting.Clear();
            for (int Number = 0; Number < 3; Number++)
                Sound_Setting.Add(new List<Voice_Event_Setting>());
            Main_Voice_List.Add("味方にダメージ | 未選択");
            Main_Voice_List.Add("弾薬庫破損 | 未選択");
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
            {
                Voice_List.Items.Add(Main_Voice_List[Number]);
                Sound_Setting[0].Add(new Voice_Event_Setting());
            }
            for (int Number = 0; Number < Sub_Voice_List.Count; Number++)
            {
                Voice_Sub_List.Items.Add(Sub_Voice_List[Number]);
                if (Number == 15)
                    Sound_Setting[1][Sound_Setting[1].Count - 1].Volume = -11;
                Sound_Setting[1].Add(new Voice_Event_Setting());
            }
            for (int Number = 0; Number < Three_Voice_List.Count; Number++)
            {
                Voice_Three_List.Items.Add(Three_Voice_List[Number]);
                Sound_Setting[2].Add(new Voice_Event_Setting());
            }
            Sub_Code.Set_Event_ShortID(Sound_Setting);
            ColorMode_Change();
        }
        //引数:新サウンドエンジンの音声Mod=true,旧サウンドエンジン=false
        public async void Window_Show()
        {
            //画面を表示
            Volume_S.Value = 75;
            if (File.Exists(Voice_Set.Special_Path + "/Configs/Voice_Create.conf"))
            {
                try
                {
                    StreamReader str = Sub_Code.File_Decrypt_To_Stream(Voice_Set.Special_Path + "/Configs/Voice_Create.conf", "Voice_Create_Configs_Save");
                    Volume_S.Value = double.Parse(str.ReadLine());
                    try
                    {
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
                ColorMode_Change();
            if (Sub_Code.IsWindowBarShow)
            {
                Voice_Clear_B.Margin = new Thickness(Voice_Clear_B.Margin.Left, 25, 0, 0);
                Voice_Add_Dir_B.Margin = new Thickness(Voice_Add_Dir_B.Margin.Left, 25, 0, 0);
                Voice_Add_Dir_Help_B.Margin = new Thickness(Voice_Add_Dir_Help_B.Margin.Left, 30, 0, 0);
            }
            else
            {
                Voice_Clear_B.Margin = new Thickness(Voice_Clear_B.Margin.Left, 0, 0, 0);
                Voice_Add_Dir_B.Margin = new Thickness(Voice_Add_Dir_B.Margin.Left, 0, 0, 0);
                Voice_Add_Dir_Help_B.Margin = new Thickness(Voice_Add_Dir_Help_B.Margin.Left, 5, 0, 0);
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
            Page_Back(true);
        }
        private void Page_Back(bool IsAddBack)
        {
            //音声リスト1へ移動
            Pause_Volume_Animation(true, 5);
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
                Voice_File_Reset(Sound_Setting[0], Voice_List.SelectedIndex);
            else if (List_Index_Mode == 1 && Voice_Sub_List.SelectedIndex != -1)
                Voice_File_Reset(Sound_Setting[1], Voice_Sub_List.SelectedIndex);
            else
                Voice_File_List.Items.Clear();
            if (IsAddBack)
                Add_Back("Page_Back");
        }
        private void Voice_Next_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy || IsCreating)
                return;
            Page_Next(true);
        }
        private void Page_Next(bool IsAddBack)
        {
            //音声リスト2へ移動
            Pause_Volume_Animation(true, 5);
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
                Voice_File_Reset(Sound_Setting[1], Voice_Sub_List.SelectedIndex);
            else if (List_Index_Mode == 2 && Voice_Three_List.SelectedIndex != -1)
                Voice_File_Reset(Sound_Setting[2], Voice_Three_List.SelectedIndex);
            else
                Voice_File_List.Items.Clear();
            if (IsAddBack)
                Add_Back("Page_Next");
        }
        private void Voice_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsBusy || IsCreating)
                return;
            Pause_Volume_Animation(true, 5);
            if (Voice_List.SelectedIndex != -1)
                Voice_File_Reset(Sound_Setting[0], Voice_List.SelectedIndex);
        }
        private void Voice_Sub_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsBusy || IsCreating)
                return;
            Pause_Volume_Animation(true, 5);
            if (Voice_Sub_List.SelectedIndex != -1)
                Voice_File_Reset(Sound_Setting[1], Voice_Sub_List.SelectedIndex);
        }
        private void Voice_Three_List_SeletionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsBusy || IsCreating)
                return;
            Pause_Volume_Animation(true, 5);
            if (Voice_Three_List.SelectedIndex != -1)
                Voice_File_Reset(Sound_Setting[2], Voice_Three_List.SelectedIndex);
        }
        void Voice_File_Reset(List<Voice_Event_Setting> List, int SelectIndex)
        {
            //選択されているタイプの音声を取得してリストに追加
            Voice_File_List.Items.Clear();
            Voice_Event_Setting Files = List[SelectIndex];
            if (Files.Sounds.Count > 0)
            {
                foreach (Voice_Sound_Setting Temp in Files.Sounds)
                {
                    ListBoxItem LBI = new ListBoxItem();
                    LBI.Content = Path.GetFileName(Temp.File_Path);
                    LBI.Foreground = Brushes.Aqua;
                    System.Windows.Controls.ContextMenu pMenu = new System.Windows.Controls.ContextMenu();
                    System.Windows.Controls.MenuItem item1 = new System.Windows.Controls.MenuItem();
                    item1.Header = "削除";
                    item1.Click += Voice_List_Delete_Click;
                    System.Windows.Controls.MenuItem item2 = new System.Windows.Controls.MenuItem();
                    item2.Header = "コピー";
                    item2.Click += Voice_List_Copy_Click;
                    System.Windows.Controls.MenuItem item3 = new System.Windows.Controls.MenuItem();
                    item3.Header = "貼り付け";
                    item3.Click += Voice_List_Paste_Click;
                    pMenu.Items.Add(item1);
                    pMenu.Items.Add(item2);
                    pMenu.Items.Add(item3);
                    LBI.ContextMenu = pMenu;
                    LBI.PreviewMouseRightButtonDown += LBI_MouseRightButtonDown;
                    Voice_File_List.Items.Add(LBI);
                }
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
        private void Add_Voice(List<Add_Voice_Param> Add_Files)
        {
            if (IsBusy || IsCreating)
                return;
            List<Voice_Sound_Setting> Voice_Settings = new List<Voice_Sound_Setting>();
            List<int> Change_Colors_Main = new List<int>();
            List<int> Change_Colors_Sub = new List<int>();
            foreach (Add_Voice_Param Files in Add_Files)
            {
                Voice_Settings.Add(new Voice_Sound_Setting(Files.File_Path));
                Voice_Settings[Voice_Settings.Count - 1].Type_Index = Files.Type_Index;
                Voice_Settings[Voice_Settings.Count - 1].Voice_Index = Files.Voice_Index;
                Voice_Settings[Voice_Settings.Count - 1].File_Index = Sound_Setting[Files.Type_Index][Files.Voice_Index].Sounds.Count;
                Sound_Setting[Files.Type_Index][Files.Voice_Index].Sounds.Add(Voice_Settings[Voice_Settings.Count - 1]);
                if (Files.Type_Index == 0 && !Change_Colors_Main.Contains(Files.Voice_Index))
                    Change_Colors_Main.Add(Files.Voice_Index);
                else if (Files.Type_Index == 1 && !Change_Colors_Sub.Contains(Files.Voice_Index))
                    Change_Colors_Sub.Add(Files.Voice_Index);
            }
            foreach (int Index in Change_Colors_Main)
            {
                Main_Voice_List[Index] = Main_Voice_List[Index].Replace("未選択", "選択済み");
                Voice_List.Items[Index] = Main_Voice_List[Index];
                ListBoxItem LBI = new ListBoxItem();
                LBI.Content = Main_Voice_List[Index];
                Voice_List.Items[Index] = LBI;
                Voice_List.SelectedIndex = Index;
            }
            foreach (int Index in Change_Colors_Sub)
            {
                Sub_Voice_List[Index] = Sub_Voice_List[Index].Replace("未選択", "選択済み");
                Voice_Sub_List.Items[Index] = Sub_Voice_List[Index];
                ListBoxItem LBI = new ListBoxItem();
                LBI.Content = Sub_Voice_List[Index];
                Voice_Sub_List.Items[Index] = LBI;
                Voice_Sub_List.SelectedIndex = Index;
            }
            Change_Colors_Main.Clear();
            Change_Colors_Sub.Clear();
            Add_Back(Voice_Settings);
            Voice_List.SelectedIndex = -1;
            Voice_Sub_List.SelectedIndex = -1;
            Voice_Three_List.SelectedIndex = -1;
            Voice_File_List.Items.Clear();
        }
        public void Add_Voice(string[] Voice_Files, bool IsDragDropMode, Voice_Sound_Setting Settings = null, int List_Type_Index = -1, int Voice_Index = -1, int File_Index = -1)
        {
            if (IsBusy || IsCreating)
                return;
            if (List_Type_Index == -1)
                List_Type_Index = List_Index_Mode;
            if (IsDragDropMode)
            {
                if (Voice_List.SelectedIndex == -1 && List_Type_Index == 0)
                {
                    Message_Feed_Out("音声タイプが選択されていません。");
                    return;
                }
                else if (Voice_Sub_List.SelectedIndex == -1 && List_Type_Index == 1)
                {
                    Message_Feed_Out("音声タイプが選択されていません。");
                    return;
                }
                else if (Voice_Three_List.SelectedIndex == -1 && List_Type_Index == 2)
                {
                    Message_Feed_Out("音声タイプが選択されていません。");
                    return;
                }
            }
            int Insert_Index = File_Index;
            if (Insert_Index == -1)
                Insert_Index = Voice_File_List.Items.Count;
            //選択している音声の種類に音声ファイルを追加
            int IndexNumber = Voice_Index;
            List<Voice_Sound_Setting> Voice_Settings = new List<Voice_Sound_Setting>();
            if (IndexNumber == -1)
            {
                if (List_Type_Index == 0)
                    IndexNumber = Voice_List.SelectedIndex;
                else if (List_Type_Index == 1)
                    IndexNumber = Voice_Sub_List.SelectedIndex;
                else if (List_Type_Index == 2)
                    IndexNumber = Voice_Three_List.SelectedIndex;
                foreach (string SelectFile in Voice_Files)
                {
                    Voice_Settings.Add(new Voice_Sound_Setting(SelectFile));
                    Voice_Settings[Voice_Settings.Count - 1].Type_Index = List_Type_Index;
                    Voice_Settings[Voice_Settings.Count - 1].Voice_Index = IndexNumber;
                    Voice_Settings[Voice_Settings.Count - 1].File_Index = Insert_Index;
                    Sound_Setting[List_Type_Index][IndexNumber].Sounds.Insert(Insert_Index, Voice_Settings[Voice_Settings.Count - 1]);
                }
            }
            //音声を追加しそのタイプを選択済みにする
            else if (Settings != null)
            {
                Voice_Settings.Add(Settings);
                Voice_Settings[Voice_Settings.Count - 1].Type_Index = List_Type_Index;
                Voice_Settings[Voice_Settings.Count - 1].Voice_Index = IndexNumber;
                Voice_Settings[Voice_Settings.Count - 1].File_Index = Insert_Index;
                Sound_Setting[List_Type_Index][IndexNumber].Sounds.Insert(Insert_Index, Settings);
            }
            else
            {
                foreach (string SelectFile in Voice_Files)
                {
                    Voice_Settings.Add(new Voice_Sound_Setting(SelectFile));
                    Voice_Settings[Voice_Settings.Count - 1].Type_Index = List_Type_Index;
                    Voice_Settings[Voice_Settings.Count - 1].Voice_Index = IndexNumber;
                    Voice_Settings[Voice_Settings.Count - 1].File_Index = Sound_Setting[List_Type_Index][IndexNumber].Sounds.Count;
                    Sound_Setting[List_Type_Index][IndexNumber].Sounds.Add(Voice_Settings[Voice_Settings.Count - 1]);
                }
            }
            Voice_File_Reset(Sound_Setting[List_Type_Index], IndexNumber);
            if (List_Type_Index == 0)
            {
                Main_Voice_List[IndexNumber] = Main_Voice_List[IndexNumber].Replace("未選択", "選択済み");
                Voice_List.Items[IndexNumber] = Main_Voice_List[IndexNumber];
                ListBoxItem LBI = new ListBoxItem();
                LBI.Content = Main_Voice_List[IndexNumber];
                Voice_List.Items[IndexNumber] = LBI;
                Voice_List.SelectedIndex = IndexNumber;
            }
            else if (List_Type_Index == 1)
            {
                Sub_Voice_List[IndexNumber] = Sub_Voice_List[IndexNumber].Replace("未選択", "選択済み");
                Voice_Sub_List.Items[IndexNumber] = Sub_Voice_List[IndexNumber];
                ListBoxItem LBI = new ListBoxItem();
                LBI.Content = Sub_Voice_List[IndexNumber];
                Voice_Sub_List.Items[IndexNumber] = LBI;
                Voice_Sub_List.SelectedIndex = IndexNumber;
            }
            else if (List_Type_Index == 2)
            {
                Three_Voice_List[IndexNumber] = Three_Voice_List[IndexNumber].Replace("未選択", "選択済み");
                Voice_Three_List.Items[IndexNumber] = Three_Voice_List[IndexNumber];
                ListBoxItem LBI = new ListBoxItem();
                LBI.Content = Three_Voice_List[IndexNumber];
                Voice_Three_List.Items[IndexNumber] = LBI;
                Voice_Three_List.SelectedIndex = IndexNumber;
            }
            if (Voice_Index == -1)
                Add_Back(Voice_Settings);
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
                int Voice_Index = Voice_List.SelectedIndex;
                Sound_Setting[List_Index_Mode][Voice_Index].Sounds[Number].Type_Index = List_Index_Mode;
                Sound_Setting[List_Index_Mode][Voice_Index].Sounds[Number].Voice_Index = Voice_Index;
                Sound_Setting[List_Index_Mode][Voice_Index].Sounds[Number].File_Index = Number;
                Add_Back(Sound_Setting[List_Index_Mode][Voice_Index].Sounds[Number].Clone());
                Voice_Delete(List_Index_Mode, Voice_Index, Number);
            }
            else if (List_Index_Mode == 1)
            {
                int Voice_Index = Voice_Sub_List.SelectedIndex;
                Sound_Setting[List_Index_Mode][Voice_Index].Sounds[Number].Type_Index = List_Index_Mode;
                Sound_Setting[List_Index_Mode][Voice_Index].Sounds[Number].Voice_Index = Voice_Index;
                Sound_Setting[List_Index_Mode][Voice_Index].Sounds[Number].File_Index = Number;
                Add_Back(Sound_Setting[List_Index_Mode][Voice_Index].Sounds[Number].Clone());
                Voice_Delete(List_Index_Mode, Voice_Index, Number);
            }
            else if (List_Index_Mode == 2)
            {
                int Voice_Index = Voice_Three_List.SelectedIndex;
                Sound_Setting[List_Index_Mode][Voice_Index].Sounds[Number].Type_Index = List_Index_Mode;
                Sound_Setting[List_Index_Mode][Voice_Index].Sounds[Number].Voice_Index = Voice_Index;
                Sound_Setting[List_Index_Mode][Voice_Index].Sounds[Number].File_Index = Number;
                Add_Back(Sound_Setting[List_Index_Mode][Voice_Index].Sounds[Number].Clone());
                Voice_Delete(List_Index_Mode, Voice_Index, Number);
            }
            if (Voice_File_List.Items.Count > Number)
                Voice_File_List.SelectedIndex = Number;
        }
        private void Voice_Delete(int Type_Index, int Voice_Index, int File_Index)
        {
            ListBoxItem LBI = new ListBoxItem();
            Sound_Setting[Type_Index][Voice_Index].Sounds.RemoveAt(File_Index);
            int Before_Voice_Index = -1;
            if (Type_Index == 0)
            {
                Before_Voice_Index = Voice_List.SelectedIndex;
                if (Sound_Setting[Type_Index][Voice_Index].Sounds.Count == 0)
                {
                    Main_Voice_List[Voice_Index] = Main_Voice_List[Voice_Index].Replace("選択済み", "未選択");
                    Voice_List.Items[Voice_Index] = Main_Voice_List[Voice_Index];
                    LBI.Content = Main_Voice_List[Voice_Index];
                    LBI.Foreground = (Brush)new BrushConverter().ConvertFromString("#BFFF2C8C");
                    Voice_List.Items[Voice_Index] = LBI;
                }
                Voice_List.SelectedIndex = Voice_Index;
            }
            else if (Type_Index == 1)
            {
                Before_Voice_Index = Voice_Sub_List.SelectedIndex;
                if (Sound_Setting[Type_Index][Voice_Index].Sounds.Count == 0)
                {
                    Sub_Voice_List[Voice_Index] = Sub_Voice_List[Voice_Index].Replace("選択済み", "未選択");
                    Voice_Sub_List.Items[Voice_Index] = Sub_Voice_List[Voice_Index];
                    LBI.Content = Sub_Voice_List[Voice_Index].Replace("System.Windows.Controls.ListBoxItem: ", "");
                    LBI.Foreground = (Brush)new BrushConverter().ConvertFromString("#BFFF2C8C");
                    Voice_Sub_List.Items[Voice_Index] = LBI;
                }
                Voice_Sub_List.SelectedIndex = Voice_Index;
            }
            else if (Type_Index == 2)
            {
                Before_Voice_Index = Voice_Three_List.SelectedIndex;
                if (Sound_Setting[Type_Index][Voice_Index].Sounds.Count == 0)
                {
                    Three_Voice_List[Voice_Index] = Three_Voice_List[Voice_Index].Replace("選択済み", "未選択");
                    Voice_Three_List.Items[Voice_Index] = Three_Voice_List[Voice_Index];
                    LBI.Content = Sub_Voice_List[Voice_Index].Replace("System.Windows.Controls.ListBoxItem: ", "");
                    LBI.Foreground = (Brush)new BrushConverter().ConvertFromString("#BFFF2C8C");
                    Voice_Three_List.Items[Voice_Index] = LBI;
                }
                Voice_Three_List.SelectedIndex = Voice_Index;
            }
            if (List_Index_Mode == Type_Index && Before_Voice_Index == Voice_Index && Voice_File_List.Items.Count > File_Index)
                Voice_File_List.Items.RemoveAt(File_Index);
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
                int Voice_List_Index = Voice_List.SelectedIndex;
                if (List_Index_Mode == 1)
                    Voice_List_Index = Voice_Sub_List.SelectedIndex;
                else if (List_Index_Mode == 2)
                    Voice_List_Index = Voice_Three_List.SelectedIndex;
                string Select_File = "";
                if (Sound_Setting[List_Index_Mode][Voice_List_Index].Sounds[Voice_File_List.SelectedIndex].File_Path.Contains("\\"))
                {
                    Select_File = Sound_Setting[List_Index_Mode][Voice_List_Index].Sounds[Voice_File_List.SelectedIndex].File_Path;
                    if (!File.Exists(Select_File))
                    {
                        Message_Feed_Out("音声ファイルが存在しません。削除された可能性があります。");
                        return;
                    }
                }
                Bass.BASS_ChannelStop(Stream);
                Bass.BASS_StreamFree(Stream);
                Position_S.Value = 0;
                int StreamHandle;
                if (Select_File != "")
                    StreamHandle = Bass.BASS_StreamCreateFile(Select_File, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_LOOP);
                else
                {
                    File.Delete(Voice_Set.Special_Path + "\\Wwise\\Temp_Voice_Create_01.mp3");
                    File.WriteAllBytes(Voice_Set.Special_Path + "\\Wwise\\Temp_Voice_Create_01.mp3", WVS_File.Load_Sound(Sound_Setting[List_Index_Mode][Voice_List_Index].Sounds[Voice_File_List.SelectedIndex].Stream_Position));
                    StreamHandle = Bass.BASS_StreamCreateFile(Voice_Set.Special_Path + "\\Wwise\\Temp_Voice_Create_01.mp3", 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_LOOP);
                }
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
        private async void Voice_Save_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy || IsCreating)
                return;
            bool IsOK = false;
            foreach (List<Voice_Event_Setting> Settings in Sound_Setting)
            {
                foreach (Voice_Event_Setting Setting in Settings)
                {
                    if (Setting.Sounds.Count > 0)
                    {
                        IsOK = true;
                        break;
                    }
                }
                if (IsOK)
                    break;
            }
            if (!IsOK)
            {
                Message_Feed_Out("少なくとも1つはサウンドを追加する必要があります。");
                return;
            }
            SaveFileDialog sfd = new SaveFileDialog()
            {
                Title = "保存先を指定してください。",
                Filter = "セーブファイル(*.wvs)|*.wvs",
            };
            if (Save_Load_Dir == "")
                sfd.InitialDirectory = Directory.GetCurrentDirectory();
            else
                sfd.InitialDirectory = Save_Load_Dir;
            //現在の状態をファイルに保存する
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                Keyboard.ClearFocus();
                Save_Load_Dir = Path.GetDirectoryName(sfd.FileName);
                Configs_Save();
                try
                {
                    if (IsIncludeSound)
                        Message_T.Text = "サウンドファイルを含めたセーブデータを作成しています...";
                    else
                        Message_T.Text = "セーブしています...";
                    await Task.Delay(50);
                    WVS_Save Save = new WVS_Save();
                    Save.Add_Sound(Sound_Setting, WVS_File);
                    if (WVS_File != null)
                        WVS_File.Dispose();
                    Save.Create(sfd.FileName, Project_Name_T.Text, !Project_Name_T.IsEnabled, IsIncludeSound);
                    Save.Dispose();
                    Voice_Load_From_File(sfd.FileName);
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
            };
            if (Save_Load_Dir == "")
                ofd.InitialDirectory = Directory.GetCurrentDirectory();
            else
                ofd.InitialDirectory = Save_Load_Dir;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Save_Load_Dir = Path.GetDirectoryName(ofd.FileName);
                Configs_Save();
                Voice_Load_From_File(ofd.FileName);
            }
            ofd.Dispose();
        }
        public void Voice_Load_From_File(string WVS_File)
        {
            try
            {
                //音声を配置
                Back_To_The_Future.Clear();
                Copy_Voices.Clear();
                Back_Pos_Now = 0;
                List_Text_Reset();
                Bass.BASS_ChannelStop(Stream);
                Bass.BASS_StreamFree(Stream);
                if (this.WVS_File != null && this.WVS_File.IsLoaded)
                    this.WVS_File.Dispose();
                WVS_Load.WVS_Result Result = WVS_Load.IsBlitzWVSFile(WVS_File);
                if (Result == WVS_Load.WVS_Result.OK)
                {
                    this.WVS_File = new WVS_Load();
                    this.WVS_File.WVS_Load_File(WVS_File, Sound_Setting);
                    Project_Name_T.Text = this.WVS_File.Project_Name;
                    if (this.WVS_File.IsNotChangeNameMode)
                        Project_Name_Text.Text = "プロジェクト名(変更できません)";
                    else
                        Project_Name_Text.Text = "プロジェクト名";
                    Project_Name_T.IsEnabled = !this.WVS_File.IsNotChangeNameMode;
                    for (int Number = 0; Number < Sound_Setting[0].Count; Number++)
                    {
                        if (Sound_Setting[0][Number].Sounds.Count > 0)
                        {
                            Main_Voice_List[Number] = Main_Voice_List[Number].Replace("未選択", "選択済み");
                            Voice_List.Items[Number] = Main_Voice_List[Number];
                        }
                    }
                    for (int Number = 0; Number < Sound_Setting[1].Count; Number++)
                    {
                        if (Sound_Setting[1][Number].Sounds.Count > 0)
                        {
                            Sub_Voice_List[Number] = Sub_Voice_List[Number].Replace("未選択", "選択済み");
                            Voice_Sub_List.Items[Number] = Sub_Voice_List[Number];
                        }
                    }
                    for (int Number = 0; Number < Sound_Setting[2].Count; Number++)
                    {
                        if (Sound_Setting[2][Number].Sounds.Count > 0)
                        {
                            Three_Voice_List[Number] = Three_Voice_List[Number].Replace("未選択", "選択済み");
                            Voice_Three_List.Items[Number] = Three_Voice_List[Number];
                        }
                    }
                    IsLoadedWVS = true;
                    IsWVSIncludeSound = this.WVS_File.IsIncludedSound;
                    IsIncludeSound = this.WVS_File.IsIncludedSound;
                    if (IsWVSIncludeSound)
                        Voice_Save_WVS_C.Source = Sub_Code.Check_03;
                    else
                        Voice_Save_WVS_C.Source = Sub_Code.Check_01;
                }
                else if (Result == WVS_Load.WVS_Result.WoTMode)
                {
                    if (this.WVS_File != null)
                        this.WVS_File.Dispose();
                    this.WVS_File = null;
                    List_Text_Reset();
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
                    IsLoadedWVS = false;
                    IsWVSIncludeSound = false;
                    if (Voice_List.SelectedIndex != -1)
                        Voice_File_Reset(Sound_Setting[0], Voice_List.SelectedIndex);
                    else
                        Voice_File_List.Items.Clear();
                    Message_Feed_Out("指定したファイルは本家WoT用のセーブデータです。");
                }
                else
                {
                    IsLoadedWVS = false;
                    IsWVSIncludeSound = false;
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
                    while ((line = file.ReadLine()) != null)
                    {
                        int Number = int.Parse(line.Substring(0, line.IndexOf('|')));
                        string File_Path = line.Substring(line.IndexOf('|') + 1);
                        if (Number < 34)
                        {
                            Sound_Setting[0][Number].Sounds.Add(new Voice_Sound_Setting(File_Path));
                            Main_Voice_List[Number] = Main_Voice_List[Number].Replace("未選択", "選択済み");
                            Voice_List.Items[Number] = Main_Voice_List[Number];
                        }
                        else if (Number < 50)
                        {
                            Sound_Setting[1][Number - 34].Sounds.Add(new Voice_Sound_Setting(File_Path));
                            Sub_Voice_List[Number - 34] = Sub_Voice_List[Number - 34].Replace("未選択", "選択済み");
                            Voice_Sub_List.Items[Number - 34] = Sub_Voice_List[Number - 34];
                        }
                        else
                        {
                            Sound_Setting[2][Number - 50].Sounds.Add(new Voice_Sound_Setting(File_Path));
                            Three_Voice_List[Number - 50] = Three_Voice_List[Number - 50].Replace("未選択", "選択済み");
                            Voice_Three_List.Items[Number - 50] = Three_Voice_List[Number - 50];
                        }
                    }
                    file.Close();
                    Sub_Code.Set_Event_ShortID(Sound_Setting);
                }
                if (List_Index_Mode == 0 && Voice_List.SelectedIndex != -1)
                    Voice_File_Reset(Sound_Setting[0], Voice_List.SelectedIndex);
                else if (List_Index_Mode == 1 && Voice_Sub_List.SelectedIndex != -1)
                    Voice_File_Reset(Sound_Setting[1], Voice_Sub_List.SelectedIndex);
                else if (List_Index_Mode == 2 && Voice_Three_List.SelectedIndex != -1)
                    Voice_File_Reset(Sound_Setting[2], Voice_Three_List.SelectedIndex);
                ColorMode_Change();
                Back_Pos_Now = 0;
                Back_To_The_Future.Clear();
                Copy_Voices.Clear();
                Project_Name_T.UndoLimit = 0;
                Keyboard.ClearFocus();
                Project_Name_T.UndoLimit = 15;
                Message_Feed_Out("ロードしました。");
            }
            catch (Exception e1)
            {
                if (this.WVS_File != null)
                    this.WVS_File.Dispose();
                this.WVS_File = null;
                List_Text_Reset();
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
                IsLoadedWVS = false;
                IsWVSIncludeSound = false;
                if (Voice_List.SelectedIndex != -1)
                    Voice_File_Reset(Sound_Setting[0], Voice_List.SelectedIndex);
                else
                    Voice_File_List.Items.Clear();
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
            Sub_Code.IsForceMusicStop = true;
            Pause_Volume_Animation(false, 15);
            //作成画面へ
            List<Voice_Event_Setting> Temp = new List<Voice_Event_Setting>();
            for (int Number_01 = 0; Number_01 < Sound_Setting.Count; Number_01++)
                for (int Number_02 = 0; Number_02 < Sound_Setting[Number_01].Count; Number_02++)
                    Temp.Add(Sound_Setting[Number_01][Number_02]);
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
                    Flash.Flash_Start();
                    if (Voice_Set.WoTB_Path != "" && !Sub_Code.Only_Wwise_Project)
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
                                Sub_Code.DVPL_File_Delete(Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_battle_basic.bnk");
                                Sub_Code.DVPL_File_Copy(Dir_Name + "/voiceover_crew.bnk", Voice_Set.WoTB_Path + "/Data/WwiseSound/" + Sub_Code.SetLanguage + "/voiceover_crew.bnk", true);
                                Sub_Code.DVPL_File_Copy(Dir_Name + "/reload.bnk", Voice_Set.WoTB_Path + "/Data/WwiseSound/reload.bnk", true);
                                Sub_Code.DVPL_File_Copy(Dir_Name + "/ui_chat_quick_commands.bnk", Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_chat_quick_commands.bnk", true);
                                Sub_Code.DVPL_File_Copy(Dir_Name + "/ui_battle.bnk", Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_battle.bnk", true);
                                Sub_Code.DVPL_File_Copy(Dir_Name + "/ui_battle_basic.bnk", Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_battle_basic.bnk", true);
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
                    Message_Feed_Out("致命的なエラーが発生しました。");
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
                if (Save_Load_Dir != "")
                    stw.Write(Save_Load_Dir);
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
            //項目に音声が入っていないかつ、設定画面のチェックを入れている場合、標準の音声を再生させるように
            if (Sub_Code.Default_Voice)
                for (int Number = 0; Number < Sound_Setting[0].Count; Number++)
                    if (Sound_Setting[0][Number].Sounds.Count == 0)
                        foreach (string Name in Sub_Code.Get_Files_By_Name(Voice_Set.Special_Path + "\\SE\\Voices", Sub_Code.Default_Name[Number]))
                            Sound_Setting[0][Number].Sounds.Add(new Voice_Sound_Setting(Name));
            Directory.CreateDirectory(Dir_Name);
            Message_T.Text = "プロジェクトファイルを作成しています...";
            await Task.Delay(50);
            foreach (string dir in Directory.GetDirectories(Dir_Name))
                Directory.Delete(dir, true);
            foreach (string file in Directory.GetFiles(Dir_Name))
            {
                string Ex = Path.GetExtension(file);
                if (Ex != ".bnk" && Ex != ".dvpl")
                    File.Delete(file);
            }
            File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.tmp", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.wwu", true);
            File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Events/base_capture.tmp", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Events/base_capture.wwu", true);
            File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Events/Chat.tmp", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Events/Chat.wwu", true);
            File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Events/Crew.tmp", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Events/Crew.wwu", true);
            File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Events/Quick_Commands.tmp", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Events/Quick_Commands.wwu", true);
            File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Events/Reload.tmp", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Events/Reload.wwu", true);
            File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Events/UI_Battle.tmp", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Events/UI_Battle.wwu", true);
            File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Events/UI_Battle_Basic.tmp", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Events/UI_Battle_Basic.wwu", true);
            Wwise_Class.Wwise_Project_Create Wwise = new Wwise_Class.Wwise_Project_Create(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod");
            await Task.Delay(50);
            Message_T.Text = "設定をプロジェクトに反映しています...";
            Wwise.Clear_All_Sounds(Sound_Setting);
            Wwise.Sound_Add_Wwise(Sound_Setting, WVS_File, Voice_Create_Window.SE_Change_Window.Preset_List[Voice_Create_Window.SE_Change_Window.Preset_Index], Voice_Create_Window.SE_Change_Window.Default_SE);
            Message_T.Text = "サウンドファイルをwavに変換しています...";
            await Task.Delay(50);
            await Wwise.Encode_WAV();
            await Task.Delay(50);
            if (Sub_Code.VolumeSet)
            {
                Message_T.Text = "音量を調整しています...";
                await Task.Delay(50);
                Wwise.Set_Volume();
            }
            Wwise.Save();
            if (Sub_Code.Only_Wwise_Project)
            {
                Message_T.Text = "プロジェクトファイルをコピーしています...\nこれには時間がかかる場合があります。";
                await Task.Delay(50);
                Sub_Code.Directory_Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod", Dir_Name, false);
            }
            else
            {
                if (Sound_Setting[1][Sound_Setting[1].Count - 1].Sounds.Count > 0)
                    Message_T.Text = ".bnkファイルを作成しています...\nBGMファイルが含まれているため時間がかかります。";
                else
                    Message_T.Text = ".bnkファイルを作成しています...";
                await Task.Delay(50);
                Wwise.Project_Build("ui_battle", Dir_Name + "/ui_battle.bnk");
                await Task.Delay(50);
                Wwise.Project_Build("ui_battle_basic", Dir_Name + "/ui_battle_basic.bnk");
                await Task.Delay(50);
                Wwise.Project_Build("ui_chat_quick_commands", Dir_Name + "/ui_chat_quick_commands.bnk");
                await Task.Delay(50);
                Wwise.Project_Build("reload", Dir_Name + "/reload.bnk");
                await Task.Delay(50);
                Wwise.Project_Build("voiceover_crew", Dir_Name + "/voiceover_crew.bnk");
                Wwise.Event_Reset();
            }
            await Task.Delay(50);
            Wwise.Clear();
            File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.tmp", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.wwu", true);
            File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Events/base_capture.tmp", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Events/base_capture.wwu", true);
            File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Events/Chat.tmp", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Events/Chat.wwu", true);
            File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Events/Crew.tmp", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Events/Crew.wwu", true);
            File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Events/Quick_Commands.tmp", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Events/Quick_Commands.wwu", true);
            File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Events/Reload.tmp", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Events/Reload.wwu", true);
            File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Events/UI_Battle.tmp", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Events/UI_Battle.wwu", true);
            File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Events/UI_Battle_Basic.tmp", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Events/UI_Battle_Basic.wwu", true);
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
                IsLoadedWVS = false;
                IsWVSIncludeSound = false;
                if (WVS_File != null && WVS_File.IsLoaded)
                    WVS_File.Dispose();
                if (Voice_List.SelectedIndex != -1)
                    Voice_File_Reset(Sound_Setting[0], Voice_List.SelectedIndex);
                else
                    Voice_File_List.Items.Clear();
                Back_To_The_Future.Clear();
                Copy_Voices.Clear();
                Back_Pos_Now = 0;
                Project_Name_T.UndoLimit = 0;
                Message_Feed_Out("内容をクリアしました。");
                Project_Name_T.UndoLimit = 15;
            }
        }
        void ColorMode_Change()
        {
            Brush br = (Brush)new BrushConverter().ConvertFromString("#BFFF2C8C");
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
            Voice_Save_WVS_C.Source = Sub_Code.Check_01;
            Execute_WoTB_C.Source = Sub_Code.Check_01;
            if (File.Exists(Voice_Set.Special_Path + "/Configs/Voice_Create.conf"))
            {
                try
                {
                    StreamReader str = Sub_Code.File_Decrypt_To_Stream(Voice_Set.Special_Path + "/Configs/Voice_Create.conf", "Voice_Create_Configs_Save");
                    Volume_S.Value = double.Parse(str.ReadLine());
                    try
                    {
                        Save_Load_Dir = str.ReadLine();
                    }
                    catch { }
                    str.Close();
                }
                catch
                {
                }
            }
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
        private void Voice_Save_WVS_C_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (IsIncludeSound)
            {
                if (IsLoadedWVS && IsWVSIncludeSound)
                {
                    Message_Feed_Out("既にサウンドファイルを含めた.wvsファイルとして保存しているため、チェックを外すことはできません。");
                    return;
                }
                IsIncludeSound = false;
                Voice_Save_WVS_C.Source = Sub_Code.Check_02;
            }
            else
            {
                IsIncludeSound = true;
                Voice_Save_WVS_C.Source = Sub_Code.Check_04;
            }
        }
        private void Voice_Save_WVS_C_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsIncludeSound)
                Voice_Save_WVS_C.Source = Sub_Code.Check_04;
            else
                Voice_Save_WVS_C.Source = Sub_Code.Check_02;
        }
        private void Voice_Save_WVS_C_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsIncludeSound)
                Voice_Save_WVS_C.Source = Sub_Code.Check_03;
            else
                Voice_Save_WVS_C.Source = Sub_Code.Check_01;
        }
        private void Voice_Setting_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy || IsCreating)
                return;
            if (List_Index_Mode == 2)
                Message_Feed_Out("音声リスト3ではイベントの編集を行うことができません。");
            else
            {
                string Event_Name = "";
                int Index = -1;
                string Name = "";
                if (List_Index_Mode == 0 && Voice_List.SelectedIndex != -1)
                {
                    Index = Voice_List.SelectedIndex;
                    Name = Main_Voice_List[Voice_List.SelectedIndex];
                }
                else if (List_Index_Mode == 1 && Voice_Sub_List.SelectedIndex != -1)
                {
                    Index = Voice_Sub_List.SelectedIndex;
                    Name = Sub_Voice_List[Voice_Sub_List.SelectedIndex];
                }
                else if (List_Index_Mode == 2 && Voice_Three_List.SelectedIndex != -1)
                {
                    Index = Voice_Three_List.SelectedIndex;
                    Name = Three_Voice_List[Voice_Three_List.SelectedIndex];
                }
                else
                {
                    Message_Feed_Out("イベント名を選択してください。");
                    return;
                }
                Event_Name = Name.Substring(0, Name.IndexOf('|') - 1);
                if (Sound_Setting[List_Index_Mode][Index].Sounds.Count == 0)
                {
                    Message_Feed_Out("イベント内にサウンドが含まれていません。");
                    return;
                }
                Voice_Pause_B_Click(null, null);
                Voice_Event_Setting Settings = Sound_Setting[List_Index_Mode][Index];
                Voice_Create_Setting_Window.Window_Show(Event_Name, Settings, WVS_File, List_Index_Mode, Index);
            }
        }
        public void Add_Back(object Value)
        {
            if (Back_To_The_Future.Count - Back_Pos_Now > 0)
                Back_To_The_Future.RemoveRange(Back_Pos_Now, Back_To_The_Future.Count - Back_Pos_Now);
            Back_To_The_Future.Add(Value);
            Back_Pos_Now++;
        }
        private void History_Back()
        {
            if (Back_Pos_Now <= 0)
            {
                Message_Feed_Out("これ以上戻ることはできません。");
                return;
            }
            Back_Pos_Now--;
            object Value = Back_To_The_Future[Back_Pos_Now];
            if (Value is Voice_Sound_Setting)
            {
                Pause_Volume_Animation(true, 5);
                Voice_Sound_Setting Settings = (Voice_Sound_Setting)Value;
                Add_Voice(new[] { Settings.File_Path }, false, Settings, Settings.Type_Index, Settings.Voice_Index, Settings.File_Index);
            }
            else if (Value is List<Voice_Sound_Setting>)
            {
                Pause_Volume_Animation(true, 5);
                List<Voice_Sound_Setting> Settings = (List<Voice_Sound_Setting>)Value;
                if (Settings.Count > 0)
                {
                    if (Settings[0].IsNormalMode)
                        Settings = Settings.OrderByDescending(h => h.File_Index).ToList();
                    else
                        Settings = Settings.OrderBy(h => h.File_Index).ToList();
                    foreach (Voice_Sound_Setting Setting in Settings)
                    {
                        if (Setting.IsNormalMode)
                            Voice_Delete(Setting.Type_Index, Setting.Voice_Index, Setting.File_Index);
                        else
                            Add_Voice(new[] { Setting.File_Path }, false, Setting, Setting.Type_Index, Setting.Voice_Index, Setting.File_Index);
                    }
                }
            }
            else if (Value is string)
            {
                string str = (string)Value;
                if (str.Contains("File_Add:"))
                {
                    string Main = str.Substring(str.IndexOf(':') + 1);
                    string[] args = Main.Split(',');
                    for (int Number = 0; Number < int.Parse(args[2]); Number++)
                    {
                        int Type_Index = int.Parse(args[0]);
                        int Voice_Index = int.Parse(args[1]);
                        int File_Index = Sound_Setting[Type_Index][Voice_Index].Sounds.Count - 1;
                        Voice_Delete(Type_Index, Voice_Index, File_Index);
                    }
                }
                else if (str.Contains("Page_Back"))
                    Page_Next(false);
                else if (str.Contains("Page_Next"))
                    Page_Back(false);
            }
        }
        private void History_Next()
        {
            if (Back_Pos_Now >= Back_To_The_Future.Count)
                return;
            object Value = Back_To_The_Future[Back_Pos_Now];
            if (Value is Voice_Sound_Setting)
            {
                Voice_Sound_Setting Settings = (Voice_Sound_Setting)Value;
                Pause_Volume_Animation(true, 5);
                Voice_Delete(Settings.Type_Index, Settings.Voice_Index, Settings.File_Index);
            }
            else if (Value is List<Voice_Sound_Setting>)
            {
                List<Voice_Sound_Setting> Settings = (List<Voice_Sound_Setting>)Value;
                if (Settings.Count > 0)
                {
                    if (Settings[0].IsNormalMode)
                        Settings = Settings.OrderBy(h => h.File_Index).ToList();
                    else
                        Settings = Settings.OrderByDescending(h => h.File_Index).ToList();
                    foreach (Voice_Sound_Setting Setting in Settings)
                    {
                        Pause_Volume_Animation(true, 5);
                        if (Setting.IsNormalMode)
                            Add_Voice(new[] { Setting.File_Path }, false, Setting, Setting.Type_Index, Setting.Voice_Index, Setting.File_Index);
                        else
                            Voice_Delete(Setting.Type_Index, Setting.Voice_Index, Setting.File_Index);
                    }
                }
            }
            else if (Value is string)
            {
                string str = (string)Value;
                if (str.Contains("File_Add:"))
                {
                    string Main = str.Substring(str.IndexOf(':') + 1);
                    string[] args = Main.Split(',');
                    for (int Number = 0; Number < int.Parse(args[2]); Number++)
                    {
                        int Type_Index = int.Parse(args[0]);
                        int Voice_Index = int.Parse(args[1]);
                        Add_Voice(args[3].Split('|'), false, null, Type_Index, Voice_Index);
                    }
                }
                else if (str.Contains("Page_Back"))
                    Page_Back(false);
                else if (str.Contains("Page_Next"))
                    Page_Next(false);
            }
            Back_Pos_Now++;
        }
        private void Voice_Add_Dir_Help_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy || IsCreating)
                return;
            Message_Feed_Out("リソースフォルダ内のAdd_Voice_Setting.txtファイルをご覧ください。見ても分からなかった場合、この機能はスルーしてください。");
        }
        private void Voice_Add_Dir_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy || IsCreating)
                return;
            if (!File.Exists(Voice_Set.Special_Path + "\\Add_Voice_Setting.txt"))
            {
                Message_Feed_Out("Add_Voice_Setting.txtが存在しません。");
                return;
            }
            IsCreating = true;
            BetterFolderBrowser bfb = new BetterFolderBrowser()
            {
                Title = "音声フォルダを選択してください。",
                RootFolder = Sub_Code.Get_OpenDirectory_Path(),
                Multiselect = false,
            };
            if (bfb.ShowDialog() == DialogResult.OK)
            {
                Sub_Code.Set_Directory_Path(bfb.SelectedFolder);
                StreamReader str = new StreamReader(Voice_Set.Special_Path + "\\Add_Voice_Setting.txt");
                string[] Files = Directory.GetFiles(bfb.SelectedFolder, "*.*", SearchOption.TopDirectoryOnly);
                List<string> Main = new List<string>();
                List<string> Sub = new List<string>();
                try
                {
                    for (int Number = 0; Number < 4; Number++)
                        str.ReadLine();
                    for (int Number = 0; Number < Main_Voice_List.Count; Number++)
                    {
                        string Line = str.ReadLine();
                        Main.Add(Line.Substring(Line.IndexOf('>') + 1));
                    }
                    str.ReadLine();
                    for (int Number = 0; Number < Sub_Voice_List.Count; Number++)
                    {
                        string Line = str.ReadLine();
                        Sub.Add(Line.Substring(Line.IndexOf('>') + 1));
                    }
                }
                catch
                {
                    Message_Feed_Out("エラーが発生しました。テキストファイルが破損しています。");
                }
                str.Close();
                List<Add_Voice_Param> Params = new List<Add_Voice_Param>();
                foreach (string File_Now in Files)
                {
                    string Name_Only = Path.GetFileName(File_Now);
                    if (!Name_Only.Contains("_"))
                        continue;
                    string Name = Name_Only.Substring(0, Name_Only.LastIndexOf('_'));
                    if (Main.Contains(Name))
                    {
                        int Voice_Index = Main.IndexOf(Name);
                        if (!Sound_Setting[0][Voice_Index].Sounds.Select(h => h.File_Path).Contains(File_Now))
                            Params.Add(new Add_Voice_Param(File_Now, 0, Voice_Index));
                    }
                    else if (Sub.Contains(Name))
                    {
                        int Voice_Index = Sub.IndexOf(Name);
                        if (!Sound_Setting[1][Voice_Index].Sounds.Select(h => h.File_Path).Contains(File_Now))
                            Params.Add(new Add_Voice_Param(File_Now, 1, Sub.IndexOf(Name)));
                    }
                }
                Files = null;
                Main.Clear();
                Sub.Clear();
                IsCreating = false;
                Add_Voice(Params);
                Params.Clear();
                Message_Feed_Out("フォルダ内の音声ファイルを追加しました。");
            }
            IsCreating = false;
        }
        private void UserControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Voice_Create_Setting_Window.Visibility == Visibility.Hidden)
                Keyboard.ClearFocus();
        }
        private void Voice_File_List_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Voice_File_List.SelectedIndex = -1;
        }
        private void Voice_List_Delete_Click(object sender, RoutedEventArgs e)
        {
            List<int> Indexes = Voice_File_List.SelectedIndexes();
            if (Indexes.Count == 0)
                return;
            List<Voice_Sound_Setting> Clone_Settings = new List<Voice_Sound_Setting>();
            int Event_Index = -1;
            if (List_Index_Mode == 0)
                Event_Index = Voice_List.SelectedIndex;
            else if (List_Index_Mode == 1)
                Event_Index = Voice_Sub_List.SelectedIndex;
            else if (List_Index_Mode == 2)
                Event_Index = Voice_Three_List.SelectedIndex;
            Indexes.Sort();
            Indexes.Reverse();
            foreach (int Index in Indexes)
            {
                Voice_Sound_Setting Clone = Sound_Setting[List_Index_Mode][Event_Index].Sounds[Index].Clone();
                Clone.Type_Index = List_Index_Mode;
                Clone.File_Index = Index;
                Clone.IsNormalMode = false;
                ListBoxItem LBI = new ListBoxItem();
                Sound_Setting[List_Index_Mode][Event_Index].Sounds.RemoveAt(Index);
                int Before_Voice_Index = -1;
                if (List_Index_Mode == 0)
                {
                    Before_Voice_Index = Voice_List.SelectedIndex;
                    if (Sound_Setting[List_Index_Mode][Event_Index].Sounds.Count == 0)
                    {
                        Main_Voice_List[Event_Index] = Main_Voice_List[Event_Index].Replace("選択済み", "未選択");
                        Voice_List.Items[Event_Index] = Main_Voice_List[Event_Index];
                        LBI.Content = Main_Voice_List[Event_Index];
                        LBI.Foreground = (Brush)new BrushConverter().ConvertFromString("#BFFF2C8C");
                        Voice_List.Items[Event_Index] = LBI;
                    }
                    Voice_List.SelectedIndex = Event_Index;
                }
                else if (List_Index_Mode == 1)
                {
                    Before_Voice_Index = Voice_Sub_List.SelectedIndex;
                    if (Sound_Setting[List_Index_Mode][Event_Index].Sounds.Count == 0)
                    {
                        Sub_Voice_List[Event_Index] = Sub_Voice_List[Event_Index].Replace("選択済み", "未選択");
                        Voice_Sub_List.Items[Event_Index] = Sub_Voice_List[Event_Index];
                        LBI.Content = Sub_Voice_List[Event_Index].Replace("System.Windows.Controls.ListBoxItem: ", "");
                        LBI.Foreground = (Brush)new BrushConverter().ConvertFromString("#BFFF2C8C");
                        Voice_Sub_List.Items[Event_Index] = LBI;
                    }
                    Voice_Sub_List.SelectedIndex = Event_Index;
                }
                else if (List_Index_Mode == 2)
                {
                    Before_Voice_Index = Voice_Three_List.SelectedIndex;
                    if (Sound_Setting[List_Index_Mode][Event_Index].Sounds.Count == 0)
                    {
                        Three_Voice_List[Event_Index] = Three_Voice_List[Event_Index].Replace("選択済み", "未選択");
                        Voice_Three_List.Items[Event_Index] = Three_Voice_List[Event_Index];
                        LBI.Content = Sub_Voice_List[Event_Index].Replace("System.Windows.Controls.ListBoxItem: ", "");
                        LBI.Foreground = (Brush)new BrushConverter().ConvertFromString("#BFFF2C8C");
                        Voice_Three_List.Items[Event_Index] = LBI;
                    }
                    Voice_Three_List.SelectedIndex = Event_Index;
                }
                if (Before_Voice_Index == Event_Index && Voice_File_List.Items.Count > Index)
                    Voice_File_List.Items.RemoveAt(Index);
                Clone.Voice_Index = Before_Voice_Index;
                Clone_Settings.Add(Clone);
            }
            Add_Back(Clone_Settings);
        }
        private void Voice_List_Copy_Click(object sender, RoutedEventArgs e)
        {
            List<int> Indexes = Voice_File_List.SelectedIndexes();
            if (Indexes.Count == 0)
                return;
            int Event_Index = -1;
            if (List_Index_Mode == 0)
                Event_Index = Voice_List.SelectedIndex;
            else if (List_Index_Mode == 1)
                Event_Index = Voice_Sub_List.SelectedIndex;
            else if (List_Index_Mode == 2)
                Event_Index = Voice_Three_List.SelectedIndex;
            Indexes.Sort();
            Copy_Voices.Clear();
            foreach (int Index in Indexes)
            {
                Voice_Sound_Setting Clone = Sound_Setting[List_Index_Mode][Event_Index].Sounds[Index].Clone();
                Clone.Type_Index = -1;
                Clone.Voice_Index = -1;
                Clone.File_Index = -1;
                Copy_Voices.Add(Clone);
            }
        }
        private void Voice_List_Paste_Click(object sender, RoutedEventArgs e)
        {
            int Event_Index = -1;
            if (List_Index_Mode == 0)
                Event_Index = Voice_List.SelectedIndex;
            else if (List_Index_Mode == 1)
                Event_Index = Voice_Sub_List.SelectedIndex;
            else if (List_Index_Mode == 2)
                Event_Index = Voice_Three_List.SelectedIndex;
            if (Event_Index != -1 && Copy_Voices.Count > 0)
            {
                List<Voice_Sound_Setting> Settings = new List<Voice_Sound_Setting>();
                foreach (Voice_Sound_Setting Setting in Copy_Voices)
                {
                    Voice_Sound_Setting Temp = Setting.Clone();
                    Temp.Type_Index = List_Index_Mode;
                    Temp.Voice_Index = Event_Index;
                    Temp.File_Index = Voice_File_List.Items.Count;
                    Temp.IsNormalMode = true;
                    Add_Voice(new string[] { Temp.File_Path }, false, Temp, List_Index_Mode, Event_Index);
                    Settings.Add(Temp);
                }
                Add_Back(Settings);
            }
        }
        private void Voice_File_List_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.ContextMenu pMenu = Voice_File_List.ContextMenu;
            System.Windows.Controls.MenuItem item1 = pMenu.Items[0] as System.Windows.Controls.MenuItem;
            int Event_Index = -1;
            if (List_Index_Mode == 0)
                Event_Index = Voice_List.SelectedIndex;
            else if (List_Index_Mode == 1)
                Event_Index = Voice_Sub_List.SelectedIndex;
            else if (List_Index_Mode == 2)
                Event_Index = Voice_Three_List.SelectedIndex;
            item1.IsEnabled = Event_Index != -1 && Copy_Voices.Count > 0;
        }
        private void LBI_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.ContextMenu pMenu = ((ListBoxItem)sender).ContextMenu;
            System.Windows.Controls.MenuItem item1 = pMenu.Items[2] as System.Windows.Controls.MenuItem;
            int Event_Index = -1;
            if (List_Index_Mode == 0)
                Event_Index = Voice_List.SelectedIndex;
            else if (List_Index_Mode == 1)
                Event_Index = Voice_Sub_List.SelectedIndex;
            else if (List_Index_Mode == 2)
                Event_Index = Voice_Three_List.SelectedIndex;
            item1.IsEnabled = Event_Index != -1 && Copy_Voices.Count > 0;
        }
    }
}