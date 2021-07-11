using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WK.Libraries.BetterFolderBrowserNS;
using WoTB_Voice_Mod_Creater.Wwise_Class.BNK_To_Wwise_Project;

namespace WoTB_Voice_Mod_Creater.Wwise_Class
{
    public partial class BNK_To_Project : UserControl
    {
        List<string> BNK_File = new List<string>();
        List<string> PCK_File = new List<string>();
        List<uint> BNK_Sound_Count = new List<uint>();
        string Init_File = null;
        string SoundbankInfo_File = null;
        bool IsClosing = false;
        bool IsMessageShowing = false;
        bool IsBusy = false;
        public BNK_To_Project()
        {
            InitializeComponent();
            Info_List_Clear();
        }
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
        void Info_List_Clear()
        {
            Info_List.Items.Clear();
            Info_List.Items.Add("BNKファイル数:0個");
            Info_List.Items.Add("PCKファイル数:0個");
            Info_List.Items.Add("Initファイル:未選択");
            Info_List.Items.Add("SoundbanksInfoファイル:未選択");
            Info_List.Items.Add("サウンド数:0");
        }
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
            IsMessageShowing = false;
            Message_T.Text = "";
            Message_T.Opacity = 1;
        }
        private async void Back_B_Click(object sender, RoutedEventArgs e)
        {
            if (!IsClosing && !IsBusy)
            {
                IsClosing = true;
                while (Opacity > 0)
                {
                    Opacity -= Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
                Visibility = Visibility.Hidden;
                IsClosing = false;
            }
        }
        private void Open_BNK_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog()
            {
                Title = ".bnkファイルを選択してください。",
                Multiselect = true,
                Filter = ".bnkファイル(*.bnk)|*.bnk"
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    foreach (string FileNames in ofd.FileNames)
                    {
                        string Select_Name = Path.GetFileName(FileNames);
                        foreach (string File_Now in BNK_File)
                        {
                            if (Path.GetFileName(File_Now) == Select_Name)
                            {
                                MessageBox.Show(Select_Name + "は既に追加されています。別のファイル名を指定する必要があります。");
                                continue;
                            }
                        }
                        Wwise_Class.BNK_Parse p = new Wwise_Class.BNK_Parse(FileNames);
                        int Count = p.Get_File_Count();
                        if (Count == 0)
                        {
                            MessageBox.Show(Select_Name + "にサウンドファイルが含まれていませんでした。");
                            p.Clear();
                            continue;
                        }
                        p.Clear();
                        BNK_Sound_Count.Add((uint)Count);
                        BNK_File.Add(FileNames);
                        Info_List.Items.Add("追加:" + Select_Name);
                    }
                    Info_List.Items[0] = "BNKファイル数:" + BNK_File.Count + "個";
                    uint All_Count = 0;
                    foreach (uint Counts in BNK_Sound_Count)
                        All_Count += Counts;
                    Info_List.Items[4] = "サウンド数:" + All_Count;
                }
                catch
                {
                    Message_Feed_Out("選択したファイルは破損している可能性があります。");
                }
            }
        }
        private void Open_PCK_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog()
            {
                Title = ".pckファイルを選択してください。",
                Multiselect = true,
                Filter = ".pckファイル(*.pck)|*.pck"
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    foreach (string FileNames in ofd.FileNames)
                    {
                        string Select_Name = Path.GetFileName(FileNames);
                        foreach (string File_Now in PCK_File)
                        {
                            if (Path.GetFileName(File_Now) == Select_Name)
                            {
                                MessageBox.Show(Select_Name + "は既に追加されています。別のファイル名を指定する必要があります。");
                                continue;
                            }
                        }
                        Wwise_File_Extract_V1 p = new Wwise_File_Extract_V1(FileNames);
                        int Count = p.Wwise_Get_File_Count();
                        if (Count == 0)
                        {
                            MessageBox.Show(Select_Name + "にサウンドファイルが含まれていませんでした。");
                            p.Pck_Clear();
                            continue;
                        }
                        p.Pck_Clear();
                        PCK_File.Add(FileNames);
                        Info_List.Items.Add("追加:" + Select_Name);
                    }
                    Info_List.Items[1] = "PCKファイル数:" + PCK_File.Count + "個";
                }
                catch
                {
                    Message_Feed_Out("選択したファイルは破損している可能性があります。");
                }
            }
        }
        private void Open_Init_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog()
            {
                Title = "Init.bnkファイルを選択してください。",
                Multiselect = false,
                Filter = "Init.bnkファイル(Init.bnk)|Init.bnk"
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    Wwise_Class.BNK_Parse p = new Wwise_Class.BNK_Parse(ofd.FileName);
                    if (!p.IsInitBNK())
                    {
                        Message_Feed_Out("指定したInit.bnkは対応していません。");
                        p.Clear();
                        return;
                    }
                    p.Clear();
                    Info_List.Items[2] = "Initファイル:選択済み";
                    Init_File = ofd.FileName;
                }
                catch
                {
                    Message_Feed_Out("選択したファイルは破損している可能性があります。");
                }
            }
        }
        private void Open_JSON_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog()
            {
                Title = "SoundbanksInfo.jsonファイルを選択してください。",
                Multiselect = false,
                Filter = "SoundbanksInfo.jsonファイル(SoundbanksInfo.json)|SoundbanksInfo.json"
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    List<string> Read_All = new List<string>();
                    Read_All.AddRange(File.ReadAllLines(ofd.FileName));
                    if (!Read_All[1].Contains("\"SoundBanksInfo\": {"))
                    {
                        Message_Feed_Out("指定したSoundbanksInfo.jsonは対応していません。");
                        return;
                    }
                    Read_All.Clear();
                    Info_List.Items[3] = "SoundbanksInfoファイル:選択済み";
                    SoundbankInfo_File = ofd.FileName;
                }
                catch
                {
                    Message_Feed_Out("選択したファイルは破損している可能性があります。");
                }
            }
        }
        private void Clear_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            MessageBoxResult result = MessageBox.Show("内容をクリアしますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                BNK_File.Clear();
                PCK_File.Clear();
                BNK_Sound_Count.Clear();
                Init_File = "";
                SoundbankInfo_File = "";
                Info_List_Clear();
            }
        }
        private async void Create_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            if (BNK_File.Count == 0)
            {
                Message_Feed_Out(".bnkファイルが選択されていません。");
                return;
            }
            else if (Init_File == "")
            {
                Message_Feed_Out("Init.bnkファイルが選択されていません。");
                return;
            }
            else if (SoundbankInfo_File == "")
            {
                Message_Feed_Out("SoundbanksInfo.jsonファイルが選択されていません。");
                return;
            }
            BetterFolderBrowser bfb = new BetterFolderBrowser()
            {
                Title = "保存先のフォルダを選択してください。",
                RootFolder = Sub_Code.Get_OpenDirectory_Path(),
                Multiselect = false,
            };
            if (bfb.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Sub_Code.Set_Directory_Path(bfb.SelectedFolder);
                /*if (Directory.GetFiles(bfb.SelectedFolder).Length != 0)
                {
                    Message_Feed_Out("フォルダ内は空である必要があります。");
                    return;
                }*/
                Message_T.Text = "BNKファイルを解析しています...";
                await Task.Delay(50);
                BNK_To_Wwise_Projects BNK_To_Project = new BNK_To_Wwise_Projects(Init_File, BNK_File, PCK_File, SoundbankInfo_File);
                if (Include_Sound_C.IsChecked.Value)
                    await BNK_To_Project.Create_Project_All(bfb.SelectedFolder, false, Message_T);
                else
                    await BNK_To_Project.Create_Project_All(bfb.SelectedFolder, true, Message_T);
                BNK_To_Project.Clear();
                Flash.Flash_Start();
                Message_Feed_Out("完了しました。");
            }
        }
        private void Attention_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            string Message_01 = "・この機能は、Steam版のWoTBを想定して作成されていますので、他のゲームのサウンドで動作するかは分かりません。\n";
            string Message_02 = "・一部のサウンドファイルでしか検証していないため、うまく動作しないファイルがあるかもしれません。\n";
            string Message_03 = "・すべての設定を正確に移植することは不可能なので、Wwise起動時に何かしらエラーが出るかもしれません。\n";
            string Message_04 = "・SoundbanksInfo.jsonは指定しなくても作成はできますが、絶望的に時間がかかりますので現実的ではありません。\n";
            string Message_05 = "・Advanced SettingsとStatesの設定はまだ反映できていません。今後のアップデートで利用できるようになります。";
            MessageBox.Show(Message_01 + Message_02 + Message_03 + Message_04 + Message_05);
        }
        private void Delete_B_Click(object sender, RoutedEventArgs e)
        {
            if (Info_List.SelectedIndex == -1 || !Info_List.SelectedItem.ToString().Contains("追加:"))
            {
                Message_Feed_Out("リスト内の'追加:～'の項目を選択する必要があります。");
                return;
            }
            string Select_Name = Info_List.SelectedItem.ToString().Substring(3);
            for (int Number = 0; Number < BNK_File.Count; Number++)
            {
                if (Path.GetFileName(BNK_File[Number]) == Select_Name)
                {
                    BNK_File.RemoveAt(Number);
                    BNK_Sound_Count.RemoveAt(Number);
                    break;
                }
            }
            for (int Number = 0; Number < PCK_File.Count; Number++)
            {
                if (Path.GetFileName(PCK_File[Number]) == Select_Name)
                {
                    PCK_File.RemoveAt(Number);
                    break;
                }
            }
            Info_List.Items.RemoveAt(Info_List.SelectedIndex);
            Info_List.Items[0] = "BNKファイル数:" + BNK_File.Count + "個";
            uint All_Count = 0;
            foreach (uint Counts in BNK_Sound_Count)
                All_Count += Counts;
            Info_List.Items[4] = "サウンド数:" + All_Count;
        }
    }
}