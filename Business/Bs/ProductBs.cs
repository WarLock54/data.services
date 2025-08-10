using HelperLibrary;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business
{
    public partial class ProductBs: Core<Product,long>
    {
        internal override void RunCustomValidationRules(Product item, DValidationResult vContext)
        {
            if (item.Price <= 0)
                vContext.AddFailure("Fiyat", "Ücret 0 dan büyük olmalıdır");
        }
    }
}
