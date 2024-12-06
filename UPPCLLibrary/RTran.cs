using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Newtonsoft.Json;
using UPPCLLibrary.BillFail;
using UPPCLLibrary.BillPost;

namespace UPPCLLibrary
{
    public class RTran
    {
        public string Id { get; set; }
        public string RetailUserId { get; set; }
        public string OurApiId { get; set; }
        public string ClientApiUserReferenceId { get; set; }
        public string ApiAccountId { get; set; }
        public string RefundTransactionId { get; set; }
        public string SimId { get; set; }
        public string RetailerSimMobileNumber { get; set; }
        public decimal? RetailerSimOpeningBalance { get; set; }
        public decimal? RetailerSimClosingBalance { get; set; }
        public decimal? RetailerSimROffer { get; set; }
        public decimal? RetailerSimExtraDeduction { get; set; }
        public string TelecomOperatorName { get; set; }
        public string ApiOperatorCode { get; set; }
        public string OurApiOperator { get; set; }
        public string OurApiSubOperator { get; set; }
        public string OperatorCircle { get; set; }
        public string RechargeMobileNumber { get; set; }
        public short? TranType { get; set; }
        public string RechargeType { get; set; }
        public string RechargeStatus { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Deduction { get; set; }
        public decimal? FinalAmount { get; set; }
        public String Parameter1 { get; set; }
        public String Parameter2 { get; set; }
        public String Parameter3 { get; set; }
        public String Parameter4 { get; set; }
        public decimal? OpeningBalance { get; set; }
        public decimal? DebitAmount { get; set; }
        public decimal? CreditAmount { get; set; }
        public decimal? ClosingBalance { get; set; }
        public decimal? SalePrice { get; set; }
        public decimal? CGst { get; set; }
        public decimal? SGst { get; set; }
        public decimal? IGst { get; set; }
        public decimal? Margin { get; set; }
        public decimal? ReceivedOpenningBalance { get; set; }
        public decimal? ReceivedClosingBalance { get; set; }
        /// <summary>
        /// 1=Credit,2=Debit
        /// </summary>
        public byte? ProviderMarginCreditDebit { get; set; }
        public decimal? ProviderMarginAmount { get; set; }
        public DateTime? OurApiTime { get; set; }
        public byte? RequestSource { get; set; }
        public string RequestIp { get; set; }
        public string RequestMachine { get; set; }
        public string RequestGeoCode { get; set; }
        public string RequestNumber { get; set; }
        public string RequestMessage { get; set; }
        public string RequestUrl { get; set; }
        public DateTime? RequestTime { get; set; }
        public string InitialResponseData { get; set; }
        public DateTime? InitialResponseDataTime { get; set; }
        public byte CallBackCount { get; set; }
        public string CallbackData { get; set; }
        public DateTime? CallbackDataTime { get; set; }
        public bool IsFinalStatusChanged { get; set; }
        public string NewFinalStatusReceived { get; set; }
        public bool IsFinalStatusChecked { get; set; }
        public string FinalStatusCheckData { get; set; }
        public DateTime? FinalStatusCheckDate { get; set; }
        public DateTime? StatuscheckTime { get; set; }
        public string StatuscheckData { get; set; }
        public DateTime? StatuscheckDataTime { get; set; }
        public short StatusCheckCount { get; set; }
        public string OtherApiStatusCode { get; set; }
        public string OtherApiId { get; set; }
        public string LiveId { get; set; }
        public string OriginalLiveId { get; set; }
        public string EndCustomerName { get; set; }
        public string EndCustomerMobileNumber { get; set; }
        public string EndCustomerEMail { get; set; }
        public string ResponseId { get; set; }
        public string ParsingId { get; set; }
        public short? Gateway { get; set; }
        public string Remarks { get; set; }
        public string ApiErrorCode { get; set; }
        public string ApiErrorDescription { get; set; }
        public string ValidationExpression { get; set; }
        public string ValidationUrl { get; set; }
        public short? ValidationDaysCount { get; set; }
        public bool? IsSpecialValidationChecked { get; set; }
        public bool? IsSpecialBlock { get; set; }
        public bool? IsSuccessFail { get; set; }
        public bool Approved { get; set; }
        public long RetailUserOrderNo { get; set; }
        public long OrderNo { get; set; }
        public short? WorkStatus { get; set; }
        public bool FailoverDone { get; set; }
        public short? FailoverCount { get; set; }
        public string RefundRequestId { get; set; }
        public DateTime? RefundRequestDate { get; set; }
        public string RefundRequestData { get; set; }
        public string RefundResponse { get; set; }
        public DateTime? RefundResponseDate { get; set; }
        public bool? IsRefundProcessed { get; set; }
        public string CustomerComplainId { get; set; }
        public DateTime? CustomerComplainDate { get; set; }
        public string CustomerComplainRemarks { get; set; }
        public bool? IsManualUpdate { get; set; }
        public string ManualUpdateUser { get; set; }
        public DateTime? ManualUpdateTime { get; set; }
        public string ManualUpdateMachine { get; set; }
        public string Extra1 { get; set; }
        public string Extra2 { get; set; }
        public string ApiUpdateData { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? ConfirmDate { get; set; }
        public bool? IsMarginDistributed { get; set; }
        public string UPPCL_ProjectArea { get; set; }
        public bool? UPPCL_TDConsumer { get; set; }
        public string UPPCL_ConnectionType { get; set; }
        public string UPPCL_DivCode { get; set; }
        public string UPPCL_SDOCode { get; set; }
        public decimal? UPPCL_AccountInfo { get; set; }
        public decimal? UPPCL_BillAmount { get; set; }
        public string UPPCL_Division { get; set; }
        public string UPPCL_SubDivision { get; set; }
        public string UPPCL_PurposeOfSupply { get; set; }
        public decimal? UPPCL_SanctionedLoadInKW { get; set; }
        public string UPPCL_BillId { get; set; }
        public DateTime? UPPCL_BillPostDate { get; set; }
        public DateTime? UPPCL_BillStatusCheckDate { get; set; }
        public short? UPPCL_BillStatusRetryCount { get; set; }
        public bool? UPPCl_IsForcefullyFailed { get; set; }
        public DateTime? UPPCL_OurForcefullyFailedTime { get; set; }
        public string UPPCL_BillDate { get; set; }

        public async void UpdateAfterCallbackProcessUPPCL()
        {
            using (var con = new SqlConnection(UPPCLManager.DbConnection))
            {
                try
                {
                    var queryParameters = new DynamicParameters();
                    queryParameters.Add("@Id", Id);
                    queryParameters.Add("@RetailUserId", RetailUserId);
                    queryParameters.Add("@CallbackData", CallbackData);
                    queryParameters.Add("@CallbackDataTime", CallbackDataTime);
                    queryParameters.Add("@OtherApiStatusCode", OtherApiStatusCode);
                    queryParameters.Add("@OtherApiId", OtherApiId);
                    queryParameters.Add("@LiveId", LiveId);
                    queryParameters.Add("@OriginalLiveId", OriginalLiveId);
                    queryParameters.Add("@TranType", TranType);
                    queryParameters.Add("@RechargeStatus", RechargeStatus);
                    queryParameters.Add("@WorkStatus", 1);
                    queryParameters.Add("@ConfirmDate", DateTime.Now);
                    await con.ExecuteAsync("usp_RTranUpdateAfterCallbackUPPCL", queryParameters, commandType: System.Data.CommandType.StoredProcedure);
                }
                //catch (Exception ex)
                //{
                //    //StaticData.WriteException("RTran : UpdateAfterCallbackProcess: " + ex.Message);
                //}
                finally
                {
                }
            }
        }

        public void ManualUpdate(string statusCheckdata)
        {
            using (var con = new SqlConnection(UPPCLManager.DbConnection))
            {
                try
                {
                    statusCheckdata = statusCheckdata.Length > 490 ? statusCheckdata.Substring(0, 485) : statusCheckdata;
                    
                    var queryParameters = new DynamicParameters();
                    queryParameters.Add("@Id", Id);
                    queryParameters.Add("@ManualUpdateUser", "SYNC");
                    queryParameters.Add("@ApiErrorDescription", statusCheckdata);
                    string result = con.Query<string>("usp_RTranUPPCLManualUpdate", queryParameters, commandType: System.Data.CommandType.StoredProcedure).SingleOrDefault();
                    string answer = Id + Environment.NewLine + statusCheckdata + Environment.NewLine + DateTime.Now + Environment.NewLine + result;
                }
                catch (Exception ex)
                {
                    string error = ex.Message;
                }
                finally
                {
                }
            }
        }

        public StatusCheckResponse StatusCheck(bool updateTransaction = false, bool forceFail = false)
        {
            StatusCheckResponse statusCheckResponse = new StatusCheckResponse();
            try
            {
                statusCheckResponse = UPPCLManager.StatusCheck(UPPCL_BillId, RechargeMobileNumber,UPPCLManager.uppclConfig.AgentVANNo);
                if (updateTransaction)
                {
                    if (statusCheckResponse.status == "FAILED" || statusCheckResponse.status.ToUpper() == "FORCEFULLY_FAILED")
                    {
                        RechargeStatus = "FAILURE";
                        OtherApiId = "FAILURE";
                        LiveId = "";
                        WorkStatus = 2;

                        TranType = 10;

                        if (forceFail || statusCheckResponse.message.Contains("No Record found") || statusCheckResponse.status.ToUpper() == "FORCEFULLY_FAILED")
                        {
                            var ffResponse = ForceFail();
                            if(ffResponse != null)
                            {
                                if(ffResponse.code == 200 || ffResponse.code == 409)
                                {
                                    UpdateAfterCallbackProcessUPPCL();
                                }
                            }
                        }
                        else
                        {
                            UpdateAfterCallbackProcessUPPCL();
                        }

                        statusCheckResponse.message = "";

                    }

                    if (statusCheckResponse.status == "SUCCESS" && Amount == Convert.ToDecimal(statusCheckResponse.billAmount))
                    {
                        RechargeStatus = "SUCCESS";
                        OtherApiId = statusCheckResponse.transactionId;
                        LiveId = statusCheckResponse.transactionId;
                        WorkStatus = 2;
                        TranType = 3;

                        UpdateAfterCallbackProcessUPPCL();

                        statusCheckResponse.message = "";
                    }

                }
            }
            catch (Exception ex)
            {
                statusCheckResponse.message += "Errors: Exception: " + ex.Message;
            }
            return statusCheckResponse;
        }

        public ForceFailResponse ForceFail(bool updateTransaction = false)
        {
            ForceFailResponse forceFailResponse = new ForceFailResponse();
            try
            {
                ForceFailRequest forceFailRequest = new ForceFailRequest();
                forceFailRequest.transactionDate = UPPCLManager.CurrentDate((DateTime)CreateDate);
                forceFailRequest.vanNo = UPPCLManager.uppclConfig.AgentVANNo;
                forceFailRequest.amount = Amount.ToString();
                forceFailRequest.consumerId = RechargeMobileNumber;
                forceFailRequest.billId = UPPCL_BillId;
                forceFailRequest.referenceTransactionId = Id;


                forceFailResponse = UPPCLManager.ForceFail(forceFailRequest);

                if (updateTransaction)
                {
                    if (!string.IsNullOrEmpty(forceFailResponse.code.ToString()))
                    {
                        if (forceFailResponse.code == 200 || forceFailResponse.code == 409) //forceFailResponse.code == 200 || forceFailResponse.code == 409
                        {
                            RechargeStatus = "FAILURE";
                            OtherApiId = "FAILURE";
                            LiveId = "";
                            WorkStatus = 2;

                            TranType = 10;

                            UpdateAfterCallbackProcessUPPCL();

                            using (var con = new SqlConnection(UPPCLManager.DbConnection))
                            {
                                try
                                {
                                    var queryParameters = new DynamicParameters();
                                    queryParameters.Add("@Id", Id);
                                    string result = con.Query<string>("usp_ForceFailChecked", queryParameters, commandType: System.Data.CommandType.StoredProcedure).SingleOrDefault();

                                }
                                catch (Exception ex)
                                {
                                    string error = ex.Message;
                                }
                                finally
                                {
                                }
                            }
                        }

                       
                    }
                }

            }
            catch (Exception ex)
            {
                forceFailResponse.message += "Errors: Exception: " + ex.Message;
            }
            return forceFailResponse;
        }


        public RTran LoadRecord()
        {
            RTran rtnew = new RTran();
            try
            {
                using (var con = new SqlConnection(UPPCLManager.DbConnection))
                {
                    var queryParameters = new DynamicParameters();
                    queryParameters.Add("@Id", Id);
                    List<RTran> rt = con.Query<RTran>("usp_RTranSelectV2", queryParameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    if (rt != null)
                    {
                        if (rt.Count == 1)
                        {
                            rtnew = rt[0];
                        }
                    }
                }
            }
            catch (Exception)
            {
                //StaticData.WriteException("Ex: LoadRecord: " + Id + ", " + ex.Message);
            }
            return rtnew;
        }

    }
}
