﻿using Thirdweb;
using dotenv.net;
using System.Diagnostics;
using Thirdweb.Pay;
using Newtonsoft.Json;
using Nethereum.Hex.HexTypes;
using System.Numerics;

DotEnv.Load();

// Do not use secret keys client side, use client id/bundle id instead
var secretKey = Environment.GetEnvironmentVariable("THIRDWEB_SECRET_KEY");

// Do not use private keys client side, use InAppWallet/SmartWallet instead
var privateKey = Environment.GetEnvironmentVariable("PRIVATE_KEY");

// Fetch timeout options are optional, default is 60000ms
var client = ThirdwebClient.Create(secretKey: secretKey, fetchTimeoutOptions: new TimeoutOptions(storage: 30000, rpc: 60000));

// Create a private key wallet
var privateKeyWallet = await PrivateKeyWallet.Generate(client: client);
var walletAddress = await privateKeyWallet.GetAddress();
Console.WriteLine($"PK Wallet address: {walletAddress}");

#region Contract Interaction

var contract = await ThirdwebContract.Create(client: client, address: "0x81ebd23aA79bCcF5AaFb9c9c5B0Db4223c39102e", chain: 421614);
var readResult = await contract.Read<string>("name");
Console.WriteLine($"Contract read result: {readResult}");

#endregion

#region Account Linking

var inAppWalletMain = await InAppWallet.Create(client: client, authProvider: AuthProvider.Google);
if (!await inAppWalletMain.IsConnected())
{
    _ = await inAppWalletMain.LoginWithOauth(
        isMobile: false,
        (url) =>
        {
            var psi = new ProcessStartInfo { FileName = url, UseShellExecute = true };
            _ = Process.Start(psi);
        },
        "thirdweb://",
        new InAppWalletBrowser()
    );
}
Console.WriteLine($"Main InAppWallet address: {await inAppWalletMain.GetAddress()}");

// var inAppWalletToLink = await InAppWallet.Create(client: client, authProvider: AuthProvider.Siwe, siweSigner: privateKeyWallet);
// _ = await inAppWalletMain.LinkAccount(walletToLink: inAppWalletToLink, chainId: 421614);

// var linkedAccounts = await inAppWalletMain.GetLinkedAccounts();
// Console.WriteLine($"Linked accounts: {JsonConvert.SerializeObject(linkedAccounts, Formatting.Indented)}");

#endregion

#region ERC20 Smart Wallet - Sepolia

// var erc20SmartWalletSepolia = await SmartWallet.Create(
//     personalWallet: privateKeyWallet,
//     chainId: 11155111, // sepolia
//     gasless: true,
//     erc20PaymasterAddress: "0xEc87d96E3F324Dcc828750b52994C6DC69C8162b", // deposit paymaster
//     erc20PaymasterToken: "0x94a9D9AC8a22534E3FaCa9F4e7F2E2cf85d5E4C8" // usdc
// );
// var erc20SmartWalletSepoliaAddress = await erc20SmartWalletSepolia.GetAddress();
// Console.WriteLine($"ERC20 Smart Wallet Sepolia address: {erc20SmartWalletSepoliaAddress}");

// var selfTransfer = await ThirdwebTransaction.Create(
//     wallet: erc20SmartWalletSepolia,
//     txInput: new ThirdwebTransactionInput() { From = erc20SmartWalletSepoliaAddress, To = erc20SmartWalletSepoliaAddress, },
//     chainId: 11155111
// );

// var estimateGas = await ThirdwebTransaction.EstimateGasCosts(selfTransfer);
// Console.WriteLine($"Self transfer gas estimate: {estimateGas.ether}");
// Console.WriteLine("Make sure you have enough USDC!");
// Console.ReadLine();

// var receipt = await ThirdwebTransaction.SendAndWaitForTransactionReceipt(selfTransfer);
// Console.WriteLine($"Self transfer receipt: {JsonConvert.SerializeObject(receipt, Formatting.Indented)}");

#endregion

#region Chain Data Fetching

// var chainData = await Utils.FetchThirdwebChainDataAsync(client, 421614);
// Console.WriteLine($"Chain data: {JsonConvert.SerializeObject(chainData, Formatting.Indented)}");

#endregion

#region Self Transfer Transaction

// var tx = await ThirdwebTransaction.Create(
//     wallet: privateKeyWallet,
//     txInput: new ThirdwebTransactionInput()
//     {
//         From = await privateKeyWallet.GetAddress(),
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
//         From = await smartWallet.GetAddress(),
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
// var fiatQuoteParams = new BuyWithFiatQuoteParams(fromCurrencySymbol: "USD", toAddress: walletAddress, toChainId: "137", toTokenAddress: Thirdweb.Constants.NATIVE_TOKEN_ADDRESS, toAmount: "20");
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
//     fromTokenAddress: Thirdweb.Constants.NATIVE_TOKEN_ADDRESS,
//     toTokenAddress: Thirdweb.Constants.NATIVE_TOKEN_ADDRESS,
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
