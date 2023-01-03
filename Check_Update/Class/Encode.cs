using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Check_Update.Encode
{
    //AESで暗号化・複合化するクラス
    public static class FileEncryptor
    {
        public const int KeyLength = 16;
        private static byte[] GenerateByteKey(string password)
        {
            var bytesPassword = Encoding.UTF8.GetBytes(password);
            var bytesKey = new byte[KeyLength];
            for (int i = 0; i < KeyLength; i++)
                bytesKey[i] = (i < bytesPassword.Length) ? bytesPassword[i] : (byte)0;
            return bytesKey;
        }
        public static void Encrypt(Stream ifs, Stream ofs, string password)
        {
            var bytesKey = GenerateByteKey(password);
            var aes = new AesCryptoServiceProvider()
            {
                BlockSize = 128,
                KeySize = 128,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                Key = bytesKey,
            };
            aes.GenerateIV();
            var bytesIV = aes.IV;
            ofs.Write(bytesIV, 0, 16);
            using (var encrypt = aes.CreateEncryptor())
            {
                using (var cs = new CryptoStream(ofs, encrypt, CryptoStreamMode.Write))
                {
                    while (true)
                    {
                        var buffer = new byte[1024];
                        var len = ifs.Read(buffer, 0, buffer.Length);
                        if (len > 0)
                            cs.Write(buffer, 0, len);
                        else
                            break;
                    }
                }
            }
        }
        public static void Decrypt_To_File(Stream ifs, Stream ofs, string Password)
        {
            var bytesKey = GenerateByteKey(Password);
            var bytesIV = new byte[KeyLength];
            ifs.Read(bytesIV, 0, KeyLength);
            var aes = new AesCryptoServiceProvider()
            {
                BlockSize = 128,
                KeySize = 128,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                Key = bytesKey,
                IV = bytesIV
            };
            using (var encrypt = aes.CreateDecryptor())
            {
                using (var cs = new CryptoStream(ofs, encrypt, CryptoStreamMode.Write))
                {
                    while (true)
                    {
                        var buffer = new byte[1024];
                        var len = ifs.Read(buffer, 0, buffer.Length);
                        if (len > 0)
                            cs.Write(buffer, 0, len);
                        else
                            break;
                    }
                }
            }
        }
        public static StreamReader Decrypt_To_Stream(Stream ifs, string Password)
        {
            var bytesKey = GenerateByteKey(Password);
            var bytesIV = new byte[KeyLength];
            ifs.Read(bytesIV, 0, KeyLength);
            var aes = new AesCryptoServiceProvider()
            {
                BlockSize = 128,
                KeySize = 128,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                Key = bytesKey,
                IV = bytesIV
            };
            var sOutputFilename = new MemoryStream();
            var desdecrypt = aes.CreateDecryptor();
            var cryptostreamDecr = new CryptoStream(ifs, desdecrypt, CryptoStreamMode.Read);
            var fsDecrypted = new StreamWriter(sOutputFilename);
            fsDecrypted.Write(new StreamReader(cryptostreamDecr).ReadToEnd());
            fsDecrypted.Flush();
            sOutputFilename.Position = 0;
            return new StreamReader(sOutputFilename);
        }
    }
    public class Crypt_Decrypt
    {
        public static bool Stream_Encrypt_To_File(string From_File, string To_File, string Password, bool IsFromFileDelete)
        {
            try
            {
                if (!File.Exists(From_File))
                    return false;
                using (var eifs = new FileStream(From_File, FileMode.Open, FileAccess.Read))
                {
                    using (var eofs = new FileStream(To_File, FileMode.Create, FileAccess.Write))
                        FileEncryptor.Encrypt(eifs, eofs, Password);
                }
                if (IsFromFileDelete)
                    File.Delete(From_File);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static StreamReader File_Decrypt_To_Stream(string From_File, string Password)
        {
            try
            {
                StreamReader str = null;
                using (var eifs = new FileStream(From_File, FileMode.Open, FileAccess.Read))
                    str = FileEncryptor.Decrypt_To_Stream(eifs, Password);
                return str;
            }
            catch
            {
                return null;
            }
        }
    }
}