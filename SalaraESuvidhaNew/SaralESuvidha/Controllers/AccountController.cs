using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SaralESuvidha.Filters;
using Microsoft.AspNetCore.Mvc;

namespace SaralESuvidha.Controllers
{
    [AccountFilter]
    public class AccountController : Controller
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
    }
}
