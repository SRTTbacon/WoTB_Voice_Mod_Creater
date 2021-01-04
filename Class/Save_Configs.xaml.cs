using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WMPLib;

namespace WoTB_Voice_Mod_Creater.Class
{
    public partial class Save_Configs : UserControl
    {
        readonly string Special_Path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/WoTB_Voice_Mod_Creater";
        readonly WindowsMediaPlayer Player = new WindowsMediaPlayer();
        readonly BrushConverter bc = new BrushConverter();
        List<string> Voice_Type = new List<string>();
        List<int> Voice_Type_Number = new List<int>();
        string Select_SE_Name = "";
        string SE_Dir = "";
        int Select_SE_File_Count = 0;
        int SE_Play_Index = 1;
        bool IsBusy = false;
        bool IsSEStop = false;
        public Save_Configs()
        {
            InitializeComponent();
            SE_Lists.Items.Add("時間切れ&占領ポイントMax | 有効");
            SE_Lists.Items.Add("クイックコマンド | 有効");
            SE_Lists.Items.Add("弾薬庫破損 | 有効");
            SE_Lists.Items.Add("自車両大破 | 有効");
            SE_Lists.Items.Add("貫通 | 有効");
            SE_Lists.Items.Add("敵モジュール破損 | 有効");
            SE_Lists.Items.Add("無線機破損 | 有効");
            SE_Lists.Items.Add("燃料タンク破損 | 有効");
            SE_Lists.Items.Add("非貫通 | 有効");
            SE_Lists.Items.Add("装填完了 | 有効");
            SE_Lists.Items.Add("第六感 | 有効");
            SE_Lists.Items.Add("敵発見 | 有効");
            SE_Lists.Items.Add("戦闘開始前タイマー | 有効");
            SE_Lists.Items.Add("ロックオン | 有効");
            SE_Lists.Items.Add("アンロック | 有効");
            for (int Number = 0; Number <= 14; Number++)
            {
                Voice_Set.SE_Enable_List.Add(true);
            }
            Player.settings.volume = 100;
        }
        public void Window_Show()
        {
            SE_Dir = Special_Path + "/Server/" + Voice_Set.SRTTbacon_Server_Name + "/Voices/SE";
            Project_T.Text = "プロジェクト名:" + Voice_Set.SRTTbacon_Server_Name;
            Sub_Code.Get_Voice_Type_And_Index(Special_Path + "/Server/" + Voice_Set.SRTTbacon_Server_Name + "/Voices", ref Voice_Type, ref Voice_Type_Number);
            for (int Number = 0; Number <= Voice_Type.Count - 1; Number++)
            {
                Voice_Lists.Items.Add(Voice_Type[Number] + ":" + Voice_Type_Number[Number] + "個");
            }
        }
        public void Window_Show_V2(string Project_Name, List<List<string>> Lists)
        {
            SE_Dir = Special_Path + "/SE";
            Project_T.Text = "プロジェクト名:" + Project_Name;
            for (int Number = 0; Number <= Lists.Count - 1; Number++)
            {
                string Name = Voice_Set.Get_Voice_Type_Japanese_Name_V2(Number);
                int Number_01 = Lists[Number].Count;
                Voice_Type.Add(Name);
                Voice_Type_Number.Add(Number_01);
                Voice_Lists.Items.Add(Name + ":" + Number_01 + "個");
            }
        }
        private void SE_Lists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsBusy || SE_Lists.SelectedIndex == -1)
            {
                return;
            }
            SE_Play_Index = 1;
            if (Voice_Set.SE_Enable_List[SE_Lists.SelectedIndex])
            {
                SE_Disable_B.Background = Brushes.Transparent;
                SE_Disable_B.BorderBrush = Brushes.Aqua;
                SE_Enable_B.Background = (Brush)bc.ConvertFrom("#59999999");
                SE_Enable_B.BorderBrush = Brushes.Red;
            }
            else
            {
                SE_Disable_B.Background = (Brush)bc.ConvertFrom("#59999999");
                SE_Disable_B.BorderBrush = Brushes.Red;
                SE_Enable_B.Background = Brushes.Transparent;
                SE_Enable_B.BorderBrush = Brushes.Aqua;
            }
            if (SE_Lists.SelectedIndex == 0)
            {
                Select_SE_File_Count = SE_Get_File_Count("Capture_End");
            }
            else if (SE_Lists.SelectedIndex == 1)
            {
                Select_SE_File_Count = SE_Get_File_Count("Command");
            }
            else if (SE_Lists.SelectedIndex == 2)
            {
                Select_SE_File_Count = SE_Get_File_Count("Danyaku_SE");
            }
            else if (SE_Lists.SelectedIndex == 3)
            {
                Select_SE_File_Count = SE_Get_File_Count("Destroy");
            }
            else if (SE_Lists.SelectedIndex == 4)
            {
                Select_SE_File_Count = SE_Get_File_Count("Enable");
            }
            else if (SE_Lists.SelectedIndex == 5)
            {
                Select_SE_File_Count = SE_Get_File_Count("Enable_Special");
            }
            else if (SE_Lists.SelectedIndex == 6)
            {
                Select_SE_File_Count = SE_Get_File_Count("Musenki");
            }
            else if (SE_Lists.SelectedIndex == 7)
            {
                Select_SE_File_Count = SE_Get_File_Count("Nenryou_SE");
            }
            else if (SE_Lists.SelectedIndex == 8)
            {
                Select_SE_File_Count = SE_Get_File_Count("Not_Enable");
            }
            else if (SE_Lists.SelectedIndex == 9)
            {
                Select_SE_File_Count = SE_Get_File_Count("Reload");
            }
            else if (SE_Lists.SelectedIndex == 10)
            {
                Select_SE_File_Count = SE_Get_File_Count("Sixth");
            }
            else if (SE_Lists.SelectedIndex == 11)
            {
                Select_SE_File_Count = SE_Get_File_Count("Spot");
            }
            else if (SE_Lists.SelectedIndex == 12)
            {
                Select_SE_File_Count = SE_Get_File_Count("Timer");
            }
            else if (SE_Lists.SelectedIndex == 13)
            {
                Select_SE_File_Count = SE_Get_File_Count("Lock");
            }
            else if (SE_Lists.SelectedIndex == 14)
            {
                Select_SE_File_Count = SE_Get_File_Count("Unlock");
            }
            SE_Play_Number_T.Text = "1/" + Select_SE_File_Count;
        }
        private void SE_Play_B_Click(object sender, RoutedEventArgs e)
        {
            if (SE_Lists.SelectedIndex != -1)
            {
                SE_Play();
            }
        }
        async void SE_Play()
        {
            if (Player.playState != WMPPlayState.wmppsPlaying)
            {
                if (IsBusy)
                {
                    return;
                }
                IsSEStop = false;
                if (SE_Play_Index < 10)
                {
                    Player.URL = Sub_Code.File_Get_FileName_No_Extension(SE_Dir + "/" + Select_SE_Name + "_0" + SE_Play_Index);
                }
                else
                {
                    Player.URL = Sub_Code.File_Get_FileName_No_Extension(SE_Dir + "/" + Select_SE_Name + "_" + SE_Play_Index);
                }
                Player.controls.play();
                while (!IsSEStop)
                {
                    await Task.Delay(100);
                    if (Player.playState != WMPPlayState.wmppsPlaying)
                    {
                        break;
                    }
                }
                Player.controls.stop();
                if (SE_Play_Index < Select_SE_File_Count)
                {
                    SE_Play_Index++;
                    SE_Play_Number_T.Text = SE_Play_Index + "/" + Select_SE_File_Count;
                }
                else if (Select_SE_File_Count != 1 && SE_Play_Index == Select_SE_File_Count)
                {
                    SE_Play_Index = 1;
                    SE_Play_Number_T.Text = SE_Play_Index + "/" + Select_SE_File_Count;
                }
            }
            else
            {
                IsSEStop = true;
                await Task.Delay(101);
                SE_Play();
            }
        }
        //指定したファイル名のSEの数を取得
        //引数:パスではなく拡張子を含まないファイル名
        //戻り値:ファイル数
        int SE_Get_File_Count(string FileName)
        {
            int File_Count = 1;
            Select_SE_Name = FileName;
            while (true)
            {
                if (File_Count < 10)
                {
                    if (!Sub_Code.File_Exists(SE_Dir + "/" + FileName + "_0" + File_Count))
                    {
                        break;
                    }
                }
                else
                {
                    if (!Sub_Code.File_Exists(SE_Dir + "/" + FileName + "_" + File_Count))
                    {
                        break;
                    }
                }
                File_Count++;
            }
            return File_Count - 1;
        }
        private async void Exit_B_Click(object sender, RoutedEventArgs e)
        {
            if (Opacity >= 1)
            {
                IsBusy = true;
                Sub_Code.CreatingProject = false;
                Sub_Code.DVPL_Encode = false;
                while (Opacity > 0)
                {
                    Opacity -= 0.025;
                    await Task.Delay(1000 / 60);
                }
                Visibility = Visibility.Hidden;
                Voice_Lists.Items.Clear();
                Voice_Type.Clear();
                Voice_Type_Number.Clear();
                IsBusy = false;
            }
        }
        private void SE_Disable_B_Click(object sender, RoutedEventArgs e)
        {
            if (SE_Lists.SelectedIndex != -1)
            {
                if (Voice_Set.SE_Enable_List[SE_Lists.SelectedIndex])
                {
                    Voice_Set.SE_Enable_List[SE_Lists.SelectedIndex] = false;
                    SE_Disable_B.Background = (Brush)bc.ConvertFrom("#59999999");
                    SE_Disable_B.BorderBrush = Brushes.Red;
                    SE_Enable_B.Background = Brushes.Transparent;
                    SE_Enable_B.BorderBrush = Brushes.Aqua;
                    SE_Lists.Items[SE_Lists.SelectedIndex] = SE_Lists.Items[SE_Lists.SelectedIndex].ToString().Replace("| 有効", "| 無効");
                }
            }
        }
        private void SE_Enable_B_Click(object sender, RoutedEventArgs e)
        {
            if (SE_Lists.SelectedIndex != -1)
            {
                if (!Voice_Set.SE_Enable_List[SE_Lists.SelectedIndex])
                {
                    Voice_Set.SE_Enable_List[SE_Lists.SelectedIndex] = true;
                    SE_Disable_B.Background = Brushes.Transparent;
                    SE_Disable_B.BorderBrush = Brushes.Aqua;
                    SE_Enable_B.Background = (Brush)bc.ConvertFrom("#59999999");
                    SE_Enable_B.BorderBrush = Brushes.Red;
                    SE_Lists.Items[SE_Lists.SelectedIndex] = SE_Lists.Items[SE_Lists.SelectedIndex].ToString().Replace("| 無効", "| 有効");
                }
            }
        }
        private void Voice_Lists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Voice_Lists.SelectedIndex != -1)
            {
                Voice_Select_T.Text = "音声名:" + Voice_Type[Voice_Lists.SelectedIndex] + "|ファイル数:" + Voice_Type_Number[Voice_Lists.SelectedIndex] + "個";
            }
        }
        private void Voice_Lists_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Voice_Lists.SelectedIndex = -1;
            Voice_Select_T.Text = "";
        }
        private async void Save_B_Click(object sender, RoutedEventArgs e)
        {
            if (!IsBusy && Opacity >= 1)
            {
                IsBusy = true;
                while (Opacity > 0)
                {
                    Opacity -= 0.025;
                    await Task.Delay(1000 / 60);
                }
                Sub_Code.CreatingProject = true;
                Sub_Code.VolumeSet = Volume_Set_C.IsChecked.Value;
                Sub_Code.DVPL_Encode = DVPL_C.IsChecked.Value;
                IsBusy = false;
                Visibility = Visibility.Hidden;
            }
        }
        private void SE_Lists_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SE_Play_Number_T.Text = "0/0";
            SE_Disable_B.Background = Brushes.Transparent;
            SE_Disable_B.BorderBrush = Brushes.Aqua;
            SE_Enable_B.Background = Brushes.Transparent;
            SE_Enable_B.BorderBrush = Brushes.Aqua;
            SE_Lists.SelectedIndex = -1;
        }
        private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SE_Play_Number_T.Text = "0/0";
            SE_Disable_B.Background = Brushes.Transparent;
            SE_Disable_B.BorderBrush = Brushes.Aqua;
            SE_Enable_B.Background = Brushes.Transparent;
            SE_Enable_B.BorderBrush = Brushes.Aqua;
            SE_Lists.SelectedIndex = -1;
            Voice_Lists.SelectedIndex = -1;
            Voice_Select_T.Text = "";

        }
    }
}