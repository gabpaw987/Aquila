using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AquilaWeb.App_Code
{
    public class Portfolio
    {
        public static readonly int PFID=1;

        private NpgsqlConnector conn;
        private List<PortfolioElement> assets;
        private decimal invested;
        private decimal unrealizedPL;
        private decimal realizedPL;

        public List<PortfolioElement> Assets
        {
            get { return this.assets; }
        }

        public decimal Invested
        {
            get { return this.invested; }
        }

        public decimal UnrealizedPL
        {
            get { return unrealizedPL; }
        }

        public decimal RealizedPL
        {
            get { return realizedPL; }
        }

        public Portfolio()
        {
            assets = new List<PortfolioElement>();
            conn = new NpgsqlConnector("localhost", "postgres", "short", "aquila");
            LoadPortfolio();
            conn.Connected = false;
        }

        public void LoadPortfolio()
        {
            using (NpgsqlDataReader dr = conn.Select("SELECT * FROM pfsecurity"))
            {
                while (dr.Read())
                {
                    PortfolioElement pfe = new PortfolioElement();

                    pfe.Symbol   = (!dr.IsDBNull(dr.GetOrdinal("symbol")))   ? dr.GetString(dr.GetOrdinal("symbol"))     : "";
                    pfe.Position = (!dr.IsDBNull(dr.GetOrdinal("position"))) ? dr.GetInt32(dr.GetOrdinal("position"))    : 0;
                    pfe.Position = (!dr.IsDBNull(dr.GetOrdinal("gain")))     ? dr.GetInt32(dr.GetOrdinal("gain"))        : 0;
                    pfe.Maxinvest= (!dr.IsDBNull(dr.GetOrdinal("maxinvest")))? dr.GetDecimal(dr.GetOrdinal("maxinvest")) : 0m;
                    pfe.Cutloss  = (!dr.IsDBNull(dr.GetOrdinal("cutloss")))  ? dr.GetDecimal(dr.GetOrdinal("cutloss"))   : 0m;
                    pfe.Roi      = (!dr.IsDBNull(dr.GetOrdinal("roi")))      ? dr.GetDecimal(dr.GetOrdinal("roi"))       : 0m;
                    pfe.Auto     = (!dr.IsDBNull(dr.GetOrdinal("auto")))     ? dr.GetBoolean(dr.GetOrdinal("auto"))      : false;
                    pfe.Active   = (!dr.IsDBNull(dr.GetOrdinal("active")))   ? dr.GetBoolean(dr.GetOrdinal("active"))    : false;

                    string query = "SELECT c FROM mbar WHERE symbol=" + pfe.Symbol + "ORDER BY t DESC LIMIT 1";
                    pfe.Close = conn.SelectSingleDecimal(query);

                    query = "SELECT decision FROM series WHERE symbol=" + pfe.Symbol;
                    pfe.Decision = Convert.ToInt32(conn.SelectSingleDecimal(query));

                    assets.Add(pfe);
                }
            }
            
            // how much currently is invested
            invested = conn.SelectSingleDecimal("SELECT invested FROM portfolio WHERE pfid=" + PFID);
            // current unrealized profit
            unrealizedPL = conn.SelectSingleDecimal("SELECT urpf FROM portfolio WHERE pfid=" + PFID);
            // current realized profit
            realizedPL = conn.SelectSingleDecimal("SELECT rpf FROM portfolio WHERE pfid=1" + PFID);
        }
    }
}