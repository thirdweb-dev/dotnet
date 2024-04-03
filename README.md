# Thirdweb .NET SDK

## Overview

The Thirdweb .NET SDK is a powerful library that allows developers to interact with the blockchain using the .NET framework.
It provides a set of convenient methods and classes to simplify the integration of Web3 functionality into your .NET applications.

## Features

- Connect to any EVM network
- Query blockchain data using Thirdweb RPC
- Interact with smart contracts

## Installation

To use the Thirdweb .NET SDK in your project, you can either download the source code and build it manually, or install it via NuGet package manager.

```
dotnet add package Thirdweb
```

## Usage

```csharp
using Thirdweb;

// Create a client
var client = new ThirdwebClient(secretKey: secretKey);

// Interact with a contract
var contract = new ThirdwebContract(client: client, address: "0xBC4CA0EdA7647A8aB7C2061c2E118A18a936f13D", chain: 1, abi: "function name() view returns (string)");
var readResult = await ThirdwebContract.ReadContract<string>(contract, "name");
Console.WriteLine($"Contract read result: {readResult}");

// Or directly interact with the RPC
var rpc = ThirdwebRPC.GetRpcInstance(client, 1);
var blockNumber = await rpc.SendRequestAsync<string>("eth_blockNumber");
Console.WriteLine($"Block number: {blockNumber}");

// Create accounts
var privateKeyAccount = new PrivateKeyAccount(client, privateKey);
var embeddedAccount = new EmbeddedAccount(client, "myawesomeemail@gmail.com");
var smartAccount = new SmartAccount(client, embeddedAccount, "0xbf1C9aA4B1A085f7DA890a44E82B0A1289A40052", true, 421614);

// Attempt to connect pk accounts
await privateKeyAccount.Connect();
await embeddedAccount.Connect();

// Relog if embedded account not logged in
if (!await embeddedAccount.IsConnected())
{
    await embeddedAccount.SendOTP();
    Console.WriteLine("Please submit the OTP.");
    var otp = Console.ReadLine();
    (var embeddedAccountAddress, var canRetry) = await embeddedAccount.SubmitOTP(otp);
    if (embeddedAccountAddress == null && canRetry)
    {
        Console.WriteLine("Please submit the OTP again.");
        otp = Console.ReadLine();
        (embeddedAccountAddress, _) = await embeddedAccount.SubmitOTP(otp);
    }
    if (embeddedAccountAddress == null)
    {
        Console.WriteLine("OTP login failed. Please try again.");
        return;
    }
}

// Connect the smart account with embedded signer and grant a session key to pk account
await smartAccount.Connect();
_ = await smartAccount.CreateSessionKey(
    signerAddress: await privateKeyAccount.GetAddress(),
    approvedTargets: new List<string>() { Constants.ADDRESS_ZERO },
    nativeTokenLimitPerTransactionInWei: "0",
    permissionStartTimestamp: "0",
    permissionEndTimestamp: (Utils.GetUnixTimeStampNow() + 86400).ToString(),
    reqValidityStartTimestamp: "0",
    reqValidityEndTimestamp: Utils.GetUnixTimeStampIn10Years().ToString()
);

// Reconnect to same smart account with pk account as signer
smartAccount = new SmartAccount(client, privateKeyAccount, "0xbf1C9aA4B1A085f7DA890a44E82B0A1289A40052", true, 421614, await smartAccount.GetAddress());
await smartAccount.Connect();

// Log addresses
Console.WriteLine($"PrivateKey Account: {await privateKeyAccount.GetAddress()}");
Console.WriteLine($"Embedded Account: {await embeddedAccount.GetAddress()}");
Console.WriteLine($"Smart Account: {await smartAccount.GetAddress()}");

// Initialize wallet
var thirdwebWallet = new ThirdwebWallet();
await thirdwebWallet.Initialize(new List<IThirdwebAccount> { privateKeyAccount, embeddedAccount, smartAccount });
thirdwebWallet.SetActive(await smartAccount.GetAddress());
Console.WriteLine($"Active account: {await thirdwebWallet.GetAddress()}");

// Sign, triggering deploy as needed and 1271 verification
var message = "Hello, Thirdweb!";
var signature = await thirdwebWallet.PersonalSign(message);
Console.WriteLine($"Signed message: {signature}");
```
