using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace SaralESuvidha.Filters
{
    public class SysAdminFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Session.GetInt32("UserType") == null)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary {{ "Controller", "Home" },{ "Action", "Index" } });
            }
            else
            {
                if (filterContext.HttpContext.Session.GetInt32("UserType") != 8)
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary {{ "Controller", "Home" }, { "Action", "Index" } });
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
