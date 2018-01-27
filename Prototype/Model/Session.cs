using Prototype.Model;
using Prototype.Services;
using Prototype.Services.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Prototype
{
    public class Session
    {
        Task _coolingMonitor;
        Task _filterWheelMonitor;
        CancellationTokenSource _cancelCoolingMonitor;
        CancellationTokenSource _cancelFilterWheelMonitor;

        public Session()
        {
            Camera = new CameraView();
            Cooler = new CoolerView();
            Filter = new FilterView();

            FilterWheelService = new AscomFilterWheelService();
            FilterWheelService.Initialize(this);
        }

        public ICameraService CameraService { get; set; }
        public IFilterWheelService FilterWheelService { get; set; }
        public CameraView Camera { get; set; }
        public CoolerView Cooler { get; set; }
        public FilterView Filter { get; set; }

        public OperationResult SetCameraInterface()
        {
            switch (Camera.Interface?.Id)
            {
                case "ASCOM":
                    CameraService = new AscomCameraService();
                    break;
                default:
                    Camera.Cameras.Clear();
                    break;
            }

            if (CameraService != null)
            {
                var result = CameraService.Initialize(this);

                if (!result.IsError)
                {
                    Camera.ChangeInterface();
                }

                return result;
            }

            return OperationResult.Ok;
        }

        public OperationResult ConnectCamera()
        {
            var result = CameraService.Connect(this);

            if(!result.IsError)
            {
                StartMonitorCoolingTask();
            }

            return result;
        }

        public OperationResult MoveFilterWheel()
        {
            var result = FilterWheelService.SetSlotPosition(Filter.SelectedFilter.Number);

            if(!result.IsError)
            {
                Filter.IsFilterWheelMoving = true;
            }

            return result;
        }

        public OperationResult DisconnectCamera()
        {
            if (Cooler.IsOn && !WarnAboutCooler())
            {
                return OperationResult.Ok;
            }

            var result = CameraService.Disconnect(this);

            StopMonitorCoolingTask();

            return result;
        }

        bool WarnAboutCooler()
        {
            var result = MessageBox.Show("The cooler is on. Consider increasing the sensor temperature to protect equipment or press OK to continue anyway.", "Cooler On", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

            return result == MessageBoxResult.OK;
        }

        public OperationResult ConnectWheel()
        {
            var result = FilterWheelService.Connect(this);

            if (!result.IsError)
            {
                StartMonitorFilterWheelTask();

                Filter.ConnectWheel();
            }

            return result;
        }

        public OperationResult DisconnectWheel()
        {
            var result = FilterWheelService.Disconnect(this);

            if (!result.IsError)
            {
                StopMonitorFilterWheelTask();

                Filter.DisconnectWheel();
            }

            return result;
        }
        
        void StartMonitorFilterWheelTask()
        {
            _cancelFilterWheelMonitor = new CancellationTokenSource();
            _filterWheelMonitor = new Task(() => MonitorFilterWheel(_cancelFilterWheelMonitor.Token), _cancelFilterWheelMonitor.Token);
            _filterWheelMonitor.Start();
        }

        void StartMonitorCoolingTask()
        {
            _cancelCoolingMonitor = new CancellationTokenSource();
            _coolingMonitor = new Task(() => MonitorCooling(_cancelCoolingMonitor.Token), _cancelCoolingMonitor.Token);
            _coolingMonitor.Start();
        }
        
        public OperationResult TurnOnCooler()
        {
            var result = CameraService.ToggleCooling(true);

            if(!result.IsError)
            {
                Cooler.TurnOn();

                double? target;
                result = CameraService.GetTargetTemperature(out target);

                if (!result.IsError)
                {
                    Cooler.SetTargetTemperature(target.Value);
                }
            }

            return result;
        }

        public OperationResult TurnOffCooler()
        {
            if (Cooler.IsOn && !WarnAboutCooler())
            {
                return OperationResult.Ok;
            }

            var result = CameraService.ToggleCooling(false);

            if (!result.IsError)
            {
                Cooler.TurnOff();
            }

            return result;
        }

        public OperationResult SetTargetTemperature(double celsius)
        {
            var result = CameraService.SetTargetTemperature(celsius);

            if (!result.IsError)
            {
                Cooler.SetTargetTemperature(celsius);
            }

            return result;
        }

        void StopMonitorCoolingTask()
        {
            if (_cancelCoolingMonitor != null)
            {
                _cancelCoolingMonitor.Cancel();
                _cancelCoolingMonitor = null;
                _coolingMonitor = null;
            }
        }

        void StopMonitorFilterWheelTask()
        {
            if (_cancelFilterWheelMonitor != null)
            {
                _cancelFilterWheelMonitor.Cancel();
                _cancelFilterWheelMonitor = null;
                _filterWheelMonitor = null;
            }
        }

        void MonitorCooling(CancellationToken token)
        {
            Cooler.FirstReadingTime = DateTime.Now;

            var readingTime = Cooler.FirstReadingTime;

            while (!token.IsCancellationRequested)
            {
                CameraService.GetObservedCelsius(out double? celsius);
                CameraService.GetObservedCoolingPower(out double? power);
                
                Cooler.AddReading(DateTime.Now, celsius ?? double.NaN, power);

                Thread.Sleep(1000);

                readingTime = DateTime.Now;
            }
        }

        void MonitorFilterWheel(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var result = FilterWheelService.GetSlotPosition(out short? position, out bool? moving);

                if (!result.IsError)
                {
                    if (moving.Value)
                    {
                        Filter.SetFilterWheelMoving(true);
                    }
                    else
                    {
                        Filter.SetActiveFilter(position.Value);
                    }
                }

                Thread.Sleep(1000);
            }
        }
    }
}
