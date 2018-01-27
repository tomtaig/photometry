using ASCOM.Utilities;
using Prototype.Model;
using Prototype.Services.Interfaces;
using System;

namespace Prototype.Services
{
    public class AscomFilterWheelService : IFilterWheelService
    {
        ASCOM.DriverAccess.FilterWheel _wheel;

        public OperationResult Initialize(Session session)
        {
            var profile = new Profile();
            var filterWheels = profile.RegisteredDevices("FilterWheel");

            session.Filter.Wheels.Clear();

            foreach (KeyValuePair wheel in filterWheels)
            {
                session.Filter.Wheels.Add(new FilterWheel { Id = wheel.Key, Name = wheel.Value });
            }

            return OperationResult.Ok;
        }

        public OperationResult Connect(Session session)
        {
            var wheelId = session.Filter.SelectedWheel.Id;

            try
            {
                _wheel = new ASCOM.DriverAccess.FilterWheel(wheelId);

                _wheel.Connected = true;
            }
            catch (Exception ex)
            {
                return new OperationResult { IsError = true, ErrorMessage = ex.Message };
            }

            session.Filter.ConnectWheel();

            session.Filter.Filters.Clear();

            for (var i=0; i<_wheel.Names.Length; i++)
            {
                session.Filter.Filters.Add(new FilterItem { Name = _wheel.Names[i], Number = (short)(i+1) });
            }

            return OperationResult.Ok;
        }

        public OperationResult Disconnect(Session session)
        {
            try
            {
                _wheel.Connected = false;
            }
            catch (Exception ex)
            {
                return new OperationResult { IsError = true, ErrorMessage = ex.Message };
            }

            _wheel.Dispose();
            _wheel = null;

            return OperationResult.Ok;
        }
        
        public OperationResult GetSlotPosition(out short? position, out bool? moving)
        {
            position = null;
            moving = null;

            try
            {
                position = (short)(_wheel.Position + 1);

                if(position == 0)
                {
                    position = null;
                    moving = true;
                }
                else
                {
                    moving = false;
                }
            }
            catch (Exception ex)
            {
                return new OperationResult { IsError = true, ErrorMessage = ex.Message };
            }

            return OperationResult.Ok;
        }
        
        public OperationResult SetSlotPosition(short position)
        {
            try
            {
                _wheel.Position = (short)(position - 1);
            }
            catch (Exception ex)
            {
                return new OperationResult { IsError = true, ErrorMessage = ex.Message };
            }

            return OperationResult.Ok;
        }
    }
}
