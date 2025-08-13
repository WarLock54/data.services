using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelperLibrary
{
    public class ConfigManager
    {
        private const string APP_CONNECTION_KEY = "ConnectionString";
        public IConfigurationRoot configuration { get; private set; }

        private static ConfigManager _instance;
        public static ConfigManager GetConfig()
        {
            if (_instance == null)
                _instance = new ConfigManager();
            return _instance;
        }
        public ConfigManager()
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            configuration = builder.Build();
        }

        public string GetValue(string key)
        {
            return configuration.GetRequiredSection(key).Value;
        }
        public string GetConnectionString(string connectionStringName)
        {
            return configuration.GetConnectionString(connectionStringName);
        }
    }
}
