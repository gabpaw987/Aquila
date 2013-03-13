using AquilaWeb.ServiceReference1;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AquilaWeb.App_Code
{
    public class Portfolio
    {
        public static readonly int PFID=1;

        private int _pfid;
        private NpgsqlConnector _conn;
        private List<PortfolioElement> _assets;
        private decimal _invested;
        private decimal _unrealizedPL;
        private decimal _realizedPL;

        public int Pfid
        {
            get { return this._pfid; }
            set
            {
                // if portfolio doesn't exists
                if (!PortfolioExists(value)) 
                    throw new System.ArgumentException("There is no portfolio with a pfid of "+value);
                this._pfid = (value > 0) ? value : this._pfid;
            }
        }

        public List<PortfolioElement> Assets
        {
            get { return this._assets; }
        }

        public decimal Invested
        {
            get { return this._invested; }
        }

        public decimal UnrealizedPL
        {
            get { return _unrealizedPL; }
        }

        public decimal RealizedPL
        {
            get { return _realizedPL; }
        }

        public Portfolio() : this(Portfolio.PFID)
        {
            
        }

        public Portfolio(int pfid)
        {
            _conn = new NpgsqlConnector("localhost", "postgres", "short", "aquila");
            this.Pfid = pfid;
            _assets = new List<PortfolioElement>();
        }

        public void LoadPortfolio()
        {
            using (NpgsqlDataReader dr = _conn.Select("SELECT * FROM pfsecurity WHERE pfid=" + this._pfid))
            {
                while (dr.Read())
                {
                    PortfolioElement pfe = new PortfolioElement();

                    pfe.Symbol   = (!dr.IsDBNull(dr.GetOrdinal("symbol")))   ? dr.GetString(dr.GetOrdinal("symbol"))     : "";
                    pfe.Position = (!dr.IsDBNull(dr.GetOrdinal("position"))) ? dr.GetInt32(dr.GetOrdinal("position"))    : 0;
                    pfe.Gain     = (!dr.IsDBNull(dr.GetOrdinal("gain")))     ? dr.GetDecimal(dr.GetOrdinal("gain"))      : 0;
                    pfe.Maxinvest= (!dr.IsDBNull(dr.GetOrdinal("maxinvest")))? dr.GetDecimal(dr.GetOrdinal("maxinvest")) : 0m;
                    pfe.Cutloss  = (!dr.IsDBNull(dr.GetOrdinal("cutloss")))  ? dr.GetDecimal(dr.GetOrdinal("cutloss"))   : 0m;
                    pfe.Roi      = (!dr.IsDBNull(dr.GetOrdinal("roi")))      ? dr.GetDecimal(dr.GetOrdinal("roi"))       : 0m;
                    pfe.Auto     = (!dr.IsDBNull(dr.GetOrdinal("auto")))     ? dr.GetBoolean(dr.GetOrdinal("auto"))      : false;
                    pfe.Active   = (!dr.IsDBNull(dr.GetOrdinal("active")))   ? dr.GetBoolean(dr.GetOrdinal("active"))    : false;

                    string query = "SELECT c FROM mbar WHERE symbol=" + pfe.Symbol + " ORDER BY t DESC LIMIT 1";
                    //pfe.Close = _conn.SelectSingleValue<decimal>(query);
                    pfe.Close = 0m;

                    query = "SELECT decision FROM series WHERE symbol=" + pfe.Symbol;
                    //pfe.Decision = _conn.SelectSingleValue<int>(query);
                    pfe.Decision = 0;

                    _assets.Add(pfe);
                }
                dr.Close();
                _conn.Connected = false;
            }
            
            // how much currently is invested
            _invested = _conn.SelectSingleValue<decimal>("SELECT invested FROM portfolio WHERE pfid=" + this._pfid);
            // current unrealized profit
            _unrealizedPL = _conn.SelectSingleValue<decimal>("SELECT urpf FROM portfolio WHERE pfid=" + this._pfid);
            // current realized profit
            _realizedPL = _conn.SelectSingleValue<decimal>("SELECT rpf FROM portfolio WHERE pfid=" + this._pfid);
        }

        public PortfolioElement AddSymbol(string symbol)
        {
            if (FinancialSeries.isValidSymbol(symbol) && !IsInPortfolio(symbol))
            {
                // WCF Communication
                SettingsHandlerClient client = new SettingsHandlerClient();
                // start Thread
                try
                {
                    client.performAction(new object[] { symbol, "Create" });
                }
                catch (Exception ex)
                { 
                    System.Diagnostics.Debug.WriteLine(ex.Message + ": " + ex.StackTrace); 
                }

                // DB Communication
                PortfolioElement e = new PortfolioElement();

                List<DbParam> pl = new List<DbParam>() { new DbParam("symbol", NpgsqlDbType.Varchar, symbol) };

                using (NpgsqlDataReader dr = _conn.Select("INSERT INTO pfsecurity(pfid, symbol) VALUES(" + _pfid + ", :symbol) RETURNING *", pl))
                {
                    dr.Read();

                    e.Symbol    = (!dr.IsDBNull(dr.GetOrdinal("symbol")))   ? dr.GetString(dr.GetOrdinal("symbol"))     : "";
                    e.Position  = (!dr.IsDBNull(dr.GetOrdinal("position"))) ? dr.GetInt32(dr.GetOrdinal("position"))    : 0;
                    e.Gain      = (!dr.IsDBNull(dr.GetOrdinal("gain")))     ? dr.GetDecimal(dr.GetOrdinal("gain"))      : 0;
                    e.Maxinvest = (!dr.IsDBNull(dr.GetOrdinal("maxinvest")))? dr.GetDecimal(dr.GetOrdinal("maxinvest")) : 0m;
                    e.Cutloss   = (!dr.IsDBNull(dr.GetOrdinal("cutloss")))  ? dr.GetDecimal(dr.GetOrdinal("cutloss"))   : 0m;
                    e.Roi       = (!dr.IsDBNull(dr.GetOrdinal("roi")))      ? dr.GetDecimal(dr.GetOrdinal("roi"))       : 0m;
                    e.Auto      = (!dr.IsDBNull(dr.GetOrdinal("auto")))     ? dr.GetBoolean(dr.GetOrdinal("auto"))      : false;
                    e.Active    = (!dr.IsDBNull(dr.GetOrdinal("active")))   ? dr.GetBoolean(dr.GetOrdinal("active"))    : false;

                    dr.Close();
                }

                string query = "SELECT c FROM mbar WHERE symbol='" + e.Symbol + "' ORDER BY t DESC LIMIT 1";
                e.Close = _conn.SelectSingleValue<decimal>(query);

                query = "SELECT decision FROM series WHERE symbol='" + e.Symbol + "'";
                e.Decision = _conn.SelectSingleValue<int>(query);

                return e;
            }
            else
            {
                return null;
            }
        }

        public decimal GetCapital()
        {
            return _conn.SelectSingleValue<decimal>("SELECT capital FROM portfolio WHERE pfid="+_pfid);
        }

        public decimal GetSumMaxInvest()
        {
            return _conn.SelectSingleValue<decimal>("SELECT sum(maxinvest) FROM pfsecurity WHERE pfid="+_pfid);
        }

        public bool IsInPortfolio(string symbol)
        {
            List<DbParam> pl = new List<DbParam>() { new DbParam("symbol", NpgsqlDbType.Varchar, symbol) };
            return _conn.SelectSingleValue<Int64>("SELECT count(*) FROM pfsecurity WHERE symbol=upper( :symbol ) AND pfid="+this._pfid, pl) == 1;
        }

        public bool PortfolioExists(int pfid)
        {
            List<DbParam> pl = new List<DbParam>() 
            { 
                new DbParam("pfid", NpgsqlDbType.Integer, pfid)
            };
            return _conn.SelectSingleValue<Int64>("SELECT count(*) FROM portfolio WHERE pfid=:pfid", pl) == 1;
        }
    }
}