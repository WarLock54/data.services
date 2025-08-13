using FluentValidation;
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
    public partial class LocationBs:Core<Location,long>
    {
        public LocationBs() : base() { }
        public LocationBs(SSSessionInfo pSession, ApplicationDbContext pContext) : base(pSession, pContext) { }
        public static LocationBs GetInstance(SSSessionInfo pSession, ApplicationDbContext pContext)
        {
            return new LocationBs(pSession, pContext);
        }
    }
}
