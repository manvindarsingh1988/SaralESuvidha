using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace SaralESuvidha.ViewModel
{
    public static class HmacSha256
    {
        public static string HmacSha256Hex_0(object payload, string sharedSecret)
        {
            string jsonPayload = JsonConvert.SerializeObject(payload);

            byte[] keyBytes = Encoding.UTF8.GetBytes(sharedSecret);
            byte[] payloadBytes = Encoding.UTF8.GetBytes(jsonPayload);

            using (var hmacsha256 = new HMACSHA256(keyBytes))
            {
                byte[] hashBytes = hmacsha256.ComputeHash(payloadBytes);

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        public static string HmacSha256HexV2(string payload, string sharedSecret)
        {
            string jsonPayload = payload;
            //string jsonPayload = JsonConvert.SerializeObject(payload);

            byte[] keyBytes = Encoding.UTF8.GetBytes(sharedSecret);
            byte[] payloadBytes = Encoding.UTF8.GetBytes(jsonPayload);

            using (var hmacsha256 = new HMACSHA256(keyBytes))
            {
                byte[] hashBytes = hmacsha256.ComputeHash(payloadBytes);

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        public static string HmacSha256Hex(string payload, string sharedSecret)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(sharedSecret);
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

            using (var hmacsha256 = new HMACSHA256(keyBytes))
            {
                byte[] hashBytes = hmacsha256.ComputeHash(payloadBytes);

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.AppendFormat("{0:x2}", b);
                }
                return sb.ToString();
            }
        }
    }
}
