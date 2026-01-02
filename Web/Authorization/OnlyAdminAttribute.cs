using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class OnlyAdminAttribute : ActionFilterAttribute
{
	public override void OnActionExecuting(ActionExecutingContext context)
	{
		var role = context.HttpContext.Session.GetString("UserRole");
		if (role != "Admin")
		{
			context.Result = new RedirectToActionResult("Index", "Home", null);
		}
	}
}
