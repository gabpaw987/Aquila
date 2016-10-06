using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AquilaWeb.App_Code
{
    public class FinancialSeries
    {
        public static string getCurrency(string symbol)
        {
            List<DbParam> pl = new List<DbParam>() { new DbParam("symbol", NpgsqlDbType.Varchar, symbol) };

            NpgsqlConnector conn = new NpgsqlConnector();
            conn.CloseAfterQuery = true;
            return conn.SelectSingleValue<string>("SELECT currency FROM series WHERE symbol=:symbol", pl);
        }

        public static bool isValidSymbol(string symbol)
        {
            List<DbParam> pl = new List<DbParam>() { new DbParam("symbol", NpgsqlDbType.Varchar, symbol) };

            NpgsqlConnector conn = new NpgsqlConnector();
            conn.CloseAfterQuery = true;
            return conn.SelectSingleValue<Int64>("SELECT count(*) FROM series WHERE symbol=upper( :symbol )", pl) == 1;
        }
    }
}