﻿using System;
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
using UPPCLLibrary.OTS;
using Microsoft.VisualBasic;
using QRCoder;

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

        public IActionResult PrintOTSReceiptThermal(string t)
        {
            //PaymentReceipt pr = new PaymentReceipt();
            //pr.TelecomOperatorName = "Test";
            string tranId = "";
            try
            {
                string result = string.Empty;
                tranId = StaticData.ConvertHexToString(t);
                var uppclLReceipt = StaticData.PaymentOTSReceiptDataByTranId(tranId);
                var obj = JsonConvert.DeserializeObject<CaseInitResponse>(StaticData.GetApiResponseByApiTypeAndConsumerId(uppclLReceipt.AccountId, "OTS_CaseInit", tranId));
                var obj1 = JsonConvert.DeserializeObject<AmountDetails>(StaticData.GetApiResponseByApiTypeAndConsumerId(uppclLReceipt.AccountId, "OTS_AmountDetails", tranId));
                //PaymentReceipt pr = JsonConvert.DeserializeObject<PaymentReceipt>(StaticData.ReceiptByTranId(tranId));
                
                if (obj != null && obj1 != null)
                {
                    decimal downPayment;
                    if (uppclLReceipt.IsFull == 1)
                    {
                        var diff = Convert.ToDecimal(uppclLReceipt.Amount) - obj1.Data.FullPaymentList[0].RegistrationAmount;
                        downPayment = obj1.Data.FullPaymentList[0].Downpayment - diff;
                        if (downPayment < 0)
                        {
                            downPayment = 0;
                        }
                    }
                    else
                    {
                        downPayment = obj1.Data.InstallmentList1[0].Downpayment;
                    }
                    var dueAmount = (int)Math.Round(Convert.ToDecimal(obj.Data.BillDetails.AccountInfo), MidpointRounding.AwayFromZero);
                    result = "\n[C]<u><font size='big'>UPPCL BILL PAY" + "</font></u>\n" +
                             "[C]================================\n" +
                             "[L]<font size='tall'><b>" + obj.Data.BillDetails.Discom + "</b></font>\n" +
                             "[C]--------------------------------\n" +
                             "[C]<barcode type='128' height='10'>" + uppclLReceipt.LiveId +
                             "</barcode>\n" +
                             "[C]================================\n" +




                             "[L]<b>RECEIPT NO: </b>" + uppclLReceipt.LiveId + "\n" +
                             "[L]<b>BILL DATE: </b>" + obj.Data.BillDetails.BillDate + "\n" +
                             "[L]<b>DUE DATE: </b>" + obj.Data.BillDetails.BillDueDate + "\n" +
                             "[L]<b>PAYMENT DATE: </b>" + uppclLReceipt.CreateDate.ToString("dd-MM-yyyy HH:mm") + "\n" +
                             "[C]--------------------------------\n" +
                             "[L]<b>DIVISION: </b>" + obj.Data.BillDetails.Division + "\n" +
                             "[L]<b>SUB DIVISION: </b>" + obj.Data.BillDetails.SubDivision + "\n" +
                             "[L]<b>MOBILE: </b>" + obj.Data.BillDetails.MobileNumber + "\n" +
                             "[L]<b>ACCOUNT NO: </b>" + uppclLReceipt.AccountId + "\n" +
                             "[L]<b>CONSUMER NAME: " + obj.Data.BillDetails.ConsumerName + "</b>\n" +
                             "[L]<b>BILL AMOUNT: </b>Rs. " + obj.Data.BillDetails.BillAmount + "\n" +
                             "[L]<b>OUTSTANDING AMOUNT: </b>Rs. " + dueAmount + "\n" +
                             "[L]<b>PAID AMOUNT: </b>Rs. " + uppclLReceipt.Amount + "\n" +
                             "[L]<b>BALANCE AMOUNT: </b>Rs. " + downPayment + "\n" +
                             "[L]<b>(" + StaticData.AmountToInr(Convert.ToDouble(uppclLReceipt.Amount)) + " Only" + ")</b>\n" +
                             "[L]<b>STATUS: </b>" + uppclLReceipt.RechargeStatus + "\n" +
                             "[L]<b>PAYMENT TYPE: </b>WALLET PAYMENT" + "\n" +

                             "[L]\n" +
                             "[C]--------------------------------\n" +
                             "[R]<b>TOTAL AMOUNT : " + uppclLReceipt.Amount + "</b>\n" +
                             "[R]DATE : " + uppclLReceipt.CreateDate.ToString("dd-MM-yyyy HH:mm") + "\n" +
                             //"[R]PART PAYMENT : " + (pr.Parameter1 == "PartPayment" ? "YES" : "NO") + "\n" +
                             "[L]\n" +
                             "[C]================================\n" +
                             "[L]<b>Printed By: </b>" + uppclLReceipt.RetailerName.ToUpper() + "\n" +
                             "[L]<u><b>Printed On: </b>" + DateTime.Now.ToString("dd-MM-yyyy HH:mm") + "</u>\n" +
                             "[L]<b>Agency: </b>" + "Saral ECommerce Pvt Ltd" + "\n" +
                             "[L]<b>Agent Id: </b>" + uppclLReceipt.RetailUserId + "\n" +
                             "[L]\n" + uppclLReceipt.ReceiptMessage + "\n" +
                             "[L]This is computer generated receipt, does not require signature.\n" +
                             "[L]Your bill will be updated within 1 days from the date of payment received. .\n" +

                             "[C]\n<qrcode size='20'>http://saralesuvidha.com/Home/ReceiptOTSUPPCL?t=" + StaticData.ConvertStringToHex(tranId) +
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
        
        

        public IActionResult AccountTopupReport()
        {
            return View();
        }

        public IActionResult CheckEligibilityForOTS(string accountId, string discomId)
        {
            var obj = StaticData.CheckEligibilityForOTS(accountId, discomId);            
            return Content(JsonConvert.SerializeObject(obj));
        }

        public IActionResult GetAmountDetailsForOTS(string accountId, string discomId)
        {
            UPPCLManager.Initialize();
            UPPCLManager.CheckTokenExpiry();
            var obj = UPPCLManager.GetAmountDetails(discomId, accountId);
            var amountdetail = JsonConvert.SerializeObject(obj);
            return Content(JsonConvert.SerializeObject(obj));
        }

        public IActionResult InitiateOTSCase(string accountId, string discomId, int isFull, string amount, string downPayment, string registrationAmount)
        {
            if(isFull == 1)
            {
                var totalAmount = Convert.ToDecimal(registrationAmount);
                if (amount == null)
                {
                    var caseInitResponse = new CaseInitResponse
                    {
                        Data = null,
                        Message = $"Input amount can not be blank and must be equal or greater than Registation Payable Amount (पंजीकरण देय राशि)  ({totalAmount})",
                        Status = "error"
                    };
                    return Content(JsonConvert.SerializeObject(caseInitResponse));
                }
                else
                {
                    var val = Convert.ToDecimal(amount);
                    
                    if (val < (totalAmount))
                    {
                        var caseInitResponse = new CaseInitResponse
                        {
                            Data = null,
                            Message = $"Input amount can not be less than Registation Payable Amount (पंजीकरण देय राशि)  ({totalAmount})",
                            Status = "error"
                        };
                        return Content(JsonConvert.SerializeObject(caseInitResponse));
                    }
                }
            }
            UPPCLManager.Initialize();
            UPPCLManager.CheckTokenExpiry();
            var obj = UPPCLManager.InitiateOTSCase(discomId, accountId, isFull == 1 ? true : false, amount);
            var ots = JsonConvert.SerializeObject(obj);
            return Content(ots);
        }

        public IActionResult SubmitOTSCase(string accountId, string discomId, string amount, int isFull, string pi = "")
        {
            string result = string.Empty;
            string retailerId = HttpContext.Session.GetString("RetailerId");
            string retailUserOrderNo = HttpContext.Session.GetInt32("RetailUserOrderNo").ToString();
            string userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            string requestIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            //string retailerId, string eBillInfo, string retailUserOrderNo, string retailUserName, string userAgent, string requestIp
            var obj = JsonConvert.DeserializeObject<CaseInitResponse>(StaticData.GetApiResponseByApiTypeAndConsumerId(accountId, "OTS_CaseInit"));
            var obj1 = JsonConvert.DeserializeObject<AmountDetails>(StaticData.GetApiResponseByApiTypeAndConsumerId(accountId, "OTS_AmountDetails"));
            decimal amount1;
            if (isFull == 1)
            {
                amount1 = Convert.ToDecimal(amount);
            }
            else
            {
                amount1 = Convert.ToDecimal(obj1.Data.InstallmentList1[0].RegistrationAmount);
            }
            UPPCLManager.Initialize();
            var res = StaticData.PayOTSUPPCL(discomId, accountId, retailerId, retailUserOrderNo, requestIp, obj, userAgent, amount1, obj1.Data.TotoalOutStandingAmount, isFull, pi);
            result = res.Item1;
            result += "$$" + res.Item2;
            return Content(result);
        }

        public IActionResult ReceiptOTSUPPCL(string accountId, string discomId, string amount, int isFull, string reciptId)
        {
            var tranId = StaticData.ConvertHexToString(reciptId);
            UPPCLOTSReciptModal modal = OTSReciptGenerator.GenerateOTSRecipt(accountId, amount, isFull, tranId);

            return View(modal);
        }
    }
}
