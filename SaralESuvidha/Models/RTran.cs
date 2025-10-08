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
using UPPCLLibrary.BillFail;
using UPPCLLibrary.BillFetch;
using UPPCLLibrary.BillPost;
using UPPCLLibrary.OTS;

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

        public (string, string) PayOTSUPPCL(CaseInitResponse initResponse, decimal outStandingAmount, string inputSource = "web",
            string clientReferenceId = "")
        {
            string result = string.Empty;
            string reciptId = string.Empty;
            if (string.IsNullOrEmpty(RetailUserOrderNo.ToString()) || string.IsNullOrEmpty(RechargeMobileNumber) ||
                    string.IsNullOrEmpty(Amount.ToString()) || string.IsNullOrEmpty(TelecomOperatorName) ||
                    string.IsNullOrEmpty(initResponse.Data.BillDetails.BillId) || string.IsNullOrEmpty(initResponse.Data.BillDetails.KNumber))
            {
                result = "Errors: Invalid user data. Can not process bill payment. Please try after login again.";
            }
            else
            {
                outStandingAmount = Math.Round(outStandingAmount);
                decimal accountInfoInt = Convert.ToDecimal(initResponse.Data.BillDetails.AccountInfo);
                decimal billAmountInt = Convert.ToDecimal(initResponse.Data.BillDetails.BillAmount);
                decimal dueDateRebate = Convert.ToDecimal(initResponse.Data.BillDetails.Param1);
                dueDateRebate = Math.Ceiling(dueDateRebate);
                UPPCL_DDR = dueDateRebate;
                accountInfoInt = Math.Round(accountInfoInt);
                billAmountInt = Math.Round(billAmountInt);
                string paymentTypeFullPartial = "";
                if ((accountInfoInt + dueDateRebate) >= billAmountInt && Amount >= accountInfoInt)
                {
                    paymentTypeFullPartial = "FULL";
                }
                else if ((accountInfoInt + dueDateRebate) >= billAmountInt && Amount < accountInfoInt)
                {
                    paymentTypeFullPartial = "PARTIAL";
                }
                else if ((accountInfoInt + dueDateRebate) < billAmountInt && Amount > accountInfoInt)
                {
                    paymentTypeFullPartial = "PARTIAL";
                }
                else if ((accountInfoInt + dueDateRebate) < billAmountInt && Amount <= accountInfoInt)
                {
                    paymentTypeFullPartial = "PARTIAL";
                }

                UPPCL_PaymentType = paymentTypeFullPartial;

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
                    queryParameters.Add("@EndCustomerMobileNumber", EndCustomerMobileNumber);
                    queryParameters.Add("@Extra1", Extra1);
                    queryParameters.Add("@Extra2", Extra2);
                    queryParameters.Add("@UPPCL_ProjectArea", UPPCL_ProjectArea);
                    queryParameters.Add("@UPPCL_AccountInfo", UPPCL_AccountInfo);
                    queryParameters.Add("@UPPCL_TDConsumer", UPPCL_TDConsumer);
                    queryParameters.Add("@UPPCL_ConnectionType", UPPCL_ConnectionType);
                    queryParameters.Add("@UPPCL_DivCode", UPPCL_DivCode);
                    queryParameters.Add("@UPPCL_SDOCode", UPPCL_SDOCode);
                    queryParameters.Add("@UPPCL_BillAmount", UPPCL_BillAmount);
                    queryParameters.Add("@UPPCL_Division", UPPCL_Division);
                    queryParameters.Add("@UPPCL_SubDivision", UPPCL_SubDivision);
                    queryParameters.Add("@UPPCL_PurposeOfSupply", UPPCL_PurposeOfSupply);
                    queryParameters.Add("@UPPCL_SanctionedLoadInKW", UPPCL_SanctionedLoadInKW);
                    queryParameters.Add("@UPPCL_BillId", UPPCL_BillId);
                    queryParameters.Add("@UPPCL_Discom", UPPCL_Discom);
                    queryParameters.Add("@UPPCL_BillDate", UPPCL_BillDate);
                    queryParameters.Add("@UPPCL_PaymentType", UPPCL_PaymentType);
                    queryParameters.Add("@UPPCL_DDR", dueDateRebate);
                    queryParameters.Add("@ClientApiUserReferenceId", clientReferenceId);
                    queryParameters.Add("@IsOTS", IsOTS);
                    queryParameters.Add("@IsFull", IsFull);
                    queryParameters.Add("@UPPCL_LifelineAct", UPPCL_LifelineAct);
                    RechargeStatus = "PROCESS";

                    var resp = con.QuerySingleOrDefault<RTranValidateResponse>("usp_RTranValidateUPPCL", queryParameters, commandType: System.Data.CommandType.StoredProcedure);
                    string printMessage = "";
                    if (resp != null)
                    {
                        if (!string.IsNullOrEmpty(resp.Id))
                        {
                            Id = resp.Id;
                            // set the workstatus to 1 and billpost date to current date time.
                            var updateTime = UpdateBeforeUPPCLPush();
                            if (updateTime.Contains("Success"))
                            {
                                BillPaymentRequest billPaymentRequest = new BillPaymentRequest();
                                billPaymentRequest.agentId = UPPCLManager.uppclConfig.AgentID;
                                billPaymentRequest.agencyType = "OTHER";
                                billPaymentRequest.amount = ((int)Amount).ToString();
                                billPaymentRequest.billAmount = UPPCL_BillAmount.ToString();
                                billPaymentRequest.billId = UPPCL_BillId;
                                billPaymentRequest.connectionType = UPPCL_ConnectionType;
                                billPaymentRequest.consumerAccountId = RechargeMobileNumber;
                                billPaymentRequest.consumerName = initResponse.Data.BillDetails.ConsumerName.Replace(@"\", @"\\")
                                    .Replace(@"/", @"\/").Replace(".", "").Replace("-", "");
                                billPaymentRequest.discom = UPPCL_Discom;
                                billPaymentRequest.division = initResponse.Data.BillDetails.Division;
                                billPaymentRequest.divisionCode = initResponse.Data.BillDetails.DivCode;
                                billPaymentRequest.mobile = string.IsNullOrEmpty(initResponse.Data.BillDetails.MobileNumber) ? "" : initResponse.Data.BillDetails.MobileNumber;
                                billPaymentRequest.outstandingAmount = outStandingAmount.ToString();
                                billPaymentRequest.paymentType = paymentTypeFullPartial;
                                billPaymentRequest.referenceTransactionId = Id;
                                billPaymentRequest.sourceType = initResponse.Data.BillDetails.ProjectArea;
                                billPaymentRequest.type = initResponse.Data.BillDetails.ProjectArea;
                                billPaymentRequest.vanNo = UPPCLManager.uppclConfig.AgentVANNo;
                                billPaymentRequest.walletId = RetailUserId;

                                string city = "NA";
                                try
                                {
                                    if (initResponse.Data.BillDetails.PremiseAddress.City != null)
                                        city = initResponse.Data.BillDetails.PremiseAddress.City;
                                }
                                catch (Exception)
                                {
                                    city = "NA";
                                }

                                if (city.Length < 1)
                                {
                                    city = "NA";
                                }

                                billPaymentRequest.city = city;
                                try
                                {
                                    billPaymentRequest.param1 = initResponse.Data.BillDetails.Param1.ToString() ?? string.Empty;
                                }
                                catch (Exception)
                                {

                                }

                                string successFailMessage = "";

                                try
                                {
                                    BillPostResponse billPostResponse = UPPCLManager.BillPostBillPayment(billPaymentRequest, true);

                                    OtherApiStatusCode = billPostResponse.status;
                                    CallbackData = JsonConvert.SerializeObject(billPostResponse);
                                    ConfirmDate = DateTime.Now;

                                    if (billPostResponse.status == "SUCCESS")
                                    {
                                        RechargeStatus = "SUCCESS";
                                        OtherApiId = billPostResponse.externalTransactionId;
                                        LiveId = billPostResponse.externalTransactionId;
                                        WorkStatus = 2;
                                        TranType = 3;

                                        UpdateAfterCallbackProcessUPPCL();
                                    }
                                    else if (billPostResponse.status == "FAILED")
                                    {
                                        if (billPostResponse.message.Contains("enough funds in wallet"))
                                        {
                                            printMessage = "Errors: Can not pay bill. Insufficient balance.";
                                        }

                                        if (billPostResponse.message.Contains("payment was already done"))
                                        {
                                            printMessage = "Errors: Can not pay bill. Payment was already done.";
                                        }

                                        if (billPostResponse.message.Contains("Id and van number do not"))
                                        {
                                            printMessage = "Errors: Can not pay bill. Agent Id and van number do not match.";
                                        }

                                        RechargeStatus = "FAILURE";
                                        OtherApiId = "FAILURE";
                                        LiveId = "";
                                        WorkStatus = 2;

                                        TranType = 10;

                                        UpdateAfterCallbackProcessUPPCL();

                                        Remarks = printMessage;
                                    }

                                    if (!string.IsNullOrEmpty(billPostResponse.message))
                                    {
                                        if (billPostResponse.message.Contains("Internal: Exception: Value cannot be null.") || billPostResponse.message.Contains("Exception: Server:") || billPostResponse.message.Contains("The requested API is temporarily blocked") || billPostResponse.message.Contains("JSON parse error"))
                                        {
                                            Remarks = "Errors: Can not send bill. Please check your recharge report after 1 minute for final status.";

                                            try
                                            {
                                                Thread.Sleep(15000); // Delay 15 seconds before calling status check
                                                Id = resp.Id;
                                                RTran tempTran = LoadRecord();
                                                UPPCL_BillId = tempTran.UPPCL_BillId;
                                                CreateDate = tempTran.CreateDate;
                                                Amount = tempTran.Amount;

                                                StatusCheckResponse statusCheckResponse = StatusCheck(true);

                                                Remarks = (statusCheckResponse.status == "SUCCESS" ? "Success: " : "Errors: ") + statusCheckResponse.message;

                                                //Exception: Server:
                                                if (statusCheckResponse.message.Contains("Status check api down") || statusCheckResponse.message.Contains("Exception: ") || statusCheckResponse.message.Contains("No Record found") || statusCheckResponse.message.Contains("The requested API is temporarily blocked"))
                                                {
                                                    try
                                                    {
                                                        ForceFailResponse forceFailResponse = ForceFail(true);
                                                        //Remarks = "Errors: " + forceFailResponse.message;
                                                        Remarks = "Errors: Payment failed.";
                                                        printMessage = Remarks;
                                                    }
                                                    catch (Exception exFF)
                                                    {
                                                        Remarks = "Errors: Can not send bill. Please check your recharge report after 1 minute for final status. Force fail failed.";
                                                        printMessage = Remarks;
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Remarks = "Errors: Can not send bill. Please check your recharge report after 1 minute for final status. Status check failed.";
                                                printMessage = Remarks;
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    printMessage = "Errors: Can not send bill. Please check your recharge report in 1 minute for final status." + ex.Message;
                                    Remarks += printMessage;
                                }
                            }
                            else
                            {
                                printMessage = "Errors: Can not save record for bill post.";
                            }

                            //after post , update the details with poost data

                            //printMessage = " <a target='_blank' href='Home/PrintReceipt?t=" + StaticData.ConvertStringToHex(resp.Id) + "'>PRINT RECEIPT</a>";
                            reciptId = StaticData.ConvertStringToHex(resp.Id);
                            printMessage = " <span class='btn btn-primary' onclick=\'PrintReceipt(\"" +
                                           StaticData.ConvertStringToHex(resp.Id) + "\");\'>PRINT RECEIPT</span>";
                        }
                        else
                        {
                            Remarks = "Errors: " + resp.OperationMessage;
                            result = Remarks;
                        }
                    }

                    if (StaticData.loginSource == "mobile")
                    {
                        printMessage = "=" + resp.Id;
                    }

                    if (RechargeStatus == "FAILURE")
                    {
                        result = Remarks;
                    }
                    else if (RechargeStatus == "PROCESS")
                    {
                        result = Remarks;
                    }
                    else
                    {
                        result = "Success: Bill payment / recharge status of " + TelecomOperatorName +
                                 " of account " + RechargeMobileNumber + " of Amount " + Amount?.ToString("N2") +
                                 ". " + resp.OperationMessage + " Balance: " + resp.ClosingBalance?.ToString("N2") +
                                 (printMessage.Trim().StartsWith("<span") ? "" : printMessage);
                    }
                }
            }
            return (result, reciptId);
        }

        public string PayBillUPPCL(ESuvidhaBillFetchResponse eSuvidhaBillFetchResponse, string inputSource = "web",
            string clientReferenceId = "")
        {
            string result = string.Empty;
            string printMessageMobile = string.Empty;

            try
            {
                if (string.IsNullOrEmpty(RetailUserOrderNo.ToString()) || string.IsNullOrEmpty(RechargeMobileNumber) ||
                    string.IsNullOrEmpty(Amount.ToString()) || string.IsNullOrEmpty(TelecomOperatorName))
                {
                    result = "Errors: Invalid user data. Can not process bill payment. Please try after login again.";
                }
                else
                {
                    string paymentTypeFullPartial = "";
                    decimal accountInfoInt = Convert.ToDecimal(eSuvidhaBillFetchResponse.BillFetchResponse.Body.PaymentDetailsResponse.AccountInfo);
                    decimal billAmountInt = Convert.ToDecimal(eSuvidhaBillFetchResponse.BillFetchResponse.Body.PaymentDetailsResponse.BillAmount);
                    decimal dueDateRebate = Convert.ToDecimal(eSuvidhaBillFetchResponse.DueDateRebate);
                    dueDateRebate = Math.Ceiling(dueDateRebate);
                    UPPCL_DDR = dueDateRebate;
                    accountInfoInt = Math.Round(accountInfoInt);
                    billAmountInt = Math.Round(billAmountInt);
                    if ((accountInfoInt + dueDateRebate) >= billAmountInt && Amount >= accountInfoInt)
                    {
                        paymentTypeFullPartial = "FULL";
                    }
                    else if ((accountInfoInt + dueDateRebate) >= billAmountInt && Amount < accountInfoInt)
                    {
                        paymentTypeFullPartial = "PARTIAL";
                    }
                    else if ((accountInfoInt + dueDateRebate) < billAmountInt && Amount > accountInfoInt)
                    {
                        paymentTypeFullPartial = "PARTIAL";
                    }
                    else if ((accountInfoInt + dueDateRebate) < billAmountInt && Amount <= accountInfoInt)
                    {
                        paymentTypeFullPartial = "PARTIAL";
                    }


                    /*
                    if (Parameter1?.Length > 9)
                    {
                        EndCustomerMobileNumber = Parameter1;
                    }
                    */

                    if (eSuvidhaBillFetchResponse.BillFetchResponse.Body.PaymentDetailsResponse.ConnectionType.ToLower().Contains("prepaid"))
                    {
                        paymentTypeFullPartial = "FULL";
                        accountInfoInt = 0;
                        UPPCL_BillAmount = 0;
                    }

                    UPPCL_PaymentType = paymentTypeFullPartial;


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
                        queryParameters.Add("@EndCustomerMobileNumber", EndCustomerMobileNumber);
                        queryParameters.Add("@Extra1", Extra1);
                        queryParameters.Add("@Extra2", Extra2);
                        queryParameters.Add("@UPPCL_ProjectArea", UPPCL_ProjectArea);
                        queryParameters.Add("@UPPCL_AccountInfo", UPPCL_AccountInfo);
                        queryParameters.Add("@UPPCL_TDConsumer", UPPCL_TDConsumer);
                        queryParameters.Add("@UPPCL_ConnectionType", UPPCL_ConnectionType);
                        queryParameters.Add("@UPPCL_DivCode", UPPCL_DivCode);
                        queryParameters.Add("@UPPCL_SDOCode", UPPCL_SDOCode);
                        queryParameters.Add("@UPPCL_BillAmount", UPPCL_BillAmount);
                        queryParameters.Add("@UPPCL_Division", UPPCL_Division);
                        queryParameters.Add("@UPPCL_SubDivision", UPPCL_SubDivision);
                        queryParameters.Add("@UPPCL_PurposeOfSupply", UPPCL_PurposeOfSupply);
                        queryParameters.Add("@UPPCL_SanctionedLoadInKW", UPPCL_SanctionedLoadInKW);
                        queryParameters.Add("@UPPCL_BillId", UPPCL_BillId);
                        queryParameters.Add("@UPPCL_Discom", UPPCL_Discom);
                        queryParameters.Add("@UPPCL_BillDate", UPPCL_BillDate);
                        queryParameters.Add("@UPPCL_PaymentType", UPPCL_PaymentType);
                        queryParameters.Add("@UPPCL_DDR", dueDateRebate);
                        queryParameters.Add("@ClientApiUserReferenceId", clientReferenceId);
                        queryParameters.Add("@IsOTS", IsOTS);
                        queryParameters.Add("@IsFull", IsFull);
                        queryParameters.Add("@UPPCL_LifelineAct", UPPCL_LifelineAct);
                        queryParameters.Add("@UPPCL_ConsumerAddress", UPPCL_ConsumerAddress);

                        RechargeStatus = "PROCESS";

                        var resp = con.QuerySingleOrDefault<RTranValidateResponse>("usp_RTranValidateUPPCL", queryParameters, commandType: System.Data.CommandType.StoredProcedure);
                        string printMessage = "";
                        if (resp != null)
                        {
                            if (!string.IsNullOrEmpty(resp.Id))
                            {
                                Id = resp.Id;
                                // set the workstatus to 1 and billpost date to current date time.
                                var updateTime = UpdateBeforeUPPCLPush();
                                if (updateTime.Contains("Success"))
                                {
                                    BillPaymentRequest billPaymentRequest = new BillPaymentRequest();
                                    billPaymentRequest.agentId = UPPCLManager.uppclConfig.AgentID;
                                    billPaymentRequest.agencyType = "OTHER";
                                    billPaymentRequest.amount = ((int)Amount).ToString();
                                    billPaymentRequest.billAmount = UPPCL_BillAmount.ToString();
                                    billPaymentRequest.billId = UPPCL_BillId;
                                    billPaymentRequest.connectionType = UPPCL_ConnectionType;
                                    billPaymentRequest.consumerAccountId = RechargeMobileNumber;
                                    billPaymentRequest.consumerName = eSuvidhaBillFetchResponse.BillFetchResponse
                                        .Body.PaymentDetailsResponse.ConsumerName.Replace(@"\", @"\\")
                                        .Replace(@"/", @"\/").Replace(".", "").Replace("-", "");
                                    billPaymentRequest.discom = UPPCL_Discom;
                                    billPaymentRequest.division = eSuvidhaBillFetchResponse.BillFetchResponse.Body
                                        .PaymentDetailsResponse.Division;
                                    billPaymentRequest.divisionCode = eSuvidhaBillFetchResponse.BillFetchResponse
                                        .Body.PaymentDetailsResponse.DivCode;
                                    billPaymentRequest.mobile = string.IsNullOrEmpty(Parameter3) ? "" : Parameter3;
                                    billPaymentRequest.outstandingAmount = accountInfoInt.ToString();
                                    billPaymentRequest.paymentType = paymentTypeFullPartial;
                                    billPaymentRequest.referenceTransactionId = Id;
                                    billPaymentRequest.sourceType = eSuvidhaBillFetchResponse.BillFetchResponse.Body
                                        .PaymentDetailsResponse.ProjectArea;
                                    billPaymentRequest.type = eSuvidhaBillFetchResponse.BillFetchResponse.Body
                                        .PaymentDetailsResponse.ProjectArea;
                                    billPaymentRequest.vanNo = UPPCLManager.uppclConfig.AgentVANNo;
                                    billPaymentRequest.walletId = RetailUserId;
                                    billPaymentRequest.TDStatus = eSuvidhaBillFetchResponse.BillFetchResponse
                                        .Body.PaymentDetailsResponse.TDStatus;
                                    billPaymentRequest.LifelineAct = eSuvidhaBillFetchResponse.BillFetchResponse
                                        .Body.PaymentDetailsResponse.LifelineAct;
                                    string city = "NA";
                                    try
                                    {
                                        if (eSuvidhaBillFetchResponse.BillFetchResponse.Body.PaymentDetailsResponse
                                                .PremiseAddress.City != null)
                                            city = eSuvidhaBillFetchResponse.BillFetchResponse.Body
                                                .PaymentDetailsResponse
                                                .PremiseAddress.City;
                                    }
                                    catch (Exception)
                                    {
                                        city = "NA";
                                    }

                                    if (city.Length < 1)
                                    {
                                        city = "NA";
                                    }
                                    
                                    billPaymentRequest.city = city;
                                    try
                                    {
                                        billPaymentRequest.param1 = eSuvidhaBillFetchResponse.BillFetchResponse.Body.PaymentDetailsResponse.Param1.ToString() ?? string.Empty;
                                    }
                                    catch (Exception)
                                    {
                                        
                                    }

                                    string successFailMessage = "";

                                    try
                                    {
                                        BillPostResponse billPostResponse = UPPCLManager.BillPostBillPayment(billPaymentRequest);
                                        
                                        OtherApiStatusCode = billPostResponse.status;
                                        CallbackData = JsonConvert.SerializeObject(billPostResponse);
                                        ConfirmDate = DateTime.Now;

                                        if (billPostResponse.status == "SUCCESS")
                                        {
                                            RechargeStatus = "SUCCESS";
                                            OtherApiId = billPostResponse.externalTransactionId;
                                            LiveId = billPostResponse.externalTransactionId;
                                            WorkStatus = 2;
                                            TranType = 3;

                                            UpdateAfterCallbackProcessUPPCL();
                                        }
                                        else if (billPostResponse.status == "FAILED")
                                        {
                                            if (billPostResponse.message.Contains("enough funds in wallet"))
                                            {
                                                printMessage = "Errors: Can not pay bill. Insufficient balance.";
                                            }

                                            if (billPostResponse.message.Contains("payment was already done"))
                                            {
                                                printMessage = "Errors: Can not pay bill. Payment was already done.";
                                            }

                                            if (billPostResponse.message.Contains("Id and van number do not"))
                                            {
                                                printMessage = "Errors: Can not pay bill. Agent Id and van number do not match.";
                                            }

                                            RechargeStatus = "FAILURE";
                                            OtherApiId = "FAILURE";
                                            LiveId = "";
                                            WorkStatus = 2;

                                            TranType = 10;

                                            UpdateAfterCallbackProcessUPPCL();

                                            Remarks = printMessage;
                                        }

                                        if (!string.IsNullOrEmpty(billPostResponse.message))
                                        {
                                            if (billPostResponse.message.Contains("Internal: Exception: Value cannot be null.") || billPostResponse.message.Contains("Exception: Server:") || billPostResponse.message.Contains("The requested API is temporarily blocked"))
                                            {
                                                Remarks = "Errors: Can not send bill. Please check your recharge report after 1 minute for final status.";

                                                try
                                                {
                                                    Thread.Sleep(15000); // Delay 15 seconds before calling status check
                                                    Id = resp.Id;
                                                    RTran tempTran = LoadRecord();
                                                    UPPCL_BillId = tempTran.UPPCL_BillId;
                                                    CreateDate = tempTran.CreateDate;
                                                    Amount = tempTran.Amount;

                                                    StatusCheckResponse statusCheckResponse = StatusCheck(true);

                                                    Remarks = (statusCheckResponse.status == "SUCCESS" ? "Success: " : "Errors: ") + statusCheckResponse.message;
                                                    
                                                    //Exception: Server:
                                                    if (statusCheckResponse.message.Contains("Status check api down") || statusCheckResponse.message.Contains("Exception: ")  || statusCheckResponse.message.Contains("No Record found") || statusCheckResponse.message.Contains("The requested API is temporarily blocked"))
                                                    {
                                                        try
                                                        {
                                                            ForceFailResponse forceFailResponse = ForceFail(true);
                                                            //Remarks = "Errors: " + forceFailResponse.message;
                                                            Remarks = "Errors: Payment failed.";
                                                            printMessage = Remarks;
                                                        }
                                                        catch (Exception exFF)
                                                        {
                                                            Remarks = "Errors: Can not send bill. Please check your recharge report after 1 minute for final status. Force fail failed.";
                                                            printMessage = Remarks;
                                                        }
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    Remarks = "Errors: Can not send bill. Please check your recharge report after 1 minute for final status. Status check failed.";
                                                    printMessage = Remarks;
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        printMessage = "Errors: Can not send bill. Please check your recharge report in 1 minute for final status." + ex.Message;
                                        Remarks += printMessage;
                                    }
                                }
                                else
                                {
                                    printMessage = "Errors: Can not save record for bill post.";
                                }

                                //after post , update the details with poost data

                                //printMessage = " <a target='_blank' href='Home/PrintReceipt?t=" + StaticData.ConvertStringToHex(resp.Id) + "'>PRINT RECEIPT</a>";

                                printMessage = " <span class='btn btn-primary' onclick=\'PrintReceipt(\"" +
                                               StaticData.ConvertStringToHex(resp.Id) + "\");\'>PRINT RECEIPT</span>";
                            }
                            else
                            {
                                Remarks = "Errors: " + resp.OperationMessage;
                                result = Remarks;
                            }
                        }

                        if (StaticData.loginSource == "mobile")
                        {
                            printMessage = "=" + resp.Id;
                        }

                        if (RechargeStatus == "FAILURE")
                        {
                            result = Remarks;
                        }
                        else if (RechargeStatus == "PROCESS")
                        {
                            result = Remarks;
                        }
                        else
                        {
                            result = "Success: Bill payment / recharge status of " + TelecomOperatorName +
                                     " of account " + RechargeMobileNumber + " of Amount " + Amount?.ToString("N2") +
                                     ". " + resp.OperationMessage + " Balance: " + resp.ClosingBalance?.ToString("N2") +
                                     printMessage;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result += "Errors: Exception: RTPB: " + ex.Message;
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

        public StatusCheckResponse StatusCheck(bool updateTransaction = false)
        {
            StatusCheckResponse statusCheckResponse = new StatusCheckResponse();
            try
            {
                statusCheckResponse = UPPCLManager.StatusCheck(UPPCL_BillId, RechargeMobileNumber, UPPCLManager.uppclConfig.AgentVANNo, Id);
                if (updateTransaction)
                {
                    if (statusCheckResponse.status == "FAILED" || statusCheckResponse.status == "Forcefully_Failed") //   statusCheckResponse.message.Contains("No Record found.")
                    {
                        RechargeStatus = "FAILURE";
                        OtherApiId = "FAILURE";
                        LiveId = "";
                        WorkStatus = 2;

                        TranType = 10;

                        UpdateAfterCallbackProcessUPPCL();

                        statusCheckResponse.message = "Success: Successfully updated to failed.";
                    }
                    

                    if (statusCheckResponse.status == "SUCCESS" && Amount == Convert.ToDecimal(statusCheckResponse.billAmount))
                    {
                        RechargeStatus = "SUCCESS";
                        OtherApiId = statusCheckResponse.transactionId;
                        LiveId = statusCheckResponse.transactionId;
                        WorkStatus = 2;
                        TranType = 3;

                        UpdateAfterCallbackProcessUPPCL();

                        statusCheckResponse.message = "Success: Successfully updated to success.";
                    }

                    if (statusCheckResponse.message.Contains("Exception: Unexpected character encountered"))
                    {
                        statusCheckResponse.message += "Errors: Exception: Status check api down.";
                    }
                    
                    if (statusCheckResponse.message.Contains("Exception: Error:"))
                    {
                        statusCheckResponse.message += "Errors: Exception: Error in calling status check api.";
                    }

                    //ManualUpdate(JsonConvert.SerializeObject(statusCheckResponse));
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
                        if (forceFailResponse.code == 200 || forceFailResponse.code == 409)
                        {
                            RechargeStatus = "FAILURE";
                            OtherApiId = "FAILURE";
                            LiveId = "";
                            WorkStatus = 2;

                            TranType = 10;

                            UpdateAfterCallbackProcessUPPCL();

                            forceFailResponse.message += "Success: Successfully updated to failed.";

                            using (var con = new SqlConnection(StaticData.conString))
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
                        else
                        {
                            forceFailResponse.message = "Errors: " + forceFailResponse.message;
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