using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Dapper;
using Newtonsoft.Json;
using SaralESuvidha.ViewModel;
using UPPCLLibrary;

namespace SaralESuvidha.Models
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

        /// <summary>
        /// 0=Initial
        //1=SentToApi
        //2=Suspense
        //3=Success
        //10=Failed
        /// </summary>
        public int TranType { get; set; }

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
        public int RetailUserOrderNo { get; set; }

        public long OrderNo { get; set; }

        /// <summary>
        /// 0=Initial
        //1=Sent To Api
        //2=Suspense
        //3=Success
        //10=Failed
        /// </summary>
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
        public string UPPCL_Discom { get; set; }
        public string UPPCL_BillDate { get; set; }
        public string UPPCL_PaymentType { get; set; }
        public string UPPCL_ConsumerAddress { get; set; }
        public decimal? UPPCL_BalanceAfterPayment { get; set; }
        public decimal? UPPCL_DDR { get; set; }

        public int IsOTS { get; set; }
        public int? IsFull { get; set; }

        public string UPPCL_LifelineAct { get; set; }
        
        public string UPPCL_EventId { get; set; }
        public DateTime? UPPCL_EventTime { get; set; }
        public string UPPCL_AgencyVAN { get; set; }
        public string UPPCL_DebitVAN { get; set; }
        public string UPPCL_AgentVAN { get; set; }
        public string UPPCL_CreditVAN { get; set; }
        public DateTime? UPPCL_TransactionDate { get; set; }
        public decimal? UPPCL_Amount { get; set; }
        public decimal? UPPCL_Balance { get; set; }
        public string UPPCL_TransactionStatus { get; set; }
        public string UPPCL_TransactionId { get; set; }
        /// <summary>
        /// 0=Initial, 1=Pushed, 2=Fail, 3=success
        /// </summary>
        public byte? UPPCL_FundStatus { get; set; }
        public string UPPCL_Status { get; set; }
        
        public string TransferFundToRetailUser()
        {
            string result = "Info: Starting transfer process";

            using (var con = new SqlConnection(StaticData.conString))
            {
                try
                {
                    var queryParameters = new DynamicParameters();
                    queryParameters.Add("@RetailUserOrderNo", RetailUserOrderNo);
                    queryParameters.Add("@Amount", Amount);
                    queryParameters.Add("@Deduction", Deduction);
                    queryParameters.Add("@FinalAmount", FinalAmount);
                    queryParameters.Add("@OpeningBalance", OpeningBalance);
                    queryParameters.Add("@DebitAmount", DebitAmount);
                    queryParameters.Add("@CreditAmount", CreditAmount);
                    queryParameters.Add("@ClosingBalance", ClosingBalance);
                    queryParameters.Add("@Margin", Margin);
                    queryParameters.Add("@RequestIp", RequestIp);
                    queryParameters.Add("@RequestMachine", RequestMachine);
                    queryParameters.Add("@RequestGeoCode", RequestGeoCode);
                    queryParameters.Add("@RequestNumber", RequestNumber);
                    queryParameters.Add("@RequestMessage", RequestMessage);
                    queryParameters.Add("@RequestTime", RequestTime);
                    queryParameters.Add("@TranType", TranType);
                    queryParameters.Add("@Remarks", Remarks);
                    queryParameters.Add("@Extra1", Extra1);
                    queryParameters.Add("@Extra2", Extra2);
                    queryParameters.Add("@CreateDate", CreateDate);
                    queryParameters.Add("@ConfirmDate", ConfirmDate);


                    RTranApiFundTransfer au = con.QuerySingle<RTranApiFundTransfer>("usp_RetailClientFundTransfer", queryParameters, commandType: System.Data.CommandType.StoredProcedure);

                    result = au == null  ? "Errors: Can not transfer fund. Error in transferring fund."  : au.OperationMessage;
                    
                    /* //Disable UPPCL Direct Transfer.
                    if (!string.IsNullOrEmpty(au.Id) && au.UserType == 5)
                    {
                        UPPCLLibrary.RTran rTran = new UPPCLLibrary.RTran();
                        rTran.Id = au.Id;

                        result += ". UPPCL Transfer Status - " + rTran.TransferFundOnUPPCL();
                    }
                    */
                    
                }
                catch (Exception ex)
                {
                    result = "Errors: Ex148 " + ex.Message;
                }
            }

            return result;
        }

        public string TransferFundToUWalletRetailUser()
        {
            string result = "Info: Starting transfer process";

            using (var con = new SqlConnection(StaticData.conString))
            {
                try
                {
                    var queryParameters = new DynamicParameters();
                    queryParameters.Add("@RetailUserOrderNo", RetailUserOrderNo);
                    queryParameters.Add("@Amount", Amount);
                    queryParameters.Add("@Deduction", Deduction);
                    queryParameters.Add("@FinalAmount", FinalAmount);
                    queryParameters.Add("@OpeningBalance", OpeningBalance);
                    queryParameters.Add("@DebitAmount", DebitAmount);
                    queryParameters.Add("@CreditAmount", CreditAmount);
                    queryParameters.Add("@ClosingBalance", ClosingBalance);
                    queryParameters.Add("@Margin", Margin);
                    queryParameters.Add("@RequestIp", RequestIp);
                    queryParameters.Add("@RequestMachine", RequestMachine);
                    queryParameters.Add("@RequestGeoCode", RequestGeoCode);
                    queryParameters.Add("@RequestNumber", RequestNumber);
                    queryParameters.Add("@RequestMessage", RequestMessage);
                    queryParameters.Add("@RequestTime", RequestTime);
                    queryParameters.Add("@TranType", TranType);
                    queryParameters.Add("@Remarks", Remarks);
                    queryParameters.Add("@Extra1", Extra1);
                    queryParameters.Add("@Extra2", Extra2);
                    queryParameters.Add("@CreateDate", CreateDate);
                    queryParameters.Add("@ConfirmDate", ConfirmDate);


                    RTranApiFundTransfer au = con.QuerySingle<RTranApiFundTransfer>("usp_RetailClientFundTransferToUWallet", queryParameters, commandType: System.Data.CommandType.StoredProcedure);

                    result = au == null  ? "Errors: Can not transfer fund. Error in transferring fund to UWALLET."  : au.OperationMessage;
                }
                catch (Exception ex)
                {
                    result = "Errors: Ex148 " + ex.Message;
                }
            }

            return result;
        }

        public string TransferFundByData(string userRole)
        {
            string result = string.Empty;
            try
            {
                if (userRole == "admin")
                {
                    Deduction = 0;
                    FinalAmount = null;
                    OpeningBalance = null;
                    //DebitAmount = 0;
                    ClosingBalance = null;
                    Margin = null;
                    RequestTime = DateTime.UtcNow.AddHours(5.5);
                    //TranType = 11;
                    //Extra1 = null;
                    //Extra2 = null;
                    CreateDate = null;
                    ConfirmDate = null;
                    result = TransferFundToRetailUser();
                }
                else
                {
                    result = "Can not transfer fund. Not authorized.";
                }
            }
            catch (Exception ex)
            {
                result = "Error: Ex Error in transferring fund. " + ex.Message;
            }

            return result;
        }

        public string TransferFundTuUWalletByData(string userRole)
        {
            string result = string.Empty;
            try
            {
                if (userRole == "admin")
                {
                    Deduction = 0;
                    FinalAmount = null;
                    OpeningBalance = null;
                    ClosingBalance = null;
                    Margin = null;
                    RequestTime = DateTime.UtcNow.AddHours(5.5);
                    CreateDate = null;
                    ConfirmDate = null;
                    result = TransferFundToUWalletRetailUser();
                }
                else
                {
                    result = "Can not transfer fund. Not authorized.";
                }
            }
            catch (Exception ex)
            {
                result = "Error: Ex Error in transferring fund. " + ex.Message;
            }

            return result;
        }

        public string UpdateStatusByData(RTranPending rTran)
        {
            string result = string.Empty;
            try
            {
                LiveId = rTran.txtUpdateLiveId;
                Id = StaticData.ConvertHexToString(rTran.txtUpdateId);
                if (!string.IsNullOrEmpty(rTran.txtUpdateClientId))
                {
                    ApiAccountId = StaticData.ConvertHexToString(rTran.txtUpdateClientId);
                }

                RechargeMobileNumber = rTran.txtUpdateRechargeNumber;
                Amount = rTran.txtUpdateAmount;
                ClientApiUserReferenceId = rTran.txtUpdateClientApiUserReference;
                TranType = rTran.txtUpdateStatus == "1" ? 3 : 10;
                RechargeStatus = rTran.txtUpdateStatus == "1" ? "SUCCESS" : "FAILURE";

                using (var con = new SqlConnection(StaticData.conString))
                {
                    var queryParameters = new DynamicParameters();
                    queryParameters.Add("@Id", Id);
                    queryParameters.Add("@RechargeMobileNumber", RechargeMobileNumber);
                    queryParameters.Add("@LiveId", LiveId);
                    queryParameters.Add("@TranType", TranType);
                    queryParameters.Add("@RechargeStatus", RechargeStatus);
                    queryParameters.Add("@WorkStatus", 1);
                    queryParameters.Add("@ClientApiUserReferenceId", ClientApiUserReferenceId);
                    queryParameters.Add("@ConfirmDate", DateTime.Now);
                    queryParameters.Add("@Amount", Amount);
                    queryParameters.Add("@ApiAccountId", ApiAccountId);
                    queryParameters.Add("@Remarks", "Update by operator.");

                    con.Query("usp_RTranUpdateManual", queryParameters,
                        commandType: System.Data.CommandType.StoredProcedure);

                    result = "Success: Successfully updated recharge status to " + RechargeStatus + ".";
                }
            }
            catch (Exception ex)
            {
                result = "Error: Error in updating status. " + ex.Message + Id + "|" + ApiAccountId;
            }

            return result;
        }

        public string PayBill(string inputSource = "web")
        {
            string result = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(RetailUserOrderNo.ToString()) || string.IsNullOrEmpty(RechargeMobileNumber) ||
                    string.IsNullOrEmpty(Amount.ToString()) || string.IsNullOrEmpty(TelecomOperatorName))
                {
                    result = "Invalid user data. Can not process bill payment.";
                }
                else
                {
                    using (var con = new SqlConnection(StaticData.conString))
                    {
                        var queryParameters = new DynamicParameters();
                        //queryParameters.Add("@Id", Id);
                        queryParameters.Add("@RetailUserOrderNo", RetailUserOrderNo);
                        queryParameters.Add("@MobileNumber", RechargeMobileNumber);
                        queryParameters.Add("@OperatorName", TelecomOperatorName);
                        queryParameters.Add("@Amount", Amount);
                        queryParameters.Add("@RechargeType", "R");
                        queryParameters.Add("@RequestIP", RequestIp);
                        queryParameters.Add("@RequestMachine", RequestMachine);
                        queryParameters.Add("@RequestGeoCode", RequestGeoCode);
                        queryParameters.Add("@RequestNumber", RequestNumber);
                        queryParameters.Add("@RequestMessage", RequestMessage);
                        queryParameters.Add("@RequestTime", DateTime.Now);
                        queryParameters.Add("@Parameter1", Parameter1);
                        queryParameters.Add("@Parameter2", Parameter2);
                        queryParameters.Add("@Parameter3", Parameter3);
                        queryParameters.Add("@Parameter4", Parameter4);
                        queryParameters.Add("@EndCustomerName", EndCustomerName);
                        queryParameters.Add("@Extra1", Extra1);
                        queryParameters.Add("@Extra2", Extra2);

                        var resp = con.QuerySingleOrDefault<RTranValidateResponse>("usp_RTranValidate", queryParameters,
                            commandType: System.Data.CommandType.StoredProcedure);
                        string printMessage = "";
                        if (resp != null)
                        {
                            if (resp.Id != null)
                            {
                                //printMessage = " <a target='_blank' href='Home/PrintReceipt?t=" + StaticData.ConvertStringToHex(resp.Id) + "'>PRINT RECEIPT</a>";
                                printMessage = " <span class='btn btn-primary' onclick=\'PrintReceipt(\"" +
                                               StaticData.ConvertStringToHex(resp.Id) + "\");\'>PRINT RECEIPT</span>";
                            }
                        }

                        if (StaticData.loginSource == "mobile")
                        {
                            printMessage = "=" + resp.Id;
                        }

                        result = "Bill payment / recharge status of " + TelecomOperatorName + " of account " +
                                 RechargeMobileNumber + " of Amount " + Amount?.ToString("N2") + ". " +
                                 resp.OperationMessage + " Balance: " + resp.ClosingBalance?.ToString("N2") +
                                 printMessage;
                    }
                }
            }
            catch (Exception ex)
            {
                result += "Errors: Exception: " + ex.Message;
            }

            return result;
        }

 
        public string UpdateBeforeUPPCLPush()
        {
            string result = "Starting update.";
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var queryParameters = new DynamicParameters();
                    queryParameters.Add("@Id", Id);

                    result = con.Query<string>("usp_RTranUPPCLBeforeCallUpdate", queryParameters,
                        commandType: System.Data.CommandType.StoredProcedure).SingleOrDefault();
                }
            }
            catch (Exception ex)
            {
                result += "Exception: " + ex.Message;
            }

            return result;
        }

        public bool ValidBillPaymentInput()
        {
            bool result = true;
            try
            {
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }
        

        public RTran LoadRecord()
        {
            RTran rtnew = new RTran();
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var queryParameters = new DynamicParameters();
                    queryParameters.Add("@Id", Id);
                    List<RTran> rt = con.Query<RTran>("usp_RTranSelectV2", queryParameters,
                        commandType: System.Data.CommandType.StoredProcedure).ToList();
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

        public RTran LoadRecordByOtherApiId()
        {
            RTran rtnew = new RTran();
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    try
                    {
                        var queryParameters = new DynamicParameters();
                        queryParameters.Add("@OtherApiId", OtherApiId);
                        List<RTran> rt = con.Query<RTran>("usp_RTranSelectByOtherApiIdV2", queryParameters,
                            commandType: System.Data.CommandType.StoredProcedure).ToList();
                        if (rt != null)
                        {
                            if (rt.Count == 1)
                            {
                                rtnew = rt[0];
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch (Exception)
            {
            }

            return rtnew;
        }

        public void UpdateSuccessFail()
        {
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var queryParameters = new DynamicParameters();
                    queryParameters.Add("@Id", Id);

                    con.Query("usp_RTranSuccessFailUpdate", queryParameters,
                        commandType: System.Data.CommandType.StoredProcedure);
                }
            }
            catch
            {
                // ignored
            }
        }

        public void UpdateCallbackCount()
        {
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var queryParameters = new DynamicParameters();
                    queryParameters.Add("@Id", Id);
                    queryParameters.Add("@CallBackCount", CallBackCount);

                    con.Query("usp_RTranIncrementCallBackCount", queryParameters,
                        commandType: System.Data.CommandType.StoredProcedure);
                }
            }
            catch
            {
                // ignored
            }
        }
        
        public ApiConfigOperator GetApiConfigOperator()
        {
            return StaticDatabaseData.apiConfigOperators.SingleOrDefault(ac =>
                ac.ApiId == OurApiId && ac.ApiOperatorCode == ApiOperatorCode &&
                ac.InternalOperatorCode == OurApiOperator &&
                ac.RechargeType == RechargeType); // && ac.Gateway == Gateway
        }
        
        public async void UpdateAfterCallbackProcessUPPCL()
        {
            using (var con = new SqlConnection(StaticData.conString))
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
                    queryParameters.Add("@UPPCL_PaymentType", UPPCL_PaymentType);
                    queryParameters.Add("@UPPCL_DDR", UPPCL_DDR);
                    queryParameters.Add("@ConfirmDate", DateTime.Now);
                    await con.ExecuteAsync("usp_RTranUpdateAfterCallbackUPPCL", queryParameters,
                        commandType: System.Data.CommandType.StoredProcedure);
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
            using (var con = new SqlConnection(StaticData.conString))
            {
                try
                {
                    statusCheckdata = statusCheckdata.Length > 490 ? statusCheckdata.Substring(0, 485)  : statusCheckdata;

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


        public string SaveRefundRequest()
        {
            var result = "Info: Starting transfer process";

            using var con = new SqlConnection(StaticData.conString);
            try
            {
                var queryParameters = new DynamicParameters();
                queryParameters.Add("@RetailUserId", RetailUserId);
                queryParameters.Add("@RTranId", Id);
                queryParameters.Add("@RefundRequestData", RefundRequestData);
                OperationResponse au = con.QuerySingle<OperationResponse>("usp_RetailClientSaveDispute", queryParameters, commandType: System.Data.CommandType.StoredProcedure);

                result = au == null ? "Errors: Can not create refund request." : au.OperationMessage;
            }
            catch (Exception ex)
            {
                result = "Errors: " + ex.Message;
            }

            return result;
        }

        public string SaveInitialPushResponse()
        {
            //bool result = true;
            string result = string.Empty;
            //RTran newTran = new RTran();

            using (var con = new SqlConnection(StaticData.conString))
            {
                try
                {
                    var queryParameters = new DynamicParameters();
                    queryParameters.Add("@Id", Id);
                    queryParameters.Add("@RetailUserId", ApiAccountId);
                    queryParameters.Add("@RechargeMobileNumber", RechargeMobileNumber);
                    queryParameters.Add("@Amount", Amount);
                    queryParameters.Add("@Remarks", Remarks);
                    queryParameters.Add("@SimMobileNumber", RetailerSimMobileNumber);
                    queryParameters.Add("@OpeningBalance", OpeningBalance);
                    queryParameters.Add("@ClosingBalance", ClosingBalance);
                    queryParameters.Add("@RequestUrl", RequestUrl);
                    queryParameters.Add("@RequestTime", RequestTime);
                    queryParameters.Add("@InitialResponseData", InitialResponseData);
                    queryParameters.Add("@InitialResponseDataTime", InitialResponseDataTime);
                    queryParameters.Add("@OtherApiId", OtherApiId);
                    queryParameters.Add("@LiveId", LiveId);
                    queryParameters.Add("@TranType", TranType);
                    queryParameters.Add("@RechargeStatus", RechargeStatus);
                    queryParameters.Add("@ResponseId", ResponseId);
                    queryParameters.Add("@ParsingId", ParsingId);
                    queryParameters.Add("@WorkStatus", WorkStatus);
                    queryParameters.Add("@ReceivedOpenningBalance", ReceivedOpenningBalance);
                    queryParameters.Add("@ReceivedClosingBalance", ReceivedClosingBalance);
                    queryParameters.Add("@OtherApiStatusCode", OtherApiStatusCode);
                    queryParameters.Add("@OriginalLiveId", OriginalLiveId);
                    queryParameters.Add("@ApiErrorCode", ApiErrorCode);
                    queryParameters.Add("@ApiErrorDescription", ApiErrorDescription);
                    queryParameters.Add("@ClientApiUserReferenceId", ClientApiUserReferenceId);
                    queryParameters.Add("@RetailerSimOpeningBalance", RetailerSimOpeningBalance);
                    queryParameters.Add("@RetailerSimClosingBalance", RetailerSimClosingBalance);
                    queryParameters.Add("@ConfirmDate", ConfirmDate);
                    result = "Before Save Call" + Environment.NewLine; // + JsonConvert.SerializeObject(this) + Environment.NewLine;

                    var itran = con.Query<RTran>("usp_RTranUpdateAfterApiCallV3", queryParameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    if (itran.Count < 1)
                    {
                        //result = false;
                        result += "Save call not successfull" + Environment.NewLine;
                    }
                    else
                    {
                        result += "Transaction [" + itran[0].RechargeMobileNumber + "] successfully updated. Record ID - [" + itran[0].Id + "].";
                        //newTran = itran[0];
                    }
                }
                catch (Exception exSaveInitialResponse)
                {
                    //result = false;
                    result += Environment.NewLine + " exSaveInitialResponse: " + exSaveInitialResponse.Message;
                    //Utility.WriteException(result);
                }
            }

            return result;
        }
    }
}