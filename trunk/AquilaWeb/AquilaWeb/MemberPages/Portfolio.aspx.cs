using AquilaWeb.App_Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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

            //
            initPortfolioInfo();
        }

        protected void initPortfolioTable()
        {
            pf = new Portfolio();

            foreach (PortfolioElement asset in pf.Assets)
            {
                TableRow tRow = new TableRow();

                Stocks.addTextCell(tRow, asset.Symbol, String.Empty);
                Stocks.addTextCell(tRow, asset.Close + String.Empty, "std_cell");
                Stocks.addTextCell(tRow, asset.Position + String.Empty, "std_cell");
                Stocks.addTextCell(tRow, asset.Gain + String.Empty, "std_cell");
                Stocks.addTextCell(tRow, asset.Maxinvest + String.Empty, "std_cell");
                Stocks.addTextCell(tRow, asset.Cutloss + String.Empty, "std_cell");
                if (asset.Decision < 0)
                {
                    Stocks.addTextCell(tRow, "Sell", "std_class");
                }
                else if (asset.Decision > 0)
                {
                    Stocks.addTextCell(tRow, "Buy", "std_class");
                }
                else
                {
                    Stocks.addTextCell(tRow, "Hold", "std_class");
                }
                Stocks.addTextCell(tRow, asset.Roi + String.Empty, "std_cell");
                if (asset.Auto)
                {
                    Stocks.addTextCell(tRow, "Auto", "std_cell");
                }
                else
                {
                    Stocks.addTextCell(tRow, "Manual", "std_cell");
                }
                if (asset.Active)
                {
                    Stocks.addTextCell(tRow, "Trading", "std_cell");
                }
                else
                {
                    Stocks.addTextCell(tRow, "Inactive", "std_cell");
                }

                // Delte button at end of line
                TableCell btCell = new TableCell();
                Button delBt = new Button();
                delBt.Text = "x";
                btCell.Controls.Add(delBt);
                tRow.Cells.Add(btCell);
                
                // add row before footer
                portfolio_table.Rows.AddAt(portfolio_table.Rows.Count - 1, tRow);
            }
        }

        protected static void addTextCell(TableRow tRow, string text, string cssclass)
        {
            TableCell tCell = new TableCell();
            tCell.Text = text;
            tCell.CssClass = cssclass;
            tRow.Cells.Add(tCell);
        }

        protected void initPortfolioInfo()
        {
            lbl_invested.Text = pf.Invested + "$";
            lbl_pl_ur.Text = pf.UnrealizedPL + "$";
            lbl_pl_r.Text = pf.RealizedPL + "$";
        }
    }
}