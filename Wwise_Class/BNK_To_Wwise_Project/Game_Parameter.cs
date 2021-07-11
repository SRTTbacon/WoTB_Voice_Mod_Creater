using System.Collections.Generic;
using System.IO;

namespace WoTB_Voice_Mod_Creater.Wwise_Class.BNK_To_Wwise_Project
{
    public class Game_Parameter
    {
        //Game_Parameterの設定をファイルに保存
        public static void Create(string To_File)
        {
            int Not_Use_Count = 1;
            //使用されていないRTPCの名前を変更
            for (int Number = 0; Number < BNK_Info.RTPC_Info.Count; Number++)
            {
                if (BNK_Info.RTPC_Info[Number].Name == null)
                {
                    string Not_Use_Count_String;
                    if (Not_Use_Count < 10)
                        Not_Use_Count_String = "0" + Not_Use_Count;
                    else
                        Not_Use_Count_String = Not_Use_Count.ToString();
                    BNK_Info.RTPC_Info[Number].Name = "Not_Used_" + Not_Use_Count_String;
                    Not_Use_Count++;
                }
            }
            if (!Directory.Exists(Path.GetDirectoryName(To_File)))
                Directory.CreateDirectory(Path.GetDirectoryName(To_File));
            List<string> Write_Param = new List<string>();
            //共通
            Write_Param.Add("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            Write_Param.Add("<WwiseDocument Type=\"WorkUnit\" ID=\"{" + BNK_Info.Parent_RTPC_WorkUnit + "}\" SchemaVersion=\"97\">");
            Write_Param.Add("<GameParameters>");
            Write_Param.Add("<WorkUnit Name=\"SRTTbacon\" ID=\"{" + BNK_Info.Parent_RTPC_WorkUnit + "}\" PersistMode=\"Standalone\">");
            Write_Param.Add("<ChildrenList>");
            //RTPCの数だけループ
            foreach (RTPC_Relation RTPC_Now in BNK_Info.RTPC_Info)
            {
                if (RTPC_Now.Name.Contains("Not_Used_"))
                    continue;
                int Param_Index = Get_Param_Index_From_Name(RTPC_Now.Param);
                Write_Param.Add("<GameParameter Name=\"" + RTPC_Now.Name + "\" ID=\"{" + RTPC_Now.GUID + "}\">");
                Write_Param.Add("<PropertyList>");
                Write_Param.Add("<Property Name=\"BindToBuiltInParam\" Type=\"int16\" Value=\"" + Param_Index + "\"/>");
                Write_Param.Add("<Property Name=\"InitialValue\" Type=\"Real64\" Value=\"" + RTPC_Now.Default_Value + "\"/>");
                //Param_Indexが0か1以外はMinとMaxの値が定められているためそれに従う
                if (Param_Index == 0)
                {
                    Write_Param.Add("<Property Name=\"Max\" Type=\"Real64\" Value=\"" + RTPC_Now.Max_Value + "\"/>");
                    Write_Param.Add("<Property Name=\"Min\" Type=\"Real64\" Value=\"" + RTPC_Now.Min_Value + "\"/>");
                }
                else if (Param_Index == 1)
                    Write_Param.Add("<Property Name=\"Max\" Type=\"Real64\" Value=\"" + RTPC_Now.Max_Value + "\"/>");
                else if (Param_Index == 2)
                {
                    Write_Param.Add("<Property Name=\"Max\" Type=\"Real64\" Value=\"180\"/>");
                    Write_Param.Add("<Property Name=\"Min\" Type=\"Real64\" Value=\"-180\"/>");
                }
                else if (Param_Index == 3)
                {
                    Write_Param.Add("<Property Name=\"Max\" Type=\"Real64\" Value=\"90\"/>");
                    Write_Param.Add("<Property Name=\"Min\" Type=\"Real64\" Value=\"-90\"/>");
                }
                else if (Param_Index == 4)
                {
                    Write_Param.Add("<Property Name=\"Max\" Type=\"Real64\" Value=\"180\"/>");
                }
                else if (Param_Index == 7)
                    Write_Param.Add("<Property Name=\"Max\" Type=\"Real64\" Value=\"180\"/>");
                int Mode_Index = Get_Mode_Index_From_Name(RTPC_Now.Type);
                //Slow lateなどが設定されていればそれも適応
                if (Mode_Index != 0)
                {
                    Write_Param.Add("<Property Name=\"RTPCRamping\" Type=\"int16\" Value=\"" + Mode_Index + "\"/>");
                    if (Mode_Index == 1)
                    {
                        Write_Param.Add("<Property Name=\"SlewRateDown\" Type=\"Real64\" Value=\"" + RTPC_Now.RampDown + "\"/>");
                        Write_Param.Add("<Property Name=\"SlewRateUp\" Type=\"Real64\" Value=\"" + RTPC_Now.RampUP + "\"/>");
                    }
                    else if (Mode_Index == 2)
                    {
                        Write_Param.Add("<Property Name=\"FilterTimeDown\" Type=\"Real64\" Value=\"" + RTPC_Now.RampDown + "\"/>");
                        Write_Param.Add("<Property Name=\"FilterTimeUp\" Type=\"Real64\" Value=\"" + RTPC_Now.RampUP + "\"/>");
                    }
                }
                Write_Param.Add("</PropertyList>");
                Write_Param.Add("</GameParameter>");
            }
            Write_Param.Add("</ChildrenList>");
            Write_Param.Add("</WorkUnit>");
            Write_Param.Add("</GameParameters>");
            Write_Param.Add("</WwiseDocument>");
            //ファイルに書き込み、メモリを解放
            File.WriteAllLines(To_File, Write_Param);
            Write_Param.Clear();
        }
        static int Get_Param_Index_From_Name(string Name)
        {
            if (Name == "None")
                return 0;
            else if (Name == "Start/Distance")
                return 1;
            else if (Name == "Azimuth")
                return 2;
            else if (Name == "Elevation")
                return 3;
            else if (Name == "EmitterCone")
                return 4;
            else if (Name == "Obsruction")
                return 5;
            else if (Name == "Occlusion")
                return 6;
            else if (Name == "ListenerCone")
                return 7;
            else if (Name == "Diffraction")
                return 8;
            return 0;
        }
        static int Get_Mode_Index_From_Name(string Name)
        {
            if (Name == "SlewRate")
                return 1;
            else if (Name == "FilteringOverTime")
                return 2;
            return 0;
        }
    }
}