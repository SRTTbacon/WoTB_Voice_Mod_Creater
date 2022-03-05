using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace WoTB_Voice_Mod_Creater.Class
{
    public partial class Music_Player_Setting : UserControl
    {
        public delegate void CheckBoxEventHandler<T>(T args);
        public event CheckBoxEventHandler<bool> ChangeLPFEnable;
        public event CheckBoxEventHandler<bool> ChangeHPFEnable;
        public event CheckBoxEventHandler<bool> ChangeECHOEnable;
        public bool IsLPFChanged = false;
        public bool IsHPFChanged = false;
        public bool IsECHOChanged = false;
        public bool IsLPFEnable = false;
        public bool IsHPFEnable = false;
        public bool IsECHOEnable = false;
        bool IsClosing = false;
        public Music_Player_Setting()
        {
            InitializeComponent();
        }
        public async void Window_Show()
        {
            Opacity = 0;
            Visibility = System.Windows.Visibility.Visible;
            while (Opacity < 1 && !IsClosing)
            {
                Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
        }
        private void LPF_S_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }
        private void LPF_S_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            IsLPFChanged = true;
            LPF_T.Text = "Low Pass Filter:" + (int)LPF_S.Value;
        }
        private void HPF_S_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            IsHPFChanged = true;
            HPF_T.Text = "High Pass Filter:" + (int)HPF_S.Value;
        }
        private void ECHO_Delay_S_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            IsECHOChanged = true;
            ECHO_Delay_T.Text = "エコー(遅延):" + Math.Round(ECHO_Delay_S.Value, 1, MidpointRounding.AwayFromZero) + "秒";
        }
        private void ECHO_Power_Original_S_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            IsECHOChanged = true;
            ECHO_Power_Original_T.Text = "エコー(元音量):" + (int)ECHO_Power_Original_S.Value;
        }
        private void ECHO_Power_ECHO_S_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            IsECHOChanged = true;
            ECHO_Power_ECHO_T.Text = "エコー音量" + (int)ECHO_Power_ECHO_S.Value;
        }
        private void ECHO_Length_S_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            IsECHOChanged = true;
            ECHO_Length_T.Text = "エコー(長さ)" + (int)ECHO_Length_S.Value;
        }
        private void LPF_Enable_C_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (IsLPFEnable)
            {
                IsLPFEnable = false;
                LPF_Enable_C.Source = Sub_Code.Check_02;
            }
            else
            {
                IsLPFEnable = true;
                LPF_Enable_C.Source = Sub_Code.Check_04;
            }
            ChangeLPFEnable(IsLPFEnable);
        }
        private void LPF_Enable_C_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsLPFEnable)
                LPF_Enable_C.Source = Sub_Code.Check_04;
            else
                LPF_Enable_C.Source = Sub_Code.Check_02;
        }
        private void LPF_Enable_C_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsLPFEnable)
                LPF_Enable_C.Source = Sub_Code.Check_03;
            else
                LPF_Enable_C.Source = Sub_Code.Check_01;
        }
        private void HPF_Enable_C_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (IsHPFEnable)
            {
                IsHPFEnable = false;
                HPF_Enable_C.Source = Sub_Code.Check_02;
            }
            else
            {
                IsHPFEnable = true;
                HPF_Enable_C.Source = Sub_Code.Check_04;
            }
            ChangeHPFEnable(IsHPFEnable);
        }
        private void HPF_Enable_C_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsHPFEnable)
                HPF_Enable_C.Source = Sub_Code.Check_04;
            else
                HPF_Enable_C.Source = Sub_Code.Check_02;
        }
        private void HPF_Enable_C_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsHPFEnable)
                HPF_Enable_C.Source = Sub_Code.Check_03;
            else
                HPF_Enable_C.Source = Sub_Code.Check_01;
        }
        private void ECHO_Enable_C_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (IsECHOEnable)
            {
                IsECHOEnable = false;
                ECHO_Enable_C.Source = Sub_Code.Check_02;
            }
            else
            {
                IsECHOEnable = true;
                ECHO_Enable_C.Source = Sub_Code.Check_04;
            }
            ChangeECHOEnable(IsECHOEnable);
        }
        private void ECHO_Enable_C_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsECHOEnable)
                ECHO_Enable_C.Source = Sub_Code.Check_04;
            else
                ECHO_Enable_C.Source = Sub_Code.Check_02;
        }
        private void ECHO_Enable_C_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsECHOEnable)
                ECHO_Enable_C.Source = Sub_Code.Check_03;
            else
                ECHO_Enable_C.Source = Sub_Code.Check_01;
        }
        private async void Back_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!IsClosing)
            {
                IsClosing = true;
                Configs_Save();
                while (Opacity > 0)
                {
                    Opacity -= Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
                IsClosing = false;
                Visibility = System.Windows.Visibility.Hidden;
            }
        }
        void Configs_Save()
        {
            try
            {
                StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "\\Configs\\Music_Player_Setting.tmp");
                stw.WriteLine(LPF_S.Value);
                stw.WriteLine(HPF_S.Value);
                stw.WriteLine(ECHO_Delay_S.Value);
                stw.WriteLine(ECHO_Power_Original_S.Value);
                stw.WriteLine(ECHO_Power_ECHO_S.Value);
                stw.WriteLine(ECHO_Length_S.Value);
                stw.WriteLine(IsLPFEnable);
                stw.WriteLine(IsHPFEnable);
                stw.Write(IsECHOEnable);
                stw.Close();
                Sub_Code.File_Encrypt(Voice_Set.Special_Path + "\\Configs\\Music_Player_Setting.tmp", Voice_Set.Special_Path + "\\Configs\\Music_Player_Setting.dat", "ISCREAM_SRTTbacon_Cry", true);
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            bool IsLoaded = false;
            LPF_Enable_C.Source = Sub_Code.Check_01;
            HPF_Enable_C.Source = Sub_Code.Check_01;
            ECHO_Enable_C.Source = Sub_Code.Check_01;
            if (File.Exists(Voice_Set.Special_Path + "\\Configs\\Music_Player_Setting.dat"))
            {
                try
                {
                    Sub_Code.File_Decrypt_To_File(Voice_Set.Special_Path + "\\Configs\\Music_Player_Setting.dat", Voice_Set.Special_Path + "\\Configs\\Music_Player_Setting.tmp", "ISCREAM_SRTTbacon_Cry", false);
                    StreamReader str = new StreamReader(Voice_Set.Special_Path + "\\Configs\\Music_Player_Setting.tmp");
                    LPF_S.Value = double.Parse(str.ReadLine());
                    HPF_S.Value = double.Parse(str.ReadLine());
                    ECHO_Delay_S.Value = double.Parse(str.ReadLine());
                    ECHO_Power_Original_S.Value = double.Parse(str.ReadLine());
                    ECHO_Power_ECHO_S.Value = double.Parse(str.ReadLine());
                    ECHO_Length_S.Value = double.Parse(str.ReadLine());
                    try
                    {
                        IsLPFEnable = bool.Parse(str.ReadLine());
                        IsHPFEnable = bool.Parse(str.ReadLine());
                        IsECHOEnable = bool.Parse(str.ReadLine());
                        LPF_Enable_C_MouseLeave(null, null);
                        HPF_Enable_C_MouseLeave(null, null);
                        ECHO_Enable_C_MouseLeave(null, null);
                    }
                    catch { }
                    str.Close();
                    File.Delete(Voice_Set.Special_Path + "\\Configs\\Music_Player_Setting.tmp");
                    IsLoaded = true;
                }
                catch (Exception e1)
                {
                    Sub_Code.Error_Log_Write(e1.Message);
                }
            }
            if (!IsLoaded)
            {
                ECHO_Delay_S.Value = 0.3;
                ECHO_Power_Original_S.Value = 100;
                ECHO_Length_S.Value = 45;
            }
        }
    }
}