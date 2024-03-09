using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Un4seen.Bass;

namespace WoTB_Voice_Mod_Creater.Class
{
    public partial class Voice_Mod_Mixer : UserControl
    {
        public class WVSEvent
        {
            public int fileCount;
            public bool bEnable;

            public WVSEvent()
            {
                fileCount = 0;
                bEnable = true;
            }
        }
        public class WVSFile
        {
            public readonly List<List<WVSEvent>> wvsEvents = new List<List<WVSEvent>>();
            public WVS_Load wvsFile = new WVS_Load();
            public List<List<Voice_Event_Setting>> soundSettings = new List<List<Voice_Event_Setting>>();
            public string filePath = "";

            public WVSFile(string filePath)
            {
                this.filePath = filePath;
                for (int i = 0; i < 3; i++)
                    wvsEvents.Add(new List<WVSEvent>());
                for (int i = 0; i < 34; i++)
                    wvsEvents[0].Add(new WVSEvent());
                for (int i = 0; i < 17; i++)
                    wvsEvents[1].Add(new WVSEvent());
                for (int i = 0; i < 6; i++)
                    wvsEvents[2].Add(new WVSEvent());

                soundSettings.Clear();
                for (int Number = 0; Number < 3; Number++)
                    soundSettings.Add(new List<Voice_Event_Setting>());
                for (int i = 0; i < 34; i++)
                    soundSettings[0].Add(new Voice_Event_Setting());
                for (int i = 0; i < 17; i++)
                {
                    if (i == 15)
                        soundSettings[1][soundSettings[1].Count - 1].Volume = -11;
                    soundSettings[1].Add(new Voice_Event_Setting());
                }
                for (int i = 0; i < 6; i++)
                    soundSettings[2].Add(new Voice_Event_Setting());


                wvsFile.WVS_Load_File(filePath, soundSettings);
                for (int i = 0; i < soundSettings.Count; i++)
                {
                    for (int j = 0; j < soundSettings[i].Count; j++)
                    {
                        wvsEvents[i][j].fileCount = soundSettings[i][j].Sounds.Count;
                    }
                }
            }
            public void Dispose()
            {
                wvsEvents.Clear();
                wvsFile.Dispose();
                soundSettings.Clear();
            }
        }
        readonly List<List<string>> voiceEvents = new List<List<string>>();
        readonly List<WVSFile> wvsFiles = new List<WVSFile>();
        readonly Brush disableColor = new BrushConverter().ConvertFromString("#BFFF2C8C") as Brush;
        int selectedEventList = 0;
        bool bMessageShowing = false;
        bool bBusy = false;
        bool bClosing = false;

        public Voice_Mod_Mixer()
        {
            InitializeComponent();
            for (int i = 0; i < 3; i++)
                voiceEvents.Add(new List<string>());
            voiceEvents[0].Add("味方にダメージ");
            voiceEvents[0].Add("弾薬庫破損");
            voiceEvents[0].Add("敵への無効弾");
            voiceEvents[0].Add("敵への貫通弾");
            voiceEvents[0].Add("敵への致命弾");
            voiceEvents[0].Add("敵への跳弾");
            voiceEvents[0].Add("車長負傷");
            voiceEvents[0].Add("操縦手負傷");
            voiceEvents[0].Add("敵炎上");
            voiceEvents[0].Add("敵撃破");
            voiceEvents[0].Add("エンジン破損");
            voiceEvents[0].Add("エンジン大破");
            voiceEvents[0].Add("エンジン復旧");
            voiceEvents[0].Add("自車両火災");
            voiceEvents[0].Add("自車両消火");
            voiceEvents[0].Add("燃料タンク破損");
            voiceEvents[0].Add("主砲破損");
            voiceEvents[0].Add("主砲大破");
            voiceEvents[0].Add("主砲復旧");
            voiceEvents[0].Add("砲手負傷");
            voiceEvents[0].Add("装填手負傷");
            voiceEvents[0].Add("通信機破損");
            voiceEvents[0].Add("通信手負傷");
            voiceEvents[0].Add("戦闘開始");
            voiceEvents[0].Add("観測装置破損");
            voiceEvents[0].Add("観測装置大破");
            voiceEvents[0].Add("観測装置復旧");
            voiceEvents[0].Add("履帯破損");
            voiceEvents[0].Add("履帯大破");
            voiceEvents[0].Add("履帯復旧");
            voiceEvents[0].Add("砲塔破損");
            voiceEvents[0].Add("砲塔大破");
            voiceEvents[0].Add("砲塔復旧");
            voiceEvents[0].Add("自車両大破");
            voiceEvents[1].Add("敵発見");
            voiceEvents[1].Add("第六感");
            voiceEvents[1].Add("了解");
            voiceEvents[1].Add("拒否");
            voiceEvents[1].Add("救援を請う");
            voiceEvents[1].Add("攻撃せよ！");
            voiceEvents[1].Add("攻撃中");
            voiceEvents[1].Add("陣地を占領せよ！");
            voiceEvents[1].Add("陣地を防衛せよ！");
            voiceEvents[1].Add("固守せよ！");
            voiceEvents[1].Add("ロックオン");
            voiceEvents[1].Add("アンロック");
            voiceEvents[1].Add("装填完了");
            voiceEvents[1].Add("マップクリック時");
            voiceEvents[1].Add("戦闘終了時(時間切れや占領時のみ)");
            voiceEvents[1].Add("戦闘BGM");
            voiceEvents[1].Add("移動中！");
            voiceEvents[2].Add("チャット:味方-送信");
            voiceEvents[2].Add("チャット:味方-受信");
            voiceEvents[2].Add("チャット:全体-送信");
            voiceEvents[2].Add("チャット:全体-受信");
            voiceEvents[2].Add("チャット:小隊-送信");
            voiceEvents[2].Add("チャット:小隊-受信");
        }
        async void Message_Feed_Out(string Message)
        {
            //テキストが一定期間経ったらフェードアウト
            if (bMessageShowing)
            {
                bMessageShowing = false;
                await Task.Delay(1000 / 59);
            }
            Message_T.Text = Message;
            bMessageShowing = true;
            Message_T.Opacity = 1;
            int Number = 0;
            while (Message_T.Opacity > 0 && bMessageShowing)
            {
                Number++;
                if (Number >= 120)
                    Message_T.Opacity -= 0.025;
                await Task.Delay(1000 / 60);
            }
            bMessageShowing = false;
            Message_T.Text = "";
            Message_T.Opacity = 1;
        }
        public async void Window_Show()
        {
            Opacity = 0;
            Visibility = Visibility.Visible;
            while (Opacity < 1 && !bClosing)
            {
                Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
        }
        private async void Back_B_Click(object sender, RoutedEventArgs e)
        {
            if (!bClosing && !bBusy)
            {
                bClosing = true;
                while (Opacity > 0)
                {
                    Opacity -= Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
                Visibility = Visibility.Hidden;
                bClosing = false;
            }
        }
        void UpdateEventList(int nextEventListNum = -1)
        {
            if (nextEventListNum != -1)
                selectedEventList = nextEventListNum;
            EventL.Items.Clear();
            if (WVSFileL.SelectedIndex == -1)
                return;
            for (int i = 0; i < voiceEvents[selectedEventList].Count; i++)
            {
                string context = voiceEvents[selectedEventList][i];
                context += "(" + wvsFiles[WVSFileL.SelectedIndex].wvsEvents[selectedEventList][i].fileCount + "個) | ";

                ListBoxItem LBI = new ListBoxItem();
                LBI.Foreground = Brushes.Aqua;
                if (wvsFiles[WVSFileL.SelectedIndex].wvsEvents[selectedEventList][i].bEnable && wvsFiles[WVSFileL.SelectedIndex].wvsEvents[selectedEventList][i].fileCount > 0)
                    context += "有効";
                else
                {
                    context += "無効";
                    LBI.Foreground = disableColor;
                }
                LBI.Content = context;
                EventL.Items.Add(LBI);
            }
            Voice_List_T.Text = "音声リスト" + (selectedEventList + 1);
        }
        private void WVSFileL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WVSFileL.SelectedIndex == -1)
                return;
            UpdateEventList();
        }
        private void Event_Back_B_Click(object sender, RoutedEventArgs e)
        {
            if (bClosing || bBusy)
                return;
            if (selectedEventList > 0)
                UpdateEventList(selectedEventList - 1);
        }
        private void Event_Next_B_Click(object sender, RoutedEventArgs e)
        {
            if (bClosing || bBusy)
                return;
            if (selectedEventList < 2)
                UpdateEventList(selectedEventList + 1);
        }
        private void Event_Disable_B_Click(object sender, RoutedEventArgs e)
        {
            if (bClosing || bBusy || WVSFileL.SelectedIndex == -1 || EventL.SelectedIndex == -1)
                return;
            int selectedIndex = EventL.SelectedIndex;
            wvsFiles[WVSFileL.SelectedIndex].wvsEvents[selectedEventList][EventL.SelectedIndex].bEnable = false;
            UpdateEventList();
            EventL.SelectedIndex = selectedIndex;
        }
        private void Event_Enable_B_Click(object sender, RoutedEventArgs e)
        {
            if (bClosing || bBusy || WVSFileL.SelectedIndex == -1 || EventL.SelectedIndex == -1)
                return;
            int selectedIndex = EventL.SelectedIndex;
            wvsFiles[WVSFileL.SelectedIndex].wvsEvents[selectedEventList][EventL.SelectedIndex].bEnable = true;
            UpdateEventList();
            EventL.SelectedIndex = selectedIndex;
        }
        private void OpenWVSFile_B_Click(object sender, RoutedEventArgs e)
        {
            if (bClosing || bBusy)
                return;
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "セーブファイル(*.wvs)|*.wvs";
            ofd.Title = "合成させたい.wvsファイルを選択してください。";
            ofd.Multiselect = true;

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                List<string> existFiles = new List<string>();
                List<string> errorFiles = new List<string>();
                List<string> overFiles = new List<string>();
                int addCount = 0;
                foreach (string file in ofd.FileNames)
                {
                    bool bExist = false;
                    foreach (WVSFile wvsFile in wvsFiles)
                    {
                        if (wvsFile.filePath == file)
                        {
                            existFiles.Add(file);
                            bExist = true;
                            break;
                        }
                    }
                    if (!bExist)
                    {
                        if (wvsFiles.Count >= 50)
                        {
                            overFiles.Add(file);
                            continue;
                        }
                        WVSFile wvsFile = new WVSFile(file);
                        if (!wvsFile.wvsFile.IsLoaded)
                        {
                            errorFiles.Add(file);
                            continue;
                        }
                        wvsFiles.Add(wvsFile);
                        WVSFileL.Items.Add(Path.GetFileName(Path.GetDirectoryName(file)) + "/" + Path.GetFileName(file));
                        addCount++;
                    }
                }

                if (existFiles.Count > 0)
                {
                    string message = "以下のファイルは既に追加されているためスキップしました。";
                    foreach (string existFile in existFiles)
                        message += "\n" + Path.GetFileName(Path.GetDirectoryName(existFile)) + "/" + Path.GetFileName(existFile);
                    MessageBox.Show(message);
                }

                if (errorFiles.Count > 0)
                {
                    string message = "正常にロードできなかったため、以下のファイルをスキップしました。";
                    foreach (string errorFile in errorFiles)
                        message += "\n" + Path.GetFileName(Path.GetDirectoryName(errorFile)) + "/" + Path.GetFileName(errorFile);
                    MessageBox.Show(message);
                }

                if (overFiles.Count > 0)
                {
                    string message = "合成できるセーブファイルは最大50個までのため、以下のファイルをスキップしました。";
                    foreach (string overFile in overFiles)
                        message += "\n" + Path.GetFileName(Path.GetDirectoryName(overFile)) + "/" + Path.GetFileName(overFile);
                    MessageBox.Show(message);
                }

                if (addCount == 0)
                    Message_Feed_Out("ロードできるファイルがありませんでした。");
                else if (addCount == 1)
                    Message_Feed_Out("セーブファイルを追加しました。");
                else
                    Message_Feed_Out(addCount + "個のセーブファイルを追加しました。");
            }
            ofd.Dispose();
        }
        private void RemoveWVSFile_B_Click(object sender, RoutedEventArgs e)
        {
            if (bClosing || bBusy)
                return;
            if (WVSFileL.SelectedIndex == -1)
            {
                Message_Feed_Out("削除したい.wvsファイルを選択してください。");
                return;
            }
            string fileName = WVSFileL.Items[WVSFileL.SelectedIndex].ToString();
            MessageBoxResult result = MessageBox.Show("セーブファイル(" + fileName + ")をリストから削除しますか?", "確認", MessageBoxButton.YesNo,
                MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                WVSFileL.Items.RemoveAt(WVSFileL.SelectedIndex);
                UpdateEventList();
                Message_Feed_Out(fileName + "をリストから削除しました。");
            }
        }
        private async void CreateSaveData_B_Click(object sender, RoutedEventArgs e)
        {
            if (bClosing || bBusy)
                return;
            if (wvsFiles.Count < 2)
            {
                Message_Feed_Out("音声Mod同士を合成させる機能のため、セーブファイルは最低2つ必要です。");
                return;
            }

            bool bIncludeMode = false;
            foreach (WVSFile wvsFile in wvsFiles)
            {
                if (wvsFile.wvsFile.IsIncludedSound)
                {
                    bIncludeMode = true;
                    break;
                }
            }

            if (bIncludeMode)
            {
                string message = "'セーブファイルにサウンドも含める'にチェックが入ったデータが存在するため、合成する際に中のサウンドはすべて抽出されます。";
                MessageBox.Show(message);
            }

            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Filter = "セーブファイル(*.wvs)|*.wvs";
            sfd.Title = "保存先を指定してください。";

            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (WVSFile wvsFile in wvsFiles)
                {
                    if (wvsFile.filePath == sfd.FileName)
                    {
                        Message_Feed_Out("合成元のセーブファイルに上書きできません。別のファイル名を指定してください。");
                        sfd.Dispose();
                        return;
                    }
                }

                bBusy = true;

                Message_T.Text = "セーブファイルを合成しています...";
                await Task.Delay(50);

                List<List<Voice_Event_Setting>> newSoundSettings = new List<List<Voice_Event_Setting>>();
                foreach (List<Voice_Event_Setting> eventSettings in wvsFiles[0].soundSettings)
                {
                    List<Voice_Event_Setting> newEventSettings = new List<Voice_Event_Setting>();
                    newSoundSettings.Add(newEventSettings);
                    foreach (Voice_Event_Setting eventSetting in eventSettings)
                    {
                        Voice_Event_Setting eventSettingClone = eventSetting.Clone();
                        eventSettingClone.Sounds.Clear();
                        newEventSettings.Add(eventSettingClone);
                    }
                }
                for (int i = 0; i < wvsFiles.Count; i++)
                {
                    for (int j = 0; j < wvsFiles[i].soundSettings.Count; j++)
                    {
                        for ( int k = 0; k < wvsFiles[i].soundSettings[j].Count; k++)
                        {
                            if (!wvsFiles[i].wvsEvents[j][k].bEnable)
                                continue;
                            foreach (Voice_Sound_Setting soundSetting in wvsFiles[i].soundSettings[j][k].Sounds)
                            {
                                Voice_Sound_Setting newSoundSetting = soundSetting.Clone();
                                if (wvsFiles[i].wvsFile.IsIncludedSound)
                                {
                                    byte[] soundBytes = wvsFiles[i].wvsFile.Load_Sound(soundSetting.Stream_Position);
                                    string toFile = Path.GetDirectoryName(sfd.FileName) + "\\" + Path.GetFileNameWithoutExtension(sfd.FileName) + "\\";
                                    toFile += Path.GetFileNameWithoutExtension(soundSetting.File_Path) + "_" + Sub_Code.r.Next(10000, 1000000) + Path.GetExtension(soundSetting.File_Path);
                                    if (!Directory.Exists(Path.GetDirectoryName(toFile)))
                                        Directory.CreateDirectory(Path.GetDirectoryName(toFile));
                                    File.WriteAllBytes(toFile, soundBytes);
                                    newSoundSetting.File_Path = toFile;
                                }
                                newSoundSettings[j][k].Sounds.Add(newSoundSetting);
                            }
                        }
                    }

                    WVS_Save wvsSave = new WVS_Save();
                    wvsSave.Add_Sound(newSoundSettings, null);
                    wvsSave.Create(sfd.FileName, wvsFiles[0].wvsFile.Project_Name + "_Mix", false, false);
                    wvsSave.Dispose();
                    Message_Feed_Out(wvsFiles.Count + "個のセーブファイルを合成しました。");
                }
                bBusy = false;
            }
            sfd.Dispose();
        }
        private void Help1_B_Click(object sender, RoutedEventArgs e)
        {
            string message = "'セーブファイルにサウンドも含める'にチェックが入ったデータが存在する場合、合成する際に中のサウンドはすべて抽出されます。\n";
            message += "抽出先は'.wvsファイルが保存されるフォルダ/保存名/'です。";
            message += "そのため、生成されるセーブファイルは'セーブファイルにサウンドも含める'にチェックが入っていない状態になります。";
            MessageBox.Show(message);
        }
        private void Help2_B_Click(object sender, RoutedEventArgs e)
        {
            string message = "Event/Sound設定は一番最初に追加されたセーブファイルが優先されます。\n";
            message += "そのため、2つ目以降に追加されたセーブファイルのEvent/Sound設定はすべて無視されるためご注意ください。";
            MessageBox.Show(message);
        }
        private void Clear_B_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("内容をクリアしますか?", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (result != MessageBoxResult.Yes)
                return;
            foreach (WVSFile wvsFile in wvsFiles)
                wvsFile.Dispose();
            wvsFiles.Clear();
            WVSFileL.Items.Clear();
            EventL.Items.Clear();
            selectedEventList = 0;
            Message_Feed_Out("内容をクリアしました。");
        }
    }
}