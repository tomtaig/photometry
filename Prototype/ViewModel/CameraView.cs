using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace Prototype.ViewModel
{
    public class CameraView : INotifyPropertyChanged
    {
        public CameraView()
        {
            Interfaces = new ObservableCollection<CameraInterfaceItem>();

            Interfaces.Add(new CameraInterfaceItem("ASCOM"));
            Interfaces.Add(new CameraInterfaceItem("ZWO ASI"));
            Interfaces.Add(new CameraInterfaceItem("Canon"));
            Interfaces.Add(new CameraInterfaceItem("Nikon"));

            Cameras = new ObservableCollection<CameraItem>();
        }

        public ObservableCollection<CameraInterfaceItem> Interfaces { get; set; }
        public ObservableCollection<CameraItem> Cameras { get; set; }
        
        public CameraInterfaceItem Interface { get; set; }
        public CameraItem Camera { get; set; }
        public bool IsCameraConnected { get; set; }
        
        public double Gain { get; set; }
        public double Offset { get; set; }
        public double Gamma { get; set; }

        public bool IsInterfaceSelected => Interface != null;
        public bool IsCameraSelected => Camera != null;

        public bool IsInterfaceAscom => Interface?.Id == "ASCOM";
        public bool IsInterfaceZwo => Interface?.Id == "ZWO ASI";
        public bool IsInterfaceCanon => Interface?.Id == "Canon";        
        public bool IsInterfaceNikon => Interface?.Id == "Nikon";
        
        public bool IsInterfaceSelectionEnabled => !IsCameraConnected;
        public bool IsCameraSelectionEnabled => !IsCameraConnected && IsInterfaceSelected;

        public Visibility ZwoVisibility => IsInterfaceZwo ? Visibility.Visible : Visibility.Collapsed;
        public Visibility CanonVisibility => IsInterfaceCanon ? Visibility.Visible : Visibility.Collapsed;
        public Visibility NikonVisibility => IsInterfaceNikon ? Visibility.Visible : Visibility.Collapsed;
        public Visibility AscomVisibility => IsInterfaceAscom ? Visibility.Visible : Visibility.Collapsed;
        public Visibility CameraSettingsVisibility => IsCameraConnected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility CameraTabInfoVisibility => IsCameraConnected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility CameraTabNoConnectVisibility => !IsCameraConnected ? Visibility.Visible : Visibility.Collapsed;

        public Visibility ConnectVisibility => !IsCameraConnected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility DisconnectVisibility => IsCameraConnected ? Visibility.Visible : Visibility.Collapsed;

        public event PropertyChangedEventHandler PropertyChanged;

        public void ChangeInterface()
        {
            Camera = null;

            switch (Interface?.Id)
            {
                case "ASCOM":
                    LoadAscomCameras();
                    break;
                case "ZWO ASI":
                    LoadZwoCameras();
                    break;
                default:
                    Cameras.Clear();
                    break;
            }

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

        private void LoadZwoCameras()
        {
            Cameras.Clear();
            Cameras.Add(new CameraItem("ASI 1600MM-C"));
            Cameras.Add(new CameraItem("ASI 120MC"));
        }

        private void LoadAscomCameras()
        {
            Cameras.Clear();
            Cameras.Add(new CameraItem("ASI Camera (1)"));
            Cameras.Add(new CameraItem("ASI Camera (2)"));
        }

        public void ChangeGain(double value)
        {
            Gain = value;

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Gain)));
        }

        public void ChangeOffset(double value)
        {
            Offset = value;

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Offset)));
        }

        public void ChangeGamma(double value)
        {
            Gamma = value;

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Gamma)));
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

        public void OpenAscomSettings()
        {
        }
    }
}
