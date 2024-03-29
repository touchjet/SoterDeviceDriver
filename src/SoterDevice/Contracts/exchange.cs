// This file was generated by a tool; you should avoid making direct changes.
// Consider using 'partial classes' to extend these types
// Input: exchange.proto

#pragma warning disable CS0612, CS1591, CS3021, IDE1006, RCS1036, RCS1057, RCS1085, RCS1192
namespace SoterDevice.Contracts
{

    [global::ProtoBuf.ProtoContract()]
    public partial class ExchangeAddress : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1, Name = @"coin_type")]
        [global::System.ComponentModel.DefaultValue("")]
        public string CoinType
        {
            get { return __pbn__CoinType ?? ""; }
            set { __pbn__CoinType = value; }
        }
        public bool ShouldSerializeCoinType() => __pbn__CoinType != null;
        public void ResetCoinType() => __pbn__CoinType = null;
        private string __pbn__CoinType;

        [global::ProtoBuf.ProtoMember(2, Name = @"address")]
        [global::System.ComponentModel.DefaultValue("")]
        public string Address
        {
            get { return __pbn__Address ?? ""; }
            set { __pbn__Address = value; }
        }
        public bool ShouldSerializeAddress() => __pbn__Address != null;
        public void ResetAddress() => __pbn__Address = null;
        private string __pbn__Address;

        [global::ProtoBuf.ProtoMember(3, Name = @"dest_tag")]
        [global::System.ComponentModel.DefaultValue("")]
        public string DestTag
        {
            get { return __pbn__DestTag ?? ""; }
            set { __pbn__DestTag = value; }
        }
        public bool ShouldSerializeDestTag() => __pbn__DestTag != null;
        public void ResetDestTag() => __pbn__DestTag = null;
        private string __pbn__DestTag;

        [global::ProtoBuf.ProtoMember(4, Name = @"rs_address")]
        [global::System.ComponentModel.DefaultValue("")]
        public string RsAddress
        {
            get { return __pbn__RsAddress ?? ""; }
            set { __pbn__RsAddress = value; }
        }
        public bool ShouldSerializeRsAddress() => __pbn__RsAddress != null;
        public void ResetRsAddress() => __pbn__RsAddress = null;
        private string __pbn__RsAddress;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class ExchangeResponseV2 : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1, Name = @"deposit_address")]
        public ExchangeAddress DepositAddress { get; set; }

        [global::ProtoBuf.ProtoMember(2, Name = @"deposit_amount")]
        public byte[] DepositAmount
        {
            get { return __pbn__DepositAmount; }
            set { __pbn__DepositAmount = value; }
        }
        public bool ShouldSerializeDepositAmount() => __pbn__DepositAmount != null;
        public void ResetDepositAmount() => __pbn__DepositAmount = null;
        private byte[] __pbn__DepositAmount;

        [global::ProtoBuf.ProtoMember(3, Name = @"expiration")]
        public long Expiration
        {
            get { return __pbn__Expiration.GetValueOrDefault(); }
            set { __pbn__Expiration = value; }
        }
        public bool ShouldSerializeExpiration() => __pbn__Expiration != null;
        public void ResetExpiration() => __pbn__Expiration = null;
        private long? __pbn__Expiration;

        [global::ProtoBuf.ProtoMember(4, Name = @"quoted_rate")]
        public byte[] QuotedRate
        {
            get { return __pbn__QuotedRate; }
            set { __pbn__QuotedRate = value; }
        }
        public bool ShouldSerializeQuotedRate() => __pbn__QuotedRate != null;
        public void ResetQuotedRate() => __pbn__QuotedRate = null;
        private byte[] __pbn__QuotedRate;

        [global::ProtoBuf.ProtoMember(5, Name = @"withdrawal_address")]
        public ExchangeAddress WithdrawalAddress { get; set; }

        [global::ProtoBuf.ProtoMember(6, Name = @"withdrawal_amount")]
        public byte[] WithdrawalAmount
        {
            get { return __pbn__WithdrawalAmount; }
            set { __pbn__WithdrawalAmount = value; }
        }
        public bool ShouldSerializeWithdrawalAmount() => __pbn__WithdrawalAmount != null;
        public void ResetWithdrawalAmount() => __pbn__WithdrawalAmount = null;
        private byte[] __pbn__WithdrawalAmount;

        [global::ProtoBuf.ProtoMember(7, Name = @"return_address")]
        public ExchangeAddress ReturnAddress { get; set; }

        [global::ProtoBuf.ProtoMember(8, Name = @"api_key")]
        public byte[] ApiKey
        {
            get { return __pbn__ApiKey; }
            set { __pbn__ApiKey = value; }
        }
        public bool ShouldSerializeApiKey() => __pbn__ApiKey != null;
        public void ResetApiKey() => __pbn__ApiKey = null;
        private byte[] __pbn__ApiKey;

        [global::ProtoBuf.ProtoMember(9, Name = @"miner_fee")]
        public byte[] MinerFee
        {
            get { return __pbn__MinerFee; }
            set { __pbn__MinerFee = value; }
        }
        public bool ShouldSerializeMinerFee() => __pbn__MinerFee != null;
        public void ResetMinerFee() => __pbn__MinerFee = null;
        private byte[] __pbn__MinerFee;

        [global::ProtoBuf.ProtoMember(10, Name = @"order_id")]
        public byte[] OrderId
        {
            get { return __pbn__OrderId; }
            set { __pbn__OrderId = value; }
        }
        public bool ShouldSerializeOrderId() => __pbn__OrderId != null;
        public void ResetOrderId() => __pbn__OrderId = null;
        private byte[] __pbn__OrderId;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class SignedExchangeResponse : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1, Name = @"response")]
        public ExchangeResponse Response { get; set; }

        [global::ProtoBuf.ProtoMember(2, Name = @"signature")]
        public byte[] Signature
        {
            get { return __pbn__Signature; }
            set { __pbn__Signature = value; }
        }
        public bool ShouldSerializeSignature() => __pbn__Signature != null;
        public void ResetSignature() => __pbn__Signature = null;
        private byte[] __pbn__Signature;

        [global::ProtoBuf.ProtoMember(3)]
        public ExchangeResponseV2 responseV2 { get; set; }

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class ExchangeResponse : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1, Name = @"deposit_address")]
        public ExchangeAddress DepositAddress { get; set; }

        [global::ProtoBuf.ProtoMember(2, Name = @"deposit_amount")]
        public ulong DepositAmount
        {
            get { return __pbn__DepositAmount.GetValueOrDefault(); }
            set { __pbn__DepositAmount = value; }
        }
        public bool ShouldSerializeDepositAmount() => __pbn__DepositAmount != null;
        public void ResetDepositAmount() => __pbn__DepositAmount = null;
        private ulong? __pbn__DepositAmount;

        [global::ProtoBuf.ProtoMember(3, Name = @"expiration")]
        public long Expiration
        {
            get { return __pbn__Expiration.GetValueOrDefault(); }
            set { __pbn__Expiration = value; }
        }
        public bool ShouldSerializeExpiration() => __pbn__Expiration != null;
        public void ResetExpiration() => __pbn__Expiration = null;
        private long? __pbn__Expiration;

        [global::ProtoBuf.ProtoMember(4, Name = @"quoted_rate")]
        public ulong QuotedRate
        {
            get { return __pbn__QuotedRate.GetValueOrDefault(); }
            set { __pbn__QuotedRate = value; }
        }
        public bool ShouldSerializeQuotedRate() => __pbn__QuotedRate != null;
        public void ResetQuotedRate() => __pbn__QuotedRate = null;
        private ulong? __pbn__QuotedRate;

        [global::ProtoBuf.ProtoMember(5, Name = @"withdrawal_address")]
        public ExchangeAddress WithdrawalAddress { get; set; }

        [global::ProtoBuf.ProtoMember(6, Name = @"withdrawal_amount")]
        public ulong WithdrawalAmount
        {
            get { return __pbn__WithdrawalAmount.GetValueOrDefault(); }
            set { __pbn__WithdrawalAmount = value; }
        }
        public bool ShouldSerializeWithdrawalAmount() => __pbn__WithdrawalAmount != null;
        public void ResetWithdrawalAmount() => __pbn__WithdrawalAmount = null;
        private ulong? __pbn__WithdrawalAmount;

        [global::ProtoBuf.ProtoMember(7, Name = @"return_address")]
        public ExchangeAddress ReturnAddress { get; set; }

        [global::ProtoBuf.ProtoMember(8, Name = @"api_key")]
        public byte[] ApiKey
        {
            get { return __pbn__ApiKey; }
            set { __pbn__ApiKey = value; }
        }
        public bool ShouldSerializeApiKey() => __pbn__ApiKey != null;
        public void ResetApiKey() => __pbn__ApiKey = null;
        private byte[] __pbn__ApiKey;

        [global::ProtoBuf.ProtoMember(9, Name = @"miner_fee")]
        public ulong MinerFee
        {
            get { return __pbn__MinerFee.GetValueOrDefault(); }
            set { __pbn__MinerFee = value; }
        }
        public bool ShouldSerializeMinerFee() => __pbn__MinerFee != null;
        public void ResetMinerFee() => __pbn__MinerFee = null;
        private ulong? __pbn__MinerFee;

        [global::ProtoBuf.ProtoMember(10, Name = @"order_id")]
        public byte[] OrderId
        {
            get { return __pbn__OrderId; }
            set { __pbn__OrderId = value; }
        }
        public bool ShouldSerializeOrderId() => __pbn__OrderId != null;
        public void ResetOrderId() => __pbn__OrderId = null;
        private byte[] __pbn__OrderId;

    }

}

#pragma warning restore CS0612, CS1591, CS3021, IDE1006, RCS1036, RCS1057, RCS1085, RCS1192
