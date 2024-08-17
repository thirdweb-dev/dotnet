using System.Numerics;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Newtonsoft.Json;

namespace Thirdweb
{
    /// <summary>
    /// Represents the input parameters for a Thirdweb transaction.
    /// </summary>
    public class ThirdwebTransactionInput
    {
        public ThirdwebTransactionInput() { }

        public ThirdwebTransactionInput(
            string from = null,
            string to = null,
            BigInteger? nonce = null,
            BigInteger? gas = null,
            BigInteger? gasPrice = null,
            BigInteger? value = null,
            string data = null,
            BigInteger? chainId = null,
            BigInteger? maxFeePerGas = null,
            BigInteger? maxPriorityFeePerGas = null,
            ZkSyncOptions? zkSync = null
        )
        {
            From = string.IsNullOrEmpty(from) ? Constants.ADDRESS_ZERO : from;
            To = string.IsNullOrEmpty(to) ? Constants.ADDRESS_ZERO : to;
            Nonce = nonce == null ? null : new HexBigInteger(nonce.Value);
            Gas = gas == null ? null : new HexBigInteger(gas.Value);
            GasPrice = gasPrice == null ? null : new HexBigInteger(gasPrice.Value);
            Value = value == null ? null : new HexBigInteger(value.Value);
            Data = string.IsNullOrEmpty(data) ? "0x" : data;
            ChainId = chainId == null ? null : new HexBigInteger(chainId.Value);
            MaxFeePerGas = maxFeePerGas == null ? null : new HexBigInteger(maxFeePerGas.Value);
            MaxPriorityFeePerGas = maxPriorityFeePerGas == null ? null : new HexBigInteger(maxPriorityFeePerGas.Value);
            ZkSync = zkSync;
        }

        /// <summary>
        /// Gets or sets the nonce of the transaction.
        /// </summary>
        [JsonProperty(PropertyName = "nonce")]
        public HexBigInteger Nonce { get; set; }

        private string _from;
        private string _to;
        private string _data;

        /// <summary>
        /// Gets or sets the sender address of the transaction.
        /// </summary>
        [JsonProperty(PropertyName = "from")]
        internal string From
        {
            get => _from.EnsureHexPrefix();
            set => _from = value;
        }

        /// <summary>
        /// Gets or sets the recipient address of the transaction.
        /// </summary>
        [JsonProperty(PropertyName = "to")]
        public string To
        {
            get => _to.EnsureHexPrefix();
            set => _to = value;
        }

        /// <summary>
        /// Gets or sets the gas limit for the transaction.
        /// </summary>
        [JsonProperty(PropertyName = "gas")]
        public HexBigInteger Gas { get; set; }

        /// <summary>
        /// Gets or sets the gas price for the transaction.
        /// </summary>
        [JsonProperty(PropertyName = "gasPrice")]
        public HexBigInteger GasPrice { get; set; }

        /// <summary>
        /// Gets or sets the value to be transferred in the transaction.
        /// </summary>
        [JsonProperty(PropertyName = "value")]
        public HexBigInteger Value { get; set; }

        /// <summary>
        /// Gets or sets the data to be sent with the transaction.
        /// </summary>
        [JsonProperty(PropertyName = "data")]
        public string Data
        {
            get => _data.EnsureHexPrefix();
            set => _data = value;
        }

        /// <summary>
        /// Gets or sets the maximum fee per gas for the transaction.
        /// </summary>
        [JsonProperty(PropertyName = "maxFeePerGas")]
        public HexBigInteger MaxFeePerGas { get; set; }

        /// <summary>
        /// Gets or sets the maximum priority fee per gas for the transaction.
        /// </summary>
        [JsonProperty(PropertyName = "maxPriorityFeePerGas")]
        public HexBigInteger MaxPriorityFeePerGas { get; set; }

        /// <summary>
        /// Gets or sets the chain ID for the transaction.
        /// </summary>
        [JsonProperty(PropertyName = "chainId")]
        internal HexBigInteger ChainId { get; set; }

        /// <summary>
        /// Gets or sets the zkSync options for the transaction.
        /// </summary>
        [JsonProperty(PropertyName = "zkSyncOptions", NullValueHandling = NullValueHandling.Ignore)]
        public ZkSyncOptions? ZkSync { get; set; }
    }

    /// <summary>
    /// Represents the zkSync options for a transaction.
    /// </summary>
    public struct ZkSyncOptions
    {
        /// <summary>
        /// Gets or sets the gas limit per pubdata byte.
        /// </summary>
        [JsonProperty(PropertyName = "gasPerPubdataByteLimit")]
        public BigInteger? GasPerPubdataByteLimit { get; set; }

        /// <summary>
        /// Gets or sets the factory dependencies.
        /// </summary>
        [JsonProperty(PropertyName = "factoryDeps")]
        public List<byte[]> FactoryDeps { get; set; }

        /// <summary>
        /// Gets or sets the paymaster.
        /// </summary>
        [JsonProperty(PropertyName = "paymaster")]
        public BigInteger Paymaster { get; set; }

        /// <summary>
        /// Gets or sets the paymaster input data.
        /// </summary>
        [JsonProperty(PropertyName = "paymasterInput")]
        public byte[] PaymasterInput { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZkSyncOptions"/> struct.
        /// </summary>
        /// <param name="paymaster">The paymaster.</param>
        /// <param name="paymasterInput">The paymaster input data.</param>
        /// <param name="gasPerPubdataByteLimit">The gas limit per pubdata byte.</param>
        /// <param name="factoryDeps">The factory dependencies.</param>
        public ZkSyncOptions(string paymaster, string paymasterInput, BigInteger? gasPerPubdataByteLimit = null, List<byte[]> factoryDeps = null)
        {
            if (string.IsNullOrEmpty(paymaster) || string.IsNullOrEmpty(paymasterInput))
            {
                Paymaster = 0;
                PaymasterInput = null;
            }
            else
            {
                Paymaster = new HexBigInteger(paymaster).Value;
                PaymasterInput = paymasterInput.HexToByteArray();
                GasPerPubdataByteLimit = gasPerPubdataByteLimit;
                FactoryDeps = factoryDeps ?? new List<byte[]>();
            }
        }
    }
}
