using System.Collections.Generic;
using System.IO;

namespace WoTB_Voice_Mod_Creater.Wwise_Class.BNK_To_Wwise_Project
{
    public class Attenuation_Relation
    {
        //Attenuationの設定を保存するためのクラス
        //NameとGUIDはなんでも良い(日本語は無理かも?)
        public string Name { get; private set; }
        public string GUID { get; private set; }
        public uint ShortID { get; private set; }
        public int Curve_Start_Line { get; private set; }
        public int Curve_End_Line { get; private set; }
        public List<bool> curveToUse { get; private set; }
        public Attenuation_Relation(string Name, string GUID, uint ShortID, int Curve_Start_Line, int Curve_End_Line, List<bool> curveToUse)
        {
            this.Name = Name;
            this.GUID = GUID;
            this.ShortID = ShortID;
            this.Curve_Start_Line = Curve_Start_Line;
            this.Curve_End_Line = Curve_End_Line;
            this.curveToUse = curveToUse;
        }
    }
    public class Attenuations
    {
        //Public Attenuations()の形で迷ったけど、何かと使いやすいのでstaticを選択
        public static List<string> Write_All = new List<string>();
        static bool IsInit = false;
        public static void Init()
        {
            if (BNK_Info.Read_All.Count == 0)
                return;
            for (int Number = 0; Number < BNK_Info.Read_All.Count; Number++)
            {
                if (BNK_Info.Read_All[Number].Contains("type=\"u8\" name=\"eHircType\""))
                {
                    if (BNK_Info.Read_All[Number - 1].Contains("<object name=\"CAkAttenuation\""))
                    {
                        string Name;
                        string GUID = System.Guid.NewGuid().ToString().ToUpper();
                        if (BNK_Info.Attenuation_Info.Count + 1 < 10)
                            Name = "Attenuation_0" + (BNK_Info.Attenuation_Info.Count + 1);
                        else
                            Name = "Attenuation_" + (BNK_Info.Attenuation_Info.Count + 1);
                        uint ShortID = WwiseHash.HashString(Name);
                        int Curve_Start_Line = 0;
                        int Curve_End_Line = 0;
                        List<bool> curveToUse = new List<bool>();
                        for (int Number_02 = 0; Number_02 < 7; Number_02++)
                            curveToUse.Add(false);
                        for (int Number_01 = Number + 1; Number_01 < BNK_Info.Read_All.Count; Number_01++)
                        {
                            if (BNK_Info.Read_All[Number_01].Contains("type=\"sid\" name=\"ulID\""))
                                ShortID = uint.Parse(Get_Config.Get_Property_Value(BNK_Info.Read_All[Number_01]));
                            if (BNK_Info.Read_All[Number_01].Contains("type=\"s8\" name=\"curveToUse[0]\""))
                            {
                                curveToUse[0] = true;
                                for (int Number_02 = 1; Number_02 < 7; Number_02++)
                                {
                                    int Value = int.Parse(Get_Config.Get_Property_Value(BNK_Info.Read_All[Number_01 + Number_02]));
                                    if (Value > 0)
                                    {
                                        curveToUse[Number_02] = true;
                                    }
                                }
                            }
                            if (BNK_Info.Read_All[Number_01].Contains("<list name=\"curves\""))
                                Curve_Start_Line = Number_01;
                            if (BNK_Info.Read_All[Number_01].Contains("<object name=\"InitialRTPC\">") || BNK_Info.Read_All[Number_01].Contains("type=\"u8\" name=\"eHircType\""))
                            {
                                Curve_End_Line = Number_01 - 1;
                                break;
                            }
                        }
                        BNK_Info.Attenuation_Info.Add(new Attenuation_Relation(Name, GUID, ShortID, Curve_Start_Line, Curve_End_Line, curveToUse));
                    }
                    else
                        break;
                }
            }
            IsInit = true;
        }
        public static void Create(string To_File)
        {
            if (!IsInit)
                return;
            //共通
            if (!Directory.Exists(Path.GetDirectoryName(To_File)))
                Directory.CreateDirectory(Path.GetDirectoryName(To_File));
            Write_All.Add("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            Write_All.Add("<WwiseDocument Type=\"WorkUnit\" ID=\"{2A4DC1AF-DA27-4BAB-BFB5-D16386FA615E}\" SchemaVersion=\"97\">");
            Write_All.Add("<Attenuations>");
            Write_All.Add("<WorkUnit Name=\"Default Work Unit\" ID=\"{2A4DC1AF-DA27-4BAB-BFB5-D16386FA615E}\" PersistMode=\"Standalone\">");
            Write_All.Add("<ChildrenList>");
            //Attenuationの数だけ追加
            foreach (Attenuation_Relation Temp_Info in BNK_Info.Attenuation_Info)
            {
                Write_All.Add("<Attenuation Name=\"" + Temp_Info.Name + "\" ID=\"{" + Temp_Info.GUID + "}\" ShortID=\"" + Temp_Info.ShortID + "\">");
                Write_All.Add("<PropertyList>");
                int Write_Max_Value_Line = Write_All.Count;
                Write_All.Add("</PropertyList>");
                Write_All.Add("<CurveUsageInfoList>");
                List<int> CurveToUseOnly = new List<int>();
                for (int Number = 0; Number < 7; Number++)
                {
                    if (Temp_Info.curveToUse[Number])
                        CurveToUseOnly.Add(Number);
                }
                int Curve_Number = 0;
                bool IsExistLPF = false;
                bool IsExistHPF = false;
                bool IsExistSpread = false;
                bool IsExistFocus = false;
                double RTPC_Max_Value = 0;
                for (int Number = Temp_Info.Curve_Start_Line; Number <= Temp_Info.Curve_End_Line; Number++)
                {
                    if (BNK_Info.Read_All[Number].Contains("<object name=\"CAkConversionTable\""))
                    {
                        bool IsEndOK = true;
                        int pRTPCMgr_Count = 0;
                        for (int Number_01 = Number; Number_01 <= Temp_Info.Curve_End_Line; Number_01++)
                        {
                            if (BNK_Info.Read_All[Number_01].Contains("<list name=\"pRTPCMgr\""))
                            {
                                pRTPCMgr_Count = int.Parse(Get_Config.Get_Count(BNK_Info.Read_All[Number_01]));
                                IsEndOK = false;
                            }
                            else if (BNK_Info.Read_All[Number_01].Contains("</list>"))
                                IsEndOK = true;
                            if (BNK_Info.Read_All[Number_01].Contains("</object>") && IsEndOK)
                                break;
                        }
                        string Header_Name = "";
                        string Curve_Name_Line = "";
                        int Flag = 0;
                        if (CurveToUseOnly[Curve_Number] == 0)
                        {
                            Header_Name = "VolumeDryUsage";
                            Curve_Name_Line = "<Curve Name=\"VolumeDry\" ID=\"{A9E42A0E-83F8-4B7D-A14F-480B0C2B51DF}\">";
                            Flag = 3;
                            IsExistLPF = true;
                        }
                        else if (CurveToUseOnly[Curve_Number] == 3)
                        {
                            Header_Name = "LowPassFilterUsage";
                            Curve_Name_Line = "<Curve Name=\"LowPassFilter\" ID=\"{4F001EC2-9CC8-4392-8288-1EEA50989868}\">";
                            Flag = 65537;
                            IsExistLPF = true;
                        }
                        else if (CurveToUseOnly[Curve_Number] == 4)
                        {
                            Header_Name = "HighPassFilterUsage";
                            Curve_Name_Line = "<Curve Name=\"HighPassFilter\" ID=\"{5B13F357-F022-4AB6-B0F0-9AB4B403F248}\">";
                            Flag = 65537;
                            IsExistHPF = true;
                        }
                        else if (CurveToUseOnly[Curve_Number] == 5)
                        {
                            Header_Name = "SpreadUsage";
                            Curve_Name_Line = "<Curve Name=\"Spread\" ID=\"{67C40131-35B3-4291-A98F-1BA12172DF31}\">";
                            Flag = 1;
                            IsExistSpread = true;
                        }
                        else if (CurveToUseOnly[Curve_Number] == 6)
                        {
                            Header_Name = "FocusUsage";
                            Curve_Name_Line = "<Curve Name=\"Focus\" ID=\"{6DA87678-DAE5-4A25-B176-67FC98AE4E99}\">";
                            Flag = 1;
                            IsExistFocus = true;
                        }
                        Write_All.Add("<" + Header_Name + ">");
                        Write_All.Add("<CurveUsageInfo Platform=\"Linked\" CurveToUse=\"Custom\">");
                        Write_All.Add(Curve_Name_Line);
                        Write_All.Add("<PropertyList>");
                        Write_All.Add("<Property Name=\"Flags\" Type=\"int32\" Value=\"" + Flag + "\"/>");
                        Write_All.Add("</PropertyList>");
                        Write_All.Add("<PointList>");
                        //RTPCの処理とほとんど同じ
                        //ただし、音量の設定のみは0で元の音量、-1で全く聞こえなくなるという感じなので45 * Y_Posにしています(45は大体の目安)
                        for (int Number_01 = Number; Number_01 <= Temp_Info.Curve_End_Line; Number_01++)
                        {
                            if (BNK_Info.Read_All[Number_01].Contains("<object name=\"AkRTPCGraphPoint\""))
                            {
                                int RTPC_Number = int.Parse(Get_Config.Get_Index(BNK_Info.Read_All[Number_01]));
                                Write_All.Add("<Point>");
                                double X_Pos = double.Parse(Get_Config.Get_Property_Value(BNK_Info.Read_All[Number_01 + 1]));
                                double Y_Pos = double.Parse(Get_Config.Get_Property_Value(BNK_Info.Read_All[Number_01 + 2]));
                                string Interp = Get_Config.Get_SegmentShape(Get_Config.Get_Value_FMT(BNK_Info.Read_All[Number_01 + 3]));
                                Write_All.Add("<XPos>" + X_Pos + "</XPos>");
                                if (CurveToUseOnly[Curve_Number] == 0)
                                    Write_All.Add("<YPos>" + (int)(45 * Y_Pos) + "</YPos>");
                                else
                                    Write_All.Add("<YPos>" + Y_Pos + "</YPos>");
                                if (RTPC_Number == 0)
                                    Write_All.Add("<Flags>5</Flags>");
                                else if (RTPC_Number + 1 == pRTPCMgr_Count)
                                    Write_All.Add("<Flags>37</Flags>");
                                else
                                    Write_All.Add("<Flags>0</Flags>");
                                if (RTPC_Max_Value < X_Pos)
                                    RTPC_Max_Value = X_Pos;
                                if (Interp != "")
                                    Write_All.Add("<SegmentShape>" + Interp + "</SegmentShape>");
                                Write_All.Add("</Point>");
                            }
                            if (BNK_Info.Read_All[Number_01].Contains("</list>"))
                                break;
                        }
                        //RTPCの設定より前に記述する必要があるため、Insertで無理やりねじ込む
                        Write_All.Add("</PointList>");
                        Write_All.Add("</Curve>");
                        Write_All.Add("</CurveUsageInfo>");
                        Write_All.Add("</" + Header_Name + ">");
                        //音量の設定のみ以下の文字列が必要
                        if (CurveToUseOnly[Curve_Number] == 0)
                        {
                            Write_All.Add("<VolumeWetGameUsage>");
                            Write_All.Add("<CurveUsageInfo Platform=\"Linked\" CurveToUse=\"UseVolumeDry\"/>");
                            Write_All.Add("</VolumeWetGameUsage>");
                            Write_All.Add("<VolumeWetUserUsage>");
                            Write_All.Add("<CurveUsageInfo Platform=\"Linked\" CurveToUse=\"UseVolumeDry\"/>");
                            Write_All.Add("</VolumeWetUserUsage>");
                        }
                        Curve_Number++;
                    }
                }
                Write_All.Insert(Write_Max_Value_Line, "<Property Name=\"RadiusMax\" Type=\"Real64\" Value=\"" + RTPC_Max_Value + "\"/>");
                //以下の設定がない場合、設定がないことを記述
                //よくよく考えたら必要ないことに気が付いた
                if (!IsExistLPF)
                    Add_Not_Use_Property("LowPassFilterUsage");
                if (!IsExistHPF)
                    Add_Not_Use_Property("HighPassFilterUsage");
                if (!IsExistSpread)
                    Add_Not_Use_Property("SpreadUsage");
                if (!IsExistFocus)
                    Add_Not_Use_Property("FocusUsage");
                Write_All.Add("</CurveUsageInfoList>");
                Write_All.Add("</Attenuation>");
            }
            Write_All.Add("</ChildrenList>");
            Write_All.Add("</WorkUnit>");
            Write_All.Add("</Attenuations>");
            Write_All.Add("</WwiseDocument>");
            //ファイルに書き込んでメモリを解放
            File.WriteAllLines(To_File, Write_All);
            Write_All.Clear();
        }
        static void Add_Not_Use_Property(string Name)
        {
            /*Write_All.Add("<" + Name + ">");
            Write_All.Add("<CurveUsageInfo Platform=\"Linked\" CurveToUse=\"None\"/>");
            Write_All.Add("</" + Name + ">");*/
        }
    }
}