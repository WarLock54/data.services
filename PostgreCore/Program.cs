using AutoMapper;
using Business;
using DataServiceMvc;
using FluentValidation;
using FluentValidation.AspNetCore;
using HelperLibrary;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Persistence;
using PostgreCore;
using PostgreCore.Jwt;
using Serilog;
using ServiceStack;
using StackExchange.Redis;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;
public class Program
{
    public static void Main(string[] args) {

        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog();

        builder.Services.AddControllersWithViews().AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;
            options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Local;
            options.SerializerSettings.ContractResolver = new DefaultContractResolver();
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();

        ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("tr");

        var supportedCultures = new[] { "tr-TR", "en-US" };
        builder.Services.Configure<RequestLocalizationOptions>(options =>
        {

            options.SetDefaultCulture(supportedCultures[0])
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);

            options.RequestCultureProviders = new List<IRequestCultureProvider>
                    {
                        new QueryStringRequestCultureProvider(),
                        new AcceptLanguageHeaderRequestCultureProvider()
                    };
        });
        // redis connection config 
        var redisConfig = builder.Configuration.GetSection("Redis").Get<RedisOptions>();
        var redisOptions = new ConfigurationOptions
        {
            EndPoints = { { redisConfig.Host, redisConfig.Port } },
            User = redisConfig.User,
            Password = redisConfig.Password,
            AbortOnConnectFail = false,
            Ssl = redisConfig.Ssl
        };

        builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisOptions));
        builder.Services.AddScoped<IRedisDbProvider, RedisDbProvider>();
        builder.Services.AddScoped<ICacheHandler, RedisCacheHandler>();
        builder.Services.AddScoped(typeof(IRedisService<>), typeof(RedisService<>));


        builder.Services.AddSwaggerGen(option =>
        {
            option.SwaggerDoc("Authgroup", new OpenApiInfo { Title = "(AUTH)", Version = "v1" });
            option.SwaggerDoc("Productgroup", new OpenApiInfo { Title = "(PRODUCT)", Version = "v1" });
            option.SwaggerDoc("Marketgroup", new OpenApiInfo { Title = "(MARKET)", Version = "v1" });
            option.SwaggerDoc("Locationgroup", new OpenApiInfo { Title = "Location", Version = "v1" });
            option.SwaggerDoc("MesajSablongroup", new OpenApiInfo { Title = "MesajSablon", Version = "v1" });
            option.SwaggerDoc("Customergroup", new OpenApiInfo { Title = "Customer", Version = "v1" });
            option.SwaggerDoc("common", new OpenApiInfo { Title = "(Common)", Version = "v1" });

            // Filter APIs into the correct documents based on custom logic
            option.DocInclusionPredicate((docName, apiDesc) =>
            {
                var controllerName = apiDesc.ActionDescriptor.RouteValues["controller"] + "";
                bool ProductGroup = (controllerName.StartsWith("Product"));
                bool MarketGroup = (controllerName.StartsWith("Market"));
                bool LocationGroup = (controllerName.StartsWith("Location"));
                bool MesajSablonGroup = (controllerName.StartsWith("MesajSablon"));
                bool CustomernGroup = (controllerName.StartsWith("Customer"));
                bool Authgroup = (controllerName.StartsWith("Auth"));
                bool commonGroup = !ProductGroup && !MarketGroup && !LocationGroup && !MesajSablonGroup && !CustomernGroup && !Authgroup;

                if (docName == "Productgroup" && ProductGroup) return true;
                else if (docName == "Marketgroup" && MarketGroup) return true;
                else if (docName == "LocationGroup" && LocationGroup) return true;
                else if (docName == "MesajSablon" && MesajSablonGroup) return true;
                else if (docName == "Customer" && CustomernGroup) return true;
                else if (docName == "common" && commonGroup) return true;
                else if (docName == "Auth" && Authgroup) return true;
                return false;
            });
            option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            option.AddSecurityRequirement(new OpenApiSecurityRequirement
                        {
                            {
                                new OpenApiSecurityScheme
                                {
                                    Reference = new OpenApiReference
                                    {
                                        Type = ReferenceType.SecurityScheme,
                                        Id = "Bearer"
                                    }
                                },
                                new string[] { }
                            }
                        });
        });
        builder.Services.Configure<JsonOptions>(options =>
        {
            // Set this to true to ignore null or default values
        });
        builder.Services.AddControllers(
                    options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                        //options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                    });
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("Test",
                    opt => opt.WithOrigins("https://localhost/44306", "http://localhost/44306")
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                );
        });
        builder.Services.AddSingleton<DailyJwtKeyProvider>();
        builder.Services.AddAuthentication("JwtBearer")
        .AddJwtBearer("JwtBearer", options =>
        {
            options.RequireHttpsMetadata = false;
            // Servis sağlayıcısını oluşturup DailyJwtKeyProvider'ı manuel olarak çözümlüyoruz.
            var sp = builder.Services.BuildServiceProvider();
            var keyProvider = sp.GetRequiredService<DailyJwtKeyProvider>();

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                // Anahtarı artık DailyJwtKeyProvider'dan alıyoruz.
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyProvider.GetDailyKey()))
            };
        });
        builder.Services.AddAuthorization(options =>
        {

        });

        var mapperConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile(new HelperLibrary.Map());
        });
        IMapper mapper = mapperConfig.CreateMapper();
        builder.Services.AddSingleton(mapper);
        builder.Services.AddScoped<IMessageServiceUtil, MessageServiceUtil>();
        builder.Services.AddScoped<IMessageHelper, MessageHelper>();
        builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseNpgsql("Host=localhost;Port=5432;Username=postgres;Password=admin;Database=netCore");
        });

        builder.Services.AddRate Limiter (rateLimiterOptions =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                rateLimiterOptions.AddTokenBucketLimiter("token", options =>
            {
                options.TokenLimit = 1000;
                options.ReplenishmentPeriod = TimeSpan. FromHours(1);
                options.TokensPerPeriod = 700;
                options.AutoReplenishment = true;
                });
            });
        var app = builder.Build();
        app.UseMiddleware<ExceptionMiddleware>();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/Authgroup/swagger.json", "Auth (Auth)");
                c.SwaggerEndpoint("/swagger/common/swagger.json", "GenelServices (Common)");
                c.SwaggerEndpoint("/swagger/Productgroup/swagger.json", "(Product)");
                c.SwaggerEndpoint("/swagger/Marketgroup/swagger.json", "(Market)");
                c.SwaggerEndpoint("/swagger/Locationgroup/swagger.json", "(Location)");
                c.SwaggerEndpoint("/swagger/MesajSablongroup/swagger.json", "(MesajSablon)");
                c.SwaggerEndpoint("/swagger/Customergroup/swagger.json", "(Customer)");
            });
        }


        //add userculture provider for authenticated user
        var requestOpt = new RequestLocalizationOptions();
        requestOpt.SupportedCultures = new List<CultureInfo>
        {
            new CultureInfo("tr-TR")
        };
        requestOpt.SupportedUICultures = new List<CultureInfo>
        {
            new CultureInfo("tr-TR")
        };
        requestOpt.RequestCultureProviders.Clear();
        requestOpt.RequestCultureProviders.Add(new SingleCultureProvider());

        app.UseRequestLocalization(requestOpt);

        //     app.UseSwaggerUI

        app.UseRouting();
        app.UseCors("Test"); // Örnek

        app.UseHttpsRedirection();

        // DOĞRU SIRALAMA BURASI
        app.UseAuthentication();
        app.UseAuthorization();


      


        app.MapControllers();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
        PatchUtil.InitPatch();


        app.Run();
    }

}
    
public class SingleCultureProvider : IRequestCultureProvider
{
    public Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
    {
        return Task.Run(() => new ProviderCultureResult("tr-TR", "tr-TR"));
    }
}
