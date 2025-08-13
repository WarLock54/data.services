using HelperLibrary;
using Model;
using Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business
{
    public partial class ProductBs: Core<Product,long>
    {
        public ProductBs() : base() { }
        public ProductBs(SSSessionInfo pSession, ApplicationDbContext pContext) : base(pSession, pContext) { }
        public static ProductBs GetInstance(SSSessionInfo pSession, ApplicationDbContext pContext)
        {
            return new ProductBs(pSession, pContext);
        }
        internal override void RunCustomValidationRules(Product item, DValidationResult vContext)
        {
            if (item.Price <= 0)
                vContext.AddFailure("Fiyat", "Ücret 0 dan büyük olmalıdır");
        }
    }
}
