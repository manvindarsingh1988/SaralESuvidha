using System;
using System.Data.SqlClient;
using Dapper;
using System.IO;
using SaralESuvidha.Filters;
using SaralESuvidha.Models;
using SaralESuvidha.ViewModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace SaralESuvidha.Controllers
{
    [SysAdminFilter]
    public class SysAdminController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;

        public SysAdminController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
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

        public IActionResult DayWiseClosing()
        {
            return View();
        }

        public IActionResult RefundList()
        {
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CreateMasterDistributor()
        {
            return View();
        }
        
        public IActionResult CreateWhiteLabel()
        {
            return View();
        }

        public IActionResult SetMargin()
        {
            return View();
        }
        
        public IActionResult UserCommissionReport()
        {
            return View();
        }
        
        public IActionResult DailyStatement()
        {
            return View();
        }
        
        public IActionResult ListAllUserWithBalance()
        {
            return View();
        }

        public IActionResult PnLReport()
        {
            return View();
        }

        //[HttpPost]
        //public IActionResult SaveMasterDistributor(RetailUserViewModel data) //FromBody
        //{
        //    return Json(data);
        //}

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
                return View(retailUserViewModel);
            }
            else
            {
                //return Content("Invalid model state - " + retailUserViewModel.FirstName);
                retailUserViewModel.OperationMessage = "Error in creating white label. User data can not be validated.";
                return View(retailUserViewModel);
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
            return View(retailUserViewModel);

            /*
            if (ModelState.IsValid)
            {
                //return Content("Saved - " + retailUserViewModel.FirstName);
                
            }
            else
            {
                //return Content("Invalid model state - " + retailUserViewModel.FirstName);
                retailUserViewModel.OperationMessage = "Error in creating master distributor. User data can not be validated.";
                return View(retailUserViewModel);
            }
            */
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult UpdateUserInfo(RetailUserViewModel retailUserViewModel)
        {
            retailUserViewModel.Update();
            if(retailUserViewModel.OperationMessage != null && retailUserViewModel.OperationMessage.StartsWith("Error:"))
            {
                return View(retailUserViewModel);
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

                return View(retailUserViewModel);
            }
        }

        public IActionResult UpdateUser(string id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            using (var con = new SqlConnection(StaticData.conString))
            {
                var retailUserToUpdate = con.QuerySingleOrDefault<RetailUserViewModel>("usp_GetUserDetailsToUpdate", parameters, commandType: System.Data.CommandType.StoredProcedure);
                return View(retailUserToUpdate);
            }
        }

        public IActionResult LoadUser(string id)
        {
            var retailUserToUpdate = KYCHelper.LoadUser(id, Path.Combine(_hostingEnvironment.WebRootPath, "KYCDocFiles/"));
            return View(retailUserToUpdate);
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
                return View(save);
            }
            else
            {
                userUtilityMargin.OperationMessage = "Error in setting margin. User data can not be validated.";
                return View(userUtilityMargin);
            }
        }

        public IActionResult ListMasterDistributor()
        {
            return View();
        }
        
        public IActionResult UserReport()
        {
            return View();
        }
        
        public IActionResult TransferFund()
        {
            return View();
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
            string result = "";
            try
            {
                result = StaticData.SystemSettingList();
            }
            catch (Exception ex)
            {
                result = "Exception: " + ex.Message;
            }

            return Content(result);
        }

        public IActionResult DailySalesReport()
        {
            return View();
        }

        public IActionResult ClientStatement()
        {
            return View();
        }

        public IActionResult ClientFundReport()
        {
            return View();
        }

        public IActionResult SearchRecharge()
        {
            return View();
        }

        public IActionResult Pending()
        {
            return View();
        }

        public IActionResult CreatePlan()
        {
            return View();
        }

        public IActionResult SavePlan()
        {
            return Content("saved");
        }

        public IActionResult MarginSheet()
        {
            return View();
        }

        public IActionResult UPPCLDailyReport()
        {
            return View();
        }
        
        public IActionResult AccountTopupReport()
        {
            return View();
        }
        
        public IActionResult DailyUPPCLReportResult(string dateFrom, string dateTo, int x)
        {
            string result = string.Empty;
            string filePath = Path.Combine(_hostingEnvironment.WebRootPath,"FileData/");
            
            try
            {
                DateTime dateF = Convert.ToDateTime(StaticData.ConvertHexToString(dateFrom));
                DateTime dateT = Convert.ToDateTime(StaticData.ConvertHexToString(dateTo));
                if (HttpContext.Session != null)
                    result = StaticData.RechargeReportUPPCLByDate( dateF, dateT, x, filePath);
            }
            catch (Exception ex)
            {
                result = "Errors: Exception: " + ex.Message;
            }
            return Content(result);
        }

        public IActionResult AllUserReportResult(int x)
        {
            string result = string.Empty;
            string filePath = Path.Combine(_hostingEnvironment.WebRootPath,"FileData/");
            
            try
            {
                if (HttpContext.Session != null)
                    result = StaticData.AllUserReportResult( x, filePath);
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

        public IActionResult AllPnLReportResultByUserAndDate(string dateFrom,string dateTo, int x, int orderNo)
        {
            string result = string.Empty;
            string filePath = Path.Combine(_hostingEnvironment.WebRootPath, "FileData/");
            DateTime dateF = Convert.ToDateTime(StaticData.ConvertHexToString(dateFrom));
            DateTime dateT = Convert.ToDateTime(StaticData.ConvertHexToString(dateTo));
            try
            {
                if (HttpContext.Session != null)
                    result = StaticData.AllPnLReportResultByUserAndDate(x, dateF,dateT, orderNo, filePath);
            }
            catch (Exception ex)
            {
                result = "Errors: Exception: " + ex.Message;
            }
            return Content(result);
        }
        
        public IActionResult AddSalaryForUser(string userId, int salAmount)
        {
            string result = string.Empty;
            if(string.IsNullOrEmpty(userId)) { return Content("Errors: Id cannot be null"); }
            if (salAmount > 0)
            {
                try
                {
                    result = StaticData.AddSalary(userId, salAmount) + "Id - " + userId;
                }
                catch (Exception ex)
                {
                    result = "Errors: Exception: " + ex.Message;
                }
                return Content(result);
            }
            else
            {
                return Content(string.Empty);
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
            string result = string.Empty;
            SystemSetting ss = new SystemSetting
            {
                IsDown = active == 1 ? true : false,
                IsDownMessage = m,
                IsOTSDown = ots == 1 ? true : false
            };
            result = ss.SaveSystemMaintain();
            return Content(result);
        }

        public IActionResult SystemSetting()
        {
            return View();
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

    }
}
