using System.Windows;
using System.Windows.Controls;

namespace WoTB_Voice_Mod_Creater.Class
{
    public partial class Voice_Control_Sub : UserControl
    {
        public Voice_Control_Sub()
        {
            InitializeComponent();
        }
        private void Enemy_Sight_B_Click(object sender, RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("hakken");
        }
        private void Ally_Sight_B_Click(object sender, RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("lamp");
        }
        private void OK_B_Click(object sender, RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("ryoukai");
        }
        private void No_B_Click(object sender, RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("kyohi");
        }
        private void Help_Click(object sender, RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("help");
        }
        private void Attack_B_Click(object sender, RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("attack");
        }
        private void Attack_This_Click(object sender, RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("attack_now");
        }
        private void Capture_B_Click(object sender, RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("capture");
        }
        private void Defense_B_Click(object sender, RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("defence");
        }
        private void Keep_B_Click(object sender, RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("keep");
        }
        private void Lock_B_Click(object sender, RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("lock");
        }
        private void Unlock_B_Click(object sender, RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("unlock");
        }
        private void Reload_B_Click(object sender, RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("reload");
        }
        private void Map_B_Click(object sender, RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("map");
        }
        private void Battle_End_B_Click(object sender, RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("battle_end");
        }
    }
}