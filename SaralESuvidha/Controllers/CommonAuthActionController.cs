using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dapper;
using DocumentFormat.OpenXml.Spreadsheet;
using SaralESuvidha.Filters;
using SaralESuvidha.Models;
using SaralESuvidha.ViewModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Razorpay.Api;
using UPPCLLibrary.BillFail;

namespace SaralESuvidha.Controllers
{
    [CommonAuthFilter]
    public class CommonAuthActionController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CommonAuthActionController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult ForceFail(string id)
        {
            string result = "Start";
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    RTran myTran = new RTran();
                    myTran.Id = id;
                    myTran = myTran.LoadRecord();
                    ForceFailResponse forceFailResponse = myTran.ForceFail(true);
                    result = forceFailResponse.message;
                }
                else
                {
                    result = "Invalid id.";
                }
            }
            catch (Exception ex)
            {
                result += ex.Message;
            }

            return Content(result);
        }

        public IActionResult RetailUserDetail(string usd)
        {
            try
            {
                int Id = Convert.ToInt32(StaticData.ConvertHexToString(usd));
                var balResponse = StaticData.retailUser.GetBalanceWithName(Id);
                if (!balResponse.OperationMessage.Contains("Errors"))
                {
                    //ViewData["Error"] = "0";
                    return Content(" [" + balResponse.Order.ToString() + " - " + balResponse.OperationMessage +
                                   "is &#x20B9; " + balResponse.Balance.ToString("N2") + "]");
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
        
        

        public IActionResult DailyAllClientStatementResult(string dateFrom, string dateTo, int x)
        {
            string result = string.Empty;
            try
            {
                DateTime dateF = Convert.ToDateTime(StaticData.ConvertHexToString(dateFrom));
                DateTime dateT = Convert.ToDateTime(StaticData.ConvertHexToString(dateTo));

                //string fileName = "FundReport" + "_" + DateTime.Now.ToString("ddMMMyy-HHmmss") + "_" + Guid.NewGuid().ToString() + ".xlsx";
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "FileData/"); //+ fileName   FileData

                result = StaticData.RechargeReportAllRetailClientByDate(dateF, dateT, x, filePath);
            }
            catch (Exception ex)
            {
                result = "Errors: Exception: " + ex.Message;
            }

            return Content(result);
        }

        public IActionResult DailyClientStatementResult(string dateFrom, string dateTo, int x, int orderNo)
        {
            string result = string.Empty;
            try
            {
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

        public IActionResult DailyClientStatementSummaryResult(string dateFrom, string dateTo, int x, int orderNo)
        {
            string result = string.Empty;
            try
            {
                DateTime dateF = Convert.ToDateTime(StaticData.ConvertHexToString(dateFrom));
                DateTime dateT = Convert.ToDateTime(StaticData.ConvertHexToString(dateTo));
                result = StaticData.RechargeSummaryReportRetailClientByDate(orderNo, dateF, dateT, x);
            }
            catch (Exception ex)
            {
                result = "Errors: Exception: " + ex.Message;
            }

            return Content(result);
        }


        public IActionResult FundReportResult(string dateFrom, string dateTo, int orderNo = 0, string bySource = "All",
            string export = "0")
        {
            string result = string.Empty;
            try
            {
                DateTime dateF = Convert.ToDateTime(StaticData.ConvertHexToString(dateFrom));
                DateTime dateT = Convert.ToDateTime(StaticData.ConvertHexToString(dateTo));
                if (export == "0")
                {
                    result = StaticData.FundTransferBetweenPeriodOffice(dateF, dateT, orderNo, bySource);
                }
                else if (export == "1")
                {
                    string fileName = "FundReport" + "_" + DateTime.Now.ToString("ddMMMyy-HHmmss") + "_" +
                                      Guid.NewGuid().ToString() + ".xlsx";

                    string filePath =
                        Path.Combine(_webHostEnvironment.WebRootPath, "FileData/"); //+ fileName   FileData
                    //result = filePath;
                    result = StaticData.FundTransferBetweenPeriodOffice(dateF, dateT, orderNo, bySource, export,
                        fileName, filePath);
                }
            }
            catch (Exception ex)
            {
                result = "Errors: Exception: " + ex.Message;
            }

            return Content(result);
        }

        public IActionResult CommissionReportResult(string dateFrom, string dateTo, int userType = 0,
            string export = "0")
        {
            string result = string.Empty;
            try
            {
                DateTime dateF = Convert.ToDateTime(StaticData.ConvertHexToString(dateFrom));
                DateTime dateT = Convert.ToDateTime(StaticData.ConvertHexToString(dateTo));
                if (export == "0")
                {
                    result = StaticData.CommissionBetweenPeriodOffice(dateF, dateT, userType);
                }
                else if (export == "1")
                {
                    string
                        fileName =
                            "CommissionReport"; // + "_" + DateTime.Now.ToString("ddMMMyy-HHmmss") + "_" + Guid.NewGuid().ToString() + ".xlsx";
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "FileData/");
                    result = StaticData.CommissionBetweenPeriodOffice(dateF, dateT, userType, export, fileName,
                        filePath);
                }
            }
            catch (Exception ex)
            {
                result = "Errors: Exception: " + ex.Message;
            }

            return Content(result);
        }


        public IActionResult SearchNumberResult(string rNumber, string dateFrom, string dateTo)
        {
            string result = string.Empty;
            try
            {
                DateTime dateF = Convert.ToDateTime(StaticData.ConvertHexToString(dateFrom));
                DateTime dateT = Convert.ToDateTime(StaticData.ConvertHexToString(dateTo));
                result = StaticData.SearchNumberByDate(StaticData.ConvertHexToString(rNumber), dateF, dateT);
            }
            catch (Exception ex)
            {
                result = "Errors: Exception: " + ex.Message;
            }

            return Content(result);
        }


        [HttpGet]
        public IActionResult GetAllMasterDistributor()
        {
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    List<RetailUserGrid> allRetailUser = con.Query<RetailUserGrid>("usp_RetailUserList",
                        commandType: System.Data.CommandType.StoredProcedure).ToList();
                    //OperationMessage = saveResponse;
                    var aaData1 = new { data = allRetailUser };
                    return Json(aaData1);
                }
            }
            catch (Exception ex)
            {
                return Content("Exception: " + ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetRetailUserByType(string UserType, int ExportExcel = 0)
        {
            try
            {
                string fileName = "UserList";
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "FileData/"); //+ fileName   FileData
                return Content(StaticData.UserListByType(UserType, fileName, filePath, (ExportExcel != 0)));
            }
            catch (Exception ex)
            {
                return Content("Exception: " + ex.Message);
            }
        }


        public IActionResult PendingResult()
        {
            return Content(StaticData.RechargePendingReport());
        }

        public IActionResult RTranUpdateData(string recordId)
        {
            return Content(StaticData.RTranUpdateData(StaticData.ConvertHexToString(recordId)));
        }

        public IActionResult RTranUpdateStatus([FromForm] RTranPending rTran)
        {
            //using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            //{
            //    return Content(reader.ReadToEnd());
            //}
            //return Content(rTran.txtUpdateRechargeNumber + " || " + rTran.txtUpdateStatus + " || " + rTran.txtUpdateLiveId);
            string result = "";
            RTran myTran = new RTran();
            try
            {
                result = myTran.UpdateStatusByData(rTran);
            }
            catch (Exception ex)
            {
                result = "Error: " + ex.Message;
            }
            finally
            {
                myTran = null;
            }

            return Content(result);
        }

        public IActionResult SmsPassword(string usd)
        {
            try
            {
                return Content(StaticData.SmsPassword(Convert.ToInt32(StaticData.ConvertHexToString(usd))));
            }
            catch (Exception ex)
            {
                return Content("Errors: Exception: " + ex.Message);
            }
        }

        public IActionResult ResetPassword(string usd)
        {
            try
            {
                return Content(StaticData.ResetPassword(Convert.ToInt32(StaticData.ConvertHexToString(usd))));
            }
            catch (Exception ex)
            {
                return Content("Errors: Exception: " + ex.Message);
            }
        }

        public IActionResult ROfferServerManager(string recordId, int status)
        {
            try
            {
                return Content(StaticData.ROfferServerUpdate(Convert.ToInt32(StaticData.ConvertHexToString(recordId)),
                    status));
            }
            catch (Exception ex)
            {
                return Content("Errors: Exception: " + ex.Message);
            }
        }

        public IActionResult MarginSheetList()
        {
            string result = "";
            try
            {
                if (HttpContext.Session != null)
                    result = StaticData.MarginSheetListByRetailer(HttpContext.Session.GetString("UserId"));
            }
            catch (Exception ex)
            {
                result = "Exception: " + ex.Message;
            }

            return Content(result);
        }

        public IActionResult MarginSheetListByOrderNo(string usd)
        {
            string result = "";
            try
            {
                int Id = Convert.ToInt32(StaticData.ConvertHexToString(usd));
                result = StaticData.MarginSheetByRetailerOrderNo(Id);
            }
            catch (Exception ex)
            {
                result = "Exception: " + ex.Message;
            }

            return Content(result);
        }

        public IActionResult RTranApiLoadByDate(string dateFrom, string dateTo)
        {
            string result = string.Empty;
            try
            {
                DateTime dateF = Convert.ToDateTime(StaticData.ConvertHexToString(dateFrom));
                DateTime dateT = Convert.ToDateTime(StaticData.ConvertHexToString(dateTo));
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "FileData/");
                result = StaticData.RTranApiLoadByDate(dateF, dateT, filePath);
            }
            catch (Exception ex)
            {
                result = "Errors: " + ex.Message;
            }

            return Content(result);
        }

        
    }
}