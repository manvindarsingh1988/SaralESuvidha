using Dapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SaralESuvidha.Filters;
using SaralESuvidha.Models;
using SaralESuvidha.ViewModel;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace SaralESuvidha.Controllers
{
    [SecondaryAdminFilter]
    public class SecondaryAdminController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;

        public SecondaryAdminController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            return View("~/Views/SysAdmin/Index.cshtml");
        }

        public IActionResult ListAllUserWithBalance()
        {
            return View("~/Views/SysAdmin/ListAllUserWithBalance.cshtml");
        }

        public IActionResult ListMasterDistributor()
        {
            ViewData["UserType"] = HttpContext.Session.GetInt32("UserType") != null ? HttpContext.Session.GetInt32("UserType").ToString() : string.Empty;
            return View("~/Views/SysAdmin/ListMasterDistributor.cshtml");
        }

        public IActionResult RefundList()
        {
            return View("~/Views/SysAdmin/RefundList.cshtml");
        }

        //reports 
        public IActionResult DailySalesReport()
        {
            return View("~/Views/SysAdmin/DailySalesReport.cshtml");
        }

        public IActionResult DailyStatement()
        {
            return View("~/Views/SysAdmin/DailyStatement.cshtml");
        }

        public IActionResult Pending()
        {
            return View("~/Views/SysAdmin/Pending.cshtml");
        }

        public IActionResult ClientStatement()
        {
            return View("~/Views/SysAdmin/ClientStatement.cshtml");
        }

        public IActionResult ClientFundReport()
        {
            return View("~/Views/SysAdmin/ClientFundReport.cshtml");
        }

        public IActionResult AccountTopupReport()
        {
            return View("~/Views/SysAdmin/AccountTopupReport.cshtml");
        }

        public IActionResult SearchRecharge()
        {
            return View("~/Views/SysAdmin/SearchRecharge.cshtml");
        }

        public IActionResult UserReport()
        {
            return View("~/Views/SysAdmin/UserReport.cshtml");
        }

        public IActionResult UserCommissionReport()
        {
            return View("~/Views/SysAdmin/UserCommissionReport.cshtml");
        }

        public IActionResult UPPCLDailyReport()
        {
            return View("~/Views/SysAdmin/UPPCLDailyReport.cshtml");
        }

        public IActionResult SystemSetting()
        {
            return View("~/Views/SysAdmin/SystemSetting.cshtml");
        }

        public IActionResult ChangePassword()
        {
            return View("~/Views/SysAdmin/ChangePassword.cshtml");
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

        public IActionResult AllUserReportResult(int x)
        {
            string result = string.Empty;
            string filePath = Path.Combine(_hostingEnvironment.WebRootPath, "FileData/");

            try
            {
                if (HttpContext.Session != null)
                    result = StaticData.AllUserReportResult(x, filePath);
            }
            catch (Exception ex)
            {
                result = "Errors: Exception: " + ex.Message;
            }
            return Content(result);
        }

        public IActionResult AllUserReportResultByUserAndDate(string date, int x, int orderNo)
        {
            string result = string.Empty;
            string filePath = Path.Combine(_hostingEnvironment.WebRootPath, "FileData/");
            DateTime dateF = Convert.ToDateTime(StaticData.ConvertHexToString(date));
            try
            {
                if (HttpContext.Session != null)
                    result = StaticData.AllUserReportResultByUserAndDate(x, dateF, orderNo, filePath);
            }
            catch (Exception ex)
            {
                result = "Errors: Exception: " + ex.Message;
            }
            return Content(result);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult SaveWhiteLabel(RetailUserViewModel retailUserViewModel)
        {
            if (ModelState.IsValid)
            {
                //return Content("Saved - " + retailUserViewModel.FirstName);
                retailUserViewModel.MasterId = null;
                retailUserViewModel.UserType = 9;
                retailUserViewModel.Password = StaticData.GeneratePassword(8);
                retailUserViewModel.Save();
                //retailUserViewModel.OperationMessage = "Successfully created master distributor.";
                return View("~/Views/SysAdmin/SaveWhiteLabel.cshtml", retailUserViewModel);
            }
            else
            {
                //return Content("Invalid model state - " + retailUserViewModel.FirstName);
                retailUserViewModel.OperationMessage = "Error in creating white label. User data can not be validated.";
                return View("~/Views/SysAdmin/SaveWhiteLabel.cshtml", retailUserViewModel);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult SaveMasterDistributor(RetailUserViewModel retailUserViewModel)
        {
            retailUserViewModel.MasterId = null;
            retailUserViewModel.UserType = 7;
            retailUserViewModel.Password = StaticData.GeneratePassword(8);
            retailUserViewModel.Save();
            //retailUserViewModel.OperationMessage = "Successfully created master distributor.";
            if (retailUserViewModel.Id != null)
            {
                string folderPath = Path.Combine(_hostingEnvironment.WebRootPath, "KYCDocFiles/" + retailUserViewModel.Id + "/");
                string kycFolder = Path.Combine(_hostingEnvironment.WebRootPath, "KYCDocFiles/" + retailUserViewModel.ReferenceId + "/");
                if (Directory.Exists(kycFolder))
                {
                    Directory.Move(kycFolder, folderPath);
                }

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                KYCHelper.SaveFile(retailUserViewModel, folderPath, retailUserViewModel.Photo, "Photo");
                KYCHelper.SaveFile(retailUserViewModel, folderPath, retailUserViewModel.Agreement, "Agreement");
                KYCHelper.SaveFile(retailUserViewModel, folderPath, retailUserViewModel.Affidavit, "Affidavit");
                KYCHelper.SaveFile(retailUserViewModel, folderPath, retailUserViewModel.PoliceVerification, "PoliceVerification");
                KYCHelper.UploadToKYCServer(folderPath, retailUserViewModel.Id);
            }
            return View("~/Views/SysAdmin/SaveMasterDistributor.cshtml", retailUserViewModel);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult UpdateUserInfo(RetailUserViewModel retailUserViewModel)
        {
            retailUserViewModel.Update();
            if (retailUserViewModel.OperationMessage != null && retailUserViewModel.OperationMessage.StartsWith("Error:"))
            {
                return View("~/Views/SysAdmin/UpdateUserInfo.cshtml", retailUserViewModel);
            }
            else
            {
                var docUpdated = false;

                string folderPath = Path.Combine(_hostingEnvironment.WebRootPath, "KYCDocFiles/" + retailUserViewModel.Id + "/");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                docUpdated = docUpdated || KYCHelper.SaveFile(retailUserViewModel, folderPath, retailUserViewModel.Photo, "Photo");
                docUpdated = docUpdated || KYCHelper.SaveFile(retailUserViewModel, folderPath, retailUserViewModel.Agreement, "Agreement");
                docUpdated = docUpdated || KYCHelper.SaveFile(retailUserViewModel, folderPath, retailUserViewModel.Affidavit, "Affidavit");
                docUpdated = docUpdated || KYCHelper.SaveFile(retailUserViewModel, folderPath, retailUserViewModel.PoliceVerification, "PoliceVerification");

                if (docUpdated)
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@Id", retailUserViewModel.Id);
                    using (var con = new SqlConnection(StaticData.conString))
                    {
                        var retailUserToUpdate = con.QuerySingleOrDefault<RetailUserViewModel>("usp_UpdateDocumentUploadStatus", parameters, commandType: System.Data.CommandType.StoredProcedure);

                    }

                    KYCHelper.UploadToKYCServer(folderPath, retailUserViewModel.Id);
                }

                return View("~/Views/SysAdmin/UpdateUserInfo.cshtml", retailUserViewModel);
            }
        }

        public IActionResult UpdateUser(string id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            using (var con = new SqlConnection(StaticData.conString))
            {
                var retailUserToUpdate = con.QuerySingleOrDefault<RetailUserViewModel>("usp_GetUserDetailsToUpdate", parameters, commandType: System.Data.CommandType.StoredProcedure);
                return View("~/Views/SysAdmin/UpdateUser.cshtml", retailUserToUpdate);
            }
        }

        public IActionResult LoadUser(string id)
        {
            var retailUserToUpdate = KYCHelper.LoadUser(id, Path.Combine(_hostingEnvironment.WebRootPath, "KYCDocFiles/"));
            return View("~/Views/SysAdmin/LoadUser.cshtml", retailUserToUpdate);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult SaveMargin(UtilityMargin userUtilityMargin)
        {
            if (ModelState.IsValid)
            {
                userUtilityMargin.LastUpdateMachine = HttpContext.Connection.RemoteIpAddress.ToString() + " | " +
                                                      HttpContext.Request.Headers["User-Agent"].ToString();
                userUtilityMargin.CreatedByUser = HttpContext.Session.GetString("UserId");
                var save = userUtilityMargin.Save(true);
                return View("~/Views/SysAdmin/SaveMargin.cshtml", save);
            }
            else
            {
                userUtilityMargin.OperationMessage = "Error in setting margin. User data can not be validated.";
                return View("~/Views/SysAdmin/SaveMargin.cshtml", userUtilityMargin);
            }
        }

        public IActionResult TransferValidate(string id, string amt, string rem, string ac)
        {
            string result = string.Empty;
            RTran fundTransferRTran = new RTran();
            try
            {
                string tranType = StaticData.ConvertHexToString(ac);

                fundTransferRTran.RequestIp = HttpContext.Connection.RemoteIpAddress.ToString();
                fundTransferRTran.RequestMachine = HttpContext.Request.Headers["User-Agent"].ToString();
                fundTransferRTran.RetailUserOrderNo = Convert.ToInt32(StaticData.ConvertHexToString(id));
                fundTransferRTran.Amount = Convert.ToDecimal(StaticData.ConvertHexToString(amt));
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
                fundTransferRTran.Remarks = StaticData.ConvertHexToString(rem);
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

            return Content("[ " + result + " ]");
        }

        public IActionResult SalesReportResult(string dateFrom, string dateTo, int x)
        {
            string result = string.Empty;
            string filePath = Path.Combine(_hostingEnvironment.WebRootPath, "FileData/");

            try
            {
                DateTime dateF = Convert.ToDateTime(StaticData.ConvertHexToString(dateFrom));
                DateTime dateT = Convert.ToDateTime(StaticData.ConvertHexToString(dateTo));
                if (HttpContext.Session != null)
                    result = StaticData.SalesReportAllByDate(dateF, dateT, x, filePath);
            }
            catch (Exception ex)
            {
                result = "Errors: Exception: " + ex.Message;
            }
            return Content(result);
        }

        public IActionResult SystemSettingList()
        {
            var controller = new SysAdminController(_hostingEnvironment);
            return controller.SystemSettingList();
        }

        public IActionResult DailyUPPCLReportResult(string dateFrom, string dateTo, int x)
        {
            string result = string.Empty;
            string filePath = Path.Combine(_hostingEnvironment.WebRootPath, "FileData/");

            try
            {
                DateTime dateF = Convert.ToDateTime(StaticData.ConvertHexToString(dateFrom));
                DateTime dateT = Convert.ToDateTime(StaticData.ConvertHexToString(dateTo));
                if (HttpContext.Session != null)
                    result = StaticData.RechargeReportUPPCLByDate(dateF, dateT, x, filePath);
            }
            catch (Exception ex)
            {
                result = "Errors: Exception: " + ex.Message;
            }
            return Content(result);
        }

        public IActionResult AllPnLReportResultByUserAndDate(string dateFrom, string dateTo, int x, int orderNo)
        {
            var controller = new SysAdminController(_hostingEnvironment);
            return controller.AllPnLReportResultByUserAndDate(dateFrom, dateTo, x, orderNo);
        }

        public IActionResult AddSalaryForUser(string userId, int salAmount)
        {
            var controller = new SysAdminController(_hostingEnvironment);
            return controller.AddSalaryForUser(userId, salAmount);
        }


        public IActionResult DailyTopupReportResult(string dateFrom, string dateTo, int x)
        {
            string result = string.Empty;
            string filePath = Path.Combine(_hostingEnvironment.WebRootPath, "FileData/");

            try
            {
                DateTime dateF = Convert.ToDateTime(StaticData.ConvertHexToString(dateFrom));
                DateTime dateT = Convert.ToDateTime(StaticData.ConvertHexToString(dateTo));

                if (HttpContext.Session != null)
                    result = StaticData.TopupReportRetailClientByDate(999999999, dateF, dateT, x, filePath);
            }
            catch (Exception ex)
            {
                result = "Errors: Exception: " + ex.Message;
            }
            return Content(result);
        }

        public IActionResult SaveSystemMaintain(string m, int active, int ots)
        {
            var controller = new SysAdminController(_hostingEnvironment);
            return controller.SaveSystemMaintain(m, active, ots);
        }

        public IActionResult SaveMonitor(string ln, string p, string m, string st = "00:00", string et = "23:59", string oldid = "")
        {
            var controller = new SysAdminController(_hostingEnvironment);
            return controller.SaveMonitor(ln, p, m, st, et, oldid);
        }

        public IActionResult UpdateMapping(string id, int? usl, string st, string et)
        {
            var controller = new SysAdminController(_hostingEnvironment);
            return controller.UpdateMapping(id, usl, st, et);
        }

        public IActionResult GetMonthlySummaries(DateTime startDate, DateTime endDate, int x = 0)
        {
            var controller = new SysAdminController(_hostingEnvironment);
            return controller.GetMonthlySummaries(startDate, endDate, x);
        }
    }
}