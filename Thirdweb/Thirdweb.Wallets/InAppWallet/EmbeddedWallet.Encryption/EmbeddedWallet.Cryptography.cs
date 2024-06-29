using System.Security.Cryptography;
using System.Text;
using Nethereum.Web3.Accounts;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace Thirdweb.EWS
{
    internal partial class EmbeddedWallet
    {
        private async Task<string> DecryptShareAsync(string encryptedShare, string password)
        {
            var parts = encryptedShare.Split(ENCRYPTION_SEPARATOR);
            var ciphertextWithTag = Convert.FromBase64String(parts[0]);
            var iv = Convert.FromBase64String(parts[1]);
            var salt = Convert.FromBase64String(parts[2]);

            int iterationCount;
            if (parts.Length > 3 && int.TryParse(parts[3], out var parsedIterationCount))
            {
                iterationCount = parsedIterationCount;
            }
            else
            {
                iterationCount = DEPRECATED_ITERATION_COUNT;
            }

            var key = await GetEncryptionKeyAsync(password, salt, iterationCount).ConfigureAwait(false);

            byte[] encodedShare;
            try
            {
                // Bouncy Castle expects the authentication tag after the ciphertext.
                GcmBlockCipher cipher = new(new AesEngine());
                cipher.Init(forEncryption: false, new AeadParameters(new KeyParameter(key), 8 * TAG_SIZE, iv));
                encodedShare = new byte[cipher.GetOutputSize(ciphertextWithTag.Length)];
                var offset = cipher.ProcessBytes(ciphertextWithTag, 0, ciphertextWithTag.Length, encodedShare, 0);
                cipher.DoFinal(encodedShare, offset);
            }
            catch
            {
                try
                {
                    var ciphertextSize = ciphertextWithTag.Length - TAG_SIZE;
                    var ciphertext = new byte[ciphertextSize];
                    Array.Copy(ciphertextWithTag, ciphertext, ciphertext.Length);
                    var tag = new byte[TAG_SIZE];
                    Array.Copy(ciphertextWithTag, ciphertextSize, tag, 0, tag.Length);
                    encodedShare = new byte[ciphertext.Length];
#if NET8_0_OR_GREATER
                using AesGcm crypto = new(key, TAG_SIZE);
#else
                    using AesGcm crypto = new(key);
#endif
                    crypto.Decrypt(iv, ciphertext, tag, encodedShare);
                }
                catch (CryptographicException)
                {
                    throw new VerificationException("Invalid recovery code", true);
                }
            }
            var share = Encoding.ASCII.GetString(encodedShare);
            return share;
        }

        private async Task<string> EncryptShareAsync(string share, string password)
        {
            const int saltSize = 16;
            var salt = new byte[saltSize];
            RandomNumberGenerator.Fill(salt);
            var key = await GetEncryptionKeyAsync(password, salt, CURRENT_ITERATION_COUNT).ConfigureAwait(false);
            var encodedShare = Encoding.ASCII.GetBytes(share);
            const int ivSize = 12;
            var iv = new byte[ivSize];
            await ivGenerator.ComputeIvAsync(iv).ConfigureAwait(false);
            byte[] encryptedShare;
            try
            {
                // Bouncy Castle includes the authentication tag after the ciphertext.
                GcmBlockCipher cipher = new(new AesEngine());
                cipher.Init(forEncryption: true, new AeadParameters(new KeyParameter(key), 8 * TAG_SIZE, iv));
                encryptedShare = new byte[cipher.GetOutputSize(encodedShare.Length)];
                var offset = cipher.ProcessBytes(encodedShare, 0, encodedShare.Length, encryptedShare, 0);
                cipher.DoFinal(encryptedShare, offset);
            }
            catch
            {
                var tag = new byte[TAG_SIZE];
                encryptedShare = new byte[encodedShare.Length];
#if NET8_0_OR_GREATER
            using AesGcm crypto = new(key, TAG_SIZE);
#else
                using AesGcm crypto = new(key);
#endif
                crypto.Encrypt(iv, encodedShare, encryptedShare, tag);
                encryptedShare = encryptedShare.Concat(tag).ToArray();
            }
            var rv =
                $"{Convert.ToBase64String(encryptedShare)}{ENCRYPTION_SEPARATOR}{Convert.ToBase64String(iv)}{ENCRYPTION_SEPARATOR}{Convert.ToBase64String(salt)}{ENCRYPTION_SEPARATOR}{CURRENT_ITERATION_COUNT}";
            return rv;
        }

        private (string deviceShare, string recoveryShare, string authShare) CreateShares(string secret)
        {
            Secrets secrets = new();
            secret = $"{WALLET_PRIVATE_KEY_PREFIX}{secret}";
            var encodedSecret = Secrets.GetHexString(Encoding.ASCII.GetBytes(secret));
            var shares = secrets.Share(encodedSecret, 3, 2);
            return (shares[0], shares[1], shares[2]);
        }

        private async Task<byte[]> GetEncryptionKeyAsync(string password, byte[] salt, int iterationCount)
        {
            return await Task.Run(() =>
                {
                    var generator = new Pkcs5S2ParametersGenerator(new Sha256Digest());
                    var keyLength = 256; // 256 bits key size
                    generator.Init(Encoding.UTF8.GetBytes(password), salt, iterationCount);
                    var keyParam = (KeyParameter)generator.GenerateDerivedMacParameters(keyLength);
                    return keyParam.GetKey();
                })
                .ConfigureAwait(false);
        }

        private Account MakeAccountFromShares(params string[] shares)
        {
            Secrets secrets = new();
            var encodedSecret = secrets.Combine(shares);
            var secret = Encoding.ASCII.GetString(Secrets.GetBytes(encodedSecret));
            if (!secret.StartsWith(WALLET_PRIVATE_KEY_PREFIX))
            {
                throw new InvalidOperationException($"Corrupted share encountered {secret}");
            }
            return new Account(secret.Split(WALLET_PRIVATE_KEY_PREFIX)[1]);
        }

        private string MakeRecoveryCode()
        {
            const int codeSize = 16;
            const string characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            string recoveryCode = new(Enumerable.Range(0, codeSize).Select((_) => characters[RandomNumberGenerator.GetInt32(characters.Length)]).ToArray());
            return recoveryCode;
        }
    }
}
