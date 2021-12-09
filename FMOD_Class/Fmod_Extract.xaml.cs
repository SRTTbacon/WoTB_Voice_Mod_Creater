using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WK.Libraries.BetterFolderBrowserNS;

namespace WoTB_Voice_Mod_Creater.FMOD
{
    public partial class Fmod_Extract : UserControl
    {
        string File_Full_Name = "";
        bool IsClosing = false;
        bool IsMessageShowing = false;
        bool IsPlaying = false;
        bool IsPause = false;
        bool IsMouseDown = false;
        bool IsLocationMouseChange = false;
        bool IsSaveOK = false;
        int Sound_Frequency = 441;
        FMOD_API.Sound MainSound = new FMOD_API.Sound();
        FMOD_API.Sound SubSound = new FMOD_API.Sound();
        FMOD_API.Channel FModChannel = new FMOD_API.Channel();
        public Fmod_Extract()
        {
            InitializeComponent();
            Select_L.Items.Add("リスト内のすべてのファイル");
            Select_L.Items.Add("選択したファイルのみ");
            Extract_L.Items.Add(".aac");
            Extract_L.Items.Add(".flac");
            Extract_L.Items.Add(".mp3");
            Extract_L.Items.Add(".ogg");
            Extract_L.Items.Add(".wav");
            Extract_L.Items.Add(".webm");
            Extract_L.Items.Add(".wma");
            //Sliderにクリック判定がないため強制的に判定を付ける
            Location_S.AddHandler(MouseDownEvent, new MouseButtonEventHandler(Location_MouseDown), true);
            Location_S.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Location_MouseUp), true);
            Volume_S.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Volume_MouseUp), true);
        }
        //画面を表示
        public async void Window_Show()
        {
            Opacity = 0;
            Visibility = System.Windows.Visibility.Visible;
            Pitch_S.Value = 100;
            //設定を反映
            if (File.Exists(Voice_Set.Special_Path + "/Configs/FSB_Extract.conf"))
            {
                try
                {
                    StreamReader str = Sub_Code.File_Decrypt_To_Stream(Voice_Set.Special_Path + "/Configs/FSB_Extract.conf", "FSB_Extract_Configs_Save");
                    Extract_L.SelectedIndex = int.Parse(str.ReadLine());
                    Select_L.SelectedIndex = int.Parse(str.ReadLine());
                    Volume_S.Value = double.Parse(str.ReadLine());
                    str.Close();
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show("設定を読み込めませんでした。。\nエラー回避のため設定は削除されます。");
                    File.Delete(Voice_Set.Special_Path + "/Configs/FSB_Extract.conf");
                    Select_L.SelectedIndex = 0;
                    Extract_L.SelectedIndex = 2;
                    Volume_S.Value = 50;
                    Sub_Code.Error_Log_Write(e.Message);
                }
            }
            else
            {
                Select_L.SelectedIndex = 0;
                Extract_L.SelectedIndex = 2;
                Volume_S.Value = 50;
            }
            IsSaveOK = true;
            Position_Change();
            while (Opacity < 1 && !IsClosing)
            {
                Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
        }
        async void Position_Change()
        {
            //サウンドの位置をSliderに反映
            while (Visibility == System.Windows.Visibility.Visible)
            {
                if (File_Full_Name != "")
                {
                    bool IsPaused = false;
                    FModChannel.getPaused(ref IsPaused);
                    FModChannel.isPlaying(ref IsPlaying);
                    if (!IsMouseDown)
                    {
                        if (!IsPaused && !IsPlaying)
                        {
                            Sound_Start();
                        }
                        if (!IsPaused && !IsLocationMouseChange)
                        {
                            Set_Position_TextBlock(true);
                        }
                    }
                }
                await Task.Delay(1000 / 30);
            }
        }
        //メッセージをフェードアウト
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
            while (Message_T.Opacity > 0 && IsMessageShowing)
            {
                Number++;
                if (Number >= 120)
                {
                    Message_T.Opacity -= 0.025;
                }
                await Task.Delay(1000 / 60);
            }
            Message_T.Text = "";
            Message_T.Opacity = 1;
            IsMessageShowing = false;
        }
        //選択されている音声を再生
        void Sound_Start()
        {
            if (File_Full_Name == "" || FSB_Name_L.SelectedIndex == -1)
            {
                return;
            }
            FModChannel.setPaused(true);
            FModChannel = new FMOD_API.Channel();
            SubSound.release();
            MainSound.release();
            SubSound = new FMOD_API.Sound();
            MainSound = new FMOD_API.Sound();
            Fmod_System.FModSystem.createSound(File_Full_Name, FMOD_API.MODE.CREATESTREAM, ref MainSound);
            MainSound.getSubSound(FSB_Name_L.SelectedIndex, ref SubSound);
            Fmod_System.FModSystem.playSound(FMOD_API.CHANNELINDEX.FREE, SubSound, true, ref FModChannel);
            float Frequency_Now = 44100f;
            FModChannel.getFrequency(ref Frequency_Now);
            Sound_Frequency = (int)Frequency_Now / 100;
            FModChannel.setVolume((float)(Volume_S.Value / (double)100));
            FModChannel.setFrequency((float)(Pitch_S.Value * Sound_Frequency));
        }
        //サウンドの現在の時間をテキストボックスに反映
        void Set_Position_TextBlock(bool IsSetSoundPosition)
        {
            if (IsSetSoundPosition && !IsLocationMouseChange)
            {
                uint Position_Now = 0;
                FModChannel.getPosition(ref Position_Now, FMOD_API.TIMEUNIT.MS);
                Location_S.Value = Position_Now;
            }
            TimeSpan Time = TimeSpan.FromMilliseconds(Location_S.Value);
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
        //FSBファイルを選択
        private async void FSB_Select_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (IsClosing)
            {
                return;
            }
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog()
            {
                Title = "FSBファイルを選択してください。",
                Filter = "FSBファイル(*.fsb;*.fsb.dvpl)|*.fsb;*.fsb.dvpl",
                Multiselect = false
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    bool IsDVPL = false;
                    int Number = 0;
                    if (Path.GetExtension(ofd.FileName) == ".dvpl")
                    {
                        IsDVPL = true;
                        if (!File.Exists(Voice_Set.Special_Path + "/FSB_Select_Temp_01.fsb"))
                        {
                            DVPL.DVPL_UnPack(ofd.FileName, Voice_Set.Special_Path + "/FSB_Select_Temp_01.fsb", false);
                            Number = 1;
                        }
                        else if (!File.Exists(Voice_Set.Special_Path + "/FSB_Select_Temp_02.fsb"))
                        {
                            DVPL.DVPL_UnPack(ofd.FileName, Voice_Set.Special_Path + "/FSB_Select_Temp_02.fsb", false);
                            Number = 2;
                        }
                        else if (!File.Exists(Voice_Set.Special_Path + "/FSB_Select_Temp_03.fsb"))
                        {
                            DVPL.DVPL_UnPack(ofd.FileName, Voice_Set.Special_Path + "/FSB_Select_Temp_03.fsb", false);
                            Number = 3;
                        }
                    }
                    FModChannel.setPaused(true);
                    FModChannel = new FMOD_API.Channel();
                    SubSound.release();
                    MainSound.release();
                    SubSound = new FMOD_API.Sound();
                    MainSound = new FMOD_API.Sound();
                    Location_T.Text = "00:00";
                    Message_T.Text = "FSBファイルを読み込んでいます...";
                    await Task.Delay(50);
                    FSB_Select_T.Text = Path.GetFileName(ofd.FileName);
                    List<string> Name_List;
                    if (IsDVPL)
                    {
                        File_Full_Name = Voice_Set.Special_Path + "/FSB_Select_Temp_0" + Number + ".fsb";
                        Name_List = Fmod_Class.FSB_GetNames(Voice_Set.Special_Path + "/FSB_Select_Temp_0" + Number + ".fsb");
                    }
                    else
                    {
                        File_Full_Name = ofd.FileName;
                        Name_List = Fmod_Class.FSB_GetNames(ofd.FileName);
                    }
                    FSB_Number_T.Text = "ファイル数:" + Name_List.Count + " | 選択:なし";
                    FSB_Name_L.Items.Clear();
                    foreach (string Name_Now in Name_List)
                    {
                        FSB_Name_L.Items.Add(Name_Now);
                    }
                    if (Name_List.Count == 0)
                    {
                        Message_Feed_Out("内容を取得できませんでした。");
                    }
                    else
                    {
                        Message_Feed_Out("読み込みが完了しました。");
                    }
                    if (Number == 1)
                    {
                        Sub_Code.File_Delete_V2(Voice_Set.Special_Path + "/FSB_Select_Temp_02.fsb");
                        Sub_Code.File_Delete_V2(Voice_Set.Special_Path + "/FSB_Select_Temp_03.fsb");
                    }
                    else if (Number == 2)
                    {
                        Sub_Code.File_Delete_V2(Voice_Set.Special_Path + "/FSB_Select_Temp_01.fsb");
                        Sub_Code.File_Delete_V2(Voice_Set.Special_Path + "/FSB_Select_Temp_03.fsb");
                    }
                    else if (Number == 3)
                    {
                        Sub_Code.File_Delete_V2(Voice_Set.Special_Path + "/FSB_Select_Temp_01.fsb");
                        Sub_Code.File_Delete_V2(Voice_Set.Special_Path + "/FSB_Select_Temp_02.fsb");
                    }
                }
                catch (Exception e1)
                {
                    Message_Feed_Out("エラー:FSBファイルが壊れている可能性があります。");
                    Sub_Code.Error_Log_Write(e1.Message);
                }
            }
        }
        //ファイルリストの選択が変更された場合情報を初期化
        private void FSB_Name_L_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsClosing || FSB_Name_L.SelectedIndex == -1)
            {
                return;
            }
            Sound_Start();
            uint Sound_Length = 0;
            SubSound.getLength(ref Sound_Length, FMOD_API.TIMEUNIT.MS);
            Location_S.Value = 0;
            Location_S.Maximum = Sound_Length;
            FSB_Number_T.Text = FSB_Number_T.Text.Substring(0, FSB_Number_T.Text.IndexOf('|') + 5) + (FSB_Name_L.SelectedIndex + 1) + "個目";
            Location_T.Text = "00:00";
        }
        //再生
        private void Play_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (IsClosing)
            {
                return;
            }
            FModChannel.setPaused(false);
        }
        //一時停止
        private void Pause_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            FModChannel.setPaused(true);
        }
        //音量の変更
        private void Volume_S_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            Volume_T.Text = "音量:" + (int)Volume_S.Value;
            FModChannel.setVolume((float)(Volume_S.Value / (double)100));
        }
        //速度の変更
        private void Pitch_S_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            Pitch_T.Text = "速度:" + (int)Pitch_S.Value;
            FModChannel.setFrequency((float)(Pitch_S.Value * Sound_Frequency));
        }
        //Sliderのクリック判定部分が小さいためBorderがクリックされても再生時間を変更
        private void Location_Board_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IsMouseDown = true;
            FModChannel.getPaused(ref IsPause);
            FModChannel.setPaused(true);
            System.Drawing.Point p = new System.Drawing.Point();
            int w = System.Windows.Forms.Screen.GetBounds(p).Width;
            double Width_Display_From_1920 = (double)w / 1920;
            int Location_Mouse_X_Display = System.Math.Abs((int)Location_S.PointToScreen(new System.Windows.Point()).X - System.Windows.Forms.Cursor.Position.X) - 10;
            double Percent = Location_Mouse_X_Display / (390 * Width_Display_From_1920);
            Location_S.Value = Location_S.Maximum * Percent;
            FModChannel.setPosition((uint)Location_S.Value, FMOD_API.TIMEUNIT.MS);
            if (!IsPause)
            {
                FModChannel.setPaused(false);
            }
            Set_Position_TextBlock(false);
            IsMouseDown = false;
        }
        //マウスがある位置まで再生時間を移動
        void Location_MouseDown(object sender, MouseEventArgs e)
        {
            IsMouseDown = true;
            FModChannel.getPaused(ref IsPause);
            FModChannel.setPaused(true);
            //計算大変だった...
            System.Drawing.Point p = new System.Drawing.Point();
            int w = System.Windows.Forms.Screen.GetBounds(p).Width;
            double Width_Display_From_1920 = (double)w / 1920;
            int Location_Mouse_X_Display = System.Math.Abs((int)Location_S.PointToScreen(new System.Windows.Point()).X - System.Windows.Forms.Cursor.Position.X) - 10;
            double Percent = Location_Mouse_X_Display / (390 * Width_Display_From_1920);
            Location_S.Value = Location_S.Maximum * Percent;
        }
        //Sliderの位置をサウンドに反映
        void Location_MouseUp(object sender, MouseEventArgs e)
        {
            FModChannel.setPosition((uint)Location_S.Value, FMOD_API.TIMEUNIT.MS);
            IsLocationMouseChange = true;
            IsMouseDown = false;
            if (!IsPause)
            {
                bool IsPauseNow = false;
                FModChannel.getPaused(ref IsPauseNow);
                if (IsPauseNow)
                {
                    FModChannel.setPaused(false);
                }
            }
            Set_Position_TextBlock(true);
            IsLocationMouseChange = false;
        }
        //上と同じ
        private void Location_S_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsMouseDown)
            {
                Set_Position_TextBlock(false);
            }
        }
        //-5秒
        private void Minus_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (IsPlaying)
            {
                uint Position_Now = 0;
                FModChannel.getPosition(ref Position_Now, FMOD_API.TIMEUNIT.MS);
                if (Position_Now > 5000)
                {
                    FModChannel.setPosition(Position_Now - 5000, FMOD_API.TIMEUNIT.MS);
                }
                else
                {
                    FModChannel.setPosition(0, FMOD_API.TIMEUNIT.MS);
                }
                if (IsPause)
                {
                    Set_Position_TextBlock(true);
                }
            }
        }
        //+5秒
        private void Plus_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (IsPlaying)
            {
                uint Position_Now = 0;
                FModChannel.getPosition(ref Position_Now, FMOD_API.TIMEUNIT.MS);
                if (Position_Now < Location_S.Maximum - 5000)
                {
                    FModChannel.setPosition(Position_Now + 5000, FMOD_API.TIMEUNIT.MS);
                }
                else
                {
                    FModChannel.setPosition((uint)Location_S.Maximum, FMOD_API.TIMEUNIT.MS);
                }
                if (IsPause)
                {
                    Set_Position_TextBlock(true);
                }
            }
        }
        //設定を保存(音量,抽出形式,適応元)
        void Configs_Save()
        {
            if (!IsSaveOK)
            {
                return;
            }
            try
            {
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Configs/FSB_Extract.tmp");
                stw.WriteLine(Extract_L.SelectedIndex);
                stw.WriteLine(Select_L.SelectedIndex);
                stw.WriteLine(Volume_S.Value);
                stw.Close();
                using (var eifs = new FileStream(Voice_Set.Special_Path + "/Configs/FSB_Extract.tmp", FileMode.Open, FileAccess.Read))
                {
                    using (var eofs = new FileStream(Voice_Set.Special_Path + "/Configs/FSB_Extract.conf", FileMode.Create, FileAccess.Write))
                    {
                        FileEncode.FileEncryptor.Encrypt(eifs, eofs, "FSB_Extract_Configs_Save");
                    }
                }
                File.Delete(Voice_Set.Special_Path + "/Configs/FSB_Extract.tmp");
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        //抽出開始
        private async void Extract_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (IsClosing)
            {
                return;
            }
            if (Select_L.SelectedIndex == -1)
            {
                Message_Feed_Out("適応元が指定されていません。");
                return;
            }
            if (Extract_L.SelectedIndex == -1)
            {
                Message_Feed_Out("出力形式が指定されていません。");
                return;
            }
            if (FSB_Name_L.Items.Count == 0)
            {
                Message_Feed_Out("FSBファイルが選択されていません。");
                return;
            }
            if (Select_L.SelectedIndex == 1 && FSB_Name_L.SelectedIndex == -1)
            {
                Message_Feed_Out("リスト内のファイルが選択されていません。");
                return;
            }
            if (Extract_L.SelectedIndex == 0 || Extract_L.SelectedIndex == 5)
            {
                string Message = ".aacと.webm形式はこのソフトでは再エンコード(fsbに戻す作業)ができなくなります。よろしいですか？";
                MessageBoxResult result = MessageBox.Show(Message, "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }
            //抽出元が選択したファイルのみの場合
            if (Select_L.SelectedIndex == 1)
            {
                System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
                sfd.Title = "保存先を指定してください。";
                sfd.Filter = "音声ファイル(*" + Extract_L.SelectedItem + ")|*" + Extract_L.SelectedItem;
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Message_T.Text = "FSBファイルから抽出しています...";
                    await Task.Delay(50);
                    string File_Name = sfd.FileName.Replace(Path.GetExtension(sfd.FileName), "");
                    if (Fmod_File_Extract_V1.FSB_Extract_To_File(File_Full_Name, FSB_Name_L.SelectedIndex, File_Name + ".wav"))
                    {
                        if (Extract_L.SelectedIndex != 4)
                        {
                            Message_T.Text = Extract_L.SelectedItem.ToString().Replace(".", "").ToUpper() + "にエンコードしています...";
                            await Task.Delay(5);
                            Sub_Code.Audio_Encode_To_Other(File_Name + ".wav", File_Name + Extract_L.SelectedItem, Extract_L.SelectedItem.ToString(), true);
                        }
                    }
                    else
                    {
                        Message_Feed_Out("抽出できませんでした。");
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            //抽出元がすべてのファイルの場合
            else
            {
                BetterFolderBrowser ofd = new BetterFolderBrowser()
                {
                    Title = "抽出先のフォルダを選択してください。",
                    RootFolder = Sub_Code.Get_OpenDirectory_Path(),
                    Multiselect = false
                };
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Sub_Code.Set_Directory_Path(ofd.SelectedFolder);
                    try
                    {
                        Message_T.Text = "FSBファイルから抽出しています...";
                        await Task.Delay(50);
                        List<string> Name_Delete = Fmod_Class.FSB_GetNames(File_Full_Name);
                        foreach (string Name_Now in Name_Delete)
                        {
                            if (Sub_Code.File_Exists(ofd.SelectedFolder + "/" + Name_Now))
                            {
                                Sub_Code.File_Delete(ofd.SelectedFolder + "/" + Name_Now);
                            }
                        }
                        //.wavに変換されたファイルパスすべてを取得し指定した形式にエンコード(形式がwavだった場合を除く)
                        if (Fmod_File_Extract_V2.FSB_Extract_To_Directory(File_Full_Name, ofd.SelectedFolder))
                        {
                            //FSBのファイル構成を保存&エンコード
                            StreamWriter stw = File.CreateText(ofd.SelectedFolder + "/" + Path.GetFileNameWithoutExtension(File_Full_Name) + ".tmp");
                            string[] Output_Files = Directory.GetFiles(ofd.SelectedFolder, "*.wav", SearchOption.TopDirectoryOnly);
                            foreach (string File_Now in Output_Files)
                            {
                                if (Extract_L.SelectedIndex != 4)
                                {
                                    Message_T.Text = Path.GetFileNameWithoutExtension(File_Now) + "をエンコードしています...";
                                    await Task.Delay(5);
                                    Sub_Code.Audio_Encode_To_Other(File_Now, Path.GetDirectoryName(File_Now) + "/" + Path.GetFileNameWithoutExtension(File_Now) + Extract_L.SelectedItem, Extract_L.SelectedItem.ToString(), true);
                                    stw.WriteLine(Path.GetDirectoryName(File_Now) + "/" + Path.GetFileNameWithoutExtension(File_Now) + Extract_L.SelectedItem);
                                }
                                else
                                {
                                    Message_T.Text = Path.GetFileNameWithoutExtension(File_Now) + "をエンコードしています...";
                                    await Task.Delay(5);
                                    Sub_Code.Audio_Encode_To_Other(File_Now, Path.GetDirectoryName(File_Now) + "/Temp" + Path.GetFileNameWithoutExtension(File_Now) + Extract_L.SelectedItem, Extract_L.SelectedItem.ToString(), true);
                                    Sub_Code.File_Move(Path.GetDirectoryName(File_Now) + "/Temp" + Path.GetFileNameWithoutExtension(File_Now) + Extract_L.SelectedItem, File_Now, true);
                                    stw.WriteLine(File_Now);
                                }
                            }
                            stw.Close();
                            if (Extract_L.SelectedIndex == 0 || Extract_L.SelectedIndex == 5)
                            {
                                File.Delete(ofd.SelectedFolder + "/" + Path.GetFileNameWithoutExtension(File_Full_Name) + ".tmp");
                            }
                            else
                            {
                                string FromFile = ofd.SelectedFolder + "/" + Path.GetFileNameWithoutExtension(File_Full_Name) + ".tmp";
                                string ToFile = ofd.SelectedFolder + "/" + Path.GetFileNameWithoutExtension(File_Full_Name) + ".wfs";
                                Sub_Code.File_Encrypt(FromFile, ToFile, "WoTB_FSB_Encode_Save", true);
                            }
                        }
                        else
                        {
                            Message_Feed_Out("抽出できませんでした。");
                            return;
                        }
                    }
                    catch (Exception e1)
                    {
                        Message_Feed_Out("正常に完了しませんでした。");
                        Sub_Code.Error_Log_Write(e1.Message);
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            Message_Feed_Out("抽出しました。");
        }
        //戻る
        private async void Back_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!IsClosing)
            {
                IsClosing = true;
                float Down = (float)((Volume_S.Value / (double)100) / (double)30);
                while (Opacity > 0)
                {
                    Opacity -= Sub_Code.Window_Feed_Time;
                    float Volume_Now = 1f;
                    FModChannel.getVolume(ref Volume_Now);
                    FModChannel.setVolume(Volume_Now - Down);
                    await Task.Delay(1000 / 60);
                }
                FModChannel.setPaused(true);
                FModChannel = new FMOD_API.Channel();
                MainSound.release();
                SubSound.release();
                MainSound = new FMOD_API.Sound();
                SubSound = new FMOD_API.Sound();
                Location_T.Text = "00:00";
                Location_S.Value = 0;
                Location_S.Maximum = 0;
                FSB_Name_L.SelectedIndex = -1;
                FSB_Number_T.Text = "ファイル数:" + FSB_Name_L.Items.Count + " | 選択:なし";
                Visibility = System.Windows.Visibility.Hidden;
                IsClosing = false;
            }
        }
        void Volume_MouseUp(object sender, MouseEventArgs e)
        {
            Configs_Save();
        }
        private void Select_L_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Configs_Save();
        }
        private void Extract_L_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Extract_L.SelectedIndex == 4)
            {
                Attention_T.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                Attention_T.Visibility = System.Windows.Visibility.Visible;
            }
            Configs_Save();
        }
        //速度のSliderを右クリックすると値を初期化
        private void Pitch_S_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Pitch_S.Value = 100;
        }
    }
}