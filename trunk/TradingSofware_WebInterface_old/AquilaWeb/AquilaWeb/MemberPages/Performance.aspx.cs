using AquilaWeb.App_Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AquilaWeb.MemberPages
{
    public partial class Performance : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            // change menu to display current page
            ((Main)Master).initMenu(2);

            // page newly called
            if (!IsPostBack)
            {
                
                InitPortfolioInfo();
            }

            Portfolio pf = new Portfolio();
            ChartUtils.InitChart(Chart1, pf.GetProfitHistory(), "Historical Cumulative Realized Profit");
        }

        protected void InitPortfolioInfo()
        {
            Portfolio pf = new Portfolio();
            pf.LoadPortfolio();

            lbl_invested.Text = pf.Invested.ToString("C2", CurrencyFormatter.getCurrencyFormatter("USD"));
            lbl_pl_ur.Text = pf.UnrealizedPL.ToString("C2", CurrencyFormatter.getCurrencyFormatter("USD"));
            lbl_pl_r.Text = pf.RealizedPL.ToString("C2", CurrencyFormatter.getCurrencyFormatter("USD"));
            lbl_pt.Text = pf.GoodTrades.ToString();
            lbl_upt.Text = pf.BadTrades.ToString();
            lbl_ratio.Text = (pf.BadTrades != 0) ? Math.Round(pf.GoodTrades / pf.BadTrades, 2).ToString() : "0";
        }
    }
}