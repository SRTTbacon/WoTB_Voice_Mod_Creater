using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using WK.Libraries.BetterFolderBrowserNS;
using WoTB_Voice_Mod_Creater.Class;

namespace WoTB_Voice_Mod_Creater.Wwise_Class
{
    public partial class Tools_V2 : System.Windows.Controls.UserControl
    {
        string BNK_PCK_File = "";
        int Stream;
        int SelectIndex = -1;
        double Volume_Sync_Mode = 95;
        double Volume_Async_Mode = 0;
        float SetFirstFreq = 44100f;
        bool IsClosing = false;
        bool IsEnded = false;
        bool IsPaused = false;
        bool IsLocationChanging = false;
        bool IsMessageShowing = false;
        bool IsPCKFile = false;
        bool IsOpenDialog = false;
        bool Is_Sync_Checked = false;
        bool IsOverWrite_Checked = false;
        SYNCPROC IsMusicEnd;
        Wwise_File_Extract_V2 Wwise_Bnk;
        Wwise_File_Extract_V1 Wwise_Pck;
        public Tools_V2()
        {
            InitializeComponent();
            Location_S.AddHandler(MouseDownEvent, new MouseButtonEventHandler(Location_MouseDown), true);
            Location_S.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Location_MouseUp), true);
            Change_Volume_S.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Change_Volume_S_MouseUp), true);
            Volume_Setting_Change();
        }
        public async void Window_Show()
        {
            if (All_Volume_Sync_C.Source == null)
                All_Volume_Sync_C.Source = Sub_Code.Check_01;
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
        void Change_Volume_S_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Is_Sync_Checked)
                Volume_Sync_Mode = Change_Volume_S.Value;
            else
                Volume_Async_Mode = Change_Volume_S.Value;
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
            if (IsMessageShowing)
            {
                IsMessageShowing = false;
                Message_T.Text = "";
            }
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
                    BNK_PCK_File = ofd.FileName;
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
                return;
            if (Sound_List.SelectedIndex != -1)
            {
                Message_T.Opacity = 1;
                Message_T.Text = "サウンドファイルに変換しています...";
                Bass.BASS_ChannelStop(Stream);
                Bass.BASS_StreamFree(Stream);
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
                return;
            if (Sound_List.SelectedIndex == -1)
                return;
            else if (!File.Exists(Voice_Set.Special_Path + "/Wwise/Temp_02.ogg"))
            {
                Message_Feed_Out("サウンドファイルが変換されませんでした。");
                return;
            }
            if (SelectIndex == Sound_List.SelectedIndex)
                Bass.BASS_ChannelPlay(Stream, false);
            else
            {
                Bass.BASS_ChannelStop(Stream);
                Location_S.Value = 0;
                Bass.BASS_StreamFree(Stream);
                int StreamHandle = Bass.BASS_StreamCreateFile(Voice_Set.Special_Path + "/Wwise/Temp_02.wav", 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_LOOP);
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
                Speed_T.Text = "速度:" + (SetFirstFreq + Math.Floor(Speed_S.Value));
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
                    Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)Volume_S.Value / 100);
            }
        }
        private void Minus_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            long position = Bass.BASS_ChannelGetPosition(Stream);
            if (Bass.BASS_ChannelBytes2Seconds(Stream, position) > 5)
                Music_Pos_Change(Bass.BASS_ChannelBytes2Seconds(Stream, position) - 5, true);
            else
                Music_Pos_Change(0, true);
            long position2 = Bass.BASS_ChannelGetPosition(Stream);
            Location_S.Value = Bass.BASS_ChannelBytes2Seconds(Stream, position2);
        }
        private void Plus_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            long position = Bass.BASS_ChannelGetPosition(Stream);
            if (Bass.BASS_ChannelBytes2Seconds(Stream, position) + 5 > Location_S.Maximum)
                Music_Pos_Change(Location_S.Maximum, true);
            else
                Music_Pos_Change(Bass.BASS_ChannelBytes2Seconds(Stream, position) + 5, true);
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
                Sound_List.ScrollIntoView(Sound_List.Items[Number]);
            else
                Message_Feed_Out("リスト内に存在しませんでした。");
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
                            Sub_Code.WEM_To_File(File_Now, Path.GetDirectoryName(File_Now) + "/" + Path.GetFileNameWithoutExtension(File_Now) + ".wav", "wav", false);
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
            BNK_PCK_File = "";
            Bass.BASS_ChannelStop(Stream);
            Bass.BASS_StreamFree(Stream);
            File.Delete(Voice_Set.Special_Path + "/Wwise/Temp_02.ogg");
        }
        private void Change_Volume_Help_B_Click(object sender, RoutedEventArgs e)
        {
            string Message_01 = "----使用方法----\n";
            string Message_02 = "ステップ1:'サウンドファイルを開く'を押し、音量を変更させたい.bnkまたは.pckファイルを選択します。\n";
            string Message_03 = "ステップ2:'～音量調整～'の下の音量バーを変更します。この際、'すべて均一にする'のチェックの有無によって処理内容が異なります。\n";
            string Message_04 = "ステップ3:'適応'ボタンを押し、保存先を指定したのち、処理が完了するまで待ちます。";
            System.Windows.MessageBox.Show(Message_01 + Message_02 + Message_03 + Message_04);
        }
        private void All_Volume_Sync_C_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Is_Sync_Checked)
                All_Volume_Sync_C.Source = Sub_Code.Check_02;
            else
                All_Volume_Sync_C.Source = Sub_Code.Check_04;
            Is_Sync_Checked = !Is_Sync_Checked;
            Volume_Setting_Change();
        }
        private void All_Volume_Sync_C_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Is_Sync_Checked)
                All_Volume_Sync_C.Source = Sub_Code.Check_04;
            else
                All_Volume_Sync_C.Source = Sub_Code.Check_02;
        }
        private void All_Volume_Sync_C_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Is_Sync_Checked)
                All_Volume_Sync_C.Source = Sub_Code.Check_03;
            else
                All_Volume_Sync_C.Source = Sub_Code.Check_01;
        }
        //よくよく考えたら上書き保存できませんでした(笑)
        private void OverWrite_Mode_C_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Is_Sync_Checked)
                OverWrite_Mode_C.Source = Sub_Code.Check_02;
            else
                OverWrite_Mode_C.Source = Sub_Code.Check_04;
            IsOverWrite_Checked = !IsOverWrite_Checked;
        }
        private void OverWrite_Mode_C_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Is_Sync_Checked)
                OverWrite_Mode_C.Source = Sub_Code.Check_04;
            else
                OverWrite_Mode_C.Source = Sub_Code.Check_02;
        }
        private void OverWrite_Mode_C_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Is_Sync_Checked)
                OverWrite_Mode_C.Source = Sub_Code.Check_03;
            else
                OverWrite_Mode_C.Source = Sub_Code.Check_01;
        }
        private void All_Volume_Sync_Help_B_Click(object sender, RoutedEventArgs e)
        {
            string Message_01 = "単位はdbで、ソフト内で音声を作成するときは100dbに設定されます。(WoTBの設定で勝手に低くなるためこの値にしています。)\n";
            string Message_02 = "チェックなし:元サウンドの音量から値を足した数値が音量になります。\n";
            string Message_03 = "例:元サウンドの音量が90で、値を-5にすると適応後は85になります。\n";
            string Message_04 = "チェックあり:元サウンドの音量関係なく、すべてのサウンドの音量を均一に変更します。\n";
            string Message_05 = "例:値を85にして適応すると、.bnk内のすべてのサウンドの音量が85になります。";
            System.Windows.MessageBox.Show(Message_01 + Message_02 + Message_03 + Message_04 + Message_05);
        }
        void Volume_Setting_Change()
        {
            Change_Volume_T.Text = "音量:±0";
            if (Is_Sync_Checked)
            {
                Change_Volume_S.Maximum = 100;
                Change_Volume_S.Value = Volume_Sync_Mode;
                Change_Volume_S.Minimum = 70;
            }
            else
            {
                Change_Volume_S.Maximum = 11;
                Change_Volume_S.Minimum = -19;
                Change_Volume_S.Value = Volume_Async_Mode;
            }
        }
        private void Change_Volume_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!Is_Sync_Checked)
            {
                double Value = Math.Round(Change_Volume_S.Value, 1, MidpointRounding.AwayFromZero);
                if (Value == 0)
                    Change_Volume_T.Text = "音量:±0";
                else if (Value > 0)
                    Change_Volume_T.Text = "音量:＋" + Value;
                else
                    Change_Volume_T.Text = "音量:" + Value;
            }
            else
                Change_Volume_T.Text = "音量:" + (int)Change_Volume_S.Value;
        }
        private async void Change_Volume_B_Click(object sender, RoutedEventArgs e)
        {
            if (Sound_List.Items.Count == 0)
            {
                Message_Feed_Out(".bnkまたは.pckファイルが選択されていません。");
                return;
            }
            string Message_01 = ".bnkまたは.pck内のサウンドの長さや量によって処理時間が変わります。\n";
            string Message_02 = "処理中はフリーズしたように見えますが、実際はちゃんと処理されているのでご安心ください。";
            System.Windows.MessageBox.Show(Message_01 + Message_02);
            string To_File_Path = BNK_PCK_File;
            string Ex = "";
            if (IsPCKFile)
                Ex = ".pck";
            else
                Ex = ".bnk";
            if (!IsOverWrite_Checked)
            {
                SaveFileDialog ofd = new SaveFileDialog()
                {
                    Title = "保存先をしていてください。",
                    Filter = Ex + "ファイル|*" + Ex,
                    AddExtension = true
                };
                if (ofd.ShowDialog() == DialogResult.OK)
                    To_File_Path = ofd.FileName;
                else
                {
                    ofd.Dispose();
                    Message_Feed_Out("操作をキャンセルしました。");
                    return;
                }
                ofd.Dispose();
            }
            if (!Sub_Code.CanDirectoryAccess(Path.GetDirectoryName(To_File_Path)))
            {
                Message_Feed_Out("フォルダにアクセスできませんでした。別の場所を選択してください。");
                return;
            }
            try
            {
                IsMessageShowing = false;
                Message_T.Text = Ex + "ファイルからサウンドを抽出しています...";
                await Task.Delay(50);
                if (IsPCKFile)
                    Wwise_Pck.Wwise_Extract_To_OGG_OR_WAV_Directory(Voice_Set.Special_Path + "\\Other\\Extract_To_WAV", true);
                else
                    Wwise_Bnk.Wwise_Extract_To_OGG_OR_WAV_Directory(Voice_Set.Special_Path + "\\Other\\Extract_To_WAV", true);
                await Multithread.Conert_OGG_To_Wav(Directory.GetFiles(Voice_Set.Special_Path + "\\Other\\Extract_To_WAV", "*.ogg", SearchOption.TopDirectoryOnly), true);
                Message_T.Text = "音量を調整しています...";
                await Task.Delay(50);
                string[] WAV_Files = Directory.GetFiles(Voice_Set.Special_Path + "\\Other\\Extract_To_WAV", "*.wav", SearchOption.TopDirectoryOnly);
                if (!Is_Sync_Checked)
                {
                    List<double> Volume_Gains = new List<double>();
                    foreach (string File_Now in WAV_Files)
                        Volume_Gains.Add(Sub_Code.Get_WAV_Gain(File_Now));
                    await Multithread.WAV_Gain(WAV_Files, Volume_Gains.ToArray());
                }
                else
                    await Multithread.WAV_Gain(WAV_Files, Volume_Sync_Mode - 89);
                Message_T.Text = ".wavを.wemファイルに変換しています...";
                await Task.Delay(50);
                string Project_Dir = Voice_Set.Special_Path + "\\Other\\WEM_Create";
                if (File.Exists(Project_Dir + "/Actor-Mixer Hierarchy/Backup.tmp"))
                    File.Copy(Project_Dir + "/Actor-Mixer Hierarchy/Backup.tmp", Project_Dir + "/Actor-Mixer Hierarchy/Default Work Unit.wwu", true);
                else
                    File.Copy(Project_Dir + "/Actor-Mixer Hierarchy/Default Work Unit.wwu", Project_Dir + "/Actor-Mixer Hierarchy/Backup.tmp", true);
                Wwise_Project_Create Wwise_Project = new Wwise_Project_Create(Project_Dir);
                foreach (string File_Now in WAV_Files)
                    Wwise_Project.Add_Sound("778220130", File_Now, "SFX", false, null, "", 0, false, false);
                Wwise_Project.Save();
                Wwise_Project.Project_Build("WEM_Create", Voice_Set.Special_Path + "\\Other\\Temp.bnk");
                File.Delete(Voice_Set.Special_Path + "\\Other\\Temp.bnk");
                string Dir = Voice_Set.Special_Path + "\\Other\\WEM_Create\\.cache\\Windows\\SFX";
                foreach (string Name in Directory.GetFiles(Dir, "*.wem", SearchOption.TopDirectoryOnly))
                {
                    string Name_Only = Path.GetFileNameWithoutExtension(Name);
                    File.Move(Name, Path.GetDirectoryName(Name) + "\\" + Name_Only.Substring(0, Name_Only.IndexOf('_')) + ".wem");
                }
                Message_T.Text = ".bnk内の.wemと交換しています...";
                await Task.Delay(50);
                int Number1 = Sub_Code.r.Next(0, 10000);
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Other/Replace_WEM_" + Number1 + ".bat");
                stw.WriteLine("chcp 65001");
                stw.Write("\"" + Voice_Set.Special_Path + "/Wwise/wwiseutil.exe\" -replace -f \"" + BNK_PCK_File + "\" -t " + Dir + " -o \"" + To_File_Path + "\"");
                stw.Close();
                ProcessStartInfo processStartInfo1 = new ProcessStartInfo
                {
                    FileName = Voice_Set.Special_Path + "/Other/Replace_WEM_" + Number1 + ".bat",
                    WorkingDirectory = Voice_Set.Special_Path + "\\Other",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process p = Process.Start(processStartInfo1);
                p.WaitForExit();
                File.Delete(Voice_Set.Special_Path + "/Other/Replace_WEM_" + Number1 + ".bat");
                Wwise_Project.Clear();
                if (File.Exists(Project_Dir + "/Actor-Mixer Hierarchy/Backup.tmp"))
                    File.Copy(Project_Dir + "/Actor-Mixer Hierarchy/Backup.tmp", Project_Dir + "/Actor-Mixer Hierarchy/Default Work Unit.wwu", true);
                Directory.Delete(Voice_Set.Special_Path + "\\Other\\Extract_To_WAV", true);
                foreach (string Name in Directory.GetFiles(Dir, "*.wem", SearchOption.TopDirectoryOnly))
                    Sub_Code.File_Delete_V2(Name);
                Message_Feed_Out("完了しました。");
            }
            catch (Exception e1)
            {
                Sub_Code.Error_Log_Write(e1.Message);
                Message_Feed_Out("エラーが発生しました。詳しくはError_Log.txtを参照してください。");
            }
        }
    }
}