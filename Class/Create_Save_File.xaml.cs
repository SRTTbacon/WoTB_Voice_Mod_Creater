using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WoTB_Voice_Mod_Creater.FMOD;
using WoTB_Voice_Mod_Creater.Wwise_Class;

namespace WoTB_Voice_Mod_Creater.Class
{
    public partial class Create_Save_File : UserControl
    {
        string Selected_File = "";
        int Max_Stream_Count = 0;
        int Now_Stream_Count = 0;
        bool IsMessageShowing = false;
        bool IsClosing = false;
        bool IsBusy = false;
        List<List<string>> BNK_FSB_Voices = new List<List<string>>();
        List<string> Need_Files = new List<string>();
        public Create_Save_File()
        {
            InitializeComponent();
            Voices_L.Items.Add("音声ファイルが選択されていません。");
        }
        async void Message_Feed_Out(string Message)
        {
            //テキストが一定期間経ったらフェードアウト
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
                {
                    Message_T.Opacity -= 0.025;
                }
                await Task.Delay(1000 / 60);
            }
            IsMessageShowing = false;
            Message_T.Text = "";
            Message_T.Opacity = 1;
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
        private async void Open_File_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
            {
                return;
            }
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog()
            {
                Title = "音声Modを選択してください。",
                Filter = "音声ファイル(*.fsb;*.bnk)|*.fsb;*.bnk",
                Multiselect = false
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    IsBusy = true;
                    string Ex = Path.GetExtension(ofd.FileName);
                    if (Ex == ".fsb")
                    {
                        Message_T.Text = ".fsbファイルを解析しています...";
                        await Task.Delay(50);
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
                            IsBusy = false;
                            return;
                        }
                        Voices.Clear();
                        Max_Stream_Count = Fmod_Class.FSB_GetLength(ofd.FileName);
                        BNK_FSB_Voices = Voice_Set.Voice_BGM_Name_Change_From_FSB_To_Index_FSBFile(ofd.FileName);
                        Now_Stream_Count = 0;
                        Voices_L.Items.Clear();
                        for (int Number = 0; Number < 34; Number++)
                        {
                            Now_Stream_Count += BNK_FSB_Voices[Number].Count;
                            Voices_L.Items.Add(Voice_Set.Get_Voice_Type_Japanese_Name_V2(Number) + " : " + BNK_FSB_Voices[Number].Count + "個");
                        }
                    }
                    else if (Ex == ".bnk")
                    {
                        Message_T.Text = ".bnkファイルを解析しています...";
                        await Task.Delay(50);
                        string To_Dir = Voice_Set.Special_Path + "/Wwise/BNK_WAV_WoT";
                        try
                        {
                            if (Directory.Exists(To_Dir))
                            {
                                Directory.Delete(To_Dir, true);
                            }
                        }
                        catch (Exception e1)
                        {
                            Sub_Code.Error_Log_Write(e1.Message);
                        }
                        int BNK_Mode = 0;
                        Wwise_Class.BNK_Parse p = new Wwise_Class.BNK_Parse(ofd.FileName);
                        if (p.IsVoiceFile(true))
                        {
                            BNK_Mode = 1;
                        }
                        else if (p.IsVoiceFile())
                        {
                            BNK_Mode = 2;
                        }
                        else
                        {
                            Message_Feed_Out("選択されたbnkファイルは音声データではありません。");
                            IsBusy = false;
                            return;
                        }
                        bool Mode;
                        if (BNK_Mode == 1)
                        {
                            Mode = true;
                        }
                        else if (BNK_Mode == 2)
                        {
                            Mode = false;
                        }
                        else
                        {
                            Message_Feed_Out("選択されたbnkファイルは音声データではありません。");
                            IsBusy = false;
                            return;
                        }
                        BNK_FSB_Voices = p.Get_Voices(Mode);
                        foreach (List<string> Types in BNK_FSB_Voices)
                        {
                            foreach (string File_Now in Types)
                            {
                                Need_Files.Add(File_Now);
                            }
                        }
                        if (Need_Files.Count == 0)
                        {
                            Message_T.Text = "移植できるファイルが見つからなかったため、特殊な方法で解析しています...";
                            await Task.Delay(50);
                            p.SpecialBNKFileMode = 1;
                            BNK_FSB_Voices = p.Get_Voices(Mode);
                            foreach (List<string> Types in BNK_FSB_Voices)
                            {
                                foreach (string File_Now in Types)
                                {
                                    Need_Files.Add(File_Now);
                                }
                            }
                        }
                        if (Need_Files.Count == 0)
                        {
                            p.Clear();
                            BNK_FSB_Voices.Clear();
                            Message_Feed_Out("移植できる音声が見つかりませんでした。");
                            IsBusy = false;
                            return;
                        }
                        p.Clear();
                        Voices_L.Items.Clear();
                        Now_Stream_Count = 0;
                        for (int Number = 0; Number < 34; Number++)
                        {
                            Now_Stream_Count += BNK_FSB_Voices[Number].Count;
                            Voices_L.Items.Add(Voice_Set.Get_Voice_Type_Japanese_Name_V2(Number) + " : " + BNK_FSB_Voices[Number].Count + "個");
                        }
                        Wwise_File_Extract_V2 Wwise_BNK = new Wwise_File_Extract_V2(ofd.FileName);
                        Max_Stream_Count = Wwise_BNK.Wwise_Get_Numbers();
                        Wwise_BNK.Bank_Clear();
                    }
                    Message_Feed_Out("解析が完了しました。");
                    Selected_File = ofd.FileName;
                    File_Name_T.Text = "ファイル名:" + Path.GetFileName(ofd.FileName);
                }
                catch (Exception e1)
                {
                    Selected_File = "";
                    BNK_FSB_Voices.Clear();
                    Voices_L.Items.Clear();
                    Voices_L.Items.Add("音声ファイルが選択されていません。");
                    Sub_Code.Error_Log_Write(e1.Message);
                    Message_Feed_Out("エラーが発生しました。ファイル形式が正しくない可能性があります。");
                }
            }
            ofd.Dispose();
            IsBusy = false;
        }
        private async void Save_File_Create_B_Click(object sender, RoutedEventArgs e)
        {
            if (Selected_File == "" || BNK_FSB_Voices.Count == 0)
            {
                Message_Feed_Out("音声ファイルが選択されていません。");
                return;
            }
            if (Project_Name_T.Text == "")
            {
                Message_Feed_Out("プロジェクト名が設定されていません。");
                return;
            }
            if (Project_Name_T.Text.Contains("  "))
            {
                Message_Feed_Out("プロジェクト名に空白を連続で使用することはできません。");
                return;
            }
            try
            {
                Directory.CreateDirectory(Voice_Set.Special_Path + "/Temp/" + Project_Name_T.Text);
                Directory.Delete(Voice_Set.Special_Path + "/Temp", true);
                if (Project_Name_T.Text.Contains("/") || Project_Name_T.Text.Contains("\\"))
                {
                    throw new Exception("プロジェクト名に'/'を付けることはできません。");
                }
            }
            catch (Exception e1)
            {
                Message_Feed_Out("プロジェクト名に不適切な文字が含まれています。");
                Sub_Code.Error_Log_Write(e1.Message);
                return;
            }
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog()
            {
                Title = "セーブファイルの保存先を指定してください。",
                Filter = "セーブファイル(*.wvs)|*.wvs",
                InitialDirectory = Directory.GetCurrentDirectory()
            };
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                IsBusy = true;
                try
                {
                    string Ex = Path.GetExtension(Selected_File);
                    if (Ex == ".fsb")
                    {
                        Message_T.Text = "音声ファイルを抽出しています...";
                        await Task.Delay(50);
                        Fmod_File_Extract_V2.FSB_Extract_To_Directory(Selected_File, Voice_Set.Local_Path + "/Projects/" + Project_Name_T.Text + "/All_Voices_Temp");
                        Message_T.Text = ".wavファイルをエンコードしています...";
                        await Task.Delay(50);
                        await Multithread.Convert_To_Wav(Voice_Set.Local_Path + "/Projects/" + Project_Name_T.Text + "/All_Voices_Temp", Voice_Set.Local_Path + "/Projects/" + Project_Name_T.Text + "/All_Voices", true, true);
                        Directory.Delete(Voice_Set.Local_Path + "/Projects/" + Project_Name_T.Text + "/All_Voices_Temp", true);
                    }
                    else if (Ex == ".bnk")
                    {
                        Message_T.Text = ".wavまたは.oggに変換しています...";
                        await Task.Delay(50);
                        Wwise_File_Extract_V2 Wwise_BNK = new Wwise_File_Extract_V2(Selected_File);
                        Wwise_BNK.Wwise_Extract_To_WEM_Directory_V2(Voice_Set.Local_Path + "/Projects/" + Project_Name_T.Text + "/All_Voices");
                        Wwise_BNK.Bank_Clear();
                        Message_T.Text = "不要な音声ファイルを削除しています...";
                        await Task.Delay(50);
                        string[] All_Files = Directory.GetFiles(Voice_Set.Local_Path + "/Projects/" + Project_Name_T.Text + "/All_Voices", "*", SearchOption.TopDirectoryOnly);
                        foreach (string File_Now in All_Files)
                        {
                            if (!Need_Files.Contains(Path.GetFileNameWithoutExtension(File_Now)))
                            {
                                Sub_Code.File_Delete_V2(File_Now);
                            }
                        }
                        string[] Files = Directory.GetFiles(Voice_Set.Local_Path + "/Projects/" + Project_Name_T.Text + "/All_Voices", "*.wem", SearchOption.TopDirectoryOnly);
                        foreach (string File_Now in Files)
                        {
                            Sub_Code.File_Delete_V2(File_Now);
                        }
                    }
                    StreamWriter stw = File.CreateText(sfd.FileName + ".tmp");
                    stw.WriteLine(Project_Name_T.Text + "|IsNotChangeProjectNameMode=true");
                    for (int Number = 0; Number < BNK_FSB_Voices.Count; Number++)
                    {
                        foreach (string Name in BNK_FSB_Voices[Number])
                        {
                            stw.WriteLine(Number + "|" + Sub_Code.File_Get_FileName_No_Extension(Voice_Set.Local_Path + "\\Projects\\" + Project_Name_T.Text + "\\All_Voices\\" + Name));
                        }
                    }
                    stw.Close();
                    Sub_Code.File_Encrypt(sfd.FileName + ".tmp", sfd.FileName, "SRTTbacon_Create_Voice_Save", true);
                    Message_Feed_Out("保存しました。\nProjectsフォルダに該当する音声が入っています。");
                }
                catch (Exception e1)
                {
                    Sub_Code.Error_Log_Write(e1.Message);
                    Message_Feed_Out("エラーが発生しました。");
                }
                IsBusy = false;
            }
            sfd.Dispose();
        }
        private void Clear_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy || Selected_File == "")
            {
                return;
            }
            MessageBoxResult result = MessageBox.Show("クリアしますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                Selected_File = "";
                Project_Name_T.Text = "";
                Now_Stream_Count = 0;
                Max_Stream_Count = 0;
                BNK_FSB_Voices.Clear();
                Need_Files.Clear();
                Voices_L.Items.Clear();
                Voices_L.Items.Add("音声ファイルが選択されていません。");
                File_Name_T.Text = "ファイル名:未選択";
                Message_Feed_Out("内容をクリアしました。");
            }
        }
        private void Details_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsBusy)
            {
                return;
            }
            if (BNK_FSB_Voices.Count == 0)
            {
                Message_Feed_Out("音声ファイルが指定されていないため、表示できる情報がありません。");
                return;
            }
            Message_Feed_Out("ファイル内の音声ファイル数:" + Max_Stream_Count + "\n" + "使用できる音声数:" + Now_Stream_Count);
        }
    }
}