using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WoTB_Voice_Mod_Creater.Class
{
    public class GoogleDrive
    {
		//引用元 : https://gist.github.com/yasirkula/d0ec0c07b138748e5feaecbd93b6223c#file-filedownloader-cs
		//ありがとナス！
		const string GOOGLE_DRIVE_DOMAIN = "drive.google.com";
		const string GOOGLE_DRIVE_DOMAIN2 = "https://drive.google.com";
		const int GOOGLE_DRIVE_MAX_DOWNLOAD_ATTEMPT = 3;
		public delegate void DownloadProgressChangedEventHandler(object sender, DownloadProgress progress);
		public class DownloadProgress
		{
			public long BytesReceived, TotalBytesToReceive;
			public object UserState;
			public int ProgressPercentage
			{
				get
				{
					if (TotalBytesToReceive > 0L)
						return (int)(((double)BytesReceived / TotalBytesToReceive) * 100);
					return 0;
				}
			}
		}
		class CookieAwareWebClient : WebClient
		{
			class CookieContainer
			{
				readonly Dictionary<string, string> cookies = new Dictionary<string, string>();
				public string this[Uri address]
				{
					get
					{
						string cookie;
						if (cookies.TryGetValue(address.Host, out cookie))
							return cookie;
						return null;
					}
					set
					{
						cookies[address.Host] = value;
					}
				}
			}
			readonly CookieContainer cookies = new CookieContainer();
			public DownloadProgress ContentRangeTarget;
			protected override WebRequest GetWebRequest(Uri address)
			{
				WebRequest request = base.GetWebRequest(address);
				if (request is HttpWebRequest)
				{
					string cookie = cookies[address];
					if (cookie != null)
						((HttpWebRequest)request).Headers.Set("cookie", cookie);
					if (ContentRangeTarget != null)
						((HttpWebRequest)request).AddRange(0);
				}
				return request;
			}
			protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
			{
				return ProcessResponse(base.GetWebResponse(request, result));
			}
			protected override WebResponse GetWebResponse(WebRequest request)
			{
				return ProcessResponse(base.GetWebResponse(request));
			}
			WebResponse ProcessResponse(WebResponse response)
			{
				string[] cookies = response.Headers.GetValues("Set-Cookie");
				if (cookies != null && cookies.Length > 0)
				{
					int length = 0;
					for (int i = 0; i < cookies.Length; i++)
						length += cookies[i].Length;
					StringBuilder cookie = new StringBuilder(length);
					for (int i = 0; i < cookies.Length; i++)
						cookie.Append(cookies[i]);

					this.cookies[response.ResponseUri] = cookie.ToString();
				}
				if (ContentRangeTarget != null)
				{
					string[] rangeLengthHeader = response.Headers.GetValues("Content-Range");
					if (rangeLengthHeader != null && rangeLengthHeader.Length > 0)
					{
						int splitIndex = rangeLengthHeader[0].LastIndexOf('/');
						if (splitIndex >= 0 && splitIndex < rangeLengthHeader[0].Length - 1)
						{
							long length;
							if (long.TryParse(rangeLengthHeader[0].Substring(splitIndex + 1), out length))
								ContentRangeTarget.TotalBytesToReceive = length;
						}
					}
				}
				return response;
			}
		}
		readonly CookieAwareWebClient webClient;
		readonly DownloadProgress downloadProgress;
		Uri downloadAddress;
		string downloadPath;
		bool asyncDownload;
		object userToken;
		bool downloadingDriveFile;
		int driveDownloadAttempt;
		public event DownloadProgressChangedEventHandler DownloadProgressChanged;
		public event AsyncCompletedEventHandler DownloadFileCompleted;
		public GoogleDrive()
		{
			webClient = new CookieAwareWebClient();
			webClient.DownloadProgressChanged += DownloadProgressChangedCallback;
			webClient.DownloadFileCompleted += DownloadFileCompletedCallback;
			downloadProgress = new DownloadProgress();
		}
		public void StopDownloadAsync()
        {
			webClient.CancelAsync();
		}
		public void DownloadFile(string address, string fileName)
		{
			DownloadFile(address, fileName, false, null);
		}
		public void DownloadFileAsync(string address, string fileName, object userToken = null)
		{
			DownloadFile(address, fileName, true, userToken);
		}
		void DownloadFile(string address, string fileName, bool asyncDownload, object userToken)
		{
			downloadingDriveFile = address.StartsWith(GOOGLE_DRIVE_DOMAIN) || address.StartsWith(GOOGLE_DRIVE_DOMAIN2);
			if (downloadingDriveFile)
			{
				address = GetGoogleDriveDownloadAddress(address);
				driveDownloadAttempt = 1;

				webClient.ContentRangeTarget = downloadProgress;
			}
			else
				webClient.ContentRangeTarget = null;
			downloadAddress = new Uri(address);
			downloadPath = fileName;
			downloadProgress.TotalBytesToReceive = -1L;
			downloadProgress.UserState = userToken;
			this.asyncDownload = asyncDownload;
			this.userToken = userToken;
			DownloadFileInternal();
		}
		void DownloadFileInternal()
		{
			if (!asyncDownload)
			{
				webClient.DownloadFile(downloadAddress, downloadPath);
				DownloadFileCompletedCallback(webClient, new AsyncCompletedEventArgs(null, false, null));
			}
			else if (userToken == null)
				webClient.DownloadFileAsync(downloadAddress, downloadPath);
			else
				webClient.DownloadFileAsync(downloadAddress, downloadPath, userToken);
		}
		void DownloadProgressChangedCallback(object sender, DownloadProgressChangedEventArgs e)
		{
			if (DownloadProgressChanged != null)
			{
				downloadProgress.BytesReceived = e.BytesReceived;
				if (e.TotalBytesToReceive > 0L)
					downloadProgress.TotalBytesToReceive = e.TotalBytesToReceive;
				DownloadProgressChanged(this, downloadProgress);
			}
		}
		void DownloadFileCompletedCallback(object sender, AsyncCompletedEventArgs e)
		{
			if (!downloadingDriveFile)
			{
				if (DownloadFileCompleted != null)
					DownloadFileCompleted(this, e);
			}
			else
			{
				if (driveDownloadAttempt < GOOGLE_DRIVE_MAX_DOWNLOAD_ATTEMPT && !ProcessDriveDownload())
				{
					driveDownloadAttempt++;
					DownloadFileInternal();
				}
				else if (DownloadFileCompleted != null)
					DownloadFileCompleted(this, e);
			}
		}
		bool ProcessDriveDownload()
		{
			FileInfo downloadedFile = new FileInfo(downloadPath);
			if (downloadedFile == null)
				return true;
			if (downloadedFile.Length > 60000L)
				return true;
			string content;
			using (var reader = downloadedFile.OpenText())
			{
				char[] header = new char[20];
				int readCount = reader.ReadBlock(header, 0, 20);
				if (readCount < 20 || !(new string(header).Contains("<!DOCTYPE html>")))
					return true;
				content = reader.ReadToEnd();
			}
			int linkIndex = content.LastIndexOf("href=\"/uc?");
			if (linkIndex < 0)
				return true;
			linkIndex += 6;
			int linkEnd = content.IndexOf('"', linkIndex);
			if (linkEnd < 0)
				return true;
			downloadAddress = new Uri("https://drive.google.com" + content.Substring(linkIndex, linkEnd - linkIndex).Replace("&amp;", "&"));
			return false;
		}
		string GetGoogleDriveDownloadAddress(string address)
		{
			int index = address.IndexOf("id=");
			int closingIndex;
			if (index > 0)
			{
				index += 3;
				closingIndex = address.IndexOf('&', index);
				if (closingIndex < 0)
					closingIndex = address.Length;
			}
			else
			{
				index = address.IndexOf("file/d/");
				if (index < 0)
					return string.Empty;
				index += 7;
				closingIndex = address.IndexOf('/', index);
				if (closingIndex < 0)
				{
					closingIndex = address.IndexOf('?', index);
					if (closingIndex < 0)
						closingIndex = address.Length;
				}
			}
			return string.Concat("https://drive.google.com/uc?id=", address.Substring(index, closingIndex - index), "&export=download");
		}
		public void Dispose()
		{
			webClient.Dispose();
		}
	}
}