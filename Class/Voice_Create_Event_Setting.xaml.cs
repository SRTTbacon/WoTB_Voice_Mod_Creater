using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Flac;
using Un4seen.Bass.AddOn.Fx;
using Un4seen.Bass.Misc;

namespace WoTB_Voice_Mod_Creater.Class
{
    public partial class Voice_Create_Event_Setting : UserControl
    {
        Voice_Event_Setting Settings = null;
        Voice_Event_Setting Settings_Source = null;
        BASS_BFX_BQF Voice_LPF_Setting = new BASS_BFX_BQF(BASSBFXBQF.BASS_BFX_BQF_LOWPASS, 12000f, 0f, 0f, 1f, 0f, BASSFXChan.BASS_BFX_CHANALL);
        BASS_BFX_BQF Event_LPF_Setting = new BASS_BFX_BQF(BASSBFXBQF.BASS_BFX_BQF_LOWPASS, 12000f, 0f, 0f, 1f, 0f, BASSFXChan.BASS_BFX_CHANALL);
        BASS_BFX_BQF Voice_HPF_Setting = new BASS_BFX_BQF(BASSBFXBQF.BASS_BFX_BQF_HIGHPASS, 0f, 0f, 0f, 1f, 0f, BASSFXChan.BASS_BFX_CHANALL);
        BASS_BFX_BQF Event_HPF_Setting = new BASS_BFX_BQF(BASSBFXBQF.BASS_BFX_BQF_HIGHPASS, 0f, 0f, 0f, 1f, 0f, BASSFXChan.BASS_BFX_CHANALL);
        DSP_Gain Voice_Gain = null;
        DSP_Gain Event_Gain = null;
        WVS_Load WVS_File = null;
        SE_Setting seSetting = null;
        SYNCPROC IsMusicEnd;
        GCHandle Sound_IntPtr;
        List<int> Streams = new List<int>();
        byte[] Sound_Bytes;
        int Stream_Mix;
        int Max_Time_Stream;
        int Stream_Voice_LPF;
        int Stream_Event_LPF;
        int Stream_Voice_HPF;
        int Stream_Event_HPF;
        int List_Index_Mode = -1;
        int Index = -1;
        int Selected_Voice_Index = -1;
        int HPF_Before_Value = 0;
        int LPF_Before_Value = 0;
        float Mix_Freq = 44100f;
        float Voice_Freq = 44100f;
        string Event_Name = "";
        string Max_Time = "00:00";
        public bool IsClosing = false;
        bool IsMessageShowing = false;
        bool IsLocationChanging = false;
        bool IsPaused = false;
        bool IsPlayingMouseDown = false;
        bool IsEnded = false;
        bool IsLeftKeyDown = false;
        bool IsRightKeyDown = false;
        bool IsSpaceKeyDown = false;
        bool IsLControlKeyDown = false;
        bool IsHPFMoving = false;
        bool IsLPFMoving = false;
        public Voice_Create_Event_Setting()
        {
            InitializeComponent();
            When_Limit_C.Items.Add("Kill Voice");
            When_Limit_C.Items.Add("Use virtual voice settings");
            When_Priority_C.Items.Add("Discard oldest instance");
            When_Priority_C.Items.Add("Discard newest instance");
            When_Limit_C.SelectedIndex = 0;
            When_Priority_C.SelectedIndex = 0;
            Limit_T.Text = "50";
            All_Volume_S.Value = 75;
            Position_S.AddHandler(MouseDownEvent, new MouseButtonEventHandler(Position_MouseDown), true);
            Position_S.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Position_MouseUp), true);
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Volume_C.Source = Sub_Code.Check_01;
            Pitch_C.Source = Sub_Code.Check_01;
            LPF_C.Source = Sub_Code.Check_01;
            HPF_C.Source = Sub_Code.Check_01;
        }
        public async void Window_Show(string Event_Name, Voice_Event_Setting Settings, WVS_Load WVS_File, int List_Index_Mode, int Index, SE_Setting seSetting)
        {
            IsClosing = false;
            this.Event_Name = Event_Name;
            this.List_Index_Mode = List_Index_Mode;
            this.Index = Index;
            Settings_Source = Settings.Clone();
            this.Settings = Settings;
            this.WVS_File = WVS_File;
            this.seSetting = seSetting;
            Opacity = 0;
            Visibility = Visibility.Visible;
            Position_Change();
            if (Settings.IsVolumeRange)
            {
                Volume_Start_S.Value = Settings.Volume_Range.Start;
                Volume_End_S.Value = Settings.Volume_Range.End;
                Volume_C.Source = Sub_Code.Check_03;
            }
            else
            {
                Volume_Start_S.Value = Settings.Volume;
                Volume_C.Source = Sub_Code.Check_01;
            }
            if (Settings.IsPitchRange)
            {
                Pitch_Start_S.Value = Settings.Pitch_Range.Start;
                Pitch_End_S.Value = Settings.Pitch_Range.End;
                Pitch_C.Source = Sub_Code.Check_03;
            }
            else
            {
                Pitch_Start_S.Value = Settings.Pitch;
                Pitch_C.Source = Sub_Code.Check_01;
            }
            if (Settings.IsLPFRange)
            {
                LPF_Start_S.Value = Settings.LPF_Range.Start;
                LPF_End_S.Value = Settings.LPF_Range.End;
                LPF_C.Source = Sub_Code.Check_03;
            }
            else
            {
                LPF_Start_S.Value = Settings.Low_Pass_Filter;
                LPF_C.Source = Sub_Code.Check_01;
            }
            if (Settings.IsHPFRange)
            {
                HPF_Start_S.Value = Settings.HPF_Range.Start;
                HPF_End_S.Value = Settings.HPF_Range.End;
                HPF_C.Source = Sub_Code.Check_03;
            }
            else
            {
                HPF_Start_S.Value = Settings.High_Pass_Filter;
                HPF_C.Source = Sub_Code.Check_01;
            }
            Delay_S.Value = Settings.Delay;
            When_Limit_C.SelectedIndex = Settings.When_Limit_Reached;
            When_Priority_C.SelectedIndex = Settings.When_Priority_Equal;
            Limit_T.Text = Settings.Limit_Sound_Instance.ToString();
            Change_Range_Mode();
            while (Opacity < 1 && !IsClosing)
            {
                Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
        }
        async void Position_Change()
        {
            double nextFrame = (double)Environment.TickCount;
            float period = 1000f / 30f;
            while (Visibility == Visibility.Visible)
            {
                double tickCount = (double)Environment.TickCount;
                if (tickCount < nextFrame)
                {
                    if (nextFrame - tickCount > 1)
                        await Task.Delay((int)(nextFrame - tickCount));
                    System.Windows.Forms.Application.DoEvents();
                    continue;
                }
                bool IsPlaying = (Bass.BASS_ChannelIsActive(Stream_Mix) == BASSActive.BASS_ACTIVE_PLAYING && !IsLocationChanging) ? true : false;
                if (IsPlaying && Max_Time_Stream != -1)
                {
                    Bass.BASS_ChannelUpdate(Stream_Mix, 400);
                    double Start_Time = Settings.Sounds[Selected_Voice_Index].Play_Time.Start_Time;
                    double End_Time = Settings.Sounds[Selected_Voice_Index].Play_Time.End_Time;
                    if (End_Time != 0 && Position_S.Value + Settings.Sounds[Selected_Voice_Index].Play_Time.Start_Time >= End_Time)
                    {
                        if (Max_Time_Stream == 1)
                        {
                            Bass.BASS_ChannelStop(Stream_Mix);
                            Bass.BASS_ChannelSetPosition(Stream_Mix, 0);
                            Bass.BASS_ChannelSetPosition(Streams[0], 0);
                            Bass.BASS_ChannelSetPosition(Streams[1], Settings.Sounds[Selected_Voice_Index].Play_Time.Start_Time);
                            Bass.BASS_ChannelSetAttribute(Streams[1], BASSAttribute.BASS_ATTRIB_VOL, 1f);
                        }
                        else
                            Bass.BASS_ChannelStop(Streams[1]);
                    }
                    long position = Bass.BASS_ChannelGetPosition(Streams[Max_Time_Stream]);
                    if (Max_Time_Stream == 1)
                        Position_S.Value = Bass.BASS_ChannelBytes2Seconds(Streams[Max_Time_Stream], position) - Start_Time;
                    else
                        Position_S.Value = Bass.BASS_ChannelBytes2Seconds(Streams[Max_Time_Stream], position);
                    if (End_Time != 0 && Settings.Sounds[Selected_Voice_Index].IsFadeOut && End_Time - Start_Time > 0.5 && End_Time - Start_Time - Position_S.Value <= 0.6)
                        _ = Pause_Volume_Animation(false, 25f, Streams[1]);
                    Position_T.Text = Sub_Code.Get_Time_String(Position_S.Value) + " / " + Max_Time;
                }
                if (Sub_Code.IsForcusWindow && !IsClosing)
                {
                    bool IsLeft_or_RightPushed = false;
                    if ((Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) > 0)
                    {
                        Sound_Minus_B.Content = "-10秒";
                        Sound_Plus_B.Content = "+10秒";
                        IsLControlKeyDown = true;
                    }
                    else if (IsLControlKeyDown)
                    {
                        Sound_Minus_B.Content = "-5秒";
                        Sound_Plus_B.Content = "+5秒";
                        IsLControlKeyDown = false;
                    }
                    if ((Keyboard.GetKeyStates(Key.Left) & KeyStates.Down) > 0)
                    {
                        if (!IsLeftKeyDown && !IsLeft_or_RightPushed)
                            Sound_Minus_B.PerformClick();
                        IsLeftKeyDown = true;
                        IsLeft_or_RightPushed = true;
                    }
                    else
                        IsLeftKeyDown = false;
                    if ((Keyboard.GetKeyStates(Key.Right) & KeyStates.Down) > 0)
                    {
                        if (!IsRightKeyDown && !IsLeft_or_RightPushed)
                            Sound_Plus_B.PerformClick();
                        IsRightKeyDown = true;
                        IsLeft_or_RightPushed = true;
                    }
                    else
                        IsRightKeyDown = false;
                    if ((Keyboard.GetKeyStates(Key.Space) & KeyStates.Down) > 0)
                    {
                        if (!IsSpaceKeyDown)
                        {
                            if (IsPlaying)
                                Pause_B.PerformClick();
                            else
                                Play_B.PerformClick();
                        }
                        IsSpaceKeyDown = true;
                    }
                    else
                        IsSpaceKeyDown = false;
                    if ((Keyboard.GetKeyStates(Key.V) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.Up) & KeyStates.Down) > 0)
                    {
                        double Increase = 0.4;
                        if (Volume_Start_S.Value + Increase > 11)
                            Volume_Start_S.Value = 11;
                        else
                            Volume_Start_S.Value += Increase;
                    }
                    else if ((Keyboard.GetKeyStates(Key.V) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.Down) & KeyStates.Down) > 0)
                    {
                        double Increase = 0.4;
                        if (Volume_Start_S.Value - Increase < -11)
                            Volume_Start_S.Value = -11;
                        else
                            Volume_Start_S.Value -= Increase;
                    }
                    if ((Keyboard.GetKeyStates(Key.S) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.Up) & KeyStates.Down) > 0)
                    {
                        double Increase = 5;
                        if (Pitch_Start_S.Value + Increase > 1200)
                            Pitch_Start_S.Value = 1200;
                        else
                            Pitch_Start_S.Value += Increase;
                    }
                    else if ((Keyboard.GetKeyStates(Key.S) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.Down) & KeyStates.Down) > 0)
                    {
                        double Decrease = 5;
                        if (Pitch_Start_S.Value - Decrease < -1200)
                            Pitch_Start_S.Value = -1200;
                        else
                            Pitch_Start_S.Value -= Decrease;
                    }
                    else if ((Keyboard.GetKeyStates(Key.S) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.R) & KeyStates.Down) > 0)
                        Pitch_Start_S.Value = 0;
                }
                if (IsEnded)
                {
                    Bass.BASS_ChannelStop(Stream_Mix);
                    Bass.BASS_ChannelSetPosition(Stream_Mix, 0);
                    Bass.BASS_ChannelSetPosition(Streams[0], 0);
                    Bass.BASS_ChannelSetPosition(Streams[1], 0);
                    Position_S.Value = 0;
                    Position_T.Text = "00:00 / " + Max_Time;
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
        private void OK_B_Click(object sender, RoutedEventArgs e)
        {
            Closing();
        }
        private void Cancel_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            Settings.High_Pass_Filter = Settings_Source.High_Pass_Filter;
            Settings.Low_Pass_Filter = Settings_Source.Low_Pass_Filter;
            Settings.Pitch = Settings_Source.Pitch;
            Settings.Volume = Settings_Source.Volume;
            Settings.HPF_Range = Settings_Source.HPF_Range;
            Settings.LPF_Range = Settings_Source.LPF_Range;
            Settings.Pitch_Range = Settings_Source.Pitch_Range;
            Settings.Volume_Range = Settings_Source.Volume_Range;
            Settings.IsHPFRange = Settings_Source.IsHPFRange;
            Settings.IsLPFRange = Settings_Source.IsLPFRange;
            Settings.IsPitchRange = Settings_Source.IsPitchRange;
            Settings.IsVolumeRange = Settings_Source.IsVolumeRange;
            Settings.When_Limit_Reached = Settings_Source.When_Limit_Reached;
            Settings.Limit_Sound_Instance = Settings_Source.Limit_Sound_Instance;
            Settings.When_Priority_Equal = Settings_Source.When_Priority_Equal;
            Settings.Delay = Settings_Source.Delay; ;
            Closing();
        }
        async void Closing()
        {
            if (!IsClosing)
            {
                _ = Pause_Volume_Animation(true, 15);
                IsClosing = true;
                Max_Time_Stream = -1;
                Selected_Voice_Index = -1;
                Settings_Source = null;
                Settings = null;
                while (Opacity > 0)
                {
                    Opacity -= Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
                Sound_Dispose();
                File.Delete(Voice_Set.Special_Path + "\\Wwise\\Temp_Voice_Create_03.mp3");
                Visibility = Visibility.Hidden;
                IsClosing = false;
            }
        }
        private void Limit_T_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var fullText = Limit_T.Text.Insert(Limit_T.SelectionStart, e.Text);
            double val;
            e.Handled = !double.TryParse(fullText, out val);
            e.Handled = Limit_T.Text.Length >= 2;
        }
        private void Slider_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((Slider)sender).Value = 0;
        }
        private void LPF_S_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            LPF_Move(0, 2);
        }
        private void HPF_S_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            HPF_Move(0, 2);
        }
        private void Volume_Start_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsClosing)
                return;
            Volume_Start_T.Text = "ゲイン(db):" + (int)e.NewValue;
            if (!Settings.IsVolumeRange)
            {
                Settings.Volume = e.NewValue;
                if (Event_Gain != null)
                    Event_Gain.Gain_dBV = e.NewValue;
            }
            else
                Settings.Volume_Range.Start = e.NewValue;
        }
        private void Volume_End_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsClosing)
                return;
            Volume_End_T.Text = "～:" + (int)e.NewValue;
            Settings.Volume_Range.End = e.NewValue;
        }
        private void Pitch_Start_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsClosing)
                return;
            Pitch_Start_T.Text = "ピッチ:" + (int)e.NewValue;
            if (!Settings.IsPitchRange)
            {
                Settings.Pitch = (int)e.NewValue;
                Set_Mix_Pitch(Settings.Pitch);
            }
            else
                Settings.Pitch_Range.Start = (int)Pitch_Start_S.Value;
        }
        private void Pitch_End_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsClosing)
                return;
            Pitch_End_T.Text = "～:" + (int)e.NewValue;
            Settings.Pitch_Range.End = (int)e.NewValue;
        }
        private async void LPF_Start_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsClosing)
                return;
            LPF_Start_T.Text = "LPF:" + (int)e.NewValue;
            int NewValue = (int)e.NewValue == 0 ? 1 : (int)e.NewValue;
            if (LPF_Before_Value < NewValue)
                LPF_Before_Value = NewValue;
            while (IsLPFMoving)
                await Task.Delay(50);
            if (LPF_Before_Value != NewValue)
                return;
            if (!Settings.IsLPFRange)
            {
                Event_LPF_Setting.fCenter = (float)Get_LPF_Value(NewValue);
                Settings.Low_Pass_Filter = (int)e.NewValue;
                Bass.BASS_FXSetParameters(Stream_Event_LPF, Event_LPF_Setting);
            }
            else
                Settings.LPF_Range.Start = (int)e.NewValue;
            LPF_Before_Value = 0;
        }
        private void LPF_End_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsClosing)
                return;
            LPF_End_T.Text = "～:" + (int)e.NewValue;
            Settings.LPF_Range.End = (int)e.NewValue;
        }
        int Get_LPF_Value(int Value)
        {
            int NewValue = Value == 0 ? 1 : Value;
            int Index = (int)Math.Floor(NewValue / 10.0);
            int Key = Sub_Code.LPF_Values.Keys.ElementAt(Index);
            return Key - (int)((double)Sub_Code.LPF_Values[Key] * Sub_Code.Get_Decimal(NewValue / 10.0));
        }
        private async void HPF_Start_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsClosing)
                return;
            HPF_Start_T.Text = "HPF:" + (int)e.NewValue;
            int NewValue = (int)e.NewValue == 0 ? 1 : (int)e.NewValue;
            if (HPF_Before_Value < NewValue)
                HPF_Before_Value = NewValue;
            while (IsHPFMoving)
                await Task.Delay(50);
            if (HPF_Before_Value != NewValue)
                return;
            if (!Settings.IsHPFRange)
            {
                Event_HPF_Setting.fCenter = (float)Get_HPF_Value(NewValue);
                Settings.High_Pass_Filter = (int)e.NewValue;
                Bass.BASS_FXSetParameters(Stream_Event_HPF, Event_HPF_Setting);
            }
            else
                Settings.HPF_Range.End = (int)e.NewValue;
            HPF_Before_Value = 0;
        }
        private void HPF_End_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsClosing)
                return;
            HPF_End_T.Text = "～:" + (int)e.NewValue;
            Settings.HPF_Range.End = (int)e.NewValue;
        }
        int Get_HPF_Value(int Value)
        {
            int NewValue = Value == 0 ? 1 : Value;
            int Index = (int)Math.Floor(NewValue / 5.0);
            int Key = Sub_Code.HPF_Values.Keys.ElementAt(Index);
            return Key + (int)((double)Sub_Code.HPF_Values[Key] * Sub_Code.Get_Decimal(NewValue / 5.0));
        }
        async void HPF_Move(int Min, double Down_Speed = 1)
        {
            IsHPFMoving = true;
            double Now_Value = HPF_Start_S.Value;
            HPF_Start_S.Value = Min;
            int a = Min;
            if (Min == 0)
                Min = 1;
            while (Now_Value > Min)
            {
                Now_Value -= Down_Speed;
                if (Now_Value < Min)
                    Now_Value = Min;
                int Index = (int)Math.Floor(Now_Value / 5.0);
                int Key = Sub_Code.HPF_Values.Keys.ElementAt(Index);
                int Value = Key + (int)((double)Sub_Code.HPF_Values[Key] * Sub_Code.Get_Decimal(Now_Value / 5.0));
                Event_HPF_Setting.fCenter = (float)Value;
                Bass.BASS_FXSetParameters(Stream_Event_HPF, Event_HPF_Setting);
                await Task.Delay(1000 / 60);
            }
            Settings.High_Pass_Filter = a;
            IsHPFMoving = false;
        }
        async void LPF_Move(int Min, double Down_Speed = 1)
        {
            IsLPFMoving = true;
            double Now_Value = LPF_Start_S.Value;
            LPF_Start_S.Value = Min;
            int a = Min;
            if (Min == 0)
                Min = 1;
            while (Now_Value > Min)
            {
                Now_Value -= Down_Speed;
                if (Now_Value < Min)
                    Now_Value = Min;
                int Index = (int)Math.Floor(Now_Value / 10.0);
                int Key = Sub_Code.LPF_Values.Keys.ElementAt(Index);
                int Value = Key - (int)((double)Sub_Code.LPF_Values[Key] * Sub_Code.Get_Decimal(Now_Value / 10.0));
                Event_LPF_Setting.fCenter = (float)Value;
                Bass.BASS_FXSetParameters(Stream_Event_LPF, Event_LPF_Setting);
                await Task.Delay(1000 / 60);
            }
            Settings.Low_Pass_Filter = a;
            IsLPFMoving = false;
        }
        private void Delay_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double Value = Math.Round(e.NewValue, 2, MidpointRounding.AwayFromZero);
            Settings.Delay = Value;
            Delay_T.Text = "遅延:" + Value + "秒";
        }
        void Set_Mix_Pitch(int Pitch)
        {
            Set_Pitch(Stream_Mix, Mix_Freq, Pitch);
        }
        void Set_Voice_Pitch(int Pitch)
        {
            Set_Pitch(Streams[1], Voice_Freq, Pitch);
        }
        void Set_Pitch(int Handle, float Default_Freq, int Pitch)
        {
            if (Pitch == 0)
            {
                Bass.BASS_ChannelSetAttribute(Handle, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, Default_Freq);
                return;
            }
            int Key = 0;
            int Index = 0;
            List<int> Key_INT = Sub_Code.Pitch_Values.Keys.ToList();
            if (Pitch > 0)
            {
                for (int Number = 0; Number < Sub_Code.Pitch_Values.Keys.Count; Number++)
                {
                    if (Pitch >= Key_INT[Number])
                    {
                        Key = Key_INT[Number];
                        Index = Number;
                        break;
                    }
                }
            }
            else if (Pitch < 0)
            {
                for (int Number = Sub_Code.Pitch_Values.Keys.Count - 1; Number >= 0; Number--)
                {
                    if (Pitch <= Key_INT[Number])
                    {
                        Key = Key_INT[Number];
                        Index = Number;
                        break;
                    }
                }
            }
            double Plus_Freq = 0;
            if (Pitch > 0 && Pitch < 1200)
                Plus_Freq = 10.0 * (Pitch - Key) / (Key_INT[Index - 1] - Key);
            else if (Pitch < 0 && Pitch > -1200)
                Plus_Freq = -10.0 * (Pitch - Key) / (Key_INT[Index + 1] - Key);
            Bass.BASS_ChannelSetAttribute(Handle, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, Default_Freq * (float)(1 + (Sub_Code.Pitch_Values[Key] + Plus_Freq) / 100.0));
        }
        private void Slider_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }
        private void Range_C_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Image Check = sender as Image;
            bool IsEnable = false;
            if (Check.Source == Sub_Code.Check_04)
                (sender as Image).Source = Sub_Code.Check_02;
            else
            {
                IsEnable = true;
                (sender as Image).Source = Sub_Code.Check_04;
            }
            if (Check.Name == "Volume_C")
            {
                Settings.IsVolumeRange = IsEnable;
                if (IsEnable)
                {
                    Volume_Start_S.Value = Settings.Volume_Range.Start;
                    Volume_End_S.Value = Settings.Volume_Range.End;
                }
                else
                    Volume_Start_S.Value = Settings.Volume;
                Change_Effect(0);
            }
            else if (Check.Name == "Pitch_C")
            {
                Settings.IsPitchRange = IsEnable;
                if (IsEnable)
                {
                    Pitch_Start_S.Value = Settings.Pitch_Range.Start;
                    Pitch_End_S.Value = Settings.Pitch_Range.End;
                }
                else
                    Pitch_Start_S.Value = Settings.Pitch;
                Change_Effect(1);
            }
            else if (Check.Name == "LPF_C")
            {
                Settings.IsLPFRange = IsEnable;
                if (IsEnable)
                {
                    LPF_Start_S.Value = Settings.LPF_Range.Start;
                    LPF_End_S.Value = Settings.LPF_Range.End;
                }
                else
                    LPF_Start_S.Value = Settings.Low_Pass_Filter;
                Change_Effect(2);
            }
            else if (Check.Name == "HPF_C")
            {
                Settings.IsHPFRange = IsEnable;
                if (IsEnable)
                {
                    HPF_Start_S.Value = Settings.HPF_Range.Start;
                    HPF_End_S.Value = Settings.HPF_Range.End;
                }
                else
                    HPF_Start_S.Value = Settings.High_Pass_Filter;
                Change_Effect(3);
            }
            Change_Range_Mode();
        }
        private void Range_C_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Image Check = sender as Image;
            if (Check.Source == Sub_Code.Check_03)
                Check.Source = Sub_Code.Check_04;
            else if (Check.Source == Sub_Code.Check_01)
                Check.Source = Sub_Code.Check_02;
        }
        private void Range_C_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Image Check = sender as Image;
            if (Check.Source == Sub_Code.Check_04)
                Check.Source = Sub_Code.Check_03;
            else if (Check.Source == Sub_Code.Check_02)
                Check.Source = Sub_Code.Check_01;
        }
        void Change_Range_Mode()
        {
            Update_Effect_B.Visibility = Visibility.Hidden;
            Volume_End_T.Visibility = Settings.IsVolumeRange ? Visibility.Visible : Visibility.Hidden;
            Volume_End_S.Visibility = Settings.IsVolumeRange ? Visibility.Visible : Visibility.Hidden;
            Pitch_End_T.Visibility = Settings.IsPitchRange ? Visibility.Visible : Visibility.Hidden;
            Pitch_End_S.Visibility = Settings.IsPitchRange ? Visibility.Visible : Visibility.Hidden;
            LPF_End_T.Visibility = Settings.IsLPFRange ? Visibility.Visible : Visibility.Hidden;
            LPF_End_S.Visibility = Settings.IsLPFRange ? Visibility.Visible : Visibility.Hidden;
            HPF_End_T.Visibility = Settings.IsHPFRange ? Visibility.Visible : Visibility.Hidden;
            HPF_End_S.Visibility = Settings.IsHPFRange ? Visibility.Visible : Visibility.Hidden;
            if (Settings.IsVolumeRange || Settings.IsPitchRange || Settings.IsLPFRange || Settings.IsHPFRange)
                Update_Effect_B.Visibility = Visibility.Visible;
        }
        private void Update_Effect_B_Click(object sender, RoutedEventArgs e)
        {
            Change_Effect();
        }
        void Change_Effect(int Mode = -1)
        {
            if (Mode == -1 || Mode == 0)
            {
                double Volume;
                if (Settings.IsVolumeRange)
                {
                    if (Settings.Volume_Range.End >= Settings.Volume_Range.Start)
                        Volume = Sub_Code.Get_Random_Double(Settings.Volume_Range.Start, Settings.Volume_Range.End);
                    else
                        Volume = Sub_Code.Get_Random_Double(Settings.Volume_Range.End, Settings.Volume_Range.Start);
                }
                else
                    Volume = Settings.Volume;
                if (Event_Gain != null)
                    Event_Gain.Gain_dBV = Volume;
            }
            if (Mode == -1 || Mode == 1)
            {
                int Pitch;
                if (Settings.IsPitchRange)
                {
                    if (Settings.Pitch_Range.End >= Settings.Pitch_Range.Start)
                        Pitch = Sub_Code.r.Next(Settings.Pitch_Range.Start, Settings.Pitch_Range.End + 1);
                    else
                        Pitch = Sub_Code.r.Next(Settings.Pitch_Range.End, Settings.Pitch_Range.Start);
                }
                else
                    Pitch = Settings.Pitch;
                Set_Mix_Pitch(Pitch);
            }
            if (Mode == -1 || Mode == 2)
            {
                int LPF;
                if (Settings.IsLPFRange)
                {
                    if (Settings.LPF_Range.End >= Settings.LPF_Range.Start)
                        LPF = Sub_Code.r.Next(Settings.LPF_Range.Start, Settings.LPF_Range.End + 1);
                    else
                        LPF = Sub_Code.r.Next(Settings.LPF_Range.End, Settings.LPF_Range.Start);
                }
                else
                    LPF = Settings.Low_Pass_Filter;
                Event_LPF_Setting.fCenter = (float)Get_LPF_Value(LPF);
                Bass.BASS_FXSetParameters(Stream_Event_LPF, Event_LPF_Setting);
            }
            if (Mode == -1 || Mode == 3)
            {
                int HPF;
                if (Settings.IsHPFRange)
                {
                    if (Settings.HPF_Range.End >= Settings.HPF_Range.Start)
                        HPF = Sub_Code.r.Next(Settings.HPF_Range.Start, Settings.HPF_Range.End + 1);
                    else
                        HPF = Sub_Code.r.Next(Settings.HPF_Range.End, Settings.HPF_Range.Start);
                }
                else
                    HPF = Settings.High_Pass_Filter;
                Event_HPF_Setting.fCenter = (float)Get_HPF_Value(HPF);
                Bass.BASS_FXSetParameters(Stream_Event_HPF, Event_HPF_Setting);
            }
        }
        private void Reset_B_Click(object sender, RoutedEventArgs e)
        {
            Sound_Dispose();
            List<double> Sound_Time = new List<double>();
            string Play_SE_Name = "なし";
            Streams.Add(0);
            Sound_Time.Add(0);
            if (Settings.SE_Index != -1)
            {
                SE_Type Temp = seSetting.sePreset.types[Settings.SE_Index - 1];
                if (Temp.items.Count > 0)
                {
                    List<string> playRandomSE = Temp.GetRandomItems();
                    string Name = playRandomSE[Sub_Code.r.Next(0, playRandomSE.Count)];
                    if (Path.GetExtension(Name) == ".flac")
                        Streams[0] = BassFlac.BASS_FLAC_StreamCreateFile(Name, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE);
                    else
                        Streams[0] = Bass.BASS_StreamCreateFile(Name, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE);
                    Sound_Time[0] = Bass.BASS_ChannelBytes2Seconds(Streams[0], Bass.BASS_ChannelGetLength(Streams[0], BASSMode.BASS_POS_BYTES));
                    Play_SE_Name = Path.GetFileName(Name);
                }
            }
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, 100);
            string Play_Voice_Name = "なし";
            Streams.Add(0);
            Sound_Time.Add(0);
            Selected_Voice_Index = -1;
            if (Settings.Sounds.Count > 0)
            {
                int Max_Probability = 0;
                foreach (Voice_Sound_Setting Sound in Settings.Sounds)
                    Max_Probability += (int)Sound.Probability;
                int Random_Probability = Sub_Code.r.Next(0, Max_Probability + 1);
                int Now_Probability = 0;
                if (Max_Probability > 0)
                {
                    for (int Number = 0; Number < Settings.Sounds.Count; Number++)
                    {
                        if (Now_Probability + Settings.Sounds[Number].Probability >= Random_Probability)
                        {
                            int Voice_Sound_Handle;
                            if (Settings.Sounds[Number].File_Path.Contains("\\"))
                            {
                                if (Path.GetExtension(Settings.Sounds[Number].File_Path) == ".flac")
                                    Voice_Sound_Handle = BassFlac.BASS_FLAC_StreamCreateFile(Settings.Sounds[Number].File_Path, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE);
                                else
                                    Voice_Sound_Handle = Bass.BASS_StreamCreateFile(Settings.Sounds[Number].File_Path, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE);
                            }
                            else
                            {
                                Sound_Bytes = WVS_File.Load_Sound(Settings.Sounds[Number].Stream_Position);
                                if (Sound_IntPtr != null && Sound_IntPtr.IsAllocated)
                                    Sound_IntPtr.Free();
                                Sound_IntPtr = GCHandle.Alloc(Sound_Bytes, GCHandleType.Pinned);
                                IntPtr pin = Sound_IntPtr.AddrOfPinnedObject();
                                if (Path.GetExtension(Settings.Sounds[Number].File_Path) == ".flac")
                                    Voice_Sound_Handle = BassFlac.BASS_FLAC_StreamCreateFile(pin, 0L, Sound_Bytes.Length, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE);
                                else
                                    Voice_Sound_Handle = Bass.BASS_StreamCreateFile(pin, 0L, Sound_Bytes.Length, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE);
                            }
                            Play_Voice_Name = Path.GetFileName(Settings.Sounds[Number].File_Path);
                            Streams[1] = BassFx.BASS_FX_TempoCreate(Voice_Sound_Handle, BASSFlag.BASS_FX_FREESOURCE | BASSFlag.BASS_STREAM_DECODE);
                            Selected_Voice_Index = Number;
                            break;
                        }
                        Now_Probability += (int)Settings.Sounds[Number].Probability;
                    }
                }
            }
            int Stream_Mix_Handle = Un4seen.Bass.AddOn.Mix.BassMix.BASS_Mixer_StreamCreate((int)Mix_Freq, 2, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE);
            if (Selected_Voice_Index != -1)
            {
                Bass.BASS_ChannelGetAttribute(Streams[1], BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, ref Voice_Freq);
                Stream_Voice_LPF = Bass.BASS_ChannelSetFX(Streams[1], BASSFXType.BASS_FX_BFX_BQF, 2);
                Stream_Voice_HPF = Bass.BASS_ChannelSetFX(Streams[1], BASSFXType.BASS_FX_BFX_BQF, 1);
                Voice_Gain = new DSP_Gain(Streams[1], 0);
                Set_Voice_Effect(Selected_Voice_Index);
                long Start = Bass.BASS_ChannelSeconds2Bytes(Streams[1], Settings.Sounds[Selected_Voice_Index].Play_Time.Start_Time);
                if (Settings.Sounds[Selected_Voice_Index].Play_Time.End_Time == 0)
                    Sound_Time[1] = Bass.BASS_ChannelBytes2Seconds(Streams[1], Bass.BASS_ChannelGetLength(Streams[1], BASSMode.BASS_POS_BYTES)) - Settings.Sounds[Selected_Voice_Index].Play_Time.Start_Time;
                else
                    Sound_Time[1] = Settings.Sounds[Selected_Voice_Index].Play_Time.End_Time - Settings.Sounds[Selected_Voice_Index].Play_Time.Start_Time;
                long Start_Pos = Bass.BASS_ChannelSeconds2Bytes(Stream_Mix_Handle, Settings.Delay + Settings.Sounds[Selected_Voice_Index].Delay);
                Un4seen.Bass.AddOn.Mix.BassMix.BASS_Mixer_StreamAddChannelEx(Stream_Mix_Handle, Streams[1], BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE, Start_Pos, 0);
            }
            Un4seen.Bass.AddOn.Mix.BassMix.BASS_Mixer_StreamAddChannel(Stream_Mix_Handle, Streams[0], BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE);
            Stream_Mix = BassFx.BASS_FX_TempoCreate(Stream_Mix_Handle, BASSFlag.BASS_FX_FREESOURCE);
            Stream_Event_LPF = Bass.BASS_ChannelSetFX(Stream_Mix, BASSFXType.BASS_FX_BFX_BQF, 2);
            Stream_Event_HPF = Bass.BASS_ChannelSetFX(Stream_Mix, BASSFXType.BASS_FX_BFX_BQF, 1);
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, 500);
            Event_Gain = new DSP_Gain(Stream_Mix, 0);
            Bass.BASS_ChannelSetDevice(Stream_Mix, Video_Mode.Sound_Device);
            IsMusicEnd = new SYNCPROC(EndSync);
            Change_Effect();
            Max_Time_Stream = -1;
            if (Sound_Time.Count > 0)
            {
                Position_S.Maximum = Sound_Time.Max();
                Max_Time_Stream = Sound_Time.IndexOf(Position_S.Maximum);
                Max_Time = Sub_Code.Get_Time_String(Position_S.Maximum);
                Position_T.Text = "00:00 / " + Max_Time;
            }
            else
            {
                Position_S.Maximum = 0;
                Max_Time = "00:00";
                Position_T.Text = "00:00 / 00:00";
            }
            About_T.Text = "イベント名:" + Event_Name + "\nSE:" + Play_SE_Name + "\n音声:" + Play_Voice_Name;
            Bass.BASS_ChannelSetSync(Streams[Max_Time_Stream], BASSSync.BASS_SYNC_END | BASSSync.BASS_SYNC_MIXTIME, 0, IsMusicEnd, IntPtr.Zero);
            if (Selected_Voice_Index != -1)
                Bass.BASS_ChannelSetPosition(Streams[1], Settings.Sounds[Selected_Voice_Index].Play_Time.Start_Time);
            Bass.BASS_ChannelSetAttribute(Stream_Mix, BASSAttribute.BASS_ATTRIB_VOL, (float)All_Volume_S.Value / 100f);
            Sound_Time.Clear();
            if (Selected_Voice_Index != -1 && Settings.Sounds[Selected_Voice_Index].IsFadeIn)
            {
                Bass.BASS_ChannelSetAttribute(Streams[1], BASSAttribute.BASS_ATTRIB_VOL, 0f);
                double Play_Time = 0;
                if (Settings.Sounds[Selected_Voice_Index].Play_Time.End_Time != 0)
                    Play_Time = Settings.Sounds[Selected_Voice_Index].Play_Time.End_Time - Settings.Sounds[Selected_Voice_Index].Play_Time.Start_Time;
                else
                    Play_Time = Settings.Sounds[Selected_Voice_Index].Play_Time.Max_Time - Settings.Sounds[Selected_Voice_Index].Play_Time.Start_Time;
                if (Play_Time > 0.5)
                    Play_Volume_Animation(30f, Streams[1], 1f);
            }
            Bass.BASS_ChannelPlay(Stream_Mix, true);
        }
        void Set_Voice_Effect(int Voice_Index)
        {
            if (Voice_Index == -1)
                return;
            double Volume;
            if (Settings.Sounds[Voice_Index].IsVolumeRange)
            {
                if (Settings.Sounds[Voice_Index].Volume_Range.End >= Settings.Sounds[Voice_Index].Volume_Range.Start)
                    Volume = Sub_Code.Get_Random_Double(Settings.Sounds[Voice_Index].Volume_Range.Start, Settings.Sounds[Voice_Index].Volume_Range.End);
                else
                    Volume = Sub_Code.Get_Random_Double(Settings.Sounds[Voice_Index].Volume_Range.End, Settings.Sounds[Voice_Index].Volume_Range.Start);
            }
            else
                Volume = Settings.Sounds[Voice_Index].Volume;
            Voice_Gain.Gain_dBV = Volume;
            int Pitch;
            if (Settings.Sounds[Voice_Index].IsPitchRange)
            {
                if (Settings.Sounds[Voice_Index].Pitch_Range.End >= Settings.Sounds[Voice_Index].Pitch_Range.Start)
                    Pitch = Sub_Code.r.Next(Settings.Sounds[Voice_Index].Pitch_Range.Start, Settings.Sounds[Voice_Index].Pitch_Range.End + 1);
                else
                    Pitch = Sub_Code.r.Next(Settings.Sounds[Voice_Index].Pitch_Range.End, Settings.Sounds[Voice_Index].Pitch_Range.Start);
            }
            else
                Pitch = Settings.Sounds[Voice_Index].Pitch;
            Set_Voice_Pitch(Pitch);
            int LPF;
            if (Settings.Sounds[Voice_Index].IsLPFRange)
            {
                if (Settings.Sounds[Voice_Index].LPF_Range.End >= Settings.Sounds[Voice_Index].LPF_Range.Start)
                    LPF = Sub_Code.r.Next(Settings.Sounds[Voice_Index].LPF_Range.Start, Settings.Sounds[Voice_Index].LPF_Range.End + 1);
                else
                    LPF = Sub_Code.r.Next(Settings.Sounds[Voice_Index].LPF_Range.End, Settings.Sounds[Voice_Index].LPF_Range.Start);
            }
            else
                LPF = Settings.Sounds[Voice_Index].Low_Pass_Filter;
            Voice_LPF_Setting.fCenter = (float)Get_LPF_Value(LPF);
            Bass.BASS_FXSetParameters(Stream_Voice_LPF, Voice_LPF_Setting);
            int HPF;
            if (Settings.Sounds[Voice_Index].IsHPFRange)
            {
                if (Settings.Sounds[Voice_Index].HPF_Range.End >= Settings.Sounds[Voice_Index].HPF_Range.Start)
                    HPF = Sub_Code.r.Next(Settings.Sounds[Voice_Index].HPF_Range.Start, Settings.Sounds[Voice_Index].HPF_Range.End + 1);
                else
                    HPF = Sub_Code.r.Next(Settings.Sounds[Voice_Index].HPF_Range.End, Settings.Sounds[Voice_Index].HPF_Range.Start);
            }
            else
                HPF = Settings.Sounds[Voice_Index].High_Pass_Filter;
            Voice_HPF_Setting.fCenter = (float)Get_HPF_Value(HPF);
            Bass.BASS_FXSetParameters(Stream_Voice_HPF, Voice_HPF_Setting);
        }
        private void Sound_Minus_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            int Move_Time = 5;
            if (IsLControlKeyDown)
                Move_Time = 10;
            if (Position_S.Value <= Move_Time)
                Position_S.Value = 0;
            else
                Position_S.Value -= Move_Time;
            Music_Pos_Change(Position_S.Value, true);
        }
        private void Sound_Plus_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            int Move_Time = 5;
            if (IsLControlKeyDown)
                Move_Time = 10;
            if (Position_S.Value + Move_Time >= Position_S.Maximum)
                Position_S.Value = Position_S.Maximum;
            else
                Position_S.Value += Move_Time;
            Music_Pos_Change(Position_S.Value, true);
        }
        void Music_Pos_Change(double Pos, bool IsBassPosChange)
        {
            if (IsBassPosChange && Streams.Count > 0)
            {
                Un4seen.Bass.AddOn.Mix.BassMix.BASS_Mixer_ChannelSetPosition(Streams[0], Bass.BASS_ChannelSeconds2Bytes(Streams[0], Pos));
                if (Selected_Voice_Index != -1)
                    Un4seen.Bass.AddOn.Mix.BassMix.BASS_Mixer_ChannelSetPosition(Streams[1], Bass.BASS_ChannelSeconds2Bytes(Streams[1], Pos + Settings.Sounds[Selected_Voice_Index].Play_Time.Start_Time));
                Un4seen.Bass.AddOn.Mix.BassMix.BASS_Mixer_ChannelSetPosition(Stream_Mix, 0);
            }
            Position_T.Text = Sub_Code.Get_Time_String(Pos) + " / " + Max_Time;
        }
        private void Pause_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            _ = Pause_Volume_Animation(false, 5);
        }
        private void Play_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            Play_Volume_Animation(5);
        }
        async Task Pause_Volume_Animation(bool IsStop, float Fade_Time = 30f, int Handle = -1)
        {
            if (IsPaused)
                return;
            int Before_Handle = Handle;
            if (Handle == -1)
            {
                Handle = Stream_Mix;
                IsPaused = true;
            }
            float Volume_Now = 1f;
            Bass.BASS_ChannelGetAttribute(Handle, BASSAttribute.BASS_ATTRIB_VOL, ref Volume_Now);
            float Volume_Minus = Volume_Now / Fade_Time;
            while (Volume_Now > 0f && IsPaused)
            {
                Volume_Now -= Volume_Minus;
                if (Volume_Now < 0f)
                    Volume_Now = 0f;
                Bass.BASS_ChannelSetAttribute(Handle, BASSAttribute.BASS_ATTRIB_VOL, Volume_Now);
                await Task.Delay(1000 / 60);
            }
            if (Volume_Now <= 0f)
            {
                if (IsStop)
                {
                    Bass.BASS_ChannelStop(Handle);
                    Bass.BASS_StreamFree(Handle);
                    Position_S.Value = 0;
                    Position_S.Maximum = 0;
                    Position_T.Text = "00:00 / 00:00";
                    Max_Time = "00:00";
                    if (Sound_IntPtr != null && Sound_IntPtr.IsAllocated)
                        Sound_IntPtr.Free();
                    Sound_Bytes = null;
                }
                else if (IsPaused || Before_Handle != -1)
                    Bass.BASS_ChannelPause(Handle);
            }
        }
        async void Play_Volume_Animation(float Feed_Time = 30f, int Handle = -1, float Max_Volume = -1)
        {
            if (Handle == -1)
                Handle = Stream_Mix;
            if (Max_Volume == -1)
                Max_Volume = (float)(All_Volume_S.Value / 100);
            IsPaused = false;
            Bass.BASS_ChannelPlay(Stream_Mix, false);
            float Volume_Now = 1f;
            Bass.BASS_ChannelGetAttribute(Handle, BASSAttribute.BASS_ATTRIB_VOL, ref Volume_Now);
            float Volume_Plus = Max_Volume / Feed_Time;
            while (Volume_Now < Max_Volume && !IsPaused)
            {
                Volume_Now += Volume_Plus;
                if (Volume_Now > 1f)
                    Volume_Now = 1f;
                Bass.BASS_ChannelSetAttribute(Handle, BASSAttribute.BASS_ATTRIB_VOL, Volume_Now);
                await Task.Delay(1000 / 60);
            }
            if (!IsPaused && Selected_Voice_Index != -1)
                Bass.BASS_ChannelSetAttribute(Streams[1], BASSAttribute.BASS_ATTRIB_VOL, 1f);
        }
        private void All_Volume_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsClosing && !IsPaused)
                Bass.BASS_ChannelSetAttribute(Stream_Mix, BASSAttribute.BASS_ATTRIB_VOL, (float)All_Volume_S.Value / 100);
            All_Volume_T.Text = "全体音量:" + (int)All_Volume_S.Value;
        }
        private void Position_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLocationChanging && !IsClosing)
                Music_Pos_Change(Position_S.Value, false);
        }
        async void Position_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsClosing)
                return;
            IsLocationChanging = true;
            if (Bass.BASS_ChannelIsActive(Stream_Mix) == BASSActive.BASS_ACTIVE_PLAYING)
            {
                IsPlayingMouseDown = true;
                _ = Pause_Volume_Animation(false, 10);
            }
            while (IsLocationChanging)
            {
                Position_T.Text = Sub_Code.Get_Time_String(Position_S.Value) + " / " + Max_Time;
                await Task.Delay(1000 / 60);
            }
        }
        void Position_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsClosing)
                return;
            IsLocationChanging = false;
            Bass.BASS_ChannelStop(Stream_Mix);
            Music_Pos_Change(Position_S.Value, true);
            if (IsPlayingMouseDown)
            {
                IsPaused = false;
                Play_Volume_Animation(10);
                IsPlayingMouseDown = false;
            }
            else
                Position_T.Text = Sub_Code.Get_Time_String(Position_S.Value) + " / " + Max_Time;
        }
        void Sound_Dispose()
        {
            Max_Time_Stream = -1;
            Selected_Voice_Index = -1;
            Bass.BASS_ChannelRemoveFX(Stream_Mix, Stream_Event_LPF);
            Bass.BASS_ChannelRemoveFX(Stream_Mix, Stream_Event_HPF);
            if (Streams.Count >= 2)
            {
                Bass.BASS_ChannelRemoveFX(Streams[1], Stream_Voice_LPF);
                Bass.BASS_ChannelRemoveFX(Streams[1], Stream_Voice_HPF);
            }
            Bass.BASS_FXReset(Stream_Event_LPF);
            Bass.BASS_FXReset(Stream_Event_HPF);
            Bass.BASS_FXReset(Stream_Voice_LPF);
            Bass.BASS_FXReset(Stream_Voice_HPF);
            Bass.BASS_ChannelStop(Stream_Mix);
            Bass.BASS_StreamFree(Stream_Mix);
            if (Streams.Count >= 2)
            {
                Bass.BASS_StreamFree(Streams[0]);
                Bass.BASS_StreamFree(Streams[1]);
            }
            if (Sound_IntPtr != null && Sound_IntPtr.IsAllocated)
                Sound_IntPtr.Free();
            Sound_Bytes = null;
            Streams.Clear();
            Position_T.Text = "00:00 / 00:00";
            Position_S.Value = 0;
            Position_S.Maximum = 0;
            About_T.Text = "";
        }
        private void Volume_Help_B_Click(object sender, RoutedEventArgs e)
        {
            Message_Feed_Out("この値はMod Creator内で再生する際の音量で、値を変更してもWoTBには反映されません。\nWoTBに反映させる場合は\"ゲイン(db)\"の値を変更してください。");
        }
        private void Delay_Help_B_Click(object sender, RoutedEventArgs e)
        {
            Message_Feed_Out("イベント再生中に値を変更した場合、'イベント更新'を押さないと反映されません。\nこの値は音声または戦闘BGMのみ反映されます。");
        }
    }
}