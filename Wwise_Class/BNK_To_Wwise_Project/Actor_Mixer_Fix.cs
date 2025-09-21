using System.Collections.Generic;
using System.IO;

namespace WoTB_Voice_Mod_Creater.Wwise_Class.BNK_To_Wwise_Project
{
    class RTPCReference
    {
        public string PropertyName;

        public string RTPCName;
        public string RTPCUUID;
        public string RTPCUnitID;

        public string ReferenceUUID;
        public uint ReferenceShortID;

        public List<string> CurveList = new List<string>();
    }

    class ObjectList
    {
        public uint ParentContainerID;
        
        public List<RTPCReference> ReferenceList = new List<RTPCReference>();
    }

    public class Actor_Mixer_Fix
    {
        List<string> m_allLines;

        Dictionary<uint, ObjectList> m_objects = new Dictionary<uint, ObjectList>();

        string toFilePath = "";

        public Actor_Mixer_Fix(string actorMixerFile)
        {
            if (!File.Exists(actorMixerFile))
                return;

            toFilePath = actorMixerFile;

            m_allLines = new List<string>(File.ReadAllLines(actorMixerFile));
            GetRTPCInfo();
        }

        public void Execute()
        {
            int lineIndex = 0;
            while (lineIndex < m_allLines.Count - 1)
            {
                string line = m_allLines[lineIndex];
                if (line.Contains("<BlendContainer Name=") || line.Contains("<ActorMixer Name=") || line.Contains("<RandomSequenceContainer Name=") || line.Contains("<SwitchContainer Name="))
                {
                    uint containerID = Get_Config.Get_ShortID(line);

                    string containerType = Get_Config.Get_ContainerType(line);

                    SetContainerProperty(containerType, lineIndex, containerID);
                }

                lineIndex++;
            }

            File.WriteAllLines(toFilePath, m_allLines.ToArray());
        }


        //-------------------Private-------------------
        void GetRTPCInfo()
        {
            uint containerID = 0;

            bool bRemoveLine = false;

            int lineIndex = 0;
            while (lineIndex < m_allLines.Count - 1)
            {
                string line = m_allLines[lineIndex];
                if (line.Contains("<BlendContainer Name=") || line.Contains("<ActorMixer Name=") || line.Contains("<RandomSequenceContainer Name=") || line.Contains("<SwitchContainer Name="))
                {
                    containerID = Get_Config.Get_ShortID(line);
                }
                else if (line.Contains("<Property Name=") && m_allLines[lineIndex + 1].Contains("<RTPCList>"))
                {
                    GetRTPCInfo(containerID, lineIndex);
                    bRemoveLine = true;
                }

                if (bRemoveLine)
                {
                    if (line.Contains("</RTPCList>") && m_allLines[lineIndex + 1].Contains("</Property>"))
                    {
                        bRemoveLine = false;
                        m_allLines.RemoveAt(lineIndex);
                    }

                    m_allLines.RemoveAt(lineIndex);
                    lineIndex--;
                }

                lineIndex++;
            }
        }

        //RTPCを用いたエフェクト情報を取得
        void GetRTPCInfo(uint containerID, int index)
        {
            if (!m_objects.ContainsKey(containerID))
            {
                m_objects[containerID] = new ObjectList();
                m_objects[containerID].ParentContainerID = containerID;
            }

            RTPCReference rtpcReference = new RTPCReference();

            rtpcReference.PropertyName = Get_Config.Get_Name(m_allLines[index]);
            rtpcReference.ReferenceUUID = Get_Config.Get_UUID(m_allLines[index + 2]);
            rtpcReference.ReferenceShortID = Get_Config.Get_ShortID(m_allLines[index + 2]);
            rtpcReference.RTPCName = Get_Config.Get_Name(m_allLines[index + 5]);
            rtpcReference.RTPCUUID = Get_Config.Get_UUID(m_allLines[index + 5]);
            rtpcReference.RTPCUnitID = Get_Config.Get_UnitID(m_allLines[index + 5]);

            //Curveの構造は変更ないため行すべて保存
            int startIndex = index + 8;
            while (true)
            {
                string line = m_allLines[startIndex];
                if (line.Contains("</RTPC>"))
                    break;
                rtpcReference.CurveList.Add(line);
                if (line.Contains("</Curve>"))
                    break;

                startIndex++;
            }

            m_objects[containerID].ReferenceList.Add(rtpcReference);
        }

        //コンテナの終了行を取得
        int GetEndContainerLineIndex(string containerType, int startIndex)
        {
            string containerStartHeader = "<" + containerType + " Name=";
            string containerEndHeader = "</" + containerType + ">";

            int skipCount = 1;

            for (int i = startIndex + 1; i < m_allLines.Count; i++)
            {
                string line = m_allLines[i];
                if (line.Contains(containerStartHeader))
                {
                    skipCount++;
                }
                else if (line.Contains(containerEndHeader))
                {
                    skipCount--;
                    if (skipCount <= 0)
                    {
                        return i;
                    }
                }
            }

            return 0;
        }

        void SetContainerProperty(string containerType, int startIndex, uint containerID)
        {
            if (!m_objects.ContainsKey(containerID))
                return;

            int insertIndex = GetEndContainerLineIndex(containerType, startIndex);
            if (insertIndex <= 0)
                return;

            More_Class.List_Init(insertIndex);
            More_Class.List_Add(m_allLines, "<ObjectLists>");
            More_Class.List_Add(m_allLines, "<ObjectList Name=\"RTPC\">");

            foreach (RTPCReference reference in m_objects[containerID].ReferenceList)
            {
                More_Class.List_Add(m_allLines, "<Reference>");
                More_Class.List_Add(m_allLines, "<Local>");
                More_Class.List_Add(m_allLines, "<RTPC Name=\"\" ID=\"" + reference.ReferenceUUID + "\" ShortID=\"" + reference.ReferenceShortID + "\">");
                More_Class.List_Add(m_allLines, "<PropertyList>");
                More_Class.List_Add(m_allLines, "<Property Name=\"PropertyName\" Type=\"string\" Value=\"" + reference.PropertyName + "\"/>");
                More_Class.List_Add(m_allLines, "</PropertyList>");
                More_Class.List_Add(m_allLines, "<ReferenceList>");
                More_Class.List_Add(m_allLines, "<Reference Name=\"ControlInput\">");
                More_Class.List_Add(m_allLines, "<ObjectRef Name=\"" + reference.RTPCName + "\" ID=\"" + reference.RTPCUUID + "\" WorkUnitID=\"" + reference.RTPCUnitID + "\"/>");
                More_Class.List_Add(m_allLines, "</Reference>");
                More_Class.List_Add(m_allLines, "<Reference Name=\"Curve\">");
                More_Class.List_Add(m_allLines, "<Custom>");
                foreach (string line in reference.CurveList)
                    More_Class.List_Add(m_allLines, line);
                More_Class.List_Add(m_allLines, "</Custom>");
                More_Class.List_Add(m_allLines, "</Reference>");
                More_Class.List_Add(m_allLines, "</ReferenceList>");
                More_Class.List_Add(m_allLines, "</RTPC>");
                More_Class.List_Add(m_allLines, "</Local>");
                More_Class.List_Add(m_allLines, "</Reference>");
            }

            More_Class.List_Add(m_allLines, "</ObjectList>");
            More_Class.List_Add(m_allLines, "</ObjectLists>");
        }
    }
}
