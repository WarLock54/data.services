using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using ServiceStack;
using ServiceStack.Web;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.Features;
using HelperLibrary;

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


                var user = httpContext.User;
                var userClaims = httpContext.User?.Identity as ClaimsIdentity;

                if (userClaims != null)
                {
                    sessionInfo.userClaims = userClaims?.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList<object>();


                    sessionInfo.Username = userClaims?.FindFirst(ClaimTypes.Name)?.Value;
                    sessionInfo.UserId = 0;  //"Means No User Id " Fetch when needied
                    if (sessionInfo.Username.IsNullOrEmpty())
                    {
                        sessionInfo.Username = sessionInfo.Username.IsNullOrEmpty() ? "No_Username" : sessionInfo.Username;
                        sessionInfo.UserId = -1;  //"Means No User name no need to fetch from DB
                    }
                    sessionInfo.FirstName = userClaims?.FindFirst(ClaimTypes.Name)?.Value;
                    sessionInfo.DisplayName = userClaims?.FindFirst("DisplayName")?.Value;
                    sessionInfo.UserEmail = userClaims?.FindFirst(ClaimTypes.Email)?.Value;
                    sessionInfo.UserPhone = userClaims?.FindFirst(ClaimTypes.MobilePhone)?.Value;
                    sessionInfo.Roles = userClaims?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                    sessionInfo.TokenId = userClaims?.FindFirst("TokenId")?.Value;
                    sessionInfo.ClientInfo = userClaims?.FindFirst("client_id")?.Value;
                    var phoneNumber = userClaims?.FindFirst("phone_number")?.Value;

                    DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                    var authTiming = userClaims?.FindFirst("auth_time")?.Value;
                    sessionInfo.AuthTime = epoch.AddSeconds(Convert.ToDouble(authTiming)).ToLocalTime();

                    var expirationTiming = userClaims?.FindFirst("exp")?.Value;
                    sessionInfo.ExpirationDate = epoch.AddSeconds(Convert.ToDouble(expirationTiming)).ToLocalTime();


                }
                else
                { 
                    sessionInfo.UserId = -1; 
                    sessionInfo.Username = "No_Claims";
                }
                sessionInfo.IPAddress = httpContext?.Connection?.RemoteIpAddress?.ToString();
                var x = httpContext?.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress?.ToString();
                var x2 = GetIPAddress(httpContext);

            }
            catch (Exception ex)
            {
                sessionInfo.UserId = -1;
                sessionInfo.Username = "Exception:" + ex.Message;
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
