using HelperLibrary;
using ServiceStack;
using ServiceStack.Auth;
using ServiceStack.Configuration;
using System.Net;

namespace ServicePackage
{
    public static class BaseHostUtil
    {
        public static CorsFeature GetBaseHostCors()
        {
            List<string> corsUrls = new List<string>() {
                
                "ionic://localhost","capacitor://localhost",
                "http://localhost", "https://localhost:44306","http://localhost:44306"
            };

            #region LocalNetwork

            corsUrls.AddRange(new List<string>() {
                    "http://localhost",
                    "http://localhost:8101",
                    "http://localhost:8100",
                    "http://localhost:8200",
                    "http://localhost:35729",
                    "http://localhost:35703",
                    "http://localhost:8001",
                    "http://localhost:8000",
                    "http://localhost:53703",
                    "http://localhost:60441",
                    "http://localhost:8080",
                    "http://localhost:5431",
                    "http://localhost:56110",
                    "http://localhost:4200",
                    "https://localhost:44306",
                    "http://localhost:44306"

            });
            //44306
            for (int i = 2; i <= 254; i++)
            {
                corsUrls.Add(string.Format("http://127.0.0.1.{0}:8101", i.ToString()));
                corsUrls.Add(string.Format("http://127.0.0.{0}:8100", i.ToString()));
            }

            #endregion LocalNetwork

            return new CorsFeature(allowOriginWhitelist: corsUrls,
                        allowCredentials: true,
                        allowedHeaders: "Content-Type, Allow, Authorization"
                        );
        }

        public static string GetRedisUrl()
        {
            ConfigManager cManager = new ConfigManager();
            string redisHost = cManager.GetValue("RedisHost");
            string redisPort = cManager.GetValue("RedisPort");
            string redisDB = cManager.GetValue("RedisServiceDB");
            return string.Format("redis://{0}:{1}?db={2}", redisHost, redisPort, redisDB);
        }
        public static string GetRedisUrl(string redisHost, string redisPort, int redisDB)
        {
            return string.Format("redis://{0}:{1}?db={2}", redisHost, redisPort, redisDB);
        }

        public static AuthFeature GetAuthFeature(IAppSettings appSetting)
        {
            var authFeature = new AuthFeature(() => new AuthUserSession(),
                                            new IAuthProvider[] {
                                                    new CustomJwtAuthProvider(appSetting) {
                                                        AuthKey =  System.Text.Encoding.UTF8.GetBytes("JwtBearer"),
                                                        RequireSecureConnection = false,
                                                    }
                                            });
            authFeature.IncludeAssignRoleServices = false;
            authFeature.IncludeRegistrationService = false;
            authFeature.ServiceRoutes.Clear(); 
            authFeature.IncludeAuthMetadataProvider = false;
            return authFeature;
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static void AddCors(ServiceStackHost host)
        {
        }
        public static void AddGlobalResponseFilters(ServiceStackHost host)
        {
            host.GlobalResponseFilters.Add((req, res, responseDto) =>
            {
                if (responseDto is HttpError error)
                {
                    Console.WriteLine($"Error: {error.Message}");

                    var baseResponse = new BaseResponse
                    {
                        Result = false,
                        ErrorCode = error.Status.ToString(),
                        ErrorMessage = error.Message + "\n" + req.OriginalPathInfo + "\n" + error.InnerException?.StackTrace,
                    };

                    res.StatusCode = (int)HttpStatusCode.OK;
                    res.StatusDescription = "OK";
                    res.WriteAsync(baseResponse.ToJson());
                    res.EndRequest();
                }
            });
        }
    }
}
