using BNKManager;
using System;
using System.Collections.Generic;
using System.IO;

namespace WoTB_Voice_Mod_Creater.Wwise_Class
{
    //WoTBのサウンドファイルが.bnkだった場合
    public class Wwise_File_Extract_V2
    {
        WwiseBank WB;
        List<LoLSoundBank> LL = new List<LoLSoundBank>();
        LoLSoundBankManager LOL;
        List<LoLSoundBankManager.WEMFile> WEML;
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
            LOL.banksList.Clear();
            WEML.Clear();
            LL.Clear();
            IsClear = true;
            Selected_BNK_File = "";
        }
    }
}