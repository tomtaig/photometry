using System;
using Prototype.ViewModel;

namespace Prototype
{
    public class Session
    {
        public Session()
        {
            Camera = new CameraView();
            Cooler = new CoolerView();
            Filter = new FilterView();
        }

        public CameraView Camera { get; set; }
        public CoolerView Cooler { get; set; }
        public FilterView Filter { get; set; }

        public void ConnectCamera()
        {
            Camera.ConnectCamera();

            Cooler.SetAvailable(true);
        }

        public void DisconnectCamera()
        {
            Camera.DisconnectCamera();

            Cooler.SetAvailable(false);
        }

        public void ConnectWheel()
        {
            Filter.ConnectWheel();
        }

        public void DisconnectWheel()
        {
            Filter.DisconnectWheel();
        }
    }
}
