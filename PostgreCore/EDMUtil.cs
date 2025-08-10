using Microsoft.OData.Edm;
using Microsoft.OData;
using Model;
using Microsoft.OData.ModelBuilder;

namespace PostgreCore
{
    public class EDMUtil
    {
        
        public  static IEdmModel GetEdmModel()
        {
           ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Market>("Market");
            builder.EntitySet<Product>("Product");
            builder.EntitySet<Market>("Location");
            builder.EntitySet<Product>("LocationDate");
            builder.EntitySet<Market>("GeoResult");
            builder.EntitySet<Product>("FindGeo");
            BuildFunctions(builder);

            var model = builder.GetEdmModel();

            return model;

        }
        private static void BuildFunctions(ODataConventionModelBuilder builder)
        {

            return;
        }
    }
}
