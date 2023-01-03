
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WoTB_Voice_Mod_Creater.Class
{
    public class Other_File_Type
    {
        public string Full_Path { get; set; }
        public string Full_Path_Source { get; set; }
        public string Name_Text => Full_Path_Source != null ? Path.GetFileName(Full_Path_Source) : Path.GetFileName(Full_Path);
        public Music_Play_Time Music_Time = new Music_Play_Time(0, 0);
        public bool Music_Feed_In = true;
        public Other_File_Type(string Full_Path, string Full_Path_Source = null, bool Music_Feed_In = false, Music_Play_Time Music_Time = null)
        {
            this.Full_Path = Full_Path;
            this.Full_Path_Source = Full_Path_Source;
            this.Music_Feed_In = Music_Feed_In;
            if (Music_Time != null)
                this.Music_Time = Music_Time;
        }
        public Other_File_Type Clone()
        {
            Other_File_Type Temp = (Other_File_Type)MemberwiseClone();
            Temp.Music_Time = Music_Time.Clone();
            return Temp;
        }
    }
    public class Other_Type_List
    {
        public List<Other_File_Type> Files = new List<Other_File_Type>();
        public string Name { get; set; }
        public string Name_Text => Name + " | " + Files.Count + "個";
        public int Index { get; private set; }
        public bool IsMusicType { get; private set; }
        public Other_Type_List(string Name, int Index, bool IsMusicType = false)
        {
            this.Name = Name;
            this.Index = Index;
            this.IsMusicType = IsMusicType;
        }
        public Other_Type_List Clone()
        {
            return (Other_Type_List)MemberwiseClone();
        }
    }
    public class WMS_Save
    {
        public enum WMS_Save_Mode
        {
            All = 0,
            Load_BGM = 1,
            Result = 2,
            Dominance = 3,
            Hit = 4,
            Garage = 5,
            Gun = 6
        }
        private class WMS_Save_ID
        {
            public int Sound_Length { get; private set; }
            public long Position { get; private set; }
            public string Name { get; private set; }
            public WMS_Save_ID(int Sound_Length, long Position, string Name)
            {
                this.Sound_Length = Sound_Length;
                this.Position = Position;
                this.Name = Name;
            }
        }
        public const string WMS_Header = "WMSFormat";
        public const ushort WMS_Version = 3;
        private readonly List<List<Other_Type_List>> Sound_Files = new List<List<Other_Type_List>>();
        private readonly List<List<List<byte[]>>> Sound_Binary = new List<List<List<byte[]>>>();
        private readonly List<WMS_Save_ID> Sound_ID = new List<WMS_Save_ID>();
        public void Add_Sound(List<List<Other_Type_List>> Voice_Full_File_Name, WMS_Load WMS_File, WMS_Save_Mode Save_Mode)
        {
            for (int Number_00 = 0; Number_00 < Voice_Full_File_Name.Count; Number_00++)
            {
                Sound_Files.Add(new List<Other_Type_List>());
                Sound_Binary.Add(new List<List<byte[]>>());
                for (int Number_01 = 0; Number_01 < Voice_Full_File_Name[Number_00].Count; Number_01++)
                {
                    Other_Type_List Before = Voice_Full_File_Name[Number_00][Number_01];
                    List<Other_File_Type> Temp = new List<Other_File_Type>(Before.Files);
                    Sound_Files[Number_00].Add(Before.Clone());
                    Sound_Files[Number_00][Sound_Files[Number_00].Count - 1].Files.Clear();
                    Sound_Binary[Number_00].Add(new List<byte[]>());
                    for (int Number_02 = 0; Number_02 < Temp.Count; Number_02++)
                    {
                        bool IsAdd = false;
                        if (Number_00 == 0 && Number_01 >= 0 && Number_01 <= 10 && (Save_Mode == WMS_Save_Mode.All || Save_Mode == WMS_Save_Mode.Load_BGM))
                            IsAdd = true;
                        else if (Number_00 == 0 && Number_01 >= 11 && Number_01 <= 16 && (Save_Mode == WMS_Save_Mode.All || Save_Mode == WMS_Save_Mode.Result))
                            IsAdd = true;
                        else if (Number_00 == 0 && (Number_01 == 17 || Number_01 == 18) && (Save_Mode == WMS_Save_Mode.All || Save_Mode == WMS_Save_Mode.Dominance))
                            IsAdd = true;
                        else if (Number_00 == 0 && (Number_01 == 19 || Number_01 == 20) && (Save_Mode == WMS_Save_Mode.All || Save_Mode == WMS_Save_Mode.Hit))
                            IsAdd = true;
                        else if (Number_00 == 1 && (Save_Mode == WMS_Save_Mode.All || Save_Mode == WMS_Save_Mode.Garage))
                            IsAdd = true;
                        else if (Number_00 == 2 && (Save_Mode == WMS_Save_Mode.All || Save_Mode == WMS_Save_Mode.Gun))
                            IsAdd = true;
                        if (IsAdd)
                        {
                            Sound_Files[Number_00][Sound_Files[Number_00].Count - 1].Files.Add(Temp[Number_02]);
                            if (WMS_File != null && !Temp[Number_02].Full_Path.Contains("\\"))
                                Sound_Binary[Number_00][Number_01].Add(WMS_File.Get_Sound_Bytes(Number_00, Number_01, Number_02));
                            else
                                Sound_Binary[Number_00][Number_01].Add(null);
                        }
                    }
                }
            }
        }
        public void Create(string To_File, string Project_Name, bool IsIncludeSound, bool IsGunOverWriteMode)
        {
            if (File.Exists(To_File))
                File.Delete(To_File);
            int Sound_Count = 0;
            for (int Number_01 = 0; Number_01 < Sound_Files.Count; Number_01++)
                for (int Number_02 = 0; Number_02 < Sound_Files[Number_01].Count; Number_02++)
                    Sound_Count += Sound_Files[Number_01][Number_02].Files.Count;
            BinaryWriter bin = new BinaryWriter(File.OpenWrite(To_File));
            bin.Write(Encoding.ASCII.GetBytes(WMS_Header));
            bin.Write(new byte[] { 0x00, 0x00, 0x00, 0x00 });
            bin.Write(WMS_Version);
            bin.Write((ushort)Sound_Count);
            byte[] Project_Name_Byte = Encoding.UTF8.GetBytes(Project_Name);
            bin.Write(Project_Name_Byte.Length);
            bin.Write(Project_Name_Byte);
            bin.Write(IsGunOverWriteMode);
            bin.Write((byte)0x0a);
            for (int Number_01 = 0; Number_01 < Sound_Files.Count; Number_01++)
            {
                for (int Number_02 = 0; Number_02 < Sound_Files[Number_01].Count; Number_02++)
                {
                    for (int Number_03 = 0; Number_03 < Sound_Files[Number_01][Number_02].Files.Count; Number_03++)
                    {
                        byte[] File_Name_Byte = Encoding.UTF8.GetBytes(Sound_Files[Number_01][Number_02].Files[Number_03].Name_Text);
                        bin.Write(File_Name_Byte.Length);
                        bin.Write(File_Name_Byte);
                        bin.Write((ushort)Number_01);
                        bin.Write((ushort)Number_02);
                        bin.Write(Sound_Files[Number_01][Number_02].Files[Number_03].Music_Feed_In);
                        bin.Write(Sound_Files[Number_01][Number_02].Files[Number_03].Music_Time.Start_Time);
                        bin.Write(Sound_Files[Number_01][Number_02].Files[Number_03].Music_Time.End_Time);
                        bin.Write((byte)0x0a);
                        bool IsReferSound;
                        if (Sound_Binary[Number_01][Number_02][Number_03] != null)
                        {
                            long Refer_Sound_Position = Get_Refer_Position(Sound_Binary[Number_01][Number_02][Number_03], out IsReferSound);
                            bin.Write(true);
                            bin.Write(IsReferSound);
                            if (IsReferSound)
                                bin.Write(Refer_Sound_Position);
                            else
                            {
                                string Pos = Number_01 + "|" + Number_02 + "|" + Number_03;
                                Sound_ID.Add(new WMS_Save_ID(Sound_Binary[Number_01][Number_02][Number_03].Length, bin.BaseStream.Position, Pos));
                                bin.Write(Sound_Binary[Number_01][Number_02][Number_03].Length);
                                bin.Write(Sound_Binary[Number_01][Number_02][Number_03]);
                            }
                        }
                        else
                        {
                            bin.Write(IsIncludeSound);
                            if (IsIncludeSound)
                            {
                                byte[] Sound_File_Bytes = File.ReadAllBytes(Sound_Files[Number_01][Number_02].Files[Number_03].Full_Path);
                                long Refer_Sound_Position = Get_Refer_Position(Sound_File_Bytes, out IsReferSound);
                                bin.Write(IsReferSound);
                                if (IsReferSound)
                                    bin.Write(Refer_Sound_Position);
                                else
                                {
                                    Sound_ID.Add(new WMS_Save_ID(Sound_File_Bytes.Length, bin.BaseStream.Position, Sound_Files[Number_01][Number_02].Files[Number_03].Full_Path));
                                    bin.Write(Sound_File_Bytes.Length);
                                    bin.Write(Sound_File_Bytes);
                                }
                            }
                            else
                            {
                                byte[] File_Name = Encoding.UTF8.GetBytes(Sound_Files[Number_01][Number_02].Files[Number_03].Full_Path);
                                bin.Write(File_Name.Length);
                                bin.Write(File_Name);
                            }
                        }
                    }
                }
            }
            bin.Close();
        }
        private long Get_Refer_Position(byte[] Sound, out bool IsReferSound)
        {
            IsReferSound = false;
            foreach (WMS_Save_ID Temp in Sound_ID)
            {
                if (Temp.Sound_Length == Sound.Length)
                {
                    if (Temp.Name.Contains("|"))
                    {
                        string[] Temp_Name = Temp.Name.Split('|');
                        int Mod_Index = int.Parse(Temp_Name[0]);
                        int Type_Index = int.Parse(Temp_Name[1]);
                        int Sound_Index = int.Parse(Temp_Name[2]);
                        for (int Number = 0; Number < Sound_Binary[Mod_Index][Type_Index][Sound_Index].Length * 0.9; Number++)
                            if (!Sound[Number].Equals(Sound_Binary[Mod_Index][Type_Index][Sound_Index][Number]))
                                continue;
                        IsReferSound = true;
                        return Temp.Position;
                    }
                    else
                    {
                        byte[] Sound_File_Bytes = File.ReadAllBytes(Temp.Name);
                        for (int Number = 0; Number < Sound.Length * 0.9; Number++)
                            if (!Sound[Number].Equals(Sound_File_Bytes[Number]))
                                continue;
                        IsReferSound = true;
                        return Temp.Position;
                    }
                }
            }
            return -1;
        }
        public void Dispose()
        {
            Sound_Files.Clear();
            Sound_Binary.Clear();
        }
    }
    public class WMS_Load
    {
        public string Project_Name { get; private set; }
        public string WMS_File = "";
        public ushort Version { get; private set; }
        public bool IsNotChangeNameMode { get; private set; }
        public bool IsGunOverWriteMode { get; private set; }
        private readonly List<List<Other_Type_List>> Other_Main_List = new List<List<Other_Type_List>>();
        private readonly List<List<List<long>>> Other_Main_List_Position = new List<List<List<long>>>();
        private readonly List<List<List<bool>>> Other_Main_List_Position_IsIncludeSound = new List<List<List<bool>>>();
        private BinaryReader bin = null;
        public bool IsLoaded = false;
        //サウンドが含まれている.wmsファイルか確認(そうであればtrue)
        public static bool IsFullWMSFile(string From_File)
        {
            if (!File.Exists(From_File))
                return false;
            try
            {
                BinaryReader bin = new BinaryReader(File.OpenRead(From_File));
                string Header = Encoding.ASCII.GetString(bin.ReadBytes(9));
                if (Header != WMS_Save.WMS_Header)
                    return false;
                _ = bin.ReadBytes(4);
                ushort Version = bin.ReadUInt16();
                ushort Sound_Count = bin.ReadUInt16();
                int Project_Name_Byte = bin.ReadInt32();
                _ = Encoding.UTF8.GetString(bin.ReadBytes(Project_Name_Byte));
                if (Version >= 3)
                    _ = bin.ReadBoolean();
                _ = bin.ReadByte();
                for (ushort Number_01 = 0; Number_01 < Sound_Count; Number_01++)
                {
                    int Sound_Name_Length = bin.ReadInt32();
                    _ = Encoding.UTF8.GetString(bin.ReadBytes(Sound_Name_Length));
                    _ = bin.ReadUInt16();
                    _ = bin.ReadUInt16();
                    _ = bin.ReadBoolean();
                    _ = bin.ReadDouble();
                    _ = bin.ReadDouble();
                    _ = bin.ReadByte();
                    bool IsIncludeSound = bin.ReadBoolean();
                    if (IsIncludeSound)
                    {
                        bool IsReferMode = bin.ReadBoolean();
                        if (IsReferMode)
                            _ = bin.ReadInt64();
                        if (!IsReferMode)
                        {
                            int Sound_Length = bin.ReadInt32();
                            _ = bin.BaseStream.Seek(Sound_Length, SeekOrigin.Current);
                        }
                    }
                    else
                    {
                        int Name_Length = bin.ReadInt32();
                        _ = bin.BaseStream.Seek(Name_Length, SeekOrigin.Current);
                    }
                }
                bin.Close();
                return true;
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                return false;
            }
        }
        //バージョンが異なればTrue
        public static ushort VersionCheck(string From_File)
        {
            if (!File.Exists(From_File))
                return 0;
            BinaryReader bin = new BinaryReader(File.OpenRead(From_File));
            _ = Encoding.ASCII.GetString(bin.ReadBytes(9));
            _ = bin.ReadBytes(4);
            ushort Version = bin.ReadUInt16();
            bin.Close();
            return Version;
        }
        //WMSファイルをロードします。この時点ではサウンドを読み込まないため、サウンドを取得するときはGet_Sound_Bytes()を実行する必要があります。(メモリ使用率を抑えるため)
        public bool WMS_Load_File(string From_File, List<List<Other_Type_List>> Other_Main)
        {
            if (!File.Exists(From_File))
                return false;
            Dispose();
            for (int Number = 0; Number < Other_Main.Count; Number++)
            {
                Other_Main_List.Add(new List<Other_Type_List>());
                Other_Main_List_Position.Add(new List<List<long>>());
                Other_Main_List_Position_IsIncludeSound.Add(new List<List<bool>>());
                for (int Number_01 = 0; Number_01 < Other_Main[Number].Count; Number_01++)
                {
                    Other_Main_List[Number].Add(new Other_Type_List(Other_Main[Number][Number_01].Name, Other_Main[Number][Number_01].Index, Other_Main[Number][Number_01].IsMusicType));
                    Other_Main[Number][Number_01].Files.Clear();
                    Other_Main_List[Number].Add(new Other_Type_List(Other_Main[Number][Number_01].Name, Other_Main[Number][Number_01].Index));
                    Other_Main_List_Position[Number].Add(new List<long>());
                    Other_Main_List_Position_IsIncludeSound[Number].Add(new List<bool>());
                }
            }
            bin = new BinaryReader(File.OpenRead(From_File));
            if (Encoding.ASCII.GetString(bin.ReadBytes(9)) != WMS_Save.WMS_Header)
                return false;
            _ = bin.ReadBytes(4);
            Version = bin.ReadUInt16();
            ushort Sound_Count = bin.ReadUInt16();
            int Project_Name_Byte = bin.ReadInt32();
            Project_Name = Encoding.UTF8.GetString(bin.ReadBytes(Project_Name_Byte));
            if (Version >= 3)
                IsGunOverWriteMode = bin.ReadBoolean();
            _ = bin.ReadByte();
            for (ushort Number_01 = 0; Number_01 < Sound_Count; Number_01++)
            {
                int Sound_Name_Length = bin.ReadInt32();
                string Name = Encoding.UTF8.GetString(bin.ReadBytes(Sound_Name_Length));
                ushort Number_02 = bin.ReadUInt16();
                ushort Number_03 = bin.ReadUInt16();
                bool IsFeedInMode = bin.ReadBoolean();
                double Start_Time = bin.ReadDouble();
                double End_Time = bin.ReadDouble();
                _ = bin.ReadByte();
                Other_Main_List[Number_02][Number_03].Files.Add(new Other_File_Type(Name, null, IsFeedInMode, new Music_Play_Time(Start_Time, End_Time)));
                Other_Main[Number_02][Number_03].Files.Add(new Other_File_Type(Name, null, IsFeedInMode, new Music_Play_Time(Start_Time, End_Time)));
                bool IsIncludeSound = bin.ReadBoolean();
                if (IsIncludeSound)
                {
                    bool IsReferMode = bin.ReadBoolean();
                    if (IsReferMode)
                        Other_Main_List_Position[Number_02][Number_03].Add(bin.ReadInt64());
                    else
                    {
                        Other_Main_List_Position[Number_02][Number_03].Add(bin.BaseStream.Position);
                        int Sound_Length = bin.ReadInt32();
                        _ = bin.BaseStream.Seek(Sound_Length, SeekOrigin.Current);
                    }
                    Other_Main_List_Position_IsIncludeSound[Number_02][Number_03].Add(true);
                }
                else
                {
                    Other_Main_List_Position[Number_02][Number_03].Add(bin.BaseStream.Position);
                    int Name_Length = bin.ReadInt32();
                    Other_Main[Number_02][Number_03].Files[Other_Main[Number_02][Number_03].Files.Count - 1].Full_Path = Encoding.UTF8.GetString(bin.ReadBytes(Name_Length));
                    Other_Main_List_Position_IsIncludeSound[Number_02][Number_03].Add(false);
                }
            }
            WMS_File = From_File;
            IsLoaded = true;
            return true;
        }
        private byte[] Load_Sound(long Start_Position, bool IsIncludeSound)
        {
            _ = bin.BaseStream.Seek(Start_Position, SeekOrigin.Begin);
            //サウンドのバイト数を取得し、その長さぶん読み取る
            int Sound_Length = bin.ReadInt32();
            if (IsIncludeSound)
                return bin.ReadBytes(Sound_Length);
            else
            {
                string File_Name = Encoding.UTF8.GetString(bin.ReadBytes(Sound_Length));
                if (File.Exists(File_Name))
                    return File.ReadAllBytes(File_Name);
                else
                    return new byte[] { };
            }
        }
        public byte[] Get_Sound_Bytes(int List_Index, int Voice_Index, int Sound_Index)
        {
            if (IsLoaded && List_Index >= 0 && List_Index < 3)
                if (Other_Main_List_Position.Count > List_Index && Other_Main_List_Position[List_Index].Count > Voice_Index && Other_Main_List_Position[List_Index][Voice_Index].Count > Sound_Index)
                    return Load_Sound(Other_Main_List_Position[List_Index][Voice_Index][Sound_Index], Other_Main_List_Position_IsIncludeSound[List_Index][Voice_Index][Sound_Index]);
            return null;
        }
        public string Get_Sound_Path(int List_Index, int Voice_Index, int Sound_Index)
        {
            _ = bin.BaseStream.Seek(Other_Main_List_Position[List_Index][Voice_Index][Sound_Index], SeekOrigin.Begin);
            int Sound_Length = bin.ReadInt32();
            return Encoding.UTF8.GetString(bin.ReadBytes(Sound_Length));
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
                catch { }
            }
            return false;
        }
        public void All_Sound_To_Directory(string To_Dir)
        {
            if (!Directory.Exists(To_Dir))
                Directory.CreateDirectory(To_Dir);
            for (int Number_01 = 0; Number_01 < 3; Number_01++)
            {
                for (int Number_02 = 0; Number_02 < Other_Main_List[Number_01].Count; Number_02++)
                {
                    for (int Number_03 = 0; Number_03 < Other_Main_List[Number_01][Number_02].Files.Count; Number_03++)
                    {
                        try
                        {
                            Sound_To_File(To_Dir + "/" + Other_Main_List[Number_01][Number_02].Files[Number_03].Full_Path, Number_01, Number_02, Number_03);
                        }
                        catch { }
                    }
                }
            }
        }
        public void Delete_Sound(int List_Index, int Voice_Index, int Sound_Index)
        {
            if (IsLoaded && List_Index >= 0 && List_Index < 3)
            {
                if (Other_Main_List_Position.Count > List_Index && Other_Main_List_Position[List_Index].Count > Voice_Index && Other_Main_List_Position[List_Index][Voice_Index].Count > Sound_Index)
                {
                    Other_Main_List[List_Index][Voice_Index].Files.RemoveAt(Sound_Index);
                    Other_Main_List_Position[List_Index][Voice_Index].RemoveAt(Sound_Index);
                    Other_Main_List_Position_IsIncludeSound[List_Index][Voice_Index].RemoveAt(Sound_Index);
                }
            }
        }
        public void Dispose()
        {
            if (!IsLoaded)
                return;
            Project_Name = "";
            IsNotChangeNameMode = false;
            WMS_File = "";
            Other_Main_List.Clear();
            Other_Main_List_Position.Clear();
            Other_Main_List_Position_IsIncludeSound.Clear();
            if (bin != null)
            {
                bin.Close();
                bin = null;
            }
            IsLoaded = false;
        }
    }
}