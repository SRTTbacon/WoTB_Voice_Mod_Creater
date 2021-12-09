using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Un4seen.Bass;
using WK.Libraries.BetterFolderBrowserNS;

namespace WoTB_Voice_Mod_Creater.Class
{
    public partial class Vocal_Inst_Cut : UserControl
    {
        List<string> File_Name = new List<string>();
        List<string> File_Full_Name = new List<string>();
        List<string> File_Ex = new List<string>();
        List<bool> File_IsUploaded = new List<bool>();
        List<bool> File_IsChanged = new List<bool>();
        string Output_Dir = "";
        bool IsClosing = false;
        bool IsBusy = false;
        bool IsMessageShowing = false;
        bool IsOpenDialog = false;
        public Vocal_Inst_Cut()
        {
            InitializeComponent();
            MP3_OR_WAV_C.Items.Add("MP3");
            MP3_OR_WAV_C.Items.Add("WAV");
            MP3_OR_WAV_C.SelectedIndex = 0;
            Download_Vocal_B.Visibility = Visibility.Hidden;
            Download_Inst_B.Visibility = Visibility.Hidden;
            Download_P.Visibility = Visibility.Hidden;
            Download_T.Visibility = Visibility.Hidden;
        }
        public async void Window_Show()
        {
            if (File.Exists(Voice_Set.Special_Path + "/Configs/Vocal_Inst_Cut.conf"))
            {
                try
                {
                    StreamReader str = Sub_Code.File_Decrypt_To_Stream(Voice_Set.Special_Path + "/Configs/Vocal_Inst_Cut.conf", "Vocal_Inst_Cut_Save_Data_SRTTbacon");
                    MP3_OR_WAV_C.SelectedIndex = int.Parse(str.ReadLine());
                    Output_Dir = str.ReadLine();
                    Out_Dir_T.Text = Output_Dir + "\\";
                    string line;
                    List_Clear();
                    while ((line = str.ReadLine()) != null)
                    {
                        if (!line.Contains("|"))
                            continue;
                        string[] Split = line.Split('|');
                        File_Name.Add(Path.GetFileName(Split[0]));
                        File_Full_Name.Add(Split[0]);
                        File_IsUploaded.Add(bool.Parse(Split[1]));
                        File_IsChanged.Add(bool.Parse(Split[2]));
                        File_Ex.Add(Split[3]);
                    }
                    str.Dispose();
                    Music_Status_Change();
                }
                catch (Exception e)
                {
                    Output_Dir = "";
                    Sub_Code.Error_Log_Write(e.Message);
                }
            }
            else
                List_Clear();
            Opacity = 0;
            Visibility = Visibility.Visible;
            while (Opacity < 1 && !IsClosing)
            {
                Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
        }
        //メッセージを表示してフェードアウト
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
            if (!IsClosing && !IsBusy)
            {
                IsClosing = true;
                Configs_Save();
                while (Opacity > 0)
                {
                    Opacity -= Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
                IsClosing = false;
                Visibility = Visibility.Hidden;
            }
        }
        void Configs_Save()
        {
            if (!IsLoaded)
                return;
            try
            {
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Configs/Vocal_Inst_Cut.dat");
                stw.WriteLine(MP3_OR_WAV_C.SelectedIndex);
                stw.WriteLine(Output_Dir);
                for (int Number = 0; Number < File_Name.Count; Number++)
                    stw.WriteLine(File_Full_Name[Number] + "|" + File_IsUploaded[Number] + "|" + File_IsChanged[Number] + "|" + File_Ex[Number]);
                stw.Close();
                Sub_Code.File_Encrypt(Voice_Set.Special_Path + "/Configs/Vocal_Inst_Cut.dat", Voice_Set.Special_Path + "/Configs/Vocal_Inst_Cut.conf", "Vocal_Inst_Cut_Save_Data_SRTTbacon", true);
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        void List_Clear()
        {
            File_Name.Clear();
            File_Full_Name.Clear();
            File_IsUploaded.Clear();
            File_IsChanged.Clear();
            Music_List.Items.Clear();
        }
        public void Change_Server_Stetus(int Number_of_People)
        {
            Waiting_Thread_T.Text = "サーバー全体の変換中の数:" + Number_of_People;
        }
        public void Music_Status_Change(string Change_Name = "")
        {
            Dispatcher.Invoke(() =>
            {
                List<string> List_Change_Text = new List<string>();
                for (int Number = 0; Number < File_Name.Count; Number++)
                {
                    string FileNameOnly = File_Name[Number].Substring(0, File_Name[Number].LastIndexOf('.'));
                    if (!File_IsChanged[Number] && Voice_Set.FTPClient.File_Exist("/WoTB_Voice_Mod/Users/" + Voice_Set.UserName + "/" + FileNameOnly + "/Vocal." + File_Ex[Number].ToLower()))
                        File_IsChanged[Number] = true;
                    if (!File_IsUploaded[Number])
                        List_Change_Text.Add(File_Name[Number] + " | アップロード待ち | " + File_Ex[Number]);
                    else if (!File_IsChanged[Number])
                        List_Change_Text.Add(File_Name[Number] + " | 変換中 | " + File_Ex[Number]);
                    else
                        List_Change_Text.Add(File_Name[Number] + " | 変換済み | " + File_Ex[Number]);
                }
                Music_List.Items.Clear();
                foreach (string Now in List_Change_Text)
                    Music_List.Items.Add(Now);
                List_Change_Text.Clear();
                if (Change_Name != "")
                    Message_Feed_Out(Change_Name + "が変換されました。");
            });
        }
        void Music_Status_Change_V2(int Number)
        {
            if (!File_IsUploaded[Number])
                Music_List.Items[Number] = File_Name[Number] + " | アップロード待ち | " + File_Ex[Number];
            else if (!File_IsChanged[Number])
                Music_List.Items[Number] = File_Name[Number] + " | 変換中 | " + File_Ex[Number];
            else
                Music_List.Items[Number] = File_Name[Number] + " | 変換済み | " + File_Ex[Number];
        }
        private void Music_Add_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog()
            {
                Title = "切り分ける曲を選択してください。",
                Filter = "サウンドファイル(*.wav;*.mp3;*.flac;*.ogg;*.acc;*.wma)|*.wav;*.mp3;*.flac;*.ogg;*.aac;*.wma",
                Multiselect = true
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                IsBusy = true;
                string Error_File_Name = "";
                foreach (string File_Now in ofd.FileNames)
                {
                    string FileName = Path.GetFileName(File_Now);
                    string Name_Only = FileName.Substring(0, FileName.LastIndexOf('.'));
                    if (File_Name.Contains(Name_Only + ".wav") || File_Name.Contains(Name_Only + ".mp3"))
                    {
                        if (Error_File_Name == "")
                            Error_File_Name = FileName;
                        else
                            Error_File_Name += "\n" + FileName;
                        continue;
                    }
                    File_Full_Name.Add(File_Now);
                    File_IsUploaded.Add(false);
                    File_IsChanged.Add(false);
                    //拡張子を小文字限定にしたいため
                    if (MP3_OR_WAV_C.SelectedIndex == 0)
                    {
                        File_Name.Add(Name_Only + ".mp3");
                        File_Ex.Add("MP3");
                    }
                    else if (MP3_OR_WAV_C.SelectedIndex == 1)
                    {
                        File_Name.Add(Name_Only + ".wav");
                        File_Ex.Add("WAV");
                    }
                    Music_List.Items.Add(FileName + " | アップロード待ち | " + File_Ex[File_Ex.Count - 1]);
                }
                if (Error_File_Name == "")
                    Message_Feed_Out("ファイルをリストに追加しました。\nアップロードのボタンを押すと変換が開始されます。");
                else
                {
                    MessageBox.Show("以下のファイルを追加できませんでした。\n" + Error_File_Name);
                    Message_Feed_Out("例外を除いたファイルをリストに追加しました。\nアップロードのボタンを押すと変換が開始されます。");
                }
                Configs_Save();
                IsBusy = false;
            }
            ofd.Dispose();
        }
        private async void Music_Upload_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            int Upload_Count = 0;
            int NotChanged_Count = 0;
            string Error_FileName = "";
            string Over_Minutes_FileName = "";
            for (int Number = 0; Number < File_Name.Count; Number++)
            {
                if (!File_IsUploaded[Number])
                {
                    Upload_Count++;
                    if (!File.Exists(File_Full_Name[Number]))
                    {
                        if (Error_FileName == "")
                            Error_FileName = File_Name[Number];
                        else
                            Error_FileName += "\n" + File_Name[Number];
                        continue;
                    }
                    int StreamHandle = Bass.BASS_StreamCreateFile(File_Full_Name[Number], 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE);
                    double Length = Bass.BASS_ChannelBytes2Seconds(StreamHandle, Bass.BASS_ChannelGetLength(StreamHandle, BASSMode.BASS_POS_BYTES));
                    if (Length > 600)
                    {
                        if (Over_Minutes_FileName == "")
                            Over_Minutes_FileName = File_Name[Number];
                        else
                            Over_Minutes_FileName += "\n" + File_Name[Number];
                    }
                    Bass.BASS_StreamFree(StreamHandle);
                }
                if (!File_IsChanged[Number])
                    NotChanged_Count++;
            }
            if (Error_FileName != "")
            {
                MessageBox.Show("以下のファイルが存在しません。確認してください。\n" + Error_FileName);
                Message_Feed_Out("存在しないファイルがリストに追加されています。\n確認してもう一度お試しください。");
                return;
            }
            else if (Over_Minutes_FileName != "")
            {
                MessageBox.Show("以下のファイルは10分を超えているので続行できません。\n" + Over_Minutes_FileName);
                Message_Feed_Out("10分を超えるサウンドファイルはアップロードできません。");
                return;
            }
            if (Upload_Count == 0)
            {
                Message_Feed_Out("アップロード待ちのファイルが存在しません。");
                return;
            }
            else if (NotChanged_Count > 10)
            {
                Message_Feed_Out("一度に変換できるファイル数は10個です。");
                return;
            }
            MessageBoxResult result = MessageBox.Show("アップロード待ちのファイルをサーバーにアップロードしますか?", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                if (!Voice_Set.FTPClient.Directory_Exist("/WoTB_Voice_Mod/Users/" + Voice_Set.UserName))
                    Voice_Set.FTPClient.Directory_Create("/WoTB_Voice_Mod/Users/" + Voice_Set.UserName);
                IsMessageShowing = false;
                Message_T.Opacity = 1;
                for (int Number = 0; Number < File_Name.Count; Number++)
                {
                    if (!File_IsUploaded[Number])
                    {
                        Message_T.Text = File_Name[Number] + "をアップロードしています...";
                        await Task.Delay(75);
                        string FileNameOnly = File_Name[Number].Substring(0, File_Name[Number].LastIndexOf('.'));
                        Voice_Set.FTPClient.Directory_Create("/WoTB_Voice_Mod/Users/" + Voice_Set.UserName + "/" + FileNameOnly);
                        if (Voice_Set.FTPClient.UploadFile(File_Full_Name[Number], "/WoTB_Voice_Mod/Users/" + Voice_Set.UserName + "/" + FileNameOnly + "/" + File_Name[Number], true))
                        {
                            if (!Voice_Set.TCP_Server.IsConnected)
                                Voice_Set.TCP_Server.Connect(SRTTbacon_Server.IP, SRTTbacon_Server.TCP_Port);
                            if (MP3_OR_WAV_C.SelectedIndex == 0)
                                Voice_Set.TCP_Server.Send(Voice_Set.UserName + "_Private|Music_Change|" + File_Name[Number] + "|true");
                            else
                                Voice_Set.TCP_Server.Send(Voice_Set.UserName + "_Private|Music_Change|" + File_Name[Number] + "|false");
                            File_IsUploaded[Number] = true;
                            Music_Status_Change_V2(Number);
                        }
                        else
                        {
                            Message_Feed_Out("ファイルのアップロード中にエラーが発生しました。");
                            return;
                        }
                    }
                }
                Configs_Save();
                Message_Feed_Out("アップロードが完了しました。変換されるまで時間がかかる場合があります。");
            }
        }
        private void MP3_OR_WAV_Help_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
                return;
            string Message_01 = "・WAVだとダウンロードに時間がかかってしまうのでMP3をおススメします。(MP3はWAVの約1/10のファイルサイズ)\n";
            string Message_02 = "・今後の更新で対応するファイル形式が増えるかもしれません。";
            MessageBox.Show(Message_01 + Message_02);
        }
        private void Music_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Music_List.SelectedIndex == -1 || !File_IsChanged[Music_List.SelectedIndex])
            {
                Download_Vocal_B.Visibility = Visibility.Hidden;
                Download_Inst_B.Visibility = Visibility.Hidden;
                return;
            }
            Download_Vocal_B.Visibility = Visibility.Visible;
            Download_Inst_B.Visibility = Visibility.Visible;
        }
        private void Out_Dir_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy || IsOpenDialog)
                return;
            IsOpenDialog = true;
            IsBusy = true;
            BetterFolderBrowser bfb = new BetterFolderBrowser()
            {
                Title = "保存先のフォルダを選択してください。",
                RootFolder = Sub_Code.Get_OpenDirectory_Path(),
                Multiselect = false,
            };
            if (bfb.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Sub_Code.Set_Directory_Path(bfb.SelectedFolder);
                if (!Sub_Code.CanDirectoryAccess(bfb.SelectedFolder))
                {
                    Message_Feed_Out("指定したフォルダにアクセス権限がありません。別のフォルダを選択してください。");
                    bfb.Dispose();
                    IsOpenDialog = false;
                    IsBusy = false;
                    return;
                }
                Output_Dir = bfb.SelectedFolder;
                Out_Dir_T.Text = Output_Dir + "\\";
                Configs_Save();
            }
            bfb.Dispose();
            IsOpenDialog = false;
            IsBusy = false;
        }
        async Task Download_Music(string From_File, string To_File, string Name)
        {
            try
            {
                Download_P.Visibility = Visibility.Visible;
                Download_T.Visibility = Visibility.Visible;
                long Full_Size = Voice_Set.FTPClient.GetFileSize(From_File);
                Task task = Task.Run(() =>
                {
                    Voice_Set.FTPClient.DownloadFile(From_File, To_File);
                });
                while (true)
                {
                    long File_Size_Now = 0;
                    if (File.Exists(To_File))
                    {
                        FileInfo fi = new FileInfo(To_File);
                        File_Size_Now = fi.Length;
                    }
                    double Download_Percent = (double)File_Size_Now / Full_Size * 100;
                    int Percent_INT = (int)Math.Round(Download_Percent, MidpointRounding.AwayFromZero);
                    Download_P.Value = Percent_INT;
                    Download_T.Text = "進捗:" + Percent_INT + "%";
                    if (File_Size_Now >= Full_Size)
                    {
                        Download_P.Value = 0;
                        Download_T.Text = "進捗:0%";
                        break;
                    }
                    await Task.Delay(100);
                }
                Download_P.Visibility = Visibility.Hidden;
                Download_T.Visibility = Visibility.Hidden;
                Message_Feed_Out(Name + "のダウンロードが完了しました。");
            }
            catch (Exception e1)
            {
                Download_P.Visibility = Visibility.Hidden;
                Download_T.Visibility = Visibility.Hidden;
                Sub_Code.Error_Log_Write(e1.Message);
                Message_Feed_Out("エラーが発生しました。詳しくはError_Log.txtを参照してください。");
            }
        }
        private async void Download_Vocal_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy || Music_List.SelectedIndex == -1)
                return;
            if (Output_Dir == "")
            {
                Message_Feed_Out("保存先のフォルダが指定されていません。");
                return;
            }
            string Ex = "";
            if (File_Ex[Music_List.SelectedIndex] == "MP3")
                Ex = ".mp3";
            else if (File_Ex[Music_List.SelectedIndex] == "WAV")
                Ex = ".wav";
            string FileNameOnly = File_Name[Music_List.SelectedIndex].Substring(0, File_Name[Music_List.SelectedIndex].LastIndexOf('.'));
            if (!Voice_Set.FTPClient.File_Exist("/WoTB_Voice_Mod/Users/" + Voice_Set.UserName + "/" + FileNameOnly + "/Vocal" + Ex))
            {
                Message_Feed_Out("ボーカル部分のファイルが見つかりませんでした。管理者に連絡してください。");
                return;
            }
            IsBusy = true;
            await Download_Music("/WoTB_Voice_Mod/Users/" + Voice_Set.UserName + "/" + FileNameOnly + "/Vocal" + Ex, Output_Dir + "\\" + FileNameOnly + "_Vocal" + Ex, "ボーカル部分");
            IsBusy = false;
        }
        private async void Download_Inst_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy || Music_List.SelectedIndex == -1)
                return;
            if (Output_Dir == "")
            {
                Message_Feed_Out("保存先のフォルダが指定されていません。");
                return;
            }
            string FileNameOnly = File_Name[Music_List.SelectedIndex].Substring(0, File_Name[Music_List.SelectedIndex].LastIndexOf('.'));
            string Ex = "";
            if (File_Ex[Music_List.SelectedIndex] == "MP3")
                Ex = ".mp3";
            else if (File_Ex[Music_List.SelectedIndex] == "WAV")
                Ex = ".wav";
            if (!Voice_Set.FTPClient.File_Exist("/WoTB_Voice_Mod/Users/" + Voice_Set.UserName + "/" + FileNameOnly + "/Instrument" + Ex))
            {
                Message_Feed_Out("楽器部分のファイルが見つかりませんでした。管理者に連絡してください。");
                return;
            }
            IsBusy = true;
            await Download_Music("/WoTB_Voice_Mod/Users/" + Voice_Set.UserName + "/" + FileNameOnly + "/Instrument" + Ex, Output_Dir + "\\" + FileNameOnly + "_Instrument" + Ex, "楽器部分");
            IsBusy = false;
        }
        private void MP3_OR_WAV_C_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Configs_Save();
        }
        private void Music_List_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Music_List.SelectedIndex = -1;
        }
        private void Music_Delete_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy || Music_List.SelectedIndex == -1)
                return;
            string NameOnly = File_Name[Music_List.SelectedIndex].Substring(0, File_Name[Music_List.SelectedIndex].LastIndexOf('.'));
            File_Name.RemoveAt(Music_List.SelectedIndex);
            File_Full_Name.RemoveAt(Music_List.SelectedIndex);
            File_Ex.RemoveAt(Music_List.SelectedIndex);
            File_IsUploaded.RemoveAt(Music_List.SelectedIndex);
            File_IsChanged.RemoveAt(Music_List.SelectedIndex);
            Music_List.Items.RemoveAt(Music_List.SelectedIndex);
            if (Voice_Set.FTPClient.Directory_Exist("/WoTB_Voice_Mod/Users/" + Voice_Set.UserName + "/" + NameOnly))
                Voice_Set.FTPClient.Directory_Delete("/WoTB_Voice_Mod/Users/" + Voice_Set.UserName + "/" + NameOnly);
        }
        private void Attention_B_Click(object sender, RoutedEventArgs e)
        {
            string Message_01 = "・アップロードのボタンを押すとアップロード待ちのすべての曲がサーバーに送信されます。";
            string Message_02 = "通信環境によっては時間がかかるかもしれません。\n";
            string Message_03 = "・10分を超える曲は処理に時間がかかるためアップロードできないようになっています。\n";
            string Message_04 = "・'リストから削除'を押すと、サーバーにアップロードされた曲も一緒に削除されます。\n";
            string Message_05 = "・変換した曲はYoutubeなどにアップロードしないようお願い致します。\n";
            string Message_06 = "・中央下の部分に書いてある文字と数字は、現在サーバーが処理中の曲数を表しています。これを見てだいたい何分後か予想できます。";
            MessageBox.Show(Message_01 + Message_02 + Message_03 + Message_04 + Message_05 + Message_06);
        }
        private void Music_List_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = true;
        }
    }
}
