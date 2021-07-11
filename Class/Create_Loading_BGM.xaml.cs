using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using WK.Libraries.BetterFolderBrowserNS;
using WoTB_Voice_Mod_Creater.Wwise_Class;

namespace WoTB_Voice_Mod_Creater.Class
{
    //曲の開始位置と終了位置をメモリに保存(終了位置が0の場合は最後まで再生される)
    public class Music_Play_Time
    {
        public double Start_Time { get; set; }
        public double End_Time { get; set; }
        public Music_Play_Time(double Set_Start_Time, double Set_End_Time)
        {
            Start_Time = Set_Start_Time;
            End_Time = Set_End_Time;
        }
    }
    public partial class Create_Loading_BGM : UserControl
    {
        List<List<string>> Music_Type_Music = new List<List<string>>();
        List<List<Music_Play_Time>> Music_Play_Times = new List<List<Music_Play_Time>>();
        List<List<bool>> Music_Feed_In = new List<List<bool>>();
        int Stream;
        float SetFirstFreq = 44100f;
        bool IsClosing = false;
        bool IsBusy = false;
        bool IsMessageShowing = false;
        bool IsLocationChanging = false;
        bool IsPaused = false;
        bool IsEnded = false;
        Random r = new Random();
        SYNCPROC IsMusicEnd;
        public Create_Loading_BGM()
        {
            InitializeComponent();
            Position_S.AddHandler(MouseDownEvent, new MouseButtonEventHandler(Position_S_MouseDown), true);
            Position_S.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Position_S_MouseUp), true);
            Volume_S.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Volume_S_MouseUp), true);
            BGM_Type_L.Items.Add("ロード1:America_lakville | 0個");
            BGM_Type_L.Items.Add("ロード2:America_overlord | 0個");
            BGM_Type_L.Items.Add("ロード3:Chinese | 0個");
            BGM_Type_L.Items.Add("ロード4:Desert_airfield | 0個");
            BGM_Type_L.Items.Add("ロード5:Desert_sand_river | 0個");
            BGM_Type_L.Items.Add("ロード6:Europe_himmelsdorf | 0個");
            BGM_Type_L.Items.Add("ロード7:Europe_mannerheim | 0個");
            BGM_Type_L.Items.Add("ロード8:Europe_ruinberg | 0個");
            BGM_Type_L.Items.Add("ロード9:Japan | 0個");
            BGM_Type_L.Items.Add("ロード10:Russian_malinovka | 0個");
            BGM_Type_L.Items.Add("ロード11:Russian_prokhorovka | 0個");
            BGM_Type_L.Items.Add("リザルト:勝利-BGM | 0個");
            BGM_Type_L.Items.Add("リザルト:勝利-音声 | 0個");
            BGM_Type_L.Items.Add("リザルト:引き分け-BGM | 0個");
            BGM_Type_L.Items.Add("リザルト:引き分け-音声 | 0個");
            BGM_Type_L.Items.Add("リザルト:敗北-BGM | 0個");
            BGM_Type_L.Items.Add("リザルト:敗北-音声 | 0個");
            BGM_Type_L.Items.Add("優勢:味方 | 0個");
            BGM_Type_L.Items.Add("優勢:敵 | 0個");
            for (int Number = 0; Number < BGM_Type_L.Items.Count; Number++)
            {
                Music_Type_Music.Add(new List<string>());
                Music_Play_Times.Add(new List<Music_Play_Time>());
                Music_Feed_In.Add(new List<bool>());
            }
        }
        //画面を表示
        public async void Window_Show()
        {
            Opacity = 0;
            Visibility = Visibility.Visible;
            Position_Change();
            //設定をロード
            if (File.Exists(Voice_Set.Special_Path + "/Configs/Create_Loading_BGM.conf"))
            {
                try
                {
                    Sub_Code.File_Decrypt(Voice_Set.Special_Path + "/Configs/Create_Loading_BGM.conf", Voice_Set.Special_Path + "/Configs/Create_Loading_BGM.tmp", "Create_Loading_BGM_Configs_Save", false);
                    StreamReader str = new StreamReader(Voice_Set.Special_Path + "/Configs/Create_Loading_BGM.tmp");
                    Volume_S.Value = double.Parse(str.ReadLine());
                    str.Close();
                    File.Delete(Voice_Set.Special_Path + "/Configs/Create_Loading_BGM.tmp");
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show("設定を読み込めませんでした。。\nエラー回避のため設定は削除されます。");
                    File.Delete(Voice_Set.Special_Path + "/Configs/Create_Loading_BGM.conf");
                    Sub_Code.Error_Log_Write(e.Message);
                }
            }
            while (Opacity < 1 && !IsClosing)
            {
                Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
        }
        //曲の位置を更新
        async void Position_Change()
        {
            while (Visibility == Visibility.Visible)
            {
                if (!IsBusy)
                {
                    //曲が終わったら開始位置に戻る
                    if (IsEnded)
                    {
                        IsPaused = true;
                        if (BGM_Type_L.SelectedIndex != -1 && BGM_Music_L.SelectedIndex != -1)
                        {
                            Position_S.Value = Music_Play_Times[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex].Start_Time;
                            Music_Pos_Change(Position_S.Value, true);
                            Bass.BASS_ChannelPause(Stream);
                        }
                        IsEnded = false;
                    }
                    //曲が再生中だったら
                    if (Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_PLAYING && !IsLocationChanging)
                    {
                        long position = Bass.BASS_ChannelGetPosition(Stream);
                        Position_S.Value = Bass.BASS_ChannelBytes2Seconds(Stream, position);
                        if (BGM_Type_L.SelectedIndex != -1 && BGM_Music_L.SelectedIndex != -1)
                        {
                            double End_Time = Music_Play_Times[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex].End_Time;
                            if (End_Time != 0 && Position_S.Value >= End_Time)
                            {
                                Music_Pos_Change(Music_Play_Times[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex].Start_Time, true);
                                long position2 = Bass.BASS_ChannelGetPosition(Stream);
                                Position_S.Value = Bass.BASS_ChannelBytes2Seconds(Stream, position2);
                            }
                            else if (Position_S.Value < Music_Play_Times[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex].Start_Time)
                            {
                                Music_Pos_Change(Music_Play_Times[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex].Start_Time, true);
                                long position2 = Bass.BASS_ChannelGetPosition(Stream);
                                Position_S.Value = Bass.BASS_ChannelBytes2Seconds(Stream, position2);
                            }
                        }
                        //テキストボックスに曲の現在時間を表示
                        //例:00:05 : 01:21など
                        TimeSpan Time = TimeSpan.FromSeconds(Position_S.Value);
                        string Minutes = Time.Minutes.ToString();
                        string Seconds = Time.Seconds.ToString();
                        if (Time.Minutes < 10)
                            Minutes = "0" + Time.Minutes;
                        if (Time.Seconds < 10)
                            Seconds = "0" + Time.Seconds;
                        Position_T.Text = Minutes + ":" + Seconds;
                    }
                    else if (Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_STOPPED && !IsLocationChanging && !IsPaused)
                    {
                        Position_S.Value = 0;
                        Position_T.Text = "00:00";
                    }
                }
                await Task.Delay(1000 / 30);
            }
        }
        //曲が終わったら呼ばれる
        void EndSync(int handle, int channel, int data, IntPtr user)
        {
            if (!IsEnded)
                IsEnded = true;
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
            IsMessageShowing = false;
            Message_T.Text = "";
            Message_T.Opacity = 1;
        }
        //ウィンドウを閉じる
        private async void Back_B_Click(object sender, RoutedEventArgs e)
        {
            if (!IsClosing && !IsBusy)
            {
                IsClosing = true;
                float Volume_Now = 1f;
                Bass.BASS_ChannelGetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, ref Volume_Now);
                float Volume_Minus = Volume_Now / 20f;
                while (Opacity > 0)
                {
                    Volume_Now -= Volume_Minus;
                    if (Volume_Now < 0f)
                        Volume_Now = 0f;
                    Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, Volume_Now);
                    Opacity -= Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
                IsPaused = true;
                Bass.BASS_ChannelPause(Stream);
                IsClosing = false;
                Visibility = Visibility.Hidden;
            }
        }
        //ロードBGMの種類が変更された場合、右の欄に追加されている曲を表示
        private void BGM_Type_L_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BGM_Type_L.SelectedIndex == -1)
                return;
            Pause_Volume_Animation(true, 10f);
            BGM_Music_L.Items.Clear();
            foreach (string Name in Music_Type_Music[BGM_Type_L.SelectedIndex])
            {
                BGM_Music_L.Items.Add(Path.GetFileName(Name));
            }
        }
        //左の欄のBGM数を更新
        void Update_Music_Type_List()
        {
            int SelectedIndex = BGM_Type_L.SelectedIndex;
            for (int Number = 0; Number < BGM_Type_L.Items.Count; Number++)
            {
                string Name = BGM_Type_L.Items[Number].ToString();
                Name = Name.Substring(0, Name.IndexOf('|') + 2);
                BGM_Type_L.Items[Number] = Name + Music_Type_Music[Number].Count + "個";
            }
            BGM_Type_L.SelectedIndex = SelectedIndex;
        }
        //セーブ
        private void Save_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            int Music_Count = 0;
            for (int Number = 0; Number < Music_Type_Music.Count; Number++)
            {
                Music_Count += Music_Type_Music[Number].Count;
            }
            if (Music_Count == 0)
            {
                Message_Feed_Out("セーブする際、少なくとも1つはBGMを追加する必要があります。");
                return;
            }
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog()
            {
                Title = "セーブファイルの保存先を選択してください。",
                Filter = "セーブファイル(*.wms)|*.wms",
                InitialDirectory = Directory.GetCurrentDirectory()
            };
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!Sub_Code.CanDirectoryAccess(Path.GetDirectoryName(sfd.FileName)))
                {
                    Message_Feed_Out("指定したフォルダにアクセスできません。別の場所を選択してください。");
                    sfd.Dispose();
                    return;
                }
                try
                {
                    StreamWriter stw = File.CreateText(sfd.FileName + ".tmp");
                    stw.WriteLine("V1.4");
                    stw.WriteLine(Volume_WoTB_S.Value);
                    for (int Number = 0; Number < Music_Type_Music.Count; Number++)
                    {
                        for (int Number_01 = 0; Number_01 < Music_Type_Music[Number].Count; Number_01++)
                        {
                            stw.WriteLine(Number + "|" + Music_Type_Music[Number][Number_01] + "|" +
                                Music_Play_Times[Number][Number_01].Start_Time + "～" + Music_Play_Times[Number][Number_01].End_Time + "|" + Music_Feed_In[Number][Number_01]);
                        }
                    }
                    stw.Close();
                    stw.Dispose();
                    Sub_Code.File_Encrypt(sfd.FileName + ".tmp", sfd.FileName, "SRTTbacon_WoTB_Loading_Music_Mode", true);
                    Message_Feed_Out("セーブしました。");
                }
                catch (Exception e1)
                {
                    Sub_Code.Error_Log_Write(e1.Message);
                    Message_Feed_Out("エラー:指定したファイル場所は使用できません。");
                }
            }
            sfd.Dispose();
        }
        //ロード
        private void Load_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog()
            {
                Title = "セーブファイルを選択してください。",
                Filter = "セーブファイル(*.wms)|*.wms",
                InitialDirectory = Directory.GetCurrentDirectory()
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                Load_From_File(ofd.FileName);
            ofd.Dispose();
        }
        public async void Load_From_File(string WMS_File)
        {
            try
            {
                IsPaused = true;
                float Volume_Now = 1f;
                Bass.BASS_ChannelGetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, ref Volume_Now);
                float Volume_Minus = Volume_Now / 10f;
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
                    Bass.BASS_ChannelStop(Stream);
                    Bass.BASS_StreamFree(Stream);
                    Position_S.Value = 0;
                    Position_S.Maximum = 0;
                    Position_T.Text = "00:00";
                }
                Sub_Code.File_Decrypt(WMS_File, WMS_File + ".tmp", "SRTTbacon_WoTB_Loading_Music_Mode", false);
                for (int Number = 0; Number < Music_Type_Music.Count; Number++)
                {
                    Music_Type_Music[Number].Clear();
                }
                for (int Number = 0; Number < Music_Play_Times.Count; Number++)
                {
                    Music_Play_Times[Number].Clear();
                }
                Play_Time_T.Text = "再生時間:0～0";
                BGM_Music_L.Items.Clear();
                StreamReader str = new StreamReader(WMS_File + ".tmp");
                string line;
                bool IsOneLine = false;
                bool IsVersion_Upgrade_Mode = false;
                while ((line = str.ReadLine()) != null)
                {
                    if (!IsOneLine)
                    {
                        if (line == "V1.4")
                            IsVersion_Upgrade_Mode = true;
                        double Volume = 75;
                        if (IsVersion_Upgrade_Mode)
                        {
                            if (double.TryParse(str.ReadLine(), out Volume))
                                Volume_WoTB_S.Value = Volume;
                        }
                        else
                        {
                            if (double.TryParse(line, out Volume))
                                Volume_WoTB_S.Value = Volume;
                        }
                        IsOneLine = true;
                        continue;
                    }
                    string[] Line_Split = line.Split('|');
                    int Index = int.Parse(Line_Split[0]);
                    string FilePath = Line_Split[1];
                    string Play_Time_Only = Line_Split[2];
                    double Start_Time = double.Parse(Play_Time_Only.Substring(0, Play_Time_Only.IndexOf('～')));
                    double End_Time = double.Parse(Play_Time_Only.Substring(Play_Time_Only.IndexOf('～') + 1));
                    bool IsFeed_In_Mode = bool.Parse(Line_Split[3]);
                    if (!IsVersion_Upgrade_Mode && Index == 12)
                    {
                        Music_Type_Music[13].Add(FilePath);
                        Music_Play_Times[13].Add(new Music_Play_Time(Start_Time, End_Time));
                        Music_Feed_In[13].Add(IsFeed_In_Mode);
                    }
                    else if (!IsVersion_Upgrade_Mode && Index == 13)
                    {
                        Music_Type_Music[15].Add(FilePath);
                        Music_Play_Times[15].Add(new Music_Play_Time(Start_Time, End_Time));
                        Music_Feed_In[15].Add(IsFeed_In_Mode);
                    }
                    else if (!IsVersion_Upgrade_Mode && Index == 14)
                    {
                        Music_Type_Music[17].Add(FilePath);
                        Music_Play_Times[17].Add(new Music_Play_Time(Start_Time, End_Time));
                        Music_Feed_In[17].Add(IsFeed_In_Mode);
                    }
                    else if (!IsVersion_Upgrade_Mode && Index == 15)
                    {
                        Music_Type_Music[18].Add(FilePath);
                        Music_Play_Times[18].Add(new Music_Play_Time(Start_Time, End_Time));
                        Music_Feed_In[18].Add(IsFeed_In_Mode);
                    }
                    else
                    {
                        Music_Type_Music[Index].Add(FilePath);
                        Music_Play_Times[Index].Add(new Music_Play_Time(Start_Time, End_Time));
                        Music_Feed_In[Index].Add(IsFeed_In_Mode);
                    }
                }
                str.Close();
                str.Dispose();
                File.Delete(WMS_File + ".tmp");
                Feed_In_C.IsChecked = false;
                Update_Music_Type_List();
                Message_Feed_Out("ロードしました。");
            }
            catch (Exception e1)
            {
                Sub_Code.Error_Log_Write(e1.Message);
                Message_Feed_Out("エラー:ファイルを読み取れませんでした。");
            }
        }
        //再生中の曲を変更
        private void BGM_Music_L_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BGM_Music_L.SelectedIndex == -1)
                return;
            Bass.BASS_ChannelStop(Stream);
            Bass.BASS_StreamFree(Stream);
            Position_S.Value = 0;
            Position_T.Text = "00:00";
            if (!File.Exists(Music_Type_Music[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex]))
            {
                Message_Feed_Out("ファイルが存在しませんでした。");
                return;
            }
            int StreamHandle = Bass.BASS_StreamCreateFile(Music_Type_Music[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex], 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_LOOP);
            Stream = BassFx.BASS_FX_TempoCreate(StreamHandle, BASSFlag.BASS_FX_FREESOURCE);
            IsMusicEnd = new SYNCPROC(EndSync);
            Bass.BASS_ChannelGetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, ref SetFirstFreq);
            Bass.BASS_ChannelSetSync(Stream, BASSSync.BASS_SYNC_END | BASSSync.BASS_SYNC_MIXTIME, 0, IsMusicEnd, IntPtr.Zero);
            Bass.BASS_ChannelSetDevice(Stream, Video_Mode.Sound_Device);
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)Volume_S.Value / 100);
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, SetFirstFreq * (float)(Speed_S.Value / 50));
            Position_S.Maximum = Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetLength(Stream, BASSMode.BASS_POS_BYTES));
            double End_Time = Music_Play_Times[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex].End_Time;
            if (End_Time == 0)
                End_Time = Position_S.Maximum;
            Play_Time_T.Text = "再生時間:" + (int)Music_Play_Times[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex].Start_Time + "～" + (int)End_Time;
            Feed_In_C.IsChecked = Music_Feed_In[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex];
            IsPaused = true;
        }
        //音量を変更(ソフト内用)
        private void Volume_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Volume_T.Text = "音量(ソフト内):" + (int)Volume_S.Value;
            if (!IsPaused)
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)Volume_S.Value / 100);
        }
        //音量を変更(WoTB用)
        private void Volume_WoTB_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Volume_WoTB_T.Text = "音量(WoTB内):" + (int)Volume_WoTB_S.Value;
        }
        //再生速度を変更
        private void Speed_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Speed_T.Text = "速度:" + (int)Speed_S.Value;
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, SetFirstFreq * (float)(Speed_S.Value / 50));
        }
        //再生
        private void Play_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsPaused && !IsBusy)
                Play_Volume_Animation();
        }
        //一時停止
        private void Stop_B_Click(object sender, RoutedEventArgs e)
        {
            if (!IsPaused)
                Pause_Volume_Animation(false);
        }
        //フェードインしながら再生
        //引数:フェードインのかかる時間
        async void Play_Volume_Animation(float Feed_Time = 15f)
        {
            IsPaused = false;
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
        //フェードアウトしながら一時停止または停止
        public async void Pause_Volume_Animation(bool IsStop, float Feed_Time = 15f)
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
                    Position_S.Value = 0;
                    Position_S.Maximum = 0;
                    Position_T.Text = "00:00";
                }
                else
                    Bass.BASS_ChannelPause(Stream);
            }
        }
        //-5秒
        private void Minus_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
                return;
            long position = Bass.BASS_ChannelGetPosition(Stream);
            double Time_Temp = Bass.BASS_ChannelBytes2Seconds(Stream, position);
            if (Bass.BASS_ChannelBytes2Seconds(Stream, position) > 5)
                Music_Pos_Change(Bass.BASS_ChannelBytes2Seconds(Stream, position) - 5, true);
            else
                Music_Pos_Change(0, true);
            long position2 = Bass.BASS_ChannelGetPosition(Stream);
            Position_S.Value = Bass.BASS_ChannelBytes2Seconds(Stream, position2);
        }
        //+5秒
        private void Plus_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
                return;
            long position = Bass.BASS_ChannelGetPosition(Stream);
            if (Bass.BASS_ChannelBytes2Seconds(Stream, position) + 5 > Position_S.Maximum)
                Music_Pos_Change(Position_S.Maximum, true);
            else
                Music_Pos_Change(Bass.BASS_ChannelBytes2Seconds(Stream, position) + 5, true);
            long position2 = Bass.BASS_ChannelGetPosition(Stream);
            Position_S.Value = Bass.BASS_ChannelBytes2Seconds(Stream, position2);
        }
        //再生位置を変更
        //引数:時間、曲の時間も一緒に変更するか
        void Music_Pos_Change(double Position, bool IsBassPosChange)
        {
            if (IsBusy)
                return;
            if (IsBassPosChange)
                Bass.BASS_ChannelSetPosition(Stream, Position);
            TimeSpan Time = TimeSpan.FromSeconds(Position);
            string Minutes = Time.Minutes.ToString();
            string Seconds = Time.Seconds.ToString();
            if (Time.Minutes < 10)
                Minutes = "0" + Time.Minutes;
            if (Time.Seconds < 10)
                Seconds = "0" + Time.Seconds;
            Position_T.Text = Minutes + ":" + Seconds;
        }
        //再生位置を変更(スライダー)
        private void Position_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLocationChanging)
                Music_Pos_Change(Position_S.Value, false);
        }
        //再生位置のスライダーを押したら
        void Position_S_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsBusy)
                return;
            IsLocationChanging = true;
            Bass.BASS_ChannelPause(Stream);
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, 0f);
        }
        //再生位置のスライダーを離したら
        void Position_S_MouseUp(object sender, MouseButtonEventArgs e)
        {
            IsLocationChanging = false;
            Bass.BASS_ChannelSetPosition(Stream, Position_S.Value);
            if (!IsPaused)
            {
                Bass.BASS_ChannelPlay(Stream, false);
                Play_Volume_Animation();
            }
        }
        void Volume_S_MouseUp(object sender, MouseEventArgs e)
        {
            Configs_Save();
        }
        //設定を保存
        void Configs_Save()
        {
            try
            {
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Configs/Create_Loading_BGM.tmp");
                stw.Write(Volume_S.Value);
                stw.Close();
                Sub_Code.File_Encrypt(Voice_Set.Special_Path + "/Configs/Create_Loading_BGM.tmp", Voice_Set.Special_Path + "/Configs/Create_Loading_BGM.conf", "Create_Loading_BGM_Configs_Save", true);
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        //リストに曲を追加
        private void Music_Add_B_Click(object sender, RoutedEventArgs e)
        {
            if (BGM_Type_L.SelectedIndex == -1)
            {
                Message_Feed_Out("先に\"ロードBGMの種類\"を選択してください。");
                return;
            }
            if (IsBusy)
                return;
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog()
            {
                Title = "追加する曲を選択してください。",
                Filter = "音楽ファイル(*.mp3;*.wav;*.ogg;*.flac;*.wma;*.aac;*.mp4)|*.mp3;*.wav;*.ogg;*.flac;*.wma;*.aac;*.mp4",
                Multiselect = true
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string Error_FileName = "";
                foreach (string FilePath in ofd.FileNames)
                {
                    if (Music_Type_Music[BGM_Type_L.SelectedIndex].Contains(FilePath))
                    {
                        Error_FileName += "\n" + Path.GetFileName(FilePath);
                        continue;
                    }
                    Music_Type_Music[BGM_Type_L.SelectedIndex].Add(FilePath);
                    Music_Play_Times[BGM_Type_L.SelectedIndex].Add(new Music_Play_Time(0, 0));
                    if (BGM_Type_L.SelectedIndex == 12 || BGM_Type_L.SelectedIndex == 14 || BGM_Type_L.SelectedIndex == 16)
                        Music_Feed_In[BGM_Type_L.SelectedIndex].Add(false);
                    else
                        Music_Feed_In[BGM_Type_L.SelectedIndex].Add(true);
                    BGM_Music_L.Items.Add(Path.GetFileName(FilePath));
                }
                if (Error_FileName != "")
                    MessageBox.Show("以下のファイルは既に指定されているため、新たに追加することはできません。" + Error_FileName);
                Update_Music_Type_List();
            }
            ofd.Dispose();
        }
        //リストから曲を削除
        private void Music_Delete_B_Click(object sender, RoutedEventArgs e)
        {
            if (BGM_Type_L.SelectedIndex == -1 || BGM_Music_L.SelectedIndex == -1 || IsBusy)
                return;
            Bass.BASS_ChannelStop(Stream);
            Bass.BASS_StreamFree(Stream);
            Position_S.Value = 0;
            Position_T.Text = "00:00";
            Play_Time_T.Text = "再生時間:0～0";
            Music_Type_Music[BGM_Type_L.SelectedIndex].RemoveAt(BGM_Music_L.SelectedIndex);
            Music_Play_Times[BGM_Type_L.SelectedIndex].RemoveAt(BGM_Music_L.SelectedIndex);
            Music_Feed_In[BGM_Type_L.SelectedIndex].RemoveAt(BGM_Music_L.SelectedIndex);
            BGM_Music_L.Items.RemoveAt(BGM_Music_L.SelectedIndex);
            Feed_In_C.IsChecked = false;
            Update_Music_Type_List();
        }
        //再生速度を初期化
        private void Speed_S_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            Speed_S.Value = 50;
        }
        //作成(すべて)
        private void Create_B_Click(object sender, RoutedEventArgs e)
        {
            int Music_Count = 0;
            for (int Number = 0; Number < Music_Type_Music.Count; Number++)
            {
                Music_Count += Music_Type_Music[Number].Count;
            }
            if (Music_Count == 0)
            {
                Message_Feed_Out("最低1つはBGMファイルを選択する必要があります。");
                return;
            }
            Music_Mod_Create(false);
        }
        //指定した項目のみ作成
        private void Create_One_B_Click(object sender, RoutedEventArgs e)
        {
            if (BGM_Type_L.SelectedIndex == -1)
            {
                Message_Feed_Out("\"BGMの種類\"を選択する必要があります。");
                return;
            }
            if (Music_Type_Music[BGM_Type_L.SelectedIndex].Count == 0)
            {
                if (BGM_Type_L.SelectedIndex == 13 || BGM_Type_L.SelectedIndex == 14 || BGM_Type_L.SelectedIndex == 15 || BGM_Type_L.SelectedIndex == 16)
                {
                    if (Music_Type_Music[13].Count == 0 || Music_Type_Music[15].Count == 0)
                    {
                        Message_Feed_Out("引き分け、または敗北はどちらともに1つ以上BGMを入れる必要があります。");
                    }
                }
                else if (BGM_Type_L.SelectedIndex == 16 || BGM_Type_L.SelectedIndex == 17)
                {
                    if (Music_Type_Music[16].Count == 0 || Music_Type_Music[17].Count == 0)
                    {
                        Message_Feed_Out("優勢はどちらともに1つ以上BGMを入れる必要があります。");
                    }
                }
                else
                {
                    Message_Feed_Out("選択したタイプに最低1つはBGMファイルを追加する必要があります。");
                }
                return;
            }
            Music_Mod_Create(true);
        }
        //すべて作成する場合は-1
        async void Music_Mod_Create(bool IsSelectedOnly)
        {
            if (IsClosing || IsBusy)
                return;
            BetterFolderBrowser bfb = new BetterFolderBrowser()
            {
                Title = "保存先を指定してください。",
                Multiselect = false,
                RootFolder = Sub_Code.Get_OpenDirectory_Path()
            };
            if (bfb.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                IsBusy = true;
                IsMessageShowing = false;
                Message_T.Opacity = 1;
                Sub_Code.Set_Directory_Path(bfb.SelectedFolder);
                if (!Sub_Code.CanDirectoryAccess(bfb.SelectedFolder))
                {
                    Message_Feed_Out("指定したフォルダにアクセスできません。");
                    IsBusy = false;
                    bfb.Dispose();
                    return;
                }
                IsPaused = true;
                float Volume_Now = 1f;
                Bass.BASS_ChannelGetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, ref Volume_Now);
                float Volume_Minus = Volume_Now / 10f;
                while (Volume_Now > 0f && IsPaused)
                {
                    Volume_Now -= Volume_Minus;
                    if (Volume_Now < 0f)
                        Volume_Now = 0f;
                    Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, Volume_Now);
                    await Task.Delay(1000 / 60);
                }
                if (Volume_Now <= 0f)
                    Bass.BASS_ChannelPause(Stream);
                Message_T.Text = "プロジェクトファイルを作成しています...";
                await Task.Delay(50);
                FileInfo fi = new FileInfo(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.wwu");
                if (File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp") && fi.Length >= 1000000)
                    File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.wwu", true);
                if (!File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp"))
                    File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.wwu", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp", true);
                int Set_Volume = (int)(-40 * (1 - Volume_WoTB_S.Value / 100));
                Wwise_Project_Create Wwise = new Wwise_Project_Create(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod");
                for (int Number = 0; Number < Music_Type_Music.Count; Number++)
                {
                    for (int Number_01 = 0; Number_01 < Music_Type_Music[Number].Count; Number_01++)
                    {
                        Wwise.Loading_Music_Add_Wwise(Music_Type_Music[Number][Number_01], Number, Music_Play_Times[Number][Number_01], Music_Feed_In[Number][Number_01], Set_Volume);
                    }
                }
                Message_T.Text = "ファイルを.wavにエンコードしています...";
                await Task.Delay(50);
                await Wwise.Sound_To_WAV();
                Wwise.Save();
                //数を合わせるため使用しない項目を入れています。
                string[] Loading_Music_Name = { "America_lakville", "America_overlord", "Chinese", "Desert_airfield", "Desert_sand_river","Europe_himmelsdorf",
                "Europe_mannerheim","Europe_ruinberg","Japan","Russian_malinovka","Russian_prokhorovka","リザルト(勝利)","リザルト(勝利)","リザルト(敗北、または引き分け)","リザルト(敗北、または引き分け)",
                "リザルト(敗北、または引き分け)","リザルト(敗北、または引き分け)","優勢(敵味方両方)","優勢(敵味方両方)"};
                string[] Loading_Music_Type = { "music_maps_america_lakville", "music_maps_america_overlord", "music_maps_chinese", "music_maps_desert_airfield",
                "music_maps_desert_sand_river","music_maps_europe_himmelsdorf","music_maps_europe_mannerheim","music_maps_europe_ruinberg","music_maps_japan",
                "music_maps_russian_malinovka","music_maps_russian_prokhorovka","music_result_screen_basic","music_result_screen_basic", "music_result_screen","music_result_screen",
                "music_result_screen","music_result_screen","music_battle","music_battle"};
                List<string> Build_Names = new List<string>();
                if (!IsSelectedOnly)
                {
                    for (int Number = 0; Number < BGM_Type_L.Items.Count; Number++)
                    {
                        if (Music_Type_Music[Number].Count > 0)
                        {
                            if (Build_Names.Contains(Loading_Music_Type[Number]))
                                continue;
                            Build_Names.Add(Loading_Music_Type[Number]);
                            Message_T.Text = Loading_Music_Name[Number] + "をビルドしています...";
                            await Task.Delay(100);
                            Wwise.Project_Build(Loading_Music_Type[Number], bfb.SelectedFolder + "/" + Loading_Music_Type[Number] + ".bnk");
                        }
                    }
                }
                else
                {
                    foreach (int Number in ListBoxEx.SelectedIndexs(BGM_Type_L))
                    {
                        if (Build_Names.Contains(Loading_Music_Type[Number]))
                            continue;
                        Build_Names.Add(Loading_Music_Type[Number]);
                        Message_T.Text = Loading_Music_Name[Number] + "をビルドしています...";
                        await Task.Delay(100);
                        Wwise.Project_Build(Loading_Music_Type[Number], bfb.SelectedFolder + "/" + Loading_Music_Type[Number] + ".bnk");
                    }
                }
                Build_Names.Clear();
                await Task.Delay(100);
                Wwise.Clear();
                if (File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp"))
                    File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.wwu", true);
                Message_Feed_Out("完了しました。指定したフォルダを参照してください。");
                Flash.Flash_Start();
                IsBusy = false;
            }
            bfb.Dispose();
        }
        //クリア
        private async void Clear_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            int Music_Count = 0;
            for (int Number = 0; Number < Music_Type_Music.Count; Number++)
            {
                Music_Count += Music_Type_Music[Number].Count;
            }
            if (Music_Count == 0)
            {
                if (r.Next(0, 10) == 5)
                    Message_Feed_Out("内容がないようです。");
                else
                    Message_Feed_Out("既にクリアされています。");
                return;
            }
            MessageBoxResult result = MessageBox.Show("内容をクリアしますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                IsPaused = true;
                float Volume_Now = 1f;
                Bass.BASS_ChannelGetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, ref Volume_Now);
                float Volume_Minus = Volume_Now / 10f;
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
                    Bass.BASS_ChannelStop(Stream);
                    Bass.BASS_StreamFree(Stream);
                    Position_S.Value = 0;
                    Position_S.Maximum = 0;
                    Position_T.Text = "00:00";
                }
                for (int Number = 0; Number < Music_Type_Music.Count; Number++)
                {
                    Music_Type_Music[Number].Clear();
                }
                for (int Number = 0; Number < Music_Play_Times.Count; Number++)
                {
                    Music_Play_Times[Number].Clear();
                }
                for (int Number = 0; Number < Music_Feed_In.Count; Number++)
                {
                    Music_Feed_In[Number].Clear();
                }
                Play_Time_T.Text = "再生時間:0～0";
                Volume_WoTB_S.Value = 75;
                BGM_Music_L.Items.Clear();
                Update_Music_Type_List();
                BGM_Type_L.SelectedIndex = -1;
                Feed_In_C.IsChecked = false;
                Message_Feed_Out("内容を初期化しました。");
            }
        }
        //再生開始位置を指定
        private void Start_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy || BGM_Type_L.SelectedIndex == -1 || BGM_Music_L.SelectedIndex == -1)
                return;
            Music_Play_Times[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex].Start_Time = Position_S.Value;
            double End_Time = Music_Play_Times[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex].End_Time;
            if (End_Time != 0 && Music_Play_Times[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex].Start_Time > End_Time)
            {
                Music_Play_Times[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex].End_Time = 0;
                Play_Time_T.Text = "再生時間:" + (int)Music_Play_Times[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex].Start_Time + "～" + (int)Position_S.Maximum;
                Message_Feed_Out("開始時間が終了時間より大きかったため、終了時間を最大にします。");
            }
            else if (End_Time != 0)
                Play_Time_T.Text = "再生時間:" + (int)Music_Play_Times[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex].Start_Time + "～" + (int)End_Time;
            else
                Play_Time_T.Text = "再生時間:" + (int)Music_Play_Times[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex].Start_Time + "～" + (int)Position_S.Maximum;
        }
        //再生終了位置を指定
        private void End_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy || BGM_Type_L.SelectedIndex == -1 || BGM_Music_L.SelectedIndex == -1)
                return;
            Music_Play_Times[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex].End_Time = Position_S.Value;
            if (Music_Play_Times[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex].End_Time < Music_Play_Times[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex].Start_Time)
            {
                Music_Play_Times[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex].Start_Time = 0;
                Message_Feed_Out("終了時間が開始時間より小さかったため、開始時間を0秒にします。");
            }
            Play_Time_T.Text = "再生時間:" + (int)Music_Play_Times[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex].Start_Time + "～" + (int)Music_Play_Times[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex].End_Time;
        }
        //開始位置、終了位置を初期化
        private void Time_Clear_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy || BGM_Type_L.SelectedIndex == -1 || BGM_Music_L.SelectedIndex == -1)
                return;
            Music_Play_Times[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex].Start_Time = 0;
            Music_Play_Times[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex].End_Time = 0;
            Play_Time_T.Text = "再生時間:0～" + (int)Position_S.Maximum;
        }
        //フェードインのチェックボックスが押されたら
        private void Feed_In_C_Click(object sender, RoutedEventArgs e)
        {
            if (BGM_Type_L.SelectedIndex == -1 || BGM_Music_L.SelectedIndex == -1)
            {
                Feed_In_C.IsChecked = false;
                return;
            }
            if (Feed_In_C.IsChecked.Value)
                Music_Feed_In[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex] = true;
            else
                Music_Feed_In[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex] = false;
        }
        private void Volume_WoTB_Help_B_Click(object sender, RoutedEventArgs e)
        {
            string Message_01 = "・この設定は、全BGMに当てはまります。個別で設定することはできません。\n";
            string Message_02 = "・実際に聞こえる音量は、この設定の数値とWoTB内のBGMの数値を足したものになります。\n";
            string Message_03 = "例えば、この設定を50にし、WoTBのBGM設定も50にすると、だいたい25くらいの音量になります。\n";
            string Message_04 = "試してみた感じ、この設定を30、WoTBのBGM設定を20にするとほとんど聞こえないような感じでした。ご参考までに。";
            MessageBox.Show(Message_01 + Message_02 + Message_03 + Message_04);
        }
    }
}