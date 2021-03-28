using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;

namespace WoTB_Voice_Mod_Creater.Multiplayer
{
    public partial class Server_Voices_Sort : UserControl
    {
        bool IsClosing = false;
        bool IsMessageShowing = false;
        int Stream;
        public Server_Voices_Sort()
        {
            InitializeComponent();
            Volume_S.Value = 50;
            Voice_Select_S.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Voice_Select_S_MouseUp), true);
        }
        async void Message_Feed_Out(string Message)
        {
            if (IsMessageShowing)
            {
                IsMessageShowing = false;
                await Task.Delay(1000 / 59);
            }
            Message_T.Text = Message;
            IsMessageShowing = true;
            Message_T.Opacity = 1;
            int Number = 0;
            while (Message_T.Opacity > 0 && IsMessageShowing)
            {
                Number++;
                if (Number >= 120)
                {
                    Message_T.Opacity -= 0.025;
                }
                await Task.Delay(1000 / 60);
            }
            IsMessageShowing = false;
            Message_T.Text = "";
            Message_T.Opacity = 1;
        }
        public async void Window_Show(bool IsFeedIn, string Message)
        {
            Voice_Select_S.Maximum = Server_Voices.Voice_List.Count - 1;
            Voice_Select_S.Value = 0;
            Voice_Select_T.Text = "1/" + Server_Voices.Voice_List.Count;
            if (IsFeedIn)
            {
                Opacity = 0;
                Visibility = System.Windows.Visibility.Visible;
                while (Opacity < 1 && !IsClosing)
                {
                    Opacity += Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
            }
            Opacity = 1;
            Visibility = System.Windows.Visibility.Visible;
            Volume_S.Focus();
            if (Message != "")
            {
                Message_Feed_Out(Message);
            }
        }
        private void Voice_Select_S_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            Voice_Select_T.Text = (int)(Voice_Select_S.Value + 1) + "/" + Server_Voices.Voice_List.Count;
        }
        private void Voice_Select_S_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Voice_Set.FTP_Server.DownloadFile(Voice_Set.Special_Path + "/Server/" + Voice_Set.SRTTbacon_Server_Name + "/Voices/" + Server_Voices.Voice_List[(int)Voice_Select_S.Value],
                    "/WoTB_Voice_Mod/Voice_Online/" + Voice_Set.SRTTbacon_Server_Name + "/Voices/" + Server_Voices.Voice_List[(int)Voice_Select_S.Value]);
            }
            catch (Exception e1)
            {
                Sub_Code.Error_Log_Write(e1.Message);
            }
        }
        private void UserControl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            MessageBox.Show("テスト");
        }
        private async void Exit_B_Click(object sender, RoutedEventArgs e)
        {
            if (!IsClosing)
            {
                IsClosing = true;
                while (Opacity > 0)
                {
                    Opacity -= Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
                Visibility = Visibility.Hidden;
                Bass.BASS_ChannelStop(Stream);
                Bass.BASS_StreamFree(Stream);
                IsClosing = false;
            }
        }
        private void Play_B_Click(object sender, RoutedEventArgs e)
        {
            Bass.BASS_ChannelStop(Stream);
            Bass.BASS_StreamFree(Stream);
            int StreamHandle = Bass.BASS_StreamCreateFile(Voice_Set.Special_Path + "/Server/" + Voice_Set.SRTTbacon_Server_Name + "/Voices/" + Server_Voices.Voice_List[(int)Voice_Select_S.Value], 0, 0, BASSFlag.BASS_STREAM_DECODE);
            Stream = BassFx.BASS_FX_TempoCreate(StreamHandle, BASSFlag.BASS_FX_FREESOURCE);
            Bass.BASS_ChannelSetDevice(Stream, Video_Mode.Sound_Device);
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)(Volume_S.Value / 100));
            Bass.BASS_ChannelPlay(Stream, true);
        }
        private void Stop_B_Click(object sender, RoutedEventArgs e)
        {
            Bass.BASS_ChannelStop(Stream);
        }
        private void Volume_S_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)(Volume_S.Value / 100));
            Volume_T.Text = "音量:" + (int)Volume_S.Value;
        }
    }
}