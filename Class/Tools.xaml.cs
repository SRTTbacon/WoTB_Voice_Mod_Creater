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
        int Voice_Max_Index = 0;
        float Volume = 1f;
        float Pitch = 0f;
        bool IsBusy = false;
        bool IsMessageShowing = false;
        bool IsOpenDialog = false;
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
            Opacity = 0;
            Visibility = Visibility.Visible;
            while (Opacity < 1 && !IsBusy)
            {
                Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
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
                    DVPL.DVPL_UnPack(File_Path, Path.GetDirectoryName(File_Path) + "/" + Path.GetFileNameWithoutExtension(File_Path), DVPL_Delete_C.IsChecked.Value);
                    if (!File.Exists(Path.GetDirectoryName(File_Path) + "/" + Path.GetFileNameWithoutExtension(File_Path)))
                        Error_Path += File_Path + "\n";
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
                        DVPL.DVPL_Pack(File_Path, File_Path + ".dvpl", DVPL_Delete_C.IsChecked.Value);
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