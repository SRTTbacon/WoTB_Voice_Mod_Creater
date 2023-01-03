using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WoTB_Voice_Mod_Creater.Wwise_Class.BNK_To_Wwise_Project
{
    //長々と書いていますが、基本的に各コンテナの内容を取得してWwiseプロジェクトに変換するコードです
    public class Actor_Mixer_Info
    {
        public string Type { get; private set; }
        public string GUID { get; private set; }
        public uint ShortID { get; private set; }
        public int Line { get; set; }
        public Actor_Mixer_Info(string Type, string GUID, uint ShortID, int Line)
        {
            this.Type = Type;
            this.GUID = GUID;
            this.ShortID = ShortID;
            this.Line = Line;
        }
    }
    //よく使う関数をまとめています
    public class Get_Config
    {
        //FNV_Hash_Classを初期化(ShortIDから文字列を作成します)
        static FNV_Hash_Class hasher = new FNV_Hash_Class();
        //value=の後の数字を取得
        public static string Get_Property_Value(string Read_Line)
        {
            string strValue = Read_Line.Substring(Read_Line.IndexOf("value=\"") + 7);
            return strValue.Substring(0, strValue.IndexOf("\""));
        }
        //name=の後の文字を取得
        public static string Get_Property_Name(string Read_Line)
        {
            string strValue = Read_Line.Substring(Read_Line.IndexOf("name=\"") + 6);
            return strValue.Substring(0, strValue.IndexOf("\""));
        }
        //Name=の後の文字を取得
        public static string Get_Name(string Read_Line)
        {
            string strValue = Read_Line.Substring(Read_Line.IndexOf("Name=\"") + 6);
            return strValue.Substring(0, strValue.IndexOf("\""));
        }
        //ShortID=の後の数字を取得
        public static string Get_ShortID_Project(string Read_Line)
        {
            string strValue = Read_Line.Substring(Read_Line.IndexOf("ShortID=\"") + 9);
            return strValue.Substring(0, strValue.IndexOf("\""));
        }
        //[]の中に入っている文字を取得
        public static string Get_Value_FMT(string Read_Line)
        {
            string strValue = Read_Line.Substring(Read_Line.IndexOf('[') + 1);
            return strValue.Substring(0, strValue.IndexOf(']'));
        }
        //index=の後の数字を取得
        public static string Get_Index(string Read_Line)
        {
            string strValue = Read_Line.Substring(Read_Line.IndexOf("index=\"") + 7);
            return strValue.Substring(0, strValue.IndexOf("\""));
        }
        //count=の後の数字を取得
        public static string Get_Count(string Read_Line)
        {
            string strValue = Read_Line.Substring(Read_Line.IndexOf("count=\"") + 7);
            return strValue.Substring(0, strValue.IndexOf("\""));
        }
        //hashname=の後の文字を取得
        public static string Get_HashName(string Read_Line)
        {
            string strValue = Read_Line.Substring(Read_Line.IndexOf("hashname=\"") + 10);
            return strValue.Substring(0, strValue.IndexOf("\""));
        }
        //説明が難しい...
        public static Property_Type Get_Property_Type(string Type_Name)
        {
            if (Type_Name == "Volume")
                return Property_Type.Real64;
            else if (Type_Name == "MakeUpGain")
                return Property_Type.Real64;
            else if (Type_Name == "Pitch")
                return Property_Type.int32;
            else if (Type_Name == "LPF")
                return Property_Type.int16;
            else if (Type_Name == "HPF")
                return Property_Type.int16;
            else if (Type_Name == "InitialDelay")
                return Property_Type.Real64;
            return Property_Type.Real64;
        }
        //説明が難しい...
        public static string Get_SegmentShape(string SegmentShape_Name)
        {
            if (SegmentShape_Name == "SineRecip")
                return "Exp2";
            else if (SegmentShape_Name == "InvSCurve")
                return "InvertedSCurve";
            else if (SegmentShape_Name == "Exp3/LastFadeCurve")
                return "Exp3";
            else if (SegmentShape_Name == "Sine")
                return "Log2";
            return SegmentShape_Name;
        }
        //ShortIDから文字列に変換(時間がかかります)
        public static string Get_Hash_Name_From_ShortID(uint ShortID)
        {
            string Parse = hasher.Bruteforce(8, ShortID);
            return Parse;
        }
    }
    public class Actor_Mixer
    {
        //親のGUIDを設定(これは固定なのでconstを指定)
        public const string Actor_Mixer_Unit = "DF3DA361-09F8-430D-814F-4080F28088DD";
        int Property_End_Line = -1;
        List<uint> RTPC_ShortID = new List<uint>();
        List<uint> Master_ShortID = new List<uint>();
        List<uint> Switch_Child_ShortID = new List<uint>();
        List<uint> Switch_Parent_ShortID = new List<uint>();
        System.Windows.Controls.TextBlock Message_T;
        public Actor_Mixer()
        {
            BNK_Info.Actor_Mixer_Hierarchy_Project.Clear();
            //親のShortIDが0でも実行できるようにあらかじめ追加しておく
            BNK_Info.Actor_Mixer_Child_Line.Add(new Actor_Mixer_Info("WorkUnit", Actor_Mixer_Unit, 0, 5));
        }
        public void Clear()
        {
            RTPC_ShortID.Clear();
            Master_ShortID.Clear();
            Switch_Child_ShortID.Clear();
            Switch_Parent_ShortID.Clear();
        }
        public async Task Get_Actor_Mixer_Hierarchy(string To_File, bool IsExtractSound, System.Windows.Controls.TextBlock TextBlock)
        {
            Message_T = TextBlock;
            Message_T.Text = "Actor_Mixerを変換しています...";
            await Task.Delay(50);
            //全プロジェクト共通の記述
            BNK_Info.Actor_Mixer_Hierarchy_Project.Add("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            BNK_Info.Actor_Mixer_Hierarchy_Project.Add("<WwiseDocument Type=\"WorkUnit\" ID=\"{" + Actor_Mixer_Unit + "}\" SchemaVersion=\"97\">");
            BNK_Info.Actor_Mixer_Hierarchy_Project.Add("<AudioObjects>");
            BNK_Info.Actor_Mixer_Hierarchy_Project.Add("<WorkUnit Name=\"Default Work Unit\" ID=\"{" + Actor_Mixer_Unit + "}\" PersistMode=\"Standalone\">");
            BNK_Info.Actor_Mixer_Hierarchy_Project.Add("<ChildrenList>");
            List<int> Not_Done_Number = new List<int>();
            for (int Number = 0; Number < BNK_Info.Relation.Count; Number++)
                Not_Done_Number.Add(Number);
            try
            {
                //この処理が終わったあと、なぜかクラッシュしてしまうため、一時的にtry catchにしておく
                //原因がわかり次第修正予定
                while (Not_Done_Number.Count > 0)
                {
                    bool IsExist = false;
                    foreach (int Number in Not_Done_Number)
                    {
                        bool IsOK = false;
                        foreach (Actor_Mixer_Info Temp_Relation in BNK_Info.Actor_Mixer_Child_Line)
                        {
                            //親のShortIDが既にプロジェクト内に存在していたら追加
                            if (BNK_Info.Relation[Number].Parent == Temp_Relation.ShortID)
                            {
                                Set_Actor_Mixer_Container(BNK_Info.Relation[Number], IsExtractSound);
                                Not_Done_Number.Remove(Number);
                                IsOK = true;
                                IsExist = true;
                                break;
                            }
                        }
                        if (IsOK)
                            break;
                    }
                    if (!IsExist)
                        break;
                }
            }
            catch {}
            if (!Directory.Exists(Path.GetDirectoryName(To_File)))
                Directory.CreateDirectory(Path.GetDirectoryName(To_File));
            Not_Done_Number.Clear();
            More_Class.List_Init(BNK_Info.Actor_Mixer_Hierarchy_Project.Count);
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</ChildrenList>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</WorkUnit>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</AudioObjects>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</WwiseDocument>");
            Message_T.Text = "Actor_Mixerを変換しました。";
            await Task.Delay(50);
            //RTPCのShortIDを文字列に変換
            if (RTPC_ShortID.Count > 0)
            {
                foreach (uint ShortID_Now in RTPC_ShortID)
                {
                    int RTPC_Info_Number = -1;
                    for (int Number = 0; Number < BNK_Info.RTPC_Info.Count; Number++)
                    {
                        if (BNK_Info.RTPC_Info[Number].ShortID == ShortID_Now && BNK_Info.RTPC_Info[Number].Name == null)
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
                        BNK_Info.RTPC_Info[RTPC_Info_Number].Name = Name;
                    }
                }
                for (int Number = 0; Number < BNK_Info.Actor_Mixer_Hierarchy_Project.Count; Number++)
                {
                    if (BNK_Info.Actor_Mixer_Hierarchy_Project[Number].Contains("<ObjectRef Name=\""))
                    {
                        foreach (RTPC_Relation RTPC_Now in BNK_Info.RTPC_Info)
                        {
                            if (BNK_Info.Actor_Mixer_Hierarchy_Project[Number].Contains(RTPC_Now.GUID))
                            {
                                BNK_Info.Actor_Mixer_Hierarchy_Project[Number] =
                                    "<ObjectRef Name=\"" + RTPC_Now.Name + "\" ID=\"{" + RTPC_Now.GUID + "}\" WorkUnitID=\"{" + BNK_Info.Parent_RTPC_WorkUnit + "}\"/>";
                                break;
                            }
                        }
                    }
                }
                Message_T.Text = "RTPCを適応しました。";
                await Task.Delay(50);
            }
            //Master Audio BusのShortIDを文字列に変換
            if (Master_ShortID.Count > 0)
            {
                foreach (uint ShortID_Now in Master_ShortID)
                {
                    int Master_Bus_Info_Number = -1;
                    for (int Number = 0; Number < Master_Mixer.Master_Audio_Info.Count; Number++)
                    {
                        if (Master_Mixer.Master_Audio_Info[Number].ShortID == ShortID_Now)
                        {
                            Master_Bus_Info_Number = Number;
                            break;
                        }
                    }
                    Message_T.Text = ShortID_Now + "を対応する文字列へ変換しています...";
                    await Task.Delay(50);
                    if (Master_Bus_Info_Number != -1)
                    {
                        string Name = Get_Config.Get_Hash_Name_From_ShortID(ShortID_Now);
                        Master_Mixer.Master_Audio_Info[Master_Bus_Info_Number].Name = Name;
                    }
                }
                for (int Number = 0; Number < BNK_Info.Actor_Mixer_Hierarchy_Project.Count; Number++)
                {
                    if (BNK_Info.Actor_Mixer_Hierarchy_Project[Number].Contains("<ObjectRef Name=\""))
                    {
                        foreach (Master_Audio_Relation Master_Bus_Now in Master_Mixer.Master_Audio_Info)
                        {
                            if (BNK_Info.Actor_Mixer_Hierarchy_Project[Number].Contains(Master_Bus_Now.GUID))
                            {
                                BNK_Info.Actor_Mixer_Hierarchy_Project[Number] =
                                    "<ObjectRef Name=\"" + Master_Bus_Now.Name + "\" ID=\"{" + Master_Bus_Now.GUID + "}\" WorkUnitID=\"{" + Master_Mixer.Master_Bus_Unit + "}\"/>";
                                break;
                            }
                        }
                    }
                }
                Message_T.Text = "Master Audio Busを適応しました。";
                await Task.Delay(50);
            }
            await Set_Switch_Name();
            File.WriteAllLines(To_File, BNK_Info.Actor_Mixer_Hierarchy_Project);
            BNK_Info.Actor_Mixer_Hierarchy_Project.Clear();
        }
        async Task Set_Switch_Name()
        {
            //Switch(親)のShortIDを文字列に変換
            if (Switch_Parent_ShortID.Count > 0)
            {
                foreach (uint ShortID_Now in Switch_Parent_ShortID)
                {
                    int Switch_Info_Number = -1;
                    for (int Number = 0; Number < Switch.Switch_Info.Count; Number++)
                    {
                        if (Switch.Switch_Info[Number].ShortID == ShortID_Now && Switch.Switch_Info[Number].Name == null)
                        {
                            Switch_Info_Number = Number;
                            break;
                        }
                    }
                    Message_T.Text = ShortID_Now + "を対応する文字列へ変換しています...";
                    await Task.Delay(50);
                    if (Switch_Info_Number != -1)
                    {
                        string Name = Get_Config.Get_Hash_Name_From_ShortID(ShortID_Now);
                        Switch.Switch_Info[Switch_Info_Number].Name = Name;
                    }
                }
                for (int Number = 0; Number < BNK_Info.Actor_Mixer_Hierarchy_Project.Count; Number++)
                {
                    if (BNK_Info.Actor_Mixer_Hierarchy_Project[Number].Contains("<ObjectRef Name=\""))
                    {
                        foreach (Switch_Relation Switch_Now in Switch.Switch_Info)
                        {
                            if (BNK_Info.Actor_Mixer_Hierarchy_Project[Number].Contains(Switch_Now.GUID))
                            {
                                BNK_Info.Actor_Mixer_Hierarchy_Project[Number] =
                                    "<ObjectRef Name=\"" + Switch_Now.Name + "\" ID=\"{" + Switch_Now.GUID + "}\" WorkUnitID=\"{" + Switch.Work_Unit_GUID + "}\"/>";
                                break;
                            }
                        }
                    }
                }
            }
            //Switch(子)のShortIDを文字列に変換
            if (Switch_Child_ShortID.Count > 0)
            {
                foreach (uint ShortID_Now in Switch_Child_ShortID)
                {
                    int Switch_Info_Number = -1;
                    int Switch_Info_Child_Number = -1;
                    for (int Number = 0; Number < Switch.Switch_Info.Count; Number++)
                    {
                        for (int Number_01 = 0; Number_01 < Switch.Switch_Info[Number].Children.Count; Number_01++)
                        {
                            if (Switch.Switch_Info[Number].Children[Number_01].ShortID == ShortID_Now && Switch.Switch_Info[Number].Name == null)
                            {
                                Switch_Info_Number = Number;
                                Switch_Info_Child_Number = Number_01;
                                break;
                            }
                        }
                    }
                    Message_T.Text = ShortID_Now + "を対応する文字列へ変換しています...";
                    await Task.Delay(50);
                    if (Switch_Info_Number != -1 && Switch_Info_Child_Number != -1)
                    {
                        string Name = Get_Config.Get_Hash_Name_From_ShortID(ShortID_Now);
                        Switch.Switch_Info[Switch_Info_Number].Children[Switch_Info_Child_Number].Name = Name;
                    }
                }
                for (int Number = 0; Number < BNK_Info.Actor_Mixer_Hierarchy_Project.Count; Number++)
                {
                    if (BNK_Info.Actor_Mixer_Hierarchy_Project[Number].Contains("<SwitchRef Name=\""))
                    {
                        foreach (Switch_Relation Switch_Now in Switch.Switch_Info)
                        {
                            foreach (Switch_Child_Relation Switch_Child_Now in Switch_Now.Children)
                            {
                                if (BNK_Info.Actor_Mixer_Hierarchy_Project[Number].Contains(Switch_Child_Now.GUID))
                                {
                                    BNK_Info.Actor_Mixer_Hierarchy_Project[Number] =
                                        "<SwitchRef Name=\"" + Switch_Child_Now.Name + "\" ID=\"{" + Switch_Child_Now.GUID + "}\"/>";
                                    break;
                                }
                            }
                        }
                    }
                }
                Message_T.Text = "Switchを適応しました。";
                await Task.Delay(50);
            }
        }
        //Get_Actor_Mixer_Historyから呼ばれます
        void Set_Actor_Mixer_Container(BNK_Relation Temp_Relation, bool IsExtractSound)
        {
            int From_Line = Temp_Relation.Line;
            int To_Line;
            int Temp_Line = Temp_Relation.Line;
            uint OverWriteParent = Temp_Relation.Parent;
            bool IsNoParent = Temp_Relation.Parent == 0 ? true : false;
            while (true)
            {
                Temp_Line++;
                if (BNK_Info.Read_All.Count - 1 == Temp_Line)
                {
                    To_Line = Temp_Line - 1;
                    break;
                }
                if (BNK_Info.Read_All[Temp_Line].Contains("type=\"u8\" name=\"eHircType\""))
                {
                    To_Line = Temp_Line - 4;
                    break;
                }
            }
            int Write_Start_Line = -1;
            //親がない場合5行目に追加
            if (IsNoParent)
                Write_Start_Line = 5;
            else
            {
                //親の<ChildrenList>の場所に移動
                int Temp_Start_Line = 0;
                string Type_Temp = "";
                foreach (Actor_Mixer_Info Temp in BNK_Info.Actor_Mixer_Child_Line)
                {
                    if (Temp.ShortID == OverWriteParent)
                    {
                        for (int Number_01 = 0; Number_01 < BNK_Info.Actor_Mixer_Hierarchy_Project.Count; Number_01++)
                        {
                            if (BNK_Info.Actor_Mixer_Hierarchy_Project[Number_01].Contains("<" + Temp.Type + " Name=\"" + Temp.ShortID + "\" ID=\"{" + Temp.GUID + "}\" ShortID=\"" + Temp.ShortID + "\">"))
                            {
                                Temp_Start_Line = Number_01;
                                Type_Temp = Temp.Type;
                            }
                        }
                        break;
                    }
                }
                if (Temp_Start_Line == 0 || Type_Temp == "")
                    return;
                for (int Number_01 = Temp_Start_Line; Number_01 < BNK_Info.Actor_Mixer_Hierarchy_Project.Count; Number_01++)
                {
                    if (BNK_Info.Actor_Mixer_Hierarchy_Project[Number_01].Contains("<ChildrenList>"))
                    {
                        Write_Start_Line = Number_01 + 1;
                        break;
                    }
                    else if (BNK_Info.Actor_Mixer_Hierarchy_Project[Number_01].Contains("</" + Type_Temp + ">"))
                        break;
                }
            }
            if (Write_Start_Line == -1)
                return;
            //コンテナ名に合わせて処理を変更
            CAkType Type = Temp_Relation.Type;
            More_Class.List_Init(Write_Start_Line);
            if (Type == CAkType.CAkLayerCntr)
                Add_CAkLayerCntr(Temp_Relation, From_Line, To_Line);
            else if (Type == CAkType.CAkActorMixer)
                Add_CAkActorCntr(Temp_Relation, From_Line, To_Line);
            else if (Type == CAkType.CAkRanSeqCntr)
                Add_CAkRanSeqCntr(Temp_Relation, From_Line, To_Line);
            else if (Type == CAkType.CAkSwitchCntr)
                Add_CAkSwitchCntr(Temp_Relation, From_Line, To_Line);
            else if (Type == CAkType.CAkSound)
            {
                int Temp_Line_02 = -1;
                foreach (List<string> Temp in BNK_Info.ID_Line)
                {
                    if (Temp[0] == Temp_Relation.My.ToString())
                    {
                        Temp_Line_02 = int.Parse(Temp[1]);
                        break;
                    }
                }
                if (Temp_Line_02 == -1)
                    return;
                Add_CAkSound(Temp_Relation, Temp_Line_02, To_Line, IsExtractSound);
            }
        }
        //Blend Containerを設定
        void Add_CAkLayerCntr(BNK_Relation Temp_Relation, int From_Line, int To_Line)
        {
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<BlendContainer Name=\"" + Temp_Relation.My + "\" ID=\"{" + Temp_Relation.GUID + "}\" ShortID=\"" + Temp_Relation.My + "\">");
            Add_CAkCommon(Temp_Relation, From_Line, To_Line, true, "BlendContainer");
            int Start_Line = More_Class.List_Get_Line();
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</BlendContainer>");
            bool IsExist = false;
            foreach (Layer_Relation Relation in BlendTracks.Layer_Info)
            {
                if (Relation.Parent_ShortID == Temp_Relation.My)
                {
                    IsExist = true;
                    break;
                }
            }
            if (IsExist)
            {
                More_Class.List_Init(Start_Line);
                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<BlendTrackList>");
                foreach (Layer_Relation Relation in BlendTracks.Layer_Info)
                {
                    if (Relation.Parent_ShortID == Temp_Relation.My)
                    {
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<BlendTrack Name=\"" + Relation.ShortID + "\" ID=\"{" + Relation.GUID + "}\" ShortID=\"" + Relation.ShortID + "\">");
                        if (Relation.Children.Count > 0)
                        {
                            int RTPCNumber = -1;
                            for (int Number = 0; Number < BNK_Info.RTPC_Info.Count; Number++)
                            {
                                if (BNK_Info.RTPC_Info[Number].ShortID == Relation.RTPC_ShortID)
                                {
                                    RTPCNumber = Number;
                                    break;
                                }
                            }
                            bool IsExistBlendTrack = false;
                            if (RTPCNumber != -1)
                            {
                                int RTPC_Start_Line = More_Class.List_Get_Line();
                                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<PropertyList>");
                                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"EnableCrossFading\" Type=\"bool\" Value=\"True\"/>");
                                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</PropertyList>");
                                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<ReferenceList>");
                                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Reference Name=\"LayerCrossFadeControlInput\">");
                                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<ObjectRef Name=\"" + BNK_Info.RTPC_Info[RTPCNumber].Name + "\" ID=\"{" + BNK_Info.RTPC_Info[RTPCNumber].GUID + "}\" WorkUnitID=\"{" + BNK_Info.Parent_RTPC_WorkUnit + "}\"/>");
                                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</Reference>");
                                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</ReferenceList>");
                                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<BlendTrackAssocList>");
                                List<int> Numbers = new List<int>();
                                for (int Number = 0; Number < Relation.Children.Count; Number++)
                                    Numbers.Add(Number);
                                while (Numbers.Count > 0)
                                {
                                    List<double> Start_From_List = new List<double>();
                                    foreach (int Number in Numbers)
                                        Start_From_List.Add(Relation.Children[Number].Start_From);
                                    double Min = Start_From_List.Min();
                                    Start_From_List.Clear();
                                    foreach (int Number in Numbers)
                                    {
                                        if (Relation.Children[Number].Start_From == Min)
                                        {
                                            string Target_GUID = "";
                                            foreach (BNK_Relation Temp in BNK_Info.Relation)
                                            {
                                                if (Temp.My == Relation.Children[Number].Target_ShortID)
                                                {
                                                    Target_GUID = Temp.GUID;
                                                    break;
                                                }
                                            }
                                            if (Target_GUID == "")
                                                continue;
                                            Numbers.Remove(Number);
                                            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<BlendTrackAssoc>");
                                            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<ItemRef Name=\"" + Relation.Children[Number].Target_ShortID + "\" ID=\"{" + Target_GUID + "}\"/>");
                                            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<CrossfadingInfo>");
                                            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<LeftEdgePos>" + Relation.Children[Number].Start_From + "</LeftEdgePos>");
                                            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<LeftFadingMode>Automatic</LeftFadingMode>");
                                            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<RightEdgePos>" + Relation.Children[Number].End_From + "</RightEdgePos>");
                                            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<RightFadingMode>Automatic</RightFadingMode>");
                                            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</CrossfadingInfo>");
                                            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</BlendTrackAssoc>");
                                            IsExistBlendTrack = true;
                                            break;
                                        }
                                    }
                                }
                                Numbers.Clear();
                                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</BlendTrackAssocList>");
                                if (IsExistBlendTrack)
                                {
                                    for (int Number = Relation.Start_Line; Number < BNK_Info.Read_All.Count; Number++)
                                    {
                                        if (BNK_Info.Read_All[Number].Contains("<object name=\"RTPC\" index=\""))
                                            Add_Property_RTPC(Temp_Relation, Number, RTPC_Start_Line);
                                        else if (BNK_Info.Read_All[Number].Contains("type=\"tid\" name=\"rtpcID\""))
                                            break;
                                    }
                                    for (int Number = RTPC_Start_Line; Number < BNK_Info.Actor_Mixer_Hierarchy_Project.Count; Number++)
                                    {
                                        if (BNK_Info.Actor_Mixer_Hierarchy_Project[Number].Contains("</BlendTrackAssocList>"))
                                        {
                                            More_Class.List_Init(Number + 1);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</BlendTrack>");
                    }
                }
                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</BlendTrackList>");
            }
        }
        //Actor_Mixer Containerを設定
        void Add_CAkActorCntr(BNK_Relation Temp_Relation, int From_Line, int To_Line)
        {
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<ActorMixer Name=\"" + Temp_Relation.My + "\" ID=\"{" + Temp_Relation.GUID + "}\" ShortID=\"" + Temp_Relation.My + "\">");
            Add_CAkCommon(Temp_Relation, From_Line, To_Line, false, "ActorMixer");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</ActorMixer>");
        }
        //Random Containerを設定
        void Add_CAkRanSeqCntr(BNK_Relation Temp_Relation, int From_Line, int To_Line)
        {
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<RandomSequenceContainer Name=\"" + Temp_Relation.My + "\" ID=\"{" + Temp_Relation.GUID + "}\" ShortID=\"" + Temp_Relation.My + "\">");
            Add_CAkCommon(Temp_Relation, From_Line, To_Line, true, "RandomSequenceContainer");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</RandomSequenceContainer>");
        }
        //Switch Containerを設定
        void Add_CAkSwitchCntr(BNK_Relation Temp_Relation, int From_Line, int To_Line)
        {
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<SwitchContainer Name=\"" + Temp_Relation.My + "\" ID=\"{" + Temp_Relation.GUID + "}\" ShortID=\"" + Temp_Relation.My + "\">");
            Add_CAkCommon(Temp_Relation, From_Line, To_Line, true, "SwitchContainer");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</SwitchContainer>");
            int Line_Start = -1;
            for (int Number = 5; Number < BNK_Info.Actor_Mixer_Hierarchy_Project.Count; Number++)
            {
                if (BNK_Info.Actor_Mixer_Hierarchy_Project[Number].Contains("Name=\"") && BNK_Info.Actor_Mixer_Hierarchy_Project[Number].Contains("ShortID=\""))
                {
                    if (Get_Config.Get_ShortID_Project(BNK_Info.Actor_Mixer_Hierarchy_Project[Number]) == Temp_Relation.My.ToString())
                        Line_Start = Number + 1;
                }
            }
            if (Line_Start == -1)
                return;
            int Write_Line = 0;
            int Line_Start_Temp = Line_Start;
            //どこに記述するかを取得
            while (true)
            {
                string Read_Line = BNK_Info.Actor_Mixer_Hierarchy_Project[Line_Start];
                if (Read_Line.Contains("<ReferenceList>") && !BNK_Info.Actor_Mixer_Hierarchy_Project[Line_Start - 1].Contains("<RTPC"))
                {
                    Write_Line = Line_Start + 1;
                    break;
                }
                Line_Start++;
            }
            Switch_Relation Selected_Switch = null;
            List<Switch_Child_Container> Switch_Child_Container = new List<Switch_Child_Container>();
            More_Class.List_Init(Write_Line);
            uint Parent_ShortID = 0;
            for (int Number = From_Line; Number <= To_Line; Number++)
            {
                string Read_Line = BNK_Info.Read_All[Number];
                if (Read_Line.Contains("type=\"tid\" name=\"ulGroupID\""))
                {
                    uint ShortID = uint.Parse(Get_Config.Get_Property_Value(Read_Line));
                    foreach (Switch_Relation Temp in Switch.Switch_Info)
                    {
                        if (Temp.ShortID == ShortID)
                        {
                            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Reference Name=\"SwitchGroupOrStateGroup\">");
                            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<ObjectRef Name=\"" + Temp.ShortID + "\" ID=\"{" + Temp.GUID + "}\" WorkUnitID=\"{" + Switch.Work_Unit_GUID + "}\"/>");
                            if (!Switch_Parent_ShortID.Contains(Temp.ShortID))
                                Switch_Parent_ShortID.Add(Temp.ShortID);
                            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</Reference>");
                            Parent_ShortID = Temp.ShortID;
                            Selected_Switch = Temp;
                            for (int Number_01 = 0; Number_01 < BNK_Info.Actor_Mixer_Child_Line.Count; Number_01++)
                            {
                                if (BNK_Info.Actor_Mixer_Child_Line[Number_01].ShortID == Temp_Relation.My)
                                {
                                    BNK_Info.Actor_Mixer_Child_Line[Number_01].Line = BNK_Info.Actor_Mixer_Child_Line[Number_01].Line + 3;
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }
                if (Read_Line.Contains("<object name=\"CAkSwitchPackage\" index=\""))
                {
                    uint Child_Short_ID = 0;
                    List<uint> Child_Containers = new List<uint>();
                    if (BNK_Info.Read_All[Number + 1].Contains("name=\"ulSwitchID\""))
                        Child_Short_ID = uint.Parse(Get_Config.Get_Property_Value(BNK_Info.Read_All[Number + 1]));
                    if (Child_Short_ID != 0)
                    {
                        foreach (Switch_Relation Switch_Parent in Switch.Switch_Info)
                        {
                            bool IsExist = false;
                            foreach (Switch_Child_Relation Switch_Child in Switch_Parent.Children)
                            {
                                if (Switch_Child.ShortID == Child_Short_ID)
                                {
                                    for (int Number_01 = Number; Number_01 < BNK_Info.Read_All.Count; Number_01++)
                                    {
                                        if (BNK_Info.Read_All[Number_01].Contains("</object>"))
                                            break;
                                        if (BNK_Info.Read_All[Number_01].Contains("name=\"NodeID\""))
                                            Child_Containers.Add(uint.Parse(Get_Config.Get_Property_Value(BNK_Info.Read_All[Number_01])));
                                    }
                                    Switch_Child_Container.Add(new BNK_To_Wwise_Project.Switch_Child_Container(Switch_Child.GUID, Parent_ShortID, Child_Short_ID, Child_Containers));
                                    IsExist = true;
                                    break;
                                }
                            }
                            if (IsExist)
                                break;
                        }
                    }
                    Child_Containers.Clear();
                }
            }
            for (int Number = 5; Number < BNK_Info.Actor_Mixer_Hierarchy_Project.Count; Number++)
            {
                if (BNK_Info.Actor_Mixer_Hierarchy_Project[Number].Contains("Name=\"") && BNK_Info.Actor_Mixer_Hierarchy_Project[Number].Contains("ShortID=\""))
                {
                    if (Get_Config.Get_ShortID_Project(BNK_Info.Actor_Mixer_Hierarchy_Project[Number]) == Temp_Relation.My.ToString())
                        Line_Start = Number + 1;
                }
            }
            if (Line_Start == -1)
                return;
            while (true)
            {
                string Read_Line = BNK_Info.Actor_Mixer_Hierarchy_Project[Line_Start];
                if (Read_Line.Contains("/SwitchContainer"))
                {
                    Write_Line = Line_Start;
                    break;
                }
                Line_Start++;
            }
            More_Class.List_Init(Write_Line);
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<GroupingInfo>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<GroupingBehaviorList>");
            foreach (BNK_Relation Temp in BNK_Info.Relation)
            {
                if (Temp.Parent == Temp_Relation.My)
                {
                    More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<GroupingBehavior>");
                    More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<ItemRef Name=\"" + Temp.My + "\" ID=\"{" + Temp.GUID + "}\"/>");
                    More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</GroupingBehavior>");
                }
            }
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</GroupingBehaviorList>");
            if (Selected_Switch != null)
            {
                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<GroupingList>");
                foreach (Switch_Child_Container Temp in Switch_Child_Container)
                {
                    uint Child_ShortID = 0;
                    bool IsExist = false;
                    More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Grouping>");
                    foreach (Switch_Relation Temp_01 in Switch.Switch_Info)
                    {
                        if (Temp_01.ShortID == Temp.Parent_ShortID)
                        {
                            foreach (Switch_Child_Relation Temp_02 in Temp_01.Children)
                            {
                                if (Temp_02.ShortID == Temp.ShortID)
                                {
                                    More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<SwitchRef Name=\"" + Temp_02.ShortID + "\" ID=\"{" + Temp_02.GUID + "}\"/>");
                                    Child_ShortID = Temp_02.ShortID;
                                    IsExist = true;
                                    break;
                                }
                            }
                            if (IsExist)
                                break;
                        }
                    }
                    if (Child_ShortID == 0)
                    {
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<SwitchRef Name=\"" + Temp.ShortID + "\" ID=\"{" + Temp.GUID + "}\"/>");
                        Child_ShortID = Temp.ShortID;
                    }
                    if (!Switch_Child_ShortID.Contains(Child_ShortID))
                        Switch_Child_ShortID.Add(Child_ShortID);
                    More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<ItemList>");
                    foreach (uint ShortID in Temp.Containers)
                    {
                        string GUID = "";
                        //foreach多用して大丈夫かな...(処理速度的に)
                        foreach (BNK_Relation Temp_01 in BNK_Info.Relation)
                        {
                            if (Temp_01.My == ShortID)
                            {
                                GUID = Temp_01.GUID;
                                break;
                            }
                        }
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<ItemRef Name=\"" + ShortID + "\" ID=\"{" + GUID + "}\"/>");
                    }
                    More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</ItemList>");
                    More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</Grouping>");
                }
                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</GroupingList>");
            }
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</GroupingInfo>");
            Switch_Child_Container.Clear();
        }
        //Soundを設定
        void Add_CAkSound(BNK_Relation Temp_Relation, int From_Line, int To_Line, bool IsExtractSound)
        {
            string GetCAkSoundLanguage = "";
            string GetCAkSoundMediaID = "";
            for (int Number = From_Line; Number <= To_Line; Number++)
            {
                if (BNK_Info.Read_All[Number].Contains("name=\"sourceID\""))
                    GetCAkSoundMediaID = Get_Config.Get_Property_Value(BNK_Info.Read_All[Number]);
                else if (BNK_Info.Read_All[Number].Contains("name=\"bIsLanguageSpecific\""))
                {
                    string Value = Get_Config.Get_Property_Value(BNK_Info.Read_All[Number]);
                    if (Value == "0")
                        GetCAkSoundLanguage = "SFX";
                    else
                        GetCAkSoundLanguage = "ja";
                }
            }
            if (IsExtractSound)
            {
                bool IsExistSound = false;
                foreach (Wwise_File_Extract_V2 Wwise in BNK_Info.BNK_File)
                {
                    List<uint> Files = Wwise.Wwise_Get_IDs();
                    if (Files.Contains(uint.Parse(GetCAkSoundMediaID)))
                    {
                        IsExistSound = true;
                        break;
                    }
                    Files.Clear();
                }
                if (!IsExistSound)
                {
                    if (GetCAkSoundLanguage == "SFX")
                    {
                        if (!BNK_Info.Add_Empty_Wav_Files.Contains("\\Originals\\SFX\\" + GetCAkSoundMediaID + ".wav"))
                            BNK_Info.Add_Empty_Wav_Files.Add("\\Originals\\SFX\\" + GetCAkSoundMediaID + ".wav");
                    }
                    else
                    {
                        if (!BNK_Info.Add_Empty_Wav_Files.Contains("\\Originals\\Voices\\" + GetCAkSoundLanguage + "\\" + GetCAkSoundMediaID + ".wav"))
                            BNK_Info.Add_Empty_Wav_Files.Add("\\Originals\\Voices\\" + GetCAkSoundLanguage + "\\" + GetCAkSoundMediaID + ".wav");
                    }
                }
            }
            string Temp_GUID_02 = Guid.NewGuid().ToString().ToUpper();
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Sound Name=\"" + Temp_Relation.Name + "\" ID=\"{" + Temp_Relation.GUID + "}\" ShortID=\"" + Temp_Relation.My + "\">");
            int Property_Start_Line = More_Class.List_Get_Line();
            Add_CAkCommon(Temp_Relation, From_Line, To_Line, true, "Sound", false);
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<AudioFileSource Name=\"" + GetCAkSoundMediaID + "\" ID=\"{" + Temp_GUID_02 + "}\">");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Language>" + GetCAkSoundLanguage + "</Language>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<AudioFile>" + GetCAkSoundMediaID + ".wav</AudioFile>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<MediaIDList>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<MediaID ID=\"" + GetCAkSoundMediaID + "\"/>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</MediaIDList>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</AudioFileSource>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</ChildrenList>");
            Add_Property_State(More_Class.List_Get_Line(), From_Line, To_Line);
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<ActiveSourceList>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<ActiveSource Name=\"" + GetCAkSoundMediaID + "\" ID=\"{" + Temp_GUID_02 + "}\" Platform=\"Linked\"/>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</ActiveSourceList>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</Sound>");
            More_Class.List_Init(Property_Start_Line + 1);
            if (GetCAkSoundLanguage != "SFX")
                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"IsVoice\" Type=\"bool\" Value=\"True\"/>");
            uint MediaID_Parse = uint.Parse(GetCAkSoundMediaID);
            bool IsExist = false;
            foreach (CAkSound Temp in BNK_Info.CAkSound_Info)
            {
                if (Temp.Language == GetCAkSoundLanguage && Temp.ShortID == MediaID_Parse)
                {
                    IsExist = true;
                    break;
                }
            }
            if (!IsExist)
                BNK_Info.CAkSound_Info.Add(new CAkSound(GetCAkSoundLanguage, MediaID_Parse));
        }
        //どのコンテナでも共通で記述される文字を追加
        void Add_CAkCommon(BNK_Relation Temp_Relation, int From_Line, int To_Line, bool Include_Option, string Type_Name_Project, bool IsChildrenEndLine = true)
        {
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<PropertyList>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</PropertyList>");
            int Add_Property_Mode = 0;
            Property_End_Line = More_Class.List_Get_Line();
            bool IsExistAttenuations = false;
            uint Attenuation_ShortID = 0;
            Master_Audio_Relation Output_Bus = null;
            bool IsExistPanner = false;
            double Pan_LR = 0;
            double Pan_FR = 0;
            int SpeakerPanning_Volume = 100;
            bool IsOverWriteParentMaxSoundInstance = false;
            bool IsOverWritePriority = false;
            bool IsOverWriteVirtualVoice = false;
            //音量やピッチなどの情報を取得し、Wwiseプロジェクト用に変換
            for (int Number = From_Line; Number <= To_Line; Number++)
            {
                //以下の文字で初期値か範囲値かを分ける
                string Read_Line = BNK_Info.Read_All[Number];
                //特定の文字列までいったらAdd_Property_Modeの値を変更
                if (Read_Line.Contains("<object name=\"AkPropBundle&lt;AkPropValue,unsigned char&gt;\">"))
                    Add_Property_Mode = 1;
                else if (Read_Line.Contains("<object name=\"AkPropBundle&lt;RANGED_MODIFIERS&lt;AkPropValue&gt;&gt;\">"))
                    Add_Property_Mode = 2;
                else if (Read_Line.Contains("<object name=\"AdvSettingsParams\">"))
                    Add_Property_Mode = 3;
                else if (Read_Line.Contains("<object name=\"InitialRTPC\">"))
                    Add_Property_Mode = 4;
                //初期値を取得して入力(主にランダムコンテナなどで初回再生時の音量やピッチを変更します)
                if (Add_Property_Mode == 1)
                {
                    if (Read_Line.Contains("valuefmt=\"0x00 [Volume]\""))
                    {
                        Add_PropertyList(Temp_Relation, "Volume", Get_Config.Get_Property_Value(BNK_Info.Read_All[Number + 1]), Property_Type.Real64, false);
                        continue;
                    }
                    else if (Read_Line.Contains("valuefmt=\"0x06 [MakeUpGain]\""))
                    {
                        Add_PropertyList(Temp_Relation, "MakeUpGain", Get_Config.Get_Property_Value(BNK_Info.Read_All[Number + 1]), Property_Type.Real64, false);
                        continue;
                    }
                    else if (Read_Line.Contains("valuefmt=\"0x02 [Pitch]\""))
                    {
                        Add_PropertyList(Temp_Relation, "Pitch", Get_Config.Get_Property_Value(BNK_Info.Read_All[Number + 1]), Property_Type.int32, true);
                        continue;
                    }
                    else if (Read_Line.Contains("valuefmt=\"0x03 [LPF]\""))
                    {
                        Add_PropertyList(Temp_Relation, "Lowpass", Get_Config.Get_Property_Value(BNK_Info.Read_All[Number + 1]), Property_Type.int16, true);
                        continue;
                    }
                    else if (Read_Line.Contains("valuefmt=\"0x04 [HPF]\""))
                    {
                        Add_PropertyList(Temp_Relation, "Highpass", Get_Config.Get_Property_Value(BNK_Info.Read_All[Number + 1]), Property_Type.int16, true);
                        continue;
                    }
                    else if (Read_Line.Contains("valuefmt=\"0x3B [InitialDelay]\"") && Include_Option)
                    {
                        Add_PropertyList(Temp_Relation, "InitialDelay", Get_Config.Get_Property_Value(BNK_Info.Read_All[Number + 1]), Property_Type.Real64,false);
                        continue;
                    }
                    else if (Read_Line.Contains("valuefmt=\"0x07 [StatePropNum/Priority]\""))
                    {
                        Add_PropertyList(Temp_Relation, "Priority", Get_Config.Get_Property_Value(BNK_Info.Read_All[Number + 1]), Property_Type.int16, true);
                        IsOverWritePriority = true;
                        continue;
                    }
                    else if (Read_Line.Contains("valuefmt=\"0x08 [PriorityDistanceOffset]\""))
                    {
                        Add_PropertyList(Temp_Relation, "PriorityDistanceOffset", Get_Config.Get_Property_Value(BNK_Info.Read_All[Number + 1]), Property_Type.int16, true);
                        IsOverWritePriority = true;
                        continue;
                    }
                    else if (Read_Line.Contains("valuefmt=\"0x46 [AttenuationID]\""))
                    {
                        IsExistAttenuations = true;
                        string Value = Get_Config.Get_Property_Value(BNK_Info.Read_All[Number + 1]);
                        if (!Value.Contains("."))
                            Attenuation_ShortID = uint.Parse(Value);
                        continue;
                    }
                    else if (Read_Line.Contains("valuefmt=\"0x0C [PAN_LR]\""))
                    {
                        Pan_LR = double.Parse(Get_Config.Get_Property_Value(BNK_Info.Read_All[Number + 1]));
                        continue;
                    }
                    else if (Read_Line.Contains("valuefmt=\"0x0D [PAN_FR]\""))
                    {
                        Pan_FR = double.Parse(Get_Config.Get_Property_Value(BNK_Info.Read_All[Number + 1]));
                        continue;
                    }
                    else if (Read_Line.Contains("valuefmt=\"0x47 [PositioningTypeBlend]\""))
                    {
                        SpeakerPanning_Volume = int.Parse(Get_Config.Get_Property_Value(BNK_Info.Read_All[Number]));
                        continue;
                    }
                }
                //範囲値を取得して入力(主にランダムコンテナなどで再生される度に音量やピッチが異なって聞こえるようにするやつです)
                else if (Add_Property_Mode == 2)
                {
                    //範囲値の場合のProperty_Typeはなぜか全部Real64
                    if (Read_Line.Contains("valuefmt=\"0x00 [Volume]\""))
                    {
                        Add_Property_Range(Temp_Relation, "Volume", Get_Config.Get_Property_Value(BNK_Info.Read_All[Number + 1]), Get_Config.Get_Property_Value(BNK_Info.Read_All[Number + 2]), Property_Type.Real64);
                        continue;
                    }
                    else if (Read_Line.Contains("valuefmt=\"0x06 [MakeUpGain]\""))
                    {
                        Add_Property_Range(Temp_Relation, "MakeUpGain", Get_Config.Get_Property_Value(BNK_Info.Read_All[Number + 1]), Get_Config.Get_Property_Value(BNK_Info.Read_All[Number + 2]), Property_Type.Real64);
                        continue;
                    }
                    else if (Read_Line.Contains("valuefmt=\"0x02 [Pitch]\""))
                    {
                        Add_Property_Range(Temp_Relation, "Pitch", Get_Config.Get_Property_Value(BNK_Info.Read_All[Number + 1]), Get_Config.Get_Property_Value(BNK_Info.Read_All[Number + 2]), Property_Type.Real64);
                        continue;
                    }
                    else if (Read_Line.Contains("valuefmt=\"0x03 [LPF]\""))
                    {
                        Add_Property_Range(Temp_Relation, "Lowpass", Get_Config.Get_Property_Value(BNK_Info.Read_All[Number + 1]), Get_Config.Get_Property_Value(BNK_Info.Read_All[Number + 2]), Property_Type.Real64);
                        continue;
                    }
                    else if (Read_Line.Contains("valuefmt=\"0x04 [HPF]\""))
                    {
                        Add_Property_Range(Temp_Relation, "Highpass", Get_Config.Get_Property_Value(BNK_Info.Read_All[Number + 1]), Get_Config.Get_Property_Value(BNK_Info.Read_All[Number + 2]), Property_Type.Real64);
                        continue;
                    }
                    else if (Read_Line.Contains("valuefmt=\"0x3B [InitialDelay]\"") && Include_Option)
                    {
                        Add_Property_Range(Temp_Relation, "InitialDelay", Get_Config.Get_Property_Value(BNK_Info.Read_All[Number + 1]), Get_Config.Get_Property_Value(BNK_Info.Read_All[Number + 2]), Property_Type.Real64);
                        continue;
                    }
                }
                else if (Add_Property_Mode == 3)
                {
                    if (Read_Line.Contains("name=\"bKillNewest\"") && Get_Config.Get_Property_Value(BNK_Info.Read_All[Number]) == "1")
                    {
                        Add_PropertyList(Temp_Relation, "MaxReachedBehavior", "1", Property_Type.int16, false, true);
                        IsOverWriteParentMaxSoundInstance = true;
                        continue;
                    }
                    else if (Read_Line.Contains("name=\"bUseVirtualBehavior\"") && Get_Config.Get_Property_Value(BNK_Info.Read_All[Number]) == "1")
                    {
                        Add_PropertyList(Temp_Relation, "OverLimitBehavior", "1", Property_Type.int16, false, true);
                        IsOverWriteParentMaxSoundInstance = true;
                        continue;
                    }
                    else if (Read_Line.Contains("name=\"eVirtualQueueBehavior\"") && Get_Config.Get_Property_Value(BNK_Info.Read_All[Number]) != "1")
                    {
                        Add_PropertyList(Temp_Relation, "VirtualVoiceQueueBehavior", Get_Config.Get_Property_Value(BNK_Info.Read_All[Number]), Property_Type.int16, false, true);
                        IsOverWriteVirtualVoice = true;
                        continue;
                    }
                    else if (Read_Line.Contains("name=\"u16MaxNumInstance\"") && Get_Config.Get_Property_Value(BNK_Info.Read_All[Number]) != "50")
                    {
                        Add_PropertyList(Temp_Relation, "UseMaxSoundPerInstance", "True", Property_Type.boolean, false);
                        Add_PropertyList(Temp_Relation, "MaxSoundPerInstance", Get_Config.Get_Property_Value(BNK_Info.Read_All[Number]), Property_Type.int16, true);
                        IsOverWriteParentMaxSoundInstance = true;
                        continue;
                    }
                    else if (Read_Line.Contains("name=\"eBelowThresholdBehavior\"") && Get_Config.Get_Property_Value(BNK_Info.Read_All[Number]) != "0")
                    {
                        Add_PropertyList(Temp_Relation, "BelowThresholdBehavior", Get_Config.Get_Property_Value(BNK_Info.Read_All[Number]), Property_Type.int16, false, true);
                        IsOverWriteVirtualVoice = true;
                        continue;
                    }
                }
                //RTPCを設定(プロパティの項目と一緒に記述される必要があるためここに置く)
                else if (Add_Property_Mode == 4)
                {
                    if (Read_Line.Contains("<object name=\"RTPC\" index=\""))
                        Add_Property_RTPC(Temp_Relation, Number);
                }
                if (BNK_Info.Read_All[Number].Contains("<object name=\"Children\">"))
                    break;
            }
            if (IsOverWriteVirtualVoice)
                Add_PropertyList(Temp_Relation, "OverrideVirtualVoice", "True", Property_Type.boolean, false, true);
            if (IsOverWritePriority)
                Add_PropertyList(Temp_Relation, "OverridePriority", "True", Property_Type.boolean, false, true);
            if (IsOverWriteParentMaxSoundInstance)
                Add_PropertyList(Temp_Relation, "IgnoreParentMaxSoundInstance", "True", Property_Type.boolean, false, true);
            //記入位置を再取得(範囲を記入する際に位置が変更されるため)
            More_Class.List_Init(Property_End_Line);
            int Line_Start = -1;
            for (int Number = 5; Number < BNK_Info.Actor_Mixer_Hierarchy_Project.Count; Number++)
            {
                if (BNK_Info.Actor_Mixer_Hierarchy_Project[Number].Contains("Name=\"") && BNK_Info.Actor_Mixer_Hierarchy_Project[Number].Contains("ShortID=\""))
                {
                    if (Get_Config.Get_ShortID_Project(BNK_Info.Actor_Mixer_Hierarchy_Project[Number]) == Temp_Relation.My.ToString())
                        Line_Start = Number + 1;
                }
            }
            if (Line_Start == -1)
                return;
            int Write_Line = 0;
            int Line_Start_Temp = Line_Start;
            bool IsThrowEnd = false;
            while (true)
            {
                string Read_Line = BNK_Info.Actor_Mixer_Hierarchy_Project[Line_Start];
                if (Read_Line.Contains("<ModifierList>") || Read_Line.Contains("<Curve Name=\"\""))
                    IsThrowEnd = true;
                else if (Read_Line.Contains("</ModifierList>") || Read_Line.Contains("<PointList>"))
                    IsThrowEnd = false;
                else if (Read_Line.Contains("</PropertyList>") && !IsThrowEnd)
                {
                    Write_Line = Line_Start;
                    break;
                }
                Line_Start++;
            }
            More_Class.List_Init(Write_Line);
            bool IsContinueMode = false;
            //1行で済ますことができるプロパティ設定を反映させる用
            for (int Number = From_Line; Number < To_Line; Number++)
            {
                string Read_Line = BNK_Info.Read_All[Number];
                if (Read_Line.Contains("type=\"tid\" name=\"OverrideBusId\""))
                {
                    uint Output_Bus_ShortID = uint.Parse(Get_Config.Get_Property_Value(Read_Line));
                    if (Output_Bus_ShortID != 0)
                    {
                        foreach (Master_Audio_Relation Temp in Master_Mixer.Master_Audio_Info)
                        {
                            if (Temp.ShortID == Output_Bus_ShortID)
                            {
                                Output_Bus = Temp;
                                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"OverrideOutput\" Type=\"bool\" Value=\"True\"/>");
                                break;
                            }
                        }
                    }
                }
                else if (Read_Line.Contains("type=\"u8\" name=\"bIsContinuousValidation\""))
                {
                    int Value = int.Parse(Get_Config.Get_Property_Value(Read_Line));
                    if (Value == 1)
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"SwitchBehavior\" Type=\"int16\" Value=\"1\"/>");
                }
                if (Read_Line.Contains("name=\"bPositioningInfoOverrideParent\""))
                    if (Get_Config.Get_Property_Value(Read_Line) == "1")
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"OverridePositioning\" Type=\"bool\" Value=\"True\"/>");
                if (Read_Line.Contains("name=\"ePannerType\""))
                {
                    int Value = int.Parse(Get_Config.Get_Property_Value(Read_Line));
                    if (Value > 0)
                    {
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"SpeakerPanning\" Type=\"int16\" Value=\"" + Value + "\"/>");
                        IsExistPanner = true;
                    }
                }
                if (Read_Line.Contains("name=\"e3DPositionType\""))
                {
                    int Value = int.Parse(Get_Config.Get_Property_Value(Read_Line));
                    if (Value > 0)
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"3DPosition\" Type=\"int16\" Value=\"" + Value + "\"/>");
                }
                if (Read_Line.Contains("name=\"eSpatializationMode\""))
                {
                    int Value = int.Parse(Get_Config.Get_Property_Value(Read_Line));
                    if (Value > 0)
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"3DSpatialization\" Type=\"int16\" Value=\"" + Value + "\"/>");
                }
                if (Read_Line.Contains("name=\"bHoldEmitterPosAndOrient\""))
                    if (Get_Config.Get_Property_Value(Read_Line) == "1")
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"HoldEmitterPositionOrientation\" Type=\"bool\" Value=\"True\"/>");
                if (Read_Line.Contains("name=\"bEnableDiffraction\""))
                    if (Get_Config.Get_Property_Value(Read_Line) == "0")
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"EnableDiffraction\" Type=\"bool\" Value=\"True\"/>");
                if (Read_Line.Contains("name=\"bHoldListenerOrient\""))
                    if (Get_Config.Get_Property_Value(Read_Line) == "1")
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"HoldListenerOrientation\" Type=\"bool\" Value=\"True\"/>");
                if (Read_Line.Contains("type=\"u16\" name=\"sLoopCount\""))
                {
                    int Value = int.Parse(Get_Config.Get_Property_Value(Read_Line));
                    if (Value == 0)
                    {
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"PlayMechanismLoop\" Type=\"bool\" Value=\"True\"/>");
                        if (!IsContinueMode)
                        {
                            IsContinueMode = true;
                            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"PlayMechanismStepOrContinuous\" Type=\"int16\" Value=\"0\"/>");
                        }
                    }
                    else if (Value >= 2)
                    {
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"PlayMechanismInfiniteOrNumberOfLoops\" Type=\"int16\" Value=\"0\"/>");
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"PlayMechanismLoop\" Type=\"bool\" Value=\"True\"/>");
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"PlayMechanismLoopCount\" Type=\"int32\" Value=\"" + Value + "\"/>");
                    }
                }
                else if (Read_Line.Contains("name=\"wAvoidRepeatCount\""))
                {
                    int Value = int.Parse(Get_Config.Get_Property_Value(Read_Line));
                    if (Value == 0)
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"RandomAvoidRepeating\" Type=\"bool\" Value=\"False\"/>");
                    else if (Value > 1)
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"RandomAvoidRepeatingCount\" Type=\"int32\" Value=\"" + Value + "\"/>");
                }
                else if (Read_Line.Contains("name=\"fTransitionTime\""))
                {
                    double Value = double.Parse(Get_Config.Get_Property_Value(Read_Line)) / 1000;
                    if (Value != 1)
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"PlayMechanismSpecialTransitionsValue\" Type=\"Real64\" Value=\"" + Value + "\"/>");
                }
                else if (Read_Line.Contains("name=\"eTransitionMode\""))
                {
                    int Value = int.Parse(Get_Config.Get_Property_Value(Read_Line));
                    if (Value > 0)
                    {
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"PlayMechanismSpecialTransitions\" Type=\"bool\" Value=\"True\"/>");
                        if (!IsContinueMode)
                        {
                            IsContinueMode = true;
                            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"PlayMechanismStepOrContinuous\" Type=\"int16\" Value=\"0\"/>");
                        }
                    }
                    if (Value == 2)
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"PlayMechanismSpecialTransitionsType\" Type=\"int16\" Value=\"4\"/>");
                    if (Value == 3)
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"PlayMechanismSpecialTransitionsType\" Type=\"int16\" Value=\"1\"/>");
                    if (Value == 4)
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"PlayMechanismSpecialTransitionsType\" Type=\"int16\" Value=\"2\"/>");
                    if (Value == 5)
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"PlayMechanismSpecialTransitionsType\" Type=\"int16\" Value=\"3\"/>");
                }
                else if (Read_Line.Contains("name=\"bIsContinuous\""))
                {
                    int Value = int.Parse(Get_Config.Get_Property_Value(Read_Line));
                    if (Value != 0 && !IsContinueMode)
                    {
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"PlayMechanismStepOrContinuous\" Type=\"int16\" Value=\"0\"/>");
                        IsContinueMode = true;
                    }
                }
                else if (Read_Line.Contains("name=\"bIsGlobal\""))
                {
                    int Value = int.Parse(Get_Config.Get_Property_Value(Read_Line));
                    if (Value == 0)
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"GlobalOrPerObject\" Type=\"int16\" Value=\"0\"/>");
                }
                else if (Read_Line.Contains("valuefmt=\"0x3A [Loop]\""))
                {
                    int Value = int.Parse(Get_Config.Get_Property_Value(Read_Line));
                    if (Value == 0)
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"IsLoopingEnabled\" Type=\"bool\" Value=\"True\"/>");
                }
            }
            if (SpeakerPanning_Volume != 100)
                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"SpeakerPanning3DSpatializationMix\" Type=\"int32\" Value=\"" + SpeakerPanning_Volume + "\"/>");
            //入力する場所を再取得
            Line_Start = Line_Start_Temp;
            while (true)
            {
                string Read_Line = BNK_Info.Actor_Mixer_Hierarchy_Project[Line_Start];
                if (Read_Line.Contains("<ModifierList>") || Read_Line.Contains("<Curve Name=\"\""))
                    IsThrowEnd = true;
                else if (Read_Line.Contains("</ModifierList>") || Read_Line.Contains("<PointList>"))
                    IsThrowEnd = false;
                else if (Read_Line.Contains("</PropertyList>") && !IsThrowEnd)
                {
                    Write_Line = Line_Start + 1;
                    break;
                }
                Line_Start++;
            }
            //プロパティを終了
            More_Class.List_Init(Write_Line);
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<ReferenceList>");
            //3D設定が存在する場合追加
            if (IsExistAttenuations && Attenuation_ShortID != 0)
            {
                foreach (Attenuation_Relation Temp in BNK_Info.Attenuation_Info)
                {
                    if (Temp.ShortID == Attenuation_ShortID)
                    {
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Reference Name=\"Attenuation\">");
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<ObjectRef Name=\"" + Temp.Name + "\" ID=\"{" + Temp.GUID + "}\" WorkUnitID=\"{2A4DC1AF-DA27-4BAB-BFB5-D16386FA615E}\"/>");
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</Reference>");
                        break;
                    }
                }
            }
            else if (IsExistAttenuations && BNK_Info.Attenuation_Info.Count > 0)
            {
                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Reference Name=\"Attenuation\">");
                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<ObjectRef Name=\"" + BNK_Info.Attenuation_Info[0].Name + "\" ID=\"{" + BNK_Info.Attenuation_Info[0].GUID + "}\" WorkUnitID=\"{2A4DC1AF-DA27-4BAB-BFB5-D16386FA615E}\"/>");
                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</Reference>");
            }
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Reference Name=\"Conversion\">");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<ObjectRef Name=\"Vorbis Quality High\" ID=\"{53A9DE0F-3F4F-4B59-8614-3F9E3C7358FC}\" WorkUnitID=\"{F6B2880C-85E5-47FA-A126-645B5DFD9ACC}\"/>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</Reference>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Reference Name=\"OutputBus\">");
            if (Output_Bus != null)
            {
                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<ObjectRef Name=\"" + Output_Bus.ShortID + "\" ID=\"{" + Output_Bus.GUID + "}\" WorkUnitID=\"{" + Master_Mixer.Master_Bus_Unit + "}\"/>");
                if (!Master_ShortID.Contains(Output_Bus.ShortID))
                    Master_ShortID.Add(Output_Bus.ShortID);
            }
            else
                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<ObjectRef Name=\"Master Audio Bus\" ID=\"{1514A4D8-1DA6-412A-A17E-75CA0C2149F3}\" WorkUnitID=\"{" + Master_Mixer.Master_Bus_Unit + "}\"/>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</Reference>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</ReferenceList>");
            BNK_Info.Actor_Mixer_Child_Line.Add(new Actor_Mixer_Info(Type_Name_Project, Temp_Relation.GUID,  Temp_Relation.My, More_Class.List_Get_Line()));
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<ChildrenList>");
            if (IsChildrenEndLine)
            {
                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</ChildrenList>");
                if (IsExistPanner)
                {
                    More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<PositioningInfo>");
                    More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Panner Name=\"\" ID=\"{" + Guid.NewGuid().ToString().ToUpper() + "}\">");
                    More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<PropertyList>");
                    More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"PanX\" Type=\"Real64\" Value=\"" + Pan_LR + "\"/>");
                    More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"PanY\" Type=\"Real64\" Value=\"" + Pan_FR + "\"/>");
                    More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</PropertyList>");
                    More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</Panner>");
                    More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Position Name=\"\" ID=\"{" + Guid.NewGuid().ToString().ToUpper() + "}\"/>");
                    More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</PositioningInfo>");
                }
                Add_Property_State(More_Class.List_Get_Line(), From_Line, To_Line);
            }
        }
        //初期値を記入
        void Add_PropertyList(BNK_Relation Temp_Relation, string Name, string Value, Property_Type Type, bool IsIntMode, bool IsOneLine = false)
        {
            int Line_Start = -1;
            for (int Number = 5; Number < BNK_Info.Actor_Mixer_Hierarchy_Project.Count; Number++)
            {
                if (BNK_Info.Actor_Mixer_Hierarchy_Project[Number].Contains("Name=\"") && BNK_Info.Actor_Mixer_Hierarchy_Project[Number].Contains("ShortID=\""))
                {
                    if (Get_Config.Get_ShortID_Project(BNK_Info.Actor_Mixer_Hierarchy_Project[Number]) == Temp_Relation.My.ToString())
                        Line_Start = Number;
                }
            }
            if (Line_Start == -1)
                return;
            string Type_Name = Type.ToString();
            if (Type == Property_Type.boolean)
                Type_Name = "bool";
            More_Class.List_Init(Line_Start + 2);
            if (Name == "PriorityDistanceOffset")
                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"PriorityDistanceFactor\" Type=\"bool\" Value=\"True\"/>");
            if (IsOneLine)
                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"" + Name + "\" Type=\"" + Type_Name + "\" Value=\"" + Value + "\"/>");
            else
            {
                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"" + Name + "\" Type=\"" + Type_Name + "\">");
                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<ValueList>");
                if (IsIntMode)
                    More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Value>" + (int)double.Parse(Value) + "</Value>");
                else
                    More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Value>" + Value + "</Value>");
                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</ValueList>");
                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</Property>");
            }
            Property_End_Line = More_Class.List_Get_Line();
        }
        //範囲を記入
        void Add_Property_Range(BNK_Relation Temp_Relation, string Name, string Min_Value, string Max_Value, Property_Type Type)
        {
            int Line_Start = -1;
            for (int Number = 5; Number < BNK_Info.Actor_Mixer_Hierarchy_Project.Count; Number++)
            {
                if (BNK_Info.Actor_Mixer_Hierarchy_Project[Number].Contains("Name=\"") && BNK_Info.Actor_Mixer_Hierarchy_Project[Number].Contains("ShortID=\""))
                {
                    if (Get_Config.Get_ShortID_Project(BNK_Info.Actor_Mixer_Hierarchy_Project[Number]) == Temp_Relation.My.ToString())
                        Line_Start = Number + 1;
                }
            }
            if (Line_Start == -1)
                return;
            bool IsThrowEnd = false;
            //どこに記述するかを取得
            while (true)
            {
                string Read_Line = BNK_Info.Actor_Mixer_Hierarchy_Project[Line_Start];
                if (Read_Line.Contains("<Property Name=\"" + Name + "\""))
                {
                    More_Class.List_Init(Line_Start + 4);
                    break;
                }
                if (Read_Line.Contains("<ModifierList>") || Read_Line.Contains("<Curve Name=\"\""))
                    IsThrowEnd = true;
                else if (Read_Line.Contains("</ModifierList>") || Read_Line.Contains("<PointList>"))
                    IsThrowEnd = false;
                else if (Read_Line.Contains("</PropertyList>") && !IsThrowEnd)
                {
                    More_Class.List_Init(Line_Start);
                    More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"" + Name + "\" Type=\"" + Type + "\">");
                    break;
                }
                Line_Start++;
            }
            //適当なGUIDを取得して必要な文"字を追加
            string Temp_GUID = Guid.NewGuid().ToString().ToUpper();
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<ModifierList>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<ModifierInfo>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Modifier Name=\"\" ID=\"{" + Temp_GUID + "}\">");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<PropertyList>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"Enabled\" Type=\"bool\" Value=\"True\"/>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"Max\" Type=\"" + Type + "\" Value=\"" + Max_Value + "\"/>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"Min\" Type=\"" + Type + "\" Value=\"" + Min_Value + "\"/>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</PropertyList>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</Modifier>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</ModifierInfo>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</ModifierList>");
            if (BNK_Info.Actor_Mixer_Hierarchy_Project[More_Class.List_Get_Line()] != "</Property>")
            {
                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</Property>");
                Property_End_Line = More_Class.List_Get_Line();
            }
            else
                Property_End_Line = More_Class.List_Get_Line() + 1;
        }
        void Add_Property_RTPC(BNK_Relation Temp_Relation, int RTPC_Start_Line, int LayerModeWriteLine = 0)
        {
            string GetRTPCParam = "";
            uint GetRTPCShortID = uint.Parse(Get_Config.Get_Property_Value(BNK_Info.Read_All[RTPC_Start_Line + 1]));
            uint GetRTPCCurveID = 0;
            int RTPCNumber = -1;
            int pRTPCMgr_Line = 0;
            int pRTPCMgr_Count = 0;
            for (int Number = 0; Number < BNK_Info.RTPC_Info.Count; Number++)
            {
                if (BNK_Info.RTPC_Info[Number].ShortID == GetRTPCShortID)
                {
                    RTPCNumber = Number;
                    break;
                }
            }
            for (int Number = RTPC_Start_Line; Number < BNK_Info.Read_All.Count; Number++)
            {
                if (BNK_Info.Read_All[Number].Contains("name=\"ParamID\""))
                    GetRTPCParam = Get_Config.Get_Value_FMT(BNK_Info.Read_All[Number]);
                if (BNK_Info.Read_All[Number].Contains("name=\"rtpcCurveID\""))
                    GetRTPCCurveID = uint.Parse(Get_Config.Get_Property_Value(BNK_Info.Read_All[Number]));
                if (BNK_Info.Read_All[Number].Contains("name=\"pRTPCMgr\""))
                {
                    pRTPCMgr_Count = int.Parse(Get_Config.Get_Count(BNK_Info.Read_All[Number]));
                    pRTPCMgr_Line = Number;
                    break;
                }
            }
            if (RTPCNumber == -1 || GetRTPCCurveID == 0)
                return;
            if (!RTPC_ShortID.Contains(BNK_Info.RTPC_Info[RTPCNumber].ShortID))
                RTPC_ShortID.Add(BNK_Info.RTPC_Info[RTPCNumber].ShortID);
            int Line_Start = -1;
            if (LayerModeWriteLine == 0)
            {
                for (int Number = 5; Number < BNK_Info.Actor_Mixer_Hierarchy_Project.Count; Number++)
                {
                    if (BNK_Info.Actor_Mixer_Hierarchy_Project[Number].Contains("Name=\"") && BNK_Info.Actor_Mixer_Hierarchy_Project[Number].Contains("ShortID=\""))
                    {
                        if (Get_Config.Get_ShortID_Project(BNK_Info.Actor_Mixer_Hierarchy_Project[Number]) == Temp_Relation.My.ToString())
                        {
                            Line_Start = Number + 1;
                            break;
                        }
                    }
                }
            }
            else
                Line_Start = LayerModeWriteLine;
            if (Line_Start == -1)
                return;
            if (GetRTPCParam == "HPF")
                GetRTPCParam = "Highpass";
            else if (GetRTPCParam == "LPF")
                GetRTPCParam = "Lowpass";
            bool IsThrowEnd = false;
            bool IsExistProperty = false;
            //どこに記述するかを取得
            while (true)
            {
                string Read_Line_01 = BNK_Info.Actor_Mixer_Hierarchy_Project[Line_Start];
                if (Read_Line_01.Contains("<Property Name=\"" + GetRTPCParam + "\""))
                {
                    More_Class.List_Init(Line_Start + 1);
                    IsExistProperty = true;
                    break;
                }
                if (Read_Line_01.Contains("<ModifierList>") || Read_Line_01.Contains("<Curve Name=\"\""))
                    IsThrowEnd = true;
                else if (Read_Line_01.Contains("</ModifierList>") || Read_Line_01.Contains("<PointList>"))
                    IsThrowEnd = false;
                else if (Read_Line_01.Contains("</PropertyList>") && !IsThrowEnd)
                {
                    More_Class.List_Init(Line_Start);
                    More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"" + GetRTPCParam + "\" Type=\"" + Get_Config.Get_Property_Type(GetRTPCParam) + "\">");
                    break;
                }
                Line_Start++;
            }
            if (IsExistProperty)
            {
                IsThrowEnd = false;
                Line_Start = More_Class.List_Get_Line();
                while (true)
                {
                    string Read_Line_01 = BNK_Info.Actor_Mixer_Hierarchy_Project[Line_Start];
                    if (Read_Line_01 == "<PropertyList>")
                        IsThrowEnd = true;
                    else if (Read_Line_01 == "</PropertyList>")
                        IsThrowEnd = false;
                    else if (Read_Line_01 == "</Property>" && !IsThrowEnd)
                    {
                        More_Class.List_Init(Line_Start);
                        break;
                    }
                    Line_Start++;
                }
            }
            bool IsExistRTPC = false;
            if (BNK_Info.Actor_Mixer_Hierarchy_Project[More_Class.List_Get_Line() - 1].Contains("</RTPCList>"))
            {
                More_Class.List_Init(More_Class.List_Get_Line() - 1);
                IsExistRTPC = true;
            }
            else
                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<RTPCList>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<RTPC Name=\"\" ID=\"{" + Guid.NewGuid().ToString().ToUpper() + "}\" ShortID=\"" + GetRTPCCurveID + "\">");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<ReferenceList>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Reference Name=\"ControlInput\">");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<ObjectRef Name=\"" + BNK_Info.RTPC_Info[RTPCNumber].Name + "\" ID=\"{" + BNK_Info.RTPC_Info[RTPCNumber].GUID + "}\" WorkUnitID=\"{" + BNK_Info.Parent_RTPC_WorkUnit + "}\"/>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</Reference>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</ReferenceList>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Curve Name=\"\" ID=\"{" + Guid.NewGuid().ToString().ToUpper() + "}\">");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<PropertyList>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"Flags\" Type=\"int32\" Value=\"65537\"/>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</PropertyList>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<PointList>");
            double RTPC_Min_Value = 0;
            double RTPC_Max_Value = 0;
            for (int Number = pRTPCMgr_Line; Number < BNK_Info.Read_All.Count; Number++)
            {
                if (BNK_Info.Read_All[Number].Contains("<object name=\"AkRTPCGraphPoint\""))
                {
                    int RTPC_Number = int.Parse(Get_Config.Get_Index(BNK_Info.Read_All[Number]));
                    More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Point>");
                    double X_Pos = double.Parse(Get_Config.Get_Property_Value(BNK_Info.Read_All[Number + 1]));
                    double Y_Pos = double.Parse(Get_Config.Get_Property_Value(BNK_Info.Read_All[Number + 2]));
                    string Interp = Get_Config.Get_SegmentShape(Get_Config.Get_Value_FMT(BNK_Info.Read_All[Number + 3]));
                    More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<XPos>" + X_Pos + "</XPos>");
                    More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<YPos>" + Y_Pos + "</YPos>");
                    if (RTPC_Number == 0)
                    {
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Flags>5</Flags>");
                        RTPC_Min_Value = X_Pos;
                    }
                    else if (RTPC_Number + 1 == pRTPCMgr_Count)
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Flags>37</Flags>");
                    else
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Flags>0</Flags>");
                    if (RTPC_Max_Value < X_Pos)
                        RTPC_Max_Value = X_Pos;
                    if (Interp != "")
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<SegmentShape>" + Interp + "</SegmentShape>");
                    More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</Point>");
                }
                if (BNK_Info.Read_All[Number].Contains("</list>"))
                    break;
            }
            BNK_Info.RTPC_Info[RTPCNumber].Min_Value = RTPC_Min_Value;
            BNK_Info.RTPC_Info[RTPCNumber].Max_Value = RTPC_Max_Value;
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</PointList>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</Curve>");
            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</RTPC>");
            if (!IsExistRTPC)
                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</RTPCList>");
            string Read_Line = BNK_Info.Actor_Mixer_Hierarchy_Project[More_Class.List_Get_Line()];
            if (Read_Line != "</Property>" && Read_Line != "<PropertyList>" && Read_Line != "<Property Name=\"" && Read_Line != "<ModifierList>" && !IsExistRTPC)
                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</Property>");
            if (LayerModeWriteLine == 0)
                Property_End_Line = More_Class.List_Get_Line();
        }
        void Add_Property_State(int Start_Line, int From_Line, int To_Line)
        {
            for (int Number = From_Line; Number <= To_Line; Number++)
            {
                string Read_Line = BNK_Info.Read_All[Number];
                bool IsOK = false;
                if (Read_Line.Contains("<list name=\"pStateChunks\""))
                {
                    int Count = int.Parse(Get_Config.Get_Count(Read_Line));
                    if (Count > 0)
                    {
                        More_Class.List_Init(Start_Line);
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<StateInfo>");
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<StateGroupList>");
                        List<uint> State_IDs = new List<uint>();
                        List<uint> State_Instance_IDs = new List<uint>();
                        for (int Number_01 = Number; Number_01 <= To_Line; Number_01++)
                        {
                            if (BNK_Info.Read_All[Number_01].Contains("<object name=\"AkStateGroupChunk\""))
                            {
                                uint Parent_ShortID = uint.Parse(Get_Config.Get_Property_Value(BNK_Info.Read_All[Number_01 + 1]));
                                for (int Number_02 = 0; Number_02 < State.State_All_Info.Count; Number_02++)
                                {
                                    if (State.State_All_Info[Number_02].Short_ID == Parent_ShortID)
                                    {
                                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<StateGroupInfo>");
                                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<StateGroupRef Name=\"" + State.State_All_Info[Number_02].Name + "\" ID=\"{" + State.State_All_Info[Number_02].GUID + "}\"/>");
                                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</StateGroupInfo>");
                                        break;
                                    }
                                }
                            }
                            else if (BNK_Info.Read_All[Number_01].Contains("type=\"tid\" name=\"ulStateID\""))
                            {
                                State_IDs.Add(uint.Parse(Get_Config.Get_Property_Value(BNK_Info.Read_All[Number_01])));
                                State_Instance_IDs.Add(uint.Parse(Get_Config.Get_Property_Value(BNK_Info.Read_All[Number_01 + 1])));
                            }
                            else if (BNK_Info.Read_All[Number_01].Contains("<object name=\"InitialRTPC\">"))
                                break;
                        }
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</StateGroupList>");
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<CustomStateList>");
                        for (int Number_01 = 0; Number_01 < State_IDs.Count; Number_01++)
                        {
                            State_Child_Relation Child_State = null;
                            State_Relation Relation = null;
                            for (int Number_02 = 0; Number_02 < State.State_Child_Info.Count; Number_02++)
                            {
                                if (State.State_Child_Info[Number_02].Short_ID == State_IDs[Number_01])
                                {
                                    Child_State = State.State_Child_Info[Number_02];
                                    break;
                                }
                            }
                            for (int Number_02 = 0; Number_02 < State.State_Value_Info.Count; Number_02++)
                            {
                                if (State.State_Value_Info[Number_02].Short_ID == State_Instance_IDs[Number_01])
                                {
                                    Relation = State.State_Value_Info[Number_02];
                                    break;
                                }
                            }
                            if (Child_State == null || Relation == null)
                                continue;
                            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<CustomState>");
                            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<StateRef Name=\"" + Child_State.Name + "\" ID=\"{" + Child_State.GUID + "}\"/>");
                            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<CustomState Name=\"\" ID=\"{" + Relation.GUID + "}\" ShortID=\"" + Relation.Short_ID + "\">");
                            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<PropertyList>");
                            if (Relation.Volume != 0)
                                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"Volume\" Type=\"Real64\" Value=\"" + Relation.Volume + "\"/>");
                            if (Relation.Pitch != 0)
                                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"Pitch\" Type=\"int16\" Value=\"" + Relation.Pitch + "\"/>");
                            if (Relation.LPF != 0)
                                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"Lowpass\" Type=\"int16\" Value=\"" + Relation.LPF + "\"/>");
                            if (Relation.HPF != 0)
                                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"Highpass\" Type=\"int16\" Value=\"" + Relation.HPF + "\"/>");
                            if (Relation.MakeUpGain != 0)
                                More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "<Property Name=\"Real64\" Type=\"Real64\" Value=\"" + Relation.MakeUpGain + "\"/>");
                            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</PropertyList>");
                            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</CustomState>");
                            More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</CustomState>");
                        }
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</CustomStateList>");
                        More_Class.List_Add(BNK_Info.Actor_Mixer_Hierarchy_Project, "</StateInfo>");
                        State_IDs.Clear();
                        State_Instance_IDs.Clear();
                        IsOK = true;
                    }
                }
                if (IsOK)
                    break;
            }
        }
    }
}