using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WoTB_Voice_Mod_Creater.Wwise_Class
{
    public partial class Wwise_Event_Player : UserControl
    {
        List<List<string>> Event_Names = new List<List<string>>();
        List<List<Name_ID_Contaier>> Event_Info = new List<List<Name_ID_Contaier>>();
        List<string> Bank_Names = new List<string>();
        List<Container_und_Volume> Window_Show_Volumes = new List<Container_und_Volume>();
        int Page = 0;
        double Location_Before = 0;
        bool IsClosing = false;
        bool IsMessageShowing = false;
        bool IsInitSelected = false;
        bool IsPaused = false;
        bool IsLocationChanging = false;
        bool IsPlayingMouseDown = false;
        bool IsZoomMode = false;
        Random r = new Random();
        public Wwise_Event_Player()
        {
            InitializeComponent();
            Page_Back_B.Visibility = Visibility.Hidden;
            Page_Next_B.Visibility = Visibility.Hidden;
            Location_S.AddHandler(MouseDownEvent, new MouseButtonEventHandler(Location_MouseDown), true);
            Location_S.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Location_MouseUp), true);
            Volume_S.Value = 75;
            Zoom_Mode_C.Source = Sub_Code.Check_01;
        }
        public async void Window_Show()
        {
            Opacity = 0;
            Visibility = Visibility.Visible;
            Loop();
            if (Window_Show_Volumes.Count > 0 && Wwise_Player.IsExecution && Wwise_Player.IsInited())
            {
                for (int Number = 0; Number < Window_Show_Volumes.Count; Number++)
                    Wwise_Player.Set_Volume(Window_Show_Volumes[Number].Container_ID, Window_Show_Volumes[Number].Volume);
                Window_Show_Volumes.Clear();
            }
            while (Opacity < 1 && !IsClosing)
            {
                Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
        }
        //メッセージを表示
        async void Message_Feed_Out(string Message)
        {
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
        private async void Back_B_Click(object sender, RoutedEventArgs e)
        {
            if (!IsClosing)
            {
                IsClosing = true;
                Volume_Feed_Out(false);
                while (Opacity > 0)
                {
                    Opacity -= Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
                IsClosing = false;
                Visibility = Visibility.Hidden;
            }
        }
        async void Volume_Feed_Out(bool IsStop, double Feed_Time = 12.5)
        {
            if (!Wwise_Player.IsExecution || !Wwise_Player.IsInited())
                return;
            Window_Show_Volumes = Wwise_Player.Get_Volume();
            List<Container_und_Volume> Volumes = Wwise_Player.Get_Volume();
            List<double> Volume_Minus = new List<double>();
            for (int Number = 0; Number < Volumes.Count; Number++)
                Volume_Minus.Add(Volumes[Number].Volume / Feed_Time);
            while (true)
            {
                int Zero_Count = 0;
                for (int Number = 0; Number < Volumes.Count; Number++)
                {
                    Volumes[Number].Volume -= Volume_Minus[Number];
                    if (Volumes[Number].Volume <= 0)
                    {
                        Volumes[Number].Volume = 0;
                        Zero_Count++;
                    }
                    Wwise_Player.Set_Volume(Volumes[Number].Container_ID, Volumes[Number].Volume);
                }
                if (Zero_Count >= Volumes.Count)
                    break;
                await Task.Delay(1000 / 60);
            }
            if (IsStop)
            {
                Wwise_Player.Stop();
                Location_S.Value = 0;
                Location_S.Maximum = 0;
            }
            else
            {
                Wwise_Player.Pause_All(false);
                IsPaused = true;
            }
        }
        async void Loop()
        {
            double nextFrame = (double)Environment.TickCount;
            double period = 1000.0 / 30.0;
            while (Visibility == Visibility.Visible)
            {
                //FPSを上回っていたらスキップ
                double tickCount = (double)Environment.TickCount;
                if (tickCount < nextFrame)
                {
                    if (nextFrame - tickCount > 1)
                        await Task.Delay((int)(nextFrame - tickCount));
                    System.Windows.Forms.Application.DoEvents();
                    continue;
                }
                if (Bank_Names.Count > 0)
                {
                    if (Event_Name_List.SelectedIndex != -1 && Location_S.Maximum > 0 && !IsLocationChanging && !IsPaused)
                    {
                        int Position_Now = 0;
                        if (Event_Info[Page][Event_Name_List.SelectedIndex].Event_ID == 0)
                        {
                            Position_Now = Wwise_Player.Get_Position(Event_Info[Page][Event_Name_List.SelectedIndex].Event_Name);
                            if (Location_S.Maximum >= Position_Now)
                                Location_S.Value = Position_Now;
                            else
                            {
                                Event_Info[Page][Event_Name_List.SelectedIndex].Max_Length = Wwise_Player.Get_Max_Length(Event_Info[Page][Event_Name_List.SelectedIndex].Event_Name);
                                Location_S.Value = 0;
                                Location_S.Maximum = Event_Info[Page][Event_Name_List.SelectedIndex].Max_Length;
                            }
                        }
                        else
                        {
                            Position_Now = Wwise_Player.Get_Position(Event_Info[Page][Event_Name_List.SelectedIndex].Event_ID);
                            if (Location_S.Maximum >= Position_Now)
                                Location_S.Value = Position_Now;
                            else
                            {
                                uint Event_ID = Event_Info[Page][Event_Name_List.SelectedIndex].Event_ID;
                                Event_Info[Page][Event_Name_List.SelectedIndex].Max_Length = Wwise_Player.Get_Max_Length(Event_ID);
                                Location_S.Value = 0;
                                Location_S.Maximum = Event_Info[Page][Event_Name_List.SelectedIndex].Max_Length;
                            }
                        }
                    }
                    TimeSpan Time = TimeSpan.FromSeconds(Location_S.Value / 1000);
                    string Minutes = Time.Minutes.ToString();
                    string Seconds = Time.Seconds.ToString();
                    if (Time.Minutes < 10)
                        Minutes = "0" + Time.Minutes;
                    if (Time.Seconds < 10)
                        Seconds = "0" + Time.Seconds;
                    Location_T.Text = Minutes + ":" + Seconds;
                }
                //次のフレーム時間を計算
                if ((double)System.Environment.TickCount >= nextFrame + period)
                {
                    nextFrame += period;
                    continue;
                }
                nextFrame += period;
            }
        }
        private void Init_Bank_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            if (Wwise_Player.IsExecution)
            {
                Message_Feed_Out("内容がクリアされていません。先に右下のクリアボタンを押す必要があります。");
                return;
            }
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog()
            {
                Title = "Init.bnkを選択してください。",
                Filter = "Init.bnk(Init.bnk)|Init.bnk",
                Multiselect = false
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    Wwise_Player.Dispose();
                    Wwise_Player.Init(ofd.FileName, 1, Volume_S.Value / 100);
                    Init_Bank_B.Visibility = Visibility.Hidden;
                    Init_Bank_Help_B.Visibility = Visibility.Hidden;
                    IsInitSelected = true;
                }
                catch
                {
                    Message_Feed_Out("エラーが発生しました。Wwiseを初期化できません。");
                }
            }
            ofd.Dispose();
        }
        private async void Load_Bank_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog()
            {
                Title = ".bnkファイルを選択してください。",
                Filter = ".bnkファイル(*.bnk)|*.bnk",
                Multiselect = true
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    if (!IsInitSelected)
                    {
                        if (!Wwise_Player.IsExecution || !Wwise_Player.IsInited())
                        {
                            if (Wwise_Player.Init(Voice_Set.Special_Path + "\\Other\\Init.bnk", 1, Volume_S.Value / 100))
                            {
                                Init_Bank_B.Visibility = Visibility.Hidden;
                                Init_Bank_Help_B.Visibility = Visibility.Hidden;
                            }
                            else
                            {
                                Message_Feed_Out("Wwiseを初期化できません。");
                                return;
                            }
                        }
                    }
                    foreach (string File_Now in ofd.FileNames)
                    {
                        string filePath = Path.GetDirectoryName(File_Now) + "\\" + Path.GetFileNameWithoutExtension(File_Now);
                        if (!Wwise_Player.Load_Bank(filePath + ".bnk"))
                        {
                            uint ErrorCode = Wwise_Player.Get_Result_Index();
                            if (ErrorCode == 69)
                            {
                                Message_Feed_Out("既にロードされている.bnkファイルが存在します。");
                                return;
                            }
                            throw new Exception("ファイル:" + File_Now + "をロードできませんでした。\nエラーコード:" + Wwise_Player.Get_Result_Index());
                        }
                        if (File.Exists(filePath + ".pck"))
                        {
                            MessageBox.Show(Wwise_Player.Load_PCK(filePath + ".pck").ToString());
                            uint ErrorCode = Wwise_Player.Get_Result_Index();
                            MessageBox.Show("エラーコード:" + ErrorCode);
                        }
                        string Name_Only = Path.GetFileName(File_Now);
                        IsMessageShowing = false;
                        Message_T.Text = Name_Only + "を追加しています...";
                        await Task.Delay(50);
                        if (Bank_Names.Contains(File_Now))
                        {
                            Message_Feed_Out("既に同名のファイルが追加されています。\n" + Name_Only + "はスキップされます。");
                            continue;
                        }
                        BNK_Parse Wwise_BNK = new BNK_Parse(File_Now);
                        Message_T.Text = Name_Only + "のIDを文字列に変換しています...";
                        List<string> Temp = Wwise_BNK.Get_BNK_Event_ID_To_String();
                        Wwise_BNK.Clear();
                        Event_ID_To_Name(Temp);
                        Event_Names.Add(Temp);
                        Event_Info.Add(new List<Name_ID_Contaier>());
                        for (int Number_01 = 0; Number_01 < Temp.Count; Number_01++)
                        {
                            Name_ID_Contaier Temp_01 = new Name_ID_Contaier();
                            if (Temp[Number_01].All(char.IsDigit))
                            {
                                Temp_01.Event_Name = "";
                                Temp_01.Event_ID = uint.Parse(Temp[Number_01]);
                            }
                            else
                            {
                                Temp_01.Event_Name = Temp[Number_01];
                                Temp_01.Event_ID = 0;
                            }
                            Temp_01.Max_Length = 0;
                            Temp_01.Container_ID = r.Next(1, 1000000);
                            Temp_01.Volume = Volume_S.Value;
                            Event_Info[Event_Info.Count - 1].Add(Temp_01);
                        }
                        Bank_Names.Add(File_Now);
                        if (Page + 1 < Bank_Names.Count)
                            Page_Next_B.Visibility = Visibility.Visible;
                    }
                    Event_List_Change();
                    Change_Zoom_Mode();
                    Message_Feed_Out(".bnkファイルを読み込みました。");
                }
                catch (Exception e1)
                {
                    Message_Feed_Out("エラーが発生しました。");
                    Sub_Code.Error_Log_Write(e1.Message);
                    return;
                }
            }
            ofd.Dispose();
        }
        void Event_ID_To_Name(List<string> Event_List)
        {
            if (!File.Exists(Voice_Set.Special_Path + "\\Wwise\\SoundbanksInfo.json"))
            {
                Message_Feed_Out("SoundbanksInfo.jsonが見つからなかったため、処理を中止します。");
                return;
            }
            List<string> Read_Info = new List<string>();
            Read_Info.AddRange(File.ReadAllLines(Voice_Set.Special_Path + "\\Wwise\\SoundbanksInfo.json"));
            for (int Number = 0; Number < Read_Info.Count; Number++)
            {
                try
                {
                    if (Read_Info[Number].Contains("\"ObjectPath\": \"\\\\Events\\\\"))
                    {
                        uint ShortID = uint.Parse(Get_Value(Read_Info[Number - 2]));
                        string Name = Get_Value(Read_Info[Number - 1]);
                        if (Event_List.Contains(ShortID.ToString()))
                        {
                            int Index = Event_List.IndexOf(ShortID.ToString());
                            Event_List[Index] = Name;
                        }
                    }
                }
                catch { }
            }
            Read_Info.Clear();
            Event_List.Sort();
        }
        private void Init_Bank_Help_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            string Message_01 = "・WoTB用の.bnkファイルを読み込む場合、Init.bnkは指定しなくても大丈夫です。\n";
            string Message_02 = "・Init.bnkを指定する場合は、右下のクリアボタンを押す必要があります。";
            MessageBox.Show(Message_01 + Message_02);
        }
        string Get_Value(string Read_Line)
        {
            string Temp = Read_Line;
            if (Temp.Contains(":"))
                Temp = Temp.Substring(Temp.IndexOf(':'));
            Temp = Temp.Substring(Temp.IndexOf('"') + 1);
            return Temp.Substring(0, Temp.IndexOf('"'));
        }
        void Event_List_Change()
        {
            if (Bank_Names.Count == 0 || Page >= Bank_Names.Count)
                return;
            Window_Show_Volumes.Clear();
            Event_Name_List.Items.Clear();
            foreach (string Name in Event_Names[Page])
                Event_Name_List.Items.Add(Name);
            Bank_Name_T.Text = Path.GetFileName(Bank_Names[Page]);
            if (Page + 1 >= Bank_Names.Count)
                Page_Next_B.Visibility = Visibility.Hidden;
            else
                Page_Next_B.Visibility = Visibility.Visible;
            if (Page <= 0)
                Page_Back_B.Visibility = Visibility.Hidden;
            else
                Page_Back_B.Visibility = Visibility.Visible;
        }
        private void Page_Next_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            if (Page + 1 >= Bank_Names.Count)
            {
                Message_Feed_Out("ページが最大値を超えています。");
                return;
            }
            Page++;
            Event_List_Change();
            Wwise_Player.Stop();
        }
        private void Page_Back_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            if (Page == 0)
            {
                Message_Feed_Out("ページ既には最小値です。");
                return;
            }
            Page--;
            Event_List_Change();
            Wwise_Player.Stop();
        }
        private void Clear_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            MessageBoxResult result = MessageBox.Show("追加されている.bnkファイルをすべてクリアしますか?", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                Bank_Names.Clear();
                Event_Names.Clear();
                Event_Name_List.Items.Clear();
                Window_Show_Volumes.Clear();
                Event_Info.Clear();
                Page = 0;
                IsInitSelected = false;
                Location_S.Value = 0;
                Location_S.Maximum = 0;
                Location_T.Text = "00:00";
                Wwise_Player.Dispose();
                Init_Bank_B.Visibility = Visibility.Visible;
                Init_Bank_Help_B.Visibility = Visibility.Visible;
                Bank_Name_T.Text = "";
                Message_Feed_Out("内容をクリアしました。");
            }
        }
        private void Play_B_Click(object sender, RoutedEventArgs e)
        {
            if (!Wwise_Player.IsExecution || !Wwise_Player.IsInited() || IsClosing)
                return;
            if (Event_Name_List.SelectedIndex == -1)
            {
                Message_Feed_Out("'イベント名またはID'を選択してください。");
                return;
            }
            if (IsPaused)
            {
                Wwise_Player.Play_All();
                IsPaused = false;
            }
            else
            {
                if (Event_Name_List.Items[Event_Name_List.SelectedIndex].ToString().All(char.IsDigit))
                {
                    uint Event_ID = uint.Parse(Event_Name_List.Items[Event_Name_List.SelectedIndex].ToString());
                    Wwise_Player.Stop(Event_ID);
                    Wwise_Player.Play(Event_ID, Event_Info[Page][Event_Name_List.SelectedIndex].Container_ID);
                    Event_Info[Page][Event_Name_List.SelectedIndex].Max_Length = Wwise_Player.Get_Max_Length(Event_ID);
                    Location_S.Value = 0;
                    Location_S.Maximum = Event_Info[Page][Event_Name_List.SelectedIndex].Max_Length;
                }
                else
                {
                    Wwise_Player.Stop(Event_Name_List.Items[Event_Name_List.SelectedIndex].ToString());
                    Wwise_Player.Play(Event_Name_List.Items[Event_Name_List.SelectedIndex].ToString(), Event_Info[Page][Event_Name_List.SelectedIndex].Container_ID);
                    Event_Info[Page][Event_Name_List.SelectedIndex].Max_Length = Wwise_Player.Get_Max_Length(Event_Name_List.Items[Event_Name_List.SelectedIndex].ToString());
                    Location_S.Value = 0;
                    Location_S.Maximum = Event_Info[Page][Event_Name_List.SelectedIndex].Max_Length;
                }
                Wwise_Player.Set_Volume(Event_Info[Page][Event_Name_List.SelectedIndex].Container_ID, Event_Info[Page][Event_Name_List.SelectedIndex].Volume / 100);
            }
        }
        private void Pause_B_Click(object sender, RoutedEventArgs e)
        {
            if (!Wwise_Player.IsExecution || !Wwise_Player.IsInited() || IsClosing)
                return;
            Wwise_Player.Pause_All(false);
            IsPaused = true;
        }
        private void Stop_B_Click(object sender, RoutedEventArgs e)
        {
            if (!Wwise_Player.IsExecution || !Wwise_Player.IsInited() || IsClosing)
                return;
            Wwise_Player.Stop();
            IsPaused = false;
            for (int Number = 0; Number < Event_Info[Page].Count; Number++)
                Event_Info[Page][Number].Max_Length = 0;
            Location_S.Value = 0;
            Location_S.Maximum = 0;
        }
        private void Minus_B_Click(object sender, RoutedEventArgs e)
        {
            if (!Wwise_Player.IsExecution || !Wwise_Player.IsInited() || IsClosing)
                return;
            if (Bank_Names.Count > 0 && Event_Name_List.SelectedIndex != -1)
            {
                if (Event_Info[Page][Event_Name_List.SelectedIndex].Event_ID == 0)
                {
                    int Position_Now = Wwise_Player.Get_Position(Event_Info[Page][Event_Name_List.SelectedIndex].Event_Name);
                    Wwise_Player.Set_Position(Event_Info[Page][Event_Name_List.SelectedIndex].Event_Name, Position_Now - 5000);
                }
                else
                {
                    int Position_Now = Wwise_Player.Get_Position(Event_Info[Page][Event_Name_List.SelectedIndex].Event_ID);
                    Wwise_Player.Set_Position(Event_Info[Page][Event_Name_List.SelectedIndex].Event_ID, Position_Now - 5000);
                }
            }
        }
        private void Plus_B_Click(object sender, RoutedEventArgs e)
        {
            if (!Wwise_Player.IsExecution || !Wwise_Player.IsInited() || IsClosing)
                return;
            if (Bank_Names.Count > 0 && Event_Name_List.SelectedIndex != -1)
            {
                if (Event_Info[Page][Event_Name_List.SelectedIndex].Event_ID == 0)
                {
                    int Position_Now = Wwise_Player.Get_Position(Event_Info[Page][Event_Name_List.SelectedIndex].Event_Name);
                    Wwise_Player.Set_Position(Event_Info[Page][Event_Name_List.SelectedIndex].Event_Name, Position_Now + 5000);
                }
                else
                {
                    int Position_Now = Wwise_Player.Get_Position(Event_Info[Page][Event_Name_List.SelectedIndex].Event_ID);
                    Wwise_Player.Set_Position(Event_Info[Page][Event_Name_List.SelectedIndex].Event_ID, Position_Now + 5000);
                }
            }
        }
        private void Volume_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Volume_T.Text = "音量:" + (int)Volume_S.Value;
            if (!Wwise_Player.IsExecution || !Wwise_Player.IsInited() || IsClosing)
                return;
            if (Event_Name_List.SelectedIndex != -1)
            {
                if (Event_Info[Page][Event_Name_List.SelectedIndex].Event_ID == 0)
                    Wwise_Player.Set_Volume(Event_Info[Page][Event_Name_List.SelectedIndex].Event_Name, Volume_S.Value / 100);
                else
                    Wwise_Player.Set_Volume(Event_Info[Page][Event_Name_List.SelectedIndex].Event_ID, Volume_S.Value / 100);
                Event_Info[Page][Event_Name_List.SelectedIndex].Volume = Volume_S.Value;
            }
        }
        private void Event_Name_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Wwise_Player.IsExecution || !Wwise_Player.IsInited() || IsClosing || Event_Name_List.SelectedIndex == -1)
                return;
            Location_Before = 0;
            Location_S.Value = 0;
            Location_S.Maximum = Event_Info[Page][Event_Name_List.SelectedIndex].Max_Length;
            Volume_S.Value = Event_Info[Page][Event_Name_List.SelectedIndex].Volume;
        }
        void Location_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!Wwise_Player.IsExecution || !Wwise_Player.IsInited() || IsClosing || Event_Name_List.SelectedIndex == -1 || Event_Info[Page][Event_Name_List.SelectedIndex].Max_Length <= 0)
                return;
            IsLocationChanging = true;
            if (!IsPaused)
            {
                IsPlayingMouseDown = true;
                Wwise_Player.Pause_All(false);
                IsPaused = true;
            }
        }
        async void Location_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsLocationChanging)
                return;
            if (Event_Info[Page][Event_Name_List.SelectedIndex].Event_ID == 0)
                Wwise_Player.Set_Position(Event_Info[Page][Event_Name_List.SelectedIndex].Event_Name, (int)Location_S.Value);
            else
                Wwise_Player.Set_Position(Event_Info[Page][Event_Name_List.SelectedIndex].Event_ID, (int)Location_S.Value);
            if (IsPlayingMouseDown)
            {
                IsPaused = false;
                Wwise_Player.Play_All();
                IsPlayingMouseDown = false;
            }
            await Task.Delay(100);
            Location_Before = Location_S.Value;
            IsLocationChanging = false;
        }
        private void Reload_B_Click(object sender, RoutedEventArgs e)
        {
            if (Event_Name_List.SelectedIndex == -1)
                return;
            Event_Info[Page][Event_Name_List.SelectedIndex].Max_Length = Wwise_Player.Get_Max_Length(Event_Name_List.Items[Event_Name_List.SelectedIndex].ToString());
            Location_S.Maximum = Event_Info[Page][Event_Name_List.SelectedIndex].Max_Length;
            if (IsPaused)
                Wwise_Player.Pause_All(false);
        }
        private void Reload_Help_B_Click(object sender, RoutedEventArgs e)
        {
            string Message_01 = "ループ再生するサウンドは、いつ最初に戻るのか取得できないので手動でサウンドの長さを再度取得します。\n";
            string Message_02 = "サウンドの長さの再取得には0.2秒ほどかかります。";
            MessageBox.Show(Message_01 + Message_02);
        }
        private void Zoom_Mode_C_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IsZoomMode = !IsZoomMode;
            if (IsZoomMode)
                Zoom_Mode_C.Source = Sub_Code.Check_04;
            else
                Zoom_Mode_C.Source = Sub_Code.Check_02;
            Change_Zoom_Mode();
        }
        private void Zoom_Mode_C_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsZoomMode)
                Zoom_Mode_C.Source = Sub_Code.Check_04;
            else
                Zoom_Mode_C.Source = Sub_Code.Check_02;
        }
        private void Zoom_Mode_C_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsZoomMode)
                Zoom_Mode_C.Source = Sub_Code.Check_03;
            else
                Zoom_Mode_C.Source = Sub_Code.Check_01;
        }
        void Change_Zoom_Mode()
        {
            if (Wwise_Player.IsInited())
            {
                if (IsZoomMode)
                    Wwise_Player.Set_State("STATE_view_play_mode", "STATE_view_play_mode_sniper");
                else
                    Wwise_Player.Set_State("STATE_view_play_mode", "STATE_view_play_mode_arcade");
            }
        }
    }
}