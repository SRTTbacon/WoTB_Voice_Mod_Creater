using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using WoTB_Voice_Mod_Creater.Class;
using WoTB_Voice_Mod_Creater.FMOD_Class;

namespace WoTB_Voice_Mod_Creater
{
    public class Voice_Set
    {
        static List<List<string>> Voice_BGM_Change_List = new List<List<string>>();
        static List<string> Voice_Lists = new List<string>();
        public static List<bool> SE_Enable_Disable = new List<bool>();
        public static string Local_Path = Directory.GetCurrentDirectory();
        static string Special_Path_Dir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\WoTB_Voice_Mod_Creater";
        static string Server_Name = "";
        static string User_Name = "";
        static string WoTB_Location = "";
        static string WoT_Mod_Location = "";
        static int Voice_Number = 0;
        static bool IsBusy = false;
        public static List<string> Voice_Files
        {
            get { return Voice_Lists; }
            set { Voice_Lists = value; }
        }
        public static string SRTTbacon_Server_Name
        {
            get { return Server_Name; }
            set { Server_Name = value; }
        }
        public static int Voice_Files_Number
        {
            get { return Voice_Number; }
            set { Voice_Number = value; }
        }
        public static string UserName
        {
            get { return User_Name; }
            set { User_Name = value; }
        }
        public static bool App_Busy
        {
            get { return IsBusy; }
            set { IsBusy = value; }
        }
        public static string WoTB_Path
        {
            get { return WoTB_Location; }
            set { WoTB_Location = value; }
        }
        public static string WoT_Mod_Path
        {
            get { return WoT_Mod_Location; }
            set { WoT_Mod_Location = value; }
        }
        public static string Special_Path
        {
            get { return Special_Path_Dir; }
            set { Special_Path_Dir = value; }
        }
        //ファイル名が既に変更されたものか
        public static bool Voice_Name_Hide(string File)
        {
            string a = File;
            if (a.Contains("mikata") || a.Contains("danyaku") || a.Contains("hikantuu") || a.Contains("kantuu") || a.Contains("tokusyu") || a.Contains("tyoudan") || a.Contains("syatyou") ||
                a.Contains("souzyuusyu") || a.Contains("tekikasai") || a.Contains("gekiha") || a.Contains("enjinhason") || a.Contains("enjintaiha") || a.Contains("enjinhukkyuu") || a.Contains("kasai") ||
                a.Contains("syouka") || a.Contains("nenryou") || a.Contains("housinhason") || a.Contains("housintaiha") || a.Contains("housinhukkyuu") || a.Contains("housyu") || a.Contains("soutensyu") ||
                a.Contains("musen") || a.Contains("musensyu") || a.Contains("battle") || a.Contains("kansokuhason") || a.Contains("kansokutaiha") || a.Contains("kansokuhukkyuu") || a.Contains("ritaihason") ||
                a.Contains("ritaitaiha") || a.Contains("ritaihukkyuu") || a.Contains("houtouhason") || a.Contains("houtoutaiha") || a.Contains("houtouhukkyuu") || a.Contains("taiha") || a.Contains("hakken") ||
                a.Contains("lamp") || a.Contains("ryoukai") || a.Contains("kyohi") || a.Contains("help") || a.Contains("attack") || a.Contains("attack_now") || a.Contains("capture") || a.Contains("defence") ||
                a.Contains("keep") || a.Contains("lock") || a.Contains("unlock") || a.Contains("reload") || a.Contains("map") || a.Contains("battle_end") || a.Contains("chat_allies_send") ||
                a.Contains("chat_allies_receive") || a.Contains("chat_enemy_send") || a.Contains("chat_enemy_receive") || a.Contains("chat_platoon_send") || a.Contains("chat_platoon_receive"))
            {
                return true;
            }
            return false;
        }
        //音声の日本語の種類を取得
        public static string Get_Voice_Type_Japanese_Name(string Voice_Name)
        {
            if (Voice_Name == "mikata")
                return "フレンドリーファイヤー";
            else if (Voice_Name == "danyaku")
                return "弾薬庫損傷";
            else if (Voice_Name == "hikantuu")
                return "敵への非貫通";
            else if (Voice_Name == "kantuu")
                return "敵への貫通";
            else if (Voice_Name == "tokusyu")
                return "敵へのモジュール損傷";
            else if (Voice_Name == "tyoudan")
                return "敵への跳弾";
            else if (Voice_Name == "syatyou")
                return "車長負傷";
            else if (Voice_Name == "souzyuusyu")
                return "操縦手負傷";
            else if (Voice_Name == "tekikasai")
                return "敵車両火災発生";
            else if (Voice_Name == "gekiha")
                return "敵車両撃破";
            else if (Voice_Name == "enjinhason")
                return "エンジン損傷";
            else if (Voice_Name == "enjintaiha")
                return "エンジン大破";
            else if (Voice_Name == "enjinhukkyuu")
                return "エンジン復旧";
            else if (Voice_Name == "kasai")
                return "自車両火災発生";
            else if (Voice_Name == "syouka")
                return "自車両消火";
            else if (Voice_Name == "nenryou")
                return "燃料タンク損傷";
            else if (Voice_Name == "housinhason")
                return "砲身破損";
            else if (Voice_Name == "housintaiha")
                return "砲身大破";
            else if (Voice_Name == "housinhukkyuu")
                return "砲身復旧";
            else if (Voice_Name == "housyu")
                return "砲手負傷";
            else if (Voice_Name == "soutensyu")
                return "装填手負傷";
            else if (Voice_Name == "musen")
                return "無線機破損";
            else if (Voice_Name == "musensyu")
                return "無選手負傷";
            else if (Voice_Name == "battle")
                return "戦闘開始";
            else if (Voice_Name == "kansokuhason")
                return "観測装置破損";
            else if (Voice_Name == "kansokutaiha")
                return "観測装置大破";
            else if (Voice_Name == "kansokuhukkyuu")
                return "観測装置復旧";
            else if (Voice_Name == "ritaihason")
                return "履帯破損";
            else if (Voice_Name == "ritaitaiha")
                return "履帯大破";
            else if (Voice_Name == "ritaihukkyuu")
                return "履帯復旧";
            else if (Voice_Name == "houtouhason")
                return "砲塔破損";
            else if (Voice_Name == "houtoutaiha")
                return "砲塔大破";
            else if (Voice_Name == "houtouhukkyuu")
                return "砲塔復旧";
            else if (Voice_Name == "taiha")
                return "自車両大破";
            else if (Voice_Name == "hakken")
                return "敵発見";
            else if (Voice_Name == "lamp")
                return "第六感";
            else if (Voice_Name == "ryoukai")
                return "了解";
            else if (Voice_Name == "kyohi")
                return "拒否";
            else if (Voice_Name == "help")
                return "救援を請う";
            else if (Voice_Name == "attack")
                return "攻撃せよ！";
            else if (Voice_Name == "attack_now")
                return "攻撃中";
            else if (Voice_Name == "capture")
                return "陣地を占領せよ！";
            else if (Voice_Name == "defence")
                return "陣地を防衛せよ！";
            else if (Voice_Name == "keep")
                return "固守せよ！";
            else if (Voice_Name == "lock")
                return "ロックオン";
            else if (Voice_Name == "unlock")
                return "アンロック";
            else if (Voice_Name == "reload")
                return "装填完了";
            else if (Voice_Name == "map")
                return "マップクリック時";
            else if (Voice_Name == "battle_end")
                return "戦闘終了時";
            else if (Voice_Name == "battle_bgm")
                return "戦闘BGM";
            else if (Voice_Name == "chat_allies_send")
                return "チャット:味方-送信";
            else if (Voice_Name == "chat_allies_receive")
                return "チャット:味方-受信";
            else if (Voice_Name == "chat_enemy_send")
                return "チャット:全体-送信";
            else if (Voice_Name == "chat_enemy_receive")
                return "チャット:全体-受信";
            else if (Voice_Name == "chat_platoon_send")
                return "チャット:小隊-送信";
            else if (Voice_Name == "chat_platoon_receive")
                return "チャット:小隊-受信";
            else
                return "";
        }
        //音声の日本語の種類を取得(V2)
        public static string Get_Voice_Type_Japanese_Name_V2(int Voice_Number)
        {
            if (Voice_Number == 0)
                return "フレンドリーファイヤー";
            else if (Voice_Number == 1)
                return "弾薬庫損傷";
            else if (Voice_Number == 2)
                return "敵への非貫通";
            else if (Voice_Number == 3)
                return "敵への貫通";
            else if (Voice_Number == 4)
                return "敵へのモジュール損傷";
            else if (Voice_Number == 5)
                return "敵への跳弾";
            else if (Voice_Number == 6)
                return "車長負傷";
            else if (Voice_Number == 7)
                return "操縦手負傷";
            else if (Voice_Number == 8)
                return "敵車両火災発生";
            else if (Voice_Number == 9)
                return "敵車両撃破";
            else if (Voice_Number == 10)
                return "エンジン損傷";
            else if (Voice_Number == 11)
                return "エンジン大破";
            else if (Voice_Number == 12)
                return "エンジン復旧";
            else if (Voice_Number == 13)
                return "自車両火災発生";
            else if (Voice_Number == 14)
                return "自車両消火";
            else if (Voice_Number == 15)
                return "燃料タンク損傷";
            else if (Voice_Number == 16)
                return "砲身破損";
            else if (Voice_Number == 17)
                return "砲身大破";
            else if (Voice_Number == 18)
                return "砲身復旧";
            else if (Voice_Number == 19)
                return "砲手負傷";
            else if (Voice_Number == 20)
                return "装填手負傷";
            else if (Voice_Number == 21)
                return "無線機破損";
            else if (Voice_Number == 22)
                return "無選手負傷";
            else if (Voice_Number == 23)
                return "戦闘開始";
            else if (Voice_Number == 24)
                return "観測装置破損";
            else if (Voice_Number == 25)
                return "観測装置大破";
            else if (Voice_Number == 26)
                return "観測装置復旧";
            else if (Voice_Number == 27)
                return "履帯破損";
            else if (Voice_Number == 28)
                return "履帯大破";
            else if (Voice_Number == 29)
                return "履帯復旧";
            else if (Voice_Number == 30)
                return "砲塔破損";
            else if (Voice_Number == 31)
                return "砲塔大破";
            else if (Voice_Number == 32)
                return "砲塔復旧";
            else if (Voice_Number == 33)
                return "自車両大破";
            else if (Voice_Number == 34)
                return "敵発見";
            else if (Voice_Number == 35)
                return "第六感";
            else if (Voice_Number == 36)
                return "了解";
            else if (Voice_Number == 37)
                return "拒否";
            else if (Voice_Number == 38)
                return "救援を請う";
            else if (Voice_Number == 39)
                return "攻撃せよ！";
            else if (Voice_Number == 40)
                return "攻撃中";
            else if (Voice_Number == 41)
                return "陣地を占領せよ！";
            else if (Voice_Number == 42)
                return "陣地を防衛せよ！";
            else if (Voice_Number == 43)
                return "固守せよ！";
            else if (Voice_Number == 44)
                return "ロックオン";
            else if (Voice_Number == 45)
                return "アンロック";
            else if (Voice_Number == 46)
                return "装填完了";
            else if (Voice_Number == 47)
                return "マップクリック時";
            else if (Voice_Number == 48)
                return "戦闘終了時";
            else if (Voice_Number == 49)
                return "戦闘BGM";
            else if (Voice_Number == 50)
                return "チャット:味方-送信";
            else if (Voice_Number == 51)
                return "チャット:味方-受信";
            else if (Voice_Number == 52)
                return "チャット:全体-送信";
            else if (Voice_Number == 53)
                return "チャット:全体-受信";
            else if (Voice_Number == 54)
                return "チャット:小隊-送信";
            else if (Voice_Number == 55)
                return "チャット:小隊-受信";
            else
                return "";
        }
        //インデックスからローマ字の音声タイプを取得
        public static string Get_Voice_Type_Romaji_Name(int Index)
        {
            if (Index == 0)
                return "mikata";
            else if (Index == 1)
                return "danyaku";
            else if (Index == 2)
                return "hikantuu";
            else if (Index == 3)
                return "kantuu";
            else if (Index == 4)
                return "tokusyu";
            else if (Index == 5)
                return "tyoudan";
            else if (Index == 6)
                return "syatyou";
            else if (Index == 7)
                return "souzyuusyu";
            else if (Index == 8)
                return "tekikasai";
            else if (Index == 9)
                return "gekiha";
            else if (Index == 10)
                return "enjinhason";
            else if (Index == 11)
                return "enjintaiha";
            else if (Index == 12)
                return "enjinhukkyuu";
            else if (Index == 13)
                return "kasai";
            else if (Index == 14)
                return "syouka";
            else if (Index == 15)
                return "nenryou";
            else if (Index == 16)
                return "housinhason";
            else if (Index == 17)
                return "housintaiha";
            else if (Index == 18)
                return "housinhukkyuu";
            else if (Index == 19)
                return "housyu";
            else if (Index == 20)
                return "soutensyu";
            else if (Index == 21)
                return "musen";
            else if (Index == 22)
                return "musensyu";
            else if (Index == 23)
                return "battle";
            else if (Index == 24)
                return "kansokuhason";
            else if (Index == 25)
                return "kansokutaiha";
            else if (Index == 26)
                return "kansokuhukkyuu";
            else if (Index == 27)
                return "ritaihason";
            else if (Index == 28)
                return "ritaitaiha";
            else if (Index == 29)
                return "ritaihukkyuu";
            else if (Index == 30)
                return "houtouhason";
            else if (Index == 31)
                return "houtoutaiha";
            else if (Index == 32)
                return "houtouhukkyuu";
            else if (Index == 33)
                return "taiha";
            else if (Index == 34)
                return "hakken";
            else if (Index == 35)
                return "lamp";
            else if (Index == 36)
                return "ryoukai";
            else if (Index == 37)
                return "kyohi";
            else if (Index == 38)
                return "help";
            else if (Index == 39)
                return "attack";
            else if (Index == 40)
                return "attack_now";
            else if (Index == 41)
                return "capture";
            else if (Index == 42)
                return "defence";
            else if (Index == 43)
                return "keep";
            else if (Index == 44)
                return "lock";
            else if (Index == 45)
                return "unlock";
            else if (Index == 46)
                return "reload";
            else if (Index == 47)
                return "map";
            else if (Index == 48)
                return "battle_end";
            else if (Index == 49)
                return "battle_bgm";
            else if (Index == 50)
                return "chat_allies_send";
            else if (Index == 51)
                return "chat_allies_receive";
            else if (Index == 52)
                return "chat_enemy_send";
            else if (Index == 53)
                return "chat_enemy_receive";
            else if (Index == 54)
                return "chat_platoon_send";
            else if (Index == 55)
                return "chat_platoon_receive";
            else
                return "";
        }
        public static void Voice_BGM_Change_List_Init()
        {
            if (!File.Exists(Special_Path + "/Wwise/Change_To_Wwise.dat"))
                return;
            try
            {
                Voice_BGM_Change_List.Clear();
                for (int Number = 0; Number < 37; Number++)
                    Voice_BGM_Change_List.Add(new List<string>());
                string line;
                int Number_01 = -1;
                StreamReader str = new StreamReader(Special_Path + "/Wwise/Change_To_Wwise.dat");
                while ((line = str.ReadLine()) != null)
                {
                    if (line[0] == '・')
                    {
                        Number_01++;
                        continue;
                    }
                    Voice_BGM_Change_List[Number_01].Add(line.Trim());
                }
                str.Close();
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        public static void Voice_BGM_Name_Change_From_FSB(string Dir_Path)
        {
            if (!Directory.Exists(Dir_Path))
                return;
            if (Voice_BGM_Change_List.Count == 0)
                Voice_BGM_Change_List_Init();
            string[] To_File_Name = { "mikata", "danyaku", "hikantuu", "kantuu", "tokusyu", "tyoudan", "syatyou", "souzyuusyu", "tekikasai", "gekiha", "enjinhason", "enjintaiha", "enjinhukkyuu"
            ,"kasai","syouka","nenryou","housinhason","housintaiha","housinhukkyuu","housyu","soutensyu","musen","musensyu","battle","kansokuhason","kansokutaiha"
            ,"kansokuhukkyuu","ritaihason","ritaitaiha","ritaihukkyuu","houtouhason","houtoutaiha","houtouhukkyuu","taiha","battle_bgm","reload","touzyouin"};
            string[] Files = Directory.GetFiles(Dir_Path, "*.wav", SearchOption.TopDirectoryOnly);
            foreach (string File_Now in Files)
            {
                string Name = "";
                string Name_Only = Path.GetFileNameWithoutExtension(File_Now);
                if (Name_Only.Contains(" ("))
                {
                    Name = Name_Only.Substring(0, Name_Only.LastIndexOf('(') - 1);
                    Name = Name.Trim();
                }
                else if (Name_Only.Contains("_"))
                {
                    Name = Name_Only.Substring(0, Name_Only.LastIndexOf('_'));
                    Name = Name.Trim();
                }
                if (Name == "")
                    continue;
                for (int Number = 0; Number < 37; Number++)
                {
                    foreach (string Voice_Name in Voice_BGM_Change_List[Number])
                    {
                        if (Name.Contains(Voice_Name))
                        {
                            if (Number == 36)
                            {
                                File.Copy(File_Now, Sub_Code.File_Rename_Get_Name(Path.GetDirectoryName(File_Now) + "\\syatyou") + ".wav", true);
                                File.Copy(File_Now, Sub_Code.File_Rename_Get_Name(Path.GetDirectoryName(File_Now) + "\\souzyuusyu") + ".wav", true);
                                File.Copy(File_Now, Sub_Code.File_Rename_Get_Name(Path.GetDirectoryName(File_Now) + "\\housyu") + ".wav", true);
                                File.Copy(File_Now, Sub_Code.File_Rename_Get_Name(Path.GetDirectoryName(File_Now) + "\\housyu") + ".wav", true);
                                File.Copy(File_Now, Sub_Code.File_Rename_Get_Name(Path.GetDirectoryName(File_Now) + "\\soutensyu") + ".wav", true);
                                Sub_Code.File_Move(File_Now, Sub_Code.File_Rename_Get_Name(Path.GetDirectoryName(File_Now) + "\\musensyu") + ".wav", true);
                                continue;
                            }
                            Sub_Code.File_Rename_Number(File_Now, To_File_Name[Number]);
                        }
                    }
                }
            }
        }
        public static List<List<string>> Voice_BGM_Name_Change_From_FSB_To_Index(string Dir_Path)
        {
            if (!Directory.Exists(Dir_Path))
                return new List<List<string>>();
            if (Voice_BGM_Change_List.Count == 0)
                Voice_BGM_Change_List_Init();
            List<List<string>> Temp = new List<List<string>>();
            for (int Number_01 = 0; Number_01 < 34; Number_01++)
                Temp.Add(new List<string>());
            List<string> To_File_Name = new List<string>();To_File_Name.AddRange(new string[]{ "mikata", "danyaku", "hikantuu", "kantuu", "tokusyu", "tyoudan", "syatyou", "souzyuusyu", "tekikasai", "gekiha", "enjinhason", "enjintaiha", "enjinhukkyuu"
            ,"kasai","syouka","nenryou","housinhason","housintaiha","housinhukkyuu","housyu","soutensyu","musen","musensyu","battle","kansokuhason","kansokutaiha"
            ,"kansokuhukkyuu","ritaihason","ritaitaiha","ritaihukkyuu","houtouhason","houtoutaiha","houtouhukkyuu","taiha"});
            string[] Files = Directory.GetFiles(Dir_Path, "*.wav", SearchOption.TopDirectoryOnly);
            foreach (string File_Now in Files)
            {
                string Name_Temp = Path.GetFileNameWithoutExtension(File_Now);
                if (!Sub_Code.IsIncludeInt_From_String_V2(Name_Temp, "_"))
                    continue;
                if (Name_Temp.Contains("_"))
                {
                    string Name = Name_Temp.Substring(0, Name_Temp.LastIndexOf('_'));
                    Name = Name.Trim();
                    for (int Number = 0; Number < 34; Number++)
                    {
                        if (To_File_Name[Number] == Name)
                        {
                            Temp[Number].Add(Name_Temp);
                            break;
                        }
                        foreach (string Voice_Name in Voice_BGM_Change_List[Number])
                        {
                            if (Name.Contains(Voice_Name))
                            {
                                Temp[Number].Add(Name_Temp);
                                break;
                            }
                        }
                    }
                }
            }
            return Temp;
        }
        public static List<List<string>> Voice_BGM_Name_Change_From_FSB_To_Index_FSBFile(string FSB_File_Path)
        {
            List<List<string>> Temp = new List<List<string>>();
            for (int Number_01 = 0; Number_01 < 34; Number_01++)
                Temp.Add(new List<string>());
            if (Voice_BGM_Change_List.Count == 0)
                Voice_BGM_Change_List_Init();
            List<string> To_File_Name = new List<string>(); To_File_Name.AddRange(new string[]{ "mikata", "danyaku", "hikantuu", "kantuu", "tokusyu", "tyoudan", "syatyou", "souzyuusyu", "tekikasai", "gekiha", "enjinhason", "enjintaiha", "enjinhukkyuu"
            ,"kasai","syouka","nenryou","housinhason","housintaiha","housinhukkyuu","housyu","soutensyu","musen","musensyu","battle","kansokuhason","kansokutaiha"
            ,"kansokuhukkyuu","ritaihason","ritaitaiha","ritaihukkyuu","houtouhason","houtoutaiha","houtouhukkyuu","taiha"});
            List<string> Files = Fmod_Class.FSB_GetNames(FSB_File_Path);
            foreach (string File_Now in Files)
            {
                string Name_Temp = Path.GetFileNameWithoutExtension(File_Now);
                if (!Sub_Code.IsIncludeInt_From_String_V2(Name_Temp, "_"))
                    continue;
                if (Name_Temp.Contains("_"))
                {
                    string Name = Name_Temp.Substring(0, Name_Temp.LastIndexOf('_'));
                    Name = Name.Trim();
                    for (int Number = 0; Number < 34; Number++)
                    {
                        if (To_File_Name[Number] == Name)
                        {
                            Temp[Number].Add(Name_Temp);
                            break;
                        }
                        foreach (string Voice_Name in Voice_BGM_Change_List[Number])
                        {
                            if (Name.Contains(Voice_Name))
                            {
                                Temp[Number].Add(Name_Temp);
                                break;
                            }
                        }
                    }
                }
            }
            return Temp;
        }
    }
}