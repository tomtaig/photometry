using OxyPlot;
using OxyPlot.Axes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace Prototype.Model
{
    public class CoolerView : INotifyPropertyChanged
    {
        public CoolerView()
        {            
            TemperatureReadings = new List<DataPoint>();
            PowerReadings = new List<DataPoint>();            
        }

        public IList<DataPoint> PowerReadings { get; set; }
        public IList<DataPoint> TemperatureReadings { get; set; }
        public DateTime FirstReadingTime { get; set; }
        public DateTime LastReadingTime { get; set; }        
        public double? TargetTemperature { get; set; }
        public bool IsOn { get; set; }
        public bool IsTargetTemperatureAchieved { get; set; }
        public bool IsAvailable { get; set; }

        public bool IsOnAndTargetAchieved => IsOn && IsTargetTemperatureAchieved;
        public bool IsOnAndTargetSet => IsOn && TargetTemperature.HasValue;
        public bool TurnOnEnabled => !IsOn && IsAvailable;
        public bool TurnOffEnabled => IsOn;

        public string Status => !IsAvailable ? "Unavailable" : IsOn ? IsTargetTemperatureAchieved ? "Stable" : "Active" : "Off";

        public Visibility CoolerTabInfoVisibility => IsOn ? Visibility.Visible : Visibility.Collapsed;
        public Visibility CoolerTabNotOnVisibility => !IsOn ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsOnVisibility => IsOn ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsOffVisibility => !IsOn ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsAvailableVisibility => IsAvailable ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsNotAvailableVisibility => !IsAvailable ? Visibility.Visible : Visibility.Collapsed;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler ReadingsUpdated;
        
        public void TurnOn()
        {
            if(IsOn)
            {
                return;
            }

            IsOn = true;

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsOn)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(TurnOnEnabled)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(TurnOffEnabled)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(CoolerTabInfoVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(CoolerTabNotOnVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsOffVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsOnVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsOnAndTargetSet)));
        }

        public void AddReading(DateTime readingTime, double? temperature, double? power)
        {
            LastReadingTime = readingTime;
            
            TemperatureReadings.Add(new DataPoint(DateTimeAxis.ToDouble(readingTime), temperature ?? double.NaN));
            PowerReadings.Add(new DataPoint(DateTimeAxis.ToDouble(readingTime), power ?? double.NaN));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastPowerReading)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastTemperatureReading)));
            ReadingsUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void TurnOff()
        {
            if(!IsOn)
            {
                return;
            }

            IsOn = false;

            TargetTemperature = null;

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsOn)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(TurnOnEnabled)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(TurnOffEnabled)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(CoolerTabInfoVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(CoolerTabNotOnVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsOffVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsOnVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsOnAndTargetSet)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(TargetTemperature)));
        }
        
        public void SetAvailable(bool available)
        {
            if(IsOn)
            {
                TurnOff();
            }
            
            if(!available)
            {
                TemperatureReadings.Clear();
                PowerReadings.Clear();
                ReadingsUpdated?.Invoke(this, EventArgs.Empty);
            }

            IsAvailable = available;

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsAvailable)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsOn)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(TurnOnEnabled)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(TurnOffEnabled)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(CoolerTabInfoVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(CoolerTabNotOnVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsOffVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsOnVisibility)));
        }

        public void SetTargetTemperature(double degrees)
        {
            TargetTemperature = degrees;

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(TargetTemperature)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsOnAndTargetSet)));
        }

        public double LastTemperatureReading
        {
            get { return TemperatureReadings.LastOrDefault().Y; }
        }

        public double LastPowerReading
        {
            get { return PowerReadings.LastOrDefault().Y; }
        }
    }
}
