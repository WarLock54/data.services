using Model;
using ServiceStack.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelperLibrary;
using ServicePackage;

namespace ServicePackages
{
    public class MultiInsertServices : ServiceCore
    {
        public object Any(SpMultiInsert_Request request)
        {
            using var servis = new MultiInsert(Session);
            try
            {
                var result = servis.SpMultiInsert(request.MarketBrandName,request.MarketGeolocation,request.ProductDescription , request.ProductPrice , request.ProductHeader);
                //var res = GetBaseResponse<_Response>(result);
                return  GetBaseResponse(result);
            }
            catch (Exception ex)
            {
                return new BaseResponse().SetError(ex);
            }
        }
    }
}
