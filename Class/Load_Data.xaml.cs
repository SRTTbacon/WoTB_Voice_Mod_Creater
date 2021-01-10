using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace WoTB_Voice_Mod_Creater.Class
{
    public partial class Load_Data : UserControl
    {
        bool IsLoading = false;
        public Load_Data()
        {
            InitializeComponent();
        }
        public async void Window_Start()
        {
            IsLoading = true;
            //一時ファイルの保存先を変更している場合それを適応
            if (File.Exists(Directory.GetCurrentDirectory() + "/TempDirPath.dat"))
            {
                try
                {
                    using (var eifs = new FileStream(Directory.GetCurrentDirectory() + "/TempDirPath.dat", FileMode.Open, FileAccess.Read))
                    {
                        using (var eofs = new FileStream(Directory.GetCurrentDirectory() + "/Temp.dat", FileMode.Create, FileAccess.Write))
                        {
                            FileEncode.FileEncryptor.Decrypt(eifs, eofs, "Temp_Directory_Path_Pass");
                        }
                    }
                    StreamReader str = new StreamReader(Directory.GetCurrentDirectory() + "/Temp.dat");
                    string Read = str.ReadLine();
                    str.Close();
                    File.Delete(Directory.GetCurrentDirectory() + "/Temp.dat");
                    if (Read != "")
                    {
                        Voice_Set.Special_Path = Read;
                    }
                }
                catch
                {

                }
            }
            Visibility = Visibility.Visible;
            if (!File.Exists(Voice_Set.Special_Path + "/Loading/1.png"))
            {
                DVPL.Loading_Extract();
            }
            int Number_01 = 1;
            int Number_02 = 0;
            int Number_03 = 0;
            while (IsLoading)
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
                if (Number_02 % 30 == 1)
                {
                    if (Number_03 == 0)
                    {
                        Load_Text.Text = "必要なデータをロードしています.";
                        Number_03++;
                    }
                    else if (Number_03 == 1)
                    {
                        Load_Text.Text = "必要なデータをロードしています..";
                        Number_03++;
                    }
                    else if (Number_03 == 2)
                    {
                        Load_Text.Text = "必要なデータをロードしています...";
                        Number_03 = 0;
                    }
                }
                Number_01++;
                Number_02++;
                await Task.Delay(1000 / 30);
            }
        }
        public void Window_Stop()
        {
            IsLoading = false;
            Visibility = Visibility.Hidden;
        }
    }
}