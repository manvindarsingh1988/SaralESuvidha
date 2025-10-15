using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SaralESuvidha.Filters;
using Microsoft.AspNetCore.Mvc;

namespace SaralESuvidha.Controllers
{
    [CustomerSupportFilter]
    public class CustomerSupportController : Controller
    {
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

        public IActionResult Index()
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
        
        public IActionResult RetailUserList()
        {
            return View();
        }
        
        public IActionResult Pending()
        {
            return View();
        }

        public IActionResult ApiLoad()
        {
            return View();
        }
        
    }
}
