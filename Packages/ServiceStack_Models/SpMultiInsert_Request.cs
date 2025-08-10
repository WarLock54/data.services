using HelperLibrary;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packages
{
    public class SpMultiInsert_Request:IReturn<BaseResponse>
    {
        public string MarketBrandName { get; set; }
        public string MarketGeolocation { get; set; }
        public string ProductDescription { get; set; }
        public decimal ProductPrice { get; set; }
        public string ProductHeader { get; set; }

    }
}
