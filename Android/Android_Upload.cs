using PortableDevices;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace WoTB_Voice_Mod_Creater.Android
{
    public class Android_Class
    {
        bool IsDeviceExsist = false;
        PortableDeviceCollection Devices;
        PortableDevice Device;
        public Android_Class()
        {
            try
            {
                Init();
                IsDeviceExsist = true;
            }
            catch
            {
                IsDeviceExsist = false;
            }
        }
        void Init()
        {
            Devices = new PortableDeviceCollection();
            if (null == Devices)
            {
                throw new Exception("デバイスを取得できませんでした。");
            }
            else
            {
                Devices.Refresh();
                Device = Devices.First();
            }
            //devices.Clear();
        }
        //To_Fileは"/Android/data"や"/Download/"などから始める
        public void Upload_File(string From_File, string To_File)
        {
            if (!IsDeviceExsist || !File.Exists(From_File))
            {
                return;
            }
            try
            {
                string phoneDir = "内部ストレージ" + To_File;
                PortableDeviceFolder root = Device.Root();
                foreach (PortableDeviceObject Name_Now in root.Files)
                {
                    MessageBox.Show(Name_Now.Name);
                }
                PortableDeviceFolder result = root.FindDir(phoneDir);
                if (null == result)
                {
                    result = Device.Root().FindDir("Tablet" + To_File);
                    phoneDir = "Tablet" + To_File;
                    MessageBox.Show("");
                }
                if (null == result)
                {
                    return;
                }
                Device.TransferContentToDevice(result, From_File);
            }
            catch (Exception ex)
            {
                Sub_Code.Error_Log_Write(ex.Message);
            }
        }
    }
}