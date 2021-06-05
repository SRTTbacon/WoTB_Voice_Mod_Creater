using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoTB_Voice_Mod_Creater.Wwise_Class
{
    class BNK_Relation
    {
        public string Type { get; private set; }
        public uint My { get; private set; }
        public uint Parent { get; private set; }
        public int Line { get; private set; }
        public BNK_Relation(string My_Type, uint My_Short_ID, uint Parent_Short_ID, int Type_Name_Line)
        {
            Type = My_Type;
            My = My_Short_ID;
            Parent = Parent_Short_ID;
            Line = Type_Name_Line;
        }
    }
    public class BNK_To_Wwise_Projects
    {
        //Init.bnkの情報を保存
        List<List<string>> Master_Audio_Bus_List = new List<List<string>>();
        //解析した内容を行に分けてすべて記録
        List<string> Read_All = new List<string>();
        //↑の内容から、イベントID、そのIDの行、イベント形式(RandomコンテナやSwitchコンテナなど)のみを抽出
        List<List<string>> ID_Line = new List<List<string>>();
        //IsSpecialBNKFileModeがtrueの場合に使用する
        List<List<string>> ID_Line_Special = new List<List<string>>();
        //プロジェクトファイルに追加されたらその項目はtrueにする
        List<bool> ID_Line_IsEnable = new List<bool>();
        //Actor_Audioのプロジェクトファイル
        List<string> Actor_Mixer_Hierarchy_Project = new List<string>();
        //BNK内の親子関係を保存
        List<BNK_Relation> Relation = new List<BNK_Relation>();
        //再現するActor_MixerのShortIDがある行を保存
        List<List<uint>> Actor_Mixer_Child_Line = new List<List<uint>>();
        bool IsSelected = false;
        public BNK_To_Wwise_Projects(string Init_File, string BNK_File, bool IsInitParsed)
        {
            if (!File.Exists(Init_File) || !File.Exists(BNK_File))
                return;
            if (IsInitParsed)
            {
                Sub_Code.File_Decrypt(Init_File, Voice_Set.Special_Path + "/Wwise_Parse/Init.dat", "SRTTbacon_Init_BNK_File_Parse", false);
                StreamReader str = new StreamReader(Voice_Set.Special_Path + "/Wwise_Parse/Init.dat");
                string line2;
                while ((line2 = str.ReadLine()) != null)
                {
                    List<string> Line_Info = new List<string>();
                    Line_Info.Add(line2.Substring(0, line2.IndexOf('=') - 1));
                    string Replace_ShortID = line2.Substring(line2.IndexOf('=') + 2);
                    Line_Info.Add(Replace_ShortID.Substring(0, Replace_ShortID.IndexOf(':') - 1));
                    Line_Info.Add(Replace_ShortID.Substring(Replace_ShortID.LastIndexOf('=') + 2));
                    Master_Audio_Bus_List.Add(Line_Info);
                }
                str.Close();
                str.Dispose();
                File.Delete(Voice_Set.Special_Path + "/Wwise_Parse/Init.dat");
            }
            else
            {
                
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
                    List<string> ID_Line_Tmp = new List<string>();
                    //イベントIDを取得
                    string strValue = line.Remove(0, line.IndexOf("value=\"") + 7);
                    strValue = strValue.Remove(strValue.LastIndexOf("\""));
                    //イベントの内容を取得(CASoundやCAkRanSeqCntrなど)
                    string strValue2 = Read_All[Read_All.Count - 4].Remove(0, Read_All[Read_All.Count - 4].IndexOf("name=\"") + 6);
                    strValue2 = strValue2.Remove(strValue2.IndexOf("index") - 2);
                    ID_Line_Tmp.Add(strValue);
                    //イベントの行
                    ID_Line_Tmp.Add((Read_All.Count - 1).ToString());
                    ID_Line_Tmp.Add(strValue2);
                    ID_Line.Add(ID_Line_Tmp);
                    if (strValue2 == "CAkRanSeqCntr" || strValue2 == "CAkSwitchCntr" || strValue2 == "CAkSound" || strValue2 == "CAkLayerCntr" || strValue2 == "CAkActorMixer")
                    {
                        while ((line = file.ReadLine()) != null)
                        {
                            Read_All.Add(line);
                            if (line.Contains("name=\"DirectParentID\""))
                            {
                                string strValue3 = line.Remove(0, line.IndexOf("value=\"") + 7);
                                strValue3 = strValue3.Remove(strValue3.LastIndexOf("\""));
                                Relation.Add(new BNK_Relation(strValue2, uint.Parse(strValue), uint.Parse(strValue3), (Read_All.Count - 1)));
                                break;
                            }
                        }
                    }
                    else
                    {
                        List<string> ID_Line_Tmp_Special = new List<string>();
                        ID_Line_Tmp_Special.Add("1");
                        ID_Line_Tmp_Special.Add("1");
                        ID_Line_Tmp_Special.Add("1");
                        ID_Line_Special.Add(ID_Line_Tmp_Special);
                    }
                    ID_Line_IsEnable.Add(false);
                }
            }
            file.Close();
            file.Dispose();
            //生成されたxmlファイルは使用しないので消しておく
            File.Delete(Voice_Set.Special_Path + "/Wwise_Parse/" + Temp_Name + ".xml");
            Actor_Mixer_Hierarchy_Project.Add("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            Actor_Mixer_Hierarchy_Project.Add("<WwiseDocument Type=\"WorkUnit\" ID=\"{DF3DA361-09F8-430D-814F-4080F28088DD}\" SchemaVersion=\"97\">");
            Actor_Mixer_Hierarchy_Project.Add("<AudioObjects>");
            Actor_Mixer_Hierarchy_Project.Add("<WorkUnit Name=\"Default Work Unit\" ID=\"{DF3DA361-09F8-430D-814F-4080F28088DD}\" PersistMode=\"Standalone\">");
            Actor_Mixer_Hierarchy_Project.Add("<ChildrenList>");
        }
        public void Get_Actor_Mixer_Hierarchy(string To)
        {
            List<int> Not_Done_Number = new List<int>();
            for (int Number = 0; Number < Relation.Count; Number++)
                Not_Done_Number.Add(Number);
            while (true)
            {
                if (Not_Done_Number.Count == 0)
                    break;
                foreach (int Number in Not_Done_Number)
                {
                    if (Relation[Number].Parent == 0)
                    {
                        Not_Done_Number.RemoveAt(Number);
                        break;
                    }
                }
            }
        }
        void Set_Actor_Mixer_Container(BNK_Relation Temp_Relation)
        {
            int From_Line = Temp_Relation.Line;
            int To_Line;
            int Temp_Line = Temp_Relation.Line;
            uint OverWriteParent = Temp_Relation.Parent;
            bool IsNoParent = Temp_Relation.Parent == 0 ? true : false;
            while (true)
            {
                Temp_Line++;
                if (Read_All.Count - 1 == Temp_Line)
                {
                    To_Line = Temp_Line - 1;
                    break;
                }
                if (Read_All[Temp_Line].Contains("type=\"u8\" name=\"eHircType\""))
                {
                    To_Line = Temp_Line - 4;
                    break;
                }
            }
            int Write_Start_Line = -1;
            if (IsNoParent)
                Write_Start_Line = 4;
            else
            {
                for (int Number_01 = 0; Number_01 < Actor_Mixer_Child_Line.Count; Number_01++)
                {
                    if (Actor_Mixer_Child_Line[Number_01][0] == OverWriteParent)
                    {
                        Write_Start_Line = (int)Actor_Mixer_Child_Line[Number_01][1];
                        break;
                    }

                }
            }
            if (Write_Start_Line == -1)
                return;
            string Type = Temp_Relation.Type;
            if (Type == "CAkLayerCntr")
            {
                More_Class.List_Init(Write_Start_Line + 1);
            }
        }
        void Add_CAkLayerCntr(BNK_Relation Temp_Relation, int From_Line, int To_Line)
        {
            string Temp_GUID = Guid.NewGuid().ToString().ToUpper();
            More_Class.List_Add(Actor_Mixer_Hierarchy_Project, "<ActorMixer Name=\"" + Temp_Relation.My + "\" ID=\"{" + Temp_GUID + "}\" ShortID=\"" + Temp_Relation.My + "\">");
            //
            Add_CAkCommon(Temp_Relation, From_Line, To_Line);
        }
        void Add_CAkCommon(BNK_Relation Temp_Relation, int From_Line, int To_Line)
        {
            More_Class.List_Add(Actor_Mixer_Hierarchy_Project, "<PropertyList>");
            int Add_Property_Mode = 0;
            for (int Number = From_Line; Number <= To_Line; Number++)
            {
                if (Read_All[Number].Contains("<object name=\"AkPropBundle&lt;AkPropValue,unsigned char&gt;\">"))
                    Add_Property_Mode = 1;
                else if (Read_All[Number].Contains("<object name=\"AkPropBundle&lt;RANGED_MODIFIERS&lt;AkPropValue&gt;&gt;\">"))
                    Add_Property_Mode = 2;
                if (Read_All[Number].Contains("valuefmt=\"0x06[MakeUpGain]\""))
                {
                    string strValue = Read_All[Number].Remove(0, Read_All[Number].IndexOf("value=\"") + 7);
                    strValue = strValue.Remove(strValue.LastIndexOf("\""));
                    More_Class.List_Add(Actor_Mixer_Hierarchy_Project, "<Property Name=\"MakeUpGain\" Type=\"Real64\">");
                    More_Class.List_Add(Actor_Mixer_Hierarchy_Project, "<ValueList>");
                    More_Class.List_Add(Actor_Mixer_Hierarchy_Project, "<Value>" + strValue + "</Value>");
                    More_Class.List_Add(Actor_Mixer_Hierarchy_Project, "</ValueList>");
                    More_Class.List_Add(Actor_Mixer_Hierarchy_Project, "</Property>");
                    continue;
                }
                if (Read_All[Number].Contains("<object name=\"PositioningParams\">"))
                    break;
            }
        }
    }
}