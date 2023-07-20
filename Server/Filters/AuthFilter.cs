using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Server.Filters
{
	public class AuthFilter : IAuthorizationFilter
	{
		public void OnAuthorization(AuthorizationFilterContext context)
		{
			if (!context.HttpContext.Session.TryGetValue("isAuth", out byte[] isAuth))
			{
				context.Result = new RedirectToActionResult("Index", "Authorization", null);
			}
		}
	}
}
