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
using UPPCLLibrary.OTS;
using Microsoft.VisualBasic;

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
            UPPCLManager.CheckTokenExpiry();
            var obj = UPPCLManager.CheckEligibility(discomId, accountId);
            
            return Content(JsonConvert.SerializeObject(obj));
        }

        public IActionResult GetAmountDetailsForOTS(string accountId, string discomId)
        {
            UPPCLManager.CheckTokenExpiry();
            var obj = UPPCLManager.GetAmountDetails(discomId, accountId);
            var amountdetail = JsonConvert.SerializeObject(obj);
            HttpContext.Session.SetString("amountdetail", amountdetail);
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
            
            UPPCLManager.CheckTokenExpiry();
            var obj = UPPCLManager.InitiateOTSCase(discomId, accountId, isFull == 1 ? true : false, amount);
            var ots = JsonConvert.SerializeObject(obj);
            HttpContext.Session.SetString("otsinfo", ots);
            return Content(ots);
        }

        public IActionResult SubmitOTSCase(string accountId, string discomId, string amount, int isFull)
        {
            string result = string.Empty;
            string retailerId = HttpContext.Session.GetString("RetailerId");
            string retailUserOrderNo = HttpContext.Session.GetInt32("RetailUserOrderNo").ToString();
            string userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            string requestIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            //string retailerId, string eBillInfo, string retailUserOrderNo, string retailUserName, string userAgent, string requestIp
            var obj = JsonConvert.DeserializeObject<CaseInitResponse>(HttpContext.Session.GetString("otsinfo"));
            var obj1 = JsonConvert.DeserializeObject<AmountDetails>(HttpContext.Session.GetString("amountdetail"));
            decimal amount1;
            if (isFull == 1)
            {
                amount1 = Convert.ToDecimal(amount);
            }
            else
            {
                amount1 = Convert.ToDecimal(obj1.Data.InstallmentList1[0].RegistrationAmount);
            }
            HttpContext.Session.SetString("isFull", isFull.ToString());
            result = StaticData.PayOTSUPPCL(discomId, accountId, retailerId, retailUserOrderNo, requestIp, obj, userAgent, amount1, obj1.Data.TotoalOutStandingAmount);
            return Content(result);
        }

        public IActionResult ReceiptOTSUPPCL(string accountId, string discomId, string amount, int isFull)
        {
            var obj = JsonConvert.DeserializeObject<CaseInitResponse>(HttpContext.Session.GetString("otsinfo"));
            var obj1 = JsonConvert.DeserializeObject<AmountDetails>(HttpContext.Session.GetString("amountdetail"));
            string discomName = "";
            discomName = StaticData.UPPCLOperatorName(discomId);
            var append = string.Empty;
            if(obj.Data.BillDetails.PurposeOfSupply == "LMV1")
            {
                obj.Data.BillDetails.PurposeOfSupply += " (घरेलू)";
            }
            if (obj.Data.BillDetails.PurposeOfSupply == "LMV2")
            {
                obj.Data.BillDetails.PurposeOfSupply += " (वाणिज्य)";
            }
            if (obj.Data.BillDetails.PurposeOfSupply == "LMV4")
            {
                obj.Data.BillDetails.PurposeOfSupply += " (निजी संस्थान)";
            }
            if (obj.Data.BillDetails.PurposeOfSupply == "LMV6")
            {
                obj.Data.BillDetails.PurposeOfSupply += " (औद्योगिक)";
            }
            var modal = new UPPCLOTSReciptModal();
            modal.RechargeMobileNumber = obj.Data.BillDetails.ConsumerName;
            modal.TelecomOperatorName = discomName;
            modal.ApiOperatorCode = discomId.Trim();

            modal.InfoTable = "<table class='table'>";
            modal.InfoTable += "<tr class='table'>";
            modal.InfoTable += "<td class='table'>";
            modal.InfoTable += "श्रीमती/श्री";
            modal.InfoTable += "</td>";
            modal.InfoTable += "<td class='table'>";
            modal.InfoTable += obj.Data.BillDetails.ConsumerName;
            modal.InfoTable += "</td>";
            modal.InfoTable += "</tr>";
            modal.InfoTable += "<tr class='table'>";
            modal.InfoTable += "<td class='table'>";
            modal.InfoTable += "अकाउंट आईडी";
            modal.InfoTable += "</td>";
            modal.InfoTable += "<td class='table'>";
            modal.InfoTable += obj.Data.BillDetails.KNumber;
            modal.InfoTable += "</td>";
            modal.InfoTable += "</tr>";
            modal.InfoTable += "<tr class='table'>";
            modal.InfoTable += "<td class='table'>";
            modal.InfoTable += "विधा";
            modal.InfoTable += "</td>";
            modal.InfoTable += "<td class='table'>";
            modal.InfoTable += obj.Data.BillDetails.PurposeOfSupply;
            modal.InfoTable += "</td>";
            modal.InfoTable += "</tr>";
            modal.InfoTable += "<tr class='table'>";
            modal.InfoTable += "<td class='table'>";
            modal.InfoTable += "स्वीकृत भार";
            modal.InfoTable += "</td>";
            modal.InfoTable += "<td class='table'>";
            modal.InfoTable += obj.Data.BillDetails.SanctionedLoadInKW + " KW";
            modal.InfoTable += "</td>";
            modal.InfoTable += "</tr>";
            if (isFull == 1)
            {
                modal.Para1 = $"आपके द्वारा OTS योजना के अंतर्गत दिनांक {DateTime.Now.ToString("dd-MM-yyyy")} को पंजीकरण किया गया है। इस योजना के अंतर्गत अधिकतम <Waiver LPSC> की छूट प्राप्त करने के लिये शेष बकाया धनराशि रू. <Remaining Principal Amount> का भुगतान दिनांक {DateTime.Now.AddDays(30).ToString("dd-MM-yyyy")} तक विभागीय खण्ड/उपखण्ड कार्यालय/कैश काउन्टर, जनसेवा केन्द्र, विद्युत सखी, फिनटेक प्रतिनिधि अथवा मीटर रीडर (बिलिगं एजेन्सी) अथवा UPPCL वेबसाइट (uppcl.org) के माध्यम से किया जा सकेगा";

                modal.InfoTable += "<tr class='table'>";
                modal.InfoTable += "<td class='table'>";
                modal.InfoTable += "चयनित विकल्प";
                modal.InfoTable += "</td>";
                modal.InfoTable += "<td class='table'>";
                modal.InfoTable += "एकमुश्त";
                modal.InfoTable += "</td>";
                modal.InfoTable += "</tr>";

            }
            else
            {
                modal.Para1 = $"आपके द्वारा OTS योजना के अंतर्गत दिनांक {DateTime.Now.ToString("dd-MM-yyyy")} को पंजीकरण किया गया है। इस योजना के अंतर्गत अधिकतम <Waiver LPSC> की छूट प्राप्त करने के लिये शेष बकाया धनराशि रू. <Remaining Principal Amount> का भुगतान निम्नांकित किश्तों में अपने मासिक विद्युत बिल के साथ विभागीय खण्ड/उपखण्ड कार्यालय/कैश काउन्टर, जनसेवा केन्द्र, विद्युत सखी, फिनटेक प्रतिनिधि अथवा मीटर रीडर (बिलिगं एजेन्सी) अथवा UPPCL वेबसाइट (uppcl.org) के माध्यम से किया जा सकेगा";
                modal.InfoTable += "<tr class='table'>";
                modal.InfoTable += "<td class='table'>";
                modal.InfoTable += "चयनित विकल्प";
                modal.InfoTable += "</td>";
                modal.InfoTable += "<td class='table'>";
                modal.InfoTable += "किश्त";
                modal.InfoTable += "</td>";
                modal.InfoTable += "</tr>";

                modal.InstallmentTable = "<table class='table'>";
                
                var months = new string[12]
                {
                    "जनवरी 2025",
                    "फ़रवरी 2025",
                    "मार्च 2025",
                    "अप्रैल 2025",
                    "मई 2025",
                    "जून 2025",
                    "जुलाई 2025",
                    "अगस्त 2025",
                    "सितम्बर 2025",
                    "अक्टूबर 2025",
                    "नवंबर 2025",
                    "दिसंबर 2025"
                };
                var installmet = new string[12]
                {
                    "पहली",
                    "दूसरी",
                    "तीसरी",
                    "चौथी",
                    "पांचवी",
                    "छटवी",
                    "सातवी",
                    "आठवी",
                    "नवमी",
                    "दसवी",
                    "ग्यारवी",
                    "बारहवीं"
                };
                modal.InstallmentTable += "<tr class='table'>";
                modal.InstallmentTable += "<td class='table'>";
                modal.InstallmentTable += "क्र.";
                modal.InstallmentTable += "</td>";
                modal.InstallmentTable += "<td class='table'>";
                modal.InstallmentTable += "किश्त धनराशि (रू.)";
                modal.InstallmentTable += "</td>";
                modal.InstallmentTable += "<td class='table'>";
                modal.InstallmentTable += "माह";
                modal.InstallmentTable += "</td>";
                modal.InstallmentTable += "</tr>";
                for (int i =0; i< obj1.Data.InstallmentList1[0].NoOfInstallments; i++)
                {
                    if(i < 12)
                    {
                        modal.InstallmentTable += "<tr class='table'>";
                        modal.InstallmentTable += "<td class='table'>";
                        modal.InstallmentTable += installmet[i] + " किश्त";
                        modal.InstallmentTable += "</td>";
                        modal.InstallmentTable += "<td class='table'>";
                        modal.InstallmentTable += obj1.Data.InstallmentList1[0].InstallmentAmount;
                        modal.InstallmentTable += "</td>";
                        modal.InstallmentTable += "<td class='table'>";
                        modal.InstallmentTable += months[i];
                        modal.InstallmentTable += "</td>";
                        modal.InstallmentTable += "</tr>";
                    }
                }
                modal.InstallmentTable += "</table>";
            }
            modal.InfoTable += "<tr class='table'>";
            modal.InfoTable += "<td class='table'>";
            modal.InfoTable += "मूल बकाया";
            modal.InfoTable += "</td>";
            modal.InfoTable += "<td class='table'>";
            modal.InfoTable += obj1.Data.Payment31;
            modal.InfoTable += "</td>";
            modal.InfoTable += "</tr>";
            modal.InfoTable += "<tr class='table'>";
            modal.InfoTable += "<td class='table'>";
            modal.InfoTable += "ब्याज";
            modal.InfoTable += "</td>";
            modal.InfoTable += "<td class='table'>";
            modal.InfoTable += "";
            modal.InfoTable += "</td>";
            modal.InfoTable += "</tr>";
            modal.InfoTable += "<tr class='table'>";
            modal.InfoTable += "<td class='table'>";
            modal.InfoTable += "माफ़ी योग्य अधिकतम ब्याज";
            modal.InfoTable += "</td>";
            modal.InfoTable += "<td class='table'>";
            modal.InfoTable += "";
            modal.InfoTable += "</td>";
            modal.InfoTable += "</tr>";
            modal.InfoTable += "<tr class='table'>";
            modal.InfoTable += "<td class='table'>";
            modal.InfoTable += "पंजीकरण धनराशि प्राप्त";
            modal.InfoTable += "</td>";
            modal.InfoTable += "<td class='table'>";
            modal.InfoTable += isFull == 1 ? amount : obj1.Data.InstallmentList1[0].RegistrationAmount;
            modal.InfoTable += "</td>";
            modal.InfoTable += "</tr>";
            modal.InfoTable += "</table>";

            
            return View(modal);
        }
    }
}
