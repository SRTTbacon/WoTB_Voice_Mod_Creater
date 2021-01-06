using Force.Crc32;
using K4os.Compression.LZ4;
using System;
using System.Diagnostics;
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
            StreamWriter DVPL_Pack = File.CreateText(Special_Path + "/DVPL/Pack.bat");
            DVPL_Pack.Write("\"" + Special_Path + "/DVPL/Python/python.exe\" \"" + Special_Path + "/DVPL/Pack.py\" \"" + FileName + "\" \"" + FileName + ".dvpl\"");
            DVPL_Pack.Close();
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = Special_Path + "/DVPL/Pack.bat",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process p = Process.Start(processStartInfo);
            p.WaitForExit();
            File.Delete(Special_Path + "/DVPL/Pack.bat");
        }
    }
}