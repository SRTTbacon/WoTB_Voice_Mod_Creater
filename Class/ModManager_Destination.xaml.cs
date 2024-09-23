using System.Windows;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using WK.Libraries.BetterFolderBrowserNS;
using System;

namespace WoTB_Voice_Mod_Creater.Class
{
    public partial class ModManager_Destination : UserControl
    {
        public List<string> UserDirs = new List<string>();
        ObservableCollection<CModDestList> dirs = new ObservableCollection<CModDestList>();
        List<string> wotbDirs = null;

        CModTypeList type = null;
        CModFileList file = null;

        ModManager modManager = null;

        bool bMessageShowing = false;
        bool bClosing = false;

        public ModManager_Destination()
        {
            InitializeComponent();
            Dest_Dir_List.ItemsSource = dirs;
        }

        public async void Window_Show(ModManager modManager, CModTypeList type, CModFileList file)
        {
            Opacity = 0;
            Visibility = Visibility.Visible;

            this.modManager = modManager;

            this.type = type;
            this.file = file;

            dirs.Clear();
            if (ModManager.WoTBFiles.ContainsKey(file.NameOnly))
            {
                wotbDirs = ModManager.WoTBFiles[file.NameOnly];
                for (int i = 0; i < wotbDirs.Count; i++)
                    dirs.Add(new CModDestList(wotbDirs[i], dirs.Count));
            }
            for (int i = 0; i < UserDirs.Count; i++)
            {
                CModDestList newDest = new CModDestList(UserDirs[i], dirs.Count);
                newDest.IsUserAddDir = true;
                dirs.Add(newDest);
            }

            bool bExist = false;
            if (file.UserDir != "")
            {
                for (int i = 0; i < dirs.Count; i++)
                {
                    if (dirs[i].DestDir == file.UserDir)
                    {
                        Dest_Dir_List.SelectedIndex = i;
                        bExist = true;
                        break;
                    }
                }
            }
            if (!bExist && Dest_Dir_List.Items.Count > file.WoTBDirIndex)
                Dest_Dir_List.SelectedIndex = file.WoTBDirIndex;
            if (Dest_Dir_List.SelectedIndex == -1)
                Selected_Index_T.Text = "選択しているインデックス:なし";

            if (file.WoTBDirIndex == -1)
                Default_Index_T.Text = "デフォルトのインデックス:なし (選択してください)";
            else
                Default_Index_T.Text = "デフォルトのインデックス:" + file.WoTBDirIndex;

            Mod_Type_File_T.Text = type.ModName + " -> " + file.FilePath;

            while (Opacity < 1)
            {
                Opacity += Sub_Code.Window_Feed_Time * 2.0;
                await Task.Delay(1000 / 60);
            }
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

        private void Add_Dir_B_Click(object sender, RoutedEventArgs e)
        {
            if (bClosing)
                return;

            BetterFolderBrowser bfb = new BetterFolderBrowser()
            {
                Title = "導入先のフォルダを選択してください。",
                RootFolder = Sub_Code.Get_OpenDirectory_Path(),
                Multiselect = false
            };
            if (bfb.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Sub_Code.Set_Directory_Path(bfb.SelectedFolder);
                if (!bfb.SelectedFolder.Contains(Voice_Set.WoTB_Path))
                {
                    Message_Feed_Out("WoTB内のフォルダを選択してください。\nWoTBのフォルダ:" + Voice_Set.WoTB_Path);
                    bfb.Dispose();
                    return;
                }
                string dirOnly = bfb.SelectedFolder.Replace(Voice_Set.WoTB_Path, "").Replace("\\", "/");
                foreach (CModDestList dest in dirs)
                {
                    if (dest.DestDir == dirOnly)
                    {
                        Message_Feed_Out("指定したフォルダは既に追加されています。");
                        bfb.Dispose();
                        return;
                    }
                }
                CModDestList newDest = new CModDestList(dirOnly, dirs.Count);
                newDest.IsUserAddDir = true;
                dirs.Add(newDest);
                UserDirs.Add(dirOnly);
                Dest_Dir_List.ScrollIntoView(newDest);
                Message_Feed_Out("'" + dirOnly + "'を追加しました。");
            }
            bfb.Dispose();
        }

        private void Delete_Dir_B_Click(object sender, RoutedEventArgs e)
        {
            if (bClosing || Dest_Dir_List.SelectedIndex == -1)
                return;

            if (wotbDirs != null && Dest_Dir_List.SelectedIndex < wotbDirs.Count)
            {
                Message_Feed_Out("デフォルトで追加されているフォルダを削除することはできません。");
                return;
            }

            CModDestList dest = Dest_Dir_List.SelectedItem as CModDestList;

            MessageBoxResult result = MessageBox.Show("フォルダ'" + dest.DestDir + "'をリストから削除しますか?", "確認", MessageBoxButton.YesNo,
                MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                string dir = dest.DestDir;
                int index = Dest_Dir_List.SelectedIndex;
                if (wotbDirs != null)
                    index -= wotbDirs.Count;
                UserDirs.RemoveAt(index);
                for (int i = Dest_Dir_List.SelectedIndex + 1; i < dirs.Count; i++)
                    dirs[i].Index--;
                dirs.RemoveAt(Dest_Dir_List.SelectedIndex);
                Message_Feed_Out("'" + dir + "'をリストから削除しました。");
            }
        }

        private void Dest_Dir_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Dest_Dir_List.SelectedIndex == -1)
            {
                Dest_Dir_T.Text = "導入先:未選択";
                return;
            }

            Dest_Dir_T.Text = "導入先:" + Voice_Set.WoTB_Path.Replace("\\", "/") + dirs[Dest_Dir_List.SelectedIndex].DestDir;

            Selected_Index_T.Text = "選択しているインデックス:" + Dest_Dir_List.SelectedIndex;
        }

        private async void Apply_B_Click(object sender, RoutedEventArgs e)
        {
            //閉じる
            if (!bClosing)
            {
                if (Dest_Dir_List.SelectedIndex == -1)
                    file.UserDir = "";
                else if (Dest_Dir_List.SelectedIndex != file.WoTBDirIndex)
                    file.UserDir = dirs[Dest_Dir_List.SelectedIndex].DestDir;
                bClosing = true;
                while (Opacity > 0)
                {
                    Opacity -= Sub_Code.Window_Feed_Time * 2.0;
                    await Task.Delay(1000 / 60);
                }
                modManager.SaveModList();
                Visibility = Visibility.Hidden;
                wotbDirs = null;
                bClosing = false;
            }
        }
    }
}
