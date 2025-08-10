using HelperLibrary;
using Microsoft.EntityFrameworkCore;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence
{
    public partial class ModelBuilderExt
    {
        public virtual void BuildModelBefore(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(ConfigHelper.Schema);
        }
        public virtual void BuildModelAfter(ModelBuilder modelBuilder)
        {
            BuildModelExt_AlternateKeys(modelBuilder);
            BuildModelExt_UI_Navigations(modelBuilder);
        }
        public virtual void BuildModelExt_AlternateKeys(ModelBuilder modelBuilder, bool loadYn = false)
        {
            // Multi Null da problem çıkartıyor
            if (!loadYn) return;
        }

        public virtual void BuildModelExt_UI_Navigations(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().HasOne(x => x.Market).WithMany().HasPrincipalKey(e => new { e.Id }).HasForeignKey(e => new { e.MarketId });//.IsRequired(true);
        }
    }
}
