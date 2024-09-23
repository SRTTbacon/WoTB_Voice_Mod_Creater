using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Flac;
using Un4seen.Bass.AddOn.Fx;
using WK.Libraries.BetterFolderBrowserNS;

namespace WoTB_Voice_Mod_Creater.Class
{
    public class SE_Type
    {
        public Dictionary<uint, List<string>> items = new Dictionary<uint, List<string>>();
        public bool bEnable = true;

        public void Add(string files)
        {
            Add(0, files);
        }
        public void Add(uint shortID, string files)
        {
            if (!items.ContainsKey(shortID))
                items.Add(shortID, new List<string>());
            foreach (string file in files.Split('|'))
                items[shortID].Add(file);
        }
        public SE_Type Clone()
        {
            SE_Type clone = (SE_Type)MemberwiseClone();
            clone.items = new Dictionary<uint, List<string>>();
            foreach (KeyValuePair<uint, List<string>> item in items)
            {
                clone.items.Add(item.Key, new List<string>(item.Value));
            }
            return clone;
        }
        public List<string> GetRandomItems()
        {
            if (items.Count == 0)
            {
                return null;
            }
            foreach (uint key in items.Keys)
            {
                return items[key];
            }
            return null;
        }
    }

    public class SE_Preset
    {
        public List<SE_Type> types = new List<SE_Type>();
        public string presetName = "";
        public uint presetID = 0;
        public SE_Preset(string presetName)
        {
            this.presetName = presetName;
            presetID = WwiseHash.HashString(presetName);
        }
        public SE_Preset Clone()
        {
            SE_Preset clone = (SE_Preset)MemberwiseClone();
            clone.types = new List<SE_Type>();
            foreach(SE_Type type in types)
            {
                SE_Type newType = new SE_Type();
                if (type.items.ContainsKey(0))
                {
                    List<string> newList = new List<string>();
                    newType.items.Add(0, newList);
                    foreach (string file in type.items[0])
                        newList.Add(file);
                }
                clone.types.Add(newType);
            }
            return clone;
        }
    }
    public class SE_Setting
    {
        public SE_Preset sePreset;
        public SE_Preset seDefault;

        public const int SE_COUNT = 25;

        private readonly string SE = Voice_Set.Special_Path + "\\SE\\";

        public SE_Setting()
        {
            sePreset = new SE_Preset("標準");
            seDefault = new SE_Preset("標準");
            for (int Number = 0; Number < SE_COUNT; Number++)
                seDefault.types.Add(new SE_Type());
            seDefault.types[0].Add(0, SE + "Capture_Finish_SE.mp3");
            seDefault.types[1].Add(904269149, SE + "quick_commands_positive.mp3");
            seDefault.types[1].Add(747137713, SE + "quick_commands_negative.mp3");
            seDefault.types[1].Add(990119123, SE + "quick_commands_help_me.mp3");
            seDefault.types[1].Add(560124813, SE + "quick_commands_reloading.mp3");
            seDefault.types[1].Add(1039956691, SE + "quick_commands_attack.mp3");
            seDefault.types[1].Add(1041861596, SE + "quick_commands_attack_target.mp3");
            seDefault.types[1].Add(284419845, SE + "quick_commands_capture_base.mp3");
            seDefault.types[1].Add(93467631, SE + "quick_commands_defend_base.mp3");
            seDefault.types[10].Add(0, SE + "howitzer_load_01.wav|" + SE + "howitzer_load_02.wav|" + SE + "howitzer_load_03.wav|" + SE + "howitzer_load_04.wav");
            seDefault.types[11].Add(0, SE + "lamp_SE_01.mp3");
            seDefault.types[12].Add(0, SE + "tekihakken_SE_01.mp3");
            seDefault.types[13].Add(0, SE + "Timer_SE.mp3");
            seDefault.types[14].Add(0, SE + "target_on_SE_01.wav");
            seDefault.types[15].Add(0, SE + "target_off_SE_01.wav");
            seDefault.types[16].Add(0, SE + "Noise_01.mp3|" + SE + "Noise_02.mp3|" + SE + "Noise_03.mp3|" + SE + "Noise_04.mp3|" + SE + "Noise_05.mp3|" + SE + "Noise_06.mp3");
            seDefault.types[16].Add(0, SE + "Noise_07.mp3|" + SE + "Noise_08.mp3|" + SE + "Noise_09.mp3|" + SE + "Noise_10.mp3");
            seDefault.types[23].Add(0, SE + "Map_Click_01.wav");
            seDefault.types[24].Add(0, SE + "Map_Move_01.wav");

            SetDefault(sePreset);
        }

        public void SetDefault(SE_Preset sep)
        {
            sep.presetName = "標準";
            sep.presetID = WwiseHash.HashString(sep.presetName);
            sep.types.Clear();
            for (int Number = 0; Number < SE_COUNT; Number++)
            {
                sep.types.Add(new SE_Type());
            }
            sep.types[0].Add(0, SE + "Capture_End_01.mp3|" + SE + "Capture_End_02.mp3");
            sep.types[1].Add(0, SE + "Command_01.wav");
            sep.types[2].Add(0, SE + "Danyaku_SE_01.mp3");
            sep.types[3].Add(0, SE + "Destroy_01.mp3");
            sep.types[4].Add(0, SE + "Enable_01.mp3|" + SE + "Enable_02.mp3|" + SE + "Enable_03.mp3");
            sep.types[5].Add(0, SE + "Enable_Special_01.mp3");
            sep.types[6].Add(0, SE + "Musenki_01.mp3");
            sep.types[7].Add(0, SE + "Nenryou_SE_01.mp3");
            sep.types[8].Add(0, SE + "Not_Enable_01.mp3");
            sep.types[9].Add(0, SE + "Not_Enable_01.mp3");
            sep.types[10].Add(0, SE + "Reload_01.mp3|" + SE + "Reload_02.mp3|" + SE + "Reload_03.mp3|" + SE + "Reload_04.mp3|" + SE + "Reload_05.mp3|" + SE + "Reload_06.mp3");
            sep.types[11].Add(0, SE + "Sixth_01.mp3|" + SE + "Sixth_02.mp3|" + SE + "Sixth_03.mp3");
            sep.types[12].Add(0, SE + "Spot_01.mp3");
            sep.types[13].Add(0, SE + "Timer_01.wav|" + SE + "Timer_02.wav");
            sep.types[14].Add(0, SE + "Lock_01.mp3");
            sep.types[15].Add(0, SE + "Unlock_01.mp3");
            sep.types[16].Add(0, SE + "Noise_01.mp3|" + SE + "Noise_02.mp3|" + SE + "Noise_03.mp3|" + SE + "Noise_04.mp3|" + SE + "Noise_05.mp3|" + SE + "Noise_06.mp3");
            sep.types[16].Add(0, SE + "Noise_07.mp3|" + SE + "Noise_08.mp3|" + SE + "Noise_09.mp3|" + SE + "Noise_10.mp3");
            sep.types[23].Add(0, SE + "Map_Click_01.wav");
            sep.types[24].Add(0, SE + "Map_Move_01.wav");
        }

        public void SetZeroSounds(SE_Preset sep)
        {
            for (int i = 0; i < sep.types.Count; i++)
            {
                if (sep.types[i].items.Count == 0 && ((i >= 0 && i <= 16) || (i >= 23 && i <= 24)))
                    sep.types[i].items.Add(0, new List<string>());
            }
        }
    }
    public partial class Save_Configs : UserControl
    {
        private const byte VERSION_CONF = 0;
        private const byte VERSION_PRESET = 0;

        public SE_Setting seSetting = null;

        readonly List<SE_Preset> loadedPresets = new List<SE_Preset>();

        readonly BrushConverter bc = new BrushConverter();

        List<Voice_Event_Setting> settings;

        readonly string[] Languages = { "arb", "cn", "cs", "de", "en", "es", "fi", "fr", "gup", "it", "ja", "ko", "pbr", "pl", "ru", "th", "tr", "vi" };
        string seDir = "";
        int selectedLanguageIndex = 10;
        int nowPresetIndex = 0;
        int Stream;
        bool bClosing = false;
        bool bMessageShowing = false;
        bool bLoaded = false;
        bool bPushedLeftControl = false;
        bool bOpenedChangeDir = false;

        public Save_Configs()
        {
            InitializeComponent();
            Add_SE_List("時間切れ&占領ポイントMax", true);
            Add_SE_List("クイックコマンド", true);
            Add_SE_List("弾薬庫破損", true);
            Add_SE_List("自車両大破", true);
            Add_SE_List("貫通", true);
            Add_SE_List("敵モジュール破損", true);
            Add_SE_List("無線機破損", true);
            Add_SE_List("燃料タンク破損", true);
            Add_SE_List("非貫通-無効弾", true);
            Add_SE_List("非貫通-跳弾", true);
            Add_SE_List("装填完了", true);
            Add_SE_List("第六感", true);
            Add_SE_List("敵発見", true);
            Add_SE_List("戦闘開始前タイマー", true);
            Add_SE_List("ロックオン", true);
            Add_SE_List("アンロック", true);
            Add_SE_List("ノイズ音", true);
            Add_SE_List("搭乗員負傷", true);
            Add_SE_List("モジュール破損", true);
            Add_SE_List("モジュール大破", true);
            Add_SE_List("モジュール復旧", true);
            Add_SE_List("戦闘開始", true);
            Add_SE_List("敵炎上", true);
            Add_SE_List("マップクリック", true);
            Add_SE_List("移動中", true);
            Load_Combo.Items.Add("標準");
            Load_Combo.SelectedIndex = 0;
        }
        void Add_SE_List(string Text, bool IsEnable)
        {
            ListBoxItem Item = new ListBoxItem()
            {
                Content = Text + " | "
            };
            if (IsEnable)
                Item.Content += "有効";
            else
            {
                Item.Content += "無効";
                Item.Foreground = (Brush)new BrushConverter().ConvertFromString("#BFFF2C8C");
            }
            SE_Lists.Items.Add(Item);
        }
        async void Message_Feed_Out(string Message)
        {
            if (bMessageShowing)
            {
                bMessageShowing = false;
                await Task.Delay(1000 / 59);
            }
            Message_T.Text = Message;
            bMessageShowing = true;
            Message_T.Opacity = 1;
            int Number = 0;
            bool bForce = false;
            while (Message_T.Opacity > 0)
            {
                if (!bMessageShowing)
                {
                    bForce = true;
                    break;
                }
                Number++;
                if (Number >= 120)
                    Message_T.Opacity -= 0.025;
                await Task.Delay(1000 / 60);
            }
            if (!bForce)
            {
                bMessageShowing = false;
                Message_T.Text = "";
                Message_T.Opacity = 1;
            }
        }

        public void InitializeLoadData()
        {
            if (!bLoaded)
                Configs_Load();
        }

        public void Window_Show_V2(string Project_Name, List<Voice_Event_Setting> Lists)
        {
            //画面を表示(オフラインモードで行った場合)
            Volume_Set_C.Visibility = Visibility.Visible;
            Volume_Set_T.Visibility = Visibility.Visible;
            Default_Voice_Mode_C.Visibility = Visibility.Visible;
            Default_Voice_Mode_T.Visibility = Visibility.Visible;
            Exit_B.Visibility = Visibility.Visible;
            Save_B.Content = "作成";
            Language_Left_B.Visibility = Visibility.Visible;
            Language_Right_B.Visibility = Visibility.Visible;
            Android_T.Text = "言語:" + Languages[selectedLanguageIndex];
            if (!bLoaded)
                Configs_Load();
            seDir = Voice_Set.Special_Path + "/SE";
            Project_T.Text = "プロジェクト名:" + Project_Name;
            settings = Lists;
        }
        public async void Window_Show_V3(string BNK_Name, List<Voice_Event_Setting> Lists)
        {
            Opacity = 0;
            Visibility = Visibility.Visible;
            Language_Left_B.Visibility = Visibility.Hidden;
            Language_Right_B.Visibility = Visibility.Hidden;
            Android_T.Visibility = Visibility.Hidden;
            Volume_Set_C.Visibility = Visibility.Hidden;
            Volume_Set_T.Visibility = Visibility.Hidden;
            DVPL_C.Visibility = Visibility.Hidden;
            DVPL_T.Visibility = Visibility.Hidden;
            Exit_B.Visibility = Visibility.Hidden;
            Default_Voice_Mode_C.Visibility = Visibility.Hidden;
            Default_Voice_Mode_T.Visibility = Visibility.Hidden;
            Save_B.Content = "保存";
            if (!bLoaded)
                Configs_Load();
            seDir = Voice_Set.Special_Path + "/SE";
            Project_T.Text = "プロジェクト名:" + BNK_Name;
            settings = Lists;
            while (Opacity < 1 && !bClosing)
            {
                Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
        }

        void Loop()
        {
            bPushedLeftControl = (Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) > 0;
            if (bPushedLeftControl && (Keyboard.GetKeyStates(Key.D) & KeyStates.Down) > 0)
                ReplaceSEDir();
        }

        void Change_SE_List(int Index = -1)
        {
            if (Index == -1)
            {
                for (int i = 0; i < SE_Lists.Items.Count; i++)
                    Change_SE_List(i);
            }
            else
            {
                ListBoxItem Item = SE_Lists.Items[Index] as ListBoxItem;
                string Text = Item.Content.ToString();
                Text = Text.Substring(0, Text.IndexOf('|') + 2);
                if (seSetting.sePreset.types[Index].bEnable)
                {
                    Text += "有効";
                    Item.Foreground = Brushes.Aqua;
                }
                else
                {
                    Text += "無効";
                    Item.Foreground = (Brush)new BrushConverter().ConvertFromString("#BFFF2C8C");
                }
                Item.Content = Text;
            }
        }

        void UpdateTypeColor()
        {
            int index = SE_Lists.SelectedIndex;
            for (int i = 0; i < seSetting.sePreset.types.Count; i++)
            {
                Change_SE_List(i);
                if (i == SE_Lists.SelectedIndex)
                {
                    if (seSetting.sePreset.types[i].bEnable)
                    {
                        SE_Disable_B.Background = Brushes.Transparent;
                        SE_Disable_B.BorderBrush = Brushes.Aqua;
                        SE_Enable_B.Background = (Brush)bc.ConvertFrom("#59999999");
                        SE_Enable_B.BorderBrush = Brushes.Red;
                    }
                    else
                    {
                        SE_Disable_B.Background = (Brush)bc.ConvertFrom("#59999999");
                        SE_Disable_B.BorderBrush = Brushes.Red;
                        SE_Enable_B.Background = Brushes.Transparent;
                        SE_Enable_B.BorderBrush = Brushes.Aqua;
                    }
                }
            }
            SE_Lists.SelectedIndex = index;
        }

        void Configs_Load()
        {
            loadedPresets.Clear();
            Load_Combo.Items.Clear();
            Load_Combo.Items.Add(seSetting.seDefault.presetName);
            if (File.Exists(Voice_Set.Special_Path + "\\Configs\\SE_Presets.dat"))
            {
                BinaryReader br = new BinaryReader(File.OpenRead(Voice_Set.Special_Path + "\\Configs\\SE_Presets.dat"));
                br.ReadBytes(br.ReadByte());
                br.ReadByte();
                byte presetCount = br.ReadByte();
                br.ReadByte();
                for (int i = 0; i < presetCount; i++)
                {
                    string presetName = Encoding.UTF8.GetString(br.ReadBytes(br.ReadByte()));
                    SE_Preset preset = new SE_Preset(presetName);
                    loadedPresets.Add(preset);
                    Load_Combo.Items.Add(preset.presetName);
                    preset.presetID = br.ReadUInt32();
                    byte typeCount = br.ReadByte();
                    for (int j = 0; j < typeCount; j++)
                    {
                        SE_Type type = new SE_Type();
                        preset.types.Add(type);
                        type.bEnable = br.ReadBoolean();
                        byte itemCount = br.ReadByte();
                        for (int k = 0; k < itemCount; k++)
                        {
                            uint shortID = br.ReadUInt32();
                            byte seCount = br.ReadByte();
                            for (int m = 0; m < seCount; m++)
                            {
                                string filePath = Encoding.UTF8.GetString(br.ReadBytes(br.ReadByte()));
                                type.Add(shortID, filePath);
                            }
                        }
                    }
                    seSetting.SetZeroSounds(preset);
                    br.ReadByte();
                }
                br.Close();
            }
            if (File.Exists(Voice_Set.Special_Path + "/Configs/Save_Configs.conf"))
            {
                BinaryReader br = new BinaryReader(File.OpenRead(Voice_Set.Special_Path + "/Configs/Save_Configs.conf"));
                try
                {
                    br.ReadBytes(br.ReadByte());
                    byte version = br.ReadByte();
                    Volume_Set_C.IsChecked = br.ReadBoolean();
                    DVPL_C.IsChecked = br.ReadBoolean();
                    Default_Voice_Mode_C.IsChecked = br.ReadBoolean();
                    nowPresetIndex = br.ReadInt16();
                    _ = br.ReadByte();
                    Change_Volume_S.Value = br.ReadDouble();
                }
                catch { }
                finally
                {
                    br.Close();
                }
            }

            if (loadedPresets.Count <= nowPresetIndex)
            {
                nowPresetIndex = 0;
                Load_Combo.SelectedIndex = 0;
            }
            else
            {
                Load_Combo.SelectedIndex = nowPresetIndex + 1;
                Preset_Name_T.Text = Load_Combo.Items[Load_Combo.SelectedIndex].ToString();
            }
            if (loadedPresets.Count > 0)
            {
                seSetting.sePreset = loadedPresets[nowPresetIndex].Clone();
                UpdateTypeColor();
            }
            bLoaded = true;
        }
        private void Save_Presets()
        {
            try
            {
                string saveFile = Voice_Set.Special_Path + "\\Configs\\SE_Presets.dat";
                File.Delete(saveFile);
                BinaryWriter bw = new BinaryWriter(File.OpenWrite(saveFile));
                byte[] headerBuf = Encoding.UTF8.GetBytes("WVS-CONF");
                bw.Write((byte)headerBuf.Length);
                bw.Write(headerBuf);
                bw.Write(VERSION_PRESET);
                bw.Write((byte)loadedPresets.Count);
                bw.Write((byte)0x0a);
                foreach (SE_Preset preset in loadedPresets)
                {
                    byte[] name = Encoding.UTF8.GetBytes(preset.presetName);
                    bw.Write((byte)name.Length);
                    bw.Write(name);
                    bw.Write(preset.presetID);
                    bw.Write((byte)preset.types.Count);
                    foreach (SE_Type type in preset.types)
                    {
                        bw.Write(type.bEnable);
                        bw.Write((byte)type.items.Count);
                        foreach (KeyValuePair<uint, List<string>> items in type.items)
                        {
                            bw.Write(items.Key);
                            bw.Write((byte)items.Value.Count);
                            foreach (string item in items.Value)
                            {
                                byte[] nameBuf = Encoding.UTF8.GetBytes(item);
                                bw.Write((byte)nameBuf.Length);
                                bw.Write(nameBuf);
                            }
                        }
                    }
                    bw.Write((byte)0x0a);
                }
                bw.Close();
                Load_Combo.Items.Clear();
                Load_Combo.Items.Add(seSetting.seDefault.presetName);
                for (int i = 0; i < loadedPresets.Count; i++)
                {
                    Load_Combo.Items.Add(loadedPresets[i].presetName);
                    Load_Combo.SelectedIndex = i + 1;
                    nowPresetIndex = i + 1;
                }
            }
            catch
            {
                Message_Feed_Out("プリセット保存中にエラーが発生しました。");
            }
        }

        private void Update_List()
        {
            int selectedIndex = SE_Files.SelectedIndex;
            SE_Files.Items.Clear();
            if (SE_Lists.SelectedIndex == -1)
            {
                return;
            }
            SE_Type type = seSetting.sePreset.types[SE_Lists.SelectedIndex];
            foreach (string file in type.items[0])
            {
                SE_Files.Items.Add(Path.GetFileName(file));
            }
            if (SE_Files.Items.Count > selectedIndex)
            {
                SE_Files.SelectedIndex = selectedIndex;
            }
        }

        private void SE_Lists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (bClosing || SE_Lists.SelectedIndex == -1)
                return;
            //選択したSEの状態によって色を変更
            if (seSetting.sePreset.types[SE_Lists.SelectedIndex].bEnable)
            {
                SE_Disable_B.Background = Brushes.Transparent;
                SE_Disable_B.BorderBrush = Brushes.Aqua;
                SE_Enable_B.Background = (Brush)bc.ConvertFrom("#59999999");
                SE_Enable_B.BorderBrush = Brushes.Red;
            }
            else
            {
                SE_Disable_B.Background = (Brush)bc.ConvertFrom("#59999999");
                SE_Disable_B.BorderBrush = Brushes.Red;
                SE_Enable_B.Background = Brushes.Transparent;
                SE_Enable_B.BorderBrush = Brushes.Aqua;
            }
            SE_Files.Items.Clear();
            if (seSetting.sePreset.types[SE_Lists.SelectedIndex].items.Count > 0)
                foreach (string file in seSetting.sePreset.types[SE_Lists.SelectedIndex].items[0])
                    SE_Files.Items.Add(Path.GetFileName(file));
        }
        private void SE_Play_B_Click(object sender, RoutedEventArgs e)
        {
            if (SE_Lists.SelectedIndex != -1 && SE_Files.SelectedIndex != -1)
            {
                if (bClosing)
                    return;
                string filePath = seSetting.sePreset.types[SE_Lists.SelectedIndex].items[0][SE_Files.SelectedIndex];
                if (!File.Exists(filePath))
                {
                    Message_Feed_Out("サウンドファイルが見つかりませんでした。\n場所:" + filePath);
                    SE_Files.SelectedIndex = -1;
                    return;
                }
                Bass.BASS_ChannelStop(Stream);
                Bass.BASS_StreamFree(Stream);
                int StreamHandle;
                if (Path.GetExtension(filePath) == ".flac")
                    StreamHandle = BassFlac.BASS_FLAC_StreamCreateFile(filePath, 0, 0, BASSFlag.BASS_STREAM_DECODE);
                else
                    StreamHandle = Bass.BASS_StreamCreateFile(filePath, 0, 0, BASSFlag.BASS_STREAM_DECODE);
                Stream = BassFx.BASS_FX_TempoCreate(StreamHandle, BASSFlag.BASS_FX_FREESOURCE);
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, 1f);
                Bass.BASS_ChannelSetDevice(Stream, Video_Mode.Sound_Device);
                Bass.BASS_ChannelPlay(Stream, false);
            }
        }
        private void SE_Stop_B_Click(object sender, RoutedEventArgs e)
        {
            Bass.BASS_ChannelStop(Stream);
            Bass.BASS_StreamFree(Stream);
        }
        private async void Exit_B_Click(object sender, RoutedEventArgs e)
        {
            //閉じる
            if (Opacity >= 1)
            {
                bClosing = true;
                Sub_Code.CreatingProject = false;
                Sub_Code.DVPL_Encode = false;
                Bass.BASS_ChannelStop(Stream);
                Bass.BASS_StreamFree(Stream);
                Configs_Save();
                while (Opacity > 0)
                {
                    Opacity -= Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
                SE_Lists.SelectedIndex = -1;
                Visibility = Visibility.Hidden;
                bClosing = false;
            }
        }
        private void SE_Disable_B_Click(object sender, RoutedEventArgs e)
        {
            //選択しているSEを無効化
            if (SE_Lists.SelectedIndex != -1)
            {
                if (seSetting.sePreset.types[SE_Lists.SelectedIndex].bEnable)
                {
                    int Number = SE_Lists.SelectedIndex;
                    seSetting.sePreset.types[SE_Lists.SelectedIndex].bEnable = false;
                    SE_Disable_B.Background = (Brush)bc.ConvertFrom("#59999999");
                    SE_Disable_B.BorderBrush = Brushes.Red;
                    SE_Enable_B.Background = Brushes.Transparent;
                    SE_Enable_B.BorderBrush = Brushes.Aqua;
                    SE_Lists.SelectedIndex = Number;
                    Change_SE_List(Number);
                }
            }
        }
        private void SE_Enable_B_Click(object sender, RoutedEventArgs e)
        {
            //選択しているSEを有効化
            if (SE_Lists.SelectedIndex != -1)
            {
                if (!seSetting.sePreset.types[SE_Lists.SelectedIndex].bEnable)
                {
                    int Number = SE_Lists.SelectedIndex;
                    seSetting.sePreset.types[SE_Lists.SelectedIndex].bEnable = true;
                    SE_Disable_B.Background = Brushes.Transparent;
                    SE_Disable_B.BorderBrush = Brushes.Aqua;
                    SE_Enable_B.Background = (Brush)bc.ConvertFrom("#59999999");
                    SE_Enable_B.BorderBrush = Brushes.Red;
                    SE_Lists.SelectedIndex = Number;
                    Change_SE_List(Number);
                }
            }
        }
        private async void Save_B_Click(object sender, RoutedEventArgs e)
        {
            if (!bClosing && Opacity >= 1)
            {
                //作成ボタンが押されたら別クラスに情報を送り画面を閉じる
                bClosing = true;
                Sub_Code.CreatingProject = true;
                Sub_Code.VolumeSet = Volume_Set_C.IsChecked.Value;
                Sub_Code.DVPL_Encode = DVPL_C.IsChecked.Value;
                Sub_Code.SetLanguage = Languages[selectedLanguageIndex];
                Sub_Code.Default_Voice = Default_Voice_Mode_C.IsChecked.Value;
                Sub_Code.Only_Wwise_Project = Only_Wwise_C.IsChecked.Value;
                Configs_Save();
                while (Opacity > 0)
                {
                    Opacity -= Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
                bClosing = false;
                Visibility = Visibility.Hidden;
            }
        }
        private void SE_Lists_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //SEの選択を解除
            SE_Disable_B.Background = Brushes.Transparent;
            SE_Disable_B.BorderBrush = Brushes.Aqua;
            SE_Enable_B.Background = Brushes.Transparent;
            SE_Enable_B.BorderBrush = Brushes.Aqua;
            SE_Lists.SelectedIndex = -1;
            SE_Stop_B_Click(null, null);
            SE_Files.Items.Clear();
        }
        private void Language_Left_B_Click(object sender, RoutedEventArgs e)
        {
            if (bClosing || selectedLanguageIndex <= 0)
                return;
            selectedLanguageIndex--;
            Android_T.Text = "言語:" + Languages[selectedLanguageIndex];
        }
        private void Language_Right_B_Click(object sender, RoutedEventArgs e)
        {
            if (bClosing || selectedLanguageIndex >= 17)
                return;
            selectedLanguageIndex++;
            Android_T.Text = "言語:" + Languages[selectedLanguageIndex];
        }
        void Configs_Save()
        {
            try
            {
                if (File.Exists(Voice_Set.Special_Path + "/Configs/Save_Configs.conf"))
                    File.Delete(Voice_Set.Special_Path + "/Configs/Save_Configs.conf");
                BinaryWriter bw = new BinaryWriter(File.OpenWrite(Voice_Set.Special_Path + "/Configs/Save_Configs.conf"));
                string header = "WVS-CONF";
                byte[] headerBuf = Encoding.UTF8.GetBytes(header);
                bw.Write((byte)headerBuf.Length);
                bw.Write(headerBuf);
                bw.Write((byte)VERSION_CONF);
                bw.Write(Volume_Set_C.IsChecked.Value);
                bw.Write(DVPL_C.IsChecked.Value);
                bw.Write(Default_Voice_Mode_C.IsChecked.Value);
                bw.Write((short)nowPresetIndex);
                bw.Write((byte)SE_Setting.SE_COUNT);
                bw.Write(Change_Volume_S.Value);
                bw.Close();
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        private void SE_All_Enable_B_Click(object sender, RoutedEventArgs e)
        {
            int SelectedIndex = SE_Lists.SelectedIndex;
            for (int Number = 0; Number < SE_Lists.Items.Count; Number++)
            {
                seSetting.sePreset.types[Number].bEnable = true;
                SE_Disable_B.Background = Brushes.Transparent;
                SE_Disable_B.BorderBrush = Brushes.Aqua;
                SE_Enable_B.Background = (Brush)bc.ConvertFrom("#59999999");
                SE_Enable_B.BorderBrush = Brushes.Red;
            }
            SE_Lists.SelectedIndex = SelectedIndex;
            Change_SE_List();
        }
        private void SE_All_Disable_B_Click(object sender, RoutedEventArgs e)
        {
            int SelectedIndex = SE_Lists.SelectedIndex;
            for (int Number = 0; Number < SE_Lists.Items.Count; Number++)
            {
                seSetting.sePreset.types[Number].bEnable = false;
                SE_Disable_B.Background = (Brush)bc.ConvertFrom("#59999999");
                SE_Disable_B.BorderBrush = Brushes.Red;
                SE_Enable_B.Background = Brushes.Transparent;
                SE_Enable_B.BorderBrush = Brushes.Aqua;
            }
            SE_Lists.SelectedIndex = SelectedIndex;
            Change_SE_List();
        }
        private void Only_Wwise_C_Click(object sender, RoutedEventArgs e)
        {
            if (Only_Wwise_C.IsChecked.Value)
            {
                MessageBoxResult result = MessageBox.Show("この項目にチェックを入れると、音声Mod(*.bnk)を作成するのではなく、音声Modを作成するためのWwiseのプロジェクトファイルが生成" +
                    "されます。\n続行しますか?", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.No)
                    Only_Wwise_C.IsChecked = false;
            }
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            seSetting = new SE_Setting();
        }
        private void Remove_Preset_B_Click(object sender, RoutedEventArgs e)
        {
            if (Load_Combo.SelectedIndex <= 0)
            {
                Message_Feed_Out("標準プリセットは削除できません。");
                return;
            }
            string removePresetName = loadedPresets[Load_Combo.SelectedIndex - 1].presetName;
            MessageBoxResult result = MessageBox.Show(removePresetName + "を削除しますか?\nこの操作は取り消せません。続行しますか?",
                "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                loadedPresets.RemoveAt(Load_Combo.SelectedIndex - 1);
                seSetting.SetDefault(seSetting.sePreset);
                Load_Combo.Items.RemoveAt(Load_Combo.SelectedIndex);
                Update_List();
                Save_Presets();
                Configs_Save();
                nowPresetIndex = 0;
                Load_Combo.SelectedIndex = 0;
                Message_Feed_Out("'" + removePresetName + "'を削除し、標準をロードしました。");
            }
        }
        private void Load_Preset_B_Click(object sender, RoutedEventArgs e)
        {
            SE_Preset sePreset = Load_Combo.SelectedIndex == 0 ? seSetting.seDefault : loadedPresets[Load_Combo.SelectedIndex - 1];
            MessageBoxResult result = MessageBox.Show(sePreset.presetName + "を読み込みますか?\n保存していないデータは破棄されます。",
                "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                if (sePreset.presetID == seSetting.seDefault.presetID)
                    seSetting.SetDefault(seSetting.sePreset);
                else
                    seSetting.sePreset = sePreset.Clone();
                Preset_Name_T.Text = Load_Combo.Items[Load_Combo.SelectedIndex].ToString();
                Message_Feed_Out("'" + seSetting.sePreset.presetName + "'をロードしました。");
                UpdateTypeColor();
                Update_List();
                Configs_Save();
            }
        }
        private void Save_Preset_B_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(Preset_Name_T.Text))
            {
                Message_Feed_Out("プリセット名が入力されていません。");
                return;
            }
            uint shortID = WwiseHash.HashString(Preset_Name_T.Text);
            if (seSetting.seDefault.presetID == shortID)
            {
                Message_Feed_Out("標準プリセットは編集できません。");
                return;
            }

            bool bExist = false;
            foreach (SE_Preset loadedPreset in loadedPresets)
            {
                if (loadedPreset.presetName == Preset_Name_T.Text)
                {
                    MessageBoxResult result = MessageBox.Show("同名のプリセットが存在します。上書きしますか?", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes);
                    if (result == MessageBoxResult.Yes)
                    {
                        loadedPreset.types.Clear();
                        foreach (SE_Type type in seSetting.sePreset.types)
                            loadedPreset.types.Add(type.Clone());
                        bExist = true;
                        break;
                    }
                    else
                        return;
                }
            }
            if (!bExist)
            {
                seSetting.sePreset.presetName = Preset_Name_T.Text;
                seSetting.sePreset.presetID = WwiseHash.HashString(Preset_Name_T.Text);
                SE_Preset preset = seSetting.sePreset.Clone();
                loadedPresets.Add(preset);
            }

            Save_Presets();

            MessageBox.Show("プリセットを保存しました。");
        }
        private void SE_Add_B_Click(object sender, RoutedEventArgs e)
        {
            if (SE_Lists.SelectedIndex == -1)
            {
                Message_Feed_Out("SEイベントが選択されていません。");
                return;
            }
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog()
            {
                Title = "サウンドファイルを選択してください。",
                Multiselect = true,
                Filter = "サウンドファイル(*.aac;*.flac;*.m4a;*.mp3;*.mp4;*.ogg;*.wav;*.wma)|*.aac;*.flac;*.m4a;*.mp3;*.mp4;*.ogg;*.wav;*.wma"
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                int count = 0;
                foreach (string selectFile in ofd.FileNames)
                {
                    bool bEnable = true;
                    foreach (KeyValuePair<uint, List<string>> already in seSetting.sePreset.types[SE_Lists.SelectedIndex].items)
                    {
                        foreach (string item in already.Value)
                        {
                            if (item == selectFile)
                            {
                                bEnable = false;
                                break;
                            }
                        }
                        if (!bEnable)
                            break;
                    }
                    if (!bEnable)
                        continue;
                    seSetting.sePreset.types[SE_Lists.SelectedIndex].Add(0, selectFile);
                    count++;
                }
                if (count == 0)
                    Message_Feed_Out("選択したファイルは既に追加されています。");
                else
                    Message_Feed_Out(count + "個のファイルを追加しました。");
                Update_List();
            }
        }
        private void SE_Delete_B_Click(object sender, RoutedEventArgs e)
        {
            if (SE_Lists.SelectedIndex == -1 || SE_Files.SelectedIndex == -1)
            {
                Message_Feed_Out("削除するサウンドファイルを選択してください。");
                return;
            }
            try
            {
                seSetting.sePreset.types[SE_Lists.SelectedIndex].items[0].RemoveAt(SE_Files.SelectedIndex);
                Update_List();
            }
            catch (Exception e1)
            {
                Message_Feed_Out("エラーが発生しました。");
                Sub_Code.Error_Log_Write(e1.Message);
            }
        }

        private void SE_File_Help_B_Click(object sender, RoutedEventArgs e)
        {
            Message_Feed_Out("ここに追加したサウンドがランダムで再生されます。");
        }

        private async void Change_Volume_B_Click(object sender, RoutedEventArgs e)
        {
            if (bClosing)
                return;

            Message_T.Text = "音量を変更しています...";
            Message_T.Opacity = 1.0;
            await Task.Delay(50);

            List<string> voiceFile = new List<string>();
            foreach (Voice_Event_Setting setting in settings)
                foreach (Voice_Sound_Setting soundSetting in setting.Sounds)
                    voiceFile.Add(soundSetting.File_Path);
            if (voiceFile.Count == 0)
            {
                Message_Feed_Out("1ページ目に音声ファイルが存在しないためテストできません。");
                return;
            }
            string testFile = voiceFile[Sub_Code.r.Next(voiceFile.Count)];

            if (!File.Exists(testFile))
            {
                Message_Feed_Out("ファイル:" + Path.GetFileName(testFile) + "が存在しません。");
                return;
            }

            voiceFile.Clear();

            string toFile = Path.GetTempPath() + "WVS_Temp_02.tmp";

            Bass.BASS_ChannelStop(Stream);
            Bass.BASS_StreamFree(Stream);

            if (Sub_Code.Audio_IsWAV(testFile))
                File.Copy(testFile, toFile, true);
            else if (!Sub_Code.Audio_Encode_To_Other(testFile, toFile, "wav", false))
            {
                Message_Feed_Out(".wavへ変換できませんでした。");
                return;
            }

            Sub_Code.Set_WAV_Gain(toFile, Change_Volume_S.Value - 89.0);

            int StreamHandle;
            if (Path.GetExtension(toFile) == ".flac")
                StreamHandle = BassFlac.BASS_FLAC_StreamCreateFile(toFile, 0, 0, BASSFlag.BASS_STREAM_DECODE);
            else
                StreamHandle = Bass.BASS_StreamCreateFile(toFile, 0, 0, BASSFlag.BASS_STREAM_DECODE);
            Stream = BassFx.BASS_FX_TempoCreate(StreamHandle, BASSFlag.BASS_FX_FREESOURCE);
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, 1f);
            Bass.BASS_ChannelSetDevice(Stream, Video_Mode.Sound_Device);
            Bass.BASS_ChannelPlay(Stream, false);

            Message_T.Text = "";
        }

        private void Change_Volume_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double Value = Math.Round(Change_Volume_S.Value, 1, MidpointRounding.AwayFromZero);
            Change_Volume_T.Text = "音量:" + Value + "db";
        }

        void ReplaceSEDir()
        {
            if (bClosing || bOpenedChangeDir)
                return;
            bOpenedChangeDir = true;
            BetterFolderBrowser bfb = new BetterFolderBrowser()
            {
                Title = "SEの移動先のフォルダを選択してください。",
                Multiselect = false,
                RootFolder = Sub_Code.Get_OpenDirectory_Path()
            };
            if (bfb.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Sub_Code.Set_Directory_Path(bfb.SelectedFolder);
                if (!Sub_Code.CanDirectoryAccess(bfb.SelectedFolder))
                {
                    bfb.Dispose();
                    bOpenedChangeDir = false;
                    return;
                }
                bool bSaveMode = false;
                int replaceCount = 0;
                foreach (SE_Type seList in seSetting.sePreset.types)
                {
                    if (seList.items.ContainsKey(0))
                    {
                        for (int i = 0; i < seList.items[0].Count; i++)
                        {
                            string fromFilePath = seList.items[0][i];
                            string toFilePath = bfb.SelectedFolder + "\\" + Path.GetFileName(fromFilePath);
                            if (!File.Exists(fromFilePath) && File.Exists(toFilePath))
                            {
                                seList.items[0][i] = toFilePath;
                                bSaveMode = true;
                                replaceCount++;
                            }
                        }
                    }
                }
                if (bSaveMode)
                {
                    Message_Feed_Out(replaceCount + "個のファイルパスを指定したフォルダに変換しました。\n変更を保存してください。");
                }
            }
            bfb.Dispose();
            bOpenedChangeDir = false;
        }
    }
}