using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;

namespace Thirdweb.AccountAbstraction
{
    public class UserOperation
    {
        [Parameter("address", "sender", 1)]
        public virtual string Sender { get; set; }

        [Parameter("uint256", "nonce", 2)]
        public virtual BigInteger Nonce { get; set; }

        [Parameter("bytes", "initCode", 3)]
        public virtual byte[] InitCode { get; set; }

        [Parameter("bytes", "callData", 4)]
        public virtual byte[] CallData { get; set; }

        [Parameter("uint256", "callGasLimit", 5)]
        public virtual BigInteger CallGasLimit { get; set; }

        [Parameter("uint256", "verificationGasLimit", 6)]
        public virtual BigInteger VerificationGasLimit { get; set; }

        [Parameter("uint256", "preVerificationGas", 7)]
        public virtual BigInteger PreVerificationGas { get; set; }

        [Parameter("uint256", "maxFeePerGas", 8)]
        public virtual BigInteger MaxFeePerGas { get; set; }

        [Parameter("uint256", "maxPriorityFeePerGas", 9)]
        public virtual BigInteger MaxPriorityFeePerGas { get; set; }

        [Parameter("bytes", "paymasterAndData", 10)]
        public virtual byte[] PaymasterAndData { get; set; }

        [Parameter("bytes", "signature", 11)]
        public virtual byte[] Signature { get; set; }
    }

    public class UserOperationHexified
    {
        public string sender { get; set; }
        public string nonce { get; set; }
        public string initCode { get; set; }
        public string callData { get; set; }
        public string callGasLimit { get; set; }
        public string verificationGasLimit { get; set; }
        public string preVerificationGas { get; set; }
        public string maxFeePerGas { get; set; }
        public string maxPriorityFeePerGas { get; set; }
        public string paymasterAndData { get; set; }
        public string signature { get; set; }
    }

    [Function("execute")]
    public class ExecuteFunction : FunctionMessage
    {
        [Parameter("address", "_target", 1)]
        public virtual string Target { get; set; }

        [Parameter("uint256", "_value", 2)]
        public virtual BigInteger Value { get; set; }

        [Parameter("bytes", "_calldata", 3)]
        public virtual byte[] Calldata { get; set; }
    }

    public class EthEstimateUserOperationGasResponse
    {
        public string PreVerificationGas { get; set; }
        public string VerificationGas { get; set; }
        public string CallGasLimit { get; set; }
    }

    public class EthGetUserOperationReceiptResponse
    {
        public TransactionReceipt receipt { get; set; }
    }

    public class EntryPointWrapper
    {
        public string entryPoint { get; set; }
    }

    public class PMSponsorOperationResponse
    {
        public string paymasterAndData { get; set; }
    }

    public class ThirdwebGetUserOperationGasPriceResponse
    {
        public string maxFeePerGas { get; set; }
        public string maxPriorityFeePerGas { get; set; }
    }

    [Event("UserOperationEvent")]
    public class UserOperationEventEventDTO : IEventDTO
    {
        [Parameter("bytes32", "userOpHash", 1, true)]
        public virtual byte[] UserOpHash { get; set; }

        [Parameter("address", "sender", 2, true)]
        public virtual string Sender { get; set; }

        [Parameter("address", "paymaster", 3, true)]
        public virtual string Paymaster { get; set; }

        [Parameter("uint256", "nonce", 4, false)]
        public virtual BigInteger Nonce { get; set; }

        [Parameter("bool", "success", 5, false)]
        public virtual bool Success { get; set; }

        [Parameter("uint256", "actualGasCost", 6, false)]
        public virtual BigInteger ActualGasCost { get; set; }

        [Parameter("uint256", "actualGasUsed", 7, false)]
        public virtual BigInteger ActualGasUsed { get; set; }
    }

    [Event("UserOperationRevertReason")]
    public class UserOperationRevertReasonEventDTO : IEventDTO
    {
        [Parameter("bytes32", "userOpHash", 1, true)]
        public virtual byte[] UserOpHash { get; set; }

        [Parameter("address", "sender", 2, true)]
        public virtual string Sender { get; set; }

        [Parameter("uint256", "nonce", 3, false)]
        public virtual BigInteger Nonce { get; set; }

        [Parameter("bytes", "revertReason", 4, false)]
        public virtual byte[] RevertReason { get; set; }
    }

    public class SignerPermissionRequest
    {
        [Parameter("address", "signer", 1)]
        public virtual string Signer { get; set; }

        [Parameter("uint8", "isAdmin", 2)]
        public virtual byte IsAdmin { get; set; }

        [Parameter("address[]", "approvedTargets", 3)]
        public virtual List<string> ApprovedTargets { get; set; }

        [Parameter("uint256", "nativeTokenLimitPerTransaction", 4)]
        public virtual BigInteger NativeTokenLimitPerTransaction { get; set; }

        [Parameter("uint128", "permissionStartTimestamp", 5)]
        public virtual BigInteger PermissionStartTimestamp { get; set; }

        [Parameter("uint128", "permissionEndTimestamp", 6)]
        public virtual BigInteger PermissionEndTimestamp { get; set; }

        [Parameter("uint128", "reqValidityStartTimestamp", 7)]
        public virtual BigInteger ReqValidityStartTimestamp { get; set; }

        [Parameter("uint128", "reqValidityEndTimestamp", 8)]
        public virtual BigInteger ReqValidityEndTimestamp { get; set; }

        [Parameter("bytes32", "uid", 9)]
        public virtual byte[] Uid { get; set; }
    }

    public class AccountMessage
    {
        [Parameter("bytes", "message", 1)]
        public virtual byte[] Message { get; set; }
    }

    [Struct("Transaction")]
    public class ZkSyncAATransaction
    {
        [Parameter("uint256", "txType", 1)]
        public virtual BigInteger TxType { get; set; }

        [Parameter("uint256", "from", 2)]
        public virtual BigInteger From { get; set; }

        [Parameter("uint256", "to", 3)]
        public virtual BigInteger To { get; set; }

        [Parameter("uint256", "gasLimit", 4)]
        public virtual BigInteger GasLimit { get; set; }

        [Parameter("uint256", "gasPerPubdataByteLimit", 5)]
        public virtual BigInteger GasPerPubdataByteLimit { get; set; }

        [Parameter("uint256", "maxFeePerGas", 6)]
        public virtual BigInteger MaxFeePerGas { get; set; }

        [Parameter("uint256", "maxPriorityFeePerGas", 7)]
        public virtual BigInteger MaxPriorityFeePerGas { get; set; }

        [Parameter("uint256", "paymaster", 8)]
        public virtual BigInteger Paymaster { get; set; }

        [Parameter("uint256", "nonce", 9)]
        public virtual BigInteger Nonce { get; set; }

        [Parameter("uint256", "value", 10)]
        public virtual BigInteger Value { get; set; }

        [Parameter("bytes", "data", 11)]
        public virtual byte[] Data { get; set; }

        [Parameter("bytes32[]", "factoryDeps", 12)]
        public virtual List<byte[]> FactoryDeps { get; set; }

        [Parameter("bytes", "paymasterInput", 13)]
        public virtual byte[] PaymasterInput { get; set; }
    }
}
