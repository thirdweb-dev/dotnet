using dotenv.net;

namespace Thirdweb.Tests;

public class BaseTests
{
    protected readonly ITestOutputHelper _output;
    protected readonly string? _secretKey;

    public BaseTests(ITestOutputHelper output)
    {
        DotEnv.Load();
        _output = output;
        _secretKey = Environment.GetEnvironmentVariable("THIRDWEB_SECRET_KEY");
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
