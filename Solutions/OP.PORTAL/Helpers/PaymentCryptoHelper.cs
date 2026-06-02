using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System.Text;

namespace OP.PORTAL.Helpers
{
    public class PaymentCryptoHelper
    {
        public string getchecksum(string MerchantId, string OrderId, string Amount, string redirectUrl, string WorkingKey)
        {
            string strPattern = MerchantId + "|" + OrderId + "|" + Amount + "|" + redirectUrl + "|" + WorkingKey;
            long adler = 1L;
            return adler32(adler, strPattern);
        }

        public string verifychecksum(string MerchantId, string OrderId, string Amount, string AuthDesc, string WorkingKey, string checksum)
        {
            string strPattern = MerchantId + "|" + OrderId + "|" + Amount + "|" + AuthDesc + "|" + WorkingKey;
            long adler = 1L;
            string strA = adler32(adler, strPattern);
            if (string.Compare(strA, checksum, ignoreCase: true) == 0)
            {
                return "true";
            }
            return "false";
        }

        private string adler32(long adler, string strPattern)
        {
            long num = 0L;
            long num2 = 65521L;
            long num3 = andop(adler, 65535L);
            long num4 = andop(cdec(rightshift(cbin(adler), 16L)), 65535L);
            for (int i = 0; i < strPattern.Length; i++)
            {
                char[] array = strPattern.Substring(i, 1).ToCharArray();
                num = array[0];
                num3 = (num3 + num) % num2;
                num4 = (num4 + num3) % num2;
            }
            return (cdec(leftshift(cbin(num4), 16L)) + num3).ToString();
        }

        private long power(long num)
        {
            long num2 = 1L;
            for (int i = 1; i <= num; i++)
            {
                num2 *= 2;
            }
            return num2;
        }

        private long andop(long op1, long op2)
        {
            string text = "";
            string text2 = cbin(op1);
            string text3 = cbin(op2);
            for (int i = 0; i < 32; i++)
            {
                text += long.Parse(text2.Substring(i, 1)) & long.Parse(text3.Substring(i, 1));
            }
            return cdec(text);
        }

        private string cbin(long num)
        {
            string text = "";
            do
            {
                text = (num % 2 + text).ToString();
                num = (long)Math.Floor((decimal)num / 2m);
            }
            while (num != 0);
            long num2 = 32 - text.Length;
            for (int i = 1; i <= num2; i++)
            {
                text = "0" + text;
            }
            return text;
        }

        private string leftshift(string str, long num)
        {
            long num2 = 32 - str.Length;
            for (int i = 1; i <= num2; i++)
            {
                str = "0" + str;
            }
            for (int j = 1; j <= num; j++)
            {
                str += "0";
                str = str.Substring(1, str.Length - 1);
            }
            return str;
        }

        private string rightshift(string str, long num)
        {
            for (int i = 1; i <= num; i++)
            {
                str = "0" + str;
                str = str.Substring(0, str.Length - 1);
            }
            return str;
        }

        private long cdec(string strNum)
        {
            long num = 0L;
            for (int i = 0; i < strNum.Length; i++)
            {
                num += long.Parse(strNum.Substring(i, 1)) * power(strNum.Length - (i + 1));
            }
            return num;
        }

        public string Encrypt(string strToEncrypt, string Key)
        {
            PaymentAesGcm256 aesCryptUtil = new PaymentAesGcm256(Key);
            return aesCryptUtil.encrypt(strToEncrypt);
        }

        public string Decrypt(string strToDecrypt, string Key)
        {
            PaymentAesGcm256 aesCryptUtil = new PaymentAesGcm256(Key);
            return aesCryptUtil.decrypt(strToDecrypt);
        }
    }

    public class PaymentAesGcm256
    {
        private static readonly SecureRandom Random = new SecureRandom();
        public byte[] data;
        // Pre-configured Encryption Parameters
        public static readonly int NonceBitSize = 128;
        public static readonly int MacBitSize = 128;
        public static readonly int KeyBitSize = 256;

        private PaymentAesGcm256() { }

        public PaymentAesGcm256(string Key)
        {
            data = Encoding.UTF8.GetBytes(Key);
        }

        public byte[] NewIv()
        {
            var iv = new byte[NonceBitSize / 8];
            Random.NextBytes(iv);
            return iv;
        }

        public static Byte[] HexToByte(string hexStr)
        {
            byte[] bArray = new byte[hexStr.Length / 2];
            for (int i = 0; i < (hexStr.Length / 2); i++)
            {
                byte firstNibble = Byte.Parse(hexStr.Substring((2 * i), 1),
                                   System.Globalization.NumberStyles.HexNumber); // [x,y)
                byte secondNibble = Byte.Parse(hexStr.Substring((2 * i) + 1, 1),
                                    System.Globalization.NumberStyles.HexNumber);
                int finalByte = (secondNibble) | (firstNibble << 4); // bit-operations 
                                                                     // only with numbers, not bytes.
                bArray[i] = (byte)finalByte;
            }
            return bArray;
        }

        public string toHex(byte[] data)
        {
            string hex = string.Empty;
            foreach (byte c in data)
            {
                hex += c.ToString("X2");
            }
            return hex;
        }

        public static string toHex(string asciiString)
        {
            string hex = string.Empty;
            foreach (char c in asciiString)
            {
                int tmp = c;
                hex += string.Format("{0:x2}", System.Convert.ToUInt32(tmp.ToString()));
            }
            return hex;
        }

        public string encrypt(string PlainText)
        {
            string sR = string.Empty;

            byte[] iv = NewIv();
            try
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(PlainText);
                GcmBlockCipher cipher = new GcmBlockCipher(new AesEngine());
                AeadParameters parameters =
                             new AeadParameters(new KeyParameter(data), 128, iv, null);
                cipher.Init(true, parameters);
                byte[] encryptedBytes = new byte[cipher.GetOutputSize(plainBytes.Length)];
                Int32 retLen = cipher.ProcessBytes
                               (plainBytes, 0, plainBytes.Length, encryptedBytes, 0);
                cipher.DoFinal(encryptedBytes, retLen);
                //auth_tag = cipher.GetMac();
                sR = toHex(encryptedBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            return toHex(iv) + sR;
        }

        public string decrypt(string EncryptedText)
        {
            string sR = string.Empty;
            string encodedIV = EncryptedText.TrimEnd("\r\n\0".ToCharArray()).Substring(0, 32);
            string pEncryptedText = EncryptedText.Substring(32);
            try
            {
                byte[] iv = HexToByte(encodedIV);
                byte[] encryptedBytes = HexToByte(pEncryptedText.TrimEnd("\r\n\0".ToCharArray()));
                GcmBlockCipher cipher = new GcmBlockCipher(new AesEngine());
                AeadParameters parameters =
                          new AeadParameters(new KeyParameter(data), 128, iv, null);
                //ParametersWithIV parameters = new ParametersWithIV(new KeyParameter(key), iv);

                cipher.Init(false, parameters);
                byte[] plainBytes = new byte[cipher.GetOutputSize(encryptedBytes.Length)];
                Int32 retLen = cipher.ProcessBytes
                               (encryptedBytes, 0, encryptedBytes.Length, plainBytes, 0);
                cipher.DoFinal(plainBytes, retLen);

                sR = Encoding.UTF8.GetString(plainBytes).TrimEnd("\r\n\0".ToCharArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            return sR;
        }
    }
}
