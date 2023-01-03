using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;

namespace WoTB_Voice_Mod_Creater.Class
{
    class Time_Relation
    {
        //時間と単位を保存するクラス
        public List<double> Times { get; private set; }
        public string Unit { get; private set; }
        public Time_Relation(List<double> Times, string Unit)
        {
            this.Times = Times;
            this.Unit = Unit;
        }
    }
    class Sound_Index_Relation
    {
        public int Sound_Index { get; private set; }
        public int Add_Sound_Index { get; set; }
        public Sound_Index_Relation(int Sound_Index, int Add_Sound_Index)
        {
            this.Sound_Index = Sound_Index;
            this.Add_Sound_Index = Add_Sound_Index;
        }
    }
    public partial class Sound_Editor : UserControl
    {
        string Add_File_Dir = "";
        string Save_File_Dir = "";
        int FPS = 60;
        int Save_Serial_Number = 0;
        int Max_Sound_Index = 0;
        double Play_Time = 0;
        double Time_Line_Move_Width_Scrool = 0;
        double Pitch_Value = 0;
        double Time_Line_Left = 0;
        double All_Volume_Set = 1;
        float Play_Pitch_Percent = 0.5f;
        bool IsClosing = false;
        bool IsImageClicked = false;
        bool IsMessageShowing = false;
        bool IsPlaying = false;
        bool IsSoundPosChanged = false;
        bool IsControlMode = false;
        bool IsSoundSelected = false;
        bool IsDeleted = false;
        bool IsLeftKeyDown = false;
        bool IsRightKeyDown = false;
        bool IsSpaceKeyDown = false;
        bool IsCKeyDown = false;
        bool IsAKeyDown = false;
        bool IsSKeyDown = false;
        bool IsDeleteDown = false;
        bool IsSoundMoving = false;
        bool IsTimeMoveMode = false;
        bool IsTimeMoveMode_IsPlaying = false;
        bool IsBusy = false;
        bool IsLoading = false;
        bool IsFeedIn = false;
        bool IsFeedOut = false;
        bool IsVolume_Speed_Changed_By_Key = false;
        public bool IsFocusMode = true;
        //xamlに配置するコントロールを置く
        List<Image> Sound_Images = new List<Image>();
        List<TextBlock> Time_Text = new List<TextBlock>();
        List<Border> Border_Lines = new List<Border>();
        List<Border> Sound_Select_Lines = new List<Border>();
        List<Slider> Sound_Volumes = new List<Slider>();
        List<TextBlock> Sound_Names = new List<TextBlock>();
        List<Canvas> Setting_Canvases = new List<Canvas>();
        //サウンドの設定を保存
        List<string> Sound_Files = new List<string>();
        List<double> Sound_Positions = new List<double>();
        List<double> Sound_Plus_Play_Time = new List<double>();
        List<double> Sound_Minus_Play_Time = new List<double>();
        List<double> Sound_Max_Length = new List<double>();
        List<double> Time_Side_Values = new List<double>();
        List<int> Sound_Images_Y_Pos = new List<int>();
        List<int> Setting_Canvas_Y_Pos = new List<int>();
        List<int> Sound_Streams = new List<int>();
        List<int> Sound_Selected_Index = new List<int>();
        List<float> Sound_Frequencys = new List<float>();
        List<bool> IsNumberKeyDown = new List<bool>();
        List<Time_Relation> Time_Info = new List<Time_Relation>();
        List<Sound_Index_Relation> Sound_Index_Info = new List<Sound_Index_Relation>();
        List<System.Windows.Point> Image_Point = new List<System.Windows.Point>();
        BitmapFrame Temp_WaveForm;
        System.Windows.Point Mouse_Point = new System.Windows.Point(0, 0);
        public Sound_Editor()
        {
            InitializeComponent();
            Time_Set();
            //スライダーの初期設定
            Track_Scrool.Value = 0;
            Track_Scrool.Maximum = 0;
            Track_Scrool.Visibility = Visibility.Hidden;
            Time_Scrool.Value = 0;
            Time_Scrool.Maximum = Time_Info.Count - 1;
            Pitch_S.Value = 50;
            //タイムラインの時間を入れるようのTextBlockを作成
            for (int Number = 0; Number < 6; Number++)
            {
                Time_Text.Add(new TextBlock());
                Time_Text[Number].Name = "Time_Text_" + Number;
                Time_Text[Number].Text = "";
                Time_Text[Number].Width = 100;
                Time_Text[Number].Height = 60;
                Time_Text[Number].Foreground = Brushes.Aqua;
                Time_Text[Number].FontSize = 35;
                Time_Text[Number].VerticalAlignment = VerticalAlignment.Top;
                Time_Text[Number].HorizontalAlignment = HorizontalAlignment.Center;
                Time_Text[Number].TextWrapping = TextWrapping.NoWrap;
                Time_Text[Number].TextAlignment = TextAlignment.Center;
                Time_Text[Number].Margin = new Thickness(-3320 + 555 * Number, 250, 0, 0);
                Parent_Dock.Children.Add(Time_Text[Number]);
            }
            for (int Number = 0; Number < 9; Number++)
                IsNumberKeyDown.Add(false);
            //.NameはSound_Image以外必要ないけど一応指定
            Time_Text.Add(new TextBlock());
            Time_Text[6].Name = "Time_Text_Unit";
            Time_Text[6].Width = 175;
            Time_Text[6].Height = 60;
            Time_Text[6].Foreground = Brushes.Aqua;
            Time_Text[6].FontSize = 30;
            Time_Text[6].VerticalAlignment = VerticalAlignment.Top;
            Time_Text[6].HorizontalAlignment = HorizontalAlignment.Center;
            Time_Text[6].TextWrapping = TextWrapping.NoWrap;
            Time_Text[6].TextAlignment = TextAlignment.Center;
            Time_Text[6].Margin = new Thickness(-300, 250, 0, 0);
            //再生時間バーの▼マーク
            Time_Text.Add(new TextBlock());
            Time_Text[7].Name = "Time_Tri";
            Time_Text[7].Width = 35;
            Time_Text[7].Height = 35;
            Time_Text[7].Foreground = Brushes.Aquamarine;
            Time_Text[7].Text = "▼";
            Time_Text[7].FontSize = 30;
            Time_Text[7].VerticalAlignment = VerticalAlignment.Top;
            Time_Text[7].HorizontalAlignment = HorizontalAlignment.Center;
            Time_Text[7].TextWrapping = TextWrapping.NoWrap;
            Time_Text[7].TextAlignment = TextAlignment.Center;
            Time_Text[7].Margin = new Thickness(-16, -40, 0, 0);
            Save_Combo.Items.Add("すべてのトラック");
            Save_Combo.Items.Add("選択中のトラックのみ");
            Save_Combo.SelectedIndex = 0;
            Parent_Dock.Children.Add(Time_Text[6]);
            Time_Tri_Canvas.Children.Add(Time_Text[7]);
            Pitch_S.AddHandler(MouseDownEvent, new MouseButtonEventHandler(Pitch_S_MouseDown), true);
            Pitch_S.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Pitch_S_MouseUp), true);
            Time_Scrool.Value = 20;
            System.Reflection.Assembly sra = System.Reflection.Assembly.GetExecutingAssembly();
            Temp_WaveForm = BitmapFrame.Create(sra.GetManifestResourceStream("WoTB_Voice_Mod_Creater.Resources.Temp_WaveForm.png"));
        }
        private void Slider_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }
        //横軸の時間幅を指定
        void Time_Set()
        {
            Time_Info.Clear();
            string[] Time_Units = { "秒", "分" };
            foreach (string Time in Time_Units)
            {
                string Time_Unit = Time;
                if (Time_Unit == "秒")
                    Time_Info.Add(new Time_Relation(new List<double>() { 0, 0.2, 0.4, 0.6, 0.8, 1 }, Time_Unit));
                else
                    Time_Info.Add(new Time_Relation(new List<double>() { 0, 0.4, 0.8, 1.2, 1.6, 2 }, Time_Unit));
                Time_Info.Add(new Time_Relation(new List<double>() { 0, 1, 2, 3 }, Time_Unit));
                Time_Info.Add(new Time_Relation(new List<double>() { 0, 1, 2, 3, 4, 5 }, Time_Unit));
                Time_Info.Add(new Time_Relation(new List<double>() { 0, 1.5, 3, 4.5, 6, 7.5 }, Time_Unit));
                Time_Info.Add(new Time_Relation(new List<double>() { 0, 2, 4, 6, 8, 10 }, Time_Unit));
                Time_Info.Add(new Time_Relation(new List<double>() { 0, 2.5, 5, 7.5, 10, 12.5 }, Time_Unit));
                Time_Info.Add(new Time_Relation(new List<double>() { 0, 3, 6, 9, 12, 15 }, Time_Unit));
                Time_Info.Add(new Time_Relation(new List<double>() { 0, 3.5, 7, 10.5, 14, 17.5 }, Time_Unit));
                Time_Info.Add(new Time_Relation(new List<double>() { 0, 4, 8, 12, 16, 20 }, Time_Unit));
                Time_Info.Add(new Time_Relation(new List<double>() { 0, 4.5, 9, 13.5, 18, 22.5 }, Time_Unit));
                Time_Info.Add(new Time_Relation(new List<double>() { 0, 5, 10, 15, 20, 25 }, Time_Unit));
                Time_Info.Add(new Time_Relation(new List<double>() { 0, 5.5, 11, 16.5, 22, 27.5 }, Time_Unit));
                Time_Info.Add(new Time_Relation(new List<double>() { 0, 6, 12, 18, 24, 30 }, Time_Unit));
                Time_Info.Add(new Time_Relation(new List<double>() { 0, 6.5, 13, 19.5, 26, 32.5 }, Time_Unit));
                Time_Info.Add(new Time_Relation(new List<double>() { 0, 7, 14, 21, 28, 35 }, Time_Unit));
                Time_Info.Add(new Time_Relation(new List<double>() { 0, 7.5, 15, 22.5, 30, 37.5 }, Time_Unit));
                Time_Info.Add(new Time_Relation(new List<double>() { 0, 8, 16, 24, 32, 40 }, Time_Unit));
                Time_Info.Add(new Time_Relation(new List<double>() { 0, 8.5, 17, 25.5, 34, 42.5 }, Time_Unit));
                Time_Info.Add(new Time_Relation(new List<double>() { 0, 9, 18, 27, 36, 45 }, Time_Unit));
                Time_Info.Add(new Time_Relation(new List<double>() { 0, 9.5, 19, 28.5, 38, 47.5 }, Time_Unit));
                Time_Info.Add(new Time_Relation(new List<double>() { 0, 10, 20, 30, 40, 50 }, Time_Unit));
                Time_Info.Add(new Time_Relation(new List<double>() { 0, 10.5, 21, 31.5, 42, 52.5 }, Time_Unit));
                Time_Info.Add(new Time_Relation(new List<double>() { 0, 11, 22, 33, 44, 55 }, Time_Unit));
                Time_Info.Add(new Time_Relation(new List<double>() { 0, 11.5, 23, 34.5, 46, 57.5 }, Time_Unit));
                Time_Info.Add(new Time_Relation(new List<double>() { 0, 12, 24, 36, 48, 60 }, Time_Unit));
            }
        }
        //ループ処理
        //引数:FPS値(基本は60fです)
        async void Loop_FPS()
        {
            double nextFrame = (double)Environment.TickCount;
            float period = 1000f / FPS;
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
                //再生中なら実行
                if (IsPlaying)
                {
                    Play_Time += (double)1 / FPS * Play_Pitch_Percent;
                    double Max_Time_Seconds = Time_Info[(int)Time_Scrool.Value].Times[Time_Info[(int)Time_Scrool.Value].Times.Count - 1];
                    if (Time_Info[(int)Time_Scrool.Value].Unit == "分")
                        Max_Time_Seconds *= 60;
                    else if (Time_Info[(int)Time_Scrool.Value].Unit == "時間")
                        Max_Time_Seconds *= 60 * 60;
                    double Parcent = Play_Time / Max_Time_Seconds;
                    Time_Line.Margin = new Thickness(1400 * Parcent - 1400 * Time_Line_Move_Width_Scrool, Time_Line.Margin.Top, 0, 0);
                    double Max_Track_Length = 0;
                    for (int Number = 0; Number < Sound_Images.Count; Number++)
                    {
                        if (Sound_Images.Count - 1 >= Number)
                        {
                            Bass.BASS_ChannelUpdate(Sound_Streams[Number], 400);
                            if (Time_Line.Margin.Left >= Sound_Images[Number].Margin.Left && Time_Line.Margin.Left < Sound_Images[Number].Margin.Left + Sound_Images[Number].Width)
                            {
                                if (Bass.BASS_ChannelIsActive(Sound_Streams[Number]) != BASSActive.BASS_ACTIVE_PLAYING)
                                {
                                    Bass.BASS_ChannelSetPosition(Sound_Streams[Number], Play_Time - Sound_Positions[Number] + Sound_Plus_Play_Time[Number]);
                                    Bass.BASS_ChannelPlay(Sound_Streams[Number], false);
                                }
                                else if (IsSoundPosChanged)
                                    Bass.BASS_ChannelSetPosition(Sound_Streams[Number], Play_Time - Sound_Positions[Number] + Sound_Plus_Play_Time[Number]);
                            }
                            else if (Bass.BASS_ChannelIsActive(Sound_Streams[Number]) == BASSActive.BASS_ACTIVE_PLAYING)
                                Bass.BASS_ChannelPause(Sound_Streams[Number]);
                        }
                        double End_Time = Sound_Max_Length[Number] * Sound_Minus_Play_Time[Number] + Sound_Positions[Number];
                        if (Max_Track_Length < End_Time)
                            Max_Track_Length = End_Time;
                    }
                    if (Setting_Window.Set_Loop_Mode_C.IsChecked.Value && Max_Track_Length > 0 && Max_Track_Length < Play_Time && !IsImageClicked)
                        Play_Time = 0;
                    IsSoundPosChanged = false;
                }
                if (Time_Text[7].Margin.Left < -30 || Time_Text[7].Margin.Left > 1380)
                    Time_Text[7].Visibility = Visibility.Hidden;
                else
                    Time_Text[7].Visibility = Visibility.Visible;
                //設定画面の全体の音量を変更した場合、瞬時に反映されるように
                if (Setting_Window.Visibility == Visibility.Visible && Setting_Window.All_Volume_S.Value / 100 != All_Volume_Set)
                {
                    All_Volume_Set = Setting_Window.All_Volume_S.Value / 100;
                    for (int Number = 0; Number < Sound_Streams.Count; Number++)
                        Bass.BASS_ChannelSetAttribute(Sound_Streams[Number], BASSAttribute.BASS_ATTRIB_VOL, (float)(Sound_Volumes[Number].Value / 100 * All_Volume_Set));
                }
                if (!IsBusy && !IsLoading)
                {
                    //Ctrlキーが押されているか
                    bool IsControlDown = (System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Control) == System.Windows.Forms.Keys.Control;
                    if (IsControlDown && !IsControlMode)
                    {
                        IsControlMode = true;
                        Music_Minus_B.Content = "-10秒";
                        Music_Plus_B.Content = "+10秒";
                    }
                    else if (!IsControlDown && IsControlMode)
                    {
                        IsControlMode = false;
                        Music_Minus_B.Content = "-5秒";
                        Music_Plus_B.Content = "+5秒";
                    }
                    //ウィンドウにフォーカスがあれば実行
                    if (Sub_Code.IsForcusWindow)
                    {
                        //数秒戻る
                        if ((Keyboard.GetKeyStates(Key.Left) & KeyStates.Down) > 0)
                        {
                            if (!IsLeftKeyDown)
                                Music_Minus_B.PerformClick();
                            IsLeftKeyDown = true;
                        }
                        else
                            IsLeftKeyDown = false;
                        //数秒進む
                        if ((Keyboard.GetKeyStates(Key.Right) & KeyStates.Down) > 0)
                        {
                            if (!IsRightKeyDown)
                                Music_Plus_B.PerformClick();
                            IsRightKeyDown = true;
                        }
                        else
                            IsRightKeyDown = false;
                        //再生・停止
                        if ((Keyboard.GetKeyStates(Key.Space) & KeyStates.Down) > 0)
                        {
                            if (!IsSpaceKeyDown)
                            {
                                if (IsPlaying)
                                {
                                    Music_Pause_B.PerformClick();
                                }
                                else
                                    Music_Start_B.PerformClick();
                            }
                            IsSpaceKeyDown = true;
                        }
                        else
                            IsSpaceKeyDown = false;
                        if ((Keyboard.GetKeyStates(Key.C) & KeyStates.Down) > 0 && IsControlDown)
                        {
                            if (!IsCKeyDown && Setting_Window.Cut_ShortCut_C.IsChecked.Value)
                                Music_Cut_B.PerformClick();
                            IsCKeyDown = true;
                        }
                        else
                            IsCKeyDown = false;
                        if ((Keyboard.GetKeyStates(Key.A) & KeyStates.Down) > 0 && IsControlDown)
                        {
                            if (!IsAKeyDown)
                            {
                                bool IsAllSelected = true;
                                for (int Number = 0; Number < Sound_Images.Count; Number++)
                                {
                                    if (!Sound_Selected_Index.Contains(Number))
                                        IsAllSelected = false;
                                }
                                Sound_Select_Unlock();
                                if (!IsAllSelected)
                                {
                                    for (int Number = 0; Number < Sound_Images.Count; Number++)
                                    {
                                        Sound_Selected_Index.Add(Number);
                                        Image_Point.Add(new Point(Sound_Images[Number].Margin.Left, Sound_Images[Number].Margin.Top));
                                        Sound_Select_Lines[Number].Opacity = 0.5;
                                    }
                                }
                            }
                            IsAKeyDown = true;
                        }
                        else
                            IsAKeyDown = false;
                        if ((Keyboard.GetKeyStates(Key.S) & KeyStates.Down) > 0 && IsControlDown)
                        {
                            if (!IsSKeyDown)
                                Music_Save_B.PerformClick();
                            IsSKeyDown = true;
                        }
                        else
                            IsSKeyDown = false;
                        if ((Keyboard.GetKeyStates(Key.Delete) & KeyStates.Down) > 0)
                        {
                            if (!IsDeleteDown)
                                Sound_Remove_Index();
                            IsDeleteDown = true;
                        }
                        else
                            IsDeleteDown = false;
                        if ((Keyboard.GetKeyStates(Key.D1) & KeyStates.Down) > 0)
                            Number_Key_Down(0);
                        else
                            IsNumberKeyDown[0] = false;
                        if ((Keyboard.GetKeyStates(Key.D2) & KeyStates.Down) > 0)
                            Number_Key_Down(1);
                        else
                            IsNumberKeyDown[1] = false;
                        if ((Keyboard.GetKeyStates(Key.D3) & KeyStates.Down) > 0)
                            Number_Key_Down(2);
                        else
                            IsNumberKeyDown[2] = false;
                        if ((Keyboard.GetKeyStates(Key.D4) & KeyStates.Down) > 0)
                            Number_Key_Down(3);
                        else
                            IsNumberKeyDown[3] = false;
                        if ((Keyboard.GetKeyStates(Key.D5) & KeyStates.Down) > 0)
                            Number_Key_Down(4);
                        else
                            IsNumberKeyDown[4] = false;
                        if ((Keyboard.GetKeyStates(Key.D6) & KeyStates.Down) > 0)
                            Number_Key_Down(5);
                        else
                            IsNumberKeyDown[5] = false;
                        if ((Keyboard.GetKeyStates(Key.D7) & KeyStates.Down) > 0)
                            Number_Key_Down(6);
                        else
                            IsNumberKeyDown[6] = false;
                        if ((Keyboard.GetKeyStates(Key.D8) & KeyStates.Down) > 0)
                            Number_Key_Down(7);
                        else
                            IsNumberKeyDown[7] = false;
                        if ((Keyboard.GetKeyStates(Key.D9) & KeyStates.Down) > 0)
                            Number_Key_Down(8);
                        else
                            IsNumberKeyDown[8] = false;
                        if ((Keyboard.GetKeyStates(Key.V) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.Up) & KeyStates.Down) > 0)
                            foreach (int Index in Sound_Selected_Index)
                            {
                                double Increase = 0.65 * 60 / FPS;
                                if (Sound_Volumes[Index].Value + Increase > 100)
                                    Sound_Volumes[Index].Value = 100;
                                else
                                    Sound_Volumes[Index].Value += Increase;
                            }
                        else if ((Keyboard.GetKeyStates(Key.V) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.Down) & KeyStates.Down) > 0)
                            foreach (int Index in Sound_Selected_Index)
                            {
                                double Decrease = 0.65 * 60 / FPS;
                                if (Sound_Volumes[Index].Value - Decrease < 0)
                                    Sound_Volumes[Index].Value = 0;
                                else
                                    Sound_Volumes[Index].Value -= Decrease;
                            }
                        if ((Keyboard.GetKeyStates(Key.S) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.Up) & KeyStates.Down) > 0)
                        {
                            double Increase = 0.3 * 60 / FPS;
                            if (Pitch_S.Value + Increase > 100)
                                Pitch_S.Value = 100;
                            else
                                Pitch_S.Value += Increase;
                            IsVolume_Speed_Changed_By_Key = true;
                        }
                        else if ((Keyboard.GetKeyStates(Key.S) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.Down) & KeyStates.Down) > 0)
                        {
                            double Decrease = 0.3 * 60 / FPS;
                            if (Pitch_S.Value + Decrease < 0)
                                Pitch_S.Value = 0;
                            else
                                Pitch_S.Value -= Decrease;
                            IsVolume_Speed_Changed_By_Key = true;
                        }
                        else if ((Keyboard.GetKeyStates(Key.S) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.R) & KeyStates.Down) > 0)
                        {
                            Pitch_S.Value = 50;
                            IsVolume_Speed_Changed_By_Key = true;
                        }
                        if (IsPlaying)
                        {
                            if (IsVolume_Speed_Changed_By_Key && (Keyboard.GetKeyStates(Key.S) & KeyStates.Down) == 0 && (Keyboard.GetKeyStates(Key.Up) & KeyStates.Down) == 0 && (Keyboard.GetKeyStates(Key.Down) & KeyStates.Down) == 0)
                            {
                                IsVolume_Speed_Changed_By_Key = false;
                                Pitch_Change_Await();
                            }
                        }
                    }
                }
                //現在時間を表示(秒)
                Time_T.Text = Math.Round(Play_Time, 1, MidpointRounding.AwayFromZero) + "秒";
                Time_Text[7].Margin = new Thickness(Time_Line.Margin.Left - 17, -40, 0, 0);
                if (period != 1000f / FPS)
                    period = 1000f / FPS;
                //次のフレーム時間を計算
                if ((double)System.Environment.TickCount >= nextFrame + (double)period)
                {
                    nextFrame += period;
                    continue;
                }
                nextFrame += period;
            }
        }
        //ウィンドウを表示
        public async void Window_Show()
        {
            Opacity = 0;
            Visibility = Visibility.Visible;
            Time_Text_Change((int)Time_Scrool.Value);
            if (File.Exists(Voice_Set.Special_Path + "/Configs/Sound_Editor.conf"))
            {
                try
                {
                    StreamReader str = Sub_Code.File_Decrypt_To_Stream(Voice_Set.Special_Path + "/Configs/Sound_Editor.conf", "Sound_Editor_Configs_Save");
                    Add_File_Dir = str.ReadLine();
                    Save_File_Dir = str.ReadLine();
                    str.Close();
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show("Sound_Editorの設定を読み込めませんでした。\nエラー回避のため設定は削除されます。");
                    File.Delete(Voice_Set.Special_Path + "/Configs/Sound_Editor.conf");
                    Sub_Code.Error_Log_Write(e.Message);
                }
            }
            if (!Setting_Window.IsConfigsLoaded)
                Setting_Window.Configs_Load();
            if (Setting_Window.Framerate_C.SelectedIndex == 0)
                FPS = 30;
            else if (Setting_Window.Framerate_C.SelectedIndex == 1)
                FPS = 60;
            else if (Setting_Window.Framerate_C.SelectedIndex == 2)
                FPS = 120;
            Loop_FPS();
            if (!Setting_Window.Save_Once_C.IsChecked.Value)
                Set_Serial_Number(Setting_Window.Save_Dir + "\\" + Setting_Window.Save_File_Name_T.Text);
            if (Sub_Code.IsWindowBarShow)
                Window_Bar_Canvas.Margin = new Thickness(0, 25, 0, 0);
            else
                Window_Bar_Canvas.Margin = new Thickness(0, 0, 0, 0);
            while (Opacity < 1 && !IsClosing)
            {
                Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
        }
        //押された数字キーのトラックを選択または未選択状態にする(Ctrlキーが押されたら複数選択)
        void Number_Key_Down(int Number)
        {
            if (!IsNumberKeyDown[Number] && Sound_Images.Count > Number)
            {
                IsNumberKeyDown[Number] = true;
                for (int Number_01 = 0; Number_01 < Sound_Images_Y_Pos.Count; Number_01++)
                {
                    if (Sound_Images_Y_Pos[Number_01] == 125 * Number + 10 * Number + 10)
                    {
                        Number = Number_01;
                        break;
                    }
                }
                if (!IsControlMode && !Sound_Selected_Index.Contains(Number))
                    Sound_Select_Unlock();
                else if (!IsControlMode && Sound_Selected_Index.Count >= 2)
                    Sound_Select_Unlock();
                if (Sound_Selected_Index.Contains(Number))
                {
                    int Index = Sound_Selected_Index.IndexOf(Number);
                    if (Index == -1)
                        return;
                    Sound_Selected_Index.RemoveAt(Index);
                    Image_Point.RemoveAt(Index);
                    Sound_Select_Lines[Number].Opacity = 0;
                }
                else
                {
                    Sound_Selected_Index.Add(Number);
                    Image_Point.Add(new Point(Sound_Images[Number].Margin.Left, Sound_Images[Number].Margin.Top));
                    Sound_Select_Lines[Number].Opacity = 0.5;
                }
            }
        }
        //メッセージを表示してフェードアウト
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
        //戻る
        private async void Back_B_Click(object sender, RoutedEventArgs e)
        {
            if (!IsClosing && !IsBusy)
            {
                IsClosing = true;
                All_Sound_Pause(15f);
                while (Opacity > 0)
                {
                    Opacity -= Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
                IsClosing = false;
                if (IsPlaying)
                    IsPlaying = false;
                Visibility = Visibility.Hidden;
            }
        }
        //ファイルを追加
        private async void Music_Add_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog()
            {
                Title = "サウンドファイルを指定してください。",
                Multiselect = true,
                Filter = "サウンドファイル(*.mp3;*.wav;)|*.mp3;*.wav;",
            };
            if (Add_File_Dir != "")
                ofd.InitialDirectory = Add_File_Dir;
            bool IsPlaying_Temp = IsPlaying;
            IsPlaying = false;
            All_Sound_Pause();
            await Task.Delay(100);
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Add_File_Dir = Path.GetDirectoryName(ofd.FileName);
                Add_Sound_File(ofd.FileNames);
                Configs_Save();
            }
            if (IsPlaying_Temp)
            {
                IsPlaying = true;
                All_Sound_Play(10f);
            }
            ofd.Dispose();
        }
        //サウンドを追加(引数:複数のファイル場所)
        public async void Add_Sound_File(string[] Sound_File)
        {
            if (IsClosing || IsBusy)
                return;
            IsPlaying = false;
            All_Sound_Pause();
            foreach (string File_Now in Sound_File)
            {
                if (!File.Exists(File_Now))
                    continue;
                Message_T.Text = "波形を生成しています...";
                await Task.Delay(50);
                ImageSource Image_Wave = null;
                string Ex = Path.GetExtension(File_Now);
                int StreamHandle = Bass.BASS_StreamCreateFile(File_Now, 0, 0, BASSFlag.BASS_STREAM_DECODE);
                if (Bass.BASS_ChannelBytes2Seconds(StreamHandle, Bass.BASS_ChannelGetLength(StreamHandle, BASSMode.BASS_POS_BYTES)) > 600)
                    Image_Wave = Temp_WaveForm;
                else
                    Image_Wave = Sub_Code.BassRenderWaveForm(File_Now);
                Bass.BASS_StreamFree(StreamHandle);
                if (Image_Wave == null)
                    continue;
                Message_T.Text = "サウンドを読み込んでいます...";
                await Task.Delay(50);
                await Add_Sound(File_Now, Image_Wave, -1, Setting_Window.Volume_S.Value);
            }
            Set_Sound_Width(Time_Info[(int)Time_Scrool.Value]);
            Message_Feed_Out("サウンドを読み込みました。");
        }
        //サウンドの追加(単体)
        async Task Add_Sound(string File_Now, ImageSource Image_Wave, int Add_Index_Pos = -1, double Set_Volume = -1, int Image_Y_Pos = -1)
        {
            IsBusy = true;
            //Bassの設定
            int StreamHandle = Bass.BASS_StreamCreateFile(File_Now, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_STREAM_PRESCAN);
            BASS_CHANNELINFO info = new BASS_CHANNELINFO();
            Bass.BASS_ChannelGetInfo(StreamHandle, info);
            if (Path.GetExtension(File_Now) == ".mp3")
            {
                if (info.ctype != BASSChannelType.BASS_CTYPE_STREAM_MP1 && info.ctype != BASSChannelType.BASS_CTYPE_STREAM_MP2 && info.ctype != BASSChannelType.BASS_CTYPE_STREAM_MP3)
                {
                    bool IsError = false;
                    MessageBoxResult result = MessageBox.Show("\"" + Path.GetFileName(File_Now) + "\"はMP3形式ではありません。MP3に変換して読み込みますか？\nいいえを選択した場合はそのまま読み込みます。",
                        "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes);
                    if (result == MessageBoxResult.Yes)
                    {
                        Message_T.Text = ".mp3形式に変換しています...";
                        await Task.Delay(50);
                        if (Sub_Code.Audio_Encode_To_Other(File_Now, Path.GetDirectoryName(File_Now) + "\\" + Path.GetFileNameWithoutExtension(File_Now) + "_Temp.mp3", "mp3", false))
                        {
                            if (Sub_Code.File_Move(Path.GetDirectoryName(File_Now) + "\\" + Path.GetFileNameWithoutExtension(File_Now) + "_Temp.mp3", File_Now, true))
                            {
                                Bass.BASS_StreamFree(StreamHandle);
                                StreamHandle = Bass.BASS_StreamCreateFile(File_Now, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_STREAM_PRESCAN);
                            }
                            else
                                IsError = true;
                        }
                        else
                            IsError = true;
                        Message_T.Text = "";
                    }
                    if (IsError)
                        MessageBox.Show("\"" + Path.GetFileName(File_Now) + "\"を変換できませんでした。そのまま読み込みます。");
                }
            }
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, 100);
            Sound_Streams.Add(BassFx.BASS_FX_TempoCreate(StreamHandle, BASSFlag.BASS_FX_FREESOURCE));
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, 500);
            Sound_Max_Length.Add(Bass.BASS_ChannelBytes2Seconds(StreamHandle, Bass.BASS_ChannelGetLength(StreamHandle, BASSMode.BASS_POS_BYTES)));
            float Temp_Freq = 44100;
            Bass.BASS_ChannelSetDevice(Sound_Streams[Sound_Streams.Count - 1], Video_Mode.Sound_Device);
            Bass.BASS_ChannelGetAttribute(Sound_Streams[Sound_Streams.Count - 1], BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, ref Temp_Freq);
            if (IsPlaying)
            {
                if (Set_Volume != -1)
                    Bass.BASS_ChannelSetAttribute(Sound_Streams[Sound_Streams.Count - 1], BASSAttribute.BASS_ATTRIB_VOL, (float)(Set_Volume / 100 * All_Volume_Set));
                else
                    Bass.BASS_ChannelSetAttribute(Sound_Streams[Sound_Streams.Count - 1], BASSAttribute.BASS_ATTRIB_VOL, (float)(0.75 * All_Volume_Set));
            }
            else
                Bass.BASS_ChannelSetAttribute(Sound_Streams[Sound_Streams.Count - 1], BASSAttribute.BASS_ATTRIB_VOL, 0f);
            Bass.BASS_ChannelSetAttribute(Sound_Streams[Sound_Streams.Count - 1], BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, Temp_Freq * Play_Pitch_Percent);
            Sound_Frequencys.Add(Temp_Freq);
            //追加する画像の表示位置(高さ)を指定
            int Set_Top = 125 * Sound_Images.Count + 10 * Sound_Images.Count + 10;
            if (Image_Y_Pos != -1)
                Set_Top = Image_Y_Pos;
            //それぞれの初期値を保存
            Sound_Images_Y_Pos.Add(Set_Top);
            Setting_Canvas_Y_Pos.Add(Set_Top);
            Sound_Positions.Add(0);
            Sound_Plus_Play_Time.Add(0);
            Sound_Minus_Play_Time.Add(1);
            Sound_Files.Add(File_Now);
            Sound_Images.Add(new Image());
            Border_Lines.Add(new Border());
            Sound_Select_Lines.Add(new Border());
            Sound_Volumes.Add(new Slider());
            Sound_Names.Add(new TextBlock());
            Setting_Canvases.Add(new Canvas());
            int This_Image_Index = Sound_Images.Count - 1;
            int Now_Sound_Index = Max_Sound_Index;
            Sound_Index_Info.Add(new Sound_Index_Relation(Now_Sound_Index, This_Image_Index));
            Max_Sound_Index++;
            //右クリックしたときに表示されるメニューを追加
            ContextMenu pMenu = new ContextMenu();
            MenuItem item1 = new MenuItem();
            item1.Header = "削除";
            item1.Click += delegate
            {
                Sound_Remove_Index();
            };
            MenuItem item2 = new MenuItem();
            item2.Header = "0秒に配置";
            item2.Click += delegate
            {
                Sound_Move_Zero();
            };
            pMenu.Items.Add(item1);
            pMenu.Items.Add(item2);
            //Canvas
            Setting_Canvases[This_Image_Index].Name = "Setting_Canvas_" + Now_Sound_Index;
            Setting_Canvases[This_Image_Index].VerticalAlignment = VerticalAlignment.Top;
            Setting_Canvases[This_Image_Index].HorizontalAlignment = HorizontalAlignment.Left;
            Setting_Canvases[This_Image_Index].Width = 1;
            Setting_Canvases[This_Image_Index].Height = 1;
            Setting_Canvases[This_Image_Index].Focusable = false;
            Setting_Canvases[This_Image_Index].Margin = new Thickness(0, Set_Top, 0, 0);
            //波形
            Sound_Images[This_Image_Index].Name = "Sound_Wave_Image_" + Now_Sound_Index;
            Sound_Images[This_Image_Index].VerticalAlignment = VerticalAlignment.Top;
            Sound_Images[This_Image_Index].HorizontalAlignment = HorizontalAlignment.Left;
            Sound_Images[This_Image_Index].Width = 800;
            Sound_Images[This_Image_Index].Height = 125;
            Sound_Images[This_Image_Index].Focusable = false;
            Sound_Images[This_Image_Index].Margin = new Thickness(0, Set_Top, 0, 0);
            Sound_Images[This_Image_Index].Stretch = System.Windows.Media.Stretch.Fill;
            Sound_Images[This_Image_Index].Source = Image_Wave;
            Sound_Images[This_Image_Index].MouseLeftButtonDown += delegate
            {
                IsSoundMoving = true;
                Sound_Select_Change(Now_Sound_Index);
            };
            Sound_Images[This_Image_Index].MouseRightButtonUp += delegate
            {
                Sound_Select_Change(Now_Sound_Index);
            };
            Sound_Images[This_Image_Index].ContextMenu = pMenu;
            //左の線
            Border_Lines[This_Image_Index].Name = "Border_Line_" + Now_Sound_Index;
            Border_Lines[This_Image_Index].VerticalAlignment = VerticalAlignment.Top;
            Border_Lines[This_Image_Index].HorizontalAlignment = HorizontalAlignment.Left;
            Border_Lines[This_Image_Index].Width = 250;
            Border_Lines[This_Image_Index].Height = 1;
            Border_Lines[This_Image_Index].Focusable = false;
            Border_Lines[This_Image_Index].Background = Brushes.Aqua;
            Border_Lines[This_Image_Index].BorderBrush = Brushes.Aqua;
            Border_Lines[This_Image_Index].BorderThickness = new Thickness(1);
            Border_Lines[This_Image_Index].Margin = new Thickness(5, 125, 0, 0);
            Border_Lines[This_Image_Index].MouseRightButtonUp += delegate
            {
                Sound_Select_Change(Now_Sound_Index);
            };
            Border_Lines[This_Image_Index].MouseLeftButtonUp += delegate
            {
                Sound_Select_Change(Now_Sound_Index);
            };
            Border_Lines[This_Image_Index].ContextMenu = pMenu;
            //一列選択する用のBorder
            Sound_Select_Lines[This_Image_Index].Name = "Sound_Select_Line_" + Now_Sound_Index;
            Sound_Select_Lines[This_Image_Index].VerticalAlignment = VerticalAlignment.Top;
            Sound_Select_Lines[This_Image_Index].HorizontalAlignment = HorizontalAlignment.Left;
            Sound_Select_Lines[This_Image_Index].Width = 258;
            Sound_Select_Lines[This_Image_Index].Height = 125;
            Sound_Select_Lines[This_Image_Index].Focusable = false;
            Sound_Select_Lines[This_Image_Index].Background = Brushes.White;
            Sound_Select_Lines[This_Image_Index].BorderBrush = Brushes.Aqua;
            Sound_Select_Lines[This_Image_Index].BorderThickness = new Thickness(1);
            Sound_Select_Lines[This_Image_Index].Margin = new Thickness(0, 0, 0, 0);
            Sound_Select_Lines[This_Image_Index].Opacity = 0;
            Sound_Select_Lines[This_Image_Index].ContextMenu = pMenu;
            Sound_Select_Lines[This_Image_Index].MouseRightButtonUp += delegate
            {
                Sound_Select_Change(Now_Sound_Index);
            };
            Sound_Select_Lines[This_Image_Index].MouseLeftButtonUp += delegate
            {
                Sound_Select_Change(Now_Sound_Index);
            };
            //音量バー
            Sound_Volumes[This_Image_Index].Name = "Volume_Slider_" + Now_Sound_Index;
            Sound_Volumes[This_Image_Index].VerticalAlignment = VerticalAlignment.Top;
            Sound_Volumes[This_Image_Index].HorizontalAlignment = HorizontalAlignment.Left;
            Sound_Volumes[This_Image_Index].Width = 175;
            Sound_Volumes[This_Image_Index].Height = 25;
            Sound_Volumes[This_Image_Index].Focusable = false;
            Sound_Volumes[This_Image_Index].Style = (Style)(this.Resources["CustomSliderStyle_Yoko"]);
            Sound_Volumes[This_Image_Index].Maximum = 100;
            if (Set_Volume != -1)
                Sound_Volumes[This_Image_Index].Value = Set_Volume;
            else
                Sound_Volumes[This_Image_Index].Value = 75;
            Sound_Volumes[This_Image_Index].Margin = new Thickness(40, 90, 0, 0);
            Sound_Volumes[This_Image_Index].ValueChanged += delegate
            {
                int Index_Temp = -1;
                for (int Number = 0; Number < Sound_Images.Count; Number++)
                {
                    if (int.Parse(Sound_Images[Number].Name.Substring(Sound_Images[Number].Name.LastIndexOf('_') + 1)) == Now_Sound_Index)
                    {
                        Index_Temp = Number;
                        break;
                    }
                }
                if (Index_Temp == -1)
                    return;
                Bass.BASS_ChannelSetAttribute(Sound_Streams[Index_Temp], BASSAttribute.BASS_ATTRIB_VOL, (float)(Sound_Volumes[Index_Temp].Value / 100 * All_Volume_Set));
            };
            //曲名
            Sound_Names[This_Image_Index].Name = "Sound_Title_" + Now_Sound_Index;
            Sound_Names[This_Image_Index].Text = Path.GetFileName(File_Now);
            Sound_Names[This_Image_Index].VerticalAlignment = VerticalAlignment.Top;
            Sound_Names[This_Image_Index].HorizontalAlignment = HorizontalAlignment.Left;
            Sound_Names[This_Image_Index].Width = 260;
            Sound_Names[This_Image_Index].Height = 85;
            Sound_Names[This_Image_Index].Focusable = false;
            Sound_Names[This_Image_Index].Foreground = Brushes.Aqua;
            Sound_Names[This_Image_Index].FontSize = 25;
            Sound_Names[This_Image_Index].TextWrapping = TextWrapping.Wrap;
            Sound_Names[This_Image_Index].TextAlignment = TextAlignment.Center;
            Sound_Names[This_Image_Index].Margin = new Thickness(0, 5, 0, 0);
            Sound_Names[This_Image_Index].MouseRightButtonUp += delegate
            {
                Sound_Select_Change(Now_Sound_Index);
            };
            Sound_Names[This_Image_Index].MouseLeftButtonUp += delegate
            {
                Sound_Select_Change(Now_Sound_Index);
            };
            Sound_Names[This_Image_Index].ContextMenu = pMenu;
            Child_Canvas.Children.Add(Sound_Images[This_Image_Index]);
            //画面内に表示
            Setting_Canvases[This_Image_Index].Children.Add(Sound_Select_Lines[This_Image_Index]);
            Setting_Canvases[This_Image_Index].Children.Add(Border_Lines[This_Image_Index]);
            Setting_Canvases[This_Image_Index].Children.Add(Sound_Volumes[This_Image_Index]);
            Setting_Canvases[This_Image_Index].Children.Add(Sound_Names[This_Image_Index]);
            Setting_Canvas_Main.Children.Add(Setting_Canvases[This_Image_Index]);
            //カットされたサウンドの場合はカット元の位置の下に来るように
            if (Add_Index_Pos != -1)
            {
                int Set_Top_New = 0;
                //カット元のY座標を計算
                for (int Number = 0; Number < Sound_Images_Y_Pos.Count; Number++)
                {
                    if (Sound_Images_Y_Pos[Number] > Sound_Images_Y_Pos[Add_Index_Pos - 1])
                    {
                        Sound_Images[Number].Margin = new Thickness(Sound_Images[Number].Margin.Left, Sound_Images[Number].Margin.Top + 135, 0, 0);
                        Setting_Canvases[Number].Margin = new Thickness(0, Setting_Canvases[Number].Margin.Top + 135, 0, 0);
                        Sound_Images_Y_Pos[Number] += 135;
                        Setting_Canvas_Y_Pos[Number] += 135;
                    }
                    if (Sound_Images[Number].Margin.Top == Sound_Images[Add_Index_Pos - 1].Margin.Top)
                    {
                        Set_Top_New = Sound_Images_Y_Pos[Add_Index_Pos - 1];
                        Sound_Images[This_Image_Index].Margin = new Thickness(0, Set_Top_New + 135, 0, 0);
                        Setting_Canvases[This_Image_Index].Margin = new Thickness(0, Set_Top_New + 135, 0, 0);
                        Sound_Images_Y_Pos[This_Image_Index] = Set_Top_New + 135;
                        Setting_Canvas_Y_Pos[This_Image_Index] = Set_Top_New + 135;
                    }
                }
            }
            //全体の高さが600以上の場合Y軸を動かすバーを表示
            if (Set_Top + 125 > 600)
            {
                if (Track_Scrool.Visibility == Visibility.Hidden)
                    Track_Scrool.Visibility = Visibility.Visible;
                Track_Scrool.Maximum = Set_Top - 470;
            }
            Set_Time_Slider((int)Time_Scrool.Value);
            IsBusy = false;
        }
        //指定したIndexを選択状態に
        void Sound_Select_Change(int Index)
        {
            //正確なIndexを取得
            int Index_Temp = -1;
            for (int Number = 0; Number < Sound_Index_Info.Count; Number++)
            {
                if (Sound_Index_Info[Number].Sound_Index == Index)
                {
                    Index_Temp = Sound_Index_Info[Number].Add_Sound_Index;
                    break;
                }
            }
            if (Index_Temp == -1)
                return;
            //Ctrlが押されていない場合はすべての選択を解除
            if (!IsControlMode)
                Sound_Select_Unlock();
            if (Index_Temp != -1)
            {
                //どれが選択されているかを保存
                if (!IsControlMode || IsDeleted)
                {
                    Sound_Selected_Index.Clear();
                    Image_Point.Clear();
                    IsDeleted = false;
                }
                if (!Sound_Selected_Index.Contains(Index_Temp))
                {
                    Sound_Selected_Index.Add(Index_Temp);
                    Image_Point.Add(new Point(Sound_Images[Index_Temp].Margin.Left, Sound_Images[Index_Temp].Margin.Top));
                }
                else
                    Image_Point[Sound_Selected_Index.IndexOf(Index_Temp)] = new Point(Sound_Images[Index_Temp].Margin.Left, Sound_Images[Index_Temp].Margin.Top);
                Mouse_Point = new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                IsImageClicked = true;
            }
            //左の枠を若干白くさせる
            Sound_Select_Lines[Index_Temp].Opacity = 0.5;
            IsSoundSelected = true;
        }
        private void Parent_Dock_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            if (IsImageClicked && Sound_Selected_Index.Count > 0)
            {
                //波形を移動
                double X_Plus = System.Windows.Forms.Cursor.Position.X - Mouse_Point.X;
                for (int Number = 0; Number < Sound_Selected_Index.Count; Number++)
                {
                    Sound_Images[Sound_Selected_Index[Number]].Margin = new Thickness(Sound_Images[Sound_Selected_Index[Number]].Margin.Left + X_Plus, Sound_Images[Sound_Selected_Index[Number]].Margin.Top, 0, 0);
                    Sound_Positions[Sound_Selected_Index[Number]] = (int)Sound_Images[Sound_Selected_Index[Number]].Margin.Left;
                }
                Mouse_Point = new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
            }
            if (IsTimeMoveMode)
            {
                double X_Plus = System.Windows.Forms.Cursor.Position.X - Mouse_Point.X;
                if (Time_Line_Left + X_Plus >= 0 && Time_Line_Left + X_Plus <= 1395)
                {
                    Time_Line.Margin = new Thickness(Time_Line_Left + X_Plus, Time_Line.Margin.Top, 0, 0);
                    double Time_Left_Percent = (Time_Line.Margin.Left + 1400 * Time_Line_Move_Width_Scrool) / 1400;
                    double Max_Time_Seconds = Time_Info[(int)Time_Scrool.Value].Times[Time_Info[(int)Time_Scrool.Value].Times.Count - 1];
                    if (Time_Info[(int)Time_Scrool.Value].Unit == "分")
                        Max_Time_Seconds *= 60;
                    else if (Time_Info[(int)Time_Scrool.Value].Unit == "時間")
                        Max_Time_Seconds *= 60 * 60;
                    Play_Time = Max_Time_Seconds * Time_Left_Percent;
                }
            }
        }
        private void Parent_Dock_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            if (IsImageClicked)
            {
                //移動後の位置を保存
                double Max_Time_Seconds = Time_Info[(int)Time_Scrool.Value].Times[Time_Info[(int)Time_Scrool.Value].Times.Count - 1];
                if (Time_Info[(int)Time_Scrool.Value].Unit == "分")
                    Max_Time_Seconds *= 60;
                else if (Time_Info[(int)Time_Scrool.Value].Unit == "時間")
                    Max_Time_Seconds *= 60 * 60;
                if (Sound_Selected_Index.Count > 0)
                {
                    if (Image_Point[0].X != Sound_Images[Sound_Selected_Index[0]].Margin.Left)
                    {
                        //元の位置と移動後の位置が異なっている場合保存&再生位置を変更
                        for (int Number = 0; Number < Sound_Selected_Index.Count; Number++)
                        {
                            double Parcent_X = (Sound_Positions[Sound_Selected_Index[Number]] + 1400 * Time_Line_Move_Width_Scrool) / 1400;
                            Sound_Positions[Sound_Selected_Index[Number]] = Max_Time_Seconds * Parcent_X;
                            Image_Point[Number] = new Point(Sound_Images[Sound_Selected_Index[Number]].Margin.Left, Sound_Images[Sound_Selected_Index[Number]].Margin.Top);
                        }
                        IsSoundPosChanged = true;
                    }
                    if (Image_Point[0].X != Sound_Images[Sound_Selected_Index[0]].Margin.Left && !IsControlMode)
                    {
                        IsSoundPosChanged = true;
                    }
                }
                IsImageClicked = false;
                IsSoundMoving = false;
            }
            if (IsSoundSelected && !IsImageClicked)
            {
                IsSoundSelected = false;
            }
            else
                Sound_Select_Unlock();
            if (IsTimeMoveMode)
            {
                IsTimeMoveMode = false;
                double Percent = (Time_Line.Margin.Left + 1400 * Time_Line_Move_Width_Scrool) / 1400;
                double Max_Time_Seconds = Time_Info[(int)Time_Scrool.Value].Times[Time_Info[(int)Time_Scrool.Value].Times.Count - 1];
                if (Time_Info[(int)Time_Scrool.Value].Unit == "分")
                    Max_Time_Seconds *= 60;
                else if (Time_Info[(int)Time_Scrool.Value].Unit == "時間")
                    Max_Time_Seconds *= 60 * 60;
                Play_Time = Max_Time_Seconds * Percent;
                if (IsTimeMoveMode_IsPlaying)
                {
                    IsPlaying = true;
                    All_Sound_Play(10f);
                }
                IsTimeMoveMode_IsPlaying = false;
            }
        }
        private void Track_Scrool_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //Y軸を変更
            for (int Number = 0; Number < Sound_Images.Count; Number++)
            {
                Sound_Images[Number].Margin = new Thickness(Sound_Images[Number].Margin.Left, Sound_Images_Y_Pos[Number] - Track_Scrool.Value, 0, 0);
                Setting_Canvases[Number].Margin = new Thickness(0, Setting_Canvas_Y_Pos[Number] - Track_Scrool.Value, 0, 0);
            }
        }
        private void Time_Scrool_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //時間バーの値が変更されたら実行
            Time_Text_Change((int)Time_Scrool.Value);
        }
        void Set_Time_Slider(int Time_Info_Index)
        {
            //横の時間バーの設定
            Time_Side_Scrool.Value = 0;
            double Max_Time_Now = Time_Info[Time_Info_Index].Times[Time_Info[Time_Info_Index].Times.Count - 1];
            double Time_Side_Time = Max_Time_Now - Time_Info[Time_Info_Index].Times[Time_Info[Time_Info_Index].Times.Count - 2];
            Time_Side_Values.Clear();
            double Add_Time_Length = 1;
            Time_Side_Time = Math.Round(Time_Side_Time, 1, MidpointRounding.AwayFromZero);
            if (Time_Side_Time == 0.2 || Time_Side_Time == 0.4)
            {
                Add_Time_Length = Time_Side_Time;
            }
            double Max_Stream_Length = Get_Max_Stream_Length();
            while (true)
            {
                if (Max_Time_Now + Add_Time_Length >= 3600 && Time_Info[Time_Info_Index].Unit == "秒")
                    Max_Time_Now = 3600;
                else if (Max_Time_Now + Add_Time_Length >= 60)
                    if (Add_Time_Length != 1 || Time_Info[Time_Info_Index].Unit == "分")
                        Max_Time_Now = 60;
                Time_Side_Values.Add(Max_Time_Now);
                if (Max_Time_Now == 60 && Add_Time_Length != 1)
                    break;
                else if (Time_Info[Time_Info_Index].Unit == "秒")
                {
                    if (Max_Time_Now == 3600 || Max_Time_Now >= Max_Stream_Length)
                        break;
                }
                else if (Time_Info[Time_Info_Index].Unit == "分")
                    if (Max_Time_Now == 60 || Max_Time_Now >= Max_Stream_Length / 60)
                        break;
                Max_Time_Now += Add_Time_Length;
            }
            if (Time_Side_Values.Count > 0)
                Time_Side_Scrool.Maximum = Time_Side_Values.Count - 1;
        }
        //時間軸を反映
        void Time_Text_Change(int Time_Info_Index)
        {
            Set_Time_Slider(Time_Info_Index);
            //時間テキストの位置を設定
            if (Time_Info[Time_Info_Index].Times.Count == 2)
            {
                Time_Text[0].Margin = new Thickness(-3310, 250, 0, 0);
                Time_Text[1].Margin = new Thickness(-510, 250, 0, 0);
                Time_Text[2].Visibility = Visibility.Hidden;
                Time_Text[3].Visibility = Visibility.Hidden;
                Time_Text[4].Visibility = Visibility.Hidden;
                Time_Text[5].Visibility = Visibility.Hidden;
            }
            else if (Time_Info[Time_Info_Index].Times.Count == 4)
            {
                Time_Text[2].Visibility = Visibility.Visible;
                Time_Text[3].Visibility = Visibility.Visible;
                Time_Text[0].Margin = new Thickness(-3310, 250, 0, 0);
                Time_Text[1].Margin = new Thickness(-2376.7, 250, 0, 0);
                Time_Text[2].Margin = new Thickness(-1443.4, 250, 0, 0);
                Time_Text[3].Margin = new Thickness(-510, 250, 0, 0);
                Time_Text[4].Visibility = Visibility.Hidden;
                Time_Text[5].Visibility = Visibility.Hidden;
            }
            else if (Time_Info[Time_Info_Index].Times.Count == 5)
            {
                Time_Text[2].Visibility = Visibility.Visible;
                Time_Text[3].Visibility = Visibility.Visible;
                Time_Text[4].Visibility = Visibility.Visible;
                Time_Text[0].Margin = new Thickness(-3310, 250, 0, 0);
                Time_Text[1].Margin = new Thickness(-2610, 250, 0, 0);
                Time_Text[2].Margin = new Thickness(-1910, 250, 0, 0);
                Time_Text[3].Margin = new Thickness(-1210, 250, 0, 0);
                Time_Text[4].Margin = new Thickness(-510, 250, 0, 0);
                Time_Text[5].Visibility = Visibility.Hidden;
            }
            else if (Time_Info[Time_Info_Index].Times.Count == 6)
            {
                for (int Number = 0; Number < 6; Number++)
                {
                    Time_Text[Number].Visibility = Visibility.Visible;
                    Time_Text[Number].Margin = new Thickness(-3310 + 556 * Number, 250, 0, 0);
                }
            }
            //
            for (int Number = 0; Number < Time_Info[Time_Info_Index].Times.Count; Number++)
                Time_Text[Number].Text = Time_Info[Time_Info_Index].Times[Number].ToString();
            Time_Text[6].Text = "単位:" + Time_Info[Time_Info_Index].Unit;
            //タイムラインの白いラインの位置を変更
            if (!IsPlaying && !IsSoundMoving)
            {
                double Max_Time_Seconds = Time_Info[(int)Time_Scrool.Value].Times[Time_Info[(int)Time_Scrool.Value].Times.Count - 1];
                if (Time_Info[(int)Time_Scrool.Value].Unit == "分")
                    Max_Time_Seconds *= 60;
                else if (Time_Info[(int)Time_Scrool.Value].Unit == "時間")
                    Max_Time_Seconds *= 60 * 60;
                double Parcent = Play_Time / Max_Time_Seconds;
                Time_Line.Margin = new Thickness(1400 * Parcent, Time_Line.Margin.Top, 0, 0);
            }
            Set_Sound_Width(Time_Info[Time_Info_Index]);
        }
        //マウスホイールでズームやスクロールができるように
        private void Parent_Dock_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            if (e.Delta > 0 && (int)Time_Scrool.Value > 0)
            {
                if (IsControlMode && !IsImageClicked && !IsTimeMoveMode)
                    Time_Scrool.Value = (int)Time_Scrool.Value - 1;
                else
                {
                    if (Track_Scrool.Value - 135 < 0)
                        Track_Scrool.Value = 0;
                    else
                        Track_Scrool.Value -= 135;
                }
            }
            else if (e.Delta < 0)
            {
                if (IsControlMode && !IsImageClicked && Time_Scrool.Value < (int)Time_Scrool.Maximum)
                    Time_Scrool.Value = (int)Time_Scrool.Value + 1;
                else
                {
                    if (Track_Scrool.Value + 135 > Track_Scrool.Maximum)
                        Track_Scrool.Value = Track_Scrool.Maximum;
                    else
                        Track_Scrool.Value += 135;
                }
            }
        }
        //すべてのサウンドを停止
        async void All_Sound_Pause(float Feed_Time = 0)
        {
            if (Feed_Time == 0)
            {
                for (int Number = 0; Number < Sound_Streams.Count; Number++)
                {
                    Bass.BASS_ChannelPause(Sound_Streams[Number]);
                    Bass.BASS_ChannelSetAttribute(Sound_Streams[Number], BASSAttribute.BASS_ATTRIB_VOL, 0f);
                }
            }
            else
            {
                List<float> Volume_Now = new List<float>();
                List<float> Volume_Minus = new List<float>();
                for (int Number = 0; Number < Sound_Streams.Count; Number++)
                {
                    float Volume_Now_Stream = 1f;
                    Bass.BASS_ChannelGetAttribute(Sound_Streams[Number], BASSAttribute.BASS_ATTRIB_VOL, ref Volume_Now_Stream);
                    Volume_Now.Add(Volume_Now_Stream);
                    Volume_Minus.Add(Volume_Now_Stream / Feed_Time);
                }
                IsFeedIn = false;
                IsFeedOut = true;
                while (IsFeedOut)
                {
                    List<int> End_Index = new List<int>();
                    for (int Number = 0; Number < Sound_Streams.Count; Number++)
                    {
                        if (End_Index.Contains(Number))
                            continue;
                        Volume_Now[Number] -= Volume_Minus[Number];
                        if (Volume_Now[Number] < 0f)
                        {
                            Volume_Now[Number] = 0f;
                            End_Index.Add(Number);
                        }
                        Bass.BASS_ChannelSetAttribute(Sound_Streams[Number], BASSAttribute.BASS_ATTRIB_VOL, (float)(Volume_Now[Number] * All_Volume_Set));
                    }
                    if (End_Index.Count >= Sound_Streams.Count)
                    {
                        End_Index.Clear();
                        break;
                    }
                    await Task.Delay(1000 / 60);
                }
                for (int Number = 0; Number < Sound_Streams.Count; Number++)
                {
                    Bass.BASS_ChannelPause(Sound_Streams[Number]);
                    Bass.BASS_ChannelSetAttribute(Sound_Streams[Number], BASSAttribute.BASS_ATTRIB_VOL, 0f);
                }
                IsFeedOut = false;
            }
        }
        async void All_Sound_Play(float Feed_Time = 0)
        {
            if (Feed_Time == 0)
                for (int Number = 0; Number < Sound_Streams.Count; Number++)
                    Bass.BASS_ChannelSetAttribute(Sound_Streams[Number], BASSAttribute.BASS_ATTRIB_VOL, (float)(Sound_Volumes[Number].Value / 100 * All_Volume_Set));
            else
            {
                List<float> Volume_Now = new List<float>();
                List<float> Volume_Plus = new List<float>();
                for (int Number = 0; Number < Sound_Streams.Count; Number++)
                {
                    float Volume_Now_Stream = 1f;
                    Bass.BASS_ChannelGetAttribute(Sound_Streams[Number], BASSAttribute.BASS_ATTRIB_VOL, ref Volume_Now_Stream);
                    Volume_Now.Add(Volume_Now_Stream);
                    Volume_Plus.Add((float)Sound_Volumes[Number].Value / 100f / Feed_Time);
                }
                IsFeedIn = true;
                IsFeedOut = false;
                while (IsFeedIn)
                {
                    List<int> End_Index = new List<int>();
                    for (int Number = 0; Number < Sound_Streams.Count; Number++)
                    {
                        Volume_Now[Number] += Volume_Plus[Number];
                        if (Volume_Now[Number] > 1f)
                        {
                            Volume_Now[Number] = 1f;
                            End_Index.Add(Number);
                        }
                        else if (Volume_Now[Number] >= (float)(Sound_Volumes[Number].Value / 100))
                        {
                            Volume_Now[Number] = (float)(Sound_Volumes[Number].Value / 100);
                            End_Index.Add(Number);
                        }
                        Bass.BASS_ChannelSetAttribute(Sound_Streams[Number], BASSAttribute.BASS_ATTRIB_VOL, (float)(Volume_Now[Number] * All_Volume_Set));
                    }
                    if (End_Index.Count >= Sound_Streams.Count)
                    {
                        End_Index.Clear();
                        break;
                    }
                    await Task.Delay(1000 / 60);
                }
                IsFeedIn = false;
            }
        }
        //タイムラインの時間単位によって波形の横の長さを調整
        void Set_Sound_Width(Time_Relation Time_R)
        {
            double Max_Time_Seconds = Time_R.Times[Time_R.Times.Count - 1];
            double Max_Time_Now = Time_Info[(int)Time_Scrool.Value].Times[Time_Info[(int)Time_Scrool.Value].Times.Count - 1];
            double Time_Side_Time = Max_Time_Now - Time_Info[(int)Time_Scrool.Value].Times[Time_Info[(int)Time_Scrool.Value].Times.Count - 2];
            double Min_Time_Text = Time_Side_Values[(int)Time_Side_Scrool.Value] - Time_Side_Time * (Time_Info[(int)Time_Scrool.Value].Times.Count - 1);
            Time_Line_Move_Width_Scrool = Min_Time_Text / Max_Time_Seconds;
            if (Time_R.Unit == "分")
                Max_Time_Seconds *= 60;
            else if (Time_R.Unit == "時間")
                Max_Time_Seconds *= 60 * 60;
            for (int Number = 0; Number < Sound_Images.Count; Number++)
            {
                double Stream_Time_Seconds = Bass.BASS_ChannelBytes2Seconds(Sound_Streams[Number], Bass.BASS_ChannelGetLength(Sound_Streams[Number], BASSMode.BASS_POS_BYTES));
                double Parcent_Width = Stream_Time_Seconds / Max_Time_Seconds * Sound_Minus_Play_Time[Number];
                Sound_Images[Number].Width = 1400 * Parcent_Width;
                if (!IsSoundMoving)
                {
                    double Percent_X = Sound_Positions[Number] / Max_Time_Seconds;
                    Sound_Images[Number].Margin = new Thickness(1400 * Percent_X - 1400 * Time_Line_Move_Width_Scrool, Sound_Images[Number].Margin.Top, 0, 0);
                }
            }
        }
        //再生
        private void Music_Start_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            if (!IsPlaying)
            {
                IsPlaying = true;
                All_Sound_Play(10f);
            }
        }
        //停止
        private void Music_Pause_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            if (IsPlaying)
            {
                All_Sound_Pause();
                IsPlaying = false;
            }
        }
        //マイナス
        private void Music_Minus_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            if (IsControlMode)
                Play_Time -= 10;
            else
                Play_Time -= 5;
            if (Play_Time < 0)
                Play_Time = 0;
            if (!IsPlaying)
            {
                double Max_Time_Seconds = Time_Info[(int)Time_Scrool.Value].Times[Time_Info[(int)Time_Scrool.Value].Times.Count - 1];
                if (Time_Info[(int)Time_Scrool.Value].Unit == "分")
                    Max_Time_Seconds *= 60;
                else if (Time_Info[(int)Time_Scrool.Value].Unit == "時間")
                    Max_Time_Seconds *= 60 * 60;
                double Parcent = Play_Time / Max_Time_Seconds;
                Time_Line.Margin = new Thickness(1400 * Parcent - 1400 * Time_Line_Move_Width_Scrool, Time_Line.Margin.Top, 0, 0);
            }
            IsSoundPosChanged = true;
        }
        //プラス
        private void Music_Plus_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            if (IsControlMode)
                Play_Time += 10;
            else
                Play_Time += 5;
            IsSoundPosChanged = true;
            if (!IsPlaying)
            {
                double Max_Time_Seconds = Time_Info[(int)Time_Scrool.Value].Times[Time_Info[(int)Time_Scrool.Value].Times.Count - 1];
                if (Time_Info[(int)Time_Scrool.Value].Unit == "分")
                    Max_Time_Seconds *= 60;
                else if (Time_Info[(int)Time_Scrool.Value].Unit == "時間")
                    Max_Time_Seconds *= 60 * 60;
                double Parcent = Play_Time / Max_Time_Seconds;
                Time_Line.Margin = new Thickness(1400 * Parcent - 1400 * Time_Line_Move_Width_Scrool, Time_Line.Margin.Top, 0, 0);
            }
        }
        //Pitchを変更
        private void Pitch_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Play_Pitch_Percent = (float)(Pitch_S.Value / 50);
            Pitch_T.Text = "速度:" + (int)Pitch_S.Value;
            for (int Number = 0; Number < Sound_Streams.Count; Number++)
                Bass.BASS_ChannelSetAttribute(Sound_Streams[Number], BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, Sound_Frequencys[Number] * Play_Pitch_Percent);
        }
        private void Pitch_S_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Pitch_Value = Pitch_S.Value;
        }
        private void Pitch_S_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Pitch_Change_Await();
        }
        private void Pitch_S_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Pitch_S.Value != 50)
            {
                Pitch_S.Value = 50;
                Pitch_Change_Await();
            }
        }
        async void Pitch_Change_Await()
        {
            await Task.Delay(500);
            IsSoundPosChanged = true;
        }
        //選択状態を解除
        void Sound_Select_Unlock()
        {
            for (int Number = 0; Number < Sound_Select_Lines.Count; Number++)
                Sound_Select_Lines[Number].Opacity = 0;
            Sound_Selected_Index.Clear();
            Image_Point.Clear();
        }
        //選択している項目をすべて削除
        void Sound_Remove_Index()
        {
            Sound_Selected_Index.Sort();
            int Number_Back = 0;
            for (int Number_01 = 0; Number_01 < Sound_Selected_Index.Count; Number_01++)
            {
                int Index = Sound_Selected_Index[Number_01] - Number_Back;
                int Error_Number = 0;
                try
                {
                    Bass.BASS_ChannelStop(Sound_Streams[Index]);
                    Bass.BASS_StreamFree(Sound_Streams[Index]);
                    Error_Number++;
                    //選択されている項目のY座標より大きいサウンドを1つ上に上げる(語彙力)
                    if (Sound_Images.Count - 1 >= Index)
                    {
                        for (int Number = 0; Number < Sound_Images.Count; Number++)
                        {
                            if (Sound_Images[Number].Margin.Top > Sound_Images[Index].Margin.Top)
                            {
                                Sound_Images[Number].Margin = new Thickness(Sound_Images[Number].Margin.Left, Sound_Images[Number].Margin.Top - 135, 0, 0);
                                Setting_Canvases[Number].Margin = new Thickness(0, Setting_Canvases[Number].Margin.Top - 135, 0, 0);
                                Sound_Images_Y_Pos[Number] -= 135;
                                Setting_Canvas_Y_Pos[Number] -= 135;
                            }
                        }
                    }
                    for (int Number = 0; Number < Sound_Index_Info.Count; Number++)
                    {
                        if (Sound_Index_Info[Number].Add_Sound_Index == Index)
                        {
                            Sound_Index_Info.RemoveAt(Number);
                            break;
                        }
                    }
                    for (int Number = 0; Number < Sound_Index_Info.Count; Number++)
                    {
                        if (Sound_Index_Info[Number].Add_Sound_Index >= Index)
                            Sound_Index_Info[Number].Add_Sound_Index--;
                    }
                    //それぞれの項目を削除(エラーが怒る可能性があるためError_Numberを増やしていきどこでエラーが起こったかわかりやすくしています)
                    Error_Number++;
                    Child_Canvas.Children.Remove(Sound_Images[Index]);
                    Error_Number++;
                    Setting_Canvas_Main.Children.Remove(Setting_Canvases[Index]);
                    Error_Number++;
                    Sound_Images[Index].Source = null;
                    Error_Number++;
                    Sound_Images.RemoveAt(Index);
                    Error_Number++;
                    Border_Lines.RemoveAt(Index);
                    Error_Number++;
                    Sound_Select_Lines.RemoveAt(Index);
                    Error_Number++;
                    Sound_Volumes.RemoveAt(Index);
                    Error_Number++;
                    Sound_Names.RemoveAt(Index);
                    Error_Number++;
                    Sound_Positions.RemoveAt(Index);
                    Error_Number++;
                    Sound_Images_Y_Pos.RemoveAt(Index);
                    Error_Number++;
                    Setting_Canvas_Y_Pos.RemoveAt(Index);
                    Error_Number++;
                    Sound_Streams.RemoveAt(Index);
                    Error_Number++;
                    Sound_Frequencys.RemoveAt(Index);
                    Error_Number++;
                    Setting_Canvases.RemoveAt(Index);
                    Error_Number++;
                    Sound_Files.RemoveAt(Index);
                    Error_Number++;
                    Sound_Plus_Play_Time.RemoveAt(Index);
                    Sound_Minus_Play_Time.RemoveAt(Index);
                    Sound_Max_Length.RemoveAt(Index);
                    Error_Number++;
                    //Y軸のスクロールバーを調整
                    if (Track_Scrool.Visibility == Visibility.Visible)
                    {
                        if (Track_Scrool.Value > Track_Scrool.Maximum - 135)
                            Track_Scrool.Value = Track_Scrool.Maximum - 135;
                        if (Track_Scrool.Maximum > 135)
                            Track_Scrool.Maximum -= 135;
                        else
                        {
                            Track_Scrool.Value = 0;
                            Track_Scrool.Maximum = 0;
                            Track_Scrool.Visibility = Visibility.Hidden;
                        }
                    }
                    Number_Back++;
                }
                catch
                {
                    MessageBox.Show("正常に処理できませんでした。次の文字を開発者に送信してください。\n" + Index + " | " + (Sound_Images.Count - 1) + "\n + Error_Code:" + Error_Number);
                }
            }
            Sound_Selected_Index.Clear();
            IsDeleted = true;
        }
        //サウンドをカット
        private async void Music_Cut_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            bool IsExist = false;
            if (Sound_Selected_Index.Count > 1)
                Message_Feed_Out("トラックが複数選択されています。");
            if (IsDeleted)
            {
                Message_Feed_Out("カットするトラックが選択されていません。");
                return;
            }
            foreach (int Index in Sound_Selected_Index)
            {
                IsExist = true;
                //選択されているサウンドの波形がタイムラインバーの中にある場合実行
                if (Time_Line.Margin.Left >= Sound_Images[Index].Margin.Left && Time_Line.Margin.Left < Sound_Images[Index].Margin.Left + Sound_Images[Index].Width)
                {
                    //複雑な計算(説明が難しいため詳しく書きません)
                    double Size_Percent = ((Time_Line.Margin.Left + 1400 * Time_Line_Move_Width_Scrool) - (Sound_Images[Index].Margin.Left + 1400 * Time_Line_Move_Width_Scrool)) / Sound_Images[Index].Width;
                    BitmapSource From_Image;
                    BitmapSource To_Image;
                    Sub_Code.Resize_From_BitmapImage((BitmapSource)Sound_Images[Index].Source, (int)(Sound_Images[Index].Source.Width * Size_Percent), (int)Sound_Images[Index].Source.Height, out From_Image, out To_Image);
                    Sound_Images[Index].Source = From_Image;
                    if (Setting_Window.Cut_Volume_Sync_C.IsChecked.Value)
                        await Add_Sound(Sound_Files[Index], To_Image, Index + 1, Sound_Volumes[Sound_Selected_Index[0]].Value);
                    else
                        await Add_Sound(Sound_Files[Index], To_Image, Index + 1, Setting_Window.Volume_S.Value);
                    double Max_Time_Seconds = Time_Info[(int)Time_Scrool.Value].Times[Time_Info[(int)Time_Scrool.Value].Times.Count - 1];
                    if (Time_Info[(int)Time_Scrool.Value].Unit == "分")
                        Max_Time_Seconds *= 60;
                    else if (Time_Info[(int)Time_Scrool.Value].Unit == "時間")
                        Max_Time_Seconds *= 60 * 60;
                    Sound_Images[Index].Width *= Size_Percent;
                    Sound_Minus_Play_Time[Sound_Images.Count - 1] = Sound_Minus_Play_Time[Index] * (1 - Size_Percent);
                    double Sound_Max_Secounds = Bass.BASS_ChannelBytes2Seconds(Sound_Streams[Sound_Images.Count - 1], Bass.BASS_ChannelGetLength(Sound_Streams[Sound_Images.Count - 1], BASSMode.BASS_POS_BYTES));
                    double Plus_Width = Sound_Images[Index].Width;
                    if (Setting_Window.Cut_Pos_C.IsChecked.Value)
                        Plus_Width = 0;
                    double Parcent_X = (Sound_Images[Index].Margin.Left + 1400 * Time_Line_Move_Width_Scrool + Plus_Width) / 1400;
                    Sound_Positions[Sound_Images.Count - 1] = Max_Time_Seconds * Parcent_X;
                    Sound_Images[Sound_Images.Count - 1].Margin = new Thickness(Sound_Images[Index].Margin.Left + 1400 * Time_Line_Move_Width_Scrool + Plus_Width, Sound_Images[Index].Margin.Top + 135, 0, 0);
                    Setting_Canvases[Sound_Images.Count - 1].Margin = new Thickness(0, Setting_Canvases[Index].Margin.Top + 135, 0, 0);
                    Sound_Images_Y_Pos[Sound_Images.Count - 1] = Sound_Images_Y_Pos[Index] + 135;
                    Setting_Canvas_Y_Pos[Sound_Images.Count - 1] = Setting_Canvas_Y_Pos[Index] + 135;
                    Setting_Canvases[Sound_Images.Count - 1].Margin = new Thickness(0, Setting_Canvases[Index].Margin.Top + 135, 0, 0);
                    Sound_Plus_Play_Time[Sound_Images.Count - 1] = Play_Time - Sound_Positions[Index] + Sound_Plus_Play_Time[Index];
                    Sound_Minus_Play_Time[Index] *= Size_Percent;
                    if (Setting_Window.Cut_Volume_C.IsChecked.Value)
                        Sound_Volumes[Index].Value = 0;
                    Set_Sound_Width(Time_Info[(int)Time_Scrool.Value]);
                }
                else
                    Message_Feed_Out("トラックが範囲外です。");
            }
            if (!IsExist)
                Message_Feed_Out("カットするトラックが選択されていません。");
        }
        private void Time_Side_Scrool_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //横軸のバーが変更されたら実行
            if (Visibility != Visibility.Visible)
                return;
            double Max_Time_Now = Time_Info[(int)Time_Scrool.Value].Times[Time_Info[(int)Time_Scrool.Value].Times.Count - 1];
            double Time_Side_Time = Max_Time_Now - Time_Info[(int)Time_Scrool.Value].Times[Time_Info[(int)Time_Scrool.Value].Times.Count - 2];
            if (Time_Info[(int)Time_Scrool.Value].Times.Count == 4)
            {
                double Count_Zero = Math.Round(Time_Side_Values[(int)Time_Side_Scrool.Value] - Time_Side_Time * 3, 1, MidpointRounding.AwayFromZero);
                double Count_One = Math.Round(Time_Side_Values[(int)Time_Side_Scrool.Value] - Time_Side_Time * 2, 1, MidpointRounding.AwayFromZero);
                double Count_Two = Math.Round(Time_Side_Values[(int)Time_Side_Scrool.Value] - Time_Side_Time * 1, 1, MidpointRounding.AwayFromZero);
                double Count_Three = Math.Round(Time_Side_Values[(int)Time_Side_Scrool.Value], 1, MidpointRounding.AwayFromZero);
                Time_Text[0].Text = Count_Zero.ToString();
                Time_Text[1].Text = Count_One.ToString();
                Time_Text[2].Text = Count_Two.ToString();
                Time_Text[3].Text = Count_Three.ToString();
            }
            else if (Time_Info[(int)Time_Scrool.Value].Times.Count == 6)
            {
                double Count_Zero = Math.Round(Time_Side_Values[(int)Time_Side_Scrool.Value] - Time_Side_Time * 5, 1, MidpointRounding.AwayFromZero);
                double Count_One = Math.Round(Time_Side_Values[(int)Time_Side_Scrool.Value] - Time_Side_Time * 4, 1, MidpointRounding.AwayFromZero);
                double Count_Two = Math.Round(Time_Side_Values[(int)Time_Side_Scrool.Value] - Time_Side_Time * 3, 1, MidpointRounding.AwayFromZero);
                double Count_Three = Math.Round(Time_Side_Values[(int)Time_Side_Scrool.Value] - Time_Side_Time * 2, 1, MidpointRounding.AwayFromZero);
                double Count_Four = Math.Round(Time_Side_Values[(int)Time_Side_Scrool.Value] - Time_Side_Time * 1, 1, MidpointRounding.AwayFromZero);
                double Count_Five = Math.Round(Time_Side_Values[(int)Time_Side_Scrool.Value], 1, MidpointRounding.AwayFromZero);
                Time_Text[0].Text = Count_Zero.ToString();
                Time_Text[1].Text = Count_One.ToString();
                Time_Text[2].Text = Count_Two.ToString();
                Time_Text[3].Text = Count_Three.ToString();
                Time_Text[4].Text = Count_Four.ToString();
                Time_Text[5].Text = Count_Five.ToString();
            }
            Set_Sound_Width(Time_Info[(int)Time_Scrool.Value]);
            if (Time_Info[(int)Time_Scrool.Value].Unit == "分")
                Max_Time_Now *= 60;
            else if (Time_Info[(int)Time_Scrool.Value].Unit == "時間")
                Max_Time_Now *= 60 * 60;
            double Parcent = Play_Time / Max_Time_Now;
            Time_Line.Margin = new Thickness(1400 * Parcent - 1400 * Time_Line_Move_Width_Scrool, Time_Line.Margin.Top, 0, 0);
        }
        //現在追加されているサウンドの中で、一番長いサウンドの最大秒数を取得(トラックの開始位置は考慮しません)
        double Get_Max_Stream_Length()
        {
            double Length = 0;
            for (int Stream_Now = 0; Stream_Now < Sound_Streams.Count; Stream_Now++)
            {
                double Temp = Sound_Positions[Stream_Now] + Bass.BASS_ChannelBytes2Seconds(Sound_Streams[Stream_Now], Bass.BASS_ChannelGetLength(Sound_Streams[Stream_Now], BASSMode.BASS_POS_BYTES));
                if (Temp > Length)
                    Length = Temp;
            }
            return Length;
        }
        //保存先のファイル名を連番にするとき用
        void Set_Serial_Number(string File_Path)
        {
            if (!Sub_Code.File_Exists(File_Path + "1") && !Sub_Code.File_Exists(File_Path + "01") && !Sub_Code.File_Exists(File_Path + "001"))
            {
                Save_Serial_Number = 1;
                return;
            }
            Save_Serial_Number = 2;
            if (Setting_Window.Save_File_Mode_C.SelectedIndex == 2)
            {
                while (true)
                {
                    if (Sub_Code.File_Exists(File_Path + Save_Serial_Number.ToString()))
                        Save_Serial_Number++;
                    else
                        break;
                }
            }
            else if (Setting_Window.Save_File_Mode_C.SelectedIndex == 1)
            {
                bool IsExist = true;
                for (int Hundred = 0; Hundred < 10; Hundred++)
                {
                    for (int Ten = 0; Ten < 10; Ten++)
                    {
                        for (int One = 1; One < 10; One++)
                        {
                            if (Hundred == 0)
                            {
                                if (Sub_Code.File_Exists(File_Path + Ten.ToString() + One.ToString()))
                                    Save_Serial_Number++;
                                else
                                {
                                    IsExist = false;
                                    break;
                                }
                            }
                            else
                            {
                                if (Sub_Code.File_Exists(File_Path + Hundred.ToString() + Ten.ToString() + One.ToString()))
                                    Save_Serial_Number++;
                                else
                                {
                                    IsExist = false;
                                    break;
                                }
                            }
                        }
                        if (!IsExist)
                            break;
                    }
                    if (!IsExist)
                        break;
                }
            }
            else if (Setting_Window.Save_File_Mode_C.SelectedIndex == 0)
            {
                bool IsExist = true;
                for (int Hundred = 0; Hundred < 10; Hundred++)
                {
                    for (int Ten = 0; Ten < 10; Ten++)
                    {
                        for (int One = 1; One < 10; One++)
                        {
                            if (Sub_Code.File_Exists(File_Path + Hundred.ToString() + Ten.ToString() + One.ToString()))
                                Save_Serial_Number++;
                            else
                            {
                                MessageBox.Show(File_Path + Hundred.ToString() + Ten.ToString() + One.ToString());
                                IsExist = false;
                                break;
                            }
                        }
                        if (!IsExist)
                            break;
                    }
                    if (!IsExist)
                        break;
                }
            }
            else
                Save_Serial_Number = 2;
            Save_Serial_Number--;
        }
        //保存先のファイル名を現在時刻にするとき用
        string Get_Now_Time_To_File_Path(string File_Path)
        {
            string Time = DateTime.Now.Hour.ToString() + DateTime.Now.Minute + DateTime.Now.Second;
            if (Sub_Code.File_Exists(File_Path + Time))
            {
                System.Threading.Thread.Sleep(1000);
                return Get_Now_Time_To_File_Path(File_Path + Time);
            }
            return Time;
        }
        //トラックを書き出す
        private void Music_Save_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            if (Sound_Images.Count == 0)
            {
                Message_Feed_Out("トラックが存在しません。");
                return;
            }
            else if (Save_Combo.SelectedIndex == 1 && Sound_Selected_Index.Count == 0)
            {
                Message_Feed_Out("選択中のトラックが存在しません。");
                return;
            }
            if (!Setting_Window.Save_Once_C.IsChecked.Value && Setting_Window.Save_Dir == "")
            {
                Message_Feed_Out("詳細設定から保存先のフォルダを指定してください。");
                return;
            }
            else if (!Setting_Window.Save_Once_C.IsChecked.Value && Setting_Window.Save_File_Name_T.Text == "")
            {
                Message_Feed_Out("詳細設定から保存先のファイル名を設定してください。");
                return;
            }
            if (Pitch_S.Value < 10 && Setting_Window.Set_Speed_Mode_C.IsChecked.Value)
            {
                Message_Feed_Out("速度が10以下の場合は保存できません。");
                return;
            }
            else if (Setting_Window.Save_Track_Delete_C.IsChecked.Value)
            {
                bool IsMessage = false;
                if (Save_Combo.SelectedIndex == 1 && Sound_Selected_Index.Count > 1)
                    IsMessage = true;
                else if (Save_Combo.SelectedIndex == 0 && Sound_Images.Count > 1)
                    IsMessage = true;
                if (IsMessage)
                {
                    Message_Feed_Out("詳細設定の'保存後トラックを削除'にチェックを入れている場合、複数のトラックを書き出すことはできません。");
                    return;
                }
            }
            //ファイル名を事前に決めておく
            if (!Setting_Window.Save_Once_C.IsChecked.Value)
            {
                string To_File = Setting_Window.Save_Dir + "\\" + Setting_Window.Save_File_Name_T.Text;
                if (Setting_Window.Save_File_Mode_C.SelectedIndex < 3)
                {
                    if (Setting_Window.Save_File_Mode_C.SelectedIndex == 2)
                    {
                        if (Sub_Code.File_Exists(To_File + Save_Serial_Number.ToString()))
                        {
                            Set_Serial_Number(To_File);
                            Music_Save_B.PerformClick();
                            return;
                        }
                        else
                            To_File += Save_Serial_Number.ToString();
                    }
                    else if (Setting_Window.Save_File_Mode_C.SelectedIndex == 1)
                    {
                        if (Save_Serial_Number < 10)
                        {
                            if (Sub_Code.File_Exists(To_File + "0" + Save_Serial_Number.ToString()))
                            {
                                Set_Serial_Number(To_File);
                                Music_Save_B.PerformClick();
                                return;
                            }
                            else
                                To_File += "0" + Save_Serial_Number.ToString();
                        }
                        else
                        {
                            if (Sub_Code.File_Exists(To_File + Save_Serial_Number))
                            {
                                Set_Serial_Number(To_File);
                                Music_Save_B.PerformClick();
                                return;
                            }
                            else
                                To_File += Save_Serial_Number.ToString();
                        }
                    }
                    else if (Setting_Window.Save_File_Mode_C.SelectedIndex == 0)
                    {
                        if (Save_Serial_Number < 10)
                        {
                            if (Sub_Code.File_Exists(To_File + "00" + Save_Serial_Number.ToString()))
                            {
                                Set_Serial_Number(To_File);
                                Music_Save_B.PerformClick();
                                return;
                            }
                            else
                                To_File += "00" + Save_Serial_Number.ToString();
                        }
                        else if (Save_Serial_Number < 100)
                        {
                            if (Sub_Code.File_Exists(To_File + "0" + Save_Serial_Number.ToString()))
                            {
                                Set_Serial_Number(To_File);
                                Music_Save_B.PerformClick();
                                return;
                            }
                            else
                                To_File += "0" + Save_Serial_Number.ToString();
                        }
                        else
                        {
                            if (Sub_Code.File_Exists(To_File + Save_Serial_Number.ToString()))
                            {
                                Set_Serial_Number(To_File);
                                Music_Save_B.PerformClick();
                                return;
                            }
                            else
                                To_File += Save_Serial_Number.ToString();
                        }
                    }
                    Save_Serial_Number++;
                }
                else if (Setting_Window.Save_File_Mode_C.SelectedIndex == 3)
                    To_File += Get_Now_Time_To_File_Path(To_File);
                else if (Setting_Window.Save_File_Mode_C.SelectedIndex == 4)
                    To_File += Sub_Code.Generate_Random_String(To_File, 2, 6);
                To_File += Setting_Window.Save_Ex_C.SelectedItem;
                //生成されたファイル名で出力
                Sound_Export(To_File);
            }
            else
            {
                //保存先を指定して出力
                System.Windows.Forms.SaveFileDialog ofd = new System.Windows.Forms.SaveFileDialog()
                {
                    Title = "保存先を指定してください。",
                    Filter = "サウンドファイル(*" + Setting_Window.Save_Ex_C.Items[Setting_Window.Save_Ex_C.SelectedIndex] + ";)|*" + Setting_Window.Save_Ex_C.Items[Setting_Window.Save_Ex_C.SelectedIndex] + ";"
                };
                if (Save_File_Dir != "")
                    ofd.InitialDirectory = Save_File_Dir;
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    Sound_Export(ofd.FileName);
                ofd.Dispose();
            }
        }
        //トラックをファイルに出力
        async void Sound_Export(string To_File)
        {
            //再生中の場合停止させる
            IsPlaying = false;
            IsBusy = true;
            All_Sound_Pause();
            Save_File_Dir = Path.GetDirectoryName(To_File);
            Configs_Save();
            Message_T.Text = "ファイルに書き出しています...";
            await Task.Delay(50);
            try
            {
                //トラック情報を一時的に保存
                List<string> Output_Files = new List<string>();
                List<double> Pos = new List<double>();
                List<double> Volumes = new List<double>();
                List<double> Speeds = new List<double>();
                //すべてのトラックを保存
                if (Save_Combo.SelectedIndex == 0)
                {
                    for (int Number = 0; Number < Sound_Images.Count; Number++)
                    {
                        if (Sound_Volumes[Number].Value > 0)
                        {
                            //トラックの元ファイルをコピー
                            string Out_File = Voice_Set.Special_Path + "\\Encode_Mp3\\Sound_Editor_TMP_" + Output_Files.Count + Path.GetExtension(Sound_Files[Number]);
                            File.Copy(Sound_Files[Number], Out_File, true);
                            Output_Files.Add(Out_File);
                            double Pos_Now = Sound_Positions[Number] / (Pitch_S.Value / 50);
                            if (Pos_Now >= 0)
                                Pos.Add(Pos_Now);
                            else
                                Pos.Add(0);
                            Volumes.Add(Sound_Volumes[Number].Value);
                            if (Setting_Window.Set_Speed_Mode_C.IsChecked.Value)
                                Speeds.Add(Pitch_S.Value / 50);
                            else
                                Speeds.Add(1.0);
                            double Stream_Time_Seconds = Bass.BASS_ChannelBytes2Seconds(Sound_Streams[Number], Bass.BASS_ChannelGetLength(Sound_Streams[Number], BASSMode.BASS_POS_BYTES));
                            double End_Time = Stream_Time_Seconds * Sound_Minus_Play_Time[Number];
                            if (Sound_Minus_Play_Time[Number] < 1)
                            {
                                //ffmpegを使用して指定時間以外をカット(再生開始位置が0秒より小さかった場合処理を少し変更させています)
                                if (Pos_Now < 0)
                                    ffmpeg.Sound_Cut_From_To(Out_File, Out_File, Sound_Plus_Play_Time[Number] - Pos_Now, Sound_Plus_Play_Time[Number] + End_Time);
                                else
                                    ffmpeg.Sound_Cut_From_To(Out_File, Out_File, Sound_Plus_Play_Time[Number], Sound_Plus_Play_Time[Number] + End_Time);
                            }
                        }
                    }
                }
                //選択中のトラックのみ保存
                else
                {
                    foreach (int Index in Sound_Selected_Index)
                    {
                        if (Sound_Volumes[Index].Value > 0)
                        {
                            string Out_File = Voice_Set.Special_Path + "\\Encode_Mp3\\Sound_Editor_TMP_" + Output_Files.Count + Path.GetExtension(Sound_Files[Index]);
                            File.Copy(Sound_Files[Index], Out_File, true);
                            Output_Files.Add(Out_File);
                            double Pos_Now = Sound_Positions[Index] / (Pitch_S.Value / 50);
                            if (Pos_Now >= 0)
                                Pos.Add(Pos_Now);
                            else
                                Pos.Add(0);
                            Volumes.Add(Sound_Volumes[Index].Value);
                            if (Setting_Window.Set_Speed_Mode_C.IsChecked.Value)
                                Speeds.Add(Pitch_S.Value / 50);
                            else
                                Speeds.Add(1.0);
                            double Stream_Time_Seconds = Bass.BASS_ChannelBytes2Seconds(Sound_Streams[Index], Bass.BASS_ChannelGetLength(Sound_Streams[Index], BASSMode.BASS_POS_BYTES));
                            double End_Time = Stream_Time_Seconds * Sound_Minus_Play_Time[Index];
                            if (Sound_Minus_Play_Time[Index] < 1)
                            {
                                if (Pos_Now < 0)
                                    ffmpeg.Sound_Cut_From_To(Out_File, Out_File, Sound_Plus_Play_Time[Index] - Pos_Now, Sound_Plus_Play_Time[Index] + End_Time);
                                else
                                    ffmpeg.Sound_Cut_From_To(Out_File, Out_File, Sound_Plus_Play_Time[Index], Sound_Plus_Play_Time[Index] + End_Time);
                            }
                        }
                    }
                }
                //カットしたファイル同士をつなげる
                if (Setting_Window.Save_Ex_C.Items[Setting_Window.Save_Ex_C.SelectedIndex].ToString() == ".mp3")
                    ffmpeg.Sound_Combine(Output_Files, Pos, Volumes, Speeds, To_File, true, true);
                else
                    ffmpeg.Sound_Combine(Output_Files, Pos, Volumes, Speeds, To_File, false, true);
                foreach (string FIle_Now in Output_Files)
                    if (File.Exists(FIle_Now))
                        File.Delete(FIle_Now);
                //クリア
                Output_Files.Clear();
                Pos.Clear();
                Volumes.Clear();
                Speeds.Clear();
                if (Setting_Window.Save_Track_Delete_C.IsChecked.Value)
                {
                    if (Save_Combo.SelectedIndex == 0)
                    {
                        Sound_Selected_Index.Clear();
                        Sound_Selected_Index.Add(0);
                        Sound_Remove_Index();
                    }
                    else if (Save_Combo.SelectedIndex == 1)
                        Sound_Remove_Index();
                }
                Message_Feed_Out("正常に書き出されました。");
            }
            catch (Exception e1)
            {
                Sub_Code.Error_Log_Write(e1.Message);
                Message_Feed_Out("エラーが発生しました。詳しくはError_Log.txtを参照してください。");
            }
            IsBusy = false;
        }
        //ファイル選択の場所と保存先の場所を記録
        void Configs_Save()
        {
            try
            {
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Configs/Sound_Editor.tmp");
                stw.WriteLine(Add_File_Dir);
                stw.Write(Save_File_Dir);
                stw.Close();
                Sub_Code.File_Encrypt(Voice_Set.Special_Path + "/Configs/Sound_Editor.tmp", Voice_Set.Special_Path + "/Configs/Sound_Editor.conf", "Sound_Editor_Configs_Save", true);
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        //詳細設定の画面を表示(FPSは瞬時に反映されるように)
        private async void Setting_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            int FPS_Index = Setting_Window.Framerate_C.SelectedIndex;
            Setting_Window.Window_Show();
            while (Setting_Window.Visibility == Visibility.Visible)
            {
                if (FPS_Index != Setting_Window.Framerate_C.SelectedIndex)
                {
                    FPS_Index = Setting_Window.Framerate_C.SelectedIndex;
                    if (FPS_Index == 0)
                        FPS = 30;
                    else if (FPS_Index == 1)
                        FPS = 60;
                    else if (FPS_Index == 2)
                        FPS = 120;
                }
                await Task.Delay(500);
            }
            Set_Serial_Number(Setting_Window.Save_Dir + "\\" + Setting_Window.Save_File_Name_T.Text);
        }
        //クリックした位置に再生時間を移動
        private void Time_Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IsTimeMoveMode = true;
            Mouse_Point = new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
            if (IsPlaying)
            {
                IsTimeMoveMode_IsPlaying = true;
                IsPlaying = false;
                All_Sound_Pause();
            }
            Time_Line.Margin = new Thickness(e.GetPosition(Time_Border).X, Time_Line.Margin.Top, 0, 0);
            Time_Line_Left = Time_Line.Margin.Left;
            double Time_Left_Percent = (Time_Line.Margin.Left + 1400 * Time_Line_Move_Width_Scrool) / 1400;
            double Max_Time_Seconds = Time_Info[(int)Time_Scrool.Value].Times[Time_Info[(int)Time_Scrool.Value].Times.Count - 1];
            if (Time_Info[(int)Time_Scrool.Value].Unit == "分")
                Max_Time_Seconds *= 60;
            else if (Time_Info[(int)Time_Scrool.Value].Unit == "時間")
                Max_Time_Seconds *= 60 * 60;
            Play_Time = Max_Time_Seconds * Time_Left_Percent;
        }
        //選択中のトラックを0秒に戻す
        void Sound_Move_Zero()
        {
            foreach (int Number_01 in Sound_Selected_Index)
            {
                Sound_Positions[Number_01] = 0;
                Set_Sound_Width(Time_Info[(int)Time_Scrool.Value]);
                Bass.BASS_ChannelSetPosition(Sound_Streams[Number_01], Play_Time + Sound_Plus_Play_Time[Number_01]);
            }
        }
        //ファイルにトラック情報を保存
        private async void Save_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            if (Sound_Images.Count == 0)
            {
                Message_Feed_Out("保存する内容がありません。");
                return;
            }
            IsPlaying = false;
            All_Sound_Pause();
            await Task.Delay(100);
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog()
            {
                Title = "保存先を指定してください。",
                InitialDirectory = Directory.GetCurrentDirectory(),
                Filter = "セーブファイル(*.wse)|*.wse"
            };
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!Sub_Code.CanDirectoryAccess(Path.GetDirectoryName(sfd.FileName)))
                {
                    Message_Feed_Out("指定したフォルダにアクセスできませんでした。");
                    sfd.Dispose();
                    return;
                }
                try
                {
                    StreamWriter stw = File.CreateText(sfd.FileName + ".tmp");
                    stw.WriteLine(Pitch_S.Value);
                    for (int Index = 0; Index < Sound_Images.Count; Index++)
                    {
                        stw.WriteLine(Sound_Files[Index] + "|" + Sound_Volumes[Index].Value + "|" + Sound_Images_Y_Pos[Index] + "|" + Sound_Positions[Index] + "|" + Sound_Plus_Play_Time[Index] + "|" +
                            Sound_Minus_Play_Time[Index]);
                    }
                    stw.Write("WoTB_Sound_Editor_Save_File_End");
                    stw.Close();
                    stw.Dispose();
                    Sub_Code.File_Encrypt(sfd.FileName + ".tmp", sfd.FileName, "Sound_Editor_Save_File", true);
                    Message_Feed_Out("内容を保存しました。");
                }
                catch (Exception e1)
                {
                    Sub_Code.Error_Log_Write(e1.Message);
                    Message_Feed_Out("エラーが発生しました。Error_Log.txtを参照してください。");
                }
            }
            sfd.Dispose();
        }
        //ファイルをロード
        private async void Load_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            IsPlaying = false;
            All_Sound_Pause();
            await Task.Delay(100);
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog()
            {
                Title = "セーブファイルを指定してください。",
                Multiselect = false,
                InitialDirectory = Directory.GetCurrentDirectory(),
                Filter = "セーブファイル(*.wse)|*.wse"
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                Contents_Load(ofd.FileName);
            ofd.Dispose();
        }
        //保存したトラック情報をロード
        public async void Contents_Load(string File_Path)
        {
            IsLoading = true;
            if (IsPlaying)
            {
                IsPlaying = false;
                All_Sound_Pause();
                await Task.Delay(100);
            }
            if (!File.Exists(File_Path))
            {
                Message_Feed_Out("指定したファイルが見つかりませんでした。");
                IsBusy = false;
                return;
            }
            //既に存在するトラックをすべてクリア
            Sound_Selected_Index.Clear();
            for (int Number = 0; Number < Sound_Images.Count; Number++)
                Sound_Selected_Index.Add(Number);
            Sound_Remove_Index();
            IsDeleted = false;
            Play_Time = 0;
            try
            {
                //.wseファイルから読み込む
                Sub_Code.File_Decrypt_To_File(File_Path, File_Path + ".tmp", "Sound_Editor_Save_File", false);
                string[] Read_Lines = File.ReadAllLines(File_Path + ".tmp");
                File.Delete(File_Path + ".tmp");
                //1行目は速度の設定
                Pitch_S.Value = double.Parse(Read_Lines[0]);
                //トラックの数だけループ
                List<ImageSource> Temp_Images = new List<ImageSource>();
                List<string> Temp_Files = new List<string>();
                List<string> Add_Files = new List<string>();
                List<string> Add_All = new List<string>();
                for (int Number = 1; Number < Read_Lines.Length; Number++)
                {
                    if (Read_Lines[Number].Contains("WoTB_Sound_Editor_Save_File_End"))
                        break;
                    string[] Split = Read_Lines[Number].Split('|');
                    string Dir_Name = Path.GetDirectoryName(Split[0]) + "\\" + Path.GetFileNameWithoutExtension(Split[0]);
                    if (Add_Files.Contains(Split[0]))
                    {
                        int Index_Temp = Add_Files.LastIndexOf(Split[0]);
                        Add_All.Insert(Index_Temp + 1, Read_Lines[Number]);
                    }
                    else if (Add_Files.Contains(Split[0]))
                    {
                        int Index_Temp = Add_Files.LastIndexOf(Split[0]);
                        Add_All.Insert(Index_Temp + 1, Read_Lines[Number]);
                    }
                    else
                        Add_All.Add(Read_Lines[Number]);
                    Add_Files.Add(Split[0]);
                }
                Add_Files.Clear();
                foreach (string Temp_01 in Add_All)
                {
                    string[] Split = Temp_01.Split('|');
                    string Dir_Name = Path.GetDirectoryName(Split[0]) + "\\" + Path.GetFileNameWithoutExtension(Split[0]);
                    Message_T.Text = "波形を生成しています...";
                    await Task.Delay(50);
                    ImageSource Image_Wave = null;
                    string File_Path_No_Ex;
                    if (Temp_Files.Contains(Dir_Name + ".wav"))
                    {
                        int Index_Image = Temp_Files.IndexOf(Dir_Name + ".wav");
                        Image_Wave = Temp_Images[Index_Image];
                        File_Path_No_Ex = Dir_Name + ".wav";
                    }
                    else if (Temp_Files.Contains(Dir_Name + ".mp3"))
                    {
                        int Index_Image = Temp_Files.IndexOf(Dir_Name + ".mp3");
                        Image_Wave = Temp_Images[Index_Image];
                        File_Path_No_Ex = Dir_Name + ".mp3";
                    }
                    else
                    {
                        if (File.Exists(Dir_Name + ".wav"))
                            Temp_Files.Add(Dir_Name + ".wav");
                        else if (File.Exists(Dir_Name + ".mp3"))
                            Temp_Files.Add(Dir_Name + ".mp3");
                        else
                            Temp_Files.Add(Split[0]);
                        int StreamHandle = Bass.BASS_StreamCreateFile(Temp_Files[Temp_Files.Count - 1], 0, 0, BASSFlag.BASS_STREAM_DECODE);
                        if (Bass.BASS_ChannelBytes2Seconds(StreamHandle, Bass.BASS_ChannelGetLength(StreamHandle, BASSMode.BASS_POS_BYTES)) > 600)
                            Image_Wave = Temp_WaveForm;
                        else
                            Image_Wave = Sub_Code.BassRenderWaveForm(Temp_Files[Temp_Files.Count - 1]);
                        Bass.BASS_StreamFree(StreamHandle);
                        Temp_Images.Add(Image_Wave);
                        File_Path_No_Ex = Temp_Files[Temp_Files.Count - 1];
                    }
                    if (Image_Wave == null)
                        continue;
                    Message_T.Text = "サウンドを読み込んでいます...";
                    await Task.Delay(50);
                    await Add_Sound(File_Path_No_Ex, Image_Wave, -1, double.Parse(Split[1]), int.Parse(Split[2]));
                    int Index = Sound_Images.Count - 1;
                    Sound_Positions[Index] = double.Parse(Split[3]);
                    Sound_Plus_Play_Time[Index] = double.Parse(Split[4]);
                    Sound_Minus_Play_Time[Index] = double.Parse(Split[5]);
                    //波形をカット
                    double Start_Pos = 1920 * Sound_Plus_Play_Time[Index] / Sound_Max_Length[Index];
                    double End_Pos = 1920 * Sound_Minus_Play_Time[Index] + Start_Pos;
                    BitmapSource Temp;
                    Sub_Code.Resize_From_BitmapImage((BitmapSource)Sound_Images[Index].Source, (int)(End_Pos - Start_Pos), (int)Sound_Images[Index].Source.Height, (int)Start_Pos, out Temp);
                    Sound_Images[Index].Source = Temp;
                    //波形の横の長さや配置場所を設定するため実行
                    Set_Sound_Width(Time_Info[(int)Time_Scrool.Value]);
                    Time_Line.Margin = new Thickness(-1400 * Time_Line_Move_Width_Scrool, Time_Line.Margin.Top, 0, 0);
                }
                Temp_Images.Clear();
                Temp_Files.Clear();
                Add_All.Clear();
                Message_Feed_Out("ロードしました。");
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                Message_Feed_Out("エラーが発生しました。");
                Sound_Selected_Index.Clear();
                for (int Number = 0; Number < Sound_Images.Count; Number++)
                    Sound_Selected_Index.Add(Number);
                Sound_Remove_Index();
                IsDeleted = false;
            }
            IsBusy = false;
            IsLoading = false;
        }
    }
}