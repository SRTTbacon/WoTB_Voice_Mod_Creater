using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using WK.Libraries.BetterFolderBrowserNS;
using WoTB_Voice_Mod_Creater.Class;
using WoTB_Voice_Mod_Creater.FMOD;

namespace WoTB_Voice_Mod_Creater.Wwise_Class
{
    public partial class Change_To_Wwise : System.Windows.Controls.UserControl
    {
        string Voice_FSB_File = "";
        string BGM_FSB_File = "";
        bool IsClosing = false;
        bool IsMessageShowing = false;
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
            if (File.Exists(Voice_Set.Special_Path + "/Configs/Change_To_Wwise.conf"))
            {
                try
                {
                    using (var eifs = new FileStream(Voice_Set.Special_Path + "/Configs/Change_To_Wwise.conf", FileMode.Open, FileAccess.Read))
                    {
                        using (var eofs = new FileStream(Voice_Set.Special_Path + "/Configs/Change_To_Wwise.tmp", FileMode.Create, FileAccess.Write))
                        {
                            FileEncode.FileEncryptor.Decrypt(eifs, eofs, "Wwise_Setting_Configs_Save");
                        }
                    }
                    StreamReader str = new StreamReader(Voice_Set.Special_Path + "/Configs/Change_To_Wwise.tmp");
                    DVPL_C.IsChecked = bool.Parse(str.ReadLine());
                    Install_C.IsChecked = bool.Parse(str.ReadLine());
                    str.Close();
                    File.Delete(Voice_Set.Special_Path + "/Configs/Change_To_Wwise.tmp");
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show("設定を読み込めませんでした。。\nエラー回避のため設定は削除されます。");
                    File.Delete(Voice_Set.Special_Path + "/Configs/Change_To_Wwise.conf");
                    File.Delete(Voice_Set.Special_Path + "/Configs/Change_To_Wwise.tmp");
                    Sub_Code.Error_Log_Write(e.Message);
                }
            }
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
        private void Voice_Select_B_Click(object sender, RoutedEventArgs e)
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
                    Message_Feed_Out("指定したファイルは対応していません。詳しくは注意事項を参照してください。");
                    return;
                }
                Voices.Clear();
                Voice_FSB_File = ofd.FileName;
                FSB_Details_L.Items[0] = "音声ファイル:" + Path.GetFileName(ofd.FileName);
                FSB_Details_L.Items[2] = "音声数:" + Fmod_Class.FSB_GetLength(ofd.FileName) + "個";
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
            string Message_01 = "この画面では、FModで作成された.fsbファイルをWwiseの.bnkファイルに自動で変換します。\n";
            string Message_02 = "ただし、FModで作成された音声やBGMは、このソフトで作成されたものか、ファイル名を変更していない他の音声Modに限ります。\n";
            string Message_03 = "例1:戦闘開始->start_battle_01 | 貫通->armor_pierced_by_player_01\n";
            string Message_04 = "例2:戦闘開始->battle_01 | 貫通->kantuu_01など\n";
            string Message_05 = "このツールがどこまで通用するかわからないため、できれば音声作成ツールV2で作成することをお勧めします。";
            System.Windows.MessageBox.Show(Message_01 + Message_02 + Message_03 + Message_04 + Message_05);
        }
        //変換
        //fsbからwavファイルを抽出し、ファイル名を変更、adpcmの場合ファイルが破損しているため復元してから.bnkを作成
        private async void Chnage_To_Wwise_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || Opacity < 1)
                return;
            try
            {
                IsClosing = true;
                BetterFolderBrowser sfd = new BetterFolderBrowser()
                {
                    Title = "保存先のフォルダを指定してください。",
                    RootFolder = Sub_Code.Get_OpenDirectory_Path(),
                    Multiselect = false
                };
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    Sub_Code.Set_Directory_Path(sfd.SelectedFolder);
                    FSB_Details_L.Items[4] = "出力先のフォルダ名:" + sfd.SelectedFolder.Substring(sfd.SelectedFolder.LastIndexOf("\\") + 1);
                    if (Directory.Exists(Voice_Set.Special_Path + "/Wwise/FSB_Extract_Voices_TMP"))
                    {
                        Directory.Delete(Voice_Set.Special_Path + "/Wwise/FSB_Extract_Voices_TMP", true);
                    }
                    if (Directory.Exists(Voice_Set.Special_Path + "/Wwise/FSB_Extract_Voices"))
                    {
                        Directory.Delete(Voice_Set.Special_Path + "/Wwise/FSB_Extract_Voices", true);
                    }
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
                    await Multithread.Convert_To_Wav(Voice_Set.Special_Path + "/Wwise/FSB_Extract_Voices_TMP", Voice_Set.Special_Path + "/Wwise/FSB_Extract_Voices", true, true);
                    Directory.Delete(Voice_Set.Special_Path + "/Wwise/FSB_Extract_Voices_TMP", true);
                    if (BGM_Add_Only.Count > 0)
                    {
                        Message_T.Text = "追加されたBGMファイルをwavに変換しています...";
                        await Task.Delay(50);
                        foreach (string File_Now in BGM_Add_Only)
                        {
                            if (!Sub_Code.Audio_IsWAV(File_Now))
                            {
                                Sub_Code.Audio_Encode_To_Other(File_Now, Sub_Code.File_Rename_Get_Name(Voice_Set.Special_Path + "/Wwise/FSB_Extract_Voices/battle_bgm") + ".wav", ".wav", false);
                            }
                            else
                            {
                                File.Copy(File_Now, Sub_Code.File_Rename_Get_Name(Voice_Set.Special_Path + "/Wwise/FSB_Extract_Voices/battle_bgm") + ".wav", true);
                            }
                        }
                    }
                    Message_T.Text = "音声のファイル名を変更しています...";
                    await Task.Delay(50);
                    Voice_Set.Voice_BGM_Name_Change_From_FSB(Voice_Set.Special_Path + "/Wwise/FSB_Extract_Voices");
                    string[] Reload_Files = Directory.GetFiles(Voice_Set.Special_Path + "/Wwise/FSB_Extract_Voices", "reload_*", SearchOption.TopDirectoryOnly);
                    foreach (string Reload_Now in Reload_Files)
                    {
                        FileInfo fi_reload = new FileInfo(Reload_Now);
                        if (fi_reload.Length == 290340 || fi_reload.Length == 335796 || fi_reload.Length == 336036 || fi_reload.Length == 445836 || fi_reload.Length == 497268 || fi_reload.Length == 541980)
                        {
                            fi_reload.Delete();
                        }
                    }
                    string[] Voice_Files = Directory.GetFiles(Voice_Set.Special_Path + "/Wwise/FSB_Extract_Voices", "*.wav", SearchOption.TopDirectoryOnly);
                    //音声の場合はたいていファイル名の語尾に_01や_02と書いているため、書かれていないファイルは削除する
                    foreach (string Voice_Now in Voice_Files)
                    {
                        if (!Path.GetFileNameWithoutExtension(Voice_Now).Contains("_") || !Sub_Code.IsIncludeInt_From_String(Path.GetFileNameWithoutExtension(Voice_Now), "_"))
                        {
                            File.Delete(Voice_Now);
                        }
                    }
                    if (File.Exists(Voice_Set.Special_Path + "/Wwise/FSB_Extract_Voices/lock_on.wav"))
                    {
                        File.Delete(Voice_Set.Special_Path + "/Wwise/FSB_Extract_Voices/lock_on.wav");
                    }
                    if (BGM_Add_List.Items.Count > 0)
                    {
                        Message_T.Text = ".bnkファイルを作成しています...\nBGMが含まれているため時間がかかります...";
                    }
                    else
                    {
                        Message_T.Text = ".bnkファイルを作成しています...";
                    }
                    await Task.Delay(50);
                    FileInfo fi = new FileInfo(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.wwu");
                    if (File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp") && fi.Length >= 800000)
                    {
                        File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.wwu", true);
                    }
                    if (!File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp"))
                    {
                        File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.wwu", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp", true);
                    }
                    Wwise_Class.Wwise_Project_Create Wwise = new Wwise_Class.Wwise_Project_Create(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod");
                    Wwise.Sound_Add_Wwise(Voice_Set.Special_Path + "/Wwise/FSB_Extract_Voices");
                    Wwise.Save();
                    Directory.Delete(Voice_Set.Special_Path + "/Wwise/FSB_Extract_Voices", true);
                    Wwise.Project_Build("voiceover_crew", sfd.SelectedFolder + "/voiceover_crew.bnk");
                    await Task.Delay(500);
                    Wwise.Project_Build("ui_battle", sfd.SelectedFolder + "/ui_battle.bnk");
                    await Task.Delay(500);
                    Wwise.Project_Build("ui_battle_basic", sfd.SelectedFolder + "/ui_battle_basic.bnk");
                    await Task.Delay(500);
                    Wwise.Project_Build("ui_chat_quick_commands", sfd.SelectedFolder + "/ui_chat_quick_commands.bnk");
                    await Task.Delay(500);
                    Wwise.Project_Build("reload", sfd.SelectedFolder + "/reload.bnk");
                    Wwise.Clear();
                    if (File.Exists(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp"))
                    {
                        File.Copy(Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Backup.tmp", Voice_Set.Special_Path + "/Wwise/WoTB_Sound_Mod/Actor-Mixer Hierarchy/Default Work Unit.wwu", true);
                    }
                    if (DVPL_C.IsChecked.Value)
                    {
                        Message_T.Text = "DVPL化しています...";
                        await Task.Delay(50);
                        DVPL.DVPL_Pack(sfd.SelectedFolder + "/voiceover_crew.bnk", sfd.SelectedFolder + "/voiceover_crew.bnk.dvpl", true);
                        DVPL.DVPL_Pack(sfd.SelectedFolder + "/ui_battle.bnk", sfd.SelectedFolder + "/ui_battle.bnk.dvpl", true);
                        DVPL.DVPL_Pack(sfd.SelectedFolder + "/ui_battle_basic.bnk", sfd.SelectedFolder + "/ui_battle_basic.bnk.dvpl", true);
                        DVPL.DVPL_Pack(sfd.SelectedFolder + "/ui_chat_quick_commands.bnk", sfd.SelectedFolder + "/ui_chat_quick_commands.bnk.dvpl", true);
                        DVPL.DVPL_Pack(sfd.SelectedFolder + "/reload.bnk", sfd.SelectedFolder + "/reload.bnk.dvpl", true);
                    }
                    if (Install_C.IsChecked.Value)
                    {
                        Message_T.Text = "WoTBに適応しています...";
                        await Task.Delay(50);
                        Sub_Code.DVPL_File_Delete(Voice_Set.WoTB_Path + "/Data/WwiseSound/ja/voiceover_crew.bnk");
                        Sub_Code.DVPL_File_Delete(Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_battle.bnk");
                        Sub_Code.DVPL_File_Delete(Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_chat_quick_commands.bnk");
                        Sub_Code.DVPL_File_Delete(Voice_Set.WoTB_Path + "/Data/WwiseSound/reload.bnk");
                        Sub_Code.DVPL_File_Copy(sfd.SelectedFolder + "/voiceover_crew.bnk", Voice_Set.WoTB_Path + "/Data/WwiseSound/ja/voiceover_crew.bnk", true);
                        Sub_Code.DVPL_File_Copy(sfd.SelectedFolder + "/ui_battle.bnk", Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_battle.bnk", true);
                        Sub_Code.DVPL_File_Copy(sfd.SelectedFolder + "/ui_chat_quick_commands.bnk", Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_chat_quick_commands.bnk", true);
                        Sub_Code.DVPL_File_Copy(sfd.SelectedFolder + "/reload.bnk", Voice_Set.WoTB_Path + "/Data/WwiseSound/reload.bnk", true);
                    }
                    Message_Feed_Out("完了しました。\nファイル容量が極端に少ない場合は失敗している可能性があります。");
                }
            }
            catch (Exception e1)
            {
                Sub_Code.Error_Log_Write(e1.Message);
                Message_Feed_Out("エラーが発生しました。詳しくはLog.txtを参照してください。");
            }
            IsClosing = false;
        }
        private void Install_Help_B_Click(object sender, RoutedEventArgs e)
        {
            string Message_01 = "・音声ファイル(voiceover_crew.bnk)は、'/Data/WwiseSound/ja'に配置されるため、国籍音声を有効化している場合は、日本の車両でしか再生されません。\n";
            string Message_02 = "・音声を再生するには、ゲーム内の言語を日本語にする必要があります。音声ファイル以外のファイルは言語を問わず再生されるはずです。\n";
            string Message_03 = "・WoTB内のファイルは上書きされるため、事前にバックアップを取っておくと良いかもしれません。";
            System.Windows.MessageBox.Show(Message_01 + Message_02 + Message_03);
        }
        private void DVPL_C_Click(object sender, RoutedEventArgs e)
        {
            Configs_Save();
        }
        private void Install_C_Click(object sender, RoutedEventArgs e)
        {
            Configs_Save();
        }
        void Configs_Save()
        {
            try
            {
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Configs/Change_To_Wwise.tmp");
                stw.WriteLine(DVPL_C.IsChecked.Value);
                stw.Write(Install_C.IsChecked.Value);
                stw.Close();
                using (var eifs = new FileStream(Voice_Set.Special_Path + "/Configs/Change_To_Wwise.tmp", FileMode.Open, FileAccess.Read))
                {
                    using (var eofs = new FileStream(Voice_Set.Special_Path + "/Configs/Change_To_Wwise.conf", FileMode.Create, FileAccess.Write))
                    {
                        FileEncode.FileEncryptor.Encrypt(eifs, eofs, "Wwise_Setting_Configs_Save");
                    }
                }
                File.Delete(Voice_Set.Special_Path + "/Configs/Change_To_Wwise.tmp");
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
    }
}