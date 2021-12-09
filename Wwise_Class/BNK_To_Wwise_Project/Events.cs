using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace WoTB_Voice_Mod_Creater.Wwise_Class.BNK_To_Wwise_Project
{
    public class Event_Action
    {
        public string Name { get; set; }
        public string GUID { get; private set; }
        public string SoundBank { get; set; }
        public uint ShortID { get; private set; }
        public uint Target_ShortID { get; set; }
        public int Line { get; private set; }
        public int Mode { get; set; }
        public Event_Action(uint ShortID, int Line)
        {
            this.GUID = Guid.NewGuid().ToString().ToUpper();
            this.ShortID = ShortID;
            this.Line = Line;
        }
    }
    public class Event_Parent
    {
        public string Name { get; set; }
        public string GUID { get; private set; }
        public string SoundBankName { get; set; }
        public uint ShortID { get; private set; }
        public List<Event_Action> Action = new List<Event_Action>();
        public Event_Parent(uint ShortID, List<Event_Action> Action)
        {
            this.GUID = Guid.NewGuid().ToString().ToUpper();
            this.ShortID = ShortID;
            this.Action = Action;
        }
    }
    public class Events
    {
        public static List<Event_Parent> Event_Info = new List<Event_Parent>();
        public static List<uint> Event_ShortID = new List<uint>();
        public static readonly string Event_Unit = Guid.NewGuid().ToString().ToUpper();
        public const string Event_Parent_UUID = "A69BF0FC-D84A-487C-ABDD-56F71CB2A126";
        public const string RootDocumentID = "B34051A6-F1D3-4558-A9ED-EB78F1DC3BDE";
        //必ず実行させる
        //イベント情報をリストに保存
        public static void Init()
        {
            foreach (List<string> Temp in BNK_Info.ID_Line)
            {
                if (Temp[2] == "CAkEvent")
                {
                    uint Parent_ShortID = uint.Parse(Temp[0]);
                    if (!Event_ShortID.Contains(Parent_ShortID))
                        Event_ShortID.Add(Parent_ShortID);
                    List<Event_Action> Actions = new List<Event_Action>();
                    for (int Number = int.Parse(Temp[1]); Number < BNK_Info.Read_All.Count; Number++)
                    {
                        if (BNK_Info.Read_All[Number].Contains("type=\"u8\" name=\"eHircType\"") || BNK_Info.Read_All[Number].Contains("</list>"))
                            break;
                        if (BNK_Info.Read_All[Number].Contains("type=\"tid\" name=\"ulActionID\""))
                        {
                            uint Action_ShortID = uint.Parse(Get_Config.Get_Property_Value(BNK_Info.Read_All[Number]));
                            foreach (List<string> Temp_01 in BNK_Info.ID_Line)
                            {
                                if (Temp_01[2] == "CAkActionPlay" && uint.Parse(Temp_01[0]) == Action_ShortID)
                                {
                                    Actions.Add(new Event_Action(uint.Parse(Get_Config.Get_Property_Value(BNK_Info.Read_All[Number])), int.Parse(Temp_01[1])));
                                    break;
                                }
                            }
                        }
                    }
                    Event_Info.Add(new Event_Parent(Parent_ShortID, Actions));
                }
            }
            for (int Number_00 = 0; Number_00 < Event_Info.Count; Number_00++)
            {
                for (int Number = 0; Number < Event_Info[Number_00].Action.Count; Number++)
                {
                    for (int Line = Event_Info[Number_00].Action[Number].Line; Line < BNK_Info.Read_All.Count; Line++)
                    {
                        if (BNK_Info.Read_All[Line].Contains("type=\"u8\" name=\"eHircType\""))
                            break;
                        else if (BNK_Info.Read_All[Line].Contains("type=\"u16\" name=\"ulActionType\""))
                            Event_Info[Number_00].Action[Number].Mode = Change_Name_To_Mode(Get_Config.Get_Value_FMT(BNK_Info.Read_All[Line]));
                        else if (BNK_Info.Read_All[Line].Contains("type=\"tid\" name=\"idExt\""))
                            Event_Info[Number_00].Action[Number].Target_ShortID = uint.Parse(Get_Config.Get_Property_Value(BNK_Info.Read_All[Line]));
                        else if (BNK_Info.Read_All[Line].Contains("type=\"tid\" name=\"bankID\""))
                            Event_Info[Number_00].Action[Number].SoundBank = Get_Config.Get_HashName(BNK_Info.Read_All[Line]);
                    }
                }
            }
        }
        //イベント名がなければShortIDから作成する
        public static async Task Set_Name(System.Windows.Controls.TextBlock Message_T)
        {
            if (Event_ShortID.Count > 0)
            {
                foreach (uint ShortID_Now in Event_ShortID)
                {
                    int RTPC_Info_Number = -1;
                    for (int Number = 0; Number < Event_Info.Count; Number++)
                    {
                        if (Event_Info[Number].ShortID == ShortID_Now && Event_Info[Number].Name == null)
                        {
                            RTPC_Info_Number = Number;
                            break;
                        }
                    }
                    Message_T.Text = ShortID_Now + "を対応する文字列へ変換しています...";
                    await Task.Delay(50);
                    if (RTPC_Info_Number != -1)
                    {
                        string Name = Get_Config.Get_Hash_Name_From_ShortID(ShortID_Now);
                        Event_Info[RTPC_Info_Number].Name = Name;
                    }
                }
                Message_T.Text = "Eventsを適応しました。";
                await Task.Delay(50);
            }
        }
        //イベントの内容をファイルに保存
        public static void Create(string To_File)
        {
            if (!Directory.Exists(Path.GetDirectoryName(To_File)))
                Directory.CreateDirectory(Path.GetDirectoryName(To_File));
            List<string> Write_All = new List<string>();
            Write_All.Add("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            Write_All.Add("<WwiseDocument Type=\"WorkUnit\" ID=\"{" + Event_Unit + "}\" SchemaVersion=\"97\">");
            Write_All.Add("<Events>");
            Write_All.Add("<WorkUnit Name=\"SRTTbacon\" ID=\"{" + Event_Unit + "}\" PersistMode=\"Standalone\">");
            Write_All.Add("<ChildrenList>");
            foreach (Event_Parent Temp_01 in Event_Info)
            {
                Write_All.Add("<Event Name=\"" + Temp_01.Name + "\" ID=\"{" + Temp_01.GUID + "}\">");
                Write_All.Add("<ChildrenList>");
                foreach (Event_Action Temp_02 in Temp_01.Action)
                {
                    Write_All.Add("<Action Name=\"\" ID=\"{" + Temp_02.GUID + "}\" ShortID=\"" + Temp_02.ShortID + "\">");
                    Write_All.Add("<PropertyList>");
                    Write_All.Add("<Property Name=\"ActionType\" Type=\"int16\" Value=\"" + Temp_02.Mode + "\"/>");
                    Write_All.Add("</PropertyList>");
                    Write_All.Add("<ReferenceList>");
                    Write_All.Add("<Reference Name=\"Target\">");
                    foreach (BNK_Relation Temp_03 in BNK_Info.Relation)
                    {
                        if (Temp_03.My == Temp_02.Target_ShortID)
                        {
                            Write_All.Add("<ObjectRef Name=\"" + Temp_03.My + "\" ID=\"{" + Temp_03.GUID + "}\" WorkUnitID=\"{" + Actor_Mixer.Actor_Mixer_Unit + "}\"/>");
                            break;
                        }
                    }
                    Write_All.Add("</Reference>");
                    Write_All.Add("</ReferenceList>");
                    Write_All.Add("</Action>");
                }
                Write_All.Add("</ChildrenList>");
                Write_All.Add("</Event>");
            }
            Write_All.Add("</ChildrenList>");
            Write_All.Add("</WorkUnit>");
            Write_All.Add("</Events>");
            Write_All.Add("</WwiseDocument>");
            File.WriteAllLines(To_File, Write_All);
            Write_All.Clear();
        }
        static int Change_Name_To_Mode(string Name)
        {
            if (Name == "Play")
                return 1;
            else if (Name == "Stop_E_O")
                return 2;
            else if (Name == "Stop_ALL_O")
                return 3;
            else if (Name == "Pause_E_O")
                return 7;
            else if (Name == "Pause_ALL_O")
                return 8;
            else if (Name == "Resume_E_O")
                return 9;
            else if (Name == "Resume_ALL_O")
                return 10;
            else if (Name == "Seek_E_O")
                return 36;
            else if (Name == "Seek_ALL_O")
                return 37;
            return 1;
        }
    }
}