using System;
using System.Threading.Tasks;

namespace SoterDevice
{
    public interface ISoterDeviceDfu
    {
        Task<bool> SearchAndConnectDeviceAsync();
        Task<bool> PerformDfuAsync();
    }
}
