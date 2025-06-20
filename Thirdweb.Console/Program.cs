#pragma warning disable IDE0005
#pragma warning disable IDE0059

using System.Diagnostics;
using System.Numerics;
using System.Text;
using dotenv.net;
using Nethereum.ABI;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Thirdweb;
using Thirdweb.AccountAbstraction;
using Thirdweb.AI;
using Thirdweb.Bridge;
using Thirdweb.Indexer;
using Thirdweb.Pay;

DotEnv.Load();

// Do not use secret keys client side, use client id/bundle id instead
var secretKey = Environment.GetEnvironmentVariable("THIRDWEB_SECRET_KEY");

// Do not use private keys client side, use InAppWallet/SmartWallet instead
var privateKey = Environment.GetEnvironmentVariable("PRIVATE_KEY");

// Fetch timeout options are optional, default is 120000ms
var client = ThirdwebClient.Create(secretKey: secretKey);

//  Create a private key wallet
var privateKeyWallet = await PrivateKeyWallet.Generate(client);

// var walletAddress = await privateKeyWallet.GetAddress();
// Console.WriteLine($"PK Wallet address: {walletAddress}");

#region Contract Interaction

// var contract = await ThirdwebContract.Create(client: client, address: "0xbc4ca0eda7647a8ab7c2061c2e118a18a936f13d", chain: 1);
// var nfts = await contract.ERC721_GetAllNFTs();
// Console.WriteLine($"NFTs: {JsonConvert.SerializeObject(nfts, Formatting.Indented)}");

#endregion

#region Bridge

// // Create a ThirdwebBridge instance
// var bridge = await ThirdwebBridge.Create(client);

// // Buy - Get a quote for buying a specific amount of tokens
// var buyQuote = await bridge.Buy_Quote(
//     originChainId: 1,
//     originTokenAddress: "0xA0b86991c6218b36c1d19D4a2e9Eb0cE3606eB48", // USDC on Ethereum
//     destinationChainId: 324,
//     destinationTokenAddress: Constants.NATIVE_TOKEN_ADDRESS, // ETH on zkSync
//     buyAmountWei: BigInteger.Parse("0.01".ToWei())
// );
// Console.WriteLine($"Buy quote: {JsonConvert.SerializeObject(buyQuote, Formatting.Indented)}");

// // Buy - Get an executable set of transactions (alongside a quote) for buying a specific amount of tokens
// var preparedBuy = await bridge.Buy_Prepare(
//     originChainId: 1,
//     originTokenAddress: "0xA0b86991c6218b36c1d19D4a2e9Eb0cE3606eB48", // USDC on Ethereum
//     destinationChainId: 324,
//     destinationTokenAddress: Constants.NATIVE_TOKEN_ADDRESS, // ETH on zkSync
//     buyAmountWei: BigInteger.Parse("0.01".ToWei()),
//     sender: await Utils.GetAddressFromENS(client, "vitalik.eth"),
//     receiver: await myWallet.GetAddress()
// );
// Console.WriteLine($"Prepared Buy contains {preparedBuy.Steps.Count} steps(s) with a total of {preparedBuy.Steps.Sum(step => step.Transactions.Count)} transactions!");

// // Sell - Get a quote for selling a specific amount of tokens
// var sellQuote = await bridge.Sell_Quote(
//     originChainId: 324,
//     originTokenAddress: Constants.NATIVE_TOKEN_ADDRESS, // ETH on zkSync
//     destinationChainId: 1,
//     destinationTokenAddress: "0xA0b86991c6218b36c1d19D4a2e9Eb0cE3606eB48", // USDC on Ethereum
//     sellAmountWei: BigInteger.Parse("0.01".ToWei())
// );
// Console.WriteLine($"Sell quote: {JsonConvert.SerializeObject(sellQuote, Formatting.Indented)}");

// // Sell - Get an executable set of transactions (alongside a quote) for selling a specific amount of tokens
// var preparedSell = await bridge.Sell_Prepare(
//     originChainId: 324,
//     originTokenAddress: Constants.NATIVE_TOKEN_ADDRESS, // ETH on zkSync
//     destinationChainId: 1,
//     destinationTokenAddress: "0xA0b86991c6218b36c1d19D4a2e9Eb0cE3606eB48", // USDC on Ethereum
//     sellAmountWei: BigInteger.Parse("0.01".ToWei()),
//     sender: await Utils.GetAddressFromENS(client, "vitalik.eth"),
//     receiver: await myWallet.GetAddress()
// );
// Console.WriteLine($"Prepared Sell contains {preparedBuy.Steps.Count} steps(s) with a total of {preparedBuy.Steps.Sum(step => step.Transactions.Count)} transactions!");

// // Transfer - Get an executable transaction for transferring a specific amount of tokens
// var preparedTransfer = await bridge.Transfer_Prepare(
//     chainId: 137,
//     tokenAddress: Constants.NATIVE_TOKEN_ADDRESS, // POL on Polygon
//     transferAmountWei: BigInteger.Parse("0.01".ToWei()),
//     sender: await Utils.GetAddressFromENS(client, "vitalik.eth"),
//     receiver: await myWallet.GetAddress()
// );
// Console.WriteLine($"Prepared Transfer: {JsonConvert.SerializeObject(preparedTransfer, Formatting.Indented)}");

// // You may use our extensions to execute yourself...
// var myTx = await preparedTransfer.Transactions[0].ToThirdwebTransaction(myWallet);
// var myHash = await ThirdwebTransaction.Send(myTx);

// // ...and poll for the status...
// var status = await bridge.Status(transactionHash: myHash, chainId: 1);
// var isComplete = status.StatusType == StatusType.COMPLETED;
// Console.WriteLine($"Status: {JsonConvert.SerializeObject(status, Formatting.Indented)}");

// // Or use our Execute extensions directly to handle everything for you!

// // Execute a prepared Buy
// var buyResult = await bridge.Execute(myWallet, preparedBuy);
// var buyHashes = buyResult.Select(receipt => receipt.TransactionHash).ToList();
// Console.WriteLine($"Buy hashes: {JsonConvert.SerializeObject(buyHashes, Formatting.Indented)}");

// // Execute a prepared Sell
// var sellResult = await bridge.Execute(myWallet, preparedSell);
// var sellHashes = sellResult.Select(receipt => receipt.TransactionHash).ToList();
// Console.WriteLine($"Sell hashes: {JsonConvert.SerializeObject(sellHashes, Formatting.Indented)}");

// // Execute a prepared Transfer
// var transferResult = await bridge.Execute(myWallet, preparedTransfer);
// var transferHashes = transferResult.Select(receipt => receipt.TransactionHash).ToList();
// Console.WriteLine($"Transfer hashes: {JsonConvert.SerializeObject(transferHashes, Formatting.Indented)}");

// // Onramp - Get a quote for buying crypto with Fiat
// var preparedOnramp = await bridge.Onramp_Prepare(
//     onramp: OnrampProvider.Coinbase,
//     chainId: 8453,
//     tokenAddress: "0x833589fCD6eDb6E08f4c7C32D4f71b54bdA02913", // USDC on Base
//     amount: "10000000",
//     receiver: await myWallet.GetAddress()
// );
// Console.WriteLine($"Onramp link: {preparedOnramp.Link}");
// Console.WriteLine($"Full onramp quote and steps data: {JsonConvert.SerializeObject(preparedOnramp, Formatting.Indented)}");

// while (true)
// {
//     var onrampStatus = await bridge.Onramp_Status(id: preparedOnramp.Id);
//     Console.WriteLine($"Full Onramp Status: {JsonConvert.SerializeObject(onrampStatus, Formatting.Indented)}");
//     if (onrampStatus.StatusType is StatusType.COMPLETED or StatusType.FAILED)
//     {
//         break;
//     }
//     await ThirdwebTask.Delay(5000);
// }

// if (preparedOnramp.IsSwapRequiredPostOnramp())
// {
//     // Execute additional steps that are required post-onramp to get to your token, manually or via the Execute extension
//     var receipts = await bridge.Execute(myWallet, preparedOnramp);
//     Console.WriteLine($"Onramp receipts: {JsonConvert.SerializeObject(receipts, Formatting.Indented)}");
// }
// else
// {
//     Console.WriteLine("No additional steps required post-onramp, you can use the tokens directly!");
// }

#endregion

#region Indexer

// // Create a ThirdwebInsight instance
// var insight = await ThirdwebInsight.Create(client);

// var ethPriceToday = await insight.GetTokenPrice(addressOrSymbol: "ETH", chainId: 1);
// Console.WriteLine($"ETH price today: {ethPriceToday.PriceUsd}");

// var ethPriceYesterday = await insight.GetTokenPrice(addressOrSymbol: "ETH", chainId: 1, timestamp: Utils.GetUnixTimeStampNow() - 86400);
// Console.WriteLine($"ETH price yesterday: {ethPriceYesterday.PriceUsd}");

// var multiTokenPrices = await insight.GetTokenPrices(addressOrSymbols: new[] { "POL", "APE" }, chainIds: new BigInteger[] { 137, 33139 });
// Console.WriteLine($"Multi token prices: {JsonConvert.SerializeObject(multiTokenPrices, Formatting.Indented)}");

// // Setup some filters
// var address = await Utils.GetAddressFromENS(client, "vitalik.eth");
// var chains = new BigInteger[] { 1, 137, 42161 };

// // Fetch all token types
// var tokens = await insight.GetTokens(address, chains);
// Console.WriteLine($"ERC20 Count: {tokens.erc20Tokens.Length} | ERC721 Count: {tokens.erc721Tokens.Length} | ERC1155 Count: {tokens.erc1155Tokens.Length}");

// // Fetch specific token types
// var erc20Tokens = await insight.GetTokens_ERC20(address, chains);
// Console.WriteLine($"ERC20 Tokens: {JsonConvert.SerializeObject(erc20Tokens, Formatting.Indented)}");

// // Fetch specific token types
// var erc721Tokens = await insight.GetTokens_ERC721(address, chains);
// Console.WriteLine($"ERC721 Tokens: {JsonConvert.SerializeObject(erc721Tokens, Formatting.Indented)}");

// // Fetch specific token types
// var erc1155Tokens = await insight.GetTokens_ERC1155(address, chains);
// Console.WriteLine($"ERC1155 Tokens: {JsonConvert.SerializeObject(erc1155Tokens, Formatting.Indented)}");

// // Fetch events (great amount of optional filters available)
// var events = await insight.GetEvents(
//     chainIds: new BigInteger[] { 1 }, // ethereum
//     contractAddress: "0xbc4ca0eda7647a8ab7c2061c2e118a18a936f13d", // bored apes
//     eventSignature: "Transfer(address,address,uint256)", // transfer event
//     fromTimestamp: Utils.GetUnixTimeStampNow() - 3600, // last hour
//     sortBy: SortBy.TransactionIndex, // block number, block timestamp or transaction index
//     sortOrder: SortOrder.Desc, // latest first
//     limit: 5 // last 5 transfers
// );
// Console.WriteLine($"Events: {JsonConvert.SerializeObject(events, Formatting.Indented)}");

// // Fetch transactions (great amount of optional filters available)
// var transactions = await insight.GetTransactions(
//     chainIds: new BigInteger[] { 1 }, // ethereum
//     contractAddress: "0xbc4ca0eda7647a8ab7c2061c2e118a18a936f13d", // bored apes
//     fromTimestamp: Utils.GetUnixTimeStampNow() - 3600, // last hour
//     sortBy: SortBy.TransactionIndex, // block number, block timestamp or transaction index
//     sortOrder: SortOrder.Desc, // latest first
//     limit: 5 // last 5 transactions
// );
// Console.WriteLine($"Transactions: {JsonConvert.SerializeObject(transactions, Formatting.Indented)}");

// // Use ToNFT to ToNFTList extensions
// var convertedNft = erc721Tokens[0].ToNFT();

// var convertedNfts = erc721Tokens.ToNFTList();

// // Use NFT Extensions (GetNFTImageBytes, or GetNFTSprite in Unity)
// var imageBytes = await convertedNft.GetNFTImageBytes(client);
// var pathToSave = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "nft.png");
// await File.WriteAllBytesAsync(pathToSave, imageBytes);
// Console.WriteLine($"NFT image saved to: {pathToSave}");

#endregion

#region AI

// // Prepare some context
// var myChain = 11155111;
// var myWallet = await SmartWallet.Create(personalWallet: await PrivateKeyWallet.Generate(client), chainId: myChain, gasless: true);
// var myContractAddress = "0xe2cb0eb5147b42095c2FfA6F7ec953bb0bE347D8"; // DropERC1155
// var usdcAddress = "0x1c7D4B196Cb0C7B01d743Fbc6116a902379C7238";

// // Create a Nebula session
// var nebula = await ThirdwebNebula.Create(client);

// // Chat, passing wallet context
// var response1 = await nebula.Chat(message: "What is my wallet address?", wallet: myWallet);
// Console.WriteLine($"Response 1: {response1.Message}");

// // Chat, passing contract context
// var response2 = await nebula.Chat(
//     message: "What's the total supply of token id 0 for this contract?",
//     context: new NebulaContext(contractAddresses: new List<string> { myContractAddress }, chainIds: new List<BigInteger> { myChain })
// );
// Console.WriteLine($"Response 2: {response2.Message}");

// // Chat, passing multiple messages and context
// var response3 = await nebula.Chat(
//     messages: new List<NebulaChatMessage>
//     {
//         new($"Tell me the name of this contract: {myContractAddress}", NebulaChatRole.User),
//         new("The name of the contract is CatDrop", NebulaChatRole.Assistant),
//         new("What's the symbol of this contract?", NebulaChatRole.User),
//     },
//     context: new NebulaContext(contractAddresses: new List<string> { myContractAddress }, chainIds: new List<BigInteger> { myChain })
// );
// Console.WriteLine($"Response 3: {response3.Message}");

// // Execute, this directly sends transactions
// var executionResult = await nebula.Execute("Approve 1 USDC to vitalik.eth", wallet: myWallet, context: new NebulaContext(contractAddresses: new List<string>() { usdcAddress }));
// if (executionResult.TransactionReceipts != null && executionResult.TransactionReceipts.Count > 0)
// {
//     Console.WriteLine($"Receipt: {executionResult.TransactionReceipts[0]}");
// }
// else
// {
//     Console.WriteLine($"Message: {executionResult.Message}");
// }

// // Batch execute
// var batchExecutionResult = await nebula.Execute(
//     new List<NebulaChatMessage>
//     {
//         new("What's the address of vitalik.eth", NebulaChatRole.User),
//         new("The address of vitalik.eth is 0xd8dA6BF26964aF8E437eEa5e3616511D7G3a3298", NebulaChatRole.Assistant),
//         new("Approve 1 USDC to them", NebulaChatRole.User),
//     },
//     wallet: myWallet,
//     context: new NebulaContext(contractAddresses: new List<string>() { usdcAddress })
// );
// if (batchExecutionResult.TransactionReceipts != null && batchExecutionResult.TransactionReceipts.Count > 0)
// {
//     Console.WriteLine($"Receipts: {JsonConvert.SerializeObject(batchExecutionResult.TransactionReceipts, Formatting.Indented)}");
// }
// else
// {
//     Console.WriteLine($"Message: {batchExecutionResult.Message}");
// }

#endregion

#region Get Social Profiles

// var socialProfiles = await Utils.GetSocialProfiles(client, "joenrv.eth");
// Console.WriteLine($"Social Profiles: {socialProfiles}");

#endregion

#region AA 0.6

// var smartWallet06 = await SmartWallet.Create(personalWallet: privateKeyWallet, chainId: 421614, gasless: true);
// var receipt06 = await smartWallet06.Transfer(chainId: 421614, toAddress: await smartWallet06.GetAddress(), weiAmount: 0);
// Console.WriteLine($"Receipt: {receipt06}");

#endregion

#region AA 0.7

// var smartWallet07 = await SmartWallet.Create(personalWallet: privateKeyWallet, chainId: 421614, gasless: true, entryPoint: Constants.ENTRYPOINT_ADDRESS_V07);
// var receipt07 = await smartWallet07.Transfer(chainId: 421614, toAddress: await smartWallet07.GetAddress(), weiAmount: 0);
// Console.WriteLine($"Receipt: {receipt07}");

#endregion

#region AA ZkSync

// var zkSmartWallet = await SmartWallet.Create(personalWallet: privateKeyWallet, chainId: 4654, gasless: true);

// var hash = await zkSmartWallet.SendTransaction(
//     new ThirdwebTransactionInput(4654)
//     {
//         To = await zkSmartWallet.GetAddress(),
//         Value = new HexBigInteger(BigInteger.Zero),
//         Data = "0x",
//     }
// );

// Console.WriteLine($"Transaction hash: {hash}");

#endregion

#region Engine Wallet

// // EngineWallet is compatible with IThirdwebWallet and can be used with any SDK method/extension
// var engineWallet = await EngineWallet.Create(
//     client: client,
//     engineUrl: Environment.GetEnvironmentVariable("ENGINE_URL"),
//     authToken: Environment.GetEnvironmentVariable("ENGINE_ACCESS_TOKEN"),
//     walletAddress: Environment.GetEnvironmentVariable("ENGINE_BACKEND_WALLET_ADDRESS"),
//     timeoutSeconds: null, // no timeout
//     additionalHeaders: null // can set things like x-account-address if using basic session keys
// );

// // Simple self transfer
// var receipt = await engineWallet.Transfer(chainId: 11155111, toAddress: await engineWallet.GetAddress(), weiAmount: 0);
// Console.WriteLine($"Receipt: {receipt}");

#endregion

#region EIP-7702

var chain = 11155111; // 7702-compatible chain

// Connect to EOA
var smartEoa = await InAppWallet.Create(client, authProvider: AuthProvider.Guest, executionMode: ExecutionMode.EIP7702Sponsored);
if (!await smartEoa.IsConnected())
{
    _ = await smartEoa.LoginWithGuest(defaultSessionIdOverride: new Guid().ToString());
}
var smartEoaAddress = await smartEoa.GetAddress();
Console.WriteLine($"User Wallet address: {await smartEoa.GetAddress()}");

// Upgrade EOA - This wallet explicitly uses EIP-7702 delegation to the thirdweb MinimalAccount (will delegate upon first tx)

// Transact, will upgrade EOA
var receipt = await smartEoa.Transfer(chainId: chain, toAddress: await Utils.GetAddressFromENS(client, "vitalik.eth"), weiAmount: 0);
Console.WriteLine($"Transfer Receipt: {receipt.TransactionHash}");

// Double check that it was upgraded
var isDelegated = await Utils.IsDelegatedAccount(client, chain, smartEoaAddress);
Console.WriteLine($"Is delegated: {isDelegated}");

// Create a session key
var sessionKeyReceipt = await smartEoa.CreateSessionKey(
    chain,
    new SessionSpec()
    {
        Signer = await Utils.GetAddressFromENS(client, "vitalik.eth"),
        IsWildcard = true,
        ExpiresAt = Utils.GetUnixTimeStampNow() + 86400, // 1 day
        CallPolicies = new List<CallSpec>() { },
        TransferPolicies = new List<TransferSpec>() { },
        Uid = "my-session-key-uid".HashMessage().HexToBytes()
    }
);
Console.WriteLine($"Session key receipt: {sessionKeyReceipt.TransactionHash}");

#endregion

#region Smart Ecosystem Wallet

// var eco = await EcosystemWallet.Create(client: client, ecosystemId: "ecosystem.the-bonfire", authProvider: AuthProvider.Github);
// if (!await eco.IsConnected())
// {
//     _ = await eco.LoginWithOauth(
//         isMobile: false,
//         browserOpenAction: (url) =>
//         {
//             var psi = new ProcessStartInfo { FileName = url, UseShellExecute = true };
//             _ = Process.Start(psi);
//         }
//     );
// }
// var smartEco = await SmartWallet.Create(eco, 421614);
// var addy = await smartEco.GetAddress();
// Console.WriteLine($"Smart Ecosystem Wallet address: {addy}");

#endregion

#region Ecosystem Wallet

// var ecosystemWallet = await EcosystemWallet.Create(client: client, ecosystemId: "ecosystem.the-bonfire", authProvider: AuthProvider.Telegram);

// if (!await ecosystemWallet.IsConnected())
// {
//     _ = await ecosystemWallet.LoginWithOauth(
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
// var ecosystemWalletAddress = await ecosystemWallet.GetAddress();
// Console.WriteLine($"Ecosystem Wallet address: {ecosystemWalletAddress}");

// var ecosystemPersonalSignature = await ecosystemWallet.PersonalSign("Hello, Thirdweb!");
// Console.WriteLine($"Ecosystem Wallet personal sign: {ecosystemPersonalSignature}");
// var isValidPersonal = (await ecosystemWallet.RecoverAddressFromPersonalSign("Hello, Thirdweb!", ecosystemPersonalSignature)) == ecosystemWalletAddress;
// Console.WriteLine($"Ecosystem Wallet personal sign valid: {isValidPersonal}");

// var ecosystemTypedSignature = await ecosystemWallet.SignTypedDataV4(
//     /*lang=json,strict*/
//     "{\"types\": {\"EIP712Domain\": [{\"name\": \"name\",\"type\": \"string\"},{\"name\": \"version\",\"type\": \"string\"},{\"name\": \"chainId\",\"type\": \"uint256\"},{\"name\": \"verifyingContract\",\"type\": \"address\"}],\"Person\": [{\"name\": \"name\",\"type\": \"string\"},{\"name\": \"wallet\",\"type\": \"address\"}],\"Mail\": [{\"name\": \"from\",\"type\": \"Person\"},{\"name\": \"to\",\"type\": \"Person\"},{\"name\": \"contents\",\"type\": \"string\"}]},\"primaryType\": \"Mail\",\"domain\": {\"name\": \"Ether Mail\",\"version\": \"1\",\"chainId\": 1,\"verifyingContract\": \"0xCcCCccccCCCCcCCCCCCcCcCccCcCCCcCcccccccC\"},\"message\": {\"from\": {\"name\": \"Cow\",\"wallet\": \"0xCD2a3d9F938E13CD947Ec05AbC7FE734Df8DD826\"},\"to\": {\"name\": \"Bob\",\"wallet\": \"0xbBbBBBBbbBBBbbbBbbBbbbbBBbBbbbbBbBbbBBbB\"},\"contents\": \"Hello, Bob!\"}}"
// );
// Console.WriteLine($"Ecosystem Wallet typed sign: {ecosystemTypedSignature}");

// var ecosystemWalletOther = await EcosystemWallet.Create(client: client, ecosystemId: "ecosystem.the-bonfire", authProvider: AuthProvider.Telegram);
// var linkedAccounts = await ecosystemWallet.LinkAccount(
//     walletToLink: ecosystemWalletOther,
//     browserOpenAction: (url) =>
//     {
//         var psi = new ProcessStartInfo { FileName = url, UseShellExecute = true };
//         _ = Process.Start(psi);
//     }
// );
// Console.WriteLine($"Linked accounts: {JsonConvert.SerializeObject(linkedAccounts, Formatting.Indented)}");

// var ecosystemSmartWallet = await SmartWallet.Create(ecosystemWallet, 421614);

// var ecosystemTx = await ThirdwebTransaction.Create(wallet: ecosystemSmartWallet, txInput: new ThirdwebTransactionInput(chainId: 421614, to: await ecosystemWallet.GetAddress()));

// var ecosystemTxHash = await ThirdwebTransaction.Send(ecosystemTx);
// Console.WriteLine($"Ecosystem Wallet transaction hash: {ecosystemTxHash}");

#endregion

#region Maximum low level zksync tx

// var chainId = 300;

// var zkRawWallet = await PrivateKeyWallet.Generate(client: client);
// var zkRawAddy = await zkRawWallet.GetAddress();
// Console.WriteLine($"ZkSync raw address: {zkRawAddy}");

// // Less raw example

// var zkRawTx = await ThirdwebTransaction.Create(
//     wallet: zkRawWallet,
//     txInput: new ThirdwebTransactionInput(chainId: chainId, from: zkRawAddy, to: zkRawAddy, value: 0, data: "0x", zkSync: new ZkSyncOptions(gasPerPubdataByteLimit: 50000))
// );

// zkRawTx = await ThirdwebTransaction.Prepare(zkRawTx);

// Console.WriteLine($"ZkSync raw transaction: {zkRawTx}");
// Console.WriteLine("Make sure you have enough funds!");
// Console.ReadLine();

// var receipt = await ThirdwebTransaction.SendAndWaitForTransactionReceipt(zkRawTx);
// Console.WriteLine($"Receipt: {receipt}");

// // Extremely raw example

// var zkRawTx = new Thirdweb.AccountAbstraction.ZkSyncAATransaction
// {
//     TxType = 0x71,
//     From = new HexBigInteger(zkRawAddy).Value,
//     To = new HexBigInteger(zkRawAddy).Value,
//     GasLimit = 250000,
//     GasPerPubdataByteLimit = 50000,
//     MaxFeePerGas = 1000000000,
//     MaxPriorityFeePerGas = 1000000000,
//     Paymaster = 0,
//     Nonce = 0,
//     Value = 0,
//     Data = new byte[] { 0x00 },
//     FactoryDeps = new List<byte[]>(),
//     PaymasterInput = Array.Empty<byte>(),
// };
// var signedZkRawTx = await EIP712.GenerateSignature_ZkSyncTransaction("zkSync", "2", chainId, zkRawTx, zkRawWallet);

// Console.WriteLine($"ZkSync raw transaction: {JsonConvert.SerializeObject(zkRawTx, Formatting.Indented)}");
// Console.WriteLine("Make sure you have enough funds!");
// Console.ReadLine();

// var rpcInstance = ThirdwebRPC.GetRpcInstance(client, chainId);
// var hash = await rpcInstance.SendRequestAsync<string>("eth_sendRawTransaction", signedZkRawTx);
// Console.WriteLine($"Transaction hash: {hash}");

#endregion

#region Guest Login

// var guestWallet = await EcosystemWallet.Create(ecosystemId: "ecosystem.the-bonfire", client: client, authProvider: AuthProvider.Guest);
// if (!await guestWallet.IsConnected())
// {
//     _ = await guestWallet.LoginWithGuest();
// }
// var address = await guestWallet.GetAddress();
// Console.WriteLine($"Guest address: {address}");

// var oldLinkedAccounts = await guestWallet.GetLinkedAccounts();
// Console.WriteLine($"Old linked accounts: {JsonConvert.SerializeObject(oldLinkedAccounts, Formatting.Indented)}");

// var emailWalletFresh = await EcosystemWallet.Create(ecosystemId: "ecosystem.the-bonfire", client: client, email: "firekeeper+guestupgrade5@thirdweb.com");
// _ = await emailWalletFresh.SendOTP();
// Console.WriteLine("Enter OTP:");
// var otp = Console.ReadLine();

// var linkedAccounts = await guestWallet.LinkAccount(walletToLink: emailWalletFresh, otp: otp);
// Console.WriteLine($"Linked accounts: {JsonConvert.SerializeObject(linkedAccounts, Formatting.Indented)}");

#endregion

#region Backend Wallet Auth

// var inAppWalletBackend = await InAppWallet.Create(client: client, authProvider: AuthProvider.Backend, walletSecret: "very-secret");
// if (!await inAppWalletBackend.IsConnected())
// {
//     _ = await inAppWalletBackend.LoginWithBackend();
// }
// var inAppWalletBackendAddress = await inAppWalletBackend.GetAddress();
// Console.WriteLine($"InAppWallet Backend address: {inAppWalletBackendAddress}");

#endregion

#region Account Linking

// var inAppWalletMain = await InAppWallet.Create(client: client, authProvider: AuthProvider.Telegram);
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

// var oldLinkedAccounts = await inAppWalletMain.GetLinkedAccounts();
// Console.WriteLine($"Old linked accounts: {JsonConvert.SerializeObject(oldLinkedAccounts, Formatting.Indented)}");

// // External wallet variant
// var externalWallet = await PrivateKeyWallet.Generate(client: client);
// var inAppWalletToLink = await InAppWallet.Create(client: client, authProvider: AuthProvider.Siwe, siweSigner: externalWallet);
// var linkedAccounts = await inAppWalletMain.LinkAccount(walletToLink: inAppWalletToLink, chainId: 421614);
// Console.WriteLine($"Linked accounts: {JsonConvert.SerializeObject(linkedAccounts, Formatting.Indented)}");

// var unlinkingResult = await inAppWalletMain.UnlinkAccount(linkedAccounts.First(linkedAccounts => linkedAccounts.Type == "siwe"));
// Console.WriteLine($"Unlinking result: {JsonConvert.SerializeObject(unlinkingResult, Formatting.Indented)}");

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

#region TokenPaymaster - Celo CUSD

// var chainId = 42220; // celo

// var erc20SmartWallet = await SmartWallet.Create(personalWallet: privateKeyWallet, chainId: chainId, tokenPaymaster: TokenPaymaster.CELO_CUSD);

// var erc20SmartWalletAddress = await erc20SmartWallet.GetAddress();
// Console.WriteLine($"ERC20 Smart Wallet address: {erc20SmartWalletAddress}");

// var receipt = await erc20SmartWallet.Transfer(chainId: chainId, toAddress: erc20SmartWalletAddress, weiAmount: 0);
// Console.WriteLine($"Receipt: {JsonConvert.SerializeObject(receipt, Formatting.Indented)}");

#endregion

#region TokenPaymaster - Base USDC

// var chainId = 8453; // base

// var erc20SmartWallet = await SmartWallet.Create(personalWallet: privateKeyWallet, chainId: chainId, tokenPaymaster: TokenPaymaster.BASE_USDC);

// var erc20SmartWalletAddress = await erc20SmartWallet.GetAddress();
// Console.WriteLine($"ERC20 Smart Wallet address: {erc20SmartWalletAddress}");

// var receipt = await erc20SmartWallet.Transfer(chainId: chainId, toAddress: erc20SmartWalletAddress, weiAmount: 0);
// Console.WriteLine($"Receipt: {JsonConvert.SerializeObject(receipt, Formatting.Indented)}");

#endregion

#region TokenPaymaster - Lisk LSK

// var chainId = 1135; // lisk

// var erc20SmartWallet = await SmartWallet.Create(personalWallet: privateKeyWallet, chainId: chainId, tokenPaymaster: TokenPaymaster.LISK_LSK);

// var erc20SmartWalletAddress = await erc20SmartWallet.GetAddress();
// Console.WriteLine($"ERC20 Smart Wallet address: {erc20SmartWalletAddress}");

// var receipt = await erc20SmartWallet.Transfer(chainId: chainId, toAddress: erc20SmartWalletAddress, weiAmount: 0);
// Console.WriteLine($"Receipt: {JsonConvert.SerializeObject(receipt, Formatting.Indented)}");

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

// var inAppWalletOAuth = await InAppWallet.Create(client: client, authProvider: AuthProvider.Github);
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

// var inAppWalletAuthDetails = inAppWalletOAuth.GetUserAuthDetails();
// Console.WriteLine($"InAppWallet OAuth auth details: {JsonConvert.SerializeObject(inAppWalletAuthDetails, Formatting.Indented)}");

#endregion

#region InAppWallet - SiweExternal

// var inAppWalletSiweExternal = await InAppWallet.Create(client: client, authProvider: AuthProvider.SiweExternal);
// if (!await inAppWalletSiweExternal.IsConnected())
// {
//     _ = await inAppWalletSiweExternal.LoginWithSiweExternal(
//         isMobile: false,
//         browserOpenAction: (url) =>
//         {
//             var psi = new ProcessStartInfo { FileName = url, UseShellExecute = true };
//             _ = Process.Start(psi);
//         },
//         forceWalletIds: new List<string> { "io.metamask", "com.coinbase.wallet", "xyz.abs" }
//     );
// }
// var inAppWalletOAuthAddress = await inAppWalletSiweExternal.GetAddress();
// Console.WriteLine($"InAppWallet SiweExternal address: {inAppWalletOAuthAddress}");

// var inAppWalletAuthDetails = inAppWalletSiweExternal.GetUserAuthDetails();
// Console.WriteLine($"InAppWallet OAuth auth details: {JsonConvert.SerializeObject(inAppWalletAuthDetails, Formatting.Indented)}");

// await inAppWalletSiweExternal.Disconnect();

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
