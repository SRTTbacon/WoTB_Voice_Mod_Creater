using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;

namespace WoTB_Voice_Mod_Creater.Class
{
    public partial class SE_Change : UserControl
    {
        public List<List<string>> Preset_List = new List<List<string>>();
        List<List<string>> SE_Files = new List<List<string>>();
        public int Preset_Index = 0;
        int Stream = 0;
        bool IsClosing = false;
        bool IsMessageShowing = false;
        public SE_Change()
        {
            InitializeComponent();
            SE_List.Items.Add("時間切れ&占領ポイントMax");
            SE_List.Items.Add("クイックコマンド");
            SE_List.Items.Add("弾薬庫破損");
            SE_List.Items.Add("自車両大破");
            SE_List.Items.Add("貫通");
            SE_List.Items.Add("敵モジュール破損");
            SE_List.Items.Add("無線機破損");
            SE_List.Items.Add("燃料タンク破損");
            SE_List.Items.Add("非貫通");
            SE_List.Items.Add("装填完了");
            SE_List.Items.Add("第六感");
            SE_List.Items.Add("敵発見");
            SE_List.Items.Add("戦闘開始前タイマー");
            SE_List.Items.Add("ロックオン");
            SE_List.Items.Add("アンロック");
            SE_List.Items.Add("ノイズ音");
        }
        public async void Window_Show()
        {
            Opacity = 0;
            Visibility = Visibility.Visible;
            if (File.Exists(Voice_Set.Special_Path + "/Configs/Voice_Create.conf"))
            {
                try
                {
                    StreamReader str = Sub_Code.File_Decrypt_To_Stream(Voice_Set.Special_Path + "/Configs/Voice_Create.conf", "Voice_Create_Configs_Save");
                    Volume_S.Value = double.Parse(str.ReadLine());
                    str.Close();
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show("設定を読み込めませんでした。。\nエラー回避のため設定は削除されます。");
                    File.Delete(Voice_Set.Special_Path + "/Configs/Voice_Create.conf");
                    Volume_S.Value = 75;
                    Sub_Code.Error_Log_Write(e.Message);
                }
            }
            else
                Volume_S.Value = 75;
            if (Sub_Code.IsWindowBarShow)
                Delete_B.Margin = new Thickness(-3368, 25, 0, 0);
            else
                Delete_B.Margin = new Thickness(-3368, 0, 0, 0);
            while (Opacity < 1 && !IsClosing)
            {
                Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
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
        void Preset_Save_File()
        {
            try
            {
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "\\Configs\\SE_Change_Presets_Temp.dat");
                stw.WriteLine(Preset_Index);
                foreach (List<string> Preset_Now in Preset_List)
                {
                    if (Preset_Now[0] == "標準")
                        continue;
                    stw.WriteLine("!*---Preset_Start---*!");
                    foreach (string Line in Preset_Now)
                        stw.WriteLine(Line);
                }
                stw.Close();
                stw.Dispose();
                Sub_Code.File_Encrypt(Voice_Set.Special_Path + "\\Configs\\SE_Change_Presets_Temp.dat", Voice_Set.Special_Path + "\\Configs\\SE_Change_Presets.dat", "Period_Lost-Words", true);
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                Message_Feed_Out("エラー:プリセットを保存できませんでした。");
            }
        }
        void Preset_Load_File()
        {
            try
            {
                Preset_List.Clear();
                string SE = Voice_Set.Special_Path + "\\SE\\";
                List<string> Default_Preset = new List<string>();
                Default_Preset.Add("標準");
                Default_Preset.Add(SE + "Capture_End_01.wav|" + SE + "Capture_End_02.wav");
                Default_Preset.Add(SE + "Command_01.wav");
                Default_Preset.Add(SE + "Danyaku_SE_01.mp3");
                Default_Preset.Add(SE + "Destroy_01.wav");
                Default_Preset.Add(SE + "Enable_01.mp3|" + SE + "Enable_02.mp3|" + SE + "Enable_03.mp3");
                Default_Preset.Add(SE + "Enable_Special_01.mp3");
                Default_Preset.Add(SE + "Musenki_01.wav");
                Default_Preset.Add(SE + "Nenryou_SE_01.mp3");
                Default_Preset.Add(SE + "Not_Enable_01.mp3");
                Default_Preset.Add(SE + "Reload_01.mp3|" + SE + "Reload_02.mp3|" + SE + "Reload_03.mp3|" + SE + "Reload_04.mp3|" + SE + "Reload_05.mp3|" + SE + "Reload_06.mp3");
                Default_Preset.Add(SE + "Sixth_01.wav|" + SE + "Sixth_02.wav|" + SE + "Sixth_03.mp3");
                Default_Preset.Add(SE + "Spot_01.wav");
                Default_Preset.Add(SE + "Timer_01.wav|" + SE + "Timer_02.wav");
                Default_Preset.Add(SE + "Lock_01.wav");
                Default_Preset.Add(SE + "Unlock_01.wav");
                Default_Preset.Add(SE + "Noise_01.mp3|" + SE + "Noise_02.mp3|" + SE + "Noise_03.mp3|" + SE + "Noise_04.mp3|" + SE + "Noise_05.mp3|" + SE + "Noise_06.mp3|" + SE + "Noise_07.mp3|" + SE + "Noise_08.mp3|" + SE + "Noise_09.mp3|" + SE + "Noise_10.mp3");
                Preset_List.Add(Default_Preset);
                Load_Combo.Items.Add("標準");
                int Index = 0;
                if (File.Exists(Voice_Set.Special_Path + "\\Configs\\SE_Change_Presets.dat"))
                {
                    Sub_Code.File_Decrypt_To_File(Voice_Set.Special_Path + "\\Configs\\SE_Change_Presets.dat", Voice_Set.Special_Path + "\\Configs\\SE_Change_Presets_Temp.dat", "Period_Lost-Words", false);
                    string[] All_Lines = File.ReadAllLines(Voice_Set.Special_Path + "\\Configs\\SE_Change_Presets_Temp.dat");
                    File.Delete(Voice_Set.Special_Path + "\\Configs\\SE_Change_Presets_Temp.dat");
                    for (int Number = 0; Number < All_Lines.Length; Number++)
                    {
                        if (Number == 0)
                            Index = int.Parse(All_Lines[Number]);
                        else if (All_Lines[Number] == "!*---Preset_Start---*!")
                        {
                            List<string> New_Preset = new List<string>();
                            New_Preset.Add(All_Lines[Number + 1]);
                            New_Preset.Add(All_Lines[Number + 2]);
                            New_Preset.Add(All_Lines[Number + 3]);
                            New_Preset.Add(All_Lines[Number + 4]);
                            New_Preset.Add(All_Lines[Number + 5]);
                            New_Preset.Add(All_Lines[Number + 6]);
                            New_Preset.Add(All_Lines[Number + 7]);
                            New_Preset.Add(All_Lines[Number + 8]);
                            New_Preset.Add(All_Lines[Number + 9]);
                            New_Preset.Add(All_Lines[Number + 10]);
                            New_Preset.Add(All_Lines[Number + 11]);
                            New_Preset.Add(All_Lines[Number + 12]);
                            New_Preset.Add(All_Lines[Number + 13]);
                            New_Preset.Add(All_Lines[Number + 14]);
                            New_Preset.Add(All_Lines[Number + 15]);
                            New_Preset.Add(All_Lines[Number + 16]);
                            Number += 16;
                            if (All_Lines.Length > Number + 17)
                            {
                                if (All_Lines[Number + 17].Contains("!*---Preset_Start---*!"))
                                    New_Preset.Add("");
                                else
                                    New_Preset.Add(All_Lines[Number + 17]);
                                Number++;
                            }
                            else
                                New_Preset.Add("");
                            Preset_List.Add(New_Preset);
                            Load_Combo.Items.Add(New_Preset[0]);
                        }
                    }
                }
                Load_Combo.SelectedIndex = Index;
                Preset_Index = Index;
                Load_Preset(Index, false);
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                Message_Feed_Out("エラーが発生しました。プリセットをロードできません。");
                MessageBox.Show("エラー:プリセットをロードできませんでした。\n詳しくはError_Log.txtを参照してください。");
            }
        }
        private void Save_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            if (Save_Name_T.Text == "")
            {
                Message_Feed_Out("プリセット名を入力してください。");
                return;
            }
            if (Save_Name_T.Text == "標準")
            {
                Message_Feed_Out("標準プリセットは上書きできません。");
                return;
            }
            else if (Save_Name_T.Text.Contains("|"))
            {
                Message_Feed_Out("プリセット名に'|'を使用することはできません。");
                return;
            }
            List<string> New_Preset = new List<string>();
            New_Preset.Add(Save_Name_T.Text);
            for (int Number = 0; Number < SE_Files.Count; Number++)
            {
                string Add_Line = "";
                foreach (string File_Path in SE_Files[Number])
                {
                    if (Add_Line == "")
                        Add_Line += File_Path;
                    else
                        Add_Line += "|" + File_Path;
                }
                New_Preset.Add(Add_Line);
            }
            int Index = -1;
            for (int Number_01 = 0; Number_01 < Preset_List.Count; Number_01++)
            {
                if (Preset_List[Number_01][0] == Save_Name_T.Text)
                {
                    Index = Number_01;
                    break;
                }
            }
            if (Index == -1)
            {
                Preset_List.Add(New_Preset);
                Load_Combo.Items.Add(New_Preset[0]);
                Load_Combo.SelectedIndex = Load_Combo.Items.Count - 1;
                Preset_Index = Load_Combo.Items.Count - 1;
            }
            else
            {
                Preset_List[Index] = New_Preset;
                Load_Combo.SelectedIndex = Index;
                Preset_Index = Index;
            }
            Preset_Save_File();
            Message_Feed_Out("プリセットを保存しました。");
        }
        private void Delete_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            if (Load_Combo.SelectedIndex == 0)
            {
                Message_Feed_Out("標準プリセットは削除できません。");
                return;
            }
            string Preset_Name = Load_Combo.Items[Load_Combo.SelectedIndex].ToString();
            MessageBoxResult result = MessageBox.Show("プリセット:" + Preset_Name + "を削除しますか?", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                SE_Sound_List.Items.Clear();
                Preset_List.RemoveAt(Load_Combo.SelectedIndex);
                Load_Combo.Items.RemoveAt(Load_Combo.SelectedIndex);
                Load_Combo.SelectedIndex = 0;
                Preset_Index = 0;
                SE_List.SelectedIndex = -1;
                Load_Preset(0, false);
            }
            Message_Feed_Out("プリセット:" + Preset_Name + "を削除しました。");
        }
        private async void Adaptation_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            MessageBoxResult result = MessageBox.Show("Mod内に追加されるSEは、現在選択しているプリセットから使用されます。適応する前に必ずプリセットの保存を行ってください。\n" +
                "現在選択中のプリセット:" + Preset_List[Preset_Index][0] + "   続行しますか?", "確認",
                MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                IsClosing = true;
                Preset_Save_File();
                float Volume_Now = 1f;
                Bass.BASS_ChannelGetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, ref Volume_Now);
                float Volume_Minus = Volume_Now / 15f;
                while (Opacity > 0)
                {
                    Volume_Now -= Volume_Minus;
                    if (Volume_Now < 0f)
                        Volume_Now = 0f;
                    Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, Volume_Now);
                    Opacity -= Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
                Bass.BASS_ChannelStop(Stream);
                Bass.BASS_StreamFree(Stream);
                Visibility = Visibility.Hidden;
                IsClosing = false;
            }
        }
        private void Load_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            MessageBoxResult result = MessageBox.Show("プリセット:" + Load_Combo.SelectedItem.ToString() + "をロードしますか?", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
                Load_Preset(Load_Combo.SelectedIndex, true);
        }
        void Load_Preset(int Index, bool IsShowMessage)
        {
            SE_Sound_List.Items.Clear();
            SE_Sound_List.SelectedIndex = -1;
            SE_List.SelectedIndex = -1;
            SE_Files.Clear();
            for (int Number = 0; Number < Preset_List[Index].Count; Number++)
            {
                if (Number == 0)
                {
                    if (Preset_List[Index][0] != "標準")
                        Save_Name_T.Text = Preset_List[Index][0];
                }
                else
                {
                    List<string> Files = new List<string>();
                    string Line = Preset_List[Index][Number];
                    if (Line != "")
                    {
                        string[] Cut_Line = Line.Split('|');
                        foreach (string File_Now in Cut_Line)
                            Files.Add(File_Now);
                    }
                    SE_Files.Add(Files);
                }
            }
            if (SE_Files.Count < 17)
                SE_Files.Add(new List<string>());
            Preset_Index = Index;
            if (IsShowMessage)
                Message_Feed_Out("プリセット:" + Preset_List[Index][0] + "をロードしました。");
        }
        private void SE_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SE_List.SelectedIndex == -1)
                return;
            SE_Sound_List.Items.Clear();
            foreach (string File_Now in SE_Files[SE_List.SelectedIndex])
                SE_Sound_List.Items.Add(Path.GetFileName(File_Now));
        }
        private void SE_List_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SE_List.SelectedIndex = -1;
            SE_Sound_List.Items.Clear();
        }
        private void SE_Sound_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsClosing)
                return;
            Bass.BASS_ChannelStop(Stream);
            Bass.BASS_StreamFree(Stream);
        }
        private void SE_Play_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || SE_List.SelectedIndex == -1 || SE_Sound_List.SelectedIndex == -1)
            {
                Message_Feed_Out("ファイルが選択されていません。");
                return;
            }
            if (!File.Exists(SE_Files[SE_List.SelectedIndex][SE_Sound_List.SelectedIndex]))
            {
                Message_Feed_Out("音声ファイルが存在しません。削除された可能性があります。");
                return;
            }
            Bass.BASS_ChannelStop(Stream);
            Bass.BASS_StreamFree(Stream);
            int StreamHandle = Bass.BASS_StreamCreateFile(SE_Files[SE_List.SelectedIndex][SE_Sound_List.SelectedIndex], 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_LOOP);
            Stream = BassFx.BASS_FX_TempoCreate(StreamHandle, BASSFlag.BASS_FX_FREESOURCE);
            Bass.BASS_ChannelSetDevice(Stream, Video_Mode.Sound_Device);
            Bass.BASS_ChannelPlay(Stream, false);
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)Volume_S.Value / 100);
        }
        private void SE_Pause_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            Bass.BASS_ChannelStop(Stream);
        }
        private void Volume_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsClosing)
                return;
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)Volume_S.Value / 100);
            Volume_T.Text = "音量:" + (int)Volume_S.Value;
        }
        private void Help_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            string Message_01 = "!*---重要---*!\n・プリセットを保存せずに適応すると、変更や追加を行ったSEは反映されなくなります。\n";
            string Message_02 = "・適応する際は該当のプリセットを\"ロード\"されている状態かよく確認して実行してください。\n\n";
            string Message_03 = "!*---注意---*!\n";
            string Message_04 = "・この画面で設定した音量はゲーム内には反映されません。\n";
            string Message_05 = "・プリセットが保存されているファイルを自身で編集しないでください。ソフトが正常に起動しなくなる可能性があります。";
            MessageBox.Show(Message_01 + Message_02 + Message_03 + Message_04 + Message_05);
        }
        private void SE_Add_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            if (SE_List.SelectedIndex == -1)
            {
                Message_Feed_Out("SEの種類を選択してください。");
                return;
            }
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog()
            {
                Title = "サウンドファイルを選択してください。",
                Filter = "サウンドファイル(*.aac;*.flac;*.m4a;*.mp3;*.mp4;*.ogg;*.wav;*.wma)|*.aac;*.flac;*.m4a;*.mp3;*.mp4;*.ogg;*.wav;*.wma",
                Multiselect = true,
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string Error_File = "";
                foreach (string File_Now in ofd.FileNames)
                {
                    bool IsExist = false;
                    foreach (string File_Temp in SE_Files[SE_List.SelectedIndex])
                    {
                        if (File_Temp == File_Now)
                        {
                            if (Error_File == "")
                                Error_File = Path.GetFileName(File_Now);
                            else
                                Error_File += "\n" + Path.GetFileName(File_Now);
                            IsExist = true;
                            break;
                        }
                    }
                    if (IsExist)
                        continue;
                    SE_Files[SE_List.SelectedIndex].Add(File_Now);
                    SE_Sound_List.Items.Add(Path.GetFileName(File_Now));
                }
                if (Error_File != "")
                    MessageBox.Show("以下のファイルは既に追加されているため処理できませんでした。\n" + Error_File);
            }
            ofd.Dispose();
        }
        private void SE_Delete_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            if (SE_List.SelectedIndex == -1)
            {
                Message_Feed_Out("SEの種類を選択してください。");
                return;
            }
            else if (SE_Sound_List.SelectedIndex == -1)
            {
                Message_Feed_Out("削除するサウンドファイルを選択してください。");
                return;
            }
            SE_Files[SE_List.SelectedIndex].RemoveAt(SE_Sound_List.SelectedIndex);
            SE_Sound_List.Items.RemoveAt(SE_Sound_List.SelectedIndex);
        }
        private void SE_Change_Loaded(object sender, RoutedEventArgs e)
        {
            Preset_Load_File();
        }
    }
}