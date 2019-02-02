using System;
using SoterDevice.Contracts;

namespace SoterDevice.Models
{
    public class BitcoinTransactionOutput
    {
        public string Address { get; set; }
        public uint[] AddressNs { get; set; }
        public ulong Amount { get; set; }
        public OutputScriptType ScriptType { get; set; }
        public MultisigRedeemScriptType Multisig { get; set; }
        public byte[] OpReturnData { get; set; }
        public OutputAddressType AddressType { get; set; }
        public ExchangeType ExchangeType { get; set; }
        public uint DecredScriptVersion { get; set; }
    }
}
