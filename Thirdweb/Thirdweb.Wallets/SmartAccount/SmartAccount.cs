using System.Numerics;
using System.Security.Cryptography;
using Nethereum.ABI.EIP712;
using Nethereum.Contracts;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.Client.RpcMessages;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;
using Newtonsoft.Json;
using Thirdweb.AccountAbstraction;

namespace Thirdweb
{
    public class SmartAccount : IThirdwebAccount
    {
        public ThirdwebAccountType AccountType => ThirdwebAccountType.SmartAccount;

        private ThirdwebClient _client;
        private IThirdwebAccount _personalAccount;
        private string _factoryAddress;
        private bool _gasless;
        private ThirdwebContract _factoryContract;
        private ThirdwebContract _accountContract;
        private ThirdwebContract _entryPointContract;
        private BigInteger _chainId;
        private string _bundlerUrl;
        private string _paymasterUrl;
        private string _entryPoint;
        private string _accountAddressOverride;

        public SmartAccount(
            ThirdwebClient client,
            IThirdwebAccount personalAccount,
            string factoryAddress,
            bool gasless,
            BigInteger chainId,
            string accountAddressOverride = null,
            string entryPoint = null,
            string bundlerUrl = null,
            string paymasterUrl = null
        )
        {
            _client = client;
            _personalAccount = personalAccount;
            _factoryAddress = factoryAddress;
            _gasless = gasless;
            _chainId = chainId;
            _accountAddressOverride = accountAddressOverride;
            _entryPoint ??= $"0x5FF137D4b0FDCD49DcA30c7CF57E578a026d2789"; // v0.6.0
            _bundlerUrl ??= $"https://{chainId}.bundler.thirdweb.com";
            _paymasterUrl ??= $"https://{chainId}.bundler.thirdweb.com";
        }

        public async Task Connect()
        {
            if (!await _personalAccount.IsConnected())
            {
                throw new Exception("SmartAccount.Connect: Personal account must be connected.");
            }

            _entryPointContract = new ThirdwebContract(
                _client,
                _entryPoint,
                _chainId,
                "[{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"preOpGas\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"paid\",\"type\":\"uint256\"},{\"internalType\":\"uint48\",\"name\":\"validAfter\",\"type\":\"uint48\"},{\"internalType\":\"uint48\",\"name\":\"validUntil\",\"type\":\"uint48\"},{\"internalType\":\"bool\",\"name\":\"targetSuccess\",\"type\":\"bool\"},{\"internalType\":\"bytes\",\"name\":\"targetResult\",\"type\":\"bytes\"}],\"name\":\"ExecutionResult\",\"type\":\"error\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"opIndex\",\"type\":\"uint256\"},{\"internalType\":\"string\",\"name\":\"reason\",\"type\":\"string\"}],\"name\":\"FailedOp\",\"type\":\"error\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"sender\",\"type\":\"address\"}],\"name\":\"SenderAddressResult\",\"type\":\"error\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"aggregator\",\"type\":\"address\"}],\"name\":\"SignatureValidationFailed\",\"type\":\"error\"},{\"inputs\":[{\"components\":[{\"internalType\":\"uint256\",\"name\":\"preOpGas\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"prefund\",\"type\":\"uint256\"},{\"internalType\":\"bool\",\"name\":\"sigFailed\",\"type\":\"bool\"},{\"internalType\":\"uint48\",\"name\":\"validAfter\",\"type\":\"uint48\"},{\"internalType\":\"uint48\",\"name\":\"validUntil\",\"type\":\"uint48\"},{\"internalType\":\"bytes\",\"name\":\"paymasterContext\",\"type\":\"bytes\"}],\"internalType\":\"struct IEntryPoint.ReturnInfo\",\"name\":\"returnInfo\",\"type\":\"tuple\"},{\"components\":[{\"internalType\":\"uint256\",\"name\":\"stake\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"unstakeDelaySec\",\"type\":\"uint256\"}],\"internalType\":\"struct IStakeManager.StakeInfo\",\"name\":\"senderInfo\",\"type\":\"tuple\"},{\"components\":[{\"internalType\":\"uint256\",\"name\":\"stake\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"unstakeDelaySec\",\"type\":\"uint256\"}],\"internalType\":\"struct IStakeManager.StakeInfo\",\"name\":\"factoryInfo\",\"type\":\"tuple\"},{\"components\":[{\"internalType\":\"uint256\",\"name\":\"stake\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"unstakeDelaySec\",\"type\":\"uint256\"}],\"internalType\":\"struct IStakeManager.StakeInfo\",\"name\":\"paymasterInfo\",\"type\":\"tuple\"}],\"name\":\"ValidationResult\",\"type\":\"error\"},{\"inputs\":[{\"components\":[{\"internalType\":\"uint256\",\"name\":\"preOpGas\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"prefund\",\"type\":\"uint256\"},{\"internalType\":\"bool\",\"name\":\"sigFailed\",\"type\":\"bool\"},{\"internalType\":\"uint48\",\"name\":\"validAfter\",\"type\":\"uint48\"},{\"internalType\":\"uint48\",\"name\":\"validUntil\",\"type\":\"uint48\"},{\"internalType\":\"bytes\",\"name\":\"paymasterContext\",\"type\":\"bytes\"}],\"internalType\":\"struct IEntryPoint.ReturnInfo\",\"name\":\"returnInfo\",\"type\":\"tuple\"},{\"components\":[{\"internalType\":\"uint256\",\"name\":\"stake\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"unstakeDelaySec\",\"type\":\"uint256\"}],\"internalType\":\"struct IStakeManager.StakeInfo\",\"name\":\"senderInfo\",\"type\":\"tuple\"},{\"components\":[{\"internalType\":\"uint256\",\"name\":\"stake\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"unstakeDelaySec\",\"type\":\"uint256\"}],\"internalType\":\"struct IStakeManager.StakeInfo\",\"name\":\"factoryInfo\",\"type\":\"tuple\"},{\"components\":[{\"internalType\":\"uint256\",\"name\":\"stake\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"unstakeDelaySec\",\"type\":\"uint256\"}],\"internalType\":\"struct IStakeManager.StakeInfo\",\"name\":\"paymasterInfo\",\"type\":\"tuple\"},{\"components\":[{\"internalType\":\"address\",\"name\":\"aggregator\",\"type\":\"address\"},{\"components\":[{\"internalType\":\"uint256\",\"name\":\"stake\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"unstakeDelaySec\",\"type\":\"uint256\"}],\"internalType\":\"struct IStakeManager.StakeInfo\",\"name\":\"stakeInfo\",\"type\":\"tuple\"}],\"internalType\":\"struct IEntryPoint.AggregatorStakeInfo\",\"name\":\"aggregatorInfo\",\"type\":\"tuple\"}],\"name\":\"ValidationResultWithAggregation\",\"type\":\"error\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"bytes32\",\"name\":\"userOpHash\",\"type\":\"bytes32\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"sender\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"address\",\"name\":\"factory\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"address\",\"name\":\"paymaster\",\"type\":\"address\"}],\"name\":\"AccountDeployed\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[],\"name\":\"BeforeExecution\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"totalDeposit\",\"type\":\"uint256\"}],\"name\":\"Deposited\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"aggregator\",\"type\":\"address\"}],\"name\":\"SignatureAggregatorChanged\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"totalStaked\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"unstakeDelaySec\",\"type\":\"uint256\"}],\"name\":\"StakeLocked\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"withdrawTime\",\"type\":\"uint256\"}],\"name\":\"StakeUnlocked\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"address\",\"name\":\"withdrawAddress\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"}],\"name\":\"StakeWithdrawn\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"bytes32\",\"name\":\"userOpHash\",\"type\":\"bytes32\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"sender\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"paymaster\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"nonce\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"bool\",\"name\":\"success\",\"type\":\"bool\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"actualGasCost\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"actualGasUsed\",\"type\":\"uint256\"}],\"name\":\"UserOperationEvent\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"bytes32\",\"name\":\"userOpHash\",\"type\":\"bytes32\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"sender\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"nonce\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"bytes\",\"name\":\"revertReason\",\"type\":\"bytes\"}],\"name\":\"UserOperationRevertReason\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"address\",\"name\":\"withdrawAddress\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"}],\"name\":\"Withdrawn\",\"type\":\"event\"},{\"inputs\":[],\"name\":\"SIG_VALIDATION_FAILED\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"bytes\",\"name\":\"initCode\",\"type\":\"bytes\"},{\"internalType\":\"address\",\"name\":\"sender\",\"type\":\"address\"},{\"internalType\":\"bytes\",\"name\":\"paymasterAndData\",\"type\":\"bytes\"}],\"name\":\"_validateSenderAndPaymaster\",\"outputs\":[],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint32\",\"name\":\"unstakeDelaySec\",\"type\":\"uint32\"}],\"name\":\"addStake\",\"outputs\":[],\"stateMutability\":\"payable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"balanceOf\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"depositTo\",\"outputs\":[],\"stateMutability\":\"payable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"name\":\"deposits\",\"outputs\":[{\"internalType\":\"uint112\",\"name\":\"deposit\",\"type\":\"uint112\"},{\"internalType\":\"bool\",\"name\":\"staked\",\"type\":\"bool\"},{\"internalType\":\"uint112\",\"name\":\"stake\",\"type\":\"uint112\"},{\"internalType\":\"uint32\",\"name\":\"unstakeDelaySec\",\"type\":\"uint32\"},{\"internalType\":\"uint48\",\"name\":\"withdrawTime\",\"type\":\"uint48\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"getDepositInfo\",\"outputs\":[{\"components\":[{\"internalType\":\"uint112\",\"name\":\"deposit\",\"type\":\"uint112\"},{\"internalType\":\"bool\",\"name\":\"staked\",\"type\":\"bool\"},{\"internalType\":\"uint112\",\"name\":\"stake\",\"type\":\"uint112\"},{\"internalType\":\"uint32\",\"name\":\"unstakeDelaySec\",\"type\":\"uint32\"},{\"internalType\":\"uint48\",\"name\":\"withdrawTime\",\"type\":\"uint48\"}],\"internalType\":\"struct IStakeManager.DepositInfo\",\"name\":\"info\",\"type\":\"tuple\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"sender\",\"type\":\"address\"},{\"internalType\":\"uint192\",\"name\":\"key\",\"type\":\"uint192\"}],\"name\":\"getNonce\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"nonce\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"bytes\",\"name\":\"initCode\",\"type\":\"bytes\"}],\"name\":\"getSenderAddress\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"components\":[{\"internalType\":\"address\",\"name\":\"sender\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"nonce\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"initCode\",\"type\":\"bytes\"},{\"internalType\":\"bytes\",\"name\":\"callData\",\"type\":\"bytes\"},{\"internalType\":\"uint256\",\"name\":\"callGasLimit\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"verificationGasLimit\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"preVerificationGas\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"maxFeePerGas\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"maxPriorityFeePerGas\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"paymasterAndData\",\"type\":\"bytes\"},{\"internalType\":\"bytes\",\"name\":\"signature\",\"type\":\"bytes\"}],\"internalType\":\"struct UserOperation\",\"name\":\"userOp\",\"type\":\"tuple\"}],\"name\":\"getUserOpHash\",\"outputs\":[{\"internalType\":\"bytes32\",\"name\":\"\",\"type\":\"bytes32\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"components\":[{\"components\":[{\"internalType\":\"address\",\"name\":\"sender\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"nonce\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"initCode\",\"type\":\"bytes\"},{\"internalType\":\"bytes\",\"name\":\"callData\",\"type\":\"bytes\"},{\"internalType\":\"uint256\",\"name\":\"callGasLimit\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"verificationGasLimit\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"preVerificationGas\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"maxFeePerGas\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"maxPriorityFeePerGas\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"paymasterAndData\",\"type\":\"bytes\"},{\"internalType\":\"bytes\",\"name\":\"signature\",\"type\":\"bytes\"}],\"internalType\":\"struct UserOperation[]\",\"name\":\"userOps\",\"type\":\"tuple[]\"},{\"internalType\":\"contract IAggregator\",\"name\":\"aggregator\",\"type\":\"address\"},{\"internalType\":\"bytes\",\"name\":\"signature\",\"type\":\"bytes\"}],\"internalType\":\"struct IEntryPoint.UserOpsPerAggregator[]\",\"name\":\"opsPerAggregator\",\"type\":\"tuple[]\"},{\"internalType\":\"address payable\",\"name\":\"beneficiary\",\"type\":\"address\"}],\"name\":\"handleAggregatedOps\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"components\":[{\"internalType\":\"address\",\"name\":\"sender\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"nonce\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"initCode\",\"type\":\"bytes\"},{\"internalType\":\"bytes\",\"name\":\"callData\",\"type\":\"bytes\"},{\"internalType\":\"uint256\",\"name\":\"callGasLimit\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"verificationGasLimit\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"preVerificationGas\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"maxFeePerGas\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"maxPriorityFeePerGas\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"paymasterAndData\",\"type\":\"bytes\"},{\"internalType\":\"bytes\",\"name\":\"signature\",\"type\":\"bytes\"}],\"internalType\":\"struct UserOperation[]\",\"name\":\"ops\",\"type\":\"tuple[]\"},{\"internalType\":\"address payable\",\"name\":\"beneficiary\",\"type\":\"address\"}],\"name\":\"handleOps\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint192\",\"name\":\"key\",\"type\":\"uint192\"}],\"name\":\"incrementNonce\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"bytes\",\"name\":\"callData\",\"type\":\"bytes\"},{\"components\":[{\"components\":[{\"internalType\":\"address\",\"name\":\"sender\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"nonce\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"callGasLimit\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"verificationGasLimit\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"preVerificationGas\",\"type\":\"uint256\"},{\"internalType\":\"address\",\"name\":\"paymaster\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"maxFeePerGas\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"maxPriorityFeePerGas\",\"type\":\"uint256\"}],\"internalType\":\"struct EntryPoint.MemoryUserOp\",\"name\":\"mUserOp\",\"type\":\"tuple\"},{\"internalType\":\"bytes32\",\"name\":\"userOpHash\",\"type\":\"bytes32\"},{\"internalType\":\"uint256\",\"name\":\"prefund\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"contextOffset\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"preOpGas\",\"type\":\"uint256\"}],\"internalType\":\"struct EntryPoint.UserOpInfo\",\"name\":\"opInfo\",\"type\":\"tuple\"},{\"internalType\":\"bytes\",\"name\":\"context\",\"type\":\"bytes\"}],\"name\":\"innerHandleOp\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"actualGasCost\",\"type\":\"uint256\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"},{\"internalType\":\"uint192\",\"name\":\"\",\"type\":\"uint192\"}],\"name\":\"nonceSequenceNumber\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"components\":[{\"internalType\":\"address\",\"name\":\"sender\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"nonce\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"initCode\",\"type\":\"bytes\"},{\"internalType\":\"bytes\",\"name\":\"callData\",\"type\":\"bytes\"},{\"internalType\":\"uint256\",\"name\":\"callGasLimit\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"verificationGasLimit\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"preVerificationGas\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"maxFeePerGas\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"maxPriorityFeePerGas\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"paymasterAndData\",\"type\":\"bytes\"},{\"internalType\":\"bytes\",\"name\":\"signature\",\"type\":\"bytes\"}],\"internalType\":\"struct UserOperation\",\"name\":\"op\",\"type\":\"tuple\"},{\"internalType\":\"address\",\"name\":\"target\",\"type\":\"address\"},{\"internalType\":\"bytes\",\"name\":\"targetCallData\",\"type\":\"bytes\"}],\"name\":\"simulateHandleOp\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"components\":[{\"internalType\":\"address\",\"name\":\"sender\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"nonce\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"initCode\",\"type\":\"bytes\"},{\"internalType\":\"bytes\",\"name\":\"callData\",\"type\":\"bytes\"},{\"internalType\":\"uint256\",\"name\":\"callGasLimit\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"verificationGasLimit\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"preVerificationGas\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"maxFeePerGas\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"maxPriorityFeePerGas\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"paymasterAndData\",\"type\":\"bytes\"},{\"internalType\":\"bytes\",\"name\":\"signature\",\"type\":\"bytes\"}],\"internalType\":\"struct UserOperation\",\"name\":\"userOp\",\"type\":\"tuple\"}],\"name\":\"simulateValidation\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"unlockStake\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address payable\",\"name\":\"withdrawAddress\",\"type\":\"address\"}],\"name\":\"withdrawStake\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address payable\",\"name\":\"withdrawAddress\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"withdrawAmount\",\"type\":\"uint256\"}],\"name\":\"withdrawTo\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"stateMutability\":\"payable\",\"type\":\"receive\"}]"
            );
            _factoryContract = new ThirdwebContract(
                _client,
                _factoryAddress,
                _chainId,
                "[{\"type\": \"constructor\",\"name\": \"\",\"inputs\": [{\"type\": \"address\",\"name\": \"_defaultAdmin\",\"internalType\": \"address\"},{\"type\": \"address\",\"name\": \"_entrypoint\",\"internalType\": \"contract IEntryPoint\"},{\"type\": \"tuple[]\",\"name\": \"_defaultExtensions\",\"components\": [{\"type\": \"tuple\",\"name\": \"metadata\",\"components\": [{\"internalType\": \"string\",\"name\": \"name\",\"type\": \"string\"},{\"internalType\": \"string\",\"name\": \"metadataURI\",\"type\": \"string\"},{\"internalType\": \"address\",\"name\": \"implementation\",\"type\": \"address\"}],\"internalType\": \"struct IExtension.ExtensionMetadata\"},{\"type\": \"tuple[]\",\"name\": \"functions\",\"components\": [{\"internalType\": \"bytes4\",\"name\": \"functionSelector\",\"type\": \"bytes4\"},{\"internalType\": \"string\",\"name\": \"functionSignature\",\"type\": \"string\"}],\"internalType\": \"struct IExtension.ExtensionFunction[]\"}],\"internalType\": \"struct IExtension.Extension[]\"}],\"outputs\": [],\"stateMutability\": \"nonpayable\"},{\"type\": \"error\",\"name\": \"InvalidCodeAtRange\",\"inputs\": [{\"type\": \"uint256\",\"name\": \"_size\",\"internalType\": \"uint256\"},{\"type\": \"uint256\",\"name\": \"_start\",\"internalType\": \"uint256\"},{\"type\": \"uint256\",\"name\": \"_end\",\"internalType\": \"uint256\"}],\"outputs\": []},{\"type\": \"error\",\"name\": \"WriteError\",\"inputs\": [],\"outputs\": []},{\"type\": \"event\",\"name\": \"AccountCreated\",\"inputs\": [{\"type\": \"address\",\"name\": \"account\",\"indexed\": true,\"internalType\": \"address\"},{\"type\": \"address\",\"name\": \"accountAdmin\",\"indexed\": true,\"internalType\": \"address\"}],\"outputs\": [],\"anonymous\": false},{\"type\": \"event\",\"name\": \"ContractURIUpdated\",\"inputs\": [{\"type\": \"string\",\"name\": \"prevURI\",\"indexed\": false,\"internalType\": \"string\"},{\"type\": \"string\",\"name\": \"newURI\",\"indexed\": false,\"internalType\": \"string\"}],\"outputs\": [],\"anonymous\": false},{\"type\": \"event\",\"name\": \"ExtensionAdded\",\"inputs\": [{\"type\": \"string\",\"name\": \"name\",\"indexed\": true,\"internalType\": \"string\"},{\"type\": \"address\",\"name\": \"implementation\",\"indexed\": true,\"internalType\": \"address\"},{\"type\": \"tuple\",\"name\": \"extension\",\"components\": [{\"type\": \"tuple\",\"name\": \"metadata\",\"components\": [{\"internalType\": \"string\",\"name\": \"name\",\"type\": \"string\"},{\"internalType\": \"string\",\"name\": \"metadataURI\",\"type\": \"string\"},{\"internalType\": \"address\",\"name\": \"implementation\",\"type\": \"address\"}],\"internalType\": \"struct IExtension.ExtensionMetadata\"},{\"type\": \"tuple[]\",\"name\": \"functions\",\"components\": [{\"internalType\": \"bytes4\",\"name\": \"functionSelector\",\"type\": \"bytes4\"},{\"internalType\": \"string\",\"name\": \"functionSignature\",\"type\": \"string\"}],\"internalType\": \"struct IExtension.ExtensionFunction[]\"}],\"indexed\": false,\"internalType\": \"struct IExtension.Extension\"}],\"outputs\": [],\"anonymous\": false},{\"type\": \"event\",\"name\": \"ExtensionRemoved\",\"inputs\": [{\"type\": \"string\",\"name\": \"name\",\"indexed\": true,\"internalType\": \"string\"},{\"type\": \"tuple\",\"name\": \"extension\",\"components\": [{\"type\": \"tuple\",\"name\": \"metadata\",\"components\": [{\"internalType\": \"string\",\"name\": \"name\",\"type\": \"string\"},{\"internalType\": \"string\",\"name\": \"metadataURI\",\"type\": \"string\"},{\"internalType\": \"address\",\"name\": \"implementation\",\"type\": \"address\"}],\"internalType\": \"struct IExtension.ExtensionMetadata\"},{\"type\": \"tuple[]\",\"name\": \"functions\",\"components\": [{\"internalType\": \"bytes4\",\"name\": \"functionSelector\",\"type\": \"bytes4\"},{\"internalType\": \"string\",\"name\": \"functionSignature\",\"type\": \"string\"}],\"internalType\": \"struct IExtension.ExtensionFunction[]\"}],\"indexed\": false,\"internalType\": \"struct IExtension.Extension\"}],\"outputs\": [],\"anonymous\": false},{\"type\": \"event\",\"name\": \"ExtensionReplaced\",\"inputs\": [{\"type\": \"string\",\"name\": \"name\",\"indexed\": true,\"internalType\": \"string\"},{\"type\": \"address\",\"name\": \"implementation\",\"indexed\": true,\"internalType\": \"address\"},{\"type\": \"tuple\",\"name\": \"extension\",\"components\": [{\"type\": \"tuple\",\"name\": \"metadata\",\"components\": [{\"internalType\": \"string\",\"name\": \"name\",\"type\": \"string\"},{\"internalType\": \"string\",\"name\": \"metadataURI\",\"type\": \"string\"},{\"internalType\": \"address\",\"name\": \"implementation\",\"type\": \"address\"}],\"internalType\": \"struct IExtension.ExtensionMetadata\"},{\"type\": \"tuple[]\",\"name\": \"functions\",\"components\": [{\"internalType\": \"bytes4\",\"name\": \"functionSelector\",\"type\": \"bytes4\"},{\"internalType\": \"string\",\"name\": \"functionSignature\",\"type\": \"string\"}],\"internalType\": \"struct IExtension.ExtensionFunction[]\"}],\"indexed\": false,\"internalType\": \"struct IExtension.Extension\"}],\"outputs\": [],\"anonymous\": false},{\"type\": \"event\",\"name\": \"FunctionDisabled\",\"inputs\": [{\"type\": \"string\",\"name\": \"name\",\"indexed\": true,\"internalType\": \"string\"},{\"type\": \"bytes4\",\"name\": \"functionSelector\",\"indexed\": true,\"internalType\": \"bytes4\"},{\"type\": \"tuple\",\"name\": \"extMetadata\",\"components\": [{\"type\": \"string\",\"name\": \"name\",\"internalType\": \"string\"},{\"type\": \"string\",\"name\": \"metadataURI\",\"internalType\": \"string\"},{\"type\": \"address\",\"name\": \"implementation\",\"internalType\": \"address\"}],\"indexed\": false,\"internalType\": \"struct IExtension.ExtensionMetadata\"}],\"outputs\": [],\"anonymous\": false},{\"type\": \"event\",\"name\": \"FunctionEnabled\",\"inputs\": [{\"type\": \"string\",\"name\": \"name\",\"indexed\": true,\"internalType\": \"string\"},{\"type\": \"bytes4\",\"name\": \"functionSelector\",\"indexed\": true,\"internalType\": \"bytes4\"},{\"type\": \"tuple\",\"name\": \"extFunction\",\"components\": [{\"type\": \"bytes4\",\"name\": \"functionSelector\",\"internalType\": \"bytes4\"},{\"type\": \"string\",\"name\": \"functionSignature\",\"internalType\": \"string\"}],\"indexed\": false,\"internalType\": \"struct IExtension.ExtensionFunction\"},{\"type\": \"tuple\",\"name\": \"extMetadata\",\"components\": [{\"type\": \"string\",\"name\": \"name\",\"internalType\": \"string\"},{\"type\": \"string\",\"name\": \"metadataURI\",\"internalType\": \"string\"},{\"type\": \"address\",\"name\": \"implementation\",\"internalType\": \"address\"}],\"indexed\": false,\"internalType\": \"struct IExtension.ExtensionMetadata\"}],\"outputs\": [],\"anonymous\": false},{\"type\": \"event\",\"name\": \"RoleAdminChanged\",\"inputs\": [{\"type\": \"bytes32\",\"name\": \"role\",\"indexed\": true,\"internalType\": \"bytes32\"},{\"type\": \"bytes32\",\"name\": \"previousAdminRole\",\"indexed\": true,\"internalType\": \"bytes32\"},{\"type\": \"bytes32\",\"name\": \"newAdminRole\",\"indexed\": true,\"internalType\": \"bytes32\"}],\"outputs\": [],\"anonymous\": false},{\"type\": \"event\",\"name\": \"RoleGranted\",\"inputs\": [{\"type\": \"bytes32\",\"name\": \"role\",\"indexed\": true,\"internalType\": \"bytes32\"},{\"type\": \"address\",\"name\": \"account\",\"indexed\": true,\"internalType\": \"address\"},{\"type\": \"address\",\"name\": \"sender\",\"indexed\": true,\"internalType\": \"address\"}],\"outputs\": [],\"anonymous\": false},{\"type\": \"event\",\"name\": \"RoleRevoked\",\"inputs\": [{\"type\": \"bytes32\",\"name\": \"role\",\"indexed\": true,\"internalType\": \"bytes32\"},{\"type\": \"address\",\"name\": \"account\",\"indexed\": true,\"internalType\": \"address\"},{\"type\": \"address\",\"name\": \"sender\",\"indexed\": true,\"internalType\": \"address\"}],\"outputs\": [],\"anonymous\": false},{\"type\": \"event\",\"name\": \"SignerAdded\",\"inputs\": [{\"type\": \"address\",\"name\": \"account\",\"indexed\": true,\"internalType\": \"address\"},{\"type\": \"address\",\"name\": \"signer\",\"indexed\": true,\"internalType\": \"address\"}],\"outputs\": [],\"anonymous\": false},{\"type\": \"event\",\"name\": \"SignerRemoved\",\"inputs\": [{\"type\": \"address\",\"name\": \"account\",\"indexed\": true,\"internalType\": \"address\"},{\"type\": \"address\",\"name\": \"signer\",\"indexed\": true,\"internalType\": \"address\"}],\"outputs\": [],\"anonymous\": false},{\"type\": \"fallback\",\"name\": \"\",\"inputs\": [],\"outputs\": [],\"stateMutability\": \"payable\"},{\"type\": \"function\",\"name\": \"DEFAULT_ADMIN_ROLE\",\"inputs\": [],\"outputs\": [{\"type\": \"bytes32\",\"name\": \"\",\"internalType\": \"bytes32\"}],\"stateMutability\": \"view\"},{\"type\": \"function\",\"name\": \"_disableFunctionInExtension\",\"inputs\": [{\"type\": \"string\",\"name\": \"_extensionName\",\"internalType\": \"string\"},{\"type\": \"bytes4\",\"name\": \"_functionSelector\",\"internalType\": \"bytes4\"}],\"outputs\": [],\"stateMutability\": \"nonpayable\"},{\"type\": \"function\",\"name\": \"accountImplementation\",\"inputs\": [],\"outputs\": [{\"type\": \"address\",\"name\": \"\",\"internalType\": \"address\"}],\"stateMutability\": \"view\"},{\"type\": \"function\",\"name\": \"addExtension\",\"inputs\": [{\"type\": \"tuple\",\"name\": \"_extension\",\"components\": [{\"type\": \"tuple\",\"name\": \"metadata\",\"components\": [{\"internalType\": \"string\",\"name\": \"name\",\"type\": \"string\"},{\"internalType\": \"string\",\"name\": \"metadataURI\",\"type\": \"string\"},{\"internalType\": \"address\",\"name\": \"implementation\",\"type\": \"address\"}],\"internalType\": \"struct IExtension.ExtensionMetadata\"},{\"type\": \"tuple[]\",\"name\": \"functions\",\"components\": [{\"internalType\": \"bytes4\",\"name\": \"functionSelector\",\"type\": \"bytes4\"},{\"internalType\": \"string\",\"name\": \"functionSignature\",\"type\": \"string\"}],\"internalType\": \"struct IExtension.ExtensionFunction[]\"}],\"internalType\": \"struct IExtension.Extension\"}],\"outputs\": [],\"stateMutability\": \"nonpayable\"},{\"type\": \"function\",\"name\": \"contractURI\",\"inputs\": [],\"outputs\": [{\"type\": \"string\",\"name\": \"\",\"internalType\": \"string\"}],\"stateMutability\": \"view\"},{\"type\": \"function\",\"name\": \"createAccount\",\"inputs\": [{\"type\": \"address\",\"name\": \"_admin\",\"internalType\": \"address\"},{\"type\": \"bytes\",\"name\": \"_data\",\"internalType\": \"bytes\"}],\"outputs\": [{\"type\": \"address\",\"name\": \"\",\"internalType\": \"address\"}],\"stateMutability\": \"nonpayable\"},{\"type\": \"function\",\"name\": \"defaultExtensions\",\"inputs\": [],\"outputs\": [{\"type\": \"address\",\"name\": \"\",\"internalType\": \"address\"}],\"stateMutability\": \"view\"},{\"type\": \"function\",\"name\": \"disableFunctionInExtension\",\"inputs\": [{\"type\": \"string\",\"name\": \"_extensionName\",\"internalType\": \"string\"},{\"type\": \"bytes4\",\"name\": \"_functionSelector\",\"internalType\": \"bytes4\"}],\"outputs\": [],\"stateMutability\": \"nonpayable\"},{\"type\": \"function\",\"name\": \"enableFunctionInExtension\",\"inputs\": [{\"type\": \"string\",\"name\": \"_extensionName\",\"internalType\": \"string\"},{\"type\": \"tuple\",\"name\": \"_function\",\"components\": [{\"type\": \"bytes4\",\"name\": \"functionSelector\",\"internalType\": \"bytes4\"},{\"type\": \"string\",\"name\": \"functionSignature\",\"internalType\": \"string\"}],\"internalType\": \"struct IExtension.ExtensionFunction\"}],\"outputs\": [],\"stateMutability\": \"nonpayable\"},{\"type\": \"function\",\"name\": \"entrypoint\",\"inputs\": [],\"outputs\": [{\"type\": \"address\",\"name\": \"\",\"internalType\": \"address\"}],\"stateMutability\": \"view\"},{\"type\": \"function\",\"name\": \"getAccounts\",\"inputs\": [{\"type\": \"uint256\",\"name\": \"_start\",\"internalType\": \"uint256\"},{\"type\": \"uint256\",\"name\": \"_end\",\"internalType\": \"uint256\"}],\"outputs\": [{\"type\": \"address[]\",\"name\": \"accounts\",\"internalType\": \"address[]\"}],\"stateMutability\": \"view\"},{\"type\": \"function\",\"name\": \"getAccountsOfSigner\",\"inputs\": [{\"type\": \"address\",\"name\": \"signer\",\"internalType\": \"address\"}],\"outputs\": [{\"type\": \"address[]\",\"name\": \"accounts\",\"internalType\": \"address[]\"}],\"stateMutability\": \"view\"},{\"type\": \"function\",\"name\": \"getAddress\",\"inputs\": [{\"type\": \"address\",\"name\": \"_adminSigner\",\"internalType\": \"address\"},{\"type\": \"bytes\",\"name\": \"_data\",\"internalType\": \"bytes\"}],\"outputs\": [{\"type\": \"address\",\"name\": \"\",\"internalType\": \"address\"}],\"stateMutability\": \"view\"},{\"type\": \"function\",\"name\": \"getAllAccounts\",\"inputs\": [],\"outputs\": [{\"type\": \"address[]\",\"name\": \"\",\"internalType\": \"address[]\"}],\"stateMutability\": \"view\"},{\"type\": \"function\",\"name\": \"getAllExtensions\",\"inputs\": [],\"outputs\": [{\"type\": \"tuple[]\",\"name\": \"allExtensions\",\"components\": [{\"type\": \"tuple\",\"name\": \"metadata\",\"components\": [{\"internalType\": \"string\",\"name\": \"name\",\"type\": \"string\"},{\"internalType\": \"string\",\"name\": \"metadataURI\",\"type\": \"string\"},{\"internalType\": \"address\",\"name\": \"implementation\",\"type\": \"address\"}],\"internalType\": \"struct IExtension.ExtensionMetadata\"},{\"type\": \"tuple[]\",\"name\": \"functions\",\"components\": [{\"internalType\": \"bytes4\",\"name\": \"functionSelector\",\"type\": \"bytes4\"},{\"internalType\": \"string\",\"name\": \"functionSignature\",\"type\": \"string\"}],\"internalType\": \"struct IExtension.ExtensionFunction[]\"}],\"internalType\": \"struct IExtension.Extension[]\"}],\"stateMutability\": \"view\"},{\"type\": \"function\",\"name\": \"getExtension\",\"inputs\": [{\"type\": \"string\",\"name\": \"extensionName\",\"internalType\": \"string\"}],\"outputs\": [{\"type\": \"tuple\",\"name\": \"\",\"components\": [{\"type\": \"tuple\",\"name\": \"metadata\",\"components\": [{\"internalType\": \"string\",\"name\": \"name\",\"type\": \"string\"},{\"internalType\": \"string\",\"name\": \"metadataURI\",\"type\": \"string\"},{\"internalType\": \"address\",\"name\": \"implementation\",\"type\": \"address\"}],\"internalType\": \"struct IExtension.ExtensionMetadata\"},{\"type\": \"tuple[]\",\"name\": \"functions\",\"components\": [{\"internalType\": \"bytes4\",\"name\": \"functionSelector\",\"type\": \"bytes4\"},{\"internalType\": \"string\",\"name\": \"functionSignature\",\"type\": \"string\"}],\"internalType\": \"struct IExtension.ExtensionFunction[]\"}],\"internalType\": \"struct IExtension.Extension\"}],\"stateMutability\": \"view\"},{\"type\": \"function\",\"name\": \"getImplementationForFunction\",\"inputs\": [{\"type\": \"bytes4\",\"name\": \"_functionSelector\",\"internalType\": \"bytes4\"}],\"outputs\": [{\"type\": \"address\",\"name\": \"\",\"internalType\": \"address\"}],\"stateMutability\": \"view\"},{\"type\": \"function\",\"name\": \"getMetadataForFunction\",\"inputs\": [{\"type\": \"bytes4\",\"name\": \"functionSelector\",\"internalType\": \"bytes4\"}],\"outputs\": [{\"type\": \"tuple\",\"name\": \"\",\"components\": [{\"type\": \"string\",\"name\": \"name\",\"internalType\": \"string\"},{\"type\": \"string\",\"name\": \"metadataURI\",\"internalType\": \"string\"},{\"type\": \"address\",\"name\": \"implementation\",\"internalType\": \"address\"}],\"internalType\": \"struct IExtension.ExtensionMetadata\"}],\"stateMutability\": \"view\"},{\"type\": \"function\",\"name\": \"getRoleAdmin\",\"inputs\": [{\"type\": \"bytes32\",\"name\": \"role\",\"internalType\": \"bytes32\"}],\"outputs\": [{\"type\": \"bytes32\",\"name\": \"\",\"internalType\": \"bytes32\"}],\"stateMutability\": \"view\"},{\"type\": \"function\",\"name\": \"getRoleMember\",\"inputs\": [{\"type\": \"bytes32\",\"name\": \"role\",\"internalType\": \"bytes32\"},{\"type\": \"uint256\",\"name\": \"index\",\"internalType\": \"uint256\"}],\"outputs\": [{\"type\": \"address\",\"name\": \"member\",\"internalType\": \"address\"}],\"stateMutability\": \"view\"},{\"type\": \"function\",\"name\": \"getRoleMemberCount\",\"inputs\": [{\"type\": \"bytes32\",\"name\": \"role\",\"internalType\": \"bytes32\"}],\"outputs\": [{\"type\": \"uint256\",\"name\": \"count\",\"internalType\": \"uint256\"}],\"stateMutability\": \"view\"},{\"type\": \"function\",\"name\": \"grantRole\",\"inputs\": [{\"type\": \"bytes32\",\"name\": \"role\",\"internalType\": \"bytes32\"},{\"type\": \"address\",\"name\": \"account\",\"internalType\": \"address\"}],\"outputs\": [],\"stateMutability\": \"nonpayable\"},{\"type\": \"function\",\"name\": \"hasRole\",\"inputs\": [{\"type\": \"bytes32\",\"name\": \"role\",\"internalType\": \"bytes32\"},{\"type\": \"address\",\"name\": \"account\",\"internalType\": \"address\"}],\"outputs\": [{\"type\": \"bool\",\"name\": \"\",\"internalType\": \"bool\"}],\"stateMutability\": \"view\"},{\"type\": \"function\",\"name\": \"hasRoleWithSwitch\",\"inputs\": [{\"type\": \"bytes32\",\"name\": \"role\",\"internalType\": \"bytes32\"},{\"type\": \"address\",\"name\": \"account\",\"internalType\": \"address\"}],\"outputs\": [{\"type\": \"bool\",\"name\": \"\",\"internalType\": \"bool\"}],\"stateMutability\": \"view\"},{\"type\": \"function\",\"name\": \"isRegistered\",\"inputs\": [{\"type\": \"address\",\"name\": \"_account\",\"internalType\": \"address\"}],\"outputs\": [{\"type\": \"bool\",\"name\": \"\",\"internalType\": \"bool\"}],\"stateMutability\": \"view\"},{\"type\": \"function\",\"name\": \"multicall\",\"inputs\": [{\"type\": \"bytes[]\",\"name\": \"data\",\"internalType\": \"bytes[]\"}],\"outputs\": [{\"type\": \"bytes[]\",\"name\": \"results\",\"internalType\": \"bytes[]\"}],\"stateMutability\": \"nonpayable\"},{\"type\": \"function\",\"name\": \"onRegister\",\"inputs\": [{\"type\": \"bytes32\",\"name\": \"_salt\",\"internalType\": \"bytes32\"}],\"outputs\": [],\"stateMutability\": \"nonpayable\"},{\"type\": \"function\",\"name\": \"onSignerAdded\",\"inputs\": [{\"type\": \"address\",\"name\": \"_signer\",\"internalType\": \"address\"},{\"type\": \"bytes32\",\"name\": \"_salt\",\"internalType\": \"bytes32\"}],\"outputs\": [],\"stateMutability\": \"nonpayable\"},{\"type\": \"function\",\"name\": \"onSignerRemoved\",\"inputs\": [{\"type\": \"address\",\"name\": \"_signer\",\"internalType\": \"address\"},{\"type\": \"bytes32\",\"name\": \"_salt\",\"internalType\": \"bytes32\"}],\"outputs\": [],\"stateMutability\": \"nonpayable\"},{\"type\": \"function\",\"name\": \"removeExtension\",\"inputs\": [{\"type\": \"string\",\"name\": \"_extensionName\",\"internalType\": \"string\"}],\"outputs\": [],\"stateMutability\": \"nonpayable\"},{\"type\": \"function\",\"name\": \"renounceRole\",\"inputs\": [{\"type\": \"bytes32\",\"name\": \"role\",\"internalType\": \"bytes32\"},{\"type\": \"address\",\"name\": \"account\",\"internalType\": \"address\"}],\"outputs\": [],\"stateMutability\": \"nonpayable\"},{\"type\": \"function\",\"name\": \"replaceExtension\",\"inputs\": [{\"type\": \"tuple\",\"name\": \"_extension\",\"components\": [{\"type\": \"tuple\",\"name\": \"metadata\",\"components\": [{\"internalType\": \"string\",\"name\": \"name\",\"type\": \"string\"},{\"internalType\": \"string\",\"name\": \"metadataURI\",\"type\": \"string\"},{\"internalType\": \"address\",\"name\": \"implementation\",\"type\": \"address\"}],\"internalType\": \"struct IExtension.ExtensionMetadata\"},{\"type\": \"tuple[]\",\"name\": \"functions\",\"components\": [{\"internalType\": \"bytes4\",\"name\": \"functionSelector\",\"type\": \"bytes4\"},{\"internalType\": \"string\",\"name\": \"functionSignature\",\"type\": \"string\"}],\"internalType\": \"struct IExtension.ExtensionFunction[]\"}],\"internalType\": \"struct IExtension.Extension\"}],\"outputs\": [],\"stateMutability\": \"nonpayable\"},{\"type\": \"function\",\"name\": \"revokeRole\",\"inputs\": [{\"type\": \"bytes32\",\"name\": \"role\",\"internalType\": \"bytes32\"},{\"type\": \"address\",\"name\": \"account\",\"internalType\": \"address\"}],\"outputs\": [],\"stateMutability\": \"nonpayable\"},{\"type\": \"function\",\"name\": \"setContractURI\",\"inputs\": [{\"type\": \"string\",\"name\": \"_uri\",\"internalType\": \"string\"}],\"outputs\": [],\"stateMutability\": \"nonpayable\"},{\"type\": \"function\",\"name\": \"totalAccounts\",\"inputs\": [],\"outputs\": [{\"type\": \"uint256\",\"name\": \"\",\"internalType\": \"uint256\"}],\"stateMutability\": \"view\"}]"
            );
            var accountAddress = _accountAddressOverride ?? await ThirdwebContract.ReadContract<string>(_factoryContract, "getAddress", await _personalAccount.GetAddress(), new byte[0]);
            _accountContract = new ThirdwebContract(
                _client,
                accountAddress,
                _chainId,
                "[{type: \"constructor\",inputs: [{name: \"_entrypoint\",type: \"address\",internalType: \"contract IEntryPoint\",},{ name: \"_factory\", type: \"address\", internalType: \"address\" },],stateMutability: \"nonpayable\",},{ type: \"receive\", stateMutability: \"payable\" },{type: \"function\",name: \"addDeposit\",inputs: [],outputs: [],stateMutability: \"payable\",},{type: \"function\",name: \"contractURI\",inputs: [],outputs: [{ name: \"\", type: \"string\", internalType: \"string\" }],stateMutability: \"view\",},{type: \"function\",name: \"entryPoint\",inputs: [],outputs: [{ name: \"\", type: \"address\", internalType: \"contract IEntryPoint\" },],stateMutability: \"view\",},{type: \"function\",name: \"execute\",inputs: [{ name: \"_target\", type: \"address\", internalType: \"address\" },{ name: \"_value\", type: \"uint256\", internalType: \"uint256\" },{ name: \"_calldata\", type: \"bytes\", internalType: \"bytes\" },],outputs: [],stateMutability: \"nonpayable\",},{type: \"function\",name: \"executeBatch\",inputs: [{ name: \"_target\", type: \"address[]\", internalType: \"address[]\" },{ name: \"_value\", type: \"uint256[]\", internalType: \"uint256[]\" },{ name: \"_calldata\", type: \"bytes[]\", internalType: \"bytes[]\" },],outputs: [],stateMutability: \"nonpayable\",},{type: \"function\",name: \"factory\",inputs: [],outputs: [{ name: \"\", type: \"address\", internalType: \"address\" }],stateMutability: \"view\",},{type: \"function\",name: \"getAllActiveSigners\",inputs: [],outputs: [{name: \"signers\",type: \"tuple[]\",internalType: \"struct IAccountPermissions.SignerPermissions[]\",components: [{ name: \"signer\", type: \"address\", internalType: \"address\" },{name: \"approvedTargets\",type: \"address[]\",internalType: \"address[]\",},{name: \"nativeTokenLimitPerTransaction\",type: \"uint256\",internalType: \"uint256\",},{ name: \"startTimestamp\", type: \"uint128\", internalType: \"uint128\" },{ name: \"endTimestamp\", type: \"uint128\", internalType: \"uint128\" },],},],stateMutability: \"view\",},{type: \"function\",name: \"getAllAdmins\",inputs: [],outputs: [{ name: \"\", type: \"address[]\", internalType: \"address[]\" }],stateMutability: \"view\",},{type: \"function\",name: \"getAllSigners\",inputs: [],outputs: [{name: \"signers\",type: \"tuple[]\",internalType: \"struct IAccountPermissions.SignerPermissions[]\",components: [{ name: \"signer\", type: \"address\", internalType: \"address\" },{name: \"approvedTargets\",type: \"address[]\",internalType: \"address[]\",},{name: \"nativeTokenLimitPerTransaction\",type: \"uint256\",internalType: \"uint256\",},{ name: \"startTimestamp\", type: \"uint128\", internalType: \"uint128\" },{ name: \"endTimestamp\", type: \"uint128\", internalType: \"uint128\" },],},],stateMutability: \"view\",},{type: \"function\",name: \"getMessageHash\",inputs: [{ name: \"_hash\", type: \"bytes32\", internalType: \"bytes32\" }],outputs: [{ name: \"\", type: \"bytes32\", internalType: \"bytes32\" }],stateMutability: \"view\",},{type: \"function\",name: \"getNonce\",inputs: [],outputs: [{ name: \"\", type: \"uint256\", internalType: \"uint256\" }],stateMutability: \"view\",},{type: \"function\",name: \"getPermissionsForSigner\",inputs: [{ name: \"signer\", type: \"address\", internalType: \"address\" }],outputs: [{name: \"\",type: \"tuple\",internalType: \"struct IAccountPermissions.SignerPermissions\",components: [{ name: \"signer\", type: \"address\", internalType: \"address\" },{name: \"approvedTargets\",type: \"address[]\",internalType: \"address[]\",},{name: \"nativeTokenLimitPerTransaction\",type: \"uint256\",internalType: \"uint256\",},{ name: \"startTimestamp\", type: \"uint128\", internalType: \"uint128\" },{ name: \"endTimestamp\", type: \"uint128\", internalType: \"uint128\" },],},],stateMutability: \"view\",},{type: \"function\",name: \"initialize\",inputs: [{ name: \"_defaultAdmin\", type: \"address\", internalType: \"address\" },{ name: \"_data\", type: \"bytes\", internalType: \"bytes\" },],outputs: [],stateMutability: \"nonpayable\",},{type: \"function\",name: \"isActiveSigner\",inputs: [{ name: \"signer\", type: \"address\", internalType: \"address\" }],outputs: [{ name: \"\", type: \"bool\", internalType: \"bool\" }],stateMutability: \"view\",},{type: \"function\",name: \"isAdmin\",inputs: [{ name: \"_account\", type: \"address\", internalType: \"address\" }],outputs: [{ name: \"\", type: \"bool\", internalType: \"bool\" }],stateMutability: \"view\",},{type: \"function\",name: \"isValidSignature\",inputs: [{ name: \"_hash\", type: \"bytes32\", internalType: \"bytes32\" },{ name: \"_signature\", type: \"bytes\", internalType: \"bytes\" },],outputs: [{ name: \"magicValue\", type: \"bytes4\", internalType: \"bytes4\" }],stateMutability: \"view\",},{type: \"function\",name: \"isValidSigner\",inputs: [{ name: \"_signer\", type: \"address\", internalType: \"address\" },{name: \"_userOp\",type: \"tuple\",internalType: \"struct UserOperation\",components: [{ name: \"sender\", type: \"address\", internalType: \"address\" },{ name: \"nonce\", type: \"uint256\", internalType: \"uint256\" },{ name: \"initCode\", type: \"bytes\", internalType: \"bytes\" },{ name: \"callData\", type: \"bytes\", internalType: \"bytes\" },{ name: \"callGasLimit\", type: \"uint256\", internalType: \"uint256\" },{name: \"verificationGasLimit\",type: \"uint256\",internalType: \"uint256\",},{name: \"preVerificationGas\",type: \"uint256\",internalType: \"uint256\",},{ name: \"maxFeePerGas\", type: \"uint256\", internalType: \"uint256\" },{name: \"maxPriorityFeePerGas\",type: \"uint256\",internalType: \"uint256\",},{ name: \"paymasterAndData\", type: \"bytes\", internalType: \"bytes\" },{ name: \"signature\", type: \"bytes\", internalType: \"bytes\" },],},],outputs: [{ name: \"\", type: \"bool\", internalType: \"bool\" }],stateMutability: \"view\",},{type: \"function\",name: \"multicall\",inputs: [{ name: \"data\", type: \"bytes[]\", internalType: \"bytes[]\" }],outputs: [{ name: \"results\", type: \"bytes[]\", internalType: \"bytes[]\" }],stateMutability: \"nonpayable\",},{type: \"function\",name: \"onERC1155BatchReceived\",inputs: [{ name: \"\", type: \"address\", internalType: \"address\" },{ name: \"\", type: \"address\", internalType: \"address\" },{ name: \"\", type: \"uint256[]\", internalType: \"uint256[]\" },{ name: \"\", type: \"uint256[]\", internalType: \"uint256[]\" },{ name: \"\", type: \"bytes\", internalType: \"bytes\" },],outputs: [{ name: \"\", type: \"bytes4\", internalType: \"bytes4\" }],stateMutability: \"nonpayable\",},{type: \"function\",name: \"onERC1155Received\",inputs: [{ name: \"\", type: \"address\", internalType: \"address\" },{ name: \"\", type: \"address\", internalType: \"address\" },{ name: \"\", type: \"uint256\", internalType: \"uint256\" },{ name: \"\", type: \"uint256\", internalType: \"uint256\" },{ name: \"\", type: \"bytes\", internalType: \"bytes\" },],outputs: [{ name: \"\", type: \"bytes4\", internalType: \"bytes4\" }],stateMutability: \"nonpayable\",},{type: \"function\",name: \"onERC721Received\",inputs: [{ name: \"\", type: \"address\", internalType: \"address\" },{ name: \"\", type: \"address\", internalType: \"address\" },{ name: \"\", type: \"uint256\", internalType: \"uint256\" },{ name: \"\", type: \"bytes\", internalType: \"bytes\" },],outputs: [{ name: \"\", type: \"bytes4\", internalType: \"bytes4\" }],stateMutability: \"nonpayable\",},{type: \"function\",name: \"setContractURI\",inputs: [{ name: \"_uri\", type: \"string\", internalType: \"string\" }],outputs: [],stateMutability: \"nonpayable\",},{type: \"function\",name: \"setEntrypointOverride\",inputs: [{name: \"_entrypointOverride\",type: \"address\",internalType: \"contract IEntryPoint\",},],outputs: [],stateMutability: \"nonpayable\",},{type: \"function\",name: \"setPermissionsForSigner\",inputs: [{name: \"_req\",type: \"tuple\",internalType: \"struct IAccountPermissions.SignerPermissionRequest\",components: [{ name: \"signer\", type: \"address\", internalType: \"address\" },{ name: \"isAdmin\", type: \"uint8\", internalType: \"uint8\" },{name: \"approvedTargets\",type: \"address[]\",internalType: \"address[]\",},{name: \"nativeTokenLimitPerTransaction\",type: \"uint256\",internalType: \"uint256\",},{name: \"permissionStartTimestamp\",type: \"uint128\",internalType: \"uint128\",},{name: \"permissionEndTimestamp\",type: \"uint128\",internalType: \"uint128\",},{name: \"reqValidityStartTimestamp\",type: \"uint128\",internalType: \"uint128\",},{name: \"reqValidityEndTimestamp\",type: \"uint128\",internalType: \"uint128\",},{ name: \"uid\", type: \"bytes32\", internalType: \"bytes32\" },],},{ name: \"_signature\", type: \"bytes\", internalType: \"bytes\" },],outputs: [],stateMutability: \"nonpayable\",},{type: \"function\",name: \"supportsInterface\",inputs: [{ name: \"interfaceId\", type: \"bytes4\", internalType: \"bytes4\" }],outputs: [{ name: \"\", type: \"bool\", internalType: \"bool\" }],stateMutability: \"view\",},{type: \"function\",name: \"validateUserOp\",inputs: [{name: \"userOp\",type: \"tuple\",internalType: \"struct UserOperation\",components: [{ name: \"sender\", type: \"address\", internalType: \"address\" },{ name: \"nonce\", type: \"uint256\", internalType: \"uint256\" },{ name: \"initCode\", type: \"bytes\", internalType: \"bytes\" },{ name: \"callData\", type: \"bytes\", internalType: \"bytes\" },{ name: \"callGasLimit\", type: \"uint256\", internalType: \"uint256\" },{name: \"verificationGasLimit\",type: \"uint256\",internalType: \"uint256\",},{name: \"preVerificationGas\",type: \"uint256\",internalType: \"uint256\",},{ name: \"maxFeePerGas\", type: \"uint256\", internalType: \"uint256\" },{name: \"maxPriorityFeePerGas\",type: \"uint256\",internalType: \"uint256\",},{ name: \"paymasterAndData\", type: \"bytes\", internalType: \"bytes\" },{ name: \"signature\", type: \"bytes\", internalType: \"bytes\" },],},{ name: \"userOpHash\", type: \"bytes32\", internalType: \"bytes32\" },{ name: \"missingAccountFunds\", type: \"uint256\", internalType: \"uint256\" },],outputs: [{ name: \"validationData\", type: \"uint256\", internalType: \"uint256\" },],stateMutability: \"nonpayable\",},{type: \"function\",name: \"verifySignerPermissionRequest\",inputs: [{name: \"req\",type: \"tuple\",internalType: \"struct IAccountPermissions.SignerPermissionRequest\",components: [{ name: \"signer\", type: \"address\", internalType: \"address\" },{ name: \"isAdmin\", type: \"uint8\", internalType: \"uint8\" },{name: \"approvedTargets\",type: \"address[]\",internalType: \"address[]\",},{name: \"nativeTokenLimitPerTransaction\",type: \"uint256\",internalType: \"uint256\",},{name: \"permissionStartTimestamp\",type: \"uint128\",internalType: \"uint128\",},{name: \"permissionEndTimestamp\",type: \"uint128\",internalType: \"uint128\",},{name: \"reqValidityStartTimestamp\",type: \"uint128\",internalType: \"uint128\",},{name: \"reqValidityEndTimestamp\",type: \"uint128\",internalType: \"uint128\",},{ name: \"uid\", type: \"bytes32\", internalType: \"bytes32\" },],},{ name: \"signature\", type: \"bytes\", internalType: \"bytes\" },],outputs: [{ name: \"success\", type: \"bool\", internalType: \"bool\" },{ name: \"signer\", type: \"address\", internalType: \"address\" },],stateMutability: \"view\",},{type: \"function\",name: \"withdrawDepositTo\",inputs: [{name: \"withdrawAddress\",type: \"address\",internalType: \"address payable\",},{ name: \"amount\", type: \"uint256\", internalType: \"uint256\" },],outputs: [],stateMutability: \"nonpayable\",},{type: \"event\",name: \"AdminUpdated\",inputs: [{name: \"signer\",type: \"address\",indexed: true,internalType: \"address\",},{ name: \"isAdmin\", type: \"bool\", indexed: false, internalType: \"bool\" },],anonymous: false,},{type: \"event\",name: \"ContractURIUpdated\",inputs: [{name: \"prevURI\",type: \"string\",indexed: false,internalType: \"string\",},{name: \"newURI\",type: \"string\",indexed: false,internalType: \"string\",},],anonymous: false,},{type: \"event\",name: \"Initialized\",inputs: [{ name: \"version\", type: \"uint8\", indexed: false, internalType: \"uint8\" },],anonymous: false,},{type: \"event\",name: \"SignerPermissionsUpdated\",inputs: [{name: \"authorizingSigner\",type: \"address\",indexed: true,internalType: \"address\",},{name: \"targetSigner\",type: \"address\",indexed: true,internalType: \"address\",},{name: \"permissions\",type: \"tuple\",indexed: false,internalType: \"struct IAccountPermissions.SignerPermissionRequest\",components: [{ name: \"signer\", type: \"address\", internalType: \"address\" },{ name: \"isAdmin\", type: \"uint8\", internalType: \"uint8\" },{name: \"approvedTargets\",type: \"address[]\",internalType: \"address[]\",},{name: \"nativeTokenLimitPerTransaction\",type: \"uint256\",internalType: \"uint256\",},{name: \"permissionStartTimestamp\",type: \"uint128\",internalType: \"uint128\",},{name: \"permissionEndTimestamp\",type: \"uint128\",internalType: \"uint128\",},{name: \"reqValidityStartTimestamp\",type: \"uint128\",internalType: \"uint128\",},{name: \"reqValidityEndTimestamp\",type: \"uint128\",internalType: \"uint128\",},{ name: \"uid\", type: \"bytes32\", internalType: \"bytes32\" },],},],anonymous: false,},]"
            );
        }

        public async Task<bool> IsDeployed()
        {
            var code = await ThirdwebRPC.GetRpcInstance(_client, _chainId).SendRequestAsync<string>("eth_getCode", _accountContract.Address, "latest");
            return code != "0x";
        }

        public async Task<string> SendTransaction(TransactionInput transaction)
        {
            var signedOp = await SignUserOp(transaction);
            return await SendUserOp(signedOp);
        }

        private async Task<byte[]> GetInitCode()
        {
            if (await IsDeployed())
            {
                return new byte[0];
            }

            var data = new Contract(null, _factoryContract.Abi, _factoryContract.Address).GetFunction("createAccount").GetData(await _personalAccount.GetAddress(), new byte[0]);
            data = Utils.HexConcat(_factoryAddress, data);
            return data.HexToByteArray();
        }

        private async Task<UserOperation> SignUserOp(TransactionInput transactionInput, int? requestId = null)
        {
            requestId ??= 1;

            // Create the user operation and its safe (hexified) version

            var executeFn = new ExecuteFunction
            {
                Target = transactionInput.To,
                Value = transactionInput.Value.Value,
                Calldata = transactionInput.Data.HexToByteArray(),
                FromAddress = await GetAddress(),
            };
            var executeInput = executeFn.CreateTransactionInput(await GetAddress());

            var fees = await BundlerClient.ThirdwebGetUserOperationGasPrice(_client, _bundlerUrl, requestId);
            var maxFee = new HexBigInteger(fees.maxFeePerGas).Value;
            var maxPriorityFee = new HexBigInteger(fees.maxPriorityFeePerGas).Value;

            var partialUserOp = new UserOperation()
            {
                Sender = _accountContract.Address,
                Nonce = await GetNonce(),
                InitCode = await GetInitCode(),
                CallData = executeInput.Data.HexToByteArray(),
                CallGasLimit = 0,
                VerificationGasLimit = 0,
                PreVerificationGas = 0,
                MaxFeePerGas = maxFee,
                MaxPriorityFeePerGas = maxPriorityFee,
                PaymasterAndData = new byte[] { },
                Signature = Constants.DUMMY_SIG.HexToByteArray(),
            };

            // Update paymaster data if any

            partialUserOp.PaymasterAndData = await GetPaymasterAndData(requestId, EncodeUserOperation(partialUserOp));

            // Estimate gas

            var gasEstimates = await BundlerClient.EthEstimateUserOperationGas(_client, _bundlerUrl, requestId, EncodeUserOperation(partialUserOp), _entryPoint);
            partialUserOp.CallGasLimit = 50000 + new HexBigInteger(gasEstimates.CallGasLimit).Value;
            partialUserOp.VerificationGasLimit = new HexBigInteger(gasEstimates.VerificationGas).Value;
            partialUserOp.PreVerificationGas = new HexBigInteger(gasEstimates.PreVerificationGas).Value;

            // Update paymaster data if any

            partialUserOp.PaymasterAndData = await GetPaymasterAndData(requestId, EncodeUserOperation(partialUserOp));

            // Hash, sign and encode the user operation

            partialUserOp.Signature = await HashAndSignUserOp(partialUserOp, _entryPointContract);

            return partialUserOp;
        }

        private async Task<string> SendUserOp(UserOperation userOperation, int? requestId = null)
        {
            requestId ??= 1;

            // Send the user operation

            var userOpHash = await BundlerClient.EthSendUserOperation(_client, _bundlerUrl, requestId, EncodeUserOperation(userOperation), _entryPoint);

            // Wait for the transaction to be mined

            string txHash = null;
            while (txHash == null)
            {
                var userOpReceipt = await BundlerClient.EthGetUserOperationReceipt(_client, _bundlerUrl, requestId, userOpHash);
                txHash = userOpReceipt?.receipt?.TransactionHash;
                await Task.Delay(1000);
            }
            Console.WriteLine($"Transaction hash: {txHash} | UserOp hash: {userOpHash}");
            return txHash;
        }

        private async Task<BigInteger> GetNonce()
        {
            var randomBytes = new byte[24];
            RandomNumberGenerator.Fill(randomBytes);
            BigInteger randomInt192 = new(randomBytes);
            randomInt192 = BigInteger.Abs(randomInt192) % (BigInteger.One << 192);
            return await ThirdwebContract.ReadContract<BigInteger>(_entryPointContract, "getNonce", await GetAddress(), randomInt192);
        }

        private async Task<byte[]> GetPaymasterAndData(object requestId, UserOperationHexified userOp)
        {
            if (_gasless)
            {
                var paymasterAndData = await BundlerClient.PMSponsorUserOperation(_client, _paymasterUrl, requestId, userOp, _entryPoint);
                return paymasterAndData.paymasterAndData.HexToByteArray();
            }
            else
            {
                return new byte[] { };
            }
        }

        private async Task<byte[]> HashAndSignUserOp(UserOperation userOp, ThirdwebContract entryPointContract)
        {
            var userOpHash = await ThirdwebContract.ReadContract<byte[]>(entryPointContract, "getUserOpHash", userOp);
            var sig = await _personalAccount.PersonalSign(userOpHash);
            return sig.HexToByteArray();
        }

        private UserOperationHexified EncodeUserOperation(UserOperation userOperation)
        {
            return new UserOperationHexified()
            {
                sender = userOperation.Sender,
                nonce = userOperation.Nonce.ToHexBigInteger().HexValue,
                initCode = userOperation.InitCode.ToHex(true),
                callData = userOperation.CallData.ToHex(true),
                callGasLimit = userOperation.CallGasLimit.ToHexBigInteger().HexValue,
                verificationGasLimit = userOperation.VerificationGasLimit.ToHexBigInteger().HexValue,
                preVerificationGas = userOperation.PreVerificationGas.ToHexBigInteger().HexValue,
                maxFeePerGas = userOperation.MaxFeePerGas.ToHexBigInteger().HexValue,
                maxPriorityFeePerGas = userOperation.MaxPriorityFeePerGas.ToHexBigInteger().HexValue,
                paymasterAndData = userOperation.PaymasterAndData.ToHex(true),
                signature = userOperation.Signature.ToHex(true)
            };
        }

        internal async Task ForceDeploy()
        {
            var input = new TransactionInput("0x", _accountContract.Address, new HexBigInteger(0));
            var txHash = await SendTransaction(input);
            _ = await Utils.GetTransactionReceipt(_client, _chainId, txHash);
        }

        public Task<string> GetAddress()
        {
            return Task.FromResult(_accountContract.Address);
        }

        public Task<string> EthSign(string message)
        {
            return _personalAccount.EthSign(message);
        }

        public Task<string> PersonalSign(byte[] rawMessage)
        {
            return _personalAccount.PersonalSign(rawMessage);
        }

        public async Task<string> PersonalSign(string message)
        {
            if (!await IsDeployed())
            {
                Console.WriteLine("SmartWallet not deployed, deploying before signing...");
                await ForceDeploy();
            }
            if (await IsDeployed())
            {
                var originalMsgHash = System.Text.Encoding.UTF8.GetBytes(message).HashPrefixedMessage();
                bool factorySupports712;
                try
                {
                    _ = await ThirdwebContract.ReadContract<byte[]>(_accountContract, "getMessageHash", originalMsgHash);
                    factorySupports712 = true;
                }
                catch
                {
                    factorySupports712 = false;
                }

                var sig = factorySupports712
                    ? await EIP712.GenerateSignature_SmartAccount_AccountMessage("Account", "1", _chainId, await GetAddress(), originalMsgHash, _personalAccount)
                    : await _personalAccount.PersonalSign(originalMsgHash);

                var isValid = await IsValidSignature(message, sig);
                return isValid ? sig : throw new Exception("Invalid signature.");
            }
            else
            {
                throw new Exception("Smart account could not be deployed, unable to sign message.");
            }
        }

        public async Task<bool> IsValidSignature(string message, string signature)
        {
            try
            {
                var magicValue = await ThirdwebContract.ReadContract<byte[]>(_accountContract, "isValidSignature", message.HashPrefixedMessage().HexToByteArray(), signature.HexToByteArray());
                return magicValue.ToHex(true) == new byte[] { 0x16, 0x26, 0xba, 0x7e }.ToHex(true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public async Task<TransactionReceipt> CreateSessionKey(
            string signerAddress,
            List<string> approvedTargets,
            string nativeTokenLimitPerTransactionInWei,
            string permissionStartTimestamp,
            string permissionEndTimestamp,
            string reqValidityStartTimestamp,
            string reqValidityEndTimestamp
        )
        {
            var request = new SignerPermissionRequest()
            {
                Signer = signerAddress,
                IsAdmin = 0,
                ApprovedTargets = approvedTargets,
                NativeTokenLimitPerTransaction = BigInteger.Parse(nativeTokenLimitPerTransactionInWei),
                PermissionStartTimestamp = BigInteger.Parse(permissionStartTimestamp),
                PermissionEndTimestamp = BigInteger.Parse(permissionEndTimestamp),
                ReqValidityStartTimestamp = BigInteger.Parse(reqValidityStartTimestamp),
                ReqValidityEndTimestamp = BigInteger.Parse(reqValidityEndTimestamp),
                Uid = Guid.NewGuid().ToByteArray()
            };

            var signature = await EIP712.GenerateSignature_SmartAccount("Account", "1", _chainId, await GetAddress(), request, _personalAccount);
            var data = new Contract(null, _accountContract.Abi, _accountContract.Address).GetFunction("setPermissionsForSigner").GetData(request, signature.HexToByteArray());
            var txInput = new TransactionInput()
            {
                From = await GetAddress(),
                To = _accountContract.Address,
                Value = new HexBigInteger(0),
                Data = data
            };
            var txHash = await SendTransaction(txInput);
            return await Utils.GetTransactionReceipt(_client, _chainId, txHash);
        }

        public Task<string> SignTypedDataV4(string json)
        {
            return _personalAccount.SignTypedDataV4(json);
        }

        public Task<string> SignTypedDataV4<T, TDomain>(T data, TypedData<TDomain> typedData)
            where TDomain : IDomain
        {
            throw new NotImplementedException();
        }

        public Task<string> SignTransaction(TransactionInput transaction, BigInteger chainId)
        {
            return _personalAccount.SignTransaction(transaction, chainId);
        }

        public Task<bool> IsConnected()
        {
            return Task.FromResult(_accountContract != null);
        }

        public Task Disconnect()
        {
            return _personalAccount.Disconnect();
        }
    }
}
