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
using Microsoft.AspNetCore.Http;
using RTran = SaralESuvidha.Models.RTran;
using SaralESuvidha.Models;
using Microsoft.AspNetCore.Authorization;
using SaralESuvidha.Services;

namespace SaralESuvidha.Controllers
{
    [RetailUserFilter]
    public class RetailClientController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly SabPaisaService _sabPaisaService;

        public RetailClientController(IWebHostEnvironment hostingEnvironment, SabPaisaService sabPaisaService)
        {
            _hostingEnvironment = hostingEnvironment;
            _sabPaisaService = sabPaisaService;
        }

        public IActionResult Index()
        {
            return View();
        }
        

        public IActionResult ChangePassword()
        {
            return View();
        }

        public IActionResult OTS()
        {
            return View();
        }

        public IActionResult DefaultOperator()
        {
            return View();
        }
        
        public IActionResult DefaultPrinter()
        {
            return View();
        }
        

        public IActionResult DuplicateBillInfo()
        {
            string eBillInfo = HttpContext.Session.GetString("billinfo");
            return Content(eBillInfo);
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
            ViewData["Razor"] = StaticData.CheckTopupServiceIsDown("Razor");
            ViewData["SabPaisa"] = StaticData.CheckTopupServiceIsDown("SabPaisa");
            return View();
        }
        
        

        public IActionResult AccountTopupReport()
        {
            return View();
        }

        public IActionResult FundRequest()
        {
            return View();
        }

        public IActionResult TransferValidate(string did, string amt, string rem, string ac)
        {
            RetailUser ru = new RetailUser();
            string result = "";
            try
            {
                string tranType = StaticData.ConvertHexToString(ac); // cr or dr
                string requestIp = HttpContext.Connection.RemoteIpAddress.ToString();
                string requestMachine = HttpContext.Request.Headers["User-Agent"].ToString();
                int retailUserOrderNo = Convert.ToInt32(StaticData.ConvertHexToString(did));
                decimal amount = Convert.ToDecimal(StaticData.ConvertHexToString(amt));
                string loggedInUserId = HttpContext.Session.GetString("RetailerId").ToString();
                int loggedInUserOrderNo = (int)HttpContext.Session.GetInt32("RetailUserOrderNo");
                string remarks = String.IsNullOrEmpty(rem) ? "" : StaticData.ConvertHexToString(rem);

                if (tranType == "cr")
                {
                    //Transfer to downline
                    result = ru.TransferFundToDownline(loggedInUserId, loggedInUserOrderNo, retailUserOrderNo, amount, remarks, requestIp, requestMachine, null, null, null, DateTime.UtcNow.AddHours(5.5), 1).OperationMessage;
                }

            }
            catch (Exception e)
            {
                return Content("Errors: Error in validating data." + e.Message);
            }
            finally
            {
                ru = null;
            }

            return Content(result);
        }

        public IActionResult FundRequestValidate(string did, string amt, string rem, string ac)
        {
            RetailUser ru = new RetailUser();
            string result = "";
            try
            {
                string tranType = StaticData.ConvertHexToString(ac); // cr or dr
                string requestIp = HttpContext.Connection.RemoteIpAddress.ToString();
                string requestMachine = HttpContext.Request.Headers["User-Agent"].ToString();
                int retailUserOrderNo = Convert.ToInt32(StaticData.ConvertHexToString(did));
                decimal amount = Convert.ToDecimal(StaticData.ConvertHexToString(amt));
                string loggedInUserId = HttpContext.Session.GetString("RetailerId").ToString();
                int loggedInUserOrderNo = (int)HttpContext.Session.GetInt32("RetailUserOrderNo");
                string remarks = String.IsNullOrEmpty(rem) ? "" : StaticData.ConvertHexToString(rem);

                if (tranType == "dr")
                {
                    var fundRequest = ru.FundRequest(loggedInUserId, loggedInUserOrderNo, amount, remarks, requestIp, requestMachine, null, null, null, DateTime.UtcNow.AddHours(5.5), 1);
                    result = fundRequest.OperationMessage;

                    if (!string.IsNullOrEmpty(fundRequest.RecordIdPrimary))
                    {
                        //Push to UPPPCL, then create transaction in uppcl tran
                        UPPCLLibrary.RTran rTran = new UPPCLLibrary.RTran();
                        rTran.Id = fundRequest.RecordIdPrimary;

                        result += ". UPPCL Transfer Status - " + rTran.TransferFundOnUPPCL();
                    }
                }

                

            }
            catch (Exception e)
            {
                return Content("Errors: Error in validating data." + e.Message);
            }
            finally
            {
                ru = null;
            }

            return Content(result);
        }

        [AllowAnonymous] // 🚀 important
        [IgnoreAntiforgeryToken]
        public IActionResult SabPaisaCallback()
        {
            try
            {
                string query = Request.Form["encResponse"];
                TempData["encResponse"] = query;
                return Redirect(Url.Action(action: "SabPaisaCallback1", controller: "RetailClient"));
            }
            catch
            {
                return Redirect(Url.Action(action: "Index", controller: "RetailClient"));
            }
        }

        public async Task<IActionResult> SabPaisaCallback1()
        {
            var query = TempData["encResponse"] as string;
            if (string.IsNullOrEmpty(query))
            {
                return Redirect(Url.Action(action: "Index", controller: "RetailClient"));
            }
            string result = string.Empty;
            try
            {
                var requestIp = HttpContext.Connection.RemoteIpAddress.ToString();
                var requestMachine = HttpContext.Request.Headers["User-Agent"].ToString();
                var orderNo = (int)HttpContext.Session.GetInt32("RetailUserOrderNo");
                var verified = await SabPaisaHelper.PostOrder(_sabPaisaService, query, requestIp, requestMachine, orderNo);
                return View("SabPaisaCallback", verified);
            }
            finally
            {
                TempData["encResponse"] = "";
            }
        }
    }
}
