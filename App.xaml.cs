using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace WoTB_Voice_Mod_Creater
{
    public partial class App : Application
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool SetDllDirectory(string lpPathName);
        private static readonly string mutexName = "SRTTbacon_WoTB_Voice_Mod_Creater";
        private static readonly Mutex mutex = new Mutex(false, mutexName);
        private static bool hasHandle = false;
        protected override void OnStartup(StartupEventArgs e)
        {
            hasHandle = mutex.WaitOne(0, false);
            if (!hasHandle)
            {
                MessageBox.Show("既にアプリが起動されています。");
                this.Shutdown();
                return;
            }
            base.OnStartup(e);
        }
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            if (hasHandle)
            {
                mutex.ReleaseMutex();
            }
            mutex.Close();
        }
        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            //dllの位置を変更
            string dllPath = System.IO.Path.Combine(System.IO.Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location).FullName, @"dll");
            SetDllDirectory(dllPath);
            MainCode windows = new MainCode();
            windows.Show();
        }
    }
}