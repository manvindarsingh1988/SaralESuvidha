using SaralESuvidha.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SaralESuvidha.Filters
{
    public class HomePageFilter: ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (StaticData.loginSource == "web" && filterContext.RouteData.Values["Controller"].ToString() == "AamApp")
            {
                filterContext.Result = new ContentResult{Content = "<script>window.location='/';</script>", ContentType= "text/html"};
            }
            else if (StaticData.loginSource == "mobile" && filterContext.RouteData.Values["Controller"].ToString() == "Home" && filterContext.RouteData.Values["Action"].ToString() != "RetailLogin")
            {
                filterContext.Result = new ContentResult{Content = "<script>window.location='/AamApp/Index';</script>", ContentType= "text/html"};
            }
        }
        
    }
}