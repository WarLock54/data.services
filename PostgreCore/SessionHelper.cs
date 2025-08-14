using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using ServiceStack;
using ServiceStack.Web;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.Features;
using HelperLibrary;
using System.IdentityModel.Tokens.Jwt;

namespace PostgreCore
{
    public class SessionHelper
    {

        public static SSSessionInfo GetWebApiSessionInfo(HttpContext httpContext)
        {
            SSSessionInfo sessionInfo = new();

            try
            {
                Dictionary<string, string> headersDictionary = httpContext.Request.Headers.ToDictionary(
                  h => h.Key,
                  h => h.Value.FirstOrDefault().ToString() ?? string.Empty,//tostring sonradan eklendi???
                  StringComparer.OrdinalIgnoreCase
                );

                sessionInfo.Headers = headersDictionary;
                string token = httpContext.Request.Headers["Authorization"].FirstOrDefault().ToString();//tostring sonradan eklendi ????
                if (string.IsNullOrWhiteSpace(token))
                {
                    sessionInfo.UserId = -1;
                    sessionInfo.Username = "No_Token";
                }


                if (httpContext.User.Identity is { IsAuthenticated: true })
                {
                    // Token'daki 'sub' claim'ini kullanıcı adı olarak alıyoruz.
                    sessionInfo.Username = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                    // Eğer token'a userId gibi bir claim eklediyseniz onu da buradan alabilirsiniz:
                    // string userIdClaim = httpContext.User.FindFirst("userId")?.Value;
                    // if (!string.IsNullOrEmpty(userIdClaim))
                    // {
                    //     sessionInfo.UserId = int.Parse(userIdClaim);
                    // }
                }
                else
                {
                    sessionInfo.UserId = -1;
                    sessionInfo.Username = "Unauthorized_User";
                }
            
           
                sessionInfo.IPAddress = httpContext?.Connection?.RemoteIpAddress?.ToString();
                var x = httpContext?.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress?.ToString();
                var x2 = GetIPAddress(httpContext);

            }
           catch (Exception)
            {
                sessionInfo.UserId = -1;
                sessionInfo.Username = "Session_Error";
            }
            return sessionInfo;
        }
        public static string GetIPAddress(HttpContext context)
        {
            if (!string.IsNullOrEmpty(context.Request.Headers["X-Forwarded-For"]))
            {
                return context.Request.Headers["X-Forwarded-For"];
            }
            return context.Connection?.RemoteIpAddress?.ToString();
        }

    }
}
