using System;
using System.Collections.Generic;

namespace WoTB_Voice_Mod_Creater.Wwise_Class.BNK_To_Wwise_Project
{
    public class Layer_CrossFade
    {
        public uint Target_ShortID { get; private set; }
        public double Start_From { get; private set; }
        public double Start_To { get; private set; }
        public double End_From { get; private set; }
        public double End_To { get; private set; }
        public Layer_CrossFade(uint Target_ShortID, double Start_From, double Start_To, double End_From, double End_To)
        {
            this.Target_ShortID = Target_ShortID;
            this.Start_From = Start_From;
            this.Start_To = Start_To;
            this.End_From = End_From;
            this.End_To = End_To;
        }
    }
    public class Layer_Relation
    {
        public string GUID { get; private set; }
        public uint Parent_ShortID { get; private set; }
        public uint ShortID { get; private set; }
        public uint RTPC_ShortID { get; private set; }
        public int Start_Line { get; private set; }
        public List<Layer_CrossFade> Children = new List<Layer_CrossFade>();
        public Layer_Relation(uint Parent_ShortID, uint ShortID, uint RTPC_ShortID, int Start_Line, List<Layer_CrossFade> Children)
        {
            this.Parent_ShortID = Parent_ShortID;
            this.ShortID = ShortID;
            this.RTPC_ShortID = RTPC_ShortID;
            this.Start_Line = Start_Line;
            this.Children = Children;
            GUID = Guid.NewGuid().ToString().ToUpper();
        }
    }
    public class BlendTracks
    {
        public static List<Layer_Relation> Layer_Info = new List<Layer_Relation>();
        public static void Init()
        {
            if (BNK_Info.Read_All.Count == 0)
                return;
            List<int> Switch_Numbers = new List<int>();
            List<uint> Temp = new List<uint>();
            //Blend Containerのみを抽出
            for (int Number = 0; Number < BNK_Info.ID_Line.Count; Number++)
            {
                if (BNK_Info.ID_Line[Number][2] == "CAkLayerCntr")
                    Switch_Numbers.Add(Number);
            }
            foreach (int Number in Switch_Numbers)
            {
                uint Parent_ShortID = uint.Parse(BNK_Info.ID_Line[Number][0]);
                int Start_Line = -1;
                //特定の文字がある行までスキップ
                for (int Number_01 = int.Parse(BNK_Info.ID_Line[Number][1]); Number_01 < BNK_Info.Read_All.Count; Number_01++)
                {
                    if (BNK_Info.Read_All[Number_01].Contains("<list name=\"pLayers\""))
                    {
                        Start_Line = Number_01;
                        break;
                    }
                    if (BNK_Info.Read_All[Number_01].Contains("type=\"u8\" name=\"eHircType\""))
                        break;
                }
                if (Start_Line == -1)
                    break;
                //レイヤー1つ1つの中の情報をリストに保存
                for (int Number_01 = Start_Line; Number_01 < BNK_Info.Read_All.Count; Number_01++)
                {
                    if (BNK_Info.Read_All[Number_01].Contains("<object name=\"CAkLayer\""))
                    {
                        uint Layer_ShortID = uint.Parse(Get_Config.Get_Property_Value(BNK_Info.Read_All[Number_01 + 1]));
                        uint RTPC_ShortID = 0;
                        for (int Number_02 = Number_01; Number_02 < BNK_Info.Read_All.Count; Number_02++)
                        {
                            if (BNK_Info.Read_All[Number_02].Contains("type=\"tid\" name=\"rtpcID\""))
                            {
                                RTPC_ShortID = uint.Parse(Get_Config.Get_Property_Value(BNK_Info.Read_All[Number_02]));
                                break;
                            }
                            if (BNK_Info.Read_All[Number_01].Contains("type=\"u8\" name=\"eHircType\""))
                                break;
                        }
                        if (RTPC_ShortID != 0)
                        {
                            List<Layer_CrossFade> CrossFades = new List<Layer_CrossFade>();
                            for (int Number_02 = Number_01 + 1; Number_02 < BNK_Info.Read_All.Count; Number_02++)
                            {
                                if (BNK_Info.Read_All[Number_02].Contains("<object name=\"CAssociatedChildData\""))
                                {
                                    uint Target_ShortID = uint.Parse(Get_Config.Get_Property_Value(BNK_Info.Read_All[Number_02 + 1]));
                                    int RTPC_Count = int.Parse(Get_Config.Get_Count(BNK_Info.Read_All[Number_02 + 3]));
                                    int Next_Index = 0;
                                    double Start_From = 0;
                                    double Start_To = 0;
                                    double End_From = 0;
                                    double End_To = 0;
                                    bool IsEnd = false;
                                    for (int Number_03 = Number_02; Number_03 < BNK_Info.Read_All.Count; Number_03++)
                                    {
                                        if (BNK_Info.Read_All[Number_03].Contains("</list>") || BNK_Info.Read_All[Number_01].Contains("type=\"u8\" name=\"eHircType\""))
                                            break;
                                        if (BNK_Info.Read_All[Number_03].Contains("<object name=\"AkRTPCGraphPoint\""))
                                        {
                                            int Index_Now = int.Parse(Get_Config.Get_Index(BNK_Info.Read_All[Number_03]));
                                            if (Index_Now == Next_Index)
                                            {
                                                Start_From = double.Parse(Get_Config.Get_Property_Value(BNK_Info.Read_All[Number_03 + 1]));
                                                Start_To = double.Parse(Get_Config.Get_Property_Value(BNK_Info.Read_All[Number_03 + 2]));
                                            }
                                            else if (Index_Now == Next_Index + 2 || Index_Now == RTPC_Count - 1)
                                            {
                                                End_From = double.Parse(Get_Config.Get_Property_Value(BNK_Info.Read_All[Number_03 + 1]));
                                                End_To = double.Parse(Get_Config.Get_Property_Value(BNK_Info.Read_All[Number_03 + 2]));
                                                Next_Index = Index_Now + 1;
                                                IsEnd = true;
                                            }
                                        }
                                        if (IsEnd)
                                        {
                                            CrossFades.Add(new Layer_CrossFade(Target_ShortID, Start_From, Start_To, End_From, End_To));
                                            Start_From = 0;
                                            Start_To = 0;
                                            End_From = 0;
                                            End_To = 0;
                                            IsEnd = false;
                                        }
                                    }
                                }
                                else if (BNK_Info.Read_All[Number_02].Contains("type=\"u8\" name=\"eHircType\"") || BNK_Info.Read_All[Number_02].Contains("<object name=\"CAkLayer\""))
                                    break;
                            }
                            Layer_Info.Add(new Layer_Relation(Parent_ShortID, Layer_ShortID, RTPC_ShortID, Number_01, CrossFades));
                        }
                    }
                    else if (BNK_Info.Read_All[Number_01].Contains("type=\"u8\" name=\"eHircType\""))
                        break;
                }
            }
            //情報を視覚的にするためファイルに保存
            /*List<string> Test = new List<string>();
            foreach (Layer_Relation Temp_01 in Layer_Info)
            {
                Test.Add("ShortID:" + Temp_01.ShortID);
                Test.Add("Parent_ShortID:" + Temp_01.Parent_ShortID);
                Test.Add("RTPC_ShortID:" + Temp_01.RTPC_ShortID);
                Test.Add("GUID:" + Temp_01.GUID);
                foreach (Layer_CrossFade Temp_02 in Temp_01.Children)
                {
                    Test.Add("    Target_ShortID:" + Temp_02.Target_ShortID);
                    Test.Add("    Start_From:" + Temp_02.Start_From);
                    Test.Add("    Start_To:" + Temp_02.Start_To);
                    Test.Add("    End_From:" + Temp_02.End_From);
                    Test.Add("    End_To:" + Temp_02.End_To);
                }
            }
            File.WriteAllLines(Voice_Set.Special_Path + "/Test.txt", Test);
            Test.Clear();*/
        }
    }
}