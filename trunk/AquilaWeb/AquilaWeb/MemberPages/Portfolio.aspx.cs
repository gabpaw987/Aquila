using AquilaWeb.App_Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AquilaWeb.MemberPages
{
    public partial class Stocks : System.Web.UI.Page
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

        [WebMethod]
        public static PortfolioElement LoadPortfolioElement(string symbol)
        {
            return Portfolio.AddSymbol(symbol);
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
                TableUtils.addTextCell(tRow, asset.Symbol, null, "content_portfolio_sym_" + n, true);
                TableUtils.addTextCell(tRow, asset.Close + String.Empty, "std_cell");
                TableUtils.addTextCell(tRow, asset.Position + String.Empty, "std_cell");
                TableUtils.addTextCell(tRow, asset.Gain + String.Empty, "std_cell");
                TableUtils.addTextCell(tRow, asset.Maxinvest + String.Empty, "std_cell");
                TableUtils.addTextCell(tRow, asset.Cutloss + String.Empty, "std_cell");

                if (asset.Decision < 0)
                {
                    TableUtils.addTextCell(tRow, "Sell", "std_class");
                }
                else if (asset.Decision > 0)
                {
                    TableUtils.addTextCell(tRow, "Buy", "std_class");
                }
                else
                {
                    TableUtils.addTextCell(tRow, "Hold", "std_class");
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
                TableUtils.addDeleteButtonCell(tRow);

                tRows.Add(tRow);

                lbl_invested.Text += n+": " + asset.Symbol;

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
                TableUtils.addTextCell(tfRow, String.Empty, "std_cell");
            }

            // Add empty footer to the table
            portfolio_table.Rows.Add(tfRow);
        }

        protected void initPortfolioInfo()
        {
            lbl_invested.Text = pf.Invested + "$";
            lbl_pl_ur.Text = pf.UnrealizedPL + "$";
            lbl_pl_r.Text = pf.RealizedPL + "$";
        }
    }
}