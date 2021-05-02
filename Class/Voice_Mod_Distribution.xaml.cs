using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;

namespace WoTB_Voice_Mod_Creater.Class
{
    public class Fmod_Player
    {
        static Cauldron.FMOD.EventSystem ESystem_01 = new Cauldron.FMOD.EventSystem();
        public static Cauldron.FMOD.EventSystem ESystem
        {
            get { return ESystem_01; }
            set { ESystem_01 = value; }
        }
    }
    public partial class Voice_Mod_Distribution : UserControl
    {
        int Stream;
        int Voice_Max_Index = 0;
        int Voice_Select_Now = -1;
        int Select_Language = 10;
        float Voice_Volume = 1f;
        float Voice_Pitch = 0f;
        float SetFirstFreq = 44100f;
        bool IsBusy = false;
        bool IsMessageShowing = false;
        bool IsSaveOK = false;
        string Mod_Select_Name = "";
        Wwise_Class.Wwise_File_Extract_V2 Wwise_Bnk = null;
        string[] Languages = { "arb", "cn", "cs", "de", "en", "es", "fi", "fr", "gup", "it", "ja", "ko", "pbr", "pl", "ru", "th", "tr", "vi" };
        List<string> Mod_List_Save = new List<string>();
        //サーバー内のファイルを直接読み込む(改行で分ける)
        List<string> Server_Open_File_Line(string From_File)
        {
            List<string> Temp = new List<string>();
            try
            {
                Stream stream = Voice_Set.FTP_Server.OpenRead(From_File);
                StreamReader str = new StreamReader(stream);
                while (str.EndOfStream == false)
                {
                    Temp.Add(str.ReadLine());
                }
                str.Close();
                stream.Close();
                stream.Dispose();
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
            return Temp;
        }
        public Voice_Mod_Distribution()
        {
            InitializeComponent();
            Mod_Back_B.Visibility = Visibility.Hidden;
            Mod_Password_B.Visibility = Visibility.Hidden;
            Mod_Password_T.Visibility = Visibility.Hidden;
            Mod_Password_Text.Visibility = Visibility.Hidden;
            Explanation_Scrool.Visibility = Visibility.Hidden;
            Explanation_Border.Visibility = Visibility.Hidden;
            Explanation_Text.Visibility = Visibility.Hidden;
            Create_Name_T.Visibility = Visibility.Hidden;
            Mod_Config_T.Visibility = Visibility.Hidden;
            Mod_Select_B.Visibility = Visibility.Hidden;
            Mod_Change_B.Visibility = Visibility.Hidden;
            Download_P.Visibility = Visibility.Hidden;
            Download_T.Visibility = Visibility.Hidden;
            Download_Border.Visibility = Visibility.Hidden;
            Language_T.Visibility = Visibility.Hidden;
            Language_Left_B.Visibility = Visibility.Hidden;
            Language_Right_B.Visibility = Visibility.Hidden;
            Language_Help_B.Visibility = Visibility.Hidden;
            Mod_Control_Change_Visible(false);
            Voice_Volume_S.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Voice_Volume_MouseUp), true);
            Opacity = 0;
            Voice_Pitch_S.Minimum = -30000;
            Voice_Pitch_S.Maximum = 30000;
            Voice_Volume_S.Minimum = 0;
            Voice_Volume_S.Maximum = 100;
            Voice_Pitch_S.Value = 0;
            Voice_Volume_S.Value = 75;
        }
        async public void Window_Show()
        {
            Mod_Install_B.Margin = new Thickness(-1245, 125, 0, 0);
            Language_T.Margin = new Thickness(-1250, 250, 0, 0);
            Language_Left_B.Margin = new Thickness(-1480, 250, 0, 0);
            Language_Right_B.Margin = new Thickness(-1020, 250, 0, 0);
            Language_Help_B.Margin = new Thickness(-875, 250, 0, 0);
            Visibility = Visibility.Visible;
            Mod_List_Update();
            if (File.Exists(Voice_Set.Special_Path + "/Configs/Voice_Mod.conf"))
            {
                try
                {
                    using (var eifs = new FileStream(Voice_Set.Special_Path + "/Configs/Voice_Mod.conf", FileMode.Open, FileAccess.Read))
                    {
                        using (var eofs = new FileStream(Voice_Set.Special_Path + "/Configs/Temp_Voice_Mod.tmp", FileMode.Create, FileAccess.Write))
                        {
                            FileEncode.FileEncryptor.Decrypt(eifs, eofs, "Voice_Mod_Configs_Save");
                        }
                    }
                    StreamReader str = new StreamReader(Voice_Set.Special_Path + "/Configs/Temp_Voice_Mod.tmp");
                    Voice_Volume_S.Value = double.Parse(str.ReadLine());
                    List_Change_C.IsChecked = bool.Parse(str.ReadLine());
                    Select_Language = int.Parse(str.ReadLine());
                    str.Close();
                    if (List_Change_C.IsChecked.Value)
                    {
                        Fmod_Bank_List.Items.Clear();
                        if (List_Change_C.IsChecked.Value)
                        {
                            List<string> List_Temp = new List<string>();
                            List_Temp.AddRange(Mod_List_Save);
                            List_Temp.Sort();
                            foreach (string Name in List_Temp)
                            {
                                Fmod_Bank_List.Items.Add(Name);
                            }
                        }
                        else
                        {
                            foreach (string Name in Mod_List_Save)
                            {
                                Fmod_Bank_List.Items.Add(Name);
                            }
                        }
                    }
                    Language_T.Text = "言語:" + Languages[Select_Language];
                    File.Delete(Voice_Set.Special_Path + "/Configs/Temp_Voice_Mod.tmp");
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show("設定を読み込めませんでした。。\nエラー回避のため設定は削除されます。");
                    File.Delete(Voice_Set.Special_Path + "/Configs/Voice_Mod.conf");
                    Voice_Volume_S.Value = 75;
                    Sub_Code.Error_Log_Write(e.Message);
                }
            }
            if (Fmod_Bank_List.Items.Count == 0)
            {
                Message_T.Text = "現在配布されているModはありません。";
            }
            while (Opacity < 1 && !IsBusy)
            {
                Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
            IsSaveOK = true;
            while (Opacity > 0)
            {
                Fmod_Player.ESystem.Update();
                await Task.Delay(1000 / 60);
            }
        }
        //配布されているModを更新
        void Mod_List_Update()
        {
            Fmod_Bank_List.Items.Clear();
            List<string> Mods_Read = Server_Open_File_Line("/WoTB_Voice_Mod/Mods/Mod_Names_Wwise.dat");
            Mod_List_Save = Mods_Read;
            if (List_Change_C.IsChecked.Value)
            {
                Mods_Read.Sort();
            }
            foreach (string Line in Mods_Read)
            {
                Fmod_Bank_List.Items.Add(Line);
            }
            if (Fmod_Bank_List.Items.Count == 0)
            {
                Message_T.Text = "現在配布されているModはありません。";
            }
            else if (Message_T.Text == "現在配布されているModはありません。")
            {
                Message_T.Text = "";
            }
        }
        //ランダム再生
        private async void Random_Play_B_Click(object sender, RoutedEventArgs e)
        {
            if (Voice_Max_Index == 0)
            {
                return;
            }
            Random r = new Random();
            int Number = r.Next(0, Voice_Max_Index - 1);
            await Set_Fmod_Bank_Play(Number);
            Voice_Index_S.Value = Number;
        }
        //再生しているサウンドを停止
        private void Stop_B_Click(object sender, RoutedEventArgs e)
        {
            Bass.BASS_ChannelPause(Stream);
        }
        private async void Voice_Index_Play_Click(object sender, RoutedEventArgs e)
        {
            //スライダーの位置のサウンドを再生
            await Set_Fmod_Bank_Play((int)Voice_Index_S.Value);
        }
        private void Voice_Index_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Voice_Index_T.Text = (int)(Voice_Index_S.Value + 1) + "/" + Voice_Max_Index;
        }
        async void Sample_Download()
        {
            //選択しているModをサーバーからダウンロード
            try
            {
                string Bank_Name = Fmod_Bank_List.Items[Fmod_Bank_List.SelectedIndex].ToString();
                if (!Voice_Set.FTP_Server.DirectoryExists("/WoTB_Voice_Mod/Mods/" + Bank_Name))
                {
                    Message_Feed_Out("Modが見つかりませんでした。削除された可能性があります。");
                    return;
                }
                if (!Directory.Exists(Voice_Set.Special_Path + "/Server/Download_Mods/" + Bank_Name) || Directory.GetFiles(Voice_Set.Special_Path + "/Server/Download_Mods/" + Bank_Name, "*", SearchOption.AllDirectories).Length == 0)
                {
                    if (Directory.Exists(Voice_Set.Special_Path + "/Server/Download_Mods/" + Bank_Name))
                    {
                        Directory.Delete(Voice_Set.Special_Path + "/Server/Download_Mods/" + Bank_Name, true);
                    }
                    Directory.CreateDirectory(Voice_Set.Special_Path + "/Server/Download_Mods/" + Bank_Name);
                    Message_T.Text = "サンプルをダウンロードしています...";
                    await Task.Delay(50);
                    Download_P.Value = 0;
                    Download_T.Text = "進捗:0%";
                    Download_P.Visibility = Visibility.Visible;
                    Download_T.Visibility = Visibility.Visible;
                    Download_Border.Visibility = Visibility.Visible;
                    List<string> strList = new List<string>();
                    foreach (FluentFTP.FtpListItem item in Voice_Set.FTP_Server.GetListing("/WoTB_Voice_Mod/Mods/" + Bank_Name + "/Files"))
                    {
                        strList.Add(item.Name);
                    }
                    foreach (string File_Name in strList)
                    {
                        try
                        {
                            Message_T.Text = File_Name + "をダウンロードしています...";
                            long File_Size_Full = Voice_Set.FTP_Server.GetFileSize("/WoTB_Voice_Mod/Mods/" + Bank_Name + "/Files/" + File_Name);
                            Task task = Task.Run(() =>
                            {
                                Voice_Set.FTP_Server.DownloadFile(Voice_Set.Special_Path + "/Server/Download_Mods/" + Bank_Name + "/" + File_Name, "/WoTB_Voice_Mod/Mods/" + Bank_Name + "/Files/" + File_Name);
                            });
                            while (true)
                            {
                                long File_Size_Now = 0;
                                if (File.Exists(Voice_Set.Special_Path + "/Server/Download_Mods/" + Bank_Name + "/" + File_Name))
                                {
                                    FileInfo fi = new FileInfo(Voice_Set.Special_Path + "/Server/Download_Mods/" + Bank_Name + "/" + File_Name);
                                    File_Size_Now = fi.Length;
                                }
                                double Download_Percent = (double)File_Size_Now / File_Size_Full * 100;
                                int Percent_INT = (int)Math.Round(Download_Percent, MidpointRounding.AwayFromZero);
                                Download_P.Value = Percent_INT;
                                Download_T.Text = "進捗:" + Percent_INT + "%";
                                if (File_Size_Now >= File_Size_Full)
                                {
                                    Download_P.Value = 0;
                                    Download_T.Text = "進捗:0%";
                                    break;
                                }
                                await Task.Delay(100);
                            }
                        }
                        catch (Exception e)
                        {
                            Sub_Code.Error_Log_Write(e.Message);
                        }
                    }
                    //Voice_Set.FTP_Server.DownloadDirectory(Voice_Set.Special_Path + "/Server/Download_Mods/" + Bank_Name, "/WoTB_Voice_Mod/Mods/" + Bank_Name + "/Files");
                    string Dir = Voice_Set.Special_Path + "/Server/Download_Mods/" + Bank_Name;
                    string[] Dir_Files = Directory.GetFiles(Dir, "*.dvpl", SearchOption.TopDirectoryOnly);
                    foreach (string Files in Dir_Files)
                    {
                        Directory.CreateDirectory(Dir + "/Temp");
                        DVPL.DVPL_UnPack(Files, Dir + "/Temp/" + Path.GetFileName(Files).Replace(".dvpl", ""), false);
                    }
                    Download_P.Visibility = Visibility.Hidden;
                    Download_T.Visibility = Visibility.Hidden;
                    Download_Border.Visibility = Visibility.Hidden;
                    Message_Feed_Out(Bank_Name + "をダウンロードしました。");
                }
                Mod_Select_Name = Bank_Name;
                Fmod_Bank_List.Items.Clear();
                //.fevをリストに追加(.dvplは今のところ再生できない -> V1.2.2より再生可)
                string[] Dir1 = Directory.GetFiles(Voice_Set.Special_Path + "/Server/Download_Mods/" + Bank_Name, "*.bnk", SearchOption.TopDirectoryOnly);
                foreach (string Name in Dir1)
                {
                    Fmod_Bank_List.Items.Add(Path.GetFileName(Name));
                }
                string[] Dir2 = Directory.GetFiles(Voice_Set.Special_Path + "/Server/Download_Mods/" + Bank_Name, "*.bnk.dvpl", SearchOption.TopDirectoryOnly);
                foreach (string Name in Dir2)
                {
                    Fmod_Bank_List.Items.Add(Path.GetFileName(Name));
                }
                Mod_Back_B.Visibility = Visibility.Visible;
                Mod_Install_B.Visibility = Visibility.Visible;
                Language_T.Visibility = Visibility.Visible;
                Language_Left_B.Visibility = Visibility.Visible;
                Language_Right_B.Visibility = Visibility.Visible;
                Language_Help_B.Visibility = Visibility.Visible;
            }
            catch (Exception e)
            {
                Message_Feed_Out("エラーが発生しました。詳しくはError_Log.txtを参照してください。");
                Sub_Code.Error_Log_Write(e.Message);
                return;
            }
        }
        private void Fmod_Bank_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Fmod_Bank_List.SelectedIndex != -1)
            {
                if (IsBusy)
                {
                    return;
                }
                Message_T.Text = "";
                string Bank_Name = Fmod_Bank_List.Items[Fmod_Bank_List.SelectedIndex].ToString();
                if (Mod_Select_Name != "")
                {
                    //fevの選択が変更されたら新たにロード(V1.2.2からdvplも対応)
                    try
                    {
                        Bass.BASS_ChannelStop(Stream);
                        if (Wwise_Bnk != null)
                        {
                            Wwise_Bnk.Bank_Clear();
                        }
                        string Dir = Voice_Set.Special_Path + "/Server/Download_Mods/" + Mod_Select_Name;
                        if (Path.GetExtension(Dir + "/" + Bank_Name) == ".dvpl")
                        {
                            //Fmod_Player.ESystem.Load(Dir + "/Temp/" + Bank_Name.Replace(".dvpl", ""), ref ELI, ref EP);
                            Wwise_Bnk = new Wwise_Class.Wwise_File_Extract_V2(Dir + "/Temp/" + Bank_Name.Replace(".dvpl", ""));
                        }
                        else
                        {
                            //Fmod_Player.ESystem.Load(Voice_Set.Special_Path + "/Server/Download_Mods/" + Mod_Select_Name + "/" + Bank_Name, ref ELI, ref EP);
                            Wwise_Bnk = new Wwise_Class.Wwise_File_Extract_V2(Dir + "/" + Bank_Name);
                        }
                        /*Cauldron.FMOD.EventProject EC = new Cauldron.FMOD.EventProject();
                        Fmod_Player.ESystem.GetProjectByIndex(0, ref EC);
                        EC.GetGroupByIndex(0, true, ref EG);
                        EG.GetNumEvents(ref Voice_Max_Index);*/
                        Voice_Select_Now = -1;
                        Voice_Max_Index = Wwise_Bnk.Wwise_Get_Numbers();
                        Voice_Index_S.Value = 0;
                        Voice_Index_S.Maximum = Voice_Max_Index - 1;
                        Voice_Index_T.Text = "1/" + Voice_Max_Index;
                        Mod_Install_B.Margin = new Thickness(-1200, 550, 0, 0);
                        Language_T.Margin = new Thickness(-1250, 475, 0, 0);
                        Language_Left_B.Margin = new Thickness(-1480, 475, 0, 0);
                        Language_Right_B.Margin = new Thickness(-1020, 475, 0, 0);
                        Language_Help_B.Margin = new Thickness(-875, 475, 0, 0);
                        Mod_Control_Change_Visible(true);
                    }
                    catch (Exception e1)
                    {
                        Sub_Code.Error_Log_Write(e1.Message);
                        Message_Feed_Out("エラー:ファイルを読み取れませんでした。");
                        return;
                    }
                }
                else
                {
                    //Modが選択されたら詳細を表示
                    if (!Voice_Set.FTP_Server.FileExists("/WoTB_Voice_Mod/Mods/" + Bank_Name + "/Configs.dat"))
                    {
                        Fmod_Bank_List.SelectedIndex = -1;
                        Message_Feed_Out("選択したModは現在利用できません。");
                        return;
                    }
                    XDocument xml2 = XDocument.Load(Voice_Set.FTP_Server.OpenRead("/WoTB_Voice_Mod/Mods/" + Bank_Name + "/Configs.dat"));
                    XElement item2 = xml2.Element("Mod_Upload_Config");
                    if (bool.Parse(item2.Element("IsPassword").Value))
                    {
                        Mod_Password_T.Text = "";
                        Mod_Password_B.Visibility = Visibility.Visible;
                        Mod_Password_T.Visibility = Visibility.Visible;
                        Mod_Password_Text.Visibility = Visibility.Visible;
                        Mod_Select_B.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        Mod_Password_B.Visibility = Visibility.Hidden;
                        Mod_Password_T.Visibility = Visibility.Hidden;
                        Mod_Password_Text.Visibility = Visibility.Hidden;
                        Mod_Select_B.Visibility = Visibility.Visible;
                    }
                    string IsEnableR18;
                    string IsBGMMode;
                    if (bool.Parse(item2.Element("IsEnableR18").Value) == true)
                    {
                        IsEnableR18 = "あり";
                    }
                    else
                    {
                        IsEnableR18 = "なし";
                    }
                    if (bool.Parse(item2.Element("IsBGMMode").Value) == true)
                    {
                        IsBGMMode = "あり";
                    }
                    else
                    {
                        IsBGMMode = "なし";
                    }
                    Mod_Config_T.Text = "R18音声:" + IsEnableR18 + "\n戦闘BGM:" + IsBGMMode;
                    Mod_Config_T.Visibility = Visibility.Visible;
                    Create_Name_T.Text = "配布者:" + item2.Element("UserName").Value;
                    Create_Name_T.Visibility = Visibility.Visible;
                    Explanation_T.Text = item2.Element("Explanation").Value;
                    Explanation_Scrool.Visibility = Visibility.Visible;
                    Explanation_Border.Visibility = Visibility.Visible;
                    Explanation_Text.Visibility = Visibility.Visible;
                    if (Voice_Set.UserName == item2.Element("UserName").Value)
                    {
                        Mod_Change_B.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        Mod_Change_B.Visibility = Visibility.Hidden;
                    }
                    if (Explanation_T.Text == "")
                    {
                        Explanation_T.Text = "説明なし";
                    }
                }
            }
        }
        private void Fmod_Bank_List_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //リストの選択を解除
            if (IsBusy)
            {
                return;
            }
            if (Fmod_Bank_List.SelectedIndex != -1)
            {
                if (Mod_Select_Name != "")
                {
                    Mod_Control_Change_Visible(false);
                    Mod_Install_B.Margin = new Thickness(-1200, 125, 0, 0);
                    Language_T.Margin = new Thickness(-1250, 250, 0, 0);
                    Language_Left_B.Margin = new Thickness(-1480, 250, 0, 0);
                    Language_Right_B.Margin = new Thickness(-1020, 250, 0, 0);
                    Language_Help_B.Margin = new Thickness(-875, 250, 0, 0);
                    Mod_Install_B.Visibility = Visibility.Visible;
                    Language_T.Visibility = Visibility.Visible;
                    Language_Left_B.Visibility = Visibility.Visible;
                    Language_Right_B.Visibility = Visibility.Visible;
                    Language_Help_B.Visibility = Visibility.Visible;
                }
                else
                {
                    Mod_Password_B.Visibility = Visibility.Hidden;
                    Mod_Password_T.Visibility = Visibility.Hidden;
                    Mod_Password_Text.Visibility = Visibility.Hidden;
                    Explanation_Scrool.Visibility = Visibility.Hidden;
                    Explanation_Border.Visibility = Visibility.Hidden;
                    Explanation_Text.Visibility = Visibility.Hidden;
                    Create_Name_T.Visibility = Visibility.Hidden;
                    Mod_Config_T.Visibility = Visibility.Hidden;
                    Mod_Change_B.Visibility = Visibility.Hidden;
                }
                Message_T.Text = "";
                Fmod_Bank_List.SelectedIndex = -1;
            }
        }
        async Task Set_Fmod_Bank_Play(int Voice_Number)
        {
            //fevの中から指定した位置のサウンドを再生
            if (IsBusy || Wwise_Bnk == null || Wwise_Bnk.IsClear)
            {
                return;
            }
            /*if (FE != null)
            {
                FE.Stop();
            }
            EG.GetEventByIndex(Voice_Number, Cauldron.FMOD.EVENT_MODE.DEFAULT, ref FE);
            FE.SetVolume(Voice_Volume);
            FE.SetPitch(Voice_Pitch, Cauldron.FMOD.EVENT_PITCHUNITS.TONES);
            FE.Start();*/
            Bass.BASS_ChannelStop(Stream);
            if (Voice_Select_Now != Voice_Number)
            {
                Message_T.Text = "変換しています...";
                await Task.Delay(50);
                Wwise_Bnk.Wwise_Extract_To_Ogg_File(Voice_Number, Voice_Set.Special_Path + "/Wwise/Temp_03.ogg", true);
                Voice_Select_Now = Voice_Number;
            }
            int StreamHandle = Bass.BASS_StreamCreateFile(Voice_Set.Special_Path + "/Wwise/Temp_03.ogg", 0, 0, BASSFlag.BASS_STREAM_DECODE);
            Stream = BassFx.BASS_FX_TempoCreate(StreamHandle, BASSFlag.BASS_FX_FREESOURCE);
            Bass.BASS_ChannelSetDevice(Stream, Video_Mode.Sound_Device);
            Bass.BASS_ChannelGetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, ref SetFirstFreq);
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, SetFirstFreq + Voice_Pitch);
            Bass.BASS_ChannelPlay(Stream, true);
            Message_T.Text = "";
        }
        void Mod_Control_Change_Visible(bool IsVisible)
        {
            //Modを選択する画面かfevを選択する画面かを変更(trueがfevを選択する画面)
            if (IsVisible)
            {
                Random_Play_B.Visibility = Visibility.Visible;
                Voice_Index_Play.Visibility = Visibility.Visible;
                Stop_B.Visibility = Visibility.Visible;
                Voice_Index_S.Visibility = Visibility.Visible;
                Voice_Index_T.Visibility = Visibility.Visible;
                Voice_Volume_S.Visibility = Visibility.Visible;
                Voice_Volume_T.Visibility = Visibility.Visible;
                Voice_Pitch_S.Visibility = Visibility.Visible;
                Voice_Pitch_T.Visibility = Visibility.Visible;
            }
            else
            {
                Random_Play_B.Visibility = Visibility.Hidden;
                Voice_Index_Play.Visibility = Visibility.Hidden;
                Stop_B.Visibility = Visibility.Hidden;
                Voice_Index_S.Visibility = Visibility.Hidden;
                Voice_Index_T.Visibility = Visibility.Hidden;
                Voice_Volume_S.Visibility = Visibility.Hidden;
                Voice_Volume_T.Visibility = Visibility.Hidden;
                Voice_Pitch_S.Visibility = Visibility.Hidden;
                Voice_Pitch_T.Visibility = Visibility.Hidden;
                Mod_Install_B.Visibility = Visibility.Hidden;
                Language_T.Visibility = Visibility.Hidden;
                Language_Left_B.Visibility = Visibility.Hidden;
                Language_Right_B.Visibility = Visibility.Hidden;
                Language_Help_B.Visibility = Visibility.Hidden;
                /*EP.Release();
                FE.Release();
                EG = new Cauldron.FMOD.EventGroup();
                EP = new Cauldron.FMOD.EventProject();
                FE = new Cauldron.FMOD.Event();*/
                Bass.BASS_ChannelStop(Stream);
                if (Wwise_Bnk != null && !Wwise_Bnk.IsClear)
                {
                    Wwise_Bnk.Bank_Clear();
                }
            }
        }
        private async void Back_B_Click(object sender, RoutedEventArgs e)
        {
            if (!IsBusy)
            {
                //画面を閉じる
                IsBusy = true;
                while (Opacity > 0)
                {
                    Opacity -= Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
                Visibility = Visibility.Hidden;
                Mod_Control_Change_Visible(false);
                if (Directory.Exists(Voice_Set.Special_Path + "/Server/Download_Mods/" + Mod_Select_Name) && Mod_Select_Name != "")
                {
                    Directory.Delete(Voice_Set.Special_Path + "/Server/Download_Mods/" + Mod_Select_Name, true);
                }
                Mod_Install_B.Margin = new Thickness(-1200, 125, 0, 0);
                Language_T.Margin = new Thickness(-1250, 250, 0, 0);
                Language_Left_B.Margin = new Thickness(-1480, 250, 0, 0);
                Language_Right_B.Margin = new Thickness(-1020, 250, 0, 0);
                Language_Help_B.Margin = new Thickness(-875, 250, 0, 0);
                Mod_Back_B.Visibility = Visibility.Hidden;
                Explanation_Scrool.Visibility = Visibility.Hidden;
                Explanation_Border.Visibility = Visibility.Hidden;
                Explanation_Text.Visibility = Visibility.Hidden;
                Create_Name_T.Visibility = Visibility.Hidden;
                Mod_Config_T.Visibility = Visibility.Hidden;
                Mod_Change_B.Visibility = Visibility.Hidden;
                Language_T.Visibility = Visibility.Hidden;
                Language_Left_B.Visibility = Visibility.Hidden;
                Language_Right_B.Visibility = Visibility.Hidden;
                Language_Help_B.Visibility = Visibility.Hidden;
                Message_T.Text = "";
                Mod_Select_Name = "";
                IsBusy = false;
            }
        }
        private void Voice_Volume_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //音量を変更(ダブルクリックで初期化)
            Voice_Volume = (float)Voice_Volume_S.Value / 100;
            Voice_Volume_T.Text = "音量:" + (int)Voice_Volume_S.Value;
            /*if (FE != null)
            {
                FE.SetVolume(Voice_Volume);
            }*/
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, Voice_Volume);
        }
        private void Voice_Pitch_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //ピッチを変更(ダブルクリックで初期化)
            Voice_Pitch = (float)Voice_Pitch_S.Value;
            Voice_Pitch_T.Text = "ピッチ:" + Math.Round(Voice_Pitch_S.Value, 2, MidpointRounding.AwayFromZero);
            /*if (FE != null)
            {
                FE.SetPitch(Voice_Pitch, Cauldron.FMOD.EVENT_PITCHUNITS.TONES);
            }*/
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, SetFirstFreq + Voice_Pitch);
        }
        private void Mod_Back_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
            //fevを選択する画面からModを選ぶ画面に戻る
            Mod_Control_Change_Visible(false);
            if (Directory.Exists(Voice_Set.Special_Path + "/Server/Download_Mods/" + Mod_Select_Name) && Mod_Select_Name != "")
            {
                Directory.Delete(Voice_Set.Special_Path + "/Server/Download_Mods/" + Mod_Select_Name, true);
            }
            Mod_Install_B.Margin = new Thickness(-1200, 125, 0, 0);
            Language_T.Margin = new Thickness(-1250, 250, 0, 0);
            Language_Left_B.Margin = new Thickness(-1480, 250, 0, 0);
            Language_Right_B.Margin = new Thickness(-1020, 250, 0, 0);
            Language_Help_B.Margin = new Thickness(-875, 250, 0, 0);
            Mod_Back_B.Visibility = Visibility.Hidden;
            Explanation_Scrool.Visibility = Visibility.Hidden;
            Explanation_Border.Visibility = Visibility.Hidden;
            Explanation_Text.Visibility = Visibility.Hidden;
            Create_Name_T.Visibility = Visibility.Hidden;
            Mod_Config_T.Visibility = Visibility.Hidden;
            Mod_Change_B.Visibility = Visibility.Hidden;
            Language_T.Visibility = Visibility.Hidden;
            Language_Left_B.Visibility = Visibility.Hidden;
            Language_Right_B.Visibility = Visibility.Hidden;
            Language_Help_B.Visibility = Visibility.Hidden;
            List_Change_C.Visibility = Visibility.Visible;
            List_Change_T.Visibility = Visibility.Visible;
            Mod_Select_Name = "";
            Message_T.Text = "";
            Mod_List_Update();
        }
        private async void Create_Mod_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
            if (Mod_Select_Name == "" && Fmod_Bank_List.Items.Count == 0 && !SRTTbacon_Server.IsSRTTbaconOwnerMode)
            {
                Message_Feed_Out("現在Modを公開することはできません。");
                return;
            }
            //Modを配布する画面に移動(Mod_Uploads.csを参照)
            Mod_Uploads_Window.Window_Show();
            if (Mod_Select_Name == "")
            {
                while (Mod_Uploads_Window.Visibility == Visibility.Visible)
                {
                    await Task.Delay(100);
                }
                Mod_List_Update();
            }
        }
        private async void Mod_Install_B_Click(object sender, RoutedEventArgs e)
        {
            if (Voice_Set.WoTB_Path == "")
            {
                Message_Feed_Out("WoTBのフォルダを取得できませんでした。");
                return;
            }
            if (IsBusy || Opacity < 1)
            {
                return;
            }
            //選択したModをWoTBに導入
            IsBusy = true;
            Message_T.Text = "ファイルをコピーしています...";
            await Task.Delay(50);
            //bool IsDVPL = false;
            if (!Directory.Exists(Voice_Set.WoTB_Path + "/Data/Mods"))
            {
                Directory.CreateDirectory(Voice_Set.WoTB_Path + "/Data/Mods");
            }
            string[] Dir = Directory.GetFiles(Voice_Set.Special_Path + "/Server/Download_Mods/" + Mod_Select_Name, "*", SearchOption.TopDirectoryOnly);
            List<string> FEV_List = new List<string>();
            foreach (string Name in Dir)
            {
                string Name_Only = Path.GetFileName(Name).Replace(".dvpl", "");
                /*if (Name_Only.Contains("sounds.yaml"))
                {
                    //sounds.yamlが古い場合サーバーに置いている最新のものと比較して置き換える
                    Message_T.Text = "sounds.yamlを更新しています...";
                    await Task.Delay(10);
                    if (Path.GetExtension(Name) == ".dvpl")
                    {
                        DVPL.DVPL_UnPack(Name, Path.GetDirectoryName(Name) + "/sounds.yaml", true);
                        //引数:導入するModのsounds.yamlの場所,置き換え後のファイル場所,置き換えたらdvpl化するか
                        Sub_Code.Sounds_Yaml_Update(Path.GetDirectoryName(Name) + "/sounds.yaml", Voice_Set.Special_Path + "/Temp_Change_Sounds.yaml.dvpl", true);
                        IsDVPL = true;
                    }
                    else
                    {
                        Sub_Code.Sounds_Yaml_Update(Name, Voice_Set.Special_Path + "/Temp_Change_Sounds.yaml", false);
                    }
                    File.Delete(Voice_Set.WoTB_Path + "/Data/sounds.yaml");
                    File.Delete(Voice_Set.WoTB_Path + "/Data/sounds.yaml.dvpl");
                    Sub_Code.DVPL_File_Copy(Voice_Set.Special_Path + "/Temp_Change_Sounds.yaml", Voice_Set.WoTB_Path + "/Data/sounds.yaml", true);
                    File.Delete(Voice_Set.Special_Path + "/Temp_Change_Sounds.yaml");
                    File.Delete(Voice_Set.Special_Path + "/Temp_DVPL.yaml");
                    File.Delete(Voice_Set.Special_Path + "/Temp_Download_Sounds.yaml.dvpl");
                }
                if (Name_Only.Contains(".fev") || Name_Only.Contains(".fsb"))
                {
                    File.Copy(Name, Voice_Set.WoTB_Path + "/Data/Mods/" + Path.GetFileName(Name), true);
                    if (Name_Only.Contains(".fev"))
                    {
                        FEV_List.Add(Name_Only.Replace(".dvpl", ""));
                    }
                }*/
                if (Name_Only == "voiceover_crew.bnk")
                {
                    string WoTB_Path = Voice_Set.WoTB_Path + "/Data/WwiseSound/" + Languages[Select_Language] + "/voiceover_crew.bnk";
                    Sub_Code.DVPL_File_Delete(Voice_Set.WoTB_Path + "/Data/WwiseSound/" + Languages[Select_Language] + "/voiceover_crew.bnk");
                    Sub_Code.DVPL_File_Copy(Name.Replace(".dvpl", ""), WoTB_Path, true);
                }
                else if (Name_Only.Contains(".bnk") || Name_Only.Contains(".pck"))
                {
                    Message_T.Text = "ファイルをコピーしています...";
                    Sub_Code.DVPL_File_Delete(Voice_Set.WoTB_Path + "/Data/WwiseSound/" + Path.GetFileName(Name).Replace(".dvpl", ""));
                    Sub_Code.DVPL_File_Copy(Name.Replace(".dvpl", ""), Voice_Set.WoTB_Path + "/Data/WwiseSound/" + Path.GetFileName(Name).Replace(".dvpl", ""), true);
                }
            }
            /*if (FEV_List.Count != 0)
            {
                try
                {
                    //sfx_high(low).yamlにModのfevを追加
                    //このため配布の時点でsfx_high(low).yamlを追加する必要はない
                    if (File.Exists(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_high.yaml"))
                    {
                        Change_Sfx_High_And_Low(FEV_List, Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_high.yaml", Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_low.yaml");
                        File.Delete(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_high.yaml.dvpl");
                        File.Delete(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_low.yaml.dvpl");
                    }
                    else if (File.Exists(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_high.yaml.dvpl"))
                    {
                        DVPL.DVPL_UnPack(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_high.yaml.dvpl", Voice_Set.Special_Path + "/Temp_sfx_high.yaml", false);
                        DVPL.DVPL_UnPack(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_low.yaml.dvpl", Voice_Set.Special_Path + "/Temp_sfx_low.yaml", false);
                        Change_Sfx_High_And_Low(FEV_List, Voice_Set.Special_Path + "/Temp_sfx_high.yaml", Voice_Set.Special_Path + "/Temp_sfx_low.yaml");
                        DVPL.DVPL_Pack(Voice_Set.Special_Path + "/Temp_sfx_high.yaml", Voice_Set.Special_Path + "/Temp_sfx_high.yaml.dvpl", true);
                        DVPL.DVPL_Pack(Voice_Set.Special_Path + "/Temp_sfx_low.yaml", Voice_Set.Special_Path + "/Temp_sfx_low.yaml.dvpl", true);
                        File.Copy(Voice_Set.Special_Path + "/Temp_sfx_high.yaml.dvpl", Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_high.yaml.dvpl", true);
                        File.Copy(Voice_Set.Special_Path + "/Temp_sfx_low.yaml.dvpl", Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_low.yaml.dvpl", true);
                        File.Delete(Voice_Set.Special_Path + "/Temp_sfx_high.yaml.dvpl");
                        File.Delete(Voice_Set.Special_Path + "/Temp_sfx_low.yaml.dvpl");
                    }
                    else
                    {
                        throw new Exception("sfx_high(low).yamlが存在しません。");
                    }
                }
                catch (Exception e1)
                {
                    Message_Feed_Out("sfx_high(low).yamlが見つかりませんでした。");
                    Sub_Code.Error_Log_Write(e1.Message);
                    IsBusy = false;
                    return;
                }
            }
            //sounds.yamlのVOICE_START_BATTLEの項目をBGMMod用に変更(SRTTbaconが作成した音声以外は戦闘開始の音声を犠牲にする)
            XDocument xml2 = XDocument.Load(Voice_Set.FTP_Server.OpenRead("/WoTB_Voice_Mod/Mods/" + Mod_Select_Name + "/Configs.dat"));
            XElement item2 = xml2.Element("Mod_Upload_Config");
            if (bool.Parse(item2.Element("IsBGMMode").Value) == true)
            {
                if (File.Exists(Voice_Set.WoTB_Path + "/Data/sounds.yaml.dvpl"))
                {
                    DVPL.DVPL_UnPack(Voice_Set.WoTB_Path + "/Data/sounds.yaml.dvpl", Voice_Set.WoTB_Path + "/Data/sounds.yaml", true);
                }
                StreamReader str = new StreamReader(Voice_Set.WoTB_Path + "/Data/sounds.yaml");
                string Read = str.ReadToEnd();
                str.Close();
                string Temp = "";
                string[] Lines = Read.Split('\n');
                foreach (string Line in Lines)
                {
                    if (Line.Contains("VOICE_START_BATTLE"))
                    {
                        Temp += "    VOICE_START_BATTLE: \"Music/Music/Music\"\n";
                        continue;
                    }
                    Temp += Line + "\n";
                }
                StreamWriter stw = File.CreateText(Voice_Set.WoTB_Path + "/Data/sounds.yaml");
                stw.Write(Temp);
                stw.Close();
            }
            //もともとdvplだった場合dvpl化する
            if (IsDVPL)
            {
                DVPL.DVPL_Pack(Voice_Set.WoTB_Path + "/Data/sounds.yaml", Voice_Set.WoTB_Path + "/Data/sounds.yaml.dvpl", true);
            }
            else if (File.Exists(Voice_Set.WoTB_Path + "/Data/sounds.yaml.dvpl"))
            {
                DVPL.DVPL_UnPack(Voice_Set.WoTB_Path + "/Data/sounds.yaml.dvpl", Voice_Set.WoTB_Path + "/Data/sounds.yaml", true);
            }*/
            IsBusy = false;
            Message_Feed_Out("インストールしました。");
        }
        void Change_Sfx_High_And_Low(List<string> Write, string High_Path, string Low_Path)
        {
            //sfx_high(low).yamlに"Write"にある.fevファイルを追加
            StreamReader str = new StreamReader(High_Path);
            string[] Lines = str.ReadToEnd().Split('\n');
            str.Close();
            bool IsIncludeMusic = false;
            bool IsIncludedMusic = false;
            StreamWriter stw1 = new StreamWriter(High_Path, true);
            StreamWriter stw2 = new StreamWriter(Low_Path, true);
            foreach (string FEV_Name in Write)
            {
                bool IsExist = false;
                if (FEV_Name == "Music.fev")
                {
                    IsIncludeMusic = true;
                }
                foreach (string Line in Lines)
                {
                    if (Line.Contains("/"))
                    {
                        if (Line.Substring(Line.LastIndexOf('/')).Contains(FEV_Name))
                        {
                            IsExist = true;
                            break;
                        }
                    }
                }
                if (!IsExist)
                {
                    stw1.Write("\n -\n  \"~res:/Mods/" + FEV_Name + "\"");
                    stw2.Write("\n -\n  \"~res:/Mods/" + FEV_Name + "\"");
                }
            }
            foreach (string Line_01 in Lines)
            {
                if (Line_01.Contains("/"))
                {
                    if (Line_01.Substring(Line_01.LastIndexOf('/')).Contains("Music.fev"))
                    {
                        IsIncludedMusic = true;
                        break;
                    }
                }
            }
            if (!IsIncludeMusic && !IsIncludedMusic)
            {
                stw1.Write("\n -\n  \"~res:/Mods/Music.fev\"");
                stw2.Write("\n -\n  \"~res:/Mods/Music.fev\"");
            }
            stw1.Close();
            stw2.Close();
        }
        private void Mod_Backup_B_Click(object sender, RoutedEventArgs e)
        {
            //起動時にコピーされるファイルから復元
            if (IsBusy)
            {
                return;
            }
            if (Voice_Set.WoTB_Path == "")
            {
                Message_Feed_Out("WoTBのフォルダを取得できませんでした。");
                return;
            }
            MessageBoxResult result = MessageBox.Show("初めて起動したときに生成されるファイルからバックアップします。よろしいですか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                if (Sub_Code.DVPL_File_Exists(Directory.GetCurrentDirectory() + "/Backup/Main/voiceover_crew.bnk") && Voice_Set.WoTB_Path != "")
                {
                    string[] Files = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\Backup\\Main", "*", SearchOption.AllDirectories);
                    foreach (string File_Now in Files)
                    {
                        string FileName = File_Now.Replace(Directory.GetCurrentDirectory() + "\\Backup\\Main\\", "");
                        Sub_Code.DVPL_File_Delete(Voice_Set.WoTB_Path + "/Data/WwiseSound/" + File_Now);
                        Sub_Code.DVPL_File_Copy(File_Now, Voice_Set.WoTB_Path + "/Data/WwiseSound/" + File_Now, true);
                    }
                    MessageBox.Show("バックアップから復元しました。この操作でうまく復元できていなかった場合は、\"サーバーから復元\"をお試しください。");
                }
                else
                {
                    Message_Feed_Out("WoTBのバックアップがされていません。");
                }
            }
        }
        private async void Mod_Server_Backup_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
            if (Voice_Set.WoTB_Path == "")
            {
                Message_Feed_Out("WoTBのフォルダを取得できませんでした。");
                return;
            }
            //サーバーに置いている初期状態のファイルから復元
            MessageBoxResult result = MessageBox.Show("サーバーに保存してある初期状態のファイルからバックアップします。よろしいですか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    /*FluentFTP.FtpStatus result2 = Voice_Set.FTP_Server.DownloadFile(Voice_Set.WoTB_Path + "/Data/sounds.yaml.dvpl", "/WoTB_Voice_Mod/Mods/Backup/sounds.yaml.dvpl", FluentFTP.FtpLocalExists.Overwrite);
                    Voice_Set.FTP_Server.DownloadFile(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_high.yaml.dvpl", "/WoTB_Voice_Mod/Mods/Backup/sfx_high.yaml.dvpl", FluentFTP.FtpLocalExists.Overwrite);
                    Voice_Set.FTP_Server.DownloadFile(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_low.yaml.dvpl", "/WoTB_Voice_Mod/Mods/Backup/sfx_low.yaml.dvpl", FluentFTP.FtpLocalExists.Overwrite);
                    Voice_Set.FTP_Server.DownloadFile(Voice_Set.WoTB_Path + "/Data/Sfx/GUI_battle_streamed.fsb.dvpl", "/WoTB_Voice_Mod/Mods/Backup/GUI_battle_streamed.fsb.dvpl", FluentFTP.FtpLocalExists.Overwrite);
                    Voice_Set.FTP_Server.DownloadFile(Voice_Set.WoTB_Path + "/Data/Sfx/GUI_notifications_FX_howitzer_load.fsb.dvpl", "/WoTB_Voice_Mod/Mods/Backup/GUI_notifications_FX_howitzer_load.fsb.dvpl", FluentFTP.FtpLocalExists.Overwrite);
                    Voice_Set.FTP_Server.DownloadFile(Voice_Set.WoTB_Path + "/Data/Sfx/GUI_quick_commands.fsb.dvpl", "/WoTB_Voice_Mod/Mods/Backup/GUI_quick_commands.fsb.dvpl", FluentFTP.FtpLocalExists.Overwrite);
                    Voice_Set.FTP_Server.DownloadFile(Voice_Set.WoTB_Path + "/Data/Sfx/GUI_sirene.fsb.dvpl", "/WoTB_Voice_Mod/Mods/Backup/GUI_sirene.fsb.dvpl", FluentFTP.FtpLocalExists.Overwrite);*/
                    double SizeMB = (double)(Voice_Set.FTP_Server.GetFileSize("/WoTB_Voice_Mod/Mods/Backup/New_Sound_Engine.zip") / 1000000);
                    SizeMB = (Math.Floor(SizeMB * 10)) / 10;
                    Message_T.Text = "サーバーからデータをダウンロードしています...\nファイルサイズ:約" + SizeMB + "MB";
                    await Task.Delay(50);
                    FluentFTP.FtpStatus result2 = Voice_Set.FTP_Server.DownloadFile(Voice_Set.Special_Path + "/Backup.dat", "/WoTB_Voice_Mod/Mods/Backup/New_Sound_Engine.zip", FluentFTP.FtpLocalExists.Overwrite);
                    if (result2 == FluentFTP.FtpStatus.Success)
                    {
                        System.IO.Compression.ZipFile.ExtractToDirectory(Voice_Set.Special_Path + "/Backup.dat", Voice_Set.Special_Path + "/Backup");
                        string[] Files = Directory.GetFiles(Voice_Set.Special_Path + "\\Backup", "*", SearchOption.AllDirectories);
                        foreach (string File_Now in Files)
                        {
                            string FileName = File_Now.Replace(Voice_Set.Special_Path + "\\Backup\\", "");
                            Sub_Code.DVPL_File_Delete(Voice_Set.WoTB_Path + "/Data/WwiseSound/" + FileName);
                            Sub_Code.DVPL_File_Copy(File_Now, Voice_Set.WoTB_Path + "/Data/WwiseSound/" + FileName, true);
                            File.Delete(File_Now);
                        }
                        /*File.Delete(Voice_Set.WoTB_Path + "/Data/sounds.yaml");
                        File.Delete(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_high.yaml");
                        File.Delete(Voice_Set.WoTB_Path + "/Data/Configs/Sfx/sfx_low.yaml");*/
                        File.Delete(Voice_Set.Special_Path + "/Backup.dat");
                        Message_Feed_Out("サーバーから復元しました。");
                    }
                    else
                    {
                        Message_Feed_Out("エラー:サーバーから復元できませんでした。");
                    }
                }
                catch (Exception e1)
                {
                    Message_Feed_Out("エラー:元ファイルが使用中です。");
                    Sub_Code.Error_Log_Write(e1.Message);
                    return;
                }
            }
        }
        private void Mod_Backup_Help_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
            //？ボタンを押すと表示
            string Message_01 = "バックアップからファイルの復元を試みます。\n\"サーバーから復元\"はサーバーに置いてあるWoTBの初期状態のファイルをコピーします。\n";
            string Message_02 = "\"バックアップから復元\"は、このソフトを初めて起動したときにコピーされたファイルから復元します。";
            string Message_03 = "バックアップでは、日本語の音声ファイル(voiceover_crew.bnk)と一部のSEファイル(reload,ui_battle,ui_chat_quick_commands,music_login_screen)しか復元できませんので、自身で変更したファイルは対応できません。";
            MessageBox.Show(Message_01 + Message_02 + Message_03);
        }
        private void Mod_Password_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
            //Modにパスワードがかかっている場合テキストボックスに入力されている文字と比較して同じだったらダウンロードが開始される
            XDocument xml2 = XDocument.Load(Voice_Set.FTP_Server.OpenRead("/WoTB_Voice_Mod/Mods/" + Fmod_Bank_List.Items[Fmod_Bank_List.SelectedIndex] + "/Configs.dat"));
            XElement item2 = xml2.Element("Mod_Upload_Config");
            if (item2.Element("Password").Value == Mod_Password_T.Text)
            {
                Mod_Password_B.Visibility = Visibility.Hidden;
                Mod_Password_T.Visibility = Visibility.Hidden;
                Mod_Password_Text.Visibility = Visibility.Hidden;
                Mod_Change_B.Visibility = Visibility.Hidden;
                List_Change_C.Visibility = Visibility.Hidden;
                List_Change_T.Visibility = Visibility.Hidden;
                Sample_Download();
                Explanation_Scrool.Visibility = Visibility.Visible;
                Explanation_Border.Visibility = Visibility.Visible;
                Explanation_Text.Visibility = Visibility.Visible;
                Create_Name_T.Visibility = Visibility.Visible;
                Mod_Config_T.Visibility = Visibility.Visible;
            }
            else
            {
                Message_Feed_Out("パスワードが違います。");
            }
        }
        private void Voice_Pitch_S_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Voice_Pitch_S.Value = 0;
        }
        private void Mod_Select_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
            //Modを選択した状態かつパスワードがかかっていない場合実行
            Sample_Download();
            Mod_Select_B.Visibility = Visibility.Hidden;
            Mod_Change_B.Visibility = Visibility.Hidden;
            List_Change_C.Visibility = Visibility.Hidden;
            List_Change_T.Visibility = Visibility.Hidden;
        }
        async void Message_Feed_Out(string Message)
        {
            //テキストが一定期間経ったらフェードアウト
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
                {
                    Message_T.Opacity -= 0.025;
                }
                await Task.Delay(1000 / 60);
            }
            IsMessageShowing = false;
            Message_T.Text = "";
            Message_T.Opacity = 1;
        }
        private async void Mod_Change_B_Click(object sender, RoutedEventArgs e)
        {
            if (Mod_Select_Name != "")
            {
                Mod_Change_Window.Window_Show(Mod_Select_Name);
            }
            else
            {
                Mod_Change_Window.Window_Show(Fmod_Bank_List.Items[Fmod_Bank_List.SelectedIndex].ToString());
            }
            await Task.Delay(100);
            while (Mod_Change_Window.Visibility == Visibility.Visible)
            {
                if (Sub_Code.ModChange)
                {
                    if (Fmod_Bank_List.SelectedIndex != -1)
                    {
                        if (Mod_Select_Name != "")
                        {
                            Mod_Control_Change_Visible(false);
                            Mod_Install_B.Margin = new Thickness(-1200, 125, 0, 0);
                            Language_T.Margin = new Thickness(-1250, 250, 0, 0);
                            Language_Left_B.Margin = new Thickness(-1480, 250, 0, 0);
                            Language_Right_B.Margin = new Thickness(-1020, 250, 0, 0);
                            Language_Help_B.Margin = new Thickness(-875, 250, 0, 0);
                            Mod_Install_B.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            Mod_Password_B.Visibility = Visibility.Hidden;
                            Mod_Password_T.Visibility = Visibility.Hidden;
                            Mod_Password_Text.Visibility = Visibility.Hidden;
                            Explanation_Scrool.Visibility = Visibility.Hidden;
                            Explanation_Border.Visibility = Visibility.Hidden;
                            Explanation_Text.Visibility = Visibility.Hidden;
                            Create_Name_T.Visibility = Visibility.Hidden;
                            Mod_Config_T.Visibility = Visibility.Hidden;
                            Mod_Change_B.Visibility = Visibility.Hidden;
                            Mod_Select_B.Visibility = Visibility.Hidden;
                        }
                        Message_T.Text = "";
                        Fmod_Bank_List.SelectedIndex = -1;
                    }
                    Mod_List_Update();
                    Sub_Code.ModChange = false;
                    break;
                }
                await Task.Delay(500);
            }
        }
        void Voice_Volume_MouseUp(object sender, MouseEventArgs e)
        {
            Configs_Save();
        }
        void Configs_Save()
        {
            if (!IsSaveOK)
            {
                return;
            }
            try
            {
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Configs/Voice_Mod.tmp");
                stw.WriteLine(Voice_Volume_S.Value);
                stw.WriteLine(List_Change_C.IsChecked.Value);
                stw.Write(Select_Language);
                stw.Close();
                using (var eifs = new FileStream(Voice_Set.Special_Path + "/Configs/Voice_Mod.tmp", FileMode.Open, FileAccess.Read))
                {
                    using (var eofs = new FileStream(Voice_Set.Special_Path + "/Configs/Voice_Mod.conf", FileMode.Create, FileAccess.Write))
                    {
                        FileEncode.FileEncryptor.Encrypt(eifs, eofs, "Voice_Mod_Configs_Save");
                    }
                }
                File.Delete(Voice_Set.Special_Path + "/Configs/Voice_Mod.tmp");
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        //リストをソート(名前順、公開順)
        private void List_Change_C_Click(object sender, RoutedEventArgs e)
        {
            if (Mod_Select_Name != "")
            {
                return;
            }
            if (Mod_List_Save.Count == 0)
            {
                return;
            }
            bool IsSelectedItem = false;
            if (Fmod_Bank_List.SelectedIndex != -1)
            {
                IsSelectedItem = true;
            }
            string SelectModName = "";
            if (IsSelectedItem)
            {
                SelectModName = Fmod_Bank_List.SelectedItem.ToString();
            }
            Fmod_Bank_List.Items.Clear();
            if (List_Change_C.IsChecked.Value)
            {
                List<string> List_Temp = new List<string>();
                List_Temp.AddRange(Mod_List_Save);
                List_Temp.Sort();
                foreach (string Name in List_Temp)
                {
                    Fmod_Bank_List.Items.Add(Name);
                }
            }
            else
            {
                foreach (string Name in Mod_List_Save)
                {
                    Fmod_Bank_List.Items.Add(Name);
                }
            }
            if (IsSelectedItem)
            {
                Fmod_Bank_List.SelectedItem = SelectModName;
                Fmod_Bank_List.ScrollIntoView(SelectModName);
            }
            Configs_Save();
        }
        //適応先の言語を変更
        private void Language_Left_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy || Select_Language <= 0)
            {
                return;
            }
            Select_Language--;
            Language_T.Text = "言語:" + Languages[Select_Language];
            Configs_Save();
        }
        //適応先の言語を変更
        private void Language_Right_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy || Select_Language >= 17)
            {
                return;
            }
            Select_Language++;
            Language_T.Text = "言語:" + Languages[Select_Language];
            Configs_Save();
        }
        private void Language_Help_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
            string Message_01 = "国別音声を有効化している場合ここで選択した国家以外では再生されません。\n";
            string Message_02 = "ゲーム内の言語を日本語にしている場合は\"ja\"を選択します。";
            MessageBox.Show(Message_01 + Message_02);
        }
    }
}