using Funq;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using ServicePackage;
using ServicePackages;
using ServiceStack;
using ServiceStack.Api.OpenApi;
using ServiceStack.Api.OpenApi.Specification;
using ServiceStack.Auth;
using ServiceStack.Text;
using System.Net;

[assembly: HostingStartup(typeof(AppHost))]

namespace ServicePackage;

public class AppHost : AppHostBase, IHostingStartup
{
    private static IConfigurationBuilder builder = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    private static Microsoft.Extensions.Configuration.IConfigurationRoot configuration = builder.Build();
    public void Configure(IWebHostBuilder builder) => builder
        .ConfigureServices(services =>
        {
            // Configure ASP.NET Core IOC Dependencies
            string licenseKey = configuration.GetRequiredSection("servicestack:license").Value;
            /*Microsoft.Extensions.Configuration.Configuration["servicestack:license"];*/

         //   Licensing.RegisterLicense(licenseKey);
            services.AddAuthentication("Bearer")
   .AddJwtBearer("Bearer", options =>
   {
       options.RequireHttpsMetadata = false;
   });
        });
    //servisler buraya eklenmeli
    public AppHost() : base("ServicePackage",
       typeof(MultiInsertServices).Assembly
        )
    { }

    public override void Configure(Container container)
    {
        SetConfig(new HostConfig
        {
            UseSameSiteCookies = true,

        });
        JsConfig<DateTime>.SerializeFn = time => time.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz");
        JsConfig<DateTime?>.SerializeFn = time => time?.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz");

        // DT : This will read from appSettings 
        Plugins.Add(new CorsFeature(AppSettings));
        Plugins.Add(new AuthFeature(() => new AuthUserSession(), new IAuthProvider[] { new NetCoreIdentityAuthProvider(AppSettings) }));

        Plugins.Add(new OpenApiFeature
        {
            AnyRouteVerbs = new List<string> { ServiceStack.HttpMethods.Post },
            UseCamelCaseSchemaPropertyNames = true,
            UseLowercaseUnderscoreSchemaPropertyNames = true,
            DisableAutoDtoInBodyParam = true,
            DisableSwaggerUI = true,
      
        });

        Plugins.Add(new PostmanFeature());
        Plugins.Add(new RequestLogsFeature());

        BaseHostUtil.AddGlobalResponseFilters(this);


        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        container.AddSingleton<IMemoryCache>(memoryCache);
     //   DaBMemoryCache.Configure(memoryCache);
    }
}

