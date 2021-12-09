using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace WoTB_Voice_Mod_Creater
{
    public class Name_ID_Contaier
    {
        //イベント名
        public string Event_Name { get; set; } = "";
        //イベントID
        public uint Event_ID { get; set; } = 0;
        //自由に決めるID(イベント再生が終了したことを)
        public int Container_ID { get; set; } = 0;
        //イベントの長さ
        public int Max_Length { get; set; } = 0;
        //イベントの音量
        public double Volume { get; set; } = 75;
    }
    //謎のガルパン意識
    public class Container_und_Volume
    {
        public int Container_ID { get; set; }
        public double Volume { get; set; }
    }
    public class Wwise_Player
    {
        [DllImport("Wwise_Player.dll")]
        protected static extern IntPtr Wwise_Get_End_Event_Container_ID();
        [DllImport("Wwise_Player.dll")]
        protected static extern int Wwise_Get_End_Event_Count();
        [DllImport("Wwise_Player.dll")]
        protected static extern int Wwise_Get_Playing_Event_Count();
        [DllImport("Wwise_Player.dll")]
        protected static extern bool Wwise_Init(string Init_BNK, int Listener_Index, double Init_Volume = 1.0);
        [DllImport("Wwise_Player.dll")]
        protected static extern bool Wwise_Load_Bank(string Stream_BNK);
        [DllImport("Wwise_Player.dll")]
        protected static extern void Wwise_Set_Path(string Base_Dir_Path);
        [DllImport("Wwise_Player.dll")]
        protected static extern bool Wwise_Play_Name(string Name, int Container_ID, double Volume = -1);
        [DllImport("Wwise_Player.dll")]
        protected static extern bool Wwise_Play_ID(uint Event_ID, int Container_ID, double Volume = -1);
        [DllImport("Wwise_Player.dll")]
        protected static extern bool Wwise_Stop_Name(string Name);
        [DllImport("Wwise_Player.dll")]
        protected static extern bool Wwise_Stop_ID(uint Event_ID);
        [DllImport("Wwise_Player.dll")]
        protected static extern bool Wwise_Stop_Container_ID(int Container_ID);
        [DllImport("Wwise_Player.dll")]
        protected static extern void Wwise_Stop_All();
        [DllImport("Wwise_Player.dll")]
        protected static extern bool Wwise_Pause_All(bool IsRenderAudio);
        [DllImport("Wwise_Player.dll")]
        protected static extern bool Wwise_Play_All();
        [DllImport("Wwise_Player.dll")]
        protected static extern int Wwise_Get_Position_Name(string Name);
        [DllImport("Wwise_Player.dll")]
        protected static extern int Wwise_Get_Position_ID(uint Event_ID);
        [DllImport("Wwise_Player.dll")]
        protected static extern int Wwise_Get_Position_Container_ID(int Container_ID);
        [DllImport("Wwise_Player.dll")]
        protected static extern bool Wwise_Set_Position_Percent_Name(string Name, float Percent);
        [DllImport("Wwise_Player.dll")]
        protected static extern bool Wwise_Set_Position_Percent_ID(uint Event_ID, float Percent);
        [DllImport("Wwise_Player.dll")]
        protected static extern bool Wwise_Set_Position_Percent_Container_ID(int Container_ID, float Percent);
        [DllImport("Wwise_Player.dll")]
        protected static extern bool Wwise_Set_Position_Second_Name(string Name, int Position);
        [DllImport("Wwise_Player.dll")]
        protected static extern bool Wwise_Set_Position_Second_ID(uint Event_ID, int Position);
        [DllImport("Wwise_Player.dll")]
        protected static extern bool Wwise_Set_Position_Second_Container_ID(int Container_ID, int Position);
        [DllImport("Wwise_Player.dll")]
        protected static extern int Wwise_Get_Max_Length_Name(string Name);
        [DllImport("Wwise_Player.dll")]
        protected static extern int Wwise_Get_Max_Length_ID(uint Event_ID);
        [DllImport("Wwise_Player.dll")]
        protected static extern int Wwise_Get_Max_Length_Container_ID(int Container_ID);
        [DllImport("Wwise_Player.dll")]
        protected static extern bool Wwise_Set_Volume_Name(string Name, double Volume);
        [DllImport("Wwise_Player.dll")]
        protected static extern bool Wwise_Set_Volume_ID(uint Event_ID, double Volume);
        [DllImport("Wwise_Player.dll")]
        protected static extern bool Wwise_Set_Volume_Container_ID(int Container_ID, double Volume);
        [DllImport("Wwise_Player.dll")]
        protected static extern bool Wwise_Set_Volume_All(double Volume);
        [DllImport("Wwise_Player.dll")]
        protected static extern double Wwise_Get_Volume_Name(string Name);
        [DllImport("Wwise_Player.dll")]
        protected static extern double Wwise_Get_Volume_ID(uint Event_ID);
        [DllImport("Wwise_Player.dll")]
        protected static extern double Wwise_Get_Volume_Container_ID(int Container_ID);
        [DllImport("Wwise_Player.dll")]
        protected static extern IntPtr Wwise_Get_Volume_All();
        [DllImport("Wwise_Player.dll")]
        protected static extern IntPtr Wwise_Get_All_Container_ID();
        [DllImport("Wwise_Player.dll")]
        protected static extern void Wwise_Dispose();
        [DllImport("Wwise_Player.dll")]
        protected static extern bool Wwise_IsInited();
        [DllImport("Wwise_Player.dll")]
        protected static extern uint Wwise_Get_Result_Index();

        public static bool IsExecution { get; private set; } = false;

        /// <summary>
        /// Wwiseが読み込めるInit.bnkは1つのみなので、このクラスは2つ以上作成できません。2回以上宣言する場合は前のWwise_PlayerのDispose()を実行してメモリを解放してください。
        /// </summary>
        public static bool Init(string Init_BNK, int Listener_Index = 1, double Set_Init_Volume = 1.0)
        {
            if (IsExecution)
                //throw new Exception("Wwise_Player.dllは既に初期化済みです。Dispose()を実行する必要があります。");
                return false;
            if (!Wwise_Init(Init_BNK, Listener_Index, Set_Init_Volume))
                //throw new Exception("関数 Init(string Init_BNK, Listener_Index)を実行できませんでした。");
                return false;
            IsExecution = true;
            return true;
        }
        ///<summary>
        ///.bnkファイルをロードします。
        ///Init.bnkが異なる.bnkファイルはロードできません。
        ///</summary>
        public static bool Load_Bank(string Stream_BNK)
        {
            Wwise_Set_Path(Path.GetDirectoryName(Stream_BNK));
            return Wwise_Load_Bank(Path.GetFileName(Stream_BNK));
        }
        ///<summaty>
        ///終了したイベントID(Container_ID)をすべて取得します。
        ///一度受け取ると中身は空になります。
        ///1秒に1回は受け取るようにすることをおすすめします。
        ///</summaty>
        public static List<int> Get_End_Event_List()
        {
            List<int> Temp = new List<int>();
            int Count = Wwise_Get_End_Event_Count();
            int[] arrInt = new int[Count];
            IntPtr ptrInt = Wwise_Get_End_Event_Container_ID();
            Marshal.Copy(ptrInt, arrInt, 0, Count);
            Temp.AddRange(arrInt);
            return Temp;
        }
        ///<summary>
        ///再生中のイベント数を取得(同じイベントが複数再生中の場合も1つ1つカウントされます)
        ///例:再生中のイベント名が"Sample|Test|Sample"だった場合3になります。
        ///</summary>
        public static int Get_Playing_Event_Count()
        {
            return Wwise_Get_Playing_Event_Count();
        }
        ///<summary>
        ///指定したイベント名で再生します。
        ///</summary>
        public static bool Play(string Event_Name, int Container_ID, double Volume = -1)
        {
            return Wwise_Play_Name(Event_Name, Container_ID, Volume);
        }
        ///<summary>
        ///指定したイベントIDで再生します。
        ///</summary>
        public static bool Play(uint Event_ID, int Container_ID, double Volume = -1)
        {
            return Wwise_Play_ID(Event_ID, Container_ID, Volume);
        }
        ///<summary>
        ///指定したイベント名を停止
        ///</summary>
        public static bool Stop(string Event_Name)
        {
            return Wwise_Stop_Name(Event_Name);
        }
        ///<summary>
        ///指定したイベントIDを停止
        ///</summary>
        public static bool Stop(uint Event_ID)
        {
            return Wwise_Stop_ID(Event_ID);
        }
        ///<summary>
        ///指定したコンテナIDを停止
        ///</summary>
        public static bool Stop(int Container_ID)
        {
            return Wwise_Stop_Container_ID(Container_ID);
        }
        ///<summary>
        ///再生中のイベントをすべて停止
        ///</summary>
        public static void Stop()
        {
            Wwise_Stop_All();
        }
        ///<summary>
        ///再生中のイベントをすべて一時停止
        ///</summary>
        public static bool Pause_All(bool IsRenderAudio)
        {
            return Wwise_Pause_All(IsRenderAudio);
        }
        ///<summary>
        ///一時停止中のイベントをすべて再生
        ///</summary>
        public static bool Play_All()
        {
            return Wwise_Play_All();
        }
        ///<summary>
        ///指定したイベント名の現在の再生位置を取得
        ///</summary>
        public static int Get_Position(string Event_Name)
        {
            return Wwise_Get_Position_Name(Event_Name);
        }
        ///<summary>
        ///指定したイベントIDの現在の再生位置を取得
        ///</summary>
        public static int Get_Position(uint Event_ID)
        {
            return Wwise_Get_Position_ID(Event_ID);
        }
        ///<summary>
        ///指定したコンテナIDの現在の再生位置を取得
        ///</summary>
        public static int Get_Position(int Container_ID)
        {
            return Wwise_Get_Position_Container_ID(Container_ID);
        }
        ///<summary>
        ///指定したイベント名の再生位置を設定(0～1の範囲)
        ///</summary>
        public static bool Set_Position(string Event_Name, float Percent)
        {
            return Wwise_Set_Position_Percent_Name(Event_Name, Percent);
        }
        ///<summary>
        ///指定したイベントIDの再生位置を設定(0～1の範囲)
        ///</summary>
        public static bool Set_Position(uint Event_ID, float Percent)
        {
            return Wwise_Set_Position_Percent_ID(Event_ID, Percent);
        }
        ///<summary>
        ///指定したコンテナIDの再生位置を設定(0～1の範囲)
        ///</summary>
        public static bool Set_Position(int Container_ID, float Percent)
        {
            return Wwise_Set_Position_Percent_Container_ID(Container_ID, Percent);
        }
        ///<summary>
        ///指定したイベント名の再生位置を設定(ミリ秒の場合)
        ///</summary>
        public static bool Set_Position(string Event_Name, int Position)
        {
            return Wwise_Set_Position_Second_Name(Event_Name, Position);
        }
        ///<summary>
        ///指定したイベントIDの再生位置を設定(ミリ秒の場合)
        ///</summary>
        public static bool Set_Position(uint Event_ID, int Position)
        {
            return Wwise_Set_Position_Second_ID(Event_ID, Position);
        }
        ///<summary>
        ///指定したイベントIDの再生位置を設定(ミリ秒の場合)
        ///</summary>
        public static bool Set_Position(int Container_ID, int Position)
        {
            return Wwise_Set_Position_Second_Container_ID(Container_ID, Position);
        }
        ///<summary>
        ///指定したイベント名のサウンドの長さを取得
        ///イベント内に複数のサウンドが存在する場合、一番最初にヒットしたサウンドの長さを取得します
        ///</summary>
        public static int Get_Max_Length(string Event_Name)
        {
            return Wwise_Get_Max_Length_Name(Event_Name);
        }
        ///<summary>
        ///指定したイベントIDのサウンドの長さを取得
        ///イベント内に複数のサウンドが存在する場合、一番最初にヒットしたサウンドの長さを取得します
        ///</summary>
        public static int Get_Max_Length(uint Event_ID)
        {
            return Wwise_Get_Max_Length_ID(Event_ID);
        }
        ///<summary>
        ///指定したコンテナIDのサウンドの長さを取得
        ///イベント内に複数のサウンドが存在する場合、一番最初にヒットしたサウンドの長さを取得します
        ///</summary>
        public static int Get_Max_Length(int Container_ID)
        {
            return Wwise_Get_Max_Length_Container_ID(Container_ID);
        }
        ///<summary>
        ///指定したイベント名の音量を設定(0～1の範囲)
        ///</summary>
        public static bool Set_Volume(string Event_Name, double Volume)
        {
            return Wwise_Set_Volume_Name(Event_Name, Volume);
        }
        ///<summary>
        ///指定したイベントIDの音量を設定(0～1の範囲)
        ///</summary>
        public static bool Set_Volume(uint Event_ID, double Volume)
        {
            return Wwise_Set_Volume_ID(Event_ID, Volume);
        }
        ///<summary>
        ///指定したイベントIDの音量を設定(0～1の範囲)
        ///</summary>
        public static bool Set_Volume(int Container_ID, double Volume)
        {
            return Wwise_Set_Volume_Container_ID(Container_ID, Volume);
        }
        ///<summary>
        ///すべてのイベントの音量を設定(0～1の範囲)
        ///</summary>
        public static bool Set_Volume(double Volume)
        {
            return Wwise_Set_Volume_All(Volume);
        }
        ///<summary>
        ///指定したイベント名の音量を取得
        ///</summary>
        public static double Get_Volume(string Event_Name)
        {
            return Wwise_Get_Volume_Name(Event_Name);
        }
        ///<summary>
        ///指定したイベントIDの音量を取得
        ///</summary>
        public static double Get_Volume(uint Event_ID)
        {
            return Wwise_Get_Volume_ID(Event_ID);
        }
        ///<summary>
        ///指定したイベントIDの音量を取得
        ///</summary>
        public static double Get_Volume(int Container_ID)
        {
            return Wwise_Get_Volume_Container_ID(Container_ID);
        }
        ///<summary>
        ///すべてのイベントの音量を取得
        ///</summary>
        public static List<Container_und_Volume> Get_Volume()
        {
            List<Container_und_Volume> Result = new List<Container_und_Volume>();
            int Count = Wwise_Get_Playing_Event_Count();
            //音量の配列
            double[] arrDouble = new double[Count];
            IntPtr ptrInt_Volume = Wwise_Get_Volume_All();
            Marshal.Copy(ptrInt_Volume, arrDouble, 0, Count);
            List<double> Volumes = new List<double>();
            Volumes.AddRange(arrDouble);
            //Container_IDの配列
            int[] arrInt = new int[Count];
            IntPtr ptrInt_ID = Wwise_Get_All_Container_ID();
            Marshal.Copy(ptrInt_ID, arrInt, 0, Count);
            List<int> Container_IDs = new List<int>();
            Container_IDs.AddRange(arrInt);
            //配列同士を合わせる
            for (int Number = 0; Number < Volumes.Count; Number++)
            {
                Container_und_Volume Temp = new Container_und_Volume();
                Temp.Volume = Volumes[Number];
                Temp.Container_ID = Container_IDs[Number];
                Result.Add(Temp);
            }
            return Result;
        }
        ///<summary>
        ///メモリを解放
        ///このメゾットを実行すると、すべてのメゾットの使用ができなくなります。
        ///</summary>
        public static void Dispose()
        {
            if (IsExecution)
            {
                Wwise_Dispose();
                IsExecution = false;
            }
        }
        ///<summary>
        ///Wwiseの初期化がされているかを取得
        ///</summary>
        public static bool IsInited()
        {
            return Wwise_IsInited();
        }
        ///<summary>
        ///Wwiseから返された最後のログを取得
        ///</summary>
        public static uint Get_Result_Index()
        {
            return Wwise_Get_Result_Index();
        }
    }
}