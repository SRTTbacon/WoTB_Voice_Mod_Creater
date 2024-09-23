using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Compression;
using System.IO;
using System.Windows.Media;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Input;

namespace WoTB_Voice_Mod_Creater.Class
{
    //右のModファイルリストに入れるクラス
    public class CModFileList : INotifyPropertyChanged
    {
        private bool _bIsDirectory;
        private bool _bIsZipFile;
        private bool _bIsChildEnable = true;
        private bool _bIsAnyDir = false;
        private int _childCount = 0;
        private CModFileList _parent = null;

        private ICommand _clickCommand1;
        private ICommand _clickCommand2;
        public ICommand ClickCommand1
        {
            get
            {
                return _clickCommand1 ?? (_clickCommand1 = new CommandHandler(() => OpenFromDirectory(false), () => true));
            }

        }
        public ICommand ClickCommand2
        {
            get
            {
                return _clickCommand2 ?? (_clickCommand2 = new CommandHandler(() => OpenFromDirectory(true), () => true));
            }

        }
        public CModFileList Parent
        {
            get { return _parent; }
            set
            {
                _parent = value;
                NotifyPropertyChanged("TextColor");
            }
        }
        public bool IsDirectory
        {
            get { return _bIsDirectory; }
            set
            {
                _bIsDirectory = value;
                NotifyPropertyChanged("TextColor");
            }
        }
        public bool IsZipFile
        {
            get { return _bIsZipFile; }
            set
            {
                _bIsZipFile = value;
                NotifyPropertyChanged("TextColor");
            }
        }
        public bool IsChildEnable
        {
            get { return _bIsChildEnable; }
            set
            {
                _bIsChildEnable = value;
                NotifyPropertyChanged("TextColor");
            }
        }
        public int ChildCount
        {
            get { return _childCount; }
            set
            {
                _childCount = value;
                NotifyPropertyChanged("Text");
            }
        }
        public string Text
        {
            get
            {
                if (Parent == null)
                    return FilePath + " - ファイル数:" + ChildCount;
                else
                    return "-" + FilePath.Replace("\\", "/");
            }
        }
        public bool IsAnyDir
        {
            get { return _bIsAnyDir; }
            set
            {
                _bIsAnyDir = value;
                NotifyPropertyChanged("IsShow");
                NotifyPropertyChanged("Column");
            }
        }

        public Brush TextColor => IsEnable ? Brushes.Aqua : (Brush)new BrushConverter().ConvertFromString("#BFFF2C8C");
        public Visibility IsShow => IsAnyDir ? Visibility.Visible : Visibility.Collapsed;
        public string NameOnly = "";
        public string FilePath = "";
        public string UserDir = "";
        public string ButtonName => FileID.ToString();
        public uint FileID = 0;
        public int WoTBDirIndex = -1;
        public int Column => IsAnyDir ? 2 : 0;
        public bool IsEnable => Parent != null && IsChildEnable && _parent.IsChildEnable;
        public bool IsCanDelete = true;

        public CModFileList(bool bDirectoryMode, bool bZipFile, CModFileList parent, string childPath, bool bIsCanDelete = true)
        {
            IsDirectory = bDirectoryMode;
            IsZipFile = bZipFile;
            Parent = parent;
            if (Parent != null)
                childPath = childPath.Replace("\\", "/");
            FilePath = childPath;
            NameOnly = Path.GetFileName(FilePath).Replace(".dvpl", "");
            IsCanDelete = bIsCanDelete;
            SetDefaultDir();
        }
        public CModFileList(ZipArchiveEntry zipEntry, CModFileList parent)
        {
            IsDirectory = false;
            IsZipFile = true;
            Parent = parent;
            if (Parent == null)
                FilePath = zipEntry.FullName;
            else
                FilePath = zipEntry.FullName.Replace("\\", "/");
            NameOnly = Path.GetFileName(FilePath).Replace(".dvpl", "");
            IsCanDelete = true;
            SetDefaultDir();
        }

        public void UpdateColor()
        {
            NotifyPropertyChanged("TextColor");
        }

        //voiceover_crewは初期化の時点でフォルダを決定
        private void SetDefaultDir()
        {
            if (Parent != null && NameOnly.Contains("voiceover_crew.bnk"))
            {
                List<string> dirs = ModManager.WoTBFiles[NameOnly];
                for (int i = 0; i < dirs.Count; i++)
                {
                    if (dirs[i].EndsWith("ja"))
                    {
                        WoTBDirIndex = i;
                        break;
                    }
                }
            }
        }

        public string GetDirectoryORFile(bool bDestMode)
        {
            if (bDestMode)
            {
                if (Parent == null)
                    return "";

                string dir;
                if (UserDir != "")
                    dir = Voice_Set.WoTB_Path + UserDir.Replace("/", "\\");
                else if (ModManager.WoTBFiles[NameOnly].Count <= 0)
                    return "";
                else if (WoTBDirIndex != -1 && ModManager.WoTBFiles[NameOnly].Count > WoTBDirIndex)
                    dir = Voice_Set.WoTB_Path + ModManager.WoTBFiles[NameOnly][WoTBDirIndex].Replace("/", "\\");
                else
                    dir = Voice_Set.WoTB_Path + ModManager.WoTBFiles[NameOnly][0].Replace("/", "\\");
                string filePath = dir + "\\" + NameOnly;
                if (File.Exists(filePath + ".dvpl"))
                    return filePath + ".dvpl";
                return filePath;
            }
            else
            {
                if (Parent == null && IsDirectory)
                    return FilePath + "\\";
                else if (Parent == null && IsZipFile)
                    return FilePath;
                else if (Parent.IsZipFile)
                    return Parent.FilePath;
                else if (FilePath.Contains("/"))
                    return Parent.FilePath + FilePath.Replace("/", "\\");
                else
                    return Parent.FilePath + "\\" + FilePath;
            }
        }

        public void OpenFromDirectory(bool bDestMode)
        {
            string temp = GetDirectoryORFile(bDestMode);
            if (temp != "")
                OpenDirectory(temp);
        }

        private void OpenDirectory(string file)
        {
            if (Sub_Code.ShowExplorerFileAndSelect(file))
                return;
            System.Diagnostics.Process.Start("Explorer.exe", "/select,\"" + file + "\"");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
                PropertyChanged(this, new PropertyChangedEventArgs("TextColor"));
                PropertyChanged(this, new PropertyChangedEventArgs("Text"));
                PropertyChanged(this, new PropertyChangedEventArgs("IsShow"));
                PropertyChanged(this, new PropertyChangedEventArgs("Column"));
            }
        }
    }

    //左のModリストに入れるクラス
    public class CModTypeList : INotifyPropertyChanged
    {
        public const int MAXCOUNT = 1000;

        private string _name = "";
        private List<string> textFiles = new List<string>();

        public string ModName
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                NotifyPropertyChanged("ModName");
            }
        }

        public ObservableCollection<CModFileList> fileList = new ObservableCollection<CModFileList>();

        public string ButtonName => ModID.ToString();
        public uint ModID = 0;

        public readonly bool IsCanDelete = true;

        public CModTypeList(string modName, bool bCanDelete = true)
        {
            _name = modName;

            ModID = WwiseHash.HashString(modName);
            IsCanDelete = bCanDelete;
        }

        public int GetDirIndex(string dir)
        {
            for (int i = 0; i < fileList.Count; i++)
                if (fileList[i].IsDirectory && fileList[i].FilePath == dir)
                    return i;
            return -1;
        }
        public bool IsExistDir(string dir)
        {
            return GetDirIndex(dir) != -1;
        }

        public int GetZipIndex(string zipFile)
        {
            for (int i = 0; i < fileList.Count; i++)
                if (fileList[i].IsZipFile && fileList[i].FilePath == zipFile)
                    return i;
            return -1;
        }
        public bool IsExistZip(string zipFile)
        {
            return GetZipIndex(zipFile) != -1;
        }

        public bool IsExistFile(string filePath)
        {
            string dir = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileName(filePath);

            foreach (CModFileList file in fileList)
            {
                if (file.Parent != null && file.Parent.FilePath == dir && file.FilePath == fileName)
                    return true;
                else if (file.Parent != null && dir.Contains(file.Parent.FilePath) && file.FilePath == filePath.Replace(file.Parent.FilePath, ""))
                    return true;
            }
            return false;
        }

        public bool IsMaxSize()
        {
            return fileList.Count > MAXCOUNT;
        }

        void AddTextFiles(string dir)
        {
            if (!Sub_Code.CanDirectoryAccess(dir))
                return;
            foreach (string subDir in Directory.GetDirectories(dir))
                AddTextFiles(subDir);
            foreach (string file in Directory.GetFiles(dir))
                if (Path.GetExtension(file) == ".txt" && !ModManager.WoTBFiles.ContainsKey(Path.GetFileName(file)))
                    textFiles.Add(file);
        }

        //フォルダ内のファイルを追加
        //戻り値 : 追加したファイル数
        public int AddDirectory(string dir, bool bIncludeChildDir, bool bIsCanDelete = true, string parentDir = null)
        {
            if (!Directory.Exists(dir))
                return 0;
            if (IsMaxSize())
                return 0;

            bool bChildDirMode = parentDir != null;

            if (!bChildDirMode)
            {
                textFiles.Clear();
                AddTextFiles(dir);
            }

            int addedCount = 0;

            int parentIndex;

            //既にリスト内にフォルダがあればそこに追加する
            if (!bChildDirMode && !IsExistDir(dir))
            {
                CModFileList file = new CModFileList(true, false, null, dir, IsCanDelete);
                file.FileID = GetRandomID(dir);
                fileList.Add(file);
            }

            //子フォルダを解析
            if (bIncludeChildDir)
            {
                foreach (string childDir in Directory.GetDirectories(dir))
                {
                    if (bChildDirMode)
                        addedCount += AddDirectory(childDir, true, bIsCanDelete, parentDir);
                    else
                        addedCount += AddDirectory(childDir, true, bIsCanDelete, dir);
                }
            }

            if (IsMaxSize())
                return addedCount;

            if (bChildDirMode)
                parentIndex = GetDirIndex(parentDir);
            else
                parentIndex = GetDirIndex(dir);

            foreach (string file in Directory.GetFiles(dir))
            {
                int insertIndex = parentIndex + fileList[parentIndex].ChildCount + 1;
                string fileName;
                if (!bChildDirMode)
                    fileName = Path.GetFileName(file);
                else
                    fileName = file.Replace(parentDir, "");
                bool bExistFile = false;
                for (int i = parentIndex + 1; i < insertIndex - 1; i++)
                {
                    if (fileList[i].FilePath == fileName)
                    {
                        bExistFile = true;
                        break;
                    }
                }
                if (!bExistFile)
                {
                    int count;
                    string nameOnly = Path.GetFileName(file).Replace(".dvpl", "");
                    if (ModManager.WoTBFiles.ContainsKey(nameOnly))
                        count = ModManager.WoTBFiles[nameOnly].Count;
                    else if (Path.GetExtension(file) == ".dvpl")
                        count = 0;
                    else
                        continue;
                    CModFileList newFile = new CModFileList(false, false, fileList[parentIndex], fileName, bIsCanDelete);
                    fileList.Insert(insertIndex, newFile);
                    newFile.IsAnyDir = count > 1 || count == 0;       //このファイル名が存在するフォルダが1つじゃない場合またはオリジナルのファイル名の場合true
                    newFile.Parent.ChildCount++;
                    newFile.FileID = GetRandomID(fileName);
                    if (count > 0)
                    {
                        Mod_Dest_Analyze.SetDirIndex(newFile);
                        foreach (string textFile in textFiles)
                            Mod_Dest_Analyze.SetDirIndex(newFile, textFile);
                    }
                    addedCount++;

                    if (fileList.Count > MAXCOUNT)
                        return addedCount;
                }
            }

            //1つもファイルが追加出来なかった場合親を削除
            for (int i = 0; i < fileList.Count; i++)
            {
                CModFileList file = fileList[i];
                if (file.Parent == null && file.ChildCount <= 0)
                {
                    fileList.RemoveAt(i);
                    i--;
                }
            }

            return addedCount;
        }

        //ファイルを追加
        public bool AddFile(string filePath)
        {
            if (!File.Exists(filePath))
                return false;

            if (fileList.Count > MAXCOUNT) 
                return false;

            int count;
            string nameOnly = Path.GetFileName(filePath).Replace(".dvpl", "");
            if (ModManager.WoTBFiles.ContainsKey(nameOnly))
                count = ModManager.WoTBFiles[nameOnly].Count;
            else if (Path.GetExtension(filePath) == ".dvpl")
                count = 0;
            else
                return false;

            string dir = Path.GetDirectoryName(filePath);

            //既に追加されていれば処理しない
            if (IsExistFile(filePath))
                return false;

            //既にフォルダが存在する場合それに追加
            CModFileList existDir = null;
            foreach (CModFileList file in fileList)
            {
                if (file.IsDirectory && dir.Contains(file.FilePath))
                {
                    existDir = file;
                    break;
                }
            }

            string fileName;
            int parentIndex;

            if (existDir != null)
            {
                parentIndex = fileList.IndexOf(existDir);
                fileName = filePath.Replace(existDir.FilePath, "");
            }
            else
            {
                fileName = Path.GetFileName(filePath);
                //親フォルダがなければ追加
                if (!IsExistDir(dir))
                {
                    CModFileList file = new CModFileList(true, false, null, dir);
                    file.FileID = GetRandomID(dir);
                    fileList.Add(file);
                }

                parentIndex = GetDirIndex(dir);
            }

            int insertIndex = parentIndex + fileList[parentIndex].ChildCount + 1;

            //ファイルを追加
            CModFileList newFile = new CModFileList(false, false, fileList[parentIndex], fileName);
            newFile.FileID = GetRandomID(fileName);
            newFile.IsAnyDir = count > 1 || count == 0;
            if (count > 0)
                Mod_Dest_Analyze.SetDirIndex(newFile);
            fileList.Insert(insertIndex, newFile);
            newFile.Parent.ChildCount++;

            return true;
        }

        public bool AddZip(string filePath)
        {
            if (!File.Exists(filePath))
                return false;

            foreach (CModFileList file in fileList)
                if (file.IsZipFile && file.FilePath == filePath)
                    return false;

            textFiles.Clear();

            try
            {
                string tempDir = Voice_Set.Special_Path + "\\Other";

                bool bParentAdded = false;

                //テキストファイルを抽出
                using (ZipArchive archive = ZipFile.OpenRead(filePath))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (!entry.FullName.EndsWith("/"))
                        {
                            string nameOnly = Path.GetFileName(entry.FullName);
                            if (Path.GetExtension(entry.FullName) != ".txt" || ModManager.WoTBFiles.ContainsKey(nameOnly))
                                continue;

                            string tempFile = tempDir + "\\" + Sub_Code.r.Next(100000, 1000000) + ".txt";
                            entry.ExtractToFile(tempFile);
                            textFiles.Add(tempFile);
                        }
                    }
                }

                //Zipファイル内のファイルをリストに追加
                using (ZipArchive archive = ZipFile.OpenRead(filePath))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (!entry.FullName.EndsWith("/"))
                        {
                            int count;
                            string nameOnly = Path.GetFileName(entry.FullName).Replace(".dvpl", "");
                            if (ModManager.WoTBFiles.ContainsKey(nameOnly))
                                count = ModManager.WoTBFiles[nameOnly].Count;
                            else if (Path.GetExtension(entry.FullName) == ".dvpl")
                                count = 0;
                            else
                                continue;

                            CModFileList child;
                            if (!bParentAdded)
                            {
                                CModFileList parent = new CModFileList(false, true, null, filePath);
                                child = new CModFileList(entry, parent);
                                fileList.Add(parent);
                                fileList.Add(child);
                                parent.ChildCount++;
                                bParentAdded = true;
                            }
                            else
                            {
                                int parentIndex = GetZipIndex(filePath);
                                CModFileList parent = fileList[parentIndex];
                                child = new CModFileList(entry, parent);
                                parent.ChildCount++;
                                fileList.Insert(parentIndex + parent.ChildCount, child);
                            }

                            child.FileID = GetRandomID(entry.FullName);
                            child.IsAnyDir = count > 1 || count == 0;
                            if (count > 0)
                            {
                                Mod_Dest_Analyze.SetDirIndex(child);
                                foreach (string tempFile in textFiles)
                                    Mod_Dest_Analyze.SetDirIndex(child, tempFile);
                            }

                            if (fileList.Count > MAXCOUNT)
                                return true;
                        }
                    }
                }

                foreach (string tempFile in textFiles)
                    File.Delete(tempFile);

                //1つもファイルが追加出来なかった場合親を削除
                for (int i = 0; i < fileList.Count; i++)
                {
                    CModFileList file = fileList[i];
                    if (file.Parent == null && file.ChildCount <= 0)
                    {
                        fileList.RemoveAt(i);
                        i--;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                return false;
            }
        }

        //絶対に被らないIDかつ、可能であればファイル名から特定できるIDを生成
        uint GetRandomID(string name)
        {
            uint id = WwiseHash.HashString(name);
            while(true)
            {
                bool bExist = false;
                foreach (CModFileList file in fileList)
                {
                    if (file.FileID == id)
                    {
                        bExist = true;
                        break;
                    }
                }
                if (!bExist)
                    return id;
                id = WwiseHash.HashString(name + Sub_Code.r.Next(0, 100000));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
                PropertyChanged(this, new PropertyChangedEventArgs("ModName"));
            }
        }
    }

    //ModManager_Destinationのリストに使うクラス
    public class CModDestList : INotifyPropertyChanged
    {
        private bool _bIsUserAddDir = false;

        public string Text
        {
            get
            {
                return "[" + Index + "] - " + DestDir;
            }
        }
        public bool IsUserAddDir
        {
            get { return _bIsUserAddDir; }
            set
            {
                _bIsUserAddDir = value;
                NotifyPropertyChanged("TextColor");
            }
        }

        private ICommand _clickCommand;
        public ICommand ClickCommand
        {
            get
            {
                return _clickCommand ?? (_clickCommand = new CommandHandler(() => OpenFromDirectory(), () => true));
            }

        }

        public Brush TextColor => IsUserAddDir ? Brushes.Aqua : Brushes.White;
        public string DestDir = "";
        public int Index = -1;

        public CModDestList(string dir, int index)
        {
            DestDir = dir;
            Index = index;
        }

        void OpenFromDirectory()
        {
            string dir = Voice_Set.WoTB_Path + DestDir.Replace("/", "\\") + "\\";

            if (Sub_Code.ShowExplorerFileAndSelect(dir))
                return;
            System.Diagnostics.Process.Start("Explorer.exe", "/select,\"" + dir + "\"");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
                PropertyChanged(this, new PropertyChangedEventArgs("Text"));
                PropertyChanged(this, new PropertyChangedEventArgs("TextColor"));
            }
        }
    }

    public class Mod_Dest_Analyze
    {
        //フォルダ構造から導入先を特定
        public static void SetDirIndex(CModFileList file)
        {
            //フォルダが1つに決まっているか、既に解析済みの場合スキップ
            if (!file.IsAnyDir || file.WoTBDirIndex != -1 || file.Parent == null)
                return;

            List<string> dirSplit = new List<string>();
            if (file.Parent.IsZipFile)
                dirSplit.AddRange(Path.GetDirectoryName(file.FilePath).Replace("\\", "/").Split('/'));
            else
                dirSplit.AddRange(file.Parent.FilePath.Replace("\\", "/").Split('/'));

            //フォルダ構造を反転
            for (int i = 0; i < dirSplit.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(dirSplit[i]))
                {
                    dirSplit.RemoveAt(i);
                    i--;
                }
            }
            dirSplit.Reverse();

            List<string> dirList = ModManager.WoTBFiles[file.NameOnly];
            List<List<string>> wotbDirSplits = new List<List<string>>();
            List<bool> wotbDirEnable = new List<bool>();
            for (int i = 0; i < dirList.Count; i++)
            {
                List<string> wotbDirSplit = new List<string>(dirList[i].Split('/'));
                //フォルダ構造を反転
                for (int j = 0; j < wotbDirSplit.Count; j++)
                {
                    if (string.IsNullOrWhiteSpace(wotbDirSplit[j]))
                    {
                        wotbDirSplit.RemoveAt(j);
                        j--;
                    }
                }
                wotbDirSplit.Reverse();
                wotbDirSplits.Add(wotbDirSplit);
                wotbDirEnable.Add(true);
            }

            int nowIndex = -1;  //有力候補
            for (int i = 0; i < dirSplit.Count; i++)
            {
                bool bExist = false;
                int bOKCount = 0;
                for (int j = 0; j < wotbDirSplits.Count; j++)
                {
                    if (wotbDirEnable[j] && wotbDirSplits[j].Count > i && dirSplit[i] == wotbDirSplits[j][i])
                    {
                        nowIndex = j;
                        bExist = true;
                        bOKCount++;
                    }
                    else
                        wotbDirEnable[j] = false;
                }
                if (!bExist || bOKCount <= 1)
                    break;
            }

            file.WoTBDirIndex = nowIndex;
        }

        //テキストファイルから導入先を特定
        public static void SetDirIndex(CModFileList file, string textFile)
        {
            if (!File.Exists(textFile) || !file.IsAnyDir || file.WoTBDirIndex != -1 || file.Parent == null)
                return;

            StreamReader reader = null;
            try
            {
                reader = new StreamReader(textFile, System.Text.Encoding.UTF8);
                List<List<string>> dirs = new List<List<string>>();
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Replace("\\", "/");
                    if (line.Contains("/"))
                    {
                        int index = line.IndexOf("/");
                        string first = line.Substring(0, index);
                        string last = line.Substring(index + 1);
                        CutString(ref first, true);
                        CutString(ref last, false);
                        line = first + last;
                        List<string> split = new List<string>(line.Split('/'));
                        //フォルダ構造を反転
                        for (int i = 0; i < split.Count; i++)
                        {
                            if (string.IsNullOrWhiteSpace(split[i]))
                            {
                                split.RemoveAt(i);
                                i--;
                            }
                        }
                        split.Reverse();
                        dirs.Add(split);
                    }
                }
                reader.Close();

                //WoTBのフォルダリスト
                List<string> dirList = ModManager.WoTBFiles[file.NameOnly];
                List<List<string>> wotbDirSplits = new List<List<string>>();
                for (int i = 0; i < dirList.Count; i++)
                {
                    List<string> wotbDirSplit = new List<string>(dirList[i].Split('/'));
                    //フォルダ構造を反転
                    for (int j = 0; j < wotbDirSplit.Count; j++)
                    {
                        if (string.IsNullOrWhiteSpace(wotbDirSplit[j]))
                        {
                            wotbDirSplit.RemoveAt(j);
                            j--;
                        }
                    }
                    wotbDirSplit.Reverse();
                    wotbDirSplits.Add(wotbDirSplit);
                }

                int nowEqualCount = 0;
                int lastEqualIndex = -1;
                foreach (List<string> dir in dirs)
                {
                    for (int i = 0; i < wotbDirSplits.Count; i++)
                    {
                        int count = 0;
                        for (int j = 0; j < dir.Count; j++)
                        {
                            if (wotbDirSplits[i].Count > j && dir[j] == wotbDirSplits[i][j])
                                count++;
                        }
                        if (nowEqualCount < count)
                        {
                            nowEqualCount = count;
                            lastEqualIndex = i;
                        }
                    }
                }
                file.WoTBDirIndex = lastEqualIndex;
            }
            catch
            {
                if (reader != null)
                    reader.Close();
            }
        }

        static void CutString(ref string text, bool bFirst)
        {
            int cutIndex = -1;
            if (text.IndexOf("\"") != -1)
                cutIndex = text.IndexOf("\"");
            else if (text.IndexOf("'") != -1)
                cutIndex = text.IndexOf("'");
            else if (text.IndexOf(" ") != -1)
                cutIndex = text.IndexOf(" ");
            else if (text.IndexOf("　") != -1)
                cutIndex = text.IndexOf("　");
            if (cutIndex != -1)
            {
                if (bFirst)
                    text = text.Substring(cutIndex + 1);
                else
                    text = text.Substring(0, cutIndex);
            }
        }
    }

    public class ModFromTo
    {
        public string FromFile;
        public string ToFile;
        public bool IsDeleteMode;

        public ModFromTo(string fromFile, string toFile, bool bDelete)
        {
            FromFile = fromFile;
            ToFile = toFile;
            IsDeleteMode = bDelete;
        }
    }

    public class CommandHandler : ICommand
    {
        private Action _action;
        private Func<bool> _canExecute;

        public CommandHandler(Action action, Func<bool> canExecute)
        {
            _action = action;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute.Invoke();
        }

        public void Execute(object parameter)
        {
            _action();
        }
    }
}
