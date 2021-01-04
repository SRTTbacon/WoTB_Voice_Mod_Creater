using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;

namespace WoTB_Voice_Mod_Creater.Class
{
    public partial class Other : System.Windows.Controls.UserControl
    {
        readonly string Special_Path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/WoTB_Voice_Mod_Creater";
        readonly List<string> File_Full_Path = new List<string>();
        bool IsBusy = false;
        bool IsLocationChanging = false;
        bool IsPaused = false;
        bool IsEnded = false;
        bool IsVideoClicked = false;
        bool IsVideoEnter = false;
        Point Mouse_Point = new Point(0, 0);
        Point Video_Point = new Point(0, 0);
        int Stream;
        double X_Move = 0;
        double Y_Move = 0;
        int Double_Click = 0;
        SYNCPROC IsMusicEnd;
        public Other()
        {
            InitializeComponent();
            Video_Change_B.Visibility = Visibility.Hidden;
            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            Video_V.LoadedBehavior = MediaState.Manual;
            Video_V.UnloadedBehavior = MediaState.Stop;
            Video_V.Stretch = System.Windows.Media.Stretch.Uniform;
            Opacity = 0;
            Location_S.AddHandler(MouseDownEvent, new MouseButtonEventHandler(Location_MouseDown), true);
            Location_S.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Location_MouseUp), true);
            Speed_S.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Speed_MouseUp), true);
            Video_Mode_Change(false);
            Video_V.MediaFailed += Video_V_MediaFailed;
            Location_S.Maximum = 0;
            Video_V.MaxWidth = 5760;
            Video_V.MaxHeight = 3240;
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
            Position_Change();
            if (File.Exists(Special_Path + "/Other_Music_List.dat"))
            {
                StreamReader str = new StreamReader(Special_Path + "/Other_Music_List.dat");
                File_Full_Path.Clear();
                Music_List.Items.Clear();
                string line;
                while ((line = str.ReadLine()) != null)
                {
                    if (line != "")
                    {
                        File_Full_Path.Add(line);
                        Music_List.Items.Add(line.Substring(line.LastIndexOf('\\') + 1));
                    }
                }
                str.Close();
            }
            while (Opacity < 1 && !IsBusy)
            {
                Opacity += 0.025;
                await Task.Delay(1000 / 60);
            }
        }
        async void Position_Change()
        {
            while (Visibility == Visibility.Visible)
            {
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
                        Stream = 1;
                        Video_V.Stop();
                        Video_V.Close();
                        Video_V.Source = null;
                        Video_Mode_Change(false);
                        Music_List.SelectedIndex = -1;
                    }
                    IsEnded = false;
                }
                if (Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_PLAYING && !IsLocationChanging)
                {
                    long position = Bass.BASS_ChannelGetPosition(Stream);
                    Location_S.Value = Bass.BASS_ChannelBytes2Seconds(Stream, position);
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
                await Task.Delay(500);
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
                Filter = "音楽ファイル(*.mp3;*.wav;*.ogg;*.aiff;*.mp4)|*.mp3;*.wav;*.ogg;*.aiff;*.mp4",
                Multiselect = true
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                foreach (string File_Now in ofd.FileNames)
                {
                    File_Full_Path.Add(File_Now);
                    Music_List.Items.Add(Path.GetFileName(File_Now));
                }
            }
            StreamWriter stw = File.CreateText(Special_Path + "/Other_Music_List.dat");
            foreach (string Now in File_Full_Path)
            {
                stw.WriteLine(Now);
            }
            stw.Close();
        }
        void List_Remove_Index()
        {
            int Music_Selected_Index = Music_List.SelectedIndex;
            Music_List.SelectedIndex = -1;
            File_Full_Path.RemoveAt(Music_Selected_Index);
            Music_List.Items.RemoveAt(Music_Selected_Index);
            Bass.BASS_ChannelStop(Stream);
            Video_V.Stop();
            Video_V.Source = null;
            Video_Change_B.Visibility = Visibility.Hidden;
            Bass.BASS_StreamFree(Stream);
            StreamWriter stw = File.CreateText(Special_Path + "/Other_Music_List.dat");
            foreach (string Now in File_Full_Path)
            {
                stw.WriteLine(Now);
            }
            stw.Close();
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
            if (IsBusy)
            {
                return;
            }
            if (Music_List.SelectedIndex != -1)
            {
                if (!File.Exists(File_Full_Path[Music_List.SelectedIndex]))
                {
                    System.Windows.MessageBox.Show("ファイルが存在しません。リストから削除されます。");
                    List_Remove_Index();
                }
                Video_Change_B.Visibility = Visibility.Visible;
                Bass.BASS_ChannelStop(Stream);
                Location_S.Value = 0;
                Bass.BASS_StreamFree(Stream);
                int StreamHandle = Bass.BASS_StreamCreateFile(File_Full_Path[Music_List.SelectedIndex], 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_LOOP);
                Stream = BassFx.BASS_FX_TempoCreate(StreamHandle, BASSFlag.BASS_FX_FREESOURCE);
                IsMusicEnd = new SYNCPROC(EndSync);
                Bass.BASS_ChannelSetSync(Stream, BASSSync.BASS_SYNC_END | BASSSync.BASS_SYNC_MIXTIME, 0, IsMusicEnd, IntPtr.Zero);
                Bass.BASS_ChannelPlay(Stream, false);
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)Volume_S.Value / 100);
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, (float)Pitch_S.Value);
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO, (float)Speed_S.Value);
                if (Path.GetExtension(File_Full_Path[Music_List.SelectedIndex]) == ".mp4" && Video_Mode_C.IsChecked.Value)
                {
                    Video_Mode_Change(true);
                    Video_V.Source = new Uri(File_Full_Path[Music_List.SelectedIndex]);
                    Video_V.Volume = 0;
                    Video_V.Visibility = Visibility.Visible;
                    Video_V.Play();
                    long position = Bass.BASS_ChannelGetPosition(Stream);
                    TimeSpan time = TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(Stream, position));
                    Video_V.Position = time;
                }
                else if (Path.GetExtension(File_Full_Path[Music_List.SelectedIndex]) == ".mp4")
                {
                    Video_Mode_Change(false);
                    Video_V.Source = new Uri(File_Full_Path[Music_List.SelectedIndex]);
                    Video_V.Volume = 0;
                }
                else
                {
                    Video_Mode_Change(false);
                    Video_V.Stop();
                    Video_V.Source = null;
                    Video_Change_B.Visibility = Visibility.Hidden;
                }
                Location_S.Maximum = Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetLength(Stream, BASSMode.BASS_POS_BYTES));
            }
            else
            {
                Location_S.Value = 0;
                Location_S.Maximum = 0;
                Location_T.Text = "00:00";
                Video_Change_B.Visibility = Visibility.Hidden;
            }
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
        private void Speed_S_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Speed_S.Value = 0;
        }
        void Location_MouseDown(object sender, MouseButtonEventArgs e)
        {
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
            Bass.BASS_ChannelSetPosition(Stream, Location_S.Value);
            Bass.BASS_ChannelPlay(Stream, false);
            if (Video_V.Visibility == Visibility.Visible)
            {
                Video_V.Play();
                long position = Bass.BASS_ChannelGetPosition(Stream);
                TimeSpan time = TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(Stream, position));
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
            TimeSpan time = TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(Stream, position));
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
        private async void Music_Play_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
            IsPaused = false;
            if (Bass.BASS_ChannelIsActive(Stream) != BASSActive.BASS_ACTIVE_PLAYING)
            {
                Video_V.Play();
                Bass.BASS_ChannelPlay(Stream, false);
            }
            float Volume_Now = 1f;
            Bass.BASS_ChannelGetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, ref Volume_Now);
            float Volume_Plus = (float)(Volume_S.Value / 100) / 30f;
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
        private void Music_Pause_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
            Pause_Volume_Animation(false);
        }
        async void Pause_Volume_Animation(bool IsStop)
        {
            IsPaused = true;
            float Volume_Now = 1f;
            Bass.BASS_ChannelGetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, ref Volume_Now);
            float Volume_Minus = Volume_Now / 30f;
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
                    Video_V.Stop();
                    Bass.BASS_StreamFree(Stream);
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
            if (Bass.BASS_ChannelBytes2Seconds(Stream, position) > 5)
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
                TimeSpan time = TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(Stream, position));
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
            }
        }
        void Video_Mode_Change(bool IsVideoMode)
        {
            if (IsVideoMode)
            {
                Music_List_T.Visibility = Visibility.Hidden;
                Music_List.Visibility = Visibility.Hidden;
                Music_Add_B.Visibility = Visibility.Hidden;
                Music_Delete_B.Visibility = Visibility.Hidden;
                Video_Mode_T.Visibility = Visibility.Hidden;
                Video_Mode_C.Visibility = Visibility.Hidden;
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
                Video_Change_B.Margin = new Thickness(-2000, 900, 0, 0);
            }
            else
            {
                Music_List_T.Visibility = Visibility.Visible;
                Music_List.Visibility = Visibility.Visible;
                Music_Add_B.Visibility = Visibility.Visible;
                Music_Delete_B.Visibility = Visibility.Visible;
                Video_Mode_T.Visibility = Visibility.Visible;
                Video_Mode_C.Visibility = Visibility.Visible;
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
                Loop_T.Margin = new Thickness(-3125, 680, 0, 0);
                Loop_C.Margin = new Thickness(-3350, 700, 0, 0);
                Random_T.Margin = new Thickness(-3100, 755, 0, 0);
                Random_C.Margin = new Thickness(-3350, 775, 0, 0);
                Video_Change_B.Margin = new Thickness(-2225, 657, 0, 0);
            }
        }
        private void Music_Fix_B_Click(object sender, RoutedEventArgs e)
        {
            long position = Bass.BASS_ChannelGetPosition(Stream);
            TimeSpan time = TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(Stream, position));
            Video_V.Position = time;
        }
        private async void Exit_B_Click(object sender, RoutedEventArgs e)
        {
            Pause_Volume_Animation(false);
            IsBusy = true;
            while (Opacity > 0)
            {
                Opacity -= 0.025;
                await Task.Delay(1000 / 60);
            }
            Pause_Volume_Animation(true);
            Video_Mode_Change(false);
            Video_V.Visibility = Visibility.Hidden;
            Video_Change_B.Visibility = Visibility.Hidden;
            X_Move = 0;
            Y_Move = 0;
            Zoom_S.Value = 1;
            IsPaused = false;
            IsBusy = false;
            Visibility = Visibility.Hidden;
        }
        private void Loop_C_Click(object sender, RoutedEventArgs e)
        {
            Random_C.IsChecked = false;
        }
        private void Random_C_Click(object sender, RoutedEventArgs e)
        {
            Loop_C.IsChecked = false;
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
            catch
            {

            }
        }
        async void Double_Click_Video_V()
        {
            if (Double_Click == 1)
            {
                int Number = 0;
                while (true)
                {
                    if (Double_Click >= 2)
                    {
                        X_Move = 0;
                        Y_Move = 0;
                        Double_Click = 0;
                        Video_V.Margin = new Thickness(-1200 - Video_V.Width / 2, -(Video_V.Height - 810) / 3, 0, 0);
                        break;
                    }
                    if (Number >= 10)
                    {
                        Double_Click = 0;
                        break;
                    }
                    Number++;
                    await Task.Delay(1000 / 60);
                }
            }
        }
        private void Video_V_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Mouse_Point.X = System.Windows.Forms.Cursor.Position.X;
            Mouse_Point.Y = System.Windows.Forms.Cursor.Position.Y;
            Video_Point.X = Video_V.Margin.Left;
            Video_Point.Y = Video_V.Margin.Top;
            IsVideoClicked = true;
        }
        private void DockPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsVideoClicked)
            {
                X_Move += (System.Windows.Forms.Cursor.Position.X - Mouse_Point.X) * (1 / Zoom_S.Value);
                Y_Move += (System.Windows.Forms.Cursor.Position.Y - Mouse_Point.Y) * (1 / Zoom_S.Value);
                IsVideoClicked = false;
                Double_Click++;
                Double_Click_Video_V();
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
    }
}