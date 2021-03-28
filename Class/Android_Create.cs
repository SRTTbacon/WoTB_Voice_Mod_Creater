using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace WoTB_Voice_Mod_Creater.Class
{
    public class Android_Create
    {
        static string Temp_Dir = "";
        //Android用のプロジェクトを作成(プロジェクト名は関係なくingane_voice_ja.fsb、GUI_battle_streamed等が作成される)
        public static async Task Android_Project_Create(System.Windows.Controls.TextBlock Message_T, string Project_Name, string Voice_Dir, string SE_Dir)
        {
            try
            {
                string Dir_Path = Directory.GetCurrentDirectory();
                string Save_Path = Dir_Path + "/Projects/" + Project_Name;
                Message_T.Opacity = 1;
                if (!Directory.Exists(Save_Path))
                {
                    Directory.CreateDirectory(Save_Path);
                }
                if (!Directory.Exists(Dir_Path + "/Backup"))
                {
                    Directory.CreateDirectory(Dir_Path + "/Backup");
                }
                DateTime dt = DateTime.Now;
                string Time_Now = dt.Year + "." + dt.Month + "." + dt.Day + "." + dt.Hour + "." + dt.Minute + "." + dt.Second;
                Directory.CreateDirectory(Dir_Path + "/Backup/" + Time_Now);
                Message_T.Text = "音声のバックアップを行っています...";
                await Task.Delay(50);
                Sub_Code.DVPL_File_Copy(Voice_Set.WoTB_Path + "/Data/Sfx/ingame_voice_ja.fsb", Dir_Path + "/Backup/" + Time_Now + "/ingame_voice_ja.fsb", true);
                Sub_Code.DVPL_File_Copy(Voice_Set.WoTB_Path + "/Data/Sfx/GUI_battle_streamed.fsb", Dir_Path + "/Backup/" + Time_Now + "/GUI_battle_streamed.fsb", true);
                Sub_Code.DVPL_File_Copy(Voice_Set.WoTB_Path + "/Data/Sfx/GUI_notifications_FX_howitzer_load.fsb", Dir_Path + "/Backup/" + Time_Now + "/GUI_notifications_FX_howitzer_load.fsb", true);
                Sub_Code.DVPL_File_Copy(Voice_Set.WoTB_Path + "/Data/Sfx/GUI_quick_commands.fsb", Dir_Path + "/Backup/" + Time_Now + "/GUI_quick_commands.fsb", true);
                Sub_Code.DVPL_File_Copy(Voice_Set.WoTB_Path + "/Data/Sfx/GUI_sirene.fsb", Dir_Path + "/Backup/" + Time_Now + "/GUI_sirene.fsb", true);
                string[] Dirs = Directory.GetDirectories(Dir_Path + "/Backup");
                foreach (string Dir in Dirs)
                {
                    string Dir_Name_Only = Path.GetFileName(Dir);
                    if (Dir_Name_Only == Time_Now || Dir_Name_Only == "Main")
                    {
                        continue;
                    }
                    try
                    {
                        Directory.Delete(Dir, true);
                    }
                    catch (Exception e1)
                    {
                        Sub_Code.Error_Log_Write(e1.Message);
                    }
                }
                Message_T.Text = "音声のファイル名を変換しています...";
                await Task.Delay(50);
                Voice_Name_To_Ingame_Voice(Voice_Dir);
                Message_T.Text = "音声のファイル数を修正しています...";
                await Task.Delay(50);
                Ingame_Voice_Set_Number(Voice_Set.Special_Path + "/Fmod_Android_Create", Voice_Dir);
                Message_T.Text = "音声ファイルにSEを付けています...";
                await Task.Delay(50);
                await Ingame_Voice_In_SE_By_Dir(Voice_Dir, SE_Dir, Voice_Set.Special_Path + "/Fmod_Android_Create/Voices");
                Directory.Delete(Voice_Dir, true);
                Message_T.Text = "サーバーから必要なファイルをダウンロードしています...";
                await Task.Delay(50);
                Ingame_Voice_Move_Directory(Voice_Set.Special_Path + "/Fmod_Android_Create/Voices", true);
                Message_T.Text = "FSBファイルを作成中...";
                await Task.Delay(50);
                Android_FSB_Create(Voice_Set.Special_Path + "/Fmod_Android_Create/Voices", Voice_Set.Special_Path + "/Fmod_Android_Create/ingame_voice_ja.fsb");
                await Task.Delay(10);
                Android_FSB_Create(Voice_Set.Special_Path + "/Fmod_Android_Create/Voices/GUI_battle_streamed", Voice_Set.Special_Path + "/Fmod_Android_Create/GUI_battle_streamed.fsb");
                await Task.Delay(10);
                Android_FSB_Create(Voice_Set.Special_Path + "/Fmod_Android_Create/Voices/GUI_notifications_FX_howitzer_load", Voice_Set.Special_Path + "/Fmod_Android_Create/GUI_notifications_FX_howitzer_load.fsb");
                await Task.Delay(10);
                Android_FSB_Create(Voice_Set.Special_Path + "/Fmod_Android_Create/Voices/GUI_quick_commands", Voice_Set.Special_Path + "/Fmod_Android_Create/GUI_quick_commands.fsb");
                await Task.Delay(10);
                Android_FSB_Create(Voice_Set.Special_Path + "/Fmod_Android_Create/Voices/GUI_sirene", Voice_Set.Special_Path + "/Fmod_Android_Create/GUI_sirene.fsb");
                Message_T.Text = "ファイルをコピーしています...";
                await Task.Delay(50);
                Sub_Code.Directory_Copy(Voice_Set.Special_Path + "/Fmod_Android_Create/Voices", Voice_Dir);
                Directory.Delete(Voice_Set.Special_Path + "/Fmod_Android_Create/Voices", true);
                Sub_Code.File_Move(Voice_Set.Special_Path + "/Fmod_Android_Create/ingame_voice_ja.fsb", Save_Path + "/ingame_voice_ja.fsb", true);
                Sub_Code.File_Move(Voice_Set.Special_Path + "/Fmod_Android_Create/GUI_battle_streamed.fsb", Save_Path + "/GUI_battle_streamed.fsb", true);
                Sub_Code.File_Move(Voice_Set.Special_Path + "/Fmod_Android_Create/GUI_notifications_FX_howitzer_load.fsb", Save_Path + "/GUI_notifications_FX_howitzer_load.fsb", true);
                Sub_Code.File_Move(Voice_Set.Special_Path + "/Fmod_Android_Create/GUI_quick_commands.fsb", Save_Path + "/GUI_quick_commands.fsb", true);
                Sub_Code.File_Move(Voice_Set.Special_Path + "/Fmod_Android_Create/GUI_sirene.fsb", Save_Path + "/GUI_sirene.fsb", true);
            }
            catch (Exception e)
            {
                Message_T.Text = "エラー:正常に作成できませんでした。";
                MessageBox.Show(e.Message);
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        //*_01から計算してファイルがない番号にファイル名を変更する
        static string File_Rename_Number(string File_Path_Not_Ex_And_Number)
        {
            if (Temp_Dir == "")
            {
                return "";
            }
            int Number = 1;
            string Number_01;
            while (true)
            {
                if (Number < 10)
                {
                    if (!File.Exists(Sub_Code.File_Get_FileName_No_Extension(Temp_Dir + "/" + File_Path_Not_Ex_And_Number + "_0" + Number)))
                    {
                        Number_01 = "0" + Number;
                        break;
                    }
                }
                else
                {
                    if (!File.Exists(Sub_Code.File_Get_FileName_No_Extension(Temp_Dir + "/" + File_Path_Not_Ex_And_Number + "_" + Number)))
                    {
                        Number_01 = Number.ToString();
                        break;
                    }
                }
                Number++;
            }
            return Number_01;
        }
        //ファイル名をAndroidで使用できるファイル名に変更する
        public static void Voice_Name_To_Ingame_Voice(string Voice_Dir)
        {
            Temp_Dir = Voice_Dir;
            string[] Voices = Directory.GetFiles(Voice_Dir, "*", SearchOption.TopDirectoryOnly);
            foreach (string Voice_Now in Voices)
            {
                string Name_Only = Voice_Mod_Create.Get_Voice_Type_V1(Voice_Now);
                if (Name_Only == "reload")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/howitzer_load_" + File_Rename_Number("howitzer_load") + Path.GetExtension(Voice_Now), false);
                }
                else if (Name_Only == "hakken")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/enemy_sighted" + Path.GetExtension(Voice_Now), false);
                    if (File.Exists(Voice_Now))
                    {
                        File.Delete(Voice_Now);
                    }
                }
                else if (Name_Only == "lamp")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/lamp_" + File_Rename_Number("lamp") + Path.GetExtension(Voice_Now), false);
                }
                else if (Name_Only == "battle_end")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/capture_end" + Path.GetExtension(Voice_Now), false);
                    if (File.Exists(Voice_Now))
                    {
                        File.Delete(Voice_Now);
                    }
                }
                else if (Name_Only == "lock")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/auto_target_on" + Path.GetExtension(Voice_Now), false);
                    if (File.Exists(Voice_Now))
                    {
                        File.Delete(Voice_Now);
                    }
                }
                else if (Name_Only == "unlock")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/auto_target_off" + Path.GetExtension(Voice_Now), false);
                    if (File.Exists(Voice_Now))
                    {
                        File.Delete(Voice_Now);
                    }
                }
                else if (Name_Only == "mikata")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/ally_killed_by_player_" + File_Rename_Number("ally_killed_by_player") + Path.GetExtension(Voice_Now), false);
                }
                else if (Name_Only == "taiha")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/vehicle_destroyed_" + File_Rename_Number("vehicle_destroyed") + Path.GetExtension(Voice_Now), false);
                }
                else if (Name_Only == "kantuu")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/armor_pierced_by_player_" + File_Rename_Number("armor_pierced_by_player") + Path.GetExtension(Voice_Now), false);
                }
                else if (Name_Only == "tokusyu")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/armor_pierced_crit_by_player_" + File_Rename_Number("armor_pierced_crit_by_player") + Path.GetExtension(Voice_Now), false);
                }
                else if (Name_Only == "hikantuu")
                {
                    if (File.Exists(Voice_Now))
                    {
                        File.Copy(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/armor_not_pierced_by_player_" + File_Rename_Number("armor_not_pierced_by_player") + Path.GetExtension(Voice_Now), true);
                        File.Copy(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/armor_ricochet_by_player_" + File_Rename_Number("armor_ricochet_by_player") + Path.GetExtension(Voice_Now), true);
                        File.Delete(Voice_Now);
                    }
                }
                else if (Name_Only == "tekikasai")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/enemy_fire_started_by_player_" + File_Rename_Number("enemy_fire_started_by_player") + Path.GetExtension(Voice_Now), false);
                }
                else if (Name_Only == "battle")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/start_battle_" + File_Rename_Number("start_battle") + Path.GetExtension(Voice_Now), false);
                }
                else if (Name_Only == "ryoukai")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/quick_commands_positive" + Path.GetExtension(Voice_Now), false);
                    if (File.Exists(Voice_Now))
                    {
                        File.Delete(Voice_Now);
                    }
                }
                else if (Name_Only == "kyohi")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/quick_commands_negative" + Path.GetExtension(Voice_Now), false);
                    if (File.Exists(Voice_Now))
                    {
                        File.Delete(Voice_Now);
                    }
                }
                else if (Name_Only == "keep")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/quick_commands_reloading" + Path.GetExtension(Voice_Now), false);
                    if (File.Exists(Voice_Now))
                    {
                        File.Delete(Voice_Now);
                    }
                }
                else if (Name_Only == "help")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/quick_commands_help_me" + Path.GetExtension(Voice_Now), false);
                    if (File.Exists(Voice_Now))
                    {
                        File.Delete(Voice_Now);
                    }
                }
                else if (Name_Only == "capture")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/quick_commands_capture_base" + Path.GetExtension(Voice_Now), false);
                    if (File.Exists(Voice_Now))
                    {
                        File.Delete(Voice_Now);
                    }
                }
                else if (Name_Only == "defence")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/quick_commands_defend_base" + Path.GetExtension(Voice_Now), false);
                    if (File.Exists(Voice_Now))
                    {
                        File.Delete(Voice_Now);
                    }
                }
                else if (Name_Only == "attack")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/quick_commands_attack" + Path.GetExtension(Voice_Now), false);
                    if (File.Exists(Voice_Now))
                    {
                        File.Delete(Voice_Now);
                    }
                }
                else if (Name_Only == "attack_now")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/quick_commands_attack_target" + Path.GetExtension(Voice_Now), false);
                    if (File.Exists(Voice_Now))
                    {
                        File.Delete(Voice_Now);
                    }
                }
                else if (Name_Only == "kasai")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/fire_started_" + File_Rename_Number("fire_started") + Path.GetExtension(Voice_Now), false);
                }
                else if (Name_Only == "syouka")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/fire_stopped_" + File_Rename_Number("fire_stopped") + Path.GetExtension(Voice_Now), false);
                }
                else if (Name_Only == "ritaitaiha")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/track_destroyed_" + File_Rename_Number("track_destroyed") + Path.GetExtension(Voice_Now), false);
                }
                else if (Name_Only == "ritaihason")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/track_damaged_" + File_Rename_Number("track_damaged") + Path.GetExtension(Voice_Now), false);
                }
                else if (Name_Only == "ritaihukkyuu")
                {
                    if (File.Exists(Voice_Now))
                    {
                        File.Copy(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/track_functional_" + File_Rename_Number("track_functional") + Path.GetExtension(Voice_Now), true);
                        File.Copy(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/track_functional_can_move_" + File_Rename_Number("track_functional_can_move") + Path.GetExtension(Voice_Now), true);
                        File.Delete(Voice_Now);
                    }
                }
                else if (Name_Only == "housinhason")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/gun_damaged_" + File_Rename_Number("gun_damaged") + Path.GetExtension(Voice_Now), false);
                }
                else if (Name_Only == "housintaiha")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/gun_destroyed_" + File_Rename_Number("gun_destroyed") + Path.GetExtension(Voice_Now), false);
                }
                else if (Name_Only == "housinhukkyuu")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/gun_functional_" + File_Rename_Number("gun_functional") + Path.GetExtension(Voice_Now), false);
                }
                else if (Name_Only == "musen")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/radio_damaged_" + File_Rename_Number("radio_damaged") + Path.GetExtension(Voice_Now), false);
                }
                else if (Name_Only == "kansokuhason")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/surveying_devices_damaged_" + File_Rename_Number("surveying_devices_damaged") + Path.GetExtension(Voice_Now), false);
                }
                else if (Name_Only == "kansokuhukkyuu")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/surveying_devices_functional_" + File_Rename_Number("surveying_devices_functional") + Path.GetExtension(Voice_Now), false);
                }
                else if (Name_Only == "kansokutaiha")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/surveying_devices_destroyed_" + File_Rename_Number("surveying_devices_destroyed") + Path.GetExtension(Voice_Now), false);
                }
                else if (Name_Only == "houtouhason")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/turret_rotator_damaged_" + File_Rename_Number("turret_rotator_damaged") + Path.GetExtension(Voice_Now), false);
                }
                else if (Name_Only == "houtouhukkyuu")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/turret_rotator_functional_" + File_Rename_Number("turret_rotator_functional") + Path.GetExtension(Voice_Now), false);
                }
                else if (Name_Only == "houtoutaiha")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/turret_rotator_destroyed_" + File_Rename_Number("turret_rotator_destroyed") + Path.GetExtension(Voice_Now), false);
                }
                else if (Name_Only == "danyaku")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/ammo_bay_damaged_" + File_Rename_Number("ammo_bay_damaged") + Path.GetExtension(Voice_Now), false);
                }
                else if (Name_Only == "enjinhukkyuu")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/engine_functional_" + File_Rename_Number("engine_functional") + Path.GetExtension(Voice_Now), false);
                }
                else if (Name_Only == "enjintaiha")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/engine_destroyed_" + File_Rename_Number("engine_destroyed") + Path.GetExtension(Voice_Now), false);
                }
                else if (Name_Only == "enjinhason")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/engine_damaged_" + File_Rename_Number("engine_damaged") + Path.GetExtension(Voice_Now), false);
                }
                else if (Name_Only == "nenryou")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/fuel_tank_damaged_" + File_Rename_Number("fuel_tank_damaged") + Path.GetExtension(Voice_Now), false);
                }
                else if (Name_Only == "musensyu")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/radioman_killed_" + File_Rename_Number("radioman_killed") + Path.GetExtension(Voice_Now), false);
                }
                else if (Name_Only == "soutensyu")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/loader_killed_" + File_Rename_Number("loader_killed") + Path.GetExtension(Voice_Now), false);
                }
                else if (Name_Only == "housyu")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/gunner_killed_" + File_Rename_Number("gunner_killed") + Path.GetExtension(Voice_Now), false);
                }
                else if (Name_Only == "souzyuusyu")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/driver_killed_" + File_Rename_Number("driver_killed") + Path.GetExtension(Voice_Now), false);
                }
                else if (Name_Only == "syatyou")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/commander_killed_" + File_Rename_Number("commander_killed") + Path.GetExtension(Voice_Now), false);
                }
                else if (Name_Only == "gekiha")
                {
                    Sub_Code.File_Move(Voice_Now, Path.GetDirectoryName(Voice_Now) + "/enemy_killed_by_player_" + File_Rename_Number("enemy_killed_by_player") + Path.GetExtension(Voice_Now), false);
                }
                else
                {
                    File.Delete(Voice_Now);
                }
            }
        }
        //それぞれのフォルダへ移動(第六感やリロードなど)
        public static void Ingame_Voice_Move_Directory(string Voice_Dir, bool IsAndroid)
        {
            if (Directory.Exists(Voice_Dir + "/GUI_battle_streamed"))
            {
                Directory.Delete(Voice_Dir + "/GUI_battle_streamed", true);
            }
            if (Directory.Exists(Voice_Dir + "/GUI_notifications_FX_howitzer_load"))
            {
                Directory.Delete(Voice_Dir + "/GUI_notifications_FX_howitzer_load", true);
            }
            if (Directory.Exists(Voice_Dir + "/GUI_quick_commands"))
            {
                Directory.Delete(Voice_Dir + "/GUI_quick_commands", true);
            }
            if (Directory.Exists(Voice_Dir + "/GUI_sirene"))
            {
                Directory.Delete(Voice_Dir + "/GUI_sirene", true);
            }
            if (IsAndroid)
            {
                Voice_Set.FTP_Server.DownloadDirectory(Voice_Dir + "/GUI_battle_streamed", "/WoTB_Voice_Mod/Android/GUI_battle_streamed");
                Voice_Set.FTP_Server.DownloadDirectory(Voice_Dir + "/GUI_notifications_FX_howitzer_load", "/WoTB_Voice_Mod/Android/GUI_notifications_FX_howitzer_load");
                Voice_Set.FTP_Server.DownloadDirectory(Voice_Dir + "/GUI_quick_commands", "/WoTB_Voice_Mod/Android/GUI_quick_commands");
                Voice_Set.FTP_Server.DownloadDirectory(Voice_Dir + "/GUI_sirene", "/WoTB_Voice_Mod/Android/GUI_sirene");
            }
            else
            {
                Directory.CreateDirectory(Voice_Dir + "/GUI_battle_streamed");
                Directory.CreateDirectory(Voice_Dir + "/GUI_notifications_FX_howitzer_load");
                Directory.CreateDirectory(Voice_Dir + "/GUI_quick_commands");
                Directory.CreateDirectory(Voice_Dir + "/GUI_sirene");
            }
            if (Voice_Set.SE_Enable_Disable[1])
            {
                Sub_Code.File_Delete(Voice_Dir + "/GUI_quick_commands/quick_commands_attack");
                Sub_Code.File_Delete(Voice_Dir + "/GUI_quick_commands/quick_commands_attack_target");
                Sub_Code.File_Delete(Voice_Dir + "/GUI_quick_commands/quick_commands_capture_base");
                Sub_Code.File_Delete(Voice_Dir + "/GUI_quick_commands/quick_commands_defend_base");
                Sub_Code.File_Delete(Voice_Dir + "/GUI_quick_commands/quick_commands_help_me");
                Sub_Code.File_Delete(Voice_Dir + "/GUI_quick_commands/quick_commands_negative");
                Sub_Code.File_Delete(Voice_Dir + "/GUI_quick_commands/quick_commands_positive");
                Sub_Code.File_Delete(Voice_Dir + "/GUI_quick_commands/quick_commands_reloading");
                File.Copy(Voice_Set.Special_Path + "/SE/Command_01.wav", Voice_Dir + "/GUI_quick_commands/quick_commands_attack.wav", true);
                File.Copy(Voice_Set.Special_Path + "/SE/Command_01.wav", Voice_Dir + "/GUI_quick_commands/quick_commands_attack_target.wav", true);
                File.Copy(Voice_Set.Special_Path + "/SE/Command_01.wav", Voice_Dir + "/GUI_quick_commands/quick_commands_capture_base.wav", true);
                File.Copy(Voice_Set.Special_Path + "/SE/Command_01.wav", Voice_Dir + "/GUI_quick_commands/quick_commands_defend_base.wav", true);
                File.Copy(Voice_Set.Special_Path + "/SE/Command_01.wav", Voice_Dir + "/GUI_quick_commands/quick_commands_help_me.wav", true);
                File.Copy(Voice_Set.Special_Path + "/SE/Command_01.wav", Voice_Dir + "/GUI_quick_commands/quick_commands_negative.wav", true);
                File.Copy(Voice_Set.Special_Path + "/SE/Command_01.wav", Voice_Dir + "/GUI_quick_commands/quick_commands_positive.wav", true);
                File.Copy(Voice_Set.Special_Path + "/SE/Command_01.wav", Voice_Dir + "/GUI_quick_commands/quick_commands_reloading.wav", true);
            }
            if (Sub_Code.File_Exists(Voice_Dir + "/auto_target_off"))
            {
                Sub_Code.File_Move_V2(Voice_Dir + "/auto_target_off", Voice_Dir + "/GUI_battle_streamed/auto_target_off", true);
            }
            if (Sub_Code.File_Exists(Voice_Dir + "/auto_target_on"))
            {
                Sub_Code.File_Move_V2(Voice_Dir + "/auto_target_on", Voice_Dir + "/GUI_battle_streamed/auto_target_on", true);
            }
            if (Sub_Code.File_Exists(Voice_Dir + "/enemy_sighted"))
            {
                Sub_Code.File_Move_V2(Voice_Dir + "/enemy_sighted", Voice_Dir + "/GUI_battle_streamed/enemy_sighted", true);
            }
            if (Sub_Code.File_Exists(Voice_Dir + "/howitzer_load_01"))
            {
                Sub_Code.File_Move_V2(Voice_Dir + "/howitzer_load_01", Voice_Dir + "/GUI_notifications_FX_howitzer_load/howitzer_load_01", true);
                Sub_Code.File_Move_V2(Voice_Dir + "/howitzer_load_03", Voice_Dir + "/GUI_notifications_FX_howitzer_load/howitzer_load_03", true);
                Sub_Code.File_Move_V2(Voice_Dir + "/howitzer_load_04", Voice_Dir + "/GUI_notifications_FX_howitzer_load/howitzer_load_04", true);
                Sub_Code.File_Move_V2(Voice_Dir + "/howitzer_load_05", Voice_Dir + "/GUI_notifications_FX_howitzer_load/howitzer_load_05", true);
                Sub_Code.File_Delete(Voice_Dir + "/howitzer_load_02");
            }
            if (Sub_Code.File_Exists(Voice_Dir + "/lamp_01"))
            {
                Sub_Code.File_Move_V2(Voice_Dir + "/lamp_01", Voice_Dir + "/GUI_battle_streamed/lamp_01", true);
            }
            if (Sub_Code.File_Exists(Voice_Dir + "/quick_commands_attack"))
            {
                Sub_Code.File_Move_V2(Voice_Dir + "/quick_commands_attack", Voice_Dir + "/GUI_quick_commands/quick_commands_attack", true);
            }
            if (Sub_Code.File_Exists(Voice_Dir + "/quick_commands_attack_target"))
            {
                Sub_Code.File_Move_V2(Voice_Dir + "/quick_commands_attack_target", Voice_Dir + "/GUI_quick_commands/quick_commands_attack_target", true);
            }
            if (Sub_Code.File_Exists(Voice_Dir + "/quick_commands_capture_base"))
            {
                Sub_Code.File_Move_V2(Voice_Dir + "/quick_commands_capture_base", Voice_Dir + "/GUI_quick_commands/quick_commands_capture_base", true);
            }
            if (Sub_Code.File_Exists(Voice_Dir + "/quick_commands_defend_base"))
            {
                Sub_Code.File_Move_V2(Voice_Dir + "/quick_commands_defend_base", Voice_Dir + "/GUI_quick_commands/quick_commands_defend_base", true);
            }
            if (Sub_Code.File_Exists(Voice_Dir + "/quick_commands_help_me"))
            {
                Sub_Code.File_Move_V2(Voice_Dir + "/quick_commands_help_me", Voice_Dir + "/GUI_quick_commands/quick_commands_help_me", true);
            }
            if (Sub_Code.File_Exists(Voice_Dir + "/quick_commands_negative"))
            {
                Sub_Code.File_Move_V2(Voice_Dir + "/quick_commands_negative", Voice_Dir + "/GUI_quick_commands/quick_commands_negative", true);
            }
            if (Sub_Code.File_Exists(Voice_Dir + "/quick_commands_positive"))
            {
                Sub_Code.File_Move_V2(Voice_Dir + "/quick_commands_positive", Voice_Dir + "/GUI_quick_commands/quick_commands_positive", true);
            }
            if (Sub_Code.File_Exists(Voice_Dir + "/quick_commands_reloading"))
            {
                Sub_Code.File_Move_V2(Voice_Dir + "/quick_commands_reloading", Voice_Dir + "/GUI_quick_commands/quick_commands_reloading", true);
            }
        }
        //音声を指定したファイル数にする(ファイルがなければ依存のファイルで補い、ファイル数を超えていれば削除)
        static void Set_Voice_Number(string From_Dir, string Voice_Dir, string Voice_Type, int Number)
        {
            int File_Number = 0;
            for (int Number2 = 0; Number2 <= 9; Number2++)
            {
                string[] Files2 = Directory.GetFiles(Voice_Dir, Voice_Type + "_" + Number2 + "*", SearchOption.TopDirectoryOnly);
                File_Number += Files2.Length;
            }
            if (File_Number == 0)
            {
                for (int Number2 = 1; Number2 <= Number; Number2++)
                {
                    try
                    {
                        if (Number2 < 10)
                        {
                            File.Copy(From_Dir + "/Not_Voice.mp3", Voice_Dir + "/" + Voice_Type + "_0" + Number2 + ".mp3", true);
                        }
                        else
                        {
                            File.Copy(From_Dir + "/Not_Voice.mp3", Voice_Dir + "/" + Voice_Type + "_" + Number2 + ".mp3", true);
                        }
                    }
                    catch
                    {
                    }
                }
                return;
            }
            if (File_Number > Number)
            {
                for (int Number2 = Number + 1; Number2 <= File_Number + 1; Number2++)
                {
                    try
                    {
                        if (Number2 < 10)
                        {
                            File.Delete(Sub_Code.File_Get_FileName_No_Extension(Voice_Dir + "/" + Voice_Type + "_0" + Number2));
                        }
                        else
                        {
                            File.Delete(Sub_Code.File_Get_FileName_No_Extension(Voice_Dir + "/" + Voice_Type + "_" + Number2));
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            else if (File_Number < Number)
            {
                Random r = new Random();
                for (int Number2 = File_Number; Number2 <= Number; Number2++)
                {
                    int Test = 0;
                    start:
                    Test++;
                    try
                    {
                        int r2 = r.Next(1, File_Number + 1);
                        string r3;
                        if (r2 < 10)
                        {
                            r3 = "0" + r2;
                        }
                        else
                        {
                            r3 = r2.ToString();
                        }
                        string FilePath = Sub_Code.File_Get_FileName_No_Extension(Voice_Dir + "/" + Voice_Type + "_" + r3);
                        if (Number2 < 10)
                        {
                            File.Copy(FilePath, Voice_Dir + "/" + Voice_Type + "_0" + Number2 + Path.GetExtension(FilePath), true);
                        }
                        else
                        {
                            File.Copy(FilePath, Voice_Dir + "/" + Voice_Type + "_" + Number2 + Path.GetExtension(FilePath), true);
                        }
                    }
                    catch
                    {
                        if (Test >= 10)
                        {
                            continue;
                        }
                        goto start;
                    }
                }
            }
        }
        //音声の数をWoTBに合わせる(音声の数が規定以上だった場合その音声は削除され、なければ今ある音声の中からランダムでコピーされる)
        public static void Ingame_Voice_Set_Number(string FromDir, string Voice_Dir)
        {
            if (!Sub_Code.File_Exists(Voice_Dir + "/auto_target_off"))
            {
                File.Copy(FromDir + "/Not_Voice.mp3", Voice_Dir + "/auto_target_off.mp3", true);
            }
            if (!Sub_Code.File_Exists(Voice_Dir + "/auto_target_on"))
            {
                File.Copy(FromDir + "/Not_Voice.mp3", Voice_Dir + "/auto_target_on.mp3", true);
            }
            if (!Sub_Code.File_Exists(Voice_Dir + "/enemy_sighted"))
            {
                File.Copy(FromDir + "/Not_Voice.mp3", Voice_Dir + "/enemy_sighted.mp3", true);
            }
            if (!Sub_Code.File_Exists(Voice_Dir + "/quick_commands_positive"))
            {
                File.Copy(FromDir + "/Not_Voice.mp3", Voice_Dir + "/quick_commands_positive.mp3", true);
            }
            if (!Sub_Code.File_Exists(Voice_Dir + "/quick_commands_negative"))
            {
                File.Copy(FromDir + "/Not_Voice.mp3", Voice_Dir + "/quick_commands_negative.mp3", true);
            }
            if (!Sub_Code.File_Exists(Voice_Dir + "/quick_commands_reloading"))
            {
                File.Copy(FromDir + "/Not_Voice.mp3", Voice_Dir + "/quick_commands_reloading.mp3", true);
            }
            if (!Sub_Code.File_Exists(Voice_Dir + "/quick_commands_help_me"))
            {
                File.Copy(FromDir + "/Not_Voice.mp3", Voice_Dir + "/quick_commands_help_me.mp3", true);
            }
            if (!Sub_Code.File_Exists(Voice_Dir + "/quick_commands_capture_base"))
            {
                File.Copy(FromDir + "/Not_Voice.mp3", Voice_Dir + "/quick_commands_capture_base.mp3", true);
            }
            if (!Sub_Code.File_Exists(Voice_Dir + "/quick_commands_defend_base"))
            {
                File.Copy(FromDir + "/Not_Voice.mp3", Voice_Dir + "/quick_commands_defend_base.mp3", true);
            }
            if (!Sub_Code.File_Exists(Voice_Dir + "/quick_commands_attack"))
            {
                File.Copy(FromDir + "/Not_Voice.mp3", Voice_Dir + "/quick_commands_attack.mp3", true);
            }
            if (!Sub_Code.File_Exists(Voice_Dir + "/quick_commands_attack_target"))
            {
                File.Copy(FromDir + "/Not_Voice.mp3", Voice_Dir + "/quick_commands_attack_target.mp3", true);
            }
            if (!Sub_Code.File_Exists(Voice_Dir + "/capture_end"))
            {
                File.Copy(FromDir + "/Not_Voice.mp3", Voice_Dir + "/capture_end.mp3", true);
            }
            Set_Voice_Number(FromDir, Voice_Dir, "howitzer_load", 5);
            Set_Voice_Number(FromDir, Voice_Dir, "lamp", 1);
            Set_Voice_Number(FromDir, Voice_Dir, "ally_killed_by_player", 2);
            Set_Voice_Number(FromDir, Voice_Dir, "vehicle_destroyed", 3);
            Set_Voice_Number(FromDir, Voice_Dir, "armor_pierced_by_player", 12);
            Set_Voice_Number(FromDir, Voice_Dir, "armor_pierced_crit_by_player", 9);
            Set_Voice_Number(FromDir, Voice_Dir, "armor_not_pierced_by_player", 5);
            Set_Voice_Number(FromDir, Voice_Dir, "armor_ricochet_by_player", 7);
            Set_Voice_Number(FromDir, Voice_Dir, "enemy_fire_started_by_player", 4);
            Set_Voice_Number(FromDir, Voice_Dir, "start_battle", 8);
            Set_Voice_Number(FromDir, Voice_Dir, "fire_started", 2);
            Set_Voice_Number(FromDir, Voice_Dir, "fire_stopped", 3);
            Set_Voice_Number(FromDir, Voice_Dir, "track_destroyed", 4);
            Set_Voice_Number(FromDir, Voice_Dir, "track_damaged", 4);
            Set_Voice_Number(FromDir, Voice_Dir, "track_functional", 4);
            Set_Voice_Number(FromDir, Voice_Dir, "track_functional_can_move", 5);
            Set_Voice_Number(FromDir, Voice_Dir, "gun_damaged", 4);
            Set_Voice_Number(FromDir, Voice_Dir, "gun_destroyed", 3);
            Set_Voice_Number(FromDir, Voice_Dir, "gun_functional", 4);
            Set_Voice_Number(FromDir, Voice_Dir, "radio_damaged", 5);
            Set_Voice_Number(FromDir, Voice_Dir, "surveying_devices_damaged", 5);
            Set_Voice_Number(FromDir, Voice_Dir, "surveying_devices_functional", 6);
            Set_Voice_Number(FromDir, Voice_Dir, "surveying_devices_destroyed", 6);
            Set_Voice_Number(FromDir, Voice_Dir, "turret_rotator_damaged", 2);
            Set_Voice_Number(FromDir, Voice_Dir, "turret_rotator_functional", 2);
            Set_Voice_Number(FromDir, Voice_Dir, "turret_rotator_destroyed", 2);
            Set_Voice_Number(FromDir, Voice_Dir, "ammo_bay_damaged", 3);
            Set_Voice_Number(FromDir, Voice_Dir, "engine_functional", 2);
            Set_Voice_Number(FromDir, Voice_Dir, "engine_destroyed", 3);
            Set_Voice_Number(FromDir, Voice_Dir, "engine_damaged", 5);
            Set_Voice_Number(FromDir, Voice_Dir, "fuel_tank_damaged", 4);
            Set_Voice_Number(FromDir, Voice_Dir, "radioman_killed", 1);
            Set_Voice_Number(FromDir, Voice_Dir, "loader_killed", 2);
            Set_Voice_Number(FromDir, Voice_Dir, "gunner_killed", 3);
            Set_Voice_Number(FromDir, Voice_Dir, "driver_killed", 3);
            Set_Voice_Number(FromDir, Voice_Dir, "commander_killed", 3);
            Set_Voice_Number(FromDir, Voice_Dir, "enemy_killed_by_player", 9);
        }
        //指定した音声にSEを合わせる
        static void Ingame_Voice_In_SE_By_FileName(string Voice_Path, string SE_Path, string To_Dir)
        {
            string OutPath = To_Dir + "/" + Path.GetFileName(Voice_Path);
            StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Encode_Mp3/Voice_In_SE.bat");
            stw.WriteLine("chcp 65001");
            stw.Write(Voice_Set.Special_Path + "/Encode_Mp3/ffmpeg.exe -y -i " + Voice_Path + " -i " + SE_Path + " -filter_complex amix=inputs=2:duration=longest " + OutPath);
            stw.Close();
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = Voice_Set.Special_Path + "/Encode_Mp3/Voice_In_SE.bat",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process p = Process.Start(processStartInfo);
            p.WaitForExit();
            File.Delete(Voice_Set.Special_Path + "/Encode_Mp3/Voice_In_SE.bat");
        }
        //指定したファイルの再生速度を変更(１より下にした場合速度が落ちる)
        static void Sound_Speed_Change(string From_Path, string To_Path, double Rate)
        {
            StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Encode_Mp3/Rate_Change.bat");
            stw.WriteLine("chcp 65001");
            stw.Write(Voice_Set.Special_Path + "/Encode_Mp3/ffmpeg.exe -y -i " + From_Path + " -af aresample=44100,asetrate=44100*" + Rate + " " + To_Path);
            stw.Close();
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = Voice_Set.Special_Path + "/Encode_Mp3/Rate_Change.bat",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process p = Process.Start(processStartInfo);
            p.WaitForExit();
            File.Delete(Voice_Set.Special_Path + "/Encode_Mp3/Rate_Change.bat");
        }
        //指定した音声フォルダの中身のファイルすべてにSEを付ける(SEが有効な場合のみ)
        public static async Task Ingame_Voice_In_SE_By_Dir(string Voice_Dir, string SE_Dir, string To_Dir)
        {
            try
            {
                if (Directory.Exists(To_Dir))
                {
                    Directory.Delete(To_Dir, true);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("エラーが発生しました。\n" + e.Message);
                Sub_Code.Error_Log_Write(e.Message);
                return;
            }
            if (Sub_Code.File_Exists(Voice_Dir + "/howitzer_load_01"))
            {
                FileInfo f = new FileInfo(Sub_Code.File_Get_FileName_No_Extension(Voice_Dir + "/howitzer_load_01"));
                if (f.Length == 3760)
                {
                    File.Delete(Sub_Code.File_Get_FileName_No_Extension(Voice_Dir + "/howitzer_load_01"));
                    File.Delete(Sub_Code.File_Get_FileName_No_Extension(Voice_Dir + "/howitzer_load_02"));
                    File.Delete(Sub_Code.File_Get_FileName_No_Extension(Voice_Dir + "/howitzer_load_03"));
                    File.Delete(Sub_Code.File_Get_FileName_No_Extension(Voice_Dir + "/howitzer_load_04"));
                    File.Delete(Sub_Code.File_Get_FileName_No_Extension(Voice_Dir + "/howitzer_load_05"));
                }
            }
            if (Sub_Code.File_Exists(Voice_Dir + "/enemy_sighted"))
            {
                FileInfo f = new FileInfo(Sub_Code.File_Get_FileName_No_Extension(Voice_Dir + "/enemy_sighted"));
                if (f.Length == 3760)
                {
                    File.Delete(Sub_Code.File_Get_FileName_No_Extension(Voice_Dir + "/enemy_sighted"));
                }
            }
            if (Sub_Code.File_Exists(Voice_Dir + "/lamp_01"))
            {
                FileInfo f = new FileInfo(Sub_Code.File_Get_FileName_No_Extension(Voice_Dir + "/lamp_01"));
                if (f.Length == 3760)
                {
                    File.Delete(Sub_Code.File_Get_FileName_No_Extension(Voice_Dir + "/lamp_01"));
                }
            }
            if (Sub_Code.File_Exists(Voice_Dir + "/auto_target_off"))
            {
                FileInfo f = new FileInfo(Sub_Code.File_Get_FileName_No_Extension(Voice_Dir + "/auto_target_off"));
                if (f.Length == 3760)
                {
                    File.Delete(Sub_Code.File_Get_FileName_No_Extension(Voice_Dir + "/auto_target_off"));
                }
            }
            if (Sub_Code.File_Exists(Voice_Dir + "/auto_target_on"))
            {
                FileInfo f = new FileInfo(Sub_Code.File_Get_FileName_No_Extension(Voice_Dir + "/auto_target_on"));
                if (f.Length == 3760)
                {
                    File.Delete(Sub_Code.File_Get_FileName_No_Extension(Voice_Dir + "/auto_target_on"));
                }
            }
            if (Sub_Code.File_Exists(Voice_Dir + "/quick_commands_positive"))
            {
                FileInfo f = new FileInfo(Sub_Code.File_Get_FileName_No_Extension(Voice_Dir + "/quick_commands_positive"));
                if (f.Length == 3760)
                {
                    File.Delete(Sub_Code.File_Get_FileName_No_Extension(Voice_Dir + "/quick_commands_positive"));
                }
            }
            if (Sub_Code.File_Exists(Voice_Dir + "/quick_commands_negative"))
            {
                FileInfo f = new FileInfo(Sub_Code.File_Get_FileName_No_Extension(Voice_Dir + "/quick_commands_negative"));
                if (f.Length == 3760)
                {
                    File.Delete(Sub_Code.File_Get_FileName_No_Extension(Voice_Dir + "/quick_commands_negative"));
                }
            }
            if (Sub_Code.File_Exists(Voice_Dir + "/quick_commands_reloading"))
            {
                FileInfo f = new FileInfo(Sub_Code.File_Get_FileName_No_Extension(Voice_Dir + "/quick_commands_reloading"));
                if (f.Length == 3760)
                {
                    File.Delete(Sub_Code.File_Get_FileName_No_Extension(Voice_Dir + "/quick_commands_reloading"));
                }
            }
            if (Sub_Code.File_Exists(Voice_Dir + "/quick_commands_help_me"))
            {
                FileInfo f = new FileInfo(Sub_Code.File_Get_FileName_No_Extension(Voice_Dir + "/quick_commands_help_me"));
                if (f.Length == 3760)
                {
                    File.Delete(Sub_Code.File_Get_FileName_No_Extension(Voice_Dir + "/quick_commands_help_me"));
                }
            }
            if (Sub_Code.File_Exists(Voice_Dir + "/quick_commands_capture_base"))
            {
                FileInfo f = new FileInfo(Sub_Code.File_Get_FileName_No_Extension(Voice_Dir + "/quick_commands_capture_base"));
                if (f.Length == 3760)
                {
                    File.Delete(Sub_Code.File_Get_FileName_No_Extension(Voice_Dir + "/quick_commands_capture_base"));
                }
            }
            if (Sub_Code.File_Exists(Voice_Dir + "/quick_commands_defend_base"))
            {
                FileInfo f = new FileInfo(Sub_Code.File_Get_FileName_No_Extension(Voice_Dir + "/quick_commands_defend_base"));
                if (f.Length == 3760)
                {
                    File.Delete(Sub_Code.File_Get_FileName_No_Extension(Voice_Dir + "/quick_commands_defend_base"));
                }
            }
            if (Sub_Code.File_Exists(Voice_Dir + "/quick_commands_attack"))
            {
                FileInfo f = new FileInfo(Sub_Code.File_Get_FileName_No_Extension(Voice_Dir + "/quick_commands_attack"));
                if (f.Length == 3760)
                {
                    File.Delete(Sub_Code.File_Get_FileName_No_Extension(Voice_Dir + "/quick_commands_attack"));
                }
            }
            if (Sub_Code.File_Exists(Voice_Dir + "/quick_commands_attack_target"))
            {
                FileInfo f = new FileInfo(Sub_Code.File_Get_FileName_No_Extension(Voice_Dir + "/quick_commands_attack_target"));
                if (f.Length == 3760)
                {
                    File.Delete(Sub_Code.File_Get_FileName_No_Extension(Voice_Dir + "/quick_commands_attack_target"));
                }
            }
            if (Sub_Code.File_Exists(Voice_Dir + "/capture_end"))
            {
                FileInfo f = new FileInfo(Sub_Code.File_Get_FileName_No_Extension(Voice_Dir + "/capture_end"));
                if (f.Length == 3760)
                {
                    File.Delete(Sub_Code.File_Get_FileName_No_Extension(Voice_Dir + "/capture_end"));
                }
            }
            Directory.CreateDirectory(To_Dir);
            Random r = new Random();
            List<string> Normal_Copy = new List<string>();
            string[] Voice_Files = Directory.GetFiles(Voice_Dir, "*", SearchOption.TopDirectoryOnly);
            foreach (string Voice_Now in Voice_Files)
            {
                string Voice_Type = Voice_Mod_Create.Get_Voice_Type_V1(Voice_Now);
                string NameOnly = Path.GetFileNameWithoutExtension(Voice_Now);
                if (Voice_Type == "howitzer_load" && Voice_Set.SE_Enable_Disable[9])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, Sub_Code.File_Get_FileName_No_Extension(SE_Dir + "/Reload_0" + r.Next(1, 7)), To_Dir);
                    continue;
                }
                if (Voice_Type == "lamp" && Voice_Set.SE_Enable_Disable[10])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, Sub_Code.File_Get_FileName_No_Extension(SE_Dir + "/Sixth_0" + r.Next(1, 4)), To_Dir);
                    continue;
                }
                if (Voice_Type == "vehicle_destroyed" && Voice_Set.SE_Enable_Disable[3])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, Sub_Code.File_Get_FileName_No_Extension(SE_Dir + "/Destroy_01"), To_Dir);
                    continue;
                }
                if (Voice_Type == "armor_pierced_by_player" && Voice_Set.SE_Enable_Disable[4])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, Sub_Code.File_Get_FileName_No_Extension(SE_Dir + "/Enable_0" + r.Next(1, 4)), To_Dir);
                    continue;
                }
                if (Voice_Type == "armor_pierced_crit_by_player" && Voice_Set.SE_Enable_Disable[5])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, Sub_Code.File_Get_FileName_No_Extension(SE_Dir + "/Enable_Special_01"), To_Dir);
                    continue;
                }
                if (Voice_Type == "armor_not_pierced_by_player" && Voice_Set.SE_Enable_Disable[8])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, Sub_Code.File_Get_FileName_No_Extension(SE_Dir + "/Not_Enable_01"), To_Dir);
                    continue;
                }
                if (Voice_Type == "armor_ricochet_by_player" && Voice_Set.SE_Enable_Disable[8])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, Sub_Code.File_Get_FileName_No_Extension(SE_Dir + "/Not_Enable_01"), To_Dir);
                    continue;
                }
                if (Voice_Type == "radio_damaged" && Voice_Set.SE_Enable_Disable[6])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, Sub_Code.File_Get_FileName_No_Extension(SE_Dir + "/Musenki_01"), To_Dir);
                    continue;
                }
                if (Voice_Type == "ammo_bay_damaged" && Voice_Set.SE_Enable_Disable[2])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, Sub_Code.File_Get_FileName_No_Extension(SE_Dir + "/Danyaku_SE_01"), To_Dir);
                    continue;
                }
                if (Voice_Type == "fuel_tank_damaged" && Voice_Set.SE_Enable_Disable[7])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, Sub_Code.File_Get_FileName_No_Extension(SE_Dir + "/Nenryou_SE_01"), To_Dir);
                    continue;
                }
                if (Voice_Type == "enemy_killed_by_player" && Voice_Set.SE_Enable_Disable[4])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, Sub_Code.File_Get_FileName_No_Extension(SE_Dir + "/Enable_0" + r.Next(1, 4)), To_Dir);
                    continue;
                }
                if (NameOnly == "enemy_sighted" && Voice_Set.SE_Enable_Disable[11])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, Sub_Code.File_Get_FileName_No_Extension(SE_Dir + "/Spot_01"), To_Dir);
                    continue;
                }
                if (NameOnly == "capture_end" && Voice_Set.SE_Enable_Disable[0])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, Sub_Code.File_Get_FileName_No_Extension(SE_Dir + "/Capture_End_0" + r.Next(1, 3)), To_Dir);
                    continue;
                }
                if (NameOnly == "auto_target_on" && Voice_Set.SE_Enable_Disable[13])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, Sub_Code.File_Get_FileName_No_Extension(SE_Dir + "/Lock_01"), To_Dir);
                    continue;
                }
                if (NameOnly == "auto_target_off" && Voice_Set.SE_Enable_Disable[14])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, Sub_Code.File_Get_FileName_No_Extension(SE_Dir + "/Unlock_01"), To_Dir);
                    continue;
                }
                if (NameOnly == "quick_commands_positive" && Voice_Set.SE_Enable_Disable[1])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, Sub_Code.File_Get_FileName_No_Extension(SE_Dir + "/Command_01"), To_Dir);
                    continue;
                }
                if (NameOnly == "quick_commands_negative" && Voice_Set.SE_Enable_Disable[1])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, Sub_Code.File_Get_FileName_No_Extension(SE_Dir + "/Command_01"), To_Dir);
                    continue;
                }
                if (NameOnly == "quick_commands_reloading" && Voice_Set.SE_Enable_Disable[1])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, Sub_Code.File_Get_FileName_No_Extension(SE_Dir + "/Command_01"), To_Dir);
                    continue;
                }
                if (NameOnly == "quick_commands_help_me" && Voice_Set.SE_Enable_Disable[1])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, Sub_Code.File_Get_FileName_No_Extension(SE_Dir + "/Command_01"), To_Dir);
                    continue;
                }
                if (NameOnly == "quick_commands_capture_base" && Voice_Set.SE_Enable_Disable[1])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, Sub_Code.File_Get_FileName_No_Extension(SE_Dir + "/Command_01"), To_Dir);
                    continue;
                }
                if (NameOnly == "quick_commands_defend_base" && Voice_Set.SE_Enable_Disable[1])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, Sub_Code.File_Get_FileName_No_Extension(SE_Dir + "/Command_01"), To_Dir);
                    continue;
                }
                if (NameOnly == "quick_commands_attack" && Voice_Set.SE_Enable_Disable[1])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, Sub_Code.File_Get_FileName_No_Extension(SE_Dir + "/Command_01"), To_Dir);
                    continue;
                }
                if (NameOnly == "quick_commands_attack_target" && Voice_Set.SE_Enable_Disable[1])
                {
                    Ingame_Voice_In_SE_By_FileName(Voice_Now, Sub_Code.File_Get_FileName_No_Extension(SE_Dir + "/Command_01"), To_Dir);
                    continue;
                }
                Normal_Copy.Add(Voice_Now);
            }
            await Sub_Code.Change_MP3_Encode(To_Dir);
            foreach (string Copy_File in Normal_Copy)
            {
                try
                {
                    File.Copy(Copy_File, To_Dir + "/" + Path.GetFileName(Copy_File), true);
                }
                catch (Exception e)
                {
                    Sub_Code.Error_Log_Write(e.Message);
                }
            }
            if (Sub_Code.File_Exists(To_Dir + "/howitzer_load_01"))
            {
                try
                {
                    string Full_Name_01 = Sub_Code.File_Get_FileName_No_Extension(To_Dir + "/howitzer_load_01");
                    string Full_Name_02 = Sub_Code.File_Get_FileName_No_Extension(To_Dir + "/howitzer_load_02");
                    string Full_Name_03 = Sub_Code.File_Get_FileName_No_Extension(To_Dir + "/howitzer_load_03");
                    string Full_Name_04 = Sub_Code.File_Get_FileName_No_Extension(To_Dir + "/howitzer_load_04");
                    string Full_Name_05 = Sub_Code.File_Get_FileName_No_Extension(To_Dir + "/howitzer_load_05");
                    string Ex_01 = Path.GetExtension(Full_Name_01);
                    string Ex_02 = Path.GetExtension(Full_Name_02);
                    string Ex_03 = Path.GetExtension(Full_Name_03);
                    string Ex_04 = Path.GetExtension(Full_Name_04);
                    string Ex_05 = Path.GetExtension(Full_Name_05);
                    Sound_Speed_Change(Full_Name_01, Path.GetDirectoryName(Full_Name_01) + "/Temp_howitzer_load_01" + Ex_01, 1.25);
                    Sound_Speed_Change(Full_Name_02, Path.GetDirectoryName(Full_Name_02) + "/Temp_howitzer_load_02" + Ex_02, 1.25);
                    Sound_Speed_Change(Full_Name_03, Path.GetDirectoryName(Full_Name_03) + "/Temp_howitzer_load_03" + Ex_03, 1.25);
                    Sound_Speed_Change(Full_Name_04, Path.GetDirectoryName(Full_Name_04) + "/Temp_howitzer_load_04" + Ex_04, 1.25);
                    Sound_Speed_Change(Full_Name_05, Path.GetDirectoryName(Full_Name_05) + "/Temp_howitzer_load_05" + Ex_05, 1.25);
                    Sub_Code.File_Move(Path.GetDirectoryName(Full_Name_01) + "/Temp_howitzer_load_01" + Ex_01, Full_Name_01, true);
                    Sub_Code.File_Move(Path.GetDirectoryName(Full_Name_02) + "/Temp_howitzer_load_02" + Ex_02, Full_Name_02, true);
                    Sub_Code.File_Move(Path.GetDirectoryName(Full_Name_03) + "/Temp_howitzer_load_03" + Ex_03, Full_Name_03, true);
                    Sub_Code.File_Move(Path.GetDirectoryName(Full_Name_04) + "/Temp_howitzer_load_04" + Ex_04, Full_Name_04, true);
                    Sub_Code.File_Move(Path.GetDirectoryName(Full_Name_05) + "/Temp_howitzer_load_05" + Ex_05, Full_Name_05, true);
                }
                catch (Exception e)
                {
                    MessageBox.Show("エラー:" + e.Message);
                    Sub_Code.Error_Log_Write(e.Message);
                }
            }
        }
        //Fmod_Android_Create.exeを使用してfsbを作成する
        static void Android_FSB_Create(string File_Dir, string Save_File)
        {
            int Number = 0;
            start:
            Number++;
            if (Number >= 3)
            {
                return;
            }
            if (!Directory.Exists(File_Dir))
            {
                return;
            }
            File.Delete(Save_File);
            string Name_Only = Path.GetFileNameWithoutExtension(Save_File);
            string SPath = Voice_Set.Special_Path + "\\Fmod_Android_Create";
            string Cache_Path = Directory.GetCurrentDirectory() + "\\FMod_Cache";
            StreamWriter stw = File.CreateText(SPath + "\\FSB_Create.lst");
            string[] Files_Dir = Directory.GetFiles(File_Dir, "*", SearchOption.TopDirectoryOnly);
            foreach (string File_Now in Files_Dir)
            {
                stw.WriteLine(File_Now);
            }
            stw.Close();
            Directory.CreateDirectory(Cache_Path);
            StreamWriter Bat = File.CreateText(SPath + "\\" + Name_Only + ".bat");
            Bat.WriteLine("chcp 65001");
            Bat.Write("\"" + SPath + "\\Fmod_Android_Create.exe\" -cache_dir \"Cache\" -format adpcm -o \"" + SPath + "\\Temp.fsb\" \"" + SPath + "\\FSB_Create.lst\"");
            Bat.Close();
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = SPath + "\\" + Name_Only + ".bat",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process p = Process.Start(processStartInfo);
            p.WaitForExit();
            Sub_Code.File_Move(SPath + "\\Temp.fsb", Save_File, true);
            File.Delete(SPath + "\\FSB_Create.lst");
            File.Delete(SPath + "\\" + Name_Only + ".bat");
            try
            {
                if (Directory.Exists(Cache_Path))
                {
                    Directory.Delete(Cache_Path, true);
                }
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
            if (!File.Exists(Save_File))
            {
                goto start;
            }
        }
    }
}