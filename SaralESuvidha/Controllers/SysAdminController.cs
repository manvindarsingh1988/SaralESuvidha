﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using System.IO;
using SaralESuvidha.Filters;
using SaralESuvidha.Models;
using SaralESuvidha.ViewModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

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
        
        public IActionResult SaveSystemMaintain(string m, int active)
        {
            string result = string.Empty;
            SystemSetting ss = new SystemSetting
            {
                IsDown = active == 1 ? true : false,
                IsDownMessage = m
            };
            result = ss.SaveSystemMaintain();
            return Content(result);
        }

        public IActionResult SystemSetting()
        {
            return View();
        }

    }
}
