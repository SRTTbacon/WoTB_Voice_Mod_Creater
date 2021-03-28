using Force.Crc32;
using K4os.Compression.LZ4;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Windows;

namespace WoTB_Voice_Mod_Creater
{
    public class DVPL
    {
        public static void DDS_DLL_Extract()
        {
            if (!Directory.Exists(Directory.GetCurrentDirectory() + "/dll"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/dll");
            }
            using (Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("WoTB_Voice_Mod_Creater.Resources.DdsFileTypePlusIO_x86.dll"))
            {
                using (FileStream bw = new FileStream(Directory.GetCurrentDirectory() + "/dll/DdsFileTypePlusIO_x86.dll", FileMode.Create))
                {
                    while (stream.Position < stream.Length)
                    {
                        byte[] bits = new byte[stream.Length];
                        stream.Read(bits, 0, (int)stream.Length);
                        bw.Write(bits, 0, (int)stream.Length);
                    }
                }
                stream.Close();
            }
        }
        public static void LAME_DLL_Extract()
        {
            if (!Directory.Exists(Directory.GetCurrentDirectory() + "/dll"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/dll");
            }
            using (Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("WoTB_Voice_Mod_Creater.Resources.libmp3lame.32.dll"))
            {
                using (FileStream bw = new FileStream(Directory.GetCurrentDirectory() + "/dll/libmp3lame.32.dll", FileMode.Create))
                {
                    while (stream.Position < stream.Length)
                    {
                        byte[] bits = new byte[stream.Length];
                        stream.Read(bits, 0, (int)stream.Length);
                        bw.Write(bits, 0, (int)stream.Length);
                    }
                }
                stream.Close();
            }
        }
        public static void BASS_DLL_Extract()
        {
            using (Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("WoTB_Voice_Mod_Creater.Resources.bassenc.dll"))
            {
                using (FileStream bw = new FileStream(Directory.GetCurrentDirectory() + "/dll/bassenc.dll", FileMode.Create))
                {
                    while (stream.Position < stream.Length)
                    {
                        byte[] bits = new byte[stream.Length];
                        stream.Read(bits, 0, (int)stream.Length);
                        bw.Write(bits, 0, (int)stream.Length);
                    }
                }
                stream.Close();
            }
        }
        //.dvplを解除する
        //例:sounds.yaml.dvpl->sounds.yaml
        static Process p;
        public static void DVPL_UnPack(string From_File, string To_File, bool IsFromFileDelete)
        {
            if (!File.Exists(From_File))
            {
                return;
            }
            File.Copy(From_File, Voice_Set.Special_Path + "/DVPL/Temp_Unpack.tmp.dvpl", true);
            StreamWriter DVPL_Unpack = File.CreateText(Voice_Set.Special_Path + "/DVPL/UnPack.bat");
            DVPL_Unpack.WriteLine("chcp 65001");
            DVPL_Unpack.Write("\"" + Voice_Set.Special_Path + "/DVPL/DVPL_Extract.exe\"");
            DVPL_Unpack.Close();
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = Voice_Set.Special_Path + "/DVPL/UnPack.bat",
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                WorkingDirectory = Voice_Set.Special_Path + "/DVPL",
                UseShellExecute = false
            };
            p = Process.Start(processStartInfo);
            p.StandardInput.WriteLine("\r\n");
            p.OutputDataReceived += new DataReceivedEventHandler(WriteMessage);
            p.BeginOutputReadLine();
            p.WaitForExit();
            p.Close();
            p.Dispose();
            try
            {
                Sub_Code.File_Move(Voice_Set.Special_Path + "/DVPL/Temp_Unpack.tmp", To_File, true);
                if (IsFromFileDelete)
                {
                    File.Delete(From_File);
                }
            }
            catch (Exception e)
            {
                //dvplが解除されなかった場合
                Sub_Code.Error_Log_Write(e.Message);
            }
            File.Delete(Voice_Set.Special_Path + "/DVPL/UnPack.bat");
        }
        static void WriteMessage(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
            {
                return;
            }
            p.StandardInput.WriteLine("\r\n");
        }
        public static bool DVPL_Pack(string From_File, string To_File, bool IsFromFileDelete)
        {
            if (!File.Exists(From_File))
            {
                return false;
            }
            FileInfo file = new FileInfo(From_File);
            long size = file.Length;
            if (size >= 50000000)
            {
                return DVPL_Pack_V2(From_File, To_File, IsFromFileDelete);
            }
            else
            {
                if (Path.GetExtension(From_File) == ".tex")
                {
                    CREATE_DVPL(LZ4Level.L00_FAST, From_File, To_File, IsFromFileDelete);
                }
                else
                {
                    CREATE_DVPL(LZ4Level.L03_HC, From_File, To_File, IsFromFileDelete);
                }
            }
            return true;
        }
        static bool DVPL_Pack_V2(string From_File, string To_File, bool IsFromFileDelete)
        {
            if (!Environment.Is64BitOperatingSystem)
            {
                MessageBox.Show("50MB以上のファイルをdvpl化する場合64BitのOSを使用する必要があります。");
                return false;
            }
            StreamWriter DVPL_Unpack = File.CreateText(Voice_Set.Special_Path + "/DVPL/DVPL_Pack.bat");
            DVPL_Unpack.WriteLine("chcp 65001");
            DVPL_Unpack.Write("\"" + Voice_Set.Special_Path + "/DVPL/DVPL_Convert.exe\" \"" + From_File + "\" \"" + To_File + "\"");
            DVPL_Unpack.Close();
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = Voice_Set.Special_Path + "/DVPL/DVPL_Pack.bat",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process p2 = Process.Start(processStartInfo);
            p2.WaitForExit();
            p2.Close();
            p2.Dispose();
            File.Delete(Voice_Set.Special_Path + "/DVPL/DVPL_Pack.bat");
            if (IsFromFileDelete)
            {
                File.Delete(From_File);
            }
            if (File.Exists(To_File))
            {
                return true;
            }
            return false;
        }
        static void CREATE_DVPL(LZ4Level COMPRESSION_TYPE, string From_File, string ToFile, bool IsFromFileDelete)
        {
            byte[] ORIGINAL_DATA = File.ReadAllBytes(From_File);
            int ORIGINAL_SIZE = ORIGINAL_DATA.Length;
            byte[] LZ4_CONTENT = new byte[LZ4Codec.MaximumOutputSize(ORIGINAL_SIZE)];
            int LZ4_SIZE = LZ4Codec.Encode(ORIGINAL_DATA, LZ4_CONTENT, COMPRESSION_TYPE);
            if (COMPRESSION_TYPE == LZ4Level.L00_FAST)
            {
                Buffer.BlockCopy(LZ4_CONTENT, 2, LZ4_CONTENT, 0, LZ4_CONTENT.Length - 2);
                LZ4_SIZE -= 2;
            }
            Array.Resize(ref LZ4_CONTENT, LZ4_SIZE);
            byte[] DVPL_CONTENT = FORMAT_WG_DVPL(LZ4_CONTENT, LZ4_SIZE, ORIGINAL_SIZE, COMPRESSION_TYPE);
            File.WriteAllBytes(ToFile, DVPL_CONTENT);
            if (IsFromFileDelete)
            {
                File.Delete(From_File);
            }
        }
        static byte[] FORMAT_WG_DVPL(byte[] LZ4_CONTENT, int LZ4_SIZE, int ORIGINAL_SIZE, LZ4Level COMPRESSION_TYPE)
        {
            uint LZ4_CRC32 = Crc32Algorithm.Compute(LZ4_CONTENT);
            byte[] DVPL_TEXT = Encoding.UTF8.GetBytes("DVPL");
            ushort COMPRESSION_TYPE_USHORT = (ushort)COMPRESSION_TYPE;
            if (COMPRESSION_TYPE != LZ4Level.L00_FAST) COMPRESSION_TYPE_USHORT -= 1;
            byte[] DVPL_CONTENT = new byte[LZ4_CONTENT.Length + sizeof(uint) * 3 + sizeof(ushort) * 2 + DVPL_TEXT.Length];
            int OFFSET_ACCUMULATOR = 0;
            Buffer.BlockCopy(LZ4_CONTENT, 0, DVPL_CONTENT, OFFSET_ACCUMULATOR, LZ4_CONTENT.Length);
            OFFSET_ACCUMULATOR += LZ4_CONTENT.Length;
            Buffer.BlockCopy(BitConverter.GetBytes((uint)ORIGINAL_SIZE), 0, DVPL_CONTENT, OFFSET_ACCUMULATOR, sizeof(uint));
            OFFSET_ACCUMULATOR += sizeof(uint);
            Buffer.BlockCopy(BitConverter.GetBytes((uint)LZ4_SIZE), 0, DVPL_CONTENT, OFFSET_ACCUMULATOR, sizeof(uint));
            OFFSET_ACCUMULATOR += sizeof(uint);
            Buffer.BlockCopy(BitConverter.GetBytes(LZ4_CRC32), 0, DVPL_CONTENT, OFFSET_ACCUMULATOR, sizeof(uint));
            OFFSET_ACCUMULATOR += sizeof(uint);
            Buffer.BlockCopy(BitConverter.GetBytes(COMPRESSION_TYPE_USHORT), 0, DVPL_CONTENT, OFFSET_ACCUMULATOR, sizeof(ushort));
            OFFSET_ACCUMULATOR += sizeof(ushort);
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)0), 0, DVPL_CONTENT, OFFSET_ACCUMULATOR, sizeof(ushort));
            OFFSET_ACCUMULATOR += sizeof(ushort);
            Buffer.BlockCopy(DVPL_TEXT, 0, DVPL_CONTENT, OFFSET_ACCUMULATOR, DVPL_TEXT.Length);
            return DVPL_CONTENT;
        }
    }
}