using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SaralESuvidha.Filters;
using SaralESuvidha.Models;
using SaralESuvidha.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SaralESuvidha.Controllers
{
    [RetailUserCommonFilter]
    public class RetailUserCommonController : Controller
    {
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
        
        public IActionResult DailyClientStatementResult(string dateFrom, string dateTo, int x)
        {
            string result = string.Empty;
            try
            {
                int orderNo = (int) HttpContext.Session.GetInt32("RetailUserOrderNo");
                DateTime dateF = Convert.ToDateTime(StaticData.ConvertHexToString(dateFrom));
                DateTime dateT = Convert.ToDateTime(StaticData.ConvertHexToString(dateTo));
                result = StaticData.RechargeReportRetailClientByDate(orderNo, dateF, dateT, x);
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
                np = Regex.Replace(np, @"[^0-9a-zA-Z]+", "");
                op = Regex.Replace(op, @"[^0-9a-zA-Z]+", "");
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

    }
}
