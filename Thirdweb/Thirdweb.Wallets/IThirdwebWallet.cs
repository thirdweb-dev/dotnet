using Nethereum.ABI.EIP712;
using Newtonsoft.Json;

namespace Thirdweb;

/// <summary>
/// Interface for a Thirdweb wallet.
/// </summary>
public interface IThirdwebWallet
{
    /// <summary>
    /// Gets the Thirdweb client associated with the wallet.
    /// </summary>
    ThirdwebClient Client { get; }

    /// <summary>
    /// Gets the account type of the wallet.
    /// </summary>
    ThirdwebAccountType AccountType { get; }

    /// <summary>
    /// Gets the address of the wallet.
    /// </summary>
    /// <returns>The wallet address.</returns>
    Task<string> GetAddress();

    /// <summary>
    /// Signs a raw message using Ethereum's signing method.
    /// </summary>
    /// <param name="rawMessage">The raw message to sign.</param>
    /// <returns>The signed message.</returns>
    Task<string> EthSign(byte[] rawMessage);

    /// <summary>
    /// Signs a message using Ethereum's signing method.
    /// </summary>
    /// <param name="message">The message to sign.</param>
    /// <returns>The signed message.</returns>
    Task<string> EthSign(string message);

    /// <summary>
    /// Recovers the address from a signed message using Ethereum's signing method.
    /// </summary>
    /// <param name="message">The UTF-8 encoded message.</param>
    /// <param name="signature">The signature.</param>
    /// <returns>The recovered address.</returns>
    Task<string> RecoverAddressFromEthSign(string message, string signature);

    /// <summary>
    /// Signs a raw message using personal signing.
    /// </summary>
    /// <param name="rawMessage">The raw message to sign.</param>
    /// <returns>The signed message.</returns>
    Task<string> PersonalSign(byte[] rawMessage);

    /// <summary>
    /// Signs a message using personal signing.
    /// </summary>
    /// <param name="message">The message to sign.</param>
    /// <returns>The signed message.</returns>
    Task<string> PersonalSign(string message);

    /// <summary>
    /// Recovers the address from a signed message using personal signing.
    /// </summary>
    /// <param name="message">The UTF-8 encoded and prefixed message.</param>
    /// <param name="signature">The signature.</param>
    /// <returns>The recovered address.</returns>
    Task<string> RecoverAddressFromPersonalSign(string message, string signature);

    /// <summary>
    /// Signs typed data (version 4).
    /// </summary>
    /// <param name="json">The JSON representation of the typed data.</param>
    /// <returns>The signed data.</returns>
    Task<string> SignTypedDataV4(string json);

    /// <summary>
    /// Signs typed data (version 4).
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <typeparam name="TDomain">The type of the domain.</typeparam>
    /// <param name="data">The data to sign.</param>
    /// <param name="typedData">The typed data.</param>
    /// <returns>The signed data.</returns>
    Task<string> SignTypedDataV4<T, TDomain>(T data, TypedData<TDomain> typedData)
        where TDomain : IDomain;

    /// <summary>
    /// Recovers the address from a signed message using typed data (version 4).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TDomain"></typeparam>
    /// <param name="data">The data to sign.</param>
    /// <param name="typedData">The typed data.</param>
    /// <param name="signature">The signature.</param>
    /// <returns>The recovered address.</returns>
    Task<string> RecoverAddressFromTypedDataV4<T, TDomain>(T data, TypedData<TDomain> typedData, string signature)
        where TDomain : IDomain;

    /// <summary>
    /// Checks if the wallet is connected.
    /// </summary>
    /// <returns>True if connected, otherwise false.</returns>
    Task<bool> IsConnected();

    /// <summary>
    /// Signs a transaction.
    /// </summary>
    /// <param name="transaction">The transaction to sign.</param>
    /// <returns>The signed transaction.</returns>
    Task<string> SignTransaction(ThirdwebTransactionInput transaction);

    /// <summary>
    /// Sends a transaction.
    /// </summary>
    /// <param name="transaction">The transaction to send.</param>
    /// <returns>The transaction hash.</returns>
    Task<string> SendTransaction(ThirdwebTransactionInput transaction);

    /// <summary>
    /// Sends a transaction and waits for its receipt.
    /// </summary>
    /// <param name="transaction">The transaction to execute.</param>
    /// <returns>The transaction receipt.</returns>
    Task<ThirdwebTransactionReceipt> ExecuteTransaction(ThirdwebTransactionInput transaction);

    /// <summary>
    /// Disconnects the wallet (if using InAppWallet, clears session)
    /// </summary>
    Task Disconnect();
}

/// <summary>
/// Enum for the types of Thirdweb accounts.
/// </summary>
public enum ThirdwebAccountType
{
    PrivateKeyAccount,
    SmartAccount,
    ExternalAccount
}

/// <summary>
/// Represents a login payload.
/// </summary>
[Serializable]
public struct LoginPayload
{
    public LoginPayloadData Payload { get; set; }
    public string Signature { get; set; }
}

/// <summary>
/// Represents login payload data.
/// </summary>
[Serializable]
public class LoginPayloadData
{
    /// <summary>
    /// Gets or sets the domain of the login payload.
    /// </summary>
    [JsonProperty("domain")]
    public string Domain { get; set; }

    /// <summary>
    /// Gets or sets the address of the login payload.
    /// </summary>
    [JsonProperty("address")]
    public string Address { get; set; }

    /// <summary>
    /// Gets or sets the statement of the login payload.
    /// </summary>
    [JsonProperty("statement")]
    public string Statement { get; set; }

    /// <summary>
    /// Gets or sets the URI of the login payload.
    /// </summary>
    [JsonProperty("uri", NullValueHandling = NullValueHandling.Ignore)]
    public string Uri { get; set; }

    /// <summary>
    /// Gets or sets the version of the login payload.
    /// </summary>
    [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
    public string Version { get; set; }

    /// <summary>
    /// Gets or sets the chain ID of the login payload.
    /// </summary>
    [JsonProperty("chain_id", NullValueHandling = NullValueHandling.Ignore)]
    public string ChainId { get; set; }

    /// <summary>
    /// Gets or sets the nonce of the login payload.
    /// </summary>
    [JsonProperty("nonce", NullValueHandling = NullValueHandling.Ignore)]
    public string Nonce { get; set; }

    /// <summary>
    /// Gets or sets the issued at timestamp of the login payload.
    /// </summary>
    [JsonProperty("issued_at", NullValueHandling = NullValueHandling.Ignore)]
    public string IssuedAt { get; set; }

    /// <summary>
    /// Gets or sets the expiration time of the login payload.
    /// </summary>
    [JsonProperty("expiration_time", NullValueHandling = NullValueHandling.Ignore)]
    public string ExpirationTime { get; set; }

    /// <summary>
    /// Gets or sets the invalid before timestamp of the login payload.
    /// </summary>
    [JsonProperty("invalid_before", NullValueHandling = NullValueHandling.Ignore)]
    public string InvalidBefore { get; set; }

    /// <summary>
    /// Gets or sets the resources of the login payload.
    /// </summary>
    [JsonProperty("resources", NullValueHandling = NullValueHandling.Ignore)]
    public List<string> Resources { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginPayloadData"/> class.
    /// </summary>
    public LoginPayloadData() { }
}
