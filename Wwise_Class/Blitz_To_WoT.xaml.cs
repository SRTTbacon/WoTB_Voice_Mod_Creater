using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using WK.Libraries.BetterFolderBrowserNS;
using WoTB_Voice_Mod_Creater.Class;
using WoTB_Voice_Mod_Creater.FMOD;

namespace WoTB_Voice_Mod_Creater.Wwise_Class
{
    public partial class Blitz_To_WoT : System.Windows.Controls.UserControl
    {
        int Stream;
        int Max_Stream_Count = 0;
        int Now_Stream_Count = 0;
        bool IsMessageShowing = false;
        bool IsClosing = false;
        bool IsBusy = false;
        bool IsPaused = false;
        bool IsLocationChanging = false;
        bool IsModeChanging = false;
        Random r = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
        List<List<string>> BNK_FSB_Voices = new List<List<string>>();
        List<List<bool>> BNK_FSB_Enable = new List<List<bool>>();
        public Blitz_To_WoT()
        {
            InitializeComponent();
            Location_S.AddHandler(MouseDownEvent, new MouseButtonEventHandler(Location_MouseDown), true);
            Location_S.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Location_MouseUp), true);
            Volume_S.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Volume_MouseUp), true);
            Voices_L.Items.Add("音声ファイルが選択されていません。");
            Location_S.Maximum = 0;
        }
        async void Position_Change()
        {
            while (Visibility == Visibility.Visible)
            {
                if (!IsBusy)
                {
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
                    else if (Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_STOPPED && !IsLocationChanging && !IsPaused)
                    {
                        Location_S.Value = 0;
                        Location_T.Text = "00:00";
                    }
                }
                await Task.Delay(1000 / 30);
            }
        }
        public async void Window_Show()
        {
            Opacity = 0;
            Visibility = Visibility.Visible;
            if (File.Exists(Voice_Set.Special_Path + "/Configs/Blitz_To_WoT.conf"))
            {
                try
                {
                    using (var eifs = new FileStream(Voice_Set.Special_Path + "/Configs/Blitz_To_WoT.conf", FileMode.Open, FileAccess.Read))
                    {
                        using (var eofs = new FileStream(Voice_Set.Special_Path + "/Configs/Blitz_To_WoT.tmp", FileMode.Create, FileAccess.Write))
                        {
                            FileEncode.FileEncryptor.Decrypt(eifs, eofs, "Blitz_To_WoT_Configs_Save");
                        }
                    }
                    StreamReader str = new StreamReader(Voice_Set.Special_Path + "/Configs/Blitz_To_WoT.tmp");
                    Volume_S.Value = double.Parse(str.ReadLine());
                    Volume_Set_C.IsChecked = bool.Parse(str.ReadLine());
                    str.Close();
                    File.Delete(Voice_Set.Special_Path + "/Configs/Blitz_To_WoT.tmp");
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show("設定を読み込めませんでした。。\nエラー回避のため設定は削除されます。");
                    File.Delete(Voice_Set.Special_Path + "/Configs/Blitz_To_WoT.conf");
                    Sub_Code.Error_Log_Write(e.Message);
                }
            }
            Position_Change();
            while (Opacity < 1 && !IsClosing)
            {
                Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
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
                {
                    Message_T.Opacity -= 0.025;
                }
                await Task.Delay(1000 / 60);
            }
            IsMessageShowing = false;
            Message_T.Text = "";
            Message_T.Opacity = 1;
        }
        private async void Back_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!IsClosing && !IsBusy)
            {
                IsClosing = true;
                IsPaused = true;
                float Volume_Now = 1f;
                Bass.BASS_ChannelGetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, ref Volume_Now);
                float Volume_Minus = Volume_Now / 30f;
                while (Opacity > 0)
                {
                    Opacity -= Sub_Code.Window_Feed_Time;
                    Volume_Now -= Volume_Minus;
                    if (Volume_Now < 0f)
                    {
                        Volume_Now = 0f;
                    }
                    Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, Volume_Now);
                    await Task.Delay(1000 / 60);
                }
                Bass.BASS_ChannelStop(Stream);
                Bass.BASS_StreamFree(Stream);
                Location_S.Value = 0;
                Location_S.Maximum = 0;
                Voices_L.SelectedIndex = -1;
                Visibility = Visibility.Hidden;
                IsClosing = false;
            }
        }
        private void Clear_B_Click(object sender, RoutedEventArgs e)
        {
            BNK_FSB_Clear();
        }
        void BNK_FSB_Clear(bool IsMessageShow = true)
        {
            if (IsClosing || IsBusy)
                return;
            if (IsMessageShow)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("内容をクリアしますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
                if (result == MessageBoxResult.Yes)
                {
                    Bass.BASS_ChannelStop(Stream);
                    Bass.BASS_StreamFree(Stream);
                    Voices_L.Items.Clear();
                    Content_L.Items.Clear();
                    BNK_FSB_Enable.Clear();
                    BNK_FSB_Voices.Clear();
                    Voices_L.Items.Add("音声ファイルが選択されていません。");
                    BNK_FSB_Voices.Clear();
                    File_Name_T.Text = "";
                    Location_S.Value = 0;
                    Location_S.Maximum = 0;
                    Max_Stream_Count = 0;
                    Now_Stream_Count = 0;
                    Location_T.Text = "00:00";
                    try
                    {
                        if (Directory.Exists(Voice_Set.Special_Path + "/Wwise/BNK_WAV_WoT"))
                        {
                            Directory.Delete(Voice_Set.Special_Path + "/Wwise/BNK_WAV_WoT", true);
                        }
                    }
                    catch (Exception e1)
                    {
                        Sub_Code.Error_Log_Write(e1.Message);
                    }
                }
            }
            else
            {
                Bass.BASS_ChannelStop(Stream);
                Bass.BASS_StreamFree(Stream);
                Voices_L.Items.Clear();
                Content_L.Items.Clear();
                BNK_FSB_Enable.Clear();
                BNK_FSB_Voices.Clear();
                Voices_L.Items.Add("音声ファイルが選択されていません。");
                BNK_FSB_Voices.Clear();
                File_Name_T.Text = "";
                Location_S.Value = 0;
                Location_S.Maximum = 0;
                Max_Stream_Count = 0;
                Now_Stream_Count = 0;
                Location_T.Text = "00:00";
                try
                {
                    if (Directory.Exists(Voice_Set.Special_Path + "/Wwise/BNK_WAV_WoT"))
                    {
                        Directory.Delete(Voice_Set.Special_Path + "/Wwise/BNK_WAV_WoT", true);
                    }
                }
                catch (Exception e1)
                {
                    Sub_Code.Error_Log_Write(e1.Message);
                }
            }
        }
        void Load_Sound(string File_Name = "")
        {
            Location_S.Value = 0;
            Location_T.Text = "00:00";
            int StreamHandle;
            StreamHandle = Bass.BASS_StreamCreateFile(File_Name, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_LOOP);
            Stream = BassFx.BASS_FX_TempoCreate(StreamHandle, BASSFlag.BASS_FX_FREESOURCE);
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)Volume_S.Value / 100);
            Bass.BASS_ChannelSetDevice(Stream, Video_Mode.Sound_Device);
            Location_S.Maximum = Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetLength(Stream, BASSMode.BASS_POS_BYTES));
            Bass.BASS_ChannelPlay(Stream, true);
        }
        private void Volume_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Volume_T.Text = "音量:" + (int)Volume_S.Value;
            if (!IsPaused)
            {
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)Volume_S.Value / 100);
            }
        }
        void Location_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IsLocationChanging = true;
            IsPaused = true;
            Bass.BASS_ChannelPause(Stream);
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, 0f);
        }
        async void Location_MouseUp(object sender, MouseButtonEventArgs e)
        {
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
        void Music_Pos_Change(double Pos, bool IsBassPosChange)
        {
            if (IsClosing || IsBusy)
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
        private void Play_B_Click(object sender, RoutedEventArgs e)
        {
            if (Voices_L.SelectedIndex == -1 || BNK_FSB_Voices.Count == 0 || IsBusy || IsClosing)
            {
                return;
            }
            Bass.BASS_ChannelPlay(Stream, false);
        }
        private void Pause_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
            {
                return;
            }
            Bass.BASS_ChannelPause(Stream);
        }
        void Volume_MouseUp(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Configs_Save();
        }
        void Configs_Save()
        {
            try
            {
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Configs/Blitz_To_WoT.tmp");
                stw.WriteLine(Volume_S.Value);
                stw.WriteLine(Volume_Set_C.IsChecked.Value);
                stw.Close();
                using (var eifs = new FileStream(Voice_Set.Special_Path + "/Configs/Blitz_To_WoT.tmp", FileMode.Open, FileAccess.Read))
                {
                    using (var eofs = new FileStream(Voice_Set.Special_Path + "/Configs/Blitz_To_WoT.conf", FileMode.Create, FileAccess.Write))
                    {
                        FileEncode.FileEncryptor.Encrypt(eifs, eofs, "Blitz_To_WoT_Configs_Save");
                    }
                }
                File.Delete(Voice_Set.Special_Path + "/Configs/Blitz_To_WoT.tmp");
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                Message_Feed_Out("設定を保存できませんでした。");
            }
        }
        private void Voices_L_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Voices_L.SelectedIndex != -1)
            {
                Bass.BASS_ChannelStop(Stream);
                Bass.BASS_StreamFree(Stream);
                Voices_L.SelectedIndex = -1;
            }
        }
        private async void Open_File_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
            {
                return;
            }
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Title = "WoTBの音声Modを選択してください。",
                Multiselect = false,
                Filter = "WoTB音声(*.fsb;*.bnk)|*.fsb;*.bnk"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                IsBusy = true;
                try
                {
                    Message_T.Text = ".bnkファイルを解析しています...";
                    await Task.Delay(50);
                    string To_Dir = Voice_Set.Special_Path + "/Wwise/BNK_WAV_WoT";
                    try
                    {
                        if (Directory.Exists(To_Dir))
                        {
                            Directory.Delete(To_Dir, true);
                        }
                    }
                    catch (Exception e1)
                    {
                        Sub_Code.Error_Log_Write(e1.Message);
                    }
                    for (int Number = 0; Number < 34; Number++)
                    {
                        BNK_FSB_Enable.Add(new List<bool>());
                    }
                    if (Path.GetExtension(ofd.FileName) == ".bnk")
                    {
                        Wwise_Class.BNK_Parse p = new Wwise_Class.BNK_Parse(ofd.FileName);
                        if (!p.IsVoiceFile(true))
                        {
                            Message_Feed_Out("選択されたbnkファイルは音声データではありません。");
                            IsBusy = false;
                            return;
                        }
                        BNK_FSB_Voices = p.Get_Voices(true);
                        List<string> Need_Files = new List<string>();
                        foreach (List<string> Types in BNK_FSB_Voices)
                        {
                            foreach (string File_Now in Types)
                            {
                                Need_Files.Add(File_Now);
                            }
                        }
                        if (Need_Files.Count == 0)
                        {
                            Message_T.Text = "移植できるファイルが見つからなかったため、特殊な方法で解析しています...";
                            await Task.Delay(50);
                            p.IsSpecialBNKFileMode = true;
                            BNK_FSB_Voices = p.Get_Voices(true);
                            foreach (List<string> Types in BNK_FSB_Voices)
                            {
                                foreach (string File_Now in Types)
                                {
                                    Need_Files.Add(File_Now);
                                }
                            }
                            if (Need_Files.Count == 0)
                            {
                                p.Clear();
                                BNK_FSB_Voices.Clear();
                                Message_Feed_Out("移植できる音声が見つかりませんでした。");
                                IsBusy = false;
                                return;
                            }
                        }
                        p.Clear();
                        Voices_L.Items.Clear();
                        Content_L.Items.Clear();
                        Now_Stream_Count = 0;
                        for (int Number = 0; Number < 34; Number++)
                        {
                            Now_Stream_Count += BNK_FSB_Voices[Number].Count;
                            Voices_L.Items.Add(Voice_Set.Get_Voice_Type_Japanese_Name_V2(Number) + " : " + BNK_FSB_Voices[Number].Count + "個");
                        }
                        Message_T.Text = ".wavまたは.oggに変換しています...";
                        await Task.Delay(50);
                        Wwise_File_Extract_V2 Wwise_BNK = new Wwise_File_Extract_V2(ofd.FileName);
                        Max_Stream_Count = Wwise_BNK.Wwise_Get_Numbers();
                        Wwise_BNK.Wwise_Extract_To_WEM_Directory_V2(To_Dir);
                        Wwise_BNK.Bank_Clear();
                        Message_T.Text = "不要な音声ファイルを削除しています...";
                        await Task.Delay(50);
                        string[] All_Files = Directory.GetFiles(To_Dir, "*", SearchOption.TopDirectoryOnly);
                        foreach (string File_Now in All_Files)
                        {
                            if (!Need_Files.Contains(Path.GetFileNameWithoutExtension(File_Now)))
                            {
                                Sub_Code.File_Delete_V2(File_Now);
                            }
                        }
                        //効果音を削除(これ以外は取り除けない)
                        string[] Files = Directory.GetFiles(To_Dir, "*.wem", SearchOption.TopDirectoryOnly);
                        foreach (string File_Now in Files)
                        {
                            Sub_Code.File_Delete_V2(File_Now);
                        }
                        Message_T.Text = ".oggファイルを.wavファイルに変換しています...";
                        await Task.Delay(50);
                        await Multithread.Convert_To_Wav(Directory.GetFiles(To_Dir, "*.ogg", SearchOption.TopDirectoryOnly), To_Dir, true, true);
                        File_Name_T.Text = Path.GetFileName(ofd.FileName);
                        Message_Feed_Out(".bnkファイルをロードしました。SEが含まれている場合は無効化してください。");
                    }
                    else
                    {
                        Message_T.Text = ".fsbファイルを解析しています...";
                        await Task.Delay(50);
                        bool IsVoiceExist = false;
                        List<string> Voices = Fmod_Class.FSB_GetNames(ofd.FileName);
                        foreach (string File_Now in Voices)
                        {
                            string File_Now_01 = File_Now.Replace(" ", "");
                            if (File_Now_01.Contains("battle_01") || File_Now_01.Contains("battle_02") || File_Now_01.Contains("battle_03") || File_Now_01.Contains("start_battle_01"))
                            {
                                IsVoiceExist = true;
                                break;
                            }
                        }
                        if (!IsVoiceExist)
                        {
                            Message_Feed_Out("指定したFSBファイルは対応していません。");
                            IsBusy = false;
                            return;
                        }
                        Voices.Clear();
                        Message_T.Text = "FSBファイルからファイルを抽出しています...";
                        await Task.Delay(50);
                        Max_Stream_Count = Fmod_Class.FSB_GetLength(ofd.FileName);
                        Fmod_File_Extract_V2.FSB_Extract_To_Directory(ofd.FileName, To_Dir + "_TMP");
                        Message_T.Text = ".wavファイルを修正しています...";
                        await Task.Delay(50);
                        await Multithread.Convert_To_Wav(To_Dir + "_TMP", To_Dir, true, true, true);
                        Directory.Delete(To_Dir + "_TMP", true);
                        Message_T.Text = "ファイル名からリストに配置しています...";
                        await Task.Delay(50);
                        BNK_FSB_Voices = Voice_Set.Voice_BGM_Name_Change_From_FSB_To_Index(To_Dir);
                        Voices_L.Items.Clear();
                        Content_L.Items.Clear();
                        Now_Stream_Count = 0;
                        for (int Number = 0; Number < 34; Number++)
                        {
                            Now_Stream_Count += BNK_FSB_Voices[Number].Count;
                            Voices_L.Items.Add(Voice_Set.Get_Voice_Type_Japanese_Name_V2(Number) + " : " + BNK_FSB_Voices[Number].Count + "個");
                        }
                        Message_T.Text = "不要なファイルを削除しています...";
                        await Task.Delay(50);
                        foreach (string File_Name in Directory.GetFiles(To_Dir, "*", SearchOption.TopDirectoryOnly))
                        {
                            string Name_Only = Path.GetFileNameWithoutExtension(File_Name);
                            bool IsExsist = false;
                            for (int Number = 0; Number < BNK_FSB_Voices.Count; Number++)
                            {
                                foreach (string Name_Now in BNK_FSB_Voices[Number])
                                {
                                    if (Name_Only == Name_Now)
                                    {
                                        IsExsist = true;
                                        break;
                                    }
                                }
                                if (IsExsist)
                                {
                                    break;
                                }
                            }
                            if (!IsExsist)
                            {
                                Sub_Code.File_Delete_V2(File_Name);
                            }
                        }
                        File_Name_T.Text = Path.GetFileName(ofd.FileName);
                        Message_Feed_Out(".fsbファイルをロードしました。");
                    }
                    for (int Number_01 = 0; Number_01 < BNK_FSB_Voices.Count; Number_01++)
                    {
                        for (int Number_02 = 0; Number_02 < BNK_FSB_Voices[Number_01].Count; Number_02++)
                        {
                            BNK_FSB_Enable[Number_01].Add(true);
                        }
                    }
                }
                catch (Exception e1)
                {
                    IsBusy = false;
                    Sub_Code.Error_Log_Write(e1.Message);
                    BNK_FSB_Clear(false);
                    Message_Feed_Out("エラーが発生しました。Error_Log.txtを参照してください。");
                }
            }
            IsBusy = false;
        }
        //反映される音声の数を取得
        void Get_Available_Voice_Count()
        {
            Now_Stream_Count = 0;
            for (int Number = 0; Number < BNK_FSB_Voices.Count; Number++)
            {
                Now_Stream_Count += BNK_FSB_Voices[Number].Count;
            }
        }
        private void Volume_Set_C_Click(object sender, RoutedEventArgs e)
        {
            Configs_Save();
        }
        private void Details_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
            {
                return;
            }
            if (BNK_FSB_Voices.Count == 0)
            {
                Message_Feed_Out("音声ファイルが指定されていないため、表示できる情報がありません。");
                return;
            }
            Message_Feed_Out("ファイル内の音声ファイル数:" + Max_Stream_Count + "\n" + "移植後の音声数:" + Now_Stream_Count);
        }
        private void Volume_Set_Help_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
            {
                return;
            }
            string Message_01 = "・音量をWoT用に調整します。(MP3Gainで音量を100にします。)\n";
            string Message_02 = "・一度MP3形式に変換し、音量を調整してからWAV形式にしますので、通常より多くの時間が必要になります。";
            string Message_03 = "・変換中は、CPU使用率が高くなりますので、なるべく他の作業をしないことをおすすめします。";
            System.Windows.MessageBox.Show(Message_01 + Message_02 + Message_03);
        }
        private async void Start_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
            {
                return;
            }
            if (BNK_FSB_Voices.Count == 0)
            {
                Message_Feed_Out("音声ファイルが選択されていません。");
                return;
            }
            BetterFolderBrowser fbd = new BetterFolderBrowser()
            {
                Title = "保存先を選択してください。",
                Multiselect = false,
                RootFolder = Sub_Code.Get_OpenDirectory_Path()
            };
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                IsBusy = true;
                string To_Dir = Voice_Set.Special_Path + "/Wwise/BNK_WAV_WoT";
                string SetPath = fbd.SelectedFolder;
                Sub_Code.Set_Directory_Path(SetPath);
                if (!Sub_Code.CanDirectoryAccess(SetPath))
                {
                    Message_Feed_Out("指定したフォルダにアクセスできませんでした。");
                    IsBusy = false;
                    return;
                }
                try
                {
                    Message_T.Opacity = 1;
                    IsMessageShowing = false;
                    await Task.Delay(50);
                    if (Volume_Set_C.IsChecked.Value)
                    {
                        Message_T.Text = ".mp3に変換しています...";
                        await Task.Delay(50);
                        await Multithread.Convert_To_MP3(Directory.GetFiles(To_Dir, "*", SearchOption.TopDirectoryOnly), To_Dir, true);
                        Message_T.Text = "音量をWoT用に調整しています...";
                        await Task.Delay(50);
                        Sub_Code.MP3_Volume_Set(To_Dir);
                        Message_T.Text = ".wavに変換しています...";
                        await Task.Delay(50);
                        await Multithread.Convert_To_Wav(Directory.GetFiles(To_Dir, "*", SearchOption.TopDirectoryOnly), To_Dir, true, true);
                    }
                    Message_T.Text = "ファイルをコピーしています...";
                    if (Directory.Exists(To_Dir + "/Voices"))
                    {
                        Directory.Delete(To_Dir + "/Voices", true);
                    }
                    await Task.Delay(50);
                    string Log_01 = Sub_Code.Set_Voice_Type_Change_Name_By_Index(To_Dir, To_Dir + "/Voices", BNK_FSB_Voices, BNK_FSB_Enable);
                    if (Log_01 != "")
                    {
                        Message_Feed_Out("ファイルをコピーできませんでした。詳しくは\"Error_Log.txt\"を参照してください。");
                        Directory.Delete(To_Dir + "/Voices", true);
                        IsBusy = false;
                        return;
                    }
                    await BNK_Create_V2(To_Dir + "/Voices", SetPath);
                    Directory.Delete(To_Dir + "/Voices", true);
                    if (Sub_Code.WoT_Get_ModDirectory())
                    {
                        Message_T.Text = "ダイアログを表示しています...";
                        await Task.Delay(50);
                        MessageBoxResult result = System.Windows.MessageBox.Show("WoTのインストールフォルダの取得に成功しました。\n作成した音声ModをWoTに導入しますか？\n配置場所:" + Voice_Set.WoT_Mod_Path, "確認",
                            MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                        if (result == MessageBoxResult.Yes)
                        {
                            try
                            {
                                if (!Directory.Exists(Voice_Set.WoT_Mod_Path + "/audioww"))
                                {
                                    Directory.CreateDirectory(Voice_Set.WoT_Mod_Path + "/audioww");
                                }
                                if (File.Exists(Voice_Set.WoT_Mod_Path + "/audioww/voiceover.bnk"))
                                {
                                    File.Copy(Voice_Set.WoT_Mod_Path + "/audioww/voiceover.bnk", Voice_Set.WoT_Mod_Path + "/audioww/voiceover_bak.bnk", true);
                                }
                                File.Copy(SetPath + "/voiceover.bnk", Voice_Set.WoT_Mod_Path + "/audioww/voiceover.bnk", true);
                            }
                            catch (Exception e1)
                            {
                                Sub_Code.Error_Log_Write(e1.Message);
                                Message_Feed_Out("WoTにインストールできませんでした。Error_Log.txtを確認してください。");
                                IsBusy = false;
                                fbd.Dispose();
                                return;
                            }
                        }
                    }
                    Message_Feed_Out("完了しました。");
                }
                catch (Exception e1)
                {
                    Sub_Code.Error_Log_Write(e1.Message);
                    Message_Feed_Out("エラーが発生しました。Error_Log.txtを参照してください。");
                }
                IsBusy = false;
            }
            fbd.Dispose();
        }
        //Voice_Create.xaml.csから引用&若干の修正
        async Task BNK_Create_V2(string Voice_Dir, string Save_Dir)
        {
            if (!Directory.Exists(Voice_Dir))
            {
                return;
            }
            Message_T.Text = "プロジェクトファイルを作成しています...";
            await Task.Delay(50);
            FileInfo fi = new FileInfo(Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.wwu");
            if (File.Exists(Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp") && fi.Length >= 200000)
            {
                File.Copy(Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp", Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.wwu", true);
            }
            if (!File.Exists(Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp"))
            {
                File.Copy(Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.wwu", Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp", true);
            }
            Wwise_Class.Wwise_Project_Create Wwise = new Wwise_Class.Wwise_Project_Create(Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod");
            Wwise.Sound_Add_Wwise(Voice_Dir, true);
            Wwise.Save();
            Message_T.Text = "voiceover.bnkを作成しています...";
            await Task.Delay(50);
            Wwise.Project_Build("voiceover", Save_Dir + "/voiceover.bnk", "WinHighRes");
            Wwise.Clear("Windows_HighRes");
            if (File.Exists(Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp"))
            {
                File.Copy(Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp", Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.wwu", true);
            }
        }
        private void Voice_Enable_B_Click(object sender, RoutedEventArgs e)
        {
            int SelectedIndex = Content_L.SelectedIndex;
            if (SelectedIndex == -1 || Voices_L.SelectedIndex == -1)
            {
                return;
            }
            IsModeChanging = true;
            BNK_FSB_Enable[Voices_L.SelectedIndex][SelectedIndex] = true;
            string Get_Name = Content_L.SelectedItem.ToString();
            Content_L.Items[SelectedIndex] = Get_Name.Substring(0, Get_Name.LastIndexOf('|') + 1) + "有効";
            Content_L.SelectedIndex = SelectedIndex;
        }
        private void Voice_Disable_B_Click(object sender, RoutedEventArgs e)
        {
            int SelectedIndex = Content_L.SelectedIndex;
            if (SelectedIndex == -1 || Voices_L.SelectedIndex == -1)
            {
                return;
            }
            IsModeChanging = true;
            BNK_FSB_Enable[Voices_L.SelectedIndex][SelectedIndex] = false;
            string Get_Name = Content_L.SelectedItem.ToString();
            Content_L.Items[SelectedIndex] = Get_Name.Substring(0, Get_Name.LastIndexOf('|') + 1) + "無効";
            Content_L.SelectedIndex = SelectedIndex;
        }
        private void Voices_L_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Content_L.Items.Clear();
            if (Voices_L.SelectedIndex == -1)
            {
                return;
            }
            foreach (string Name in BNK_FSB_Voices[Voices_L.SelectedIndex])
            {
                string Name_Temp = Name;
                if (Name_Temp.Length > 15)
                {
                    if (BNK_FSB_Enable[Voices_L.SelectedIndex][Content_L.Items.Count])
                    {
                        Name_Temp = Name_Temp.Substring(0, 16) + "...|有効";
                    }
                    else
                    {
                        Name_Temp = Name_Temp.Substring(0, 16) + "...|無効";
                    }
                }
                else
                {
                    if (BNK_FSB_Enable[Voices_L.SelectedIndex][Content_L.Items.Count])
                    {
                        Name_Temp += "|有効";
                    }
                    else
                    {
                        Name_Temp += "|無効";
                    }
                }
                Content_L.Items.Add(Name_Temp);
            }
        }
        private void Content_L_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (Content_L.SelectedIndex == -1)
            {
                return;
            }
            if (IsModeChanging)
            {
                IsModeChanging = false;
                return;
            }
            Bass.BASS_ChannelStop(Stream);
            Bass.BASS_StreamFree(Stream);
            int Voice_Count = BNK_FSB_Voices[Voices_L.SelectedIndex].Count;
            if (Voice_Count == 0)
            {
                Location_S.Value = 0;
                Location_S.Maximum = 0;
                Location_T.Text = "00:00";
                return;
            }
            string Get_Number = BNK_FSB_Voices[Voices_L.SelectedIndex][Content_L.SelectedIndex];
            if (!File.Exists(Sub_Code.File_Get_FileName_No_Extension(Voice_Set.Special_Path + "/Wwise/BNK_WAV_WoT/" + Get_Number)))
            {
                Message_Feed_Out("ファイルが見つかりませんでした。");
                return;
            }
            Load_Sound(Sub_Code.File_Get_FileName_No_Extension(Voice_Set.Special_Path + "/Wwise/BNK_WAV_WoT/" + Get_Number));
        }
        private void Help_B_Click(object sender, RoutedEventArgs e)
        {
            string Message_01 = "WoTBの音声ModをPC版WoT用に移植します。\n";
            string Message_02 = ".bnkファイルを指定した場合は、SEが含まれている可能性が高いため、各イベントを確認し、あれば無効化することをおススメします。\n";
            string Message_03 = "低ティア&数戦しかテストしていないため、正常に再生されないイベントがあるかもしれません。";
            System.Windows.MessageBox.Show(Message_01 + Message_02 + Message_03);
        }
    }
}