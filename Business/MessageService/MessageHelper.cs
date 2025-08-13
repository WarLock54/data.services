using HelperLibrary;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Business.MessageService
{
    public partial class MessageHelper
    : IMessageHelper
    {
        private readonly ILogger<CustomerBs> _logger;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        public MessageHelper(ILogger<CustomerBs> logger, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<BaseResultDTO> LogoutFCMTokenRequest(LogoutDeviceRequestDTO request)
        {
            BaseResultDTO result;
            try
            {
                using (var httpClient = new HttpClient())
                {

                    string json = JsonConvert.SerializeObject(request);
                    var logoutRequest = new StringContent(json, Encoding.UTF8, "application/json");

                    var hostUrl = _configuration["MessageServiceUrl"];
                    httpClient.BaseAddress = new Uri($"{hostUrl}/LogoutDeviceRequest");


                    HttpResponseMessage response = await httpClient.PostAsync(httpClient.BaseAddress, logoutRequest);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Logout Token Kayıt İşlemi Başarılı");
                        result = new BaseResultDTO() { Result = true, Message = $"İşlem Başarılı" };
                    }
                    else
                    {
                        Console.WriteLine("Logout Token Kayıt İşlemi Başarısız! " + response.StatusCode);
                        result = new BaseResultDTO() { Result = false, Message = $"İşlem Başarısız" };
                    }

                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                return result = new BaseResultDTO() { Result = false, Message = ex.Message };
            }
        }

        public async Task<BaseResultDTO> LoginFCMTokenRequest(LoginDeviceRequestDTO request)
        {
            BaseResultDTO result;
            try
            {

                using (var httpClient = new HttpClient())
                {

                    string json = JsonConvert.SerializeObject(request);
                    var loginRequest = new StringContent(json, Encoding.UTF8, "application/json");

                    var hostUrl = _configuration.GetValue<string>("MessageService:MessageServiceUrl");
                    var apiKey = _configuration.GetValue<string>("MessageService:Apikey");
                    var secretKey = _configuration.GetValue<string>("MessageService:SecretKey");
                    httpClient.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
                    httpClient.DefaultRequestHeaders.Add("X-Secret-Key", secretKey);
                    httpClient.BaseAddress = new Uri($"{hostUrl}/LoginDeviceRequest");

                    HttpResponseMessage response = await httpClient.PostAsync(httpClient.BaseAddress, loginRequest);

                    if (response.IsSuccessStatusCode)
                    {
                        var res = await response.Content.ReadFromJsonAsync<LoginDeviceResponse>();
                        if (res.Result)
                            result = new BaseResultDTO() { Result = true, Message = $"İşlem Başarılı" };
                        else
                        {
                            result = new BaseResultDTO() { Result = false, Message = res.Error?.ErrorDescription };
                        }

                    }
                    else
                    {
                        Console.WriteLine("Login Token Kayıt İşlemi Başarısız! " + response.StatusCode);
                        var str = await response.Content.ReadAsStringAsync();
                        result = new BaseResultDTO() { Result = false, Message = $"İşlem Başarısız:{response.StatusCode}{str}" };

                    }


                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                return result = new BaseResultDTO() { Result = false, Message = ex.Message };
            }
        }
    }
}
