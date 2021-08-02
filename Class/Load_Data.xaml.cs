using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

public class Download_Data_File
{
    public static long Download_Total_Size { get; set; }
    public static List<string> Download_File_Path = new List<string>();
}
namespace WoTB_Voice_Mod_Creater.Class
{
    public partial class Load_Data : UserControl
    {
        bool IsLoading = false;
        bool IsExtracting = false;
        public Load_Data()
        {
            InitializeComponent();
        }
        public async void Window_Start(string Message)
        {
            IsLoading = true;
            Visibility = Visibility.Visible;
            int Number_01 = 1;
            int Number_02 = 0;
            int Number_03 = 0;
            while (IsLoading)
            {
                if (File.Exists(Voice_Set.Special_Path + "/Loading/148.png"))
                {
                    try
                    {
                        if (Number_01 < 148)
                        {
                            MemoryStream data = new MemoryStream(File.ReadAllBytes(Voice_Set.Special_Path + "/Loading/" + Number_01 + ".png"));
                            WriteableBitmap wbmp = new WriteableBitmap(BitmapFrame.Create(data));
                            data.Close();
                            Load_Image.Source = wbmp;
                        }
                        else
                        {
                            MemoryStream data = new MemoryStream(File.ReadAllBytes(Voice_Set.Special_Path + "/Loading/148.png"));
                            WriteableBitmap wbmp = new WriteableBitmap(BitmapFrame.Create(data));
                            data.Close();
                            Load_Image.Source = wbmp;
                            Number_01 = 0;
                        }
                    }
                    catch
                    {

                    }
                }
                if (Number_02 % 30 == 1)
                {
                    if (Number_03 == 0)
                    {
                        Load_Text.Text = Message + ".";
                        Number_03++;
                    }
                    else if (Number_03 == 1)
                    {
                        Load_Text.Text = Message + "..";
                        Number_03++;
                    }
                    else if (Number_03 == 2)
                    {
                        Load_Text.Text = Message + "...";
                        Number_03 = 0;
                    }
                    if (IsExtracting)
                    {
                        Load_Text.Text += "\n" + "ファイルを展開しています...";
                    }
                    else if (Download_Data_File.Download_File_Path.Count > 0)
                    {
                        try
                        {
                            ulong Download_Size_Now = 0;
                            foreach (string File_Path in Download_Data_File.Download_File_Path)
                            {
                                FileInfo fi = new FileInfo(File_Path);
                                Download_Size_Now += (ulong)fi.Length;
                            }
                            if (Download_Size_Now >= (ulong)Download_Data_File.Download_Total_Size)
                                IsExtracting = true;
                            else
                                Load_Text.Text += "\n" + Math.Round(Download_Size_Now / 1024.0 / 1024.0, 1, MidpointRounding.AwayFromZero) + "MB/" + Math.Round(Download_Data_File.Download_Total_Size / 1024.0 / 1024.0, 1, MidpointRounding.AwayFromZero) + "MB";
                        }
                        catch
                        {
                            
                        }
                    }
                }
                Number_01++;
                Number_02++;
                await Task.Delay(1000 / 30);
            }
            IsExtracting = false;
        }
        public void Window_Stop()
        {
            IsLoading = false;
            Visibility = Visibility.Hidden;
        }
    }
}