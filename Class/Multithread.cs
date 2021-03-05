using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace WoTB_Voice_Mod_Creater.Class
{
    public class Multithread
    {
        static List<string> From_Files = new List<string>();
        //マルチスレッドで.mp3や.oggを.wav形式にエンコード
        //拡張子とファイル内容が異なっていた場合実行されない(ファイル拡張子が.mp3なのに実際は.oggだった場合など)
        public static async Task Convert_To_Wav(string From_Dir, bool IsFromFileDelete)
        {
            await Convert_To_Wav(From_Dir, From_Dir, IsFromFileDelete, false);
        }
        public static async Task Convert_To_Wav(string From_Dir, string To_Dir, bool IsFromFileDelete, bool IsUseFFmpeg = false)
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
                    Ex = new string[]{ ".mp3", ".aac", ".ogg", ".flac", ".wma", ".wav" };
                }
                else
                {
                    Ex = new string[]{ ".mp3", ".aac", ".ogg", ".flac", ".wma" };
                }
                From_Files.AddRange(DirectoryEx.GetFiles(From_Dir, SearchOption.TopDirectoryOnly, Ex));
                var tasks = new List<Task>();
                for (int i = 0; i < From_Files.Count; i++)
                {
                    tasks.Add(To_WAV(i, To_Dir, IsFromFileDelete, IsUseFFmpeg));
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
        static async Task<bool> To_WAV(int File_Number, string To_Dir, bool IsFromFileDelete, bool IsUseFFmpeg)
        {
            if (!File.Exists(From_Files[File_Number]))
            {
                return false;
            }
            if (IsUseFFmpeg)
            {
                string Encode_Style = "-y -vn -ac 2 -ar 44100 -acodec pcm_s24le -f wav";
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Encode_Mp3/Audio_Encode" + File_Number + ".bat");
                stw.WriteLine("chcp 65001");
                stw.Write(Voice_Set.Special_Path + "/Encode_Mp3/ffmpeg.exe -i \"" + From_Files[File_Number] + "\" " + Encode_Style + " \"" + To_Dir + "\\" +
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