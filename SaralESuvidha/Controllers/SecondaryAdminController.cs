using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using SaralESuvidha.Filters;
using System;

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

        public IActionResult Pending()
        {
            return View("~/Views/SysAdmin/Pending.cshtml");
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
        
    }
}
