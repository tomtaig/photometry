using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Prototype.Model
{
    public class FilterView : INotifyPropertyChanged
    {
        public FilterView()
        {
            Wheels = new ObservableCollection<FilterWheel>();
            Wheels.Add(new FilterWheel { Name = "ZWO Filter Wheel (0)" });
            Wheels.Add(new FilterWheel { Name = "ZWO Filter Wheel (1)" });

            Filters = new ObservableCollection<FilterItem>();
        }

        public FilterItem ActiveFilter { get; set; }
        public FilterItem SelectedFilter { get; set; }
        public FilterWheel SelectedWheel { get; set; }
        public ObservableCollection<FilterWheel> Wheels { get; set; }
        public ObservableCollection<FilterItem> Filters { get; set; }
        public bool IsFilterWheelMoving { get; set; }
        public bool IsConnected { get; set; }
        
        public Visibility ActiveFilterVisibility => !IsFilterWheelMoving && IsConnected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility MovingFilterVisibility => IsFilterWheelMoving && IsConnected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility FilterTabNoConnectVisibility => !IsConnected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ConnectVisibility => !IsConnected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility DisconnectVisibility => IsConnected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility FilterSettingsVisibility => IsConnected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility FilterTabInfoVisibility => IsConnected ? Visibility.Visible : Visibility.Collapsed;
        public bool IsFilterSelectionEnabled => IsConnected;
        public bool IsWheelSelectionEnabled => !IsConnected;
        public bool IsConnectEnabled => SelectedWheel != null;
        public string ActiveFilterName => ActiveFilter?.DisplayName;
        public string TabFilterSlot => ActiveFilter?.Number.ToString() ?? "-";
        public string FilterStatus => IsFilterWheelMoving ? "Moving.." : string.IsNullOrEmpty(ActiveFilter?.Name) ? string.Empty : $"({ActiveFilter.Name})";

        public event PropertyChangedEventHandler PropertyChanged;

        public void ConnectWheel()
        {
            if (SelectedWheel.Name == "ZWO Filter Wheel (0)")
            {
                Filters.Clear();
                Filters.Add(new FilterItem { Number = 1, Name = string.Empty });
                Filters.Add(new FilterItem { Number = 2, Name = string.Empty });
                Filters.Add(new FilterItem { Number = 3, Name = string.Empty });
                Filters.Add(new FilterItem { Number = 4, Name = string.Empty });
                Filters.Add(new FilterItem { Number = 5, Name = string.Empty });
                Filters.Add(new FilterItem { Number = 6, Name = string.Empty });
                Filters.Add(new FilterItem { Number = 7, Name = string.Empty });
                Filters.Add(new FilterItem { Number = 8, Name = string.Empty });
            }

            if (SelectedWheel.Name == "ZWO Filter Wheel (1)")
            {
                Filters.Clear();
                Filters.Add(new FilterItem { Number = 1, Name = string.Empty });
                Filters.Add(new FilterItem { Number = 2, Name = string.Empty });
                Filters.Add(new FilterItem { Number = 3, Name = string.Empty });
                Filters.Add(new FilterItem { Number = 4, Name = string.Empty });
            }

            foreach (var filter in Filters)
            {
                filter.NewName = filter.Name;
            }

            SelectedFilter = null;
            ActiveFilter = null;
            IsConnected = true;
            
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Filters)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsConnected)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveFilter)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedFilter)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsWheelSelectionEnabled)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsFilterSelectionEnabled)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(ConnectVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(DisconnectVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(FilterSettingsVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveFilterName)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(FilterTabInfoVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(FilterTabNoConnectVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(TabFilterSlot)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsConnectEnabled)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(FilterStatus)));
        }
        
        public void SetActiveFilter(int position)
        {
            ActiveFilter = Filters.First(x => x.Number == position);
            IsFilterWheelMoving = false;

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveFilterName)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveFilter)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(TabFilterSlot)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(MovingFilterVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveFilterVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(FilterStatus)));
        }
        
        public void SetFilterWheelMoving(bool moving)
        {
            IsFilterWheelMoving = moving;

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveFilterName)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveFilter)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(TabFilterSlot)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(MovingFilterVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveFilterVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(FilterStatus)));
        }

        public void DisconnectWheel()
        {
            Filters.Clear();

            SelectedFilter = null;
            ActiveFilter = null;
            IsConnected = false;

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Filters)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsConnected)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveFilter)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedFilter)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsWheelSelectionEnabled)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(ConnectVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(DisconnectVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(FilterSettingsVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsFilterSelectionEnabled)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveFilterName)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(FilterTabInfoVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(FilterTabNoConnectVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(TabFilterSlot)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsConnectEnabled)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(FilterStatus)));
        }

        public void SelectFilterWheel()
        {
            ActiveFilter = null;
            
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveFilter)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsWheelSelectionEnabled)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsFilterSelectionEnabled)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveFilterName)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(TabFilterSlot)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsConnectEnabled)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(MovingFilterVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveFilterVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(FilterStatus)));
        }
        
        public void MoveToSelectedSlot()
        {
            IsFilterWheelMoving = true;

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsFilterWheelMoving)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(MovingFilterVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveFilterVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(FilterStatus)));
        }

        public void SelectFilterSlot()
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedFilter)));
        }

        public void SaveLabels()
        {
            foreach (var filter in Filters)
            {
                filter.Save();
            }

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveFilterName)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(TabFilterSlot)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(FilterStatus)));
        }
    }
}
