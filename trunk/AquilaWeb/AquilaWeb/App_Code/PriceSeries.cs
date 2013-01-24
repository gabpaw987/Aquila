using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Npgsql;
using NpgsqlTypes;

namespace AquilaWeb.App_Code
{
    public class PriceSeries
    {
        public static readonly int INTRADAY = 0;
        public static readonly int YEAR = 1;

        private List<Bar> prices;
        private int barType;

        public List<Bar> Prices
        {
            get { return this.prices; }
            set { this.prices = value; }
        }

        public int BarType
        {
            get { return this.barType; }
        }

        public PriceSeries()
        {
            prices = new List<Bar>();
        }

        public decimal MaxPrice()
        {
            decimal max = 0;
            foreach (Bar price in Prices)
            {
                if (price.High > max) max = price.High;
            }
            return max;
        }

        public decimal MinPrice()
        {
            decimal min = (prices.Count != 0) ? prices[0].Low : 0;
            foreach (Bar price in Prices)
            {
                if (price.Low < min) min = price.Low;
            }
            return min;
        }

        public void LoadPrices(string symbol, int period)
        {
            prices = new List<Bar>();
            barType = period;

            string sql = String.Empty;
            switch (period) {
                case 0: 
                    sql = "SELECT t, o, h, l, c " +
                          "FROM mbar " +
                          "WHERE symbol=:symbol " +
                          "AND t::timestamp::date =(" +
	                            "SELECT min(t::timestamp::date) " +
	                            "FROM mbar " +
	                            "WHERE symbol= :symbol2)";
                    break;
                case 1:
                    sql = "SELECT bdate, o, h, l, c FROM dbar WHERE symbol=:symbol AND bdate > current_date - interval '1 year'";
                    break;
            }

            NpgsqlConnection conn = NpgsqlConnector.Connect();
            conn.Open();
            using (NpgsqlCommand command = new NpgsqlCommand(sql, conn))
            {
                command.Parameters.Add(new NpgsqlParameter("symbol", NpgsqlDbType.Varchar));
                command.Parameters[0].Value = symbol;
                if (period == PriceSeries.INTRADAY)
                {
                    command.Parameters.Add(new NpgsqlParameter("symbol2", NpgsqlDbType.Varchar));
                    command.Parameters[1].Value = symbol;
                }

                using (NpgsqlDataReader dr = command.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        DateTime t;
                        if (period == PriceSeries.YEAR)
                        {
                            t = (!dr.IsDBNull(dr.GetOrdinal("bdate"))) ? dr.GetDateTime(dr.GetOrdinal("bdate")) : new DateTime();
                        }
                        else
                        {
                            t = (!dr.IsDBNull(dr.GetOrdinal("t"))) ? dr.GetDateTime(dr.GetOrdinal("t")) : new DateTime();
                        }
                        
                        decimal o = (!dr.IsDBNull(dr.GetOrdinal("o"))) ? dr.GetDecimal(dr.GetOrdinal("o")) : 0m;
                        decimal h = (!dr.IsDBNull(dr.GetOrdinal("h"))) ? dr.GetDecimal(dr.GetOrdinal("h")) : 0m;
                        decimal l = (!dr.IsDBNull(dr.GetOrdinal("l"))) ? dr.GetDecimal(dr.GetOrdinal("l")) : 0m;
                        decimal c = (!dr.IsDBNull(dr.GetOrdinal("c"))) ? dr.GetDecimal(dr.GetOrdinal("c")) : 0m;
                        Bar b = new Bar(t, o, h, l, c);

                        prices.Add(b);
                    }
                }
            }
            conn.Close();
        }
    }
}