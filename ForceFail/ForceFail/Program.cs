// See https://aka.ms/new-console-template for more information


using Dapper;
using ForceFail;
using Newtonsoft.Json;
using Razorpay.Api;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UPPCLLibrary;
using UPPCLLibrary.BillFail;
using UPPCLLibrary.BillPost;

public class Program
{
    const string constring = "Data Source=103.163.200.59;Initial Catalog=ESuvidha;User ID=esdbmain;Password=dgr@423f6ndyWyb;Encrypt=False;";
    static string RechargeStatus;
    static string OtherApiId;
    static string LiveId;
    static short? WorkStatus;
    static int TranType;
    static string UPPCL_BillId;
    static string RechargeMobileNumber;
    static string AgentVANNo;
    static decimal? Amount;
    static string Id;
    static string RetailUserId;
    static string CallbackData;
    static DateTime? CallbackDataTime;
    static string OtherApiStatusCode;
    static string OriginalLiveId;
    static string UPPCL_PaymentType;
    static DateTime? CreateDate;
    static decimal? UPPCL_DDR;

    public static void Main()
    {
        try
        {
            CheckAndUpdateRazorpayStatus();
        }
        catch (Exception ex) { }
        try
        {
            CheckAndCreditAmountIfRazorResponseWasSuccess();
        }
        catch (Exception ex) { }
        try
        {
            CheckAndUpdatePendingBillStatus();
        }
        catch (Exception ex) { }
    }

    private static void CheckAndCreditAmountIfRazorResponseWasSuccess()
    {
        var key = "rzp_live_gMiAscmFhv8wrv";      // Your API key
        var secret = "jtX8wTfpArPLse0PerpTkthO";
        RazorpayClient client = new RazorpayClient(key, secret);
        var date = DateTime.Now.ToString("yyyyMMdd");
        var date1 = DateTime.Now.AddMinutes(-30).ToString("yyyy-MM-dd HH:mm:ss");
        var con = new SqlConnection(constring);
        var query = $"Select RPO.Id, RPO.RetailerId, RU.OrderNo from RazorPayOrder RPO inner join RetailUser RU on RU.Id = RPO.RetailerId and RPO.OrderStatus = 'captured' and CreateDate > '{date}' and CreateDate < '{date1}' and CreditTranId is null";
        var result = con.Query<RazorpayOrderLite>(query, commandType: System.Data.CommandType.Text);
        foreach (var item in result)
        {
            Dictionary<string, object> options = new Dictionary<string, object>();
            options.Add("receipt", item.Id);  // The receipt ID you stored when creating the order

            var orders = client.Order.All(options);

            if (orders.Count > 0)
            {
                Order order = orders[0];
                var payment = order.Payments().FirstOrDefault(_ => _.Attributes.status == "captured");
                if (payment == null)
                {
                    payment = order.Payments().LastOrDefault();
                }
                var regeneratedSignature = string.Empty;
                if (payment != null && payment.Attributes?.error == null)
                {
                    if (payment.Attributes?.id != null)
                    {
                        long rAmount = Convert.ToInt64(payment.Attributes.amount.ToString());
                        long rFee = Convert.ToInt64(payment.Attributes.fee.ToString());
                        long rTax = Convert.ToInt64(payment.Attributes.tax.ToString());
                        long oFee = 0;
                        string r_status = payment.Attributes.status.ToString();
                        string r_method = payment.Attributes.method.ToString();
                        string r_error = payment.Attributes.error_code.ToString();
                        var paymentType = payment.Attributes.upi?.payer_account_type?.ToString();

                        RazorpayOrder razorpayOrder = RazorpayOrderLoadByRazorpayId(order.Attributes.id.ToString());
                        RTran fundTransferRTran = new RTran();
                        try
                        {
                            string tranType = "cr";

                            fundTransferRTran.RequestIp = "Auto-Check";
                            fundTransferRTran.RequestMachine = "Auto-Check";
                            fundTransferRTran.RetailUserOrderNo = item.OrderNo; //

                            /*
                                     if (r_method == "upi" && paymentType != "credit_card")// || r_method == "netbanking"
                                     {
                                         fundTransferRTran.Amount = Convert.ToDecimal((decimal)rAmount / 100);
                                     }
                                     else
                                     {
                                         fundTransferRTran.Amount = Convert.ToDecimal(((decimal)rAmount / 100) - ((decimal)rFee / 100) - ((decimal)oFee/100));
                                     }
                                     */

                            //fee deduction for all type of transactions.
                            fundTransferRTran.Amount = Convert.ToDecimal(((decimal)rAmount / 100) - ((decimal)rFee / 100) - ((decimal)oFee / 100));

                            fundTransferRTran.Extra1 = "razor";
                            fundTransferRTran.Extra2 = razorpayOrder.razorpay_order_id;

                            if (tranType == "cr")
                            {
                                fundTransferRTran.CreditAmount = fundTransferRTran.Amount;
                                fundTransferRTran.TranType = 11;
                            }

                            if (tranType == "dr")
                            {
                                fundTransferRTran.DebitAmount = fundTransferRTran.Amount;
                                fundTransferRTran.TranType = 12;
                            }

                            fundTransferRTran.Remarks =
                                "Wallet topup via Razorpay order-" + order.Attributes.id.ToString() + ", payment id-" + payment.Attributes?.id.ToString() + ", method-" + r_method;
                            //fundTransferRTran.Remarks = "Wallet topup of Rs. " + razorpayOrder.Amount.ToString() + " , fees-" + (rFee/100).ToString() + " via Razorpay order-" + o + ", payment id-" + p;
                            fundTransferRTran.RequestMessage = "WEBPORTAL";

                            if (fundTransferRTran.Amount > 0)
                            {
                                TransferFundByData("admin", fundTransferRTran);
                            }

                        }
                        catch (Exception ex)
                        {
                            //result = "Errors: Exception: " + ex.Message;
                        }
                        finally
                        {
                            fundTransferRTran = null;
                        }
                    }
                }
            }
        }
    }

    private static void CheckAndUpdatePendingBillStatus()
    {
        var date = DateTime.Now.ToString("yyyyMMdd");
        var date1 = DateTime.Now.AddMinutes(-30).ToString("yyyy-MM-dd HH:mm:ss");
        UPPCLManager.DbConnection = constring;
        UPPCLManager.Initialize();
        UPPCLManager.CheckTokenExpiry();
        AgentVANNo = "UPCA1419609895";
        var con = new SqlConnection(constring);
        var queryParameters = new DynamicParameters();
        var query = $"select UPPCL_BillId, RechargeStatus, OtherApiId, LiveId, WorkStatus, TranType, UPPCL_BillId, RechargeMobileNumber, Amount, Id, RetailUserId, CallbackData, CallbackDataTime, OtherApiStatusCode, OriginalLiveId, UPPCL_PaymentType, CreateDate, UPPCL_DDR, RetailUserOrderNo, isOTs, UPPCL_Discom, isfull from RTran  with (nolock) where RechargeStatus = 'Process' and Createdate> '{date}' and CreateDate < '{date1}' and UPPCL_PaymentType in ('Full', 'Partial')";
        var result = con.Query<FaildRTran>(query, queryParameters, commandType: System.Data.CommandType.Text);
        foreach (var item in result)
        {

            RechargeStatus = item.RechargeStatus;
            OtherApiId = item.OtherApiId;
            LiveId = item.LiveId;
            TranType = item.TranType;
            UPPCL_BillId = item.UPPCL_BillId;
            RechargeMobileNumber = item.RechargeMobileNumber;
            Amount = item.Amount;
            Id = item.Id;
            RetailUserId = item.RetailUserId;
            CallbackData = item.CallbackData;
            CallbackDataTime = item.CallbackDataTime;
            OtherApiStatusCode = item.OtherApiStatusCode;
            OriginalLiveId = item.OriginalLiveId;
            UPPCL_PaymentType = item.UPPCL_PaymentType;
            CreateDate = item.CreateDate;
            UPPCL_DDR = item.UPPCL_DDR;
            if (string.IsNullOrEmpty(item.UPPCL_BillId))
            {
                RechargeStatus = "FAILURE";
                OtherApiId = "FAILURE";
                LiveId = "";
                WorkStatus = 2;

                TranType = 10;

                UpdateAfterCallbackProcessUPPCL();
                continue;
            }
            StatusCheckResponse statusCheckResponse = StatusCheck(true);
            if (statusCheckResponse.message.Contains("Status check api down") || statusCheckResponse.message.Contains("Exception: ") || statusCheckResponse.message.Contains("No Record found") || statusCheckResponse.message.Contains("The requested API is temporarily blocked"))
            {
                try
                {
                    ForceFailResponse forceFailResponse = ForceFail(true);
                }
                catch (Exception exFF)
                {
                }
            }
        }
    }

    private static void CheckAndUpdateRazorpayStatus()
    {
        var key = "rzp_live_gMiAscmFhv8wrv";      // Your API key
        var secret = "jtX8wTfpArPLse0PerpTkthO";
        var date = DateTime.Now.ToString("yyyyMMdd");
        var date1 = DateTime.Now.AddMinutes(-30).ToString("yyyy-MM-dd HH:mm:ss");// Your API secret
        RazorpayClient client = new RazorpayClient(key, secret);
        var con = new SqlConnection(constring);
        var queryParameters = new DynamicParameters();
        var query = $"Select RPO.Id, RPO.RetailerId, RU.OrderNo from RazorPayOrder RPO inner join RetailUser RU on RU.Id = RPO.RetailerId  where CreateDate > '{date}' and CreateDate < '{date1}' and OrderStatus is null";
        var result = con.Query<RazorpayOrderLite>(query, queryParameters, commandType: System.Data.CommandType.Text);

        foreach (var item in result)
        {
            // 2. Fetch all orders with the given receipt (you may need to filter manually)
            Dictionary<string, object> options = new Dictionary<string, object>();
            options.Add("receipt", item.Id);  // The receipt ID you stored when creating the order

            var orders = client.Order.All(options);

            if (orders.Count > 0)
            {
                foreach (Order order in orders)
                {
                    var payment = order.Payments().FirstOrDefault(_ => _.Attributes.status == "captured");
                    if(payment == null)
                    {
                        payment = order.Payments().LastOrDefault();
                    }
                    var regeneratedSignature = string.Empty;
                    if (payment != null && payment.Attributes?.error == null)
                    {
                        if (payment.Attributes?.id != null)
                        {
                            long rAmount = Convert.ToInt64(payment.Attributes.amount.ToString());
                            long rFee = Convert.ToInt64(payment.Attributes.fee.ToString());
                            long rTax = Convert.ToInt64(payment.Attributes.tax.ToString());
                            long oFee = 0;
                            string r_status = payment.Attributes.status.ToString();
                            string r_method = payment.Attributes.method.ToString();
                            string r_error = payment.Attributes.error_code.ToString();
                            var paymentType = payment.Attributes.upi?.payer_account_type?.ToString();

                            RazorpayOrder razorpayOrder = RazorpayOrderLoadByRazorpayId(order.Attributes.id.ToString());
                            string payload = $"{order.Attributes.id.ToString()}|{payment.Attributes?.id}";

                            //Step 2: Compute HMAC SHA256
                            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
                            {
                                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                                regeneratedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();
                            }
                            ////string retailerId, string OrderId, string CustomerMobile, string RazorpayOrderId, string logType, string logData, DateTime CreateDate
                            RazorpayLogSave(razorpayOrder.RetailerId, "", "", order.Attributes.id.ToString(), "PostPayment", "o=" + order.Attributes.id.ToString() + ">>p=" + payment.Attributes?.id.ToString() + ">>s=" + regeneratedSignature, DateTime.Now);
                            RazorpayLogSave(razorpayOrder.RetailerId, razorpayOrder.Id, razorpayOrder.CustomerMobile, order.Attributes.id.ToString(), "PaymentCheck", JsonConvert.SerializeObject(payment), DateTime.Now);

                            RazorpayOrderUpdateFees(razorpayOrder.razorpay_order_id, rFee.ToString(), rTax.ToString(), oFee.ToString(), r_status);

                            if (r_status == "captured" && razorpayOrder.RazorpayAmount == rAmount)
                            {
                                RecordSaveResponse recordSaveResponse = RazorpayOrderUpdateOPS(order.Attributes.id.ToString(), payment.Attributes?.id.ToString(), regeneratedSignature);
                                if (recordSaveResponse.OperationMessage.Contains("Success"))
                                {
                                    RTran fundTransferRTran = new RTran();
                                    try
                                    {
                                        string tranType = "cr";

                                        fundTransferRTran.RequestIp = "Auto-Check";
                                        fundTransferRTran.RequestMachine = "Auto-Check";
                                        fundTransferRTran.RetailUserOrderNo = item.OrderNo; //


                                        /*
                                                 if (r_method == "upi" && paymentType != "credit_card")// || r_method == "netbanking"
                                                 {
                                                     fundTransferRTran.Amount = Convert.ToDecimal((decimal)rAmount / 100);
                                                 }
                                                 else
                                                 {
                                                     fundTransferRTran.Amount = Convert.ToDecimal(((decimal)rAmount / 100) - ((decimal)rFee / 100) - ((decimal)oFee/100));
                                                 }
                                                 */

                                        //fee deduction for all type of transactions.
                                        fundTransferRTran.Amount = Convert.ToDecimal(((decimal)rAmount / 100) - ((decimal)rFee / 100) - ((decimal)oFee / 100));

                                        fundTransferRTran.Extra1 = "razor";
                                        fundTransferRTran.Extra2 = razorpayOrder.razorpay_order_id;

                                        if (tranType == "cr")
                                        {
                                            fundTransferRTran.CreditAmount = fundTransferRTran.Amount;
                                            fundTransferRTran.TranType = 11;
                                        }

                                        if (tranType == "dr")
                                        {
                                            fundTransferRTran.DebitAmount = fundTransferRTran.Amount;
                                            fundTransferRTran.TranType = 12;
                                        }

                                        fundTransferRTran.Remarks =
                                            "Wallet topup via Razorpay order-" + order.Attributes.id.ToString() + ", payment id-" + payment.Attributes?.id.ToString() + ", method-" + r_method;
                                        //fundTransferRTran.Remarks = "Wallet topup of Rs. " + razorpayOrder.Amount.ToString() + " , fees-" + (rFee/100).ToString() + " via Razorpay order-" + o + ", payment id-" + p;
                                        fundTransferRTran.RequestMessage = "WEBPORTAL";

                                        if (fundTransferRTran.Amount > 0)
                                        {
                                            TransferFundByData("admin", fundTransferRTran);
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        //result = "Errors: Exception: " + ex.Message;
                                    }
                                    finally
                                    {
                                        fundTransferRTran = null;
                                    }
                                }
                            }
                        }
                        else
                        {
                            RazorpayOrderUpdateStausToFailed(item.Id, "failed");
                        }
                    }
                    else
                    {
                        RazorpayOrderUpdateStausToFailed(item.Id, "failed");
                    }
                }
            }
            else
            {
                RazorpayOrderUpdateStausToFailed(item.Id, "failed");
            }
        }
    }

    public static string TransferFundByData(string userRole, RTran rTran)
    {
        string result = "Info: Starting transfer process";
        try
        {
            if (userRole == "admin")
            {
                using (var con = new SqlConnection(constring))
                {
                    try
                    {
                        var queryParameters = new DynamicParameters();
                        queryParameters.Add("@RetailUserOrderNo", rTran.RetailUserOrderNo);
                        queryParameters.Add("@Amount", rTran.Amount);
                        queryParameters.Add("@Deduction", 0);
                        queryParameters.Add("@FinalAmount", null);
                        queryParameters.Add("@OpeningBalance", null);
                        queryParameters.Add("@DebitAmount", rTran.DebitAmount);
                        queryParameters.Add("@CreditAmount", rTran.CreditAmount);
                        queryParameters.Add("@ClosingBalance", null);
                        queryParameters.Add("@Margin", null);
                        queryParameters.Add("@RequestIp", rTran.RequestIp);
                        queryParameters.Add("@RequestMachine", rTran.RequestMachine);
                        queryParameters.Add("@RequestGeoCode", rTran.RequestGeoCode);
                        queryParameters.Add("@RequestNumber", rTran.RequestNumber);
                        queryParameters.Add("@RequestMessage", rTran.RequestMessage);
                        queryParameters.Add("@RequestTime", DateTime.UtcNow.AddHours(5.5));
                        queryParameters.Add("@TranType", rTran.TranType);
                        queryParameters.Add("@Remarks", rTran.Remarks);
                        queryParameters.Add("@Extra1", rTran.Extra1);
                        queryParameters.Add("@Extra2", rTran.Extra2);
                        queryParameters.Add("@CreateDate", null);
                        queryParameters.Add("@ConfirmDate", null);


                        RTranApiFundTransfer au = con.QuerySingle<RTranApiFundTransfer>("usp_RetailClientFundTransfer", queryParameters, commandType: System.Data.CommandType.StoredProcedure);

                        result = au == null ? "Errors: Can not transfer fund. Error in transferring fund." : au.OperationMessage;
                    }
                    catch (Exception ex)
                    {
                        result = "Errors: Ex148 " + ex.Message;
                    }
                }

                return result;
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

    public static RecordSaveResponse RazorpayOrderUpdateOPS(string o, string p, string s)
    {
        RecordSaveResponse result = new RecordSaveResponse();
        try
        {

            using (var con = new SqlConnection(constring))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@razorpay_order_id", o);
                parameters.Add("@razorpay_payment_id", p);
                parameters.Add("@razorpay_signature", s);
                result = con.QuerySingleOrDefault<RecordSaveResponse>("usp_RazorPayOrderUpdateOPS", parameters, commandType: System.Data.CommandType.StoredProcedure);
            }
        }
        catch (Exception ex)
        {
            result.OperationMessage = "Errors: ExCodeNet " + ex.Message;
        }

        return result;
    }

    public static void RazorpayOrderUpdateStausToFailed(string id, string pstatus)
    {
        try
        {
            using (var con = new SqlConnection(constring))
            {
                con.Execute($"Update RazorPayOrder set OrderStatus = '{pstatus}' where Id = '{id}' ", commandType: System.Data.CommandType.Text);
            }
        }
        catch (Exception ex)
        {
        }
    }

    public static RecordSaveResponse RazorpayOrderUpdateFees(string orderId, string rfees, string rgst, string ofees, string pstatus = "")
    {
        RecordSaveResponse result = new RecordSaveResponse();
        try
        {

            using (var con = new SqlConnection(constring))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@razorpay_order_id", orderId);
                parameters.Add("@RazorpayFees", rfees);
                parameters.Add("@RazorpayGST", rgst);
                parameters.Add("@OurFees", ofees);
                parameters.Add("@OrderStatus", pstatus);
                result = con.QuerySingleOrDefault<RecordSaveResponse>("usp_RazorPayOrderUpdateFees", parameters, commandType: System.Data.CommandType.StoredProcedure);
            }
        }
        catch (Exception ex)
        {
            result.OperationMessage = "Errors: ExCodeNet " + ex.Message;
        }

        return result;
    }

    public static RazorpayOrder RazorpayOrderLoadByRazorpayId(string id)
    {
        RazorpayOrder result = new RazorpayOrder();
        try
        {

            using (var con = new SqlConnection(constring))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@razorpay_order_id", id);
                result = con.QuerySingleOrDefault<RazorpayOrder>("usp_RazorPayOrderByRazorpayIdSelect", parameters, commandType: System.Data.CommandType.StoredProcedure);
            }
        }
        catch (Exception ex)
        {
            result.OperationMessage = "Errors: ExCodeNet " + ex.Message;
        }

        return result;
    }

    public static string RazorpayLogSave(string retailerId, string OrderId, string CustomerMobile, string RazorpayOrderId, string logType, string logData, DateTime CreateDate)
    {
        string result = string.Empty;
        try
        {
            using (var con = new SqlConnection(constring))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@OrderId", OrderId);
                parameters.Add("@RetailerId", retailerId);
                parameters.Add("@CustomerMobile", CustomerMobile);
                parameters.Add("@RazorpayOrderId", RazorpayOrderId);
                parameters.Add("@LogType", logType);
                parameters.Add("@LogData", logData);
                parameters.Add("@CreateDate", CreateDate);
                result = con.QuerySingleOrDefault<string>("usp_RazorpayLogDataInsert", parameters, commandType: System.Data.CommandType.StoredProcedure);
            }
        }
        catch (Exception ex)
        {
            result = "Errors: ExCodeNet " + ex.Message;
        }

        return result;
    }

    public static StatusCheckResponse StatusCheck(bool updateTransaction = false)
    {
        StatusCheckResponse statusCheckResponse = new StatusCheckResponse();
        try
        {
            statusCheckResponse = UPPCLManager.StatusCheck(UPPCL_BillId, RechargeMobileNumber, AgentVANNo);
            if (updateTransaction)
            {
                if (statusCheckResponse.status == "FAILED") //   statusCheckResponse.message.Contains("No Record found.")
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
                    if (CheckDuplicacy() > 0)
                    {
                        RechargeStatus = "FAILURE";
                        OtherApiId = "FAILURE";
                        LiveId = "";
                        WorkStatus = 2;

                        TranType = 10;

                        UpdateAfterCallbackProcessUPPCL();

                        statusCheckResponse.message = "Success: Successfully updated to failed.";
                    }
                    else
                    {
                        RechargeStatus = "SUCCESS";
                        OtherApiId = statusCheckResponse.transactionId;
                        LiveId = statusCheckResponse.transactionId;
                        WorkStatus = 2;
                        TranType = 3;

                        UpdateAfterCallbackProcessUPPCL();
                        statusCheckResponse.message = "Success: Successfully updated to success.";
                    }
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

    public static void UpdateAfterCallbackProcessUPPCL()
    {
        using (var con = new SqlConnection(constring))
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
                con.Execute("usp_RTranUpdateAfterCallbackUPPCL", queryParameters,
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


    public static int CheckDuplicacy()
    {
        using (var con = new SqlConnection(constring))
        {
            try
            {
                return con.ExecuteScalar<int>($"Select Count(1) from RTran where RechargeMobileNumber = '{RechargeMobileNumber}' and CreateDate between '{CreateDate.GetValueOrDefault().ToString("yyyyMMdd")}' and '{CreateDate.GetValueOrDefault().AddDays(1).ToString("yyyyMMdd")}' and RechargeStatus = 'Success'",
                    commandType: System.Data.CommandType.Text);
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

    public static ForceFailResponse ForceFail(bool updateTransaction = false)
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

                        using (var con = new SqlConnection(constring))
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
}
