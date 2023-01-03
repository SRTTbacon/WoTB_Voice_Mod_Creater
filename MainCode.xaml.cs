using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WK.Libraries.BetterFolderBrowserNS;
using WoTB_Voice_Mod_Creater.Class;
using WoTB_Voice_Mod_Creater.Wwise_Class.BNK_To_Wwise_Project;

public static partial class StringExtensions
{
    //文字列に指定した文字が何個あるか取得
    public static int CountOf(this string self, string Name)
    {
        int count = 0;
        int index = self.IndexOf(Name, 0);
        while (index != -1)
        {
            count++;
            index = self.IndexOf(Name, index + Name.Length);
        }
        return count;
    }
}
public static class ListBoxExtensions
{
    public static List<int> SelectedIndexes(this System.Windows.Controls.ListBox list)
    {
        List<int> Temp = new List<int>();
        foreach (object Now_String in list.SelectedItems)
            Temp.Add(list.Items.IndexOf(Now_String));
        return Temp;
    }
}
public static class ButtonExtensions
{
    public static void PerformClick(this System.Windows.Controls.Button button)
    {
        if (button == null)
            throw new ArgumentNullException("値がnullです。このメゾットを実行する前に値を初期化してください。");
        IInvokeProvider provider = new ButtonAutomationPeer(button);
        provider.Invoke();
    }
}
public static class ListExtensions
{
    //リストを指定した数に分ける
    public static List<List<T>> Split<T>(this List<T> source, int Count)
    {
        List<List<T>> Temp = new List<List<T>>();
        for (int Number_01 = 0; Number_01 < source.Count; Number_01 += Count)
        {
            List<T> Temp_Now = new List<T>();
            for (int Number_02 = Number_01; Number_02 < Number_01 + Count; Number_02++)
            {
                if (source.Count <= Number_02)
                    break;
                Temp_Now.Add(source[Number_02]);
            }
            Temp.Add(Temp_Now);
        }
        return Temp;
    }
}
namespace WoTB_Voice_Mod_Creater
{
    public partial class MainCode : Window
    {
        readonly string Path = Directory.GetCurrentDirectory();
        string Primary_Display_Name = "";
        bool IsClosing = false;
        bool IsMessageShowing = false;
        bool IsFullScreen = true;
        bool IsChange_To_Wwise_Checked = false;
        bool IsDragMoveMode = false;
        readonly BrushConverter bc = new BrushConverter();
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", EntryPoint = "GetWindowText", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        public MainCode()
        {
            try
            {
                StreamWriter stw = File.CreateText(Path + "/Test.dat");
                stw.WriteLine("テストファイル");
                stw.Close();
                File.Delete(Path + "/Test.dat");
            }
            catch
            {
                MessageBox.Show("フォルダにアクセスできませんでした。ソフトを別の場所に移動する必要があります。");
                Application.Current.Shutdown();
            }
            //必要なdllがなかったら強制終了
            List<string> DLL_Error_List = Sub_Code.DLL_Exists();
            if (DLL_Error_List.Count > 0)
            {
                string DLLs = "";
                foreach (string DLL_None in DLL_Error_List)
                    DLLs += DLL_None + "\n";
                MessageBox.Show("/dll内に以下のファイルが存在しません。\n" + DLLs + "ソフトは強制終了されます。");
                Application.Current.Shutdown();
            }
            InitializeComponent();
            try
            {
                WoTB_Select_B.Visibility = Visibility.Hidden;
                //左上のアイコンを設定
                System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(Process.GetCurrentProcess().MainModule.FileName);
                BitmapSource source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                Icon_Small_Image.Source = source;
                Save_Window.Opacity = 0;
                MouseLeftButtonDown += (sender, e) => { ScreenMove(); };
                FMOD_API.EventSystem ES = new FMOD_API.EventSystem();
                FMOD_API.Event_Factory.EventSystem_Create(ref ES);
                Fmod_Player.ESystem = ES;
                Fmod_Player.ESystem.init(128, FMOD_API.INITFLAGS.NORMAL, IntPtr.Zero);
                FMOD_API.FMOD_System FModSys = new FMOD_API.FMOD_System();
                FMOD_API.Factory.System_Create(ref FModSys);
                FMOD_Class.Fmod_System.FModSystem = FModSys;
                FMOD_Class.Fmod_System.FModSystem.init(16, FMOD_API.INITFLAGS.NORMAL, IntPtr.Zero);
                Sub_Code.Init();
                Voice_Set.Special_Path = Path + "\\Resources";
                try
                {
                    if (!Directory.Exists(Voice_Set.Special_Path + "/Server"))
                        Directory.CreateDirectory(Voice_Set.Special_Path + "/Server");
                    if (!Directory.Exists(Voice_Set.Special_Path + "/Configs"))
                        Directory.CreateDirectory(Voice_Set.Special_Path + "/Configs");
                }
                catch (Exception e)
                {
                    Sub_Code.Error_Log_Write(e.Message);
                }
                Window_Show();
                if (!File.Exists(Path + "/WoTB_Path.dat"))
                    Sub_Code.WoTB_Get_Directory();
                else
                {
                    try
                    {
                        StreamReader str = Sub_Code.File_Decrypt_To_Stream(Path + "/WoTB_Path.dat", "WoTB_Directory_Path_Pass");
                        string Read = str.ReadLine();
                        str.Close();
                        if (!File.Exists(Read + "/wotblitz.exe"))
                        {
                            if (!Sub_Code.WoTB_Get_Directory())
                            {
                                WoTB_Select_B.Visibility = Visibility.Visible;
                                Message_Feed_Out("WoTBのインストール先を取得できません。手動で指定してください。");
                            }
                        }
                        else
                            Voice_Set.WoTB_Path = Read;
                    }
                    catch (Exception e)
                    {
                        if (!Sub_Code.WoTB_Get_Directory())
                        {
                            WoTB_Select_B.Visibility = Visibility.Visible;
                            Message_Feed_Out("WoTBのインストール先を取得できません。手動で指定してください。");
                        }
                        Sub_Code.Error_Log_Write(e.Message);
                    }
                }
                if (Voice_Set.WoTB_Path != "" && !Sub_Code.DVPL_File_Exists(Path + "/Backup/Main/reload.bnk"))
                {
                    string[] Languages = { "arb", "cn", "cs", "de", "en", "es", "fi", "fr", "gup", "it", "ja", "ko", "pbr", "pl", "ru", "th", "tr", "vi" };
                    foreach (string Language_Now in Languages)
                    {
                        Directory.CreateDirectory(Path + "/Backup/Main/" + Language_Now);
                        Sub_Code.DVPL_File_Copy(Voice_Set.WoTB_Path + "/Data/WwiseSound/" + Language_Now + "/voiceover_crew.bnk", Path + "/Backup/Main/" + Language_Now + "/voiceover_crew.bnk", false);
                    }
                    Sub_Code.DVPL_File_Copy(Voice_Set.WoTB_Path + "/Data/WwiseSound/reload.bnk", Path + "/Backup/Main/reload.bnk", false);
                    Sub_Code.DVPL_File_Copy(Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_chat_quick_commands.bnk", Path + "/Backup/Main/ui_chat_quick_commands.bnk", false);
                    Sub_Code.DVPL_File_Copy(Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_battle.bnk", Path + "/Backup/Main/ui_battle.bnk", false);
                    Sub_Code.DVPL_File_Copy(Voice_Set.WoTB_Path + "/Data/WwiseSound/ui_battle_basic.bnk", Path + "/Backup/Main/ui_battle_basic.bnk", false);
                }
                Version_T.Text = "V" + SRTTbacon.Version.Version_Name;
                try
                {
                    File.Delete(Voice_Set.Special_Path + "/FSB_Select_Temp_01.fsb");
                    File.Delete(Voice_Set.Special_Path + "/FSB_Select_Temp_02.fsb");
                    File.Delete(Voice_Set.Special_Path + "/FSB_Select_Temp_03.fsb");
                    if (Directory.Exists(Voice_Set.Special_Path + "/Wwise"))
                    {
                        File.Delete(Voice_Set.Special_Path + "/Wwise/Temp_01.ogg");
                        File.Delete(Voice_Set.Special_Path + "/Wwise/Temp_02.ogg");
                        File.Delete(Voice_Set.Special_Path + "/Wwise/WoT_To_Blitz_Temp.ogg");
                    }
                }
                catch { }
                Flash.Handle = this;
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;
                //Texture_Editor_Window.Window_Show();
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                MessageBox.Show("エラーが発生しました。作者にError_Log.txtを送ってください。\nソフトは強制終了されます。");
                Application.Current.Shutdown();
            }
        }
        private async void Exit_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            MessageBoxResult result = MessageBox.Show("終了しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                IsClosing = true;
                Voice_Set.App_Busy = true;
                Other_Window.Pause_Volume_Animation(true, 25);
                while (Opacity > 0)
                {
                    Opacity -= 0.05;
                    await Task.Delay(1000 / 60);
                }
                try
                {
                    if (Directory.Exists(Voice_Set.Special_Path + "/Wwise/BNK_WAV"))
                        Directory.Delete(Voice_Set.Special_Path + "/Wwise/BNK_WAV", true);
                    if (Directory.Exists(Voice_Set.Special_Path + "/Wwise/BNK_WAV_WoT"))
                        Directory.Delete(Voice_Set.Special_Path + "/Wwise/BNK_WAV_WoT", true);
                }
                catch (Exception e1)
                {
                    Sub_Code.Error_Log_Write(e1.Message);
                }
                Application.Current.Shutdown();
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            System.Drawing.Size MaxSize = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size;
            MaxWidth = MaxSize.Width;
            MaxHeight = MaxSize.Height;
            Video_Mode.Width = Width;
            if (IsFullScreen)
            {
                Left = 0;
                Top = 0;
            }
            if (File.Exists(Voice_Set.Special_Path + "/Configs/Main_Configs_Save.conf"))
            {
                try
                {
                    using (StreamReader str = Sub_Code.File_Decrypt_To_Stream(Voice_Set.Special_Path + "/Configs/Main_Configs_Save.conf", "SRTTbacon_Main_Config_Save"))
                    {
                        IsFullScreen = bool.Parse(str.ReadLine());
                        Sub_Code.IsWindowBarShow = bool.Parse(str.ReadLine());
                        try
                        {
                            string Display_Name = str.ReadLine();
                            foreach (System.Windows.Forms.Screen Screen_Info in System.Windows.Forms.Screen.AllScreens)
                            {
                                if (Screen_Info.DeviceName == Display_Name)
                                {
                                    Primary_Display_Name = Display_Name;
                                    Left = Screen_Info.Bounds.Left;
                                    Top = Screen_Info.Bounds.Top;
                                    break;
                                }
                            }
                        }
                        catch { }
                        str.Close();
                        if (!IsFullScreen)
                            Window_Size_Change(false);
                    }
                }
                catch (Exception e1)
                {
                    Sub_Code.Error_Log_Write(e1.Message);
                }
            }
            if (Sub_Code.IsWindowBarShow)
                WindowBarMode_Image.Source = Sub_Code.Check_03;
            else
                WindowBarMode_Image.Source = Sub_Code.Check_01;
            Button_Move();
        }
        //ウィンドウが最大化していない場合ドラッグで移動
        void ScreenMove()
        {
            if (!IsFullScreen && IsDragMoveMode && !Video_Mode.IsVideoClicked)
                DragMove();
            else if (!IsFullScreen && !Sub_Code.IsWindowBarShow && !Video_Mode.IsVideoClicked)
                DragMove();
        }
        //ウィンドウのフェードイン
        async void Window_Show()
        {
            Opacity = 0;
            Loop();
            while (Opacity < 1 && !IsClosing)
            {
                Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
        }
        //キャッシュを削除
        private void Cache_Delete_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            string Message_01 = "現在の設定を削除します。この操作は取り消せません。よろしいですか？";
            MessageBoxResult result = MessageBox.Show(Message_01, "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    Directory.Delete(Voice_Set.Special_Path + "/Server", true);
                    Directory.Delete(Voice_Set.Special_Path + "/Configs", true);
                    string[] Dirs = Directory.GetDirectories(Path + "/Backup", "*", SearchOption.TopDirectoryOnly);
                    foreach (string Dir in Dirs)
                    {
                        string Dir_Name_Only = System.IO.Path.GetFileName(Dir);
                        if (Dir_Name_Only == "Main")
                            continue;
                        try
                        {
                            Directory.Delete(Dir, true);
                        }
                        catch (Exception e1)
                        {
                            Sub_Code.Error_Log_Write(e1.Message);
                        }
                    }
                    Directory.CreateDirectory(Voice_Set.Special_Path + "/Server");
                    MessageBox.Show("各画面の設定を削除しました。");
                }
                catch (Exception m)
                {
                    MessageBox.Show("設定を削除できませんでした。ファイルが使用中でないか確認してください。\nエラー:" + m.Message);
                    Sub_Code.Error_Log_Write(m.Message);
                }
            }
        }
        private void Voice_Mod_Free_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            Message_Feed_Out("現在この機能は使用できません。");
            //Voice_Mods_Window.Window_Show();
        }
        private void Tool_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            Tools_Window.Window_Show();
        }
        private void Other_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            Other_Window.Window_Show();
        }
        private void WoTB_Select_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            BetterFolderBrowser ofd = new BetterFolderBrowser()
            {
                Title = "WoTBのインストール先を選択してください。",
                RootFolder = Sub_Code.Get_OpenDirectory_Path(),
                Multiselect = false
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Sub_Code.Set_Directory_Path(ofd.SelectedFolder);
                if (!ofd.SelectedFolder.Contains("\\steamapps\\") && !ofd.SelectedFolder.Contains("\\Steam\\"))
                    Message_Feed_Out("steamappsフォルダ以降の階層のフォルダを選択する必要があります。");
                ApplyAllFiles(ofd.SelectedFolder, ProcessFile);
                if (Voice_Set.WoTB_Path == "")
                    Message_Feed_Out("WoTBのフォルダを取得できませんでした。");
            }
        }
        void ProcessFile(string path)
        {
            string Dir = System.IO.Path.GetDirectoryName(path);
            if (Sub_Code.File_Exists(Dir + "/sounds"))
            {
                string WoTB_Path = Dir.Substring(0, Dir.LastIndexOf('\\'));
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Temp_WoTB_Path.dat");
                stw.Write(WoTB_Path);
                stw.Close();
                Sub_Code.File_Encrypt(Voice_Set.Special_Path + "/Temp_WoTB_Path.dat", Path + "/WoTB_Path.dat", "WoTB_Directory_Path_Pass", true);
                Voice_Set.WoTB_Path = WoTB_Path;
                Message_Feed_Out("WoTBのフォルダを取得しました。");
                WoTB_Select_B.Visibility = Visibility.Hidden;
                return;
            }
        }
        void ApplyAllFiles(string folder, Action<string> fileAction)
        {
            foreach (string file in Directory.GetFiles(folder, "sounds.*"))
            {
                if (file.Contains("World of Tanks Blitz"))
                {
                    fileAction(file);
                    return;
                }
            }
            foreach (string subDir in Directory.GetDirectories(folder))
            {
                try
                {
                    ApplyAllFiles(subDir, fileAction);
                }
                catch (Exception e)
                {
                    Sub_Code.Error_Log_Write(e.Message);
                }
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
        void Main_Config_Save()
        {
            try
            {
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Configs/Main_Configs_Save.tmp");
                stw.WriteLine(IsFullScreen);
                stw.WriteLine(Sub_Code.IsWindowBarShow);
                stw.Write(Primary_Display_Name);
                stw.Close();
                Sub_Code.File_Encrypt(Voice_Set.Special_Path + "/Configs/Main_Configs_Save.tmp", Voice_Set.Special_Path + "/Configs/Main_Configs_Save.conf", "SRTTbacon_Main_Config_Save", true);
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        //ウィンドウサイズを変更
        void Window_Size_Change(bool IsChangeSize)
        {
            try
            {
                if (IsChangeSize)
                    IsFullScreen = !IsFullScreen;
                System.Windows.Forms.Screen Screen_Info = System.Windows.Forms.Screen.FromPoint(new System.Drawing.Point((int)Left, (int)Top));
                System.Drawing.Rectangle MaxSize = Screen_Info.Bounds;
                if (!IsFullScreen)
                {
                    Width = ((double)MaxSize.Width / 1.25);
                    Height = ((double)MaxSize.Height / 1.25);
                    Left = (MaxSize.Width - Width) / 2 + MaxSize.Left;
                    Top = (MaxSize.Height - Height) / 2 + MaxSize.Top;
                }
                else
                {
                    Width = MaxSize.Width;
                    Height = MaxSize.Height;
                    Left = MaxSize.Left;
                    Top = MaxSize.Top;
                }
                Video_Mode.Width = Width;
            }
            catch (Exception e1)
            {
                Sub_Code.Error_Log_Write(e1.Message);
                Message_Feed_Out("画面サイズを変更できませんでした。");
            }
        }
        bool IsOtherWindowShowed()
        {
            if (Save_Window.Visibility == Visibility.Visible || Voice_Mods_Window.Visibility == Visibility.Visible || Tools_Window.Visibility == Visibility ||
                Other_Window.Visibility == Visibility.Visible || Voice_Create_Window.Visibility == Visibility.Visible ||
                Tools_V2_Window.Visibility == Visibility.Visible || Change_To_Wwise_Window.Visibility == Visibility.Visible || WoT_To_Blitz_Window.Visibility == Visibility.Visible ||
                Blitz_To_WoT_Window.Visibility == Visibility.Visible || Bank_Editor_Window.Visibility == Visibility.Visible || Create_Save_File_Window.Visibility == Visibility.Visible ||
                Create_Loading_BGM_Window.Visibility == Visibility.Visible || BNK_Event_Window.Visibility == Visibility.Visible || BNK_To_Project_Window.Visibility == Visibility.Visible ||
                Sound_Editor_Window.Visibility == Visibility.Visible || WoT_Sound_Mod_Window.Visibility == Visibility.Visible)
                return true;
            return false;
        }
        //キーが押されたときの処理
        private async void DockPanel_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (IsClosing)
                return;
            if (Other_Window.Visibility == Visibility.Visible)
                Other_Window.RootWindow_KeyDown(e);
            //ファイル名を入力中にShift+Fが働いてしまうと困るので設定
            if (Sound_Editor_Window.Setting_Window.Visibility == Visibility)
                return;
            if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Shift) == System.Windows.Forms.Keys.Shift && e.Key == System.Windows.Input.Key.F)
            {
                Window_Size_Change(true);
                Main_Config_Save();
            }
            if (IsOtherWindowShowed())
                return;
            if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Shift) == System.Windows.Forms.Keys.Shift && e.Key == System.Windows.Input.Key.G)
            {
                BetterFolderBrowser bfb = new BetterFolderBrowser()
                {
                    Title = "保存先のフォルダを選択してください。",
                    RootFolder = Sub_Code.Get_OpenDirectory_Path(),
                    Multiselect = false,
                };
                if (bfb.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    foreach (string Project_File in Directory.GetFiles(bfb.SelectedFolder))
                    {
                        List<string> Lines = new List<string>(File.ReadAllLines(Project_File));
                        bool IsEnd = false;
                        foreach (string line in Lines)
                        {
                            if (line.Contains("<GameParameter Name="))
                            {
                                string GameParam_Name = Get_Config.Get_Name(line);
                                uint ID = WwiseHash.HashString(GameParam_Name);
                                if (ID == 682857933)
                                {
                                    IsEnd = true;
                                    MessageBox.Show("発見しました!\nファイル名:" + System.IO.Path.GetFileName(Project_File) + "\n" + "RTPC名:" + GameParam_Name);
                                    break;
                                }
                            }
                        }
                        Lines.Clear();
                        if (IsEnd)
                            break;
                    }
                }
            }
            //アンインストール
            if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Shift) == System.Windows.Forms.Keys.Shift && e.Key == System.Windows.Input.Key.Escape)
            {
                if (IsClosing || Voice_Set.App_Busy)
                    return;
                IsClosing = true;
                MessageBoxResult result = MessageBox.Show("ソフトをアンインストールしますか？これには一時ファイルも削除されます。\n注意:この操作は取り消しできません。", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
                if (result == MessageBoxResult.Yes)
                {
                    Voice_Set.App_Busy = true;
                    Other_Window.Pause_Volume_Animation(true, 25);
                    Message_T.Text = "一時ファイルを削除しています...";
                    await Task.Delay(50);
                    try
                    {
                        Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/Configs");
                        Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/DVPL");
                        Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/Encode_Mp3");
                        Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/Loading");
                        Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/SE");
                        Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/WoT_SE");
                        Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/Server");
                        Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/Wwise");
                        Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/Wwise_Parse");
                        Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/WEM_To_WAV");
                        Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/Other");
                        Sub_Code.Directory_Delete(Voice_Set.Special_Path + "/Temp");
                    }
                    catch (Exception e1)
                    {
                        Sub_Code.Error_Log_Write(e1.Message);
                    }
                    Other_Window.Pause_Volume_Animation(true, 25);
                    while (Opacity > 0)
                    {
                        Opacity -= Sub_Code.Window_Feed_Time;
                        await Task.Delay(1000 / 60);
                    }
                    StreamWriter stw = File.CreateText(System.IO.Path.GetTempPath() + "/WoTB_Voice_Mod_Creater_Remove.bat");
                    stw.WriteLine("timeout 2");
                    stw.WriteLine("del " + Path + "/WoTB_Voice_Mod_Creater.exe");
                    stw.WriteLine("del " + Path + "/ChangeLog.txt");
                    stw.WriteLine("del " + Path + "/Error_Log.txt");
                    stw.WriteLine("del " + Path + "/TempDirPath.dat");
                    stw.WriteLine("del " + Path + "/User.dat");
                    stw.WriteLine("del " + Path + "/WoTB_Path.dat");
                    stw.WriteLine("rd /s /q " + Path + "/dll");
                    stw.WriteLine("rd /s /q " + Path + "/Backup");
                    stw.WriteLine("rd /s /q " + Path + "/Projects");
                    stw.WriteLine("rd /s /q " + Path + "/Youtube");
                    stw.Close();
                    ProcessStartInfo processStartInfo = new ProcessStartInfo
                    {
                        FileName = System.IO.Path.GetTempPath() + "/WoTB_Voice_Mod_Creater_Remove.bat",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    Process.Start(processStartInfo);
                    Application.Current.Shutdown();
                }
            }
            //エラーログをクリア
            if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Shift) == System.Windows.Forms.Keys.Shift && e.Key == System.Windows.Input.Key.L)
            {
                try
                {
                    if (!File.Exists(Path + "/Error_Log.txt"))
                        Message_Feed_Out("ログファイルが存在しません。");
                    StreamReader str = new StreamReader(Path + "/Error_Log.txt");
                    string Temp = str.ReadToEnd();
                    str.Close();
                    if (Temp == "")
                        Message_Feed_Out("ログはすでにクリアされています。");
                    MessageBoxResult result = MessageBox.Show("エラーログをクリアしますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
                    if (result == MessageBoxResult.Yes)
                        File.WriteAllText(Path + "/Error_Log.txt", "");
                }
                catch (Exception e1)
                {
                    Message_Feed_Out("エラーログをクリアできませんでした。");
                    Sub_Code.Error_Log_Write(e1.Message);
                }
            }
            //一時フォルダの位置を確認
            if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Shift) == System.Windows.Forms.Keys.Shift && e.Key == System.Windows.Input.Key.P)
                MessageBox.Show("一時フォルダ場所:" + Voice_Set.Special_Path);
            //超上級者向け
            if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Shift) == System.Windows.Forms.Keys.Shift && e.Key == System.Windows.Input.Key.E)
                BNK_Event_Window.Window_Show();
            IsClosing = false;
        }
        int CountChar(string s, char c)
        {
            return s.Length - s.Replace(c.ToString(), "").Length;
        }
        private void Voice_Create_V2_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            IsMessageShowing = false;
            Voice_Create_Window.Window_Show();
        }
        private void Tool_V2_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            Tools_V2_Window.Window_Show();
        }
        private void Advanced_Mode_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            Bank_Editor_Window.Window_Show();
        }
        private void Change_Wwise_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            if (!IsChange_To_Wwise_Checked)
            {
                Voice_Set.Voice_BGM_Change_List_Init();
                IsChange_To_Wwise_Checked = true;
            }
            Change_To_Wwise_Window.Window_Show();
        }
        private void WoT_To_Blitz_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            WoT_To_Blitz_Window.Window_Show();
        }
        private void WoWS_WoTB_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            WoWS_To_WoTB_Window.Window_Show();
        }
        private void Blitz_To_WoT_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            Blitz_To_WoT_Window.Window_Show();
        }
        private void Create_Save_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            Create_Save_File_Window.Window_Show();
        }
        private void Loading_BGM_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            Create_Loading_BGM_Window.Window_Show();
        }
        private void BNK_To_Project_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            if (!IsChange_To_Wwise_Checked)
            {
                Voice_Set.Voice_BGM_Change_List_Init();
                IsChange_To_Wwise_Checked = true;
            }
            BNK_To_Project_Window.Window_Show();
        }
        private void Window_PreviewDragOver(object sender, DragEventArgs e)
        {
            string[] Drag_Files = (string[])e.Data.GetData(DataFormats.FileDrop);
            string Ex = System.IO.Path.GetExtension(Drag_Files[0]);
            if (Ex == ".wvs" || Ex == ".wms" || Ex == ".wse")
                e.Effects = DragDropEffects.Copy;
            else if (Other_Window.Visibility == Visibility.Visible)
            {
                if (Ex == ".mp3" || Ex == ".wav" || Ex == ".ogg" || Ex == ".aiff" || Ex == ".flac" || Ex == ".m4a" || Ex == ".mp4")
                    e.Effects = DragDropEffects.Copy;
                else
                    e.Effects = DragDropEffects.None;
            }
            else if (Sound_Editor_Window.Visibility == Visibility.Visible)
            {
                if (Ex == ".mp3" || Ex == ".wav")
                    e.Effects = DragDropEffects.Copy;
                else
                    e.Effects = DragDropEffects.None;
            }
            else if (Voice_Create_Window.Visibility == Visibility.Visible || WoT_Sound_Mod_Window.Visibility == Visibility.Visible)
            {
                if (Ex == ".mp3" || Ex == ".wav" || Ex == ".ogg" || Ex == ".wma" || Ex == ".flac" || Ex == ".mp4")
                    e.Effects = DragDropEffects.Copy;
                else
                    e.Effects = DragDropEffects.None;
            }
            else
                e.Effects = DragDropEffects.None;
            e.Handled = true;
        }
        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (IsClosing)
                return;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                try
                {
                    string[] Drop_Files = e.Data.GetData(DataFormats.FileDrop) as string[];
                    string Ex = System.IO.Path.GetExtension(Drop_Files[0]);
                    if (Other_Window.Visibility == Visibility.Visible)
                    {
                        if (Ex == ".mp3" || Ex == ".wav" || Ex == ".ogg" || Ex == ".aiff" || Ex == ".flac" || Ex == ".m4a" || Ex == ".mp4")
                            Other_Window.Add_Music_From_Drop(Drop_Files);
                        else
                            Message_Feed_Out("対応したファイルをドラッグしてください。");
                    }
                    else if (Voice_Create_Window.Visibility == Visibility.Visible)
                    {
                        if (Ex == ".wvs")
                            Voice_Create_Window.Voice_Load_From_File(Drop_Files[0]);
                        if (Ex == ".mp3" || Ex == ".wav" || Ex == ".ogg" || Ex == ".wma" || Ex == ".flac" || Ex == ".mp4")
                            Voice_Create_Window.Add_Voice(Drop_Files, true);
                        else
                            Message_Feed_Out("対応したファイルをドラッグしてください。");
                    }
                    else if (WoT_Sound_Mod_Window.Visibility == Visibility.Visible)
                    {
                        if (Ex == ".wvs")
                            WoT_Sound_Mod_Window.Voice_Load_From_File(Drop_Files[0]);
                        if (Ex == ".mp3" || Ex == ".wav" || Ex == ".ogg" || Ex == ".wma" || Ex == ".flac" || Ex == ".mp4")
                            WoT_Sound_Mod_Window.Add_Voice(Drop_Files, true);
                        else
                            Message_Feed_Out("対応したファイルをドラッグしてください。");
                    }
                    else if (Sound_Editor_Window.Visibility == Visibility.Visible)
                    {
                        if (Ex == ".mp3" || Ex == ".wav")
                            Sound_Editor_Window.Add_Sound_File(Drop_Files);
                        else if (Ex == ".wse")
                            Sound_Editor_Window.Contents_Load(Drop_Files[0]);
                        else
                            Message_Feed_Out("対応したファイルをドラッグしてください。");
                    }
                    else if (Ex == ".wvs")
                    {
                        Voice_Create_Window.Window_Show();
                        Voice_Create_Window.Voice_Load_From_File(Drop_Files[0]);
                    }
                    else if (Ex == ".wms")
                    {
                        if (Create_Loading_BGM_Window.Visibility != Visibility.Visible)
                            Create_Loading_BGM_Window.Window_Show();
                        Create_Loading_BGM_Window.Load_From_File(Drop_Files[0]);
                    }
                    else if (Ex == ".wse")
                    {
                        Sound_Editor_Window.Window_Show();
                        Sound_Editor_Window.Contents_Load(Drop_Files[0]);
                    }
                    else
                        Message_Feed_Out("対応したファイルをドラッグしてください。");
                }
                catch (Exception e1)
                {
                    Message_Feed_Out("ファイルを読み込めませんでした。");
                    Sub_Code.Error_Log_Write(e1.Message);
                }
            }
        }
        private void Change_Log_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            ChangeLog_Window.Window_Show();
        }
        async void Loop()
        {
            while (true)
            {
                StringBuilder sb = new StringBuilder(65535);
                GetWindowText(GetForegroundWindow(), sb, 65535);
                if (sb.ToString() == "WoTB_Voice_Mod_Creater")
                    Sub_Code.IsForcusWindow = true;
                else
                    Sub_Code.IsForcusWindow = false;
                await Task.Delay(50);
            }
        }
        private void Sound_Editor_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            Sound_Editor_Window.Window_Show();
        }
        private void Minimize_B_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void Maximize_B_Click(object sender, RoutedEventArgs e)
        {
            Window_Size_Change(true);
            Main_Config_Save();
        }
        private void Close_B_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void Window_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (IsDragMoveMode)
            {
                System.Windows.Forms.Screen Screen_Info = System.Windows.Forms.Screen.FromPoint(new System.Drawing.Point((int)Left, (int)Top));
                Primary_Display_Name = Screen_Info.DeviceName;
                Main_Config_Save();
            }
            IsDragMoveMode = false;
        }
        private void Canvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IsDragMoveMode = true;
            if (e.ClickCount == 2)
                Maximize_B_Click(null, null);
        }
        void Button_Move()
        {
            if (Sub_Code.IsWindowBarShow)
                WindowBarCanvas.Visibility = Visibility.Visible;
            else
                WindowBarCanvas.Visibility = Visibility.Hidden;
        }
        private void WindowBarMode_Image_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Sub_Code.IsWindowBarShow)
            {
                Sub_Code.IsWindowBarShow = false;
                WindowBarMode_Image.Source = Sub_Code.Check_02;
            }
            else
            {
                Sub_Code.IsWindowBarShow = true;
                WindowBarMode_Image.Source = Sub_Code.Check_04;
            }
            Button_Move();
            Main_Config_Save();
        }
        private void WindowBarMode_Image_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Sub_Code.IsWindowBarShow)
                WindowBarMode_Image.Source = Sub_Code.Check_04;
            else
                WindowBarMode_Image.Source = Sub_Code.Check_02;
        }
        private void WindowBarMode_Image_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Sub_Code.IsWindowBarShow)
                WindowBarMode_Image.Source = Sub_Code.Check_03;
            else
                WindowBarMode_Image.Source = Sub_Code.Check_01;
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //ウィンドウが画面外に行ったときサイズが変更されないように
            if (Top <= 0 && !IsFullScreen)
            {
                double Left_Before = Left;
                Window_Size_Change(false);
                Top = 0;
                Left = Left_Before;
            }
        }
        private void Wwise_Player_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            Wwise_Event_Player_Window.Window_Show();
        }
        private void Gun_To_Gun_B_Click(object sender, RoutedEventArgs e)
        {
            Message_Feed_Out("この機能はV1.5.4で有効化されます。");
        }
        private void WoT_Create_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            WoT_Sound_Mod_Window.Window_Show();
        }
    }
}