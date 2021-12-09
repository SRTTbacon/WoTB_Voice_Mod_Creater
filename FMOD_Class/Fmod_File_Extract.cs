using FMOD_API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace WoTB_Voice_Mod_Creater.FMOD
{
    public class Fmod_System
    {
        static FMOD_API.System FModSystem_Temp = new FMOD_API.System();
        public static FMOD_API.System FModSystem
        {
            get { return FModSystem_Temp; }
            set { FModSystem_Temp = value; }
        }
    }
    public class Fmod_File_Extract_V1
    {
        //FSB内のファイルを1つだけ抽出
        //引数:FSBファイルの場所,ファイル番号,出力先
        public static bool FSB_Extract_To_File(string FSB_File, int Index, string To_File)
        {
            if (!File.Exists(FSB_File))
            {
                Sub_Code.Error_Log_Write("指定したFSBファイルが存在しません。");
                return false;
            }
            RESULT FModResult;
            Sound FModSound = new Sound();
            FModResult = Fmod_System.FModSystem.createSound(FSB_File, MODE.SOFTWARE | MODE.CREATESTREAM | MODE.ACCURATETIME, ref FModSound);
            if (FModResult != RESULT.OK)
            {
                Sub_Code.Error_Log_Write(Error.String(FModResult));
                return false;
            }
            Sound SubSound = new Sound();
            if (FModSound.getSubSound(Index, ref SubSound) == RESULT.OK)
            {
                SubSound.seekData(0);
                FileStream SubSoundStream = new FileStream(To_File, FileMode.Create, FileAccess.Write);
                uint Length = WriteHeader(SubSoundStream, SubSound);
                do
                {
                    byte[] SoundData = new byte[65536];
                    uint LenToRead;
                    if (Length > SoundData.Length)
                    {
                        LenToRead = 65536;
                    }
                    else
                    {
                        LenToRead = Length;
                    }
                    uint LenRead = LenToRead;
                    IntPtr BufferPtr = Marshal.AllocHGlobal((int)LenToRead);
                    FModResult = SubSound.readData(BufferPtr, LenToRead, ref LenRead);
                    Marshal.Copy(BufferPtr, SoundData, 0, (int)LenRead);
                    SubSoundStream.Write(SoundData, 0, (int)LenRead);
                    Marshal.FreeHGlobal(BufferPtr);
                    Length -= LenRead;
                } while ((Length > 0) && (FModResult == RESULT.OK));
                long FileSize = SubSoundStream.Position;
                SubSoundStream.Seek(4, SeekOrigin.Begin);
                SubSoundStream.Write(BitConverter.GetBytes((long)(FileSize - 8)), 0, 4);
                SubSoundStream.Close();
            }
            else
            {
                SubSound.release();
                FModSound.release();
                return false;
            }
            SubSound.release();
            FModSound.release();
            return true;
        }
        //FSB内のすべての音声をディレクトリに抽出
        public static bool FSB_Extract_To_Directry(string FSB_File, string To_Dir, ref List<string> Output_Files)
        {
            try
            {
                if (!File.Exists(FSB_File))
                {
                    return false;
                }
                if (!Directory.Exists(To_Dir))
                {
                    Directory.CreateDirectory(To_Dir);
                }
                Extract_All_Files(FSB_File, To_Dir, ref Output_Files);
                return true;
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                return false;
            }
        }
        //FSB内のすべてのファイルを抽出
        static RESULT Extract_All_Files(string FSBFileStr, string To_Dir, ref List<string> Output_Files)
        {
            RESULT FModResult;
            Sound FModSound = new Sound();
            FModResult = Fmod_System.FModSystem.createSound(FSBFileStr, MODE.SOFTWARE | MODE.CREATESTREAM | MODE.ACCURATETIME, ref FModSound);
            if (FModResult != RESULT.OK)
            {
                Sub_Code.Error_Log_Write(Error.String(FModResult));
                return FModResult;
            }
            else
            {
                //FSB内のファイル数を取得
                List<string> Names = Fmod_Class.FSB_GetNames(FSBFileStr);
                Sound SubSound = new Sound();
                //残りのファイルがなくなるまで続ける
                for (int i = 0; i < Names.Count; i++)
                {
                    if (FModSound.getSubSound(i, ref SubSound) == RESULT.OK)
                    {
                        SubSound.seekData(0);
                        //今のところwav形式しか対応していません(.wav以外はffmpegでエンコードします)
                        FileStream SubSoundStream = new FileStream(To_Dir + @"\" + Names[i] + ".wav", FileMode.Create, FileAccess.Write);
                        Output_Files.Add(To_Dir + "\\" + Names[i] + ".wav");
                        uint Length = WriteHeader(SubSoundStream, SubSound);
                        do
                        {
                            //音をファイルに出力(1度に65536バイトしか入らないためループさせる)
                            byte[] SoundData = new byte[65536];
                            uint LenToRead;
                            if (Length > SoundData.Length)
                            {
                                LenToRead = 65536;
                            }
                            else
                            {
                                LenToRead = Length;
                            }
                            uint LenRead = LenToRead;
                            IntPtr BufferPtr = Marshal.AllocHGlobal((int)LenToRead);
                            FModResult = SubSound.readData(BufferPtr, LenToRead, ref LenRead);
                            Marshal.Copy(BufferPtr, SoundData, 0, (int)LenRead);
                            SubSoundStream.Write(SoundData, 0, (int)LenRead);
                            Marshal.FreeHGlobal(BufferPtr);
                            Length -= LenRead;
                        } while ((Length > 0) && (FModResult == RESULT.OK));
                        long FileSize = SubSoundStream.Position;
                        SubSoundStream.Seek(4, SeekOrigin.Begin);
                        SubSoundStream.Write(BitConverter.GetBytes((long)(FileSize - 8)), 0, 4);
                        SubSoundStream.Close();
                    }
                    SubSound.release();
                }
                FModSound.release();
            }
            return FModResult;
        }
        //.wavのヘッダーを入力
        static uint WriteHeader(FileStream SubSoundStream, Sound SubSound)
        {
            uint Milliseconds = 0;
            uint RAWBytes = 0;
            uint PCMBytes = 0;
            uint Length = 0;
            //音の情報を取得
            SubSound.getLength(ref Milliseconds, TIMEUNIT.MS);
            SubSound.getLength(ref RAWBytes, TIMEUNIT.RAWBYTES);
            SubSound.getLength(ref PCMBytes, TIMEUNIT.PCMBYTES);
            SubSoundStream.Seek(0, SeekOrigin.Begin);
            float Frequency = 0f;
            int DefPriority = 0;
            float DefPan = 0;
            float DefVolume = 0;
            SOUND_TYPE FModSoundType = new SOUND_TYPE();
            SOUND_FORMAT FModSoundFormat = new SOUND_FORMAT();
            int Channels = 0;
            int BitsPerSample = 0;
            SubSound.getDefaults(ref Frequency, ref DefVolume, ref DefPan, ref DefPriority);
            SubSound.getFormat(ref FModSoundType, ref FModSoundFormat, ref Channels, ref BitsPerSample);
            int BlockAlign = (Channels * BitsPerSample) / 8;
            float DataRate = (Channels * Frequency * BitsPerSample) / 8;
            uint PCMSamples = Convert.ToUInt32(Milliseconds * Frequency / 1000);
            if (Channels == 2)
            {
                Length = Convert.ToUInt32(PCMSamples * Channels * (BitsPerSample / 8));
            }
            else
            {
                Length = PCMBytes * 2;
            }
            //詳しくはこちらをお読みください。http://www.topherlee.com/software/pcm-tut-wavformat.html
            //最初の4バイトは必ず"RIFF"
            SubSoundStream.Write(Encoding.ASCII.GetBytes("RIFF"), 0, 4);
            SubSoundStream.Write(BitConverter.GetBytes((long)0), 0, 4);
            SubSoundStream.Write(Encoding.ASCII.GetBytes("WAVEfmt "), 0, 8);
            SubSoundStream.Write(BitConverter.GetBytes((long)16), 0, 4);
            SubSoundStream.Write(BitConverter.GetBytes((int)1), 0, 2);
            SubSoundStream.Write(BitConverter.GetBytes((int)2), 0, 2);
            SubSoundStream.Write(BitConverter.GetBytes((long)Frequency), 0, 4);
            SubSoundStream.Write(BitConverter.GetBytes((int)DataRate), 0, 4);
            SubSoundStream.Write(BitConverter.GetBytes(BlockAlign), 0, 2);
            SubSoundStream.Write(BitConverter.GetBytes(BitsPerSample), 0, 2);
            SubSoundStream.Write(Encoding.ASCII.GetBytes("data"), 0, 4);
            SubSoundStream.Write(BitConverter.GetBytes((long)Length), 0, 4);
            return Length;
        }
    }
    public class Fmod_File_Extract_V2
    {
        public static bool FSB_Extract_To_Directory(string FSB_Path, string To_Dir)
        {
            if (!File.Exists(FSB_Path))
                return false;
            if (!Directory.Exists(To_Dir))
                Directory.CreateDirectory(To_Dir);
            StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Wwise/FSB_Extract.bat");
            stw.WriteLine("chcp 65001");
            stw.Write("\"" + Voice_Set.Special_Path + "/Wwise/fsbext.exe\" " + "-d \"" + To_Dir + "\" \"" + FSB_Path + "\"");
            stw.Close();
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = Voice_Set.Special_Path + "/Wwise/FSB_Extract.bat",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process p = Process.Start(processStartInfo);
            p.WaitForExit();
            File.Delete(Voice_Set.Special_Path + "/Wwise/FSB_Extract.bat");
            return true;
        }
    }
    public class Fmod_Class
    {
        //FSB内ののファイル名をすべて取得
        public static List<string> FSB_GetNames(string FSB_File)
        {
            List<string> Name_List = new List<string>();
            int NumSubSounds = FSB_GetLength(FSB_File);
            if (NumSubSounds == 0)
                return Name_List;
            Sound MainSound = new Sound();
            Sound SubSound = new Sound();
            for (int i = 0; i < NumSubSounds; i++)
            {
                Fmod_System.FModSystem.createSound(FSB_File, MODE.CREATESTREAM, ref MainSound);
                if (MainSound.getSubSound(i, ref SubSound) == RESULT.OK)
                {
                    StringBuilder SubSoundName = new StringBuilder(256);
                    if (SubSound.getName(SubSoundName, 256) == RESULT.OK && SubSoundName[0] != 0)
                        Name_List.Add(SubSoundName.ToString());
                }
                SubSound.release();
                MainSound.release();
            }
            MainSound.release();
            return Name_List;
        }
        //FSB内のファイル数を取得
        public static int FSB_GetLength(string FSB_File)
        {
            RESULT FModResult;
            FMOD_API.System FModSystem = new FMOD_API.System();
            FModResult = Factory.System_Create(ref FModSystem);
            FModResult = FModSystem.init(16, INITFLAGS.NORMAL, IntPtr.Zero);
            Sound FModSound = new Sound();
            FModResult = FModSystem.createSound(FSB_File, MODE.CREATESTREAM, ref FModSound);
            if (FModResult != RESULT.OK)
            {
                FModSound.release();
                FModSystem.release();
                Sub_Code.Error_Log_Write(Error.String(FModResult));
                return 0;
            }
            else
            {
                int NumSubSounds = 0;
                FModSound.getNumSubSounds(ref NumSubSounds);
                FModSound.release();
                FModSystem.release();
                return NumSubSounds;
            }
        }
    }
}