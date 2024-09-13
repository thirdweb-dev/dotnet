using System.Numerics;
using System.Text;
using Nethereum.ABI;
using Nethereum.ABI.EIP712;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.Util;
using Newtonsoft.Json;
using Thirdweb.AccountAbstraction;

namespace Thirdweb;

public class ModularSmartWallet : IThirdwebWallet
{
    public ThirdwebClient Client { get; }

    public ThirdwebAccountType AccountType => ThirdwebAccountType.ModularSmartAccount;

    public bool IsDeploying { get; private set; }

    private readonly IThirdwebWallet _personalAccount;
    private ThirdwebContract _factoryContract;
    private ThirdwebContract _accountContract;
    private ThirdwebContract _entryPointContract;
    private BigInteger _chainId;
    private string _bundlerUrl;
    private string _paymasterUrl;
    private readonly bool _gasless;

    protected ModularSmartWallet(
        IThirdwebWallet personalAccount,
        bool gasless,
        BigInteger chainId,
        string bundlerUrl,
        string paymasterUrl,
        ThirdwebContract entryPointContract,
        ThirdwebContract factoryContract,
        ThirdwebContract accountContract
    )
    {
        this.Client = personalAccount.Client;

        this._personalAccount = personalAccount;
        this._gasless = gasless;
        this._chainId = chainId;
        this._bundlerUrl = bundlerUrl;
        this._paymasterUrl = paymasterUrl;
        this._entryPointContract = entryPointContract;
        this._factoryContract = factoryContract;
        this._accountContract = accountContract;
    }

    public static async Task<ModularSmartWallet> Create(
        IThirdwebWallet personalWallet,
        BigInteger chainId,
        bool gasless = true,
        string factoryAddress = null,
        string accountAddressOverride = null,
        string entryPoint = null,
        string bundlerUrl = null,
        string paymasterUrl = null
    )
    {
        if (!await personalWallet.IsConnected().ConfigureAwait(false))
        {
            throw new InvalidOperationException("SmartAccount.Connect: Personal account must be connected.");
        }

        entryPoint ??= Constants.ENTRYPOINT_ADDRESS_V07;

        bundlerUrl ??= $"https://{chainId}.bundler.thirdweb.com/v2";
        paymasterUrl ??= $"https://{chainId}.bundler.thirdweb.com/v2";
        factoryAddress ??= Constants.DEFAULT_FACTORY_ADDRESS_V07;

        var entryPointAbi = Constants.ENTRYPOINT_V07_ABI;
        var factoryAbi = Constants.MODULAR_FACTORY_ABI;
        var accountAbi = Constants.MODULAR_ACCOUNT_ABI;

        var entryPointContract = await ThirdwebContract.Create(personalWallet.Client, entryPoint, chainId, entryPointAbi).ConfigureAwait(false);
        var factoryContract = await ThirdwebContract.Create(personalWallet.Client, factoryAddress, chainId, factoryAbi).ConfigureAwait(false);

        var personalAddress = await personalWallet.GetAddress().ConfigureAwait(false);
        var accountAddress = accountAddressOverride ?? await ThirdwebContract.Read<string>(factoryContract, "getAddress", personalAddress, Array.Empty<byte>()).ConfigureAwait(false);
        var accountContract = await ThirdwebContract.Create(personalWallet.Client, accountAddress, chainId, accountAbi).ConfigureAwait(false);
        return new ModularSmartWallet(personalWallet, gasless, chainId, bundlerUrl, paymasterUrl, entryPointContract, factoryContract, accountContract);
    }

    /// <summary>
    /// Attempts to set the active network to the specified chain ID. Requires related contracts to be deterministically deployed on the chain.
    /// </summary>
    /// <param name="chainId">The chain ID to switch to.</param>
    /// <returns></returns>
    public async Task SwitchNetwork(BigInteger chainId)
    {
        if (this._chainId == chainId)
        {
            return;
        }

        this._chainId = chainId;

        this._bundlerUrl = $"https://{chainId}.bundler.thirdweb.com/v2";
        this._paymasterUrl = $"https://{chainId}.bundler.thirdweb.com/v2";
        this._entryPointContract = await ThirdwebContract.Create(this.Client, this._entryPointContract.Address, chainId, this._entryPointContract.Abi).ConfigureAwait(false);
        this._factoryContract = await ThirdwebContract.Create(this.Client, this._factoryContract.Address, chainId, this._factoryContract.Abi).ConfigureAwait(false);
        this._accountContract = await ThirdwebContract.Create(this.Client, this._accountContract.Address, chainId, this._accountContract.Abi).ConfigureAwait(false);
    }

    public async Task<bool> IsDeployed()
    {
        var code = await ThirdwebRPC.GetRpcInstance(this.Client, this._chainId).SendRequestAsync<string>("eth_getCode", this._accountContract.Address, "latest").ConfigureAwait(false);
        return code != "0x";
    }

    public async Task<string> SendTransaction(ThirdwebTransactionInput transactionInput)
    {
        if (transactionInput == null)
        {
            throw new InvalidOperationException("SmartAccount.SendTransaction: Transaction input is required.");
        }

        await this.SwitchNetwork(transactionInput.ChainId.Value).ConfigureAwait(false);

        var transaction = await ThirdwebTransaction.Create(this, transactionInput).ConfigureAwait(false);
        transaction = await ThirdwebTransaction.Prepare(transaction).ConfigureAwait(false);
        transactionInput = transaction.Input;

        var signedOp = await this.SignUserOp(transactionInput).ConfigureAwait(false);
        return await this.SendUserOp(signedOp).ConfigureAwait(false);
    }

    public async Task<ThirdwebTransactionReceipt> ExecuteTransaction(ThirdwebTransactionInput transactionInput)
    {
        var txHash = await this.SendTransaction(transactionInput).ConfigureAwait(false);
        return await ThirdwebTransaction.WaitForTransactionReceipt(this.Client, this._chainId, txHash).ConfigureAwait(false);
    }

    private async Task<(byte[] initCode, string factory, string factoryData)> GetInitCode()
    {
        if (await this.IsDeployed().ConfigureAwait(false))
        {
            return (Array.Empty<byte>(), null, null);
        }

        var personalAccountAddress = await this._personalAccount.GetAddress().ConfigureAwait(false);
        var factoryContract = new Contract(null, this._factoryContract.Abi, this._factoryContract.Address);
        var createFunction = factoryContract.GetFunction("createAccount");
        var data = createFunction.GetData(personalAccountAddress, Array.Empty<byte>(), new List<InitializerInstallModule>() { }); // TODO: ALlow passing in modules of diff types
        return (Utils.HexConcat(this._factoryContract.Address, data).HexToBytes(), this._factoryContract.Address, data);
    }

    private async Task<object> SignUserOp(ThirdwebTransactionInput transactionInput, int? requestId = null, bool simulation = false)
    {
        requestId ??= 1;

        (var initCode, var factory, var factoryData) = await this.GetInitCode().ConfigureAwait(false);

        // Wait until deployed to avoid double initCode
        if (!simulation)
        {
            if (this.IsDeploying)
            {
                initCode = Array.Empty<byte>();
                factory = null;
                factoryData = null;
            }

            while (this.IsDeploying)
            {
                await ThirdwebTask.Delay(100).ConfigureAwait(false);
            }

            this.IsDeploying = initCode.Length > 0;
        }

        // Create the user operation and its safe (hexified) version

        var fees = await BundlerClient.ThirdwebGetUserOperationGasPrice(this.Client, this._bundlerUrl, requestId).ConfigureAwait(false);
        var maxFee = new HexBigInteger(fees.MaxFeePerGas).Value;
        var maxPriorityFee = new HexBigInteger(fees.MaxPriorityFeePerGas).Value;

        ABIEncode abiEncoder = new();
        var bytes = abiEncoder.GetABIEncodedPacked(
            new ABIValue(new AddressType(), transactionInput.To),
            new ABIValue(new IntType("uint256"), transactionInput.Value?.Value ?? 0),
            new ABIValue(new BytesType(), transactionInput.Data.HexToBytes())
        );

        var executeFn = new ExecuteModular { Mode = "0x".HexToBytes32(), ExecutionCallData = bytes, }; // TODO: Maybe allow batch and try
        var executeInput = executeFn.CreateTransactionInput(await this.GetAddress().ConfigureAwait(false));

        var partialUserOp = new UserOperationV7()
        {
            Sender = this._accountContract.Address,
            Nonce = await this.GetNonce().ConfigureAwait(false),
            Factory = factory,
            FactoryData = factoryData.HexToBytes(),
            CallData = executeInput.Data.HexToBytes(),
            CallGasLimit = 0,
            VerificationGasLimit = 0,
            PreVerificationGas = 0,
            MaxFeePerGas = maxFee,
            MaxPriorityFeePerGas = maxPriorityFee,
            Paymaster = null,
            PaymasterVerificationGasLimit = 0,
            PaymasterPostOpGasLimit = 0,
            PaymasterData = Array.Empty<byte>(),
            Signature = Constants.DUMMY_SIG.HexToBytes(),
        };

        // Update Paymaster Data / Estimate gas

        var res = await this.GetPaymasterAndData(requestId, EncodeUserOperation(partialUserOp)).ConfigureAwait(false);
        partialUserOp.Paymaster = res.Paymaster;
        partialUserOp.PaymasterData = res.PaymasterData?.HexToBytes() ?? Array.Empty<byte>();
        partialUserOp.PreVerificationGas = new HexBigInteger(res.PreVerificationGas ?? "0x0").Value;
        partialUserOp.VerificationGasLimit = new HexBigInteger(res.VerificationGasLimit ?? "0x0").Value;
        partialUserOp.CallGasLimit = new HexBigInteger(res.CallGasLimit ?? "0x0").Value;
        partialUserOp.PaymasterVerificationGasLimit = new HexBigInteger(res.PaymasterVerificationGasLimit ?? "0x0").Value;
        partialUserOp.PaymasterPostOpGasLimit = new HexBigInteger(res.PaymasterPostOpGasLimit ?? "0x0").Value;

        // Hash, sign and encode the user operation

        partialUserOp.Signature = await this.HashAndSignUserOp(partialUserOp, this._entryPointContract).ConfigureAwait(false);

        return partialUserOp;
    }

    private async Task<string> SendUserOp(object userOperation, int? requestId = null)
    {
        requestId ??= 1;

        // Encode op

        object encodedOp;

        encodedOp = userOperation is UserOperationV7 ? (object)EncodeUserOperation(userOperation as UserOperationV7) : throw new Exception("Invalid signed operation type");

        // Send the user operation

        var userOpHash = await BundlerClient.EthSendUserOperation(this.Client, this._bundlerUrl, requestId, encodedOp, this._entryPointContract.Address).ConfigureAwait(false);

        // Wait for the transaction to be mined

        string txHash = null;
        while (txHash == null)
        {
            var userOpReceipt = await BundlerClient.EthGetUserOperationReceipt(this.Client, this._bundlerUrl, requestId, userOpHash).ConfigureAwait(false);
            txHash = userOpReceipt?.Receipt?.TransactionHash;
            await ThirdwebTask.Delay(100).ConfigureAwait(false);
        }

        this.IsDeploying = false;
        return txHash;
    }

    private async Task<BigInteger> GetNonce()
    {
        var validatorBytes = "0x0000000000000000000000000000000000000000".HexToBytes(); // TODO: Use potentially passed in validator
        BigInteger validatorInt192 = new(validatorBytes);
        validatorInt192 = BigInteger.Abs(validatorInt192) % (BigInteger.One << 192);
        return await ThirdwebContract.Read<BigInteger>(this._entryPointContract, "getNonce", await this.GetAddress().ConfigureAwait(false), validatorInt192).ConfigureAwait(false);
    }

    private async Task<PMSponsorOperationResponse> GetPaymasterAndData(object requestId, object userOp)
    {
        return this._gasless
            ? await BundlerClient.PMSponsorUserOperation(this.Client, this._paymasterUrl, requestId, userOp, this._entryPointContract.Address).ConfigureAwait(false)
            : new PMSponsorOperationResponse();
    }

    private async Task<byte[]> HashAndSignUserOp(UserOperationV7 userOp, ThirdwebContract entryPointContract)
    {
        var factoryBytes = userOp.Factory.HexToBytes();
        var factoryDataBytes = userOp.FactoryData;
        var initCodeBuffer = new byte[factoryBytes.Length + factoryDataBytes.Length];
        Buffer.BlockCopy(factoryBytes, 0, initCodeBuffer, 0, factoryBytes.Length);
        Buffer.BlockCopy(factoryDataBytes, 0, initCodeBuffer, factoryBytes.Length, factoryDataBytes.Length);

        var verificationGasLimitBytes = userOp.VerificationGasLimit.ToHexBigInteger().HexValue.HexToBytes().PadBytes(16);
        var callGasLimitBytes = userOp.CallGasLimit.ToHexBigInteger().HexValue.HexToBytes().PadBytes(16);
        var accountGasLimitsBuffer = new byte[32];
        Buffer.BlockCopy(verificationGasLimitBytes, 0, accountGasLimitsBuffer, 0, 16);
        Buffer.BlockCopy(callGasLimitBytes, 0, accountGasLimitsBuffer, 16, 16);

        var maxPriorityFeePerGasBytes = userOp.MaxPriorityFeePerGas.ToHexBigInteger().HexValue.HexToBytes().PadBytes(16);
        var maxFeePerGasBytes = userOp.MaxFeePerGas.ToHexBigInteger().HexValue.HexToBytes().PadBytes(16);
        var gasFeesBuffer = new byte[32];
        Buffer.BlockCopy(maxPriorityFeePerGasBytes, 0, gasFeesBuffer, 0, 16);
        Buffer.BlockCopy(maxFeePerGasBytes, 0, gasFeesBuffer, 16, 16);

        var paymasterBytes = userOp.Paymaster.HexToBytes();
        var paymasterVerificationGasLimitBytes = userOp.PaymasterVerificationGasLimit.ToHexBigInteger().HexValue.HexToBytes().PadBytes(16);
        var paymasterPostOpGasLimitBytes = userOp.PaymasterPostOpGasLimit.ToHexBigInteger().HexValue.HexToBytes().PadBytes(16);
        var paymasterDataBytes = userOp.PaymasterData;
        var paymasterAndDataBuffer = new byte[20 + 16 + 16 + paymasterDataBytes.Length];
        Buffer.BlockCopy(paymasterBytes, 0, paymasterAndDataBuffer, 0, 20);
        Buffer.BlockCopy(paymasterVerificationGasLimitBytes, 0, paymasterAndDataBuffer, 20, 16);
        Buffer.BlockCopy(paymasterPostOpGasLimitBytes, 0, paymasterAndDataBuffer, 20 + 16, 16);
        Buffer.BlockCopy(paymasterDataBytes, 0, paymasterAndDataBuffer, 20 + 16 + 16, paymasterDataBytes.Length);

        var packedOp = new PackedUserOperation()
        {
            Sender = userOp.Sender,
            Nonce = userOp.Nonce,
            InitCode = initCodeBuffer,
            CallData = userOp.CallData,
            AccountGasLimits = accountGasLimitsBuffer,
            PreVerificationGas = userOp.PreVerificationGas,
            GasFees = gasFeesBuffer,
            PaymasterAndData = paymasterAndDataBuffer,
            Signature = userOp.Signature
        };

        var userOpHash = await ThirdwebContract.Read<byte[]>(entryPointContract, "getUserOpHash", packedOp).ConfigureAwait(false);

        var sig =
            this._personalAccount.AccountType == ThirdwebAccountType.ExternalAccount
                ? await this._personalAccount.PersonalSign(userOpHash.BytesToHex()).ConfigureAwait(false)
                : await this._personalAccount.PersonalSign(userOpHash).ConfigureAwait(false);

        return sig.HexToBytes();
    }

    private static UserOperationHexifiedV7 EncodeUserOperation(UserOperationV7 userOperation)
    {
        return new UserOperationHexifiedV7()
        {
            Sender = userOperation.Sender,
            Nonce = Utils.HexConcat(userOperation.Nonce.ToHexBigInteger().HexValue),
            Factory = userOperation.Factory,
            FactoryData = userOperation.FactoryData.BytesToHex(),
            CallData = userOperation.CallData.BytesToHex(),
            CallGasLimit = userOperation.CallGasLimit.ToHexBigInteger().HexValue,
            VerificationGasLimit = userOperation.VerificationGasLimit.ToHexBigInteger().HexValue,
            PreVerificationGas = userOperation.PreVerificationGas.ToHexBigInteger().HexValue,
            MaxFeePerGas = userOperation.MaxFeePerGas.ToHexBigInteger().HexValue,
            MaxPriorityFeePerGas = userOperation.MaxPriorityFeePerGas.ToHexBigInteger().HexValue,
            Paymaster = userOperation.Paymaster,
            PaymasterVerificationGasLimit = userOperation.PaymasterVerificationGasLimit.ToHexBigInteger().HexValue,
            PaymasterPostOpGasLimit = userOperation.PaymasterPostOpGasLimit.ToHexBigInteger().HexValue,
            PaymasterData = userOperation.PaymasterData.BytesToHex(),
            Signature = userOperation.Signature.BytesToHex()
        };
    }

    public async Task ForceDeploy()
    {
        if (await this.IsDeployed().ConfigureAwait(false))
        {
            return;
        }

        if (this.IsDeploying)
        {
            throw new InvalidOperationException("SmartAccount.ForceDeploy: Account is already deploying.");
        }

        var input = new ThirdwebTransactionInput(this._chainId)
        {
            Data = "0x",
            To = this._accountContract.Address,
            Value = new HexBigInteger(0)
        };
        var txHash = await this.SendTransaction(input).ConfigureAwait(false);
        _ = await ThirdwebTransaction.WaitForTransactionReceipt(this.Client, this._chainId, txHash).ConfigureAwait(false);
    }

    public Task<IThirdwebWallet> GetPersonalWallet()
    {
        return Task.FromResult(this._personalAccount);
    }

    public Task<string> GetAddress()
    {
        return Task.FromResult(this._accountContract.Address.ToChecksumAddress());
    }

    public Task<string> EthSign(byte[] rawMessage)
    {
        throw new NotImplementedException();
    }

    public Task<string> EthSign(string message)
    {
        throw new NotImplementedException();
    }

    public Task<string> RecoverAddressFromEthSign(string message, string signature)
    {
        throw new NotImplementedException();
    }

    public Task<string> PersonalSign(byte[] rawMessage)
    {
        throw new NotImplementedException();
    }

    public async Task<string> PersonalSign(string message)
    {
        if (!await this.IsDeployed())
        {
            while (this.IsDeploying)
            {
                await ThirdwebTask.Delay(100).ConfigureAwait(false);
            }
            await this.ForceDeploy().ConfigureAwait(false);
        }

        if (await this.IsDeployed().ConfigureAwait(false))
        {
            var originalMsgHash = Encoding.UTF8.GetBytes(message).HashPrefixedMessage();
            bool factorySupports712;
            try
            {
                _ = await ThirdwebContract.Read<byte[]>(this._accountContract, "getMessageHash", originalMsgHash).ConfigureAwait(false);
                factorySupports712 = true;
            }
            catch
            {
                factorySupports712 = false;
            }

            var sig = factorySupports712
                ? await EIP712
                    .GenerateSignature_SmartAccount_AccountMessage("Account", "1", this._chainId, await this.GetAddress().ConfigureAwait(false), originalMsgHash, this._personalAccount)
                    .ConfigureAwait(false)
                : await this._personalAccount.PersonalSign(message).ConfigureAwait(false);

            var isValid = await this.IsValidSignature(message, sig);
            return isValid ? sig : throw new Exception("Invalid signature.");
        }
        else
        {
            throw new Exception("Smart account could not be deployed, unable to sign message.");
        }
    }

    public async Task<string> RecoverAddressFromPersonalSign(string message, string signature)
    {
        return !await this.IsValidSignature(message, signature).ConfigureAwait(false)
            ? await this._personalAccount.RecoverAddressFromPersonalSign(message, signature).ConfigureAwait(false)
            : await this.GetAddress().ConfigureAwait(false);
    }

    public async Task<bool> IsValidSignature(string message, string signature)
    {
        try
        {
            var magicValue = await ThirdwebContract.Read<byte[]>(this._accountContract, "isValidSignature", message.StringToHex(), signature.HexToBytes()).ConfigureAwait(false);
            return magicValue.BytesToHex() == new byte[] { 0x16, 0x26, 0xba, 0x7e }.BytesToHex();
        }
        catch
        {
            try
            {
                var magicValue = await ThirdwebContract
                    .Read<byte[]>(this._accountContract, "isValidSignature", Encoding.UTF8.GetBytes(message).HashPrefixedMessage(), signature.HexToBytes())
                    .ConfigureAwait(false);
                return magicValue.BytesToHex() == new byte[] { 0x16, 0x26, 0xba, 0x7e }.BytesToHex();
            }
            catch
            {
                return false;
            }
        }
    }

    public async Task<List<string>> GetAllAdmins()
    {
        throw new NotImplementedException();
    }

    public async Task<List<SignerPermissions>> GetAllActiveSigners()
    {
        var result = await ThirdwebContract.Read<List<SignerPermissions>>(this._accountContract, "getAllActiveSigners").ConfigureAwait(false);
        return result ?? new List<SignerPermissions>();
    }

    public async Task<ThirdwebTransactionReceipt> CreateSessionKey(
        string signerAddress,
        List<string> approvedTargets,
        BigInteger nativeTokenLimitPerTransactionInWei,
        BigInteger startTimestamp,
        BigInteger endTimestamp,
        SessionKeyType sessionKeyType = SessionKeyType.Regular
    )
    {
        var request = new SessionKeyParamsModular()
        {
            ApprovedTargets = approvedTargets,
            NativeTokenLimitPerTransaction = nativeTokenLimitPerTransactionInWei,
            StartTimestamp = startTimestamp,
            EndTimestamp = endTimestamp,
            KeyType = (byte)sessionKeyType,
        };

        // Do it this way to avoid triggering an extra sig from estimation
        var data = new Contract(null, this._accountContract.Abi, this._accountContract.Address).GetFunction("createSessionKeyForSigner").GetData(signerAddress, request);
        var txInput = new ThirdwebTransactionInput(this._chainId)
        {
            To = this._accountContract.Address,
            Value = new HexBigInteger(0),
            Data = data
        };
        var txHash = await this.SendTransaction(txInput).ConfigureAwait(false);
        return await ThirdwebTransaction.WaitForTransactionReceipt(this.Client, this._chainId, txHash).ConfigureAwait(false);
    }

    public async Task<ThirdwebTransactionReceipt> RevokeSessionKey(string signerAddress)
    {
        return await this.CreateSessionKey(signerAddress, new List<string>(), 0, 0, 0, SessionKeyType.Regular).ConfigureAwait(false);
    }

    public async Task<ThirdwebTransactionReceipt> AddAdmin(string admin)
    {
        throw new NotImplementedException();
    }

    public async Task<ThirdwebTransactionReceipt> RemoveAdmin(string admin)
    {
        throw new NotImplementedException();
    }

    public Task<string> SignTypedDataV4(string json)
    {
        return this._personalAccount.SignTypedDataV4(json);
    }

    public Task<string> SignTypedDataV4<T, TDomain>(T data, TypedData<TDomain> typedData)
        where TDomain : IDomain
    {
        return this._personalAccount.SignTypedDataV4(data, typedData);
    }

    public Task<string> RecoverAddressFromTypedDataV4<T, TDomain>(T data, TypedData<TDomain> typedData, string signature)
        where TDomain : IDomain
    {
        return this._personalAccount.RecoverAddressFromTypedDataV4(data, typedData, signature);
    }

    public async Task<BigInteger> EstimateUserOperationGas(ThirdwebTransactionInput transaction)
    {
        await this.SwitchNetwork(transaction.ChainId.Value).ConfigureAwait(false);

        var signedOp = await this.SignUserOp(transaction, null, simulation: true).ConfigureAwait(false);

        var castSignedOp = signedOp as UserOperationV7;
        var cost = castSignedOp.CallGasLimit + castSignedOp.VerificationGasLimit + castSignedOp.PreVerificationGas + castSignedOp.PaymasterVerificationGasLimit + castSignedOp.PaymasterPostOpGasLimit;
        return cost;
    }

    public async Task<string> SignTransaction(ThirdwebTransactionInput transaction)
    {
        await this.SwitchNetwork(transaction.ChainId.Value).ConfigureAwait(false);

        var signedOp = await this.SignUserOp(transaction).ConfigureAwait(false);
        var encodedOp = EncodeUserOperation(signedOp as UserOperationV7);
        return JsonConvert.SerializeObject(encodedOp);
    }

    public Task<bool> IsConnected()
    {
        return Task.FromResult(this._accountContract != null);
    }

    public Task Disconnect()
    {
        this._accountContract = null;
        return Task.CompletedTask;
    }
}
