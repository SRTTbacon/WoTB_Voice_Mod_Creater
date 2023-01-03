using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;

namespace WoTB_Voice_Mod_Creater.Class
{
    public partial class Save_Configs : UserControl
    {
        readonly BrushConverter bc = new BrushConverter();
        List<string> Voice_Type = new List<string>();
        List<int> Voice_Type_Number = new List<int>();
        string[] Languages = { "arb", "cn", "cs", "de", "en", "es", "fi", "fr", "gup", "it", "ja", "ko", "pbr", "pl", "ru", "th", "tr", "vi" };
        string Select_SE_Name = "";
        string SE_Dir = "";
        int Select_SE_File_Count = 0;
        int SE_Play_Index = 1;
        int Select_Language = 10;
        int Stream;
        double Wwise_Version = 1.0;
        bool IsClosing = false;
        bool IsMessageShowing = false;
        public Save_Configs()
        {
            InitializeComponent();
            Add_SE_List("時間切れ&占領ポイントMax", true);
            Add_SE_List("クイックコマンド", true);
            Add_SE_List("弾薬庫破損", true);
            Add_SE_List("自車両大破", true);
            Add_SE_List("貫通", true);
            Add_SE_List("敵モジュール破損", true);
            Add_SE_List("無線機破損", true);
            Add_SE_List("燃料タンク破損", true);
            Add_SE_List("非貫通-無効弾", true);
            Add_SE_List("非貫通-跳弾", true);
            Add_SE_List("装填完了", true);
            Add_SE_List("第六感", true);
            Add_SE_List("敵発見", true);
            Add_SE_List("戦闘開始前タイマー", true);
            Add_SE_List("ロックオン", true);
            Add_SE_List("アンロック", true);
            Add_SE_List("ノイズ音", true);
            Add_SE_List("搭乗員負傷", true);
            Add_SE_List("モジュール破損", true);
            Add_SE_List("モジュール大破", true);
            Add_SE_List("モジュール復旧", true);
            Add_SE_List("戦闘開始", true);
            Add_SE_List("敵炎上", true);
            for (int Number = 0; Number < SE_Lists.Items.Count; Number++)
                Voice_Set.SE_Enable_Disable.Add(true);
        }
        void Add_SE_List(string Text, bool IsEnable)
        {
            ListBoxItem Item = new ListBoxItem()
            {
                Content = Text + " | "
            };
            if (IsEnable)
                Item.Content += "有効";
            else
            {
                Item.Content += "無効";
                Item.Foreground = (Brush)new BrushConverter().ConvertFromString("#BFFF2C8C");
            }
            SE_Lists.Items.Add(Item);
        }
        void Change_SE_List(int Index)
        {
            ListBoxItem Item = SE_Lists.Items[Index] as ListBoxItem;
            string Text = Item.Content.ToString();
            Text = Text.Substring(0, Text.IndexOf('|') + 2);
            if (Voice_Set.SE_Enable_Disable[Index])
            {
                Text += "有効";
                Item.Foreground = Brushes.Aqua;
            }
            else
            {
                Text += "無効";
                Item.Foreground = (Brush)new BrushConverter().ConvertFromString("#BFFF2C8C");
            }
            Item.Content = Text;
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
        public void Window_Show()
        {
            //画面を表示(マルチで行った場合)
            Volume_Set_C.Visibility = Visibility.Visible;
            Volume_Set_T.Visibility = Visibility.Visible;
            Exit_B.Visibility = Visibility.Visible;
            Save_B.Content = "作成";
            Android_T.Visibility = Visibility.Hidden;
            Default_Voice_Mode_C.Visibility = Visibility.Visible;
            Default_Voice_Mode_T.Visibility = Visibility.Visible;
            Configs_Load();
            SE_Dir = Voice_Set.Special_Path + "/Server/" + Voice_Set.SRTTbacon_Server_Name + "/Voices/SE";
            Project_T.Text = "プロジェクト名:" + Voice_Set.SRTTbacon_Server_Name;
            Sub_Code.Get_Voice_Type_And_Index(Voice_Set.Special_Path + "/Server/" + Voice_Set.SRTTbacon_Server_Name + "/Voices", ref Voice_Type, ref Voice_Type_Number);
            for (int Number = 0; Number <= Voice_Type.Count - 1; Number++)
                Voice_Lists.Items.Add(Voice_Type[Number] + ":" + Voice_Type_Number[Number] + "個");
            Only_Wwise_C.IsChecked = false;
        }
        public void Window_Show_V2(string Project_Name, List<Voice_Event_Setting> Lists)
        {
            //画面を表示(オフラインモードで行った場合)
            Volume_Set_C.Visibility = Visibility.Visible;
            Volume_Set_T.Visibility = Visibility.Visible;
            Default_Voice_Mode_C.Visibility = Visibility.Visible;
            Default_Voice_Mode_T.Visibility = Visibility.Visible;
            Exit_B.Visibility = Visibility.Visible;
            Save_B.Content = "作成";
            Language_Left_B.Visibility = Visibility.Visible;
            Language_Right_B.Visibility = Visibility.Visible;
            Android_T.Text = "言語:" + Languages[Select_Language];
            Configs_Load();
            SE_Dir = Voice_Set.Special_Path + "/SE";
            Project_T.Text = "プロジェクト名:" + Project_Name;
            for (int Number = 0; Number < Lists.Count; Number++)
            {
                string Name = Voice_Set.Get_Voice_Type_Japanese_Name_V2(Number);
                int Number_01 = Lists[Number].Sounds.Count;
                Voice_Type.Add(Name);
                Voice_Type_Number.Add(Number_01);
                Voice_Lists.Items.Add(Name + ":" + Number_01 + "個");
            }
        }
        public async void Window_Show_V3(string BNK_Name, List<Voice_Event_Setting> Lists)
        {
            Opacity = 0;
            Visibility = Visibility.Visible;
            Language_Left_B.Visibility = Visibility.Hidden;
            Language_Right_B.Visibility = Visibility.Hidden;
            Android_T.Visibility = Visibility.Hidden;
            Volume_Set_C.Visibility = Visibility.Hidden;
            Volume_Set_T.Visibility = Visibility.Hidden;
            DVPL_C.Visibility = Visibility.Hidden;
            DVPL_T.Visibility = Visibility.Hidden;
            Exit_B.Visibility = Visibility.Hidden;
            Default_Voice_Mode_C.Visibility = Visibility.Hidden;
            Default_Voice_Mode_T.Visibility = Visibility.Hidden;
            Save_B.Content = "保存";
            Configs_Load();
            SE_Dir = Voice_Set.Special_Path + "/SE";
            Project_T.Text = "プロジェクト名:" + BNK_Name;
            for (int Number = 0; Number <= Lists.Count - 1; Number++)
            {
                string Name = Voice_Set.Get_Voice_Type_Japanese_Name_V2(Number);
                int Number_01 = Lists[Number].Sounds.Count;
                Voice_Type.Add(Name);
                Voice_Type_Number.Add(Number_01);
                Voice_Lists.Items.Add(Name + ":" + Number_01 + "個");
            }
            while (Opacity < 1 && !IsClosing)
            {
                Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
        }
        void Configs_Load()
        {
            if (File.Exists(Voice_Set.Special_Path + "/Configs/Save_Configs.conf"))
            {
                try
                {
                    StreamReader str = Sub_Code.File_Decrypt_To_Stream(Voice_Set.Special_Path + "/Configs/Save_Configs.conf", "Save_Configs_Configs_Save");
                    bool IsNewVersionMode = false;
                    string One_Line = str.ReadLine();
                    if (One_Line.Contains("V1.4_Save_Mode"))
                    {
                        IsNewVersionMode = true;
                        Volume_Set_C.IsChecked = bool.Parse(str.ReadLine());
                    }
                    else
                        Volume_Set_C.IsChecked = bool.Parse(One_Line);
                    DVPL_C.IsChecked = bool.Parse(str.ReadLine());
                    if (IsNewVersionMode)
                        Default_Voice_Mode_C.IsChecked = bool.Parse(str.ReadLine());
                    for (int Number = 0; Number <= 14; Number++)
                    {
                        Voice_Set.SE_Enable_Disable[Number] = bool.Parse(str.ReadLine());
                        Change_SE_List(Number);
                    }
                    str.Close();
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show("設定を読み込めませんでした。\nエラー回避のため設定は削除されます。");
                    File.Delete(Voice_Set.Special_Path + "/Configs/Save_Configs.conf");
                    Sub_Code.Error_Log_Write(e.Message);
                }
            }
            try
            {
                if (File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Version.dat"))
                {
                    StreamReader str = new StreamReader(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Version.dat");
                    Wwise_Version = double.Parse(str.ReadLine());
                    str.Close();
                }
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                Message_Feed_Out("Wwiseプロジェクトのバージョンを取得できませんでした。");
            }
        }
        private void SE_Lists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsClosing || SE_Lists.SelectedIndex == -1)
                return;
            SE_Play_Index = 1;
            //選択したSEの状態によって色を変更
            if (Voice_Set.SE_Enable_Disable[SE_Lists.SelectedIndex])
            {
                SE_Disable_B.Background = Brushes.Transparent;
                SE_Disable_B.BorderBrush = Brushes.Aqua;
                SE_Enable_B.Background = (Brush)bc.ConvertFrom("#59999999");
                SE_Enable_B.BorderBrush = Brushes.Red;
            }
            else
            {
                SE_Disable_B.Background = (Brush)bc.ConvertFrom("#59999999");
                SE_Disable_B.BorderBrush = Brushes.Red;
                SE_Enable_B.Background = Brushes.Transparent;
                SE_Enable_B.BorderBrush = Brushes.Aqua;
            }
            //選択したSEのファイルが何個あるか取得して表示
            string SE_Count = SE_Change_Window.Preset_List[SE_Change_Window.Preset_Index][SE_Lists.SelectedIndex + 1];
            Select_SE_File_Count = SE_Count.Split('|').Length;
            SE_Play_Number_T.Text = "1/" + Select_SE_File_Count;
        }
        private void SE_Play_B_Click(object sender, RoutedEventArgs e)
        {
            if (SE_Lists.SelectedIndex != -1)
                //選択しているSEを再生
                SE_Play();
        }
        void SE_Play()
        {
            //選択しているSEを再生
            if (IsClosing)
                return;
            Bass.BASS_ChannelStop(Stream);
            Bass.BASS_StreamFree(Stream);
            string SE_Count = SE_Change_Window.Preset_List[SE_Change_Window.Preset_Index][SE_Lists.SelectedIndex + 1];
            int StreamHandle = Bass.BASS_StreamCreateFile(SE_Count.Split('|')[SE_Play_Index - 1], 0, 0, BASSFlag.BASS_STREAM_DECODE);
            Stream = BassFx.BASS_FX_TempoCreate(StreamHandle, BASSFlag.BASS_FX_FREESOURCE);
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, 1f);
            Bass.BASS_ChannelSetDevice(Stream, Video_Mode.Sound_Device);
            Bass.BASS_ChannelPlay(Stream, true);
            if (SE_Play_Index < Select_SE_File_Count)
            {
                SE_Play_Index++;
                SE_Play_Number_T.Text = SE_Play_Index + "/" + Select_SE_File_Count;
            }
            else if (Select_SE_File_Count != 1 && SE_Play_Index == Select_SE_File_Count)
            {
                SE_Play_Index = 1;
                SE_Play_Number_T.Text = SE_Play_Index + "/" + Select_SE_File_Count;
            }
        }
        //指定したファイル名のSEの数を取得
        //引数:パスではなく拡張子を含まないファイル名
        //戻り値:ファイル数
        int SE_Get_File_Count(string FileName)
        {
            int File_Count = 1;
            Select_SE_Name = FileName;
            while (true)
            {
                if (File_Count < 10)
                    if (!Sub_Code.File_Exists(SE_Dir + "/" + FileName + "_0" + File_Count))
                        break;
                else
                    if (!Sub_Code.File_Exists(SE_Dir + "/" + FileName + "_" + File_Count))
                        break;
                File_Count++;
            }
            return File_Count - 1;
        }
        private async void Exit_B_Click(object sender, RoutedEventArgs e)
        {
            //閉じる
            if (Opacity >= 1)
            {
                IsClosing = true;
                Sub_Code.CreatingProject = false;
                Sub_Code.DVPL_Encode = false;
                Configs_Save();
                while (Opacity > 0)
                {
                    Opacity -= Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
                SE_Lists.SelectedIndex = -1;
                Visibility = Visibility.Hidden;
                Voice_Lists.Items.Clear();
                Voice_Type.Clear();
                Voice_Type_Number.Clear();
                IsClosing = false;
            }
        }
        private void SE_Disable_B_Click(object sender, RoutedEventArgs e)
        {
            //選択しているSEを無効化
            if (SE_Lists.SelectedIndex != -1)
            {
                if (Voice_Set.SE_Enable_Disable[SE_Lists.SelectedIndex])
                {
                    if (Wwise_Version < 1.1)
                    {
                        Message_Feed_Out("この機能を有効にするには、Wwiseプロジェクトをアップデートする必要があります。");
                        return;
                    }
                    int Number = SE_Lists.SelectedIndex;
                    Voice_Set.SE_Enable_Disable[SE_Lists.SelectedIndex] = false;
                    SE_Disable_B.Background = (Brush)bc.ConvertFrom("#59999999");
                    SE_Disable_B.BorderBrush = Brushes.Red;
                    SE_Enable_B.Background = Brushes.Transparent;
                    SE_Enable_B.BorderBrush = Brushes.Aqua;
                    Change_SE_List(SE_Lists.SelectedIndex);
                    SE_Lists.SelectedIndex = Number;
                }
            }
        }
        private void SE_Enable_B_Click(object sender, RoutedEventArgs e)
        {
            //選択しているSEを有効化
            if (SE_Lists.SelectedIndex != -1)
            {
                if (!Voice_Set.SE_Enable_Disable[SE_Lists.SelectedIndex])
                {
                    int Number = SE_Lists.SelectedIndex;
                    Voice_Set.SE_Enable_Disable[SE_Lists.SelectedIndex] = true;
                    SE_Disable_B.Background = Brushes.Transparent;
                    SE_Disable_B.BorderBrush = Brushes.Aqua;
                    SE_Enable_B.Background = (Brush)bc.ConvertFrom("#59999999");
                    SE_Enable_B.BorderBrush = Brushes.Red;
                    Change_SE_List(SE_Lists.SelectedIndex);
                    SE_Lists.SelectedIndex = Number;
                }
            }
        }
        private void Voice_Lists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //選択している音声の種類が変更されたら音声の名前とファイル数を表示
            if (Voice_Lists.SelectedIndex != -1)
                Voice_Select_T.Text = "音声名:" + Voice_Type[Voice_Lists.SelectedIndex] + "|ファイル数:" + Voice_Type_Number[Voice_Lists.SelectedIndex] + "個";
        }
        private void Voice_Lists_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //音声の選択を解除
            Voice_Lists.SelectedIndex = -1;
            Voice_Select_T.Text = "";
        }
        private async void Save_B_Click(object sender, RoutedEventArgs e)
        {
            if (!IsClosing && Opacity >= 1)
            {
                //作成ボタンが押されたら別クラスに情報を送り画面を閉じる
                IsClosing = true;
                Sub_Code.CreatingProject = true;
                Sub_Code.VolumeSet = Volume_Set_C.IsChecked.Value;
                Sub_Code.DVPL_Encode = DVPL_C.IsChecked.Value;
                Sub_Code.SetLanguage = Languages[Select_Language];
                Sub_Code.Default_Voice = Default_Voice_Mode_C.IsChecked.Value;
                Sub_Code.Only_Wwise_Project = Only_Wwise_C.IsChecked.Value;
                Configs_Save();
                while (Opacity > 0)
                {
                    Opacity -= Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
                Voice_Lists.Items.Clear();
                Voice_Select_T.Text = "";
                Voice_Type.Clear();
                Voice_Type_Number.Clear();
                IsClosing = false;
                Visibility = Visibility.Hidden;
            }
        }
        private void SE_Lists_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //SEの選択を解除
            SE_Play_Number_T.Text = "0/0";
            SE_Disable_B.Background = Brushes.Transparent;
            SE_Disable_B.BorderBrush = Brushes.Aqua;
            SE_Enable_B.Background = Brushes.Transparent;
            SE_Enable_B.BorderBrush = Brushes.Aqua;
            SE_Lists.SelectedIndex = -1;
        }
        private void Language_Left_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || Select_Language <= 0)
                return;
            Select_Language--;
            Android_T.Text = "言語:" + Languages[Select_Language];
        }
        private void Language_Right_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || Select_Language >= 17)
                return;
            Select_Language++;
            Android_T.Text = "言語:" + Languages[Select_Language];
        }
        void Configs_Save()
        {
            try
            {
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Configs/Save_Configs.tmp");
                stw.WriteLine("V1.4_Save_Mode");
                stw.WriteLine(Volume_Set_C.IsChecked.Value);
                stw.WriteLine(DVPL_C.IsChecked.Value);
                stw.WriteLine(Default_Voice_Mode_C.IsChecked.Value);
                foreach (bool Value in Voice_Set.SE_Enable_Disable)
                    stw.WriteLine(Value);
                stw.Close();
                Sub_Code.File_Encrypt(Voice_Set.Special_Path + "/Configs/Save_Configs.tmp", Voice_Set.Special_Path + "/Configs/Save_Configs.conf", "Save_Configs_Configs_Save", true);
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        private void SE_All_Enable_B_Click(object sender, RoutedEventArgs e)
        {
            int SelectedIndex = SE_Lists.SelectedIndex;
            for (int Number = 0; Number < SE_Lists.Items.Count; Number++)
            {
                Voice_Set.SE_Enable_Disable[Number] = true;
                SE_Disable_B.Background = Brushes.Transparent;
                SE_Disable_B.BorderBrush = Brushes.Aqua;
                SE_Enable_B.Background = (Brush)bc.ConvertFrom("#59999999");
                SE_Enable_B.BorderBrush = Brushes.Red;
                Change_SE_List(Number);
            }
            SE_Lists.SelectedIndex = SelectedIndex;
        }
        private void SE_All_Disable_B_Click(object sender, RoutedEventArgs e)
        {
            if (Wwise_Version < 1.1)
            {
                Message_Feed_Out("この機能を有効にするには、Wwiseプロジェクトをアップデートする必要があります。");
                return;
            }
            int SelectedIndex = SE_Lists.SelectedIndex;
            for (int Number = 0; Number < SE_Lists.Items.Count; Number++)
            {
                Voice_Set.SE_Enable_Disable[Number] = false;
                SE_Disable_B.Background = (Brush)bc.ConvertFrom("#59999999");
                SE_Disable_B.BorderBrush = Brushes.Red;
                SE_Enable_B.Background = Brushes.Transparent;
                SE_Enable_B.BorderBrush = Brushes.Aqua;
                Change_SE_List(Number);
            }
            SE_Lists.SelectedIndex = SelectedIndex;
        }
        private void Default_Voice_Mode_C_Click(object sender, RoutedEventArgs e)
        {

        }
        private void SE_Change_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            SE_Change_Window.Window_Show();
            SE_Play_Number_T.Text = "0/0";
            SE_Disable_B.Background = Brushes.Transparent;
            SE_Disable_B.BorderBrush = Brushes.Aqua;
            SE_Enable_B.Background = Brushes.Transparent;
            SE_Enable_B.BorderBrush = Brushes.Aqua;
            SE_Lists.SelectedIndex = -1;
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
    }
}