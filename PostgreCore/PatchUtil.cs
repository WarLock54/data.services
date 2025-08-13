using Microsoft.Extensions.Options;
using Model;
using SimplePatch;
using SimplePatch.Mapping;
using System.Data.Entity.Infrastructure;
using System.Globalization;

namespace PostgreCore
{
    public class PatchUtil
    {
        public static void InitPatch()
        {
            DeltaConfig.Init(cfg =>
            {
                cfg.AddEntity<Product>();
                cfg.AddEntity<Market>();
                cfg.AddEntity<Location>();
                cfg.AddEntity<LocationDate>();
                cfg.AddEntity<GeoResult>();
                cfg.AddEntity<FindGeo>();
                cfg.AddEntity<MesajSablon>();
                cfg.AddEntity<Customer>();
            });
        }
    }
}
