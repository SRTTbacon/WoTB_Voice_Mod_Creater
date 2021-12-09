using System.Collections.Generic;

public class Voice_Mod_Create
{
    //どの音声かを取得
    //例:"battle_01.mp3"->"battle"
    public static string Get_Voice_Type_V1(string FilePath)
    {
        string NameOnly = System.IO.Path.GetFileNameWithoutExtension(FilePath);
        if (!NameOnly.Contains("_"))
            return NameOnly;
        return NameOnly.Substring(0, NameOnly.LastIndexOf('_'));
    }
    public static string Get_Voice_Type_V2(string FilePath)
    {
        string NameOnly = System.IO.Path.GetFileNameWithoutExtension(FilePath);
        if (!NameOnly.Contains("_"))
            return NameOnly;
        return NameOnly.Substring(0, NameOnly.IndexOf('_'));
    }
    //音声の種類のみを抽出
    //種類が被っていたらスキップ
    public static string[] Get_Voice_Type_Only(string[] Voices)
    {
        List<string> Type_List = new List<string>();
        foreach (string Type in Voices)
        {
            string Type_Name = Get_Voice_Type_V1(Type);
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
                Type_List.Add(Type_Name);
        }
        return Type_List.ToArray();
    }
}