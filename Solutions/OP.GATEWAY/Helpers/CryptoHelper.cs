namespace OP.GATEWAY.Helpers
{
    public class CryptoHelper
    {
        public static string ComputeSha256Hash(string rawData)
        {
            using (var sha256Hash = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawData));
                var builder = new System.Text.StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static bool VerifySha256Hash(string rawData, string hash)
        {
            string computedHash = ComputeSha256Hash(rawData);
            return StringComparer.OrdinalIgnoreCase.Compare(computedHash, hash) == 0;
        }
    }
}
