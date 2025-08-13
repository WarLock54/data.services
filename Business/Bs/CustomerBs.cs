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
    public partial class CustomerBs : Core<Customer, long>
    {
        public CustomerBs() : base() { }
        public CustomerBs(SSSessionInfo pSession, ApplicationDbContext pContext) : base(pSession, pContext) { }
        public static CustomerBs GetInstance(SSSessionInfo pSession, ApplicationDbContext pContext)
        {
            return new CustomerBs(pSession, pContext);
        }
    }
}
