namespace SoterDevice.Models
{
    public interface ITransaction
    {
        IAddressPath From { get; }
        decimal Value { get; }
        string To { get; }
    }
}
