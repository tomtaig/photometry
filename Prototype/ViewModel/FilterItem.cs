using System.ComponentModel;

namespace Prototype.ViewModel
{
    public class FilterItem : INotifyPropertyChanged
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public string NewName { get; set; }

        public string DisplayName
        {
            get { return $"{Number}. {Name}".Trim(); }
        }

        public void Save()
        {
            Name = NewName;

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayName)));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
