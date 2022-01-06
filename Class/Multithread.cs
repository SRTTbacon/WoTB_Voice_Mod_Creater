using K4os.Compression.LZ4;
using NAudio.Lame;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Enc;
using Un4seen.Bass.Misc;

namespace WoTB_Voice_Mod_Creater.Class
{
    public class Multithread
    {
        static List<string> From_Files = new List<string>();
        static List<double> From_Gains = new List<double>();
        public static async Task Convert_OGG_To_Wav(string[] Files, bool IsFromFileDelete)
        {
            try
            {
                From_Files.Clear();
                From_Files.AddRange(Files);
                var tasks = new List<Task>();
                for (int i = 0; i < From_Files.Count; i++)
                    tasks.Add(OGG_To_WAV(i, Path.GetDirectoryName(Files[i]) + "\\" + Path.GetFileNameWithoutExtension(Files[i]) + ".wav", IsFromFileDelete));
                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
            From_Files.Clear();
        }
        //マルチスレッドで.mp3や.oggを.wav形式にエンコード
        //拡張子とファイル内容が異なっていた場合実行されない(ファイル拡張子が.mp3なのに実際は.oggだった場合など)
        public static async Task Convert_To_Wav(List<string> Files, List<string> ToFilePath, List<Music_Play_Time> Time = null, bool IsFromFileDelete = false)
        {
            try
            {
                From_Files.Clear();
                From_Files.AddRange(Files);
                var tasks = new List<Task>();
                for (int i = 0; i < From_Files.Count; i++)
                {
                    if (Time != null)
                        tasks.Add(To_WAV(i, ToFilePath[i], Time[i], IsFromFileDelete));
                    else
                        tasks.Add(To_WAV(i, ToFilePath[i], IsFromFileDelete, false, true));
                }
                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
            From_Files.Clear();
        }
        public static async Task Convert_To_Wav(string From_Dir, bool IsFromFileDelete, bool IsUseFFmpeg = false, bool BassEncode = false)
        {
            await Convert_To_Wav(From_Dir, From_Dir, IsFromFileDelete, IsUseFFmpeg, BassEncode);
        }
        public static async Task Convert_To_Wav(string From_Dir, string To_Dir, bool IsFromFileDelete, bool IsUseFFmpeg = false, bool BassEncode = false, bool NoWAVFileMode = true)
        {
            try
            {
                if (!Directory.Exists(To_Dir))
                    Directory.CreateDirectory(To_Dir);
                From_Files.Clear();
                string[] Ex;
                if (IsUseFFmpeg || !NoWAVFileMode)
                    Ex = new string[] { ".mp3", ".aac", ".ogg", ".flac", ".wma", ".wav" };
                else
                    Ex = new string[] { ".mp3", ".aac", ".ogg", ".flac", ".wma" };
                From_Files.AddRange(DirectoryEx.GetFiles(From_Dir, SearchOption.TopDirectoryOnly, Ex));
                var tasks = new List<Task>();
                for (int i = 0; i < From_Files.Count; i++)
                {
                    if (Sub_Code.Audio_IsWAV(From_Files[i]) && NoWAVFileMode)
                        continue;
                    tasks.Add(To_WAV(i, To_Dir, IsFromFileDelete, IsUseFFmpeg, BassEncode));
                }
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                Sub_Code.Error_Log_Write(ex.Message);
            }
            From_Files.Clear();
        }
        public static async Task Convert_To_Wav(string[] Files, string To_Dir, bool IsFromFileDelete, bool BassEncode = false)
        {
            try
            {
                From_Files.Clear();
                From_Files.AddRange(Files);
                var tasks = new List<Task>();
                for (int i = 0; i < From_Files.Count; i++)
                    if (!Sub_Code.Audio_IsWAV(Files[i]))
                        tasks.Add(To_WAV(i, To_Dir, IsFromFileDelete, false, BassEncode));
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                Sub_Code.Error_Log_Write(ex.Message);
            }
            From_Files.Clear();
        }
        public static async Task Convert_To_Wav(string FilePath, bool IsFromFileDelete, bool BassEncode)
        {
            try
            {
                From_Files.Clear();
                From_Files.Add(FilePath);
                var tasks = new List<Task>();
                tasks.Add(To_WAV(0, Path.GetDirectoryName(FilePath), IsFromFileDelete, false, BassEncode));
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                Sub_Code.Error_Log_Write(ex.Message);
            }
            From_Files.Clear();
        }
        public static async Task Convert_To_MP3(string[] Files, string To_Dir, bool IsFromFileDelete)
        {
            try
            {
                From_Files.Clear();
                From_Files.AddRange(Files);
                var tasks = new List<Task>();
                for (int i = 0; i < From_Files.Count; i++)
                    tasks.Add(To_MP3(i, To_Dir, IsFromFileDelete));
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                Sub_Code.Error_Log_Write(ex.Message);
            }
            From_Files.Clear();
        }
        public static async Task Convert_Ogg_To_Wav(List<string> From_Dir, bool IsFromFileDelete)
        {
            try
            {
                From_Files.Clear();
                From_Files.AddRange(From_Dir);
                var tasks = new List<Task>();
                for (int i = 0; i < From_Files.Count; i++)
                    tasks.Add(To_WAV(i, Path.GetDirectoryName(From_Files[i]), IsFromFileDelete, true, false));
                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        public static async Task Convert_WEM_To_Ogg(List<string> From_Dir, bool IsFromFileDelete)
        {
            try
            {
                From_Files.Clear();
                From_Files.AddRange(From_Dir);
                var tasks = new List<Task>();
                for (int i = 0; i < From_Files.Count; i++)
                    tasks.Add(To_OGG(i, Path.GetDirectoryName(From_Files[i]), IsFromFileDelete));
                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        public static async Task WAV_Gain(string[] Files, double[] Gain_Values)
        {
            try
            {
                From_Files.Clear();
                From_Files.AddRange(Files);
                From_Gains.Clear();
                From_Gains.AddRange(Gain_Values);
                var tasks = new List<Task>();
                for (int i = 0; i < From_Files.Count; i++)
                    tasks.Add(Gain(i));
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                Sub_Code.Error_Log_Write(ex.Message);
            }
            From_Files.Clear();
            From_Gains.Clear();
        }
        public static async Task WAV_Gain(string[] Files, double Gain_Values)
        {
            try
            {
                From_Files.Clear();
                From_Files.AddRange(Files);
                From_Gains.Clear();
                for (int i = 0; i < From_Files.Count; i++)
                    From_Gains.Add(Gain_Values);
                var tasks = new List<Task>();
                for (int i = 0; i < From_Files.Count; i++)
                    tasks.Add(Gain(i));
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                Sub_Code.Error_Log_Write(ex.Message);
            }
            From_Files.Clear();
            From_Gains.Clear();
        }
        public static async Task Convert_PSB_To_WAV(string[] Files, bool IsFromFileDelete = false)
        {
            From_Files.Clear();
            From_Files.AddRange(Files);
            var tasks = new List<Task>();
            for (int i = 0; i < From_Files.Count; i++)
                tasks.Add(PSB_To_WAV(i, IsFromFileDelete));
            await Task.WhenAll(tasks);
            From_Files.Clear();
        }
        public static async Task DVPL_Pack(List<string> Files, bool IsFromFileDelete)
        {
            From_Files.Clear();
            From_Files.AddRange(Files);
            var tasks = new List<Task>();
            for (int i = 0; i < From_Files.Count; i++)
                tasks.Add(To_DVPL_Pack(i, IsFromFileDelete));
            await Task.WhenAll(tasks);
            From_Files.Clear();
        }
        static async Task<bool> To_DVPL_Pack(int File_Number, bool IsFromFileDelete)
        {
            if (!File.Exists(From_Files[File_Number]))
                return false;
            bool IsOK = false;
            try
            {
                await Task.Run(() =>
                {
                    FileInfo file = new FileInfo(From_Files[File_Number]);
                    long size = file.Length;
                    if (size >= 50000000)
                        DVPL.DVPL_Pack_V2(From_Files[File_Number], From_Files[File_Number] + ".dvpl", IsFromFileDelete);
                    else
                    {
                        try
                        {
                            if (Path.GetExtension(From_Files[File_Number]) == ".tex")
                                DVPL.CREATE_DVPL(LZ4Level.L00_FAST, From_Files[File_Number], From_Files[File_Number] + ".dvpl", IsFromFileDelete);
                            else
                                DVPL.CREATE_DVPL(LZ4Level.L03_HC, From_Files[File_Number], From_Files[File_Number] + ".dvpl", IsFromFileDelete);
                            IsOK = true;
                        }
                        catch
                        {
                        }
                    }
                });
            }
            catch
            {
            }
            if (IsOK)
                return true;
            return false;
        }
        static async Task<bool> PSB_To_WAV(int File_Number, bool IsFromFileDelete)
        {
            if (!File.Exists(From_Files[File_Number]))
                return false;
            string GetDir = Path.GetDirectoryName(From_Files[File_Number]);
            string GetFileNameOnly = Path.GetFileNameWithoutExtension(From_Files[File_Number]);
            StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Encode_Mp3/PSB_To_WAV_" + File_Number + ".bat");
            stw.WriteLine("chcp 65001");
            stw.Write("\"D:/Downloads/SDA Downloads/XCI_NCA_NSP_v2/ncaDecrypted/FreeMoteToolkit/PsbDecompile.exe\" \"" + From_Files[File_Number] + "\"");
            stw.Close();
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = Voice_Set.Special_Path + "/Encode_Mp3/PSB_To_WAV_" + File_Number + ".bat",
                CreateNoWindow = true,
                WorkingDirectory = "D:/Downloads/SDA Downloads/XCI_NCA_NSP_v2/ncaDecrypted/FreeMoteToolkit",
                UseShellExecute = false
            };
            Process p = Process.Start(processStartInfo);
            await Task.Run(() =>
            {
                p.WaitForExit();
                if (File.Exists(GetDir + "\\" + GetFileNameOnly + "\\" + GetFileNameOnly + ".opus.wav"))
                {
                    Sub_Code.File_Move(GetDir + "\\" + GetFileNameOnly + "\\" + GetFileNameOnly + ".opus.wav", GetDir + "\\" + GetFileNameOnly + ".wav", true);
                    Directory.Delete(GetDir + "\\" + GetFileNameOnly, true);
                }
                if (File.Exists(GetDir + "\\" + GetFileNameOnly + ".wav") && IsFromFileDelete)
                    File.Delete(From_Files[File_Number]);
                File.Delete(GetDir + "\\" + GetFileNameOnly + ".json");
                File.Delete(GetDir + "\\" + GetFileNameOnly + ".resx.json");
                File.Delete(Voice_Set.Special_Path + "/Encode_Mp3/PSB_To_WAV_" + File_Number + ".bat");
            });
            return true;
        }
        static async Task<bool> To_MP3(int File_Number, string To_Dir, bool IsFromFileDelete)
        {
            if (!File.Exists(From_Files[File_Number]))
                return false;
            try
            {
                string To_Audio_File = To_Dir + "\\" + Path.GetFileNameWithoutExtension(From_Files[File_Number]) + ".mp3";
                await Task.Run(() =>
                {
                    string Ex = Path.GetExtension(From_Files[File_Number]);
                    if (Ex == ".ogg")
                        using (NAudio.Vorbis.VorbisWaveReader reader = new NAudio.Vorbis.VorbisWaveReader(From_Files[File_Number]))
                            using (LameMP3FileWriter wtr = new LameMP3FileWriter(To_Audio_File, reader.WaveFormat, 128))
                                reader.CopyTo(wtr);
                    else if (Ex == ".wav")
                        using (WaveFileReader reader = new WaveFileReader(From_Files[File_Number]))
                            using (LameMP3FileWriter wtr = new LameMP3FileWriter(To_Audio_File, reader.WaveFormat, 128))
                                reader.CopyTo(wtr);
                    else if (Ex == ".flac")
                        using (NAudio.Flac.FlacReader reader = new NAudio.Flac.FlacReader(From_Files[File_Number]))
                            using (LameMP3FileWriter wtr = new LameMP3FileWriter(To_Audio_File, reader.WaveFormat, 128))
                                reader.CopyTo(wtr);
                    else if (Ex == ".aac" || Ex == ".wma" || Ex == ".mp4" || Ex == ".webm")
                        using (MediaFoundationReader reader = new MediaFoundationReader(From_Files[File_Number]))
                            using (LameMP3FileWriter wtr = new LameMP3FileWriter(To_Audio_File, reader.WaveFormat, 128))
                                reader.CopyTo(wtr);
                    else
                    {
                        Un4seen.Bass.Misc.EncoderLAME mc = new Un4seen.Bass.Misc.EncoderLAME(0);
                        mc.EncoderDirectory = Voice_Set.Special_Path + "/Encode_Mp3";
                        mc.InputFile = From_Files[File_Number];
                        mc.OutputFile = To_Audio_File;
                        mc.LAME_Bitrate = (int)Un4seen.Bass.Misc.EncoderLAME.BITRATE.kbps_160;
                        mc.LAME_Mode = Un4seen.Bass.Misc.EncoderLAME.LAMEMode.Default;
                        mc.LAME_Quality = Un4seen.Bass.Misc.EncoderLAME.LAMEQuality.Q2;
                        Un4seen.Bass.Misc.BaseEncoder.EncodeFile(mc, null, true, false, true);
                    }
                    if (IsFromFileDelete)
                        File.Delete(From_Files[File_Number]);
                });
            }
            catch
            {
                return false;
            }
            return true;
        }
        static async Task OGG_To_WAV(int File_Number, string ToFilePath, bool IsFromFileDelete)
        {
            StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Other/Audio_WAV_Encode" + File_Number + ".bat");
            stw.WriteLine("chcp 65001");
            stw.Write("\"" + Voice_Set.Special_Path + "/Other/oggdec.exe\" -w \"" + ToFilePath + "\" \"" + From_Files[File_Number] + "\"");
            stw.Close();
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = Voice_Set.Special_Path + "/Other/Audio_WAV_Encode" + File_Number + ".bat",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process p = Process.Start(processStartInfo);
            await Task.Run(() =>
            {
                p.WaitForExit();
                if (IsFromFileDelete)
                    File.Delete(From_Files[File_Number]);
                File.Delete(Voice_Set.Special_Path + "/Other/Audio_WAV_Encode" + File_Number + ".bat");
            });
        }
        static async Task To_OGG(int File_Number, string To_Dir, bool IsFromFileDelete)
        {
            StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Encode_Mp3/Audio_OGG_Encode" + File_Number + ".bat");
            stw.WriteLine("chcp 65001");
            stw.Write("\"" + Voice_Set.Special_Path + "/Wwise/ww2ogg.exe\" --pcb packed_codebooks_aoTuV_603.bin -o \"" + To_Dir + "\\" + Path.GetFileNameWithoutExtension(From_Files[File_Number]) + ".ogg\" \"" + From_Files[File_Number] + "\"");
            stw.Close();
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = Voice_Set.Special_Path + "/Encode_Mp3/Audio_OGG_Encode" + File_Number + ".bat",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process p = Process.Start(processStartInfo);
            await Task.Run(() =>
            {
                p.WaitForExit();
                if (IsFromFileDelete)
                    File.Delete(From_Files[File_Number]);
                File.Delete(Voice_Set.Special_Path + "/Encode_Mp3/Audio_OGG_Encode" + File_Number + ".bat");
            });
        }
        static async Task To_WAV(int File_Number, string ToFilePath, Music_Play_Time Time, bool IsFromFileDelete)
        {
            double End = Time.End_Time - Time.Start_Time;
            StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Encode_Mp3/Audio_WAV_Encode" + File_Number + ".bat");
            stw.WriteLine("chcp 65001");
            stw.Write("\"" + Voice_Set.Special_Path + "/Encode_Mp3/ffmpeg.exe\" -y -i \"" + From_Files[File_Number] + "\" -vn -ac 2 -ar 44100 -acodec pcm_s24le -f wav -ss " + Time.Start_Time + " -t " + End + " \"" + ToFilePath + "\"");
            stw.Close();
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = Voice_Set.Special_Path + "/Encode_Mp3/Audio_WAV_Encode" + File_Number + ".bat",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process p = Process.Start(processStartInfo);
            await Task.Run(() =>
            {
                p.WaitForExit();
                if (IsFromFileDelete)
                    File.Delete(From_Files[File_Number]);
                File.Delete(Voice_Set.Special_Path + "/Encode_Mp3/Audio_WAV_Encode" + File_Number + ".bat");
            });
        }
        static async Task<bool> To_WAV(int File_Number, string To_Dir, bool IsFromFileDelete, bool IsUseFFmpeg, bool IsUseBass)
        {
            if (!File.Exists(From_Files[File_Number]))
                return false;
            if (IsUseFFmpeg)
            {
                if (IsUseBass)
                {
                    string To_Audio_File = To_Dir + "\\" + Path.GetFileNameWithoutExtension(From_Files[File_Number]) + ".wav";
                    Un4seen.Bass.Misc.EncoderWAV w = new Un4seen.Bass.Misc.EncoderWAV(0);
                    w.InputFile = From_Files[File_Number];
                    w.OutputFile = To_Audio_File;
                    w.WAV_BitsPerSample = 24;
                    w.Start(null, IntPtr.Zero, false);
                    w.Stop();
                    if (IsFromFileDelete)
                        File.Delete(From_Files[File_Number]);
                }
                else
                {
                    string Encode_Style = "-y -vn -ac 2 -ar 44100 -acodec pcm_s24le -f wav";
                    StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Encode_Mp3/Audio_Encode" + File_Number + ".bat");
                    stw.WriteLine("chcp 65001");
                    stw.Write("\"" + Voice_Set.Special_Path + "/Encode_Mp3/ffmpeg.exe\" -i \"" + From_Files[File_Number] + "\" " + Encode_Style + " \"" + To_Dir + "\\" +
                              Path.GetFileNameWithoutExtension(From_Files[File_Number]) + ".wav\"");
                    stw.Close();
                    ProcessStartInfo processStartInfo = new ProcessStartInfo
                    {
                        FileName = Voice_Set.Special_Path + "/Encode_Mp3/Audio_Encode" + File_Number + ".bat",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    Process p = Process.Start(processStartInfo);
                    await Task.Run(() =>
                    {
                        p.WaitForExit();
                        if (IsFromFileDelete)
                            File.Delete(From_Files[File_Number]);
                        File.Delete(Voice_Set.Special_Path + "/Encode_Mp3/Audio_Encode" + File_Number + ".bat");
                    });
                }
            }
            else if (IsUseBass)
            {
                string To_Audio_File = To_Dir + "\\" + Path.GetFileNameWithoutExtension(From_Files[File_Number]) + ".wav";
                Un4seen.Bass.Misc.EncoderWAV w = new Un4seen.Bass.Misc.EncoderWAV(0);
                w.InputFile = From_Files[File_Number];
                w.OutputFile = To_Audio_File;
                w.WAV_BitsPerSample = 24;
                w.Start(null, IntPtr.Zero, false);
                w.Stop();
                if (IsFromFileDelete)
                    File.Delete(From_Files[File_Number]);
            }
            else
            {
                int Number = 0;
                string To_Audio_File = To_Dir + "\\" + Path.GetFileNameWithoutExtension(From_Files[File_Number]) + ".wav";
                await Task.Run(() =>
                {
                    start:
                    try
                    {
                        string Ex = Path.GetExtension(From_Files[File_Number]);
                        if (Ex == ".ogg")
                            using (NAudio.Vorbis.VorbisWaveReader reader = new NAudio.Vorbis.VorbisWaveReader(From_Files[File_Number]))
                                using (WaveStream pcmStream = WaveFormatConversionStream.CreatePcmStream(reader))
                                    WaveFileWriter.CreateWaveFile(To_Audio_File, pcmStream);
                        else if (Ex == ".mp3")
                            using (NAudio.Wave.Mp3FileReader reader = new NAudio.Wave.Mp3FileReader(From_Files[File_Number]))
                                using (WaveStream pcmStream = WaveFormatConversionStream.CreatePcmStream(reader))
                                    WaveFileWriter.CreateWaveFile(To_Audio_File, pcmStream);
                        else if (Ex == ".flac")
                            using (NAudio.Flac.FlacReader reader = new NAudio.Flac.FlacReader(From_Files[File_Number]))
                                using (WaveStream pcmStream = WaveFormatConversionStream.CreatePcmStream(reader))
                                    WaveFileWriter.CreateWaveFile(To_Audio_File, pcmStream);
                        else if (Ex == ".aac" || Ex == ".wma" || Ex == ".mp4")
                            using (MediaFoundationReader reader = new MediaFoundationReader(From_Files[File_Number]))
                                WaveFileWriter.CreateWaveFile(To_Audio_File, reader);
                        else if (Ex == ".wav")
                            using (WaveFileReader reader = new WaveFileReader(From_Files[File_Number]))
                                using (WaveStream pcmStream = WaveFormatConversionStream.CreatePcmStream(reader))
                                    WaveFileWriter.CreateWaveFile(To_Audio_File, pcmStream);
                        if (IsFromFileDelete)
                            File.Delete(From_Files[File_Number]);
                    }
                    catch (Exception e)
                    {
                        if (Number < 4)
                        {
                            Number++;
                            //goto文...許してください...
                            goto start;
                        }
                        else
                            Sub_Code.Error_Log_Write(e.Message);
                    }
                });
            }
            return true;
        }
        static async Task<bool> Gain(int Count)
        {
            try
            {
                if (From_Gains[Count] <= -20)
                    From_Gains[Count] = -19.9;
                else if (From_Gains[Count] >= 12)
                    From_Gains[Count] = 11.9;
                int Number = Sub_Code.r.Next(0, 10000);
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Other/WAV_Set_Gain_" + Number + ".bat");
                stw.WriteLine("chcp 65001");
                stw.Write("\"" + Voice_Set.Special_Path + "/Other/WaveGain.exe\" -r -y -n -g " + From_Gains[Count] + " \"" + From_Files[Count] + "\"");
                stw.Close();
                ProcessStartInfo processStartInfo1 = new ProcessStartInfo
                {
                    FileName = Voice_Set.Special_Path + "/Other/WAV_Set_Gain_" + Number + ".bat",
                    WorkingDirectory = Voice_Set.Special_Path + "\\Other",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process p = Process.Start(processStartInfo1);
                await Task.Run(() =>
                {
                    p.WaitForExit();
                    File.Delete(Voice_Set.Special_Path + "/Other/WAV_Set_Gain_" + Number + ".bat");
                });
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}