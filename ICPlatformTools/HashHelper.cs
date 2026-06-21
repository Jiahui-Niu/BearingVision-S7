using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace ICPlatformTools
{
    public class HashHelper
    {
        public static string Sha256(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }  

        public static string HMACsha256(string rawData, string secret)
        {
            return HMACsha256(Encoding.UTF8.GetBytes(rawData), secret);
        }

        public static string HMACsha256(byte[] rawData, string secret)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            using (var hmacsha256 = new HMACSHA256(keyBytes))
            {
                var result = hmacsha256.ComputeHash(rawData, 0, rawData.Length);
                return string.Join("", result.Select(s => s.ToString("X2")));
            }
        }

        public static string HMACSha384(string rawData, string secret)
        {
            return HMACSha384(Encoding.UTF8.GetBytes(rawData), secret);
        }

        public static string HMACSha384(byte[] rawData, string secret)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            using (var hmacsha384 = new HMACSHA384(keyBytes))
            {
                var result = hmacsha384.ComputeHash(rawData, 0, rawData.Length);
                return string.Join("", result.Select(s => s.ToString("X2")));
            }
        }

        public static string HMACSha512(string rawData, string secret)
        {
            return HMACSha512(Encoding.UTF8.GetBytes(rawData), secret);
        }

        public static string HMACSha512(byte[] rawData, string secret)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            using (var hmacsha512 = new HMACSHA512(keyBytes))
            {
                var result = hmacsha512.ComputeHash(rawData, 0, rawData.Length);
                return string.Join("", result.Select(s => s.ToString("X2")));
            }
        }

        public static string GetMD5(byte[] rawData)
        {
			try
			{
            	using (MD5 md5 = MD5.Create())
            	{
                	var bytes = md5.ComputeHash(rawData, 0, rawData.Length);
                	return string.Join("", bytes.Select(s => s.ToString("x2")));
            	}
			}
			catch (Exception ex)
			{
			 	LogHelper.Log.Error(ex.Message, ex);
				return "";
			}
        }

        /*
         * AES128CFB entrypt
         * plain: 要加密的字符串
         * */
        public static string AES128CFBEncrypt(string plain)
        {
            //
            // Encrypt a small sample of data
            //
            byte[] plainBytes = Encoding.UTF8.GetBytes(plain);

            byte[] savedKey = new byte[16];
            byte[] savedIV = new byte[16];
            byte[] cipherBytes;

            try
            {
                using (RijndaelManaged Aes128 = new RijndaelManaged())
                {
                    //
                    // Specify a blocksize of 128, and a key size of 128, which make this
                    // instance of RijndaelManaged an instance of AES 128.
                    //
                    Aes128.BlockSize = 128;
                    Aes128.KeySize = 128;

                    //
                    // Specify CFB8 mode
                    //
                    Aes128.Mode = CipherMode.CFB;
                    Aes128.FeedbackSize = 8;
                    Aes128.Padding = PaddingMode.None;

                    var keyStr = "dahua";
                    var ivStr = "dahua";
                    var _savedKey = Encoding.UTF8.GetBytes(keyStr);
                    var _savedIV = Encoding.UTF8.GetBytes(ivStr);

                    Array.Copy(_savedKey, savedKey, _savedKey.Length < savedKey.Length ? _savedKey.Length : 16);
                    Array.Copy(_savedIV, savedIV, _savedIV.Length < savedIV.Length ? _savedIV.Length : 16);

                    Aes128.Key = savedKey;
                    Aes128.IV = savedIV;

                    //
                    // Generate and save random key and IV.
                    //
                    //Aes128.GenerateKey();
                    //Aes128.GenerateIV();

                    Aes128.Key.CopyTo(savedKey, 0);
                    Aes128.IV.CopyTo(savedIV, 0);

                    using (var encryptor = Aes128.CreateEncryptor())
                    using (var msEncrypt = new MemoryStream())
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (var bw = new BinaryWriter(csEncrypt, Encoding.UTF8))
                    {
                        bw.Write(plainBytes);
                        bw.Close();

                        cipherBytes = msEncrypt.ToArray();
                        return Convert.ToBase64String(cipherBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error(ex.Message, ex);
                return "";
            }
        }

        /*
         * AES128CFB Dencrypt
         * str: encrypted base64 string
         * */
        public static string AES128CFBDencrypt(string str)
        {
            byte[] savedKey = new byte[16];
            byte[] savedIV = new byte[16];

            try
            {
                var cipherBytes = Convert.FromBase64String(str);
                using (RijndaelManaged Aes128 = new RijndaelManaged())
                {
                    Aes128.BlockSize = 128;
                    Aes128.KeySize = 128;
                    Aes128.Mode = CipherMode.CFB;
                    Aes128.FeedbackSize = 8;
                    Aes128.Padding = PaddingMode.None;

                    var keyStr = "dahua";
                    var ivStr = "dahua";
                    var _savedKey = Encoding.UTF8.GetBytes(keyStr);
                    var _savedIV = Encoding.UTF8.GetBytes(ivStr);

                    Array.Copy(_savedKey, savedKey, _savedKey.Length < savedKey.Length ? _savedKey.Length : 16);
                    Array.Copy(_savedIV, savedIV, _savedIV.Length < savedIV.Length ? _savedIV.Length : 16);

                    Aes128.Key = savedKey;
                    Aes128.IV = savedIV;

                    Aes128.Key = savedKey;
                    Aes128.IV = savedIV;

                    using (var decryptor = Aes128.CreateDecryptor())
                    using (var msEncrypt = new MemoryStream(cipherBytes))
                    using (var csEncrypt = new CryptoStream(msEncrypt, decryptor, CryptoStreamMode.Read))
                    using (var br = new BinaryReader(csEncrypt, Encoding.UTF8))
                    {
                        //csEncrypt.FlushFinalBlock();
                        var plainBytes = br.ReadBytes(cipherBytes.Length);
                        return Encoding.UTF8.GetString(plainBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error(ex.Message, ex);
                return "";
            }
        }
    }
}
