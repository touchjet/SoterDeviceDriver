using System;
using System.Threading.Tasks;

namespace SoterDevice.Ble
{
    public class SoterDeviceBle : SoterDeviceBase
    {
        public SoterDeviceBle(EnterPinArgs enterPinCallback) : base(enterPinCallback)
        {
        }

        protected override Task<object> ReadAsync()
        {
            throw new NotImplementedException();
        }

        protected override Task WriteAsync(object msg)
        {
            throw new NotImplementedException();
        }
    }
}
