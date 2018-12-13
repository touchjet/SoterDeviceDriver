using System.Collections.Generic;

namespace SoterDevice.Models
{
    public interface IAddressPath
    {
        List<IAddressPathElement> AddressPathElements { get; }

        uint[] ToArray();
    }
}
