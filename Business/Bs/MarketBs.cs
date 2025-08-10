using HelperLibrary;
using Model;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business
{
    public partial class MarketBs: Core<Market,long>
    {
        internal override void RunCustomValidationRules(Market item, DValidationResult vContext)
        {
            if (!item.BrandName.IsNullOrEmpty() && item.BrandName.Length > 10)
                vContext.AddFailure("BrandName","Marka adınız 10 karakteri geçemez");
        }
    }
}
