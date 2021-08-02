﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace WoTB_Voice_Mod_Creater
{
    public partial class MainCode : Window
    {
        //レイアウトを更新
        async void IsConnecting()
        {
            while (true)
            {
                if (Connectiong != Voice_Set.FTPClient.IsConnected && !IsClosing)
                {
                    Connectiong = Voice_Set.FTPClient.IsConnected;
                    Connect_Mode_Layout();
                    Message_T.Text = "サーバーとの接続が切断されました。";
                    break;
                }
                await Task.Delay(1000);
            }
        }
        //アカウントが存在するか
        bool Account_Exist(string UserName, string Password)
        {
            StreamReader streamReader = Voice_Set.FTPClient.GetFileRead("/WoTB_Voice_Mod/Accounts.dat");
            while (streamReader.EndOfStream == false)
            {
                string Line = streamReader.ReadLine();
                if (Line.Contains(":"))
                {
                    string UserName_Line = Line.Substring(0, Line.IndexOf(':'));
                    string Password_Line = Line.Substring(Line.IndexOf(':') + 1);
                    if (UserName_Line == UserName && Password_Line == Password)
                    {
                        streamReader.Close();
                        return true;
                    }
                }
            }
            streamReader.Dispose();
            return false;
        }
        //ユーザー名が既に存在するか
        bool UserExist(string UserName)
        {
            StreamReader streamReader = Voice_Set.FTPClient.GetFileRead("/WoTB_Voice_Mod/Accounts.dat");
            while (streamReader.EndOfStream == false)
            {
                string Line = streamReader.ReadLine();
                if (Line.Contains(":"))
                {
                    string UserName_Line = Line.Substring(0, Line.IndexOf(':'));
                    if (UserName_Line == UserName)
                    {
                        streamReader.Close();
                        return true;
                    }
                }
            }
            streamReader.Dispose();
            return false;
        }
        //管理者が設定を変更した場合それを反映
        void TCP_Change_Config(string[] Message_Temp)
        {
            try
            {
                string Server_Rename = Message_Temp[2].Replace("\0", "");
                if (Voice_Set.SRTTbacon_Server_Name != Server_Rename)
                {
                    Directory.Move(Voice_Set.Special_Path + "/Server/" + Voice_Set.SRTTbacon_Server_Name, Voice_Set.Special_Path + "/Server/" + Server_Rename);
                    Voice_Set.SRTTbacon_Server_Name = Server_Rename;
                }
                if (Voice_Set.FTPClient.File_Exist("/WoTB_Voice_Mod/" + Voice_Set.SRTTbacon_Server_Name + "/Remove_Files.dat"))
                {
                    StreamReader str = Voice_Set.FTPClient.GetFileRead("/WoTB_Voice_Mod/" + Voice_Set.SRTTbacon_Server_Name + "/Remove_Files.dat");
                    string[] Read = str.ReadToEnd().Split('\n');
                    str.Dispose();
                    foreach (string Line in Read)
                    {
                        if (Line != "")
                        {
                            try
                            {
                                File.Delete(Voice_Set.Special_Path + "/Server/" + Voice_Set.SRTTbacon_Server_Name + "/Voices/" + Line);
                            }
                            catch
                            {
                            }
                        }
                    }
                }
                if (Voice_Set.FTPClient.File_Exist("/WoTB_Voice_Mod/" + Voice_Set.SRTTbacon_Server_Name + "/Add_Files.dat"))
                {
                    bool IsChanged = false;
                    StreamReader str = Voice_Set.FTPClient.GetFileRead("/WoTB_Voice_Mod/" + Voice_Set.SRTTbacon_Server_Name + "/Add_Files.dat");
                    string[] Read = str.ReadToEnd().Split('\n');
                    str.Dispose();
                    foreach (string Line in Read)
                    {
                        if (Line != "")
                        {
                            try
                            {
                                Voice_Set.FTPClient.DownloadFile("/WoTB_Voice_Mod/" + Voice_Set.SRTTbacon_Server_Name + "/Voices/" + Line, Voice_Set.Special_Path + "/Server/" + Voice_Set.SRTTbacon_Server_Name + "/Voices/" + Line);
                                IsChanged = true;
                            }
                            catch
                            {
                            }
                        }
                    }
                    if (IsChanged)
                    {
                        string[] Temp = Directory.GetFiles(Voice_Set.Special_Path + "/Server/" + Voice_Set.SRTTbacon_Server_Name + "/Voices");
                        List<string> Temp_01 = new List<string>();
                        for (int Number = 0; Number <= Temp.Length - 1; Number++)
                            if (!Voice_Set.Voice_Name_Hide(Temp[Number]))
                                Temp_01.Add(System.IO.Path.GetFileName(Temp[Number]));
                        Voice_Set.Voice_Files = Temp_01;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("エラー:" + e.Message);
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
    }
}