using dotenv.net;

namespace Thirdweb.Tests;

public class BaseTests
{
    protected ITestOutputHelper Output { get; }
    protected string? SecretKey { get; }
    protected string? ClientIdBundleIdOnly { get; }
    protected string? BundleIdBundleIdOnly { get; }

    public BaseTests(ITestOutputHelper output)
    {
        DotEnv.Load();
        this.Output = output;
        this.SecretKey = Environment.GetEnvironmentVariable("THIRDWEB_SECRET_KEY");
        this.ClientIdBundleIdOnly = Environment.GetEnvironmentVariable("THIRDWEB_CLIENT_ID_BUNDLE_ID_ONLY");
        this.BundleIdBundleIdOnly = Environment.GetEnvironmentVariable("THIRDWEB_BUNDLE_ID_BUNDLE_ID_ONLY");

        this.Output.WriteLine($"Started {this.GetType().FullName}");
    }

    [Fact(Timeout = 120000)]
    public void DotEnvTest()
    {
        Assert.NotNull(this.SecretKey);
    }
}
