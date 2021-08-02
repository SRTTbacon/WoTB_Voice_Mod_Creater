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
using WoTB_Voice_Mod_Creater.Class;

namespace WoTB_Voice_Mod_Creater.Wwise_Class
{
    public partial class BNK_Event : UserControl
    {
        int Stream;
        bool IsClosing = false;
        bool IsBusy = false;
        bool IsMessageShowing = false;
        bool IsLocationChanging = false;
        bool IsPaused = false;
        bool IsPCKMode = false;
        bool IsOpenDialog = false;
        Wwise_Class.BNK_Parse p;
        Wwise_File_Extract_V1 Wwise_PCK;
        Wwise_File_Extract_V2 Wwise_BNK;
        public BNK_Event()
        {
            InitializeComponent();
            Location_S.AddHandler(MouseDownEvent, new MouseButtonEventHandler(Location_MouseDown), true);
            Location_S.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Location_MouseUp), true);
            Volume_S.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Volume_MouseUp), true);
            Location_S.Maximum = 0;
        }
        public async void Window_Show()
        {
            Opacity = 0;
            Visibility = Visibility.Visible;
            Sub_Code.File_Decrypt(Voice_Set.Special_Path + "/Configs/BNK_Event.conf", Voice_Set.Special_Path + "/Configs/BNK_Event.tmp", "BNK_Event_Configs_Save", false);
            StreamReader str = new StreamReader(Voice_Set.Special_Path + "/Configs/BNK_Event.tmp");
            Volume_S.Value = double.Parse(str.ReadLine());
            PCK_Mode_C.IsChecked = bool.Parse(str.ReadLine());
            str.Close();
            File.Delete(Voice_Set.Special_Path + "/Configs/BNK_Event.tmp");
            while (Opacity < 1 && !IsClosing)
            {
                Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
            Position_Change();
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
        private async void Exit_B_Click(object sender, RoutedEventArgs e)
        {
            if (!IsClosing && !IsBusy)
            {
                IsClosing = true;
                Bass.BASS_ChannelPause(Stream);
                while (Opacity > 0)
                {
                    Opacity -= Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
                IsClosing = false;
                Visibility = Visibility.Hidden;
            }
        }
        private async void Open_BNK_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog()
            {
                Title = ".bnkファイルを選択してください。",
                Multiselect = false,
                Filter = ".bnkファイル(*.bnk)|*.bnk"
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    IsBusy = true;
                    string PCK_File_Path = Path.GetDirectoryName(ofd.FileName) + "/" + Path.GetFileNameWithoutExtension(ofd.FileName) + ".pck";
                    if (PCK_Mode_C.IsChecked.Value && !File.Exists(PCK_File_Path))
                    {
                        Message_Feed_Out(Path.GetFileNameWithoutExtension(ofd.FileName) + ".pckが見つかりませんでした。");
                        IsBusy = false;
                        return;
                    }
                    Bass.BASS_ChannelStop(Stream);
                    Bass.BASS_StreamFree(Stream);
                    if (Directory.Exists(Voice_Set.Special_Path + "/Wwise/BNK_WAV_Special"))
                        Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/Wwise/BNK_WAV_Special");
                    Message_T.Text = ".bnkファイルを解析しています...";
                    await Task.Delay(50);
                    p = new Wwise_Class.BNK_Parse(ofd.FileName);
                    List<uint> Event_List = p.Get_BNK_Event_ID();
                    Event_Type_L.Items.Clear();
                    Sound_L.Items.Clear();
                    foreach (uint ID in Event_List)
                        Event_Type_L.Items.Add((Event_Type_L.Items.Count + 1) + ":" + ID);
                    Event_List.Clear();
                    if (PCK_Mode_C.IsChecked.Value)
                    {
                        Wwise_PCK = new Wwise_File_Extract_V1(PCK_File_Path);
                        IsPCKMode = true;
                    }
                    else
                    {
                        Wwise_BNK = new Wwise_File_Extract_V2(ofd.FileName);
                        IsPCKMode = false;
                    }
                    if (!Directory.Exists(Voice_Set.Special_Path + "/Wwise/BNK_WAV_Special"))
                        Directory.CreateDirectory(Voice_Set.Special_Path + "/Wwise/BNK_WAV_Special");
                    File_Name_T.Text = Path.GetFileName(ofd.FileName);
                    Message_Feed_Out(".bnkファイルをロードしました。");
                }
                catch (Exception e1)
                {
                    Sub_Code.Error_Log_Write(e1.Message);
                    if (Wwise_PCK != null)
                        Wwise_PCK.Pck_Clear();
                    if (Wwise_BNK != null)
                        Wwise_BNK.Bank_Clear();
                    if (p != null)
                        p.Clear();
                    Wwise_PCK = null;
                    Wwise_BNK = null;
                    p = null;
                    IsPCKMode = false;
                    File_Name_T.Text = "";
                    Event_Type_L.Items.Clear();
                    Sound_L.Items.Clear();
                    Message_Feed_Out("エラーが発生しました。詳しくはError_Log.txtを参照してください。");
                }
            }
            IsBusy = false;
        }
        private void Event_Type_L_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Event_Type_L.SelectedIndex == -1 || p == null)
                return;
            Sound_L.Items.Clear();
            Bass.BASS_ChannelStop(Stream);
            Bass.BASS_StreamFree(Stream);
            string Name = Event_Type_L.Items[Event_Type_L.SelectedIndex].ToString();
            uint Event_ID = uint.Parse(Name.Substring(Name.IndexOf(':') + 1));
            foreach (uint IDs in p.Get_Sounds_From_EventID(Event_ID))
                Sound_L.Items.Add(IDs);
            if (Sound_L.Items.Count == 0)
            {
                p.SpecialBNKFileMode = 1;
                foreach (uint IDs in p.Get_Sounds_From_EventID(Event_ID))
                    Sound_L.Items.Add(IDs);
            }
            p.SpecialBNKFileMode = 0;
        }
        void Load_Sound(string File_Name = "")
        {
            Location_S.Value = 0;
            Location_T.Text = "00:00";
            int StreamHandle = Bass.BASS_StreamCreateFile(File_Name, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_LOOP);
            Stream = BassFx.BASS_FX_TempoCreate(StreamHandle, BASSFlag.BASS_FX_FREESOURCE);
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)Volume_S.Value / 100);
            Bass.BASS_ChannelSetDevice(Stream, Video_Mode.Sound_Device);
            Location_S.Maximum = Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetLength(Stream, BASSMode.BASS_POS_BYTES));
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
            if (IsClosing || IsBusy)
                return;
            Bass.BASS_ChannelPlay(Stream, false);
            IsPaused = false;
        }
        private void Pause_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            Bass.BASS_ChannelPause(Stream);
            IsPaused = true;
        }
        void Volume_MouseUp(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Configs_Save();
        }
        private void PCK_Mode_C_Click(object sender, RoutedEventArgs e)
        {
            Configs_Save();
        }
        void Configs_Save()
        {
            try
            {
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Configs/BNK_Event.tmp");
                stw.WriteLine(Volume_S.Value);
                stw.Write(PCK_Mode_C.IsChecked.Value);
                stw.Close();
                Sub_Code.File_Encrypt(Voice_Set.Special_Path + "/Configs/BNK_Event.tmp", Voice_Set.Special_Path + "/Configs/BNK_Event.conf", "BNK_Event_Configs_Save", true);
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                Message_Feed_Out("設定を保存できませんでした。");
            }
        }
        private async void Sound_L_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Sound_L.SelectedIndex == -1 || IsClosing || IsBusy)
                return;
            Bass.BASS_ChannelStop(Stream);
            Bass.BASS_StreamFree(Stream);
            Message_T.Text = "ファイルを変換しています...";
            await Task.Delay(50);
            string To_File = Voice_Set.Special_Path + "/Wwise/BNK_WAV_Special/" + Sound_L.Items[Sound_L.SelectedIndex];
            if (IsPCKMode)
                Wwise_PCK.Wwise_Extract_To_Ogg_File(uint.Parse(Sound_L.Items[Sound_L.SelectedIndex].ToString()), To_File + ".ogg", true);
            else
                Wwise_BNK.Wwise_Extract_To_Ogg_File(uint.Parse(Sound_L.Items[Sound_L.SelectedIndex].ToString()), To_File + ".ogg", true);
            Message_T.Text = "";
            string File_Name = Sub_Code.File_Get_FileName_No_Extension(To_File);
            Load_Sound(File_Name);
            Bass.BASS_ChannelPlay(Stream, false);
            IsPaused = false;
        }
        private void Event_ID_Search_B_Click(object sender, RoutedEventArgs e)
        {
            if (Event_ID_Search_T.Text == "" || IsClosing || IsBusy)
                return;
            try
            {
                uint Get_Short_ID = WwiseHash.HashString(Event_ID_Search_T.Text);
                int ID_Number = -1;
                for (int Number = 0; Number < Event_Type_L.Items.Count; Number++)
                {
                    string Name = Event_Type_L.Items[Number].ToString();
                    uint Event_ID = uint.Parse(Name.Substring(Name.IndexOf(':') + 1));
                    if (Event_ID == Get_Short_ID)
                        ID_Number = Number;
                }
                if (ID_Number == -1)
                    Message_Feed_Out("ShortID : " + WwiseHash.HashString(Event_ID_Search_T.Text) + "\n取得したIDは、イベント内に含まれていませんでした。");
                else
                    Message_Feed_Out("ShortID : " + WwiseHash.HashString(Event_ID_Search_T.Text) + "\nイベント番号 : " + (ID_Number + 1));
            }
            catch
            {
                Message_Feed_Out("文字列をShortIDに変換できませんでした。");
            }
        }
        private async void Extract_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy || IsOpenDialog)
                return;
            if (Event_Type_L.SelectedIndex == -1)
            {
                Message_Feed_Out("イベントIDが選択されていません。");
                return;
            }
            if (Sound_L.Items.Count == 0)
            {
                Message_Feed_Out("イベント内にサウンドが存在しません。");
                return;
            }
            IsOpenDialog = true;
            BetterFolderBrowser bfb = new BetterFolderBrowser()
            {
                Title = "保存先のフォルダを選択してください。",
                RootFolder = Sub_Code.Get_OpenDirectory_Path(),
                Multiselect = false,
            };
            if (bfb.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Sub_Code.Set_Directory_Path(bfb.SelectedFolder);
                Message_T.Text = "イベント内のサウンドを抽出しています...";
                await Task.Delay(50);
                List<string> Extract_Sound_Files = new List<string>();
                for (int Number = 0; Number < Sound_L.Items.Count; Number++)
                {
                    string To_File = Voice_Set.Special_Path + "/Wwise/BNK_WAV_Special/" + Sound_L.Items[Number];
                    if (IsPCKMode)
                        Wwise_PCK.Wwise_Extract_To_Ogg_File(uint.Parse(Sound_L.Items[Number].ToString()), To_File + ".ogg", true);
                    else
                        Wwise_BNK.Wwise_Extract_To_Ogg_File(uint.Parse(Sound_L.Items[Number].ToString()), To_File + ".ogg", true);
                    if (File.Exists(To_File + ".ogg"))
                        Extract_Sound_Files.Add(To_File + ".ogg");
                }
                Message_T.Text = "ファイルをwavに変換しています...";
                await Multithread.Convert_To_Wav(Extract_Sound_Files.ToArray(), bfb.SelectedFolder, true, true);
                Message_Feed_Out("イベント内のファイルを抽出しました。");
            }
            bfb.Dispose();
            IsOpenDialog = false;
        }
        private void Clear_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            MessageBoxResult result = MessageBox.Show("内容をクリアしますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (Wwise_BNK != null)
                        Wwise_BNK.Bank_Clear();
                    if (Wwise_PCK != null)
                        Wwise_PCK.Pck_Clear();
                    if (p != null)
                        p.Clear();
                    Bass.BASS_ChannelStop(Stream);
                    Bass.BASS_StreamFree(Stream);
                    Event_Type_L.Items.Clear();
                    Sound_L.Items.Clear();
                    File_Name_T.Text = "";
                    if (Directory.Exists(Voice_Set.Special_Path + "/Wwise/BNK_WAV_Special"))
                        Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/Wwise/BNK_WAV_Special");
                    Message_Feed_Out("内容をクリアしました。");
                }
                catch (Exception e1)
                {
                    Sub_Code.Error_Log_Write(e1.Message);
                    Message_Feed_Out("正常にクリアできませんでした。");
                }
            }
        }
    }
}