//using DdsFileTypePlus;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WoTB_Voice_Mod_Creater.Class
{
    public partial class DDS_Tool : UserControl
    {
        bool IsClosing = false;
        bool IsMessageShowing = false;
        string Select_File = "";
        ImageFormat[] Image_01 = { ImageFormat.Png, ImageFormat.Jpeg, ImageFormat.Bmp, ImageFormat.Gif, ImageFormat.Tiff, ImageFormat.Emf, ImageFormat.Exif, ImageFormat.Icon, ImageFormat.Wmf };
        //DdsFileFormat[] Image_02 = { DdsFileFormat.BC1, DdsFileFormat.BC2, DdsFileFormat.BC3, DdsFileFormat.BC4, DdsFileFormat.BC5, DdsFileFormat.BC6H, DdsFileFormat.BC7 };
        public DDS_Tool()
        {
            InitializeComponent();
            Encode_L.Items.Add("PNG");
            Encode_L.Items.Add("JPG");
            Encode_L.Items.Add("BMP");
            Encode_L.Items.Add("GIF");
            Encode_L.Items.Add("TIFF");
            Encode_L.Items.Add("EMF");
            Encode_L.Items.Add("EXIF");
            Encode_L.Items.Add("ICO");
            Encode_L.Items.Add("WMF");
            /*Encode_L.Items.Add("DDS(BC1)");
            Encode_L.Items.Add("DDS(BC2)");
            Encode_L.Items.Add("DDS(BC3)");
            Encode_L.Items.Add("DDS(BC4)");
            Encode_L.Items.Add("DDS(BC5)");
            Encode_L.Items.Add("DDS(BC6)");
            Encode_L.Items.Add("DDS(BC7)");*/
        }
        public async void Window_Show()
        {
            Opacity = 0;
            Visibility = Visibility.Visible;
            while (Opacity < 1 && !IsClosing)
            {
                Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
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
            Message_T.Text = "";
            Message_T.Opacity = 1;
            IsMessageShowing = false;
        }
        private void DDS_Main_Image_DragOver(object sender, DragEventArgs e)
        {
            string[] Drag_Files = (string[])e.Data.GetData(DataFormats.FileDrop);
            string Ex = Path.GetExtension(Drag_Files[0]);
            if (Ex == ".png" || Ex == ".jpg" || Ex == ".jpeg" || Ex == ".bmp" || Ex == ".gif" || Ex == ".tiff" || Ex == ".exif")
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }
        private void DDS_Main_Image_Drop(object sender, DragEventArgs e)
        {
            if (IsClosing)
            {
                return;
            }
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                try
                {
                    string[] Drop_Files = e.Data.GetData(DataFormats.FileDrop) as string[];
                    string Ex = Path.GetExtension(Drop_Files[0]);
                    if (Ex == ".png" || Ex == ".jpg" || Ex == ".jpeg" || Ex == ".bmp" || Ex == ".gif" || Ex == ".tiff" || Ex == ".exif")
                    {
                        if (Ex == ".dds")
                        {
                            //DDS_Main_Image.Source = Sub_Code.Bitmap_To_BitmapImage(DDS_Format.Load_To_Bitmap(Drop_Files[0]));
                        }
                        else
                        {
                            DDS_Main_Image.Source = Sub_Code.Bitmap_To_BitmapImage(new Bitmap(Drop_Files[0]));
                        }
                        File_Select_T.Text = Path.GetFileName(Drop_Files[0]);
                        Select_File = Drop_Files[0];
                        if (Encode_L.SelectedIndex != -1)
                        {
                            string Ex_After = "";
                            if (Encode_L.SelectedIndex < 9)
                            {
                                Ex_After = Encode_L.SelectedItem.ToString().ToLower();
                            }
                            else
                            {
                                Ex_After = "dds";
                            }
                            Encode_T.Text = Ex + " -> ." + Ex_After;
                        }
                    }
                    else
                    {
                        Message_Feed_Out("画像ファイルをドラッグしてください。");
                    }
                }
                catch (Exception e1)
                {
                    Message_Feed_Out("画像ファイルを読み込めませんでした。");
                    Sub_Code.Error_Log_Write(e1.Message);
                }
            }
        }
        private void File_Open_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
            {
                return;
            }
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog()
            {
                Title = "画像ファイルを選択してください。",
                Multiselect = false,
                Filter = "画像ファイル(*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tiff;*.exif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tiff;*.exif"
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    if (Path.GetExtension(ofd.FileName) == ".dds")
                    {
                        //DDS_Main_Image.Source = Sub_Code.Bitmap_To_BitmapImage(DDS_Format.Load_To_Bitmap(ofd.FileName));
                    }
                    else
                    {
                        DDS_Main_Image.Source = Sub_Code.Bitmap_To_BitmapImage(new Bitmap(ofd.FileName));
                    }
                    File_Select_T.Text = Path.GetFileName(ofd.FileName);
                    Select_File = ofd.FileName;
                }
                catch (Exception e1)
                {
                    Message_Feed_Out("画像ファイルを読み込めませんでした。");
                    Sub_Code.Error_Log_Write(e1.Message);
                }
            }
        }
        private void Encode_L_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Encode_L.SelectedIndex == -1)
            {
                return;
            }
            string Ex;
            if (Encode_L.SelectedIndex < 9)
            {
                Ex = Encode_L.SelectedItem.ToString().ToLower();
            }
            else
            {
                Ex = "dds";
            }
            Encode_T.Text = Path.GetExtension(Select_File) + " -> ." + Ex;
        }
        private void Encode_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
            {
                return;
            }
            if (Select_File == "")
            {
                Message_Feed_Out("画像ファイルが選択されていません。");
                return;
            }
            if (!File.Exists(Select_File))
            {
                Message_Feed_Out("指定された画像ファイルが存在しません。");
                return;
            }
            if (Encode_L.SelectedIndex == -1)
            {
                Message_Feed_Out("変換形式が選択されていません。");
            }
            string File_Ex;
            if (Encode_L.SelectedIndex >= 9)
            {
                File_Ex = "dds";
            }
            else
            {
                File_Ex = Encode_L.SelectedItem.ToString().ToLower();
            }
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog()
            {
                Title = "保存先を指定してください。",
                Filter = "画像ファイル(*." + File_Ex + ")|*." + File_Ex
            };
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    string Ex = Path.GetExtension(Select_File);
                    /*if (Encode_L.SelectedIndex >= 9 && Ex == ".dds")
                    {
                        DDS_Format.DDS_To_DDS(Select_File, sfd.FileName, Image_02[Encode_L.SelectedIndex - 9]);
                    }
                    else if (Ex == ".dds" && Encode_L.SelectedIndex < 9)
                    {
                        DDS_Format.DDS_To_PNG(Select_File, sfd.FileName, Image_01[Encode_L.SelectedIndex]);
                    }
                    else if (Ex != ".dds" && Encode_L.SelectedIndex >= 9)
                    {
                        DDS_Format.PNG_To_DDS(Select_File, sfd.FileName, Image_02[Encode_L.SelectedIndex - 9]);
                    }*/
                    if (Ex != ".dds" && Encode_L.SelectedIndex < 9)
                    {
                        Bitmap bitmap = new Bitmap(Select_File);
                        bitmap.Save(sfd.FileName, Image_01[Encode_L.SelectedIndex]);
                        bitmap.Dispose();
                        //DDS_Format.PNG_To_PNG(Select_File, sfd.FileName, Image_01[Encode_L.SelectedIndex]);
                    }
                    else
                    {
                        throw new Exception("画像形式が正しくありません。開発者へご連絡ください。");
                    }
                    Message_Feed_Out("画像を変換しました。");
                }
                catch (Exception e1)
                {
                    Message_Feed_Out("画像を変換できませんでした。");
                    Sub_Code.Error_Log_Write(e1.Message);
                }
            }
        }
        private void Help_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosing)
            {
                return;
            }
            string Message_01 = "WoTBのDDSはBC3形式です。\n";
            string Message_02 = "一部の形式でしかテストしていないためすべて正常に動くかはわかりません。\n";
            string Message_03 = "画像はドラッグ&ドロップでも指定できます。";
            MessageBox.Show(Message_01 + Message_02 + Message_03);
        }
        private async void Back_B_Click(object sender, RoutedEventArgs e)
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
                Visibility = Visibility.Hidden;
                DDS_Main_Image.Source = null;
                Encode_T.Text = "";
                File_Select_T.Text = "";
                Select_File = "";
                Encode_L.SelectedIndex = -1;
            }
        }
    }
}