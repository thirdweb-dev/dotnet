using System.Numerics;
using Nethereum.ABI;
using Nethereum.Util;

namespace Thirdweb;

/// <summary>
/// Provides utilities for Merkle tree operations compatible with OpenZeppelin and Thirdweb standards.
/// </summary>
public static class MerkleTreeUtils
{
    private static readonly BigInteger _maxUint256 = BigInteger.Parse("115792089237316195423570985008687907853269984665640564039457584007913129639935");

    /// <summary>
    /// Computes the Thirdweb-compatible leaf hash for a whitelist entry.
    /// Format: keccak256(encodePacked(address, uint256 maxClaimable, uint256 price, address currency))
    /// </summary>
    /// <param name="entry">The whitelist entry to hash.</param>
    /// <returns>The 32-byte keccak256 hash of the encoded entry.</returns>
    public static byte[] HashLeaf(WhitelistEntry entry)
    {
        var address = entry.Address.ToLower();

        // maxClaimable: "unlimited" or missing = MAX_UINT256
        var maxClaimable = string.IsNullOrEmpty(entry.MaxClaimable) || string.Equals(entry.MaxClaimable, "unlimited", StringComparison.OrdinalIgnoreCase)
            ? _maxUint256
            : BigInteger.Parse(entry.MaxClaimable);

        // price: "unlimited" or missing = MAX_UINT256
        var price = string.IsNullOrEmpty(entry.Price) || string.Equals(entry.Price, "unlimited", StringComparison.OrdinalIgnoreCase)
            ? _maxUint256
            : BigInteger.Parse(entry.Price);

        // currency: missing = zero address
        var currency = string.IsNullOrEmpty(entry.CurrencyAddress)
            ? Constants.ADDRESS_ZERO
            : entry.CurrencyAddress.ToLower();

        // ABI encode packed
        var abiEncode = new ABIEncode();
        var packed = abiEncode.GetABIEncodedPacked(
            new ABIValue("address", address),
            new ABIValue("uint256", maxClaimable),
            new ABIValue("uint256", price),
            new ABIValue("address", currency)
        );

        // keccak256
        return Sha3Keccack.Current.CalculateHash(packed);
    }

    /// <summary>
    /// Compares two byte arrays lexicographically (for OpenZeppelin-compatible sorting).
    /// </summary>
    private static int CompareBytes(byte[] a, byte[] b)
    {
        for (var i = 0; i < Math.Min(a.Length, b.Length); i++)
        {
            if (a[i] < b[i])
            {
                return -1;
            }

            if (a[i] > b[i])
            {
                return 1;
            }
        }

        return a.Length.CompareTo(b.Length);
    }

    /// <summary>
    /// Hashes a pair of nodes in sorted order (OpenZeppelin standard).
    /// </summary>
    private static byte[] HashPair(byte[] a, byte[] b)
    {
        // Sort: smaller first
        var (first, second) = CompareBytes(a, b) < 0 ? (a, b) : (b, a);

        // Concatenate and hash
        var combined = new byte[first.Length + second.Length];
        Buffer.BlockCopy(first, 0, combined, 0, first.Length);
        Buffer.BlockCopy(second, 0, combined, first.Length, second.Length);

        return Sha3Keccack.Current.CalculateHash(combined);
    }

    /// <summary>
    /// Calculates the full Merkle proof for a claimer address from shard data.
    /// </summary>
    /// <param name="shardData">The shard data containing entries and shard proofs.</param>
    /// <param name="claimerAddress">The address to get the proof for.</param>
    /// <returns>The allowlist proof with full Merkle proof, or null if claimer not found.</returns>
    public static AllowlistProof CalculateMerkleProof(ShardData shardData, string claimerAddress)
    {
        if (shardData?.Entries == null || shardData.Entries.Count == 0)
        {
            return null;
        }

        var normalizedAddress = claimerAddress.ToLower();

        // Find the claimer's entry
        var claimerEntry = shardData.Entries.Find(e => string.Equals(e.Address, normalizedAddress, StringComparison.OrdinalIgnoreCase));
        if (claimerEntry == null)
        {
            return null;
        }

        // Hash all entries
        var leaves = new List<byte[]>();
        var leafToEntryMap = new Dictionary<string, WhitelistEntry>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in shardData.Entries)
        {
            var leaf = HashLeaf(entry);
            leaves.Add(leaf);
            leafToEntryMap[leaf.BytesToHex()] = entry;
        }

        // Sort leaves (OpenZeppelin standard)
        leaves.Sort(CompareBytes);

        // Build tree layers
        var layers = new List<List<byte[]>> { leaves };
        var currentLayer = leaves;

        while (currentLayer.Count > 1)
        {
            var nextLayer = new List<byte[]>();

            for (var i = 0; i < currentLayer.Count; i += 2)
            {
                if (i + 1 == currentLayer.Count)
                {
                    // Odd node, promote to next layer
                    nextLayer.Add(currentLayer[i]);
                }
                else
                {
                    // Hash pair
                    nextLayer.Add(HashPair(currentLayer[i], currentLayer[i + 1]));
                }
            }

            layers.Add(nextLayer);
            currentLayer = nextLayer;
        }

        // Get claimer's leaf
        var claimerLeaf = HashLeaf(claimerEntry);
        var claimerLeafHex = claimerLeaf.BytesToHex();

        // Find leaf index in sorted leaves
        var leafIndex = -1;
        for (var i = 0; i < leaves.Count; i++)
        {
            if (string.Equals(leaves[i].BytesToHex(), claimerLeafHex, StringComparison.OrdinalIgnoreCase))
            {
                leafIndex = i;
                break;
            }
        }

        if (leafIndex == -1)
        {
            return null;
        }

        // Build mini-proof
        var miniProof = new List<byte[]>();
        var index = leafIndex;

        for (var layerIdx = 0; layerIdx < layers.Count - 1; layerIdx++)
        {
            var layer = layers[layerIdx];
            var isRightNode = index % 2 == 1;
            var pairIndex = isRightNode ? index - 1 : index + 1;

            if (pairIndex < layer.Count)
            {
                miniProof.Add(layer[pairIndex]);
            }

            index /= 2;
        }

        // Combine mini-proof with shard proofs
        var fullProof = new List<byte[]>(miniProof);

        if (shardData.Proofs != null)
        {
            foreach (var proofHex in shardData.Proofs)
            {
                fullProof.Add(proofHex.HexToBytes());
            }
        }

        // Parse entry values for AllowlistProof
        var maxClaimable = string.IsNullOrEmpty(claimerEntry.MaxClaimable) || string.Equals(claimerEntry.MaxClaimable, "unlimited", StringComparison.OrdinalIgnoreCase)
            ? _maxUint256
            : BigInteger.Parse(claimerEntry.MaxClaimable);

        var price = string.IsNullOrEmpty(claimerEntry.Price) || string.Equals(claimerEntry.Price, "unlimited", StringComparison.OrdinalIgnoreCase)
            ? _maxUint256
            : BigInteger.Parse(claimerEntry.Price);

        var currency = string.IsNullOrEmpty(claimerEntry.CurrencyAddress)
            ? Constants.ADDRESS_ZERO
            : claimerEntry.CurrencyAddress;

        return new AllowlistProof
        {
            Proof = fullProof,
            QuantityLimitPerWallet = maxClaimable,
            PricePerToken = price,
            Currency = currency
        };
    }

    /// <summary>
    /// Calculates the shard key for a wallet address.
    /// </summary>
    /// <param name="walletAddress">The wallet address.</param>
    /// <param name="shardNybbles">The number of hex characters for the shard key (default: 2).</param>
    /// <returns>The shard key (e.g., "c1" for address 0xc143...).</returns>
    public static string GetShardKey(string walletAddress, int shardNybbles = 2)
    {
        var normalized = walletAddress.ToLower();
        if (normalized.StartsWith("0x"))
        {
            normalized = normalized[2..];
        }

        return normalized[..Math.Min(shardNybbles, normalized.Length)];
    }
}
