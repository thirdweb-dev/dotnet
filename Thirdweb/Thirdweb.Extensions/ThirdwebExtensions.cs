using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.Util;
using Newtonsoft.Json;

namespace Thirdweb;

public static class ThirdwebExtensions
{
    #region Common

    public static async Task<bool> SupportsInterface(this ThirdwebContract contract, string interfaceId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        return await ThirdwebContract.Read<bool>(contract, "supportsInterface", interfaceId.HexToBytes());
    }

    /// <summary>
    /// Reads data from the contract using the specified method.
    /// </summary>
    /// <typeparam name="T">The type of the return value.</typeparam>
    /// <param name="contract">The contract instance.</param>
    /// <param name="method">The method to call.</param>
    /// <param name="parameters">The parameters for the method.</param>
    /// <returns>The result of the method call.</returns>
    public static async Task<T> Read<T>(this ThirdwebContract contract, string method, params object[] parameters)
    {
        return await ThirdwebContract.Read<T>(contract, method, parameters);
    }

    /// <summary>
    /// Writes data to the contract using the specified method and parameters.
    /// </summary>
    /// <param name="contract">The contract instance.</param>
    /// <param name="wallet">The wallet instance.</param>
    /// <param name="method">The method to call.</param>
    /// <param name="weiValue">The value in wei to send.</param>
    /// <param name="parameters">The parameters for the method.</param>
    /// <returns>A transaction receipt.</returns>
    public static async Task<ThirdwebTransactionReceipt> Write(this ThirdwebContract contract, IThirdwebWallet wallet, string method, BigInteger weiValue, params object[] parameters)
    {
        return await ThirdwebContract.Write(wallet, contract, method, weiValue, parameters);
    }

    /// <summary>
    /// Prepares a transaction for the specified method and parameters.
    /// </summary>
    /// <param name="contract">The contract instance.</param>
    /// <param name="wallet">The wallet instance.</param>
    /// <param name="method">The method to call.</param>
    /// <param name="weiValue">The value in wei to send.</param>
    /// <param name="parameters">The parameters for the method.</param>
    /// <returns>A prepared transaction.</returns>
    public static async Task<ThirdwebTransaction> Prepare(this ThirdwebContract contract, IThirdwebWallet wallet, string method, BigInteger weiValue, params object[] parameters)
    {
        return await ThirdwebContract.Prepare(wallet, contract, method, weiValue, parameters);
    }

    /// <summary>
    /// Retrieves the metadata of the specified contract.
    /// </summary>
    /// <param name="contract">The contract to retrieve metadata for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the contract metadata.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    public static async Task<ContractMetadata> GetMetadata(this ThirdwebContract contract)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        var contractUri = await ThirdwebContract.Read<string>(contract, "contractURI");

        return await ThirdwebStorage.Download<ContractMetadata>(contract.Client, contractUri);
    }

    /// <summary>
    /// Retrieves the image bytes of the specified NFT.
    /// </summary>
    /// <param name="nft">The NFT to retrieve the image bytes for.</param>
    /// <param name="client">The client used to download the image bytes.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the image bytes.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the client is null.</exception>
    public static async Task<byte[]> GetNFTImageBytes(this NFT nft, ThirdwebClient client)
    {
        return client == null
            ? throw new ArgumentNullException(nameof(client))
            : string.IsNullOrEmpty(nft.Metadata.Image)
                ? Array.Empty<byte>()
                : await ThirdwebStorage.Download<byte[]>(client, nft.Metadata.Image).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves the default royalty information of the specified contract.
    /// </summary>
    /// <param name="contract">The contract to retrieve the default royalty information for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the royalty information.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    public static async Task<RoyaltyInfoResult> GetDefaultRoyaltyInfo(this ThirdwebContract contract)
    {
        return contract == null ? throw new ArgumentNullException(nameof(contract)) : await ThirdwebContract.Read<RoyaltyInfoResult>(contract, "getDefaultRoyaltyInfo");
    }

    /// <summary>
    /// Retrieves the primary sale recipient address of the specified contract.
    /// </summary>
    /// <param name="contract">The contract to retrieve the primary sale recipient address for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the primary sale recipient address.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    public static async Task<string> GetPrimarySaleRecipient(this ThirdwebContract contract)
    {
        return contract == null ? throw new ArgumentNullException(nameof(contract)) : await ThirdwebContract.Read<string>(contract, "primarySaleRecipient");
    }

    /// <summary>
    /// Retrieves the balance of the specified address on the specified chain.
    /// </summary>
    /// <param name="client">The client used to retrieve the balance.</param>
    /// <param name="chainId">The chain ID to retrieve the balance from.</param>
    /// <param name="address">The address to retrieve the balance for.</param>
    /// <param name="erc20ContractAddress">The optional ERC20 contract address to retrieve the balance from.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the balance in Wei.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the client is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the chain ID is less than or equal to 0.</exception>
    /// <exception cref="ArgumentException">Thrown when the address is null or empty.</exception>
    public static async Task<BigInteger> GetBalanceRaw(ThirdwebClient client, BigInteger chainId, string address, string erc20ContractAddress = null)
    {
        if (client == null)
        {
            throw new ArgumentNullException(nameof(client));
        }

        if (chainId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(chainId), "Chain ID must be greater than 0.");
        }

        if (string.IsNullOrEmpty(address))
        {
            throw new ArgumentException("Address must be provided");
        }

        if (erc20ContractAddress != null)
        {
            var erc20Contract = await ThirdwebContract.Create(client, erc20ContractAddress, chainId).ConfigureAwait(false);
            return await erc20Contract.ERC20_BalanceOf(address).ConfigureAwait(false);
        }

        var rpc = ThirdwebRPC.GetRpcInstance(client, chainId);
        var balanceHex = await rpc.SendRequestAsync<string>("eth_getBalance", address, "latest").ConfigureAwait(false);
        return new HexBigInteger(balanceHex).Value;
    }

    /// <summary>
    /// Retrieves the balance of the specified contract.
    /// </summary>
    /// <param name="contract">The contract to retrieve the balance for.</param>
    /// <param name="erc20ContractAddress">The optional ERC20 contract address to retrieve the balance from.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the balance in Wei.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    public static async Task<BigInteger> GetBalance(this ThirdwebContract contract, string erc20ContractAddress = null)
    {
        return contract == null
            ? throw new ArgumentNullException(nameof(contract))
            : await GetBalanceRaw(contract.Client, contract.Chain, contract.Address, erc20ContractAddress).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves the balance of the specified wallet on the specified chain.
    /// </summary>
    /// <param name="wallet">The wallet to retrieve the balance for.</param>
    /// <param name="chainId">The chain ID to retrieve the balance from.</param>
    /// <param name="erc20ContractAddress">The optional ERC20 contract address to retrieve the balance from.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the balance in Wei.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the wallet is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the chain ID is less than or equal to 0.</exception>
    public static async Task<BigInteger> GetBalance(this IThirdwebWallet wallet, BigInteger chainId, string erc20ContractAddress = null)
    {
        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        if (chainId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(chainId), "Chain ID must be greater than 0.");
        }

        var address = await wallet.GetAddress().ConfigureAwait(false);
        return await GetBalanceRaw(wallet.Client, chainId, address, erc20ContractAddress).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves the transaction count (i.e. nonce) of the specified address on the specified chain.
    /// </summary>
    /// <param name="client">The client used to retrieve the transaction count.</param>
    /// <param name="chainId">The chain ID to retrieve the transaction count from.</param>
    /// <param name="address">The address to retrieve the transaction count for.</param>
    /// <param name="blocktag">The block tag to retrieve the transaction count at. Defaults to "pending".</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the transaction count.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the client is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the chain ID is less than or equal to 0.</exception>
    /// <exception cref="ArgumentException">Thrown when the address is null or empty.</exception>
    public static async Task<BigInteger> GetTransactionCountRaw(ThirdwebClient client, BigInteger chainId, string address, string blocktag = "pending")
    {
        if (client == null)
        {
            throw new ArgumentNullException(nameof(client));
        }

        if (chainId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(chainId), "Chain ID must be greater than 0.");
        }

        if (string.IsNullOrEmpty(address))
        {
            throw new ArgumentException("Address must be provided");
        }

        var rpc = ThirdwebRPC.GetRpcInstance(client, chainId);
        var balanceHex = await rpc.SendRequestAsync<string>("eth_getTransactionCount", address, blocktag).ConfigureAwait(false);
        return new HexBigInteger(balanceHex).Value;
    }

    /// <summary>
    /// Retrieves the transaction count (i.e. nonce) of the specified contract.
    /// </summary>
    /// <param name="contract">The contract to retrieve the transaction count for.</param>
    /// <param name="blocktag">The block tag to retrieve the transaction count at. Defaults to "pending".</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the transaction count.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    public static async Task<BigInteger> GetTransactionCount(this ThirdwebContract contract, string blocktag = "pending")
    {
        return contract == null ? throw new ArgumentNullException(nameof(contract)) : await GetTransactionCountRaw(contract.Client, contract.Chain, contract.Address, blocktag).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves the transaction count (i.e. nonce) of the specified wallet on the specified chain.
    /// </summary>
    /// <param name="wallet">The wallet to retrieve the transaction count for.</param>
    /// <param name="chainId">The chain ID to retrieve the transaction count from.</param>
    /// <param name="blocktag">The block tag to retrieve the transaction count at. Defaults to "pending".</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the transaction count.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the wallet is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the chain ID is less than or equal to 0.</exception>
    public static async Task<BigInteger> GetTransactionCount(this IThirdwebWallet wallet, BigInteger chainId, string blocktag = "pending")
    {
        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        if (chainId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(chainId), "Chain ID must be greater than 0.");
        }

        var address = await wallet.GetAddress().ConfigureAwait(false);
        return await GetTransactionCountRaw(wallet.Client, chainId, address, blocktag).ConfigureAwait(false);
    }

    /// <summary>
    /// Transfers the specified amount of Wei to the specified address. Passing tokenAddress will override this function to transfer ERC20 tokens.
    /// </summary>
    /// <param name="wallet">The wallet to transfer from.</param>
    /// <param name="chainId">The chain ID to transfer on.</param>
    /// <param name="toAddress">The address to transfer to.</param>
    /// <param name="weiAmount">The amount of Wei to transfer.</param>
    /// <param name="tokenAddress">The optional token address to transfer from. Defaults to the native token address (ETH or equivalent).</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the transaction receipt.</returns>
    /// <returns>A task that represents the asynchronous operation. The task result contains the transaction receipt.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the wallet is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the chain ID is less than or equal to 0.</exception>
    /// <exception cref="ArgumentException">Thrown when the recipient address is null or empty.</exception>
    public static async Task<ThirdwebTransactionReceipt> Transfer(
        this IThirdwebWallet wallet,
        BigInteger chainId,
        string toAddress,
        BigInteger weiAmount,
        string tokenAddress = Constants.NATIVE_TOKEN_ADDRESS
    )
    {
        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        if (chainId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(chainId), "Chain ID must be greater than 0.");
        }

        if (string.IsNullOrEmpty(toAddress))
        {
            throw new ArgumentException(nameof(toAddress), "Recipient address cannot be null or empty.");
        }

        if (weiAmount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(weiAmount), "Amount must be 0 or greater.");
        }

        if (tokenAddress != Constants.NATIVE_TOKEN_ADDRESS)
        {
            var erc20Contract = await ThirdwebContract.Create(wallet.Client, tokenAddress, chainId).ConfigureAwait(false);
            return await erc20Contract.ERC20_Transfer(wallet, toAddress, weiAmount).ConfigureAwait(false);
        }
        else
        {
            var txInput = new ThirdwebTransactionInput(chainId) { To = toAddress, Value = new HexBigInteger(weiAmount) };
            var tx = await ThirdwebTransaction.Create(wallet, txInput).ConfigureAwait(false);
            return await ThirdwebTransaction.SendAndWaitForTransactionReceipt(tx).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Authenticates the wallet.
    /// </summary>
    /// <param name="wallet">The wallet that will sign the SIWE payload.</param>
    /// <param name="domain">The authentication domain.</param>
    /// <param name="chainId">The chain ID.</param>
    /// <param name="authPayloadPath">The authentication payload path.</param>
    /// <param name="authLoginPath">The authentication login path.</param>
    /// <param name="httpClientOverride">The HTTP client override.</param>
    /// <param name="authPayloadMethod">The authentication payload method.</param>
    /// <param name="authLoginMethod">The authentication login method.</param>
    /// <param name="separatePayloadAndSignatureInBody">Whether to separate the payload and signature in the body.</param>
    /// <returns>The authentication result.</returns>
    public static async Task<T> Authenticate<T>(
        this IThirdwebWallet wallet,
        string domain,
        BigInteger chainId,
        string authPayloadPath = "/auth/payload",
        string authLoginPath = "/auth/login",
        string authPayloadMethod = "GET",
        string authLoginMethod = "POST",
        bool separatePayloadAndSignatureInBody = false,
        IThirdwebHttpClient httpClientOverride = null
    )
    {
        var payloadURL = domain + authPayloadPath;
        var loginURL = domain + authLoginPath;

        var payloadBodyRaw = new { address = await wallet.GetAddress(), chainId = chainId.ToString() };
        var payloadBody = JsonConvert.SerializeObject(payloadBodyRaw);

        var httpClient = httpClientOverride ?? wallet.Client.HttpClient;

        var payloadContent = new StringContent(payloadBody, System.Text.Encoding.UTF8, "application/json");
        ThirdwebHttpResponseMessage payloadResponse;
        if (authPayloadMethod == "GET")
        {
            payloadURL += "?address=" + await wallet.GetAddress() + "&chainId=" + chainId.ToString();
            payloadResponse = await httpClient.GetAsync(payloadURL);
        }
        else if (authPayloadMethod == "POST")
        {
            payloadResponse = await httpClient.PostAsync(payloadURL, payloadContent);
        }
        else
        {
            throw new Exception("Unsupported HTTP method for auth payload");
        }

        _ = payloadResponse.EnsureSuccessStatusCode();
        var payloadString = await payloadResponse.Content.ReadAsStringAsync();

        var loginBodyRaw = JsonConvert.DeserializeObject<LoginPayload>(payloadString);
        if (loginBodyRaw.Payload == null)
        {
            var payloadDataRaw = JsonConvert.DeserializeObject<LoginPayloadData>(payloadString);
            loginBodyRaw.Payload = payloadDataRaw;
        }

        var payloadToSign = Utils.GenerateSIWE(loginBodyRaw.Payload);
        loginBodyRaw.Signature = await wallet.PersonalSign(payloadToSign);

        var loginBody = JsonConvert.SerializeObject(separatePayloadAndSignatureInBody ? new { payload = loginBodyRaw.Payload, signature = loginBodyRaw.Signature } : new { payload = loginBodyRaw });

        var loginContent = new StringContent(loginBody, System.Text.Encoding.UTF8, "application/json");
        ThirdwebHttpResponseMessage loginResponse;
        if (authLoginMethod == "GET")
        {
            loginURL = loginURL + "?payload=" + JsonConvert.SerializeObject(loginBodyRaw);
            loginResponse = await httpClient.GetAsync(loginURL);
        }
        else if (authLoginMethod == "POST")
        {
            loginResponse = await httpClient.PostAsync(loginURL, loginContent);
        }
        else
        {
            throw new Exception("Unsupported HTTP method for auth login");
        }
        _ = loginResponse.EnsureSuccessStatusCode();

        if (typeof(T) == typeof(byte[]))
        {
            return (T)(object)await loginResponse.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        }
        else if (typeof(T) == typeof(string))
        {
            return (T)(object)await loginResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
        else
        {
            var content = await loginResponse.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(System.Text.Encoding.UTF8.GetString(content));
        }
    }

    #endregion

    #region ERC20

    /// <summary>
    /// Check the balance of a specific address.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="ownerAddress">The address of the owner whose balance is to be checked.</param>
    /// <returns>A task representing the asynchronous operation, with a BigInteger result containing the balance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the owner address is null or empty.</exception>
    public static async Task<BigInteger> ERC20_BalanceOf(this ThirdwebContract contract, string ownerAddress)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        return string.IsNullOrEmpty(ownerAddress) ? throw new ArgumentException("Owner address must be provided") : await ThirdwebContract.Read<BigInteger>(contract, "balanceOf", ownerAddress);
    }

    /// <summary>
    /// Get the total supply of the token.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <returns>A task representing the asynchronous operation, with a BigInteger result containing the total supply.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    public static async Task<BigInteger> ERC20_TotalSupply(this ThirdwebContract contract)
    {
        return contract == null ? throw new ArgumentNullException(nameof(contract)) : await ThirdwebContract.Read<BigInteger>(contract, "totalSupply");
    }

    /// <summary>
    /// Get the number of decimals used by the token.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <returns>A task representing the asynchronous operation, with an int result containing the number of decimals.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    public static async Task<int> ERC20_Decimals(this ThirdwebContract contract)
    {
        return contract == null ? throw new ArgumentNullException(nameof(contract)) : await ThirdwebContract.Read<int>(contract, "decimals");
    }

    /// <summary>
    /// Get the symbol of the token.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <returns>A task representing the asynchronous operation, with a string result containing the symbol.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    public static async Task<string> ERC20_Symbol(this ThirdwebContract contract)
    {
        return contract == null ? throw new ArgumentNullException(nameof(contract)) : await ThirdwebContract.Read<string>(contract, "symbol");
    }

    /// <summary>
    /// Get the name of the token.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <returns>A task representing the asynchronous operation, with a string result containing the name.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    public static async Task<string> ERC20_Name(this ThirdwebContract contract)
    {
        return contract == null ? throw new ArgumentNullException(nameof(contract)) : await ThirdwebContract.Read<string>(contract, "name");
    }

    /// <summary>
    /// Get the allowance of a spender for a specific owner.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="ownerAddress">The address of the owner.</param>
    /// <param name="spenderAddress">The address of the spender.</param>
    /// <returns>A task representing the asynchronous operation, with a BigInteger result containing the allowance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the owner address or spender address is null or empty.</exception>
    public static async Task<BigInteger> ERC20_Allowance(this ThirdwebContract contract, string ownerAddress, string spenderAddress)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (string.IsNullOrEmpty(ownerAddress))
        {
            throw new ArgumentException("Owner address must be provided");
        }

        return string.IsNullOrEmpty(spenderAddress)
            ? throw new ArgumentException("Spender address must be provided")
            : await ThirdwebContract.Read<BigInteger>(contract, "allowance", ownerAddress, spenderAddress);
    }

    /// <summary>
    /// Approve a spender to spend a specific amount of tokens.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="wallet">The wallet to use for the transaction.</param>
    /// <param name="spenderAddress">The address of the spender.</param>
    /// <param name="amount">The amount of tokens to approve.</param>
    /// <returns>A task representing the asynchronous operation, with a ThirdwebTransactionReceipt result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract or wallet is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the spender address is null or empty.</exception>
    public static async Task<ThirdwebTransactionReceipt> ERC20_Approve(this ThirdwebContract contract, IThirdwebWallet wallet, string spenderAddress, BigInteger amount)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        return string.IsNullOrEmpty(spenderAddress)
            ? throw new ArgumentException("Spender address must be provided")
            : await ThirdwebContract.Write(wallet, contract, "approve", 0, spenderAddress, amount);
    }

    /// <summary>
    /// Transfer tokens to a specific address.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="wallet">The wallet to use for the transaction.</param>
    /// <param name="toAddress">The address of the recipient.</param>
    /// <param name="amount">The amount of tokens to transfer.</param>
    /// <returns>A task representing the asynchronous operation, with a ThirdwebTransactionReceipt result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract or wallet is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the recipient address is null or empty.</exception>
    public static async Task<ThirdwebTransactionReceipt> ERC20_Transfer(this ThirdwebContract contract, IThirdwebWallet wallet, string toAddress, BigInteger amount)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        return string.IsNullOrEmpty(toAddress) ? throw new ArgumentException("Recipient address must be provided") : await ThirdwebContract.Write(wallet, contract, "transfer", 0, toAddress, amount);
    }

    /// <summary>
    /// Transfer tokens from one address to another.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="wallet">The wallet to use for the transaction.</param>
    /// <param name="fromAddress">The address of the sender.</param>
    /// <param name="toAddress">The address of the recipient.</param>
    /// <param name="amount">The amount of tokens to transfer.</param>
    /// <returns>A task representing the asynchronous operation, with a ThirdwebTransactionReceipt result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract or wallet is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the sender address or recipient address is null or empty.</exception>
    public static async Task<ThirdwebTransactionReceipt> ERC20_TransferFrom(this ThirdwebContract contract, IThirdwebWallet wallet, string fromAddress, string toAddress, BigInteger amount)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        if (string.IsNullOrEmpty(fromAddress))
        {
            throw new ArgumentException("Sender address must be provided");
        }

        return string.IsNullOrEmpty(toAddress)
            ? throw new ArgumentException("Recipient address must be provided")
            : await ThirdwebContract.Write(wallet, contract, "transferFrom", 0, fromAddress, toAddress, amount);
    }

    #endregion

    #region ERC721A

    public static async Task<List<BigInteger>> ERC721A_TokensOfOwner(this ThirdwebContract contract, string ownerAddress)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        return string.IsNullOrEmpty(ownerAddress)
            ? throw new ArgumentException("Owner address must be provided")
            : await ThirdwebContract.Read<List<BigInteger>>(contract, "tokensOfOwner", ownerAddress);
    }

    public static async Task<List<BigInteger>> ERC721A_TokensOfOwnerIn(this ThirdwebContract contract, string ownerAddress, BigInteger startIndex, BigInteger endIndex)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        return string.IsNullOrEmpty(ownerAddress)
            ? throw new ArgumentException("Owner address must be provided")
            : await ThirdwebContract.Read<List<BigInteger>>(contract, "tokensOfOwnerIn", ownerAddress, startIndex, endIndex);
    }

    #endregion

    #region ERC721

    /// <summary>
    /// Get the total supply of the token.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <returns>A task representing the asynchronous operation, with a BigInteger result containing the total supply.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    public static async Task<BigInteger> ERC721_TotalSupply(this ThirdwebContract contract)
    {
        return contract == null ? throw new ArgumentNullException(nameof(contract)) : await ThirdwebContract.Read<BigInteger>(contract, "totalSupply");
    }

    /// <summary>
    /// Get the token ID of a specific owner by index.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="ownerAddress">The address of the owner.</param>
    /// <param name="index">The index of the token.</param>
    /// <returns>A task representing the asynchronous operation, with a BigInteger result containing the token ID.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the owner address is null or empty.</exception>
    public static async Task<BigInteger> ERC721_TokenOfOwnerByIndex(this ThirdwebContract contract, string ownerAddress, BigInteger index)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        return string.IsNullOrEmpty(ownerAddress)
            ? throw new ArgumentException("Owner address must be provided")
            : await ThirdwebContract.Read<BigInteger>(contract, "tokenOfOwnerByIndex", ownerAddress, index);
    }

    /// <summary>
    /// Get the token ID of a specific token by index.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="index">The index of the token.</param>
    /// <returns>A task representing the asynchronous operation, with a BigInteger result containing the token ID.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    public static async Task<BigInteger> ERC721_TokenByIndex(this ThirdwebContract contract, BigInteger index)
    {
        return contract == null ? throw new ArgumentNullException(nameof(contract)) : await ThirdwebContract.Read<BigInteger>(contract, "tokenByIndex", index);
    }

    /// <summary>
    /// Check the balance of a specific address.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="ownerAddress">The address of the owner whose balance is to be checked.</param>
    /// <returns>A task representing the asynchronous operation, with a BigInteger result containing the balance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the owner address is null or empty.</exception>
    public static async Task<BigInteger> ERC721_BalanceOf(this ThirdwebContract contract, string ownerAddress)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        return string.IsNullOrEmpty(ownerAddress) ? throw new ArgumentException("Owner address must be provided") : await ThirdwebContract.Read<BigInteger>(contract, "balanceOf", ownerAddress);
    }

    /// <summary>
    /// Get the owner of a specific token.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="tokenId">The ID of the token.</param>
    /// <returns>A task representing the asynchronous operation, with a string result containing the owner's address.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the token ID is less than 0.</exception>
    public static async Task<string> ERC721_OwnerOf(this ThirdwebContract contract, BigInteger tokenId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        return tokenId < 0 ? throw new ArgumentOutOfRangeException(nameof(tokenId), "Token ID must be equal or greater than 0") : await ThirdwebContract.Read<string>(contract, "ownerOf", tokenId);
    }

    /// <summary>
    /// Get the name of the token.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <returns>A task representing the asynchronous operation, with a string result containing the name.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    public static async Task<string> ERC721_Name(this ThirdwebContract contract)
    {
        return contract == null ? throw new ArgumentNullException(nameof(contract)) : await ThirdwebContract.Read<string>(contract, "name");
    }

    /// <summary>
    /// Get the symbol of the token.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <returns>A task representing the asynchronous operation, with a string result containing the symbol.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    public static async Task<string> ERC721_Symbol(this ThirdwebContract contract)
    {
        return contract == null ? throw new ArgumentNullException(nameof(contract)) : await ThirdwebContract.Read<string>(contract, "symbol");
    }

    /// <summary>
    /// Get the URI of a specific token.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="tokenId">The ID of the token.</param>
    /// <returns>A task representing the asynchronous operation, with a string result containing the token URI.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the token ID is less than 0.</exception>
    public static async Task<string> ERC721_TokenURI(this ThirdwebContract contract, BigInteger tokenId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        return tokenId < 0 ? throw new ArgumentOutOfRangeException(nameof(tokenId), "Token ID must be equal or greater than 0") : await ThirdwebContract.Read<string>(contract, "tokenURI", tokenId);
    }

    /// <summary>
    /// Approve a specific address to transfer a specific token.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="wallet">The wallet to use for the transaction.</param>
    /// <param name="toAddress">The address of the recipient.</param>
    /// <param name="tokenId">The ID of the token.</param>
    /// <returns>A task representing the asynchronous operation, with a ThirdwebTransactionReceipt result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract or wallet is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the recipient address is null or empty.</exception>
    public static async Task<ThirdwebTransactionReceipt> ERC721_Approve(this ThirdwebContract contract, IThirdwebWallet wallet, string toAddress, BigInteger tokenId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        return string.IsNullOrEmpty(toAddress) ? throw new ArgumentException("Recipient address must be provided") : await ThirdwebContract.Write(wallet, contract, "approve", 0, toAddress, tokenId);
    }

    /// <summary>
    /// Get the approved address for a specific token.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="tokenId">The ID of the token.</param>
    /// <returns>A task representing the asynchronous operation, with a string result containing the approved address.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the token ID is less than 0.</exception>
    public static async Task<string> ERC721_GetApproved(this ThirdwebContract contract, BigInteger tokenId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        return tokenId < 0 ? throw new ArgumentOutOfRangeException(nameof(tokenId), "Token ID must be equal or greater than 0") : await ThirdwebContract.Read<string>(contract, "getApproved", tokenId);
    }

    /// <summary>
    /// Check if an address is an operator for another address.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="ownerAddress">The address of the owner.</param>
    /// <param name="operatorAddress">The address of the operator.</param>
    /// <returns>A task representing the asynchronous operation, with a boolean result indicating if the operator is approved for the owner.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the owner address or operator address is null or empty.</exception>
    public static async Task<bool> ERC721_IsApprovedForAll(this ThirdwebContract contract, string ownerAddress, string operatorAddress)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (string.IsNullOrEmpty(ownerAddress))
        {
            throw new ArgumentException("Owner address must be provided");
        }

        return string.IsNullOrEmpty(operatorAddress)
            ? throw new ArgumentException("Operator address must be provided")
            : await ThirdwebContract.Read<bool>(contract, "isApprovedForAll", ownerAddress, operatorAddress);
    }

    /// <summary>
    /// Set or unset an operator for an owner.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="wallet">The wallet to use for the transaction.</param>
    /// <param name="operatorAddress">The address of the operator.</param>
    /// <param name="approved">A boolean indicating whether to set or unset the operator.</param>
    /// <returns>A task representing the asynchronous operation, with a ThirdwebTransactionReceipt result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract or wallet is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the operator address is null or empty.</exception>
    public static async Task<ThirdwebTransactionReceipt> ERC721_SetApprovalForAll(this ThirdwebContract contract, IThirdwebWallet wallet, string operatorAddress, bool approved)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        return string.IsNullOrEmpty(operatorAddress)
            ? throw new ArgumentException("Operator address must be provided")
            : await ThirdwebContract.Write(wallet, contract, "setApprovalForAll", 0, operatorAddress, approved);
    }

    /// <summary>
    /// Transfer a specific token from one address to another.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="wallet">The wallet to use for the transaction.</param>
    /// <param name="fromAddress">The address of the sender.</param>
    /// <param name="toAddress">The address of the recipient.</param>
    /// <param name="tokenId">The ID of the token.</param>
    /// <returns>A task representing the asynchronous operation, with a ThirdwebTransactionReceipt result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract or wallet is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the sender address or recipient address is null or empty.</exception>
    public static async Task<ThirdwebTransactionReceipt> ERC721_TransferFrom(this ThirdwebContract contract, IThirdwebWallet wallet, string fromAddress, string toAddress, BigInteger tokenId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        if (string.IsNullOrEmpty(fromAddress))
        {
            throw new ArgumentException("Sender address must be provided");
        }

        return string.IsNullOrEmpty(toAddress)
            ? throw new ArgumentException("Recipient address must be provided")
            : await ThirdwebContract.Write(wallet, contract, "transferFrom", 0, fromAddress, toAddress, tokenId);
    }

    /// <summary>
    /// Safely transfer a specific token from one address to another.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="wallet">The wallet to use for the transaction.</param>
    /// <param name="fromAddress">The address of the sender.</param>
    /// <param name="toAddress">The address of the recipient.</param>
    /// <param name="tokenId">The ID of the token.</param>
    /// <returns>A task representing the asynchronous operation, with a ThirdwebTransactionReceipt result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract or wallet is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the sender address or recipient address is null or empty.</exception>
    public static async Task<ThirdwebTransactionReceipt> ERC721_SafeTransferFrom(this ThirdwebContract contract, IThirdwebWallet wallet, string fromAddress, string toAddress, BigInteger tokenId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        if (string.IsNullOrEmpty(fromAddress))
        {
            throw new ArgumentException("Sender address must be provided");
        }

        return string.IsNullOrEmpty(toAddress)
            ? throw new ArgumentException("Recipient address must be provided")
            : await ThirdwebContract.Write(wallet, contract, "safeTransferFrom", 0, fromAddress, toAddress, tokenId);
    }

    #endregion

    #region ERC1155

    /// <summary>
    /// Check the balance of a specific token for a specific address.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="ownerAddress">The address of the owner whose balance is to be checked.</param>
    /// <param name="tokenId">The ID of the token.</param>
    /// <returns>A task representing the asynchronous operation, with a BigInteger result containing the balance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the owner address is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the token ID is less than 0.</exception>
    public static async Task<BigInteger> ERC1155_BalanceOf(this ThirdwebContract contract, string ownerAddress, BigInteger tokenId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (string.IsNullOrEmpty(ownerAddress))
        {
            throw new ArgumentException("Owner address must be provided");
        }

        return tokenId < 0
            ? throw new ArgumentOutOfRangeException(nameof(tokenId), "Token ID must be equal or greater than 0")
            : await ThirdwebContract.Read<BigInteger>(contract, "balanceOf", ownerAddress, tokenId);
    }

    /// <summary>
    /// Check the balance of multiple tokens for multiple addresses.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="ownerAddresses">The array of owner addresses.</param>
    /// <param name="tokenIds">The array of token IDs.</param>
    /// <returns>A task representing the asynchronous operation, with a list of BigInteger results containing the balances.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the owner addresses or token IDs are null.</exception>
    public static async Task<List<BigInteger>> ERC1155_BalanceOfBatch(this ThirdwebContract contract, string[] ownerAddresses, BigInteger[] tokenIds)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        return ownerAddresses == null || tokenIds == null
            ? throw new ArgumentException("Owner addresses and token IDs must be provided")
            : await ThirdwebContract.Read<List<BigInteger>>(contract, "balanceOfBatch", ownerAddresses, tokenIds);
    }

    /// <summary>
    /// Approve a specific address to transfer specific tokens.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="wallet">The wallet to use for the transaction.</param>
    /// <param name="operatorAddress">The address of the operator.</param>
    /// <param name="approved">A boolean indicating whether to approve or revoke approval.</param>
    /// <returns>A task representing the asynchronous operation, with a ThirdwebTransactionReceipt result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract or wallet is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the operator address is null or empty.</exception>
    public static async Task<ThirdwebTransactionReceipt> ERC1155_SetApprovalForAll(this ThirdwebContract contract, IThirdwebWallet wallet, string operatorAddress, bool approved)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        return string.IsNullOrEmpty(operatorAddress)
            ? throw new ArgumentException("Operator address must be provided")
            : await ThirdwebContract.Write(wallet, contract, "setApprovalForAll", 0, operatorAddress, approved);
    }

    /// <summary>
    /// Check if an address is approved to transfer specific tokens.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="ownerAddress">The address of the owner.</param>
    /// <param name="operatorAddress">The address of the operator.</param>
    /// <returns>A task representing the asynchronous operation, with a boolean result indicating if the operator is approved for the owner.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the owner address or operator address is null or empty.</exception>
    public static async Task<bool> ERC1155_IsApprovedForAll(this ThirdwebContract contract, string ownerAddress, string operatorAddress)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (string.IsNullOrEmpty(ownerAddress))
        {
            throw new ArgumentException("Owner address must be provided");
        }

        return string.IsNullOrEmpty(operatorAddress)
            ? throw new ArgumentException("Operator address must be provided")
            : await ThirdwebContract.Read<bool>(contract, "isApprovedForAll", ownerAddress, operatorAddress);
    }

    /// <summary>
    /// Transfer specific tokens from one address to another.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="wallet">The wallet to use for the transaction.</param>
    /// <param name="fromAddress">The address of the sender.</param>
    /// <param name="toAddress">The address of the recipient.</param>
    /// <param name="tokenId">The ID of the token.</param>
    /// <param name="amount">The amount of tokens to transfer.</param>
    /// <param name="data">Additional data with no specified format.</param>
    /// <returns>A task representing the asynchronous operation, with a ThirdwebTransactionReceipt result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract or wallet is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the sender address or recipient address is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the token ID is less than 0.</exception>
    public static async Task<ThirdwebTransactionReceipt> ERC1155_SafeTransferFrom(
        this ThirdwebContract contract,
        IThirdwebWallet wallet,
        string fromAddress,
        string toAddress,
        BigInteger tokenId,
        BigInteger amount,
        byte[] data = null
    )
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        if (string.IsNullOrEmpty(fromAddress))
        {
            throw new ArgumentException("Sender address must be provided");
        }

        if (string.IsNullOrEmpty(toAddress))
        {
            throw new ArgumentException("Recipient address must be provided");
        }

        return tokenId < 0
            ? throw new ArgumentOutOfRangeException(nameof(tokenId), "Token ID must be equal or greater than 0")
            : await ThirdwebContract.Write(wallet, contract, "safeTransferFrom", 0, fromAddress, toAddress, tokenId, amount, data ?? Array.Empty<byte>());
    }

    /// <summary>
    /// Transfer multiple tokens from one address to another.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="wallet">The wallet to use for the transaction.</param>
    /// <param name="fromAddress">The address of the sender.</param>
    /// <param name="toAddress">The address of the recipient.</param>
    /// <param name="tokenIds">The array of token IDs to transfer.</param>
    /// <param name="amounts">The array of amounts for each token ID.</param>
    /// <param name="data">Additional data with no specified format.</param>
    /// <returns>A task representing the asynchronous operation, with a ThirdwebTransactionReceipt result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract or wallet is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the sender address, recipient address, token IDs, or amounts are null or empty.</exception>
    public static async Task<ThirdwebTransactionReceipt> ERC1155_SafeBatchTransferFrom(
        this ThirdwebContract contract,
        IThirdwebWallet wallet,
        string fromAddress,
        string toAddress,
        BigInteger[] tokenIds,
        BigInteger[] amounts,
        byte[] data = null
    )
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        if (string.IsNullOrEmpty(fromAddress))
        {
            throw new ArgumentException("Sender address must be provided");
        }

        if (string.IsNullOrEmpty(toAddress))
        {
            throw new ArgumentException("Recipient address must be provided");
        }

        return tokenIds == null || amounts == null
            ? throw new ArgumentException("Token IDs and amounts must be provided")
            : await ThirdwebContract.Write(wallet, contract, "safeBatchTransferFrom", 0, fromAddress, toAddress, tokenIds, amounts, data ?? Array.Empty<byte>());
    }

    /// <summary>
    /// Get the URI for a specific token.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="tokenId">The ID of the token.</param>
    /// <returns>A task representing the asynchronous operation, with a string result containing the URI.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the token ID is less than 0.</exception>
    public static async Task<string> ERC1155_URI(this ThirdwebContract contract, BigInteger tokenId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        return tokenId < 0 ? throw new ArgumentOutOfRangeException(nameof(tokenId), "Token ID must be equal or greater than 0") : await ThirdwebContract.Read<string>(contract, "uri", tokenId);
    }

    /// <summary>
    /// Get the total supply of a specific token.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="tokenId">The ID of the token.</param>
    /// <returns>A task representing the asynchronous operation, with a BigInteger result containing the total supply.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the token ID is less than 0.</exception>
    public static async Task<BigInteger> ERC1155_TotalSupply(this ThirdwebContract contract, BigInteger tokenId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        return tokenId < 0
            ? throw new ArgumentOutOfRangeException(nameof(tokenId), "Token ID must be equal or greater than 0")
            : await ThirdwebContract.Read<BigInteger>(contract, "totalSupply", tokenId);
    }

    /// <summary>
    /// Get the total supply of tokens.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <returns>A task representing the asynchronous operation, with a BigInteger result containing the total supply.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    public static async Task<BigInteger> ERC1155_TotalSupply(this ThirdwebContract contract)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        try
        {
            return await ThirdwebContract.Read<BigInteger>(contract, "nextTokenIdToMint");
        }
        catch (Exception)
        {
            return await ThirdwebContract.Read<BigInteger>(contract, "totalSupply");
        }
    }

    #endregion

    #region NFT

    /// <summary>
    /// Get the details of a specific ERC721 token.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="tokenId">The ID of the token.</param>
    /// <param name="fillOwner">A boolean indicating whether to fill the owner details. Defaults to true.</param>
    /// <returns>A task representing the asynchronous operation, with an NFT result containing the token details.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    public static async Task<NFT> ERC721_GetNFT(this ThirdwebContract contract, BigInteger tokenId, bool fillOwner = true)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        var uri = await contract.ERC721_TokenURI(tokenId).ConfigureAwait(false);
        NFTMetadata metadata;
        try
        {
            metadata = await ThirdwebStorage.Download<NFTMetadata>(contract.Client, uri).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            metadata = new NFTMetadata { Description = e.Message };
        }
        metadata.Id = tokenId.ToString();

        var owner = Constants.ADDRESS_ZERO;
        if (fillOwner)
        {
            try
            {
                owner = await contract.ERC721_OwnerOf(tokenId).ConfigureAwait(false);
            }
            catch (Exception)
            {
                owner = Constants.ADDRESS_ZERO;
            }
        }

        return new NFT
        {
            Metadata = metadata,
            Owner = owner,
            Type = NFTType.ERC721,
            Supply = 1,
            QuantityOwned = 1
        };
    }

    /// <summary>
    /// Get a list of all ERC721 tokens within a specified range.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="startTokenId">The starting token ID (inclusive). Defaults to 0 if not specified.</param>
    /// <param name="count">The number of tokens to retrieve. Defaults to 100 if not specified.</param>
    /// <param name="fillOwner">A boolean indicating whether to fill the owner details. Defaults to true.</param>
    /// <returns>A task representing the asynchronous operation, with a list of NFT results containing the token details.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    public static async Task<List<NFT>> ERC721_GetAllNFTs(this ThirdwebContract contract, int startTokenId = 0, int count = 100, bool fillOwner = true)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        var totalSupply = await contract.ERC721_TotalSupply().ConfigureAwait(false);
        count = Math.Min(count, (int)(totalSupply - startTokenId));

        var nftTasks = new List<Task<NFT>>();
        for (var i = startTokenId; i < startTokenId + count; i++)
        {
            nftTasks.Add(contract.ERC721_GetNFT(i, fillOwner));
        }

        var allNfts = await Task.WhenAll(nftTasks).ConfigureAwait(false);
        return allNfts.ToList();
    }

    /// <summary>
    /// Get a list of ERC721 tokens owned by a specific address.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="owner">The address of the owner.</param>
    /// <param name="startTokenId">The starting token ID (inclusive). Defaults to 0 if not specified.</param>
    /// <param name="count">The number of tokens to retrieve. Defaults to 100 if not specified.</param>
    /// <returns>A task representing the asynchronous operation, with a list of NFT results containing the token details.</returns>
    /// <remarks>ERC721AQueryable and ERC721Enumerable are supported.</remarks>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the owner address is null or empty.</exception>
    public static async Task<List<NFT>> ERC721_GetOwnedNFTs(this ThirdwebContract contract, string owner, int startTokenId = 0, int count = 100)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (string.IsNullOrEmpty(owner))
        {
            throw new ArgumentException("Owner must be provided");
        }

        var totalSupply = await contract.ERC721_TotalSupply().ConfigureAwait(false);
        count = Math.Min(count, (int)(totalSupply - startTokenId));

        // ERC721AQueryable
        try
        {
            var tokensOfOwnerIn = await contract.ERC721A_TokensOfOwnerIn(owner, startTokenId, startTokenId + count).ConfigureAwait(false);
            var ownedNftTasks = new List<Task<NFT>>();
            foreach (var tokenId in tokensOfOwnerIn)
            {
                ownedNftTasks.Add(contract.ERC721_GetNFT(tokenId));
            }
            var ownedNfts = await Task.WhenAll(ownedNftTasks).ConfigureAwait(false);
            return ownedNfts.ToList();
        }
        catch
        {
            // ERC721Enumerable
            try
            {
                var balanceOfOwner = await contract.ERC721_BalanceOf(owner).ConfigureAwait(false);
                var ownedNftTasks = new List<Task<NFT>>();
                for (var i = 0; i < balanceOfOwner; i++)
                {
                    var tokenOfOwnerByIndex = await contract.ERC721_TokenOfOwnerByIndex(owner, i).ConfigureAwait(false);
                    if (tokenOfOwnerByIndex >= startTokenId && tokenOfOwnerByIndex < startTokenId + count)
                    {
                        ownedNftTasks.Add(contract.ERC721_GetNFT(tokenOfOwnerByIndex));
                    }
                }
                var ownedNfts = await Task.WhenAll(ownedNftTasks).ConfigureAwait(false);
                return ownedNfts.ToList();
            }
            catch (Exception)
            {
                var allNfts = await contract.ERC721_GetAllNFTs(startTokenId, count).ConfigureAwait(false);
                var ownedNfts = new List<NFT>();
                foreach (var nft in allNfts)
                {
                    if (nft.Owner == owner)
                    {
                        ownedNfts.Add(nft);
                    }
                }
                return ownedNfts.ToList();
            }
        }
    }

    /// <summary>
    /// Get the details of a specific ERC1155 token.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="tokenId">The ID of the token.</param>
    /// <param name="fillSupply">A boolean indicating whether to fill the supply. Defaults to true if not specified.</param>
    /// <returns>A task representing the asynchronous operation, with an NFT result containing the token details.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    public static async Task<NFT> ERC1155_GetNFT(this ThirdwebContract contract, BigInteger tokenId, bool fillSupply = true)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        var uri = await contract.ERC1155_URI(tokenId).ConfigureAwait(false);
        NFTMetadata metadata;
        try
        {
            metadata = await ThirdwebStorage.Download<NFTMetadata>(contract.Client, uri).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            metadata = new NFTMetadata { Description = e.Message };
        }
        metadata.Id = tokenId.ToString();

        var supply = BigInteger.MinusOne;
        if (fillSupply)
        {
            try
            {
                supply = await contract.ERC1155_TotalSupply(tokenId).ConfigureAwait(false);
            }
            catch (Exception)
            {
                supply = BigInteger.MinusOne;
            }
        }

        return new NFT
        {
            Metadata = metadata,
            Owner = "",
            Type = NFTType.ERC1155,
            Supply = supply,
        };
    }

    /// <summary>
    /// Get a list of all ERC1155 tokens within a specified range.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="startTokenId">The starting token ID (inclusive). Defaults to 0 if not specified.</param>
    /// <param name="count">The number of tokens to retrieve. Defaults to the 100 if not specified.</param>
    /// <param name="fillSupply">A boolean indicating whether to fill the supply. Defaults to true if not specified.</param>
    /// <returns>A task representing the asynchronous operation, with a list of NFT results containing the token details.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    public static async Task<List<NFT>> ERC1155_GetAllNFTs(this ThirdwebContract contract, int startTokenId = 0, int count = 100, bool fillSupply = true)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        BigInteger totalCount;
        try
        {
            // Not part of IERC1155 so we fallback just in case
            totalCount = await contract.ERC1155_TotalSupply().ConfigureAwait(false);
        }
        catch
        {
            totalCount = int.MaxValue;
        }
        count = Math.Min(count, (int)(totalCount - startTokenId));

        var nftTasks = new List<Task<NFT>>();
        for (var i = startTokenId; i < startTokenId + count; i++)
        {
            nftTasks.Add(contract.ERC1155_GetNFT(i, fillSupply));
        }

        var allNfts = await Task.WhenAll(nftTasks).ConfigureAwait(false);
        return allNfts.ToList();
    }

    /// <summary>
    /// Get a list of ERC1155 tokens owned by a specific address.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="owner">The address of the owner.</param>
    /// <param name="startTokenId">The starting token ID (inclusive). Defaults to 0 if not specified.</param>
    /// <param name="count">The number of tokens to retrieve. Defaults to 100 if not specified.</param>
    /// <returns>A task representing the asynchronous operation, with a list of NFT results containing the token details.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the owner address is null or empty.</exception>
    public static async Task<List<NFT>> ERC1155_GetOwnedNFTs(this ThirdwebContract contract, string owner, int startTokenId = 0, int count = 100)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (string.IsNullOrEmpty(owner))
        {
            throw new ArgumentException("Owner must be provided");
        }

        BigInteger totalSupply;
        try
        {
            // Not part of IERC1155 so we fallback just in case
            totalSupply = await contract.ERC1155_TotalSupply().ConfigureAwait(false);
        }
        catch
        {
            totalSupply = int.MaxValue;
        }
        count = Math.Min(count, (int)(totalSupply - startTokenId));

        var ownerArray = new List<string>();
        var tokenIds = new List<BigInteger>();

        for (var i = startTokenId; i < startTokenId + count; i++)
        {
            ownerArray.Add(owner);
            tokenIds.Add(i);
        }

        var balanceOfBatch = await contract.ERC1155_BalanceOfBatch(ownerArray.ToArray(), tokenIds.ToArray()).ConfigureAwait(false);

        var ownerNftTasks = new List<Task<NFT>>();
        for (var i = 0; i < balanceOfBatch.Count; i++)
        {
            if (balanceOfBatch[i] > 0)
            {
                ownerNftTasks.Add(contract.ERC1155_GetNFT(tokenIds[i]));
            }
        }

        var ownerNfts = await Task.WhenAll(ownerNftTasks).ConfigureAwait(false);
        for (var i = 0; i < ownerNfts.Length; i++)
        {
            ownerNfts[i].QuantityOwned = balanceOfBatch[i];
        }
        return ownerNfts.ToList();
    }

    #endregion

    #region DropERC20

    /// <summary>
    /// Claim a specific amount of ERC20 tokens for a receiver.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="wallet">The wallet to use for the transaction.</param>
    /// <param name="receiverAddress">The address of the receiver.</param>
    /// <param name="amount">The amount of tokens to claim.</param>
    /// <returns>A task representing the asynchronous operation, with a ThirdwebTransactionReceipt result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract or wallet is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the receiver address or amount is null or empty.</exception>
    public static async Task<ThirdwebTransactionReceipt> DropERC20_Claim(this ThirdwebContract contract, IThirdwebWallet wallet, string receiverAddress, string amount)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        if (string.IsNullOrEmpty(receiverAddress))
        {
            throw new ArgumentException("Receiver address must be provided");
        }

        if (string.IsNullOrEmpty(amount))
        {
            throw new ArgumentException("Amount must be provided");
        }

        // TODO: Task.WhenAll

        var activeClaimCondition = await contract.DropERC20_GetActiveClaimCondition();

        var erc20Decimals = await contract.ERC20_Decimals();

        var rawAmountToClaim = BigInteger.Parse(amount.ToWei()).AdjustDecimals(18, erc20Decimals);

        var isNativeToken = activeClaimCondition.Currency == Constants.NATIVE_TOKEN_ADDRESS;

        var payableAmount = isNativeToken ? rawAmountToClaim * activeClaimCondition.PricePerToken : BigInteger.Zero;

        // TODO: Merkle
        var allowlistProof = new object[] { Array.Empty<byte>(), BigInteger.Zero, BigInteger.Zero, Constants.ADDRESS_ZERO };

        var fnArgs = new object[]
        {
            receiverAddress, // receiver
            rawAmountToClaim, // quantity
            activeClaimCondition.Currency, // currency
            activeClaimCondition.PricePerToken, // pricePerToken
            allowlistProof, // allowlistProof
            Array.Empty<byte>() // data
        };

        return await ThirdwebContract.Write(wallet, contract, "claim", payableAmount, fnArgs);
    }

    /// <summary>
    /// Get the active claim condition ID for the ERC20 drop.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <returns>A task representing the asynchronous operation, with a BigInteger result containing the active claim condition ID.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    public static async Task<BigInteger> DropERC20_GetActiveClaimConditionId(this ThirdwebContract contract)
    {
        return contract == null ? throw new ArgumentNullException(nameof(contract)) : await ThirdwebContract.Read<BigInteger>(contract, "getActiveClaimConditionId");
    }

    /// <summary>
    /// Get the claim condition details for a specific claim condition ID.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="claimConditionId">The ID of the claim condition.</param>
    /// <returns>A task representing the asynchronous operation, with a Drop_ClaimCondition result containing the claim condition details.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the claim condition ID is less than 0.</exception>
    public static async Task<Drop_ClaimCondition> DropERC20_GetClaimConditionById(this ThirdwebContract contract, BigInteger claimConditionId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        return claimConditionId < 0
            ? throw new ArgumentOutOfRangeException(nameof(claimConditionId), "Claim condition ID must be equal or greater than 0")
            : await ThirdwebContract.Read<Drop_ClaimCondition>(contract, "getClaimConditionById", claimConditionId);
    }

    /// <summary>
    /// Get the details of the active claim condition for the ERC20 drop.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <returns>A task representing the asynchronous operation, with a Drop_ClaimCondition result containing the active claim condition details.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    public static async Task<Drop_ClaimCondition> DropERC20_GetActiveClaimCondition(this ThirdwebContract contract)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        var activeClaimConditionId = await contract.DropERC20_GetActiveClaimConditionId();
        return await contract.DropERC20_GetClaimConditionById(activeClaimConditionId);
    }

    #endregion

    #region DropERC721

    /// <summary>
    /// Claim a specific quantity of ERC721 tokens for a receiver.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="wallet">The wallet to use for the transaction.</param>
    /// <param name="receiverAddress">The address of the receiver.</param>
    /// <param name="quantity">The quantity of tokens to claim.</param>
    /// <returns>A task representing the asynchronous operation, with a ThirdwebTransactionReceipt result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract or wallet is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the receiver address is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the quantity is less than or equal to 0.</exception>
    public static async Task<ThirdwebTransactionReceipt> DropERC721_Claim(this ThirdwebContract contract, IThirdwebWallet wallet, string receiverAddress, BigInteger quantity)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        if (string.IsNullOrEmpty(receiverAddress))
        {
            throw new ArgumentException("Receiver address must be provided");
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than 0");
        }

        // TODO: Task.WhenAll

        var activeClaimCondition = await contract.DropERC721_GetActiveClaimCondition();

        var isNativeToken = activeClaimCondition.Currency == Constants.NATIVE_TOKEN_ADDRESS;

        var payableAmount = isNativeToken ? quantity * activeClaimCondition.PricePerToken : BigInteger.Zero;

        // TODO: Merkle
        var allowlistProof = new object[] { Array.Empty<byte>(), BigInteger.Zero, BigInteger.Zero, Constants.ADDRESS_ZERO };

        var fnArgs = new object[]
        {
            receiverAddress, // receiver
            quantity, // quantity
            activeClaimCondition.Currency, // currency
            activeClaimCondition.PricePerToken, // pricePerToken
            allowlistProof, // allowlistProof
            Array.Empty<byte>() // data
        };

        return await ThirdwebContract.Write(wallet, contract, "claim", payableAmount, fnArgs);
    }

    /// <summary>
    /// Get the active claim condition ID for the ERC721 drop.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <returns>A task representing the asynchronous operation, with a BigInteger result containing the active claim condition ID.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    public static async Task<BigInteger> DropERC721_GetActiveClaimConditionId(this ThirdwebContract contract)
    {
        return contract == null ? throw new ArgumentNullException(nameof(contract)) : await ThirdwebContract.Read<BigInteger>(contract, "getActiveClaimConditionId");
    }

    /// <summary>
    /// Get the claim condition details for a specific claim condition ID.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="claimConditionId">The ID of the claim condition.</param>
    /// <returns>A task representing the asynchronous operation, with a Drop_ClaimCondition result containing the claim condition details.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the claim condition ID is less than 0.</exception>
    public static async Task<Drop_ClaimCondition> DropERC721_GetClaimConditionById(this ThirdwebContract contract, BigInteger claimConditionId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        return claimConditionId < 0
            ? throw new ArgumentOutOfRangeException(nameof(claimConditionId), "Claim condition ID must be equal or greater than 0")
            : await ThirdwebContract.Read<Drop_ClaimCondition>(contract, "getClaimConditionById", claimConditionId);
    }

    /// <summary>
    /// Get the details of the active claim condition for the ERC721 drop.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <returns>A task representing the asynchronous operation, with a Drop_ClaimCondition result containing the active claim condition details.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    public static async Task<Drop_ClaimCondition> DropERC721_GetActiveClaimCondition(this ThirdwebContract contract)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        var activeClaimConditionId = await contract.DropERC721_GetActiveClaimConditionId();
        return await contract.DropERC20_GetClaimConditionById(activeClaimConditionId);
    }

    #endregion

    #region DropERC1155

    /// <summary>
    /// Claim a specific quantity of ERC1155 tokens for a receiver.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="wallet">The wallet to use for the transaction.</param>
    /// <param name="receiverAddress">The address of the receiver.</param>
    /// <param name="tokenId">The ID of the token.</param>
    /// <param name="quantity">The quantity of tokens to claim.</param>
    /// <returns>A task representing the asynchronous operation, with a ThirdwebTransactionReceipt result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract or wallet is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the receiver address is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the token ID is less than 0 or the quantity is less than or equal to 0.</exception>
    public static async Task<ThirdwebTransactionReceipt> DropERC1155_Claim(this ThirdwebContract contract, IThirdwebWallet wallet, string receiverAddress, BigInteger tokenId, BigInteger quantity)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        if (string.IsNullOrEmpty(receiverAddress))
        {
            throw new ArgumentException("Receiver address must be provided");
        }

        if (tokenId < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tokenId), "Token ID must be equal or greater than 0");
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than 0");
        }

        // TODO: Task.WhenAll

        var activeClaimCondition = await contract.DropERC1155_GetActiveClaimCondition(tokenId);

        var isNativeToken = activeClaimCondition.Currency == Constants.NATIVE_TOKEN_ADDRESS;

        var payableAmount = isNativeToken ? quantity * activeClaimCondition.PricePerToken : BigInteger.Zero;

        // TODO: Merkle
        var allowlistProof = new object[] { Array.Empty<byte>(), BigInteger.Zero, BigInteger.Zero, Constants.ADDRESS_ZERO };

        var fnArgs = new object[]
        {
            receiverAddress, // receiver
            tokenId, // tokenId
            quantity, // quantity
            activeClaimCondition.Currency, // currency
            activeClaimCondition.PricePerToken, // pricePerToken
            allowlistProof, // allowlistProof
            Array.Empty<byte>() // data
        };

        return await ThirdwebContract.Write(wallet, contract, "claim", payableAmount, fnArgs);
    }

    /// <summary>
    /// Get the active claim condition ID for a specific ERC1155 token.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="tokenId">The ID of the token.</param>
    /// <returns>A task representing the asynchronous operation, with a BigInteger result containing the active claim condition ID.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the token ID is less than 0.</exception>
    public static async Task<BigInteger> DropERC1155_GetActiveClaimConditionId(this ThirdwebContract contract, BigInteger tokenId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        return tokenId < 0
            ? throw new ArgumentOutOfRangeException(nameof(tokenId), "Token ID must be equal or greater than 0")
            : await ThirdwebContract.Read<BigInteger>(contract, "getActiveClaimConditionId", tokenId);
    }

    /// <summary>
    /// Get the claim condition details for a specific claim condition ID of a specific ERC1155 token.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="tokenId">The ID of the token.</param>
    /// <param name="claimConditionId">The ID of the claim condition.</param>
    /// <returns>A task representing the asynchronous operation, with a Drop_ClaimCondition result containing the claim condition details.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the token ID or claim condition ID is less than 0.</exception>
    public static async Task<Drop_ClaimCondition> DropERC1155_GetClaimConditionById(this ThirdwebContract contract, BigInteger tokenId, BigInteger claimConditionId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (tokenId < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tokenId), "Token ID must be equal or greater than 0");
        }

        return claimConditionId < 0
            ? throw new ArgumentOutOfRangeException(nameof(claimConditionId), "Claim condition ID must be equal or greater than 0")
            : await ThirdwebContract.Read<Drop_ClaimCondition>(contract, "getClaimConditionById", tokenId, claimConditionId);
    }

    /// <summary>
    /// Get the details of the active claim condition for a specific ERC1155 token.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="tokenId">The ID of the token.</param>
    /// <returns>A task representing the asynchronous operation, with a Drop_ClaimCondition result containing the active claim condition details.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the token ID is less than 0.</exception>
    public static async Task<Drop_ClaimCondition> DropERC1155_GetActiveClaimCondition(this ThirdwebContract contract, BigInteger tokenId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (tokenId < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tokenId), "Token ID must be equal or greater than 0");
        }

        var activeClaimConditionId = await contract.DropERC1155_GetActiveClaimConditionId(tokenId);
        return await contract.DropERC1155_GetClaimConditionById(tokenId, activeClaimConditionId);
    }

    #endregion

    #region TokenERC20

    /// <summary>
    /// Mint a specific amount of ERC20 tokens to a receiver address.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="wallet">The wallet to use for the transaction.</param>
    /// <param name="receiverAddress">The address of the receiver.</param>
    /// <param name="amount">The amount of tokens to mint.</param>
    /// <returns>A task representing the asynchronous operation, with a ThirdwebTransactionReceipt result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract or wallet is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the receiver address or amount is null or empty.</exception>
    public static async Task<ThirdwebTransactionReceipt> TokenERC20_MintTo(this ThirdwebContract contract, IThirdwebWallet wallet, string receiverAddress, string amount)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        if (string.IsNullOrEmpty(receiverAddress))
        {
            throw new ArgumentException("Receiver address must be provided");
        }

        if (string.IsNullOrEmpty(amount))
        {
            throw new ArgumentException("Amount must be provided");
        }

        var erc20Decimals = await contract.ERC20_Decimals();

        var rawAmountToMint = BigInteger.Parse(amount.ToWei()).AdjustDecimals(18, erc20Decimals);

        return await ThirdwebContract.Write(wallet, contract, "mintTo", 0, receiverAddress, rawAmountToMint);
    }

    /// <summary>
    /// Mint ERC20 tokens with a signature.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="wallet">The wallet to use for the transaction.</param>
    /// <param name="mintRequest">The mint request containing the minting details.</param>
    /// <param name="signature">The signature to authorize the minting.</param>
    /// <returns>A task representing the asynchronous operation, with a ThirdwebTransactionReceipt result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract, wallet, or mint request is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the signature is null or empty.</exception>
    public static async Task<ThirdwebTransactionReceipt> TokenERC20_MintWithSignature(this ThirdwebContract contract, IThirdwebWallet wallet, TokenERC20_MintRequest mintRequest, string signature)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        if (mintRequest == null)
        {
            throw new ArgumentNullException(nameof(mintRequest));
        }

        if (string.IsNullOrEmpty(signature))
        {
            throw new ArgumentException("Signature must be provided");
        }

        var isNativeToken = mintRequest.Currency == Constants.NATIVE_TOKEN_ADDRESS;

        var payableAmount = isNativeToken ? mintRequest.Quantity * mintRequest.Price : BigInteger.Zero;

        return await ThirdwebContract.Write(wallet, contract, "mintWithSignature", payableAmount, mintRequest, signature.HexToBytes());
    }

    /// <summary>
    /// Generate a mint signature for ERC20 tokens.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="wallet">The wallet to use for generating the signature.</param>
    /// <param name="mintRequest">The mint request containing the minting details.</param>
    /// <returns>A task representing the asynchronous operation, with a tuple containing the mint request and the generated signature.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract, wallet, or mint request is null.</exception>
    public static async Task<(TokenERC20_MintRequest, string)> TokenERC20_GenerateMintSignature(this ThirdwebContract contract, IThirdwebWallet wallet, TokenERC20_MintRequest mintRequest)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        if (mintRequest == null)
        {
            throw new ArgumentNullException(nameof(mintRequest));
        }

        mintRequest = new TokenERC20_MintRequest
        {
            To = mintRequest.To ?? throw new ArgumentNullException(nameof(mintRequest.To)),
            PrimarySaleRecipient = mintRequest.PrimarySaleRecipient ?? await contract.GetPrimarySaleRecipient(),
            Quantity = mintRequest.Quantity > 0 ? mintRequest.Quantity : 1,
            Price = mintRequest.Price,
            Currency = mintRequest.Currency ?? Constants.NATIVE_TOKEN_ADDRESS,
            ValidityStartTimestamp = mintRequest.ValidityStartTimestamp,
            ValidityEndTimestamp = mintRequest.ValidityEndTimestamp > 0 ? mintRequest.ValidityEndTimestamp : Utils.GetUnixTimeStampIn10Years(),
            Uid = mintRequest.Uid ?? Guid.NewGuid().ToByteArray().PadTo32Bytes()
        };

        var contractMetadata = await contract.GetMetadata();

        var signature = await EIP712.GenerateSignature_TokenERC20(
            domainName: contractMetadata.Name,
            version: "1",
            chainId: contract.Chain,
            verifyingContract: contract.Address,
            mintRequest: mintRequest,
            signer: wallet
        );

        return (mintRequest, signature);
    }

    /// <summary>
    /// Verify a mint signature for ERC20 tokens.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="mintRequest">The mint request containing the minting details.</param>
    /// <param name="signature">The signature to verify.</param>
    /// <returns>A task representing the asynchronous operation, with a VerifyResult result containing the verification details.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract or mint request is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the signature is null or empty.</exception>
    public static async Task<VerifyResult> TokenERC20_VerifyMintSignature(this ThirdwebContract contract, TokenERC20_MintRequest mintRequest, string signature)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (mintRequest == null)
        {
            throw new ArgumentNullException(nameof(mintRequest));
        }

        return string.IsNullOrEmpty(signature)
            ? throw new ArgumentException("Signature must be provided")
            : await ThirdwebContract.Read<VerifyResult>(contract, "verify", mintRequest, signature.HexToBytes());
    }

    #endregion

    #region TokenERC721

    /// <summary>
    /// Mint a specific ERC721 token to a receiver address with a given URI.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="wallet">The wallet to use for the transaction.</param>
    /// <param name="receiverAddress">The address of the receiver.</param>
    /// <param name="uri">The URI of the token metadata.</param>
    /// <returns>A task representing the asynchronous operation, with a ThirdwebTransactionReceipt result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract or wallet is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the receiver address or URI is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the token ID is less than 0.</exception>
    public static async Task<ThirdwebTransactionReceipt> TokenERC721_MintTo(this ThirdwebContract contract, IThirdwebWallet wallet, string receiverAddress, string uri)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        if (string.IsNullOrEmpty(receiverAddress))
        {
            throw new ArgumentException("Receiver address must be provided");
        }

        return uri == null ? throw new ArgumentException("URI must be provided") : await ThirdwebContract.Write(wallet, contract, "mintTo", 0, receiverAddress, uri);
    }

    /// <summary>
    /// Mint a specific ERC721 token to a receiver address with metadata.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="wallet">The wallet to use for the transaction.</param>
    /// <param name="receiverAddress">The address of the receiver.</param>
    /// <param name="metadata">The metadata of the token.</param>
    /// <returns>A task representing the asynchronous operation, with a ThirdwebTransactionReceipt result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract or wallet is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the receiver address is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the token ID is less than 0.</exception>
    public static async Task<ThirdwebTransactionReceipt> TokenERC721_MintTo(this ThirdwebContract contract, IThirdwebWallet wallet, string receiverAddress, NFTMetadata metadata)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        if (string.IsNullOrEmpty(receiverAddress))
        {
            throw new ArgumentException("Receiver address must be provided");
        }

        var json = JsonConvert.SerializeObject(metadata);

        var jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);

        var ipfsResult = await ThirdwebStorage.UploadRaw(contract.Client, jsonBytes);

        return await ThirdwebContract.Write(wallet, contract, "mintTo", 0, receiverAddress, $"ipfs://{ipfsResult.IpfsHash}");
    }

    /// <summary>
    /// Mint ERC721 tokens with a signature.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="wallet">The wallet to use for the transaction.</param>
    /// <param name="mintRequest">The mint request containing the minting details.</param>
    /// <param name="signature">The signature to authorize the minting.</param>
    /// <returns>A task representing the asynchronous operation, with a ThirdwebTransactionReceipt result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract, wallet, or mint request is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the signature is null or empty.</exception>
    public static async Task<ThirdwebTransactionReceipt> TokenERC721_MintWithSignature(this ThirdwebContract contract, IThirdwebWallet wallet, TokenERC721_MintRequest mintRequest, string signature)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        if (mintRequest == null)
        {
            throw new ArgumentNullException(nameof(mintRequest));
        }

        if (string.IsNullOrEmpty(signature))
        {
            throw new ArgumentException("Signature must be provided");
        }

        var isNativeToken = mintRequest.Currency == Constants.NATIVE_TOKEN_ADDRESS;

        var payableAmount = isNativeToken ? mintRequest.Price : BigInteger.Zero;

        return await ThirdwebContract.Write(wallet, contract, "mintWithSignature", payableAmount, mintRequest, signature.HexToBytes());
    }

    /// <summary>
    /// Generate a mint signature for ERC721 tokens.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="wallet">The wallet to use for generating the signature.</param>
    /// <param name="mintRequest">The mint request containing the minting details.</param>
    /// <param name="metadataOverride">Optional metadata override for the token.</param>
    /// <returns>A task representing the asynchronous operation, with a tuple containing the mint request and the generated signature.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract, wallet, or mint request is null.</exception>
    public static async Task<(TokenERC721_MintRequest, string)> TokenERC721_GenerateMintSignature(
        this ThirdwebContract contract,
        IThirdwebWallet wallet,
        TokenERC721_MintRequest mintRequest,
        NFTMetadata? metadataOverride = null
    )
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        if (mintRequest == null)
        {
            throw new ArgumentNullException(nameof(mintRequest));
        }

        var finalUri = mintRequest.Uri;

        if (finalUri == null) // allow empty string uri
        {
            if (metadataOverride != null)
            {
                var json = JsonConvert.SerializeObject(metadataOverride);

                var jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);

                var ipfsResult = await ThirdwebStorage.UploadRaw(contract.Client, jsonBytes);

                finalUri = $"ipfs://{ipfsResult.IpfsHash}";
            }
            else
            {
                throw new ArgumentException("MintRequest URI or NFTMetadata override must be provided");
            }
        }

        var defaultRoyaltyInfo = await contract.GetDefaultRoyaltyInfo();

        mintRequest = new TokenERC721_MintRequest
        {
            To = mintRequest.To ?? throw new ArgumentNullException(nameof(mintRequest.To)),
            RoyaltyRecipient = defaultRoyaltyInfo.Recipient,
            RoyaltyBps = defaultRoyaltyInfo.Bps,
            PrimarySaleRecipient = mintRequest.PrimarySaleRecipient ?? await contract.GetPrimarySaleRecipient(),
            Uri = finalUri,
            Price = mintRequest.Price,
            Currency = mintRequest.Currency ?? Constants.NATIVE_TOKEN_ADDRESS,
            ValidityStartTimestamp = mintRequest.ValidityStartTimestamp,
            ValidityEndTimestamp = mintRequest.ValidityEndTimestamp > 0 ? mintRequest.ValidityEndTimestamp : Utils.GetUnixTimeStampIn10Years(),
            Uid = mintRequest.Uid ?? Guid.NewGuid().ToByteArray().PadTo32Bytes()
        };

        var signature = await EIP712.GenerateSignature_TokenERC721(
            domainName: "TokenERC721",
            version: "1",
            chainId: contract.Chain,
            verifyingContract: contract.Address,
            mintRequest: mintRequest,
            signer: wallet
        );

        return (mintRequest, signature);
    }

    /// <summary>
    /// Verify a mint signature for ERC721 tokens.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="mintRequest">The mint request containing the minting details.</param>
    /// <param name="signature">The signature to verify.</param>
    /// <returns>A task representing the asynchronous operation, with a VerifyResult result containing the verification details.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract or mint request is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the signature is null or empty.</exception>
    public static async Task<VerifyResult> TokenERC721_VerifyMintSignature(this ThirdwebContract contract, TokenERC721_MintRequest mintRequest, string signature)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (mintRequest == null)
        {
            throw new ArgumentNullException(nameof(mintRequest));
        }

        return string.IsNullOrEmpty(signature)
            ? throw new ArgumentException("Signature must be provided")
            : await ThirdwebContract.Read<VerifyResult>(contract, "verify", mintRequest, signature.HexToBytes());
    }

    #endregion

    #region TokenERC1155

    /// <summary>
    /// Mint a specific quantity of ERC1155 tokens to a receiver address with a given URI.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="wallet">The wallet to use for the transaction.</param>
    /// <param name="receiverAddress">The address of the receiver.</param>
    /// <param name="tokenId">The ID of the token.</param>
    /// <param name="quantity">The quantity of tokens to mint.</param>
    /// <param name="uri">The URI of the token metadata.</param>
    /// <returns>A task representing the asynchronous operation, with a ThirdwebTransactionReceipt result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract, wallet, or URI is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the receiver address is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the token ID is less than 0 or the quantity is less than or equal to 0.</exception>
    public static async Task<ThirdwebTransactionReceipt> TokenERC1155_MintTo(
        this ThirdwebContract contract,
        IThirdwebWallet wallet,
        string receiverAddress,
        BigInteger tokenId,
        BigInteger quantity,
        string uri
    )
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        if (string.IsNullOrEmpty(receiverAddress))
        {
            throw new ArgumentException("Receiver address must be provided");
        }

        if (tokenId < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tokenId), "Token ID must be equal or greater than 0");
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than 0");
        }

        return uri == null ? throw new ArgumentNullException(nameof(uri)) : await ThirdwebContract.Write(wallet, contract, "mintTo", 0, receiverAddress, tokenId, uri, quantity);
    }

    /// <summary>
    /// Mint a specific quantity of ERC1155 tokens to a receiver address with metadata.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="wallet">The wallet to use for the transaction.</param>
    /// <param name="receiverAddress">The address of the receiver.</param>
    /// <param name="tokenId">The ID of the token.</param>
    /// <param name="quantity">The quantity of tokens to mint.</param>
    /// <param name="metadata">The metadata of the token.</param>
    /// <returns>A task representing the asynchronous operation, with a ThirdwebTransactionReceipt result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract or wallet is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the receiver address is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the token ID is less than 0 or the quantity is less than or equal to 0.</exception>
    public static async Task<ThirdwebTransactionReceipt> TokenERC1155_MintTo(
        this ThirdwebContract contract,
        IThirdwebWallet wallet,
        string receiverAddress,
        BigInteger tokenId,
        BigInteger quantity,
        NFTMetadata metadata
    )
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        if (string.IsNullOrEmpty(receiverAddress))
        {
            throw new ArgumentException("Receiver address must be provided");
        }

        if (tokenId < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tokenId), "Token ID must be equal or greater than 0");
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than 0");
        }

        var json = JsonConvert.SerializeObject(metadata);

        var jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);

        var ipfsResult = await ThirdwebStorage.UploadRaw(contract.Client, jsonBytes);

        return await ThirdwebContract.Write(wallet, contract, "mintTo", 0, receiverAddress, tokenId, $"ipfs://{ipfsResult.IpfsHash}", quantity);
    }

    /// <summary>
    /// Mint ERC1155 tokens with a signature.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="wallet">The wallet to use for the transaction.</param>
    /// <param name="mintRequest">The mint request containing the minting details.</param>
    /// <param name="signature">The signature to authorize the minting.</param>
    /// <returns>A task representing the asynchronous operation, with a ThirdwebTransactionReceipt result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract, wallet, or mint request is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the signature is null or empty.</exception>
    public static async Task<ThirdwebTransactionReceipt> TokenERC1155_MintWithSignature(this ThirdwebContract contract, IThirdwebWallet wallet, TokenERC1155_MintRequest mintRequest, string signature)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        if (mintRequest == null)
        {
            throw new ArgumentNullException(nameof(mintRequest));
        }

        if (string.IsNullOrEmpty(signature))
        {
            throw new ArgumentException("Signature must be provided");
        }

        var isNativeToken = mintRequest.Currency == Constants.NATIVE_TOKEN_ADDRESS;

        var payableAmount = isNativeToken ? mintRequest.Quantity * mintRequest.PricePerToken : BigInteger.Zero;

        return await ThirdwebContract.Write(wallet, contract, "mintWithSignature", payableAmount, mintRequest, signature.HexToBytes());
    }

    /// <summary>
    /// Generate a mint signature for ERC1155 tokens.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="wallet">The wallet to use for generating the signature.</param>
    /// <param name="mintRequest">The mint request containing the minting details.</param>
    /// <param name="metadataOverride">Optional metadata override for the token.</param>
    /// <returns>A task representing the asynchronous operation, with a tuple containing the mint request and the generated signature.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract, wallet, or mint request is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the MintRequest URI or NFTMetadata override is not provided.</exception>
    public static async Task<(TokenERC1155_MintRequest, string)> TokenERC1155_GenerateMintSignature(
        this ThirdwebContract contract,
        IThirdwebWallet wallet,
        TokenERC1155_MintRequest mintRequest,
        NFTMetadata? metadataOverride = null
    )
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        if (mintRequest == null)
        {
            throw new ArgumentNullException(nameof(mintRequest));
        }

        var finalUri = mintRequest.Uri;

        if (finalUri == null) // allow empty string uri
        {
            if (metadataOverride != null)
            {
                var json = JsonConvert.SerializeObject(metadataOverride);

                var jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);

                var ipfsResult = await ThirdwebStorage.UploadRaw(contract.Client, jsonBytes);

                finalUri = $"ipfs://{ipfsResult.IpfsHash}";
            }
            else
            {
                throw new ArgumentException("MintRequest URI or NFTMetadata override must be provided");
            }
        }

        var defaultRoyaltyInfo = await contract.GetDefaultRoyaltyInfo();

        mintRequest = new TokenERC1155_MintRequest
        {
            To = mintRequest.To ?? throw new ArgumentNullException(nameof(mintRequest.To)),
            RoyaltyRecipient = defaultRoyaltyInfo.Recipient,
            RoyaltyBps = defaultRoyaltyInfo.Bps,
            TokenId = mintRequest.TokenId ?? await contract.ERC1155_TotalSupply(),
            PrimarySaleRecipient = mintRequest.PrimarySaleRecipient ?? await contract.GetPrimarySaleRecipient(),
            Uri = finalUri,
            Quantity = mintRequest.Quantity > 0 ? mintRequest.Quantity : 1,
            PricePerToken = mintRequest.PricePerToken,
            Currency = mintRequest.Currency ?? Constants.NATIVE_TOKEN_ADDRESS,
            ValidityStartTimestamp = mintRequest.ValidityStartTimestamp,
            ValidityEndTimestamp = mintRequest.ValidityEndTimestamp > 0 ? mintRequest.ValidityEndTimestamp : Utils.GetUnixTimeStampIn10Years(),
            Uid = mintRequest.Uid ?? Guid.NewGuid().ToByteArray().PadTo32Bytes()
        };

        var signature = await EIP712.GenerateSignature_TokenERC1155(
            domainName: "TokenERC1155",
            version: "1",
            chainId: contract.Chain,
            verifyingContract: contract.Address,
            mintRequest: mintRequest,
            signer: wallet
        );

        return (mintRequest, signature);
    }

    /// <summary>
    /// Verify a mint signature for ERC1155 tokens.
    /// </summary>
    /// <param name="contract">The contract to interact with.</param>
    /// <param name="mintRequest">The mint request containing the minting details.</param>
    /// <param name="signature">The signature to verify.</param>
    /// <returns>A task representing the asynchronous operation, with a VerifyResult result containing the verification details.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contract or mint request is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the signature is null or empty.</exception>
    public static async Task<VerifyResult> TokenERC1155_VerifyMintSignature(this ThirdwebContract contract, TokenERC1155_MintRequest mintRequest, string signature)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (mintRequest == null)
        {
            throw new ArgumentNullException(nameof(mintRequest));
        }

        return string.IsNullOrEmpty(signature)
            ? throw new ArgumentException("Signature must be provided")
            : await ThirdwebContract.Read<VerifyResult>(contract, "verify", mintRequest, signature.HexToBytes());
    }

    #endregion
}
