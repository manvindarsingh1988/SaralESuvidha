using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SaralESuvidha.ViewModel;
using System;

namespace SaralESuvidha.Controllers
{
    public class FRController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return Content("ok");
        }

        [HttpGet]
        public IActionResult GetContent()
        {
            string retailerId = HttpContext.Session.GetString("RetailerId");

            if (!string.IsNullOrEmpty(retailerId))
            {
                return PartialView("_PartialFR");
            }
            else
            {
                return PartialView("_PartialFRLogin");
            }

        }

        public IActionResult GetBalanceUWallet()
        {
            string result = string.Empty;
            try
            {
                var balResponse = StaticData.retailUser.GetUPPCLBalance(HttpContext.Session.GetString("RetailerId"));
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

        public IActionResult GetBalanceSWallet()
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

        public IActionResult RetailLogin(string m, string p, string s = "w", string f = "", string d = "")
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
                            }
                            catch (Exception exPin)
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
                        if (user.Item2 == -1)
                        {
                            result[1] = "Account is inactive. Please contact with admin for activation.";
                        }
                    }
                    if (retailUser != null && retailUser.ActivatedTill != null)
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
            return Content(string.Join("$$", result));
        }

    }
}
