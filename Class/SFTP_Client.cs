using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.IO;

namespace WoTB_Voice_Mod_Creater.Class
{
    //IsErrorLogModeはデバッグ用で基本false
    public class SFTP_Client
    {
        public bool IsConnected = false;
        //SSH.NETを使用してSFTPでやり取りします
        public SftpClient SFTP_Server = null;
        //引数:サーバーIP, ユーザー名, パスワード, ポート (これらはMain_Code.csのSRTTbacon_Serverクラスに記述しています)
        public SFTP_Client(string IP, string UserName, string Password, int Port)
        {
            try
            {
                ConnectionInfo ConnNfo = new ConnectionInfo(IP, Port, UserName, new AuthenticationMethod[] { new PasswordAuthenticationMethod(UserName, Password) });
                SFTP_Server = new SftpClient(ConnNfo);
                SFTP_Server.Connect();
                IsConnected = true;
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                IsConnected = false;
            }
        }
        //サーバーから切断
        public bool Close(bool IsErrorLogMode = false)
        {
            if (!IsConnected)
                return false;
            try
            {
                SFTP_Server.Disconnect();
                SFTP_Server.Dispose();
                IsConnected = false;
                return true;
            }
            catch (Exception e)
            {
                if (IsErrorLogMode)
                    Sub_Code.Error_Log_Write(e.Message);
                return false;
            }
        }
        //ファイルサイズを取得
        public long GetFileSize(string From_File, bool IsErrorLogMode = false)
        {
            if (!IsConnected)
                return 0;
            else if (!SFTP_Server.IsConnected)
                SFTP_Server.Connect();
            try
            {
                SftpFile GetFileInfo = SFTP_Server.Get(From_File);
                return GetFileInfo.Attributes.Size;
            }
            catch (Exception e)
            {
                if (IsErrorLogMode)
                    Sub_Code.Error_Log_Write(e.Message);
                return 0;
            }
        }
        //フォルダ内のファイルやフォルダを取得
        //引数:フォルダパス, フルネームで取得するか(/Example/Test.txtかTest.txtか), フォルダ名も含むか, エラー時にログを出力するか
        public List<string> GetFiles(string From_Dir, bool IsFullName, bool IsIncludeDirectory,  bool IsErrorLogMode = false)
        {
            if (!IsConnected)
                return new List<string>();
            else if (!SFTP_Server.IsConnected)
                SFTP_Server.Connect();
            try
            {
                IEnumerable<SftpFile> Files = SFTP_Server.ListDirectory(From_Dir);
                List<string> Temp = new List<string>();
                foreach (SftpFile File_Now in Files)
                {
                    if ((File_Now.Name != ".") && (File_Now.Name != ".."))
                    {
                        if (File_Now.IsDirectory && !IsIncludeDirectory)
                            continue;
                        if (IsFullName)
                            Temp.Add(File_Now.FullName);
                        else
                            Temp.Add(File_Now.Name);
                    }
                }
                return Temp;
            }
            catch (Exception e)
            {
                if (IsErrorLogMode)
                    Sub_Code.Error_Log_Write(e.Message);
                return new List<string>();
            }
        }
        //サーバー上のテキストファイルを1行読み取る
        public string GetFileLine(string From_File, bool IsErrorLogMode = false)
        {
            if (!IsConnected)
                return "";
            else if (!SFTP_Server.IsConnected)
                SFTP_Server.Connect();
            try
            {
                StreamReader str = SFTP_Server.OpenText(From_File);
                string GetLine = str.ReadLine();
                str.Dispose();
                return GetLine;
            }
            catch (Exception e)
            {
                if (IsErrorLogMode)
                    Sub_Code.Error_Log_Write(e.Message);
                return "";
            }
        }
        //サーバー上のテキストファイルの内容をStreamReaderとして出力します
        public StreamReader GetFileRead(string From_File, bool IsErrorLogMode = false)
        {
            if (!IsConnected)
                return new StreamReader(new MemoryStream());
            else if (SFTP_Server != null && !SFTP_Server.IsConnected)
                SFTP_Server.Connect();
            try
            {
                if (SFTP_Server.Exists(From_File))
                    return SFTP_Server.OpenText(From_File);
                else
                    return new StreamReader(new MemoryStream());
            }
            catch (Exception e)
            {
                if (IsErrorLogMode)
                    Sub_Code.Error_Log_Write(e.Message);
                return new StreamReader(new MemoryStream());
            }
        }
        //サーバー上に指定した名前のファイルが存在するか
        public bool File_Exist(string To_File, bool IsErrorLogMode = false)
        {
            if (!IsConnected)
                return false;
            else if (!SFTP_Server.IsConnected)
                SFTP_Server.Connect();
            try
            {
                return SFTP_Server.Exists(To_File);
            }
            catch (Exception e)
            {
                if (IsErrorLogMode)
                    Sub_Code.Error_Log_Write(e.Message);
                return false;
            }
        }
        //サーバー上に指定した名前のフォルダが存在するか
        public bool Directory_Exist(string To_Dir, bool IsErrorLogMode = false)
        {
            if (!IsConnected)
                return false;
            else if (!SFTP_Server.IsConnected)
                SFTP_Server.Connect();
            try
            {
                return SFTP_Server.Exists(To_Dir);
            }
            catch (Exception e)
            {
                if (IsErrorLogMode)
                    Sub_Code.Error_Log_Write(e.Message);
                return false;
            }
        }
        //サーバー上のテキストファイルに文字を追加
        public bool File_Append(string To_File, string Text,  bool IsErrorLogMode = false)
        {
            if (!IsConnected)
                return false;
            else if (!SFTP_Server.IsConnected)
                SFTP_Server.Connect();
            try
            {
                SFTP_Server.AppendAllText(To_File, Text);
                return true;
            }
            catch (Exception e)
            {
                if (IsErrorLogMode)
                    Sub_Code.Error_Log_Write(e.Message);
                return false;
            }
        }
        //サーバー上にテキストファイルを作成(引数:Textに文字を入れると作成されたファイルに記述されます)
        public bool File_Create(string To_File, bool IsOverWrite, string Text = "", bool IsErrorLogMode = false)
        {
            if (!IsConnected)
                return false;
            else if (!SFTP_Server.IsConnected)
                SFTP_Server.Connect();
            try
            {
                if (SFTP_Server.Exists(To_File) && !IsOverWrite)
                    return true;
                using (StreamWriter ostream = SFTP_Server.CreateText(To_File))
                {
                    try
                    {
                        ostream.Write(Text);
                    }
                    finally
                    {
                        ostream.Close();
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                if (IsErrorLogMode)
                    Sub_Code.Error_Log_Write(e.Message);
                return false;
            }
        }
        //サーバー上にフォルダを作成
        public bool Directory_Create(string To_Dir, bool IsErrorLogMode = false)
        {
            if (!IsConnected)
                return false;
            else if (!SFTP_Server.IsConnected)
                SFTP_Server.Connect();
            try
            {
                if (SFTP_Server.Exists(To_Dir))
                    return false;
                SFTP_Server.CreateDirectory(To_Dir);
                return true;
            }
            catch (Exception e)
            {
                if (IsErrorLogMode)
                    Sub_Code.Error_Log_Write(e.Message);
                return false;
            }
        }
        //ファイルを移動
        public bool File_Move(string From_File, string To_File, bool IsOverWrite, bool IsErrorLogMode = false)
        {
            if (!IsConnected)
                return false;
            else if (!SFTP_Server.IsConnected)
                SFTP_Server.Connect();
            try
            {
                bool IsExist = SFTP_Server.Exists(To_File);
                if (IsExist && !IsOverWrite)
                    return true;
                else if (IsExist)
                    SFTP_Server.DeleteFile(To_File);
                else if (!SFTP_Server.Exists(Path.GetDirectoryName(To_File)))
                    SFTP_Server.CreateDirectory(Path.GetDirectoryName(To_File));
                SftpFile File_Info = SFTP_Server.Get(From_File);
                File_Info.MoveTo(To_File);
                return true;
            }
            catch (Exception e)
            {
                if (IsErrorLogMode)
                    Sub_Code.Error_Log_Write(e.Message);
                return false;
            }
        }
        //フォルダを移動
        public bool Directory_Move(string From_Dir, string To_Dir, bool IsOverWrite, bool IsErrorLogMode = false)
        {
            if (!IsConnected)
                return false;
            try
            {
                foreach (string File_Now in GetFiles(From_Dir, true, false))
                {
                    string Next_Dir = Path.GetDirectoryName(File_Now).Replace(From_Dir.Replace("/", "\\"), "");
                    File_Move(File_Now, To_Dir + Next_Dir + Path.GetFileName(File_Now), IsOverWrite);
                }
                return true;
            }
            catch (Exception e)
            {
                if (IsErrorLogMode)
                    Sub_Code.Error_Log_Write(e.Message);
                return false;
            }
        }
        //サーバー上にあるフォルダを削除(サブディレクトも含む)
        public bool Directory_Delete(string To_Dir, bool IsErrorLogMode = false)
        {
            if (!IsConnected)
                return false;
            else if (!SFTP_Server.IsConnected)
                SFTP_Server.Connect();
            try
            {
                if (!SFTP_Server.Exists(To_Dir))
                    return false;
                foreach (SftpFile file in SFTP_Server.ListDirectory(To_Dir))
                {
                    if ((file.Name != ".") && (file.Name != ".."))
                    {
                        if (file.IsDirectory)
                            Directory_Delete(file.FullName);
                        else
                            SFTP_Server.DeleteFile(file.FullName);
                    }
                }
                SFTP_Server.DeleteDirectory(To_Dir);
                return true;
            }
            catch (Exception e)
            {
                if (IsErrorLogMode)
                    Sub_Code.Error_Log_Write(e.Message);
                return false;
            }
        }
        //サーバーにファイルをアップロードします
        public bool UploadFile(string From_File, string To_File, bool IsErrorLogMode = false)
        {
            if (!IsConnected)
                return false;
            else if (!SFTP_Server.IsConnected)
                SFTP_Server.Connect();
            try
            {
                using (Stream fs = File.OpenRead(From_File))
                    SFTP_Server.UploadFile(fs, To_File, true);
                return true;
            }
            catch (Exception e)
            {
                if (IsErrorLogMode)
                    Sub_Code.Error_Log_Write(e.Message);
                return false;
            }
        }
        //サーバーからファイルをダウンロードします
        bool IsDownloading = false;
        public bool DownloadFile(string From_File, string To_File, bool IsErrorLogMode = false)
        {
            if (!IsConnected)
                return false;
            else if (!SFTP_Server.IsConnected)
                SFTP_Server.Connect();
            try
            {
                IsDownloading = true;
                using (Stream fs = File.OpenWrite(To_File))
                    SFTP_Server.DownloadFile(From_File, fs, (ulong upSize) =>
                    {
                        if (!IsDownloading)
                            fs.Close();
                    });
                IsDownloading = false;
                return true;
            }
            catch (Exception e)
            {
                if (IsErrorLogMode)
                    Sub_Code.Error_Log_Write(e.Message);
                IsDownloading = false;
                return false;
            }
        }
        public void Stop_DownloadFile()
        {
            IsDownloading = false;
        }
        //サーバーのフォルダ内すべてのファイルをダウンロードします
        public bool DownloadDirectory(string From_Dir, string To_Dir, bool IsErrorLogMode = false)
        {
            if (!IsConnected)
                return false;
            else if (!SFTP_Server.IsConnected)
                SFTP_Server.Connect();
            try
            {
                if (!Directory.Exists(To_Dir))
                    Directory.CreateDirectory(To_Dir);
                IEnumerable<SftpFile> files = SFTP_Server.ListDirectory(From_Dir);
                foreach (SftpFile file in files)
                {
                    if ((file.Name != ".") && (file.Name != ".."))
                    {
                        string sourceFilePath = From_Dir + "/" + file.Name;
                        string destFilePath = Path.Combine(To_Dir, file.Name);
                        if (file.IsDirectory)
                            DownloadDirectory(sourceFilePath, destFilePath);
                        else
                            using (Stream fileStream = File.Create(destFilePath))
                                SFTP_Server.DownloadFile(sourceFilePath, fileStream);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                if (IsErrorLogMode)
                    Sub_Code.Error_Log_Write(e.Message);
                return false;
            }
        }
        //サーバーにあるファイルを削除します
        public bool DeleteFile(string To_File, bool IsErrorLogMode = false)
        {
            if (!IsConnected)
                return false;
            else if (!SFTP_Server.IsConnected)
                SFTP_Server.Connect();
            try
            {
                if (!SFTP_Server.Exists(To_File))
                    return false;
                SFTP_Server.DeleteFile(To_File);
                return true;
            }
            catch (Exception e)
            {
                if (IsErrorLogMode)
                    Sub_Code.Error_Log_Write(e.Message);
                return false;
            }
        }
    }
}