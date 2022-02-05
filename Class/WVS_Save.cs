using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WoTB_Voice_Mod_Creater.Class
{
    //Mod Creater専用のセーブファイルをバイナリデータとして作成
    //このセーブファイルは、サウンドファイルも一緒に書き込まれるため、別のPCでロードしても正常に再生やModの作成が行えます。
    public class WVS_Save
    {
        //ヘッダ情報(WVSFormatの9バイトとバージョン情報の2バイトは確定)
        public const string WVS_Header = "WVSFormat";
        public const ushort WVS_Version = 1;
        List<string> Sound_Files = new List<string>();
        //4バイト
        List<int> Sound_Indexes = new List<int>();
        //2バイト
        List<ushort> List_Indexes = new List<ushort>();
        List<byte[]> Sound_Binary = new List<byte[]>();
        //List_IndexはVoice_List_Full_File_Nameが0、Voice_Sub_List_Full_File_Nameが1、Voice_Three_List_Full_File_Nameが2と決まっています。
        //WVS_Fileは、.wvsファイルがロードされていない場合はnullを指定します。
        //ここでWVS_Fileを指定かつ、セーブファイルを上書きする場合はCreate()を実行する前に必ずWVS_File.Dispose()を実行する必要があります。
        public void Add_Sound(List<List<string>> Voice_Full_File_Name, WVS_Load WVS_File, ushort List_Index)
        {
            for (int Number = 0; Number < Voice_Full_File_Name.Count; Number++)
            {
                for (int Number_01 = 0; Number_01 < Voice_Full_File_Name[Number].Count; Number_01++)
                {
                    Sound_Files.Add(Voice_Full_File_Name[Number][Number_01]);
                    Sound_Indexes.Add(Number);
                    List_Indexes.Add(List_Index);
                    if (WVS_File != null && !Voice_Full_File_Name[Number][Number_01].Contains("\\"))
                        Sound_Binary.Add(WVS_File.Get_Sound_Bytes(List_Index, Number, Number_01));
                    else
                        Sound_Binary.Add(null);
                }
            }
        }
        public void Create(string To_File, string Project_Name, bool IsNotChangeName)
        {
            if (File.Exists(To_File))
                File.Delete(To_File);
            BinaryWriter bin = new BinaryWriter(File.OpenWrite(To_File));
            //ヘッダー
            bin.Write(Encoding.ASCII.GetBytes(WVS_Header));
            //謎の4バイト
            bin.Write(new byte[] { 0x00, 0x00, 0x00, 0x00 });
            //ロード時にWVS_Versionの値が異なればロードできないようにする
            bin.Write(WVS_Version);
            bin.Write(Sound_Files.Count);
            byte[] Project_Name_Byte = Encoding.UTF8.GetBytes(Project_Name);
            //プロジェクト名のバイト数
            bin.Write(Project_Name_Byte.Length);
            bin.Write(Project_Name_Byte);
            bin.Write(IsNotChangeName);
            //改行1バイト
            bin.Write((byte)0x0a);
            for (int Number = 0; Number < Sound_Files.Count; Number++)
            {
                bin.Write(List_Indexes[Number]);
                bin.Write(Sound_Indexes[Number]);
                byte[] File_Name_Byte = Encoding.UTF8.GetBytes(Path.GetFileName(Sound_Files[Number]));
                //ファイル名のバイト数(ロード時にその長さを読み取ってその長さぶん文字を読み取ります。)
                bin.Write(File_Name_Byte.Length);
                bin.Write(File_Name_Byte);
                bin.Write((byte)0x0a);
                //サウンドのバイト数
                if (Sound_Binary[Number] != null)
                {
                    bin.Write(Sound_Binary[Number].Length);
                    bin.Write(Sound_Binary[Number]);
                }
                else
                {
                    byte[] Sound_File_Bytes = File.ReadAllBytes(Sound_Files[Number]);
                    bin.Write(Sound_File_Bytes.Length);
                    //System.Windows.MessageBox.Show(Number + "個目:" + Sound_Files[Number] + "\n" + Sound_File_Bytes.Length);
                    bin.Write(Sound_File_Bytes);
                }
            }
            bin.Close();
        }
        public void Dispose()
        {
            Sound_Files.Clear();
            Sound_Indexes.Clear();
            List_Indexes.Clear();
            Sound_Binary.Clear();
        }
    }
    public class WVS_Load
    {
        public string Project_Name { get; private set; }
        public bool IsNotChangeNameMode { get; private set; }
        List<List<long>> Voice_Main_List_Sound_Position = new List<List<long>>();
        List<List<long>> Voice_Sub_List_Sound_Position = new List<List<long>>();
        List<List<long>> Voice_Three_List_Sound_Position = new List<List<long>>();
        BinaryReader bin = null;
        string WVS_File = "";
        public bool IsLoaded = false;
        //サウンドが含まれている.wvsファイルか確認(そうであればtrue)
        public static bool IsFullWVSFile(string From_File)
        {
            if (!File.Exists(From_File))
                return false;
            BinaryReader bin = new BinaryReader(File.OpenRead(From_File));
            string Header = Encoding.ASCII.GetString(bin.ReadBytes(9));
            bin.Close();
            if (Header == "WVSFormat")
                return true;
            return false;
        }
        //WVSファイルをロードします。この時点ではサウンドを読み込まないため、サウンドを取得するときはGet_Sound_Bytes()を実行する必要があります。(メモリ使用率を抑えるため)
        public bool WVS_Load_File(string From_File, List<List<string>> Voice_List_Full_File_Name, List<List<string>> Voice_Sub_List_Full_File_Name, List<List<string>> Voice_Three_List_Full_File_Name)
        {
            if (!File.Exists(From_File))
                return false;
            Dispose();
            for (int Number = 0; Number < Voice_List_Full_File_Name.Count; Number++)
            {
                Voice_List_Full_File_Name[Number].Clear();
                Voice_Main_List_Sound_Position.Add(new List<long>());
            }
            for (int Number = 0; Number < Voice_Sub_List_Full_File_Name.Count; Number++)
            {
                Voice_Sub_List_Full_File_Name[Number].Clear();
                Voice_Sub_List_Sound_Position.Add(new List<long>());
            }
            for (int Number = 0; Number < Voice_Three_List_Full_File_Name.Count; Number++)
            {
                Voice_Three_List_Full_File_Name[Number].Clear();
                Voice_Three_List_Sound_Position.Add(new List<long>());
            }
            List<string> Sound_Files = new List<string>();
            List<int> Sound_Indexes = new List<int>();
            bin = new BinaryReader(File.OpenRead(From_File));
            //ヘッダーが異なればfalse
            if (Encoding.ASCII.GetString(bin.ReadBytes(9)) != "WVSFormat")
                return false;
            //4バイトスキップ
            bin.ReadBytes(4);
            //セーブファイルのバージョンが異なればfalse
            if (bin.ReadUInt16() != WVS_Save.WVS_Version)
                return false;
            //ファイル内のサウンド数
            int Sound_Count = bin.ReadInt32();
            //プロジェクト名のバイト数
            int Project_Name_Byte = bin.ReadInt32();
            //バイト数ぶん読み取る
            Project_Name = Encoding.UTF8.GetString(bin.ReadBytes(Project_Name_Byte));
            //プロジェクト名の変更が可能かどうかを取得
            IsNotChangeNameMode = bin.ReadBoolean();
            bin.ReadByte();
            for (int Number = 0; Number < Sound_Count; Number++)
            {
                ushort List_Index = bin.ReadUInt16();
                int Sound_Index = bin.ReadInt32();
                int File_Name_Count = bin.ReadInt32();
                string File_Name = Encoding.UTF8.GetString(bin.ReadBytes(File_Name_Count));
                bin.ReadByte();
                //サウンドが開始される地点を保存しておく(Get_Sound_Bytes()でその地点のサウンドをbyte[]形式で読み取れます)
                if (List_Index == 0)
                {
                    Voice_Main_List_Sound_Position[Sound_Index].Add(bin.BaseStream.Position);
                    Voice_List_Full_File_Name[Sound_Index].Add(File_Name);
                }
                else if (List_Index == 1)
                {
                    Voice_Sub_List_Sound_Position[Sound_Index].Add(bin.BaseStream.Position);
                    Voice_Sub_List_Full_File_Name[Sound_Index].Add(File_Name);
                }
                else if (List_Index == 2)
                {
                    Voice_Three_List_Sound_Position[Sound_Index].Add(bin.BaseStream.Position);
                    Voice_Three_List_Full_File_Name[Sound_Index].Add(File_Name);
                }
                //サウンドの長さを取得し、次のサウンドの位置までスキップ
                int Sound_Length = bin.ReadInt32();
                bin.BaseStream.Seek(Sound_Length, SeekOrigin.Current);
            }
            WVS_File = From_File;
            IsLoaded = true;
            return true;
        }
        private byte[] Load_Sound(long Start_Position)
        {
            bin.BaseStream.Seek(Start_Position, SeekOrigin.Begin);
            //サウンドのバイト数を取得し、その長さぶん読み取る
            int Sound_Length = bin.ReadInt32();
            return bin.ReadBytes(Sound_Length);
        }
        public byte[] Get_Sound_Bytes(int List_Index, int Voice_Index, int Sound_Index)
        {
            if (IsLoaded && List_Index >= 0 && List_Index < 3)
            {
                if (List_Index == 0 && Voice_Main_List_Sound_Position.Count > Voice_Index && Voice_Main_List_Sound_Position[Voice_Index].Count > Sound_Index)
                    return Load_Sound(Voice_Main_List_Sound_Position[Voice_Index][Sound_Index]);
                else if (List_Index == 1 && Voice_Sub_List_Sound_Position.Count > Voice_Index && Voice_Sub_List_Sound_Position[Voice_Index].Count > Sound_Index)
                    return Load_Sound(Voice_Sub_List_Sound_Position[Voice_Index][Sound_Index]);
                else if (List_Index == 2 && Voice_Three_List_Sound_Position.Count > Voice_Index && Voice_Three_List_Sound_Position[Voice_Index].Count > Sound_Index)
                    return Load_Sound(Voice_Three_List_Sound_Position[Voice_Index][Sound_Index]);
            }
            return null;
        }
        public bool Sound_To_File(string To_File, int List_Index, int Voice_Index, int Sound_Index)
        {
            if (IsLoaded && List_Index >= 0 && List_Index < 3)
            {
                try
                {
                    File.WriteAllBytes(To_File, Get_Sound_Bytes(List_Index, Voice_Index, Sound_Index));
                    return true;
                }
                catch (Exception e)
                {
                    Sub_Code.Error_Log_Write(e.Message);
                }
            }
            return false;
        }
        public void Delete_Sound(int List_Index, int Voice_Index, int Sound_Index)
        {
            if (IsLoaded && List_Index >= 0 && List_Index < 3)
            {
                if (List_Index == 0 && Voice_Main_List_Sound_Position.Count > Voice_Index && Voice_Main_List_Sound_Position[Voice_Index].Count > Sound_Index)
                    Voice_Main_List_Sound_Position[Voice_Index].RemoveAt(Sound_Index);
                else if (List_Index == 1 && Voice_Sub_List_Sound_Position.Count > Voice_Index && Voice_Sub_List_Sound_Position[Voice_Index].Count > Sound_Index)
                    Voice_Sub_List_Sound_Position[Voice_Index].RemoveAt(Sound_Index);
                else if (List_Index == 2 && Voice_Three_List_Sound_Position.Count > Voice_Index && Voice_Three_List_Sound_Position[Voice_Index].Count > Sound_Index)
                    Voice_Three_List_Sound_Position[Voice_Index].RemoveAt(Sound_Index);
            }
        }
        public void Dispose()
        {
            Voice_Main_List_Sound_Position.Clear();
            Voice_Sub_List_Sound_Position.Clear();
            Voice_Three_List_Sound_Position.Clear();
            if (bin != null)
            {
                bin.Close();
                bin = null;
            }
            IsLoaded = false;
        }
    }
}