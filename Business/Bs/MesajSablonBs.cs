using HelperLibrary;
using Microsoft.EntityFrameworkCore;
using Model;
using Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business
{
    public partial class MesajSablonBs:Core<MesajSablon,long>
    {
        public MesajSablonBs() : base() { }
        public MesajSablonBs(SSSessionInfo pSession, ApplicationDbContext pContext) : base(pSession, pContext) { }
        public static MesajSablonBs GetInstance(SSSessionInfo pSession, ApplicationDbContext pContext)
        {
            return new MesajSablonBs(pSession, pContext);
        }
        public async Task<MesajSablon> GetTemplateAsync(MessageTemplateTypes templateType, string templateCode)
        {
            return await context.MesajSablons.FirstOrDefaultAsync(x => x.SablonTip == (byte)templateType && x.SablonKod == templateCode);
        }
    }
}
