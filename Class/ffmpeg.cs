using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using Un4seen.Bass.AddOn.Mix;
using Un4seen.Bass.AddOn.Tags;
using Un4seen.Bass.Misc;

namespace WoTB_Voice_Mod_Creater.Class
{
    //ffmpeg.exeを利用していろいろできるようにするクラス
    public class ffmpeg
    {
        //指定範囲にカット
        public static bool Sound_Cut_From_To(string From_File, string To_File, double From_Time, double To_Time)
        {
            if (!File.Exists(From_File))
                return false;
            string To_Temp_File = Path.GetDirectoryName(From_File) + "\\" + Path.GetFileName(From_File) + "_Temp" + Path.GetExtension(From_File);
            StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Encode_Mp3/Audio_Cut.bat");
            stw.WriteLine("chcp 65001");
            if (Path.GetExtension(From_File) == ".mp3")
                stw.Write("ffmpeg.exe -y -ss " + From_Time + " -to " + To_Time + " -i \"" + From_File + "\" -c copy \"" + To_Temp_File + "\"");
            else
                stw.Write("ffmpeg.exe -y -ss " + From_Time + " -to " + To_Time + " -i \"" + From_File + "\" \"" + To_Temp_File + "\"");
            stw.Close();
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = Voice_Set.Special_Path + "/Encode_Mp3/Audio_Cut.bat",
                WorkingDirectory = Voice_Set.Special_Path + "/Encode_Mp3",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process p = Process.Start(processStartInfo);
            p.WaitForExit();
            File.Delete(Voice_Set.Special_Path + "/Encode_Mp3/Audio_Cut.bat");
            if (Sub_Code.File_Move(To_Temp_File, To_File, true))
                return true;
            else 
                return false;
        }
        //遅延と音量調整と速度(-c copyが使用できないため、時間短縮を兼ねて同時に処理します)
        public static bool Sound_Volume(string From_File, string To_File, double Volume)
        {
            if (!File.Exists(From_File))
                return false;
            string To_Temp_File = Path.GetDirectoryName(From_File) + "\\" + Path.GetFileName(From_File) + "_Temp" + Path.GetExtension(From_File);
            StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Encode_Mp3/Audio_Delay.bat");
            stw.WriteLine("chcp 65001");
            if (Path.GetExtension(From_File) == ".wav")
                stw.Write("ffmpeg.exe -y -i \"" + From_File + "\" -af volume=" + (Volume / 100) + " -async 1 -ab 144k \"" + To_Temp_File + "\"");
            else
                stw.Write("ffmpeg.exe -y -i \"" + From_File + "\" -af volume=" + (Volume / 100) + " -async 1 -ab 144k -acodec libmp3lame -f mp3 \"" + To_Temp_File + "\"");
            stw.Close();
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = Voice_Set.Special_Path + "/Encode_Mp3/Audio_Delay.bat",
                WorkingDirectory = Voice_Set.Special_Path + "/Encode_Mp3",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process p = Process.Start(processStartInfo);
            p.WaitForExit();
            File.Delete(Voice_Set.Special_Path + "/Encode_Mp3/Audio_Delay.bat");
            if (Sub_Code.File_Move(To_Temp_File, To_File, true))
                return true;
            else
                return false;
        }
        //遅延と音量調整と速度(-c copyが使用できないため、時間短縮を兼ねて同時に処理します)
        public static bool Sound_Volume_Speed(string From_File, string To_File, double Volume, double Speed, uint SampleRate)
        {
            if (!File.Exists(From_File))
                return false;
            string To_Temp_File = Path.GetDirectoryName(From_File) + "\\" + Path.GetFileName(From_File) + "_Temp" + Path.GetExtension(From_File);
            StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Encode_Mp3/Audio_Delay.bat");
            stw.WriteLine("chcp 65001");
            if (Path.GetExtension(From_File) == ".wav")
                stw.Write("ffmpeg.exe -y -i \"" + From_File + "\" -af \"volume=" + (Volume / 100) + ",asetrate=" + (SampleRate * Speed) + "\" -ar " + SampleRate +
                " -async 1 -ab 144k \"" + To_Temp_File + "\"");
            else
                stw.Write("ffmpeg.exe -y -i \"" + From_File + "\" -af \"volume=" + (Volume / 100) + ",asetrate=" + (SampleRate * Speed) + "\" -ar " + SampleRate +
                " -async 1 -ab 144k -acodec libmp3lame -f mp3 \"" + To_Temp_File + "\"");
            stw.Close();
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = Voice_Set.Special_Path + "/Encode_Mp3/Audio_Delay.bat",
                WorkingDirectory = Voice_Set.Special_Path + "/Encode_Mp3",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process p = Process.Start(processStartInfo);
            p.WaitForExit();
            File.Delete(Voice_Set.Special_Path + "/Encode_Mp3/Audio_Delay.bat");
            if (Sub_Code.File_Move(To_Temp_File, To_File, true))
                return true;
            else
                return false;
        }
        //複数のサウンドを合体
        public static void Sound_Combine(List<string> Files, List<double> Pos, List<double>Volume, List<double> Speed, string To_File, bool IsEncodeMP3, bool IsFromFileDelete = false)
        {
            int mixer = BassMix.BASS_Mixer_StreamCreate(44100, 2, BASSFlag.BASS_STREAM_DECODE);
            long Mixer_Max_Length = 0;
            List<int> Streams = new List<int>();
            List<int> Stream_Handles = new List<int>();
            for (int Number = 0; Number < Files.Count; Number++)
            {
                Streams.Add(Bass.BASS_StreamCreateFile(Files[Number], 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE));
                Stream_Handles.Add(BassFx.BASS_FX_TempoCreate(Streams[Streams.Count - 1], BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_FX_FREESOURCE));
                float Freq = 44100;
                Bass.BASS_ChannelGetAttribute(Stream_Handles[Stream_Handles.Count - 1], BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, ref Freq);
                Bass.BASS_ChannelSetAttribute(Stream_Handles[Stream_Handles.Count - 1], BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, Freq * (float)Speed[Number]);
                Bass.BASS_ChannelSetAttribute(Stream_Handles[Stream_Handles.Count - 1], BASSAttribute.BASS_ATTRIB_VOL, (float)(Volume[Number] / 100));
                long start = Bass.BASS_ChannelSeconds2Bytes(mixer, Pos[Number]);
                BassMix.BASS_Mixer_StreamAddChannelEx(mixer, Stream_Handles[Stream_Handles.Count - 1], BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE, start, 0);
                long Now_Stream_Length = Bass.BASS_ChannelGetLength(Streams[Streams.Count - 1], BASSMode.BASS_POS_BYTES);
                if (Mixer_Max_Length < Now_Stream_Length + start)
                    Mixer_Max_Length = Now_Stream_Length + start;
            }
            EncoderWAV l = new EncoderWAV(mixer);
            l.InputFile = null;
            l.OutputFile = To_File + ".tmp";
            l.WAV_BitsPerSample = 24;
            l.Start(null, IntPtr.Zero, false);
            byte[] encBuffer = new byte[65536];
            while (Bass.BASS_ChannelIsActive(mixer) == BASSActive.BASS_ACTIVE_PLAYING)
            {
                int len = Bass.BASS_ChannelGetData(mixer, encBuffer, encBuffer.Length);
                if (len <= 0)
                    break;
                else if (Mixer_Max_Length <= Bass.BASS_ChannelGetPosition(mixer, BASSMode.BASS_POS_BYTES))
                    break;
            }
            l.Stop();
            Bass.BASS_StreamFree(mixer);
            foreach (int Stream in Stream_Handles)
                Bass.BASS_StreamFree(Stream);
            foreach (int Stream in Streams)
                Bass.BASS_StreamFree(Stream);
            if (IsEncodeMP3)
            {
                Un4seen.Bass.Misc.EncoderLAME mc = new Un4seen.Bass.Misc.EncoderLAME(0);
                mc.EncoderDirectory = Voice_Set.Special_Path + "/Encode_Mp3";
                mc.InputFile = To_File + ".tmp";
                mc.OutputFile = To_File;
                mc.LAME_Bitrate = (int)Un4seen.Bass.Misc.EncoderLAME.BITRATE.kbps_144;
                mc.LAME_Mode = Un4seen.Bass.Misc.EncoderLAME.LAMEMode.Default;
                mc.LAME_Quality = Un4seen.Bass.Misc.EncoderLAME.LAMEQuality.Q2;
                Un4seen.Bass.Misc.BaseEncoder.EncodeFile(mc, null, true, false, true);
                mc.Dispose();
                File.Delete(To_File + ".tmp");
            }
            else
                Sub_Code.File_Move(To_File + ".tmp", To_File, true);
            if (File.Exists(To_File) && IsFromFileDelete)
            {
                foreach (string File_Now in Files)
                    Sub_Code.File_Delete_V2(File_Now);
            }
        }
    }
}