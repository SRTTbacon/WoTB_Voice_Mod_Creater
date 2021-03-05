using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

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
        //取得したデータから指定したイベントにサウンドを追加(Save()が呼ばれるまで保存しない)
        public bool Add_Sound(string Container_ShortID, string Audio_File, string Language)
        {
            if (!File.Exists(Audio_File) || Path.GetExtension(Audio_File) != ".wav" || Project_Dir == "")
            {
                return false;
            }
            try
            {
                if (Language == "SFX")
                {
                    File.Copy(Audio_File, Project_Dir + "/Originals/SFX/" + Path.GetFileName(Audio_File), true);
                    Add_Wav_Files.Add(Project_Dir + "/Originals/SFX/" + Path.GetFileName(Audio_File));
                }
                else
                {
                    File.Copy(Audio_File, Project_Dir + "/Originals/Voices/" + Language + "/" + Path.GetFileName(Audio_File), true);
                    Add_Wav_Files.Add(Project_Dir + "/Originals/Voices/" + Language + "/" + Path.GetFileName(Audio_File));
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
                    return List_Add_File(ReferenceListEnd_Line + 2, Path.GetFileName(Audio_File), Language);
                }
                else
                {
                    return List_Add_File(ChildrenList_Line + 1, Path.GetFileName(Audio_File), Language);
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
        public void Project_Build(string BankName, string OutputFilePath)
        {
            if (Project_Dir == "" || !Directory.Exists(Path.GetDirectoryName(OutputFilePath)))
            {
                return;
            }
            try
            {
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
                string GeneratedFile = Directory.GetFiles(Project_Dir + "/GeneratedSoundBanks/Windows", BankName + ".bnk", SearchOption.AllDirectories)[0];
                Sub_Code.File_Move(GeneratedFile, OutputFilePath, true);
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        //使わなくなったら必ず実行させる
        //一時ファイルを削除
        public void Clear()
        {
            try
            {
                foreach (string File_Now in Add_Wav_Files)
                {
                    Sub_Code.File_Delete_V2(File_Now);
                    Sub_Code.File_Delete_V2(File_Now.Replace(".wav", ".akd"));
                }
                string[] GetWEMFiles = Directory.GetFiles(Project_Dir + "/.cache/Windows", "*.wem", SearchOption.AllDirectories);
                foreach (string File_Now in GetWEMFiles)
                {
                    Sub_Code.File_Delete_V2(File_Now);
                }
                Actor_Mixer_Hierarchy.Clear();
                Add_Wav_Files.Clear();
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
        bool List_Add_File(int Line_Number, string FileName, string Language)
        {
            try
            {
                FileName = FileName.Replace(".wav", "");
                string Sound_GUID = Guid.NewGuid().ToString().ToUpper();
                string AudioFileSource_GUID = Guid.NewGuid().ToString().ToUpper();
                string Replace_Name = FileName.Replace(".", "_");
                uint Sound_ID = WwiseHash.HashGUID(Sound_GUID);
                uint AudioFileSource_ID = WwiseHash.HashGUID(AudioFileSource_GUID);
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
        public void Sound_Add_Wwise(string Dir_Name)
        {
            bool IsIncludeBGM = false;
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
                        Add_Sound("649358221", Voice_Now, "ja");
                        IsIncludeBGM = true;
                        break;
                    case "load_bgm":
                        Add_Sound("915105627", Voice_Now, "ja");
                        break;
                }
            }
            //BGMが含まれていなければ強制的に追加
            if (!IsIncludeBGM)
            {
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