using System;
using System.Collections.Generic;
using System.IO;

namespace WoTB_Voice_Mod_Creater.Wwise_Class.BNK_To_Wwise_Project
{
    public class SoundBanks
    {
        //SoundBanksの情報をファイルに情報
        public static readonly string SoundBanksUnit = Guid.NewGuid().ToString().ToUpper();
        public static void Create(string To_File)
        {
            if (!Directory.Exists(Path.GetDirectoryName(To_File)))
                Directory.CreateDirectory(Path.GetDirectoryName(To_File));
            List<string> SoundBankNames = new List<string>();
            for (int Number = 0; Number < Events.Event_Info.Count; Number++)
            {
                foreach (Event_Action Action in Events.Event_Info[Number].Action)
                {
                    if (Action.SoundBank != null && Action.SoundBank != "")
                    {
                        Events.Event_Info[Number].SoundBankName = Action.SoundBank;
                        if (!SoundBankNames.Contains(Action.SoundBank))
                            SoundBankNames.Add(Action.SoundBank);
                    }
                }
            }
            List<string> Write_All = new List<string>();
            Write_All.Add("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            Write_All.Add("<WwiseDocument Type=\"WorkUnit\" ID=\"{" + SoundBanksUnit + "}\" SchemaVersion=\"97\">");
            Write_All.Add("<SoundBanks>");
            Write_All.Add("<WorkUnit Name=\"Default Work Unit\" ID=\"{" + SoundBanksUnit + "}\" PersistMode=\"Standalone\">");
            Write_All.Add("<ChildrenList>");
            foreach (string Name in SoundBankNames)
            {
                Write_All.Add("<SoundBank Name=\"" + Name + "\" ID=\"{" + Guid.NewGuid().ToString().ToUpper() + "}\">");
                Write_All.Add("<ObjectInclusionList>");
                foreach (Event_Parent Event_Now in Events.Event_Info)
                {
                    if (Event_Now.SoundBankName == Name)
                        Write_All.Add("<ObjectRef Name=\"" + Event_Now.Name + "\" ID=\"{" + Event_Now.GUID + "}\" WorkUnitID=\"{" + Events.RootDocumentID + "}\" Origin=\"Manual\" Filter=\"7\"/>");
                }
                Write_All.Add("</ObjectInclusionList>");
                Write_All.Add("<ObjectExclusionList/>");
                Write_All.Add("<GameSyncExclusionList/>");
                Write_All.Add("</SoundBank>");
            }
            Write_All.Add("</ChildrenList>");
            Write_All.Add("</WorkUnit>");
            Write_All.Add("</SoundBanks>");
            Write_All.Add("</WwiseDocument>");
            File.WriteAllLines(To_File, Write_All);
            Write_All.Clear();
        }
    }
}