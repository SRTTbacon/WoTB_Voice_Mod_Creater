using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using WoTB_Voice_Mod_Creater.Wwise_Class;

namespace WoTB_Voice_Mod_Creater.Class
{
    public class SE_Info_Parent
    {
        public string SE_Path = "";
        public long Stream_Position = -1;
        public uint SE_ShortID = 0;
        public byte[] Sound_Binary = null;
        public SE_Info_Parent(string SE_Path, uint SE_ShortID)
        {
            this.SE_Path = SE_Path;
            this.SE_ShortID = SE_ShortID;
        }
    }
    public partial class WoT_Sound_Mod_Settings : UserControl
    {
        public static readonly Dictionary<string, List<SE_Info_Parent>> SE_Info = new Dictionary<string, List<SE_Info_Parent>>();
        public static readonly List<uint> SE_ShortIDs = new List<uint>();
        readonly BrushConverter bc = new BrushConverter();
        WVS_Load WVS_File = null;
        int Stream;
        bool IsClosing = false;
        bool IsMessageShowing = false;
        bool IsPaused = false;
        public WoT_Sound_Mod_Settings()
        {
            InitializeComponent();
            SE_Info.Add("第六感", new List<SE_Info_Parent>());
            SE_Info.Add("自走砲の警報", new List<SE_Info_Parent>());
            SE_ShortIDs.Add(871639093);
            SE_ShortIDs.Add(2540444);
            foreach (string Key_Name in SE_Info.Keys)
                Add_SE_List(Key_Name);
        }
        void Add_SE_List(string Text)
        {
            ListBoxItem Item = new ListBoxItem()
            {
                Content = Text + " | 0個",
                Foreground = (Brush)new BrushConverter().ConvertFromString("#BFFF2C8C")
            };
            SE_Type_Lists.Items.Add(Item);
        }
        List<SE_Info_Parent> Get_Sounds_By_Index(int Type_Index)
        {
            return SE_Info[SE_Info.Keys.ToList()[Type_Index]];
        }
        void Change_SE_List(int Index)
        {
            ListBoxItem Item = SE_Type_Lists.Items[Index] as ListBoxItem;
            string Text = Item.Content.ToString();
            Text = Text.Substring(0, Text.IndexOf('|') + 2);
            Text += Get_Sounds_By_Index(Index).Count + "個";
            if (Get_Sounds_By_Index(Index).Count > 0)
                Item.Foreground = Brushes.Aqua;
            else
                Item.Foreground = (Brush)new BrushConverter().ConvertFromString("#BFFF2C8C");
            Item.Content = Text;
        }
        public void All_Aplly_List()
        {
            for (int Number = 0; Number < SE_Type_Lists.Items.Count; Number++)
            {
                ListBoxItem Item = SE_Type_Lists.Items[Number] as ListBoxItem;
                string Text = Item.Content.ToString();
                Text = Text.Substring(0, Text.IndexOf('|') + 2);
                Text += Get_Sounds_By_Index(Number).Count + "個";
                if (Get_Sounds_By_Index(Number).Count > 0)
                    Item.Foreground = Brushes.Aqua;
                else
                    Item.Foreground = (Brush)new BrushConverter().ConvertFromString("#BFFF2C8C");
                Item.Content = Text;
            }
        }
        async void Message_Feed_Out(string Message)
        {
            if (IsMessageShowing)
            {
                IsMessageShowing = false;
                await Task.Delay(1000 / 59);
            }
            Message_T.Text = Message;
            IsMessageShowing = true;
            Message_T.Opacity = 1;
            int Number = 0;
            bool IsForce = false;
            while (Message_T.Opacity > 0)
            {
                if (!IsMessageShowing)
                {
                    IsForce = true;
                    break;
                }
                Number++;
                if (Number >= 120)
                    Message_T.Opacity -= 0.025;
                await Task.Delay(1000 / 60);
            }
            if (!IsForce)
            {
                IsMessageShowing = false;
                Message_T.Text = "";
                Message_T.Opacity = 1;
            }
        }
        public async void Window_Show(string Project_Name, WVS_Load WVS_File)
        {
            Opacity = 0;
            Visibility = Visibility.Visible;
            this.WVS_File = WVS_File;
            Project_T.Text = "プロジェクト名:" + Project_Name;
            while (Opacity < 1 && !IsClosing)
            {
                Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
        }
        private void SE_Play_B_Click(object sender, RoutedEventArgs e)
        {
            if (SE_Type_Lists.SelectedIndex == -1 || SE_Sound_Lists.SelectedIndex == -1 || IsClosing)
                return;
            Bass.BASS_ChannelStop(Stream);
            Bass.BASS_StreamFree(Stream);
            string Select_File = "";
            string SE_Path = Get_Sounds_By_Index(SE_Type_Lists.SelectedIndex)[SE_Sound_Lists.SelectedIndex].SE_Path;
            if (SE_Path.Contains("\\"))
            {
                Select_File = SE_Path;
                if (!File.Exists(SE_Path))
                {
                    Message_Feed_Out("音声ファイルが存在しません。削除された可能性があります。");
                    return;
                }
            }
            int StreamHandle;
            if (Select_File != "")
                StreamHandle = Bass.BASS_StreamCreateFile(Select_File, 0, 0, BASSFlag.BASS_STREAM_DECODE);
            else
            {
                File.Delete(Voice_Set.Special_Path + "\\Wwise\\Temp_Voice_Create_02.mp3");
                File.WriteAllBytes(Voice_Set.Special_Path + "\\Wwise\\Temp_Voice_Create_02.mp3", WVS_File.Load_Sound(Get_Sounds_By_Index(SE_Type_Lists.SelectedIndex)
                    [SE_Sound_Lists.SelectedIndex].Stream_Position));
                StreamHandle = Bass.BASS_StreamCreateFile(Voice_Set.Special_Path + "\\Wwise\\Temp_Voice_Create_02.mp3", 0, 0, BASSFlag.BASS_STREAM_DECODE);
            }
            Stream = BassFx.BASS_FX_TempoCreate(StreamHandle, BASSFlag.BASS_FX_FREESOURCE);
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, 1f);
            Bass.BASS_ChannelSetDevice(Stream, Video_Mode.Sound_Device);
            Bass.BASS_ChannelPlay(Stream, true);
            IsPaused = false;
        }
        private void SE_Stop_B_Click(object sender, RoutedEventArgs e)
        {
            Pause_Volume_Animation(true, 15f);
        }
        async void Pause_Volume_Animation(bool IsStop, float Feed_Time = 30f)
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
                    if (File.Exists(Voice_Set.Special_Path + "\\Wwise\\Temp_Voice_Create_02.mp3"))
                        File.Delete(Voice_Set.Special_Path + "\\Wwise\\Temp_Voice_Create_02.mp3");
                }
                else if (IsPaused)
                    Bass.BASS_ChannelPause(Stream);
            }
        }
        private async void Exit_B_Click(object sender, RoutedEventArgs e)
        {
            //閉じる
            if (Opacity >= 1)
            {
                IsClosing = true;
                Sub_Code.CreatingProject = false;
                Pause_Volume_Animation(true, 15f);
                while (Opacity > 0)
                {
                    Opacity -= Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
                SE_Type_Lists.SelectedIndex = -1;
                Visibility = Visibility.Hidden;
                SE_Sound_Lists.Items.Clear();
                IsClosing = false;
            }
        }
        private async void Save_B_Click(object sender, RoutedEventArgs e)
        {
            if (!IsClosing && Opacity >= 1)
            {
                //作成ボタンが押されたら別クラスに情報を送り画面を閉じる
                IsClosing = true;
                Sub_Code.CreatingProject = true;
                Sub_Code.Only_Wwise_Project = Only_Wwise_C.IsChecked.Value;
                Sub_Code.VolumeSet = false;
                while (Opacity > 0)
                {
                    Opacity -= Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
                SE_Type_Lists.SelectedIndex = -1;
                IsClosing = false;
                Visibility = Visibility.Hidden;
            }
        }
        private void Only_Wwise_C_Click(object sender, RoutedEventArgs e)
        {
            if (Only_Wwise_C.IsChecked.Value)
            {
                MessageBoxResult result = MessageBox.Show("この項目にチェックを入れると、音声Mod(*.bnk)を作成するのではなく、音声Modを作成するためのWwiseのプロジェクトファイルが生成" +
                    "されます。\n続行しますか?", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.No)
                    Only_Wwise_C.IsChecked = false;
            }
        }
        private void SE_Type_Lists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SE_Type_Lists.SelectedIndex == -1)
            {
                SE_Sound_Lists.Items.Clear();
                Pause_Volume_Animation(true, 15f);
                return;
            }
            SE_Sound_Lists.Items.Clear();
            foreach (string File_Path in Get_Sounds_By_Index(SE_Type_Lists.SelectedIndex).Select(h => h.SE_Path))
                SE_Sound_Lists.Items.Add(Path.GetFileName(File_Path));
        }
        private string Get_EventName_By_Index(int Index)
        {
            if (Index == 0)
                return "lightbulb";
            else if (Index == 1)
                return "artillery_lightbulb";
            return "";
        }
        private void SE_Add_B_Click(object sender, RoutedEventArgs e)
        {
            if (SE_Type_Lists.SelectedIndex == -1)
            {
                Message_Feed_Out("SEの種類が選択されていません。");
                return;
            }
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog()
            {
                Title = "サウンドファイルを選択してください。",
                Filter = "サウンドファイル(*.wav;*.mp3;*.ogg;*.flac;*.aac)|*.wav;*.mp3;*.ogg;*.flac;*.aac",
                Multiselect = true
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                bool IsHasError = false;
                foreach (string File_Now in ofd.FileNames)
                {
                    if (Get_Sounds_By_Index(SE_Type_Lists.SelectedIndex).Select(h => h.SE_Path).Contains(File_Now))
                    {
                        IsHasError = true;
                        continue;
                    }
                    Get_Sounds_By_Index(SE_Type_Lists.SelectedIndex).Add(new SE_Info_Parent(File_Now, SE_ShortIDs[SE_Type_Lists.SelectedIndex]));
                    SE_Sound_Lists.Items.Add(Path.GetFileName(File_Now));
                }
                Change_SE_List(SE_Type_Lists.SelectedIndex);
                if (IsHasError)
                    Message_Feed_Out("既にリストに存在するファイルは追加できません。");
            }
            ofd.Dispose();
        }
        private void SE_Remove_B_Click(object sender, RoutedEventArgs e)
        {
            if (SE_Type_Lists.SelectedIndex == -1 || SE_Sound_Lists.SelectedIndex == -1)
            {
                Message_Feed_Out("削除するSEが選択されていません。");
                return;
            }
            Get_Sounds_By_Index(SE_Type_Lists.SelectedIndex).RemoveAt(SE_Sound_Lists.SelectedIndex);
            SE_Sound_Lists.Items.RemoveAt(SE_Sound_Lists.SelectedIndex);
            Pause_Volume_Animation(true, 15f);
            Change_SE_List(SE_Type_Lists.SelectedIndex);
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            string SE_Path = Voice_Set.Special_Path + "\\WoT_SE";
            Get_Sounds_By_Index(0).Add(new SE_Info_Parent(SE_Path + "\\Lightbulb_01.wav", SE_ShortIDs[0]));
            Get_Sounds_By_Index(1).Add(new SE_Info_Parent(SE_Path + "\\SPG_Lightbulb.wav", SE_ShortIDs[1]));
            All_Aplly_List();
        }
        public void Add_XML_Change_Mod(WoT_Create_XML XML)
        {
            string SE_Path = Voice_Set.Special_Path + "\\WoT_SE";
            List<SE_Info_Parent> Parent_01 = Get_Sounds_By_Index(0);
            List<SE_Info_Parent> Parent_02 = Get_Sounds_By_Index(1);
            if (Parent_01.Count == 1 && Parent_01[0].SE_Path == SE_Path + "\\Lightbulb_01.wav")
                XML.Add_Change_Event(Get_EventName_By_Index(0), Get_EventName_By_Index(0) + "_Mod");
            if (Parent_02.Count == 1 && Parent_02[0].SE_Path == SE_Path + "\\SPG_Lightbulb.wav")
                XML.Add_Change_Event(Get_EventName_By_Index(0), Get_EventName_By_Index(0) + "_Mod");
        }
    }
}