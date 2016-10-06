using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AquilaWeb.App_Code
{
    public class Instrument
    {
        private NpgsqlConnector _conn;
        private string _symbol;
        private string _currency;
        private string _exchange;

        public string Symbol
        {
            get { return _symbol; }
            set 
            {
                // symbol exists
                if (_conn.SelectSingleValue<Int64>("SELECT count(*) FROM series WHERE symbol=:symbol", new List<DbParam>(){ new DbParam("symbol", NpgsqlDbType.Varchar, value) }) == 1)
                {
                    _currency = _conn.SelectSingleValue<string>("SELECT currency FROM series WHERE symbol=:symbol", new List<DbParam>() { new DbParam("symbol", NpgsqlDbType.Varchar, value) });
                    _exchange = _conn.SelectSingleValue<string>("SELECT ename FROM series WHERE symbol=:symbol", new List<DbParam>() { new DbParam("symbol", NpgsqlDbType.Varchar, value) });
                    _symbol = value;
                }

                _conn.Connected = false;
            }
        }

        public string Currency
        {
            get { return _currency; }
        }

        public string Exchange
        {
            get { return _exchange; }
        }

        public Instrument(string symbol)
        {
            _conn = new NpgsqlConnector();
            Symbol = symbol;
        }

    }
}