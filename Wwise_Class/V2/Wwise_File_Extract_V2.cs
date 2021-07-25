using BNKManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using WoTB_Voice_Mod_Creater.Class;

namespace WoTB_Voice_Mod_Creater.Wwise_Class
{
    //WoTBのサウンドファイルが.bnkだった場合
    public class Wwise_File_Extract_V2
    {
        WwiseBank WB;
        List<LoLSoundBank> LL = new List<LoLSoundBank>();
        LoLSoundBankManager LOL = null;
        List<LoLSoundBankManager.WEMFile> WEML = null;
        public bool IsClear = false;
        public string Selected_BNK_File = "";
        public Wwise_File_Extract_V2(string Bank_File)
        {
            if (!File.Exists(Bank_File))
            {
                IsClear = true;
                return;
            }
            Selected_BNK_File = Bank_File;
            WB = new WwiseBank(Bank_File);
            LL.Add((LoLSoundBank)WB);
            LOL = new LoLSoundBankManager(LL);
            WEML = LOL.GetAudioFiles();
        }
        //.bnk内のすべてのファイル名を取得
        public List<string> Wwise_Get_Names()
        {
            if (IsClear)
            {
                return new List<string>();
            }
            List<string> Temp = new List<string>();
            foreach (LoLSoundBankManager.WEMFile Now in WEML)
            {
                Temp.Add(Now.ID.ToString());
            }
            return Temp;
        }
        //.bnk内に指定したファイル名が存在するか
        public bool Wwise_File_Exsist(string Name)
        {
            if (IsClear)
            {
                return false;
            }
            List<string> Files = Wwise_Get_Names();
            return Files.Contains(Name);
        }
        //.bnk内のファイル名を1つ取得
        public string Wwise_Get_Name(int Index)
        {
            if (WEML.Count <= Index || IsClear)
            {
                return "";
            }
            return WEML[Index].ID.ToString();
        }
        //.bnk内のファイル数を取得
        public int Wwise_Get_Numbers()
        {
            return WEML.Count;
        }
        //指定したIDのインデックスを取得(なければ-1)
        public int Wwise_Get_Index_By_Name(uint ID)
        {
            if (IsClear)
            {
                return -1;
            }
            for (int Number = 0; Number < WEML.Count; Number++)
            {
                if (ID == WEML[Number].ID)
                {
                    return Number;
                }
            }
            return -1;
        }
        public bool Wwise_Extract_To_WAV_Directory(string To_Dir)
        {
            if (IsClear)
            {
                return false;
            }
            if (!Directory.Exists(To_Dir))
            {
                return false;
            }
            try
            {
                Random r = new Random();
                for (int Number = 0; Number < WEML.Count; Number++)
                {
                    int Number_01 = r.Next(0, 1000);
                    if (Wwise_Extract_To_WEM_File(Number, To_Dir + "\\" + Number_01 + ".wem", true))
                        Sub_Code.WEM_To_File(To_Dir + "\\" + Number_01 + ".wem", To_Dir + "\\" + WEML[Number].ID + ".wav", "wav", true);
                }
                return true;
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                return false;
            }
        }
        //.bnkファイルの中身全てをフォルダに抽出
        //Name_Mode:1=連番,2=音声ID(数字数桁)
        public bool Wwise_Extract_To_WEM_Directory(string To_Dir, int Name_Mode)
        {
            if (IsClear)
            {
                return false;
            }
            List<string> Temp = new List<string>();
            return Wwise_Extract_To_WEM_Directory(To_Dir, Name_Mode, ref Temp);
        }
        public bool Wwise_Extract_To_WEM_Directory(string To_Dir, int Name_Mode, ref List<string> File_Names)
        {
            if (Name_Mode < 1 || Name_Mode > 2 || IsClear)
            {
                return false;
            }
            if (!Directory.Exists(To_Dir))
            {
                Directory.CreateDirectory(To_Dir);
            }
            try
            {
                File_Names.Clear();
                int Number = 0;
                foreach (LoLSoundBankManager.WEMFile Files in WEML)
                {
                    string Name;
                    if (Name_Mode == 1)
                    {
                        Name = Number.ToString();
                        Number++;
                    }
                    else
                    {
                        Name = Files.ID.ToString();
                    }
                    using (FileStream ms = new FileStream(To_Dir + "/" + Name + ".wem", FileMode.Create))
                    {
                        using (BinaryWriter bw = new BinaryWriter(ms))
                        {
                            bw.Write(LOL.GetFileData(Files.ID));
                        }
                    }
                    File_Names.Add(To_Dir + "\\" + Name + ".wem");
                }
                return true;
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                return false;
            }
        }
        //.bnkファイルの中身全てをフォルダに抽出(V2)
        public bool Wwise_Extract_To_WEM_Directory_V2(string To_Dir)
        {
            if (IsClear)
            {
                return false;
            }
            try
            {
                if (!Directory.Exists(To_Dir))
                {
                    Directory.CreateDirectory(To_Dir);
                }
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Wwise_Parse/BNK_Extract.bat");
                stw.WriteLine("chcp 65001");
                stw.Write(Voice_Set.Special_Path + "/Wwise_Parse/BNK_Extract.exe -a \"" + Selected_BNK_File + "\" -o \"" + To_Dir + "\" --oggs-only");
                stw.Close();
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = Voice_Set.Special_Path + "/Wwise_Parse/BNK_Extract.bat",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process p = Process.Start(processStartInfo);
                p.WaitForExit();
                File.Delete(Voice_Set.Special_Path + "/Wwise_Parse/BNK_Extract.bat");
                return true;
            }
            catch
            {
                return false;
            }
        }
        //.bnkファイルから.wemファイルを抽出(1つのみ)
        public bool Wwise_Extract_To_WEM_File(int Index, string To_File, bool IsOverWrite)
        {
            if (File.Exists(To_File) && !IsOverWrite)
            {
                return false;
            }
            if (WEML.Count <= Index || IsClear)
            {
                return false;
            }
            try
            {
                LoLSoundBankManager.WEMFile File_Index = WEML[Index];
                using (FileStream ms = new FileStream(To_File, FileMode.Create))
                {
                    using (BinaryWriter bw = new BinaryWriter(ms))
                    {
                        bw.Write(LOL.GetFileData(File_Index.ID));
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                return false;
            }
        }
        //.bnkファイルからwemをすべて抽出しoggへエンコード
        public bool Wwise_Extract_To_Ogg_Directory(string To_Dir, int Name_Mode, ref List<string> File_Names)
        {
            if (IsClear)
            {
                return false;
            }
            try
            {
                if (!Directory.Exists(To_Dir))
                {
                    Directory.CreateDirectory(To_Dir);
                }
                File_Names.Clear();
                List<string> Temp = new List<string>();
                if (Wwise_Extract_To_WEM_Directory(To_Dir, Name_Mode, ref Temp))
                {
                    foreach (string File_Now in Temp)
                    {
                        Sub_Code.WEM_To_File(File_Now, File_Now.Replace(".wem", ".ogg"), "ogg", true);
                        File_Names.Add(File_Now.Replace(".wem", ".ogg"));
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                return false;
            }
        }
        //↑が1つのみになったバージョン
        public bool Wwise_Extract_To_Ogg_File(int Index, string To_File, bool IsOverWrite)
        {
            if (IsClear)
            {
                return false;
            }
            if (File.Exists(To_File) && !IsOverWrite)
            {
                return false;
            }
            try
            {
                if (Wwise_Extract_To_WEM_File(Index, To_File + ".wem", true))
                {
                    if (Sub_Code.WEM_To_File(To_File + ".wem", To_File, "ogg", true))
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                return false;
            }
        }
        public bool Wwise_Extract_To_Ogg_File(uint ShortID, string To_File, bool IsOverWrite)
        {
            if (IsClear)
            {
                return false;
            }
            if (File.Exists(To_File) && !IsOverWrite)
            {
                return false;
            }
            try
            {
                int Index = -1;
                for (int Number = 0; Number < WEML.Count; Number++)
                {
                    if (WEML[Number].ID == ShortID)
                        Index = Number;
                }
                if (Index == -1)
                    return false;
                if (Wwise_Extract_To_WEM_File(Index, To_File + ".wem", true))
                {
                    if (Sub_Code.WEM_To_File(To_File + ".wem", To_File, "ogg", true))
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                return false;
            }
        }
        public bool Wwise_Extract_To_Wav_File(uint ShortID, string To_File, bool IsOverWrite)
        {
            if (IsClear)
            {
                return false;
            }
            if (File.Exists(To_File) && !IsOverWrite)
            {
                return false;
            }
            try
            {
                int Index = -1;
                for (int Number = 0; Number < WEML.Count; Number++)
                {
                    if (WEML[Number].ID == ShortID)
                        Index = Number;
                }
                if (Index == -1)
                    return false;
                if (Wwise_Extract_To_WEM_File(Index, To_File + ".wem", true))
                {
                    if (Sub_Code.WEM_To_File(To_File + ".wem", To_File, "wav", true))
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                return false;
            }
        }
        public void Set_Sound_Name(string SoundbanksInfo, string Dir)
        {
            if (!File.Exists(SoundbanksInfo) || !Directory.Exists(Dir) || IsClear)
                return;
            string[] Lines = File.ReadAllLines(SoundbanksInfo);
            for (int Number = 0; Number < Lines.Length; Number++)
            {
                if (Lines[Number].Contains("\"Id\": \"") && Lines[Number + 2].Contains("\"ShortName\": \""))
                {
                    string Temp_01 = Lines[Number].Substring(0, Lines[Number].LastIndexOf('\"'));
                    string Temp_02 = Temp_01.Substring(Temp_01.LastIndexOf('\"') + 1);
                    string Temp_03 = Lines[Number + 2].Substring(0, Lines[Number + 2].LastIndexOf('\"'));
                    string Temp_04 = Temp_03.Substring(Temp_03.LastIndexOf('\"') + 1);
                    if (Temp_04.Contains("\\"))
                        Temp_04 = Temp_04.Substring(Temp_04.LastIndexOf('\\') + 1);
                    string Temp_05 = Temp_04.Substring(0, Temp_04.LastIndexOf('.'));
                    Sub_Code.File_Move_V2(Dir + "\\" + Temp_02, Dir + "\\" + Temp_05, true);
                    Number += 2;
                }
            }
        }
        //.wemファイルを.bnkファイルに書き込む
        //LOL.Save()をしない場合ファイルには書き込まれず、メモリに情報を保存するだけになります。
        public bool Bank_Edit_Sound(int Index, string From_File, bool Save)
        {
            if (IsClear)
            {
                return false;
            }
            try
            {
                LOL.EditAudioFile(WEML[Index].ID, File.ReadAllBytes(From_File));
                if (Save)
                {
                    LOL.Save();
                }
                return true;
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                return false;
            }
        }
        //差し替えた.wemファイルを.bnkファイルに書き込む
        public bool Bank_Save()
        {
            if (IsClear)
            {
                return false;
            }
            try
            {
                LOL.Save();
                return true;
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                return false;
            }
        }
        public bool Bank_Save(string To_File)
        {
            if (IsClear)
            {
                return false;
            }
            try
            {
                LOL.Save(To_File);
                return true;
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                return false;
            }
        }
        //初期化
        public void Bank_Clear()
        {
            if (IsClear)
            {
                return;
            }
            if (LOL != null && LOL.banksList != null)
                LOL.banksList.Clear();
            if (WEML != null)
                WEML.Clear();
            LL.Clear();
            Selected_BNK_File = "";
            IsClear = true;
        }
    }
}