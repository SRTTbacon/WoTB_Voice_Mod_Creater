using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace WoTB_Voice_Mod_Creater.Wwise_Class
{
    public class Master_Audio_Bus
    {
        List<string> Read_All = new List<string>();
        List<List<string>> ID_Line = new List<List<string>>();
        List<List<string>> ID_Line_Special = new List<List<string>>();
        bool IsSelected = false;
        public Master_Audio_Bus(string Init_File)
        {
            if (!File.Exists(Init_File))
            {
                IsSelected = false;
                return;
            }
            //bnkファイルを解析(Wwiserを使用)
            string Temp_Name = Sub_Code.Get_Time_Now(DateTime.Now, "-", 1, 6);
            StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Wwise_Parse/BNK_Parse_Start.bat");
            stw.WriteLine("chcp 65001");
            stw.Write("\"" + Voice_Set.Special_Path + "/Wwise_Parse/Python/python.exe\" \"" + Voice_Set.Special_Path + "/Wwise_Parse/wwiser.pyz\" -iv \"" + Init_File + "\" -dn \"" +
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
            Read_All.Clear();
            StreamReader file = new StreamReader(Voice_Set.Special_Path + "/Wwise_Parse/" + Temp_Name + ".xml");
            while ((line = file.ReadLine()) != null)
            {
                Read_All.Add(line);
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
                    if (strValue2 == "CAkBus")
                    {
                        while ((line = file.ReadLine()) != null)
                        {
                            Read_All.Add(line);
                            if (line.Contains("name=\"OverrideBusId\""))
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
            File.Delete(Voice_Set.Special_Path + "/Wwise_Parse/" + Temp_Name + ".xml");
            IsSelected = true;
        }
        //総当たりでShortIDを検索して一致するShortIDの文字列を取得
        public async Task Get_Hash_Name_To_File(string To_File, System.Windows.Controls.TextBlock Message_T)
        {
            if (!IsSelected)
                return;
            StreamWriter stw2 = File.CreateText(To_File);
            stw2.Close();
            List<int> Numbers = new List<int>();
            for (int Number = 0; Number < ID_Line.Count; Number++)
            {
                if (ID_Line[Number][2] == "CAkBus")
                    Numbers.Add(Number);
            }
            foreach (int Number in Numbers)
            {
                string ShortID = ID_Line[Number][0];
                Message_T.Text = ShortID + "を復号化しています...";
                await Task.Delay(50);
                FNV_Hash_Class hasher = new FNV_Hash_Class();
                string Parse = hasher.Bruteforce(8, uint.Parse(ShortID));
                StreamWriter stw = new StreamWriter(To_File, true);
                stw.WriteLine(ShortID + " = " + Parse + " : 親 = " + ID_Line_Special[Number][0]);
                stw.Close();
            }
        }
    }
    class FNV_Hash_Class
    {
        private const uint OffsetBasis = 2166136261;
        private const uint Prime = 16777619;
        private byte[] _bytes;
        private uint[] _hashes;
        private void Initialize(int length)
        {
            _bytes = new byte[length - 1];
            _hashes = new uint[length - 1];
            _bytes[0] = 0x60;
            for (var i = 1; i < _bytes.Length; i++)
            {
                _bytes[i] = 0x5f;
            }
        }
        public string Bruteforce(int Length, uint match)
        {
            Initialize(Length);
            while (true)
            {
                var depth = _bytes.Length - 1;
                while (_bytes.Increment(depth))
                {
                    depth--;
                    if (depth == -1) return "";
                }
                _hashes.ZeroFrom(depth);
                byte lastByte = 0x2f;
                uint tempHash = Hash(_bytes, _hashes, _bytes.Length) * Prime;
                while (!lastByte.Increment(out var nextByte))
                {
                    lastByte = nextByte;
                    uint result = tempHash;
                    result ^= lastByte;
                    if (result == match)
                    {
                        return Encoding.ASCII.GetString(_bytes) + Convert.ToChar(lastByte);
                    }
                }
            }
        }
        static uint Hash(byte[] array, uint[] hashes, int length)
        {
            if (length > 1)
            {
                uint hash = hashes[length - 1];
                if (hash > 0)
                {
                    return hash;
                }
                else
                {
                    hash = Hash(array, hashes, length - 1) * Prime;
                    hash ^= array[length - 1];
                    hashes[length - 1] = hash;
                    return hash;
                }
            }
            else
            {
                uint hash = OffsetBasis;
                hash *= Prime;
                hash ^= array[0];
                hashes[0] = hash;
                return hash;
            }
        }
        public delegate void OnMatchFound(int length, string match);
    }
    static class Utilities
    {
        public static bool Increment(this byte[] array, int i)
        {
            var result = (byte)(array[i] + 1);
            if (result == 0x3a)
            {
                array[i] = 0x61;
                return false;
            }
            else if (result == 0x7b)
            {
                if (i > 0)
                {
                    array[i] = 0x5f;
                    return false;
                }
                else
                {
                    array[i] = 0x61;
                    return true;
                }
            }
            else if (result == 0x60)
            {
                array[i] = 0x30;
                return true;
            }
            array[i] = result;
            return false;
        }
        public static bool Increment(this byte from, out byte result)
        {
            result = (byte)(from + 1);
            if (result == 0x3a)
            {
                result = 0x61;
                return false;
            }
            else if (result == 0x7b)
            {
                result = 0x5f;
                return false;
            }
            else if (result == 0x60)
            {
                result = 0x30;
                return true;
            }
            return false;
        }
        public static void ZeroFrom(this uint[] array, int i)
        {
            while (i < array.Length)
            {
                array[i] = 0;
                i++;
            }
        }
    }
}