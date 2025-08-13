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
            builder.EntitySet<Location>("Location");
            builder.EntitySet<LocationDate>("LocationDate");
            builder.EntitySet<GeoResult>("GeoResult");
            builder.EntitySet<FindGeo>("FindGeo");
            builder.EntitySet<MesajSablon>("MesajSablon");
            builder.EntitySet<Customer>("Customer");
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
