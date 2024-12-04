using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SaralESuvidha.ViewModel;
using Newtonsoft.Json;

namespace SaralESuvidha.Models
{
    public class RetailUser : IDisposable
    {
        public string Id { get; set; }
        public string PlanId { get; set; }
        public bool? IsVirtualUser { get; set; }
        public string MasterId { get; set; }
        public int? UserType { get; set; }
        public short? MarginType { get; set; }
        public short? HitMethodType { get; set; }
        public String FirstName { get; set; }
        public String MiddleName { get; set; }
        public String LastName { get; set; }
        public String EMail { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Mobile { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string PinCode { get; set; }
        public string Country { get; set; }
        public string StateName { get; set; }
        public string CompanyName { get; set; }
        public string GstNumber { get; set; }
        public string GstStatecode { get; set; }
        public string CompanyRegistrationNumber { get; set; }
        public string Password { get; set; }
        public string SecurityQuestion { get; set; }
        public string SecurityAnswer { get; set; }
        public string DomainName { get; set; }
        public String RequestDomain { get; set; }
        public String RequestIP { get; set; }
        public String ResponseURL { get; set; }
        public float? CreditLimit { get; set; }
        public float? MinFundValue { get; set; }
        public float? Commission { get; set; }
        public decimal? CurrentBalance { get; set; }
        public DateTime? CurrentBalanceDate { get; set; }
        public decimal? AlarmingBalance { get; set; }
        public byte? NotificationCount { get; set; }
        public byte? NotificationInterval { get; set; }
        public string ReferredBy { get; set; }
        public bool? RepeatPaymentAllowed { get; set; }
        public int? RepeatDelayInSecond { get; set; }
        public int? Active { get; set; }
        public int? MemberLevel { get; set; }
        public string MachineId { get; set; }
        public string AppId { get; set; }
        public DateTime? LastLoginMachine { get; set; }
        public DateTime? LastLoginApp { get; set; }
        public bool? OtpActive { get; set; }
        public bool? CaptchaActive { get; set; }
        public DateTime? SignupDate { get; set; }
        public int OrderNo { get; set; }
        public long? OldRetailerId { get; set; }
        public string SmsSenderId { get; set; }
        public bool? SmsSenderEnabled { get; set; }
        public string WhiteLabelName { get; set; }
        public bool? WhitelabelEnabled { get; set; }
        public string AppName { get; set; }
        public bool? AppEnabled { get; set; }

        public bool? GlobalFundTransferEnabled { get; set; }
        public short? MaxPlanCount { get; set; }
        
        public string PanNumber { get; set; }
        public string AadharNumber { get; set; }

        public void Dispose()
        {
            //this = null;
        }

        public string SaveNew()
        {
            string result = "";

            return result;
        }

        public string UpdateReceiptMessage(string userId, string receiptMessage, string updateDetails)
        {
            string result = string.Empty;
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var queryParameters = new DynamicParameters();
                    queryParameters.Add("@RetailUserId", userId);
                    queryParameters.Add("@ReceiptMessage", receiptMessage);
                    queryParameters.Add("@UpdateDetails", updateDetails);
                    result = con.QuerySingle<OperationResponse>("usp_UpdateReceiptMessage", queryParameters, commandType: System.Data.CommandType.StoredProcedure).OperationMessage;
                }
            }
            catch (Exception ex)
            {
                result += "Exception in updating receipt message. " + ex.Message;
            }
            return result;
        }

        public string CurrentReceiptMessage(string userId)
        {
            string result = string.Empty;
            UtilityReceiptMessage urm = new UtilityReceiptMessage();
            try
            {
                using var con = new SqlConnection(StaticData.conString);
                var queryParameters = new DynamicParameters();
                queryParameters.Add("@RetailUserId", userId);
                urm = con.QuerySingle<UtilityReceiptMessage>("usp_ReadReceiptMessage", queryParameters, commandType: System.Data.CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                urm.ReceiptMessage += "Exception in getting receipt message. " + ex.Message;
            }

            //var aaData = new { aadata = urm };
            result = JsonConvert.SerializeObject(urm);

            return result;
        }
        
        public RetailUserBalanceResponse GetBalance(string retailUserId="")
        {
            RetailUserBalanceResponse rubr = new RetailUserBalanceResponse();
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var queryParameters = new DynamicParameters();
                    queryParameters.Add("@RetailUserId", retailUserId);
                    rubr = con.QuerySingle<RetailUserBalanceResponse>("usp_GetUserBalance", queryParameters, commandType: System.Data.CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                rubr.OperationMessage += "Exception: " + ex.Message;
            }

            return rubr;
        }

        public RetailUserBalanceResponse GetBalanceWithName(int? retailUserOrderNo, string currentUserId="", int isAdmin = 0)
        {
            RetailUserBalanceResponse rubr = new RetailUserBalanceResponse();
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var queryParameters = new DynamicParameters();
                    queryParameters.Add("@RetailUserOrderNo", retailUserOrderNo);
                    queryParameters.Add("@CurrentUserId", currentUserId);
                    queryParameters.Add("@isAdmin", isAdmin);
                    rubr = con.QuerySingle<RetailUserBalanceResponse>("usp_GetUserBalanceWithName", queryParameters, commandType: System.Data.CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                rubr.OperationMessage += "Exception: " + ex.Message;
            }

            return rubr;
        }
        
        public RetailUserBalanceResponse GetBalanceWithNameWhiteLabel(int? retailUserOrderNo, string currentUserId="")
        {
            RetailUserBalanceResponse rubr = new RetailUserBalanceResponse();
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var queryParameters = new DynamicParameters();
                    queryParameters.Add("@RetailUserOrderNo", retailUserOrderNo);
                    queryParameters.Add("@CurrentUserId", currentUserId);
                    rubr = con.QuerySingle<RetailUserBalanceResponse>("usp_GetUserBalanceWithNameWhiteLabel", queryParameters, commandType: System.Data.CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                rubr.OperationMessage += "Exception: " + ex.Message;
            }

            return rubr;
        }

        public OperationResponse TransferFundToDownline(string transferFromId, int transferFromOrderNo, int transferToOrderNo, decimal amt, string remarks, string requestIp, string requestMachine, string requestGeoCode, string requestNumber, string requestMessage, DateTime requestTime, int requestSource)
        {
            //@RequestSource 1=Webportal, 2=MobileAppAndroid, 3=sms, 4=Whatsapp, 5=MobileAppIos
            OperationResponse result = new OperationResponse();
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    try
                    {
                        var queryParameters = new DynamicParameters();
                        queryParameters.Add("@TransferFromUserId", transferFromId);
                        queryParameters.Add("@TransferFromOrderNo", transferFromOrderNo);
                        queryParameters.Add("@TransferToOrderNo", transferToOrderNo);
                        queryParameters.Add("@TransferToAmount", amt);
                        queryParameters.Add("@RequestIp", requestIp);
                        queryParameters.Add("@RequestMachine", requestMachine);
                        queryParameters.Add("@RequestGeoCode", requestGeoCode);
                        queryParameters.Add("@RequestNumber", requestNumber);
                        queryParameters.Add("@RequestMessage", requestMessage);
                        queryParameters.Add("@RequestTime", requestTime);
                        queryParameters.Add("@Remarks", remarks);
                        queryParameters.Add("@Extra1", null);
                        queryParameters.Add("@Extra2", null);
                        queryParameters.Add("@RequestSource", requestSource);
                        
                        result = con.QuerySingle<OperationResponse>("usp_RetailClientFundTransferToDownline", queryParameters, commandType: System.Data.CommandType.StoredProcedure);
                    }
                    catch (Exception e)
                    {
                        result.OperationMessage = "Errors: Ex:  " + e.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                result.OperationMessage = "Errors: Exception: " + ex.Message;
            }

            return result;
        }
        
        public OperationResponse FundReversalDownline(string reversalToId, int reversalToOrderNo, int reversalFromOrderNo, decimal amt, string remarks, string requestIp, string requestMachine, string requestGeoCode, string requestNumber, string requestMessage, DateTime requestTime, int requestSource)
        {
            OperationResponse result = new OperationResponse();
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    try
                    {
                        var queryParameters = new DynamicParameters();
                        queryParameters.Add("@ReversalToUserId", reversalToId);
                        queryParameters.Add("@ReversalFromOrderNo", reversalFromOrderNo);
                        queryParameters.Add("@ReversalToOrderNo", reversalToOrderNo);
                        queryParameters.Add("@ReversalFromAmount", amt);
                        queryParameters.Add("@RequestIp", requestIp);
                        queryParameters.Add("@RequestMachine", requestMachine);
                        queryParameters.Add("@RequestGeoCode", requestGeoCode);
                        queryParameters.Add("@RequestNumber", requestNumber);
                        queryParameters.Add("@RequestMessage", requestMessage);
                        queryParameters.Add("@RequestTime", requestTime);
                        queryParameters.Add("@Remarks", remarks);
                        queryParameters.Add("@Extra1", null);
                        queryParameters.Add("@Extra2", null);
                        queryParameters.Add("@RequestSource", requestSource);
                        
                        result = con.QuerySingle<OperationResponse>("usp_RetailClientFundReversalFromDowline", queryParameters, commandType: System.Data.CommandType.StoredProcedure);
                    }
                    catch (Exception e)
                    {
                        result.OperationMessage = "Errors: Ex:  " + e.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                result.OperationMessage = "Errors: Exception: " + ex.Message;
            }

            return result;
        }
        
        public OperationResponse TransferFundToDownlineWhiteLabel(string transferFromId, int transferFromOrderNo, int transferToOrderNo, decimal amt, string remarks, string requestIp, string requestMachine, string requestGeoCode, string requestNumber, string requestMessage, DateTime requestTime, int requestSource)
        {
            //@RequestSource 1=Webportal, 2=MobileAppAndroid, 3=sms, 4=Whatsapp, 5=MobileAppIos
            OperationResponse result = new OperationResponse();
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    try
                    {
                        var queryParameters = new DynamicParameters();
                        queryParameters.Add("@TransferFromUserId", transferFromId);
                        queryParameters.Add("@TransferFromOrderNo", transferFromOrderNo);
                        queryParameters.Add("@TransferToOrderNo", transferToOrderNo);
                        queryParameters.Add("@TransferToAmount", amt);
                        queryParameters.Add("@RequestIp", requestIp);
                        queryParameters.Add("@RequestMachine", requestMachine);
                        queryParameters.Add("@RequestGeoCode", requestGeoCode);
                        queryParameters.Add("@RequestNumber", requestNumber);
                        queryParameters.Add("@RequestMessage", requestMessage);
                        queryParameters.Add("@RequestTime", requestTime);
                        queryParameters.Add("@Remarks", remarks);
                        queryParameters.Add("@Extra1", null);
                        queryParameters.Add("@Extra2", null);
                        queryParameters.Add("@RequestSource", requestSource);
                        
                        result = con.QuerySingle<OperationResponse>("usp_RetailClientFundTransferToDownlineWhiteLabel", queryParameters, commandType: System.Data.CommandType.StoredProcedure);
                    }
                    catch (Exception e)
                    {
                        result.OperationMessage = "Errors: Ex:  " + e.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                result.OperationMessage = "Errors: Exception: " + ex.Message;
            }

            return result;
        }
        
        public OperationResponse FundReversalDownlineWhiteLabel(string reversalToId, int reversalToOrderNo, int reversalFromOrderNo, decimal amt, string remarks, string requestIp, string requestMachine, string requestGeoCode, string requestNumber, string requestMessage, DateTime requestTime, int requestSource)
        {
            OperationResponse result = new OperationResponse();
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    try
                    {
                        var queryParameters = new DynamicParameters();
                        queryParameters.Add("@ReversalToUserId", reversalToId);
                        queryParameters.Add("@ReversalFromOrderNo", reversalFromOrderNo);
                        queryParameters.Add("@ReversalToOrderNo", reversalToOrderNo);
                        queryParameters.Add("@ReversalFromAmount", amt);
                        queryParameters.Add("@RequestIp", requestIp);
                        queryParameters.Add("@RequestMachine", requestMachine);
                        queryParameters.Add("@RequestGeoCode", requestGeoCode);
                        queryParameters.Add("@RequestNumber", requestNumber);
                        queryParameters.Add("@RequestMessage", requestMessage);
                        queryParameters.Add("@RequestTime", requestTime);
                        queryParameters.Add("@Remarks", remarks);
                        queryParameters.Add("@Extra1", null);
                        queryParameters.Add("@Extra2", null);
                        queryParameters.Add("@RequestSource", requestSource);
                        
                        result = con.QuerySingle<OperationResponse>("usp_RetailClientFundReversalFromDowlineWhiteLabel", queryParameters, commandType: System.Data.CommandType.StoredProcedure);
                    }
                    catch (Exception e)
                    {
                        result.OperationMessage = "Errors: Ex:  " + e.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                result.OperationMessage = "Errors: Exception: " + ex.Message;
            }

            return result;
        }

    }
}
