using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WoTB_Voice_Mod_Creater.Wwise_Class
{
    public class BNK_Parse
    {
        //解析した内容を行に分けてすべて記録
        List<string> Read_All = new List<string>();
        //↑の内容から、イベントID、そのIDの行、イベント形式(RandomコンテナやSwitchコンテナなど)のみを抽出
        List<List<string>> ID_Line = new List<List<string>>();
        //ファイルが正しくない場合falseにする
        bool IsSelected = false;
        public BNK_Parse(string BNK_File)
        {
            if (!File.Exists(BNK_File))
            {
                IsSelected = false;
                return;
            }
            //bnkファイルを解析(wwiserを使用)
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
                }
            }
            file.Close();
            //生成されたxmlファイルは使用しないので消しておく
            File.Delete(Voice_Set.Special_Path + "/Wwise_Parse/" + Temp_Name + ".xml");
            IsSelected = true;
        }
        //選択されているbnkファイルが音声データかを調べる
        public bool IsVoiceFile()
        {
            bool IsExist = false;
            if (!IsSelected)
            {
                return IsExist;
            }
            foreach (List<string> ID_Now in ID_Line)
            {
                //戦闘開始のイベントがあるかないかで判定("590598450"が戦闘開始のイベントID)
                if (ID_Now[0] == "590598450")
                {
                    IsExist = true;
                }
            }
            return IsExist;
        }
        //データからどの音声ファイルが戦闘中のどこで再生されるかを取得(貫通時や火災時など)
        public List<List<string>> Get_Voices()
        {
            if (!IsSelected)
            {
                return new List<List<string>>();
            }
            //空のリストを作成
            List<List<string>> Voices_Temp = new List<List<string>>();
            for (int Number = 0; Number <= 49; Number++)
            {
                List<string> Temp = new List<string>();
                Voices_Temp.Add(Temp);
            }
            //イベントIDのみを抽出
            List<uint> GetEventsID = new List<uint>();
            foreach (List<string> List_Now in ID_Line)
            {
                if (List_Now[2] == "CAkEvent")
                {
                    GetEventsID.Add(uint.Parse(List_Now[0]));
                }
            }
            //イベントに入っている音声をすべて取得(Switchがある場合どちらも取得してしまうため注意)
            for (int Number_01 = 0; Number_01 < GetEventsID.Count; Number_01++)
            {
                int Event_Number = Get_Voice_Type_Number(GetEventsID[Number_01]);
                if (Event_Number == -1)
                {
                    continue;
                }
                Voices_Temp[Event_Number] = Get_Event_Voices(GetEventsID[Number_01]);
            }
            return Voices_Temp;
        }
        //イベントからアクションIDを取得して音声ファイルを取得
        List<string> Get_Event_Voices(uint Event_ID)
        {
            int Number_01 = -1;
            //指定されたイベントIDがID_Lineのどこに入っているか調べる
            for (int Number = 0; Number < ID_Line.Count; Number++)
            {
                if (Event_ID == uint.Parse(ID_Line[Number][0]))
                {
                    Number_01 = Number;
                }
            }
            if (Number_01 == -1)
            {
                return new List<string>();
            }
            //行を取得
            int Number_02 = int.Parse(ID_Line[Number_01][1]);
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
                foreach (string ID_Now in Action_Children(Child_ID))
                {
                    Child_SourceIDs.Add(ID_Now);
                }
            }
            return Child_SourceIDs;
        }
        //アクションに入っているイベントをすべて取得し、そのIDから音声を取得
        List<string> Action_Children(uint Action_ID)
        {
            int Number_01 = -1;
            for (int Number = 0; Number < ID_Line.Count; Number++)
            {
                if (Action_ID == uint.Parse(ID_Line[Number][0]))
                {
                    Number_01 = Number;
                }
            }
            int Number_02 = int.Parse(ID_Line[Number_01][1]);
            //Playイベントではない場合飛ばす
            if (!Read_All[Number_02 + 1].Contains("0x0403"))
            {
                return new List<string>();
            }
            //子コンテナの数を取得
            string Index = Read_All[Number_02 + 3].Remove(0, Read_All[Number_02 + 3].IndexOf("value=\"") + 7);
            Index = Index.Remove(Index.IndexOf("\""));
            uint Child_ID = uint.Parse(Index);
            return Children_Sort(Child_ID);
        }
        //CAkSoundの階層に到達するまで繰り返す
        //CAkSoundに行ったらSourceIDを取得して戻り値にリストとして返す
        List<string> Children_Sort(uint Child_ID)
        {
            int Number_01 = -1;
            for (int Number = 0; Number < ID_Line.Count; Number++)
            {
                if (Child_ID == uint.Parse(ID_Line[Number][0]))
                {
                    Number_01 = Number;
                }
            }
            int Start_Line = int.Parse(ID_Line[Number_01][1]);
            //CAkSoundの場合それ以上階層がないためSourceIDを取得して終わる
            if (ID_Line[Number_01][2] == "CAkSound")
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
                {
                    return new List<string>();
                }
                //音声ファイルのIDを取得
                string Index = Read_All[End_Line].Remove(0, Read_All[End_Line].IndexOf("value=\"") + 7);
                Index = Index.Remove(Index.IndexOf("\""));
                int SourceID = int.Parse(Index);
                List<string> Temp = new List<string>();
                Temp.Add(SourceID.ToString());
                return Temp;
            }
            //CAkSound以外の場合はまだ下に階層があるためなくなるまで続ける
            else
            {
                int End_Line = -1;
                //子コンテナがある行を取得
                while (Start_Line < Read_All.Count)
                {
                    Start_Line++;
                    if (Read_All[Start_Line].Contains("Children"))
                    {
                        End_Line = Start_Line;
                        break;
                    }
                }
                if (End_Line == -1)
                {
                    return new List<string>();
                }
                //下の階層がいくつあるか取得
                string Index = Read_All[End_Line + 1].Remove(0, Read_All[End_Line + 1].IndexOf("value=\"") + 7);
                Index = Index.Remove(Index.IndexOf("\""));
                int Child_Count = int.Parse(Index);
                List<string> Child_Source_IDs = new List<string>();
                //階層の数だけこの関数を実行
                for (int Number = 1; Number <= Child_Count; Number++)
                {
                    //子コンテナのIDを取得してChildren_Sortを実行
                    string Index2 = Read_All[End_Line + 1 + Number].Remove(0, Read_All[End_Line + 1 + Number].IndexOf("value=\"") + 7);
                    Index2 = Index2.Remove(Index2.IndexOf("\""));
                    uint Child_ID_2 = uint.Parse(Index2);
                    foreach (string IDs in Children_Sort(Child_ID_2))
                    {
                        Child_Source_IDs.Add(IDs);
                    }
                }
                return Child_Source_IDs;
            }
        }
        //イベントIDからインデックスを取得("戻り値が0のときはフレインドリーファイヤ－"などはVoice_Set.csに定義しています)
        int Get_Voice_Type_Number(uint ID)
        {
            if (ID == 2301863335)
            {
                return 0;
            }
            else if (ID == 3166122996)
            {
                return 1;
            }
            else if (ID == 1332081395)
            {
                return 2;
            }
            else if (ID == 1057290642 || ID == 4209631318)
            {
                return 3;
            }
            else if (ID == 102434383 || ID == 136522141 || ID == 3406769 || ID == 3175172091 || ID == 730352367 || ID == 1713814001)
            {
                return 4;
            }
            else if (ID == 2655350512)
            {
                return 5;
            }
            else if (ID == 1946435397)
            {
                return 6;
            }
            else if (ID == 664892591)
            {
                return 7;
            }
            else if (ID == 3563950026)
            {
                return 8;
            }
            else if (ID == 1122640859)
            {
                return 9;
            }
            else if (ID == 1702267773)
            {
                return 10;
            }
            else if (ID == 1302422561)
            {
                return 11;
            }
            else if (ID == 4148607935)
            {
                return 12;
            }
            else if (ID == 1396083645)
            {
                return 13;
            }
            else if (ID == 3616464105)
            {
                return 14;
            }
            else if (ID == 1641781602)
            {
                return 15;
            }
            else if (ID == 693799259)
            {
                return 16;
            }
            else if (ID == 825236543)
            {
                return 17;
            }
            else if (ID == 4011208105)
            {
                return 18;
            }
            else if (ID == 1107412350)
            {
                return 19;
            }
            else if (ID == 1781671118)
            {
                return 20;
            }
            else if (ID == 575170028)
            {
                return 21;
            }
            else if (ID == 3928775652)
            {
                return 22;
            }
            else if (ID == 590598450)
            {
                return 23;
            }
            else if (ID == 2775977275 || ID == 1077949222)
            {
                return 24;
            }
            else if (ID == 1112395295)
            {
                return 25;
            }
            else if (ID == 998311689)
            {
                return 26;
            }
            else if (ID == 766987452)
            {
                return 27;
            }
            else if (ID == 1244332268)
            {
                return 28;
            }
            else if (ID == 3498324732 || ID == 311966861)
            {
                return 29;
            }
            else if (ID == 47755571)
            {
                return 30;
            }
            else if (ID == 1630212423)
            {
                return 31;
            }
            else if (ID == 257621937)
            {
                return 32;
            }
            else if (ID == 3917054405)
            {
                return 33;
            }
            else if (ID == 4072375507)
            {
                return 34;
            }
            else if (ID == 3034227203 || ID == 79416786)
            {
                return 35;
            }
            else if (ID == 1491984609)
            {
                return 44;
            }
            else if (ID == 85617264)
            {
                return 45;
            }
            else if (ID == 2594173436)
            {
                return 47;
            }
            else
            {
                return -1;
            }
        }
    }
}