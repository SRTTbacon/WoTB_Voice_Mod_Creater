using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;

public class Video_Mode
{
    public static double Width = 1920;
    public static bool IsVideoClicked = false;
    public static int Sound_Device = -1;
}
namespace WoTB_Voice_Mod_Creater.Class
{
    public partial class Other : System.Windows.Controls.UserControl
    {
        readonly List<string> File_Full_Path = new List<string>();
        readonly List<string> File_Name_Path = new List<string>();
        string Video_Mode_Select_Name = "";
        bool IsBusy = false;
        bool IsLocationChanging = false;
        bool IsPaused = false;
        bool IsEnded = false;
        bool IsVideoClicked = false;
        bool IsVideoEnter = false;
        bool IsNotMusicChange = false;
        bool IsSaveOK = false;
        bool IsShowWAVEForm = false;
        bool IsWaveGrayLoaded = false;
        bool IsWaveColorLoaded = false;
        bool IsPlayingMouseDown = false;
        bool IsSyncPitch_And_Speed = false;
        System.Windows.Point Mouse_Point = new System.Windows.Point(0, 0);
        System.Windows.Point Video_Point = new System.Windows.Point(0, 0);
        System.Windows.Media.Imaging.BitmapImage Wave_Gray_Image_Source = null;
        System.Windows.Media.Imaging.BitmapImage Wave_Color_Image_Source = null;
        Un4seen.Bass.Misc.WaveForm WF_Gray = null;
        Un4seen.Bass.Misc.WaveForm WF_Color = null;
        int Stream;
        int SetFirstDevice = -1;
        int WAVEForm_Image_Width = 0;
        int WAVEForm_Image_Height = 0;
        float Music_Frequency = 44100f;
        double X_Move = 0;
        double Y_Move = 0;
        double Start_Time = -1;
        double End_Time = -1;
        SYNCPROC IsMusicEnd;
        BASS_DEVICEINFO info = new BASS_DEVICEINFO();
        public Other()
        {
            InitializeComponent();
            Video_Change_B.Visibility = Visibility.Hidden;
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
            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            SetFirstDevice = Bass.BASS_GetDevice();
            Device_L.SelectedIndex = SetFirstDevice - 1;
            Bass.BASS_Free();
            for (int n = 1; Bass.BASS_GetDeviceInfo(n, info); n++)
            {
                Bass.BASS_Init(n, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
                Device_L.Items.Add(info.name);
            }
            WAVEForm_Image_Width = (int)WAVEForm_Gray_Image.Width;
            WAVEForm_Image_Height = (int)WAVEForm_Gray_Image.Height;
            Position_Change();
        }
        private void Video_V_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Video_V.Stop();
            Video_V.Close();
            Video_V.Source = new Uri(File_Full_Path[Music_List.SelectedIndex]);
            Video_V.Volume = 0;
            Video_V.Play();
            long position = Bass.BASS_ChannelGetPosition(Stream);
            TimeSpan time = TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(Stream, position));
            Video_V.Position = time;
        }
        public async void Window_Show()
        {
            Visibility = Visibility.Visible;
            Zoom_S.Value = 1;
            if (File.Exists(Voice_Set.Special_Path + "/Configs/Other_Music_List.dat") && Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_STOPPED)
            {
                try
                {
                    using (var eifs = new FileStream(Voice_Set.Special_Path + "/Configs/Other_Music_List.dat", FileMode.Open, FileAccess.Read))
                    {
                        using (var eofs = new FileStream(Voice_Set.Special_Path + "/Configs/Temp_Other_Music_List.dat", FileMode.Create, FileAccess.Write))
                        {
                            FileEncode.FileEncryptor.Decrypt(eifs, eofs, "SRTTbacon_Music_List_Save");
                        }
                    }
                    StreamReader str = new StreamReader(Voice_Set.Special_Path + "/Configs/Temp_Other_Music_List.dat");
                    File_Full_Path.Clear();
                    File_Name_Path.Clear();
                    string line;
                    while ((line = str.ReadLine()) != null)
                    {
                        if (line != "")
                        {
                            File_Full_Path.Add(line);
                            File_Name_Path.Add(line.Substring(line.LastIndexOf('\\') + 1));
                        }
                    }
                    str.Close();
                    File.Delete(Voice_Set.Special_Path + "/Configs/Temp_Other_Music_List.dat");
                    Music_List_Sort();
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show("リストが破損しているためファイルを読み込めませんでした。\nエラー回避のためリストは削除されます。");
                    File.Delete(Voice_Set.Special_Path + "/Configs/Other_Music_List.dat");
                    File_Full_Path.Clear();
                    File_Name_Path.Clear();
                    Music_List.Items.Clear();
                    Sub_Code.Error_Log_Write(e.Message);
                }
            }
            if (File.Exists(Voice_Set.Special_Path + "/Configs/Music_Player.conf"))
            {
                try
                {
                    using (var eifs = new FileStream(Voice_Set.Special_Path + "/Configs/Music_Player.conf", FileMode.Open, FileAccess.Read))
                    {
                        using (var eofs = new FileStream(Voice_Set.Special_Path + "/Configs/Temp_Music_Player.tmp", FileMode.Create, FileAccess.Write))
                        {
                            FileEncode.FileEncryptor.Decrypt(eifs, eofs, "Music_Player_Configs_Save");
                        }
                    }
                    StreamReader str = new StreamReader(Voice_Set.Special_Path + "/Configs/Temp_Music_Player.tmp");
                    Video_Mode_C.IsChecked = bool.Parse(str.ReadLine());
                    Loop_C.IsChecked = bool.Parse(str.ReadLine());
                    Random_C.IsChecked = bool.Parse(str.ReadLine());
                    Background_C.IsChecked = bool.Parse(str.ReadLine());
                    Ex_Sort_C.IsChecked = bool.Parse(str.ReadLine());
                    Volume_S.Value = double.Parse(str.ReadLine());
                    //V1.2.9以前のデータでは読み込まれません
                    try
                    {
                        WAVEForm_C.IsChecked = bool.Parse(str.ReadLine());
                        if (WAVEForm_C.IsChecked.Value)
                        {
                            IsShowWAVEForm = true;
                            WAVEForm_Gray_Image.Visibility = Visibility.Visible;
                            WAVEForm_Color_Image.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            IsShowWAVEForm = false;
                            WAVEForm_Gray_Image.Visibility = Visibility.Hidden;
                            WAVEForm_Color_Image.Visibility = Visibility.Hidden;
                        }
                        Mode_C.IsChecked = bool.Parse(str.ReadLine());
                        Music_Play_Mode_Change(Mode_C.IsChecked.Value);
                    }
                    catch
                    {
                    }
                    str.Close();
                    File.Delete(Voice_Set.Special_Path + "/Configs/Temp_Music_Player.tmp");
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show("設定を読み込めませんでした。。\nエラー回避のため設定は削除されます。");
                    File.Delete(Voice_Set.Special_Path + "/Configs/Music_Player.conf");
                    Video_Mode_C.IsChecked = false;
                    Loop_C.IsChecked = false;
                    Random_C.IsChecked = false;
                    Background_C.IsChecked = false;
                    Ex_Sort_C.IsChecked = false;
                    Sub_Code.Error_Log_Write(e.Message);
                }
            }
            Music_List_Sort();
            if (Video_V.Source != null && Music_Fix_B.Visibility == Visibility.Visible)
            {
                Video_V.Play();
                long position2 = Bass.BASS_ChannelGetPosition(Stream);
                Location_S.Value = Bass.BASS_ChannelBytes2Seconds(Stream, position2);
                TimeSpan time = TimeSpan.FromSeconds(Location_S.Value);
                Video_V.Position = time;
                Video_V.Visibility = Visibility.Visible;
                if (Bass.BASS_ChannelIsActive(Stream) != BASSActive.BASS_ACTIVE_PLAYING)
                {
                    Video_V.Pause();
                }
            }
            //キーイベントを使用できるようにするため、何でも良いのでフォーカスを与える
            IsSaveOK = true;
            while (Opacity < 1 && !IsBusy)
            {
                Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
        }
        async void Position_Change()
        {
            while (true)
            {
                if (Visibility == Visibility.Visible)
                {
                    if (Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_PLAYING && !IsLocationChanging)
                    {
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
                        long position = Bass.BASS_ChannelGetPosition(Stream);
                        Location_S.Value = Bass.BASS_ChannelBytes2Seconds(Stream, position);
                        if (Loop_C.IsChecked.Value && Start_Time != -1 && Location_S.Value >= End_Time)
                        {
                            Music_Pos_Change(Start_Time, true);
                            long position2 = Bass.BASS_ChannelGetPosition(Stream);
                            Location_S.Value = Bass.BASS_ChannelBytes2Seconds(Stream, position2);
                            TimeSpan time = TimeSpan.FromSeconds(Location_S.Value);
                            Video_V.Position = time;
                        }
                        else if (Loop_C.IsChecked.Value && Start_Time != -1 && Location_S.Value < Start_Time)
                        {
                            Music_Pos_Change(Start_Time, true);
                            long position2 = Bass.BASS_ChannelGetPosition(Stream);
                            Location_S.Value = Bass.BASS_ChannelBytes2Seconds(Stream, position2);
                            TimeSpan time = TimeSpan.FromSeconds(Location_S.Value);
                            Video_V.Position = time;
                        }
                        if (WAVEForm_Color_Image.Visibility == Visibility.Visible)
                        {
                            WAVEForm_Color_Image.Width = (Location_S.Value / Location_S.Maximum) * WAVEForm_Image_Width;
                        }
                        TimeSpan Time = TimeSpan.FromSeconds(Location_S.Value);
                        string Minutes = Time.Minutes.ToString();
                        string Seconds = Time.Seconds.ToString();
                        if (Time.Minutes < 10)
                        {
                            Minutes = "0" + Time.Minutes;
                        }
                        if (Time.Seconds < 10)
                        {
                            Seconds = "0" + Time.Seconds;
                        }
                        Location_T.Text = Minutes + ":" + Seconds;
                    }
                    if (Music_Fix_B.Visibility == Visibility.Hidden && Video_V.Visibility == Visibility.Visible)
                    {
                        Video_V.Pause();
                        Video_V.Visibility = Visibility.Hidden;
                    }
                }
                if (IsEnded)
                {
                    if (Loop_C.IsChecked.Value)
                    {
                        Bass.BASS_ChannelSetPosition(Stream, 0);
                        Video_V.Position = TimeSpan.FromSeconds(0);
                    }
                    else if (Random_C.IsChecked.Value)
                    {
                        if (Music_List.Items.Count == 1)
                        {
                            Bass.BASS_ChannelSetPosition(Stream, 0);
                            Video_V.Position = TimeSpan.FromSeconds(0);
                        }
                        else
                        {
                            Random r = new Random();
                            while (true)
                            {
                                int r2 = r.Next(0, Music_List.Items.Count);
                                if (Music_List.SelectedIndex != r2)
                                {
                                    Music_List.SelectedIndex = r2;
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
                        Device_T.Margin = new Thickness(-2100, 640, 0, 0);
                        Device_L.Margin = new Thickness(-2100, 700, 0, 0);
                        WAVEForm_Gray_Image.Source = null;
                        WAVEForm_Color_Image.Source = null;
                    }
                    IsEnded = false;
                }
                await Task.Delay(1000 / 15);
            }
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
        void EndSync(int handle, int channel, int data, IntPtr user)
        {
            if (!IsEnded)
            {
                IsEnded = true;
            }
        }
        private void Music_Add_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Title = "音楽ファイルを選択してください。",
                Filter = "音楽ファイル(*.mp3;*.wav;*.ogg;*.aiff;*.flac;*.m4a;*.mp4)|*.mp3;*.wav;*.ogg;*.aiff;*.flac;*.m4a;*.mp4",
                Multiselect = true
            };
            string Error_File = "";
            int Number = 0;
            string Name = "";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                foreach (string File_Now in ofd.FileNames)
                {
                    foreach (string File_Now_01 in File_Name_Path)
                    {
                        if (Path.GetFileName(File_Now) == File_Now_01)
                        {
                            if (Error_File == "")
                            {
                                Error_File = File_Now;
                            }
                            else
                            {
                                Error_File += "\n" + File_Now;
                            }
                            continue;
                        }
                    }
                    File_Full_Path.Add(File_Now);
                    File_Name_Path.Add(Path.GetFileName(File_Now));
                    Number++;
                    if (Number == 1)
                    {
                        Name = Path.GetFileName(File_Now);
                    }
                }
            }
            if (Error_File != "")
            {
                System.Windows.MessageBox.Show("同名の曲が存在するため以下のファイルを追加できませんでした。\n" + Error_File);
            }
            Music_List_Sort();
            StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Configs/Temp_Other_Music_List.dat");
            foreach (string Now in File_Full_Path)
            {
                stw.WriteLine(Now);
            }
            stw.Close();
            using (var eifs = new FileStream(Voice_Set.Special_Path + "/Configs/Temp_Other_Music_List.dat", FileMode.Open, FileAccess.Read))
            {
                using (var eofs = new FileStream(Voice_Set.Special_Path + "/Configs/Other_Music_List.dat", FileMode.Create, FileAccess.Write))
                {
                    FileEncode.FileEncryptor.Encrypt(eifs, eofs, "SRTTbacon_Music_List_Save");
                }
            }
            File.Delete(Voice_Set.Special_Path + "/Configs/Temp_Other_Music_List.dat");
            if (Number == 1)
            {
                try
                {
                    Music_List.ScrollIntoView(Name);
                }
                catch (Exception e1)
                {
                    Sub_Code.Error_Log_Write(e1.Message);
                }
            }
        }
        //リストを並び替える
        //true=拡張子順,false=名前順
        void Music_List_Sort()
        {
            IsNotMusicChange = true;
            string List_Now = "";
            if (Music_List.SelectedIndex != -1)
            {
                List_Now = Music_List.SelectedItem.ToString();
            }
            if (Ex_Sort_C.IsChecked.Value)
            {
                string[] Temp_01 = { ".aiff", ".mp3", ".mp4", ".ogg", ".wav" };
                List<List<string>> Temp_02 = new List<List<string>>();
                Temp_02.Add(new List<string>());
                Temp_02.Add(new List<string>());
                Temp_02.Add(new List<string>());
                Temp_02.Add(new List<string>());
                Temp_02.Add(new List<string>());
                foreach (string Name_Now in File_Name_Path)
                {
                    for (int Number = 0; Number <= Temp_01.Length - 1; Number++)
                    {
                        if (Temp_01[Number] == Path.GetExtension(Name_Now))
                        {
                            Temp_02[Number].Add(Name_Now);
                        }
                    }
                }
                for (int Number = 0; Number <= Temp_02.Count - 1; Number++)
                {
                    Array Sort_List_01 = Temp_02[Number].ToArray();
                    Array.Sort(Sort_List_01);
                    Temp_02[Number].Clear();
                    Temp_02[Number].AddRange((IEnumerable<string>)Sort_List_01);
                }
                Music_List.Items.Clear();
                for (int Number = 0; Number <= Temp_02.Count - 1; Number++)
                {
                    if (Temp_02[Number].Count != 0)
                    {
                        foreach (string Name in Temp_02[Number])
                        {
                            Music_List.Items.Add(Name);
                        }
                    }
                }
            }
            else
            {
                Array Array_Sort = File_Name_Path.ToArray();
                Array.Sort(Array_Sort);
                Music_List.Items.Clear();
                foreach (string Name in Array_Sort)
                {
                    Music_List.Items.Add(Name);
                }
            }
            if (List_Now != "")
            {
                Music_List.SelectedItem = List_Now;
            }
            IsNotMusicChange = false;
        }
        void List_Remove_Index()
        {
            string NameOnly = Music_List.SelectedItem.ToString();
            Music_List.SelectedIndex = -1;
            int Delete_Number = File_Name_Path.IndexOf(NameOnly);
            File_Full_Path.RemoveAt(Delete_Number);
            File_Name_Path.RemoveAt(Delete_Number);
            Bass.BASS_ChannelStop(Stream);
            Video_V.Stop();
            Video_V.Source = null;
            Video_Change_B.Visibility = Visibility.Hidden;
            Bass.BASS_StreamFree(Stream);
            Music_List_Sort();
            StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Configs/Temp_Other_Music_List.dat");
            foreach (string Now in File_Full_Path)
            {
                stw.WriteLine(Now);
            }
            stw.Close();
            using (var eifs = new FileStream(Voice_Set.Special_Path + "/Configs/Temp_Other_Music_List.dat", FileMode.Open, FileAccess.Read))
            {
                using (var eofs = new FileStream(Voice_Set.Special_Path + "/Configs/Other_Music_List.dat", FileMode.Create, FileAccess.Write))
                {
                    FileEncode.FileEncryptor.Encrypt(eifs, eofs, "SRTTbacon_Music_List_Save");
                }
            }
            File.Delete(Voice_Set.Special_Path + "/Configs/Temp_Other_Music_List.dat");
        }
        private void Music_Delete_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy || Music_List.SelectedIndex == -1)
            {
                return;
            }
            List_Remove_Index();
        }
        private void Music_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsBusy || IsNotMusicChange)
            {
                return;
            }
            if (Music_List.SelectedIndex != -1)
            {
                int Select_Index = File_Name_Path.IndexOf(Music_List.Items[Music_List.SelectedIndex].ToString());
                if (!File.Exists(File_Full_Path[Select_Index]))
                {
                    System.Windows.MessageBox.Show("ファイルが存在しません。リストから削除されます。");
                    List_Remove_Index();
                    return;
                }
                Video_Change_B.Visibility = Visibility.Visible;
                IsWaveGrayLoaded = false;
                IsWaveColorLoaded = false;
                WAVEForm_Gray_Image.Source = null;
                WAVEForm_Color_Image.Source = null;
                WAVEForm_Color_Image.Width = 0;
                Bass.BASS_ChannelStop(Stream);
                Bass.BASS_StreamFree(Stream);
                Location_S.Value = 0;
                int StreamHandle = Bass.BASS_StreamCreateFile(File_Full_Path[Select_Index], 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_LOOP);
                Stream = BassFx.BASS_FX_TempoCreate(StreamHandle, BASSFlag.BASS_FX_FREESOURCE);
                IsMusicEnd = new SYNCPROC(EndSync);
                Bass.BASS_ChannelGetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, ref Music_Frequency);
                Bass.BASS_ChannelSetSync(Stream, BASSSync.BASS_SYNC_END | BASSSync.BASS_SYNC_MIXTIME, 0, IsMusicEnd, IntPtr.Zero);
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)Volume_S.Value / 100);
                if (IsSyncPitch_And_Speed)
                {
                    Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, Music_Frequency * (float)(Pitch_Speed_S.Value / 50));
                }
                else
                {
                    Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, (float)Pitch_S.Value);
                    Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO, (float)Speed_S.Value);
                }
                Location_S.Maximum = Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetLength(Stream, BASSMode.BASS_POS_BYTES));
                Start_Time = 0;
                End_Time = Location_S.Maximum;
                Loop_Time_T.Text = "再生時間:" + (int)Start_Time + "～" + (int)End_Time;
                IsPaused = false;
                Bass.BASS_ChannelPlay(Stream, false);
                if (Device_L.SelectedIndex != -1)
                {
                    Bass.BASS_ChannelSetDevice(Stream, Device_L.SelectedIndex + 1);
                }
                else
                {
                    Bass.BASS_ChannelSetDevice(Stream, SetFirstDevice);
                }
                if (Path.GetExtension(File_Full_Path[Select_Index]) == ".mp4" && Video_Mode_C.IsChecked.Value)
                {
                    Video_Mode_Change(true);
                    Device_T.Margin = new Thickness(-2100, 710, 0, 0);
                    Device_L.Margin = new Thickness(-2100, 770, 0, 0);
                    Video_V.Close();
                    Video_V.Source = new Uri(File_Full_Path[Select_Index]);
                    Video_V.Volume = 0;
                    Video_V.Visibility = Visibility.Visible;
                    Video_V.Play();
                    long position = Bass.BASS_ChannelGetPosition(Stream);
                    TimeSpan time = TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(Stream, position) + 0.1);
                    Video_V.Position = time;
                    IsShowWAVEForm = false;
                    WAVEForm_Gray_Image.Visibility = Visibility.Hidden;
                    WAVEForm_Color_Image.Visibility = Visibility.Hidden;
                    WAVEForm_C.IsChecked = false;
                }
                else if (Path.GetExtension(File_Full_Path[Select_Index]) == ".mp4")
                {
                    Device_T.Margin = new Thickness(-2100, 710, 0, 0);
                    Device_L.Margin = new Thickness(-2100, 770, 0, 0);
                    Video_Mode_Change(false);
                    Video_V.Close();
                    Video_V.Source = new Uri(File_Full_Path[Select_Index]);
                    Video_V.Volume = 0;
                    IsShowWAVEForm = false;
                    WAVEForm_Gray_Image.Visibility = Visibility.Hidden;
                    WAVEForm_Color_Image.Visibility = Visibility.Hidden;
                    WAVEForm_C.IsChecked = false;
                }
                else
                {
                    Device_T.Margin = new Thickness(-2100, 640, 0, 0);
                    Device_L.Margin = new Thickness(-2100, 700, 0, 0);
                    Video_Mode_Change(false);
                    Video_V.Stop();
                    Video_V.Close();
                    Video_Change_B.Visibility = Visibility.Hidden;
                    WF_Gray = new Un4seen.Bass.Misc.WaveForm(File_Full_Path[Select_Index], new Un4seen.Bass.Misc.WAVEFORMPROC(GetWaveFormData_Gray), null);
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
                    WF_Color = new Un4seen.Bass.Misc.WaveForm(File_Full_Path[Select_Index], new Un4seen.Bass.Misc.WAVEFORMPROC(GetWaveFormData_Color), null);
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
            {
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)Volume_S.Value / 100);
            }
        }
        private void Pitch_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Pitch_T.Text = "音程:" + (Math.Floor(Pitch_S.Value * 10) / 10).ToString();
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, (float)Pitch_S.Value);
        }
        private void Speed_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Speed_T.Text = "速度:" + (Math.Floor(Speed_S.Value * 10) / 10).ToString();
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO, (float)Speed_S.Value);
        }
        private void Pitch_S_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Pitch_S.Value = 0;
        }
        private async void Speed_S_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Speed_S.Value = 0;
            await Task.Delay(500);
            long position = Bass.BASS_ChannelGetPosition(Stream);
            TimeSpan time = TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(Stream, position) + 0.1);
            Video_V.Position = time;
            Video_V.SpeedRatio = 1 + Speed_S.Value / 100;
        }
        void Location_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
            IsLocationChanging = true;
            IsPaused = true;
            Bass.BASS_ChannelPause(Stream);
            if (Video_V.Visibility == Visibility.Visible)
            {
                Video_V.Pause();
            }
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, 0f);
        }
        async void Location_MouseUp(object sender, MouseButtonEventArgs e)
        {
            IsLocationChanging = false;
            IsPaused = false;
            if (WAVEForm_Color_Image.Visibility == Visibility.Visible)
            {
                WAVEForm_Color_Image.Width = (Location_S.Value / Location_S.Maximum) * WAVEForm_Image_Width;
            }
            Bass.BASS_ChannelSetPosition(Stream, Location_S.Value);
            Bass.BASS_ChannelPlay(Stream, false);
            if (Video_V.Visibility == Visibility.Visible)
            {
                Video_V.Play();
                long position = Bass.BASS_ChannelGetPosition(Stream);
                TimeSpan time = TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(Stream, position) + 0.1);
                Video_V.Position = time;
            }
            float Volume_Now = 0f;
            float Volume_Plus = (float)(Volume_S.Value / 100) / 20f;
            while (Volume_Now < (float)(Volume_S.Value / 100) && !IsPaused)
            {
                Volume_Now += Volume_Plus;
                if (Volume_Now > 1f)
                {
                    Volume_Now = 1f;
                }
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, Volume_Now);
                await Task.Delay(1000 / 60);
            }
        }
        private void Location_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLocationChanging)
            {
                Music_Pos_Change(Location_S.Value, false);
            }
        }
        async void Speed_MouseUp(object sender, MouseButtonEventArgs e)
        {
            await Task.Delay(500);
            long position = Bass.BASS_ChannelGetPosition(Stream);
            TimeSpan time = TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(Stream, position) + 0.1);
            Video_V.Position = time;
            Video_V.SpeedRatio = 1 + Speed_S.Value / 100;
        }
        void Music_Pos_Change(double Pos, bool IsBassPosChange)
        {
            if (IsBusy)
            {
                return;
            }
            if (IsBassPosChange)
            {
                Bass.BASS_ChannelSetPosition(Stream, Pos);
            }
            if (WAVEForm_Color_Image.Visibility == Visibility.Visible)
            {
                WAVEForm_Color_Image.Width = (Pos / Location_S.Maximum) * WAVEForm_Image_Width;
            }
            TimeSpan Time = TimeSpan.FromSeconds(Pos);
            string Minutes = Time.Minutes.ToString();
            string Seconds = Time.Seconds.ToString();
            if (Time.Minutes < 10)
            {
                Minutes = "0" + Time.Minutes;
            }
            if (Time.Seconds < 10)
            {
                Seconds = "0" + Time.Seconds;
            }
            Location_T.Text = Minutes + ":" + Seconds;
        }
        private void Music_Play_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
            Play_Volume_Animation();
        }
        private void Music_Pause_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
            Pause_Volume_Animation(false);
        }
        async void Play_Volume_Animation(float Feed_Time = 30f)
        {
            IsPaused = false;
            if (Video_V.Visibility == Visibility.Visible)
            {
                Video_V.Play();
            }
            Bass.BASS_ChannelPlay(Stream, false);
            float Volume_Now = 1f;
            Bass.BASS_ChannelGetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, ref Volume_Now);
            float Volume_Plus = (float)(Volume_S.Value / 100) / Feed_Time;
            while (Volume_Now < (float)(Volume_S.Value / 100) && !IsPaused)
            {
                Volume_Now += Volume_Plus;
                if (Volume_Now > 1f)
                {
                    Volume_Now = 1f;
                }
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
                {
                    Volume_Now = 0f;
                }
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
                    Location_S.Value = 0;
                    Location_S.Maximum = 0;
                    Location_T.Text = "00:00";
                    Music_List.SelectedIndex = -1;
                }
                else
                {
                    Bass.BASS_ChannelPause(Stream);
                    Video_V.Pause();
                }
            }
        }
        private void Music_Minus_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
            long position = Bass.BASS_ChannelGetPosition(Stream);
            double Time_Temp = Bass.BASS_ChannelBytes2Seconds(Stream, position);
            if (Loop_C.IsChecked.Value && Time_Temp - 5 < Start_Time)
            {
                Music_Pos_Change(Start_Time, true);
            }
            else if (Bass.BASS_ChannelBytes2Seconds(Stream, position) > 5)
            {
                Music_Pos_Change(Bass.BASS_ChannelBytes2Seconds(Stream, position) - 5, true);
            }
            else
            {
                Music_Pos_Change(0, true);
            }
            long position2 = Bass.BASS_ChannelGetPosition(Stream);
            Location_S.Value = Bass.BASS_ChannelBytes2Seconds(Stream, position2);
            TimeSpan time = TimeSpan.FromSeconds(Location_S.Value);
            Video_V.Position = time;
        }
        private void Music_Plus_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
            long position = Bass.BASS_ChannelGetPosition(Stream);
            if (Bass.BASS_ChannelBytes2Seconds(Stream, position) + 5 > Location_S.Maximum)
            {
                Music_Pos_Change(Location_S.Maximum, true);
            }
            else
            {
                Music_Pos_Change(Bass.BASS_ChannelBytes2Seconds(Stream, position) + 5, true);
            }
            long position2 = Bass.BASS_ChannelGetPosition(Stream);
            Location_S.Value = Bass.BASS_ChannelBytes2Seconds(Stream, position2);
            TimeSpan time = TimeSpan.FromSeconds(Location_S.Value);
            Video_V.Position = time;
        }
        private void Video_Change_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
            if (Music_List_T.Visibility == Visibility.Visible)
            {
                Video_Mode_Change(true);
                Video_V.Visibility = Visibility.Visible;
                Video_V.Play();
                long position = Bass.BASS_ChannelGetPosition(Stream);
                TimeSpan time = TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(Stream, position) + 0.1);
                Video_V.Position = time;
                if (IsPaused)
                {
                    Video_V.Pause();
                }
            }
            else
            {
                Video_Mode_Change(false);
                Video_V.Pause();
                Video_V.Visibility = Visibility.Hidden;
                WAVEForm_Color_Image.Width = (Location_S.Value / Location_S.Maximum) * WAVEForm_Image_Width;
            }
        }
        void Video_Mode_Change(bool IsVideoMode)
        {
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
                Loop_Time_T.Visibility = Visibility.Hidden;
                Ex_Sort_C.Visibility = Visibility.Hidden;
                Ex_Sort_T.Visibility = Visibility.Hidden;
                WAVEForm_Gray_Image.Visibility = Visibility.Hidden;
                WAVEForm_Color_Image.Visibility = Visibility.Hidden;
                WAVEForm_C.Visibility = Visibility.Hidden;
                WAVEForm_T.Visibility = Visibility.Hidden;
                Zoom_T.Visibility = Visibility.Visible;
                Zoom_S.Visibility = Visibility.Visible;
                Music_Fix_B.Visibility = Visibility.Visible;
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
                Video_Change_B.Margin = new Thickness(-1350, 945, 0, 0);
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
                Loop_Time_T.Visibility = Visibility.Visible;
                Ex_Sort_C.Visibility = Visibility.Visible;
                Ex_Sort_T.Visibility = Visibility.Visible;
                if (IsShowWAVEForm)
                {
                    WAVEForm_Gray_Image.Visibility = Visibility.Visible;
                    WAVEForm_Color_Image.Visibility = Visibility.Visible;
                }
                else
                {
                    WAVEForm_Gray_Image.Visibility = Visibility.Hidden;
                    WAVEForm_Color_Image.Visibility = Visibility.Hidden;
                }
                WAVEForm_C.Visibility = Visibility.Visible;
                WAVEForm_T.Visibility = Visibility.Visible;
                Music_Fix_B.Visibility = Visibility.Hidden;
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
                Loop_T.Margin = new Thickness(-3125, 630, 0, 0);
                Loop_C.Margin = new Thickness(-3350, 650, 0, 0);
                Random_T.Margin = new Thickness(-3100, 705, 0, 0);
                Random_C.Margin = new Thickness(-3350, 725, 0, 0);
                Background_T.Margin = new Thickness(-3005, 780, 0, 0);
                Background_C.Margin = new Thickness(-3350, 800, 0, 0);
                Mode_T.Margin = new Thickness(-3025, 930, 0, 0);
                Mode_C.Margin = new Thickness(-3350, 950, 0, 0);
                Video_Change_B.Margin = new Thickness(-2100, 615, 0, 0);
                if (Video_Mode_Select_Name != "")
                {
                    try
                    {
                        Music_List.ScrollIntoView(Video_Mode_Select_Name);
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
            {
                return;
            }
            long position = Bass.BASS_ChannelGetPosition(Stream);
            TimeSpan time = TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(Stream, position) + 0.1);
            Video_V.Position = time;
        }
        private async void Exit_B_Click(object sender, RoutedEventArgs e)
        {
            if (!IsBusy)
            {
                IsBusy = true;
                bool IsPaused_Now = IsPaused;
                if (!Background_C.IsChecked.Value)
                {
                    Pause_Volume_Animation(false, 25);
                }
                while (Opacity > 0)
                {
                    Opacity -= Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
                if (!Background_C.IsChecked.Value)
                {
                    Pause_Volume_Animation(true);
                    Video_Mode_Change(false);
                    Video_V.Visibility = Visibility.Hidden;
                    Video_Change_B.Visibility = Visibility.Hidden;
                    X_Move = 0;
                    Y_Move = 0;
                    Zoom_S.Value = 1;
                    if (Music_List.Items.Count > 0)
                    {
                        Music_List.ScrollIntoView(Music_List.Items[0]);
                    }
                    WAVEForm_Gray_Image.Source = null;
                    WAVEForm_Color_Image.Source = null;
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
            {
                return;
            }
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
                    {
                        Zoom_S.Value += 0.1;
                    }
                    else
                    {
                        Zoom_S.Value = 4;
                    }
                }
                else
                {
                    if (Zoom_S.Value - 0.1 >= 1)
                    {
                        Zoom_S.Value -= 0.1;
                    }
                    else
                    {
                        Zoom_S.Value = 1;
                    }
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
                Video_Mode.Sound_Device = Device_L.SelectedIndex + 1;
            }
        }
        private void Message_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
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
            {
                return;
            }
            Youtube_Link_Window.Window_Show();
            while (Youtube_Link_Window.Visibility == Visibility.Visible)
            {
                await Task.Delay(100);
            }
            if (Sub_Code.AutoListAdd.Count > 0)
            {
                foreach (string File_Now in Sub_Code.AutoListAdd)
                {
                    File_Full_Path.Add(File_Now);
                    File_Name_Path.Add(Path.GetFileName(File_Now));
                }
                Music_List_Sort();
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Configs/Temp_Other_Music_List.dat");
                foreach (string Now in File_Full_Path)
                {
                    stw.WriteLine(Now);
                }
                stw.Close();
                using (var eifs = new FileStream(Voice_Set.Special_Path + "/Configs/Temp_Other_Music_List.dat", FileMode.Open, FileAccess.Read))
                {
                    using (var eofs = new FileStream(Voice_Set.Special_Path + "/Configs/Other_Music_List.dat", FileMode.Create, FileAccess.Write))
                    {
                        FileEncode.FileEncryptor.Encrypt(eifs, eofs, "SRTTbacon_Music_List_Save");
                    }
                }
                File.Delete(Voice_Set.Special_Path + "/Configs/Temp_Other_Music_List.dat");
                if (Sub_Code.AutoListAdd.Count == 1)
                {
                    try
                    {
                        Music_List.ScrollIntoView(Music_List.Items[Music_List.Items.IndexOf(Path.GetFileName(Sub_Code.AutoListAdd[0]))]);
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
            {
                return;
            }
            try
            {
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Configs/Music_Player.tmp");
                stw.WriteLine(Video_Mode_C.IsChecked.Value);
                stw.WriteLine(Loop_C.IsChecked.Value);
                stw.WriteLine(Random_C.IsChecked.Value);
                stw.WriteLine(Background_C.IsChecked.Value);
                stw.WriteLine(Ex_Sort_C.IsChecked.Value);
                stw.WriteLine(Volume_S.Value);
                stw.WriteLine(WAVEForm_C.IsChecked.Value);
                stw.Write(Mode_C.IsChecked.Value);
                stw.Close();
                using (var eifs = new FileStream(Voice_Set.Special_Path + "/Configs/Music_Player.tmp", FileMode.Open, FileAccess.Read))
                {
                    using (var eofs = new FileStream(Voice_Set.Special_Path + "/Configs/Music_Player.conf", FileMode.Create, FileAccess.Write))
                    {
                        FileEncode.FileEncryptor.Encrypt(eifs, eofs, "Music_Player_Configs_Save");
                    }
                }
                File.Delete(Voice_Set.Special_Path + "/Configs/Music_Player.tmp");
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
            Video_V.Margin = new Thickness(-1200 - Video_V.Width / 2, -(Video_V.Height - 810) / 3, 0, 0);
        }
        private void Zoom_S_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Zoom_S.Value = 1;
        }
        private void WAVEForm_C_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (WAVEForm_C.IsChecked.Value)
                {
                    if (WAVEForm_Gray_Image.Source == null)
                    {
                        WAVEForm_C.IsChecked = false;
                        return;
                    }
                    IsShowWAVEForm = true;
                    WAVEForm_Gray_Image.Visibility = Visibility.Visible;
                    WAVEForm_Color_Image.Visibility = Visibility.Visible;
                    WAVEForm_Color_Image.Width = (Location_S.Value / Location_S.Maximum) * WAVEForm_Image_Width;
                }
                else
                {
                    IsShowWAVEForm = false;
                    WAVEForm_Gray_Image.Visibility = Visibility.Hidden;
                    WAVEForm_Color_Image.Visibility = Visibility.Hidden;
                }
                Configs_Save();
            }
            catch
            {
            }
        }
        private void WAVEForm_Gray_Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
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
                {
                    WAVEForm_Color_Image.Width = 0;
                }
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
                {
                    Minutes = "0" + Time.Minutes;
                }
                if (Time.Seconds < 10)
                {
                    Seconds = "0" + Time.Seconds;
                }
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
                    await Task.Delay(500);
                    if (Music_List_T.Visibility != Visibility.Visible)
                    {
                        long position = Bass.BASS_ChannelGetPosition(Stream);
                        TimeSpan time = TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(Stream, position) + 0.1);
                        Video_V.Position = time;
                    }
                    Video_V.SpeedRatio = Pitch_Speed_S.Value / 50;
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
                    await Task.Delay(500);
                    if (Music_List_T.Visibility != Visibility.Visible)
                    {
                        long position = Bass.BASS_ChannelGetPosition(Stream);
                        TimeSpan time = TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(Stream, position) + 0.1);
                        Video_V.Position = time;
                    }
                    Video_V.SpeedRatio = 1 + Speed_S.Value / 100;
                }
            }
        }
        private void Pitch_Speed_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, Music_Frequency * (float)(Pitch_Speed_S.Value / 50));
            Pitch_Speed_T.Text = "音程と速度:" + (int)Pitch_Speed_S.Value;
        }
        private async void Pitch_Speed_S_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Pitch_Speed_S.Value = 50;
            await Task.Delay(500);
            if (Music_List_T.Visibility != Visibility.Visible)
            {
                long position = Bass.BASS_ChannelGetPosition(Stream);
                TimeSpan time = TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(Stream, position) + 0.1);
                Video_V.Position = time;
            }
            Video_V.SpeedRatio = Pitch_Speed_S.Value / 50;
        }
        private async void Pitch_Speed_S_MouseUp(object sender, MouseButtonEventArgs e)
        {
            await Task.Delay(500);
            if (Music_List_T.Visibility != Visibility.Visible)
            {
                long position = Bass.BASS_ChannelGetPosition(Stream);
                TimeSpan time = TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(Stream, position) + 0.1);
                Video_V.Position = time;
            }
            Video_V.SpeedRatio = Pitch_Speed_S.Value / 50;
        }
        //キーイベント
        public void RootWindow_KeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (Loop_C.IsChecked.Value)
            {
                //再生開始時間を保存
                if ((System.Windows.Forms.Control.ModifierKeys & Keys.Shift) == Keys.Shift && e.Key == Key.S)
                {
                    Start_Time = Location_S.Value;
                    if (Start_Time > End_Time)
                    {
                        End_Time = Location_S.Maximum;
                    }
                    Loop_Time_T.Text = "再生時間:" + (int)Start_Time + "～" + (int)End_Time;
                }
                //再生終了時間を保存
                if ((System.Windows.Forms.Control.ModifierKeys & Keys.Shift) == Keys.Shift && e.Key == Key.E)
                {
                    End_Time = Location_S.Value;
                    if (End_Time < Start_Time)
                    {
                        Start_Time = 0;
                    }
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
        }
    }
}