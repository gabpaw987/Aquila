﻿using AquilaWeb.App_Code;
using AquilaWeb.ServiceReference1;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AquilaWeb.MemberPages
{
    public partial class PortfolioPage : System.Web.UI.Page
    {
        private Portfolio pf;

        protected void Page_Load(object sender, EventArgs e)
        {
            // change menu to display current page
            ((Main)Master).initMenu(1);

            // load/calculate portfolio table data
            initPortfolioTable();

            // page newly called
            if (!IsPostBack)
            {
                //
                initPortfolioInfo();
            }
            // postback
            else { 
                
            }
        }

        // WEB METHODS //

        [WebMethod]
        public static PortfolioElement LoadPortfolioElement(string symbol)
        {
            return Portfolio.AddSymbol(symbol, Portfolio.PFID);
        }

        [WebMethod]
        public static List<String> SearchForSymbol(string str)
        {
            string query = "SELECT symbol FROM series WHERE lower(symbol) LIKE :str ORDER BY symbol ASC LIMIT 10";

            NpgsqlConnector conn = new NpgsqlConnector();
            NpgsqlDataReader dr = conn.Select(query, new List<DbParam> { new DbParam("str", NpgsqlDbType.Varchar, "%" + str + "%") });
            List<string> res = new List<string>();
            while (dr != null && dr.Read())
            {
                // System.Diagnostics.Debug.WriteLine("in dr.read");
                res.Add( (!dr.IsDBNull(0)) ? dr.GetString(0) : "" );
            }
            conn.Connected = false;
            return res;
        }

        [WebMethod]
        public static string SetSetting(string symbol, string setting, string value)
        {
            // WCF
            //SettingsHandlerClient client = new SettingsHandlerClient();
            //bool successWCF = client.SetSetting(symbol + "&&" + setting + "&&" + value);
            bool successWCF = true;
            if (successWCF == false) return null;

            // DB
            bool successDB = false;
            string query;
            NpgsqlConnector conn = new NpgsqlConnector();
            conn.CloseAfterQuery = true;

            // Start/Stop trading:
            if (setting.Equals("Active"))
            {
                query = "UPDATE pfsecurity SET active=:value WHERE symbol=:symbol AND pfid=1";
                bool val;
                bool parsed = Boolean.TryParse(value, out val);
                if (parsed)
                {
                    if (conn.ExecuteDMLCommand(query, new List<DbParam> { 
                        new DbParam("value", NpgsqlDbType.Boolean, val),
                        new DbParam("symbol", NpgsqlDbType.Varchar, symbol)
                    }) == 1)
                        successDB = true;
                }
            } 
            // Auto/Manual
            else if (setting.Equals("isManual"))
            {
                query = "UPDATE pfsecurity SET auto=:value WHERE symbol=:symbol AND pfid=1";
                bool val;
                bool parsed = Boolean.TryParse(value, out val);
                if (parsed)
                {
                    if (conn.ExecuteDMLCommand(query, new List<DbParam> { 
                        new DbParam("value", NpgsqlDbType.Boolean, val),
                        new DbParam("symbol", NpgsqlDbType.Varchar, symbol)
                    }) == 1)
                        successDB = true;
                }
            }
            // Change maximum investment
            else if (setting.Equals("Maxinvest"))
            {
                query = "UPDATE pfsecurity SET maxinvest=:value WHERE symbol=:symbol AND pfid=1";
                if (conn.ExecuteDMLCommand(query, new List<DbParam> { 
                        new DbParam("value", NpgsqlDbType.Numeric, value),
                        new DbParam("symbol", NpgsqlDbType.Varchar, symbol)
                    }) == 1)
                        successDB = true;
                decimal val;
                if (Decimal.TryParse(value, out val))
                {
                    value = val.ToString("C2", CurrencyFormatter.getCurrencyFormatter(FinancialSeries.getCurrency(symbol)));
                }
            }

            // close connection
            conn.Connected = false;

            if (successWCF == true && successDB == true)
                return value;
            else
                return null;
        }

        public bool RemovePortfolioElement(string symbol)
        {
            // check if symbol exists
            if (FinancialSeries.isValidSymbol(symbol))
            {
                NpgsqlConnector conn = new NpgsqlConnector();
                // conn.CloseAfterQuery = true;

                string query = "SELECT active FROM pfsecurity WHERE symbol=:symbol";
                List<DbParam> pl = new List<DbParam> { new DbParam("symbol", NpgsqlDbType.Varchar, symbol) };
                bool active = conn.SelectSingleValue<bool>(query, pl);

                if (!active)
                {
                    conn.CloseAfterQuery = true;
                    query = "DELETE FROM pfsecurity WHERE symbol = :symbol";
                    return (conn.ExecuteDMLCommand(query, new List<DbParam> { new DbParam("symbol", NpgsqlDbType.Varchar, symbol) }) > 0) ? true : false;
                }
            }
            return false;
        }

        protected void initPortfolioTable()
        {
            pf = new Portfolio();

            List<TableRow> tRows = new List<TableRow>();
            
            int n = 1;
            foreach (PortfolioElement asset in pf.Assets)
            {
                TableRow tRow = new TableRow();

                // add symbol cell with editing onClick
                TableUtils.addTextCell(tRow, asset.Symbol, null, "content_portfolio_sym_" + n, false, "Instrument.aspx?symbol=" + asset.Symbol);
                TableUtils.addTextCell(tRow, asset.Close.ToString("C3", CurrencyFormatter.getCurrencyFormatter(FinancialSeries.getCurrency(asset.Symbol))), "std_cell");
                TableUtils.addTextCell(tRow, asset.Position + String.Empty, "std_cell");
                TableUtils.addTextCell(tRow, asset.Gain.ToString("C2", CurrencyFormatter.getCurrencyFormatter(FinancialSeries.getCurrency(asset.Symbol))), "std_cell");
                
                TableUtils.addTextCell(tRow, asset.Maxinvest.ToString("C2", CurrencyFormatter.getCurrencyFormatter(FinancialSeries.getCurrency(asset.Symbol))), "std_cell");
                tRow.Cells[tRow.Cells.Count - 1].Attributes.Add("rel", asset.Maxinvest.ToString());
                tRow.Cells[tRow.Cells.Count - 1].Attributes.Add("onclick",
                    "changeValueInput(this, '"+asset.Symbol+"', 'Maxinvest')");
                
                TableUtils.addTextCell(tRow, Math.Round(asset.Cutloss,2) + "%", "std_cell");
                tRow.Cells[tRow.Cells.Count - 1].Attributes.Add("rel", asset.Cutloss.ToString());
                tRow.Cells[tRow.Cells.Count - 1].Attributes.Add("onclick",
                    "changeValueInput(this, '" + asset.Symbol + "', 'cutloss')");

                if (asset.Decision < 0)
                {
                    TableUtils.addTextCell(tRow, "Sell", "std_cell");
                }
                else if (asset.Decision > 0)
                {
                    TableUtils.addTextCell(tRow, "Buy", "std_cell");
                }
                else
                {
                    TableUtils.addTextCell(tRow, "Hold", "std_cell");
                }

                TableUtils.addTextCell(tRow, asset.Roi + String.Empty, "std_cell");

                if (asset.Auto)
                {
                    TableUtils.addTextCell(tRow, "Auto", "button_cell auto");
                }
                else
                {
                    TableUtils.addTextCell(tRow, "Manual", "button_cell manual");
                }
                tRow.Cells[tRow.Cells.Count - 1].Attributes.Add("rel", (!asset.Auto).ToString().ToLower());
                tRow.Cells[tRow.Cells.Count - 1].Attributes.Add("onclick",
                    "toggleSetting(\'" + asset.Symbol + "\', \'isManual\', new Array(true,false), this, new Array('auto', 'manual'))");

                if (asset.Active)
                {
                    TableUtils.addTextCell(tRow, "Trading", "button_cell running");
                }
                else
                {
                    TableUtils.addTextCell(tRow, "Inactive", "button_cell inactive", null);
                }
                tRow.Cells[tRow.Cells.Count - 1].Attributes.Add("rel", (!asset.Active).ToString().ToLower());
                tRow.Cells[tRow.Cells.Count - 1].Attributes.Add("onclick", 
                    "toggleSetting(\'"+asset.Symbol+"\', \'Active\', new Array(true,false), this, new Array('running', 'inactive'))");

                // Add delete-button at end of line
                TableCell btCell = new TableCell();
                Button delBt = new Button();
                delBt.ID = "delBt_" + n;
                delBt.CommandArgument = asset.Symbol;                   // Which symbol should be removed
                delBt.Click += new EventHandler(this.DelBt_Clicked);    // function reference
                delBt.CssClass = "delete";
                delBt.Attributes.Add("rel", asset.Symbol);              // Add symbol for client side script
                delBt.Text = "x";
                btCell.Controls.Add(delBt);
                tRow.Cells.Add(btCell);

                tRows.Add(tRow);

                n++;
            }

            // Add rows to the table
            portfolio_table.Rows.AddRange(tRows.ToArray());

            // Create empty footer row
            TableFooterRow tfRow = new TableFooterRow();
            tfRow.TableSection = TableRowSection.TableFooter;

            TableUtils.addTextCell(tfRow, "+", null, "content_portfolio_sym_n", true);
            tfRow.Cells[tfRow.Cells.Count - 1].Attributes.Add("rel", "+");
            for (int i=0; i<10; i++)
            {
                TableUtils.addTextCell(tfRow, String.Empty);
            }

            // Add empty footer to the table
            portfolio_table.Rows.Add(tfRow);
        }

        protected void DelBt_Clicked(Object sender, EventArgs e)
        {
            Button delBt = (Button)sender;
            if (RemovePortfolioElement(delBt.CommandArgument))
            {
                portfolio_table.Rows.RemoveAt(Int32.Parse(delBt.ID.Substring(delBt.ID.Length - 1, 1)));
            }
        }

        protected void initPortfolioInfo()
        {
            lbl_invested.Text = pf.Invested.ToString("C2",CurrencyFormatter.getCurrencyFormatter("USD"));
            lbl_pl_ur.Text = pf.UnrealizedPL.ToString("C2", CurrencyFormatter.getCurrencyFormatter("USD"));
            lbl_pl_r.Text = pf.RealizedPL.ToString("C2", CurrencyFormatter.getCurrencyFormatter("USD"));
        }
    }
}