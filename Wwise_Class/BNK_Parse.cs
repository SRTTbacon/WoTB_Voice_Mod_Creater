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
        //IsSpecialBNKFileModeがtrueの場合に使用する
        List<List<string>> ID_Line_Special = new List<List<string>>();
        //ファイルが正しくない場合falseにする
        bool IsSelected = false;
        //特殊な.bnkファイルの場合trueにする(別の方法で.bnkファイルを解析します。)
        public bool IsSpecialBNKFileMode = false;
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
                    if (strValue2 == "CAkRanSeqCntr" || strValue2 == "CAkSwitchCntr" || strValue2 == "CAkSound")
                    {
                        while ((line = file.ReadLine()) != null)
                        {
                            Read_All.Add(line);
                            if (line.Contains("name=\"DirectParentID\""))
                            {
                                List<string> ID_Line_Tmp_Special = new List<string>();
                                string strValue3 = line.Remove(0, line.IndexOf("value=\"") + 7);
                                strValue3 = strValue3.Remove(strValue3.LastIndexOf("\""));
                                ID_Line_Tmp_Special.Add(strValue3);
                                ID_Line_Tmp_Special.Add(ID_Line_Tmp[1]);
                                ID_Line_Tmp_Special.Add(strValue2);
                                ID_Line_Special.Add(ID_Line_Tmp_Special);
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
                }
            }
            file.Close();
            file.Dispose();
            //生成されたxmlファイルは使用しないので消しておく
            File.Delete(Voice_Set.Special_Path + "/Wwise_Parse/" + Temp_Name + ".xml");
            IsSelected = true;
        }
        //選択されているbnkファイルが音声データかを調べる
        public bool IsVoiceFile(bool IsBlitzMode = false)
        {
            uint Battle_Short_ID;
            bool IsExist = false;
            if (!IsSelected)
            {
                return IsExist;
            }
            if (IsBlitzMode)
            {
                Battle_Short_ID = 1419869192;
            }
            else
            {
                Battle_Short_ID = 590598450;
            }
            foreach (List<string> ID_Now in ID_Line)
            {
                //戦闘開始のイベントがあるかないかで判定("590598450"が戦闘開始のイベントID)
                if (ID_Now[0] == Battle_Short_ID.ToString())
                {
                    IsExist = true;
                    break;
                }
            }
            return IsExist;
        }
        //データからどの音声ファイルが戦闘中のどこで再生されるかを取得(貫通時や火災時など)
        //引数:BlitzからPC版WoTに移植する際はtrue
        public List<List<string>> Get_Voices(bool IsBlitzToWoT)
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
                int Event_Number = Get_Voice_Type_Number(GetEventsID[Number_01], IsBlitzToWoT);
                if (Event_Number == -1)
                {
                    continue;
                }
                Voices_Temp[Event_Number] = Get_Event_Voices(GetEventsID[Number_01]);
            }
            return Voices_Temp;
        }
        public void Clear()
        {
            Read_All.Clear();
            ID_Line.Clear();
            ID_Line_Special.Clear();
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
            if (IsSpecialBNKFileMode)
                return Children_Sort_Special(Child_ID);
            else
                return Children_Sort(Child_ID);
        }
        //CAkSoundの階層に到達するまで繰り返す
        //CAkSoundに到達したらSourceIDを取得して戻り値にリストとして返す
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
            if (Number_01 == -1)
            {
                return new List<string>();
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
                List<string> Temp = new List<string>();
                Temp.Add(Index);
                return Temp;
            }
            //CAkSound以外の場合はまだ下に階層があるためなくなるまで続ける
            else
            {
                int End_Line = -1;
                int Object_Count = 0;
                //子コンテナがある行を取得
                while (Start_Line < Read_All.Count)
                {
                    Start_Line++;
                    //Childrenの項目がない場合最後の行まで検索してしまうため</object>が3つ続いたらループを終了
                    if (Read_All[Start_Line].Contains("</object>"))
                    {
                        Object_Count++;
                    }
                    else
                    {
                        Object_Count = 0;
                    }
                    if (Object_Count >= 3)
                    {
                        break;
                    }
                    if (Read_All[Start_Line].Contains("Children"))
                    {
                        End_Line = Start_Line + 1;
                        break;
                    }
                }
                if (End_Line == -1)
                {
                    return new List<string>();
                }
                List<string> Child_Source_IDs = new List<string>();
                //階層の数だけこの関数を実行
                int Line_Count = 0;
                while (true)
                {
                    Line_Count++;
                    if (Read_All[End_Line + Line_Count].Contains("</object>"))
                    {
                        break;
                    }
                    //子コンテナのIDを取得してChildren_Sortを実行
                    string Index2 = Read_All[End_Line + Line_Count].Remove(0, Read_All[End_Line + Line_Count].IndexOf("value=\"") + 7);
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
        //CAkSoundの階層に到達するまで繰り返す
        //CAkSoundに行ったらSourceIDを取得して戻り値にリストとして返す
        //Childrenから取得するのではなく、DirectParentIDが一致するファイルから取得
        //一部のbnkファイルは上の形式が使用できないためこちらを使用
        List<string> Children_Sort_Special(uint Child_ID)
        {
            int Number_01 = -1;
            for (int Number = 0; Number < ID_Line.Count; Number++)
            {
                if (Child_ID == uint.Parse(ID_Line[Number][0]))
                {
                    Number_01 = Number;
                }
            }
            if (Number_01 == -1)
            {
                return new List<string>();
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
                List<string> Temp = new List<string>();
                Temp.Add(Index);
                return Temp;
            }
            else
            {
                List<string> Temp = new List<string>();
                for (int Index = 0; Index < ID_Line_Special.Count; Index++)
                {
                    if (uint.Parse(ID_Line_Special[Index][0]) == Child_ID)
                    {
                        foreach (string IDs in Children_Sort_Special(uint.Parse(ID_Line[Index][0])))
                        {
                            Temp.Add(IDs);
                        }
                    }
                }
                return Temp;
            }
        }
        //イベントIDからインデックスを取得("戻り値が0のときはフレインドリーファイヤ－"などはVoice_Set.csに定義しています)
        int Get_Voice_Type_Number(uint ID, bool IsBlitzToWoT)
        {
            if (!IsBlitzToWoT)
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
                else if (ID == 790147034)
                {
                    return 46;
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
            else
            {
                if (ID == 247635361)
                {
                    return 0;
                }
                else if (ID == 2228523046)
                {
                    return 1;
                }
                else if (ID == 1007238985)
                {
                    return 2;
                }
                else if (ID == 1954895547)
                {
                    return 3;
                }
                else if (ID == 2051125750)
                {
                    return 4;
                }
                else if (ID == 736917766)
                {
                    return 5;
                }
                else if (ID == 3774135051)
                {
                    return 6;
                }
                else if (ID == 1562476861)
                {
                    return 7;
                }
                else if (ID == 2892979156)
                {
                    return 8;
                }
                else if (ID == 2744672597)
                {
                    return 9;
                }
                else if (ID == 1043866079)
                {
                    return 10;
                }
                else if (ID == 3604333611)
                {
                    return 11;
                }
                else if (ID == 566427957)
                {
                    return 12;
                }
                else if (ID == 1238687255)
                {
                    return 13;
                }
                else if (ID == 360516555)
                {
                    return 14;
                }
                else if (ID == 3699645660)
                {
                    return 15;
                }
                else if (ID == 3903941485)
                {
                    return 16;
                }
                else if (ID == 703476817)
                {
                    return 17;
                }
                else if (ID == 457312015)
                {
                    return 18;
                }
                else if (ID == 1446710144)
                {
                    return 19;
                }
                else if (ID == 3515803728)
                {
                    return 20;
                }
                else if (ID == 219082534)
                {
                    return 21;
                }
                else if (ID == 1156399922)
                {
                    return 22;
                }
                else if (ID == 1419869192)
                {
                    return 23;
                }
                else if (ID == 526392401)
                {
                    return 24;
                }
                else if (ID == 830603277)
                {
                    return 25;
                }
                else if (ID == 3054323003)
                {
                    return 26;
                }
                else if (ID == 24048714)
                {
                    return 27;
                }
                else if (ID == 3970929146)
                {
                    return 28;
                }
                else if (ID == 2936553558)
                {
                    return 29;
                }
                else if (ID == 2529672877)
                {
                    return 30;
                }
                else if (ID == 1609897361)
                {
                    return 31;
                }
                else if (ID == 3114568527)
                {
                    return 32;
                }
                else if (ID == 479656207)
                {
                    return 33;
                }
                else
                {
                    return -1;
                }
            }
        }
    }
}