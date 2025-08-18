using System.Numerics;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.ABI.Model;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Newtonsoft.Json;

namespace Thirdweb;

/// <summary>
/// Represents a Thirdweb contract.
/// </summary>
public class ThirdwebContract
{
    public ThirdwebClient Client { get; private set; }
    public string Address { get; private set; }
    public BigInteger Chain { get; private set; }
    public string Abi { get; private set; }

    private static readonly Dictionary<string, string> _contractAbiCache = new();
    private static readonly object _cacheLock = new();

    private ThirdwebContract(ThirdwebClient client, string address, BigInteger chain, string abi)
    {
        this.Client = client;
        this.Address = address;
        this.Chain = chain;
        this.Abi = abi;
    }

    /// <summary>
    /// Deploys a new contract on the specified chain.
    /// </summary>
    /// <param name="client">The Thirdweb client.</param>
    /// <param name="chainId">The chain ID.</param>
    /// <param name="serverWalletAddress">The server wallet address.</param>
    /// <param name="bytecode">The bytecode of the contract.</param>
    /// <param name="abi">The ABI of the contract.</param>
    /// <param name="constructorParams">The constructor parameters (optional).</param>
    /// <param name="salt">The salt for the contract deployment (optional).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The contract address of the fully deployed contract.</returns>
    /// <remarks>
    /// This method deploys a new contract using a server wallet, create one via the ServerWallet class or api.thirdweb.com, or the dashboard.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if any of the required parameters are null.</exception>
    /// <exception cref="ArgumentException">Thrown if any of the required parameters are invalid.</exception>
    /// <exception cref="OverflowException">Thrown if the chain ID is too large.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the deployment fails or the API response is invalid.</exception>
    /// <exception cref="JsonException">Thrown if the ABI JSON cannot be parsed.</exception>
    public static async Task<string> Deploy(
        ThirdwebClient client,
        BigInteger chainId,
        string serverWalletAddress,
        string bytecode,
        string abi,
        Dictionary<string, object> constructorParams = null,
        string salt = null,
        CancellationToken cancellationToken = default
    )
    {
        // Input validation
        if (client == null)
        {
            throw new ArgumentNullException(nameof(client));
        }

        if (chainId <= 0)
        {
            throw new ArgumentException("Chain ID must be greater than 0.", nameof(chainId));
        }

        if (string.IsNullOrEmpty(serverWalletAddress))
        {
            throw new ArgumentException("Server wallet address cannot be null or empty.", nameof(serverWalletAddress));
        }

        if (string.IsNullOrEmpty(bytecode))
        {
            throw new ArgumentException("Bytecode cannot be null or empty.", nameof(bytecode));
        }

        if (string.IsNullOrEmpty(abi))
        {
            throw new ArgumentException("ABI cannot be null or empty.", nameof(abi));
        }

        // Perform checked cast to int
        // TODO: Remove this when generated client supports BigInteger for chainId
        int chainIdInt;
        try
        {
            chainIdInt = checked((int)chainId);
        }
        catch (OverflowException)
        {
            throw new OverflowException($"Chain ID {chainId} is too large; ThirdwebContract.Deploy currently only supports chain IDs up to {int.MaxValue}.");
        }

        // Parse ABI with error handling
        List<object> parsedAbi;
        try
        {
            parsedAbi = JsonConvert.DeserializeObject<List<object>>(abi);
            if (parsedAbi == null)
            {
                throw new InvalidOperationException("ABI deserialization returned null.");
            }
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Failed to parse ABI JSON: {ex.Message}", nameof(abi), ex);
        }

        var response =
            await client
                .Api.DeployContractAsync(
                    new Api.Body8()
                    {
                        ChainId = chainIdInt,
                        From = serverWalletAddress,
                        Bytecode = bytecode,
                        Abi = parsedAbi,
                        ConstructorParams = constructorParams,
                        Salt = salt,
                    },
                    cancellationToken
                )
                .ConfigureAwait(false) ?? throw new InvalidOperationException("Failed to deploy contract: API response was null.");

        if (response.Result is null)
        {
            throw new InvalidOperationException("Failed to deploy contract: API response result was null.");
        }

        var contractAddress = response.Result.Address;
        if (string.IsNullOrEmpty(contractAddress))
        {
            throw new InvalidOperationException("Failed to deploy contract: Could not compute contract address.");
        }

        var transactionId = response.Result.TransactionId;
        if (string.IsNullOrEmpty(transactionId))
        {
            throw new InvalidOperationException("Failed to deploy contract: Transaction ID is empty.");
        }

        var hash = await ThirdwebTransaction.WaitForTransactionHash(client, transactionId, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrEmpty(hash))
        {
            throw new InvalidOperationException("Failed to deploy contract: Transaction hash is empty.");
        }

        _ = await ThirdwebTransaction.WaitForTransactionReceipt(client, chainId, hash, cancellationToken).ConfigureAwait(false);
        return contractAddress;
    }

    /// <summary>
    /// Creates a new instance of <see cref="ThirdwebContract"/>.
    /// </summary>
    /// <param name="client">The Thirdweb client.</param>
    /// <param name="address">The contract address.</param>
    /// <param name="chain">The chain ID.</param>
    /// <param name="abi">The contract ABI (optional).</param>
    /// <returns>A new instance of <see cref="ThirdwebContract"/>.</returns>
    /// <exception cref="ArgumentException">Thrown if any of the required parameters are missing.</exception>
    public static async Task<ThirdwebContract> Create(ThirdwebClient client, string address, BigInteger chain, string abi = null)
    {
        if (client == null)
        {
            throw new ArgumentException("Client must be provided");
        }

        if (string.IsNullOrEmpty(address))
        {
            throw new ArgumentException("Address must be provided");
        }

        if (chain == 0)
        {
            throw new ArgumentException("Chain must be provided");
        }

        abi ??= await FetchAbi(client, address, chain).ConfigureAwait(false);
        return new ThirdwebContract(client, address, chain, abi);
    }

    /// <summary>
    /// Fetches the ABI for the specified contract.
    /// </summary>
    /// <param name="client">The Thirdweb client.</param>
    /// <param name="address">The contract address.</param>
    /// <param name="chainId">The chain ID.</param>
    /// <returns>The contract ABI.</returns>
    public static async Task<string> FetchAbi(ThirdwebClient client, string address, BigInteger chainId)
    {
        var cacheKey = $"{address}:{chainId}";

        lock (_cacheLock)
        {
            if (_contractAbiCache.TryGetValue(cacheKey, out var cachedAbi))
            {
                return cachedAbi;
            }
        }

        var url = $"https://contract.thirdweb.com/abi/{chainId}/{address}";
        var httpClient = client.HttpClient;
        var response = await httpClient.GetAsync(url).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();
        var abi = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        lock (_cacheLock)
        {
            _contractAbiCache[cacheKey] = abi;
        }

        return abi;
    }

    /// <summary>
    /// Reads data from the contract using the specified method.
    /// </summary>
    /// <typeparam name="T">The type of the return value.</typeparam>
    /// <param name="contract">The contract instance.</param>
    /// <param name="method">The method to call.</param>
    /// <param name="parameters">The parameters for the method.</param>
    /// <returns>The result of the method call.</returns>
    public static async Task<T> Read<T>(ThirdwebContract contract, string method, params object[] parameters)
    {
        var rpc = ThirdwebRPC.GetRpcInstance(contract.Client, contract.Chain);
        (var data, var function) = EncodeFunctionCall(contract, method, parameters);
        var resultData = await rpc.SendRequestAsync<string>("eth_call", new { to = contract.Address, data }, "latest").ConfigureAwait(false);

        if ((typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>)) || typeof(T).IsArray)
        {
            var contractRaw = new Contract(null, contract.Abi, contract.Address);
            var functionAbi = contractRaw.ContractBuilder.ContractABI.FindFunctionABIFromInputData(data);
            var decoder = new FunctionCallDecoder();
            var outputList = new FunctionCallDecoder().DecodeDefaultData(resultData.HexToBytes(), functionAbi.OutputParameters);
            var resultList = outputList.Select(x => x.Result).ToList();

            if (typeof(T) == typeof(List<object>))
            {
                return (T)(object)resultList;
            }

            if (typeof(T) == typeof(object[]))
            {
                return (T)(object)resultList.ToArray();
            }

            try
            {
                var json = JsonConvert.SerializeObject(resultList);
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception)
            {
                var dict = outputList.ConvertToObjectDictionary();
                var ser = JsonConvert.SerializeObject(dict.First().Value);
                return JsonConvert.DeserializeObject<T>(ser);
            }
        }
        else
        {
            return function.DecodeTypeOutput<T>(resultData);
        }
    }

    /// <summary>
    /// Prepares a transaction for the specified method and parameters.
    /// </summary>
    /// <param name="wallet">The wallet instance.</param>
    /// <param name="contract">The contract instance.</param>
    /// <param name="method">The method to call.</param>
    /// <param name="weiValue">The value in wei to send.</param>
    /// <param name="parameters">The parameters for the method.</param>
    /// <returns>A prepared transaction.</returns>
    public static async Task<ThirdwebTransaction> Prepare(IThirdwebWallet wallet, ThirdwebContract contract, string method, BigInteger weiValue, params object[] parameters)
    {
        var data = contract.CreateCallData(method, parameters);
        var transaction = new ThirdwebTransactionInput(chainId: contract.Chain)
        {
            To = contract.Address,
            Data = data,
            Value = new HexBigInteger(weiValue),
        };

        return await ThirdwebTransaction.Create(wallet, transaction).ConfigureAwait(false);
    }

    /// <summary>
    /// Writes data to the contract using the specified method and parameters.
    /// </summary>
    /// <param name="wallet">The wallet instance.</param>
    /// <param name="contract">The contract instance.</param>
    /// <param name="method">The method to call.</param>
    /// <param name="weiValue">The value in wei to send.</param>
    /// <param name="parameters">The parameters for the method.</param>
    /// <returns>A transaction receipt.</returns>
    public static async Task<ThirdwebTransactionReceipt> Write(IThirdwebWallet wallet, ThirdwebContract contract, string method, BigInteger weiValue, params object[] parameters)
    {
        var thirdwebTx = await Prepare(wallet, contract, method, weiValue, parameters).ConfigureAwait(false);
        return await ThirdwebTransaction.SendAndWaitForTransactionReceipt(thirdwebTx).ConfigureAwait(false);
    }

    internal static (string callData, Function function) EncodeFunctionCall(ThirdwebContract contract, string method, params object[] parameters)
    {
        var contractRaw = new Contract(null, contract.Abi, contract.Address);
        var function = GetFunctionMatchSignature(contractRaw, method, parameters);
        return (function.GetData(parameters), function);
    }

    /// <summary>
    /// Gets a function matching the specified signature from the contract.
    /// </summary>
    /// <param name="contract">The contract instance.</param>
    /// <param name="functionName">The name of the function.</param>
    /// <param name="args">The arguments for the function.</param>
    /// <returns>The matching function, or null if no match is found.</returns>
    private static Function GetFunctionMatchSignature(Contract contract, string functionName, params object[] args)
    {
        if (functionName.StartsWith("0x"))
        {
            return contract.GetFunctionBySignature(functionName);
        }

        var abi = contract.ContractBuilder.ContractABI;
        var functions = abi.Functions;
        var paramsCount = args?.Length ?? 0;
        foreach (var function in functions)
        {
            if (function.Name == functionName && function.InputParameters.Length == paramsCount)
            {
                var sha = function.Sha3Signature;
                return contract.GetFunctionBySignature(sha);
            }
        }

        if (functionName.Contains('('))
        {
            var canonicalSignature = ExtractCanonicalSignature(functionName);
            var selector = Utils.HashMessage(canonicalSignature)[..8];
            return contract.GetFunctionBySignature(selector);
        }
        else
        {
            throw new ArgumentException("Method signature not found in contract ABI.");
        }
    }

    /// <summary>
    /// Extracts the canonical signature from the specified method.
    /// </summary>
    /// <param name="method">The method to extract the signature from.</param>
    /// <returns>The canonical signature.</returns>
    /// <exception cref="ArgumentException"></exception>
    private static string ExtractCanonicalSignature(string method)
    {
        method = method.Split("returns")[0];
        var startOfParameters = method.IndexOf('(');
        if (startOfParameters == -1)
        {
            throw new ArgumentException("Invalid function signature: Missing opening parenthesis.");
        }

        var endOfParameters = method.LastIndexOf(')');
        if (endOfParameters == -1)
        {
            throw new ArgumentException("Invalid function signature: Missing closing parenthesis.");
        }

        var functionName = method[..startOfParameters].Trim().Split(' ').Last(); // Get the last part after any spaces (in case of "function name(...)")
        var parameters = method.Substring(startOfParameters + 1, endOfParameters - startOfParameters - 1);

        var paramTypes = parameters.Split(',').Select(param => param.Trim().Split(' ')[0]).ToArray();

        var canonicalSignature = $"{functionName}({string.Join(",", paramTypes)})";
        return canonicalSignature;
    }
}
