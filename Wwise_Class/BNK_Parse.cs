using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WoTB_Voice_Mod_Creater.Class;

namespace WoTB_Voice_Mod_Creater.Wwise_Class
{
    public class BNK_Parse
    {
        public enum Container_Name
        {
            CAkBus,
            CAkEvent,
            CAkRanSeqCntr,
            CAkSwitchCntr,
            CAkSound,
            CAkLayerCntr,
            CAkActorMixer,
            None
        }
        public class Parse_ID_Line
        {
            public Container_Name Cntr = Container_Name.None;
            public uint Parent_ID = 0;
            public uint ID = 0;
            public int Line = 0;
        }
        //解析した内容を行に分けてすべて記録
        List<string> Read_All = new List<string>();
        //↑の内容から、イベントID、そのIDの行、イベント形式(RandomコンテナやSwitchコンテナなど)のみを抽出
        List<Parse_ID_Line> ID_Line = new List<Parse_ID_Line>();
        //IsSpecialBNKFileModeがtrueの場合に使用する
        List<Parse_ID_Line> ID_Line_Special = new List<Parse_ID_Line>();
        //ファイルが正しくない場合falseにする
        bool IsSelected = false;
        //特殊な.bnkファイルの場合1または2にします
        public int SpecialBNKFileMode = 0;
        List<List<uint>> WoT_Event_ID = new List<List<uint>>();
        List<uint> WoTB_Old_Gun_ID = new List<uint>();
        public BNK_Parse(string BNK_File)
        {
            if (!File.Exists(BNK_File))
            {
                IsSelected = false;
                return;
            }
            //bnkファイルを解析(Wwiserを使用)
            string Temp_Name = Sub_Code.Get_Time_Now(DateTime.Now, "-", 1, 6);
            StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Wwise_Parse/BNK_Parse_Start.bat");
            stw.WriteLine("chcp 65001");
            stw.Write("\"" + Voice_Set.Special_Path + "/Wwise_Parse/Python/python.exe\" \"" + Voice_Set.Special_Path + "/Wwise_Parse/wwiser.pyz\" -iv \"" + BNK_File + "\" -dn \"" +
                Voice_Set.Special_Path + "/Wwise_Parse/" + Temp_Name + "\"");
            stw.Close();
            ProcessStartInfo processStartInfo1 = new ProcessStartInfo
            {
                FileName = Voice_Set.Special_Path + "/Wwise_Parse/BNK_Parse_Start.bat",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process p = Process.Start(processStartInfo1);
            p.WaitForExit();
            File.Delete(Voice_Set.Special_Path + "/Wwise_Parse/BNK_Parse_Start.bat");
            //解析に失敗していたら終了
            if (!File.Exists(Voice_Set.Special_Path + "/Wwise_Parse/" + Temp_Name + ".xml"))
            {
                IsSelected = false;
                return;
            }
            string line;
            //内容をListに追加
            StreamReader file = new StreamReader(Voice_Set.Special_Path + "/Wwise_Parse/" + Temp_Name + ".xml");
            while ((line = file.ReadLine()) != null)
            {
                Read_All.Add(line);
                //この文字が含まれていたらイベントやコンテナ確定
                if (line.Contains("type=\"sid\" name=\"ulID\" value=\""))
                {
                    //イベントIDを取得
                    string strValue = line.Substring(line.IndexOf("value=\"") + 7);
                    strValue = strValue.Substring(0, strValue.IndexOf("\""));
                    //イベントの内容を取得(CASoundやCAkRanSeqCntrなど)
                    string strValue2 = Read_All[Read_All.Count - 4].Substring(Read_All[Read_All.Count - 4].IndexOf("name=\"") + 6);
                    strValue2 = strValue2.Substring(0, strValue2.IndexOf('"'));
                    Parse_ID_Line Parse_Line = new Parse_ID_Line();
                    Parse_Line.ID = uint.Parse(strValue);
                    Parse_Line.Line = Read_All.Count - 1;
                    Container_Name PC;
                    if (!Enum.TryParse<Container_Name>(strValue2, out PC))
                        PC = Container_Name.None;
                    Parse_Line.Cntr = PC;
                    if (strValue2 == "CAkRanSeqCntr" || strValue2 == "CAkSwitchCntr" || strValue2 == "CAkSound" || strValue2 == "CAkLayerCntr" || strValue2 == "CAkActorMixer")
                    {
                        while ((line = file.ReadLine()) != null)
                        {
                            Read_All.Add(line);
                            if (line.Contains("name=\"DirectParentID\""))
                            {
                                string strValue3 = line.Remove(0, line.IndexOf("value=\"") + 7);
                                strValue3 = strValue3.Remove(strValue3.IndexOf("\""));
                                Parse_ID_Line Parse_Line_Special = new Parse_ID_Line();
                                Parse_Line_Special.ID = uint.Parse(strValue3);
                                Parse_Line.Parent_ID = Parse_Line_Special.ID;
                                Parse_Line_Special.Line = Parse_Line.Line;
                                Parse_Line_Special.Cntr = PC;
                                ID_Line_Special.Add(Parse_Line_Special);
                                break;
                            }
                        }
                    }
                    else
                        ID_Line_Special.Add(new Parse_ID_Line());
                    ID_Line.Add(Parse_Line);
                }
            }
            file.Close();
            file.Dispose();
            //生成されたxmlファイルは使用しないので消しておく
            File.Delete(Voice_Set.Special_Path + "/Wwise_Parse/" + Temp_Name + ".xml");
            //通常のイベントIDを挿入
            for (int Number = 0; Number < 40; Number++)
                WoT_Event_ID.Add(new List<uint>());
            Wot_Event_IDs_Clear();
            IsSelected = true;
        }
        void Wot_Event_IDs_Clear()
        {
            for (int Number_IDs = 0; Number_IDs < WoT_Event_ID.Count; Number_IDs++)
                WoT_Event_ID[Number_IDs].Clear();
            WoT_Event_ID[0].Add(2301863335);
            WoT_Event_ID[1].Add(3166122996);
            WoT_Event_ID[2].Add(1332081395);
            WoT_Event_ID[3].Add(1057290642);
            WoT_Event_ID[3].Add(4209631318);
            WoT_Event_ID[4].Add(102434383);
            WoT_Event_ID[4].Add(136522141);
            WoT_Event_ID[4].Add(3406769);
            WoT_Event_ID[4].Add(3175172091);
            WoT_Event_ID[4].Add(730352367);
            WoT_Event_ID[4].Add(1713814001);
            WoT_Event_ID[5].Add(2655350512);
            WoT_Event_ID[6].Add(1946435397);
            WoT_Event_ID[7].Add(664892591);
            WoT_Event_ID[8].Add(3563950026);
            WoT_Event_ID[9].Add(1122640859);
            WoT_Event_ID[10].Add(1702267773);
            WoT_Event_ID[11].Add(1302422561);
            WoT_Event_ID[12].Add(4148607935);
            WoT_Event_ID[13].Add(1396083645);
            WoT_Event_ID[14].Add(3616464105);
            WoT_Event_ID[15].Add(1641781602);
            WoT_Event_ID[16].Add(693799259);
            WoT_Event_ID[17].Add(825236543);
            WoT_Event_ID[18].Add(4011208105);
            WoT_Event_ID[19].Add(1107412350);
            WoT_Event_ID[20].Add(1781671118);
            WoT_Event_ID[21].Add(575170028);
            WoT_Event_ID[22].Add(3928775652);
            WoT_Event_ID[23].Add(590598450);
            WoT_Event_ID[24].Add(2775977275);
            WoT_Event_ID[24].Add(1077949222);
            WoT_Event_ID[25].Add(1112395295);
            WoT_Event_ID[26].Add(998311689);
            WoT_Event_ID[27].Add(766987452);
            WoT_Event_ID[28].Add(1244332268);
            WoT_Event_ID[29].Add(3498324732);
            WoT_Event_ID[29].Add(311966861);
            WoT_Event_ID[30].Add(47755571);
            WoT_Event_ID[31].Add(1630212423);
            WoT_Event_ID[32].Add(257621937);
            WoT_Event_ID[33].Add(3917054405);
            WoT_Event_ID[34].Add(4072375507);
            WoT_Event_ID[35].Add(3034227203);
            WoT_Event_ID[35].Add(79416786);
            WoT_Event_ID[36].Add(1491984609);
            WoT_Event_ID[37].Add(85617264);
            WoT_Event_ID[38].Add(790147034);
            WoT_Event_ID[39].Add(2594173436);
        }
        //bnkファイル内のCAkSoundの数を取得
        public int Get_File_Count()
        {
            int Count = 0;
            for (int Number = 0; Number < ID_Line.Count; Number++)
            {
                if (ID_Line[Number].Cntr == Container_Name.CAkSound)
                    Count++;
            }
            return Count;
        }
        //bnk内にMaster Audio Busが含まれているか
        public bool IsInitBNK()
        {
            for (int Number = 0; Number < ID_Line.Count; Number++)
            {
                if (ID_Line[Number].Cntr == Container_Name.CAkBus)
                    return true;
            }
            return false;
        }
        //選択されているbnkファイルが音声データかを調べる
        public bool IsVoiceFile(bool IsBlitzMode = false)
        {
            uint Battle_Short_ID;
            bool IsExist = false;
            if (!IsSelected)
                return IsExist;
            if (IsBlitzMode)
                Battle_Short_ID = 1419869192;
            else
                Battle_Short_ID = WoT_Event_ID[23][0];
            foreach (Parse_ID_Line ID_Now in ID_Line)
            {
                //戦闘開始のイベントがあるかないかで判定
                if (ID_Now.ID == Battle_Short_ID)
                {
                    IsExist = true;
                    break;
                }
            }
            return IsExist;
        }
        //イベントIDをリストとして取得
        public List<uint> Get_BNK_Event_ID()
        {
            if (!IsSelected)
                return new List<uint>();
            //イベントIDのみを抽出
            List<uint> GetEventsID = new List<uint>();
            foreach (Parse_ID_Line List_Now in ID_Line)
                if (List_Now.Cntr == Container_Name.CAkEvent)
                    GetEventsID.Add(List_Now.ID);
            return GetEventsID;
        }
        public List<string> Get_BNK_Event_ID_To_String()
        {
            if (!IsSelected)
                return new List<string>();
            //イベントIDのみを抽出
            List<string> GetEventsID = new List<string>();
            foreach (Parse_ID_Line List_Now in ID_Line)
                if (List_Now.Cntr == Container_Name.CAkEvent)
                    GetEventsID.Add(List_Now.ID.ToString());
            return GetEventsID;
        }
        //指定したイベントIDのサウンドをリストとして取得
        public List<uint> Get_Sounds_From_EventID(uint EventID)
        {
            List<uint> Sounds = new List<uint>();
            foreach (string ID in Get_Event_Voices(EventID))
                Sounds.Add(uint.Parse(ID));
            return Sounds;
        }
        //データからどの音声ファイルが戦闘中のどこで再生されるかを取得(貫通時や火災時など)
        //引数:BlitzからPC版WoTに移植する際はtrue
        public List<List<string>> Get_Voices(bool IsBlitzToWoT)
        {
            if (!IsSelected)
                return new List<List<string>>();
            //空のリストを作成
            List<List<string>> Voices_Temp = new List<List<string>>();
            for (int Number = 0; Number <= 49; Number++)
            {
                List<string> Temp = new List<string>();
                Voices_Temp.Add(Temp);
            }
            //イベントIDのみを抽出
            List<uint> GetEventsID = new List<uint>();
            foreach (Parse_ID_Line List_Now in ID_Line)
                if (List_Now.Cntr == Container_Name.CAkEvent)
                    GetEventsID.Add(List_Now.ID);
            //イベントに入っている音声をすべて取得(Switchがある場合どちらも取得してしまうため注意)
            for (int Number_01 = 0; Number_01 < GetEventsID.Count; Number_01++)
            {
                int Event_Number = Get_Voice_Type_Number(GetEventsID[Number_01], IsBlitzToWoT);
                if (Event_Number == -1)
                    continue;
                Voices_Temp[Event_Number] = Get_Event_Voices(GetEventsID[Number_01]);
            }
            return Voices_Temp;
        }
        public void Get_Voices(List<Voice_Event_Setting> Settings)
        {
            if (!IsSelected)
                return;
            Settings.Clear();
            //空のリストを作成
            for (int Number = 0; Number <= 49; Number++)
                Settings.Add(new Voice_Event_Setting());
            Sub_Code.Set_Event_ShortID(Settings);
            //イベントIDのみを抽出
            List<uint> GetEventsID = new List<uint>();
            foreach (Parse_ID_Line List_Now in ID_Line)
                if (List_Now.Cntr == Container_Name.CAkEvent)
                    GetEventsID.Add(List_Now.ID);
            //イベントに入っている音声をすべて取得(Switchがある場合どちらも取得してしまうため注意)
            for (int Number_01 = 0; Number_01 < GetEventsID.Count; Number_01++)
            {
                int Event_Number = Get_Voice_Type_Number(GetEventsID[Number_01], false);
                if (Event_Number == -1)
                    continue;
                List<string> Voices = Get_Event_Voices(GetEventsID[Number_01]);
                foreach (string Voice in Voices)
                    Settings[Event_Number].Sounds.Add(new Voice_Sound_Setting(Voice));
            }
        }
        public void Clear()
        {
            IsSelected = false;
            Read_All.Clear();
            ID_Line.Clear();
            ID_Line_Special.Clear();
            WoT_Event_ID.Clear();
        }
        //イベントからアクションIDを取得して音声ファイルを取得
        List<string> Get_Event_Voices(uint Event_ID)
        {
            int Number_01 = -1;
            //指定されたイベントIDがID_Lineのどこに入っているか調べる
            for (int Number = 0; Number < ID_Line.Count; Number++)
                if (Event_ID == ID_Line[Number].ID)
                    Number_01 = Number;
            if (Number_01 == -1)
                return new List<string>();
            //行を取得
            int Number_02 = ID_Line[Number_01].Line;
            //子コンテナが何個あるか確認
            string Index = Read_All[Number_02 + 3].Remove(0, Read_All[Number_02 + 3].IndexOf("count=\"") + 7);
            Index = Index.Remove(Index.IndexOf("\""));
            int Action_Count = int.Parse(Index);
            int Last_Index = -1;
            List<string> Child_SourceIDs = new List<string>();
            //子コンテナがあるだけループ
            for (int Number = 0; Number < Action_Count; Number++)
            {
                Last_Index += 3;
                //子コンテナのIDを取得
                string ActionID = Read_All[Number_02 + 3 + Last_Index].Remove(0, Read_All[Number_02 + 3 + Last_Index].IndexOf("value=\"") + 7);
                string ActionID_String = ActionID.Remove(ActionID.IndexOf("\""));
                uint Child_ID = uint.Parse(ActionID_String);
                //子コンテナの内容をChild_SourceIDsに追加
                foreach (uint ID_Now in Action_Children(Child_ID))
                    Child_SourceIDs.Add(ID_Now.ToString());
            }
            return Child_SourceIDs;
        }
        //アクションに入っているイベントをすべて取得し、そのIDから音声を取得
        List<uint> Action_Children(uint Action_ID)
        {
            int Number_01 = -1;
            for (int Number = 0; Number < ID_Line.Count; Number++)
                if (Action_ID == ID_Line[Number].ID)
                    Number_01 = Number;
            if (Number_01 == -1)
                return new List<uint>();
            int Number_02 = ID_Line[Number_01].Line;
            //Playイベントではない場合飛ばす(0x0403がPlayイベント)
            if (!Read_All[Number_02 + 1].Contains("0x0403"))
                return new List<uint>();
            //子コンテナの数を取得
            string Index = Read_All[Number_02 + 3].Remove(0, Read_All[Number_02 + 3].IndexOf("value=\"") + 7);
            Index = Index.Remove(Index.IndexOf("\""));
            uint Child_ID = uint.Parse(Index);
            if (Sub_Code.ShortIDs.Contains(Child_ID) || Child_ID == 649358221)
                return new List<uint>();
            if (SpecialBNKFileMode == 1)
                return Children_Sort_Special(Child_ID);
            else
                return Children_Sort(Child_ID);
        }
        //CAkSoundの階層に到達するまで繰り返す
        //CAkSoundに到達したらSourceIDを取得して戻り値にリストとして返す
        List<uint> Children_Sort(uint Child_ID)
        {
            int Number_01 = -1;
            for (int Number = 0; Number < ID_Line.Count; Number++)
                if (Child_ID == ID_Line[Number].ID)
                    Number_01 = Number;
            if (Number_01 == -1)
                return new List<uint>();
            int Start_Line = ID_Line[Number_01].Line;
            //CAkSoundの場合それ以上階層がないためSourceIDを取得して終わる
            if (ID_Line[Number_01].Cntr == Container_Name.CAkSound)
            {
                int End_Line = -1;
                //sourceIDの文字がある部分までループ
                while (Start_Line < Read_All.Count)
                {
                    Start_Line++;
                    if (Read_All[Start_Line].Contains("sourceID"))
                    {
                        End_Line = Start_Line;
                        break;
                    }
                }
                if (End_Line == -1)
                    return new List<uint>();
                //音声ファイルのIDを取得
                string Index = Read_All[End_Line].Remove(0, Read_All[End_Line].IndexOf("value=\"") + 7);
                uint Index_UINT = uint.Parse(Index.Remove(Index.IndexOf("\"")));
                List<uint> Temp = new List<uint>();
                Temp.Add(Index_UINT);
                return Temp;
            }
            //CAkSound以外の場合はまだ下に階層があるためなくなるまで続ける
            else
            {
                int End_Line = -1;
                int Object_Count = 0;
                //子コンテナがある行を取得
                while (Start_Line < Read_All.Count)
                {
                    Start_Line++;
                    //Childrenの項目がない場合最後の行まで検索してしまうため</object>が3つ続いたらループを終了
                    if (Read_All[Start_Line].Contains("</object>"))
                        Object_Count++;
                    else
                        Object_Count = 0;
                    if (Object_Count >= 3)
                        break;
                    if (Read_All[Start_Line].Contains("Children"))
                    {
                        End_Line = Start_Line + 1;
                        break;
                    }
                }
                if (End_Line == -1)
                    return new List<uint>();
                List<uint> Child_Source_IDs = new List<uint>();
                //階層の数だけこの関数を実行
                int Line_Count = 0;
                while (true)
                {
                    Line_Count++;
                    if (Read_All[End_Line + Line_Count].Contains("</object>"))
                        break;
                    //子コンテナのIDを取得してChildren_Sortを実行
                    string Index2 = Read_All[End_Line + Line_Count].Remove(0, Read_All[End_Line + Line_Count].IndexOf("value=\"") + 7);
                    Index2 = Index2.Remove(Index2.IndexOf("\""));
                    uint Child_ID_2 = uint.Parse(Index2);
                    foreach (uint IDs in Children_Sort(Child_ID_2))
                        Child_Source_IDs.Add(IDs);
                }
                return Child_Source_IDs;
            }
        }
        //CAkSoundの階層に到達するまで繰り返す
        //CAkSoundに行ったらSourceIDを取得して戻り値にリストとして返す
        //Childrenから取得するのではなく、DirectParentIDが一致するファイルから取得
        //一部のbnkファイルは上の形式が使用できないためこちらを使用
        List<uint> Children_Sort_Special(uint Child_ID)
        {
            int Number_01 = -1;
            for (int Number = 0; Number < ID_Line.Count; Number++)
                if (Child_ID == ID_Line[Number].ID)
                    Number_01 = Number;
            if (Number_01 == -1)
                return new List<uint>();
            int Start_Line = ID_Line[Number_01].Line;
            //CAkSoundの場合それ以上階層がないためSourceIDを取得して終わる
            if (ID_Line[Number_01].Cntr == Container_Name.CAkSound)
            {
                int End_Line = -1;
                //sourceIDの文字がある部分までループ
                while (Start_Line < Read_All.Count)
                {
                    Start_Line++;
                    if (Read_All[Start_Line].Contains("sourceID"))
                    {
                        End_Line = Start_Line;
                        break;
                    }
                }
                if (End_Line == -1)
                    return new List<uint>();
                //音声ファイルのIDを取得
                string Index = Read_All[End_Line].Remove(0, Read_All[End_Line].IndexOf("value=\"") + 7);
                uint Index_UINT = uint.Parse(Index.Remove(Index.IndexOf("\"")));
                List<uint> Temp = new List<uint>();
                Temp.Add(Index_UINT);
                return Temp;
            }
            else
            {
                List<uint> Temp = new List<uint>();
                for (int Index = 0; Index < ID_Line_Special.Count; Index++)
                {
                    if (ID_Line_Special[Index].ID == Child_ID)
                        foreach (uint IDs in Children_Sort_Special(ID_Line[Index].ID))
                            Temp.Add(IDs);
                }
                return Temp;
            }
        }
        public List<List<uint>> Get_Event_ID()
        {
            return WoT_Event_ID;
        }
        public List<uint> Get_Sound_Random_Cntr(uint Random_Cntr_ShortID)
        {
            if (SpecialBNKFileMode == 1)
                return Children_Sort_Special(Random_Cntr_ShortID);
            else
                return Children_Sort(Random_Cntr_ShortID);
        }
        //audio_mods.xmlからイベントIDを取得
        public bool Get_Event_ID_From_XML(string audio_mods_file)
        {
            if (!File.Exists(audio_mods_file))
                return false;
            try
            {
                //大杉ィ
                string[] WoT_Events = { "vo_start_battle", "vo_vehicle_destroyed", "vo_enemy_killed_by_player", "vo_fire_started", "vo_fuel_tank_damaged", "vo_target_captured", "vo_driver_killed"
                , "vo_ammo_bay_damaged", "vo_engine_damaged", "vo_enemy_hp_damaged_by_explosion_at_direct_hit_by_player", "vo_armor_not_pierced_by_player", "vo_track_destroyed", "vo_track_functional"
                , "vo_track_damaged", "vo_track_functional_can_move", "vo_target_lost", "minimap_attention", "enemy_sighted_for_team", "vo_fire_stopped", "vo_gun_damaged", "vo_target_unlocked"
                , "vo_surveying_devices_damaged", "vo_turret_rotator_destroyed", "vo_radio_damaged", "vo_engine_destroyed", "vo_enemy_fire_started_by_player", "vo_ally_killed_by_player"
                , "vo_surveying_devices_destroyed", "vo_commander_killed", "vo_gunner_killed", "vo_loader_killed", "vo_radioman_killed", "vo_turret_rotator_damaged", "vo_gun_destroyed"
                , "vo_armor_ricochet_by_player", "vo_enemy_hp_damaged_by_projectile_by_player", "vo_enemy_no_hp_damage_at_no_attempt_by_player", "vo_engine_functional", "vo_gun_functional"
                , "vo_surveying_devices_functional", "vo_turret_rotator_functional", "vo_enemy_hp_damaged_by_projectile_and_chassis_damaged_by_player", "vo_enemy_hp_damaged_by_projectile_and_gun_damaged_by_player"
                , "vo_enemy_no_hp_damage_at_attempt_and_chassis_damaged_by_player","vo_enemy_no_hp_damage_at_attempt_and_gun_damaged_by_player","vo_enemy_no_hp_damage_at_no_attempt_and_chassis_damaged_by_player"};
                //上の各項目がどの音声タイプか(詳しくはVoice_Set.csを参照)
                int[] WoT_Events_Number = {23,33,9,13,15,36,7
                ,1,10,3,2,28,29
                ,27,29,37,39,34,14,16,37
                ,24,31,21,11,8,0
                ,25,6,19,20,22,30,17
                ,5,3,2,12,18
                ,26,32,4,4
                ,4,4,4};
                List<string> All_Lines = new List<string>();
                All_Lines.AddRange(File.ReadAllLines(audio_mods_file));
                //WoT_Event_IDの内容をクリア
                Wot_Event_IDs_Clear();
                List<bool> IsAdd_Event_Number = new List<bool>();
                for (int Number_Temp = 0; Number_Temp < 40; Number_Temp++)
                    IsAdd_Event_Number.Add(false);
                //xmlファイルを1行ずつ参照
                for (int Line_Number = 0; Line_Number < All_Lines.Count; Line_Number++)
                {
                    try
                    {
                        //イベント名が書かれている行であれば続行
                        if (!All_Lines[Line_Number].Contains("<name>"))
                            continue;
                        //標準のイベント名を取得
                        string After_Event_Name_01 = All_Lines[Line_Number].Substring(All_Lines[Line_Number].IndexOf("<name>") + 6);
                        string After_Event_Name_02 = After_Event_Name_01.Substring(0, After_Event_Name_01.IndexOf("</name>"));
                        for (int Number = 0; Number < WoT_Events.Length; Number++)
                        {
                            //WoT_Eventsに一致するイベント名があれば続行
                            if (WoT_Events[Number] == After_Event_Name_02)
                            {
                                if (!IsAdd_Event_Number[WoT_Events_Number[Number]])
                                {
                                    WoT_Event_ID[WoT_Events_Number[Number]].Clear();
                                    IsAdd_Event_Number[WoT_Events_Number[Number]] = true;
                                }
                                //変IsAdd_Event_Number更後のイベント名を取得
                                string After_Mod_Event_Name_01 = All_Lines[Line_Number + 1].Substring(All_Lines[Line_Number + 1].IndexOf("<mod>") + 5);
                                string After_Mod_Event_Name_02 = After_Mod_Event_Name_01.Substring(0, After_Mod_Event_Name_01.IndexOf("</mod>"));
                                //そのままの名前ではイベント内に入っている音声を取得できないため、変更後のイベント名をハッシュ値に変更(uint)
                                uint Get_Mod_Event_ID = WwiseHash.HashString(After_Mod_Event_Name_02);
                                WoT_Event_ID[WoT_Events_Number[Number]].Add(Get_Mod_Event_ID);
                            }
                        }
                    }
                    catch
                    {
                        //例外処理なし
                    }
                }
                All_Lines.Clear();
                IsAdd_Event_Number.Clear();
                //イベントIDが含まれていたらtrueを返す
                int Voice_Event_Count = 0;
                for (int Number_IDs = 0; Number_IDs < WoT_Event_ID.Count; Number_IDs++)
                    Voice_Event_Count += WoT_Event_ID[Number_IDs].Count;
                if (Voice_Event_Count > 0)
                    return true;
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
            Wot_Event_IDs_Clear();
            return false;
        }
        //イベントIDを初期に戻す
        public void Set_Event_ID_Init()
        {
            Wot_Event_IDs_Clear();
        }
        //イベントIDからインデックスを取得("戻り値が0のときはフレインドリーファイヤ－"などはVoice_Set.csに定義しています)
        int Get_Voice_Type_Number(uint ID, bool IsBlitzToWoT)
        {
            if (!IsBlitzToWoT)
            {
                if (WoT_Event_ID[0].Contains(ID))
                    return 0;
                else if (WoT_Event_ID[1].Contains(ID))
                    return 1;
                else if (WoT_Event_ID[2].Contains(ID))
                    return 2;
                else if (WoT_Event_ID[3].Contains(ID))
                    return 3;
                else if (WoT_Event_ID[4].Contains(ID))
                    return 4;
                else if (WoT_Event_ID[5].Contains(ID))
                    return 5;
                else if (WoT_Event_ID[6].Contains(ID))
                    return 6;
                else if (WoT_Event_ID[7].Contains(ID))
                    return 7;
                else if (WoT_Event_ID[8].Contains(ID))
                    return 8;
                else if (WoT_Event_ID[9].Contains(ID))
                    return 9;
                else if (WoT_Event_ID[10].Contains(ID))
                    return 10;
                else if (WoT_Event_ID[11].Contains(ID))
                    return 11;
                else if (WoT_Event_ID[12].Contains(ID))
                    return 12;
                else if (WoT_Event_ID[13].Contains(ID))
                    return 13;
                else if (WoT_Event_ID[14].Contains(ID))
                    return 14;
                else if (WoT_Event_ID[15].Contains(ID))
                    return 15;
                else if (WoT_Event_ID[16].Contains(ID))
                    return 16;
                else if (WoT_Event_ID[17].Contains(ID))
                    return 17;
                else if (WoT_Event_ID[18].Contains(ID))
                    return 18;
                else if (WoT_Event_ID[19].Contains(ID))
                    return 19;
                else if (WoT_Event_ID[20].Contains(ID))
                    return 20;
                else if (WoT_Event_ID[21].Contains(ID))
                    return 21;
                else if (WoT_Event_ID[22].Contains(ID))
                    return 22;
                else if (WoT_Event_ID[23].Contains(ID))
                    return 23;
                else if (WoT_Event_ID[24].Contains(ID))
                    return 24;
                else if (WoT_Event_ID[25].Contains(ID))
                    return 25;
                else if (WoT_Event_ID[26].Contains(ID))
                    return 26;
                else if (WoT_Event_ID[27].Contains(ID))
                    return 27;
                else if (WoT_Event_ID[28].Contains(ID))
                    return 28;
                else if (WoT_Event_ID[29].Contains(ID))
                    return 29;
                else if (WoT_Event_ID[30].Contains(ID))
                    return 30;
                else if (WoT_Event_ID[31].Contains(ID))
                    return 31;
                else if (WoT_Event_ID[32].Contains(ID))
                    return 32;
                else if (WoT_Event_ID[33].Contains(ID))
                    return 33;
                else if (WoT_Event_ID[34].Contains(ID))
                    return 34;
                else if (WoT_Event_ID[35].Contains(ID))
                    return 35;
                else if (WoT_Event_ID[36].Contains(ID))
                    return 44;
                else if (WoT_Event_ID[37].Contains(ID))
                    return 45;
                else if (WoT_Event_ID[38].Contains(ID))
                    return 46;
                else if (WoT_Event_ID[39].Contains(ID))
                    return 47;
                else
                    return -1;
            }
            else
            {
                if (ID == 247635361)
                    return 0;
                else if (ID == 2228523046)
                    return 1;
                else if (ID == 1007238985)
                    return 2;
                else if (ID == 1954895547)
                    return 3;
                else if (ID == 2051125750)
                    return 4;
                else if (ID == 736917766)
                    return 5;
                else if (ID == 3774135051)
                    return 6;
                else if (ID == 1562476861)
                    return 7;
                else if (ID == 2892979156)
                    return 8;
                else if (ID == 2744672597)
                    return 9;
                else if (ID == 1043866079)
                    return 10;
                else if (ID == 3604333611)
                    return 11;
                else if (ID == 566427957)
                    return 12;
                else if (ID == 1238687255)
                    return 13;
                else if (ID == 360516555)
                    return 14;
                else if (ID == 3699645660)
                    return 15;
                else if (ID == 3903941485)
                    return 16;
                else if (ID == 703476817)
                    return 17;
                else if (ID == 457312015)
                    return 18;
                else if (ID == 1446710144)
                    return 19;
                else if (ID == 3515803728)
                    return 20;
                else if (ID == 219082534)
                    return 21;
                else if (ID == 1156399922)
                    return 22;
                else if (ID == 1419869192)
                    return 23;
                else if (ID == 526392401)
                    return 24;
                else if (ID == 830603277)
                    return 25;
                else if (ID == 3054323003)
                    return 26;
                else if (ID == 24048714)
                    return 27;
                else if (ID == 3970929146)
                    return 28;
                else if (ID == 2936553558)
                    return 29;
                else if (ID == 2529672877)
                    return 30;
                else if (ID == 1609897361)
                    return 31;
                else if (ID == 3114568527)
                    return 32;
                else if (ID == 479656207)
                    return 33;
                else
                    return -1;
            }
        }
    }
}