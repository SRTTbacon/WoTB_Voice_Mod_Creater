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
using WK.Libraries.BetterFolderBrowserNS;
using WoTB_Voice_Mod_Creater.Class;

namespace WoTB_Voice_Mod_Creater.Wwise_Class
{
    public partial class WoT_To_Blitz : System.Windows.Controls.UserControl
    {
        bool IsMessageShowing = false;
        bool IsClosing = false;
        bool IsBusy = false;
        bool IsLocationChanging = false;
        bool IsPaused = false;
        int Stream;
        int Max_Stream_Count = 0;
        int Available_Stream_Count = 0;
        Random r = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
        List<List<string>> BNK_Voices = new List<List<string>>();
        List<string> BGM_Add = new List<string>();
        Wwise_Class.Wwise_File_Extract_V1 Wwise_PCK;
        Wwise_Class.Wwise_File_Extract_V2 Wwise_BNK;
        public WoT_To_Blitz()
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
        //画面を表示
        public async void Window_Show()
        {
            Opacity = 0;
            Visibility = Visibility.Visible;
            if (File.Exists(Voice_Set.Special_Path + "/Configs/WoT_To_Blitz.conf"))
            {
                try
                {
                    using (var eifs = new FileStream(Voice_Set.Special_Path + "/Configs/WoT_To_Blitz.conf", FileMode.Open, FileAccess.Read))
                    {
                        using (var eofs = new FileStream(Voice_Set.Special_Path + "/Configs/WoT_To_Blitz.tmp", FileMode.Create, FileAccess.Write))
                        {
                            FileEncode.FileEncryptor.Decrypt(eifs, eofs, "WoT_To_Blitz_Configs_Save");
                        }
                    }
                    StreamReader str = new StreamReader(Voice_Set.Special_Path + "/Configs/WoT_To_Blitz.tmp");
                    Volume_S.Value = double.Parse(str.ReadLine());
                    DVPL_C.IsChecked = bool.Parse(str.ReadLine());
                    Install_C.IsChecked = bool.Parse(str.ReadLine());
                    Volume_Set_C.IsChecked = bool.Parse(str.ReadLine());
                    PCK_Mode_C.IsChecked = bool.Parse(str.ReadLine());
                    str.Close();
                    File.Delete(Voice_Set.Special_Path + "/Configs/WoT_To_Blitz.tmp");
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show("設定を読み込めませんでした。。\nエラー回避のため設定は削除されます。");
                    File.Delete(Voice_Set.Special_Path + "/Configs/WoT_To_Blitz.conf");
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
        //戻る
        private async void Back_B_Click(object sender, RoutedEventArgs e)
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
                BGM_Add_List.SelectedIndex = -1;
                Visibility = Visibility.Hidden;
                IsClosing = false;
            }
        }
        //PC版WoTの音声ファイルを指定
        private async void Open_File_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
            {
                return;
            }
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Title = "BNKファイルを選択してください。",
                Multiselect = false,
                Filter = "BNKファイル(*.bnk)|*.bnk"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string PCK_File_Path = Path.GetDirectoryName(ofd.FileName) + "/" + Path.GetFileNameWithoutExtension(ofd.FileName) + ".pck";
                if (PCK_Mode_C.IsChecked.Value && !File.Exists(PCK_File_Path))
                {
                    Message_Feed_Out(Path.GetFileNameWithoutExtension(ofd.FileName) + ".pckが見つかりませんでした。");
                    IsBusy = false;
                    return;
                }
                Bass.BASS_ChannelStop(Stream);
                Bass.BASS_StreamFree(Stream);
                try
                {
                    if (Directory.Exists(Voice_Set.Special_Path + "/Wwise/BNK_WAV"))
                    {
                        Directory.Delete(Voice_Set.Special_Path + "/Wwise/BNK_WAV", true);
                    }
                }
                catch (Exception e1)
                {
                    Sub_Code.Error_Log_Write(e1.Message);
                }
                IsBusy = true;
                try
                {
                    Message_T.Text = ".bnkファイルを解析しています...";
                    await Task.Delay(50);
                    Wwise_Class.BNK_Parse p = new Wwise_Class.BNK_Parse(ofd.FileName);
                    if (!p.IsVoiceFile())
                    {
                        Message_Feed_Out("選択されたbnkファイルは音声データではありません。");
                        IsBusy = false;
                        return;
                    }
                    BNK_Voices = p.Get_Voices(false);
                    List<string> Need_Files = new List<string>();
                    foreach (List<string> Types in BNK_Voices)
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
                        BNK_Voices = p.Get_Voices(false);
                        foreach (List<string> Types in BNK_Voices)
                        {
                            foreach (string File_Now in Types)
                            {
                                Need_Files.Add(File_Now);
                            }
                        }
                        if (Need_Files.Count == 0)
                        {
                            p.Clear();
                            BNK_Voices.Clear();
                            Message_Feed_Out("移植できる音声が見つかりませんでした。");
                            IsBusy = false;
                            return;
                        }
                    }
                    p.Clear();
                    Get_Available_Voice_Count();
                    Voices_L.Items.Clear();
                    for (int Number = 0; Number < BNK_Voices.Count; Number++)
                    {
                        Voices_L.Items.Add(Voice_Set.Get_Voice_Type_Japanese_Name_V2(Number) + " : " + BNK_Voices[Number].Count + "個");
                    }
                    BGM_Count_Change();
                    if (PCK_Mode_C.IsChecked.Value)
                    {
                        Message_T.Text = ".wemファイルに変換しています...";
                        await Task.Delay(50);
                        Wwise_PCK = new Wwise_File_Extract_V1(PCK_File_Path);
                        Max_Stream_Count = Wwise_PCK.Wwise_Get_File_Count();
                        await Wwise_PCK.Async_Wwise_Extract_To_WEM_Directory(Voice_Set.Special_Path + "/Wwise/BNK_WAV");
                        Message_T.Text = ".wemファイルを.oggに変換しています...";
                        await Task.Delay(50);
                        await Wwise_PCK.Async_Wwise_Extract_To_OGG_Directory(Voice_Set.Special_Path + "/Wwise/BNK_WAV");
                    }
                    else
                    {
                        Message_T.Text = ".wavまたは.oggに変換しています...";
                        await Task.Delay(50);
                        Wwise_BNK = new Wwise_File_Extract_V2(ofd.FileName);
                        Max_Stream_Count = Wwise_BNK.Wwise_Get_Numbers();
                        Wwise_BNK.Wwise_Extract_To_WEM_Directory_V2(Voice_Set.Special_Path + "/Wwise/BNK_WAV");
                    }
                    string[] Files = Directory.GetFiles(Voice_Set.Special_Path + "/Wwise/BNK_WAV", "*.wem", SearchOption.TopDirectoryOnly);
                    foreach (string File_Now in Files)
                    {
                        Sub_Code.File_Delete_V2(File_Now);
                    }
                    Message_T.Text = "不要な音声ファイルを削除しています...";
                    await Task.Delay(50);
                    string[] All_Files = Directory.GetFiles(Voice_Set.Special_Path + "/Wwise/BNK_WAV", "*", SearchOption.TopDirectoryOnly);
                    foreach (string File_Now in All_Files)
                    {
                        if (!Need_Files.Contains(Path.GetFileNameWithoutExtension(File_Now)))
                        {
                            Sub_Code.File_Delete_V2(File_Now);
                        }
                    }
                    File_Name_T.Text = Path.GetFileName(ofd.FileName);
                    Message_Feed_Out(".bnkファイルをロードしました。");
                }
                catch (Exception e1)
                {
                    Sub_Code.Error_Log_Write(e1.Message);
                    Message_Feed_Out("解析に失敗しました。Error_Log.txtを参照してください。");
                }
                IsBusy = false;
            }
        }
        //反映される音声の数を取得
        void Get_Available_Voice_Count()
        {
            Available_Stream_Count = 0;
            for (int Number = 0; Number < BNK_Voices.Count; Number++)
            {
                Available_Stream_Count += BNK_Voices[Number].Count;
            }
        }
        //追加されたBGMをリストから削除
        private void BGM_Delete_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            if (BGM_Add_List.SelectedIndex != -1)
            {
                Bass.BASS_ChannelStop(Stream);
                Bass.BASS_StreamFree(Stream);
                int Selected_Index_Now = BGM_Add_List.SelectedIndex;
                for (int File_Now = 0; File_Now < BGM_Add.Count; File_Now++)
                {
                    if (Path.GetFileName(BGM_Add[File_Now]) == BGM_Add_List.Items[Selected_Index_Now].ToString())
                    {
                        BGM_Add.RemoveAt(File_Now);
                        break;
                    }
                }
                BGM_Add_List.Items.RemoveAt(BGM_Add_List.SelectedIndex);
                if (BGM_Add_List.Items.Count - 1 >= Selected_Index_Now)
                {
                    BGM_Add_List.SelectedIndex = Selected_Index_Now;
                }
                Location_S.Value = 0;
                Location_S.Maximum = 0;
                Location_T.Text = "00:00";
                BGM_Count_Change();
            }
        }
        //戦闘BGMを追加
        private void BGM_Add_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Title = "BGMファイルを選択してください。",
                Filter = "BGMファイル(*.mp3;*.aac;*.flac;*.wav;*.ogg;*.m4a;*.aiff)|*.mp3;*.aac;*.flac;*.wav;*.ogg;*.m4a;*.aiff",
                Multiselect = true
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                foreach (string File_Now in ofd.FileNames)
                {
                    BGM_Add_List.Items.Add(Path.GetFileName(File_Now));
                    BGM_Add.Add(File_Now);
                }
                BGM_Count_Change();
            }
        }
        //1つ1つ削除するのがめんどくさいとき用に一気に削除
        private void BGM_Clear_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            MessageBoxResult result = System.Windows.MessageBox.Show("BGMをクリアしますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                if (BGM_Add_List.SelectedIndex != -1)
                {
                    Bass.BASS_ChannelStop(Stream);
                    Bass.BASS_StreamFree(Stream);
                }
                BGM_Add_List.Items.Clear();
                BGM_Add.Clear();
                BGM_Count_Change();
            }
        }
        void BGM_Count_Change()
        {
            if (BNK_Voices.Count > 0)
            {
                string All = Voices_L.Items[49].ToString();
                string Name = All.Substring(0, All.IndexOf(":") + 2);
                Voices_L.Items[49] = Name + BGM_Add_List.Items.Count + "個";
            }
        }
        private void Clear_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            MessageBoxResult result = System.Windows.MessageBox.Show("内容をクリアしますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                Bass.BASS_ChannelStop(Stream);
                Bass.BASS_StreamFree(Stream);
                BGM_Add_List.Items.Clear();
                BGM_Add.Clear();
                Voices_L.Items.Clear();
                Voices_L.Items.Add("音声ファイルが選択されていません。");
                BNK_Voices.Clear();
                File_Name_T.Text = "";
                Location_S.Value = 0;
                Location_S.Maximum = 0;
                Max_Stream_Count = 0;
                Available_Stream_Count = 0;
                Location_T.Text = "00:00";
                try
                {
                    if (Directory.Exists(Voice_Set.Special_Path + "/Wwise/BNK_WAV"))
                    {
                        Directory.Delete(Voice_Set.Special_Path + "/Wwise/BNK_WAV", true);
                    }
                }
                catch (Exception e1)
                {
                    Sub_Code.Error_Log_Write(e1.Message);
                }
                if (Wwise_PCK != null)
                    Wwise_PCK.Pck_Clear();
                if (Wwise_BNK != null)
                    Wwise_BNK.Bank_Clear();
            }
        }
        //指定した音声を変換して再生
        private void Voices_L_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Voices_L.SelectedIndex == -1 || BNK_Voices.Count == 0 || IsBusy)
            {
                return;
            }
            Bass.BASS_ChannelStop(Stream);
            Bass.BASS_StreamFree(Stream);
            int Voice_Count = BNK_Voices[Voices_L.SelectedIndex].Count;
            if (Voice_Count == 0)
            {
                Location_S.Value = 0;
                Location_S.Maximum = 0;
                Location_T.Text = "00:00";
                return;
            }
            string Get_Number = BNK_Voices[Voices_L.SelectedIndex][r.Next(0, Voice_Count)];
            BGM_Add_List.SelectedIndex = -1;
            Load_Sound(Sub_Code.File_Get_FileName_No_Extension(Voice_Set.Special_Path + "/Wwise/BNK_WAV/" + Get_Number));
        }
        //BGMに指定したファイルを再生
        private void BGM_Add_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BGM_Add_List.SelectedIndex == -1)
            {
                return;
            }
            Bass.BASS_ChannelStop(Stream);
            Bass.BASS_StreamFree(Stream);
            Voices_L.SelectedIndex = -1;
            Load_Sound();
        }
        void Load_Sound(string File_Name = "")
        {
            Location_S.Value = 0;
            Location_T.Text = "00:00";
            int StreamHandle;
            if (Voices_L.SelectedIndex != -1)
            {
                StreamHandle = Bass.BASS_StreamCreateFile(File_Name, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_LOOP);
            }
            else if (BGM_Add_List.SelectedIndex != -1)
            {
                StreamHandle = Bass.BASS_StreamCreateFile(BGM_Add[BGM_Add_List.SelectedIndex], 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_LOOP);
            }
            else
            {
                Location_S.Maximum = 0;
                return;
            }
            Stream = BassFx.BASS_FX_TempoCreate(StreamHandle, BASSFlag.BASS_FX_FREESOURCE);
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)Volume_S.Value / 100);
            Bass.BASS_ChannelSetDevice(Stream, Video_Mode.Sound_Device);
            Location_S.Maximum = Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetLength(Stream, BASSMode.BASS_POS_BYTES));
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
            if (IsClosing || IsBusy)
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
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Configs/WoT_To_Blitz.tmp");
                stw.WriteLine(Volume_S.Value);
                stw.WriteLine(DVPL_C.IsChecked.Value);
                stw.WriteLine(Install_C.IsChecked.Value);
                stw.WriteLine(Volume_Set_C.IsChecked.Value);
                stw.Write(PCK_Mode_C.IsChecked.Value);
                stw.Close();
                using (var eifs = new FileStream(Voice_Set.Special_Path + "/Configs/WoT_To_Blitz.tmp", FileMode.Open, FileAccess.Read))
                {
                    using (var eofs = new FileStream(Voice_Set.Special_Path + "/Configs/WoT_To_Blitz.conf", FileMode.Create, FileAccess.Write))
                    {
                        FileEncode.FileEncryptor.Encrypt(eifs, eofs, "WoT_To_Blitz_Configs_Save");
                    }
                }
                File.Delete(Voice_Set.Special_Path + "/Configs/WoT_To_Blitz.tmp");
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
        private void BGM_Add_List_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (BGM_Add_List.SelectedIndex != -1)
            {
                Bass.BASS_ChannelStop(Stream);
                Bass.BASS_StreamFree(Stream);
                BGM_Add_List.SelectedIndex = -1;
            }
        }
        private async void Start_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
            {
                return;
            }
            if (BNK_Voices.Count == 0)
            {
                Message_Feed_Out("音声ファイルが選択されていません。");
                return;
            }
            foreach (string BGM_File in BGM_Add)
            {
                if (!File.Exists(BGM_File))
                {
                    Message_Feed_Out(Path.GetFileName(BGM_File) + "が存在しませんでした。");
                    return;
                }
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
                    List<string> Add_BGM_List = new List<string>();
                    if (BGM_Add_List.Items.Count > 0)
                    {
                        Message_T.Text = "BGMファイルを適応させています...";
                        await Task.Delay(50);
                        for (int Number = 0; Number < BGM_Add.Count; Number++)
                        {
                            if (Number < 10)
                            {
                                File.Copy(BGM_Add[Number], Voice_Set.Special_Path + "/Wwise/BNK_WAV/battle_bgm_0" + Number + Path.GetExtension(BGM_Add[Number]), true);
                                Add_BGM_List.Add(Voice_Set.Special_Path + "/Wwise/BNK_WAV/battle_bgm_0" + Number + ".wav");
                            }
                            else
                            {
                                File.Copy(BGM_Add[Number], Voice_Set.Special_Path + "/Wwise/BNK_WAV/battle_bgm_" + Number + Path.GetExtension(BGM_Add[Number]), true);
                                Add_BGM_List.Add(Voice_Set.Special_Path + "/Wwise/BNK_WAV/battle_bgm_" + Number + ".wav");
                            }
                        }
                    }
                    if (Volume_Set_C.IsChecked.Value)
                    {
                        Message_T.Text = ".mp3に変換しています...";
                        await Task.Delay(50);
                        await Multithread.Convert_To_MP3(Directory.GetFiles(Voice_Set.Special_Path + "/Wwise/BNK_WAV", "*", SearchOption.TopDirectoryOnly), Voice_Set.Special_Path + "/Wwise/BNK_WAV", true);
                        Message_T.Text = "音量をWoTB用に調整しています...";
                        await Task.Delay(50);
                        Sub_Code.MP3_Volume_Set(Voice_Set.Special_Path + "/Wwise/BNK_WAV");
                    }
                    Message_T.Text = ".wavに変換しています...";
                    await Task.Delay(50);
                    await Multithread.Convert_To_Wav(Directory.GetFiles(Voice_Set.Special_Path + "/Wwise/BNK_WAV", "*", SearchOption.TopDirectoryOnly), Voice_Set.Special_Path + "/Wwise/BNK_WAV", true, true);
                    Message_T.Text = "ファイルをコピーしています...";
                    if (Directory.Exists(Voice_Set.Special_Path + "/Wwise/BNK_WAV/Voices"))
                    {
                        Directory.Delete(Voice_Set.Special_Path + "/Wwise/BNK_WAV/Voices", true);
                    }
                    await Task.Delay(50);
                    string Log_01 = Sub_Code.Set_Voice_Type_Change_Name_By_Index(Voice_Set.Special_Path + "/Wwise/BNK_WAV", Voice_Set.Special_Path + "/Wwise/BNK_WAV/Voices", BNK_Voices);
                    if (Log_01 != "")
                    {
                        Message_Feed_Out("ファイルをコピーできませんでした。詳しくは\"Error_Log.txt\"を参照してください。");
                        Directory.Delete(Voice_Set.Special_Path + "/Wwise/BNK_WAV/Voices", true);
                        IsBusy = false;
                        return;
                    }
                    foreach (string BGM_Now in Add_BGM_List)
                    {
                        Sub_Code.File_Move(BGM_Now, Voice_Set.Special_Path + "/Wwise/BNK_WAV/Voices/" + Path.GetFileName(BGM_Now), true);
                    }
                    Message_T.Text = "SEの有無をチェックし、適応しています...";
                    await Task.Delay(50);
                    Voice_Set.Set_SE_Change_Name();
                    await BNK_Create_V2(Voice_Set.Special_Path + "/Wwise/BNK_WAV/Voices", SetPath);
                    if (DVPL_C.IsChecked.Value)
                    {
                        Message_T.Text = "DVPL化しています...";
                        await Task.Delay(50);
                        DVPL.DVPL_Pack(SetPath + "/voiceover_crew.bnk", SetPath + "/voiceover_crew.bnk.dvpl", true);
                        DVPL.DVPL_Pack(SetPath + "/ui_battle.bnk", SetPath + "/ui_battle.bnk.dvpl", true);
                        DVPL.DVPL_Pack(SetPath + "/ui_chat_quick_commands.bnk", SetPath + "/ui_chat_quick_commands.bnk.dvpl", true);
                        DVPL.DVPL_Pack(SetPath + "/reload.bnk", SetPath + "/reload.bnk.dvpl", true);
                    }
                    Directory.Delete(Voice_Set.Special_Path + "/Wwise/BNK_WAV/Voices", true);
                    bool IsOK = true;
                    if (Install_C.IsChecked.Value)
                    {
                        Message_T.Text = "WoTBに適応しています...";
                        await Task.Delay(50);
                        Sub_Code.DVPL_File_Delete(Voice_Set.WoTB_Path + "/Data/WwiseSound/ja/voiceover_crew.bnk");
                        Sub_Code.DVPL_File_Delete(Voice_Set.WoTB_Path + "/Data/WwiseSound/reload.bnk");
                        Sub_Code.DVPL_File_Delete(Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_chat_quick_commands.bnk");
                        Sub_Code.DVPL_File_Delete(Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_battle.bnk");
                        if (!Sub_Code.DVPL_File_Copy(SetPath + "/voiceover_crew.bnk", Voice_Set.WoTB_Path + "/Data/WwiseSound/ja/voiceover_crew.bnk", true))
                        {
                            IsOK = false;
                        }
                        Sub_Code.DVPL_File_Copy(SetPath + "/reload.bnk", Voice_Set.WoTB_Path + "/Data/WwiseSound/reload.bnk", true);
                        Sub_Code.DVPL_File_Copy(SetPath + "/ui_chat_quick_commands.bnk", Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_chat_quick_commands.bnk", true);
                        Sub_Code.DVPL_File_Copy(SetPath + "/ui_battle.bnk", Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_battle.bnk", true);
                    }
                    if (IsOK)
                        Message_Feed_Out("完了しました。");
                    else
                        Message_Feed_Out("エラー:WoTBに適応できませんでした。");
                }
                catch (Exception e1)
                {
                    Sub_Code.Error_Log_Write(e1.Message);
                }
                IsBusy = false;
            }
            fbd.Dispose();
        }
        private void DVPL_C_Click(object sender, RoutedEventArgs e)
        {
            Configs_Save();
        }
        private void Install_C_Click(object sender, RoutedEventArgs e)
        {
            Configs_Save();
        }
        private void Volume_Set_C_Click(object sender, RoutedEventArgs e)
        {
            Configs_Save();
        }
        private void PCK_Mode_C_Click(object sender, RoutedEventArgs e)
        {
            Configs_Save();
        }
        //Voice_Create.xaml.csから引用
        async Task BNK_Create_V2(string Voice_Dir, string Save_Dir)
        {
            if (!Directory.Exists(Voice_Dir))
            {
                return;
            }
            Message_T.Text = "プロジェクトファイルを作成しています...";
            await Task.Delay(50);
            FileInfo fi = new FileInfo(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.wwu");
            if (File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp") && fi.Length >= 900000)
            {
                File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.wwu", true);
            }
            if (!File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp"))
            {
                File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.wwu", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp", true);
            }
            Wwise_Class.Wwise_Project_Create Wwise = new Wwise_Class.Wwise_Project_Create(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod");
            Wwise.Sound_Add_Wwise(Voice_Dir);
            Wwise.Save();
            if (File.Exists(Voice_Dir + "/battle_bgm_01.wav"))
            {
                Message_T.Text = ".bnkファイルを作成しています...\nBGMファイルが含まれているため時間がかかります。";
            }
            else
            {
                Message_T.Text = ".bnkファイルを作成しています...";
            }
            await Task.Delay(50);
            Wwise.Project_Build("voiceover_crew", Save_Dir + "/voiceover_crew.bnk");
            await Task.Delay(500);
            Wwise.Project_Build("ui_battle", Save_Dir + "/ui_battle.bnk");
            await Task.Delay(500);
            Wwise.Project_Build("ui_chat_quick_commands", Save_Dir + "/ui_chat_quick_commands.bnk");
            await Task.Delay(500);
            Wwise.Project_Build("reload", Save_Dir + "/reload.bnk");
            Wwise.Clear();
            if (File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp"))
            {
                File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.wwu", true);
            }
        }
        private void Help_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
            {
                return;
            }
            string Message_01 = "重要:このツールで移植した音声の配布は、制作者の許可がある場合を除き、絶対にしないでください。\n";
            string Message_02 = "・ほとんどのPC版WoTの音声に対応していますが、特殊な仕様の音声(フロントラインなど)がある場合は一部しか反映されないかもしれません。\n";
            string Message_03 = "・PC版WoTの音声のみ対応しています。音声以外の.bnkファイルを選択しても反映されません。\n";
            string Message_04 = "・今後のアップデートで対応する音声の種類が増えるかもしれません。";
            System.Windows.MessageBox.Show(Message_01 + Message_02 + Message_03 + Message_04);
        }
        private void Volume_Set_Help_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
            {
                return;
            }
            string Message_01 = "・音量をWoTB用に調整します。\n";
            string Message_02 = "・一度MP3形式に変換し、音量を調整してからWAV形式にしますので、通常より多くの時間が必要になります。";
            string Message_03 = "・変換中は、CPU使用率が高くなりますので、なるべく他の作業をしないことをおすすめします。";
            System.Windows.MessageBox.Show(Message_01 + Message_02 + Message_03);
        }
        private void Details_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
            {
                return;
            }
            if (BNK_Voices.Count == 0)
            {
                Message_Feed_Out("音声ファイルが指定されていないため、表示できる情報がありません。");
                return;
            }
            Message_Feed_Out("ファイル内の音声ファイル数:" + Max_Stream_Count + "\n" + "移植後の音声数:" + Available_Stream_Count);
        }
        private void Configs_B_Click(object sender, RoutedEventArgs e)
        {
            Save_Configs_Window.Window_Show_V3(File_Name_T.Text, BNK_Voices);
        }
    }
}