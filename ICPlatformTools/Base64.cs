/* ==============================================================================
 *Copyright (c) 2019   All Rights Reserved.
 *CLR版本：4.0.30319.42000
 * 类名称：Base64
 * 类描述：
 * 创建人：ysc
 * 创建日期：2019/12/6 9:43:43
 * 修改人：
 * 修改时间：
 * 修改备注：
 * @version 1.0
 * ==============================================================================*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ICPlatformTools
{
    /// <summary>
    /// DES加密、解密
    /// </summary>
    public class DES
    {
        private static byte[] Key = new byte[] { 0x6A, 0x32, 0x64, 0x7F, 0x3A, 0xB8, 0x2D, 0x67, 0xB3, 0x55, 0x19, 0x4E, 0xB8, 0xBF, 0xDD, 0x81, 0xBC, 0xA1, 0x6A, 0xF5, 0x87, 0x3B, 0xE6, 0x59, 0x2A, 0xBB, 0x2B, 0x68, 0xE2, 0x5F, 0x06, 0xFB };
        private DES()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }

        /// <summary>
        /// DES加密
        /// </summary>
        /// <param name="source">待加密字串</param>
        /// <returns>加密后的字符串</returns>
        public static string Encrypt(string source)
        {
            return Encrypt(source, Key);
        }

        /// <summary>
        /// DES加密
        /// </summary>
        /// <param name="source">待加密字串</param>
        /// <param name="key">Key值</param>
        /// <returns>加密后的字符串</returns>
        public static string Encrypt(string source, byte[] key)
        {
            SymmetricAlgorithm sa = Rijndael.Create();
            sa.Key = key;
            sa.Mode = CipherMode.ECB;
            sa.Padding = PaddingMode.Zeros;
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, sa.CreateEncryptor(), CryptoStreamMode.Write);
            byte[] bytes = Encoding.Unicode.GetBytes(source);
            cs.Write(bytes, 0, bytes.Length);
            cs.FlushFinalBlock();
            cs.Close();
            return Convert.ToBase64String(ms.ToArray());
        }

        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="source">待解密的字串</param>
        /// <returns>解密后的字符串</returns>
        public static string Decrypt(string strSource)
        {
            if (strSource.Trim().Length == 0) return "";
            return Decrypt(strSource, Key);
        }

        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="source">待解密的字串</param>
        /// <param name="key">Key值</param>
        /// <returns>解密后的字符串</returns>
        public static string Decrypt(string source, byte[] key)
        {
            SymmetricAlgorithm sa = Rijndael.Create();
            try
            {
                sa.Key = key;
                sa.Mode = CipherMode.ECB;
                sa.Padding = PaddingMode.Zeros;
                ICryptoTransform ct = sa.CreateDecryptor();
                byte[] bytes = Convert.FromBase64String(source);
                MemoryStream ms = new MemoryStream(bytes);
                CryptoStream cs = new CryptoStream(ms, ct, CryptoStreamMode.Read);
                StreamReader sr = new StreamReader(cs, Encoding.Unicode);
                string str = sr.ReadToEnd();
                str = str.Replace('\0', ' ');
                return str.Trim();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.Write(ex.Message);
                return "";
            }
        }
    }
}
