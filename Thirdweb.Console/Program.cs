#pragma warning disable IDE0005
#pragma warning disable IDE0059

using System.Diagnostics;
using System.Numerics;
using System.Text;
using dotenv.net;
using Nethereum.ABI;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Thirdweb;
using Thirdweb.AccountAbstraction;
using Thirdweb.AI;
using Thirdweb.Indexer;
using Thirdweb.Pay;

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

// var contract = await ThirdwebContract.Create(client: client, address: "0xbc4ca0eda7647a8ab7c2061c2e118a18a936f13d", chain: 1);
// var nfts = await contract.ERC721_GetAllNFTs();
// Console.WriteLine($"NFTs: {JsonConvert.SerializeObject(nfts, Formatting.Indented)}");

#endregion

#region Indexer

// // Create a ThirdwebInsight instance
// var insight = await ThirdwebInsight.Create(client);

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

// // --------------------------------------------------------------------------
// // Configuration
// // --------------------------------------------------------------------------

// var chainWith7702 = 911867;
// var delegationContractAddress = "0xb012446cba783d0f7723daf96cf4c49005022307"; // MinimalAccount

// // Required environment variables
// var backendWalletAddress = Environment.GetEnvironmentVariable("ENGINE_BACKEND_WALLET_ADDRESS") ?? throw new Exception("ENGINE_BACKEND_WALLET_ADDRESS is required");
// var engineUrl = Environment.GetEnvironmentVariable("ENGINE_URL") ?? throw new Exception("ENGINE_URL is required");
// var engineAccessToken = Environment.GetEnvironmentVariable("ENGINE_ACCESS_TOKEN") ?? throw new Exception("ENGINE_ACCESS_TOKEN is required");

// // --------------------------------------------------------------------------
// // Initialize Engine Wallet
// // --------------------------------------------------------------------------

// var engineWallet = await EngineWallet.Create(client, engineUrl, engineAccessToken, backendWalletAddress, 15);

// // --------------------------------------------------------------------------
// // Delegation Contract Implementation
// // --------------------------------------------------------------------------

// var delegationContract = await ThirdwebContract.Create(client, delegationContractAddress, chainWith7702);

// // Initialize a (to-be) 7702 EOA
// var eoaWallet = await PrivateKeyWallet.Generate(client);
// var eoaWalletAddress = await eoaWallet.GetAddress();
// Console.WriteLine($"EOA address: {eoaWalletAddress}");

// // Sign the authorization to point to the delegation contract
// var authorization = await eoaWallet.SignAuthorization(chainWith7702, delegationContractAddress, willSelfExecute: false);
// Console.WriteLine($"Authorization: {JsonConvert.SerializeObject(authorization, Formatting.Indented)}");

// // Sign message for session key
// var sessionKeyParams = new SessionKeyParams_7702()
// {
//     Signer = backendWalletAddress,
//     NativeTokenLimitPerTransaction = 0,
//     StartTimestamp = 0,
//     EndTimestamp = Utils.GetUnixTimeStampNow() + (3600 * 24),
//     ApprovedTargets = new List<string> { Constants.ADDRESS_ZERO },
//     Uid = Guid.NewGuid().ToByteArray()
// };
// var sessionKeySig = await EIP712.GenerateSignature_SmartAccount_7702("MinimalAccount", "1", chainWith7702, eoaWalletAddress, sessionKeyParams, eoaWallet);

// // Create call data for the session key
// var sessionKeyCallData = delegationContract.CreateCallData("createSessionKeyWithSig", sessionKeyParams, sessionKeySig.HexToBytes());

// // Execute the delegation & session key creation in one go, from the backend!
// var delegationReceipt = await engineWallet.ExecuteTransaction(new ThirdwebTransactionInput(chainId: chainWith7702, to: eoaWalletAddress, data: sessionKeyCallData, authorization: authorization));
// Console.WriteLine($"Delegation Execution Receipt: {JsonConvert.SerializeObject(delegationReceipt, Formatting.Indented)}");

// // Verify contract code deployed to the EOA
// var rpc = ThirdwebRPC.GetRpcInstance(client, chainWith7702);
// var code = await rpc.SendRequestAsync<string>("eth_getCode", eoaWalletAddress, "latest");
// Console.WriteLine($"EOA code: {code}");

// // The EOA is now a contract
// var eoaContract = await ThirdwebContract.Create(client, eoaWalletAddress, chainWith7702, delegationContract.Abi);

// // --------------------------------------------------------------------------
// // Mint Tokens (DropERC20) to the EOA Using the backend session key
// // --------------------------------------------------------------------------

// var erc20ContractAddress = "0xAA462a5BE0fc5214507FDB4fB2474a7d5c69065b"; // DropERC20
// var erc20Contract = await ThirdwebContract.Create(client, erc20ContractAddress, chainWith7702);

// // Log ERC20 balance before mint
// var eoaBalanceBefore = await erc20Contract.ERC20_BalanceOf(eoaWalletAddress);
// Console.WriteLine($"EOA balance before: {eoaBalanceBefore}");

// // Create execution call data (calling 'claim' on the DropERC20)
// var executeCallData = eoaContract.CreateCallData(
//     "execute",
//     new object[]
//     {
//         new List<Call>
//         {
//             new()
//             {
//                 Data = erc20Contract
//                     .CreateCallData(
//                         "claim",
//                         new object[]
//                         {
//                             eoaWalletAddress, // receiver
//                             100, // quantity
//                             Constants.NATIVE_TOKEN_ADDRESS, // currency
//                             0, // pricePerToken
//                             new object[] { Array.Empty<byte>(), BigInteger.Zero, BigInteger.Zero, Constants.ADDRESS_ZERO }, // allowlistProof
//                             Array.Empty<byte>() // data
//                         }
//                     )
//                     .HexToBytes(),
//                 To = erc20ContractAddress,
//                 Value = BigInteger.Zero
//             }
//         }
//     }
// );

// var executeReceipt = await engineWallet.ExecuteTransaction(new ThirdwebTransactionInput(chainId: chainWith7702, to: eoaWalletAddress, data: executeCallData));
// Console.WriteLine($"Execute receipt: {JsonConvert.SerializeObject(executeReceipt, Formatting.Indented)}");

// // Log ERC20 balance after mint
// var eoaBalanceAfter = await erc20Contract.ERC20_BalanceOf(eoaWalletAddress);
// Console.WriteLine($"EOA balance after: {eoaBalanceAfter}");

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

#region Buy with Fiat

// // Supported currencies
// var supportedCurrencies = await ThirdwebPay.GetBuyWithFiatCurrencies(client);
// Console.WriteLine($"Supported currencies: {JsonConvert.SerializeObject(supportedCurrencies, Formatting.Indented)}");

// // Get a Buy with Fiat quote
// var fiatQuoteParamsWithProvider = new BuyWithFiatQuoteParams(fromCurrencySymbol: "USD", toAddress: walletAddress, toChainId: "137", toTokenAddress: Constants.NATIVE_TOKEN_ADDRESS, toAmount: "20", preferredProvider: "STRIPE");
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
