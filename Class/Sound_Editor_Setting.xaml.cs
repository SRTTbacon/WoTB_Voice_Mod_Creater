using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WK.Libraries.BetterFolderBrowserNS;

namespace WoTB_Voice_Mod_Creater.Class
{
    public partial class Sound_Editor_Setting : UserControl
    {
        public string Save_Dir = "";
        bool IsClosing = false;
        bool IsMessageShowing = false;
        public bool IsConfigsLoaded = false;
        public Sound_Editor_Setting()
        {
            InitializeComponent();
            Save_File_Mode_C.Items.Add("連番(001)");
            Save_File_Mode_C.Items.Add("連番(01)");
            Save_File_Mode_C.Items.Add("連番(1)");
            Save_File_Mode_C.Items.Add("時刻(A時B分C秒)");
            Save_File_Mode_C.Items.Add("ランダム(2～6文字)");
            Save_File_Mode_C.SelectedIndex = 0;
            Save_Ex_C.Items.Add(".wav");
            Save_Ex_C.Items.Add(".mp3");
            Save_Ex_C.SelectedIndex = 0;
            Framerate_C.Items.Add("30FPS");
            Framerate_C.Items.Add("60FPS");
            Framerate_C.Items.Add("120FPS");
            Framerate_C.SelectedIndex = 1;
            Volume_S.Value = 75;
            Save_Once_C.IsChecked = true;
            Save_File_Name_T.IsEnabled = false;
        }
        public async void Window_Show()
        {
            Opacity = 0;
            Save_File_Name_T.IsEnabled = true;
            Visibility = System.Windows.Visibility.Visible;
            while (Opacity < 1 && !IsClosing)
            {
                Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
        }
        public void Configs_Load()
        {
            if (File.Exists(Voice_Set.Special_Path + "/Configs/Sound_Editor_Setting.conf"))
            {
                try
                {
                    Sub_Code.File_Decrypt(Voice_Set.Special_Path + "/Configs/Sound_Editor_Setting.conf", Voice_Set.Special_Path + "/Configs/Sound_Editor_Setting.tmp", "Sound_Editor_Setting_Configs_Save", false);
                    StreamReader str = new StreamReader(Voice_Set.Special_Path + "/Configs/Sound_Editor_Setting.tmp");
                    Save_Dir = str.ReadLine();
                    Save_File_Name_T.Text = str.ReadLine();
                    Save_File_Mode_C.SelectedIndex = int.Parse(str.ReadLine());
                    Save_Ex_C.SelectedIndex = int.Parse(str.ReadLine());
                    Cut_ShortCut_C.IsChecked = bool.Parse(str.ReadLine());
                    Cut_Volume_C.IsChecked = bool.Parse(str.ReadLine());
                    Save_Track_Delete_C.IsChecked = bool.Parse(str.ReadLine());
                    Save_Once_C.IsChecked = bool.Parse(str.ReadLine());
                    Cut_Pos_C.IsChecked = bool.Parse(str.ReadLine());
                    Framerate_C.SelectedIndex = int.Parse(str.ReadLine());
                    Volume_S.Value = double.Parse(str.ReadLine());
                    Cut_Volume_Sync_C.IsChecked = bool.Parse(str.ReadLine());
                    Set_Speed_Mode_C.IsChecked = bool.Parse(str.ReadLine());
                    if (Save_Dir == "")
                        Select_Dir_T.Text = "未指定";
                    else
                        Select_Dir_T.Text = Save_Dir + "\\";
                    Volume_T.Text = "追加時の音量:" + Math.Round(Volume_S.Value, 1, MidpointRounding.AwayFromZero);
                    str.Close();
                    Save_Text_Change();
                    File.Delete(Voice_Set.Special_Path + "/Configs/Sound_Editor_Setting.tmp");
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show("Sound_Editor_Settingの設定を読み込めませんでした。\nエラー回避のため設定は削除されます。");
                    File.Delete(Voice_Set.Special_Path + "/Configs/Sound_Editor_Setting.conf");
                    Sub_Code.Error_Log_Write(e.Message);
                }
            }
            IsConfigsLoaded = true;
        }
        void Configs_Save()
        {
            try
            {
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Configs/Sound_Editor_Setting.tmp");
                stw.WriteLine(Save_Dir);
                stw.WriteLine(Save_File_Name_T.Text);
                stw.WriteLine(Save_File_Mode_C.SelectedIndex);
                stw.WriteLine(Save_Ex_C.SelectedIndex);
                stw.WriteLine(Cut_ShortCut_C.IsChecked.Value);
                stw.WriteLine(Cut_Volume_C.IsChecked.Value);
                stw.WriteLine(Save_Track_Delete_C.IsChecked.Value);
                stw.WriteLine(Save_Once_C.IsChecked.Value);
                stw.WriteLine(Cut_Pos_C.IsChecked.Value);
                stw.WriteLine(Framerate_C.SelectedIndex);
                stw.WriteLine(Volume_S.Value);
                stw.WriteLine(Cut_Volume_Sync_C.IsChecked.Value);
                stw.Write(Set_Speed_Mode_C.IsChecked.Value);
                stw.Close();
                Sub_Code.File_Encrypt(Voice_Set.Special_Path + "/Configs/Sound_Editor_Setting.tmp", Voice_Set.Special_Path + "/Configs/Sound_Editor_Setting.conf", "Sound_Editor_Setting_Configs_Save", true);
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
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
        private async void Back_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (Save_Once_C.IsChecked.Value)
            {
                if (!Sub_Code.IsSafeFileName(Save_File_Name_T.Text))
                {
                    Message_Feed_Out("'保存先のファイル名'に使用できない文字が含まれています。(これは空白の場合も表示されます)");
                    return;
                }
            }
            if (!IsClosing)
            {
                Save_File_Name_T.IsEnabled = true;
                IsClosing = true;
                Configs_Save();
                while (Opacity > 0)
                {
                    Opacity -= Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
                IsClosing = false;
                Visibility = System.Windows.Visibility.Hidden;
            }
        }
        void Save_Text_Change()
        {
            if (!IsLoaded)
                return;
            string Mode = "";
            if (Save_File_Mode_C.SelectedIndex == 0)
                Mode = "001";
            else if (Save_File_Mode_C.SelectedIndex == 1)
                Mode = "01";
            else if (Save_File_Mode_C.SelectedIndex == 2)
                Mode = "1";
            else if (Save_File_Mode_C.SelectedIndex == 3)
                Mode = DateTime.Now.Hour.ToString() + DateTime.Now.Minute + DateTime.Now.Second;
            else if (Save_File_Mode_C.SelectedIndex == 4)
                Mode = Sub_Code.Generate_Random_String(2, 6);
            Save_File_View_T.Text = "保存先のファイル名:" + Save_File_Name_T.Text + Mode + Save_Ex_C.Items[Save_Ex_C.SelectedIndex];
        }
        private void Save_File_Name_T_TextChanged(object sender, TextChangedEventArgs e)
        {
            Save_Text_Change();
        }
        private void Save_File_Mode_C_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Save_Text_Change();
        }
        private void Save_Ex_C_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Save_Text_Change();
        }
        private void Save_Ex_Help_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string Message_01 = ".mp3形式を選択した場合、作成された.wavファイルを.mp3に変換する時間が必要になりますので、.wav形式で保存するときの2倍ほど時間がかかります。\n";
            string Message_02 = "複数回保存するのであれば.wav形式を選択して、あとから一気に.mp3に変換する方が良いかと思います。";
            MessageBox.Show(Message_01 + Message_02);
        }
        private void Save_Dir_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            BetterFolderBrowser bfb = new BetterFolderBrowser()
            {
                Title = "保存先のフォルダを指定してください。",
                Multiselect = false,
                RootFolder = Sub_Code.Get_OpenDirectory_Path()
            };
            if (bfb.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Sub_Code.Set_Directory_Path(bfb.SelectedFolder);
                if (Sub_Code.CanDirectoryAccess(bfb.SelectedFolder))
                {
                    Select_Dir_T.Text = bfb.SelectedFolder + "\\";
                    Save_Dir = bfb.SelectedFolder;
                }
                else
                    Message_Feed_Out("指定したフォルダのアクセス権がありませんでした。");
            }
            bfb.Dispose();
        }
        private void Volume_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Volume_T.Text = "追加時の音量:" + Math.Round(Volume_S.Value, 1, MidpointRounding.AwayFromZero);
        }
    }
}