using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WoTB_Voice_Mod_Creater.Wwise_Class.BNK_To_Wwise_Project
{
    public class SoundbanksInfo
    {
        //SoundbanksInfo.jsonを指定しない場合
        public static void Init()
        {
            //名前をランダムに生成
            //なるべく文字が被らないようにGenerate_Random_Stringの引数を(6～12)に設定(6～12文字のランダムな文字列を生成)
            for (int Number_01 = 0; Number_01 < BNK_Info.RTPC_Info.Count; Number_01++)
                BNK_Info.RTPC_Info[Number_01].Name = Sub_Code.Generate_Random_String(6, 12);
            for (int Number_01 = 0; Number_01 < Switch.Switch_Info.Count; Number_01++)
            {
                Switch.Switch_Info[Number_01].Name = Sub_Code.Generate_Random_String(6, 12);
                for (int Number_02 = 0; Number_02 < Switch.Switch_Info[Number_01].Children.Count; Number_02++)
                    Switch.Switch_Info[Number_01].Children[Number_02].Name = Sub_Code.Generate_Random_String(6, 12);
            }
            for (int Number_01 = 0; Number_01 < Events.Event_Info.Count; Number_01++)
                Events.Event_Info[Number_01].Name = Sub_Code.Generate_Random_String(6, 12);
            for (int Number_01 = 0; Number_01 < State.State_All_Info.Count; Number_01++)
            {
                State.State_All_Info[Number_01].Name = Sub_Code.Generate_Random_String(6, 12);
                for (int Number_02 = 0; Number_02 < State.State_Child_Info.Count; Number_02++)
                    State.State_Child_Info[Number_02].Name = Sub_Code.Generate_Random_String(6, 12);
            }
        }
        //SoundbanksInfo.jsonを指定する場合
        public static void Init(string SoundbanksInfo)
        {
            List<string> Read_Info = new List<string>();
            Read_Info.AddRange(File.ReadAllLines(SoundbanksInfo));
            for (int Number = 0; Number < Read_Info.Count; Number++)
            {
                try
                {
                    if (Read_Info[Number].Contains("\"ObjectPath\": \"\\\\Game Parameters\\\\"))
                    {
                        uint ShortID = uint.Parse(Get_Value(Read_Info[Number - 2]));
                        string Name = Get_Value(Read_Info[Number - 1]);
                        for (int Number_01 = 0; Number_01 < BNK_Info.RTPC_Info.Count; Number_01++)
                        {
                            if (BNK_Info.RTPC_Info[Number_01].ShortID == ShortID)
                            {
                                BNK_Info.RTPC_Info[Number_01].Name = Name;
                                break;
                            }
                        }
                    }
                    else if (Read_Info[Number].Contains("\"ObjectPath\": \"\\\\Switches\\\\"))
                    {
                        uint ShortID = uint.Parse(Get_Value(Read_Info[Number - 2]));
                        string Name = Get_Value(Read_Info[Number - 1]);
                        for (int Number_01 = 0; Number_01 < Switch.Switch_Info.Count; Number_01++)
                        {
                            if (Switch.Switch_Info[Number_01].ShortID == ShortID)
                            {
                                Switch.Switch_Info[Number_01].Name = Name;
                            }
                            for (int Number_02 = 0; Number_02 < Switch.Switch_Info[Number_01].Children.Count; Number_02++)
                            {
                                if (Switch.Switch_Info[Number_01].Children[Number_02].ShortID == ShortID)
                                {
                                    Switch.Switch_Info[Number_01].Children[Number_02].Name = Name;
                                    break;
                                }
                            }
                        }
                    }
                    else if (Read_Info[Number].Contains("\"ObjectPath\": \"\\\\Events\\\\"))
                    {
                        uint ShortID = uint.Parse(Get_Value(Read_Info[Number - 2]));
                        string Name = Get_Value(Read_Info[Number - 1]);
                        for (int Number_01 = 0; Number_01 < Events.Event_Info.Count; Number_01++)
                        {
                            if (Events.Event_Info[Number_01].ShortID == ShortID)
                            {
                                Events.Event_Info[Number_01].Name = Name;
                                break;
                            }
                        }
                    }
                    else if (Read_Info[Number].Contains("\"ObjectPath\": \"\\\\States\\\\"))
                    {
                        uint ShortID = uint.Parse(Get_Value(Read_Info[Number - 2]));
                        string Name = Get_Value(Read_Info[Number - 1]);
                        for (int Number_01 = 0; Number_01 < State.State_All_Info.Count; Number_01++)
                        {
                            if (State.State_All_Info[Number_01].Short_ID == ShortID)
                            {
                                State.State_All_Info[Number_01].Name = Name;
                                break;
                            }
                        }
                        for (int Number_01 = Number + 2; Number_01 < Read_Info.Count; Number_01++)
                        {
                            if (Read_Info[Number_01].Contains("\"ObjectPath\": \"\\\\States\\\\"))
                            {
                                uint ShortID_Child = uint.Parse(Get_Value(Read_Info[Number_01 - 2]));
                                string Name_Child = Get_Value(Read_Info[Number_01 - 1]);
                                for (int Number_02 = 0; Number_02 < State.State_Child_Info.Count; Number_02++)
                                {
                                    if (State.State_Child_Info[Number_02].Short_ID == ShortID_Child)
                                    {
                                        State.State_Child_Info[Number_02].Name = Name_Child;
                                        break;
                                    }
                                }
                            }
                            else if (Read_Info[Number_01].Contains("]"))
                            {
                                Number = Number_01;
                                break;
                            }
                        }
                    }
                }
                catch { }
            }
            Read_Info.Clear();
        }
        public static async Task Generate_Name(TextBlock Message_T)
        {
            for (int Number_01 = 0; Number_01 < BNK_Info.RTPC_Info.Count; Number_01++)
            {
                Message_T.Text = BNK_Info.RTPC_Info[Number_01].ShortID + "を対応する文字列へ変換しています...";
                await Task.Delay(50);
                string Name = Get_Config.Get_Hash_Name_From_ShortID(BNK_Info.RTPC_Info[Number_01].ShortID);
                BNK_Info.RTPC_Info[Number_01].Name = Name;
            }
            for (int Number_01 = 0; Number_01 < Switch.Switch_Info.Count; Number_01++)
            {
                Message_T.Text = Switch.Switch_Info[Number_01].ShortID + "を対応する文字列へ変換しています...";
                await Task.Delay(50);
                string Name = Get_Config.Get_Hash_Name_From_ShortID(Switch.Switch_Info[Number_01].ShortID);
                Switch.Switch_Info[Number_01].Name = Name;
            }
            for (int Number_01 = 0; Number_01 < Events.Event_Info.Count; Number_01++)
            {
                Message_T.Text = Events.Event_Info[Number_01].ShortID + "を対応する文字列へ変換しています...";
                await Task.Delay(50);
                string Name = Get_Config.Get_Hash_Name_From_ShortID(Events.Event_Info[Number_01].ShortID);
                Events.Event_Info[Number_01].Name = Name;
            }
            for (int Number_01 = 0; Number_01 < State.State_All_Info.Count; Number_01++)
            {
                Message_T.Text = State.State_All_Info[Number_01].Short_ID + "を対応する文字列へ変換しています...";
                await Task.Delay(50);
                string Name = Get_Config.Get_Hash_Name_From_ShortID(State.State_All_Info[Number_01].Short_ID);
                State.State_All_Info[Number_01].Name = Name;
            }
            for (int Number_01 = 0; Number_01 < State.State_Child_Info.Count; Number_01++)
            {
                Message_T.Text = State.State_Child_Info[Number_01].Short_ID + "を対応する文字列へ変換しています...";
                await Task.Delay(50);
                string Name = Get_Config.Get_Hash_Name_From_ShortID(State.State_Child_Info[Number_01].Short_ID);
                State.State_Child_Info[Number_01].Name = Name;
            }
        }
        static string Get_Value(string Read_Line)
        {
            string Temp = Read_Line;
            if (Temp.Contains(":"))
                Temp = Temp.Substring(Temp.IndexOf(':'));
            Temp = Temp.Substring(Temp.IndexOf('"') + 1);
            return Temp.Substring(0, Temp.IndexOf('"'));
        }
    }
}