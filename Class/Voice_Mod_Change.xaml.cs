using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace WoTB_Voice_Mod_Creater.Class
{
    public partial class Voice_Mod_Change : UserControl
    {
        bool IsBusy = false;
        bool IsMessageShowing = false;
        string Mod_Name;
        /*Cauldron.FMOD.EventProject EP = new Cauldron.FMOD.EventProject();
        Cauldron.FMOD.Event FE = new Cauldron.FMOD.Event();*/
        readonly List<string> Delete_Files = new List<string>();
        readonly List<string> Add_Files = new List<string>();
        public Voice_Mod_Change()
        {
            InitializeComponent();
        }
        //画面を表示
        public async void Window_Show(string Mod_Name)
        {
            Opacity = 0;
            Visibility = Visibility.Visible;
            Mod_File_L.Items.Clear();
            Delete_Files.Clear();
            Add_Files.Clear();
            //指定したModのファイルを参照
            foreach (string File_Now in Voice_Set.FTPClient.GetFiles("/WoTB_Voice_Mod/Mods/" + Mod_Name + "/Files", false, false))
                Mod_File_L.Items.Add(Path.GetFileName(File_Now));
            Mod_Name_T.Text = Mod_Name;
            try
            {
                //Modの情報を取得
                XDocument xml2 = XDocument.Load(Voice_Set.FTPClient.GetFileRead("/WoTB_Voice_Mod/Mods/" + Mod_Name + "/Configs.dat"));
                XElement item2 = xml2.Element("Mod_Upload_Config");
                if (bool.Parse(item2.Element("IsPassword").Value))
                {
                    Password_C.IsChecked = true;
                    Password_T.Visibility = Visibility.Visible;
                    Password_T.Text = item2.Element("Password").Value;
                }
                else
                {
                    Password_C.IsChecked = false;
                    Password_T.Visibility = Visibility.Hidden;
                    Password_T.Text = "";
                }
                R_18_C.IsChecked = bool.Parse(item2.Element("IsEnableR18").Value);
                BGM_Mode_C.IsChecked = bool.Parse(item2.Element("IsBGMMode").Value);
                Mod_Explanation_T.Text = item2.Element("Explanation").Value;
            }
            catch (Exception e)
            {
                MessageBox.Show("Modの情報を取得できませんでした。");
                Sub_Code.Error_Log_Write(e.Message);
            }
            while (Opacity < 1 && !IsBusy)
            {
                Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
            this.Mod_Name = Mod_Name;
        }
        //時間が経つと文字をフェードアウトさせる
        async void Message_Feed_Out(string Message)
        {
            //テキストが一定期間経ったらフェードアウト
            if (IsMessageShowing)
            {
                IsMessageShowing = false;
                await Task.Delay(1000 / 59);
            }
            Message_T.Text = Message;
            IsMessageShowing = true;
            Message_T.Opacity = 1;
            int Number = 0;
            while (Message_T.Opacity > 0 && IsMessageShowing)
            {
                Number++;
                if (Number >= 120)
                {
                    Message_T.Opacity -= 0.025;
                }
                await Task.Delay(1000 / 60);
            }
            IsMessageShowing = false;
            Message_T.Text = "";
            Message_T.Opacity = 1;
        }
        //ファイルのリストの空きスペースをクリックするとインデックスを-1にする
        private void Mod_File_L_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Mod_File_L.SelectedIndex = -1;
        }
        //ファイルを削除
        private void Mod_File_Delete_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
            if (Mod_File_L.SelectedIndex != -1)
            {
                int Index = Mod_File_L.SelectedIndex;
                Mod_File_L.SelectedIndex = -1;
                bool IsDeleteExist = false;
                bool IsAddExist = false;
                //既にファイル削除に指定していた場合は追加しない
                foreach (string Delete in Delete_Files)
                {
                    if (Delete == Mod_File_L.Items[Index].ToString())
                    {
                        IsDeleteExist = true;
                        break;
                    }
                }
                int Number_Add = 0;
                //ファイル追加に指定しているファイル名だった場合それも削除
                foreach (string Add in Add_Files)
                {
                    if (Path.GetFileName(Add) == Mod_File_L.Items[Index].ToString())
                    {
                        IsAddExist = true;
                        break;
                    }
                    Number_Add++;
                }
                if (!IsDeleteExist)
                {
                    Delete_Files.Add(Mod_File_L.Items[Index].ToString());
                }
                if (IsAddExist)
                {
                    Add_Files.RemoveAt(Number_Add);
                }
                Mod_File_L.Items.RemoveAt(Index);
            }
        }
        //Modファイルの追加
        private void Mod_File_Add_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog()
            {
                Title = "Modファイルを選択してください。",
                Multiselect = true,
                Filter = "Modファイル(*.bnk;*.bnk.dvpl)|*.bnk;*.bnk.dvpl"
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string Error_Files = "";
                int Error_Number = 0;
                foreach (string SelectFile in ofd.FileNames)
                {
                    bool IsExist = false;
                    //既に同じファイル名が存在した場合エラーを表示(ファイル名を表示させる)
                    foreach (string FileExist in Mod_File_L.Items)
                    {
                        if (Path.GetFileName(SelectFile) == FileExist)
                        {
                            IsExist = true;
                            Error_Files += "\n" + Path.GetFileName(SelectFile);
                            Error_Number++;
                            break;
                        }
                    }
                    if (!IsExist)
                    {
                        bool IsDeleteExist = false;
                        bool IsAddExist = false;
                        int Number_Delete = 0;
                        //ファイル削除に指定しているファイル名だった場合削除を取り消す
                        foreach (string Delete in Delete_Files)
                        {
                            if (Delete == Path.GetFileName(SelectFile))
                            {
                                IsDeleteExist = true;
                                break;
                            }
                            Number_Delete++;
                        }
                        //ファイル追加に既に指定しているファイル名だった場合Add_Filesには追加しない(はたしてこれは必要なの...?)
                        foreach (string Add in Add_Files)
                        {
                            if (Path.GetFileName(Add) == Path.GetFileName(SelectFile))
                            {
                                IsAddExist = true;
                                break;
                            }
                        }
                        if (IsDeleteExist)
                        {
                            Delete_Files.RemoveAt(Number_Delete);
                        }
                        if (!IsAddExist)
                        {
                            Add_Files.Add(SelectFile);
                        }
                        Mod_File_L.Items.Add(Path.GetFileName(SelectFile));
                    }
                }
                if (Error_Files != "")
                {
                    MessageBox.Show("以下のファイル名は追加されませんでした。すでに同じファイル名が存在します。" + Error_Files);
                    Message_Feed_Out(Error_Number + "つのファイルが追加されませんでした。");
                }
            }
        }
        //パスワードにチェックが入った場合テキストボックスを表示(逆も同じく)
        private void Password_C_Click(object sender, RoutedEventArgs e)
        {
            if (Password_C.IsChecked.Value)
                Password_T.Visibility = Visibility.Visible;
            else
                Password_T.Visibility = Visibility.Hidden;
        }
        //変更しないで閉じる
        private void Exit_B_Click(object sender, RoutedEventArgs e)
        {
            Window_Close();
        }
        //閉じる
        async void Window_Close()
        {
            if (!IsBusy)
            {
                IsBusy = true;
                while (Opacity > 0)
                {
                    Opacity -= Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
                IsBusy = false;
                Visibility = Visibility.Hidden;
            }
        }
        //Mod情報を保存
        private async void Save_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }
            if (Mod_File_L.Items.Count == 0)
            {
                Message_Feed_Out("最低1つはファイルが必要です。");
                return;
            }
            if (Mod_Name_T.Text == "")
            {
                Message_Feed_Out("Mod名が指定されていません。");
                return;
            }
            try
            {
                Directory.CreateDirectory(Voice_Set.Special_Path + "/" + Mod_Name_T.Text);
                Directory.Delete(Voice_Set.Special_Path + "/" + Mod_Name_T.Text);
                if (Mod_Name_T.Text.Contains("/"))
                {
                    Message_Feed_Out("Mod名に不適切な文字が含まれています。");
                    return;
                }
            }
            catch (Exception e1)
            {
                Message_Feed_Out("Mod名に不適切な文字が含まれています。");
                Sub_Code.Error_Log_Write(e1.Message);
                return;
            }
            if (Mod_Name_T.Text.CountOf("  ") > 0)
            {
                Message_Feed_Out("Mod名に空白を2つ続けて付けることはできません。");
                return;
            }
            if (Mod_Name_T.Text == "Backup")
            {
                Message_Feed_Out("そのMod名は別の目的に使用されています。");
                return;
            }
            if (Voice_Set.FTPClient.Directory_Exist("/WoTB_Voice_Mod/Mods/" + Mod_Name_T.Text) && Mod_Name != Mod_Name_T.Text)
            {
                Message_Feed_Out("同名のModが既に存在します。");
                return;
            }
            bool IsError = false;
            foreach (string Add_File in Add_Files)
            {
                if (!File.Exists(Add_File))
                {
                    IsError = true;
                    MessageBox.Show("次のファイルが見つかりません。削除したか移動している可能性があります。\n" + Add_File);
                }
            }
            if (IsError)
            {
                Message_Feed_Out("エラー:指定されたファイルが存在しません。");
                return;
            }
            IsBusy = true;
            Message_T.Text = "情報を保存しています...";
            await Task.Delay(50);
            try
            {
                //Modの情報をXMLファイルに書き込む
                XDocument xml = new XDocument();
                XElement datas = new XElement("Mod_Upload_Config",
                new XElement("IsBGMMode", BGM_Mode_C.IsChecked.Value),
                new XElement("IsPassword", Password_C.IsChecked.Value),
                new XElement("IsEnableR18", R_18_C.IsChecked.Value),
                new XElement("UserName", Voice_Set.UserName),
                new XElement("Explanation", Mod_Explanation_T.Text),
                new XElement("Password", Password_T.Text));
                xml.Add(datas);
                xml.Save(Voice_Set.Special_Path + "/Temp_Create_Mod.dat");
                //Mod情報をアップロード
                Voice_Set.FTPClient.UploadFile(Voice_Set.Special_Path + "/Temp_Create_Mod.dat", "/WoTB_Voice_Mod/Mods/" + Mod_Name + "/Configs.dat");
                File.Delete(Voice_Set.Special_Path + "/Temp_Create_Mod.dat");
                foreach (string Delete_File in Delete_Files)
                    Voice_Set.FTPClient.DeleteFile("/WoTB_Voice_Mod/Mods/" + Mod_Name + "/Files/" + Delete_File);
                foreach (string Add_File in Add_Files)
                    Voice_Set.FTPClient.UploadFile(Add_File, "/WoTB_Voice_Mod/Mods/" + Mod_Name + "/Files/" + Path.GetFileName(Add_File));
                if (Mod_Name != Mod_Name_T.Text)
                {
                    Voice_Set.FTPClient.Directory_Move("/WoTB_Voice_Mod/Mods/" + Mod_Name, "/WoTB_Voice_Mod/Mods/" + Mod_Name_T.Text, true);
                    StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Temp_Mod_Names.dat");
                    StreamReader str = Voice_Set.FTPClient.GetFileRead("/WoTB_Voice_Mod/Mods/Mod_Names.dat");
                    string line;
                    while ((line = str.ReadLine()) != null)
                    {
                        if (line == Mod_Name)
                        {
                            stw.WriteLine(Mod_Name_T.Text);
                            continue;
                        }
                        stw.WriteLine(line);
                    }
                    str.Close();
                    stw.Close();
                    Voice_Set.FTPClient.UploadFile(Voice_Set.Special_Path + "/Temp_Mod_Names.dat", "/WoTB_Voice_Mod/Mods/Mod_Names.dat");
                    File.Delete(Voice_Set.Special_Path + "/Temp_Mod_Names.dat");
                    Voice_Set.TCP_Server.Send("Message|" + Voice_Set.UserName + "->配布Mod:" + Mod_Name + "を変更しました。変更後:" + Mod_Name_T.Text);
                }
                else
                    Voice_Set.TCP_Server.Send("Message|" + Voice_Set.UserName + "->配布Mod:" + Mod_Name + "を変更しました。");
                Message_Feed_Out("変更を保存しました。");
            }
            catch (Exception e1)
            {
                Message_Feed_Out("正常に保存できませんでした。");
                Sub_Code.Error_Log_Write(e1.Message);
            }
            IsBusy = false;
            Sub_Code.ModChange = true;
            Window_Close();
        }
        //プロジェクトを削除(サーバーから削除)
        private void Delete_B_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy)
                return;
            MessageBoxResult result = MessageBox.Show("このプロジェクトを削除しますか？この操作は取り消せません。", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                IsBusy = true;
                try
                {
                    StreamWriter stw = File.CreateText(Voice_Set.Special_Path + "/Temp_Mod_Names.dat");
                    StreamReader str = Voice_Set.FTPClient.GetFileRead("/WoTB_Voice_Mod/Mods/Mod_Names.dat");
                    string line;
                    while ((line = str.ReadLine()) != null)
                        if (line != Mod_Name)
                            stw.WriteLine(line);
                    str.Close();
                    stw.Close();
                    Voice_Set.FTPClient.UploadFile(Voice_Set.Special_Path + "/Temp_Mod_Names.dat", "/WoTB_Voice_Mod/Mods/Mod_Names.dat");
                    File.Delete(Voice_Set.Special_Path + "/Temp_Mod_Names.dat");
                    Voice_Set.FTPClient.Directory_Delete("/WoTB_Voice_Mod/Mods/" + Mod_Name);
                    Sub_Code.ModChange = true;
                    Voice_Set.TCP_Server.Send("Message|" + Voice_Set.UserName + "->配布Mod:" + Mod_Name + "を削除しました。");
                    Message_Feed_Out("正常に削除しました。");
                }
                catch (Exception e1)
                {
                    Message_Feed_Out("正常に削除できませんでした。。");
                    Sub_Code.Error_Log_Write(e1.Message);
                }
                IsBusy = false;
                Sub_Code.ModChange = true;
                Window_Close();
            }
        }
    }
}