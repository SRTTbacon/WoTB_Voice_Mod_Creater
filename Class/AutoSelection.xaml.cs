using AngleSharp.Io.Dom;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WoTB_Voice_Mod_Creater.Class
{
    public partial class AutoSelection : UserControl
    {
        const string WOT_DATA_PATH = "\\Configs\\WoTAutoSelection.conf";
        const string WOTB_DATA_PATH = "\\Configs\\WoTBAutoSelection.conf";
        const byte VERSION = 0;

        public readonly ObservableCollection<CAutoSelectionEvent> m_eventList = new ObservableCollection<CAutoSelectionEvent>();

        CAutoSelectionItem renameTemp = null;

        bool bClosing = false;          //画面フェードアウト中
        bool bRenameClosing = false;    //リネーム画面フェードアウト中
        bool bMessageShowing = false;   //メッセージ表示中
        bool bFolderMode = false;       //検索キーをフォルダ名にするか
        bool bIncludeSubFolder = false; //サブフォルダを再帰的に検索するか
        bool bWoTMode = false;          //WoTの場合true、WoTBの場合false

        public AutoSelection()
        {
            InitializeComponent();
        }

        public async void Window_Show(List<Dictionary<string, Voice_Event_Setting>> Sound_Setting, bool bShow)
        {
            //初期化
            m_eventList.Clear();
            for (int i = 0; i < Sound_Setting.Count; i++)
            {
                foreach (KeyValuePair<string, Voice_Event_Setting> events in Sound_Setting[i])
                {
                    m_eventList.Add(new CAutoSelectionEvent(events.Key, events.Value.Voice_ShortID));
                }
            }

            Event_List.ItemsSource = m_eventList;
            Item_List.ItemsSource = null;

            bWoTMode = true;

            Load(bWoTMode);

            if (bShow)
            {
                //画面を表示
                Opacity = 0.0;
                Visibility = Visibility.Visible;
                while (Opacity < 1.0 && !bClosing)
                {
                    Opacity += Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
            }
        }

        public async void Window_Show(List<List<Voice_Event_Setting>> soundSetting, List<string> mainEventNames, List<string> sub1EventNames, List<string> sub2EventNames, bool bShow)
        {
            //初期化
            m_eventList.Clear();
            for (int i = 0; i < soundSetting.Count; i++)
            {
                List<string> eventNames;
                if (i == 0)
                    eventNames = mainEventNames;
                else if (i == 1)
                    eventNames = sub1EventNames;
                else
                    eventNames = sub2EventNames;

                for (int j = 0; j < eventNames.Count; j++)
                {
                    string eventName = eventNames[j].Substring(0, eventNames[j].LastIndexOf('|') - 1);
                    m_eventList.Add(new CAutoSelectionEvent(eventName, soundSetting[i][j].Voice_ShortID));
                }
            }

            Event_List.ItemsSource = m_eventList;
            Item_List.ItemsSource = null;

            bWoTMode = false;

            Load(bWoTMode);

            if (bShow)
            {
                //画面を表示
                Opacity = 0.0;
                Visibility = Visibility.Visible;
                while (Opacity < 1.0 && !bClosing)
                {
                    Opacity += Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
            }
        }

        private async void Exit_B_Click(object sender, RoutedEventArgs e)
        {
            Save();

            bClosing = true;
            while (Opacity > 0.0)
            {
                Opacity -= Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
            Visibility = Visibility.Hidden;
            bClosing = false;
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
            while (Message_T.Opacity > 0.0 && bMessageShowing)
            {
                Number++;
                if (Number >= 120)
                    Message_T.Opacity -= 0.025;
                await Task.Delay(1000 / 60);
            }
            if (bMessageShowing)
            {
                bMessageShowing = false;
                Message_T.Text = "";
            }
            Message_T.Opacity = 1;
        }

        //チェックボックス
        private void FolderMode_C_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (bFolderMode)
            {
                bFolderMode = false;
                FolderMode_C.Source = Sub_Code.Check_02;
            }
            else
            {
                bFolderMode = true;
                FolderMode_C.Source = Sub_Code.Check_04;
            }
        }
        private void FolderMode_C_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (bFolderMode)
                FolderMode_C.Source = Sub_Code.Check_04;
            else
                FolderMode_C.Source = Sub_Code.Check_02;
        }
        private void FolderMode_C_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (bFolderMode)
                FolderMode_C.Source = Sub_Code.Check_03;
            else
                FolderMode_C.Source = Sub_Code.Check_01;
        }

        private void IncludeSubFolder_C_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (bIncludeSubFolder)
            {
                bIncludeSubFolder = false;
                IncludeSubFolder_C.Source = Sub_Code.Check_02;
            }
            else
            {
                bIncludeSubFolder = true;
                IncludeSubFolder_C.Source = Sub_Code.Check_04;
            }
        }
        private void IncludeSubFolder_C_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (bIncludeSubFolder)
                IncludeSubFolder_C.Source = Sub_Code.Check_04;
            else
                IncludeSubFolder_C.Source = Sub_Code.Check_02;
        }
        private void IncludeSubFolder_C_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (bIncludeSubFolder)
                IncludeSubFolder_C.Source = Sub_Code.Check_03;
            else
                IncludeSubFolder_C.Source = Sub_Code.Check_01;
        }

        private async void Rename_B_Click(object sender, RoutedEventArgs e)
        {
            if (bClosing || Event_List.SelectedIndex == -1)
                return;

            CAutoSelectionEvent autoEvent = m_eventList[Event_List.SelectedIndex];

            Button button = sender as Button;
            uint shortID = uint.Parse(button.DataContext.ToString());
            foreach (CAutoSelectionItem item in autoEvent.ItemList)
            {
                if (item.ShortID == shortID)
                {
                    renameTemp = item;
                    break;
                }
            }

            if (renameTemp == null)
            {
                Message_Feed_Out("エラーが発生しました。ShortIDが見つかりません。");
                return;
            }

            Rename_T.Text = renameTemp.ItemName;
            Rename_T.UndoLimit = 0;
            Rename_T.UndoLimit = 15;
            Rename_Canvas.Opacity = 0;
            Rename_Canvas.Visibility = Visibility.Visible;
            while (Rename_Canvas.Opacity < 1 && !bRenameClosing)
            {
                Rename_Canvas.Opacity += Sub_Code.Window_Feed_Time * 2;
                await Task.Delay(1000 / 60);
            }
        }

        private async void Rename_Cancel_B_Click(object sender, RoutedEventArgs e)
        {
            if (!bRenameClosing)
            {
                bRenameClosing = true;

                while (Rename_Canvas.Opacity > 0)
                {
                    Rename_Canvas.Opacity -= Sub_Code.Window_Feed_Time * 2;
                    await Task.Delay(1000 / 60);
                }

                Rename_T.Text = "";
                bRenameClosing = false;
                Rename_Canvas.Visibility = Visibility.Hidden;
                renameTemp = null;
            }
        }

        private async void Rename_Apply_B_Click(object sender, RoutedEventArgs e)
        {
            if (!bRenameClosing)
            {
                bRenameClosing = true;

                if (Event_List.SelectedIndex == -1)
                    return;

                CAutoSelectionEvent autoEvent = m_eventList[Event_List.SelectedIndex];

                if (Encoding.UTF8.GetBytes(Rename_T.Text).Length >= 255)
                {
                    Message_Feed_Out("キーワードは255文字以下である必要があります。");
                    return;
                }
                foreach (CAutoSelectionItem item in autoEvent.ItemList)
                {
                    if (item.ItemName == Rename_T.Text)
                    {
                        System.Windows.MessageBox.Show("同名のキーワードがリスト内に存在します。別の名前を設定してください。");
                        bRenameClosing = false;
                        return;
                    }
                }

                if (renameTemp.ItemName != Rename_T.Text)
                {
                    renameTemp.ItemName = Rename_T.Text;
                }

                while (Rename_Canvas.Opacity > 0)
                {
                    Rename_Canvas.Opacity -= Sub_Code.Window_Feed_Time * 2;
                    await Task.Delay(1000 / 60);
                }

                Rename_T.Text = "";
                bRenameClosing = false;
                Rename_Canvas.Visibility = Visibility.Hidden;

                renameTemp = null;
            }
        }

        private void Event_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Event_List.SelectedIndex == -1)
            {
                Item_List.ItemsSource = null;
                return;
            }

            Item_List.ItemsSource = ((CAutoSelectionEvent)Event_List.SelectedItem).ItemList;
        }

        private void Add_B_Click(object sender, RoutedEventArgs e)
        {
            if (bClosing || Event_List.SelectedIndex == -1)
                return;

            CAutoSelectionEvent autoEvent = m_eventList[Event_List.SelectedIndex];

            if (autoEvent.ItemList.Count >= 20)
            {
                Message_Feed_Out("検索できるワードは20個までです。");
                return;
            }

            string itemName = "検索ワードを入力してください";
            for (int i = 1; i < int.MaxValue; i++)
            {
                uint id = (uint)Sub_Code.r.Next(0, int.MaxValue);
                bool bExist = false;

                foreach (CAutoSelectionItem item in autoEvent.ItemList)
                {
                    if (item.ShortID == id)
                    {
                        bExist = true;
                        break;
                    }
                }

                if (bExist)
                    continue;

                autoEvent.ItemList.Add(new CAutoSelectionItem(itemName, id));
                autoEvent.UpdateTextColor();

                return;
            }
        }

        private void Remove_B_Click(object sender, RoutedEventArgs e)
        {
            if (bClosing || Event_List.SelectedIndex == -1 || Item_List.SelectedIndex == -1)
                return;

            CAutoSelectionEvent autoEvent = m_eventList[Event_List.SelectedIndex];
            autoEvent.ItemList.RemoveAt(Item_List.SelectedIndex);
            autoEvent.UpdateTextColor();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            FolderMode_C.Source = Sub_Code.Check_01;
            IncludeSubFolder_C.Source = Sub_Code.Check_01;
        }

        bool GetFilesWithDepth(string searchName, string directory, int currentDepth, int maxDepth, List<string> fileList)
        {
            try
            {
                string nameOnly = Path.GetFileNameWithoutExtension(directory);
                if (bFolderMode && string.Equals(nameOnly, searchName, StringComparison.OrdinalIgnoreCase))
                {
                    //ファイルを追加
                    fileList.AddRange(Directory.GetFiles(directory));
                    return true;
                }
                else
                {
                    string[] files = Directory.GetFiles(directory);
                    foreach (string file in files)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(file).ToLower();
                        if (fileName.StartsWith(searchName))
                        {
                            //ファイルを追加
                            fileList.Add(file);
                        }
                    }
                }

                //さらに深い階層に進む
                if (currentDepth < maxDepth && bIncludeSubFolder)
                {
                    foreach (string subDir in Directory.GetDirectories(directory))
                    {
                        bool bStopSearch = GetFilesWithDepth(searchName, subDir, currentDepth + 1, maxDepth, fileList);
                        if (bStopSearch)
                            return true;
                    }
                }
            }
            catch
            {
            }

            return false;
        }

        public List<string> GetFiles(string searchDirectory, CAutoSelectionEvent autoEvent)
        {
            List<string> files = new List<string>();

            int maxDepth = 3;

            foreach (CAutoSelectionItem item in autoEvent.ItemList)
            {
                GetFilesWithDepth(item.ItemName.ToLower(), searchDirectory, 0, maxDepth, files);
            }

            return files;
        }

        void Save()
        {
            string toFilePath = Voice_Set.Special_Path;
            if (bWoTMode)
                toFilePath += WOT_DATA_PATH;
            else 
                toFilePath += WOTB_DATA_PATH;

            if (File.Exists(toFilePath))
                File.Delete(toFilePath);

            BinaryWriter bw = new BinaryWriter(File.OpenWrite(toFilePath));
            bw.Write(VERSION);
            bw.Write(bFolderMode);
            bw.Write(bIncludeSubFolder);
            bw.Write((byte)m_eventList.Count);
            foreach (CAutoSelectionEvent autoEvent in m_eventList)
            {
                bw.Write(autoEvent.ShortID);
                bw.Write((byte)autoEvent.ItemList.Count);
                foreach (CAutoSelectionItem item in autoEvent.ItemList)
                {
                    byte[] itemName = Encoding.UTF8.GetBytes(item.ItemName);
                    bw.Write((byte)itemName.Length);
                    bw.Write(itemName);
                    bw.Write(item.ShortID);
                }
            }

            bw.Close();
        }

        void Load(bool bWoTMode)
        {
            string toFilePath = Voice_Set.Special_Path;
            if (bWoTMode)
                toFilePath += WOT_DATA_PATH;
            else
                toFilePath += WOTB_DATA_PATH;

            if (!File.Exists(toFilePath))
                return;

            BinaryReader br = new BinaryReader(File.OpenRead(toFilePath));
            br.ReadByte();
            bFolderMode = br.ReadBoolean();
            bIncludeSubFolder = br.ReadBoolean();

            FolderMode_C_MouseLeave(null, null);
            IncludeSubFolder_C_MouseLeave(null, null);

            byte eventCount = br.ReadByte();
            for (int i = 0; i < eventCount; i++)
            {
                uint eventID = br.ReadUInt32();
                byte itemCount = br.ReadByte();

                CAutoSelectionEvent autoEvent = null;
                foreach (CAutoSelectionEvent e in m_eventList)
                {
                    if (e.ShortID == eventID)
                    {
                        autoEvent = e;
                        autoEvent.ItemList.Clear();
                        break;
                    }
                }

                for (int j = 0; j < itemCount; j++)
                {
                    string itemName = Encoding.UTF8.GetString(br.ReadBytes(br.ReadByte()));
                    uint itemID = br.ReadUInt32();

                    if (autoEvent != null)
                        autoEvent.ItemList.Add(new CAutoSelectionItem(itemName, itemID));
                }

                if (autoEvent != null)
                    autoEvent.UpdateTextColor();
            }

            br.Close();
        }

        private void Search_Help_B_Click(object sender, RoutedEventArgs e)
        {
            string message = "ファイルまたはフォルダを自動追加する際に使用する検索ワードです。\n";
            message += "例えば、letsbattle_01.mp3, letsbattle_02.mp3...などのファイルを選択したい場合、\"letsbattle\"というキーワードをリストに追加しておくことでこれらのファイルが自動選択されます。";
            MessageBox.Show(message);
        }
    }
}
