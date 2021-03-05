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

namespace WoTB_Voice_Mod_Creater.Wwise_Class
{
    public partial class Bank_Editor : System.Windows.Controls.UserControl
    {
        int Stream;
        int SelectIndex = -2;
        float SetFirstFreq = 44100f;
        bool IsClosing = false;
        bool IsEnded = false;
        bool IsLocationChanging = false;
        bool IsPaused = false;
        bool IsMessageShowing = false;
        bool IsPCKFile = false;
        List<string> Change_Sound_Full_Name = new List<string>();
        SYNCPROC IsMusicEnd;
        Wwise_File_Extract_V1 Wwise_Pck;
        Wwise_File_Extract_V2 Wwise_Bnk;
        public Bank_Editor()
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
                    {
                        Minutes = "0" + Time.Minutes;
                    }
                    if (Time.Seconds < 10)
                    {
                        Seconds = "0" + Time.Seconds;
                    }
                    Location_T.Text = Minutes + ":" + Seconds;
                }
                await Task.Delay(100);
            }
        }
        void Location_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsClosing)
            {
                return;
            }
            IsLocationChanging = true;
            IsPaused = true;
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, 0f);
        }
        async void Location_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsClosing)
            {
                return;
            }
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
            {
                Minutes = "0" + Time.Minutes;
            }
            if (Time.Seconds < 10)
            {
                Seconds = "0" + Time.Seconds;
            }
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
                {
                    Message_T.Opacity -= 0.025;
                }
                await Task.Delay(1000 / 60);
            }
            IsMessageShowing = false;
            Message_T.Text = "";
            Message_T.Opacity = 1;
        }
        void EndSync(int handle, int channel, int data, IntPtr user)
        {
            if (!IsEnded)
            {
                IsEnded = true;
            }
        }
        private void Open_File_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (IsClosing)
            {
                return;
            }
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Title = "サウンドファイルを選択してください。",
                Filter = "サウンドファイル(*.bnk;*.pck)|*.bnk;*.pck",
                Multiselect = false
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string Ex = Path.GetExtension(ofd.FileName);
                Sound_List.Items.Clear();
                Change_List.Items.Clear();
                Change_Sound_Full_Name.Clear();
                if (Ex == ".bnk")
                {
                    Wwise_Bnk = new Wwise_File_Extract_V2(ofd.FileName);
                    foreach (string Name_ID in Wwise_Bnk.Wwise_Get_Names())
                    {
                        Sound_List.Items.Add((Sound_List.Items.Count + 1) + ":" + Name_ID);
                    }
                    IsPCKFile = false;
                }
                else if (Ex == ".pck")
                {
                    Wwise_Pck = new Wwise_File_Extract_V1(ofd.FileName);
                    foreach (string Name_ID in Wwise_Pck.Wwise_Get_Banks_ID())
                    {
                        Sound_List.Items.Add((Sound_List.Items.Count + 1) + ":" + Name_ID);
                    }
                    IsPCKFile = true;
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
                Change_List.SelectedIndex = -1;
                SelectIndex = -2;
                if (Not_Extract_C.IsChecked.Value)
                {
                    return;
                }
                Message_T.Opacity = 1;
                Message_T.Text = "サウンドファイルに変換しています...";
                await Task.Delay(50);
                if (IsPCKFile)
                {
                    if (Wwise_Pck.Wwise_Extract_To_Ogg_File(Sound_List.SelectedIndex, Voice_Set.Special_Path + "/Wwise/Temp_01.ogg", true))
                    {
                        Message_Feed_Out("変換しました。");
                    }
                    else
                    {
                        Message_Feed_Out("変換できませんでした。");
                    }
                }
                else
                {
                    if (Wwise_Bnk.Wwise_Extract_To_Ogg_File(Sound_List.SelectedIndex, Voice_Set.Special_Path + "/Wwise/Temp_01.ogg", true))
                    {
                        Message_Feed_Out("変換しました。");
                    }
                    else
                    {
                        Message_Feed_Out("変換できませんでした。");
                    }
                }
            }
        }
        private void Change_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsClosing)
            {
                return;
            }
            if (Change_List.SelectedIndex != -1)
            {
                string Sound_File = Change_Sound_Full_Name[Change_List.SelectedIndex];
                if (!File.Exists(Sound_File))
                {
                    int Number = Change_List.SelectedIndex;
                    Change_List.SelectedIndex = -1;
                    Change_Sound_Full_Name.RemoveAt(Number);
                    Change_List.Items.RemoveAt(Number);
                    Message_Feed_Out("選択されたファイルが存在しません。選択した項目は削除されます。");
                    return;
                }
                Sound_List.SelectedIndex = -1;
                SelectIndex = -2;
            }
        }
        private void Play_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (IsClosing)
            {
                return;
            }
            if (Sound_List.SelectedIndex == -1 && Change_List.SelectedIndex == -1)
            {
                return;
            }
            else if (Sound_List.SelectedIndex != -1 && !File.Exists(Voice_Set.Special_Path + "/Wwise/Temp_01.ogg"))
            {
                Message_Feed_Out("サウンドファイルが変換されませんでした。");
                return;
            }
            else if (Change_List.SelectedIndex != -1 && !File.Exists(Change_Sound_Full_Name[Change_List.SelectedIndex]))
            {
                Message_Feed_Out("選択されたファイルが存在しません。");
                return;
            }
            if (SelectIndex == Sound_List.SelectedIndex || SelectIndex == Change_List.SelectedIndex)
            {
                Bass.BASS_ChannelPlay(Stream, false);
            }
            else
            {
                Bass.BASS_ChannelStop(Stream);
                Location_S.Value = 0;
                Bass.BASS_StreamFree(Stream);
                if (Sound_List.SelectedIndex != -1)
                {
                    int StreamHandle = Bass.BASS_StreamCreateFile(Voice_Set.Special_Path + "/Wwise/Temp_01.ogg", 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_LOOP);
                    Stream = BassFx.BASS_FX_TempoCreate(StreamHandle, BASSFlag.BASS_FX_FREESOURCE);
                    SelectIndex = Sound_List.SelectedIndex;
                }
                else if (Change_List.SelectedIndex != -1)
                {
                    int StreamHandle = Bass.BASS_StreamCreateFile(Change_Sound_Full_Name[Change_List.SelectedIndex], 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_LOOP);
                    Stream = BassFx.BASS_FX_TempoCreate(StreamHandle, BASSFlag.BASS_FX_FREESOURCE);
                    SelectIndex = Change_List.SelectedIndex;
                }
                else
                {
                    Message_Feed_Out("エラーが発生しました。");
                    return;
                }
                IsMusicEnd = new SYNCPROC(EndSync);
                Bass.BASS_ChannelSetDevice(Stream, -1);
                Bass.BASS_ChannelSetSync(Stream, BASSSync.BASS_SYNC_END | BASSSync.BASS_SYNC_MIXTIME, 0, IsMusicEnd, IntPtr.Zero);
                Bass.BASS_ChannelPlay(Stream, true);
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)Volume_S.Value / 100);
                Bass.BASS_ChannelGetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, ref SetFirstFreq);
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, SetFirstFreq + (float)Speed_S.Value);
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
        private void Change_File_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
            {
                return;
            }
            try
            {
                if (Sound_List.SelectedIndex == -1)
                {
                    Message_Feed_Out("ファイルが選択されていません。");
                    return;
                }
                OpenFileDialog ofd = new OpenFileDialog()
                {
                    Title = "サウンドファイルを選択してください。",
                    Filter = "サウンドファイル(*.mp3;*.wav;*.ogg;*.flac;*.aac;*.wma)|*.mp3;*.wav;*.ogg;*.flac;*.aac;*.wma",
                    Multiselect = false
                };
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    int Number_01 = -1;
                    for (int Number = 0; Number < Change_List.Items.Count; Number++)
                    {
                        if (Change_List.Items[Number] == Sound_List.SelectedItem)
                        {
                            Number_01 = Number;
                        }
                    }
                    if (Number_01 == -1)
                    {
                        Change_List.Items.Add(Sound_List.SelectedItem);
                        Change_Sound_Full_Name.Add(ofd.FileName);
                    }
                    else
                    {
                        Change_Sound_Full_Name[Number_01] = ofd.FileName;
                    }
                }
            }
            catch (Exception e1)
            {
                Sub_Code.Error_Log_Write(e1.Message);
            }
        }
        private async void Save_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
            {
                return;
            }
            if (Change_List.Items.Count == 0)
            {
                Message_Feed_Out("変更点が見つかりませんでした。");
                return;
            }
            try
            {
                string Save_Ex;
                if (IsPCKFile)
                {
                    Save_Ex = "*.pck";
                }
                else
                {
                    Save_Ex = "*.bnk";
                }
                SaveFileDialog sfd = new SaveFileDialog()
                {
                    Title = "保存先を指定してください。",
                    Filter = "WoTBサウンドファイル(" + Save_Ex + ")|" + Save_Ex
                };
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Border_All.Visibility = Visibility.Visible;
                    if (Directory.Exists(Voice_Set.Special_Path + "/Wwise/Temp"))
                    {
                        Directory.Delete(Voice_Set.Special_Path + "/Wwise/Temp", true);
                    }
                    Directory.CreateDirectory(Voice_Set.Special_Path + "/Wwise/Temp");
                    int Number = 0;
                    IsMessageShowing = false;
                    Message_T.Opacity = 1;
                    foreach (string Name in Change_List.Items)
                    {
                        int Index = int.Parse(Name.Substring(0, Name.IndexOf(':'))) - 1;
                        Message_T.Text = (Index + 1) + "番目の項目をWEMファイルに変換しています...";
                        await Task.Delay(50);
                        Sub_Code.File_To_WEM(Change_Sound_Full_Name[Number], Voice_Set.Special_Path + "/Wwise/Temp/" + (Index + 1) + ".wem", true);
                        if (!IsPCKFile)
                        {
                            Message_T.Text = (Index + 1) + "番目の項目をBNKファイルに差し替えています...";
                            await Task.Delay(50);
                            Wwise_Bnk.Bank_Edit_Sound(Index, Voice_Set.Special_Path + "/Wwise/Temp/" + (Index + 1) + ".wem", false);
                        }
                        Number++;
                    }
                    Message_T.Text = "BNKファイルを保存しています...";
                    await Task.Delay(50);
                    if (IsPCKFile)
                    {
                        Wwise_Pck.Wwise_PCK_Save(sfd.FileName, Voice_Set.Special_Path + "/Wwise/Temp", true);
                    }
                    else
                    {
                        Wwise_Bnk.Bank_Save(sfd.FileName);
                    }
                    Border_All.Visibility = Visibility.Hidden;
                    Message_Feed_Out("変更を保存しました。");
                    Directory.Delete(Voice_Set.Special_Path + "/Wwise/Temp", true);
                }
            }
            catch (Exception e1)
            {
                Sub_Code.Error_Log_Write(e1.Message);
                Message_Feed_Out("エラーが発生しました。");
                Border_All.Visibility = Visibility.Hidden;
            }
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
        private void Search_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
            {
                return;
            }
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
        Random r = new Random();
        private void Content_Save_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
            {
                return;
            }
            if (Sound_List.Items.Count == 0)
            {
                int r1 = r.Next(0, 11);
                if (r1 == 0)
                {
                    Message_Feed_Out("内容がないようです。");
                }
                else
                {
                    Message_Feed_Out("保存する内容がありません。");
                }
                return;
            }
            SaveFileDialog sfd = new SaveFileDialog()
            {
                Title = "保存先を指定してください。",
                Filter = ".wbeファイル(*.wbe)|*.wbe"
            };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Temp_Bank_Editor_Save.dat");
                    if (IsPCKFile)
                    {
                        stw.WriteLine(Wwise_Pck.Selected_PCK_File);
                    }
                    else
                    {
                        stw.WriteLine(Wwise_Bnk.Selected_BNK_File);
                    }
                    for (int Number = 0; Number < Change_List.Items.Count; Number++)
                    {
                        int Index = int.Parse(Change_List.Items[Number].ToString().Substring(0, Change_List.Items[Number].ToString().IndexOf(':'))) - 1;
                        stw.WriteLine(Index + ":" + Change_Sound_Full_Name[Number]);
                    }
                    stw.Close();
                    Sub_Code.File_Encrypt(Voice_Set.Special_Path + "/Temp_Bank_Editor_Save.dat", sfd.FileName, "Bank_Editor_Change_Sound_Save", true);
                    Message_Feed_Out("内容を保存しました。");
                }
                catch (Exception e1)
                {
                    Sub_Code.Error_Log_Write(e1.Message);
                    Message_Feed_Out("正常に保存できませんでした。");
                }
            }
        }
        private void Content_Load_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
            {
                return;
            }
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Title = ".wbeファイルを選択してください。",
                Filter = ".wbeファイル(*.wbe)|*.wbe"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Sub_Code.File_Decrypt(ofd.FileName, Voice_Set.Special_Path + "/Temp_Bank_Editor_Load.dat", "Bank_Editor_Change_Sound_Save", false);
                    StreamReader str = new StreamReader(Voice_Set.Special_Path + "/Temp_Bank_Editor_Load.dat");
                    string Open_File = str.ReadLine();
                    if (Path.GetExtension(Open_File) == ".bnk")
                    {
                        Sound_List.Items.Clear();
                        Wwise_Bnk = new Wwise_File_Extract_V2(Open_File);
                        foreach (string Name_ID in Wwise_Bnk.Wwise_Get_Names())
                        {
                            Sound_List.Items.Add((Sound_List.Items.Count + 1) + ":" + Name_ID);
                        }
                        IsPCKFile = false;
                    }
                    else if (Path.GetExtension(Open_File) == ".pck")
                    {
                        Sound_List.Items.Clear();
                        Wwise_Pck = new Wwise_File_Extract_V1(Open_File);
                        foreach (string Name_ID in Wwise_Pck.Wwise_Get_Banks_ID())
                        {
                            Sound_List.Items.Add((Sound_List.Items.Count + 1) + ":" + Name_ID);
                        }
                        IsPCKFile = true;
                    }
                    else
                    {
                        throw new Exception("ファイル形式が違います。対応しているファイル形式は.bnk、または.pckのみです。");
                    }
                    Change_List.Items.Clear();
                    Change_Sound_Full_Name.Clear();
                    string line;
                    while ((line = str.ReadLine()) != null)
                    {
                        int Index = int.Parse(line.Substring(0, line.IndexOf(':')));
                        string File_Name = line.Substring(line.IndexOf(':') + 1);
                        Change_List.Items.Add(Sound_List.Items[Index]);
                        Change_Sound_Full_Name.Add(File_Name);
                    }
                    str.Close();
                    File.Delete(Voice_Set.Special_Path + "/Temp_Bank_Editor_Load.dat");
                }
                catch (Exception e1)
                {
                    Sub_Code.Error_Log_Write(e1.Message);
                    Message_Feed_Out("正常に読み込めませんでした。");
                }
            }
        }
    }
}