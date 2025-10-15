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
using UPPCLLibrary.AgentCreation;
using UPPCLLibrary.AgentActiveInActive;
using Microsoft.VisualBasic.ApplicationServices;
using UPPCLLibrary.EventResponseType.WalletTransfer;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;


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
                    if (!string.IsNullOrEmpty(uppclConfig?.AgencyID))
                    {
                        uppclConfig.TokenUrl = uppclConfig.TokenUrl.Replace("_token_user_name_", uppclConfig.Token_ApiUsername).Replace("_token_password_", uppclConfig.Token_ApiPassword);
                        //suppclConfig.RefreshTokenUrl = uppclConfig.RefreshTokenUrl;
                    }

                    if (!string.IsNullOrEmpty(uppclConfig?.AgencyID))
                    {
                        uppclConfig.AgentStatusByMobile_Url = uppclConfig.AgentStatusByMobile_Url.Replace("_agencyId_", uppclConfig.AgencyID);
                    }

                    CheckTokenExpiry();

                    return (JsonConvert.SerializeObject(uppclConfig));
                }
            }
            catch (Exception ex)
            {
                result = "Exception in config Initialize: " + ex.Message;
            }
            return result;
        }


        public static string SSO_Url(RetailUser retailUser)
        {
            string result = "";
            try
            {

            }catch(Exception ex)
            {
                result = "Error: " + ex.Message;
            }
            return result;
        }


        /// <summary>
        /// Event Code Start
        /// </summary>
        /// <returns></returns>

        public static TokenResponse EventToken()
        {
            TokenResponse tr = new TokenResponse();

            try
            {
                var client = new RestClient(uppclConfig.TokenUrl);
                var request = new RestRequest("", Method.Post);
                string basicAuth = "Basic " + EncodeBase64(Encoding.ASCII, uppclConfig.Event_TokenConsumerKey + ":" + uppclConfig.Event_TokenConsumerSecret);
                request.AddHeader("Authorization", basicAuth);
                var response = client.Execute<TokenResponse>(request);

                tr = JsonConvert.DeserializeObject<TokenResponse>(response.Content);
                if (tr.error != "invalid_grant" || tr.error == "invalid_client")
                {
                    UpdateEventToken(tr);
                    Initialize();
                }

                SaveHitLog("S", null, null, null, "Event", uppclConfig.TokenUrl, basicAuth, response?.Content, DateTime.Now, null);

            }
            catch (Exception ex)
            {
                tr.error_description = "Internal: Exception: " + ex.Message;
            }
            return tr;
        }

        public static TokenResponse EventRefreshToken()
        {
            TokenResponse tr = new TokenResponse();

            try
            {
                var client = new RestClient(uppclConfig.RefreshTokenUrl.Replace("_refresh_token_", uppclConfig.Event_RefreshToken));
                var request = new RestRequest("", Method.Post);
                string basicAuth = "Basic " + EncodeBase64(Encoding.ASCII, uppclConfig.Event_TokenConsumerKey + ":" + uppclConfig.Event_TokenConsumerSecret);
                request.AddHeader("Authorization", basicAuth);
                var response = client.Execute<TokenResponse>(request);

                if (response.Content.Contains("access token data not"))
                {
                    EventToken();
                }

                if (response.IsSuccessful)
                {
                    tr = JsonConvert.DeserializeObject<TokenResponse>(response.Content);
                    if (tr.error != "invalid_grant" || tr.error == "invalid_client")
                    {
                        UpdateEventToken(tr);
                        Initialize();
                    }
                    else
                    {
                        //Refresh token invalid, so get the fresh access token and refresh token.
                        EventToken();
                    }
                }
                else
                {
                    tr.error_description = "Internal: Error: " + response.Content;
                }

                SaveHitLog("S", null, null, null, "EventRefresh", uppclConfig.TokenUrl, basicAuth, response?.Content, DateTime.Now, null);

            }
            catch (Exception ex)
            {
                tr.error_description = "Internal: Exception: " + ex.Message;
            }
            return tr;
        }

        public static string EventResponse(string eventId, string eventType = "")
        {
            CheckTokenExpiry();

            string tr = "";

            try
            {
                string eventUrl = uppclConfig.Event_Url.Replace("_eventid_", eventId);
                var client = new RestClient(eventUrl);
                var request = new RestRequest("", Method.Get);
                string basicAuth = "Bearer " + uppclConfig.Event_AccessToken;
                request.AddHeader("Authorization", basicAuth);
                request.AddHeader("Content-Type", "application/json");
                var response = client.Execute<string>(request);

                tr = response.Content ?? "no data found";

                SaveHitLog("S", null, null, eventType, "EventResponse", eventUrl, basicAuth, response?.Content, DateTime.Now, null);
            }
            catch (Exception ex)
            {
                tr += "Internal: Exception: " + ex.Message;
            }
            return tr;
        }

        public static string UpdateEventToken(TokenResponse tr)
        {
            string result = "";
            try
            {
                TimeSpan ts = new TimeSpan(0, 0, tr.expires_in / 60, 0);
                DateTime dtExpiry = DateTime.Now.Add(ts);

                using var con = new SqlConnection(DbConnection);
                var queryParameters = new DynamicParameters();
                queryParameters.Add("@Id", "CON001");
                queryParameters.Add("@Event_AccessToken", tr.access_token);
                queryParameters.Add("@Event_RefreshToken", tr.refresh_token);
                queryParameters.Add("@Event_TokenType", tr.token_type);
                queryParameters.Add("@Event_ExpiresIn", tr.expires_in);
                queryParameters.Add("@Event_ExpiresTime", dtExpiry);
                result = con.Query<string>("usp_UPPCLConfigEventTokenUpdate", queryParameters, commandType: System.Data.CommandType.StoredProcedure).SingleOrDefault();
            }
            catch (Exception ex)
            {
                result += "Exception: " + ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Event List Code End
        /// </summary>


        //// AgentActivate Code Start

        public static TokenResponse AgentActivateToken()
        {
            

            TokenResponse tr = new TokenResponse();

            try
            {
                var client = new RestClient(uppclConfig.TokenUrl);
                var request = new RestRequest("", Method.Post);
                string basicAuth = "Basic " + EncodeBase64(Encoding.ASCII, uppclConfig.AgentActivate_TokenConsumerKey + ":" + uppclConfig.AgentActivate_TokenConsumerSecret);
                request.AddHeader("Authorization", basicAuth);
                //request.AddHeader("Content-Type", "application/json");
                var response = client.Execute<TokenResponse>(request);


                if (response.IsSuccessful)
                {
                    tr = JsonConvert.DeserializeObject<TokenResponse>(response.Content);
                    if (tr.error != "invalid_grant" || tr.error == "invalid_client")
                    {
                        UpdateAgentActivateToken(tr);
                        Initialize();
                    }
                }
                else
                {
                    tr.error_description = "Internal: Error: " + response.Content;
                }

                SaveHitLog("S", null, null, null, "AgentActivateAccess", uppclConfig.TokenUrl, basicAuth, response?.Content, DateTime.Now, null);
            }
            catch (Exception ex)
            {
                tr.error_description = "Internal: Exception: " + ex.Message + " -- " + uppclConfig.TokenUrl;
            }
            return tr;
        }

        public static TokenResponse AgentActivateRefreshToken()
        {
            TokenResponse tr = new TokenResponse();

            try
            {
                var client = new RestClient(uppclConfig.RefreshTokenUrl.Replace("_refresh_token_", uppclConfig.AgentActivate_RefreshToken));
                var request = new RestRequest("", Method.Post);
                string basicAuth = "Basic " + EncodeBase64(Encoding.ASCII, uppclConfig.AgentActivate_TokenConsumerKey + ":" + uppclConfig.AgentActivate_TokenConsumerSecret);
                request.AddHeader("Authorization", basicAuth);
                var response = client.Execute<TokenResponse>(request);

                if (response.Content.Contains("access token data not"))
                {
                    AgentActivateToken();
                }

                if (response.IsSuccessful)
                {
                    tr = JsonConvert.DeserializeObject<TokenResponse>(response.Content);
                    if (tr.error != "invalid_grant" || tr.error == "invalid_client")
                    {
                        UpdateAgentActivateToken(tr);
                        Initialize();
                    }
                    else
                    {
                        //Refresh token invalid, so get the fresh access token and refresh token.
                        AgentActivateToken();
                    }
                }
                else
                {
                    tr.error_description = "Internal: Error: " + response.Content;
                }

                SaveHitLog("S", null, null, null, "AgentActivateRefresh", uppclConfig.TokenUrl, basicAuth, response?.Content, DateTime.Now, null);

            }
            catch (Exception ex)
            {
                tr.error_description = "Internal: Exception: " + ex.Message;
            }
            return tr;
        }

        public static AgentActiveInActiveResponse AgentActivate(RetailUser retailUser, string agentStatus)
        {
            CheckTokenExpiry();

            AgentActiveInActiveResponse agentActInActResponse = new AgentActiveInActiveResponse();

            

            try
            {
                string apiUrl = uppclConfig.AgentActivate_Url.Replace("_agency_id_",uppclConfig.AgencyID).Replace("_agent_van_", retailUser.UPPCL_AgentVAN).Replace("_status_",agentStatus);
                var client = new RestClient(apiUrl);
                var request = new RestRequest("", Method.Put);
                string basicAuth = "Bearer " + uppclConfig.AgentActivate_AccessToken;
                request.AddHeader("Authorization", basicAuth);
                request.AddHeader("Content-Type", "application/json");

                var response = client.Execute<string>(request);

                SaveHitLog("S", null, null, retailUser.UPPCL_AgentVAN, "AgentActiveInActive_0", apiUrl, basicAuth, response?.Content, DateTime.Now, null);

                if (response.Content.Contains("Invalid Credentials. Make sure you have provided the correct"))
                {
                    AgentActivateToken();
                }

                if (response.Content.Contains("panNumber"))
                {
                    agentActInActResponse = JsonConvert.DeserializeObject<AgentActiveInActiveResponse>(response?.Content);
                }
                else if (response.Content.Contains("error"))
                {
                    agentActInActResponse.user.firstName = "Exception: Server: " + GetRegExFirstMatch(response.Content, "error\":\"(.*)\",\"me");
                }
                else if (response.Content.Contains("ams:fault"))
                {
                    string? errorCode = GetRegExFirstMatch(response.Content, "ams:message>(.*)</ams:message>");
                    if (errorCode != null && errorCode == "Invalid Credentials")
                    {
                        AgentActivateToken();
                    }
                    agentActInActResponse.user.firstName = "Exception: Server: " + errorCode;
                }
                else
                {
                    agentActInActResponse = JsonConvert.DeserializeObject<AgentActiveInActiveResponse>(response?.Content);

                }

                if(agentActInActResponse?.status != "FAILED")
                {
                    retailUser.UPPCL_Balance = (decimal)agentActInActResponse?.balanceAmount;
                    retailUser.UPPCL_BalanceTime = DateTime.Now;
                    UpdateAgenActivationLog(retailUser, agentActInActResponse, "ACTIN");
                }

                SaveHitLog("S", null, null, retailUser.UPPCL_AgentVAN, "AgentActiveInActive", apiUrl, basicAuth, response?.Content, DateTime.Now, null);

            }
            catch (Exception ex)
            {
                agentActInActResponse.user.firstName = "Internal: Exception: " + ex.Message;
            }
            return agentActInActResponse;
        }

        public static AgentActiveStatusResponse AgentActiveStatus(RetailUser retailUser)
        {
            CheckTokenExpiry();

            string agentVAN = retailUser.UPPCL_AgentVAN;
            AgentActiveStatusResponse agentActiveStatus = new AgentActiveStatusResponse();

            try
            {
                string apiUrl = uppclConfig.AgentStatus_Url.Replace("_agent_van_", agentVAN);
                var client = new RestClient(apiUrl);
                var request = new RestRequest("", Method.Get);
                string basicAuth = "Bearer " + uppclConfig.AgentActivate_AccessToken;
                request.AddHeader("Authorization", basicAuth);
                request.AddHeader("Content-Type", "application/json");

                var response = client.Execute<string>(request);

                SaveHitLog("S", null, null, agentVAN, "AgentActiveStatus_0", apiUrl, basicAuth, response?.Content, DateTime.Now, null);

                if (response.Content.Contains("Invalid Credentials. Make sure you have provided the correct"))
                {
                    AgentActivateToken();
                }

                if (response.Content.Contains("empId"))
                {
                    agentActiveStatus = JsonConvert.DeserializeObject<AgentActiveStatusResponse>(response?.Content);
                }
                else if (response.Content.Contains("error"))
                {
                    agentActiveStatus.firstName = "Exception: Server: " + GetRegExFirstMatch(response.Content, "error\":\"(.*)\",\"me");
                }
                else if (response.Content.Contains("ams:fault"))
                {
                    string? errorCode = GetRegExFirstMatch(response.Content, "ams:message>(.*)</ams:message>");
                    if (errorCode != null && errorCode == "Invalid Credentials")
                    {
                        AgentActivateToken();
                    }
                    agentActiveStatus.firstName = "Exception: Server: " + errorCode;
                }
                else
                {
                    agentActiveStatus = JsonConvert.DeserializeObject<AgentActiveStatusResponse>(response?.Content);

                }

                if (!string.IsNullOrEmpty(agentActiveStatus.status))
                {
                    retailUser.UPPCL_Active = agentActiveStatus.status == "ACTIVE" ? true : false;
                    retailUser.UPPCL_BalanceTime = DateTime.Now;
                    retailUser.UPPCL_AgentVAN = agentActiveStatus.van;

                    var aiResponse = new AgentActiveInActiveResponse();
                    aiResponse.status = agentActiveStatus.status;
                    aiResponse.van = agentActiveStatus.van;

                    UpdateAgenActivationLog(retailUser, aiResponse, "STATUS");
                }

                SaveHitLog("S", null, null, agentVAN, "AgentActiveStatus", apiUrl, basicAuth, response?.Content, DateTime.Now, null);

            }
            catch (Exception ex)
            {
                agentActiveStatus.firstName = "Internal: Exception: " + ex.Message;
            }
            return agentActiveStatus;
        }

        public static string UpdateAgentActivateToken(TokenResponse tr)
        {
            string result = "";
            try
            {
                TimeSpan ts = new TimeSpan(0, 0, tr.expires_in / 60, 0);
                DateTime dtExpiry = DateTime.Now.Add(ts);

                using var con = new SqlConnection(DbConnection);
                var queryParameters = new DynamicParameters();
                queryParameters.Add("@Id", "CON001");
                queryParameters.Add("@AgentActivate_AccessToken", tr.access_token);
                queryParameters.Add("@AgentActivate_RefreshToken", tr.refresh_token);
                queryParameters.Add("@AgentActivate_TokenType", tr.token_type);
                queryParameters.Add("@AgentActivate_ExpiresIn", tr.expires_in);
                queryParameters.Add("@AgentActivate_ExpiresTime", dtExpiry);
                result = con.Query<string>("usp_UPPCLConfigAgentActivateTokenUpdate", queryParameters, commandType: System.Data.CommandType.StoredProcedure).SingleOrDefault();
            }
            catch (Exception ex)
            {
                result += "Exception: " + ex.Message;
            }
            return result;
        }

        //// AgentActivate Code End



        //// Wallet Balance Code Start
        public static TokenResponse WalletBalanceToken()
        {
            TokenResponse tr = new TokenResponse();

            try
            {
                var client = new RestClient(uppclConfig.TokenUrl);
                var request = new RestRequest("", Method.Post);
                string basicAuth = "Basic " + EncodeBase64(Encoding.ASCII, uppclConfig.WalletBalance_TokenConsumerKey + ":" + uppclConfig.WalletBalance_TokenConsumerSecret);
                request.AddHeader("Authorization", basicAuth);
                var response = client.Execute<TokenResponse>(request);


                if (response.IsSuccessful)
                {
                    tr = JsonConvert.DeserializeObject<TokenResponse>(response.Content);
                    if (tr.error != "invalid_grant" || tr.error == "invalid_client")
                    {
                        UpdateWalletBalanceToken(tr);
                        Initialize();
                    }
                }
                else
                {
                    tr.error_description = "Internal: Error: " + response.Content;
                }


                SaveHitLog("S", null, null, null, "WalletBalanceAccess", uppclConfig.TokenUrl, basicAuth, response?.Content, DateTime.Now, null);
            }
            catch (Exception ex)
            {
                tr.error_description = "Internal: Exception: " + ex.Message;
            }
            return tr;
        }

        public static TokenResponse WalletBalanceRefreshToken()
        {
            TokenResponse tr = new TokenResponse();

            try
            {
                var client = new RestClient(uppclConfig.RefreshTokenUrl.Replace("_refresh_token_", uppclConfig.WalletBalance_RefreshToken));
                var request = new RestRequest("", Method.Post);
                string basicAuth = "Basic " + EncodeBase64(Encoding.ASCII, uppclConfig.WalletBalance_TokenConsumerKey + ":" + uppclConfig.WalletBalance_TokenConsumerSecret);
                request.AddHeader("Authorization", basicAuth);
                var response = client.Execute<TokenResponse>(request);

                if (response.Content.Contains("access token data not"))
                {
                    WalletBalanceToken();
                }

                if (response.IsSuccessful)
                {
                    tr = JsonConvert.DeserializeObject<TokenResponse>(response.Content);
                    if (tr.error != "invalid_grant" || tr.error == "invalid_client")
                    {
                        UpdateWalletBalanceToken(tr);
                        Initialize();
                    }
                    else
                    {
                        //Refresh token invalid, so get the fresh access token and refresh token.
                        WalletBalanceToken();
                    }
                }
                else
                {
                    tr.error_description = "Internal: Error: " + response.Content;
                }

                SaveHitLog("S", null, null, null, "WalletBalanceRefresh", uppclConfig.TokenUrl, basicAuth, response?.Content, DateTime.Now, null);

            }
            catch (Exception ex)
            {
                tr.error_description = "Internal: Exception: " + ex.Message;
            }
            return tr;
        }

        public static AgentBalanceResponse WalletBalanceDetails(RetailUser retailUser)
        {
            CheckTokenExpiry();

            string agentVAN = retailUser.UPPCL_AgentVAN;
            AgentBalanceResponse wr = new AgentBalanceResponse();

            try
            {

                RestClientOptions restClientOptions = new RestClientOptions();
                restClientOptions.MaxTimeout = 5000;
                restClientOptions.BaseUrl = new Uri(uppclConfig.WalletBalance_Url.Replace("_agent_van_", agentVAN));

                var client = new RestClient(restClientOptions);
                var request = new RestRequest("", Method.Get);
                request.Timeout = TimeSpan.FromSeconds(90);

                string basicAuth = "Bearer " + uppclConfig.WalletBalance_AccessToken;
                request.AddHeader("Authorization", basicAuth);

                var response = client.Execute<AgentBalanceResponse>(request);

                var respData = response?.Content;
                if (respData != null)
                {
                    if (respData.Contains("balance"))
                    {
                        wr = JsonConvert.DeserializeObject<AgentBalanceResponse>(response?.Content);
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
                            WalletBalanceToken();
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

                if (!string.IsNullOrEmpty(wr.vanId))
                {
                    retailUser.UPPCL_Balance = (decimal)wr.balance;
                    retailUser.UPPCL_BalanceTime = DateTime.Now;
                    retailUser.UPPCL_AgentVAN = wr.vanId;
                    UpdateAgentBalance(retailUser, "BALANCE");
                }

                SaveHitLog("S", null, null, null, "WalletBalanceDetails", restClientOptions.BaseUrl.ToString(), basicAuth, response?.Content, DateTime.Now, null);

            }
            catch (Exception ex)
            {
                wr.message = "Internal: Exception: " + ex.Message;
            }
            return wr;
        }

        public static string UpdateWalletBalanceToken(TokenResponse tr)
        {
            string result = "";
            try
            {
                TimeSpan ts = new TimeSpan(0, 0, tr.expires_in / 60, 0);
                DateTime dtExpiry = DateTime.Now.Add(ts);

                using var con = new SqlConnection(DbConnection);
                var queryParameters = new DynamicParameters();
                queryParameters.Add("@Id", "CON001");
                queryParameters.Add("@WalletBalance_AccessToken", tr.access_token);
                queryParameters.Add("@WalletBalance_RefreshToken", tr.refresh_token);
                queryParameters.Add("@WalletBalance_TokenType", tr.token_type);
                queryParameters.Add("@WalletBalance_ExpiresIn", tr.expires_in);
                queryParameters.Add("@WalletBalance_ExpiresTime", dtExpiry);
                result = con.Query<string>("usp_UPPCLConfigWalletBalanceTokenUpdate", queryParameters, commandType: System.Data.CommandType.StoredProcedure).SingleOrDefault();
            }
            catch (Exception ex)
            {
                result += "Exception: " + ex.Message;
            }
            return result;
        }
        //// Wallet Balance Code End


        //// WalletTopup Code Start
        public static TokenResponse WalletTopupToken()
        {
            TokenResponse tr = new TokenResponse();

            try
            {
                var client = new RestClient(uppclConfig.TokenUrl);
                var request = new RestRequest("", Method.Post);
                string basicAuth = "Basic " + EncodeBase64(Encoding.ASCII, uppclConfig.WalletTopup_TokenConsumerKey + ":" + uppclConfig.WalletTopup_TokenConsumerSecret);
                request.AddHeader("Authorization", basicAuth);
                var response = client.Execute<TokenResponse>(request);


                if (response.IsSuccessful)
                {
                    tr = JsonConvert.DeserializeObject<TokenResponse>(response.Content);
                    if (tr.error != "invalid_grant" || tr.error == "invalid_client")
                    {
                        UpdateWalletTopupToken(tr);
                        Initialize();
                    }
                }
                else
                {
                    tr.error_description = "Internal: Error: " + response.Content;
                }



                SaveHitLog("S", null, null, null, "WalletTopupAccess", uppclConfig.TokenUrl, basicAuth, response?.Content, DateTime.Now, null);
            }
            catch (Exception ex)
            {
                tr.error_description = "Internal: Exception: " + ex.Message;
            }
            return tr;
        }

        public static TokenResponse WalletTopupRefreshToken()
        {
            TokenResponse tr = new TokenResponse();

            try
            {
                string url = uppclConfig.RefreshTokenUrl.Replace("_refresh_token_", uppclConfig.WalletTopup_RefreshToken);
                var client = new RestClient(url);
                var request = new RestRequest("", Method.Post);
                string basicAuth = "Basic " + EncodeBase64(Encoding.ASCII, uppclConfig.WalletTopup_TokenConsumerKey + ":" + uppclConfig.WalletTopup_TokenConsumerSecret);
                request.AddHeader("Authorization", basicAuth);
                var response = client.Execute<TokenResponse>(request);

                if (response.Content.Contains("access token data not"))
                {
                    WalletTopupToken();
                }

                if (response.IsSuccessful)
                {
                    tr = JsonConvert.DeserializeObject<TokenResponse>(response.Content);
                    if (tr.error != "invalid_grant" || tr.error == "invalid_client")
                    {
                        UpdateWalletTopupToken(tr);
                        Initialize();
                    }
                    else
                    {
                        //Refresh token invalid, so get the fresh access token and refresh token.
                        WalletTopupToken();
                    }
                }
                else
                {
                    tr.error_description = "Internal: Error: " + response.Content;
                }



                SaveHitLog("S", null, null, null, "WalletTopupRefresh", url, basicAuth, response?.Content, DateTime.Now, null);

            }
            catch (Exception ex)
            {
                tr.error_description = "Internal: Exception: " + ex.Message;
            }
            return tr;
        }

        public static EventResponse WalletTopup(WalletTopupRequest walletTopupRequest)
        {
            CheckTokenExpiry();

            EventResponse eventResponse = new EventResponse();
            RestResponse response = new RestResponse();
            string postData = "";
            string basicAuth = "";

            try
            {
                RestClientOptions restClientOptions = new RestClientOptions();
                restClientOptions.MaxTimeout = 15000;
                restClientOptions.BaseUrl = new Uri(uppclConfig.WalletTopup_Url);

                var client = new RestClient(restClientOptions);

                var request = new RestRequest("", Method.Post);
                request.Timeout = TimeSpan.FromSeconds(90);

                basicAuth = "Bearer " + uppclConfig.WalletTopup_AccessToken;
                request.AddHeader("Authorization", basicAuth);

                postData = uppclConfig.WalletTopup_PostData;

                postData = postData
                    .Replace("_agent_van_", walletTopupRequest.agentVan)
                    .Replace("_agency_van_", walletTopupRequest.agencyVan)
                    .Replace("_amount_", walletTopupRequest.amount)
                    .Replace("_ref_id_", walletTopupRequest.referenceId)
                    .Replace("_date_", walletTopupRequest.transactionDate);

                request.AddStringBody(postData, ContentType.Json);

                response = client.Execute<EventResponse>(request);


                SaveHitLog("S", null, null, walletTopupRequest?.agentVan, "WalletTopup_0", uppclConfig.WalletTopup_Url + " " + postData, basicAuth, response?.Content, DateTime.Now, null);

                var respData = response?.Content;
                if (respData != null)
                {
                    if (respData.Contains("Invalid Credentials. Make sure you have provided the correct"))
                    {
                        WalletTopupToken();
                    }

                    if (respData.Contains("/v1/event"))
                    {
                        eventResponse = JsonConvert.DeserializeObject<EventResponse>(response.Content);
                    }
                    else if (respData.Contains("error"))
                    {
                        eventResponse.message = "Exception: Server: " + GetRegExFirstMatch(respData, "error\":\"(.*)\",\"me");
                        
                    }
                    else if (respData.Contains("ams:fault"))
                    {
                        string? errorCode = GetRegExFirstMatch(respData, "ams:message>(.*)</ams:message>");
                        if (errorCode != null && errorCode == "Invalid Credentials")
                        {
                            WalletTopupToken();
                        }
                        eventResponse.message = "Exception: Server: " + errorCode;
                        if (respData.Contains("ams:description"))
                        {
                            eventResponse.message += GetRegExFirstMatch(respData, ":description>(.*)</ams:description");

                        }
                    }
                    else
                    {
                        eventResponse = JsonConvert.DeserializeObject<EventResponse>(response.Content);

                    }

                    try
                    {
                        if (eventResponse.location != null)
                        {
                            eventResponse.EventId = eventResponse.location.Replace("/v1/event/", "");
                        }
                    }
                    catch (Exception ex) { }
                }

                string nullResponse = "";
                if (response.StatusCode == 0)
                {
                    eventResponse.message = "Exception: Server: null response.";
                    nullResponse = "NULL_RESP";
                }

                SaveHitLog("S", walletTopupRequest.agentVan, nullResponse, eventResponse.EventId, "WalletTopup", uppclConfig.WalletTopup_Url + " " + postData, basicAuth, response?.Content, DateTime.Now, null);

            }
            catch (Exception ex)
            {
                eventResponse.message = "Internal: Exception: " + ex.Message + Environment.NewLine + response?.Content;
            }
            finally
            {
                //SaveHitLog("S", null, null, billPaymentRequest?.consumerAccountId, "BillPostBillPayment", uppclConfig.BillFetch_BillDetail_Url + " " + postData, basicAuth, response?.Content, DateTime.Now, null);

            }
            return eventResponse;
        }

        public static string UpdateWalletTopupToken(TokenResponse tr)
        {
            string result = "";
            try
            {
                TimeSpan ts = new TimeSpan(0, 0, tr.expires_in / 60, 0);
                DateTime dtExpiry = DateTime.Now.Add(ts);

                using var con = new SqlConnection(DbConnection);
                var queryParameters = new DynamicParameters();
                queryParameters.Add("@Id", "CON001");
                queryParameters.Add("@WalletTopup_AccessToken", tr.access_token);
                queryParameters.Add("@WalletTopup_RefreshToken", tr.refresh_token);
                queryParameters.Add("@WalletTopup_TokenType", tr.token_type);
                queryParameters.Add("@WalletTopup_ExpiresIn", tr.expires_in);
                queryParameters.Add("@WalletTopup_ExpiresTime", dtExpiry);
                result = con.Query<string>("usp_UPPCLConfigWalletTopupTokenUpdate", queryParameters, commandType: System.Data.CommandType.StoredProcedure).SingleOrDefault();
            }
            catch (Exception ex)
            {
                result += "Exception: " + ex.Message;
            }
            return result;
        }

        //// WalletTopup Code End
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// 



        //// WalletTopupStatusByRange Status Code Start
        public static TokenResponse WalletTopupStatusByRangeToken()
        {
            TokenResponse tr = new TokenResponse();

            try
            {
                var client = new RestClient(uppclConfig.TokenUrl);
                var request = new RestRequest("", Method.Post);
                string basicAuth = "Basic " + EncodeBase64(Encoding.ASCII, uppclConfig.WalletTopupStatusByRange_TokenConsumerKey + ":" + uppclConfig.WalletTopupStatusByRange_TokenConsumerSecret);
                request.AddHeader("Authorization", basicAuth);
                var response = client.Execute<TokenResponse>(request);


                if (response.IsSuccessful)
                {
                    tr = JsonConvert.DeserializeObject<TokenResponse>(response.Content);
                    if (tr.error != "invalid_grant" || tr.error == "invalid_client")
                    {
                        UpdateWalletTopupStatusByRangeToken(tr);
                        Initialize();
                    }
                }
                else
                {
                    tr.error_description = "Internal: Error: " + response.Content;
                }


                SaveHitLog("S", null, null, null, "WalletTopupStatusByRangeAccess", uppclConfig.TokenUrl, basicAuth, response?.Content, DateTime.Now, null);
            }
            catch (Exception ex)
            {
                tr.error_description = "Internal: Exception: " + ex.Message;
            }
            return tr;
        }

        public static TokenResponse WalletTopupStatusByRangeRefreshToken()
        {
            TokenResponse tr = new TokenResponse();

            try
            {
                var client = new RestClient(uppclConfig.RefreshTokenUrl.Replace("_refresh_token_", uppclConfig.WalletTopupStatusByRange_RefreshToken));
                var request = new RestRequest("", Method.Post);
                string basicAuth = "Basic " + EncodeBase64(Encoding.ASCII, uppclConfig.WalletTopupStatusByRange_TokenConsumerKey + ":" + uppclConfig.WalletTopupStatusByRange_TokenConsumerSecret);
                request.AddHeader("Authorization", basicAuth);
                var response = client.Execute<TokenResponse>(request);

                if (response.Content.Contains("access token data not"))
                {
                    WalletTopupStatusByRangeToken();
                }

                if (response.IsSuccessful)
                {
                    tr = JsonConvert.DeserializeObject<TokenResponse>(response.Content);
                    if (tr.error != "invalid_grant" || tr.error == "invalid_client")
                    {
                        UpdateWalletTopupStatusByRangeToken(tr);
                        Initialize();
                    }
                    else
                    {
                        //Refresh token invalid, so get the fresh access token and refresh token.
                        WalletTopupStatusByRangeToken();
                    }
                }
                else
                {
                    tr.error_description = "Internal: Error: " + response.Content;
                }

                SaveHitLog("S", null, null, null, "WalletTopupStatusByRangeRefresh", uppclConfig.TokenUrl, basicAuth, response?.Content, DateTime.Now, null);

            }
            catch (Exception ex)
            {
                tr.error_description = "Internal: Exception: " + ex.Message;
            }
            return tr;
        }

        public static WalletTransferByDateRangeResponse WalletTopupStatusByRangeDetails(RTran rTran, bool updateTransaction = false)
        {
            //CheckTokenExpiry();

            if (string.IsNullOrEmpty(rTran.RetailUserId))
            {
                rTran = rTran.LoadRecord();
            }
            RetailUser retailUser = UPPCLManager.RetailUserDetail(rTran.RetailUserId);
            WalletTransferByDateRangeResponse wr = new WalletTransferByDateRangeResponse();

            if (!string.IsNullOrEmpty(retailUser.UPPCL_AgentVAN))
            {
                string agentVAN = retailUser.UPPCL_AgentVAN;
                string referenceId = rTran.Id;

                DateTime startTime = rTran.CreateDate.Value.Date; //rTran.CreateDate.Value.AddMinutes(-30);
                DateTime endTime = rTran.CreateDate.Value.Date.AddDays(1).AddTicks(-1);

                //DateTime startTime = DateTime.Now.AddDays(-90);
                //DateTime endTime = DateTime.Now;
              
                long startTimeMillis = UnixTimestampConverter.ToUnixTimestampMillisIST(startTime);
                long endTimeMillis = UnixTimestampConverter.ToUnixTimestampMillisIST(endTime);

                try
                {
                    string urlApi = uppclConfig.WalletTopupStatusByRange_Url
                                    .Replace("_agency_van_", uppclConfig.AgencyVANNo)
                                    .Replace("_agent_van_", agentVAN)
                                    .Replace("_start_time_", startTimeMillis.ToString())
                                    .Replace("_end_time_", endTimeMillis.ToString())
                                    .Replace("_ref_id_", referenceId);
                    RestClientOptions restClientOptions = new RestClientOptions();
                    restClientOptions.MaxTimeout = 5000;
                    restClientOptions.BaseUrl = new Uri(urlApi);

                    var client = new RestClient(restClientOptions);
                    var request = new RestRequest("", Method.Get);
                    request.Timeout = TimeSpan.FromSeconds(90);

                    string basicAuth = "Bearer " + uppclConfig.WalletTopupStatusByRange_AccessToken;
                    request.AddHeader("Authorization", basicAuth);

                    var response = client.Execute<WalletTransferByDateRangeResponse>(request);

                    var respData = response?.Content;
                    if (respData != null)
                    {
                        if (respData.Contains("transactionId"))
                        {
                            wr = JsonConvert.DeserializeObject<WalletTransferByDateRangeResponse>(response?.Content);
                            if (updateTransaction)
                            {
                                try
                                {
                                    using var con = new SqlConnection(DbConnection);
                                    var queryParameters = new DynamicParameters();
                                    queryParameters.Add("@Id", rTran.Id);
                                    queryParameters.Add("@UPPCL_TransactionId", wr.items[0].transactionId);
                                    queryParameters.Add("@UPPCL_FundStatus", 3);
                                    queryParameters.Add("@UPPCL_TransactionDate", UnixTimestampConverter.FromUnixTimestampMillisIST(wr.items[0].transactionTime));
                                    var resData = con.Query<RetailUser>("usp_RTranUPPCLFundByStatusUpdate", queryParameters, commandType: System.Data.CommandType.StoredProcedure).ToList();

                                }
                                catch (Exception)
                                {

                                }
                            }

                        }
                        else if (respData.Contains("error"))
                        {
                            wr.nextPageToken = "Exception: Server: " + GetRegExFirstMatch(respData, "error\":\"(.*)\",\"me");
                        }
                        else if (respData.Contains("ams:fault"))
                        {
                            string? errorCode = GetRegExFirstMatch(respData, "ams:message>(.*)</ams:message>");
                            if (errorCode != null && errorCode == "Invalid Credentials")
                            {
                                WalletTopupStatusByRangeToken();
                            }
                            wr.nextPageToken = "Exception: Server: Error Code-" + errorCode + ", Error:" + respData;
                        }
                        else if (respData.Contains("am:fault"))
                        {
                            string? errorCode = GetRegExFirstMatch(respData, "am:message>(.*)</am:message>");
                            wr.nextPageToken = "Exception: Server: Error Code-" + errorCode + ", Error:" + respData;
                        }
                    }
                    else
                    {
                        wr.nextPageToken = "Exception: Server: null response.";
                    }

                    SaveHitLog("S", null, null, null, "WalletTopupStatusByRangeDetails", urlApi, basicAuth, response?.Content, DateTime.Now, null);

                }
                catch (Exception ex)
                {
                    wr.nextPageToken = "Internal: Exception: " + ex.Message;
                }
            }
            else
            {
                wr.nextPageToken = "Exception: Invalid agent VAN, VAN id not found.";
            }

            
            return wr;
        }


        
        public static string UpdateWalletTopupStatusByRangeToken(TokenResponse tr)
        {
            string result = "";
            try
            {
                TimeSpan ts = new TimeSpan(0, 0, tr.expires_in / 60, 0);
                DateTime dtExpiry = DateTime.Now.Add(ts);

                using var con = new SqlConnection(DbConnection);
                var queryParameters = new DynamicParameters();
                queryParameters.Add("@Id", "CON001");
                queryParameters.Add("@WalletTopupStatusByRange_AccessToken", tr.access_token);
                queryParameters.Add("@WalletTopupStatusByRange_RefreshToken", tr.refresh_token);
                queryParameters.Add("@WalletTopupStatusByRange_TokenType", tr.token_type);
                queryParameters.Add("@WalletTopupStatusByRange_ExpiresIn", tr.expires_in);
                queryParameters.Add("@WalletTopupStatusByRange_ExpiresTime", dtExpiry);
                result = con.Query<string>("usp_UPPCLConfigWalletTopupStatusByRangeTokenUpdate", queryParameters, commandType: System.Data.CommandType.StoredProcedure).SingleOrDefault();
            }
            catch (Exception ex)
            {
                result += "Exception: " + ex.Message;
            }
            return result;
        }
        //// WalletTopup Status Code End


        //// AgentCreation Code Start
        public static TokenResponse AgentCreationToken()
        {
            

            TokenResponse tr = new TokenResponse();

            try
            {
                var client = new RestClient(uppclConfig.TokenUrl);
                var request = new RestRequest("", Method.Post);
                string basicAuth = "Basic " + EncodeBase64(Encoding.ASCII, uppclConfig.AgentCreation_TokenConsumerKey + ":" + uppclConfig.AgentCreation_TokenConsumerSecret);
                request.AddHeader("Authorization", basicAuth);
                var response = client.Execute<TokenResponse>(request);


                if (response.IsSuccessful)
                {
                    tr = JsonConvert.DeserializeObject<TokenResponse>(response.Content);
                    if (tr.error != "invalid_grant" || tr.error == "invalid_client")
                    {
                        UpdateAgentCreationToken(tr);
                        Initialize();
                    }
                }
                else
                {
                    tr.error_description = "Internal: Error: " + response.Content;
                }



                SaveHitLog("S", null, null, null, "AgentCreationAccess", uppclConfig.TokenUrl, basicAuth, response?.Content, DateTime.Now, null);
            }
            catch (Exception ex)
            {
                tr.error_description = "Internal: Exception: " + ex.Message;
            }
            return tr;
        }

        public static TokenResponse AgentCreationRefreshToken()
        {
            TokenResponse tr = new TokenResponse();

            try
            {
                string url = uppclConfig.RefreshTokenUrl.Replace("_refresh_token_", uppclConfig.AgentCreation_RefreshToken);
                var client = new RestClient(url);
                var request = new RestRequest("", Method.Post);
                string basicAuth = "Basic " + EncodeBase64(Encoding.ASCII, uppclConfig.AgentCreation_TokenConsumerKey + ":" + uppclConfig.AgentCreation_TokenConsumerSecret);
                request.AddHeader("Authorization", basicAuth);
                var response = client.Execute<TokenResponse>(request);

                if (response.Content.Contains("access token data not"))
                {
                    AgentCreationToken();
                }

                if (response.IsSuccessful)
                {
                    tr = JsonConvert.DeserializeObject<TokenResponse>(response.Content);
                    if (tr.error != "invalid_grant" || tr.error == "invalid_client")
                    {
                        UpdateAgentCreationToken(tr);
                        Initialize();
                    }
                    else
                    {
                        //Refresh token invalid, so get the fresh access token and refresh token.
                        AgentCreationToken();
                    }
                }
                else
                {
                    tr.error_description = "Internal: Error: " + response.Content;
                }



                SaveHitLog("S", null, null, null, "AgentCreationRefresh", url, basicAuth, response?.Content, DateTime.Now, null);

            }
            catch (Exception ex)
            {
                tr.error_description = "Internal: Exception: " + ex.Message;
            }
            return tr;
        }

        public static TokenResponse AgentStatusByMobileToken()
        {
            TokenResponse tr = new TokenResponse();

            try
            {
                var client = new RestClient(uppclConfig.TokenUrl);
                var request = new RestRequest("", Method.Post);
                string basicAuth = "Basic " + EncodeBase64(Encoding.ASCII, uppclConfig.AgentStatusByMobile_TokenConsumerKey + ":" + uppclConfig.AgentStatusByMobile_TokenConsumerSecret);
                request.AddHeader("Authorization", basicAuth);
                var response = client.Execute<TokenResponse>(request);


                if (response.IsSuccessful)
                {
                    tr = JsonConvert.DeserializeObject<TokenResponse>(response.Content);
                    if (tr.error != "invalid_grant" || tr.error == "invalid_client")
                    {
                        UpdateAgentStatusByMobileToken(tr);
                        Initialize();
                    }
                }
                else
                {
                    tr.error_description = "Internal: Error: " + response.Content;
                }



                SaveHitLog("S", null, null, null, "AgentStatusByMobileAccess", uppclConfig.TokenUrl, basicAuth, response?.Content, DateTime.Now, null);
            }
            catch (Exception ex)
            {
                tr.error_description = "Internal: Exception: " + ex.Message;
            }
            return tr;
        }

        public static TokenResponse AgentStatusByMobileRefreshToken()
        {
            TokenResponse tr = new TokenResponse();

            try
            {
                string url = uppclConfig.RefreshTokenUrl.Replace("_refresh_token_", uppclConfig.AgentStatusByMobile_RefreshToken);
                var client = new RestClient(url);
                var request = new RestRequest("", Method.Post);
                string basicAuth = "Basic " + EncodeBase64(Encoding.ASCII, uppclConfig.AgentStatusByMobile_TokenConsumerKey + ":" + uppclConfig.AgentStatusByMobile_TokenConsumerSecret);
                request.AddHeader("Authorization", basicAuth);
                var response = client.Execute<TokenResponse>(request);

                if (response.Content.Contains("access token data not"))
                {
                    AgentStatusByMobileToken();
                }

                if (response.IsSuccessful)
                {
                    tr = JsonConvert.DeserializeObject<TokenResponse>(response.Content);
                    if (tr.error != "invalid_grant" || tr.error == "invalid_client")
                    {
                        UpdateAgentStatusByMobileToken(tr);
                        Initialize();
                    }
                    else
                    {
                        //Refresh token invalid, so get the fresh access token and refresh token.
                        AgentStatusByMobileToken();
                    }
                }
                else
                {
                    tr.error_description = "Internal: Error: " + response.Content;
                }



                SaveHitLog("S", null, null, null, "AgentStatusByMobileRefresh", url, basicAuth, response?.Content, DateTime.Now, null);

            }
            catch (Exception ex)
            {
                tr.error_description = "Internal: Exception: " + ex.Message;
            }
            return tr;
        }

        public static EventResponse CreateAgent(AgentCreationRequest agentCreationRequest)
        {
            CheckTokenExpiry();

            EventResponse eventResponse = new EventResponse();
            RestResponse response = new RestResponse();
            string postData = "";
            string basicAuth = "";

            try
            {
                RestClientOptions restClientOptions = new RestClientOptions();
                restClientOptions.MaxTimeout = 15000;
                restClientOptions.BaseUrl = new Uri(uppclConfig.AgentCreation_Url);

                var client = new RestClient(restClientOptions);

                var request = new RestRequest("", Method.Post);
                request.Timeout = TimeSpan.FromSeconds(90);

                basicAuth = "Bearer " + uppclConfig.AgentCreation_AccessToken;
                request.AddHeader("Authorization", basicAuth);

                postData = uppclConfig.AgentCreation_PostData;

                postData = postData
                    .Replace("_email_", agentCreationRequest.email)
                    .Replace("_id_", agentCreationRequest.empId)
                    .Replace("_firstname_", agentCreationRequest.firstName)
                    .Replace("_lastname_", agentCreationRequest.lastName)
                    .Replace("_mobile_", agentCreationRequest.mobile)
                    .Replace("_pan_", agentCreationRequest.panNumber)
                    .Replace("_city_", agentCreationRequest.city)
                    .Replace("_district_", agentCreationRequest.district)
                    .Replace("_address_", agentCreationRequest.address)
                    .Replace("_postalCode_", agentCreationRequest.postalCode)
                    .Replace("_state_", agentCreationRequest.state)
                    .Replace("_discom_", agentCreationRequest.discoms);

                request.AddStringBody(postData, ContentType.Json);

                response = client.Execute<EventResponse>(request);


                SaveHitLog("S", null, null, agentCreationRequest?.empId, "AgentCreation_0", uppclConfig.AgentCreation_Url + " " + postData, basicAuth, response?.Content, DateTime.Now, null);

                var respData = response?.Content;
                if (respData != null)
                {
                    if (respData.Contains("Invalid Credentials. Make sure you have provided the correct"))
                    {
                        AgentCreationToken();
                    }

                    var responseStatus = JsonConvert.DeserializeObject<EventResponse>(respData);

                    if (responseStatus.status == "FAILED")
                    {
                        eventResponse.message = "Error: " + responseStatus.message;
                    }
                    else
                    {
                        if (respData.Contains("/v1/event"))
                        {
                            eventResponse = JsonConvert.DeserializeObject<EventResponse>(response.Content);
                        }
                    }

                    

                    
                    if (respData.Contains("error"))
                    {
                        eventResponse.message = "Exception: Server: " + GetRegExFirstMatch(respData, "error\":\"(.*)\",\"me");
                    }
                    else if (respData.Contains("ams:fault"))
                    {
                        string? errorCode = GetRegExFirstMatch(respData, "ams:message>(.*)</ams:message>");
                        if (errorCode != null && errorCode == "Invalid Credentials")
                        {
                            AgentCreationToken();
                        }
                        eventResponse.message = "Exception: Server: " + errorCode;
                    }
                    else
                    {
                        eventResponse = JsonConvert.DeserializeObject<EventResponse>(response.Content);

                    }

                    try
                    {
                        if(eventResponse.location != null)
                        {
                            //eventResponse.EventId = GetRegExFirstMatch(eventResponse.location, "event/(.*)\",");
                            eventResponse.EventId = eventResponse.location.Replace("/v1/event/", "");
                        }
                    }catch(Exception ex) { }
                }

                string nullResponse = "";
                if (response.StatusCode == 0)
                {
                    eventResponse.message = "Exception: Server: null response.";
                    nullResponse = "NULL_RESP";
                }

                SaveHitLog("S", agentCreationRequest.empId, nullResponse, eventResponse.EventId, "AgentCreation", uppclConfig.AgentCreation_Url + " " + postData, basicAuth, response?.Content, DateTime.Now, null);

            }
            catch (Exception ex)
            {
                eventResponse.message = "Internal: Exception: " + ex.Message + Environment.NewLine + response?.Content;
            }
            finally
            {
                //SaveHitLog("S", null, null, billPaymentRequest?.consumerAccountId, "BillPostBillPayment", uppclConfig.BillFetch_BillDetail_Url + " " + postData, basicAuth, response?.Content, DateTime.Now, null);

            }
            return eventResponse;
        }

        public static AgentStatusCheckByMobileResponse AgentStatusByMobile(string mobileNumber)
        {
            CheckTokenExpiry();

            AgentStatusCheckByMobileResponse eventResponse = new AgentStatusCheckByMobileResponse();
            RestResponse response = new RestResponse();
            string postData = "";
            string basicAuth = "";

            try
            {
                string mobile = uppclConfig.AgentStatusByMobile_Url.Replace("_mobile_", mobileNumber);
                RestClientOptions restClientOptions = new RestClientOptions();
                restClientOptions.MaxTimeout = 15000;
                restClientOptions.BaseUrl = new Uri(mobile);

                var client = new RestClient(restClientOptions);

                var request = new RestRequest("", Method.Get);
                request.Timeout = TimeSpan.FromSeconds(90);

                basicAuth = "Bearer " + uppclConfig.AgentStatusByMobile_AccessToken;
                request.AddHeader("Authorization", basicAuth);

                postData = "";

                response = client.Execute<AgentStatusCheckByMobileResponse>(request);


                SaveHitLog("S", null, null, "", "AgentStatusByMobile_0", mobile + " " + postData, basicAuth, response?.Content, DateTime.Now, null);

                var respData = response?.Content;
                if (respData != null)
                {
                    if (respData.Contains("Invalid Credentials. Make sure you have provided the correct"))
                    {
                        AgentStatusByMobileToken();
                    }

                    var responseStatus = JsonConvert.DeserializeObject<AgentStatusCheckByMobileResponse>(respData);

                    if (responseStatus.status == "NOT_FOUND")
                    {
                        eventResponse.message = "Error: " + responseStatus.message;
                    }
                    else if (responseStatus.status == "ACTIVE")
                    {
                        
                    }
                    
                    if (respData.Contains("error"))
                    {
                        eventResponse.message = "Exception: Server: " + GetRegExFirstMatch(respData, "error\":\"(.*)\",\"me");
                    }
                    else if (respData.Contains("ams:fault"))
                    {
                        string? errorCode = GetRegExFirstMatch(respData, "ams:message>(.*)</ams:message>");
                        if (errorCode != null && errorCode == "Invalid Credentials")
                        {
                            AgentStatusByMobileToken();
                        }
                        eventResponse.message = "Exception: Server: " + errorCode;
                    }
                    else
                    {
                        eventResponse = JsonConvert.DeserializeObject<AgentStatusCheckByMobileResponse>(response.Content);

                    }

                }

                string nullResponse = "";
                if (response.StatusCode == 0)
                {
                    eventResponse.message = "Exception: Server: null response.";
                    nullResponse = "NULL_RESP";
                }

                SaveHitLog("S", "", nullResponse, "", "AgentStatusByMobile", mobile + " " + postData, basicAuth, response?.Content, DateTime.Now, null);

            }
            catch (Exception ex)
            {
                eventResponse.message = "Internal: Exception: " + ex.Message + Environment.NewLine + response?.Content;
            }
            return eventResponse;
        }

        public static string UpdateAgentCreationToken(TokenResponse tr)
        {
            string result = "";
            try
            {
                TimeSpan ts = new TimeSpan(0, 0, tr.expires_in / 60, 0);
                DateTime dtExpiry = DateTime.Now.Add(ts);

                using var con = new SqlConnection(DbConnection);
                var queryParameters = new DynamicParameters();
                queryParameters.Add("@Id", "CON001");
                queryParameters.Add("@AgentCreation_AccessToken", tr.access_token);
                queryParameters.Add("@AgentCreation_RefreshToken", tr.refresh_token);
                queryParameters.Add("@AgentCreation_TokenType", tr.token_type);
                queryParameters.Add("@AgentCreation_ExpiresIn", tr.expires_in);
                queryParameters.Add("@AgentCreation_ExpiresTime", dtExpiry);
                result = con.Query<string>("usp_UPPCLConfigAgentCreationTokenUpdate", queryParameters, commandType: System.Data.CommandType.StoredProcedure).SingleOrDefault();
            }
            catch (Exception ex)
            {
                result += "Exception: " + ex.Message;
            }
            return result;
        }

        public static string UpdateAgentStatusByMobileToken(TokenResponse tr)
        {
            string result = "";
            try
            {
                TimeSpan ts = new TimeSpan(0, 0, tr.expires_in / 60, 0);
                DateTime dtExpiry = DateTime.Now.Add(ts);

                using var con = new SqlConnection(DbConnection);
                var queryParameters = new DynamicParameters();
                queryParameters.Add("@Id", "CON001");
                queryParameters.Add("@AgentStatusByMobile_AccessToken", tr.access_token);
                queryParameters.Add("@AgentStatusByMobile_RefreshToken", tr.refresh_token);
                queryParameters.Add("@AgentStatusByMobile_TokenType", tr.token_type);
                queryParameters.Add("@AgentStatusByMobile_ExpiresIn", tr.expires_in);
                queryParameters.Add("@AgentStatusByMobile_ExpiresTime", dtExpiry);
                result = con.Query<string>("usp_UPPCLConfigAgentStatusByMobileTokenUpdate", queryParameters, commandType: System.Data.CommandType.StoredProcedure).SingleOrDefault();
            }
            catch (Exception ex)
            {
                result += "Exception: " + ex.Message;
            }
            return result;
        }

        //// AgentCreation Code End
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// 

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
            CheckTokenExpiry();

            string result = "Status check started";
            try
            {
                using (var con = new SqlConnection(UPPCLManager.DbConnection))
                {
                    var queryParameters = new DynamicParameters();
                    List<RTran> rt = con.Query<RTran>("usp_RTranForStatusCheck", queryParameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    foreach (RTran tran in rt)
                    {
                        //var statusResponse = tran.StatusCheck(true);
                        //result += Environment.NewLine + tran.RechargeMobileNumber + " - " + tran.UPPCL_BillId + " - " + tran.Amount.ToString() + " - " + statusResponse.status + statusResponse.message;
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
                    if (tokenExpiry.EventDetail < 0)
                    {
                        UPPCLManager.EventToken();
                    }
                    else if (tokenExpiry.EventDetail < 15)
                    {
                        UPPCLManager.EventRefreshToken();
                    }

                    if (tokenExpiry.AgentActivate < 0)
                    {
                        UPPCLManager.AgentActivateToken();
                    }
                    else if (tokenExpiry.AgentActivate < 0)
                    {
                        UPPCLManager.AgentActivateRefreshToken();
                    }

                    if (tokenExpiry.WalletTopupStatusByRange < 0)
                    {
                        UPPCLManager.WalletTopupStatusByRangeToken();
                    }
                    else if (tokenExpiry.WalletTopupStatusByRange < 15)
                    {
                        UPPCLManager.WalletTopupStatusByRangeRefreshToken();
                    }

                    if (tokenExpiry.AgentCreation < 0)
                    {
                        UPPCLManager.AgentCreationToken();
                    }
                    else if (tokenExpiry.AgentCreation < 15)
                    {
                        UPPCLManager.AgentCreationRefreshToken();
                    }

                    
                    if (tokenExpiry.AgentStatusByMobile < 0)
                    {
                        UPPCLManager.AgentStatusByMobileToken();
                    }
                    else if (tokenExpiry.AgentStatusByMobile < 15)
                    {
                        UPPCLManager.AgentStatusByMobileRefreshToken();
                    }

                    

                    if (tokenExpiry.WalletTopup < 0)
                    {
                        UPPCLManager.WalletTopupToken();
                    }
                    else if (tokenExpiry.WalletTopup < 15)
                    {
                        UPPCLManager.WalletTopupRefreshToken();
                    }

                    if (tokenExpiry.WalletBalance < 0)
                    {
                        UPPCLManager.WalletBalanceToken();
                    }
                    else if (tokenExpiry.WalletBalance < 15)
                    {
                        UPPCLManager.WalletBalanceRefreshToken();
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

        public static RetailUser RetailUserDetail(string retailUserId)
        {
            var retailUser = new RetailUser();
            try
            {
                using var con = new SqlConnection(DbConnection);
                var queryParameters = new DynamicParameters();
                queryParameters.Add("@Id", retailUserId);
                retailUser = con.Query<RetailUser>("usp_RetailUserByIdForRegistration", queryParameters, commandType: System.Data.CommandType.StoredProcedure).SingleOrDefault();

            }
            catch (Exception ex)
            {
                retailUser.StateName = ex.Message;
            }
            return retailUser;
        }

        public static string RegisterRetailerToUPPCL(string retailerId)
        {
            string result = "Success: Start of process.";
            try
            {
                using var con = new SqlConnection(DbConnection);
                var retailUser = RetailUserDetail(retailerId);


                if (retailUser != null)
                {
                    if(retailUser.UPPCL_AgentVAN?.Length > 3)
                    {
                        result = "Info: Agent already registered on UPPCL. Agent VAN id is - " + retailUser.UPPCL_AgentVAN;
                    }
                    else
                    {
                        if (retailUser.UserType == 5)
                        {
                            var agentStatusByMobile = AgentStatusByMobile(retailUser.Mobile);

                            if (agentStatusByMobile != null)
                            {
                                if (agentStatusByMobile.status == "ACTIVE")
                                {
                                    //Agent exist, update details.
                                    var queryParametersEvent = new DynamicParameters();
                                    queryParametersEvent.Add("@Id", retailerId);
                                    queryParametersEvent.Add("@UPPCL_AgentVAN", agentStatusByMobile.van);
                                    queryParametersEvent.Add("@UPPCL_Mobile", agentStatusByMobile.mobile);
                                    queryParametersEvent.Add("@UPPCL_UserName", agentStatusByMobile.empId);
                                    queryParametersEvent.Add("@UPPCL_Status", agentStatusByMobile.status);
                                    queryParametersEvent.Add("@UPPCL_Active", 1);
                                    queryParametersEvent.Add("@UPPCL_ActiveTime", DateTime.Now);
                                    var eventUpdateStatus = con.Query<ActionResponse>("usp_RetailUserUPPCLUpdateAfterMobileStatusCheck", queryParametersEvent, commandType: System.Data.CommandType.StoredProcedure).SingleOrDefault();

                                    result = "Info: Save Create Agent by status check. " + eventUpdateStatus.OperationMessage + " Agent VAN id is - " + agentStatusByMobile.van;

                                }
                                else
                                {
                                    //Agent does not exist, create new agent.
                                    if (string.IsNullOrEmpty(retailUser.UPPCL_AgentVAN))
                                    {
                                        var agentCreationRequest = new AgentCreationRequest();
                                        agentCreationRequest.state = "";
                                        agentCreationRequest.mobile = retailUser.Mobile;
                                        agentCreationRequest.empId = retailUser.Id;
                                        agentCreationRequest.accountNumber = "";
                                        agentCreationRequest.panNumber = "";//NOT_AVAILABLE
                                        agentCreationRequest.firstName = retailUser.FirstName;
                                        agentCreationRequest.lastName = retailUser.LastName;
                                        agentCreationRequest.address = "";
                                        agentCreationRequest.city = retailUser.City;
                                        agentCreationRequest.discoms = retailUser.Discom; //"ALL";
                                        agentCreationRequest.email = "";
                                        agentCreationRequest.divisions = "";
                                        agentCreationRequest.district = string.IsNullOrEmpty(retailUser.City) ? "NA" : retailUser.City;
                                        agentCreationRequest.postalCode = "";

                                        UPPCLManager.CheckTokenExpiry();

                                        DateTime regInitTime = DateTime.Now;
                                        var eventResponse = UPPCLManager.CreateAgent(agentCreationRequest);

                                        Thread.Sleep(3000);

                                        if (eventResponse != null)
                                        {

                                            if (eventResponse.status == "FAILED")
                                            {
                                                result = "Errors: " + eventResponse.message;
                                            }
                                            else
                                            {
                                                if (eventResponse.location != null)
                                                {
                                                    eventResponse.EventId = eventResponse.location.Replace("/v1/event/", "");
                                                }

                                                //using var con = new SqlConnection(DbConnection);
                                                var queryParametersEvent = new DynamicParameters();
                                                queryParametersEvent.Add("@Id", retailerId);
                                                queryParametersEvent.Add("@UPPCL_EventId", eventResponse.EventId);
                                                queryParametersEvent.Add("@UPPCL_EventTime", DateTime.Now);
                                                queryParametersEvent.Add("@UPPCL_EventResponse", JsonConvert.SerializeObject(eventResponse));
                                                queryParametersEvent.Add("@UPPCL_RegInit", 1);
                                                queryParametersEvent.Add("@UPPCL_RegInitTime", regInitTime);
                                                var eventUpdate = con.Query<ActionResponse>("usp_RetailUserUPPCLEventUpdate", queryParametersEvent, commandType: System.Data.CommandType.StoredProcedure).SingleOrDefault();

                                                result = "Info: Save Create Agent event response. " + eventUpdate.OperationMessage;



                                                if (!string.IsNullOrEmpty(eventResponse.EventId))
                                                {
                                                    string eventResponseData = UPPCLManager.EventResponse(eventResponse.EventId);

                                                    var retailerRegistrationResponse = JsonConvert.DeserializeObject<AgentCreationEventResponse>(eventResponseData);
                                                    if (retailerRegistrationResponse != null)
                                                    {
                                                        if (retailerRegistrationResponse.status != "FAILED")//SUCCESS
                                                        {
                                                            var queryParametersSuccess = new DynamicParameters();
                                                            queryParametersSuccess.Add("@Id", retailerId);
                                                            queryParametersSuccess.Add("@UPPCL_EventTime", DateTime.Now);
                                                            queryParametersSuccess.Add("@UPPCL_EventResponse", JsonConvert.SerializeObject(retailerRegistrationResponse));
                                                            queryParametersSuccess.Add("@UPPCL_AgencyId", retailerRegistrationResponse.payload.agencyId);
                                                            queryParametersSuccess.Add("@UPPCL_AgentVAN", retailerRegistrationResponse.payload.van);
                                                            queryParametersSuccess.Add("@UPPCL_AgentId", retailerRegistrationResponse.id);
                                                            queryParametersSuccess.Add("@UPPCL_CreateDate", DateTime.Now);
                                                            queryParametersSuccess.Add("@UPPCL_PAN", retailerRegistrationResponse.payload.panNumber);
                                                            queryParametersSuccess.Add("@UPPCL_Discom", "");
                                                            queryParametersSuccess.Add("@UPPCL_Active", 1);
                                                            queryParametersSuccess.Add("@UPPCL_ActiveTime", DateTime.Now);
                                                            queryParametersSuccess.Add("@UPPCL_UserName", retailerRegistrationResponse.payload.userName);
                                                            queryParametersSuccess.Add("@UPPCL_Mobile", retailerRegistrationResponse.payload.mobile);
                                                            queryParametersSuccess.Add("@UPPCL_Balance", 0);
                                                            queryParametersSuccess.Add("@UPPCL_BalanceTime", DateTime.Now);
                                                            queryParametersSuccess.Add("@UPPCL_Status", retailerRegistrationResponse.status);

                                                            var successUpdate = con.Query<ActionResponse>("usp_RetailUserUPPCLUpdate", queryParametersSuccess, commandType: System.Data.CommandType.StoredProcedure).SingleOrDefault();

                                                            if (retailerRegistrationResponse.status == "SUCCESS")
                                                            {
                                                                result = "Success: Retailer successfullly onboarded to UPPCL EWallet. VAN id is - " + retailerRegistrationResponse.payload.van;
                                                            }
                                                            else if(retailerRegistrationResponse.status == "IN_QUEUE")
                                                            {
                                                                result = "Success: Retailer successfullly onboarded to UPPCL EWallet. Registration status is - IN_QUEUE. Please reverify after 5 minutes.";

                                                            }
                                                        }
                                                        else if (retailerRegistrationResponse.status == "FAILED")
                                                        {
                                                            result = "Error: " + retailerRegistrationResponse.reason;
                                                        }
                                                    }

                                                }
                                                else
                                                {
                                                    //event id not received
                                                    if (eventResponse.message.Contains("should not be "))
                                                    {
                                                        result = "Error: " + eventResponse.message;
                                                    }

                                                }

                                                if (eventResponse.status == "FAILED")
                                                {
                                                    result = "Error: " + eventResponse.message;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            result = "Error: Unable to get proper api response on agent creation.";
                                        }

                                        agentCreationRequest = null;

                                    }
                                    else
                                    {
                                        result = "Error: Retailer is already registered on uppcl. VAN id is - " + retailUser.UPPCL_AgentVAN;
                                    }
                                }
                            }
                        }
                        else
                        {
                            result = "Error: Invalid user type, not retailer.";
                        }
                    }
                    
                }
                else
                {
                    result = "Error: Invalid retailer id.";
                }
            }
            catch (Exception exReg)
            {
                result = "Error: " + exReg.Message;
            }
            return result;
        }


        public static string VerifyRetailerByEventId(string retailerId)
        {
            string result = "Success: Start of process.";
            try
            {
                using var con = new SqlConnection(DbConnection);
                var retailUser = RetailUserDetail(retailerId);


                if (retailUser != null)
                {
                    if (retailUser.UserType == 5)
                    {
                        if (string.IsNullOrEmpty(retailUser.UPPCL_AgentVAN))
                        {
                            UPPCLManager.CheckTokenExpiry();

                            if (!string.IsNullOrEmpty(retailUser.UPPCL_EventId))
                            {
                                string eventResponseData = UPPCLManager.EventResponse(retailUser.UPPCL_EventId);

                                var retailerRegistrationResponse = JsonConvert.DeserializeObject<AgentCreationEventResponse>(eventResponseData);
                                if (retailerRegistrationResponse != null)
                                {
                                    if (retailerRegistrationResponse.status == "SUCCESS")
                                    {
                                        var queryParametersSuccess = new DynamicParameters();
                                        queryParametersSuccess.Add("@Id", retailerId);
                                        queryParametersSuccess.Add("@UPPCL_EventTime", DateTime.Now);
                                        queryParametersSuccess.Add("@UPPCL_EventResponse", JsonConvert.SerializeObject(retailerRegistrationResponse));
                                        queryParametersSuccess.Add("@UPPCL_AgencyId", retailerRegistrationResponse.payload.agencyId);
                                        queryParametersSuccess.Add("@UPPCL_AgentVAN", retailerRegistrationResponse.payload.van);
                                        queryParametersSuccess.Add("@UPPCL_AgentId", retailerRegistrationResponse.id);
                                        queryParametersSuccess.Add("@UPPCL_CreateDate", DateTime.Now);
                                        queryParametersSuccess.Add("@UPPCL_PAN", retailerRegistrationResponse.payload.panNumber);
                                        queryParametersSuccess.Add("@UPPCL_Discom", "");
                                        queryParametersSuccess.Add("@UPPCL_Active", 1);
                                        queryParametersSuccess.Add("@UPPCL_ActiveTime", DateTime.Now);
                                        queryParametersSuccess.Add("@UPPCL_UserName", retailerRegistrationResponse.payload.userName);
                                        queryParametersSuccess.Add("@UPPCL_Mobile", retailerRegistrationResponse.payload.mobile);
                                        queryParametersSuccess.Add("@UPPCL_Balance", 0);
                                        queryParametersSuccess.Add("@UPPCL_BalanceTime", DateTime.Now);

                                        var successUpdate = con.Query<ActionResponse>("usp_RetailUserUPPCLUpdate", queryParametersSuccess, commandType: System.Data.CommandType.StoredProcedure).SingleOrDefault();

                                        result = "Success: Retiler successfullly onboarded to UPPCL EWallet. VAN id is - " + retailerRegistrationResponse.payload.van;
                                    }
                                    else if (retailerRegistrationResponse.status == "FAILED")
                                    {
                                        result = "Error: " + retailerRegistrationResponse.reason;
                                    }
                                }

                            }
                            else
                            {
                                
                                result = "Error: event id not received.";
                            }

                        }
                        else
                        {
                            result = "Error: Retailer is already registered on uppcl. VAN id is - " + retailUser.UPPCL_AgentVAN;
                        }
                    }
                    else
                    {
                        result = "Error: Invalid user type, not retailer.";
                    }
                }
                else
                {
                    result = "Error: Invalid retailer id.";
                }
            }
            catch (Exception exReg)
            {
                result = "Error: " + exReg.Message;
            }
            return result;
        }


        public static string UpdateAgentBalance(RetailUser ru, string commandSource)
        {
            string result = "";
            try
            {
                using var con = new SqlConnection(DbConnection);
                var queryParameters = new DynamicParameters();
                queryParameters.Add("@RetailUserId", ru.Id);
                queryParameters.Add("@AgentVAN", ru.UPPCL_AgentVAN);
                queryParameters.Add("@UPPCL_Balance", ru.UPPCL_Balance);
                queryParameters.Add("@UPPCL_BalanceTime", ru.UPPCL_BalanceTime);
                queryParameters.Add("@CommandSource", commandSource);
                result = con.Query<string>("usp_RetailUserUpdateUPPCLBalance", queryParameters, commandType: System.Data.CommandType.StoredProcedure).SingleOrDefault();
            }
            catch (Exception ex)
            {
                result += "Exception: " + ex.Message;
            }
            return result;
        }

        public static string UpdateAgenActivationLog(RetailUser ru, AgentActiveInActiveResponse aiResponse,string commandSource)
        {
            string result = "";
            try
            {
                using var con = new SqlConnection(DbConnection);
                var queryParameters = new DynamicParameters();
                queryParameters.Add("@RetailUserId", aiResponse.empId);
                queryParameters.Add("@AgentVAN", aiResponse.van);
                queryParameters.Add("@ActivationStatus", aiResponse.status);
                queryParameters.Add("@ActivationDate", ru.UPPCL_BalanceTime);
                queryParameters.Add("@CommandSource", commandSource);
                queryParameters.Add("@CommandStatus", 1);
                queryParameters.Add("@Balance", aiResponse.balanceAmount);
                result = con.Query<string>("usp_AgentActivationLogInsert", queryParameters, commandType: System.Data.CommandType.StoredProcedure).SingleOrDefault();

                UpdateAgentBalance(ru, commandSource);
            }
            catch (Exception ex)
            {
                result += "Exception: " + ex.Message;
            }
            return result;
        }



        public static string VerifyWalletTopupByEventId(RTran rTran)
        {
            string result = "Success: Start of process.";

            if (string.IsNullOrEmpty(rTran.RetailUserId))
            {
                rTran = rTran.LoadRecord();
            }

            try
            {
                using var con = new SqlConnection(DbConnection);
                var retailUser = RetailUserDetail(rTran.RetailUserId);

                UPPCLManager.CheckTokenExpiry();

                if (!string.IsNullOrEmpty(rTran.UPPCL_EventId))
                {
                    Thread.Sleep(2000);
                    string eventResponseData = UPPCLManager.EventResponse(rTran.UPPCL_EventId);

                    var walletTransferResponse = JsonConvert.DeserializeObject<WalletTransferResponse>(eventResponseData);

                    if (walletTransferResponse != null)
                    {
                        rTran.UPPCL_Status = walletTransferResponse.status;
                        var tempResult = rTran.UpdateEventStatus();

                        if (walletTransferResponse.status != "FAILED")//
                        {
                            if(walletTransferResponse.status == "SUCCESS")
                            {
                                var DebitTransaction = walletTransferResponse.response.Where(r => r.activity == "DEBIT").FirstOrDefault();
                                var CreditTransaction = walletTransferResponse.response.Where(r => r.activity == "CREDIT").FirstOrDefault();

                                if (DebitTransaction != null && CreditTransaction != null)
                                {
                                    rTran.UPPCL_AgencyVAN = DebitTransaction.vanId;
                                    rTran.UPPCL_DebitVAN = DebitTransaction.vanId;
                                    rTran.UPPCL_AgentVAN = CreditTransaction.vanId;
                                    rTran.UPPCL_CreditVAN = CreditTransaction.vanId;
                                    rTran.UPPCL_TransactionDate = DateTime.ParseExact(walletTransferResponse.createdAt, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                                    rTran.UPPCL_Amount = Convert.ToDecimal(walletTransferResponse.payload.amount);
                                    rTran.UPPCL_Balance = Convert.ToDecimal(walletTransferResponse.payload.amount);
                                    rTran.UPPCL_TransactionStatus = walletTransferResponse.status;
                                    rTran.UPPCL_TransactionId = walletTransferResponse.payload.transactionId;
                                    rTran.UPPCL_FundStatus = 3;

                                    try
                                    {
                                        var balance = UPPCLManager.WalletBalanceDetails(retailUser);
                                        if (balance != null)
                                        {
                                            if (balance.balance > -1)
                                            {
                                                rTran.UPPCL_Balance = Convert.ToDecimal(balance.balance);
                                            }
                                        }
                                    }
                                    catch (Exception ex) { }

                                    string saveResponse = rTran.UpdateEventPayment();

                                    result = "Success: Payment successfully transferred to Retailer UPPCL EWallet. Transaction id is - " + walletTransferResponse.payload.transactionId;

                                }
                            }
                            else
                            {
                                //Update UPPCL Event Status
                                result = "Info: Payment is in pending state in UPPCL EWallet. UPPCL event status is - " + walletTransferResponse.status + ". Please reverify after 5 minutes.";
                                
                                //result += rTran.UpdateEventStatus();
                            }

                        }
                        else if (walletTransferResponse.status == "FAILED")
                        {
                            result = "Error: " + walletTransferResponse.message;
                        }
                    }

                }
                else
                {

                    result = "Error: event id not received.";
                }
            }
            catch (Exception exReg)
            {
                result = "Error: " + exReg.Message;
            }
            return result;
        }


        public static List<RetailUser> RetailUserList()
        {
            List<RetailUser> result = new List<RetailUser>();
            try
            {
                using var con = new SqlConnection(DbConnection);
                var queryParameters = new DynamicParameters();
                queryParameters.Add("@UserType", 10);
                result = con.Query<RetailUser>("usp_RetailUserListByType", queryParameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
            }
            catch (Exception ex)
            {
                result[0].FirstName += "Exception: " + ex.Message;
            }
            return result;
        }

        public static List<RetailUser> RetailUserHighBalanceList()
        {
            List<RetailUser> result = new List<RetailUser>();
            try
            {
                using var con = new SqlConnection(DbConnection);
                var queryParameters = new DynamicParameters();
                result = con.Query<RetailUser>("usp_RetailUserHighBalanceList", queryParameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
            }
            catch (Exception ex)
            {
                result[0].FirstName += "Exception: " + ex.Message;
            }
            return result;
        }

        public static string AgentSSOUrl(string agentMobile)
        {
            string result = "";
            try
            {
                string encodedData = CryptoHelper.Encrypt(uppclConfig.SSO_Hash, uppclConfig.SSO_Secret);
                result = uppclConfig.SSO_Url.Replace("_enccode_", encodedData).Replace("_mobile_", agentMobile);
            }catch(Exception ex)
            {
                result = "Errors: Exception - " + ex.Message;
            }
            return result;
        }


    }
}
