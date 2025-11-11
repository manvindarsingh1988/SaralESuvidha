using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Quartz;
using Razorpay.Api;
using SaralESuvidha.Filters;
using SaralESuvidha.Models;
using SaralESuvidha.QuartzJobs;
using SaralESuvidha.Services;
using SaralESuvidha.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SaralESuvidha.Controllers
{
    [RetailUserCommonFilter]
    public class RetailUserCommonController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly SabPaisaService _sabPaisaService;
        private readonly ISchedulerFactory _schedulerFactory;

        public RetailUserCommonController(IWebHostEnvironment hostingEnvironment, SabPaisaService sabPaisaService, ISchedulerFactory schedulerFactory)
        {
            _hostingEnvironment = hostingEnvironment;
            _sabPaisaService = sabPaisaService;
            _schedulerFactory = schedulerFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Logout()
        {
            try
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                return Content("Exception: " + ex.Message);
            }
        }

        public async Task<IActionResult> Initiate(string name, decimal amount, string email, string mobile)
        {
            var userType = HttpContext.Session.GetInt32("RetailerType");
            var controller = "";
            if (userType == 5)
            {
                controller = "RetailClient";
            }
            else if (userType == 6)
            {
                controller = "Distributor";
            }
            else
            {
                controller = "MasterDistributor";
            }
            var result = await SabPaisaHelper.IntiateOrder(_sabPaisaService, name, amount, email, mobile, Url.Action("SabPaisaCallback", controller, null, "https"), HttpContext.Session.GetString("RetailerId"));
            return Content(result);
        }

        public IActionResult GenerateOrder(string name, decimal amount, string email, string mobile)
        {
            if (StaticData.CheckTopupServiceIsDown("Razor") == false)
            {
                return Content("Error: Please refresh the page and try again.");
            }
            string retailerId = HttpContext.Session.GetString("RetailerId");
            string result = string.Empty;
            try
            {
                if (amount > 0)
                {
                    decimal tempAmount = Convert.ToDecimal(amount.ToString("N2"));
                    //tempAmount = tempAmount * 100;
                    string amountWithPaisa = (tempAmount*100).ToString("N0").Replace(",","");
                    var orderResponse = StaticData.RazorpayOrderSave(name, tempAmount, amountWithPaisa, email, mobile, retailerId);

                    
                    if (!string.IsNullOrEmpty(orderResponse.Id))
                    {
                        //Order saved
                        RazorpayClient client = new RazorpayClient(StaticData.rzp_ApiKey, StaticData.rzp_ApiSecret);

                        Dictionary<string, object> options = new Dictionary<string,object>();
                        options.Add("amount", amountWithPaisa); // amount in the smallest currency unit
                        options.Add("receipt", orderResponse.Id);
                        options.Add("currency", "INR");
                        Order order = client.Order.Create(options);
                        
                        result =  JsonConvert.SerializeObject(order);
                        StaticData.RazorpayLogSave(retailerId, orderResponse.Id, mobile, order.Attributes.id.ToString(),
                            "order_pre", result, DateTime.Now);
                        
                        if (!string.IsNullOrEmpty(order.Attributes.id.ToString()))
                        {
                            //order successfully posted to razorpay
                            RecordSaveResponse orderIdSaveResponse = StaticData.RazorpayOrderUpdateOrderId(orderResponse.Id, order.Attributes.id.ToString());
                            
                            if (!string.IsNullOrEmpty(order.Attributes.id.ToString()))
                            {
                                result = JsonConvert.SerializeObject(StaticData.RazorpayOrderLoad(orderResponse.Id));
                                //RedirectToAction("AccountTopup","RetailClient")
                            }
                            else
                            {
                                result = "Error: Error in updating razorpay order id.";
                            }
                            
                        }
                        else
                        {
                            result = "Error: Error in posting order to razorpay.";
                        }
                    }
                    else
                    {
                        result = "Error: " + orderResponse.OperationMessage;
                    }
                    
                }
                else
                {
                    result = "Error: Invalid amount.";
                }
            }
            catch (Exception ex)
            {
                result = "Error: Can not generate order. " + ex.Message;
            }

            return Content(result);
        }
        
        public IActionResult PGOrder(string o, string p, string s)
        {
            string result = string.Empty;

            try
            {
                StaticData.RazorpayLogSave(HttpContext.Session.GetString("RetailerId"), "", "", o, "PostPayment", "o=" + o + ">>p=" + p + ">>s=" + s,
                    DateTime.Now);

                RazorpayClient client = new RazorpayClient(StaticData.rzp_ApiKey, StaticData.rzp_ApiSecret);

                Dictionary<string, string> options = new Dictionary<string, string>();
                options.Add("razorpay_order_id", o);
                options.Add("razorpay_payment_id", p);
                options.Add("razorpay_signature", s);

                Utils.verifyPaymentSignature(options);

                Payment payment = client.Payment.Fetch(p);

                if (payment.Attributes?.error == null)
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
                        RazorpayOrder razorpayOrder = StaticData.RazorpayOrderLoadByRazorpayId(o);

                        StaticData.RazorpayLogSave(HttpContext.Session.GetString("RetailerId"), razorpayOrder.Id, razorpayOrder.CustomerMobile,
                            o, "PaymentCheck", JsonConvert.SerializeObject(payment), DateTime.Now);
                        StaticData.RazorpayOrderUpdateFees(o, rFee.ToString(), rTax.ToString(), oFee.ToString(),r_status);

                        if (razorpayOrder.RazorpayAmount == rAmount)
                        {
                            RecordSaveResponse recordSaveResponse = StaticData.RazorpayOrderUpdateOPS(o, p, s);
                            if (recordSaveResponse.OperationMessage.Contains("Success"))
                            {
                                RTran fundTransferRTran = new RTran();
                                try
                                {
                                    string tranType = "cr";

                                    fundTransferRTran.RequestIp = HttpContext.Connection.RemoteIpAddress.ToString();
                                    fundTransferRTran.RequestMachine = HttpContext.Request.Headers["User-Agent"].ToString();
                                    fundTransferRTran.RetailUserOrderNo = (int)HttpContext.Session.GetInt32("RetailUserOrderNo"); //

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
                                    fundTransferRTran.Extra2 = o;

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
                                        "Wallet topup via Razorpay order-" + o + ", payment id-" + p + ", method-" + r_method;
                                    //fundTransferRTran.Remarks = "Wallet topup of Rs. " + razorpayOrder.Amount.ToString() + " , fees-" + (rFee/100).ToString() + " via Razorpay order-" + o + ", payment id-" + p;
                                    fundTransferRTran.RequestMessage = "WEBPORTAL";

                                    if (fundTransferRTran.Amount > 0)
                                    {
                                        result = fundTransferRTran.TransferFundByData("admin");
                                    }
                                    else
                                    {
                                        result = "Errors: Invalid amount, can not process transfer.";
                                    }
                                }
                                catch (Exception ex)
                                {
                                    result = "Errors: Exception: " + ex.Message;
                                }
                                finally
                                {
                                    fundTransferRTran = null;
                                }
                            }
                            else
                            {
                                result = "Errors: Invalid amount, can not process transfer.";
                            }
                        }
                        else
                        {
                            result =
                                "Error: Unable to verify payment details, invalid amount received. Please contact Saral e-Suvidha custome support.";
                        }
                    }
                    else
                    {
                        result = "Error: Unable to get payment details(id), please contact customer support.";
                    }
                }
                else
                {
                    result = "Error: Unable to verify payment details, please contact customer support.";
                }
            }
            catch (Exception ex)
            {
                result = "Error: Error in processing payments. " + ex.Message;
            }

            return Content(result);
        }
        
        public IActionResult DailyTopupReportResult(string dateFrom,string dateTo, int x)
        {
            string result = string.Empty;
            string filePath = Path.Combine(_hostingEnvironment.WebRootPath, "FileData/");

            try
            {
                DateTime dateF = Convert.ToDateTime(StaticData.ConvertHexToString(dateFrom));
                DateTime dateT = Convert.ToDateTime(StaticData.ConvertHexToString(dateTo));
                
                if (HttpContext.Session != null)
                    result = StaticData.TopupReportRetailClientByDate((int)HttpContext.Session.GetInt32("RetailUserOrderNo"), dateF, dateT, x, filePath);
            }
            catch (Exception ex)
            {
                result = "Errors: Exception: " + ex.Message;
            }
            return Content(result);
        }

        public IActionResult GetBalanceWithName()
        {
            string result = string.Empty;
            try
            {
                var balResponse = StaticData.retailUser.GetBalanceWithName(HttpContext.Session.GetInt32("RetailUserOrderNo"));
                if (!balResponse.OperationMessage.Contains("Errors"))
                {
                    //ViewData["Error"] = "0";
                    return Content(" [" + balResponse.Order.ToString() + " - " + balResponse.OperationMessage + "is &#x20B9; " + balResponse.Balance.ToString("N2") + "] ");
                }
                else
                {
                    //ViewData["Error"] = "1";
                    return Content(" [" + balResponse.Order.ToString() + " - " + balResponse.OperationMessage + "]");
                }
            }
            catch (Exception ex)
            {
                result = "Errors: Exception: Can not get balance details." + ex.Message;
            }

            return Content(result);
        }

        public IActionResult GetBalance()
        {
            string result = string.Empty;
            try
            {
                var balResponse = StaticData.retailUser.GetBalanceWithName(HttpContext.Session.GetInt32("RetailUserOrderNo"));
                if (!balResponse.OperationMessage.Contains("Errors"))
                {
                    //ViewData["Error"] = "0";
                    return Content(balResponse.Balance.ToString("N2"));
                }
                else
                {
                    //ViewData["Error"] = "1";
                    return Content("NA");
                }
            }
            catch (Exception ex)
            {
                result = "Errors: Exception: Can not get balance details." + ex.Message;
            }

            return Content(result);
        }

        public IActionResult GetBalanceUWalletByOrderNo()
        {
            string result = string.Empty;
            try
            {
                using (var connection = new SqlConnection(StaticData.conString))
                {
                    var balance = connection.ExecuteScalar<decimal>("SELECT dbo.RetailUserUBalanceByOrderNo(@RetailUserId)", new { RetailUserId = HttpContext.Session.GetInt32("RetailUserOrderNo") });
                    return Content(balance.ToString("N2"));
                }
            }
            catch (Exception ex)
            {
                result = "Errors: Exception: Can not get balance details." + ex.Message;
            }

            return Content(result);
        }

        public IActionResult GetBalanceSWalletByOrderNo()
        {
            string result = string.Empty;
            try
            {
                using (var connection = new SqlConnection(StaticData.conString))
                {
                    var balance = connection.ExecuteScalar<decimal>("SELECT dbo.RetailUserSBalanceByOrderNo(@RetailUserId)", new { RetailUserId = HttpContext.Session.GetInt32("RetailUserOrderNo") });
                    return Content(balance.ToString("N2"));
                }
            }
            catch (Exception ex)
            {
                result = "Errors: Exception: Can not get balance details." + ex.Message;
            }

            return Content(result);
        }


        public IActionResult DailyClientStatementResult(string dateFrom, string dateTo, int x)
        {
            string filePath = Path.Combine(_hostingEnvironment.WebRootPath, "FileData/");
            string result = string.Empty;
            try
            {
                int orderNo = (int) HttpContext.Session.GetInt32("RetailUserOrderNo");
                DateTime dateF = Convert.ToDateTime(StaticData.ConvertHexToString(dateFrom));
                DateTime dateT = Convert.ToDateTime(StaticData.ConvertHexToString(dateTo));
                result = StaticData.RechargeReportRetailClientByDate(orderNo, dateF, dateT, x, filePath);
            }
            catch (Exception ex)
            {
                result = "Errors: Exception: " + ex.Message;
            }
            return Content(result);
        }

        
        public IActionResult DailyClientStatementSWalletResult(string dateFrom, string dateTo, int x)
        {
            string filePath = Path.Combine(_hostingEnvironment.WebRootPath, "FileData/");
            string result = string.Empty;
            try
            {
                int orderNo = (int) HttpContext.Session.GetInt32("RetailUserOrderNo");
                DateTime dateF = Convert.ToDateTime(StaticData.ConvertHexToString(dateFrom));
                DateTime dateT = Convert.ToDateTime(StaticData.ConvertHexToString(dateTo));
                result = StaticData.RechargeReportSWalletRetailClientByDate(orderNo, dateF, dateT, x, filePath);
            }
            catch (Exception ex)
            {
                result = "Errors: Exception: " + ex.Message;
            }
            return Content(result);
        }

        
        public IActionResult FundReportResult(string dateFrom, string dateTo)
        {
            string result = string.Empty;
            try
            {
                DateTime dateF = Convert.ToDateTime(StaticData.ConvertHexToString(dateFrom));
                DateTime dateT = Convert.ToDateTime(StaticData.ConvertHexToString(dateTo));
                if (HttpContext.Session != null)
                    result = StaticData.FunTransferBetweenPeriod(dateF, dateT, HttpContext.Session.GetString("RetailerId"));
            }
            catch (Exception ex)
            {
                result = "Errors: Exception: " + ex.Message;
            }
            return Content(result);
        }
        
        public IActionResult RefundReportResult(string dateFrom, string dateTo)
        {
            string result = string.Empty;
            try
            {
                DateTime dateF = Convert.ToDateTime(StaticData.ConvertHexToString(dateFrom));
                DateTime dateT = Convert.ToDateTime(StaticData.ConvertHexToString(dateTo));
                if (HttpContext.Session != null)
                    result = StaticData.RefundBetweenPeriod(dateF, dateT, HttpContext.Session.GetString("RetailerId"));
            }
            catch (Exception ex)
            {
                result = "Errors: Exception: " + ex.Message;
            }
            return Content(result);
        }

        public IActionResult CustomerSupportDetailUpdate(string receiptMessage, bool read = false)
        {
            if (HttpContext.Session != null)
            {
                if (read)
                {
                    using var retailUser = new RetailUser();
                    return Content(retailUser.CurrentReceiptMessage(HttpContext.Session.GetString("RetailerId")));
                }
                else
                {
                    using var retailUser = new RetailUser();
                    return Content(retailUser.UpdateReceiptMessage(HttpContext.Session.GetString("RetailerId"), receiptMessage,
                        "Last update at " + DateTime.Now.ToLongDateString() + ", IP address-" + HttpContext.Connection.RemoteIpAddress.ToString()));
                }
            }
            else
            {
                return Content("Invalid login");
            }
        }

        public IActionResult RetailUserDetail(string usd)
        {
            try
            {
                string currentUserId = HttpContext.Session.GetString("RetailerId");
                int Id = Convert.ToInt32(StaticData.ConvertHexToString(usd));
                var balResponse = StaticData.retailUser.GetBalanceWithName(Id, currentUserId);
                if (!balResponse.OperationMessage.Contains("Errors"))
                {
                    //ViewData["Error"] = "0";
                    return Content(" [" + balResponse.Order.ToString() + " - " + balResponse.OperationMessage + "is &#x20B9; " + balResponse.Balance.ToString("N2") + "]");
                }
                else
                {
                    //ViewData["Error"] = "1";
                    return Content(" [" + balResponse.Order.ToString() + " - " + balResponse.OperationMessage + "]");
                }
            }
            catch (Exception ex)
            {
                return Content("Exception: " + ex.Message);
            }
        }
        
        public IActionResult RetailUserDetailWhiteLabel(string usd)
        {
            try
            {
                string currentUserId = HttpContext.Session.GetString("RetailerId");
                int Id = Convert.ToInt32(StaticData.ConvertHexToString(usd));
                var balResponse = StaticData.retailUser.GetBalanceWithNameWhiteLabel(Id, currentUserId);
                if (!balResponse.OperationMessage.Contains("Errors"))
                {
                    //ViewData["Error"] = "0";
                    return Content(" [" + balResponse.Order.ToString() + " - " + balResponse.OperationMessage + "is &#x20B9; " + balResponse.Balance.ToString("N2") + "]");
                }
                else
                {
                    //ViewData["Error"] = "1";
                    return Content(" [" + balResponse.Order.ToString() + " - " + balResponse.OperationMessage + "]");
                }
            }
            catch (Exception ex)
            {
                return Content("Exception: " + ex.Message);
            }
        }
        
        [HttpGet]
        public IActionResult AmountToWord(int amount = 0)
        {
            try
            {
                string amountToWord = StaticData.AmountToInr(amount);
                return Content(amountToWord);
            }
            catch (Exception ex)
            {
                return Content("Errors: Exception: " + ex.Message);
            }
        }


        
        public IActionResult RetailerMarginReport(string dateFrom, string dateTo)
        {
            string result = "";
            try
            {
                DateTime dateF = Convert.ToDateTime(StaticData.ConvertHexToString(dateFrom));
                DateTime dateT = Convert.ToDateTime(StaticData.ConvertHexToString(dateTo));
                if (HttpContext.Session != null)
                    result = StaticData.MarginReportByRetailer(HttpContext.Session.GetString("RetailerId"), dateF, dateT);
            }
            catch (Exception ex)
            {
                result = "Exception: " + ex.Message;
            }

            return Content(result);
        }
        
        public IActionResult RetailerMasterMarginReport(string dateFrom, string dateTo)
        {
            string result = "";
            try
            {
                DateTime dateF = Convert.ToDateTime(StaticData.ConvertHexToString(dateFrom));
                DateTime dateT = Convert.ToDateTime(StaticData.ConvertHexToString(dateTo));
                if (HttpContext.Session != null)
                    result = StaticData.MarginReportByRetailer(HttpContext.Session.GetString("RetailerId"), dateF, dateT, true);
            }
            catch (Exception ex)
            {
                result = "Exception: " + ex.Message;
            }

            return Content(result);
        }

        public IActionResult MarginSheet()
        {
            string result = "";
            try
            {
                if (HttpContext.Session != null)
                    result = StaticData.MarginSheetByRetailer(HttpContext.Session.GetString("RetailerId"));
            }
            catch (Exception ex)
            {
                result = "Exception: " + ex.Message;
            }

            return Content(result);
        }

        public IActionResult MarginSheetDownline()
        {
            string result = "";
            try
            {
                if (HttpContext.Session != null)
                    result = StaticData.MarginSheetDownlineByRetailer(HttpContext.Session.GetString("RetailerId"));
            }
            catch (Exception ex)
            {
                result = "Exception: " + ex.Message;
            }

            return Content(result);
        }
        
        public IActionResult MarginSheetDownlineWhiteLabel()
        {
            string result = "";
            try
            {
                if (HttpContext.Session != null)
                    result = StaticData.MarginSheetDownlineByRetailerWhiteLabel(HttpContext.Session.GetString("RetailerId"));
            }
            catch (Exception ex)
            {
                result = "Exception: " + ex.Message;
            }

            return Content(result);
        }
        
        public IActionResult MarginSheetListByOrderNoPV(string usd)
        {
            string result = "";
            try
            {
                int Id = Convert.ToInt32(StaticData.ConvertHexToString(usd));
                result = StaticData.MarginSheetByRetailerOrderNoParentValidation(Id, HttpContext.Session.GetString("RetailerId"));
            }
            catch (Exception ex)
            {
                result = "Exception: " + ex.Message;
            }

            return Content(result);
        }

        public IActionResult UpdatePassword(string op, string np)
        {
            string result = "";
            try
            {
                np = np.Length > 20 ? np.Substring(0, 20) : np;
                result = StaticData.UpdatePassword(op, np, HttpContext.Session.GetString("RetailerId"));
            }
            catch (Exception ex)
            {
                result = "Exception: " + ex.Message;
            }

            return Content(result);
        }

        public IActionResult UpdatePin(string op)
        {
            string result = "";
            try
            {
                string np = StaticData.EncodePIN(StaticData.Random6DigitPINString());
                op = Regex.Replace(op, @"[^0-9a-zA-Z]+", "");
                
                result = StaticData.ResetPin(op, np, HttpContext.Session.GetString("RetailerId"));
            }
            catch (Exception ex)
            {
                result = "Exception: " + ex.Message;
            }

            return Content(result);
        }

        public IActionResult ResendPin(string op)
        {
            string result = "";
            try
            {
                string pin = StaticData.RetailUserPin(HttpContext.Session.GetString("RetailerId"));
                
                result = StaticData.ResendRetailUserPin(HttpContext.Session.GetString("RetailerId"), pin);

            }
            catch (Exception ex)
            {
                result = "Exception: " + ex.Message;
            }

            return Content(result);
        }

        public IActionResult UpdateDefaultOperator(string op)
        {
            string result = "";
            try
            {
                result = StaticData.UpdateDefaultOperator(op, HttpContext.Session.GetString("RetailerId"));
                if (result.Contains("Success"))
                {
                    HttpContext.Session.SetString("DefaultUtilityOperator", op);
                }
            }
            catch (Exception ex)
            {
                result = "Exception: " + ex.Message;
            }

            return Content(result);
        }
        
        public IActionResult UpdateDefaultPrinter(string defaultPrinter)
        {
            string result = "";
            try
            {
                result = StaticData.UpdateDefaultPrinter(defaultPrinter, HttpContext.Session.GetString("RetailerId"));
                if (result.Contains("Success"))
                {
                    HttpContext.Session.SetString("DefaultPrinter", defaultPrinter);
                }
            }
            catch (Exception ex)
            {
                result = "Exception: " + ex.Message;
            }

            return Content(result);
        }

        public IActionResult RofferMobile(string o, string m)
        {
            string result = string.Empty;
            //m = Regex.Replace(m, @"[^0-9a-zA-Z]+", "");
            //o = Regex.Replace(o, @"[^0-9a-zA-Z]+", "");
            
            try
            {
                result = StaticData.ReadURL(StaticData.rofferMobileUrl.Replace("[op]", o).Replace("[mo]", m.Trim()));
            }
            catch (Exception ex)
            {
                result = "Errors: " + ex.Message;
            }
            
            return Content(result);
        }
        
        public IActionResult RofferDth(string o, string m)
        {
            string result = string.Empty;
            //m = Regex.Replace(m, @"[^0-9a-zA-Z]+", "");
            //o = Regex.Replace(o, @"[^0-9a-zA-Z]+", "");
            
            try
            {
                result = StaticData.ReadURL(StaticData.rofferDthCustInfoUrl.Replace("[op]", o).Replace("[mo]", m.Trim()));
            }
            catch (Exception ex)
            {
                result = "Errors: " + ex.Message;
            }
            
            return Content(result);
        }

        public IActionResult CheckAndUpdatePendingTopup()
        {
            string result = string.Empty;
            Task.Run(async () =>
            {
                var scheduler = await _schedulerFactory.GetScheduler();
                var executingJobs = await scheduler.GetCurrentlyExecutingJobs();
                if (executingJobs != null && executingJobs.Any())
                {
                    result = "Errors: Failed to update the status. Please try again after sometime.";
                }
                else
                {
                    var userId = HttpContext.Session.GetString("RetailerId");
                    try
                    {
                        SabpaisaStatusCheckJob.CheckAndUpdateSabpaisaStatus(userId);
                        CheckAndUpdateRazorpayStatusjob.CheckAndUpdateRazorpayStatus(userId);
                        CheckAndUpdateRazorpayStatusjob.CheckAndCreditAmountIfRazorResponseWasSuccess(userId);
                        result = "Status updated successfully";
                    }
                    catch
                    {
                        result = "Errors: Failed to update the status. Please try again after sometime.";
                    }
                }
            }).Wait();
            return Content(result);
        }
    }
}
