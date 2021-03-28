using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using WEMSharp;

namespace WoTB_Voice_Mod_Creater.Wwise_Class
{
    //WoTBのサウンドファイルが.pckだった場合
    public class Wwise_File_Extract_V1
    {
        public string Selected_PCK_File = "";
        bool IsPCKSelected = false;
        Wwise_Pack Wwise;
        List<Wwise_Pack.soundFile> Sounds = new List<Wwise_Pack.soundFile>();
        public Wwise_File_Extract_V1(string PCK_File)
        {
            if (!File.Exists(PCK_File))
            {
                return;
            }
            this.Selected_PCK_File = PCK_File;
            Wwise = new Wwise_Pack(PCK_File);
            Sounds = Wwise.GetWEMFileList();
            IsPCKSelected = true;
        }
        public void Pck_Clear()
        {
            Selected_PCK_File = "";
            IsPCKSelected = false;
            Wwise = null;
        }
        //.pckファイルの中身全てをフォルダに抽出
        public bool Wwise_Extract_To_Directory(string To_Dir)
        {
            if (!IsPCKSelected)
            {
                return false;
            }
            try
            {
                if (!Directory.Exists(To_Dir))
                {
                    Directory.CreateDirectory(To_Dir);
                }
                Wwise.ExtractPack(To_Dir);
                return true;
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                return false;
            }
        }
        //.pckファイルの内容を取得
        public List<string> Wwise_Get_Banks_ID()
        {
            List<string> IDs = new List<string>();
            if (!IsPCKSelected)
            {
                return IDs;
            }
            foreach (Wwise_Pack.soundFile Sound_Now in Sounds)
            {
                IDs.Add(Sound_Now.id.ToString());
            }
            return IDs;
        }
        public int Wwise_Get_File_Count()
        {
            return Sounds.Count;
        }
        //.bnkファイルから.wemファイルを抽出(1つのみ)
        public bool Wwise_Extract_To_WEM_File(int Index, string To_File, bool IsOverWrite)
        {
            if (File.Exists(To_File) && !IsOverWrite)
            {
                return false;
            }
            if (Sounds.Count <= Index || !IsPCKSelected)
            {
                return false;
            }
            try
            {
                Wwise.ExtractFileIndex(Index, To_File);
                return true;
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                return false;
            }
        }
        //.pckファイルから.wemファイルを抽出し、oggに変換(1つのみ)
        public bool Wwise_Extract_To_Ogg_File(int Index, string To_File, bool IsOverWrite)
        {
            if (Sounds.Count <= Index || !IsPCKSelected)
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
        public async Task Async_Wwise_Extract_To_WEM_Directory(string To_Dir)
        {
            if (!IsPCKSelected)
            {
                return;
            }
            try
            {
                var tasks = new List<Task>();
                for (int i = 0; i < Sounds.Count; i++)
                {
                    tasks.Add(Async_Wwise_Extract_To_WEM_File(i, To_Dir));
                }
                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        public async Task Async_Wwise_Extract_To_OGG_Directory(string To_Dir)
        {
            if (!IsPCKSelected)
            {
                return;
            }
            try
            {
                var tasks = new List<Task>();
                for (int i = 0; i < Sounds.Count; i++)
                {
                    tasks.Add(Async_Wwise_Extract_To_Ogg_File(i, To_Dir));
                }
                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        async Task Async_Wwise_Extract_To_WEM_File(int Index, string To_Dir)
        {
            try
            {
                await Task.Run(() =>
                {
                    Wwise.ExtractFileIndex(Index, To_Dir + "/" + Sounds[Index].id + ".wem");
                });
            }
            catch
            {

            }
        }
        async Task Async_Wwise_Extract_To_Ogg_File(int Index, string To_Dir)
        {
            await Task.Run(() =>
            {
                try
                {
                    WEMFile wem = new WEMFile(To_Dir + "/" + Sounds[Index].id + ".wem", WEMForcePacketFormat.NoForcePacketFormat);
                    wem.GenerateOGG(To_Dir + "/" + Sounds[Index].id + ".ogg", Voice_Set.Special_Path + "/Wwise/packed_codebooks_aoTuV_603.bin", false, false);
                    wem.Close();
                }
                catch
                {

                }
            });
        }
        //wwiseutil.exeから指定したディレクトリにあるwemファイルをpckファイルに書き換える
        //引数:保存先(元ファイルと同じでもOK),wemファイルがあるフォルダ,既にファイルがある場合上書きするか
        public void Wwise_PCK_Save(string To_File, string Set_Dir, bool IsOverWrite)
        {
            if (!Directory.Exists(Set_Dir) || !File.Exists(Selected_PCK_File) || !IsPCKSelected)
            {
                return;
            }
            if (File.Exists(To_File) && !IsOverWrite)
            {
                return;
            }
            try
            {
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Wwise/Wwise_PCK_Repack.bat");
                stw.WriteLine("chcp 65001");
                if (Selected_PCK_File == To_File)
                {
                    stw.Write("\"" + Voice_Set.Special_Path + "/Wwise/wwiseutil.exe\" -f \"" + Selected_PCK_File + "\" -r -t \"" + Set_Dir + "\"");
                }
                else
                {
                    stw.Write("\"" + Voice_Set.Special_Path + "/Wwise/wwiseutil.exe\" -f \"" + Selected_PCK_File + "\" -o \"" + To_File + "\" -r -t \"" + Set_Dir + "\"");
                }
                stw.Close();
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = Voice_Set.Special_Path + "/Wwise/Wwise_PCK_Repack.bat",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process p = Process.Start(processStartInfo);
                p.WaitForExit();
                File.Delete(Voice_Set.Special_Path + "/Wwise/Wwise_PCK_Repack.bat");
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        public void Wwise_PCK_Save(string Set_Dir)
        {
            if (!IsPCKSelected)
            {
                return;
            }
            Wwise_PCK_Save(Selected_PCK_File, Set_Dir, true);
        }
    }
}