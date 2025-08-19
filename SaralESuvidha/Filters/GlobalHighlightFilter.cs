using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using SaralESuvidha.ViewModel;

namespace SaralESuvidha.Filters
{
    public class GlobalHighlightFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var controller = context.Controller as Controller;
            if (controller != null)
            {
                var message = StaticData.GetHighlights();
                controller.ViewData["GlobalHighlights"] = message.GlobalHighlights;
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // do nothing
        }
    }
}
