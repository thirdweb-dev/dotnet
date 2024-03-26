using System.Security.Cryptography;
using System.Text;

namespace Thirdweb
{
    public static class Utils
    {
        public static string ComputeClientIdFromSecretKey(string secretKey)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(secretKey));
            return BitConverter.ToString(hash).Replace("-", "").ToLower().Substring(0, 32);
        }
    }
}
