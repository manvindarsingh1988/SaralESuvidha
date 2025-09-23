using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ForceFail
{
    public class SabPaisaService
    {
        private readonly SabPaisaOptions _options;
        Encryption enc = new Encryption();

        public SabPaisaService()
        {
            _options = new SabPaisaOptions()
            {
                AuthIV = "tr6ASsMaAPjQ/eIIzdZegETUwhUfaODu2bThDqFNAVh2oAfQRjQ+q0CEUxyZvz11",
                AuthKey = "kSxIGCSzSTpn7NMYsRcjXiX6BIyGpN2mI9ZXtwQaz+Y=",
                ClientCode = "SARA69",
                InitiateUrl = "https://securepay.sabpaisa.in/SabPaisa/sabPaisaInit?v=1",
                MerchantId = "4900",
                StatusUrl = "https://txnenquiry.sabpaisa.in/SPTxtnEnquiry/getTxnStatusByClientxnId",
                TransUserName = "anuraggupta2204@gmail.com",
                TransUserPassword = "SARA69_SP23024"
            };
        }

        public async Task<string> InitiatePaymentAsync(TransactionRequest request)
        {
            string clientCode = _options.ClientCode;
            string transUserName = _options.TransUserName;
            string transUserPassword = _options.TransUserPassword;
            string authKey = _options.AuthKey;
            string authIV = _options.AuthIV;

            string payerName = request.CustomerName;
            string payerEmail = request.CustomerEmail;
            string payerMobile = request.CustomerPhone;

            string clientTxnId = request.TxnId;
            string amount = request.Amount.ToString("F2");
            string amountType = "INR";
            string channelId = "W";
            string mcc = _options.MerchantId;
            string callbackUrl = request.ReturnUrl;
            string query = "";

            query = query + "payerName=" + payerName.Trim() + "";
            query = query + "&payerEmail=" + payerEmail.Trim() + "";
            query = query + "&payerMobile=" + payerMobile.Trim() + "";
            query = query + "&clientCode=" + clientCode.Trim() + "";
            query = query + "&transUserName=" + transUserName.Trim() + "";
            query = query + "&transUserPassword=" + transUserPassword.Trim() + "";
            query = query + "&clientTxnId=" + clientTxnId.Trim() + "";
            query = query + "&amount=" + amount.Trim() + "";
            query = query + "&amountType=" + amountType.Trim() + "";
            query = query + "&channelId=" + channelId.Trim() + "";
            query = query + "&mcc=" + mcc.Trim() + "";
            query = query + "&callbackUrl=" + callbackUrl.Trim() + "";
            query = query + "&udf20=" + request.Udf20.Trim() + "";
            query = query + "&udf1=" + "SARAL" + "";
            string encdata = enc.EncryptString(authKey, authIV, query);
            string respString = $"<form id=\"sabPaisaForm\" action=\"{_options.InitiateUrl}\" method=\"post\">" +
                                    "<input type=\"hidden\" name=\"encData\" value=\"" + encdata + "\" id=\"frm1\">" +
                                    "<input type=\"hidden\" name=\"clientCode\" value=\"" + clientCode + "\" id=\"frm2\">" +
                                    "<input type=\"submit\" name=\"submit\" value=\"submit\" id=\"submitButton\" class=\"form-control btn btn-primary\">" +
                                    "</form>";

            return respString;
        }

        public async Task<TransactionStatus> CheckStatusAsync(string query)
        {
            if (!string.IsNullOrEmpty(query))
            {
                string authKey = _options.AuthKey;
                string authIV = _options.AuthIV;
                string decodedQuery = HttpUtility.UrlDecode(query);
                string decQuery = decodedQuery.Replace("%2B", "+").Replace("%2F", "/").Replace("%3D", "=");
                decQuery = enc.DecryptString(authKey, authIV, decQuery);
                Dictionary<string, string> dictParams = enc.queryParser(decQuery);

                return new TransactionStatus() { Amount = Convert.ToDecimal(dictParams["amount"]), PaidAmount = Convert.ToDecimal(dictParams["paidAmount"]), Message = dictParams["sabpaisaMessage"], PaymentMode = dictParams["paymentMode"], Status = dictParams["status"], TxnId = dictParams["clientTxnId"], SabPaisaTxnId = dictParams["sabpaisaTxnId"] };
            }
            else
            {
                return new TransactionStatus() { Status = "FAILED", Message = "No response received." };
            }
        }

        public async Task<TransactionStatus> CheckStatusByJobAsync(string clientTxnId)
        {
            try
            {
                if (!string.IsNullOrEmpty(clientTxnId))
                {
                    string authKey = _options.AuthKey;
                    string authIV = _options.AuthIV;
                    string query = "";

                    query = query + "clientCode=" + _options.ClientCode.Trim() + "";
                    query = query + "&clientTxnId=" + clientTxnId.Trim() + "";
                    string encdata = enc.EncryptString(authKey, authIV, query);
                    var payload = new StatusCheckPayload
                    {
                        clientCode = _options.ClientCode,
                        statusTransEncData = encdata
                    };
                    string jsonPayload = JsonConvert.SerializeObject(payload);

                    var requestContent = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
                    var client = new HttpClient();
                    var response = await client.PostAsync(
                        _options.StatusUrl,
                        requestContent);

                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();


                    var responseJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseBody);


                    string encryptedHex = responseJson["statusResponseData"];


                    string decryptedJson = enc.DecryptString(authKey, authIV, encryptedHex);

                    var list = decryptedJson.Split('&');
                    var dictParams = new Dictionary<string, string>();

                    foreach (var item in list)
                    {
                        if (string.IsNullOrWhiteSpace(item)) continue;

                        var param = item.Split('=');
                        if (param.Length == 2)
                        {
                            string key = param[0];
                            string value = param[1] == "null" ? null : param[1];

                            if (!dictParams.ContainsKey(key))
                            {
                                dictParams.Add(key, value);
                            }
                        }
                    }

                    return new TransactionStatus() { Amount = Convert.ToDecimal(dictParams["amount"]), PaidAmount = Convert.ToDecimal(dictParams["paidAmount"]), Message = dictParams["sabpaisaMessage"], PaymentMode = dictParams["paymentMode"], Status = dictParams["status"], TxnId = dictParams["clientTxnId"], SabPaisaTxnId = dictParams["sabpaisaTxnId"] };
                }
                else
                {
                    return new TransactionStatus() { Status = "FAILED", Message = "No response received." };
                }
            }
            catch (Exception ex)
            {
                return new TransactionStatus() { Status = "FAILED", Message = "No response received." };
            }
        }
    }

    public class StatusCheckPayload
    {
        public string clientCode { get; set; }
        public string statusTransEncData { get; set; }
    }

    public class SabPaisaOptions
    {
        public string MerchantId { get; set; } = string.Empty;
        public string ClientCode { get; set; } = string.Empty;
        public string AuthKey { get; set; } = string.Empty;
        public string AuthIV { get; set; } = string.Empty;
        public string InitiateUrl { get; set; } = string.Empty;
        public string StatusUrl { get; set; } = string.Empty;
        public string TransUserName { get; set; } = string.Empty;
        public string TransUserPassword { get; set; } = string.Empty;
    }

    public class TransactionRequest
    {
        // Unique transaction/order id (you generate this)
        public string TxnId { get; set; } = string.Empty;

        // Transaction amount
        public decimal Amount { get; set; }

        // Currency (if supported, usually INR)
        public string Currency { get; set; } = "INR";

        // Customer info
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;

        // Optional billing/shipping info
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;

        // Purpose/description of the payment
        public string ProductInfo { get; set; } = string.Empty;

        // Callback URL – can override the one from appsettings
        public string ReturnUrl { get; set; } = string.Empty;
        public string Udf20 { get; set; } = string.Empty;
    }

    public class TransactionStatus
    {
        public string TxnId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;   // e.g., SUCCESS, FAILED, PENDING
        public string Message { get; set; } = string.Empty;  // Additional message
        public decimal Amount { get; set; }
        public decimal Fee { get; set; }
        public decimal PaidAmount { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string PaymentMode { get; set; } = string.Empty;
        public string BankRefNo { get; set; } = string.Empty;
        public string SabPaisaTxnId { get; set; } = string.Empty;
    }

    public class Encryption
    {
        private const int IV_SIZE = 12;  // 96 bits for AES-GCM
        private const int TAG_SIZE = 16; // 128 bits for the authentication tag
        private const int HMAC_LENGTH = 48; // SHA-384 = 48 bytes

        // Convert Base64 string to byte array (for keys)
        private static byte[] Base64ToBytes(string base64Key)
        {
            return Convert.FromBase64String(base64Key);
        }

        // Convert byte array to Hex string
        private static string BytesToHex(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                sb.AppendFormat("{0:X2}", b);
            }
            return sb.ToString();
        }

        // Parse query string to Dictionary
        public Dictionary<string, string> queryParser(string values)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            string[] sites = values.Split('&');
            string[] token;

            foreach (string s in sites)
            {
                token = s.Split('=');
                if (token.Length == 2) // Ensure both key and value exist
                {
                    dict.Add(token[0], token[1]);
                }
            }
            return dict;
        }

        // Encrypt the plaintext using AES-GCM and HMAC-SHA384
        public string EncryptString(string aesKeyBase64, string hmacKeyBase64, string plaintext)
        {
            byte[] aesKey = Base64ToBytes(aesKeyBase64);  // Decode AES key from Base64
            byte[] hmacKey = Base64ToBytes(hmacKeyBase64);  // Decode HMAC key from Base64

            // Generate random IV (12 bytes, 96 bits)
            byte[] iv = new byte[IV_SIZE];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(iv);  // Generate a random IV
            }

            // AES-GCM Encryption using BouncyCastle
            var gcmBlockCipher = new GcmBlockCipher(new AesEngine());
            var parameters = new AeadParameters(new KeyParameter(aesKey), TAG_SIZE * 8, iv); // Tag size in bits
            gcmBlockCipher.Init(true, parameters);  // true for encryption
            byte[] encryptedBytes = new byte[plaintext.Length + TAG_SIZE];
            int outputLen = gcmBlockCipher.ProcessBytes(Encoding.UTF8.GetBytes(plaintext), 0, plaintext.Length, encryptedBytes, 0);
            gcmBlockCipher.DoFinal(encryptedBytes, outputLen);

            // Generate HMAC over IV + Ciphertext + Tag
            byte[] cipherTextWithTag = new byte[IV_SIZE + encryptedBytes.Length];
            Buffer.BlockCopy(iv, 0, cipherTextWithTag, 0, IV_SIZE);
            Buffer.BlockCopy(encryptedBytes, 0, cipherTextWithTag, IV_SIZE, encryptedBytes.Length);

            using (var hmacSha384 = new HMACSHA384(hmacKey))
            {
                byte[] hmac = hmacSha384.ComputeHash(cipherTextWithTag);

                // Combine HMAC + IV + Ciphertext + Tag for the final message
                byte[] finalMessage = new byte[hmac.Length + cipherTextWithTag.Length];
                Buffer.BlockCopy(hmac, 0, finalMessage, 0, hmac.Length);
                Buffer.BlockCopy(cipherTextWithTag, 0, finalMessage, hmac.Length, cipherTextWithTag.Length);

                return BytesToHex(finalMessage);  // Return the encrypted message in HEX format
            }
        }

        // Decrypt the ciphertext using AES-GCM and verify HMAC
        public string DecryptString(string aesKeyBase64, string hmacKeyBase64, string hexCipherText)
        {
            byte[] aesKey = Base64ToBytes(aesKeyBase64);  // Decode AES key from Base64
            byte[] hmacKey = Base64ToBytes(hmacKeyBase64);  // Decode HMAC key from Base64

            // Convert the hex ciphertext to byte array
            byte[] fullMessage = HexToBytes(hexCipherText);

            // Split the HMAC from the rest of the message
            byte[] hmacReceived = new byte[HMAC_LENGTH];
            byte[] encryptedData = new byte[fullMessage.Length - HMAC_LENGTH];
            Buffer.BlockCopy(fullMessage, 0, hmacReceived, 0, HMAC_LENGTH);
            Buffer.BlockCopy(fullMessage, HMAC_LENGTH, encryptedData, 0, encryptedData.Length);

            // Verify HMAC
            using (var hmacSha384 = new HMACSHA384(hmacKey))
            {
                byte[] computedHmac = hmacSha384.ComputeHash(encryptedData);
                if (!AreByteArraysEqual(hmacReceived, computedHmac))
                {
                    throw new CryptographicException("HMAC validation failed. Data may have been tampered.");
                }
            }

            // Extract the IV, Ciphertext, and Tag
            byte[] iv = new byte[IV_SIZE];
            Buffer.BlockCopy(encryptedData, 0, iv, 0, IV_SIZE);

            byte[] cipherTextWithTag = new byte[encryptedData.Length - IV_SIZE];
            Buffer.BlockCopy(encryptedData, IV_SIZE, cipherTextWithTag, 0, cipherTextWithTag.Length);

            // AES-GCM Decryption using BouncyCastle
            var gcmBlockCipher = new GcmBlockCipher(new AesEngine());
            var parameters = new AeadParameters(new KeyParameter(aesKey), TAG_SIZE * 8, iv);  // Tag size in bits
            gcmBlockCipher.Init(false, parameters);  // false for decryption
            byte[] plainBytes = new byte[gcmBlockCipher.GetOutputSize(cipherTextWithTag.Length)];
            int outputLen = gcmBlockCipher.ProcessBytes(cipherTextWithTag, 0, cipherTextWithTag.Length, plainBytes, 0);
            gcmBlockCipher.DoFinal(plainBytes, outputLen);


            return Encoding.UTF8.GetString(plainBytes).TrimEnd('\0');
        }

        // Utility method to compare byte arrays
        private static bool AreByteArraysEqual(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }

        // Utility method to convert HEX string to byte array
        private static byte[] HexToBytes(string hex)
        {
            if (hex.Length % 2 != 0)
                throw new ArgumentException("Invalid hex string length");

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                string byteValue = hex.Substring(i * 2, 2);
                bytes[i] = Convert.ToByte(byteValue, 16);  // parse 2 hex chars
            }
            return bytes;
        }


        //var client = _httpClientFactory.CreateClient();
        //var payload = new Dictionary<string, string>
        //{
        //    { "clientCode", _options.ClientCode },
        //    { "merchantId", _options.MerchantId },
        //    { "txnId", txnId }
        //    // Sometimes API key or hash/signature is also required
        //};

        //var response = await client.PostAsync(
        //    _options.StatusUrl,
        //    new FormUrlEncodedContent(payload));

        //response.EnsureSuccessStatusCode();

        //var json = await response.Content.ReadAsStringAsync();

        //// Parse SabPaisa response into your model
        //return JsonConvert.DeserializeObject<TransactionStatus>(json)
        //       ?? new TransactionStatus { TxnId = txnId, Status = "UNKNOWN" };


        // For production Use this Url :- https://securepay.sabpaisa.in/SabPaisa/sabPaisaInit?v=1
        // For testing Use this Url :- https://stage-securepay.sabpaisa.in/SabPaisa/sabPaisaInit?v=1

        // Write the form HTML to the response
    }

}
