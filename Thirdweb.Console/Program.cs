#pragma warning disable IDE0005
#pragma warning disable IDE0059

using Thirdweb;
using dotenv.net;
using System.Diagnostics;
using Thirdweb.Pay;
using Newtonsoft.Json;
using Nethereum.Hex.HexTypes;
using System.Numerics;
using Newtonsoft.Json.Linq;

DotEnv.Load();

// Do not use secret keys client side, use client id/bundle id instead
var secretKey = Environment.GetEnvironmentVariable("THIRDWEB_SECRET_KEY");

// Do not use private keys client side, use InAppWallet/SmartWallet instead
var privateKey = Environment.GetEnvironmentVariable("PRIVATE_KEY");

// Fetch timeout options are optional, default is 120000ms
var client = ThirdwebClient.Create(secretKey: secretKey, fetchTimeoutOptions: new TimeoutOptions(storage: 120000, rpc: 120000, other: 120000));

// Create a private key wallet
var privateKeyWallet = await PrivateKeyWallet.Generate(client: client);

// var walletAddress = await privateKeyWallet.GetAddress();
// Console.WriteLine($"PK Wallet address: {walletAddress}");

#region Contract Interaction

// var contract = await ThirdwebContract.Create(client: client, address: "0x81ebd23aA79bCcF5AaFb9c9c5B0Db4223c39102e", chain: 421614);
// var readResult = await contract.Read<string>("name");
// Console.WriteLine($"Contract read result: {readResult}");

#endregion

#region AA 0.6

// var smartWallet06 = await SmartWallet.Create(personalWallet: privateKeyWallet, chainId: 421614, gasless: true, entryPoint: Constants.ENTRYPOINT_ADDRESS_V06);

// var receipt06 = await smartWallet06.ExecuteTransaction(
//     new ThirdwebTransactionInput()
//     {
//         To = await smartWallet06.GetAddress(),
//         Value = new HexBigInteger(BigInteger.Zero),
//         Data = "0x",
//     }
// );

// Console.WriteLine($"Receipt: {receipt06}");

#endregion

#region AA 0.7

// var smartWallet07 = await SmartWallet.Create(personalWallet: privateKeyWallet, chainId: 421614, gasless: true, entryPoint: Constants.ENTRYPOINT_ADDRESS_V07);

// var receipt07 = await smartWallet07.ExecuteTransaction(
//     new ThirdwebTransactionInput()
//     {
//         To = await smartWallet07.GetAddress(),
//         Value = new HexBigInteger(BigInteger.Zero),
//         Data = "0x"
//     }
// );

// Console.WriteLine($"Receipt: {receipt07}");

#endregion

#region AA ZkSync (Abstract)

// var smartWalletAbstract = await SmartWallet.Create(personalWallet: privateKeyWallet, chainId: 11124, gasless: true);

// var hash = await smartWalletAbstract.SendTransaction(
//     new ThirdwebTransactionInput()
//     {
//         To = await smartWalletAbstract.GetAddress(),
//         Value = new HexBigInteger(BigInteger.Zero),
//         Data = "0x"
//     }
// );

// Console.WriteLine($"Transaction hash: {hash}");

#endregion

#region Account Linking

// var inAppWalletMain = await InAppWallet.Create(client: client, authProvider: AuthProvider.Google);
// if (!await inAppWalletMain.IsConnected())
// {
//     _ = await inAppWalletMain.LoginWithOauth(
//         isMobile: false,
//         (url) =>
//         {
//             var psi = new ProcessStartInfo { FileName = url, UseShellExecute = true };
//             _ = Process.Start(psi);
//         },
//         "thirdweb://",
//         new InAppWalletBrowser()
//     );
// }
// Console.WriteLine($"Main InAppWallet address: {await inAppWalletMain.GetAddress()}");

// var inAppWalletToLink = await InAppWallet.Create(client: client, authProvider: AuthProvider.Siwe, siweSigner: privateKeyWallet);
// _ = inAppWalletToLink.SendOTP();
// Console.WriteLine("Enter OTP:");
// var otp = Console.ReadLine();
// _ = await inAppWalletMain.LinkAccount(walletToLink: inAppWalletToLink, otp: otp);

// var linkedAccounts = await inAppWalletMain.GetLinkedAccounts();
// Console.WriteLine($"Linked accounts: {JsonConvert.SerializeObject(linkedAccounts, Formatting.Indented)}");

#endregion

#region Smart Wallet - Authenticate

// var appWallet = await InAppWallet.Create(client: client, authProvider: AuthProvider.Google);
// if (!await appWallet.IsConnected())
// {
//     _ = await appWallet.LoginWithOauth(
//         isMobile: false,
//         (url) =>
//         {
//             var psi = new ProcessStartInfo { FileName = url, UseShellExecute = true };
//             _ = Process.Start(psi);
//         },
//         "thirdweb://",
//         new InAppWalletBrowser()
//     );
// }
// var smartWallet = await SmartWallet.Create(appWallet, 37714555429);

// var data = await smartWallet.Authenticate<JObject>(
//     domain: "https://myepicdomain.com",
//     chainId: 37714555429,
//     authPayloadPath: "/my-epic-auth/login",
//     authLoginPath: "/my-epic-auth/login",
//     separatePayloadAndSignatureInBody: true,
//     authPayloadMethod: "GET",
//     authLoginMethod: "POST"
// );
// Console.WriteLine($"Token: {data["token"]}");

#endregion

#region ERC20 Smart Wallet - Base USDC

// var erc20SmartWallet = await SmartWallet.Create(
//     personalWallet: privateKeyWallet,
//     chainId: 8453, // base mainnet
//     gasless: true,
//     factoryAddress: "0xEc87d96E3F324Dcc828750b52994C6DC69C8162b",
//     entryPoint: Constants.ENTRYPOINT_ADDRESS_V07,
//     tokenPaymaster: TokenPaymaster.BASE_USDC
// );
// var erc20SmartWalletAddress = await erc20SmartWallet.GetAddress();
// Console.WriteLine($"ERC20 Smart Wallet address: {erc20SmartWalletAddress}");

// var selfTransfer = await ThirdwebTransaction.Create(
//     wallet: erc20SmartWallet,
//     txInput: new ThirdwebTransactionInput() { To = erc20SmartWalletAddress, },
//     chainId: 8453
// );

// var estimateGas = await ThirdwebTransaction.EstimateGasCosts(selfTransfer);
// Console.WriteLine($"Self transfer gas estimate: {estimateGas.Ether}");
// Console.WriteLine("Make sure you have enough USDC!");
// Console.ReadLine();

// var receipt = await ThirdwebTransaction.SendAndWaitForTransactionReceipt(selfTransfer);
// Console.WriteLine($"Self transfer receipt: {JsonConvert.SerializeObject(receipt, Formatting.Indented)}");

#endregion

#region Chain Data Fetching

// var chainData = await Utils.GetChainMetadata(client, 421614);
// Console.WriteLine($"Chain data: {JsonConvert.SerializeObject(chainData, Formatting.Indented)}");

#endregion

#region Self Transfer Transaction

// var tx = await ThirdwebTransaction.Create(
//     wallet: privateKeyWallet,
//     txInput: new ThirdwebTransactionInput()
//     {
//         To = await privateKeyWallet.GetAddress(),
//         Value = new HexBigInteger(BigInteger.Zero),
//     },
//     chainId: 842
// );
// var txHash = await ThirdwebTransaction.Send(tx);
// Console.WriteLine($"Transaction hash: {txHash}");

#endregion

#region InAppWallet - OAuth

// var inAppWalletOAuth = await InAppWallet.Create(client: client, authProvider: AuthProvider.Telegram);
// if (!await inAppWalletOAuth.IsConnected())
// {
//     _ = await inAppWalletOAuth.LoginWithOauth(
//         isMobile: false,
//         (url) =>
//         {
//             var psi = new ProcessStartInfo { FileName = url, UseShellExecute = true };
//             _ = Process.Start(psi);
//         },
//         "thirdweb://",
//         new InAppWalletBrowser()
//     );
// }
// var inAppWalletOAuthAddress = await inAppWalletOAuth.GetAddress();
// Console.WriteLine($"InAppWallet OAuth address: {inAppWalletOAuthAddress}");

#endregion

#region Smart Wallet - Gasless Transaction

// var smartWallet = await SmartWallet.Create(privateKeyWallet, 78600);

// // Self transfer 0
// var tx2 = await ThirdwebTransaction.Create(
//     smartWallet,
//     new ThirdwebTransactionInput()
//     {
//         To = await smartWallet.GetAddress(),
//         Value = new HexBigInteger(BigInteger.Zero)
//     },
//     78600
// );
// var txHash2 = await ThirdwebTransaction.Send(tx2);
// Console.WriteLine($"Transaction hash: {txHash2}");

#endregion

#region Buy with Fiat

// // Supported currencies
// var supportedCurrencies = await ThirdwebPay.GetBuyWithFiatCurrencies(client);
// Console.WriteLine($"Supported currencies: {JsonConvert.SerializeObject(supportedCurrencies, Formatting.Indented)}");

// // Get a Buy with Fiat quote
// var fiatQuoteParams = new BuyWithFiatQuoteParams(fromCurrencySymbol: "USD", toAddress: walletAddress, toChainId: "137", toTokenAddress: Constants.NATIVE_TOKEN_ADDRESS, toAmount: "20");
// var fiatOnrampQuote = await ThirdwebPay.GetBuyWithFiatQuote(client, fiatQuoteParams);
// Console.WriteLine($"Fiat onramp quote: {JsonConvert.SerializeObject(fiatOnrampQuote, Formatting.Indented)}");

// // Get a Buy with Fiat link
// var onRampLink = ThirdwebPay.BuyWithFiat(fiatOnrampQuote);
// Console.WriteLine($"Fiat onramp link: {onRampLink}");

// // Open onramp link to start the process (use your framework's version of this)
// var psi = new ProcessStartInfo { FileName = onRampLink, UseShellExecute = true };
// _ = Process.Start(psi);

// // Poll for status
// var currentOnRampStatus = OnRampStatus.NONE;
// while (currentOnRampStatus is not OnRampStatus.ON_RAMP_TRANSFER_COMPLETED and not OnRampStatus.ON_RAMP_TRANSFER_FAILED)
// {
//     var onRampStatus = await ThirdwebPay.GetBuyWithFiatStatus(client, fiatOnrampQuote.IntentId);
//     currentOnRampStatus = Enum.Parse<OnRampStatus>(onRampStatus.Status);
//     Console.WriteLine($"Fiat onramp status: {JsonConvert.SerializeObject(onRampStatus, Formatting.Indented)}");
//     await Task.Delay(5000);
// }

#endregion

#region Buy with Crypto

// // Swap Polygon MATIC to Base ETH
// var swapQuoteParams = new BuyWithCryptoQuoteParams(
//     fromAddress: walletAddress,
//     fromChainId: 137,
//     fromTokenAddress: Constants.NATIVE_TOKEN_ADDRESS,
//     toTokenAddress: Constants.NATIVE_TOKEN_ADDRESS,
//     toChainId: 8453,
//     toAmount: "0.1"
// );
// var swapQuote = await ThirdwebPay.GetBuyWithCryptoQuote(client, swapQuoteParams);
// Console.WriteLine($"Swap quote: {JsonConvert.SerializeObject(swapQuote, Formatting.Indented)}");

// // Initiate swap
// var txHash3 = await ThirdwebPay.BuyWithCrypto(wallet: privateKeyWallet, buyWithCryptoQuote: swapQuote);
// Console.WriteLine($"Swap transaction hash: {txHash3}");

// // Poll for status
// var currentSwapStatus = SwapStatus.NONE;
// while (currentSwapStatus is not SwapStatus.COMPLETED and not SwapStatus.FAILED)
// {
//     var swapStatus = await ThirdwebPay.GetBuyWithCryptoStatus(client, txHash3);
//     currentSwapStatus = Enum.Parse<SwapStatus>(swapStatus.Status);
//     Console.WriteLine($"Swap status: {JsonConvert.SerializeObject(swapStatus, Formatting.Indented)}");
//     await Task.Delay(5000);
// }

#endregion

#region Storage Actions

// // Will download from IPFS or normal urls
// var downloadResult = await ThirdwebStorage.Download<string>(client: client, uri: "AnyUrlIncludingIpfs");
// Console.WriteLine($"Download result: {downloadResult}");

// // Will upload to IPFS
// var uploadResult = await ThirdwebStorage.Upload(client: client, path: "AnyPath");
// Console.WriteLine($"Upload result preview: {uploadResult.PreviewUrl}");

#endregion

#region RPC Access

// // Access RPC directly if needed, generally not recommended
// var rpc = ThirdwebRPC.GetRpcInstance(client, 421614);
// var blockNumber = await rpc.SendRequestAsync<string>("eth_blockNumber");
// Console.WriteLine($"Block number: {blockNumber}");

#endregion
