using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelperLibrary
{
    public class Map : Profile
    {
        public Map()
        {

        }
    }

    public static class MapUtil
    {
        public static IMapper Mapper { get; private set; }
        static MapUtil()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<string, bool>().ConvertUsing(str => str.ToUpper() == "Y" || str.ToUpper() == "E" || str.ToUpper() == "TRUE");
                cfg.CreateMap<int, bool>().ConvertUsing(n => n != 0);
                cfg.CreateMap<byte, bool>().ConvertUsing(n => n != 0);
                cfg.CreateMap<long, bool>().ConvertUsing(n => n != 0);
            });
            Mapper = config.CreateMapper();


        }
    }
}
