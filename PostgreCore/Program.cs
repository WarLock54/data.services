using AutoMapper;
using Business.IRepository;
using Business.IServices;
using Business.Repository;
using Business.Service;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Persistence;
using PostgreCore;
using ServiceStack;
using ServiceStack.Redis;
using StackExchange.Redis;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
builder.Services.AddSingleton<LocationService>();
builder.Services.AddSingleton<IConnectionMultiplexer>(opt =>
{
    var redisUrl = builder.Configuration.GetConnectionString("Redis");
    return ConnectionMultiplexer.Connect(redisUrl);
});
builder.Services.AddSingleton<IRedisClientsManager>(c =>
      new RedisManagerPool("localhost:6379"));

builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("Productgroup", new OpenApiInfo { Title = "(PRODUCT)", Version = "v1" });
    option.SwaggerDoc("Marketgroup", new OpenApiInfo { Title = "(MARKET)", Version = "v1" });
    option.SwaggerDoc("Locationgroup", new OpenApiInfo { Title= "Location",Version = "v1"});
    option.SwaggerDoc("common", new OpenApiInfo { Title = "(Common)", Version = "v1" });

    // Filter APIs into the correct documents based on custom logic
    option.DocInclusionPredicate((docName, apiDesc) =>
    {
        var controllerName = apiDesc.ActionDescriptor.RouteValues["controller"] + "";
        bool ProductGroup = (controllerName.StartsWith("Product"));
        bool MarketGroup = (controllerName.StartsWith("Market"));
        bool LocationGroup = (controllerName.StartsWith("Location"));
        bool commonGroup = !ProductGroup && !MarketGroup && !LocationGroup;

        if (docName == "Productgroup" && ProductGroup) return true;
        else if (docName == "Marketgroup" && MarketGroup) return true;
        else if (docName == "LocationGroup" && LocationGroup) return true;
        else if (docName == "common" && commonGroup) return true;
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
    builder.Services.Configure<JsonOptions>(options =>
    {
        // Set this to true to ignore null or default values
    });
    builder.Services.AddControllers(
        options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
        .AddJsonOptions(options =>
        {
            //options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Test",
              
                opt => opt.WithOrigins()
                       .AllowAnyMethod()
                       .AllowAnyHeader()
            );
    });
    var mapperConfig = new MapperConfiguration(mc =>
    {

    });
    IMapper mapper = mapperConfig.CreateMapper();
    builder.Services.AddSingleton(mapper);
});
builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    options.UseNpgsql("Host=localhost;Port=5432;Username=postgres;Password=admin;Database=netCore");
});


var app = builder.Build();
await using var scope = app.Services.CreateAsyncScope();
var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
var canConnect = await db.Database.CanConnectAsync();
app.Logger.LogInformation("Can connect to database: {CanConnect}", canConnect);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/common/swagger.json", "GenelServices (Common)");
        c.SwaggerEndpoint("/swagger/Productgroup/swagger.json", "(Product)");
        c.SwaggerEndpoint("/swagger/Marketgroup/swagger.json", "(Market)");
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

app.UseRequestLocalization(requestOpt);

//     app.UseSwaggerUI
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

//app.UseHttpLogging();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
PatchUtil.InitPatch();

app.Run();
