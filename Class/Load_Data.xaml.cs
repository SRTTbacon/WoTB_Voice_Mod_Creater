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
        readonly string Special_Path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/WoTB_Voice_Mod_Creater";
        bool IsLoading = false;
        public Load_Data()
        {
            InitializeComponent();
        }
        public async void Window_Start()
        {
            IsLoading = true;
            Visibility = Visibility.Visible;
            if (!File.Exists(Special_Path + "/Loading/1.png"))
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
                    Load_Image.Source = new BitmapImage(new Uri(Special_Path + "/Loading/" + Number_01 + ".png"));
                }
                else
                {
                    Load_Image.Source = new BitmapImage(new Uri(Special_Path + "/Loading/148.png"));
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