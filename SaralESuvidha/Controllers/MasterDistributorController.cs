using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SaralESuvidha.Filters;
using SaralESuvidha.Models;
using SaralESuvidha.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace SaralESuvidha.Controllers
{
    [MasterDistributorFilter]
    public class MasterDistributorController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;

        public MasterDistributorController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ListDistributor()
        {
            return View();
        }
        
        public IActionResult FundReport()
        {
            return View();
        }
        

        [HttpGet]
        public IActionResult GetAllDistributor()
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
        
        public IActionResult TransferFund()
        {
            return View();
        }
        
        //did = Downline Retailer Id, ac tran type i.e. cr or dr
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
        
        public IActionResult AccountStatement()
        {
            return View();
        }

        public IActionResult CreateDistributor()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult SaveDistributor(RetailUserViewModel retailUserViewModel)
        {
            retailUserViewModel.MasterId = HttpContext.Session.GetString("RetailerId");
            retailUserViewModel.UserType = 6;
            retailUserViewModel.Password = StaticData.GeneratePassword(8);
            retailUserViewModel.Save();
            if(retailUserViewModel.Id != null)
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
            }
            
            return View(retailUserViewModel);

            /*
            if (ModelState.IsValid)
            {
                
            }
            else
            {
                //return Content("Invalid model state - " + retailUserViewModel.FirstName);
                retailUserViewModel.OperationMessage = "Error in creating distributor. User data can not be validated.";
                return View(retailUserViewModel);
            }
            */
        }

        public IActionResult UpdateDistributor(string id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            using (var con = new SqlConnection(StaticData.conString))
            {
                var retailUserToUpdate = con.QuerySingleOrDefault<RetailUserViewModel>("usp_GetUserDetailsToUpdate", parameters, commandType: System.Data.CommandType.StoredProcedure);
                return View(retailUserToUpdate);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult UpdateDistributorInfo(RetailUserViewModel retailUserViewModel)
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
            if(docUpdated)
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Id", retailUserViewModel.Id);
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var retailUserToUpdate = con.QuerySingleOrDefault<RetailUserViewModel>("usp_UpdateDocumentUploadStatus", parameters, commandType: System.Data.CommandType.StoredProcedure);
                    
                }
            }
            return View(retailUserViewModel);
        }

        public IActionResult LoadUser(string id)
        {
            var retailUserToUpdate = KYCHelper.LoadUser(id, Path.Combine(_hostingEnvironment.WebRootPath, "KYCDocFiles/"));
            return View(retailUserToUpdate);
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

        public IActionResult ChangePassword()
        {
            return View();
        }

    }
}
