using System.Security.Cryptography;

namespace Thirdweb.EWS;

internal abstract class IvGeneratorBase
{
    internal abstract Task ComputeIvAsync(byte[] iv);
}

internal class IvGenerator : IvGeneratorBase
{
    private const int NPrbsBits = 48;
    private const long PrbsPeriod = (1L << NPrbsBits) - 1;

    private long _prbsValue;
    private readonly string _ivFilePath;
    private static readonly long _taps = new int[] { NPrbsBits, 47, 21, 20 }.Aggregate(0L, (a, b) => a + (1L << (NPrbsBits - b))); // https://docs.xilinx.com/v/u/en-US/xapp052, page 5

    internal IvGenerator(string storageDirectoryPath = null)
    {
        string directory;
        directory = storageDirectoryPath ?? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        directory = Path.Combine(directory, "EWS");
        _ = Directory.CreateDirectory(directory);
        this._ivFilePath = Path.Combine(directory, "iv.txt");
        try
        {
            this._prbsValue = long.Parse(File.ReadAllText(this._ivFilePath));
        }
        catch (Exception)
        {
            this._prbsValue = (0x434a49445a27 ^ DateTime.Now.Ticks) & PrbsPeriod;
        }
    }

    /// <summary>
    /// Compute IV using half LFSR-generated and half random bytes.
    /// </summary>
    /// <remarks>https://crypto.stackexchange.com/questions/84357/what-are-the-rules-for-using-aes-gcm-correctly</remarks>
    /// <param name="iv">The IV byte array to fill.  This must be twelve bytes in size.</param>
    internal override async Task ComputeIvAsync(byte[] iv)
    {
        RandomNumberGenerator.Fill(iv);
        this._prbsValue = ComputeNextPrbsValue(this._prbsValue);
        await File.WriteAllTextAsync(this._ivFilePath, this._prbsValue.ToString()).ConfigureAwait(false);
        var prbsBytes = Enumerable.Range(0, NPrbsBits / 8).Select((i) => (byte)(this._prbsValue >> (8 * i))).ToArray();
        Array.Copy(prbsBytes, iv, prbsBytes.Length);
    }

    /// <summary>
    /// Compute the next value of a PRBS using a 48-bit Galois LFSR.
    /// </summary>
    /// <remarks>https://en.wikipedia.org/wiki/Linear-feedback_shift_register</remarks>
    /// <param name="prbsValue">The current PRBS value.</param>
    /// <returns>The next value.</returns>
    private static long ComputeNextPrbsValue(long prbsValue)
    {
        prbsValue <<= 1;
        if ((prbsValue & (1L << NPrbsBits)) != 0)
        {
            prbsValue ^= _taps;
            prbsValue &= PrbsPeriod;
        }
        return prbsValue;
    }
}
