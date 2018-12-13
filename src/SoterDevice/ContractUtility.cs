/*
 * Copyright (C) 2018 Touchjet Limited.
 * 
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/
using System;
using SoterDevice.Contracts;

namespace SoterDevice
{
    public static class ContractUtility
    {
        public static Type GetContractType(MessageType messageType)
        {
            switch (messageType)
            {
                case MessageType.MessageTypeAddress:
                    return typeof(Address);
                case MessageType.MessageTypeGetAddress:
                    return typeof(GetAddress);
                case MessageType.MessageTypeButtonAck:
                    return typeof(ButtonAck);
                case MessageType.MessageTypeButtonRequest:
                    return typeof(ButtonRequest);
                case MessageType.MessageTypePublicKey:
                    return typeof(PublicKey);
                case MessageType.MessageTypeFeatures:
                    return typeof(Features);
                case MessageType.MessageTypePinMatrixAck:
                    return typeof(PinMatrixAck);
                case MessageType.MessageTypePinMatrixRequest:
                    return typeof(PinMatrixRequest);
                case MessageType.MessageTypeApplySettings:
                    return typeof(ApplySettings);
                case MessageType.MessageTypeCancel:
                    return typeof(Cancel);
                case MessageType.MessageTypeSuccess:
                    return typeof(Success);
                case MessageType.MessageTypeTxAck:
                    return typeof(TxAck);
                case MessageType.MessageTypeTxRequest:
                    return typeof(TxRequest);
                case MessageType.MessageTypeVerifyMessage:
                    return typeof(VerifyMessage);
                case MessageType.MessageTypeWipeDevice:
                    return typeof(WipeDevice);
                case MessageType.MessageTypeWordAck:
                    return typeof(WordAck);
                case MessageType.MessageTypeWordRequest:
                    return typeof(WordRequest);
                case MessageType.MessageTypeInitialize:
                    return typeof(Initialize);
                case MessageType.MessageTypePing:
                    return typeof(Ping);
                case MessageType.MessageTypeFailure:
                    return typeof(Failure);
                case MessageType.MessageTypeChangePin:
                    return typeof(ChangePin);
                case MessageType.MessageTypeGetEntropy:
                    return typeof(GetEntropy);
                case MessageType.MessageTypeEntropy:
                    return typeof(Entropy);
                case MessageType.MessageTypeLoadDevice:
                    return typeof(LoadDevice);
                case MessageType.MessageTypeResetDevice:
                    return typeof(ResetDevice);
                case MessageType.MessageTypeClearSession:
                    return typeof(ClearSession);
                case MessageType.MessageTypeEntropyRequest:
                    return typeof(EntropyRequest);
                case MessageType.MessageTypeEntropyAck:
                    return typeof(EntropyAck);
                case MessageType.MessageTypePassphraseRequest:
                    return typeof(PassphraseRequest);
                case MessageType.MessageTypePassphraseAck:
                    return typeof(PassphraseAck);
                case MessageType.MessageTypeRecoveryDevice:
                    return typeof(RecoveryDevice);
                case MessageType.MessageTypeGetFeatures:
                    return typeof(GetFeatures);
                case MessageType.MessageTypeFirmwareErase:
                    return typeof(FirmwareErase);
                case MessageType.MessageTypeFirmwareUpload:
                    return typeof(FirmwareUpload);
                case MessageType.MessageTypeGetPublicKey:
                    return typeof(GetPublicKey);
                case MessageType.MessageTypeEstimateTxSize:
                    return typeof(EstimateTxSize);
                case MessageType.MessageTypeTxSize:
                    return typeof(TxSize);
                case MessageType.MessageTypeSignTx:
                    return typeof(SignTx);
                case MessageType.MessageTypeSimpleSignTx:
                    return typeof(SimpleSignTx);
                case MessageType.MessageTypeSignMessage:
                    return typeof(SignMessage);
                case MessageType.MessageTypeMessageSignature:
                    return typeof(MessageSignature);
                case MessageType.MessageTypeCipherKeyValue:
                    return typeof(CipherKeyValue);
                case MessageType.MessageTypeCipheredKeyValue:
                    return typeof(CipheredKeyValue);
                case MessageType.MessageTypeSignIdentity:
                    return typeof(SignIdentity);
                case MessageType.MessageTypeSignedIdentity:
                    return typeof(SignedIdentity);
                case MessageType.MessageTypeDebugLinkDecision:
                    return typeof(DebugLinkDecision);
                case MessageType.MessageTypeDebugLinkGetState:
                    return typeof(DebugLinkGetState);
                case MessageType.MessageTypeDebugLinkState:
                    return typeof(DebugLinkState);
                case MessageType.MessageTypeDebugLinkStop:
                    return typeof(DebugLinkStop);
                case MessageType.MessageTypeDebugLinkLog:
                    return typeof(DebugLinkLog);
                case MessageType.MessageTypeEthereumGetAddress:
                    return typeof(EthereumGetAddress);
                case MessageType.MessageTypeEthereumAddress:
                    return typeof(EthereumAddress);
                case MessageType.MessageTypeEthereumSignTx:
                    return typeof(EthereumSignTx);
                case MessageType.MessageTypeEthereumTxRequest:
                    return typeof(EthereumTxRequest);
                case MessageType.MessageTypeEthereumTxAck:
                    return typeof(EthereumTxAck);
                case MessageType.MessageTypeGetCoinTable:
                    return typeof(GetCoinTable);
                case MessageType.MessageTypeCoinTable:
                    return typeof(CoinTable);
                case MessageType.MessageTypeEthereumSignMessage:
                    return typeof(EthereumSignMessage);
                case MessageType.MessageTypeEthereumVerifyMessage:
                    return typeof(EthereumVerifyMessage);
                case MessageType.MessageTypeEthereumMessageSignature:
                    return typeof(EthereumMessageSignature);
                case MessageType.MessageTypeEncryptMessage:
                    return typeof(EncryptMessage);
                case MessageType.MessageTypeEncryptedMessage:
                    return typeof(EncryptedMessage);
                case MessageType.MessageTypeDecryptMessage:
                    return typeof(DecryptMessage);
                case MessageType.MessageTypeDecryptedMessage:
                    return typeof(DecryptedMessage);
                case MessageType.MessageTypeCharacterRequest:
                    return typeof(CharacterRequest);
                case MessageType.MessageTypeCharacterAck:
                    return typeof(CharacterAck);
                case MessageType.MessageTypeRawTxAck:
                    return typeof(RawTxAck);
                case MessageType.MessageTypeApplyPolicies:
                    return typeof(ApplyPolicies);
                case MessageType.MessageTypeFlashHash:
                    return typeof(FlashHash);
                case MessageType.MessageTypeFlashWrite:
                    return typeof(FlashWrite);
                case MessageType.MessageTypeFlashHashResponse:
                    return typeof(FlashHashResponse);
                case MessageType.MessageTypeDebugLinkFlashDump:
                    return typeof(DebugLinkFlashDump);
                case MessageType.MessageTypeDebugLinkFlashDumpResponse:
                    return typeof(DebugLinkFlashDumpResponse);
                case MessageType.MessageTypeSoftReset:
                    return typeof(SoftReset);
                case MessageType.MessageTypeDebugLinkFillConfig:
                    return typeof(DebugLinkFillConfig);
                default:
                    throw new NotImplementedException($"{messageType} not implemented");
            }
        }
    }
}
