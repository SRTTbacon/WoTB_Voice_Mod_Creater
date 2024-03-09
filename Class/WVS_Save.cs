using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using WoTB_Voice_Mod_Creater.Properties;

namespace WoTB_Voice_Mod_Creater.Class
{
    //Mod Creator専用のセーブファイルをバイナリデータとして作成
    //このセーブファイルは、サウンドファイルも一緒に書き込まれるため、別のPCでロードしても正常に再生やModの作成が行えます。
    public class WVS_Save
    {
        class WVS_WoT_Format_Child
        {
            public List<Voice_Sound_Setting> Sounds = new List<Voice_Sound_Setting>();
            public List<byte[]> Sound_Binary = new List<byte[]>();
            public byte Type_Index;
            public WVS_WoT_Format_Child(byte Voice_Index)
            {
                this.Type_Index = Voice_Index;
            }
        }
        class WVS_WoT_Format_Parent
        {
            public string Key_Name;
            public byte Page_Index;
            public List<WVS_WoT_Format_Child> Childs = new List<WVS_WoT_Format_Child>();
            public WVS_WoT_Format_Parent(string Key_Name, byte Page_Index)
            {
                this.Key_Name = Key_Name;
                this.Page_Index = Page_Index;
            }
        }
        //ヘッダ情報(WVSFormatの9バイトとバージョン情報の2バイトは確定)
        public const string WVS_Header = "WVSFormat";
        public const ushort WVS_Version = 4;
        public readonly bool WVS_IsWoT = false;
        List<List<Voice_Event_Setting>> Blitz_Events;
        List<Dictionary<string, Voice_Event_Setting>> WoT_Events;
        Dictionary<string, List<SE_Info_Parent>> SE_Info;
        readonly List<WVS_WoT_Format_Parent> WoT_Infos = new List<WVS_WoT_Format_Parent>();
        readonly List<Voice_Sound_Setting> Sound_Files = new List<Voice_Sound_Setting>();
        //4バイト
        readonly List<int> Sound_Indexes = new List<int>();
        //2バイト
        readonly List<ushort> List_Indexes = new List<ushort>();
        readonly List<byte[]> Sound_Binary = new List<byte[]>();
        public WVS_Save()
        {
        }
        public WVS_Save(bool IsWoT)
        {
            WVS_IsWoT = IsWoT;
        }
        //WVS_Fileは、.wvsファイルがロードされていない場合はnullを指定します。
        //ここでWVS_Fileを指定かつ、セーブファイルを上書きする場合はCreate()を実行する前に必ずWVS_File.Dispose()を実行する必要があります。
        public void Add_Sound(List<List<Voice_Event_Setting>> Blitz_Events, WVS_Load WVS_File)
        {
            this.Blitz_Events = Blitz_Events;
            for (int Number_01 = 0; Number_01 < Blitz_Events.Count; Number_01++)
            {
                for (int Number_02 = 0; Number_02 < Blitz_Events[Number_01].Count; Number_02++)
                {
                    for (int Number_03 = 0; Number_03 < Blitz_Events[Number_01][Number_02].Sounds.Count; Number_03++)
                    {
                        Sound_Files.Add(Blitz_Events[Number_01][Number_02].Sounds[Number_03]);
                        Sound_Indexes.Add(Number_02);
                        List_Indexes.Add((ushort)Number_01);
                        if (WVS_File != null && !Blitz_Events[Number_01][Number_02].Sounds[Number_03].File_Path.Contains("\\"))
                            Sound_Binary.Add(WVS_File.Load_Sound(Blitz_Events[Number_01][Number_02].Sounds[Number_03].Stream_Position));
                        else
                            Sound_Binary.Add(null);
                    }
                }
            }
        }
        public void Add_Sound(List<Dictionary<string, Voice_Event_Setting>> WoT_Events, Dictionary<string, List<SE_Info_Parent>> SE_Info, WVS_Load WVS_File)
        {
            this.WoT_Events = WoT_Events;
            this.SE_Info = SE_Info;
            for (int Number_01 = 0; Number_01 < WoT_Events.Count; Number_01++)
            {
                List<string> Keys = WoT_Events[Number_01].Keys.ToList();
                foreach (string Key_Name in Keys)
                {
                    WoT_Infos.Add(new WVS_WoT_Format_Parent(Key_Name, (byte)Number_01));
                    WVS_WoT_Format_Child Child = new WVS_WoT_Format_Child((byte)Keys.IndexOf(Key_Name));
                    Child.Sounds = WoT_Events[Number_01][Key_Name].Sounds;
                    for (int Number_03 = 0; Number_03 < WoT_Events[Number_01][Key_Name].Sounds.Count; Number_03++)
                    {
                        if (WVS_File != null && !WoT_Events[Number_01][Key_Name].Sounds[Number_03].File_Path.Contains("\\"))
                            Child.Sound_Binary.Add(WVS_File.Load_Sound(WoT_Events[Number_01][Key_Name].Sounds[Number_03].Stream_Position));
                        else
                            Child.Sound_Binary.Add(null);
                    }
                    WoT_Infos[WoT_Infos.Count - 1].Childs.Add(Child);
                }
                Keys.Clear();
            }
            List<string> SE_Keys = SE_Info.Keys.ToList();
            foreach (string SE_Key_Name in SE_Keys)
            {
                for (int Number_01 = 0; Number_01 < SE_Info[SE_Key_Name].Count; Number_01++)
                {
                    if (WVS_File != null && !SE_Info[SE_Key_Name][Number_01].SE_Path.Contains("\\"))
                        SE_Info[SE_Key_Name][Number_01].Sound_Binary = WVS_File.Load_Sound(SE_Info[SE_Key_Name][Number_01].Stream_Position);
                    else
                        SE_Info[SE_Key_Name][Number_01].Sound_Binary = null;
                }
            }
        }
        public void Create(string To_File, string Project_Name, bool IsNotChangeName, bool IsIncludeSound)
        {
            if (File.Exists(To_File))
                File.Delete(To_File);
            List<string> Dir_Names = new List<string>();
            BinaryWriter bin = new BinaryWriter(File.OpenWrite(To_File));
            //ヘッダー
            bin.Write(Encoding.ASCII.GetBytes(WVS_Header));
            //謎の4バイト
            bin.Write(new byte[] { 0x00, 0x00, 0x00, 0x00 });
            //WVSファイルのバージョン
            bin.Write(WVS_Version);
            //フォーマット
            bin.Write(WVS_IsWoT);
            if (WVS_IsWoT)
            {
                bin.Write((byte)WoT_Infos.Count);
                byte[] Project_Name_Byte = Encoding.UTF8.GetBytes(Project_Name);
                bin.Write((byte)Project_Name_Byte.Length);
                bin.Write(Project_Name_Byte);
                bin.Write(IsNotChangeName);
                bin.Write(IsIncludeSound);
                bin.Write((byte)0x0a);
                if (!IsIncludeSound)
                {
                    foreach (Dictionary<string, Voice_Event_Setting> Settings in WoT_Events)
                    {
                        foreach (string Key_Name in Settings.Keys)
                        {
                            foreach (Voice_Sound_Setting Sound in Settings[Key_Name].Sounds)
                            {
                                string Dir = Path.GetDirectoryName(Sound.File_Path);
                                if (Path.IsPathRooted(Sound.File_Path) && !Dir_Names.Contains(Dir))
                                    Dir_Names.Add(Dir);
                            }
                        }
                    }
                    foreach (string Key_Name in SE_Info.Keys)
                    {
                        foreach (string Sound in SE_Info[Key_Name].Select(h => h.SE_Path))
                        {
                            string Dir = Path.GetDirectoryName(Sound);
                            if (Path.IsPathRooted(Sound) && !Dir_Names.Contains(Dir))
                                Dir_Names.Add(Dir);
                        }
                    }
                    bin.Write((ushort)Dir_Names.Count);
                    foreach (string Dir in Dir_Names)
                    {
                        byte[] Dir_Name_Byte = Encoding.UTF8.GetBytes(Dir);
                        bin.Write((byte)Dir_Name_Byte.Length);
                        bin.Write(Dir_Name_Byte);
                    }
                    bin.Write((byte)0x0a);
                }
                bin.Write(Encoding.ASCII.GetBytes("Created by SRTTbacon"));
                foreach (WVS_WoT_Format_Parent Parent in WoT_Infos)
                {
                    bin.Write((byte)0x0a);
                    byte[] Dir_Name_Byte = Encoding.UTF8.GetBytes(Parent.Key_Name);
                    bin.Write((byte)Dir_Name_Byte.Length);
                    bin.Write(Dir_Name_Byte);
                    bin.Write(Parent.Page_Index);
                    bin.Write((byte)Parent.Childs.Count);
                    foreach (WVS_WoT_Format_Child Child in Parent.Childs)
                    {
                        bin.Write(Child.Type_Index);
                        bin.Write((byte)Child.Sounds.Count);
                        for (byte Number = 0; Number < Child.Sounds.Count; Number++)
                        {
                            if (!IsIncludeSound)
                                bin.Write((ushort)Dir_Names.IndexOf(Path.GetDirectoryName(Child.Sounds[Number].File_Path)));
                            byte[] File_Name_Byte = Encoding.UTF8.GetBytes(Path.GetFileName(Child.Sounds[Number].File_Path));
                            bin.Write((byte)File_Name_Byte.Length);
                            bin.Write(File_Name_Byte);
                            if (IsIncludeSound)
                            {
                                if (Child.Sound_Binary[Number] != null)
                                {
                                    bin.Write(Child.Sound_Binary[Number].Length);
                                    bin.Write(Child.Sound_Binary[Number]);
                                }
                                else
                                {
                                    byte[] Sound_File_Bytes = File.ReadAllBytes(Child.Sounds[Number].File_Path);
                                    bin.Write(Sound_File_Bytes.Length);
                                    bin.Write(Sound_File_Bytes);
                                }
                            }
                        }
                    }
                }
                bin.Write((byte)SE_Info.Count);
                foreach (string Key_Name in SE_Info.Keys)
                {
                    byte[] Dir_Name_Byte = Encoding.UTF8.GetBytes(Key_Name);
                    bin.Write((byte)Dir_Name_Byte.Length);
                    bin.Write(Dir_Name_Byte);
                    bin.Write((byte)SE_Info[Key_Name].Count);
                    foreach (SE_Info_Parent SE_File in SE_Info[Key_Name])
                    {
                        if (!IsIncludeSound)
                            bin.Write((ushort)Dir_Names.IndexOf(Path.GetDirectoryName(SE_File.SE_Path)));
                        byte[] File_Name_Byte = Encoding.UTF8.GetBytes(Path.GetFileName(SE_File.SE_Path));
                        bin.Write((byte)File_Name_Byte.Length);
                        bin.Write(File_Name_Byte);
                        if (IsIncludeSound)
                        {
                            if (SE_File.Sound_Binary != null)
                            {
                                bin.Write(SE_File.Sound_Binary.Length);
                                bin.Write(SE_File.Sound_Binary);
                            }
                            else
                            {
                                byte[] Sound_File_Bytes = File.ReadAllBytes(SE_File.SE_Path);
                                bin.Write(Sound_File_Bytes.Length);
                                bin.Write(Sound_File_Bytes);
                            }
                        }
                    }
                }
            }
            else
            {
                //サウンド数
                bin.Write((ushort)Sound_Files.Count);
                byte[] Project_Name_Byte = Encoding.UTF8.GetBytes(Project_Name);
                //プロジェクト名のバイト数
                bin.Write((byte)Project_Name_Byte.Length);
                bin.Write(Project_Name_Byte);
                bin.Write(IsNotChangeName);
                bin.Write(IsIncludeSound);
                //改行1バイト
                bin.Write((byte)0x0a);
                if (!IsIncludeSound)
                {
                    foreach (List<Voice_Event_Setting> Settings in Blitz_Events)
                    {
                        foreach (Voice_Event_Setting Setting in Settings)
                        {
                            foreach (Voice_Sound_Setting Sound in Setting.Sounds)
                            {
                                string Dir = Path.GetDirectoryName(Sound.File_Path);
                                if (Path.IsPathRooted(Sound.File_Path) && !Dir_Names.Contains(Dir))
                                    Dir_Names.Add(Dir);
                            }
                        }
                    }
                    bin.Write((ushort)Dir_Names.Count);
                    foreach (string Dir in Dir_Names)
                    {
                        byte[] Dir_Name_Byte = Encoding.UTF8.GetBytes(Dir);
                        bin.Write((byte)Dir_Name_Byte.Length);
                        bin.Write(Dir_Name_Byte);
                    }
                    bin.Write((byte)0x0a);
                }
                bin.Write((byte)Blitz_Events.Count);
                for (int Type_Index = 0; Type_Index < Blitz_Events.Count; Type_Index++)
                {
                    bin.Write((byte)Blitz_Events[Type_Index].Count);
                    for (int Event_Index = 0; Event_Index < Blitz_Events[Type_Index].Count; Event_Index++)
                    {
                        Voice_Event_Setting Event_Info = Blitz_Events[Type_Index][Event_Index];
                        bin.Write(Event_Info.IsVolumeRange);
                        bin.Write(Event_Info.IsPitchRange);
                        bin.Write(Event_Info.IsLPFRange);
                        bin.Write(Event_Info.IsHPFRange);
                        bin.Write(Event_Info.Volume);
                        bin.Write((sbyte)Event_Info.Pitch);
                        bin.Write((sbyte)Event_Info.Low_Pass_Filter);
                        bin.Write((sbyte)Event_Info.High_Pass_Filter);
                        bin.Write(Event_Info.Volume_Range.Start);
                        bin.Write(Event_Info.Volume_Range.End);
                        bin.Write((sbyte)Event_Info.Pitch_Range.Start);
                        bin.Write((sbyte)Event_Info.Pitch_Range.End);
                        bin.Write((sbyte)Event_Info.LPF_Range.Start);
                        bin.Write((sbyte)Event_Info.LPF_Range.End);
                        bin.Write((sbyte)Event_Info.HPF_Range.Start);
                        bin.Write((sbyte)Event_Info.HPF_Range.End);
                        bin.Write(Event_Info.Delay);
                        bin.Write((byte)Event_Info.Limit_Sound_Instance);
                        bin.Write((byte)Event_Info.When_Limit_Reached);
                        bin.Write((byte)Event_Info.When_Priority_Equal);
                        if (Event_Index == 17)
                            bin.Write((byte)0x0a);
                    }
                    bin.Write((byte)0x0a);
                }
                for (int Number = 0; Number < Sound_Files.Count; Number++)
                {
                    bin.Write((byte)List_Indexes[Number]);
                    bin.Write((byte)Sound_Indexes[Number]);
                    //ファイル名のバイト数(ロード時にその長さを読み取ってその長さぶん文字を読み取ります。)
                    if (!IsIncludeSound)
                        bin.Write((ushort)Dir_Names.IndexOf(Path.GetDirectoryName(Sound_Files[Number].File_Path)));
                    byte[] File_Name_Byte = Encoding.UTF8.GetBytes(Path.GetFileName(Sound_Files[Number].File_Path));
                    bin.Write((byte)File_Name_Byte.Length);
                    bin.Write(File_Name_Byte);
                    bin.Write((byte)0x0a);
                    Voice_Sound_Setting Sound_Info = Sound_Files[Number];
                    bin.Write(Sound_Info.Probability);
                    bin.Write(Sound_Info.Play_Time.Start_Time);
                    bin.Write(Sound_Info.Play_Time.End_Time);
                    bin.Write(Sound_Info.IsVolumeRange);
                    bin.Write(Sound_Info.IsPitchRange);
                    bin.Write(Sound_Info.IsLPFRange);
                    bin.Write(Sound_Info.IsHPFRange);
                    bin.Write(Sound_Info.Volume);
                    bin.Write((sbyte)Sound_Info.Pitch);
                    bin.Write((sbyte)Sound_Info.Low_Pass_Filter);
                    bin.Write((sbyte)Sound_Info.High_Pass_Filter);
                    bin.Write(Sound_Info.Volume_Range.Start);
                    bin.Write(Sound_Info.Volume_Range.End);
                    bin.Write((sbyte)Sound_Info.Pitch_Range.Start);
                    bin.Write((sbyte)Sound_Info.Pitch_Range.End);
                    bin.Write((sbyte)Sound_Info.LPF_Range.Start);
                    bin.Write((sbyte)Sound_Info.LPF_Range.End);
                    bin.Write((sbyte)Sound_Info.HPF_Range.Start);
                    bin.Write((sbyte)Sound_Info.HPF_Range.End);
                    bin.Write(Sound_Info.Delay);
                    bin.Write(Sound_Info.IsFadeIn);
                    bin.Write(Sound_Info.IsFadeOut);
                    if (IsIncludeSound)
                    {
                        //サウンドのバイト数
                        if (Sound_Binary[Number] != null)
                        {
                            bin.Write(Sound_Binary[Number].Length);
                            bin.Write(Sound_Binary[Number]);
                        }
                        else
                        {
                            byte[] Sound_File_Bytes = File.ReadAllBytes(Sound_Files[Number].File_Path);
                            bin.Write(Sound_File_Bytes.Length);
                            bin.Write(Sound_File_Bytes);
                        }
                    }
                }
            }
            bin.Close();
            Dir_Names.Clear();
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
        public enum WVS_Result
        {
            OK,
            Wrong_Version,
            Wrong_Header,
            No_Exist_File,
            WoTMode,
            BlitzMode
        }
        public string Project_Name { get; private set; }
        public bool IsNotChangeNameMode { get; private set; }
        public bool IsIncludedSound = true;
        BinaryReader bin = null;
        public string WVS_File { get; private set; } = "";
        public bool IsLoaded = false;
        private const byte Max_Version = 5;
        //サウンドが含まれている.wvsファイルか確認(そうであればtrue)
        public static WVS_Result IsBlitzWVSFile(string From_File)
        {
            if (!File.Exists(From_File))
                return WVS_Result.No_Exist_File;
            BinaryReader bin = new BinaryReader(File.OpenRead(From_File));
            string Header = Encoding.ASCII.GetString(bin.ReadBytes(9));
            bin.ReadBytes(4);
            ushort Version = bin.ReadUInt16();
            bool IsWoTMode = false;
            if (Version >= 4)
                IsWoTMode = bin.ReadBoolean();
            bin.Close();
            if (Version > Max_Version)
                return WVS_Result.Wrong_Version;
            if (Header != "WVSFormat")
                return WVS_Result.Wrong_Header;
            if (IsWoTMode)
                return WVS_Result.WoTMode;
            return WVS_Result.OK;
        }
        public static WVS_Result IsWoTWVSFile(string From_File)
        {
            if (!File.Exists(From_File))
                return WVS_Result.No_Exist_File;
            BinaryReader bin = new BinaryReader(File.OpenRead(From_File));
            string Header = Encoding.ASCII.GetString(bin.ReadBytes(9));
            bin.ReadBytes(4);
            ushort Version = bin.ReadUInt16();
            bool IsWoTMode = false;
            if (Version >= 4)
                IsWoTMode = bin.ReadBoolean();
            bin.Close();
            if (Version > Max_Version)
                return WVS_Result.Wrong_Version;
            if (Header != "WVSFormat")
                return WVS_Result.Wrong_Header;
            if (!IsWoTMode)
                return WVS_Result.BlitzMode;
            return WVS_Result.OK;
        }
        public bool WVS_Load_File(string From_File, List<Dictionary<string, Voice_Event_Setting>> WoT_Events, Dictionary<string, List<SE_Info_Parent>> SE_Info)
        {
            if (!File.Exists(From_File))
                return false;
            Dispose();
            for (int Number_01 = 0; Number_01 < WoT_Events.Count; Number_01++)
                foreach (string Key_Name in WoT_Events[Number_01].Keys)
                    WoT_Events[Number_01][Key_Name].Sounds.Clear();
            foreach (string key_Name in SE_Info.Keys)
                SE_Info[key_Name].Clear();
            List<string> Dir_Names = new List<string>();
            bin = new BinaryReader(File.OpenRead(From_File));
            //ヘッダーが異なればfalse
            if (Encoding.ASCII.GetString(bin.ReadBytes(9)) != "WVSFormat")
            {
                bin.Close();
                return false;
            }
            //4バイトスキップ
            bin.ReadBytes(4);
            //セーブファイルのバージョン
            ushort Version = bin.ReadUInt16();
            if (Version < 4 || !bin.ReadBoolean())
            {
                bin.Close();
                return false;
            }
            byte WoT_Info_Count = bin.ReadByte();
            Project_Name = Encoding.UTF8.GetString(bin.ReadBytes(bin.ReadByte()));
            IsNotChangeNameMode = bin.ReadBoolean();
            IsIncludedSound = bin.ReadBoolean();
            _ = bin.ReadByte();
            if (!IsIncludedSound)
            {
                ushort Dir_Count = bin.ReadUInt16();
                for (int Number = 0; Number < Dir_Count; Number++)
                    Dir_Names.Add(Encoding.UTF8.GetString(bin.ReadBytes(bin.ReadByte())));
                bin.ReadByte();
            }
            _ = bin.ReadBytes(Encoding.ASCII.GetBytes("Created by SRTTbacon").Length);
            for (byte Number_01 = 0; Number_01 < WoT_Info_Count; Number_01++)
            {
                _ = bin.ReadByte();
                string KeyName = Encoding.UTF8.GetString(bin.ReadBytes(bin.ReadByte()));
                byte Page_Index = bin.ReadByte();
                byte Child_Count = bin.ReadByte();
                for (int Number_02 = 0; Number_02 < Child_Count; Number_02++)
                {
                    byte Type_Index = bin.ReadByte();
                    byte Sound_Count = bin.ReadByte();
                    for (int Number_03 = 0; Number_03 < Sound_Count; Number_03++)
                    {
                        string File_Name = "";
                        if (!IsIncludedSound)
                            File_Name = Dir_Names[bin.ReadUInt16()] + "\\";
                        File_Name += Encoding.UTF8.GetString(bin.ReadBytes(bin.ReadByte()));
                        Voice_Sound_Setting Setting = new Voice_Sound_Setting();
                        Setting.File_Path = File_Name;
                        if (IsIncludedSound)
                        {
                            Setting.Stream_Position = bin.BaseStream.Position;
                            int Sound_Length = bin.ReadInt32();
                            bin.BaseStream.Seek(Sound_Length, SeekOrigin.Current);
                        }
                        WoT_Events[Page_Index][KeyName].Sounds.Add(Setting);
                    }
                }
            }
            byte SE_Type_Count = bin.ReadByte();
            for (int Number_01 = 0; Number_01 < SE_Type_Count; Number_01++)
            {
                string KeyName = Encoding.UTF8.GetString(bin.ReadBytes(bin.ReadByte()));
                byte SE_File_Count = bin.ReadByte();
                for (int Number_02 = 0; Number_02 < SE_File_Count; Number_02++)
                {
                    string File_Name = "";
                    if (!IsIncludedSound)
                        File_Name = Dir_Names[bin.ReadUInt16()] + "\\";
                    File_Name += Encoding.UTF8.GetString(bin.ReadBytes(bin.ReadByte()));
                    SE_Info_Parent Parent = new SE_Info_Parent(File_Name, WoT_Sound_Mod_Settings.SE_ShortIDs[WoT_Sound_Mod_Settings.SE_Info.Keys.ToList().IndexOf(KeyName)]);
                    if (IsIncludedSound)
                    {
                        Parent.Stream_Position = bin.BaseStream.Position;
                        int Sound_Length = bin.ReadInt32();
                        bin.BaseStream.Seek(Sound_Length, SeekOrigin.Current);
                    }
                    SE_Info[KeyName].Add(Parent);
                }
            }
            Sub_Code.Set_Event_ShortID(WoT_Events);
            WVS_File = From_File;
            IsLoaded = true;
            return true;
        }
        //WVSファイルをロードします。この時点ではサウンドを読み込まないため、サウンドを取得するときはGet_Sound_Bytes()を実行する必要があります。(メモリ使用率を抑えるため)
        public bool WVS_Load_File(string From_File, List<List<Voice_Event_Setting>> Voice_List_Full_File_Name)
        {
            if (!File.Exists(From_File))
                return false;
            Dispose();
            for (int Number_01 = 0; Number_01 < Voice_List_Full_File_Name.Count; Number_01++)
                for (int Number_02 = 0; Number_02 < Voice_List_Full_File_Name[Number_01].Count; Number_02++)
                    Voice_List_Full_File_Name[Number_01][Number_02].Sounds.Clear();
            List<string> Dir_Names = new List<string>();
            bin = new BinaryReader(File.OpenRead(From_File));
            //ヘッダーが異なればfalse
            if (Encoding.ASCII.GetString(bin.ReadBytes(9)) != "WVSFormat")
            {
                bin.Close();
                return false;
            }
            //4バイトスキップ
            bin.ReadBytes(4);
            //セーブファイルのバージョン
            ushort Version = bin.ReadUInt16();
            if (Version >= 4)
            {
                bool IsWoTMode = bin.ReadBoolean();
                if (IsWoTMode)
                {
                    bin.Close();
                    return false;
                }
            }
            //ファイル内のサウンド数
            int Sound_Count;
            if (Version < 3)
                Sound_Count = bin.ReadInt32();
            else
                Sound_Count = bin.ReadUInt16();
            //プロジェクト名のバイト数
            int Project_Name_Byte = Version < 3 ? bin.ReadInt32() : bin.ReadByte();
            Project_Name = Encoding.UTF8.GetString(bin.ReadBytes(Project_Name_Byte));
            //プロジェクト名の変更が可能かどうかを取得
            IsNotChangeNameMode = bin.ReadBoolean();
            if (Version >= 2)
                IsIncludedSound = bin.ReadBoolean();
            bin.ReadByte();
            if (!IsIncludedSound && Version >= 3)
            {
                ushort Dir_Count = bin.ReadUInt16();
                for (int Number = 0; Number < Dir_Count; Number++)
                    Dir_Names.Add(Encoding.UTF8.GetString(bin.ReadBytes(bin.ReadByte())));
                bin.ReadByte();
            }
            //WVSファイルのバージョンが2以降であればイベントの設定を取得
            if (Version >= 2)
            {
                ushort Type_Count;
                if (Version < 3)
                    Type_Count = bin.ReadUInt16();
                else
                    Type_Count = bin.ReadByte();
                for (ushort Type_Index = 0; Type_Index < Type_Count; Type_Index++)
                {
                    ushort Event_Count;
                    if (Version < 3)
                        Event_Count = bin.ReadUInt16();
                    else
                        Event_Count = bin.ReadByte();
                    for (ushort Event_Index = 0; Event_Index < Event_Count; Event_Index++)
                    {
                        Voice_Event_Setting Event_Info = new Voice_Event_Setting()
                        {
                            IsLoadMode = true,
                            IsVolumeRange = bin.ReadBoolean(),
                            IsPitchRange = bin.ReadBoolean(),
                            IsLPFRange = bin.ReadBoolean(),
                            IsHPFRange = bin.ReadBoolean(),
                            Volume = bin.ReadDouble(),
                        };
                        if (Version < 3)
                        {
                            Event_Info.Pitch = bin.ReadInt32();
                            Event_Info.Low_Pass_Filter = bin.ReadInt32();
                            Event_Info.High_Pass_Filter = bin.ReadInt32();
                        }
                        else
                        {
                            Event_Info.Pitch = bin.ReadSByte();
                            Event_Info.Low_Pass_Filter = bin.ReadSByte();
                            Event_Info.High_Pass_Filter = bin.ReadSByte();
                        }
                        Event_Info.Volume_Range.Start = bin.ReadDouble();
                        Event_Info.Volume_Range.End = bin.ReadDouble();
                        if (Version < 3)
                        {
                            Event_Info.Pitch_Range.Start = bin.ReadInt32();
                            Event_Info.Pitch_Range.End = bin.ReadInt32();
                            Event_Info.LPF_Range.Start = bin.ReadInt32();
                            Event_Info.LPF_Range.End = bin.ReadInt32();
                            Event_Info.HPF_Range.Start = bin.ReadInt32();
                            Event_Info.HPF_Range.End = bin.ReadInt32();
                        }
                        else
                        {
                            Event_Info.Pitch_Range.Start = bin.ReadSByte();
                            Event_Info.Pitch_Range.End = bin.ReadSByte();
                            Event_Info.LPF_Range.Start = bin.ReadSByte();
                            Event_Info.LPF_Range.End = bin.ReadSByte();
                            Event_Info.HPF_Range.Start = bin.ReadSByte();
                            Event_Info.HPF_Range.End = bin.ReadSByte();
                        }
                        if (Version >= 3)
                        {
                            Event_Info.Delay = bin.ReadDouble();
                            Event_Info.Limit_Sound_Instance = bin.ReadByte();
                            Event_Info.When_Limit_Reached = bin.ReadByte();
                            Event_Info.When_Priority_Equal = bin.ReadByte();
                            if (Event_Index == 17)
                                _ = bin.ReadByte();
                        }
                        Voice_List_Full_File_Name[Type_Index][Event_Index] = Event_Info;
                    }
                    if (Version >= 3)
                        _ = bin.ReadByte();
                }
            }
            else
            {
                //初期化
                foreach (List<Voice_Event_Setting> Info_List in Voice_List_Full_File_Name)
                    foreach (Voice_Event_Setting Info in Info_List)
                        Info.Init(0, 0);
            }
            for (ushort Number = 0; Number < Sound_Count; Number++)
            {
                //イベントのタイプ(のインデックス)
                ushort List_Index;
                //サウンドのインデックス
                int Sound_Index;
                //ファイル名の長さ
                int File_Name_Count;
                //ファイル名
                string File_Name = "";
                if (Version < 3)
                {
                    List_Index = bin.ReadUInt16();
                    Sound_Index = bin.ReadInt32();
                    File_Name_Count = bin.ReadInt32();
                }
                else
                {
                    List_Index = bin.ReadByte();
                    Sound_Index = bin.ReadByte();
                    if (!IsIncludedSound)
                        File_Name = Dir_Names[bin.ReadUInt16()] + "\\";
                    File_Name_Count = bin.ReadByte();
                }
                File_Name += Encoding.UTF8.GetString(bin.ReadBytes(File_Name_Count));
                //謎の1バイト
                bin.ReadByte();
                Voice_Sound_Setting Sound_Info = new Voice_Sound_Setting();
                //WVSファイルのバージョンが2以降であればサウンド設定を取得
                if (Version >= 2)
                {
                    Sound_Info.File_Path = File_Name;
                    Sound_Info.Probability = bin.ReadDouble();
                    Sound_Info.Play_Time.Start_Time = bin.ReadDouble();
                    Sound_Info.Play_Time.End_Time = bin.ReadDouble();
                    Sound_Info.IsVolumeRange = bin.ReadBoolean();
                    Sound_Info.IsPitchRange = bin.ReadBoolean();
                    Sound_Info.IsLPFRange = bin.ReadBoolean();
                    Sound_Info.IsHPFRange = bin.ReadBoolean();
                    Sound_Info.Volume = bin.ReadDouble();
                    if (Version < 3)
                    {
                        Sound_Info.Pitch = bin.ReadInt32();
                        Sound_Info.Low_Pass_Filter = bin.ReadInt32();
                        Sound_Info.High_Pass_Filter = bin.ReadInt32();
                    }
                    else
                    {
                        Sound_Info.Pitch = bin.ReadSByte();
                        Sound_Info.Low_Pass_Filter = bin.ReadSByte();
                        Sound_Info.High_Pass_Filter = bin.ReadSByte();
                    }
                    Sound_Info.Volume_Range.Start = bin.ReadDouble();
                    Sound_Info.Volume_Range.End = bin.ReadDouble();
                    if (Version < 3)
                    {
                        Sound_Info.Pitch_Range.Start = bin.ReadInt32();
                        Sound_Info.Pitch_Range.End = bin.ReadInt32();
                        Sound_Info.LPF_Range.Start = bin.ReadInt32();
                        Sound_Info.LPF_Range.End = bin.ReadInt32();
                        Sound_Info.HPF_Range.Start = bin.ReadInt32();
                        Sound_Info.HPF_Range.End = bin.ReadInt32();
                    }
                    else
                    {
                        Sound_Info.Pitch_Range.Start = bin.ReadSByte();
                        Sound_Info.Pitch_Range.End = bin.ReadSByte();
                        Sound_Info.LPF_Range.Start = bin.ReadSByte();
                        Sound_Info.LPF_Range.End = bin.ReadSByte();
                        Sound_Info.HPF_Range.Start = bin.ReadSByte();
                        Sound_Info.HPF_Range.End = bin.ReadSByte();
                    }
                    if (Version >= 3)
                    {
                        Sound_Info.Delay = bin.ReadDouble();
                        Sound_Info.IsFadeIn = bin.ReadBoolean();
                        Sound_Info.IsFadeOut = bin.ReadBoolean();
                    }
                    if (IsIncludedSound)
                    {
                        //サウンドが開始される地点を保存しておく(Get_Sound_Bytes()でその地点のサウンドをbyte[]形式で読み取れます)
                        Sound_Info.Stream_Position = bin.BaseStream.Position;
                        int Sound_Length = bin.ReadInt32();
                        bin.BaseStream.Seek(Sound_Length, SeekOrigin.Current);
                        Sound_Info.File_Path = Path.GetFileName(File_Name);
                    }
                }
                else if (IsIncludedSound)
                {
                    //サウンドが開始される地点を保存しておく(Get_Sound_Bytes()でその地点のサウンドをbyte[]形式で読み取れます)
                    Sound_Info.Stream_Position = bin.BaseStream.Position;
                    //サウンドの長さを取得し、次のサウンドの位置までスキップ
                    int Sound_Length = bin.ReadInt32();
                    bin.BaseStream.Seek(Sound_Length, SeekOrigin.Current);
                    Sound_Info.File_Path = Path.GetFileName(File_Name);
                }
                Voice_List_Full_File_Name[List_Index][Sound_Index].Sounds.Add(Sound_Info);
            }
            Sub_Code.Set_Event_ShortID(Voice_List_Full_File_Name);
            WVS_File = From_File;
            IsLoaded = true;
            return true;
        }
        public byte[] Load_Sound(long Start_Position)
        {
            //サウンドのバイト数を取得し、その長さぶん読み取る
            bin.BaseStream.Seek(Start_Position, SeekOrigin.Begin);
            int Sound_Length = bin.ReadInt32();
            return bin.ReadBytes(Sound_Length);
        }
        public bool Sound_To_File(Voice_Sound_Setting Setting, string To_File)
        {
            if (IsLoaded && Setting.Stream_Position > 0)
            {
                try
                {
                    File.WriteAllBytes(To_File, Load_Sound(Setting.Stream_Position));
                    return true;
                }
                catch (Exception e)
                {
                    Sub_Code.Error_Log_Write(e.Message);
                }
            }
            return false;
        }
        public void Dispose()
        {
            if (bin != null)
            {
                bin.Close();
                bin = null;
            }
            WVS_File = "";
            IsLoaded = false;
        }
    }
}