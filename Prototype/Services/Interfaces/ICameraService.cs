using OpenCvSharp;
using Prototype.Model;
using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Prototype.Services.Interfaces
{
    public interface ICameraService
    {
        OperationResult Initialize(Session session);
        OperationResult Connect(Session session);
        OperationResult Disconnect(Session session);
        OperationResult ToggleCooling(bool on);
        OperationResult SetTargetTemperature(double celsius);
        OperationResult GetObservedCelsius(out double? celsius);
        OperationResult GetObservedCoolingPower(out double? celsius);
        OperationResult GetTargetTemperature(out double? celsius);
        OperationResult SetDiscreteGain(short value);
        OperationResult SetGainMode(string value);
        OperationResult SetBinXY(int xy);
        OperationResult SetBinY(int y);
        OperationResult SetBinX(int x);
        Task<Mat> Capture(double exposure);
        OperationResult SetRegion(int subFrameX, int subFrameY, int subFrameWidth, int subFrameHeight);
        OperationResult ClearRegion();
    }
}
