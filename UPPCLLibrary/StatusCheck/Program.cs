using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace StatusCheck
{
    internal class Program
    {
        static void Main(string[] args)
        {
        }

        //public static StatusCheckResponse StatusCheck(bool updateTransaction = false)
        //{
        //    StatusCheckResponse statusCheckResponse = new StatusCheckResponse();
        //    try
        //    {
        //        statusCheckResponse = UPPCLManager.StatusCheck(UPPCL_BillId, RechargeMobileNumber, UPPCLManager.uppclConfig.AgentVANNo);
        //        if (updateTransaction)
        //        {
        //            if (statusCheckResponse.status == "FAILED") //   statusCheckResponse.message.Contains("No Record found.")
        //            {
        //                RechargeStatus = "FAILURE";
        //                OtherApiId = "FAILURE";
        //                LiveId = "";
        //                WorkStatus = 2;

        //                TranType = 10;

        //                UpdateAfterCallbackProcessUPPCL();

        //                statusCheckResponse.message = "Success: Successfully updated to failed.";
        //            }


        //            if (statusCheckResponse.status == "SUCCESS" && Amount == Convert.ToDecimal(statusCheckResponse.billAmount))
        //            {
        //                RechargeStatus = "SUCCESS";
        //                OtherApiId = statusCheckResponse.transactionId;
        //                LiveId = statusCheckResponse.transactionId;
        //                WorkStatus = 2;
        //                TranType = 3;

        //                UpdateAfterCallbackProcessUPPCL();

        //                statusCheckResponse.message = "Success: Successfully updated to success.";
        //            }

        //            if (statusCheckResponse.message.Contains("Exception: Unexpected character encountered"))
        //            {
        //                statusCheckResponse.message += "Errors: Exception: Status check api down.";
        //            }

        //            if (statusCheckResponse.message.Contains("Exception: Error:"))
        //            {
        //                statusCheckResponse.message += "Errors: Exception: Error in calling status check api.";
        //            }

        //            //ManualUpdate(JsonConvert.SerializeObject(statusCheckResponse));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        statusCheckResponse.message += "Errors: Exception: " + ex.Message;
        //    }

        //    return statusCheckResponse;
        //}

        //public static ForceFailResponse ForceFail(bool updateTransaction = false)
        //{
        //    ForceFailResponse forceFailResponse = new ForceFailResponse();
        //    try
        //    {
        //        ForceFailRequest forceFailRequest = new ForceFailRequest();
        //        forceFailRequest.transactionDate = UPPCLManager.CurrentDate((DateTime)CreateDate);
        //        forceFailRequest.vanNo = UPPCLManager.uppclConfig.AgentVANNo;
        //        forceFailRequest.amount = Amount.ToString();
        //        forceFailRequest.consumerId = RechargeMobileNumber;
        //        forceFailRequest.billId = UPPCL_BillId;
        //        forceFailRequest.referenceTransactionId = Id;


        //        forceFailResponse = UPPCLManager.ForceFail(forceFailRequest);

        //        if (updateTransaction)
        //        {
        //            if (!string.IsNullOrEmpty(forceFailResponse.code.ToString()))
        //            {
        //                if (forceFailResponse.code == 200 || forceFailResponse.code == 409)
        //                {
        //                    RechargeStatus = "FAILURE";
        //                    OtherApiId = "FAILURE";
        //                    LiveId = "";
        //                    WorkStatus = 2;

        //                    TranType = 10;

        //                    UpdateAfterCallbackProcessUPPCL();
        //                    forceFailResponse.message += "Success: Successfully updated to failed.";

        //                    using (var con = new SqlConnection(StaticData.conString))
        //                    {
        //                        try
        //                        {
        //                            var queryParameters = new DynamicParameters();
        //                            queryParameters.Add("@Id", Id);
        //                            string result = con.Query<string>("usp_ForceFailChecked", queryParameters, commandType: System.Data.CommandType.StoredProcedure).SingleOrDefault();
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            string error = ex.Message;
        //                        }
        //                        finally
        //                        {
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    forceFailResponse.message = "Errors: " + forceFailResponse.message;
        //                }
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        forceFailResponse.message += "Errors: Exception: " + ex.Message;
        //    }

        //    return forceFailResponse;
        //}
    }
}
