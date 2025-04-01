using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
namespace SaralESuvidha.Filters
{
    public class SecondaryAdminFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Session.GetInt32("UserType") == null)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "Controller", "Home" }, { "Action", "Index" } });
            }
            else
            {
                if (filterContext.HttpContext.Session.GetInt32("UserType") != 9)
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "Controller", "Home" }, { "Action", "Index" } });
                }
            }
            filterContext.HttpContext.Session.SetString("Controller", "SecondaryAdmin");
            base.OnActionExecuting(filterContext);
        }
    }
}