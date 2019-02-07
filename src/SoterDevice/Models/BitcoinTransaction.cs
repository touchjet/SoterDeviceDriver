using System.Collections.Generic;

namespace SoterDevice.Models
{
    public class BitcoinTransaction
    {
        public BitcoinTransaction(string coinName, uint version = 2, uint lockTime = 0, uint expiry = 0)
        {
            CoinName = coinName;
            Version = version;
            LockTime = lockTime;
            Expiry = expiry;
            Inputs = new List<BitcoinTransactionInput>();
            Outputs = new List<BitcoinTransactionOutput>();
        }

        public string CoinName { get; set; }
        public uint Version { get; set; }
        public uint LockTime { get; set; }
        public uint Expiry { get; set; }
        public List<BitcoinTransactionInput> Inputs { get; }
        public List<BitcoinTransactionOutput> Outputs { get; }
        public byte[] ExtraData { get; set; }

        public byte[] SerializedTx { get; set; }
    }
}
