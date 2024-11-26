using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Dapper;
using SaralESuvidha.Filters;
using SaralESuvidha.Models;
using SaralESuvidha.ViewModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DocumentFormat.OpenXml.EMMA;

namespace SaralESuvidha.Controllers
{
    [DistributorFilter]
    public class DistributorController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;

        public DistributorController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult ListRetailer()
        {
            return View();
        }
        
        public IActionResult FundReport()
        {
            return View();
        }
        

        public IActionResult DailyDownlineSales()
        {
            return View();
        }

        public IActionResult DownlineSalesReportResult(string dateFrom, string dateTo, int x)
        {
            string result = string.Empty;
            string filePath = Path.Combine(_hostingEnvironment.WebRootPath, "FileData/");

            try
            {
                DateTime dateF = Convert.ToDateTime(StaticData.ConvertHexToString(dateFrom));
                DateTime dateT = Convert.ToDateTime(StaticData.ConvertHexToString(dateTo));
                if (HttpContext.Session != null)
                    result = StaticData.DownlineSalesReportRetailClientByDate(HttpContext.Session.GetString("RetailerId"), dateF, dateT, x, filePath);
            }
            catch (Exception ex)
            {
                result = "Errors: Exception: " + ex.Message;
            }
            return Content(result);
        }

        [HttpGet]
        public IActionResult GetAllRetailer()
        {
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var queryParameters = new DynamicParameters();
                    queryParameters.Add("@MasterId", HttpContext.Session.GetString("RetailerId").ToString());
                    List<RetailUserGrid> allRetailUser = con.Query<RetailUserGrid>("usp_RetailUserListByParent", queryParameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
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

        public IActionResult CreateRetailer()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult SaveRetailer(RetailUserViewModel retailUserViewModel)
        {
            retailUserViewModel.MasterId = HttpContext.Session.GetString("RetailerId");
            retailUserViewModel.UserType = 5;
            retailUserViewModel.Password = StaticData.GeneratePassword(8);
            retailUserViewModel.Save();

            string folderPath = Path.Combine(_hostingEnvironment.WebRootPath, "KYCDocFiles/" + retailUserViewModel.Id + "/");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            SaveFile(retailUserViewModel, folderPath, retailUserViewModel.AadharFront, "AadharFront");
            SaveFile(retailUserViewModel, folderPath, retailUserViewModel.AadharBack, "AadharBack");
            SaveFile(retailUserViewModel, folderPath, retailUserViewModel.PanCard, "PanCard");
            SaveFile(retailUserViewModel, folderPath, retailUserViewModel.Photo, "Photo");
            SaveFile(retailUserViewModel, folderPath, retailUserViewModel.Agreement, "Agreement");
            SaveFile(retailUserViewModel, folderPath, retailUserViewModel.Affidavit, "Affidavit");
            if (retailUserViewModel.Other != null)
            {
                SaveFile(retailUserViewModel, folderPath, retailUserViewModel.Other, "Other");
            }
            return View(retailUserViewModel);

            /*
            if (ModelState.IsValid)
            {
                
            }
            else
            {
                //return Content("Invalid model state - " + retailUserViewModel.FirstName);
                retailUserViewModel.OperationMessage = "Error in creating retailer. User data can not be validated.";
                return View(retailUserViewModel);
            }
            */
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult UpdateRetailerInfo(RetailUserViewModel retailUserViewModel)
        {
            var kycRequired = false;
            
            string folderPath = Path.Combine(_hostingEnvironment.WebRootPath, "KYCDocFiles/" + retailUserViewModel.Id + "/");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            kycRequired = kycRequired || SaveFile(retailUserViewModel, folderPath, retailUserViewModel.AadharFront, "AadharFront");
            kycRequired = kycRequired || SaveFile(retailUserViewModel, folderPath, retailUserViewModel.AadharBack, "AadharBack");
            kycRequired = kycRequired || SaveFile(retailUserViewModel, folderPath, retailUserViewModel.PanCard, "PanCard");
            kycRequired = kycRequired || SaveFile(retailUserViewModel, folderPath, retailUserViewModel.Photo, "Photo");
            kycRequired = kycRequired || SaveFile(retailUserViewModel, folderPath, retailUserViewModel.Agreement, "Agreement");
            kycRequired = kycRequired || SaveFile(retailUserViewModel, folderPath, retailUserViewModel.Affidavit, "Affidavit");
            if (retailUserViewModel.Other != null)
            {
                kycRequired = kycRequired || SaveFile(retailUserViewModel, folderPath, retailUserViewModel.Other, "Other");
            }
            if(retailUserViewModel.KYCRequired == 0 && kycRequired)
            {
                retailUserViewModel.KYCRequired = kycRequired ? 1 : 0;
                retailUserViewModel.PhysicalKYCDone = kycRequired ? 0 : 1;
                retailUserViewModel.Active = kycRequired ? 0 : 1;
            }
            retailUserViewModel.Update();
            return View(retailUserViewModel);
        }

        public IActionResult UpdateRetailer(string id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            using (var con = new SqlConnection(StaticData.conString))
            {
                var retailUserToUpdate = con.QuerySingleOrDefault<RetailUserViewModel>("usp_GetUserDetailsToUpdate", parameters, commandType: System.Data.CommandType.StoredProcedure);
                return View(retailUserToUpdate);
            }
        }

        private static bool SaveFile(RetailUserViewModel retailUserViewModel, string folderPath, IFormFile formFile, string fileName)
        {
            if(formFile != null)
            {
                var name = string.Empty;
                using (var target = new MemoryStream())
                {
                    formFile.CopyTo(target);
                    fileName = fileName + Path.GetExtension(formFile.FileName);
                    name = fileName;
                    fileName = fileName = folderPath + fileName;
                    if(System.IO.File.Exists(fileName))
                    {
                        System.IO.File.Delete(fileName);
                    }
                    System.IO.File.WriteAllBytes(fileName, target.ToArray());
                    return true;
                }
            }
            return false;
        }

        public IActionResult TransferFund()
        {
            return View();
        }
        
        public IActionResult RetailerRechargeReport()
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
                int loggedInUserOrderNo = (int) HttpContext.Session.GetInt32("RetailUserOrderNo");
                string remarks = String.IsNullOrEmpty(rem) ? "" : StaticData.ConvertHexToString(rem);

                if (tranType == "cr")
                {
                    //Transfer to downline
                    result = ru.TransferFundToDownline(loggedInUserId, loggedInUserOrderNo, retailUserOrderNo, amount, remarks, requestIp, requestMachine, null,null,null,DateTime.UtcNow.AddHours(5.5), 1).OperationMessage;
                }
                else if (tranType == "dr")
                {
                    //Reversal from downline
                    result = ru.FundReversalDownline(loggedInUserId,loggedInUserOrderNo, retailUserOrderNo, amount, remarks, requestIp, requestMachine, null,null,null,DateTime.UtcNow.AddHours(5.5), 1).OperationMessage;
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
        
        public IActionResult DailyDistributorDownlineStatementResult(string dateFrom, string dateTo, int x)
        {
            string result = string.Empty;
            try
            {
                DateTime dateF = Convert.ToDateTime(StaticData.ConvertHexToString(dateFrom));
                DateTime dateT = Convert.ToDateTime(StaticData.ConvertHexToString(dateTo));

                string filePath = Path.Combine(_hostingEnvironment.WebRootPath, "FileData/");

                result = StaticData.StatementReportDistributorRetailClientByDate(dateF, dateT, (int) HttpContext.Session.GetInt32("RetailUserOrderNo"), x, filePath);
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

                if (DateOnly.FromDateTime(dateT) == DateOnly.FromDateTime(DateTime.Today) && DateTime.Now.Hour < 23)
                {
                    result = "Errors: Today's report is available after 11 P.M.";
                }
                else
                {
                    if (HttpContext.Session != null)
                        result = StaticData.RechargeReportDistributorByDate((int) HttpContext.Session.GetInt32("RetailUserOrderNo"), dateF, dateT, x, filePath);
                }
            }
            catch (Exception ex)
            {
                result = "Errors: Exception: " + ex.Message;
            }
            return Content(result);
        }
        
        public IActionResult AccountStatement()
        {
            return View();
        }
        
        public IActionResult SetMargin()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult SaveMargin(UtilityMargin userUtilityMargin)
        {
            if (ModelState.IsValid)
            {
                userUtilityMargin.LastUpdateMachine = HttpContext.Connection.RemoteIpAddress.ToString() + " | " +
                                                      HttpContext.Request.Headers["User-Agent"].ToString();
                userUtilityMargin.CreatedByUser = HttpContext.Session.GetString("RetailerId");
                var save = userUtilityMargin.Save();
                return View(save);
            }
            else
            {
                userUtilityMargin.OperationMessage = "Error in setting margin. User data can not be validated.";
                return View(userUtilityMargin);
            }
        }

        public IActionResult MarginSheet()
        {
            return View();
        }
        
        public IActionResult MarginSheetDownline()
        {
            return View();
        }
        
        public IActionResult MarginReport()
        {
            return View();
        }
        
        public IActionResult RetailerStatementReport()
        {
            return View();
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

    }
}
