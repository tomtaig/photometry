using ASCOM.Utilities;
using Prototype.Services.Interfaces;
using Prototype.Model;
using System;
using ASCOM;

namespace Prototype.Services
{
    public class AscomCameraService : ICameraService
    {
        public ASCOM.DriverAccess.Camera _camera;

        public OperationResult Initialize(Session session)
        {
            var profile = new Profile();
            var ascomCameras = profile.RegisteredDevices("Camera");

            session.Camera.Cameras.Clear();
            
            foreach (KeyValuePair camera in ascomCameras)
            {
                session.Camera.Cameras.Add(new Model.CameraItem { Id = camera.Key, Name = camera.Value });
            }

            return OperationResult.Ok;
        }

        public OperationResult Connect(Session session)
        {
            var cameraId = session.Camera.Camera.Id;

            try
            {
                _camera = new ASCOM.DriverAccess.Camera(cameraId);

                _camera.Connected = true;
            }
            catch (Exception ex)
            {
                return new OperationResult { IsError = true, ErrorMessage = ex.Message };
            }

            session.Camera.ConnectCamera();
            session.Cooler.SetAvailable(_camera.CanSetCCDTemperature);

            if (_camera.CanSetCCDTemperature)
            {
                session.Cooler.TargetTemperature = _camera.SetCCDTemperature;
            }

            return OperationResult.Ok;
        }

        public OperationResult Disconnect(Session session)
        {
            if (_camera.CanSetCCDTemperature)
            {
                _camera.CoolerOn = false;
            }

            _camera.Connected = false;
            _camera.Dispose();
            _camera = null;
            
            session.Camera.DisconnectCamera();
            session.Cooler.SetAvailable(false);

            return OperationResult.Ok;
        }

        public OperationResult OpenSetupDialog()
        {
            try
            {
                _camera.SetupDialog();
            }
            catch (Exception ex)
            {
                return new OperationResult { IsError = true, ErrorMessage = ex.Message };
            }

            return OperationResult.Ok;
        }

        public OperationResult ToggleCooling(bool on)
        {
            try
            {
                _camera.CoolerOn = on;
            }
            catch (Exception ex)
            {
                return new OperationResult { IsError = true, ErrorMessage = ex.Message };
            }

            return OperationResult.Ok;
        }
        
        public OperationResult SetTargetTemperature(double celsius)
        {
            try
            {
                _camera.SetCCDTemperature = celsius;
            }
            catch (Exception ex)
            {
                return new OperationResult { IsError = true, ErrorMessage = ex.Message };
            }

            return OperationResult.Ok;
        }

        public OperationResult GetObservedCelsius(out double? celsius)
        {
            celsius = null;

            try
            {
                celsius = _camera.CCDTemperature;
            }
            catch (Exception ex)
            {
                return new OperationResult { IsError = true, ErrorMessage = ex.Message };
            }

            return OperationResult.Ok;
        }

        public OperationResult GetObservedCoolingPower(out double? power)
        {
            power = null;

            try
            {
                power = _camera.CoolerPower;
            }
            catch (Exception ex)
            {
                return new OperationResult { IsError = true, ErrorMessage = ex.Message };
            }

            return OperationResult.Ok;
        }

        public OperationResult GetTargetTemperature(out double? celsius)
        {
            celsius = null;

            try
            {
                celsius = _camera.SetCCDTemperature;
            }
            catch (Exception ex)
            {
                return new OperationResult { IsError = true, ErrorMessage = ex.Message };
            }

            return OperationResult.Ok;
        }
    }
}
