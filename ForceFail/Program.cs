// See https://aka.ms/new-console-template for more information



using Dapper;
using ForceFail;
using System.Data.Common;
using System.Data.SqlClient;
using UPPCLLibrary;
using UPPCLLibrary.BillFail;
using UPPCLLibrary.BillFetch;
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
        var date = DateTime.Now.ToString("yyyyMMdd");
        UPPCLManager.DbConnection = constring;
        UPPCLManager.Initialize();
        UPPCLManager.CheckTokenExpiry();
        AgentVANNo = "UPCA1419609895";
        using var con = new SqlConnection(constring);
        var queryParameters = new DynamicParameters();
        var query = $"select UPPCL_BillId, RechargeStatus, OtherApiId, LiveId, WorkStatus, TranType, UPPCL_BillId, RechargeMobileNumber, Amount, Id, RetailUserId, CallbackData, CallbackDataTime, OtherApiStatusCode, OriginalLiveId, UPPCL_PaymentType, CreateDate, UPPCL_DDR, RetailUserOrderNo, isOTs, UPPCL_Discom, isfull from RTran  with (nolock) where RechargeStatus = 'Process' and Createdate> '{date}' and UPPCL_PaymentType in ('Full', 'Partial')";
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
                    if(CheckDuplicacy() > 0)
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
