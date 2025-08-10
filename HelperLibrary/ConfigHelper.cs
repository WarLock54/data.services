using Microsoft.Extensions.Configuration;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelperLibrary
{
    public class ConfigHelper
    {
        private static IConfigurationBuilder builder = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
         .SetBasePath(Directory.GetCurrentDirectory())
         .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        private static Microsoft.Extensions.Configuration.IConfigurationRoot _configuration = builder.Build();

        private static string _connectionString = "";
        public static string ConnectionString
        {
            get
            {
                if (_connectionString.IsNullOrEmpty())
                    _connectionString = _configuration.GetConnectionString("DataBase") ?? string.Empty;
                if (_connectionString.IsNullOrEmpty())
                    _connectionString = "Host=localhost;Database=netCore;Username=postgres;Password=admin";
                return _connectionString;
            }
        }
        public static Microsoft.Extensions.Configuration.IConfigurationRoot Configuration
        {
            get { return _configuration; }
        }
        private static string _schema = "";
        public static string Schema
        {
            get
            {
                if (_schema.IsNullOrEmpty())
                {
                    _schema = _configuration["Schema"] ?? "DataBase";
                }
                return _schema;
            }
        }
    }
}
