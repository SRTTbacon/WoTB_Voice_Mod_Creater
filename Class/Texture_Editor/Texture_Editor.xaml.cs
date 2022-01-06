using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WoTB_Voice_Mod_Creater.Class.Texture_Editor
{
    public partial class Texture_Editor : UserControl
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);
        Texture_Core Core = null;
        List<Line> Dollet_Lines = new List<Line>();
        List<System.Windows.Shapes.Ellipse> Rotate_Circles = new List<System.Windows.Shapes.Ellipse>();
        Texture_Info Select_Texture = null;
        Point MoveStartPosition = new Point(0, 0);
        Point Move_Select_Texture = new Point(0, 0);
        Point Move_Rotate_Circle_Position = new Point(0, 0);
        int FPS = 15;
        int Select_Texture_Index = -1;
        int Move_Rotate_Circle_Index = -1;
        bool IsOnTextureCanvas = false;
        bool IsMoveMode = false;
        bool IsClosing = false;
        bool IsMessageShowing = false;
        bool IsMouseClicked = false;
        bool IsMoveWaitMode = false;
        public Texture_Editor()
        {
            InitializeComponent();
            for (int Number = 0; Number < 4; Number++)
            {
                Line Dollet_Line = new Line();
                Dollet_Line.Visibility = System.Windows.Visibility.Hidden;
                Dollet_Line.StrokeThickness = 3;
                Dollet_Line.StrokeDashArray = new System.Windows.Media.DoubleCollection(2);
                Dollet_Line.SnapsToDevicePixels = true;
                Dollet_Line.StrokeStartLineCap = System.Windows.Media.PenLineCap.Round;
                Dollet_Line.StrokeEndLineCap = System.Windows.Media.PenLineCap.Round;
                Dollet_Line.Stroke = System.Windows.Media.Brushes.White;
                Dollet_Line.Margin = new Thickness(0, 0, 0, 0);
                Dollet_Line.VerticalAlignment = VerticalAlignment.Top;
                Dollet_Line.HorizontalAlignment = HorizontalAlignment.Left;
                Dollet_Lines.Add(Dollet_Line);
                Line_Canvas.Children.Add(Dollet_Line);
            }
            for (int Number = 0; Number < 3; Number++)
            {
                int Index = Number;
                System.Windows.Shapes.Ellipse Rotate_Circle = new System.Windows.Shapes.Ellipse();
                SolidColorBrush Circle_Brush = new SolidColorBrush();
                Circle_Brush.Color = Colors.White;
                Rotate_Circle.Fill = Circle_Brush;
                Rotate_Circle.StrokeThickness = 1;
                Rotate_Circle.Stroke = System.Windows.Media.Brushes.Black;
                Rotate_Circle.Width = 15;
                Rotate_Circle.Height = 15;
                Rotate_Circle.Visibility = Visibility.Hidden;
                Rotate_Circle.MouseLeftButtonDown += delegate
                {
                    Move_Rotate_Circle_Index = Index;
                    Move_Rotate_Circle_Position = Mouse.GetPosition(this);
                };
                Rotate_Circles.Add(Rotate_Circle);
                Line_Canvas.Children.Add(Rotate_Circle);
            }
        }
        public async void Window_Show()
        {
            if (Core == null)
                Core = new Texture_Core((Style)(this.Resources["CustomSliderStyle_Yoko"]), new Vector(Texture_Canvas.Width, Texture_Canvas.Height));
            Opacity = 0;
            Visibility = Visibility.Visible;
            Loop_FPS();
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
            bool IsForce = false;
            while (Message_T.Opacity > 0)
            {
                if (!IsMessageShowing)
                {
                    IsForce = true;
                    break;
                }
                Number++;
                if (Number >= 120)
                    Message_T.Opacity -= 0.025;
                await Task.Delay(1000 / 60);
            }
            if (!IsForce)
            {
                IsMessageShowing = false;
                Message_T.Text = "";
                Message_T.Opacity = 1;
            }
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
                Visibility = Visibility.Hidden;
                IsClosing = false;
            }
        }
        async void Loop_FPS()
        {
            double nextFrame = (double)Environment.TickCount;
            float period = 1000f / FPS;
            while (Visibility == Visibility.Visible)
            {
                double tickCount = (double)Environment.TickCount;
                if (tickCount < nextFrame)
                {
                    if (nextFrame - tickCount > 1)
                        await Task.Delay((int)(nextFrame - tickCount));
                    System.Windows.Forms.Application.DoEvents();
                    continue;
                }
                Point Mouse_Point_Now = Mouse.GetPosition(this);
                IsMouseClicked = GetAsyncKeyState((int)System.Windows.Forms.Keys.LButton) != 0;
                if (IsMoveMode && IsMouseClicked)
                {
                    if (Core.All_Textures.Count != 0)
                        Core.Move_Texture(Core.All_Textures[0], Core.Position_To_Center(new System.Drawing.Point((int)Mouse_Point_Now.X, (int)Mouse_Point_Now.Y), 
                            Core.All_Textures[0].Texture_Image.Width, Core.All_Textures[0].Texture_Image.Height));
                }
                if (IsOnTextureCanvas)
                {
                    bool IsLCtrlDown = (Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) == KeyStates.Down;
                    if (IsLCtrlDown && IsMouseClicked)
                        Cursor = Cursors.Cross;
                    else if (IsLCtrlDown)
                        Cursor = Cursors.Hand;
                    else
                        Cursor = null;
                }
                if (MoveStartPosition.X != 0 && MoveStartPosition.Y != 0)
                {
                    if (IsMouseClicked)
                    {
                        foreach (Texture_Info Info in Core.All_Textures)
                        {
                            Matrix matrix = Info.Texture_Image.Texture_Image.RenderTransform.Value;
                            Point v = new Point(MoveStartPosition.X - Mouse_Point_Now.X, MoveStartPosition.Y - Mouse_Point_Now.Y);
                            matrix.Translate(-v.X, -v.Y);
                            Info.Texture_Image.Texture_Image.RenderTransform = new MatrixTransform(matrix);
                            if (Select_Texture == Info)
                                Set_Dollet_Line(Info);
                        }
                        MoveStartPosition = Mouse_Point_Now;
                    }
                    else
                        MoveStartPosition = new Point(0, 0);
                }
                else if (Move_Select_Texture.X != 0 && Move_Select_Texture.Y != 0 && IsMouseClicked && Select_Texture != null)
                {
                    Point v = new Point(Move_Select_Texture.X - Mouse_Point_Now.X, Move_Select_Texture.Y - Mouse_Point_Now.Y);
                    Select_Texture.Texture_Image.Position = new System.Drawing.Point((int)(Select_Texture.Texture_Image.Position.X - v.X / Core.Zoom),
                        (int)(Select_Texture.Texture_Image.Position.Y - v.Y / Core.Zoom));
                    Matrix matrix = Select_Texture.Texture_Image.Texture_Image.RenderTransform.Value;
                    matrix.Translate(-v.X, -v.Y);
                    Select_Texture.Texture_Image.Texture_Image.RenderTransform = new MatrixTransform(matrix);
                    Set_Dollet_Line(Select_Texture);
                    Move_Select_Texture = Mouse_Point_Now;
                }
                else if (Move_Select_Texture.X != 0 && Move_Select_Texture.Y != 0 && !IsMouseClicked)
                {
                    Move_Select_Texture = new Point(0, 0);
                    IsMoveWaitMode = false;
                }
                else if (Move_Rotate_Circle_Index != -1 && Select_Texture != null && IsMouseClicked)
                {
                    if (Mouse_Point_Now != Move_Rotate_Circle_Position)
                    {
                        Point a = new Point(Move_Rotate_Circle_Position.X - Mouse_Point_Now.X, Move_Rotate_Circle_Position.Y - Mouse_Point_Now.Y);
                        Point b = new Point(a.X / Core.Zoom, a.Y / Core.Zoom);
                        if (Move_Rotate_Circle_Index == 0)
                            Select_Texture.Texture_Image.Left_UP_Position = new System.Drawing.Point((int)(Select_Texture.Texture_Image.Left_UP_Position.X - b.X), (int)(Select_Texture.Texture_Image.Left_UP_Position.Y - b.Y));
                        else if (Move_Rotate_Circle_Index == 1)
                            Select_Texture.Texture_Image.Right_UP_Position = new System.Drawing.Point((int)(Select_Texture.Texture_Image.Right_UP_Position.X - b.X), (int)(Select_Texture.Texture_Image.Right_UP_Position.Y - b.Y));
                        else if (Move_Rotate_Circle_Index == 2)
                            Select_Texture.Texture_Image.Left_Down_Position = new System.Drawing.Point((int)(Select_Texture.Texture_Image.Left_Down_Position.X - b.X), (int)(Select_Texture.Texture_Image.Left_Down_Position.Y - b.Y));
                        Move_Rotate_Circle_Position = Mouse_Point_Now;
                        Set_Dollet_Line(Select_Texture);
                    }
                }
                else if (Move_Rotate_Circle_Index != -1 && Select_Texture != null && !IsMouseClicked)
                {
                    Core.Change_Rotate_Texture(Select_Texture, Move_Rotate_Circle_Index);
                    Move_Rotate_Circle_Index = -1;
                    Move_Rotate_Circle_Position = new Point(0, 0);
                }
                if (period != 1000f / FPS)
                    period = 1000f / FPS;
                if ((double)System.Environment.TickCount >= nextFrame + (double)period)
                {
                    nextFrame += period;
                    continue;
                }
                nextFrame += period;
            }
        }
        public bool Add_Texture(string File, System.Drawing.Point Position)
        {
            Texture_Info Info = Core.Add_Texture(File, Position, Core.All_Textures.Count, true);
            if (Info == null)
                return false;
            Property_Window.Children.Add(Info.Property.Parent);
            Paint_Canvas.Children.Add(Info.Texture_Image.Texture_Image);
            Set_Texture_Zoom(Info, new Point(Position.X, Position.Y), Core.Zoom);
            Control_Events(Info);
            return true;
        }
        private void Add_Texture_B_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog()
            {
                Title = "追加する画像を選択してください。",
                Multiselect = true,
                Filter = "画像ファイル(*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tiff;*.exif;*.dds)|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tiff;*.exif;*.dds"
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string Error_File = "";
                foreach (string File_Name in ofd.FileNames)
                {
                    if (!Add_Texture(File_Name, new System.Drawing.Point(750, 450)))
                    {
                        if (Error_File == "")
                            Error_File = File_Name;
                        else
                            Error_File += "\n" + File_Name;
                    }
                }
                if (Error_File != "")
                {
                    MessageBox.Show("エラー:以下のファイルを追加できませんでした。\n" + Error_File);
                    Message_Feed_Out("エラー:追加されなかった画像が" + (Error_File.CountOf("\n") + 1) + "個存在します。");
                }
            }
            ofd.Dispose();
        }
        void Control_Events(Texture_Info TexInfo)
        {
            Texture_Property_Info PInfo = TexInfo.Property;
            PInfo.Opacity.ValueChanged += delegate
            {
                TexInfo.Texture_Image.Texture_Image.Opacity = PInfo.Opacity.Value;
            };
            PInfo.ChangeCheckBox += delegate (bool IsVisibility)
            {
                if (IsVisibility)
                    TexInfo.Texture_Image.Texture_Image.Visibility = Visibility.Visible;
                else
                    TexInfo.Texture_Image.Texture_Image.Visibility = Visibility.Hidden;
            };
        }
        void Draw_Dollet_Line(Point Left_UP, Point Right_UP, Point Left_Down)
        {
            Dollet_Lines[0].X1 = Left_UP.X - 1;
            Dollet_Lines[0].Y1 = Left_UP.Y - 1;
            Dollet_Lines[0].X2 = Left_Down.X - 1;
            Dollet_Lines[0].Y2 = Left_Down.Y + 1;
            Dollet_Lines[1].X1 = Right_UP.X + 1;
            Dollet_Lines[1].Y1 = Right_UP.Y - 1;
            Dollet_Lines[1].X2 = Right_UP.X + (Left_Down.X - Left_UP.X) + 1;
            Dollet_Lines[1].Y2 = Right_UP.Y + (Left_Down.Y - Left_UP.Y) + 1;
            Dollet_Lines[2].X1 = Left_UP.X - 1;
            Dollet_Lines[2].Y1 = Left_UP.Y - 1;
            Dollet_Lines[2].X2 = Right_UP.X + 1;
            Dollet_Lines[2].Y2 = Right_UP.Y - 1;
            Dollet_Lines[3].X1 = Left_Down.X - 1;
            Dollet_Lines[3].Y1 = Left_Down.Y + 1;
            Dollet_Lines[3].X2 = Right_UP.X + (Left_Down.X - Left_UP.X) + 1;
            Dollet_Lines[3].Y2 = Right_UP.Y + (Left_Down.Y - Left_UP.Y) + 2;
        }
        void Set_Dollet_Line(Texture_Info Select_Texture_Image)
        {
            Texture_Image_Info Info = Select_Texture_Image.Texture_Image;
            Matrix matrix = ((MatrixTransform)Info.Texture_Image.RenderTransform).Matrix;
            int Start_X = (int)(Info.Init_Position.X + matrix.OffsetX);
            int Start_Y = (int)(Info.Init_Position.Y + matrix.OffsetY);
            int End_X = Start_X + (int)(Info.Texture_Image.Width * matrix.M11);
            int End_Y = Start_Y + (int)(Info.Texture_Image.Height * matrix.M11);
            Rotate_Circles[0].Margin = new Thickness(Start_X - 8 + Info.Left_UP_Position.X * Core.Zoom, Start_Y - 8 + Info.Left_UP_Position.Y * Core.Zoom, 0, 0);
            Rotate_Circles[1].Margin = new Thickness(End_X - 8 + Info.Right_UP_Position.X * Core.Zoom, Start_Y - 8 + Info.Right_UP_Position.Y * Core.Zoom, 0, 0);
            Rotate_Circles[2].Margin = new Thickness(Start_X - 8 + Info.Left_Down_Position.X * Core.Zoom, End_Y - 8 + Info.Left_Down_Position.Y * Core.Zoom, 0, 0);
            Point Left_UP = new Point(Rotate_Circles[0].Margin.Left + 8, Rotate_Circles[0].Margin.Top + 8);
            Point Right_UP = new Point(Rotate_Circles[1].Margin.Left + 8, Rotate_Circles[1].Margin.Top + 8);
            Point Left_Down = new Point(Rotate_Circles[2].Margin.Left + 8, Rotate_Circles[2].Margin.Top + 8);
            Draw_Dollet_Line(Left_UP, Right_UP, Left_Down);
        }
        Texture_Info Get_Point_Texture(System.Drawing.Point Select_Point, int End_Index = -1)
        {
            int Start_Index = 0;
            if (Select_Texture_Index != -1)
                Start_Index = Select_Texture_Index + 1;
            if (End_Index == -1)
            {
                for (int Number = Start_Index; Number < Core.All_Textures.Count; Number++)
                {
                    Matrix matrix = ((MatrixTransform)Core.All_Textures[Number].Texture_Image.Texture_Image.RenderTransform).Matrix;
                    Point Position = Core.All_Textures[Number].Texture_Image.Texture_Image.TranslatePoint(new Point(0, 0), Paint_Canvas);
                    int Start_X = (int)(Core.All_Textures[Number].Texture_Image.Position.X + matrix.OffsetX);
                    int Start_Y = (int)(Core.All_Textures[Number].Texture_Image.Position.Y + matrix.OffsetY);
                    int End_X = Start_X + (int)(Core.All_Textures[Number].Texture_Image.Texture_Image.Width * matrix.M11);
                    int End_Y = Start_Y + (int)(Core.All_Textures[Number].Texture_Image.Texture_Image.Height * matrix.M11);
                    if (Start_X <= Select_Point.X && Start_Y <= Select_Point.Y && End_X >= Select_Point.X && End_Y >= Select_Point.Y)
                    {
                        Select_Texture_Index = Number;
                        return Core.All_Textures[Number];
                    }
                }
                if (Start_Index != 0)
                {
                    Select_Texture_Index = 0;
                    return Get_Point_Texture(Select_Point, Start_Index);
                }
            }
            else
            {
                for (int Number = 0; Number < End_Index; Number++)
                {
                    Matrix matrix = ((MatrixTransform)Core.All_Textures[Number].Texture_Image.Texture_Image.RenderTransform).Matrix;
                    Point Position = Core.All_Textures[Number].Texture_Image.Texture_Image.TranslatePoint(new Point(0, 0), Paint_Canvas);
                    int Start_X = (int)(Core.All_Textures[Number].Texture_Image.Position.X + matrix.OffsetX);
                    int Start_Y = (int)(Core.All_Textures[Number].Texture_Image.Position.Y + matrix.OffsetY);
                    int End_X = Start_X + (int)(Core.All_Textures[Number].Texture_Image.Texture_Image.Width * matrix.M11);
                    int End_Y = Start_Y + (int)(Core.All_Textures[Number].Texture_Image.Texture_Image.Height * matrix.M11);
                    if (Start_X <= Select_Point.X && Start_Y <= Select_Point.Y && End_X >= Select_Point.X && End_Y >= Select_Point.Y)
                    {
                        Select_Texture_Index = Number;
                        return Core.All_Textures[Number];
                    }
                }
            }
            return null;
        }
        void Dollet_Line_Visibility(bool IsVisible)
        {
            if (IsVisible)
            {
                foreach (Line Temp in Dollet_Lines)
                    Temp.Visibility = Visibility.Visible;
                foreach (System.Windows.Shapes.Ellipse Circle in Rotate_Circles)
                    Circle.Visibility = Visibility.Visible;
            }
            else
            {
                foreach (Line Temp in Dollet_Lines)
                    Temp.Visibility = Visibility.Hidden;
                foreach (System.Windows.Shapes.Ellipse Circle in Rotate_Circles)
                    Circle.Visibility = Visibility.Hidden;
            }
        }
        double Set_Texture_Zoom(Texture_Info Info, Point Center_Point, double Change_Zoom)
        {
            Matrix matrix = ((MatrixTransform)Info.Texture_Image.Texture_Image.RenderTransform).Matrix;
            matrix.ScaleAt(Change_Zoom, Change_Zoom, Center_Point.X - Info.Texture_Image.Init_Position.X, Center_Point.Y - Info.Texture_Image.Init_Position.Y);
            Info.Texture_Image.Texture_Image.RenderTransform = new MatrixTransform(matrix);
            return matrix.M11;
        }
        private void Texture_Canvas_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            bool IsLCtrlDown = (Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) == KeyStates.Down;
            if (IsLCtrlDown)
                MoveStartPosition = Mouse.GetPosition(this);
            else if (Select_Texture == null)
            {
                Point MousePoint = Mouse.GetPosition(this);
                Texture_Info Info = Get_Point_Texture(new System.Drawing.Point((int)MousePoint.X, (int)MousePoint.Y));
                if (Info != null)
                {
                    Select_Texture = Info;
                    Set_Dollet_Line(Info);
                    Dollet_Line_Visibility(true);
                }
                else
                {
                    Select_Texture = null;
                    Dollet_Line_Visibility(false);
                }
            }
            else
            {
                IsMoveWaitMode = true;
                Move_Select_Texture = Mouse.GetPosition(this);
            }
        }
        private void Texture_Canvas_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            bool IsPlus = false;
            if (e.Delta > 0)
                IsPlus = true;
            Point MousePoint = e.GetPosition(Texture_Canvas);
            foreach (Texture_Info Info in Core.All_Textures)
            {
                Matrix matrix = ((MatrixTransform)Info.Texture_Image.Texture_Image.RenderTransform).Matrix;
                if (IsPlus && matrix.M11 < 9)
                    Core.Zoom = Set_Texture_Zoom(Info, MousePoint, 1.1);
                else if (!IsPlus && matrix.M11 > 0.12)
                    Core.Zoom = Set_Texture_Zoom(Info, MousePoint, 1 / 1.1);
            }
            if (Select_Texture != null)
                Set_Dollet_Line(Select_Texture);
        }
        private void Texture_Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point MousePosition = e.GetPosition(Texture_Canvas);
            Mouse_Pos_T.Text = (int)MousePosition.X + "," + (int)MousePosition.Y;
            if (IsMoveWaitMode)
                IsMoveWaitMode = false;
        }
        private void Texture_Canvas_MouseEnter(object sender, MouseEventArgs e)
        {
            IsOnTextureCanvas = true;
        }
        private void Texture_Canvas_MouseLeave(object sender, MouseEventArgs e)
        {
            Mouse_Pos_T.Text = "";
            IsOnTextureCanvas = false;
            Cursor = null;
        }
        private void Texture_Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsMoveWaitMode)
            {
                IsMoveWaitMode = false;
                Point MousePoint = Mouse.GetPosition(this);
                Texture_Info Info = Get_Point_Texture(new System.Drawing.Point((int)MousePoint.X, (int)MousePoint.Y));
                if (Info != null)
                {
                    Select_Texture = Info;
                    Set_Dollet_Line(Info);
                    Dollet_Line_Visibility(true);
                }
                else
                {
                    Select_Texture = null;
                    Dollet_Line_Visibility(false);
                }
            }
            Move_Select_Texture = Mouse.GetPosition(this);
        }
    }
}