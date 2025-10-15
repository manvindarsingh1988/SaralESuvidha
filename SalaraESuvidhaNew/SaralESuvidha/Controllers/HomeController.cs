using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SaralESuvidha.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SaralESuvidha.Models;
using SaralESuvidha.ViewModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Newtonsoft.Json;
using QRCoder;
using DocumentFormat.OpenXml.Wordprocessing;
//using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using Dapper;
using System.Data.SqlClient;
using Razorpay.Api;
using OfficeOpenXml.FormulaParsing.Excel.Functions;

namespace SaralESuvidha.Controllers
{
    [HomePageFilter]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RechargeBillDTH()
        {
            return View();
        }

        public async Task<IActionResult> BM()
        {
            var results = new List<RetailUserUPPCLBalance>();
            using (var connection = new SqlConnection(StaticData.conString))
            {
                await connection.OpenAsync();
                var query = @"SELECT Id, OrderNo AS USL, FirstName, Mobile, UPPCL_AgentVAN AS VANId, UPPCL_Balance, DATEDIFF(MINUTE,UPPCL_BalanceTime,GETDATE()) AS BalanceTime
                        FROM RetailUser WITH(NOLOCK)
                        WHERE UPPCL_AgentVAN IS NOT NULL AND UPPCL_Balance IS NOT NULL AND UPPCL_Balance > 10 AND DATEDIFF(MINUTE,UPPCL_BalanceTime,GETDATE()) > 5
                        ORDER BY UPPCL_Balance DESC, OrderNo ASC";
                using (var command = new SqlCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        results.Add(new RetailUserUPPCLBalance
                        {
                            Id = reader.GetString(0),
                            USL = reader.GetInt64(1),
                            FirstName = reader.GetString(2),
                            Mobile = reader.GetString(3),
                            VANId = reader.GetString(4),
                            UPPCL_Balance = reader.GetDecimal(5),
                            BalanceTime = reader.GetInt32(6)
                        });
                    }
                }
            }
            return View(results);
        }

        public IActionResult DataCardRecharge()
        {
            return View();
        }

        public IActionResult CableTVRecharge()
        {
            return View();
        }

        public IActionResult ElectricityRecharge()
        {
            return View();
        }

        public IActionResult MetroRecharge()
        {
            return View();
        }

        public IActionResult GasRecharge()
        {
            return View();
        }

        public IActionResult WaterRecharge()
        {
            return View();
        }

        public IActionResult LandlineRecharge()
        {
            return View();
        }

        public IActionResult IndexOld()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult BroadbandRecharge()
        {
            return View();
        }
        
        public IActionResult RefundCancellation()
        {
            return View();
        }
        public IActionResult Terms()
        {
            return View();
        }
        public IActionResult PrivacyPolicy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //[ValidateAntiForgeryToken]
        public IActionResult StateList(int id)
        {
            return Content(StaticData.StateListJson(id));
        }

        //[ValidateAntiForgeryToken]
        public IActionResult CityList(int id)
        {
            //Thread.Sleep(5000);
            return Content(StaticData.CitiesListJson(id));
        }

        public IActionResult OperatorListForMargin()
        {
            return Content(StaticData.OperatorListJson());
        }
        
        public IActionResult MonitorLogin()
        {
            return View();
        }
        
        public IActionResult TranLog()
        {
            if (HttpContext.Session.GetString("monitor") != null && HttpContext.Session.GetString("monitor") == "9415004756")
            {
                return Content(StaticData.TranListJson());
            }
            else
            {
                return Content("Invalid request");
            }
        }
        
        public IActionResult TranMonitor()
        {
            if (HttpContext.Session.GetString("monitor") != null && HttpContext.Session.GetString("monitor") == "9415004756")
            {
                return View();
            }
            else
            {
                return Content("Invalid request");
            }
        }
        
        public IActionResult WalletBalance()
        {
            if (HttpContext.Session.GetString("monitor") != null && HttpContext.Session.GetString("monitor") == "9415004756")
            {
                return Content(StaticData.WalletBalanceJson());
            }
            else
            {
                return Content("Invalid request");
            }
        }

        public IActionResult RetailLogin(string m, string p, string s="w", string f="", string d="")
        {
            string[] result = new string[3];
            try
            {
                if (m.Length == 10 && p.Length > 5)
                {
                    try
                    {
                        //f = HttpContext.Session.GetString("f");
                        /*
                        if (HttpContext.Session.GetString("f") != null)
                        {
                            
                        }*/
                    }
                    catch (Exception)
                    {
                        
                    }
                    
                    var user = StaticData.ValidateRetailUserLogin(m, p, f, d);
                    var retailUser = user.Item1;
                    if (retailUser != null)
                    {
                        if (retailUser.USL > 0)
                        {
                            
                            HttpContext.Session.SetInt32("RetailUserOrderNo", (int)retailUser.USL);
                            HttpContext.Session.SetString("RetailMobile", retailUser.MobileNumber);
                            HttpContext.Session.SetString("RetailerName", retailUser.RetailerName);
                            HttpContext.Session.SetString("RetailerId", retailUser.Id);
                            HttpContext.Session.SetInt32("RetailerType", retailUser.UserType);
                            HttpContext.Session.SetString("ApiEnabled", retailUser.ApiEnabled.ToString());
                            HttpContext.Session.SetString("DefaultUtilityOperator", string.IsNullOrEmpty(retailUser.DefaultUtilityOperator) ? "MVVNL" : retailUser.DefaultUtilityOperator);
                            HttpContext.Session.SetString("DefaultPrinter", string.IsNullOrEmpty(retailUser.DefaultPrinter) ? "Normal" : retailUser.DefaultPrinter);

                            try
                            {
                                string existingPin = StaticData.RetailUserPin(retailUser.Id);
                                if (string.IsNullOrEmpty(existingPin))
                                {
                                    string newPin = StaticData.Random6DigitPINString();
                                    StaticData.UpdateRetailUserPin(retailUser.Id, newPin);
                                }
                            }catch(Exception exPin)
                            {

                            }

                            switch (s)
                            {
                                case "w":
                                    HttpContext.Session.SetString("LoginSource", "web");
                                    break;
                                case "m":
                                    HttpContext.Session.SetString("LoginSource", "mobile");
                                    break;
                            }
                            //return Redirect("~/RetailClient/Index");
                            //return RedirectToAction("Index", "RetailClient");
                            if (retailUser.UserType == 9)
                            {
                                result[0] = "Success: login ok. WhiteLabel";
                            }
                            if (retailUser.UserType == 7)
                            {
                                result[0] = "Success: login ok. MasterDistributor";
                            }
                            if (retailUser.UserType == 6)
                            {
                                result[0] = "Success: login ok. Distributor";
                            }
                            if (retailUser.UserType == 5)
                            {
                                result[0] = "Success: login ok. Retailer";
                            }
                        }
                        else
                        {
                            result[0] = "Errors: Invalid user or password.";
                        }
                    }
                    else
                    {
                        result[0] = "Errors: User not found or not active.";
                        if(user.Item2 == -1)
                        {
                            result[1] = "Account is inactive. Please contact with admin for activation.";
                        }
                    }
                    if(retailUser != null && retailUser.ActivatedTill != null)
                    {                        
                        if (DateTime.Now < retailUser.ActivatedTill.GetValueOrDefault())
                        {
                            var days = (retailUser.ActivatedTill.GetValueOrDefault() - DateTime.Now).Days;
                            result[1] = $"Account will get deactivate in {days} days as physical KYC is still pending. Please contact with admin for more details.";
                        }
                    }
                    if (retailUser != null)
                    {
                        result[2] = retailUser.AgreementAccepted != null ? retailUser.AgreementAccepted.ToString() : "0";
                    }
                }
                else
                {
                    result[0] = "Errors: Invalid login details.";
                }
            }
            catch (Exception ex)
            {
                result[0] = "Errors: Exception: " + ex.Message;
            }
            return Content(string.Join("$$",result));
        }

        public IActionResult UpdateAgreementAceptStatus()
        {
            var id = HttpContext.Session.GetString("RetailerId");
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            using (var con = new SqlConnection(StaticData.conString))
            {
                var result = con.QuerySingleOrDefault<string>("usp_UpdateAgreementStatus", parameters, commandType: System.Data.CommandType.StoredProcedure);
                return Content("status updated.");
            }
        }

        public IActionResult OfficeLogin()
        {
            return View("OfficeLogin");
        }

        public IActionResult OfficeLoginResult(string m, string p)
        {
            string result = string.Empty;
            try
            {
                if (m.Length == 10 && p.Length > 5)
                {
                    var officeUser = StaticData.ValidateOfficeLogin(m, p);
                    if (officeUser != null)
                    {
                        if (officeUser.OrderNo > 0)
                        {
                            HttpContext.Session.SetInt32("OfficeUserOrderNo", (int)officeUser.OrderNo);
                            HttpContext.Session.SetString("UserMobile", officeUser.Mobile);
                            HttpContext.Session.SetString("UserName", officeUser.UserName);
                            HttpContext.Session.SetString("UserId", officeUser.Id);
                            HttpContext.Session.SetInt32("UserType", officeUser.UserType); // 8= Admin, 1=CustomerSupport, 2=Account
                            //return Redirect("/RetailClient/Index");
                            //return RedirectToAction("Index", "RetailClient");
                            string usr = "";
                            switch (officeUser.UserType)
                            {
                                case 8:
                                    //Redirect("SysAdmin");
                                    usr = "ad";
                                    break;

                                case 1:
                                    //Redirect("CustomerSupport");
                                    usr = "cs";
                                    break;
                                case 9:
                                    //Redirect("SecondaryAdmin");
                                    usr = "sadm";
                                    break;

                                default:
                                    break;

                            }
                            HttpContext.Session.SetString("usr", usr);
                            result = "Success: login ok. " + usr;

                        }
                        else
                        {
                            result = "Errors: Invalid user or password.";
                        }
                    }
                    else
                    {
                        result = "Errors: User not found or not active.";
                    }
                }
                else
                {
                    result = "Errors: Invalid login details.";
                }
            }
            catch (Exception ex)
            {
                result = "Errors: Exception: " + ex.Message;
            }
            return Content(result);
        }
        
        public IActionResult MonitorLoginResult(string m, string p)
        {
            string result = string.Empty;
            try
            {
                if (m.Length == 10 && p.Length > 5)
                {
                    if (m == "9415004756" && p == "9651788888")
                    {
                        HttpContext.Session.SetString("monitor", "9415004756");
                        result = "Success: login ok. " + m;
                        
                    }
                    else
                    {
                        result = "Errors: Invalid user or password.";
                    }
                }
                else
                {
                    result = "Errors: Invalid login details.";
                }
            }
            catch (Exception ex)
            {
                result = "Errors: Exception: " + ex.Message;
            }
            return Content(result);
        }
        
        public IActionResult ReceiptUPPCL(string t)
        {
            string tranId = "";
            try
            {
                tranId = StaticData.ConvertHexToString(t);
                var UPPCLReceipt = StaticData.PaymentReceiptUPPCLByTranId(tranId);
                try
                {
                    string verifyUrl = "http://saralesuvidha.com/Home/ReceiptUPPCL?t=" + t;//VerifyReceipt
                    QRCodeGenerator QrGenerator = new QRCodeGenerator();
                    QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(verifyUrl, QRCodeGenerator.ECCLevel.Q);
                    QRCoder.Base64QRCode qr = new Base64QRCode();
                    qr.SetQRCodeData(QrCodeInfo);
                    UPPCLReceipt.QrCode = "data:image/png;base64," + qr.GetGraphic(20);
                }
                catch (Exception)
                {
                    
                }
                return View(UPPCLReceipt);

            }
            catch (Exception)
            {
                return View(new PaymentReceiptUPPCL() { TelecomOperatorName = "INVALID DETAILS", Amount = 0 });
            }
        }
        
        public IActionResult ReceiptUPPCLT(string t)
        {
            string tranId = "";
            try
            {
                tranId = StaticData.ConvertHexToString(t);
                var UPPCLReceipt = StaticData.PaymentReceiptUPPCLByTranId(tranId);
                try
                {
                    string verifyUrl = "http://saralesuvidha.com/Home/ReceiptUPPCL?t=" + t;//VerifyReceipt
                    QRCodeGenerator QrGenerator = new QRCodeGenerator();
                    QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(verifyUrl, QRCodeGenerator.ECCLevel.Q);
                    QRCoder.Base64QRCode qr = new Base64QRCode();
                    qr.SetQRCodeData(QrCodeInfo);
                    UPPCLReceipt.QrCode = "data:image/png;base64," + qr.GetGraphic(20);
                }
                catch (Exception)
                {
                    
                }
                return View(UPPCLReceipt);

            }
            catch (Exception)
            {
                return View(new PaymentReceiptUPPCL() { TelecomOperatorName = "INVALID DETAILS", Amount = 0 });
            }
        }
        
        public IActionResult ReceiptUPPCLTL(string t)
        {
            string tranId = "";
            try
            {
                tranId = StaticData.ConvertHexToString(t);
                var UPPCLReceipt = StaticData.PaymentReceiptUPPCLByTranId(tranId);
                try
                {
                    string verifyUrl = "http://saralesuvidha.com/Home/ReceiptUPPCL?t=" + t;//VerifyReceipt
                    QRCodeGenerator QrGenerator = new QRCodeGenerator();
                    QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(verifyUrl, QRCodeGenerator.ECCLevel.Q);
                    QRCoder.Base64QRCode qr = new Base64QRCode();
                    qr.SetQRCodeData(QrCodeInfo);
                    UPPCLReceipt.QrCode = "data:image/png;base64," + qr.GetGraphic(20);
                }
                catch (Exception)
                {
                    
                }
                return View(UPPCLReceipt);

            }
            catch (Exception)
            {
                return View(new PaymentReceiptUPPCL() { TelecomOperatorName = "INVALID DETAILS", Amount = 0 });
            }
        }
        
        

        public IActionResult VerifyReceipt(string t)
        {
            string tranId = "";
            try
            {
                tranId = StaticData.ConvertHexToString(t);
                return View(StaticData.PaymentReceiptByTranId(tranId));

            }
            catch (Exception)
            {
                return View(new PaymentReceipt() {TelecomOperatorName = "INVALID DETAILS", Amount = 0});
            }
        }

        public IActionResult Test()
        {
            try
            {
                //return Content(UPPCLLibrary.UPPCLManager.DiscomList());
                /*
                string pin = StaticData.RandomNumberString().Substring(0,6);
                string enc = StaticData.EncodeBase64(System.Text.Encoding.Unicode, PSFTCrypto.Encrypt(pin));
                return Content(pin + Environment.NewLine + enc + Environment.NewLine + PSFTCrypto.Decrypt(StaticData.DecodeBase64(System.Text.Encoding.Unicode,enc)));
                */

                string pin = StaticData.Random6DigitPINString();
                string enc = StaticData.EncodePIN(pin);
                return Content(pin + Environment.NewLine + enc + Environment.NewLine + StaticData.DecodePIN(enc));

            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }
        
        
        

        public IActionResult rzcb()
        {
            return View();
        }

        public IActionResult AccountTopup()
        {
            return View();
        }
        
        public IActionResult RMonitor()
        {
            return View();
        }
        
        public IActionResult RMonitorLog(string a, int dt=0)
        {
            return Content(StaticData.RMonitorListJson(a, dt));
        }

        public IActionResult TestWeb()
        {
            string testData = @"{transactionId='EW1406950259387199488', billId='415047620197', consumerId='4151520000', amount=52.0, commissionAmount=5.94, areaType='IPDS', paymentType='FULL', agencyId='0df7d59bfb554e748b2b4477be379ed3', agentVan='UPCA7488547690', empId='RU001626', connectionType='Postpaid', scheme='null', transactionTime='2025-08-18 16:08:01'}";
            return Content(StaticData.ComputeHmacSha256(testData, StaticData.webhookSharedSecret));
        }



        //[HttpPost]
        [Route("webuppcl")]
        public async Task<IActionResult> ReceiveCommission([FromHeader(Name = "X-Signature")] string signature)
        {
            //[FromBody] CommissionPayload payload, 
            // Enable rewinding of the request body
            HttpContext.Request.EnableBuffering();

            // Read the raw request body as a string
            string rawBody;
            using (var reader = new StreamReader(HttpContext.Request.Body,encoding: System.Text.Encoding.UTF8,detectEncodingFromByteOrderMarks: false,leaveOpen: true))
            {
                rawBody = await reader.ReadToEndAsync();
                HttpContext.Request.Body.Position = 0;
            }

            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            string responseMessage = "";

            WebhookLog wLog = new WebhookLog();
            wLog.RequestIp = ipAddress;
            wLog.RequestSignature = signature;
            wLog.RequestData = rawBody;// System.Text.Json.JsonSerializer.Serialize(payload).ToString();//rawBody;
            wLog.CreateDate = DateTime.Now;
            wLog.IsMatch = false;
            wLog.IsAlreadyReceived = false;

            //CommissionPayload payload = System.Text.Json.JsonSerializer.Serialize(payload).ToString();
            CommissionPayload payload = JsonConvert.DeserializeObject<CommissionPayload>(rawBody);

            try
            {
                wLog.EWId = StaticData.GetRegExFirstMatch(wLog.RequestData, "transactionId\":\"(.*)\",\"bill");
            }
            catch (Exception ex)
            {

            }

            if (wLog.CheckLogInDb())
            {
                responseMessage = "{\"status\":\"SUCCESS\",\"message\":\"Data already present\"}";
                wLog.IsAlreadyReceived = true;
            }
            else
            {
                try
                {
                    var jsonPayload = System.Text.Json.JsonSerializer.Serialize(rawBody);
                    //var computedSignature = StaticData.ComputeHmacSha256(jsonPayload, StaticData.webhookSharedSecret);
                    //var computedSignature = HmacSha256.HmacSha256Hex(jsonPayload, StaticData.webhookSharedSecret);
                    var computedSignature = HmacSha256.HmacSha256Hex(rawBody, StaticData.webhookSharedSecret);

                    if (!signature.Equals(computedSignature, StringComparison.OrdinalIgnoreCase))
                    {
                        wLog.IsMatch = false;
                        //responseMessage = "{\"status\":\"FAILED\",\"message\":\"Invalid signature\" " + computedSignature + ", Secret-" + StaticData.webhookSharedSecret + "}";
                        responseMessage = "{\"status\":\"FAILED\",\"message\":\"Invalid signature\"}";
                    }
                    else
                    {
                        wLog.IsMatch = true;
                        WebhookTransaction webhookTransaction = new WebhookTransaction();
                        string saveResponse = webhookTransaction.Save(payload);
                        webhookTransaction = null;
                        responseMessage = "{\"status\":\"SUCCESS\",\"message\":\"Data accepted\"}";
                    }
                }
                catch (Exception)
                {

                }
            }

            wLog.ResponseMessage = responseMessage;
            wLog.SaveLog();

            if(!wLog.IsMatch && !wLog.IsAlreadyReceived)
            {
                return Unauthorized(responseMessage);
            }
            else
            {
                return Ok(responseMessage);
            }

        }


        

    }

    
}