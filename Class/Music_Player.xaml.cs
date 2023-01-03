using FMOD_API;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Linq;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

public class Video_Mode
{
    public static double Width = 1920;
    public static bool IsVideoClicked = false;
    public static int Sound_Device = -1;
}
namespace WoTB_Voice_Mod_Creater.Class
{
    public class Music_Child_Class
    {
        public string File_Full_Path { get; private set; }
        public string File_Name_Path { get; set; }
        public uint ID;
        public bool IsPlayed = false;
        public Music_Child_Class(string File_Full_Path, string File_Name_Path)
        {
            this.File_Full_Path = File_Full_Path;
            this.File_Name_Path = File_Name_Path;
            ID = (uint)Sub_Code.r.Next(10000, 100000);
        }
    }
    public partial class Music_Player : System.Windows.Controls.UserControl
    {
        readonly List<List<Music_Child_Class>> Music_Data = new List<List<Music_Child_Class>>();
        readonly List<uint> Played_IDs = new List<uint>();
        readonly List<byte[]> Thumbnails = new List<byte[]>();
        readonly System.Windows.Controls.ContextMenu pMenu;
        string Video_Mode_Select_Name = "";
        string Playing_Music_Name_Now = "";
        bool IsBusy = false;
        bool IsLocationChanging = false;
        bool IsPaused = false;
        bool IsEnded = false;
        bool IsVideoClicked = false;
        bool IsVideoEnter = false;
        bool IsNotMusicChange = false;
        bool IsSaveOK = false;
        bool IsWaveGrayLoaded = false;
        bool IsWaveColorLoaded = false;
        bool IsPlayingMouseDown = false;
        bool IsSyncPitch_And_Speed = false;
        bool IsLeftKeyDown = false;
        bool IsRightKeyDown = false;
        bool IsSpaceKeyDown = false;
        bool IsLControlKeyDown = false;
        bool IsMKeyDown = false;
        bool IsFKeyDown = false;
        bool IsESCKeyDown = false;
        bool IsVolume_Speed_Changed_By_Key = false;
        bool IsFullScreen = false;
        bool IsVideoMode = false;
        bool IsRenameClosing = false;
        System.Windows.Point Mouse_Point = new System.Windows.Point(0, 0);
        System.Windows.Point Video_Point = new System.Windows.Point(0, 0);
        System.Windows.Media.Imaging.BitmapImage Wave_Gray_Image_Source = null;
        System.Windows.Media.Imaging.BitmapImage Wave_Color_Image_Source = null;
        Un4seen.Bass.Misc.WaveForm WF_Gray = null;
        Un4seen.Bass.Misc.WaveForm WF_Color = null;
        BASS_BFX_BQF LPF_Setting = new BASS_BFX_BQF(BASSBFXBQF.BASS_BFX_BQF_LOWPASS, 500f, 0f, 0.707f, 0f, 0f, BASSFXChan.BASS_BFX_CHANALL);
        BASS_BFX_BQF HPF_Setting = new BASS_BFX_BQF(BASSBFXBQF.BASS_BFX_BQF_HIGHPASS, 1000f, 0f, 0.707f, 0f, 0f, BASSFXChan.BASS_BFX_CHANALL);
        BASS_BFX_ECHO4 ECHO_Setting = new BASS_BFX_ECHO4(0, 0, 0, 0, true, BASSFXChan.BASS_BFX_CHANALL);
        int Stream;
        int Stream_LPF = 0;
        int Stream_HPF = 0;
        int Stream_ECHO = 0;
        int SetFirstDevice = -1;
        int WAVEForm_Image_Width = 0;
        int WAVEForm_Image_Height = 0;
        int Thumbnail_Index_Now = -1;
        int Fade_Count = 0;
        int Music_Rename_Index = -1;
        //曲のリストのインデックス:0～8
        int Music_Select_List = 0;
        float Music_Frequency = 44100f;
        double X_Move = 0;
        double Y_Move = 0;
        double Start_Time = -1;
        double End_Time = -1;
        SYNCPROC IsMusicEnd;
        BASS_DEVICEINFO info = new BASS_DEVICEINFO();
        public Music_Player()
        {
            InitializeComponent();
            Video_Change_B.Visibility = Visibility.Hidden;
            Thumbnail_Main.Visibility = Visibility.Hidden;
            Thumbnail_Sub.Visibility = Visibility.Hidden;
            Thumbnail_Sub.Opacity = 0;
            Video_V.LoadedBehavior = MediaState.Manual;
            Video_V.UnloadedBehavior = MediaState.Stop;
            Video_V.Stretch = System.Windows.Media.Stretch.Uniform;
            Opacity = 0;
            Location_S.AddHandler(MouseDownEvent, new MouseButtonEventHandler(Location_MouseDown), true);
            Location_S.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Location_MouseUp), true);
            Speed_S.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Speed_MouseUp), true);
            Volume_S.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Volume_MouseUp), true);
            Pitch_Speed_S.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Pitch_Speed_S_MouseUp), true);
            Video_Mode_Change(false);
            Video_V.MediaFailed += Video_V_MediaFailed;
            Location_S.Maximum = 0;
            Video_V.MaxWidth = 5760;
            Video_V.MaxHeight = 3240;
            Pitch_Speed_S.Value = 50;
            Bass.BASS_Init(-1, 48000, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            SetFirstDevice = Bass.BASS_GetDevice();
            Device_L.SelectedIndex = SetFirstDevice - 1;
            Bass.BASS_Free();
            for (int n = 1; Bass.BASS_GetDeviceInfo(n, info); n++)
            {
                Bass.BASS_Init(n, 48000, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
                Device_L.Items.Add(info.name);
            }
            WAVEForm_Image_Width = (int)WAVEForm_Gray_Image.Width;
            WAVEForm_Image_Height = (int)WAVEForm_Gray_Image.Height;
            for (int Number = 0; Number < 9; Number++)
                Music_Data.Add(new List<Music_Child_Class>());
            Position_Change();
            Music_Player_Setting_Window.ChangeLPFEnable += delegate (bool IsEnable)
            {
                if (IsEnable)
                {
                    LPF_Setting.fCenter = 500f + 4000f * (1 - (float)Music_Player_Setting_Window.LPF_S.Value / 100f);
                    Bass.BASS_FXSetParameters(Stream_LPF, LPF_Setting);
                }
                else
                {
                    Bass.BASS_ChannelRemoveFX(Stream, Stream_LPF);
                    Stream_LPF = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_BFX_BQF, 2);
                }
            };
            Music_Player_Setting_Window.ChangeHPFEnable += delegate (bool IsEnable)
            {
                if (IsEnable)
                {
                    HPF_Setting.fCenter = 100f + 4000f * (float)Music_Player_Setting_Window.HPF_S.Value / 100f;
                    Bass.BASS_FXSetParameters(Stream_HPF, HPF_Setting);
                }
                else
                {
                    Bass.BASS_ChannelRemoveFX(Stream, Stream_HPF);
                    Stream_HPF = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_BFX_BQF, 1);
                }
            };
            Music_Player_Setting_Window.ChangeECHOEnable += delegate (bool IsEnable)
            {
                if (IsEnable)
                {
                    Bass.BASS_ChannelRemoveFX(Stream, Stream_ECHO);
                    Stream_ECHO = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_BFX_ECHO4, 0);
                    ECHO_Setting.fDelay = (float)Music_Player_Setting_Window.ECHO_Delay_S.Value;
                    ECHO_Setting.fDryMix = (float)Music_Player_Setting_Window.ECHO_Power_Original_S.Value / 100f;
                    ECHO_Setting.fWetMix = (float)Music_Player_Setting_Window.ECHO_Power_ECHO_S.Value / 100f;
                    ECHO_Setting.fFeedback = (float)Music_Player_Setting_Window.ECHO_Length_S.Value / 100f;
                    Bass.BASS_FXSetParameters(Stream_ECHO, ECHO_Setting);
                }
                else
                {
                    Bass.BASS_ChannelRemoveFX(Stream, Stream_ECHO);
                    Stream_ECHO = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_BFX_ECHO4, 0);
                    ECHO_Setting.fWetMix = 0;
                    Bass.BASS_FXSetParameters(Stream_ECHO, ECHO_Setting);
                }
            };
            pMenu = new System.Windows.Controls.ContextMenu();
            System.Windows.Controls.MenuItem item1 = new System.Windows.Controls.MenuItem();
            item1.Header = "名前を変更";
            item1.Click += Music_Rename_Click;
            pMenu.Items.Add(item1);
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Zoom_S.Value = 1;
            bool IsListSave = false;
            if (File.Exists(Voice_Set.Special_Path + "/Configs/Other_Music_List.dat") && Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_STOPPED)
            {
                try
                {
                    StreamReader str = Sub_Code.File_Decrypt_To_Stream(Voice_Set.Special_Path + "/Configs/Other_Music_List.dat", "SRTTbacon_Music_List_Save");
                    for (int Number = 0; Number < 9; Number++)
                        Music_Data[Number].Clear();
                    string line;
                    while ((line = str.ReadLine()) != null)
                    {
                        if (line != "")
                        {
                            int Index;
                            string Full_Path = "";
                            string Name_Path = "";
                            if (line.Contains("|"))
                            {
                                string[] Split = line.Split('|');
                                if (Split.Length == 2)
                                {
                                    Index = int.Parse(line.Substring(0, line.IndexOf('|')));
                                    Full_Path = line.Substring(line.IndexOf('|') + 1);
                                    Name_Path = line.Substring(line.LastIndexOf('\\') + 1);
                                }
                                else
                                {
                                    Index = int.Parse(Split[0]);
                                    Full_Path = Split[1];
                                    Name_Path = Split[2];
                                }
                            }
                            else
                            {
                                Index = 0;
                                Full_Path = line;
                                Name_Path = line.Substring(line.LastIndexOf('\\') + 1);
                                IsListSave = true;
                            }
                            Music_Child_Class Child = new Music_Child_Class(Full_Path, Name_Path);
                            Music_Data[Index].Add(Child);
                        }
                    }
                    str.Close();
                    Music_List_Sort();
                    Played_IDs.Clear();
                    if (IsListSave)
                        Music_List_Save();
                }
                catch (Exception e1)
                {
                    System.Windows.MessageBox.Show("リストが破損しているためファイルを読み込めませんでした。\nエラー回避のためリストは削除されます。");
                    File.Delete(Voice_Set.Special_Path + "/Configs/Other_Music_List.dat");
                    for (int Number = 0; Number < 9; Number++)
                        Music_Data[Number].Clear();
                    Music_List.Items.Clear();
                    Sub_Code.Error_Log_Write(e1.Message);
                }
            }
            if (File.Exists(Voice_Set.Special_Path + "/Configs/Music_Player.conf"))
            {
                try
                {
                    StreamReader str = Sub_Code.File_Decrypt_To_Stream(Voice_Set.Special_Path + "/Configs/Music_Player.conf", "Music_Player_Configs_Save");
                    Video_Mode_C.IsChecked = bool.Parse(str.ReadLine());
                    Loop_C.IsChecked = bool.Parse(str.ReadLine());
                    Random_C.IsChecked = bool.Parse(str.ReadLine());
                    Background_C.IsChecked = bool.Parse(str.ReadLine());
                    Ex_Sort_C.IsChecked = bool.Parse(str.ReadLine());
                    Volume_S.Value = double.Parse(str.ReadLine());
                    //V1.2.9以前のデータでは読み込まれません
                    try
                    {
                        _ = bool.Parse(str.ReadLine());
                        WAVEForm_Gray_Image.Visibility = Visibility.Visible;
                        WAVEForm_Color_Image.Visibility = Visibility.Visible;
                        Mode_C.IsChecked = bool.Parse(str.ReadLine());
                        Music_Play_Mode_Change(Mode_C.IsChecked.Value);
                        Music_List_Change(int.Parse(str.ReadLine()));
                    }
                    catch
                    {
                    }
                    str.Close();
                }
                catch (Exception e1)
                {
                    System.Windows.MessageBox.Show("設定を読み込めませんでした。。\nエラー回避のため設定は削除されます。");
                    File.Delete(Voice_Set.Special_Path + "/Configs/Music_Player.conf");
                    Video_Mode_C.IsChecked = false;
                    Loop_C.IsChecked = false;
                    Random_C.IsChecked = false;
                    Background_C.IsChecked = false;
                    Ex_Sort_C.IsChecked = false;
                    Sub_Code.Error_Log_Write(e1.Message);
                }
            }
            Music_List_Sort();
            IsSaveOK = true;
        }
        private void Video_V_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Video_V.Stop();
            Video_V.Close();
            Video_V.Source = new Uri(Music_Data[Music_Select_List][Music_List.SelectedIndex].File_Full_Path);
            Video_V.Volume = 0;
            Video_V.Play();
            long position = Bass.BASS_ChannelGetPosition(Stream);
            TimeSpan time = TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(Stream, position));
            Video_V.Position = time;
        }
        public async void Window_Show()
        {
            Visibility = Visibility.Visible;
            if (Video_V.Source != null && Music_Fix_B.Visibility == Visibility.Visible)
            {
                Video_V.Play();
                long position2 = Bass.BASS_ChannelGetPosition(Stream);
                Location_S.Value = Bass.BASS_ChannelBytes2Seconds(Stream, position2);
                TimeSpan time = TimeSpan.FromSeconds(Location_S.Value);
                Video_V.Position = time;
                Video_V.Visibility = Visibility.Visible;
                if (Bass.BASS_ChannelIsActive(Stream) != BASSActive.BASS_ACTIVE_PLAYING)
                    Video_V.Pause();
            }
            Window_Bar_Canvas.Margin = new Thickness(0, 0, 0, 0);
            if (Sub_Code.IsWindowBarShow && Video_V.Visibility == Visibility.Visible)
                Window_Bar_Canvas.Margin = new Thickness(0, 25, 0, 0);
            while (Opacity < 1 && !IsBusy)
            {
                Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
        }
        async void Position_Change()
        {
            double nextFrame = (double)Environment.TickCount;
            float period = 1000f / 30f;
            while (true)
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
                bool IsPlaying = Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_PLAYING;
                if (IsPlaying)
                {
                    Bass.BASS_ChannelUpdate(Stream, 400);
                    if (Start_Time != -1 && Location_S.Value >= End_Time)
                    {
                        Music_Pos_Change(Start_Time, true);
                        Set_Position_Slider();
                        TimeSpan time = TimeSpan.FromSeconds(Location_S.Value);
                        Video_V.Position = time;
                    }
                    else if (Start_Time != -1 && Location_S.Value < Start_Time)
                    {
                        Music_Pos_Change(Start_Time, true);
                        Set_Position_Slider();
                        TimeSpan time = TimeSpan.FromSeconds(Location_S.Value);
                        Video_V.Position = time;
                    }
                    if (Thumbnails.Count >= 1 && Location_S.Maximum >= 60)
                    {
                        double Now_Percent = Location_S.Value / Location_S.Maximum;
                        if (Now_Percent >= 0.75 && Thumbnail_Index_Now != 3 && Thumbnails.Count >= 4)
                            Change_Thumbnail_Fade(3);
                        else if (Now_Percent >= 0.5 && Now_Percent < 0.75 && Thumbnail_Index_Now != 2 && Thumbnails.Count >= 3)
                            Change_Thumbnail_Fade(2);
                        else if (Now_Percent >= 0.25 && Now_Percent < 0.5 && Thumbnail_Index_Now != 1 && Thumbnails.Count >= 2)
                            Change_Thumbnail_Fade(1);
                        else if (Now_Percent < 0.25 && Thumbnail_Index_Now != 0 && Thumbnails.Count >= 1)
                            Change_Thumbnail_Fade(0);
                    }
                }
                if (Visibility == Visibility.Visible)
                {
                    if (IsPlaying && !IsLocationChanging)
                    {
                        Set_Position_Slider();
                        if (IsWaveGrayLoaded)
                        {
                            IsWaveGrayLoaded = false;
                            WAVEForm_Gray_Image.Source = Wave_Gray_Image_Source;
                        }
                        if (IsWaveColorLoaded)
                        {
                            IsWaveColorLoaded = false;
                            WAVEForm_Color_Image.Source = Wave_Color_Image_Source;
                        }
                        if (WAVEForm_Color_Image.Visibility == Visibility.Visible)
                            WAVEForm_Color_Image.Width = (Location_S.Value / Location_S.Maximum) * WAVEForm_Image_Width;
                        TimeSpan Time = TimeSpan.FromSeconds(Location_S.Value);
                        string Minutes = Time.Minutes.ToString();
                        string Seconds = Time.Seconds.ToString();
                        if (Time.Minutes < 10)
                            Minutes = "0" + Time.Minutes;
                        if (Time.Seconds < 10)
                            Seconds = "0" + Time.Seconds;
                        Location_T.Text = Minutes + ":" + Seconds;
                    }
                    if (Music_Fix_B.Visibility == Visibility.Hidden && Video_V.Visibility == Visibility.Visible && !IsFullScreen)
                    {
                        Video_V.Pause();
                        Video_V.Visibility = Visibility.Hidden;
                    }
                    if (Stream_LPF != 0 && Music_Player_Setting_Window.Visibility == Visibility.Visible)
                    {
                        if (Music_Player_Setting_Window.IsLPFChanged && Music_Player_Setting_Window.IsLPFEnable)
                        {
                            LPF_Setting.fCenter = 500f + 4000f * (1 - (float)Music_Player_Setting_Window.LPF_S.Value / 100f);
                            Bass.BASS_FXSetParameters(Stream_LPF, LPF_Setting);
                            Music_Player_Setting_Window.IsLPFChanged = false;
                        }
                        if (Music_Player_Setting_Window.IsHPFChanged && Music_Player_Setting_Window.IsHPFEnable)
                        {
                            HPF_Setting.fCenter = 100f + 4000f * (float)Music_Player_Setting_Window.HPF_S.Value / 100f;
                            Bass.BASS_FXSetParameters(Stream_HPF, HPF_Setting);
                            Music_Player_Setting_Window.IsHPFChanged = false;
                        }
                        if (Music_Player_Setting_Window.IsECHOChanged && Music_Player_Setting_Window.IsECHOEnable)
                        {
                            ECHO_Setting.fDelay = (float)Music_Player_Setting_Window.ECHO_Delay_S.Value;
                            ECHO_Setting.fDryMix = (float)Music_Player_Setting_Window.ECHO_Power_Original_S.Value / 100f;
                            ECHO_Setting.fWetMix = (float)Music_Player_Setting_Window.ECHO_Power_ECHO_S.Value / 100f;
                            ECHO_Setting.fFeedback = (float)Music_Player_Setting_Window.ECHO_Length_S.Value / 100f;
                            Bass.BASS_FXSetParameters(Stream_ECHO, ECHO_Setting);
                            Music_Player_Setting_Window.IsECHOChanged = false;
                        }
                    }
                    if (Sub_Code.IsForcusWindow && Youtube_Link_Window.Visibility == Visibility.Hidden && Vocal_Inst_Cut_User_Window.Visibility == Visibility.Hidden &&
                        Rename_Canvas.Visibility == Visibility.Hidden)
                    {
                        bool IsLeft_or_RightPushed = false;
                        if ((Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) > 0)
                        {
                            Music_Minus_B.Content = "-10秒";
                            Music_Plus_B.Content = "+10秒";
                            IsLControlKeyDown = true;
                        }
                        else if (IsLControlKeyDown)
                        {
                            Music_Minus_B.Content = "-5秒";
                            Music_Plus_B.Content = "+5秒";
                            IsLControlKeyDown = false;
                        }
                        if ((Keyboard.GetKeyStates(Key.Left) & KeyStates.Down) > 0)
                        {
                            if (!IsLeftKeyDown && !IsLeft_or_RightPushed)
                                Music_Minus_B.PerformClick();
                            IsLeftKeyDown = true;
                            IsLeft_or_RightPushed = true;
                        }
                        else
                            IsLeftKeyDown = false;
                        if ((Keyboard.GetKeyStates(Key.Right) & KeyStates.Down) > 0)
                        {
                            if (!IsRightKeyDown && !IsLeft_or_RightPushed)
                                Music_Plus_B.PerformClick();
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
                                    Music_Pause_B.PerformClick();
                                else
                                    Music_Play_B.PerformClick();
                            }
                            IsSpaceKeyDown = true;
                        }
                        else
                            IsSpaceKeyDown = false;
                        if ((Keyboard.GetKeyStates(Key.V) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.Up) & KeyStates.Down) > 0)
                        {
                            double Increase = 1;
                            if (Volume_S.Value + Increase > 100)
                                Volume_S.Value = 100;
                            else
                                Volume_S.Value += Increase;
                        }
                        else if ((Keyboard.GetKeyStates(Key.V) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.Down) & KeyStates.Down) > 0)
                        {
                            double Increase = 1;
                            if (Volume_S.Value - Increase < 0)
                                Volume_S.Value = 0;
                            else
                                Volume_S.Value -= Increase;
                        }
                        if ((Keyboard.GetKeyStates(Key.S) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.Up) & KeyStates.Down) > 0)
                        {
                            if (IsSyncPitch_And_Speed)
                            {
                                double Increase = 0.6;
                                if (Pitch_Speed_S.Value + Increase > 100)
                                    Pitch_Speed_S.Value = 100;
                                else
                                    Pitch_Speed_S.Value += Increase;
                            }
                            else
                            {
                                if (Speed_S.Value + 1 > 100)
                                    Speed_S.Value = 100;
                                else
                                    Speed_S.Value += 1;
                            }
                            IsVolume_Speed_Changed_By_Key = true;
                        }
                        else if ((Keyboard.GetKeyStates(Key.S) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.Down) & KeyStates.Down) > 0)
                        {
                            if (IsSyncPitch_And_Speed)
                            {
                                double Decrease = 0.6;
                                if (Pitch_Speed_S.Value - Decrease < 0)
                                    Pitch_Speed_S.Value = 0;
                                else
                                    Pitch_Speed_S.Value -= Decrease;
                            }
                            else
                            {
                                if (Speed_S.Value - 1 < -80)
                                    Speed_S.Value = -80;
                                else
                                    Speed_S.Value -= 1;
                            }
                            IsVolume_Speed_Changed_By_Key = true;
                        }
                        else if ((Keyboard.GetKeyStates(Key.S) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.R) & KeyStates.Down) > 0)
                        {
                            Speed_S.Value = 0;
                            Pitch_S.Value = 0;
                            Pitch_Speed_S.Value = 50;
                            Pitch_Speed_S_MouseUp(null, null);
                            Configs_Save();
                        }
                        else if ((Keyboard.GetKeyStates(Key.P) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.Up) & KeyStates.Down) > 0)
                        {
                            if (!IsSyncPitch_And_Speed)
                            {
                                double Decrease = 0.3;
                                if (Pitch_S.Value + Decrease > 20)
                                    Pitch_S.Value = 20;
                                else
                                    Pitch_S.Value += Decrease;
                            }
                        }
                        else if ((Keyboard.GetKeyStates(Key.P) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.Down) & KeyStates.Down) > 0)
                        {
                            if (!IsSyncPitch_And_Speed)
                            {
                                double Decrease = 0.3;
                                if (Pitch_S.Value - Decrease < -30)
                                    Pitch_S.Value = -30;
                                else
                                    Pitch_S.Value -= Decrease;
                            }
                        }
                        else if ((Keyboard.GetKeyStates(Key.M) & KeyStates.Down) > 0)
                        {
                            if (!IsMKeyDown)
                                Music_Fix_B_Click(null, null);
                            IsMKeyDown = true;
                        }
                        else
                            IsMKeyDown = false;
                        if ((Keyboard.GetKeyStates(Key.F) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.LeftShift) & KeyStates.Down) <= 0)
                        {
                            if (!IsFKeyDown)
                                Music_Full_Screen_B_Click(null, null);
                            IsFKeyDown = true;
                        }
                        else
                            IsFKeyDown = false;
                        if ((Keyboard.GetKeyStates(Key.Escape) & KeyStates.Down) > 0)
                        {
                            if (!IsESCKeyDown && IsFullScreen)
                                Music_Full_Screen_B_Click(null, null);
                            IsESCKeyDown = true;
                        }
                        else
                            IsESCKeyDown = false;
                        if (IsPlaying)
                        {
                            if (IsVolume_Speed_Changed_By_Key && (Keyboard.GetKeyStates(Key.S) & KeyStates.Down) == 0 && (Keyboard.GetKeyStates(Key.Up) & KeyStates.Down) == 0 && (Keyboard.GetKeyStates(Key.Down) & KeyStates.Down) == 0)
                            {
                                IsVolume_Speed_Changed_By_Key = false;
                                if (IsSyncPitch_And_Speed)
                                    Pitch_Speed_S_MouseUp(null, null);
                                else
                                    Speed_MouseUp(null, null);
                            }
                        }
                    }
                }
                if (IsEnded)
                {
                    if (Loop_C.IsChecked.Value)
                    {
                        Bass.BASS_ChannelStop(Stream);
                        Video_V.Position = TimeSpan.FromSeconds(0);
                        Bass.BASS_ChannelPlay(Stream, true);
                    }
                    else if (Random_C.IsChecked.Value)
                    {
                        if (Music_List.Items.Count == 1)
                        {
                            Video_V.Position = TimeSpan.FromSeconds(0);
                            Bass.BASS_ChannelSetPosition(Stream, 0);
                        }
                        else
                        {
                            Random r = new Random();
                            if (Played_IDs.Count >= Music_List.Items.Count)
                            {
                                Played_IDs.Clear();
                                Music_List_Sort();
                            }
                            else
                            {
                                ListBoxItem LBI = Music_List.SelectedItem as ListBoxItem;
                                LBI.Foreground = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#BF6C6C6C");
                            }
                            while (true)
                            {
                                int r2 = r.Next(0, Music_List.Items.Count);
                                ListBoxItem LBI = Music_List.Items[r2] as ListBoxItem;
                                int Select_Index = Music_Data[Music_Select_List].Select(h => h.File_Name_Path).ToList().IndexOf(LBI.Content.ToString());
                                if (!Played_IDs.Contains(Music_Data[Music_Select_List][Select_Index].ID))
                                {
                                    Music_List.SelectedIndex = r2;
                                    Played_IDs.Add(Music_Data[Music_Select_List][Select_Index].ID);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        Bass.BASS_ChannelStop(Stream);
                        Bass.BASS_StreamFree(Stream);
                        Video_V.Stop();
                        Video_V.Close();
                        Video_V.Source = null;
                        Video_Mode_Change(false);
                        Music_List.SelectedIndex = -1;
                        Playing_Music_Name_Now = "";
                        Device_T.Margin = new Thickness(Device_T.Margin.Left, 640, 0, 0);
                        Device_L.Margin = new Thickness(Device_L.Margin.Left, 700, 0, 0);
                        WAVEForm_Gray_Image.Source = null;
                        WAVEForm_Color_Image.Source = null;
                        Window_Bar_Canvas.Margin = new Thickness(0, 0, 0, 0);
                    }
                }
                IsEnded = false;
                if (Sub_Code.IsForceMusicStop)
                {
                    Pause_Volume_Animation(false, 15f);
                    Sub_Code.IsForceMusicStop = false;
                }
                //次のフレーム時間を計算
                if ((double)System.Environment.TickCount >= nextFrame + (double)period)
                {
                    nextFrame += period;
                    continue;
                }
                nextFrame += period;
            }
        }
        async void Pitch_Change_Await()
        {
            await Task.Delay(500);
        }
        //動画の位置を変更中の間ループさせる
        async void Video_Click_Loop()
        {
            while (IsVideoClicked)
            {
                if ((System.Windows.Forms.Control.MouseButtons & MouseButtons.Left) == MouseButtons.None)
                {
                    IsVideoClicked = false;
                    Video_Mode.IsVideoClicked = false;
                    break;
                }
                await Task.Delay(1000 / 30);
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
        private void Music_Add_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
                return;
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Title = "音楽ファイルを選択してください。",
                Filter = "音楽ファイル(*.aac;*.mp3;*.wav;*.ogg;*.aiff;*.flac;*.m4a;*.mp4)|*.aac;*.mp3;*.wav;*.ogg;*.aiff;*.flac;*.m4a;*.mp4",
                Multiselect = true
            };
            if (ofd.ShowDialog() == DialogResult.OK)
                Music_Add_From_Array(ofd.FileNames);
        }
        void Music_Add_From_Array(string[] Files)
        {
            string Error_File = "";
            int Number = 0;
            string Name = "";
            foreach (string File_Now in Files)
            {
                bool IsExist = false;
                foreach (string File_Now_01 in Music_Data[Music_Select_List].Select(h => h.File_Full_Path))
                {
                    if (File_Now == File_Now_01)
                    {
                        if (Error_File == "")
                            Error_File = File_Now;
                        else
                            Error_File += "\n" + File_Now;
                        IsExist = true;
                        continue;
                    }
                }
                if (IsExist)
                    continue;
                Music_Data[Music_Select_List].Add(new Music_Child_Class(File_Now, Path.GetFileName(File_Now)));
                Number++;
                if (Number == 1)
                    Name = Path.GetFileName(File_Now);
            }
            if (Error_File != "")
                System.Windows.MessageBox.Show("同名の曲が存在するため以下のファイルを追加できませんでした。\n" + Error_File);
            Music_List_Sort();
            Played_IDs.Clear();
            Music_List_Save();
            if (Number == 1)
            {
                try
                {
                    int Index = -1;
                    for (int Number_02 = 0; Number_02 < Music_List.Items.Count; Number_02++)
                    {
                        ListBoxItem LBI = Music_List.Items[Number_02] as ListBoxItem;
                        if (LBI.Content.ToString() == Name)
                        {
                            Index = Number_02;
                            break;
                        }
                    }
                    if (Index != -1)
                        Music_List.ScrollIntoView(Music_List.Items[Index]);
                }
                catch (Exception e1)
                {
                    Sub_Code.Error_Log_Write(e1.Message);
                }
            }
        }
        //リストを並び替える
        void Music_List_Sort()
        {
            IsNotMusicChange = true;
            string List_Now = "";
            if (Music_List.SelectedIndex != -1)
            {
                ListBoxItem LBI = Music_List.SelectedItem as ListBoxItem;
                List_Now = LBI.Content.ToString();
            }
            if (Ex_Sort_C.IsChecked.Value)
            {
                string[] Temp_01 = { ".aac", ".aiff", ".flac", ".mp3", ".m4a", ".mp4", ".ogg", ".wav" };
                List<List<Music_Child_Class>> Temp_02 = new List<List<Music_Child_Class>>();
                Temp_02.Add(new List<Music_Child_Class>());
                Temp_02.Add(new List<Music_Child_Class>());
                Temp_02.Add(new List<Music_Child_Class>());
                Temp_02.Add(new List<Music_Child_Class>());
                Temp_02.Add(new List<Music_Child_Class>());
                Temp_02.Add(new List<Music_Child_Class>());
                Temp_02.Add(new List<Music_Child_Class>());
                Temp_02.Add(new List<Music_Child_Class>());
                foreach (Music_Child_Class Name_Now in Music_Data[Music_Select_List])
                    for (int Number = 0; Number < Temp_01.Length; Number++)
                        if (Temp_01[Number] == Path.GetExtension(Name_Now.File_Full_Path))
                            Temp_02[Number].Add(Name_Now);
                for (int Number = 0; Number < Temp_02.Count; Number++)
                    Temp_02[Number] = Temp_02[Number].OrderBy(h => h.File_Name_Path).ToList();
                Music_List.Items.Clear();
                for (int Number = 0; Number <= Temp_02.Count - 1; Number++)
                {
                    if (Temp_02[Number].Count != 0)
                    {
                        foreach (Music_Child_Class Name in Temp_02[Number])
                        {
                            ListBoxItem LBI = new ListBoxItem();
                            LBI.Content = Name.File_Name_Path;
                            LBI.Foreground = System.Windows.Media.Brushes.Aqua;
                            LBI.ContextMenu = pMenu;
                            LBI.Tag = Name.ID;
                            Music_List.Items.Add(LBI);
                        }
                    }
                }
            }
            else
            {
                IOrderedEnumerable<Music_Child_Class> Order = Music_Data[Music_Select_List].OrderBy(h => h.File_Name_Path);
                Music_List.Items.Clear();
                foreach (Music_Child_Class Name in Order)
                {
                    ListBoxItem LBI = new ListBoxItem();
                    LBI.Content = Name.File_Name_Path;
                    LBI.Foreground = System.Windows.Media.Brushes.Aqua;
                    LBI.ContextMenu = pMenu;
                    LBI.Tag = Name.ID;
                    Music_List.Items.Add(LBI);
                }
            }
            if (List_Now != "")
            {
                for (int Number = 0; Number < Music_List.Items.Count; Number++)
                {
                    ListBoxItem LBI = Music_List.Items[Number] as ListBoxItem;
                    if (LBI.Content.ToString() == List_Now)
                    {
                        Music_List.SelectedIndex = Number;
                        break;
                    }
                }
            }
            IsNotMusicChange = false;
        }
        void List_Remove_Index()
        {
            ListBoxItem LBI = Music_List.SelectedItem as ListBoxItem;
            string NameOnly = LBI.Content.ToString();
            Music_List.SelectedIndex = -1;
            int Delete_Number = Music_Data[Music_Select_List].Select(h => h.File_Name_Path).ToList().IndexOf(NameOnly);
            Music_Data[Music_Select_List].RemoveAt(Delete_Number);
            Bass.BASS_ChannelStop(Stream);
            Video_V.Stop();
            Video_V.Source = null;
            Video_Change_B.Visibility = Visibility.Hidden;
            Bass.BASS_StreamFree(Stream);
            Loop_Time_T.Text = "再生時間:0～0";
            Music_List_Sort();
            Played_IDs.Clear();
            Music_List_Save();
            Thumbnail_Border.Visibility = Visibility.Visible;
            Thumbnail_Main.Visibility = Visibility.Hidden;
            Thumbnail_Sub.Visibility = Visibility.Hidden;
            No_Image_T.Visibility = Visibility.Visible;
            Thumbnail_Main.Opacity = 1;
            Thumbnail_Sub.Opacity = 0;
            Thumbnails.Clear();
            Thumbnail_Index_Now = -1;
            Fade_Count++;
        }
        //曲のリストをファイルに記録
        void Music_List_Save()
        {
            StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Configs/Temp_Other_Music_List.dat");
            for (int Number = 0; Number < Music_Data.Count; Number++)
                foreach (Music_Child_Class Now in Music_Data[Number])
                    stw.WriteLine(Number + "|" + Now.File_Full_Path + "|" + Now.File_Name_Path);
            stw.Close();
            Sub_Code.File_Encrypt(Voice_Set.Special_Path + "/Configs/Temp_Other_Music_List.dat", Voice_Set.Special_Path + "/Configs/Other_Music_List.dat", "SRTTbacon_Music_List_Save", true);
        }
        private void Music_Delete_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy || Music_List.SelectedIndex == -1)
                return;
            MessageBoxResult result = System.Windows.MessageBox.Show("選択中の曲をリストから削除しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                WAVEForm_Gray_Image.Source = null;
                WAVEForm_Color_Image.Source = null;
                List_Remove_Index();
                Video_Mode_Change(false);
            }
        }
        private void Music_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsBusy || IsNotMusicChange)
                return;
            if (Music_List.SelectedIndex != -1)
            {
                ListBoxItem LBI = Music_List.SelectedItem as ListBoxItem;
                int Select_Index = Music_Data[Music_Select_List].Select(h => h.File_Name_Path).ToList().IndexOf(LBI.Content.ToString());
                if (!File.Exists(Music_Data[Music_Select_List][Select_Index].File_Full_Path))
                {
                    System.Windows.MessageBox.Show("ファイルが存在しません。リストから削除されます。");
                    List_Remove_Index();
                    return;
                }
                if (Playing_Music_Name_Now == Music_Data[Music_Select_List][Select_Index].File_Full_Path && Location_S.Maximum > 0)
                    return;
                Video_Change_B.Visibility = Visibility.Visible;
                IsWaveGrayLoaded = false;
                IsWaveColorLoaded = false;
                WAVEForm_Gray_Image.Source = null;
                WAVEForm_Color_Image.Source = null;
                WAVEForm_Color_Image.Width = 0;
                Bass.BASS_ChannelStop(Stream);
                Bass.BASS_ChannelRemoveFX(Stream, Stream_LPF);
                Bass.BASS_ChannelRemoveFX(Stream, Stream_HPF);
                Bass.BASS_ChannelRemoveFX(Stream, Stream_ECHO);
                Bass.BASS_FXReset(Stream_LPF);
                Bass.BASS_FXReset(Stream_HPF);
                Bass.BASS_FXReset(Stream_ECHO);
                Bass.BASS_StreamFree(Stream);
                Location_S.Value = 0;
                Playing_Music_Name_Now = Music_Data[Music_Select_List][Select_Index].File_Full_Path;
                Thumbnail_Main.Visibility = Visibility.Hidden;
                Thumbnail_Sub.Visibility = Visibility.Hidden;
                No_Image_T.Visibility = Visibility.Visible;
                Thumbnail_Main.Source = null;
                Thumbnail_Sub.Source = null;
                Thumbnails.Clear();
                TagLib.File MP3_Tag = TagLib.File.Create(Playing_Music_Name_Now);
                foreach (TagLib.IPicture pic in MP3_Tag.Tag.Pictures)
                    if (pic.MimeType.Contains("image/"))
                        Thumbnails.Add(pic.Data.Data);
                MP3_Tag.Dispose();
                Thumbnail_Index_Now = -1;
                if (Thumbnails.Count > 0)
                {
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.StreamSource = new MemoryStream(Thumbnails[0]);
                    image.EndInit();
                    Thumbnail_Main.Source = image;
                    Thumbnail_Main.Visibility = Visibility.Visible;
                    Thumbnail_Main.Opacity = 1;
                    Thumbnail_Sub.Opacity = 0;
                    Fade_Count++;
                    No_Image_T.Visibility = Visibility.Hidden;
                    Thumbnail_Index_Now = 0;
                }
                Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, 100);
                int StreamHandle = Bass.BASS_StreamCreateFile(Playing_Music_Name_Now, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_LOOP);
                Stream = BassFx.BASS_FX_TempoCreate(StreamHandle, BASSFlag.BASS_FX_FREESOURCE);
                Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, 500);
                IsMusicEnd = new SYNCPROC(EndSync);
                //FMOD_Class.Fmod_System.FModSystem.createSound(File_Full_Path[Music_Select_List][Select_Index], MODE.MPEGSEARCH | MODE.CREATESTREAM, ref FMOD_Sound);
                //FMOD_Class.Fmod_System.FModSystem.playSound(CHANNELINDEX.FREE, FMOD_Sound, true, ref FMOD_Channel);
                Bass.BASS_ChannelGetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, ref Music_Frequency);
                Bass.BASS_ChannelSetSync(Stream, BASSSync.BASS_SYNC_END | BASSSync.BASS_SYNC_MIXTIME, 0, IsMusicEnd, IntPtr.Zero);
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)Volume_S.Value / 100);
                Stream_LPF = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_BFX_BQF, 2);
                Stream_HPF = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_BFX_BQF, 1);
                HPF_Setting.fCenter = 1000f;
                if (Music_Player_Setting_Window.IsLPFEnable)
                {
                    LPF_Setting.fCenter = 500 + 4000f * (1 - (float)Music_Player_Setting_Window.LPF_S.Value / 100.0f);
                    Bass.BASS_FXSetParameters(Stream_LPF, LPF_Setting);
                }
                if (Music_Player_Setting_Window.IsHPFEnable)
                {
                    HPF_Setting.fCenter = 1000 + 4000f * (float)Music_Player_Setting_Window.HPF_S.Value / 100.0f;
                    Bass.BASS_FXSetParameters(Stream_HPF, HPF_Setting);
                }
                ECHO_Setting.fWetMix = 0f;
                if (Music_Player_Setting_Window.IsECHOEnable)
                {
                    Stream_ECHO = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_BFX_ECHO4, 0);
                    ECHO_Setting.fDelay = (float)Music_Player_Setting_Window.ECHO_Delay_S.Value;
                    ECHO_Setting.fDryMix = (float)Music_Player_Setting_Window.ECHO_Power_Original_S.Value / 100f;
                    ECHO_Setting.fWetMix = (float)Music_Player_Setting_Window.ECHO_Power_ECHO_S.Value / 100f;
                    ECHO_Setting.fFeedback = (float)Music_Player_Setting_Window.ECHO_Length_S.Value / 100f;
                    Bass.BASS_FXSetParameters(Stream_ECHO, ECHO_Setting);
                }
                //FMOD_Channel.setVolume((float)Volume_S.Value / 100);
                if (IsSyncPitch_And_Speed)
                {
                    Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, Music_Frequency * (float)(Pitch_Speed_S.Value / 50));
                    //FMOD_Channel.setFrequency(Music_Frequency * (float)Pitch_Speed_S.Value / 50);
                }
                else
                {
                    Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, (float)Pitch_S.Value);
                    Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO, (float)Speed_S.Value);
                    //FMOD_Sound.setMusicSpeed((float)(1 + Speed_S.Value / 100));
                }
                Location_S.Maximum = Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetLength(Stream, BASSMode.BASS_POS_BYTES));
                Start_Time = 0;
                End_Time = Location_S.Maximum;
                Loop_Time_T.Text = "再生時間:" + (int)Start_Time + "～" + (int)End_Time;
                IsPaused = false;
                Bass.BASS_ChannelPlay(Stream, true);
                if (Device_L.SelectedIndex != -1)
                    Bass.BASS_ChannelSetDevice(Stream, Device_L.SelectedIndex + 1);
                else
                    Bass.BASS_ChannelSetDevice(Stream, SetFirstDevice);
                IsFullScreen = true;
                Music_Full_Screen_B_Click(null, null);
                IsFullScreen = false;
                if (Path.GetExtension(Music_Data[Music_Select_List][Select_Index].File_Full_Path) == ".mp4" && Video_Mode_C.IsChecked.Value)
                {
                    Video_Mode_Change(true);
                    Device_T.Margin = new Thickness(Device_T.Margin.Left, 700, 0, 0);
                    Device_L.Margin = new Thickness(Device_L.Margin.Left, 755, 0, 0);
                    Video_V.Close();
                    Video_V.Source = new Uri(Music_Data[Music_Select_List][Select_Index].File_Full_Path);
                    Video_V.Volume = 0;
                    Video_V.Visibility = Visibility.Visible;
                    Video_V.Play();
                    long position = Bass.BASS_ChannelGetPosition(Stream);
                    TimeSpan time = TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(Stream, position) + 0.1);
                    Video_V.Position = time;
                    if (Location_S.Maximum >= 420)
                    {
                        WAVEForm_Gray_Image.Visibility = Visibility.Hidden;
                        WAVEForm_Color_Image.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        WF_Gray = new Un4seen.Bass.Misc.WaveForm(Music_Data[Music_Select_List][Select_Index].File_Full_Path, new Un4seen.Bass.Misc.WAVEFORMPROC(GetWaveFormData_Gray), null);
                        WF_Gray.CallbackFrequency = 0;
                        WF_Gray.ColorBackground = Color.Transparent;
                        WF_Gray.ColorLeft = Color.Gray;
                        WF_Gray.ColorMiddleLeft = Color.DarkGray;
                        WF_Gray.ColorRight = Color.Transparent;
                        WF_Gray.ColorLeft2 = Color.Transparent;
                        WF_Gray.ColorRight2 = Color.Transparent;
                        WF_Gray.ColorLeftEnvelope = Color.Transparent;
                        WF_Gray.ColorRightEnvelope = Color.Transparent;
                        WF_Gray.RenderStart(true, BASSFlag.BASS_DEFAULT);
                        WF_Color = new Un4seen.Bass.Misc.WaveForm(Music_Data[Music_Select_List][Select_Index].File_Full_Path, new Un4seen.Bass.Misc.WAVEFORMPROC(GetWaveFormData_Color), null);
                        WF_Color.CallbackFrequency = 0;
                        WF_Color.ColorBackground = Color.Transparent;
                        WF_Color.ColorLeft = Color.Aqua;
                        WF_Color.ColorMiddleLeft = Color.DarkBlue;
                        WF_Color.ColorRight = Color.Transparent;
                        WF_Color.ColorLeft2 = Color.Transparent;
                        WF_Color.ColorRight2 = Color.Transparent;
                        WF_Color.ColorLeftEnvelope = Color.Transparent;
                        WF_Color.ColorRightEnvelope = Color.Transparent;
                        WF_Color.RenderStart(true, BASSFlag.BASS_DEFAULT);
                        Window_Bar_Canvas.Margin = new Thickness(0, 0, 0, 0);
                    }
                }
                else if (Path.GetExtension(Music_Data[Music_Select_List][Select_Index].File_Full_Path) == ".mp4")
                {
                    Device_T.Margin = new Thickness(Device_T.Margin.Left, 700, 0, 0);
                    Device_L.Margin = new Thickness(Device_L.Margin.Left, 755, 0, 0);
                    Video_Mode_Change(false);
                    Video_V.Stop();
                    Video_V.Close();
                    Video_V.Source = new Uri(Music_Data[Music_Select_List][Select_Index].File_Full_Path);
                    Video_V.Volume = 0;
                    if (Location_S.Maximum >= 420)
                    {
                        WAVEForm_Gray_Image.Visibility = Visibility.Hidden;
                        WAVEForm_Color_Image.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        WF_Gray = new Un4seen.Bass.Misc.WaveForm(Music_Data[Music_Select_List][Select_Index].File_Full_Path, new Un4seen.Bass.Misc.WAVEFORMPROC(GetWaveFormData_Gray), null);
                        WF_Gray.CallbackFrequency = 0;
                        WF_Gray.ColorBackground = Color.Transparent;
                        WF_Gray.ColorLeft = Color.Gray;
                        WF_Gray.ColorMiddleLeft = Color.DarkGray;
                        WF_Gray.ColorRight = Color.Transparent;
                        WF_Gray.ColorLeft2 = Color.Transparent;
                        WF_Gray.ColorRight2 = Color.Transparent;
                        WF_Gray.ColorLeftEnvelope = Color.Transparent;
                        WF_Gray.ColorRightEnvelope = Color.Transparent;
                        WF_Gray.RenderStart(true, BASSFlag.BASS_DEFAULT);
                        WF_Color = new Un4seen.Bass.Misc.WaveForm(Music_Data[Music_Select_List][Select_Index].File_Full_Path, new Un4seen.Bass.Misc.WAVEFORMPROC(GetWaveFormData_Color), null);
                        WF_Color.CallbackFrequency = 0;
                        WF_Color.ColorBackground = Color.Transparent;
                        WF_Color.ColorLeft = Color.Aqua;
                        WF_Color.ColorMiddleLeft = Color.DarkBlue;
                        WF_Color.ColorRight = Color.Transparent;
                        WF_Color.ColorLeft2 = Color.Transparent;
                        WF_Color.ColorRight2 = Color.Transparent;
                        WF_Color.ColorLeftEnvelope = Color.Transparent;
                        WF_Color.ColorRightEnvelope = Color.Transparent;
                        WF_Color.RenderStart(true, BASSFlag.BASS_DEFAULT);
                        Window_Bar_Canvas.Margin = new Thickness(0, 0, 0, 0);
                    }
                }
                else
                {
                    Device_T.Margin = new Thickness(Device_T.Margin.Left, 640, 0, 0);
                    Device_L.Margin = new Thickness(Device_L.Margin.Left, 690, 0, 0);
                    Video_Mode_Change(false);
                    Video_V.Stop();
                    Video_V.Close();
                    Video_Change_B.Visibility = Visibility.Hidden;
                    WF_Gray = new Un4seen.Bass.Misc.WaveForm(Music_Data[Music_Select_List][Select_Index].File_Full_Path, new Un4seen.Bass.Misc.WAVEFORMPROC(GetWaveFormData_Gray), null);
                    WF_Gray.CallbackFrequency = 0;
                    WF_Gray.ColorBackground = Color.Transparent;
                    WF_Gray.ColorLeft = Color.Gray;
                    WF_Gray.ColorMiddleLeft = Color.DarkGray;
                    WF_Gray.ColorRight = Color.Transparent;
                    WF_Gray.ColorLeft2 = Color.Transparent;
                    WF_Gray.ColorRight2 = Color.Transparent;
                    WF_Gray.ColorLeftEnvelope = Color.Transparent;
                    WF_Gray.ColorRightEnvelope = Color.Transparent;
                    WF_Gray.RenderStart(true, BASSFlag.BASS_DEFAULT);
                    WF_Color = new Un4seen.Bass.Misc.WaveForm(Music_Data[Music_Select_List][Select_Index].File_Full_Path, new Un4seen.Bass.Misc.WAVEFORMPROC(GetWaveFormData_Color), null);
                    WF_Color.CallbackFrequency = 0;
                    WF_Color.ColorBackground = Color.Transparent;
                    WF_Color.ColorLeft = Color.Aqua;
                    WF_Color.ColorMiddleLeft = Color.DarkBlue;
                    WF_Color.ColorRight = Color.Transparent;
                    WF_Color.ColorLeft2 = Color.Transparent;
                    WF_Color.ColorRight2 = Color.Transparent;
                    WF_Color.ColorLeftEnvelope = Color.Transparent;
                    WF_Color.ColorRightEnvelope = Color.Transparent;
                    WF_Color.RenderStart(true, BASSFlag.BASS_DEFAULT);
                    Window_Bar_Canvas.Margin = new Thickness(0, 0, 0, 0);
                }
            }
            else
            {
                Location_S.Value = 0;
                Location_S.Maximum = 0;
                Location_T.Text = "00:00";
                Video_Change_B.Visibility = Visibility.Hidden;
            }
        }
        void GetWaveFormData_Gray(int framesDone, int framesTotal, TimeSpan elapsedTime, bool finished)
        {
            WAVEForm_Gray_Image.Dispatcher.BeginInvoke(new Action(() =>
            {
                Wave_Gray_Image_Source = Sub_Code.Bitmap_To_BitmapImage(WF_Gray.CreateBitmap(WAVEForm_Image_Width, WAVEForm_Image_Height * 2, -1, -1, false));
                WF_Gray.RenderStop();
                IsWaveGrayLoaded = true;
            }));
        }
        void GetWaveFormData_Color(int framesDone, int framesTotal, TimeSpan elapsedTime, bool finished)
        {
            WAVEForm_Color_Image.Dispatcher.BeginInvoke(new Action(() =>
            {
                Wave_Color_Image_Source = Sub_Code.Bitmap_To_BitmapImage(WF_Color.CreateBitmap(WAVEForm_Image_Width, WAVEForm_Image_Height * 2, -1, -1, false));
                WF_Color.RenderStop();
                IsWaveColorLoaded = true;
            }));
        }
        private void Volume_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Volume_T.Text = "音量:" + (int)Volume_S.Value;
            if (!IsPaused)
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)Volume_S.Value / 100);
        }
        private void Pitch_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Pitch_T.Text = "音程:" + (Math.Floor(Pitch_S.Value * 10) / 10).ToString();
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, (float)Pitch_S.Value);
        }
        private void Speed_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Speed_T.Text = "速度:" + (int)e.NewValue;
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO, (float)e.NewValue);
        }
        private void Pitch_S_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Pitch_S.Value = 0;
        }
        private async void Speed_S_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Speed_S.Value = 0;
            if (Music_List_T.Visibility != Visibility.Visible)
            {
                Video_V.SpeedRatio = 1;
                await Task.Delay(400);
                Video_V.Position = TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetPosition(Stream)) + 0.1);
            }
        }
        void Location_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsBusy)
                return;
            IsLocationChanging = true;
            if (Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_PLAYING)
            {
                IsPlayingMouseDown = true;
                Pause_Volume_Animation(false, 10);
            }
            if (Video_V.Visibility == Visibility.Visible)
                Video_V.Pause();
        }
        async void Location_MouseUp(object sender, MouseButtonEventArgs e)
        {
            IsLocationChanging = false;
            if (WAVEForm_Color_Image.Visibility == Visibility.Visible)
                WAVEForm_Color_Image.Width = (Location_S.Value / Location_S.Maximum) * WAVEForm_Image_Width;
            Bass.BASS_ChannelSetPosition(Stream, Location_S.Value);
            if (IsPlayingMouseDown)
            {
                IsPaused = false;
                Play_Volume_Animation(10);
                IsPlayingMouseDown = false;
            }
            if (Video_V.Visibility == Visibility.Visible)
            {
                Video_V.Pause();
                Video_V.Position = TimeSpan.FromSeconds(Location_S.Value);
                Bass.BASS_ChannelPause(Stream);
                Bass.BASS_ChannelSetPosition(Stream, Video_V.Position.TotalSeconds - 0.1);
                await Task.Delay(50);
                if (!IsPaused)
                {
                    Video_V.Play();
                    Bass.BASS_ChannelPlay(Stream, false);
                }
            }
        }
        private void Location_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLocationChanging)
                Music_Pos_Change(Location_S.Value, false);
        }
        async void Speed_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Music_List_T.Visibility != Visibility.Visible)
            {
                Video_V.SpeedRatio = 1 + Speed_S.Value / 100;
                await Task.Delay(400);
                Bass.BASS_ChannelPause(Stream);
                double Delay_Time = 0.1 * Pitch_Speed_S.Value / 50;
                Video_V.Position = TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetPosition(Stream)) + Delay_Time);
                Bass.BASS_ChannelPlay(Stream, false);
            }
        }
        void Music_Pos_Change(double Pos, bool IsBassPosChange)
        {
            if (IsBusy)
                return;
            if (IsBassPosChange)
                Bass.BASS_ChannelSetPosition(Stream, Pos);
            if (WAVEForm_Color_Image.Visibility == Visibility.Visible)
                WAVEForm_Color_Image.Width = (Pos / Location_S.Maximum) * WAVEForm_Image_Width;
            TimeSpan Time = TimeSpan.FromSeconds(Pos);
            string Minutes = Time.Minutes.ToString();
            string Seconds = Time.Seconds.ToString();
            if (Time.Minutes < 10)
                Minutes = "0" + Time.Minutes;
            if (Time.Seconds < 10)
                Seconds = "0" + Time.Seconds;
            Location_T.Text = Minutes + ":" + Seconds;
        }
        private void Music_Play_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
                return;
            Play_Volume_Animation();
        }
        private void Music_Pause_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
                return;
            Pause_Volume_Animation(false);
        }
        async void Play_Volume_Animation(float Feed_Time = 30f)
        {
            IsPaused = false;
            if (Video_V.Visibility == Visibility.Visible)
                Video_V.Play();
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
        public async void Pause_Volume_Animation(bool IsStop, float Feed_Time = 30f)
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
                    Video_V.Stop();
                    Video_V.Source = null;
                    WAVEForm_Gray_Image.Source = null;
                    WAVEForm_Color_Image.Source = null;
                    Location_S.Value = 0;
                    Location_S.Maximum = 0;
                    Location_T.Text = "00:00";
                    Loop_Time_T.Text = "再生時間:0～0";
                    Music_List.SelectedIndex = -1;
                    Thumbnail_Border.Visibility = Visibility.Visible;
                    Thumbnail_Main.Visibility = Visibility.Hidden;
                    Thumbnail_Sub.Visibility = Visibility.Hidden;
                    No_Image_T.Visibility = Visibility.Visible;
                    Thumbnail_Main.Opacity = 1;
                    Thumbnail_Sub.Opacity = 0;
                    Thumbnails.Clear();
                    Thumbnail_Index_Now = -1;
                    Fade_Count++;
                }
                else if (IsPaused)
                {
                    Bass.BASS_ChannelPause(Stream);
                    Video_V.Pause();
                }
            }
        }
        private void Music_Minus_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
                return;
            int Move_Time = 5;
            if (IsLControlKeyDown)
                Move_Time = 10;
            if (Video_V.Visibility == Visibility.Visible)
            {
                if (Video_V.Position.TotalSeconds - Move_Time < 0)
                {
                    Video_V.Position = TimeSpan.FromSeconds(0);
                    Bass.BASS_ChannelSetPosition(Stream, 0);
                }
                else
                {
                    Video_V.Position = TimeSpan.FromSeconds(Video_V.Position.TotalSeconds - Move_Time);
                    Bass.BASS_ChannelSetPosition(Stream, Video_V.Position.TotalSeconds);
                }
                Location_S.Value = Video_V.Position.TotalSeconds;
                Music_Pos_Change(Video_V.Position.TotalSeconds, false);
            }
            else
            {
                if (Location_S.Value <= Move_Time)
                    Location_S.Value = 0;
                else
                    Location_S.Value -= Move_Time;
                Music_Pos_Change(Location_S.Value, true);
            }
        }
        private void Music_Plus_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
                return;
            int Move_Time = 5;
            if (IsLControlKeyDown)
                Move_Time = 10;
            if (Video_V.Visibility == Visibility.Visible)
            {
                Video_V.Position = TimeSpan.FromSeconds(Video_V.Position.TotalSeconds + Move_Time);
                Bass.BASS_ChannelSetPosition(Stream, Video_V.Position.TotalSeconds);
                Location_S.Value = Video_V.Position.TotalSeconds;
                Music_Pos_Change(Video_V.Position.TotalSeconds, false);
            }
            else
            {
                if (Location_S.Value + Move_Time >= Location_S.Maximum)
                    Location_S.Value = Location_S.Maximum;
                else
                    Location_S.Value += Move_Time;
                Music_Pos_Change(Location_S.Value, true);
            }
        }
        private void Video_Change_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
                return;
            if (Music_List_T.Visibility == Visibility.Visible)
            {
                Video_Mode_Change(true);
                Video_V.Visibility = Visibility.Visible;
                Video_V.Play();
                if (Sub_Code.IsWindowBarShow)
                    Window_Bar_Canvas.Margin = new Thickness(0, 25, 0, 0);
                if (IsPaused)
                    Video_V.Pause();
            }
            else
            {
                Video_Mode_Change(false);
                Video_V.Pause();
                Video_V.Visibility = Visibility.Hidden;
                WAVEForm_Color_Image.Width = (Location_S.Value / Location_S.Maximum) * WAVEForm_Image_Width;
                Window_Bar_Canvas.Margin = new Thickness(0, 0, 0, 0);
            }
        }
        void Video_Mode_Change(bool IsVideoMode)
        {
            this.IsVideoMode = IsVideoMode;
            if (IsVideoMode)
            {
                Video_Mode_Select_Name = Music_List.SelectedItem.ToString();
                Music_List_T.Visibility = Visibility.Hidden;
                Music_List.Visibility = Visibility.Hidden;
                Music_Add_B.Visibility = Visibility.Hidden;
                Music_Delete_B.Visibility = Visibility.Hidden;
                Video_Mode_T.Visibility = Visibility.Hidden;
                Video_Mode_C.Visibility = Visibility.Hidden;
                Device_T.Visibility = Visibility.Hidden;
                Device_L.Visibility = Visibility.Hidden;
                Youtube_Link_B.Visibility = Visibility.Hidden;
                Setting_B.Visibility = Visibility.Hidden;
                Music_Vocal_Inst_Cut_B.Visibility = Visibility.Hidden;
                Loop_Time_T.Visibility = Visibility.Hidden;
                Ex_Sort_C.Visibility = Visibility.Hidden;
                Ex_Sort_T.Visibility = Visibility.Hidden;
                WAVEForm_Gray_Image.Visibility = Visibility.Hidden;
                WAVEForm_Color_Image.Visibility = Visibility.Hidden;
                List_Number_T.Visibility = Visibility.Hidden;
                Page_Next_B.Visibility = Visibility.Hidden;
                Page_Back_B.Visibility = Visibility.Hidden;
                Zoom_T.Visibility = Visibility.Visible;
                Zoom_S.Visibility = Visibility.Visible;
                Music_Fix_B.Visibility = Visibility.Visible;
                Music_Full_Screen_B.Visibility = Visibility.Visible;
                Thumbnail_Border.Visibility = Visibility.Hidden;
                Thumbnail_Sub.Visibility = Visibility.Hidden;
                No_Image_T.Visibility = Visibility.Hidden;
                Video_Change_B.Content = "BGMとして再生";
                Volume_T.Margin = new Thickness(-475, 100, 0, 0);
                Volume_S.Margin = new Thickness(-475, 200, 0, 0);
                Pitch_T.Margin = new Thickness(-475, 300, 0, 0);
                Pitch_S.Margin = new Thickness(-475, 400, 0, 0);
                Speed_T.Margin = new Thickness(-475, 500, 0, 0);
                Speed_S.Margin = new Thickness(-475, 600, 0, 0);
                Location_T.Margin = new Thickness(-475, 700, 0, 0);
                Location_S.Margin = new Thickness(-475, 800, 0, 0);
                Music_Play_B.Margin = new Thickness(-2150, 812, 0, 0);
                Music_Pause_B.Margin = new Thickness(-1640, 812, 0, 0);
                Music_Minus_B.Margin = new Thickness(-3220, 812, 0, 0);
                Music_Plus_B.Margin = new Thickness(-2710, 812, 0, 0);
                Loop_T.Margin = new Thickness(-2750, 925, 0, 0);
                Loop_C.Margin = new Thickness(-2975, 945, 0, 0);
                Random_T.Margin = new Thickness(-2725, 1000, 0, 0);
                Random_C.Margin = new Thickness(-2975, 1020, 0, 0);
                Background_T.Margin = new Thickness(-2000, 1005, 0, 0);
                Background_C.Margin = new Thickness(-2350, 1020, 0, 0);
                Mode_T.Margin = new Thickness(-2025, 925, 0, 0);
                Mode_C.Margin = new Thickness(-2350, 945, 0, 0);
                Video_Change_B.Margin = new Thickness(-1350, 1005, 0, 0);
            }
            else
            {
                Music_List_T.Visibility = Visibility.Visible;
                Music_List.Visibility = Visibility.Visible;
                Music_Add_B.Visibility = Visibility.Visible;
                Music_Delete_B.Visibility = Visibility.Visible;
                Video_Mode_T.Visibility = Visibility.Visible;
                Video_Mode_C.Visibility = Visibility.Visible;
                Device_T.Visibility = Visibility.Visible;
                Device_L.Visibility = Visibility.Visible;
                Youtube_Link_B.Visibility = Visibility.Visible;
                Setting_B.Visibility = Visibility.Visible;
                Music_Vocal_Inst_Cut_B.Visibility = Visibility.Visible;
                Loop_Time_T.Visibility = Visibility.Visible;
                Ex_Sort_C.Visibility = Visibility.Visible;
                Ex_Sort_T.Visibility = Visibility.Visible;
                List_Number_T.Visibility = Visibility.Visible;
                Page_Next_B.Visibility = Visibility.Visible;
                Page_Back_B.Visibility = Visibility.Visible;
                WAVEForm_Gray_Image.Visibility = Visibility.Visible;
                WAVEForm_Color_Image.Visibility = Visibility.Visible;
                Thumbnail_Border.Visibility = Visibility.Visible;
                Thumbnail_Main.Visibility = Visibility.Visible;
                Thumbnail_Sub.Visibility = Visibility.Hidden;
                if (Thumbnails.Count == 0)
                {
                    Thumbnail_Main.Visibility = Visibility.Hidden;
                    No_Image_T.Visibility = Visibility.Visible;
                }
                Music_Fix_B.Visibility = Visibility.Hidden;
                Music_Full_Screen_B.Visibility = Visibility.Hidden;
                Zoom_T.Visibility = Visibility.Hidden;
                Zoom_S.Visibility = Visibility.Hidden;
                Video_Change_B.Content = "動画として再生";
                Volume_T.Margin = new Thickness(-3200, 100, 0, 0);
                Volume_S.Margin = new Thickness(-3200, 200, 0, 0);
                Pitch_T.Margin = new Thickness(-2100, 100, 0, 0);
                Pitch_S.Margin = new Thickness(-2100, 200, 0, 0);
                Speed_T.Margin = new Thickness(-2100, 300, 0, 0);
                Speed_S.Margin = new Thickness(-2100, 400, 0, 0);
                Location_T.Margin = new Thickness(-3200, 300, 0, 0);
                Location_S.Margin = new Thickness(-3200, 400, 0, 0);
                Music_Play_B.Margin = new Thickness(-2345, 525, 0, 0);
                Music_Pause_B.Margin = new Thickness(-1830, 525, 0, 0);
                Music_Minus_B.Margin = new Thickness(-3475, 525, 0, 0);
                Music_Plus_B.Margin = new Thickness(-2960, 525, 0, 0);
                Loop_T.Margin = new Thickness(-2875, 650, 0, 0);
                Loop_C.Margin = new Thickness(-3100, 670, 0, 0);
                Random_T.Margin = new Thickness(-2850, 725, 0, 0);
                Random_C.Margin = new Thickness(-3100, 745, 0, 0);
                Background_T.Margin = new Thickness(-2755, 800, 0, 0);
                Background_C.Margin = new Thickness(-3100, 820, 0, 0);
                Mode_T.Margin = new Thickness(-2775, 875, 0, 0);
                Mode_C.Margin = new Thickness(-3100, 895, 0, 0);
                Video_Change_B.Margin = new Thickness(-2050, 615, 0, 0);
                if (Video_Mode_Select_Name != "")
                {
                    try
                    {
                        int Index = -1;
                        for (int Number = 0; Number < Music_List.Items.Count; Number++)
                        {
                            ListBoxItem LBI = Music_List.Items[Number] as ListBoxItem;
                            if (LBI.Content.ToString() == Video_Mode_Select_Name)
                            {
                                Index = Number;
                                break;
                            }
                        }
                        if (Index != -1)
                            Music_List.ScrollIntoView(Music_List.Items[Index]);
                    }
                    catch
                    {
                    }
                    Video_Mode_Select_Name = "";
                }
            }
            Music_Play_Mode_Change(IsSyncPitch_And_Speed);
        }
        private void Music_Fix_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
                return;
            if (Music_List_T.Visibility != Visibility.Visible)
                Bass.BASS_ChannelSetPosition(Stream, Video_V.Position.TotalSeconds);
        }
        private async void Exit_B_Click(object sender, RoutedEventArgs e)
        {
            if (!IsBusy)
            {
                IsBusy = true;
                bool IsPaused_Now = IsPaused;
                if (!Background_C.IsChecked.Value)
                    Pause_Volume_Animation(false, 25);
                while (Opacity > 0)
                {
                    Opacity -= Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
                if (!Background_C.IsChecked.Value)
                {
                    Bass.BASS_ChannelStop(Stream);
                    Bass.BASS_StreamFree(Stream);
                    Video_V.Stop();
                    Video_V.Source = null;
                    Location_S.Value = 0;
                    Location_S.Maximum = 0;
                    Location_T.Text = "00:00";
                    Music_List.SelectedIndex = -1;
                    Video_Mode_Change(false);
                    Video_V.Visibility = Visibility.Hidden;
                    Video_Change_B.Visibility = Visibility.Hidden;
                    X_Move = 0;
                    Y_Move = 0;
                    Zoom_S.Value = 1;
                    if (Music_List.Items.Count > 0)
                        Music_List.ScrollIntoView(Music_List.Items[0]);
                    WAVEForm_Gray_Image.Source = null;
                    WAVEForm_Color_Image.Source = null;
                    Thumbnail_Border.Visibility = Visibility.Visible;
                    Thumbnail_Main.Visibility = Visibility.Hidden;
                    Thumbnail_Sub.Visibility = Visibility.Hidden;
                    No_Image_T.Visibility = Visibility.Visible;
                    Thumbnail_Main.Opacity = 1;
                    Thumbnail_Sub.Opacity = 0;
                    Thumbnails.Clear();
                    Thumbnail_Index_Now = -1;
                    Fade_Count++;
                }
                else if (Video_V.Source != null)
                {
                    Video_V.Pause();
                    Video_V.Visibility = Visibility.Hidden;
                }
                IsVideoClicked = false;
                Video_Mode.IsVideoClicked = false;
                IsBusy = false;
                Visibility = Visibility.Hidden;
                IsPaused = IsPaused_Now;
            }
        }
        private void Loop_C_Click(object sender, RoutedEventArgs e)
        {
            Random_C.IsChecked = false;
            Configs_Save();
        }
        private void Random_C_Click(object sender, RoutedEventArgs e)
        {
            Loop_C.IsChecked = false;
            Configs_Save();
        }
        private void Video_Mode_C_Click(object sender, RoutedEventArgs e)
        {
            Configs_Save();
        }
        private void Background_C_Click(object sender, RoutedEventArgs e)
        {
            Configs_Save();
        }
        private void Ex_Sort_C_Click(object sender, RoutedEventArgs e)
        {
            Music_List_Sort();
            IsNotMusicChange = true;
            int Index = Music_List.SelectedIndex;
            for (int Number = 0; Number < Music_List.Items.Count; Number++)
            {
                ListBoxItem LBI = Music_List.Items[Number] as ListBoxItem;
                int Select_Index = Music_Data[Music_Select_List].Select(h => h.File_Name_Path).ToList().IndexOf(LBI.Content.ToString());
                if (Played_IDs.Contains(Music_Data[Music_Select_List][Select_Index].ID) && Music_Data[Music_Select_List][Select_Index].File_Full_Path != Playing_Music_Name_Now)
                {
                    LBI.Content = Music_Data[Music_Select_List][Select_Index].File_Name_Path;
                    LBI.Foreground = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#BF6C6C6C");
                }
            }
            Music_List.SelectedIndex = Index;
            IsNotMusicChange = false;
            Configs_Save();
        }
        private void Zoom_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Video_V.Width = 1440 * Zoom_S.Value;
            Video_V.Height = 810 * Zoom_S.Value;
            Zoom_T.Text = "拡大率:" + (Math.Floor(Zoom_S.Value * 100) / 100).ToString();
            try
            {
                Video_V.Margin = new Thickness(-1200 - Video_V.Width / 2 + (X_Move * Zoom_S.Value), (-(Video_V.Height - 810) / 3) + (Y_Move * Zoom_S.Value), 0, 0);
            }
            catch (Exception e1)
            {
                Sub_Code.Error_Log_Write(e1.Message);
            }
        }
        private void Video_V_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsBusy)
                return;
            Mouse_Point.X = System.Windows.Forms.Cursor.Position.X;
            Mouse_Point.Y = System.Windows.Forms.Cursor.Position.Y;
            Video_Point.X = Video_V.Margin.Left;
            Video_Point.Y = Video_V.Margin.Top;
            IsVideoClicked = true;
            Video_Mode.IsVideoClicked = true;
            Video_Click_Loop();
        }
        private void DockPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsVideoClicked)
            {
                X_Move += (System.Windows.Forms.Cursor.Position.X - Mouse_Point.X) * (1 / Zoom_S.Value);
                Y_Move += (System.Windows.Forms.Cursor.Position.Y - Mouse_Point.Y) * (1 / Zoom_S.Value);
                IsVideoClicked = false;
                Video_Mode.IsVideoClicked = false;
            }
        }
        private void Video_V_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsVideoClicked)
            {
                double X_Plus = System.Windows.Forms.Cursor.Position.X - Mouse_Point.X;
                double Y_Plus = System.Windows.Forms.Cursor.Position.Y - Mouse_Point.Y;
                Video_V.Margin = new Thickness(Video_Point.X + X_Plus, Video_Point.Y + Y_Plus, 0, 0);
            }
        }
        private void DockPanel_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (IsVideoEnter)
            {
                if (e.Delta > 0)
                {
                    if (Zoom_S.Value + 0.1 <= 4)
                        Zoom_S.Value += 0.1;
                    else
                        Zoom_S.Value = 4;
                }
                else
                {
                    if (Zoom_S.Value - 0.1 >= 1)
                        Zoom_S.Value -= 0.1;
                    else
                        Zoom_S.Value = 1;
                }
            }
        }
        private void Video_V_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            IsVideoEnter = true;
        }
        private void Video_V_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            IsVideoEnter = false;
        }
        private void Device_L_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Device_L.SelectedIndex != -1)
            {
                Bass.BASS_ChannelSetDevice(Stream, Device_L.SelectedIndex + 1);
                //FMOD_Class.Fmod_System.FModSystem.setDriver(Device_L.SelectedIndex + 1);
                Video_Mode.Sound_Device = Device_L.SelectedIndex + 1;
                Bass.BASS_ChannelPause(Stream);
                Play_Volume_Animation(20f);
            }
        }
        private void Message_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
                return;
            string Message_01 = "・音楽ファイル、リストはサーバーに送信されません。\n";
            string Message_02 = "・一時フォルダの\"Other_Music_List.dat\"を削除するとリストが初期化されます。(ソフトの再起動が必要)\n";
            string Message_03 = "・音楽ファイルが見つからない場合はリストから削除されます。\n";
            string Message_04 = "・音程や速度、動画の表示位置などはその場所を右クリックすると初期値に戻ります。\n";
            string Message_05 = "・波形は読み込みに時間がかかります。また、動画ファイルは重くなるため対応させていません。";
            System.Windows.MessageBox.Show(Message_01 + Message_02 + Message_03 + Message_04 + Message_05);
        }
        private async void Youtube_Link_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
                return;
            Youtube_Link_Window.Window_Show();
            while (Youtube_Link_Window.Visibility == Visibility.Visible)
                await Task.Delay(100);
            if (Sub_Code.AutoListAdd.Count > 0)
            {
                foreach (string File_Now in Sub_Code.AutoListAdd)
                    Music_Data[Music_Select_List].Add(new Music_Child_Class(File_Now, Path.GetFileName(File_Now)));
                Music_List_Sort();
                Played_IDs.Clear();
                Music_List_Save();
                if (Sub_Code.AutoListAdd.Count == 1)
                {
                    try
                    {
                        int Index = -1;
                        string List_Add_Name = Path.GetFileName(Sub_Code.AutoListAdd[0]);
                        for (int Number = 0; Number < Music_List.Items.Count; Number++)
                        {
                            ListBoxItem LBI = Music_List.Items[Number] as ListBoxItem;
                            if (LBI.Content.ToString() == List_Add_Name)
                            {
                                Index = Number;
                                break;
                            }
                        }
                        if (Index != -1)
                            Music_List.ScrollIntoView(Music_List.Items[Index]);
                    }
                    catch (Exception e1)
                    {
                        Sub_Code.Error_Log_Write(e1.Message);
                    }
                }
            }
        }
        void Volume_MouseUp(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Configs_Save();
        }
        void Configs_Save()
        {
            if (!IsSaveOK)
                return;
            try
            {
                int Number_01 = 0;
                if (Music_Data[Music_Select_List].Count == 0)
                {
                    for (int Number_02 = 0; Number_02 < Music_Data.Count; Number_02++)
                        if (Music_Data[Number_02].Count > 0)
                            Number_01 = Number_02;
                }
                else
                    Number_01 = Music_Select_List;
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Configs/Music_Player.tmp");
                stw.WriteLine(Video_Mode_C.IsChecked.Value);
                stw.WriteLine(Loop_C.IsChecked.Value);
                stw.WriteLine(Random_C.IsChecked.Value);
                stw.WriteLine(Background_C.IsChecked.Value);
                stw.WriteLine(Ex_Sort_C.IsChecked.Value);
                stw.WriteLine(Volume_S.Value);
                stw.WriteLine(true);
                stw.WriteLine(Mode_C.IsChecked.Value);
                stw.Write(Number_01);
                stw.Close();
                Sub_Code.File_Encrypt(Voice_Set.Special_Path + "/Configs/Music_Player.tmp", Voice_Set.Special_Path + "/Configs/Music_Player.conf", "Music_Player_Configs_Save", true);
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        private void Video_V_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            X_Move = 0;
            Y_Move = 0;
            if (IsFullScreen)
            {
                Zoom_S.Value = 1.333333333333;
                Video_V.Margin = new Thickness(-1920, 0, 0, 0);
            }
            else
                Video_V.Margin = new Thickness(-1200 - Video_V.Width / 2, -(Video_V.Height - 810) / 3, 0, 0);
        }
        private void Zoom_S_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Zoom_S.Value = 1;
        }
        private void WAVEForm_Gray_Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsBusy)
                return;
            if (Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_PLAYING)
            {
                Pause_Volume_Animation(false, 10);
                IsPlayingMouseDown = true;
            }
            IsLocationChanging = true;
            Video_Mode.IsVideoClicked = true;
            WaveForm_Click_Position_Change();
        }
        //波形の部分をクリックすると、その場所まで時間を進める
        async void WaveForm_Click_Position_Change()
        {
            double Percent_End = 0;
            //クリックが離されるまでループ
            while ((System.Windows.Forms.Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left || (System.Windows.Forms.Control.MouseButtons & MouseButtons.Right) == MouseButtons.Right)
            {
                //現在のマウス位置が曲のどのあたりかを取得&適応
                double Width_Display_From_1920 = Video_Mode.Width / 1920;
                double Location_Mouse_X_Display = System.Math.Abs((int)WAVEForm_Gray_Image.PointToScreen(new System.Windows.Point()).X - System.Windows.Forms.Cursor.Position.X);
                double Percent = Location_Mouse_X_Display / (WAVEForm_Gray_Image.Width * Width_Display_From_1920);
                //曲の長さを超えていたら最大にする
                if (Percent >= 1)
                {
                    WAVEForm_Color_Image.Width = WAVEForm_Gray_Image.Width;
                    Percent_End = 1;
                }
                //曲の長さより短かったら最初から
                else if (Percent <= 0)
                    WAVEForm_Color_Image.Width = 0;
                //それ以外はマウスの位置に合わせる
                else
                {
                    WAVEForm_Color_Image.Width = WAVEForm_Gray_Image.Width * Percent;
                    Percent_End = Percent;
                }
                //適応
                TimeSpan Time = TimeSpan.FromSeconds(Location_S.Maximum * Percent_End);
                string Minutes = Time.Minutes.ToString();
                string Seconds = Time.Seconds.ToString();
                if (Time.Minutes < 10)
                    Minutes = "0" + Time.Minutes;
                if (Time.Seconds < 10)
                    Seconds = "0" + Time.Seconds;
                Location_T.Text = Minutes + ":" + Seconds;
                //60fps
                await Task.Delay(1000 / 60);
            }
            //再生
            IsLocationChanging = false;
            Video_Mode.IsVideoClicked = false;
            Location_S.Value = Location_S.Maximum * Percent_End;
            Bass.BASS_ChannelSetPosition(Stream, Location_S.Maximum * Percent_End);
            if (IsPlayingMouseDown)
            {
                Bass.BASS_ChannelPlay(Stream, false);
                Play_Volume_Animation(10);
            }
            IsPlayingMouseDown = false;
        }
        //動画の位置を初期化
        private void Border_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            X_Move = 0;
            Y_Move = 0;
            Video_V.Margin = new Thickness(-1200 - Video_V.Width / 2, -(Video_V.Height - 810) / 3, 0, 0);
        }
        private void Mode_C_Click(object sender, RoutedEventArgs e)
        {
            Music_Play_Mode_Change(Mode_C.IsChecked.Value);
            Configs_Save();
        }
        //システムの変更(true=音程と速度を同期させる, false=音程と速度を別々に設定)
        async void Music_Play_Mode_Change(bool IsPitch_Speed_Set)
        {
            if (IsPitch_Speed_Set)
            {
                IsSyncPitch_And_Speed = true;
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, Music_Frequency * (float)(Pitch_Speed_S.Value / 50));
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, 0f);
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO, 0f);
                Pitch_Speed_T.Visibility = Visibility.Visible;
                Pitch_Speed_S.Visibility = Visibility.Visible;
                Pitch_T.Visibility = Visibility.Hidden;
                Pitch_S.Visibility = Visibility.Hidden;
                Speed_T.Visibility = Visibility.Hidden;
                Speed_S.Visibility = Visibility.Hidden;
                if (Music_List_T.Visibility == Visibility.Visible)
                {
                    Pitch_Speed_T.Margin = new Thickness(-2100, 200, 0, 0);
                    Pitch_Speed_S.Margin = new Thickness(-2100, 300, 0, 0);
                }
                else
                {
                    Pitch_Speed_T.Margin = new Thickness(-475, 300, 0, 0);
                    Pitch_Speed_S.Margin = new Thickness(-475, 400, 0, 0);
                    Location_T.Margin = new Thickness(-475, 500, 0, 0);
                    Location_S.Margin = new Thickness(-475, 600, 0, 0);
                    Zoom_T.Margin = new Thickness(-475, 700, 0, 0);
                    Zoom_S.Margin = new Thickness(-475, 800, 0, 0);
                }
                if (Music_List_T.Visibility != Visibility.Visible)
                {
                    Video_V.SpeedRatio = Pitch_Speed_S.Value / 50;
                    await Task.Delay(100);
                }
            }
            else
            {
                IsSyncPitch_And_Speed = false;
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, Music_Frequency);
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, (float)Pitch_S.Value);
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO, (float)Speed_S.Value);
                Pitch_Speed_T.Visibility = Visibility.Hidden;
                Pitch_Speed_S.Visibility = Visibility.Hidden;
                if (Music_List_T.Visibility != Visibility.Visible)
                {
                    Location_T.Margin = new Thickness(-475, 700, 0, 0);
                    Location_S.Margin = new Thickness(-475, 800, 0, 0);
                    Zoom_T.Margin = new Thickness(-475, 900, 0, 0);
                    Zoom_S.Margin = new Thickness(-475, 1000, 0, 0);
                }
                Pitch_T.Visibility = Visibility.Visible;
                Pitch_S.Visibility = Visibility.Visible;
                Speed_T.Visibility = Visibility.Visible;
                Speed_S.Visibility = Visibility.Visible;
                if (Music_List_T.Visibility != Visibility.Visible)
                {
                    Video_V.SpeedRatio = 1 + Speed_S.Value / 100;
                    await Task.Delay(100);
                }
            }
            if (Music_List_T.Visibility != Visibility.Visible)
            {
                Video_V.Pause();
                Video_V.Position = TimeSpan.FromSeconds(Location_S.Value);
                Bass.BASS_ChannelPause(Stream);
                Bass.BASS_ChannelSetPosition(Stream, Video_V.Position.TotalSeconds - 0.1);
                await Task.Delay(50);
                if (!IsPaused)
                {
                    Bass.BASS_ChannelPlay(Stream, false);
                    Video_V.Play();
                }
            }
        }
        private void Pitch_Speed_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, Music_Frequency * (float)(Pitch_Speed_S.Value / 50));
            Bass.BASS_ChannelUpdate(Stream, 50);
            Pitch_Speed_T.Text = "音程と速度:" + (int)Pitch_Speed_S.Value;
        }
        private async void Pitch_Speed_S_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Pitch_Speed_S.Value = 50;
            if (Music_List_T.Visibility != Visibility.Visible)
            {
                Video_V.SpeedRatio = 1;
                await Task.Delay(400);
                Video_V.Position = TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetPosition(Stream)) + 0.1);
            }
        }
        private async void Pitch_Speed_S_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Music_List_T.Visibility != Visibility.Visible)
            {
                Video_V.SpeedRatio = Pitch_Speed_S.Value / 50;
                await Task.Delay(400);
                Bass.BASS_ChannelPause(Stream);
                double Delay_Time = 0.1 * Pitch_Speed_S.Value / 50;
                Video_V.Position = TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetPosition(Stream)) + Delay_Time);
                Bass.BASS_ChannelPlay(Stream, false);
            }
        }
        //曲のリストを変更
        void Music_List_Change(int Index)
        {
            if (Music_Select_List == Index)
                return;
            Played_IDs.Clear();
            Music_List_Sort();
            Music_Select_List = Index;
            List_Number_T.Text = "リスト番号:" + (Music_Select_List + 1);
            Pause_Volume_Animation(true, 10f);
            Video_Change_B.Visibility = Visibility.Hidden;
            Device_T.Margin = new Thickness(Device_T.Margin.Left, 640, 0, 0);
            Device_L.Margin = new Thickness(Device_L.Margin.Left, 690, 0, 0);
            Video_Mode_Change(false);
            Music_List.Items.Clear();
            Music_List_Sort();
            Configs_Save();
        }
        //キーイベント
        public void RootWindow_KeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (Opacity < 1)
                return;
            //追加されている曲をクリア
            if ((System.Windows.Forms.Control.ModifierKeys & Keys.Shift) == Keys.Shift && e.Key == Key.D)
            {
                if (Music_List.Items.Count == 0)
                    return;
                MessageBoxResult result = System.Windows.MessageBox.Show("現在追加されている曲をクリアしますか？\nこの操作は取り消しできません。", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        Pause_Volume_Animation(true, 10);
                        for (int Number = 0; Number < 9; Number++)
                            Music_Data[Number].Clear();
                        Music_List.Items.Clear();
                        WAVEForm_Color_Image.Source = null;
                        WAVEForm_Gray_Image.Source = null;
                        WAVEForm_Color_Image.Width = 0;
                        WAVEForm_Color_Image.Visibility = Visibility.Hidden;
                        WAVEForm_Gray_Image.Visibility = Visibility.Hidden;
                        File.Delete(Voice_Set.Special_Path + "/Configs/Other_Music_List.dat");
                    }
                    catch (Exception e1)
                    {
                        Sub_Code.Error_Log_Write(e1.Message);
                    }
                }
            }
            //Shift+1～9でリストを変更できるように
            if ((System.Windows.Forms.Control.ModifierKeys & Keys.Shift) == Keys.Shift && e.Key == Key.D1)
                Music_List_Change(0);
            else if ((System.Windows.Forms.Control.ModifierKeys & Keys.Shift) == Keys.Shift && e.Key == Key.D2)
                Music_List_Change(1);
            else if ((System.Windows.Forms.Control.ModifierKeys & Keys.Shift) == Keys.Shift && e.Key == Key.D3)
                Music_List_Change(2);
            else if ((System.Windows.Forms.Control.ModifierKeys & Keys.Shift) == Keys.Shift && e.Key == Key.D4)
                Music_List_Change(3);
            else if ((System.Windows.Forms.Control.ModifierKeys & Keys.Shift) == Keys.Shift && e.Key == Key.D5)
                Music_List_Change(4);
            else if ((System.Windows.Forms.Control.ModifierKeys & Keys.Shift) == Keys.Shift && e.Key == Key.D6)
                Music_List_Change(5);
            else if ((System.Windows.Forms.Control.ModifierKeys & Keys.Shift) == Keys.Shift && e.Key == Key.D7)
                Music_List_Change(6);
            else if ((System.Windows.Forms.Control.ModifierKeys & Keys.Shift) == Keys.Shift && e.Key == Key.D8)
                Music_List_Change(7);
            else if ((System.Windows.Forms.Control.ModifierKeys & Keys.Shift) == Keys.Shift && e.Key == Key.D9)
                Music_List_Change(8);
            //再生開始時間を保存
            if ((System.Windows.Forms.Control.ModifierKeys & Keys.Shift) == Keys.Shift && e.Key == Key.S)
            {
                Start_Time = Location_S.Value;
                if (Start_Time > End_Time)
                    End_Time = Location_S.Maximum;
                Loop_Time_T.Text = "再生時間:" + (int)Start_Time + "～" + (int)End_Time;
            }
            //再生終了時間を保存
            if ((System.Windows.Forms.Control.ModifierKeys & Keys.Shift) == Keys.Shift && e.Key == Key.E)
            {
                End_Time = Location_S.Value;
                if (End_Time < Start_Time)
                    Start_Time = 0;
                Loop_Time_T.Text = "再生時間:" + (int)Start_Time + "～" + (int)End_Time;
            }
            //保存した時間を取り消す
            if ((System.Windows.Forms.Control.ModifierKeys & Keys.Shift) == Keys.Shift && e.Key == Key.C)
            {
                Start_Time = 0;
                End_Time = Location_S.Maximum;
                Loop_Time_T.Text = "再生時間:" + (int)Start_Time + "～" + (int)End_Time;
            }
        }
        //曲のファイルをドラッグするとリストに追加できるように(ドラッグのコードはMainCode.csに記載)
        public void Add_Music_From_Drop(string[] Music_Files)
        {
            if (IsBusy)
                return;
            try
            {
                List<string> Files = new List<string>();
                foreach (string Drop_File in Music_Files)
                {
                    string Ex = System.IO.Path.GetExtension(Drop_File);
                    if (Ex == ".mp3" || Ex == ".wav" || Ex == ".ogg" || Ex == ".aiff" || Ex == ".flac" || Ex == ".m4a" || Ex == ".mp4")
                        Files.Add(Drop_File);
                }
                string Message = "";
                for (int Number = 0; Number <= 7; Number++)
                {
                    if (Files.Count - 1 < Number)
                        break;
                    Message += Path.GetFileName(Files[Number]) + "\n";
                    if (Number == 7 && Files.Count > 8)
                        Message += "...\n";
                }
                MessageBoxResult result = System.Windows.MessageBox.Show("以下のファイルをリスト" + (Music_Select_List + 1) + "に追加しますか？\n" + Message, "確認", MessageBoxButton.YesNo,
                    MessageBoxImage.Exclamation, MessageBoxResult.Yes);
                if (result == MessageBoxResult.Yes)
                    Music_Add_From_Array(Files.ToArray());
            }
            catch (Exception e1)
            {
                Sub_Code.Error_Log_Write(e1.Message);
            }
        }
        private void Music_Vocal_Inst_Cut_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
                return;
            Vocal_Inst_Cut_User_Window.Window_Show();
        }
        private void ListBoxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            int Before_Index = Music_List.SelectedIndex;
            ((ListBoxItem)sender).IsSelected = true;
            int Index = Music_List.SelectedIndex;
            Music_List.SelectedIndex = Index;
            if (Index != -1)
            {
                ListBoxItem LBI = Music_List.Items[Index] as ListBoxItem;
                int Select_Index = Music_Data[Music_Select_List].Select(h => h.File_Name_Path).ToList().IndexOf(LBI.Content.ToString());
                if (Before_Index != -1)
                {
                    LBI = Music_List.Items[Index] as ListBoxItem;
                    int Select_Before_Index = Music_Data[Music_Select_List].Select(h => h.File_Name_Path).ToList().IndexOf(LBI.Content.ToString());
                    if (Index == Before_Index)
                        return;
                }
                if (Played_IDs.Contains(Music_Data[Music_Select_List][Select_Index].ID))
                {
                    Played_IDs.Clear();
                    Music_List_Sort();
                    Music_List.SelectedIndex = Index;
                }
                else if (Before_Index != -1)
                {
                    LBI = Music_List.Items[Before_Index] as ListBoxItem;
                    LBI.Foreground = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#BF6C6C6C");
                }
                Played_IDs.Add(Music_Data[Music_Select_List][Select_Index].ID);
            }
        }
        private void Page_Next_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
                return;
            if (Music_Select_List < 8)
                Music_List_Change(Music_Select_List + 1);
        }
        private void Page_Back_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
                return;
            if (Music_Select_List > 0)
                Music_List_Change(Music_Select_List - 1);
        }
        void Set_Position_Slider()
        {
            long position = Bass.BASS_ChannelGetPosition(Stream);
            Location_S.Value = Bass.BASS_ChannelBytes2Seconds(Stream, position);
        }
        private void Slider_S_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = true;
        }
        private void Setting_B_Click(object sender, RoutedEventArgs e)
        {
            Music_Player_Setting_Window.Window_Show();
        }
        private void Music_Full_Screen_B_Click(object sender, RoutedEventArgs e)
        {
            if (!IsVideoMode)
                return;
            IsFullScreen = !IsFullScreen;
            Volume_T.Visibility = IsFullScreen ? Visibility.Hidden : Visibility.Visible;
            Volume_S.Visibility = IsFullScreen ? Visibility.Hidden : Visibility.Visible;
            Location_T.Visibility = IsFullScreen ? Visibility.Hidden : Visibility.Visible;
            Location_S.Visibility = IsFullScreen ? Visibility.Hidden : Visibility.Visible;
            if (IsSyncPitch_And_Speed)
            {
                Pitch_Speed_T.Visibility = IsFullScreen ? Visibility.Hidden : Visibility.Visible;
                Pitch_Speed_S.Visibility = IsFullScreen ? Visibility.Hidden : Visibility.Visible;
            }
            else
            {
                Pitch_T.Visibility = IsFullScreen ? Visibility.Hidden : Visibility.Visible;
                Pitch_S.Visibility = IsFullScreen ? Visibility.Hidden : Visibility.Visible;
                Speed_T.Visibility = IsFullScreen ? Visibility.Hidden : Visibility.Visible;
                Speed_S.Visibility = IsFullScreen ? Visibility.Hidden : Visibility.Visible;
            }
            Video_Change_B.Visibility = IsFullScreen ? Visibility.Hidden : Visibility.Visible;
            Music_Fix_B.Visibility = IsFullScreen ? Visibility.Hidden : Visibility.Visible;
            Exit_B.Visibility = IsFullScreen ? Visibility.Hidden : Visibility.Visible;
            Music_Minus_B.Visibility = IsFullScreen ? Visibility.Hidden : Visibility.Visible;
            Music_Plus_B.Visibility = IsFullScreen ? Visibility.Hidden : Visibility.Visible;
            Music_Play_B.Visibility = IsFullScreen ? Visibility.Hidden : Visibility.Visible;
            Music_Pause_B.Visibility = IsFullScreen ? Visibility.Hidden : Visibility.Visible;
            Loop_T.Visibility = IsFullScreen ? Visibility.Hidden : Visibility.Visible;
            Loop_C.Visibility = IsFullScreen ? Visibility.Hidden : Visibility.Visible;
            Random_T.Visibility = IsFullScreen ? Visibility.Hidden : Visibility.Visible;
            Random_C.Visibility = IsFullScreen ? Visibility.Hidden : Visibility.Visible;
            Mode_T.Visibility = IsFullScreen ? Visibility.Hidden : Visibility.Visible;
            Mode_C.Visibility = IsFullScreen ? Visibility.Hidden : Visibility.Visible;
            Background_T.Visibility = IsFullScreen ? Visibility.Hidden : Visibility.Visible;
            Background_C.Visibility = IsFullScreen ? Visibility.Hidden : Visibility.Visible;
            Zoom_T.Visibility = IsFullScreen ? Visibility.Hidden : Visibility.Visible;
            Zoom_S.Visibility = IsFullScreen ? Visibility.Hidden : Visibility.Visible;
            Video_Border_01.Visibility = IsFullScreen ? Visibility.Hidden : Visibility.Visible;
            Video_Border_02.Visibility = IsFullScreen ? Visibility.Hidden : Visibility.Visible;
            Video_V.Width = IsFullScreen ? 2000 : 1440;
            Video_V.Height = IsFullScreen ? (Sub_Code.IsWindowBarShow ? 1055 : 1080) : 810;
            Music_Full_Screen_B.Margin = IsFullScreen ? new Thickness(-3515, (Sub_Code.IsWindowBarShow ? 25 : 0), 0, 0) : new Thickness(-1350, 915, 0, 0);
            Music_Full_Screen_B.Content = IsFullScreen ? "ウィンドウモード" : "全画面モード";
            Zoom_S.Value = IsFullScreen ? 1.333333333333333 : 1;
            if (IsFullScreen)
                Video_V.Margin = new Thickness(-1920, 0, 0, 0);
            else
                Video_V_MouseRightButtonDown(null, null);
        }
        async void Change_Thumbnail_Fade(int Index)
        {
            if (Thumbnail_Main.Visibility == Thumbnail_Sub.Visibility && Thumbnail_Main.Visibility == Visibility.Hidden)
                return;
            Fade_Count++;
            int Fade_Count_Now = Fade_Count;
            Thumbnail_Index_Now = Index;
            System.Windows.Controls.Image Fade_Out;
            System.Windows.Controls.Image Fade_In;
            if (Thumbnail_Main.Opacity >= Thumbnail_Sub.Opacity)
            {
                Fade_Out = Thumbnail_Main;
                Fade_In = Thumbnail_Sub;
            }
            else
            {
                Fade_Out = Thumbnail_Sub;
                Fade_In = Thumbnail_Main;
            }
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = new MemoryStream(Thumbnails[Index]);
            image.EndInit();
            Fade_In.Source = image;
            Fade_In.Visibility = Visibility.Visible;
            while ((Fade_Out.Opacity > 0 || Fade_In.Opacity < 1) && Fade_Count == Fade_Count_Now)
            {
                Fade_Out.Opacity -= 0.025;
                Fade_In.Opacity += 0.025;
                await Task.Delay(1000 / 60);
            }
        }
        private async void Music_Rename_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy || Music_List.SelectedIndex == -1)
                return;
            ListBoxItem Item = Music_List.SelectedItem as ListBoxItem;
            Music_Rename_Index = Music_Data[Music_Select_List].Select(h => h.ID).ToList().IndexOf((uint)Item.Tag);
            Rename_T.Text = Path.GetFileNameWithoutExtension(Music_Data[Music_Select_List][Music_Rename_Index].File_Name_Path);
            Rename_T.UndoLimit = 0;
            Rename_T.UndoLimit = 15;
            Rename_Canvas.Opacity = 0;
            Rename_Canvas.Visibility = Visibility.Visible;
            while (Rename_Canvas.Opacity < 1 && !IsRenameClosing)
            {
                Rename_Canvas.Opacity += Sub_Code.Window_Feed_Time * 2;
                await Task.Delay(1000 / 60);
            }
        }
        private async void Rename_Cancel_B_Click(object sender, RoutedEventArgs e)
        {
            if (!IsRenameClosing)
            {
                IsRenameClosing = true;
                while (Rename_Canvas.Opacity > 0)
                {
                    Rename_Canvas.Opacity -= Sub_Code.Window_Feed_Time * 2;
                    await Task.Delay(1000 / 60);
                }
                Rename_T.Text = "";
                IsRenameClosing = false;
                Rename_Canvas.Visibility = Visibility.Hidden;
            }
        }
        private async void Rename_Apply_B_Click(object sender, RoutedEventArgs e)
        {
            if (!IsRenameClosing)
            {
                IsRenameClosing = true;
                string Ex = Path.GetExtension(Music_Data[Music_Select_List][Music_Rename_Index].File_Full_Path);
                foreach (Music_Child_Class Child in Music_Data[Music_Select_List])
                {
                    if (Child.File_Name_Path == Rename_T.Text + Ex)
                    {
                        System.Windows.MessageBox.Show("同名の曲がリスト内に存在します。別の名前を設定してください。");
                        IsRenameClosing = false;
                        return;
                    }
                }
                bool IsSave = false;
                ListBoxItem Item = Music_List.SelectedItem as ListBoxItem;
                if (Music_Data[Music_Select_List][Music_Rename_Index].File_Name_Path != Rename_T.Text + Ex)
                {
                    Music_Data[Music_Select_List][Music_Rename_Index].File_Name_Path = Rename_T.Text + Ex;
                    Item.Content = Rename_T.Text + Ex;
                    IsSave = true;
                    Music_List_Sort();
                }
                while (Rename_Canvas.Opacity > 0)
                {
                    Rename_Canvas.Opacity -= Sub_Code.Window_Feed_Time * 2;
                    await Task.Delay(1000 / 60);
                }
                Rename_T.Text = "";
                IsRenameClosing = false;
                Rename_Canvas.Visibility = Visibility.Hidden;
                if (IsSave)
                    Music_List_Save();
            }
        }
    }
}