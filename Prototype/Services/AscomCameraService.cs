using ASCOM.Utilities;
using Prototype.Services.Interfaces;
using Prototype.Model;
using System;
using System.Linq;
using ASCOM;
using System.Collections.Generic;
using OpenCvSharp;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;
using OpenCvSharp.Extensions;
using System.Windows.Media.Imaging;
using System.IO;

namespace Prototype.Services
{
    public class AscomCameraService : ICameraService
    {
        public ASCOM.DriverAccess.Camera _camera;
        bool? _coolerPowerAvailable;

        public OperationResult Initialize(Session session)
        {
            var profile = new Profile();
            var ascomCameras = profile.RegisteredDevices("Camera");

            session.Camera.Cameras.Clear();

            foreach (KeyValuePair camera in ascomCameras)
            {
                session.Camera.Cameras.Add(new CameraItem { Id = camera.Key, Name = camera.Value });
            }

            return OperationResult.Ok;
        }

        public OperationResult Connect(Session session)
        {
            var cameraId = session.Camera.Camera.Id;

            _coolerPowerAvailable = null;

            try
            {
                session.Camera.IsConnecting = true;

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
                session.Camera.ChangeUnbinnedSize(_camera.CameraXSize, _camera.CameraYSize);

                session.Camera.SetMaxADU(_camera.MaxADU);
                session.Camera.SetExposureOptions(_camera.ExposureMin, _camera.ExposureMax, _camera.ExposureResolution);

                if (_camera.CanAsymmetricBin)
                {
                    var x = new List<string>();

                    for (var i = 1; i <= _camera.MaxBinX; i++)
                    {
                        x.Add(i.ToString());
                    }

                    var y = new List<string>();

                    for (var i = 1; i <= _camera.MaxBinY; i++)
                    {
                        y.Add(i.ToString());
                    }

                    session.SetAsymmetricBins(x, y);

                    session.Camera.ChangeBinX(_camera.BinX);
                    session.Camera.ChangeBinY(_camera.BinY);
                }
                else
                {
                    var bin = new List<string>();

                    for (var i = 1; i <= _camera.MaxBinY; i++)
                    {
                        bin.Add($"{i}x{i}");
                    }

                    session.SetSymmetricBins(bin);

                    session.Camera.ChangeBinXY(_camera.BinX);
                }

                try
                {
                    if (_camera.Gains.Count > 0)
                    {
                        session.SetGainSettings(false, 0, 0);

                        var gainOptions = _camera.Gains
                            .ToArray()
                            .Select(x => new GainOption { Index = session.Camera.Gains.Count, Value = (string)x })
                            .ToList();

                        session.SetGainOptions(gainOptions);
                        session.Camera.ChangeGainMode(_camera.Gain);
                    }
                }
                catch (PropertyNotImplementedException) // happens when the camera does not support specific gain settings
                {
                    session.SetGainSettings(true, _camera.GainMin, _camera.GainMax);
                }

                session.Cooler.SetAvailable(_camera.CanSetCCDTemperature);

                if (_camera.CanSetCCDTemperature)
                {
                    session.Cooler.TargetTemperature = _camera.SetCCDTemperature;
                }

                session.Camera.SubFrameX = _camera.StartX;
                session.Camera.SubFrameY = _camera.StartY;
                session.Camera.SubFrameWidth = _camera.NumX;
                session.Camera.SubFrameHeight = _camera.NumY;
            }
            finally
            {
                session.Camera.IsConnecting = false;
            }

            return OperationResult.Ok;
        }

        public OperationResult SetDiscreteGain(short value)
        {
            _camera.Gain = value;

            return OperationResult.Ok;
        }

        public OperationResult SetGainMode(string value)
        {
            _camera.Gain = (short)_camera.Gains.IndexOf(value);

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

            if (_coolerPowerAvailable.HasValue && !_coolerPowerAvailable.Value)
            {
                return new OperationResult { IsError = true, ErrorMessage = "Cooler power not available" };
            }

            try
            {
                power = _camera.CoolerPower;
            }
            catch (PropertyNotImplementedException)
            {
                _coolerPowerAvailable = false;
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

        public OperationResult SetBinXY(int xy)
        {
            _camera.NumX = _camera.CameraXSize / xy;
            _camera.NumY = _camera.CameraYSize / xy;

            _camera.BinX = (short)xy;

            return OperationResult.Ok;
        }

        public OperationResult SetBinY(int y)
        {
            _camera.BinY = (short)y;
            _camera.NumY = _camera.CameraYSize / y;

            return OperationResult.Ok;
        }

        public OperationResult SetBinX(int x)
        {
            _camera.BinX = (short)x;
            _camera.NumX = _camera.CameraXSize / x;

            return OperationResult.Ok;
        }

        public unsafe Task<ImageView> Capture(double exposure)
        {
            return new Task<ImageView>(() =>
            {
                _camera.StartExposure(exposure, true);

                Thread.Sleep((int)(exposure / 1000.0));

                while (!_camera.ImageReady) ;

                var array = (int[,])_camera.ImageArray;

                var xSize = array.GetUpperBound(0) + 1;
                var ySize = array.GetUpperBound(1) + 1;

                fixed (int* img = &(array[0, 0]))
                {
                    var pixels = new ushort[ySize * xSize];

                    for (var i = 0; i < pixels.Length; i++)
                    {
                        pixels[i] = (ushort)img[i];
                    }

                    return new ImageView { XSize = xSize, YSize = ySize, Image = pixels };
                }
            });
        }

        public OperationResult SetRegion(int subFrameX, int subFrameY, int subFrameWidth, int subFrameHeight)
        {
            _camera.NumX = subFrameWidth;
            _camera.NumY = subFrameHeight;
            _camera.StartX = subFrameX;
            _camera.StartY = subFrameY;

            return OperationResult.Ok;
        }

        public OperationResult ClearRegion()
        {
            _camera.NumX = _camera.CameraXSize / _camera.BinX;
            _camera.NumY = _camera.CameraYSize / _camera.BinY;
            _camera.StartX = 0;
            _camera.StartY = 0;

            return OperationResult.Ok;
        }
    }
}
