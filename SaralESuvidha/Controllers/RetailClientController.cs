using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SaralESuvidha.Filters;
using SaralESuvidha.ViewModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Session;
using Newtonsoft.Json;
using OfficeOpenXml;
using Razorpay.Api;
using UPPCLLibrary;
using UPPCLLibrary.BillFetch;
using RTran = SaralESuvidha.Models.RTran;

namespace SaralESuvidha.Controllers
{
    [RetailUserFilter]
    public class RetailClientController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;

        public RetailClientController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }
        

        public IActionResult ChangePassword()
        {
            return View();
        }

        public IActionResult DefaultOperator()
        {
            return View();
        }
        

        public IActionResult DuplicateBillInfo()
        {
            string eBillInfo = HttpContext.Session.GetString("billinfo");
            return Content(eBillInfo);
        }
        
        public IActionResult PayBillUPPCL_A(string operatorName, string accountNumber, decimal billAmount, string additionalInfo1 = "", string customerName = null, string dueDate = null, string dueAmount = null, string p1 = "", string p2 = "", string inputSource = "web", string pi="")
        {
            
            string result = string.Empty;
            string retailerId = HttpContext.Session.GetString("RetailerId");
            string eBillInfo = HttpContext.Session.GetString("billinfo");
            string retailUserOrderNo = HttpContext.Session.GetInt32("RetailUserOrderNo").ToString();
            string retailUserName = HttpContext.Session.GetString("RetailerName");
            string userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            string requestIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            //string retailerId, string eBillInfo, string retailUserOrderNo, string retailUserName, string userAgent, string requestIp

            result = StaticData.PayBillUPPCL(operatorName, accountNumber, billAmount, retailerId,eBillInfo, retailUserOrderNo, retailUserName, userAgent, requestIp,
                additionalInfo1, customerName, dueDate, dueAmount, p1, p2, inputSource, pi);
            
            return Content(result);
        }
        
        public IActionResult SearchRecharge()
        {
            return View();
        }

        public IActionResult SearchNumberResult(string rNumber, string rDate)
        {
            string result = string.Empty;
            try
            {
                DateTime dateR = Convert.ToDateTime(StaticData.ConvertHexToString(rDate));
                if (HttpContext.Session != null)
                    result = StaticData.SearchNumberByDateByClient(StaticData.ConvertHexToString(rNumber), dateR,(int)HttpContext.Session.GetInt32("RetailUserOrderNo"));
            }
            catch (Exception ex)
            {
                result = "Errors: Exception: " + ex.Message;
            }
            return Content(result);
        }

        public IActionResult RaiseDispute(string t)
        {
            string rtranId = StaticData.ConvertHexToString(t);
            //RTranReportServerWise rt = new RTranReportServerWise{Rid = rtranId};
            return View(new RTranReportServerWise{Rid = rtranId}.LoadForRefundRequest());
        }
        
        [HttpPost]
        public IActionResult SaveDispute(string Rid, string rem)
        {
            var billTran = new RTran();
            string result = string.Empty;
            try
            {
                if (HttpContext.Session.GetString("RetailerId") != null)
                {
                    billTran.RetailUserId = HttpContext.Session.GetString("RetailerId");
                    billTran.Id = Rid;
                    billTran.RefundRequestData = rem;

                    result = billTran.SaveRefundRequest();
                }
                else
                {
                    return Content("Errors: Invalid login session.");
                }
            }
            catch (Exception ex)
            {
                result += "Errors: Exception: " + ex.Message;
            }
            finally
            {
                billTran = null;
            }

            return Content(result);
        }

        public IActionResult DisputeList()
        {
            return View();
        }

        public IActionResult BankDetails()
        {
            return View();
        }
        
        public IActionResult AccountStatement()
        {
            return View();
        }

        public IActionResult DailyRechargeReport()
        {
            return View();
        }

        public IActionResult DailySales()
        {
            return View();
        }
        
        public IActionResult EBillInfo(string operatorName, string accountNumber)
        {
            UPPCLManager.CheckTokenExpiry();
            
            if (StaticData.RecordCount(operatorName, accountNumber) > 0)
            {
                ESuvidhaBillFetchResponse eSuvidhaBillFetchResponse = new ESuvidhaBillFetchResponse();
                eSuvidhaBillFetchResponse.Reason = "Payment request will not be initiated, payment will be kept blocked for the Day.";
                return Content(JsonConvert.SerializeObject(eSuvidhaBillFetchResponse));
            }
            else
            {
                HttpContext.Session.SetString("billinfo", "");
                string eBillInfo = StaticData.ElectricityBillInfoUPPCL(operatorName, accountNumber);
                
                HttpContext.Session.SetString("billinfo", eBillInfo);
                return Content(eBillInfo);
            }
            
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

        public IActionResult DailySalesReportResult(string dateFrom, int x)
        {
            string result = string.Empty;
            string filePath = Path.Combine(_hostingEnvironment.WebRootPath, "FileData/");

            try
            {
                DateTime dateF = Convert.ToDateTime(StaticData.ConvertHexToString(dateFrom));
                if (HttpContext.Session != null)
                    result = StaticData.SalesReportRetailClientByDate((int)HttpContext.Session.GetInt32("RetailUserOrderNo"), dateF, x, filePath);
            }
            catch (Exception ex)
            {
                result = "Errors: Exception: " + ex.Message;
            }
            return Content(result);
        }

        
        
        public IActionResult DailyRechargeReportResult(string dateFrom, string dateTo, int x)
        {
            string result = string.Empty;
            string filePath = Path.Combine(_hostingEnvironment.WebRootPath,"FileData/");
            
            try
            {
                DateTime dateF = Convert.ToDateTime(StaticData.ConvertHexToString(dateFrom));
                DateTime dateT = Convert.ToDateTime(StaticData.ConvertHexToString(dateTo));
                if (HttpContext.Session != null)
                    result = StaticData.RechargeReportRetailClientByDate((int) HttpContext.Session.GetInt32("RetailUserOrderNo"), dateF, dateT, x, filePath);
            }
            catch (Exception ex)
            {
                result = "Errors: Exception: " + ex.Message;
            }
            return Content(result);
        }

        public IActionResult DailyRechargeReportSummaryResult(string dateFrom, string dateTo, int x)
        {
            string result = string.Empty;
            string filePath = Path.Combine(_hostingEnvironment.WebRootPath,"FileData/");
            
            try
            {
                DateTime dateF = Convert.ToDateTime(StaticData.ConvertHexToString(dateFrom));
                DateTime dateT = Convert.ToDateTime(StaticData.ConvertHexToString(dateTo));
                if (HttpContext.Session != null)
                    result = StaticData.RechargeSummaryReportRetailClientByDate((int)HttpContext.Session.GetInt32("RetailUserOrderNo"), dateF, dateT, x, filePath);
            }
            catch (Exception ex)
            {
                result = "Errors: Exception: " + ex.Message;
            }
            return Content(result);
        }

        public IActionResult MarginSheet()
        {
            return View();
        }

        public IActionResult FundReport()
        {
            return View();
        }

        public IActionResult CustomerSupportDetail()
        {
            return View();
        }

        public IActionResult T()
        {
            return View();
        }

        public IActionResult CreatePlan()
        {
            if (HttpContext.Session.GetInt32("UserType") != null)
            {
                if (HttpContext.Session.GetInt32("UserType") == 7 || HttpContext.Session.GetInt32("UserType") == 6)
                {
                    return View();
                }
                else
                {
                    return Content("Not Authorized");
                }
            }
            else
            {
                return Content("Not Authorized");
            }
        }

        public IActionResult MarginReport()
        {
            return View();
        }
        
        public IActionResult PrintReceiptJson(string t)
        {
            //PaymentReceipt pr = new PaymentReceipt();
            //pr.TelecomOperatorName = "Test";
            string tranId = "";
            try
            {
                tranId = StaticData.ConvertHexToString(t);
                return Content(StaticData.ReceiptByTranId(tranId));
            }
            catch (Exception)
            {
                return Content("INVALID DETAILS");
            }
        }
        
        public IActionResult PrintReceiptThermal(string t)
        {
            //PaymentReceipt pr = new PaymentReceipt();
            //pr.TelecomOperatorName = "Test";
            string tranId = "";
            try
            {
                string result = string.Empty;
                tranId = StaticData.ConvertHexToString(t);
                //PaymentReceipt pr = JsonConvert.DeserializeObject<PaymentReceipt>(StaticData.ReceiptByTranId(tranId));
                PaymentReceiptUPPCL pr = StaticData.PaymentReceiptUPPCLByTranId(tranId);
                if (pr != null)
                {
                    result = "\n[C]<u><font size='big'>UPPCL BILL PAY" + "</font></u>\n" +
                             "[C]================================\n" +
                             "[L]<font size='tall'><b>" + pr.TelecomOperatorName + "</b></font>\n" +
                             "[C]--------------------------------\n" +
                             "[C]<barcode type='128' height='10'>" + pr.GetRTran() +
                             "</barcode>\n" +
                             "[C]================================\n" +




                             "[L]<b>RECEIPT NO: </b>" + pr.GetRTran() + "\n" +
                             "[L]<b>BILL DATE: </b>" + pr.UPPCL_BillDate + "\n" +
                             "[L]<b>DUE DATE: </b>" + pr.Extra1 + "\n" +
                             "[L]<b>PAYMENT DATE: </b>" + Convert.ToDateTime(pr.CreateDate).ToString("dd-MM-yyyy HH:mm") + "\n" +
                             "[C]--------------------------------\n" +
                             "[L]<b>DIVISION: </b>" + pr.UPPCL_Division + "\n" +
                             "[L]<b>SUB DIVISION: </b>" + pr.UPPCL_SubDivision + "\n" +
                             "[L]<b>MOBILE: </b>" + pr.EndCustomerMobileNumber + "\n" +
                             "[L]<b>ACCOUNT NO: </b>" + pr.RechargeMobileNumber + "\n" +
                             "[L]<b>CONSUMER NAME: " + pr.EndCustomerName + "</b>\n" +
                             "[L]<b>BILL AMOUNT: </b>Rs. " + pr.UPPCL_BillAmount + "\n" +
                             "[L]<b>OUTSTANDING AMOUNT: </b>Rs. " + pr.Extra2 + "\n" +
                             "[L]<b>PAID AMOUNT: </b>Rs. " + pr.Amount + "\n" +
                             "[L]<b>BALANCE AMOUNT: </b>Rs. " + pr.GetBalanceBillAmount() + "\n" +
                             "[L]<b>(" + pr.AmountInWords.ToUpper() + ")</b>\n" +
                             "[L]<b>STATUS: </b>" + pr.RechargeStatus + "\n" +
                             "[L]<b>PAYMENT TYPE: </b>WALLET PAYMENT" + "\n" +

                             "[L]\n" +
                             "[C]--------------------------------\n" +
                             "[R]<b>TOTAL AMOUNT : " + pr.Amount + "</b>\n" +
                             "[R]DATE : " + pr.CreateDate + "\n" +
                             //"[R]PART PAYMENT : " + (pr.Parameter1 == "PartPayment" ? "YES" : "NO") + "\n" +
                             "[L]\n" +
                             "[C]================================\n" +
                             "[L]<b>Printed By: </b>" + pr.RetailerName.ToUpper() + "\n" +
                             "[L]<u><b>Printed On: </b>" + DateTime.Now.ToString("dd-MM-yyyy HH:mm") + "</u>\n" +
                             "[L]<b>Agency: </b>" + pr.AgencyName + "\n" +
                             "[L]<b>Agent Id: </b>" + pr.RetailUserId + "\n" +
                             "[L]\n" + pr.ReceiptMessage + "\n" +
                             "[L]This is computer generated receipt, does not require signature.\n" +
                             "[L]Your bill will be updated within 1 days from the date of payment received. .\n" +

                             "[C]\n<qrcode size='20'>http://saralesuvidha.com/Home/ReceiptUPPCL?t=" + StaticData.ConvertStringToHex(pr.Id) +
                             "</qrcode>\n" +
                             "[L]\n[L]\n[L]\n";
                    result = StaticData.EncodeBase64(Encoding.UTF8, result);

                }
                return Content(result);
            }
            catch (Exception)
            {
                return Content("INVALID DETAILS");
            }
        }

        public IActionResult AccountTopup()
        {
            return View();
        }
        
        public IActionResult GenerateOrder(string name, decimal amount, string email, string mobile)
        {
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
                                    fundTransferRTran.Amount = Convert.ToDecimal(((decimal)rAmount / 100) - ((decimal)rFee / 100) - ((decimal)oFee/100));

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
                                        "Wallet topup via Razorpay order-" + o + ", payment id-" + p;
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

        public IActionResult AccountTopupReport()
        {
            return View();
        }
        
    }
}
