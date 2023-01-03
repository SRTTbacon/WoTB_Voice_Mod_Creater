using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;

namespace WoTB_Voice_Mod_Creater.Class
{
    public class Server_GetData
    {
        public delegate void Download_Event_Handler<T>(T args);
        public static event Download_Event_Handler<AsyncCompletedEventArgs> Download_Complete_Handler;
        public static event Download_Event_Handler<DownloadProgressChangedEventArgs> Download_Progress_Handler;
        public static async void Download_To_File_Async(string URL, string To_File)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadProgressChanged += delegate (object sender, DownloadProgressChangedEventArgs e)
                    {
                        Download_Progress_Handler(e);
                    };
                    client.DownloadFileCompleted += delegate (object sender, AsyncCompletedEventArgs e)
                    {
                        Download_Complete_Handler(e);
                    };
                    await client.DownloadFileTaskAsync(URL, To_File);
                }
            }
            catch { Download_Complete_Handler(new AsyncCompletedEventArgs(new System.Exception("指定したURLが存在しませんでした。"), false, null)); }
        }
        public static void Download_To_File(string URL, string To_File)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(URL, To_File);
            }
        }
        public static StreamReader Download_To_Stream(string URL)
        {
            try
            {
                byte[] Data = null;
                using (WebClient client = new WebClient())
                    Data = client.DownloadData(URL);
                if (Data != null)
                    return new StreamReader(new MemoryStream(Data));
                else
                    return null;
            }
            catch { return null; }
        }
        public static byte[] Download_To_Bytes(string URL)
        {
            try
            {
                byte[] Data = null;
                using (WebClient client = new WebClient())
                    Data = client.DownloadData(URL);
                if (Data != null)
                    return Data;
                else
                    return null;
            }
            catch { return null; }
        }
        public static bool URLExists(string URL)
        {
            bool result = false;
            WebRequest webRequest = WebRequest.Create(URL);
            webRequest.Timeout = 3000;
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)webRequest.GetResponse();
                result = true;
            }
            catch
            {
            }
            finally
            {
                if (response != null)
                    response.Close();
            }
            return result;
        }
        public static List<string> GetFiles(string URL)
        {
            StreamReader str = Download_To_Stream(URL);
            if (str == null)
                return new List<string>();
            List<string> Files = new List<string>();
            string Line;
            while ((Line = str.ReadLine()) != null)
                Files.Add(Line);
            str.Close();
            return Files;
        }
        public static long GetFileSize(string URL)
        {
            long result = 0;
            try
            {
                WebRequest req = WebRequest.Create(URL);
                req.Method = "HEAD";
                using (WebResponse resp = req.GetResponse())
                    if (long.TryParse(resp.Headers.Get("Content-Length"), out long contentLength))
                        result = contentLength;
            }
            catch { }
            return result;
        }
    }
}