using dotenv.net;
using Newtonsoft.Json;
using Thirdweb;

DotEnv.Load();

var secretKey = Environment.GetEnvironmentVariable("THIRDWEB_SECRET_KEY");

var clientOptions = new ThirdwebClientOptions(secretKey: secretKey, fetchTimeoutOptions: new TimeoutOptions(storage: 30000, rpc: 10000));
var client = new ThirdwebClient(clientOptions);

Console.WriteLine($"Initialized ThirdwebClient: {JsonConvert.SerializeObject(clientOptions, Formatting.Indented)}");

var rpc = ThirdwebRPC.GetRpcInstance(client, 1);
var blockNumber = await rpc.SendRequestAsync<string>("eth_blockNumber");
Console.WriteLine($"Block number: {blockNumber}");

var contractOptions = new ThirdwebContractOptions(client: client, address: "0xBC4CA0EdA7647A8aB7C2061c2E118A18a936f13D", chain: 1, abi: "function name() view returns (string)");
var contract = new ThirdwebContract(contractOptions);
var readResult = await ThirdwebContract.ReadContract<string>(contract, "name");

Console.WriteLine($"Contract read result: {readResult}");

Console.ReadLine();
