using Force.Crc32;
using K4os.Compression.LZ4;
using System;
using System.IO;
using System.Text;
using System.Windows;

namespace WoTB_Voice_Mod_Creater
{
    public class DVPL
    {
        readonly static string Special_Path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/WoTB_Voice_Mod_Creater";
        public static void DVPL_Unpack_Extract()
        {
            try
            {
                using (Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("WoTB_Voice_Mod_Creater.Resources.DVPL.zip"))
                {
                    using (FileStream bw = new FileStream(Special_Path + "/Temp_DVPL.zip", FileMode.Create))
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
                System.IO.Compression.ZipFile.ExtractToDirectory(Special_Path + "/Temp_DVPL.zip", Special_Path + "/DVPL");
                File.Delete(Special_Path + "/Temp_DVPL.zip");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        public static void Loading_Extract()
        {
            try
            {
                using (Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("WoTB_Voice_Mod_Creater.Resources.Loading.zip"))
                {
                    using (FileStream bw = new FileStream(Special_Path + "/Temp_Loading.zip", FileMode.Create))
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
                System.IO.Compression.ZipFile.ExtractToDirectory(Special_Path + "/Temp_Loading.zip", Special_Path + "/Loading");
                File.Delete(Special_Path + "/Temp_Loading.zip");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        public static void Encode_Mp3_Extract()
        {
            try
            {
                using (Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("WoTB_Voice_Mod_Creater.Resources.Encode_Mp3.zip"))
                {
                    using (FileStream bw = new FileStream(Special_Path + "/Temp_Encode_Mp3.zip", FileMode.Create))
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
                System.IO.Compression.ZipFile.ExtractToDirectory(Special_Path + "/Temp_Encode_Mp3.zip", Special_Path + "/Encode_Mp3");
                File.Delete(Special_Path + "/Temp_Encode_Mp3.zip");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        public static void Fmod_Designer_Extract()
        {
            try
            {
                using (Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("WoTB_Voice_Mod_Creater.Resources.Fmod_Designer.zip"))
                {
                    using (FileStream bw = new FileStream(Special_Path + "/Temp_Fmod_Designer.zip", FileMode.Create))
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
                System.IO.Compression.ZipFile.ExtractToDirectory(Special_Path + "/Temp_Fmod_Designer.zip", Special_Path + "/Fmod_Designer");
                File.Delete(Special_Path + "/Temp_Fmod_Designer.zip");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        public static void SE_Extract()
        {
            try
            {
                using (Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("WoTB_Voice_Mod_Creater.Resources.SE.zip"))
                {
                    using (FileStream bw = new FileStream(Special_Path + "/Temp_SE.zip", FileMode.Create))
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
                System.IO.Compression.ZipFile.ExtractToDirectory(Special_Path + "/Temp_SE.zip", Special_Path + "/SE");
                File.Delete(Special_Path + "/Temp_SE.zip");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        public static void DVPL_Encode(string FileName)
        {
            if (!File.Exists(FileName))
            {
                return;
            }
            CREATE_DVPL(LZ4Level.L03_HC, FileName);
        }
        static void CREATE_DVPL(LZ4Level COMPRESSION_TYPE, string file)
        {
            byte[] ORIGINAL_DATA = File.ReadAllBytes(file);
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
            File.WriteAllBytes(file + ".dvpl", DVPL_CONTENT);
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