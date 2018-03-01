using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace Prototype.Model
{
    public class CameraView : INotifyPropertyChanged
    {
        public CameraView()
        {
            Interfaces = new ObservableCollection<CameraInterfaceItem>
            {
                new CameraInterfaceItem("ASCOM"),
                new CameraInterfaceItem("ZWO ASI"),
                new CameraInterfaceItem("Canon"),
                new CameraInterfaceItem("Nikon")
            };

            Cameras = new ObservableCollection<CameraItem>();
            Gains = new ObservableCollection<GainOption>();
            BinValues = new ObservableCollection<string>();
            BinXValues = new ObservableCollection<string>();
            BinYValues = new ObservableCollection<string>();
        }

        public ObservableCollection<string> BinValues { get; set; }
        public ObservableCollection<string> BinXValues { get; set; }
        public ObservableCollection<string> BinYValues { get; set; }
        public ObservableCollection<CameraInterfaceItem> Interfaces { get; set; }
        public ObservableCollection<CameraItem> Cameras { get; set; }
        public ObservableCollection<GainOption> Gains { get; set; }
        public CameraInterfaceItem Interface { get; set; }
        public CameraItem Camera { get; set; }
        public bool IsCameraConnected { get; set; }  
        public bool IsGainDiscrete { get; set; }      
        public int SelectedGainIndex { get; set; }
        public double DiscreteGain { get; set; }
        public double GainMin { get; set; }
        public double GainMax { get; set; }        
        public double Offset { get; set; }
        public string BinX { get; set; }
        public string BinY { get; set; }
        public string BinXY { get; set; }
        public bool IsAsymmetricBinSupported { get; set; }
        public bool IsConnecting { get; set; }
        public int MaxADU { get; set; }
        public int UnbinnedX { get; set; }
        public int UnbinnedY { get; set; }
        public int BinnedX { get; set; }
        public int BinnedY { get; set; }
        public int ActualX { get; set; }
        public int ActualY { get; set; }
        public int SubFrameX { get; set; }
        public int SubFrameY { get; set; }
        public int SubFrameWidth { get; set; }
        public int SubFrameHeight { get; set; }
        public double ExposureMin { get; set; }
        public double ExposureMax { get; set; }
        public double ExposureMaxInMin { get; set; }
        public double ExposureResolution { get; set; }
        public double SecondTickFrequency { get; set; }
        public double MinuteTickFrequency { get; set; }
        public double MsecTickFrequency { get; set; }
        public bool IsSubFrameActive { get; set; }
        public bool IsInterfaceSelected => Interface != null;
        public bool IsCameraSelected => Camera != null;
        public bool IsInterfaceAscom => Interface?.Id == "ASCOM";
        public bool IsInterfaceZwo => Interface?.Id == "ZWO ASI";
        public bool IsInterfaceCanon => Interface?.Id == "Canon";        
        public bool IsInterfaceNikon => Interface?.Id == "Nikon";        
        public bool IsInterfaceSelectionEnabled => !IsCameraConnected;
        public bool IsCameraSelectionEnabled => !IsCameraConnected && IsInterfaceSelected;
        public Visibility AsymmetricBinVisibility => IsAsymmetricBinSupported ? Visibility.Visible : Visibility.Collapsed;
        public Visibility SymmetricBinVisibility => !IsAsymmetricBinSupported ? Visibility.Visible : Visibility.Collapsed;
        public Visibility GainComboBoxVisibility => !IsGainDiscrete ? Visibility.Visible : Visibility.Collapsed;
        public Visibility GainSliderVisibility => IsGainDiscrete ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ZwoVisibility => IsInterfaceZwo ? Visibility.Visible : Visibility.Collapsed;
        public Visibility CanonVisibility => IsInterfaceCanon ? Visibility.Visible : Visibility.Collapsed;
        public Visibility NikonVisibility => IsInterfaceNikon ? Visibility.Visible : Visibility.Collapsed;
        public Visibility AscomVisibility => IsInterfaceAscom ? Visibility.Visible : Visibility.Collapsed;
        public Visibility CameraSettingsVisibility => IsCameraConnected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility CameraTabInfoVisibility => IsCameraConnected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility CameraTabNoConnectVisibility => !IsCameraConnected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ConnectVisibility => !IsCameraConnected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility DisconnectVisibility => IsCameraConnected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ClearSubFrameVisibility => IsSubFrameActive ? Visibility.Visible : Visibility.Collapsed;
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        public void SetExposureOptions(double exposureMin, double exposureMax, double exposureResolution)
        {
            ExposureMin = exposureMin;
            ExposureMax = exposureMax;
            ExposureMaxInMin = exposureMax / 60.0;

            ExposureResolution = exposureResolution;

            MsecTickFrequency = exposureResolution * 1000;
            SecondTickFrequency = exposureResolution > 1 ? exposureResolution : 1;
            MinuteTickFrequency = exposureResolution / 60.0 > 1 ? exposureResolution : 1;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExposureMin)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExposureMax)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExposureMaxInMin)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExposureResolution)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MinuteTickFrequency)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MsecTickFrequency)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SecondTickFrequency)));
        }

        public void SetMaxADU(int maxADU)
        {
            MaxADU = maxADU;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MaxADU)));
        }

        public void ChangeActualSize(int width, int height)
        {
            ActualX = width;
            ActualY = height;
            
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(ActualX)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(ActualY)));
        }

        public void SetRegion(int x, int y, int width, int height)
        {
            SubFrameX = x;
            SubFrameY = y;
            SubFrameWidth = width;
            SubFrameHeight = height;

            IsSubFrameActive = true;
            
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsSubFrameActive)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(ClearSubFrameVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(SubFrameHeight)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(SubFrameWidth)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(SubFrameX)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(SubFrameY)));
        }

        public void ClearRegion()
        {
            SubFrameX = 0;
            SubFrameY = 0;
            SubFrameWidth = BinnedX;
            SubFrameHeight = BinnedY;

            IsSubFrameActive = false;
            
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsSubFrameActive)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(ClearSubFrameVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(SubFrameHeight)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(SubFrameWidth)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(SubFrameX)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(SubFrameY)));
        }


        public void ChangeUnbinnedSize(int unbinnedXSize, int unbinnedYSize)
        {
            UnbinnedX = unbinnedXSize;
            UnbinnedY = unbinnedYSize;
            
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(UnbinnedX)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(UnbinnedY)));
        }

        public void ChangeBinX(int x)
        {
            BinX = x.ToString();
            BinnedX = UnbinnedX / x;
            
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(BinX)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(BinnedX)));
        }

        public void ChangeBinY(int y)
        {
            BinY = y.ToString();
            BinnedY = UnbinnedY / y;
            
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(BinY)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(BinnedY)));
        }

        public void ChangeBinXY(int xy)
        {
            BinXY = xy.ToString();

            BinnedX = UnbinnedX / xy;
            BinnedY = UnbinnedY / xy;
            
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(BinXY)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(BinnedX)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(BinnedY)));
        }
        
        public void ChangeAsymmetricBinning(bool supported)
        {
            IsAsymmetricBinSupported = supported;

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(AsymmetricBinVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(SymmetricBinVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsAsymmetricBinSupported)));
        }

        public void ChangeGainMode(int index)
        {
            SelectedGainIndex = index;

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedGainIndex)));
        }

        public void ChangeGainSettings(bool discrete, double min, double max)
        {
            IsGainDiscrete = discrete;
            GainMin = min;
            GainMax = max;

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsGainDiscrete)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(GainComboBoxVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(GainSliderVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(GainMin)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(GainMax)));
        }

        public void ChangeInterface()
        {
            Camera = null;
            
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Interface)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsInterfaceSelected)));

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Camera)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsCameraSelected)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsCameraSelectionEnabled)));

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsInterfaceAscom)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsInterfaceZwo)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsInterfaceCanon)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsInterfaceNikon)));

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(AscomVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(ZwoVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(CanonVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(NikonVisibility)));
        }

        public void ChangeDiscreteGain(double value)
        {
            DiscreteGain = value;

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(DiscreteGain)));
        }

        public void ChangeOffset(double value)
        {
            Offset = value;

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Offset)));
        }

        public void ChangeCamera()
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Camera)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsCameraSelected)));

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(AscomVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(ZwoVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(CanonVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(NikonVisibility)));
        }

        public void ConnectCamera()
        {
            IsCameraConnected = true;

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(ConnectVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(DisconnectVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(CameraTabInfoVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(CameraTabNoConnectVisibility)));

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsCameraConnected)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsInterfaceSelectionEnabled)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsCameraSelectionEnabled)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(CameraSettingsVisibility)));
        }

        public void DisconnectCamera()
        {
            IsCameraConnected = false;

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(ConnectVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(DisconnectVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(CameraTabInfoVisibility)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(CameraTabNoConnectVisibility)));

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsCameraConnected)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsInterfaceSelectionEnabled)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsCameraSelectionEnabled)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(CameraSettingsVisibility)));
        }
    }
}
