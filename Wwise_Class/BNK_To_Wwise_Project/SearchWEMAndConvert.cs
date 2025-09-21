using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace WoTB_Voice_Mod_Creater.Wwise_Class.BNK_To_Wwise_Project
{
    public class SearchWEMAndConvert
    {
        string m_searchPath;
        string m_outputPath;

        HashSet<string> targetFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        // 検出済みファイルを記録する（重複防止）
        ConcurrentDictionary<string, byte> copiedFiles = new ConcurrentDictionary<string, byte>();

        public SearchWEMAndConvert(string actormixierFile, string searchPath, string outputPath)
        {
            if (!File.Exists(actormixierFile))
                return;

            m_searchPath = searchPath;
            m_outputPath = outputPath;

            string[] allLine = File.ReadAllLines(actormixierFile);
            foreach (string line in allLine)
            {
                if (line.Contains("<AudioFile>"))
                {
                    string strValue = line.Substring(line.IndexOf("<AudioFile>") + 11);
                    strValue = strValue.Substring(0, strValue.LastIndexOf("."));
                    targetFileNames.Add(strValue);
                }
            }
        }

        public int Execute()
        {
            // 検索対象の全ファイルを再帰的に列挙
            string[] allFiles = Directory.GetFiles(m_searchPath, "*.wem", SearchOption.AllDirectories);

            int count = 0;

            Parallel.ForEach(allFiles, filePath =>
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);

                if (targetFileNames.Contains(fileName) &&
                    copiedFiles.TryAdd(fileName, 0)) //初回のみ許可
                {
                    string destPath = Path.Combine(m_outputPath, fileName + ".wav");
                    Sub_Code.WEM_To_File(filePath, destPath, "wav", false);
                    count++;
                }
            });

            return count;
        }
    }
}
