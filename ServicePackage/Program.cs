using AutoMapper;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.FileProviders;
using ServicePackage;
using ServiceStack;
using System.Net;
using Business;

var builder = WebApplication.CreateBuilder(args);

var options = new PostgreSqlStorageOptions
{
    PrepareSchemaIfNecessary = true
};
builder.Services.AddHangfire(x => x.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("HangfireConnection"), options));
var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new HelperLibrary.Map());
});

IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IMessageServiceUtil, MessageServiceUtil>();
builder.Services.AddScoped<IMessageHelper, MessageHelper>();

var app = builder.Build();


ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "swagger-ui")),
    RequestPath = "/swagger-ui"
});


app.UseServiceStack(new AppHost());
app.Run();