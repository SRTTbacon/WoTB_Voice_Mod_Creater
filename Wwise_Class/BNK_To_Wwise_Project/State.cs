using System;
using System.Collections.Generic;
using System.IO;

namespace WoTB_Voice_Mod_Creater.Wwise_Class.BNK_To_Wwise_Project
{
    //Stateの情報を保存するクラス
    public class State_Relation
    {
        public string GUID { get; private set; }
        public uint Short_ID { get; private set; }
        public double Volume { get; private set; }
        public double Pitch { get; private set; }
        public double LPF { get; private set; }
        public double HPF { get; private set; }
        public double MakeUpGain { get; private set; }
        public State_Relation(uint Short_ID, double Volume, double Pitch, double LPF, double HPF, double MakeUpGain)
        {
            this.GUID = Guid.NewGuid().ToString().ToUpper();
            this.Volume = Volume;
            this.Short_ID = Short_ID;
            this.Pitch = Pitch;
            this.LPF = LPF;
            this.HPF = HPF;
            this.MakeUpGain = MakeUpGain;
        }
    }
    public class State_Child_Relation
    {
        public string Name { get; set; }
        public string GUID { get; private set; }
        public uint Short_ID { get; private set; }
        public bool IsWrote = false;
        public State_Child_Relation(uint Short_ID)
        {
            this.GUID = Guid.NewGuid().ToString().ToUpper();
            this.Short_ID = Short_ID;
        }
    }
    public class State_Child
    {
        public uint StateFrom_Short_ID { get; private set; }
        public uint StateTo_Short_ID { get; private set; }
        public double TransitionTime { get; private set; }
        public State_Child(uint StateFrom_Short_ID, uint StateTo_Short_ID, double TransitionTime)
        {
            this.StateFrom_Short_ID = StateFrom_Short_ID;
            this.StateTo_Short_ID = StateTo_Short_ID;
            this.TransitionTime = TransitionTime;
        }
    }
    public class State_Parent
    {
        public string Name { get; set; }
        public string GUID { get; private set; }
        public uint Short_ID { get; private set; }
        public double DefaultTransitionTime { get; private set; }
        public List<State_Child> Children { get; private set; }
        public State_Parent(uint Short_ID, double DefaultTransitionTime, List<State_Child> Children)
        {
            this.GUID = Guid.NewGuid().ToString().ToUpper();
            this.Short_ID = Short_ID;
            this.DefaultTransitionTime = DefaultTransitionTime;
            this.Children = Children;
        }
    }
    public class State
    {
        public static List<State_Parent> State_All_Info = new List<State_Parent>();
        public static List<State_Relation> State_Value_Info = new List<State_Relation>();
        public static List<State_Child_Relation> State_Child_Info = new List<State_Child_Relation>();
        const string Work_Unit_GUID = "09157149-5AC0-414F-9B28-B91AC0D08A8C";
        static bool IsInited = false;
        public static void Init()
        {
            for (int Number = 0; Number < BNK_Info.Init_Read_All.Count; Number++)
            {
                if (BNK_Info.Init_Read_All[Number].Contains("<object name=\"AkStateGroup\""))
                {
                    uint Parent_Short_ID = uint.Parse(Get_Config.Get_Property_Value(BNK_Info.Init_Read_All[Number + 1]));
                    double DefaultTransitionTime = double.Parse(Get_Config.Get_Property_Value(BNK_Info.Init_Read_All[Number + 2]));
                    Number += 2;
                    List<State_Child> Child = new List<State_Child>();
                    for (int Number_01 = Number + 3; Number_01 < BNK_Info.Init_Read_All.Count; Number_01++)
                    {
                        if (BNK_Info.Init_Read_All[Number_01].Contains("object name=\"AkStateTransition\""))
                        {
                            uint From = uint.Parse(Get_Config.Get_Property_Value(BNK_Info.Init_Read_All[Number_01 + 1]));
                            uint To = uint.Parse(Get_Config.Get_Property_Value(BNK_Info.Init_Read_All[Number_01 + 2]));
                            bool IsFromExist = false;
                            bool IsToExist = false;
                            for (int Number_02 = 0; Number_02 < State_Child_Info.Count; Number_02++)
                            {
                                if (State_Child_Info[Number_02].Short_ID == From)
                                    IsFromExist = true;
                                else if (State_Child_Info[Number_02].Short_ID == To)
                                    IsToExist = true;
                            }
                            if (!IsFromExist)
                                State_Child_Info.Add(new State_Child_Relation(From));
                            if (!IsToExist)
                                State_Child_Info.Add(new State_Child_Relation(To));
                            double Time = double.Parse(Get_Config.Get_Property_Value(BNK_Info.Init_Read_All[Number_01 + 3]));
                            Child.Add(new State_Child(From, To, Time));
                            Number_01 += 2;
                            Number += 3;
                        }
                        else if (BNK_Info.Init_Read_All[Number_01].Contains("</list>"))
                            break;
                    }
                    State_All_Info.Add(new State_Parent(Parent_Short_ID, DefaultTransitionTime, Child));
                }
            }
            List<int> State_Numbers = new List<int>();
            for (int Number = 0; Number < BNK_Info.ID_Line.Count; Number++)
                if (BNK_Info.ID_Line[Number][2] == "CAkState")
                    State_Numbers.Add(Number);
            foreach (int Number in State_Numbers)
            {
                double Volume = 0;
                double Pitch = 0;
                double LPF = 0;
                double HPF = 0;
                double MakeUpGain = 0;
                for (int Number_01 = int.Parse(BNK_Info.ID_Line[Number][1]); Number_01 < BNK_Info.Read_All.Count; Number_01++)
                {
                    string Read_Line = BNK_Info.Read_All[Number_01];
                    //Switch_GroupのShortIDを取得
                    if (Read_Line.Contains("valuefmt=\"0x00[Volume]\""))
                        Volume = double.Parse(Get_Config.Get_Property_Value(BNK_Info.Read_All[Number_01 + 1]));
                    else if (Read_Line.Contains("valuefmt=\"0x02 [Pitch]\""))
                        Pitch = double.Parse(Get_Config.Get_Property_Value(BNK_Info.Read_All[Number_01 + 1]));
                    else if (Read_Line.Contains("valuefmt=\"0x03 [LPF]\""))
                        LPF = double.Parse(Get_Config.Get_Property_Value(BNK_Info.Read_All[Number_01 + 1]));
                    else if (Read_Line.Contains("valuefmt=\"0x04 [HPF]\""))
                        HPF = double.Parse(Get_Config.Get_Property_Value(BNK_Info.Read_All[Number_01 + 1]));
                    else if (Read_Line.Contains("valuefmt=\"0x07 [StatePropNum/Priority]\""))
                        MakeUpGain = double.Parse(Get_Config.Get_Property_Value(BNK_Info.Read_All[Number_01 + 1]));
                    if (Read_Line.Contains("</list>"))
                        break;
                }
                State_Value_Info.Add(new State_Relation(uint.Parse(BNK_Info.ID_Line[Number][0]), Volume, Pitch, LPF, HPF, MakeUpGain));

            }
            State_Numbers.Clear();
            IsInited = true;
        }
        public static void Create(string To_File)
        {
            if (!IsInited)
                return;
            if (!Directory.Exists(Path.GetDirectoryName(To_File)))
                Directory.CreateDirectory(Path.GetDirectoryName(To_File));
            List<string> State_Write = new List<string>();
            State_Write.Add("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            State_Write.Add("<WwiseDocument Type=\"WorkUnit\" ID=\"{" + Work_Unit_GUID + "}\" SchemaVersion=\"97\">");
            State_Write.Add("<States>");
            if (State_All_Info.Count == 0)
                State_Write.Add("<WorkUnit Name=\"Default Work Unit\" ID=\"{" + Work_Unit_GUID + "}\" PersistMode=\"Standalone\"/>");
            else
            {
                State_Write.Add("<WorkUnit Name=\"Default Work Unit\" ID=\"{" + Work_Unit_GUID + "}\" PersistMode=\"Standalone\">");
                State_Write.Add("<ChildrenList>");
                foreach (State_Parent SP in State_All_Info)
                {
                    State_Write.Add("<StateGroup Name=\"" + SP.Name + "\" ID=\"{" + SP.GUID + "}\">");
                    if (SP.DefaultTransitionTime != 0)
                    {
                        State_Write.Add("<PropertyList>");
                        State_Write.Add("<Property Name=\"DefaultTransitionTime\" Type=\"Real64\" Value=\"" + SP.DefaultTransitionTime + "\"/>");
                        State_Write.Add("</PropertyList>");
                    }
                    State_Write.Add("<ChildrenList>");
                    State_Write.Add("<State Name=\"None\" ID=\"{" + Guid.NewGuid().ToString().ToUpper() + "}\"/>");
                    foreach (State_Child Child in SP.Children)
                    {
                        for (int Number = 0; Number < State_Child_Info.Count; Number++)
                        {
                            if (State_Child_Info[Number].Short_ID == Child.StateFrom_Short_ID || State_Child_Info[Number].Short_ID == Child.StateTo_Short_ID)
                            {
                                if (!State_Child_Info[Number].IsWrote)
                                {
                                    State_Child_Info[Number].IsWrote = true;
                                    State_Write.Add("<State Name=\"" + State_Child_Info[Number].Name + "\" ID=\"{" + State_Child_Info[Number].GUID + "}\"/>");
                                    break;
                                }
                            }
                        }
                    }
                    State_Write.Add("</ChildrenList>");
                    if (SP.Children.Count > 0)
                    {
                        State_Write.Add("<TransitionList>");
                        foreach (State_Child Child in SP.Children)
                        {
                            State_Write.Add("<Transition>");
                            for (int Number = 0; Number < State_Child_Info.Count; Number++)
                            {
                                if (State_Child_Info[Number].Short_ID == Child.StateFrom_Short_ID)
                                {
                                    State_Write.Add("<StartState Name=\"" + State_Child_Info[Number].Name + "\" ID=\"{" + State_Child_Info[Number].GUID + "}\"/>");
                                    break;
                                }
                            }
                            for (int Number = 0; Number < State_Child_Info.Count; Number++)
                            {
                                if (State_Child_Info[Number].Short_ID == Child.StateTo_Short_ID)
                                {
                                    State_Write.Add("<EndState Name=\"" + State_Child_Info[Number].Name + "\" ID=\"{" + State_Child_Info[Number].GUID + "}\"/>");
                                    break;
                                }
                            }
                            State_Write.Add("<Time>" + Child.TransitionTime + "</Time>");
                            State_Write.Add("<IsShared>false</IsShared>");
                            State_Write.Add("</Transition>");
                        }
                        State_Write.Add("</TransitionList>");
                    }
                    State_Write.Add("</StateGroup>");
                    for (int Number = 0; Number > State_Child_Info.Count; Number++)
                        State_Child_Info[Number].IsWrote = false;
                }
                State_Write.Add("</ChildrenList>");
                State_Write.Add("</WorkUnit>");
            }
            State_Write.Add("</States>");
            State_Write.Add("</WwiseDocument>");
            File.WriteAllLines(To_File, State_Write);
            State_Write.Clear();
        }
    }
}