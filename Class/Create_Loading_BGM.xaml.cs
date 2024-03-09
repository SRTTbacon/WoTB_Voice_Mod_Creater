using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
        public double Max_Time { get; set; }
        public Music_Play_Time(double Set_Start_Time, double Set_End_Time)
        {
            Start_Time = Set_Start_Time;
            End_Time = Set_End_Time;
            Max_Time = 0;
        }
        public Music_Play_Time Clone()
        {
            return (Music_Play_Time)MemberwiseClone();
        }
    }
    public partial class Create_Loading_BGM : UserControl
    {
        public readonly List<List<Other_Type_List>> Other_List = new List<List<Other_Type_List>>();
        public readonly WMS_Load WMS_File = new WMS_Load();
        string Project_Name = "";
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
        SYNCPROC IsMusicEnd;
        readonly Brush Gray_Text = (Brush)new BrushConverter().ConvertFromString("#BFFF2C8C");
        public Create_Loading_BGM()
        {
            InitializeComponent();
            Position_S.AddHandler(MouseDownEvent, new MouseButtonEventHandler(Position_S_MouseDown), true);
            Position_S.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Position_S_MouseUp), true);
            Volume_S.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Volume_S_MouseUp), true);
            Default_SE_B.Visibility = Visibility.Hidden;
            Change_OverWrite_Visibility();
        }
        public void Other_Init(int Mod_Page, List<List<Other_Type_List>> Other_List = null, bool IsSetSE = true)
        {
            if (Other_List == null)
                Other_List = this.Other_List;
            if (Mod_Page == 0)
            {
                Other_List[0].Clear();
                Other_List[0].Add(new Other_Type_List("ロード1:America_lakville", 0, true));
                Other_List[0].Add(new Other_Type_List("ロード2:America_overlord", 1, true));
                Other_List[0].Add(new Other_Type_List("ロード3:Chinese", 2, true));
                Other_List[0].Add(new Other_Type_List("ロード4:Desert_airfield", 3, true));
                Other_List[0].Add(new Other_Type_List("ロード5:Desert_sand_river", 4, true));
                Other_List[0].Add(new Other_Type_List("ロード6:Europe_himmelsdorf", 5, true));
                Other_List[0].Add(new Other_Type_List("ロード7:Europe_mannerheim", 6, true));
                Other_List[0].Add(new Other_Type_List("ロード8:Europe_ruinberg", 7, true));
                Other_List[0].Add(new Other_Type_List("ロード9:Japan", 8, true));
                Other_List[0].Add(new Other_Type_List("ロード10:Russian_malinovka", 9, true));
                Other_List[0].Add(new Other_Type_List("ロード11:Russian_prokhorovka", 10, true));
                Other_List[0].Add(new Other_Type_List("リザルト:勝利-BGM", 11, true));
                Other_List[0].Add(new Other_Type_List("リザルト:勝利-音声", 12));
                Other_List[0].Add(new Other_Type_List("リザルト:引き分け-BGM", 13, true));
                Other_List[0].Add(new Other_Type_List("リザルト:引き分け-音声", 14));
                Other_List[0].Add(new Other_Type_List("リザルト:敗北-BGM", 15, true));
                Other_List[0].Add(new Other_Type_List("リザルト:敗北-音声", 16));
                Other_List[0].Add(new Other_Type_List("優勢:味方", 17, true));
                Other_List[0].Add(new Other_Type_List("優勢:敵", 18, true));
                Other_List[0].Add(new Other_Type_List("被弾:貫通-音声", 19));
                Other_List[0].Add(new Other_Type_List("被弾:非貫通-音声", 20));
            }
            else if (Mod_Page == 1)
            {
                Other_List[1].Clear();
                Other_List[1].Add(new Other_Type_List("コンテナ開封-ノーマル-SE", 0));
                Other_List[1].Add(new Other_Type_List("コンテナ開封-ノーマル-音声", 1));
                Other_List[1].Add(new Other_Type_List("コンテナ開封-レア-SE", 2));
                Other_List[1].Add(new Other_Type_List("コンテナ開封-レア-音声", 3));
                Other_List[1].Add(new Other_Type_List("購入-SE", 4));
                Other_List[1].Add(new Other_Type_List("購入-音声", 5));
                Other_List[1].Add(new Other_Type_List("売却-SE", 6));
                Other_List[1].Add(new Other_Type_List("売却-音声", 7));
                Other_List[1].Add(new Other_Type_List("チェックボックス-SE", 8));
                Other_List[1].Add(new Other_Type_List("チェックボックス-音声", 9));
                Other_List[1].Add(new Other_Type_List("小隊受信-SE", 10));
                Other_List[1].Add(new Other_Type_List("小隊受信-音声", 11));
                Other_List[1].Add(new Other_Type_List("モジュールの切り替え-SE", 12));
                Other_List[1].Add(new Other_Type_List("モジュールの切り替え-音声", 13));
                Other_List[1].Add(new Other_Type_List("戦闘開始-SE", 14));
                Other_List[1].Add(new Other_Type_List("戦闘開始-音声", 15));
                Other_List[1].Add(new Other_Type_List("ニュース-SE", 16));
                Other_List[1].Add(new Other_Type_List("ニュース-音声", 17));
                Other_List[1].Add(new Other_Type_List("車両購入-SE", 18));
                Other_List[1].Add(new Other_Type_List("車両購入-音声", 19));
                if (IsSetSE)
                    Set_Garage_Default_SE();
            }
            else if (Mod_Page == 2)
            {
                Other_List[2].Clear();
                Other_List[2].Add(new Other_Type_List("12～23mm:自車両-通常", 0));
                Other_List[2].Add(new Other_Type_List("12～23mm:自車両-ズーム時", 1));
                Other_List[2].Add(new Other_Type_List("12～23mm:他車両", 2));
                Other_List[2].Add(new Other_Type_List("20～45mm:自車両-通常", 3));
                Other_List[2].Add(new Other_Type_List("20～45mm:自車両-ズーム時", 4));
                Other_List[2].Add(new Other_Type_List("20～45mm:他車両", 5));
                Other_List[2].Add(new Other_Type_List("50～75mm:自車両-通常", 6));
                Other_List[2].Add(new Other_Type_List("50～75mm:自車両-ズーム時", 7));
                Other_List[2].Add(new Other_Type_List("50～75mm:他車両", 8));
                Other_List[2].Add(new Other_Type_List("85～107mm:自車両-通常", 9));
                Other_List[2].Add(new Other_Type_List("85～107mm:自車両-ズーム時", 10));
                Other_List[2].Add(new Other_Type_List("85～107mm:他車両", 11));
                Other_List[2].Add(new Other_Type_List("115～152mm:自車両-通常", 12));
                Other_List[2].Add(new Other_Type_List("115～152mm:自車両-ズーム時", 13));
                Other_List[2].Add(new Other_Type_List("115～152mm:他車両", 14));
                Other_List[2].Add(new Other_Type_List("152mm以上:自車両-通常", 15));
                Other_List[2].Add(new Other_Type_List("152mm以上:自車両-ズーム時", 16));
                Other_List[2].Add(new Other_Type_List("152mm以上:他車両", 17));
                Other_List[2].Add(new Other_Type_List("152mm以上_Extra:自車両-通常", 18));
                Other_List[2].Add(new Other_Type_List("152mm以上_Extra:自車両-ズーム時", 19));
                Other_List[2].Add(new Other_Type_List("152mm以上_Extra:他車両", 20));
                Other_List[2].Add(new Other_Type_List("音声(12～23mm以外):自車両", 21));
                Other_List[2].Add(new Other_Type_List("音声(12～23mm以外):他車両", 22));
            }
            else if (Mod_Page == 3)
            {
                Other_List[3].Clear();
                Other_List[3].Add(new Other_Type_List("12～23mm:自車両-通常", 0));
                Other_List[3].Add(new Other_Type_List("12～23mm:自車両-ズーム時", 1));
                Other_List[3].Add(new Other_Type_List("12～23mm:他車両", 2));
                Other_List[3].Add(new Other_Type_List("20～45mm:自車両-通常", 3));
                Other_List[3].Add(new Other_Type_List("20～45mm:自車両-ズーム時", 4));
                Other_List[3].Add(new Other_Type_List("20～45mm:他車両", 5));
                Other_List[3].Add(new Other_Type_List("50～75mm:自車両-通常", 6));
                Other_List[3].Add(new Other_Type_List("50～75mm:自車両-ズーム時", 7));
                Other_List[3].Add(new Other_Type_List("50～75mm:他車両", 8));
                Other_List[3].Add(new Other_Type_List("85～105mm:自車両-通常", 9));
                Other_List[3].Add(new Other_Type_List("85～105mm:自車両-ズーム時", 10));
                Other_List[3].Add(new Other_Type_List("85～105mm:他車両", 11));
                Other_List[3].Add(new Other_Type_List("85～105mm 2連装砲:自車両-通常", 12));
                Other_List[3].Add(new Other_Type_List("85～105mm 2連装砲:自車両-ズーム時", 13));
                Other_List[3].Add(new Other_Type_List("85～105mm 2連装砲:他車両", 14));
                Other_List[3].Add(new Other_Type_List("105mm:自車両-通常", 15));
                Other_List[3].Add(new Other_Type_List("105mm:自車両-ズーム時", 16));
                Other_List[3].Add(new Other_Type_List("105mm:他車両", 17));
                Other_List[3].Add(new Other_Type_List("105mm 2連装砲:自車両-通常", 18));
                Other_List[3].Add(new Other_Type_List("105mm 2連装砲:自車両-ズーム時",19));
                Other_List[3].Add(new Other_Type_List("105mm 2連装砲:他車両", 20));
                Other_List[3].Add(new Other_Type_List("115～120mm:自車両-通常", 21));
                Other_List[3].Add(new Other_Type_List("115～120mm:自車両-ズーム時", 22));
                Other_List[3].Add(new Other_Type_List("115～120mm:他車両", 23));
                Other_List[3].Add(new Other_Type_List("115～120mm 2連装砲:自車両-通常", 24));
                Other_List[3].Add(new Other_Type_List("115～120mm 2連装砲:自車両-ズーム時", 25));
                Other_List[3].Add(new Other_Type_List("115～120mm 2連装砲:他車両", 26));
                Other_List[3].Add(new Other_Type_List("128mm:自車両-通常", 27));
                Other_List[3].Add(new Other_Type_List("128mm:自車両-ズーム時", 28));
                Other_List[3].Add(new Other_Type_List("128mm:他車両", 29));
                Other_List[3].Add(new Other_Type_List("152mm以上:自車両-通常", 30));
                Other_List[3].Add(new Other_Type_List("152mm以上:自車両-ズーム時", 31));
                Other_List[3].Add(new Other_Type_List("152mm以上:他車両", 32));
                Other_List[3].Add(new Other_Type_List("音声(12～23mm以外):自車両", 33));
                Other_List[3].Add(new Other_Type_List("音声(12～23mm以外):他車両", 34));
            }
        }
        void Set_Garage_Default_SE()
        {
            for (int Number = 0; Number < Other_List[1].Count; Number++)
            {
                if (Number == 0)
                    Set_Garage_Default_SE_By_Name(Number, Voice_Set.Special_Path + "\\Wwise\\WoTB_UI_Button_Sound\\SE\\176974408.mp3");
                else if (Number == 2)
                    Set_Garage_Default_SE_By_Name(Number, Voice_Set.Special_Path + "\\Wwise\\WoTB_UI_Button_Sound\\SE\\262333918.mp3");
                else if (Number == 4)
                    Set_Garage_Default_SE_By_Name(Number, Voice_Set.Special_Path + "\\Wwise\\WoTB_UI_Button_Sound\\SE\\440745850.wav");
                else if (Number == 6)
                    Set_Garage_Default_SE_By_Name(Number, Voice_Set.Special_Path + "\\Wwise\\WoTB_UI_Button_Sound\\SE\\394850995.wav");
                else if (Number == 8)
                {
                    Set_Garage_Default_SE_By_Name(Number, Voice_Set.Special_Path + "\\Wwise\\WoTB_UI_Button_Sound\\SE\\376157875.wav");
                    Set_Garage_Default_SE_By_Name(Number, Voice_Set.Special_Path + "\\Wwise\\WoTB_UI_Button_Sound\\SE\\118267228.wav");
                }
                else if (Number == 10)
                    Set_Garage_Default_SE_By_Name(Number, Voice_Set.Special_Path + "\\Wwise\\WoTB_UI_Button_Sound\\SE\\166277761.wav");
                else if (Number == 12)
                    Set_Garage_Default_SE_By_Name(Number, Voice_Set.Special_Path + "\\Wwise\\WoTB_UI_Button_Sound\\SE\\387372795.wav");
                else if (Number == 14)
                    Set_Garage_Default_SE_By_Name(Number, Voice_Set.Special_Path + "\\Wwise\\WoTB_UI_Button_Sound\\SE\\843967064.mp3");
                else if (Number == 16)
                    Set_Garage_Default_SE_By_Name(Number, Voice_Set.Special_Path + "\\Wwise\\WoTB_UI_Button_Sound\\SE\\493097325.wav");
                else if (Number == 18)
                    Set_Garage_Default_SE_By_Name(Number, Voice_Set.Special_Path + "\\Wwise\\WoTB_UI_Button_Sound\\SE\\437893724.mp3");
            }
        }
        void Set_Garage_Default_SE_By_Name(int Index, string SE_Name)
        {
            Other_List[1][Index].Files.Add(new Other_File_Type(SE_Name));
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
                                Position_S.Value = Other_List[Mod_Page][BGM_Type_L.SelectedIndex].Files[BGM_Music_L.SelectedIndex].Music_Time.Start_Time;
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
                        Bass.BASS_ChannelUpdate(Stream, 400);
                        long position = Bass.BASS_ChannelGetPosition(Stream);
                        Position_S.Value = Bass.BASS_ChannelBytes2Seconds(Stream, position);
                        if (BGM_Type_L.SelectedIndex != -1 && BGM_Music_L.SelectedIndex != -1 && Mod_Page == 0)
                        {
                            double End_Time = Other_List[Mod_Page][BGM_Type_L.SelectedIndex].Files[BGM_Music_L.SelectedIndex].Music_Time.End_Time;
                            if (End_Time != 0 && Position_S.Value >= End_Time)
                            {
                                Music_Pos_Change(Other_List[Mod_Page][BGM_Type_L.SelectedIndex].Files[BGM_Music_L.SelectedIndex].Music_Time.Start_Time, true);
                                long position2 = Bass.BASS_ChannelGetPosition(Stream);
                                Position_S.Value = Bass.BASS_ChannelBytes2Seconds(Stream, position2);
                            }
                            else if (Position_S.Value < Other_List[Mod_Page][BGM_Type_L.SelectedIndex].Files[BGM_Music_L.SelectedIndex].Music_Time.Start_Time)
                            {
                                Music_Pos_Change(Other_List[Mod_Page][BGM_Type_L.SelectedIndex].Files[BGM_Music_L.SelectedIndex].Music_Time.Start_Time, true);
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
            foreach (Other_File_Type Info in Other_List[Mod_Page][BGM_Type_L.SelectedIndex].Files)
                BGM_Music_L.Items.Add(Path.GetFileName(Info.Name_Text));
        }
        //左の欄のBGM数を更新
        void Update_Music_Type_List()
        {
            int SelectedIndex = BGM_Type_L.SelectedIndex;
            for (int Number = 0; Number < BGM_Type_L.Items.Count; Number++)
            {
                ListBoxItem item = BGM_Type_L.Items[Number] as ListBoxItem;
                string Name = item.Content.ToString();
                item.Content = Name.Substring(0, Name.IndexOf('|') + 2) + Other_List[Mod_Page][Number].Files.Count + "個";
                if (Other_List[Mod_Page][Number].Files.Count == 0)
                    item.Foreground = Gray_Text;
                else
                    item.Foreground = Brushes.Aqua;
            }
            BGM_Type_L.SelectedIndex = SelectedIndex;
        }
        //セーブ
        private void Save_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            int Music_Count = 0;
            foreach (List<Other_Type_List> Info_List in Other_List)
                foreach (Other_Type_List Info in Info_List)
                    Music_Count += Info.Files.Count;
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
                    WMS_Save Save = new WMS_Save();
                    Save.Add_Sound(Other_List, WMS_File, WMS_Save.WMS_Save_Mode.All);
                    WMS_File.Dispose();
                    Save.Create(sfd.FileName, "None", false, OverWrite_C.IsChecked.Value);
                    Save.Dispose();
                    Load_From_File(sfd.FileName, false);
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
        public async void Load_From_File(string WMS_File, bool IsShowMessage = true)
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
                Play_Time_T.Text = "再生時間:0～0";
                BGM_Music_L.Items.Clear();
                Other_Init(0);
                Other_Init(1, null, false);
                Other_Init(2);
                Other_Init(3);
                if (WMS_Load.IsFullWMSFile(WMS_File))
                {
                    this.WMS_File.WMS_Load_File(WMS_File, Other_List);
                    Project_Name = this.WMS_File.Project_Name;
                    if (this.WMS_File.Version >= 3)
                        OverWrite_C.IsChecked = this.WMS_File.IsGunOverWriteMode;
                    if (this.WMS_File.Version < 3)
                    {
                        Other_List[2][18].Files.Clear();
                        Other_List[2][19].Files.Clear();
                        Other_List[2][20].Files.Clear();
                        if (IsShowMessage)
                            Message_Feed_Out("ロードしました。砲撃音の仕様が変更されたため、新たに設定する必要があります。");
                    }
                    else if (IsShowMessage)
                        Message_Feed_Out("ロードしました。");
                }
                else
                {
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
                                if (double.TryParse(str.ReadLine(), out Volume))
                                    Volume_WoTB_S.Value = Volume;
                            else
                                if (double.TryParse(line, out Volume))
                                    Volume_WoTB_S.Value = Volume;
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
                                Other_List[0][13].Files.Add(new Other_File_Type(FilePath, null, IsFeed_In_Mode, new Music_Play_Time(Start_Time, End_Time)));
                            else if (!IsVersion_Upgrade_Mode && Index == 13)
                                Other_List[0][15].Files.Add(new Other_File_Type(FilePath, null, IsFeed_In_Mode, new Music_Play_Time(Start_Time, End_Time)));
                            else if (!IsVersion_Upgrade_Mode && Index == 14)
                                Other_List[0][17].Files.Add(new Other_File_Type(FilePath, null, IsFeed_In_Mode, new Music_Play_Time(Start_Time, End_Time)));
                            else if (!IsVersion_Upgrade_Mode && Index == 15)
                                Other_List[0][18].Files.Add(new Other_File_Type(FilePath, null, IsFeed_In_Mode, new Music_Play_Time(Start_Time, End_Time)));
                            else
                                Other_List[0][Index].Files.Add(new Other_File_Type(FilePath, null, IsFeed_In_Mode, new Music_Play_Time(Start_Time, End_Time)));
                        }
                        else
                            Other_List[Sound_Mode][Index].Files.Add(new Other_File_Type(FilePath));
                    }
                    str.Close();
                    str.Dispose();
                    if (IsShowMessage)
                    {
                        Message_Feed_Out("ロードしました。");
                        if (IsOldVersion)
                            Message_Feed_Out("砲撃音Mod(WoT用)をロードできませんでした。ロード元のバージョンが古い可能性があります。");
                    }
                }
                Feed_In_C.IsChecked = false;
                Update_Music_Type_List();
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
            string Path = Other_List[Mod_Page][BGM_Type_L.SelectedIndex].Files[BGM_Music_L.SelectedIndex].Full_Path;
            if (!Path.Contains("\\") && WMS_File.IsLoaded)
                Path = WMS_File.Get_Sound_Path(Mod_Page, BGM_Type_L.SelectedIndex, BGM_Music_L.SelectedIndex);
            if (!File.Exists(Path))
            {
                Message_Feed_Out("ファイルが存在しませんでした。");
                return;
            }
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, 100);
            int StreamHandle = Bass.BASS_StreamCreateFile(Path, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_LOOP);
            Stream = BassFx.BASS_FX_TempoCreate(StreamHandle, BASSFlag.BASS_FX_FREESOURCE);
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, 500);
            IsMusicEnd = new SYNCPROC(EndSync);
            Bass.BASS_ChannelGetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, ref SetFirstFreq);
            Bass.BASS_ChannelSetSync(Stream, BASSSync.BASS_SYNC_END | BASSSync.BASS_SYNC_MIXTIME, 0, IsMusicEnd, IntPtr.Zero);
            Bass.BASS_ChannelSetDevice(Stream, Video_Mode.Sound_Device);
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)Volume_S.Value / 100);
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, SetFirstFreq * (float)(Speed_S.Value / 50));
            Position_S.Maximum = Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetLength(Stream, BASSMode.BASS_POS_BYTES));
            if (Mod_Page == 0)
            {
                double End_Time = Other_List[Mod_Page][BGM_Type_L.SelectedIndex].Files[BGM_Music_L.SelectedIndex].Music_Time.End_Time;
                if (End_Time == 0)
                    End_Time = Position_S.Maximum;
                Play_Time_T.Text = "再生時間:" + (int)Other_List[Mod_Page][BGM_Type_L.SelectedIndex].Files[BGM_Music_L.SelectedIndex].Music_Time.Start_Time + "～" + (int)End_Time;
                Feed_In_C.IsChecked = Other_List[Mod_Page][BGM_Type_L.SelectedIndex].Files[BGM_Music_L.SelectedIndex].Music_Feed_In;
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
                        if (Other_List[Mod_Page][BGM_Type_L.SelectedIndex].Files.Select(h => h.Full_Path).Contains(FilePath))
                        {
                            Error_FileName += "\n" + Path.GetFileName(FilePath);
                            continue;
                        }
                        if (BGM_Type_L.SelectedIndex == 12 || BGM_Type_L.SelectedIndex == 14 || BGM_Type_L.SelectedIndex == 16 || BGM_Type_L.SelectedIndex == 19 || BGM_Type_L.SelectedIndex == 20)
                            Other_List[Mod_Page][BGM_Type_L.SelectedIndex].Files.Add(new Other_File_Type(FilePath));
                        else
                            Other_List[Mod_Page][BGM_Type_L.SelectedIndex].Files.Add(new Other_File_Type(FilePath, null, true));
                    }
                    else if (Other_List[Mod_Page][BGM_Type_L.SelectedIndex].Files.Select(h => h.Full_Path).Contains(FilePath))
                    {
                        Error_FileName += "\n" + Path.GetFileName(FilePath);
                        continue;
                    }
                    else
                        Other_List[Mod_Page][BGM_Type_L.SelectedIndex].Files.Add(new Other_File_Type(FilePath));
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
            Other_List[Mod_Page][BGM_Type_L.SelectedIndex].Files.RemoveAt(BGM_Music_L.SelectedIndex);
            if (Mod_Page == 0)
            {
                BGM_Music_L.Items.RemoveAt(BGM_Music_L.SelectedIndex);
                Feed_In_C.IsChecked = false;
            }
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
            if (IsClosing || IsBusy || IsOpenDialog)
                return;
            if (Mod_Page == 0)
            {
                int Music_Count = 0;
                for (int Number = 0; Number < Other_List[Mod_Page].Count; Number++)
                    Music_Count += Other_List[Mod_Page][Number].Files.Count;
                if (Music_Count == 0)
                {
                    Message_Feed_Out("最低1つはBGM(音声)ファイルを選択する必要があります。");
                    return;
                }
            }
            else if (Mod_Page == 1)
            {
                bool IsExist = false;
                for (int Number = 0; Number < Other_List[Mod_Page].Count; Number++)
                {
                    if (Other_List[Mod_Page][Number].Files.Count > 0)
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
                for (int Number = 0; Number < Other_List[Mod_Page].Count; Number++)
                {
                    if (Other_List[Mod_Page][Number].Files.Count > 0)
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
                for (int Number = 0; Number < Other_List[Mod_Page].Count; Number++)
                {
                    if (Other_List[Mod_Page][Number].Files.Count > 0)
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
        private void Create_One_B_Click(object sender, RoutedEventArgs e)
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
                    if (Other_List[Mod_Page][13].Files.Count == 0 || Other_List[Mod_Page][15].Files.Count == 0)
                    {
                        Message_Feed_Out("引き分け、または敗北はどちらともに1つ以上BGMを入れる必要があります。");
                        return;
                    }
                }
                else if (Select_Indexs.Contains(16) || Select_Indexs.Contains(17))
                {
                    if (Other_List[Mod_Page][16].Files.Count == 0 || Other_List[Mod_Page][17].Files.Count == 0)
                    {
                        Message_Feed_Out("優勢はどちらともに1つ以上BGMを入れる必要があります。");
                        return;
                    }
                }
                else if (Other_List[Mod_Page][BGM_Type_L.SelectedIndex].Files.Count == 0)
                {
                    Message_Feed_Out("選択したタイプに最低1つはBGM(音声)ファイルを追加する必要があります。");
                    return;
                }
            }
            else if (Mod_Page == 1)
            {
                bool IsExist = false;
                for (int Number = 0; Number < Other_List[Mod_Page].Count; Number++)
                {
                    if (Other_List[Mod_Page][Number].Files.Count > 0)
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
                for (int Number = 0; Number < Other_List[Mod_Page].Count; Number ++)
                {
                    if (Other_List[Mod_Page][Number].Files.Count > 0)
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
                for (int Number = 0; Number < Other_List[Mod_Page].Count; Number++)
                {
                    if (Other_List[Mod_Page][Number].Files.Count > 0)
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
                Sub_Code.IsForceMusicStop = true;
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
                        if (File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/WoT_Blitz_Project.tmp"))
                            File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/WoT_Blitz_Project.tmp", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.wwu", true);
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
                            for (int Number = 0; Number < Other_List[Mod_Page].Count; Number++)
                            {
                                for (int Number_01 = 0; Number_01 < Other_List[Mod_Page][Number].Files.Count; Number_01++)
                                {
                                    if (Path.GetExtension(Other_List[Mod_Page][Number].Files[Number_01].Full_Path) == ".wav" && !Sub_Code.Audio_IsWAV(Other_List[Mod_Page][Number].Files[Number_01].Full_Path))
                                        IsNotWAVList.Add(Other_List[Mod_Page][Number].Files[Number_01].Full_Path);
                                    else
                                        Wwise.Loading_Music_Add_Wwise(Other_List[Mod_Page][Number].Files[Number_01].Full_Path, Number,
                                            Other_List[Mod_Page][Number].Files[Number_01].Music_Time, Other_List[Mod_Page][Number].Files[Number_01].Music_Feed_In, Set_Volume);
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
                                    if (Other_List[Mod_Page][Number].Files.Count > 0)
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
                                    for (int Number_01 = 0; Number_01 < Other_List[Mod_Page][Number].Files.Count; Number_01++)
                                    {
                                        if (Path.GetExtension(Other_List[Mod_Page][Number].Files[Number_01].Full_Path) == ".wav" && !Sub_Code.Audio_IsWAV(Other_List[Mod_Page][Number].Files[Number_01].Full_Path))
                                            IsNotWAVList.Add(Other_List[Mod_Page][Number].Files[Number_01].Full_Path);
                                        else
                                            Wwise.Loading_Music_Add_Wwise(Other_List[Mod_Page][Number].Files[Number_01].Full_Path, Number,
                                                Other_List[Mod_Page][Number].Files[Number_01].Music_Time, Other_List[Mod_Page][Number].Files[Number_01].Music_Feed_In, Set_Volume);
                                    }
                                }
                            }
                            await Wwise.Sound_To_WAV();
                            Wwise.Save();
                            foreach (int Number in Selection_Index)
                            {
                                if (Number != 19 && Number != 20 && Other_List[Mod_Page][Number].Files.Count > 0)
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
                        if (!IsSelectedOnly || Selection_Index.Contains(19) || Selection_Index.Contains(20))
                        {
                            Wwise_Project_Create Wwise_Hits = new Wwise_Project_Create(Voice_Set.Special_Path + "/Wwise/WoTB_Hits_Sound");
                            for (int Number = 19; Number < Other_List[Mod_Page].Count; Number++)
                            {
                                foreach (string File_Now in Other_List[Mod_Page][Number].Files.Select(h => h.Full_Path))
                                {
                                    if (Number == 19)
                                        Wwise_Hits.Add_Sound("195846168", File_Now, "SFX");
                                    else if (Number == 20)
                                        Wwise_Hits.Add_Sound("508923673", File_Now, "SFX");
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
                        for (int Number = 0; Number < Other_List[Mod_Page].Count; Number++)
                        {
                            for (int Number_01 = 0; Number_01 < Other_List[Mod_Page][Number].Files.Count; Number_01++)
                            {
                                if (Path.GetExtension(Other_List[Mod_Page][Number].Files[Number_01].Full_Path) == ".wav" && !Sub_Code.Audio_IsWAV(Other_List[Mod_Page][Number].Files[Number_01].Full_Path))
                                    IsNotWAVList.Add(Other_List[Mod_Page][Number].Files[Number_01].Full_Path);
                                else
                                    Wwise.Loading_Music_Add_Wwise(Other_List[Mod_Page][Number].Files[Number_01].Full_Path, Number, null, false, 0, 1);
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
                        if (!File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Gun_Sound_New/Actor-Mixer Hierarchy/Backup.tmp"))
                            File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Gun_Sound_New/Actor-Mixer Hierarchy/Default Work Unit.wwu", Voice_Set.Special_Path + "/Wwise/WoTB_Gun_Sound_New/Actor-Mixer Hierarchy/Backup.tmp", true);
                        Wwise_Project_Create Wwise = new Wwise_Project_Create(Voice_Set.Special_Path + "/Wwise/WoTB_Gun_Sound_New");
                        for (int Number_01 = 0; Number_01 < Other_List[2].Count; Number_01++)
                            if (Other_List[2][Number_01].Files.Count > 0 || !OverWrite_C.IsChecked.Value)
                                Wwise.Delete_CAkSounds(Sub_Code.Get_WoTB_New_Gun_Sound_ShortID(Number_01));
                        for (int Number = 0; Number < Other_List[Mod_Page].Count; Number++)
                        {
                            for (int Number_01 = 0; Number_01 < Other_List[Mod_Page][Number].Files.Count; Number_01++)
                            {
                                if (Path.GetExtension(Other_List[Mod_Page][Number].Files[Number_01].Full_Path) == ".wav" && !Sub_Code.Audio_IsWAV(Other_List[Mod_Page][Number].Files[Number_01].Full_Path))
                                    IsNotWAVList.Add(Other_List[Mod_Page][Number].Files[Number_01].Full_Path);
                                else
                                    Wwise.Loading_Music_Add_Wwise(Other_List[Mod_Page][Number].Files[Number_01].Full_Path, Number, null, false, 0, 2);
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
                        if (File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Gun_Sound_New/Actor-Mixer Hierarchy/Backup.tmp"))
                            File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Gun_Sound_New/Actor-Mixer Hierarchy/Backup.tmp", Voice_Set.Special_Path + "/Wwise/WoTB_Gun_Sound_New/Actor-Mixer Hierarchy/Default Work Unit.wwu", true);
                    }
                    else if (Mod_Page == 3)
                    {
                        if (!File.Exists(Voice_Set.Special_Path + "/Wwise/WoT_Gun_Sound/Actor-Mixer Hierarchy/Backup.tmp"))
                            File.Copy(Voice_Set.Special_Path + "/Wwise/WoT_Gun_Sound/Actor-Mixer Hierarchy/Default Work Unit.wwu", Voice_Set.Special_Path + "/Wwise/WoT_Gun_Sound/Actor-Mixer Hierarchy/Backup.tmp", true);
                        Wwise_Project_Create Wwise = new Wwise_Project_Create(Voice_Set.Special_Path + "/Wwise/WoT_Gun_Sound");
                        for (int Number = 0; Number < Other_List[Mod_Page].Count; Number++)
                        {
                            for (int Number_01 = 0; Number_01 < Other_List[Mod_Page][Number].Files.Count; Number_01++)
                            {
                                if (Path.GetExtension(Other_List[Mod_Page][Number].Files[Number_01].Full_Path) == ".wav" && !Sub_Code.Audio_IsWAV(Other_List[Mod_Page][Number].Files[Number_01].Full_Path))
                                    IsNotWAVList.Add(Other_List[Mod_Page][Number].Files[Number_01].Full_Path);
                                else
                                    Wwise.Loading_Music_Add_Wwise(Other_List[Mod_Page][Number].Files[Number_01].Full_Path, Number, null, false, 0, 3);
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
            foreach (List<Other_Type_List> Infos in Other_List)
                foreach (Other_Type_List info in Infos)
                    Music_Count += info.Files.Count;
            if (Music_Count == 0)
            {
                if (Sub_Code.r.Next(0, 10) == 5)
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
                Other_Init(0);
                Other_Init(1);
                Other_Init(2);
                Other_Init(3);
                Play_Time_T.Text = "再生時間:0～0";
                Volume_WoTB_S.Value = 75;
                BGM_Music_L.Items.Clear();
                Update_Music_Type_List();
                BGM_Type_L.SelectedIndex = -1;
                Feed_In_C.IsChecked = false;
                WMS_File.Dispose();
                Message_Feed_Out("内容をクリアしました。");
            }
        }
        //再生開始位置を指定
        private void Start_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy || BGM_Type_L.SelectedIndex == -1 || BGM_Music_L.SelectedIndex == -1)
                return;
            Other_List[Mod_Page][BGM_Type_L.SelectedIndex].Files[BGM_Music_L.SelectedIndex].Music_Time.Start_Time = Position_S.Value;
            double End_Time = Other_List[Mod_Page][BGM_Type_L.SelectedIndex].Files[BGM_Music_L.SelectedIndex].Music_Time.End_Time;
            if (End_Time != 0 && Other_List[Mod_Page][BGM_Type_L.SelectedIndex].Files[BGM_Music_L.SelectedIndex].Music_Time.Start_Time > End_Time)
            {
                Other_List[Mod_Page][BGM_Type_L.SelectedIndex].Files[BGM_Music_L.SelectedIndex].Music_Time.End_Time = 0;
                Play_Time_T.Text = "再生時間:" + (int)Other_List[Mod_Page][BGM_Type_L.SelectedIndex].Files[BGM_Music_L.SelectedIndex].Music_Time.Start_Time + "～" + (int)Position_S.Maximum;
                Message_Feed_Out("開始時間が終了時間より大きかったため、終了時間を最大にします。");
            }
            else if (End_Time != 0)
                Play_Time_T.Text = "再生時間:" + (int)Other_List[Mod_Page][BGM_Type_L.SelectedIndex].Files[BGM_Music_L.SelectedIndex].Music_Time.Start_Time + "～" + (int)End_Time;
            else
                Play_Time_T.Text = "再生時間:" + (int)Other_List[Mod_Page][BGM_Type_L.SelectedIndex].Files[BGM_Music_L.SelectedIndex].Music_Time.Start_Time + "～" + (int)Position_S.Maximum;
        }
        //再生終了位置を指定
        private void End_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy || BGM_Type_L.SelectedIndex == -1 || BGM_Music_L.SelectedIndex == -1)
                return;
            Other_List[Mod_Page][BGM_Type_L.SelectedIndex].Files[BGM_Music_L.SelectedIndex].Music_Time.End_Time = Position_S.Value;
            if (Other_List[Mod_Page][BGM_Type_L.SelectedIndex].Files[BGM_Music_L.SelectedIndex].Music_Time.End_Time < Other_List[Mod_Page][BGM_Type_L.SelectedIndex].Files[BGM_Music_L.SelectedIndex].Music_Time.Start_Time)
            {
                Other_List[Mod_Page][BGM_Type_L.SelectedIndex].Files[BGM_Music_L.SelectedIndex].Music_Time.Start_Time = 0;
                Message_Feed_Out("終了時間が開始時間より小さかったため、開始時間を0秒にします。");
            }
            Play_Time_T.Text = "再生時間:" + (int)Other_List[Mod_Page][BGM_Type_L.SelectedIndex].Files[BGM_Music_L.SelectedIndex].Music_Time.Start_Time + "～" + 
                (int)Other_List[Mod_Page][BGM_Type_L.SelectedIndex].Files[BGM_Music_L.SelectedIndex].Music_Time.End_Time;
        }
        //開始位置、終了位置を初期化
        private void Time_Clear_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy || BGM_Type_L.SelectedIndex == -1 || BGM_Music_L.SelectedIndex == -1)
                return;
            Other_List[Mod_Page][BGM_Type_L.SelectedIndex].Files[BGM_Music_L.SelectedIndex].Music_Time.Start_Time = 0;
            Other_List[Mod_Page][BGM_Type_L.SelectedIndex].Files[BGM_Music_L.SelectedIndex].Music_Time.End_Time = 0;
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
                Other_List[Mod_Page][BGM_Type_L.SelectedIndex].Files[BGM_Music_L.SelectedIndex].Music_Feed_In = true;
            else
                Other_List[Mod_Page][BGM_Type_L.SelectedIndex].Files[BGM_Music_L.SelectedIndex].Music_Feed_In = false;
        }
        void Mod_Page_Change()
        {
            Change_Other_List();
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
            Change_OverWrite_Visibility();
            Mod_Page_Change();
            Update_Music_Type_List();
        }
        private void Next_Mod_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy || IsOpenDialog)
                return;
            //後からリストを追加しても大丈夫なように
            if (Mod_Page < 3)
                Mod_Page++;
            Change_OverWrite_Visibility();
            Mod_Page_Change();
            Update_Music_Type_List();
        }
        private void Create_Help_B_Click(object sender, RoutedEventArgs e)
        {
            string Message_01 = "・2ページ目のガレージSE及び、3,4ページ目の砲撃音関連の項目を表示している場合、作成(選択のみ)のボタンは表示されません。";
            MessageBox.Show(Message_01);
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
            for (int Number = 0; Number < 4; Number++)
                Other_List.Add(new List<Other_Type_List>());
            for (int Number = 3; Number >= 0; Number--)
            {
                Mod_Page = Number;
                Other_Init(Number);
                Change_Other_List();
            }
        }
        void Change_Other_List()
        {
            if (Mod_Page == 0)
                Mod_Name_T.Text = "いろいろ";
            else if (Mod_Page == 1)
                Mod_Name_T.Text = "ガレージSE";
            else if (Mod_Page == 2)
                Mod_Name_T.Text = "砲撃音(Blitz用)";
            else if (Mod_Page == 3)
                Mod_Name_T.Text = "砲撃音(本家WoT用)";
            BGM_Type_L.Items.Clear();
            foreach (Other_Type_List Info in Other_List[Mod_Page])
            {
                ListBoxItem item = new ListBoxItem();
                item.Content = Info.Name_Text;
                if (Info.Files.Count == 0)
                    item.Foreground = Gray_Text;
                else
                    item.Foreground = Brushes.Aqua;
                BGM_Type_L.Items.Add(item);
            }
        }
        void Change_OverWrite_Visibility()
        {
            if (Mod_Page == 2)
            {
                OverWrite_C.Visibility = Visibility.Visible;
                OverWrite_T.Visibility = Visibility.Visible;
                return;
            }
            OverWrite_C.Visibility = Visibility.Hidden;
            OverWrite_T.Visibility = Visibility.Hidden;
        }
    }
}