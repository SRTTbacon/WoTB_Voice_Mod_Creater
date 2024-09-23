using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace WoTB_Voice_Mod_Creater.Class
{
    public static partial class DownloadManager
    {
        public static int TotalRead = 0;

        static HttpClient Client = new HttpClient();

        public static async Task<int> GetDownloadSize(string url)
        {
            using (HttpResponseMessage response = await Client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
                return (int)response.Content.Headers.ContentLength;
        }
        public static async Task DownloadAsync(string file, string url)
        {
            TotalRead = 0;
            using (HttpResponseMessage response = await Client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
            {
                using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    byte[] buffer = new byte[2048];
                    bool isMoreToRead = true;
                    FileStream output = new FileStream(file, FileMode.Create);
                    do
                    {
                        int read = await streamToReadFrom.ReadAsync(buffer, 0, buffer.Length);

                        if (read == 0)
                            isMoreToRead = false;

                        else
                        {
                            await output.WriteAsync(buffer, 0, read);

                            TotalRead += read;
                        }

                    } while (isMoreToRead);

                    output.Close();
                }
            }
        }
    }
}
