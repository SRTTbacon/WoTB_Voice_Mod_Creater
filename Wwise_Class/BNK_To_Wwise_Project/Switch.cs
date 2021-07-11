using System;
using System.Collections.Generic;
using System.IO;

namespace WoTB_Voice_Mod_Creater.Wwise_Class.BNK_To_Wwise_Project
{
    public class Switch_Child_Relation
    {
        public string Name { get; set; }
        public string GUID { get; private set; }
        public uint ShortID { get; private set; }
        public Switch_Child_Relation(string Name, string GUID, uint ShortID)
        {
            this.Name = Name;
            this.GUID = GUID;
            this.ShortID = ShortID;
        }
    }
    public class Switch_Child_Container
    {
        public string GUID { get; private set; }
        public uint Parent_ShortID { get; private set; }
        public uint ShortID { get; private set; }
        public List<uint> Containers = new List<uint>();
        public Switch_Child_Container(string GUID, uint Parent_ShortID, uint ShortID, List<uint> Containers)
        {
            this.GUID = GUID;
            this.Parent_ShortID = Parent_ShortID;
            this.ShortID = ShortID;
            this.Containers = Containers;
        }
    }
    public class Switch_Relation
    {
        public string Name { get; set; }
        public string GUID { get; private set; }
        public uint ShortID { get; private set; }
        public List<Switch_Child_Relation> Children { get; set; }
        public Switch_Relation(string Name, string GUID, uint ShortID, List<Switch_Child_Relation> Children)
        {
            this.Name = Name;
            this.GUID = GUID;
            this.ShortID = ShortID;
            this.Children = Children;
        }
    }
    public class Switch
    {
        static bool IsInit = false;
        public const string Work_Unit_GUID = "A38CA771-2117-43FB-9E76-7EA07E4F9611";
        public static List<Switch_Relation> Switch_Info = new List<Switch_Relation>();
        public static void Init()
        {
            List<int> Switch_Numbers = new List<int>();
            List<uint> Temp = new List<uint>();
            for (int Number = 0; Number < BNK_Info.ID_Line.Count; Number++)
            {
                if (BNK_Info.ID_Line[Number][2] == "CAkSwitchCntr")
                    Switch_Numbers.Add(Number);
            }
            foreach (int Number in Switch_Numbers)
            {
                //GUIDは自由に指定できます
                string My_GUID = Guid.NewGuid().ToString().ToUpper();
                uint My_ShortID = 0;
                List<Switch_Child_Relation> Child = new List<Switch_Child_Relation>();
                for (int Number_01 = int.Parse(BNK_Info.ID_Line[Number][1]); Number_01 < BNK_Info.Read_All.Count; Number_01++)
                {
                    string Read_Line = BNK_Info.Read_All[Number_01];
                    //Switch_GroupのShortIDを取得
                    if (Read_Line.Contains("type=\"tid\" name=\"ulGroupID\""))
                        My_ShortID = uint.Parse(Get_Config.Get_Property_Value(Read_Line));
                    if (Read_Line.Contains("type=\"sid\" name=\"ulSwitchID\""))
                    {
                        uint Child_ShortID = uint.Parse(Get_Config.Get_Property_Value(Read_Line));
                        Child.Add(new Switch_Child_Relation("", Guid.NewGuid().ToString().ToUpper(), Child_ShortID));
                    }
                    if (Read_Line.Contains("type=\"u8\" name=\"eHircType\""))
                        break;
                }
                if (!Temp.Contains(My_ShortID))
                {
                    Switch_Info.Add(new Switch_Relation("", My_GUID, My_ShortID, Child));
                    Temp.Add(My_ShortID);
                }
            }
            Temp.Clear();
            IsInit = true;
        }
        public static void Create(string To_File)
        {
            if (!IsInit)
                return;
            if (!Directory.Exists(Path.GetDirectoryName(To_File)))
                Directory.CreateDirectory(Path.GetDirectoryName(To_File));
            List<string> Switch_Write = new List<string>();
            Switch_Write.Add("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            Switch_Write.Add("<WwiseDocument Type=\"WorkUnit\" ID=\"{" + Work_Unit_GUID + "}\" SchemaVersion=\"97\">");
            Switch_Write.Add("<Switches>");
            Switch_Write.Add("<WorkUnit Name=\"Default Work Unit\" ID=\"{" + Work_Unit_GUID + "}\" PersistMode=\"Standalone\">");
            Switch_Write.Add("<ChildrenList>");
            foreach (Switch_Relation Switch_Parent in Switch_Info)
            {
                if (Switch_Parent.Name != null && Switch_Parent.Name != "")
                {
                    Switch_Write.Add("<SwitchGroup Name=\"" + Switch_Parent.Name + "\" ID=\"{" + Switch_Parent.GUID + "}\">");
                    Switch_Write.Add("<ChildrenList>");
                    foreach (Switch_Child_Relation Switch_Child in Switch_Parent.Children)
                        if (Switch_Child.Name != null && Switch_Child.Name != "")
                            Switch_Write.Add("<Switch Name=\"" + Switch_Child.Name + "\" ID=\"{" + Switch_Child.GUID + "}\"/>");
                    Switch_Write.Add("</ChildrenList>");
                    Switch_Write.Add("</SwitchGroup>");
                }
            }
            Switch_Write.Add("</ChildrenList>");
            Switch_Write.Add("</WorkUnit>");
            Switch_Write.Add("</Switches>");
            Switch_Write.Add("</WwiseDocument>");
            File.WriteAllLines(To_File, Switch_Write);
            Switch_Write.Clear();
        }
    }
}