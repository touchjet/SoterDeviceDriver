using System.Collections.ObjectModel;

namespace SoterDevice.Models
{
    public class GetAddressesResult
    {
        public Collection<AccountResult> Accounts { get; } = new Collection<AccountResult>();
    }
}
