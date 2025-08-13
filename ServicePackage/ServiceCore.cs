using HelperLibrary;
using Microsoft.AspNetCore.Http;
using Persistence;
using ServiceStack;
using ServiceStack.Host;
using System.Linq;
using System.Security.Claims;

namespace ServicePackage
{
    public class ServiceCore : Service
    {
        public static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly bool _publicServiceYn; // True ise Elediyede Kullnılacak Demektir:
        private SSSessionInfo _session;
        private HttpContext _originalHttpContext;
        public SSSessionInfo Session
        {
            get
            {
                if (_session == null)
                    _session = GetSession(true, _publicServiceYn); // new SSSessionInfo();
                return _session;
            }
        }

        public ApplicationDbContext GetContext()
        {
            return new ApplicationDbContext();
        }


        public ServiceCore(bool publicServiceYn = false, HttpContext originalHttpContext = null)
        {
            _publicServiceYn = publicServiceYn;

            if (originalHttpContext != null)
            {
                this._originalHttpContext = originalHttpContext;

            }
        }
        public virtual SSSessionInfo SetSession(int userId, string userName, string password, string iPAddress, string tckn)
        {
            SSSessionInfo sessionInfo = new();
            sessionInfo.UserId = userId;
            sessionInfo.Username = userName;
            sessionInfo.IPAddress = iPAddress;
            return sessionInfo;
        }
        public virtual SSSessionInfo GetSession(bool getUserId = true, bool getRegistryYn = false)
        {
            SSSessionInfo sessionInfo = new();

            try
            {

                var httpContext = base.Request;
                ClaimsPrincipal user = null;
                if (base.Request == null && _originalHttpContext != null)
                {
                    user = _originalHttpContext.Request.HttpContext.User;
                    sessionInfo.Username = user.Claims.FirstOrDefault(x => x.Type == "name")?.Value;
                }
                else
                {
                    sessionInfo.Headers = httpContext.Headers.AllKeys.ToDictionary(key => key, key => httpContext.Headers.Get(key));
                    string token = httpContext.Authorization?.ToString(); //httpContext.Headers["Authorization"].FirstOrDefault();
                    if (string.IsNullOrWhiteSpace(token))
                    {
                        sessionInfo.UserId = -1;
                        sessionInfo.Username = "No_Token";
                        //throw new UnauthorizedAccessException("User not logged in");
                    }
                    user = httpContext.GetClaimsPrincipal();
                }

                // var userClaims = httpContext.User?.Identity as ClaimsIdentity;

                if (user != null)
                {
                    sessionInfo.userClaims = user?.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList<object>();

                    //sessionInfo.UserId = user?.Claims.FirstOrDefault(x=>x.Type=="dab_user_id")?.Value;
                    if (string.IsNullOrEmpty(sessionInfo.Username))
                        sessionInfo.Username = user.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;

                    if (string.IsNullOrEmpty(sessionInfo.Username))
                        sessionInfo.Username = user.Claims.FirstOrDefault(x => x.Type == "name")?.Value;

                    sessionInfo.UserId = 0;  //"Means No User Id " Fetch when needied
                    if (sessionInfo.Username.IsNullOrEmpty())
                    {
                        sessionInfo.Username = sessionInfo.Username.IsNullOrEmpty() ? "No_Username" : sessionInfo.Username;
                        sessionInfo.UserId = -1;  //"Means No User name no need to fetch from DB
                    }
                    sessionInfo.FirstName = user.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
                    sessionInfo.DisplayName = user.Claims.FirstOrDefault(x => x.Type == "DisplayName")?.Value;
                    sessionInfo.UserEmail = user.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
                    sessionInfo.UserPhone = user.Claims.FirstOrDefault(x => x.Type == ClaimTypes.MobilePhone)?.Value;
                    sessionInfo.Roles = user.Claims.Where(x => x.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
                    sessionInfo.TokenId = user.Claims.FirstOrDefault(x => x.Type == "TokenId")?.Value;
                    sessionInfo.ClientInfo = user.Claims?.FirstOrDefault(x => x.Type == "client_id")?.Value;
                    var phoneNumber = user.Claims?.FirstOrDefault(x => x.Type == "phone_number")?.Value;

                    DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                    var authTiming = user.Claims?.FirstOrDefault(x => x.Type == "auth_time")?.Value;
                    sessionInfo.AuthTime = epoch.AddSeconds(Convert.ToDouble(authTiming)).ToLocalTime();

                    var expirationTiming = user.Claims?.FirstOrDefault(x => x.Type == "exp")?.Value;
                    sessionInfo.ExpirationDate = epoch.AddSeconds(Convert.ToDouble(expirationTiming)).ToLocalTime();

                }

                else
                {
                    sessionInfo.UserId = -1;
                    sessionInfo.Username = "No_Claims";
                }
                sessionInfo.IPAddress = httpContext != null ? httpContext.Headers["XRealIp"] : user.Claims?.FirstOrDefault(x => x.Type == "XRealIp")?.Value;
            }
            catch (Exception ex)
            {
                sessionInfo.UserId = -1;
                sessionInfo.Username = "Exception:" + ex.Message;
            }
            return sessionInfo;
        }

        public T GetBaseResponse<T>(BsResult res) where T : BaseResponse, new()
        {
            if (!res.Result)
            {
                var result = new T { Result = res.Result, ErrorCode = res.Error.ErrorCode, ErrorMessage = res.Error.ErrorDescription + res.VError?.ToString() };
                //if (result.Errors != null && result.Errors.Length >= 1)
                //{ 
                //    result.ErrorMessage = result.Errors.Select(x=>x.).Join (", ");
                //}
                return result;
            }
            return new T { Result = res.Result };
        }
        public BaseResponse GetBaseResponse(BsResult res)
        {
            if (!res.Result)
            {
                return new BaseResponse { Result = res.Result, ErrorCode = res.Error?.ErrorCode, ErrorMessage = res.Error?.ErrorDescription };
            }
            return new BaseResponse { Result = res.Result };
        }
        public BaseResponse GetBaseResponse(bool result)
        {
            return new BaseResponse { Result = result };
        }

        public BaseResponse GetErrorResponse(Exception ex)
        {
            return new BaseResponse { Result = false, ErrorCode = ex.ToErrorCode(), ErrorMessage = ex.Message };
        }
        public BaseResponse GetErrorResponse(string errorCode, string errorMessage)
        {
            return new BaseResponse { Result = false, ErrorCode = errorCode, ErrorMessage = errorMessage };
        }
    }
}
