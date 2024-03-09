using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using Un4seen.Bass.Misc;
using WK.Libraries.BetterFolderBrowserNS;

namespace WoTB_Voice_Mod_Creater.Class
{
    public class Extension_File
    {
        public string Full_Path = "";
        public string Name_Path => Path.GetFileName(Full_Path);
        public string Encode_Name_Path => Path.GetDirectoryName(Full_Path) + "\\" + Path.GetFileNameWithoutExtension(Full_Path);
        public float Volume = -1f;
        public float Freq = -1f;
        public float Default_Freq = 0f;
        public Extension_File(string Full_Path)
        {
            this.Full_Path = Full_Path;
        }
    }
    public class Extension_Directory
    {
        public List<Extension_File> Files = new List<Extension_File>();
        public string Dir_Path = "";
        public string Dir_Name => Dir_Path + " - ファイル数:" + Files.Count;
        public Extension_Directory(string Dir_Path)
        {
            this.Dir_Path = Dir_Path;
        }
        public bool Add_File(string Full_Path)
        {
            if (!File.Exists(Full_Path))
                return false;
            foreach (Extension_File Now_File in Files)
                if (Now_File.Full_Path == Full_Path)
                    return false;
            Files.Add(new Extension_File(Full_Path));
            Files = Files.OrderBy(x => x.Name_Path).ToList();
            return true;
        }
    }
    public enum Extension_Format
    {
        WAV,
        MP3,
        OGG
    }
    public partial class Extension_Converter : UserControl
    {
        List<Extension_Directory> Dirs = new List<Extension_Directory>();
        Dictionary<int, int> Dir_Index = new Dictionary<int, int>();
        Brush Gray_Text = (Brush)new BrushConverter().ConvertFromString("#BFFF2C8C");
        SYNCPROC IsMusicEnd;
        Extension_File Playing_File = null;
        Extension_Execute Extension_EXE = null;
        BASS_BFX_VOLUME Volume_FX = new BASS_BFX_VOLUME();
        string To_Dir_Path = "";
        int Stream = 0;
        int Stream_Volume = 0;
        bool IsMessageShowing = false;
        bool IsClosing = false;
        bool IsDeleteSourceFile = false;
        bool IsExtension_Volume = false;
        bool IsExtension_Speed = false;
        bool IsToFileSourceDir = false;
        bool IsSelectedOnly = false;
        bool IsWindowLoaded = false;
        bool IsPaused = false;
        bool IsPositionChanging = false;
        bool IsLControlKeyDown = false;
        bool IsLeftKeyDown = false;
        bool IsRightKeyDown = false;
        bool IsSpaceKeyDown = false;
        bool IsEnded = false;
        bool IsExtensionRunning = false;
        public Extension_Converter()
        {
            InitializeComponent();
            Extension_C.Items.Add(".wav");
            Extension_C.Items.Add(".mp3");
            Extension_C.Items.Add(".ogg");
            Extension_C.SelectedIndex = 0;
            Delete_Source_File_C.Source = Sub_Code.Check_01;
            Extension_Volume_C.Source = Sub_Code.Check_01;
            Extension_Speed_C.Source = Sub_Code.Check_01;
            To_File_Sorce_Dir_C.Source = Sub_Code.Check_01;
            Selected_Only_C.Source = Sub_Code.Check_01;
            Position_S.AddHandler(MouseDownEvent, new MouseButtonEventHandler(Position_S_MouseDown), true);
            Position_S.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Position_S_MouseUp), true);
            Volume_S.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Volume_S_MouseUp), true);
            Extension_Execute_B.IsEnabled = false;
        }
        async void Position_Change()
        {
            double nextFrame = (double)Environment.TickCount;
            float period = 1000f / 30f;
            while (Visibility == Visibility.Visible)
            {
                double tickCount = (double)Environment.TickCount;
                if (tickCount < nextFrame)
                {
                    if (nextFrame - tickCount > 1)
                        await Task.Delay((int)(nextFrame - tickCount));
                    System.Windows.Forms.Application.DoEvents();
                    continue;
                }
                if (Extension_EXE != null)
                {
                    if (!IsMessageShowing)
                        Message_T.Text = "サウンドファイルを変換しています...\n" + Extension_EXE.Now_Count + "/" + Extension_EXE.Max_File;
                    if (Extension_EXE.IsEnded)
                    {
                        Message_Feed_Out(Extension_EXE.Max_File + "個のサウンドファイルを変換しました。");
                        Extension_EXE = null;
                        IsExtensionRunning = false;
                    }
                }
                if (Sound_List.Items.Count > 0 && !IsSelectedOnly)
                    Extension_Execute_B.IsEnabled = true;
                else if (Sound_List.Items.Count == 0 || (Sound_List.SelectedIndex == -1 && IsSelectedOnly))
                    Extension_Execute_B.IsEnabled = false;
                bool IsPlaying = Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_PLAYING;
                if (IsPlaying)
                {
                    Bass.BASS_ChannelUpdate(Stream, 400);
                    if (!IsPositionChanging)
                    {
                        long position = Bass.BASS_ChannelGetPosition(Stream);
                        Position_S.Value = Bass.BASS_ChannelBytes2Seconds(Stream, position);
                        TimeSpan Time = TimeSpan.FromSeconds(Position_S.Value);
                        string Minutes = Time.Minutes.ToString();
                        string Seconds = Time.Seconds.ToString();
                        if (Time.Minutes < 10)
                            Minutes = "0" + Time.Minutes;
                        if (Time.Seconds < 10)
                            Seconds = "0" + Time.Seconds;
                        Position_T.Text = Minutes + ":" + Seconds;
                    }
                }
                if (Sub_Code.IsForcusWindow)
                {
                    if ((Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) > 0)
                    {
                        Minus_B.Content = "-10秒";
                        Plus_B.Content = "+10秒";
                        IsLControlKeyDown = true;
                    }
                    else
                    {
                        Minus_B.Content = "-5秒";
                        Plus_B.Content = "+5秒";
                        IsLControlKeyDown = false;
                    }
                    if ((Keyboard.GetKeyStates(Key.Left) & KeyStates.Down) > 0)
                    {
                        if (!IsLeftKeyDown && !IsRightKeyDown)
                            Minus_B.PerformClick();
                        IsLeftKeyDown = true;
                    }
                    else
                        IsLeftKeyDown = false;
                    if ((Keyboard.GetKeyStates(Key.Right) & KeyStates.Down) > 0)
                    {
                        if (!IsRightKeyDown && !IsLeftKeyDown)
                            Plus_B.PerformClick();
                        IsRightKeyDown = true;
                    }
                    else
                        IsRightKeyDown = false;
                    if ((Keyboard.GetKeyStates(Key.Space) & KeyStates.Down) > 0)
                    {
                        if (!IsSpaceKeyDown)
                        {
                            if (IsPlaying)
                                Pause_B.PerformClick();
                            else
                                Play_B.PerformClick();
                        }
                        IsSpaceKeyDown = true;
                    }
                    else
                        IsSpaceKeyDown = false;
                    if ((Keyboard.GetKeyStates(Key.V) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.Up) & KeyStates.Down) > 0)
                    {
                        double Increase = 1;
                        if (Volume_S.Value + Increase > 100)
                            Volume_S.Value = 100;
                        else
                            Volume_S.Value += Increase;
                    }
                    else if ((Keyboard.GetKeyStates(Key.V) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.Down) & KeyStates.Down) > 0)
                    {
                        double Increase = 1;
                        if (Volume_S.Value - Increase < 0)
                            Volume_S.Value = 0;
                        else
                            Volume_S.Value -= Increase;
                    }
                    if ((Keyboard.GetKeyStates(Key.S) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.Up) & KeyStates.Down) > 0)
                    {
                        if (Speed_S.Value + 0.6 > 100)
                            Speed_S.Value = 100;
                        else
                            Speed_S.Value += 0.6;
                    }
                    else if ((Keyboard.GetKeyStates(Key.S) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.Down) & KeyStates.Down) > 0)
                    {
                        if (Speed_S.Value - 0.6 < 0)
                            Speed_S.Value = 0;
                        else
                            Speed_S.Value -= 0.6;
                    }
                    else if ((Keyboard.GetKeyStates(Key.S) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.R) & KeyStates.Down) > 0)
                        Speed_S.Value = 50;
                }
                if (IsEnded)
                {
                    Bass.BASS_ChannelStop(Stream);
                    Bass.BASS_ChannelPlay(Stream, true);
                }
                IsEnded = false;
                if ((double)System.Environment.TickCount >= nextFrame + (double)period)
                {
                    nextFrame += period;
                    continue;
                }
                nextFrame += period;
            }
        }
        async void EndSync(int handle, int channel, int data, IntPtr user)
        {
            if (!IsEnded)
            {
                await Task.Delay(500);
                IsEnded = true;
            }
        }
        async void Message_Feed_Out(string Message)
        {
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
            if (IsMessageShowing)
            {
                IsMessageShowing = false;
                Message_T.Text = "";
            }
            Message_T.Opacity = 1;
        }
        public async void Window_Show()
        {
            //画面を表示
            Opacity = 0;
            Visibility = Visibility.Visible;
            if (!IsWindowLoaded)
            {
                IsWindowLoaded = true;
                if (File.Exists(Voice_Set.Special_Path + "\\Configs\\Extension_Converter.dat"))
                {
                    BinaryReader bin = new BinaryReader(File.OpenRead(Voice_Set.Special_Path + "\\Configs\\Extension_Converter.dat"));
                    try
                    {
                        To_Dir_Path = Encoding.UTF8.GetString(bin.ReadBytes(bin.ReadUInt16()));
                        //Volume_S.Value = bin.ReadDouble();
                    }
                    catch { }
                    bin.Close();
                }
                else
                    To_Dir_Path = Voice_Set.Local_Path + "\\Extension_Converter";
                To_Dir_T.Text = To_Dir_Path;
            }
            Position_Change();
            while (Opacity < 1)
            {
                Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
        }
        //ウィンドウを閉じる
        private async void Back_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            else if (IsExtensionRunning)
            {
                Message_Feed_Out("変換中は画面移動ができません。");
                return;
            }
            IsClosing = true;
            Pause_Volume_Animation(false, 20f);
            while (Opacity > 0)
            {
                Opacity -= Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
            IsClosing = false;
            Visibility = Visibility.Hidden;
        }
        private void Delete_Source_File_C_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (IsDeleteSourceFile)
            {
                IsDeleteSourceFile = false;
                Delete_Source_File_C.Source = Sub_Code.Check_02;
            }
            else
            {
                IsDeleteSourceFile = true;
                Delete_Source_File_C.Source = Sub_Code.Check_04;
            }
        }
        private void Delete_Source_File_C_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsDeleteSourceFile)
                Delete_Source_File_C.Source = Sub_Code.Check_04;
            else
                Delete_Source_File_C.Source = Sub_Code.Check_02;
        }
        private void Delete_Source_File_C_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsDeleteSourceFile)
                Delete_Source_File_C.Source = Sub_Code.Check_03;
            else
                Delete_Source_File_C.Source = Sub_Code.Check_01;
        }
        private void Extension_Volume_C_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (IsExtension_Volume)
            {
                IsExtension_Volume = false;
                Extension_Volume_C.Source = Sub_Code.Check_02;
            }
            else
            {
                IsExtension_Volume = true;
                Extension_Volume_C.Source = Sub_Code.Check_04;
            }
        }
        private void Extension_Volume_C_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsExtension_Volume)
                Extension_Volume_C.Source = Sub_Code.Check_04;
            else
                Extension_Volume_C.Source = Sub_Code.Check_02;
        }
        private void Extension_Volume_C_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsExtension_Volume)
                Extension_Volume_C.Source = Sub_Code.Check_03;
            else
                Extension_Volume_C.Source = Sub_Code.Check_01;
        }
        private void Extension_Speed_C_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (IsExtension_Speed)
            {
                IsExtension_Speed = false;
                Extension_Speed_C.Source = Sub_Code.Check_02;
            }
            else
            {
                IsExtension_Speed = true;
                Extension_Speed_C.Source = Sub_Code.Check_04;
            }
        }
        private void Extension_Speed_C_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsExtension_Speed)
                Extension_Speed_C.Source = Sub_Code.Check_04;
            else
                Extension_Speed_C.Source = Sub_Code.Check_02;
        }
        private void Extension_Speed_C_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsExtension_Speed)
                Extension_Speed_C.Source = Sub_Code.Check_03;
            else
                Extension_Speed_C.Source = Sub_Code.Check_01;
        }
        private void To_File_Sorce_Dir_C_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (IsToFileSourceDir)
            {
                IsToFileSourceDir = false;
                To_File_Sorce_Dir_C.Source = Sub_Code.Check_02;
                To_Dir_T.Foreground = Brushes.Aqua;
                To_Dir_B.IsEnabled = true;
            }
            else
            {
                IsToFileSourceDir = true;
                To_File_Sorce_Dir_C.Source = Sub_Code.Check_04;
                To_Dir_T.Foreground = Brushes.Gray;
                To_Dir_B.IsEnabled = false;
            }
        }
        private void To_File_Sorce_Dir_C_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsToFileSourceDir)
                To_File_Sorce_Dir_C.Source = Sub_Code.Check_04;
            else
                To_File_Sorce_Dir_C.Source = Sub_Code.Check_02;
        }
        private void To_File_Sorce_Dir_C_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsToFileSourceDir)
                To_File_Sorce_Dir_C.Source = Sub_Code.Check_03;
            else
                To_File_Sorce_Dir_C.Source = Sub_Code.Check_01;
        }
        private void Selected_Only_C_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (IsSelectedOnly)
            {
                IsSelectedOnly = false;
                Selected_Only_C.Source = Sub_Code.Check_02;
                if (Sound_List.Items.Count == 0)
                    Extension_Execute_B.IsEnabled = false;
                else
                    Extension_Execute_B.IsEnabled = true;
            }
            else
            {
                IsSelectedOnly = true;
                Selected_Only_C.Source = Sub_Code.Check_04;
                if (Sound_List.SelectedIndex == -1)
                    Extension_Execute_B.IsEnabled = false;
                else
                    Extension_Execute_B.IsEnabled = true;

            }
        }
        private void Selected_Only_C_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsSelectedOnly)
                Selected_Only_C.Source = Sub_Code.Check_04;
            else
                Selected_Only_C.Source = Sub_Code.Check_02;
        }
        private void Selected_Only_C_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsSelectedOnly)
                Selected_Only_C.Source = Sub_Code.Check_03;
            else
                Selected_Only_C.Source = Sub_Code.Check_01;
        }
        private void Volume_S_MouseUp(object sender, MouseEventArgs e)
        {
            Save_Configs();
        }
        public void Update_Sound_List()
        {
            Extension_File Selected_File = Get_File_From_Index(Sound_List.SelectedIndex);
            Dir_Index.Clear();
            Sound_List.Items.Clear();
            for (int i = 0; i < Dirs.Count; i++)
            {
                Dir_Index.Add(Sound_List.Items.Count, i);
                ListBoxItem Dir_Item = new ListBoxItem();
                Dir_Item.Content = Dirs[i].Dir_Name;
                Dir_Item.Foreground = Gray_Text;
                Dir_Item.FontSize = 30;
                Sound_List.Items.Add(Dir_Item);
                foreach (Extension_File ExFile in Dirs[i].Files)
                {
                    ListBoxItem File_Item = new ListBoxItem();
                    File_Item.Content = " ---> " + ExFile.Name_Path;
                    File_Item.Foreground = Brushes.Aqua;
                    Sound_List.Items.Add(File_Item);
                }
            }
            Sound_List.SelectedIndex = Get_Index_From_Extension_File(Selected_File);
            if (Sound_List.SelectedIndex == -1 && Playing_File != null)
                Sound_List.SelectedIndex = Get_Index_From_Extension_File(Playing_File);
            if (Dir_Index.ContainsKey(Sound_List.SelectedIndex) || Sound_List.SelectedIndex == -1)
                Pause_Volume_Animation(true, 15f);
            Add_File_Count_T.Text = "追加済みのファイル:" + Get_File_Count();
        }
        private void Add_File_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog()
            {
                Title = "サウンドファイルを選択してください。",
                Multiselect = true,
                Filter = "サウンドファイル(*.mp3;*.wav;*.ogg;*.flac;*.wma;*.aac;*.mp4)|*.mp3;*.wav;*.ogg;*.flac;*.wma;*.aac;*.mp4"
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Add_Files(ofd.FileNames);
                Update_Sound_List();
            }
            ofd.Dispose();
        }
        private void Add_Dir_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            BetterFolderBrowser fbd = new BetterFolderBrowser()
            {
                Title = "追加するファイルが存在するフォルダを選択してください。",
                Multiselect = true,
                RootFolder = Sub_Code.Get_OpenDirectory_Path()
            };
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Sub_Code.Set_Directory_Path(fbd.SelectedFolder);
                List<string> files = new List<string>();
                foreach (string Folder in fbd.SelectedFolders)
                {
                    List<string> Add_Files = new List<string>();
                    string[] Filters = { "*.mp3", "*.wav", "*.ogg", "*.flac", "*.wma", "*.aac", "*.m4a", "*.mp4" };
                    foreach (string Extension in Filters)
                        foreach (string File_Now in Directory.GetFiles(Folder, Extension, SearchOption.TopDirectoryOnly))
                            files.Add(File_Now);
                }
                Add_Files(files.ToArray());
                files.Clear();
                Update_Sound_List();
            }
            fbd.Dispose();
        }
        public void Add_Files(string[] Files)
        {
            int Add_Count = 0;
            foreach (string File_Now in Files)
            {
                string Dir = Path.GetDirectoryName(File_Now);
                Extension_Directory Add_Dir = null;
                foreach (Extension_Directory edir in Dirs)
                {
                    if (edir.Dir_Path == Dir)
                    {
                        Add_Dir = edir;
                        break;
                    }
                }
                if (Add_Dir == null)
                {
                    Add_Dir = new Extension_Directory(Dir);
                    Dirs.Add(Add_Dir);
                }
                if (Add_Dir.Add_File(File_Now))
                    Add_Count++;
            }
            if (Add_Count == 0)
                Message_Feed_Out("ファイルを追加できませんでした。既に追加されているか、ファイルが存在しません。");
            else if (Add_Count == Files.Length)
                Message_Feed_Out(Add_Count + "個のサウンドファイルを追加しました。");
            else
                Message_Feed_Out(Add_Count + "個のサウンドファイルを追加しました。追加済みの同名のファイルはスキップされます。");
        }
        private void Delete_File_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            List<int> Indexes = Sound_List.SelectedIndexes();
            if (Indexes.Count == 0 || IsClosing)
                return;
            if (Indexes.Count == 1 && Dir_Index.ContainsKey(Sound_List.SelectedIndex))
            {
                Extension_Directory d = Dirs[Dir_Index[Sound_List.SelectedIndex]];
                MessageBoxResult result2 = MessageBox.Show("フォルダ:'" + d.Dir_Path + "'内のファイル(" + d.Files.Count + "個)をリストから削除しますか?", "確認",
                    MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
                if (result2 == MessageBoxResult.Yes)
                {
                    Dirs.RemoveAt(Dir_Index[Sound_List.SelectedIndex]);
                    Message_Feed_Out("フォルダをリストから削除しました。");
                    Update_Sound_List();
                }
                return;
            }
            int Delete_Index_One = -1;
            if (Indexes.Count == 1)
                Delete_Index_One = Sound_List.SelectedIndex;
            int Delete_Count = 0;
            foreach (int index in Indexes)
            {
                if (Dir_Index.ContainsKey(index))
                    continue;
                Delete_Count++;
            }
            MessageBoxResult result = MessageBox.Show(Delete_Count + "個のファイルをリストから削除しますか?", "確認",
                    MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (result == MessageBoxResult.No)
                return;
            int Delete_File_Count = 0;
            List<int> Indexes_Order = Indexes.OrderBy(a => -a).ToList();
            foreach (int RemoveIndex in Dir_Index.Keys)
            {
                if (Indexes_Order.Contains(RemoveIndex))
                    Indexes_Order.Remove(RemoveIndex);
            }
            foreach (int index in Indexes_Order)
            {
                if (Dir_Index.ContainsKey(index))
                    continue;
                if (Get_File_From_Index(index) == Playing_File)
                {
                    Pause_Volume_Animation(true, 10f);
                }
                if (Delete_File_From_Index(index))
                    Delete_File_Count++;
            }
            if (Delete_File_Count == 0)
                Message_Feed_Out("エラー:ファイルをリストから削除できませんでした。");
            else
            {
                Message_Feed_Out(Delete_File_Count + "個のファイルをリストから削除しました。");
                Update_Sound_List();
            }
            if (Sound_List.Items.Count > Delete_Index_One)
                Sound_List.SelectedIndex = Delete_Index_One;
        }
        private int Get_Max_Index()
        {
            int Count = 0;
            foreach (Extension_Directory dir in Dirs)
            {
                Count++;
                Count += dir.Files.Count;
            }
            return Count;
        }
        private int Get_File_Count()
        {
            int Count = 0;
            foreach (Extension_Directory dir in Dirs)
                Count += dir.Files.Count;
            return Count;
        }
        private Extension_File Get_File_From_Index(int Index)
        {
            int File_Index = -1;
            for (int i = Index; i >= 0; i--)
            {
                if (Dir_Index.ContainsKey(i))
                {
                    if (File_Index == -1 || Dirs[Dir_Index[i]].Files.Count <= File_Index)
                        return null;
                    return Dirs[Dir_Index[i]].Files[File_Index];
                }
                File_Index++;
            }
            return null;
        }
        private int Get_Index_From_Extension_File(Extension_File ExFile)
        {
            if (ExFile == null)
                return -1;
            int Index = -1;
            foreach (int dir in Dir_Index.Keys)
            {
                Index++;
                bool IsExist = false;
                foreach (Extension_File now in Dirs[Dir_Index[dir]].Files)
                {
                    Index++;
                    if (now == ExFile)
                    {
                        IsExist = true;
                        break;
                    }
                }
                if (IsExist)
                    break;
            }
            return Index;
        }
        private bool Delete_File_From_Index(int Index)
        {
            if (Dir_Index.ContainsKey(Index))
                return false;
            int File_Index = -1;
            for (int i = Index; i >= 0; i--)
            {
                if (Dir_Index.ContainsKey(i))
                {
                    Dirs[Dir_Index[i]].Files.RemoveAt(File_Index);
                    if (Dirs[Dir_Index[i]].Files.Count == 0)
                    {
                        Dirs.RemoveAt(Dir_Index[i]);
                        Dir_Index.Remove(i);
                    }
                    return true;
                }
                File_Index++;
            }
            return false;
        }
        private void To_Dir_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || IsToFileSourceDir)
                return;
            BetterFolderBrowser fbd = new BetterFolderBrowser()
            {
                Title = "変換先のフォルダを選択してください。",
                RootFolder = Sub_Code.Get_OpenDirectory_Path(),
                Multiselect = false
            };
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Sub_Code.Set_Directory_Path(fbd.SelectedFolder);
                if (Sub_Code.CanDirectoryAccess(fbd.SelectedFolder))
                {
                    To_Dir_Path = fbd.SelectedFolder;
                    To_Dir_T.Text = To_Dir_Path;
                    Save_Configs();
                }
                else
                    Message_Feed_Out("指定したフォルダはアクセスが制限されています。アクセス可能なフォルダを選択してください。");
            }
            fbd.Dispose();
        }
        private void Save_Configs()
        {
            if (!IsLoaded)
                return;
            if (File.Exists(Voice_Set.Special_Path + "\\Configs\\Extension_Converter.dat"))
                File.Delete(Voice_Set.Special_Path + "\\Configs\\Extension_Converter.dat");
            BinaryWriter bin = new BinaryWriter(File.OpenWrite(Voice_Set.Special_Path + "\\Configs\\Extension_Converter.dat"));
            byte[] To_Dir_Bytes = Encoding.UTF8.GetBytes(To_Dir_Path);
            bin.Write((ushort)To_Dir_Bytes.Length);
            bin.Write(To_Dir_Bytes);
            bin.Write(Volume_S.Value);
            bin.Write((byte)Extension_C.SelectedIndex);
            bin.Close();
        }
        private void Sound_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsClosing || Sound_List.SelectedIndexes().Count > 1 || Playing_File == Get_File_From_Index(Sound_List.SelectedIndex) || Dir_Index.ContainsKey(Sound_List.SelectedIndex)
                || Sound_List.SelectedIndex == -1)
                return;
            //Pause_Volume_Animation(true, 10f);
        }
        private void Volume_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Volume_T.Text = "音量:" + (int)Volume_S.Value;
            if (!IsPaused)
            {
                Volume_FX.fVolume = (float)Volume_S.Value / 100f;
                Bass.BASS_FXSetParameters(Stream_Volume, Volume_FX);
            }
            foreach (int index in Sound_List.SelectedIndexes())
            {
                Extension_File file = Get_File_From_Index(index);
                file.Volume = (float)Volume_S.Value / 100f;
            }
        }
        void Music_Pos_Change(double Pos, bool IsBassPosChange)
        {
            if (IsClosing)
                return;
            if (IsBassPosChange)
                Bass.BASS_ChannelSetPosition(Stream, Pos);
            TimeSpan Time = TimeSpan.FromSeconds(Pos);
            string Minutes = Time.Minutes.ToString();
            string Seconds = Time.Seconds.ToString();
            if (Time.Minutes < 10)
                Minutes = "0" + Time.Minutes;
            if (Time.Seconds < 10)
                Seconds = "0" + Time.Seconds;
            Position_T.Text = Minutes + ":" + Seconds;
        }
        //フェードインしながら再生
        //引数:フェードインのかかる時間
        async void Play_Volume_Animation(float Feed_Time = 15f)
        {
            IsPaused = false;
            Bass.BASS_ChannelPlay(Stream, false);
            float Volume_Now = Volume_FX.fVolume;
            float Volume_Plus = (float)(Volume_S.Value / 100) / Feed_Time;
            while (Volume_Now < (float)(Volume_S.Value / 100) && !IsPaused)
            {
                Volume_Now += Volume_Plus;
                if (Volume_Now > 2f)
                    Volume_Now = 2f;
                Volume_FX.fVolume = Volume_Now;
                Bass.BASS_FXSetParameters(Stream_Volume, Volume_FX);
                await Task.Delay(1000 / 60);
            }
        }
        //フェードアウトしながら一時停止または停止
        public async void Pause_Volume_Animation(bool IsStop, float Feed_Time = 15f)
        {
            IsPaused = true;
            float Volume_Now = Volume_FX.fVolume;
            float Volume_Minus = Volume_Now / Feed_Time;
            while (Volume_Now > 0f && IsPaused)
            {
                Volume_Now -= Volume_Minus;
                if (Volume_Now < 0f)
                    Volume_Now = 0f;
                Volume_FX.fVolume = Volume_Now;
                Bass.BASS_FXSetParameters(Stream_Volume, Volume_FX);
                await Task.Delay(1000 / 60);
            }
            if (Volume_Now <= 0f)
            {
                if (IsStop)
                {
                    Bass.BASS_ChannelStop(Stream);
                    Bass.BASS_StreamFree(Stream_Volume);
                    Bass.BASS_StreamFree(Stream);
                    Position_S.Value = 0;
                    Position_S.Maximum = 0;
                    Position_T.Text = "00:00";
                    Stream = 0;
                }
                else
                    Bass.BASS_ChannelPause(Stream);
            }
        }
        private void Pause_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
                return;
            Pause_Volume_Animation(false, 10f);
        }
        private void Play_B_Click(object sender, RoutedEventArgs e)
        {
            if (Sound_List.SelectedIndex == -1 || IsClosing)
                return;
            Extension_File file = Get_File_From_Index(Sound_List.SelectedIndex);
            if (file == null)
                return;
            if (Playing_File != file)
            {
                Bass.BASS_ChannelStop(Stream);
                Bass.BASS_StreamFree(Stream_Volume);
                Bass.BASS_StreamFree(Stream);
                Position_S.Value = 0;
                Position_T.Text = "00:00";
                if (!File.Exists(file.Full_Path))
                {
                    Message_Feed_Out("ファイルが存在しませんでした。");
                    return;
                }
                Playing_File = file;
                Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, 100);
                int StreamHandle = Bass.BASS_StreamCreateFile(file.Full_Path, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_LOOP);
                Stream = BassFx.BASS_FX_TempoCreate(StreamHandle, BASSFlag.BASS_FX_FREESOURCE);
                Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, 500);
                IsMusicEnd = new SYNCPROC(EndSync);
                if (file.Default_Freq != 0f)
                {
                    Speed_S.Value = (int)(file.Freq * 50f);
                }
                else
                {
                    Speed_S.Value = 50;
                    Bass.BASS_ChannelGetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, ref file.Default_Freq);
                }
                if (file.Volume != -1f)
                {
                    Volume_S.Value = (int)(file.Volume * 100f);
                }
                else
                {
                    Volume_S.Value = 100;
                }
                Bass.BASS_ChannelSetSync(Stream, BASSSync.BASS_SYNC_END | BASSSync.BASS_SYNC_MIXTIME, 0, IsMusicEnd, IntPtr.Zero);
                Bass.BASS_ChannelSetDevice(Stream, Video_Mode.Sound_Device);
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, file.Default_Freq * file.Freq);
                Stream_Volume = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_BFX_VOLUME, 1);
                Volume_FX.fVolume = file.Volume;
                Bass.BASS_FXSetParameters(Stream_Volume, Volume_FX);
                Position_S.Maximum = Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetLength(Stream, BASSMode.BASS_POS_BYTES));
                Play_Volume_Animation(5f);
            }
            else
            {
                Bass.BASS_ChannelSetDevice(Stream, Video_Mode.Sound_Device);
                Play_Volume_Animation(10f);
            }
        }
        private void Minus_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || Stream == 0)
                return;
            long position = Bass.BASS_ChannelGetPosition(Stream);
            double Time_Temp = Bass.BASS_ChannelBytes2Seconds(Stream, position);
            int Skip_Time = IsLControlKeyDown ? 10 : 5;
            if (Bass.BASS_ChannelBytes2Seconds(Stream, position) > Skip_Time)
                Music_Pos_Change(Bass.BASS_ChannelBytes2Seconds(Stream, position) - Skip_Time, true);
            else
                Music_Pos_Change(0, true);
            long position2 = Bass.BASS_ChannelGetPosition(Stream);
            Position_S.Value = Bass.BASS_ChannelBytes2Seconds(Stream, position2);
        }
        private void Plus_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || Stream == 0)
                return;
            long position = Bass.BASS_ChannelGetPosition(Stream);
            int Skip_Time = IsLControlKeyDown ? 10 : 5;
            if (Bass.BASS_ChannelBytes2Seconds(Stream, position) + Skip_Time > Position_S.Maximum)
                Music_Pos_Change(Position_S.Maximum, true);
            else
                Music_Pos_Change(Bass.BASS_ChannelBytes2Seconds(Stream, position) + Skip_Time, true);
            long position2 = Bass.BASS_ChannelGetPosition(Stream);
            Position_S.Value = Bass.BASS_ChannelBytes2Seconds(Stream, position2);
        }
        private void Speed_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Speed_T.Text = "速度:" + (int)Speed_S.Value;
            if (Sound_List.SelectedIndex == -1)
            {
                return;
            }
            float freq = (float)(Speed_S.Value / 50);
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, Get_File_From_Index(Sound_List.SelectedIndex).Default_Freq * freq);
            foreach (int index in Sound_List.SelectedIndexes())
            {
                Extension_File file = Get_File_From_Index(index);
                file.Freq = freq;
            }
        }
        private void Position_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsPositionChanging)
                Music_Pos_Change(Position_S.Value, false);
        }
        //再生位置のスライダーを押したら
        void Position_S_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsClosing)
                return;
            IsPositionChanging = true;
            Bass.BASS_ChannelPause(Stream);
            Volume_FX.fVolume = 0f;
            Bass.BASS_FXSetParameters(Stream_Volume, Volume_FX);
        }
        //再生位置のスライダーを離したら
        void Position_S_MouseUp(object sender, MouseButtonEventArgs e)
        {
            IsPositionChanging = false;
            Bass.BASS_ChannelSetPosition(Stream, Position_S.Value);
            if (!IsPaused)
            {
                Bass.BASS_ChannelPlay(Stream, false);
                Play_Volume_Animation();
            }
        }
        private void Speed_S_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsClosing)
                return;
            Speed_S.Value = 50;
        }
        private void Sound_List_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Left || e.Key == Key.Right)
                e.Handled = true;
        }
        private async void Extension_Execute_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || Sound_List.Items.Count == 0 || (IsSelectedOnly && Sound_List.SelectedIndex == -1) || IsExtensionRunning)
                return;
            List<Extension_File> Extension_Files = new List<Extension_File>();
            if (IsSelectedOnly)
            {
                foreach (int Selected_Index in Sound_List.SelectedIndexes())
                {
                    Extension_File Now_File = Get_File_From_Index(Selected_Index);
                    if (Now_File != null)
                        Extension_Files.Add(Now_File);
                }
            }
            else
            {
                foreach (Extension_Directory Dir in Dirs)
                    foreach (Extension_File Files in Dir.Files)
                        Extension_Files.Add(Files);
            }
            if (Extension_Files.Count == 0)
            {
                Message_Feed_Out("変換できるファイルが存在しません。");
                return;
            }
            if (IsExtension_Speed && Speed_S.Value < 25)
            {
                Message_Feed_Out("速度が25未満の状態では変換することができません。\nファイルサイズが大きくなっちゃう...><");
                return;
            }
            MessageBoxResult result = MessageBox.Show(Extension_Files.Count + "個のサウンドファイルを変換しますか?", "確認",
                    MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (result == MessageBoxResult.No)
            {
                Extension_Files.Clear();
                return;
            }
            IsMessageShowing = true;
            Message_T.Opacity = 1;
            Message_T.Text = "準備しています...";
            await Task.Delay(50);
            Extension_Execute.pfcounter.NextValue();
            try
            {
                if (!IsToFileSourceDir && !Directory.Exists(To_Dir_Path))
                    Directory.CreateDirectory(To_Dir_Path);
                if (!IsToFileSourceDir && !Sub_Code.CanDirectoryAccess(To_Dir_Path))
                {
                    Message_Feed_Out("保存先のフォルダにアクセスできませんでした。");
                    return;
                }
                Extension_Format f = Extension_Format.WAV;
                if (Extension_C.SelectedIndex == 1)
                    f = Extension_Format.MP3;
                else if (Extension_C.SelectedIndex == 2)
                    f = Extension_Format.OGG;
                IsMessageShowing = false;
                if (IsToFileSourceDir)
                    Extension_EXE = new Extension_Execute(Extension_Files, f, IsDeleteSourceFile, "", IsExtension_Volume, IsExtension_Speed);
                else
                    Extension_EXE = new Extension_Execute(Extension_Files, f, IsDeleteSourceFile, To_Dir_Path, IsExtension_Volume, IsExtension_Speed);
                Extension_EXE.Execute();
                IsExtensionRunning = true;
            }
            catch (Exception e1)
            {
                Message_Feed_Out("エラーが発生しました。\n" + e1.Message);
                Sub_Code.Error_Log_Write(e1.Message);
            }
        }
        private void Extension_C_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Save_Configs();
        }
        /*private void CPU_Usage_B_Click(object sender, RoutedEventArgs e)
        {
            string Message_01 = "許容するCPU使用率を指定します。\n";
            string Message_02 = "---変換中、CPU使用率は指定した数値のあたりに留まります。\n";
            string Message_03 = "---指定する数値が高いほど変換速度が向上します。\n";
            string Message_04 = "---OSが制御してくれるため、数値を100%に設定してもPCに害はほとんどありません。";
            MessageBox.Show(Message_01 + Message_02 + Message_03 + Message_04);
        }
        private void CPU_Usage_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int Allow_CPU_Usage = (int)CPU_Usage_S.Value;
            CPU_Usage_T.Text = "最大CPU使用率:" + Allow_CPU_Usage + "%";
            if (Allow_CPU_Usage < 75)
                CPU_Usage_T.Foreground = Brushes.PaleGreen;
            else if (Allow_CPU_Usage < 90)
                CPU_Usage_T.Foreground = Brushes.Yellow;
            else
                CPU_Usage_T.Foreground = Brushes.OrangeRed;
        }*/
    }
    public class Extension_Execute
    {
        public static PerformanceCounter pfcounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        public List<Extension_File> Extension_Files { get; set; }
        public Extension_Format Format { get; private set; }
        private string To_Dir = "";
        public double Use_CPU_Usage = 100;
        public int Max_File => Extension_Files.Count;
        public int Now_Count { get; private set; }
        private bool IsDeleteFromFile = false;
        private bool IsExtensionVolume = false;
        private bool IsExtensionSpeed = false;
        public bool IsEnded { get; private set; }
        public Extension_Execute(List<Extension_File> Extension_Files, Extension_Format Format, bool IsDeleteFromFile, string To_Dir = "", bool IsExtensionVolume = false, bool IsExtensionSpeed = false)
        {
            this.Extension_Files = Extension_Files;
            Now_Count = 0;
            IsEnded = false;
            this.Format = Format;
            this.IsDeleteFromFile = IsDeleteFromFile;
            this.To_Dir = To_Dir;
            this.IsExtensionVolume = IsExtensionVolume;
            this.IsExtensionSpeed = IsExtensionSpeed;
        }
        public void Execute()
        {
            Task task = Task.Run(async () =>
            {
                List<Task> task1 = new List<Task>();
                foreach (Extension_File file in Extension_Files)
                    task1.Add(Encode_Bass(file));
                await Task.WhenAll(task1);
                IsEnded = true;
                task1.Clear();
            });
        }
        private async Task<bool> Encode_Bass(Extension_File file)
        {
            try
            {
                bool IsSuccess = false;
                await Task.Run(() =>
                {
                    string To_Dir_Path = To_Dir == "" ? file.Encode_Name_Path : To_Dir + "\\" + Path.GetFileNameWithoutExtension(file.Full_Path);
                    int streamhandle = 0;
                    int stream;
                    if (!IsExtensionSpeed && IsExtensionVolume)
                        stream = Bass.BASS_StreamCreateFile(file.Full_Path, 0, 0, BASSFlag.BASS_STREAM_DECODE);
                    else
                    {
                        streamhandle = Bass.BASS_StreamCreateFile(file.Full_Path, 0, 0, BASSFlag.BASS_STREAM_DECODE);
                        stream = BassFx.BASS_FX_TempoCreate(streamhandle, BASSFlag.BASS_FX_FREESOURCE | BASSFlag.BASS_STREAM_DECODE);
                        if (IsExtensionSpeed)
                            Bass.BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, file.Default_Freq * file.Freq);
                        if (!IsExtensionVolume)
                        {
                            BASS_BFX_VOLUME Volume_FX = new BASS_BFX_VOLUME(file.Volume);
                            int Stream_Volume = Bass.BASS_ChannelSetFX(stream, BASSFXType.BASS_FX_BFX_VOLUME, 1);
                            Bass.BASS_FXSetParameters(Stream_Volume, Volume_FX);
                        }
                    }
                    if (Format == Extension_Format.MP3)
                    {
                        if (To_Dir_Path + ".mp3" == file.Full_Path)
                        {
                            Bass.BASS_StreamFree(stream);
                            if (streamhandle != 0)
                                Bass.BASS_StreamFree(streamhandle);
                            return;
                        }
                        EncoderLAME l = new EncoderLAME(stream);
                        l.EncoderDirectory = Voice_Set.Special_Path + "\\Encode_Mp3";
                        l.InputFile = null;
                        if (File.Exists(To_Dir_Path + ".mp3"))
                            l.OutputFile = To_Dir_Path + "_" + Sub_Code.r.Next(10000, 100000) + ".mp3";
                        else
                            l.OutputFile = To_Dir_Path + ".mp3";
                        l.LAME_Bitrate = (int)EncoderLAME.BITRATE.kbps_144;
                        l.LAME_Mode = EncoderLAME.LAMEMode.Default;
                        l.LAME_Quality = EncoderLAME.LAMEQuality.Quality;
                        IsSuccess = l.Start(null, IntPtr.Zero, false);
                        byte[] encBuffer = new byte[65536];
                        while (Bass.BASS_ChannelIsActive(stream) == BASSActive.BASS_ACTIVE_PLAYING)
                        {
                            int len = Bass.BASS_ChannelGetData(stream, encBuffer, encBuffer.Length);
                        }
                        l.Stop();
                    }
                    else if (Format == Extension_Format.WAV)
                    {
                        if (To_Dir_Path + ".wav" == file.Full_Path)
                        {
                            Bass.BASS_StreamFree(stream);
                            if (streamhandle != 0)
                                Bass.BASS_StreamFree(streamhandle);
                            return;
                        }
                        Un4seen.Bass.Misc.EncoderWAV w = new Un4seen.Bass.Misc.EncoderWAV(stream);
                        w.InputFile = null;
                        if (File.Exists(To_Dir_Path + ".wav"))
                            w.OutputFile = To_Dir_Path + "_" + Sub_Code.r.Next(10000, 100000) + ".wav";
                        else
                            w.OutputFile = To_Dir_Path + ".wav";
                        w.WAV_BitsPerSample = 24;
                        IsSuccess = w.Start(null, IntPtr.Zero, false);
                        byte[] encBuffer = new byte[65536];
                        while (Bass.BASS_ChannelIsActive(stream) == BASSActive.BASS_ACTIVE_PLAYING)
                        {
                            int len = Bass.BASS_ChannelGetData(stream, encBuffer, encBuffer.Length);
                        }
                        w.Stop();
                    }
                    else if (Format == Extension_Format.OGG)
                    {
                        if (To_Dir_Path + ".ogg" == file.Full_Path)
                        {
                            Bass.BASS_StreamFree(stream);
                            if (streamhandle != 0)
                                Bass.BASS_StreamFree(streamhandle);
                            return;
                        }
                        EncoderOGG l = new EncoderOGG(stream);
                        l.EncoderDirectory = Voice_Set.Special_Path + "\\Encode_Mp3";
                        l.InputFile = null;
                        if (File.Exists(To_Dir_Path + ".ogg"))
                            l.OutputFile = To_Dir_Path + "_" + Sub_Code.r.Next(10000, 100000) + ".ogg";
                        else
                            l.OutputFile = To_Dir_Path + ".ogg";
                        l.OGG_UseQualityMode = true;
                        l.OGG_Quality = 8;
                        IsSuccess = l.Start(null, IntPtr.Zero, false);
                        byte[] encBuffer = new byte[65536];
                        Bass.BASS_ChannelPlay(stream, true);
                        while (Bass.BASS_ChannelIsActive(stream) == BASSActive.BASS_ACTIVE_PLAYING)
                        {
                            int len = Bass.BASS_ChannelGetData(stream, encBuffer, encBuffer.Length);
                        }
                        l.Stop();
                    }
                    Bass.BASS_StreamFree(stream);
                    if (streamhandle != 0)
                        Bass.BASS_StreamFree(streamhandle);
                });
                Now_Count++;
                if (IsSuccess && IsDeleteFromFile)
                    File.Delete(file.Full_Path);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}