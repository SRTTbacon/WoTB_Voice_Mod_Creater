using System.IO;
using System.Windows.Controls;
using System.Windows;
using System.Threading.Tasks;
using System;
using System.Windows.Media.Imaging;
using System.Net;
using YoutubeExplode;
using System.Collections.Generic;
using System.Linq;
using YoutubeExplode.Videos.Streams;

namespace WoTB_Voice_Mod_Creater.Class
{
    public partial class Youtube_Link : UserControl
    {
        bool IsClosing = false;
        bool IsMessageShowing = false;
        bool IsSaveOK = false;
        public Youtube_Link()
        {
            InitializeComponent();
            Type_L.Items.Add("音声のみを保存");
            Type_L.Items.Add("動画も含めて保存");
        }
        //画面を表示
        public async void Window_Show()
        {
            Sub_Code.AutoListAdd.Clear();
            Opacity = 0;
            Visibility = Visibility.Visible;
            Save_Destination_T.Text = Directory.GetCurrentDirectory() + "\\Youtube\\";
            if (File.Exists(Voice_Set.Special_Path + "/Download_Location.dat"))
            {
                try
                {
                    using (var eifs = new FileStream(Voice_Set.Special_Path + "/Configs/Download_Location.dat", FileMode.Open, FileAccess.Read))
                    {
                        using (var eofs = new FileStream(Voice_Set.Special_Path + "/Configs/Temp_Location.dat", FileMode.Create, FileAccess.Write))
                        {
                            FileEncode.FileEncryptor.Decrypt(eifs, eofs, "Youtube_Download_Location_Save");
                        }
                    }
                    StreamReader str = new StreamReader(Voice_Set.Special_Path + "/Configs/Temp_Location.dat");
                    Save_Destination_T.Text = str.ReadLine();
                    Type_L.SelectedIndex = int.Parse(str.ReadLine());
                    List_Add_C.IsChecked = bool.Parse(str.ReadLine());
                    Close_C.IsChecked = bool.Parse(str.ReadLine());
                    str.Close();
                    File.Delete(Voice_Set.Special_Path + "/Configs/Temp_Location.dat");
                }
                catch (Exception e)
                {
                    Message_Feed_Out("保存先を取得できませんでした。");
                    File.Delete(Voice_Set.Special_Path + "/Configs/Download_Location.dat");
                    Type_L.SelectedIndex = 0;
                    Sub_Code.Error_Log_Write(e.Message);
                }
            }
            else
            {
                Type_L.SelectedIndex = 0;
            }
            IsSaveOK = true;
            while (Opacity < 1 && !IsClosing)
            {
                Opacity += 0.025;
                await Task.Delay(1000 / 60);
            }
        }
        async void Message_Feed_Out(string Message)
        {
            //テキストが一定期間経ったらフェードアウト
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
        private void Save_Destination_B_Click(object sender, RoutedEventArgs e)
        {
            //保存場所を変更
            if (IsClosing)
            {
                return;
            }
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog()
            {
                Title = "フォルダを選択してください。",
                Filter = "フォルダを選択(Folder;)|",
                FileName = "フォルダを指定",
                CheckFileExists = false,
                Multiselect = false
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Save_Destination_T.Text = Path.GetDirectoryName(ofd.FileName) + "\\";
                Configs_Save();
            }
        }
        private void Type_L_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Configs_Save();
        }
        //現在の設定を保存
        void Configs_Save()
        {
            if (!IsSaveOK)
            {
                return;
            }
            try
            {
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Configs/Temp_Location.dat");
                stw.WriteLine(Save_Destination_T.Text);
                stw.WriteLine(Type_L.SelectedIndex);
                stw.WriteLine(List_Add_C.IsChecked.Value);
                stw.Write(Close_C.IsChecked.Value);
                stw.Close();
                using (var eifs = new FileStream(Voice_Set.Special_Path + "/Configs/Temp_Location.dat", FileMode.Open, FileAccess.Read))
                {
                    using (var eofs = new FileStream(Voice_Set.Special_Path + "/Configs/Download_Location.dat", FileMode.Create, FileAccess.Write))
                    {
                        FileEncode.FileEncryptor.Encrypt(eifs, eofs, "Youtube_Download_Location_Save");
                    }
                }
                File.Delete(Voice_Set.Special_Path + "/Configs/Temp_Location.dat");
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        private void Back_B_Click(object sender, RoutedEventArgs e)
        {
            Exit();
        }
        //閉じる
        async void Exit()
        {
            if (!IsClosing)
            {
                IsClosing = true;
                while (Opacity > 0)
                {
                    Opacity -= 0.025;
                    await Task.Delay(1000 / 60);
                }
                Visibility = Visibility.Hidden;
                IsClosing = false;
            }
        }
        //保存をクリック
        private async void Download_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing || Opacity < 1)
            {
                return;
            }
            IsClosing = true;
            try
            {
                string ID = "aaa";
                if (Link_T.Text.Contains("youtu.be/"))
                {
                    ID = Link_T.Text.Substring(Link_T.Text.LastIndexOf('/') + 1, 11);
                }
                else if (Link_T.Text.Contains("youtube.com/watch"))
                {
                    ID = Link_T.Text.Substring(Link_T.Text.IndexOf('=') + 1, 11);
                }
                if (ID == "aaa")
                {
                    Message_Feed_Out("動画を取得できませんでした。");
                    IsClosing = false;
                    return;
                }
                if (!Directory.Exists(Save_Destination_T.Text))
                {
                    Directory.CreateDirectory(Save_Destination_T.Text);
                }
                Message_T.Text = "動画を取得しています...";
                await Task.Delay(50);
                await Video_Download(Link_T.Text, ID, Save_Destination_T.Text, Message_T);
                Message_Feed_Out("保存しました。");
                if (Close_C.IsChecked.Value)
                {
                    IsClosing = false;
                    Exit();
                    return;
                }
            }
            catch (Exception e1)
            {
                Message_Feed_Out("保存できませんでした。");
                Sub_Code.Error_Log_Write(e1.Message);
            }
            IsClosing = false;
        }
        async Task Video_Download(string Link, string Link_ID_Only, string OutDir, TextBlock Message_T)
        {
            //動画と音声を別々にダウンロード
            //動画の場合はダウンロード後ffmpegで合わせる
            var youtube = new YoutubeClient();
            var video = await youtube.Videos.GetAsync(Link);
            var title = Sub_Code.File_Replace_Name(video.Title);
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(Link_ID_Only);
            var streamInfo = streamManifest.GetAudioOnly().WithHighestBitrate();
            if (streamInfo != null)
            {
                Message_T.Text = "音声を取得しています...";
                await Task.Delay(50);
                var stream = await youtube.Videos.Streams.GetAsync(streamInfo);
                await youtube.Videos.Streams.DownloadAsync(streamInfo, OutDir + "Temp.mp3");
            }
            if (Type_L.SelectedIndex == 1)
            {
                Message_T.Text = "動画を取得しています...";
                await Task.Delay(50);
                var streamInfo2 = streamManifest.GetVideoOnly().Where(s => s.Container == Container.Mp4).WithHighestVideoQuality();
                if (streamInfo2 != null)
                {
                    var stream = await youtube.Videos.Streams.GetAsync(streamInfo2);
                    await youtube.Videos.Streams.DownloadAsync(streamInfo2, OutDir + "Temp.mp4");
                    Message_T.Text = "動画と音声を結合しています...";
                    await Task.Delay(50);
                    Sub_Code.Audio_Video_Convert(OutDir + "Temp.mp4", OutDir + "Temp.mp3", OutDir + title + ".mp4");
                    if (List_Add_C.IsChecked.Value)
                    {
                        Sub_Code.AutoListAdd.Add(OutDir + title + ".mp4");
                    }
                }
                else
                {
                    Message_Feed_Out("動画を取得できなかったため音声のみ保存しました。");
                    Sub_Code.File_Move(OutDir + "Temp.mp3", OutDir + title + ".mp3", true);
                    if (List_Add_C.IsChecked.Value)
                    {
                        Sub_Code.AutoListAdd.Add(OutDir + title + ".mp3");
                    }
                }
            }
            else
            {
                Sub_Code.File_Move(OutDir + "Temp.mp3", OutDir + title + ".mp3", true);
                if (List_Add_C.IsChecked.Value)
                {
                    Sub_Code.AutoListAdd.Add(OutDir + title + ".mp3");
                }
            }
            File.Delete(OutDir + "Temp.mp3");
            File.Delete(OutDir + "Temp.mp4");
        }
        private void Link_T_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsClosing)
            {
                return;
            }
            try
            {
                string ID = "aaa";
                if (Link_T.Text.Contains("youtu.be/"))
                {
                    ID = Link_T.Text.Substring(Link_T.Text.LastIndexOf('/') + 1, 11);
                }
                else if (Link_T.Text.Contains("youtube.com/watch"))
                {
                    ID = Link_T.Text.Substring(Link_T.Text.IndexOf('=') + 1, 11);
                }
                //サムネイル画像を取得
                BitmapImage image = new BitmapImage();
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://img.youtube.com/vi/" + ID + "/maxresdefault.jpg");
                req.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows 10)";
                req.Referer = "http://www.ipentec.com/index.html";
                WebResponse res = req.GetResponse();
                Stream st = res.GetResponseStream();
                byte[] buffer = new byte[65535];
                MemoryStream ms = new MemoryStream();
                while (true)
                {
                    int rb = st.Read(buffer, 0, buffer.Length);
                    if (rb > 0)
                    {
                        ms.Write(buffer, 0, rb);
                    }
                    else
                    {
                        break;
                    }
                }
                image.BeginInit();
                ms.Seek(0, SeekOrigin.Begin);
                image.StreamSource = ms;
                image.EndInit();
                Thumbnail_Image.Source = image;
                Thumbnail_Image.Visibility = Visibility.Visible;
            }
            catch
            {
                if (Thumbnail_Image != null)
                {
                    Thumbnail_Image.Visibility = Visibility.Hidden;
                }
            }
        }
        private void List_Add_C_Click(object sender, RoutedEventArgs e)
        {
            Configs_Save();
        }
        private void Close_C_Click(object sender, RoutedEventArgs e)
        {
            Configs_Save();
        }
    }
}