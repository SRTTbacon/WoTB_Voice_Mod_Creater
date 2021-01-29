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
        public static void DVPL_Unpack_Extract()
        {
            try
            {
                if (Directory.Exists(Voice_Set.Special_Path + "/DVPL"))
                {
                    Directory.Delete(Voice_Set.Special_Path + "/DVPL", true);
                }
                using (Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("WoTB_Voice_Mod_Creater.Resources.DVPL.zip"))
                {
                    using (FileStream bw = new FileStream(Voice_Set.Special_Path + "/Temp_DVPL.zip", FileMode.Create))
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
                ZipFile.ExtractToDirectory(Voice_Set.Special_Path + "/Temp_DVPL.zip", Voice_Set.Special_Path + "/DVPL");
                File.Delete(Voice_Set.Special_Path + "/Temp_DVPL.zip");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        public static void Loading_Extract()
        {
            try
            {
                if (Directory.Exists(Voice_Set.Special_Path + "/Loading"))
                {
                    Directory.Delete(Voice_Set.Special_Path + "/Loading", true);
                }
                using (Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("WoTB_Voice_Mod_Creater.Resources.Loading.zip"))
                {
                    using (FileStream bw = new FileStream(Voice_Set.Special_Path + "/Temp_Loading.zip", FileMode.Create))
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
                ZipFile.ExtractToDirectory(Voice_Set.Special_Path + "/Temp_Loading.zip", Voice_Set.Special_Path + "/Loading");
                File.Delete(Voice_Set.Special_Path + "/Temp_Loading.zip");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        public static void Encode_Mp3_Extract()
        {
            try
            {
                if (Directory.Exists(Voice_Set.Special_Path + "/Encode_Mp3"))
                {
                    Directory.Delete(Voice_Set.Special_Path + "/Encode_Mp3", true);
                }
                using (Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("WoTB_Voice_Mod_Creater.Resources.Encode_Mp3.zip"))
                {
                    using (FileStream bw = new FileStream(Voice_Set.Special_Path + "/Temp_Encode_Mp3.zip", FileMode.Create))
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
                ZipFile.ExtractToDirectory(Voice_Set.Special_Path + "/Temp_Encode_Mp3.zip", Voice_Set.Special_Path + "/Encode_Mp3");
                File.Delete(Voice_Set.Special_Path + "/Temp_Encode_Mp3.zip");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        public static void Fmod_Designer_Extract()
        {
            try
            {
                if (Directory.Exists(Voice_Set.Special_Path + "/Fmod_Designer"))
                {
                    Directory.Delete(Voice_Set.Special_Path + "/Fmod_Designer", true);
                }
                using (Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("WoTB_Voice_Mod_Creater.Resources.Fmod_Designer.zip"))
                {
                    using (FileStream bw = new FileStream(Voice_Set.Special_Path + "/Temp_Fmod_Designer.zip", FileMode.Create))
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
                ZipFile.ExtractToDirectory(Voice_Set.Special_Path + "/Temp_Fmod_Designer.zip", Voice_Set.Special_Path + "/Fmod_Designer");
                File.Delete(Voice_Set.Special_Path + "/Temp_Fmod_Designer.zip");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        public static void SE_Extract()
        {
            try
            {
                if (Directory.Exists(Voice_Set.Special_Path + "/SE"))
                {
                    Directory.Delete(Voice_Set.Special_Path + "/SE", true);
                }
                using (Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("WoTB_Voice_Mod_Creater.Resources.SE.zip"))
                {
                    using (FileStream bw = new FileStream(Voice_Set.Special_Path + "/Temp_SE.zip", FileMode.Create))
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
                ZipFile.ExtractToDirectory(Voice_Set.Special_Path + "/Temp_SE.zip", Voice_Set.Special_Path + "/SE");
                File.Delete(Voice_Set.Special_Path + "/Temp_SE.zip");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        public static void Fmod_Android_Create_Extract()
        {
            try
            {
                if (Directory.Exists(Voice_Set.Special_Path + "/Fmod_Android_Create"))
                {
                    Directory.Delete(Voice_Set.Special_Path + "/Fmod_Android_Create", true);
                }
                using (Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("WoTB_Voice_Mod_Creater.Resources.Fmod_Android_Create.zip"))
                {
                    using (FileStream bw = new FileStream(Voice_Set.Special_Path + "/Temp_Fmod_Android_Create.zip", FileMode.Create))
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
                ZipFile.ExtractToDirectory(Voice_Set.Special_Path + "/Temp_Fmod_Android_Create.zip", Voice_Set.Special_Path + "/Fmod_Android_Create");
                File.Delete(Voice_Set.Special_Path + "/Temp_Fmod_Android_Create.zip");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        //.dvplを解除する
        //例:sounds.yaml.dvpl->sounds.yaml
        public static void DVPL_UnPack(string From_File, string To_File, bool IsFromFileDelete)
        {
            if (!File.Exists(From_File))
            {
                return;
            }
            File.Copy(From_File, Voice_Set.Special_Path + "/DVPL/Temp_Unpack.dvpl", true);
            StreamWriter DVPL_Unpack = File.CreateText(Voice_Set.Special_Path + "/DVPL/UnPack.bat");
            DVPL_Unpack.WriteLine("chcp 65001");
            DVPL_Unpack.Write("\"" + Voice_Set.Special_Path + "/DVPL/Python/python.exe\" \"" + Voice_Set.Special_Path + "/DVPL/UnPack.py\" \"" + From_File + "\" \"" + To_File + "\"");
            DVPL_Unpack.Close();
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = Voice_Set.Special_Path + "/DVPL/UnPack.bat",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process p = Process.Start(processStartInfo);
            p.WaitForExit();
            try
            {
                File.Copy(Voice_Set.Special_Path + "/DVPL/Temp_Unpacked.tmp", To_File, true);
                File.Delete(Voice_Set.Special_Path + "/DVPL/Temp_Unpacked.tmp");
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
            File.Delete(Voice_Set.Special_Path + "/DVPL/Temp_Unpack.dvpl");
            File.Delete(Voice_Set.Special_Path + "/DVPL/UnPack.bat");
        }
        public static bool DVPL_Pack(string From_File, string To_File, bool IsFromFileDelete)
        {
            if (!File.Exists(From_File))
            {
                return false;
            }
            CREATE_DVPL(LZ4Level.L03_HC, From_File, To_File, IsFromFileDelete);
            return true;
        }
        static void CREATE_DVPL(LZ4Level COMPRESSION_TYPE, string From_File, string ToFile, bool Delete)
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
            if (Delete)
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