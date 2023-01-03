using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WoTB_Voice_Mod_Creater.Wwise_Class
{
    public class WoT_Create_XML
    {
        List<string> All_BNK = new List<string>();
        Dictionary<string, string> All_Change = new Dictionary<string, string>();
        public void Add_BNK(string BNK_Name)
        {
            if (!All_BNK.Contains(BNK_Name))
                All_BNK.Add(BNK_Name);
        }
        public void Add_Change_Event(string From_Event, string To_Event)
        {
            if (!All_Change.ContainsKey(From_Event))
                All_Change.Add(From_Event, To_Event);
        }
        public void Create(string To_File)
        {
            StreamWriter stw = new StreamWriter(To_File, false);
            stw.WriteLine("<audio_mods.xml>");
            stw.WriteLine("\t<loadBanks>");
            foreach (string BNK in All_BNK)
            {
                stw.WriteLine("\t\t<bank>");
                stw.WriteLine("\t\t\t<name>" + BNK + "</name>");
                stw.WriteLine("\t\t</bank>");
            }
            stw.WriteLine("\t</loadBanks>");
            stw.WriteLine("\t<events>");
            foreach (string Key_Name in All_Change.Keys)
            {
                stw.WriteLine("\t\t<event>");
                stw.WriteLine("\t\t\t<name>" + Key_Name + "</name>");
                stw.WriteLine("\t\t\t<mod>" + All_Change[Key_Name] + "</mod>");
                stw.WriteLine("\t\t</event>");
            }
            stw.WriteLine("\t</events>");
            stw.Write("</audio_mods.xml>");
            stw.Close();
        }
        public void Dispose()
        {
            All_BNK.Clear();
            All_Change.Clear();
        }
    }
}