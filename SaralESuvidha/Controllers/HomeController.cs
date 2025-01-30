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
using ElectricityBillInfo = UPPCLLibrary.ElectricityBillInfo;
using DocumentFormat.OpenXml.Wordprocessing;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using Dapper;
using System.Data.SqlClient;
using Razorpay.Api;

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

                                default:
                                    break;

                            }

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
        
        public IActionResult ReceiptOTSUPPCL(string t)
        {
            string tranId = "";
            try
            {
                tranId = StaticData.ConvertHexToString(t);
                var UPPCLReceipt = StaticData.PaymentOTSReceiptDataByTranId(tranId);
                UPPCLOTSReciptModal modal = OTSReciptGenerator.GenerateOTSRecipt(UPPCLReceipt.AccountId, UPPCLReceipt.Amount, UPPCLReceipt.IsFull.GetValueOrDefault(), tranId, UPPCLReceipt.RechargeStatus);
                return View(modal);
            }
            catch (Exception)
            {
                return View(new UPPCLOTSReciptModal() { TelecomOperatorName = "INVALID DETAILS" });
            }
        }
        public IActionResult ReceiptOTSUPPCLT(string t)
        {
            string tranId = "";
            try
            {
                tranId = StaticData.ConvertHexToString(t);
                var UPPCLReceipt = StaticData.PaymentOTSReceiptDataByTranId(tranId);
                UPPCLOTSReciptModal modal = OTSReciptGenerator.GenerateOTSRecipt(UPPCLReceipt.AccountId, UPPCLReceipt.Amount, UPPCLReceipt.IsFull.GetValueOrDefault(), tranId, UPPCLReceipt.RechargeStatus);
                return View(modal);
            }
            catch (Exception)
            {
                return View(new UPPCLOTSReciptModal() { TelecomOperatorName = "INVALID DETAILS" });
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
        
        public IActionResult Joker(string m, string o, string k)
        {
            try
            {
                ElectricityBillInfo ebi = new ElectricityBillInfo();
                if (k == "9415004756")
                {
                    UPPCLLibrary.UPPCLManager.CheckTokenExpiry();
                    ebi = UPPCLLibrary.UPPCLManager.ElectricityBillInfoFromBillFetch(UPPCLLibrary.UPPCLManager.BillFetch(o, m));

                }
                return Content(JsonConvert.SerializeObject(ebi));

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

    }
}
