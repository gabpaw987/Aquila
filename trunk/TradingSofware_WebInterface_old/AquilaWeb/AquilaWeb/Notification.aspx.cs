using AquilaWeb.App_Code;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AquilaWeb
{
    public partial class Notification : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // all parameters give
            if (Request["symbol"] != null &&
                Request["newDecision"] != null &&
                Request["oldDecision"] != null &&
                Request["transactionPrice"] != null &&
                Request["size"] != null)
            {
                string symbol = Request["symbol"];
                decimal price = Decimal.Parse(Request["transactionPrice"]);
                int size = Int32.Parse(Request["size"]);
                int decision = Int32.Parse(Request["newDecision"]);

                // valid symbol
                if (FinancialSeries.isValidSymbol(symbol))
                {
                    // get all TelNrs requesting notifications for this symbol
                    List<string> nrs = new List<string>();
                    NpgsqlConnector conn = new NpgsqlConnector();
                    List<DbParam> pl = new List<DbParam>() 
                    { 
                        new DbParam("symbol", NpgsqlDbType.Varchar, symbol)
                    };
                    using (NpgsqlDataReader dr = conn.Select("SELECT \"TelNr\" " +
                                                                "FROM \"Users\" NATURAL JOIN pfcontrol " +
                                                                "WHERE pfcontrol.notify = true " +
                                                                "AND pfid IN ( " +
	                                                                "SELECT pfid " +
	                                                                "FROM pfsecurity " +
	                                                                "WHERE symbol=:symbol)", pl))
                                                                
                    {
                        // new data entry
                        while (dr.Read())
                        {
                            // tel nr available
                            if (!dr.IsDBNull(dr.GetOrdinal("TelNr")))
                            {
                                // add nr
                                nrs.Add(dr.GetString(dr.GetOrdinal("TelNr")));
                            }
                        }
                    }

                    // get if auto or manual
                    pl = new List<DbParam>() 
                    { 
                        new DbParam("symbol", NpgsqlDbType.Varchar, Request["symbol"])
                    };
                    string auto = (conn.SelectSingleValue<bool>("SELECT auto FROM pfsecurity WHERE symbol=:symbol", pl)) ? "AUTO" : "MANUAL";

                    // close db connection
                    conn.Connected = false;

                    string msg = String.Empty;
                    if (size > 0)
                    {
                        msg += auto + ": BUY " + symbol + " " + size + "RLs for " + price + ". Signal: " + decision;
                    }
                    else
                    {
                        msg += auto + ": SELL " + symbol + " " + size + "RLs for " + price + ". Signal: " + decision;
                    }

                    foreach (string nr in nrs)
                    {
                        WebRequest request = WebRequest.Create("http://192.168.1.1:9090/sendsms?phone=" + nr + "&text=" + msg + "&password=");
                        WebResponse response = request.GetResponse();
                        System.Diagnostics.Debug.WriteLine("sent to: " + nr + "with address: " + "http://192.168.1.1:9090/sendsms?phone=" + nr + "&text=" + msg + "&password=");
                        Response.Write("sent to: " + nr);
                    }
                }
            } 
        }
    }
}