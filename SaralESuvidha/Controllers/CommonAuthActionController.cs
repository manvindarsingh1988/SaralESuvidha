using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using SaralESuvidha.Filters;
using SaralESuvidha.Models;
using SaralESuvidha.ViewModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UPPCLLibrary.BillFail;
using System.Net.Http;
using System.Net.Http.Headers;

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

        public IActionResult RetailUserDetail(string usd, int fundTransfer = 0)
        {
            try
            {
                int Id = Convert.ToInt32(StaticData.ConvertHexToString(usd));
                var balResponse = StaticData.retailUser.GetBalanceWithName(Id, "", fundTransfer);
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
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "FileData/");
                result = StaticData.RechargeReportRetailClientByDate(orderNo, dateF, dateT, x, filePath);
            }
            catch (Exception ex)
            {
                result = "Errors: Exception: " + ex.Message;
            }

            return Content(result);
        }

        public IActionResult DailyClientStatementSummaryResult(string dateFrom, string dateTo, int x, int orderNo, int export = 0)
        {
            string result = string.Empty;
            try
            {
                DateTime dateF = Convert.ToDateTime(StaticData.ConvertHexToString(dateFrom));
                DateTime dateT = Convert.ToDateTime(StaticData.ConvertHexToString(dateTo));
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "FileData/");
                result = StaticData.RechargeSummaryReportRetailClientByDate(orderNo, dateF, dateT, x, filePath);
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
        public IActionResult GetAllPendingRequests()
        {
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    List<RetailUserGrid> allRetailUser = con.Query<RetailUserGrid>("usp_RetailUserList",
                        commandType: System.Data.CommandType.StoredProcedure).ToList();
                    //OperationMessage = saveResponse;
                    var aaData1 = new { data = allRetailUser.Where(_ => _.Active == 0 || _.DocVerification == 0 || _.ActivatedTill != null) };
                    return Json(aaData1);
                }
            }
            catch (Exception ex)
            {
                return Content("Exception: " + ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetActivationDetails(string id)
        {
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var queryParameters = new DynamicParameters();
                    queryParameters.Add("@Id", id);
                    RetailUserGrid user = con.QuerySingleOrDefault<RetailUserGrid>("usp_GetActivationDetails", queryParameters, commandType: System.Data.CommandType.StoredProcedure);
                    return Json(user);
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

        public ActionResult DownloadFiles(string id)
        {
            var content = Array.Empty<byte>();
            try
            {
                string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "KYCDocFiles/" + id + "/");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                var fileName = id;
                fileName = folderPath + fileName + ".zip";
                if (System.IO.File.Exists(fileName))
                {
                    System.IO.File.Delete(fileName);
                }
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
                };

                var client = new HttpClient(handler);

                // Set the base address to simplify maintenance & requests
                client.BaseAddress = new Uri("https://kycdoc.saralesuvidha.com/");

                // Post to the endpoint

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //GET Method

                Task.Run(async () =>
                {
                    try
                    {
                        var response = await client.GetAsync($"/saralkycdoc/downloadfile?fileName={id}");
                        if (response.IsSuccessStatusCode)
                        {
                            var data = await response.Content.ReadAsAsync<FileItem>();
                            //content = ;
                            System.IO.File.WriteAllBytes(fileName, data.Content);
                        }
                        return Content("Failed to retrieve file");
                    }
                    catch (Exception ex)
                    {
                        return Content("Exception Task: " + ex.Message);
                    }
                }).Wait();
                
                content = System.IO.File.ReadAllBytes(fileName);
                System.IO.File.Delete(fileName);
                return File(content, "application/zip", id + ".zip");

            }
            catch (FileNotFoundException ex)
            {
                return Content("Exception Task: " + ex.Message);
            }
            catch (Exception ex)
            {
                return Content("Exception Task: " + ex.Message);
            }
            return File(content, "application/zip", id + ".zip");
            //return File(content, "application/zip", id + ".zip");
        }

        public class FileItem
        {
            public string FileName { get; set; }
            public byte[] Content { get; set; }
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

        public IActionResult ChangeActivation(string id, int updateChild)
        {
            string result = string.Empty;
            result = StaticData.UpdateActiveState(id, updateChild);
            return Content(result);
        }

        [HttpPost]
        public IActionResult ChangeKYCActivation(ActivateUser activateUser)
        {
            string result = string.Empty;
            result = StaticData.UpdateKYCState(activateUser.Id, activateUser.DocVerification, activateUser.Active, activateUser.DocVerificationFailed, activateUser.FailureReason);
            
            return Json(new { success = false, responseText = result });
        }

        public IActionResult UpdateDistributor(string id, string masterId)
        {
            string result = string.Empty;
            result = StaticData.UpdateDistributor(id, masterId);
            return Content(result);
        }

        [HttpGet]
        public IActionResult GetUserDetails(string id)
        {
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@RetailUserId", id);
                    List<RetailUserGrid> allRetailUser = con.Query<RetailUserGrid>("usp_GetUserDetails", parameters,
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

        public IActionResult DistributorList(int id)
        {
            return Content(StaticData.DistributorListJson(id));
        }
    }

    public class ActivateUser
    {
        public string Id { get; set; }
        public int Active { get; set; }
        public int DocVerificationFailed { get; set; }
        public int DocVerification { get; set; }
        public string FailureReason { get; set; }
    }
}