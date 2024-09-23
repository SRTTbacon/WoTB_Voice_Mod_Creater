using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WK.Libraries.BetterFolderBrowserNS;

namespace WoTB_Voice_Mod_Creater.Class
{
    //Modの管理ウィンドウ
    public partial class ModManager : UserControl, INotifyPropertyChanged
    {
        enum StateType
        {
            None,
            Enable,
            Disable
        }

        enum WoTB_Analysis
        {
            None,
            Trying,
            Already
        }

        const string MOD_INFO_DATA_PATH = "\\Configs\\Mod_Info.conf";
        const string MOD_INFO_HEADER = "WVMHeader";
        const byte MOD_INFO_VERSION = 0x00;

        List<ObservableCollection<CModTypeList>> modLists = new List<ObservableCollection<CModTypeList>>();
        ObservableCollection<CModTypeList> modList => modLists[modPage];
        //WoTBのファイル構造を保存 (Keyがファイル名でValueがそのファイル名が存在するフォルダのリスト)
        public static Dictionary<string, List<string>> WoTBFiles = new Dictionary<string, List<string>>();

        CModTypeList renameTemp = null;

        WoTB_Analysis loading = WoTB_Analysis.None;

        const int MAXPAGE = 10;

        int modPage = 0;
        int loadInitTime = 0;
        int loadEndTime = 0;

        bool bClosing = false;
        bool bRenameClosing = false;
        bool bAttensionClosing = false;
        bool bMessageShowing = false;
        bool bDVPLMode = true;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public ModManager()
        {
            InitializeComponent();

            ChangeButtonColor(StateType.None);

            string msg = "この機能を使用することで、ほぼすべてのModを簡単に導入することができますが、少数のスキンModやサウンドModでしかテストできていない" +
                "ため、ものによってはうまく導入できない可能性があります。\nModの導入はすべて自己責任でお願いします。このソフトウェアを使用して導入した際" +
                "に起きた、いかなる問題も開発者は責任を負いかねます。\n\nこの機能を用いてModを導入する際、自動的に該当ファイルのバックアップが行われますが、" +
                "バックアップから復元するときは自身で行う必要があります。バックアップファイルは\n" + Directory.GetCurrentDirectory() + "\\Backup\n" +
                "に保存されます。";
            Attension_T.Text = msg;
        }

        public async void Window_Show()
        {
            Opacity = 0;
            Visibility = Visibility.Visible;

            if (loading == WoTB_Analysis.None)
                DVPL_C.Source = Sub_Code.Check_01;

            while (Opacity < 1)
            {
                Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }

            if (loading == WoTB_Analysis.None)
            {
                loading = WoTB_Analysis.Trying;
                Mod_List.ItemsSource = null;
                Message_T.Text = "WoTBのファイル構造を解析しています...\nこの処理はHDDだと時間がかかる場合があります。";
                Loading_Loop();
                GetWoTBFilesAsync();
            }
        }

        async void Loading_Loop()
        {
            while (loading != WoTB_Analysis.Already)
                await Task.Delay(100);

            Message_T.Text = "WoTBのファイル構造を解析しました。処理時間:" + (loadEndTime - loadInitTime) + "ミリ秒\nMod情報を取得しています...";

            loading = WoTB_Analysis.Trying;

            modLists.Clear();

            for (int i = 0; i < MAXPAGE; i++)
                modLists.Add(new ObservableCollection<CModTypeList>());

            Task task = Task.Run(() =>
            {
                foreach (string dir in Directory.GetDirectories(Directory.GetCurrentDirectory() + "\\Projects"))
                {
                    foreach (string file in Directory.GetFiles(dir))
                    {
                        if (Path.GetFileName(file).Contains(".bnk"))
                        {
                            modLists[0].Add(new CModTypeList(Path.GetFileName(dir), false));
                            modLists[0][modLists[0].Count - 1].AddDirectory(dir, false, false);
                            break;
                        }
                    }
                }
                LoadModList();
                loading = WoTB_Analysis.Already;
            });

            while (loading != WoTB_Analysis.Already)
                await Task.Delay(100);

            foreach (ObservableCollection<CModTypeList> mods in modLists)
            {
                for (int i = 0; i < mods.Count; i++)
                {
                    if (!mods[i].IsCanDelete && mods[i].fileList.Count <= 0)
                    {
                        mods.RemoveAt(i);
                        i--;
                    }
                }
            }

            Mod_List.ItemsSource = modList;

            if (bDVPLMode)
                DVPL_C.Source = Sub_Code.Check_03;
            else
                DVPL_C.Source = Sub_Code.Check_01;

            Message_Feed_Out("ロードしました。");
        }

        async void Message_Feed_Out(string Message)
        {
            //テキストが一定期間経ったらフェードアウト
            if (bMessageShowing)
            {
                bMessageShowing = false;
                await Task.Delay(30);
            }
            Message_T.Text = Message;
            bMessageShowing = true;
            Message_T.Opacity = 1;
            int initTickTime = Environment.TickCount;
            while (Message_T.Opacity > 0 && bMessageShowing)
            {
                if (Environment.TickCount - initTickTime > 4000)
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

        private async void Exit_B_Click(object sender, RoutedEventArgs e)
        {
            //閉じる
            if (!bClosing)
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

        private async void Rename_B_Click(object sender, RoutedEventArgs e)
        {
            if (bClosing || loading != WoTB_Analysis.Already)
                return;

            Button button = sender as Button;
            uint modID = uint.Parse(button.DataContext.ToString());
            foreach (CModTypeList mod in modList)
            {
                if (mod.ModID == modID)
                {
                    renameTemp = mod;
                    break;
                }
            }

            if (renameTemp == null)
            {
                Message_Feed_Out("エラーが発生しました。ModIDが見つかりません。");
                return;
            }

            if (!renameTemp.IsCanDelete)
            {
                Message_Feed_Out("Mod Creatorで作成した音声Modは命名変更できません。\n/Projects内のフォルダ名を変更してください。");
                return;
            }

            Rename_T.Text = renameTemp.ModName;
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
                if (Encoding.UTF8.GetBytes(Rename_T.Text).Length >= 255)
                {
                    Message_Feed_Out("Mod名は255文字以下である必要があります。");
                    return;
                }
                foreach (CModTypeList mod in modList)
                {
                    if (mod.ModName == Rename_T.Text)
                    {
                        System.Windows.MessageBox.Show("同名のModがリスト内に存在します。別の名前を設定してください。");
                        bRenameClosing = false;
                        return;
                    }
                }

                bool bSave = false;
                if (renameTemp.ModName != Rename_T.Text)
                {
                    renameTemp.ModName = Rename_T.Text;
                    SortModList();
                    bSave = true;
                }

                while (Rename_Canvas.Opacity > 0)
                {
                    Rename_Canvas.Opacity -= Sub_Code.Window_Feed_Time * 2;
                    await Task.Delay(1000 / 60);
                }

                Rename_T.Text = "";
                bRenameClosing = false;
                Rename_Canvas.Visibility = Visibility.Hidden;


                if (bSave)
                    SaveModList();

                renameTemp = null;
            }
        }

        public void SaveModList()
        {
            try
            {
                if (File.Exists(Voice_Set.Special_Path + MOD_INFO_DATA_PATH + ".tmp"))
                    File.Delete(Voice_Set.Special_Path + MOD_INFO_DATA_PATH + ".tmp");
                BinaryWriter bw = new BinaryWriter(File.OpenWrite(Voice_Set.Special_Path + MOD_INFO_DATA_PATH + ".tmp"));
                byte[] header = Encoding.ASCII.GetBytes(MOD_INFO_HEADER);
                bw.Write((byte)header.Length);
                bw.Write(header);
                bw.Write(MOD_INFO_VERSION);
                bw.Write(bDVPLMode);
                bw.Write((byte)modPage);
                bw.Write((byte)modLists.Count);
                foreach (ObservableCollection<CModTypeList> types in modLists)
                {
                    bw.Write((byte)types.Count);
                    foreach (CModTypeList type in types)
                    {
                        bool bSkip = true;
                        foreach (CModFileList file in type.fileList)
                        {
                            if (file.IsCanDelete)
                            {
                                bSkip = false;
                                break;
                            }
                        }
                        bw.Write(bSkip);
                        if (bSkip)
                            continue;
                        byte[] modName = Encoding.UTF8.GetBytes(type.ModName);
                        bw.Write((byte)modName.Length);
                        bw.Write(modName);
                        bw.Write(type.ModID);
                        bw.Write(type.IsCanDelete);
                        bw.Write((ushort)type.fileList.Count);
                        foreach (CModFileList file in type.fileList)
                        {
                            bSkip = !file.IsCanDelete || (file.Parent != null && file.IsZipFile);
                            bw.Write(bSkip);
                            if (bSkip)
                                continue;
                            bw.Write(file.Parent == null);
                            bw.Write(file.IsDirectory);
                            bw.Write(file.IsZipFile);
                            bw.Write(file.IsAnyDir);
                            bw.Write(file.IsChildEnable);
                            bw.Write((short)file.WoTBDirIndex);
                            bw.Write(file.FileID);
                            byte[] filePath = Encoding.UTF8.GetBytes(file.FilePath);
                            bw.Write((byte)filePath.Length);
                            bw.Write(filePath);
                            byte[] userDir = Encoding.UTF8.GetBytes(file.UserDir);
                            bw.Write((byte)userDir.Length);
                            bw.Write(userDir);
                            if (file.IsZipFile)
                            {
                                bw.Write((ushort)file.ChildCount);
                                int firstIndex = type.fileList.IndexOf(file) + 1;
                                for (int i = firstIndex; i < firstIndex + file.ChildCount; i++)
                                {
                                    byte[] childFilePath = Encoding.UTF8.GetBytes(type.fileList[i].FilePath);
                                    bw.Write((byte)childFilePath.Length);
                                    bw.Write(childFilePath);
                                    bw.Write(type.fileList[i].IsChildEnable);
                                    byte[] zipUserDir = Encoding.UTF8.GetBytes(type.fileList[i].UserDir);
                                    bw.Write((byte)zipUserDir.Length);
                                    bw.Write(zipUserDir);
                                }
                            }
                        }
                        bw.Write((byte)0x0a);
                    }
                }
                bw.Close();
                Sub_Code.File_Move(Voice_Set.Special_Path + MOD_INFO_DATA_PATH + ".tmp", Voice_Set.Special_Path + MOD_INFO_DATA_PATH, true);
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                Message_Feed_Out("データ保存中にエラーが発生しました。\n" + e.Message);
            }
        }

        void LoadModList()
        {
            if (!File.Exists(Voice_Set.Special_Path + MOD_INFO_DATA_PATH))
                return;
            BinaryReader br = null;
            try
            {
                br = new BinaryReader(File.OpenRead(Voice_Set.Special_Path + MOD_INFO_DATA_PATH));
                string header = Encoding.ASCII.GetString(br.ReadBytes(br.ReadByte()));
                if (header != MOD_INFO_HEADER)
                {
                    br.Close();
                    Dispatcher.Invoke(() =>
                    {
                        Message_Feed_Out("Mod情報をロード中にエラーが発生しました。\nヘッダーが間違っています。");
                    });
                    return;
                }

                _ = br.ReadByte();
                bDVPLMode = br.ReadBoolean();
                modPage = br.ReadByte();
                byte modListCount = br.ReadByte();
                for (int i = 0; i < modListCount; i++)
                {
                    ObservableCollection<CModTypeList> nowModList;
                    if (modLists.Count <= i)
                    {
                        nowModList = new ObservableCollection<CModTypeList>();
                        modLists.Add(nowModList);
                    }
                    else
                        nowModList = modLists[i];

                    byte typeCount = br.ReadByte();
                    for (int j = 0; j < typeCount; j++)
                    {
                        bool bSkip = br.ReadBoolean();
                        if (bSkip)
                            continue;
                        string modName = Encoding.UTF8.GetString(br.ReadBytes(br.ReadByte()));
                        uint modID = br.ReadUInt32();
                        bool bCanDelete = br.ReadBoolean();
                        CModTypeList newType = null;
                        CModFileList nowParent = null;
                        if (bCanDelete)
                        {
                            newType = new CModTypeList(modName, bCanDelete);
                            newType.ModID = modID;
                            nowModList.Add(newType);
                        }
                        else
                        {
                            newType = nowModList[j];
                            if (newType.fileList.Count > 0)
                                nowParent = newType.fileList[0];
                        }
                        ushort fileCount = br.ReadUInt16();
                        for (int k = 0; k < fileCount; k++)
                        {
                            bSkip = br.ReadBoolean();
                            if (bSkip)
                                continue;
                            bool bParent = br.ReadBoolean();
                            bool bIsDirectory = br.ReadBoolean();
                            bool bIsZipFile = br.ReadBoolean();
                            bool bIsAnyDir = br.ReadBoolean();
                            bool bIsChildEnable = br.ReadBoolean();
                            short wotbDirIndex = br.ReadInt16();
                            uint fileID = br.ReadUInt32();
                            string filePath = Encoding.UTF8.GetString(br.ReadBytes(br.ReadByte()));
                            string userDir = Encoding.UTF8.GetString((br.ReadBytes(br.ReadByte())));
                            bool bCanAddFile = !bIsZipFile || File.Exists(filePath);
                            if (!bIsZipFile)
                            {
                                if (!bParent && nowParent == null)
                                    continue;
                                CModFileList parent = bParent ? null : nowParent;
                                CModFileList newFile = new CModFileList(bIsDirectory, bIsZipFile, parent, filePath);
                                newType.fileList.Add(newFile);
                                if (bParent)
                                    nowParent = newFile;
                                else
                                    nowParent.ChildCount++;
                                newFile.IsAnyDir = bIsAnyDir;
                                newFile.IsChildEnable = bIsChildEnable;
                                newFile.WoTBDirIndex = wotbDirIndex;
                                newFile.UserDir = userDir;
                                newFile.FileID = fileID;
                            }
                            if (bIsZipFile)
                            {
                                if (bCanAddFile)
                                    newType.AddZip(filePath);
                                int zipParentIndex = newType.GetZipIndex(filePath);
                                ushort zipFileCount = br.ReadUInt16();
                                for (int l = 0; l < zipFileCount; l++)
                                {
                                    string childFilePath = Encoding.UTF8.GetString(br.ReadBytes(br.ReadByte()));
                                    bool bZipChildEnable = br.ReadBoolean();
                                    string zipUserDir = Encoding.UTF8.GetString(br.ReadBytes(br.ReadByte()));
                                    if (bCanAddFile)
                                    {
                                        CModFileList zipParent = newType.fileList[zipParentIndex];
                                        for (int m = zipParentIndex + 1; m < zipParentIndex + zipParent.ChildCount + 1; m++)
                                        {
                                            CModFileList zipChild = newType.fileList[m];
                                            if (zipChild.FilePath == childFilePath)
                                            {
                                                zipChild.IsChildEnable = bZipChildEnable;
                                                zipChild.UserDir = zipUserDir;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        _ = br.ReadByte();
                    }
                }
                br.Close();
            }
            catch (Exception e)
            {
                if (br != null)
                    br.Close();
#if DEBUG
                StackFrame[] stackFrames = new StackTrace(e, true).GetFrames();
                foreach (StackFrame frame in stackFrames)
                {
                    Sub_Code.Error_Log_Write("該当ソース:" + frame.GetFileName() + "->" + frame.GetFileLineNumber() + "行目");
                }
                Sub_Code.Error_Log_Write(e.StackTrace);
#else
                Sub_Code.Error_Log_Write(e.Message);
#endif
                Dispatcher.Invoke(() =>
                {
                    Message_Feed_Out("Mod情報ロード中にエラーが発生しました。\n" + e.Message);
                });
            }
        }

        void SortModList()
        {
            uint selectedModID = 0;
            if (Mod_List.SelectedIndex != -1)
                selectedModID = modList[Mod_List.SelectedIndex].ModID;

            CModTypeList[] sortedMods = modList.OrderBy(h => h.ModName).ToArray();
            for (int i = 0; i < sortedMods.Count(); i++)
            {
                modList.RemoveAt(0);
                modList.Add(sortedMods[i]);
            }

            if (selectedModID != 0)
            {
                for (int i = 0; i < modList.Count; i++)
                {
                    if (modList[i].ModID == selectedModID)
                    {
                        Mod_List.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void Add_Mod_B_Click(object sender, RoutedEventArgs e)
        {
            if (bClosing || loading != WoTB_Analysis.Already)
                return;

            if (modList.Count >= 255)
            {
                Message_Feed_Out("1ページに入れれるModは255個までです。");
                return;
            }

            string modName = "新しいMod";
            for (int i = 1; i < int.MaxValue; i++)
            {
                string newModName = modName + i;
                uint hash = WwiseHash.HashString(newModName);
                bool bExist = false;

                foreach (CModTypeList mod in modList)
                {
                    if (mod.ModID == hash)
                    {
                        bExist = true;
                        break;
                    }
                }

                if (bExist)
                    continue;

                modList.Add(new CModTypeList(newModName));

                SortModList();
                SaveModList();

                return;
            }
        }

        private void Delete_Mod_B_Click(object sender, RoutedEventArgs e)
        {
            if (Mod_List.SelectedIndex == -1 || bClosing || loading != WoTB_Analysis.Already)
                return;

            if (!modList[Mod_List.SelectedIndex].IsCanDelete)
            {
                Message_Feed_Out("Mod Creatorで作成した音声Modはリストから削除できません。");
                return;
            }

            MessageBoxResult result = MessageBox.Show("'" + modList[Mod_List.SelectedIndex].ModName + "'をリストから削除しますか?", "確認",
                MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);

            if (result != MessageBoxResult.Yes)
                return;

            string modName = modList[Mod_List.SelectedIndex].ModName;

            modList.RemoveAt(Mod_List.SelectedIndex);
            SaveModList();

            Message_Feed_Out("'" + modName + "'を削除しました。");
        }

        private void NextPage_B_Click(object sender, RoutedEventArgs e)
        {
            if (bClosing || loading != WoTB_Analysis.Already)
                return;

            if (modPage < MAXPAGE - 1)
            {
                modPage++;
                Mod_List_T.Text = "Modリスト" + (modPage + 1);
                Mod_List.ItemsSource = modList;
                File_List.ItemsSource = null;
            }
        }

        private void BackPage_B_Click(object sender, RoutedEventArgs e)
        {
            if (bClosing)
                return;

            if (modPage > 0)
            {
                modPage--;
                Mod_List_T.Text = "Modリスト" + (modPage + 1);
                Mod_List.ItemsSource = modList;
                File_List.ItemsSource = null;
            }
        }

        private void Mod_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Mod_List.SelectedIndex == -1)
            {
                File_List.ItemsSource = null;
                return;
            }

            File_List.ItemsSource = ((CModTypeList)Mod_List.SelectedItem).fileList;
        }

        private void File_Enable_B_Click(object sender, RoutedEventArgs e)
        {
            if (bClosing || loading != WoTB_Analysis.Already)
                return;

            if (File_List.SelectedIndex == -1 || Mod_List.SelectedIndex == -1)
                return;
            CModFileList file = File_List.SelectedItem as CModFileList;
            file.IsChildEnable = true;
            if (file.IsDirectory || file.IsZipFile)
            {
                CModTypeList type = Mod_List.SelectedItem as CModTypeList;
                for (int i = File_List.SelectedIndex + 1; i < File_List.SelectedIndex + file.ChildCount + 1; i++)
                    type.fileList[i].UpdateColor();
            }
            ChangeButtonColor(file.IsChildEnable ? StateType.Disable : StateType.Enable);

            SaveModList();
        }

        private void File_Disable_B_Click(object sender, RoutedEventArgs e)
        {
            if (bClosing || loading != WoTB_Analysis.Already)
                return;

            if (File_List.SelectedIndex == -1 || Mod_List.SelectedIndex == -1)
                return;
            CModFileList file = File_List.SelectedItem as CModFileList;
            file.IsChildEnable = false;
            if (file.IsDirectory || file.IsZipFile)
            {
                CModTypeList type = Mod_List.SelectedItem as CModTypeList;
                for (int i = File_List.SelectedIndex + 1; i < File_List.SelectedIndex + file.ChildCount + 1; i++)
                    type.fileList[i].UpdateColor();
            }
            ChangeButtonColor(file.IsChildEnable ? StateType.Disable : StateType.Enable);

            SaveModList();
        }

        private void Delete_File_B_Click(object sender, RoutedEventArgs e)
        {
            if (bClosing || loading != WoTB_Analysis.Already)
                return;

            if (File_List.SelectedIndex == -1 || Mod_List.SelectedIndex == -1)
            {
                Message_Feed_Out("ファイルが選択されていません。");
                return;
            }

            CModFileList file = File_List.SelectedItem as CModFileList;
            if (file.Parent != null && file.Parent.IsZipFile)
            {
                Message_Feed_Out("Zip内のファイルは削除できません。");
                return;
            }

            CModTypeList type = Mod_List.SelectedItem as CModTypeList;

            if (!file.IsCanDelete)
            {
                Message_Feed_Out("Mod Creatorで作成した音声Modのファイルは削除できません。");
                return;
            }

            if (file.Parent != null)
            {
                MessageBoxResult result = MessageBox.Show("リストからファイル'" + file.FilePath + "'を削除しますか?", "確認",
                    MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
                if (result != MessageBoxResult.Yes)
                    return;
                file.Parent.ChildCount--;
                type.fileList.RemoveAt(File_List.SelectedIndex);
                if (file.Parent.ChildCount == 0)
                    type.fileList.Remove(file);
            }
            else
            {
                if (file.ChildCount > 0)
                {
                    MessageBoxResult result = MessageBoxResult.Yes;
                    if (file.IsZipFile)
                    {
                        result = MessageBox.Show("リストからZipファイル'" + file.FilePath + "'を削除しますか?\n同時に" + file.ChildCount +
                            "個の子ファイルも削除されます。", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
                    }
                    else if (file.IsDirectory)
                    {
                        result = MessageBox.Show("リストからフォルダ'" + file.FilePath + "'を削除しますか?\n同時に" + file.ChildCount +
                            "個の子ファイルも削除されます。", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
                    }

                    if (result != MessageBoxResult.Yes)
                        return;

                    for (int i = 0; i < file.ChildCount; i++)
                        type.fileList.RemoveAt(File_List.SelectedIndex + 1);
                }
                type.fileList.RemoveAt(File_List.SelectedIndex);
            }

            SaveModList();
        }

        private void Add_File_B_Click(object sender, RoutedEventArgs e)
        {
            if (bClosing || loading != WoTB_Analysis.Already)
                return;

            if (Mod_List.SelectedIndex == -1)
            {
                Message_Feed_Out("Modリストが選択されていません。");
                return;
            }

            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog()
            {
                Title = "Modファイルを選択してください。",
                Multiselect = true,
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                CModTypeList type = Mod_List.SelectedItem as CModTypeList;

                int addedCount = 0;
                foreach (string file in ofd.FileNames)
                {
                    //エラー回避
                    if (file.Contains(Voice_Set.WoTB_Path.Replace("/", "\\")))
                    {
                        Message_Feed_Out("パスにWoTBのインストール先が含まれています。\nエラー回避のため該当ファイルを別のフォルダに移動してください。");
                        ofd.Dispose();
                        return;
                    }

                    if (type.AddFile(file))
                        addedCount++;
                }

                if (addedCount > 0)
                {
                    SaveModList();
                    Message_Feed_Out(addedCount + "個のファイルを追加しました。");
                }
                else
                    Message_Feed_Out("既にリスト内に存在するため追加できませんでした。");

                if (type.IsMaxSize())
                    Message_Feed_Out("ファイル追加中にエラーが発生しました。\n1つのModに追加できるファイルは" + CModTypeList.MAXCOUNT + "個までです。");
            }
            ofd.Dispose();
        }

        private void Add_Folder_B_Click(object sender, RoutedEventArgs e)
        {
            if (bClosing || loading != WoTB_Analysis.Already)
                return;

            if (Mod_List.SelectedIndex == -1)
            {
                Message_Feed_Out("Modリストが選択されていません。");
                return;
            }

            BetterFolderBrowser bfb = new BetterFolderBrowser()
            {
                Title = "Modファイルが入っているフォルダを選択してください。",
                RootFolder = Sub_Code.Get_OpenDirectory_Path(),
                Multiselect = true
            };
            if (bfb.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Sub_Code.Set_Directory_Path(bfb.SelectedFolder);
                CModTypeList type = Mod_List.SelectedItem as CModTypeList;
                int addedCount = 0;
                foreach (string dir in bfb.SelectedFolders)
                    addedCount += type.AddDirectory(dir, false);

                SaveModList();

                Message_Feed_Out(addedCount + "個のファイルを追加しました。");

                if (type.IsMaxSize())
                    Message_Feed_Out("ファイル追加中にエラーが発生しました。\n1つのModに追加できるファイルは" + CModTypeList.MAXCOUNT + "個までです。");
            }
            bfb.Dispose();
        }

        private void Add_Folder_EX_B_Click(object sender, RoutedEventArgs e)
        {
            if (bClosing || loading != WoTB_Analysis.Already)
                return;

            if (Mod_List.SelectedIndex == -1)
            {
                Message_Feed_Out("Modリストが選択されていません。");
                return;
            }

            BetterFolderBrowser bfb = new BetterFolderBrowser()
            {
                Title = "Modファイルが入っているフォルダを選択してください。",
                RootFolder = Sub_Code.Get_OpenDirectory_Path(),
                Multiselect = true
            };
            if (bfb.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Sub_Code.Set_Directory_Path(bfb.SelectedFolder);
                CModTypeList type = Mod_List.SelectedItem as CModTypeList;
                int addedCount = 0;
                foreach (string dir in bfb.SelectedFolders)
                    addedCount += type.AddDirectory(dir, true);

                SaveModList();

                if (addedCount <= 0)
                    Message_Feed_Out("追加できるファイルが1つもありませんでした。");
                else
                    Message_Feed_Out(addedCount + "個のファイルを追加しました。");

                if (type.IsMaxSize())
                    Message_Feed_Out("ファイル追加中にエラーが発生しました。\n1つのModに追加できるファイルは" + CModTypeList.MAXCOUNT + "個までです。");
            }
            bfb.Dispose();
        }

        private void Add_Zip_B_Click(object sender, RoutedEventArgs e)
        {
            if (bClosing || loading != WoTB_Analysis.Already)
                return;

            if (Mod_List.SelectedIndex == -1)
            {
                Message_Feed_Out("Modリストが選択されていません。");
                return;
            }

            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog()
            {
                Title = "Modが入っているzipファイルを選択してください。",
                Filter = "Zipファイル(*.zip)|*.zip",
                Multiselect = false
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                CModTypeList type = Mod_List.SelectedItem as CModTypeList;
                type.AddZip(ofd.FileName);

                SaveModList();

                if (type.IsMaxSize())
                    Message_Feed_Out("ファイル追加中にエラーが発生しました。\n1つのModに追加できるファイルは" + CModTypeList.MAXCOUNT + "個までです。");
            }
            ofd.Dispose();
        }

        private void File_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (File_List.SelectedIndex == -1 || Mod_List.SelectedIndex == -1)
            {
                ChangeButtonColor(StateType.None);
                return;
            }

            CModFileList file = File_List.SelectedItem as CModFileList;
            ChangeButtonColor(file.IsChildEnable ? StateType.Disable : StateType.Enable);
        }

        void ChangeButtonColor(StateType state)
        {
            File_Enable_B.BorderBrush = Brushes.Gray;
            File_Enable_B.Foreground = Brushes.Gray;
            File_Disable_B.BorderBrush = Brushes.Gray;
            File_Disable_B.Foreground = Brushes.Gray;

            if (state == StateType.Enable)
            {
                File_Enable_B.BorderBrush = Brushes.Aqua;
                File_Enable_B.Foreground = Brushes.Aqua;
            }
            else if (state == StateType.Disable)
            {
                File_Disable_B.BorderBrush = Brushes.Aqua;
                File_Disable_B.Foreground = Brushes.Aqua;
            }
        }

        void GetWoTBFilesAsync()
        {
            Task.Run(() =>
            {
                loadInitTime = Environment.TickCount;
                WoTBFiles.Clear();
                GetFiles(Voice_Set.WoTB_Path);
                loadEndTime = Environment.TickCount;
                loading = WoTB_Analysis.Already;
            });
        }
        void GetFiles(string dir)
        {
            foreach (string nowDir in Directory.GetDirectories(dir))
                GetFiles(nowDir);
            foreach (string file in Directory.GetFiles(dir))
            {
                string fileName = System.IO.Path.GetFileName(file).Replace(".dvpl", "");
                string newWoTBDir = dir.Replace(Voice_Set.WoTB_Path, "").Replace("\\", "/");
                if (WoTBFiles.ContainsKey(fileName))
                {
                    bool bExist = false;
                    foreach (string wotbDir in WoTBFiles[fileName])
                    {
                        if (wotbDir == newWoTBDir)
                        {
                            bExist = true;
                            break;
                        }
                    }
                    if (!bExist)
                        WoTBFiles[fileName].Add(newWoTBDir);
                }
                else
                {
                    WoTBFiles.Add(fileName, new List<string>());
                    WoTBFiles[fileName].Add(newWoTBDir);
                }
            }
        }

        private void Apply_WoTB_B_Click(object sender, RoutedEventArgs e)
        {
            if (bClosing || loading != WoTB_Analysis.Already)
                return;

            if (Mod_List.SelectedIndex == -1)
            {
                Message_Feed_Out("導入するModが選択されていません。");
                return;
            }

            CModTypeList type = Mod_List.SelectedItem as CModTypeList;

            if (type.fileList.Count == 0)
            {
                Message_Feed_Out("ファイルが1つも追加されていません。");
                return;
            }

            //同じ導入先かつ同じ名前のファイルが存在するか確認
            for (int i = 0; i < type.fileList.Count; i++)
            {
                if (type.fileList[i].Parent == null || !type.fileList[i].IsChildEnable || !type.fileList[i].Parent.IsChildEnable)
                    continue;

                //の前に導入先がちゃんと設定されているか確認
                if (type.fileList[i].IsAnyDir && type.fileList[i].WoTBDirIndex == -1 && type.fileList[i].UserDir == "")
                {
                    Message_Feed_Out("ファイル:'" + type.fileList[i].FilePath + "'の導入先が見つかりません。手動で設定する必要があります。");
                    return;
                }

                //ファイルが存在するか確認
                if (type.fileList[i].Parent.IsZipFile && !File.Exists(type.fileList[i].Parent.FilePath))
                {
                    Message_Feed_Out("Zipファイル:'" + type.fileList[i].Parent.FilePath + "'が見つかりませんでした。削除されたか、移動された可能性があります。");
                    return;
                }
                else if (!type.fileList[i].Parent.IsZipFile)
                {
                    string filePas = "";
                    if (type.fileList[i].FilePath.Contains("/"))
                        filePas = type.fileList[i].Parent.FilePath + type.fileList[i].FilePath.Replace("/", "\\");
                    else
                        filePas = type.fileList[i].Parent.FilePath + "\\" + type.fileList[i].FilePath;
                    if (!File.Exists(filePas))
                    {
                        string parentDirName = Path.GetFileName(type.fileList[i].Parent.FilePath);
                        Message_Feed_Out("親フォルダ:'" + parentDirName + "'内のファイル:'" + type.fileList[i].FilePath + "'が見つかりませんでした。");
                        return;
                    }
                }


                string iDestDir;
                if (type.fileList[i].UserDir != "")
                    iDestDir = type.fileList[i].UserDir;
                else if (type.fileList[i].WoTBDirIndex != -1)
                    iDestDir = ModManager.WoTBFiles[type.fileList[i].NameOnly][type.fileList[i].WoTBDirIndex];
                else
                    iDestDir = ModManager.WoTBFiles[type.fileList[i].NameOnly][0];

                for (int j = i + 1; j < type.fileList.Count; j++)
                {
                    if (type.fileList[j].Parent == null || !type.fileList[j].IsChildEnable)
                        continue;

                    string jDestDir;
                    if (type.fileList[j].UserDir != "")
                        jDestDir = type.fileList[j].UserDir;
                    else if (type.fileList[j].WoTBDirIndex != -1)
                        jDestDir = ModManager.WoTBFiles[type.fileList[j].NameOnly][type.fileList[j].WoTBDirIndex];
                    else
                        jDestDir = ModManager.WoTBFiles[type.fileList[j].NameOnly][0];
                    if (type.fileList[i].NameOnly == type.fileList[j].NameOnly && iDestDir == jDestDir)
                    {
                        string message = "ファイル名'" + type.fileList[i].FilePath + "'と'" + type.fileList[j].FilePath + "'の導入先が同じです。\n";
                        message += "どちらかの導入先を変更するか、無効にする必要があります。";
                        Message_Feed_Out(message);
                        return;
                    }
                }
            }

            List<ModFromTo> fromToFiles = new List<ModFromTo>();
            for (int i = 0; i < type.fileList.Count; i++)
            {
                if (type.fileList[i].Parent != null || !type.fileList[i].IsChildEnable)
                    continue;
                if (type.fileList[i].IsZipFile)
                {
                    using (ZipArchive archive = ZipFile.OpenRead(type.fileList[i].FilePath))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            if (!entry.FullName.EndsWith("/"))
                            {
                                for (int j = i + 1; j < i + type.fileList[i].ChildCount + 1; j++)
                                {
                                    if (type.fileList[j].IsChildEnable && entry.FullName == type.fileList[j].FilePath)
                                    {
                                        string tempFile = Voice_Set.Special_Path + "\\Other\\" + Sub_Code.r.Next(100000, 1000000) + ".dat";
                                        string toFile = type.fileList[j].GetDirectoryORFile(true);
                                        entry.ExtractToFile(tempFile, true);
                                        fromToFiles.Add(new ModFromTo(tempFile, toFile, true));
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    for (int j = i + 1; j < i + type.fileList[i].ChildCount + 1; j++)
                    {
                        if (!type.fileList[j].IsChildEnable)
                            continue;
                        string fromFile = type.fileList[j].GetDirectoryORFile(false);
                        string toFile = type.fileList[j].GetDirectoryORFile(true);
                        fromToFiles.Add(new ModFromTo(fromFile, toFile, false));
                    }
                }
            }

            int count = 0;
            int errorCount = 0;

            string backupDir = Directory.GetCurrentDirectory() + "\\Backup";

            foreach (ModFromTo mod in fromToFiles)
            {
                bool bExistFile = File.Exists(mod.FromFile);
                try
                {
                    if (bExistFile)
                    {
                        if (File.Exists(mod.ToFile) && mod.ToFile.EndsWith(".dvpl"))
                        {
                            string path = backupDir + mod.ToFile.Replace(Voice_Set.WoTB_Path, "");
                            if (!File.Exists(path))
                            {
                                string pathDir = Path.GetDirectoryName(path);
                                if (!Directory.Exists(pathDir))
                                    Directory.CreateDirectory(pathDir);
                                File.Copy(mod.ToFile, path, true);
                            }
                        }

                        if (!bDVPLMode && !mod.FromFile.EndsWith(".dvpl") && mod.ToFile.EndsWith(".dvpl"))
                        {
                            if (File.Exists(mod.ToFile))
                                File.Delete(mod.ToFile);
                            mod.ToFile.Replace(".dvpl", "");
                        }

                        if (bDVPLMode && !mod.ToFile.EndsWith(".dvpl"))
                            mod.ToFile += ".dvpl";

                        if (bDVPLMode && !mod.FromFile.EndsWith(".dvpl"))
                            DVPL.DVPL_Pack(mod.FromFile, mod.ToFile, false);
                        else
                            File.Copy(mod.FromFile, mod.ToFile, true);
                        count++;
                    }
                    if (mod.IsDeleteMode && bExistFile)
                        File.Delete(mod.FromFile);
                }
                catch
                {
                    errorCount++;
                }
            }

            string message2 = count + "個のファイルをWoTBにコピーしました。";
            if (errorCount > 0)
                message2 += "\n" + errorCount + "個のファイルがコピー中にエラーが発生しました。";
            Message_Feed_Out(message2);
        }

        private void AnyDir_B_Click(object sender, RoutedEventArgs e)
        {
            if (bClosing || Mod_List.SelectedIndex == -1 || loading != WoTB_Analysis.Already)
                return;

            Button button = sender as Button;

            CModTypeList modType = Mod_List.SelectedItem as CModTypeList;

            CModFileList modFile = null;

            uint fileID = uint.Parse(button.Tag.ToString());
            foreach (CModFileList file in modType.fileList)
            {
                if (file.FileID == fileID)
                {
                    modFile = file;
                    break;
                }
            }

            if (modFile == null)
            {
                Message_Feed_Out("エラーが発生しました。FileIDが見つかりません。");
                return;
            }

            ModManager_Dest_Window.Window_Show(this, modType, modFile);
        }

        private async void Attension_Back_B_Click(object sender, RoutedEventArgs e)
        {
            if (!bAttensionClosing)
            {
                bAttensionClosing = true;
                while (Attension_Canvas.Opacity > 0.0)
                {
                    Attension_Canvas.Opacity -= Sub_Code.Window_Feed_Time * 2;
                    await Task.Delay(1000 / 60);
                }
                Attension_Canvas.Visibility = Visibility.Hidden;
                bAttensionClosing = false;
            }
        }

        private async void Attension_B_Click(object sender, RoutedEventArgs e)
        {
            if (bClosing || loading != WoTB_Analysis.Already)
                return;

            Attension_Canvas.Visibility = Visibility.Visible;
            Attension_Canvas.Opacity = 0.0;
            while (Attension_Canvas.Opacity < 1.0 && !bAttensionClosing)
            {
                Attension_Canvas.Opacity += Sub_Code.Window_Feed_Time * 2;
                await Task.Delay(1000 / 60);
            }
        }

        private void DVPL_C_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (bDVPLMode)
            {
                bDVPLMode = false;
                DVPL_C.Source = Sub_Code.Check_02;
            }
            else
            {
                bDVPLMode = true;
                DVPL_C.Source = Sub_Code.Check_04;
            }
            SaveModList();
        }
        private void DVPL_C_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (bDVPLMode)
                DVPL_C.Source = Sub_Code.Check_04;
            else
                DVPL_C.Source = Sub_Code.Check_02;
        }
        private void DVPL_C_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (bDVPLMode)
                DVPL_C.Source = Sub_Code.Check_03;
            else
                DVPL_C.Source = Sub_Code.Check_01;
        }
    }
}
