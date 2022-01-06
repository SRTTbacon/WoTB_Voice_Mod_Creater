using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;

namespace WoTB_Voice_Mod_Creater.Class.Texture_Editor
{
    public class Texture_Property_Info
    {
        public delegate void CheckBoxEventHandler<T>(T args);
        public event CheckBoxEventHandler<bool> ChangeCheckBox;
        public Canvas Parent { get; set; }
        public Slider Opacity { get; set; }
        public TextBlock File_Name { get; set; }
        public System.Windows.Controls.Image Visibility { get; set; }
        public System.Windows.Controls.Image Texture { get; set; }
        public Border Texture_Border { get; set; }
        public bool IsVisibility { get; set; }
        public Texture_Property_Info(Canvas Parent, Slider Opacity, TextBlock File_Name, System.Windows.Controls.Image Visibility, System.Windows.Controls.Image Texture, Border Texture_Border)
        {
            this.Parent = Parent;
            this.Opacity = Opacity;
            this.File_Name = File_Name;
            this.Visibility = Visibility;
            this.Texture = Texture;
            this.Texture_Border = Texture_Border;
            IsVisibility = true;
            Visibility.MouseDown += delegate
            {
                if (IsVisibility)
                {
                    IsVisibility = false;
                    Visibility.Source = Sub_Code.Check_02;
                    ChangeCheckBox(false);
                }
                else
                {
                    IsVisibility = true;
                    Visibility.Source = Sub_Code.Check_04;
                    ChangeCheckBox(true);
                }
            };
        }
    }
    public class Texture_Image_Info
    {
        public System.Drawing.Bitmap Texture_Main { get; private set; }
        public System.Drawing.Bitmap Texture_Change { get; set; }
        public System.Windows.Controls.Image Texture_Image { get; set; }
        public System.Drawing.Point Init_Position { get; set; }
        public System.Drawing.Point Position { get; set; }
        public int Width
        {
            get { return Texture_Change.Width; }
        }
        public int Height
        {
            get { return Texture_Change.Height; }
        }
        public int Priority { get; set; }
        public System.Drawing.Point Left_UP_Position { get; set; }
        public System.Drawing.Point Right_UP_Position { get; set; }
        public System.Drawing.Point Left_Down_Position { get; set; }
        public Texture_Image_Info(System.Drawing.Bitmap Texture_Main, System.Windows.Controls.Image Texture_Image, System.Drawing.Point Position, int Priority)
        {
            this.Texture_Main = Texture_Main;
            this.Texture_Image = Texture_Image;
            this.Position = Position;
            this.Init_Position = Position;
            this.Priority = Priority;
            Texture_Change = (System.Drawing.Bitmap)Texture_Main.Clone();
            Left_UP_Position = new System.Drawing.Point(0, 0);
            Right_UP_Position = new System.Drawing.Point(0, 0);
            Left_Down_Position = new System.Drawing.Point(0, 0);
        }
    }
    public class Texture_Info
    {
        public Texture_Property_Info Property { get; set; }
        public Texture_Image_Info Texture_Image { get; set; }
        public int ID { get; private set; }
        public Texture_Info(Texture_Property_Info Property, Texture_Image_Info Texture_Image, int ID)
        {
            this.Property = Property;
            this.Texture_Image = Texture_Image;
            this.ID = ID;
        }
    }
    public class Texture_Core
    {
        public List<Texture_Info> All_Textures = new List<Texture_Info>();
        public double Zoom = 1.0;
        public bool IsInited = false;
        Style Slider_Style_Yoko = null;
        double Paint_Width = 0;
        double Paint_Height = 0;
        public Texture_Core(Style Slider_Style, Vector Paint_Size)
        {
            Slider_Style_Yoko = Slider_Style;
            Paint_Width = Paint_Size.X;
            Paint_Height = Paint_Size.Y;
            IsInited = true;
        }
        public Texture_Info Add_Texture(string File, System.Drawing.Point Init_Position, int Property_Position, bool IsCenterMode)
        {
            if (!IsInited)
                return null;
            try
            {
                int ID = Sub_Code.r.Next(0, int.MaxValue);
                Bitmap Texture_BMP = null;
                BitmapImage Texture = null;
                if (Path.GetExtension(File) == ".dds")
                {
                    //Texture_BMP = DDS_Format.Load_To_Bitmap(File);
                    //Texture = Sub_Code.Bitmap_To_BitmapImage(Texture_BMP);
                }
                else
                {
                    Texture = new BitmapImage(new Uri(File));
                    Texture_BMP = new Bitmap(File);
                }
                Canvas Parent = new Canvas();
                Parent.Name = "Property_Parent_" + ID;
                Parent.VerticalAlignment = VerticalAlignment.Top;
                Parent.HorizontalAlignment = HorizontalAlignment.Left;
                Parent.Width = 399;
                Parent.Height = 95;
                Parent.Focusable = false;
                Parent.Background = System.Windows.Media.Brushes.Transparent;
                Parent.Margin = new Thickness(0, 95 * Property_Position, 0, 0);
                Slider Opacity = new Slider();
                Opacity.Name = "Opacity_Slider_" + ID;
                Opacity.VerticalAlignment = VerticalAlignment.Top;
                Opacity.HorizontalAlignment = HorizontalAlignment.Left;
                Opacity.Width = 200;
                Opacity.Height = 25;
                Opacity.Focusable = false;
                Opacity.Style = Slider_Style_Yoko;
                Opacity.Maximum = 1;
                Opacity.Minimum = 0;
                Opacity.Value = 1;
                Opacity.Margin = new Thickness(150, 70, 0, 0);
                TextBlock File_Name = new TextBlock();
                File_Name.Name = "Texture_Name_" + ID;
                File_Name.Text = Path.GetFileName(File);
                File_Name.VerticalAlignment = VerticalAlignment.Top;
                File_Name.HorizontalAlignment = HorizontalAlignment.Left;
                File_Name.Width = 260;
                File_Name.Height = 70;
                File_Name.Focusable = false;
                File_Name.Foreground = System.Windows.Media.Brushes.Aqua;
                File_Name.FontSize = 25;
                File_Name.TextWrapping = TextWrapping.Wrap;
                File_Name.TextAlignment = TextAlignment.Center;
                File_Name.Margin = new Thickness(125, 0, 0, 0);
                System.Windows.Controls.Image Check = new System.Windows.Controls.Image();
                Check.Name = "Visibility_Image_" + ID;
                Check.Margin = new Thickness(10, 32.5, 0, 0);
                Check.Width = 30;
                Check.Height = 30;
                Check.Stretch = System.Windows.Media.Stretch.Fill;
                Check.HorizontalAlignment = HorizontalAlignment.Left;
                Check.VerticalAlignment = VerticalAlignment.Top;
                Check.Focusable = false;
                Check.Source = Sub_Code.Check_03;
                System.Windows.Controls.Image Texture_Icon = new System.Windows.Controls.Image();
                Texture_Icon.Name = "Texture_Icon_" + ID;
                Texture_Icon.Margin = new Thickness(75, 27.5, 0, 0);
                Texture_Icon.Width = 50;
                Texture_Icon.Height = 50;
                Texture_Icon.Stretch = System.Windows.Media.Stretch.UniformToFill;
                Texture_Icon.HorizontalAlignment = HorizontalAlignment.Left;
                Texture_Icon.VerticalAlignment = VerticalAlignment.Top;
                Texture_Icon.Focusable = false;
                Texture_Icon.Source = Texture;
                Border Texture_Border = new Border();
                Texture_Border.Name = "Texture_Border_" + ID;
                Texture_Border.Margin = new Thickness(74, 26.5, 0, 0);
                Texture_Border.Width = 52;
                Texture_Border.Height = 52;
                Texture_Border.BorderThickness = new Thickness(1);
                Texture_Border.BorderBrush = System.Windows.Media.Brushes.Aqua;
                Texture_Border.HorizontalAlignment = HorizontalAlignment.Left;
                Texture_Border.VerticalAlignment = VerticalAlignment.Top;
                Texture_Border.Focusable = false;
                Parent.Children.Add(Opacity);
                Parent.Children.Add(File_Name);
                Parent.Children.Add(Check);
                Parent.Children.Add(Texture_Icon);
                Parent.Children.Add(Texture_Border);
                System.Windows.Controls.Image Texture_Image = new System.Windows.Controls.Image();
                Texture_Image.Name = "Texture_Image_" + ID;
                Texture_Image.Width = Texture_BMP.Width;
                Texture_Image.Height = Texture_BMP.Height;
                Texture_Image.VerticalAlignment = VerticalAlignment.Top;
                Texture_Image.HorizontalAlignment = HorizontalAlignment.Left;
                Texture_Image.Focusable = false;
                Texture_Image.Source = Texture;
                Texture_Image.Stretch = Stretch.Uniform;
                System.Drawing.Point Set_Position = Init_Position;
                if (IsCenterMode)
                    Set_Position = Position_To_Center(Init_Position, (int)Texture.Width, (int)Texture.Height);
                Texture_Image.Margin = new Thickness(Set_Position.X, Set_Position.Y, 0, 0);
                Texture_Property_Info Property_Info = new Texture_Property_Info(Parent, Opacity, File_Name, Check, Texture_Icon, Texture_Border);
                Texture_Image_Info Image_Info = new Texture_Image_Info(Texture_BMP, Texture_Image, Set_Position, Property_Position);
                Texture_Info Info = new Texture_Info(Property_Info, Image_Info, ID);
                Check.MouseEnter += delegate
                {
                    if (Property_Info.IsVisibility)
                        Check.Source = Sub_Code.Check_04;
                    else
                        Check.Source = Sub_Code.Check_02;
                };
                Check.MouseLeave += delegate
                {
                    if (Property_Info.IsVisibility)
                        Check.Source = Sub_Code.Check_03;
                    else
                        Check.Source = Sub_Code.Check_01;
                };
                All_Textures.Add(Info);
                Move_Texture(Info, Set_Position);
                return Info;
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                return null;
            }
        }
        public void Move_Texture(Texture_Info TexInfo, System.Drawing.Point To_Position)
        {
            TexInfo.Texture_Image.Position = To_Position;
            TexInfo.Texture_Image.Texture_Image.Margin = new Thickness(To_Position.X, To_Position.Y, 0, 0);
        }
        public void Update_Texture(Texture_Info TexInfo)
        {
            TexInfo.Texture_Image.Texture_Image.Source = Sub_Code.Bitmap_To_BitmapImage(TexInfo.Texture_Image.Texture_Change);
        }
        public void Set_Texture_Opacity(Texture_Info TexInfo, double Opacity)
        {
            System.Drawing.Bitmap Temp_Bitmap = new System.Drawing.Bitmap(TexInfo.Texture_Image.Width, TexInfo.Texture_Image.Height);
            using (System.Drawing.Graphics gfx = System.Drawing.Graphics.FromImage(Temp_Bitmap))
            {
                ColorMatrix matrix = new ColorMatrix();
                matrix.Matrix33 = (float)TexInfo.Property.Opacity.Value;
                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                gfx.DrawImage(TexInfo.Texture_Image.Texture_Main, new System.Drawing.Rectangle(0, 0, Temp_Bitmap.Width, Temp_Bitmap.Height), 0, 0, Temp_Bitmap.Width, Temp_Bitmap.Height, System.Drawing.GraphicsUnit.Pixel, attributes);
            }
            TexInfo.Texture_Image.Texture_Change.Dispose();
            TexInfo.Texture_Image.Texture_Change = Temp_Bitmap;
        }
        public void Change_Rotate_Texture(Texture_Info TexInfo, int Init_Index)
        {
            Texture_Image_Info Info = TexInfo.Texture_Image;
            int Minus_X = Math.Min(Info.Left_UP_Position.X, Info.Left_Down_Position.X);
            int Minus_Y = Math.Min(Info.Left_UP_Position.Y, Info.Right_UP_Position.Y);
            System.Drawing.Point Left_UP = new System.Drawing.Point(Info.Left_UP_Position.X - Minus_X, Info.Left_UP_Position.Y - Minus_Y);
            System.Drawing.Point Right_UP = new System.Drawing.Point(Info.Right_UP_Position.X + Info.Width - Minus_X, Info.Right_UP_Position.Y - Minus_Y);
            System.Drawing.Point Left_Down = new System.Drawing.Point(Info.Left_Down_Position.X - Minus_X, Info.Left_Down_Position.Y + Info.Height - Minus_Y);
            int New_Width = Info.Width + Info.Right_UP_Position.X - Info.Left_UP_Position.X;
            int New_Height = Info.Height + Info.Left_Down_Position.Y - Info.Left_UP_Position.Y;
            Bitmap canvas = new Bitmap(New_Width, New_Height);
            Graphics g = Graphics.FromImage(canvas);
            Bitmap img = new Bitmap(TexInfo.Texture_Image.Texture_Main);
            System.Drawing.Point[] destinationPoints = new System.Drawing.Point[] { Left_UP, Right_UP, Left_Down };
            g.DrawImage(img, destinationPoints);
            g.Dispose();
            img.Dispose();
            TexInfo.Texture_Image.Texture_Change = canvas;
            TexInfo.Texture_Image.Texture_Image.Source = Sub_Code.Bitmap_To_BitmapImage(TexInfo.Texture_Image.Texture_Change);
            Matrix matrix = TexInfo.Texture_Image.Texture_Image.RenderTransform.Value;
            matrix.Translate(-Minus_X * Zoom, -Minus_Y * Zoom);
            TexInfo.Texture_Image.Texture_Image.RenderTransform = new MatrixTransform(matrix);
        }
        public System.Drawing.Point Position_To_Center(System.Drawing.Point Position, int Texture_Size_X, int Texture_Size_Y)
        {
            return new System.Drawing.Point(Position.X - (int)(Texture_Size_X / 2), Position.Y - (int)(Texture_Size_Y / 2));
        }
    }
}