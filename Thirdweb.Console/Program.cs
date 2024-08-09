﻿using Thirdweb;
using dotenv.net;
using System.Numerics;

DotEnv.Load();

var secretKey = Environment.GetEnvironmentVariable("THIRDWEB_SECRET_KEY");

var client = ThirdwebClient.Create(secretKey: secretKey);

// dotnet run --project Thirdweb.Console [chainId] [optionalGasLimitOverride]
var cmd = Environment.GetCommandLineArgs();
var chainId = BigInteger.Parse(cmd[1]);
BigInteger? gasLimitOverride = cmd.Length > 2 ? BigInteger.Parse(cmd[2]) : null;

try
{
    var receipt = await Utils.DeployEntryPoint(client, chainId, gasLimitOverride);
    Console.WriteLine($"Deployed entry point to chain {chainId} with receipt {receipt}");
}
catch (Exception e)
{
    Console.WriteLine($"Failed to deploy entry point on chain {chainId}:\n{e.Message}");
}
