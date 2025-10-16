using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using SaralESuvidha.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace SaralESuvidha.Filters
{
    public class RetailUserFilter: ActionFilterAttribute
    {
        // private readonly IConfiguration _config;
        //
        // public RetailUserFilter(IConfiguration config)
        // {
        //     _config = config;
        // }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string action = filterContext.ActionDescriptor.RouteValues["action"];
            string controller = filterContext.ActionDescriptor.RouteValues["controller"];

            // 🚀 Skip session check for SabPaisaCallback
            if (controller == "RetailClient" && action == "SabPaisaCallback")
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            if (filterContext.HttpContext.Session.GetInt32("RetailUserOrderNo") == null || filterContext.HttpContext.Session.GetInt32("RetailerType") != 5)
            {
                //Controller controller = filterContext.Controller as Controller;
                
                if (StaticData.loginSource == "web")
                {
                    filterContext.Result = new ContentResult{Content = "<script>window.location='/';</script>", ContentType= "text/html"};
                }
                else if (StaticData.loginSource == "mobile")
                {
                    filterContext.Result = new ContentResult{Content = "<script>window.location='/AamApp/';</script>", ContentType= "text/html"};
                }
                
                //filterContext.Cancel = true;
                //controller.HttpContext.Response.Redirect("./Home");
                
                /*filterContext.HttpContext.Response.Clear();
                filterContext.HttpContext.Response.StatusCode = 302;
                filterContext.HttpContext.Response.RedirectLocation = "http://localhost:5000/";
                filterContext.HttpContext.Response.End();*/
                
                // if (filterContext.HttpContext.Session.GetString("LoginSource") == "web")
                // {
                //     filterContext.Result = new ContentResult{Content = "<script>window.location='/';</script>", ContentType= "text/html"};
                // }
                // else if (filterContext.HttpContext.Session.GetString("LoginSource") == "mobile")
                // {
                //     filterContext.Result = new ContentResult{Content = "<script>window.location='/AamApp/';</script>", ContentType= "text/html"};
                // }
                
                //filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary {{ "Controller", "Home" }, { "Action", "Index" } });
                //window.location.reload();
                //new RouteValueDictionary {{ "Controller", "AamApp" }, { "Action", "Index" }, {"ab", 1} });
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
