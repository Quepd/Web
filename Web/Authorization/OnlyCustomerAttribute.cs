using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class OnlyCustomerAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var role = context.HttpContext.Session.GetString("UserRole");

        
        var actionName = context.ActionDescriptor.RouteValues["action"];
        var controllerName = context.ActionDescriptor.RouteValues["controller"];

        if (controllerName == "Admin" && actionName == "Index")
        {
            base.OnActionExecuting(context);
            return;
        }

       
        if (!string.IsNullOrEmpty(role) && role != "Customer")
        {
            context.Result = new RedirectToActionResult("Index", "Admin", null);
            return;
        }

        base.OnActionExecuting(context);
    }
}
