using System.Numerics;

namespace SoterDevice.Models
{
    public interface IEthereumTransaction : ITransaction
    {
        uint ChainId { get; }
        BigInteger GasPrice { get; }
        BigInteger GasLimit { get; }
        BigInteger Nonce { get; }
        byte[] Data { get; }
    }
}
