namespace SoterDevice.Models
{
    public interface IAddressPathFactory
    {
        IAddressPath GetAddressPath(uint change, uint account, uint addressIndex);
    }
}
