using Prototype.Model;

namespace Prototype.Services.Interfaces
{
    public interface IFilterWheelService
    {
        OperationResult Initialize(Session session);
        OperationResult Connect(Session session);
        OperationResult Disconnect(Session session);
        OperationResult GetSlotPosition(out short? position, out bool? moving);
        OperationResult SetSlotPosition(short position);
    }
}
