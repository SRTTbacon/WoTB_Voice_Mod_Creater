using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WoTB_Voice_Mod_Creater.Class
{
    public partial class Message : UserControl
    {
        bool IsClosing = false;
        public Message()
        {
            InitializeComponent();
        }
        public async void Window_Show()
        {
            Opacity = 0;
            if (Voice_Set.FTP_Server.FileExists("/WoTB_Voice_Mod/Message.dat"))
            {
                StreamReader str = new StreamReader(Voice_Set.FTP_Server.OpenRead("/WoTB_Voice_Mod/Message.dat"));
                string Read = str.ReadToEnd();
                str.Close();
                Message_T.Text = Read + "\nCreated by SRTTbacon(introwonderful#ベーコン)";
            }
            else
            {
                Message_T.Text = "現在作者からのメッセージはありません。\nCreated by SRTTbacon(introwonderful#ベーコン)";
            }
            Visibility = System.Windows.Visibility.Visible;
            while (Opacity < 1 && !IsClosing)
            {
                Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
        }
        private async void Exit_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!IsClosing)
            {
                IsClosing = true;
                while (Opacity > 0)
                {
                    Opacity -= Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
                IsClosing = false;
                Visibility = System.Windows.Visibility.Hidden;
            }
        }
    }
}