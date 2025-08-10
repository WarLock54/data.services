using Business;
using Business.IServices;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace PostgreCore.Controllers
{
    public partial class ProductController : DaBDataController<Product, long, ProductBs> { };

}
