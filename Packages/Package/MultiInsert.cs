using Business;
using HelperLibrary;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packages
{
    public class MultiInsert:PgBase
    {
        public MultiInsert(SSSessionInfo session) : base(session)
        {
           
        }
        public MultiInsert() : base()
        {
        }
        public BsResult SpMultiInsert(string MarketBrandName, string MarketGeolocation,string ProductDescription,decimal ProductPrice,string ProductHeader)
        {
            using var cmd = new NpgsqlCommand("SP_MULTI_INSERT", Connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            var parameters = GetNewParameterCol(cmd);
            parameters.Add(new NpgsqlParameter("p_market_brand_name", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = MarketBrandName });
            parameters.Add(new NpgsqlParameter("p_market_geolocation", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = MarketGeolocation });
            parameters.Add(new NpgsqlParameter("p_product_description", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = ProductDescription });
            parameters.Add(new NpgsqlParameter("p_product_price", NpgsqlTypes.NpgsqlDbType.Double) { Value = ProductPrice });
            parameters.Add(new NpgsqlParameter("p_product_header", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = ProductHeader });
            try
            {
                cmd.ExecuteNonQuery();
                return GetBsResult(parameters);
            }
            catch (Exception ex)
            {
                return new BsResult { Result = false, Message = ex.Message, Error = new Error(ex) };
            }
        }


    }
}
