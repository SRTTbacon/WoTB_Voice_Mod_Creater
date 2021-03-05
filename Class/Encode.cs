using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FileEncode
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
            {
                bytesKey[i] = (i < bytesPassword.Length) ? bytesPassword[i] : (byte)0;
            }
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
                        {
                            cs.Write(buffer, 0, len);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }
        public static void Decrypt(Stream ifs, Stream ofs, string password)
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
            var bytesIV = new byte[KeyLength];
            ifs.Read(bytesIV, 0, KeyLength);
            aes.IV = bytesIV;
            using (var encrypt = aes.CreateDecryptor())
            {
                using (var cs = new CryptoStream(ofs, encrypt, CryptoStreamMode.Write))
                {
                    while (true)
                    {
                        var buffer = new byte[1024];
                        var len = ifs.Read(buffer, 0, buffer.Length);
                        if (len > 0)
                        {
                            cs.Write(buffer, 0, len);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }
    }
}