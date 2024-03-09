using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using WK.Libraries.BetterFolderBrowserNS;
using WoTB_Voice_Mod_Creater.Class;
using WoTB_Voice_Mod_Creater.FMOD_Class;

namespace WoTB_Voice_Mod_Creater.Wwise_Class
{
    public class Voice_Setting
    {
        public string File_Name = "";
        public int FSB_Index = -1;
        public uint File_ID = 0;
        public bool IsEnable = true;
        public Voice_Setting(int FSB_Index, string File_Name)
        {
            this.FSB_Index = FSB_Index;
            this.File_Name = File_Name;
        }
        public Voice_Setting(uint File_ID)
        {
            this.File_ID = File_ID;
        }
    }
    public class Type_Setting
    {
        public List<Voice_Setting> Voices = new List<Voice_Setting>();
        public uint Parent_Unique_ID = 0;
        public uint Container_ID = 0;
        public int Type_Index = 0;
        public bool IsEnable = true;
        public Type_Setting(uint Parent_Unique_ID, uint Container_ID, int Type_Index)
        {
            this.Parent_Unique_ID = Parent_Unique_ID;
            this.Container_ID = Container_ID;
            this.Type_Index = Type_Index;
        }
        public Type_Setting(uint Container_ID,int Type_Index)
        {
            this.Container_ID = Container_ID;
            this.Type_Index = Type_Index;
        }
    }
    public class BNK_FSB_Voice
    {
        public Dictionary<uint, Type_Setting> Types = new Dictionary<uint, Type_Setting>();
        public Wwise_File_Extract_V2 Wwise_BNK = null;
        public string FSB_File = "";
        public bool IsBNKMode = false;
        public void Clear()
        {
            foreach (Type_Setting Setting in Types.Values)
                Setting.Voices.Clear();
            Types.Clear();
            if (Wwise_BNK != null)
            {
                Wwise_BNK.Bank_Clear();
                Wwise_BNK = null;
            }
            FSB_File = "";
            IsBNKMode = false;
        }
        public void Add_Voice(uint Container_ID, uint File_ID, int Type_Index)
        {
            uint Unique_ID = WwiseHash.HashString(Type_Index + "_" + Container_ID);
            if (!Types.ContainsKey(Unique_ID))
                Types.Add(Unique_ID, new Type_Setting(Unique_ID, Container_ID, Type_Index));
            Types[Unique_ID].Voices.Add(new Voice_Setting(File_ID));
        }
        public void Add_Voice(int FSB_Index, string File_Name, int Type_Index)
        {
            uint Unique_ID = WwiseHash.HashString("FSB_" + Type_Index);
            if (!Types.ContainsKey(Unique_ID))
                Types.Add(Unique_ID, new Type_Setting(Unique_ID, Type_Index));
            Types[Unique_ID].Voices.Add(new Voice_Setting(FSB_Index, File_Name));
        }
        public void Add_Voice(Type_Setting Type, Voice_Setting Voice)
        {
            Voice_Setting New_Voice = new Voice_Setting(Voice.File_ID);
            New_Voice.IsEnable = Voice.IsEnable;
            Type.Voices.Add(New_Voice);
        }
    }
    public partial class Blitz_To_WoT : System.Windows.Controls.UserControl
    {
        uint Container_Copy_ID = 0;
        uint Now_Playing_File_ID = 0;
        int Stream;
        int Max_Stream_Count = 0;
        int Now_Stream_Count = 0;
        bool IsMessageShowing = false;
        bool IsClosing = false;
        bool IsBusy = false;
        bool IsPaused = false;
        bool IsLocationChanging = false;
        bool IsModeChanging = false;
        bool IsOpenDialog = false;
        bool IsCutMode = false;
        Voice_Setting Voice_Copy = null;
        List<int> Container_Indexes = new List<int>();
        Random r = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
        BNK_FSB_Voice BNK_FSB_Voices = new BNK_FSB_Voice();
        Brush Gray_Color = (Brush)new BrushConverter().ConvertFromString("#BFFF2C8C");
        public Blitz_To_WoT()
        {
            InitializeComponent();
            Location_S.AddHandler(MouseDownEvent, new MouseButtonEventHandler(Location_MouseDown), true);
            Location_S.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Location_MouseUp), true);
            Volume_S.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Volume_MouseUp), true);
            Voices_L.Items.Add("音声ファイルが選択されていません。");
            System.Windows.Controls.ContextMenu pMenu = new System.Windows.Controls.ContextMenu();
            System.Windows.Controls.MenuItem item1 = new System.Windows.Controls.MenuItem();
            System.Windows.Controls.MenuItem item2 = new System.Windows.Controls.MenuItem();
            item1.Header = "新規のコンテナを作成";
            item1.Click += delegate
            {
                if (Voices_L.SelectedIndex == -1)
                    Message_Feed_Out("先にイベントを選択してください。");
                else
                {
                    uint Unique_ID = WwiseHash.HashString("New_Container_" + Sub_Code.r.Next(10000, 100000));
                    BNK_FSB_Voices.Types.Add(Unique_ID, new Type_Setting(Unique_ID, Unique_ID, Voices_L.SelectedIndex));
                    IsModeChanging = true;
                    Voices_L_SelectionChanged(null, null);
                    IsModeChanging = false;
                    Message_Feed_Out("コンテナ(ID:" + Unique_ID + ")を作成しました。");
                }
            };
            item2.Header = "貼り付け";
            item2.Click += delegate
            {
                if (Voice_Copy != null)
                    Message_Feed_Out("コンテナが選択されている状態でのみ音声のコピーができます。");
                else
                    Container_OR_Voice_Copy(Voices_L.SelectedIndex, 0);
            };
            pMenu.Opened += delegate
            {
                item2.IsEnabled = !(Container_Copy_ID == 0 && Voice_Copy == null && Voices_L.SelectedIndex != -1);
            };
            pMenu.Items.Add(item1);
            pMenu.Items.Add(item2);
            Content_L.ContextMenu = pMenu;
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
                            Minutes = "0" + Time.Minutes;
                        if (Time.Seconds < 10)
                            Seconds = "0" + Time.Seconds;
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
                    StreamReader str = Sub_Code.File_Decrypt_To_Stream(Voice_Set.Special_Path + "/Configs/Blitz_To_WoT.conf", "Blitz_To_WoT_Configs_Save");
                    Volume_S.Value = double.Parse(str.ReadLine());
                    Volume_Set_C.IsChecked = bool.Parse(str.ReadLine());
                    str.Close();
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
                    Message_T.Opacity -= 0.025;
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
                        Volume_Now = 0f;
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
                if (result != MessageBoxResult.Yes)
                    return;
            }
            Bass.BASS_ChannelStop(Stream);
            Bass.BASS_StreamFree(Stream);
            Voices_L.Items.Clear();
            Content_L.Items.Clear();
            BNK_FSB_Voices.Clear();
            Voices_L.Items.Add("音声ファイルが選択されていません。");
            Container_Indexes.Clear();
            Voice_Copy = null;
            File_Name_T.Text = "";
            Container_Copy_ID = 0;
            Location_S.Value = 0;
            Location_S.Maximum = 0;
            Max_Stream_Count = 0;
            Now_Stream_Count = 0;
            Now_Playing_File_ID = 0;
            IsCutMode = false;
            IsOpenDialog = false;
            Location_T.Text = "00:00";
            try
            {
                string Temp_File = Sub_Code.File_Get_FileName_No_Extension(Voice_Set.Special_Path + "/Wwise/Blitz_To_WoT_Temp");
                if (File.Exists(Temp_File))
                    File.Delete(Temp_File);
                if (Directory.Exists(Voice_Set.Special_Path + "/Wwise/BNK_WAV_WoT"))
                    Directory.Delete(Voice_Set.Special_Path + "/Wwise/BNK_WAV_WoT", true);
            }
            catch (Exception e1)
            {
                Sub_Code.Error_Log_Write(e1.Message);
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
            IsPaused = false;
        }
        private void Volume_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Volume_T.Text = "音量:" + (int)Volume_S.Value;
            if (!IsPaused)
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)Volume_S.Value / 100);
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
            if (IsClosing || IsBusy)
                return;
            if (IsBassPosChange)
                Bass.BASS_ChannelSetPosition(Stream, Pos);
            TimeSpan Time = TimeSpan.FromSeconds(Pos);
            string Minutes = Time.Minutes.ToString();
            string Seconds = Time.Seconds.ToString();
            if (Time.Minutes < 10)
                Minutes = "0" + Time.Minutes;
            if (Time.Seconds < 10)
                Seconds = "0" + Time.Seconds;
            Location_T.Text = Minutes + ":" + Seconds;
        }
        private void Play_B_Click(object sender, RoutedEventArgs e)
        {
            if (Voices_L.SelectedIndex == -1 || IsBusy || IsClosing)
                return;
            Play_Volume_Animation(10f);
        }
        private void Pause_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            Pause_Volume_Animation(false, 10f);
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
                Sub_Code.File_Encrypt(Voice_Set.Special_Path + "/Configs/Blitz_To_WoT.tmp", Voice_Set.Special_Path + "/Configs/Blitz_To_WoT.conf", "Blitz_To_WoT_Configs_Save", true);
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                Message_Feed_Out("設定を保存できませんでした。");
            }
        }
        private async void Open_File_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
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
                    string To_Dir = Voice_Set.Special_Path + "/Wwise/BNK_WAV_WoT";
                    try
                    {
                        if (Directory.Exists(To_Dir))
                            Directory.Delete(To_Dir, true);
                    }
                    catch (Exception e1)
                    {
                        Sub_Code.Error_Log_Write(e1.Message);
                    }
                    BNK_FSB_Voices.Clear();
                    if (Path.GetExtension(ofd.FileName) == ".bnk")
                    {
                        Message_T.Text = ".bnkファイルを解析しています...";
                        await Task.Delay(50);
                        Wwise_Class.BNK_Parse p = new Wwise_Class.BNK_Parse(ofd.FileName);
                        if (!p.IsVoiceFile(true))
                        {
                            Message_Feed_Out("選択されたbnkファイルは音声データではありません。");
                            IsBusy = false;
                            return;
                        }
                        p.Get_Voices(BNK_FSB_Voices);
                        int Voice_Count = 0;
                        foreach (Type_Setting Types in BNK_FSB_Voices.Types.Values)
                            Voice_Count += Types.Voices.Count;
                        if (Voice_Count == 0)
                        {
                            Message_T.Text = "移植できるファイルが見つからなかったため、特殊な方法で解析しています...";
                            await Task.Delay(50);
                            p.SpecialBNKFileMode = 1;
                            p.Get_Voices(BNK_FSB_Voices);
                            foreach (Type_Setting Types in BNK_FSB_Voices.Types.Values)
                                Voice_Count += Types.Voices.Count;
                            if (Voice_Count == 0)
                            {
                                p.Clear();
                                BNK_FSB_Voices.Clear();
                                Message_Feed_Out("移植できる音声が見つかりませんでした。");
                                IsBusy = false;
                                return;
                            }
                        }
                        p.Clear();
                        BNK_FSB_Voices.Wwise_BNK = new Wwise_File_Extract_V2(ofd.FileName);
                        List<uint> BGM_ShortIDs = new List<uint>();
                        foreach (Type_Setting Types in BNK_FSB_Voices.Types.Values)
                        {
                            if (Types.Type_Index == 23)
                            {
                                for (int i = 0; i < Types.Voices.Count; i++)
                                {
                                    if (BNK_FSB_Voices.Wwise_BNK.Wwise_Get_Sound_Length(Types.Voices[i].File_ID) >= 60)
                                    {
                                        BGM_ShortIDs.Add(Types.Voices[i].File_ID);
                                        Types.Voices.RemoveAt(i);
                                        i--;
                                    }
                                }
                            }
                        }
                        foreach (uint BGM_ID in BGM_ShortIDs)
                            BNK_FSB_Voices.Add_Voice(863608586, BGM_ID, 34);
                        Content_L.Items.Clear();
                        Now_Stream_Count = 0;
                        Max_Stream_Count = BNK_FSB_Voices.Wwise_BNK.Wwise_Get_Numbers();
                        File_Name_T.Text = Path.GetFileName(ofd.FileName);
                        Message_Feed_Out(".bnkファイルをロードしました。SEが含まれている場合は無効化してください。");
                        BNK_FSB_Voices.IsBNKMode = true;
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
                        Max_Stream_Count = Fmod_Class.FSB_GetLength(ofd.FileName);
                        Message_T.Text = "ファイル名からリストに配置しています...";
                        await Task.Delay(50);
                        Voice_Set.Voice_BGM_Name_Change_From_FSB_To_Index_FSBFile(BNK_FSB_Voices, ofd.FileName);
                        Content_L.Items.Clear();
                        Now_Stream_Count = 0;
                        File_Name_T.Text = Path.GetFileName(ofd.FileName);
                        BNK_FSB_Voices.FSB_File = ofd.FileName;
                        Message_Feed_Out(".fsbファイルをロードしました。");
                        BNK_FSB_Voices.IsBNKMode = false;
                    }
                    /*if (BNK_FSB_Voices[5].Count == 0 && BNK_FSB_Voices[2].Count > 0)
                        BNK_FSB_Voices[5] = new List<string>(BNK_FSB_Voices[2]);
                    if (BNK_FSB_Voices[4].Count == 0 && BNK_FSB_Voices[3].Count > 0)
                        BNK_FSB_Voices[4] = new List<string>(BNK_FSB_Voices[3]);*/
                    Get_Available_Voice_Count();
                    Set_Voice_Types();
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
        void Set_Voice_Types()
        {
            IsModeChanging = true;
            int Selected_Index = Voices_L.SelectedIndex;
            Voices_L.Items.Clear();
            for (int Number = 0; Number < 34; Number++)
            {
                int Count = 0;
                foreach (Type_Setting Types in BNK_FSB_Voices.Types.Values.Where(h => h.Type_Index == Number))
                    if (Types.IsEnable)
                        Count += Types.Voices.Where(h => h.IsEnable).Count();
                Voices_L.Items.Add(Voice_Set.Get_Voice_Type_Japanese_Name_V2(Number) + " : " + Count + "個");
            }
            int Count_BGM = 0;
            foreach (Type_Setting Types in BNK_FSB_Voices.Types.Values.Where(h => h.Type_Index == 34))
                if (Types.IsEnable)
                    Count_BGM += Types.Voices.Where(h => h.IsEnable).Count();
            Voices_L.Items.Add("戦闘BGM : " + Count_BGM + "個");
            Voices_L.SelectedIndex = Selected_Index;
            IsModeChanging = false;
        }
        //反映される音声の数を取得
        void Get_Available_Voice_Count()
        {
            Now_Stream_Count = 0;
            foreach (Type_Setting Types in BNK_FSB_Voices.Types.Values)
                if (Types.IsEnable)
                    Now_Stream_Count += Types.Voices.Where(h => h.IsEnable).Count();
        }
        private void Volume_Set_C_Click(object sender, RoutedEventArgs e)
        {
            Configs_Save();
        }
        private void Details_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            if (Now_Stream_Count == 0)
            {
                Message_Feed_Out("音声ファイルが指定されていないため、表示できる情報がありません。");
                return;
            }
            Get_Available_Voice_Count();
            Message_Feed_Out("ファイル内の音声ファイル数:" + Max_Stream_Count + "\n" + "移植後の音声数:" + Now_Stream_Count);
        }
        private void Volume_Set_Help_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            string Message_01 = "・この機能はV1.5.4で新しくなりました。まだ調整中なため、正確に調整されない可能性があります。\n";
            string Message_02 = "・特殊な方法で音量を調整しているため、この機能により処理時間が増えることはありません。";
            System.Windows.MessageBox.Show(Message_01 + Message_02);
        }
        private async void Start_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy || IsOpenDialog)
                return;
            if (Now_Stream_Count == 0)
            {
                Message_Feed_Out("音声ファイルが選択されていません。");
                return;
            }
            IsOpenDialog = true;
            BetterFolderBrowser fbd = new BetterFolderBrowser()
            {
                Title = "保存先を選択してください。",
                Multiselect = false,
                RootFolder = Sub_Code.Get_OpenDirectory_Path()
            };
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                IsBusy = true;
                //string To_Dir = Voice_Set.Special_Path + "/Wwise/BNK_WAV_WoT";
                string SetPath = fbd.SelectedFolder;
                Sub_Code.Set_Directory_Path(SetPath);
                if (!Sub_Code.CanDirectoryAccess(SetPath))
                {
                    Message_Feed_Out("指定したフォルダにアクセスできませんでした。");
                    IsBusy = false;
                    IsOpenDialog = false;
                    return;
                }
                try
                {
                    Message_T.Opacity = 1;
                    IsMessageShowing = false;
                    /*string Log_01 = Sub_Code.Set_Voice_Type_Change_Name_By_Index(To_Dir, To_Dir + "/Voices", BNK_FSB_Voices, BNK_FSB_Enable);
                    if (Log_01 != "")
                    {
                        Message_Feed_Out("ファイルをコピーできませんでした。詳しくは\"Error_Log.txt\"を参照してください。");
                        Directory.Delete(To_Dir + "/Voices", true);
                        IsBusy = false;
                        IsOpenDialog = false;
                        return;
                    }*/
                    await BNK_Create_V2(SetPath);
                    Flash.Flash_Start();
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
                                    Directory.CreateDirectory(Voice_Set.WoT_Mod_Path + "/audioww");
                                if (File.Exists(Voice_Set.WoT_Mod_Path + "/audioww/voiceover.bnk") && !File.Exists(Voice_Set.WoT_Mod_Path + "/audioww/voiceover_bak.bnk"))
                                    File.Move(Voice_Set.WoT_Mod_Path + "/audioww/voiceover.bnk", Voice_Set.WoT_Mod_Path + "/audioww/voiceover_bak.bnk");
                                File.Copy(SetPath + "/voiceover.bnk", Voice_Set.WoT_Mod_Path + "/audioww/voiceover.bnk", true);
                            }
                            catch (Exception e1)
                            {
                                Sub_Code.Error_Log_Write(e1.Message);
                                Message_Feed_Out("WoTにインストールできませんでした。Error_Log.txtを確認してください。");
                                IsBusy = false;
                                fbd.Dispose();
                                IsOpenDialog = false;
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
            IsOpenDialog = false;
        }
        //Voice_Create.xaml.csから引用&若干の修正
        async Task BNK_Create_V2(string Save_Dir)
        {
            if (!Directory.Exists(Save_Dir))
                return;
            Message_T.Text = "WEMファイルを抽出しています...";
            await Task.Delay(50);
            if (BNK_FSB_Voices.IsBNKMode)
            {
                if (Directory.Exists(Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod/.cache/Windows_HighRes/Voices/Japanese"))
                    Directory.Delete(Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod/.cache/Windows_HighRes/Voices/Japanese", true);
                Directory.CreateDirectory(Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod/.cache/Windows_HighRes/Voices/Japanese");
                BNK_FSB_Voices.Wwise_BNK.Wwise_Extract_To_WEM_Directory(BNK_FSB_Voices, Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod/.cache/Windows_HighRes/Voices/Japanese");
            }
            Message_T.Text = "プロジェクトファイルを作成しています...";
            await Task.Delay(50);
            FileInfo fi = new FileInfo(Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.wwu");
            if (File.Exists(Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp") && fi.Length >= 200000)
                File.Copy(Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp", Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.wwu", true);
            if (!File.Exists(Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp"))
                File.Copy(Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.wwu", Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp", true);
            Wwise_Class.Wwise_Project_Create Wwise = new Wwise_Class.Wwise_Project_Create(Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod");
            Wwise.Sound_Add_Wwise(BNK_FSB_Voices, Volume_Set_C.IsChecked.Value);
            Wwise.Save();
            Message_T.Text = "voiceover.bnkを作成しています...";
            await Task.Delay(50);
            Wwise.Project_Build("voiceover", Save_Dir + "/voiceover.bnk", "WinHighRes", true, "Japanese", Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod/.cache/Windows_HighRes/Voices/Japanese");
            Wwise.Clear(true, "Windows_HighRes");
            if (File.Exists(Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp"))
                File.Copy(Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp", Voice_Set.Special_Path + "/Wwise/WoT_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.wwu", true);
        }
        private void Voice_Enable_B_Click(object sender, RoutedEventArgs e)
        {
            int SelectedIndex = Content_L.SelectedIndex;
            if (SelectedIndex == -1 || Voices_L.SelectedIndex == -1)
                return;
            uint Container_ID = Get_Container_By_Content();
            if (Container_ID == 0)
            {
                Message_Feed_Out("コンテナIDが見つかりませんでした。");
                return;
            }
            int Index = Get_Content_Index();
            IsModeChanging = true;
            if (Index == -1 && !BNK_FSB_Voices.Types[Container_ID].IsEnable)
            {
                BNK_FSB_Voices.Types[Container_ID].IsEnable = true;
                Voices_L_SelectionChanged(null, null);
                Content_L.SelectedIndex = SelectedIndex;
            }
            else if (!BNK_FSB_Voices.Types[Container_ID].Voices[Index].IsEnable)
            {
                BNK_FSB_Voices.Types[Container_ID].Voices[Index].IsEnable = true;
                ListBoxItem Selected_Item = Content_L.SelectedItem as ListBoxItem;
                string Get_Name = Selected_Item.Content as string;
                Selected_Item.Content = Get_Name.Substring(0, Get_Name.LastIndexOf('|') + 1) + "有効";
                if (BNK_FSB_Voices.Types[Container_ID].IsEnable)
                    Selected_Item.Foreground = Brushes.Aqua;
            }
            IsModeChanging = false;
        }
        private void Voice_Disable_B_Click(object sender, RoutedEventArgs e)
        {
            int SelectedIndex = Content_L.SelectedIndex;
            if (SelectedIndex == -1 || Voices_L.SelectedIndex == -1)
                return;
            uint Container_ID = Get_Container_By_Content();
            if (Container_ID == 0)
            {
                Message_Feed_Out("コンテナIDが見つかりませんでした。");
                return;
            }
            int Index = Get_Content_Index();
            IsModeChanging = true;
            if (Index == -1 && BNK_FSB_Voices.Types[Container_ID].IsEnable)
            {
                BNK_FSB_Voices.Types[Container_ID].IsEnable = false;
                Voices_L_SelectionChanged(null, null);
                Content_L.SelectedIndex = SelectedIndex;
            }
            else if (Index != -1 && BNK_FSB_Voices.Types[Container_ID].Voices[Index].IsEnable)
            {
                BNK_FSB_Voices.Types[Container_ID].Voices[Index].IsEnable = false;
                ListBoxItem Selected_Item = Content_L.SelectedItem as ListBoxItem;
                string Get_Name = Selected_Item.Content as string;
                Selected_Item.Content = Get_Name.Substring(0, Get_Name.LastIndexOf('|') + 1) + "無効";
                Selected_Item.Foreground = Gray_Color;
            }
        }
        private void Voices_L_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Content_L.Items.Clear();
            if (Voices_L.SelectedIndex == -1 || Voices_L.Items.Count == 1)
                return;
            Container_Indexes.Clear();
            if (!IsModeChanging)
                Pause_Volume_Animation(true, 5f);
            foreach (Type_Setting Types in BNK_FSB_Voices.Types.Values.Where(h => h.Type_Index == Voices_L.SelectedIndex))
            {
                ListBoxItem LBI_Container = new ListBoxItem();
                LBI_Container.Content = "コンテナ->" + Types.Container_ID + "|" + (Types.IsEnable ? "有効" : "無効");
                LBI_Container.Foreground = Types.IsEnable ? Brushes.Aqua : Gray_Color;
                System.Windows.Controls.ContextMenu pMenu_Container = new System.Windows.Controls.ContextMenu();
                System.Windows.Controls.MenuItem item1_Container = new System.Windows.Controls.MenuItem();
                System.Windows.Controls.MenuItem item2_Container = new System.Windows.Controls.MenuItem();
                System.Windows.Controls.MenuItem item3_Container = new System.Windows.Controls.MenuItem();
                System.Windows.Controls.MenuItem item4_Container = new System.Windows.Controls.MenuItem();
                item1_Container.Header = "コピー";
                item1_Container.Click += delegate
                {
                    Container_Copy_ID = Types.Parent_Unique_ID;
                    Voice_Copy = null;
                    IsCutMode = false;
                };
                item2_Container.Header = "切り取り";
                item2_Container.Click += delegate
                {
                    Container_Copy_ID = Types.Parent_Unique_ID;
                    Voice_Copy = null;
                    IsCutMode = true;
                };
                item3_Container.Header = "貼り付け";
                item3_Container.Click += delegate
                {
                    Container_OR_Voice_Copy(Types.Type_Index, Types.Parent_Unique_ID);
                };
                item4_Container.Header = "削除";
                item4_Container.Click += delegate
                {
                    string Message_01 = "コンテナ(ID:" + Types.Container_ID + ")を削除しますか?\n";
                    string Message_02 = "※コンテナ内の子音声も削除されます。この操作は取り消せません。";
                    MessageBoxResult Result = System.Windows.MessageBox.Show(Message_01 + Message_02, "確認", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                    if (Result == MessageBoxResult.Yes)
                    {
                        if (Container_Copy_ID == Types.Parent_Unique_ID)
                        {
                            Container_Copy_ID = 0;
                            Voice_Copy = null;
                            IsCutMode = false;
                        }
                        BNK_FSB_Voices.Types.Remove(Types.Parent_Unique_ID);
                        Voices_L_SelectionChanged(null, null);
                        Message_Feed_Out("コンテナ(ID:" + Types.Container_ID + ")を削除しました。");
                    }
                };
                pMenu_Container.Opened += delegate
                {
                    item3_Container.IsEnabled = !(Container_Copy_ID == 0 && Voice_Copy == null);
                };
                pMenu_Container.Items.Add(item1_Container);
                pMenu_Container.Items.Add(item2_Container);
                pMenu_Container.Items.Add(item3_Container);
                pMenu_Container.Items.Add(item4_Container);
                LBI_Container.ContextMenu = pMenu_Container;
                Content_L.Items.Add(LBI_Container);
                Container_Indexes.Add(Content_L.Items.Count - 1);
                foreach (Voice_Setting Voices in Types.Voices)
                {
                    string Name_Temp = "  " + (Voices.File_ID == 0 ? Voices.File_Name.Substring(0, Voices.File_Name.LastIndexOf('_')) : Voices.File_ID.ToString());
                    if (Name_Temp.Length > 13)
                    {
                        if (Voices.IsEnable)
                            Name_Temp = Name_Temp.Substring(0, 16) + "...|有効";
                        else
                            Name_Temp = Name_Temp.Substring(0, 16) + "...|無効";
                    }
                    else
                    {
                        if (Voices.IsEnable)
                            Name_Temp += "|有効";
                        else
                            Name_Temp += "|無効";
                    }
                    ListBoxItem LBI = new ListBoxItem();
                    LBI.Content = Name_Temp;
                    LBI.Foreground = Voices.IsEnable && Types.IsEnable ? Brushes.Aqua : Gray_Color;
                    System.Windows.Controls.ContextMenu pMenu = new System.Windows.Controls.ContextMenu();
                    System.Windows.Controls.MenuItem item1 = new System.Windows.Controls.MenuItem();
                    System.Windows.Controls.MenuItem item2 = new System.Windows.Controls.MenuItem();
                    System.Windows.Controls.MenuItem item3 = new System.Windows.Controls.MenuItem();
                    System.Windows.Controls.MenuItem item4 = new System.Windows.Controls.MenuItem();
                    item1.Header = "コピー";
                    item1.Click += delegate
                    {
                        Container_Copy_ID = Types.Parent_Unique_ID;
                        Voice_Copy = Voices;
                        IsCutMode = false;
                    };
                    item2.Header = "切り取り";
                    item2.Click += delegate
                    {
                        Container_Copy_ID = Types.Parent_Unique_ID;
                        Voice_Copy = Voices;
                        IsCutMode = true;
                    };
                    item3.Header = "貼り付け";
                    item3.Click += delegate
                    {
                        Container_OR_Voice_Copy(Types.Type_Index, Types.Parent_Unique_ID);
                    };
                    item4.Header = "削除";
                    item4.Click += delegate
                    {
                        string ID_Name = Voices.File_ID == 0 ? Voices.File_Name : Voices.File_ID.ToString();
                        string Message_01 = "アイテム(ID:" + ID_Name + ")を削除しますか?\n";
                        string Message_02 = "※この操作は取り消せません。";
                        MessageBoxResult Result = System.Windows.MessageBox.Show(Message_01 + Message_02, "確認", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                        if (Result == MessageBoxResult.Yes)
                        {
                            if (Voice_Copy == Voices)
                            {
                                Container_Copy_ID = 0;
                                Voice_Copy = null;
                                IsCutMode = false;
                            }
                            BNK_FSB_Voices.Types[Types.Parent_Unique_ID].Voices.Remove(Voices);
                            Voices_L_SelectionChanged(null, null);
                            Message_Feed_Out("アイテム(ID:" + ID_Name + ")を削除しました。");
                        }
                    };
                    pMenu.Opened += delegate
                    {
                        item3.IsEnabled = !(Container_Copy_ID == 0 && Voice_Copy == null);
                    };
                    pMenu.Items.Add(item1);
                    pMenu.Items.Add(item2);
                    pMenu.Items.Add(item3);
                    pMenu.Items.Add(item4);
                    LBI.ContextMenu = pMenu;
                    Content_L.Items.Add(LBI);
                }
            }
            if (Content_L.Items.Count > 0 && !IsModeChanging)
                Content_L.ScrollIntoView(Content_L.Items[0]);
        }
        private void Container_OR_Voice_Copy(int To_Type_Index, uint To_Container_ID)
        {
            bool IsChanged = false;
            if (Voice_Copy != null && Container_Copy_ID != 0 && To_Container_ID != 0)
            {
                BNK_FSB_Voices.Add_Voice(BNK_FSB_Voices.Types[To_Container_ID], Voice_Copy);
                if (IsCutMode)
                {
                    BNK_FSB_Voices.Types[Container_Copy_ID].Voices.Remove(Voice_Copy);
                    Container_Copy_ID = 0;
                    Voice_Copy = null;
                    IsCutMode = false;
                    Message_Feed_Out("コンテナ(ID:" + To_Container_ID + ")にアイテム(ID:" + Voice_Copy.File_ID + ")を移動しました。");
                }
                else
                    Message_Feed_Out("コンテナ(ID:" + To_Container_ID + ")にアイテム(ID:" + Voice_Copy.File_ID + ")をコピーしました。");
                IsChanged = true;
            }
            else if (Container_Copy_ID != 0)
            {
                foreach (Type_Setting Types in BNK_FSB_Voices.Types.Values)
                {
                    if (Types.Type_Index == To_Type_Index && Types.Container_ID == BNK_FSB_Voices.Types[Container_Copy_ID].Container_ID)
                    {
                        Message_Feed_Out("既に同じIDのコンテナが含まれているためコピーできませんでした。");
                        return;
                    }
                }
                Type_Setting From_Type = BNK_FSB_Voices.Types[Container_Copy_ID];
                foreach (Voice_Setting Voices in From_Type.Voices)
                    BNK_FSB_Voices.Add_Voice(From_Type.Container_ID, Voices.File_ID, To_Type_Index);
                if (IsCutMode)
                {
                    BNK_FSB_Voices.Types.Remove(Container_Copy_ID);
                    Message_Feed_Out("コンテナ(ID:" + Container_Copy_ID + ")を移動しました。");
                    Container_Copy_ID = 0;
                    Voice_Copy = null;
                    IsCutMode = false;
                }
                else
                    Message_Feed_Out("コンテナ(ID:" + Container_Copy_ID + ")をコピーしました。");
                IsChanged = true;
            }
            if (IsChanged)
            {
                IsModeChanging = true;
                Voices_L_SelectionChanged(null, null);
                Set_Voice_Types();
                IsModeChanging = false;
            }
        }
        private async void Content_L_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (Content_L.SelectedIndex == -1)
                return;
            if (IsModeChanging)
            {
                IsModeChanging = false;
                return;
            }
            uint Container_ID = Get_Container_By_Content();
            if (Container_ID == 0)
            {
                Message_Feed_Out("コンテナIDが見つかりませんでした。");
                return;
            }
            int Content_Index = Get_Content_Index();
            if (Content_Index == -1)
                return;
            string Temp_File = Sub_Code.File_Get_FileName_No_Extension(Voice_Set.Special_Path + "/Wwise/Blitz_To_WoT_Temp");
            bool IsNotExtractMode = false;
            uint Selected_Sound_ID = WwiseHash.HashString(BNK_FSB_Voices.Types[Container_ID].Container_ID + "_" + BNK_FSB_Voices.Types[Container_ID].Type_Index + "_" + (BNK_FSB_Voices.IsBNKMode
                ? BNK_FSB_Voices.Types[Container_ID].Voices[Content_Index].File_ID.ToString() : BNK_FSB_Voices.Types[Container_ID].Voices[Content_Index].FSB_Index.ToString()));
            if (File.Exists(Temp_File))
            {
                if (Now_Playing_File_ID == Selected_Sound_ID)
                    IsNotExtractMode = true;
                else
                {
                    Bass.BASS_ChannelStop(Stream);
                    Bass.BASS_StreamFree(Stream);
                    File.Delete(Temp_File);
                }
            }
            if (!IsNotExtractMode)
            {
                Message_T.Text = "サウンドを抽出しています...";
                await Task.Delay(50);
                if (BNK_FSB_Voices.IsBNKMode)
                {
                    uint ShortID = BNK_FSB_Voices.Types[Container_ID].Voices[Content_Index].File_ID;
                    BNK_FSB_Voices.Wwise_BNK.Wwise_Extract_To_OGG_OR_WAV_File(ShortID, Voice_Set.Special_Path + "/Wwise/Blitz_To_WoT_Temp", true);
                }
                else
                {
                    int Index = BNK_FSB_Voices.Types[Container_ID].Voices[Content_Index].FSB_Index;
                    Fmod_File_Extract_V1.FSB_Extract_To_File(BNK_FSB_Voices.FSB_File, Index, Voice_Set.Special_Path + "/Wwise/Blitz_To_WoT_Temp.mp3");
                }
                if (!File.Exists(Temp_File))
                {
                    Message_Feed_Out("ファイルが見つかりませんでした。");
                    return;
                }
                Load_Sound(Temp_File);
            }
            Message_T.Text = "";
            Now_Playing_File_ID = Selected_Sound_ID;
        }
        private void Help_B_Click(object sender, RoutedEventArgs e)
        {
            string Message_01 = "WoTBの音声ModをPC版WoT用に移植します。\n";
            string Message_02 = ".bnkファイルを指定した場合は、SEが含まれている可能性が高いため、各イベントを確認し、あれば無効化することをおススメします。\n";
            string Message_03 = "低ティア&数戦しかテストしていないため、正常に再生されないイベントがあるかもしれません。";
            System.Windows.MessageBox.Show(Message_01 + Message_02 + Message_03);
        }
        private int Get_Content_Index()
        {
            foreach (int i in Container_Indexes)
                if (i == Content_L.SelectedIndex)
                    return -1;
            List<int> a = Container_Indexes.Where(h => h < Content_L.SelectedIndex).ToList();
            return Content_L.SelectedIndex - a[a.Count - 1] - 1;
        }
        private uint Get_Container_By_Content()
        {
            for (int i = Content_L.SelectedIndex; i >= 0; i--)
            {
                string Content_Name = "";
                if (Content_L.Items[i] is ListBoxItem Item)
                    Content_Name = (string)Item.Content;
                else if (Content_L.Items[i] is string Value)
                    Content_Name = Value;
                if (Content_Name.StartsWith("コンテナ->"))
                {
                    string Temp = Content_Name.Substring(0, Content_Name.LastIndexOf('|'));
                    uint Container_ID =  uint.Parse(Temp.Substring(Temp.IndexOf("->") + 2));
                    IEnumerable<Type_Setting> t = BNK_FSB_Voices.Types.Values.Where(h => h.Type_Index == Voices_L.SelectedIndex);
                    foreach (Type_Setting Types in t)
                        if (Types.Container_ID == Container_ID)
                            return Types.Parent_Unique_ID;
                }
            }
            return 0;
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
                    Location_S.Value = 0;
                    Location_S.Maximum = 0;
                    Location_T.Text = "00:00";
                }
                else
                    Bass.BASS_ChannelPause(Stream);
            }
        }
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
            Location_S.Value = Bass.BASS_ChannelBytes2Seconds(Stream, position2);
        }
        //+5秒
        private void Plus_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
                return;
            long position = Bass.BASS_ChannelGetPosition(Stream);
            if (Bass.BASS_ChannelBytes2Seconds(Stream, position) + 5 > Location_S.Maximum)
                Music_Pos_Change(Location_S.Maximum, true);
            else
                Music_Pos_Change(Bass.BASS_ChannelBytes2Seconds(Stream, position) + 5, true);
            long position2 = Bass.BASS_ChannelGetPosition(Stream);
            Location_S.Value = Bass.BASS_ChannelBytes2Seconds(Stream, position2);
        }
    }
}