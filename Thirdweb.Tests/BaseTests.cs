using dotenv.net;

namespace Thirdweb.Tests;

public class BaseTests
{
    protected ITestOutputHelper Output { get; }
    protected string? SecretKey { get; }
    protected string? ClientIdBundleIdOnly { get; }
    protected string? BundleIdBundleIdOnly { get; }

    protected ThirdwebClient Client { get; }

    public BaseTests(ITestOutputHelper output)
    {
        DotEnv.Load();
        this.Output = output;
        this.SecretKey = Environment.GetEnvironmentVariable("THIRDWEB_SECRET_KEY");
        this.ClientIdBundleIdOnly = Environment.GetEnvironmentVariable("THIRDWEB_CLIENT_ID_BUNDLE_ID_ONLY");
        this.BundleIdBundleIdOnly = Environment.GetEnvironmentVariable("THIRDWEB_BUNDLE_ID_BUNDLE_ID_ONLY");

        this.Client = ThirdwebClient.Create(secretKey: this.SecretKey);

        this.Output.WriteLine($"Started {this.GetType().FullName}");
    }

    [Fact(Timeout = 120000)]
    public void DotEnvTest()
    {
        Assert.NotNull(this.SecretKey);
    }

    public async Task<IThirdwebWallet> GetGuestAccount()
    {
        var iaw = await InAppWallet.Create(this.Client, authProvider: AuthProvider.Guest);
        if (await iaw.IsConnected())
        {
            await iaw.Disconnect();
        }
        _ = iaw.LoginWithGuest(defaultSessionIdOverride: Guid.NewGuid().ToString());
        return iaw;
    }

    public async Task<SmartWallet> GetSmartAccount(int chainId = 421614)
    {
        var guestAccount = await this.GetGuestAccount();
        var smartAccount = await SmartWallet.Create(personalWallet: guestAccount, chainId: chainId);
        return smartAccount;
    }
}
