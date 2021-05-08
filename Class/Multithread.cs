using NAudio.Lame;
using NAudio.Wave;
using SimpleTCP;
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
        //マルチスレッドで.mp3や.oggを.wav形式にエンコード
        //拡張子とファイル内容が異なっていた場合実行されない(ファイル拡張子が.mp3なのに実際は.oggだった場合など)
        public static async Task Convert_To_Wav(List<string> Files, List<string> ToFilePath, List<Music_Play_Time> Time, bool IsFromFileDelete)
        {
            try
            {
                From_Files.Clear();
                From_Files.AddRange(Files);
                var tasks = new List<Task>();
                for (int i = 0; i < From_Files.Count; i++)
                {
                    tasks.Add(To_WAV(i, ToFilePath[i], Time[i], IsFromFileDelete));
                }
                await Task.WhenAll(tasks);
                From_Files.Clear();
            }
            catch (Exception e)
            {
                From_Files.Clear();
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        public static async Task Convert_To_Wav(string From_Dir, bool IsFromFileDelete, bool IsUseFFmpeg = false, bool BassEncode = false)
        {
            await Convert_To_Wav(From_Dir, From_Dir, IsFromFileDelete, IsUseFFmpeg, BassEncode);
        }
        public static async Task Convert_To_Wav(string From_Dir, string To_Dir, bool IsFromFileDelete, bool IsUseFFmpeg = false, bool BassEncode = false)
        {
            try
            {
                if (!Directory.Exists(To_Dir))
                {
                    Directory.CreateDirectory(To_Dir);
                }
                From_Files.Clear();
                string[] Ex;
                if (IsUseFFmpeg)
                {
                    Ex = new string[] { ".mp3", ".aac", ".ogg", ".flac", ".wma", ".wav" };
                }
                else
                {
                    Ex = new string[] { ".mp3", ".aac", ".ogg", ".flac", ".wma" };
                }
                From_Files.AddRange(DirectoryEx.GetFiles(From_Dir, SearchOption.TopDirectoryOnly, Ex));
                var tasks = new List<Task>();
                for (int i = 0; i < From_Files.Count; i++)
                {
                    tasks.Add(To_WAV(i, To_Dir, IsFromFileDelete, IsUseFFmpeg, BassEncode));
                }
                await Task.WhenAll(tasks);
                From_Files.Clear();
            }
            catch (Exception ex)
            {
                From_Files.Clear();
                Sub_Code.Error_Log_Write(ex.Message);
            }
        }
        public static async Task Convert_To_Wav(string[] Files, string To_Dir, bool IsFromFileDelete, bool BassEncode = false)
        {
            try
            {
                From_Files.Clear();
                From_Files.AddRange(Files);
                var tasks = new List<Task>();
                for (int i = 0; i < From_Files.Count; i++)
                {
                    if (!Sub_Code.Audio_IsWAV(Files[i]))
                    {
                        tasks.Add(To_WAV(i, To_Dir, IsFromFileDelete, false, BassEncode));
                    }
                }
                await Task.WhenAll(tasks);
                From_Files.Clear();
            }
            catch (Exception ex)
            {
                From_Files.Clear();
                Sub_Code.Error_Log_Write(ex.Message);
            }
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
                From_Files.Clear();
            }
            catch (Exception ex)
            {
                From_Files.Clear();
                Sub_Code.Error_Log_Write(ex.Message);
            }
        }
        public static async Task Convert_To_MP3(string[] Files, string To_Dir, bool IsFromFileDelete)
        {
            try
            {
                From_Files.Clear();
                From_Files.AddRange(Files);
                var tasks = new List<Task>();
                for (int i = 0; i < From_Files.Count; i++)
                {
                    tasks.Add(To_MP3(i, To_Dir, IsFromFileDelete));
                }
                await Task.WhenAll(tasks);
                From_Files.Clear();
            }
            catch (Exception ex)
            {
                From_Files.Clear();
                Sub_Code.Error_Log_Write(ex.Message);
            }
        }
        static async Task<bool> To_MP3(int File_Number, string To_Dir, bool IsFromFileDelete)
        {
            if (!File.Exists(From_Files[File_Number]))
            {
                return false;
            }
            try
            {
                string To_Audio_File = To_Dir + "\\" + Path.GetFileNameWithoutExtension(From_Files[File_Number]) + ".mp3";
                await Task.Run(() =>
                {
                    string Ex = Path.GetExtension(From_Files[File_Number]);
                    if (Ex == ".ogg")
                    {
                        using (NAudio.Vorbis.VorbisWaveReader reader = new NAudio.Vorbis.VorbisWaveReader(From_Files[File_Number]))
                        {
                            using (LameMP3FileWriter wtr = new LameMP3FileWriter(To_Audio_File, reader.WaveFormat, 128))
                            {
                                reader.CopyTo(wtr);
                            }
                        }
                    }
                    else if (Ex == ".wav")
                    {
                        using (WaveFileReader reader = new WaveFileReader(From_Files[File_Number]))
                        {
                            using (LameMP3FileWriter wtr = new LameMP3FileWriter(To_Audio_File, reader.WaveFormat, 128))
                            {
                                reader.CopyTo(wtr);
                            }
                        }
                    }
                    if (IsFromFileDelete)
                    {
                        File.Delete(From_Files[File_Number]);
                    }
                });
            }
            catch
            {
                return false;
            }
            return true;
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
                {
                    File.Delete(From_Files[File_Number]);
                }
                File.Delete(Voice_Set.Special_Path + "/Encode_Mp3/Audio_WAV_Encode" + File_Number + ".bat");
            });
        }
        static async Task<bool> To_WAV(int File_Number, string To_Dir, bool IsFromFileDelete, bool IsUseFFmpeg, bool IsUseBass)
        {
            if (!File.Exists(From_Files[File_Number]))
            {
                return false;
            }
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
                    {
                        File.Delete(From_Files[File_Number]);
                    }
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
                        {
                            File.Delete(From_Files[File_Number]);
                        }
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
                {
                    File.Delete(From_Files[File_Number]);
                }
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
                        {
                            using (NAudio.Vorbis.VorbisWaveReader reader = new NAudio.Vorbis.VorbisWaveReader(From_Files[File_Number]))
                            {
                                using (WaveStream pcmStream = WaveFormatConversionStream.CreatePcmStream(reader))
                                {
                                    WaveFileWriter.CreateWaveFile(To_Audio_File, pcmStream);
                                }
                            }
                        }
                        else if (Ex == ".mp3")
                        {
                            using (NAudio.Wave.Mp3FileReader reader = new NAudio.Wave.Mp3FileReader(From_Files[File_Number]))
                            {
                                using (WaveStream pcmStream = WaveFormatConversionStream.CreatePcmStream(reader))
                                {
                                    WaveFileWriter.CreateWaveFile(To_Audio_File, pcmStream);
                                }
                            }
                        }
                        else if (Ex == ".flac")
                        {
                            using (NAudio.Flac.FlacReader reader = new NAudio.Flac.FlacReader(From_Files[File_Number]))
                            {
                                using (WaveStream pcmStream = WaveFormatConversionStream.CreatePcmStream(reader))
                                {
                                    WaveFileWriter.CreateWaveFile(To_Audio_File, pcmStream);
                                }
                            }
                        }
                        else if (Ex == ".aac" || Ex == ".wma" || Ex == ".mp4")
                        {
                            using (MediaFoundationReader reader = new MediaFoundationReader(From_Files[File_Number]))
                            {
                                WaveFileWriter.CreateWaveFile(To_Audio_File, reader);
                            }
                        }
                        else if (Ex == ".wav")
                        {
                            using (WaveFileReader reader = new WaveFileReader(From_Files[File_Number]))
                            {
                                using (WaveStream pcmStream = WaveFormatConversionStream.CreatePcmStream(reader))
                                {
                                    WaveFileWriter.CreateWaveFile(To_Audio_File, pcmStream);
                                }
                            }
                        }
                        if (IsFromFileDelete)
                        {
                            File.Delete(From_Files[File_Number]);
                        }
                    }
                    catch (Exception e)
                    {
                        if (Number < 4)
                        {
                            Number++;
                            goto start;
                        }
                        else
                        {
                            Sub_Code.Error_Log_Write(e.Message);
                        }
                    }
                });
            }
            return true;
        }
    }
}