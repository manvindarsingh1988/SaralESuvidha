using Dapper;
using Microsoft.Extensions.Configuration;
using Quartz;
using SaralESuvidha.Models;
using SaralESuvidha.Services;
using SaralESuvidha.ViewModel;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SaralESuvidha.QuartzJobs
{
    public class SabpaisaStatusCheckJob : IJob
    {
        string constring = string.Empty;
        private readonly SabPaisaService _sabPaisaService;
        public SabpaisaStatusCheckJob(IConfiguration configuration, SabPaisaService sabPaisaService)
        {
            constring = configuration.GetConnectionString("DefaultConnection");
            _sabPaisaService = sabPaisaService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                CheckAndUpdateSabpaisaStatus();
                await Task.CompletedTask;
            }
            catch (Exception ex) { }
        }

        private void CheckAndUpdateSabpaisaStatus()
        {
            var date = DateTime.Now.ToString("yyyyMMdd");
            var date1 = DateTime.Now.AddMinutes(-30).ToString("yyyy-MM-dd HH:mm:ss");
            var con = new SqlConnection(constring);
            var query = $"Select RPO.Id, RPO.RetailerId, RU.OrderNo, OrderStatus from RazorPayOrder RPO inner join RetailUser RU on RU.Id = RPO.RetailerId  where CreateDate > '{date}' and CreateDate < '{date1}' and (OrderStatus is null or (OrderStatus = 'SUCCESS' and CreditTranId is null)) and Provider = 'SabPaisa'";
            var result = con.Query<RazorpayOrderLite>(query, commandType: System.Data.CommandType.Text); ;
            // Optional: verify with status API
            foreach (var item in result)
            {
                try
                {
                    var t = Task.Run(async () =>
                    {
                        var verified = await _sabPaisaService.CheckStatusByJobAsync(item.Id);
                        RazorpayOrder razorpayOrder = StaticData.RazorpayOrderLoadByRazorpayId(verified.TxnId);
                        var fee = verified.PaidAmount - verified.Amount;
                        verified.Fee = fee;
                        if (item.OrderStatus != "SUCCESS")
                        {
                            StaticData.RazorpayOrderUpdateFees(item.Id, Convert.ToInt64(fee).ToString(), "", "", verified.Status);
                        }

                        if (razorpayOrder != null && razorpayOrder.Amount == verified.Amount)
                        {
                            RecordSaveResponse recordSaveResponse = StaticData.RazorpayOrderUpdateOPS(verified.TxnId, verified.SabPaisaTxnId, "");
                            if (recordSaveResponse.OperationMessage.Contains("Success") && verified.Status.ToUpper() == "SUCCESS")
                            {
                                RTran fundTransferRTran = new RTran();
                                try
                                {
                                    string tranType = "cr";

                                    fundTransferRTran.RequestIp = "Auto-Check";
                                    fundTransferRTran.RequestMachine = "Auto-Check";
                                    fundTransferRTran.RetailUserOrderNo = item.OrderNo;

                                    fundTransferRTran.Amount = verified.Amount;


                                    fundTransferRTran.Extra1 = "razor";
                                    fundTransferRTran.Extra2 = verified.TxnId;

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
                                        "Wallet topup via Razorpay order-" + verified.TxnId + ", payment id-" + verified.SabPaisaTxnId + ", method-" + verified.PaymentMode;
                                    //fundTransferRTran.Remarks = "Wallet topup of Rs. " + razorpayOrder.Amount.ToString() + " , fees-" + (rFee/100).ToString() + " via Razorpay order-" + o + ", payment id-" + p;
                                    fundTransferRTran.RequestMessage = "WEBPORTAL";

                                    if (fundTransferRTran.Amount > 0)
                                    {
                                        fundTransferRTran.TransferFundByData("admin");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    RazorpayOrderUpdateStausToFailed(item.Id, "failed");
                                }
                                finally
                                {
                                    fundTransferRTran = null;
                                }
                            }
                        }
                    });
                    t.Wait();
                }
                catch (Exception ex)
                {
                    RazorpayOrderUpdateStausToFailed(item.Id, "failed");
                }
            }
        }

        public void RazorpayOrderUpdateStausToFailed(string id, string pstatus)
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
    }
}
