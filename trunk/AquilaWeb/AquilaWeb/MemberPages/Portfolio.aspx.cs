using AquilaWeb.App_Code;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
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
            return Portfolio.AddSymbol(symbol);
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

        public bool RemovePortfolioElement(string symbol)
        {
            // check if symbol exists
            if (Portfolio.isValidSymbol(symbol))
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
                TableUtils.addTextCell(tRow, asset.Close + String.Empty, "std_cell");
                TableUtils.addTextCell(tRow, asset.Position + String.Empty, "std_cell");
                TableUtils.addTextCell(tRow, asset.Gain + String.Empty, "std_cell");
                TableUtils.addTextCell(tRow, asset.Maxinvest + String.Empty, "std_cell");
                TableUtils.addTextCell(tRow, asset.Cutloss + String.Empty, "std_cell");

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
                    TableUtils.addTextCell(tRow, "Auto", "std_cell");
                }
                else
                {
                    TableUtils.addTextCell(tRow, "Manual", "std_cell");
                }

                if (asset.Active)
                {
                    TableUtils.addTextCell(tRow, "Trading", "std_cell");
                }
                else
                {
                    TableUtils.addTextCell(tRow, "Inactive", "std_cell");
                }

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

            TableUtils.addTextCell(tfRow, String.Empty, null, "content_portfolio_sym_n", true);
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
            lbl_invested.Text = pf.Invested + "$";
            lbl_pl_ur.Text = pf.UnrealizedPL + "$";
            lbl_pl_r.Text = pf.RealizedPL + "$";
        }
    }
}