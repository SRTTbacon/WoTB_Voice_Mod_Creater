using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using WK.Libraries.BetterFolderBrowserNS;

namespace WoTB_Voice_Mod_Creater.Wwise_Class
{
    public partial class Tools_V2 : System.Windows.Controls.UserControl
    {
        int Stream;
        int SelectIndex = -1;
        float SetFirstFreq = 44100f;
        bool IsClosing = false;
        bool IsEnded = false;
        bool IsPaused = false;
        bool IsLocationChanging = false;
        bool IsMessageShowing = false;
        bool IsPCKFile = false;
        bool IsOpenDialog = false;
        SYNCPROC IsMusicEnd;
        Wwise_File_Extract_V2 Wwise_Bnk;
        Wwise_File_Extract_V1 Wwise_Pck;
        public Tools_V2()
        {
            InitializeComponent();
            Location_S.AddHandler(MouseDownEvent, new MouseButtonEventHandler(Location_MouseDown), true);
            Location_S.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Location_MouseUp), true);
        }
        public async void Window_Show()
        {
            Opacity = 0;
            Visibility = Visibility.Visible;
            Position_Change();
            while (Opacity < 1 && !IsClosing)
            {
                Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
        }
        async void Position_Change()
        {
            while (Visibility == Visibility.Visible)
            {
                if (IsEnded)
                {
                    await Task.Delay(500);
                    Bass.BASS_ChannelPause(Stream);
                    Bass.BASS_ChannelSetPosition(Stream, 0, BASSMode.BASS_POS_BYTES);
                    IsPaused = true;
                    IsEnded = false;
                    Location_S.Value = 0;
                    Location_T.Text = "00:00";
                }
                if (Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_PLAYING && !IsLocationChanging)
                {
                    long position = Bass.BASS_ChannelGetPosition(Stream);
                    Location_S.Value = Bass.BASS_ChannelBytes2Seconds(Stream, position);
                    TimeSpan Time = TimeSpan.FromSeconds(Location_S.Value);
                    string Minutes = Time.Minutes.ToString();
                    string Seconds = Time.Seconds.ToString();
                    if (Time.Minutes < 10)
                        Minutes = "0" + Time.Minutes;
                    if (Time.Seconds < 10)
                        Seconds = "0" + Time.Seconds;
                    Location_T.Text = Minutes + ":" + Seconds;
                }
                await Task.Delay(100);
            }
        }
        void Location_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsClosing)
                return;
            IsLocationChanging = true;
            IsPaused = true;
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, 0f);
        }
        async void Location_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsClosing)
                return;
            IsLocationChanging = false;
            IsPaused = false;
            Bass.BASS_ChannelSetPosition(Stream, Location_S.Value);
            Bass.BASS_ChannelPlay(Stream, false);
            float Volume_Now = 0f;
            float Volume_Plus = (float)(Volume_S.Value / 100) / 20f;
            while (Volume_Now < (float)(Volume_S.Value / 100) && !IsPaused)
            {
                Volume_Now += Volume_Plus;
                if (Volume_Now > 1f)
                    Volume_Now = 1f;
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, Volume_Now);
                await Task.Delay(1000 / 60);
            }
        }
        private void Location_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLocationChanging)
                Music_Pos_Change(Location_S.Value, false);
        }
        void Music_Pos_Change(double Pos, bool IsBassPosChange)
        {
            if (IsClosing)
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
                Minutes = "0" + Time.Minutes;
            if (Time.Seconds < 10)
                Seconds = "0" + Time.Seconds;
            Location_T.Text = Minutes + ":" + Seconds;
        }
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
            IsMessageShowing = false;
            Message_T.Text = "";
            Message_T.Opacity = 1;
        }
        void EndSync(int handle, int channel, int data, IntPtr user)
        {
            if (!IsEnded)
                IsEnded = true;
        }
        private void Open_File_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog()
            {
                Title = "サウンドファイルを選択してください。",
                Filter = "サウンドファイル(*.bnk;*.pck)|*.bnk;*.pck",
                Multiselect = false
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    Sound_List.Items.Clear();
                    if (Wwise_Bnk != null)
                        Wwise_Bnk.Bank_Clear();
                    if (Wwise_Pck != null)
                        Wwise_Pck.Pck_Clear();
                    string Ex = Path.GetExtension(ofd.FileName);
                    if (Ex == ".bnk")
                    {
                        Wwise_Bnk = new Wwise_File_Extract_V2(ofd.FileName);
                        foreach (string Name_ID in Wwise_Bnk.Wwise_Get_Names())
                            Sound_List.Items.Add((Sound_List.Items.Count + 1) + ":" + Name_ID);
                        IsPCKFile = false;
                    }
                    else if (Ex == ".pck")
                    {
                        Wwise_Pck = new Wwise_File_Extract_V1(ofd.FileName);
                        foreach (string Name_ID in Wwise_Pck.Wwise_Get_Banks_ID())
                            Sound_List.Items.Add((Sound_List.Items.Count + 1) + ":" + Name_ID);
                        IsPCKFile = true;
                    }
                }
                catch (Exception e1)
                {
                    Sub_Code.Error_Log_Write(e1.Message);
                    Message_Feed_Out("エラー:ファイルを読み取れませんでした。");
                }
            }
        }
        private async void Sound_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsClosing)
            {
                return;
            }
            if (Sound_List.SelectedIndex != -1)
            {
                Message_T.Opacity = 1;
                Message_T.Text = "サウンドファイルに変換しています...";
                await Task.Delay(50);
                if (IsPCKFile)
                {
                    if (Wwise_Pck.Wwise_Extract_To_Ogg_File(Sound_List.SelectedIndex, Voice_Set.Special_Path + "/Wwise/Temp_02.ogg", true))
                        Message_Feed_Out("変換しました。");
                    else
                        Message_Feed_Out("変換できませんでした。");
                }
                else
                {
                    if (Wwise_Bnk.Wwise_Extract_To_Ogg_File(Sound_List.SelectedIndex, Voice_Set.Special_Path + "/Wwise/Temp_02.ogg", true))
                        Message_Feed_Out("変換しました。");
                    else
                        Message_Feed_Out("変換できませんでした。");
                }
            }
        }
        private void Play_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (IsClosing)
            {
                return;
            }
            if (Sound_List.SelectedIndex == -1)
            {
                return;
            }
            else if (!File.Exists(Voice_Set.Special_Path + "/Wwise/Temp_02.ogg"))
            {
                Message_Feed_Out("サウンドファイルが変換されませんでした。");
                return;
            }
            if (SelectIndex == Sound_List.SelectedIndex)
            {
                Bass.BASS_ChannelPlay(Stream, false);
            }
            else
            {
                Bass.BASS_ChannelStop(Stream);
                Location_S.Value = 0;
                Bass.BASS_StreamFree(Stream);
                int StreamHandle = Bass.BASS_StreamCreateFile(Voice_Set.Special_Path + "/Wwise/Temp_02.ogg", 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_LOOP);
                Stream = BassFx.BASS_FX_TempoCreate(StreamHandle, BASSFlag.BASS_FX_FREESOURCE);
                IsMusicEnd = new SYNCPROC(EndSync);
                Bass.BASS_ChannelSetDevice(Stream, Video_Mode.Sound_Device);
                Bass.BASS_ChannelSetSync(Stream, BASSSync.BASS_SYNC_END | BASSSync.BASS_SYNC_MIXTIME, 0, IsMusicEnd, IntPtr.Zero);
                Bass.BASS_ChannelPlay(Stream, true);
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)Volume_S.Value / 100);
                Bass.BASS_ChannelGetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, ref SetFirstFreq);
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, SetFirstFreq + (float)Speed_S.Value);
                SelectIndex = Sound_List.SelectedIndex;
                Location_S.Maximum = Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetLength(Stream, BASSMode.BASS_POS_BYTES));
            }
            IsPaused = false;
        }
        private void Pause_B_Click(object sender, RoutedEventArgs e)
        {
            Bass.BASS_ChannelPause(Stream);
            IsPaused = true;
        }
        private void Speed_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Speed_T.Text = "速度:" + (SetFirstFreq + Math.Floor(Speed_S.Value));
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, SetFirstFreq + (float)Speed_S.Value);
        }
        private void Speed_S_MouseRightClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Speed_S.Value = 0;
        }
        private void Volume_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Volume_T != null)
            {
                Volume_T.Text = "音量:" + (int)Volume_S.Value;
                if (!IsPaused)
                {
                    Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)Volume_S.Value / 100);
                }
            }
        }
        private void Minus_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
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
        }
        private void Plus_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
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
        }
        private async void Back_B_Click(object sender, RoutedEventArgs e)
        {
            if (!IsClosing)
            {
                IsClosing = true;
                Bass.BASS_ChannelPause(Stream);
                IsPaused = true;
                while (Opacity > 0)
                {
                    Opacity -= Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
                Visibility = Visibility.Hidden;
                IsClosing = false;
            }
        }
        private void File_Encode_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog()
            {
                Title = "サウンドファイルを選択してください。",
                Filter = "サウンドファイル(*.mp3;*.wav;*.ogg;*.aac;*.flac;*.wma)|*.mp3;*.wav;*.ogg;*.aac;*.flac;*.wma",
                Multiselect = true
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (string File_Now in ofd.FileNames)
                {
                    string File_Dir = Path.GetDirectoryName(File_Now);
                    Sub_Code.File_To_WEM(File_Now, File_Dir + "/" + Path.GetFileNameWithoutExtension(File_Now) + ".wem", true);
                }
            }
        }
        private async void Extract_All_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsOpenDialog)
                return;
            if (Sound_List.Items.Count == 0)
            {
                Message_Feed_Out("ファイルが選択されていません。");
                return;
            }
            IsOpenDialog = true;
            BetterFolderBrowser ofd = new BetterFolderBrowser()
            {
                Title = "抽出先を指定してください。",
                Multiselect = false,
                RootFolder = Sub_Code.Get_OpenDirectory_Path()
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                IsClosing = true;
                for (int i = 0; i < Sound_List.Items.Count; i++)
                {
                    Message_T.Text = (i + 1) + "個目のファイルを抽出しています...";
                    await Task.Delay(30);
                    string Name = Sound_List.Items[i].ToString().Substring(Sound_List.Items[i].ToString().IndexOf(':') + 1);
                    if (IsPCKFile)
                        Wwise_Pck.Wwise_Extract_To_Ogg_File(i, ofd.SelectedFolder + "/" + Name + ".ogg", true);
                    else
                        Wwise_Bnk.Wwise_Extract_To_Ogg_File(i, ofd.SelectedFolder + "/" + Name + ".ogg", true);
                }
                Message_Feed_Out(Sound_List.Items.Count + "個のファイルを抽出しました。");
                IsClosing = false;
            }
            IsOpenDialog = false;
        }
        private void Search_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            int Number = 0;
            bool IsExist = false;
            foreach (string Name_Now in Sound_List.Items)
            {
                if (Name_Now.Contains(Search_T.Text))
                {
                    IsExist = true;
                    break;
                }
                Number++;
            }
            if (IsExist)
            {
                Sound_List.ScrollIntoView(Sound_List.Items[Number]);
            }
            else
            {
                Message_Feed_Out("リスト内に存在しませんでした。");
            }
        }
        private async void File_Encode_V2_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog()
            {
                Title = "WEMファイルを選択してください。",
                Filter = "WEMファイル(*.wem)|*.wem",
                Multiselect = true
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    foreach (string File_Now in ofd.FileNames)
                    {
                        bool IsOK = false;
                        using (FileStream fs = new FileStream(File_Now, FileMode.Open))
                            using (BinaryReader br = new BinaryReader(fs))
                                if (System.Text.Encoding.ASCII.GetString(br.ReadBytes(4)) == "RIFF")
                                    IsOK = true;
                        if (IsOK)
                        {
                            Message_T.Text = Path.GetFileName(File_Now) + "を変換しています...";
                            await Task.Delay(50);
                            Sub_Code.WEM_To_File(File_Now, Path.GetDirectoryName(File_Now) + "/" + Path.GetFileNameWithoutExtension(File_Now) + ".mp3", "mp3", false);
                        }
                    }
                    Message_Feed_Out("変換しました。");
                }
                catch (Exception e1)
                {
                    Sub_Code.Error_Log_Write(e1.Message);
                    Message_Feed_Out("エラーが発生しました。");
                }
            }
        }
        private async void Extract_Select_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            if (Sound_List.Items.Count == 0)
            {
                Message_Feed_Out("ファイルが選択されていません。");
                return;
            }
            if (Sound_List.SelectedIndex == -1)
            {
                Message_Feed_Out("抽出したい項目を選択してください。");
                return;
            }
            SaveFileDialog sfd = new SaveFileDialog()
            {
                Title = "保存場所を指定してください。",
                Filter = "MP3ファイル(*.mp3)|*.mp3|WAVファイル(*.wav)|*.wav",
                AddExtension = true
            };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                Message_T.Text = "指定した項目のファイルを抽出しています...";
                await Task.Delay(50);
                string SaveExtention = ".mp3";
                if (Path.GetExtension(sfd.FileName) == ".wav")
                    SaveExtention = ".wav";
                if (IsPCKFile)
                    Wwise_Pck.Wwise_Extract_To_Ogg_File(Sound_List.SelectedIndex, Voice_Set.Special_Path + "/" + Name + ".ogg", true);
                else
                    Wwise_Bnk.Wwise_Extract_To_Ogg_File(Sound_List.SelectedIndex, Voice_Set.Special_Path + "/" + Name + ".ogg", true);
                Sub_Code.Audio_Encode_To_Other(Voice_Set.Special_Path + "/" + Name + ".ogg", sfd.FileName, SaveExtention, true);
                Message_Feed_Out("保存しました。");
            }
        }
        private void Clear_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            Sound_List.Items.Clear();
            if (Wwise_Bnk != null)
                Wwise_Bnk.Bank_Clear();
            if (Wwise_Pck != null)
                Wwise_Pck.Pck_Clear();
            IsPCKFile = false;
        }
    }
}