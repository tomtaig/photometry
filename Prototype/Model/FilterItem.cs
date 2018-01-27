using System.ComponentModel;

namespace Prototype.Model
{
    public class FilterItem : INotifyPropertyChanged
    {
        public short Number { get; set; }
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
