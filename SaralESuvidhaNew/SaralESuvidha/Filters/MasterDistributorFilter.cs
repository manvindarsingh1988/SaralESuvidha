using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SaralESuvidha.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace SaralESuvidha.Filters
{
    public class MasterDistributorFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Session.GetInt32("RetailUserOrderNo") == null || filterContext.HttpContext.Session.GetInt32("RetailerType") != 7)
            {
                /*filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary {{ "Controller", "Home" },
                        { "Action", "Index" } });*/
                
                if (StaticData.loginSource == "web")
                {
                    filterContext.Result = new ContentResult{Content = "<script>window.location='/';</script>", ContentType= "text/html"};
                }
                else if (StaticData.loginSource == "mobile")
                {
                    filterContext.Result = new ContentResult{Content = "<script>window.location='/AamApp/';</script>", ContentType= "text/html"};
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
