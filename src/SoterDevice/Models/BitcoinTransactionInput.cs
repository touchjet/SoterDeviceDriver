using System;
using SoterDevice.Contracts;

namespace SoterDevice.Models
{
    public class BitcoinTransactionInput
    {
        public uint[] AddressNs { get; set; }

        public byte[] PrevHash { get; set; }

        public uint PrevIndex { get; set; }

        public byte[] ScriptSig { get; set; }

        public uint Sequence { get; set; }

        public InputScriptType ScriptType { get; set; }

        public MultisigRedeemScriptType Multisig { get; set; }

        public ulong Amount { get; set; }

        public uint DecredTree { get; set; }

        public uint DecredScriptVersion { get; set; }

        public BitcoinTransaction PrevTransaction { get; set; }

    }
}
