using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
        string Project_Dir;
        List<string> Actor_Mixer_Hierarchy = new List<string>();
        List<string> Add_Wav_Files = new List<string>();
        List<string> Add_All_Files = new List<string>();
        List<Music_Play_Time> Add_All_Files_Time = new List<Music_Play_Time>();
        int Battle_Number = 0;
        //プロジェクトファイルの内容を取得
        public Wwise_Project_Create(string Project_Dir)
        {
            try
            {
                if (!File.Exists(Project_Dir + "/Actor-Mixer Hierarchy/Default Work Unit.wwu"))
                {
                    return;
                }
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
            await Multithread.Convert_To_Wav(Add_All_Files, Add_Wav_Files, Add_All_Files_Time, false);
        }
        //取得したデータから指定したイベントにサウンドを追加(Save()が呼ばれるまで保存しない)
        public bool Add_Sound(string Container_ShortID, string Audio_File, string Language, bool IsSetShortIDMode = false, Music_Play_Time Time = null, string Effect = "")
        {
            if (Project_Dir == "")
            {
                return false;
            }
            try
            {
                uint FileName_Short_ID = WwiseHash.HashString(Audio_File + Container_ShortID);
                if (Language == "SFX")
                {
                    if (File.Exists(Audio_File))
                    {
                        if (Time == null)
                            File.Copy(Audio_File, Project_Dir + "/Originals/SFX/" + FileName_Short_ID + Path.GetExtension(Audio_File), true);
                        else
                        {
                            Add_All_Files_Time.Add(Time);
                        }
                        Add_Wav_Files.Add(Project_Dir + "/Originals/SFX/" + FileName_Short_ID + ".wav");
                        Add_All_Files.Add(Audio_File);
                    }
                }
                else if (Language != null)
                {
                    if (File.Exists(Audio_File))
                    {
                        if (Time == null)
                        {
                            File.Copy(Audio_File, Project_Dir + "/Originals/Voices/" + Language + "/" + FileName_Short_ID + Path.GetExtension(Audio_File), true);
                            Add_All_Files_Time.Add(new Music_Play_Time(0, 9999));
                        }
                        else
                        {
                            Add_All_Files_Time.Add(Time);
                        }
                        Add_Wav_Files.Add(Project_Dir + "/Originals/Voices/" + Language + "/" + FileName_Short_ID + ".wav");
                        Add_All_Files.Add(Audio_File);
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
                {
                    return false;
                }
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
                    {
                        ReferenceListEnd_Line = Number;
                    }
                    if (Actor_Mixer_Hierarchy[Number].Contains("</RandomSequenceContainer>"))
                    {
                        break;
                    }
                }
                if (ChildrenList_Line == 0 && ReferenceListEnd_Line == 0)
                {
                    return false;
                }
                else if (ChildrenList_Line == 0)
                {
                    More_Class.List_Init(ReferenceListEnd_Line + 1);
                    More_Class.List_Add(Actor_Mixer_Hierarchy, "<ChildrenList>");
                    More_Class.List_Add(Actor_Mixer_Hierarchy, "</ChildrenList>");
                    if (IsSetShortIDMode)
                    {
                        return List_Add_File(ReferenceListEnd_Line + 2, FileName_Short_ID + ".wav", Language, FileName_Short_ID, Effect);
                    }
                    return List_Add_File(ReferenceListEnd_Line + 2, FileName_Short_ID + ".wav", Language);
                }
                else
                {
                    if (IsSetShortIDMode)
                    {
                        return List_Add_File(ChildrenList_Line + 1, FileName_Short_ID + ".wav", Language, FileName_Short_ID, Effect);
                    }
                    return List_Add_File(ChildrenList_Line + 1, FileName_Short_ID + ".wav", Language);
                }
            }
            catch
            {
                return false;
            }
        }
        //追加したサウンドをファイルに保存する
        public bool Save()
        {
            if (Project_Dir == "")
            {
                return false;
            }
            try
            {
                File.WriteAllLines(Project_Dir + "/Actor-Mixer Hierarchy/Default Work Unit.wwu", Actor_Mixer_Hierarchy.ToArray());
                return true;
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                return false;
            }
        }
        //保存されたデータをもとに.bnkファイルをビルド(内容によって時間がかかります)
        public void Project_Build(string BankName, string OutputFilePath, string GeneratedSoundBanksPath = null)
        {
            if (Project_Dir == "" || !Directory.Exists(Path.GetDirectoryName(OutputFilePath)))
            {
                return;
            }
            try
            {
                //廃止
                /*if (BankName == "reload" && !IsIncludeBGM)
                {
                    //BGMが含まれていなければ強制的に追加
                    Sub_Code.Audio_Encode_To_Other(Voice_Set.Special_Path + "/Wwise/Not_Voice.mp3", Voice_Set.Special_Path + "/Wwise/Battle_Music_01.wav", "wav", false);
                    File.Copy(Voice_Set.Special_Path + "/Wwise/Battle_Music_01.wav", Voice_Set.Special_Path + "/Wwise/Battle_Music_02.wav", true);
                    File.Copy(Voice_Set.Special_Path + "/Wwise/Battle_Music_01.wav", Voice_Set.Special_Path + "/Wwise/Battle_Music_03.wav", true);
                    File.Copy(Voice_Set.Special_Path + "/Wwise/Battle_Music_01.wav", Voice_Set.Special_Path + "/Wwise/Battle_Music_04.wav", true);
                    File.Copy(Voice_Set.Special_Path + "/Wwise/Battle_Music_01.wav", Voice_Set.Special_Path + "/Wwise/Battle_Music_05.wav", true);
                    Add_Sound("649358221", Voice_Set.Special_Path + "/Wwise/Battle_Music_01.wav", "ja");
                    Add_Sound("649358221", Voice_Set.Special_Path + "/Wwise/Battle_Music_02.wav", "ja");
                    Add_Sound("649358221", Voice_Set.Special_Path + "/Wwise/Battle_Music_03.wav", "ja");
                    Add_Sound("649358221", Voice_Set.Special_Path + "/Wwise/Battle_Music_04.wav", "ja");
                    Add_Sound("649358221", Voice_Set.Special_Path + "/Wwise/Battle_Music_05.wav", "ja");
                    File.Delete(Voice_Set.Special_Path + "/Wwise/Battle_Music_01.wav");
                    File.Delete(Voice_Set.Special_Path + "/Wwise/Battle_Music_02.wav");
                    File.Delete(Voice_Set.Special_Path + "/Wwise/Battle_Music_03.wav");
                    File.Delete(Voice_Set.Special_Path + "/Wwise/Battle_Music_04.wav");
                    File.Delete(Voice_Set.Special_Path + "/Wwise/Battle_Music_05.wav");
                    IsIncludeBGM = true;
                }*/
                BankName = BankName.Replace(" ", "_");
                string Project_File = Directory.GetFiles(Project_Dir, "*.wproj", SearchOption.TopDirectoryOnly)[0];
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Wwise/Project_Build.bat");
                stw.WriteLine("chcp 65001");
                stw.Write("\"" + Voice_Set.Special_Path + "/Wwise/x64/Release/bin/WwiseCLI.exe\" \"" + Project_File + "\" -GenerateSoundBanks -Language ja -Platform Windows ");
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
                    GeneratedFile = Directory.GetFiles(Project_Dir + "/GeneratedSoundBanks/Windows", BankName + ".bnk", SearchOption.AllDirectories)[0];
                }
                else
                {
                    GeneratedFile = Directory.GetFiles(Project_Dir + "/GeneratedSoundBanks/" + GeneratedSoundBanksPath, BankName + ".bnk", SearchOption.AllDirectories)[0];
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
        public void Clear(string CachePath = null)
        {
            try
            {
                foreach (string File_Now in Add_Wav_Files)
                {
                    Sub_Code.File_Delete_V2(File_Now);
                    Sub_Code.File_Delete_V2(File_Now.Replace(".wav", ".akd"));
                }
                string[] GetWEMFiles;
                if (CachePath == null)
                {
                    GetWEMFiles = Directory.GetFiles(Project_Dir + "/.cache/Windows", "*.wem", SearchOption.AllDirectories);
                }
                else
                {
                    GetWEMFiles = Directory.GetFiles(Project_Dir + "/.cache/" + CachePath, "*.wem", SearchOption.AllDirectories);
                }
                foreach (string File_Now in GetWEMFiles)
                {
                    Sub_Code.File_Delete_V2(File_Now);
                }
                Actor_Mixer_Hierarchy.Clear();
                Add_Wav_Files.Clear();
                Add_All_Files.Clear();
                Add_All_Files_Time.Clear();
                Project_Dir = "";
                Battle_Number = 0;
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        //以下の文字を指定した行に追加
        //GUIDやSourceIDはどんな数でも問題ないっぽいのでランダムに作成させる
        bool List_Add_File(int Line_Number, string FileName, string Language, uint Set_Media_ID = 0, string Effect = "")
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
                {
                    AudioFileSource_ID = Set_Media_ID;
                }
                //BGMはあとから変更できるように500000000から始まるように設定
                if (FileName.Contains("battle_bgm_"))
                {
                    AudioFileSource_ID = (uint)(500000000 + Battle_Number);
                    Battle_Number++;
                }
                More_Class.List_Init(Line_Number);
                More_Class.List_Add(Actor_Mixer_Hierarchy, "<Sound Name=\"" + Replace_Name + "\" ID=\"{" + Sound_GUID + "}\" ShortID=\"" + Sound_ID + "\">");
                if (Language != "SFX")
                {
                    More_Class.List_Add(Actor_Mixer_Hierarchy, "<PropertyList>");
                    More_Class.List_Add(Actor_Mixer_Hierarchy, "<Property Name=\"IsVoice\" Type=\"bool\" Value=\"True\"/>");
                    More_Class.List_Add(Actor_Mixer_Hierarchy, "</PropertyList>");
                }
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
                if (Effect != "")
                {
                    More_Class.List_Add(Actor_Mixer_Hierarchy, "<PropertyList>");
                    if (Effect == "Feed_In")
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
        //プロジェクトに戦闘BGMを追加
        public void Sound_Music_Add_Wwise(string Dir_Name)
        {
            string[] Voice_Files_01 = Directory.GetFiles(Dir_Name, "*.wav", SearchOption.TopDirectoryOnly);
            foreach (string Voice_Now in Voice_Files_01)
            {
                string Name_Only;
                if (Path.GetFileNameWithoutExtension(Voice_Now).Contains("reload"))
                {
                    Name_Only = Voice_Mod_Create.Get_Voice_Type_V2(Voice_Now);
                }
                else
                {
                    Name_Only = Voice_Mod_Create.Get_Voice_Type_V1(Voice_Now);
                }
                switch (Name_Only)
                {
                    case "battle_bgm":
                        Add_Sound("649358221", Voice_Now, "ja");
                        break;
                }
            }
        }
        //ロードBGMをプロジェクトに追加
        public void Loading_Music_Add_Wwise(string Music_File, int Music_Index, Music_Play_Time Time, bool IsFeed_In_Mode)
        {
            string Mode = "";
            Music_Play_Time Time_Set = new Music_Play_Time(Time.Start_Time, Time.End_Time);
            if (Time.End_Time == 0)
            {
                Time_Set.End_Time = 9999;
            }
            if (IsFeed_In_Mode)
            {
                Mode = "Feed_In";
            }
            if (Music_Index == 0)
                Add_Sound("205170598", Music_File, "ja", true, Time_Set, Mode);
            if (Music_Index == 1)
                Add_Sound("148841988", Music_File, "ja", true, Time_Set, Mode);
            if (Music_Index == 2)
                Add_Sound("1067185674", Music_File, "ja", true, Time_Set, Mode);
            if (Music_Index == 3)
                Add_Sound("99202684", Music_File, "ja", true, Time_Set, Mode);
            if (Music_Index == 4)
                Add_Sound("493356780", Music_File, "ja", true, Time_Set, Mode);
            if (Music_Index == 5)
                Add_Sound("277287194", Music_File, "ja", true, Time_Set, Mode);
            if (Music_Index == 6)
                Add_Sound("321403539", Music_File, "ja", true, Time_Set, Mode);
            if (Music_Index == 7)
                Add_Sound("603412881", Music_File, "ja", true, Time_Set, Mode);
            if (Music_Index == 8)
                Add_Sound("256533957", Music_File, "ja", true, Time_Set, Mode);
            if (Music_Index == 9)
                Add_Sound("520751345", Music_File, "ja", true, Time_Set, Mode);
            if (Music_Index == 10)
                Add_Sound("307041675", Music_File, "ja", true, Time_Set, Mode);
            if (Music_Index == 11)
                Add_Sound("960016609", Music_File, "ja", true, Time_Set, Mode);
            if (Music_Index == 12)
                Add_Sound("404033224", Music_File, "ja", true, Time_Set, Mode);
            if (Music_Index == 13)
                Add_Sound("797792182", Music_File, "ja", true, Time_Set, Mode);
            if (Music_Index == 14)
                Add_Sound("434309394", Music_File, "ja", true, Time_Set, Mode);
            if (Music_Index == 15)
                Add_Sound("868083406", Music_File, "ja", true, Time_Set, Mode);
        }
        //音声をプロジェクトに追加
        public void Sound_Add_Wwise(string Dir_Name, bool IsWoT_Project = false, bool IsNotIncludeBGM = false)
        {
            string[] Voice_Files_01 = Directory.GetFiles(Dir_Name, "*.wav", SearchOption.TopDirectoryOnly);
            foreach (string Voice_Now in Voice_Files_01)
            {
                string Name_Only;
                if (Path.GetFileNameWithoutExtension(Voice_Now).Contains("reload"))
                {
                    Name_Only = Voice_Mod_Create.Get_Voice_Type_V2(Voice_Now);
                }
                else
                {
                    Name_Only = Voice_Mod_Create.Get_Voice_Type_V1(Voice_Now);
                }
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
                    switch (Name_Only)
                    {
                        case "mikata":
                            Add_Sound("170029050", Voice_Now, "ja");
                            break;
                        case "danyaku":
                            Add_Sound("95559763", Voice_Now, "ja");
                            break;
                        case "hikantuu":
                            Add_Sound("766083947", Voice_Now, "ja");
                            break;
                        case "kantuu":
                            Add_Sound("569784404", Voice_Now, "ja");
                            break;
                        case "tokusyu":
                            Add_Sound("266422868", Voice_Now, "ja");
                            break;
                        case "tyoudan":
                            Add_Sound("1052258113", Voice_Now, "ja");
                            break;
                        case "syatyou":
                            Add_Sound("242302464", Voice_Now, "ja");
                            break;
                        case "souzyuusyu":
                            Add_Sound("334837201", Voice_Now, "ja");
                            break;
                        case "tekikasai":
                            Add_Sound("381780774", Voice_Now, "ja");
                            break;
                        case "gekiha":
                            Add_Sound("489572734", Voice_Now, "ja");
                            break;
                        case "enjinhason":
                            Add_Sound("210078142", Voice_Now, "ja");
                            break;
                        case "enjintaiha":
                            Add_Sound("249535989", Voice_Now, "ja");
                            break;
                        case "enjinhukkyuu":
                            Add_Sound("908710042", Voice_Now, "ja");
                            break;
                        case "kasai":
                            Add_Sound("1057023960", Voice_Now, "ja");
                            break;
                        case "syouka":
                            Add_Sound("953778289", Voice_Now, "ja");
                            break;
                        case "nenryou":
                            Add_Sound("121897540", Voice_Now, "ja");
                            break;
                        case "housinhason":
                            Add_Sound("127877647", Voice_Now, "ja");
                            break;
                        case "housintaiha":
                            Add_Sound("462397017", Voice_Now, "ja");
                            break;
                        case "housinhukkyuu":
                            Add_Sound("651656679", Voice_Now, "ja");
                            break;
                        case "housyu":
                            Add_Sound("739086111", Voice_Now, "ja");
                            break;
                        case "soutensyu":
                            Add_Sound("363753108", Voice_Now, "ja");
                            break;
                        case "musen":
                            Add_Sound("91697210", Voice_Now, "ja");
                            break;
                        case "musensyu":
                            Add_Sound("987172940", Voice_Now, "ja");
                            break;
                        case "battle":
                            Add_Sound("518589126", Voice_Now, "ja");
                            break;
                        case "kansokuhason":
                            Add_Sound("330491031", Voice_Now, "ja");
                            break;
                        case "kansokutaiha":
                            Add_Sound("792301846", Voice_Now, "ja");
                            break;
                        case "kansokuhukkyuu":
                            Add_Sound("539730785", Voice_Now, "ja");
                            break;
                        case "ritaihason":
                            Add_Sound("38261315", Voice_Now, "ja");
                            break;
                        case "ritaitaiha":
                            Add_Sound("37535832", Voice_Now, "ja");
                            break;
                        case "ritaihukkyuu":
                            Add_Sound("558576963", Voice_Now, "ja");
                            break;
                        case "houtouhason":
                            Add_Sound("1014565012", Voice_Now, "ja");
                            break;
                        case "houtoutaiha":
                            Add_Sound("135817430", Voice_Now, "ja");
                            break;
                        case "houtouhukkyuu":
                            Add_Sound("985679417", Voice_Now, "ja");
                            break;
                        case "taiha":
                            Add_Sound("164671745", Voice_Now, "ja");
                            break;
                        case "hakken":
                            Add_Sound("447063394", Voice_Now, "ja");
                            break;
                        case "lamp":
                            Add_Sound("154835998", Voice_Now, "ja");
                            break;
                        case "ryoukai":
                            Add_Sound("607694618", Voice_Now, "ja");
                            break;
                        case "kyohi":
                            Add_Sound("391276124", Voice_Now, "ja");
                            break;
                        case "help":
                            Add_Sound("840378218", Voice_Now, "ja");
                            break;
                        case "attack":
                            Add_Sound("549968154", Voice_Now, "ja");
                            break;
                        case "attack_now":
                            Add_Sound("1015337424", Voice_Now, "ja");
                            break;
                        case "capture":
                            Add_Sound("271044645", Voice_Now, "ja");
                            break;
                        case "defence":
                            Add_Sound("310153012", Voice_Now, "ja");
                            break;
                        case "keep":
                            Add_Sound("379548034", Voice_Now, "ja");
                            break;
                        case "lock":
                            Add_Sound("839607605", Voice_Now, "ja");
                            break;
                        case "unlock":
                            Add_Sound("233444430", Voice_Now, "ja");
                            break;
                        case "reload":
                            Add_Sound("299739777", Voice_Now, "ja");
                            break;
                        case "map":
                            Add_Sound("120795627", Voice_Now, "ja");
                            break;
                        case "battle_end":
                            Add_Sound("924876614", Voice_Now, "ja");
                            break;
                        case "battle_bgm":
                            if (!IsNotIncludeBGM)
                            {
                                Add_Sound("649358221", Voice_Now, "ja");
                            }
                            break;
                        case "load_bgm":
                            Add_Sound("915105627", Voice_Now, "ja");
                            break;
                    }
                }
            }
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
            {
                return;
            }
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
        {
            guidBytes.Add(byte.Parse(filtered.Substring(byteOrder[i] * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
        }
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
        {
            return hash;
        }
        else
        {
            return (hash >> 30) ^ (hash & mask);
        }
    }
}