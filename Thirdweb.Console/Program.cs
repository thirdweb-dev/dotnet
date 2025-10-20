#pragma warning disable IDE0005
#pragma warning disable IDE0059

using System.Diagnostics;
using dotenv.net;
using Newtonsoft.Json;
using Thirdweb;

DotEnv.Load();

// Do not use secret keys client side, use client id/bundle id instead
var secretKey = Environment.GetEnvironmentVariable("THIRDWEB_SECRET_KEY");

// Fetch timeout options are optional, default is 120000ms
var client = ThirdwebClient.Create(secretKey: secretKey);

#region Signing Messages

//  Create a guest wallet
var guestWallet = await InAppWallet.Create(client, authProvider: AuthProvider.Guest);
var walletAddress = await guestWallet.LoginWithGuest();
Console.WriteLine($"Guest Wallet address: {walletAddress}");

var signature = await guestWallet.PersonalSign("Hello, Thirdweb!");
Console.WriteLine($"Guest Wallet personal sign: {signature}");

#endregion

#region Reading from Contracts

// var contract = await ThirdwebContract.Create(client: client, address: "0xbc4ca0eda7647a8ab7c2061c2e118a18a936f13d", chain: 1);
// var nfts = await contract.ERC721_GetNFT(0);
// Console.WriteLine($"NFTs: {JsonConvert.SerializeObject(nfts, Formatting.Indented)}");

#endregion

#region User Wallets (Social Auth Example)

// var inAppWalletOAuth = await InAppWallet.Create(client: client, authProvider: AuthProvider.Google);
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

#region Server Wallets

// // ServerWallet is compatible with IThirdwebWallet and can be used with any SDK method/extension
// var serverWallet = await ServerWallet.Create(
//     client: client,
//     label: "Test",
//     // Optional, defaults to Auto - we choose between EIP-7702, EIP-4337 or native zkSync AA execution / EOA is also available
//     executionOptions: new AutoExecutionOptions()
// );

// var serverWalletAddress = await serverWallet.GetAddress();
// Console.WriteLine($"Server Wallet address: {serverWalletAddress}");

// var serverWalletPersonalSig = await serverWallet.PersonalSign("Hello, Thirdweb!");
// Console.WriteLine($"Server Wallet personal sign: {serverWalletPersonalSig}");

// var json =
//     /*lang=json,strict*/
//     "{\"types\":{\"EIP712Domain\":[{\"name\":\"name\",\"type\":\"string\"},{\"name\":\"version\",\"type\":\"string\"},{\"name\":\"chainId\",\"type\":\"uint256\"},{\"name\":\"verifyingContract\",\"type\":\"address\"}],\"Person\":[{\"name\":\"name\",\"type\":\"string\"},{\"name\":\"wallet\",\"type\":\"address\"}],\"Mail\":[{\"name\":\"from\",\"type\":\"Person\"},{\"name\":\"to\",\"type\":\"Person\"},{\"name\":\"contents\",\"type\":\"string\"}]},\"primaryType\":\"Mail\",\"domain\":{\"name\":\"Ether Mail\",\"version\":\"1\",\"chainId\":84532,\"verifyingContract\":\"0xCcCCccccCCCCcCCCCCCcCcCccCcCCCcCcccccccC\"},\"message\":{\"from\":{\"name\":\"Cow\",\"wallet\":\"0xCD2a3d9F938E13CD947Ec05AbC7FE734Df8DD826\"},\"to\":{\"name\":\"Bob\",\"wallet\":\"0xbBbBBBBbbBBBbbbBbbBbbBBbBbbBbBbBbBbbBBbB\"},\"contents\":\"Hello, Bob!\"}}";
// var serverWalletTypedDataSign = await serverWallet.SignTypedDataV4(json);
// Console.WriteLine($"Server Wallet typed data sign: {serverWalletTypedDataSign}");

// // Simple self transfer
// var serverWalletReceipt = await serverWallet.Transfer(chainId: 84532, toAddress: await serverWallet.GetAddress(), weiAmount: 0);
// Console.WriteLine($"Server Wallet Hash: {serverWalletReceipt.TransactionHash}");

// // ServerWallet forcing ERC-4337 Execution Mode
// var smartServerWallet = await ServerWallet.Create(client: client, label: "Test", executionOptions: new ERC4337ExecutionOptions(chainId: 84532, signerAddress: serverWalletAddress));
// var smartServerWalletAddress = await smartServerWallet.GetAddress();
// Console.WriteLine($"Smart Server Wallet address: {smartServerWalletAddress}");

// var smartServerWalletPersonalSig = await smartServerWallet.PersonalSign("Hello, Thirdweb!");
// Console.WriteLine($"Smart Server Wallet personal sign: {smartServerWalletPersonalSig}");

// var smartServerWalletTypedDataSign = await smartServerWallet.SignTypedDataV4(json);
// Console.WriteLine($"Smart Server Wallet typed data sign: {smartServerWalletTypedDataSign}");

// // Simple self transfer
// var smartServerWalletReceipt = await smartServerWallet.Transfer(chainId: 84532, toAddress: await smartServerWallet.GetAddress(), weiAmount: 0);
// Console.WriteLine($"Server Wallet Hash: {smartServerWalletReceipt.TransactionHash}");

#endregion

#region Thirdweb API Wrapper

// var metadata = await client.Api.GetContractMetadataAsync(chainId: 1, address: "0xBd3531dA5CF5857e7CfAA92426877b022e612cf8");

// Console.WriteLine($"ABI: {JsonConvert.SerializeObject(metadata.Result.Output.Abi, Formatting.Indented)}");
// Console.WriteLine($"Compiler version: {metadata.Result.Compiler.Version}");

#endregion

#region AA 7702

// var chain = 84532; // 7702-compatible chain

// // Connect to EOA
// var smartEoa = await InAppWallet.Create(client, authProvider: AuthProvider.Guest, executionMode: ExecutionMode.EIP7702Sponsored);
// if (!await smartEoa.IsConnected())
// {
//     _ = await smartEoa.LoginWithGuest(defaultSessionIdOverride: new Guid().ToString());
// }
// var smartEoaAddress = await smartEoa.GetAddress();
// Console.WriteLine($"User Wallet address: {await smartEoa.GetAddress()}");

// // Transact, will upgrade EOA
// var receipt = await smartEoa.Transfer(chainId: chain, toAddress: await Utils.GetAddressFromENS(client, "vitalik.eth"), weiAmount: 0);
// Console.WriteLine($"Transfer Receipt: {receipt.TransactionHash}");

#endregion

#region AA 0.6

// var smartWallet06 = await SmartWallet.Create(personalWallet: guestWallet, chainId: 421614, gasless: true);
// var receipt06 = await smartWallet06.Transfer(chainId: 421614, toAddress: await smartWallet06.GetAddress(), weiAmount: 0);
// Console.WriteLine($"Receipt: {receipt06}");

#endregion

#region AA 0.7

// var smartWallet07 = await SmartWallet.Create(personalWallet: guestWallet, chainId: 421614, gasless: true, entryPoint: Constants.ENTRYPOINT_ADDRESS_V07);
// var receipt07 = await smartWallet07.Transfer(chainId: 421614, toAddress: await smartWallet07.GetAddress(), weiAmount: 0);
// Console.WriteLine($"Receipt: {receipt07}");

#endregion

#region AA ZkSync

// var zkSmartWallet = await SmartWallet.Create(personalWallet: privateKeyWallet, chainId: 11124, gasless: true);

// var hash = await zkSmartWallet.SendTransaction(new ThirdwebTransactionInput(chainId: 11124, to: await zkSmartWallet.GetAddress(), value: 0, data: "0x"));

// Console.WriteLine($"Transaction hash: {hash}");

#endregion

#region Deploy Contract

// var serverWallet = await ServerWallet.Create(client: client, label: "TestFromDotnet");

// var abi =
//     "[ { \"inputs\": [], \"name\": \"welcome\", \"outputs\": [ { \"internalType\": \"string\", \"name\": \"\", \"type\": \"string\" } ], \"stateMutability\": \"pure\", \"type\": \"function\" } ]";

// var contractAddress = await ThirdwebContract.Deploy(
//     client: client,
//     chainId: 11155111,
//     serverWalletAddress: await serverWallet.GetAddress(),
//     bytecode: "6080604052348015600e575f5ffd5b5061014e8061001c5f395ff3fe608060405234801561000f575f5ffd5b5060043610610029575f3560e01c8063b627cf3b1461002d575b5f5ffd5b61003561004b565b60405161004291906100f8565b60405180910390f35b60606040518060400160405280601481526020017f57656c636f6d6520746f20746869726477656221000000000000000000000000815250905090565b5f81519050919050565b5f82825260208201905092915050565b8281835e5f83830152505050565b5f601f19601f8301169050919050565b5f6100ca82610088565b6100d48185610092565b93506100e48185602086016100a2565b6100ed816100b0565b840191505092915050565b5f6020820190508181035f83015261011081846100c0565b90509291505056fea264697066735822122001498e9d7d6125ce22613ef32fdb7e8e03bf11ad361d7b00e210b82d7b7e0d4464736f6c634300081e0033",
//     abi: abi
// );
// Console.WriteLine($"Contract deployed at: {contractAddress}");

// var contract = await ThirdwebContract.Create(client: client, address: contractAddress, chain: 11155111, abi: abi);
// var welcomeMessage = await contract.Read<string>("welcome");
// Console.WriteLine($"Welcome message from deployed contract: {welcomeMessage}");

#endregion

#region Get Social Profiles

// var socialProfiles = await Utils.GetSocialProfiles(client, "joenrv.eth");
// Console.WriteLine($"Social Profiles: {socialProfiles}");

#endregion

#region EIP-7702 (Low Level)

// var chain = 42220; // 7702-compatible chain

// // Connect to EOA
// var smartEoa = await InAppWallet.Create(client, authProvider: AuthProvider.Guest, executionMode: ExecutionMode.EIP7702Sponsored);
// if (!await smartEoa.IsConnected())
// {
//     _ = await smartEoa.LoginWithGuest(defaultSessionIdOverride: new Guid().ToString());
// }
// var smartEoaAddress = await smartEoa.GetAddress();
// Console.WriteLine($"User Wallet address: {await smartEoa.GetAddress()}");

// // Upgrade EOA - This wallet explicitly uses EIP-7702 delegation to the thirdweb MinimalAccount (will delegate upon first tx)
// var signerAddress = await Utils.GetAddressFromENS(client, "vitalik.eth");

// // Transact, will upgrade EOA
// var receipt = await smartEoa.Transfer(chainId: chain, toAddress: signerAddress, weiAmount: 0);
// Console.WriteLine($"Transfer Receipt: {receipt.TransactionHash}");

// // Double check that it was upgraded
// var isDelegated = await Utils.IsDeployed(client, chain, smartEoaAddress);
// Console.WriteLine($"Is delegated: {isDelegated}");

// // Create a session key
// var sessionKeyReceipt = await smartEoa.CreateSessionKey(chainId: chain, signerAddress: signerAddress, durationInSeconds: 86400, grantFullPermissions: true);
// Console.WriteLine($"Session key receipt: {sessionKeyReceipt.TransactionHash}");

// // Validate session key config
// var hasFullPermissions = await smartEoa.SignerHasFullPermissions(chain, signerAddress);
// Console.WriteLine($"Signer has full permissions: {hasFullPermissions}");

// var sessionExpiration = await smartEoa.GetSessionExpirationForSigner(chain, signerAddress);
// Console.WriteLine($"Session expires in {sessionExpiration - Utils.GetUnixTimeStampNow()} seconds");

// // Create a session key with granular permissions
// var granularSessionKeyReceipt = await smartEoa.CreateSessionKey(
//     chainId: chain,
//     signerAddress: signerAddress,
//     durationInSeconds: 86400,
//     grantFullPermissions: false,
//     transferPolicies: new List<TransferSpec>
//     {
//         new()
//         {
//             Target = signerAddress,
//             MaxValuePerUse = BigInteger.Parse("0.001".ToWei()),
//             ValueLimit = new UsageLimit
//             {
//                 LimitType = 1, // Lifetime
//                 Limit = BigInteger.Parse("0.01".ToWei()),
//                 Period = 86400, // 1 day
//             }
//         }
//     }
// );

// // Validate session key config
// var sessionState = await smartEoa.GetSessionStateForSigner(chain, signerAddress);
// Console.WriteLine($"Session state: {JsonConvert.SerializeObject(sessionState, Formatting.Indented)}");

// var transferPolcies = await smartEoa.GetTransferPoliciesForSigner(chain, signerAddress);
// Console.WriteLine($"Transfer policies: {JsonConvert.SerializeObject(transferPolcies, Formatting.Indented)}");

// var callPolicies = await smartEoa.GetCallPoliciesForSigner(chain, signerAddress);
// Console.WriteLine($"Call policies: {JsonConvert.SerializeObject(callPolicies, Formatting.Indented)}");

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
