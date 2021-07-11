using System.Collections.Generic;
using System.IO;

namespace WoTB_Voice_Mod_Creater.Wwise_Class.BNK_To_Wwise_Project
{
    public class SoundbanksInfo
    {
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
                }
                catch { }
            }
            Read_Info.Clear();
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