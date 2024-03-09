using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using WK.Libraries.BetterFolderBrowserNS;
using WoTB_Voice_Mod_Creater.Class;
using WoTB_Voice_Mod_Creater.FMOD_Class;

namespace WoTB_Voice_Mod_Creater.Wwise_Class
{
    public partial class Change_To_Wwise : System.Windows.Controls.UserControl
    {
        string Voice_FSB_File = "";
        string BGM_FSB_File = "";
        bool IsClosing = false;
        bool IsMessageShowing = false;
        bool IsOpenDialog = false;
        List<string> BGM_Select_Only = new List<string>();
        List<string> BGM_Add_Only = new List<string>();
        public Change_To_Wwise()
        {
            InitializeComponent();
            FMod_List_Clear();
        }
        //リストを初期化
        void FMod_List_Clear()
        {
            FSB_Details_L.Items.Clear();
            FSB_Details_L.Items.Add("音声ファイル:未指定");
            FSB_Details_L.Items.Add("BGMファイル:未指定");
            FSB_Details_L.Items.Add("音声数:なし");
            FSB_Details_L.Items.Add("BGM数:なし");
            FSB_Details_L.Items.Add("出力先のフォルダ名:未指定");
            FSB_Details_L.Items.Add("SE:有効(変更できません)");
        }
        //画面を表示
        public async void Window_Show()
        {
            Opacity = 0;
            Visibility = Visibility.Visible;
            while (Opacity < 1 && !IsClosing)
            {
                Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
        }
        //戻る
        private async void Back_B_Click(object sender, RoutedEventArgs e)
        {
            if (!IsClosing)
            {
                IsClosing = true;
                while (Opacity > 0)
                {
                    Opacity -= Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
                IsClosing = false;
                Visibility = Visibility.Hidden;
            }
        }
        //テキストの表示
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
        //FModの音声ファイルを選択
        private async void Voice_Select_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Title = "音声ファイルを選択してください。",
                Filter = "音声ファイル(*.fsb)|*.fsb",
                Multiselect = false
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                bool IsVoiceExist = false;
                Message_T.Opacity = 1.0;
                Message_T.Text = "ファイル構造を取得しています...";
                await Task.Delay(50);
                List<string> Voices = Fmod_Class.FSB_GetNames(ofd.FileName);
                foreach (string File_Now in Voices)
                {
                    string File_Now_01 = File_Now.Replace(" ", "");
                    if (File_Now_01.Contains("battle_01") || File_Now_01.Contains("battle_02") || File_Now_01.Contains("battle_03") || File_Now_01.Contains("start_battle_01") || File_Now_01.Contains("start_battle("))
                    {
                        IsVoiceExist = true;
                        break;
                    }
                }
                if (!IsVoiceExist)
                {
                    Message_Feed_Out("指定したファイルは対応していません。詳しくは注意事項を参照してください。");
                    return;
                }
                Voices.Clear();
                Voice_FSB_File = ofd.FileName;
                FSB_Details_L.Items[0] = "音声ファイル:" + Path.GetFileName(ofd.FileName);
                FSB_Details_L.Items[2] = "音声数:" + Fmod_Class.FSB_GetLength(ofd.FileName) + "個";

                Message_Feed_Out("FSBファイルを追加しました。");
            }
        }
        //FModのBGMファイルを選択
        private void BGM_Select_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Title = "BGMファイルを選択してください。",
                Filter = "BGMファイル(Music.fsb)|Music.fsb",
                Multiselect = false
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (Fmod_Class.FSB_GetLength(ofd.FileName) >= 20)
                {
                    Message_Feed_Out("指定したファイルはファイル数が20個以上あるため追加することができません。");
                    return;
                }
                BGM_Select_Only = Fmod_Class.FSB_GetNames(ofd.FileName);
                bool IsBGMExsist = false;
                foreach (string File_Now in BGM_Select_Only)
                {
                    if (File_Now == "Music_01")
                    {
                        IsBGMExsist = true;
                        break;
                    }
                }
                if (!IsBGMExsist)
                {
                    Message_Feed_Out("指定したファイルにBGMが含まれていませんでした。");
                    return;
                }
                BGM_FSB_File = ofd.FileName;
                FSB_Details_L.Items[1] = "BGMファイル:" + Path.GetFileName(ofd.FileName);
                foreach (string BGM_Name_Now in BGM_Select_Only)
                {
                    BGM_Add_List.Items.Add(BGM_Name_Now);
                }
                FSB_Details_L.Items[3] = "BGM数:" + BGM_Add_List.Items.Count + "個";
            }
        }
        //追加されたBGMをリストから削除
        private void BGM_Delete_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            if (BGM_Add_List.SelectedIndex != -1)
            {
                int Selected_Index_Now = BGM_Add_List.SelectedIndex;
                for (int File_Now = 0; File_Now < BGM_Select_Only.Count; File_Now++)
                {
                    if (Path.GetFileName(BGM_Select_Only[File_Now]) == BGM_Add_List.Items[Selected_Index_Now].ToString())
                    {
                        BGM_Select_Only.RemoveAt(File_Now);
                        break;
                    }
                }
                for (int File_Now = 0; File_Now < BGM_Add_Only.Count; File_Now++)
                {
                    if (Path.GetFileName(BGM_Add_Only[File_Now]) == BGM_Add_List.Items[Selected_Index_Now].ToString())
                    {
                        BGM_Add_Only.RemoveAt(File_Now);
                        break;
                    }
                }
                BGM_Add_List.Items.RemoveAt(BGM_Add_List.SelectedIndex);
                if (BGM_Add_List.Items.Count - 1 >= Selected_Index_Now)
                {
                    BGM_Add_List.SelectedIndex = Selected_Index_Now;
                }
                FSB_Details_L.Items[3] = "BGM数:" + BGM_Add_List.Items.Count + "個";
            }
        }
        //戦闘BGMを追加
        private void BGM_Add_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
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
                    BGM_Add_Only.Add(File_Now);
                }
                FSB_Details_L.Items[3] = "BGM数:" + BGM_Add_List.Items.Count + "個";
            }
        }
        //1つ1つ削除するのがめんどくさいとき用に一気に削除
        private void BGM_Clear_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            MessageBoxResult result = System.Windows.MessageBox.Show("BGMをクリアしますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                BGM_Add_List.Items.Clear();
                BGM_Add_Only.Clear();
                BGM_Select_Only.Clear();
                FSB_Details_L.Items[1] = "BGMファイル:未指定";
                FSB_Details_L.Items[3] = "BGM数:なし";
                BGM_FSB_File = "";
                Message_Feed_Out("内容をクリアしました。");
            }
        }
        //音声、BGMともに初期化
        private void Clear_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            MessageBoxResult result = System.Windows.MessageBox.Show("内容をクリアしますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                FMod_List_Clear();
                BGM_Add_Only.Clear();
                BGM_FSB_File = "";
                BGM_Select_Only.Clear();
                BGM_Add_List.Items.Clear();
                Voice_FSB_File = "";
            }
        }
        //ヘルプ
        private void Help_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            string Message_01 = "この画面では、FModで作成された.fsbファイルをMod Creator用のセーブファイルに変換します。\n";
            string Message_02 = "ただし、FModで作成された音声やBGMは、このソフトで作成されたものか、ファイル名を変更していない他の音声Modに限ります。\n";
            string Message_03 = "例1:戦闘開始->start_battle_01 | 貫通->armor_pierced_by_player_01\n";
            string Message_04 = "例2:戦闘開始->battle_01 | 貫通->kantuu_01など\n";
            string Message_05 = "V1.5.6以前のバージョンまではそのまま.bnk形式に変換していましたが、不具合が多数見つかることから仕様を変更しました。";
            System.Windows.MessageBox.Show(Message_01 + Message_02 + Message_03 + Message_04 + Message_05);
        }
        //変換
        //fsbからwavファイルを抽出し、ファイル名を変更、adpcmの場合ファイルが破損しているため復元してから.bnkを作成
        private async void Chnage_To_Wwise_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || Opacity < 1 || IsOpenDialog)
                return;
            if (Voice_FSB_File == "")
            {
                Message_Feed_Out("FSBファイルが選択されていません。");
                return;
            }

            if (string.IsNullOrWhiteSpace(ProjectName_T.Text))
            {
                Message_Feed_Out("プロジェクト名を入力してください。");
                return;
            }

            if (!Sub_Code.IsSafeFileName(ProjectName_T.Text))
            {
                Message_Feed_Out("プロジェクト名に使用不可な文字が含まれています。");
                return;
            }
            string toDir = Voice_Set.Local_Path + "\\Projects\\" + ProjectName_T.Text;
            if (Directory.Exists(toDir))
            {
                Message_Feed_Out("既に同名のプロジェクトが存在します。");
                return;
            }

            MessageBoxResult result = System.Windows.MessageBox.Show(".wvs形式に変換しますか?\n変換後は/Projects/" + ProjectName_T.Text + "/" + ProjectName_T.Text + ".wvsとして保存されます。", "確認",
                MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes);

            if (result == MessageBoxResult.No)
            {
                return;
            }

            try
            {
                IsClosing = true;
                IsOpenDialog = true;
                Directory.CreateDirectory(toDir);

                Message_T.Text = "FSBファイルから音声を抽出しています...";
                await Task.Delay(50);
                Fmod_File_Extract_V2.FSB_Extract_To_Directory(Voice_FSB_File, Voice_Set.Special_Path + "/Wwise/FSB_Extract_Voices_TMP");
                if (BGM_FSB_File != "")
                {
                    Message_T.Text = "FSBファイルからBGMを抽出しています...";
                    await Task.Delay(50);
                    Fmod_File_Extract_V2.FSB_Extract_To_Directory(BGM_FSB_File, Voice_Set.Special_Path + "/Wwise/FSB_Extract_Voices_TMP");
                }
                Message_T.Text = ".wavファイルをエンコードしています...";
                await Task.Delay(50);
                await Multithread.Convert_To_Wav(Voice_Set.Special_Path + "/Wwise/FSB_Extract_Voices_TMP", toDir + "\\AllVoices", true, true, false, false);
                Directory.Delete(Voice_Set.Special_Path + "/Wwise/FSB_Extract_Voices_TMP", true);
                if (BGM_Add_Only.Count > 0)
                {
                    Message_T.Text = "追加されたBGMファイルをwavに変換しています...";
                    await Task.Delay(50);
                    foreach (string File_Now in BGM_Add_Only)
                    {
                        if (!Sub_Code.Audio_IsWAV(File_Now))
                            Sub_Code.Audio_Encode_To_Other(File_Now, Sub_Code.File_Rename_Get_Name(toDir + "\\AllVoices/battle_bgm") + ".wav", ".wav", false);
                        else
                            File.Copy(File_Now, Sub_Code.File_Rename_Get_Name(toDir + "\\AllVoices/battle_bgm") + ".wav", true);
                    }
                }

                Voice_Set.Voice_BGM_Name_Change_From_FSB(toDir + "\\AllVoices");

                string[] Reload_Files = Directory.GetFiles(toDir + "\\AllVoices", "reload_*", SearchOption.TopDirectoryOnly);
                foreach (string Reload_Now in Reload_Files)
                {
                    FileInfo fi_reload = new FileInfo(Reload_Now);
                    if (fi_reload.Length == 290340 || fi_reload.Length == 335796 || fi_reload.Length == 336036 || fi_reload.Length == 445836 || fi_reload.Length == 497268 || fi_reload.Length == 541980)
                        fi_reload.Delete();
                }

                await CreateSaveData(toDir);

                Flash.Flash_Start();
                Message_Feed_Out("完了しました。\n出力先:/Projects/" + ProjectName_T.Text + "/");
            }
            catch (Exception e1)
            {
                Sub_Code.Error_Log_Write(e1.Message);
                Message_Feed_Out("エラーが発生しました。詳しくはLog.txtを参照してください。");
            }
            IsClosing = false;
            IsOpenDialog = false;
        }
        private void Install_Help_B_Click(object sender, RoutedEventArgs e)
        {
            string Message_01 = "・音声ファイル(voiceover_crew.bnk)は、'/Data/WwiseSound/ja'に配置されるため、国籍音声を有効化している場合は、日本の車両でしか再生されません。\n";
            string Message_02 = "・音声を再生するには、ゲーム内の言語を日本語にする必要があります。音声ファイル以外のファイルは言語を問わず再生されるはずです。\n";
            string Message_03 = "・WoTB内のファイルは上書きされるため、事前にバックアップを取っておくと良いかもしれません。";
            System.Windows.MessageBox.Show(Message_01 + Message_02 + Message_03);
        }
        private async Task CreateSaveData(string dir)
        {
            List<List<Voice_Event_Setting>> Sound_Setting = new List<List<Voice_Event_Setting>>();

            Sound_Setting.Clear();
            for (int Number = 0; Number < 3; Number++)
                Sound_Setting.Add(new List<Voice_Event_Setting>());
            for (int Number = 0; Number < 34; Number++)
            {
                Sound_Setting[0].Add(new Voice_Event_Setting());
            }
            for (int Number = 0; Number < 17; Number++)
            {
                if (Number == 15)
                    Sound_Setting[1][Sound_Setting[1].Count - 1].Volume = -11;
                Sound_Setting[1].Add(new Voice_Event_Setting());
            }
            for (int Number = 0; Number < 6; Number++)
            {
                Sound_Setting[2].Add(new Voice_Event_Setting());
            }

            string[] Voice_Files_01 = Directory.GetFiles(dir + "\\AllVoices", "*.wav", SearchOption.TopDirectoryOnly);
            foreach (string Voice_Now in Voice_Files_01)
            {
                string Name_Only;
                if (Path.GetFileNameWithoutExtension(Voice_Now).Contains("reload"))
                    Name_Only = Voice_Mod_Create.Get_Voice_Type_V2(Voice_Now);
                else
                    Name_Only = Voice_Mod_Create.Get_Voice_Type_V1(Voice_Now);
                if (Name_Only == "mikata" || Name_Only == "ally_killed_by_player")
                    Sound_Setting[0][0].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "danyaku" || Name_Only == "ammo_bay_damaged")
                    Sound_Setting[0][1].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "hikantuu" || Name_Only == "armor_not_pierced_by_player")
                    Sound_Setting[0][2].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "kantuu" || Name_Only == "armor_pierced_by_player")
                    Sound_Setting[0][3].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "tokusyu" || Name_Only == "armor_pierced_crit_by_player")
                    Sound_Setting[0][4].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "tyoudan" || Name_Only == "armor_ricochet_by_player")
                    Sound_Setting[0][5].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "syatyou" || Name_Only == "commander_killed")
                    Sound_Setting[0][6].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "souzyuusyu" || Name_Only == "driver_killed")
                    Sound_Setting[0][7].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "tekikasai" || Name_Only == "enemy_fire_started_by_player")
                    Sound_Setting[0][8].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "gekiha" || Name_Only == "enemy_killed_by_player")
                    Sound_Setting[0][9].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "enjinhason" || Name_Only == "engine_damaged")
                    Sound_Setting[0][10].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "enjintaiha" || Name_Only == "engine_destroyed")
                    Sound_Setting[0][11].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "enjinhukkyuu" || Name_Only == "engine_functional")
                    Sound_Setting[0][12].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "kasai" || Name_Only == "fire_started")
                    Sound_Setting[0][13].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "syouka" || Name_Only == "fire_stopped")
                    Sound_Setting[0][14].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "nenryou" || Name_Only == "fuel_tank_damaged")
                    Sound_Setting[0][15].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "housinhason" || Name_Only == "gun_damaged")
                    Sound_Setting[0][16].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "housintaiha" || Name_Only == "gun_destroyed")
                    Sound_Setting[0][17].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "housinhukkyuu" || Name_Only == "gun_functional")
                    Sound_Setting[0][18].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "housyu" || Name_Only == "gunner_killed")
                    Sound_Setting[0][19].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "soutensyu" || Name_Only == "loader_killed")
                    Sound_Setting[0][20].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "musen" || Name_Only == "radio_damaged")
                    Sound_Setting[0][21].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "musensyu" || Name_Only == "radioman_killed")
                    Sound_Setting[0][22].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "battle" || Name_Only == "start_battle")
                    Sound_Setting[0][23].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "kansokuhason" || Name_Only == "surveying_devices_damaged")
                    Sound_Setting[0][24].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "kansokutaiha" || Name_Only == "surveying_devices_destroyed")
                    Sound_Setting[0][25].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "kansokuhukkyuu" || Name_Only == "surveying_devices_functional")
                    Sound_Setting[0][26].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "ritaihason" || Name_Only == "track_damaged")
                    Sound_Setting[0][27].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "ritaitaiha" || Name_Only == "track_destroyed")
                    Sound_Setting[0][28].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "ritaihukkyuu" || Name_Only == "track_functional")
                    Sound_Setting[0][29].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "houtouhason" || Name_Only == "turret_rotator_damaged")
                    Sound_Setting[0][30].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "houtoutaiha" || Name_Only == "turret_rotator_destroyed")
                    Sound_Setting[0][31].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "houtouhukkyuu" || Name_Only == "turret_rotator_functional")
                    Sound_Setting[0][32].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "taiha" || Name_Only == "vehicle_destroyed")
                    Sound_Setting[0][33].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "hakken")
                    Sound_Setting[1][0].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "lamp")
                    Sound_Setting[1][1].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "battle_bgm")
                    Sound_Setting[1][15].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "chat_allies_send")
                    Sound_Setting[2][0].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "chat_allies_receive")
                    Sound_Setting[2][1].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "chat_enemy_send")
                    Sound_Setting[2][2].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "chat_enemy_receive")
                    Sound_Setting[2][3].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "chat_platoon_send")
                    Sound_Setting[2][4].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
                else if (Name_Only == "chat_platoon_receive")
                    Sound_Setting[2][5].Sounds.Add(new Voice_Sound_Setting(Voice_Now));
            }

            Message_T.Text = "セーブしています...";
            await Task.Delay(50);
            WVS_Save Save = new WVS_Save();
            Save.Add_Sound(Sound_Setting, null);
            Save.Create(dir + "\\" + ProjectName_T.Text + ".wvs", ProjectName_T.Text, !ProjectName_T.IsEnabled, false);
            Save.Dispose();
        }
    }
}