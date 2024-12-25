using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Thirdweb.Console;

public class Call
{
    [Parameter("bytes", "data", 1)]
    public required byte[] Data { get; set; }

    [Parameter("address", "to", 2)]
    public required string To { get; set; }

    [Parameter("uint256", "value", 3)]
    public required BigInteger Value { get; set; }
}
