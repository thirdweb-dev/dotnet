using System.Numerics;
using Newtonsoft.Json.Linq;

namespace Thirdweb.Tests.Utilities;

public class UtilsTests : BaseTests
{
    public UtilsTests(ITestOutputHelper output)
        : base(output) { }

    [Fact(Timeout = 120000)]
    public void ComputeClientIdFromSecretKey()
    {
        Assert.True(Utils.ComputeClientIdFromSecretKey(this.SecretKey).Length == 32);
    }

    [Fact(Timeout = 120000)]
    public void HexConcat()
    {
        var hexStrings = new string[] { "0x1234", "0x5678", "0x90AB" };
        Assert.Equal("0x1234567890AB", Utils.HexConcat(hexStrings));
    }

    [Fact(Timeout = 120000)]
    public void HashPrefixedMessage()
    {
        var messageStr = "Hello, World!";
        var message = System.Text.Encoding.UTF8.GetBytes(messageStr);
        var hashStr = Utils.HashPrefixedMessage(messageStr);
        var hash = Utils.HashPrefixedMessage(message);
        Assert.Equal(hashStr, Utils.BytesToHex(hash));
        Assert.Equal("0xc8ee0d506e864589b799a645ddb88b08f5d39e8049f9f702b3b61fa15e55fc73", hashStr);
    }

    [Fact(Timeout = 120000)]
    public void HashMessage()
    {
        var messageStr = "Hello, World!";
        var hashStr = "0x" + Utils.HashMessage(messageStr);

        var message = System.Text.Encoding.UTF8.GetBytes(messageStr);
        var hash = Utils.HashMessage(message);

        Assert.Equal(hashStr, Utils.BytesToHex(hash));
        Assert.Equal("0xacaf3289d7b601cbd114fb36c4d29c85bbfd5e133f14cb355c3fd8d99367964f", hashStr);
    }

    [Fact(Timeout = 120000)]
    public void BytesToHex()
    {
        var bytes = new byte[] { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
        Assert.Equal("0x1234567890abcdef", Utils.BytesToHex(bytes));
    }

    [Fact(Timeout = 120000)]
    public void HexToBytes()
    {
        var hex = "0x1234567890abcdef";
        var bytes = Utils.HexToBytes(hex);
        Assert.Equal(new byte[] { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF }, bytes);
    }

    [Fact(Timeout = 120000)]
    public void StringToHex()
    {
        var str = "Hello, World!";
        var hex = Utils.StringToHex(str);
        Assert.Equal("0x48656c6c6f2c20576f726c6421", hex);
    }

    [Fact(Timeout = 120000)]
    public void HexToString()
    {
        var hex = "0x48656c6c6f2c20576f726c6421";
        var str = Utils.HexToString(hex);
        Assert.Equal("Hello, World!", str);
    }

    [Fact(Timeout = 120000)]
    public void GetUnixTimeStampNow()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var now2 = Utils.GetUnixTimeStampNow();
        Assert.Equal(now, now2);
    }

    [Fact(Timeout = 120000)]
    public void GetUnixTimeStampIn10Years()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var tenYears = 60 * 60 * 24 * 365 * 10;
        var tenYearsFromNow = now + tenYears;
        var tenYearsFromNow2 = Utils.GetUnixTimeStampIn10Years();
        Assert.Equal(tenYearsFromNow, tenYearsFromNow2);
    }

    [Fact(Timeout = 120000)]
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

    [Fact(Timeout = 120000)]
    public void ToWei_ConvertsCorrectly()
    {
        var eth = "1.5";
        var expectedWei = "1500000000000000000";
        Assert.Equal(expectedWei, Utils.ToWei(eth));
    }

    [Fact(Timeout = 120000)]
    public void ToWei_ThrowsOnInvalidInput()
    {
        var invalidEth = "abc";
        _ = Assert.Throws<ArgumentException>(() => Utils.ToWei(invalidEth));
    }

    [Fact(Timeout = 120000)]
    public void ToWei_ThrowsExceptionForInvalidInput()
    {
        var invalidEth = "invalid";
        _ = Assert.Throws<ArgumentException>(() => Utils.ToWei(invalidEth));
    }

    [Fact(Timeout = 120000)]
    public void ToWei_ConvertsNegativeValue()
    {
        var negativeEth = "-1.5";
        var expectedWei = new BigInteger(-1.5 * Math.Pow(10, 18)).ToString();
        Assert.Equal(expectedWei, Utils.ToWei(negativeEth));
    }

    [Fact(Timeout = 120000)]
    public void ToWei_ConvertsLargeFloat()
    {
        var largeEth = "1234567890.123456789";
        var expectedWei = new BigInteger(1234567890.123456789 * Math.Pow(10, 18)).ToString();
        Assert.Equal(expectedWei, Utils.ToWei(largeEth));
    }

    [Fact(Timeout = 120000)]
    public void ToEth_ConvertsCorrectly()
    {
        var wei = "1500000000000000000";
        var expectedEth = "1.5000";
        Assert.Equal(expectedEth, Utils.ToEth(wei));
    }

    [Fact(Timeout = 120000)]
    public void ToEth_WithCommas()
    {
        var wei = "1234500000000000000000";
        var expectedEth = "1,234.5000";
        Assert.Equal(expectedEth, Utils.ToEth(wei, 4, true));
    }

    [Fact(Timeout = 120000)]
    public void ToEth_ConvertsZeroWei()
    {
        var zeroWei = "0";
        Assert.Equal("0.0000", Utils.ToEth(zeroWei));
    }

    [Fact(Timeout = 120000)]
    public void ToEth_ConvertsSmallWei()
    {
        var smallWei = "1234";
        Assert.Equal("0.0000", Utils.ToEth(smallWei));
    }

    [Fact(Timeout = 120000)]
    public void FormatERC20_NoDecimalsNoCommas()
    {
        var wei = "1500000000000000000";
        var expectedEth = "2";
        Assert.Equal(expectedEth, Utils.FormatERC20(wei, 0));
    }

    [Fact(Timeout = 120000)]
    public void FormatERC20_LargeNumberWithCommas()
    {
        var wei = "1000000000000000000000000";
        var expectedEth = "1,000,000";
        Assert.Equal(expectedEth, Utils.FormatERC20(wei, 0, 18, true));
    }

    [Fact(Timeout = 120000)]
    public void FormatERC20_ConvertsZeroWei()
    {
        var zeroWei = "0";
        Assert.Equal("0", Utils.FormatERC20(zeroWei, 0));
    }

    [Fact(Timeout = 120000)]
    public void FormatERC20_SmallFractionalWei()
    {
        var fractionalWei = "10";
        Assert.Equal("0.0000", Utils.FormatERC20(fractionalWei, 4));
    }

    [Fact(Timeout = 120000)]
    public void FormatERC20_ThrowsOnInvalidWei()
    {
        var invalidWei = "not_a_number";
        _ = Assert.Throws<ArgumentException>(() => Utils.FormatERC20(invalidWei, 4));
    }

    [Fact(Timeout = 120000)]
    public void GenerateSIWE_ReturnsCorrectValue()
    {
        var loginPayloadData = new LoginPayloadData
        {
            Version = "1",
            ChainId = "421614",
            Nonce = "0",
            Address = Constants.ADDRESS_ZERO,
            Domain = "thirdweb.com",
            IssuedAt = "0",
            ExpirationTime = "0",
            InvalidBefore = "0"
        };
        var expectedSIWE =
            "thirdweb.com wants you to sign in with your Ethereum account:\n0x0000000000000000000000000000000000000000\n\n\nVersion: 1\nChain ID: 421614\nNonce: 0\nIssued At: 0\nExpiration Time: 0\nNot Before: 0";
        var siwe = Utils.GenerateSIWE(loginPayloadData);
        Assert.Equal(expectedSIWE, siwe);
    }

    [Fact(Timeout = 120000)]
    public void GenerateSIWE_WithAllOptional_ReturnsCorrectValue()
    {
        var loginPayloadData = new LoginPayloadData
        {
            Version = "1",
            ChainId = "421614",
            Nonce = "0",
            Address = Constants.ADDRESS_ZERO,
            Domain = "thirdweb.com",
            IssuedAt = "0",
            ExpirationTime = "0",
            InvalidBefore = "0",
            Statement = "This is a statement",
            Uri = "https://thirdweb.com",
            Resources = new List<string>() { "resource1", "resource2" }
        };
        var expectedSIWE =
            "thirdweb.com wants you to sign in with your Ethereum account:\n0x0000000000000000000000000000000000000000\n\nThis is a statement\n\nURI: https://thirdweb.com\nVersion: 1\nChain ID: 421614\nNonce: 0\nIssued At: 0\nExpiration Time: 0\nNot Before: 0\nResources:\n- resource1\n- resource2";
        var siwe = Utils.GenerateSIWE(loginPayloadData);
        Assert.Equal(expectedSIWE, siwe);
    }

    [Fact(Timeout = 120000)]
    public void GenerateSIWE_WithResources_ReturnsCorrectValue()
    {
        var loginPayloadData = new LoginPayloadData
        {
            Version = "1",
            ChainId = "421614",
            Nonce = "0",
            Address = Constants.ADDRESS_ZERO,
            Domain = "thirdweb.com",
            IssuedAt = "0",
            ExpirationTime = "0",
            InvalidBefore = "0",
            Resources = new List<string>() { "resource1", "resource2" }
        };
        var expectedSIWE =
            "thirdweb.com wants you to sign in with your Ethereum account:\n0x0000000000000000000000000000000000000000\n\n\nVersion: 1\nChain ID: 421614\nNonce: 0\nIssued At: 0\nExpiration Time: 0\nNot Before: 0\nResources:\n- resource1\n- resource2";
        var siwe = Utils.GenerateSIWE(loginPayloadData);
        Assert.Equal(expectedSIWE, siwe);
    }

    [Fact(Timeout = 120000)]
    public void GenerateSIWE_ThrowsOnNullLoginPayloadData()
    {
        LoginPayloadData? loginPayloadData = null;
        _ = Assert.Throws<ArgumentNullException>(() => Utils.GenerateSIWE(loginPayloadData));
    }

    [Fact(Timeout = 120000)]
    public void GenerateSIWE_ThrowsOnNullDomain()
    {
        var loginPayloadData = new LoginPayloadData
        {
            Version = "1",
            ChainId = "421614",
            Nonce = "0",
            Address = Constants.ADDRESS_ZERO,
            Domain = null!,
            IssuedAt = "0",
            ExpirationTime = "0",
            InvalidBefore = "0"
        };
        _ = Assert.Throws<ArgumentNullException>(() => Utils.GenerateSIWE(loginPayloadData));
    }

    [Fact(Timeout = 120000)]
    public void GenerateSIWE_ThrowsOnNullAddress()
    {
        var loginPayloadData = new LoginPayloadData
        {
            Version = "1",
            ChainId = "421614",
            Nonce = "0",
            Address = null!,
            Domain = "thirdweb.com",
            IssuedAt = "0",
            ExpirationTime = "0",
            InvalidBefore = "0"
        };
        _ = Assert.Throws<ArgumentNullException>(() => Utils.GenerateSIWE(loginPayloadData));
    }

    [Fact(Timeout = 120000)]
    public void GenerateSIWE_ThrowsOnNullVersion()
    {
        var loginPayloadData = new LoginPayloadData
        {
            Version = null!,
            ChainId = "421614",
            Nonce = "0",
            Address = Constants.ADDRESS_ZERO,
            Domain = "thirdweb.com",
            IssuedAt = "0",
            ExpirationTime = "0",
            InvalidBefore = "0"
        };
        _ = Assert.Throws<ArgumentNullException>(() => Utils.GenerateSIWE(loginPayloadData));
    }

    [Fact(Timeout = 120000)]
    public void GenerateSIWE_ThrowsOnNullChainId()
    {
        var loginPayloadData = new LoginPayloadData
        {
            Version = "1",
            ChainId = null!,
            Nonce = "0",
            Address = Constants.ADDRESS_ZERO,
            Domain = "thirdweb.com",
            IssuedAt = "0",
            ExpirationTime = "0",
            InvalidBefore = "0"
        };
        _ = Assert.Throws<ArgumentNullException>(() => Utils.GenerateSIWE(loginPayloadData));
    }

    [Fact(Timeout = 120000)]
    public void GenerateSIWE_ThrowsOnNullNonce()
    {
        var loginPayloadData = new LoginPayloadData
        {
            Version = "1",
            ChainId = "421614",
            Nonce = null!,
            Address = Constants.ADDRESS_ZERO,
            Domain = "thirdweb.com",
            IssuedAt = "0",
            ExpirationTime = "0",
            InvalidBefore = "0"
        };
        _ = Assert.Throws<ArgumentNullException>(() => Utils.GenerateSIWE(loginPayloadData));
    }

    [Fact(Timeout = 120000)]
    public void GenerateSIWE_ThrowsOnNullIssuedAt()
    {
        var loginPayloadData = new LoginPayloadData
        {
            Version = "1",
            ChainId = "421614",
            Nonce = "0",
            Address = Constants.ADDRESS_ZERO,
            Domain = "thirdweb.com",
            IssuedAt = null!,
            ExpirationTime = "0",
            InvalidBefore = "0"
        };
        _ = Assert.Throws<ArgumentNullException>(() => Utils.GenerateSIWE(loginPayloadData));
    }

    [Fact(Timeout = 120000)]
    public void ToChecksumAddress_ReturnsCorrectValue()
    {
        var address = "0x5aAeb6053F3E94C9b9A09f33669435E7Ef1BeAed".ToLower();
        var checksumAddress = Utils.ToChecksumAddress(address);
        Assert.Equal("0x5aAeb6053F3E94C9b9A09f33669435E7Ef1BeAed", checksumAddress);
    }

    [Fact(Timeout = 120000)]
    public void AdjustDecimals_ReturnsCorrectValue()
    {
        var value = new BigInteger(1500000000000000000); // 1.5 ETH
        var adjustedValue = value.AdjustDecimals(18, 0);
        Assert.Equal(new BigInteger(1), adjustedValue);
    }

    [Fact(Timeout = 120000)]
    public void AdjustDecimals_ReturnsCorrectValue2()
    {
        var value = new BigInteger(1500000000000000000); // 1.5 ETH
        // Not having 18 decimals is a sin
        var adjustedValue = value.AdjustDecimals(18, 2);
        Assert.Equal(new BigInteger(150), adjustedValue);
    }

    [Fact(Timeout = 120000)]
    public void AdjustDecimals_ReturnsCorrectValue3()
    {
        var value = new BigInteger(1500000000000000000); // 1.5 ETH
        var adjustedValue = value.AdjustDecimals(18, 18);
        Assert.Equal(new BigInteger(1500000000000000000), adjustedValue);
    }

    [Fact(Timeout = 120000)]
    public void AdjustDecimals_ReturnsCorrectValue4()
    {
        var value = new BigInteger(1500000000000000000); // 1.5 ETH
        // In some fictional world where ETH equivalent has 19 decimals
        var adjustedValue = value.AdjustDecimals(18, 19);
        Assert.Equal(new BigInteger(15000000000000000000), adjustedValue);
    }

    [Fact(Timeout = 120000)]
    public async Task FetchThirdwebChainDataAsync_ReturnsChainData_WhenResponseIsSuccessful()
    {
        var timer = System.Diagnostics.Stopwatch.StartNew();
        var chainId = new BigInteger(1);

        var chainData = await Utils.GetChainMetadata(this.Client, chainId);
        Assert.NotNull(chainData);
        _ = Assert.IsType<ThirdwebChainData>(chainData);

        Assert.Equal("Ethereum Mainnet", chainData.Name);
        Assert.Equal("eth", chainData.ShortName);
        Assert.Equal(1, chainData.ChainId);
        Assert.Equal(1, chainData.NetworkId);
        Assert.Equal("ethereum", chainData.Slug);
        Assert.Equal("https://ethereum.org", chainData.InfoURL);
        Assert.NotNull(chainData.Icon);
        Assert.NotNull(chainData.NativeCurrency);
        Assert.NotNull(chainData.NativeCurrency.Name);
        Assert.NotNull(chainData.NativeCurrency.Symbol);
        Assert.Equal(18, chainData.NativeCurrency.Decimals);
        Assert.NotNull(chainData.Explorers);

        timer.Stop();
        var timeAttempt1 = timer.ElapsedMilliseconds;

        timer.Restart();
        var chainData2 = await Utils.GetChainMetadata(this.Client, chainId);
        Assert.NotNull(chainData2);
        _ = Assert.IsType<ThirdwebChainData>(chainData);

        var timeAttempt2 = timer.ElapsedMilliseconds;
        Assert.True(timeAttempt1 > timeAttempt2);
    }

    [Fact(Timeout = 120000)]
    public async Task FetchThirdwebChainDataAsync_ReturnsStack_WhenResponseIsSuccessful()
    {
        var chainId = new BigInteger(4654);

        var chainData = await Utils.GetChainMetadata(this.Client, chainId);
        Assert.NotNull(chainData);
        _ = Assert.IsType<ThirdwebChainData>(chainData);
        Assert.NotNull(chainData.StackType);
        Assert.Contains("zksync", chainData.StackType);
    }

    [Fact(Timeout = 120000)]
    public async Task FetchThirdwebChainDataAsync_ThrowsException_WhenResponseHasError()
    {
        var chainId = 123124125418928133;

        var exception = await Assert.ThrowsAsync<Exception>(async () => await Utils.GetChainMetadata(this.Client, chainId));

        Assert.Contains("Failed to fetch chain data", exception.Message);
    }

    [Fact(Timeout = 120000)]
    public async Task FetchThirdwebChainDataAsync_ThrowsException_InvalidChainId()
    {
        var chainId = BigInteger.Zero;

        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await Utils.GetChainMetadata(this.Client, chainId));

        Assert.Contains("Invalid chain", exception.Message);
    }

    [Fact(Timeout = 120000)]
    public async void ToJsonExternalWalletFriendly_ReturnsCorrectValue4()
    {
        var pkWallet = await PrivateKeyWallet.Generate(this.Client); // Assume external wallet
        var msg = new AccountAbstraction.AccountMessage { Message = new byte[] { 0x01, 0x02, 0x03, 0x04 } };
        var verifyingContract = await pkWallet.GetAddress(); // doesn't matter here
        var typedDataRaw = EIP712.GetTypedDefinition_SmartAccount_AccountMessage("Account", "1", 137, verifyingContract);
        var json = Utils.ToJsonExternalWalletFriendly(typedDataRaw, msg);
        var jsonObject = JObject.Parse(json);
        var internalMsg = jsonObject.SelectToken("$.message.message");
        Assert.NotNull(internalMsg);
        Assert.Equal("0x01020304", internalMsg);
    }

    [Fact(Timeout = 120000)]
    public void HexToBytes32_ReturnsCorrectly()
    {
        var hex = "0x1";
        var bytes32 = Utils.HexToBytes32(hex);
        var expectedBytes = new byte[32];
        expectedBytes[31] = 1;
        Assert.Equal(expectedBytes, bytes32);

        hex = "1";
        bytes32 = Utils.HexToBytes32(hex);
        Assert.Equal(expectedBytes, bytes32);

        var hexTooLarge = "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef";
        _ = Assert.Throws<ArgumentException>(() => Utils.HexToBytes32(hexTooLarge));
    }

    [Fact(Timeout = 120000)]
    public async Task GetENSFromAddress_ThrowsException_WhenAddressIsNull()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () => await Utils.GetENSFromAddress(this.Client, null));

        Assert.Equal("address", exception.ParamName);
    }

    [Fact(Timeout = 120000)]
    public async Task GetENSFromAddress_ThrowsException_WhenAddressIsInvalid()
    {
        var invalidAddress = "invalid_address";
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await Utils.GetENSFromAddress(this.Client, invalidAddress));

        Assert.Contains("Invalid address", exception.Message);
    }

    [Fact(Timeout = 120000)]
    public async Task GetENSFromAddress_ReturnsENSName_WhenAddressIsValid()
    {
        var validAddress = "0xDaaBDaaC8073A7dAbdC96F6909E8476ab4001B34";
        var expectedENSName = "0xfirekeeper.eth";
        var ensName = await Utils.GetENSFromAddress(this.Client, validAddress);

        Assert.Equal(expectedENSName, ensName);

        ensName = await Utils.GetENSFromAddress(this.Client, validAddress);
        Assert.Equal(expectedENSName, ensName);
    }

    [Fact(Timeout = 120000)]
    public async Task GetAddressFromENS_ThrowsException_WhenENSNameIsNull()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () => await Utils.GetAddressFromENS(this.Client, null));
        Assert.Equal("ensName", exception.ParamName);
    }

    [Fact(Timeout = 120000)]
    public async Task GetAddressFromENS_ReturnsENSName_WhenENSNameIsAlreadyAddress()
    {
        var address = "0xDaaBDaaC8073A7dAbdC96F6909E8476ab4001B34".ToLower();
        var result = await Utils.GetAddressFromENS(this.Client, address);
        Assert.Equal(address.ToChecksumAddress(), result);
    }

    [Fact(Timeout = 120000)]
    public async Task GetAddressFromENS_ThrowsException_WhenENSNameIsInvalid()
    {
        var invalidENSName = "invalid_name";
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await Utils.GetAddressFromENS(this.Client, invalidENSName));

        Assert.Contains("Invalid ENS name", exception.Message);
    }

    [Fact(Timeout = 120000)]
    public async Task GetAddressFromENS_ReturnsAddress_WhenENSNameIsValid()
    {
        var validENSName = "0xfirekeeper.eth";
        var expectedAddress = "0xDaaBDaaC8073A7dAbdC96F6909E8476ab4001B34";
        var result = await Utils.GetAddressFromENS(this.Client, validENSName);

        Assert.Equal(expectedAddress.ToChecksumAddress(), result);

        result = await Utils.GetAddressFromENS(this.Client, validENSName);
        Assert.Equal(expectedAddress.ToChecksumAddress(), result);
    }

    /*
    public static async Task<bool> IsDeployed(ThirdwebClient client, BigInteger chainId, string address)
    {
        var rpc = ThirdwebRPC.GetRpcInstance(client, chainId);
        var code = await rpc.SendRequestAsync<string>("eth_getCode", address, "latest");
        return code != "0x";
    }
    */

    [Fact(Timeout = 120000)]
    public async Task IsDeployed_ReturnsTrue_WhenContractIsDeployed()
    {
        var chainId = new BigInteger(1);
        var address = "0xBC4CA0EdA7647A8aB7C2061c2E118A18a936f13D";
        var isDeployed = await Utils.IsDeployed(this.Client, chainId, address);

        Assert.True(isDeployed);
    }

    [Fact(Timeout = 120000)]
    public async Task IsDeployed_ReturnsFalse_WhenContractIsNotDeployed()
    {
        var chainId = new BigInteger(1);
        var address = await Utils.GetAddressFromENS(this.Client, "vitalik.eth");
        var isDeployed = await Utils.IsDeployed(this.Client, chainId, address);

        Assert.False(isDeployed);
    }

    [Fact(Timeout = 120000)]
    public async Task GetSocialProfiles_WithENS()
    {
        var socialProfiles = await Utils.GetSocialProfiles(this.Client, "joenrv.eth");

        Assert.NotNull(socialProfiles);
        Assert.True(socialProfiles.EnsProfiles.Count > 0);
    }

    [Fact(Timeout = 120000)]
    public async Task GetSocialProfiles_WithAddress()
    {
        var address = "0x2247d5d238d0f9d37184d8332aE0289d1aD9991b";
        var socialProfiles = await Utils.GetSocialProfiles(this.Client, address);

        Assert.NotNull(socialProfiles);
        Assert.True(socialProfiles.EnsProfiles.Count > 0);
    }

    [Fact(Timeout = 120000)]
    public async Task GetSocialProfiles_ThrowsException_WhenInputIsInvalid()
    {
        var invalidInput = "invalid_input";
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await Utils.GetSocialProfiles(this.Client, invalidInput));

        Assert.Contains("Invalid address or ENS.", exception.Message);
    }

    [Fact(Timeout = 120000)]
    public async Task GetSocialProfiles_ThrowsException_WhenInputIsNull()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () => await Utils.GetSocialProfiles(this.Client, null));

        Assert.Equal("addressOrEns", exception.ParamName);
    }

    [Fact(Timeout = 120000)]
    public async Task GetSocialProfiles_ThrowsException_InvalidAuth()
    {
        var client = ThirdwebClient.Create("a");
        var exception = await Assert.ThrowsAsync<Exception>(async () => await Utils.GetSocialProfiles(client, "joenrv.eth"));

        Assert.Contains("Failed to fetch social profiles", exception.Message);
    }

    [Fact(Timeout = 120000)]
    public async Task IsEip155Enforced_ReturnsFalse_WhenChainIs1()
    {
        var chainId = new BigInteger(1);
        var isEip155Enforced = await Utils.IsEip155Enforced(this.Client, chainId);

        Assert.False(isEip155Enforced);
    }

    // [Fact(Timeout = 120000)]
    // public async Task IsEip155Enforced_ReturnsTrue_WhenChainIs842()
    // {
    //     var chainId = new BigInteger(842);
    //     var isEip155Enforced = await Utils.IsEip155Enforced(this.Client, chainId);

    //     Assert.True(isEip155Enforced);
    // }

    [Fact(Timeout = 120000)]
    public void ReconstructHttpClient_WithHeaders()
    {
        var newClient = Utils.ReconstructHttpClient(this.Client.HttpClient, this.Client.HttpClient.Headers);
        var newHeaders = newClient.Headers;

        Assert.NotNull(newHeaders);
        Assert.Equal(this.Client.HttpClient.Headers, newHeaders);
    }

    [Fact(Timeout = 120000)]
    public void ReconstructHttpClient_WithoutHeaders()
    {
        var newClient = Utils.ReconstructHttpClient(this.Client.HttpClient);
        var newHeaders = newClient.Headers;

        Assert.NotNull(newHeaders);
        Assert.Empty(newHeaders);
    }

    [Fact(Timeout = 120000)]
    public async Task FetchGasPrice_Success()
    {
        var gasPrice = await Utils.FetchGasPrice(this.Client, 1);
        Assert.True(gasPrice > 0);
    }

    [Fact(Timeout = 120000)]
    public async Task FetchGasFees_1()
    {
        var (maxFee, maxPrio) = await Utils.FetchGasFees(this.Client, 1);
        Assert.True(maxFee > 0);
        Assert.True(maxPrio > 0);
        Assert.NotEqual(maxFee, maxPrio);
    }

    [Fact(Timeout = 120000)]
    public async Task FetchGasFees_137()
    {
        var (maxFee, maxPrio) = await Utils.FetchGasFees(this.Client, 137);
        Assert.True(maxFee > 0);
        Assert.True(maxPrio > 0);
        Assert.True(maxFee > maxPrio);
    }

    [Fact(Timeout = 120000)]
    public async Task FetchGasFees_80002()
    {
        var (maxFee, maxPrio) = await Utils.FetchGasFees(this.Client, 80002);
        Assert.True(maxFee > 0);
        Assert.True(maxPrio > 0);
        Assert.True(maxFee > maxPrio);
    }

    [Fact(Timeout = 120000)]
    public async Task FetchGasFees_Celo()
    {
        var chainId = new BigInteger(42220);
        var (maxFee, maxPrio) = await Utils.FetchGasFees(this.Client, chainId);
        Assert.True(maxFee > 0);
        Assert.True(maxPrio > 0);
        Assert.Equal(maxFee, maxPrio);

        chainId = new BigInteger(44787);
        (maxFee, maxPrio) = await Utils.FetchGasFees(this.Client, chainId);
        Assert.True(maxFee > 0);
        Assert.True(maxPrio > 0);
        Assert.Equal(maxFee, maxPrio);

        chainId = new BigInteger(62320);
        (maxFee, maxPrio) = await Utils.FetchGasFees(this.Client, chainId);
        Assert.True(maxFee > 0);
        Assert.True(maxPrio > 0);
        Assert.Equal(maxFee, maxPrio);
    }

    [Fact]
    public void PreprocessTypedDataJson_FormatsBigIntegers()
    {
        // Arrange
        var inputJson =
            /*lang=json,strict*/
            @"
        {
            ""from"": 973250616940336452028326648501327235277017847475,
            ""data"": ""0x"",
            ""nested"": {
                ""value"": 4294967295
            },
            ""array"": [
                123,
                973250616940336452028326648501327235277017847475
            ]
        }";

        var expectedJson =
            /*lang=json,strict*/
            @"
        {
            ""from"": ""973250616940336452028326648501327235277017847475"",
            ""data"": ""0x"",
            ""nested"": {
                ""value"": 4294967295
            },
            ""array"": [
                123,
                ""973250616940336452028326648501327235277017847475""
            ]
        }";

        // Act
        var processedJson = Utils.PreprocessTypedDataJson(inputJson);

        // Assert
        var expectedJObject = JObject.Parse(expectedJson);
        var processedJObject = JObject.Parse(processedJson);

        Assert.Equal(expectedJObject, processedJObject);
    }

    [Fact]
    public void PreprocessTypedDataJson_NoLargeNumbers_NoChange()
    {
        // Arrange
        var inputJson =
            /*lang=json,strict*/
            @"
        {
            ""value"": 123,
            ""nested"": {
                ""value"": 456
            },
            ""array"": [1, 2, 3]
        }";

        // Act
        var processedJson = Utils.PreprocessTypedDataJson(inputJson);
        var expectedJObject = JObject.Parse(inputJson);
        var processedJObject = JObject.Parse(processedJson);

        // Assert
        Assert.Equal(expectedJObject, processedJObject);
    }

    [Fact]
    public void PreprocessTypedDataJson_NestedLargeNumbers()
    {
        // Arrange
        var inputJson =
            /*lang=json,strict*/
            @"
        {
            ""nested"": {
                ""value"": 973250616940336452028326648501327235277017847475
            },
            ""array"": [123, 973250616940336452028326648501327235277017847475]
        }";

        var expectedJson =
            /*lang=json,strict*/
            @"
        {
            ""nested"": {
                ""value"": ""973250616940336452028326648501327235277017847475""
            },
            ""array"": [123, ""973250616940336452028326648501327235277017847475""]
        }";

        // Act
        var processedJson = Utils.PreprocessTypedDataJson(inputJson);

        // Assert
        var expectedJObject = JObject.Parse(expectedJson);
        var processedJObject = JObject.Parse(processedJson);

        Assert.Equal(expectedJObject, processedJObject);
    }
}
