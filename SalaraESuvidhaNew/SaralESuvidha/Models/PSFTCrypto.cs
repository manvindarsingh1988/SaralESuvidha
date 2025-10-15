using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SaralESuvidha.Models
{
    public class PSFTCrypto
    {
        /// <remarks>

        /// Supported .Net intrinsic SymmetricAlgorithm classes.

        /// </remarks>
        [System.Reflection.ObfuscationAttribute(Feature = "renaming")]
        public enum SymmProvEnum : int
        {
            DES, RC2, Rijndael
        }

        [System.Reflection.ObfuscationAttribute(Feature = "renaming")]
        private SymmetricAlgorithm mobjCryptoService;

        /// <remarks>

        /// Constructor for using an intrinsic .Net SymmetricAlgorithm class.

        /// </remarks>

        public PSFTCrypto(SymmProvEnum NetSelected)
        {
            switch (NetSelected)
            {
                case SymmProvEnum.DES:
                    mobjCryptoService = new DESCryptoServiceProvider();
                    break;
                case SymmProvEnum.RC2:
                    mobjCryptoService = new RC2CryptoServiceProvider();
                    break;
                case SymmProvEnum.Rijndael:
                    mobjCryptoService = new RijndaelManaged();
                    break;
            }
        }

        /// <remarks>

        /// Constructor for using a customized SymmetricAlgorithm class.

        /// </remarks>

        public PSFTCrypto(SymmetricAlgorithm ServiceProvider)
        {
            mobjCryptoService = ServiceProvider;
        }

        public PSFTCrypto()
        {
            // TODO: Complete member initialization
        }

        /// <remarks>

        /// Depending on the legal key size limitations of a specific CryptoService provider

        /// and length of the private key provided, padding the secret key with space character

        /// to meet the legal size of the algorithm.

        /// </remarks>
        [System.Reflection.ObfuscationAttribute(Feature = "renaming")]
        private byte[] GetLegalKey(string Key)
        {
            string sTemp;
            if (mobjCryptoService.LegalKeySizes.Length > 0)
            {
                int lessSize = 0, moreSize = mobjCryptoService.LegalKeySizes[0].MinSize;
                // key sizes are in bits

                while (Key.Length * 8 > moreSize)
                {
                    lessSize = moreSize;
                    moreSize += mobjCryptoService.LegalKeySizes[0].SkipSize;
                }
                sTemp = Key.PadRight(moreSize / 8, ' ');
            }
            else
                sTemp = Key;

            // convert the secret key to byte array

            return ASCIIEncoding.ASCII.GetBytes(sTemp);
        }

        [System.Reflection.ObfuscationAttribute(Feature = "renaming")]
        public string Encrypting(string Source, string Key)
        {
            byte[] bytIn = System.Text.ASCIIEncoding.ASCII.GetBytes(Source);
            // create a MemoryStream so that the process can be done without I/O files

            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            byte[] bytKey = GetLegalKey(Key);

            // set the private key

            mobjCryptoService.Key = bytKey;
            mobjCryptoService.IV = bytKey;

            // create an Encryptor from the Provider Service instance

            ICryptoTransform encrypto = mobjCryptoService.CreateEncryptor();

            // create Crypto Stream that transforms a stream using the encryption

            CryptoStream cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Write);

            // write out encrypted content into MemoryStream

            cs.Write(bytIn, 0, bytIn.Length);
            cs.FlushFinalBlock();

            // get the output and trim the '\0' bytes

            byte[] bytOut = ms.GetBuffer();
            int i = 0;
            for (i = 0; i < bytOut.Length; i++)
                if (bytOut[i] == 0)
                    break;

            // convert into Base64 so that the result can be used in xml

            return System.Convert.ToBase64String(bytOut, 0, i);
        }

        [System.Reflection.ObfuscationAttribute(Feature = "renaming")]
        public string Decrypting(string Source, string Key)
        {
            // convert from Base64 to binary

            byte[] bytIn = System.Convert.FromBase64String(Source);
            // create a MemoryStream with the input

            System.IO.MemoryStream ms = new System.IO.MemoryStream(bytIn, 0, bytIn.Length);

            byte[] bytKey = GetLegalKey(Key);

            // set the private key

            mobjCryptoService.Key = bytKey;
            mobjCryptoService.IV = bytKey;

            // create a Decryptor from the Provider Service instance

            ICryptoTransform encrypto = mobjCryptoService.CreateDecryptor();

            // create Crypto Stream that transforms a stream using the decryption

            CryptoStream cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Read);

            // read out the result from the Crypto Stream

            System.IO.StreamReader sr = new System.IO.StreamReader(cs);
            return sr.ReadToEnd();
        }

        [System.Reflection.ObfuscationAttribute(Feature = "renaming")]
        public static String GetKey(String StringToEncrypt)
        {
            SHA1Managed sha1Encryption = new SHA1Managed();
            Byte[] hash = sha1Encryption.ComputeHash(Encoding.ASCII.GetBytes(StringToEncrypt));
            StringBuilder Digest = new StringBuilder();
            foreach (Byte n in hash)
            {
                //Digest.Append(Convert.ToInt32(n).ToString("x2"));
                Digest.Append(Convert.ToInt32(n).ToString());
            }
            return Digest.ToString();
        }

        [System.Reflection.ObfuscationAttribute(Feature = "renaming")]
        public static string Encrypt(string toEncrypt, bool useHashing, string SecurityKey)
        {
            byte[] keyArray;
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);

            string key = SecurityKey;
            //System.Windows.Forms.MessageBox.Show(key);
            //If hashing use get hashcode regards to your key
            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                //Always release the resources and flush data
                // of the Cryptographic service provide. Best Practice

                hashmd5.Clear();
            }
            else
                keyArray = UTF8Encoding.UTF8.GetBytes(key);

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            //set the secret key for the tripleDES algorithm
            tdes.Key = keyArray;
            //mode of operation. there are other 4 modes.
            //We choose ECB(Electronic code Book)
            tdes.Mode = CipherMode.ECB;
            //padding mode(if any extra byte added)

            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateEncryptor();
            //transform the specified region of bytes array to resultArray
            byte[] resultArray =
              cTransform.TransformFinalBlock(toEncryptArray, 0,
              toEncryptArray.Length);
            //Release resources held by TripleDes Encryptor
            tdes.Clear();
            //Return the encrypted data into unreadable string format
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        [System.Reflection.ObfuscationAttribute(Feature = "renaming")]
        public static string Decrypt(string cipherString, bool useHashing, string SecurityKey)
        {
            byte[] keyArray;
            //get the byte code of the string

            byte[] toEncryptArray = Convert.FromBase64String(cipherString);

            //Get your key from config file to open the lock!
            string key = SecurityKey;

            if (useHashing)
            {
                //if hashing was used get the hash code with regards to your key
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                //release any resource held by the MD5CryptoServiceProvider

                hashmd5.Clear();
            }
            else
            {
                //if hashing was not implemented get the byte code of the key
                keyArray = UTF8Encoding.UTF8.GetBytes(key);
            }

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            //set the secret key for the tripleDES algorithm
            tdes.Key = keyArray;
            //mode of operation. there are other 4 modes. 
            //We choose ECB(Electronic code Book)

            tdes.Mode = CipherMode.ECB;
            //padding mode(if any extra byte added)
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(
                                 toEncryptArray, 0, toEncryptArray.Length);
            //Release resources held by TripleDes Encryptor                
            tdes.Clear();
            //return the Clear decrypted TEXT
            return UTF8Encoding.UTF8.GetString(resultArray);
        }

        [System.Reflection.ObfuscationAttribute(Feature = "renaming")]
        public static string Encrypt(string toEncrypt)
        {
            byte[] keyArray;
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);

            bool useHashing = true;
            string key = "27632376393032236";
            //System.Windows.Forms.MessageBox.Show(key);
            //If hashing use get hashcode regards to your key
            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                //Always release the resources and flush data
                // of the Cryptographic service provide. Best Practice

                hashmd5.Clear();
            }
            else
                keyArray = UTF8Encoding.UTF8.GetBytes(key);

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            //set the secret key for the tripleDES algorithm
            tdes.Key = keyArray;
            //mode of operation. there are other 4 modes.
            //We choose ECB(Electronic code Book)
            tdes.Mode = CipherMode.ECB;
            //padding mode(if any extra byte added)

            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateEncryptor();
            //transform the specified region of bytes array to resultArray
            byte[] resultArray =
              cTransform.TransformFinalBlock(toEncryptArray, 0,
              toEncryptArray.Length);
            //Release resources held by TripleDes Encryptor
            tdes.Clear();
            //Return the encrypted data into unreadable string format
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        [System.Reflection.ObfuscationAttribute(Feature = "renaming")]
        public static string Decrypt(string cipherString)
        {
            byte[] keyArray;
            //get the byte code of the string

            byte[] toEncryptArray = Convert.FromBase64String(cipherString);

            //Get your key from config file to open the lock!
            bool useHashing = true;
            string key = "27632376393032236";

            if (useHashing)
            {
                //if hashing was used get the hash code with regards to your key
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                //release any resource held by the MD5CryptoServiceProvider

                hashmd5.Clear();
            }
            else
            {
                //if hashing was not implemented get the byte code of the key
                keyArray = UTF8Encoding.UTF8.GetBytes(key);
            }

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            //set the secret key for the tripleDES algorithm
            tdes.Key = keyArray;
            //mode of operation. there are other 4 modes. 
            //We choose ECB(Electronic code Book)

            tdes.Mode = CipherMode.ECB;
            //padding mode(if any extra byte added)
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(
                                 toEncryptArray, 0, toEncryptArray.Length);
            //Release resources held by TripleDes Encryptor                
            tdes.Clear();
            //return the Clear decrypted TEXT
            return UTF8Encoding.UTF8.GetString(resultArray);
        }
    }
}
