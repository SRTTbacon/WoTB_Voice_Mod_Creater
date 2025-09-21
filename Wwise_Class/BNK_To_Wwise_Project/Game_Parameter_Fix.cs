using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoTB_Voice_Mod_Creater.Wwise_Class.BNK_To_Wwise_Project
{
    class ParamInfo
    {
        public string Name { get; set; }
        public uint ID { get; set; }

        public ParamInfo(string name, uint id)
        {
            Name = name;
            ID = id;
        }
    }

    public class Game_Parameter_Fix
    {
        public static int GameParameter_Fix_Execute(string fromEventFile, string toEventFile)
        {
            if (!File.Exists(fromEventFile) || !File.Exists(toEventFile))
                return 0;

            List<ParamInfo> eventInfos = new List<ParamInfo>();
            string[] lines = File.ReadAllLines(fromEventFile);
            foreach (string line in lines)
            {
                if (line.Contains("<GameParameter Name="))
                {
                    string name = Get_Config.Get_Name(line);
                    uint id = WwiseHash.HashString(name);
                    eventInfos.Add(new ParamInfo(name, id));
                }
            }

            int count = 0;

            List<string> allLine = new List<string>(File.ReadAllLines(toEventFile));
            for (int i = 0; i < allLine.Count; i++)
            {
                string line = allLine[i];
                if (line.Contains("<GameParameter Name="))
                {
                    uint id = WwiseHash.HashString(Get_Config.Get_Name(line));

                    string first = line.Substring(0, line.IndexOf("<GameParameter Name=") + 21);
                    string end = line.Substring(line.IndexOf("ID=") - 2);

                    foreach (ParamInfo info in eventInfos)
                    {
                        if (info.ID == id)
                        {
                            allLine[i] = first + info.Name + end;
                            count++;
                            break;
                        }
                    }
                }
            }

            File.WriteAllLines(toEventFile, allLine);

            return count;
        }
    }
}
