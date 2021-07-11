using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace WoTB_Voice_Mod_Creater.Class
{
    public partial class ChangeLog : UserControl
    {
        bool IsClosing = false;
        public ChangeLog()
        {
            InitializeComponent();
        }
        public async void Window_Show()
        {
            Opacity = 0;
            Visibility = Visibility.Visible;
            while (Opacity < 1 && !IsClosing)
            {
                Opacity += Sub_Code.Window_Feed_Time;
                await Task.Delay(1000 / 60);
            }
        }
        public void Text_Change(string[] Text)
        {
            Message_T.Document.Blocks.Clear();
            foreach (string Line in Text)
            {
                Run myRun = new Run(Line);
                Paragraph myParagraph = new Paragraph();
                myParagraph.Inlines.Add(myRun);
                Message_T.Document.Blocks.Add(myParagraph);
            }
        }
        private async void Exit_B_Click(object sender, RoutedEventArgs e)
        {
            if (!IsClosing)
            {
                IsClosing = true;
                while (Opacity > 0)
                {
                    Opacity -= Sub_Code.Window_Feed_Time;
                    await Task.Delay(1000 / 60);
                }
                IsClosing = false;
                Visibility = Visibility.Hidden;
            }
        }
    }
}