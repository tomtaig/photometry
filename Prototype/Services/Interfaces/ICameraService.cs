using Prototype.Model;

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
    }
}
