using dotenv.net;

namespace Thirdweb.Tests;

public class BaseTests
{
    protected readonly ITestOutputHelper _output;
    protected readonly string? _secretKey;
    protected readonly string? _clientIdBundleIdOnly;
    protected readonly string? _bundleIdBundleIdOnly;
    protected readonly string? _testPrivateKey;

    public BaseTests(ITestOutputHelper output)
    {
        DotEnv.Load();
        _output = output;
        _secretKey = Environment.GetEnvironmentVariable("THIRDWEB_SECRET_KEY");
        _clientIdBundleIdOnly = Environment.GetEnvironmentVariable("THIRDWEB_CLIENT_ID_BUNDLE_ID_ONLY");
        _bundleIdBundleIdOnly = Environment.GetEnvironmentVariable("THIRDWEB_BUNDLE_ID_BUNDLE_ID_ONLY");
        _testPrivateKey = Environment.GetEnvironmentVariable("PRIVATE_KEY");
    }

    [Fact]
    public void DotEnvTest()
    {
        Assert.NotNull(_secretKey);
    }

    [Fact]
    public void TestOutput()
    {
        _output.WriteLine("This is a test output.");
        Console.WriteLine("This is a test output.");
    }
}
