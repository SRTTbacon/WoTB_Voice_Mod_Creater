using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Un4seen.Bass;
using WK.Libraries.BetterFolderBrowserNS;

namespace WoTB_Voice_Mod_Creater.Class
{
    public partial class Tools : System.Windows.Controls.UserControl
    {
        public int IsMutualResponse = 0;
        readonly List<string> File_Full_Path = new List<string>();
        double Max_Stream_Length = 0.0;
        int Voice_Max_Index = 0;
        float Volume = 1f;
        float Pitch = 0f;
        bool IsBusy = false;
        bool IsMessageShowing = false;
        bool IsOpenDialog = false;
        bool IsInitDownloadLinks = false;
        FMOD_API.EventGroup EG = new FMOD_API.EventGroup();
        FMOD_API.Event FE = new FMOD_API.Event();
        public Tools()
        {
            InitializeComponent();
            DVPL_Extract_Help_B.Visibility = Visibility.Hidden;
            Volume_S.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Volume_MouseUp), true);
            Pitch_S.Minimum = -4;
            Pitch_S.Maximum = 2;
            Volume_S.Minimum = 0;
            Volume_S.Maximum = 100;
            Pitch_S.Value = 0;
            Volume_S.Value = 75;
        }
        public async void Window_Show()
        {
            if (File.Exists(Voice_Set.Special_Path + "/Configs/Tools.conf"))
            {
                try
                {
                    StreamReader str = Sub_Code.File_Decrypt_To_Stream(Voice_Set.Special_Path + "/Configs/Tools.conf", "Tools_Configs_Save");
                    Volume_S.Value = double.Parse(str.ReadLine());
                    str.Close();
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show("設定を読み込めませんでした。。\nエラー回避のため設定は削除されます。");
                    File.Delete(Voice_Set.Special_Path + "/Configs/Tools.conf");
                    Volume_S.Value = 75;
                    Sub_Code.Error_Log_Write(e.Message);
                }
            }
            int Start_Time = Environment.TickCount;
            if (!IsInitDownloadLinks)
                Voice_Set.TCP_Server.Send("Response|Get_Files|" + Voice_Set.UserName + "/aaa");
            Opacity = 0;
            Visibility = Visibility.Visible;
            while (Opacity < 1 && !IsBusy)
            {
                Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
            if (!IsInitDownloadLinks)
            {
                bool IsNoResponse = false;
                while (!IsInitDownloadLinks && !IsNoResponse)
                {
                    if (Start_Time + 5000 <= Environment.TickCount)
                        IsNoResponse = true;
                    await Task.Delay(50);
                }
                if (IsNoResponse)
                    Message_Feed_Out("サーバーが5秒以内に応答しませんでした。");
            }
        }
        public void Set_Download_Links(string Server_Response)
        {
            IsInitDownloadLinks = true;
            Upload_File_List.Items.Clear();
            string[] Files = Server_Response.Split(':');
            foreach (string File_Now in Files)
                if (File_Now != "")
                    Upload_File_List.Items.Add(Replace_File_Path(Path.GetFileNameWithoutExtension(File_Now) + Path.GetExtension(File_Now)));
        }
        //DVPLを解除(Pythonのプログラムに引数を渡し実行)
        private async void DVPL_Extract_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
                return;
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Title = ".DVPLファイルを選択してください。",
                Multiselect = true,
                Filter = "DVPLファイル(*.dvpl)|*.dvpl"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Message_T.Text = "DVPLファイルを展開しています...";
                await Task.Delay(50);
                string Error_Path = "";
                foreach (string File_Path in ofd.FileNames)
                {
                    DVPL.DVPL_UnPack(File_Path, Path.GetDirectoryName(File_Path) + "/" + Path.GetFileNameWithoutExtension(File_Path), false);
                    if (!File.Exists(Path.GetDirectoryName(File_Path) + "/" + Path.GetFileNameWithoutExtension(File_Path)))
                        Error_Path += File_Path + "\n";
                    else
                        File.Delete(File_Path);
                }
                Message_Feed_Out("DVPLファイルを展開しました。");
                if (Error_Path != "")
                {
                    Message_T.Text += "(エラーあり)";
                    System.Windows.MessageBox.Show("以下のファイルは展開できませんでした。\n" + Error_Path);
                }
            }
        }
        //DVPLを化(Pythonのプログラムに引数を渡し実行)
        private async void DVPL_Create_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
                return;
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Title = "dvpl化するファイルを選択してください。",
                Multiselect = true,
                Filter = "ファイル(*.*)|*.*"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string Error_Path = "";
                Message_T.Text = "DVPL化しています...";
                await Task.Delay(50);
                foreach (string File_Path in ofd.FileNames)
                {
                    if (Path.GetExtension(File_Path) == ".dvpl")
                    {
                        Error_Path += File_Path + "\n";
                        continue;
                    }
                    try
                    {
                        DVPL.DVPL_Pack(File_Path, File_Path + ".dvpl", false);
                        if (!File.Exists(File_Path + ".dvpl"))
                            throw new Exception(".dvplファイルが作成できていません。");
                    }
                    catch (Exception e1)
                    {
                        Error_Path += File_Path + "\n";
                        Sub_Code.Error_Log_Write(e1.Message);
                    }
                }
                Message_Feed_Out("ファイルをDVPL化しました。");
                if (Error_Path != "")
                {
                    Message_T.Text += "(エラーあり)";
                    System.Windows.MessageBox.Show("以下のファイルはdvpl化できませんでした。\n" + Error_Path);
                }
            }
        }
        //閉じる
        private async void Exit_B_Click(object sender, RoutedEventArgs e)
        {
            if (!IsBusy)
            {
                IsBusy = true;
                while (Opacity > 0)
                {
                    Opacity -= Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
                Visibility = Visibility.Hidden;
                Message_T.Text = "";
                File_Full_Path.Clear();
                IsBusy = false;
            }
        }
        //出力文字を時間がたったらフェードアウトさせる
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
                    Message_T.Opacity -= 0.025;
                await Task.Delay(1000 / 60);
            }
            if (IsMessageShowing)
                Message_T.Text = "";
            Message_T.Opacity = 1;
            IsMessageShowing = false;
        }
        private void DVPL_Extract_Help_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
                return;
            string Message_01 = "このバージョンではWoTB内のData/Sfxに入っている*.fev.dvplと*.fsb.dvplを解除できない可能性があります。\n";
            string Message_02 = "また、WoTB内の他のファイルも解除できないかもしれません。(sounds.yaml.dvplやマップファイルは解除できました。)";
            System.Windows.MessageBox.Show(Message_01 + Message_02);
        }
        private void FEV_Select_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
                return;
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Title = "FEVファイルを選択してください。",
                Filter = "FEVファイル(*.fev)|*.fev",
                Multiselect = false,
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    FE.release();
                    Fmod_Player.ESystem.load(ofd.FileName);
                    FMOD_API.EventProject EC = new FMOD_API.EventProject();
                    Fmod_Player.ESystem.getProjectByIndex(0, ref EC);
                    EC.getGroupByIndex(0, true, ref EG);
                    EG.getNumEvents(ref Voice_Max_Index);
                    FEV_Index_S.Value = 0;
                    FEV_Index_S.Maximum = Voice_Max_Index - 1;
                    FEV_Index_T.Text = "1/" + Voice_Max_Index;
                    FEV_Name_T.Text = Path.GetFileName(ofd.FileName);
                }
                catch (Exception e1)
                {
                    FEV_Name_T.Text = "";
                    Message_Feed_Out("FEVファイルを取得できませんでした。");
                    Sub_Code.Error_Log_Write(e1.Message);
                }
            }
        }
        void Set_Fmod_Bank_Play(int Voice_Number)
        {
            //fevの中から指定した位置のサウンドを再生
            if (IsBusy)
                return;
            if (FE != null)
                FE.stop();
            EG.getEventByIndex(Voice_Number, FMOD_API.EVENT_MODE.DEFAULT, ref FE);
            FE.setVolume(Volume);
            FE.setPitch(Pitch, FMOD_API.EVENT_PITCHUNITS.TONES);
            FE.start();
        }
        private void FEV_Stop_B_Click(object sender, RoutedEventArgs e)
        {
            FE.stop();
        }
        private void FEV_Play_B_Click(object sender, RoutedEventArgs e)
        {
            if (!IsBusy)
                Set_Fmod_Bank_Play((int)FEV_Index_S.Value);
        }
        private void FEV_Index_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            FEV_Index_T.Text = (int)(FEV_Index_S.Value + 1) + "/" + Voice_Max_Index;
        }
        private void Pitch_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Pitch = (float)Pitch_S.Value;
            Pitch_T.Text = "速度:" + Math.Round(Pitch_S.Value, 1, MidpointRounding.AwayFromZero);
            if (FE != null)
                FE.setPitch(Pitch, FMOD_API.EVENT_PITCHUNITS.TONES);
        }
        private void Volume_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //音量を変更(ダブルクリックで初期化)
            Volume = (float)Volume_S.Value / 100;
            Volume_T.Text = "音量:" + (int)Volume_S.Value;
            if (FE != null)
                FE.setVolume(Volume);
        }
        private void Pitch_S_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Pitch_S.Value = 0;
        }
        void Volume_MouseUp(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Configs_Save();
        }
        void Configs_Save()
        {
            try
            {
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Configs/Tools.tmp");
                stw.Write(Volume_S.Value);
                stw.Close();
                Sub_Code.File_Encrypt(Voice_Set.Special_Path + "/Configs/Tools.tmp", Voice_Set.Special_Path + "/Configs/Tools.conf", "Tools_Configs_Save", true);
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        private void DDS_B_Click(object sender, RoutedEventArgs e)
        {
            if (!IsBusy)
                DDS_Tool_Window.Window_Show();
        }
        private void FSB_Extract_B_Click(object sender, RoutedEventArgs e)
        {
            if (!IsBusy)
                Fmod_Extract_Window.Window_Show();
        }
        private void BGM_Create_Help_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
                return;
            string Message_01 = "注意:WoTBの設定で多国籍音声を使用している場合、指定した国家でしか再生されません。\n";
            string Message_02 = "全体の言語を日本語以外に設定されている場合でも再生されません。\n";
            string Message_03 = "初期のWoTBの音声ファイルではBGMを入れると戦闘開始時の音声が再生されなくなりますが、このソフトで作成した音声はちゃんと再生されるようになっています。\n";
            string Message_04 = "重要:このバージョンでは、初期のWoTBの音声ファイルか、このソフトで作成した音声ファイルでないと動作しません。\n";
            string Message_05 = "また、選択した音声ファイルにBGMを追加するため、事前にバックアップを作成しておくことをお勧めします。";
            System.Windows.MessageBox.Show(Message_01 + Message_02 + Message_03 + Message_04 + Message_05);
        }
        void Set_Music_Number(string Music_Dir, int MaxNumber)
        {
            int File_Number = 0;
            for (int Number2 = 0; Number2 <= 9; Number2++)
            {
                string[] Files2 = Directory.GetFiles(Music_Dir, "Music_" + Number2 + "*", SearchOption.TopDirectoryOnly);
                File_Number += Files2.Length;
            }
            if (File_Number > MaxNumber)
            {
                for (int Number2 = MaxNumber + 1; Number2 <= File_Number + 1; Number2++)
                {
                    try
                    {
                        if (Number2 < 10)
                            File.Delete(Sub_Code.File_Get_FileName_No_Extension(Music_Dir + "/" + "Music_0" + Number2));
                        else
                            File.Delete(Sub_Code.File_Get_FileName_No_Extension(Music_Dir + "/" + "Music_" + Number2));
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            else if (File_Number < MaxNumber)
            {
                Random r = new Random();
                for (int Number2 = File_Number; Number2 <= MaxNumber; Number2++)
                {
                    int Test = 0;
                    start:
                    Test++;
                    try
                    {
                        int r2 = r.Next(1, File_Number + 1);
                        string r3;
                        if (r2 < 10)
                            r3 = "0" + r2;
                        else
                            r3 = r2.ToString();
                        string FilePath = Sub_Code.File_Get_FileName_No_Extension(Music_Dir + "/" + "Music_" + r3);
                        if (Number2 < 10)
                            File.Copy(FilePath, Music_Dir + "/" + "Music_0" + Number2 + Path.GetExtension(FilePath), true);
                        else
                            File.Copy(FilePath, Music_Dir + "/" + "Music_" + Number2 + Path.GetExtension(FilePath), true);
                    }
                    catch
                    {
                        if (Test >= 10)
                            continue;
                        goto start;
                    }
                }
            }
        }
        private void BGM_Mix_Help_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
                return;
            string Message_01 = "・新サウンドエンジンでは、曲が終わったあと次の曲に変更するということができないため、事前に曲が5分以下であれば";
            string Message_02 = "別の曲を付け足して5分以上にします。(作成ボタンを押してファイルを選択する際、このソフトで作成した音声の場合この設定は無視されます。)\n";
            string Message_03 = "その分ファイル容量は増えますが、ゲームの動作に影響はないかと思います。\n";
            string Message_04 = "・BGMが一曲しかない場合や付け足しても5分以上に届かない場合は同じ曲を付け足します。\n";
            string Message_05 = "・戦闘時間が最大7分のため、BGMが7分30秒を超えるとそれ以降は強制的にカットされます。\n";
            string Message_06 = "注意:指定しているBGMによって作成時間が増える可能性があります。";
            System.Windows.MessageBox.Show(Message_01 + Message_02 + Message_03 + Message_04 + Message_05 + Message_06);
        }
        private async void DVPL_Extract_Dir_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy || IsOpenDialog)
                return;
            IsOpenDialog = true;
            BetterFolderBrowser bfb = new BetterFolderBrowser()
            {
                Title = ".dvplファイルが存在するフォルダを選択してください。",
                RootFolder = Sub_Code.Get_OpenDirectory_Path(),
                Multiselect = false,
            };
            if (bfb.ShowDialog() == DialogResult.OK)
            {
                IsBusy = true;
                Sub_Code.Set_Directory_Path(bfb.SelectedFolder);
                uint Dir_Files = 0;
                List<string> DVPL_Files = new List<string>();
                foreach (string file in Directory.EnumerateFiles(bfb.SelectedFolder, "*", SearchOption.AllDirectories))
                {
                    if (Dir_Files >= 100000)
                    {
                        Message_Feed_Out("フォルダ内のファイル数が多すぎます。");
                        DVPL_Files.Clear();
                        IsBusy = false;
                        IsOpenDialog = false;
                        return;
                    }
                    if (Path.GetExtension(file) == ".dvpl")
                        DVPL_Files.Add(file);
                    Dir_Files++;
                }
                if (DVPL_Files.Count == 0)
                {
                    Message_Feed_Out(".dvplファイルが見つかりませんでした。");
                    IsBusy = false;
                    IsOpenDialog = false;
                    return;
                }
                Message_T.Text = ".dvplファイルを変換しています...";
                await Task.Delay(50);
                int Count = DVPL.DVPL_UnPack(DVPL_Files, DVPL_Delete_C.IsChecked.Value);
                Message_Feed_Out(Count + "個の.dvplファイルを変換しました。");
            }
            bfb.Dispose();
            IsBusy = false;
            IsOpenDialog = false;
        }
        private async void DVPL_Create_Dir_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy || IsOpenDialog)
                return;
            IsOpenDialog = true;
            BetterFolderBrowser bfb = new BetterFolderBrowser()
            {
                Title = ".dvplファイルが存在するフォルダを選択してください。",
                RootFolder = Sub_Code.Get_OpenDirectory_Path(),
                Multiselect = false,
            };
            if (bfb.ShowDialog() == DialogResult.OK)
            {
                IsBusy = true;
                Sub_Code.Set_Directory_Path(bfb.SelectedFolder);
                uint Dir_Files = 0;
                List<string> Add_Files = new List<string>();
                foreach (string file in Directory.EnumerateFiles(bfb.SelectedFolder, "*", SearchOption.AllDirectories))
                {
                    if (Dir_Files >= 100000)
                    {
                        Message_Feed_Out("フォルダ内のファイル数が多すぎます。");
                        Add_Files.Clear();
                        IsBusy = false;
                        IsOpenDialog = false;
                        return;
                    }
                    if (Path.GetExtension(file) != ".dvpl")
                        Add_Files.Add(file);
                    Dir_Files++;
                }
                if (Add_Files.Count == 0)
                {
                    Message_Feed_Out("ファイルが見つかりませんでした。");
                    IsBusy = false;
                    IsOpenDialog = false;
                    return;
                }
                IsMessageShowing = false;
                Message_T.Text = "ファイルを変換しています...";
                await Task.Delay(50);
                await Multithread.DVPL_Pack(Add_Files, DVPL_Delete_C.IsChecked.Value);
                Message_Feed_Out(Add_Files.Count + "個のファイルを変換しました。");
            }
            bfb.Dispose();
            IsBusy = false;
            IsOpenDialog = false;
        }
        private async void Upload_File_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy || IsOpenDialog)
                return;
            System.Windows.Forms.OpenFileDialog ofd = new OpenFileDialog()
            {
                Title = "アップロードするファイルを選択してください。",
                Filter = "AnyFile(*.*)|*.*",
                Multiselect = false,
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                List<string> Noooooo = new List<string>()
                {
                    "#",
                    "%",
                    "{",
                    "}",
                    "^",
                    "[",
                    "]",
                    "`",
                    "@",
                    "&",
                    "=",
                    "+",
                    "$",
                    ","
                };
                string FileName = Replace_File_Path(Replace_FileName(Path.GetFileNameWithoutExtension(ofd.FileName), Noooooo.ToArray()) + Path.GetExtension(ofd.FileName));
                Noooooo.Clear();
                Voice_Set.TCP_Server.Send("Response|File_Exist|" + Voice_Set.UserName + "/" + FileName);
                IsMessageShowing = false;
                Message_T.Opacity = 1;
                Message_T.Text = "サーバーの応答を待っています...";
                int Start_Time = Environment.TickCount;
                bool NoResponse = false;
                while (IsMutualResponse == 0 && !NoResponse)
                {
                    if (Start_Time + 5000 <= Environment.TickCount)
                        NoResponse = true;
                    await Task.Delay(50);
                }
                if (NoResponse)
                {
                    ofd.Dispose();
                    Message_Feed_Out("サーバーが5秒以内に応答しませんでした。時間を置いて再度実行してください。");
                    return;
                }
                else if (IsMutualResponse == 1)
                {
                    IsMutualResponse = 0;
                    ofd.Dispose();
                    Message_Feed_Out("名前が同じファイルが存在します。既存のファイルを削除するか、ファイル名を変更してください。");
                    return;
                }
                IsMutualResponse = 0;
                Message_T.Text = "アップロードしています...";
                IsBusy = true;
                bool IsEnd = false;
                Task aa = Task.Run(() =>
                {
                    using (var stream = new FileStream(ofd.FileName, FileMode.Open))
                    {
                        Max_Stream_Length = stream.Length / 1024.0 / 1024.0;
                        Max_Stream_Length = Math.Round(Max_Stream_Length, MidpointRounding.AwayFromZero);
                        if (Voice_Set.FTPClient.Directory_Exist("/WoTB_Voice_Mod/Users/" + Voice_Set.UserName))
                            Voice_Set.FTPClient.Directory_Create("/WoTB_Voice_Mod/Users/" + Voice_Set.UserName);
                        Voice_Set.FTPClient.SFTP_Server.UploadFile(stream, "/WoTB_Voice_Mod/Users/" + Voice_Set.UserName + "/" + FileName, UpdateProgresBar);
                        IsEnd = true;
                    }
                });
                while (!IsEnd)
                    await Task.Delay(50);
                Voice_Set.TCP_Server.Send("Response|Set_Download_Link|" + Voice_Set.UserName + "/" + FileName);
                Voice_Set.TCP_Server.Send("Message|" + Voice_Set.UserName + "->ファイル:" + FileName + "のダウンロードリンクを生成しました。");
                Upload_File_List.Items.Add(FileName);
                await Task.Delay(500);
                IsBusy = false;
                Message_Feed_Out("ダウンロードリンクを生成しました。リストから項目を選択し、URLを取得してください。");
            }
            ofd.Dispose();
        }
        async void UpdateProgresBar(ulong uploaded)
        {
            double Now_Stream_Length = uploaded / 1024.0 / 1024.0;
            Now_Stream_Length = Math.Round(Now_Stream_Length, MidpointRounding.AwayFromZero);
            await Task.Delay(5);
            Dispatcher.Invoke(() =>
            {
                Message_T.Text = "アップロードしています..." + Now_Stream_Length + " / " + Max_Stream_Length + "MB";
            });
        }
        private void Delete_File_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy || IsOpenDialog)
                return;
            if (Upload_File_List.SelectedIndex == -1)
            {
                Message_Feed_Out("削除する項目を選択してください。");
                return;
            }
            string Name = Upload_File_List.SelectedItem.ToString();
            MessageBoxResult result = System.Windows.MessageBox.Show(Name + "のリンクを削除しますか?この操作は取り消せません。", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation,
                MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                Upload_File_List.Items.RemoveAt(Upload_File_List.SelectedIndex);
                Voice_Set.TCP_Server.Send("Response|File_Delete|" + Voice_Set.UserName + "/" + Name);
                Voice_Set.TCP_Server.Send("Message|" + Voice_Set.UserName + "->ファイル:" + Name + "を削除しました。");
                Message_Feed_Out(Name + "をサーバーから削除しました。");
            }
        }
        private void Generate_Link_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy || IsOpenDialog)
                return;
            if (Upload_File_List.SelectedIndex == -1)
            {
                Message_Feed_Out("リンクを取得する項目を選択してください。");
                return;
            }
            try
            {
                System.Windows.Forms.Clipboard.SetData(System.Windows.DataFormats.Text, "https://battlecry.xyz/Mod_Creater/Users/" + Voice_Set.UserName + "/" + Upload_File_List.SelectedItem.ToString());
                Message_Feed_Out("クリップボードにダウンロードリンクをコピーしました。");
            }
            catch { }
        }
        static string Replace_File_Path(string File_Path)
        {
            string ReturnString = File_Path;
            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (char aa in invalidChars)
                ReturnString = ReturnString.Replace(aa, '_');
            return ReturnString;
        }
        string Replace_FileName(string FileName, string[] Replace_String)
        {
            string Return_String = "";
            for (int Number_01 = 0;  Number_01 < Replace_String.Length; Number_01++)
                Return_String = FileName.Replace(Replace_String[Number_01], "");
            return Return_String;
        }
    }
}
public class BGM_Create
{
    //新サウンドエンジンでは曲が終わったら次の曲を流すということができないため、ランダムで曲をつなげて5分以上にします
    //戦闘時間は7分なので7分30秒以降はカットされます
    public static void Set_Music_Mix_Random(string BGM_Dir, int Max_Second = 440)
    {
        try
        {
            Random r = new Random();
            string[] Ex = { ".mp3", ".mp2", ".wav", ".ogg", "aiff", ".aif", ".asf", ".flac", ".wma" };
            string[] BGM_Files_Temp = DirectoryEx.GetFiles(BGM_Dir, SearchOption.TopDirectoryOnly, Ex);
            Directory.CreateDirectory(WoTB_Voice_Mod_Creater.Voice_Set.Special_Path + "/BGM_Temp");
            foreach (string BGM_File_Now in BGM_Files_Temp)
                File.Copy(BGM_File_Now, WoTB_Voice_Mod_Creater.Voice_Set.Special_Path + "/BGM_Temp/" + Path.GetFileName(BGM_File_Now), true);
            string[] BGM_Files = DirectoryEx.GetFiles(WoTB_Voice_Mod_Creater.Voice_Set.Special_Path + "/BGM_Temp", SearchOption.TopDirectoryOnly, Ex);
            int File_Number_Now = -1;
            foreach (string BGM_File_Now in BGM_Files)
            {
                File_Number_Now++;
                int stream = Bass.BASS_StreamCreateFile(BGM_File_Now, 0, 0, BASSFlag.BASS_STREAM_DECODE);
                int length = (int)Bass.BASS_ChannelBytes2Seconds(stream, Bass.BASS_ChannelGetLength(stream));
                Bass.BASS_StreamFree(stream);
                if (length >= 300)
                    continue;
                List<int> Include_Numbers = new List<int>();
                Include_Numbers.Add(File_Number_Now);
                for (int Number = 0; Number < BGM_Files.Length; Number++)
                {
                    if (Include_Numbers.Count >= BGM_Files.Length)
                    {
                        Include_Numbers.Clear();
                        Number = -1;
                        continue;
                    }
                    int Include_Number = 0;
                    while (true)
                    {
                        int Temp = r.Next(0, BGM_Files.Length);
                        bool IsIncluded = false;
                        foreach (int Number_Now in Include_Numbers)
                        {
                            if (Number_Now == Temp)
                            {
                                IsIncluded = true;
                                break;
                            }
                        }
                        if (!IsIncluded)
                        {
                            Include_Number = Temp;
                            break;
                        }
                    }
                    Include_Numbers.Add(Include_Number);
                    int stream2 = Bass.BASS_StreamCreateFile(BGM_Files[Include_Number], 0, 0, BASSFlag.BASS_STREAM_DECODE);
                    int length2 = (int)Bass.BASS_ChannelBytes2Seconds(stream2, Bass.BASS_ChannelGetLength(stream2));
                    Bass.BASS_StreamFree(stream2);
                    length += length2;
                    if (length >= 300)
                        break;
                }
                StreamWriter stw = File.CreateText(WoTB_Voice_Mod_Creater.Voice_Set.Special_Path + "/Encode_Mp3/BGM_Mix.bat");
                stw.WriteLine("chcp 65001");
                stw.Write("\"" + WoTB_Voice_Mod_Creater.Voice_Set.Special_Path + "/Encode_Mp3/ffmpeg.exe\" -y ");
                foreach (int Number in Include_Numbers)
                    stw.Write("-i \"" + BGM_Files[Number] + "\" ");
                stw.Write("-filter_complex \"concat=n=" + Include_Numbers.Count + ":v=0:a=1\" -t " + Max_Second + " \"" + BGM_Dir + "/" + Path.GetFileName(BGM_Files[File_Number_Now]) + "\"");
                stw.Close();
                ProcessStartInfo processStartInfo1 = new ProcessStartInfo
                {
                    FileName = WoTB_Voice_Mod_Creater.Voice_Set.Special_Path + "/Encode_Mp3/BGM_Mix.bat",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process p = Process.Start(processStartInfo1);
                p.WaitForExit();
                File.Delete(WoTB_Voice_Mod_Creater.Voice_Set.Special_Path + "/Encode_Mp3/BGM_Mix.bat");
            }
            Directory.Delete(WoTB_Voice_Mod_Creater.Voice_Set.Special_Path + "/BGM_Temp", true);
        }
        catch (Exception e)
        {
            WoTB_Voice_Mod_Creater.Sub_Code.Error_Log_Write(e.Message);
        }
    }
    //WoTBの初期の音声ファイルの言語を取得
    public static string Get_Voice_Language(List<string> Files)
    {
        if (Files.Contains("783658022"))
            return "English(US)";
        else if (Files.Contains("148236242"))
            return "arb";
        else if (Files.Contains("522280109"))
            return "gup";
        else if (Files.Contains("897549921"))
            return "pbr";
        else if (Files.Contains("1003460716"))
            return "vi";
        else if (Files.Contains("348256779"))
            return "tr";
        else if (Files.Contains("270859153"))
            return "th";
        else if (Files.Contains("892963782"))
            return "ru";
        else if (Files.Contains("778579436"))
            return "pl";
        else if (Files.Contains("885986556"))
            return "ko";
        else if (Files.Contains("1056502513"))
            return "ja";
        else if (Files.Contains("549350697"))
            return "it";
        else if (Files.Contains("26760078"))
            return "fr";
        else if (Files.Contains("327412030"))
            return "fi";
        else if (Files.Contains("365235587"))
            return "en";
        else if (Files.Contains("286527395"))
            return "es";
        else if (Files.Contains("970759741"))
            return "de";
        else if (Files.Contains("513502435"))
            return "cs";
        else if (Files.Contains("586623688"))
            return "cn";
        else
            return "";
    }
}
//フォルダ内のファイルを取得(複数の拡張子を指定できます)
public static class DirectoryEx
{
    public static string[] GetFiles(string path, params string[] extensions)
    {
        return Directory
            .GetFiles(path, "*.*")
            .Where(c => extensions.Any(extension => c.EndsWith(extension)))
            .ToArray();
    }
    public static string[] GetFiles(string path, SearchOption searchOption, params string[] extensions)
    {
        return Directory
            .GetFiles(path, "*.*", searchOption)
            .Where(c => extensions.Any(extension => c.EndsWith(extension)))
            .ToArray();
    }
}