using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using Un4seen.Bass.Misc;
using System.Linq;
using System.Runtime.InteropServices;

namespace WoTB_Voice_Mod_Creater.Class
{
    public class Voice_Sound_Range<T>
    {
        public T Start { get; set; }
        public T End { get; set; }
        public Voice_Sound_Range(T Start, T End)
        {
            this.Start = Start;
            this.End = End;
        }
        public Voice_Sound_Range<T> Clone()
        {
            return (Voice_Sound_Range<T>)MemberwiseClone();
        }
    }
    public class Voice_Sound_Setting
    {
        public string File_Path { get; set; }
        public long Stream_Position { get; set; }
        public double Probability { get; set; }
        public double Volume { get; set; }
        public double Delay { get; set; }
        public int Pitch { get; set; }
        public int Low_Pass_Filter { get; set; }
        public int High_Pass_Filter { get; set; }
        public int Type_Index { get; set; }
        public int Voice_Index { get; set; }
        public int File_Index { get; set; }
        public Voice_Sound_Range<double> Volume_Range;
        public Voice_Sound_Range<int> Pitch_Range;
        public Voice_Sound_Range<int> LPF_Range;
        public Voice_Sound_Range<int> HPF_Range;
        public bool IsVolumeRange = false;
        public bool IsPitchRange = false;
        public bool IsLPFRange = false;
        public bool IsHPFRange = false;
        public bool IsFadeIn = false;
        public bool IsFadeOut = false;
        public bool IsNormalMode = true;
        public Music_Play_Time Play_Time { get; private set; }
        public Voice_Sound_Setting()
        {
            File_Path = "";
            Init();
        }
        public Voice_Sound_Setting(string File_Path)
        {
            this.File_Path = File_Path;
            Init();
        }
        public Voice_Sound_Setting Clone()
        {
            Voice_Sound_Setting Info = (Voice_Sound_Setting)MemberwiseClone();
            Info.Volume_Range = Volume_Range.Clone();
            Info.Pitch_Range = Pitch_Range.Clone();
            Info.LPF_Range = LPF_Range.Clone();
            Info.HPF_Range = HPF_Range.Clone();
            Info.Play_Time = Play_Time.Clone();
            return Info;
        }
        private void Init()
        {
            Stream_Position = 0;
            Probability = 50;
            Pitch = 0;
            Low_Pass_Filter = 0;
            High_Pass_Filter = 0;
            Volume = 0;
            Delay = 0;
            Type_Index = -1;
            Voice_Index = -1;
            File_Index = -1;
            Volume_Range = new Voice_Sound_Range<double>(0, 0);
            Pitch_Range = new Voice_Sound_Range<int>(0, 0);
            LPF_Range = new Voice_Sound_Range<int>(0, 0);
            HPF_Range = new Voice_Sound_Range<int>(0, 0);
            Play_Time = new Music_Play_Time(0, 0);
        }
    }
    public class Voice_Event_Setting
    {
        //SE_Volumeは±表記なので、初期値の0は増減なしとなります。
        public List<Voice_Sound_Setting> Sounds = new List<Voice_Sound_Setting>();
        public string Event_Name { get; set; }
        public double Volume { get; set; }
        public double SE_Volume { get; set; }
        public double Delay { get; set; }
        public uint Event_ShortID { get; set; }
        public uint Voice_ShortID { get; set; }
        public uint SE_ShortID { get; set; }
        public int SE_Index { get; set; }
        public int Pitch { get; set; }
        public int Low_Pass_Filter { get; set; }
        public int High_Pass_Filter { get; set; }
        public int Limit_Sound_Instance { get; set; }
        public int When_Limit_Reached { get; set; }
        public int When_Priority_Equal { get; set; }
        public Voice_Sound_Range<double> Volume_Range;
        public Voice_Sound_Range<int> Pitch_Range;
        public Voice_Sound_Range<int> LPF_Range;
        public Voice_Sound_Range<int> HPF_Range;
        public bool IsVolumeRange = false;
        public bool IsPitchRange = false;
        public bool IsLPFRange = false;
        public bool IsHPFRange = false;
        public bool IsLoadMode = false;
        public Voice_Event_Setting()
        {
            SE_Index = -1;
            SE_Volume = 0;
            Init(0, 0);
        }
        public Voice_Event_Setting(uint Event_ShortID, uint Voice_ShortID)
        {
            SE_Index = -1;
            SE_Volume = 0;
            Init(Event_ShortID, Voice_ShortID);
        }
        public Voice_Event_Setting(uint Event_ShortID, uint Voice_ShortID, uint SE_ShortID = 0, int SE_Index = -1, double SE_Volume = 0)
        {
            this.SE_Index = SE_Index;
            this.SE_Volume = SE_Volume;
            Init(Event_ShortID, Voice_ShortID, SE_ShortID);
        }
        public void Set_Param(uint Event_ShortID, uint Voice_ShortID, uint SE_ShortID = 0, int SE_Index = -1, double SE_Volume = 0)
        {
            this.Event_ShortID = Event_ShortID;
            this.Voice_ShortID = Voice_ShortID;
            this.SE_ShortID = SE_ShortID;
            this.SE_Index = SE_Index;
            this.SE_Volume = SE_Volume;
        }
        public void Set_Param(string Event_Name, uint Voice_ShortID)
        {
            this.Voice_ShortID = Voice_ShortID;
            this.Event_Name = Event_Name;
        }
        public void Init(uint Event_ShortID, uint Voice_ShortID, uint SE_ShortID = 0)
        {
            this.Event_ShortID = Event_ShortID;
            this.Voice_ShortID = Voice_ShortID;
            this.SE_ShortID = SE_ShortID;
            Event_Name = "";
            Pitch = 0;
            Low_Pass_Filter = 0;
            High_Pass_Filter = 0;
            Volume = 0;
            Delay = 0;
            Limit_Sound_Instance = 50;
            When_Limit_Reached = 0;
            When_Priority_Equal = 0;
            Volume_Range = new Voice_Sound_Range<double>(0, 0);
            Pitch_Range = new Voice_Sound_Range<int>(0, 0);
            LPF_Range = new Voice_Sound_Range<int>(0, 0);
            HPF_Range = new Voice_Sound_Range<int>(0, 0);
            IsLoadMode = false;
        }
        public Voice_Event_Setting Clone()
        {
            Voice_Event_Setting Info = (Voice_Event_Setting)MemberwiseClone();
            Info.Volume_Range = Volume_Range.Clone();
            Info.Pitch_Range = Pitch_Range.Clone();
            Info.LPF_Range = LPF_Range.Clone();
            Info.HPF_Range = HPF_Range.Clone();
            Info.Sounds = new List<Voice_Sound_Setting>();
            foreach (Voice_Sound_Setting Sound_Info in Sounds)
                Info.Sounds.Add(Sound_Info.Clone());
            return Info;
        }
        public Voice_Event_Setting Get_No_Sounds()
        {
            Voice_Event_Setting Temp = new Voice_Event_Setting(Event_ShortID, Voice_ShortID, SE_ShortID);
            Temp.Event_Name = Event_Name;
            Temp.Delay = Delay;
            Temp.Volume = Volume;
            Temp.SE_Volume = SE_Volume;
            Temp.SE_Index = SE_Index;
            Temp.Pitch = Pitch;
            Temp.Low_Pass_Filter = Low_Pass_Filter;
            Temp.High_Pass_Filter = High_Pass_Filter;
            Temp.Volume_Range = Volume_Range.Clone();
            Temp.Pitch_Range = Pitch_Range.Clone();
            Temp.LPF_Range = LPF_Range.Clone();
            Temp.HPF_Range = HPF_Range.Clone();
            Temp.IsVolumeRange = IsVolumeRange;
            Temp.IsPitchRange = IsPitchRange;
            Temp.IsLPFRange = IsLPFRange;
            Temp.IsHPFRange = IsHPFRange;
            Temp.Limit_Sound_Instance = Limit_Sound_Instance;
            Temp.When_Limit_Reached = When_Limit_Reached;
            Temp.When_Priority_Equal = When_Priority_Equal;
            return Temp;
        }
    }
    public partial class Voice_Create_Setting : UserControl
    {
        Voice_Event_Setting Settings = null;
        Voice_Event_Setting Settings_Source = null;
        BASS_BFX_BQF LPF_Setting = new BASS_BFX_BQF(BASSBFXBQF.BASS_BFX_BQF_LOWPASS, 12000f, 0f, 0f, 1f, 0f, BASSFXChan.BASS_BFX_CHANALL);
        BASS_BFX_BQF HPF_Setting = new BASS_BFX_BQF(BASSBFXBQF.BASS_BFX_BQF_HIGHPASS, 0f, 0f, 0f, 1f, 0f, BASSFXChan.BASS_BFX_CHANALL);
        WVS_Load WVS_File = null;
        DSP_Gain Gain = null;
        GCHandle Sound_IntPtr;
        SYNCPROC IsMusicEnd;
        byte[] Sound_Bytes;
        int Stream;
        int Stream_LPF;
        int Stream_HPF;
        int List_Index_Mode = -1;
        int Index = -1;
        int HPF_Before_Value = 0;
        int LPF_Before_Value = 0;
        float Freq = 44100f;
        string Event_Name = "";
        string Max_Time = "00:00";
        bool IsClosing = false;
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
        bool IsNoSelectMode = false;
        public Voice_Create_Setting()
        {
            InitializeComponent();
            All_Volume_S.Value = 75;
            Weight_S.Value = 50;
            Position_S.AddHandler(MouseDownEvent, new MouseButtonEventHandler(Position_MouseDown), true);
            Position_S.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Position_MouseUp), true);
            Change_Range_Mode();
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Volume_C.Source = Sub_Code.Check_01;
            Pitch_C.Source = Sub_Code.Check_01;
            LPF_C.Source = Sub_Code.Check_01;
            HPF_C.Source = Sub_Code.Check_01;
            Fade_In_C.Source = Sub_Code.Check_01;
            Fade_Out_C.Source = Sub_Code.Check_01;
        }
        public async void Window_Show(string Event_Name, Voice_Event_Setting Settings, WVS_Load WVS_File, int List_Index_Mode, int Index)
        {
            IsClosing = false;
            Settings_Source = Settings.Clone();
            this.Event_Name = Event_Name;
            this.Settings = Settings;
            this.WVS_File = WVS_File;
            this.List_Index_Mode = List_Index_Mode;
            this.Index = Index;
            Sound_List.Items.Clear();
            foreach (Voice_Sound_Setting Info in Settings.Sounds)
                Sound_List.Items.Add(Path.GetFileName(Info.File_Path) + " | 優先度:" + (int)Info.Probability);
            Opacity = 0;
            Visibility = Visibility.Visible;
            Weight_S.Value = 50;
            Volume_Start_S.Value = 0;
            Pitch_Start_S.Value = 0;
            LPF_Start_S.Value = 0;
            HPF_Start_S.Value = 0;
            Delay_S.Value = 0;
            Position_Change();
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
                bool IsPlaying = (Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_PLAYING && !IsLocationChanging) ? true : false;
                if (IsPlaying)
                {
                    Bass.BASS_ChannelUpdate(Stream, 400);
                    if (!IsClosing)
                    {
                        double End_Time = Settings.Sounds[Sound_List.SelectedIndex].Play_Time.End_Time;
                        if (End_Time != 0 && Position_S.Value >= End_Time)
                        {
                            Change_Effect();
                            Music_Pos_Change(Settings.Sounds[Sound_List.SelectedIndex].Play_Time.Start_Time, true);
                            long position2 = Bass.BASS_ChannelGetPosition(Stream);
                            Position_S.Value = Bass.BASS_ChannelBytes2Seconds(Stream, position2);
                        }
                        else if (Position_S.Value < Settings.Sounds[Sound_List.SelectedIndex].Play_Time.Start_Time)
                        {
                            Change_Effect();
                            Music_Pos_Change(Settings.Sounds[Sound_List.SelectedIndex].Play_Time.Start_Time, true);
                            long position2 = Bass.BASS_ChannelGetPosition(Stream);
                            Position_S.Value = Bass.BASS_ChannelBytes2Seconds(Stream, position2);
                        }
                        long position = Bass.BASS_ChannelGetPosition(Stream);
                        Position_S.Value = Bass.BASS_ChannelBytes2Seconds(Stream, position);
                        Position_T.Text = Sub_Code.Get_Time_String(Position_S.Value) + " / " + Max_Time;
                    }
                }
                if (Sub_Code.IsForcusWindow && !IsClosing && Voice_Create_Event_Setting_Window.Visibility == Visibility.Hidden)
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
                                Sound_Pause_B.PerformClick();
                            else
                                Sound_Play_B.PerformClick();
                        }
                        IsSpaceKeyDown = true;
                    }
                    else
                        IsSpaceKeyDown = false;
                    if ((Keyboard.GetKeyStates(Key.V) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.Up) & KeyStates.Down) > 0)
                    {
                        double Increase = 1;
                        if (All_Volume_S.Value + Increase > 10)
                            All_Volume_S.Value = 10;
                        else
                            All_Volume_S.Value += Increase;
                    }
                    else if ((Keyboard.GetKeyStates(Key.V) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.Down) & KeyStates.Down) > 0)
                    {
                        double Increase = 1;
                        if (All_Volume_S.Value - Increase < -10)
                            All_Volume_S.Value = -10;
                        else
                            All_Volume_S.Value -= Increase;
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
                    if ((Keyboard.GetKeyStates(Key.H) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.Up) & KeyStates.Down) > 0)
                    {
                        double Increase = 0.5;
                        if (HPF_Start_S.Value + Increase > 70)
                            HPF_Start_S.Value = 70;
                        else
                            HPF_Start_S.Value += Increase;
                    }
                    else if ((Keyboard.GetKeyStates(Key.H) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.Down) & KeyStates.Down) > 0)
                    {
                        double Decrease = 0.5;
                        if (HPF_Start_S.Value - Decrease < 0)
                            HPF_Start_S.Value = 0;
                        else
                            HPF_Start_S.Value -= Decrease;
                    }
                    if ((Keyboard.GetKeyStates(Key.L) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.Up) & KeyStates.Down) > 0)
                    {
                        double Increase = 0.5;
                        if (LPF_Start_S.Value + Increase > 70)
                            LPF_Start_S.Value = 70;
                        else
                            LPF_Start_S.Value += Increase;
                    }
                    else if ((Keyboard.GetKeyStates(Key.L) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.Down) & KeyStates.Down) > 0)
                    {
                        double Decrease = 0.5;
                        if (LPF_Start_S.Value - Decrease < 0)
                            LPF_Start_S.Value = 0;
                        else
                            LPF_Start_S.Value -= Decrease;
                    }
                }
                if (IsEnded)
                {
                    Bass.BASS_ChannelStop(Stream);
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
        async Task Pause_Volume_Animation(bool IsStop, float Feed_Time = 30f)
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
                    Max_Time = "00:00";
                    if (Sound_IntPtr != null && Sound_IntPtr.IsAllocated)
                        Sound_IntPtr.Free();
                    Sound_Bytes = null;
                }
                else if (IsPaused)
                    Bass.BASS_ChannelPause(Stream);
            }
        }
        async void Play_Volume_Animation(float Feed_Time = 30f)
        {
            IsPaused = false;
            Bass.BASS_ChannelPlay(Stream, false);
            float Volume_Now = 1f;
            Bass.BASS_ChannelGetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, ref Volume_Now);
            float Volume_Plus = (float)(All_Volume_S.Value / 100) / Feed_Time;
            while (Volume_Now < (float)(All_Volume_S.Value / 100) && !IsPaused)
            {
                Volume_Now += Volume_Plus;
                if (Volume_Now > 1f)
                    Volume_Now = 1f;
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, Volume_Now);
                await Task.Delay(1000 / 60);
            }
        }
        void Music_Pos_Change(double Pos, bool IsBassPosChange)
        {
            if (IsBassPosChange)
                Bass.BASS_ChannelSetPosition(Stream, Pos);
            Position_T.Text = Sub_Code.Get_Time_String(Pos) + " / " + Max_Time;
        }
        private void Position_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLocationChanging && !IsClosing)
                Music_Pos_Change(Position_S.Value, false);
        }
        private void All_Volume_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsClosing)
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)All_Volume_S.Value / 100);
            All_Volume_T.Text = "全体音量:" + (int)All_Volume_S.Value;
        }
        private void Weight_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsClosing)
                return;
            Weight_T.Text = "優先度:" + (int)Weight_S.Value;
            if (Sound_List.SelectedIndex != -1)
            {
                IsNoSelectMode = true;
                int Index = Sound_List.SelectedIndex;
                Settings.Sounds[Sound_List.SelectedIndex].Probability = Weight_S.Value;
                string Before = Sound_List.SelectedItem.ToString().Substring(0, Sound_List.SelectedItem.ToString().IndexOf('|'));
                Sound_List.Items[Sound_List.SelectedIndex] = Before + "| 優先度:" + (int)Weight_S.Value;
                Sound_List.SelectedIndex = Index;
                IsNoSelectMode = false;
            }
        }
        void Position_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsClosing)
                return;
            IsLocationChanging = true;
            if (Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_PLAYING)
            {
                IsPlayingMouseDown = true;
                _ = Pause_Volume_Animation(false, 10);
            }
        }
        void Position_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsClosing)
                return;
            IsLocationChanging = false;
            Bass.BASS_ChannelSetPosition(Stream, Position_S.Value);
            if (IsPlayingMouseDown)
            {
                IsPaused = false;
                Play_Volume_Animation(10);
                IsPlayingMouseDown = false;
            }
        }
        private void Sound_Pause_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            _ = Pause_Volume_Animation(false, 5);
        }
        private void Sound_Play_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            Play_Volume_Animation(5);
        }
        private void Event_Play_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
        }
        private async void Sound_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsClosing || Sound_List.SelectedIndex == -1 || IsNoSelectMode)
                return;
            if (Settings.Sounds[Sound_List.SelectedIndex].File_Path.Contains("\\"))
            {
                if (!File.Exists(Settings.Sounds[Sound_List.SelectedIndex].File_Path))
                {
                    Message_Feed_Out("音声ファイルが存在しません。削除された可能性があります。");
                    return;
                }
            }
            await Pause_Volume_Animation(true, 10);
            Bass.BASS_ChannelRemoveFX(Stream, Stream_LPF);
            Bass.BASS_ChannelRemoveFX(Stream, Stream_HPF);
            Bass.BASS_FXReset(Stream_LPF);
            Bass.BASS_FXReset(Stream_HPF);
            Bass.BASS_StreamFree(Stream);
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, 100);
            int StreamHandle;
            if (Settings.Sounds[Sound_List.SelectedIndex].File_Path.Contains("\\"))
                StreamHandle = Bass.BASS_StreamCreateFile(Settings.Sounds[Sound_List.SelectedIndex].File_Path, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE);
            else
            {
                Sound_Bytes = WVS_File.Load_Sound(Settings.Sounds[Sound_List.SelectedIndex].Stream_Position);
                if (Sound_IntPtr != null && Sound_IntPtr.IsAllocated)
                    Sound_IntPtr.Free();
                Sound_IntPtr = GCHandle.Alloc(Sound_Bytes, GCHandleType.Pinned);
                StreamHandle = Bass.BASS_StreamCreateFile(Sound_IntPtr.AddrOfPinnedObject(), 0L, Sound_Bytes.Length, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE);
            }
            Stream = BassFx.BASS_FX_TempoCreate(StreamHandle, BASSFlag.BASS_FX_FREESOURCE);
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, 500);
            Position_S.Maximum = Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetLength(Stream, BASSMode.BASS_POS_BYTES));
            Settings.Sounds[Sound_List.SelectedIndex].Play_Time.Max_Time = Position_S.Maximum;
            IsMusicEnd = new SYNCPROC(EndSync);
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)All_Volume_S.Value / 100);
            Bass.BASS_ChannelSetDevice(Stream, Video_Mode.Sound_Device);
            Bass.BASS_ChannelGetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, ref Freq);
            Bass.BASS_ChannelSetSync(Stream, BASSSync.BASS_SYNC_END | BASSSync.BASS_SYNC_MIXTIME, 0, IsMusicEnd, IntPtr.Zero);
            Stream_LPF = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_BFX_BQF, 2);
            Stream_HPF = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_BFX_BQF, 1);
            Bass.BASS_FXSetParameters(Stream_LPF, LPF_Setting);
            Bass.BASS_FXSetParameters(Stream_HPF, HPF_Setting);
            Weight_S.Value = Settings.Sounds[Sound_List.SelectedIndex].Probability;
            if (Settings.Sounds[Sound_List.SelectedIndex].IsVolumeRange)
            {
                Volume_Start_S.Value = Settings.Sounds[Sound_List.SelectedIndex].Volume_Range.Start;
                Volume_End_S.Value = Settings.Sounds[Sound_List.SelectedIndex].Volume_Range.End;
                Volume_C.Source = Sub_Code.Check_03;
            }
            else
            {
                Volume_Start_S.Value = Settings.Sounds[Sound_List.SelectedIndex].Volume;
                Volume_C.Source = Sub_Code.Check_01;
            }
            if (Settings.Sounds[Sound_List.SelectedIndex].IsPitchRange)
            {
                Pitch_Start_S.Value = Settings.Sounds[Sound_List.SelectedIndex].Pitch_Range.Start;
                Pitch_End_S.Value = Settings.Sounds[Sound_List.SelectedIndex].Pitch_Range.End;
                Pitch_C.Source = Sub_Code.Check_03;
            }
            else
            {
                Pitch_Start_S.Value = Settings.Sounds[Sound_List.SelectedIndex].Pitch;
                Pitch_C.Source = Sub_Code.Check_01;
            }
            if (Settings.Sounds[Sound_List.SelectedIndex].IsLPFRange)
            {
                LPF_Start_S.Value = Settings.Sounds[Sound_List.SelectedIndex].LPF_Range.Start;
                LPF_End_S.Value = Settings.Sounds[Sound_List.SelectedIndex].LPF_Range.End;
                LPF_C.Source = Sub_Code.Check_03;
            }
            else
            {
                LPF_Start_S.Value = Settings.Sounds[Sound_List.SelectedIndex].Low_Pass_Filter;
                LPF_C.Source = Sub_Code.Check_01;
            }
            if (Settings.Sounds[Sound_List.SelectedIndex].IsHPFRange)
            {
                HPF_Start_S.Value = Settings.Sounds[Sound_List.SelectedIndex].HPF_Range.Start;
                HPF_End_S.Value = Settings.Sounds[Sound_List.SelectedIndex].HPF_Range.End;
                HPF_C.Source = Sub_Code.Check_03;
            }
            else
            {
                HPF_Start_S.Value = Settings.Sounds[Sound_List.SelectedIndex].High_Pass_Filter;
                HPF_C.Source = Sub_Code.Check_01;
            }
            Delay_S.Value = Settings.Sounds[Sound_List.SelectedIndex].Delay;
            if (Settings.Sounds[Sound_List.SelectedIndex].IsFadeIn)
                Fade_In_C.Source = Sub_Code.Check_03;
            else
                Fade_In_C.Source = Sub_Code.Check_01;
            if (Settings.Sounds[Sound_List.SelectedIndex].IsFadeOut)
                Fade_Out_C.Source = Sub_Code.Check_03;
            else
                Fade_Out_C.Source = Sub_Code.Check_01;
            Set_Pitch((int)Pitch_Start_S.Value);
            Gain = new DSP_Gain(Stream, 0);
            Change_Effect();
            IsPaused = true;
            Max_Time = Sub_Code.Get_Time_String(Position_S.Maximum);
            string Start_Time = "00:00";
            string End_Time = Max_Time;
            if (Settings.Sounds[Sound_List.SelectedIndex].Play_Time.Start_Time > 0)
                Start_Time = Sub_Code.Get_Time_String(Settings.Sounds[Sound_List.SelectedIndex].Play_Time.Start_Time);
            if (Settings.Sounds[Sound_List.SelectedIndex].Play_Time.End_Time > 0)
                End_Time = Sub_Code.Get_Time_String(Settings.Sounds[Sound_List.SelectedIndex].Play_Time.End_Time);
            Play_Time_T.Text = "再生時間:" + Start_Time + "～" + End_Time;
            Position_T.Text = "00:00 / " + Max_Time;
            Change_Range_Mode();
        }
        void Set_Pitch(int Pitch)
        {
            if (Pitch == 0)
            {
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, Freq);
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
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, Freq * (float)(1 + (Sub_Code.Pitch_Values[Key] + Plus_Freq) / 100.0));
        }
        private void Weight_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            string Message_01 = "選択している音声の優先度を設定します。";
            string Message_02 = "値が大きいほど再生される確率が上がります。\n";
            string Message_03 = "'イベントを再生'で確率を確認しながら値を決めてください。";
            Message_Feed_Out(Message_01 + Message_02 + Message_03);
        }
        private bool IsProbabilityZero()
        {
            foreach (Voice_Sound_Setting Setting in Settings.Sounds)
                if (Setting.Probability != 0)
                    return false;
            return true;
        }
        private void OK_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            if (IsProbabilityZero())
            {
                Message_Feed_Out("全てのサウンドの優先度を0にすることはできません。");
                return;
            }
            Closing();
        }
        private void Cancel_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            for (int Number = 0; Number < Settings.Sounds.Count; Number++)
                Settings.Sounds[Number] = Settings_Source.Sounds[Number];
            Closing();
        }
        async void Closing()
        {
            if (!IsClosing)
            {
                _ = Pause_Volume_Animation(true, 15);
                IsClosing = true;
                Settings_Source = null;
                Settings = null;
                while (Opacity > 0)
                {
                    Opacity -= Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
                Visibility = Visibility.Hidden;
                IsClosing = false;
            }
        }
        private void Weight_S_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Weight_S.Value = 50;
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
            if (Sound_List.SelectedIndex != -1)
            {
                if (!Settings.Sounds[Sound_List.SelectedIndex].IsVolumeRange)
                {
                    Settings.Sounds[Sound_List.SelectedIndex].Volume = e.NewValue;
                    Gain.Gain_dBV = e.NewValue;
                }
                else
                    Settings.Sounds[Sound_List.SelectedIndex].Volume_Range.Start = e.NewValue;
            }
        }
        private void Volume_End_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsClosing)
                return;
            Volume_End_T.Text = "～:" + (int)e.NewValue;
            if (Sound_List.SelectedIndex != -1)
                Settings.Sounds[Sound_List.SelectedIndex].Volume_Range.End = e.NewValue;
        }
        private void Pitch_Start_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsClosing)
                return;
            Pitch_Start_T.Text = "ピッチ:" + (int)e.NewValue;
            if (Sound_List.SelectedIndex != -1)
            {
                if (!Settings.Sounds[Sound_List.SelectedIndex].IsPitchRange)
                {
                    Settings.Sounds[Sound_List.SelectedIndex].Pitch = (int)e.NewValue;
                    Set_Pitch(Settings.Sounds[Sound_List.SelectedIndex].Pitch);
                }
                else
                    Settings.Sounds[Sound_List.SelectedIndex].Pitch_Range.Start = (int)Pitch_Start_S.Value;
            }
        }
        private void Pitch_End_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsClosing)
                return;
            Pitch_End_T.Text = "～:" + (int)e.NewValue;
            if (Sound_List.SelectedIndex != -1)
                Settings.Sounds[Sound_List.SelectedIndex].Pitch_Range.End = (int)e.NewValue;
        }
        private async void LPF_Start_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsClosing)
                return;
            LPF_Start_T.Text = "LPF:" + (int)e.NewValue;
            int NewValue = (int)e.NewValue == 0 ? 1 : (int)e.NewValue;
            if (Sound_List.SelectedIndex == -1)
                return;
            if (LPF_Before_Value < NewValue)
                LPF_Before_Value = NewValue;
            while (IsLPFMoving)
                await Task.Delay(50);
            if (LPF_Before_Value != NewValue)
                return;
            if (!Settings.Sounds[Sound_List.SelectedIndex].IsLPFRange)
            {
                LPF_Setting.fCenter = (float)Get_LPF_Value(NewValue);
                Settings.Sounds[Sound_List.SelectedIndex].Low_Pass_Filter = (int)e.NewValue;
                Bass.BASS_FXSetParameters(Stream_LPF, LPF_Setting);
            }
            else
                Settings.Sounds[Sound_List.SelectedIndex].LPF_Range.Start = (int)e.NewValue;
            LPF_Before_Value = 0;
        }
        private void LPF_End_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsClosing)
                return;
            LPF_End_T.Text = "～:" + (int)e.NewValue;
            if (Sound_List.SelectedIndex == -1)
                return;
            Settings.Sounds[Sound_List.SelectedIndex].LPF_Range.End = (int)e.NewValue;
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
            if (Sound_List.SelectedIndex == -1)
                return;
            if (HPF_Before_Value < NewValue)
                HPF_Before_Value = NewValue;
            while (IsHPFMoving)
                await Task.Delay(50);
            if (HPF_Before_Value != NewValue)
                return;
            if (!Settings.Sounds[Sound_List.SelectedIndex].IsHPFRange)
            {
                HPF_Setting.fCenter = (float)Get_HPF_Value(NewValue);
                Settings.Sounds[Sound_List.SelectedIndex].High_Pass_Filter = (int)e.NewValue;
                Bass.BASS_FXSetParameters(Stream_HPF, HPF_Setting);
            }
            else
                Settings.Sounds[Sound_List.SelectedIndex].HPF_Range.End = (int)e.NewValue;
            HPF_Before_Value = 0;
        }
        private void HPF_End_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsClosing)
                return;
            HPF_End_T.Text = "～:" + (int)e.NewValue;
            if (Sound_List.SelectedIndex == -1)
                return;
            Settings.Sounds[Sound_List.SelectedIndex].HPF_Range.End = (int)e.NewValue;
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
                HPF_Setting.fCenter = (float)Value;
                Bass.BASS_FXSetParameters(Stream_HPF, HPF_Setting);
                await Task.Delay(1000 / 60);
            }
            Settings.Sounds[Sound_List.SelectedIndex].High_Pass_Filter = a;
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
                LPF_Setting.fCenter = (float)Value;
                Bass.BASS_FXSetParameters(Stream_LPF, LPF_Setting);
                await Task.Delay(1000 / 60);
            }
            Settings.Sounds[Sound_List.SelectedIndex].Low_Pass_Filter = a;
            IsLPFMoving = false;
        }
        private void Delay_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Sound_List.SelectedIndex == -1)
            {
                e.Handled = true;
                return;
            }
            double Value = Math.Round(e.NewValue, 2, MidpointRounding.AwayFromZero);
            Settings.Sounds[Sound_List.SelectedIndex].Delay = Value;
            Delay_T.Text = "遅延:" + Value + "秒";
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
        private void Slider_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }
        private void Time_Start_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || Sound_List.SelectedIndex == -1)
                return;
            Settings.Sounds[Sound_List.SelectedIndex].Play_Time.Start_Time = Position_S.Value;
            double End_Time = Settings.Sounds[Sound_List.SelectedIndex].Play_Time.End_Time;
            if (End_Time != 0 && Settings.Sounds[Sound_List.SelectedIndex].Play_Time.Start_Time > End_Time)
            {
                Settings.Sounds[Sound_List.SelectedIndex].Play_Time.End_Time = 0;
                Play_Time_T.Text = "再生時間:" + Sub_Code.Get_Time_String(Settings.Sounds[Sound_List.SelectedIndex].Play_Time.Start_Time) + " ～ " + Max_Time;
                Message_Feed_Out("開始時間が終了時間より大きかったため、終了時間を最大にします。");
            }
            else if (End_Time != 0)
                Play_Time_T.Text = "再生時間:" + Sub_Code.Get_Time_String(Settings.Sounds[Sound_List.SelectedIndex].Play_Time.Start_Time) + " ～ " + Sub_Code.Get_Time_String(End_Time);
            else
                Play_Time_T.Text = "再生時間:" + Sub_Code.Get_Time_String(Settings.Sounds[Sound_List.SelectedIndex].Play_Time.Start_Time) + "～" + Max_Time;
        }
        private void Time_End_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || Sound_List.SelectedIndex == -1)
                return;
            Settings.Sounds[Sound_List.SelectedIndex].Play_Time.End_Time = Position_S.Value;
            if (Settings.Sounds[Sound_List.SelectedIndex].Play_Time.End_Time < Settings.Sounds[Sound_List.SelectedIndex].Play_Time.Start_Time)
            {
                Settings.Sounds[Sound_List.SelectedIndex].Play_Time.Start_Time = 0;
                Message_Feed_Out("終了時間が開始時間より小さかったため、開始時間を0秒にします。");
            }
            double End_Time = Settings.Sounds[Sound_List.SelectedIndex].Play_Time.End_Time;
            Play_Time_T.Text = "再生時間:" + Sub_Code.Get_Time_String(Settings.Sounds[Sound_List.SelectedIndex].Play_Time.Start_Time) + " ～ " + Sub_Code.Get_Time_String(End_Time);
        }
        private void Time_Clear_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || Sound_List.SelectedIndex == -1)
                return;
            Settings.Sounds[Sound_List.SelectedIndex].Play_Time.Start_Time = 0;
            Settings.Sounds[Sound_List.SelectedIndex].Play_Time.End_Time = 0;
            Play_Time_T.Text = "再生時間:00:00～" + Max_Time;
        }
        private void Range_C_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Sound_List.SelectedIndex == -1)
                return;
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
                Settings.Sounds[Sound_List.SelectedIndex].IsVolumeRange = IsEnable;
                if (IsEnable)
                {
                    Volume_Start_S.Value = Settings.Sounds[Sound_List.SelectedIndex].Volume_Range.Start;
                    Volume_End_S.Value = Settings.Sounds[Sound_List.SelectedIndex].Volume_Range.End;
                }
                else
                    Volume_Start_S.Value = Settings.Sounds[Sound_List.SelectedIndex].Volume;
                Change_Effect(0);
            }
            else if (Check.Name == "Pitch_C")
            {
                Settings.Sounds[Sound_List.SelectedIndex].IsPitchRange = IsEnable;
                if (IsEnable)
                {
                    Pitch_Start_S.Value = Settings.Sounds[Sound_List.SelectedIndex].Pitch_Range.Start;
                    Pitch_End_S.Value = Settings.Sounds[Sound_List.SelectedIndex].Pitch_Range.End;
                }
                else
                    Pitch_Start_S.Value = Settings.Sounds[Sound_List.SelectedIndex].Pitch;
                Change_Effect(1);
            }
            else if (Check.Name == "LPF_C")
            {
                Settings.Sounds[Sound_List.SelectedIndex].IsLPFRange = IsEnable;
                if (IsEnable)
                {
                    LPF_Start_S.Value = Settings.Sounds[Sound_List.SelectedIndex].LPF_Range.Start;
                    LPF_End_S.Value = Settings.Sounds[Sound_List.SelectedIndex].LPF_Range.End;
                }
                else
                    LPF_Start_S.Value = Settings.Sounds[Sound_List.SelectedIndex].Low_Pass_Filter;
                Change_Effect(2);
            }
            else if (Check.Name == "HPF_C")
            {
                Settings.Sounds[Sound_List.SelectedIndex].IsHPFRange = IsEnable;
                if (IsEnable)
                {
                    HPF_Start_S.Value = Settings.Sounds[Sound_List.SelectedIndex].HPF_Range.Start;
                    HPF_End_S.Value = Settings.Sounds[Sound_List.SelectedIndex].HPF_Range.End;
                }
                else
                    HPF_Start_S.Value = Settings.Sounds[Sound_List.SelectedIndex].High_Pass_Filter;
                Change_Effect(3);
            }
            else if (Check.Name == "Fade_In_C")
                Settings.Sounds[Sound_List.SelectedIndex].IsFadeIn = IsEnable;
            else if (Check.Name == "Fade_Out_C")
                Settings.Sounds[Sound_List.SelectedIndex].IsFadeOut = IsEnable;
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
            if (Sound_List.SelectedIndex != -1)
            {
                Voice_Sound_Setting Info = Settings.Sounds[Sound_List.SelectedIndex];
                Volume_End_T.Visibility = Info.IsVolumeRange ? Visibility.Visible : Visibility.Hidden;
                Volume_End_S.Visibility = Info.IsVolumeRange ? Visibility.Visible : Visibility.Hidden;
                Pitch_End_T.Visibility = Info.IsPitchRange ? Visibility.Visible : Visibility.Hidden;
                Pitch_End_S.Visibility = Info.IsPitchRange ? Visibility.Visible : Visibility.Hidden;
                LPF_End_T.Visibility = Info.IsLPFRange ? Visibility.Visible : Visibility.Hidden;
                LPF_End_S.Visibility = Info.IsLPFRange ? Visibility.Visible : Visibility.Hidden;
                HPF_End_T.Visibility = Info.IsHPFRange ? Visibility.Visible : Visibility.Hidden;
                HPF_End_S.Visibility = Info.IsHPFRange ? Visibility.Visible : Visibility.Hidden;
                if (Info.IsVolumeRange || Info.IsPitchRange || Info.IsLPFRange || Info.IsHPFRange)
                    Update_Effect_B.Visibility = Visibility.Visible;
            }
            else
            {
                Volume_End_T.Visibility = Visibility.Hidden;
                Volume_End_S.Visibility = Visibility.Hidden;
                Pitch_End_T.Visibility = Visibility.Hidden;
                Pitch_End_S.Visibility = Visibility.Hidden;
                LPF_End_T.Visibility = Visibility.Hidden;
                LPF_End_S.Visibility = Visibility.Hidden;
                HPF_End_T.Visibility = Visibility.Hidden;
                HPF_End_S.Visibility = Visibility.Hidden;
            }
        }
        void Change_Effect(int Mode = -1)
        {
            if (Sound_List.SelectedIndex == -1)
                return;
            if (Mode == -1 || Mode == 0)
            {
                double Volume;
                if (Settings.Sounds[Sound_List.SelectedIndex].IsVolumeRange)
                {
                    if (Settings.Sounds[Sound_List.SelectedIndex].Volume_Range.End >= Settings.Sounds[Sound_List.SelectedIndex].Volume_Range.Start)
                        Volume = Sub_Code.Get_Random_Double(Settings.Sounds[Sound_List.SelectedIndex].Volume_Range.Start, Settings.Sounds[Sound_List.SelectedIndex].Volume_Range.End);
                    else
                        Volume = Sub_Code.Get_Random_Double(Settings.Sounds[Sound_List.SelectedIndex].Volume_Range.End, Settings.Sounds[Sound_List.SelectedIndex].Volume_Range.Start);
                }
                else
                    Volume = Settings.Sounds[Sound_List.SelectedIndex].Volume;
                Gain.Gain_dBV = Volume;
            }
            if (Mode == -1 || Mode == 1)
            {
                int Pitch;
                if (Settings.Sounds[Sound_List.SelectedIndex].IsPitchRange)
                {
                    if (Settings.Sounds[Sound_List.SelectedIndex].Pitch_Range.End >= Settings.Sounds[Sound_List.SelectedIndex].Pitch_Range.Start)
                        Pitch = Sub_Code.r.Next(Settings.Sounds[Sound_List.SelectedIndex].Pitch_Range.Start, Settings.Sounds[Sound_List.SelectedIndex].Pitch_Range.End + 1);
                    else
                        Pitch = Sub_Code.r.Next(Settings.Sounds[Sound_List.SelectedIndex].Pitch_Range.End, Settings.Sounds[Sound_List.SelectedIndex].Pitch_Range.Start);
                }
                else
                    Pitch = Settings.Sounds[Sound_List.SelectedIndex].Pitch;
                Set_Pitch(Pitch);
            }
            if (Mode == -1 || Mode == 2)
            {
                int LPF;
                if (Settings.Sounds[Sound_List.SelectedIndex].IsLPFRange)
                {
                    if (Settings.Sounds[Sound_List.SelectedIndex].LPF_Range.End >= Settings.Sounds[Sound_List.SelectedIndex].LPF_Range.Start)
                        LPF = Sub_Code.r.Next(Settings.Sounds[Sound_List.SelectedIndex].LPF_Range.Start, Settings.Sounds[Sound_List.SelectedIndex].LPF_Range.End + 1);
                    else
                        LPF = Sub_Code.r.Next(Settings.Sounds[Sound_List.SelectedIndex].LPF_Range.End, Settings.Sounds[Sound_List.SelectedIndex].LPF_Range.Start);
                }
                else
                    LPF = Settings.Sounds[Sound_List.SelectedIndex].Low_Pass_Filter;
                LPF_Setting.fCenter = (float)Get_LPF_Value(LPF);
                Bass.BASS_FXSetParameters(Stream_LPF, LPF_Setting);
            }
            if (Mode == -1 || Mode == 3)
            {
                int HPF;
                if (Settings.Sounds[Sound_List.SelectedIndex].IsHPFRange)
                {
                    if (Settings.Sounds[Sound_List.SelectedIndex].HPF_Range.End >= Settings.Sounds[Sound_List.SelectedIndex].HPF_Range.Start)
                        HPF = Sub_Code.r.Next(Settings.Sounds[Sound_List.SelectedIndex].HPF_Range.Start, Settings.Sounds[Sound_List.SelectedIndex].HPF_Range.End + 1);
                    else
                        HPF = Sub_Code.r.Next(Settings.Sounds[Sound_List.SelectedIndex].HPF_Range.End, Settings.Sounds[Sound_List.SelectedIndex].HPF_Range.Start);
                }
                else
                    HPF = Settings.Sounds[Sound_List.SelectedIndex].High_Pass_Filter;
                HPF_Setting.fCenter = (float)Get_HPF_Value(HPF);
                Bass.BASS_FXSetParameters(Stream_HPF, HPF_Setting);
            }
        }
        private void Update_Effect_B_Click(object sender, RoutedEventArgs e)
        {
            Change_Effect();
        }
        private async void Event_Setting_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            if (IsProbabilityZero())
            {
                Message_Feed_Out("全てのサウンドの優先度を0にすることはできません。");
                return;
            }
            _ = Pause_Volume_Animation(false, 15);
            Voice_Create_Event_Setting_Window.Window_Show(Event_Name, Settings, WVS_File, List_Index_Mode, Index);
            while (!Voice_Create_Event_Setting_Window.IsClosing)
            {
                if (Layout_Parent.Opacity > 0)
                {
                    Layout_Parent.Opacity -= Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
                else
                    await Task.Delay(50);
            }
            while (Layout_Parent.Opacity < 1)
            {
                Layout_Parent.Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
        }
        private void Volume_Help_B_Click(object sender, RoutedEventArgs e)
        {
            Message_Feed_Out("この値はMod Creater内で再生する際の音量で、値を変更してもWoTBには反映されません。\nWoTBに反映させる場合は\"ゲイン(db)\"の値を変更してください。");
        }
        private void Delay_Help_B_Click(object sender, RoutedEventArgs e)
        {
            Message_Feed_Out("この値は指定のサウンドに設定されます。音声全体を遅延させる場合はイベント設定で行ってください。\nこの値はイベント設定内でのみ有効化されます。");
        }
    }
}