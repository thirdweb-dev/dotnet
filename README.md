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
var clientOptions = new ThirdwebClientOptions(secretKey: secretKey);
var client = new ThirdwebClient(clientOptions);

// Interact with a contract
var contractOptions = new ThirdwebContractOptions(client: client, address: "0xBC4CA0EdA7647A8aB7C2061c2E118A18a936f13D", chain: 1, abi: "function name() view returns (string)");
var contract = new ThirdwebContract(contractOptions);
var readResult = await ThirdwebContract.ReadContract<string>(contract, "name");
Console.WriteLine($"Contract read result: {readResult}");

// Or directly interact with the RPC
var rpc = ThirdwebRPC.GetRpcInstance(client, 1);
var blockNumber = await rpc.SendRequestAsync<string>("eth_blockNumber");
Console.WriteLine($"Block number: {blockNumber}");
```
