using System.Numerics;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;

namespace Thirdweb
{
    /// <summary>
    /// Represents a Thirdweb contract.
    /// </summary>
    public class ThirdwebContract
    {
        internal ThirdwebClient Client { get; private set; }
        internal string Address { get; private set; }
        internal BigInteger Chain { get; private set; }
        internal string Abi { get; private set; }

        private static Dictionary<string, string> _contractAbiCache = new();
        private static readonly object _cacheLock = new object();

        private ThirdwebContract(ThirdwebClient client, string address, BigInteger chain, string abi)
        {
            Client = client;
            Address = address;
            Chain = chain;
            Abi = abi;
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
            var contractRaw = new Contract(null, contract.Abi, contract.Address);

            var function = GetFunctionMatchSignature(contractRaw, method, parameters);
            if (function == null)
            {
                if (method.Contains("("))
                {
                    try
                    {
                        var canonicalSignature = ExtractCanonicalSignature(method);
                        var selector = Nethereum.Util.Sha3Keccack.Current.CalculateHash(canonicalSignature)[..8];
                        function = contractRaw.GetFunctionBySignature(selector);
                    }
                    catch
                    {
                        function = contractRaw.GetFunction(method);
                    }
                }
            }

            if (function == null)
            {
                throw new ArgumentException($"Function '{method}' not found in the contract ABI.");
            }

            var data = function.GetData(parameters);
            var resultData = await rpc.SendRequestAsync<string>("eth_call", new { to = contract.Address, data = data }, "latest").ConfigureAwait(false);

            return function.DecodeTypeOutput<T>(resultData);
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
            var contractRaw = new Contract(null, contract.Abi, contract.Address);
            var function = GetFunctionMatchSignature(contractRaw, method, parameters);
            if (function == null)
            {
                if (method.Contains("("))
                {
                    try
                    {
                        var canonicalSignature = ExtractCanonicalSignature(method);
                        var selector = Nethereum.Util.Sha3Keccack.Current.CalculateHash(canonicalSignature)[..8];
                        function = contractRaw.GetFunctionBySignature(selector);
                    }
                    catch
                    {
                        function = contractRaw.GetFunction(method);
                    }
                }
            }
            var data = function.GetData(parameters);
            var transaction = new ThirdwebTransactionInput
            {
                From = await wallet.GetAddress().ConfigureAwait(false),
                To = contract.Address,
                Data = data,
                Value = new HexBigInteger(weiValue),
            };

            return await ThirdwebTransaction.Create(wallet, transaction, contract.Chain).ConfigureAwait(false);
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

        /// <summary>
        /// Gets a function matching the specified signature from the contract.
        /// </summary>
        /// <param name="contract">The contract instance.</param>
        /// <param name="functionName">The name of the function.</param>
        /// <param name="args">The arguments for the function.</param>
        /// <returns>The matching function, or null if no match is found.</returns>
        private static Function GetFunctionMatchSignature(Contract contract, string functionName, params object[] args)
        {
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
            return null;
        }

        /// <summary>
        /// Extracts the canonical signature from the specified method.
        /// </summary>
        /// <param name="method">The method to extract the signature from.</param>
        /// <returns>The canonical signature.</returns>
        /// <exception cref="ArgumentException"></exception>
        private static string ExtractCanonicalSignature(string method)
        {
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

            var functionName = method.Substring(0, startOfParameters).Trim().Split(' ').Last(); // Get the last part after any spaces (in case of "function name(...)")
            var parameters = method.Substring(startOfParameters + 1, endOfParameters - startOfParameters - 1);

            var paramTypes = parameters.Split(',').Select(param => param.Trim().Split(' ')[0]).ToArray();

            var canonicalSignature = $"{functionName}({string.Join(",", paramTypes)})";
            return canonicalSignature;
        }
    }
}
