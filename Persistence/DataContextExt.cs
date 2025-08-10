using HelperLibrary;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence
{
    public partial class ApplicationDbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            if (!optionsBuilder.IsConfigured)
            {
                var builder = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                     .SetBasePath(Directory.GetCurrentDirectory())
                     .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                Microsoft.Extensions.Configuration.IConfigurationRoot configuration = builder.Build();

                string connectionString = ConfigHelper.ConnectionString;

                optionsBuilder.UseNpgsql(connectionString);

                optionsBuilder.UseNpgsql().EnableSensitiveDataLogging();
            }
        }
        public override int SaveChanges()
        {
            CheckReadonlyEntities();
            return base.SaveChanges(); 
        }

    }
}
