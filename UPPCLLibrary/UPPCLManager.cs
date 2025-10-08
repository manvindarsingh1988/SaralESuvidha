using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Authenticators;
using Dapper;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using UPPCLLibrary.BillFail;
using UPPCLLibrary.BillFetch;
using UPPCLLibrary.BillPost;
using UPPCLLibrary.OTS;

namespace UPPCLLibrary
{
    public static class UPPCLManager
    {
        public static string DbConnection = "";
        public static UPPCLConfig uppclConfig = new UPPCLConfig();

        public static string Test()
        {
            return DbConnection;
        }

        public static string Initialize()
        {
            string result = "BeforeStart";
            try
            {
                using (var con = new SqlConnection(DbConnection))
                {
                    var queryParameters = new DynamicParameters();
                    queryParameters.Add("@Id", "CON001");
                    uppclConfig = con.Query<UPPCLConfig>("usp_UPPCLConfigSelect", queryParameters, commandType: System.Data.CommandType.StoredProcedure).SingleOrDefault();

                    //_refresh_token_
                    if (!string.IsNullOrEmpty(uppclConfig?.AgentID))
                    {
                        uppclConfig.TokenUrl = uppclConfig.TokenUrl.Replace("_token_user_name_",uppclConfig.Token_APIUsername).Replace("_token_password_",uppclConfig.Token_APIPassword);
                        //suppclConfig.RefreshTokenUrl = uppclConfig.RefreshTokenUrl;
                    }
                    

                    return (JsonConvert.SerializeObject(uppclConfig));
                }
            }
            catch (Exception ex)
            {
                result = "Exception in config Initialize: " + ex.Message;
            }
            return result;
        }


        /// <summary>
        /// Discom List Code Start
        /// </summary>
        /// <returns></returns>

        public static TokenResponse DiscomListToken()
        {
            TokenResponse tr = new TokenResponse();

            try
            {
                var client = new RestClient(uppclConfig.TokenUrl);
                var request = new RestRequest("", Method.Post);
                string basicAuth = "Basic " + EncodeBase64(Encoding.ASCII, uppclConfig.BillFetch_Discom_TokenConsumerKey + ":" + uppclConfig.BillFetch_Discom_TokenConsumerSecret);
                request.AddHeader("Authorization", basicAuth);
                var response = client.Execute<TokenResponse>(request);

                /*
                if (response.IsSuccessful)
                {
                    tr = JsonConvert.DeserializeObject<TokenResponse>(response.Content);
                    if (tr.error != "invalid_grant" || tr.error == "invalid_client")
                    {
                        UpdateDiscomListToken(tr);
                        Initialize();
                    }
                }
                else
                {
                    tr.error_description = "Internal: Error: " + response.Content;
                }
                */
                tr = JsonConvert.DeserializeObject<TokenResponse>(response.Content);
                if (tr.error != "invalid_grant" || tr.error == "invalid_client")
                {
                    UpdateDiscomListToken(tr);
                    Initialize();
                }

                SaveHitLog("S", null, null, null, "DiscomAccess", uppclConfig.TokenUrl, basicAuth, response?.Content,DateTime.Now, null);

            }
            catch (Exception ex)
            {
                tr.error_description = "Internal: Exception: " + ex.Message;
            }
            return tr;
        }

        public static TokenResponse DiscomListRefreshToken()
        {
            TokenResponse tr = new TokenResponse();

            try
            {
                var client = new RestClient(uppclConfig.RefreshTokenUrl.Replace("_refresh_token_", uppclConfig.BillFetch_Discom_RefreshToken));
                var request = new RestRequest("", Method.Post);
                string basicAuth = "Basic " + EncodeBase64(Encoding.ASCII, uppclConfig.BillFetch_Discom_TokenConsumerKey + ":" + uppclConfig.BillFetch_Discom_TokenConsumerSecret);
                request.AddHeader("Authorization", basicAuth);
                var response = client.Execute<TokenResponse>(request);

                if (response.Content.Contains("access token data not"))
                {
                    DiscomListToken();
                }
                
                if (response.IsSuccessful)
                {
                    tr = JsonConvert.DeserializeObject<TokenResponse>(response.Content);
                    if (tr.error != "invalid_grant" || tr.error == "invalid_client")
                    {
                        UpdateDiscomListToken(tr);
                        Initialize();
                    }
                    else
                    {
                        //Refresh token invalid, so get the fresh access token and refresh token.
                        DiscomListToken();
                    }
                }
                else
                {
                    tr.error_description = "Internal: Error: " + response.Content;
                }

                SaveHitLog("S", null, null, null, "DiscomRefresh", uppclConfig.TokenUrl, basicAuth, response?.Content, DateTime.Now, null);

            }
            catch (Exception ex)
            {
                tr.error_description = "Internal: Exception: " + ex.Message;
            }
            return tr;
        }

        public static string DiscomList()
        {
            string tr = "";

            try
            {
                var client = new RestClient(uppclConfig.BillFetch_DiscomName_Url);
                var request = new RestRequest("", Method.Get);
                string basicAuth = "Bearer " + uppclConfig.BillFetch_Discom_AccessToken;
                request.AddHeader("Authorization", basicAuth);
                request.AddHeader("Content-Type", "application/json");
                var response = client.Execute<string>(request);

                tr = response.Content ?? "no data found";

                SaveHitLog("S", null, null, null, "DiscomList", uppclConfig.BillFetch_DiscomName_Url, basicAuth, response?.Content, DateTime.Now, null);
            }
            catch (Exception ex)
            {
                tr += "Internal: Exception: " + ex.Message;
            }
            return tr;
        }

        public static string UpdateDiscomListToken(TokenResponse tr)
        {
            string result = "";
            try
            {
                TimeSpan ts = new TimeSpan(0, 0, tr.expires_in / 60, 0);
                DateTime dtExpiry = DateTime.Now.Add(ts);

                using var con = new SqlConnection(DbConnection);
                var queryParameters = new DynamicParameters();
                queryParameters.Add("@Id", "CON001");
                queryParameters.Add("@BillFetch_Discom_AccessToken", tr.access_token);
                queryParameters.Add("@BillFetch_Discom_RefreshToken", tr.refresh_token);
                queryParameters.Add("@BillFetch_Discom_TokenType", tr.token_type);
                queryParameters.Add("@BillFetch_Discom_ExpiresIn", tr.expires_in);
                queryParameters.Add("@BillFetch_Discom_ExpiresTime", dtExpiry);
                result = con.Query<string>("usp_UPPCLConfigDiscomListTokenUpdate", queryParameters, commandType: System.Data.CommandType.StoredProcedure).SingleOrDefault();
            }
            catch (Exception ex)
            {
                result += "Exception: " + ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Discom List Code End
        /// </summary>


        //// Billfetch Code Start

        public static TokenResponse BillFetchToken()
        {
            TokenResponse tr = new TokenResponse();

            try
            {
                var client = new RestClient(uppclConfig.TokenUrl);
                var request = new RestRequest("", Method.Post);
                string basicAuth = "Basic " + EncodeBase64(Encoding.ASCII, uppclConfig.BillFetch_BillDetail_TokenConsumerKey + ":" + uppclConfig.BillFetch_BillDetail_TokenConsumerSecret);
                request.AddHeader("Authorization", basicAuth);
                var response = client.Execute<TokenResponse>(request);

                
                if (response.IsSuccessful)
                {
                    tr = JsonConvert.DeserializeObject<TokenResponse>(response.Content);
                    if (tr.error != "invalid_grant" || tr.error == "invalid_client")
                    {
                        UpdateBillFetchToken(tr);
                        Initialize();
                    }
                }
                else
                {
                    tr.error_description = "Internal: Error: " + response.Content;
                }

                SaveHitLog("S", null, null, null, "BillFetchAccess", uppclConfig.TokenUrl, basicAuth, response?.Content, DateTime.Now, null);
            }
            catch (Exception ex)
            {
                tr.error_description = "Internal: Exception: " + ex.Message;
            }
            return tr;
        }

        public static TokenResponse BillFetchRefreshToken()
        {
            TokenResponse tr = new TokenResponse();

            try
            {
                var client = new RestClient(uppclConfig.RefreshTokenUrl.Replace("_refresh_token_", uppclConfig.BillFetch_BillDetail_RefreshToken));
                var request = new RestRequest("", Method.Post);
                string basicAuth = "Basic " + EncodeBase64(Encoding.ASCII, uppclConfig.BillFetch_BillDetail_TokenConsumerKey + ":" + uppclConfig.BillFetch_BillDetail_TokenConsumerSecret);
                request.AddHeader("Authorization", basicAuth);
                var response = client.Execute<TokenResponse>(request);

                if (response.Content.Contains("access token data not"))
                {
                    BillFetchToken();
                }

                if (response.IsSuccessful)
                {
                    tr = JsonConvert.DeserializeObject<TokenResponse>(response.Content);
                    if (tr.error != "invalid_grant" || tr.error == "invalid_client")
                    {
                        UpdateBillFetchToken(tr);
                        Initialize();
                    }
                    else
                    {
                        //Refresh token invalid, so get the fresh access token and refresh token.
                        BillFetchToken();
                    }
                }
                else
                {
                    tr.error_description = "Internal: Error: " + response.Content;
                }

                SaveHitLog("S", null, null, null, "BillFetchRefresh", uppclConfig.TokenUrl, basicAuth, response?.Content, DateTime.Now, null);

            }
            catch (Exception ex)
            {
                tr.error_description = "Internal: Exception: " + ex.Message;
            }
            return tr;
        }

        public static BillFetchResponse BillFetch(string divisionName, string consumerNumber)
        {
            

            BillFetchResponse br = new BillFetchResponse();
            //string tr = "";

            try
            {
                var client = new RestClient(uppclConfig.BillFetch_BillDetail_Url);
                var request = new RestRequest("", Method.Post);
                string basicAuth = "Bearer " + uppclConfig.BillFetch_BillDetail_AccessToken;
                request.AddHeader("Authorization", basicAuth);
                string postData = uppclConfig.BillFetch_BillDetail_PostData.Replace("_discomName_", divisionName).Replace("_accountNo_", consumerNumber.Trim());
                request.AddStringBody(postData, ContentType.Json);

                var response = client.Execute<BillFetchResponse>(request);

                SaveHitLog("S", null, null, consumerNumber, "BillFetchDetail_0", uppclConfig.BillFetch_BillDetail_Url + " " + postData, basicAuth + " " + postData, response?.Content, DateTime.Now, null);

                if (response.IsSuccessful)
                {
                    br = JsonConvert.DeserializeObject<BillFetchResponse>(response.Content);
                    if (!string.IsNullOrEmpty(br.FAULTCODE)) //br.FAULTCODE == "202" || br.FAULTCODE == "214" || br.FAULTCODE == "211"
                    {
                        br.Body.Fault.detail.ErrorInfo.ErrorDetails.detail = "Errors: " + br.FAULTSTRING;
                    }
                    else
                    {
                        br.Body.PaymentDetailsResponse.BillID = br.Body.PaymentDetailsResponse.BillID?.Trim();
                        br.Body.PaymentDetailsResponse.KNumber = br.Body.PaymentDetailsResponse.KNumber.Trim();

                        try
                        {
                            if (br.Body.PaymentDetailsResponse.Param1?.ToString() != "null")
                            {
                                br.Body.PaymentDetailsResponse.Param1 = br.Body.PaymentDetailsResponse.Param1?.ToString().Trim();
                            }
                        }
                        catch (Exception exParam1)
                        {
                            
                        }
                    }
                    
                }
                else
                {
                    br.Body.Fault.detail.ErrorInfo.ErrorDetails.detail = "Internal: Error: " + response.Content;
                }
                
                SaveHitLog("S", null, null, consumerNumber, "BillFetchDetail", uppclConfig.BillFetch_BillDetail_Url + " " + postData, basicAuth + " " + postData, response?.Content, DateTime.Now, null);

            }
            catch (Exception ex)
            {
                br.Body.Fault.detail.ErrorInfo.ErrorDetails.detail = "Internal: Exception: " + ex.Message;
            }
            return br;
        }

        public static ElectricityBillInfo ElectricityBillInfoFromBillFetch(BillFetchResponse billFetchResponse)
        {
            ElectricityBillInfo electricityBillInfo = new ElectricityBillInfo();
            try
            {
                electricityBillInfo.AccountNumber = billFetchResponse.Body.PaymentDetailsResponse.KNumber;
                electricityBillInfo.Message = "";
                electricityBillInfo.AdditionalData = "";
                electricityBillInfo.Address = billFetchResponse.Body.PaymentDetailsResponse.PremiseAddress.AddressLine1 + "," +
                                                billFetchResponse.Body.PaymentDetailsResponse.PremiseAddress.AddressLine2 + "," +
                                                billFetchResponse.Body.PaymentDetailsResponse.PremiseAddress.City;
                electricityBillInfo.BillAmount = billFetchResponse.Body.PaymentDetailsResponse.AccountInfo;
                electricityBillInfo.BillDate = billFetchResponse.Body.PaymentDetailsResponse.BillDate;
                electricityBillInfo.BillNumber = billFetchResponse.Body.PaymentDetailsResponse.BillID;
                electricityBillInfo.BillPeriod = "";
                electricityBillInfo.BillConvnienceDescription = "";
                electricityBillInfo.BillConvnienceFee = "";
                electricityBillInfo.CustomerMobile = billFetchResponse.Body.PaymentDetailsResponse.MobileNumber;
                electricityBillInfo.CustomerName = billFetchResponse.Body.PaymentDetailsResponse.ConsumerName;
                electricityBillInfo.AdditionalData = billFetchResponse.Body.PaymentDetailsResponse.SDOCode + ", " + billFetchResponse.Body.PaymentDetailsResponse.Division + ", " + billFetchResponse.Body.PaymentDetailsResponse.SubDivision;
                //electricityBillInfo.LastPaymentAmount = billFetchResponse.Body.PaymentDetailsResponse.LastPaidAmount.ToString();
                electricityBillInfo.DueDate = billFetchResponse.Body.PaymentDetailsResponse.BillDueDate;
                electricityBillInfo.EarlyBillDate = "";
                electricityBillInfo.EarlyPaymentBillAmount = "";
                electricityBillInfo.PayableAmount = billFetchResponse.Body.PaymentDetailsResponse.AccountInfo;
                electricityBillInfo.OperatorBiller = billFetchResponse.Body.PaymentDetailsResponse.Discom;

                

                try
                {
                    if(billFetchResponse.Body.PaymentDetailsResponse.Param1.ToString() != "null")
                    {
                        electricityBillInfo.DueDateRebate = ((int)Math.Round(Convert.ToDecimal(billFetchResponse.Body.PaymentDetailsResponse.Param1.ToString()), MidpointRounding.AwayFromZero)).ToString();
                    }
                }catch(Exception exParam1)
                {
                    electricityBillInfo.Message += "Exception param1: " + exParam1.Message;
                }
            }
            catch (Exception ex)
            {
                electricityBillInfo.Message += "Exception: " + ex.Message;
            }
            return electricityBillInfo;
        }

        public static string UpdateBillFetchToken(TokenResponse tr)
        {
            string result = "";
            try
            {
                TimeSpan ts = new TimeSpan(0, 0, tr.expires_in / 60, 0);
                DateTime dtExpiry = DateTime.Now.Add(ts);

                using var con = new SqlConnection(DbConnection);
                var queryParameters = new DynamicParameters();
                queryParameters.Add("@Id", "CON001");
                queryParameters.Add("@BillFetch_BillDetail_AccessToken", tr.access_token);
                queryParameters.Add("@BillFetch_BillDetail_RefreshToken", tr.refresh_token);
                queryParameters.Add("@BillFetch_BillDetail_TokenType", tr.token_type);
                queryParameters.Add("@BillFetch_BillDetail_ExpiresIn", tr.expires_in);
                queryParameters.Add("@BillFetch_BillDetail_ExpiresTime", dtExpiry);
                result = con.Query<string>("usp_UPPCLConfigBillFetchTokenUpdate", queryParameters, commandType: System.Data.CommandType.StoredProcedure).SingleOrDefault();
            }
            catch (Exception ex)
            {
                result += "Exception: " + ex.Message;
            }
            return result;
        }

        //// BillFetch Code End



        //// Wallet Code End
        public static TokenResponse WalletToken()
        {
            TokenResponse tr = new TokenResponse();

            try
            {
                var client = new RestClient(uppclConfig.TokenUrl);
                var request = new RestRequest("", Method.Post);
                string basicAuth = "Basic " + EncodeBase64(Encoding.ASCII, uppclConfig.BillPost_Wallet_TokenConsumerKey + ":" + uppclConfig.BillPost_Wallet_TokenConsumerSecret);
                request.AddHeader("Authorization", basicAuth);
                var response = client.Execute<TokenResponse>(request);

                
                if (response.IsSuccessful)
                {
                    tr = JsonConvert.DeserializeObject<TokenResponse>(response.Content);
                    if (tr.error != "invalid_grant" || tr.error == "invalid_client")
                    {
                        UpdateWalletToken(tr);
                        Initialize();
                    }
                }
                else
                {
                    tr.error_description = "Internal: Error: " + response.Content;
                }


                SaveHitLog("S", null, null, null, "WalletAccess", uppclConfig.TokenUrl, basicAuth, response?.Content, DateTime.Now, null);
            }
            catch (Exception ex)
            {
                tr.error_description = "Internal: Exception: " + ex.Message;
            }
            return tr;
        }

        public static TokenResponse WalletRefreshToken()
        {
            TokenResponse tr = new TokenResponse();
            //string tr = "";

            try
            {
                var client = new RestClient(uppclConfig.RefreshTokenUrl.Replace("_refresh_token_", uppclConfig.BillPost_Wallet_RefreshToken));
                var request = new RestRequest("", Method.Post);
                string basicAuth = "Basic " + EncodeBase64(Encoding.ASCII, uppclConfig.BillPost_Wallet_TokenConsumerKey + ":" + uppclConfig.BillPost_Wallet_TokenConsumerSecret);
                request.AddHeader("Authorization", basicAuth);
                var response = client.Execute<TokenResponse>(request);

                if (response.Content.Contains("access token data not"))
                {
                    WalletToken();
                }

                if (response.IsSuccessful)
                {
                    tr = JsonConvert.DeserializeObject<TokenResponse>(response.Content);
                    if (tr.error != "invalid_grant" || tr.error == "invalid_client")
                    {
                        UpdateWalletToken(tr);
                        Initialize();
                    }
                    else
                    {
                        //Refresh token invalid, so get the fresh access token and refresh token.
                        WalletToken();
                    }
                }
                else
                {
                    tr.error_description = "Internal: Error: " + response.Content;
                }

                SaveHitLog("S", null, null, null, "WalletRefresh", uppclConfig.TokenUrl, basicAuth, response?.Content, DateTime.Now, null);

            }
            catch (Exception ex)
            {
                tr.error_description = "Internal: Exception: " + ex.Message;
            }
            return tr;
        }

        public static WalletResponse WalletDetails()
        {
            WalletResponse wr = new WalletResponse();

            try
            {
                //var client = new RestClient(uppclConfig.BillPost_Wallet_AgentWalletUrl.Replace("_van_no_", uppclConfig.AgentVANNo));
                //var request = new RestRequest("", Method.Get);

                RestClientOptions restClientOptions = new RestClientOptions();
                restClientOptions.MaxTimeout = 5000;
                restClientOptions.BaseUrl = new Uri(uppclConfig.BillPost_Wallet_AgentWalletUrl.Replace("_van_no_", uppclConfig.AgentVANNo));

                var client = new RestClient(restClientOptions);
                var request = new RestRequest("", Method.Get);
                request.Timeout = 5000;

                string basicAuth = "Bearer " + uppclConfig.BillPost_Wallet_AccessToken;
                request.AddHeader("Authorization", basicAuth);

                var response = client.Execute<WalletResponse>(request);

                var respData = response?.Content;
                if (respData != null)
                {
                    if (respData.Contains("balance"))
                    {
                        wr = JsonConvert.DeserializeObject<WalletResponse>(response?.Content);
                    }
                    else if (respData.Contains("error"))
                    {
                        wr.message = "Exception: Server: " + GetRegExFirstMatch(respData, "error\":\"(.*)\",\"me");
                    }
                    else if (respData.Contains("ams:fault"))
                    {
                        string? errorCode = GetRegExFirstMatch(respData, "ams:message>(.*)</ams:message>");
                        if (errorCode != null && errorCode == "Invalid Credentials")
                        {
                            WalletToken();
                        }
                        wr.message = "Exception: Server: " + errorCode;
                    }
                    else if (respData.Contains("am:fault"))
                    {
                        string? errorCode = GetRegExFirstMatch(respData, "am:message>(.*)</am:message>");
                        wr.message = "Exception: Server: " + errorCode;
                    }
                }
                else
                {
                    wr.message = "Exception: Server: null response.";
                }

                /*
                if (response.IsSuccessful)
                {
                    wr = JsonConvert.DeserializeObject<WalletResponse>(response?.Content);
                }
                else
                {
                    wr.message = "Internal: Error: " + response.Content;
                }
                */

                SaveHitLog("S", null, null, null, "WalletDetails", restClientOptions.BaseUrl.ToString(), basicAuth, response?.Content, DateTime.Now, null);

            }
            catch (Exception ex)
            {
                wr.message = "Internal: Exception: " + ex.Message;
            }
            return wr;
        }

        public static string UpdateWalletToken(TokenResponse tr)
        {
            string result = "";
            try
            {
                TimeSpan ts = new TimeSpan(0, 0, tr.expires_in / 60, 0);
                DateTime dtExpiry = DateTime.Now.Add(ts);

                using var con = new SqlConnection(DbConnection);
                var queryParameters = new DynamicParameters();
                queryParameters.Add("@Id", "CON001");
                queryParameters.Add("@BillPost_Wallet_AccessToken", tr.access_token);
                queryParameters.Add("@BillPost_Wallet_RefreshToken", tr.refresh_token);
                queryParameters.Add("@BillPost_Wallet_TokenType", tr.token_type);
                queryParameters.Add("@BillPost_Wallet_ExpiresIn", tr.expires_in);
                queryParameters.Add("@BillPost_Wallet_ExpiresTime", dtExpiry);
                result = con.Query<string>("usp_UPPCLConfigWalletTokenUpdate", queryParameters, commandType: System.Data.CommandType.StoredProcedure).SingleOrDefault();
            }
            catch (Exception ex)
            {
                result += "Exception: " + ex.Message;
            }
            return result;
        }
        //// Wallet Code End


        //// BillPost Code Start
        public static TokenResponse BillPostToken()
        {
            TokenResponse tr = new TokenResponse();

            try
            {
                var client = new RestClient(uppclConfig.TokenUrl);
                var request = new RestRequest("", Method.Post);
                string basicAuth = "Basic " + EncodeBase64(Encoding.ASCII, uppclConfig.BillPost_BillPayment_TokenConsumerKey + ":" + uppclConfig.BillPost_BillPayment_TokenConsumerSecret);
                request.AddHeader("Authorization", basicAuth);
                var response = client.Execute<TokenResponse>(request);

                
                if (response.IsSuccessful)
                {
                    tr = JsonConvert.DeserializeObject<TokenResponse>(response.Content);
                    if (tr.error != "invalid_grant" || tr.error == "invalid_client")
                    {
                        UpdateBillPostToken(tr);
                        Initialize();
                    }
                }
                else
                {
                    tr.error_description = "Internal: Error: " + response.Content;
                }

                

                SaveHitLog("S", null, null, null, "BillPostAccess", uppclConfig.TokenUrl, basicAuth, response?.Content, DateTime.Now, null);
            }
            catch (Exception ex)
            {
                tr.error_description = "Internal: Exception: " + ex.Message;
            }
            return tr;
        }

        public static TokenResponse BillPostRefreshToken()
        {
            TokenResponse tr = new TokenResponse();

            try
            {
                string url = uppclConfig.RefreshTokenUrl.Replace("_refresh_token_",
                    uppclConfig.BillPost_BillPayment_RefreshToken);
                var client = new RestClient(url);
                var request = new RestRequest("", Method.Post);
                string basicAuth = "Basic " + EncodeBase64(Encoding.ASCII, uppclConfig.BillPost_BillPayment_TokenConsumerKey + ":" + uppclConfig.BillPost_BillPayment_TokenConsumerSecret);
                request.AddHeader("Authorization", basicAuth);
                var response = client.Execute<TokenResponse>(request);

                if (response.Content.Contains("access token data not"))
                {
                    BillPostToken();
                }

                if (response.IsSuccessful)
                {
                    tr = JsonConvert.DeserializeObject<TokenResponse>(response.Content);
                    if (tr.error != "invalid_grant" || tr.error == "invalid_client")
                    {
                        UpdateBillPostToken(tr);
                        Initialize();
                    }
                    else
                    {
                        //Refresh token invalid, so get the fresh access token and refresh token.
                        BillPostToken();
                    }
                }
                else
                {
                    tr.error_description = "Internal: Error: " + response.Content;
                }

                

                SaveHitLog("S", null, null, null, "BillPostRefresh", url, basicAuth, response?.Content, DateTime.Now, null);

            }
            catch (Exception ex)
            {
                tr.error_description = "Internal: Exception: " + ex.Message;
            }
            return tr;
        }

        public static BillPostResponse BillPostBillPayment(BillPaymentRequest billPaymentRequest, bool isOTS = false)
        {
            BillPostResponse billPostResponse = new BillPostResponse();
            RestResponse response =  new RestResponse();
            string postData = "";
            string basicAuth = "";

            try
            {
                //new RestClientOptions { Timeout = 15000, MaxTimeout = 15000}
                RestClientOptions restClientOptions = new RestClientOptions();
                restClientOptions.MaxTimeout = 15000; // 15 sec as [er discussion - 05-08-2024 old 10 sec
                restClientOptions.BaseUrl = new Uri(uppclConfig.BillPost_BillPayment_BillPostUrl);
                
                var client = new RestClient(restClientOptions);
                
                var request = new RestRequest("", Method.Post);
                request.Timeout = 15000;
                
                basicAuth = "Bearer " + uppclConfig.BillPost_BillPayment_AccessToken;
                request.AddHeader("Authorization", basicAuth);
                if (isOTS)
                {
                    postData = uppclConfig.OTS_Submit_PostData;
                }
                else
                {
                    postData = uppclConfig.BillPost_BillPayment_PostData;
                }
                postData = postData
                    .Replace("_agencyType_", billPaymentRequest.agencyType)
                    .Replace("_agency_id_", billPaymentRequest.agentId)
                    .Replace("_payable_amount_", billPaymentRequest.amount)
                    .Replace("_bill_amount_from_fetch_api_", billPaymentRequest.billAmount)
                    .Replace("_bill_id_from_fetch_api_", billPaymentRequest.billId)
                    .Replace("_connection_type_from_fetch_api_", billPaymentRequest.connectionType)
                    .Replace("_consumer_account_number_", billPaymentRequest.consumerAccountId)
                    .Replace("_consumer_name_", billPaymentRequest.consumerName)
                    .Replace("_discom_from_fetch_api_", billPaymentRequest.discom)
                    .Replace("_division_from_fetch_api_", billPaymentRequest.division)
                    .Replace("_division_code_from_fetch_api_", billPaymentRequest.divisionCode)
                    .Replace("_payer_mobile_number_", billPaymentRequest.mobile)
                    .Replace("_account_info_from_fetch_api_", billPaymentRequest.outstandingAmount)
                    .Replace("_payment_type_Full_or_PARTIAL_", billPaymentRequest.paymentType)
                    .Replace("_our_system_id_", billPaymentRequest.referenceTransactionId)
                    .Replace("_project_area_from_bill_fetch_api_", billPaymentRequest.sourceType)
                    .Replace("_project_area_from_bill_fetch_api_", billPaymentRequest.type)
                    .Replace("_van_no_", billPaymentRequest.vanNo)
                    .Replace("_our_agent_id_", billPaymentRequest.walletId)
                    .Replace("_city_", billPaymentRequest.city)
                    .Replace("_param1_", billPaymentRequest.param1)
                    .Replace("_TDStatus_", billPaymentRequest.TDStatus)
                    .Replace("_LifelineAct_", billPaymentRequest.LifelineAct);
                request.AddStringBody(postData, ContentType.Json);

                response = client.Execute<BillPostResponse>(request);


                SaveHitLog("S", null, null, billPaymentRequest?.consumerAccountId, "BillPostBillPaymen_0", uppclConfig.BillFetch_BillDetail_Url + " " + postData, basicAuth, response?.Content, DateTime.Now, null);

                var respData = response?.Content;
                if(respData != null)
                {
                    if (respData.Contains("Invalid Credentials. Make sure you have provided the correct"))
                    {
                        BillPostToken();
                    }

                    if (respData.Contains("message") )
                    {
                        billPostResponse = JsonConvert.DeserializeObject<BillPostResponse>(response.Content);
                    }else if (respData.Contains("error"))
                    {
                        billPostResponse.message = "Exception: Server: " + GetRegExFirstMatch(respData, "error\":\"(.*)\",\"me");
                    }
                    else if (respData.Contains("ams:fault"))
                    {
                        string? errorCode = GetRegExFirstMatch(respData, "ams:message>(.*)</ams:message>");
                        if (errorCode != null && errorCode == "Invalid Credentials")
                        {
                            BillPostToken();
                        }
                        billPostResponse.message = "Exception: Server: " + errorCode;
                    }
                    else
                    {
                        billPostResponse = JsonConvert.DeserializeObject<BillPostResponse>(response.Content);

                    }
                }

                string nullResponse = "";
                if(response.StatusCode == 0)
                {
                    billPostResponse.message = "Exception: Server: null response.";
                    nullResponse = "NULL_RESP";
                }

                //If Exception server returns then call force fail.

                SaveHitLog("S", billPaymentRequest.referenceTransactionId, nullResponse, billPaymentRequest?.consumerAccountId, "BillPostBillPayment", uppclConfig.BillPost_BillPayment_BillPostUrl + " " + postData, basicAuth, response?.Content, DateTime.Now, null);

            }
            catch (Exception ex)
            {
                billPostResponse.message = "Internal: Exception: " + ex.Message + Environment.NewLine + response?.Content;
            }
            finally
            {
                //SaveHitLog("S", null, null, billPaymentRequest?.consumerAccountId, "BillPostBillPayment", uppclConfig.BillFetch_BillDetail_Url + " " + postData, basicAuth, response?.Content, DateTime.Now, null);

            }
            return billPostResponse;
        }

        public static string UpdateBillPostToken(TokenResponse tr)
        {
            string result = "";
            try
            {
                TimeSpan ts = new TimeSpan(0, 0, tr.expires_in / 60, 0);
                DateTime dtExpiry = DateTime.Now.Add(ts);

                using var con = new SqlConnection(DbConnection);
                var queryParameters = new DynamicParameters();
                queryParameters.Add("@Id", "CON001");
                queryParameters.Add("@BillPost_BillPayment_AccessToken", tr.access_token);
                queryParameters.Add("@BillPost_BillPayment_RefreshToken", tr.refresh_token);
                queryParameters.Add("@BillPost_BillPayment_TokenType", tr.token_type);
                queryParameters.Add("@BillPost_BillPayment_ExpiresIn", tr.expires_in);
                queryParameters.Add("@BillPost_BillPayment_ExpiresTime", dtExpiry);
                result = con.Query<string>("usp_UPPCLConfigBillPaymentTokenUpdate", queryParameters, commandType: System.Data.CommandType.StoredProcedure).SingleOrDefault();
            }
            catch (Exception ex)
            {
                result += "Exception: " + ex.Message;
            }
            return result;
        }

        //// BillPost Code End

        //// Status Check Code Start
        public static TokenResponse StatusCheckToken()
        {
            TokenResponse tr = new TokenResponse();

            try
            {
                var client = new RestClient(uppclConfig.TokenUrl);
                var request = new RestRequest("", Method.Post);
                string basicAuth = "Basic " + EncodeBase64(Encoding.ASCII, uppclConfig.BillPost_StatusCheck_TokenConsumerKey + ":" + uppclConfig.BillPost_StatusCheck_TokenConsumerSecret);
                request.AddHeader("Authorization", basicAuth);
                var response = client.Execute<TokenResponse>(request);

                
                if (response.IsSuccessful)
                {
                    tr = JsonConvert.DeserializeObject<TokenResponse>(response.Content);
                    if (tr.error != "invalid_grant" || tr.error == "invalid_client")
                    {
                        UpdateStatusCheckToken(tr);
                        Initialize();
                    }
                }
                else
                {
                    tr.error_description = "Internal: Error: " + response.Content;
                }

                SaveHitLog("S", null, null, null, "StatusCheckAccess", uppclConfig.TokenUrl, basicAuth, response?.Content, DateTime.Now, null);
            }
            catch (Exception ex)
            {
                tr.error_description = "Internal: Exception: " + ex.Message;
            }
            return tr;
        }

        public static TokenResponse StatusCheckRefreshToken()
        {
            TokenResponse tr = new TokenResponse();

            try
            {
                var client = new RestClient(uppclConfig.RefreshTokenUrl.Replace("_refresh_token_", uppclConfig.BillPost_StatusCheck_RefreshToken));
                var request = new RestRequest("", Method.Post);
                string basicAuth = "Basic " + EncodeBase64(Encoding.ASCII, uppclConfig.BillPost_StatusCheck_TokenConsumerKey + ":" + uppclConfig.BillPost_StatusCheck_TokenConsumerSecret);
                request.AddHeader("Authorization", basicAuth);
                var response = client.Execute<TokenResponse>(request);

                if (response.Content.Contains("access token data not"))
                {
                    StatusCheckToken();
                }

                if (response.IsSuccessful)
                {
                    tr = JsonConvert.DeserializeObject<TokenResponse>(response.Content);
                    if (tr.error != "invalid_grant" || tr.error == "invalid_client")
                    {
                        UpdateStatusCheckToken(tr);
                        Initialize();
                    }
                    else
                    {
                        //Refresh token invalid, so get the fresh access token and refresh token.
                        StatusCheckToken();
                    }
                }
                else
                {
                    tr.error_description = "Internal: Error: " + response.Content;
                }

                SaveHitLog("S", null, null, null, "StatusCheckRefresh", uppclConfig.TokenUrl, basicAuth, response?.Content, DateTime.Now, null);

            }
            catch (Exception ex)
            {
                tr.error_description = "Internal: Exception: " + ex.Message;
            }
            return tr;
        }

        public static StatusCheckResponse StatusCheck(string billId, string consumerNumber, string vanNo, string refid="")
        {
            StatusCheckResponse statusCheckResponse = new StatusCheckResponse();

            try
            {
                //var client = new RestClient(uppclConfig.BillPost_StatusCheck_ApiUrl);
                
                RestClientOptions restClientOptions = new RestClientOptions();
                restClientOptions.MaxTimeout = 20000; // 20 sec as per discussion
                restClientOptions.BaseUrl = new Uri(uppclConfig.BillPost_StatusCheck_ApiUrl);
                
                var client = new RestClient(restClientOptions);
                var request = new RestRequest("", Method.Post);
                request.Timeout = 20000;
                
                string basicAuth = "Bearer " + uppclConfig.BillPost_StatusCheck_AccessToken;
                request.AddHeader("Authorization", basicAuth);
                string postData = uppclConfig.BillPost_StatusCheck_PostData
                                    .Replace("_bill_id_from_fetch_api_", billId.Trim())
                                    .Replace("_consumer_account_number_", consumerNumber.Trim())
                                    .Replace("_van_no_", vanNo.Trim())
                                    .Replace("_refid_", refid.Trim());
                request.AddStringBody(postData, ContentType.Json);

                var response = client.Execute<StatusCheckResponse>(request);

                SaveHitLog("S", null, null, consumerNumber.Trim(), "StatusCheck_0", uppclConfig.BillPost_StatusCheck_ApiUrl, basicAuth + " " + postData, response?.Content, DateTime.Now, null);

                var respData = response?.Content;
                if (respData != null)
                {
                    if (respData.Contains("status"))
                    {
                        statusCheckResponse = JsonConvert.DeserializeObject<StatusCheckResponse>(response.Content);
                    }
                    else if (respData.Contains("error"))
                    {
                        statusCheckResponse.message = "Exception: Server: " + GetRegExFirstMatch(respData, "error\":\"(.*)\",\"me");
                    }
                    else if (respData.Contains("am:fault"))
                    {
                        string? errorCode = GetRegExFirstMatch(respData, "am:message>(.*)</am:message>");
                        statusCheckResponse.message = "Exception: Server: " + errorCode;
                    }
                    else if (respData.Contains("ams:fault"))
                    {
                        string? errorCode = GetRegExFirstMatch(respData, "ams:message>(.*)</ams:message>");
                        if (errorCode != null && errorCode == "Invalid Credentials")
                        {
                            StatusCheckToken();
                        }
                        statusCheckResponse.message = "Exception: Server: " + errorCode;
                    }
                }
                else
                {
                    statusCheckResponse.message = "Exception: Server: null response.";
                }

                string nullResponse = "";
                if (response.StatusCode == 0)
                {
                    statusCheckResponse.message = "Exception: Server: null response.";
                    nullResponse = "NULL_RESP";
                }

                SaveHitLog("S", null, null, consumerNumber.Trim(), "StatusCheck", uppclConfig.BillPost_StatusCheck_ApiUrl, basicAuth + " " + postData, response?.Content, DateTime.Now, null);

            }
            catch (Exception ex)
            {
                statusCheckResponse.message = "Exception: Internal: " + ex.Message;
            }
            return statusCheckResponse;
        }

        public static string UpdateStatusCheckToken(TokenResponse tr)
        {
            string result = "";
            try
            {
                TimeSpan ts = new TimeSpan(0, 0, tr.expires_in / 60, 0);
                DateTime dtExpiry = DateTime.Now.Add(ts);

                using var con = new SqlConnection(DbConnection);
                var queryParameters = new DynamicParameters();
                queryParameters.Add("@Id", "CON001");
                queryParameters.Add("@BillPost_StatusCheck_AccessToken", tr.access_token);
                queryParameters.Add("@BillPost_StatusCheck_RefreshToken", tr.refresh_token);
                queryParameters.Add("@BillPost_StatusCheck_TokenType", tr.token_type);
                queryParameters.Add("@BillPost_StatusCheck_ExpiresIn", tr.expires_in);
                queryParameters.Add("@BillPost_StatusCheck_ExpiresTime", dtExpiry);
                result = con.Query<string>("usp_UPPCLConfigStatusCheckTokenUpdate", queryParameters, commandType: System.Data.CommandType.StoredProcedure).SingleOrDefault();
            }
            catch (Exception ex)
            {
                result += "Exception: " + ex.Message;
            }
            return result;
        }
        //// Status Check Code End


        //// ForceFail Code Start
        public static TokenResponse ForceFailToken()
        {
            TokenResponse tr = new TokenResponse();

            try
            {
                var client = new RestClient(uppclConfig.TokenUrl);
                var request = new RestRequest("", Method.Post);
                string basicAuth = "Basic " + EncodeBase64(Encoding.ASCII, uppclConfig.Forcefail_TokenConsumerKey + ":" + uppclConfig.Forcefail_TokenConsumerSecret);
                request.AddHeader("Authorization", basicAuth);
                var response = client.Execute<TokenResponse>(request);

                
                if (response.IsSuccessful)
                {
                    tr = JsonConvert.DeserializeObject<TokenResponse>(response.Content);
                    if (tr.error != "invalid_grant" || tr.error == "invalid_client")
                    {
                        UpdateForceFailToken(tr);
                        Initialize();
                    }
                }
                else
                {
                    tr.error_description = "Internal: Error: " + response.Content;
                }

                SaveHitLog("S", null, null, null, "ForceFailAccess", uppclConfig.TokenUrl, basicAuth, response?.Content, DateTime.Now, null);
            }
            catch (Exception ex)
            {
                tr.error_description = "Internal: Exception: " + ex.Message;
            }
            return tr;
        }

        public static TokenResponse ForceFailRefreshToken()
        {
            TokenResponse tr = new TokenResponse();

            try
            {
                var client = new RestClient(uppclConfig.RefreshTokenUrl.Replace("_refresh_token_", uppclConfig.Forcefail_RefreshToken));
                var request = new RestRequest("", Method.Post);
                string basicAuth = "Basic " + EncodeBase64(Encoding.ASCII, uppclConfig.Forcefail_TokenConsumerKey + ":" + uppclConfig.Forcefail_TokenConsumerSecret);
                request.AddHeader("Authorization", basicAuth);
                var response = client.Execute<TokenResponse>(request);

                if (response.Content.Contains("access token data not"))
                {
                    ForceFailToken();
                }

                if (response.IsSuccessful)
                {
                    tr = JsonConvert.DeserializeObject<TokenResponse>(response.Content);
                    if (tr.error != "invalid_grant" || tr.error == "invalid_client")
                    {
                        UpdateForceFailToken(tr);
                        Initialize();
                    }
                    else
                    {
                        //Refresh token invalid, so get the fresh access token and refresh token.
                        ForceFailToken();
                    }
                }
                else
                {
                    tr.error_description = "Internal: Error: " + response.Content;
                }

                SaveHitLog("S", null, null, null, "ForceFailRefresh", uppclConfig.TokenUrl, basicAuth, response?.Content, DateTime.Now, null);

            }
            catch (Exception ex)
            {
                tr.error_description = "Internal: Exception: " + ex.Message;
            }
            return tr;
        }

        public static ForceFailResponse ForceFail(ForceFailRequest forceFailRequest)
        {
            ForceFailResponse forceFailResponse = new ForceFailResponse();
            string postData = "";

            try
            {
                var client = new RestClient(uppclConfig.Forcefail_FailUrl);
                var request = new RestRequest("", Method.Post);
                string basicAuth = "Bearer " + uppclConfig.Forcefail_AccessToken;
                request.AddHeader("Authorization", basicAuth);
                postData = uppclConfig.Forcefail_PostData
                                    .Replace("_paymentSource_", forceFailRequest.paymentSource)
                                    .Replace("_agencyType_", forceFailRequest.agencyType)
                                    .Replace("_payable_amount_", forceFailRequest.amount)
                                    .Replace("_bill_id_from_fetch_api_", forceFailRequest.billId)
                                    .Replace("_consumer_account_number_", forceFailRequest.consumerId)
                                    .Replace("_our_system_id_", forceFailRequest.referenceTransactionId)
                                    .Replace("_transaction_date_YYYY-MM-DD_", forceFailRequest.transactionDate)
                                    .Replace("_transactionStatus_", forceFailRequest.transactionStatus)
                                    .Replace("_van_no_", forceFailRequest.vanNo);
                request.AddStringBody(postData, ContentType.Json);

                var response = client.Execute<ForceFailResponse>(request);
                SaveHitLog("S", null, null, forceFailRequest.consumerId, "ForceFailPayment_0", uppclConfig.BillFetch_BillDetail_Url + " " + postData, basicAuth, response?.Content, DateTime.Now, null);

                forceFailResponse = JsonConvert.DeserializeObject<ForceFailResponse>(response?.Content);

                using (var con = new SqlConnection(UPPCLManager.DbConnection))
                {
                    try
                    {
                        var queryParameters = new DynamicParameters();
                        queryParameters.Add("@RTranId", forceFailRequest.referenceTransactionId);
                        queryParameters.Add("@ConsumerNo", forceFailRequest.consumerId);
                        queryParameters.Add("@ForceFailDate", DateTime.Now);
                        queryParameters.Add("@ForceFailResponse", JsonConvert.SerializeObject(forceFailResponse));
                        queryParameters.Add("@ForceFailResult", forceFailResponse?.code.ToString());
                        con.Execute("usp_ForceFailLogInsert", queryParameters, commandType: System.Data.CommandType.StoredProcedure);

                    }
                    catch (Exception ex)
                    {
                        string error = ex.Message;
                    }
                    finally
                    {
                    }
                }

                var respData = response?.Content;
                if (respData != null)
                {
                    if (respData.Contains("ams:fault"))
                    {
                        string? errorCode = GetRegExFirstMatch(respData, "ams:message>(.*)</ams:message>");
                        if (errorCode != null && errorCode == "Invalid Credentials")
                        {
                            ForceFailToken();
                        }
                        forceFailResponse.message = "Exception: Server: " + errorCode;
                    }
                }
                else
                {
                    forceFailResponse.message = "Exception: Server: null response.";
                }

                SaveHitLog("S", null, null, forceFailRequest.consumerId, "ForceFailPayment", uppclConfig.BillFetch_BillDetail_Url + " " + postData, basicAuth, response?.Content, DateTime.Now, null);

            }
            catch (Exception ex)
            {
                //forceFailResponse.message += "Internal: Exception: " + ex.Message;
                forceFailResponse.message += "Exception: Internal: " + ex.Message + Environment.NewLine + postData;
            }
            return forceFailResponse;
        }

        public static string UpdateForceFailToken(TokenResponse tr)
        {
            string result = "";
            try
            {
                TimeSpan ts = new TimeSpan(0, 0, tr.expires_in / 60, 0);
                DateTime dtExpiry = DateTime.Now.Add(ts);

                using var con = new SqlConnection(DbConnection);
                var queryParameters = new DynamicParameters();
                queryParameters.Add("@Id", "CON001");
                queryParameters.Add("@Forcefail_AccessToken", tr.access_token);
                queryParameters.Add("@Forcefail_RefreshToken", tr.refresh_token);
                queryParameters.Add("@Forcefail_TokenType", tr.token_type);
                queryParameters.Add("@Forcefail_ExpiresIn", tr.expires_in);
                queryParameters.Add("@Forcefail_ExpiresTime", dtExpiry);
                result = con.Query<string>("usp_UPPCLConfigForceFailTokenUpdate", queryParameters, commandType: System.Data.CommandType.StoredProcedure).SingleOrDefault();
            }
            catch (Exception ex)
            {
                result += "Exception: " + ex.Message;
            }
            return result;
        }

        //// ForceFail Code End


        //Token Expiry Code STart

        public static TokenExpiry TokenExpiryDetail()
        {
            TokenExpiry tokenExpiry = new TokenExpiry();

            try
            {
                using var con = new SqlConnection(DbConnection);
                var queryParameters = new DynamicParameters();
                tokenExpiry = con.Query<TokenExpiry>("usp_TokenExpiry", queryParameters, commandType: System.Data.CommandType.StoredProcedure).SingleOrDefault();
            }
            catch (Exception ex)
            {
                tokenExpiry.Message = "Errors: Exception: " + ex.Message;
            }
            return tokenExpiry;
        }

        //Token Expiry Code End


        public static string SaveHitLog(string AppSource, string RTranId, string RetailerId, string ConsumerNumber, string ApiType, string ApiUrl, string RequestData, string ResponseData, DateTime CreateDate, string CientIp)
        {
            string result = "";
            try
            {
                using var con = new SqlConnection(DbConnection);
                var queryParameters = new DynamicParameters();
                queryParameters.Add("@AppSource", AppSource);
                queryParameters.Add("@RTranId", RTranId);
                queryParameters.Add("@RetailerId", RetailerId);
                queryParameters.Add("@ConsumerNumber", ConsumerNumber);
                queryParameters.Add("@ApiType", ApiType);
                queryParameters.Add("@ApiUrl", ApiUrl);
                queryParameters.Add("@RequestData", RequestData);
                queryParameters.Add("@ResponseData", ResponseData);
                queryParameters.Add("@CreateDate", CreateDate);
                queryParameters.Add("@CientIp", CientIp);
                result = con.Query<string>("usp_UPPCLHitLogInsert", queryParameters, commandType: System.Data.CommandType.StoredProcedure).SingleOrDefault();
            }
            catch (Exception ex)
            {
                result += "Exception: " + ex.Message;
            }
            return result;
        }

        public static string TransactionsStatusCheck()
        {
            string result = "Status check started";
            try
            {
                using (var con = new SqlConnection(UPPCLManager.DbConnection))
                {
                    var queryParameters = new DynamicParameters();
                    List<RTran> rt = con.Query<RTran>("usp_RTranForStatusCheck", queryParameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    foreach (RTran tran in rt)
                    {
                        var statusResponse = tran.StatusCheck(true);
                        result += Environment.NewLine + tran.RechargeMobileNumber + " - " + tran.UPPCL_BillId + " - " + tran.Amount.ToString() + " - " + statusResponse.status + statusResponse.message;
                    }
                }
            }
            catch (Exception ex)
            {
                result += " Exception: " + ex.Message;
            }

            return result;
        }


        public static void CheckTokenExpiry()
        {
            try
            {
                TokenExpiry tokenExpiry = UPPCLManager.TokenExpiryDetail();
                if (tokenExpiry.Message.Contains("Error"))
                {
                    //MyLog("Error: In token expiry manager. " + tokenExpiry.Message, false);
                }
                else
                {
                    if (tokenExpiry.Discom < 0)
                    {
                        UPPCLManager.DiscomListToken();
                    }
                    else if(tokenExpiry.Discom < 15)
                    {
                        UPPCLManager.DiscomListRefreshToken();
                    }

                    if (tokenExpiry.BillDetail < 0)
                    {
                        UPPCLManager.BillFetchToken();
                    }
                    else if(tokenExpiry.BillDetail < 15)
                    {
                        UPPCLManager.BillFetchRefreshToken();
                    }

                    if (tokenExpiry.BillPostWallet < 0)
                    {
                         UPPCLManager.WalletToken();
                    }
                    else if (tokenExpiry.BillPostWallet < 15)
                    {
                        UPPCLManager.WalletRefreshToken();
                    }

                    if (tokenExpiry.BillPostPayment < 0)
                    {
                        UPPCLManager.BillPostToken();
                    }
                    else if (tokenExpiry.BillPostPayment < 15)
                    {
                        UPPCLManager.BillPostRefreshToken();
                    }

                    if (tokenExpiry.BillPostStatusCheck < 0)
                    {
                        UPPCLManager.StatusCheckToken();
                    }
                    else if(tokenExpiry.BillPostStatusCheck < 15)
                    {
                        UPPCLManager.StatusCheckRefreshToken();
                    }

                    if (tokenExpiry.Forcefail < 0)
                    {
                        UPPCLManager.ForceFailToken();
                    }
                    else if (tokenExpiry.Forcefail < 15)
                    {
                        UPPCLManager.ForceFailRefreshToken();
                    }

                    if (tokenExpiry.OTS < 0)
                    {
                        UPPCLManager.OTSToken();
                    }
                    else if (tokenExpiry.OTS < 15)
                    {
                        UPPCLManager.OTSRefreshToken();
                    }

                    //MyLog(JsonConvert.SerializeObject(UPPCLManager.TokenExpiryDetail()), false);
                }
            }
            catch (Exception ex)
            {
                //MyLog("Errors: Exception: In check token expiry. " + ex.Message, false);
            }
        }

        public static string EncodeBase64(this System.Text.Encoding encoding, string text)
        {
            if (text == null)
            {
                return null;
            }

            byte[] textAsBytes = encoding.GetBytes(text);
            return System.Convert.ToBase64String(textAsBytes);
        }

        public static string DecodeBase64(this System.Text.Encoding encoding, string encodedText)
        {
            if (encodedText == null)
            {
                return null;
            }

            byte[] textAsBytes = System.Convert.FromBase64String(encodedText);
            return encoding.GetString(textAsBytes);
        }

        public static string CurrentDate(DateTime dateTime)
        {
            //return DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.CreateSpecificCulture("en-US"));
            return dateTime.ToString("yyyy-MM-dd", CultureInfo.CreateSpecificCulture("en-US"));
        }

        public static String GetRegExFirstMatch(string inputString, string regexValidation)
        {
            string result = String.Empty;

            try
            {
                Match match = Regex.Match(inputString, regexValidation, RegexOptions.None);
                if (match.Success)
                {
                    result = match.Groups[1].Value;
                }
            }
            catch (Exception exReg)
            {
                result = "ExceptionRegEx: " + exReg.Message;
            }


            return result;
        }

        public static TokenResponse OTSToken()
        {
            TokenResponse tr = new TokenResponse();

            try
            {
                TokenExpiry tokenExpiry = UPPCLManager.TokenExpiryDetail();
                if (tokenExpiry.OTS < 0)
                {
                    var client = new RestClient(uppclConfig.TokenUrl);
                    var request = new RestRequest("", Method.Post);
                    string basicAuth = "Basic " + EncodeBase64(Encoding.ASCII, uppclConfig.OTS_Consumer_key + ":" + uppclConfig.OTS_Consumer_Secret);
                    request.AddHeader("Authorization", basicAuth);
                    var response = client.Execute<TokenResponse>(request);

                    if (response.IsSuccessful)
                    {
                        tr = JsonConvert.DeserializeObject<TokenResponse>(response.Content);
                        if (tr.error != "invalid_grant" || tr.error == "invalid_client")
                        {
                            UpdateOTSToken(tr);
                            Initialize();
                        }
                    }
                    else
                    {
                        tr.error_description = "Internal: Error: " + response.Content;
                    }


                    SaveHitLog("S", null, null, null, "OTSAccess", uppclConfig.TokenUrl, basicAuth, response?.Content, DateTime.Now, null);
                }
            }
            catch (Exception ex)
            {
                tr.error_description = "Internal: Exception: " + ex.Message;
            }
            return tr;
        }

        public static TokenResponse OTSRefreshToken()
        {
            TokenResponse tr = new TokenResponse();
            //string tr = "";

            try
            {
                TokenExpiry tokenExpiry = UPPCLManager.TokenExpiryDetail();
                if (tokenExpiry.OTS < 15)
                {
                    var url = uppclConfig.RefreshTokenUrl.Replace("_refresh_token_", uppclConfig.OTS_RefreshToken);
                    var client = new RestClient(uppclConfig.RefreshTokenUrl.Replace("_refresh_token_", uppclConfig.OTS_RefreshToken));
                    var request = new RestRequest("", Method.Post);
                    string basicAuth = "Basic " + EncodeBase64(Encoding.ASCII, uppclConfig.OTS_Consumer_key + ":" + uppclConfig.OTS_Consumer_Secret);
                    request.AddHeader("Authorization", basicAuth);
                    var response = client.Execute<TokenResponse>(request);

                    if (response.Content.Contains("access token data not"))
                    {
                        OTSToken();
                    }

                    if (response.IsSuccessful)
                    {
                        tr = JsonConvert.DeserializeObject<TokenResponse>(response.Content);
                        if (tr.error != "invalid_grant" || tr.error == "invalid_client")
                        {
                            UpdateOTSToken(tr);
                            Initialize();
                        }
                        else
                        {
                            //Refresh token invalid, so get the fresh access token and refresh token.
                            OTSToken();
                        }
                    }
                    else
                    {
                        tr.error_description = "Internal: Error: " + response.Content;
                    }

                    SaveHitLog("S", null, null, null, "OTSRefreshToken", url, basicAuth, response?.Content, DateTime.Now, null);
                }

            }
            catch (Exception ex)
            {
                tr.error_description = "Internal: Exception: " + ex.Message;
            }            
            return tr;
        }

        public static string UpdateOTSToken(TokenResponse tr)
        {
            string result = "";
            try
            {
                TimeSpan ts = new TimeSpan(0, 0, tr.expires_in / 60, 0);
                DateTime dtExpiry = DateTime.Now.Add(ts);

                using var con = new SqlConnection(DbConnection);
                var queryParameters = new DynamicParameters();
                queryParameters.Add("@Id", "CON001");
                queryParameters.Add("@OTS_AccessToken", tr.access_token);
                queryParameters.Add("@OTS_RefreshToken", tr.refresh_token);
                queryParameters.Add("@OTS_TokenType", tr.token_type);
                queryParameters.Add("@OTS_ExpiresIn", tr.expires_in);
                queryParameters.Add("@OTS_ExpiresTime", dtExpiry);
                result = con.Query<string>("usp_UPPCLConfigOTSTokenUpdate", queryParameters, commandType: System.Data.CommandType.StoredProcedure).SingleOrDefault();
            }
            catch (Exception ex)
            {
                result += "Exception: " + ex.Message;
            }
            return result;
        }

        public static CheckEligibility CheckEligibility(string divisionName, string consumerNumber)
        {
            var br = new CheckEligibility();

            try
            {
                string url = uppclConfig.OTS_EligibilityCheck_Url.Replace("_discomName_", divisionName).Replace("_accountNo_", consumerNumber.Trim());
                var client = new RestClient(url);
                var request = new RestRequest("", Method.Get);
                string basicAuth = "Bearer " + uppclConfig.OTS_AccessToken;
                request.AddHeader("Authorization", basicAuth);

                var response = client.Execute<CheckEligibility>(request);

                SaveHitLog("S", null, null, consumerNumber, "OTS_CheckElgibility_0", url, basicAuth, response?.Content, DateTime.Now, null);

                if (response.IsSuccessful)
                {
                    br = JsonConvert.DeserializeObject<CheckEligibility>(response.Content);
                    if(br != null && br.Data != null && br.Data.Status == "error")
                    {
                        br.Status = br.Data.Status;
                        br.Message = br.Data.Message;
                    }
                }
                else
                {
                    try
                    {
                        br = JsonConvert.DeserializeObject<CheckEligibility>(response.Content);
                        var respData = response?.Content;
                        if (respData != null && respData.Contains("Invalid Credentials. Make sure you have provided the correct"))
                        {
                            OTSToken();
                            if (br == null || string.IsNullOrEmpty(br.Status) || string.IsNullOrEmpty(br.Message))
                            {
                                if (br == null)
                                {
                                    br = new CheckEligibility();
                                }
                                br.Status = "error";
                                br.Message = "Invalid Credentials";
                            }
                        }
                        else if (respData != null && respData.Contains("ams:fault"))
                        {
                            string? errorCode = GetRegExFirstMatch(respData, "ams:message>(.*)</ams:message>");
                            if (errorCode != null && errorCode == "Invalid Credentials")
                            {
                                OTSToken();
                                if (br == null || string.IsNullOrEmpty(br.Status) || string.IsNullOrEmpty(br.Message))
                                {
                                    if (br == null)
                                    {
                                        br = new CheckEligibility();
                                    }
                                    br.Status = "error";
                                    br.Message = "Invalid Credentials";
                                }
                            }
                        }
                        if (br == null || string.IsNullOrEmpty(br.Status) || string.IsNullOrEmpty(br.Message))
                        {
                            if(br == null)
                            {
                                br = new CheckEligibility();
                            }
                            br.Status = "error";
                            br.Message = "Failed to check eligibility";
                        }

                        if (br != null && br.Data != null && br.Data.Status == "error")
                        {
                            br.Status = br.Data.Status;
                            br.Message = br.Data.Message;
                        }

                        if (br != null && br.Data != null && br.Data.Result == "False")
                        {
                            br.Status = "error";
                            br.Message = "Account Id is not eligible for OTS.";
                        }
                    }
                    catch
                    {
                        if (br == null)
                        {
                            br = new CheckEligibility();
                        }
                        br.Status = "error";
                        br.Message = "Failed to check eligibility";
                    }
                }

                SaveHitLog("S", null, null, consumerNumber, "OTS_CheckElgibility", url, basicAuth, response?.Content, DateTime.Now, null);

            }
            catch (Exception ex)
            {
                if (br == null)
                {
                    br = new CheckEligibility();
                }
                br.Status = "error";
                br.Message = ex.Message;
            }
            return br;
        }

        public static AmountDetails GetAmountDetails(string divisionName, string consumerNumber)
        {
            var br = new AmountDetails();

            try
            {
                string url = uppclConfig.OTS_AmountDetails_Url.Replace("_discomName_", divisionName).Replace("_accountNo_", consumerNumber.Trim());
                var client = new RestClient(url);
                var request = new RestRequest("", Method.Get);
                string basicAuth = "Bearer " + uppclConfig.OTS_AccessToken;
                request.AddHeader("Authorization", basicAuth);

                var response = client.Execute<AmountDetails>(request);

                SaveHitLog("S", null, null, consumerNumber, "OTS_AmountDetails_0", url, basicAuth, response?.Content, DateTime.Now, null);

                if (response.IsSuccessful)
                {
                    br = JsonConvert.DeserializeObject<AmountDetails>(response.Content);
                    if (br != null && br.Data != null && br.Data.Status == "error")
                    {
                        br.Status = br.Data.Status;
                        br.Message = br.Data.Message;
                    }
                }
                else
                {
                    try
                    {
                        br = JsonConvert.DeserializeObject<AmountDetails>(response.Content);
                        var respData = response?.Content;
                        if (respData != null && respData.Contains("Invalid Credentials. Make sure you have provided the correct"))
                        {
                            OTSToken();
                            if (br == null || string.IsNullOrEmpty(br.Status) || string.IsNullOrEmpty(br.Message))
                            {
                                if (br == null)
                                {
                                    br = new AmountDetails();
                                }
                                br.Status = "error";
                                br.Message = "Invalid Credentials";
                            }
                        }
                        else if (respData != null && respData.Contains("ams:fault"))
                        {
                            string? errorCode = GetRegExFirstMatch(respData, "ams:message>(.*)</ams:message>");
                            if (errorCode != null && errorCode == "Invalid Credentials")
                            {
                                OTSToken();
                                if (br == null || string.IsNullOrEmpty(br.Status) || string.IsNullOrEmpty(br.Message))
                                {
                                    if (br == null)
                                    {
                                        br = new AmountDetails();
                                    }
                                    br.Status = "error";
                                    br.Message = "Invalid Credentials";
                                }
                            }
                        }
                        if (br == null || string.IsNullOrEmpty(br.Status) || string.IsNullOrEmpty(br.Message))
                        {
                            if (br == null)
                            {
                                br = new AmountDetails();
                            }
                            br.Status = "error";
                            br.Message = "Failed to get Amount Details";
                        }

                        if (br != null && br.Data != null && br.Data.Status == "error")
                        {
                            br.Status = br.Data.Status;
                            br.Message = br.Data.Message;
                        }
                    }
                    catch
                    {
                        if (br == null)
                        {
                            br = new AmountDetails();
                        }
                        br.Status = "error";
                        br.Message = "Failed to get Amount Details";
                    }                    
                }

                SaveHitLog("S", null, null, consumerNumber, "OTS_AmountDetails", url, basicAuth, response?.Content, DateTime.Now, null);

            }
            catch (Exception ex)
            {
                if (br == null)
                {
                    br = new AmountDetails();
                }
                br.Status = "error";
                br.Message = ex.Message;
            }
            return br;
        }

        public static CaseInitResponse InitiateOTSCase(string divisionName, string consumerNumber, bool isFull, string amount)
        {
            var br = new CaseInitResponse();
            try
            {
                var amountDetails = GetAmountDetails(divisionName, consumerNumber);
                decimal downPayment;
                if (isFull)
                {
                    var diff = Convert.ToDecimal(amount) - amountDetails.Data.FullPaymentList[0].RegistrationAmount;
                    downPayment = amountDetails.Data.FullPaymentList[0].Downpayment - diff;
                    if (downPayment < 0)
                    {
                        downPayment = 0;
                    }
                }
                else
                {
                    downPayment = amountDetails.Data.InstallmentList1[0].Downpayment;
                }
                
                var postData = uppclConfig.OTS_Init_PostData
                                    .Replace("_account_", consumerNumber)
                                    .Replace("_discom_", divisionName)
                                    .Replace("_lpscAmount_", amountDetails.Data.LPSC31.ToString())
                                    .Replace("_supplyTypeCode_", amountDetails.Data.SupplyType)
                                    .Replace("_totalOutstandingAmt_", amountDetails.Data.TotoalOutStandingAmount.ToString())
                                    .Replace("_principalAmt_", amountDetails.Data.Payment31.ToString())
                                    .Replace("_registrationFee_", isFull ? amount : amountDetails.Data.InstallmentList1[0].RegistrationAmount.ToString())
                                    .Replace("_downPayment_", downPayment.ToString())
                                    .Replace("_existingLoad_", amountDetails.Data.SanctionLoad.ToString())
                                    .Replace("_installmentAmt_", isFull ? "0" : amountDetails.Data.InstallmentList1[0].InstallmentAmount.ToString())
                                    .Replace("_noOfInstallment_", isFull ? "0" : amountDetails.Data.InstallmentList1[0].NoOfInstallments.ToString())
                                    .Replace("_registrationOption_", "SARAL")
                                    .Replace("_lpscWaiveOff_", isFull ? amountDetails.Data.FullPaymentList[0].LPSCWaivOff.ToString() : amountDetails.Data.InstallmentList1[0].LPSCWaivOff.ToString());
                var client = new RestClient(uppclConfig.OTS_Init_Url);
                var request = new RestRequest("", Method.Post);
                request.AddStringBody(postData, ContentType.Json);
                string basicAuth = "Bearer " + uppclConfig.OTS_AccessToken;
                request.AddHeader("Authorization", basicAuth);

                var response = client.Execute<CaseInitResponse>(request);

                SaveHitLog("S", null, null, consumerNumber, "OTS_CaseInit_0", uppclConfig.OTS_Init_Url, basicAuth, response?.Content, DateTime.Now, null);

                if (response.IsSuccessful)
                {
                    br = JsonConvert.DeserializeObject<CaseInitResponse>(response.Content);
                    if (br != null && br.Data != null && br.Data.Status == "error")
                    {
                        br.Status = br.Data.Status;
                        br.Message = br.Data.Message;
                    }
                }
                else
                {
                    try
                    {
                        br = JsonConvert.DeserializeObject<CaseInitResponse>(response.Content);
                        var respData = response?.Content;
                        if (respData != null && respData.Contains("Invalid Credentials. Make sure you have provided the correct"))
                        {
                            OTSToken();
                            if (br == null || string.IsNullOrEmpty(br.Status) || string.IsNullOrEmpty(br.Message))
                            {
                                if (br == null)
                                {
                                    br = new CaseInitResponse();
                                }
                                br.Status = "error";
                                br.Message = "Invalid Credentials";
                            }
                        }
                        else if (respData != null && respData.Contains("ams:fault"))
                        {
                            string? errorCode = GetRegExFirstMatch(respData, "ams:message>(.*)</ams:message>");
                            if (errorCode != null && errorCode == "Invalid Credentials")
                            {
                                OTSToken();
                                if (br == null || string.IsNullOrEmpty(br.Status) || string.IsNullOrEmpty(br.Message))
                                {
                                    if (br == null)
                                    {
                                        br = new CaseInitResponse();
                                    }
                                    br.Status = "error";
                                    br.Message = "Invalid Credentials";
                                }
                            }
                        }
                        if (br == null || string.IsNullOrEmpty(br.Status) || string.IsNullOrEmpty(br.Message))
                        {
                            if (br == null)
                            {
                                br = new CaseInitResponse();
                            }
                            br.Status = "error";
                            br.Message = "Failed to initiate OTS case";
                        }

                        if (br != null && br.Data != null && br.Data.Status == "error")
                        {
                            br.Status = br.Data.Status;
                            br.Message = br.Data.Message;
                        }

                        if (br != null && br.Data != null && br.Data.ResMsg.ToLower().StartsWith("Eerror"))
                        {
                            br.Status = "error";
                            br.Message = br.Data.ResMsg;
                        }
                    }
                    catch
                    {
                        if (br == null)
                        {
                            br = new CaseInitResponse();
                        }
                        br.Status = "error";
                        br.Message = "Failed to initiate OTS case";
                    }
                }

                SaveHitLog("S", null, null, consumerNumber, "OTS_CaseInit", uppclConfig.OTS_Init_Url, basicAuth, response?.Content, DateTime.Now, null);

            }
            catch (Exception ex)
            {
                if (br == null)
                {
                    br = new CaseInitResponse();
                }
                br.Status = "error";
                br.Message = ex.Message;
            }
            return br;
        }
       
    }
}
