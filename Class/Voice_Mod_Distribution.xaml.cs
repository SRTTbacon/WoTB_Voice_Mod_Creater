﻿using System;
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
        static FMOD_API.EventSystem ESystem_01 = new FMOD_API.EventSystem();
        public static FMOD_API.EventSystem ESystem
        {
            get { return ESystem_01; }
            set { ESystem_01 = value; }
        }
    }
    public partial class Voice_Mod_Distribution : UserControl
    {
        private readonly string Server_Mods = "https://team-astral.com/WoTB_Voice_Mod/Mods/";
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
                StreamReader str = Server_GetData.Download_To_Stream(From_File);
                while (str.EndOfStream == false)
                { 
                    string Line = str.ReadLine();
                    if (!String.IsNullOrEmpty(Line))
                        Temp.Add(Line);
                }
                str.Close();
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
            Size_Text.Visibility = Visibility.Hidden;
            Create_Name_T.Visibility = Visibility.Hidden;
            Mod_Config_T.Visibility = Visibility.Hidden;
            Mod_Select_B.Visibility = Visibility.Hidden;
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
                    StreamReader str = Sub_Code.File_Decrypt_To_Stream(Voice_Set.Special_Path + "/Configs/Voice_Mod.conf", "Voice_Mod_Configs_Save");
                    Voice_Volume_S.Value = double.Parse(str.ReadLine());
                    _ = str.ReadLine();
                    Select_Language = int.Parse(str.ReadLine());
                    str.Close();
                    Fmod_Bank_List.Items.Clear();
                    List<string> List_Temp = new List<string>();
                    List_Temp.AddRange(Mod_List_Save);
                    List_Temp.Sort();
                    foreach (string Name in List_Temp)
                        Fmod_Bank_List.Items.Add(Name);
                    Language_T.Text = "言語:" + Languages[Select_Language];
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
                Message_T.Text = "現在配布されているModはありません。";
            while (Opacity < 1 && !IsBusy)
            {
                Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
            IsSaveOK = true;
            while (Opacity > 0)
            {
                Fmod_Player.ESystem.update();
                await Task.Delay(1000 / 60);
            }
        }
        //配布されているModを更新
        void Mod_List_Update()
        {
            Fmod_Bank_List.Items.Clear();
            List<string> Mods_Read = Server_Open_File_Line(Server_Mods + "Mod_Names_Wwise.dat");
            Mod_List_Save = Mods_Read;
            Mods_Read.Sort();
            foreach (string Line in Mods_Read)
                Fmod_Bank_List.Items.Add(Line);
            if (Fmod_Bank_List.Items.Count == 0)
                Message_T.Text = "現在配布されているModはありません。";
            else if (Message_T.Text == "現在配布されているModはありません。")
                Message_T.Text = "";
        }
        //ランダム再生
        private async void Random_Play_B_Click(object sender, RoutedEventArgs e)
        {
            if (Voice_Max_Index == 0)
                return;
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
                if (!Server_GetData.URLExists(Server_Mods + Bank_Name + "/Configs.dat"))
                {
                    Message_Feed_Out("Modが見つかりませんでした。削除された可能性があります。");
                    return;
                }
                if (!Directory.Exists(Voice_Set.Special_Path + "/Server/Download_Mods/" + Bank_Name) || Directory.GetFiles(Voice_Set.Special_Path + "/Server/Download_Mods/" + Bank_Name, "*", SearchOption.AllDirectories).Length == 0)
                {
                    if (Directory.Exists(Voice_Set.Special_Path + "/Server/Download_Mods/" + Bank_Name))
                        Directory.Delete(Voice_Set.Special_Path + "/Server/Download_Mods/" + Bank_Name, true);
                    Directory.CreateDirectory(Voice_Set.Special_Path + "/Server/Download_Mods/" + Bank_Name);
                    Message_T.Text = "サンプルをダウンロードしています...";
                    await Task.Delay(50);
                    Download_P.Value = 0;
                    Download_T.Text = "進捗:0%";
                    Download_P.Visibility = Visibility.Visible;
                    Download_T.Visibility = Visibility.Visible;
                    Download_Border.Visibility = Visibility.Visible;
                    foreach (string File_Name in Server_GetData.GetFiles(Server_Mods + Bank_Name + "/Files.dat"))
                    {
                        try
                        {
                            Message_T.Text = File_Name + "をダウンロードしています...";
                            long File_Size_Full = Server_GetData.GetFileSize(Server_Mods + Bank_Name + "/Files/" + File_Name);
                            Task task = Task.Run(() =>
                            {
                                Server_GetData.Download_To_File(Server_Mods + Bank_Name + "/Files/" + File_Name, Voice_Set.Special_Path + "/Server/Download_Mods/" + Bank_Name + "/" + File_Name);
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
                    Fmod_Bank_List.Items.Add(Path.GetFileName(Name));
                string[] Dir2 = Directory.GetFiles(Voice_Set.Special_Path + "/Server/Download_Mods/" + Bank_Name, "*.bnk.dvpl", SearchOption.TopDirectoryOnly);
                foreach (string Name in Dir2)
                    Fmod_Bank_List.Items.Add(Path.GetFileName(Name));
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
                    return;
                Message_T.Text = "";
                string Bank_Name = Fmod_Bank_List.Items[Fmod_Bank_List.SelectedIndex].ToString();
                if (Mod_Select_Name != "")
                {
                    //fevの選択が変更されたら新たにロード(V1.2.2からdvplも対応)
                    try
                    {
                        Bass.BASS_ChannelStop(Stream);
                        if (Wwise_Bnk != null)
                        Wwise_Bnk.Bank_Clear();
                        string Dir = Voice_Set.Special_Path + "/Server/Download_Mods/" + Mod_Select_Name;
                        if (Path.GetExtension(Dir + "/" + Bank_Name) == ".dvpl")
                            Wwise_Bnk = new Wwise_Class.Wwise_File_Extract_V2(Dir + "/Temp/" + Bank_Name.Replace(".dvpl", ""));
                        else
                            Wwise_Bnk = new Wwise_Class.Wwise_File_Extract_V2(Dir + "/" + Bank_Name);
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
                    if (!Server_GetData.URLExists(Server_Mods + Bank_Name + "/Configs.dat"))
                    {
                        Fmod_Bank_List.SelectedIndex = -1;
                        Message_Feed_Out("選択したModは現在利用できません。");
                        return;
                    }
                    StreamReader str = Server_GetData.Download_To_Stream(Server_Mods + Bank_Name + "/Configs.dat");
                    XDocument xml2 = XDocument.Load(str);
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
                        IsEnableR18 = "あり";
                    else
                        IsEnableR18 = "なし";
                    if (bool.Parse(item2.Element("IsBGMMode").Value) == true)
                        IsBGMMode = "あり";
                    else
                        IsBGMMode = "なし";
                    Mod_Config_T.Text = "R18音声:" + IsEnableR18 + "\n戦闘BGM:" + IsBGMMode;
                    Mod_Config_T.Visibility = Visibility.Visible;
                    Create_Name_T.Text = "配布者:" + item2.Element("UserName").Value;
                    Create_Name_T.Visibility = Visibility.Visible;
                    Explanation_T.Text = item2.Element("Explanation").Value;
                    Explanation_Scrool.Visibility = Visibility.Visible;
                    Explanation_Border.Visibility = Visibility.Visible;
                    Explanation_Text.Visibility = Visibility.Visible;
                    if (Explanation_T.Text == "")
                        Explanation_T.Text = "説明なし";
                    Size_Text.Text = "サイズ:" + System.Text.Encoding.ASCII.GetString(Server_GetData.Download_To_Bytes(Server_Mods + Bank_Name + "/Size.dat")) + "MB";
                    Size_Text.Visibility = Visibility.Visible;
                    str.Close();
                }
            }
        }
        private void Fmod_Bank_List_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //リストの選択を解除
            if (IsBusy)
                return;
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
                    Size_Text.Visibility = Visibility.Hidden;
                    Create_Name_T.Visibility = Visibility.Hidden;
                    Mod_Config_T.Visibility = Visibility.Hidden;
                }
                Message_T.Text = "";
                Fmod_Bank_List.SelectedIndex = -1;
            }
        }
        async Task Set_Fmod_Bank_Play(int Voice_Number)
        {
            //fevの中から指定した位置のサウンドを再生
            if (IsBusy || Wwise_Bnk == null || Wwise_Bnk.IsClear)
                return;
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
            //Modを選択する画面か.bnkファイルを選択する画面かを変更(trueが.bnkファイルを選択する画面)
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
                Size_Text.Visibility = Visibility.Hidden;
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
                Size_Text.Visibility = Visibility.Visible;
                Bass.BASS_ChannelStop(Stream);
                if (Wwise_Bnk != null && !Wwise_Bnk.IsClear)
                    Wwise_Bnk.Bank_Clear();
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
                    Directory.Delete(Voice_Set.Special_Path + "/Server/Download_Mods/" + Mod_Select_Name, true);
                Mod_Install_B.Margin = new Thickness(-1200, 125, 0, 0);
                Language_T.Margin = new Thickness(-1250, 250, 0, 0);
                Language_Left_B.Margin = new Thickness(-1480, 250, 0, 0);
                Language_Right_B.Margin = new Thickness(-1020, 250, 0, 0);
                Language_Help_B.Margin = new Thickness(-875, 250, 0, 0);
                Mod_Back_B.Visibility = Visibility.Hidden;
                Explanation_Scrool.Visibility = Visibility.Hidden;
                Explanation_Border.Visibility = Visibility.Hidden;
                Explanation_Text.Visibility = Visibility.Hidden;
                Size_Text.Visibility = Visibility.Hidden;
                Create_Name_T.Visibility = Visibility.Hidden;
                Mod_Config_T.Visibility = Visibility.Hidden;
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
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, Voice_Volume);
        }
        private void Voice_Pitch_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //ピッチを変更(ダブルクリックで初期化)
            Voice_Pitch = (float)Voice_Pitch_S.Value;
            Voice_Pitch_T.Text = "ピッチ:" + Math.Round(Voice_Pitch_S.Value, 2, MidpointRounding.AwayFromZero);
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, SetFirstFreq + Voice_Pitch);
        }
        private void Mod_Back_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
                return;
            //fevを選択する画面からModを選ぶ画面に戻る
            Mod_Control_Change_Visible(false);
            if (Directory.Exists(Voice_Set.Special_Path + "/Server/Download_Mods/" + Mod_Select_Name) && Mod_Select_Name != "")
                Directory.Delete(Voice_Set.Special_Path + "/Server/Download_Mods/" + Mod_Select_Name, true);
            Mod_Install_B.Margin = new Thickness(-1200, 125, 0, 0);
            Language_T.Margin = new Thickness(-1250, 250, 0, 0);
            Language_Left_B.Margin = new Thickness(-1480, 250, 0, 0);
            Language_Right_B.Margin = new Thickness(-1020, 250, 0, 0);
            Language_Help_B.Margin = new Thickness(-875, 250, 0, 0);
            Mod_Back_B.Visibility = Visibility.Hidden;
            Explanation_Scrool.Visibility = Visibility.Hidden;
            Explanation_Border.Visibility = Visibility.Hidden;
            Explanation_Text.Visibility = Visibility.Hidden;
            Size_Text.Visibility = Visibility.Hidden;
            Create_Name_T.Visibility = Visibility.Hidden;
            Mod_Config_T.Visibility = Visibility.Hidden;
            Language_T.Visibility = Visibility.Hidden;
            Language_Left_B.Visibility = Visibility.Hidden;
            Language_Right_B.Visibility = Visibility.Hidden;
            Language_Help_B.Visibility = Visibility.Hidden;
            Mod_Select_Name = "";
            Message_T.Text = "";
            Mod_List_Update();
        }
        private async void Mod_Install_B_Click(object sender, RoutedEventArgs e)
        {
            if (Voice_Set.WoTB_Path == "")
            {
                Message_Feed_Out("WoTBのフォルダを取得できませんでした。");
                return;
            }
            if (IsBusy || Opacity < 1)
                return;
            //選択したModをWoTBに導入
            IsBusy = true;
            Message_T.Text = "ファイルをコピーしています...";
            await Task.Delay(50);
            if (!Directory.Exists(Voice_Set.WoTB_Path + "/Data/Mods"))
                Directory.CreateDirectory(Voice_Set.WoTB_Path + "/Data/Mods");
            string[] Dir = Directory.GetFiles(Voice_Set.Special_Path + "/Server/Download_Mods/" + Mod_Select_Name, "*", SearchOption.TopDirectoryOnly);
            List<string> FEV_List = new List<string>();
            foreach (string Name in Dir)
            {
                string Name_Only = Path.GetFileName(Name).Replace(".dvpl", "");
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
            IsBusy = false;
            Message_Feed_Out("インストールしました。");
        }
        private void Mod_Backup_B_Click(object sender, RoutedEventArgs e)
        {
            //起動時にコピーされるファイルから復元
            if (IsBusy)
                return;
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
                    MessageBox.Show("バックアップから復元しました。");
                }
                else
                    Message_Feed_Out("WoTBのバックアップがされていません。");
            }
        }
        private void Mod_Backup_Help_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
                return;
            //？ボタンを押すと表示
            string Message_01 = "\"バックアップから復元\"は、このソフトを初めて起動したときにコピーされたファイルから復元します。";
            string Message_02 = "バックアップでは、日本語の音声ファイル(voiceover_crew.bnk)と一部のSEファイル(reload,ui_battle,ui_chat_quick_commands,music_login_screen)しか復元できませんので、自身で変更したファイルは対応できません。";
            MessageBox.Show(Message_01 + Message_02);
        }
        private void Mod_Password_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
                return;
            //Modにパスワードがかかっている場合テキストボックスに入力されている文字と比較して同じだったらダウンロードが開始される
            StreamReader str = Server_GetData.Download_To_Stream(Server_Mods + Fmod_Bank_List.Items[Fmod_Bank_List.SelectedIndex] + "/Configs.dat");
            XDocument xml2 = XDocument.Load(str);
            XElement item2 = xml2.Element("Mod_Upload_Config");
            if (item2.Element("Password").Value == Mod_Password_T.Text)
            {
                Mod_Password_B.Visibility = Visibility.Hidden;
                Mod_Password_T.Visibility = Visibility.Hidden;
                Mod_Password_Text.Visibility = Visibility.Hidden;
                Sample_Download();
                Explanation_Scrool.Visibility = Visibility.Visible;
                Explanation_Border.Visibility = Visibility.Visible;
                Explanation_Text.Visibility = Visibility.Visible;
                Size_Text.Visibility = Visibility.Visible;
                Create_Name_T.Visibility = Visibility.Visible;
                Mod_Config_T.Visibility = Visibility.Visible;
            }
            else
                Message_Feed_Out("パスワードが違います。");
            str.Close();
        }
        private void Voice_Pitch_S_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Voice_Pitch_S.Value = 0;
        }
        private void Mod_Select_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
                return;
            //Modを選択した状態かつパスワードがかかっていない場合実行
            Sample_Download();
            Mod_Select_B.Visibility = Visibility.Hidden;
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
                    Message_T.Opacity -= 0.025;
                await Task.Delay(1000 / 60);
            }
            IsMessageShowing = false;
            Message_T.Text = "";
            Message_T.Opacity = 1;
        }
        void Voice_Volume_MouseUp(object sender, MouseEventArgs e)
        {
            Configs_Save();
        }
        void Configs_Save()
        {
            if (!IsSaveOK)
                return;
            try
            {
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Configs/Voice_Mod.tmp");
                stw.WriteLine(Voice_Volume_S.Value);
                stw.Write(Select_Language);
                stw.Close();
                using (var eifs = new FileStream(Voice_Set.Special_Path + "/Configs/Voice_Mod.tmp", FileMode.Open, FileAccess.Read))
                    using (var eofs = new FileStream(Voice_Set.Special_Path + "/Configs/Voice_Mod.conf", FileMode.Create, FileAccess.Write))
                        FileEncode.FileEncryptor.Encrypt(eifs, eofs, "Voice_Mod_Configs_Save");
                File.Delete(Voice_Set.Special_Path + "/Configs/Voice_Mod.tmp");
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        //適応先の言語を変更
        private void Language_Left_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy || Select_Language <= 0)
                return;
            Select_Language--;
            Language_T.Text = "言語:" + Languages[Select_Language];
            Configs_Save();
        }
        //適応先の言語を変更
        private void Language_Right_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy || Select_Language >= 17)
                return;
            Select_Language++;
            Language_T.Text = "言語:" + Languages[Select_Language];
            Configs_Save();
        }
        private void Language_Help_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
                return;
            string Message_01 = "国別音声を有効化している場合ここで選択した国家以外では再生されません。\n";
            string Message_02 = "ゲーム内の言語を日本語にしている場合は\"ja\"を選択します。";
            MessageBox.Show(Message_01 + Message_02);
        }
    }
}