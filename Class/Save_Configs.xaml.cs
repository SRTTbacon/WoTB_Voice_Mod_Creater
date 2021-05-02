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
        bool IsBusy = false;
        bool IsNewMode = false;
        bool IsMessageShowing = false;
        public Save_Configs()
        {
            InitializeComponent();
            SE_Lists.Items.Add("時間切れ&占領ポイントMax | 有効");
            SE_Lists.Items.Add("クイックコマンド | 有効");
            SE_Lists.Items.Add("弾薬庫破損 | 有効");
            SE_Lists.Items.Add("自車両大破 | 有効");
            SE_Lists.Items.Add("貫通 | 有効");
            SE_Lists.Items.Add("敵モジュール破損 | 有効");
            SE_Lists.Items.Add("無線機破損 | 有効");
            SE_Lists.Items.Add("燃料タンク破損 | 有効");
            SE_Lists.Items.Add("非貫通 | 有効");
            SE_Lists.Items.Add("装填完了 | 有効");
            SE_Lists.Items.Add("第六感 | 有効");
            SE_Lists.Items.Add("敵発見 | 有効");
            SE_Lists.Items.Add("戦闘開始前タイマー | 有効");
            SE_Lists.Items.Add("ロックオン | 有効");
            SE_Lists.Items.Add("アンロック | 有効");
            for (int Number = 0; Number <= 14; Number++)
            {
                Voice_Set.SE_Enable_Disable.Add(true);
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
                    Message_T.Opacity -= Sub_Code.Window_Feed_Time;
                }
                await Task.Delay(1000 / 60);
            }
            IsMessageShowing = false;
            Message_T.Text = "";
            Message_T.Opacity = 1;
        }
        public void Window_Show(bool IsNewMode)
        {
            //画面を表示(マルチで行った場合)
            Volume_Set_C.Visibility = Visibility.Visible;
            Volume_Set_T.Visibility = Visibility.Visible;
            Exit_B.Visibility = Visibility.Visible;
            Save_B.Content = "作成";
            if (IsNewMode)
            {
                Android_C.Visibility = Visibility.Hidden;
                Android_T.Visibility = Visibility.Hidden;
                Android_Help_B.Visibility = Visibility.Hidden;
            }
            else
            {
                Android_C.Visibility = Visibility.Visible;
                Android_T.Visibility = Visibility.Visible;
                Android_Help_B.Visibility = Visibility.Visible;
            }
            Configs_Load();
            SE_Dir = Voice_Set.Special_Path + "/Server/" + Voice_Set.SRTTbacon_Server_Name + "/Voices/SE";
            Project_T.Text = "プロジェクト名:" + Voice_Set.SRTTbacon_Server_Name;
            Sub_Code.Get_Voice_Type_And_Index(Voice_Set.Special_Path + "/Server/" + Voice_Set.SRTTbacon_Server_Name + "/Voices", ref Voice_Type, ref Voice_Type_Number);
            for (int Number = 0; Number <= Voice_Type.Count - 1; Number++)
            {
                Voice_Lists.Items.Add(Voice_Type[Number] + ":" + Voice_Type_Number[Number] + "個");
            }
        }
        public void Window_Show_V2(string Project_Name, List<List<string>> Lists, bool IsNewMode)
        {
            //画面を表示(オフラインモードで行った場合)
            Volume_Set_C.Visibility = Visibility.Visible;
            Volume_Set_T.Visibility = Visibility.Visible;
            Exit_B.Visibility = Visibility.Visible;
            Save_B.Content = "作成";
            if (IsNewMode)
            {
                Android_C.Visibility = Visibility.Hidden;
                Language_Left_B.Visibility = Visibility.Visible;
                Language_Right_B.Visibility = Visibility.Visible;
                Android_T.Text = "言語:" + Languages[Select_Language];
                Android_Help_B.Margin = new Thickness(-1400, 892, 0, 0);
            }
            else
            {
                Android_C.Visibility = Visibility.Visible;
                Language_Left_B.Visibility = Visibility.Hidden;
                Language_Right_B.Visibility = Visibility.Hidden;
                Android_T.Text = "Android用";
                Android_Help_B.Margin = new Thickness(-1525, 892, 0, 0);
            }
            Configs_Load();
            this.IsNewMode = IsNewMode;
            SE_Dir = Voice_Set.Special_Path + "/SE";
            Project_T.Text = "プロジェクト名:" + Project_Name;
            for (int Number = 0; Number <= Lists.Count - 1; Number++)
            {
                string Name = Voice_Set.Get_Voice_Type_Japanese_Name_V2(Number);
                int Number_01 = Lists[Number].Count;
                Voice_Type.Add(Name);
                Voice_Type_Number.Add(Number_01);
                Voice_Lists.Items.Add(Name + ":" + Number_01 + "個");
            }
        }
        public async void Window_Show_V3(string BNK_Name, List<List<string>> Lists)
        {
            Opacity = 0;
            Visibility = Visibility.Visible;
            Android_C.Visibility = Visibility.Hidden;
            Language_Left_B.Visibility = Visibility.Hidden;
            Language_Right_B.Visibility = Visibility.Hidden;
            Android_T.Visibility = Visibility.Hidden;
            Android_Help_B.Visibility = Visibility.Hidden;
            Volume_Set_C.Visibility = Visibility.Hidden;
            Volume_Set_T.Visibility = Visibility.Hidden;
            DVPL_C.Visibility = Visibility.Hidden;
            DVPL_T.Visibility = Visibility.Hidden;
            Exit_B.Visibility = Visibility.Hidden;
            Save_B.Content = "保存";
            Configs_Load();
            SE_Dir = Voice_Set.Special_Path + "/SE";
            Project_T.Text = "プロジェクト名:" + BNK_Name;
            for (int Number = 0; Number <= Lists.Count - 1; Number++)
            {
                string Name = Voice_Set.Get_Voice_Type_Japanese_Name_V2(Number);
                int Number_01 = Lists[Number].Count;
                Voice_Type.Add(Name);
                Voice_Type_Number.Add(Number_01);
                Voice_Lists.Items.Add(Name + ":" + Number_01 + "個");
            }
            while (Opacity < 1)
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
                    using (var eifs = new FileStream(Voice_Set.Special_Path + "/Configs/Save_Configs.conf", FileMode.Open, FileAccess.Read))
                    {
                        using (var eofs = new FileStream(Voice_Set.Special_Path + "/Configs/Save_Configs.tmp", FileMode.Create, FileAccess.Write))
                        {
                            FileEncode.FileEncryptor.Decrypt(eifs, eofs, "Save_Configs_Configs_Save");
                        }
                    }
                    StreamReader str = new StreamReader(Voice_Set.Special_Path + "/Configs/Save_Configs.tmp");
                    Volume_Set_C.IsChecked = bool.Parse(str.ReadLine());
                    DVPL_C.IsChecked = bool.Parse(str.ReadLine());
                    for (int Number = 0; Number <= 14; Number++)
                    {
                        Voice_Set.SE_Enable_Disable[Number] = bool.Parse(str.ReadLine());
                        if (Voice_Set.SE_Enable_Disable[Number])
                        {
                            SE_Lists.Items[Number] = SE_Lists.Items[Number].ToString().Replace("| 無効", "| 有効");
                        }
                        else
                        {
                            SE_Lists.Items[Number] = SE_Lists.Items[Number].ToString().Replace("| 有効", "| 無効");
                        }
                    }
                    str.Close();
                    File.Delete(Voice_Set.Special_Path + "/Configs/Save_Configs.tmp");
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
            if (IsBusy || SE_Lists.SelectedIndex == -1)
            {
                return;
            }
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
            if (SE_Lists.SelectedIndex == 0)
            {
                Select_SE_File_Count = SE_Get_File_Count("Capture_End");
            }
            else if (SE_Lists.SelectedIndex == 1)
            {
                Select_SE_File_Count = SE_Get_File_Count("Command");
            }
            else if (SE_Lists.SelectedIndex == 2)
            {
                Select_SE_File_Count = SE_Get_File_Count("Danyaku_SE");
            }
            else if (SE_Lists.SelectedIndex == 3)
            {
                Select_SE_File_Count = SE_Get_File_Count("Destroy");
            }
            else if (SE_Lists.SelectedIndex == 4)
            {
                Select_SE_File_Count = SE_Get_File_Count("Enable");
            }
            else if (SE_Lists.SelectedIndex == 5)
            {
                Select_SE_File_Count = SE_Get_File_Count("Enable_Special");
            }
            else if (SE_Lists.SelectedIndex == 6)
            {
                Select_SE_File_Count = SE_Get_File_Count("Musenki");
            }
            else if (SE_Lists.SelectedIndex == 7)
            {
                Select_SE_File_Count = SE_Get_File_Count("Nenryou_SE");
            }
            else if (SE_Lists.SelectedIndex == 8)
            {
                Select_SE_File_Count = SE_Get_File_Count("Not_Enable");
            }
            else if (SE_Lists.SelectedIndex == 9)
            {
                Select_SE_File_Count = SE_Get_File_Count("Reload");
            }
            else if (SE_Lists.SelectedIndex == 10)
            {
                Select_SE_File_Count = SE_Get_File_Count("Sixth");
            }
            else if (SE_Lists.SelectedIndex == 11)
            {
                Select_SE_File_Count = SE_Get_File_Count("Spot");
            }
            else if (SE_Lists.SelectedIndex == 12)
            {
                Select_SE_File_Count = SE_Get_File_Count("Timer");
            }
            else if (SE_Lists.SelectedIndex == 13)
            {
                Select_SE_File_Count = SE_Get_File_Count("Lock");
            }
            else if (SE_Lists.SelectedIndex == 14)
            {
                Select_SE_File_Count = SE_Get_File_Count("Unlock");
            }
            SE_Play_Number_T.Text = "1/" + Select_SE_File_Count;
        }
        private void SE_Play_B_Click(object sender, RoutedEventArgs e)
        {
            if (SE_Lists.SelectedIndex != -1)
            {
                //選択しているSEを再生
                SE_Play();
            }
        }
        void SE_Play()
        {
            //選択しているSEを再生
            if (IsBusy)
            {
                return;
            }
            Bass.BASS_ChannelStop(Stream);
            Bass.BASS_StreamFree(Stream);
            //SEの拡張子がわからないためFile_Get_FileName_No_Extension()で取得して指定
            if (SE_Play_Index < 10)
            {
                int StreamHandle = Bass.BASS_StreamCreateFile(Sub_Code.File_Get_FileName_No_Extension(SE_Dir + "/" + Select_SE_Name + "_0" + SE_Play_Index), 0, 0, BASSFlag.BASS_STREAM_DECODE);
                Stream = BassFx.BASS_FX_TempoCreate(StreamHandle, BASSFlag.BASS_FX_FREESOURCE);
            }
            else
            {
                int StreamHandle = Bass.BASS_StreamCreateFile(Sub_Code.File_Get_FileName_No_Extension(SE_Dir + "/" + Select_SE_Name + "_" + SE_Play_Index), 0, 0, BASSFlag.BASS_STREAM_DECODE);
                Stream = BassFx.BASS_FX_TempoCreate(StreamHandle, BASSFlag.BASS_FX_FREESOURCE);
            }
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
                {
                    if (!Sub_Code.File_Exists(SE_Dir + "/" + FileName + "_0" + File_Count))
                    {
                        break;
                    }
                }
                else
                {
                    if (!Sub_Code.File_Exists(SE_Dir + "/" + FileName + "_" + File_Count))
                    {
                        break;
                    }
                }
                File_Count++;
            }
            return File_Count - 1;
        }
        private async void Exit_B_Click(object sender, RoutedEventArgs e)
        {
            //閉じる
            if (Opacity >= 1)
            {
                IsBusy = true;
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
                IsBusy = false;
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
                    SE_Lists.Items[SE_Lists.SelectedIndex] = SE_Lists.Items[SE_Lists.SelectedIndex].ToString().Replace("| 有効", "| 無効");
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
                    SE_Lists.Items[SE_Lists.SelectedIndex] = SE_Lists.Items[SE_Lists.SelectedIndex].ToString().Replace("| 無効", "| 有効");
                    SE_Lists.SelectedIndex = Number;
                }
            }
        }
        private void Voice_Lists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //選択している音声の種類が変更されたら音声の名前とファイル数を表示
            if (Voice_Lists.SelectedIndex != -1)
            {
                Voice_Select_T.Text = "音声名:" + Voice_Type[Voice_Lists.SelectedIndex] + "|ファイル数:" + Voice_Type_Number[Voice_Lists.SelectedIndex] + "個";
            }
        }
        private void Voice_Lists_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //音声の選択を解除
            Voice_Lists.SelectedIndex = -1;
            Voice_Select_T.Text = "";
        }
        private async void Save_B_Click(object sender, RoutedEventArgs e)
        {
            if (!IsBusy && Opacity >= 1)
            {
                //作成ボタンが押されたら別クラスに情報を送り画面を閉じる
                IsBusy = true;
                Sub_Code.CreatingProject = true;
                Sub_Code.VolumeSet = Volume_Set_C.IsChecked.Value;
                Sub_Code.DVPL_Encode = DVPL_C.IsChecked.Value;
                Sub_Code.AndroidMode = Android_C.IsChecked.Value;
                Sub_Code.SetLanguage = Languages[Select_Language];
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
                IsBusy = false;
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
        private void DVPL_C_Click(object sender, RoutedEventArgs e)
        {
            if (!DVPL_C.IsChecked.Value)
            {
                Android_C.IsChecked = false;
            }
        }
        private void Android_C_Click(object sender, RoutedEventArgs e)
        {
            if (Android_C.IsChecked.Value && !Voice_Set.FTP_Server.IsConnected)
            {
                MessageBox.Show("サーバーに接続されていないためAndroid用の音声を作成することはできません。");
                Android_C.IsChecked = false;
                return;
            }
            if (Android_C.IsChecked.Value)
            {
                DVPL_C.IsChecked = true;
            }
        }
        private void Android_Help_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsNewMode)
            {
                string Message_01 = "ゲーム内の言語を多国籍に設定している場合は指定してください。\n";
                string Message_02 = "多国籍音声でない場合は'ja'にします。(日本でプレイしている場合のみ)\n";
                string Message_03 = "'gup'はガルパンの音声です。ガルパン仕様のPzIVに乗る場合のみ再生されます。";
                MessageBox.Show(Message_01 + Message_02 + Message_03);
            }
            else
            {
                string Message_01 = "ingame_voice_ja.fsb,GUI_battle_streamed.fsb,GUI_notifications_FX_howitzer_load.fsb,GUI_quick_commands.fsb,GUI_sirene.fsbを作成します。\n";
                string Message_02 = "所々音量が小さく聞こえる場合がありますが、WoTBの標準のfevファイルの仕様上調整できませんのでご了承ください。\n";
                string Message_03 = "まれに↑のfsbファイルが作成されない時がありますのでその場合はもう一度作成しなおすと改善されるかと思います。";
                MessageBox.Show(Message_01 + Message_02 + Message_03);
            }
        }
        private void Language_Left_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy || Select_Language <= 0)
            {
                return;
            }
            Select_Language--;
            Android_T.Text = "言語:" + Languages[Select_Language];
        }
        private void Language_Right_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy || Select_Language >= 17)
            {
                return;
            }
            Select_Language++;
            Android_T.Text = "言語:" + Languages[Select_Language];
        }
        void Configs_Save()
        {
            try
            {
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Configs/Save_Configs.tmp");
                stw.WriteLine(Volume_Set_C.IsChecked.Value);
                stw.WriteLine(DVPL_C.IsChecked.Value);
                foreach (bool Value in Voice_Set.SE_Enable_Disable)
                {
                    stw.WriteLine(Value);
                }
                stw.Close();
                using (var eifs = new FileStream(Voice_Set.Special_Path + "/Configs/Save_Configs.tmp", FileMode.Open, FileAccess.Read))
                {
                    using (var eofs = new FileStream(Voice_Set.Special_Path + "/Configs/Save_Configs.conf", FileMode.Create, FileAccess.Write))
                    {
                        FileEncode.FileEncryptor.Encrypt(eifs, eofs, "Save_Configs_Configs_Save");
                    }
                }
                File.Delete(Voice_Set.Special_Path + "/Configs/Save_Configs.tmp");
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
                SE_Lists.Items[Number] = SE_Lists.Items[Number].ToString().Replace("| 無効", "| 有効");
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
                SE_Lists.Items[Number] = SE_Lists.Items[Number].ToString().Replace("| 有効", "| 無効");
            }
            SE_Lists.SelectedIndex = SelectedIndex;
        }
    }
}