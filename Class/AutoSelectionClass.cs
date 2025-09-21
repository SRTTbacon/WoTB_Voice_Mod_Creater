using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;

namespace WoTB_Voice_Mod_Creater.Class
{
    //右のリストに入れるクラス
    public class CAutoSelectionItem : INotifyPropertyChanged
    {
        private string _name = "";

        public string ItemName
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                NotifyPropertyChanged("ItemName");
            }
        }

        public string ButtonName => ShortID.ToString();
        public readonly uint ShortID = 0;

        public CAutoSelectionItem(string itemName, uint shortID)
        {
            _name = itemName;

            ShortID = shortID;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
                PropertyChanged(this, new PropertyChangedEventArgs("ItemName"));
            }
        }
    }

    //右のリストに入れるクラス
    public class CAutoSelectionEvent : INotifyPropertyChanged
    {
        private string _name = "";

        public string EventName => ItemList.Count > 0 ? _name + " | 追加済み" : _name + " | なし";

        public ObservableCollection<CAutoSelectionItem> ItemList = new ObservableCollection<CAutoSelectionItem>();

        public Brush TextColor => ItemList.Count > 0 ? Brushes.Aqua : purpleColor;

        public string ButtonName => ShortID.ToString();
        public readonly uint ShortID = 0;

        private Brush purpleColor = Brushes.Purple;

        public CAutoSelectionEvent(string eventName, uint shortID)
        {
            _name = eventName;

            ShortID = shortID;

            purpleColor = new System.Windows.Media.BrushConverter().ConvertFromString("#BFFF2C8C") as Brush;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("TextColor"));
                PropertyChanged(this, new PropertyChangedEventArgs("EventName"));
            }
        }

        public void UpdateTextColor()
        {
            NotifyPropertyChanged();
        }
    }
}
