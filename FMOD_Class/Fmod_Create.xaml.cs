using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WoTB_Voice_Mod_Creater.FMOD_Class
{
    public partial class Fmod_Create : UserControl
    {
        List<string> WFS_File_List = new List<string>();
        bool IsClosing = false;
        bool IsMessageShowing = false;
        public Fmod_Create()
        {
            InitializeComponent();
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
        //メッセージをフェードアウト
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
            Message_T.Text = "";
            Message_T.Opacity = 1;
            IsMessageShowing = false;
        }
        private void WFS_Select_HelP_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (IsClosing)
            {
                return;
            }
            string Message_01 = "FSB抽出時(\"リスト内のすべてのファイル\"を選択した場合のみ)に生成された～.wfsを選択します。\n";
            string Message_02 = ".wfsファイルは抽出するときに指定したファルダにあります。";
            MessageBox.Show(Message_01 + Message_02);
        }
        private void WFS_Select_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
            {
                return;
            }
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Title = "WFSファイルを選択してください。";
            ofd.Multiselect = false;
            ofd.Filter = "WFSファイル(*.wfs)|*.wfs";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    string To_File = Path.GetDirectoryName(ofd.FileName) + "/" + Path.GetFileNameWithoutExtension(ofd.FileName) + ".tmp";
                    if (Sub_Code.File_Decrypt(ofd.FileName, To_File, "WoTB_FSB_Encode_Save", false))
                    {
                        StreamReader str = new StreamReader(To_File);
                        string line;
                        Audio_List.Items.Clear();
                        WFS_File_List.Clear();
                        while ((line = str.ReadLine()) != null)
                        {
                            if (line != "")
                            {
                                Audio_List.Items.Add(Path.GetFileName(line));
                                WFS_File_List.Add(line);
                            }
                        }
                        str.Close();
                        File.Delete(To_File);
                        WFS_Select_T.Text = Path.GetFileName(ofd.FileName);
                    }
                    else
                    {
                        Message_Feed_Out(".wfsファイルが破損しています。");
                        return;
                    }
                }
                catch (Exception e1)
                {
                    Sub_Code.Error_Log_Write(e1.Message);
                }
            }
        }
        private async void FSB_Create_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
            {
                return;
            }
            string Move_Files = "";
            int Move_File_Number = 0;
            bool IsIncludeJapanese = false;
            foreach (string File_Now in WFS_File_List)
            {
                if (Sub_Code.IsTextIncludeJapanese(File_Now))
                {
                    IsIncludeJapanese = true;
                }
                if (!File.Exists(File_Now))
                {
                    if (Move_File_Number <= 9)
                    {
                        Move_Files += Path.GetFileName(File_Now) + "\n";
                    }
                    else if (Move_File_Number == 10)
                    {
                        Move_Files += "...\n";
                    }
                    Move_File_Number++;
                }
            }
            if (Move_File_Number != 0)
            {
                MessageBox.Show("以下のファイルが見つかりませんでした。\n" + Move_Files);
                Message_Feed_Out("エラー:必要なファイルが見つかりませんでした。");
                return;
            }
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog()
            {
                Title = "保存先を指定してください。",
                FileName = Path.GetFileNameWithoutExtension(WFS_Select_T.Text) + ".fsb",
                Filter = "FSBファイル(*.fsb)|*.fsb"
            };
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //Android_Create.csから引用
                int Number = 0;
                start:
                Number++;
                if (Number >= 3)
                {
                    return;
                }
                try
                {
                    string SPath = Voice_Set.Special_Path + "\\Fmod_Android_Create";
                    string Cache_Path = Directory.GetCurrentDirectory() + "\\FMod_Cache";
                    List<string> Temp_Dir = new List<string>();
                    if (IsIncludeJapanese)
                    {
                        Message_T.Text = "パスに日本語が含まれているためファイルをコピーしています...";
                        Directory.CreateDirectory(SPath + "\\FSB_Create_TEMP");
                        foreach (string File_Now in WFS_File_List)
                        {
                            File.Copy(File_Now, SPath + "\\FSB_Create_TEMP/" + Path.GetFileName(File_Now), true);
                            Temp_Dir.Add(SPath + "\\FSB_Create_TEMP/" + Path.GetFileName(File_Now));
                        }
                    }
                    else
                    {
                        Temp_Dir = WFS_File_List;
                    }
                    Message_T.Text = "FSBファイルを作成しています...";
                    await Task.Delay(50);
                    File.Delete(sfd.FileName);
                    string Name_Only = Path.GetFileNameWithoutExtension(sfd.FileName);
                    StreamWriter stw = File.CreateText(SPath + "\\FSB_Create.lst");
                    foreach (string File_Now in Temp_Dir)
                    {
                        stw.WriteLine(File_Now);
                    }
                    stw.Close();
                    Directory.CreateDirectory(Cache_Path);
                    StreamWriter Bat = File.CreateText(SPath + "\\" + Name_Only + ".bat");
                    Bat.WriteLine("chcp 65001");
                    Bat.Write("\"" + SPath + "\\Fmod_Android_Create.exe\" -cache_dir \"FMod_Cache\" -format adpcm -o \"" + SPath + "\\Temp.fsb\" \"" + SPath + "\\FSB_Create.lst\"");
                    Bat.Close();
                    ProcessStartInfo processStartInfo = new ProcessStartInfo
                    {
                        FileName = SPath + "\\" + Name_Only + ".bat",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    Process p = Process.Start(processStartInfo);
                    p.WaitForExit();
                    Sub_Code.File_Move(SPath + "\\Temp.fsb", sfd.FileName, true);
                    File.Delete(SPath + "\\FSB_Create.lst");
                    File.Delete(SPath + "\\" + Name_Only + ".bat");
                    if (Directory.Exists(Cache_Path))
                    {
                        Directory.Delete(Cache_Path, true);
                    }
                    if (!File.Exists(sfd.FileName))
                    {
                        goto start;
                    }
                    Message_Feed_Out("作成しました。");
                }
                catch (Exception e1)
                {
                    Sub_Code.Error_Log_Write(e1.Message);
                    Message_Feed_Out("エラーが発生しました。");
                }
            }
        }
        private async void Back_B_Click(object sender, RoutedEventArgs e)
        {
            if (!IsClosing)
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
    }
}