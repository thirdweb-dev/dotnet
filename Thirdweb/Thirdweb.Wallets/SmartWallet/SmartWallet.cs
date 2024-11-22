using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using Nethereum.ABI;
using Nethereum.ABI.EIP712;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.Util;
using Newtonsoft.Json;
using Thirdweb.AccountAbstraction;

namespace Thirdweb;

public enum TokenPaymaster
{
    NONE,
    BASE_USDC,
    CELO_CUSD,
    LISK_LSK
}

public class SmartWallet : IThirdwebWallet
{
    public ThirdwebClient Client { get; }

    public ThirdwebAccountType AccountType => ThirdwebAccountType.SmartAccount;

    public bool IsDeploying { get; private set; }

    private readonly IThirdwebWallet _personalAccount;
    private ThirdwebContract _factoryContract;
    private ThirdwebContract _accountContract;
    private ThirdwebContract _entryPointContract;
    private BigInteger _chainId;
    private string _bundlerUrl;
    private string _paymasterUrl;
    private bool _isApproving;
    private bool _isApproved;

    private readonly string _erc20PaymasterAddress;
    private readonly string _erc20PaymasterToken;
    private readonly BigInteger _erc20PaymasterStorageSlot;
    private readonly bool _gasless;

    private struct TokenPaymasterConfig
    {
        public BigInteger ChainId;
        public string PaymasterAddress;
        public string TokenAddress;
        public BigInteger BalanceStorageSlot;
    }

    private static readonly Dictionary<TokenPaymaster, TokenPaymasterConfig> _tokenPaymasterConfig =
        new()
        {
            {
                TokenPaymaster.NONE,
                new TokenPaymasterConfig()
                {
                    ChainId = 0,
                    PaymasterAddress = null,
                    TokenAddress = null,
                    BalanceStorageSlot = 0
                }
            },
            {
                TokenPaymaster.BASE_USDC,
                new TokenPaymasterConfig()
                {
                    ChainId = 8453,
                    PaymasterAddress = "0x2222f2738BE6bB7aA0Bfe4AEeAf2908172CF5539",
                    TokenAddress = "0x833589fCD6eDb6E08f4c7C32D4f71b54bdA02913",
                    BalanceStorageSlot = 9
                }
            },
            {
                TokenPaymaster.CELO_CUSD,
                new TokenPaymasterConfig()
                {
                    ChainId = 42220,
                    PaymasterAddress = "0x3feA3c5744D715ff46e91C4e5C9a94426DfF2aF9",
                    TokenAddress = "0x765DE816845861e75A25fCA122bb6898B8B1282a",
                    BalanceStorageSlot = 9
                }
            },
            {
                TokenPaymaster.LISK_LSK,
                new TokenPaymasterConfig()
                {
                    ChainId = 1135,
                    PaymasterAddress = "0x9eb8cf7fBa5ed9EeDCC97a0d52254cc0e9B1AC25",
                    TokenAddress = "0xac485391EB2d7D88253a7F1eF18C37f4242D1A24",
                    BalanceStorageSlot = 9
                }
            }
        };

    private bool UseERC20Paymaster => !string.IsNullOrEmpty(this._erc20PaymasterAddress) && !string.IsNullOrEmpty(this._erc20PaymasterToken);

    protected SmartWallet(
        IThirdwebWallet personalAccount,
        bool gasless,
        BigInteger chainId,
        string bundlerUrl,
        string paymasterUrl,
        ThirdwebContract entryPointContract,
        ThirdwebContract factoryContract,
        ThirdwebContract accountContract,
        string erc20PaymasterAddress,
        string erc20PaymasterToken,
        BigInteger erc20PaymasterStorageSlot
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
        this._erc20PaymasterAddress = erc20PaymasterAddress;
        this._erc20PaymasterToken = erc20PaymasterToken;
        this._erc20PaymasterStorageSlot = erc20PaymasterStorageSlot;
    }

    #region Creation

    /// <summary>
    /// Creates a new instance of <see cref="SmartWallet"/>.
    /// </summary>
    /// <param name="personalWallet">The smart wallet's signer to use.</param>
    /// <param name="chainId">The chain ID.</param>
    /// <param name="gasless">Whether to sponsor gas for transactions.</param>
    /// <param name="factoryAddress">Override the default factory address.</param>
    /// <param name="accountAddressOverride">Override the canonical account address that would be found deterministically based on the signer.</param>
    /// <param name="entryPoint">Override the default entry point address. We provide Constants for different versions.</param>
    /// <param name="bundlerUrl">Override the default thirdweb bundler URL.</param>
    /// <param name="paymasterUrl">Override the default thirdweb paymaster URL.</param>
    /// <param name="tokenPaymaster">Use an ERC20 paymaster and sponsor gas with ERC20s. If set, factoryAddress and accountAddressOverride are ignored.</param>
    /// <returns>A new instance of <see cref="SmartWallet"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the personal account is not connected.</exception>
    public static async Task<SmartWallet> Create(
        IThirdwebWallet personalWallet,
        BigInteger chainId,
        bool? gasless = null,
        string factoryAddress = null,
        string accountAddressOverride = null,
        string entryPoint = null,
        string bundlerUrl = null,
        string paymasterUrl = null,
        TokenPaymaster tokenPaymaster = TokenPaymaster.NONE
    )
    {
        if (!await personalWallet.IsConnected().ConfigureAwait(false))
        {
            throw new InvalidOperationException("SmartAccount.Connect: Personal account must be connected.");
        }

        if (personalWallet is EcosystemWallet ecoWallet)
        {
            try
            {
                var ecoDetails = await ecoWallet.GetEcosystemDetails();
                if (ecoDetails.SmartAccountOptions.HasValue)
                {
                    gasless ??= ecoDetails.SmartAccountOptions?.SponsorGas;
                    factoryAddress ??= string.IsNullOrEmpty(ecoDetails.SmartAccountOptions?.AccountFactoryAddress) ? null : ecoDetails.SmartAccountOptions?.AccountFactoryAddress;
                }
            }
            catch
            {
                // no-op
            }
        }

        entryPoint ??= tokenPaymaster == TokenPaymaster.NONE ? Constants.ENTRYPOINT_ADDRESS_V06 : Constants.ENTRYPOINT_ADDRESS_V07;

        var entryPointVersion = Utils.GetEntryPointVersion(entryPoint);

        gasless ??= true;
        bundlerUrl ??= $"https://{chainId}.bundler.thirdweb.com/v2";
        paymasterUrl ??= $"https://{chainId}.bundler.thirdweb.com/v2";
        factoryAddress ??= entryPointVersion == 6 ? Constants.DEFAULT_FACTORY_ADDRESS_V06 : Constants.DEFAULT_FACTORY_ADDRESS_V07;

        ThirdwebContract entryPointContract = null;
        ThirdwebContract factoryContract = null;
        ThirdwebContract accountContract = null;

        if (!await Utils.IsZkSync(personalWallet.Client, chainId).ConfigureAwait(false))
        {
            var entryPointAbi = entryPointVersion == 6 ? Constants.ENTRYPOINT_V06_ABI : Constants.ENTRYPOINT_V07_ABI;
            var factoryAbi = entryPointVersion == 6 ? Constants.FACTORY_V06_ABI : Constants.FACTORY_V07_ABI;
            var accountAbi = entryPointVersion == 6 ? Constants.ACCOUNT_V06_ABI : Constants.ACCOUNT_V07_ABI;

            entryPointContract = await ThirdwebContract.Create(personalWallet.Client, entryPoint, chainId, entryPointAbi).ConfigureAwait(false);
            factoryContract = await ThirdwebContract.Create(personalWallet.Client, factoryAddress, chainId, factoryAbi).ConfigureAwait(false);

            var personalAddress = await personalWallet.GetAddress().ConfigureAwait(false);
            var accountAddress = accountAddressOverride ?? await ThirdwebContract.Read<string>(factoryContract, "getAddress", personalAddress, Array.Empty<byte>()).ConfigureAwait(false);

            accountContract = await ThirdwebContract.Create(personalWallet.Client, accountAddress, chainId, accountAbi).ConfigureAwait(false);
        }

        var erc20PmInfo = _tokenPaymasterConfig[tokenPaymaster];

        if (tokenPaymaster != TokenPaymaster.NONE)
        {
            if (entryPointVersion != 7)
            {
                throw new InvalidOperationException("Token paymasters are only supported in entry point version 7.");
            }
            if (erc20PmInfo.ChainId != chainId)
            {
                throw new InvalidOperationException("Token paymaster chain ID does not match the smart account chain ID.");
            }
        }

        return new SmartWallet(
            personalWallet,
            gasless.Value,
            chainId,
            bundlerUrl,
            paymasterUrl,
            entryPointContract,
            factoryContract,
            accountContract,
            erc20PmInfo.PaymasterAddress,
            erc20PmInfo.TokenAddress,
            erc20PmInfo.BalanceStorageSlot
        );
    }

    #endregion

    #region Wallet Specific

    /// <summary>
    /// Returns the signer that was used to connect to this SmartWallet.
    /// </summary>
    /// <returns>The signer.</returns>
    public Task<IThirdwebWallet> GetPersonalWallet()
    {
        return Task.FromResult(this._personalAccount);
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

        if (this.UseERC20Paymaster)
        {
            throw new InvalidOperationException("You cannot switch networks when using an ERC20 paymaster yet.");
        }

        this._chainId = chainId;
        this._bundlerUrl = this._bundlerUrl.Contains(".thirdweb.com") ? $"https://{chainId}.bundler.thirdweb.com/v2" : this._bundlerUrl;
        this._paymasterUrl = this._paymasterUrl.Contains(".thirdweb.com") ? $"https://{chainId}.bundler.thirdweb.com/v2" : this._paymasterUrl;

        if (!await Utils.IsZkSync(this.Client, chainId).ConfigureAwait(false))
        {
            this._entryPointContract = await ThirdwebContract.Create(this.Client, this._entryPointContract.Address, chainId, this._entryPointContract.Abi).ConfigureAwait(false);
            this._factoryContract = await ThirdwebContract.Create(this.Client, this._factoryContract.Address, chainId, this._factoryContract.Abi).ConfigureAwait(false);
            this._accountContract = await ThirdwebContract.Create(this.Client, this._accountContract.Address, chainId, this._accountContract.Abi).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Checks if the smart account is deployed on the current chain. A smart account is typically deployed when a personal message is signed or a transaction is sent.
    /// </summary>
    /// <returns>True if deployed, otherwise false.</returns>
    public async Task<bool> IsDeployed()
    {
        if (await Utils.IsZkSync(this.Client, this._chainId).ConfigureAwait(false))
        {
            return true;
        }

        var code = await ThirdwebRPC.GetRpcInstance(this.Client, this._chainId).SendRequestAsync<string>("eth_getCode", this._accountContract.Address, "latest").ConfigureAwait(false);
        return code != "0x";
    }

    /// <summary>
    /// Forces the smart account to deploy on the current chain. This is typically not necessary as the account will deploy automatically when needed.
    /// </summary>
    public async Task ForceDeploy()
    {
        if (await Utils.IsZkSync(this.Client, this._chainId).ConfigureAwait(false))
        {
            return;
        }

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

    /// <summary>
    /// Verifies if a signature is valid for a message using EIP-1271.
    /// </summary>
    /// <param name="message">The message to verify.</param>
    /// <param name="signature">The signature to verify.</param>
    /// <returns>True if the signature is valid, otherwise false.</returns>
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

    /// <summary>
    /// Gets all admins for the smart account.
    /// </summary>
    /// <returns>A list of admin addresses.</returns>
    public async Task<List<string>> GetAllAdmins()
    {
        if (await Utils.IsZkSync(this.Client, this._chainId).ConfigureAwait(false))
        {
            throw new InvalidOperationException("Account Permissions are not supported in ZkSync");
        }

        var result = await ThirdwebContract.Read<List<string>>(this._accountContract, "getAllAdmins").ConfigureAwait(false);
        return result ?? new List<string>();
    }

    /// <summary>
    /// Gets all active signers for the smart account.
    /// </summary>
    /// <returns>A list of <see cref="SignerPermissions"/>.</returns>
    public async Task<List<SignerPermissions>> GetAllActiveSigners()
    {
        if (await Utils.IsZkSync(this.Client, this._chainId).ConfigureAwait(false))
        {
            throw new InvalidOperationException("Account Permissions are not supported in ZkSync");
        }

        var result = await ThirdwebContract.Read<List<SignerPermissions>>(this._accountContract, "getAllActiveSigners").ConfigureAwait(false);
        return result ?? new List<SignerPermissions>();
    }

    /// <summary>
    /// Creates a new session key for a signer to use with the smart account.
    /// </summary>
    /// <param name="signerAddress">The address of the signer to create a session key for.</param>
    /// <param name="approvedTargets">The list of approved targets for the signer. Use a list of a single Constants.ADDRESS_ZERO to enable all contracts.</param>
    /// <param name="nativeTokenLimitPerTransactionInWei">The maximum amount of native tokens the signer can send in a single transaction.</param>
    /// <param name="permissionStartTimestamp">The timestamp when the permission starts. Can be set to zero.</param>
    /// <param name="permissionEndTimestamp">The timestamp when the permission ends. Make use of our Utils to get UNIX timestamps.</param>
    /// <param name="reqValidityStartTimestamp">The timestamp when the request validity starts. Can be set to zero.</param>
    /// <param name="reqValidityEndTimestamp">The timestamp when the request validity ends. Make use of our Utils to get UNIX timestamps.</param>
    public async Task<ThirdwebTransactionReceipt> CreateSessionKey(
        string signerAddress,
        List<string> approvedTargets,
        string nativeTokenLimitPerTransactionInWei,
        string permissionStartTimestamp,
        string permissionEndTimestamp,
        string reqValidityStartTimestamp,
        string reqValidityEndTimestamp
    )
    {
        if (await Utils.IsZkSync(this.Client, this._chainId).ConfigureAwait(false))
        {
            throw new InvalidOperationException("Account Permissions are not supported in ZkSync");
        }

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

        var signature = await EIP712.GenerateSignature_SmartAccount("Account", "1", this._chainId, await this.GetAddress().ConfigureAwait(false), request, this._personalAccount).ConfigureAwait(false);
        // Do it this way to avoid triggering an extra sig from estimation
        var data = new Contract(null, this._accountContract.Abi, this._accountContract.Address).GetFunction("setPermissionsForSigner").GetData(request, signature.HexToBytes());
        var txInput = new ThirdwebTransactionInput(this._chainId)
        {
            To = this._accountContract.Address,
            Value = new HexBigInteger(0),
            Data = data
        };
        var txHash = await this.SendTransaction(txInput).ConfigureAwait(false);
        return await ThirdwebTransaction.WaitForTransactionReceipt(this.Client, this._chainId, txHash).ConfigureAwait(false);
    }

    /// <summary>
    /// Revokes a session key from a signer.
    /// </summary>
    /// <param name="signerAddress">The address of the signer to revoke.</param>
    /// <returns>The transaction receipt.</returns>
    public async Task<ThirdwebTransactionReceipt> RevokeSessionKey(string signerAddress)
    {
        return await Utils.IsZkSync(this.Client, this._chainId).ConfigureAwait(false)
            ? throw new InvalidOperationException("Account Permissions are not supported in ZkSync")
            : await this.CreateSessionKey(signerAddress, new List<string>(), "0", "0", "0", "0", Utils.GetUnixTimeStampIn10Years().ToString()).ConfigureAwait(false);
    }

    /// <summary>
    /// Adds a new admin to the smart account.
    /// </summary>
    /// <param name="admin">The address of the admin to add.</param>
    /// <returns>The transaction receipt.</returns>
    public async Task<ThirdwebTransactionReceipt> AddAdmin(string admin)
    {
        if (await Utils.IsZkSync(this.Client, this._chainId).ConfigureAwait(false))
        {
            throw new InvalidOperationException("Account Permissions are not supported in ZkSync");
        }

        var request = new SignerPermissionRequest()
        {
            Signer = admin,
            IsAdmin = 1,
            ApprovedTargets = new List<string>(),
            NativeTokenLimitPerTransaction = 0,
            PermissionStartTimestamp = Utils.GetUnixTimeStampNow() - 3600,
            PermissionEndTimestamp = Utils.GetUnixTimeStampIn10Years(),
            ReqValidityStartTimestamp = Utils.GetUnixTimeStampNow() - 3600,
            ReqValidityEndTimestamp = Utils.GetUnixTimeStampIn10Years(),
            Uid = Guid.NewGuid().ToByteArray()
        };

        var signature = await EIP712.GenerateSignature_SmartAccount("Account", "1", this._chainId, await this.GetAddress(), request, this._personalAccount).ConfigureAwait(false);
        var data = new Contract(null, this._accountContract.Abi, this._accountContract.Address).GetFunction("setPermissionsForSigner").GetData(request, signature.HexToBytes());
        var txInput = new ThirdwebTransactionInput(this._chainId)
        {
            To = this._accountContract.Address,
            Value = new HexBigInteger(0),
            Data = data
        };
        var txHash = await this.SendTransaction(txInput).ConfigureAwait(false);
        return await ThirdwebTransaction.WaitForTransactionReceipt(this.Client, this._chainId, txHash).ConfigureAwait(false);
    }

    /// <summary>
    /// Removes an existing admin from the smart account.
    /// </summary>
    /// <param name="admin">The address of the admin to remove.</param>
    /// <returns>The transaction receipt.</returns>
    public async Task<ThirdwebTransactionReceipt> RemoveAdmin(string admin)
    {
        if (await Utils.IsZkSync(this.Client, this._chainId).ConfigureAwait(false))
        {
            throw new InvalidOperationException("Account Permissions are not supported in ZkSync");
        }

        var request = new SignerPermissionRequest()
        {
            Signer = admin,
            IsAdmin = 2,
            ApprovedTargets = new List<string>(),
            NativeTokenLimitPerTransaction = 0,
            PermissionStartTimestamp = Utils.GetUnixTimeStampNow() - 3600,
            PermissionEndTimestamp = Utils.GetUnixTimeStampIn10Years(),
            ReqValidityStartTimestamp = Utils.GetUnixTimeStampNow() - 3600,
            ReqValidityEndTimestamp = Utils.GetUnixTimeStampIn10Years(),
            Uid = Guid.NewGuid().ToByteArray()
        };

        var signature = await EIP712.GenerateSignature_SmartAccount("Account", "1", this._chainId, await this.GetAddress().ConfigureAwait(false), request, this._personalAccount).ConfigureAwait(false);
        var data = new Contract(null, this._accountContract.Abi, this._accountContract.Address).GetFunction("setPermissionsForSigner").GetData(request, signature.HexToBytes());
        var txInput = new ThirdwebTransactionInput(this._chainId)
        {
            To = this._accountContract.Address,
            Value = new HexBigInteger(0),
            Data = data
        };
        var txHash = await this.SendTransaction(txInput).ConfigureAwait(false);
        return await ThirdwebTransaction.WaitForTransactionReceipt(this.Client, this._chainId, txHash).ConfigureAwait(false);
    }

    /// <summary>
    /// Estimates the gas cost for a user operation. More accurate than ThirdwebTransaction estimation, but slower.
    /// </summary>
    /// <param name="transaction">The transaction to estimate.</param>
    /// <returns>The estimated gas cost.</returns>
    public async Task<BigInteger> EstimateUserOperationGas(ThirdwebTransactionInput transaction)
    {
        await this.SwitchNetwork(transaction.ChainId.Value).ConfigureAwait(false);

        if (await Utils.IsZkSync(this.Client, this._chainId).ConfigureAwait(false))
        {
            throw new Exception("User Operations are not supported in ZkSync");
        }

        var signedOp = await this.SignUserOp(transaction, null, simulation: true).ConfigureAwait(false);
        if (signedOp is UserOperationV6)
        {
            var castSignedOp = signedOp as UserOperationV6;
            var cost = castSignedOp.CallGasLimit + castSignedOp.VerificationGasLimit + castSignedOp.PreVerificationGas;
            return cost;
        }
        else if (signedOp is UserOperationV7)
        {
            var castSignedOp = signedOp as UserOperationV7;
            var cost =
                castSignedOp.CallGasLimit + castSignedOp.VerificationGasLimit + castSignedOp.PreVerificationGas + castSignedOp.PaymasterVerificationGasLimit + castSignedOp.PaymasterPostOpGasLimit;
            return cost;
        }
        else
        {
            throw new Exception("Invalid signed operation type");
        }
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
        var data = createFunction.GetData(personalAccountAddress, Array.Empty<byte>());
        return (Utils.HexConcat(this._factoryContract.Address, data).HexToBytes(), this._factoryContract.Address, data);
    }

    private async Task<object> SignUserOp(ThirdwebTransactionInput transactionInput, int? requestId = null, bool simulation = false)
    {
        requestId ??= 1;

        (var initCode, var factory, var factoryData) = await this.GetInitCode().ConfigureAwait(false);

        // Approve tokens if ERC20Paymaster
        if (this.UseERC20Paymaster && !this._isApproving && !this._isApproved && !simulation)
        {
            try
            {
                this._isApproving = true;
                var tokenContract = await ThirdwebContract.Create(this.Client, this._erc20PaymasterToken, this._chainId).ConfigureAwait(false);
                var approvedAmount = await tokenContract.ERC20_Allowance(this._accountContract.Address, this._erc20PaymasterAddress).ConfigureAwait(false);
                if (approvedAmount == 0)
                {
                    _ = await tokenContract.ERC20_Approve(this, this._erc20PaymasterAddress, BigInteger.Pow(2, 96) - 1).ConfigureAwait(false);
                }
                this._isApproved = true;
                await ThirdwebTask.Delay(1000).ConfigureAwait(false);
                (initCode, factory, factoryData) = await this.GetInitCode().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                this._isApproved = false;
                throw new Exception($"Approving tokens for ERC20Paymaster spending failed: {e.Message}");
            }
            finally
            {
                this._isApproving = false;
            }
        }

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

        var entryPointVersion = Utils.GetEntryPointVersion(this._entryPointContract.Address);

        if (entryPointVersion == 6)
        {
            var executeFn = new ExecuteFunction
            {
                Target = transactionInput.To,
                Value = transactionInput.Value.Value,
                Calldata = transactionInput.Data.HexToBytes(),
                FromAddress = await this.GetAddress().ConfigureAwait(false),
            };
            var executeInput = executeFn.CreateTransactionInput(await this.GetAddress().ConfigureAwait(false));

            var partialUserOp = new UserOperationV6()
            {
                Sender = this._accountContract.Address,
                Nonce = await this.GetNonce().ConfigureAwait(false),
                InitCode = initCode,
                CallData = executeInput.Data.HexToBytes(),
                CallGasLimit = transactionInput.Gas == null ? 0 : 21000 + transactionInput.Gas.Value,
                VerificationGasLimit = 0,
                PreVerificationGas = 0,
                MaxFeePerGas = maxFee,
                MaxPriorityFeePerGas = maxPriorityFee,
                PaymasterAndData = Array.Empty<byte>(),
                Signature = Constants.DUMMY_SIG.HexToBytes(),
            };

            // Update paymaster data and gas

            var pmSponsorResult = await this.GetPaymasterAndData(requestId, EncodeUserOperation(partialUserOp), simulation).ConfigureAwait(false);
            partialUserOp.PaymasterAndData = pmSponsorResult.PaymasterAndData.HexToBytes();

            if (pmSponsorResult.VerificationGasLimit == null || pmSponsorResult.PreVerificationGas == null)
            {
                var gasEstimates = await BundlerClient.EthEstimateUserOperationGas(this.Client, this._bundlerUrl, requestId, EncodeUserOperation(partialUserOp), this._entryPointContract.Address);
                partialUserOp.CallGasLimit = new HexBigInteger(gasEstimates.CallGasLimit).Value;
                partialUserOp.VerificationGasLimit = new HexBigInteger(gasEstimates.VerificationGasLimit).Value;
                partialUserOp.PreVerificationGas = new HexBigInteger(gasEstimates.PreVerificationGas).Value;
            }
            else
            {
                partialUserOp.CallGasLimit = new HexBigInteger(pmSponsorResult.CallGasLimit).Value;
                partialUserOp.VerificationGasLimit = new HexBigInteger(pmSponsorResult.VerificationGasLimit).Value;
                partialUserOp.PreVerificationGas = new HexBigInteger(pmSponsorResult.PreVerificationGas).Value;
            }

            // Hash, sign and encode the user operation

            if (!simulation)
            {
                partialUserOp.Signature = await this.HashAndSignUserOp(partialUserOp, this._entryPointContract).ConfigureAwait(false);
            }

            return partialUserOp;
        }
        else
        {
            var executeFn = new ExecuteFunction
            {
                Target = transactionInput.To,
                Value = transactionInput.Value.Value,
                Calldata = transactionInput.Data.HexToBytes(),
                FromAddress = await this.GetAddress().ConfigureAwait(false),
            };
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

            var pmSponsorResult = await this.GetPaymasterAndData(requestId, EncodeUserOperation(partialUserOp), simulation).ConfigureAwait(false);
            partialUserOp.Paymaster = pmSponsorResult.Paymaster;
            partialUserOp.PaymasterData = pmSponsorResult.PaymasterData?.HexToBytes() ?? Array.Empty<byte>();

            Dictionary<string, object> stateDict = null;

            if (this.UseERC20Paymaster && !this._isApproving)
            {
                var abiEncoder = new ABIEncode();
                var slotBytes = abiEncoder.GetABIEncoded(new ABIValue("address", this._accountContract.Address), new ABIValue("uint256", this._erc20PaymasterStorageSlot));
                var desiredBalance = BigInteger.Pow(2, 96) - 1;
                var storageDict = new Dictionary<string, string> { { new Sha3Keccack().CalculateHash(slotBytes).BytesToHex(), desiredBalance.ToHexBigInteger().HexValue.HexToBytes32().BytesToHex() } };
                stateDict = new Dictionary<string, object> { { this._erc20PaymasterToken, new { stateDiff = storageDict } } };
            }
            else
            {
                partialUserOp.PreVerificationGas = new HexBigInteger(pmSponsorResult.PreVerificationGas ?? "0x0").Value;
                partialUserOp.VerificationGasLimit = new HexBigInteger(pmSponsorResult.VerificationGasLimit ?? "0x0").Value;
                partialUserOp.CallGasLimit = new HexBigInteger(pmSponsorResult.CallGasLimit ?? "0x0").Value;
                partialUserOp.PaymasterVerificationGasLimit = new HexBigInteger(pmSponsorResult.PaymasterVerificationGasLimit ?? "0x0").Value;
                partialUserOp.PaymasterPostOpGasLimit = new HexBigInteger(pmSponsorResult.PaymasterPostOpGasLimit ?? "0x0").Value;
            }

            if (partialUserOp.PreVerificationGas == 0 || partialUserOp.VerificationGasLimit == 0)
            {
                var gasEstimates = await BundlerClient
                    .EthEstimateUserOperationGas(this.Client, this._bundlerUrl, requestId, EncodeUserOperation(partialUserOp), this._entryPointContract.Address, stateDict)
                    .ConfigureAwait(false);
                partialUserOp.CallGasLimit = new HexBigInteger(gasEstimates.CallGasLimit).Value;
                partialUserOp.VerificationGasLimit = new HexBigInteger(gasEstimates.VerificationGasLimit).Value;
                partialUserOp.PreVerificationGas = new HexBigInteger(gasEstimates.PreVerificationGas).Value;
                partialUserOp.PaymasterVerificationGasLimit = new HexBigInteger(gasEstimates.PaymasterVerificationGasLimit).Value;
                partialUserOp.PaymasterPostOpGasLimit = this.UseERC20Paymaster && !this._isApproving ? 500_000 : new HexBigInteger(gasEstimates.PaymasterPostOpGasLimit).Value;
            }

            // Hash, sign and encode the user operation

            partialUserOp.Signature = await this.HashAndSignUserOp(partialUserOp, this._entryPointContract).ConfigureAwait(false);

            return partialUserOp;
        }
    }

    private async Task<string> SendUserOp(object userOperation, int? requestId = null)
    {
        requestId ??= 1;

        // Encode op

        object encodedOp;
        if (userOperation is UserOperationV6)
        {
            encodedOp = EncodeUserOperation(userOperation as UserOperationV6);
        }
        else
        {
            encodedOp = userOperation is UserOperationV7 ? (object)EncodeUserOperation(userOperation as UserOperationV7) : throw new Exception("Invalid signed operation type");
        }

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
        var randomBytes = new byte[24];
        RandomNumberGenerator.Fill(randomBytes);
        BigInteger randomInt192 = new(randomBytes);
        randomInt192 = BigInteger.Abs(randomInt192) % (BigInteger.One << 192);
        return await ThirdwebContract.Read<BigInteger>(this._entryPointContract, "getNonce", await this.GetAddress().ConfigureAwait(false), randomInt192).ConfigureAwait(false);
    }

    private async Task<(string, string)> ZkPaymasterData(ThirdwebTransactionInput transactionInput)
    {
        if (this._gasless)
        {
            var result = await BundlerClient.ZkPaymasterData(this.Client, this._paymasterUrl, 1, transactionInput).ConfigureAwait(false);
            return (result.Paymaster, result.PaymasterInput);
        }
        else
        {
            return (null, null);
        }
    }

    private async Task<string> ZkBroadcastTransaction(object transactionInput)
    {
        var result = await BundlerClient.ZkBroadcastTransaction(this.Client, this._bundlerUrl, 1, transactionInput).ConfigureAwait(false);
        return result.TransactionHash;
    }

    private async Task<PMSponsorOperationResponse> GetPaymasterAndData(object requestId, object userOp, bool simulation)
    {
        if (this.UseERC20Paymaster && !this._isApproving && !simulation)
        {
            return new PMSponsorOperationResponse()
            {
                PaymasterAndData = Utils.HexConcat(this._erc20PaymasterAddress, this._erc20PaymasterToken),
                Paymaster = this._erc20PaymasterAddress,
                PaymasterData = "0x",
            };
        }
        else
        {
            return this._gasless
                ? await BundlerClient.PMSponsorUserOperation(this.Client, this._paymasterUrl, requestId, userOp, this._entryPointContract.Address).ConfigureAwait(false)
                : new PMSponsorOperationResponse();
        }
    }

    private async Task<byte[]> HashAndSignUserOp(UserOperationV6 userOp, ThirdwebContract entryPointContract)
    {
        var userOpHash = await ThirdwebContract.Read<byte[]>(entryPointContract, "getUserOpHash", userOp);
        var sig =
            this._personalAccount.AccountType == ThirdwebAccountType.ExternalAccount
                ? await this._personalAccount.PersonalSign(userOpHash.BytesToHex()).ConfigureAwait(false)
                : await this._personalAccount.PersonalSign(userOpHash).ConfigureAwait(false);
        return sig.HexToBytes();
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

    private static UserOperationHexifiedV6 EncodeUserOperation(UserOperationV6 userOperation)
    {
        return new UserOperationHexifiedV6()
        {
            Sender = userOperation.Sender,
            Nonce = userOperation.Nonce.ToHexBigInteger().HexValue,
            InitCode = userOperation.InitCode.BytesToHex(),
            CallData = userOperation.CallData.BytesToHex(),
            CallGasLimit = userOperation.CallGasLimit.ToHexBigInteger().HexValue,
            VerificationGasLimit = userOperation.VerificationGasLimit.ToHexBigInteger().HexValue,
            PreVerificationGas = userOperation.PreVerificationGas.ToHexBigInteger().HexValue,
            MaxFeePerGas = userOperation.MaxFeePerGas.ToHexBigInteger().HexValue,
            MaxPriorityFeePerGas = userOperation.MaxPriorityFeePerGas.ToHexBigInteger().HexValue,
            PaymasterAndData = userOperation.PaymasterAndData.BytesToHex(),
            Signature = userOperation.Signature.BytesToHex()
        };
    }

    private static UserOperationHexifiedV7 EncodeUserOperation(UserOperationV7 userOperation)
    {
        return new UserOperationHexifiedV7()
        {
            Sender = userOperation.Sender,
            Nonce = Utils.HexConcat(Constants.ADDRESS_ZERO, userOperation.Nonce.ToHexBigInteger().HexValue),
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

    #endregion

    #region IThirdwebWallet

    public async Task<string> SendTransaction(ThirdwebTransactionInput transactionInput)
    {
        if (transactionInput == null)
        {
            throw new InvalidOperationException("SmartAccount.SendTransaction: Transaction input is required.");
        }

        await this.SwitchNetwork(transactionInput.ChainId.Value).ConfigureAwait(false);

        var transaction = await ThirdwebTransaction
            .Create(await Utils.IsZkSync(this.Client, this._chainId).ConfigureAwait(false) ? this._personalAccount : this, transactionInput)
            .ConfigureAwait(false);
        transaction = await ThirdwebTransaction.Prepare(transaction).ConfigureAwait(false);
        transactionInput = transaction.Input;

        if (await Utils.IsZkSync(this.Client, this._chainId).ConfigureAwait(false))
        {
            if (this._gasless)
            {
                (var paymaster, var paymasterInput) = await this.ZkPaymasterData(transactionInput).ConfigureAwait(false);
                transaction = transaction.SetZkSyncOptions(new ZkSyncOptions(paymaster: paymaster, paymasterInput: paymasterInput));
                var zkTx = await ThirdwebTransaction.ConvertToZkSyncTransaction(transaction).ConfigureAwait(false);
                var zkTxSigned = await EIP712.GenerateSignature_ZkSyncTransaction("zkSync", "2", transaction.Input.ChainId.Value, zkTx, this).ConfigureAwait(false);
                // Match bundler ZkTransactionInput type without recreating
                var hash = await this.ZkBroadcastTransaction(
                        new
                        {
                            nonce = zkTx.Nonce.ToString(),
                            from = zkTx.From,
                            to = zkTx.To,
                            gas = zkTx.GasLimit.ToString(),
                            gasPrice = string.Empty,
                            value = zkTx.Value.ToString(),
                            data = Utils.BytesToHex(zkTx.Data),
                            maxFeePerGas = zkTx.MaxFeePerGas.ToString(),
                            maxPriorityFeePerGas = zkTx.MaxPriorityFeePerGas.ToString(),
                            chainId = this._chainId.ToString(),
                            signedTransaction = zkTxSigned,
                            paymaster
                        }
                    )
                    .ConfigureAwait(false);
                return hash;
            }
            else
            {
                return await ThirdwebTransaction.Send(transaction).ConfigureAwait(false);
            }
        }
        else
        {
            var signedOp = await this.SignUserOp(transactionInput).ConfigureAwait(false);
            return await this.SendUserOp(signedOp).ConfigureAwait(false);
        }
    }

    public async Task<ThirdwebTransactionReceipt> ExecuteTransaction(ThirdwebTransactionInput transactionInput)
    {
        var txHash = await this.SendTransaction(transactionInput).ConfigureAwait(false);
        return await ThirdwebTransaction.WaitForTransactionReceipt(this.Client, this._chainId, txHash).ConfigureAwait(false);
    }

    public async Task<string> GetAddress()
    {
        return await Utils.IsZkSync(this.Client, this._chainId).ConfigureAwait(false)
            ? await this._personalAccount.GetAddress().ConfigureAwait(false)
            : this._accountContract.Address.ToChecksumAddress();
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

    /// <summary>
    /// Signs a message with the personal account. If the smart account is deployed, the message will be wrapped 712 and signed by the smart account and verified with 1271. If the smart account is not deployed, it will deploy it first.
    /// </summary>
    /// <param name="message">The message to sign.</param>
    /// <returns>The signature.</returns>
    public async Task<string> PersonalSign(string message)
    {
        if (await Utils.IsZkSync(this.Client, this._chainId).ConfigureAwait(false))
        {
            return await this._personalAccount.PersonalSign(message).ConfigureAwait(false);
        }

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

    public Task<string> SignTypedDataV4(string json)
    {
        // TODO: Implement wrapped version
        return this._personalAccount.SignTypedDataV4(json);
    }

    public Task<string> SignTypedDataV4<T, TDomain>(T data, TypedData<TDomain> typedData)
        where TDomain : IDomain
    {
        // TODO: Implement wrapped version
        return this._personalAccount.SignTypedDataV4(data, typedData);
    }

    public Task<string> RecoverAddressFromTypedDataV4<T, TDomain>(T data, TypedData<TDomain> typedData, string signature)
        where TDomain : IDomain
    {
        return this._personalAccount.RecoverAddressFromTypedDataV4(data, typedData, signature);
    }

    public async Task<string> SignTransaction(ThirdwebTransactionInput transaction)
    {
        await this.SwitchNetwork(transaction.ChainId.Value).ConfigureAwait(false);

        if (await Utils.IsZkSync(this.Client, this._chainId).ConfigureAwait(false))
        {
            throw new Exception("Offline Signing is not supported in ZkSync");
        }

        var signedOp = await this.SignUserOp(transaction).ConfigureAwait(false);
        if (signedOp is UserOperationV6)
        {
            var encodedOp = EncodeUserOperation(signedOp as UserOperationV6);
            return JsonConvert.SerializeObject(encodedOp);
        }
        else if (signedOp is UserOperationV7)
        {
            var encodedOp = EncodeUserOperation(signedOp as UserOperationV7);
            return JsonConvert.SerializeObject(encodedOp);
        }
        else
        {
            throw new Exception("Invalid signed operation type");
        }
    }

    public async Task<bool> IsConnected()
    {
        return await Utils.IsZkSync(this.Client, this._chainId).ConfigureAwait(false) ? await this._personalAccount.IsConnected().ConfigureAwait(false) : this._accountContract != null;
    }

    public Task Disconnect()
    {
        this._accountContract = null;
        return Task.CompletedTask;
    }

    public async Task<List<LinkedAccount>> LinkAccount(
        IThirdwebWallet walletToLink,
        string otp = null,
        bool? isMobile = null,
        Action<string> browserOpenAction = null,
        string mobileRedirectScheme = "thirdweb://",
        IThirdwebBrowser browser = null,
        BigInteger? chainId = null,
        string jwt = null,
        string payload = null
    )
    {
        var personalWallet = await this.GetPersonalWallet().ConfigureAwait(false);
        if (personalWallet is not InAppWallet and not EcosystemWallet)
        {
            throw new Exception("SmartWallet.LinkAccount is only supported if the signer is an InAppWallet or EcosystemWallet");
        }
        else if (walletToLink is not InAppWallet and not EcosystemWallet)
        {
            throw new Exception("SmartWallet.LinkAccount is only supported if the wallet to link is an InAppWallet or EcosystemWallet");
        }
        else if (personalWallet is InAppWallet && walletToLink is not InAppWallet)
        {
            throw new Exception("SmartWallet.LinkAccount with an InAppWallet signer is only supported if the wallet to link is also an InAppWallet");
        }
        else if (personalWallet is EcosystemWallet && walletToLink is not EcosystemWallet)
        {
            throw new Exception("SmartWallet.LinkAccount with an EcosystemWallet signer is only supported if the wallet to link is also an EcosystemWallet");
        }
        else
        {
            return await personalWallet.LinkAccount(walletToLink, otp, isMobile, browserOpenAction, mobileRedirectScheme, browser, chainId, jwt, payload).ConfigureAwait(false);
        }
    }

    public async Task<List<LinkedAccount>> GetLinkedAccounts()
    {
        var personalWallet = await this.GetPersonalWallet().ConfigureAwait(false);
        if (personalWallet is not InAppWallet and not EcosystemWallet)
        {
            throw new Exception("SmartWallet.LinkAccount is only supported if the signer is an InAppWallet or EcosystemWallet");
        }
        else
        {
            return await personalWallet.GetLinkedAccounts().ConfigureAwait(false);
        }
    }

    #endregion
}
