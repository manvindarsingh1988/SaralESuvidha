using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Quartz;
using Razorpay.Api;
using SaralESuvidha.Models;
using SaralESuvidha.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SaralESuvidha.QuartzJobs
{
    public class CheckAndUpdateRazorpayStatusjob : IJob
    {
        string constring = string.Empty;
        public CheckAndUpdateRazorpayStatusjob(IConfiguration configuration)
        {
            constring = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                CheckAndUpdateRazorpayStatus();
            }
            catch
            { }
            try
            {
                CheckAndCreditAmountIfRazorResponseWasSuccess();
            }
            catch (Exception ex) { }
            await Task.CompletedTask;
        }

        private void CheckAndCreditAmountIfRazorResponseWasSuccess()
        {
            RazorpayClient client = new RazorpayClient(StaticData.rzp_ApiKey, StaticData.rzp_ApiSecret);
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

                            RazorpayOrder razorpayOrder = StaticData.RazorpayOrderLoadByRazorpayId(order.Attributes.id.ToString());
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
                                    fundTransferRTran.TransferFundByData("admin");
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

        private void CheckAndUpdateRazorpayStatus()
        {
            var date = DateTime.Now.ToString("yyyyMMdd");
            var date1 = DateTime.Now.AddMinutes(-30).ToString("yyyy-MM-dd HH:mm:ss");// Your API secret
            RazorpayClient client = new RazorpayClient(StaticData.rzp_ApiKey, StaticData.rzp_ApiSecret);
            var con = new SqlConnection(constring);
            var queryParameters = new DynamicParameters();
            var query = $"Select RPO.Id, RPO.RetailerId, RU.OrderNo from RazorPayOrder RPO inner join RetailUser RU on RU.Id = RPO.RetailerId  where CreateDate > '{date}' and CreateDate < '{date1}' and OrderStatus is null and Provider = 'Razor'";
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

                                RazorpayOrder razorpayOrder = StaticData.RazorpayOrderLoadByRazorpayId(order.Attributes.id.ToString());
                                string payload = $"{order.Attributes.id.ToString()}|{payment.Attributes?.id}";

                                //Step 2: Compute HMAC SHA256
                                using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(StaticData.rzp_ApiSecret)))
                                {
                                    byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                                    regeneratedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();
                                }
                                ////string retailerId, string OrderId, string CustomerMobile, string RazorpayOrderId, string logType, string logData, DateTime CreateDate
                                StaticData.RazorpayLogSave(razorpayOrder.RetailerId, "", "", order.Attributes.id.ToString(), "PostPayment", "o=" + order.Attributes.id.ToString() + ">>p=" + payment.Attributes?.id.ToString() + ">>s=" + regeneratedSignature, DateTime.Now);
                                StaticData.RazorpayLogSave(razorpayOrder.RetailerId, razorpayOrder.Id, razorpayOrder.CustomerMobile, order.Attributes.id.ToString(), "PaymentCheck", JsonConvert.SerializeObject(payment), DateTime.Now);

                                StaticData.RazorpayOrderUpdateFees(razorpayOrder.razorpay_order_id, rFee.ToString(), rTax.ToString(), oFee.ToString(), r_status);

                                if (r_status == "captured" && razorpayOrder.RazorpayAmount == rAmount)
                                {
                                    RecordSaveResponse recordSaveResponse = StaticData.RazorpayOrderUpdateOPS(order.Attributes.id.ToString(), payment.Attributes?.id.ToString(), regeneratedSignature);
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
                                                fundTransferRTran.TransferFundByData("admin");
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

    public class RazorpayOrderLite
    {
        public string Id { get; set; }
        public string RetailerId { get; set; }
        public int OrderNo { get; set; }
        public string OrderStatus { get; set; }
    }
}
