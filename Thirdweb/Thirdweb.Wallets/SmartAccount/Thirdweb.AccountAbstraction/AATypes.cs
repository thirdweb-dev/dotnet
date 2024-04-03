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

    [Function("createAccount", "address")]
    public class CreateAccountFunction : FunctionMessage
    {
        [Parameter("address", "_admin", 1)]
        public virtual string Admin { get; set; }

        [Parameter("bytes", "_data", 2)]
        public virtual byte[] Data { get; set; }
    }

    public class EthEstimateUserOperationGasResponse
    {
        public string PreVerificationGas { get; set; }
        public string VerificationGas { get; set; }
        public string CallGasLimit { get; set; }
    }

    public class EthGetUserOperationByHashResponse
    {
        public string entryPoint { get; set; }
        public string transactionHash { get; set; }
        public string blockHash { get; set; }
        public string blockNumber { get; set; }
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
}
