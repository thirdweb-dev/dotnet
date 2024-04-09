# Thirdweb .NET SDK

[![codecov](https://codecov.io/gh/thirdweb-dev/thirdweb-dotnet/graph/badge.svg?token=AFF179W07C)](https://codecov.io/gh/thirdweb-dev/thirdweb-dotnet)

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

**Simple Example**

```csharp
// Generate a thirdweb client
var client = new ThirdwebClient(clientId: "myClientId", bundleId: "com.my.bundleid");

// Define a contract
var contract = new ThirdwebContract(client: client, address: "0xBC4CA0EdA7647A8aB7C2061c2E118A18a936f13D", chain: 1, abi: "MyC#EscapedContractABI");

// Read from any contract
var readResult = await ThirdwebContract.ReadContract<string>(contract, "name");
Console.WriteLine($"Contract read result: {readResult}");

// Generate a persistent cross platform EOA to act as a signer
var embeddedAccount = new EmbeddedAccount(client: client, email: "email@email.com"); // or email: null, phoneNumber: "+1234567890"
await embeddedAccount.Connect();
// If no previous session exists
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

// Finally, upgrade that signer to a Smart Account to unlock onchain features such as gasless txs and session keys out of the box
var smartAccount = new SmartAccount(client: client, personalAccount: embeddedAccount, factoryAddress: "mySmartAccountFactory", gasless: true, chainId: 421614);
await smartAccount.Connect();

// Generate a top level wallet for users (wallets may contain multiple accounts, but don't have to)
var thirdwebWallet = new ThirdwebWallet();
await thirdwebWallet.Initialize(new List<IThirdwebAccount> { smartAccount });

// Write to any contract!
var writeResult = await ThirdwebContract.WriteContract(thirdwebWallet, contract, "mintTo", 0, await thirdwebWallet.GetAddress(), 100);
Console.WriteLine($"Contract write result: {writeResult}");

```

**Advanced Example**

```csharp
using Thirdweb;

// Do not use secret keys client side, use client id/bundle id instead
var secretKey = Environment.GetEnvironmentVariable("THIRDWEB_SECRET_KEY");
// Do not use private keys client side, use embedded/smart accounts instead
var privateKey = Environment.GetEnvironmentVariable("PRIVATE_KEY");

// Fetch timeout options are optional, default is 60000ms
var client = new ThirdwebClient(secretKey: secretKey, fetchTimeoutOptions: new TimeoutOptions(storage: 30000, rpc: 60000));

// Access RPC directly if needed, generally not recommended
// var rpc = ThirdwebRPC.GetRpcInstance(client, 421614);
// var blockNumber = await rpc.SendRequestAsync<string>("eth_blockNumber");
// Console.WriteLine($"Block number: {blockNumber}");

// Interact with a contract
var contract = new ThirdwebContract(client: client, address: "0xBC4CA0EdA7647A8aB7C2061c2E118A18a936f13D", chain: 1, abi: "MyC#EscapedContractABI");
var readResult = await ThirdwebContract.ReadContract<string>(contract, "name");
Console.WriteLine($"Contract read result: {readResult}");

// Create accounts (this is an advanced use case, typically one account is plenty)
var privateKeyAccount = new PrivateKeyAccount(client: client, privateKeyHex: privateKey);
var embeddedAccount = new EmbeddedAccount(client: client, email: "firekeeper+7121271d@thirdweb.com"); // or email: null, phoneNumber: "+1234567890"
var smartAccount = new SmartAccount(client: client, personalAccount: embeddedAccount, factoryAddress: "0xbf1C9aA4B1A085f7DA890a44E82B0A1289A40052", gasless: true, chainId: 421614);

// Attempt to connect pk accounts
await privateKeyAccount.Connect();
await embeddedAccount.Connect();

// Reset embedded account (optional step for testing login flow)
if (await embeddedAccount.IsConnected())
{
    await embeddedAccount.Disconnect();
}

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

// Connect the smart account with embedded signer and grant a session key to pk account (advanced use case)
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

// Reconnect to same smart account with pk account as signer (specifying account address override)
smartAccount = new SmartAccount(
    client: client,
    personalAccount: privateKeyAccount,
    factoryAddress: "0xbf1C9aA4B1A085f7DA890a44E82B0A1289A40052",
    gasless: true,
    chainId: 421614,
    accountAddressOverride: await smartAccount.GetAddress()
);
await smartAccount.Connect();

// Log addresses
Console.WriteLine($"PrivateKey Account: {await privateKeyAccount.GetAddress()}");
Console.WriteLine($"Embedded Account: {await embeddedAccount.GetAddress()}");
Console.WriteLine($"Smart Account: {await smartAccount.GetAddress()}");

// Initialize wallet (a wallet can hold multiple accounts, but only one can be active at a time)
var thirdwebWallet = new ThirdwebWallet();
await thirdwebWallet.Initialize(new List<IThirdwebAccount> { privateKeyAccount, embeddedAccount, smartAccount });
thirdwebWallet.SetActive(await smartAccount.GetAddress());
Console.WriteLine($"Active account: {await thirdwebWallet.GetAddress()}");

// Sign, triggering deploy as needed and 1271 verification if it's a smart wallet
var message = "Hello, Thirdweb!";
var signature = await thirdwebWallet.PersonalSign(message);
Console.WriteLine($"Signed message: {signature}");

var balanceBefore = await ThirdwebContract.ReadContract<BigInteger>(contract, "balanceOf", await thirdwebWallet.GetAddress());
Console.WriteLine($"Balance before mint: {balanceBefore}");

var writeResult = await ThirdwebContract.WriteContract(thirdwebWallet, contract, "mintTo", 0, await thirdwebWallet.GetAddress(), 100);
Console.WriteLine($"Contract write result: {writeResult}");

var balanceAfter = await ThirdwebContract.ReadContract<BigInteger>(contract, "balanceOf", await thirdwebWallet.GetAddress());
Console.WriteLine($"Balance after mint: {balanceAfter}");

// Storage actions

// // Will download from IPFS or normal urls
// var downloadResult = await ThirdwebStorage.Download<string>(client: client, uri: "AnyUrlIncludingIpfs");
// Console.WriteLine($"Download result: {downloadResult}");

// // Will upload to IPFS
// var uploadResult = await ThirdwebStorage.Upload(client: client, path: "AnyPath");
// Console.WriteLine($"Upload result preview: {uploadResult.PreviewUrl}");
```
