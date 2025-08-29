using Thirdweb.EWS;

namespace Thirdweb;

/// <summary>
/// Represents an in-app wallet that supports email, phone, social, SIWE and custom authentication.
/// </summary>
public class InAppWallet : EcosystemWallet
{
    public override string WalletId => "inApp";

    internal InAppWallet(
        ThirdwebClient client,
        EmbeddedWallet embeddedWallet,
        IThirdwebHttpClient httpClient,
        string email,
        string phoneNumber,
        string authProvider,
        IThirdwebWallet siweSigner,
        string address,
        string legacyEncryptionKey,
        string walletSecret,
        ExecutionMode executionMode,
        string delegationContractAddress
    )
        : base(null, null, client, embeddedWallet, httpClient, email, phoneNumber, authProvider, siweSigner, legacyEncryptionKey, walletSecret, executionMode, delegationContractAddress)
    {
        this.Address = address;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="InAppWallet"/> class.
    /// </summary>
    /// <param name="client">The Thirdweb client instance.</param>
    /// <param name="email">The email address for Email OTP authentication.</param>
    /// <param name="phoneNumber">The phone number for Phone OTP authentication.</param>
    /// <param name="authProvider">The authentication provider to use.</param>
    /// <param name="storageDirectoryPath">The path to the storage directory.</param>
    /// <param name="siweSigner">The SIWE signer wallet for SIWE authentication.</param>
    /// <param name="legacyEncryptionKey">The encryption key that is no longer required but was used in the past. Only pass this if you had used custom auth before this was deprecated.</param>
    /// <param name="walletSecret">The wallet secret for backend authentication.</param>
    /// <param name="twAuthTokenOverride">The auth token to use for the session. This will automatically connect using a raw thirdweb auth token.</param>
    /// <param name="executionMode">The execution mode for the wallet. EOA represents traditional direct calls, EIP7702 represents upgraded account self sponsored calls, and EIP7702Sponsored represents upgraded account calls with managed/sponsored execution.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created in-app wallet.</returns>
    /// <exception cref="ArgumentException">Thrown when required parameters are not provided.</exception>
    public static async Task<InAppWallet> Create(
        ThirdwebClient client,
        string email = null,
        string phoneNumber = null,
        AuthProvider authProvider = Thirdweb.AuthProvider.Default,
        string storageDirectoryPath = null,
        IThirdwebWallet siweSigner = null,
        string legacyEncryptionKey = null,
        string walletSecret = null,
        string twAuthTokenOverride = null,
        ExecutionMode executionMode = ExecutionMode.EOA
    )
    {
        storageDirectoryPath ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Thirdweb", "InAppWallet");
        var ecoWallet = await Create(client, null, null, email, phoneNumber, authProvider, storageDirectoryPath, siweSigner, legacyEncryptionKey, walletSecret, twAuthTokenOverride, executionMode);
        return new InAppWallet(
            ecoWallet.Client,
            ecoWallet.EmbeddedWallet,
            ecoWallet.HttpClient,
            ecoWallet.Email,
            ecoWallet.PhoneNumber,
            ecoWallet.AuthProvider,
            ecoWallet.SiweSigner,
            ecoWallet.Address,
            ecoWallet.LegacyEncryptionKey,
            ecoWallet.WalletSecret,
            ecoWallet.ExecutionMode,
            ecoWallet.DelegationContractAddress
        );
    }
}
