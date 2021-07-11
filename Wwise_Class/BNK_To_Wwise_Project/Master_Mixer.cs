using System;
using System.Collections.Generic;
using System.IO;

namespace WoTB_Voice_Mod_Creater.Wwise_Class.BNK_To_Wwise_Project
{
    public class Master_Audio_Relation
    {
        public string Name { get; set; }
        public string GUID { get; private set; }
        public uint ShortID { get; private set; }
        public Master_Audio_Relation(uint ShortID)
        {
            Name = ShortID.ToString();
            this.ShortID = ShortID;
            GUID = Guid.NewGuid().ToString().ToUpper();
        }
    }
    public class Master_Mixer
    {
        public const string Master_Bus_Unit = "005C6247-5812-4D7E-86EA-2F3C50B5E166";
        public static List<Master_Audio_Relation> Master_Audio_Info = new List<Master_Audio_Relation>();
        static List<string> Master_Audio_Bus_Write = new List<string>();
        static bool IsInit = false;
        public static void Init()
        {
            if (BNK_Info.Read_All.Count == 0)
                return;
            for (int Number = 0; Number < BNK_Info.Init_Read_All.Count; Number++)
            {
                string Read_Line = BNK_Info.Init_Read_All[Number];
                if (Read_Line.Contains("type=\"sid\" name=\"ulID\" value=\""))
                {
                    List<string> ID_Line_Tmp = new List<string>();
                    //イベントIDを取得
                    string Name = Get_Config.Get_Property_Name(BNK_Info.Init_Read_All[Number - 3]);
                    if (Name == "CAkBus")
                    {
                        uint ShortID = uint.Parse(Get_Config.Get_Property_Value(Read_Line));
                        Master_Audio_Info.Add(new Master_Audio_Relation(ShortID));
                    }
                }
            }
            IsInit = true;
        }
        public static void Create(string To_File)
        {
            if (!IsInit)
                return;
            if (!Directory.Exists(Path.GetDirectoryName(To_File)))
                Directory.CreateDirectory(Path.GetDirectoryName(To_File));
            //共通
            Master_Audio_Bus_Write.Add("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            Master_Audio_Bus_Write.Add("<WwiseDocument Type=\"WorkUnit\" ID=\"{" + Master_Bus_Unit + "}\" SchemaVersion=\"97\">");
            Master_Audio_Bus_Write.Add("<Busses>");
            Master_Audio_Bus_Write.Add("<WorkUnit Name=\"Default Work Unit\" ID=\"{" + Master_Bus_Unit + "}\" PersistMode=\"Standalone\">");
            Master_Audio_Bus_Write.Add("<ChildrenList>");
            Master_Audio_Bus_Write.Add("<Bus Name=\"Master Audio Bus\" ID=\"{1514A4D8-1DA6-412A-A17E-75CA0C2149F3}\">");
            Master_Audio_Bus_Write.Add("<ReferenceList>");
            Master_Audio_Bus_Write.Add("<Reference Name=\"AudioDevice\">");
            Master_Audio_Bus_Write.Add("<ObjectRef Name=\"System\" ID=\"{4CD97F24-1FA9-4334-82C6-E371A7A51E93}\" WorkUnitID=\"{4E4E5CEF-BF9B-414A-B547-90244203047B}\"/>");
            Master_Audio_Bus_Write.Add("</Reference>");
            Master_Audio_Bus_Write.Add("</ReferenceList>");
            Master_Audio_Bus_Write.Add("<ChildrenList>");
            //Master Audio BusはInit.bnkで制御されるので、ここでは名前(ShortID)のみ合っていれば良いです。
            foreach (Master_Audio_Relation Temp in Master_Audio_Info)
                if (Temp.Name != Temp.ShortID.ToString())
                    Master_Audio_Bus_Write.Add("<Bus Name=\"" + Temp.Name + "\" ID=\"{" + Temp.GUID + "}\"/>");
            Master_Audio_Bus_Write.Add("</ChildrenList>");
            Master_Audio_Bus_Write.Add("</Bus>");
            Master_Audio_Bus_Write.Add("<Bus Name=\"Motion Factory Bus\" ID=\"{2AF9B9C6-6EF1-46E9-B5F2-E30C9E602C74}\">");
            Master_Audio_Bus_Write.Add("<ReferenceList>");
            Master_Audio_Bus_Write.Add("<Reference Name=\"AudioDevice\">");
            Master_Audio_Bus_Write.Add("<ObjectRef Name=\"Default_Motion_Device\" ID=\"{49D1BE5F-2306-4FB9-84AA-0D4D55C64CC1}\" WorkUnitID=\"{4E4E5CEF-BF9B-414A-B547-90244203047B}\"/>");
            Master_Audio_Bus_Write.Add("</Reference>");
            Master_Audio_Bus_Write.Add("</ReferenceList>");
            Master_Audio_Bus_Write.Add("</Bus>");
            Master_Audio_Bus_Write.Add("</ChildrenList>");
            Master_Audio_Bus_Write.Add("</WorkUnit>");
            Master_Audio_Bus_Write.Add("</Busses>");
            Master_Audio_Bus_Write.Add("</WwiseDocument>");
            File.WriteAllLines(To_File, Master_Audio_Bus_Write);
            Master_Audio_Bus_Write.Clear();
        }
    }
}