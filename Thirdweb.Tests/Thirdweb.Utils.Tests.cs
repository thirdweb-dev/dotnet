using System.Numerics;

namespace Thirdweb.Tests;

public class UtilsTests : BaseTests
{
    public UtilsTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void ComputeClientIdFromSecretKey()
    {
        Assert.True(Utils.ComputeClientIdFromSecretKey(_secretKey).Length == 32);
    }

    [Fact]
    public void HexConcat()
    {
        var hexStrings = new string[] { "0x1234", "0x5678", "0x90AB" };
        Assert.Equal("0x1234567890AB", Utils.HexConcat(hexStrings));
    }

    [Fact]
    public void HashPrefixedMessage()
    {
        var messageStr = "Hello, World!";
        var message = System.Text.Encoding.UTF8.GetBytes(messageStr);
        var hashStr = Utils.HashPrefixedMessage(messageStr);
        var hash = Utils.HashPrefixedMessage(message);
        Assert.Equal(hashStr, Utils.BytesToHex(hash));
        Assert.Equal("0xc8ee0d506e864589b799a645ddb88b08f5d39e8049f9f702b3b61fa15e55fc73", hashStr);
    }

    [Fact]
    public void BytesToHex()
    {
        var bytes = new byte[] { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
        Assert.Equal("0x1234567890abcdef", Utils.BytesToHex(bytes));
    }

    [Fact]
    public void HexToBytes()
    {
        var hex = "0x1234567890abcdef";
        var bytes = Utils.HexToBytes(hex);
        Assert.Equal(new byte[] { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF }, bytes);
    }

    [Fact]
    public void StringToHex()
    {
        var str = "Hello, World!";
        var hex = Utils.StringToHex(str);
        Assert.Equal("0x48656c6c6f2c20576f726c6421", hex);
    }

    [Fact]
    public void HexToString()
    {
        var hex = "0x48656c6c6f2c20576f726c6421";
        var str = Utils.HexToString(hex);
        Assert.Equal("Hello, World!", str);
    }

    [Fact]
    public void GetUnixTimeStampNow()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var now2 = Utils.GetUnixTimeStampNow();
        Assert.Equal(now, now2);
    }

    [Fact]
    public void GetUnixTimeStampIn10Years()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var tenYears = 60 * 60 * 24 * 365 * 10;
        var tenYearsFromNow = now + tenYears;
        var tenYearsFromNow2 = Utils.GetUnixTimeStampIn10Years();
        Assert.Equal(tenYearsFromNow, tenYearsFromNow2);
    }

    [Fact]
    public void ReplaceIPFS()
    {
        var uri = "ipfs://QmXn1b6Q7";
        var gateway = "https://myawesomegateway.io/ipfs/";
        var replaced = Utils.ReplaceIPFS(uri, gateway);
        Assert.Equal("https://myawesomegateway.io/ipfs/QmXn1b6Q7", replaced);

        uri = "https://myawesomegateway.io/ipfs/QmXn1b6Q7";
        replaced = Utils.ReplaceIPFS(uri, gateway);
        Assert.Equal("https://myawesomegateway.io/ipfs/QmXn1b6Q7", replaced);

        uri = "ipfs://QmXn1b6Q7";
        gateway = null;
        replaced = Utils.ReplaceIPFS(uri, gateway);
        Assert.Equal("https://ipfs.io/ipfs/QmXn1b6Q7", replaced);
    }

    [Fact]
    public void ToWei_ConvertsCorrectly()
    {
        var eth = "1.5";
        var expectedWei = "1500000000000000000";
        Assert.Equal(expectedWei, Utils.ToWei(eth));
    }

    [Fact]
    public void ToWei_ThrowsOnInvalidInput()
    {
        var invalidEth = "abc";
        _ = Assert.Throws<ArgumentException>(() => Utils.ToWei(invalidEth));
    }

    [Fact]
    public void ToWei_ThrowsExceptionForInvalidInput()
    {
        var invalidEth = "invalid";
        _ = Assert.Throws<ArgumentException>(() => Utils.ToWei(invalidEth));
    }

    [Fact]
    public void ToWei_ConvertsNegativeValue()
    {
        var negativeEth = "-1.5";
        var expectedWei = new BigInteger(-1.5 * Math.Pow(10, 18)).ToString();
        Assert.Equal(expectedWei, Utils.ToWei(negativeEth));
    }

    [Fact]
    public void ToWei_ConvertsLargeFloat()
    {
        var largeEth = "1234567890.123456789";
        var expectedWei = new BigInteger(1234567890.123456789 * Math.Pow(10, 18)).ToString();
        Assert.Equal(expectedWei, Utils.ToWei(largeEth));
    }

    [Fact]
    public void ToEth_ConvertsCorrectly()
    {
        var wei = "1500000000000000000";
        var expectedEth = "1.5000";
        Assert.Equal(expectedEth, Utils.ToEth(wei));
    }

    [Fact]
    public void ToEth_WithCommas()
    {
        var wei = "1234500000000000000000";
        var expectedEth = "1,234.5000";
        Assert.Equal(expectedEth, Utils.ToEth(wei, 4, true));
    }

    [Fact]
    public void ToEth_ConvertsZeroWei()
    {
        var zeroWei = "0";
        Assert.Equal("0.0000", Utils.ToEth(zeroWei));
    }

    [Fact]
    public void ToEth_ConvertsSmallWei()
    {
        var smallWei = "1234";
        Assert.Equal("0.0000", Utils.ToEth(smallWei));
    }

    [Fact]
    public void FormatERC20_NoDecimalsNoCommas()
    {
        var wei = "1500000000000000000";
        var expectedEth = "2";
        Assert.Equal(expectedEth, Utils.FormatERC20(wei, 0));
    }

    [Fact]
    public void FormatERC20_LargeNumberWithCommas()
    {
        var wei = "1000000000000000000000000";
        var expectedEth = "1,000,000";
        Assert.Equal(expectedEth, Utils.FormatERC20(wei, 0, 18, true));
    }

    [Fact]
    public void FormatERC20_ConvertsZeroWei()
    {
        var zeroWei = "0";
        Assert.Equal("0", Utils.FormatERC20(zeroWei, 0));
    }

    [Fact]
    public void FormatERC20_SmallFractionalWei()
    {
        var fractionalWei = "10";
        Assert.Equal("0.0000", Utils.FormatERC20(fractionalWei, 4));
    }

    [Fact]
    public void FormatERC20_ThrowsOnInvalidWei()
    {
        var invalidWei = "not_a_number";
        Assert.Throws<ArgumentException>(() => Utils.FormatERC20(invalidWei, 4));
    }
}
