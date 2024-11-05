using Thirdweb.EWS;

namespace Thirdweb;

/// <summary>
/// Represents an in-app wallet that supports email, phone, social, SIWE and custom authentication.
/// </summary>
public class InAppWallet : EcosystemWallet
{
    internal InAppWallet(
        ThirdwebClient client,
        EmbeddedWallet embeddedWallet,
        IThirdwebHttpClient httpClient,
        string email,
        string phoneNumber,
        string authProvider,
        IThirdwebWallet siweSigner,
        string address,
        string legacyEncryptionKey
    )
        : base(null, null, client, embeddedWallet, httpClient, email, phoneNumber, authProvider, siweSigner, legacyEncryptionKey)
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
    /// <returns>A task that represents the asynchronous operation. The task result contains the created in-app wallet.</returns>
    /// <exception cref="ArgumentException">Thrown when required parameters are not provided.</exception>
    public static async Task<InAppWallet> Create(
        ThirdwebClient client,
        string email = null,
        string phoneNumber = null,
        AuthProvider authProvider = Thirdweb.AuthProvider.Default,
        string storageDirectoryPath = null,
        IThirdwebWallet siweSigner = null,
        string legacyEncryptionKey = null
    )
    {
        storageDirectoryPath ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Thirdweb", "InAppWallet");
        var ecoWallet = await Create(client, null, null, email, phoneNumber, authProvider, storageDirectoryPath, siweSigner, legacyEncryptionKey);
        return new InAppWallet(
            ecoWallet.Client,
            ecoWallet.EmbeddedWallet,
            ecoWallet.HttpClient,
            ecoWallet.Email,
            ecoWallet.PhoneNumber,
            ecoWallet.AuthProvider,
            ecoWallet.SiweSigner,
            ecoWallet.Address,
            ecoWallet.LegacyEncryptionKey
        );
    }
}
