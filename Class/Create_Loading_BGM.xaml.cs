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
        List<List<string>> Music_Type_Garage_SE = new List<List<string>>();
        List<List<string>> Music_Type_WoTB_Gun = new List<List<string>>();
        List<List<string>> Music_Type_WoT_Gun = new List<List<string>>();
        List<List<Music_Play_Time>> Music_Play_Times = new List<List<Music_Play_Time>>();
        List<List<bool>> Music_Feed_In = new List<List<bool>>();
        int Stream;
        int Mod_Page = 0;
        float SetFirstFreq = 44100f;
        bool IsClosing = false;
        bool IsBusy = false;
        bool IsMessageShowing = false;
        bool IsLocationChanging = false;
        bool IsPaused = false;
        bool IsEnded = false;
        bool IsOpenDialog = false;
        bool IsDownloading = false;
        Random r = new Random();
        SYNCPROC IsMusicEnd;
        public Create_Loading_BGM()
        {
            InitializeComponent();
            Position_S.AddHandler(MouseDownEvent, new MouseButtonEventHandler(Position_S_MouseDown), true);
            Position_S.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Position_S_MouseUp), true);
            Volume_S.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Volume_S_MouseUp), true);
            Default_SE_B.Visibility = Visibility.Hidden;
        }
        void BGM_Type_Change()
        {
            BGM_Type_L.Items.Clear();
            if (Mod_Page == 0)
            {
                Mod_Name_T.Text = "Modの種類";
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
                BGM_Type_L.Items.Add("被弾:貫通-音声 | 0個");
                BGM_Type_L.Items.Add("被弾:非貫通-音声 | 0個");
                if (Music_Type_Music.Count == 0)
                {
                    for (int Number = 0; Number < BGM_Type_L.Items.Count; Number++)
                    {
                        Music_Type_Music.Add(new List<string>());
                        Music_Play_Times.Add(new List<Music_Play_Time>());
                        Music_Feed_In.Add(new List<bool>());
                    }
                }
            }
            else if (Mod_Page == 1)
            {
                Mod_Name_T.Text = "ガレージSE";
                BGM_Type_L.Items.Add("コンテナ開封-ノーマル-SE | 0個");
                BGM_Type_L.Items.Add("コンテナ開封-ノーマル-音声 | 0個");
                BGM_Type_L.Items.Add("コンテナ開封-レア-SE | 0個");
                BGM_Type_L.Items.Add("コンテナ開封-レア-音声 | 0個");
                BGM_Type_L.Items.Add("購入-SE | 0個");
                BGM_Type_L.Items.Add("購入-音声 | 0個");
                BGM_Type_L.Items.Add("売却-SE | 0個");
                BGM_Type_L.Items.Add("売却-音声 | 0個");
                BGM_Type_L.Items.Add("チェックボックス-SE | 0個");
                BGM_Type_L.Items.Add("チェックボックス-音声 | 0個");
                BGM_Type_L.Items.Add("小隊受信-SE | 0個");
                BGM_Type_L.Items.Add("小隊受信-音声 | 0個");
                BGM_Type_L.Items.Add("モジュールの切り替え-SE | 0個");
                BGM_Type_L.Items.Add("モジュールの切り替え-音声 | 0個");
                BGM_Type_L.Items.Add("戦闘開始-SE | 0個");
                BGM_Type_L.Items.Add("戦闘開始-音声 | 0個");
                BGM_Type_L.Items.Add("ニュース-SE | 0個");
                BGM_Type_L.Items.Add("ニュース-音声 | 0個");
                BGM_Type_L.Items.Add("車両購入-SE | 0個");
                BGM_Type_L.Items.Add("車両購入-音声 | 0個");
                if (Music_Type_Garage_SE.Count == 0)
                {
                    for (int Number = 0; Number < BGM_Type_L.Items.Count; Number++)
                        Music_Type_Garage_SE.Add(new List<string>());
                    Set_Garage_Default_SE();
                }
            }
            else if (Mod_Page == 2)
            {
                Mod_Name_T.Text = "砲撃音Mod(WoTB用)";
                BGM_Type_L.Items.Add("12～23mm:自車両-通常 | 0個");
                BGM_Type_L.Items.Add("12～23mm:自車両-ズーム時 | 0個");
                BGM_Type_L.Items.Add("12～23mm:他車両 | 0個");
                BGM_Type_L.Items.Add("20～45mm:自車両-通常 | 0個");
                BGM_Type_L.Items.Add("20～45mm:自車両-ズーム時 | 0個");
                BGM_Type_L.Items.Add("20～45mm:他車両 | 0個");
                BGM_Type_L.Items.Add("50～75mm:自車両-通常 | 0個");
                BGM_Type_L.Items.Add("50～75mm:自車両-ズーム時 | 0個");
                BGM_Type_L.Items.Add("50～75mm:他車両 | 0個");
                BGM_Type_L.Items.Add("85～107mm:自車両-通常 | 0個");
                BGM_Type_L.Items.Add("85～107mm:自車両-ズーム時 | 0個");
                BGM_Type_L.Items.Add("85～107mm:他車両 | 0個");
                BGM_Type_L.Items.Add("115～152mm:自車両-通常 | 0個");
                BGM_Type_L.Items.Add("115～152mm:自車両-ズーム時 | 0個");
                BGM_Type_L.Items.Add("115～152mm:他車両 | 0個");
                BGM_Type_L.Items.Add("152mm以上:自車両-通常 | 0個");
                BGM_Type_L.Items.Add("152mm以上:自車両-ズーム時 | 0個");
                BGM_Type_L.Items.Add("152mm以上:他車両 | 0個");
                BGM_Type_L.Items.Add("ロケット:自車両-通常 | 0個");
                BGM_Type_L.Items.Add("ロケット:自車両-ズーム時 | 0個");
                BGM_Type_L.Items.Add("ロケット:他車両 | 0個");
                BGM_Type_L.Items.Add("音声(12～23mm以外):自車両 | 0個");
                BGM_Type_L.Items.Add("音声(12～23mm以外):他車両 | 0個");
                if (Music_Type_WoTB_Gun.Count == 0)
                    for (int Number = 0; Number < BGM_Type_L.Items.Count; Number++)
                        Music_Type_WoTB_Gun.Add(new List<string>());
            }
            else if (Mod_Page == 3)
            {
                Mod_Name_T.Text = "砲撃音Mod(WoT用)";
                BGM_Type_L.Items.Add("12～23mm:自車両-通常 | 0個");
                BGM_Type_L.Items.Add("12～23mm:自車両-ズーム時 | 0個");
                BGM_Type_L.Items.Add("12～23mm:他車両 | 0個");
                BGM_Type_L.Items.Add("20～45mm:自車両-通常 | 0個");
                BGM_Type_L.Items.Add("20～45mm:自車両-ズーム時 | 0個");
                BGM_Type_L.Items.Add("20～45mm:他車両 | 0個");
                BGM_Type_L.Items.Add("50～75mm:自車両-通常 | 0個");
                BGM_Type_L.Items.Add("50～75mm:自車両-ズーム時 | 0個");
                BGM_Type_L.Items.Add("50～75mm:他車両 | 0個");
                BGM_Type_L.Items.Add("85～105mm:自車両-通常 | 0個");
                BGM_Type_L.Items.Add("85～105mm:自車両-ズーム時 | 0個");
                BGM_Type_L.Items.Add("85～105mm:他車両 | 0個");
                BGM_Type_L.Items.Add("85～105mm 2連装砲:自車両-通常 | 0個");
                BGM_Type_L.Items.Add("85～105mm 2連装砲:自車両-ズーム時 | 0個");
                BGM_Type_L.Items.Add("85～105mm 2連装砲:他車両 | 0個");
                BGM_Type_L.Items.Add("105mm:自車両-通常 | 0個");
                BGM_Type_L.Items.Add("105mm:自車両-ズーム時 | 0個");
                BGM_Type_L.Items.Add("105mm:他車両 | 0個");
                BGM_Type_L.Items.Add("105mm 2連装砲:自車両-通常 | 0個");
                BGM_Type_L.Items.Add("105mm 2連装砲:自車両-ズーム時 | 0個");
                BGM_Type_L.Items.Add("105mm 2連装砲:他車両 | 0個");
                BGM_Type_L.Items.Add("115～120mm:自車両-通常 | 0個");
                BGM_Type_L.Items.Add("115～120mm:自車両-ズーム時 | 0個");
                BGM_Type_L.Items.Add("115～120mm:他車両 | 0個");
                BGM_Type_L.Items.Add("115～120mm 2連装砲:自車両-通常 | 0個");
                BGM_Type_L.Items.Add("115～120mm 2連装砲:自車両-ズーム時 | 0個");
                BGM_Type_L.Items.Add("115～120mm 2連装砲:他車両 | 0個");
                BGM_Type_L.Items.Add("128mm:自車両-通常 | 0個");
                BGM_Type_L.Items.Add("128mm:自車両-ズーム時 | 0個");
                BGM_Type_L.Items.Add("128mm:他車両 | 0個");
                BGM_Type_L.Items.Add("152mm以上:自車両-通常 | 0個");
                BGM_Type_L.Items.Add("152mm以上:自車両-ズーム時 | 0個");
                BGM_Type_L.Items.Add("152mm以上:他車両 | 0個");
                BGM_Type_L.Items.Add("音声(12～23mm以外):自車両 | 0個");
                BGM_Type_L.Items.Add("音声(12～23mm以外):他車両 | 0個");
                if (Music_Type_WoT_Gun.Count == 0)
                    for (int Number = 0; Number < BGM_Type_L.Items.Count; Number++)
                        Music_Type_WoT_Gun.Add(new List<string>());
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
                    StreamReader str = Sub_Code.File_Decrypt_To_Stream(Voice_Set.Special_Path + "/Configs/Create_Loading_BGM.conf", "Create_Loading_BGM_Configs_Save");
                    Volume_S.Value = double.Parse(str.ReadLine());
                    str.Close();
                    str.Dispose();
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show("設定を読み込めませんでした。。\nエラー回避のため設定は削除されます。");
                    File.Delete(Voice_Set.Special_Path + "/Configs/Create_Loading_BGM.conf");
                    Sub_Code.Error_Log_Write(e.Message);
                }
            }
            Mod_Page_Change();
            Update_Music_Type_List();
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
                            if (Mod_Page == 0)
                                Position_S.Value = Music_Play_Times[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex].Start_Time;
                            else
                                Position_S.Value = 0;
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
                        if (BGM_Type_L.SelectedIndex != -1 && BGM_Music_L.SelectedIndex != -1 && Mod_Page == 0)
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
        async void EndSync(int handle, int channel, int data, IntPtr user)
        {
            if (!IsEnded)
            {
                await Task.Delay(500);
                IsEnded = true;
            }
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
            if (IsMessageShowing)
            {
                IsMessageShowing = false;
                Message_T.Text = "";
            }
            Message_T.Opacity = 1;
        }
        //ウィンドウを閉じる
        private async void Back_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsDownloading)
            {
                MessageBoxResult result = MessageBox.Show("プロジェクトファイルをダウンロード中です。中止しますか?", "確認",
                    MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes);
                if (result == MessageBoxResult.Yes)
                    IsDownloading = false;
                else
                    return;
            }
            else if (IsClosing || IsBusy)
            {
                return;
            }
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
            try
            {
                File.Delete(Voice_Set.Special_Path + "\\Gun.dat");
            }
            catch { }
            IsClosing = false;
            Visibility = Visibility.Hidden;
        }
        //ロードBGMの種類が変更された場合、右の欄に追加されている曲を表示
        private void BGM_Type_L_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BGM_Type_L.SelectedIndex == -1)
                return;
            Pause_Volume_Animation(true, 10f);
            BGM_Music_L.Items.Clear();
            if (Mod_Page == 0)
                foreach (string Name in Music_Type_Music[BGM_Type_L.SelectedIndex])
                    BGM_Music_L.Items.Add(Path.GetFileName(Name));
            else if (Mod_Page == 1)
                foreach (string Name in Music_Type_Garage_SE[BGM_Type_L.SelectedIndex])
                    BGM_Music_L.Items.Add(Path.GetFileName(Name));
            else if (Mod_Page == 2)
                foreach (string Name in Music_Type_WoTB_Gun[BGM_Type_L.SelectedIndex])
                    BGM_Music_L.Items.Add(Path.GetFileName(Name));
            else if (Mod_Page == 3)
                foreach (string Name in Music_Type_WoT_Gun[BGM_Type_L.SelectedIndex])
                    BGM_Music_L.Items.Add(Path.GetFileName(Name));
        }
        //左の欄のBGM数を更新
        void Update_Music_Type_List()
        {
            int SelectedIndex = BGM_Type_L.SelectedIndex;
            for (int Number = 0; Number < BGM_Type_L.Items.Count; Number++)
            {
                string Name = BGM_Type_L.Items[Number].ToString();
                Name = Name.Substring(0, Name.IndexOf('|') + 2);
                if (Mod_Page == 0)
                    BGM_Type_L.Items[Number] = Name + Music_Type_Music[Number].Count + "個";
                else if (Mod_Page == 1)
                    BGM_Type_L.Items[Number] = Name + Music_Type_Garage_SE[Number].Count + "個";
                else if (Mod_Page == 2)
                    BGM_Type_L.Items[Number] = Name + Music_Type_WoTB_Gun[Number].Count + "個";
                else if (Mod_Page == 3)
                    BGM_Type_L.Items[Number] = Name + Music_Type_WoT_Gun[Number].Count + "個";
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
                Music_Count += Music_Type_Music[Number].Count;
            for (int Number = 0; Number < Music_Type_Garage_SE.Count; Number++)
                Music_Count += Music_Type_Garage_SE[Number].Count;
            for (int Number = 0; Number < Music_Type_WoTB_Gun.Count; Number++)
                Music_Count += Music_Type_WoTB_Gun[Number].Count;
            for (int Number = 0; Number < Music_Type_WoT_Gun.Count; Number++)
                Music_Count += Music_Type_WoT_Gun[Number].Count;
            if (Music_Count == 0)
            {
                Message_Feed_Out("セーブする際、少なくとも1つはサウンドファイルを追加する必要があります。");
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
                        for (int Number_01 = 0; Number_01 < Music_Type_Music[Number].Count; Number_01++)
                            stw.WriteLine(Number + "|" + Music_Type_Music[Number][Number_01] + "|" +
                                Music_Play_Times[Number][Number_01].Start_Time + "～" + Music_Play_Times[Number][Number_01].End_Time + "|" + Music_Feed_In[Number][Number_01]);
                    stw.WriteLine("!---ここからガレージ内のSEの項目です。---!");
                    for (int Number = 0; Number < Music_Type_Garage_SE.Count; Number++)
                        for (int Number_01 = 0; Number_01 < Music_Type_Garage_SE[Number].Count; Number_01++)
                            stw.WriteLine(Number + "|" + Music_Type_Garage_SE[Number][Number_01]);
                    stw.WriteLine("!---ここから砲撃音(WoTB用)の項目です。---!");
                    for (int Number = 0; Number < Music_Type_WoTB_Gun.Count; Number++)
                        for (int Number_01 = 0; Number_01 < Music_Type_WoTB_Gun[Number].Count; Number_01++)
                            stw.WriteLine(Number + "|" + Music_Type_WoTB_Gun[Number][Number_01]);
                    stw.WriteLine("!---ここから砲撃音(WoT用)の項目です。V1.1---!");
                    for (int Number = 0; Number < Music_Type_WoT_Gun.Count; Number++)
                        for (int Number_01 = 0; Number_01 < Music_Type_WoT_Gun[Number].Count; Number_01++)
                            stw.WriteLine(Number + "|" + Music_Type_WoT_Gun[Number][Number_01]);
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
                for (int Number = 0; Number < Music_Type_Music.Count; Number++)
                    Music_Type_Music[Number].Clear();
                for (int Number = 0; Number < Music_Play_Times.Count; Number++)
                    Music_Play_Times[Number].Clear();
                for (int Number = 0; Number < Music_Type_Garage_SE.Count; Number++)
                    Music_Type_Garage_SE[Number].Clear();
                for (int Number = 0; Number < Music_Type_WoTB_Gun.Count; Number++)
                    Music_Type_WoTB_Gun[Number].Clear();
                for (int Number = 0; Number < Music_Type_WoT_Gun.Count; Number++)
                    Music_Type_WoT_Gun[Number].Clear();
                Play_Time_T.Text = "再生時間:0～0";
                BGM_Music_L.Items.Clear();
                StreamReader str = Sub_Code.File_Decrypt_To_Stream(WMS_File, "SRTTbacon_WoTB_Loading_Music_Mode");
                string line;
                bool IsOneLine = false;
                bool IsVersion_Upgrade_Mode = false;
                int Sound_Mode = 0;
                bool IsOldVersion = false;
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
                    if (line.Contains("!---ここからガレージ内のSEの項目です。---!"))
                    {
                        Sound_Mode = 1;
                        continue;
                    }
                    else if (line.Contains("!---ここから砲撃音(WoTB用)の項目です。---!") || line.Contains("!---ここから砲撃音の項目です。---!"))
                    {
                        Sound_Mode = 2;
                        continue;
                    }
                    else if (line.Contains("!---ここから砲撃音(WoT用)の項目です。V1.1---!"))
                    {
                        Sound_Mode = 3;
                        continue;
                    }
                    else if (line.Contains("!---ここから砲撃音(WoT用)の項目です。---!"))
                    {
                        Sound_Mode = -1;
                        IsOldVersion = true;
                        continue;
                    }
                    string[] Line_Split = line.Split('|');
                    int Index = int.Parse(Line_Split[0]);
                    string FilePath = Line_Split[1];
                    if (Sound_Mode == 0)
                    {
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
                    else if (Sound_Mode == 1)
                        Music_Type_Garage_SE[Index].Add(FilePath);
                    else if (Sound_Mode == 2)
                        Music_Type_WoTB_Gun[Index].Add(FilePath);
                    else if (Sound_Mode == 3)
                        Music_Type_WoT_Gun[Index].Add(FilePath);
                }
                str.Close();
                str.Dispose();
                Feed_In_C.IsChecked = false;
                Update_Music_Type_List();
                Message_Feed_Out("ロードしました。");
                if (IsOldVersion)
                    Message_Feed_Out("砲撃音Mod(WoT用)をロードできませんでした。ロード元のバージョンが古い可能性があります。");
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
            if (Mod_Page == 0)
            {
                if (!File.Exists(Music_Type_Music[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex]))
                {
                    Message_Feed_Out("ファイルが存在しませんでした。");
                    return;
                }
                int StreamHandle = Bass.BASS_StreamCreateFile(Music_Type_Music[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex], 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_LOOP);
                Stream = BassFx.BASS_FX_TempoCreate(StreamHandle, BASSFlag.BASS_FX_FREESOURCE);
            }
            else if (Mod_Page == 1)
            {
                if (!File.Exists(Music_Type_Garage_SE[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex]))
                {
                    Message_Feed_Out("ファイルが存在しませんでした。");
                    return;
                }
                int StreamHandle = Bass.BASS_StreamCreateFile(Music_Type_Garage_SE[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex], 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_LOOP);
                Stream = BassFx.BASS_FX_TempoCreate(StreamHandle, BASSFlag.BASS_FX_FREESOURCE);
            }
            else if (Mod_Page == 2)
            {
                if (!File.Exists(Music_Type_WoTB_Gun[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex]))
                {
                    Message_Feed_Out("ファイルが存在しませんでした。");
                    return;
                }
                int StreamHandle = Bass.BASS_StreamCreateFile(Music_Type_WoTB_Gun[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex], 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_LOOP);
                Stream = BassFx.BASS_FX_TempoCreate(StreamHandle, BASSFlag.BASS_FX_FREESOURCE);
            }
            else if (Mod_Page == 3)
            {
                if (!File.Exists(Music_Type_WoT_Gun[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex]))
                {
                    Message_Feed_Out("ファイルが存在しませんでした。");
                    return;
                }
                int StreamHandle = Bass.BASS_StreamCreateFile(Music_Type_WoT_Gun[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex], 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_LOOP);
                Stream = BassFx.BASS_FX_TempoCreate(StreamHandle, BASSFlag.BASS_FX_FREESOURCE);
            }
            IsMusicEnd = new SYNCPROC(EndSync);
            Bass.BASS_ChannelGetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, ref SetFirstFreq);
            Bass.BASS_ChannelSetSync(Stream, BASSSync.BASS_SYNC_END | BASSSync.BASS_SYNC_MIXTIME, 0, IsMusicEnd, IntPtr.Zero);
            Bass.BASS_ChannelSetDevice(Stream, Video_Mode.Sound_Device);
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)Volume_S.Value / 100);
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, SetFirstFreq * (float)(Speed_S.Value / 50));
            Position_S.Maximum = Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetLength(Stream, BASSMode.BASS_POS_BYTES));
            if (Mod_Page == 0)
            {
                double End_Time = Music_Play_Times[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex].End_Time;
                if (End_Time == 0)
                    End_Time = Position_S.Maximum;
                Play_Time_T.Text = "再生時間:" + (int)Music_Play_Times[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex].Start_Time + "～" + (int)End_Time;
                Feed_In_C.IsChecked = Music_Feed_In[BGM_Type_L.SelectedIndex][BGM_Music_L.SelectedIndex];
            }
            else
                Play_Time_T.Text = "再生時間:0～" + (int)Position_S.Maximum;
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
                stw.WriteLine(Volume_S.Value);
                stw.Write(Mod_Page);
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
                    if (Mod_Page == 0)
                    {
                        if (Music_Type_Music[BGM_Type_L.SelectedIndex].Contains(FilePath))
                        {
                            Error_FileName += "\n" + Path.GetFileName(FilePath);
                            continue;
                        }
                        Music_Type_Music[BGM_Type_L.SelectedIndex].Add(FilePath);
                        Music_Play_Times[BGM_Type_L.SelectedIndex].Add(new Music_Play_Time(0, 0));
                        if (BGM_Type_L.SelectedIndex == 12 || BGM_Type_L.SelectedIndex == 14 || BGM_Type_L.SelectedIndex == 16 || BGM_Type_L.SelectedIndex == 19 || BGM_Type_L.SelectedIndex == 20)
                            Music_Feed_In[BGM_Type_L.SelectedIndex].Add(false);
                        else
                            Music_Feed_In[BGM_Type_L.SelectedIndex].Add(true);
                    }
                    else if (Mod_Page == 1)
                    {
                        if (Music_Type_Garage_SE[BGM_Type_L.SelectedIndex].Contains(FilePath))
                        {
                            Error_FileName += "\n" + Path.GetFileName(FilePath);
                            continue;
                        }
                        Music_Type_Garage_SE[BGM_Type_L.SelectedIndex].Add(FilePath);
                    }
                    else if (Mod_Page == 2)
                    {
                        if (Music_Type_WoTB_Gun[BGM_Type_L.SelectedIndex].Contains(FilePath))
                        {
                            Error_FileName += "\n" + Path.GetFileName(FilePath);
                            continue;
                        }
                        Music_Type_WoTB_Gun[BGM_Type_L.SelectedIndex].Add(FilePath);
                    }
                    else if (Mod_Page == 3)
                    {
                        if (Music_Type_WoT_Gun[BGM_Type_L.SelectedIndex].Contains(FilePath))
                        {
                            Error_FileName += "\n" + Path.GetFileName(FilePath);
                            continue;
                        }
                        Music_Type_WoT_Gun[BGM_Type_L.SelectedIndex].Add(FilePath);
                    }
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
            if (Mod_Page == 0)
            {
                Music_Type_Music[BGM_Type_L.SelectedIndex].RemoveAt(BGM_Music_L.SelectedIndex);
                Music_Play_Times[BGM_Type_L.SelectedIndex].RemoveAt(BGM_Music_L.SelectedIndex);
                Music_Feed_In[BGM_Type_L.SelectedIndex].RemoveAt(BGM_Music_L.SelectedIndex);
                BGM_Music_L.Items.RemoveAt(BGM_Music_L.SelectedIndex);
                Feed_In_C.IsChecked = false;
            }
            else if (Mod_Page == 1)
                Music_Type_Garage_SE[BGM_Type_L.SelectedIndex].RemoveAt(BGM_Music_L.SelectedIndex);
            else if (Mod_Page == 2)
                Music_Type_WoTB_Gun[BGM_Type_L.SelectedIndex].RemoveAt(BGM_Music_L.SelectedIndex);
            else if (Mod_Page == 3)
                Music_Type_WoT_Gun[BGM_Type_L.SelectedIndex].RemoveAt(BGM_Music_L.SelectedIndex);
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
        private async void Create_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy || IsOpenDialog)
                return;
            if (Mod_Page == 0)
            {
                int Music_Count = 0;
                for (int Number = 0; Number < Music_Type_Music.Count; Number++)
                    Music_Count += Music_Type_Music[Number].Count;
                if (Music_Count == 0)
                {
                    Message_Feed_Out("最低1つはBGM(音声)ファイルを選択する必要があります。");
                    return;
                }
                bool IsHitsUpdated = await Hits_Project_Download(false);
                if (!IsHitsUpdated)
                {
                    Message_Feed_Out("被弾音をダウンロードまたはアップデートする必要があります。");
                    return;
                }
            }
            else if (Mod_Page == 1)
            {
                bool IsExist = false;
                for (int Number = 0; Number < Music_Type_Garage_SE.Count; Number++)
                {
                    if (Music_Type_Garage_SE[Number].Count > 0)
                    {
                        IsExist = true;
                        break;
                    }
                }
                if (!IsExist)
                {
                    Message_Feed_Out("最低1つはサウンドファイルを入れる必要があります。");
                    return;
                }
            }
            else if (Mod_Page == 2)
            {
                bool IsExist = false;
                for (int Number = 0; Number < Music_Type_WoTB_Gun.Count; Number++)
                {
                    if (Music_Type_WoTB_Gun[Number].Count > 0)
                    {
                        IsExist = true;
                        break;
                    }
                }
                if (!IsExist)
                {
                    Message_Feed_Out("最低1つはサウンドファイルを入れる必要があります。");
                    return;
                }
            }
            else if (Mod_Page == 3)
            {
                bool IsExist = false;
                for (int Number = 0; Number < Music_Type_WoT_Gun.Count; Number++)
                {
                    if (Music_Type_WoT_Gun[Number].Count > 0)
                    {
                        IsExist = true;
                        break;
                    }
                }
                if (!IsExist)
                {
                    Message_Feed_Out("最低1つはサウンドファイルを入れる必要があります。");
                    return;
                }
            }
            Music_Mod_Create(false);
        }
        //指定した項目のみ作成
        private async void Create_One_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy || IsOpenDialog)
                return;
            if (BGM_Type_L.SelectedIndex == -1)
            {
                Message_Feed_Out("\"BGM(音声)の種類\"を選択する必要があります。");
                return;
            }
            if (Mod_Page == 0)
            {
                IList<int> Select_Indexs = BGM_Type_L.SelectedIndexes();
                if (Select_Indexs.Contains(13) || Select_Indexs.Contains(14) || Select_Indexs.Contains(15) || Select_Indexs.Contains(16))
                {
                    if (Music_Type_Music[13].Count == 0 || Music_Type_Music[15].Count == 0)
                    {
                        Message_Feed_Out("引き分け、または敗北はどちらともに1つ以上BGMを入れる必要があります。");
                        return;
                    }
                }
                else if (Select_Indexs.Contains(16) || Select_Indexs.Contains(17))
                {
                    if (Music_Type_Music[16].Count == 0 || Music_Type_Music[17].Count == 0)
                    {
                        Message_Feed_Out("優勢はどちらともに1つ以上BGMを入れる必要があります。");
                        return;
                    }
                }
                else if (Music_Type_Music[BGM_Type_L.SelectedIndex].Count == 0)
                {
                    Message_Feed_Out("選択したタイプに最低1つはBGM(音声)ファイルを追加する必要があります。");
                    return;
                }
                bool IsHitsUpdated = await Hits_Project_Download(false);
                if (!IsHitsUpdated)
                {
                    Message_Feed_Out("被弾音のプロジェクトをダウンロードまたはアップデートする必要があります。");
                    return;
                }
            }
            else if (Mod_Page == 1)
            {
                bool IsExist = false;
                for (int Number = 0; Number < Music_Type_Garage_SE.Count; Number++)
                {
                    if (Music_Type_Garage_SE[Number].Count > 0)
                    {
                        IsExist = true;
                        break;
                    }
                }
                if (!IsExist)
                {
                    Message_Feed_Out("最低1つはサウンドファイルを入れる必要があります。");
                    return;
                }
            }
            else if (Mod_Page == 2)
            {
                bool IsExist = false;
                for (int Number = 0; Number < Music_Type_WoTB_Gun.Count; Number ++)
                {
                    if (Music_Type_WoTB_Gun[Number].Count > 0)
                    {
                        IsExist = true;
                        break;
                    }
                }
                if (!IsExist)
                {
                    Message_Feed_Out("最低1つはサウンドファイルを入れる必要があります。");
                    return;
                }
            }
            else if (Mod_Page == 3)
            {
                bool IsExist = false;
                for (int Number = 0; Number < Music_Type_WoT_Gun.Count; Number++)
                {
                    if (Music_Type_WoT_Gun[Number].Count > 0)
                    {
                        IsExist = true;
                        break;
                    }
                }
                if (!IsExist)
                {
                    Message_Feed_Out("最低1つはサウンドファイルを入れる必要があります。");
                    return;
                }
            }
            Music_Mod_Create(true);
        }
        async Task<bool> Hits_Project_Download(bool IsAll)
        {
            IList<int> Select_Indexs = BGM_Type_L.SelectedIndexes();
            if (Select_Indexs.Contains(19) || Select_Indexs.Contains(20) || IsAll)
            {
                if (File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Hits_Sound/Version.dat"))
                {
                    StreamReader str = new StreamReader(Voice_Set.Special_Path + "/Wwise/WoTB_Hits_Sound/Version.dat");
                    string Ver = str.ReadLine();
                    str.Close();
                    if (Sub_Code.IsWwise_Hits_Update != Ver)
                    {
                        double SizeMB = (double)(Voice_Set.FTPClient.GetFileSize("/WoTB_Voice_Mod/Update/Wwise/Wwise_Hits_Project_01.zip") / 1024.0 / 1024.0);
                        SizeMB = (Math.Floor(SizeMB * 10)) / 10;
                        MessageBoxResult result = MessageBox.Show("被弾音のアップデートがあります。データをダウンロードしますか?\nサイズ:およそ" + SizeMB + "MB", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes);
                        if (result == MessageBoxResult.Yes)
                        {
                            IsMessageShowing = false;
                            Message_T.Opacity = 1;
                            Message_T.Text = "アップデートしています...\nダウンロードサイズ:" + SizeMB + "MB";
                            await Task.Delay(75);
                            if (Directory.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Hits_Sound"))
                                Directory.Delete(Voice_Set.Special_Path + "/Wwise/WoTB_Hits_Sound", true);
                            File.Delete(Voice_Set.Special_Path + "/Wwise_Hits_Project.dat");
                            Voice_Set.FTPClient.DownloadFile("/WoTB_Voice_Mod/Update/Wwise/Wwise_Hits_Project_01.zip", Voice_Set.Special_Path + "/Wwise_Hits_Project.dat");
                            System.IO.Compression.ZipFile.ExtractToDirectory(Voice_Set.Special_Path + "/Wwise_Hits_Project.dat", Voice_Set.Special_Path + "/Wwise/WoTB_Hits_Sound");
                            File.Delete(Voice_Set.Special_Path + "/Wwise_Hits_Project.dat");
                            File.WriteAllText(Voice_Set.Special_Path + "/Wwise/WoTB_Hits_Sound/Version.dat", Sub_Code.IsWwise_Hits_Update);
                            Message_Feed_Out("プロジェクトデータをダウンロードしました。");
                        }
                        else
                            return false;
                    }
                }
                else
                {
                    double SizeMB = (double)(Voice_Set.FTPClient.GetFileSize("/WoTB_Voice_Mod/Update/Wwise/Wwise_Hits_Project_01.zip") / 1024.0 / 1024.0);
                    SizeMB = (Math.Floor(SizeMB * 10)) / 10;
                    MessageBoxResult result = MessageBox.Show("被弾音のプロジェクトデータが存在しません。ダウンロードしますか?\nサイズ:およそ" + SizeMB + "MB", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes);
                    if (result == MessageBoxResult.Yes)
                    {
                        IsMessageShowing = false;
                        Message_T.Opacity = 1;
                        Message_T.Text = "サーバーからデータを取得しています...\nダウンロードサイズ:" + SizeMB + "MB";
                        await Task.Delay(75);
                        if (Directory.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Hits_Sound"))
                            Directory.Delete(Voice_Set.Special_Path + "/Wwise/WoTB_Hits_Sound", true);
                        File.Delete(Voice_Set.Special_Path + "/Wwise_Hits_Project.dat");
                        Voice_Set.FTPClient.DownloadFile("/WoTB_Voice_Mod/Update/Wwise/Wwise_Hits_Project_01.zip", Voice_Set.Special_Path + "/Wwise_Hits_Project.dat");
                        System.IO.Compression.ZipFile.ExtractToDirectory(Voice_Set.Special_Path + "/Wwise_Hits_Project.dat", Voice_Set.Special_Path + "/Wwise/WoTB_Hits_Sound");
                        File.Delete(Voice_Set.Special_Path + "/Wwise_Hits_Project.dat");
                        File.WriteAllText(Voice_Set.Special_Path + "/Wwise/WoTB_Hits_Sound/Version.dat", Sub_Code.IsWwise_Hits_Update);
                        Message_Feed_Out("プロジェクトデータをダウンロードしました。");
                    }
                    else
                        return false;
                }
            }
            return true;
        }
        //すべて作成する場合は-1
        async void Music_Mod_Create(bool IsSelectedOnly)
        {
            IsOpenDialog = true;
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
                    IsOpenDialog = false;
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
                List<string> IsNotWAVList = new List<string>();
                await Task.Delay(50);
                try
                {
                    if (Mod_Page == 0)
                    {
                        FileInfo fi = new FileInfo(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.wwu");
                        if (File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp") && fi.Length >= 1000000)
                            File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.wwu", true);
                        if (!File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp"))
                            File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.wwu", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp", true);
                        if (File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Hits_Sound/Actor-Mixer Hierarchy/Backup.tmp") && fi.Length >= 800000)
                            File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Hits_Sound/Actor-Mixer Hierarchy/Backup.tmp", Voice_Set.Special_Path + "/Wwise/WoTB_Hits_Sound/Actor-Mixer Hierarchy/Default Work Unit.wwu", true);
                        if (!File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Hits_Sound/Actor-Mixer Hierarchy/Backup.tmp") && File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Hits_Sound/Actor-Mixer Hierarchy/Default Work Unit.wwu"))
                            File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Hits_Sound/Actor-Mixer Hierarchy/Default Work Unit.wwu", Voice_Set.Special_Path + "/Wwise/WoTB_Hits_Sound/Actor-Mixer Hierarchy/Backup.tmp", true);
                        int Set_Volume = (int)(-40 * (1 - Volume_WoTB_S.Value / 100));
                        Wwise_Project_Create Wwise = new Wwise_Project_Create(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod");
                        //数を合わせるため使用しない項目を入れています。
                        string[] Loading_Music_Name = { "America_lakville", "America_overlord", "Chinese", "Desert_airfield", "Desert_sand_river","Europe_himmelsdorf",
                "Europe_mannerheim","Europe_ruinberg","Japan","Russian_malinovka","Russian_prokhorovka","リザルト(勝利)","リザルト(勝利)","リザルト(敗北、または引き分け)","リザルト(敗北、または引き分け)",
                "リザルト(敗北、または引き分け)","リザルト(敗北、または引き分け)","優勢(敵味方両方)","優勢(敵味方両方)"};
                        string[] Loading_Music_Type = { "music_maps_america_lakville", "music_maps_america_overlord", "music_maps_chinese", "music_maps_desert_airfield",
                "music_maps_desert_sand_river","music_maps_europe_himmelsdorf","music_maps_europe_mannerheim","music_maps_europe_ruinberg","music_maps_japan",
                "music_maps_russian_malinovka","music_maps_russian_prokhorovka","music_result_screen_basic","music_result_screen_basic", "music_result_screen","music_result_screen",
                "music_result_screen","music_result_screen","music_battle","music_battle"};
                        IList<int> Selection_Index = BGM_Type_L.SelectedIndexes();
                        List<string> Build_Names = new List<string>();
                        if (!IsSelectedOnly)
                        {
                            for (int Number = 0; Number < Music_Type_Music.Count; Number++)
                            {
                                for (int Number_01 = 0; Number_01 < Music_Type_Music[Number].Count; Number_01++)
                                {
                                    if (Path.GetExtension(Music_Type_Music[Number][Number_01]) == ".wav" && !Sub_Code.Audio_IsWAV(Music_Type_Music[Number][Number_01]))
                                        IsNotWAVList.Add(Music_Type_Music[Number][Number_01]);
                                    else
                                        Wwise.Loading_Music_Add_Wwise(Music_Type_Music[Number][Number_01], Number, Music_Play_Times[Number][Number_01], Music_Feed_In[Number][Number_01], Set_Volume);
                                }
                            }
                            Message_T.Text = "ファイルを.wavにエンコードしています...";
                            await Task.Delay(50);
                            await Wwise.Sound_To_WAV();
                            Wwise.Save();
                            for (int Number = 0; Number < BGM_Type_L.Items.Count; Number++)
                            {
                                if (Number != 19 && Number != 20)
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
                        }
                        else
                        {
                            Message_T.Text = "ファイルを.wavにエンコードしています...";
                            await Task.Delay(50);
                            foreach (int Number in Selection_Index)
                            {
                                if (Number != 19 && Number != 20)
                                {
                                    for (int Number_01 = 0; Number_01 < Music_Type_Music[Number].Count; Number_01++)
                                    {
                                        if (Path.GetExtension(Music_Type_Music[Number][Number_01]) == ".wav" && !Sub_Code.Audio_IsWAV(Music_Type_Music[Number][Number_01]))
                                            IsNotWAVList.Add(Music_Type_Music[Number][Number_01]);
                                        else
                                            Wwise.Loading_Music_Add_Wwise(Music_Type_Music[Number][Number_01], Number, Music_Play_Times[Number][Number_01], Music_Feed_In[Number][Number_01], Set_Volume);
                                    }
                                }
                            }
                            await Wwise.Sound_To_WAV();
                            Wwise.Save();
                            foreach (int Number in Selection_Index)
                            {
                                if (Number != 19 && Number != 20 && Music_Type_Music[Number].Count > 0)
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
                        Build_Names.Clear();
                        await Task.Delay(100);
                        Wwise.Clear();
                        if (File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp"))
                            File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.wwu", true);
                        if (!IsSelectedOnly || Selection_Index.Contains(19) || Selection_Index.Contains(20))
                        {
                            Wwise_Project_Create Wwise_Hits = new Wwise_Project_Create(Voice_Set.Special_Path + "/Wwise/WoTB_Hits_Sound");
                            for (int Number = 19; Number < Music_Type_Music.Count; Number++)
                            {
                                foreach (string File_Now in Music_Type_Music[Number])
                                {
                                    if (Number == 19)
                                        Wwise_Hits.Add_Sound("618741068", File_Now, "SFX");
                                    else if (Number == 20)
                                        Wwise_Hits.Add_Sound("48041438", File_Now, "SFX");
                                }
                            }
                            await Wwise_Hits.Sound_To_WAV();
                            Wwise_Hits.Save();
                            Message_T.Text = "hits.bnkとhits_basic.bnkをビルドしています...";
                            await Task.Delay(75);
                            Wwise_Hits.Project_Build("hits", bfb.SelectedFolder + "/hits.bnk", null, true);
                            Wwise_Hits.Project_Build("hits_basic", bfb.SelectedFolder + "/hits_basic.bnk", null, true);
                            Wwise_Hits.Clear(true, null, true);
                            if (File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Hits_Sound/Actor-Mixer Hierarchy/Backup.tmp"))
                                File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Hits_Sound/Actor-Mixer Hierarchy/Backup.tmp", Voice_Set.Special_Path + "/Wwise/WoTB_Hits_Sound/Actor-Mixer Hierarchy/Default Work Unit.wwu", true);
                        }
                    }
                    else if (Mod_Page == 1)
                    {
                        if (!File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_UI_Button_Sound/Actor-Mixer Hierarchy/Backup.tmp"))
                            File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_UI_Button_Sound/Actor-Mixer Hierarchy/Default Work Unit.wwu", Voice_Set.Special_Path + "/Wwise/WoTB_UI_Button_Sound/Actor-Mixer Hierarchy/Backup.tmp", true);
                        Wwise_Project_Create Wwise = new Wwise_Project_Create(Voice_Set.Special_Path + "/Wwise/WoTB_UI_Button_Sound");
                        for (int Number = 0; Number < Music_Type_Garage_SE.Count; Number++)
                        {
                            for (int Number_01 = 0; Number_01 < Music_Type_Garage_SE[Number].Count; Number_01++)
                            {
                                if (Path.GetExtension(Music_Type_Garage_SE[Number][Number_01]) == ".wav" && !Sub_Code.Audio_IsWAV(Music_Type_Garage_SE[Number][Number_01]))
                                    IsNotWAVList.Add(Music_Type_Garage_SE[Number][Number_01]);
                                else
                                    Wwise.Loading_Music_Add_Wwise(Music_Type_Garage_SE[Number][Number_01], Number, null, false, 0, 1);
                            }
                        }
                        Message_T.Text = "ファイルを.wavにエンコードしています...";
                        await Task.Delay(50);
                        await Wwise.Sound_To_WAV();
                        Wwise.Save();
                        Message_T.Text = "ui_buttons_tasks.bnkをビルドしています...";
                        await Task.Delay(75);
                        Wwise.Project_Build("ui_buttons_tasks", bfb.SelectedFolder + "/ui_buttons_tasks.bnk");
                        Wwise.Clear();
                        if (File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_UI_Button_Sound/Actor-Mixer Hierarchy/Backup.tmp"))
                            File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_UI_Button_Sound/Actor-Mixer Hierarchy/Backup.tmp", Voice_Set.Special_Path + "/Wwise/WoTB_UI_Button_Sound/Actor-Mixer Hierarchy/Default Work Unit.wwu", true);
                    }
                    else if (Mod_Page == 2)
                    {
                        if (!File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Gun_Sound/Actor-Mixer Hierarchy/Backup.tmp"))
                            File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Gun_Sound/Actor-Mixer Hierarchy/Default Work Unit.wwu", Voice_Set.Special_Path + "/Wwise/WoTB_Gun_Sound/Actor-Mixer Hierarchy/Backup.tmp", true);
                        Wwise_Project_Create Wwise = new Wwise_Project_Create(Voice_Set.Special_Path + "/Wwise/WoTB_Gun_Sound");
                        for (int Number = 0; Number < Music_Type_WoTB_Gun.Count; Number++)
                        {
                            for (int Number_01 = 0; Number_01 < Music_Type_WoTB_Gun[Number].Count; Number_01++)
                            {
                                if (Path.GetExtension(Music_Type_WoTB_Gun[Number][Number_01]) == ".wav" && !Sub_Code.Audio_IsWAV(Music_Type_WoTB_Gun[Number][Number_01]))
                                    IsNotWAVList.Add(Music_Type_WoTB_Gun[Number][Number_01]);
                                else
                                    Wwise.Loading_Music_Add_Wwise(Music_Type_WoTB_Gun[Number][Number_01], Number, null, false, 0, 2);
                            }
                        }
                        Message_T.Text = "ファイルを.wavにエンコードしています...";
                        await Task.Delay(50);
                        await Wwise.Sound_To_WAV();
                        Wwise.Save();
                        Message_T.Text = "weapon.bnkをビルドしています...";
                        await Task.Delay(75);
                        Wwise.Project_Build("weapon", bfb.SelectedFolder + "/weapon.bnk");
                        Message_T.Text = "weapon_basic.bnkをビルドしています...";
                        await Task.Delay(75);
                        Wwise.Project_Build("weapon_basic", bfb.SelectedFolder + "/weapon_basic.bnk");
                        Wwise.Clear();
                        if (File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Gun_Sound/Actor-Mixer Hierarchy/Backup.tmp"))
                            File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Gun_Sound/Actor-Mixer Hierarchy/Backup.tmp", Voice_Set.Special_Path + "/Wwise/WoTB_Gun_Sound/Actor-Mixer Hierarchy/Default Work Unit.wwu", true);
                    }
                    else if (Mod_Page == 3)
                    {
                        if (!File.Exists(Voice_Set.Special_Path + "/Wwise/WoT_Gun_Sound/Actor-Mixer Hierarchy/Backup.tmp"))
                            File.Copy(Voice_Set.Special_Path + "/Wwise/WoT_Gun_Sound/Actor-Mixer Hierarchy/Default Work Unit.wwu", Voice_Set.Special_Path + "/Wwise/WoT_Gun_Sound/Actor-Mixer Hierarchy/Backup.tmp", true);
                        Wwise_Project_Create Wwise = new Wwise_Project_Create(Voice_Set.Special_Path + "/Wwise/WoT_Gun_Sound");
                        for (int Number = 0; Number < Music_Type_WoT_Gun.Count; Number++)
                        {
                            for (int Number_01 = 0; Number_01 < Music_Type_WoT_Gun[Number].Count; Number_01++)
                            {
                                if (Path.GetExtension(Music_Type_WoT_Gun[Number][Number_01]) == ".wav" && !Sub_Code.Audio_IsWAV(Music_Type_WoT_Gun[Number][Number_01]))
                                    IsNotWAVList.Add(Music_Type_WoT_Gun[Number][Number_01]);
                                else
                                    Wwise.Loading_Music_Add_Wwise(Music_Type_WoT_Gun[Number][Number_01], Number, null, false, 0, 3);
                            }
                        }
                        Message_T.Text = "ファイルを.wavにエンコードしています...";
                        await Task.Delay(50);
                        await Wwise.Sound_To_WAV();
                        Wwise.Save();
                        Message_T.Text = "wpn.bnkをビルドしています...";
                        await Task.Delay(75);
                        Wwise.Project_Build("wpn", bfb.SelectedFolder + "/wpn.bnk");
                        Wwise.Clear();
                        if (File.Exists(Voice_Set.Special_Path + "/Wwise/WoT_Gun_Sound/Actor-Mixer Hierarchy/Backup.tmp"))
                            File.Copy(Voice_Set.Special_Path + "/Wwise/WoT_Gun_Sound/Actor-Mixer Hierarchy/Backup.tmp", Voice_Set.Special_Path + "/Wwise/WoT_Gun_Sound/Actor-Mixer Hierarchy/Default Work Unit.wwu", true);
                    }
                }
                catch (Exception e)
                {
                    Sub_Code.Error_Log_Write(e.Message);
                    Message_Feed_Out("エラーが発生しました。開発者(SRTTbacon)にご連絡ください。");
                    IsBusy = false;
                    return;
                }
                bool IsNoneError = true;
                if (IsNotWAVList.Count > 0)
                {
                    StreamWriter stw = File.CreateText(Directory.GetCurrentDirectory() + "\\Error_Extension.txt");
                    stw.WriteLine("以下のファイルの拡張子が正しくありません。適切な拡張子に修正し再度実行してください。");
                    foreach (string File_Now in IsNotWAVList)
                        stw.WriteLine(File_Now);
                    stw.Write("意味がよく分からない方はご質問ください。");
                    stw.Close();
                    stw.Dispose();
                    IsNoneError = false;
                }
                if (IsNoneError)
                    Message_Feed_Out("完了しました。指定したフォルダを参照してください。");
                else
                    Message_Feed_Out("拡張子が正しくないファイルが存在したため、正常に作成できませんでした。詳しくはError_Extension.txtを参照してください。");
                Flash.Flash_Start();
                IsBusy = false;
            }
            bfb.Dispose();
            IsOpenDialog = false;
        }
        //クリア
        private async void Clear_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            int Music_Count = 0;
            for (int Number = 0; Number < Music_Type_Music.Count; Number++)
                Music_Count += Music_Type_Music[Number].Count;
            for (int Number = 0; Number < Music_Type_Garage_SE.Count; Number++)
                Music_Count += Music_Type_Garage_SE[Number].Count;
            for (int Number = 0; Number < Music_Type_WoTB_Gun.Count; Number++)
                Music_Count += Music_Type_WoTB_Gun[Number].Count;
            for (int Number = 0; Number < Music_Type_WoT_Gun.Count; Number++)
                Music_Count += Music_Type_WoT_Gun[Number].Count;
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
                for (int Number = 0; Number < Music_Type_Music.Count; Number++)
                    Music_Type_Music[Number].Clear();
                for (int Number = 0; Number < Music_Play_Times.Count; Number++)
                    Music_Play_Times[Number].Clear();
                for (int Number = 0; Number < Music_Feed_In.Count; Number++)
                    Music_Feed_In[Number].Clear();
                for (int Number = 0; Number < Music_Type_Garage_SE.Count; Number++)
                    Music_Type_Garage_SE[Number].Clear();
                for (int Number = 0; Number < Music_Type_WoTB_Gun.Count; Number++)
                    Music_Type_WoTB_Gun[Number].Clear();
                for (int Number = 0; Number < Music_Type_WoT_Gun.Count; Number++)
                    Music_Type_WoT_Gun[Number].Clear();
                Play_Time_T.Text = "再生時間:0～0";
                Volume_WoTB_S.Value = 75;
                BGM_Music_L.Items.Clear();
                Update_Music_Type_List();
                BGM_Type_L.SelectedIndex = -1;
                Feed_In_C.IsChecked = false;
                Message_Feed_Out("内容をクリアしました。");
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
        void Mod_Page_Change()
        {
            BGM_Type_Change();
            BGM_Music_L.Items.Clear();
            Default_SE_B.Visibility = Visibility.Hidden;
            if (Mod_Page == 0)
            {
                Next_Mod_B.Visibility = Visibility.Visible;
                Back_Mod_B.Visibility = Visibility.Hidden;
                Start_B.Visibility = Visibility.Visible;
                End_B.Visibility = Visibility.Visible;
                Time_Clear_B.Visibility = Visibility.Visible;
                Feed_In_C.Visibility = Visibility.Visible;
                Feed_In_T.Visibility = Visibility.Visible;
                Create_One_B.Visibility = Visibility.Visible;
                Create_B.Margin = new Thickness(-1665, Create_B.Margin.Top, 0, 0);
            }
            else if (Mod_Page == 1 || Mod_Page == 2)
            {
                if (Mod_Page == 1)
                    Default_SE_B.Visibility = Visibility.Visible;
                Next_Mod_B.Visibility = Visibility.Visible;
                Back_Mod_B.Visibility = Visibility.Visible;
                Start_B.Visibility = Visibility.Hidden;
                End_B.Visibility = Visibility.Hidden;
                Time_Clear_B.Visibility = Visibility.Hidden;
                Feed_In_C.Visibility = Visibility.Hidden;
                Feed_In_T.Visibility = Visibility.Hidden;
                Create_One_B.Visibility = Visibility.Hidden;
                Create_B.Margin = new Thickness(-1975, Create_B.Margin.Top, 0, 0);
            }
            else if (Mod_Page == 3)
            {
                Next_Mod_B.Visibility = Visibility.Hidden;
                Back_Mod_B.Visibility = Visibility.Visible;
                Start_B.Visibility = Visibility.Hidden;
                End_B.Visibility = Visibility.Hidden;
                Time_Clear_B.Visibility = Visibility.Hidden;
                Feed_In_C.Visibility = Visibility.Hidden;
                Feed_In_T.Visibility = Visibility.Hidden;
                Create_One_B.Visibility = Visibility.Hidden;
                Create_B.Margin = new Thickness(-1975, Create_B.Margin.Top, 0, 0);
            }
            Configs_Save();
        }
        private void Volume_WoTB_Help_B_Click(object sender, RoutedEventArgs e)
        {
            string Message_01 = "・この設定は、'砲撃音を除く'全てのサウンドに当てはまります。個別で設定することはできません。\n";
            string Message_02 = "・実際に聞こえる音量は、この設定の数値とWoTB内のBGMの数値を足したものになります。\n";
            string Message_03 = "例えば、この設定を50にし、WoTBのBGM設定も50にすると、だいたい25くらいの音量になります。\n";
            string Message_04 = "試してみた感じ、この設定を30、WoTBのBGM設定を20にするとほとんど聞こえないような感じでした。ご参考までに。\n";
            string Message_05 = "・追記:砲撃音(WoT,WoTB共に)及びガレージSEには適応されません。";
            MessageBox.Show(Message_01 + Message_02 + Message_03 + Message_04 + Message_05);
        }
        private void Back_Mod_B_Click(object sender, RoutedEventArgs e)
        {
            if (Mod_Page > 0)
                Mod_Page--;
            Mod_Page_Change();
            Update_Music_Type_List();
        }
        private async void Next_Mod_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy || IsOpenDialog)
                return;
            //後からリストを追加しても大丈夫なように
            if (Mod_Page < 3)
                Mod_Page++;
            if (Mod_Page == 1 && File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_UI_Button_Sound/Version.dat"))
            {
                StreamReader str = new StreamReader(Voice_Set.Special_Path + "/Wwise/WoTB_UI_Button_Sound/Version.dat");
                string Ver = str.ReadLine();
                str.Close();
                if (Sub_Code.IsWwise_WoT_Gun_Update != Ver)
                {
                    IsMessageShowing = false;
                    Message_T.Text = "ガレージSEのプロジェクトをアップデートしています...";
                    await Task.Delay(50);
                    IsDownloading = true;
                    long Full_Size = Voice_Set.FTPClient.GetFileSize("/WoTB_Voice_Mod/Update/Wwise/WoTB_UI_Button_Sound.zip");
                    double SizeMB = (double)(Full_Size / 1024.0 / 1024.0);
                    SizeMB = (Math.Floor(SizeMB * 10)) / 10;
                    if (Directory.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_UI_Button_Sound"))
                        Directory.Delete(Voice_Set.Special_Path + "/Wwise/WoTB_UI_Button_Sound", true);
                    Task task = Task.Run(() =>
                    {
                        Voice_Set.FTPClient.DownloadFile("WoTB_Voice_Mod/Update/Wwise/WoTB_UI_Button_Sound.zip", Voice_Set.Special_Path + "\\WoTB_UI_Button.dat");
                    });
                    while (IsDownloading)
                    {
                        if (File.Exists(Voice_Set.Special_Path + "\\WoTB_UI_Button.dat"))
                        {
                            FileInfo fi = new FileInfo(Voice_Set.Special_Path + "\\WoTB_UI_Button.dat");
                            long File_Size_Now = fi.Length;
                            double Size_MB_Now = File_Size_Now / 1024.0 / 1024.0;
                            Size_MB_Now = (Math.Floor(Size_MB_Now * 10)) / 10;
                            if (File_Size_Now >= Full_Size)
                                break;
                            Message_T.Text = "プロジェクトファイルをダウンロードしています...\n" + Size_MB_Now + " / " + SizeMB + "MB";
                        }
                        await Task.Delay(100);
                    }
                    if (!IsDownloading)
                    {
                        Voice_Set.FTPClient.Stop_DownloadFile();
                        Mod_Page--;
                        IsBusy = false;
                        Message_Feed_Out("データのダウンロードを中止しました。");
                        return;
                    }
                    IsDownloading = false;
                    Message_T.Text = "ファイルを展開しています...";
                    await Task.Delay(50);
                    System.IO.Compression.ZipFile.ExtractToDirectory(Voice_Set.Special_Path + "\\WoTB_UI_Button.dat", Voice_Set.Special_Path + "\\Wwise\\WoTB_UI_Button_Sound");
                    File.Delete(Voice_Set.Special_Path + "\\WoTB_UI_Button.dat");
                    File.WriteAllText(Voice_Set.Special_Path + "/Wwise/WoTB_UI_Button_Sound/Version.dat", Sub_Code.IsWwise_WoT_Gun_Update);
                    Message_Feed_Out("正常にアップデートされました。");
                }
            }
            else if (Mod_Page == 2 && File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Gun_Sound/Version.dat"))
            {
                StreamReader str = new StreamReader(Voice_Set.Special_Path + "/Wwise/WoTB_Gun_Sound/Version.dat");
                string Ver = str.ReadLine();
                str.Close();
                if (Sub_Code.IsWwise_Gun_Update != Ver || Sub_Code.IsForceWwise_Gun_Update)
                {
                    IsMessageShowing = false;
                    Message_T.Text = "砲撃音(WoTB用)のプロジェクトをアップデートしています...";
                    await Task.Delay(50);
                    if (File.Exists(Voice_Set.Special_Path + "\\Wwise\\WoTB_Gun_Sound\\Actor-Mixer Hierarchy\\Default Work Unit.wwu"))
                        File.Delete(Voice_Set.Special_Path + "\\Wwise\\WoTB_Gun_Sound\\Actor-Mixer Hierarchy\\Default Work Unit.wwu");
                    if (File.Exists(Voice_Set.Special_Path + "\\Wwise\\WoTB_Gun_Sound\\Events\\SRTTbacon.wwu"))
                        File.Delete(Voice_Set.Special_Path + "\\Wwise\\WoTB_Gun_Sound\\Events\\SRTTbacon.wwu");
                    Voice_Set.FTPClient.DownloadFile("/WoTB_Voice_Mod/Update/Wwise/Gun_Project/Actor.wwu", Voice_Set.Special_Path + "\\Wwise\\WoTB_Gun_Sound\\Actor-Mixer Hierarchy\\Default Work Unit.wwu");
                    Voice_Set.FTPClient.DownloadFile("/WoTB_Voice_Mod/Update/Wwise/Gun_Project/Event.wwu", Voice_Set.Special_Path + "\\Wwise\\WoTB_Gun_Sound\\Events\\SRTTbacon.wwu");
                    File.WriteAllText(Voice_Set.Special_Path + "/Wwise/WoTB_Gun_Sound/Version.dat", Sub_Code.IsWwise_Gun_Update);
                    Sub_Code.IsForceWwise_Gun_Update = false;
                    Message_Feed_Out("正常にアップデートされました。");
                }
            }
            else if (Mod_Page == 3 && File.Exists(Voice_Set.Special_Path + "/Wwise/WoT_Gun_Sound/Version.dat"))
            {
                StreamReader str = new StreamReader(Voice_Set.Special_Path + "/Wwise/WoT_Gun_Sound/Version.dat");
                string Ver = str.ReadLine();
                str.Close();
                if (Sub_Code.IsWwise_WoT_Gun_Update != Ver)
                {
                    IsMessageShowing = false;
                    Message_T.Text = "砲撃音(WoT用)のプロジェクトをアップデートしています...";
                    await Task.Delay(50);
                    IsDownloading = true;
                    long Full_Size = Voice_Set.FTPClient.GetFileSize("/WoTB_Voice_Mod/Update/Wwise/WoT_Gun_Sound.zip");
                    double SizeMB = (double)(Full_Size / 1024.0 / 1024.0);
                    SizeMB = (Math.Floor(SizeMB * 10)) / 10;
                    if (Directory.Exists(Voice_Set.Special_Path + "/Wwise/WoT_Gun_Sound"))
                        Directory.Delete(Voice_Set.Special_Path + "/Wwise/WoT_Gun_Sound", true);
                    Task task = Task.Run(() =>
                    {
                        Voice_Set.FTPClient.DownloadFile("WoTB_Voice_Mod/Update/Wwise/WoT_Gun_Sound.zip", Voice_Set.Special_Path + "\\WoT_Gun.dat");
                    });
                    while (IsDownloading)
                    {
                        FileInfo fi = new FileInfo(Voice_Set.Special_Path + "\\WoT_Gun.dat");
                        long File_Size_Now = fi.Length;
                        double Size_MB_Now = File_Size_Now / 1024.0 / 1024.0;
                        Size_MB_Now = (Math.Floor(Size_MB_Now * 10)) / 10;
                        if (File_Size_Now >= Full_Size)
                            break;
                        Message_T.Text = "プロジェクトファイルをダウンロードしています...\n" + Size_MB_Now + " / " + SizeMB + "MB";
                        await Task.Delay(100);
                    }
                    if (!IsDownloading)
                    {
                        Voice_Set.FTPClient.Stop_DownloadFile();
                        Mod_Page--;
                        IsBusy = false;
                        Message_Feed_Out("データのダウンロードを中止しました。");
                        return;
                    }
                    IsDownloading = false;
                    Message_T.Text = "ファイルを展開しています...";
                    await Task.Delay(50);
                    System.IO.Compression.ZipFile.ExtractToDirectory(Voice_Set.Special_Path + "\\WoT_Gun.dat", Voice_Set.Special_Path + "\\Wwise\\WoT_Gun_Sound");
                    File.Delete(Voice_Set.Special_Path + "\\WoT_Gun.dat");
                    File.WriteAllText(Voice_Set.Special_Path + "/Wwise/WoT_Gun_Sound/Version.dat", Sub_Code.IsWwise_WoT_Gun_Update);
                    Message_Feed_Out("正常にアップデートされました。");
                }
            }
            else if (Mod_Page == 1 || Mod_Page == 2 || Mod_Page == 3)
            {
                long Full_Size = 0;
                if (Mod_Page == 1)
                    Full_Size = Voice_Set.FTPClient.GetFileSize("/WoTB_Voice_Mod/Update/Wwise/WoTB_UI_Button_Sound.zip");
                else if (Mod_Page == 2)
                    Full_Size = Voice_Set.FTPClient.GetFileSize("/WoTB_Voice_Mod/Update/Wwise/WoTB_Gun_Sound.zip");
                else if (Mod_Page == 3)
                    Full_Size = Voice_Set.FTPClient.GetFileSize("/WoTB_Voice_Mod/Update/Wwise/WoT_Gun_Sound.zip");
                double SizeMB = (double)(Full_Size / 1024.0 / 1024.0);
                SizeMB = (Math.Floor(SizeMB * 10)) / 10;
                string Message = "";
                if (Mod_Page == 1)
                    Message = "ガレージSEのプロジェクトファイルをダウンロードする必要があります。\nサイズ:約" + SizeMB + "MB  -  続行しますか?";
                else if (Mod_Page == 2)
                    Message = "砲撃音のプロジェクトファイル(WoTB用)をダウンロードする必要があります。\nサイズ:約" + SizeMB + "MB  -  続行しますか?";
                else if (Mod_Page == 3)
                    Message = "砲撃音のプロジェクトファイル(WoT用)をダウンロードする必要があります。\nサイズ:約" + SizeMB + "MB  -  続行しますか?";
                MessageBoxResult result = MessageBox.Show(Message, "確認",
                    MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes);
                if (result == MessageBoxResult.Yes)
                {
                    IsBusy = true;
                    try
                    {
                        IsMessageShowing = false;
                        Message_T.Text = "プロジェクトファイルをダウンロードしています...";
                        await Task.Delay(50);
                        IsDownloading = true;
                        if (Mod_Page == 1 && Directory.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_UI_Button_Sound"))
                            Directory.Delete(Voice_Set.Special_Path + "/Wwise/WoTB_UI_Button_Sound", true);
                        else if (Mod_Page == 2 && Directory.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Gun_Sound"))
                            Directory.Delete(Voice_Set.Special_Path + "/Wwise/WoTB_Gun_Sound", true);
                        else if (Mod_Page == 3 && Directory.Exists(Voice_Set.Special_Path + "/Wwise/WoT_Gun_Sound"))
                            Directory.Delete(Voice_Set.Special_Path + "/Wwise/WoT_Gun_Sound", true);
                        Task task = Task.Run(() =>
                        {
                            if (Mod_Page == 1)
                                Voice_Set.FTPClient.DownloadFile("WoTB_Voice_Mod/Update/Wwise/WoTB_UI_Button_Sound.zip", Voice_Set.Special_Path + "\\WoTB_UI_Button.dat");
                            else if (Mod_Page == 2)
                                Voice_Set.FTPClient.DownloadFile("WoTB_Voice_Mod/Update/Wwise/WoTB_Gun_Sound.zip", Voice_Set.Special_Path + "\\WoTB_Gun.dat");
                            else if (Mod_Page == 3)
                                Voice_Set.FTPClient.DownloadFile("WoTB_Voice_Mod/Update/Wwise/WoT_Gun_Sound.zip", Voice_Set.Special_Path + "\\WoT_Gun.dat");
                        });
                        while (IsDownloading)
                        {
                            long File_Size_Now = 0;
                            if (Mod_Page == 1 && File.Exists(Voice_Set.Special_Path + "\\WoTB_UI_Button.dat"))
                            {
                                FileInfo fi = new FileInfo(Voice_Set.Special_Path + "\\WoTB_UI_Button.dat");
                                File_Size_Now = fi.Length;
                            }
                            else if (Mod_Page == 2 && File.Exists(Voice_Set.Special_Path + "\\WoTB_Gun.dat"))
                            {
                                FileInfo fi = new FileInfo(Voice_Set.Special_Path + "\\WoTB_Gun.dat");
                                File_Size_Now = fi.Length;
                            }
                            else if (Mod_Page == 3 && File.Exists(Voice_Set.Special_Path + "\\WoT_Gun.dat"))
                            {
                                FileInfo fi = new FileInfo(Voice_Set.Special_Path + "\\WoT_Gun.dat");
                                File_Size_Now = fi.Length;
                            }
                            double Size_MB_Now = File_Size_Now / 1024.0 / 1024.0;
                            Size_MB_Now = (Math.Floor(Size_MB_Now * 10)) / 10;
                            if (File_Size_Now >= Full_Size)
                                break;
                            Message_T.Text = "プロジェクトファイルをダウンロードしています...\n" + Size_MB_Now + " / " + SizeMB + "MB";
                            await Task.Delay(100);
                        }
                        if (!IsDownloading)
                        {
                            Voice_Set.FTPClient.Stop_DownloadFile();
                            Mod_Page--;
                            IsBusy = false;
                            Message_Feed_Out("データのダウンロードを中止しました。");
                            return;
                        }
                        IsDownloading = false;
                        Message_T.Text = "ファイルを展開しています...";
                        await Task.Delay(50);
                        if (Mod_Page == 1)
                        {
                            System.IO.Compression.ZipFile.ExtractToDirectory(Voice_Set.Special_Path + "\\WoTB_UI_Button.dat", Voice_Set.Special_Path + "\\Wwise\\WoTB_UI_Button_Sound");
                            File.Delete(Voice_Set.Special_Path + "\\WoTB_UI_Button.dat");
                            File.WriteAllText(Voice_Set.Special_Path + "/Wwise/WoTB_UI_Button_Sound/Version.dat", Sub_Code.IsWwise_UI_Button_Sound);
                        }
                        else if (Mod_Page == 2)
                        {
                            System.IO.Compression.ZipFile.ExtractToDirectory(Voice_Set.Special_Path + "\\WoTB_Gun.dat", Voice_Set.Special_Path + "\\Wwise\\WoTB_Gun_Sound");
                            File.Delete(Voice_Set.Special_Path + "\\WoTB_Gun.dat");
                            File.WriteAllText(Voice_Set.Special_Path + "/Wwise/WoTB_Gun_Sound/Version.dat", Sub_Code.IsWwise_Gun_Update);
                        }
                        else if (Mod_Page == 3)
                        {
                            System.IO.Compression.ZipFile.ExtractToDirectory(Voice_Set.Special_Path + "\\WoT_Gun.dat", Voice_Set.Special_Path + "\\Wwise\\WoT_Gun_Sound");
                            File.Delete(Voice_Set.Special_Path + "\\WoT_Gun.dat");
                            File.WriteAllText(Voice_Set.Special_Path + "/Wwise/WoT_Gun_Sound/Version.dat", Sub_Code.IsWwise_WoT_Gun_Update);
                        }
                        Message_Feed_Out("展開が完了しました。");
                    }
                    catch (Exception e1)
                    {
                        Sub_Code.Error_Log_Write(e1.Message);
                        Message_Feed_Out("エラーが発生しました。詳しくはError_Log.txtを参照してください。");
                        Mod_Page--;
                        IsBusy = false;
                        IsDownloading = false;
                        return;
                    }
                    IsBusy = false;
                }
                else
                {
                    Mod_Page--;
                    return;
                }
            }
            Mod_Page_Change();
            Update_Music_Type_List();
        }
        private void Create_Help_B_Click(object sender, RoutedEventArgs e)
        {
            string Message_01 = "・2ページ目のガレージSE及び、3,4ページ目の砲撃音関連の項目を表示している場合、作成(選択のみ)のボタンは表示されません。\n";
            string Message_02 = "・作成(すべて)は、現在表示しているページの中のすべてなので、2,3,4ページ目で作成(すべて)を押しても砲撃音しか作成されません。";
            MessageBox.Show(Message_01 + Message_02);
        }
        void Set_Garage_Default_SE()
        {
            for (int Number = 0; Number < Music_Type_Garage_SE.Count; Number++)
            {
                if (Number == 0)
                    Set_Garage_Default_SE_By_Name(Number, "176974408.mp3");
                else if (Number == 2)
                    Set_Garage_Default_SE_By_Name(Number, "262333918.mp3");
                else if (Number == 4)
                    Set_Garage_Default_SE_By_Name(Number, "440745850.wav");
                else if (Number == 6)
                    Set_Garage_Default_SE_By_Name(Number, "394850995.wav");
                else if (Number == 8)
                {
                    Set_Garage_Default_SE_By_Name(Number, "376157875.wav");
                    Set_Garage_Default_SE_By_Name(Number, "118267228.wav");
                }
                else if (Number == 10)
                    Set_Garage_Default_SE_By_Name(Number, "166277761.wav");
                else if (Number == 12)
                    Set_Garage_Default_SE_By_Name(Number, "387372795.wav");
                else if (Number == 14)
                    Set_Garage_Default_SE_By_Name(Number, "843967064.mp3");
                else if (Number == 16)
                    Set_Garage_Default_SE_By_Name(Number, "493097325.wav");
                else if (Number == 18)
                    Set_Garage_Default_SE_By_Name(Number, "437893724.mp3");
            }
            if (Mod_Page == 1 && BGM_Type_L.SelectedIndex != -1)
                foreach (string Name in Music_Type_Garage_SE[BGM_Type_L.SelectedIndex])
                    BGM_Music_L.Items.Add(Path.GetFileName(Name));
        }
        void Set_Garage_Default_SE_By_Name(int Index, string SE_Name)
        {
            string SE_File = Voice_Set.Special_Path + "\\Wwise\\WoTB_UI_Button_Sound\\SE\\" + SE_Name;
            if (Music_Type_Garage_SE[Index].Contains(SE_File))
                return;
            Music_Type_Garage_SE[Index].Add(SE_File);
        }
        private void Default_SE_B_Click(object sender, RoutedEventArgs e)
        {
            Set_Garage_Default_SE();
            Update_Music_Type_List();
            Message_Feed_Out("標準のSEをリストに追加しました。");
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Mod_Pageの数だけ初期化
            for (int Number = 2; Number >= 0; Number--)
            {
                Mod_Page = Number;
                BGM_Type_Change();
            }
        }
    }
}