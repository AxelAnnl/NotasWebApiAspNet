using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace NotasWeb.Services
{
	public interface IUserServices
	{
		string GetUserId();
	}
	public class UserServices : IUserServices
	{
		private HttpContext httpContext;

		public UserServices(IHttpContextAccessor httpContextAccessor)
        {
			httpContext = httpContextAccessor.HttpContext;
		}
		public string GetUserId()
		{
			if (httpContext.User.Identity.IsAuthenticated)
			{
				var idClaim = httpContext.User.Claims.
					Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault();

				return idClaim.Value;

			}
			else
			{
				throw new Exception("El Usuario No esta autenticado");
			}

		}
		//public string GetUserId()
		//{
		//	return "e5480634-d474-4750-b9b7-7ca5ee168cb1";
		//}
	}
}
