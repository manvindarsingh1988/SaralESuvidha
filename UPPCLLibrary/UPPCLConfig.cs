using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPCLLibrary
{
    public class UPPCLConfig
    {
        public string Id { get; set; }
        public string AgentVANNo { get; set; }
        public string AgentID { get; set; }
        public string Token_APIUsername { get; set; }
        public string Token_APIPassword { get; set; }
        public string TokenUrl { get; set; }
        public string RefreshTokenUrl { get; set; }
        public string BillFetch_Discom_TokenConsumerKey { get; set; }
        public string BillFetch_Discom_TokenConsumerSecret { get; set; }
        public string BillFetch_Discom_AccessToken { get; set; }
        public string BillFetch_Discom_RefreshToken { get; set; }
        public string BillFetch_Discom_TokenType { get; set; }
        public int? BillFetch_Discom_ExpiresIn { get; set; }
        public DateTime? BillFetch_Discom_ExpiresTime { get; set; }
        public string BillFetch_DiscomName_Url { get; set; }
        public string BillFetch_BillDetail_TokenConsumerKey { get; set; }
        public string BillFetch_BillDetail_TokenConsumerSecret { get; set; }
        public string BillFetch_BillDetail_AccessToken { get; set; }
        public string BillFetch_BillDetail_RefreshToken { get; set; }
        public string BillFetch_BillDetail_TokenType { get; set; }
        public int? BillFetch_BillDetail_ExpiresIn { get; set; }
        public DateTime? BillFetch_BillDetail_ExpiresTime { get; set; }
        public string BillFetch_BillDetail_Url { get; set; }
        public string BillFetch_BillDetail_PostData { get; set; }
        public string BillPost_Wallet_TokenConsumerKey { get; set; }
        public string BillPost_Wallet_TokenConsumerSecret { get; set; }
        public string BillPost_Wallet_AccessToken { get; set; }
        public string BillPost_Wallet_RefreshToken { get; set; }
        public string BillPost_Wallet_TokenType { get; set; }
        public int? BillPost_Wallet_ExpiresIn { get; set; }
        public DateTime? BillPost_Wallet_ExpiresTime { get; set; }
        public string BillPost_Wallet_AgentWalletUrl { get; set; }
        public string BillPost_BillPayment_TokenConsumerKey { get; set; }
        public string BillPost_BillPayment_TokenConsumerSecret { get; set; }
        public string BillPost_BillPayment_AccessToken { get; set; }
        public string BillPost_BillPayment_RefreshToken { get; set; }
        public string BillPost_BillPayment_TokenType { get; set; }
        public int? BillPost_BillPayment_ExpiresIn { get; set; }
        public DateTime? BillPost_BillPayment_ExpiresTime { get; set; }
        public string BillPost_BillPayment_BillPostUrl { get; set; }
        public string BillPost_BillPayment_PostData { get; set; }
        public string BillPost_StatusCheck_TokenConsumerKey { get; set; }
        public string BillPost_StatusCheck_TokenConsumerSecret { get; set; }
        public string BillPost_StatusCheck_AccessToken { get; set; }
        public string BillPost_StatusCheck_RefreshToken { get; set; }
        public string BillPost_StatusCheck_TokenType { get; set; }
        public int? BillPost_StatusCheck_ExpiresIn { get; set; }
        public DateTime? BillPost_StatusCheck_ExpiresTime { get; set; }
        public string BillPost_StatusCheck_ApiUrl { get; set; }
        public string BillPost_StatusCheck_PostData { get; set; }
        public string Forcefail_TokenConsumerKey { get; set; }
        public string Forcefail_TokenConsumerSecret { get; set; }
        public string Forcefail_AccessToken { get; set; }
        public string Forcefail_RefreshToken { get; set; }
        public string Forcefail_TokenType { get; set; }
        public int? Forcefail_ExpiresIn { get; set; }
        public DateTime? Forcefail_ExpiresTime { get; set; }
        public string Forcefail_FailUrl { get; set; }
        public string Forcefail_PostData { get; set; }
    }
}
