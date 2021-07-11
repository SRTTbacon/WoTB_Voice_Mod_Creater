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
using WoTB_Voice_Mod_Creater.Wwise_Class;

namespace WoTB_Voice_Mod_Creater.Class
{
    public partial class Tools : System.Windows.Controls.UserControl
    {
        readonly List<string> File_Full_Path = new List<string>();
        string BGM_Dir = "";
        string BGM_Dir_Now = "";
        int Voice_Max_Index = 0;
        float Volume = 1f;
        float Pitch = 0f;
        bool IsBusy = false;
        bool IsMessageShowing = false;
        Cauldron.FMOD.EVENT_LOADINFO ELI = new Cauldron.FMOD.EVENT_LOADINFO();
        Cauldron.FMOD.EventProject EP = new Cauldron.FMOD.EventProject();
        Cauldron.FMOD.EventGroup EG = new Cauldron.FMOD.EventGroup();
        Cauldron.FMOD.Event FE = new Cauldron.FMOD.Event();
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
                    using (var eifs = new FileStream(Voice_Set.Special_Path + "/Configs/Tools.conf", FileMode.Open, FileAccess.Read))
                    {
                        using (var eofs = new FileStream(Voice_Set.Special_Path + "/Configs/Temp_Tools.tmp", FileMode.Create, FileAccess.Write))
                        {
                            FileEncode.FileEncryptor.Decrypt(eifs, eofs, "Tools_Configs_Save");
                        }
                    }
                    StreamReader str = new StreamReader(Voice_Set.Special_Path + "/Configs/Temp_Tools.tmp");
                    Volume_S.Value = double.Parse(str.ReadLine());
                    str.Close();
                    File.Delete(Voice_Set.Special_Path + "/Configs/Temp_Tools.tmp");
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show("設定を読み込めませんでした。。\nエラー回避のため設定は削除されます。");
                    File.Delete(Voice_Set.Special_Path + "/Configs/Tools.conf");
                    Volume_S.Value = 75;
                    Sub_Code.Error_Log_Write(e.Message);
                }
            }
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
            {
                return;
            }
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
                    {
                        Error_Path += File_Path + "\n";
                    }
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
            {
                return;
            }
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
                        {
                            throw new Exception(".dvplファイルが作成できていません。");
                        }
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
                BGM_Volume_Set_C.IsChecked = false;
                BGM_List.Items.Clear();
                File_Full_Path.Clear();
                IsBusy = false;
            }
        }
        //BGMファイルをリストに追加
        private void BGM_Add_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
            if (BGM_List.Items.Count >= 20)
            {
                Message_Feed_Out("BGMを20個以上にすることはできません。");
                return;
            }
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Title = "BGMファイルを選択してください。",
                Multiselect = true,
                Filter = "BGMファイル(*.wav;*.mp2;*.mp3;*.ogg;*.wma;*.asf;*.aif;*.aiff;*.flac)|*.wav;*.mp2;*.mp3;*.ogg;*.wma;*.asf;*.aif;*.aiff;*.flac"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string Error_Message = "";
                foreach (string FilePath in ofd.FileNames)
                {
                    if (BGM_List.Items.Count >= 20)
                    {
                        Message_T.Text = "BGMファイルを20個以上入れることはできません。";
                        break;
                    }
                    string FileName = Path.GetFileName(FilePath);
                    bool IsOK = true;
                    foreach (string Path_Now in BGM_List.Items)
                    {
                        if (Path_Now == FileName)
                        {
                            Error_Message += FilePath + "\n";
                            IsOK = false;
                            break;
                        }
                    }
                    if (IsOK)
                    {
                        File_Full_Path.Add(FilePath);
                        BGM_List.Items.Add(FileName);
                    }
                }
                if (Error_Message != "")
                {
                    System.Windows.MessageBox.Show("以下のファイルは追加されませんでした。\n" + Error_Message);
                }
            }
        }
        //リストからファイルを削除
        private void BGM_Delete_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
            if (BGM_List.SelectedIndex == -1)
            {
                System.Windows.MessageBox.Show("取消するファイル名を選択してください。");
                return;
            }
            int Select = BGM_List.SelectedIndex;
            BGM_List.SelectedIndex = -1;
            BGM_List.Items.RemoveAt(Select);
            File_Full_Path.RemoveAt(Select);
        }
        //BGMファイル(.fev + .fsb)を作成
        private async void BGM_Create_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy || Opacity < 1)
            {
                return;
            }
            if (BGM_List.Items.Count == 0)
            {
                Message_Feed_Out("少なくとも1つはBGMファイルが必要です。");
                return;
            }
            string VoiceOver_Crew = "";
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Title = "voiceover_crew.bnkを選択してください。",
                Filter = "音声ファイル(voiceover_crew.bnk;voiceover_crew.bnk.dvpl)|voiceover_crew.bnk;voiceover_crew.bnk.dvpl",
                InitialDirectory = Voice_Set.WoTB_Path + "\\Data\\WwiseSound\\ja",
                Multiselect = false
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                VoiceOver_Crew = ofd.FileName;
            }
            else
            {
                return;
            }
            Message_T.Text = "音声ファイルを確認しています...";
            await Task.Delay(50);
            if (Path.GetExtension(ofd.FileName) == ".dvpl")
            {
                DVPL.DVPL_UnPack(ofd.FileName, Voice_Set.Special_Path + "/Temp_VoiceOver_Crew_01.bnk", false);
            }
            else
            {
                File.Copy(ofd.FileName, Voice_Set.Special_Path + "/Temp_VoiceOver_Crew_01.bnk", true);
            }
            //voiceover_crew.bnkが対応しているか確認
            Wwise_File_Extract_V2 Wwise = new Wwise_File_Extract_V2(Voice_Set.Special_Path + "/Temp_VoiceOver_Crew_01.bnk");
            List<string> Files2 = Wwise.Wwise_Get_Names();
            Wwise.Bank_Clear();
            bool IsUseThisSoftware = false;
            string WoTB_Language = "";
            if (Files2.Contains("500000000"))
            {
                IsUseThisSoftware = true;
            }
            if (!IsUseThisSoftware)
            {
                WoTB_Language = BGM_Create.Get_Voice_Language(Files2);
            }
            if (!IsUseThisSoftware && WoTB_Language == "")
            {
                Message_Feed_Out("指定したファイルは対応していません。詳しくは\"?\"ボタンを押して確認してください。");
                return;
            }
            int Start_Battle_Number = 0;
            if (IsUseThisSoftware)
            {
                //Start_Battleの要素数を取得
                int SetFirst = 500000000;
                while (true)
                {
                    if (!Files2.Contains((SetFirst).ToString()))
                    {
                        Start_Battle_Number = SetFirst - 500000000;
                        break;
                    }
                    SetFirst++;
                }
            }
            if (BGM_List.Items.Count > Start_Battle_Number)
            {
                System.Windows.MessageBox.Show("指定した音声ファイルのBGM数は最大" + Start_Battle_Number + "個です。");
                Message_Feed_Out("BGMの数を修正してください。");
                return;
            }
            else if (WoTB_Language != "")
            {
                System.Windows.MessageBox.Show("指定した音声ファイルのBGM数は最大8個です。");
                Message_Feed_Out("BGMの数を修正してください。");
                return;
            }
            IsBusy = true;
            string Path_Dir = Directory.GetCurrentDirectory();
            IsMessageShowing = true;
            string Temp;
            int Temp_01 = 1;
            //保存するフォルダを作成
            while (true)
            {
                if (Temp_01 < 10)
                {
                    if (!Directory.Exists(Path_Dir + "/Projects/BGM_Mod/BGM_0" + Temp_01))
                    {
                        Temp = "0" + Temp_01;
                        break;
                    }
                }
                else
                {
                    if (!Directory.Exists(Path_Dir + "/Projects/BGM_Mod/BGM_" + Temp_01))
                    {
                        Temp = Temp_01.ToString();
                        break;
                    }
                }
                Temp_01++;
            }
            if (BGM_Dir != "")
            {
                Temp = BGM_Dir;
            }
            try
            {
                if (Directory.Exists(Path_Dir + "/Projects/BGM_Mod/BGM_" + Temp + "/Music"))
                {
                    Directory.Delete(Path_Dir + "/Projects/BGM_Mod/BGM_" + Temp + "/Music", true);
                }
            }
            catch (Exception e1)
            {
                Sub_Code.Error_Log_Write(e1.Message);
                Message_Feed_Out("エラーが発生しました。");
                IsBusy = false;
                return;
            }
            await Task.Delay(100);
            //Music_*としてファイルをコピー
            foreach (string File_Now in File_Full_Path)
            {
                try
                {
                    Directory.CreateDirectory(Path_Dir + "/Projects/BGM_Mod/BGM_" + Temp + "/Music");
                    File.Copy(File_Now, Path_Dir + "/Projects/BGM_Mod/BGM_" + Temp + "/Music/" + Path.GetFileName(File_Now), true);
                }
                catch (Exception e1)
                {
                    Sub_Code.Error_Log_Write(e1.Message);
                    Message_Feed_Out("エラー:もう一度お試しください。");
                    IsBusy = false;
                    return;
                }
            }
            string[] Files = Directory.GetFiles(Path_Dir + "/Projects/BGM_Mod/BGM_" + Temp + "/Music", "*", SearchOption.TopDirectoryOnly);
            foreach (string File_Path in Files)
            {
                int Number = 1;
                while (true)
                {
                    if (Number < 10)
                    {
                        if (!Sub_Code.File_Exists(Path_Dir + "/Projects/BGM_Mod/BGM_" + Temp + "/Music/Music_0" + Number))
                        {
                            try
                            {
                                File.Move(File_Path, Path_Dir + "/Projects/BGM_Mod/BGM_" + Temp + "/Music/Music_0" + Number + Path.GetExtension(File_Path));
                            }
                            catch (Exception e1)
                            {
                                Sub_Code.Error_Log_Write(e1.Message);
                                System.Windows.MessageBox.Show(Path.GetFileName(File_Path + "が存在しません。"));
                            }
                            break;
                        }
                    }
                    else
                    {
                        if (!Sub_Code.File_Exists(Path_Dir + "/Projects/BGM_Mod/BGM_" + Temp + "/Music/Music_" + Number))
                        {
                            try
                            {
                                File.Move(File_Path, Path_Dir + "/Projects/BGM_Mod/BGM_" + Temp + "/Music/Music_" + Number + Path.GetExtension(File_Path));
                            }
                            catch (Exception e1)
                            {
                                Sub_Code.Error_Log_Write(e1.Message);
                                System.Windows.MessageBox.Show(Path.GetFileName(File_Path + "が存在しません。"));
                            }
                            break;
                        }
                    }
                    Number++;
                }
            }
            if (BGM_Volume_Set_C.IsChecked.Value)
            {
                //音量を均一にする場合実行
                //MP3Gainを用いてすべての音量を100にする(Fmodで音量を下げるため問題ない)
                //BNKファイルでは音量を調整できないため音量を89%にする
                Message_T.Text = "音量を均一にしています...";
                await Task.Delay(50);
                await Sub_Code.Change_MP3_Encode(Path_Dir + "/Projects/BGM_Mod/BGM_" + Temp + "/Music", 0);
            }
            /*BGM_Create.BGM_Project_Create(Directory.GetCurrentDirectory() + "/Projects/BGM_Mod/BGM_" + Temp);
            StreamWriter stw2 = File.CreateText(Voice_Set.Special_Path + "/Fmod_Designer/BGM_Create.bat");
            stw2.Write("\"" + Voice_Set.Special_Path + "/Fmod_Designer/fmod_designercl.exe\" -pc -mp3 " + Directory.GetCurrentDirectory() + "/Projects/BGM_Mod/BGM_" + Temp + "/Music.fdp");
            stw2.Close();
            Process p2 = new Process();
            p2.StartInfo.FileName = Voice_Set.Special_Path + "/Fmod_Designer/BGM_Create.bat";
            p2.StartInfo.RedirectStandardOutput = true;
            p2.StartInfo.CreateNoWindow = true;
            p2.StartInfo.UseShellExecute = false;
            p2.Start();
            int Number_01 = 2;
            while (true)
            {
                if (Number_01 == 0)
                {
                    Message_T.Text = "作成中です。しばらくお待ちください.";
                }
                else if (Number_01 == 1)
                {
                    Message_T.Text = "作成中です。しばらくお待ちください..";
                }
                else if (Number_01 == 2)
                {
                    Message_T.Text = "作成中です。しばらくお待ちください...";
                    Number_01 = -1;
                }
                if (p2.HasExited)
                {
                    p2.Close();
                    break;
                }
                Number_01++;
                await Task.Delay(1000);
            }
            File.Delete(Voice_Set.Special_Path + "/Fmod_Designer/BGM_Create.bat");*/
            if (BGM_Mix_Set_C.IsChecked.Value)
            {
                Message_T.Text = "BGMを5分以上にしています...この作業は時間がかかる可能性があります。";
                await Task.Delay(50);
                BGM_Create.Set_Music_Mix_Random(Path_Dir + "/Projects/BGM_Mod/BGM_" + Temp + "/Music");
            }
            string[] Music_Files = Directory.GetFiles(Path_Dir + "/Projects/BGM_Mod/BGM_" + Temp + "/Music", "*", SearchOption.TopDirectoryOnly);
            int Music_Now = 1;
            foreach (string Music_File in Music_Files)
            {
                Message_T.Text = Music_Now + "個目のBGMファイルをエンコードしています...";
                await Task.Delay(50);
                Sub_Code.File_To_WEM(Music_File, Path_Dir + "/Projects/BGM_Mod/BGM_" + Temp + "/Music/" + Path.GetFileNameWithoutExtension(Music_File) + ".wem", true, true);
                Music_Now++;
            }
            Message_T.Text = "ファイル数を合わせています...";
            await Task.Delay(40);
            int Set_File_Number = 0;
            if (IsUseThisSoftware)
            {
                Set_File_Number = Start_Battle_Number;
            }
            else
            {
                Set_File_Number = 8;
            }
            Set_Music_Number(Path_Dir + "/Projects/BGM_Mod/BGM_" + Temp + "/Music", Set_File_Number);
            try
            {
                Message_T.Text = "ファイル名を変更しています...";
                await Task.Delay(40);
                Directory.CreateDirectory(Path_Dir + "/Projects/BGM_Mod/BGM_" + Temp + "/ja");
                string[] BGM_Files = Directory.GetFiles(Path_Dir + "/Projects/BGM_Mod/BGM_" + Temp + "/Music", "*", SearchOption.TopDirectoryOnly);
                foreach (string BGM_File_Now in BGM_Files)
                {
                    Sub_Code.File_Rename_Number(BGM_File_Now, Path_Dir + "/Projects/BGM_Mod/BGM_" + Temp + "/Music/start_battle");
                }
                Message_T.Text = "Modファイルを作成しています...";
                await Task.Delay(50);
                Wwise_Class.Wwise_File_Extract_V2 Wwise_Bnk = new Wwise_Class.Wwise_File_Extract_V2(Voice_Set.Special_Path + "/Temp_VoiceOver_Crew_01.bnk");
                List<string> File_IDs = Wwise_Bnk.Wwise_Get_Names();
                string[] Music_Files_WEM = Directory.GetFiles(Path_Dir + "/Projects/BGM_Mod/BGM_" + Temp + "/Music", "*.wem", SearchOption.TopDirectoryOnly);
                if (IsUseThisSoftware)
                {
                    for (int Number = 0; Number < Music_Files_WEM.Length; Number++)
                    {
                        for (int Number_01 = 0; Number_01 < File_IDs.Count; Number_01++)
                        {
                            if (File_IDs[Number_01] == (500000000 + Number).ToString())
                            {
                                Wwise_Bnk.Bank_Edit_Sound(Number_01, Music_Files_WEM[Number], false);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    List<string> WoTB_IDs = Sub_Code.Get_Voices_ID(WoTB_Language);
                    for (int Number = 0; Number < Music_Files_WEM.Length; Number++)
                    {
                        Music_Files_WEM[Number] = Music_Files_WEM[Number].Replace(".wem", ".wav");
                        string File_ID_Now = "";
                        for (int Number_01 = 0; Number_01 < WoTB_IDs.Count; Number_01++)
                        {
                            string Name = WoTB_IDs[Number_01].Substring(0, WoTB_IDs[Number_01].IndexOf('|'));
                            if (Name == Path.GetFileName(Music_Files_WEM[Number]))
                            {
                                File_ID_Now = WoTB_IDs[Number_01].Substring(WoTB_IDs[Number_01].IndexOf('|') + 1);
                                break;
                            }
                        }
                        if (File_ID_Now != "")
                        {
                            for (int Number_01 = 0; Number_01 < File_IDs.Count; Number_01++)
                            {
                                if (File_IDs[Number_01] == File_ID_Now)
                                {
                                    Sub_Code.File_Move(Music_Files_WEM[Number].Replace(".wav", ".wem"), Path.GetDirectoryName(Music_Files_WEM[Number]) + "/" + Number_01 + ".wem", true);
                                    Wwise_Bnk.Bank_Edit_Sound(Number_01, Path.GetDirectoryName(Music_Files_WEM[Number]) + "/" + Number_01 + ".wem", false);
                                    break;
                                }
                            }
                        }
                    }
                }
                Wwise_Bnk.Bank_Save(Path_Dir + "/Projects/BGM_Mod/BGM_" + Temp + "/ja/voiceover_crew.bnk");
                Wwise_Bnk.Bank_Clear();
            }
            catch (Exception e1)
            {
                Message_Feed_Out("エラーが発生しました。");
                Sub_Code.Error_Log_Write(e1.Message);
                IsBusy = false;
                return;
            }
            if (BGM_Encode_Set_C.IsChecked.Value)
            {
                Message_T.Text = "DVPL化しています...";
                await Task.Delay(50);
                string Dir = Path_Dir + "/Projects/BGM_Mod/BGM_" + Temp;
                //DVPL.DVPL_Pack(Dir + "/Music.fev", Dir + "/Music.fev.dvpl", true);
                //DVPL.DVPL_Pack(Dir + "/Music.fsb", Dir + "/Music.fsb.dvpl", true);
                DVPL.DVPL_Pack(Dir + "/ja/voiceover_crew.bnk", Dir + "/ja/voiceover_crew.bnk.dvpl", true);
            }
            Message_T.Text = "BGMファイルを作成しました。ダイアログを開いています。";
            StreamWriter stw3 = new StreamWriter(Voice_Set.Special_Path + "/Temp_BGM_Create_Project.dat");
            foreach (string Line in File_Full_Path)
            {
                stw3.WriteLine(Line);
            }
            stw3.Close();
            using (var eifs = new FileStream(Voice_Set.Special_Path + "/Temp_BGM_Create_Project.dat", FileMode.Open, FileAccess.Read))
            {
                using (var eofs = new FileStream(Path_Dir + "/Projects/BGM_Mod/BGM_" + Temp + "/Save.vcb", FileMode.Create, FileAccess.Write))
                {
                    FileEncode.FileEncryptor.Encrypt(eifs, eofs, "WoTB_BGM_Create_Save");
                }
            }
            File.Delete(Voice_Set.Special_Path + "/Temp_BGM_Create_Project.dat");
            MessageBoxResult result = System.Windows.MessageBox.Show("BGMModファイルを作成しました。WoTBに導入しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    BGM_Mod_Install(Temp);
                    BGM_Dir_Now = Temp;
                }
                catch (Exception e1)
                {
                    Message_Feed_Out("エラー:BGMModをWoTBに適応できませんでした。");
                    Sub_Code.Error_Log_Write(e1.Message);
                }
            }
            else
            {
                Message_Feed_Out("BGMファイルを作成しました。");
            }
            IsBusy = false;
        }
        //ロード
        private void BGM_Load_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
            string SetDir;
            if (Directory.Exists(Directory.GetCurrentDirectory() + "/Projects/BGM_Mod"))
            {
                SetDir = Directory.GetCurrentDirectory() + "\\Projects\\BGM_Mod";
            }
            else
            {
                SetDir = Directory.GetCurrentDirectory();
            }
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Title = "Save.vcbを選択してください。",
                Multiselect = false,
                InitialDirectory = SetDir,
                CheckFileExists = true,
                FileName = "Save.vcb",
                RestoreDirectory = true,
                Filter = "セーブファイル (Save.vcb)|Save.vcb"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                using (var eifs = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read))
                {
                    using (var eofs = new FileStream(Voice_Set.Special_Path + "/Temp_BGM_Save_Decode.dat", FileMode.Create, FileAccess.Write))
                    {
                        FileEncode.FileEncryptor.Decrypt(eifs, eofs, "WoTB_BGM_Create_Save");
                    }
                }
                try
                {
                    StreamReader str = new StreamReader(Voice_Set.Special_Path + "/Temp_BGM_Save_Decode.dat");
                    File_Full_Path.Clear();
                    BGM_List.Items.Clear();
                    string line;
                    while ((line = str.ReadLine()) != null)
                    {
                        if (line != "")
                        {
                            File_Full_Path.Add(line);
                            BGM_List.Items.Add(line.Substring(line.LastIndexOf('\\') + 1));
                        }
                    }
                    str.Close();
                    File.Delete(Voice_Set.Special_Path + "/Temp_BGM_Save_Decode.dat");
                    string Temp_Dir = Path.GetDirectoryName(ofd.FileName).Substring(0, ofd.FileName.LastIndexOf('\\'));
                    string Dir = Temp_Dir.Substring(Temp_Dir.LastIndexOf('\\'));
                    BGM_Dir = Dir.Substring(Dir.LastIndexOf('_') + 1);
                }
                catch (Exception e1)
                {
                    Message_Feed_Out("データをロードできませんでした。");
                    Sub_Code.Error_Log_Write(e1.Message);
                    File_Full_Path.Clear();
                    BGM_List.Items.Clear();
                }
            }
        }
        //セーブ
        private async void BGM_Install_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy || Opacity < 1)
            {
                return;
            }
            IsBusy = true;
            string AA = Directory.GetCurrentDirectory();
            if (BGM_Dir != "")
            {
                BGM_Dir_Now = BGM_Dir;
            }
            /*if (BGM_Dir_Now == "" || !File.Exists(AA + "/Projects/BGM_Mod/BGM_" + BGM_Dir_Now + "/Music.fev") || !File.Exists(AA + "/Projects/BGM_Mod/BGM_" + BGM_Dir_Now + "/Music.fsb"))
            {
                if (BGM_Dir_Now == "" || !File.Exists(AA + "/Projects/BGM_Mod/BGM_" + BGM_Dir_Now + "/Music.fev.dvpl") || !File.Exists(AA + "/Projects/BGM_Mod/BGM_" + BGM_Dir_Now + "/Music.fsb.dvpl"))
                {
                    Message_Feed_Out("データを取得できませんでした。先に\"FEV + FSBを作成\"ボタンを押してください。");
                    return;
                }
            }*/
            if (BGM_Dir_Now == "" || !File.Exists(AA + "/Projects/BGM_Mod/BGM_" + BGM_Dir_Now + "/ja/voiceover_crew.bnk"))
            {
                if (BGM_Dir_Now == "" || !File.Exists(AA + "/Projects/BGM_Mod/BGM_" + BGM_Dir_Now + "/ja/voiceover_crew.bnk.dvpl"))
                {
                    Message_Feed_Out("データを取得できませんでした。先に\"作成\"ボタンを押してください。");
                    IsBusy = false;
                    return;
                }
            }
            Message_T.Text = "インストールしています...";
            await Task.Delay(50);
            BGM_Mod_Install(BGM_Dir_Now);
            IsBusy = false;
            Message_T.Text = "完了しました。";
        }
        //作成したBGMのModファイルをWoTBに導入(自動でsfx_high(low)やsounds.yamlに記述される)
        void BGM_Mod_Install(string Install_From_Dir)
        {
            if (IsBusy)
            {
                return;
            }
            if (Voice_Set.WoTB_Path == "")
            {
                Sub_Code.WoTB_Get_Directory();
                Message_Feed_Out("WoTBのインストール場所を取得できませんでした。");
                return;
            }
            /*if (!Directory.Exists(Voice_Set.WoTB_Path + "/Data/Mods"))
            {
                Directory.CreateDirectory(Voice_Set.WoTB_Path + "/Data/Mods");
            }
            try
            {
                Sub_Code.DVPL_File_Copy(Voice_Set.WoTB_Path + "/Data/Mods/Music.fev", Voice_Set.Special_Path + "back_Music.fev", true);
                Sub_Code.DVPL_File_Copy(Voice_Set.WoTB_Path + "/Data/Mods/Music.fsb", Voice_Set.Special_Path + "back_Music.fsb", true);
                if (File.Exists(Directory.GetCurrentDirectory() + "/Projects/BGM_Mod/BGM_" + Install_From_Dir + "/Music.fev"))
                {
                    File.Copy(Directory.GetCurrentDirectory() + "/Projects/BGM_Mod/BGM_" + Install_From_Dir + "/Music.fev", Voice_Set.WoTB_Path + "/Data/Mods/Music.fev", true);
                    File.Copy(Directory.GetCurrentDirectory() + "/Projects/BGM_Mod/BGM_" + Install_From_Dir + "/Music.fsb", Voice_Set.WoTB_Path + "/Data/Mods/Music.fsb", true);
                }
                else if (File.Exists(Directory.GetCurrentDirectory() + "/Projects/BGM_Mod/BGM_" + Install_From_Dir + "/Music.fev.dvpl"))
                {
                    File.Copy(Directory.GetCurrentDirectory() + "/Projects/BGM_Mod/BGM_" + Install_From_Dir + "/Music.fev.dvpl", Voice_Set.WoTB_Path + "/Data/Mods/Music.fev.dvpl", true);
                    File.Copy(Directory.GetCurrentDirectory() + "/Projects/BGM_Mod/BGM_" + Install_From_Dir + "/Music.fsb.dvpl", Voice_Set.WoTB_Path + "/Data/Mods/Music.fsb.dvpl", true);
                }
                else
                {
                    throw new Exception("FEV + FSBファイルが存在しません。");
                }
            }
            catch (Exception e1)
            {
                Message_Feed_Out("エラー:正しく反映されませんでした。");
                Sub_Code.Error_Log_Write(e1.Message);
                return;
            }
            try
            {
                Sub_Code.DVPL_File_Copy(Voice_Set.WoTB_Path + "/Data/sounds.yaml", Voice_Set.Special_Path + "/back_sounds.yaml", true);
                Sub_Code.DVPL_File_Copy(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_high.yaml", Voice_Set.Special_Path + "/back_sfx_high.yaml", true);
                Sub_Code.DVPL_File_Copy(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_low.yaml", Voice_Set.Special_Path + "/back_sfx_low.yaml", true);
                string Delete_Sounds = "";
                string Delete_Sfx_High_Low = "";
                //.yamlと.yaml.dvplが同じ場所にあるとうまく動作しない可能性があるためどちらかを削除するダイアログを表示
                if (File.Exists(Voice_Set.WoTB_Path + "/Data/sounds.yaml.dvpl") && File.Exists(Voice_Set.WoTB_Path + "/Data/sounds.yaml"))
                {
                    string Message_01 = "sounds.yamlとsounds.yaml.dvplの2つが同じフォルダにあります。正常に動作しない可能性があるためどちらかを削除します。\n";
                    string Message_02 = "sounds.yamlを削除する場合は\"はい\"、sounds.yaml.dvplを削除する場合は\"いいえ\",操作を取り消す場合は\"キャンセル\"を押してください。";
                    MessageBoxResult result1 = System.Windows.MessageBox.Show(Message_01 + Message_02, "確認", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation, MessageBoxResult.No);
                    if (result1 == MessageBoxResult.Yes)
                    {
                        Delete_Sounds = Voice_Set.WoTB_Path + "/Data/sounds.yaml";
                    }
                    else if (result1 == MessageBoxResult.No)
                    {
                        Delete_Sounds = Voice_Set.WoTB_Path + "/Data/sounds.yaml.dvpl";
                    }
                    else if (result1 == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                }
                if (File.Exists(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_high.yaml.dvpl") && File.Exists(Voice_Set.WoTB_Path + "/Configs/Sfx/sfx_high.yaml"))
                {
                    string Message_01 = "sfx_high(low).yamlとsfx_high(low).yaml.dvplの2つが同じフォルダにあります。正常に動作しない可能性があるためどちらかを削除します。\n";
                    string Message_02 = "sfx_high(low).yamlを削除する場合は\"はい\"、sfx_high(low).yaml.dvplを削除する場合は\"いいえ\",操作を取り消す場合は\"キャンセル\"を押してください。";
                    MessageBoxResult result1 = System.Windows.MessageBox.Show(Message_01 + Message_02, "確認", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation, MessageBoxResult.No);
                    if (result1 == MessageBoxResult.Yes)
                    {
                        Delete_Sfx_High_Low = "yaml";
                    }
                    else if (result1 == MessageBoxResult.No)
                    {
                        Delete_Sfx_High_Low = "dvpl";
                    }
                    else if (result1 == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                }
                if (Delete_Sounds != "")
                {
                    File.Delete(Delete_Sounds);
                }
                if (Delete_Sfx_High_Low != "")
                {
                    if (Delete_Sfx_High_Low == "yaml")
                    {
                        File.Delete(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_high.yaml");
                        File.Delete(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_low.yaml");
                    }
                    else if (Delete_Sfx_High_Low == "dvpl")
                    {
                        File.Delete(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_high.yaml.dvpl");
                        File.Delete(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_low.yaml.dvpl");
                    }
                }
                //.dvplのままでは記述できないためいったんdvplを解除させる
                if (File.Exists(Voice_Set.WoTB_Path + "/Data/sounds.yaml.dvpl"))
                {
                    DVPL.DVPL_UnPack(Voice_Set.WoTB_Path + "/Data/sounds.yaml.dvpl", Voice_Set.WoTB_Path + "/Data/sounds.yaml", true);
                }
                bool IsSFXDVPL = false;
                if (File.Exists(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_high.yaml.dvpl"))
                {
                    DVPL.DVPL_UnPack(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_high.yaml.dvpl", Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_high.yaml", true);
                    DVPL.DVPL_UnPack(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_low.yaml.dvpl", Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_low.yaml", true);
                    IsSFXDVPL = true;
                }
                //VOICE_START_BATTLEにBGMの構成を記述
                StreamReader str = new StreamReader(Voice_Set.WoTB_Path + "/Data/sounds.yaml");
                string[] Lines = str.ReadToEnd().Split('\n');
                str.Close();
                string Temp_02 = "";
                foreach (string Line in Lines)
                {
                    if (Line.Contains("VOICE_START_BATTLE"))
                    {
                        Temp_02 += "    VOICE_START_BATTLE: \"Music/Music/Music\"\n";
                        continue;
                    }
                    Temp_02 += Line + "\n";
                }
                //Music.fevがなければ追加
                StreamWriter stw = File.CreateText(Voice_Set.WoTB_Path + "/Data/sounds.yaml");
                stw.Write(Temp_02);
                stw.Close();
                string[] Configs = { "sfx_high.yaml", "sfx_low.yaml" };
                foreach (string File_Now in Configs)
                {
                    StreamReader str2 = new StreamReader(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/" + File_Now);
                    string[] Read = str2.ReadToEnd().Split('\n');
                    str2.Close();
                    bool IsExist = false;
                    foreach (string Line in Read)
                    {
                        if (Line.Contains("Music.fev"))
                        {
                            IsExist = true;
                        }
                    }
                    if (!IsExist)
                    {
                        StreamWriter stw4 = new StreamWriter(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/" + File_Now, true);
                        stw4.Write("\n -\n  \"~res:/Mods/Music.fev\"");
                        stw4.Close();
                    }
                }
                //もともとdvplだった場合dvplに戻す
                if (IsSFXDVPL)
                {
                    DVPL.DVPL_Pack(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_high.yaml", Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_high.yaml.dvpl", true);
                    DVPL.DVPL_Pack(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_low.yaml", Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_low.yaml.dvpl", true);
                    DVPL.DVPL_Pack(Voice_Set.WoTB_Path + "/Data/sounds.yaml", Voice_Set.WoTB_Path + "/Data/sounds.yaml.dvpl", true);
                }
                Sub_Code.DVPL_File_Delete(Voice_Set.WoTB_Path + "/back_sounds.yaml");
                Sub_Code.DVPL_File_Delete(Voice_Set.WoTB_Path + "/back_sfx_high.yaml");
                Sub_Code.DVPL_File_Delete(Voice_Set.WoTB_Path + "/back_sfx_low.yaml");
                Sub_Code.DVPL_File_Delete(Voice_Set.WoTB_Path + "/back_Music.fev");
                Sub_Code.DVPL_File_Delete(Voice_Set.WoTB_Path + "/back_Music.fsb");
                Message_Feed_Out("WoTBに適応しました。起動して確認してください。");
            }
            catch (Exception e1)
            {
                Message_Feed_Out("エラー:sounds.yamlかsfx_high(low).yamlに問題が発生しました。");
                Sub_Code.DVPL_File_Delete(Voice_Set.WoTB_Path + "/Data/Mods/Music.fev");
                Sub_Code.DVPL_File_Delete(Voice_Set.WoTB_Path + "/Data/Mods/Music.fsb");
                Sub_Code.DVPL_File_Copy(Voice_Set.Special_Path + "/back_sounds.yaml", Voice_Set.WoTB_Path + "/Data/sounds.yaml", true);
                Sub_Code.DVPL_File_Copy(Voice_Set.Special_Path + "/back_sfx_high.yaml", Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_high.yaml", true);
                Sub_Code.DVPL_File_Copy(Voice_Set.Special_Path + "/back_sfx_low.yaml", Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_low.yaml", true);
                Sub_Code.DVPL_File_Copy(Voice_Set.Special_Path + "/back_Music.fev", Voice_Set.WoTB_Path + "/Data/Mods/Music.fev", true);
                Sub_Code.DVPL_File_Copy(Voice_Set.Special_Path + "/back_Music.fsb", Voice_Set.WoTB_Path + "/Data/Mods/Music.fsb", true);
                Sub_Code.Error_Log_Write(e1.Message);
            }*/
            //BNKファイルをコピー(Modのインストール)
            DateTime dt = DateTime.Now;
            string Time = Sub_Code.Get_Time_Now(dt, ".", 1, 6);
            string Dir_Path = Directory.GetCurrentDirectory();
            Directory.CreateDirectory(Dir_Path + "/Backup/" + Time);
            Sub_Code.DVPL_File_Copy(Voice_Set.WoTB_Path + "/Data/WwiseSound/ja/voiceover_crew.bnk", Dir_Path + "/Backup/" + Time + "/voiceover_crew.bnk", true);
            try
            {
                File.Delete(Voice_Set.WoTB_Path + "/Data/WwiseSound/ja/voiceover_crew.bnk");
                File.Delete(Voice_Set.WoTB_Path + "/Data/WwiseSound/ja/voiceover_crew.bnk.dvpl");
                Sub_Code.DVPL_File_Copy(Dir_Path + "/Projects/BGM_Mod/BGM_" + Install_From_Dir + "/ja/voiceover_crew.bnk", Voice_Set.WoTB_Path + "/Data/WwiseSound/ja/voiceover_crew.bnk", true);
                Message_Feed_Out("WoTBに適応しました。起動して確認してください。");
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                Message_Feed_Out("エラーが発生したためインストールできませんでした。");
            }
        }
        private void BGM_List_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BGM_List.SelectedIndex = -1;
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
                {
                    Message_T.Opacity -= 0.025;
                }
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
            {
                return;
            }
            string Message_01 = "このバージョンではWoTB内のData/Sfxに入っている*.fev.dvplと*.fsb.dvplを解除できない可能性があります。\n";
            string Message_02 = "また、WoTB内の他のファイルも解除できないかもしれません。(sounds.yaml.dvplやマップファイルは解除できました。)";
            System.Windows.MessageBox.Show(Message_01 + Message_02);
        }
        private void FEV_Select_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
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
                    EP.Release();
                    FE.Release();
                    Fmod_Player.ESystem.Load(ofd.FileName, ref ELI, ref EP);
                    Cauldron.FMOD.EventProject EC = new Cauldron.FMOD.EventProject();
                    Fmod_Player.ESystem.GetProjectByIndex(0, ref EC);
                    EC.GetGroupByIndex(0, true, ref EG);
                    EG.GetNumEvents(ref Voice_Max_Index);
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
            {
                return;
            }
            if (FE != null)
            {
                FE.Stop();
            }
            EG.GetEventByIndex(Voice_Number, Cauldron.FMOD.EVENT_MODE.DEFAULT, ref FE);
            FE.SetVolume(Volume);
            FE.SetPitch(Pitch, Cauldron.FMOD.EVENT_PITCHUNITS.TONES);
            FE.Start();
        }
        private void FEV_Stop_B_Click(object sender, RoutedEventArgs e)
        {
            FE.Stop();
        }
        private void FEV_Play_B_Click(object sender, RoutedEventArgs e)
        {
            if (!IsBusy)
            {
                Set_Fmod_Bank_Play((int)FEV_Index_S.Value);
            }
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
            {
                FE.SetPitch(Pitch, Cauldron.FMOD.EVENT_PITCHUNITS.TONES);
            }
        }
        private void Volume_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //音量を変更(ダブルクリックで初期化)
            Volume = (float)Volume_S.Value / 100;
            Volume_T.Text = "音量:" + (int)Volume_S.Value;
            if (FE != null)
            {
                FE.SetVolume(Volume);
            }
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
                using (var eifs = new FileStream(Voice_Set.Special_Path + "/Configs/Tools.tmp", FileMode.Open, FileAccess.Read))
                {
                    using (var eofs = new FileStream(Voice_Set.Special_Path + "/Configs/Tools.conf", FileMode.Create, FileAccess.Write))
                    {
                        FileEncode.FileEncryptor.Encrypt(eifs, eofs, "Tools_Configs_Save");
                    }
                }
                File.Delete(Voice_Set.Special_Path + "/Configs/Tools.tmp");
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        private void DDS_B_Click(object sender, RoutedEventArgs e)
        {
            if (!IsBusy)
            {
                DDS_Tool_Window.Window_Show();
            }
        }
        private void FSB_Extract_B_Click(object sender, RoutedEventArgs e)
        {
            if (!IsBusy)
            {
                Fmod_Extract_Window.Window_Show();
            }
        }
        private void FSB_Create_B_Click(object sender, RoutedEventArgs e)
        {
            if (!IsBusy)
            {
                Fmod_Create_Window.Window_Show();
            }
        }
        private void BGM_Create_Help_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
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
                        {
                            File.Delete(Sub_Code.File_Get_FileName_No_Extension(Music_Dir + "/" + "Music_0" + Number2));
                        }
                        else
                        {
                            File.Delete(Sub_Code.File_Get_FileName_No_Extension(Music_Dir + "/" + "Music_" + Number2));
                        }
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
                        {
                            r3 = "0" + r2;
                        }
                        else
                        {
                            r3 = r2.ToString();
                        }
                        string FilePath = Sub_Code.File_Get_FileName_No_Extension(Music_Dir + "/" + "Music_" + r3);
                        if (Number2 < 10)
                        {
                            File.Copy(FilePath, Music_Dir + "/" + "Music_0" + Number2 + Path.GetExtension(FilePath), true);
                        }
                        else
                        {
                            File.Copy(FilePath, Music_Dir + "/" + "Music_" + Number2 + Path.GetExtension(FilePath), true);
                        }
                    }
                    catch
                    {
                        if (Test >= 10)
                        {
                            continue;
                        }
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
            if (IsBusy)
                return;
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
                    return;
                }
                Message_T.Text = ".dvplファイルを変換しています...";
                await Task.Delay(50);
                int Count = DVPL.DVPL_UnPack(DVPL_Files, DVPL_Delete_C.IsChecked.Value);
                Message_Feed_Out(Count + "個の.dvplファイルを変換しました。");
            }
            bfb.Dispose();
            IsBusy = false;
        }
        private async void DVPL_Create_Dir_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
                return;
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
            {
                File.Copy(BGM_File_Now, WoTB_Voice_Mod_Creater.Voice_Set.Special_Path + "/BGM_Temp/" + Path.GetFileName(BGM_File_Now), true);
            }
            string[] BGM_Files = DirectoryEx.GetFiles(WoTB_Voice_Mod_Creater.Voice_Set.Special_Path + "/BGM_Temp", SearchOption.TopDirectoryOnly, Ex);
            int File_Number_Now = -1;
            foreach (string BGM_File_Now in BGM_Files)
            {
                File_Number_Now++;
                int stream = Bass.BASS_StreamCreateFile(BGM_File_Now, 0, 0, BASSFlag.BASS_STREAM_DECODE);
                int length = (int)Bass.BASS_ChannelBytes2Seconds(stream, Bass.BASS_ChannelGetLength(stream));
                Bass.BASS_StreamFree(stream);
                if (length >= 300)
                {
                    continue;
                }
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
                    {
                        break;
                    }
                }
                StreamWriter stw = File.CreateText(WoTB_Voice_Mod_Creater.Voice_Set.Special_Path + "/Encode_Mp3/BGM_Mix.bat");
                stw.WriteLine("chcp 65001");
                stw.Write("\"" + WoTB_Voice_Mod_Creater.Voice_Set.Special_Path + "/Encode_Mp3/ffmpeg.exe\" -y ");
                foreach (int Number in Include_Numbers)
                {
                    stw.Write("-i \"" + BGM_Files[Number] + "\" ");
                }
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
        {
            return "English(US)";
        }
        else if (Files.Contains("148236242"))
        {
            return "arb";
        }
        else if (Files.Contains("522280109"))
        {
            return "gup";
        }
        else if (Files.Contains("897549921"))
        {
            return "pbr";
        }
        else if (Files.Contains("1003460716"))
        {
            return "vi";
        }
        else if (Files.Contains("348256779"))
        {
            return "tr";
        }
        else if (Files.Contains("270859153"))
        {
            return "th";
        }
        else if (Files.Contains("892963782"))
        {
            return "ru";
        }
        else if (Files.Contains("778579436"))
        {
            return "pl";
        }
        else if (Files.Contains("885986556"))
        {
            return "ko";
        }
        else if (Files.Contains("1056502513"))
        {
            return "ja";
        }
        else if (Files.Contains("549350697"))
        {
            return "it";
        }
        else if (Files.Contains("26760078"))
        {
            return "fr";
        }
        else if (Files.Contains("327412030"))
        {
            return "fi";
        }
        else if (Files.Contains("365235587"))
        {
            return "en";
        }
        else if (Files.Contains("286527395"))
        {
            return "es";
        }
        else if (Files.Contains("970759741"))
        {
            return "de";
        }
        else if (Files.Contains("513502435"))
        {
            return "cs";
        }
        else if (Files.Contains("586623688"))
        {
            return "cn";
        }
        else
        {
            return "";
        }
    }
    //.fdpプロジェクトを作成(長すぎて解説不可能)
    static string Set_Voice_By_Name(string File_Path)
    {
        string Temp = "<waveform>\n" +
                      "<filename>" + File_Path + "</filename>\n" +
                      "<guid>{53c7d2ab-a30c-4a64-af19-b93d10d76419}</guid>\n" +
                      "<mindistance>1</mindistance>\n" +
                      "<maxdistance>10000</maxdistance>\n" +
                      "<deffreq>44100</deffreq>\n" +
                      "<defvol>1</defvol>\n" +
                      "<defpan>0</defpan>\n" +
                      "<defpri>128</defpri>\n" +
                      "<xmafiltering>0</xmafiltering>\n" +
                      "<channelmode>0</channelmode>\n" +
                      "<quality_crossplatform>0</quality_crossplatform>\n" +
                      "<quality>-1</quality>\n" +
                      "<_PC_resamplemode>1</_PC_resamplemode>\n" +
                      "<_PC_optimisedratereduction>100</_PC_optimisedratereduction>\n" +
                      "<_PC_fixedsamplerate>48000</_PC_fixedsamplerate>\n" +
                      "<_XBOX360_resamplemode>1</_XBOX360_resamplemode>\n" +
                      "<_XBOX360_optimisedratereduction>100</_XBOX360_optimisedratereduction>\n" +
                      "<_XBOX360_fixedsamplerate>48000</_XBOX360_fixedsamplerate>\n" +
                      "<_XBOXONE_resamplemode>1</_XBOXONE_resamplemode>\n" +
                      "<_XBOXONE_optimisedratereduction>100</_XBOXONE_optimisedratereduction>\n" +
                      "<_XBOXONE_fixedsamplerate>48000</_XBOXONE_fixedsamplerate>\n" +
                      "<_PSP_resamplemode>1</_PSP_resamplemode>\n" +
                      "<_PSP_optimisedratereduction>100</_PSP_optimisedratereduction>\n" +
                      "<_PSP_fixedsamplerate>48000</_PSP_fixedsamplerate>\n" +
                      "<_PS3_resamplemode>1</_PS3_resamplemode>\n" +
                      "<_PS3_optimisedratereduction>100</_PS3_optimisedratereduction>\n" +
                      "<_PS3_fixedsamplerate>48000</_PS3_fixedsamplerate>\n" +
                      "<_PS4_resamplemode>1</_PS4_resamplemode>\n" +
                      "<_PS4_optimisedratereduction>100</_PS4_optimisedratereduction>\n" +
                      "<_PS4_fixedsamplerate>48000</_PS4_fixedsamplerate>\n" +
                      "<_WII_resamplemode>1</_WII_resamplemode>\n" +
                      "<_WII_optimisedratereduction>100</_WII_optimisedratereduction>\n" +
                      "<_WII_fixedsamplerate>48000</_WII_fixedsamplerate>\n" +
                      "<_WiiU_resamplemode>1</_WiiU_resamplemode>\n" +
                      "<_WiiU_optimisedratereduction>100</_WiiU_optimisedratereduction>\n" +
                      "<_WiiU_fixedsamplerate>48000</_WiiU_fixedsamplerate>\n" +
                      "<_3DS_resamplemode>1</_3DS_resamplemode>\n" +
                      "<_3DS_optimisedratereduction>100</_3DS_optimisedratereduction>\n" +
                      "<_3DS_fixedsamplerate>48000</_3DS_fixedsamplerate>\n" +
                      "<_NGP_resamplemode>1</_NGP_resamplemode>\n" +
                      "<_NGP_optimisedratereduction>100</_NGP_optimisedratereduction>\n" +
                      "<_NGP_fixedsamplerate>48000</_NGP_fixedsamplerate>\n" +
                      "<_ANDROID_resamplemode>1</_ANDROID_resamplemode>\n" +
                      "<_ANDROID_optimisedratereduction>100</_ANDROID_optimisedratereduction>\n" +
                      "<_ANDROID_fixedsamplerate>48000</_ANDROID_fixedsamplerate>\n" +
                      "<_IOS_resamplemode>1</_IOS_resamplemode>\n" +
                      "<_IOS_optimisedratereduction>100</_IOS_optimisedratereduction>\n" +
                      "<_IOS_fixedsamplerate>48000</_IOS_fixedsamplerate>\n" +
                      "<_BB10_resamplemode>1</_BB10_resamplemode>\n" +
                      "<_BB10_optimisedratereduction>100</_BB10_optimisedratereduction>\n" +
                      "<_BB10_fixedsamplerate>48000</_BB10_fixedsamplerate>\n" +
                      "<notes></notes>\n" +
                      "</waveform>\n";
        return Temp;
    }
    public static void BGM_Project_Create(string Dir)
    {
        StreamWriter stw = File.CreateText(Dir + "/Music.fdp");
        stw.Write("<project>\n" +
                  "<name>Music</name>\n" +
                  "<guid>{dc0f56db-d9d8-4cb4-af9f-722e1e67618e}</guid>\n" +
                  "<version>4</version>\n" +
                  "<eventgroup_nextid>0</eventgroup_nextid>\n" +
                  "<soundbank_nextid>3</soundbank_nextid>\n" +
                  "<sounddef_nextid>0</sounddef_nextid>\n" +
                  "<build_project>1</build_project>\n" +
                  "<build_headerfile>0</build_headerfile>\n" +
                  "<build_banklists>0</build_banklists>\n" +
                  "<build_programmerreport>0</build_programmerreport>\n" +
                  "<build_applytemplate>0</build_applytemplate>\n" +
                  "<build_continue_on_error>0</build_continue_on_error>\n" +
                  "<currentbank>Music</currentbank>\n" +
                  "<currentlanguage>default</currentlanguage>\n" +
                  "<primarylanguage>default</primarylanguage>\n" +
                  "<language>default</language>\n" +
                  "<templatefilename></templatefilename>\n" +
                  "<templatefileopen>1</templatefileopen>\n" +
                  "<eventcategory>\n" +
                  "<name>music</name>\n" +
                  "<guid>{5be365b9-6499-43dd-9dac-e8f39f005217}</guid>\n" +
                  "<volume_db>0</volume_db>\n" +
                  "<pitch>0</pitch>\n" +
                  "<maxplaybacks>0</maxplaybacks>\n" +
                  "<maxplaybacks_behavior>Steal_oldest</maxplaybacks_behavior>\n" +
                  "<notes></notes>\n" +
                  "<open>0</open>\n" +
                  "</eventcategory>\n" +
                  "<eventcategory>\n" +
                  "<name>ingame_voice</name>\n" +
                  "<guid>{8a085b14-0c02-441e-b4d4-d3d5a0cce8df}</guid>\n" +
                  "<volume_db>0</volume_db>\n" +
                  "<pitch>0</pitch>\n" +
                  "<maxplaybacks>0</maxplaybacks>\n" +
                  "<maxplaybacks_behavior>Steal_oldest</maxplaybacks_behavior>\n" +
                  "<notes></notes>\n" +
                  "<open>1</open>\n" +
                  "</eventcategory>\n" +
                  "<sounddeffolder>\n" +
                  "<name>master</name>\n" +
                  "<guid>{f3af6169-744a-4da0-baa6-fa0fdae22fab}</guid>\n" +
                  "<open>0</open>\n" +
                  "<sounddeffolder>\n" +
                  "<name>__simpleevent_sounddef__</name>\n" +
                  "<guid>{a50d8208-d703-42a8-908d-6cb63f8cc731}</guid>\n" +
                  "<open>0</open>\n" +
                  "</sounddeffolder>\n");
        List<string> Set_BGM_List = new List<string>();
        Set_BGM_List.AddRange(Directory.GetFiles(Dir + "/Music", "*", SearchOption.TopDirectoryOnly));
        stw.Write("<sounddef>\n" +
                          "<name>/Music</name>\n" +
                          "<guid>{c0e47b61-0077-4604-8b3c-a8017bfaf532}</guid>\n" +
                          "<type>randomnorepeat</type>\n" +
                          "<playlistmode>0</playlistmode>\n" +
                          "<randomrepeatsounds>0</randomrepeatsounds>\n" +
                          "<randomrepeatsilences>0</randomrepeatsilences>\n" +
                          "<shuffleglobal>0</shuffleglobal>\n" +
                          "<sequentialrememberposition>0</sequentialrememberposition>\n" +
                          "<sequentialglobal>0</sequentialglobal>\n" +
                          "<spawntime_min>1</spawntime_min>\n" +
                          "<spawntime_max>1</spawntime_max>\n" +
                          "<spawn_max>1</spawn_max>\n" +
                          "<mode>0</mode>\n" +
                          "<pitch>0</pitch>\n" +
                          "<pitch_randmethod>1</pitch_randmethod>\n" +
                          "<pitch_random_min>0</pitch_random_min>\n" +
                          "<pitch_random_max>0</pitch_random_max>\n" +
                          "<pitch_randomization>0</pitch_randomization>\n" +
                          "<pitch_recalculate>0</pitch_recalculate>\n" +
                          "<volume_db>0</volume_db>\n" +
                          "<volume_randmethod>1</volume_randmethod>\n" +
                          "<volume_random_min>0</volume_random_min>\n" +
                          "<volume_random_max>0</volume_random_max>\n" +
                          "<volume_randomization>0</volume_randomization>\n" +
                          "<position_randomization_min>0</position_randomization_min>\n" +
                          "<position_randomization>0</position_randomization>\n" +
                          "<trigger_delay_min>1</trigger_delay_min>\n" +
                          "<trigger_delay_max>1</trigger_delay_max>\n" +
                          "<spawncount>0</spawncount>\n" +
                          "<notes></notes>\n" +
                          "<entrylistmode>1</entrylistmode>\n");
        foreach (string Name_Now in Set_BGM_List)
        {
            stw.Write("<waveform>\n" +
              "<filename>" + Name_Now + "</filename>\n" +
              "<soundbankname>Music</soundbankname>\n" +
              "<weight>100</weight>\n" +
              "<percentagelocked>0</percentagelocked>\n" +
              "</waveform>\n");
        }
        stw.Write("</sounddef>\n" +
                  "</sounddeffolder>\n" +
                  "<eventgroup>\n" +
                  "<name>Music</name>\n" +
                  "<guid>{4681190d-f571-47c3-bbd6-38af16fa8b70}</guid>\n" +
                  "<eventgroup_nextid>0</eventgroup_nextid>\n" +
                  "<event_nextid>0</event_nextid>\n" +
                  "<open>1</open>\n" +
                  "<notes></notes>\n");
        stw.Write("<event>\n" +
          "<name>Music</name>\n" +
          "<guid>{39a991f5-ea43-40dc-ad7c-17638e4ed2d9}</guid>\n" +
          "<parameter_nextid>0</parameter_nextid>\n" +
          "<layer_nextid>2</layer_nextid>\n" +
          "<layer>\n" +
          "<name>layer00</name>\n" +
          "<height>100</height>\n" +
          "<envelope_nextid>0</envelope_nextid>\n" +
          "<mute>0</mute>\n" +
          "<solo>0</solo>\n" +
          "<soundlock>0</soundlock>\n" +
          "<envlock>0</envlock>\n" +
          "<priority>-1</priority>\n" +
          "<sound>\n" +
          "<name>/Music</name>\n" +
          "<x>0</x>\n" +
          "<width>1</width>\n" +
          "<startmode>0</startmode>\n" +
          "<loopmode>1</loopmode>\n" +
          "<loopcount2>-1</loopcount2>\n" +
          "<autopitchenabled>0</autopitchenabled>\n" +
          "<autopitchparameter>0</autopitchparameter>\n" +
          "<autopitchreference>0</autopitchreference>\n" +
          "<autopitchatzero>0</autopitchatzero>\n" +
          "<finetune>0</finetune>\n" +
          "<volume>1</volume>\n" +
          "<fadeintype>2</fadeintype>\n" +
          "<fadeouttype>2</fadeouttype>\n" +
          "</sound>\n" +
          "<_PC_enable>1</_PC_enable>\n" +
          "<_XBOX360_enable>1</_XBOX360_enable>\n" +
          "<_XBOXONE_enable>1</_XBOXONE_enable>\n" +
          "<_PS3_enable>1</_PS3_enable>\n" +
          "<_PS4_enable>1</_PS4_enable>\n" +
          "<_WII_enable>1</_WII_enable>\n" +
          "<_WiiU_enable>1</_WiiU_enable>\n" +
          "<_3DS_enable>1</_3DS_enable>\n" +
          "<_NGP_enable>1</_NGP_enable>\n" +
          "<_ANDROID_enable>1</_ANDROID_enable>\n" +
          "<_IOS_enable>1</_IOS_enable>\n" +
          "<_BB10_enable>1</_BB10_enable>\n" +
          "</layer>\n");
        stw.Write("<car_rpm>0</car_rpm>\n" +
                  "<car_rpmsmooth>0.075</car_rpmsmooth>\n" +
                  "<car_loadsmooth>0.05</car_loadsmooth>\n" +
                  "<car_loadscale>6</car_loadscale>\n" +
                  "<volume_db>-10</volume_db>\n" +
                  "<pitch>0</pitch>\n" +
                  "<pitch_units>Octaves</pitch_units>\n" +
                  "<pitch_randomization>0</pitch_randomization>\n" +
                  "<pitch_randomization_units>Octaves</pitch_randomization_units>\n" +
                  "<volume_randomization>0</volume_randomization>\n" +
                  "<priority>128</priority>\n" +
                  "<maxplaybacks>1</maxplaybacks>\n" +
                  "<maxplaybacks_behavior>Just_fail_if_quietest</maxplaybacks_behavior>\n" +
                  "<stealpriority>10000</stealpriority>\n" +
                  "<mode>x_2d</mode>\n" +
                  "<ignoregeometry>No</ignoregeometry>\n" +
                  "<rolloff>Logarithmic</rolloff>\n" +
                  "<mindistance>1</mindistance>\n" +
                  "<maxdistance>10000</maxdistance>\n" +
                  "<auto_distance_filtering>Off</auto_distance_filtering>\n" +
                  "<distance_filter_centre_freq>1500</distance_filter_centre_freq>\n" +
                  "<headrelative>World_relative</headrelative>\n" +
                  "<oneshot>Yes</oneshot>\n" +
                  "<istemplate>No</istemplate>\n" +
                  "<usetemplate></usetemplate>\n" +
                  "<notes></notes>\n" +
                  "<category>ingame_voice</category>\n" +
                  "<position_randomization_min>0</position_randomization_min>\n" +
                  "<position_randomization>0</position_randomization>\n" +
                  "<speaker_l>1</speaker_l>\n" +
                  "<speaker_c>0</speaker_c>\n" +
                  "<speaker_r>1</speaker_r>\n" +
                  "<speaker_ls>0</speaker_ls>\n" +
                  "<speaker_rs>0</speaker_rs>\n" +
                  "<speaker_lb>0</speaker_lb>\n" +
                  "<speaker_rb>0</speaker_rb>\n" +
                  "<speaker_lfe>0</speaker_lfe>\n" +
                  "<speaker_config>0</speaker_config>\n" +
                  "<speaker_pan_r>1</speaker_pan_r>\n" +
                  "<speaker_pan_theta>0</speaker_pan_theta>\n" +
                  "<cone_inside_angle>360</cone_inside_angle>\n" +
                  "<cone_outside_angle>360</cone_outside_angle>\n" +
                  "<cone_outside_volumedb>0</cone_outside_volumedb>\n" +
                  "<doppler_scale>1</doppler_scale>\n" +
                  "<reverbdrylevel_db>0</reverbdrylevel_db>\n" +
                  "<reverblevel_db>0</reverblevel_db>\n" +
                  "<speaker_spread>0</speaker_spread>\n" +
                  "<panlevel3d>1</panlevel3d>\n" +
                  "<fadein_time>0</fadein_time>\n" +
                  "<fadeout_time>1000</fadeout_time>\n" +
                  "<spawn_intensity>1</spawn_intensity>\n" +
                  "<spawn_intensity_randomization>0</spawn_intensity_randomization>\n" +
                  "<TEMPLATE_PROP_LAYERS>1</TEMPLATE_PROP_LAYERS>\n" +
                  "<TEMPLATE_PROP_KEEP_EFFECTS_PARAMS>1</TEMPLATE_PROP_KEEP_EFFECTS_PARAMS>\n" +
                  "<TEMPLATE_PROP_VOLUME>0</TEMPLATE_PROP_VOLUME>\n" +
                  "<TEMPLATE_PROP_PITCH>1</TEMPLATE_PROP_PITCH>\n" +
                  "<TEMPLATE_PROP_PITCH_RANDOMIZATION>1</TEMPLATE_PROP_PITCH_RANDOMIZATION>\n" +
                  "<TEMPLATE_PROP_VOLUME_RANDOMIZATION>1</TEMPLATE_PROP_VOLUME_RANDOMIZATION>\n" +
                  "<TEMPLATE_PROP_PRIORITY>1</TEMPLATE_PROP_PRIORITY>\n" +
                  "<TEMPLATE_PROP_MAX_PLAYBACKS>1</TEMPLATE_PROP_MAX_PLAYBACKS>\n" +
                  "<TEMPLATE_PROP_MAX_PLAYBACKS_BEHAVIOR>1</TEMPLATE_PROP_MAX_PLAYBACKS_BEHAVIOR>\n" +
                  "<TEMPLATE_PROP_STEAL_PRIORITY>1</TEMPLATE_PROP_STEAL_PRIORITY>\n" +
                  "<TEMPLATE_PROP_MODE>1</TEMPLATE_PROP_MODE>\n" +
                  "<TEMPLATE_PROP_IGNORE_GEOMETRY>1</TEMPLATE_PROP_IGNORE_GEOMETRY>\n" +
                  "<TEMPLATE_PROP_X_3D_ROLLOFF>1</TEMPLATE_PROP_X_3D_ROLLOFF>\n" +
                  "<TEMPLATE_PROP_X_3D_MIN_DISTANCE>1</TEMPLATE_PROP_X_3D_MIN_DISTANCE>\n" +
                  "<TEMPLATE_PROP_X_3D_MAX_DISTANCE>1</TEMPLATE_PROP_X_3D_MAX_DISTANCE>\n" +
                  "<TEMPLATE_PROP_X_3D_POSITION>1</TEMPLATE_PROP_X_3D_POSITION>\n" +
                  "<TEMPLATE_PROP_X_3D_MIN_POSITION_RANDOMIZATION>1</TEMPLATE_PROP_X_3D_MIN_POSITION_RANDOMIZATION>\n" +
                  "<TEMPLATE_PROP_X_3D_POSITION_RANDOMIZATION>1</TEMPLATE_PROP_X_3D_POSITION_RANDOMIZATION>\n" +
                  "<TEMPLATE_PROP_X_3D_CONE_INSIDE_ANGLE>1</TEMPLATE_PROP_X_3D_CONE_INSIDE_ANGLE>\n" +
                  "<TEMPLATE_PROP_X_3D_CONE_OUTSIDE_ANGLE>1</TEMPLATE_PROP_X_3D_CONE_OUTSIDE_ANGLE>\n" +
                  "<TEMPLATE_PROP_X_3D_CONE_OUTSIDE_VOLUME>1</TEMPLATE_PROP_X_3D_CONE_OUTSIDE_VOLUME>\n" +
                  "<TEMPLATE_PROP_X_3D_DOPPLER_FACTOR>1</TEMPLATE_PROP_X_3D_DOPPLER_FACTOR>\n" +
                  "<TEMPLATE_PROP_REVERB_WET_LEVEL>1</TEMPLATE_PROP_REVERB_WET_LEVEL>\n" +
                  "<TEMPLATE_PROP_REVERB_DRY_LEVEL>1</TEMPLATE_PROP_REVERB_DRY_LEVEL>\n" +
                  "<TEMPLATE_PROP_X_3D_SPEAKER_SPREAD>1</TEMPLATE_PROP_X_3D_SPEAKER_SPREAD>\n" +
                  "<TEMPLATE_PROP_X_3D_PAN_LEVEL>1</TEMPLATE_PROP_X_3D_PAN_LEVEL>\n" +
                  "<TEMPLATE_PROP_X_2D_SPEAKER_L>1</TEMPLATE_PROP_X_2D_SPEAKER_L>\n" +
                  "<TEMPLATE_PROP_X_2D_SPEAKER_C>1</TEMPLATE_PROP_X_2D_SPEAKER_C>\n" +
                  "<TEMPLATE_PROP_X_2D_SPEAKER_R>1</TEMPLATE_PROP_X_2D_SPEAKER_R>\n" +
                  "<TEMPLATE_PROP_X_2D_SPEAKER_LS>1</TEMPLATE_PROP_X_2D_SPEAKER_LS>\n" +
                  "<TEMPLATE_PROP_X_2D_SPEAKER_RS>1</TEMPLATE_PROP_X_2D_SPEAKER_RS>\n" +
                  "<TEMPLATE_PROP_X_2D_SPEAKER_LR>1</TEMPLATE_PROP_X_2D_SPEAKER_LR>\n" +
                  "<TEMPLATE_PROP_X_2D_SPEAKER_RR>1</TEMPLATE_PROP_X_2D_SPEAKER_RR>\n" +
                  "<TEMPLATE_PROP_X_SPEAKER_LFE>1</TEMPLATE_PROP_X_SPEAKER_LFE>\n" +
                  "<TEMPLATE_PROP_ONESHOT>1</TEMPLATE_PROP_ONESHOT>\n" +
                  "<TEMPLATE_PROP_FADEIN_TIME>1</TEMPLATE_PROP_FADEIN_TIME>\n" +
                  "<TEMPLATE_PROP_FADEOUT_TIME>1</TEMPLATE_PROP_FADEOUT_TIME>\n" +
                  "<TEMPLATE_PROP_NOTES>1</TEMPLATE_PROP_NOTES>\n" +
                  "<TEMPLATE_PROP_USER_PROPERTIES>1</TEMPLATE_PROP_USER_PROPERTIES>\n" +
                  "<TEMPLATE_PROP_CATEGORY>0</TEMPLATE_PROP_CATEGORY>\n" +
                  "<_PC_enabled>1</_PC_enabled>\n" +
                  "<_XBOX360_enabled>1</_XBOX360_enabled>\n" +
                  "<_XBOXONE_enabled>1</_XBOXONE_enabled>\n" +
                  "<_PSP_enabled>1</_PSP_enabled>\n" +
                  "<_PS3_enabled>1</_PS3_enabled>\n" +
                  "<_PS4_enabled>1</_PS4_enabled>\n" +
                  "<_WII_enabled>1</_WII_enabled>\n" +
                  "<_WiiU_enabled>1</_WiiU_enabled>\n" +
                  "<_3DS_enabled>1</_3DS_enabled>\n" +
                  "<_NGP_enabled>1</_NGP_enabled>\n" +
                  "<_ANDROID_enabled>1</_ANDROID_enabled>\n" +
                  "<_IOS_enabled>1</_IOS_enabled>\n" +
                  "<_BB10_enabled>1</_BB10_enabled>\n" +
                  "</event>\n");
        stw.Write("</eventgroup>\n" +
                  "<default_soundbank_props>\n" +
                  "<name>default_soundbank_props</name>\n" +
                  "<guid>{729237be-80e6-419f-8a3d-b9b35ceb2555}</guid>\n" +
                  "<load_into_rsx>0</load_into_rsx>\n" +
                  "<disable_seeking>0</disable_seeking>\n" +
                  "<enable_syncpoints>1</enable_syncpoints>\n" +
                  "<hasbuiltwithsyncpoints>0</hasbuiltwithsyncpoints>\n" +
                  "<_PC_banktype>DecompressedSample</_PC_banktype>\n" +
                  "<_XBOX360_banktype>DecompressedSample</_XBOX360_banktype>\n" +
                  "<_XBOXONE_banktype>DecompressedSample</_XBOXONE_banktype>\n" +
                  "<_PSP_banktype>DecompressedSample</_PSP_banktype>\n" +
                  "<_PS3_banktype>DecompressedSample</_PS3_banktype>\n" +
                  "<_PS4_banktype>DecompressedSample</_PS4_banktype>\n" +
                  "<_WII_banktype>DecompressedSample</_WII_banktype>\n" +
                  "<_WiiU_banktype>DecompressedSample</_WiiU_banktype>\n" +
                  "<_3DS_banktype>DecompressedSample</_3DS_banktype>\n" +
                  "<_NGP_banktype>DecompressedSample</_NGP_banktype>\n" +
                  "<_ANDROID_banktype>DecompressedSample</_ANDROID_banktype>\n" +
                  "<_IOS_banktype>DecompressedSample</_IOS_banktype>\n" +
                  "<_BB10_banktype>DecompressedSample</_BB10_banktype>\n" +
                  "<notes></notes>\n" +
                  "<_PC_format>PCM</_PC_format>\n" +
                  "<_PC_quality>50</_PC_quality>\n" +
                  "<_PC_forcesoftware>1</_PC_forcesoftware>\n" +
                  "<_PC_maxstreams>15</_PC_maxstreams>\n" +
                  "<_XBOX360_format>PCM</_XBOX360_format>\n" +
                  "<_XBOX360_quality>50</_XBOX360_quality>\n" +
                  "<_XBOX360_forcesoftware>1</_XBOX360_forcesoftware>\n" +
                  "<_XBOX360_maxstreams>10</_XBOX360_maxstreams>\n" +
                  "<_XBOXONE_format>PCM</_XBOXONE_format>\n" +
                  "<_XBOXONE_quality>50</_XBOXONE_quality>\n" +
                  "<_XBOXONE_forcesoftware>1</_XBOXONE_forcesoftware>\n" +
                  "<_XBOXONE_maxstreams>10</_XBOXONE_maxstreams>\n" +
                  "<_PSP_format>PCM</_PSP_format>\n" +
                  "<_PSP_quality>50</_PSP_quality>\n" +
                  "<_PSP_forcesoftware>0</_PSP_forcesoftware>\n" +
                  "<_PSP_maxstreams>10</_PSP_maxstreams>\n" +
                  "<_PS3_format>PCM</_PS3_format>\n" +
                  "<_PS3_quality>50</_PS3_quality>\n" +
                  "<_PS3_forcesoftware>1</_PS3_forcesoftware>\n" +
                  "<_PS3_maxstreams>10</_PS3_maxstreams>\n" +
                  "<_PS4_format>PCM</_PS4_format>\n" +
                  "<_PS4_quality>50</_PS4_quality>\n" +
                  "<_PS4_forcesoftware>1</_PS4_forcesoftware>\n" +
                  "<_PS4_maxstreams>10</_PS4_maxstreams>\n" +
                  "<_WII_format>PCM</_WII_format>\n" +
                  "<_WII_quality>50</_WII_quality>\n" +
                  "<_WII_forcesoftware>0</_WII_forcesoftware>\n" +
                  "<_WII_maxstreams>10</_WII_maxstreams>\n" +
                  "<_WiiU_format>PCM</_WiiU_format>\n" +
                  "<_WiiU_quality>50</_WiiU_quality>\n" +
                  "<_WiiU_forcesoftware>0</_WiiU_forcesoftware>\n" +
                  "<_WiiU_maxstreams>10</_WiiU_maxstreams>\n" +
                  "<_3DS_format>PCM</_3DS_format>\n" +
                  "<_3DS_quality>50</_3DS_quality>\n" +
                  "<_3DS_forcesoftware>0</_3DS_forcesoftware>\n" +
                  "<_3DS_maxstreams>10</_3DS_maxstreams>\n" +
                  "<_NGP_format>PCM</_NGP_format>\n" +
                  "<_NGP_quality>50</_NGP_quality>\n" +
                  "<_NGP_forcesoftware>0</_NGP_forcesoftware>\n" +
                  "<_NGP_maxstreams>10</_NGP_maxstreams>\n" +
                  "<_ANDROID_format>PCM</_ANDROID_format>\n" +
                  "<_ANDROID_quality>50</_ANDROID_quality>\n" +
                  "<_ANDROID_forcesoftware>1</_ANDROID_forcesoftware>\n" +
                  "<_ANDROID_maxstreams>10</_ANDROID_maxstreams>\n" +
                  "<_IOS_format>PCM</_IOS_format>\n" +
                  "<_IOS_quality>50</_IOS_quality>\n" +
                  "<_IOS_forcesoftware>1</_IOS_forcesoftware>\n" +
                  "<_IOS_maxstreams>10</_IOS_maxstreams>\n" +
                  "<_BB10_format>PCM</_BB10_format>\n" +
                  "<_BB10_quality>50</_BB10_quality>\n" +
                  "<_BB10_forcesoftware>0</_BB10_forcesoftware>\n" +
                  "<_BB10_maxstreams>10</_BB10_maxstreams>\n" +
                  "<_lang_default_filename_prefix></_lang_default_filename_prefix>\n" +
                  "<_lang_default_rebuild>0</_lang_default_rebuild>\n" +
                  "</default_soundbank_props>\n" +
                  "<soundbank>\n" +
                  "<name>Music</name>\n" +
                  "<guid>{95b3fc25-b5aa-498a-a13f-e0956e660954}</guid>\n" +
                  "<load_into_rsx>0</load_into_rsx>\n" +
                  "<disable_seeking>0</disable_seeking>\n" +
                  "<enable_syncpoints>1</enable_syncpoints>\n" +
                  "<hasbuiltwithsyncpoints>0</hasbuiltwithsyncpoints>\n" +
                  "<_PC_banktype>DecompressedSample</_PC_banktype>\n" +
                  "<_XBOX360_banktype>DecompressedSample</_XBOX360_banktype>\n" +
                  "<_XBOXONE_banktype>DecompressedSample</_XBOXONE_banktype>\n" +
                  "<_PSP_banktype>DecompressedSample</_PSP_banktype>\n" +
                  "<_PS3_banktype>DecompressedSample</_PS3_banktype>\n" +
                  "<_PS4_banktype>DecompressedSample</_PS4_banktype>\n" +
                  "<_WII_banktype>DecompressedSample</_WII_banktype>\n" +
                  "<_WiiU_banktype>DecompressedSample</_WiiU_banktype>\n" +
                  "<_3DS_banktype>DecompressedSample</_3DS_banktype>\n" +
                  "<_NGP_banktype>DecompressedSample</_NGP_banktype>\n" +
                  "<_ANDROID_banktype>DecompressedSample</_ANDROID_banktype>\n" +
                  "<_IOS_banktype>DecompressedSample</_IOS_banktype>\n" +
                  "<_BB10_banktype>DecompressedSample</_BB10_banktype>\n" +
                  "<notes></notes>");
        foreach (string FileName in Set_BGM_List)
        {
            stw.Write(Set_Voice_By_Name(FileName));
        }
        stw.Write("<_PC_format>MP3</_PC_format>\n" +
                  "<_PC_quality>75</_PC_quality>\n" +
                  "<_PC_forcesoftware>1</_PC_forcesoftware>\n" +
                  "<_PC_maxstreams>5</_PC_maxstreams>\n" +
                  "<_XBOX360_format>PCM</_XBOX360_format>\n" +
                  "<_XBOX360_quality>50</_XBOX360_quality>\n" +
                  "<_XBOX360_forcesoftware>1</_XBOX360_forcesoftware>\n" +
                  "<_XBOX360_maxstreams>10</_XBOX360_maxstreams>\n" +
                  "<_XBOXONE_format>PCM</_XBOXONE_format>\n" +
                  "<_XBOXONE_quality>50</_XBOXONE_quality>\n" +
                  "<_XBOXONE_forcesoftware>1</_XBOXONE_forcesoftware>\n" +
                  "<_XBOXONE_maxstreams>10</_XBOXONE_maxstreams>\n" +
                  "<_PSP_format>PCM</_PSP_format>\n" +
                  "<_PSP_quality>50</_PSP_quality>\n" +
                  "<_PSP_forcesoftware>0</_PSP_forcesoftware>\n" +
                  "<_PSP_maxstreams>10</_PSP_maxstreams>\n" +
                  "<_PS3_format>PCM</_PS3_format>\n" +
                  "<_PS3_quality>50</_PS3_quality>\n" +
                  "<_PS3_forcesoftware>1</_PS3_forcesoftware>\n" +
                  "<_PS3_maxstreams>10</_PS3_maxstreams>\n" +
                  "<_PS4_format>PCM</_PS4_format>\n" +
                  "<_PS4_quality>50</_PS4_quality>\n" +
                  "<_PS4_forcesoftware>1</_PS4_forcesoftware>\n" +
                  "<_PS4_maxstreams>10</_PS4_maxstreams>\n" +
                  "<_WII_format>PCM</_WII_format>\n" +
                  "<_WII_quality>50</_WII_quality>\n" +
                  "<_WII_forcesoftware>0</_WII_forcesoftware>\n" +
                  "<_WII_maxstreams>10</_WII_maxstreams>\n" +
                  "<_WiiU_format>PCM</_WiiU_format>\n" +
                  "<_WiiU_quality>50</_WiiU_quality>\n" +
                  "<_WiiU_forcesoftware>0</_WiiU_forcesoftware>\n" +
                  "<_WiiU_maxstreams>10</_WiiU_maxstreams>\n" +
                  "<_3DS_format>PCM</_3DS_format>\n" +
                  "<_3DS_quality>50</_3DS_quality>\n" +
                  "<_3DS_forcesoftware>0</_3DS_forcesoftware>\n" +
                  "<_3DS_maxstreams>10</_3DS_maxstreams>\n" +
                  "<_NGP_format>PCM</_NGP_format>\n" +
                  "<_NGP_quality>50</_NGP_quality>\n" +
                  "<_NGP_forcesoftware>0</_NGP_forcesoftware>\n" +
                  "<_NGP_maxstreams>10</_NGP_maxstreams>\n" +
                  "<_ANDROID_format>PCM</_ANDROID_format>\n" +
                  "<_ANDROID_quality>50</_ANDROID_quality>\n" +
                  "<_ANDROID_forcesoftware>1</_ANDROID_forcesoftware>\n" +
                  "<_ANDROID_maxstreams>10</_ANDROID_maxstreams>\n" +
                  "<_IOS_format>PCM</_IOS_format>\n" +
                  "<_IOS_quality>50</_IOS_quality>\n" +
                  "<_IOS_forcesoftware>1</_IOS_forcesoftware>\n" +
                  "<_IOS_maxstreams>10</_IOS_maxstreams>\n" +
                  "<_BB10_format>PCM</_BB10_format>\n" +
                  "<_BB10_quality>50</_BB10_quality>\n" +
                  "<_BB10_forcesoftware>0</_BB10_forcesoftware>\n" +
                  "<_BB10_maxstreams>10</_BB10_maxstreams>\n" +
                  "<_lang_default_filename_prefix></_lang_default_filename_prefix>\n" +
                  "<_lang_default_rebuild>1</_lang_default_rebuild>\n" +
                  "</soundbank>\n" +
                  "<notes></notes>\n" +
                  "<currentplatform>PC</currentplatform>\n" +
                  "<_PC_encryptionkey></_PC_encryptionkey>\n" +
                  "<_PC_builddirectory></_PC_builddirectory>\n" +
                  "<_PC_audiosourcedirectory></_PC_audiosourcedirectory>\n" +
                  "<_PC_prebuildcommands></_PC_prebuildcommands>\n" +
                  "<_PC_postbuildcommands></_PC_postbuildcommands>\n" +
                  "<_PC_buildinteractivemusic>Yes</_PC_buildinteractivemusic>\n" +
                  "<_XBOX360_encryptionkey></_XBOX360_encryptionkey>\n" +
                  "<_XBOX360_builddirectory></_XBOX360_builddirectory>\n" +
                  "<_XBOX360_audiosourcedirectory></_XBOX360_audiosourcedirectory>\n" +
                  "<_XBOX360_prebuildcommands></_XBOX360_prebuildcommands>\n" +
                  "<_XBOX360_postbuildcommands></_XBOX360_postbuildcommands>\n" +
                  "<_XBOX360_buildinteractivemusic>Yes</_XBOX360_buildinteractivemusic>\n" +
                  "<_XBOXONE_encryptionkey></_XBOXONE_encryptionkey>\n" +
                  "<_XBOXONE_builddirectory></_XBOXONE_builddirectory>\n" +
                  "<_XBOXONE_audiosourcedirectory></_XBOXONE_audiosourcedirectory>\n" +
                  "<_XBOXONE_prebuildcommands></_XBOXONE_prebuildcommands>\n" +
                  "<_XBOXONE_postbuildcommands></_XBOXONE_postbuildcommands>\n" +
                  "<_XBOXONE_buildinteractivemusic>Yes</_XBOXONE_buildinteractivemusic>\n" +
                  "<_PSP_encryptionkey></_PSP_encryptionkey>\n" +
                  "<_PSP_builddirectory></_PSP_builddirectory>\n" +
                  "<_PSP_audiosourcedirectory></_PSP_audiosourcedirectory>\n" +
                  "<_PSP_prebuildcommands></_PSP_prebuildcommands>\n" +
                  "<_PSP_postbuildcommands></_PSP_postbuildcommands>\n" +
                  "<_PSP_buildinteractivemusic>Yes</_PSP_buildinteractivemusic>\n" +
                  "<_PS3_encryptionkey></_PS3_encryptionkey>\n" +
                  "<_PS3_builddirectory></_PS3_builddirectory>\n" +
                  "<_PS3_audiosourcedirectory></_PS3_audiosourcedirectory>\n" +
                  "<_PS3_prebuildcommands></_PS3_prebuildcommands>\n" +
                  "<_PS3_postbuildcommands></_PS3_postbuildcommands>\n" +
                  "<_PS3_buildinteractivemusic>Yes</_PS3_buildinteractivemusic>\n" +
                  "<_PS4_encryptionkey></_PS4_encryptionkey>\n" +
                  "<_PS4_builddirectory></_PS4_builddirectory>\n" +
                  "<_PS4_audiosourcedirectory></_PS4_audiosourcedirectory>\n" +
                  "<_PS4_prebuildcommands></_PS4_prebuildcommands>\n" +
                  "<_PS4_postbuildcommands></_PS4_postbuildcommands>\n" +
                  "<_PS4_buildinteractivemusic>Yes</_PS4_buildinteractivemusic>\n" +
                  "<_WII_encryptionkey></_WII_encryptionkey>\n" +
                  "<_WII_builddirectory></_WII_builddirectory>\n" +
                  "<_WII_audiosourcedirectory></_WII_audiosourcedirectory>\n" +
                  "<_WII_prebuildcommands></_WII_prebuildcommands>\n" +
                  "<_WII_postbuildcommands></_WII_postbuildcommands>\n" +
                  "<_WII_buildinteractivemusic>Yes</_WII_buildinteractivemusic>\n" +
                  "<_WiiU_encryptionkey></_WiiU_encryptionkey>\n" +
                  "<_WiiU_builddirectory></_WiiU_builddirectory>\n" +
                  "<_WiiU_audiosourcedirectory></_WiiU_audiosourcedirectory>\n" +
                  "<_WiiU_prebuildcommands></_WiiU_prebuildcommands>\n" +
                  "<_WiiU_postbuildcommands></_WiiU_postbuildcommands>\n" +
                  "<_WiiU_buildinteractivemusic>Yes</_WiiU_buildinteractivemusic>\n" +
                  "<_3DS_encryptionkey></_3DS_encryptionkey>\n" +
                  "<_3DS_builddirectory></_3DS_builddirectory>\n" +
                  "<_3DS_audiosourcedirectory></_3DS_audiosourcedirectory>\n" +
                  "<_3DS_prebuildcommands></_3DS_prebuildcommands>\n" +
                  "<_3DS_postbuildcommands></_3DS_postbuildcommands>\n" +
                  "<_3DS_buildinteractivemusic>Yes</_3DS_buildinteractivemusic>\n" +
                  "<_NGP_encryptionkey></_NGP_encryptionkey>\n" +
                  "<_NGP_builddirectory></_NGP_builddirectory>\n" +
                  "<_NGP_audiosourcedirectory></_NGP_audiosourcedirectory>\n" +
                  "<_NGP_prebuildcommands></_NGP_prebuildcommands>\n" +
                  "<_NGP_postbuildcommands></_NGP_postbuildcommands>\n" +
                  "<_NGP_buildinteractivemusic>Yes</_NGP_buildinteractivemusic>\n" +
                  "<_ANDROID_encryptionkey></_ANDROID_encryptionkey>\n" +
                  "<_ANDROID_builddirectory></_ANDROID_builddirectory>\n" +
                  "<_ANDROID_audiosourcedirectory></_ANDROID_audiosourcedirectory>\n" +
                  "<_ANDROID_prebuildcommands></_ANDROID_prebuildcommands>\n" +
                  "<_ANDROID_postbuildcommands></_ANDROID_postbuildcommands>\n" +
                  "<_ANDROID_buildinteractivemusic>Yes</_ANDROID_buildinteractivemusic>\n" +
                  "<_IOS_encryptionkey></_IOS_encryptionkey>\n" +
                  "<_IOS_builddirectory></_IOS_builddirectory>\n" +
                  "<_IOS_audiosourcedirectory></_IOS_audiosourcedirectory>\n" +
                  "<_IOS_prebuildcommands></_IOS_prebuildcommands>\n" +
                  "<_IOS_postbuildcommands></_IOS_postbuildcommands>\n" +
                  "<_IOS_buildinteractivemusic>Yes</_IOS_buildinteractivemusic>\n" +
                  "<_BB10_encryptionkey></_BB10_encryptionkey>\n" +
                  "<_BB10_builddirectory></_BB10_builddirectory>\n" +
                  "<_BB10_audiosourcedirectory></_BB10_audiosourcedirectory>\n" +
                  "<_BB10_prebuildcommands></_BB10_prebuildcommands>\n" +
                  "<_BB10_postbuildcommands></_BB10_postbuildcommands>\n" +
                  "<_BB10_buildinteractivemusic>Yes</_BB10_buildinteractivemusic>\n" +
                  "<presavecommands></presavecommands>\n" +
                  "<postsavecommands></postsavecommands>\n" +
                  "<neweventusetemplate>0</neweventusetemplate>\n" +
                  "<neweventlasttemplatename></neweventlasttemplatename>\n" +
                  "\n" +
                  "<Composition>\n" +
                  "    <AuditionConsoleControlRepository>\n" +
                  "        <AuditionConsoleControl Type=\"Utility\" Xpos=\"18\" Ypos=\"18\">\n" +
                  "            <SimpleRack>\n" +
                  "                <ControlName>Reset</ControlName>\n" +
                  "                <ControlRange>0, 1</ControlRange>\n" +
                  "                <ControlWidget>\n" +
                  "                    <ResetControl/>\n" +
                  "                </ControlWidget>\n" +
                  "                <ControlMapping Type=\"3\" ID=\"0\"/>\n" +
                  "            </SimpleRack>\n" +
                  "        </AuditionConsoleControl>\n" +
                  "    </AuditionConsoleControlRepository>\n" +
                  "    <CompositionUI>\n" +
                  "        <SceneEditor>\n" +
                  "            <SceneEditorItemRepository/>\n" +
                  "        </SceneEditor>\n" +
                  "        <ThemeEditor>\n" +
                  "            <ThemeEditorItemRepository/>\n" +
                  "        </ThemeEditor>\n" +
                  "    </CompositionUI>\n" +
                  "    <CueFactory>\n" +
                  "        <UID>1</UID>\n" +
                  "    </CueFactory>\n" +
                  "    <CueRepository/>\n" +
                  "    <ExtLinkFactory>\n" +
                  "        <UID>1</UID>\n" +
                  "    </ExtLinkFactory>\n" +
                  "    <ExtLinkRepository/>\n" +
                  "    <ExtSegmentFactory>\n" +
                  "        <UID>1</UID>\n" +
                  "    </ExtSegmentFactory>\n" +
                  "    <MusicSettings>\n" +
                  "        <BaseVolume>1</BaseVolume>\n" +
                  "        <BaseReverbLevel>1</BaseReverbLevel>\n" +
                  "    </MusicSettings>\n" +
                  "    <ParameterFactory>\n" +
                  "        <UID>1</UID>\n" +
                  "    </ParameterFactory>\n" +
                  "    <ParameterRepository/>\n" +
                  "    <SceneRepository>\n" +
                  "        <Scene ID=\"1\">\n" +
                  "            <CueSheet></CueSheet>\n" +
                  "        </Scene>\n" +
                  "    </SceneRepository>\n" +
                  "    <SegmentRepository/>\n" +
                  "    <SharedFile/>\n" +
                  "    <ThemeFactory>\n" +
                  "        <UID>1</UID>\n" +
                  "    </ThemeFactory>\n" +
                  "    <ThemeRepository/>\n" +
                  "    <TimelineFactory>\n" +
                  "        <UID>1</UID>\n" +
                  "    </TimelineFactory>\n" +
                  "    <TimelineRepository/>\n" +
                  "</Composition></project>\n");
        stw.Close();
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