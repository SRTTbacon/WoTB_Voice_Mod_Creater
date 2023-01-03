

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using WK.Libraries.BetterFolderBrowserNS;

namespace WoTB_Voice_Mod_Creater.Class
{
    public class WoWS_Voice_Class
    {
        public string File_Path { get; set; }
        public bool IsEnable = true;
        public WoWS_Voice_Class(string File_Path)
        {
            this.File_Path = File_Path;
        }
    }
    public class WoWS_Event_Class
    {
        public List<WoWS_Voice_Class> Voices = new List<WoWS_Voice_Class>();
        public string From_Event_Name { get; private set; }
        public string To_Event_Name { get; private set; }
        public byte Event_Index { get; private set; } 
        public byte[] Events_Index { get; private set; } 
        public bool IsEnable = true;
        public WoWS_Event_Class(string From_Event_Name, string To_Event_Name, byte Event_Index)
        {
            this.From_Event_Name = From_Event_Name;
            this.To_Event_Name = To_Event_Name;
            this.Event_Index = Event_Index;
            this.Events_Index = null;
        }
        public WoWS_Event_Class(string From_Event_Name, string To_Event_Name, byte[] Events_Index)
        {
            this.From_Event_Name = From_Event_Name;
            this.To_Event_Name = To_Event_Name;
            this.Events_Index = Events_Index;
            Event_Index = 255;
        }
    }
    public partial class WoWS_To_WoTB : UserControl
    {
        List<WoWS_Event_Class> Main_Event_List = new List<WoWS_Event_Class>();
        Brush br = null;
        string Mod_Folder = "";
        string Mod_Name = "";
        int Stream;
        bool IsBusy = false;
        bool IsMessageShowing = false;
        bool IsPaused = false;
        bool IsLocationChanging = false;
        bool IsPlayingMouseDown = false;
        bool IsNoEncodeMode = false;
        public WoWS_To_WoTB()
        {
            InitializeComponent();
            Location_S.AddHandler(MouseDownEvent, new MouseButtonEventHandler(Location_MouseDown), true);
            Location_S.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Location_MouseUp), true);
            br = (Brush)new BrushConverter().ConvertFromString("#BFFF2C8C");
            Init_List();
        }
        public async void Window_Show()
        {
            Opacity = 0;
            Visibility = Visibility.Visible;
            Position_Change();
            while (Opacity < 1 && !IsBusy)
            {
                Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
        }
        async void Position_Change()
        {
            while (Visibility == Visibility.Visible)
            {
                if (!IsBusy)
                {
                    if (Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_PLAYING && !IsLocationChanging)
                    {
                        long position = Bass.BASS_ChannelGetPosition(Stream);
                        Location_S.Value = Bass.BASS_ChannelBytes2Seconds(Stream, position);
                        TimeSpan Time = TimeSpan.FromSeconds(Location_S.Value);
                        string Minutes = Time.Minutes.ToString();
                        string Seconds = Time.Seconds.ToString();
                        if (Time.Minutes < 10)
                            Minutes = "0" + Time.Minutes;
                        if (Time.Seconds < 10)
                            Seconds = "0" + Time.Seconds;
                        Location_T.Text = Minutes + ":" + Seconds;
                    }
                    else if (Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_STOPPED && !IsLocationChanging && !IsPaused)
                    {
                        Location_S.Value = 0;
                        Location_T.Text = "00:00";
                    }
                }
                await Task.Delay(1000 / 30);
            }
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
        void Init_List()
        {
            Bass.BASS_ChannelStop(Stream);
            Bass.BASS_StreamFree(Stream);
            Location_S.Value = 0;
            Location_S.Maximum = 0;
            Location_T.Text = "00:00";
            Mod_Folder = "";
            Mod_Name_T.Text = "";
            Mod_Name = "";
            Event_List.Items.Clear();
            Voice_List.Items.Clear();
            Main_Event_List.Clear();
            Main_Event_List.Add(new WoWS_Event_Class("自身が撃沈", "自車両大破", 33));
            Main_Event_List.Add(new WoWS_Event_Class("敵空母を発見", "敵発見", 34));
            Main_Event_List.Add(new WoWS_Event_Class("敵戦艦を発見", "敵発見", 34));
            Main_Event_List.Add(new WoWS_Event_Class("敵巡洋艦を発見", "敵発見", 34));
            Main_Event_List.Add(new WoWS_Event_Class("敵駆逐艦を発見", "敵発見", 34));
            Main_Event_List.Add(new WoWS_Event_Class("敵艦を発見", "敵発見", 34));
            Main_Event_List.Add(new WoWS_Event_Class("敵潜水艦を発見", "敵発見", 34));
            Main_Event_List.Add(new WoWS_Event_Class("火災発生", "火災発生", 13));
            Main_Event_List.Add(new WoWS_Event_Class("敵空母を撃沈", "敵撃破", 9));
            Main_Event_List.Add(new WoWS_Event_Class("敵戦艦を撃沈", "敵撃破", 9));
            Main_Event_List.Add(new WoWS_Event_Class("敵巡洋艦を撃沈", "敵撃破", 9));
            Main_Event_List.Add(new WoWS_Event_Class("敵駆逐艦を撃沈", "敵撃破", 9));
            Main_Event_List.Add(new WoWS_Event_Class("敵潜水艦を撃沈", "敵撃破", 9));
            Main_Event_List.Add(new WoWS_Event_Class("FF警告", "味方へダメージ", 0));
            Main_Event_List.Add(new WoWS_Event_Class("バイタル貫通", "敵への貫通弾", 3));
            Main_Event_List.Add(new WoWS_Event_Class("バイタル貫通", "敵へのモジュール損傷", 4));
            Main_Event_List.Add(new WoWS_Event_Class("応急工作完了", "モジュール修理完了", new byte[] { 12, 18, 26, 29, 32 }));
            Main_Event_List.Add(new WoWS_Event_Class("戦闘開始", "戦闘開始", 23));
            Main_Event_List.Add(new WoWS_Event_Class("主機が機能停止", "エンジン破損", 10));
            Main_Event_List.Add(new WoWS_Event_Class("主砲が機能停止", "主砲破損", 16));
            Main_Event_List.Add(new WoWS_Event_Class("主砲が機能停止", "通信機破損", 21));
            Main_Event_List.Add(new WoWS_Event_Class("主砲が機能停止", "観測装置破損", 24));
            Main_Event_List.Add(new WoWS_Event_Class("主砲が機能停止", "履帯破損", 27));
            Main_Event_List.Add(new WoWS_Event_Class("主砲が機能停止", "砲塔破損", 30));
            Main_Event_List.Add(new WoWS_Event_Class("主砲が大破", "エンジン大破", 11));
            Main_Event_List.Add(new WoWS_Event_Class("主砲が大破", "主砲大破", 17));
            Main_Event_List.Add(new WoWS_Event_Class("主砲が大破", "観測装置大破", 25));
            Main_Event_List.Add(new WoWS_Event_Class("主砲が大破", "履帯大破", 28));
            Main_Event_List.Add(new WoWS_Event_Class("主砲が大破", "砲塔大破", 31));
            Main_Event_List.Add(new WoWS_Event_Class("了解！", "了解", 36));
            Main_Event_List.Add(new WoWS_Event_Class("拒否！", "拒否", 37));
            Main_Event_List.Add(new WoWS_Event_Class("援護射撃を求む！", "救援を請う", 38));
            Main_Event_List.Add(new WoWS_Event_Class("幸運を！", "固守せよ", 43));
            Main_Event_List.Add(new WoWS_Event_Class("敵艦を攻撃せよ", "攻撃せよ", 39));
            Main_Event_List.Add(new WoWS_Event_Class("支援射撃を求む！", "攻撃中", 40));
            Main_Event_List.Add(new WoWS_Event_Class("自陣を防衛せよ", "陣地を防衛せよ", 42));
            Main_Event_List.Add(new WoWS_Event_Class("エリアを防衛せよ", "陣地を防衛せよ", 42));
            Main_Event_List.Add(new WoWS_Event_Class("敵陣を占領せよ", "陣地を占領せよ", 41));
            Main_Event_List.Add(new WoWS_Event_Class("エリアを占領せよ", "陣地を占領せよ", 41));
            for (int Number = 0; Number < Main_Event_List.Count; Number++)
                Event_List.Items.Add(Main_Event_List[Number].From_Event_Name + "->" + Main_Event_List[Number].To_Event_Name + "-0個");
            Event_Enable_B.Foreground = br;
            Event_Disable_B.Foreground = br;
            Voice_Enable_B.Foreground = br;
            Voice_Disable_B.Foreground = br;
            Location_S.Value = 0;
            Location_S.Maximum = 0;
            Location_T.Text = "00:00";
            ColorMode_Change();
        }
        void ColorMode_Change()
        {
            int Select_Index = -1;
            if (Event_List.Visibility == Visibility.Visible && Event_List.SelectedIndex != -1)
                Select_Index = Event_List.SelectedIndex;
            for (int Number = 0; Number < Event_List.Items.Count; Number++)
            {
                if (Main_Event_List[Number].Voices.Count == 0)
                {
                    ListBoxItem LBI = new ListBoxItem();
                    LBI.Content = Main_Event_List[Number].From_Event_Name + "->" + Main_Event_List[Number].To_Event_Name + "-0個";
                    LBI.Foreground = br;
                    Event_List.Items[Number] = LBI;
                }
                else
                    Event_List.Items[Number] = Main_Event_List[Number].From_Event_Name + "->" + Main_Event_List[Number].To_Event_Name + "-" + Main_Event_List[Number].Voices.Count + "個";
            }
            if (Select_Index != -1)
                Event_List.SelectedIndex = Select_Index;
        }
        private async void Back_B_Click(object sender, RoutedEventArgs e)
        {
            if (!IsBusy)
            {
                IsBusy = true;
                Pause_Volume_Animation(true, 20f);
                while (Opacity > 0)
                {
                    Opacity -= Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
                IsBusy = false;
                Visibility = Visibility.Hidden;
                Voice_List.SelectedIndex = -1;
            }
        }
        string Get_Value(string Line)
        {
            string a = Line.Substring(Line.IndexOf('>') + 1);
            return a.Substring(0, a.IndexOf("</"));
        }
        private void WoWS_To_WoTB_Select_B_Click(object sender, RoutedEventArgs e)
        {
            if (!IsBusy && Opacity >= 1)
            {
                IsBusy = true;
                BetterFolderBrowser bfb = new BetterFolderBrowser()
                {
                    Title = "mod.xmlファイルが存在するフォルダを選択してください。",
                    RootFolder = Sub_Code.Get_OpenDirectory_Path(),
                    Multiselect = false,
                };
                if (bfb.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    IsBusy = true;
                    Sub_Code.Set_Directory_Path(bfb.SelectedFolder);
                    if (!File.Exists(bfb.SelectedFolder + "\\mod.xml"))
                    {
                        Message_Feed_Out("指定したフォルダ内にmod.xmlが含まれていません。");
                        IsBusy = false;
                        return;
                    }
                    Dictionary<string, Dictionary<string, List<string>>> XML_Events = new Dictionary<string, Dictionary<string, List<string>>>();
                    string[] Mod_XML = File.ReadAllLines(bfb.SelectedFolder + "\\mod.xml");
                    string ProjectName = "";
                    for (int Number_01 = 0; Number_01 < Mod_XML.Length; Number_01++)
                    {
                        if (Mod_XML[Number_01].Contains("<AudioModification>"))
                        {
                            ProjectName = Get_Value(Mod_XML[Number_01 + 1]);
                            Number_01++;
                        }
                        else if (Mod_XML[Number_01].Contains("<ExternalEvent>") && Mod_XML[Number_01 + 1].Contains("<Name>"))
                        {
                            string Event_Name = Get_Value(Mod_XML[Number_01 + 1]);
                            if (!XML_Events.ContainsKey(Event_Name))
                            {
                                XML_Events.Add(Event_Name, new Dictionary<string, List<string>>());
                                Number_01++;
                                List<string> Voices = new List<string>();
                                string State_Name = "";
                                bool IsStateMode = true;
                                bool IsReadedPath = false;
                                for (int Number_02 = Number_01; Number_02 < Mod_XML.Length; Number_02++)
                                {
                                    if (Mod_XML[Number_02].Contains("<Path>"))
                                    {
                                        IsStateMode = true;
                                        IsReadedPath = true;
                                    }
                                    if (IsReadedPath)
                                    {
                                        if (Mod_XML[Number_02].Contains("</StateList>") || Mod_XML[Number_02].Contains("<StateList/>"))
                                            IsStateMode = false;
                                        if (IsStateMode && Mod_XML[Number_02].Contains("<Value>"))
                                            State_Name += Get_Value(Mod_XML[Number_02]);
                                        if (!IsStateMode && Mod_XML[Number_02].Contains("<Name>"))
                                            Voices.Add(Get_Value(Mod_XML[Number_02]));
                                        if (Mod_XML[Number_02].Contains("</Path>"))
                                        {
                                            if (State_Name == "")
                                                State_Name = "None";
                                            if (!XML_Events[Event_Name].ContainsKey(State_Name))
                                                XML_Events[Event_Name].Add(State_Name, new List<string>(Voices));
                                            Voices.Clear();
                                            State_Name = "";
                                            IsReadedPath = false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (ProjectName == "" || XML_Events.Count == 0)
                    {
                        Message_Feed_Out("Mod.xmlを解析できませんでした。");
                        XML_Events.Clear();
                        IsBusy = false;
                        return;
                    }
                    Init_List();
                    Mod_Folder = bfb.SelectedFolder;
                    Mod_Name = ProjectName;
                    Mod_Name_T.Text = "プロジェクト名:" + Mod_Name;
                    if (XML_Events.ContainsKey("Play_Damage_Death"))
                        Main_Event_List[0].Voices = List_To_Voices(XML_Events["Play_Damage_Death"]["None"]);
                    if (XML_Events.ContainsKey("Play_VO_Detection_Enemy"))
                    {
                        if (XML_Events["Play_VO_Detection_Enemy"].ContainsKey("VO_Spotting_AirCarrier"))
                            Main_Event_List[1].Voices = List_To_Voices(XML_Events["Play_VO_Detection_Enemy"]["VO_Spotting_AirCarrier"]);
                        if (XML_Events["Play_VO_Detection_Enemy"].ContainsKey("VO_Spotting_Battleship"))
                            Main_Event_List[2].Voices = List_To_Voices(XML_Events["Play_VO_Detection_Enemy"]["VO_Spotting_Battleship"]);
                        if (XML_Events["Play_VO_Detection_Enemy"].ContainsKey("VO_Spotting_Cruiser"))
                            Main_Event_List[3].Voices = List_To_Voices(XML_Events["Play_VO_Detection_Enemy"]["VO_Spotting_Cruiser"]);
                        if (XML_Events["Play_VO_Detection_Enemy"].ContainsKey("VO_Spotting_Destroyer"))
                            Main_Event_List[4].Voices = List_To_Voices(XML_Events["Play_VO_Detection_Enemy"]["VO_Spotting_Destroyer"]);
                        if (XML_Events["Play_VO_Detection_Enemy"].ContainsKey("VO_Spotting_Multi"))
                            Main_Event_List[5].Voices = List_To_Voices(XML_Events["Play_VO_Detection_Enemy"]["VO_Spotting_Multi"]);
                        if (XML_Events["Play_VO_Detection_Enemy"].ContainsKey("VO_Spotting_Submarine"))
                            Main_Event_List[6].Voices = List_To_Voices(XML_Events["Play_VO_Detection_Enemy"]["VO_Spotting_Submarine"]);
                    }
                    if (XML_Events.ContainsKey("Play_VO_Fire_Alarm"))
                        Main_Event_List[7].Voices = List_To_Voices(XML_Events["Play_VO_Fire_Alarm"]["True"]);
                    if (XML_Events.ContainsKey("Play_VO_Frags"))
                    {
                        if (IsContainsState(XML_Events["Play_VO_Frags"], "Damage_Frag_AirCarrier", "False", out string StateName1))
                            Main_Event_List[8].Voices = List_To_Voices(XML_Events["Play_VO_Fire_Alarm"][StateName1]);
                        if (IsContainsState(XML_Events["Play_VO_Frags"], "Damage_Frag_Battleship", "False", out string StateName2))
                            Main_Event_List[9].Voices = List_To_Voices(XML_Events["Play_VO_Fire_Alarm"][StateName2]);
                        if (IsContainsState(XML_Events["Play_VO_Frags"], "Damage_Frag_Cruiser", "False", out string StateName3))
                            Main_Event_List[10].Voices = List_To_Voices(XML_Events["Play_VO_Fire_Alarm"][StateName3]);
                        if (IsContainsState(XML_Events["Play_VO_Frags"], "Damage_Frag_Destroyer", "False", out string StateName4))
                            Main_Event_List[11].Voices = List_To_Voices(XML_Events["Play_VO_Fire_Alarm"][StateName4]);
                        if (IsContainsState(XML_Events["Play_VO_Frags"], "Damage_Frag_Submarine", "False", out string StateName5))
                            Main_Event_List[12].Voices = List_To_Voices(XML_Events["Play_VO_Fire_Alarm"][StateName5]);
                    }
                    if (XML_Events.ContainsKey("Play_VO_Friendly_Hit"))
                        Main_Event_List[13].Voices = List_To_Voices(XML_Events["Play_VO_Friendly_Hit"]["None"]);
                    if (XML_Events.ContainsKey("Play_VO_Hit_Feedback_Good_Hit"))
                    {
                        Main_Event_List[14].Voices = List_To_Voices(XML_Events["Play_VO_Hit_Feedback_Good_Hit"]["None"]);
                        Main_Event_List[15].Voices = List_To_Voices(XML_Events["Play_VO_Hit_Feedback_Good_Hit"]["None"]);
                    }
                    if (XML_Events.ContainsKey("Play_UI_Consumable_End") && XML_Events["Play_UI_Consumable_End"].ContainsKey("CrashCrew"))
                        Main_Event_List[16].Voices = List_To_Voices(XML_Events["Play_UI_Consumable_End"]["CrashCrew"]);
                    if (XML_Events.ContainsKey("Play_Start_Battle"))
                        Main_Event_List[17].Voices = List_To_Voices(XML_Events["Play_Start_Battle"]["None"]);
                    if (XML_Events.ContainsKey("Play_UI_Alarm_Defective_Modules") && IsContainsState(XML_Events["Play_UI_Alarm_Defective_Modules"], "Module_State_Crit",
                        "Module_Type_Engine", out string StateName6))
                        Main_Event_List[18].Voices = List_To_Voices(XML_Events["Play_UI_Alarm_Defective_Modules"][StateName6]);
                    if (XML_Events.ContainsKey("Play_UI_Alarm_Defective_Modules") && IsContainsState(XML_Events["Play_UI_Alarm_Defective_Modules"], "Module_State_Crit",
                       "Module_Type_MG", out string StateName7))
                    {
                        Main_Event_List[19].Voices = List_To_Voices(XML_Events["Play_UI_Alarm_Defective_Modules"][StateName7]);
                        Main_Event_List[20].Voices = List_To_Voices(XML_Events["Play_UI_Alarm_Defective_Modules"][StateName7]);
                        Main_Event_List[21].Voices = List_To_Voices(XML_Events["Play_UI_Alarm_Defective_Modules"][StateName7]);
                        Main_Event_List[22].Voices = List_To_Voices(XML_Events["Play_UI_Alarm_Defective_Modules"][StateName7]);
                        Main_Event_List[23].Voices = List_To_Voices(XML_Events["Play_UI_Alarm_Defective_Modules"][StateName7]);
                    }
                    if (XML_Events.ContainsKey("Play_UI_Alarm_Defective_Modules") && IsContainsState(XML_Events["Play_UI_Alarm_Defective_Modules"], "Module_Type_MG",
                       "Module_State_Dead", out string StateName8))
                    {
                        Main_Event_List[24].Voices = List_To_Voices(XML_Events["Play_UI_Alarm_Defective_Modules"][StateName8]);
                        Main_Event_List[25].Voices = List_To_Voices(XML_Events["Play_UI_Alarm_Defective_Modules"][StateName8]);
                        Main_Event_List[26].Voices = List_To_Voices(XML_Events["Play_UI_Alarm_Defective_Modules"][StateName8]);
                        Main_Event_List[27].Voices = List_To_Voices(XML_Events["Play_UI_Alarm_Defective_Modules"][StateName8]);
                        Main_Event_List[28].Voices = List_To_Voices(XML_Events["Play_UI_Alarm_Defective_Modules"][StateName8]);
                    }
                    if (XML_Events.ContainsKey("Play_VO_Quick_Commands") && XML_Events["Play_VO_Quick_Commands"].ContainsKey("CMD_QUICK_AYE_AYE"))
                        Main_Event_List[29].Voices = List_To_Voices(XML_Events["Play_VO_Quick_Commands"]["CMD_QUICK_AYE_AYE"]);
                    if (XML_Events.ContainsKey("Play_VO_Quick_Commands") && XML_Events["Play_VO_Quick_Commands"].ContainsKey("CMD_QUICK_NO_WAY"))
                        Main_Event_List[30].Voices = List_To_Voices(XML_Events["Play_VO_Quick_Commands"]["CMD_QUICK_NO_WAY"]);
                    if (XML_Events.ContainsKey("Play_VO_Quick_Commands") && XML_Events["Play_VO_Quick_Commands"].ContainsKey("CMD_QUICK_SOS"))
                        Main_Event_List[31].Voices = List_To_Voices(XML_Events["Play_VO_Quick_Commands"]["CMD_QUICK_SOS"]);
                    else if (XML_Events.ContainsKey("Play_VO_Quick_Commands") && XML_Events["Play_VO_Quick_Commands"].ContainsKey("CMD_QUICK_NEED_SUPPORT_ALLY_SHIP"))
                        Main_Event_List[31].Voices = List_To_Voices(XML_Events["Play_VO_Quick_Commands"]["CMD_QUICK_NEED_SUPPORT_ALLY_SHIP"]);
                    if (XML_Events.ContainsKey("Play_VO_Quick_Commands") && XML_Events["Play_VO_Quick_Commands"].ContainsKey("CMD_QUICK_GOOD_LUCK"))
                        Main_Event_List[32].Voices = List_To_Voices(XML_Events["Play_VO_Quick_Commands"]["CMD_QUICK_GOOD_LUCK"]);
                    if (XML_Events.ContainsKey("Play_VO_Quick_Commands") && XML_Events["Play_VO_Quick_Commands"].ContainsKey("CMD_QUICK_TACTIC_ENEMY_SHIP"))
                        Main_Event_List[33].Voices = List_To_Voices(XML_Events["Play_VO_Quick_Commands"]["CMD_QUICK_TACTIC_ENEMY_SHIP"]);
                    if (XML_Events.ContainsKey("Play_VO_Quick_Commands") && XML_Events["Play_VO_Quick_Commands"].ContainsKey("CMD_QUICK_NEED_SUPPORT"))
                        Main_Event_List[34].Voices = List_To_Voices(XML_Events["Play_VO_Quick_Commands"]["CMD_QUICK_NEED_SUPPORT"]);
                    if (XML_Events.ContainsKey("Play_VO_Quick_Commands") && XML_Events["Play_VO_Quick_Commands"].ContainsKey("CMD_QUICK_TACTIC_ALLY_BASE"))
                        Main_Event_List[35].Voices = List_To_Voices(XML_Events["Play_VO_Quick_Commands"]["CMD_QUICK_TACTIC_ALLY_BASE"]);
                    if (XML_Events.ContainsKey("Play_VO_Quick_Commands") && XML_Events["Play_VO_Quick_Commands"].ContainsKey("CMD_QUICK_TACTIC_ALLY_POINT"))
                        Main_Event_List[36].Voices = List_To_Voices(XML_Events["Play_VO_Quick_Commands"]["CMD_QUICK_TACTIC_ALLY_POINT"]);
                    if (XML_Events.ContainsKey("Play_VO_Quick_Commands") && XML_Events["Play_VO_Quick_Commands"].ContainsKey("CMD_QUICK_TACTIC_ENEMY_BASE"))
                        Main_Event_List[37].Voices = List_To_Voices(XML_Events["Play_VO_Quick_Commands"]["CMD_QUICK_TACTIC_ENEMY_BASE"]);
                    if (XML_Events.ContainsKey("Play_VO_Quick_Commands") && XML_Events["Play_VO_Quick_Commands"].ContainsKey("CMD_QUICK_TACTIC_ENEMY_POINT"))
                        Main_Event_List[38].Voices = List_To_Voices(XML_Events["Play_VO_Quick_Commands"]["CMD_QUICK_TACTIC_ENEMY_POINT"]);
                    XML_Events.Clear();
                    ColorMode_Change();
                }
                IsBusy = false;
            }
        }
        List<WoWS_Voice_Class> List_To_Voices(List<string> Voices)
        {
            List<WoWS_Voice_Class> Voice_Class = new List<WoWS_Voice_Class>();
            foreach (string Voice in Voices)
                Voice_Class.Add(new WoWS_Voice_Class(Voice));
            return Voice_Class;
        }
        bool IsContainsState(Dictionary<string, List<string>> Event_State, string State1, string State2, out string StateName)
        {
            if (Event_State.ContainsKey(State1 + State2))
            {
                StateName = State1 + State2;
                return true;
            }
            else if (Event_State.ContainsKey(State2 + State1))
            {
                StateName = State2 + State1;
                return true;
            }
            StateName = "";
            return false;
        }
        private void Event_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IsNoEncodeMode = false;
            Voice_List.Items.Clear();
            Bass.BASS_ChannelStop(Stream);
            Bass.BASS_StreamFree(Stream);
            Location_S.Value = 0;
            Location_S.Maximum = 0;
            Location_T.Text = "00:00";
            if (Event_List.SelectedIndex == -1)
            {
                Event_Enable_B.Foreground = br;
                Event_Disable_B.Foreground = br;
                Voice_Enable_B.Foreground = br;
                Voice_Disable_B.Foreground = br;
                return;
            }
            foreach (WoWS_Voice_Class Voice in Main_Event_List[Event_List.SelectedIndex].Voices)
            {
                if (Voice.IsEnable)
                    Voice_List.Items.Add(Path.GetFileName(Voice.File_Path));
                else
                {
                    ListBoxItem LBI = new ListBoxItem();
                    LBI.Content = Path.GetFileName(Voice.File_Path);
                    LBI.Foreground = br;
                    Voice_List.Items.Add(LBI);
                }
            }
            if (Main_Event_List[Event_List.SelectedIndex].IsEnable)
            {
                Event_Disable_B.Foreground = Brushes.Aqua;
                Event_Enable_B.Foreground = br;
            }
            else
            {
                Event_Disable_B.Foreground = br;
                Event_Enable_B.Foreground = Brushes.Aqua;
            }
        }
        private async void Voice_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Event_List.SelectedIndex == -1 || Voice_List.SelectedIndex == -1)
            {
                Voice_Enable_B.Foreground = br;
                Voice_Disable_B.Foreground = br;
                return;
            }
            Bass.BASS_ChannelStop(Stream);
            Bass.BASS_StreamFree(Stream);
            WoWS_Voice_Class Voice = Main_Event_List[Event_List.SelectedIndex].Voices[Voice_List.SelectedIndex];
            if (Voice.IsEnable)
            {
                Voice_Disable_B.Foreground = Brushes.Aqua;
                Voice_Enable_B.Foreground = br;
            }
            else
            {
                Voice_Disable_B.Foreground = br;
                Voice_Enable_B.Foreground = Brushes.Aqua;
            }
            string File_Path = "";
            if (!IsNoEncodeMode)
            {
                Message_T.Text = "音声ファイルへ変換しています...";
                await Task.Delay(50);
                File_Path = Sub_Code.WEM_To_OGG_WAV(Mod_Folder + "\\" + Main_Event_List[Event_List.SelectedIndex].Voices[Voice_List.SelectedIndex].File_Path,
                    Voice_Set.Special_Path + "\\Wwise\\WoWS_To_Blitz_01", false);
                Message_T.Text = "";
            }
            else
                File_Path = Sub_Code.File_Get_FileName_No_Extension(Voice_Set.Special_Path + "\\Wwise\\WoWS_To_Blitz_01");
            IsNoEncodeMode = false;
            Location_S.Value = 0;
            Location_T.Text = "00:00";
            if (File_Path == "")
            {
                Message_Feed_Out("選択した.wemファイルは破損しています。");
                Location_S.Maximum = 0;
                return;
            }
            int StreamHandle = Bass.BASS_StreamCreateFile(File_Path, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_LOOP);
            Stream = BassFx.BASS_FX_TempoCreate(StreamHandle, BASSFlag.BASS_FX_FREESOURCE);
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)Volume_S.Value / 100);
            Bass.BASS_ChannelSetDevice(Stream, Video_Mode.Sound_Device);
            Location_S.Maximum = Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetLength(Stream, BASSMode.BASS_POS_BYTES));
        }
        private void Event_Disable_B_Click(object sender, RoutedEventArgs e)
        {
            if (Event_List.SelectedIndex == -1 || IsBusy)
                return;
            ListBoxItem LBI = new ListBoxItem();
            LBI.Content = Main_Event_List[Event_List.SelectedIndex].From_Event_Name + "->" + Main_Event_List[Event_List.SelectedIndex].To_Event_Name + "-" + 
                Main_Event_List[Event_List.SelectedIndex].Voices.Count + "個";
            LBI.Foreground = br;
            int Selected_Index = Event_List.SelectedIndex;
            Event_List.Items[Event_List.SelectedIndex] = LBI;
            Event_List.SelectedIndex = Selected_Index;
            Main_Event_List[Event_List.SelectedIndex].IsEnable = false;
            Event_Disable_B.Foreground = br;
            Event_Enable_B.Foreground = Brushes.Aqua;
        }
        private void Event_Enable_B_Click(object sender, RoutedEventArgs e)
        {
            if (Event_List.SelectedIndex == -1 || IsBusy)
                return;
            if (Main_Event_List[Event_List.SelectedIndex].Voices.Count > 0)
            {
                int Selected_Index = Event_List.SelectedIndex;
                Event_List.Items[Event_List.SelectedIndex] = Main_Event_List[Event_List.SelectedIndex].From_Event_Name + "->" + Main_Event_List[Event_List.SelectedIndex].To_Event_Name + "-" +
                    Main_Event_List[Event_List.SelectedIndex].Voices.Count + "個";
                Event_List.SelectedIndex = Selected_Index;
            }
            Main_Event_List[Event_List.SelectedIndex].IsEnable = true;
            Event_Disable_B.Foreground = Brushes.Aqua;
            Event_Enable_B.Foreground = br;
        }
        private void Voice_Disable_B_Click(object sender, RoutedEventArgs e)
        {
            if (Event_List.SelectedIndex == -1 || Voice_List.SelectedIndex == -1 || IsBusy)
                return;
            IsNoEncodeMode = true;
            ListBoxItem LBI = new ListBoxItem();
            LBI.Content = Path.GetFileName(Main_Event_List[Event_List.SelectedIndex].Voices[Voice_List.SelectedIndex].File_Path);
            LBI.Foreground = br;
            int Selected_Index = Voice_List.SelectedIndex;
            Voice_List.Items[Voice_List.SelectedIndex] = LBI;
            Voice_List.SelectedIndex = Selected_Index;
            Main_Event_List[Event_List.SelectedIndex].Voices[Voice_List.SelectedIndex].IsEnable = false;
            Voice_Disable_B.Foreground = br;
            Voice_Enable_B.Foreground = Brushes.Aqua;
        }
        private void Voice_Enable_B_Click(object sender, RoutedEventArgs e)
        {
            if (Event_List.SelectedIndex == -1 || Voice_List.SelectedIndex == -1 || IsBusy)
                return;
            IsNoEncodeMode = true;
            int Selected_Index = Voice_List.SelectedIndex;
            Voice_List.Items[Voice_List.SelectedIndex] = Path.GetFileName(Main_Event_List[Event_List.SelectedIndex].Voices[Voice_List.SelectedIndex].File_Path);
            Voice_List.SelectedIndex = Selected_Index;
            Main_Event_List[Event_List.SelectedIndex].Voices[Voice_List.SelectedIndex].IsEnable = true;
            Voice_Disable_B.Foreground = Brushes.Aqua;
            Voice_Enable_B.Foreground = br;
        }
        private void Clear_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
                return;
            MessageBoxResult result = MessageBox.Show("現在の内容をクリアしますか?", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                Init_List();
                Message_Feed_Out("内容をクリアしました。");
            }
        }
        private void Play_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
                return;
            Bass.BASS_ChannelPlay(Stream, false);
            IsPaused = false;
        }
        private void Pause_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
                return;
            Bass.BASS_ChannelPause(Stream);
            IsPaused = true;
        }
        private void Volume_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Volume_T.Text = "音量:" + (int)Volume_S.Value;
            if (!IsPaused)
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)Volume_S.Value / 100);
        }
        async void Play_Volume_Animation(float Feed_Time = 30f)
        {
            IsPaused = false;
            Bass.BASS_ChannelPlay(Stream, false);
            float Volume_Now = 1f;
            Bass.BASS_ChannelGetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, ref Volume_Now);
            float Volume_Plus = (float)(Volume_S.Value / 100) / Feed_Time;
            while (Volume_Now < (float)(Volume_S.Value / 100) && !IsPaused)
            {
                Volume_Now += Volume_Plus;
                if (Volume_Now > 1f)
                    Volume_Now = 1f;
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, Volume_Now);
                await Task.Delay(1000 / 60);
            }
        }
        public async void Pause_Volume_Animation(bool IsStop, float Feed_Time = 30f)
        {
            IsPaused = true;
            float Volume_Now = 1f;
            Bass.BASS_ChannelGetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, ref Volume_Now);
            float Volume_Minus = Volume_Now / Feed_Time;
            while (Volume_Now > 0f && IsPaused)
            {
                Volume_Now -= Volume_Minus;
                if (Volume_Now < 0f)
                    Volume_Now = 0f;
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, Volume_Now);
                await Task.Delay(1000 / 60);
            }
            if (Volume_Now <= 0f)
            {
                if (IsStop)
                {
                    Bass.BASS_ChannelStop(Stream);
                    Bass.BASS_StreamFree(Stream);
                    Location_S.Value = 0;
                    Location_S.Maximum = 0;
                    Location_T.Text = "00:00";
                }
                else if (IsPaused)
                    Bass.BASS_ChannelPause(Stream);
            }
        }
        void Location_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IsLocationChanging = true;
            if (Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_PLAYING)
            {
                IsPlayingMouseDown = true;
                Pause_Volume_Animation(false, 10);
            }
        }
        void Location_MouseUp(object sender, MouseButtonEventArgs e)
        {
            IsLocationChanging = false;
            Bass.BASS_ChannelSetPosition(Stream, Location_S.Value);
            if (IsPlayingMouseDown)
            {
                IsPaused = false;
                Play_Volume_Animation(10f);
                IsPlayingMouseDown = false;
            }
        }
        private void Location_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLocationChanging)
                Music_Pos_Change(Location_S.Value, false);
        }
        void Music_Pos_Change(double Pos, bool IsBassPosChange)
        {
            if (IsBusy)
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
            Location_T.Text = Minutes + ":" + Seconds;
        }
        private async void Convert_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
                return;
            if (Mod_Folder == "")
            {
                Message_Feed_Out("変換するModが指定されていません。");
                return;
            }
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog()
            {
                Title = "セーブファイルの保存先を指定してください。",
                Filter = ".wvsファイル(*.wvs)|*.wvs"
            };
            if (Voice_Create.Save_Load_Dir == "" || Directory.Exists(Voice_Create.Save_Load_Dir))
                sfd.InitialDirectory = Voice_Set.Local_Path;
            else
                sfd.InitialDirectory = Voice_Create.Save_Load_Dir;
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!Sub_Code.CanDirectoryAccess(Path.GetDirectoryName(sfd.FileName)))
                {
                    Message_Feed_Out("指定したフォルダにアクセスできませんでした。アクセス権限があるか確認してください。");
                    sfd.Dispose();
                    return;
                }
                IsBusy = true;
                Message_T.Text = "セーブファイルを作成しています...";
                await Task.Delay(50);
                if (Directory.Exists(Voice_Set.Local_Path + "\\Projects\\" + Mod_Name + "\\All_Voices"))
                    Directory.Delete(Voice_Set.Local_Path + "\\Projects\\" + Mod_Name + "\\All_Voices", true);
                Directory.CreateDirectory(Voice_Set.Local_Path + "\\Projects\\" + Mod_Name + "\\All_Voices");
                StreamWriter stw = File.CreateText(sfd.FileName + ".tmp");
                stw.WriteLine(Mod_Name + "|IsNotChangeProjectNameMode=true");
                foreach (WoWS_Event_Class Events in Main_Event_List)
                {
                    if (Events.IsEnable)
                    {
                        foreach (WoWS_Voice_Class Voices in Events.Voices)
                        {
                            if (Voices.IsEnable)
                            {
                                string To = Voice_Set.Local_Path + "\\Projects\\" + Mod_Name + "\\All_Voices\\" + Path.GetFileNameWithoutExtension(Voices.File_Path);
                                string File_Path;
                                if (!Sub_Code.File_Exists(To))
                                    File_Path = Sub_Code.WEM_To_OGG_WAV(Mod_Folder + "\\" + Voices.File_Path, To, false);
                                else
                                    File_Path = Sub_Code.File_Get_FileName_No_Extension(To);
                                if (File_Path == "")
                                    continue;
                                if (Events.Event_Index != 255)
                                    stw.WriteLine(Events.Event_Index + "|" + File_Path);
                                else
                                    foreach (byte Index in Events.Events_Index)
                                        stw.WriteLine(Index + "|" + File_Path);
                            }
                        }
                    }
                }
                stw.Close();
                Sub_Code.File_Encrypt(sfd.FileName + ".tmp", sfd.FileName, "SRTTbacon_Create_Voice_Save", true);
                Message_Feed_Out("保存しました。\nProjectsフォルダに該当する音声が入っています。");
                Flash.Flash_Start();
                IsBusy = false;
            }
            sfd.Dispose();
        }
    }
}