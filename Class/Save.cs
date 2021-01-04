using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using WoTB_Voice_Mod_Creater;

namespace WoTB_Voice_Mod_Creater
{
    public partial class MainCode : Window
    {
        private async void Save_B_Click(object sender, RoutedEventArgs e)
        {
            Save_Window.Window_Show();
            Save_Window.Visibility = Visibility.Visible;
            while (Save_Window.Opacity < 1)
            {
                Save_Window.Opacity += 0.025;
                await Task.Delay(1000 / 60);
            }
            while (Save_Window.Visibility == Visibility.Visible)
            {
                await Task.Delay(100);
            }
            if (Sub_Code.CreatingProject)
            {
                Sub_Code.CreatingProject = false;
                Voice_Mod_Create.Project_Create(Message_T, Voice_Set.SRTTbacon_Server_Name, Special_Path + "/Server/" + Voice_Set.SRTTbacon_Server_Name + "/Voices", Special_Path + "/Server/" + Voice_Set.SRTTbacon_Server_Name + "/Voices/SE");
            }
        }
    }
}
public class Voice_Mod_Create
{
    readonly static string Path = Directory.GetCurrentDirectory();
    readonly static string Special_Path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/WoTB_Voice_Mod_Creater";
    public static void Project_Create(System.Windows.Controls.TextBlock Message_T, string Project_Name, string Voice_Dir, string SE_Dir)
    {
        string Voice_Now = "";
        string SE_Now = "";
        string Server_Name_Rename = Project_Name.Replace(" ", "_");
        try
        {
            Message_T.Opacity = 1;
            Message_T.Text = "プロジェクトを作成中...";
            if (!Directory.Exists(Path + "/Projects/" + Project_Name))
            {
                Directory.CreateDirectory(Path + "/Projects/" + Project_Name);
            }
            if (!Directory.Exists(Path + "/Backup"))
            {
                Directory.CreateDirectory(Path + "/Backup");
            }
            DateTime dt = DateTime.Now;
            if (File.Exists(Voice_Set.WoTB_Path + "/Data/sounds.yaml.dvpl"))
            {
                StreamWriter DVPL_Unpack = File.CreateText(Special_Path + "/DVPL/UnPack.bat");
                DVPL_Unpack.Write("\"" + Special_Path + "/DVPL/Python/python.exe\" \"" + Special_Path + "/DVPL/UnPack.py\" \"" + Voice_Set.WoTB_Path + "/Data/sounds.yaml.dvpl\" \"" + Special_Path + "/Temp_Sounds.yaml\"");
                DVPL_Unpack.Close();
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = Special_Path + "/DVPL/UnPack.bat",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process p = Process.Start(processStartInfo);
                p.WaitForExit();
                File.Delete(Special_Path + "/DVPL/UnPack.bat");
            }
            else
            {
                File.Copy(Voice_Set.WoTB_Path + "/Data/sounds.yaml", Special_Path + "/Temp_Sounds.yaml", true);
            }
            Change_Sounds_yaml(Voice_Dir, Server_Name_Rename);
            StreamWriter stw = File.CreateText(Path + "/Projects/" + Project_Name + "/" + Server_Name_Rename + ".fdp");
            stw.Write("<project>\n" +
                      "<name>" + Server_Name_Rename + "</name>\n" +
                      "<guid>{dc0f56db-d9d8-4cb4-af9f-722e1e67618e}</guid>\n" +
                      "<version>4</version>\n" +
                      "<eventgroup_nextid>0</eventgroup_nextid>\n" +
                      "<soundbank_nextid>3</soundbank_nextid>\n" +
                      "<sounddef_nextid>0</sounddef_nextid>\n" +
                      "<build_project>1</build_project>\n" +
                      "<build_headerfile>0</build_headerfile>\n" +
                      "<build_banklists>0</build_banklists>\n" +
                      "<build_programmerreport>0</build_programmerreport>\n" +
                      "<build_applytemplate>0</build_applytemplate>\n" +
                      "<build_continue_on_error>0</build_continue_on_error>\n" +
                      "<currentbank>" + Server_Name_Rename + "</currentbank>\n" +
                      "<currentlanguage>default</currentlanguage>\n" +
                      "<primarylanguage>default</primarylanguage>\n" +
                      "<language>default</language>\n" +
                      "<templatefilename></templatefilename>\n" +
                      "<templatefileopen>1</templatefileopen>\n" +
                      "<eventcategory>\n" +
                      "<name>music</name>\n" +
                      "<guid>{5be365b9-6499-43dd-9dac-e8f39f005217}</guid>\n" +
                      "<volume_db>0</volume_db>\n" +
                      "<pitch>0</pitch>\n" +
                      "<maxplaybacks>0</maxplaybacks>\n" +
                      "<maxplaybacks_behavior>Steal_oldest</maxplaybacks_behavior>\n" +
                      "<notes></notes>\n" +
                      "<open>0</open>\n" +
                      "</eventcategory>\n" +
                      "<eventcategory>\n" +
                      "<name>ingame_voice</name>\n" +
                      "<guid>{8a085b14-0c02-441e-b4d4-d3d5a0cce8df}</guid>\n" +
                      "<volume_db>0</volume_db>\n" +
                      "<pitch>0</pitch>\n" +
                      "<maxplaybacks>0</maxplaybacks>\n" +
                      "<maxplaybacks_behavior>Steal_oldest</maxplaybacks_behavior>\n" +
                      "<notes></notes>\n" +
                      "<open>1</open>\n" +
                      "</eventcategory>\n" +
                      "<sounddeffolder>\n" +
                      "<name>master</name>\n" +
                      "<guid>{f3af6169-744a-4da0-baa6-fa0fdae22fab}</guid>\n" +
                      "<open>0</open>\n" +
                      "<sounddeffolder>\n" +
                      "<name>__simpleevent_sounddef__</name>\n" +
                      "<guid>{a50d8208-d703-42a8-908d-6cb63f8cc731}</guid>\n" +
                      "<open>0</open>\n" +
                      "</sounddeffolder>\n");
            List<string> Set_Voice_List = new List<string>();
            string[] Temp = Directory.GetFiles(Voice_Dir, "*", SearchOption.TopDirectoryOnly);
            for (int Number = 0; Number <= Temp.Length - 1; Number++)
            {
                if (Voice_Set.Voice_Name_Hide(Temp[Number]))
                {
                    Set_Voice_List.Add(Temp[Number]);
                }
            }
            int Voice_Name_Number = 0;
            foreach (string Name_Now in Set_Voice_List)
            {
                string Voice_Type_Now = Get_Voice_Type(Name_Now);
                if (Voice_Type_Now != Voice_Now)
                {
                    Voice_Now = Voice_Type_Now;
                    if (Voice_Name_Number != 0)
                    {
                        stw.Write("</sounddef>\n");
                    }
                    else
                    {
                        Voice_Name_Number++;
                    }
                    stw.Write("<sounddef>\n" +
                              "<name>/" + Voice_Type_Now + "</name>\n" +
                              "<guid>{c0e47b61-0077-4604-8b3c-a8017bfaf532}</guid>\n" +
                              "<type>randomnorepeat</type>\n" +
                              "<playlistmode>0</playlistmode>\n" +
                              "<randomrepeatsounds>0</randomrepeatsounds>\n" +
                              "<randomrepeatsilences>0</randomrepeatsilences>\n" +
                              "<shuffleglobal>0</shuffleglobal>\n" +
                              "<sequentialrememberposition>0</sequentialrememberposition>\n" +
                              "<sequentialglobal>0</sequentialglobal>\n" +
                              "<spawntime_min>0</spawntime_min>\n" +
                              "<spawntime_max>0</spawntime_max>\n" +
                              "<spawn_max>1</spawn_max>\n" +
                              "<mode>0</mode>\n" +
                              "<pitch>0</pitch>\n" +
                              "<pitch_randmethod>1</pitch_randmethod>\n" +
                              "<pitch_random_min>0</pitch_random_min>\n" +
                              "<pitch_random_max>0</pitch_random_max>\n" +
                              "<pitch_randomization>0</pitch_randomization>\n" +
                              "<pitch_recalculate>0</pitch_recalculate>\n" +
                              "<volume_db>0</volume_db>\n" +
                              "<volume_randmethod>1</volume_randmethod>\n" +
                              "<volume_random_min>0</volume_random_min>\n" +
                              "<volume_random_max>0</volume_random_max>\n" +
                              "<volume_randomization>0</volume_randomization>\n" +
                              "<position_randomization_min>0</position_randomization_min>\n" +
                              "<position_randomization>0</position_randomization>\n" +
                              "<trigger_delay_min>0</trigger_delay_min>\n" +
                              "<trigger_delay_max>0</trigger_delay_max>\n" +
                              "<spawncount>0</spawncount>\n" +
                              "<notes></notes>\n" +
                              "<entrylistmode>1</entrylistmode>\n");
                }
                stw.Write("<waveform>\n" +
                          "<filename>" + Name_Now + "</filename>\n" +
                          "<soundbankname>" + Server_Name_Rename + "</soundbankname>\n" +
                          "<weight>100</weight>\n" +
                          "<percentagelocked>0</percentagelocked>\n" +
                          "</waveform>\n");
            }
            List<string> Set_SE_List = new List<string>();
            Set_SE_List.AddRange(Directory.GetFiles(SE_Dir, "*", SearchOption.TopDirectoryOnly));
            foreach (string Name_Now in Set_SE_List)
            {
                string SE_Type_Now = Get_Voice_Type(Name_Now);
                if (SE_Type_Now != SE_Now)
                {
                    int Delay = 0;
                    if (SE_Type_Now == "danyaku" && Voice_Set.SE_Enable_List[2])
                    {
                        Delay = 1000;
                    }
                    else if (SE_Type_Now == "nenryou" && Voice_Set.SE_Enable_List[7])
                    {
                        Delay = 1000;
                    }
                    SE_Now = SE_Type_Now;
                    stw.Write("</sounddef>\n" +
                              "<sounddef>\n" +
                              "<name>/" + SE_Type_Now + "</name>\n" +
                              "<guid>{c0e47b61-0077-4604-8b3c-a8017bfaf532}</guid>\n" +
                              "<type>randomnorepeat</type>\n" +
                              "<playlistmode>0</playlistmode>\n" +
                              "<randomrepeatsounds>0</randomrepeatsounds>\n" +
                              "<randomrepeatsilences>0</randomrepeatsilences>\n" +
                              "<shuffleglobal>0</shuffleglobal>\n" +
                              "<sequentialrememberposition>0</sequentialrememberposition>\n" +
                              "<sequentialglobal>0</sequentialglobal>\n" +
                              "<spawntime_min>0</spawntime_min>\n" +
                              "<spawntime_max>0</spawntime_max>\n" +
                              "<spawn_max>1</spawn_max>\n" +
                              "<mode>0</mode>\n" +
                              "<pitch>0</pitch>\n" +
                              "<pitch_randmethod>1</pitch_randmethod>\n" +
                              "<pitch_random_min>0</pitch_random_min>\n" +
                              "<pitch_random_max>0</pitch_random_max>\n" +
                              "<pitch_randomization>0</pitch_randomization>\n" +
                              "<pitch_recalculate>0</pitch_recalculate>\n" +
                              "<volume_db>0</volume_db>\n" +
                              "<volume_randmethod>1</volume_randmethod>\n" +
                              "<volume_random_min>0</volume_random_min>\n" +
                              "<volume_random_max>0</volume_random_max>\n" +
                              "<volume_randomization>0</volume_randomization>\n" +
                              "<position_randomization_min>0</position_randomization_min>\n" +
                              "<position_randomization>0</position_randomization>\n" +
                              "<trigger_delay_min>" + Delay + "</trigger_delay_min>\n" +
                              "<trigger_delay_max>" + Delay + "</trigger_delay_max>\n" +
                              "<spawncount>0</spawncount>\n" +
                              "<notes></notes>\n" +
                              "<entrylistmode>1</entrylistmode>\n");
                }
                stw.Write("<waveform>\n" +
                          "<filename>" + Name_Now + "</filename>\n" +
                          "<soundbankname>" + Server_Name_Rename + "</soundbankname>\n" +
                          "<weight>100</weight>\n" +
                          "<percentagelocked>0</percentagelocked>\n" +
                          "</waveform>\n");
            }
            stw.Write("</sounddef>\n" +
                      "</sounddeffolder>\n" +
                      "<eventgroup>\n" +
                      "<name>" + Server_Name_Rename + "</name>\n" +
                      "<guid>{4681190d-f571-47c3-bbd6-38af16fa8b70}</guid>\n" +
                      "<eventgroup_nextid>0</eventgroup_nextid>\n" +
                      "<event_nextid>0</event_nextid>\n" +
                      "<open>1</open>\n" +
                      "<notes></notes>\n");
            string[] Type_List = Get_Voice_Type_Only(Set_Voice_List.ToArray());
            foreach (string Type_Now in Type_List)
            {
                stw.Write("<event>\n" +
                          "<name>" + Type_Now + "</name>\n" +
                          "<guid>{39a991f5-ea43-40dc-ad7c-17638e4ed2d9}</guid>\n" +
                          "<parameter_nextid>0</parameter_nextid>\n" +
                          "<layer_nextid>2</layer_nextid>\n" +
                          "<layer>\n" +
                          "<name>layer00</name>\n" +
                          "<height>100</height>\n" +
                          "<envelope_nextid>0</envelope_nextid>\n" +
                          "<mute>0</mute>\n" +
                          "<solo>0</solo>\n" +
                          "<soundlock>0</soundlock>\n" +
                          "<envlock>0</envlock>\n" +
                          "<priority>-1</priority>\n" +
                          "<sound>\n" +
                          "<name>/" + Type_Now + "</name>\n" +
                          "<x>0</x>\n" +
                          "<width>1</width>\n" +
                          "<startmode>0</startmode>\n" +
                          "<loopmode>1</loopmode>\n" +
                          "<loopcount2>-1</loopcount2>\n" +
                          "<autopitchenabled>0</autopitchenabled>\n" +
                          "<autopitchparameter>0</autopitchparameter>\n" +
                          "<autopitchreference>0</autopitchreference>\n" +
                          "<autopitchatzero>0</autopitchatzero>\n" +
                          "<finetune>0</finetune>\n" +
                          "<volume>1</volume>\n" +
                          "<fadeintype>2</fadeintype>\n" +
                          "<fadeouttype>2</fadeouttype>\n" +
                          "</sound>\n" +
                          "<_PC_enable>1</_PC_enable>\n" +
                          "<_XBOX360_enable>1</_XBOX360_enable>\n" +
                          "<_XBOXONE_enable>1</_XBOXONE_enable>\n" +
                          "<_PS3_enable>1</_PS3_enable>\n" +
                          "<_PS4_enable>1</_PS4_enable>\n" +
                          "<_WII_enable>1</_WII_enable>\n" +
                          "<_WiiU_enable>1</_WiiU_enable>\n" +
                          "<_3DS_enable>1</_3DS_enable>\n" +
                          "<_NGP_enable>1</_NGP_enable>\n" +
                          "<_ANDROID_enable>1</_ANDROID_enable>\n" +
                          "<_IOS_enable>1</_IOS_enable>\n" +
                          "<_BB10_enable>1</_BB10_enable>\n" +
                          "</layer>\n");
                if (Type_Now == "danyaku" && Voice_Set.SE_Enable_List[2])
                {
                    stw.Write(Set_Layer_By_Name("Danyaku_SE", false));
                }
                else if (Type_Now == "gekiha" && Voice_Set.SE_Enable_List[4])
                {
                    stw.Write(Set_Layer_By_Name("Enable", false));
                }
                else if (Type_Now == "hakken" && Voice_Set.SE_Enable_List[11])
                {
                    stw.Write(Set_Layer_By_Name("Spot", false));
                }
                else if (Type_Now == "hikantuu" && Voice_Set.SE_Enable_List[8])
                {
                    stw.Write(Set_Layer_By_Name("Not_Enable", false));
                }
                else if (Type_Now == "kantuu" && Voice_Set.SE_Enable_List[4])
                {
                    stw.Write(Set_Layer_By_Name("Enable", false));
                }
                else if (Type_Now == "tokusyu" && Voice_Set.SE_Enable_List[5])
                {
                    stw.Write(Set_Layer_By_Name("Enable_Special", false));
                }
                else if (Type_Now == "tyoudan" && Voice_Set.SE_Enable_List[8])
                {
                    stw.Write(Set_Layer_By_Name("Not_Enable", false));
                }
                else if (Type_Now == "lock" && Voice_Set.SE_Enable_List[13])
                {
                    stw.Write(Set_Layer_By_Name("Lock", false));
                }
                else if (Type_Now == "musen" && Voice_Set.SE_Enable_List[6])
                {
                    stw.Write(Set_Layer_By_Name("Musenki", false));
                }
                else if (Type_Now == "nenryou" && Voice_Set.SE_Enable_List[7])
                {
                    stw.Write(Set_Layer_By_Name("Nenryou_SE", false));
                }
                else if (Type_Now == "battle" && Voice_Set.SE_Enable_List[12])
                {
                    stw.Write(Set_Layer_By_Name("Timer", true));
                }
                else if (Type_Now == "taiha" && Voice_Set.SE_Enable_List[3])
                {
                    stw.Write(Set_Layer_By_Name("Destroy", false));
                }
                else if (Type_Now == "lamp" && Voice_Set.SE_Enable_List[10])
                {
                    stw.Write(Set_Layer_By_Name("Sixth", false));
                }
                else if (Type_Now == "ryoukai" || Type_Now == "kyohi" || Type_Now == "help" || Type_Now == "attack" || Type_Now == "attack_now" || Type_Now == "capture" || Type_Now == "defence" || Type_Now == "keep" || Type_Now == "map")
                {
                    if (Voice_Set.SE_Enable_List[1])
                    {
                        stw.Write(Set_Layer_By_Name("Command", false));
                    }
                }
                else if (Type_Now == "unlock" && Voice_Set.SE_Enable_List[14])
                {
                    stw.Write(Set_Layer_By_Name("Unlock", false));
                }
                else if (Type_Now == "reload" && Voice_Set.SE_Enable_List[9])
                {
                    stw.Write(Set_Layer_By_Name("Reload", false));
                }
                else if (Type_Now == "battle_end" && Voice_Set.SE_Enable_List[0])
                {
                    stw.Write(Set_Layer_By_Name("Capture_End", false));
                }
                stw.Write("<car_rpm>0</car_rpm>\n" +
                          "<car_rpmsmooth>0.075</car_rpmsmooth>\n" +
                          "<car_loadsmooth>0.05</car_loadsmooth>\n" +
                          "<car_loadscale>6</car_loadscale>\n" +
                          "<volume_db>0</volume_db>\n" +
                          "<pitch>0</pitch>\n" +
                          "<pitch_units>Octaves</pitch_units>\n" +
                          "<pitch_randomization>0</pitch_randomization>\n" +
                          "<pitch_randomization_units>Octaves</pitch_randomization_units>\n" +
                          "<volume_randomization>0</volume_randomization>\n" +
                          "<priority>128</priority>\n" +
                          "<maxplaybacks>10</maxplaybacks>\n" +
                          "<maxplaybacks_behavior>Just_fail_if_quietest</maxplaybacks_behavior>\n" +
                          "<stealpriority>10000</stealpriority>\n" +
                          "<mode>x_2d</mode>\n" +
                          "<ignoregeometry>No</ignoregeometry>\n" +
                          "<rolloff>Logarithmic</rolloff>\n" +
                          "<mindistance>1</mindistance>\n" +
                          "<maxdistance>10000</maxdistance>\n" +
                          "<auto_distance_filtering>Off</auto_distance_filtering>\n" +
                          "<distance_filter_centre_freq>1500</distance_filter_centre_freq>\n" +
                          "<headrelative>World_relative</headrelative>\n" +
                          "<oneshot>Yes</oneshot>\n" +
                          "<istemplate>No</istemplate>\n" +
                          "<usetemplate></usetemplate>\n" +
                          "<notes></notes>\n" +
                          "<category>ingame_voice</category>\n" +
                          "<position_randomization_min>0</position_randomization_min>\n" +
                          "<position_randomization>0</position_randomization>\n" +
                          "<speaker_l>1</speaker_l>\n" +
                          "<speaker_c>0</speaker_c>\n" +
                          "<speaker_r>1</speaker_r>\n" +
                          "<speaker_ls>0</speaker_ls>\n" +
                          "<speaker_rs>0</speaker_rs>\n" +
                          "<speaker_lb>0</speaker_lb>\n" +
                          "<speaker_rb>0</speaker_rb>\n" +
                          "<speaker_lfe>0</speaker_lfe>\n" +
                          "<speaker_config>0</speaker_config>\n" +
                          "<speaker_pan_r>1</speaker_pan_r>\n" +
                          "<speaker_pan_theta>0</speaker_pan_theta>\n" +
                          "<cone_inside_angle>360</cone_inside_angle>\n" +
                          "<cone_outside_angle>360</cone_outside_angle>\n" +
                          "<cone_outside_volumedb>0</cone_outside_volumedb>\n" +
                          "<doppler_scale>1</doppler_scale>\n" +
                          "<reverbdrylevel_db>0</reverbdrylevel_db>\n" +
                          "<reverblevel_db>0</reverblevel_db>\n" +
                          "<speaker_spread>0</speaker_spread>\n" +
                          "<panlevel3d>1</panlevel3d>\n" +
                          "<fadein_time>0</fadein_time>\n" +
                          "<fadeout_time>1000</fadeout_time>\n" +
                          "<spawn_intensity>1</spawn_intensity>\n" +
                          "<spawn_intensity_randomization>0</spawn_intensity_randomization>\n" +
                          "<TEMPLATE_PROP_LAYERS>1</TEMPLATE_PROP_LAYERS>\n" +
                          "<TEMPLATE_PROP_KEEP_EFFECTS_PARAMS>1</TEMPLATE_PROP_KEEP_EFFECTS_PARAMS>\n" +
                          "<TEMPLATE_PROP_VOLUME>0</TEMPLATE_PROP_VOLUME>\n" +
                          "<TEMPLATE_PROP_PITCH>1</TEMPLATE_PROP_PITCH>\n" +
                          "<TEMPLATE_PROP_PITCH_RANDOMIZATION>1</TEMPLATE_PROP_PITCH_RANDOMIZATION>\n" +
                          "<TEMPLATE_PROP_VOLUME_RANDOMIZATION>1</TEMPLATE_PROP_VOLUME_RANDOMIZATION>\n" +
                          "<TEMPLATE_PROP_PRIORITY>1</TEMPLATE_PROP_PRIORITY>\n" +
                          "<TEMPLATE_PROP_MAX_PLAYBACKS>1</TEMPLATE_PROP_MAX_PLAYBACKS>\n" +
                          "<TEMPLATE_PROP_MAX_PLAYBACKS_BEHAVIOR>1</TEMPLATE_PROP_MAX_PLAYBACKS_BEHAVIOR>\n" +
                          "<TEMPLATE_PROP_STEAL_PRIORITY>1</TEMPLATE_PROP_STEAL_PRIORITY>\n" +
                          "<TEMPLATE_PROP_MODE>1</TEMPLATE_PROP_MODE>\n" +
                          "<TEMPLATE_PROP_IGNORE_GEOMETRY>1</TEMPLATE_PROP_IGNORE_GEOMETRY>\n" +
                          "<TEMPLATE_PROP_X_3D_ROLLOFF>1</TEMPLATE_PROP_X_3D_ROLLOFF>\n" +
                          "<TEMPLATE_PROP_X_3D_MIN_DISTANCE>1</TEMPLATE_PROP_X_3D_MIN_DISTANCE>\n" +
                          "<TEMPLATE_PROP_X_3D_MAX_DISTANCE>1</TEMPLATE_PROP_X_3D_MAX_DISTANCE>\n" +
                          "<TEMPLATE_PROP_X_3D_POSITION>1</TEMPLATE_PROP_X_3D_POSITION>\n" +
                          "<TEMPLATE_PROP_X_3D_MIN_POSITION_RANDOMIZATION>1</TEMPLATE_PROP_X_3D_MIN_POSITION_RANDOMIZATION>\n" +
                          "<TEMPLATE_PROP_X_3D_POSITION_RANDOMIZATION>1</TEMPLATE_PROP_X_3D_POSITION_RANDOMIZATION>\n" +
                          "<TEMPLATE_PROP_X_3D_CONE_INSIDE_ANGLE>1</TEMPLATE_PROP_X_3D_CONE_INSIDE_ANGLE>\n" +
                          "<TEMPLATE_PROP_X_3D_CONE_OUTSIDE_ANGLE>1</TEMPLATE_PROP_X_3D_CONE_OUTSIDE_ANGLE>\n" +
                          "<TEMPLATE_PROP_X_3D_CONE_OUTSIDE_VOLUME>1</TEMPLATE_PROP_X_3D_CONE_OUTSIDE_VOLUME>\n" +
                          "<TEMPLATE_PROP_X_3D_DOPPLER_FACTOR>1</TEMPLATE_PROP_X_3D_DOPPLER_FACTOR>\n" +
                          "<TEMPLATE_PROP_REVERB_WET_LEVEL>1</TEMPLATE_PROP_REVERB_WET_LEVEL>\n" +
                          "<TEMPLATE_PROP_REVERB_DRY_LEVEL>1</TEMPLATE_PROP_REVERB_DRY_LEVEL>\n" +
                          "<TEMPLATE_PROP_X_3D_SPEAKER_SPREAD>1</TEMPLATE_PROP_X_3D_SPEAKER_SPREAD>\n" +
                          "<TEMPLATE_PROP_X_3D_PAN_LEVEL>1</TEMPLATE_PROP_X_3D_PAN_LEVEL>\n" +
                          "<TEMPLATE_PROP_X_2D_SPEAKER_L>1</TEMPLATE_PROP_X_2D_SPEAKER_L>\n" +
                          "<TEMPLATE_PROP_X_2D_SPEAKER_C>1</TEMPLATE_PROP_X_2D_SPEAKER_C>\n" +
                          "<TEMPLATE_PROP_X_2D_SPEAKER_R>1</TEMPLATE_PROP_X_2D_SPEAKER_R>\n" +
                          "<TEMPLATE_PROP_X_2D_SPEAKER_LS>1</TEMPLATE_PROP_X_2D_SPEAKER_LS>\n" +
                          "<TEMPLATE_PROP_X_2D_SPEAKER_RS>1</TEMPLATE_PROP_X_2D_SPEAKER_RS>\n" +
                          "<TEMPLATE_PROP_X_2D_SPEAKER_LR>1</TEMPLATE_PROP_X_2D_SPEAKER_LR>\n" +
                          "<TEMPLATE_PROP_X_2D_SPEAKER_RR>1</TEMPLATE_PROP_X_2D_SPEAKER_RR>\n" +
                          "<TEMPLATE_PROP_X_SPEAKER_LFE>1</TEMPLATE_PROP_X_SPEAKER_LFE>\n" +
                          "<TEMPLATE_PROP_ONESHOT>1</TEMPLATE_PROP_ONESHOT>\n" +
                          "<TEMPLATE_PROP_FADEIN_TIME>1</TEMPLATE_PROP_FADEIN_TIME>\n" +
                          "<TEMPLATE_PROP_FADEOUT_TIME>1</TEMPLATE_PROP_FADEOUT_TIME>\n" +
                          "<TEMPLATE_PROP_NOTES>1</TEMPLATE_PROP_NOTES>\n" +
                          "<TEMPLATE_PROP_USER_PROPERTIES>1</TEMPLATE_PROP_USER_PROPERTIES>\n" +
                          "<TEMPLATE_PROP_CATEGORY>0</TEMPLATE_PROP_CATEGORY>\n" +
                          "<_PC_enabled>1</_PC_enabled>\n" +
                          "<_XBOX360_enabled>1</_XBOX360_enabled>\n" +
                          "<_XBOXONE_enabled>1</_XBOXONE_enabled>\n" +
                          "<_PSP_enabled>1</_PSP_enabled>\n" +
                          "<_PS3_enabled>1</_PS3_enabled>\n" +
                          "<_PS4_enabled>1</_PS4_enabled>\n" +
                          "<_WII_enabled>1</_WII_enabled>\n" +
                          "<_WiiU_enabled>1</_WiiU_enabled>\n" +
                          "<_3DS_enabled>1</_3DS_enabled>\n" +
                          "<_NGP_enabled>1</_NGP_enabled>\n" +
                          "<_ANDROID_enabled>1</_ANDROID_enabled>\n" +
                          "<_IOS_enabled>1</_IOS_enabled>\n" +
                          "<_BB10_enabled>1</_BB10_enabled>\n" +
                          "</event>\n");
            }
            if (!Sub_Code.File_Exist_Voice_Type(Voice_Dir, "danyaku") && Voice_Set.SE_Enable_List[2])
            {
                stw.Write(Not_Voice_Exist_SE_Add("danyaku", "Danyaku_SE", false));
            }
            if (!Sub_Code.File_Exist_Voice_Type(Voice_Dir, "gekiha") && Voice_Set.SE_Enable_List[4])
            {
                stw.Write(Not_Voice_Exist_SE_Add("gekiha", "Enable", false));
            }
            if (!Sub_Code.File_Exist_Voice_Type(Voice_Dir, "hakken") && Voice_Set.SE_Enable_List[11])
            {
                stw.Write(Not_Voice_Exist_SE_Add("hakken", "Spot", false));
            }
            if (!Sub_Code.File_Exist_Voice_Type(Voice_Dir, "hikantuu") && Voice_Set.SE_Enable_List[8])
            {
                stw.Write(Not_Voice_Exist_SE_Add("hikantuu", "Not_Enable", false));
            }
            if (!Sub_Code.File_Exist_Voice_Type(Voice_Dir, "kantuu") && Voice_Set.SE_Enable_List[4])
            {
                stw.Write(Not_Voice_Exist_SE_Add("kantuu", "Enable", false));
            }
            if (!Sub_Code.File_Exist_Voice_Type(Voice_Dir, "tokusyu") && Voice_Set.SE_Enable_List[5])
            {
                stw.Write(Not_Voice_Exist_SE_Add("tokusyu", "Enable_Special", false));
            }
            if (!Sub_Code.File_Exist_Voice_Type(Voice_Dir, "tyoudan") && Voice_Set.SE_Enable_List[8])
            {
                stw.Write(Not_Voice_Exist_SE_Add("tyoudan", "Not_Enable", false));
            }
            if (!Sub_Code.File_Exist_Voice_Type(Voice_Dir, "lock") && Voice_Set.SE_Enable_List[13])
            {
                stw.Write(Not_Voice_Exist_SE_Add("lock", "Lock", false));
            }
            if (!Sub_Code.File_Exist_Voice_Type(Voice_Dir, "musen") && Voice_Set.SE_Enable_List[6])
            {
                stw.Write(Not_Voice_Exist_SE_Add("musen", "Musenki", false));
            }
            if (!Sub_Code.File_Exist_Voice_Type(Voice_Dir, "nenryou") && Voice_Set.SE_Enable_List[7])
            {
                stw.Write(Not_Voice_Exist_SE_Add("nenryou", "Nenryou_SE", false));
            }
            if (!Sub_Code.File_Exist_Voice_Type(Voice_Dir, "battle") && Voice_Set.SE_Enable_List[12])
            {
                stw.Write(Not_Voice_Exist_SE_Add("battle", "Timer", true));
            }
            if (!Sub_Code.File_Exist_Voice_Type(Voice_Dir, "taiha") && Voice_Set.SE_Enable_List[3])
            {
                stw.Write(Not_Voice_Exist_SE_Add("taiha", "Destroy", false));
            }
            if (!Sub_Code.File_Exist_Voice_Type(Voice_Dir, "lamp") && Voice_Set.SE_Enable_List[10])
            {
                stw.Write(Not_Voice_Exist_SE_Add("lamp", "Sixth", false));
            }
            if (!Sub_Code.File_Exist_Voice_Type(Voice_Dir, "ryoukai") && Voice_Set.SE_Enable_List[1])
            {
                stw.Write(Not_Voice_Exist_SE_Add("ryoukai", "Command", false));
            }
            if (!Sub_Code.File_Exist_Voice_Type(Voice_Dir, "kyohi") && Voice_Set.SE_Enable_List[1])
            {
                stw.Write(Not_Voice_Exist_SE_Add("kyohi", "Command", false));
            }
            if (!Sub_Code.File_Exist_Voice_Type(Voice_Dir, "help") && Voice_Set.SE_Enable_List[1])
            {
                stw.Write(Not_Voice_Exist_SE_Add("help", "Command", false));
            }
            if (!Sub_Code.File_Exist_Voice_Type(Voice_Dir, "attack") && Voice_Set.SE_Enable_List[1])
            {
                stw.Write(Not_Voice_Exist_SE_Add("attack", "Command", false));
            }
            if (!Sub_Code.File_Exist_Voice_Type(Voice_Dir, "attack_now") && Voice_Set.SE_Enable_List[1])
            {
                stw.Write(Not_Voice_Exist_SE_Add("attack_now", "Command", false));
            }
            if (!Sub_Code.File_Exist_Voice_Type(Voice_Dir, "defence") && Voice_Set.SE_Enable_List[1])
            {
                stw.Write(Not_Voice_Exist_SE_Add("defence", "Command", false));
            }
            if (!Sub_Code.File_Exist_Voice_Type(Voice_Dir, "keep") && Voice_Set.SE_Enable_List[1])
            {
                stw.Write(Not_Voice_Exist_SE_Add("keep", "Command", false));
            }
            if (!Sub_Code.File_Exist_Voice_Type(Voice_Dir, "map") && Voice_Set.SE_Enable_List[1])
            {
                stw.Write(Not_Voice_Exist_SE_Add("map", "Command", false));
            }
            if (!Sub_Code.File_Exist_Voice_Type(Voice_Dir, "unlock") && Voice_Set.SE_Enable_List[14])
            {
                stw.Write(Not_Voice_Exist_SE_Add("unlock", "Unlock", false));
            }
            if (!Sub_Code.File_Exist_Voice_Type(Voice_Dir, "reload") && Voice_Set.SE_Enable_List[9])
            {
                stw.Write(Not_Voice_Exist_SE_Add("reload", "Reload", false));
            }
            if (!Sub_Code.File_Exist_Voice_Type(Voice_Dir, "battle_end") && Voice_Set.SE_Enable_List[0])
            {
                stw.Write(Not_Voice_Exist_SE_Add("battle_end", "Capture_End", false));
            }
            stw.Write("</eventgroup>\n" +
                      "<default_soundbank_props>\n" +
                      "<name>default_soundbank_props</name>\n" +
                      "<guid>{729237be-80e6-419f-8a3d-b9b35ceb2555}</guid>\n" +
                      "<load_into_rsx>0</load_into_rsx>\n" +
                      "<disable_seeking>0</disable_seeking>\n" +
                      "<enable_syncpoints>1</enable_syncpoints>\n" +
                      "<hasbuiltwithsyncpoints>0</hasbuiltwithsyncpoints>\n" +
                      "<_PC_banktype>DecompressedSample</_PC_banktype>\n" +
                      "<_XBOX360_banktype>DecompressedSample</_XBOX360_banktype>\n" +
                      "<_XBOXONE_banktype>DecompressedSample</_XBOXONE_banktype>\n" +
                      "<_PSP_banktype>DecompressedSample</_PSP_banktype>\n" +
                      "<_PS3_banktype>DecompressedSample</_PS3_banktype>\n" +
                      "<_PS4_banktype>DecompressedSample</_PS4_banktype>\n" +
                      "<_WII_banktype>DecompressedSample</_WII_banktype>\n" +
                      "<_WiiU_banktype>DecompressedSample</_WiiU_banktype>\n" +
                      "<_3DS_banktype>DecompressedSample</_3DS_banktype>\n" +
                      "<_NGP_banktype>DecompressedSample</_NGP_banktype>\n" +
                      "<_ANDROID_banktype>DecompressedSample</_ANDROID_banktype>\n" +
                      "<_IOS_banktype>DecompressedSample</_IOS_banktype>\n" +
                      "<_BB10_banktype>DecompressedSample</_BB10_banktype>\n" +
                      "<notes></notes>\n" +
                      "<_PC_format>PCM</_PC_format>\n" +
                      "<_PC_quality>50</_PC_quality>\n" +
                      "<_PC_forcesoftware>1</_PC_forcesoftware>\n" +
                      "<_PC_maxstreams>15</_PC_maxstreams>\n" +
                      "<_XBOX360_format>PCM</_XBOX360_format>\n" +
                      "<_XBOX360_quality>50</_XBOX360_quality>\n" +
                      "<_XBOX360_forcesoftware>1</_XBOX360_forcesoftware>\n" +
                      "<_XBOX360_maxstreams>10</_XBOX360_maxstreams>\n" +
                      "<_XBOXONE_format>PCM</_XBOXONE_format>\n" +
                      "<_XBOXONE_quality>50</_XBOXONE_quality>\n" +
                      "<_XBOXONE_forcesoftware>1</_XBOXONE_forcesoftware>\n" +
                      "<_XBOXONE_maxstreams>10</_XBOXONE_maxstreams>\n" +
                      "<_PSP_format>PCM</_PSP_format>\n" +
                      "<_PSP_quality>50</_PSP_quality>\n" +
                      "<_PSP_forcesoftware>0</_PSP_forcesoftware>\n" +
                      "<_PSP_maxstreams>10</_PSP_maxstreams>\n" +
                      "<_PS3_format>PCM</_PS3_format>\n" +
                      "<_PS3_quality>50</_PS3_quality>\n" +
                      "<_PS3_forcesoftware>1</_PS3_forcesoftware>\n" +
                      "<_PS3_maxstreams>10</_PS3_maxstreams>\n" +
                      "<_PS4_format>PCM</_PS4_format>\n" +
                      "<_PS4_quality>50</_PS4_quality>\n" +
                      "<_PS4_forcesoftware>1</_PS4_forcesoftware>\n" +
                      "<_PS4_maxstreams>10</_PS4_maxstreams>\n" +
                      "<_WII_format>PCM</_WII_format>\n" +
                      "<_WII_quality>50</_WII_quality>\n" +
                      "<_WII_forcesoftware>0</_WII_forcesoftware>\n" +
                      "<_WII_maxstreams>10</_WII_maxstreams>\n" +
                      "<_WiiU_format>PCM</_WiiU_format>\n" +
                      "<_WiiU_quality>50</_WiiU_quality>\n" +
                      "<_WiiU_forcesoftware>0</_WiiU_forcesoftware>\n" +
                      "<_WiiU_maxstreams>10</_WiiU_maxstreams>\n" +
                      "<_3DS_format>PCM</_3DS_format>\n" +
                      "<_3DS_quality>50</_3DS_quality>\n" +
                      "<_3DS_forcesoftware>0</_3DS_forcesoftware>\n" +
                      "<_3DS_maxstreams>10</_3DS_maxstreams>\n" +
                      "<_NGP_format>PCM</_NGP_format>\n" +
                      "<_NGP_quality>50</_NGP_quality>\n" +
                      "<_NGP_forcesoftware>0</_NGP_forcesoftware>\n" +
                      "<_NGP_maxstreams>10</_NGP_maxstreams>\n" +
                      "<_ANDROID_format>PCM</_ANDROID_format>\n" +
                      "<_ANDROID_quality>50</_ANDROID_quality>\n" +
                      "<_ANDROID_forcesoftware>1</_ANDROID_forcesoftware>\n" +
                      "<_ANDROID_maxstreams>10</_ANDROID_maxstreams>\n" +
                      "<_IOS_format>PCM</_IOS_format>\n" +
                      "<_IOS_quality>50</_IOS_quality>\n" +
                      "<_IOS_forcesoftware>1</_IOS_forcesoftware>\n" +
                      "<_IOS_maxstreams>10</_IOS_maxstreams>\n" +
                      "<_BB10_format>PCM</_BB10_format>\n" +
                      "<_BB10_quality>50</_BB10_quality>\n" +
                      "<_BB10_forcesoftware>0</_BB10_forcesoftware>\n" +
                      "<_BB10_maxstreams>10</_BB10_maxstreams>\n" +
                      "<_lang_default_filename_prefix></_lang_default_filename_prefix>\n" +
                      "<_lang_default_rebuild>0</_lang_default_rebuild>\n" +
                      "</default_soundbank_props>\n" +
                      "<soundbank>\n" +
                      "<name>" + Server_Name_Rename + "</name>\n" +
                      "<guid>{95b3fc25-b5aa-498a-a13f-e0956e660954}</guid>\n" +
                      "<load_into_rsx>0</load_into_rsx>\n" +
                      "<disable_seeking>0</disable_seeking>\n" +
                      "<enable_syncpoints>1</enable_syncpoints>\n" +
                      "<hasbuiltwithsyncpoints>0</hasbuiltwithsyncpoints>\n" +
                      "<_PC_banktype>DecompressedSample</_PC_banktype>\n" +
                      "<_XBOX360_banktype>DecompressedSample</_XBOX360_banktype>\n" +
                      "<_XBOXONE_banktype>DecompressedSample</_XBOXONE_banktype>\n" +
                      "<_PSP_banktype>DecompressedSample</_PSP_banktype>\n" +
                      "<_PS3_banktype>DecompressedSample</_PS3_banktype>\n" +
                      "<_PS4_banktype>DecompressedSample</_PS4_banktype>\n" +
                      "<_WII_banktype>DecompressedSample</_WII_banktype>\n" +
                      "<_WiiU_banktype>DecompressedSample</_WiiU_banktype>\n" +
                      "<_3DS_banktype>DecompressedSample</_3DS_banktype>\n" +
                      "<_NGP_banktype>DecompressedSample</_NGP_banktype>\n" +
                      "<_ANDROID_banktype>DecompressedSample</_ANDROID_banktype>\n" +
                      "<_IOS_banktype>DecompressedSample</_IOS_banktype>\n" +
                      "<_BB10_banktype>DecompressedSample</_BB10_banktype>\n" +
                      "<notes></notes>");
            foreach (string FileName in Set_SE_List)
            {
                stw.Write(Set_Voice_By_Name(FileName));
            }
            foreach (string FileName in Set_Voice_List)
            {
                stw.Write(Set_Voice_By_Name(FileName));
            }
            stw.Write("<_PC_format>ADPCM</_PC_format>\n" +
                      "<_PC_quality>50</_PC_quality>\n" +
                      "<_PC_forcesoftware>1</_PC_forcesoftware>\n" +
                      "<_PC_maxstreams>15</_PC_maxstreams>\n" +
                      "<_XBOX360_format>PCM</_XBOX360_format>\n" +
                      "<_XBOX360_quality>50</_XBOX360_quality>\n" +
                      "<_XBOX360_forcesoftware>1</_XBOX360_forcesoftware>\n" +
                      "<_XBOX360_maxstreams>10</_XBOX360_maxstreams>\n" +
                      "<_XBOXONE_format>PCM</_XBOXONE_format>\n" +
                      "<_XBOXONE_quality>50</_XBOXONE_quality>\n" +
                      "<_XBOXONE_forcesoftware>1</_XBOXONE_forcesoftware>\n" +
                      "<_XBOXONE_maxstreams>10</_XBOXONE_maxstreams>\n" +
                      "<_PSP_format>PCM</_PSP_format>\n" +
                      "<_PSP_quality>50</_PSP_quality>\n" +
                      "<_PSP_forcesoftware>0</_PSP_forcesoftware>\n" +
                      "<_PSP_maxstreams>10</_PSP_maxstreams>\n" +
                      "<_PS3_format>PCM</_PS3_format>\n" +
                      "<_PS3_quality>50</_PS3_quality>\n" +
                      "<_PS3_forcesoftware>1</_PS3_forcesoftware>\n" +
                      "<_PS3_maxstreams>10</_PS3_maxstreams>\n" +
                      "<_PS4_format>PCM</_PS4_format>\n" +
                      "<_PS4_quality>50</_PS4_quality>\n" +
                      "<_PS4_forcesoftware>1</_PS4_forcesoftware>\n" +
                      "<_PS4_maxstreams>10</_PS4_maxstreams>\n" +
                      "<_WII_format>PCM</_WII_format>\n" +
                      "<_WII_quality>50</_WII_quality>\n" +
                      "<_WII_forcesoftware>0</_WII_forcesoftware>\n" +
                      "<_WII_maxstreams>10</_WII_maxstreams>\n" +
                      "<_WiiU_format>PCM</_WiiU_format>\n" +
                      "<_WiiU_quality>50</_WiiU_quality>\n" +
                      "<_WiiU_forcesoftware>0</_WiiU_forcesoftware>\n" +
                      "<_WiiU_maxstreams>10</_WiiU_maxstreams>\n" +
                      "<_3DS_format>PCM</_3DS_format>\n" +
                      "<_3DS_quality>50</_3DS_quality>\n" +
                      "<_3DS_forcesoftware>0</_3DS_forcesoftware>\n" +
                      "<_3DS_maxstreams>10</_3DS_maxstreams>\n" +
                      "<_NGP_format>PCM</_NGP_format>\n" +
                      "<_NGP_quality>50</_NGP_quality>\n" +
                      "<_NGP_forcesoftware>0</_NGP_forcesoftware>\n" +
                      "<_NGP_maxstreams>10</_NGP_maxstreams>\n" +
                      "<_ANDROID_format>PCM</_ANDROID_format>\n" +
                      "<_ANDROID_quality>50</_ANDROID_quality>\n" +
                      "<_ANDROID_forcesoftware>1</_ANDROID_forcesoftware>\n" +
                      "<_ANDROID_maxstreams>10</_ANDROID_maxstreams>\n" +
                      "<_IOS_format>PCM</_IOS_format>\n" +
                      "<_IOS_quality>50</_IOS_quality>\n" +
                      "<_IOS_forcesoftware>1</_IOS_forcesoftware>\n" +
                      "<_IOS_maxstreams>10</_IOS_maxstreams>\n" +
                      "<_BB10_format>PCM</_BB10_format>\n" +
                      "<_BB10_quality>50</_BB10_quality>\n" +
                      "<_BB10_forcesoftware>0</_BB10_forcesoftware>\n" +
                      "<_BB10_maxstreams>10</_BB10_maxstreams>\n" +
                      "<_lang_default_filename_prefix></_lang_default_filename_prefix>\n" +
                      "<_lang_default_rebuild>1</_lang_default_rebuild>\n" +
                      "</soundbank>\n" +
                      "<notes></notes>\n" +
                      "<currentplatform>PC</currentplatform>\n" +
                      "<_PC_encryptionkey></_PC_encryptionkey>\n" +
                      "<_PC_builddirectory></_PC_builddirectory>\n" +
                      "<_PC_audiosourcedirectory></_PC_audiosourcedirectory>\n" +
                      "<_PC_prebuildcommands></_PC_prebuildcommands>\n" +
                      "<_PC_postbuildcommands></_PC_postbuildcommands>\n" +
                      "<_PC_buildinteractivemusic>Yes</_PC_buildinteractivemusic>\n" +
                      "<_XBOX360_encryptionkey></_XBOX360_encryptionkey>\n" +
                      "<_XBOX360_builddirectory></_XBOX360_builddirectory>\n" +
                      "<_XBOX360_audiosourcedirectory></_XBOX360_audiosourcedirectory>\n" +
                      "<_XBOX360_prebuildcommands></_XBOX360_prebuildcommands>\n" +
                      "<_XBOX360_postbuildcommands></_XBOX360_postbuildcommands>\n" +
                      "<_XBOX360_buildinteractivemusic>Yes</_XBOX360_buildinteractivemusic>\n" +
                      "<_XBOXONE_encryptionkey></_XBOXONE_encryptionkey>\n" +
                      "<_XBOXONE_builddirectory></_XBOXONE_builddirectory>\n" +
                      "<_XBOXONE_audiosourcedirectory></_XBOXONE_audiosourcedirectory>\n" +
                      "<_XBOXONE_prebuildcommands></_XBOXONE_prebuildcommands>\n" +
                      "<_XBOXONE_postbuildcommands></_XBOXONE_postbuildcommands>\n" +
                      "<_XBOXONE_buildinteractivemusic>Yes</_XBOXONE_buildinteractivemusic>\n" +
                      "<_PSP_encryptionkey></_PSP_encryptionkey>\n" +
                      "<_PSP_builddirectory></_PSP_builddirectory>\n" +
                      "<_PSP_audiosourcedirectory></_PSP_audiosourcedirectory>\n" +
                      "<_PSP_prebuildcommands></_PSP_prebuildcommands>\n" +
                      "<_PSP_postbuildcommands></_PSP_postbuildcommands>\n" +
                      "<_PSP_buildinteractivemusic>Yes</_PSP_buildinteractivemusic>\n" +
                      "<_PS3_encryptionkey></_PS3_encryptionkey>\n" +
                      "<_PS3_builddirectory></_PS3_builddirectory>\n" +
                      "<_PS3_audiosourcedirectory></_PS3_audiosourcedirectory>\n" +
                      "<_PS3_prebuildcommands></_PS3_prebuildcommands>\n" +
                      "<_PS3_postbuildcommands></_PS3_postbuildcommands>\n" +
                      "<_PS3_buildinteractivemusic>Yes</_PS3_buildinteractivemusic>\n" +
                      "<_PS4_encryptionkey></_PS4_encryptionkey>\n" +
                      "<_PS4_builddirectory></_PS4_builddirectory>\n" +
                      "<_PS4_audiosourcedirectory></_PS4_audiosourcedirectory>\n" +
                      "<_PS4_prebuildcommands></_PS4_prebuildcommands>\n" +
                      "<_PS4_postbuildcommands></_PS4_postbuildcommands>\n" +
                      "<_PS4_buildinteractivemusic>Yes</_PS4_buildinteractivemusic>\n" +
                      "<_WII_encryptionkey></_WII_encryptionkey>\n" +
                      "<_WII_builddirectory></_WII_builddirectory>\n" +
                      "<_WII_audiosourcedirectory></_WII_audiosourcedirectory>\n" +
                      "<_WII_prebuildcommands></_WII_prebuildcommands>\n" +
                      "<_WII_postbuildcommands></_WII_postbuildcommands>\n" +
                      "<_WII_buildinteractivemusic>Yes</_WII_buildinteractivemusic>\n" +
                      "<_WiiU_encryptionkey></_WiiU_encryptionkey>\n" +
                      "<_WiiU_builddirectory></_WiiU_builddirectory>\n" +
                      "<_WiiU_audiosourcedirectory></_WiiU_audiosourcedirectory>\n" +
                      "<_WiiU_prebuildcommands></_WiiU_prebuildcommands>\n" +
                      "<_WiiU_postbuildcommands></_WiiU_postbuildcommands>\n" +
                      "<_WiiU_buildinteractivemusic>Yes</_WiiU_buildinteractivemusic>\n" +
                      "<_3DS_encryptionkey></_3DS_encryptionkey>\n" +
                      "<_3DS_builddirectory></_3DS_builddirectory>\n" +
                      "<_3DS_audiosourcedirectory></_3DS_audiosourcedirectory>\n" +
                      "<_3DS_prebuildcommands></_3DS_prebuildcommands>\n" +
                      "<_3DS_postbuildcommands></_3DS_postbuildcommands>\n" +
                      "<_3DS_buildinteractivemusic>Yes</_3DS_buildinteractivemusic>\n" +
                      "<_NGP_encryptionkey></_NGP_encryptionkey>\n" +
                      "<_NGP_builddirectory></_NGP_builddirectory>\n" +
                      "<_NGP_audiosourcedirectory></_NGP_audiosourcedirectory>\n" +
                      "<_NGP_prebuildcommands></_NGP_prebuildcommands>\n" +
                      "<_NGP_postbuildcommands></_NGP_postbuildcommands>\n" +
                      "<_NGP_buildinteractivemusic>Yes</_NGP_buildinteractivemusic>\n" +
                      "<_ANDROID_encryptionkey></_ANDROID_encryptionkey>\n" +
                      "<_ANDROID_builddirectory></_ANDROID_builddirectory>\n" +
                      "<_ANDROID_audiosourcedirectory></_ANDROID_audiosourcedirectory>\n" +
                      "<_ANDROID_prebuildcommands></_ANDROID_prebuildcommands>\n" +
                      "<_ANDROID_postbuildcommands></_ANDROID_postbuildcommands>\n" +
                      "<_ANDROID_buildinteractivemusic>Yes</_ANDROID_buildinteractivemusic>\n" +
                      "<_IOS_encryptionkey></_IOS_encryptionkey>\n" +
                      "<_IOS_builddirectory></_IOS_builddirectory>\n" +
                      "<_IOS_audiosourcedirectory></_IOS_audiosourcedirectory>\n" +
                      "<_IOS_prebuildcommands></_IOS_prebuildcommands>\n" +
                      "<_IOS_postbuildcommands></_IOS_postbuildcommands>\n" +
                      "<_IOS_buildinteractivemusic>Yes</_IOS_buildinteractivemusic>\n" +
                      "<_BB10_encryptionkey></_BB10_encryptionkey>\n" +
                      "<_BB10_builddirectory></_BB10_builddirectory>\n" +
                      "<_BB10_audiosourcedirectory></_BB10_audiosourcedirectory>\n" +
                      "<_BB10_prebuildcommands></_BB10_prebuildcommands>\n" +
                      "<_BB10_postbuildcommands></_BB10_postbuildcommands>\n" +
                      "<_BB10_buildinteractivemusic>Yes</_BB10_buildinteractivemusic>\n" +
                      "<presavecommands></presavecommands>\n" +
                      "<postsavecommands></postsavecommands>\n" +
                      "<neweventusetemplate>0</neweventusetemplate>\n" +
                      "<neweventlasttemplatename></neweventlasttemplatename>\n" +
                      "\n" +
                      "<Composition>\n" +
                      "    <AuditionConsoleControlRepository>\n" +
                      "        <AuditionConsoleControl Type=\"Utility\" Xpos=\"18\" Ypos=\"18\">\n" +
                      "            <SimpleRack>\n" +
                      "                <ControlName>Reset</ControlName>\n" +
                      "                <ControlRange>0, 1</ControlRange>\n" +
                      "                <ControlWidget>\n" +
                      "                    <ResetControl/>\n" +
                      "                </ControlWidget>\n" +
                      "                <ControlMapping Type=\"3\" ID=\"0\"/>\n" +
                      "            </SimpleRack>\n" +
                      "        </AuditionConsoleControl>\n" +
                      "    </AuditionConsoleControlRepository>\n" +
                      "    <CompositionUI>\n" +
                      "        <SceneEditor>\n" +
                      "            <SceneEditorItemRepository/>\n" +
                      "        </SceneEditor>\n" +
                      "        <ThemeEditor>\n" +
                      "            <ThemeEditorItemRepository/>\n" +
                      "        </ThemeEditor>\n" +
                      "    </CompositionUI>\n" +
                      "    <CueFactory>\n" +
                      "        <UID>1</UID>\n" +
                      "    </CueFactory>\n" +
                      "    <CueRepository/>\n" +
                      "    <ExtLinkFactory>\n" +
                      "        <UID>1</UID>\n" +
                      "    </ExtLinkFactory>\n" +
                      "    <ExtLinkRepository/>\n" +
                      "    <ExtSegmentFactory>\n" +
                      "        <UID>1</UID>\n" +
                      "    </ExtSegmentFactory>\n" +
                      "    <MusicSettings>\n" +
                      "        <BaseVolume>1</BaseVolume>\n" +
                      "        <BaseReverbLevel>1</BaseReverbLevel>\n" +
                      "    </MusicSettings>\n" +
                      "    <ParameterFactory>\n" +
                      "        <UID>1</UID>\n" +
                      "    </ParameterFactory>\n" +
                      "    <ParameterRepository/>\n" +
                      "    <SceneRepository>\n" +
                      "        <Scene ID=\"1\">\n" +
                      "            <CueSheet></CueSheet>\n" +
                      "        </Scene>\n" +
                      "    </SceneRepository>\n" +
                      "    <SegmentRepository/>\n" +
                      "    <SharedFile/>\n" +
                      "    <ThemeFactory>\n" +
                      "        <UID>1</UID>\n" +
                      "    </ThemeFactory>\n" +
                      "    <ThemeRepository/>\n" +
                      "    <TimelineFactory>\n" +
                      "        <UID>1</UID>\n" +
                      "    </TimelineFactory>\n" +
                      "    <TimelineRepository/>\n" +
                      "</Composition></project>\n");
            stw.Close();
            Message_T.Text = "プロジェクトを作成しました。";
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
            Message_T.Text = "エラー:正常に作成できませんでした。";
        }
    }
    //どの音声かを取得
    //例:"battle_01.mp3"->"battle"
    static string Get_Voice_Type(string FilePath)
    {
        string NameOnly = System.IO.Path.GetFileName(FilePath);
        return NameOnly.Substring(0, NameOnly.LastIndexOf('_'));
    }
    //音声の種類のみを抽出
    //種類が被っていたらスキップ
    static string[] Get_Voice_Type_Only(string[] Voices)
    {
        List<string> Type_List = new List<string>();
        foreach (string Type in Voices)
        {
            string Type_Name = Get_Voice_Type(Type);
            bool IsOK = true;
            for (int Number = 0; Number <= Type_List.Count - 1; Number++)
            {
                if (Type_Name == Type_List[Number])
                {
                    IsOK = false;
                    break;
                }
            }
            if (IsOK)
            {
                Type_List.Add(Type_Name);
            }
        }
        return Type_List.ToArray();
    }
    //音声のレイヤーが2つある場合は使用する
    static string Set_Layer_By_Name(string Add_Name, bool LoopMode)
    {
        int Number;
        if (LoopMode)
        {
            Number = 0;
        }
        else
        {
            Number = 1;
        }
        string Temp = "<layer>\n" +
                      "<name>layer01</name>\n" +
                      "<height>100</height>\n" +
                      "<envelope_nextid>0</envelope_nextid>\n" +
                      "<mute>0</mute>\n" +
                      "<solo>0</solo>\n" +
                      "<soundlock>0</soundlock>\n" +
                      "<envlock>0</envlock>\n" +
                      "<priority>-1</priority>\n" +
                      "<sound>\n" +
                      "<name>/" + Add_Name + "</name>\n" +
                      "<x>0</x>\n" +
                      "<width>1</width>\n" +
                      "<startmode>0</startmode>\n" +
                      "<loopmode>" + Number + "</loopmode>\n" +
                      "<loopcount2>-1</loopcount2>\n" +
                      "<autopitchenabled>0</autopitchenabled>\n" +
                      "<autopitchparameter>0</autopitchparameter>\n" +
                      "<autopitchreference>0</autopitchreference>\n" +
                      "<autopitchatzero>0</autopitchatzero>\n" +
                      "<finetune>0</finetune>\n" +
                      "<volume>1</volume>\n" +
                      "<fadeintype>2</fadeintype>\n" +
                      "<fadeouttype>2</fadeouttype>\n" +
                      "</sound>\n" +
                      "<_PC_enable>1</_PC_enable>\n" +
                      "<_XBOX360_enable>1</_XBOX360_enable>\n" +
                      "<_XBOXONE_enable>1</_XBOXONE_enable>\n" +
                      "<_PSP_enable>1</_PSP_enable>\n" +
                      "<_PS3_enable>1</_PS3_enable>\n" +
                      "<_PS4_enable>1</_PS4_enable>\n" +
                      "<_WII_enable>1</_WII_enable>\n" +
                      "<_WiiU_enable>1</_WiiU_enable>\n" +
                      "<_3DS_enable>1</_3DS_enable>\n" +
                      "<_NGP_enable>1</_NGP_enable>\n" +
                      "<_ANDROID_enable>1</_ANDROID_enable>\n" +
                      "<_IOS_enable>1</_IOS_enable>\n" +
                      "<_BB10_enable>1</_BB10_enable>\n" +
                      "</layer>\n";
        return Temp;
    }
    //プロジェクトファイルにファイル情報を記録
    static string Set_Voice_By_Name(string File_Path)
    {
        string Temp = "<waveform>\n" +
                      "<filename>" + File_Path + "</filename>\n" +
                      "<guid>{53c7d2ab-a30c-4a64-af19-b93d10d76419}</guid>\n" +
                      "<mindistance>1</mindistance>\n" +
                      "<maxdistance>10000</maxdistance>\n" +
                      "<deffreq>44100</deffreq>\n" +
                      "<defvol>1</defvol>\n" +
                      "<defpan>0</defpan>\n" +
                      "<defpri>128</defpri>\n" +
                      "<xmafiltering>0</xmafiltering>\n" +
                      "<channelmode>0</channelmode>\n" +
                      "<quality_crossplatform>0</quality_crossplatform>\n" +
                      "<quality>-1</quality>\n" +
                      "<_PC_resamplemode>1</_PC_resamplemode>\n" +
                      "<_PC_optimisedratereduction>100</_PC_optimisedratereduction>\n" +
                      "<_PC_fixedsamplerate>48000</_PC_fixedsamplerate>\n" +
                      "<_XBOX360_resamplemode>1</_XBOX360_resamplemode>\n" +
                      "<_XBOX360_optimisedratereduction>100</_XBOX360_optimisedratereduction>\n" +
                      "<_XBOX360_fixedsamplerate>48000</_XBOX360_fixedsamplerate>\n" +
                      "<_XBOXONE_resamplemode>1</_XBOXONE_resamplemode>\n" +
                      "<_XBOXONE_optimisedratereduction>100</_XBOXONE_optimisedratereduction>\n" +
                      "<_XBOXONE_fixedsamplerate>48000</_XBOXONE_fixedsamplerate>\n" +
                      "<_PSP_resamplemode>1</_PSP_resamplemode>\n" +
                      "<_PSP_optimisedratereduction>100</_PSP_optimisedratereduction>\n" +
                      "<_PSP_fixedsamplerate>48000</_PSP_fixedsamplerate>\n" +
                      "<_PS3_resamplemode>1</_PS3_resamplemode>\n" +
                      "<_PS3_optimisedratereduction>100</_PS3_optimisedratereduction>\n" +
                      "<_PS3_fixedsamplerate>48000</_PS3_fixedsamplerate>\n" +
                      "<_PS4_resamplemode>1</_PS4_resamplemode>\n" +
                      "<_PS4_optimisedratereduction>100</_PS4_optimisedratereduction>\n" +
                      "<_PS4_fixedsamplerate>48000</_PS4_fixedsamplerate>\n" +
                      "<_WII_resamplemode>1</_WII_resamplemode>\n" +
                      "<_WII_optimisedratereduction>100</_WII_optimisedratereduction>\n" +
                      "<_WII_fixedsamplerate>48000</_WII_fixedsamplerate>\n" +
                      "<_WiiU_resamplemode>1</_WiiU_resamplemode>\n" +
                      "<_WiiU_optimisedratereduction>100</_WiiU_optimisedratereduction>\n" +
                      "<_WiiU_fixedsamplerate>48000</_WiiU_fixedsamplerate>\n" +
                      "<_3DS_resamplemode>1</_3DS_resamplemode>\n" +
                      "<_3DS_optimisedratereduction>100</_3DS_optimisedratereduction>\n" +
                      "<_3DS_fixedsamplerate>48000</_3DS_fixedsamplerate>\n" +
                      "<_NGP_resamplemode>1</_NGP_resamplemode>\n" +
                      "<_NGP_optimisedratereduction>100</_NGP_optimisedratereduction>\n" +
                      "<_NGP_fixedsamplerate>48000</_NGP_fixedsamplerate>\n" +
                      "<_ANDROID_resamplemode>1</_ANDROID_resamplemode>\n" +
                      "<_ANDROID_optimisedratereduction>100</_ANDROID_optimisedratereduction>\n" +
                      "<_ANDROID_fixedsamplerate>48000</_ANDROID_fixedsamplerate>\n" +
                      "<_IOS_resamplemode>1</_IOS_resamplemode>\n" +
                      "<_IOS_optimisedratereduction>100</_IOS_optimisedratereduction>\n" +
                      "<_IOS_fixedsamplerate>48000</_IOS_fixedsamplerate>\n" +
                      "<_BB10_resamplemode>1</_BB10_resamplemode>\n" +
                      "<_BB10_optimisedratereduction>100</_BB10_optimisedratereduction>\n" +
                      "<_BB10_fixedsamplerate>48000</_BB10_fixedsamplerate>\n" +
                      "<notes></notes>\n" +
                      "</waveform>\n";
        return Temp;
    }
    //音声はないが、SEが適応されていれば追加
    //引数:レイヤー名,SE名
    static string Not_Voice_Exist_SE_Add(string Add_Name, string SE_Name, bool IsLoopMode)
    {
        int Number;
        if (IsLoopMode)
        {
            Number = 0;
        }
        else
        {
            Number = 1;
        }
        string Temp;
        Temp = "<event>\n" +
               "<name>" + Add_Name + "</name>\n" +
               "<guid>{39a991f5-ea43-40dc-ad7c-17638e4ed2d9}</guid>\n" +
               "<parameter_nextid>0</parameter_nextid>\n" +
               "<layer_nextid>2</layer_nextid>\n" +
               "<layer>\n" +
               "<name>layer00</name>\n" +
               "<height>100</height>\n" +
               "<envelope_nextid>0</envelope_nextid>\n" +
               "<mute>0</mute>\n" +
               "<solo>0</solo>\n" +
               "<soundlock>0</soundlock>\n" +
               "<envlock>0</envlock>\n" +
               "<priority>-1</priority>\n" +
               "<sound>\n" +
               "<name>" + SE_Name + "</name>\n" +
               "<x>0</x>\n" +
               "<width>1</width>\n" +
               "<startmode>0</startmode>\n" +
               "<loopmode>" + Number + "</loopmode>\n" +
               "<loopcount2>-1</loopcount2>\n" +
               "<autopitchenabled>0</autopitchenabled>\n" +
               "<autopitchparameter>0</autopitchparameter>\n" +
               "<autopitchreference>0</autopitchreference>\n" +
               "<autopitchatzero>0</autopitchatzero>\n" +
               "<finetune>0</finetune>\n" +
               "<volume>1</volume>\n" +
               "<fadeintype>2</fadeintype>\n" +
               "<fadeouttype>2</fadeouttype>\n" +
               "</sound>\n" +
               "<_PC_enable>1</_PC_enable>\n" +
               "<_XBOX360_enable>1</_XBOX360_enable>\n" +
               "<_XBOXONE_enable>1</_XBOXONE_enable>\n" +
               "<_PS3_enable>1</_PS3_enable>\n" +
               "<_PS4_enable>1</_PS4_enable>\n" +
               "<_WII_enable>1</_WII_enable>\n" +
               "<_WiiU_enable>1</_WiiU_enable>\n" +
               "<_3DS_enable>1</_3DS_enable>\n" +
               "<_NGP_enable>1</_NGP_enable>\n" +
               "<_ANDROID_enable>1</_ANDROID_enable>\n" +
               "<_IOS_enable>1</_IOS_enable>\n" +
               "<_BB10_enable>1</_BB10_enable>\n" +
               "</layer>\n" +
               "<car_rpm>0</car_rpm>\n" +
               "<car_rpmsmooth>0.075</car_rpmsmooth>\n" +
               "<car_loadsmooth>0.05</car_loadsmooth>\n" +
               "<car_loadscale>6</car_loadscale>\n" +
               "<volume_db>0</volume_db>\n" +
               "<pitch>0</pitch>\n" +
               "<pitch_units>Octaves</pitch_units>\n" +
               "<pitch_randomization>0</pitch_randomization>\n" +
               "<pitch_randomization_units>Octaves</pitch_randomization_units>\n" +
               "<volume_randomization>0</volume_randomization>\n" +
               "<priority>128</priority>\n" +
               "<maxplaybacks>10</maxplaybacks>\n" +
               "<maxplaybacks_behavior>Just_fail_if_quietest</maxplaybacks_behavior>\n" +
               "<stealpriority>10000</stealpriority>\n" +
               "<mode>x_2d</mode>\n" +
               "<ignoregeometry>No</ignoregeometry>\n" +
               "<rolloff>Logarithmic</rolloff>\n" +
               "<mindistance>1</mindistance>\n" +
               "<maxdistance>10000</maxdistance>\n" +
               "<auto_distance_filtering>Off</auto_distance_filtering>\n" +
               "<distance_filter_centre_freq>1500</distance_filter_centre_freq>\n" +
               "<headrelative>World_relative</headrelative>\n" +
               "<oneshot>Yes</oneshot>\n" +
               "<istemplate>No</istemplate>\n" +
               "<usetemplate></usetemplate>\n" +
               "<notes></notes>\n" +
               "<category>ingame_voice</category>\n" +
               "<position_randomization_min>0</position_randomization_min>\n" +
               "<position_randomization>0</position_randomization>\n" +
               "<speaker_l>1</speaker_l>\n" +
               "<speaker_c>0</speaker_c>\n" +
               "<speaker_r>1</speaker_r>\n" +
               "<speaker_ls>0</speaker_ls>\n" +
               "<speaker_rs>0</speaker_rs>\n" +
               "<speaker_lb>0</speaker_lb>\n" +
               "<speaker_rb>0</speaker_rb>\n" +
               "<speaker_lfe>0</speaker_lfe>\n" +
               "<speaker_config>0</speaker_config>\n" +
               "<speaker_pan_r>1</speaker_pan_r>\n" +
               "<speaker_pan_theta>0</speaker_pan_theta>\n" +
               "<cone_inside_angle>360</cone_inside_angle>\n" +
               "<cone_outside_angle>360</cone_outside_angle>\n" +
               "<cone_outside_volumedb>0</cone_outside_volumedb>\n" +
               "<doppler_scale>1</doppler_scale>\n" +
               "<reverbdrylevel_db>0</reverbdrylevel_db>\n" +
               "<reverblevel_db>0</reverblevel_db>\n" +
               "<speaker_spread>0</speaker_spread>\n" +
               "<panlevel3d>1</panlevel3d>\n" +
               "<fadein_time>0</fadein_time>\n" +
               "<fadeout_time>1000</fadeout_time>\n" +
               "<spawn_intensity>1</spawn_intensity>\n" +
               "<spawn_intensity_randomization>0</spawn_intensity_randomization>\n" +
               "<TEMPLATE_PROP_LAYERS>1</TEMPLATE_PROP_LAYERS>\n" +
               "<TEMPLATE_PROP_KEEP_EFFECTS_PARAMS>1</TEMPLATE_PROP_KEEP_EFFECTS_PARAMS>\n" +
               "<TEMPLATE_PROP_VOLUME>0</TEMPLATE_PROP_VOLUME>\n" +
               "<TEMPLATE_PROP_PITCH>1</TEMPLATE_PROP_PITCH>\n" +
               "<TEMPLATE_PROP_PITCH_RANDOMIZATION>1</TEMPLATE_PROP_PITCH_RANDOMIZATION>\n" +
               "<TEMPLATE_PROP_VOLUME_RANDOMIZATION>1</TEMPLATE_PROP_VOLUME_RANDOMIZATION>\n" +
               "<TEMPLATE_PROP_PRIORITY>1</TEMPLATE_PROP_PRIORITY>\n" +
               "<TEMPLATE_PROP_MAX_PLAYBACKS>1</TEMPLATE_PROP_MAX_PLAYBACKS>\n" +
               "<TEMPLATE_PROP_MAX_PLAYBACKS_BEHAVIOR>1</TEMPLATE_PROP_MAX_PLAYBACKS_BEHAVIOR>\n" +
               "<TEMPLATE_PROP_STEAL_PRIORITY>1</TEMPLATE_PROP_STEAL_PRIORITY>\n" +
               "<TEMPLATE_PROP_MODE>1</TEMPLATE_PROP_MODE>\n" +
               "<TEMPLATE_PROP_IGNORE_GEOMETRY>1</TEMPLATE_PROP_IGNORE_GEOMETRY>\n" +
               "<TEMPLATE_PROP_X_3D_ROLLOFF>1</TEMPLATE_PROP_X_3D_ROLLOFF>\n" +
               "<TEMPLATE_PROP_X_3D_MIN_DISTANCE>1</TEMPLATE_PROP_X_3D_MIN_DISTANCE>\n" +
               "<TEMPLATE_PROP_X_3D_MAX_DISTANCE>1</TEMPLATE_PROP_X_3D_MAX_DISTANCE>\n" +
               "<TEMPLATE_PROP_X_3D_POSITION>1</TEMPLATE_PROP_X_3D_POSITION>\n" +
               "<TEMPLATE_PROP_X_3D_MIN_POSITION_RANDOMIZATION>1</TEMPLATE_PROP_X_3D_MIN_POSITION_RANDOMIZATION>\n" +
               "<TEMPLATE_PROP_X_3D_POSITION_RANDOMIZATION>1</TEMPLATE_PROP_X_3D_POSITION_RANDOMIZATION>\n" +
               "<TEMPLATE_PROP_X_3D_CONE_INSIDE_ANGLE>1</TEMPLATE_PROP_X_3D_CONE_INSIDE_ANGLE>\n" +
               "<TEMPLATE_PROP_X_3D_CONE_OUTSIDE_ANGLE>1</TEMPLATE_PROP_X_3D_CONE_OUTSIDE_ANGLE>\n" +
               "<TEMPLATE_PROP_X_3D_CONE_OUTSIDE_VOLUME>1</TEMPLATE_PROP_X_3D_CONE_OUTSIDE_VOLUME>\n" +
               "<TEMPLATE_PROP_X_3D_DOPPLER_FACTOR>1</TEMPLATE_PROP_X_3D_DOPPLER_FACTOR>\n" +
               "<TEMPLATE_PROP_REVERB_WET_LEVEL>1</TEMPLATE_PROP_REVERB_WET_LEVEL>\n" +
               "<TEMPLATE_PROP_REVERB_DRY_LEVEL>1</TEMPLATE_PROP_REVERB_DRY_LEVEL>\n" +
               "<TEMPLATE_PROP_X_3D_SPEAKER_SPREAD>1</TEMPLATE_PROP_X_3D_SPEAKER_SPREAD>\n" +
               "<TEMPLATE_PROP_X_3D_PAN_LEVEL>1</TEMPLATE_PROP_X_3D_PAN_LEVEL>\n" +
               "<TEMPLATE_PROP_X_2D_SPEAKER_L>1</TEMPLATE_PROP_X_2D_SPEAKER_L>\n" +
               "<TEMPLATE_PROP_X_2D_SPEAKER_C>1</TEMPLATE_PROP_X_2D_SPEAKER_C>\n" +
               "<TEMPLATE_PROP_X_2D_SPEAKER_R>1</TEMPLATE_PROP_X_2D_SPEAKER_R>\n" +
               "<TEMPLATE_PROP_X_2D_SPEAKER_LS>1</TEMPLATE_PROP_X_2D_SPEAKER_LS>\n" +
               "<TEMPLATE_PROP_X_2D_SPEAKER_RS>1</TEMPLATE_PROP_X_2D_SPEAKER_RS>\n" +
               "<TEMPLATE_PROP_X_2D_SPEAKER_LR>1</TEMPLATE_PROP_X_2D_SPEAKER_LR>\n" +
               "<TEMPLATE_PROP_X_2D_SPEAKER_RR>1</TEMPLATE_PROP_X_2D_SPEAKER_RR>\n" +
               "<TEMPLATE_PROP_X_SPEAKER_LFE>1</TEMPLATE_PROP_X_SPEAKER_LFE>\n" +
               "<TEMPLATE_PROP_ONESHOT>1</TEMPLATE_PROP_ONESHOT>\n" +
               "<TEMPLATE_PROP_FADEIN_TIME>1</TEMPLATE_PROP_FADEIN_TIME>\n" +
               "<TEMPLATE_PROP_FADEOUT_TIME>1</TEMPLATE_PROP_FADEOUT_TIME>\n" +
               "<TEMPLATE_PROP_NOTES>1</TEMPLATE_PROP_NOTES>\n" +
               "<TEMPLATE_PROP_USER_PROPERTIES>1</TEMPLATE_PROP_USER_PROPERTIES>\n" +
               "<TEMPLATE_PROP_CATEGORY>0</TEMPLATE_PROP_CATEGORY>\n" +
               "<_PC_enabled>1</_PC_enabled>\n" +
               "<_XBOX360_enabled>1</_XBOX360_enabled>\n" +
               "<_XBOXONE_enabled>1</_XBOXONE_enabled>\n" +
               "<_PSP_enabled>1</_PSP_enabled>\n" +
               "<_PS3_enabled>1</_PS3_enabled>\n" +
               "<_PS4_enabled>1</_PS4_enabled>\n" +
               "<_WII_enabled>1</_WII_enabled>\n" +
               "<_WiiU_enabled>1</_WiiU_enabled>\n" +
               "<_3DS_enabled>1</_3DS_enabled>\n" +
               "<_NGP_enabled>1</_NGP_enabled>\n" +
               "<_ANDROID_enabled>1</_ANDROID_enabled>\n" +
               "<_IOS_enabled>1</_IOS_enabled>\n" +
               "<_BB10_enabled>1</_BB10_enabled>\n" +
               "</event>\n";
        return Temp;
    }
    static string Read;
    static void Replace_Voice_Line(string Project_Name, string OldTextLine, string NewTextLine, bool IsLineDelete = false)
    {
        string Temp = "";
        string[] Lines = Read.Split('\n');
        foreach (string Line in Lines)
        {
            if (Line.Contains(OldTextLine + ":") && !IsLineDelete)
            {
                if (NewTextLine == "Music")
                {
                    Temp += "    " + OldTextLine + ": \"" + NewTextLine + "/" + NewTextLine + "/" + NewTextLine + "\"\n";
                }
                else
                {
                    Temp += "    " + OldTextLine + ": \"" + Project_Name + "/" + Project_Name + "/" + NewTextLine + "\"\n";
                }
                continue;
            }
            Temp += Line + "\n";
        }
        Read = Temp;
    }
    static void Change_Sounds_yaml(string Voice_Dir, string Project_Name)
    {
        try
        {
            Read = File.ReadAllText(Special_Path + "/Temp_Sounds.yaml", System.Text.Encoding.UTF8);
            if (Voice_Set.SE_Enable_List[9] || Sub_Code.File_Exist_Voice_Type(Voice_Dir, "reload"))
            {
                Replace_Voice_Line(Project_Name, "GUN_RELOAD", "reload");
            }
            if (Voice_Set.SE_Enable_List[11] || Sub_Code.File_Exist_Voice_Type(Voice_Dir, "hakken"))
            {
                Replace_Voice_Line(Project_Name, "ENEMY_SIGHTED", "hakken");
            }
            if (Voice_Set.SE_Enable_List[10] || Sub_Code.File_Exist_Voice_Type(Voice_Dir, "lamp"))
            {
                Replace_Voice_Line(Project_Name, "PLAYER_SIGHTED", "lamp");
            }
            if (Voice_Set.SE_Enable_List[0] || Sub_Code.File_Exist_Voice_Type(Voice_Dir, "battle_end"))
            {
                Replace_Voice_Line(Project_Name, "BASE_CAPTURE_FINISH", "battle_end");
            }
            if (Voice_Set.SE_Enable_List[13] || Sub_Code.File_Exist_Voice_Type(Voice_Dir, "lock"))
            {
                Replace_Voice_Line(Project_Name, "AUTO_ON", "lock");
            }
            if (Voice_Set.SE_Enable_List[14] || Sub_Code.File_Exist_Voice_Type(Voice_Dir, "unlock"))
            {
                Replace_Voice_Line(Project_Name, "AUTO_OFF", "unlock");
            }
            if (Sub_Code.File_Exist_Voice_Type(Voice_Dir, "mikata"))
            {
                Replace_Voice_Line(Project_Name, "VOICE_ALLY_KILLED", "mikata");
            }
            if (Voice_Set.SE_Enable_List[3] || Sub_Code.File_Exist_Voice_Type(Voice_Dir, "taiha"))
            {
                Replace_Voice_Line(Project_Name, "VOICE_VEHICLE_DESTROYED", "taiha");
            }
            if (Voice_Set.SE_Enable_List[1] || Sub_Code.File_Exist_Voice_Type(Voice_Dir, "map"))
            {
                Replace_Voice_Line(Project_Name, "CHAT_NOTIFICATION", "map");
            }
            if (Voice_Set.SE_Enable_List[4] || Sub_Code.File_Exist_Voice_Type(Voice_Dir, "kantuu"))
            {
                Replace_Voice_Line(Project_Name, "ENEMY_HP_DAMAGED_BY_PROJECTILE_BY_PLAYER", "kantuu");
            }
            if (Voice_Set.SE_Enable_List[5] || Sub_Code.File_Exist_Voice_Type(Voice_Dir, "tokusyu"))
            {
                Replace_Voice_Line(Project_Name, "ENEMY_HP_DAMAGED_BY_PROJECTILE_AND_CHASSIS_DAMAGED_BY_PLAYER", "tokusyu");
                Replace_Voice_Line(Project_Name, "ENEMY_HP_DAMAGED_BY_PROJECTILE_AND_GUN_DAMAGED_BY_PLAYER", "tokusyu");
                Replace_Voice_Line(Project_Name, "ENEMY_HP_DAMAGED_BY_EXPLOSION_AT_DIRECT_HIT_BY_PLAYER", "tokusyu");
                Replace_Voice_Line(Project_Name, "ENEMY_NO_HP_DAMAGE_AT_ATTEMPT_AND_CHASSIS_DAMAGED_BY_PLAYER", "tokusyu");
                Replace_Voice_Line(Project_Name, "ENEMY_NO_HP_DAMAGE_AT_ATTEMPT_AND_GUN_DAMAGED_BY_PLAYER", "tokusyu");
                Replace_Voice_Line(Project_Name, "ENEMY_NO_HP_DAMAGE_AT_NO_ATTEMPT_BY_PLAYER", "tokusyu");
                Replace_Voice_Line(Project_Name, "ENEMY_NO_HP_DAMAGE_AT_NO_ATTEMPT_AND_CHASSIS_DAMAGED_BY_PLAYER", "tokusyu");
                Replace_Voice_Line(Project_Name, "ENEMY_NO_HP_DAMAGE_AT_NO_ATTEMPT_AND_GUN_DAMAGED_BY_PLAYER", "tokusyu");
            }
            if (Voice_Set.SE_Enable_List[8] || Sub_Code.File_Exist_Voice_Type(Voice_Dir, "hikantuu"))
            {
                Replace_Voice_Line(Project_Name, "ENEMY_RICOCHET_BY_PLAYER", "hikantuu");
                Replace_Voice_Line(Project_Name, "ENEMY_NO_HP_DAMAGE_AT_ATTEMPT_BY_PLAYER", "hikantuu");
                Replace_Voice_Line(Project_Name, "ENEMY_NO_PIERCING_BY_PLAYER", "hikantuu");
            }
            if (Sub_Code.File_Exist_Voice_Type(Voice_Dir, "tekikasai"))
            {
                Replace_Voice_Line(Project_Name, "ENEMY_FIRE_STARTED_BY_PLAYER", "tekikasai");
            }
            if (Voice_Set.SE_Enable_List[1] || Sub_Code.File_Exist_Voice_Type(Voice_Dir, "battle"))
            {
                Replace_Voice_Line(Project_Name, "PREBATTLE_TIMER", "battle");
            }
            if (Voice_Set.SE_Enable_List[1] || Sub_Code.File_Exist_Voice_Type(Voice_Dir, "ryoukai"))
            {
                Replace_Voice_Line(Project_Name, "QCOMMAND_POSITIVE", "ryoukai");
            }
            if (Voice_Set.SE_Enable_List[1] || Sub_Code.File_Exist_Voice_Type(Voice_Dir, "kyohi"))
            {
                Replace_Voice_Line(Project_Name, "QCOMMAND_NEGATIVE", "kyohi");
            }
            if (Voice_Set.SE_Enable_List[1] || Sub_Code.File_Exist_Voice_Type(Voice_Dir, "keep"))
            {
                Replace_Voice_Line(Project_Name, "QCOMMAND_HOLD_POSITION", "keep");
            }
            if (Voice_Set.SE_Enable_List[1] || Sub_Code.File_Exist_Voice_Type(Voice_Dir, "help"))
            {
                Replace_Voice_Line(Project_Name, "QCOMMAND_HELP_ME", "help");
            }
            if (Voice_Set.SE_Enable_List[1] || Sub_Code.File_Exist_Voice_Type(Voice_Dir, "capture"))
            {
                Replace_Voice_Line(Project_Name, "QCOMMAND_CAPTURE_BASE", "capture");
            }
            if (Voice_Set.SE_Enable_List[1] || Sub_Code.File_Exist_Voice_Type(Voice_Dir, "defence"))
            {
                Replace_Voice_Line(Project_Name, "QCOMMAND_DEFEND_BASE", "defence");
            }
            if (Voice_Set.SE_Enable_List[1] || Sub_Code.File_Exist_Voice_Type(Voice_Dir, "attack"))
            {
                Replace_Voice_Line(Project_Name, "QCOMMAND_ATTACK", "attack");
            }
            if (Voice_Set.SE_Enable_List[1] || Sub_Code.File_Exist_Voice_Type(Voice_Dir, "attack_now"))
            {
                Replace_Voice_Line(Project_Name, "QCOMMAND_ATTACK_TARGET", "attack_now");
            }
            if (Sub_Code.File_Exist_Voice_Type(Voice_Dir, "kasai"))
            {
                Replace_Voice_Line(Project_Name, "fire_started", "kasai");
            }
            if (Sub_Code.File_Exist_Voice_Type(Voice_Dir, "syouka"))
            {
                Replace_Voice_Line(Project_Name, "fire_stopped", "syouka");
            }
            if (Sub_Code.File_Exist_Voice_Type(Voice_Dir, "ritaitaiha"))
            {
                Replace_Voice_Line(Project_Name, "track_destroyed", "ritaitaiha");
            }
            if (Sub_Code.File_Exist_Voice_Type(Voice_Dir, "ritaihason"))
            {
                Replace_Voice_Line(Project_Name, "track_damaged", "ritaihason");
            }
            if (Sub_Code.File_Exist_Voice_Type(Voice_Dir, "ritaihukkyuu"))
            {
                Replace_Voice_Line(Project_Name, "track_functional", "ritaihukkyuu");
                Replace_Voice_Line(Project_Name, "track_functional_can_move", "ritaihukkyuu");
            }
            if (Sub_Code.File_Exist_Voice_Type(Voice_Dir, "housinhason"))
            {
                Replace_Voice_Line(Project_Name, "gun_damaged", "housinhason");
            }
            if (Sub_Code.File_Exist_Voice_Type(Voice_Dir, "housintaiha"))
            {
                Replace_Voice_Line(Project_Name, "gun_destroyed", "housintaiha");
            }
            if (Sub_Code.File_Exist_Voice_Type(Voice_Dir, "housinhukkyuu"))
            {
                Replace_Voice_Line(Project_Name, "gun_functional", "housinhukkyuu");
            }
            if (Voice_Set.SE_Enable_List[6] || Sub_Code.File_Exist_Voice_Type(Voice_Dir, "musen"))
            {
                Replace_Voice_Line(Project_Name, "radio_damaged", "musen");
            }
            if (Sub_Code.File_Exist_Voice_Type(Voice_Dir, "kansokuhason"))
            {
                Replace_Voice_Line(Project_Name, "surveying_devices_damaged", "kansokuhason");
            }
            if (Sub_Code.File_Exist_Voice_Type(Voice_Dir, "kansokuhukkyuu"))
            {
                Replace_Voice_Line(Project_Name, "surveying_devices_functional", "kansokuhukkyuu");
            }
            if (Sub_Code.File_Exist_Voice_Type(Voice_Dir, "kansokutaiha"))
            {
                Replace_Voice_Line(Project_Name, "surveying_devices_destroyed", "kansokutaiha");
            }
            if (Sub_Code.File_Exist_Voice_Type(Voice_Dir, "houtouhason"))
            {
                Replace_Voice_Line(Project_Name, "turret_rotator_damaged", "houtouhason");
            }
            if (Sub_Code.File_Exist_Voice_Type(Voice_Dir, "houtouhukkyuu"))
            {
                Replace_Voice_Line(Project_Name, "turret_rotator_functional", "houtouhukkyuu");
            }
            if (Sub_Code.File_Exist_Voice_Type(Voice_Dir, "houtoutaiha"))
            {
                Replace_Voice_Line(Project_Name, "turret_rotator_destroyed", "houtoutaiha");
            }
            if (Sub_Code.File_Exist_Voice_Type(Voice_Dir, "danyaku"))
            {
                Replace_Voice_Line(Project_Name, "ammo_bay_damaged", "danyaku");
            }
            if (Sub_Code.File_Exist_Voice_Type(Voice_Dir, "enjinhukkyuu"))
            {
                Replace_Voice_Line(Project_Name, "engine_functional", "enjinhukkyuu");
            }
            if (Sub_Code.File_Exist_Voice_Type(Voice_Dir, "enjintaiha"))
            {
                Replace_Voice_Line(Project_Name, "engine_destroyed", "enjintaiha");
            }
            if (Sub_Code.File_Exist_Voice_Type(Voice_Dir, "enjinhason"))
            {
                Replace_Voice_Line(Project_Name, "engine_damaged", "enjinhason");
            }
            if (Sub_Code.File_Exist_Voice_Type(Voice_Dir, "nenryou"))
            {
                Replace_Voice_Line(Project_Name, "fuel_tank_damaged", "nenryou");
            }
            if (Sub_Code.File_Exist_Voice_Type(Voice_Dir, "musensyu"))
            {
                Replace_Voice_Line(Project_Name, "radioman_killed", "musensyu");
            }
            if (Sub_Code.File_Exist_Voice_Type(Voice_Dir, "soutensyu"))
            {
                Replace_Voice_Line(Project_Name, "loader_killed", "soutensyu");
            }
            if (Sub_Code.File_Exist_Voice_Type(Voice_Dir, "housyu"))
            {
                Replace_Voice_Line(Project_Name, "gunner_killed", "housyu");
            }
            if (Sub_Code.File_Exist_Voice_Type(Voice_Dir, "souzyuusyu"))
            {
                Replace_Voice_Line(Project_Name, "driver_killed", "souzyuusyu");
            }
            if (Sub_Code.File_Exist_Voice_Type(Voice_Dir, "syatyou"))
            {
                Replace_Voice_Line(Project_Name, "commander_killed", "syatyou");
            }
            if (Voice_Set.SE_Enable_List[4] || Sub_Code.File_Exist_Voice_Type(Voice_Dir, "gekiha"))
            {
                Replace_Voice_Line(Project_Name, "VOICE_ENEMY_KILLED", "gekiha");
            }
            Replace_Voice_Line(Project_Name, "VOICE_START_BATTLE", "Music");
            File.WriteAllText(Special_Path + "/Temp_Sounds.yaml", Read);
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
        }
    }
}