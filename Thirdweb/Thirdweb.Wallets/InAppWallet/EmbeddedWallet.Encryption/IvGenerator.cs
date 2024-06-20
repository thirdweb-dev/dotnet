using System.Security.Cryptography;

namespace Thirdweb.EWS
{
    internal abstract class IvGeneratorBase
    {
        internal abstract Task ComputeIvAsync(byte[] iv);
    }

    internal class IvGenerator : IvGeneratorBase
    {
        private long prbsValue;
        private readonly string ivFilePath;
        private const int nPrbsBits = 48;
        private const long prbsPeriod = (1L << nPrbsBits) - 1;
        private static readonly long taps = new int[] { nPrbsBits, 47, 21, 20 }.Aggregate(0L, (a, b) => a + (1L << (nPrbsBits - b))); // https://docs.xilinx.com/v/u/en-US/xapp052, page 5

        internal IvGenerator(string storageDirectoryPath = null)
        {
            string directory;
            directory = storageDirectoryPath ?? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            directory = Path.Combine(directory, "EWS");
            Directory.CreateDirectory(directory);
            ivFilePath = Path.Combine(directory, "iv.txt");
            try
            {
                prbsValue = long.Parse(File.ReadAllText(ivFilePath));
            }
            catch (Exception)
            {
                prbsValue = (0x434a49445a27 ^ DateTime.Now.Ticks) & prbsPeriod;
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
            prbsValue = ComputeNextPrbsValue(prbsValue);
            await File.WriteAllTextAsync(ivFilePath, prbsValue.ToString()).ConfigureAwait(false);
            byte[] prbsBytes = Enumerable.Range(0, nPrbsBits / 8).Select((i) => (byte)(prbsValue >> (8 * i))).ToArray();
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
            if ((prbsValue & (1L << nPrbsBits)) != 0)
            {
                prbsValue ^= taps;
                prbsValue &= prbsPeriod;
            }
            return prbsValue;
        }
    }
}
