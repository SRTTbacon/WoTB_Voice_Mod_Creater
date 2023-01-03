using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WK.Libraries.BetterFolderBrowserNS;

namespace WoTB_Voice_Mod_Creater.Class
{
    public class ZipProgress
    {
        public int Total { get; }
        public int Processed { get; }
        public string CurrentItem { get; }
        public ZipProgress(int total, int processed, string currentItem)
        {
            Total = total;
            Processed = processed;
            CurrentItem = currentItem;
        }
    }
    public partial class Vocal_Inst_Cut_User : UserControl
    {
        GoogleDrive fileDownloader = new GoogleDrive();
        List<string> Music_Full_Name = new List<string>();
        List<string> Music_Convert_List = new List<string>();
        List<int> Music_Convert_Mode = new List<int>();
        string To_Dir_Path = "";
        string Remove_Name = "";
        readonly bool IsCanSpleeterDownload = false;
        bool IsClosing = false;
        bool IsDownloading = false;
        bool IsMessageShowing = false;
        bool IsInstalling = false;
        bool IsVocalOnly = false;
        bool IsSyncOutputDir = false;
        bool IsDeleteMode = false;
        bool IsConverting = false;
        int IsCompleteMessageShow = 0;
        //0 = 未ダウンロード, 1 = ダウンロード済み, 2 = 解凍済み, 3 = インストール済み
        int Spleeter_Progress = 0;
        Process p = null;
        public Vocal_Inst_Cut_User()
        {
            InitializeComponent();
            Download_P.Maximum = 100;
            Download_Size_T.Visibility = Visibility.Hidden;
            Download_P.Visibility = Visibility.Hidden;
            Cut_Combo.Items.Add("切り分け : 2個");
            Cut_Combo.Items.Add("切り分け : 4個");
            Cut_Combo.Items.Add("切り分け : 5個");
            Cut_Combo.SelectedIndex = 0;
            Main_Loop();
        }
        public async void Window_Show()
        {
            Opacity = 0;
            Visibility = Visibility.Visible;
            if (File.Exists(Voice_Set.Special_Path + "/Configs/Vocal_Inst_User.conf"))
            {
                try
                {
                    StreamReader str = Sub_Code.File_Decrypt_To_Stream(Voice_Set.Special_Path + "/Configs/Vocal_Inst_User.conf", "Music_Player_Vocal_Inst_User_Save");
                    Spleeter_Progress = int.Parse(str.ReadLine());
                    To_Dir_Path = str.ReadLine();
                    if (bool.Parse(str.ReadLine()))
                    {
                        Vocal_Only_C.Source = Sub_Code.Check_03;
                        IsVocalOnly = true;
                    }
                    else
                    {
                        Vocal_Only_C.Source = Sub_Code.Check_01;
                        IsVocalOnly = false;
                    }
                    if (bool.Parse(str.ReadLine()))
                    {
                        Sync_Dir_C.Source = Sub_Code.Check_03;
                        IsSyncOutputDir = true;
                    }
                    else
                    {
                        Sync_Dir_C.Source = Sub_Code.Check_01;
                        IsSyncOutputDir = false;
                    }
                    if (bool.Parse(str.ReadLine()))
                    {
                        Delete_C.Source = Sub_Code.Check_03;
                        IsDeleteMode = true;
                    }
                    else
                    {
                        Delete_C.Source = Sub_Code.Check_01;
                        IsDeleteMode = false;
                    }
                    if (To_Dir_Path == "")
                        To_Dir_T.Text = "未選択";
                    else
                        To_Dir_T.Text = "保存先:" + To_Dir_Path;
                    str.Close();
                    if (Spleeter_Progress == 0)
                        Download_B.Content = "Spleeterをダウンロード";
                    else if (Spleeter_Progress == 1)
                        Download_B.Content = "解凍";
                    else if (Spleeter_Progress == 2)
                        Download_B.Content = "Spleeterをインストール";
                    if (Spleeter_Progress == 3 && !Directory.Exists(Voice_Set.Special_Path + "\\Other\\Spleeter_Miniconda\\envs\\spleeter-cpu"))
                    {
                        Download_B.Content = "Spleeterをインストール";
                        Spleeter_Progress = 2;
                    }
                    if (Spleeter_Progress == 0 && File.Exists(Voice_Set.Special_Path + "\\Spleeter_Miniconda.dat") && new FileInfo(Voice_Set.Special_Path + "\\Spleeter_Miniconda.dat").Length / 1024 / 1024 > 980)
                    {
                        Download_B.Content = "解凍";
                        Spleeter_Progress = 1;
                    }
                    if (Spleeter_Progress < 3 && Directory.Exists(Voice_Set.Special_Path + "\\Other\\Spleeter_Miniconda\\envs\\spleeter-cpu"))
                        Spleeter_Progress = 3;
                    if (Spleeter_Progress < 3)
                        Show_Layout(true);
                    else
                        Show_Layout(false);
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show("設定を読み込めませんでした。。\nエラー回避のため設定は削除されます。");
                    File.Delete(Voice_Set.Special_Path + "/Configs/Vocal_Inst_User.conf");
                    Sub_Code.Error_Log_Write(e.Message);
                    Show_Layout(true);
                    To_Dir_Path = "";
                    To_Dir_T.Text = "未選択";
                    Vocal_Only_C.Source = Sub_Code.Check_01;
                    Sync_Dir_C.Source = Sub_Code.Check_01;
                    Delete_C.Source = Sub_Code.Check_01;
                }
            }
            else
            {
                if (Directory.Exists(Voice_Set.Special_Path + "\\Other\\Spleeter_Miniconda\\envs\\spleeter-cpu"))
                    Show_Layout(false);
                else
                    Show_Layout(true);
                To_Dir_Path = "";
                To_Dir_T.Text = "未選択";
                Vocal_Only_C.Source = Sub_Code.Check_01;
                Sync_Dir_C.Source = Sub_Code.Check_01;
                Delete_C.Source = Sub_Code.Check_01;
            }
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
        private async void Back_B_Click(object sender, RoutedEventArgs e)
        {
            if (Download_B.Content.ToString() == "ダウンロード中...")
            {
                MessageBoxResult result = MessageBox.Show("Spleeterをダウンロード中です。停止しますか？",
                    "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
                if (result == MessageBoxResult.Yes)
                {
                    fileDownloader.StopDownloadAsync();
                    IsDownloading = false;
                }
            }
            if (Download_B.Content.ToString() == "インストール中...")
                MessageBox.Show("Spleeterをインストール中です。ソフトウェアを終了しないようにしてください。");
            if (!IsClosing && !IsDownloading)
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
        async void Main_Loop()
        {
            while (true)
            {
                if (Music_Convert_List.Count > 0 && !IsConverting)
                {
                    Message_T.Text = Path.GetFileName(Music_Convert_List[0]) + "を変換しています...";
                    await Task.Delay(75);
                    if (!Directory.Exists(Voice_Set.Special_Path + "\\Other\\Spleeter_Miniconda\\Output"))
                        Directory.CreateDirectory(Voice_Set.Special_Path + "\\Other\\Spleeter_Miniconda\\Output");
                    try
                    {
                        File.Delete(Voice_Set.Special_Path + "\\Other\\Spleeter_Miniconda\\Output\\accompaniment.wav");
                        File.Delete(Voice_Set.Special_Path + "\\Other\\Spleeter_Miniconda\\Output\\vocals.wav");
                    }
                    catch (Exception e)
                    {
                        Sub_Code.Error_Log_Write(e.Message);
                        Music_Convert_List.RemoveAt(0);
                        Music_Convert_Mode.RemoveAt(0);
                        Convert_Count_T.Text = "変換中の数:" + Music_Convert_List.Count;
                        Message_Feed_Out("エラーが発生しました。処理を続行できません。");
                        await Task.Delay(3000);
                        continue;
                    }
                    IsConverting = true;
                    Task task_01 = Task.Run(() =>
                    {
                        int Stems_Number = 2;
                        if (Music_Convert_Mode[0] == 1)
                            Stems_Number = 4;
                        else if (Music_Convert_Mode[0] == 2)
                            Stems_Number = 5;
                        StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "\\Other\\Music_Convert.bat");
                        stw.WriteLine("chcp 65001");
                        stw.WriteLine("call \"" + Voice_Set.Special_Path + "\\Other\\Spleeter_Miniconda\\Scripts\\activate.bat\"");
                        stw.WriteLine("call activate spleeter-cpu");
                        stw.Write("spleeter separate -o \"" + Voice_Set.Special_Path + "\\Other\\Spleeter_Miniconda\\Output\" -p spleeter:" + Stems_Number + "stems \"" + Music_Convert_List[0]);
                        stw.Close();
                        stw.Dispose();
                        ProcessStartInfo processStartInfo1 = new ProcessStartInfo
                        {
                            FileName = Voice_Set.Special_Path + "\\Other\\Music_Convert.bat",
                            CreateNoWindow = true,
                            WorkingDirectory = Voice_Set.Special_Path + "\\Other\\Spleeter_Miniconda\\Output",
                            UseShellExecute = false
                        };
                        p = Process.Start(processStartInfo1);
                        p.WaitForExit();
                        File.Delete(Voice_Set.Special_Path + "\\Other\\Music_Convert.bat");
                        string Name_Only = Path.GetFileNameWithoutExtension(Music_Convert_List[0]);
                        string From_Dir = "";
                        foreach (string File_Path in Directory.GetFiles(Voice_Set.Special_Path + "\\Other\\Spleeter_Miniconda\\Output", "*.wav", SearchOption.AllDirectories))
                        {
                            string folderPath = System.IO.Path.GetDirectoryName(File_Path);
                            string folderName = System.IO.Path.GetFileName(folderPath);
                            if (Path.GetFileName(Music_Convert_List[0]).Contains(folderName))
                            {
                                From_Dir = folderPath;
                                break;
                            }
                        }
                        if (From_Dir != "" && File.Exists(From_Dir + "\\vocals.wav"))
                        {
                            string Output_Dir = To_Dir_Path;
                            if (IsSyncOutputDir)
                                Output_Dir = Path.GetDirectoryName(Music_Convert_List[0]);
                            if (!IsVocalOnly)
                            {
                                if (Stems_Number == 2)
                                    Sub_Code.File_Move(From_Dir + "\\accompaniment.wav", Output_Dir + "\\" + Path.GetFileNameWithoutExtension(Music_Convert_List[0]) + "_Instrument.wav", true);
                                else if (Stems_Number == 4 || Stems_Number == 5)
                                {
                                    Sub_Code.File_Move(From_Dir + "\\bass.wav", Output_Dir + "\\" + Path.GetFileNameWithoutExtension(Music_Convert_List[0]) + "bass.wav", true);
                                    Sub_Code.File_Move(From_Dir + "\\drums.wav", Output_Dir + "\\" + Path.GetFileNameWithoutExtension(Music_Convert_List[0]) + "drums.wav", true);
                                    Sub_Code.File_Move(From_Dir + "\\other.wav", Output_Dir + "\\" + Path.GetFileNameWithoutExtension(Music_Convert_List[0]) + "other.wav", true);
                                    if (Stems_Number == 5)
                                        Sub_Code.File_Move(From_Dir + "\\piano.wav", Output_Dir + "\\" + Path.GetFileNameWithoutExtension(Music_Convert_List[0]) + "piano.wav", true);
                                }
                            }
                            Sub_Code.File_Move(From_Dir + "\\vocals.wav", Output_Dir + "\\" + Path.GetFileNameWithoutExtension(Music_Convert_List[0]) + "_Vocal.wav", true);
                            Directory.Delete(From_Dir, true);
                            Remove_Name = Path.GetFileName(Music_Convert_List[0]).Trim();
                            if (Music_Convert_List.Count == 1)
                                IsCompleteMessageShow = 1;
                            else
                                IsCompleteMessageShow = 2;
                        }
                    });
                }
                if (IsCompleteMessageShow == 1)
                {
                    if (Remove_Name != "")
                    {
                        for (int Number = 0; Number < Music_List.Items.Count; Number++)
                        {
                            string Name = Music_List.Items[Number].ToString();
                            if (Name.Substring(0, Name.IndexOf('|') - 1).Trim() == Remove_Name)
                            {
                                if (IsDeleteMode)
                                    Music_List.Items.RemoveAt(Number);
                                else
                                    Music_List.Items[Number] = Name.Substring(0, Name.IndexOf('|') + 2) + "変換済み";
                                break;
                            }
                        }
                    }
                    Message_Feed_Out(Path.GetFileName(Music_Convert_List[0]) + "の切り分けが完了しました。");
                    Music_Convert_List.RemoveAt(0);
                    Music_Convert_Mode.RemoveAt(0);
                    Convert_Count_T.Text = "変換中の数:" + Music_Convert_List.Count;
                    Remove_Name = "";
                    IsCompleteMessageShow = 0;
                    IsConverting = false;
                }
                else if (IsCompleteMessageShow == 2)
                {
                    if (Remove_Name != "")
                    {
                        for (int Number = 0; Number < Music_List.Items.Count; Number++)
                        {
                            string Name = Music_List.Items[Number].ToString();
                            if (Name.Substring(0, Name.IndexOf('|') - 1).Trim() == Remove_Name)
                            {
                                if (IsDeleteMode)
                                    Music_List.Items.RemoveAt(Number);
                                else
                                    Music_List.Items[Number] = Name.Substring(0, Name.IndexOf('|') + 2) + "変換済み";
                                break;
                            }
                        }
                    }
                    Message_T.Text = Path.GetFileName(Music_Convert_List[0]) + "の切り分けが完了しました。";
                    Music_Convert_List.RemoveAt(0);
                    Music_Convert_Mode.RemoveAt(0);
                    Convert_Count_T.Text = "変換中の数:" + Music_Convert_List.Count;
                    Remove_Name = "";
                    IsCompleteMessageShow = 0;
                    IsConverting = false;
                }
                await Task.Delay(1000);
            }
        }
        void Configs_Save()
        {
            try
            {
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Configs/Vocal_Inst_User.tmp");
                stw.WriteLine(Spleeter_Progress);
                stw.WriteLine(To_Dir_Path);
                stw.WriteLine(IsVocalOnly);
                stw.WriteLine(IsSyncOutputDir);
                stw.Write(IsDeleteMode);
                stw.Close();
                Sub_Code.File_Encrypt(Voice_Set.Special_Path + "/Configs/Vocal_Inst_User.tmp", Voice_Set.Special_Path + "/Configs/Vocal_Inst_User.conf", "Music_Player_Vocal_Inst_User_Save", true);
            }
            catch (Exception e)
            {
                Message_Feed_Out("設定を保存できませんでした。");
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        private void Download_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsDownloading || IsClosing)
                return;
            try
            {
                if (Voice_Set.Special_Path.Contains(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)))
                {
                    Message_Feed_Out("ホーム画面からShift+Dキーでリソースフォルダの場所を変更する必要があります。");
                    return;
                }
                System.IO.DriveInfo drive = new System.IO.DriveInfo(Voice_Set.Special_Path[0].ToString());
                //ドライブの準備ができているか調べる
                if (drive.IsReady)
                {
                    //空き容量が3GB以下は実行できないように
                    if (drive.TotalFreeSpace / 1024 / 1024 <= 5120)
                    {
                        Message_Feed_Out("ディスクの空き容量が5GB以上である必要があります。");
                        return;
                    }
                }
            }
            catch (Exception e1)
            {
                Sub_Code.Error_Log_Write(e1.Message);
            }
            if (Download_B.Content.ToString() == "Spleeterをダウンロード")
            {
                if (IsCanSpleeterDownload)
                {
                    //自前のサーバーでは耐えきれないと判断し、GoogleDriveを使用
                    double FileSize = 1350;
                    MessageBoxResult result = MessageBox.Show("Spleeterをダウンロードしますか?\nファイルサイズが大きいため、インターネット回線が良いときに実行することをお勧めします。\nダウンロードサイズ:約" + FileSize + "MB",
                        "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
                    if (result == MessageBoxResult.Yes)
                    {
                        IsDownloading = true;
                        Download_P.Value = 0;
                        Download_P.Visibility = Visibility.Visible;
                        Download_B.Content = "ダウンロード中...";
                        Message_T.Text = "ファイルをダウンロードしています。しばらくお待ちください...";
                        Download_Size_T.Text = "0 / " + FileSize + "MB";
                        Download_Size_T.Visibility = Visibility.Visible;
                        fileDownloader.DownloadProgressChanged += (sender1, e1) =>
                        {
                            double Now_Downloaded = e1.BytesReceived / 1024.0 / 1024.0;
                            double Full_Download_Size = e1.TotalBytesToReceive / 1024.0 / 1024.0;
                            Download_Size_T.Text = (int)Now_Downloaded + " / " + (int)Full_Download_Size + "MB";
                            Download_P.Value = (Now_Downloaded / Full_Download_Size) * 100.0;
                        };
                        fileDownloader.DownloadFileCompleted += (sender1, e1) =>
                        {
                            Download_Size_T.Visibility = Visibility.Hidden;
                            Download_Size_T.Text = "0 / 0MB";
                            Download_P.Visibility = Visibility.Hidden;
                            Download_P.Value = 0;
                            if (!e1.Cancelled)
                            {
                                Message_T.Text = "ファイルのダウンロードが完了しました。解凍ボタンを押してください。";
                                Download_B.Content = "解凍";
                                Spleeter_Progress = 1;
                                Configs_Save();
                            }
                            else
                            {
                                Message_Feed_Out("ダウンロードをキャンセルしました。");
                                Download_B.Content = "Spleeterをダウンロード";
                                Sub_Code.File_Delete_V2(Voice_Set.Special_Path + "\\Spleeter_Miniconda.dat");
                            }
                            IsDownloading = false;
                            fileDownloader.Dispose();
                        };
                        fileDownloader.DownloadFileAsync("https://drive.google.com/file/d/1hl352XOyFFCzn7adz8gfLkO4dkryYwNH", Voice_Set.Special_Path + "\\Spleeter_Miniconda.dat");
                    }
                }
                else
                    Message_Feed_Out("この機能は非推奨です。既にインストールしている方以外は、Spleeter公式からGUI版をインストールすることをお勧めします。");
            }
            else if (Download_B.Content.ToString() == "解凍")
            {
                if (!File.Exists(Voice_Set.Special_Path + "\\Spleeter_Miniconda.dat") || new FileInfo(Voice_Set.Special_Path + "\\Spleeter_Miniconda.dat").Length / 1024 / 1024 <= 980)
                {
                    Download_B.Content = "Spleeterをダウンロード";
                    Message_Feed_Out("ダウンロードファイルが存在しませんでした。再度ダウンロードしてください。");
                    return;
                }
                MessageBoxResult result = MessageBox.Show("ダウンロードしたファイルを解凍しますか?\nファイルサイズが大きいため、時間がかかる可能性があります。",
                    "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        IsDownloading = true;
                        Message_T.Text = "ファイルを展開しています。しばらくお待ちください。";
                        Download_P.Value = 0;
                        Download_P.Visibility = Visibility.Visible;
                        Download_Size_T.Text = "ファイル数 : 0 / 0";
                        Download_Size_T.Visibility = Visibility.Visible;
                        Progress<ZipProgress> Zip_Progress = new Progress<ZipProgress>();
                        Zip_Progress.ProgressChanged += (object sender1, ZipProgress zipProgress) =>
                        {
                            double Total = zipProgress.Total;
                            double Processed = zipProgress.Processed;
                            Download_Size_T.Text = "ファイル数 : " + Processed + " / " + Total;
                            Download_P.Value = (Processed / Total) * 100.0;
                            if ((int)Total == (int)Processed)
                            {
                                Download_Size_T.Visibility = Visibility.Hidden;
                                Download_Size_T.Text = "0 / 0MB";
                                Download_P.Visibility = Visibility.Hidden;
                                Download_P.Value = 0;
                                Message_T.Text = "ファイルの解凍が完了しました。インストールボタンを押してください。";
                                Download_B.Content = "Spleeterをインストール";
                                IsDownloading = false;
                                Spleeter_Progress = 2;
                                Configs_Save();
                            }
                        };
                        ExtractToDirectory(Voice_Set.Special_Path + "\\Spleeter_Miniconda.dat", Voice_Set.Special_Path + "\\Other\\Spleeter_Miniconda", Zip_Progress);
                    }
                    catch (Exception e1)
                    {
                        Sub_Code.Error_Log_Write(e1.Message);
                        Message_Feed_Out("エラーが発生しました。");
                    }
                }
            }
            else if (Download_B.Content.ToString() == "Spleeterをインストール")
            {
                MessageBoxResult result = MessageBox.Show("データをインストールしますか?\nファイルサイズが大きいため、時間がかかる可能性があります。",
                   "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
                if (result == MessageBoxResult.Yes)
                {
                    Message_T.Text = "Spleeterをインストールしています。しばらくお待ちください...";
                    Download_B.Content = "インストール中...";
                    StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "\\Other\\Vocal_Inst_Cut.bat");
                    stw.WriteLine("chcp 65001");
                    stw.WriteLine("call \"" + Voice_Set.Special_Path + "\\Other\\Spleeter_Miniconda\\Scripts\\activate.bat\"");
                    stw.Write("call conda env create -p \"" + Voice_Set.Special_Path + "\\Other\\Spleeter_Miniconda\\envs\\spleeter-cpu\" -f \"" + Voice_Set.Special_Path + "\\Other\\Spleeter_Miniconda\\Scripts\\spleeter.yaml\"");
                    stw.Close();
                    IsInstalling = true;
                    Install_Complete();
                    Task task_02 = Task.Run(() =>
                    {
                        ProcessStartInfo processStartInfo1 = new ProcessStartInfo
                        {
                            FileName = Voice_Set.Special_Path + "\\Other\\Vocal_Inst_Cut.bat",
                            CreateNoWindow = true,
                            WorkingDirectory = Voice_Set.Special_Path + "\\Other\\Spleeter_Miniconda\\Scripts",
                            RedirectStandardOutput = true,
                            UseShellExecute = false
                        };
                        Process p = Process.Start(processStartInfo1);
                        p.OutputDataReceived += (object sender1, DataReceivedEventArgs e1) =>
                        {
                            if (e1.Data == null)
                                return;
                        };
                        p.BeginOutputReadLine();
                        p.WaitForExit();
                        p.Close();
                        IsInstalling = false;
                    });
                }
            }
        }
        async void Install_Complete()
        {
            while (true)
            {
                if (!IsInstalling)
                {
                    Spleeter_Progress = 3;
                    Download_B.Content = "Spleeterをダウンロード";
                    Configs_Save();
                    Show_Layout(false);
                    Message_Feed_Out("インストールが完了しました。");
                    break;
                }
                await Task.Delay(1000);
            }
        }
        void Show_Layout(bool Not_Installed)
        {
            if (Not_Installed)
            {
                Download_B.Visibility = Visibility.Visible;
                Main_Layout.Visibility = Visibility.Hidden;
            }
            else
            {
                Download_B.Visibility = Visibility.Hidden;
                Download_P.Visibility = Visibility.Hidden;
                Download_Size_T.Visibility = Visibility.Hidden;
                Main_Layout.Visibility = Visibility.Visible;
            }
        }
        async void ExtractToDirectory(string ZipFilePath, string destinationDirectoryName, IProgress<ZipProgress> progress)
        {
            await Task.Run(() =>
            {
                Ionic.Zip.ZipFile zip = Ionic.Zip.ZipFile.Read(ZipFilePath);
                DirectoryInfo di = Directory.CreateDirectory(destinationDirectoryName);
                string destinationDirectoryFullPath = di.FullName;
                int count = 0;
                foreach (Ionic.Zip.ZipEntry entry in zip.Entries)
                {
                    count++;
                    string Combine = System.IO.Path.Combine(destinationDirectoryFullPath, entry.FileName);
                    if (destinationDirectoryFullPath.Length > 247 || Combine.Length > 259)
                        continue;
                    try
                    {
                        string fileDestinationPath = System.IO.Path.GetFullPath(Combine);
                        if (!fileDestinationPath.StartsWith(destinationDirectoryFullPath, StringComparison.OrdinalIgnoreCase))
                            throw new IOException("指定されたフォルダの外にファイルが抽出されています。");
                        if (System.IO.Path.GetFileName(fileDestinationPath).Length == 0)
                            Directory.CreateDirectory(fileDestinationPath);
                        else
                        {
                            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fileDestinationPath));
                            entry.Extract(destinationDirectoryFullPath, Ionic.Zip.ExtractExistingFileAction.DoNotOverwrite);
                        }
                        var zipProgress = new ZipProgress(zip.Entries.Count, count, entry.FileName);
                        progress.Report(zipProgress);
                    }
                    catch { }
                }
                zip.Dispose();
            });
            Sub_Code.File_Delete_V2(ZipFilePath);
        }
        private void Music_List_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }
        private void Music_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Music_List.SelectedIndex == -1)
                return;
        }
        private void Music_Add_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsDownloading)
                return;
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog()
            {
                Title = "サウンドファイルを選択してください。",
                Filter = "サウンドファイル(*.flac;*.mp3;*.ogg;*.wav)|*.flac;*.mp3;*.ogg;*.wav",
                Multiselect = true
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string Error_Files = "";
                foreach (string File_Now in ofd.FileNames)
                {
                    if (Music_Full_Name.Contains(File_Now))
                    {
                        Error_Files += Path.GetFileName(File_Now) + "\n";
                        continue;
                    }
                    Music_Full_Name.Add(File_Now);
                    Music_List.Items.Add(Path.GetFileName(File_Now) + " | 変換待ち");
                }
                if (Error_Files != "")
                    MessageBox.Show("既にリストに存在するため、以下のファイルは追加できませんでした。\n" + Error_Files);
            }
        }
        private void Music_Delete_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || Music_List.SelectedIndex == -1 || IsDownloading)
                return;
            string Name = Music_List.SelectedItem.ToString();
            if (Name.Substring(Name.LastIndexOf('|')).Contains("変換中"))
            {
                Message_Feed_Out("変換中の項目は削除できません。");
                return;
            }
            Music_Full_Name.RemoveAt(Music_List.SelectedIndex);
            Music_List.Items.RemoveAt(Music_List.SelectedIndex);
        }
        private void To_Dir_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsDownloading)
                return;
            BetterFolderBrowser bfb = new BetterFolderBrowser()
            {
                Title = "保存先のフォルダを選択してください。",
                RootFolder = Sub_Code.Get_OpenDirectory_Path(),
                Multiselect = false
            };
            if (bfb.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                To_Dir_Path = bfb.SelectedFolder;
                To_Dir_T.Text = "保存先:" + To_Dir_Path + "\\";
                Configs_Save();
            }
        }
        private void Convert_One_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsDownloading || To_Dir_Path == "" || Music_List.SelectedIndex == -1)
                return;
            string Name = Music_List.SelectedItem.ToString();
            if (!Name.Substring(Name.LastIndexOf('|')).Contains("変換待ち"))
            {
                Message_Feed_Out("変換中または変換済みの項目は選択できません。");
                return;
            }
            Music_Convert_List.Add(Music_Full_Name[Music_List.SelectedIndex]);
            Music_List.Items[Music_List.SelectedIndex] = Path.GetFileName(Music_Full_Name[Music_List.SelectedIndex]) + " | 変換中";
            Music_Convert_Mode.Add(Cut_Combo.SelectedIndex);
            Convert_Count_T.Text = "変換中の数:" + Music_Convert_List.Count;
        }
        private void Convert_All_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsDownloading || To_Dir_Path == "" || Music_List.Items.Count == 0)
                return;
            int Count = 0;
            for (int Number = 0; Number < Music_List.Items.Count; Number++)
            {
                string Name = Music_List.Items[Number].ToString();
                if (!Name.Substring(Name.LastIndexOf('|')).Contains("変換待ち") || Music_Convert_List.Contains(Music_Full_Name[Number]))
                    continue;
                Music_List.Items[Number] = Path.GetFileName(Music_Full_Name[Number]) + " | 変換中";
                Music_Convert_List.Add(Music_Full_Name[Number]);
                Music_Convert_Mode.Add(Cut_Combo.SelectedIndex);
                Count++;
            }
            if (Count == 0)
                Message_Feed_Out("変換待ちのファイルが存在しませんでした。");
            Convert_Count_T.Text = "変換中の数:" + Music_Convert_List.Count;
        }
        private void Convert_Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsDownloading)
                return;
            else if (Music_List.SelectedIndex == -1)
            {
                Message_Feed_Out("変換をキャンセルしたい項目を選択してください。");
                return;
            }
            else if (!Music_Convert_List.Contains(Music_Full_Name[Music_List.SelectedIndex]))
            {
                Message_Feed_Out("選択した項目は変換中ではありません。");
                return;
            }
            MessageBoxResult result = MessageBox.Show("・切り分け中の項目をキャンセルしますか?", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                int Remove_Index = Music_Convert_List.IndexOf(Music_Full_Name[Music_List.SelectedIndex]);
                Music_Convert_List.RemoveAt(Remove_Index);
                Music_Convert_Mode.RemoveAt(Remove_Index);
                Music_List.Items[Music_List.SelectedIndex] = Path.GetFileName(Music_Full_Name[Music_List.SelectedIndex]) + " | 変換待ち";
                p.Close();
                p.Dispose();
                p = null;
            }
        }
        private void Cut_Combo_Help_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            string Message_01 = "Spleeterには切り分け方法が複数あるのでその設定をします。\n";
            string Message_02 = "・2個 : ボーカルとその他\n・4個 : ボーカル,ベース,ドラム,その他\n・5個 : ボーカル,ベース,ドラム,ピアノ,その他\n";
            string Message_03 = "アニメやゲームから切り抜くときは2個の設定が良いかと思います。\n";
            string Message_04 = "また、この設定は変換ボタンを押した時点で決定されますので、変換中は変更できません。";
            MessageBox.Show(Message_01 + Message_02 + Message_03 + Message_04);
        }
        private void Vocal_Only_C_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (IsVocalOnly)
            {
                IsVocalOnly = false;
                Vocal_Only_C.Source = Sub_Code.Check_02;
            }
            else
            {
                IsVocalOnly = true;
                Vocal_Only_C.Source = Sub_Code.Check_04;
            }
            Configs_Save();
        }
        private void Vocal_Only_C_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsVocalOnly)
                Vocal_Only_C.Source = Sub_Code.Check_04;
            else
                Vocal_Only_C.Source = Sub_Code.Check_02;
        }
        private void Vocal_Only_C_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsVocalOnly)
                Vocal_Only_C.Source = Sub_Code.Check_03;
            else
                Vocal_Only_C.Source = Sub_Code.Check_01;
        }
        private void Sync_Dir_C_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (IsSyncOutputDir)
            {
                IsSyncOutputDir = false;
                Sync_Dir_C.Source = Sub_Code.Check_02;
            }
            else
            {
                IsSyncOutputDir = true;
                Sync_Dir_C.Source = Sub_Code.Check_04;
            }
            Configs_Save();
        }
        private void Sync_Dir_C_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsSyncOutputDir)
                Sync_Dir_C.Source = Sub_Code.Check_04;
            else
                Sync_Dir_C.Source = Sub_Code.Check_02;
        }
        private void Sync_Dir_C_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsSyncOutputDir)
                Sync_Dir_C.Source = Sub_Code.Check_03;
            else
                Sync_Dir_C.Source = Sub_Code.Check_01;
        }
        private void Delete_C_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (IsDeleteMode)
            {
                IsDeleteMode = false;
                Delete_C.Source = Sub_Code.Check_02;
            }
            else
            {
                IsDeleteMode = true;
                Delete_C.Source = Sub_Code.Check_04;
            }
            Configs_Save();
        }
        private void Delete_C_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsDeleteMode)
                Delete_C.Source = Sub_Code.Check_04;
            else
                Delete_C.Source = Sub_Code.Check_02;
        }
        private void Delete_C_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsDeleteMode)
                Delete_C.Source = Sub_Code.Check_03;
            else
                Delete_C.Source = Sub_Code.Check_01;
        }
        private void Setting_Help_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            MessageBox.Show("これらの設定は変換が完了したときに適応されるので、変換中でもチェックを変更できます。");
        }
        private void Help_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            string Message_01 = "☆---注意事項---☆\n";
            string Message_02 = "・変換中はCPUが100%になり、サウンドファイルによってはメモリを多く消費します。\n";
            string Message_03 = "5分ほどの曲で5つに切り分けた場合、約5GBのメモリを消費します。ご参考までに。\n";
            string Message_04 = "・できるだけ変換中にソフトを終了させないでください。\n";
            string Message_05 = "・切り分けたサウンドはYoutubeなどにアップロードしないでください。";
            MessageBox.Show(Message_01 + Message_02 + Message_03 + Message_04 + Message_05);
        }
        private void Music_List_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Music_List.SelectedIndex = -1;
        }
    }
}