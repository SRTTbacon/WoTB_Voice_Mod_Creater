using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WoTB_Voice_Mod_Creater.Class;

namespace WoTB_Voice_Mod_Creater.Wwise_Class
{
    //Wwiseのプロジェクトデータを編集
    public class Wwise_Project_Create
    {
        const string Vorbis_Quality_High_ID = "53A9DE0F-3F4F-4B59-8614-3F9E3C7358FC";
        const string Vorbis_Quality_High_WorkID = "F6B2880C-85E5-47FA-A126-645B5DFD9ACC";
        const string Master_Audio_Bus_ID = "1514A4D8-1DA6-412A-A17E-75CA0C2149F3";
        const string Master_Audio_Bus_WorkID = "005C6247-5812-4D7E-86EA-2F3C50B5E166";
        public string Project_Dir { get; private set; }
        List<string> Add_Voice_From = new List<string>();
        List<string> Add_Voice_To = new List<string>();
        List<string> Add_Voice_Type = new List<string>();
        List<string> Actor_Mixer_Hierarchy = new List<string>();
        List<string> Add_Wav_Files = new List<string>();
        List<string> Add_Other_Files = new List<string>();
        List<string> Add_All_Files = new List<string>();
        List<string> Delete_FIles = new List<string>();
        List<string> Add_WEM_Files = new List<string>();
        List<string> Delete_CAkSound_List = new List<string>();
        List<Music_Play_Time> Add_All_Files_Time = new List<Music_Play_Time>();
        int Battle_Number = 0;
        //プロジェクトファイルの内容を取得
        public Wwise_Project_Create(string Project_Dir)
        {
            try
            {
                if (!File.Exists(Project_Dir + "/Actor-Mixer Hierarchy/Default Work Unit.wwu"))
                    return;
                Actor_Mixer_Hierarchy.Clear();
                Actor_Mixer_Hierarchy.AddRange(File.ReadAllLines(Project_Dir + "/Actor-Mixer Hierarchy/Default Work Unit.wwu"));
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            this.Project_Dir = Project_Dir;
        }
        //Wwiseに入れるファイルが.wav形式でなければ.wavにエンコード
        public async Task Sound_To_WAV()
        {
            if (Add_All_Files_Time.Count == 0)
                await Multithread.Convert_To_Wav(Add_All_Files, Add_Wav_Files);
            else
                await Multithread.Convert_To_Wav(Add_All_Files, Add_Wav_Files, Add_All_Files_Time, false);
        }
        public bool Add_Sound(string Container_ShortID, string Audio_File, string Language, double Probability)
        {
            return Add_Sound(Container_ShortID, Audio_File, Language, false, null, "", 0, false, true, false, Probability);
        }
        //取得したデータから指定したイベントにサウンドを追加(Save()が呼ばれるまで保存しない)
        public bool Add_Sound(string Container_ShortID, string Audio_File, string Language, bool IsSetShortIDMode = false, Music_Play_Time Time = null, string Effect = "", int Set_Volume = 0, bool IsDeleteCAkSound = false, bool IsUseHash = true, bool IsLoop = false, double Probability = 50, uint Set_ShortID = 0)
        {
            if (Project_Dir == "")
                return false;
            try
            {
                string FileName_Short_ID;
                if (Set_ShortID != 0)
                    FileName_Short_ID = Set_ShortID.ToString();
                else if (IsUseHash)
                    FileName_Short_ID = WwiseHash.HashString(Audio_File + Container_ShortID).ToString();
                else
                    FileName_Short_ID = Path.GetFileNameWithoutExtension(Audio_File);
                if (Language == "SFX")
                {
                    if (File.Exists(Audio_File))
                    {
                        if (Time == null)
                            Add_All_Files_Time.Add(new Music_Play_Time(0, 9999));
                        else
                            Add_All_Files_Time.Add(Time);
                        if (Path.GetExtension(Audio_File) != ".wav")
                        {
                            Add_Wav_Files.Add(Project_Dir + "/Originals/SFX/" + FileName_Short_ID + ".wav");
                            Add_All_Files.Add(Audio_File);
                        }
                        else
                        {
                            File.Copy(Audio_File, Project_Dir + "/Originals/SFX/" + FileName_Short_ID + ".wav", true);
                            Add_Other_Files.Add(Project_Dir + "/Originals/SFX/" + FileName_Short_ID + ".wav");
                        }
                        Add_WEM_Files.Add(FileName_Short_ID.ToString());
                    }
                }
                else if (Language != null)
                {
                    if (File.Exists(Audio_File))
                    {
                        if (Time == null)
                            Add_All_Files_Time.Add(new Music_Play_Time(0, 9999));
                        else
                            Add_All_Files_Time.Add(Time);
                        if (Path.GetExtension(Audio_File) != ".wav")
                        {
                            Add_Wav_Files.Add(Project_Dir + "/Originals/Voices/" + Language + "/" + FileName_Short_ID + ".wav");
                            Add_All_Files.Add(Audio_File);
                        }
                        else
                        {
                            File.Copy(Audio_File, Project_Dir + "/Originals/Voices/" + Language + "/" + FileName_Short_ID + ".wav", true);
                            Add_Other_Files.Add(Project_Dir + "/Originals/Voices/" + Language + "/" + FileName_Short_ID + ".wav");
                        }
                        Add_WEM_Files.Add(FileName_Short_ID.ToString());
                    }
                }
                int ShortID_Line = 0;
                for (int Number = 0; Number < Actor_Mixer_Hierarchy.Count; Number++)
                {
                    if (Actor_Mixer_Hierarchy[Number].Contains("ShortID=\"" + Container_ShortID + "\""))
                    {
                        ShortID_Line = Number;
                        break;
                    }
                }
                if (ShortID_Line == 0)
                    return false;
                if (IsDeleteCAkSound && !Delete_CAkSound_List.Contains(Container_ShortID))
                    Delete_CAkSounds(Container_ShortID);
                int ChildrenList_Line = 0;
                int ReferenceListEnd_Line = 0;
                for (int Number = ShortID_Line; Number < Actor_Mixer_Hierarchy.Count; Number++)
                {
                    if (Actor_Mixer_Hierarchy[Number].Contains("<ChildrenList>"))
                    {
                        ChildrenList_Line = Number;
                        break;
                    }
                    if (Actor_Mixer_Hierarchy[Number].Contains("</ReferenceList>"))
                        ReferenceListEnd_Line = Number;
                    if (Actor_Mixer_Hierarchy[Number].Contains("</RandomSequenceContainer>"))
                        break;
                }
                if (!Delete_CAkSound_List.Contains(Container_ShortID))
                    Delete_CAkSound_List.Add(Container_ShortID);
                if (ChildrenList_Line == 0 && ReferenceListEnd_Line == 0)
                    return false;
                else if (ChildrenList_Line == 0)
                {
                    More_Class.List_Init(ReferenceListEnd_Line + 1);
                    More_Class.List_Add(Actor_Mixer_Hierarchy, "<ChildrenList>");
                    More_Class.List_Add(Actor_Mixer_Hierarchy, "</ChildrenList>");
                    if (IsSetShortIDMode)
                        return List_Add_File(ReferenceListEnd_Line + 2, FileName_Short_ID + ".wav", Language, uint.Parse(FileName_Short_ID), Effect, Set_Volume, IsLoop, Probability);
                    return List_Add_File(ReferenceListEnd_Line + 2, FileName_Short_ID + ".wav", Language, 0, Effect, Set_Volume, IsLoop, Probability);
                }
                else
                {
                    if (IsSetShortIDMode)
                        return List_Add_File(ChildrenList_Line + 1, FileName_Short_ID + ".wav", Language, uint.Parse(FileName_Short_ID), Effect, Set_Volume, IsLoop, Probability);
                    return List_Add_File(ChildrenList_Line + 1, FileName_Short_ID + ".wav", Language, 0, Effect, Set_Volume, IsLoop, Probability);
                }
            }
            catch
            {
                return false;
            }
        }
        public bool Add_Sound(SE_Info_Parent Info, WVS_Load WVS_File)
        {
            Voice_Event_Setting Setting = new Voice_Event_Setting(0, Info.SE_ShortID);
            Voice_Sound_Setting Sound = new Voice_Sound_Setting(Info.SE_Path);
            Sound.Stream_Position = Info.Stream_Position;
            Setting.Sounds.Add(Sound);
            return Add_Sound(Setting, WVS_File, false, true, "Japanese");
        }
        //音声Mod作成の際の処理です。V1.5.0にて大幅な仕様変更があったためそれに合わせて関数を分けています。
        public bool Add_Sound(Voice_Event_Setting Event_Setting, WVS_Load WVS_File, bool IsWoTToBlitzMode = false, bool IsIgnoreParent = false, string Language = "ja")
        {
            try
            {
                if (!IsIgnoreParent)
                {
                    int ShortID_Line = Get_Line_By_ShortID(Event_Setting.Event_ShortID);
                    if (ShortID_Line == -1)
                        return false;
                    bool Temp2 = Delete_Property(Event_Setting.Event_ShortID);
                    if (!Temp2)
                    {
                        More_Class.List_Init(ShortID_Line + 1);
                        More_Class.List_Add(Actor_Mixer_Hierarchy, "<PropertyList>");
                        More_Class.List_Add(Actor_Mixer_Hierarchy, "</PropertyList>");
                    }
                    int ProLine = 0;
                    for (int Number = ShortID_Line; Number < Actor_Mixer_Hierarchy.Count; Number++)
                    {
                        if (Actor_Mixer_Hierarchy[Number].Contains("<PropertyList>"))
                        {
                            ProLine = Number;
                            break;
                        }
                    }
                    if (ProLine == 0)
                        return false;
                    More_Class.List_Init(ProLine + 1);
                    Add_Event_Settings(Event_Setting);
                }
                int ChildrenList_Line = 0;
                int ReferenceListEnd_Line = 0;
                int Voice_ShortID_Line = Get_Line_By_ShortID(Event_Setting.Voice_ShortID);
                if (Voice_ShortID_Line == -1)
                    return false;
                for (int Number = Voice_ShortID_Line; Number < Actor_Mixer_Hierarchy.Count; Number++)
                {
                    if (Actor_Mixer_Hierarchy[Number].Contains("<ChildrenList>"))
                    {
                        ChildrenList_Line = Number;
                        break;
                    }
                    if (Actor_Mixer_Hierarchy[Number].Contains("</ReferenceList>"))
                        ReferenceListEnd_Line = Number;
                    if (Actor_Mixer_Hierarchy[Number].Contains("</RandomSequenceContainer>"))
                        break;
                }
                if (ChildrenList_Line == 0 && ReferenceListEnd_Line == 0)
                    return false;
                int Start_Line = ChildrenList_Line + 1;
                if (ChildrenList_Line == 0)
                {
                    More_Class.List_Init(ReferenceListEnd_Line + 1);
                    More_Class.List_Add(Actor_Mixer_Hierarchy, "<ChildrenList>");
                    More_Class.List_Add(Actor_Mixer_Hierarchy, "</ChildrenList>");
                    Start_Line = ReferenceListEnd_Line + 2;
                }
                bool IsOK = true;
                foreach (Voice_Sound_Setting Setting in Event_Setting.Sounds)
                {
                    string Random_ID = Get_Random_ShortID();
                    if (Setting.File_Path.Contains("\\"))
                    {
                        Add_Voice_From.Add(Setting.File_Path.Replace("/", "\\"));
                        Add_Voice_To.Add(Random_ID);
                        Add_Voice_Type.Add("Voices\\" + Language);
                    }
                    else if (IsWoTToBlitzMode)
                        File.Copy(Voice_Set.Special_Path + "\\Wwise\\No_Sound.wav", Project_Dir + "\\Originals\\Voices\\" + Language + "\\" + Setting.File_Path + ".wav", true);
                    else
                    {
                        string Ex = Path.GetExtension(Setting.File_Path);
                        File.WriteAllBytes(Project_Dir + "/Originals/Voices/" + Language + "/Temp_" + Random_ID + Ex, WVS_File.Load_Sound(Setting.Stream_Position));
                        Add_Voice_From.Add(Project_Dir + "\\Originals\\Voices\\" + Language + "\\Temp_" + Random_ID + Ex);
                        Add_Voice_To.Add(Random_ID);
                        Add_Voice_Type.Add("Voices\\" + Language);
                    }
                    if (!IsWoTToBlitzMode)
                        Add_Wav_Files.Add(Project_Dir + "\\Originals\\Voices\\" + Language + "\\" + Random_ID + ".wav");
                    Setting.Delay += Event_Setting.Delay;
                    bool Temp = IsWoTToBlitzMode ? List_Add_File(Start_Line, Setting.File_Path + ".wav", Language, uint.Parse(Setting.File_Path), "", 0, false, 50, Setting) : List_Add_File(Start_Line, Random_ID + ".wav", Language, 0, "", 0, false, 50, Setting);
                    Setting.Delay -= Event_Setting.Delay;
                    if (!Temp)
                        IsOK = false;
                }
                return IsOK;
            }
            catch
            {
                return false;
            }
        }
        public bool Add_Sound(uint Container_ID, string SE_File, double Volume = 0, bool IsLoop = false)
        {
            int ChildrenList_Line = 0;
            int ReferenceListEnd_Line = 0;
            int Voice_ShortID_Line = Get_Line_By_ShortID(Container_ID);
            if (Voice_ShortID_Line == -1)
                return false;
            for (int Number = Voice_ShortID_Line; Number < Actor_Mixer_Hierarchy.Count; Number++)
            {
                if (Actor_Mixer_Hierarchy[Number].Contains("<ChildrenList>"))
                {
                    ChildrenList_Line = Number;
                    break;
                }
                if (Actor_Mixer_Hierarchy[Number].Contains("</ReferenceList>"))
                    ReferenceListEnd_Line = Number;
                if (Actor_Mixer_Hierarchy[Number].Contains("</RandomSequenceContainer>"))
                    break;
            }
            if (ChildrenList_Line == 0 && ReferenceListEnd_Line == 0)
                return false;
            int Start_Line = ChildrenList_Line + 1;
            if (ChildrenList_Line == 0)
            {
                More_Class.List_Init(ReferenceListEnd_Line + 1);
                More_Class.List_Add(Actor_Mixer_Hierarchy, "<ChildrenList>");
                More_Class.List_Add(Actor_Mixer_Hierarchy, "</ChildrenList>");
                Start_Line = ReferenceListEnd_Line + 2;
            }
            string Random_ID = Get_Random_ShortID();
            Add_Voice_From.Add(SE_File.Replace("/", "\\"));
            Add_Voice_To.Add(Random_ID);
            Add_Voice_Type.Add("SFX");
            Add_Wav_Files.Add(Project_Dir + "\\Originals\\SFX\\" + Random_ID + ".wav");
            return List_Add_File(Start_Line, Random_ID + ".wav", "SFX", 0, "", Volume, IsLoop, 50);
        }
        //ランダムな数字を生成(天文学的確率ですが、仮に数字が被った場合は再度生成)
        private string Get_Random_ShortID()
        {
            string Random_ID = More_Class.CreateShortID();
            if (Add_Voice_To.Contains(Random_ID))
                return Get_Random_ShortID();
            return Random_ID;
        }
        private int Get_Line_By_ShortID(uint ShortID)
        {
            return Get_Line_By_ShortID(ShortID.ToString());
        }
        //指定したShortIDが存在する行を取得
        private int Get_Line_By_ShortID(string ShortID)
        {
            int ShortID_Line = 0;
            for (int Number = 0; Number < Actor_Mixer_Hierarchy.Count; Number++)
            {
                if (Actor_Mixer_Hierarchy[Number].Contains("ShortID=\"" + ShortID + "\""))
                {
                    ShortID_Line = Number;
                    break;
                }
            }
            if (ShortID_Line == 0)
                return -1;
            return ShortID_Line;
        }
        //指定したShortIDのコンテナ内のプロパティをすべて削除
        private bool Delete_Property(uint ShortID)
        {
            int ShortID_Line = 0;
            for (int Number = 0; Number < Actor_Mixer_Hierarchy.Count; Number++)
            {
                if (Actor_Mixer_Hierarchy[Number].Contains("ShortID=\"" + ShortID + "\""))
                {
                    ShortID_Line = Number;
                    break;
                }
            }
            if (ShortID_Line == 0)
                return false;
            bool IsExistPropertyList = false;
            bool IsKeepedProperty = false;
            int Index = -1;
            for (int Number = ShortID_Line + 1; Number < Actor_Mixer_Hierarchy.Count; Number++)
            {
                if (Actor_Mixer_Hierarchy[Number].Contains("<ChildrenList>") || Actor_Mixer_Hierarchy[Number].Contains("<ReferenceList>"))
                    break;
                if (Actor_Mixer_Hierarchy[Number].Contains("</PropertyList>"))
                {
                    /*if (IsDeletePropertyList)
                        Actor_Mixer_Hierarchy.RemoveAt(Number);*/
                    break;
                }
                if (Actor_Mixer_Hierarchy[Number].Contains("<PropertyList>"))
                {
                    IsExistPropertyList = true;
                    IsKeepedProperty = true;
                    Index = Number;
                }
                else if (Actor_Mixer_Hierarchy[Number].Contains("NormalOrShuffle") || Actor_Mixer_Hierarchy[Number].Contains("OverrideOutput"))
                    IsKeepedProperty = true;
                else if (IsExistPropertyList)
                {
                    Actor_Mixer_Hierarchy.RemoveAt(Number);
                    Number--;
                }
            }
            /*if (!IsDeletePropertyList)
            {
                More_Class.List_Init(Index);
                More_Class.List_Add(Actor_Mixer_Hierarchy, "<PropertyList>");
            }*/
            return IsKeepedProperty;
        }
        //追加したサウンドをファイルに保存する
        public bool Save()
        {
            if (Project_Dir == "")
                return false;
            try
            {
                File.WriteAllLines(Project_Dir + "/Actor-Mixer Hierarchy/Default Work Unit.wwu", Actor_Mixer_Hierarchy);
                return true;
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                return false;
            }
        }
        //保存されたデータをもとに.bnkファイルをビルド(内容によって時間がかかります)
        public void Project_Build(string BankName, string OutputFilePath, string GeneratedSoundBanksPath = null, bool IsUseCache = false, string Language = "ja")
        {
            if (Project_Dir == "" || !Directory.Exists(Path.GetDirectoryName(OutputFilePath)))
                return;
            try
            {
                BankName = BankName.Replace(" ", "_");
                string[] Files = Directory.GetFiles(Project_Dir, "*.wproj", SearchOption.TopDirectoryOnly);
                if (Files.Length == 0)
                    throw new Exception("エラー:プロジェクトファイル(*.wproj)が見つかりませんでした。");
                string Project_File = Files[0];
                Sub_Code.Wwise_Repair_Project(Project_Dir);
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Wwise/Project_Build.bat");
                stw.WriteLine("chcp 65001");
                stw.Write("\"" + Voice_Set.Special_Path + "/Wwise/x64/Release/bin/WwiseCLI.exe\" \"" + Project_File + "\" -GenerateSoundBanks -Language " + Language + " -Platform Windows ");
                if (IsUseCache)
                    stw.Write("-Bank " + BankName + " --no-wwise-dat");
                else
                    stw.Write("-Bank " + BankName + " -ClearAudioFileCache");
                stw.Close();
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = Voice_Set.Special_Path + "/Wwise/Project_Build.bat",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process p = Process.Start(processStartInfo);
                p.WaitForExit();
                File.Delete(Voice_Set.Special_Path + "/Wwise/Project_Build.bat");
                string GeneratedFile;
                if (GeneratedSoundBanksPath == null)
                {
                    string[] Files_BNK = Directory.GetFiles(Project_Dir + "/GeneratedSoundBanks/Windows", BankName + ".bnk", SearchOption.AllDirectories);
                    if (Files_BNK.Length == 0)
                        throw new Exception("エラー:プロジェクトをビルドできませんでした。プロジェクトファイルが破損している可能性があります。");
                    GeneratedFile = Files_BNK[0];
                }
                else
                {
                    string[] Files_BNK = Directory.GetFiles(Project_Dir + "/GeneratedSoundBanks/" + GeneratedSoundBanksPath, BankName + ".bnk", SearchOption.AllDirectories);
                    if (Files_BNK.Length == 0)
                        throw new Exception("エラー:プロジェクトをビルドできませんでした。プロジェクトファイルが破損している可能性があります。");
                    GeneratedFile = Files_BNK[0];
                }
                Sub_Code.File_Move(GeneratedFile, OutputFilePath, true);
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        //使わなくなったら必ず実行させる
        //一時ファイルを削除
        public void Clear(bool IsDeleteCache = true, string CachePath = null, bool IsOnlyAddFile = false)
        {
            try
            {
                foreach (string File_Now in Add_Wav_Files)
                {
                    Sub_Code.File_Delete_V2(File_Now);
                    Sub_Code.File_Delete_V2(File_Now.Replace(".wav", ".akd"));
                }
                foreach (string File_Now in Add_Other_Files)
                {
                    Sub_Code.File_Delete_V2(File_Now);
                    Sub_Code.File_Delete_V2(File_Now.Replace(".wav", ".akd"));
                }
                foreach (string File_Now in Delete_FIles)
                    Sub_Code.File_Delete_V2(File_Now);
                if (IsDeleteCache)
                {
                    string[] GetWEMFiles = { };
                    if (CachePath == null)
                        GetWEMFiles = Directory.GetFiles(Project_Dir + "/.cache/Windows", "*.wem", SearchOption.AllDirectories);
                    else
                        GetWEMFiles = Directory.GetFiles(Project_Dir + "/.cache/" + CachePath, "*.wem", SearchOption.AllDirectories);
                    if (IsOnlyAddFile)
                    {
                        List<string> Lists = new List<string>();
                        foreach (string File_Now in Add_WEM_Files)
                        {
                            foreach (string File_WEM in GetWEMFiles)
                            {
                                string GetName = Path.GetFileNameWithoutExtension(File_WEM);
                                if (GetName.Contains(File_Now + "_"))
                                {
                                    Lists.Add(File_WEM);
                                    break;
                                }
                            }
                        }
                        foreach (string File_Now in Lists)
                            Sub_Code.File_Delete_V2(File_Now);
                    }
                    else
                    {
                        foreach (string File_Now in GetWEMFiles)
                            Sub_Code.File_Delete_V2(File_Now);
                    }
                }
                Actor_Mixer_Hierarchy.Clear();
                Add_Wav_Files.Clear();
                Add_All_Files.Clear();
                Add_All_Files_Time.Clear();
                Delete_FIles.Clear();
                Add_WEM_Files.Clear();
                Add_Other_Files.Clear();
                Delete_CAkSound_List.Clear();
                Project_Dir = "";
                Battle_Number = 0;
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        public static void Delete_Cache(string Project_Dir)
        {
            string[] Temp = Directory.GetFiles(Project_Dir + "/.cache", "*.wem", SearchOption.AllDirectories);
            foreach (string File_Now in Temp)
                Sub_Code.File_Delete_V2(File_Now);
        }
        //以下の文字を指定した行に追加
        //GUIDやSourceIDはどんな数でも問題ないっぽいのでランダムに作成させる
        bool List_Add_File(int Line_Number, string FileName, string Language, uint Set_Media_ID = 0, string Effect = "", double Set_Volume = 0, bool IsLoop = false, double Probability = 50, Voice_Sound_Setting Setting = null)
        {
            try
            {
                FileName = FileName.Replace(".wav", "");
                string Sound_GUID = Guid.NewGuid().ToString().ToUpper();
                string AudioFileSource_GUID = Guid.NewGuid().ToString().ToUpper();
                string Replace_Name = FileName.Replace(".", "_");
                uint Sound_ID = WwiseHash.HashGUID(Sound_GUID);
                uint AudioFileSource_ID = WwiseHash.HashGUID(AudioFileSource_GUID);
                if (Set_Media_ID != 0)
                    AudioFileSource_ID = Set_Media_ID;
                //BGMはあとから変更できるように500000000から始まるように設定
                if (FileName.Contains("battle_bgm_"))
                {
                    AudioFileSource_ID = (uint)(500000000 + Battle_Number);
                    Battle_Number++;
                }
                More_Class.List_Init(Line_Number);
                More_Class.List_Add(Actor_Mixer_Hierarchy, "<Sound Name=\"" + Replace_Name + "\" ID=\"{" + Sound_GUID + "}\" ShortID=\"" + Sound_ID + "\">");
                More_Class.List_Add(Actor_Mixer_Hierarchy, "<PropertyList>");
                if (IsLoop)
                    More_Class.List_Add(Actor_Mixer_Hierarchy, "<Property Name=\"IsLoopingEnabled\" Type=\"bool\" Value=\"True\"/>");
                if (Language != "SFX")
                    More_Class.List_Add(Actor_Mixer_Hierarchy, "<Property Name=\"IsVoice\" Type=\"bool\" Value=\"True\"/>");
                if (Setting == null)
                {
                    if (Probability != 50)
                    {
                        More_Class.List_Add(Actor_Mixer_Hierarchy, "<Property Name=\"Weight\" Type=\"Real64\">\"");
                        More_Class.List_Add(Actor_Mixer_Hierarchy, "<ValueList>");
                        More_Class.List_Add(Actor_Mixer_Hierarchy, "<Value>" + Probability + "</Value>");
                        More_Class.List_Add(Actor_Mixer_Hierarchy, "</ValueList>");
                        More_Class.List_Add(Actor_Mixer_Hierarchy, "</Property>");
                    }
                    if (Set_Volume != 0)
                    {
                        More_Class.List_Add(Actor_Mixer_Hierarchy, "<Property Name=\"Volume\" Type=\"Real64\">");
                        More_Class.List_Add(Actor_Mixer_Hierarchy, "<ValueList>");
                        More_Class.List_Add(Actor_Mixer_Hierarchy, "<Value>" + Set_Volume + "</Value>");
                        More_Class.List_Add(Actor_Mixer_Hierarchy, "</ValueList>");
                        More_Class.List_Add(Actor_Mixer_Hierarchy, "</Property>");
                    }
                }
                else
                    Add_Sound_Settings(Setting);
                More_Class.List_Add(Actor_Mixer_Hierarchy, "</PropertyList>");
                More_Class.List_Add(Actor_Mixer_Hierarchy, "<ReferenceList>");
                More_Class.List_Add(Actor_Mixer_Hierarchy, "<Reference Name=\"Conversion\">");
                More_Class.List_Add(Actor_Mixer_Hierarchy, "<ObjectRef Name=\"Vorbis Quality High\" ID=\"{" + Vorbis_Quality_High_ID + "}\" WorkUnitID=\"{" + Vorbis_Quality_High_WorkID + "}\"/>");
                More_Class.List_Add(Actor_Mixer_Hierarchy, "</Reference>");
                More_Class.List_Add(Actor_Mixer_Hierarchy, "<Reference Name=\"OutputBus\">");
                More_Class.List_Add(Actor_Mixer_Hierarchy, "<ObjectRef Name=\"Master Audio Bus\" ID=\"{" + Master_Audio_Bus_ID + "}\" WorkUnitID=\"{" + Master_Audio_Bus_WorkID + "}\"/>");
                More_Class.List_Add(Actor_Mixer_Hierarchy, "</Reference>");
                More_Class.List_Add(Actor_Mixer_Hierarchy, "</ReferenceList>");
                More_Class.List_Add(Actor_Mixer_Hierarchy, "<ChildrenList>");
                More_Class.List_Add(Actor_Mixer_Hierarchy, "<AudioFileSource Name=\"" + Replace_Name + "\" ID=\"{" + AudioFileSource_GUID + "}\">");
                if (Effect != "" || Setting != null)
                {
                    More_Class.List_Add(Actor_Mixer_Hierarchy, "<PropertyList>");
                    if (Setting != null)
                    {
                        if (Setting.Play_Time.Max_Time != 0)
                        {
                            double Play_Time;
                            if (Setting.Play_Time.Start_Time != 0)
                                More_Class.List_Add(Actor_Mixer_Hierarchy, "<Property Name=\"TrimBegin\" Type=\"Real64\" Value=\"" + Setting.Play_Time.Start_Time + "\"/>");
                            if (Setting.Play_Time.End_Time != 0)
                            {
                                More_Class.List_Add(Actor_Mixer_Hierarchy, "<Property Name=\"TrimEnd\" Type=\"Real64\" Value=\"" + Setting.Play_Time.End_Time + "\"/>");
                                Play_Time = Setting.Play_Time.End_Time - Setting.Play_Time.Start_Time;
                            }
                            else
                                Play_Time = Setting.Play_Time.Max_Time - Setting.Play_Time.Start_Time;
                            if (Play_Time > 0.5)
                            {
                                if (Setting.IsFadeIn)
                                    More_Class.List_Add(Actor_Mixer_Hierarchy, "<Property Name=\"FadeInDuration\" Type=\"Real64\" Value=\"0.5\"/>");
                                if (Setting.IsFadeOut)
                                    More_Class.List_Add(Actor_Mixer_Hierarchy, "<Property Name=\"FadeOutDuration\" Type=\"Real64\" Value=\"0.5\"/>");
                            }
                        }
                    }
                    else if (Effect == "Feed_In")
                    {
                        More_Class.List_Add(Actor_Mixer_Hierarchy, "<Property Name=\"FadeInDuration\" Type=\"Real64\" Value=\"1.5\"/>");
                        More_Class.List_Add(Actor_Mixer_Hierarchy, "<Property Name=\"FadeOutDuration\" Type=\"Real64\" Value=\"1.5\"/>");
                    }
                    More_Class.List_Add(Actor_Mixer_Hierarchy, "</PropertyList>");
                }
                More_Class.List_Add(Actor_Mixer_Hierarchy, "<Language>" + Language + "</Language>");
                More_Class.List_Add(Actor_Mixer_Hierarchy, "<AudioFile>" + FileName + ".wav</AudioFile>");
                More_Class.List_Add(Actor_Mixer_Hierarchy, "<MediaIDList>");
                More_Class.List_Add(Actor_Mixer_Hierarchy, "<MediaID ID=\"" + AudioFileSource_ID + "\"/>");
                More_Class.List_Add(Actor_Mixer_Hierarchy, "</MediaIDList>");
                More_Class.List_Add(Actor_Mixer_Hierarchy, "</AudioFileSource>");
                More_Class.List_Add(Actor_Mixer_Hierarchy, "</ChildrenList>");
                More_Class.List_Add(Actor_Mixer_Hierarchy, "<ActiveSourceList>");
                More_Class.List_Add(Actor_Mixer_Hierarchy, "<ActiveSource Name=\"" + Replace_Name + "\" ID=\"{" + AudioFileSource_GUID + "}\" Platform=\"Linked\"/>");
                More_Class.List_Add(Actor_Mixer_Hierarchy, "</ActiveSourceList>");
                More_Class.List_Add(Actor_Mixer_Hierarchy, "</Sound>");
                return true;
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                return false;
            }
        }
        //イベントの親コンテナの設定を反映
        private void Add_Event_Settings(Voice_Event_Setting Setting)
        {
            More_Class.List_Add(Actor_Mixer_Hierarchy, "<Property Name=\"Highpass\" Type=\"int16\">");
            if (Setting.IsHPFRange)
                Add_Effect_Text(Setting.HPF_Range.Start, Setting.HPF_Range.End);
            else
                Add_Effect_Text(Setting.High_Pass_Filter);
            More_Class.List_Add(Actor_Mixer_Hierarchy, "</Property>");
            if (Setting.Limit_Sound_Instance != 50 || Setting.When_Limit_Reached != 0 || Setting.When_Priority_Equal != 0)
                More_Class.List_Add(Actor_Mixer_Hierarchy, "<Property Name=\"IgnoreParentMaxSoundInstance\" Type=\"bool\" Value=\"True\"/>");
            More_Class.List_Add(Actor_Mixer_Hierarchy, "<Property Name=\"Lowpass\" Type=\"int16\">");
            if (Setting.IsLPFRange)
                Add_Effect_Text(Setting.LPF_Range.Start, Setting.LPF_Range.End);
            else
                Add_Effect_Text(Setting.Low_Pass_Filter);
            More_Class.List_Add(Actor_Mixer_Hierarchy, "</Property>");
            if (Setting.When_Priority_Equal != 0)
                More_Class.List_Add(Actor_Mixer_Hierarchy, "<Property Name=\"MaxReachedBehavior\" Type=\"int16\" Value=\"" + Setting.When_Priority_Equal + "\"/>");
            if (Setting.Limit_Sound_Instance != 50)
            {
                More_Class.List_Add(Actor_Mixer_Hierarchy, "<Property Name=\"MaxSoundPerInstance\" Type=\"int16\">");
                Add_Effect_Text(Setting.Limit_Sound_Instance);
                More_Class.List_Add(Actor_Mixer_Hierarchy, "</Property>");
            }
            if (Setting.When_Limit_Reached != 0)
                More_Class.List_Add(Actor_Mixer_Hierarchy, "<Property Name=\"OverLimitBehavior\" Type=\"int16\" Value=\"" + Setting.When_Limit_Reached + "\"/>");
            More_Class.List_Add(Actor_Mixer_Hierarchy, "<Property Name=\"Pitch\" Type=\"int32\">");
            if (Setting.IsPitchRange)
                Add_Effect_Text(Setting.Pitch_Range.Start, Setting.Pitch_Range.End);
            else
                Add_Effect_Text(Setting.Pitch);
            More_Class.List_Add(Actor_Mixer_Hierarchy, "</Property>");
            if (Setting.Limit_Sound_Instance != 50 || Setting.When_Limit_Reached != 0 || Setting.When_Priority_Equal != 0)
            {
                More_Class.List_Add(Actor_Mixer_Hierarchy, "<Property Name=\"UseMaxSoundPerInstance\" Type=\"bool\">");
                Add_Effect_Text(true);
                More_Class.List_Add(Actor_Mixer_Hierarchy, "</Property>");
            }
            More_Class.List_Add(Actor_Mixer_Hierarchy, "<Property Name=\"Volume\" Type=\"Real64\">");
            if (Setting.IsVolumeRange)
                Add_Effect_Text(Setting.Volume_Range.Start, Setting.Volume_Range.End);
            else
                Add_Effect_Text(Setting.Volume);
            More_Class.List_Add(Actor_Mixer_Hierarchy, "</Property>");
        }
        //イベントの音声コンテナの設定を反映
        private void Add_Sound_Settings(Voice_Sound_Setting Setting)
        {
            More_Class.List_Add(Actor_Mixer_Hierarchy, "<Property Name=\"Highpass\" Type=\"int16\">");
            if (Setting.IsHPFRange)
                Add_Effect_Text(Setting.HPF_Range.Start, Setting.HPF_Range.End);
            else
                Add_Effect_Text(Setting.High_Pass_Filter);
            More_Class.List_Add(Actor_Mixer_Hierarchy, "</Property>");
            if (Setting.Delay > 0)
            {
                More_Class.List_Add(Actor_Mixer_Hierarchy, "<Property Name=\"InitialDelay\" Type=\"Real64\">");
                Add_Effect_Text(Setting.Delay);
                More_Class.List_Add(Actor_Mixer_Hierarchy, "</Property>");
            }
            More_Class.List_Add(Actor_Mixer_Hierarchy, "<Property Name=\"Lowpass\" Type=\"int16\">");
            if (Setting.IsLPFRange)
                Add_Effect_Text(Setting.LPF_Range.Start, Setting.LPF_Range.End);
            else
                Add_Effect_Text(Setting.Low_Pass_Filter);
            More_Class.List_Add(Actor_Mixer_Hierarchy, "</Property>");
            More_Class.List_Add(Actor_Mixer_Hierarchy, "<Property Name=\"Pitch\" Type=\"int32\">");
            if (Setting.IsPitchRange)
                Add_Effect_Text(Setting.Pitch_Range.Start, Setting.Pitch_Range.End);
            else
                Add_Effect_Text(Setting.Pitch);
            More_Class.List_Add(Actor_Mixer_Hierarchy, "</Property>");
            More_Class.List_Add(Actor_Mixer_Hierarchy, "<Property Name=\"Volume\" Type=\"Real64\">");
            if (Setting.IsVolumeRange)
                Add_Effect_Text(Setting.Volume_Range.Start, Setting.Volume_Range.End);
            else
                Add_Effect_Text(Setting.Volume);
            More_Class.List_Add(Actor_Mixer_Hierarchy, "</Property>");
            if (Setting.Probability != 50)
            {
                More_Class.List_Add(Actor_Mixer_Hierarchy, "<Property Name=\"Weight\" Type=\"Real64\">");
                Add_Effect_Text(Setting.Probability);
                More_Class.List_Add(Actor_Mixer_Hierarchy, "</Property>");
            }
        }
        //上2つの関数のお手伝い
        private void Add_Effect_Text(double Min, double Max)
        {
            More_Class.List_Add(Actor_Mixer_Hierarchy, "<ModifierList>");
            More_Class.List_Add(Actor_Mixer_Hierarchy, "<ModifierInfo>");
            More_Class.List_Add(Actor_Mixer_Hierarchy, "<Modifier Name=\"\" ID=\"{" + Guid.NewGuid().ToString().ToUpper() + "}\">");
            More_Class.List_Add(Actor_Mixer_Hierarchy, "<PropertyList>");
            More_Class.List_Add(Actor_Mixer_Hierarchy, "<Property Name=\"Enabled\" Type=\"bool\" Value=\"True\"/>");
            More_Class.List_Add(Actor_Mixer_Hierarchy, "<Property Name=\"Max\" Type=\"Real64\" Value=\"" + Max + "\"/>");
            More_Class.List_Add(Actor_Mixer_Hierarchy, "<Property Name=\"Min\" Type=\"Real64\" Value=\"" + Min + "\"/>");
            More_Class.List_Add(Actor_Mixer_Hierarchy, "</PropertyList>");
            More_Class.List_Add(Actor_Mixer_Hierarchy, "</Modifier>");
            More_Class.List_Add(Actor_Mixer_Hierarchy, "</ModifierInfo>");
            More_Class.List_Add(Actor_Mixer_Hierarchy, "</ModifierList>");
        }
        //同上
        private void Add_Effect_Text(double Value)
        {
            Add_Effect_Text(Value.ToString());
        }
        private void Add_Effect_Text(bool Value)
        {
            Add_Effect_Text(Value.ToString());
        }
        private void Add_Effect_Text(string Value)
        {
            More_Class.List_Add(Actor_Mixer_Hierarchy, "<ValueList>");
            More_Class.List_Add(Actor_Mixer_Hierarchy, "<Value>" + Value + "</Value>");
            More_Class.List_Add(Actor_Mixer_Hierarchy, "</ValueList>");
        }
        //プロジェクトに戦闘BGMを追加
        public void Sound_Music_Add_Wwise(string Dir_Name)
        {
            string[] Voice_Files_01 = Directory.GetFiles(Dir_Name, "*.wav", SearchOption.TopDirectoryOnly);
            foreach (string Voice_Now in Voice_Files_01)
            {
                string Name_Only;
                if (Path.GetFileNameWithoutExtension(Voice_Now).Contains("reload"))
                    Name_Only = Voice_Mod_Create.Get_Voice_Type_V2(Voice_Now);
                else
                    Name_Only = Voice_Mod_Create.Get_Voice_Type_V1(Voice_Now);
                switch (Name_Only)
                {
                    case "battle_bgm":
                        Add_Sound("649358221", Voice_Now, "ja");
                        break;
                }
            }
        }
        //指定したShortIDのコンテナ内のサウンド(CAkSound)を削除
        public void Delete_CAkSounds(uint Container_ShortID)
        {
            Delete_CAkSounds(Container_ShortID.ToString());
        }
        public void Delete_CAkSounds(string Container_ShortID)
        {
            int ShortID_Line = 0;
            for (int Number = 0; Number < Actor_Mixer_Hierarchy.Count; Number++)
            {
                if (Actor_Mixer_Hierarchy[Number].Contains("ShortID=\"" + Container_ShortID + "\""))
                {
                    ShortID_Line = Number;
                    break;
                }
            }
            if (ShortID_Line == 0)
                return;
            for (int Number = ShortID_Line; Number < Actor_Mixer_Hierarchy.Count; Number++)
            {
                if (Actor_Mixer_Hierarchy[Number].Contains("<Sound Name=\""))
                {
                    bool IsEnd = false;
                    while (true)
                    {
                        if (Actor_Mixer_Hierarchy[Number].Contains("</Sound>"))
                            IsEnd = true;
                        Actor_Mixer_Hierarchy.RemoveAt(Number);
                        if (IsEnd)
                        {
                            Number--;
                            break;
                        }
                    }
                    Number = ShortID_Line;
                }
                if (Actor_Mixer_Hierarchy[Number].Contains("</RandomSequenceContainer>"))
                    break;
            }
        }
        //ロードBGMをプロジェクトに追加
        public void Loading_Music_Add_Wwise(string Music_File, int Music_Index, Music_Play_Time Time, bool IsFeed_In_Mode, int Set_Volume, int Page = 0)
        {
            string Mode = "";
            Music_Play_Time Time_Set = null;
            if (Time != null)
            {
                Time_Set = new Music_Play_Time(Time.Start_Time, Time.End_Time);
                if (Time.End_Time == 0)
                    Time_Set.End_Time = 9999;
            }
            if (IsFeed_In_Mode)
                Mode = "Feed_In";
            if (Music_Index == 12 || Music_Index == 14 || Music_Index == 16)
            {
                string From_File = Music_File;
                Music_File = Voice_Set.Special_Path + "\\Encode_Mp3\\Temp_" + Sub_Code.Generate_Random_String(2, 6) + ".wav";
                Delete_FIles.Add(Music_File);
                Sub_Code.Audio_Encode_To_Other(From_File, Music_File, "wav", false);
                Sub_Code.Volume_Set_Start(Music_File, Encode_Mode.WAV);
            }
            if (Page == 0)
            {
                if (Music_Index == 0)
                    Add_Sound("205170598", Music_File, "ja", true, Time_Set, Mode, Set_Volume);
                else if (Music_Index == 1)
                    Add_Sound("148841988", Music_File, "ja", true, Time_Set, Mode, Set_Volume);
                else if (Music_Index == 2)
                    Add_Sound("1067185674", Music_File, "ja", true, Time_Set, Mode, Set_Volume);
                else if (Music_Index == 3)
                    Add_Sound("99202684", Music_File, "ja", true, Time_Set, Mode, Set_Volume);
                else if (Music_Index == 4)
                    Add_Sound("493356780", Music_File, "ja", true, Time_Set, Mode, Set_Volume);
                else if (Music_Index == 5)
                    Add_Sound("277287194", Music_File, "ja", true, Time_Set, Mode, Set_Volume);
                else if (Music_Index == 6)
                    Add_Sound("321403539", Music_File, "ja", true, Time_Set, Mode, Set_Volume);
                else if (Music_Index == 7)
                    Add_Sound("603412881", Music_File, "ja", true, Time_Set, Mode, Set_Volume);
                else if (Music_Index == 8)
                    Add_Sound("256533957", Music_File, "ja", true, Time_Set, Mode, Set_Volume);
                else if (Music_Index == 9)
                    Add_Sound("520751345", Music_File, "ja", true, Time_Set, Mode, Set_Volume);
                else if (Music_Index == 10)
                    Add_Sound("307041675", Music_File, "ja", true, Time_Set, Mode, Set_Volume);
                else if (Music_Index == 11)
                    Add_Sound("960016609", Music_File, "ja", true, Time_Set, Mode, Set_Volume);
                else if (Music_Index == 12)
                    Add_Sound("737229060", Music_File, "ja", true, Time_Set, Mode, Set_Volume);
                else if (Music_Index == 13)
                    Add_Sound("404033224", Music_File, "ja", true, Time_Set, Mode, Set_Volume);
                else if (Music_Index == 14)
                    Add_Sound("480862388", Music_File, "ja", true, Time_Set, Mode, Set_Volume);
                else if (Music_Index == 15)
                    Add_Sound("797792182", Music_File, "ja", true, Time_Set, Mode, Set_Volume);
                else if (Music_Index == 16)
                    Add_Sound("761638380", Music_File, "ja", true, Time_Set, Mode, Set_Volume);
                else if (Music_Index == 17)
                    Add_Sound("434309394", Music_File, "ja", true, Time_Set, Mode, Set_Volume);
                else if (Music_Index == 18)
                    Add_Sound("868083406", Music_File, "ja", true, Time_Set, Mode, Set_Volume);
            }
            else if (Page == 1)
            {
                if (Music_Index == 0)
                    Add_Sound("1061271437", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 1)
                    Add_Sound("209834362", Music_File, "ja", true, null, "", 0, true);
                else if (Music_Index == 2)
                    Add_Sound("79593697", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 3)
                    Add_Sound("818900211", Music_File, "ja", true, null, "", 0, true);
                else if (Music_Index == 4)
                    Add_Sound("409835290", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 5)
                    Add_Sound("282116325", Music_File, "ja", true, null, "", 0, true);
                else if (Music_Index == 6)
                    Add_Sound("432722439", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 7)
                    Add_Sound("26462958", Music_File, "ja", true, null, "", 0, true);
                else if (Music_Index == 8)
                    Add_Sound("278676259", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 9)
                    Add_Sound("1015843643", Music_File, "ja", true, null, "", 0, true);
                else if (Music_Index == 10)
                    Add_Sound("123366428", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 11)
                    Add_Sound("1034987615", Music_File, "ja", true, null, "", 0, true);
                else if (Music_Index == 12)
                    Add_Sound("1001742020", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 13)
                    Add_Sound("537387720", Music_File, "ja", true, null, "", 0, true);
                else if (Music_Index == 14)
                    Add_Sound("251988040", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 15)
                    Add_Sound("56850118", Music_File, "ja", true, null, "", 0, true);
                else if (Music_Index == 16)
                    Add_Sound("530790297", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 17)
                    Add_Sound("1036212148", Music_File, "ja", true, null, "", 0, true);
                else if (Music_Index == 18)
                    Add_Sound("660827574", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 19)
                    Add_Sound("192152217", Music_File, "ja", true, null, "", 0, true);
            }
            else if (Page == 2)
                Add_Sound(Sub_Code.Get_WoTB_New_Gun_Sound_ShortID(Music_Index).ToString(), Music_File, "SFX", true, null, "", 0, true);
            else if (Page == 3)
            {
                if (Music_Index == 0)
                    Add_Sound("221586381", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 1)
                    Add_Sound("696038859", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 2)
                    Add_Sound("748376026", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 3)
                    Add_Sound("91695445", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 4)
                    Add_Sound("68884127", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 5)
                    Add_Sound("680504991", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 6)
                    Add_Sound("1056432059", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 7)
                    Add_Sound("860592172", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 8)
                    Add_Sound("461084879", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 9)
                    Add_Sound("1027463243", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 10)
                    Add_Sound("217238183", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 11)
                    Add_Sound("721529499", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 12)
                    Add_Sound("974323287", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 13)
                    Add_Sound("1037140024", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 14)
                    Add_Sound("665764359", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 15)
                    Add_Sound("557544000", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 16)
                    Add_Sound("1016080233", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 17)
                    Add_Sound("501705569", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 18)
                    Add_Sound("11684242", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 19)
                    Add_Sound("916105171", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 20)
                    Add_Sound("93927311", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 21)
                    Add_Sound("183820323", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 22)
                    Add_Sound("961236444", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 23)
                    Add_Sound("692776620", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 24)
                    Add_Sound("68108929", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 25)
                    Add_Sound("775095371", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 26)
                    Add_Sound("832717544", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 27)
                    Add_Sound("326365235", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 28)
                    Add_Sound("425174247", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 29)
                    Add_Sound("864814542", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 30)
                    Add_Sound("802218981", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 31)
                    Add_Sound("296120333", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 32)
                    Add_Sound("975555946", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 33)
                    Add_Sound("112885591", Music_File, "SFX", true, null, "", 0, true);
                else if (Music_Index == 34)
                    Add_Sound("605432888", Music_File, "SFX", true, null, "", 0, true);
            }
        }
        //音声をプロジェクトに追加
        public void Sound_Add_Wwise(string Dir_Name, bool IsWoT_Project = false, bool IsNotIncludeBGM = false)
        {
            string[] Voice_Files_01 = Directory.GetFiles(Dir_Name, "*.wav", SearchOption.TopDirectoryOnly);
            foreach (string Voice_Now in Voice_Files_01)
            {
                string Name_Only;
                if (Path.GetFileNameWithoutExtension(Voice_Now).Contains("reload"))
                    Name_Only = Voice_Mod_Create.Get_Voice_Type_V2(Voice_Now);
                else
                    Name_Only = Voice_Mod_Create.Get_Voice_Type_V1(Voice_Now);
                if (IsWoT_Project)
                {
                    switch (Name_Only)
                    {
                        case "mikata":
                            Add_Sound("496744290", Voice_Now, "Japanese");
                            break;
                        case "danyaku":
                            Add_Sound("80768846", Voice_Now, "Japanese");
                            break;
                        case "hikantuu":
                            Add_Sound("611225375", Voice_Now, "Japanese");
                            Add_Sound("894898226", Voice_Now, "Japanese");
                            Add_Sound("91710501", Voice_Now, "Japanese");
                            Add_Sound("904946415", Voice_Now, "Japanese");
                            break;
                        case "kantuu":
                            Add_Sound("569021006", Voice_Now, "Japanese");
                            Add_Sound("586836082", Voice_Now, "Japanese");
                            Add_Sound("198894981", Voice_Now, "Japanese");
                            break;
                        case "tokusyu":
                            Add_Sound("114246473", Voice_Now, "Japanese");
                            Add_Sound("758697169", Voice_Now, "Japanese");
                            Add_Sound("544988916", Voice_Now, "Japanese");
                            Add_Sound("315035946", Voice_Now, "Japanese");
                            break;
                        case "tyoudan":
                            Add_Sound("976240215", Voice_Now, "Japanese");
                            break;
                        case "syatyou":
                            Add_Sound("694419949", Voice_Now, "Japanese");
                            break;
                        case "souzyuusyu":
                            Add_Sound("750503602", Voice_Now, "Japanese");
                            break;
                        case "tekikasai":
                            Add_Sound("675832160", Voice_Now, "Japanese");
                            break;
                        case "gekiha":
                            Add_Sound("961047801", Voice_Now, "Japanese");
                            Add_Sound("921878250", Voice_Now, "Japanese");
                            break;
                        case "enjinhason":
                            Add_Sound("156949684", Voice_Now, "Japanese");
                            break;
                        case "enjintaiha":
                            Add_Sound("779775207", Voice_Now, "Japanese");
                            break;
                        case "enjinhukkyuu":
                            Add_Sound("153685024", Voice_Now, "Japanese");
                            break;
                        case "kasai":
                            Add_Sound("55455728", Voice_Now, "Japanese");
                            break;
                        case "syouka":
                            Add_Sound("108269930", Voice_Now, "Japanese");
                            break;
                        case "nenryou":
                            Add_Sound("518449764", Voice_Now, "Japanese");
                            break;
                        case "housinhason":
                            Add_Sound("396042361", Voice_Now, "Japanese");
                            break;
                        case "housintaiha":
                            Add_Sound("111157803", Voice_Now, "Japanese");
                            break;
                        case "housinhukkyuu":
                            Add_Sound("468738015", Voice_Now, "Japanese");
                            break;
                        case "housyu":
                            Add_Sound("290791237", Voice_Now, "Japanese");
                            break;
                        case "soutensyu":
                            Add_Sound("574952129", Voice_Now, "Japanese");
                            break;
                        case "musen":
                            Add_Sound("947428834", Voice_Now, "Japanese");
                            break;
                        case "musensyu":
                            Add_Sound("557399837", Voice_Now, "Japanese");
                            break;
                        case "battle":
                            Add_Sound("629260003", Voice_Now, "Japanese");
                            break;
                        case "kansokuhason":
                            Add_Sound("223610709", Voice_Now, "Japanese");
                            Add_Sound("60412176", Voice_Now, "Japanese");
                            break;
                        case "kansokutaiha":
                            Add_Sound("693388069", Voice_Now, "Japanese");
                            break;
                        case "kansokuhukkyuu":
                            Add_Sound("407686850", Voice_Now, "Japanese");
                            break;
                        case "ritaihason":
                            Add_Sound("189198188", Voice_Now, "Japanese");
                            break;
                        case "ritaitaiha":
                            Add_Sound("113684286", Voice_Now, "Japanese");
                            break;
                        case "ritaihukkyuu":
                            Add_Sound("764683321", Voice_Now, "Japanese");
                            Add_Sound("1035064657", Voice_Now, "Japanese");
                            break;
                        case "houtouhason":
                            Add_Sound("755962743", Voice_Now, "Japanese");
                            break;
                        case "houtoutaiha":
                            Add_Sound("464113292", Voice_Now, "Japanese");
                            break;
                        case "houtouhukkyuu":
                            Add_Sound("230672484", Voice_Now, "Japanese");
                            break;
                        case "taiha":
                            Add_Sound("1015146434", Voice_Now, "Japanese");
                            break;
                    }
                }
                else
                {
                    if (Name_Only == "mikata" || Name_Only == "ally_killed_by_player")
                        Add_Sound("170029050", Voice_Now, "ja");
                    else if (Name_Only == "danyaku" || Name_Only == "ammo_bay_damaged")
                        Add_Sound("95559763", Voice_Now, "ja");
                    else if (Name_Only == "hikantuu" || Name_Only == "armor_not_pierced_by_player")
                        Add_Sound("766083947", Voice_Now, "ja");
                    else if (Name_Only == "kantuu" || Name_Only == "armor_pierced_by_player")
                        Add_Sound("569784404", Voice_Now, "ja");
                    else if (Name_Only == "tokusyu" || Name_Only == "armor_pierced_crit_by_player")
                        Add_Sound("266422868", Voice_Now, "ja");
                    else if (Name_Only == "tyoudan" || Name_Only == "armor_ricochet_by_player")
                        Add_Sound("1052258113", Voice_Now, "ja");
                    else if (Name_Only == "syatyou" || Name_Only == "commander_killed")
                        Add_Sound("242302464", Voice_Now, "ja");
                    else if (Name_Only == "souzyuusyu" || Name_Only == "driver_killed")
                        Add_Sound("334837201", Voice_Now, "ja");
                    else if (Name_Only == "tekikasai" || Name_Only == "enemy_fire_started_by_player")
                        Add_Sound("381780774", Voice_Now, "ja");
                    else if (Name_Only == "gekiha" || Name_Only == "enemy_killed_by_player")
                        Add_Sound("489572734", Voice_Now, "ja");
                    else if (Name_Only == "enjinhason" || Name_Only == "engine_damaged")
                        Add_Sound("210078142", Voice_Now, "ja");
                    else if (Name_Only == "enjintaiha" || Name_Only == "engine_destroyed")
                        Add_Sound("249535989", Voice_Now, "ja");
                    else if (Name_Only == "enjinhukkyuu" || Name_Only == "engine_functional")
                        Add_Sound("908710042", Voice_Now, "ja");
                    else if (Name_Only == "kasai" || Name_Only == "fire_started")
                        Add_Sound("1057023960", Voice_Now, "ja");
                    else if (Name_Only == "syouka" || Name_Only == "fire_stopped")
                        Add_Sound("953778289", Voice_Now, "ja");
                    else if (Name_Only == "nenryou" || Name_Only == "fuel_tank_damaged")
                        Add_Sound("121897540", Voice_Now, "ja");
                    else if (Name_Only == "housinhason" || Name_Only == "gun_damaged")
                        Add_Sound("127877647", Voice_Now, "ja");
                    else if (Name_Only == "housintaiha" || Name_Only == "gun_destroyed")
                        Add_Sound("462397017", Voice_Now, "ja");
                    else if (Name_Only == "housinhukkyuu" || Name_Only == "gun_functional")
                        Add_Sound("651656679", Voice_Now, "ja");
                    else if (Name_Only == "housyu" || Name_Only == "gunner_killed")
                        Add_Sound("739086111", Voice_Now, "ja");
                    else if (Name_Only == "soutensyu" || Name_Only == "loader_killed")
                        Add_Sound("363753108", Voice_Now, "ja");
                    else if (Name_Only == "musen" || Name_Only == "radio_damaged")
                        Add_Sound("91697210", Voice_Now, "ja");
                    else if (Name_Only == "musensyu" || Name_Only == "radioman_killed")
                        Add_Sound("987172940", Voice_Now, "ja");
                    else if (Name_Only == "battle" || Name_Only == "start_battle")
                        Add_Sound("518589126", Voice_Now, "ja");
                    else if (Name_Only == "kansokuhason" || Name_Only == "surveying_devices_damaged")
                        Add_Sound("330491031", Voice_Now, "ja");
                    else if (Name_Only == "kansokutaiha" || Name_Only == "surveying_devices_destroyed")
                        Add_Sound("792301846", Voice_Now, "ja");
                    else if (Name_Only == "kansokuhukkyuu" || Name_Only == "surveying_devices_functional")
                        Add_Sound("539730785", Voice_Now, "ja");
                    else if (Name_Only == "ritaihason" || Name_Only == "track_damaged")
                        Add_Sound("38261315", Voice_Now, "ja");
                    else if (Name_Only == "ritaitaiha" || Name_Only == "track_destroyed")
                        Add_Sound("37535832", Voice_Now, "ja");
                    else if (Name_Only == "ritaihukkyuu" || Name_Only == "track_functional")
                    {
                        Add_Sound("558576963", Voice_Now, "ja");
                        Add_Sound("403125077", Voice_Now, "ja");
                    }
                    else if (Name_Only == "houtouhason" || Name_Only == "turret_rotator_damaged")
                        Add_Sound("1014565012", Voice_Now, "ja");
                    else if (Name_Only == "houtoutaiha" || Name_Only == "turret_rotator_destroyed")
                        Add_Sound("135817430", Voice_Now, "ja");
                    else if (Name_Only == "houtouhukkyuu" || Name_Only == "turret_rotator_functional")
                        Add_Sound("985679417", Voice_Now, "ja");
                    else if (Name_Only == "taiha" || Name_Only == "vehicle_destroyed")
                        Add_Sound("164671745", Voice_Now, "ja");
                    else if (Name_Only == "hakken")
                        Add_Sound("447063394", Voice_Now, "ja");
                    else if (Name_Only == "lamp")
                        Add_Sound("154835998", Voice_Now, "ja");
                    else if (Name_Only == "ryoukai")
                        Add_Sound("607694618", Voice_Now, "ja");
                    else if (Name_Only == "kyohi")
                        Add_Sound("391276124", Voice_Now, "ja");
                    else if (Name_Only == "help")
                        Add_Sound("840378218", Voice_Now, "ja");
                    else if (Name_Only == "attack")
                        Add_Sound("549968154", Voice_Now, "ja");
                    else if (Name_Only == "attack_now")
                        Add_Sound("1015337424", Voice_Now, "ja");
                    else if (Name_Only == "capture")
                        Add_Sound("271044645", Voice_Now, "ja");
                    else if (Name_Only == "defence")
                        Add_Sound("310153012", Voice_Now, "ja");
                    else if (Name_Only == "keep")
                        Add_Sound("379548034", Voice_Now, "ja");
                    else if (Name_Only == "lock")
                        Add_Sound("839607605", Voice_Now, "ja");
                    else if (Name_Only == "unlock")
                        Add_Sound("233444430", Voice_Now, "ja");
                    else if (Name_Only == "reload")
                        Add_Sound("299739777", Voice_Now, "ja");
                    else if (Name_Only == "map")
                        Add_Sound("120795627", Voice_Now, "ja");
                    else if (Name_Only == "battle_end")
                        Add_Sound("924876614", Voice_Now, "ja");
                    else if (Name_Only == "battle_bgm")
                    {
                        if (!IsNotIncludeBGM)
                            Add_Sound("649358221", Voice_Now, "ja");
                    }
                    else if (Name_Only == "load_bgm")
                        Add_Sound("915105627", Voice_Now, "ja");
                    else if (Name_Only == "chat_allies_send")
                        Add_Sound("491691546", Voice_Now, "ja");
                    else if (Name_Only == "chat_allies_receive")
                        Add_Sound("417768496", Voice_Now, "ja");
                    else if (Name_Only == "chat_enemy_send")
                        Add_Sound("46472417", Voice_Now, "ja");
                    else if (Name_Only == "chat_enemy_receive")
                        Add_Sound("681331945", Voice_Now, "ja");
                    else if (Name_Only == "chat_platoon_send")
                        Add_Sound("190711689", Voice_Now, "ja");
                    else if (Name_Only == "chat_platoon_receive")
                        Add_Sound("918836720", Voice_Now, "ja");
                }
            }
        }
        public void Sound_Add_Wwise(List<List<Voice_Event_Setting>> Event_Settings, WVS_Load WVS_File, List<string> SE_Preset, List<Dictionary<uint, string>> Default_SE)
        {
            foreach (List<Voice_Event_Setting> Settings in Event_Settings)
            {
                foreach (Voice_Event_Setting Setting in Settings)
                {
                    Add_Sound(Setting, WVS_File);
                    if (Setting.SE_Index != -1)
                    {
                        if (Voice_Set.SE_Enable_Disable[Setting.SE_Index - 1])
                        {
                            string Temp = SE_Preset[Setting.SE_Index];
                            foreach (string File_Now in Temp.Split('|'))
                                if (File.Exists(File_Now))
                                    Add_Sound(Setting.SE_ShortID, File_Now, Setting.SE_Volume);
                        }
                        else
                        {
                            foreach (uint ShortID in Default_SE[Setting.SE_Index - 1].Keys)
                            {
                                string Temp = Default_SE[Setting.SE_Index - 1][ShortID];
                                foreach (string File_Now in Temp.Split('|'))
                                    if (File.Exists(File_Now))
                                        Add_Sound(ShortID, File_Now);
                            }
                        }
                    }
                }
            }
            SE_Add_Wwise(SE_Preset, Default_SE);
        }
        public void Sound_Add_Wwise(List<Dictionary<string, Voice_Event_Setting>> Event_Settings, Dictionary<string, List<SE_Info_Parent>> SE_Info, WVS_Load WVS_File)
        {
            foreach (Dictionary<string, Voice_Event_Setting> Settings in Event_Settings)
                foreach (string Key_Name in Settings.Keys)
                    Add_Sound(Settings[Key_Name], WVS_File, false, true, "Japanese");
            foreach (string Key_Name in SE_Info.Keys)
                foreach (SE_Info_Parent Parent in SE_Info[Key_Name])
                    Add_Sound(Parent, WVS_File);
        }
        public void Sound_Add_Wwise(List<Voice_Event_Setting> Event_Settings, List<string> SE_Preset, List<Dictionary<uint, string>> Default_SE)
        {
            foreach (Voice_Event_Setting Settings in Event_Settings)
            {
                Add_Sound(Settings, null, true);
                if (Settings.SE_Index != -1)
                {
                    if (Voice_Set.SE_Enable_Disable[Settings.SE_Index - 1])
                    {
                        string Temp = SE_Preset[Settings.SE_Index];
                        foreach (string File_Now in Temp.Split('|'))
                            if (File.Exists(File_Now))
                                Add_Sound(Settings.SE_ShortID, File_Now, Settings.SE_Volume);
                    }
                    else
                    {
                        foreach (uint ShortID in Default_SE[Settings.SE_Index - 1].Keys)
                        {
                            string Temp = Default_SE[Settings.SE_Index - 1][ShortID];
                            foreach (string File_Now in Temp.Split('|'))
                                if (File.Exists(File_Now))
                                    Add_Sound(ShortID, File_Now);
                        }
                    }
                }
            }
            SE_Add_Wwise(SE_Preset, Default_SE);
        }
        private void SE_Add_Wwise(List<string> SE_Preset, List<Dictionary<uint, string>> Default_SE)
        {
            Delete_CAkSounds("816581364");
            Delete_CAkSounds("921545948");
            if (Voice_Set.SE_Enable_Disable[13])
            {
                string Temp = SE_Preset[14];
                foreach (string File_Now in Temp.Split('|'))
                    if (File.Exists(File_Now))
                        Add_Sound(816581364, File_Now, 0, true);
            }
            else
            {
                string Temp = Default_SE[13][816581364];
                foreach (string File_Now in Temp.Split('|'))
                    if (File.Exists(File_Now))
                        Add_Sound(816581364, File_Now, 0, true);
            }
            if (Voice_Set.SE_Enable_Disable[16])
            {
                string Temp = SE_Preset[17];
                foreach (string File_Now in Temp.Split('|'))
                    if (File.Exists(File_Now))
                        Add_Sound(921545948, File_Now, 0);
            }
            else
            {
                string Temp = Default_SE[16][921545948];
                foreach (string File_Now in Temp.Split('|'))
                    if (File.Exists(File_Now))
                        Add_Sound(921545948, File_Now, 0);
            }
        }
        public async Task Encode_WAV()
        {
            List<string> To_Files = new List<string>();
            for (int Number = 0; Number < Add_Voice_From.Count; Number++)
                To_Files.Add(Project_Dir + "\\Originals\\" + Add_Voice_Type[Number] + "\\" + Add_Voice_To[Number] + ".wav");
            await Multithread.Convert_To_Wav(Add_Voice_From.ToArray(), To_Files.ToArray(), false, true);
            for (int Number = 0; Number < Add_Voice_From.Count; Number++)
                if (Add_Voice_From[Number].Contains(Project_Dir + "\\Originals\\" + Add_Voice_Type[Number] + "\\"))
                    File.Delete(Add_Voice_From[Number]);
        }
        public void Set_Volume()
        {
            List<string> To_Files = new List<string>();
            for (int Number = 0; Number < Add_Voice_From.Count; Number++)
                if (Add_Voice_Type[Number] != "SFX")
                    To_Files.Add(Project_Dir + "\\Originals\\" + Add_Voice_Type[Number] + "\\" + Add_Voice_To[Number] + ".wav");
            Sub_Code.Volume_Set(To_Files.ToArray(), Encode_Mode.WAV);
        }
        //イベント内の指定したShortIDの項目を無効にする
        //階層が存在する場合はフォルダの指定もする必要あり
        public void Event_Not_Include(string Event_Name, uint ShortID)
        {
            if (!File.Exists(Project_Dir + "/Events/" + Event_Name + ".wwu"))
                return;
            if (!File.Exists(Project_Dir + "/Events/" + Event_Name + ".wwu.bak"))
                File.Copy(Project_Dir + "/Events/" + Event_Name + ".wwu", Project_Dir + "/Events/" + Event_Name + ".wwu.bak", true);
            List<string> Lines = new List<string>();
            Lines.AddRange(File.ReadAllLines(Project_Dir + "/Events/" + Event_Name + ".wwu"));
            int ShortID_Line = 0;
            for (int Number = 0; Number < Lines.Count; Number++)
            {
                if (Lines[Number].Contains("ShortID=\"" + ShortID + "\""))
                {
                    ShortID_Line = Number;
                    break;
                }
            }
            if (ShortID_Line == 0)
                return;
            More_Class.List_Init(ShortID_Line + 1);
            More_Class.List_Add(Lines, "<PropertyList>");
            More_Class.List_Add(Lines, "<Property Name=\"Inclusion\" Type=\"bool\">");
            More_Class.List_Add(Lines, "<ValueList>");
            More_Class.List_Add(Lines, "<Value>False</Value>");
            More_Class.List_Add(Lines, "</ValueList>");
            More_Class.List_Add(Lines, "</Property>");
            More_Class.List_Add(Lines, "</PropertyList>");
            try
            {
                File.WriteAllLines(Project_Dir + "/Events/" + Event_Name + ".wwu", Lines.ToArray());
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
            Lines.Clear();
        }
        //Event_Not_Include()を行った場合必ず実行
        public void Event_Reset()
        {
            foreach (string Name_Now in Directory.GetFiles(Project_Dir + "/Events", "*.bak", SearchOption.TopDirectoryOnly))
            {
                string To_File = Path.GetDirectoryName(Name_Now) + "\\" + Path.GetFileNameWithoutExtension(Name_Now);
                Sub_Code.File_Move(Name_Now, To_File, true);
            }
        }
        //CAkSoundをすべて削除する
        public void Clear_All_Sounds(List<List<Voice_Event_Setting>> Sound_Settings)
        {
            foreach (List<Voice_Event_Setting> Settings in Sound_Settings)
            {
                foreach (Voice_Event_Setting Setting in Settings)
                {
                    if (Setting.SE_ShortID != 0)
                        Delete_CAkSounds(Setting.SE_ShortID);
                    Delete_CAkSounds(Setting.Voice_ShortID);
                }
            }
        }
        public void Clear_All_Sounds(List<Dictionary<string, Voice_Event_Setting>> Sound_Settings, Dictionary<string, List<SE_Info_Parent>> SE_Info)
        {
            foreach (Dictionary<string, Voice_Event_Setting> Settings in Sound_Settings)
            {
                foreach (string Key_Name in Settings.Keys)
                {
                    if (Settings[Key_Name].SE_ShortID != 0)
                        Delete_CAkSounds(Settings[Key_Name].SE_ShortID);
                    Delete_CAkSounds(Settings[Key_Name].Voice_ShortID);
                }
            }
            foreach (string Key_Name in SE_Info.Keys)
                foreach (SE_Info_Parent Parent in SE_Info[Key_Name])
                    Delete_CAkSounds(Parent.SE_ShortID);
        }
        public void Clear_All_Sounds(List<Voice_Event_Setting> Sound_Settings)
        {
            foreach (Voice_Event_Setting Setting in Sound_Settings)
            {
                if (Setting.SE_ShortID != 0)
                    Delete_CAkSounds(Setting.SE_ShortID);
                Delete_CAkSounds(Setting.Voice_ShortID);
            }
        }
    }
}
public class More_Class
{
    static Random r = new Random();
    static int Init_Line = 0;
    //指定した行に文字を挿入
    public static void List_Init(int Line_Number)
    {
        Init_Line = Line_Number;
    }
    public static int List_Get_Line()
    {
        return Init_Line;
    }
    public static void List_Add(List<string> Lists, string Text)
    {
        Lists.Insert(Init_Line, Text);
        Init_Line++;
    }
    //ランダムな9個の数字を並べる(ShortID用)
    public static string CreateShortID()
    {
        string ID = "";
        ID += r.Next(1, 10);
        ID += r.Next(1, 10);
        ID += r.Next(1, 10);
        ID += r.Next(1, 10);
        ID += r.Next(1, 10);
        ID += r.Next(1, 10);
        ID += r.Next(1, 10);
        ID += r.Next(1, 10);
        ID += r.Next(1, 10);
        return ID;
    }
}
public class WwiseHash
{
    //GUIDからShortIDを生成
    public static uint HashGUID(string ID)
    {
        Regex alphanum = new Regex("[^0-9A-Za-z]");
        string filtered = alphanum.Replace(ID, "");
        List<byte> guidBytes = new List<byte>();
        int[] byteOrder = { 3, 2, 1, 0, 5, 4, 7, 6, 8, 9, 10, 11, 12, 13, 14, 15 };
        for (int i = 0; i < byteOrder.Length; i++)
            guidBytes.Add(byte.Parse(filtered.Substring(byteOrder[i] * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
        return FnvHash(guidBytes.ToArray(), false);
    }
    public static uint HashString(string Name)
    {
        return FnvHash(Encoding.ASCII.GetBytes(Name.ToLowerInvariant()), true);
    }
    static uint FnvHash(byte[] input, bool use32bits)
    {
        uint prime = 16777619;
        uint offset = 2166136261;
        uint mask = 1073741823;
        uint hash = offset;
        for (int i = 0; i < input.Length; i++)
        {
            hash *= prime;
            hash ^= input[i];
        }
        if (use32bits)
            return hash;
        else
            return (hash >> 30) ^ (hash & mask);
    }
}