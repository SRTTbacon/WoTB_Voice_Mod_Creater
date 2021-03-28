using System.Windows.Controls;

namespace WoTB_Voice_Mod_Creater.Multiplayer
{
    public partial class Voice_Control : UserControl
    {
        public Voice_Control()
        {
            InitializeComponent();
        }
        private void Ally_Damage_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("mikata");
        }
        private void Ammo_Damage_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("danyaku");
        }
        private void Enemy_Not_Damage_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("hikantuu");
        }
        private void Enemy_Enable_Damage_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("kantuu");
        }
        private void Enemy_Crit_Damage_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("tokusyu");
        }
        private void Enemy_Ricochet_Damage_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("tyoudan");
        }
        private void Commander_Kill_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("syatyou");
        }
        private void Driver_Kill_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("souzyuusyu");
        }
        private void Enemy_Fire_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("tekikasai");
        }
        private void Enemy_Kill_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("gekiha");
        }
        private void Engine_Damage_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("enjinhason");
        }
        private void Engine_Destroy_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("enjintaiha");
        }
        private void Engine_Functional_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("enjinhukkyuu");
        }
        private void Own_Fire_Start_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("kasai");
        }
        private void Own_Fire_Stop_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("syouka");
        }
        private void Fuel_Tank_Damage_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("nenryou");
        }
        private void Gun_Damage_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("housinhason");
        }
        private void Gun_Destroy_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("housintaiha");
        }
        private void Gun_Functional_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("housinhukkyuu");
        }
        private void Gunner_Kill_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("housyu");
        }
        private void Loader_Kill_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("soutensyu");
        }
        private void Radio_Damage_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("musen");
        }
        private void Radioman_Kill_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("musensyu");
        }
        private void Battle_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("battle");
        }
        private void Surveying_Damage_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("kansokuhason");
        }
        private void Surveying_Destroy_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("kansokutaiha");
        }
        private void Surveying_Functional_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("kansokuhukkyuu");
        }
        private void Track_Damage_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("ritaihason");
        }
        private void Track_Destroy_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("ritaitaiha");
        }
        private void Track_Functional_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("ritaihukkyuu");
        }
        private void Turret_Damage_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("houtouhason");
        }
        private void Turret_Destroy_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("houtoutaiha");
        }
        private void Turret_Functional_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("houtouhukkyuu");
        }
        private void Vehicle_Destroy_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Voice_Set.Voice_Set_Name("taiha");
        }
        private void Not_Use_B_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}